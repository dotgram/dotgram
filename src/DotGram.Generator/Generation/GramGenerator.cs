using System;
using System.Collections.Immutable;
using System.Linq;

using DotGram.Grammar;

using Microsoft.CodeAnalysis;

namespace DotGram.Generation;

/// <summary>
/// Roslyn shell over <see cref="GramCompiler"/>.
/// </summary>
/// <remarks>
/// Everything here is Roslyn-specific: pull grammars out of the compilation, decide
/// accessibility from what it references, convert diagnostics on the way back.
/// Compilation itself lives in <see cref="DotGram.Grammar"/> and is callable — and
/// testable — without any of this.
/// </remarks>
[Generator(LanguageNames.CSharp)]
public sealed class GramGenerator : IIncrementalGenerator
{
	const string GramFileExtension    = ".gram";
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

		// Grammar files supplied as <AdditionalFiles Include="*.gram" />.
		var grammars = context.AdditionalTextsProvider
			.Where(static file => file.Path.EndsWith(GramFileExtension, StringComparison.OrdinalIgnoreCase))
			.Select(static (file, cancellationToken) => new GrammarSource(
				Path: file.Path,
				Text: file.GetText(cancellationToken)?.ToString() ?? string.Empty))
			.Collect();

		context.RegisterSourceOutput(context.CompilationProvider.Combine(grammars), EmitParsers);
	}

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

	static void EmitParsers(SourceProductionContext context, (Compilation Compilation, ImmutableArray<GrammarSource> Grammars) input)
	{
		if (input.Grammars.IsEmpty)
			return;

		var symbolResolver = new RoslynSymbolResolver(input.Compilation);

		foreach (var grammar in input.Grammars)
		{
			var options = new GramCompilerOptions
			{
				FileName       = grammar.Path,
				SymbolResolver = symbolResolver,
				CSharpScanner  = RoslynCSharpScanner.Instance,
			};

			var result = GramCompiler.Compile(grammar.Text, options);

			foreach (var diagnostic in result.Diagnostics)
				context.ReportDiagnostic(Diagnostics.ToRoslyn(diagnostic, grammar.Path));

			foreach (var source in result.Sources)
				context.AddSource(source.HintName, source.Text);
		}
	}

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

	readonly record struct GrammarSource(string Path, string Text);
}
