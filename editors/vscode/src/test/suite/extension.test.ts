// Checks that the extension running is the installed package, notices and all; then opens a grammar
// with a mistake in it and waits for the server's two answers that need it running: the diagnostic,
// and the semantic tokens. Then the same of a grammar in a C# string: the diagnostic on the C#
// document, the colours painted over it, hover and definition inside it, and the C# document's own
// semantic tokens left to whichever extension colours C# — none, or the C# extension where the run
// installs it (DOTGRAM_SMOKE_CSHARP).

import * as assert from 'assert';
import * as fs from 'fs';
import * as path from 'path';
import * as vscode from 'vscode';

suite('.Gram extension', () => {
    test('the extension is the installed package, with its notices', () => {
        const extension = vscode.extensions.getExtension('dotgram.VSCode');
        const installed = process.env.DOTGRAM_SMOKE_EXTENSIONS;

        assert.ok(extension, 'the extension is installed');
        assert.ok(installed && path.resolve(extension.extensionPath).startsWith(path.resolve(installed)), `${extension.extensionPath} is not the package`);
        const notices = fs.readFileSync(path.join(extension.extensionPath, 'ThirdPartyNotices.txt'), 'utf8');

        // The client's protocol library, Roslyn, and the notices Roslyn ships beside its licence.
        for (const expected of ['vscode-languageclient', 'Microsoft.CodeAnalysis.Common', '--- ThirdPartyNotices.rtf ---']) {
            assert.ok(notices.includes(expected), `the notices name ${expected}`);
        }
    });

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

    test('a grammar in a C# string gets its diagnostics, its colours, hover and definition', async () => {
        const folder = vscode.workspace.workspaceFolders![0].uri.fsPath;
        const uri = vscode.Uri.file(path.join(folder, 'Embedded.cs'));
        const document = await vscode.workspace.openTextDocument(uri);
        const text = document.getText();

        await vscode.window.showTextDocument(document);
        assert.strictEqual(document.languageId, 'csharp');

        const diagnostics = await until(() => {
            const found = vscode.languages.getDiagnostics(uri).filter((item) => item.source === 'dotgram');
            return found.length > 0 ? found : undefined;
        });
        const missing = document.positionAt(text.indexOf('Missing'));

        assert.ok(
            diagnostics.some((item) => String(item.code).startsWith('GRAM') && item.range.start.isEqual(missing)),
            'a GRAM diagnostic at the name the grammar does not define',
        );

        const extension = vscode.extensions.getExtension<{ embeddedColors(uri: string): Record<string, vscode.Range[]> }>('dotgram.VSCode')!;
        const api = await extension.activate();
        const painted = await until(() => {
            const colors = api.embeddedColors(uri.toString());
            return colors.gramSpecialSymbol?.length && colors.keyword?.length ? colors : undefined;
        });
        const bar = document.positionAt(text.indexOf('|'));

        assert.ok(painted.gramSpecialSymbol.some((range) => range.start.isEqual(bar)), 'the | is painted as a symbol of the notation');
        assert.ok(painted.variable.some((range) => range.start.isEqual(document.positionAt(text.indexOf('Word |')))), 'a rule is painted');

        const use = document.positionAt(text.indexOf('Word |') + 1);
        const hovers = await until(async () => {
            const found = await vscode.commands.executeCommand<vscode.Hover[]>('vscode.executeHoverProvider', uri, use);
            return found.some((hover) => markdown(hover).includes('Word')) ? found : undefined;
        });

        assert.ok(hovers.length > 0);

        const definitions = await vscode.commands.executeCommand<(vscode.Location | vscode.LocationLink)[]>('vscode.executeDefinitionProvider', uri, use);
        const definition = document.positionAt(text.indexOf('Word   ='));

        assert.ok(
            definitions.some((item) => ('targetUri' in item ? item.targetRange.start : item.range.start).isEqual(definition)),
            'Go to Definition goes to the rule',
        );

        // The C# document's semantic tokens are not this extension's: none where nothing colours C#, the C#
        // extension's where it is installed.
        if (process.env.DOTGRAM_SMOKE_CSHARP) {
            const legend = await until(
                async () => {
                    const found = await vscode.commands.executeCommand<vscode.SemanticTokensLegend>('vscode.provideDocumentSemanticTokensLegend', uri);
                    const tokens = await vscode.commands.executeCommand<vscode.SemanticTokens>('vscode.provideDocumentSemanticTokens', uri);
                    return found && tokens && tokens.data.length > 0 ? found : undefined;
                },
                300000,
            );

            assert.ok(!legend.tokenTypes.includes('gramPunctuation'), 'the C# extension colours the C# document');
            console.log(`The C# document's semantic tokens: ${legend.tokenTypes.join(', ')}`);
        } else {
            const tokens = await vscode.commands.executeCommand<vscode.SemanticTokens | undefined>('vscode.provideDocumentSemanticTokens', uri);

            assert.strictEqual(tokens, undefined, 'nothing gives a C# document semantic tokens');
        }
    });

    test('dotgram.embedded.enabled turned off takes the grammars in C# away, and on brings them back', async () => {
        const folder = vscode.workspace.workspaceFolders![0].uri.fsPath;
        const uri = vscode.Uri.file(path.join(folder, 'Embedded.cs'));
        const settings = vscode.workspace.getConfiguration('dotgram');
        const ours = () => vscode.languages.getDiagnostics(uri).filter((item) => item.source === 'dotgram');

        await vscode.window.showTextDocument(await vscode.workspace.openTextDocument(uri));
        await until(() => (ours().length > 0 ? true : undefined));

        try {
            await settings.update('embedded.enabled', false, vscode.ConfigurationTarget.Global);
            await until(() => (ours().length === 0 ? true : undefined));
        } finally {
            await settings.update('embedded.enabled', undefined, vscode.ConfigurationTarget.Global);
        }

        await until(() => (ours().length > 0 ? true : undefined));
    });
});

function markdown(hover: vscode.Hover): string {
    return hover.contents.map((content) => (typeof content === 'string' ? content : content.value)).join('\n');
}

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
