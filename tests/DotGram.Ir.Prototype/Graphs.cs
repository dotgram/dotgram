using System;

using System.IO;
using System.Reflection;

using DotGram.Grammar;
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

	/// <summary>
	/// A shipped grammar as the generator normalizes it: the standard library spliced on, bound
	/// and normalized for C#, and nothing asked of a compilation — a type the grammar names is
	/// taken as written.
	/// </summary>
	public static RecognitionGraph OfShipped(string grammar)
	{
		var language = new CSharpLanguage(null, RoslynCSharpScanner.Instance);
		var parsed   = GramCompiler.Read(language, grammar, null, null);
		var model    = GrammarBinder.BindIn(parsed.File, language, grammar.Length);

		return GrammarNormalizer.NormalizeIn(model, language);
	}

	/// <summary>The repository's root, which the build writes into every test assembly.</summary>
	public static string Repository { get; } =
		typeof(Graphs).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().First(static one => one.Key == "DotGramSourceRoot").Value!;

	/// <summary>The grammar a class of DotGram.Web carries in its attribute, as text.</summary>
	public static string WebGrammar(string file)
	{
		var source = File.ReadAllText(Path.Combine(Repository, "src", "DotGram.Web", file)).ReplaceLineEndings("\n");
		var from   = source.IndexOf("[Gram(\"\"\"\n", StringComparison.Ordinal) + "[Gram(\"\"\"\n".Length;
		var to     = source.IndexOf("\n\t\"\"\"", from, StringComparison.Ordinal);

		// The raw string's own indentation is one tab, which is not the grammar's.
		return string.Join("\n", source[from..to].Split('\n').Select(static line => line.StartsWith('\t') ? line[1..] : line));
	}
}
