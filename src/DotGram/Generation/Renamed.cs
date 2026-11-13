using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
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
/// <param name="Host">
/// The class whose attribute it is written in, by <see cref="GramGenerator.IdentityOf"/>, where it is one
/// written on a class: what GRAM4059 is then not said of. Null elsewhere.
/// </param>
readonly record struct RenamedUse(string Old, string Now, Place? At, string? Host = null);

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
	/// <summary>
	/// How many nodes were bound in the flow that set it: the cost of an edit, which a test holds to the
	/// rare nodes spelled like an old name. Null, and nothing counted, everywhere else.
	/// </summary>
	internal static readonly AsyncLocal<StrongBox<int>?> Bound = new();

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
	/// <c>[GramOptions]</c> or with a named argument an old option was called, or a <c>Tape</c>. Every
	/// node it lets through is bound on every edit, so it lets through only what is rare: an attribute
	/// named by an alias is found apart (<see cref="IsVariantAlias"/>, <see cref="FindThroughAliases"/>).
	/// </summary>
	public static bool IsCandidate(SyntaxNode node)
	{
		return node switch
		{
			AttributeSyntax attribute => Variants.Contains(SimpleName(attribute.Name)) || OldArguments(attribute).Any(),
			MemberAccessExpressionSyntax access => access.Name.Identifier.ValueText == "Tape",

			// `Tape` on its own, reached through `using static DotGram.GramCarrier` (Find).
			IdentifierNameSyntax { Identifier.ValueText: "Tape" } identifier => identifier.Parent is not MemberAccessExpressionSyntax { Name: var name } || name != identifier,
			_ => false,
		};
	}

	/// <summary>Every old name a candidate node is written with, as C# binds it.</summary>
	public static ImmutableArray<RenamedUse> Find(SyntaxNode node, SemanticModel model, CancellationToken cancellationToken)
	{
		if (Bound.Value is { } counted)
			Interlocked.Increment(ref counted.Value);

		var found = ImmutableArray.CreateBuilder<RenamedUse>();

		if (node is IdentifierNameSyntax tape)
		{
			// A value written where nothing of that name is in view, and a `using static` in view, in this
			// file or a global one in any, that binds to DotGram's enum: what the old name would have
			// bound to, whatever else of the new name is in view. Never a type or a namespace, which an
			// enum's member could not have been.
			if (!SyntaxFacts.IsInNamespaceOrTypeContext(tape) &&
				model.GetSymbolInfo(tape, cancellationToken) is { Symbol: null, CandidateSymbols.Length: 0 } &&
				UsingsInView(tape, model.Compilation, cancellationToken)
					.Any(directive => directive.StaticKeyword.IsKind(SyntaxKind.StaticKeyword) &&
						Bind(directive.NamespaceOrType, model.Compilation, cancellationToken) is INamedTypeSymbol carrier &&
						IsDotGram(carrier, "GramCarrier")))
			{
				found.Add(new RenamedUse(GramRenames.OldCarrier, GramRenames.NewCarrier, Place.Of(tape.GetLocation()), HostOf(tape, model, cancellationToken)));
			}
		}
		else if (node is MemberAccessExpressionSyntax access)
		{
			// DotGram's enum, through whatever name or alias reaches it, and no member of that name on it.
			if (model.GetSymbolInfo(access, cancellationToken) is { Symbol: null, CandidateSymbols.Length: 0 } &&
				model.GetSymbolInfo(access.Expression, cancellationToken).Symbol is INamedTypeSymbol carrier &&
				IsDotGram(carrier, "GramCarrier"))
			{
				found.Add(new RenamedUse(GramRenames.OldCarrier, GramRenames.NewCarrier, Place.Of(access.GetLocation()), HostOf(access, model, cancellationToken)));
			}
		}
		else if (node is AttributeSyntax attribute)
		{
			var bound = model.GetSymbolInfo(attribute, cancellationToken);

			var host = HostOf(attribute, model, cancellationToken);

			if (bound is { Symbol: null, CandidateSymbols.Length: 0 } && IsOldVariant(attribute.Name, model, cancellationToken))
				found.Add(new RenamedUse(GramRenames.OldVariant, GramRenames.NewVariant, Place.Of(attribute.Name.GetLocation()), host));
			else if ((bound.Symbol ?? bound.CandidateSymbols.FirstOrDefault()) is IMethodSymbol { ContainingType: var type } &&
				RenamesOf(type) is { } renames)
			{
				foreach (var named in OldArguments(attribute))
					if (renames.TryGetValue(named.Identifier.ValueText, out var now) && type.GetMembers(named.Identifier.ValueText).IsEmpty)
						found.Add(new RenamedUse(named.Identifier.ValueText, now, Place.Of(named.GetLocation()), host));
			}
		}

		return found.ToImmutable();
	}

	/// <summary>
	/// The class an attribute is written on, by <see cref="GramGenerator.IdentityOf"/>, where the node is
	/// in one written on a class — the attribute itself, or a value in its arguments such as
	/// <c>GramCarrier.Tape</c>; null elsewhere.
	/// </summary>
	static string? HostOf(SyntaxNode node, SemanticModel model, CancellationToken cancellationToken)
	{
		return node.FirstAncestorOrSelf<AttributeSyntax>()?.Parent?.Parent is ClassDeclarationSyntax declaration &&
			model.GetDeclaredSymbol(declaration, cancellationToken) is { } declared
				? GramGenerator.IdentityOf(declared)
				: null;
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
	/// Whether an attribute's name, which C# did not bind, is the old <c>[GramOptions]</c>, from what the old
	/// name would have bound to: an alias of that name whose target is qualified by DotGram's namespace,
	/// whatever the alias is called; else the old name written where DotGram's namespace is in view (a
	/// <c>using</c> in this file or a global one, or a namespace it is written in), or qualified by a
	/// name that binds to that namespace.
	/// </summary>
	static bool IsOldVariant(NameSyntax name, SemanticModel model, CancellationToken cancellationToken)
	{
		switch (name)
		{
			case IdentifierNameSyntax simple:
				// The alias in view of that name, or with C#'s `Attribute` after it as an attribute's name
				// may be; C# gives one for a target it found nothing behind too.
				if ((Alias(simple, simple.Identifier.ValueText) ?? Alias(simple, simple.Identifier.ValueText + "Attribute")) is { } alias)
				{
					return alias.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(cancellationToken) is UsingDirectiveSyntax directive &&
						directive.NamespaceOrType is QualifiedNameSyntax target &&
						Variants.Contains(target.Right.Identifier.ValueText) &&
						IsDotGramNamespace(Bind(target.Left, model.Compilation, cancellationToken));
				}

				return Variants.Contains(simple.Identifier.ValueText) && SeesDotGram(simple, model, cancellationToken);

			case QualifiedNameSyntax qualified when Variants.Contains(qualified.Right.Identifier.ValueText):
				return IsDotGramNamespace(model.GetSymbolInfo(qualified.Left, cancellationToken).Symbol);

			default:
				return false;
		}

		IAliasSymbol? Alias(SyntaxNode at, string alias)
		{
			return model.LookupNamespacesAndTypes(at.SpanStart, name: alias).OfType<IAliasSymbol>().FirstOrDefault();
		}
	}

	/// <summary>
	/// Whether DotGram's namespace is in view at a name by its members' simple names: the name written in
	/// it, or a plain <c>using</c> in view that binds to it.
	/// </summary>
	static bool SeesDotGram(SyntaxNode node, SemanticModel model, CancellationToken cancellationToken)
	{
		for (var space = model.GetEnclosingSymbol(node.SpanStart, cancellationToken)?.ContainingNamespace; space is { IsGlobalNamespace: false }; space = space.ContainingNamespace)
			if (IsDotGramNamespace(space))
				return true;

		return UsingsInView(node, model.Compilation, cancellationToken)
			.Any(directive => directive.Alias is null &&
				!directive.StaticKeyword.IsKind(SyntaxKind.StaticKeyword) &&
				IsDotGramNamespace(Bind(directive.NamespaceOrType, model.Compilation, cancellationToken)));
	}

	/// <summary>
	/// The <c>using</c> directives in view at a node: those of the file and the namespaces around it,
	/// and every file's global ones.
	/// </summary>
	static IEnumerable<UsingDirectiveSyntax> UsingsInView(SyntaxNode node, Compilation compilation, CancellationToken cancellationToken)
	{
		foreach (var scope in node.Ancestors())
		{
			var usings = scope switch
			{
				CompilationUnitSyntax unit           => unit.Usings,
				BaseNamespaceDeclarationSyntax space => space.Usings,
				_                                    => default,
			};

			foreach (var directive in usings)
				yield return directive;
		}

		foreach (var tree in compilation.SyntaxTrees)
			if (tree != node.SyntaxTree && tree.GetRoot(cancellationToken) is CompilationUnitSyntax unit)
				foreach (var directive in unit.Usings)
					if (directive.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword))
						yield return directive;
	}

	/// <summary>What a name in any file of the compilation binds to.</summary>
	static ISymbol? Bind(SyntaxNode name, Compilation compilation, CancellationToken cancellationToken)
	{
		return compilation.GetSemanticModel(name.SyntaxTree).GetSymbolInfo(name, cancellationToken).Symbol;
	}

	/// <summary>
	/// Whether a node is a <c>using</c> alias whose target is named like the old <c>[GramOptions]</c>, by
	/// syntax alone: those are rare, and their names are the only simple names an attribute
	/// <see cref="FindThroughAliases"/> binds.
	/// </summary>
	public static bool IsVariantAlias(SyntaxNode node)
	{
		return node is UsingDirectiveSyntax { Alias: not null, NamespaceOrType: NameSyntax target } && Variants.Contains(SimpleName(target));
	}

	/// <summary>The name a <c>using</c> alias <see cref="IsVariantAlias"/> lets through is written under.</summary>
	public static string AliasName(SyntaxNode node)
	{
		return ((UsingDirectiveSyntax)node).Alias!.Name.Identifier.ValueText;
	}

	/// <summary>
	/// The old <c>[GramOptions]</c> written through an alias of another name: every attribute of the
	/// compilation named by one of those aliases, which <see cref="IsCandidate"/> does not already let
	/// through, as C# binds it. Nothing at all, and no binding, while no such alias is written.
	/// </summary>
	public static ImmutableArray<RenamedUse> FindThroughAliases(EquatableArray<string> aliases, Compilation compilation, CancellationToken cancellationToken)
	{
		if (aliases.Items.IsDefaultOrEmpty)
			return ImmutableArray<RenamedUse>.Empty;

		var names = new HashSet<string>(aliases.Items, StringComparer.Ordinal);
		var found = ImmutableArray.CreateBuilder<RenamedUse>();

		foreach (var tree in compilation.SyntaxTrees)
		{
			SemanticModel? model = null;

			foreach (var attribute in tree.GetRoot(cancellationToken).DescendantNodes().OfType<AttributeSyntax>())
				if (attribute.Name is IdentifierNameSyntax { Identifier.ValueText: var name } &&
					(names.Contains(name) || names.Contains(name + "Attribute")) &&
					!IsCandidate(attribute))
					found.AddRange(Find(attribute, model ??= compilation.GetSemanticModel(tree), cancellationToken));
		}

		return found.ToImmutable();
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
