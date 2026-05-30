import * as path from 'path';
import * as fs from 'fs';
import {
    ExtensionContext,
    workspace,
    window,
} from 'vscode';
import {
    LanguageClient,
    LanguageClientOptions,
    ServerOptions,
    TransportKind,
} from 'vscode-languageclient/node';

let client: LanguageClient | undefined;

export async function activate(context: ExtensionContext): Promise<void> {
    const serverPath = findServerBinary(context);
    if (!serverPath) {
        void window.showWarningMessage(
            'Atoll MDA: Could not find the LSP server binary. ' +
            'Set "atoll.lsp.serverPath" in settings or install the dotnet tool.',
        );
        return;
    }

    const serverOptions: ServerOptions = {
        command: serverPath,
        transport: TransportKind.stdio,
    };

    const clientOptions: LanguageClientOptions = {
        documentSelector: [{ scheme: 'file', language: 'mda' }],
        synchronize: {
            fileEvents: workspace.createFileSystemWatcher('**/*.mda'),
        },
    };

    client = new LanguageClient(
        'atoll-mda',
        'Atoll MDA',
        serverOptions,
        clientOptions,
    );

    await client.start();
    context.subscriptions.push(client);
}

export async function deactivate(): Promise<void> {
    if (client) {
        await client.stop();
    }
}

/**
 * Resolves the LSP server binary path using the following priority:
 * 1. `atoll.lsp.serverPath` VS Code setting
 * 2. `dotnet tool run atoll-lsp` (local/global tool)
 * 3. `Atoll.Lsp` / `Atoll.Lsp.exe` next to the extension
 */
function findServerBinary(context: ExtensionContext): string | undefined {
    const config = workspace.getConfiguration('atoll.lsp');
    const configuredPath = config.get<string>('serverPath');

    if (configuredPath && configuredPath.trim().length > 0) {
        return configuredPath.trim();
    }

    // Check for the binary next to the extension (for development / self-contained publish)
    const ext = process.platform === 'win32' ? '.exe' : '';
    const bundledPath = path.join(context.extensionPath, 'server', `Atoll.Lsp${ext}`);
    if (fs.existsSync(bundledPath)) {
        return bundledPath;
    }

    // Fall back to dotnet tool invocation
    // This works when the tool is installed: dotnet tool install -g atoll-lsp
    const dotnet = process.platform === 'win32' ? 'dotnet.exe' : 'dotnet';
    return `${dotnet} tool run atoll-lsp`;
}
