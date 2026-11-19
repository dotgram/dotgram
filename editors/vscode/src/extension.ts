// The .Gram extension: starts the DotGram language server and hands it every .gram document, and every
// C# document for the grammars written into its strings. What the language means is the server's; this
// file only finds a runtime and the server, connects them, and paints what the server says of C#.

import * as fs from 'fs';
import * as os from 'os';
import * as path from 'path';
import * as vscode from 'vscode';
import { LanguageClient, LanguageClientOptions, Middleware, ServerOptions, TransportKind } from 'vscode-languageclient/node';
import { EmbeddedColors, EmbeddedColorsParams } from './embedded';

const extensionId = 'dotgram.GramForVSCode';
const runtimeVersion = '10.0';

let client: LanguageClient | undefined;

/** What the extension gives other extensions: the colours it paints, which the smoke test reads. */
export interface DotGramApi {
    embeddedColors(uri: string): Record<string, vscode.Range[]>;
}

export async function activate(context: vscode.ExtensionContext): Promise<DotGramApi | undefined> {
    const output = vscode.window.createOutputChannel('.Gram');
    context.subscriptions.push(output);

    const server = await serverCommand(context, output);
    if (!server) {
        return undefined;
    }

    const serverOptions: ServerOptions = {
        run: { ...server, transport: TransportKind.stdio },
        debug: { ...server, transport: TransportKind.stdio },
    };

    // The server looks for a grammar's host in the folder's C#, for what it includes in the folder's
    // grammars, and for what a project holds in its project files and the files they import.
    const watcher = vscode.workspace.createFileSystemWatcher('**/*.{cs,gram,csproj,props,targets}');
    context.subscriptions.push(watcher);

    const embedded = new EmbeddedColors();
    context.subscriptions.push(embedded);

    // Made again at every start: which documents the server is given depends on the settings.
    const create = () => {
        const settings = vscode.workspace.getConfiguration('dotgram');

        // A C# document is the server's for its grammars only: it publishes their diagnostics on it, tells
        // their colours, and answers inside them; the rest of the document is the C# extension's.
        const documentSelector = settings.get<boolean>('embedded.enabled', true)
            ? [{ language: 'gram' }, { language: 'csharp', scheme: 'file' }]
            : [{ language: 'gram' }];

        const clientOptions: LanguageClientOptions = {
            documentSelector,
            middleware: middleware(embedded),
            outputChannel: output,
            synchronize: {
                fileEvents: watcher,
            },
            // MSBuild runs a project's own code as it evaluates it, so the server evaluates the projects only
            // in a trusted workspace; in another it reads them by their folders. Whether the C# extension
            // runs says whether the build's own diagnostics are shown already (dotgram.embedded.diagnostics).
            initializationOptions: () => ({
                trusted: vscode.workspace.isTrusted,
                csharpExtension: vscode.extensions.getExtension('ms-dotnettools.csharp') !== undefined,
                embeddedDiagnostics: vscode.workspace.getConfiguration('dotgram').get<string>('embedded.diagnostics', 'auto'),
            }),
        };

        const created = new LanguageClient('dotgram', '.Gram language server', serverOptions, clientOptions);

        created.onNotification('dotgram/embeddedColors', (params: EmbeddedColorsParams) => embedded.told(params));

        return created;
    };

    // Started again with what the server was told forgotten: a new server's openings of the documents
    // are new ones, numbered from the start. One restart at a time, each after the one before has
    // started its client: a client still starting cannot be stopped.
    let restarting = Promise.resolve();

    const restartOnce = async () => {
        try {
            embedded.reset();

            if (client) {
                await client.dispose();
            }

            client = create();
            await client.start();
        } catch (error) {
            output.appendLine(`The language server could not be started again: ${error}`);
        }
    };

    const restart = () => {
        restarting = restarting.then(restartOnce, restartOnce);
        return restarting;
    };

    client = create();
    context.subscriptions.push({ dispose: () => void client?.dispose() });

    // An included grammar that has no file of its own is opened as a read-only copy, as Visual
    // Studio opens it: editing it would change nothing that is compiled.
    context.subscriptions.push(vscode.window.onDidChangeActiveTextEditor(readOnlyCopies));
    readOnlyCopies(vscode.window.activeTextEditor);

    // Trust granted: started again, so that the projects are evaluated; a setting of the grammars in C#
    // changed: started again with it.
    context.subscriptions.push(
        vscode.workspace.onDidGrantWorkspaceTrust(restart),
        vscode.workspace.onDidChangeConfiguration(async (change) => {
            if (change.affectsConfiguration('dotgram.embedded')) {
                await restart();
            }
        }),
    );

    // The first start is the first link of the chain: a restart waits for it.
    restarting = client.start();
    await restarting;

    return { embeddedColors: (uri) => embedded.painted(uri) };
}

