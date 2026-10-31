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
using System.Windows.Threading;

using DotGram.Language;

using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

using Moq;

using Xunit;

using Hyper = System.Windows.Documents.Span;
using Span = Microsoft.VisualStudio.Text.Span;

// The test runs on a thread of its own with nothing else on it: waiting there deadlocks nothing.
#pragma warning disable VSTHRD002

namespace DotGram.VisualStudio.Tests;

/// <summary>
/// The extension's adapters — its completion source, Quick Info panels, signature help, Ctrl+click and
/// highlight taggers, the references a command acts on — answer on the playground grammars what the
/// extension answered there before its feature code moved into the shared providers.
/// </summary>
/// <remarks>
/// The same baselines <c>DotGram.Language.Tests</c> holds the providers to, read the same way, through
/// the Visual Studio types instead: what is held here is that the adapters map the providers' answers
/// onto Visual Studio and add nothing. Go To Definition is the provider's answer, which its command
/// only opens. Nothing reaches Roslyn: the workspace here has no project.
/// </remarks>
public sealed class PlaygroundAdapterTests
{
	static readonly string[] Grammars = ["VisualStudioToolingPlayground.gram", "ToolingQuery.gram", "RecoveryPlayground.gram"];

	[Fact]
	public void The_adapters_answer_as_the_extension_did()
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

		var root      = SourceRootDirectory();
		var grammars  = Path.Combine(root, "tests", "DotGram.VisualStudio.Tests", "Playground");
		var baselines = Path.Combine(root, "tests", "DotGram.Language.Tests", "Playground");
		var markers   = File.ReadAllLines(Path.Combine(baselines, "CompletionMarkers.txt"));

		foreach (var grammar in Grammars)
		{
			var text     = File.ReadAllText(Path.Combine(grammars, grammar));
			var expected = File.ReadAllText(Path.Combine(baselines, Path.GetFileNameWithoutExtension(grammar) + ".txt")).Replace("\r\n", "\n");
			var actual   = Record(text, markers.Where(line => line.StartsWith(grammar + "|", StringComparison.Ordinal))
				.Select(line => line.Substring(grammar.Length + 1)).ToArray());

			Assert.True(expected == actual, $"{grammar}: the adapters no longer answer as the extension did. First difference:\n{FirstDifference(expected, actual)}");
		}

		const string embedded = "VisualStudioToolingPlayground.cs";
		var embeddedText     = File.ReadAllText(Path.Combine(grammars, embedded));
		var embeddedExpected = File.ReadAllText(Path.Combine(baselines, embedded + ".txt")).Replace("\r\n", "\n");
		var embeddedActual   = RecordEmbedded(embeddedText, grammars, markers
			.Where(line => line.StartsWith(embedded + "|", StringComparison.Ordinal)).Select(line => line.Substring(embedded.Length + 1)).ToArray());

