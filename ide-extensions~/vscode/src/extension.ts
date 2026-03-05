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
      // Strip the leading '@' so VSCode inserts cleanly after the typed '@'.
      provideCompletionItem(document, position, context, token, next) {
        return Promise.resolve(next(document, position, context, token)).then(result => {
          if (!result) return result;
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

  const toggleBlockCommentCommand = vscode.commands.registerTextEditorCommand(
    'uitkx.toggleBlockComment',
    async (editor) => {
      if (editor.document.languageId !== 'uitkx') {
        await vscode.commands.executeCommand('editor.action.commentLine');
        return;
      }

      await vscode.commands.executeCommand('editor.action.blockComment');
    }
  );
  context.subscriptions.push(toggleBlockCommentCommand);

  const jsxCommentHandler = vscode.workspace.onDidChangeTextDocument(event => {
    if (event.document.languageId !== 'uitkx') return;
    const editor = vscode.window.activeTextEditor;
    if (!editor || editor.document !== event.document) return;
    if (event.contentChanges.length !== 1) return;
    const change = event.contentChanges[0];
    if (change.text !== '*') return;
    const afterCol = change.range.start.character + 1;
    const lineText = event.document.lineAt(change.range.start.line).text;
    if (afterCol < 3 || lineText.substring(afterCol - 3, afterCol) !== '{/*') return;
    const nextChar = afterCol < lineText.length ? lineText[afterCol] : '';
    const insertPos = new vscode.Position(change.range.start.line, afterCol);
    if (nextChar === '}') {
      // Already have a closing brace — insert ' */' before it
      editor.edit(eb => {
        eb.insert(insertPos, ' */');
      }, { undoStopBefore: false, undoStopAfter: false }).then(ok => {
        if (ok) editor.selection = new vscode.Selection(insertPos, insertPos);
      });
    } else {
      editor.edit(eb => {
        eb.insert(insertPos, ' */}');
      }, { undoStopBefore: false, undoStopAfter: false }).then(ok => {
        if (ok) editor.selection = new vscode.Selection(insertPos, insertPos);
      });
    }
  });
  context.subscriptions.push(jsxCommentHandler);
  context.subscriptions.push(client);
}

export function deactivate(): Thenable<void> | undefined {
  return client?.stop();
}
