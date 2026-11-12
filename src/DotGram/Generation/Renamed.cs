using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using DotGram.Grammar;
using DotGram.Language;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
/// property of that name is gone; <c>[GramOptions]</c> only where C# found no type of that name and the
/// name reaches DotGram's namespace — its attributes in view, a qualifier or an alias target that binds
/// to it; and <c>Tape</c> only as a value on DotGram's <c>GramCarrier</c>, wherever it is written, an
/// attribute or not. No decision is made by how a name is spelled; spelling only picks the candidates.
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
			// A simple name may be an alias of the old [GramOptions] from a `using` in any file, which only
			// binding can say (Find).
			AttributeSyntax attribute => attribute.Name is IdentifierNameSyntax || Variants.Contains(SimpleName(attribute.Name)) || OldArguments(attribute).Any(),
			MemberAccessExpressionSyntax access => access.Name.Identifier.ValueText == "Tape",

			// `Tape` on its own, reached through `using static DotGram.GramCarrier` (Find).
			IdentifierNameSyntax { Identifier.ValueText: "Tape" } identifier => identifier.Parent is not MemberAccessExpressionSyntax { Name: var name } || name != identifier,
			_ => false,
		};
	}

	/// <summary>Every old name a candidate node is written with, as C# binds it.</summary>
	public static ImmutableArray<RenamedUse> Find(SyntaxNode node, SemanticModel model, CancellationToken cancellationToken)
	{
		var found = ImmutableArray.CreateBuilder<RenamedUse>();

		if (node is IdentifierNameSyntax tape)
		{
			// A value written where nothing of that name is in view, and DotGram's enum's members in view
			// by their simple names, as a `using static` in any file brings them; never a type or a
			// namespace, which an enum's member could not have been.
			if (!SyntaxFacts.IsInNamespaceOrTypeContext(tape) &&
				model.GetSymbolInfo(tape, cancellationToken) is { Symbol: null, CandidateSymbols.Length: 0 } &&
				model
					.LookupSymbols(tape.SpanStart, name: "Deferred")
					.Any(static member => member is IFieldSymbol { ContainingType: { } carrier } && IsDotGram(carrier, "GramCarrier")))
			{
				found.Add(new RenamedUse(GramRenames.OldCarrier, GramRenames.NewCarrier, Place.Of(tape.GetLocation())));
			}
		}
		else if (node is MemberAccessExpressionSyntax access)
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

			if (bound is { Symbol: null, CandidateSymbols.Length: 0 } && IsOldVariant(attribute.Name, model, cancellationToken))
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
	/// The aliases a host's grammar lost to an old name, as the old name wrote them: <c>As</c> on a
	/// <c>[GramInclude]</c> of the host or of any class it is built on, <c>IncludedAs</c> on the <c>[Gram]</c>
	/// of any class it includes. The grammar's names that went through one no longer resolve, which
	/// GRAM0012 explains; those reports, and only those, are not made again (<see cref="Explained"/>).
	/// </summary>
	/// <param name="compilation">What binds the alias each wrote, a constant or a <c>nameof</c> as well as a literal.</param>
	public static ImmutableArray<string> DroppedAliases(INamedTypeSymbol host, Compilation compilation)
	{
		var classes = new List<INamedTypeSymbol>();
		var dropped = new List<string>();

		GramIncludes.Walk(host, classes.Add);

		foreach (var type in classes.Prepend(host))
			foreach (var attribute in type.GetAttributes())
			{
				// Each old name on the attribute it was the option of: `As` on [GramInclude], `IncludedAs`
				// on the [Gram] an included class is included by — its reading with no Name; one with a
				// Name is never included, so what it would have been called explains nothing. Either
				// name on any other attribute is no old name, and C# says so.
				var old = attribute.AttributeClass switch
				{
					{ } include when IsDotGram(include, "GramIncludeAttribute") => "As",
					{ } gram when IsDotGram(gram, "GramAttribute") &&
						!SymbolEqualityComparer.Default.Equals(type, host) &&
						GramIncludes.Primary(type) == attribute                => "IncludedAs",
					_                                                          => null,
				};

				if (old is null || attribute.ApplicationSyntaxReference?.GetSyntax() is not AttributeSyntax syntax)
					continue;

				var model = compilation.GetSemanticModel(syntax.SyntaxTree);

				foreach (var argument in syntax.ArgumentList?.Arguments ?? default)
					if (argument.NameEquals?.Name.Identifier.ValueText == old &&
						model.GetConstantValue(argument.Expression) is { HasValue: true, Value: string alias })
					{
						dropped.Add(alias);
					}
			}

		return [.. dropped.Distinct(StringComparer.Ordinal)];
	}

	/// <summary>
	/// Whether a report is one a dropped alias explains: a namespace of that name not in view
	/// (GRAM3003), or a name reached through it (GRAM3002) — one qualified by it, or any where the
	/// grammar opens it with <c>using</c>, since that name was to be found there.
	/// </summary>
	public static bool Explained(string id, string message, IReadOnlyCollection<string> dropped, string? grammar)
	{
		if (id is not ("GRAM3002" or "GRAM3003") || Quoted(message) is not { } name)
			return false;

		foreach (var alias in dropped)
		{
			if (name == alias || name.StartsWith(alias + ".", StringComparison.Ordinal))
				return true;

			if (id == "GRAM3002" && grammar is not null &&
				System.Text.RegularExpressions.Regex.IsMatch(grammar, @"\busing\s+" + System.Text.RegularExpressions.Regex.Escape(alias) + @"\s*;"))
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>The first name a message quotes, which is the one GRAM3002 and GRAM3003 are about.</summary>
	static string? Quoted(string message)
	{
		var open  = message.IndexOf('\'');
		var close = open < 0 ? -1 : message.IndexOf('\'', open + 1);

		return close < 0 ? null : message.Substring(open + 1, close - open - 1);
	}

	/// <summary>
	/// Whether an attribute's name, which C# did not bind, is the old <c>[GramOptions]</c>, as binding says:
	/// written where DotGram's attributes are in view by their simple names, qualified by a name that
	/// binds to DotGram's namespace, or an alias whose target is so qualified.
	/// </summary>
	static bool IsOldVariant(NameSyntax name, SemanticModel model, CancellationToken cancellationToken)
	{
		switch (name)
		{
			case IdentifierNameSyntax simple when Variants.Contains(simple.Identifier.ValueText):
				return model
					.LookupNamespacesAndTypes(simple.SpanStart, name: "GramParserVariantAttribute")
					.OfType<INamedTypeSymbol>()
					.Any(static type => IsDotGram(type, "GramParserVariantAttribute"));

			case QualifiedNameSyntax qualified when Variants.Contains(qualified.Right.Identifier.ValueText):
				return IsDotGramNamespace(model.GetSymbolInfo(qualified.Left, cancellationToken).Symbol);

			case IdentifierNameSyntax alias:
				// The alias in view of that name: C# gives none for a name it found nothing behind.
				return model.LookupNamespacesAndTypes(alias.SpanStart, name: alias.Identifier.ValueText).OfType<IAliasSymbol>().FirstOrDefault() is { } symbol &&
					symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(cancellationToken) is UsingDirectiveSyntax directive &&
					directive.NamespaceOrType is QualifiedNameSyntax target &&
					Variants.Contains(target.Right.Identifier.ValueText) &&
					IsDotGramNamespace(model.Compilation.GetSemanticModel(directive.SyntaxTree).GetSymbolInfo(target.Left, cancellationToken).Symbol);

			default:
				return false;
		}
	}

	/// <summary>Whether a symbol is DotGram's namespace, the one at the root the generator emits into.</summary>
	static bool IsDotGramNamespace(ISymbol? symbol)
	{
		return symbol is INamespaceSymbol { ContainingNamespace.IsGlobalNamespace: true } space && space.Name == GramMetadataNames.Namespace;
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
