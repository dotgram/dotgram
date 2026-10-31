using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotGram.LanguageServer;

// The part of the Language Server Protocol 3.17 this server speaks, written from the specification:
// https://microsoft.github.io/language-server-protocol/specifications/lsp/3.17/specification/
// Names are the protocol's, camel-cased by the serializer; a member left null is not written.

sealed record Position(int Line, int Character);

sealed record Range(Position Start, Position End);

sealed record Location(string Uri, Range Range);

sealed record TextDocumentIdentifier(string Uri);

sealed record VersionedTextDocumentIdentifier(string Uri, int Version);

sealed record TextDocumentItem(string Uri, string LanguageId, int Version, string Text);

sealed record TextDocumentPositionParams(TextDocumentIdentifier TextDocument, Position Position);

sealed record TextEdit(Range Range, string NewText);

sealed record MarkupContent(string Kind, string Value)
{
	public static MarkupContent Markdown(string value)
	{
		return new MarkupContent("markdown", value);
	}

	public static MarkupContent PlainText(string value)
	{
		return new MarkupContent("plaintext", value);
	}
}

// Lifecycle.

sealed record WorkspaceFolder(string Uri, string Name);

sealed record InitializeParams(
	int? ProcessId = null,
	string? RootUri = null,
	string? RootPath = null,
	IReadOnlyList<WorkspaceFolder>? WorkspaceFolders = null,
	JsonElement? Capabilities = null,
	JsonElement? InitializationOptions = null,
	string? Trace = null);

sealed record ServerInfo(string Name, string Version);

sealed record InitializeResult(ServerCapabilities Capabilities, ServerInfo ServerInfo);

sealed record TextDocumentSyncOptions(bool OpenClose, int Change);

sealed record CompletionOptions(IReadOnlyList<string> TriggerCharacters, bool ResolveProvider);

sealed record SignatureHelpOptions(IReadOnlyList<string> TriggerCharacters, IReadOnlyList<string> RetriggerCharacters);

sealed record RenameOptions(bool PrepareProvider);

sealed record SemanticTokensLegend(IReadOnlyList<string> TokenTypes, IReadOnlyList<string> TokenModifiers);

sealed record SemanticTokensOptions(SemanticTokensLegend Legend, bool Full, bool Range);

sealed record ServerCapabilities(
	string PositionEncoding,
	TextDocumentSyncOptions TextDocumentSync,
	CompletionOptions CompletionProvider,
	bool HoverProvider,
	SignatureHelpOptions SignatureHelpProvider,
	bool DefinitionProvider,
	bool ReferencesProvider,
	bool DocumentHighlightProvider,
	RenameOptions RenameProvider,
	bool FoldingRangeProvider,
	bool DocumentSymbolProvider,
	SemanticTokensOptions SemanticTokensProvider);

sealed record SetTraceParams(string Value);

sealed record CancelParams(JsonElement Id);

// Documents.

sealed record DidOpenTextDocumentParams(TextDocumentItem TextDocument);

sealed record TextDocumentContentChangeEvent(string Text, Range? Range = null, int? RangeLength = null);

sealed record DidChangeTextDocumentParams(
	VersionedTextDocumentIdentifier TextDocument,
	IReadOnlyList<TextDocumentContentChangeEvent> ContentChanges);

sealed record DidCloseTextDocumentParams(TextDocumentIdentifier TextDocument);

sealed record FileEvent(string Uri, int Type);

sealed record DidChangeWatchedFilesParams(IReadOnlyList<FileEvent> Changes);

// Diagnostics and messages.

sealed record Diagnostic(Range Range, int Severity, string Code, string Source, string Message);

sealed record PublishDiagnosticsParams(string Uri, int? Version, IReadOnlyList<Diagnostic> Diagnostics);

sealed record LogMessageParams(int Type, string Message);

// Features.

sealed record SemanticTokensParams(TextDocumentIdentifier TextDocument);

sealed record SemanticTokens(IReadOnlyList<int> Data);

sealed record CompletionContext(int TriggerKind, string? TriggerCharacter = null);

sealed record CompletionParams(TextDocumentIdentifier TextDocument, Position Position, CompletionContext? Context = null);

sealed record CompletionItemLabelDetails(string? Detail);

sealed record CompletionItemData(int List, int Index);

sealed record CompletionItem(
	string Label,
	CompletionItemLabelDetails? LabelDetails = null,
	MarkupContent? Documentation = null,
	string? SortText = null,
	string? FilterText = null,
	string? TextEditText = null,
	CompletionItemData? Data = null);

sealed record CompletionItemDefaults(Range EditRange);

sealed record CompletionList(bool IsIncomplete, CompletionItemDefaults ItemDefaults, IReadOnlyList<CompletionItem> Items);

sealed record Hover(MarkupContent Contents, Range Range);

sealed record SignatureHelpContext(int TriggerKind, string? TriggerCharacter = null, bool IsRetrigger = false);

sealed record SignatureHelpParams(TextDocumentIdentifier TextDocument, Position Position, SignatureHelpContext? Context = null);

