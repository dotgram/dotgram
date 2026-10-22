using System;
using System.Collections.Immutable;
using System.Globalization;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotGram.Cli;

/// <summary>A name the C# compiler could not resolve, and where.</summary>
/// <param name="Through">
/// The type a grammar asks about whose declaration the name is in — its base list or a member's
/// signature — or, for one it depends on, that type; null where the grammar names it itself.
/// </param>
readonly record struct UnresolvedName(string Name, Diagnostic Diagnostic, string? Through = null)
{
	public string Where => Report.Place(Diagnostic.Location);
}

/// <summary>
/// The names the compilation cannot resolve, split into those a grammar asks about — where the
/// generator's answer would differ from a build that has them — and the rest, which change nothing
/// the generator writes.
/// </summary>
static class Unresolved
{
	/// <summary>The compiler's "cannot find this type or namespace" family.</summary>
	static readonly ImmutableHashSet<string> Ids = ["CS0246", "CS0234", "CS0400", "CS0012"];

	/// <summary>The diagnostics of the given trees of the generator-updated compilation, each tree's own only.</summary>
	/// <remarks>
	/// Never the whole compilation's diagnostics: those bind every generated line as well, which on
	/// a grammar of a thousand rules costs as much again as generating it.
	/// </remarks>
	public static List<Diagnostic> Diagnostics(Compilation updated, IEnumerable<SyntaxTree> trees)
	{
		var all  = new List<Diagnostic>();
		var gate = new object();

		Parallel.ForEach(trees, tree =>
		{
			var mine = updated.GetSemanticModel(tree).GetDiagnostics();

			lock (gate)
				all.AddRange(mine);
		});

		return all;
	}

	/// <summary>The names the diagnostics say cannot be found, in the order of their places.</summary>
	public static List<UnresolvedName> Of(IEnumerable<Diagnostic> diagnostics)
	{
		var found = new List<UnresolvedName>();

		foreach (var diagnostic in diagnostics)
			if (Ids.Contains(diagnostic.Id) && NameOf(diagnostic) is { } name)
				found.Add(new UnresolvedName(name, diagnostic));

		found.Sort(static (a, b) => string.CompareOrdinal(a.Where, b.Where));

		return found;
	}

	/// <summary>
	/// The simple name a diagnostic is about: the first quoted name of its message, in English,
	/// with the generic arity and any qualifier taken off.
	/// </summary>
	static string? NameOf(Diagnostic diagnostic)
	{
		var message = diagnostic.GetMessage(CultureInfo.InvariantCulture);
		var open    = message.IndexOf('\'');
		var close   = open < 0 ? -1 : message.IndexOf('\'', open + 1);

		if (close < 0)
			return null;

		var name = message.Substring(open + 1, close - open - 1);

		// CS0012 names the type in full, CS0246 a generic as `List<>`.
		var generic = name.IndexOf('<');

		if (generic >= 0)
			name = name.Substring(0, generic);

		var dot = name.LastIndexOf('.');

		return dot >= 0 ? name.Substring(dot + 1) : name;
	}

	/// <summary>
	/// Those of <paramref name="names"/> that can change what is generated, and the rest: a name a
	/// grammar asks about, and a name in the declaration of a type it asks about or of one such a type
	/// depends on (<see cref="Dependencies"/>) outside any body — its base list, its interfaces, the
	/// signature of a member — where it decides what the type is, fits or is built with.
	/// </summary>
	public static (List<UnresolvedName> Asked, List<UnresolvedName> Other) Split(
		IEnumerable<UnresolvedName> names, ImmutableHashSet<string> asked, ImmutableHashSet<string> dependencies)
	{
		var yes = new List<UnresolvedName>();
		var no  = new List<UnresolvedName>();

		foreach (var name in names)
		{
			if (asked.Contains(name.Name))
				yes.Add(name);
			else if (SignatureOf(name.Diagnostic) is { } type && dependencies.Contains(type))
				yes.Add(name with { Through = type });
			else
				no.Add(name);
		}

		return (yes, no);
	}

	/// <summary>
	/// The types a grammar asks about, and every type their declarations name outside a body, and so
	/// on to the end: what a question's answer can depend on. Found from the syntax of the given trees
	/// by simple name, so it is wider than the answer's real dependencies and never narrower.
	/// </summary>
	public static ImmutableHashSet<string> Dependencies(IEnumerable<SyntaxTree> trees, ImmutableHashSet<string> asked)
	{
		var declared = new Dictionary<string, List<BaseTypeDeclarationSyntax>>(StringComparer.Ordinal);

		foreach (var tree in trees)
			foreach (var type in tree.GetRoot().DescendantNodes().OfType<BaseTypeDeclarationSyntax>())
			{
				if (!declared.TryGetValue(type.Identifier.ValueText, out var all))
					declared[type.Identifier.ValueText] = all = [];

				all.Add(type);
			}

		var found = ImmutableHashSet.CreateBuilder<string>(StringComparer.Ordinal);
		var next  = new Queue<string>(asked.Where(declared.ContainsKey));

		while (next.Count > 0)
		{
			var name = next.Dequeue();

			if (!found.Add(name))
				continue;

			foreach (var type in declared[name])
				foreach (var used in type.DescendantNodes(node => node == type || !IsBody(node) && node is not BaseTypeDeclarationSyntax).OfType<SimpleNameSyntax>())
					if (declared.ContainsKey(used.Identifier.ValueText) && !found.Contains(used.Identifier.ValueText))
						next.Enqueue(used.Identifier.ValueText);
		}

		return found.ToImmutable();
	}

	/// <summary>The type whose declaration the diagnostic is in, outside any body; null inside one, or outside a type.</summary>
	static string? SignatureOf(Diagnostic diagnostic)
	{
		if (diagnostic.Location.SourceTree is not { } tree)
			return null;

		for (SyntaxNode? node = tree.GetRoot().FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true); node is not null; node = node.Parent)
		{
			if (node is BaseTypeDeclarationSyntax type)
				return type.Identifier.ValueText;

			if (IsBody(node))
				return null;
		}

		return null;
	}

	/// <summary>What runs rather than declares: a body, an initializer, an attribute's arguments.</summary>
	static bool IsBody(SyntaxNode node)
	{
		return node is BlockSyntax or ArrowExpressionClauseSyntax or EqualsValueClauseSyntax or AttributeListSyntax;
	}
}
