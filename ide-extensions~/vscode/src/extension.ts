import * as path from 'path';
import * as fs from 'fs';
import * as vscode from 'vscode';
import {
  DocumentFormattingRequest,
  LanguageClient,
  LanguageClientOptions,
  ServerOptions,
  TransportKind,
} from 'vscode-languageclient/node';

let client: LanguageClient | undefined;

/**
 * Walk up from each workspace folder looking for uitkx.config.json,
 * return the explicit formatter.indentSize if found.
 */
function findConfigIndentSize(): number | undefined {
  const folders = vscode.workspace.workspaceFolders;
  if (!folders?.length) return undefined;

  for (const folder of folders) {
    let dir = folder.uri.fsPath;
    while (dir) {
      const configPath = path.join(dir, 'uitkx.config.json');
      if (fs.existsSync(configPath)) {
        try {
          const content = fs.readFileSync(configPath, 'utf-8');
          const json = JSON.parse(content);
          const indentSize = json?.formatter?.indentSize;
          if (typeof indentSize === 'number' && indentSize > 0) {
            return indentSize;
          }
        } catch { /* ignore parse errors */ }
        return undefined; // config found but no indentSize
      }
      const parent = path.dirname(dir);
      if (parent === dir) break;
      dir = parent;
    }
  }
  return undefined;
}

export function activate(context: vscode.ExtensionContext): void {
  console.log('[UITKX] Extension activated');
  const config = vscode.workspace.getConfiguration('uitkx');
  const output = vscode.window.createOutputChannel('UITKX');
  context.subscriptions.push(output);

  // --- Persistence diagnostics ---
  output.appendLine(`[UITKX] globalStorageUri: ${context.globalStorageUri.fsPath}`);
  const storedChat = context.globalState.get<unknown>('chatHistory');
  output.appendLine(`[UITKX] globalState.chatHistory on activate: ${JSON.stringify(storedChat ?? null)}`);
  const allKeys = context.globalState.keys();
  output.appendLine(`[UITKX] globalState keys: ${JSON.stringify(allKeys)}`);
  // --------------------------------

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
    middleware: {
      // VSCode inserts completionItem.insertText at the cursor position.
      // For @-triggered items the server returns insertText starting with '@'
      // (e.g. "@if ()\n{\n\t\n}"), which would produce "@@if..." because the
      // user's typed '@' is already in the buffer.
      // Only strip the leading '@' when the user actually typed '@' — otherwise
      // (e.g. Ctrl+Space at line start) the '@' must be kept in insert text.
      provideCompletionItem(document, position, context, token, next) {
        return Promise.resolve(next(document, position, context, token)).then(result => {
          if (!result) return result;

          // Determine whether the character immediately before the cursor is '@'.
          const triggerIsAt = context.triggerCharacter === '@';
          const charBefore = position.character > 0
            ? document.getText(new vscode.Range(position.translate(0, -1), position))
            : '';
          const wordBeforeIsAt = charBefore === '@';

          if (!triggerIsAt && !wordBeforeIsAt) return result;

          const items = Array.isArray(result) ? result : (result as vscode.CompletionList).items;
          for (const item of items) {
            const raw = item.insertText;
            const text = typeof raw === 'string' ? raw : raw?.value;
            if (text && text.startsWith('@')) {
              const stripped = text.slice(1);
              item.insertText = typeof raw === 'string' ? stripped : new vscode.SnippetString(stripped);
            }
          }
          return result;
        });
      },
    },
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

  // OmniSharp's dynamic registration for documentFormattingProvider never fires —
  // the server always advertises DocumentFormattingProvider=null in its initialize
  // response and does not send client/registerCapability for formatting.
  // Register the provider explicitly and route calls through the LSP client.
  const formattingProvider = vscode.languages.registerDocumentFormattingEditProvider(
    [{ language: 'uitkx', scheme: 'file' }],
    {
      async provideDocumentFormattingEdits(
        document: vscode.TextDocument,
        options: vscode.FormattingOptions,
        token: vscode.CancellationToken
      ): Promise<vscode.TextEdit[] | undefined> {
        output.appendLine(`[Formatting] provideDocumentFormattingEdits called for ${document.uri.fsPath}`);
        if (!client) {
          output.appendLine('[Formatting] client is null/undefined — skipping');
          return undefined;
        }
        output.appendLine(`[Formatting] client state: ${client.state}`);
        try {
          const params = {
            textDocument: client.code2ProtocolConverter.asTextDocumentIdentifier(document),
            options: client.code2ProtocolConverter.asFormattingOptions(options, {}),
          };
          output.appendLine(`[Formatting] sending DocumentFormattingRequest`);
          const lspEdits = await client.sendRequest(
            DocumentFormattingRequest.type,
            params,
            token
          );
          output.appendLine(`[Formatting] got response: ${JSON.stringify(lspEdits)?.slice(0, 200)}`);
          if (!lspEdits) return undefined;
          return client.protocol2CodeConverter.asTextEdits(lspEdits);
        } catch (err) {
          output.appendLine(`[Formatting] ERROR: ${err}`);
          return undefined;
        }
      },
    }
  );
  context.subscriptions.push(formattingProvider);
  output.appendLine('[UITKX] DocumentFormattingEditProvider registered for uitkx');

  // ── Sync editor.tabSize with uitkx.config.json indentSize ──────────
  function syncTabSize(): void {
    const indentSize = findConfigIndentSize();
    if (indentSize == null) return;

    const editorCfg = vscode.workspace.getConfiguration('editor', { languageId: 'uitkx' });
    const current = editorCfg.get<number>('tabSize');
    if (current === indentSize) return;

    editorCfg.update('tabSize', indentSize, vscode.ConfigurationTarget.Workspace, true)
      .then(
        () => output.appendLine(`[UITKX] Synced editor.tabSize → ${indentSize} from uitkx.config.json`),
        (err: unknown) => output.appendLine(`[UITKX] Failed to sync tabSize: ${err}`)
      );
  }

  syncTabSize();

  const configWatcher = vscode.workspace.createFileSystemWatcher('**/uitkx.config.json');
  configWatcher.onDidChange(syncTabSize);
  configWatcher.onDidCreate(syncTabSize);
  context.subscriptions.push(configWatcher);
  // ────────────────────────────────────────────────────────────────────

  context.subscriptions.push(client);
  output.appendLine('[UITKX] activate() completed');
}

export function deactivate(): Thenable<void> | undefined {
  return client?.stop();
}
