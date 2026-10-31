using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

using Span = Microsoft.VisualStudio.Text.Span;
using Hyper = System.Windows.Documents.Span;
using System.Windows.Threading;

using DotGram.Language;
using DotGram.VisualStudio;

using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

using Moq;

using Xunit;

// The recording runs on a thread of its own with nothing else on it: waiting there deadlocks nothing.
#pragma warning disable VSTHRD002

namespace DotGram.VisualStudio.Tests;

/// <summary>
/// Records what the extension's own feature code answers on the playground grammars — completion,
/// Quick Info, signature help, definition, references, highlights and rename — through the classes
/// Visual Studio calls, so that the answers can be held unchanged when that code moves.
/// </summary>
/// <remarks>
/// Written to <c>%TEMP%\dotgram-playground-recording</c>, one file per grammar. The C# around a grammar
/// is left to Roslyn and is not recorded: the workspace here has no project, so every question asked of
/// Roslyn answers nothing, and what is recorded is DotGram's own part.
/// </remarks>
public sealed class PlaygroundRecording
{
	static readonly string[] Grammars = ["VisualStudioToolingPlayground.gram", "ToolingQuery.gram", "RecoveryPlayground.gram"];

	[Fact]
	public void Records_the_playground_answers()
	{
		ExceptionDispatchInfo? failure = null;
		var thread = new Thread(() =>
		{
			try
			{
				Run();
			}
			catch (Exception exception)
			{
				failure = ExceptionDispatchInfo.Capture(exception);
			}
		});

		thread.SetApartmentState(ApartmentState.STA);
		thread.Start();
		thread.Join();
		failure?.Throw();
	}

	static void Run()
	{
		SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher));

		if (Application.Current is null)
			_ = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };

		// Visual Studio's main thread, which ThreadHelper otherwise asks Visual Studio for: this one.
		var context = typeof(ThreadHelper).GetField("_joinableTaskContextCache", BindingFlags.Static | BindingFlags.NonPublic)
			?? throw new InvalidOperationException("ThreadHelper's fields: " + string.Join(", ",
				typeof(ThreadHelper).GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public).Select(static field => field.Name)));
#pragma warning disable VSSDK005 // Outside Visual Studio there is no singleton to take; this one stands in for it.
		context.SetValue(null, new Microsoft.VisualStudio.Threading.JoinableTaskContext());
