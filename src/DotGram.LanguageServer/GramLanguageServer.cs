using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;

using DotGram.Language;

using Microsoft.CodeAnalysis;

namespace DotGram.LanguageServer;

/// <summary>What a host of the server may change; the defaults are the shipped server's.</summary>
sealed class ServerOptions
{
	public IGramScheduler Scheduler { get; init; } = GramScheduler.Instance;

	/// <summary>Where included grammars without a file of their own are opened: read-only copies under the temporary directory.</summary>
	public IIncludedGrammarStore Store { get; init; } = TempDirectoryIncludedGrammarStore.Instance;

	/// <summary>The analysis to run instead of the compiler's; a test's hook.</summary>
	public Func<string, StandaloneGrammarContext?, bool, GramAnalysisClock, StandaloneAnalysis>? Analyze { get; init; }

	/// <summary>Whether the C# of a grammar is completed, described and navigated by Roslyn.</summary>
	public bool CSharp { get; init; } = true;

	/// <summary>Asked for Quick Info in C# where <see cref="CSharp"/> is off; a test's hook.</summary>
	public Func<string, int, CancellationToken, Task<RoslynGramQuickInfo?>>? CSharpQuickInfo { get; init; }
}

/// <summary>
/// The language server: the protocol's requests about <c>.gram</c> documents answered by the shared
/// core's analysis session and providers, each answer mapped to the protocol by a function of its own.
/// </summary>
/// <remarks>Nothing here knows the language; what it knows is which provider answers which request.</remarks>
sealed class GramLanguageServer : IRpcTarget, IGrammarServices
{
	readonly JsonRpcConnection _rpc;
	readonly ServerOptions     _options;
	readonly ServerStore       _store;

	readonly ConcurrentDictionary<int, IReadOnlyList<GramCompletionItem>> _completions = new();

	ImmutableDictionary<string, OpenGrammar> _documents = ImmutableDictionary.Create<string, OpenGrammar>(StringComparer.Ordinal);
	Task<FolderWorkspace>                    _load;
	Task                                     _changes;
	bool                                     _refreshSemanticTokens;
	int                                      _completionList;

	public GramLanguageServer(JsonRpcConnection rpc, ServerOptions options)
	{
		_rpc     = rpc;
		_options = options;
		_store   = new ServerStore(options.Store, this);
		_load    = Task.Run(FolderWorkspace.Empty);
		_changes = _load;
	}

	public static string Version { get; } =
		typeof(GramLanguageServer).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0";

	/// <summary>The workspace once read; the folders are read in the background from <c>initialize</c> on.</summary>
	public Task<FolderWorkspace> Folder => Volatile.Read(ref _load);

	/// <summary>The workspace, where it has been read.</summary>
	FolderWorkspace? Loaded => Folder.IsCompletedSuccessfully ? Folder.Result : null;

	public OpenGrammar? Document(string uri)
	{
		return Volatile.Read(ref _documents).TryGetValue(uri, out var document) ? document : null;
	}

	// ----- Notifications, in order, on the reading thread.

	public void Notify(string method, JsonElement? parameters)
	{
		switch (method)
		{
			case "textDocument/didOpen":
				Open(Read(parameters, LspJson.Default.DidOpenTextDocumentParams));
				break;

			case "textDocument/didChange":
				Change(Read(parameters, LspJson.Default.DidChangeTextDocumentParams));
				break;

			case "textDocument/didClose":
				Close(Read(parameters, LspJson.Default.DidCloseTextDocumentParams));
				break;

			case "workspace/didChangeWatchedFiles":
				WatchedFiles(Read(parameters, LspJson.Default.DidChangeWatchedFilesParams));
				break;

			// Nothing to do: the session reads the document's text, saved or not, and a trace is
			// the client's own.
			case "initialized":
			case "textDocument/didSave":
			case "workspace/didChangeConfiguration":
			case "$/setTrace":
				break;
		}
	}

