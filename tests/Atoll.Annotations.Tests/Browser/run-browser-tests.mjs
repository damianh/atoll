import { createServer } from 'node:http';
import { spawn } from 'node:child_process';
import { mkdtemp, readFile, rm } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { fileURLToPath } from 'node:url';

const here = fileURLToPath(new URL('.', import.meta.url));
const asset = fileURLToPath(new URL('../../../src/Atoll.Annotations/Islands/Assets/annotations-init.js', import.meta.url));
const cases = [
    'annotationCopiesEncodedLink', 'linkOnlyProvidesManualFallback',
    'unavailableClipboardShowsManualLink', 'dismissedSelectionIgnoresLateClipboardFailure',
    'bothIslandsPreserveFeedback', 'annotationFirstDoesNotDuplicateControls',
    'longAndMultilineSelectionsUseRanges', 'outsideSelectionHidesActions',
    'copiedLinkForNavigation',
    'copiedMultilineLinkForNavigation',
];
let complete;
const finished = new Promise(resolve => { complete = resolve; });
let completeNavigation;
const server = createServer(async (req, res) => {
    const path = new URL(req.url, 'http://localhost').pathname;
    try {
        if ((path === '/result' || path === '/navigation-result') && req.method === 'POST') {
            let body = '';
            for await (const chunk of req) body += chunk;
            if (path === '/result') complete(JSON.parse(body));
            else completeNavigation(JSON.parse(body));
            res.end('ok');
        } else if (path === '/annotations-init.js' || path === '/passage-links.browser.js') {
            res.setHeader('content-type', 'text/javascript');
            res.end(await readFile(path === '/annotations-init.js' ? asset : join(here, 'passage-links.browser.js')));
        } else if (path === '/destination' || path === '/destination-multiline') {
            res.setHeader('content-type', 'text/html');
            const content = path === '/destination'
                ? '<p id="target">A distinctive destination passage</p>'
                : '<p id="target">A distinctive opening passage</p><p>A distinctive ending passage</p>';
            res.end(`<!doctype html><div style="height:3000px"></div>${content}<div style="height:1000px"></div>
                <script>setTimeout(() => {
                    const target = document.querySelector('#target').getBoundingClientRect();
                    fetch('/navigation-result', { method: 'POST', body: JSON.stringify({ top: target.top, bottom: target.bottom, height: innerHeight }) });
                }, 1000)</script>`);
        } else if (path === '/case') {
            res.setHeader('content-type', 'text/html');
            res.end('<!doctype html><title>Passage test</title><article></article><p id="outside">Outside</p><script type="module" src="/passage-links.browser.js"></script>');
        } else {
            res.setHeader('content-type', 'text/html');
            res.end(`<!doctype html><script>
                const cases = ${JSON.stringify(cases)};
                const results = [];
                window.addEventListener('message', async event => {
                    if (event.origin !== location.origin) return;
                    results.push(event.data);
                    if (results.length === cases.length) {
                        await fetch('/result', { method: 'POST', body: JSON.stringify(results) });
                    } else {
                        document.querySelector('iframe').src = '/case?case=' + cases[results.length];
                    }
                });
            </script><iframe src="/case?case=${cases[0]}"></iframe>`);
        }
    } catch (error) {
        res.writeHead(500);
        res.end(String(error));
    }
});

await new Promise(resolve => server.listen(0, '127.0.0.1', resolve));
const profile = await mkdtemp(join(tmpdir(), 'atoll-passage-browser-'));
const browser = spawn('firefox', ['--headless', '--no-remote', '--profile', profile,
    `http://127.0.0.1:${server.address().port}/`], { stdio: 'ignore' });
let timer;
let navigationBrowser;
let navigationProfile;
try {
    const results = await Promise.race([
        finished,
        new Promise((_, reject) => {
            timer = setTimeout(() => reject(new Error('Browser tests timed out')), 30000);
            browser.once('error', reject);
            browser.once('exit', code => reject(new Error(`Firefox exited before completing tests (${code})`)));
        }),
    ]);
    clearTimeout(timer);
    for (const result of results) {
        console.log(`${result.ok ? 'PASS' : 'FAIL'} ${result.name}${result.error ? `\n${result.error}` : ''}`);
    }
    if (results.length !== cases.length || results.some(result => !result.ok)) process.exitCode = 1;
    for (const name of ['copiedLinkForNavigation', 'copiedMultilineLinkForNavigation']) {
        const navigationUrl = results.find(result => result.name === name && result.ok)?.url;
        if (!navigationUrl) continue;
        navigationProfile = await mkdtemp(join(tmpdir(), 'atoll-passage-navigation-'));
        const navigationFinished = new Promise(resolve => { completeNavigation = resolve; });
        navigationBrowser = spawn('firefox', ['--headless', '--no-remote', '--profile', navigationProfile,
            navigationUrl], { stdio: 'ignore' });
        const navigation = await Promise.race([
            navigationFinished,
            new Promise((_, reject) => {
                timer = setTimeout(() => reject(new Error('Navigation test timed out')), 30000);
                navigationBrowser.once('error', reject);
                navigationBrowser.once('exit', code => reject(new Error(`Navigation browser exited early (${code})`)));
            }),
        ]);
        clearTimeout(timer);
        if (navigation.top < navigation.height && navigation.bottom > 0) {
            console.log(`PASS ${name}Navigates`);
        } else {
            console.error(`FAIL ${name}Navigates: target top ${navigation.top}, viewport ${navigation.height}`);
            process.exitCode = 1;
        }
        navigationBrowser.kill();
        await new Promise(resolve => navigationBrowser.exitCode !== null ? resolve() : navigationBrowser.once('exit', resolve));
        navigationBrowser = null;
        await rm(navigationProfile, { recursive: true, force: true });
        navigationProfile = null;
    }
} finally {
    clearTimeout(timer);
    browser.kill();
    await new Promise(resolve => browser.exitCode !== null ? resolve() : browser.once('exit', resolve));
    if (navigationBrowser) {
        navigationBrowser.kill();
        await new Promise(resolve => navigationBrowser.exitCode !== null ? resolve() : navigationBrowser.once('exit', resolve));
    }
    await new Promise(resolve => server.close(resolve));
    await rm(profile, { recursive: true, force: true });
    if (navigationProfile) await rm(navigationProfile, { recursive: true, force: true });
}
