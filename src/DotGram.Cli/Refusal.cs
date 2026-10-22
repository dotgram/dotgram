using System;

using Microsoft.CodeAnalysis;

namespace DotGram.Cli;

/// <summary>
/// The errors a project's build would report in its own code, found before anything is written: a
/// generation that the build would refuse is refused.
/// </summary>
static class Refusal
{
	/// <summary>
	/// The errors in the trees that are not DotGram's output — what the user wrote and what the
	/// project's other generators add. First each tree's own diagnostics in the compilation the build
	/// has without DotGram's files; a tree with an error is then checked again in the full compilation,
	/// where what DotGram adds (the attribute a host carries, the parser its code calls) can resolve
	/// what was missing. What is left is an error the build reports as well, at the compiler's default severities.
	/// </summary>
	/// <param name="written">The trees the user wrote, the synthesized host of a <c>.gram</c> among them.</param>
	/// <returns>The errors, in the order of their places, and how long the two passes took.</returns>
	public static (List<Diagnostic> Errors, TimeSpan Elapsed) Errors(IEnumerable<SyntaxTree> written, GeneratorOutput generated)
	{
		var clock  = System.Diagnostics.Stopwatch.StartNew();
		var cheap  = Diagnostics(generated.Checked, written.Concat(generated.OtherTrees));
		var failed = cheap
			.Where(static diagnostic => IsError(diagnostic) && diagnostic.Location.SourceTree is not null)
			.Select(static diagnostic => diagnostic.Location.SourceTree!)
			.Distinct()
			.ToList();

		var errors = Of(Diagnostics(generated.Updated, failed));

		// An error a generator reported itself: the build fails on it as on any other.
		errors.AddRange(generated.OtherErrors);

		return (errors, clock.Elapsed);
	}

	/// <summary>
	/// The errors in DotGram's own output when it is compiled in the project: needed only where the
	/// project's DotGram is not the tool's, and its runtime may not be what the tool's output compiles with.
	/// </summary>
	public static List<Diagnostic> OutputErrors(GeneratorOutput generated)
	{
		return Of(Diagnostics(generated.Updated, generated.Trees.Values));
	}

	static List<Diagnostic> Of(IEnumerable<Diagnostic> diagnostics)
	{
		return [.. diagnostics
			.Where(static diagnostic => IsError(diagnostic))
			.OrderBy(static diagnostic => Report.Format(diagnostic), StringComparer.Ordinal)];
	}

	/// <summary>
	/// Whether the compiler reports the diagnostic as an error by default. The severities a build
	/// applies on top of that (<c>/warnaserror</c>, <c>/nowarn</c>, a ruleset, <c>.editorconfig</c>) are
	/// not applied: the check is not a build, and half of a build's severities would contradict that.
	/// </summary>
	static bool IsError(Diagnostic diagnostic)
	{
		return diagnostic.DefaultSeverity == DiagnosticSeverity.Error;
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
}
