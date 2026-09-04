using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar;

using Microsoft.CodeAnalysis;

namespace DotGram.Generation;

/// <summary>
/// Resolves the C# type relationships a grammar cannot know by itself.
/// </summary>
/// <remarks>
/// The only part of compilation that genuinely needs Roslyn, which is why it sits
/// behind <see cref="ISymbolResolver"/> and lives here rather than in the grammar half.
/// </remarks>
/// <param name="host">
/// The metadata name of the class the grammar is attached to, or null when there is none.
/// </param>
/// <remarks>The host matters for nested result types declared beside the grammar.</remarks>
public sealed class RoslynSymbolResolver(Compilation compilation, string? host = null) : ISymbolResolver
{
	readonly Compilation _compilation = compilation ?? throw new ArgumentNullException(nameof(compilation));
	readonly string?     _host        = host;

	public bool TypeExists(string qualifiedName) => TypeNamed(qualifiedName) is not null;

	/// <summary>
	/// Whether a value of one type may be put where the other is expected.
	/// </summary>
	/// <remarks>
	/// Roslyn's own conversion classification, minus the two kinds §4.1 case 2 does not
	/// want: a numeric widening would put an <c>@int</c> rule into a sequence of
	/// <c>@long</c> and a user-defined conversion would run somebody's operator while
	/// matching. What is left is identity, reference and boxing — the operands that
	/// already <i>are</i> the element type, which is why a sequence of <c>@object</c>
	/// takes everything.
	/// </remarks>
	public bool IsAssignable(string from, string to)
	{
		if (string.Equals(from, to, StringComparison.Ordinal))
			return true;

		return TypeNamed(from) is { } source && TypeNamed(to) is { } target && IsAssignableSymbol(source, target);
	}

	/// <summary>
	/// What the declared type can be built with, for §7.3's first way of filling one in.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Only the constructors the generated code could actually call: it sits inside the
	/// host class, so a <c>private</c> one of the host's own nested type is as callable as
	/// a public one, and <c>internal</c> is callable when the type is in this assembly.
	/// Roslyn is asked rather than guessed at.
	/// </para>
	/// <para>
	/// Static constructors and those taking a pointer are left out — the first cannot be
	/// called at all and the second cannot be reached from a grammar.
	/// </para>
	/// </remarks>
	public bool TryResolveConstructors(
		string qualifiedName,
		out IReadOnlyList<IReadOnlyList<MethodParameter>> constructors)
	{
		constructors = [];

		if (TypeNamed(qualifiedName) is not { } type)
			return false;

		var found = new List<IReadOnlyList<MethodParameter>>();

		foreach (var constructor in type.InstanceConstructors)
		{
			if (constructor.IsStatic || !_compilation.IsSymbolAccessibleWithin(constructor, _compilation.Assembly))
				continue;

			var parameters = new List<MethodParameter>(constructor.Parameters.Length);

			foreach (var parameter in constructor.Parameters)
				parameters.Add(new MethodParameter(
					parameter.Name,
					parameter.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
					parameter.IsOptional));

			found.Add(parameters);
		}

		constructors = found;

		return found.Count > 0;
	}

	/// <summary>
	/// What an object initializer could write on the declared type (§7.3's second way).
	/// </summary>
	/// <remarks>
	/// A property counts when the generated code could set it where it sits — which is
	/// inside the host class, so the host's own private members count as much as public
	/// ones. <c>init</c> and ordinary setters both qualify: an initializer may write
	/// either, and which one the author chose is their design rather than this compiler's
	/// concern.
	/// </remarks>
	public bool TryResolveSettableProperties(string qualifiedName, out IReadOnlyList<ObjectMember> properties)
	{
		properties = [];

		if (TypeNamed(qualifiedName) is not { } type)
			return false;

		var found = new List<ObjectMember>();

		for (var at = type; at is not null; at = at.BaseType)
			foreach (var member in at.GetMembers().OfType<IPropertySymbol>())
			{
				if (member.IsStatic ||
					member.IsIndexer ||
					member.SetMethod is not { } setter ||
					!_compilation.IsSymbolAccessibleWithin(setter, _compilation.Assembly) ||
					found.Any(one => string.Equals(one.Name, member.Name, StringComparison.Ordinal)))
				{
					continue;
				}

				found.Add(new ObjectMember(
					member.Name,
					member.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
					member.IsRequired));
			}

		properties = found;

		return found.Count > 0;
	}

