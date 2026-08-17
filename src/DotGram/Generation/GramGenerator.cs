using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using DotGram.Grammar;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotGram.Generation;

/// <summary>
/// Roslyn shell over <see cref="GramCompiler"/>.
/// </summary>
/// <remarks>
/// Everything here is Roslyn-specific: find the classes that host a grammar, decide
/// accessibility from what the compilation references, convert diagnostics on the way
/// back. Compilation itself lives in <see cref="DotGram.Grammar"/> and is callable —
/// and testable — without any of this.
/// </remarks>
[Generator(LanguageNames.CSharp)]
public sealed class GramGenerator : IIncrementalGenerator
{
	const string GramFileExtension = ".gram";
	const string GramAttribute     = "DotGram.GramAttribute";

	/// <summary>
	/// The stages, named so that what re-ran can be read back.
	/// </summary>
	/// <remarks>
	/// Public because the names are how the incremental behaviour is checked at all —
	/// Roslyn reports a step under the name it was given and under no other — and a
	/// property nothing can observe is a property nothing keeps.
	/// </remarks>
	public const string AskedStage    = "Asked";
	public const string AnsweredStage = "Answered";
	public const string CompiledStage = "Compiled";

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		// `[Gram]` and the one support type: always internal, always present, so that the
		// attribute can be written in source at all and nothing has to be found anywhere.
		context.RegisterPostInitializationOutput(static postInit =>
		{
			var source = GramCompiler.EmitMarkerAttributes();

			postInit.AddSource(source.HintName, source.Text);
		});

		// The unit of generation is the host class, not the file (§1). A .gram file no
		// class claims generates nothing — there would be nowhere to put the result.
		var hosts = context.SyntaxProvider.ForAttributeWithMetadataName(
			GramAttribute,
			static (node, _) => node is ClassDeclarationSyntax,
			static (candidate, _) => Host.From(candidate));

		var files = context.AdditionalTextsProvider
			.Where(static file => file.Path.EndsWith(GramFileExtension, StringComparison.OrdinalIgnoreCase))
			.Select(static (file, cancellationToken) => new GrammarFile(
				Path: file.Path,
				Text: file.GetText(cancellationToken)?.ToString() ?? ""))
			.Collect();

		// Three stages, and the shape of them is what makes this incremental.
		//
		// A `Compilation` is a different object after every keystroke, so anything
		// downstream of one is recomputed for every character typed. Binding genuinely
		// needs it for declared C# types, their constructors and their properties, so the
		// dependency cannot be removed, only narrowed to what it is for. C# methods are not
		// among those questions: syntax fixes their call shape and the C# compiler binds it.
		//
		//   grammar  ──► the questions its C# names raise      cached on the grammar
		//   + host                     │
		//                              ▼
		//   Compilation ──► the answers, as a list of values   re-runs, and is cheap
		//                              │
		//                              ▼
		//   grammar + host + answers ──► the parser            cached on all three
		//
		// So editing a C# file re-runs the middle stage — a handful of symbol lookups —
		// and stops there, because the answers it produces are the same ones.
		// The names are what a test reads to say which stage re-ran, and are the only way
		// to tell "the answers were the same" from "nothing was asked".
		var asked = hosts
			.Combine(files)
			.Select(static (input, _) => AskedSafely(input.Left, input.Right))
			.WithTrackingName(AskedStage);

		var answered = asked
			.Combine(context.CompilationProvider)
			.Select(static (input, _) => AnswerSafely(input.Left, input.Right))
			.WithTrackingName(AnsweredStage);

