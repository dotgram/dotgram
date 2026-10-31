// Opens a grammar with a mistake in it and waits for the server's two answers that need it running:
// the diagnostic, and the semantic tokens.

import * as assert from 'assert';
import * as path from 'path';
import * as vscode from 'vscode';

suite('.Gram extension', () => {
    test('a grammar gets its diagnostics and its colours', async () => {
        const folder = vscode.workspace.workspaceFolders![0].uri.fsPath;
        const uri = vscode.Uri.file(path.join(folder, 'Smoke.gram'));
        const document = await vscode.workspace.openTextDocument(uri);

        await vscode.window.showTextDocument(document);
        assert.strictEqual(document.languageId, 'gram');

        const diagnostics = await until(() => {
            const found = vscode.languages.getDiagnostics(uri).filter((item) => item.source === 'dotgram');
            return found.length > 0 ? found : undefined;
        });

        assert.ok(diagnostics.some((item) => String(item.code).startsWith('GRAM')), 'a GRAM diagnostic');

        const legend = await until(() =>
            vscode.commands.executeCommand<vscode.SemanticTokensLegend>('vscode.provideDocumentSemanticTokensLegend', uri),
        );
        const tokens = await until(async () => {
            const result = await vscode.commands.executeCommand<vscode.SemanticTokens>('vscode.provideDocumentSemanticTokens', uri);
            return result && result.data.length > 0 ? result : undefined;
        });

        assert.ok(legend.tokenTypes.includes('gramPunctuation'));
        assert.ok(tokens.data.length >= 5 * 5, 'some tokens');
    });
});

async function until<T>(probe: () => T | undefined | Thenable<T | undefined>, timeout = 120000): Promise<T> {
    const deadline = Date.now() + timeout;

    while (Date.now() < deadline) {
        const value = await probe();
        if (value !== undefined && value !== null) {
            return value;
        }
        await new Promise((resolve) => setTimeout(resolve, 250));
    }

    throw new Error('Timed out.');
}