/**
 * What the server is asked about a C# document: a position in one of its grammars, and for Go to Definition
 * also the name of a .gram file in a line that gives one; every other position is the C# extension's alone,
 * and what is no grammar's — completion, signature help, folding, the outline — is not asked at all.
 */
function middleware(embedded: EmbeddedColors): Middleware {
    const gram = (document: vscode.TextDocument) => document.languageId !== 'csharp';
    const inGrammar = (document: vscode.TextDocument, position: vscode.Position) =>
        gram(document) || embedded.inGrammar(document, position);

    return {
        provideCompletionItem: (document, position, context, token, next) =>
            gram(document) ? next(document, position, context, token) : undefined,
        provideSignatureHelp: (document, position, context, token, next) =>
            gram(document) ? next(document, position, context, token) : undefined,
        provideFoldingRanges: (document, context, token, next) => (gram(document) ? next(document, context, token) : undefined),
        provideDocumentSymbols: (document, token, next) => (gram(document) ? next(document, token) : undefined),
        provideHover: (document, position, token, next) =>
            inGrammar(document, position) ? next(document, position, token) : undefined,
        provideDefinition: (document, position, token, next) =>
            inGrammar(document, position) || /\.gram\s*"/i.test(document.lineAt(position.line).text)
                ? next(document, position, token)
                : undefined,
        provideReferences: (document, position, options, token, next) =>
            inGrammar(document, position) ? next(document, position, options, token) : undefined,
        provideDocumentHighlights: (document, position, token, next) =>
            inGrammar(document, position) ? next(document, position, token) : undefined,
        prepareRename: (document, position, token, next) =>
            inGrammar(document, position) ? next(document, position, token) : undefined,
        provideRenameEdits: (document, position, newName, token, next) =>
            inGrammar(document, position) ? next(document, position, newName, token) : undefined,
    };
}

export async function deactivate(): Promise<void> {
    if (client) {
        await client.stop();
        client = undefined;
    }
}

/**
 * How to start the server: an executable of the user's, the extension's own self-contained one where it
 * carries one, or a DLL with the dotnet that runs it.
 */
async function serverCommand(
    context: vscode.ExtensionContext,
    output: vscode.OutputChannel,
): Promise<{ command: string; args: string[] } | undefined> {
    const configuration = vscode.workspace.getConfiguration('dotgram');
    const configured = configuration.get<string>('server.path')?.trim();
    const bundled = context.asAbsolutePath(
        path.join('server', process.platform === 'win32' ? 'DotGram.LanguageServer.exe' : 'DotGram.LanguageServer'),
    );

    // A self-contained build carries its runtime and an executable; a framework-dependent one only the DLL.
    if (!configured && fs.existsSync(bundled)) {
        if (process.platform !== 'win32') {
            fs.chmodSync(bundled, 0o755);
        }

        return { command: bundled, args: [] };
    }

    const server = configured || context.asAbsolutePath(path.join('server', 'DotGram.LanguageServer.dll'));

    if (!server.toLowerCase().endsWith('.dll')) {
        return { command: server, args: [] };
    }

    const dotnet = configuration.get<string>('dotnetPath')?.trim() || (await acquireRuntime(output));
    if (!dotnet) {
        return undefined;
    }

    return { command: dotnet, args: [server] };
}

/** The .NET runtime the server is built for, from the .NET Install Tool: found where it is installed, downloaded where not. */
async function acquireRuntime(output: vscode.OutputChannel): Promise<string | undefined> {
    try {
        const result = await vscode.commands.executeCommand<{ dotnetPath: string } | undefined>('dotnet.acquire', {
            version: runtimeVersion,
            requestingExtensionId: extensionId,
        });

        if (result?.dotnetPath) {
            return result.dotnetPath;
        }
    } catch (error) {
        output.appendLine(`The .NET ${runtimeVersion} runtime could not be acquired: ${error}`);
    }

    void vscode.window.showErrorMessage(
        `.Gram: the .NET ${runtimeVersion} runtime the language server needs could not be acquired. ` +
            'Set "dotgram.dotnetPath" to a dotnet that has it.',
    );

    return undefined;
}

const copies = path.join(os.tmpdir(), 'DotGram', 'Included');

function readOnlyCopies(editor: vscode.TextEditor | undefined): void {
    const document = editor?.document;

    if (document?.uri.scheme === 'file' && document.languageId === 'gram' && isInside(document.uri.fsPath, copies)) {
        void vscode.commands.executeCommand('workbench.action.files.setActiveEditorReadonlyInSession');
    }
}

function isInside(file: string, folder: string): boolean {
    const relative = path.relative(folder, file);

    return relative.length > 0 && !relative.startsWith('..') && !path.isAbsolute(relative);
}