		context.RegisterSourceOutput(
			answered
				.Select(static (grammar, _) => CompileSafely(grammar))
				.WithTrackingName(CompiledStage),
			static (production, parser) => parser.Deliver(production));
	}

	// ── Parsers ──────────────────────────────────────────────────────────────────

	/// <summary>
	/// Grammar text is untrusted input. An unexpected non-fatal exception is a compiler
	/// defect to report, never a reason for Roslyn to disable the generator with CS8785.
	/// </summary>
	static Grammar AskedSafely(Host host, ImmutableArray<GrammarFile> files)
	{
		try
		{
			return Asked(host, files);
		}
		catch (Exception exception) when (Recoverable(exception))
		{
			return Failed(
				new Grammar(host, null, null, default, default, default),
				"reading and analyzing the grammar",
				exception);
		}
	}

	static Grammar AnswerSafely(Grammar grammar, Compilation compilation)
	{
		try
		{
			return grammar with
			{
				Answers = new EquatableArray<Answer>(Questions.Ask(
					grammar.Questions.Items,
					new RoslynSymbolResolver(compilation, grammar.Host.MetadataName))),
			};
		}
		catch (Exception exception) when (Recoverable(exception))
		{
			return Failed(grammar, "answering C# type questions", exception);
		}
	}

	static Parser CompileSafely(Grammar grammar)
	{
		try
		{
			return Compile(grammar);
		}
		catch (Exception exception) when (Recoverable(exception))
		{
			var failed = Failed(grammar, "compiling the recognition graph", exception);

			return new Parser(null, null, failed.Reports);
		}
	}

	static bool Recoverable(Exception exception) =>
		exception is not OperationCanceledException and not OutOfMemoryException;

	static Grammar Failed(Grammar grammar, string stage, Exception exception)
	{
		var reports = ImmutableArray.CreateBuilder<Report>();

		reports.AddRange(grammar.Reports.Items);
		reports.Add(Report.Of(
			Diagnostics.InternalFailure,
			grammar.Host.Location,
			stage,
			exception.GetType().FullName ?? exception.GetType().Name,
			exception.Message));

		// Stop this host here. Continuing with partial questions or answers would only turn
		// one defect into a cascade from the next stage.
		return grammar with
		{
			Text      = null,
			Questions = default,
			Answers   = default,
			Reports   = Values(reports),
		};
	}

	/// <summary>
	/// Everything one host produced, as values: the file to add and the diagnostics to
	/// report.
	/// </summary>
	/// <remarks>
	/// A <c>Diagnostic</c> is built at delivery rather than carried, because building it
	/// needs a <c>DiagnosticDescriptor</c> and a <c>Location</c>, and what those compare
	/// as is not this file's business to depend on. The pieces are strings and numbers,
	/// which compare the way arithmetic does.
	/// </remarks>
	readonly record struct Parser(string? HintName, string? Text, EquatableArray<Report> Reports)
	{
		public void Deliver(SourceProductionContext context)
		{
			foreach (var report in Reports.Items)
				try
				{
					context.ReportDiagnostic(report.ToRoslyn());
				}
				catch (Exception exception) when (Recoverable(exception))
				{
					context.ReportDiagnostic(InternalDiagnostic(
						"delivering a grammar diagnostic", exception, report.Fallback));
				}

			if (HintName is not null && Text is not null)
				try
				{
					context.AddSource(HintName, Text);
				}
				catch (Exception exception) when (Recoverable(exception))
				{
					context.ReportDiagnostic(InternalDiagnostic(
						"adding generated C# to the compilation", exception, null));
				}
		}
	}

	static Diagnostic InternalDiagnostic(string stage, Exception exception, Location? location) =>
		Diagnostic.Create(
			Diagnostics.InternalFailure,
			location ?? Location.None,
			stage,
			exception.GetType().FullName ?? exception.GetType().Name,
			exception.Message);

	/// <summary>
	/// One host's grammar, found and read, with the questions its C# names raise and —
	/// once the middle stage has run — what the host said about them.
	/// </summary>
	/// <remarks>
	/// Every field is a value, which is the point: this is what the expensive stage is
	/// cached on.
	/// </remarks>
	readonly record struct Grammar(
		Host                     Host,
		string?                  Text,
		string?                  Path,
		EquatableArray<Question> Questions,
		EquatableArray<Answer>   Answers,
		EquatableArray<Report>   Reports);

	/// <summary>
	/// Stage one: find the grammar and work out what it needs to know about the host's C#.
	/// No compilation, so this is cached on the grammar text and the host alone.
	/// </summary>
	static Grammar Asked(Host host, ImmutableArray<GrammarFile> files)
	{
		var reports = ImmutableArray.CreateBuilder<Report>();

		if (!host.IsPartial)
		{
			reports.Add(Report.Of(Diagnostics.HostNotPartial, host.Location, host.ClassName));

			return new Grammar(host, null, null, default, default, Values(reports));
		}

		if (!TryResolveGrammar(reports, host, files, out var text, out var path))
			return new Grammar(host, null, null, default, default, Values(reports));

		// Parsed twice over a grammar's life: once here for the questions, once in the
		// third stage for the answer. Both are cheap next to normalization and emission,
		// and this one only re-runs when the grammar itself changes.
		var parsed = DotGram.Grammar.Parsing.GramParser.Parse(
			DotGram.Grammar.Parsing.GramLexer.Tokenize(text, RoslynCSharpScanner.Instance)).File;

		return new Grammar(
			host,
			text,
			path,
			new EquatableArray<Question>(Questions.Of(parsed)),
			default,
			Values(reports));
	}

	/// <summary>
	/// Stage three: the grammar compiled against what the host answered. No compilation
	/// reaches here, so it runs only when the grammar or one of the answers changed.
	/// </summary>
	static Parser Compile(Grammar grammar)
	{
		if (grammar.Text is not { } text)
			return new Parser(null, null, grammar.Reports);

		var host    = grammar.Host;
		var reports = ImmutableArray.CreateBuilder<Report>();

		var result = GramCompiler.Compile(text, new GramCompilerOptions
		{
			FileName       = grammar.Path ?? host.SimpleName + GramFileExtension,
			ClassName      = host.ClassName,
			Namespace      = host.Namespace,
			SymbolResolver = new AnsweredSymbolResolver(grammar.Answers.Items),
			CSharpScanner  = RoslynCSharpScanner.Instance,

			// §7.6. A grammar that is its own file maps onto itself; one written into an
			// attribute maps into the C# file holding it, which has to be searched for
			// rather than computed — see InlineLineMap.
			LineMap        = grammar.Path is { } path
				? new GrammarLineMap(text, path)
				: host.Literal is { } spelling && host.Location?.SourceTree is { } tree
					? new InlineLineMap(text, spelling, host.LiteralAt, tree)
					: null,
		});

		foreach (var diagnostic in result.Diagnostics)
			reports.Add(Report.Of(
				diagnostic, grammar.Path, text, host.Location, host.Literal, host.LiteralAt));

		return new Parser(
			result.Sources.Count > 0 ? host.HintName + ".g.cs" : null,
			result.Sources.Count > 0 ? result.Sources[0].Text  : null,
			Values(reports));
	}

	static EquatableArray<Report> Values(ImmutableArray<Report>.Builder reports) =>
		new(reports.ToImmutable());

	/// <summary>
	/// Works out which grammar a host means: the text written into the attribute, an
	/// explicit path, or — with no argument at all — the file named after the class.
	/// </summary>
	static bool TryResolveGrammar(
		ImmutableArray<Report>.Builder reports,
		Host                           host,
		ImmutableArray<GrammarFile>    files,
		out string                     text,
		out string?                    path)
	{
		text = "";
		path = null;

		// A single line ending in .gram is a path; anything else is the grammar itself.
		// The two are told apart exactly the way the attribute documents it, and a grammar
		// short enough to be mistaken for a path would not be a grammar.
		if (host.Source is { } source && !IsPath(source))
		{
			text = source;

			return true;
		}

		var wanted = host.Source ?? host.SimpleName + GramFileExtension;
		var found  = files.Where(file => Matches(file.Path, wanted)).ToImmutableArray();

		switch (found.Length)
		{
			case 1:
				text = found[0].Text;
				path = found[0].Path;

				return true;

			case 0:
				reports.Add(Report.Of(
					Diagnostics.GrammarFileNotFound, host.Location, wanted, host.ClassName));

				return false;

			default:
				// Picking one by reference order would make which file won invisible.
				reports.Add(Report.Of(
					Diagnostics.AmbiguousGrammarFile,
					host.Location,
					wanted,
					string.Join(", ", found.Select(file => file.Path))));

				return false;
		}
	}

	static bool IsPath(string source) =>
		source.EndsWith(GramFileExtension, StringComparison.OrdinalIgnoreCase) &&
		source.IndexOf('\n') < 0 &&
		source.IndexOf('\r') < 0;

	/// <summary>
	/// A file matches a wanted path when it ends with it on a separator boundary — the
	/// attribute names a path relative to the project, and what reaches us is absolute.
	/// </summary>
	static bool Matches(string filePath, string wanted)
	{
		var normalized = wanted.Replace('/', '\\');

		if (!filePath.Replace('/', '\\').EndsWith(normalized, StringComparison.OrdinalIgnoreCase))
			return false;

		var boundary = filePath.Length - normalized.Length - 1;

		return boundary < 0 || filePath[boundary] is '\\' or '/';
	}

	// ── What the shell carries between stages ────────────────────────────────────

	readonly record struct GrammarFile(string Path, string Text);

	/// <summary>
	/// A class marked <c>[Gram]</c>, reduced to what generation needs.
	/// </summary>
	/// <remarks>
	/// Deliberately values rather than symbols: an incremental generator compares what
	/// each step produced to decide whether the next one must run again, and a symbol
	/// compares equal to nothing across compilations.
	/// </remarks>
	/// <param name="Literal">
	/// The grammar exactly as the attribute spells it — quotes, escapes, indentation and
	/// all — or null when the grammar came from a file.
	/// </param>
	/// <param name="LiteralAt">
	/// Where that spelling begins in the C# file, so a position in the grammar can be put
	/// back where the author wrote it.
	/// </param>
	readonly record struct Host(
		string    ClassName,
		string?   Namespace,
		string    HintName,
		bool      IsPartial,
		string?   Source,
		Location? Location,
		string?   Literal    = null,
		int       LiteralAt  = 0)
	{
		/// <summary>
		/// The host as metadata names it, for looking its own members up.
		/// </summary>
		/// <remarks>
		/// Nested classes are joined by <c>+</c> and not by <c>.</c>, which is the whole
		/// difference between what a grammar writes and what a compilation is asked. Type
		/// parameters are not part of it — <c>Parser&lt;T&gt;</c> is metadata's
		/// <c>Parser`1</c> — so a generic host is left alone rather than looked up wrongly.
		/// </remarks>
		public string? MetadataName
		{
			get
			{
				if (ClassName.IndexOf('<') >= 0)
					return null;

				var nested = ClassName.Replace('.', '+');

				return Namespace is null ? nested : Namespace + "." + nested;
			}
		}

		/// <summary>
		/// The class the grammar is looked up by: the innermost one, without its type
		/// parameters — <c>Parser&lt;T&gt;</c> looks for <c>Parser.gram</c>.
		/// </summary>
		public string SimpleName
		{
			get
			{
				// The innermost class first, then its type parameters: a type parameter
				// list never contains a dot, but a nested name does.
				var name = ClassName;
				var dot  = name.LastIndexOf('.');

				if (dot >= 0)
					name = name.Substring(dot + 1);

				var angle = name.IndexOf('<');

				return angle < 0 ? name : name.Substring(0, angle);
			}
		}

		static string TypeParametersOf(ClassDeclarationSyntax declaration) =>
			declaration.TypeParameterList is { Parameters.Count: > 0 } parameters
				? "<" + string.Join(", ", parameters.Parameters.Select(static p => p.Identifier.ValueText)) + ">"
				: "";

		public static Host From(GeneratorAttributeSyntaxContext candidate)
		{
			var type        = (INamedTypeSymbol)candidate.TargetSymbol;
			var declaration = (ClassDeclarationSyntax)candidate.TargetNode;
			var attribute   = candidate.Attributes[0];

			var source = attribute.ConstructorArguments.Length == 1
				? attribute.ConstructorArguments[0].Value as string
				: null;

			// The literal as written, kept beside the value it decodes to. A diagnostic
			// carries an offset into the value; putting it where the author can see it
			// means finding that place in the spelling, and the spelling is the only thing
			// that knows where the escapes and the indentation went.
			var written = attribute.ApplicationSyntaxReference?.GetSyntax() is AttributeSyntax syntax &&
				syntax.ArgumentList?.Arguments is [{ Expression: LiteralExpressionSyntax spelled }]
					? spelled.Token
					: default;

			// A nested host is written back out nested, so every enclosing class has to be
			// partial too. Checking here says so at the class; leaving it says so at
			// generated code the author never wrote.
			var names     = new List<string>();
			var isPartial = true;

			for (var node = declaration; node is not null; node = node.Parent as ClassDeclarationSyntax)
			{
				// With the type parameters: a partial declaration has to name them the
				// same way, or it declares a different type. Their constraints do not
				// have to be repeated, and are not.
				names.Insert(0, node.Identifier.ValueText + TypeParametersOf(node));
				isPartial &= node.Modifiers.Any(modifier => modifier.ValueText == "partial");
			}

			return new Host(
				ClassName: string.Join(".", names),
				Namespace: type.ContainingNamespace.IsGlobalNamespace
					? null
					: type.ContainingNamespace.ToDisplayString(),
				HintName:  type.ToDisplayString().Replace('<', '_').Replace('>', '_'),
				IsPartial: isPartial,
				Source:    source,
				Location:  attribute.ApplicationSyntaxReference is { } reference
					? Microsoft.CodeAnalysis.Location.Create(reference.SyntaxTree, reference.Span)
					: declaration.Identifier.GetLocation(),
				Literal:   written == default ? null : written.Text,
				LiteralAt: written == default ? 0    : written.SpanStart);
		}
	}
}
