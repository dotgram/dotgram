using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

namespace DotGram.Tests;

/// <summary>
/// A grammar's published rules asked the same questions in every rendering and held to the
/// reference interpreter: what NestedLookaheadTests asks of the shapes found, and the sweep in
/// DotGram.Tests.Slow of all of them.
/// </summary>
static class NestedLookahead
{
	/// <summary>
	/// The engine; the direct reader carried on the tape and immediately; the forms over text,
	/// bytes, a reader and a stream read through a window of one; compiled for positional
	/// readings, the reading from every position; and over tokens, the engine and the reader.
	/// </summary>
	public static readonly string[] Renderings =
		["engine", "tape", "immediate", "buffered", "positional", "tokens", "tokens-direct"];

	/// <summary>
	/// Every answer that differs from the reference interpreter's, said with the rule, the input
	/// and the form. Each rule is published with <c>stream bytes</c>.
	/// </summary>
	/// <param name="reference">
	/// The grammar the reference interpreter reads, where it is not <paramref name="grammar"/>:
	/// over tokens, the same grammar with its words held to one reading, which is what the lexer
	/// makes of them and what the interpreter, which reads characters, has to be told.
	/// </param>
	public static List<string> Disagreements(
		string grammar, string rendering, IReadOnlyList<(string Rule, string Shown, IReadOnlyList<string> Inputs)> rules,
		string? reference = null)
	{
		var tokens = rendering.StartsWith("tokens", StringComparison.Ordinal);
		var result = GramCompiler.Compile(
			grammar,
			new GramCompilerOptions
			{
				ClassName        = "Grammar",
				CSharpScanner    = RoslynCSharpScanner.Instance,
				Lexical          = tokens,
				Direct           = rendering is not ("engine" or "tokens"),
				Carrier          = rendering switch
				{
					"tape"      => CarrierKind.Tape,
					"immediate" => CarrierKind.Immediate,
					_           => CarrierKind.Auto,
				},
				BufferedInput    = rendering == "buffered",
				PositionalFollow = rendering == "positional",
			});

		EmittedCode.Quiet(result.Diagnostics);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);
		var host     = assembly.GetType("Grammar")!;
		var graph    = Graph(reference ?? grammar);
		var wrong    = new List<string>();

		foreach (var (rule, shown, inputs) in rules)
		{
			var method = $"TryParse{rule}";
			var start  = graph.Rules.First(one => one.Name == rule);

			foreach (var input in inputs)
			{
				var expected = ReferenceInterpreter.Parses(graph, start, input);

				void Check(string form, bool answer)
				{
					if (answer != expected)
						wrong.Add($"{rule} = {shown}, \"{input}\", {rendering} {form}: expected {expected}");
				}

				Check("text", EmittedCode.Match(assembly, "Grammar", method, input).IsSuccess);

				if (tokens)
					continue;

				Check("bytes", Succeeded(host.GetMethod(method, [typeof(byte[])])!.Invoke(null, [Encoding.ASCII.GetBytes(input)])!));

				if (rendering == "buffered")
				{
					Check("reader", Succeeded(host.GetMethod(method, [typeof(TextReader), typeof(int?), typeof(int?)])!
						.Invoke(null, [new StringReader(input), 1, null])!));
					Check("stream", Succeeded(host.GetMethod(method, [typeof(Stream), typeof(int?), typeof(int?)])!
						.Invoke(null, [new MemoryStream(Encoding.ASCII.GetBytes(input)), 1, null])!));
				}

				// Offered where the rule is not lowered to the one entry a whole parse asks for (§6.3).
				if (rendering == "positional" && host.GetMethod(method, [typeof(string), typeof(int)]) is not null)
					for (var at = 0; at <= input.Length; at++)
					{
						var end    = ReferenceInterpreter.Reads(graph, start, input, at);
						var answer = EmittedCode.Answered(assembly, "Grammar", method, input, at);

						if (answer.Read != end >= 0 || answer.Read && answer.At != end)
							wrong.Add($"{rule} = {shown}, \"{input}\" from {at}: expected {end}, answered {(answer.Read ? answer.At : -1)}");
					}
			}
		}

		return wrong;
	}

	/// <summary>Every text over an alphabet up to a length, the empty one included.</summary>
	public static List<string> Inputs(string alphabet, int longest)
	{
		var all = new List<string> { "" };

		for (var length = 1; length <= longest; length++)
			all.AddRange(all.Where(one => one.Length == length - 1).SelectMany(one => alphabet.Select(c => one + c)).ToList());

		return all;
	}

	static bool Succeeded(object match)
	{
		return (bool)match.GetType().GetProperty("IsSuccess")!.GetValue(match)!;
	}

	static RecognitionGraph Graph(string grammar)
	{
		return GrammarNormalizer.Normalize(
			GrammarBinder.Bind(GramParser.Parse(GramLexer.Tokenize(grammar, RoslynCSharpScanner.Instance)).File));
	}
}
