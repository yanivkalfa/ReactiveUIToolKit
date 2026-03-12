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

function isIdentifierChar(ch: string): boolean {
  return /[A-Za-z0-9_]/.test(ch);
}

function isInsideCodeBlock(document: vscode.TextDocument, position: vscode.Position): boolean {
  const text = document.getText();
  const targetOffset = document.offsetAt(position);

  let inCode = false;
  let awaitingCodeBrace = false;
  let codeBraceDepth = 0;

  let inLineComment = false;
  let inBlockComment = false;
  let inString = false;
  let inChar = false;
  let isVerbatimString = false;

  for (let i = 0; i < text.length && i < targetOffset; i++) {
    const ch = text[i];
    const next = i + 1 < text.length ? text[i + 1] : '';

    if (inLineComment) {
      if (ch === '\n') inLineComment = false;
      continue;
    }

    if (inBlockComment) {
      if (ch === '*' && next === '/') {
        inBlockComment = false;
        i++;
      }
      continue;
    }

    if (inString) {
      if (isVerbatimString) {
        if (ch === '"' && next === '"') {
          i++;
          continue;
        }
        if (ch === '"') {
          inString = false;
          isVerbatimString = false;
        }
      } else {
        if (ch === '\\') {
          i++;
          continue;
        }
        if (ch === '"') {
          inString = false;
        }
      }
      continue;
    }

    if (inChar) {
      if (ch === '\\') {
        i++;
        continue;
      }
      if (ch === '\'') inChar = false;
      continue;
    }

    if (ch === '/' && next === '/') {
      inLineComment = true;
      i++;
      continue;
    }

    if (ch === '/' && next === '*') {
      inBlockComment = true;
      i++;
      continue;
    }

    if (ch === '\'') {
      inChar = true;
      continue;
    }

    if (ch === '"') {
      inString = true;
      isVerbatimString = false;
      continue;
    }

    if ((ch === '@' || ch === '$') && next === '"') {
      inString = true;
      isVerbatimString = ch === '@';
      i++;
      continue;
    }

    if ((ch === '@' || ch === '$') && i + 2 < text.length && (text[i + 1] === '@' || text[i + 1] === '$') && text[i + 2] === '"') {
      inString = true;
      isVerbatimString = ch === '@' || text[i + 1] === '@';
      i += 2;
      continue;
    }

    if (!inCode) {
      if (!awaitingCodeBrace && ch === '@' && i + 5 < text.length && text.substring(i + 1, i + 5) === 'code') {
        const prev = i > 0 ? text[i - 1] : '';
        const after = i + 5 < text.length ? text[i + 5] : '';
        const prevOk = !prev || !isIdentifierChar(prev);
        const afterOk = !after || !isIdentifierChar(after);
        if (prevOk && afterOk) {
          awaitingCodeBrace = true;
          i += 4;
          continue;
        }
      }

      if (awaitingCodeBrace) {
        if (ch === '{') {
          inCode = true;
          codeBraceDepth = 1;
          awaitingCodeBrace = false;
          continue;
        }
      }

      continue;
    }

    if (ch === '{') {
      codeBraceDepth++;
      continue;
    }
    if (ch === '}') {
      codeBraceDepth--;
      if (codeBraceDepth <= 0) {
        inCode = false;
        codeBraceDepth = 0;
      }
      continue;
    }
  }

  return inCode;
}

function looksLikeMarkupSelection(text: string): boolean {
  const lines = text.split(/\r?\n/)
    .map(line => line.trim())
    .filter(line => line.length > 0);

  if (lines.length === 0) return false;

  return lines.every(line =>
    line.startsWith('<') ||
    line.startsWith('</') ||
    line.startsWith('{/*') ||
    line.startsWith('*/}')
  );
}

function firstNonWhitespaceChar(lineText: string): number {
  const idx = lineText.search(/\S/);
  return idx >= 0 ? idx : 0;
}

function trimmedLineEndChar(lineText: string): number {
  return lineText.replace(/\s+$/, '').length;
}

