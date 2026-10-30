using System;
using System.Linq;

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
/// <param name="IncludedAs">The name an including grammar reaches this one under; null for the class's name.</param>
/// <param name="PartSize">
/// Zero for "nothing was said", which is also what a consumer writing <c>PartSize = 0</c>
/// means: the emitter reads it as no wish and takes its default.
/// </param>
/// <param name="Lexical">
/// A request and not a setting: a grammar that cannot be cut in two is compiled over
/// characters and told why (GRAM5004), so nothing written here fails a build.
/// </param>
/// <param name="Portable">
/// Whether the class carries its grammar across an assembly reference. What the class already
/// says unless the attribute says otherwise: a host nobody outside can name is a host nobody
/// outside can include.
/// </param>
/// <param name="LocationType">The location type's display name, or null where none was named.</param>
/// <param name="Carrier">Which carrier the author chose; nought is the generator's own choice.</param>
/// <param name="Stacks">
/// How many stacks a parse may take past the one it began on. Nought is as many as there is
/// memory for, which is the default.
/// </param>
/// <param name="Suffix">
/// Which nested class this compilation goes into, where the host has more than one grammar.
/// Null is the host class itself, which one of them may be.
/// </param>
/// <param name="ConstructionsMayThrow">
/// What the author says of the grammar's constructions, which the immediate carrier pays for
/// on every one of them and so does not assume.
/// </param>
/// <param name="Trace">A trace build of this reading.</param>
sealed record GramHostOptions(
	string? IncludedAs,
	int     PartSize,
	bool    Lexical,
	bool    Portable,
	string? LocationType,
	int     Carrier,
	int     Stacks,
	string? Suffix,
	bool    ConstructionsMayThrow,
	bool    SpanCaptures,
	bool    BufferedBytes,
	bool    BufferedInput,
	int     MaxRetained,
	int     BufferSize,
	bool    PerCall,
	bool    Trace)
{
	/// <summary>One reading's options.</summary>
	/// <param name="attribute">The <c>[Gram]</c> or <c>[GramOptions]</c> read.</param>
	/// <param name="type">The class it is written on, whose visibility is the default for <see cref="Portable"/>.</param>
	/// <param name="first">
	/// The options of the class's first reading, which a later one takes everything it does not
	/// say from; null for the first itself, or where nothing is to be inherited.
	/// </param>
	public static GramHostOptions Read(AttributeData attribute, INamedTypeSymbol type, GramHostOptions? first)
	{
		return new GramHostOptions(
			IncludedAs:            Named(attribute, nameof(IncludedAs)) as string,
			PartSize:              Named(attribute, nameof(PartSize)) as int? ?? first?.PartSize ?? 0,
			Lexical:               Named(attribute, nameof(Lexical)) as bool? ?? first?.Lexical ?? false,
			Portable:              Named(attribute, nameof(Portable)) as bool? ?? first?.Portable ?? Visible(type),
			LocationType:          (Named(attribute, nameof(LocationType)) as INamedTypeSymbol)?.ToDisplayString() ?? first?.LocationType,
			Carrier:               Named(attribute, nameof(Carrier)) as int? ?? first?.Carrier ?? 0,
			Stacks:                Named(attribute, nameof(Stacks)) as int? ?? first?.Stacks ?? 0,
			Suffix:                Named(attribute, nameof(Suffix)) as string,
			ConstructionsMayThrow: Named(attribute, nameof(ConstructionsMayThrow)) as bool? ?? first?.ConstructionsMayThrow ?? false,
			SpanCaptures:          Named(attribute, nameof(SpanCaptures)) as bool? ?? first?.SpanCaptures ?? false,
			BufferedBytes:         Named(attribute, nameof(BufferedBytes)) as bool? ?? first?.BufferedBytes ?? false,
			BufferedInput:         Named(attribute, nameof(BufferedInput)) as bool? ?? first?.BufferedInput ?? false,
			MaxRetained:           Named(attribute, nameof(MaxRetained)) as int? ?? first?.MaxRetained ?? int.MaxValue,
			BufferSize:            Named(attribute, nameof(BufferSize)) as int? ?? first?.BufferSize ?? 4096,
			PerCall:               Named(attribute, nameof(PerCall)) as bool? ?? false,
			Trace:                 Named(attribute, nameof(Trace)) as bool? ?? first?.Trace ?? false);
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