		Assert.True(embeddedExpected == embeddedActual, $"{embedded}: the adapters no longer answer as the extension did. First difference:\n{FirstDifference(embeddedExpected, embeddedActual)}");
	}

	/// <summary>
	/// The extension's embedded adapters over the real embedded-grammar session, its host a workspace
	/// with the playground's C# beside its .gram files and the DotGram attributes declared as source, as
	/// the recording had it; nothing reaches Roslyn's Features.
	/// </summary>
	static string RecordEmbedded(string text, string root, IReadOnlyList<string> completionMarkers)
	{
		var buffer    = new FakeBuffer(text, "CSharp");
		var snapshot  = buffer.CurrentSnapshot;
		var documents = new Mock<ITextDocumentFactoryService>();
		ITextDocument? none = null;
		documents.Setup(service => service.TryGetTextDocument(It.IsAny<ITextBuffer>(), out none)).Returns(false);

		using var workspace = PlaygroundWorkspace(text, root);
		var host     = new EmbeddedHost(buffer, workspace);
		var analysis = (EmbeddedGrammarBufferAnalysis)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(EmbeddedGrammarBufferAnalysis));
		void Set(string field, object? value)
		{
			typeof(EmbeddedGrammarBufferAnalysis).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(analysis, value);
		}

		Set("_buffer", buffer);
		Set("_documents", documents.Object);
		// Both of the session's analyses, the colours from C# syntax and then the meaning, waited for
		// on its own notices: asking for a version not analysed yet starts its analysis again.
		using var analysed = new ManualResetEventSlim();
		var embeddedSession = new EmbeddedGrammarSession<ITextSnapshot>(host);

		embeddedSession.Changed += version =>
		{
			if (embeddedSession.View(version).Symbols.Count > 0)
				analysed.Set();
		};
		Set("_session", embeddedSession);
		buffer.Properties.AddProperty(typeof(EmbeddedGrammarBufferAnalysis), analysis);

		Assert.True(analysed.Wait(TimeSpan.FromSeconds(60)));

		var view   = analysis.View(snapshot);
		var result = new StringBuilder();

		Assert.True(view.Current);
		result.Append("# grammars ").Append(view.Classifications.Select(static item => item.GrammarSpan).Distinct().Count())
			.Append(", dsl sites ").Append(view.DslSites.Count).Append(", dsl symbols ").Append(view.DslSymbols.Count).Append('\n');

		var tokens = view.Classifications.Select(static item => item.Span.Start)
			.Concat(view.DslClassifications.Select(static item => item.Span.Start))
			.Concat(view.DslSites.Select(static item => item.Span.Start))
			.Distinct().OrderBy(static at => at).ToArray();
		using var empty = new AdhocWorkspace();
		var roslyn = new RoslynGramFeatures(new RoslynGramCompletion(empty, static () => null), null!);

		var completion = new EmbeddedGramCompletionSource(buffer, analysis, roslyn);

		foreach (var marker in completionMarkers)
		{
			var position = Marker(text, marker);
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

		var textView   = View(buffer, snapshot, 0, out var caret);
		var formatMaps = new Mock<IClassificationFormatMapService>();
		formatMaps.Setup(service => service.GetClassificationFormatMap(It.IsAny<ITextView>()))
			.Returns(new Mock<IClassificationFormatMap>().Object);
		var types      = new Mock<IClassificationTypeRegistryService>();

		foreach (var position in tokens)
		{
			var askedRoslyn = false;
			var answer = GramQuickInfoProvider.EmbeddedAsync(
				text,
				analysis.View(snapshot),
				position,
				(_, _, _) =>
				{
					askedRoslyn = true;
					return Task.FromResult<RoslynGramQuickInfo?>(null);
				},
				CancellationToken.None).GetAwaiter().GetResult();

			if (askedRoslyn)
			{
				result.Append("quickinfo ").Append(Place(text, position)).Append(" csharp\n");
				continue;
			}

			if (answer is null)
				continue;

			var item = GramQuickInfoSource.WithDiagnostics(
				GramQuickInfoSource.Item(textView, formatMaps.Object, types.Object, snapshot, answer.Item),
				answer.Diagnostics);

			if (item is null)
				continue;

			var span = item.ApplicableToSpan.GetSpan(snapshot);

			result.Append("quickinfo ").Append(Place(text, position)).Append(" span=").Append(Span(text, span.Start, span.Length)).Append('\n');
			Content(item.Item, result, "  ");
		}

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

		var navigation = new EmbeddedGramNavigableSymbolSource(textView, buffer, analysis, new Mock<IServiceProvider>().Object, null!, documents.Object);
		var highlights = new EmbeddedGramReferenceHighlightTagger(textView, analysis);

		foreach (var position in tokens)
		{
			caret.Position = position;

			if (GramNavigationProvider.DefinitionAsync(text, () => analysis.View(snapshot), position, null, CancellationToken.None)
				.GetAwaiter().GetResult() is { } definition)
				result.Append("definition ").Append(Place(text, position)).Append(" -> ").Append(Definition(text, definition)).Append('\n');

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

			var found = GramFindReferencesTarget.At(buffer, snapshot, position, null!, documents.Object);

			if (found is not null)
			{
				result.Append("references ").Append(Place(text, position))
					.Append(" name=").Append(found.Name)
					.Append(" definition=").Append(found.DefinitionPosition >= text.Length ? "elsewhere" : Place(text, found.DefinitionPosition))
					.Append(" at=[").Append(string.Join(", ", found.Positions.Select(at => Place(text, at)))).Append(']')
					.Append(" elsewhere=").Append(found.Elsewhere.Count)
					.Append('\n');
				result.Append("rename ").Append(Place(text, position)).Append(' ');

				if (GramRenameProvider.Refusal(found) is null)
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

	const string PlaygroundPath = @"P:\Playground\VisualStudioToolingPlayground.cs";

	/// <summary>A workspace holding the playground's C#, the DotGram attributes as source, and its .gram files.</summary>
	static AdhocWorkspace PlaygroundWorkspace(string text, string root)
	{
		var workspace = new AdhocWorkspace();
		var project   = workspace.AddProject(ProjectInfo.Create(
			ProjectId.CreateNewId(),
			VersionStamp.Default,
			"Playground",
			"Playground",
			LanguageNames.CSharp,
			parseOptions: Microsoft.CodeAnalysis.CSharp.CSharpParseOptions.Default.WithLanguageVersion(Microsoft.CodeAnalysis.CSharp.LanguageVersion.Preview),
			metadataReferences: [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)]));
		project = project.AddDocument("VisualStudioToolingPlayground.cs", Microsoft.CodeAnalysis.Text.SourceText.From(text), filePath: PlaygroundPath).Project
			.AddDocument("DotGram.Attributes.g.cs", Microsoft.CodeAnalysis.Text.SourceText.From(DotGram.Grammar.GramCompiler.EmitMarkerAttributes().Text)).Project
			.AddDocument("Declarations.cs", EmbeddedDeclarations).Project;

		foreach (var grammar in Grammars)
			project = project.AddAdditionalDocument(
				grammar, Microsoft.CodeAnalysis.Text.SourceText.From(File.ReadAllText(Path.Combine(root, grammar))), filePath: @"P:\Playground\" + grammar).Project;

		Assert.True(workspace.TryApplyChanges(project.Solution));

		return workspace;
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

	/// <summary>The embedded session's host: the fake buffer's one version, and the workspace's document of it.</summary>
	sealed class EmbeddedHost(ITextBuffer buffer, Workspace workspace) : IEmbeddedGrammarHost<ITextSnapshot>
	{
		public IGramScheduler Scheduler => GramScheduler.Instance;

		public ITextSnapshot Current => buffer.CurrentSnapshot;

		public string? FilePath => PlaygroundPath;

		public string Text(ITextSnapshot version)
		{
			return version.GetText();
		}

		public int Length(ITextSnapshot version)
		{
			return version.Length;
		}

		public char At(ITextSnapshot version, int position)
		{
			return version[position];
		}

		public Microsoft.CodeAnalysis.Text.TextSpan Translate(Microsoft.CodeAnalysis.Text.TextSpan span, ITextSnapshot from, ITextSnapshot to)
		{
			return span;
		}

		public Microsoft.CodeAnalysis.Document? Document(ITextSnapshot version)
		{
			return EmbeddedGrammarDocuments.WithText(workspace.CurrentSolution, PlaygroundPath, version.GetText());
		}

		public Task RaiseAsync(Action raise, CancellationToken cancellationToken)
		{
			raise();
			return Task.CompletedTask;
		}

		public void Report(Exception exception)
		{
			throw new InvalidOperationException("The session reported an exception.", exception);
		}
	}

	static string Record(string text, IReadOnlyList<string> completionMarkers)
	{
		var buffer   = new FakeBuffer(text);
		var snapshot = buffer.CurrentSnapshot;
		var analysis = GramBufferAnalysis.For(buffer);
		var document = Analysed(analysis, snapshot);
		var result   = new StringBuilder();

		using var workspace = new AdhocWorkspace();
		var roslyn = new RoslynGramFeatures(new RoslynGramCompletion(workspace, static () => null), null!);
		var tokens = document.Classifications.Select(static item => item.Position).Distinct().OrderBy(static at => at).ToArray();

		// Completion, at the marked places.
		var completion = new GramCompletionSource(buffer, analysis, roslyn);

		foreach (var marker in completionMarkers)
		{
			var position = Marker(text, marker);
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

		// Quick Info, at the start of every classified token: the provider's answer, and the panel the
		// extension makes of it.
		var view       = View(buffer, snapshot, 0, out var caret);
		var formatMaps = new Mock<IClassificationFormatMapService>();
		formatMaps.Setup(service => service.GetClassificationFormatMap(It.IsAny<ITextView>()))
			.Returns(new Mock<IClassificationFormatMap>().Object);
		var types      = new Mock<IClassificationTypeRegistryService>();

		foreach (var position in tokens)
		{
			var askedRoslyn = false;
			var answer = GramQuickInfoProvider.StandaloneAsync(
				analysis.View(snapshot),
				position,
				(_, _, _) =>
				{
					askedRoslyn = true;
					return Task.FromResult<RoslynGramQuickInfo?>(null);
				},
				CancellationToken.None).GetAwaiter().GetResult();

			if (askedRoslyn)
			{
				result.Append("quickinfo ").Append(Place(text, position)).Append(" csharp\n");
				continue;
			}

			var item = GramQuickInfoSource.WithDiagnostics(
				GramQuickInfoSource.Item(view, formatMaps.Object, types.Object, snapshot, answer.Item),
				answer.Diagnostics);

			if (item is null)
				continue;

			var span = item.ApplicableToSpan.GetSpan(snapshot);

			result.Append("quickinfo ").Append(Place(text, position)).Append(" span=").Append(Span(text, span.Start, span.Length)).Append('\n');
			Content(item.Item, result, "  ");
		}

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

		// Definition, Ctrl+click, references, rename and highlights, at the start of every classified token.
		var navigation = new GramNavigableSymbolSource(view, buffer, analysis, new Mock<IServiceProvider>().Object);
		var highlights = new GramReferenceHighlightTagger(view, analysis);

		foreach (var position in tokens)
		{
			caret.Position = position;

			if (GramNavigationProvider.DefinitionAsync(analysis.View(snapshot), position, null, CancellationToken.None)
				.GetAwaiter().GetResult() is { } definition)
				result.Append("definition ").Append(Place(text, position)).Append(" -> ").Append(Definition(text, definition)).Append('\n');

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

			var found = GramFindReferencesTarget.At(buffer, snapshot, position, null!, null!);

			if (found is not null)
			{
				result.Append("references ").Append(Place(text, position))
					.Append(" name=").Append(found.Name)
					.Append(" definition=").Append(found.DefinitionPosition >= text.Length ? "elsewhere" : Place(text, found.DefinitionPosition))
					.Append(" at=[").Append(string.Join(", ", found.Positions.Select(at => Place(text, at)))).Append(']')
					.Append(" elsewhere=").Append(found.Elsewhere.Count)
					.Append('\n');
				result.Append("rename ").Append(Place(text, position)).Append(' ');

				if (GramRenameProvider.Refusal(found) is null)
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

	static string Definition(string text, GramLocation definition)
	{
		return definition switch
		{
			GramLocation.PublishedMethod target   => "published " + target.MethodName,
			GramLocation.InStandardLibrary target => $"std {target.Rule} {target.Line}:{target.Column}",
			GramLocation.HostSymbol target        => $"csharp {Escape(target.Expression)} @{target.Position}",
			GramLocation.InIncludedGrammar target => $"included {target.Grammar.Name} {target.Line}:{target.Column}",
			GramLocation.InDocument target        => "here " + Place(text, target.Position),
			GramLocation.InFile target            => $"file {target.Path} {target.Line}:{target.Column}",
			_                                     => definition.ToString(),
		};
	}

	static string FirstDifference(string expected, string actual)
	{
		var left  = expected.Split('\n');
		var right = actual.Split('\n');

		for (var index = 0; index < Math.Max(left.Length, right.Length); index++)
		{
			var one   = index < left.Length ? left[index] : "<end>";
			var other = index < right.Length ? right[index] : "<end>";

			if (one != other)
				return $"line {index + 1}\n  recorded: {one}\n  now:      {other}";
		}

		return "none";
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
		var declared = typeof(PlaygroundAdapterTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
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
}
