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
/// Finds the names an attribute of the generator had before 0.3.0 (<see cref="GramRenames"/>), so that
/// the build says what each is called now (GRAM0012) rather than only that C# cannot find it.
/// </summary>
/// <remarks>
/// <para>
/// By syntax, since an old name is exactly what C# does not bind: a named argument it cannot find is
/// dropped from the attribute's data, and <c>[GramOptions]</c> is no type at all. Asked of a semantic
/// model, <c>[GramOptions]</c> and <c>GramCarrier.Tape</c> are only said where C# found nothing under
/// that name, so a type of the author's own called that is left alone.
/// </para>
/// <para>
/// A host written with one has its grammar's own diagnostics left unsaid (<c>Host.Outdated</c>): a
/// dropped <c>Alias</c> or <c>Name</c> makes the grammar look wrong in ways the rename alone explains.
/// </para>
/// </remarks>
static class Renamed
{
	static readonly HashSet<string> Readings  = new(StringComparer.Ordinal) { "Gram", "GramAttribute", "GramParserVariant", "GramParserVariantAttribute" };
	static readonly HashSet<string> Includes  = new(StringComparer.Ordinal) { "GramInclude", "GramIncludeAttribute" };
	static readonly HashSet<string> Variants  = new(StringComparer.Ordinal) { GramRenames.OldVariant, GramRenames.OldVariant + "Attribute" };

	/// <summary>Whether a node is an attribute that may hold an old name: one of the generator's, by its simple name.</summary>
	public static bool IsCandidate(SyntaxNode node)
	{
		return node is AttributeSyntax attribute && SimpleName(attribute.Name) is var name &&
			(Readings.Contains(name) || Includes.Contains(name) || Variants.Contains(name));
	}

	/// <summary>Every old name an attribute is written with, in the order written.</summary>
	/// <param name="model">The attribute's semantic model, or null to judge by syntax alone.</param>
	public static ImmutableArray<RenamedUse> Find(AttributeSyntax attribute, SemanticModel? model, CancellationToken cancellationToken)
	{
		var found = ImmutableArray.CreateBuilder<RenamedUse>();
		var name  = SimpleName(attribute.Name);

		if (Variants.Contains(name))
		{
			if (model is null || model.GetSymbolInfo(attribute, cancellationToken) is { Symbol: null, CandidateSymbols.Length: 0 })
				found.Add(new RenamedUse(GramRenames.OldVariant, GramRenames.NewVariant, Place.Of(attribute.Name.GetLocation())));
		}
		else
		{
			var renamed = Readings.Contains(name) ? GramRenames.Options : Includes.Contains(name) ? GramRenames.IncludeOptions : null;

			foreach (var argument in attribute.ArgumentList?.Arguments ?? default)
				if (renamed is not null && argument.NameEquals?.Name is { } named && renamed.TryGetValue(named.Identifier.ValueText, out var now))
					found.Add(new RenamedUse(named.Identifier.ValueText, now, Place.Of(named.GetLocation())));
		}

		foreach (var access in attribute.DescendantNodes().OfType<MemberAccessExpressionSyntax>())
			if (access.Name.Identifier.ValueText == "Tape" &&
				SimpleName(access.Expression) == "GramCarrier" &&
				(model is null || model.GetSymbolInfo(access, cancellationToken).Symbol is null))
			{
				found.Add(new RenamedUse(GramRenames.OldCarrier, GramRenames.NewCarrier, Place.Of(access.GetLocation())));
			}

		return found.ToImmutable();
	}

	/// <summary>Whether any attribute of a class declaration is written with an old name, by syntax alone.</summary>
	public static bool Any(MemberDeclarationSyntax declaration)
	{
		return declaration.AttributeLists
			.SelectMany(static list => list.Attributes)
			.Any(static attribute => Find(attribute, null, CancellationToken.None).Length > 0);
	}

	/// <summary>The last identifier of a name, qualified or not: <c>DotGram.Gram</c> is <c>Gram</c>.</summary>
	static string SimpleName(ExpressionSyntax name)
	{
		return name switch
		{
			QualifiedNameSyntax qualified       => qualified.Right.Identifier.ValueText,
			AliasQualifiedNameSyntax aliased    => aliased.Name.Identifier.ValueText,
			MemberAccessExpressionSyntax access => access.Name.Identifier.ValueText,
			SimpleNameSyntax simple             => simple.Identifier.ValueText,
			_                                   => "",
		};
	}
}
