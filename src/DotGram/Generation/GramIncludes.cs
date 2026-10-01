using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;

namespace DotGram.Generation;

/// <summary>One grammar a host is built on, as the attributes on its class say.</summary>
/// <param name="Type">The class whose grammar it is.</param>
/// <param name="Attribute">That class's own <c>[Gram]</c>: the reading without a <c>Suffix</c>.</param>
/// <param name="Name">What the including grammar reaches it under: the includer's <c>As</c>, its <c>IncludedAs</c>, or its class name.</param>
/// <param name="Source">The attribute's argument unresolved — a path or the text — or null where it has none.</param>
/// <param name="Portable">What the class carries in <c>[GramSource]</c>, which crosses an assembly reference.</param>
readonly record struct GramInclude(
	INamedTypeSymbol Type,
	AttributeData    Attribute,
	string           Name,
	string?          Source,
	string?          Portable);

/// <summary>
/// Which grammars a host is built on, and which of a class's attributes is its grammar: the one
/// walk, read by the generator and by the editor alike.
/// </summary>
/// <remarks>
/// <para>
/// By display name and not by symbol: the attribute is emitted into every assembly
/// separately and on purpose, so a base compiled elsewhere carries *its* assembly's
/// <c>DotGram.GramAttribute</c> and the two types are not the same type. What they
/// share is what they are called.
/// </para>
/// <para>
/// A base with no grammar is walked past rather than stopping the walk: a class may
/// sit between two that have one for reasons of its own.
/// </para>
/// <para>
/// Two spellings, and both are walked. A base class is the older one and says less: a
/// class has one base and as many attributes as it likes, and a base carries meaning
/// of its own that a grammar has no use for. <c>[GramInclude(typeof(X))]</c> is the
/// other, it may be written many times, and what it names is walked in turn — a
/// grammar built on a standard built on a library gets all three.
/// </para>
/// <para>
/// Named rather than inherited, cycles become possible, and they are ended rather
/// than reported: a grammar already gathered is not gathered twice, so
/// <c>A</c> naming <c>B</c> naming <c>A</c> splices each of them once, which is what
/// anybody writing it meant. Nothing is lost by not saying so.
/// </para>
/// </remarks>
static class GramIncludes
{
	public const string GramFileExtension    = ".gram";
	public const string GramAttribute        = "DotGram.GramAttribute";
	public const string GramIncludeAttribute = "DotGram.GramIncludeAttribute";
	public const string GramSourceAttribute  = "DotGram.GramSourceAttribute";

	/// <summary>Every grammar <paramref name="type"/> is built on, nearest first.</summary>
	/// <param name="visited">
	/// Told every class the walk passes, with a grammar or without one: what a change to any
	/// of them can alter.
	/// </param>
	public static IReadOnlyList<GramInclude> Walk(INamedTypeSymbol type, Action<INamedTypeSymbol>? visited = null)
	{
		var included = new List<GramInclude>();
		var seen     = new HashSet<string>(StringComparer.Ordinal) { type.ToDisplayString() };
		var pending  = new Queue<(INamedTypeSymbol Type, string? As)>();

		for (var above = type.BaseType; above is not null; above = above.BaseType)
			pending.Enqueue((above, null));

		foreach (var named in Named(type))
			pending.Enqueue(named);

		while (pending.Count > 0)
		{
			var (above, called) = pending.Dequeue();

			if (!seen.Add(above.ToDisplayString()))
				continue;

			visited?.Invoke(above);

			foreach (var named in Named(above))
				pending.Enqueue(named);

			// The one compiled into the base class itself where there are several: a
			// grammar including another names a class, and what that class publishes
			// under a scope of its own is that scope's, not the class's.
			var attribute = Primary(above);

			if (attribute is null)
				continue;

			// What the includer calls it wins over what the includee calls itself: the
			// name a grammar is reached under inside yours is your business.
			var under = called ?? attribute.NamedArguments
				.FirstOrDefault(static argument => argument.Key == "IncludedAs")
				.Value.Value as string;

			included.Add(new GramInclude(
				above,
				attribute,
				under ?? above.Name,
				Source(attribute),

				// What the class itself says its grammar is, which travels with the
				// assembly when the file does not.
				above
					.GetAttributes()
					.FirstOrDefault(static candidate =>
						candidate.AttributeClass?.ToDisplayString() == GramSourceAttribute)
					?.ConstructorArguments.FirstOrDefault().Value as string));
		}

		return included;
	}

	/// <summary>Every <c>[Gram]</c> on a class: its own reading and any suffixed ones.</summary>
	public static IEnumerable<AttributeData> Grams(ISymbol type)
	{
		return type.GetAttributes().Where(static candidate =>
			candidate.AttributeClass?.ToDisplayString() == GramAttribute);
	}

	/// <summary>The class's own <c>[Gram]</c>, the one without a <c>Suffix</c>; null where every one has one.</summary>
	public static AttributeData? Primary(ISymbol type)
	{
		return Grams(type).FirstOrDefault(static candidate =>
			candidate.NamedArguments.All(static named => named.Key != "Suffix"));
	}

	/// <summary>The attribute's argument — a path or the grammar itself — or null where it has none.</summary>
	public static string? Source(AttributeData attribute)
	{
		return attribute.ConstructorArguments.Length == 1
			? attribute.ConstructorArguments[0].Value as string
			: null;
	}

	/// <summary>
	/// The file an include's attribute names where it names one: its argument, or with none
	/// the file named after the class's innermost name.
	/// </summary>
	public static string Wanted(GramInclude include)
	{
		var name = include.Type.ToDisplayString();
		var dot  = name.LastIndexOf('.');

		return include.Source ?? (dot < 0 ? name : name.Substring(dot + 1)) + GramFileExtension;
	}

	/// <summary>A single line ending in <c>.gram</c> is a path; anything else is the grammar itself.</summary>
	public static bool IsPath(string source)
	{
		return source.EndsWith(GramFileExtension, StringComparison.OrdinalIgnoreCase) &&
		source.IndexOf('\n') < 0 &&
		source.IndexOf('\r') < 0;
	}

	/// <summary>
	/// A file matches a wanted path when it ends with it on a separator boundary — the
	/// attribute names a path relative to the project, and what reaches us is absolute.
	/// </summary>
	public static bool Matches(string filePath, string wanted)
	{
		var normalized = wanted.Replace('/', '\\');

		if (!filePath.Replace('/', '\\').EndsWith(normalized, StringComparison.OrdinalIgnoreCase))
			return false;

		var boundary = filePath.Length - normalized.Length - 1;

		return boundary < 0 || filePath[boundary] is '\\' or '/';
	}

	/// <summary>What a class names with <c>[GramInclude]</c>, in the order it wrote them.</summary>
	static IEnumerable<(INamedTypeSymbol Type, string? As)> Named(INamedTypeSymbol of)
	{
		foreach (var attribute in of.GetAttributes())
			if (attribute.AttributeClass?.ToDisplayString() == GramIncludeAttribute &&
				attribute.ConstructorArguments is [{ Value: INamedTypeSymbol grammar }])
			{
				yield return (
					grammar,
					attribute.NamedArguments
						.FirstOrDefault(static argument => argument.Key == "As")
						.Value.Value as string);
			}
	}
}