	/// <summary>
	/// What an external recognizer named this hands back, when it hands back anything of
	/// its own — §7.1's third row.
	/// </summary>
	/// <remarks>
	/// The one place this resolver inspects a method's signature rather than a type's shape.
	/// Bare <c>@Name</c> notation does not say which of §7.1's second and third rows is
	/// meant, so something has to look — an overload matching
	/// <c>bool M(ReadOnlySpan&lt;char&gt;, ref int, out T)</c> means the third; its absence
	/// means the second, unchanged. Accessible from the host specifically, not merely the
	/// assembly: every recognizer in every example is declared beside the grammar, and the
	/// generated call sits inside that class, so an otherwise-private one is exactly as
	/// callable as a public one — the same reasoning <see cref="TypeNamed"/> already gives
	/// a nested result type.
	/// </remarks>
	public ExternalValueResolution TryResolveExternalValue(string methodName, string? against, out string? valueType)
	{
		valueType = null;

		var types = new List<ITypeSymbol>();
		var host  = (ISymbol?)(_host is not null ? _compilation.GetTypeByMetadataName(_host) : null) ??
			_compilation.Assembly;

		foreach (var symbol in _compilation.GetSymbolsWithName(
			name => string.Equals(name, methodName, StringComparison.Ordinal), SymbolFilter.Member))
		{
			if (symbol is not IMethodSymbol
				{
					IsStatic: true,
					ReturnType.SpecialType: SpecialType.System_Boolean,
					Parameters: [var input, { RefKind: RefKind.Ref } position, { RefKind: RefKind.Out } value],
				} method ||
				!IsReadOnlySpanOfChar(input.Type) ||
				position.Type.SpecialType != SpecialType.System_Int32 ||
				!_compilation.IsSymbolAccessibleWithin(method, host))
			{
				continue;
			}

			if (!types.Contains(value.Type, SymbolEqualityComparer.Default))
				types.Add(value.Type);
		}

		if (types.Count == 0)
			return ExternalValueResolution.NotFound;

		if (types.Count > 1)
			return ExternalValueResolution.Ambiguous;

		// A whole rule's body being exactly this call (§4.1 case 3 applied to one) needs T
		// to fit the rule's own declared type — checked here, live, because T was just
		// discovered and nothing upstream could have asked IsAssignable about it in advance.
		// Against the symbol directly, not its FullyQualifiedFormat string round-tripped
		// through TypeNamed: that format's leading "global::" is source syntax, and
		// GetTypeByMetadataName does not parse it back out.
		if (against is not null &&
			(TypeNamed(against) is not { } target ||
				!IsAssignableSymbol(types[0], target)))
		{
			return ExternalValueResolution.NotFound;
		}

		valueType = types[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

		return ExternalValueResolution.Found;
	}

	/// <summary>The symbol-typed core of <see cref="IsAssignable"/>, shared rather than
	/// re-derived by round-tripping a symbol through a display string and back.</summary>
	bool IsAssignableSymbol(ITypeSymbol source, ITypeSymbol target)
	{
		if (SymbolEqualityComparer.Default.Equals(source, target))
			return true;

		var conversion = _compilation.ClassifyCommonConversion(source, target);

		return conversion.IsImplicit && !conversion.IsNumeric && !conversion.IsUserDefined;
	}

	/// <summary>Whether this is <c>System.ReadOnlySpan&lt;char&gt;</c>, exactly.</summary>
	static bool IsReadOnlySpanOfChar(ITypeSymbol type) =>
		type is INamedTypeSymbol
		{
			Name: "ReadOnlySpan",
			ContainingNamespace.Name: "System",
			TypeArguments: [{ SpecialType: SpecialType.System_Char }],
		};

	/// <summary>
	/// A type by the name a grammar writes, keyword or otherwise.
	/// </summary>
	/// <remarks>
	/// Roslyn looks types up by their metadata name, and <c>int</c> is not one — §2 says
	/// the C# keywords are always C# types, so a grammar writing <c>@int.Parse</c> means
	/// <c>System.Int32.Parse</c> and expects to be understood.
	/// </remarks>
	/// <remarks>
	/// The host is looked in as well: a nested type written beside the grammar is a type
	/// the author never has to name in full anywhere else. Without this lookup, <c>@Row</c>
	/// beside a nested <c>Row</c> would mean only a top-level type.
	/// </remarks>
	INamedTypeSymbol? TypeNamed(string qualifiedName)
	{
		var name = Keywords.TryGetValue(qualifiedName, out var framework) ? framework : qualifiedName;

		// The host first, because that is the order C# itself resolves in and because the
		// generated code is written inside the host class: a short name there binds to the
		// nested type whatever this method decides, so deciding otherwise would check one
		// type and call another. Nested types are `Outer+Inner` in metadata, and a grammar
		// writing `Row.Inner` means one nested inside another.
		if (_host is not null &&
			_compilation.GetTypeByMetadataName(_host + "+" + name.Replace('.', '+')) is { } nested)
		{
			return nested;
		}

		if (_compilation.GetTypeByMetadataName(name) is { } found)
			return found;

		// A type nested in another is `Outer+Inner` in metadata and `Outer.Inner` in C#, and
		// a grammar writes C#. Which of the dots is the one between the containing type and
		// the nested one is not known from the text — `A.B.C` is a namespace `A` with a type
		// `B` holding `C` as readily as a namespace `A.B` holding `C` — so every split is
		// tried, rightmost first, since a namespace is the longer prefix more often than not.
		for (var dot = name.LastIndexOf('.'); dot > 0; dot = name.LastIndexOf('.', dot - 1))
		{
			var metadata = name.Substring(0, dot) + "+" + name.Substring(dot + 1).Replace('.', '+');

			if (_compilation.GetTypeByMetadataName(metadata) is { } inner)
				return inner;
		}

		// `GetTypeByMetadataName` answers null when the name is ambiguous as well as when
		// it is unknown, and the two deserve different treatment. Two types of one name is
		// something C# reports itself (CS0101, or CS0104 at the use), so saying "no C# type
		// named 'Trade' is in view" on top of it is a second message about one mistake —
		// and a misleading one, since there are two of them. The first is taken and the
		// grammar goes on; what is wrong reaches the author from the compiler that owns
		// the question.
		var candidates = _compilation.GetTypesByMetadataName(name);

		return candidates.Length > 0 ? candidates[0] : null;
	}

	static readonly System.Collections.Generic.Dictionary<string, string> Keywords =
		new(StringComparer.Ordinal)
		{
			["bool"]    = "System.Boolean",
			["byte"]    = "System.Byte",
			["sbyte"]   = "System.SByte",
			["char"]    = "System.Char",
			["decimal"] = "System.Decimal",
			["double"]  = "System.Double",
			["float"]   = "System.Single",
			["int"]     = "System.Int32",
			["uint"]    = "System.UInt32",
			["long"]    = "System.Int64",
			["ulong"]   = "System.UInt64",
			["short"]   = "System.Int16",
			["ushort"]  = "System.UInt16",
			["string"]  = "System.String",
			["object"]  = "System.Object",
		};
}
