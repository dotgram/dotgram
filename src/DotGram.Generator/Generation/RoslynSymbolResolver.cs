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

	public bool TypeExists(string qualifiedName) =>
		_compilation.GetTypeByMetadataName(qualifiedName) is not null;

	public bool TryResolveMethod(string qualifiedName, int argumentCount, out MethodRole role)
	{
		role = MethodRole.ValueTransformation;

		var separator = qualifiedName.LastIndexOf('.');

		if (separator <= 0)
			return false;

		var type = _compilation.GetTypeByMetadataName(qualifiedName.Substring(0, separator));

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
	/// SYNTAX.md §7.1: a method taking the input and a <c>ref int pos</c> is a
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
