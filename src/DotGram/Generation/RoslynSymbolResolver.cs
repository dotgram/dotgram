using System;
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
	/// A type by the name a grammar writes, keyword or otherwise.
	/// </summary>
	/// <remarks>
	/// Roslyn looks types up by their metadata name, and <c>int</c> is not one — §2 says
	/// the C# keywords are always C# types, so a grammar writing <c>@int.Parse</c> means
	/// <c>System.Int32.Parse</c> and expects to be understood.
	/// </remarks>
	INamedTypeSymbol? TypeNamed(string qualifiedName) =>
		_compilation.GetTypeByMetadataName(Keywords.TryGetValue(qualifiedName, out var framework)
			? framework
			: qualifiedName);

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

		if (method is null)
			return false;

		role = Classify(method, argumentCount);

		return true;
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