function toMarkupRange(document: vscode.TextDocument, selection: vscode.Selection): vscode.Range {
  let startLine = selection.start.line;
  let endLine = selection.end.line;

  if (!selection.isEmpty && selection.end.character === 0 && endLine > startLine) {
    endLine -= 1;
  }

  const startText = document.lineAt(startLine).text;
  const endText = document.lineAt(endLine).text;
  const startChar = firstNonWhitespaceChar(startText);
  let endChar = trimmedLineEndChar(endText);

  if (endChar < 0) endChar = 0;

  return new vscode.Range(
    new vscode.Position(startLine, startChar),
    new vscode.Position(endLine, endChar)
  );
}

function toggleLineCommentForRanges(editor: vscode.TextEditor, lineRanges: vscode.Range[]): Thenable<boolean> {
  const document = editor.document;
  const lineNumbers = new Set<number>();

  for (const range of lineRanges) {
    const startLine = range.start.line;
    const endLine = range.end.character === 0 && range.end.line > range.start.line
      ? range.end.line - 1
      : range.end.line;
    for (let line = startLine; line <= endLine; line++) lineNumbers.add(line);
  }

  const lines = Array.from(lineNumbers).sort((a, b) => a - b);
  const uncommentAll = lines.length > 0 && lines.every(line => {
    const text = document.lineAt(line).text;
    if (text.trim().length === 0) return true;
    return /^\s*\/\//.test(text);
  });

  return editor.edit(editBuilder => {
    for (let idx = lines.length - 1; idx >= 0; idx--) {
      const line = lines[idx];
      const lineText = document.lineAt(line).text;
      const fullRange = document.lineAt(line).range;

      if (uncommentAll) {
        const updated = lineText.replace(/^(\s*)\/\/ ?/, '$1');
        editBuilder.replace(fullRange, updated);
      } else {
        if (lineText.trim().length === 0) {
          editBuilder.replace(fullRange, lineText);
        } else {
          const indent = lineText.match(/^\s*/)?.[0] ?? '';
          const rest = lineText.slice(indent.length);
          editBuilder.replace(fullRange, `${indent}// ${rest}`);
        }
      }
    }
  });
}

function toggleJsxBlockCommentForRanges(editor: vscode.TextEditor, ranges: vscode.Range[]): Thenable<boolean> {
  const document = editor.document;
  const sortedRanges = [...ranges].sort((a, b) => document.offsetAt(b.start) - document.offsetAt(a.start));

  return editor.edit(editBuilder => {
    for (const range of sortedRanges) {
      const text = document.getText(range);
      const trimmed = text.trim();

      if (trimmed.startsWith('{/*') && trimmed.endsWith('*/}')) {
        const open = text.indexOf('{/*');
        const close = text.lastIndexOf('*/}');
        if (open >= 0 && close >= open) {
          let inner = text.substring(open + 3, close);
          if (inner.startsWith(' ')) inner = inner.substring(1);
          if (inner.endsWith(' ')) inner = inner.substring(0, inner.length - 1);
          editBuilder.replace(range, text.substring(0, open) + inner + text.substring(close + 3));
          continue;
        }
      }

      editBuilder.replace(range, `{/* ${text} */}`);
    }
  });
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

  const toggleBlockCommentCommand = vscode.commands.registerTextEditorCommand(
    'uitkx.toggleBlockComment',
    async (editor) => {
      if (editor.document.languageId !== 'uitkx') {
        await vscode.commands.executeCommand('editor.action.commentLine');
        return;
      }

      const document = editor.document;
      const lineCommentRanges: vscode.Range[] = [];
      const jsxCommentRanges: vscode.Range[] = [];

      for (const selection of editor.selections) {
        const effectiveRange = selection.isEmpty
          ? document.lineAt(selection.start.line).range
          : new vscode.Range(selection.start, selection.end);

        const inCode = isInsideCodeBlock(document, effectiveRange.start);
        const selectedText = document.getText(effectiveRange);
        const isMarkupSelection = looksLikeMarkupSelection(selectedText);

        if (inCode && !isMarkupSelection) {
          lineCommentRanges.push(effectiveRange);
        } else {
          jsxCommentRanges.push(toMarkupRange(document, selection));
        }
      }

      if (jsxCommentRanges.length > 0) {
        await toggleJsxBlockCommentForRanges(editor, jsxCommentRanges);
      }

      if (lineCommentRanges.length > 0) {
        await toggleLineCommentForRanges(editor, lineCommentRanges);
      }
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
