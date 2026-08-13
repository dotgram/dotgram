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
public sealed class RoslynSymbolResolver(Compilation compilation) : ISymbolResolver
{
	readonly Compilation _compilation = compilation ?? throw new ArgumentNullException(nameof(compilation));

	public bool TypeExists(string qualifiedName) => TypeNamed(qualifiedName) is not null;

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

		if (separator <= 0)
			return false;

		var type = TypeNamed(qualifiedName.Substring(0, separator));

		if (type is null)
			return false;

		var method = type
			.GetMembers(qualifiedName.Substring(separator + 1))
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

		return method.ReturnType.SpecialType == SpecialType.System_Boolean
			? MethodRole.Guard
			: MethodRole.ValueTransformation;
	}
}
