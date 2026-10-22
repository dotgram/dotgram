using System;
using System.Collections.Immutable;
using System.Globalization;

using Microsoft.CodeAnalysis;

namespace DotGram.Cli;

/// <summary>A name the C# compiler could not resolve, and where.</summary>
readonly record struct UnresolvedName(string Name, Diagnostic Diagnostic)
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

	/// <summary>Those of <paramref name="names"/> a grammar asks about, and the rest.</summary>
	public static (List<UnresolvedName> Asked, List<UnresolvedName> Other) Split(
		IEnumerable<UnresolvedName> names, ImmutableHashSet<string> asked)
	{
		var yes = new List<UnresolvedName>();
		var no  = new List<UnresolvedName>();

		foreach (var name in names)
			(asked.Contains(name.Name) ? yes : no).Add(name);

		return (yes, no);
	}
}