sealed record ParameterInformation(IReadOnlyList<int> Label, MarkupContent? Documentation);

sealed record SignatureInformation(string Label, MarkupContent? Documentation, IReadOnlyList<ParameterInformation> Parameters);

sealed record SignatureHelp(IReadOnlyList<SignatureInformation> Signatures, int ActiveSignature, int? ActiveParameter);

sealed record ReferenceContext(bool IncludeDeclaration);

sealed record ReferenceParams(TextDocumentIdentifier TextDocument, Position Position, ReferenceContext? Context = null);

sealed record DocumentHighlight(Range Range, int Kind);

sealed record RenameParams(TextDocumentIdentifier TextDocument, Position Position, string NewName);

sealed record PrepareRenameResult(Range Range, string Placeholder);

sealed record WorkspaceEdit(IReadOnlyDictionary<string, IReadOnlyList<TextEdit>> Changes);

sealed record FoldingRangeParams(TextDocumentIdentifier TextDocument);

sealed record FoldingRange(int StartLine, int StartCharacter, int EndLine, int EndCharacter, string? CollapsedText);

sealed record DocumentSymbolParams(TextDocumentIdentifier TextDocument);

sealed record DocumentSymbol(string Name, int Kind, Range Range, Range SelectionRange, IReadOnlyList<DocumentSymbol> Children);

/// <summary>The numbers the protocol gives its enumerations, where this server sends or reads one.</summary>
static class Lsp
{
	public const int SyncIncremental = 2;

	public const int SeverityError       = 1;
	public const int SeverityWarning     = 2;
	public const int SeverityInformation = 3;

	public const int MessageError   = 1;
	public const int MessageWarning = 2;
	public const int MessageInfo    = 3;
	public const int MessageLog     = 4;

	public const int HighlightRead  = 2;
	public const int HighlightWrite = 3;

	public const int SymbolNamespace = 3;
	public const int SymbolMethod    = 6;
	public const int SymbolFunction  = 12;

	public const int FileCreated = 1;
	public const int FileChanged = 2;
	public const int FileDeleted = 3;

	// JSON-RPC's own error codes, and the protocol's.
	public const int ParseError           = -32700;
	public const int InvalidRequest       = -32600;
	public const int MethodNotFound       = -32601;
	public const int InvalidParams        = -32602;
	public const int InternalError        = -32603;
	public const int ServerNotInitialized = -32002;
	public const int RequestFailed        = -32803;
	public const int ServerCancelled      = -32802;
	public const int ContentModified      = -32801;
	public const int RequestCancelled     = -32800;
}

/// <summary>Every type the server reads or writes, serialized by generated code rather than by reflection.</summary>
// What a client must send, it must: a parameter of a record's constructor is required unless it has a
// default, and null only where its type says so, so a request without its text document, its position or
// a rename's new name is refused as invalid rather than answered with nulls.
[JsonSourceGenerationOptions(
	PropertyNamingPolicy                 = JsonKnownNamingPolicy.CamelCase,
	DefaultIgnoreCondition               = JsonIgnoreCondition.WhenWritingNull,
	RespectNullableAnnotations           = true,
	RespectRequiredConstructorParameters = true)]
[JsonSerializable(typeof(InitializeParams))]
[JsonSerializable(typeof(InitializeResult))]
[JsonSerializable(typeof(SetTraceParams))]
[JsonSerializable(typeof(CancelParams))]
[JsonSerializable(typeof(DidOpenTextDocumentParams))]
[JsonSerializable(typeof(DidChangeTextDocumentParams))]
[JsonSerializable(typeof(DidCloseTextDocumentParams))]
[JsonSerializable(typeof(DidChangeWatchedFilesParams))]
[JsonSerializable(typeof(PublishDiagnosticsParams))]
[JsonSerializable(typeof(LogMessageParams))]
[JsonSerializable(typeof(SemanticTokensParams))]
[JsonSerializable(typeof(SemanticTokens))]
[JsonSerializable(typeof(CompletionParams))]
[JsonSerializable(typeof(CompletionList))]
[JsonSerializable(typeof(CompletionItem))]
[JsonSerializable(typeof(TextDocumentPositionParams))]
[JsonSerializable(typeof(Hover))]
[JsonSerializable(typeof(SignatureHelpParams))]
[JsonSerializable(typeof(SignatureHelp))]
[JsonSerializable(typeof(ReferenceParams))]
[JsonSerializable(typeof(Location[]))]
[JsonSerializable(typeof(DocumentHighlight[]))]
[JsonSerializable(typeof(RenameParams))]
[JsonSerializable(typeof(PrepareRenameResult))]
[JsonSerializable(typeof(WorkspaceEdit))]
[JsonSerializable(typeof(FoldingRangeParams))]
[JsonSerializable(typeof(FoldingRange[]))]
[JsonSerializable(typeof(DocumentSymbolParams))]
[JsonSerializable(typeof(DocumentSymbol[]))]
[JsonSerializable(typeof(JsonElement))]
partial class LspJson : JsonSerializerContext
{
}
