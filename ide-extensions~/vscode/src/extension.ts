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

  // Locate the LSP server DLL
  const customServerPath = config.get<string>('server.path', '');
  const serverDll = customServerPath
    ? customServerPath
    : path.join(context.extensionPath, 'server', 'UitkxLanguageServer.dll');

  if (!fs.existsSync(serverDll)) {
    vscode.window.showWarningMessage(
      `UITKX: Language server not found at "${serverDll}". ` +
      'Completions and hover info will be unavailable. ' +
      'Set uitkx.server.path to point to UitkxLanguageServer.dll.'
    );
    return;
  }

  const dotnet = config.get<string>('server.dotnetPath', 'dotnet');

  const serverOptions: ServerOptions = {
    run: {
      command: dotnet,
      args: [serverDll],
      transport: TransportKind.stdio,
    },
    debug: {
      command: dotnet,
      args: [serverDll],
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

  client.start();
  context.subscriptions.push(client);
}

export function deactivate(): Thenable<void> | undefined {
  return client?.stop();
}
