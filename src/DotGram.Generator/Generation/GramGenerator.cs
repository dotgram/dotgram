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
	const string GramFileExtension    = ".gram";
	const string GramAttribute        = "DotGram.GramAttribute";
	const string RuntimeAttribute     = "DotGram.GramRuntimeAttribute";
	const string SupportProbeTypeName = "DotGram.Diagnostic";

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		// Marker attributes: always internal, always present, so that [Gram] and
		// [assembly: GramRuntime] can be written in source at all.
		context.RegisterPostInitializationOutput(static postInit =>
		{
			var source = GramCompiler.EmitMarkerAttributes();

			postInit.AddSource(source.HintName, source.Text);
		});

		context.RegisterSourceOutput(
			context.CompilationProvider.Select(static (compilation, _) => DecideSupportEmission(compilation)),
			EmitSupportTypes);

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

		context.RegisterSourceOutput(
			hosts.Combine(files).Combine(context.CompilationProvider),
			static (production, input) => EmitParser(
				production, input.Left.Left, input.Left.Right, input.Right));
	}

	// ── Support types ────────────────────────────────────────────────────────────

	/// <summary>
	/// Three cases: another assembly already publishes the support types (bind to
	/// those), this assembly is the publisher (emit public), or neither (emit internal).
	/// </summary>
	static SupportEmission DecideSupportEmission(Compilation compilation)
	{
		var referenced = compilation
			.GetTypesByMetadataName(SupportProbeTypeName)
			.Where(type =>
				type.DeclaredAccessibility == Accessibility.Public &&
				!SymbolEqualityComparer.Default.Equals(type.ContainingAssembly, compilation.Assembly))
			.Select(type => type.ContainingAssembly.Name)
			.Distinct(StringComparer.Ordinal)
			.ToImmutableArray();

		if (referenced.Length > 0)
			return new SupportEmission(Mode: SupportMode.Referenced, Providers: referenced);

		var isPublisher = compilation.Assembly
			.GetAttributes()
			.Any(attribute => attribute.AttributeClass?.ToDisplayString() == RuntimeAttribute);

		return new SupportEmission(
			Mode:      isPublisher ? SupportMode.Publish : SupportMode.Private,
			Providers: ImmutableArray<string>.Empty);
	}

	static void EmitSupportTypes(SourceProductionContext context, SupportEmission emission)
	{
		switch (emission.Mode)
		{
			case SupportMode.Referenced when emission.Providers.Length > 1:
				// Silently picking one would make which assembly wins invisible.
				context.ReportDiagnostic(Diagnostic.Create(
					Diagnostics.AmbiguousSupportTypes,
					location: null,
					string.Join(", ", emission.Providers)));
				break;

			case SupportMode.Referenced:
				break;

			default:
				var source = GramCompiler.EmitSupportTypes(
					emission.Mode == SupportMode.Publish
						? SupportAccessibility.Public
						: SupportAccessibility.Internal);

				context.AddSource(source.HintName, source.Text);
				break;
		}
	}

	// ── Parsers ──────────────────────────────────────────────────────────────────

	static void EmitParser(
		SourceProductionContext        context,
		Host                           host,
		ImmutableArray<GrammarFile>    files,
		Compilation                    compilation)
	{
		if (!host.IsPartial)
		{
			context.ReportDiagnostic(Diagnostic.Create(
				Diagnostics.HostNotPartial, host.Location, host.ClassName));

			return;
		}

		if (!TryResolveGrammar(context, host, files, out var grammarText, out var grammarPath))
			return;

		var result = GramCompiler.Compile(grammarText, new GramCompilerOptions
		{
			FileName       = grammarPath ?? host.SimpleName + GramFileExtension,
			ClassName      = host.ClassName,
			Namespace      = host.Namespace,
			SymbolResolver = new RoslynSymbolResolver(compilation),
			CSharpScanner  = RoslynCSharpScanner.Instance,
		});

		foreach (var diagnostic in result.Diagnostics)
			context.ReportDiagnostic(Diagnostics.ToRoslyn(diagnostic, grammarPath, grammarText, host.Location));

		foreach (var source in result.Sources)
			context.AddSource(host.HintName + ".g.cs", source.Text);
	}

	/// <summary>
	/// Works out which grammar a host means: the text written into the attribute, an
	/// explicit path, or — with no argument at all — the file named after the class.
	/// </summary>
	static bool TryResolveGrammar(
		SourceProductionContext     context,
		Host                        host,
		ImmutableArray<GrammarFile> files,
		out string                  text,
		out string?                 path)
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
				context.ReportDiagnostic(Diagnostic.Create(
					Diagnostics.GrammarFileNotFound, host.Location, wanted, host.ClassName));

				return false;

			default:
				// Picking one by reference order would make which file won invisible.
				context.ReportDiagnostic(Diagnostic.Create(
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

	enum SupportMode
	{
		/// <summary>Emit internal copies; nothing crosses an assembly boundary.</summary>
		Private,

		/// <summary>Emit public copies; this assembly is the publisher.</summary>
		Publish,

		/// <summary>Bind to a referenced assembly's public copies.</summary>
		Referenced,
	}

	readonly record struct SupportEmission(SupportMode Mode, ImmutableArray<string> Providers);

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
		/// <summary>The class the grammar is looked up by — the innermost one.</summary>
		public string SimpleName
		{
			get
			{
				var dot = ClassName.LastIndexOf('.');

				return dot < 0 ? ClassName : ClassName.Substring(dot + 1);
			}
		}

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
				names.Insert(0, node.Identifier.ValueText);
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