	void Open(DidOpenTextDocumentParams parameters)
	{
		var item     = parameters.TextDocument;
		var path     = LocalPath(item.Uri);
		var document = new OpenGrammar(item.Uri, path, new GramVersion(item.Version, Microsoft.CodeAnalysis.Text.SourceText.From(item.Text)), this);

		document.Published += Published;
		ImmutableInterlocked.Update(ref _documents, static (documents, pair) => documents.SetItem(pair.Uri, pair.Document), (item.Uri, Document: document));

		// Whether the grammar's host is looked for depends on the folders, which are read by now or soon.
		_ = Folder.ContinueWith(
			folder =>
			{
				if (Document(item.Uri) != document)
					return;

				if (path is not null && folder.Result.Hosts && folder.Result.Contains(path))
					folder.Result.Workspace.WorkspaceChanged += document.WorkspaceChanged;

				document.Start();
			},
			CancellationToken.None,
			TaskContinuationOptions.None,
			TaskScheduler.Default);
	}

	void Change(DidChangeTextDocumentParams parameters)
	{
		if (Document(parameters.TextDocument.Uri) is not { } document)
			return;

		document.Edit(parameters.TextDocument.Version, parameters.ContentChanges);

		if (document.FilePath is { } path)
		{
			var text = document.Current.Text;

			Chain(folder => folder.GrammarEdited(path, text));
		}
	}

	void Close(DidCloseTextDocumentParams parameters)
	{
		var uri = parameters.TextDocument.Uri;

		if (Document(uri) is not { } document)
			return;

		ImmutableInterlocked.TryRemove(ref _documents, uri, out _);
		document.Close();

		if (Loaded is { } loaded)
			loaded.Workspace.WorkspaceChanged -= document.WorkspaceChanged;

		_ = _rpc.NotifyAsync(
			"textDocument/publishDiagnostics",
			new PublishDiagnosticsParams(uri, null, []),
			LspJson.Default.PublishDiagnosticsParams);

		if (document.FilePath is { } path && File.Exists(path))
			Chain(folder => folder.FileChanged(path, deleted: false));
	}

	void WatchedFiles(DidChangeWatchedFilesParams parameters)
	{
		foreach (var change in parameters.Changes)
			if (LocalPath(change.Uri) is { } path)
				Chain(folder => folder.FileChanged(path, change.Type == Lsp.FileDeleted));
	}

	/// <summary>A change to the workspace, made after every change before it, once the folders are read.</summary>
	void Chain(Action<FolderWorkspace> change)
	{
		var folder = Folder;

		_changes = _changes.ContinueWith(
			_ =>
			{
				try
				{
					change(folder.Result);
				}
				catch (Exception exception) when (exception is not OutOfMemoryException)
				{
					Report(exception);
				}
			},
			CancellationToken.None,
			TaskContinuationOptions.None,
			TaskScheduler.Default);
	}

	void Published(OpenGrammar document, GramAnalysisResult<GramVersion> result, bool republished)
	{
		var lines = result.Version.Lines;

		_ = _rpc.NotifyAsync(
			"textDocument/publishDiagnostics",
			new PublishDiagnosticsParams(document.Uri, result.Version.Number, DiagnosticsMap.Map(result.Document.Diagnostics, lines)),
			LspJson.Default.PublishDiagnosticsParams);

		// The client asks for a version's tokens once; an analysis of the same version made again,
		// with the host found meanwhile, is offered to it.
		if (republished && Volatile.Read(ref _refreshSemanticTokens))
			_ = _rpc.RequestAsync("workspace/semanticTokens/refresh");
	}

	// ----- Requests: bound here, to the document's version as it is now, and answered off this thread.

