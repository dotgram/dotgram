using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using DotGram.Grammar;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotGram.Generation;

/// <summary>One use of a name the generator's attributes had before 0.3.0, as values.</summary>
/// <param name="Old">The name as written.</param>
/// <param name="Now">What it is called now.</param>
/// <param name="At">Where it is written.</param>
readonly record struct RenamedUse(string Old, string Now, Place? At);

/// <summary>
/// Finds the names the generator's attributes had before 0.3.0 (<see cref="GramRenames"/>), so that the
/// build says what each is called now (GRAM0012) rather than only that C# cannot find it.
/// </summary>
/// <remarks>
/// <para>
/// By the symbols C# binds, so that what is the author's own is left alone: a named argument is an old
/// option only on an attribute that is DotGram's own — however it is spelled, an alias included — whose
/// property of that name is gone; <c>[GramOptions]</c> only where C# found no type of that name; and
/// <c>.Tape</c> only on DotGram's <c>GramCarrier</c>, wherever it is written, an attribute or not.
/// </para>
/// <para>
/// What a dropped name explains is not said of the host on top (<see cref="DropsAlias"/>): an alias
/// written as <c>As</c> or <c>IncludedAs</c> is lost, and the grammar's names then fail to resolve.
/// Nothing else is held back.
/// </para>
/// </remarks>
static class Renamed
{
	static readonly HashSet<string> Variants = new(StringComparer.Ordinal) { GramRenames.OldVariant, GramRenames.OldVariant + "Attribute" };

	/// <summary>The classes of the generator's attributes whose options were renamed, by name in its namespace.</summary>
	static readonly Dictionary<string, IReadOnlyDictionary<string, string>> Renames = new(StringComparer.Ordinal)
	{
		["GramAttribute"]              = GramRenames.Options,
		["GramParserVariantAttribute"] = GramRenames.Options,
		["GramIncludeAttribute"]       = GramRenames.IncludeOptions,
	};

	/// <summary>
	/// Whether a node may hold an old name, by syntax alone and cheaply: an attribute named like
	/// <c>[GramOptions]</c> or with a named argument an old option was called, or a <c>.Tape</c>.
	/// </summary>
	public static bool IsCandidate(SyntaxNode node)
	{
		return node switch
		{
			AttributeSyntax attribute => Variants.Contains(SimpleName(attribute.Name)) || OldArguments(attribute).Any(),
			MemberAccessExpressionSyntax access => access.Name.Identifier.ValueText == "Tape",
			_ => false,
		};
	}

	/// <summary>Every old name a candidate node is written with, as C# binds it.</summary>
	public static ImmutableArray<RenamedUse> Find(SyntaxNode node, SemanticModel model, CancellationToken cancellationToken)
	{
		var found = ImmutableArray.CreateBuilder<RenamedUse>();

		if (node is MemberAccessExpressionSyntax access)
		{
			// DotGram's enum, through whatever name or alias reaches it, and no member of that name on it.
			if (model.GetSymbolInfo(access, cancellationToken) is { Symbol: null, CandidateSymbols.Length: 0 } &&
				model.GetSymbolInfo(access.Expression, cancellationToken).Symbol is INamedTypeSymbol carrier &&
				IsDotGram(carrier, "GramCarrier"))
			{
				found.Add(new RenamedUse(GramRenames.OldCarrier, GramRenames.NewCarrier, Place.Of(access.GetLocation())));
			}
		}
		else if (node is AttributeSyntax attribute)
		{
			var bound = model.GetSymbolInfo(attribute, cancellationToken);

			if (Variants.Contains(SimpleName(attribute.Name)) && bound is { Symbol: null, CandidateSymbols.Length: 0 })
				found.Add(new RenamedUse(GramRenames.OldVariant, GramRenames.NewVariant, Place.Of(attribute.Name.GetLocation())));
			else if ((bound.Symbol ?? bound.CandidateSymbols.FirstOrDefault()) is IMethodSymbol { ContainingType: var type } &&
				RenamesOf(type) is { } renames)
			{
				foreach (var named in OldArguments(attribute))
					if (renames.TryGetValue(named.Identifier.ValueText, out var now) && type.GetMembers(named.Identifier.ValueText).IsEmpty)
						found.Add(new RenamedUse(named.Identifier.ValueText, now, Place.Of(named.GetLocation())));
			}
		}

		return found.ToImmutable();
	}

	/// <summary>
	/// Whether an attribute of the generator's, as C# bound it, is written with an old name that drops an
	/// alias — <c>As</c> on <c>[GramInclude]</c>, <c>IncludedAs</c> on <c>[Gram]</c> — so that the names of a
	/// grammar built on it no longer resolve: what GRAM0012 explains, and the host is not told again.
	/// </summary>
	public static bool DropsAlias(AttributeData attribute)
	{
		return attribute.AttributeClass is { } type &&
			(IsDotGram(type, "GramIncludeAttribute") || IsDotGram(type, "GramAttribute")) &&
			attribute.ApplicationSyntaxReference?.GetSyntax() is AttributeSyntax syntax &&
			OldArguments(syntax).Any(static named => named.Identifier.ValueText is "As" or "IncludedAs");
	}

	/// <summary>The renamed options of one of the generator's attributes, or null for any other class.</summary>
	static IReadOnlyDictionary<string, string>? RenamesOf(INamedTypeSymbol type)
	{
		return Renames.TryGetValue(type.Name, out var renames) && IsDotGram(type, type.Name) ? renames : null;
	}

	/// <summary>Whether a type is the generator's own of that name, in the namespace it emits into.</summary>
	static bool IsDotGram(INamedTypeSymbol type, string name)
	{
		return type.Name == name &&
			type.ContainingType is null &&
			type.ContainingNamespace is { ContainingNamespace.IsGlobalNamespace: true } space &&
			space.Name == GramMetadataNames.Namespace;
	}

	/// <summary>The names of an attribute's named arguments that are an option's old name.</summary>
	static IEnumerable<IdentifierNameSyntax> OldArguments(AttributeSyntax attribute)
	{
		foreach (var argument in attribute.ArgumentList?.Arguments ?? default)
			if (argument.NameEquals?.Name is { } named &&
				(GramRenames.Options.ContainsKey(named.Identifier.ValueText) || GramRenames.IncludeOptions.ContainsKey(named.Identifier.ValueText)))
			{
				yield return named;
			}
	}

	/// <summary>The last identifier of a name, qualified or not: <c>DotGram.Gram</c> is <c>Gram</c>.</summary>
	static string SimpleName(NameSyntax name)
	{
		return name switch
		{
			QualifiedNameSyntax qualified    => qualified.Right.Identifier.ValueText,
			AliasQualifiedNameSyntax aliased => aliased.Name.Identifier.ValueText,
			SimpleNameSyntax simple          => simple.Identifier.ValueText,
			_                                => "",
		};
	}
}
