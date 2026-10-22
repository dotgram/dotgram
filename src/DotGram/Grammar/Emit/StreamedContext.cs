using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;

namespace DotGram.Grammar.Emit;

public static partial class CSharpEmitter
{
	/// <summary>
	/// An explicit assignment, a compound assignment or <c>++</c>/<c>--</c> of something reached through
	/// <c>context</c>, written in a construction.
	/// </summary>
	/// <remarks>
	/// Syntactic, and said as best-effort: a string that spells one is taken for one, and a change made
	/// by calling a method of the context is not seen at all.
	/// </remarks>
	static readonly Regex ContextChange = new(
		@"(?<![\w.])context\s*(?:\.\s*\w+|\[[^\]]*\])+\s*(?:\+\+|--|(?:[-+*/%&|^]|<<|>>|\?\?)?=(?![=>]))" +
		@"|(?:\+\+|--)\s*context\s*(?:\.\s*\w+|\[[^\]]*\])",
		RegexOptions.CultureInvariant | RegexOptions.Compiled);

	/// <summary>
	/// Tells a publication that is read from a reader or lines, and reaches a construction that changes the
	/// grammar's context, that a streamed reading may run that construction a different number of times
	/// than a reading of a string does.
	/// </summary>
	static void ContextChanges(RecognitionGraph graph, Publication publication, ICollection<GramDiagnostic> diagnostics)
	{
		if (graph.Context is null)
			return;

		foreach (var rule in graph.Reaches(publication.Rule))
		{
			if (!graph.Bodies.TryGetValue(rule, out var body))
				continue;

			foreach (var node in NodeWalk.Descendants(body))
				if (node is Node.Construct { How: Construction.Expression { Text: var text } } &&
					ContextChange.Match(text) is { Success: true } change)
				{
					diagnostics.Add(new GramDiagnostic(
						GramCompiler.StreamedContextChange,
						$"'{publication.MethodName}' has an overload over a reader or lines, and a construction of " +
						$"'{graph.Origin(rule).Name}' changes the context ('{Regex.Replace(change.Value, @"\s+", " ")}'). A streamed " +
						"reading may run a construction a different number of times than a reading of a string does, " +
						"so what it leaves in the context may differ (docs/syntax.md §6.3). Said where the change is " +
						"written in the construction, and best-effort: a change made by a call is not seen.",
						publication.At.Position, publication.At.Length, GramSeverity.Info));

					return;
				}
		}
	}
}
