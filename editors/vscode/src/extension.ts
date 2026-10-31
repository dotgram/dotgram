// The .Gram extension: starts the DotGram language server and hands it every .gram document. What
// the language means is the server's; this file only finds a runtime and the server and connects them.

import * as os from 'os';
import * as path from 'path';
import * as vscode from 'vscode';
import { LanguageClient, LanguageClientOptions, ServerOptions, TransportKind } from 'vscode-languageclient/node';

const extensionId = 'dotgram.dotgram';
const runtimeVersion = '10.0';

let client: LanguageClient | undefined;

export async function activate(context: vscode.ExtensionContext): Promise<void> {
    const output = vscode.window.createOutputChannel('.Gram');
    context.subscriptions.push(output);

    const server = await serverCommand(context, output);
    if (!server) {
        return;
    }

    const serverOptions: ServerOptions = {
        run: { ...server, transport: TransportKind.stdio },
        debug: { ...server, transport: TransportKind.stdio },
    };

    const clientOptions: LanguageClientOptions = {
        documentSelector: [{ language: 'gram' }],
        outputChannel: output,
        synchronize: {
            // The server looks for a grammar's host in the folder's C#, and for what it includes in
            // the folder's grammars.
            fileEvents: vscode.workspace.createFileSystemWatcher('**/*.{cs,gram}'),
        },
    };

    client = new LanguageClient('dotgram', '.Gram language server', serverOptions, clientOptions);
    context.subscriptions.push(client);

    // An included grammar that has no file of its own is opened as a read-only copy, as Visual
    // Studio opens it: editing it would change nothing that is compiled.
    context.subscriptions.push(vscode.window.onDidChangeActiveTextEditor(readOnlyCopies));
    readOnlyCopies(vscode.window.activeTextEditor);

    await client.start();
}

export async function deactivate(): Promise<void> {
    if (client) {
        await client.stop();
        client = undefined;
    }
}

/** How to start the server: a DLL with the dotnet that runs it, or an executable of the user's. */
async function serverCommand(
    context: vscode.ExtensionContext,
    output: vscode.OutputChannel,
): Promise<{ command: string; args: string[] } | undefined> {
    const configuration = vscode.workspace.getConfiguration('dotgram');
    const configured = configuration.get<string>('server.path')?.trim();
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
