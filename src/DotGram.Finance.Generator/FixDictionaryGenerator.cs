using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

using DotGram.Finance.Fix;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace DotGram.Finance.Generator;

/// <summary>
/// Compiles a FIX dictionary into validation code inside the consumer's own compilation.
/// </summary>
/// <remarks>
/// <para>
/// A dictionary named by <c>[FixDictionaryFile]</c> and listed as an <c>AdditionalFiles</c> item
/// becomes a rule per message type, a method per component and a method per repeating group, and a
/// <c>FixValidator</c> holding them. The path is fixed at build time and is never read at run time,
/// which is D25 kept by construction rather than by care.
/// </para>
/// <para>
/// The shape emitted is D74's: a method per component, called, rather than a component expanded
/// into every message that names it. Unrolling costs 2.16× the code and, measured, buys nothing.
/// </para>
/// </remarks>
[Generator(LanguageNames.CSharp)]
public sealed class FixDictionaryGenerator : IIncrementalGenerator
{
	const string Attribute = "DotGram.Finance.Fix.FixDictionaryFileAttribute";

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		context.RegisterPostInitializationOutput(static to =>
			to.AddSource("FixDictionaryFileAttribute.g.cs", SourceText.From(AttributeSource, Encoding.UTF8)));

		var marked = context.SyntaxProvider
			.ForAttributeWithMetadataName(
				Attribute,
				static (node, _) => node is ClassDeclarationSyntax { } declaration &&
					declaration.Modifiers.Any(SyntaxKind.PartialKeyword),
				static (found, _) => Asked(found))
			.Where(static asked => asked is not null)
			.Select(static (asked, _) => asked!);

		var files = context.AdditionalTextsProvider.Collect();

		context.RegisterSourceOutput(marked.Combine(files), static (to, pair) => Write(to, pair.Left, pair.Right));
	}

	/// <summary>What one marked class asks for: where to write it, and which file it names.</summary>
	sealed class Request(string space, string name, string accessibility, string file, Location at)
	{
		public readonly string   Space         = space;
		public readonly string   Name          = name;
		public readonly string   Accessibility = accessibility;
		public readonly string   File          = file;
		public readonly Location At            = at;
	}

	static Request? Asked(GeneratorAttributeSyntaxContext found)
	{
		if (found.TargetSymbol is not INamedTypeSymbol symbol)
			return null;

		var file = found.Attributes[0].ConstructorArguments is [{ Value: string named }, ..] ? named : null;

		if (string.IsNullOrWhiteSpace(file))
			return null;

		var space = symbol.ContainingNamespace.IsGlobalNamespace
			? ""
			: symbol.ContainingNamespace.ToDisplayString();

		return new Request(
			space,
			symbol.Name,
			symbol.DeclaredAccessibility == Accessibility.Public ? "public" : "internal",
			file!,
			found.TargetNode.GetLocation());
	}

	static void Write(SourceProductionContext to, Request asked, ImmutableArray<AdditionalText> files)
	{
		var file = Find(files, asked.File);

		if (file is null)
		{
			to.ReportDiagnostic(Diagnostic.Create(NoFile, asked.At, asked.File));

			return;
		}

		var text = file.GetText(to.CancellationToken)?.ToString();

		if (text is null)
		{
			to.ReportDiagnostic(Diagnostic.Create(NoFile, asked.At, asked.File));

			return;
		}

		FixDictionary dictionary;

		try
		{
			dictionary = FixDictionary.Parse(text);
		}
		catch (FormatException refused)
		{
			// The element, the line and the tag, which is what a reader needs to mend their own
			// file. Quoting it is allowed — the file is theirs — but naming the place is more use.
			to.ReportDiagnostic(Diagnostic.Create(NotADictionary, asked.At, asked.File, refused.Message));

			return;
		}

		to.AddSource(
			(asked.Space.Length == 0 ? "" : asked.Space + ".") + asked.Name + ".g.cs",
			SourceText.From(Emitter.Rules(asked.Space, asked.Name, asked.Accessibility, dictionary), Encoding.UTF8));
	}

	// A path as the attribute spells it, matched against what the build listed: the same file is
	// written "FIX44.xml", "dict/FIX44.xml" and as an absolute path, and all three mean it.
	static AdditionalText? Find(ImmutableArray<AdditionalText> files, string named)
	{
		var wanted = named.Replace('\\', '/');

		foreach (var file in files)
		{
			var path = file.Path.Replace('\\', '/');

			if (path.Equals(wanted, StringComparison.OrdinalIgnoreCase) ||
				path.EndsWith("/" + wanted, StringComparison.OrdinalIgnoreCase) ||
				Path.GetFileName(path).Equals(wanted, StringComparison.OrdinalIgnoreCase))
				return file;
		}

		return null;
	}

	static readonly DiagnosticDescriptor NoFile = new(
		"FIXGEN001",
		"The dictionary is not an additional file",
		"The dictionary '{0}' is not among the build's AdditionalFiles. Add " +
			"<AdditionalFiles Include=\"{0}\" /> to the project so that the compiler can read it.",
		"DotGram.Finance",
		DiagnosticSeverity.Error,
		isEnabledByDefault: true);

	static readonly DiagnosticDescriptor NotADictionary = new(
		"FIXGEN002",
		"The file is not a dictionary this reader accepts",
		"The dictionary '{0}' could not be read: {1}",
		"DotGram.Finance",
		DiagnosticSeverity.Error,
		isEnabledByDefault: true);

	const string AttributeSource =
		"""
		// <auto-generated/>
		#nullable enable

		namespace DotGram.Finance.Fix
		{
			/// <summary>
			/// Fills a partial class with the validation a FIX dictionary describes.
			/// </summary>
			/// <remarks>
			/// The file is named here and listed as an <c>AdditionalFiles</c> item, so the compiler
			/// reads it and nothing reads a path at run time. The class is given a
			/// <c>Validator</c> ready to use and an <c>Entries</c> table beside it, for mixing two
			/// dictionaries into one validator.
			/// </remarks>
			[global::System.AttributeUsage(global::System.AttributeTargets.Class)]
			[global::System.Diagnostics.Conditional("DOTGRAM_FINANCE_GENERATOR_KEEPS_ATTRIBUTES")]
			internal sealed class FixDictionaryFileAttribute : global::System.Attribute
			{
				/// <param name="file">
				/// The dictionary, as the project lists it in <c>AdditionalFiles</c>.
				/// </param>
				public FixDictionaryFileAttribute(string file) => File = file;

				/// <summary>The dictionary this class is filled from.</summary>
				public string File { get; }
			}
		}
		""";
}