	public Func<CancellationToken, Task<RpcReply>>? Bind(string method, JsonElement? parameters)
	{
		switch (method)
		{
			case "initialize":
				// On this thread, so that the folders are being read before the first document opens.
				var initialized = Initialize(Read(parameters, LspJson.Default.InitializeParams));
				return _ => Task.FromResult(initialized);

			case "textDocument/semanticTokens/full":
				return On(Read(parameters, LspJson.Default.SemanticTokensParams).TextDocument, SemanticTokensAsync);

			case "textDocument/completion":
				var completion = Read(parameters, LspJson.Default.CompletionParams);
				return On(completion.TextDocument, (document, version, token) => CompletionAsync(document, version, completion.Position, token));

			case "completionItem/resolve":
				var item = Read(parameters, LspJson.Default.CompletionItem);
				return _ => Task.FromResult(Resolve(item));

			case "textDocument/hover":
				var hover = Read(parameters, LspJson.Default.TextDocumentPositionParams);
				return On(hover.TextDocument, (document, version, token) => HoverAsync(document, version, hover.Position, token));

			case "textDocument/signatureHelp":
				var signature = Read(parameters, LspJson.Default.SignatureHelpParams);
				return On(signature.TextDocument, (document, version, _) => Task.FromResult(SignatureHelp(document, version, signature.Position)));

			case "textDocument/definition":
				var definition = Read(parameters, LspJson.Default.TextDocumentPositionParams);
				return On(definition.TextDocument, (document, version, token) => DefinitionAsync(document, version, definition.Position, token));

			case "textDocument/references":
				var references = Read(parameters, LspJson.Default.ReferenceParams);
				return On(references.TextDocument, (document, version, token) =>
					ReferencesAsync(document, version, references.Position, references.Context?.IncludeDeclaration ?? true, token));

			case "textDocument/documentHighlight":
				var highlight = Read(parameters, LspJson.Default.TextDocumentPositionParams);
				return On(highlight.TextDocument, (document, version, _) => Task.FromResult(Highlights(document, version, highlight.Position)));

			case "textDocument/prepareRename":
				var prepare = Read(parameters, LspJson.Default.TextDocumentPositionParams);
				return On(prepare.TextDocument, (document, version, token) => PrepareRenameAsync(document, version, prepare.Position, token));

			case "textDocument/rename":
				var rename = Read(parameters, LspJson.Default.RenameParams);
				return On(rename.TextDocument, (document, version, token) => RenameAsync(document, version, rename.Position, rename.NewName, token));

			case "textDocument/foldingRange":
				return On(Read(parameters, LspJson.Default.FoldingRangeParams).TextDocument, FoldingAsync);

			case "textDocument/documentSymbol":
				return On(Read(parameters, LspJson.Default.DocumentSymbolParams).TextDocument, DocumentSymbolsAsync);

			default:
				return null;
		}
	}

	/// <summary>A request about a document, bound to its version as the request was read.</summary>
	Func<CancellationToken, Task<RpcReply>> On(
		TextDocumentIdentifier identifier,
		Func<OpenGrammar, GramVersion, CancellationToken, Task<RpcReply>> answer)
	{
		if (Document(identifier.Uri) is not { } document)
			return static _ => Task.FromResult(RpcReply.Null);

		var version = document.Current;

		return token => answer(document, version, token);
	}

	RpcReply Initialize(InitializeParams parameters)
	{
		Volatile.Write(ref _refreshSemanticTokens, Flag(parameters.Capabilities, "workspace", "semanticTokens", "refreshSupport"));

		var folders = (parameters.WorkspaceFolders?.Select(static folder => folder.Uri) ?? [parameters.RootUri])
			.Select(LocalPath)
			.OfType<string>()
			.ToArray();

		if (folders.Length > 0)
		{
			var load = Task.Run(() => ReadFolders(folders));

			Volatile.Write(ref _load, load);
			_changes = Task.WhenAll(_changes, load);
		}

		var capabilities = new ServerCapabilities(
			"utf-16",
			new TextDocumentSyncOptions(true, Lsp.SyncIncremental),
			new CompletionOptions(["."], true),
			true,
			new SignatureHelpOptions(["(", ","], [")"]),
			true,
			true,
			true,
			new RenameOptions(true),
			true,
			true,
			new SemanticTokensOptions(new SemanticTokensLegend(SemanticTokensMap.TokenTypes, SemanticTokensMap.TokenModifiers), true, false));

		return RpcReply.Ok(new InitializeResult(capabilities, new ServerInfo("DotGram", Version)), LspJson.Default.InitializeResult);
	}

