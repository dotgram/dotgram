// The grammars written into C# strings: their colours, which the server tells as dotgram/embeddedColors,
// painted as decorations over whatever colours the C# document has. Not semantic tokens: VS Code lets one
// provider of those colour a document, and a C# document's is the C# extension's; a decoration is drawn
// over any provider's tokens, the C# extension's or the built-in TextMate grammar's alike (docs/vscode.md).

import * as vscode from 'vscode';

/** What the server tells of one version of a C# document: five numbers to a span, and where the grammars are. */
export interface EmbeddedColorsParams {
    uri: string;
    version: number;
    kinds: string[];
    data: number[];
    grammars: { start: { line: number; character: number }; end: { line: number; character: number } }[];
}

interface Known {
    version: number;
    spans: Map<string, vscode.Range[]>;
    grammars: vscode.Range[];
}

/**
 * The kinds the server may tell, each painted in a colour of the extension's own (package.json, contributes.colors):
 * the families a .gram file's semantic tokens get in the default themes, which a user may change with
 * workbench.colorCustomizations.
 */
export const kinds = [
    'comment',
    'keyword',
    'variable',
    'number',
    'string',
    'operator',
    'macro',
    'gramInvalid',
    'gramEmbeddedCode',
    'gramCaseInsensitive',
    'gramSpecialSymbol',
    'gramPunctuation',
    'type',
    'function',
    'property',
    'namespace',
    'parameter',
    'label',
];

export class EmbeddedColors implements vscode.Disposable {
    private readonly types = new Map<string, vscode.TextEditorDecorationType>();
    private readonly known = new Map<string, Known>();
    private readonly subscriptions: vscode.Disposable[] = [];

    constructor() {
        for (const kind of kinds) {
            this.types.set(
                kind,
                vscode.window.createTextEditorDecorationType({
                    color: new vscode.ThemeColor(`dotgram.embedded.${kind}`),
                    // Typing at a token's edge does not stretch its colour; the next analysis paints it again.
                    rangeBehavior: vscode.DecorationRangeBehavior.ClosedClosed,
                }),
            );
        }

        this.subscriptions.push(
            vscode.window.onDidChangeVisibleTextEditors((editors) => editors.forEach((editor) => this.paint(editor))),
            vscode.workspace.onDidCloseTextDocument((document) => this.known.delete(document.uri.toString())),
        );
    }

    /** The server told the colours of a version: kept, and painted where that version is shown. */
    told(params: EmbeddedColorsParams): void {
        const uri = vscode.Uri.parse(params.uri).toString();
        const before = this.known.get(uri);

        if (before && before.version > params.version) {
            return;
        }

        const spans = new Map<string, vscode.Range[]>();

        for (let at = 0; at + 4 < params.data.length; at += 5) {
            const kind = params.kinds[params.data[at + 4]];
            const range = new vscode.Range(params.data[at], params.data[at + 1], params.data[at + 2], params.data[at + 3]);
            const list = spans.get(kind);

            if (list) {
                list.push(range);
            } else {
                spans.set(kind, [range]);
            }
        }

        const grammars = params.grammars.map(
            (range) => new vscode.Range(range.start.line, range.start.character, range.end.line, range.end.character),
        );

        this.known.set(uri, { version: params.version, spans, grammars });

        for (const editor of vscode.window.visibleTextEditors) {
            if (editor.document.uri.toString() === uri) {
                this.paint(editor);
            }
        }
    }

    /**
     * Whether a position is in a grammar of the document, as last told: what the extension asks the server
     * about in a C# document, leaving every other position to the C# extension.
     */
    inGrammar(document: vscode.TextDocument, position: vscode.Position): boolean {
        const known = this.known.get(document.uri.toString());

        return !!known && known.grammars.some((range) => range.contains(position));
    }

    /** The spans painted in a document now, by kind: what the smoke test reads, since VS Code shows no one an editor's decorations. */
    painted(uri: string): Record<string, vscode.Range[]> {
        const known = this.known.get(vscode.Uri.parse(uri).toString());
        const result: Record<string, vscode.Range[]> = {};

        known?.spans.forEach((ranges, kind) => (result[kind] = ranges));

        return result;
    }

    /**
     * An editor painted with what is known of its document, where it is the version shown; an older version's
     * colours are left as they are drawn, which VS Code has moved with the edits since.
     */
    private paint(editor: vscode.TextEditor): void {
        const known = this.known.get(editor.document.uri.toString());

        if (!known || known.version !== editor.document.version) {
            return;
        }

        for (const [kind, type] of this.types) {
            editor.setDecorations(type, known.spans.get(kind) ?? []);
        }
    }

    dispose(): void {
        this.subscriptions.forEach((subscription) => subscription.dispose());
        this.types.forEach((type) => type.dispose());
        this.known.clear();
    }
}
