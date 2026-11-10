using System;
using System.Linq;

using DotGram.Grammar;

using Microsoft.CodeAnalysis;

namespace DotGram.Language;

/// <summary>
/// What one reading of a grammar — a <c>[Gram]</c> or a <c>[GramOptions]</c> — asks of the
/// compilation, as its named arguments say it, with what it does not say taken from the
/// class's first reading.
/// </summary>
/// <remarks>
/// <para>
/// The one reader of those arguments, for the generator and for the editor alike: a grammar
/// the editor analysed with other options than the build compiles it with would be shown
/// diagnostics the build does not report, and the other way round. So the editor does not
/// look an argument up itself; it asks this, as the generator does.
/// </para>
/// <para>
/// Read by name, like everything else about the attributes: each assembly carries its own
/// copy of them, and what the copies share is what their members are called. An enum
/// constant arrives as its underlying integer, and a <c>typeof(…)</c> as the symbol it named.
/// </para>
/// <para>
/// <see cref="IncludedAs"/>, <see cref="Suffix"/> and <see cref="PerCall"/> are the reading's
/// own and are not inherited: each says which reading this is rather than how it compiles.
/// </para>
/// </remarks>
sealed record GramHostOptions
{
	/// <summary>The name an including grammar reaches this one under; null for the class's name.</summary>
	public string? IncludedAs { get; init; }

	/// <summary>
	/// How large a divided recognizer's parts should be aimed to be.
	/// Zero for "nothing was said", which is also what a consumer writing <c>PartSize = 0</c>
	/// means: the emitter reads it as no wish and takes its default.
	/// </summary>
	public int PartSize { get; init; }

	/// <summary>
	/// Whether the grammar is read as tokens. A request and not a setting: a grammar that cannot be cut in two is compiled over
	/// characters and told why (GRAM5004), so nothing written here fails a build.
	/// </summary>
	public bool Lexical { get; init; }

	/// <summary>
	/// Whether the class carries its grammar across an assembly reference. What the class already
	/// says unless the attribute says otherwise: a host nobody outside can name is a host nobody
	/// outside can include.
	/// </summary>
	public bool Portable { get; init; }

	/// <summary>The location type's display name, or null where none was named.</summary>
	public string? LocationType { get; init; }

	/// <summary>Which carrier the author chose; nought is the generator's own choice.</summary>
	public int Carrier { get; init; }

	/// <summary>
	/// How many stacks a parse may take past the one it began on. Nought is as many as there is
	/// memory for, which is the default.
	/// </summary>
	public int Stacks { get; init; }

	/// <summary>
	/// Which nested class this compilation goes into, where the host has more than one grammar.
	/// Null is the host class itself, which one of them may be.
	/// </summary>
	public string? Suffix { get; init; }

	/// <summary>
	/// What the author says of the grammar's constructions, which the immediate carrier pays for
	/// on every one of them and so does not assume.
	/// </summary>
	public bool ConstructionsMayThrow { get; init; }

	/// <summary>Whether text captures are spans of the input.</summary>
	public bool SpanCaptures { get; init; }

	/// <summary>Whether the reading is also compiled over a stream of bytes.</summary>
	public bool BufferedBytes { get; init; }

	/// <summary>Whether the reading is also compiled over a reader.</summary>
	public bool BufferedInput { get; init; }

	/// <summary>What a buffered parse retains at most where a call does not say.</summary>
	public int MaxRetained { get; init; } = int.MaxValue;

	/// <summary>A buffered parse's initial capacity where a call does not say.</summary>
	public int BufferSize { get; init; } = 4096;

	/// <summary>Whether this reading is the per-call facade of the first: where its values were written is decided per call.</summary>
	public bool PerCall { get; init; }

	/// <summary>A trace build of this reading.</summary>
	public bool Trace { get; init; }

