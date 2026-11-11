using System;
using System.Collections.Concurrent;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace DotGram.Generation;

/// <summary>Diagnostics the shell reports, and conversion of the grammar half's own.</summary>
static class Diagnostics
{
	const string Category = "DotGram";

	/// <summary>
	/// The final safety net for a defect reached through grammar input. Kept out of the
	/// public language specification: this is an internal failure, not language behavior.
	/// </summary>
	public static readonly DiagnosticDescriptor InternalFailure = new(
		id:                 "GRAM0001",
		title:              "The generator itself failed",
		messageFormat:      "The generator failed during {0}: {1}: {2}",
		category:           Category,
		defaultSeverity:    DiagnosticSeverity.Error,
		isEnabledByDefault: true,
		helpLinkUri:        HelpLink("GRAM0001"));

	public static readonly DiagnosticDescriptor HostNotPartial = new(
		id:                 "GRAM0002",
		title:              "A class hosting a grammar must be partial",
		messageFormat:      "'{0}' is marked [Gram] but is not partial; the generated parser has nowhere to go",
		category:           Category,
		defaultSeverity:    DiagnosticSeverity.Error,
		isEnabledByDefault: true,
		helpLinkUri:        HelpLink("GRAM0002"),
		description:        "A nested host needs every enclosing class to be partial as well.");

	public static readonly DiagnosticDescriptor GrammarFileNotFound = new(
		id:                 "GRAM0003",
		title:              "No grammar file for a [Gram] class",
		messageFormat:      "No additional file '{0}' for '{1}'; add <AdditionalFiles Include=\"{0}\" /> to the project",
		category:           Category,
		defaultSeverity:    DiagnosticSeverity.Error,
		isEnabledByDefault: true,
		helpLinkUri:        HelpLink("GRAM0003"),
		description:        "A .gram file is only visible to the generator when the project lists it as an additional file. A grammar included from another project is found on the class instead, which requires that project to have exported it ([Gram(ExportGrammar = true)]) — the default for a host that is public, with every class around it.");

	public static readonly DiagnosticDescriptor AmbiguousGrammarFile = new(
		id:                 "GRAM0004",
		title:              "More than one grammar file matches",
		messageFormat:      "'{0}' matches several additional files ({1}); give [Gram] a path that names one of them",
		category:           Category,
		defaultSeverity:    DiagnosticSeverity.Error,
		isEnabledByDefault: true,
		helpLinkUri:        HelpLink("GRAM0004"),
		description:        "Which file a grammar comes from must be unambiguous, not decided silently by order.");

	public static readonly DiagnosticDescriptor InvalidIncludedName = new(
		id:                 "GRAM0005",
		title:              "The name a grammar is included under is not an identifier",
		messageFormat:      "'{0}' cannot be included as '{1}'; that has to be one identifier",
		category:           Category,
		defaultSeverity:    DiagnosticSeverity.Error,
		isEnabledByDefault: true,
		helpLinkUri:        HelpLink("GRAM0005"),
		description:        "A grammar is included by being wrapped in a namespace, and a namespace is named by an identifier. A dotted name would mean nesting, which is written in the grammar itself.");

	public static readonly DiagnosticDescriptor RepeatedGrammarScope = new(
		id:                 "GRAM0006",
		title:              "Two readings of one grammar want the same scope",
		messageFormat:      "'{0}' asks for two readings of its grammar under the same Name; give one of them a Name of its own",
		category:           Category,
		defaultSeverity:    DiagnosticSeverity.Error,
		isEnabledByDefault: true,
		helpLinkUri:        HelpLink("GRAM0006"),
		description:        "The [Gram] and every [GramParserVariant] on a class is a compilation of its own and needs a scope of its own: the [Gram] goes in the class itself, and every [GramParserVariant] names a nested class, its one argument.");

	public static readonly DiagnosticDescriptor RepeatedIncludedName = new(
		id:                 "GRAM0008",
		title:              "Two grammars are included under one name",
		messageFormat:      "'{0}' includes {1} under the same name '{2}'; give all but one an 'Alias' of its own",
		category:           Category,
		defaultSeverity:    DiagnosticSeverity.Error,
		isEnabledByDefault: true,
		helpLinkUri:        HelpLink("GRAM0008"),
		description:        "An included grammar is wrapped in a namespace named after it, which is what keeps two grammars' rules from colliding: 'Sql92.Identifier' and 'Lex.Identifier' are different names. Two under one name are one namespace, and their rules do collide.");

	public static readonly DiagnosticDescriptor InvalidGrammarScope = new(
		id:                 "GRAM0007",
		title:              "The name a grammar is compiled under is not an identifier",
		messageFormat:      "'{0}' cannot be compiled into '{1}'; that has to be one identifier",
		category:           Category,
		defaultSeverity:    DiagnosticSeverity.Error,
		isEnabledByDefault: true,
		helpLinkUri:        HelpLink("GRAM0007"),
		description:        "A [GramParserVariant] names a nested class, as [Gram(Name = …)] does, and a class is named by an identifier, never an empty one.");

	public static readonly DiagnosticDescriptor BufferOptionNotPositive = new(
		id:                 "GRAM0009",
		title:              "A buffer option is not positive",
		messageFormat:      "{0} = {1} reads no input; it has to be at least 1",
		category:           Category,
		defaultSeverity:    DiagnosticSeverity.Error,
		isEnabledByDefault: true,
		helpLinkUri:        HelpLink("GRAM0009"),
		description:        "MaxBufferedInputLength is the most input a buffered parse may hold and InitialBufferSize the capacity it starts with; neither can be zero or negative.");

