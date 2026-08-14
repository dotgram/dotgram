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

		// Compiling in a Select and not in the output step, which is the whole of what
		// makes this incremental. The transform re-runs whenever the Compilation changes,
		// which is on every keystroke — it cannot not, since resolving `@Name` needs the
		// compilation — but what it *produces* is text and reports, both compared by
		// value. An edit that changes neither leaves the output step Cached, and the
		// consumer's IDE is not handed a new syntax tree to parse and bind.
		var parsers = hosts
			.Combine(files)
			.Combine(context.CompilationProvider)
			.Select(static (input, _) => Compile(input.Left.Left, input.Left.Right, input.Right));

		context.RegisterSourceOutput(parsers, static (production, parser) => parser.Deliver(production));
	}

	// ── Parsers ──────────────────────────────────────────────────────────────────

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
				context.ReportDiagnostic(report.ToRoslyn());

			if (HintName is not null && Text is not null)
				context.AddSource(HintName, Text);
		}
	}

	static Parser Compile(Host host, ImmutableArray<GrammarFile> files, Compilation compilation)
	{
		var reports = ImmutableArray.CreateBuilder<Report>();

		if (!host.IsPartial)
		{
			reports.Add(Report.Of(Diagnostics.HostNotPartial, host.Location, host.ClassName));

			return new Parser(null, null, new EquatableArray<Report>(reports.ToImmutable()));
		}

		if (!TryResolveGrammar(reports, host, files, out var grammarText, out var grammarPath))
			return new Parser(null, null, new EquatableArray<Report>(reports.ToImmutable()));

		var result = GramCompiler.Compile(grammarText, new GramCompilerOptions
		{
			FileName       = grammarPath ?? host.SimpleName + GramFileExtension,
			ClassName      = host.ClassName,
			Namespace      = host.Namespace,
			SymbolResolver = new RoslynSymbolResolver(compilation),
			CSharpScanner  = RoslynCSharpScanner.Instance,
		});

		foreach (var diagnostic in result.Diagnostics)
			reports.Add(Report.Of(diagnostic, grammarPath, grammarText, host.Location));

		return new Parser(
			result.Sources.Count > 0 ? host.HintName + ".g.cs" : null,
			result.Sources.Count > 0 ? result.Sources[0].Text  : null,
			new EquatableArray<Report>(reports.ToImmutable()));
	}

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
	readonly record struct Host(
		string    ClassName,
		string?   Namespace,
		string    HintName,
		bool      IsPartial,
		string?   Source,
		Location? Location)
	{
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
					: declaration.Identifier.GetLocation());
		}
	}
}
