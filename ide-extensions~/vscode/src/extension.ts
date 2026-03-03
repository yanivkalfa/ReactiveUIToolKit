import * as path from 'path';
import * as fs from 'fs';
import * as vscode from 'vscode';
import {
  LanguageClient,
  LanguageClientOptions,
  ServerOptions,
  TransportKind,
} from 'vscode-languageclient/node';

let client: LanguageClient | undefined;

export function activate(context: vscode.ExtensionContext): void {
  console.log('[UITKX] Extension activated');
  const config = vscode.workspace.getConfiguration('uitkx');
  const output = vscode.window.createOutputChannel('UITKX');
  context.subscriptions.push(output);

  const customServerPath = (config.get<string>('server.path', '') || '').trim();
  const bundledDll = path.join(context.extensionPath, 'server', 'UitkxLanguageServer.dll');
  const bundledExe = path.join(context.extensionPath, 'server', 'UitkxLanguageServer.exe');

  let serverPath = bundledDll;
  if (customServerPath.length > 0) {
    if (fs.existsSync(customServerPath)) {
      serverPath = customServerPath;
      output.appendLine(`[UITKX] Using custom server path: ${serverPath}`);
    } else {
      output.appendLine(`[UITKX] Custom server path not found: ${customServerPath}`);
      output.appendLine(`[UITKX] Falling back to bundled server: ${bundledDll}`);
      vscode.window.showWarningMessage(
        `UITKX: uitkx.server.path does not exist (${customServerPath}). Falling back to bundled server.`
      );
    }
  }

  if (!fs.existsSync(serverPath) && fs.existsSync(bundledExe)) {
    serverPath = bundledExe;
    output.appendLine('[UITKX] Falling back to bundled .exe server launcher.');
  }

  if (!fs.existsSync(serverPath)) {
    vscode.window.showWarningMessage(
      `UITKX: Language server not found at "${serverPath}". ` +
      'Completions/hover/formatting will be unavailable. '
    );
    output.appendLine(`[UITKX] Language server not found: ${serverPath}`);
    return;
  }

  const dotnet = config.get<string>('server.dotnetPath', 'dotnet');

  const useExeLauncher = process.platform === 'win32' && serverPath.toLowerCase().endsWith('.exe');
  const serverCommand = useExeLauncher ? serverPath : dotnet;
  const serverArgs = useExeLauncher ? [] : [serverPath];

  output.appendLine(`[UITKX] Server command: ${serverCommand}`);
  if (serverArgs.length > 0) {
    output.appendLine(`[UITKX] Server args: ${serverArgs.join(' ')}`);
  }

  const serverOptions: ServerOptions = {
    run: {
      command: serverCommand,
      args: serverArgs,
      transport: TransportKind.stdio,
    },
    debug: {
      command: serverCommand,
      args: serverArgs,
      transport: TransportKind.stdio,
    },
  };

  const clientOptions: LanguageClientOptions = {
    documentSelector: [{ scheme: 'file', language: 'uitkx' }],
    synchronize: {
      // Notify the server when a .uitkx file is saved
      fileEvents: vscode.workspace.createFileSystemWatcher('**/*.uitkx'),
    },
    traceOutputChannel: vscode.window.createOutputChannel('UITKX LSP Trace'),
  };

  client = new LanguageClient(
    'uitkx',
    'UITKX Language Server',
    serverOptions,
    clientOptions
  );

  const started = client.start();
  Promise.resolve(started).catch((error: unknown) => {
    const message = error instanceof Error ? error.message : String(error);
    output.appendLine(`[UITKX] Failed to start language client: ${message}`);
    vscode.window.showErrorMessage(`UITKX: Failed to start language server. ${message}`);
  });
  context.subscriptions.push(client);
}

export function deactivate(): Thenable<void> | undefined {
  return client?.stop();
}