	FolderWorkspace ReadFolders(IReadOnlyList<string> folders)
	{
		try
		{
			return FolderWorkspace.Create(folders, message => _ = _rpc.LogAsync(Lsp.MessageInfo, message), CancellationToken.None);
		}
		catch (Exception exception) when (exception is not OutOfMemoryException)
		{
			Report(exception);
			return FolderWorkspace.Empty();
		}
	}

	async Task<RpcReply> SemanticTokensAsync(OpenGrammar document, GramVersion version, CancellationToken cancellationToken)
	{
		if (await document.AnalysedAsync(version, cancellationToken).ConfigureAwait(false) is not { } analysed)
			return RpcReply.Error(Lsp.ContentModified, "The document changed.");

		return RpcReply.Ok(SemanticTokensMap.Map(analysed.Classifications, version.Lines), LspJson.Default.SemanticTokens);
	}

	async Task<RpcReply> CompletionAsync(OpenGrammar document, GramVersion version, Position position, CancellationToken cancellationToken)
	{
		var text     = version.String;
		var offset   = version.Lines.Offset(position);
		var provider = new StandaloneGramCompletionProvider(document.Session.Document(version));

		if (provider.Start(text, offset) is not { } applicable)
			return RpcReply.Null;

		var roslyn = CSharp(document);
		var items  = await provider.ItemsAsync(
			text, offset, applicable, roslyn is null ? null : roslyn.CompletionAsync, cancellationToken).ConfigureAwait(false);
		var list  = Interlocked.Increment(ref _completionList);

		// The lists resolve asks about are the last few: the one shown, and those a quick typist
		// asked for since.
		_completions[list] = items;
		_completions.TryRemove(list - 8, out _);

		return RpcReply.Ok(CompletionMap.Map(applicable, items, list, version.Lines), LspJson.Default.CompletionList);
	}

	RpcReply Resolve(CompletionItem item)
	{
		if (item.Data is { } data &&
			_completions.TryGetValue(data.List, out var items) &&
			data.Index >= 0 && data.Index < items.Count)
			return RpcReply.Ok(CompletionMap.Resolve(item, items[data.Index]), LspJson.Default.CompletionItem);

		return RpcReply.Ok(item, LspJson.Default.CompletionItem);
	}

	async Task<RpcReply> HoverAsync(OpenGrammar document, GramVersion version, Position position, CancellationToken cancellationToken)
	{
		var offset = version.Lines.Offset(position);
		var answer = await GramQuickInfoProvider.StandaloneAsync(
			document.View(version), offset, CSharpQuickInfo(document), cancellationToken).ConfigureAwait(false);

		return RpcReply.Ok(HoverMap.Map(answer, offset, version.Lines), LspJson.Default.Hover);
	}

	RpcReply SignatureHelp(OpenGrammar document, GramVersion version, Position position)
	{
		var help = new StandaloneGramSignatureHelpProvider(document.Session.Document(version))
			.At(version.String, version.Lines.Offset(position));

		return help is null || help.Signatures.Count == 0
			? RpcReply.Null
			: RpcReply.Ok(SignatureHelpMap.Map(help), LspJson.Default.SignatureHelp);
	}