	/// <summary>One reading's options.</summary>
	/// <param name="attribute">The <c>[Gram]</c> or <c>[GramOptions]</c> read.</param>
	/// <param name="type">The class it is written on, whose visibility is the default for <see cref="Portable"/>.</param>
	/// <param name="first">
	/// The options of the class's first reading, which a later one takes everything it does not
	/// say from; null for the first itself, or where nothing is to be inherited.
	/// </param>
	public static GramHostOptions Read(AttributeData attribute, INamedTypeSymbol type, GramHostOptions? first)
	{
		return new GramHostOptions
		{
			IncludedAs            = Named(attribute, nameof(IncludedAs)) as string,
			PartSize              = Named(attribute, nameof(PartSize)) as int? ?? first?.PartSize ?? 0,
			Lexical               = Named(attribute, nameof(Lexical)) as bool? ?? first?.Lexical ?? false,
			Portable              = Named(attribute, nameof(Portable)) as bool? ?? first?.Portable ?? Visible(type),
			LocationType          = (Named(attribute, nameof(LocationType)) as INamedTypeSymbol)?.ToDisplayString() ?? first?.LocationType,
			Carrier               = Named(attribute, nameof(Carrier)) as int? ?? first?.Carrier ?? 0,
			Stacks                = Named(attribute, nameof(Stacks)) as int? ?? first?.Stacks ?? 0,
			Suffix                = Named(attribute, nameof(Suffix)) as string,
			ConstructionsMayThrow = Named(attribute, nameof(ConstructionsMayThrow)) as bool? ?? first?.ConstructionsMayThrow ?? false,
			SpanCaptures          = Named(attribute, nameof(SpanCaptures)) as bool? ?? first?.SpanCaptures ?? false,
			BufferedBytes         = Named(attribute, nameof(BufferedBytes)) as bool? ?? first?.BufferedBytes ?? false,
			BufferedInput         = Named(attribute, nameof(BufferedInput)) as bool? ?? first?.BufferedInput ?? false,
			MaxRetained           = Named(attribute, nameof(MaxRetained)) as int? ?? first?.MaxRetained ?? int.MaxValue,
			BufferSize            = Named(attribute, nameof(BufferSize)) as int? ?? first?.BufferSize ?? 4096,
			PerCall               = Named(attribute, nameof(PerCall)) as bool? ?? false,
			Trace                 = Named(attribute, nameof(Trace)) as bool? ?? first?.Trace ?? false,
		};
	}

	/// <summary>
	/// Hands this reading's options to a compilation: the one mapping of them, which the generator
	/// compiles with and the editor analyses with.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Not the location type, which the generator gives the reading whose class holds the per-call
	/// facade and the editor gives none: what it changes rests on which C# types implement it.
	/// Not <see cref="IncludedAs"/> or <see cref="PerCall"/>, which say which reading this is and
	/// not how it compiles.
	/// </para>
	/// <para>
	/// <see cref="Trace"/> is this reading's; a build that asks for traces everywhere
	/// (<c>DotGramTrace</c>) adds that to it.
	/// </para>
	/// </remarks>
	public void ApplyTo(GramCompilerOptions options)
	{
		if (options is null)
			throw new ArgumentNullException(nameof(options));

		// Nought is what the attribute holds when nobody set it, and what somebody setting it to
		// nought means: take the measured default either way.
		options.PartSize              = PartSize == 0 ? null : PartSize;
		options.Lexical               = Lexical;
		options.Portable              = Portable;
		options.Carrier               = (CarrierKind)Carrier;
		options.Stacks                = Stacks;
		options.Suffix                = Suffix;
		options.ConstructionsMayThrow = ConstructionsMayThrow;
		options.SpanCaptures          = SpanCaptures;
		options.BufferedBytes         = BufferedBytes;
		options.BufferedInput         = BufferedInput;
		options.MaxRetained           = MaxRetained;
		options.BufferSize            = BufferSize;
		options.Trace                 = Trace;
	}

	/// <summary>Whether anything outside this assembly could name the class.</summary>
	public static bool Visible(INamedTypeSymbol type)
	{
		for (var at = (ITypeSymbol?)type; at is not null; at = at.ContainingType)
			if (at.DeclaredAccessibility != Accessibility.Public)
				return false;

		return true;
	}

	static object? Named(AttributeData attribute, string name)
	{
		return attribute.NamedArguments.FirstOrDefault(named => named.Key == name).Value.Value;
	}
}