#pragma warning restore VSSDK005

		var output = Path.Combine(Path.GetTempPath(), "dotgram-playground-recording");
		Directory.CreateDirectory(output);

		var root    = Path.Combine(SourceRootDirectory(), "tests", "DotGram.VisualStudio.Tests", "Playground");
		var markers = File.ReadAllLines(Path.Combine(SourceRootDirectory(), "tests", "DotGram.Language.Tests", "Playground", "CompletionMarkers.txt"));

		foreach (var grammar in Grammars)
		{
			var text   = File.ReadAllText(Path.Combine(root, grammar));
			string record;
			try
			{
			record = Record(text, markers.Where(line => line.StartsWith(grammar + "|", StringComparison.Ordinal))
				.Select(line => line.Substring(grammar.Length + 1)).ToArray());
			}
			catch (Exception exception)
			{
				throw new InvalidOperationException(grammar + " at " + Stage, exception);
			}

			File.WriteAllText(Path.Combine(output, Path.GetFileNameWithoutExtension(grammar) + ".txt"), record, new UTF8Encoding(false));
		}

		// The grammars embedded in C#: the playground's attributes and its StringSyntax(".gram") literal.
		const string embedded = "VisualStudioToolingPlayground.cs";
		string embeddedRecord;
		try
		{
			embeddedRecord = RecordEmbedded(
				File.ReadAllText(Path.Combine(root, embedded)),
				root,
				markers.Where(line => line.StartsWith(embedded + "|", StringComparison.Ordinal)).Select(line => line.Substring(embedded.Length + 1)).ToArray());
		}
		catch (Exception exception)
		{
			throw new InvalidOperationException(embedded + " at " + Stage, exception);
		}

		File.WriteAllText(Path.Combine(output, embedded + ".txt"), embeddedRecord, new UTF8Encoding(false));
	}

	/// <summary>
	/// What a C# document's embedded grammars are analysed into, as the extension's buffer analysis
	/// computes it once Roslyn has a semantic model: the same functions, over a workspace that holds the
	/// playground's C# beside its .gram files. The DotGram attributes are declared as source, as the
	/// generator would emit them; the generator itself does not run, so the DSL literals, which need
	/// its publications, are not recognized.
	/// </summary>
	internal static (Microsoft.CodeAnalysis.Document Document, IReadOnlyList<EmbeddedGrammarAnalysis> Analyses, DslEmbeddedSiteResult Dsl, IReadOnlyList<HostDiagnostic> DslDiagnostics) EmbeddedState(string text, string root)
	{
		var workspace = new Microsoft.CodeAnalysis.AdhocWorkspace();
		var project   = workspace.AddProject(Microsoft.CodeAnalysis.ProjectInfo.Create(
			Microsoft.CodeAnalysis.ProjectId.CreateNewId(),
			Microsoft.CodeAnalysis.VersionStamp.Default,
			"Playground",
			"Playground",
			Microsoft.CodeAnalysis.LanguageNames.CSharp,
			parseOptions: Microsoft.CodeAnalysis.CSharp.CSharpParseOptions.Default.WithLanguageVersion(Microsoft.CodeAnalysis.CSharp.LanguageVersion.Preview),
			metadataReferences: [Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(typeof(object).Assembly.Location)]));
		var document  = project.AddDocument("VisualStudioToolingPlayground.cs", Microsoft.CodeAnalysis.Text.SourceText.From(text), filePath: @"P:\Playground\VisualStudioToolingPlayground.cs");
		project = document.Project
			.AddDocument("DotGram.Attributes.g.cs", Microsoft.CodeAnalysis.Text.SourceText.From(DotGram.Grammar.GramCompiler.EmitMarkerAttributes().Text)).Project
			.AddDocument("Declarations.cs", EmbeddedDeclarations).Project;

		foreach (var grammar in Grammars)
			project = project.AddAdditionalDocument(grammar, Microsoft.CodeAnalysis.Text.SourceText.From(File.ReadAllText(Path.Combine(root, grammar))), filePath: @"P:\Playground\" + grammar).Project;

		document = project.GetDocument(document.Id)!;

		var rootNode     = document.GetSyntaxRootAsync().GetAwaiter().GetResult()!;
		var model        = document.GetSemanticModelAsync().GetAwaiter().GetResult()!;
		var includedFile = StandaloneGrammarInheritance.GramFilesAsync(document.Project, CancellationToken.None).GetAwaiter().GetResult();
		var analyses     = EmbeddedGrammarService.Analyze(model, rootNode, CancellationToken.None, includedFile);
		var dsl          = DslEmbeddedSiteAnalysis.AnalyzeAsync(document, rootNode, model, CancellationToken.None, new DslEmbeddedSiteCache()).GetAwaiter().GetResult();
		var diagnostics  = DslClassificationDiagnostics.AnalyzeAsync(document, rootNode, model.Compilation, CancellationToken.None).GetAwaiter().GetResult();

		return (document, analyses, dsl, diagnostics);
	}

	const string EmbeddedDeclarations = """
		namespace DotGram
		{
			public enum GramClassification { Keyword, Identifier, Type, Variable, Function, Method, Property, Number, String, Comment, Operator, Punctuation, Namespace, Parameter, Label }

			[System.AttributeUsage(System.AttributeTargets.Class)]
			sealed class GramLanguageAttribute(string id) : System.Attribute;

			[System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
			sealed class GramClassifyAttribute(string target, GramClassification classification) : System.Attribute;
		}

		namespace System.Diagnostics.CodeAnalysis
		{
			[System.AttributeUsage(System.AttributeTargets.All)]
			sealed class StringSyntaxAttribute(string syntax) : System.Attribute;
		}
		""";

	static string RecordEmbedded(string text, string root, IReadOnlyList<string> completionMarkers)
	{
		Stage = "embedded analysis";
		var (_, analyses, dsl, dslDiagnostics) = EmbeddedState(text, root);
		var buffer   = new FakeBuffer(text, "CSharp");
		var snapshot = buffer.CurrentSnapshot;
		var result   = new StringBuilder();

		// The extension's buffer analysis, holding that state for this snapshot, as its AnalyzeAsync leaves it.
		var analysis = (EmbeddedGrammarBufferAnalysis)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(EmbeddedGrammarBufferAnalysis));
		void Set(string field, object? value)
		{
			typeof(EmbeddedGrammarBufferAnalysis).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(analysis, value);
		}

		var documents = new Mock<ITextDocumentFactoryService>();
		ITextDocument? none = null;
		documents.Setup(service => service.TryGetTextDocument(It.IsAny<ITextBuffer>(), out none)).Returns(false);

		Set("_gate", new object());
		Set("_buffer", buffer);
		Set("_documents", documents.Object);
		Set("_snapshot", snapshot);
		Set("_classifications", analyses.SelectMany(static item => item.Classifications).ToArray());
		Set("_dslClassifications", dsl.Classifications);
		Set("_dslSymbols", dsl.Symbols);
		Set("_dslSites", dsl.Sites);
		Set("_diagnostics", analyses.SelectMany(static item => item.Diagnostics).Concat(dslDiagnostics).Concat(dsl.Diagnostics).ToArray());
		Set("_symbols", analyses.SelectMany(static item => item.Symbols).ToArray());
		Set("_braces", analyses.SelectMany(static item => item.Braces).ToArray());
		Set("_foldingRanges", analyses.SelectMany(static item => item.FoldingRanges).ToArray());
		Set("_documentSymbols", analyses.SelectMany(static item => item.DocumentSymbols).ToArray());
		Set("_publishedApis", analyses.SelectMany(static item => item.PublishedApis).ToArray());
		buffer.Properties.AddProperty(typeof(EmbeddedGrammarBufferAnalysis), analysis);

		Assert.True(analysis.TryGet(snapshot, out var classifications, out _));
		Assert.NotEmpty(classifications);

		var tokens = classifications.Select(static item => item.Span.Start)
			.Concat(dsl.Classifications.Select(static item => item.Span.Start))
			.Concat(dsl.Sites.Select(static item => item.Span.Start))
			.Distinct().OrderBy(static at => at).ToArray();
		var roslyn = new RoslynGramCompletion(buffer, null!, documents.Object);

		result.Append("# grammars ").Append(analyses.Count).Append(", dsl sites ").Append(dsl.Sites.Count).Append(", dsl symbols ").Append(dsl.Symbols.Count).Append('\n');

		// Completion, at the marked places.
		var completion = new RecordingCompletionSource(new EmbeddedGramCompletionSource(buffer, analysis, roslyn));

		foreach (var marker in completionMarkers)
		{
			var position = Marker(text, marker);
			var point    = new SnapshotPoint(snapshot, position);
			var trigger  = new CompletionTrigger(CompletionTriggerReason.Invoke, snapshot);
			Stage = "embedded completion " + marker;
			var start    = completion.InitializeCompletion(trigger, point, CancellationToken.None);

			result.Append("completion ").Append(Place(text, position));

			if (start.Participation == CompletionParticipation.DoesNotProvideItems)
			{
				result.Append(" none\n");
				continue;
			}

			result.Append(" applicable=").Append(Span(text, start.ApplicableToSpan.Start, start.ApplicableToSpan.Length)).Append('\n');

			var context = completion.GetCompletionContextAsync(null!, trigger, point, start.ApplicableToSpan, CancellationToken.None)
				.GetAwaiter().GetResult();

			foreach (var item in context.ItemList)
			{
				var description = completion.GetDescriptionAsync(null!, item, CancellationToken.None).GetAwaiter().GetResult();

				result.Append("  ").Append(Escape(item.DisplayText))
					.Append(" | insert=").Append(Escape(item.InsertText))
					.Append(" | suffix=").Append(Escape(item.Suffix))
					.Append(" | sort=").Append(Escape(item.SortText))
					.Append(" | filter=").Append(Escape(item.FilterText))
					.Append(" | ").Append(Escape(description as string ?? description?.ToString() ?? "")).Append('\n');
			}
		}

		// Quick Info, at the start of every classified span: the extension's own source, run on this
		// thread as its main thread. Where it asks Roslyn, there is none, and that is recorded as C#.
		var view       = View(buffer, snapshot, 0, out var caret);
		var formatMaps = new Mock<IClassificationFormatMapService>();
		formatMaps.Setup(service => service.GetClassificationFormatMap(It.IsAny<ITextView>()))
			.Returns(new Mock<IClassificationFormatMap>().Object);
		var types      = new Mock<IClassificationTypeRegistryService>();
		var quickInfo  = new EmbeddedGramQuickInfoSource(buffer, analysis, roslyn, formatMaps.Object, types.Object);

		foreach (var position in tokens)
		{
			Stage = "embedded quickinfo " + Place(text, position);
			var session = new Mock<IAsyncQuickInfoSession>();
			session.Setup(item => item.GetTriggerPoint(It.IsAny<ITextSnapshot>())).Returns(new SnapshotPoint(snapshot, position));
			session.Setup(item => item.TextView).Returns(view);

			QuickInfoItem? item;
			try
			{
				item = ThreadHelper.JoinableTaskFactory.Run(() => quickInfo.GetQuickInfoItemAsync(session.Object, CancellationToken.None));
			}
			catch (NullReferenceException)
			{
				result.Append("quickinfo ").Append(Place(text, position)).Append(" csharp\n");
				continue;
			}

			if (item is null)
				continue;

			var span = item.ApplicableToSpan.GetSpan(snapshot);

			result.Append("quickinfo ").Append(Place(text, position)).Append(" span=").Append(Span(text, span.Start, span.Length)).Append('\n');
			Content(item.Item, result, "  ");
		}

		// Signature help, after every opening parenthesis and comma.
		Stage = "embedded signature";
		var signatures = new EmbeddedGramSignatureHelpSource(buffer, analysis);

		for (var position = 0; position < text.Length; position++)
		{
			if (text[position] is not ('(' or ','))
				continue;

			var at      = position + 1;
			var session = new Mock<ISignatureHelpSession>();
			session.Setup(item => item.TextView).Returns(View(buffer, snapshot, at, out _));
			var list = new List<ISignature>();

			signatures.AugmentSignatureHelpSession(session.Object, list);

			foreach (var signature in list)
			{
				var span = signature.ApplicableToSpan.GetSpan(snapshot);

				result.Append("signature ").Append(Place(text, at))
					.Append(" applicable=").Append(Span(text, span.Start, span.Length))
					.Append(" | ").Append(Escape(signature.Content))
					.Append(" | ").Append(Escape(signature.Documentation))
					.Append(" | current=").Append(Escape(signature.CurrentParameter?.Name ?? "-"))
					.Append(" | ").Append(string.Join(", ", signature.Parameters.Select(static parameter =>
						$"{Escape(parameter.Name)}@{parameter.Locus.Start}+{parameter.Locus.Length}")))
					.Append('\n');
			}
		}

		// Definition, Ctrl+click, references, rename and highlights.
		var handler = new GramGoToDefinitionCommandHandler();
		SetImport(handler, "Documents", documents.Object);
		var navigation = new EmbeddedGramNavigableSymbolSource(view, buffer, analysis, new Mock<IServiceProvider>().Object, null!, documents.Object);
		var highlights = new EmbeddedGramReferenceHighlightTagger(view, analysis);

		foreach (var position in tokens)
		{
			caret.Position = position;
			Stage = "embedded navigation " + Place(text, position);

			var args       = new GoToDefinitionCommandArgs(view, buffer);
			var definition = EmbeddedDefinition(handler, args, snapshot, text, position);

			if (definition is not null)
				result.Append("definition ").Append(Place(text, position)).Append(" -> ").Append(definition).Append('\n');

			if (navigation.GetNavigableSymbolAsync(new SnapshotSpan(snapshot, position, 0), CancellationToken.None)
				.GetAwaiter().GetResult() is { } symbol)
			{
				result.Append("navigable ").Append(Place(text, position)).Append(" span=")
					.Append(Span(text, symbol.SymbolSpan.Start, symbol.SymbolSpan.Length)).Append(" -> ");

				if (symbol is GramNavigableSymbol)
				{
					var target = (FakeTrackingPoint)typeof(GramNavigableSymbol)
						.GetField("<definition>P", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(symbol);
					result.Append(Place(text, target.Position));
				}
				else if (symbol is FileNavigableSymbol)
				{
					var file = typeof(FileNavigableSymbol);
					result.Append("file ").Append(file.GetField("<filePath>P", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(symbol))
						.Append(' ').Append(file.GetField("<line>P", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(symbol))
						.Append(':').Append(file.GetField("<column>P", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(symbol));
				}
				else
					result.Append(symbol.GetType().Name);

				result.Append('\n');
			}

			var found = GramFindReferencesTarget.Embedded(snapshot, position, analysis);

			if (found is not null)
			{
				result.Append("references ").Append(Place(text, position))
					.Append(" name=").Append(found.Name)
					.Append(" definition=").Append(found.DefinitionPosition >= text.Length ? "elsewhere" : Place(text, found.DefinitionPosition))
					.Append(" at=[").Append(string.Join(", ", found.Positions.Select(at => Place(text, at)))).Append(']')
					.Append(" elsewhere=").Append(found.Elsewhere.Count)
					.Append('\n');
				result.Append("rename ").Append(Place(text, position)).Append(' ');

				if (found.Renamable)
					result.Append("edits=[").Append(string.Join(", ", found.Positions.Select(at => Span(text, at, found.Name.Length)))).Append(']');
				else
					result.Append("refused");

				result.Append('\n');
			}

			var tags = highlights.GetTags(new NormalizedSnapshotSpanCollection(new SnapshotSpan(snapshot, 0, snapshot.Length))).ToArray();

			if (tags.Length > 0)
				result.Append("highlight ").Append(Place(text, position)).Append(" -> [")
					.Append(string.Join(", ", tags.Select(tag => Span(text, tag.Span.Start, tag.Span.Length) + " " + tag.Tag.Type)))
					.Append("]\n");
		}

		return result.ToString();
	}

	/// <summary>What Go To Definition does at a place of a C# document, in its command handler's order, Roslyn answering nothing.</summary>
	static string? EmbeddedDefinition(
		GramGoToDefinitionCommandHandler handler,
		GoToDefinitionCommandArgs args,
		ITextSnapshot snapshot,
		string text,
		int position)
	{
		if (Call(handler, "DslDefinition", args, snapshot, position) is { } dsl)
		{
			var type = dsl.GetType();
			return $"file {type.GetProperty("FilePath")!.GetValue(dsl)} {type.GetProperty("Line")!.GetValue(dsl)}:{type.GetProperty("Column")!.GetValue(dsl)}";
		}

		if (Call(handler, "PublishedApi", args, snapshot, position) is string api)
			return "published " + api;

		if (Call(handler, "StandardRuleAt", args, snapshot, position) is { } standard)
			return "std " + standard.GetType().GetProperty("Name")!.GetValue(standard) + " " +
				standard.GetType().GetProperty("Line")!.GetValue(standard) + ":" + standard.GetType().GetProperty("Column")!.GetValue(standard);

		var parameters = new object?[] { args, snapshot, position, null, null };
		if ((bool)typeof(GramGoToDefinitionCommandHandler).GetMethod("IsCSharpContext", BindingFlags.Instance | BindingFlags.NonPublic)!
			.Invoke(handler, parameters)!)
			return "csharp " + Escape((string)parameters[3]!) + " @" + parameters[4];

		var found = Call(handler, "Target", args) as GramFindReferencesTarget;

		return found is null || found.DefinedElsewhere ? null : "here " + Place(text, found.DefinitionPosition);
	}

	static string Stage = "";

	static string Record(string text, IReadOnlyList<string> completionMarkers)
	{
		Stage = "analysis";
		var buffer   = new FakeBuffer(text);
		var snapshot = buffer.CurrentSnapshot;
		var analysis = GramBufferAnalysis.For(buffer);
		var document = Analysed(analysis, snapshot);
		var result   = new StringBuilder();

		// No Visual Studio workspace can be made outside Visual Studio, so nothing here reaches Roslyn:
		// completion answers C# with nothing, and Quick Info in C# is recorded as C# and not asked.
		var roslyn = new RoslynGramCompletion(buffer, null!, null!);

		var tokens = document.Classifications.Select(static item => item.Position).Distinct().OrderBy(static at => at).ToArray();

		// Completion, at the marked places.
		Stage = "completion";
		var completion = new RecordingCompletionSource(new GramCompletionSource(buffer, analysis, roslyn));

		foreach (var marker in completionMarkers)
		{
			var position = Marker(text, marker);
			Stage = "completion " + marker;
			var point    = new SnapshotPoint(snapshot, position);
			var trigger  = new CompletionTrigger(CompletionTriggerReason.Invoke, snapshot);
			var start    = completion.InitializeCompletion(trigger, point, CancellationToken.None);

			result.Append("completion ").Append(Place(text, position));

			if (start.Participation == CompletionParticipation.DoesNotProvideItems)
			{
				result.Append(" none\n");
				continue;
			}

			result.Append(" applicable=").Append(Span(text, start.ApplicableToSpan.Start, start.ApplicableToSpan.Length)).Append('\n');

			var context = completion.GetCompletionContextAsync(null!, trigger, point, start.ApplicableToSpan, CancellationToken.None)
				.GetAwaiter().GetResult();

			foreach (var item in context.ItemList)
			{
				var description = completion.GetDescriptionAsync(null!, item, CancellationToken.None).GetAwaiter().GetResult();

				result.Append("  ").Append(Escape(item.DisplayText))
					.Append(" | insert=").Append(Escape(item.InsertText))
					.Append(" | suffix=").Append(Escape(item.Suffix))
					.Append(" | sort=").Append(Escape(item.SortText))
					.Append(" | filter=").Append(Escape(item.FilterText))
					.Append(" | ").Append(Escape(description as string ?? description?.ToString() ?? "")).Append('\n');
			}
		}

		Stage = "quickinfo setup";
		// Quick Info, at the start of every classified token.
		var view        = View(buffer, snapshot, 0, out var caret);
		var formatMaps  = new Mock<IClassificationFormatMapService>();
		formatMaps.Setup(service => service.GetClassificationFormatMap(It.IsAny<ITextView>()))
			.Returns(new Mock<IClassificationFormatMap>().Object);
		var types       = new Mock<IClassificationTypeRegistryService>();

		foreach (var position in tokens)
		{
			if (GramCSharpCompletionContext.TryGetExpression(text, position, out _, out _, out _, out _))
			{
				result.Append("quickinfo ").Append(Place(text, position)).Append(" csharp\n");
				continue;
			}

			Stage = "quickinfo " + Place(text, position);

			// What GramQuickInfoSource.GetQuickInfoItemAsync does between its switches to the main
			// thread, which needs Visual Studio's own JoinableTaskContext: the item of the first
			// classification under the point, and the squiggles' messages in front of it.
			QuickInfoItem? item = null;

			foreach (var classification in document.Classifications)
				if (classification.Position <= position && position < classification.Position + classification.Length)
				{
					item = GramQuickInfoSource.Create(
						view, formatMaps.Object, types.Object, snapshot, classification.Position, classification.Length, classification.Kind,
						classification.QuickInfo ?? GramQuickInfoSource.StandardQuickInfo(
							snapshot, classification.Position, classification.Length, classification.Kind,
							GrammarPosition.Standalone(snapshot, classification.Position)));
					break;
				}

			item = GramQuickInfoSource.WithDiagnostics(item, GramDiagnosticText.At(
				document.Diagnostics.Select(static diagnostic => (diagnostic.Position, diagnostic.Length, diagnostic)),
				position,
				snapshot.Length));

			if (item is null)
				continue;

			var span = item.ApplicableToSpan.GetSpan(snapshot);

			result.Append("quickinfo ").Append(Place(text, position)).Append(" span=").Append(Span(text, span.Start, span.Length)).Append('\n');
			Content(item.Item, result, "  ");
		}

		Stage = "signature";
		// Signature help, after every opening parenthesis and comma.
		var signatures = new GramSignatureHelpSource(buffer, analysis);

		for (var position = 0; position < text.Length; position++)
		{
			if (text[position] is not ('(' or ','))
				continue;

			var at      = position + 1;
			var session = new Mock<ISignatureHelpSession>();
			session.Setup(item => item.TextView).Returns(View(buffer, snapshot, at, out _));
			var list = new List<ISignature>();

			signatures.AugmentSignatureHelpSession(session.Object, list);

			foreach (var signature in list)
			{
				var span = signature.ApplicableToSpan.GetSpan(snapshot);

				result.Append("signature ").Append(Place(text, at))
					.Append(" applicable=").Append(Span(text, span.Start, span.Length))
					.Append(" | ").Append(Escape(signature.Content))
					.Append(" | ").Append(Escape(signature.Documentation))
					.Append(" | current=").Append(Escape(signature.CurrentParameter?.Name ?? "-"))
					.Append(" | ").Append(string.Join(", ", signature.Parameters.Select(static parameter =>
						$"{Escape(parameter.Name)}@{parameter.Locus.Start}+{parameter.Locus.Length}")))
					.Append('\n');
			}
		}

		Stage = "navigation";
		// Definition, references, highlights and rename, at the start of every classified token.
		var handler = new GramGoToDefinitionCommandHandler();
		var navigation = new GramNavigableSymbolSource(view, buffer, analysis, new Mock<IServiceProvider>().Object);
		var highlights = new GramReferenceHighlightTagger(view, analysis);

		foreach (var position in tokens)
		{
			caret.Position = position;
			Stage = "navigation " + Place(text, position);

			var args       = new GoToDefinitionCommandArgs(view, buffer);
			var definition = Definition(handler, args, analysis, snapshot, text, position);

			if (definition is not null)
				result.Append("definition ").Append(Place(text, position)).Append(" -> ").Append(definition).Append('\n');

			if (navigation.GetNavigableSymbolAsync(new SnapshotSpan(snapshot, position, 0), CancellationToken.None)
				.GetAwaiter().GetResult() is { } symbol)
			{
				result.Append("navigable ").Append(Place(text, position)).Append(" span=")
					.Append(Span(text, symbol.SymbolSpan.Start, symbol.SymbolSpan.Length)).Append(" -> ");

				if (symbol is GramNavigableSymbol)
				{
					var target = (FakeTrackingPoint)typeof(GramNavigableSymbol)
						.GetField("<definition>P", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(symbol);
					result.Append(Place(text, target.Position));
				}
				else
					result.Append(symbol.GetType().Name);

				result.Append('\n');
			}

			var found = GramFindReferencesTarget.Standalone(snapshot, position, analysis);

			if (found is not null)
			{
				result.Append("references ").Append(Place(text, position))
					.Append(" name=").Append(found.Name)
					.Append(" definition=").Append(found.DefinitionPosition >= text.Length ? "elsewhere" : Place(text, found.DefinitionPosition))
					.Append(" at=[").Append(string.Join(", ", found.Positions.Select(at => Place(text, at)))).Append(']')
					.Append(" elsewhere=").Append(found.Elsewhere.Count)
					.Append('\n');
				result.Append("rename ").Append(Place(text, position)).Append(' ');

				if (found.Renamable)
					result.Append("edits=[").Append(string.Join(", ", found.Positions.Select(at => Span(text, at, found.Name.Length)))).Append(']');
				else
					result.Append("refused");

				result.Append('\n');
			}

			var tags = highlights.GetTags(new NormalizedSnapshotSpanCollection(new SnapshotSpan(snapshot, 0, snapshot.Length))).ToArray();

			if (tags.Length > 0)
				result.Append("highlight ").Append(Place(text, position)).Append(" -> [")
					.Append(string.Join(", ", tags.Select(tag => Span(text, tag.Span.Start, tag.Span.Length) + " " + tag.Tag.Type)))
					.Append("]\n");
		}

		return result.ToString();
	}

	/// <summary>What Go To Definition does at a place, in the order its command handler asks, without opening anything.</summary>
	static string? Definition(
		GramGoToDefinitionCommandHandler handler,
		GoToDefinitionCommandArgs args,
		GramBufferAnalysis analysis,
		ITextSnapshot snapshot,
		string text,
		int position)
	{
		if (Call(handler, "PublishedApi", args, snapshot, position) is string api)
			return "published " + api;

		if (Call(handler, "StandardRuleAt", args, snapshot, position) is { } standard)
			return "std " + standard.GetType().GetProperty("Name")!.GetValue(standard) + " " +
				standard.GetType().GetProperty("Line")!.GetValue(standard) + ":" + standard.GetType().GetProperty("Column")!.GetValue(standard);

		var parameters = new object?[] { args, snapshot, position, null, null };
		if ((bool)typeof(GramGoToDefinitionCommandHandler).GetMethod("IsCSharpContext", BindingFlags.Instance | BindingFlags.NonPublic)!
			.Invoke(handler, parameters)!)
			return "csharp " + Escape((string)parameters[3]!) + " @" + parameters[4];

		if (analysis.ExternalDefinition(snapshot, position) is { } external)
			return "included " + external.Target.Grammar.Name + " " + external.Target.Line + ":" + external.Target.Column;

		var found = Call(handler, "Target", args) as GramFindReferencesTarget;

		return found is null || found.DefinedElsewhere ? null : "here " + Place(text, found.DefinitionPosition);
	}

	static object? Call(object target, string name, params object[] arguments)
	{
		return target.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(target, arguments);
	}

	static void SetImport(object target, string name, object value)
	{
		target.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(target, value);
	}

	/// <summary>The text a Quick Info panel shows, element by element.</summary>
	static void Content(object content, StringBuilder result, string indent)
	{
		switch (content)
		{
			case DotGramQuickInfoSuppression:
				result.Append(indent).Append("suppressed\n");
				break;

			case ScrollViewer scroll:
				Content(scroll.Content, result, indent);
				break;

			case Panel panel:
				foreach (UIElement child in panel.Children)
					Content(child, result, indent);
				break;

			case CrispImage image:
				result.Append(indent).Append("icon ").Append(Moniker(image.Moniker)).Append('\n');
				break;

			case TextBlock block:
				result.Append(indent).Append(block.Visibility == Visibility.Visible ? "text " : "hidden ")
					.Append(Escape(block.Inlines.Count == 0 ? block.Text : Inlines(block.Inlines))).Append('\n');
				break;

			default:
				result.Append(indent).Append(content.GetType().Name).Append('\n');
				break;
		}
	}

	static string Inlines(InlineCollection inlines)
	{
		var text = new StringBuilder();

		foreach (var inline in inlines)
			switch (inline)
			{
				case Run run:
					text.Append(run.Text);
					break;
				case LineBreak:
					text.Append('\n');
					break;
				case Hyper span:
					text.Append(Inlines(span.Inlines));
					break;
			}

		return text.ToString();
	}

	static string Moniker(ImageMoniker moniker)
	{
		foreach (var (name, known) in new[]
		{
			("Method", KnownMonikers.Method),
			("LocalVariable", KnownMonikers.LocalVariable),
			("IntellisenseKeyword", KnownMonikers.IntellisenseKeyword),
			("IntellisenseLightBulb", KnownMonikers.IntellisenseLightBulb),
		})
			if (known.Guid == moniker.Guid && known.Id == moniker.Id)
				return name;

		return moniker.Guid + ":" + moniker.Id;
	}

	static GramDocument Analysed(GramBufferAnalysis analysis, ITextSnapshot snapshot)
	{
		var document = analysis.Document(snapshot);

		for (var attempt = 0; attempt < 600 && document.Classifications.Count == 0; attempt++)
		{
			Thread.Sleep(100);
			document = analysis.Document(snapshot);
		}

		Assert.NotEmpty(document.Classifications);

		return document;
	}

	static int Marker(string text, string marker)
	{
		var caret  = marker.IndexOf('^');
		var needle = marker.Remove(caret, 1);
		var at     = text.IndexOf(needle, StringComparison.Ordinal);

		Assert.True(at >= 0, "Marker not found: " + marker);

		return at + caret;
	}

	static string Place(string text, int position)
	{
		var line   = 1;
		var column = 1;

		for (var at = 0; at < position && at < text.Length; at++)
		{
			if (text[at] == '\n')
			{
				line++;
				column = 1;
			}
			else
				column++;
		}

		return line + ":" + column;
	}

	static string Span(string text, int position, int length)
	{
		return Place(text, position) + "+" + length;
	}

	static string Escape(string? value)
	{
		return value is null
			? "<null>"
			: value.Replace("\\", "\\\\").Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");
	}

	static string SourceRootDirectory()
	{
		var declared = typeof(PlaygroundRecording).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
			.FirstOrDefault(static one => one.Key == "DotGramSourceRoot")?.Value;

		if (declared is { Length: > 0 } && File.Exists(Path.Combine(declared, "DotGram.slnx")))
			return Path.GetFullPath(declared);

		var at = new DirectoryInfo(AppContext.BaseDirectory);

		while (at is not null && !File.Exists(Path.Combine(at.FullName, "DotGram.slnx")))
			at = at.Parent;

		return at!.FullName;
	}

	static ITextView View(ITextBuffer buffer, ITextSnapshot snapshot, int position, out FakeCaret caret)
	{
		var view = new Mock<ITextView>();
		caret = new FakeCaret(snapshot) { Position = position };
		view.Setup(item => item.TextBuffer).Returns(buffer);
		view.Setup(item => item.TextSnapshot).Returns(snapshot);
		view.Setup(item => item.Caret).Returns(caret.Object);
		view.Setup(item => item.Properties).Returns(new PropertyCollection());

		return view.Object;
	}

	sealed class FakeCaret
	{
		readonly ITextSnapshot _snapshot;
		readonly Mock<ITextCaret> _caret = new();

		public FakeCaret(ITextSnapshot snapshot)
		{
			_snapshot = snapshot;
			_caret.Setup(item => item.Position).Returns(() => new CaretPosition(
				new VirtualSnapshotPoint(new SnapshotPoint(_snapshot, Position)),
				new Mock<IMappingPoint>().Object,
				PositionAffinity.Successor));
		}

		public int Position { get; set; }

		public ITextCaret Object => _caret.Object;
	}

	sealed class FakeBuffer : ITextBuffer
	{
		public FakeBuffer(string text, string contentType = GramContentType.Name)
		{
			CurrentSnapshot = new FakeSnapshot(this, text);
			ContentType     = Content(contentType);
		}

		public PropertyCollection Properties { get; } = new();
		public IContentType ContentType { get; }
		public ITextSnapshot CurrentSnapshot { get; }
		public bool EditInProgress => false;

		public event EventHandler<SnapshotSpanEventArgs>? ReadOnlyRegionsChanged { add { } remove { } }
		public event EventHandler<TextContentChangedEventArgs>? Changed { add { } remove { } }
		public event EventHandler<TextContentChangedEventArgs>? ChangedLowPriority { add { } remove { } }
		public event EventHandler<TextContentChangedEventArgs>? ChangedHighPriority { add { } remove { } }
		public event EventHandler<TextContentChangingEventArgs>? Changing { add { } remove { } }
		public event EventHandler? PostChanged { add { } remove { } }
		public event EventHandler<ContentTypeChangedEventArgs>? ContentTypeChanged { add { } remove { } }

		public void ChangeContentType(IContentType newContentType, object editTag) { throw new NotSupportedException(); }
		public bool CheckEditAccess() { return false; }
		public ITextEdit CreateEdit(EditOptions options, int? reiteratedVersionNumber, object editTag) { throw new NotSupportedException(); }
		public ITextEdit CreateEdit() { throw new NotSupportedException(); }
		public IReadOnlyRegionEdit CreateReadOnlyRegionEdit() { throw new NotSupportedException(); }
		public ITextSnapshot Delete(Span deleteSpan) { throw new NotSupportedException(); }
		public NormalizedSpanCollection GetReadOnlyExtents(Span span) { throw new NotSupportedException(); }
		public ITextSnapshot Insert(int position, string text) { throw new NotSupportedException(); }
		public bool IsReadOnly(int position) { return true; }
		public bool IsReadOnly(int position, bool isEdit) { return true; }
		public bool IsReadOnly(Span span) { return true; }
		public bool IsReadOnly(Span span, bool isEdit) { return true; }
		public ITextSnapshot Replace(Span replaceSpan, string replaceWith) { throw new NotSupportedException(); }
		public void TakeThreadOwnership() { }
	}

	static IContentType Content(string contentType)
	{
		var type = new Mock<IContentType>();
		type.Setup(item => item.IsOfType(It.IsAny<string>())).Returns<string>(name => name == contentType);
		type.Setup(item => item.TypeName).Returns(contentType);

		return type.Object;
	}

	sealed class FakeSnapshot(ITextBuffer buffer, string text) : ITextSnapshot
	{
		public ITextBuffer TextBuffer { get; } = buffer;
		public IContentType ContentType => TextBuffer.ContentType;
		public ITextVersion Version { get; } = new Mock<ITextVersion>().Object;
		public int Length => text.Length;
		public int LineCount => throw new NotSupportedException();
		public char this[int position] => text[position];
		public IEnumerable<ITextSnapshotLine> Lines => throw new NotSupportedException();

		public void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count) { text.CopyTo(sourceIndex, destination, destinationIndex, count); }
		public ITrackingPoint CreateTrackingPoint(int position, PointTrackingMode trackingMode) { return new FakeTrackingPoint(this, position); }
		public ITrackingPoint CreateTrackingPoint(int position, PointTrackingMode trackingMode, TrackingFidelityMode trackingFidelity) { return new FakeTrackingPoint(this, position); }
		public ITrackingSpan CreateTrackingSpan(Span span, SpanTrackingMode trackingMode) { return new FakeTrackingSpan(this, span); }
		public ITrackingSpan CreateTrackingSpan(Span span, SpanTrackingMode trackingMode, TrackingFidelityMode trackingFidelity) { return new FakeTrackingSpan(this, span); }
		public ITrackingSpan CreateTrackingSpan(int start, int length, SpanTrackingMode trackingMode) { return new FakeTrackingSpan(this, new Span(start, length)); }
		public ITrackingSpan CreateTrackingSpan(int start, int length, SpanTrackingMode trackingMode, TrackingFidelityMode trackingFidelity) { return new FakeTrackingSpan(this, new Span(start, length)); }
		public ITextSnapshotLine GetLineFromLineNumber(int lineNumber) { throw new NotSupportedException(); }
		public ITextSnapshotLine GetLineFromPosition(int position) { throw new NotSupportedException(); }
		public int GetLineNumberFromPosition(int position) { throw new NotSupportedException(); }
		public string GetText(Span span) { return text.Substring(span.Start, span.Length); }
		public string GetText(int startIndex, int length) { return text.Substring(startIndex, length); }
		public string GetText() { return text; }
		public char[] ToCharArray(int startIndex, int length) { return text.ToCharArray(startIndex, length); }
		public void Write(TextWriter writer, Span span) { writer.Write(GetText(span)); }
		public void Write(TextWriter writer) { writer.Write(text); }
	}

	sealed class FakeTrackingPoint(ITextSnapshot snapshot, int position) : ITrackingPoint
	{
		public int Position { get; } = position;
		public ITextBuffer TextBuffer => snapshot.TextBuffer;
		public PointTrackingMode TrackingMode => PointTrackingMode.Negative;
		public TrackingFidelityMode TrackingFidelity => TrackingFidelityMode.Forward;

		public char GetCharacter(ITextSnapshot target) { return target[Position]; }
		public SnapshotPoint GetPoint(ITextSnapshot target) { return new SnapshotPoint(target, Position); }
		public int GetPosition(ITextSnapshot target) { return Position; }
		public int GetPosition(ITextVersion version) { return Position; }
	}

	sealed class FakeTrackingSpan(ITextSnapshot snapshot, Span span) : ITrackingSpan
	{
		public ITextBuffer TextBuffer => snapshot.TextBuffer;
		public SpanTrackingMode TrackingMode => SpanTrackingMode.EdgeExclusive;
		public TrackingFidelityMode TrackingFidelity => TrackingFidelityMode.Forward;

		public SnapshotPoint GetEndPoint(ITextSnapshot target) { return new SnapshotPoint(target, span.End); }
		public Span GetSpan(ITextVersion version) { return span; }
		public SnapshotSpan GetSpan(ITextSnapshot target) { return new SnapshotSpan(target, span); }
		public SnapshotPoint GetStartPoint(ITextSnapshot target) { return new SnapshotPoint(target, span.Start); }
		public string GetText(ITextSnapshot target) { return target.GetText(span); }
	}

	/// <summary>
	/// The extension's completion source with its own answers, and none from Roslyn: what the C# around
	/// a grammar completes to is Roslyn's, and there is no project here to ask.
	/// </summary>
	sealed class RecordingCompletionSource(GramCompletionSourceBase real) : GramCompletionSourceBase
	{
		protected override (string Text, int Position)? GrammarAt(SnapshotPoint point)
		{
			return ((string Text, int Position)?)Invoke("GrammarAt", point);
		}

		protected override bool IsApplicable(SnapshotPoint point)
		{
			return (bool)Invoke("IsApplicable", point)!;
		}

		protected override IReadOnlyDictionary<string, RuleCompletion> Definitions(SnapshotPoint point)
		{
			return (IReadOnlyDictionary<string, RuleCompletion>)Invoke("Definitions", point)!;
		}

		protected override Task<System.Collections.Immutable.ImmutableArray<CompletionItem>> CSharpCompletionsAsync(
			string prefix, CancellationToken cancellationToken)
		{
			return Task.FromResult(System.Collections.Immutable.ImmutableArray<CompletionItem>.Empty);
		}

		protected override IReadOnlyList<DslLiteralCompletion> DslCompletions(SnapshotPoint point)
		{
			return (IReadOnlyList<DslLiteralCompletion>)Invoke("DslCompletions", point)!;
		}

		object? Invoke(string name, SnapshotPoint point)
		{
			return real.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(real, [point]);
		}
	}
}