	async Task<RpcReply> DefinitionAsync(OpenGrammar document, GramVersion version, Position position, CancellationToken cancellationToken)
	{
		var view     = await AnalysedViewAsync(document, version, cancellationToken).ConfigureAwait(false);
		var target   = await GramNavigationProvider.DefinitionAsync(
			view, version.Lines.Offset(position), CSharp(document)?.Core, cancellationToken).ConfigureAwait(false);
		var location = target is null
			? null
			: await NavigationMap.DefinitionAsync(target, document.Uri, view, version.Lines, _store, cancellationToken).ConfigureAwait(false);

		return location is null ? RpcReply.Null : RpcReply.Ok<Location[]>([location], LspJson.Default.LocationArray);
	}

	async Task<RpcReply> ReferencesAsync(
		OpenGrammar document, GramVersion version, Position position, bool includeDeclaration, CancellationToken cancellationToken)
	{
		var view  = await AnalysedViewAsync(document, version, cancellationToken).ConfigureAwait(false);
		var found = GramNavigationProvider.References(view, version.Lines.Offset(position));

		return found is null
			? RpcReply.Null
			: RpcReply.Ok(NavigationMap.References(found, includeDeclaration, document.Uri, version.Lines, _store), LspJson.Default.LocationArray);
	}

	RpcReply Highlights(OpenGrammar document, GramVersion version, Position position)
	{
		var highlights = GramNavigationProvider.Highlights(document.View(version), version.Lines.Offset(position));

		return highlights.Count == 0
			? RpcReply.Null
			: RpcReply.Ok(NavigationMap.Highlights(highlights, version.Lines), LspJson.Default.DocumentHighlightArray);
	}

	async Task<RpcReply> PrepareRenameAsync(OpenGrammar document, GramVersion version, Position position, CancellationToken cancellationToken)
	{
		var view   = await AnalysedViewAsync(document, version, cancellationToken).ConfigureAwait(false);
		var offset = version.Lines.Offset(position);

		if (GramNavigationProvider.References(view, offset) is not { } found)
			return RpcReply.Null;

		if (GramRenameProvider.Refusal(found) is { } refusal)
			return RpcReply.Error(Lsp.RequestFailed, refusal);

		return RpcReply.Ok(RenameMap.Prepare(found, offset, version.Lines), LspJson.Default.PrepareRenameResult);
	}

	async Task<RpcReply> RenameAsync(
		OpenGrammar document, GramVersion version, Position position, string newName, CancellationToken cancellationToken)
	{
		var view = await AnalysedViewAsync(document, version, cancellationToken).ConfigureAwait(false);

		if (GramNavigationProvider.References(view, version.Lines.Offset(position)) is not { } found)
			return RpcReply.Null;

		var rename = GramRenameProvider.Rename(found, newName);

		if (rename.Conflicts.Count > 0)
			return RpcReply.Error(Lsp.RequestFailed, string.Join(" ", rename.Conflicts));

		return RpcReply.Ok(RenameMap.Edit(rename, document.Uri, version.Lines), LspJson.Default.WorkspaceEdit);
	}

	async Task<RpcReply> FoldingAsync(OpenGrammar document, GramVersion version, CancellationToken cancellationToken)
	{
		var view = await AnalysedViewAsync(document, version, cancellationToken).ConfigureAwait(false);

		return RpcReply.Ok(StructureMap.Folding(view.Document.FoldingRanges, version.Lines), LspJson.Default.FoldingRangeArray);
	}

	async Task<RpcReply> DocumentSymbolsAsync(OpenGrammar document, GramVersion version, CancellationToken cancellationToken)
	{
		var view = await AnalysedViewAsync(document, version, cancellationToken).ConfigureAwait(false);

		return RpcReply.Ok(StructureMap.Symbols(view.Document.DocumentSymbols, version.Lines), LspJson.Default.DocumentSymbolArray);
	}

