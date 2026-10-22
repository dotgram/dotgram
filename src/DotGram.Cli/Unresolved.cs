using System;
using System.Collections.Immutable;
using System.Globalization;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotGram.Cli;

/// <summary>A name the C# compiler could not resolve, and where.</summary>
readonly record struct UnresolvedName(string Name, Diagnostic Diagnostic)
{
	public string Where => Report.Place(Diagnostic.Location);
}

/// <summary>
/// The names a project's build would report it cannot resolve: those the compilation the tool checks
/// cannot resolve — the user's files and what the project's other generators add — and that DotGram's
/// own output does not supply either.
/// </summary>
static class Unresolved
{
	/// <summary>The compiler's "cannot find this type or namespace" family, a type nested in another included.</summary>
	static readonly ImmutableHashSet<string> Ids = ["CS0246", "CS0234", "CS0400", "CS0426", "CS0012"];

	/// <summary>
	/// The names the project's build would report it cannot resolve. Looked for in the compilation the
	/// build has before DotGram's files are added — the user's files and the other generators' — each
	/// tree's own diagnostics only; a name is excused only where it binds once DotGram's files are
	/// there: the parser a host's code calls, the attribute it carries.
	/// </summary>
	/// <param name="written">The trees the user wrote, the synthesized host of a <c>.gram</c> among them.</param>
	/// <returns>The names, and how long finding the diagnostics and then excusing took.</returns>
	public static (List<UnresolvedName> Names, TimeSpan Diagnostics, TimeSpan Excusing) Find(IEnumerable<SyntaxTree> written, GeneratorOutput generated)
	{
		var clock       = System.Diagnostics.Stopwatch.StartNew();
		var diagnostics = Diagnostics(generated.Checked, written.Concat(generated.OtherTrees));
		var found       = clock.Elapsed;
		var names       = Of(diagnostics, generated.Updated);

		return (names, found, clock.Elapsed - found);
	}

	/// <summary>The diagnostics of the given trees of a compilation, each tree's own only.</summary>
	/// <remarks>
	/// Never the whole compilation's diagnostics, which would bind every generated parser's lines as
	/// well: on a grammar of a thousand rules that costs as much again as generating it.
	/// </remarks>
	public static List<Diagnostic> Diagnostics(Compilation compilation, IEnumerable<SyntaxTree> trees)
	{
		var all  = new List<Diagnostic>();
		var gate = new object();

		Parallel.ForEach(trees, tree =>
		{
			var mine = compilation.GetSemanticModel(tree).GetDiagnostics();

			lock (gate)
				all.AddRange(mine);
		});

		return all;
	}

	/// <summary>
	/// The names the diagnostics say cannot be found, less those that resolve once DotGram's files are
	/// in the compilation — the parser class a host's code calls, the attribute its host carries — in
	/// the order of their places.
	/// </summary>
	/// <param name="updated">The compilation with DotGram's files in it, holding the same trees.</param>
	public static List<UnresolvedName> Of(IEnumerable<Diagnostic> diagnostics, Compilation updated)
	{
		var found  = new List<UnresolvedName>();
		var models = new Dictionary<SyntaxTree, SemanticModel>();

		foreach (var diagnostic in diagnostics)
		{
			if (!Ids.Contains(diagnostic.Id) || NameOf(diagnostic) is not { } name)
				continue;

			if (diagnostic.Id != "CS0012" && diagnostic.Location.SourceTree is { } tree && Resolves(tree, diagnostic.Location.SourceSpan, updated, models))
				continue;

			found.Add(new UnresolvedName(name, diagnostic));
		}

		found.Sort(static (a, b) => string.CompareOrdinal(a.Where, b.Where));

		return found;
	}

	/// <summary>Whether the name at the span binds in the compilation with DotGram's files, where it stands.</summary>
	static bool Resolves(SyntaxTree tree, Microsoft.CodeAnalysis.Text.TextSpan span, Compilation updated, Dictionary<SyntaxTree, SemanticModel> models)
	{
		if (!models.TryGetValue(tree, out var model))
			models[tree] = model = updated.GetSemanticModel(tree);

		var node = tree.GetRoot().FindNode(span, getInnermostNodeForTie: true);

		// The whole of a qualified name, `Foreign.Demo` rather than its last part.
		while (node.Parent is QualifiedNameSyntax qualified && qualified.Right == node || node.Parent is AliasQualifiedNameSyntax { } alias && alias.Name == node)
			node = node.Parent;

		var info = model.GetSymbolInfo(node);

		return info.Symbol is not null;
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
}
