using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar;

using Microsoft.CodeAnalysis;

namespace DotGram.Generation;

/// <summary>
/// Resolves a grammar's <c>@Name</c> against the host compilation.
/// </summary>
/// <remarks>
/// The only part of compilation that genuinely needs Roslyn, which is why it sits
/// behind <see cref="ISymbolResolver"/> and lives here rather than in the grammar half.
/// </remarks>
/// <param name="host">
/// The metadata name of the class the grammar is attached to, or null when there is none.
/// </param>
/// <remarks>
/// The host is where a grammar's own helpers live — <c>=&gt; @TryTiny(digits)</c> means
/// the method next to the grammar, and writing it out as <c>Namespace.Class.TryTiny</c>
/// would be naming a class the author never has to name anywhere else. So an unqualified
/// name is looked for there first, the way C# itself resolves one inside a class.
/// </remarks>
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

		if (TypeNamed(from) is not { } source || TypeNamed(to) is not { } target)
			return false;

		var conversion = _compilation.ClassifyCommonConversion(source, target);

		return conversion.IsImplicit && !conversion.IsNumeric && !conversion.IsUserDefined;
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
	/// A type by the name a grammar writes, keyword or otherwise.
	/// </summary>
	/// <remarks>
	/// Roslyn looks types up by their metadata name, and <c>int</c> is not one — §2 says
	/// the C# keywords are always C# types, so a grammar writing <c>@int.Parse</c> means
	/// <c>System.Int32.Parse</c> and expects to be understood.
	/// </remarks>
	/// <remarks>
	/// The host is looked in as well, and for the same reason its methods are: a type
	/// written beside the grammar is a type the author never has to name in full anywhere
	/// else. Without it `@Row` beside `Row` meant a top-level `Row`, while `@Method`
	/// beside `Method` meant the host's own — an asymmetry nobody chose.
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

	public bool TryResolveMethod(string qualifiedName, int argumentCount, out MethodRole role)
	{
		role = MethodRole.ValueTransformation;

		var separator = qualifiedName.LastIndexOf('.');

		// Unqualified: the host class, which is where a grammar's own helpers live. Looked
		// at first and not only as a fallback — a name written without a dot is a name in
		// the class the grammar is attached to, exactly as it would be in C#.
		var type = separator <= 0
			? _host is null ? null : _compilation.GetTypeByMetadataName(_host)
			: TypeNamed(qualifiedName.Substring(0, separator));

		if (type is null)
			return false;

		var name   = separator <= 0 ? qualifiedName : qualifiedName.Substring(separator + 1);
		var method = type
			.GetMembers(name)
			.OfType<IMethodSymbol>()
			.FirstOrDefault(candidate => candidate.Parameters.Length >= argumentCount);

		// A half of a partial method counts as not being there. The author writes the
		// implementation and §7.4 writes the declaration that completes it, so a method
		// with an implementation and no definition is exactly the one to declare — saying
		// it exists would leave the two halves unjoined (CS0759).
		if (method is null || Unjoined(method))
			return false;

		role = Classify(method, argumentCount);

		return true;
	}

	/// <summary>An implementation of a partial method whose declaration nobody wrote.</summary>
	static bool Unjoined(IMethodSymbol method)
	{
		if (method.IsPartialDefinition || method.PartialDefinitionPart is not null)
			return false;

		foreach (var reference in method.DeclaringSyntaxReferences)
			if (reference.GetSyntax() is Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax declaration &&
				declaration.Modifiers.Any(static modifier => modifier.ValueText == "partial"))
			{
				return true;
			}

		return false;
	}

	/// <summary>
	/// docs/syntax.md §7.1: a method taking the input and a <c>ref int pos</c> is a
	/// recognizer; anything else never touches input.
	/// </summary>
	static MethodRole Classify(IMethodSymbol method, int argumentCount)
	{
		var parameters = method.Parameters;

		if (parameters.Length >= 2 &&
			parameters[1].RefKind == RefKind.Ref &&
			parameters[1].Type.SpecialType == SpecialType.System_Int32)
		{
			return MethodRole.ExternalRecognizer;
		}

		if (argumentCount == 0 && parameters.Length == 1 && method.ReturnType.SpecialType == SpecialType.System_Boolean)
			return MethodRole.ElementPredicate;

		if (method.ReturnType.SpecialType != SpecialType.System_Boolean)
			return MethodRole.ValueTransformation;

		// `bool M(args…, out T)` — §8.1's fallible transformation, told from a guard by the
		// out parameter that carries what it produced. The arity the grammar wrote counts
		// the arguments it passes, so the `out` is the one parameter beyond them.
		return parameters.Length == argumentCount + 1 &&
			parameters[parameters.Length - 1].RefKind == RefKind.Out
				? MethodRole.FallibleTransformation
				: MethodRole.Guard;
	}
}