	/// <summary>
	/// The version once analysed, for the questions whose answer is the analysis's own — where a rule
	/// is defined, every use of it, the outline; what was carried over from an older version where a
	/// newer one came first.
	/// </summary>
	static async Task<GramStandaloneView> AnalysedViewAsync(OpenGrammar document, GramVersion version, CancellationToken cancellationToken)
	{
		return await document.AnalysedAsync(version, cancellationToken).ConfigureAwait(false) is { } analysed
			? document.View(version, analysed)
			: document.View(version);
	}

	RoslynFeatures? CSharp(OpenGrammar document)
	{
		if (!_options.CSharp || Loaded is not { } loaded)
			return null;

		return new RoslynFeatures(new RoslynGramCompletion(loaded.Workspace, () => document.FilePath));
	}

	Func<string, int, CancellationToken, Task<RoslynGramQuickInfo?>>? CSharpQuickInfo(OpenGrammar document)
	{
		if (!_options.CSharp)
			return _options.CSharpQuickInfo;

		var roslyn = CSharp(document);

		return roslyn is null ? null : roslyn.QuickInfoAsync;
	}

	// ----- What the documents' sessions ask of the server.

	IGramScheduler IGrammarServices.Scheduler => _options.Scheduler;

	IIncludedGrammarStore IGrammarServices.Store => _store;

	Func<string, StandaloneGrammarContext?, bool, GramAnalysisClock, StandaloneAnalysis>? IGrammarServices.Analyze => _options.Analyze;

	async Task<StandaloneGrammarResolution> IGrammarServices.ResolveAsync(string filePath, CancellationToken cancellationToken)
	{
		var folder = await Folder.ConfigureAwait(false);

		return await StandaloneGrammarInheritance.ResolveAsync(folder.Workspace.CurrentSolution, filePath, cancellationToken).ConfigureAwait(false);
	}

	async Task<bool> IGrammarServices.MayHostAsync(IReadOnlyList<DocumentId> documents, string filePath, CancellationToken cancellationToken)
	{
		var solution = (await Folder.ConfigureAwait(false)).Workspace.CurrentSolution;

		foreach (var id in documents)
			if (solution.GetDocument(id) is { } document &&
				await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false) is { } root &&
				StandaloneGrammarInheritance.MayHost(root, filePath))
				return true;

		return false;
	}

	public void Report(Exception exception)
	{
		_ = _rpc.LogAsync(Lsp.MessageError, exception.ToString());
	}

	// ----- Helpers.

	static T Read<T>(JsonElement? parameters, JsonTypeInfo<T> type)
	{
		if (parameters is not { } value)
			throw new ArgumentException("The request has no parameters.");

		return value.Deserialize(type) ?? throw new ArgumentException("The request's parameters are null.");
	}

	static bool Flag(JsonElement? element, params string[] path)
	{
		if (element is not { } value)
			return false;

		foreach (var name in path)
			if (value.ValueKind != JsonValueKind.Object || !value.TryGetProperty(name, out value))
				return false;

		return value.ValueKind == JsonValueKind.True;
	}

	/// <summary>The local path of a <c>file:</c> URI; null for any other scheme, such as an unsaved document's.</summary>
	public static string? LocalPath(string? uri)
	{
		return uri is not null && Uri.TryCreate(uri, UriKind.Absolute, out var parsed) && parsed.IsFile
			? Path.GetFullPath(parsed.LocalPath)
			: null;
	}

	/// <summary>
	/// The store the sessions open included grammars through, which also says which documents are
	/// read as they stand: its own copies, and every file outside the folders the server looks for
	/// hosts in.
	/// </summary>
	sealed class ServerStore(IIncludedGrammarStore copies, GramLanguageServer server) : IIncludedGrammarStore
	{
		public string? PathToOpen(StandaloneIncludedGrammar grammar)
		{
			return copies.PathToOpen(grammar);
		}

		public bool IsCopy(string filePath)
		{
			if (copies.IsCopy(filePath))
				return true;

			return server.Loaded is not { Hosts: true } folder || !folder.Contains(filePath);
		}
	}
}
