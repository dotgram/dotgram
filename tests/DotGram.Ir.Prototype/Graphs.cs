using System;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;
using DotGram.Language;

namespace DotGram.Ir.Prototype;

/// <summary>A grammar text taken to the generator's recognition graph, as the generator takes it.</summary>
static class Graphs
{
	public static RecognitionGraph Of(string grammar)
	{
		var graph = GrammarNormalizer.Normalize(
			GrammarBinder.Bind(
				GramParser.Parse(GramLexer.Tokenize(grammar, RoslynCSharpScanner.Instance)).File),
			scanner: RoslynCSharpScanner.Instance);

		if (graph.HasErrors)
			throw new InvalidOperationException(string.Join("\n", graph.Diagnostics));

		return graph;
	}
}
