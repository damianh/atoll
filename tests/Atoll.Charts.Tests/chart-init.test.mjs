import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import { afterEach, test } from 'node:test';

const source = await readFile(
  new URL('../../src/Atoll.Charts/Islands/Assets/chart-init.js', import.meta.url),
  'utf8',
);
const vendorUrlExpression = "new URL('./atoll-charts-vendor.min.js', import.meta.url).href";
const executableSource = source.replace(vendorUrlExpression, "'/atoll-charts-vendor.min.js'");
assert.notEqual(executableSource, source, 'vendor URL expression must be replaced for Node');
const { default: init } = await import(
  `data:text/javascript;base64,${Buffer.from(executableSource).toString('base64')}`
);

afterEach(() => {
  delete globalThis.Chart;
  delete globalThis.ResizeObserver;
  delete globalThis.document;
});

async function renderChart(type, options = {}) {
  const container = { clientWidth: 1000, clientHeight: 0 };
  const canvas = {
    style: {},
    getAttribute: () => JSON.stringify({ type, options }),
    closest: () => container,
  };
  const charts = [];

  globalThis.document = { getElementById: () => ({}) };
  globalThis.Chart = class {
    constructor(renderedCanvas) {
      charts.push(renderedCanvas);
    }

    resize() {}
  };
  globalThis.ResizeObserver = class {
    constructor() {}

    observe() {}
  };

  await init({ querySelectorAll: () => [canvas] });
  assert.equal(charts.length, 1);
  return canvas;
}

test('radial charts default to a 2:1 aspect ratio', async () => {
  for (const type of ['pie', 'doughnut', 'radar', 'polarArea']) {
    const canvas = await renderChart(type);

    assert.equal(canvas.width, 1000, type);
    assert.equal(canvas.height, 500, type);
  }
});

test('other charts keep the 2:1 default aspect ratio', async () => {
  const canvas = await renderChart('bar');

  assert.equal(canvas.height, 500);
});

test('configured aspect ratio overrides the default', async () => {
  const canvas = await renderChart('doughnut', { aspectRatio: 1.25 });

  assert.equal(canvas.height, 800);
});