	/// <summary>
	/// A location type the generated code cannot tell a place through: said once, on the attribute,
	/// rather than as a compiler error in a file the author never wrote.
	/// </summary>
	public static readonly DiagnosticDescriptor LocationTypeNotLocatable = new(
		id:                 "GRAM0011",
		title:              "The location type is not an interface declaring Locate",
		messageFormat:      "'{0}' names '{1}' as its LocationType, which is not an interface declaring {2}",
		category:           Category,
		defaultSeverity:    DiagnosticSeverity.Error,
		isEnabledByDefault: true,
		helpLinkUri:        HelpLink("GRAM0011"),
		description:        "A rule whose value implements the LocationType is told where it was written through that interface: void Locate(int at, int length), or with UsePerCallLocations void Locate(int at, int length, int gapStart). It is called through the interface, so an explicit implementation serves.");

	/// <summary>
	/// A grammar a referenced assembly carries is the grammar it was compiled from; a local file of
	/// the same name is not read in its place, and is told of where it says something else.
	/// </summary>
	public static readonly DiagnosticDescriptor LocalFileNotReferencedGrammar = new(
		id:                 "GRAM0017",
		title:              "A local grammar file is not the grammar a referenced assembly carries",
		messageFormat:      "'{0}' in this project is not the grammar of the referenced '{1}'; the grammar the assembly carries is used",
		category:           Category,
		defaultSeverity:    DiagnosticSeverity.Info,
		isEnabledByDefault: true,
		helpLinkUri:        HelpLink("GRAM0017"),
		description:        "A class from a referenced assembly that carries its grammar ([GramSource]) is always included with that grammar: it is the one the assembly was compiled from. A .gram file of the same name in this project is somebody else's — a fixture, a copy — and is not read in its place.");

	/// <summary>
	/// What a generated parser is written in is C# 8 at the least, and a project compiling an
	/// earlier version would otherwise meet that as a wall of compiler errors in files it never
	/// wrote.
	/// </summary>
	public static readonly DiagnosticDescriptor LanguageVersionTooLow = new(
		id:                 "GRAM0010",
		title:              "The generated parser needs C# 8 or later",
		messageFormat:      "The generated parser needs C# 8 or later; this project compiles C# {0}. Set <LangVersion>8.0</LangVersion> or later.",
		category:           Category,
		defaultSeverity:    DiagnosticSeverity.Error,
		isEnabledByDefault: true,
		helpLinkUri:        HelpLink("GRAM0010"),
		description:        "C# 7.3 is the default for netstandard2.0 and .NET Framework projects. No parser is generated below C# 8; the attributes [Gram] is written with still are, so a project without a [Gram] builds as it did.");

	/// <summary>
	/// The descriptor for an id, made once and kept.
	/// </summary>
	/// <remarks>
	/// The grammar half reports positionally and knows nothing about
	/// <see cref="Diagnostic"/>; its ids arrive here and become rules an IDE can show.
	/// Kept rather than remade because two descriptors for one id are two rules as far as
	/// Roslyn is concerned.
	/// </remarks>
	public static DiagnosticDescriptor DescriptorFor(
		string id, string title, string messageFormat, DiagnosticSeverity severity)
	{
		return _descriptors.GetOrAdd(
			id,
			_ => new DiagnosticDescriptor(
				id: id,
				title: title,
				messageFormat: messageFormat,
				category: Category,
				defaultSeverity: severity,
				isEnabledByDefault: true,
				helpLinkUri: HelpLink(id)));
	}

	/// <summary>
	/// Where an identifier is described: its row of <c>docs/diagnostics.md</c> at the tag of the
	/// version that reports it, so that a link never shows a later meaning of the number.
	/// </summary>
	public static string HelpLink(string id)
	{
		var version = typeof(Diagnostics).Assembly
			.GetCustomAttributes(typeof(System.Reflection.AssemblyInformationalVersionAttribute), false)
			.OfType<System.Reflection.AssemblyInformationalVersionAttribute>()
			.FirstOrDefault()?.InformationalVersion ?? "";
		var plus    = version.IndexOf('+');

		if (plus >= 0)
			version = version.Substring(0, plus);

		return "https://github.com/dotgram/dotgram/blob/v" + version + "/docs/diagnostics.md#" + id.ToLowerInvariant();
	}

	/// <summary>
	/// Turns an offset span into lines and columns.
	/// </summary>
	/// <remarks>
	/// Roslyn does not read the additional file to work this out — it believes whatever
	/// it is told. Told nothing, it points every grammar message at line 1, which is the
	/// difference between a diagnostic and a diagnostic that helps.
	/// </remarks>
	public static LinePositionSpan LinesOf(string text, TextSpan span)
	{
		var line   = 0;
		var start  = 0;
		var result = default(LinePosition);

		for (var i = 0; i <= text.Length; i++)
		{
			if (i == span.Start)
				result = new LinePosition(line, i - start);

			if (i == span.End)
				return new LinePositionSpan(result, new LinePosition(line, i - start));

			if (i < text.Length && text[i] == '\n')
			{
				line++;
				start = i + 1;
			}
		}

		return new LinePositionSpan(result, result);
	}

	static readonly ConcurrentDictionary<string, DiagnosticDescriptor> _descriptors = [];
}
