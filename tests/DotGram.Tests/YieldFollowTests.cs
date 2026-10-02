using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A <c>yield</c> hands out each element as its first reading from where the last one ended
/// (§6.3), in the engine and in the direct reader, held to the reference interpreter.
/// </summary>
/// <remarks>
/// What follows an element is the next element, the end of the input, or nothing at all where
/// the caller stops enumerating: an element is read from a position, demanding nothing after it,
/// as a positional <c>parse</c> is. Compiled as though nothing could follow it, a decision taken
/// on the strength of that — a way back to an optional's skip not kept, a repetition's turn
/// taken as final — refuses or shortens the element's first reading.
/// </remarks>
public sealed class YieldFollowTests
{
	/// <summary>
	/// Bodies an element ends with: turns that share a first character and fail after it, turns
	/// of one length or two, one character, a turn that ends in an optional.
	/// </summary>
	static readonly string[] Bodies =
	[
		"('b' & 'c')",
		"('b' & 'c' | 'b' & 'a')",
		"('b' | 'b' & 'c')",
		"['b'..'c']",
		"('b' & 'c'?)",
	];

	/// <summary>
	/// What stands after that repetition at the end of the element: nothing, a look of either
	/// sign, a refusal of a class or of a rule that is one, the end of input, something that
	/// cannot fail, and things that can refuse while reading nothing.
	/// </summary>
	static readonly string[] Tails =
	[
		"",
		" & ?!'c'",
		" & ?='a'",
		" & ?!'a'",
		" & ?!'b'",
		" & Halt",
		" & 'c'?",
		" & 'c'* & ?!'b'",
		" & ('c' | ?!'b')",
		" & (?!'b' & 'c')?",
		" & eof",
		" & Sub",
	];

	/// <summary>Every body under every repetition before every tail, each its own yielded element.</summary>
	static string Shapes(out int count)
	{
		var text  = new StringBuilder("Halt = ?!['a'..'c']\nSub = ('a' | 'c')?\n");
		var index = 0;

		foreach (var body in Bodies)
			foreach (var repeat in new[] { "?", "*", "+", "{0,2}" })
				foreach (var tail in Tails)
				{
					text.Append($"I{index} : @string = v: ('a' & {body}{repeat}{tail}) => @(v)\n");
					text.Append($"F{index} : @string[] = I{index}*\n");
					text.Append($"parse F{index} as Y{index} yield : @string\n");
					index++;
				}

		count = index;

		return text.ToString();
	}

	/// <summary>The element <c>'a' &amp; ('b' &amp; 'c')?</c> on <c>aba</c> yields its <c>a</c>, then refuses the <c>b</c>.</summary>
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void An_optional_at_the_end_of_an_element_keeps_its_skip(bool direct)
	{
		var assembly = Compiled("I : @string = v: ('a' & ('b' & 'c')?) => @(v)\nF : @string[] = I*\nparse F as Y yield : @string\n", direct);

		Assert.Equal("a, !", Outcome(assembly, "Y", "aba"));
		Assert.Equal("abc, a, end", Outcome(assembly, "Y", "abca"));
	}

	/// <summary>
	/// Every generated element shape, on every short input, yields in both backends the elements
	/// the reference interpreter reads one after another, and fails or ends where it does.
	/// </summary>
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Every_element_shape_yields_its_first_readings(bool direct)
	{
		var grammar  = Shapes(out var count);
		var graph    = Graph(grammar);
		var assembly = Compiled(grammar, direct);
		var wrong    = new List<string>();

		for (var index = 0; index < count; index++)
		{
			var element = graph.Rules.First(rule => rule.Name == $"I{index}");

			foreach (var text in Inputs(['a', 'b', 'c'], 5))
			{
				var expected = Expected(graph, element, text);
				var answer   = Outcome(assembly, $"Y{index}", text);

				if (expected != answer)
					wrong.Add($"I{index} '{text}': expected {expected}, yielded {answer}");
			}
		}

		Assert.True(wrong.Count == 0, $"{wrong.Count} cells differ, the first:\n" + string.Join("\n", wrong.Take(20)) + "\n" + grammar);
	}

	/// <summary>
	/// Element bodies that end in a selector: a captured repetition, optional or choice of literals
	/// before a <c>switch</c> one of whose cases reads nothing. A selector picks its case and does
	/// not try the others, so it can refuse at one place and read nothing at another.
	/// </summary>
	static readonly string[] Selectors =
	[
		"'a' & (y: D)* & switch @(y) { case \"1\": none default: 'z' }",
		"'a' & y: D* & switch @(y) { case \"1\": none default: 'z' }",
		"'a' & (y: D)? & switch @(y) { case \"1\": none default: 'z' }",
		"'a' & (y: D)+ & switch @(y) { case \"1\": none default: 'z' }",
		"'a' & (y: D){0,2} & switch @(y) { case \"12\": none default: 'z' }",
		"'a' & (y: D)* & switch @(y) { case \"12\": 'z' default: none }",
		"'a' & (y: (\"12\" | \"1\")) & switch @(y) { case \"1\": none default: 'z' }",
		"'a' & (y: (\"12\" | \"1\"))? & switch @(y) { case \"12\": 'z' default: none }",
	];

	/// <summary>
	/// An element that ends in a selector yields, one after another, the readings a positional
	/// <c>parse</c> of it gives from where each one began — compiled with the positional follow and
	/// without its split, which is held to the reference interpreter elsewhere; the interpreter
	/// itself does not run C#.
	/// </summary>
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void An_element_ending_in_a_selector_yields_its_positional_readings(bool direct)
	{
		var wrong = new List<string>();

		for (var index = 0; index < Selectors.Length; index++)
		{
			var digits     = Selectors[index].Contains(" D", StringComparison.Ordinal) ? "D = ['0'..'9']\n" : "";
			var element    = $"{digits}I : @string = v: ({Selectors[index]}) => @(v)\n";
			var positional = Compiled(element + "parse I as P\n", direct, positionalFollow: true);
			var yielded    = Compiled(element + "F : @string[] = I*\nparse F as Y yield : @string\n", direct);

			foreach (var text in Inputs(['a', '1', '2', 'z'], 5))
			{
				var parts = new List<string>();
				var at    = 0;

				while (true)
				{
					if (at == text.Length)
					{
						parts.Add("end");

						break;
					}

					var answer = EmittedCode.Answered(positional, "Grammar", "TryP", text, at);

					if (!answer.Read)
					{
						parts.Add("!");

						break;
					}

					parts.Add(text.Substring(at, answer.At - at));
					at = answer.At;
				}

				var expected = string.Join(", ", parts);
				var outcome  = Outcome(yielded, "Y", text);

				if (expected != outcome)
					wrong.Add($"{Selectors[index]} '{text}': expected {expected}, yielded {outcome}");
			}
		}

		Assert.True(wrong.Count == 0, $"{wrong.Count} cells differ, the first:\n" + string.Join("\n", wrong.Take(20)));
	}

	/// <summary>The elements the reference interpreter reads one after another, then <c>end</c> or <c>!</c>.</summary>
	static string Expected(RecognitionGraph graph, RuleSymbol element, string text)
	{
		var parts = new List<string>();
		var at    = 0;

		while (at < text.Length)
		{
			var end = ReferenceInterpreter.Reads(graph, element, text, at);

			if (end < 0)
			{
				parts.Add("!");

				return string.Join(", ", parts);
			}

			parts.Add(text.Substring(at, end - at));
			at = end;
		}

		parts.Add("end");

		return string.Join(", ", parts);
	}

	/// <summary>What the generated enumeration yields, then <c>end</c> or <c>!</c> where it throws.</summary>
	static string Outcome(Assembly assembly, string method, string text)
	{
		var parts    = new List<string>();
		var sequence = (IEnumerable)assembly.GetType("Grammar")!.GetMethod(method, [typeof(string)])!.Invoke(null, [text])!;
		var iterator = sequence.GetEnumerator();

		while (true)
		{
			try
			{
				if (!iterator.MoveNext())
					break;
			}
			catch (FormatException)
			{
				parts.Add("!");

				return string.Join(", ", parts);
			}

			parts.Add((string)iterator.Current!);
		}

		parts.Add("end");

		return string.Join(", ", parts);
	}

	static RecognitionGraph Graph(string grammar)
	{
		return GrammarNormalizer.Normalize(
			GrammarBinder.Bind(GramParser.Parse(GramLexer.Tokenize(grammar, RoslynCSharpScanner.Instance)).File));
	}

	static Assembly Compiled(string grammar, bool direct, bool positionalFollow = false)
	{
		var result = GramCompiler.Compile(
			grammar,
			new GramCompilerOptions
			{
				ClassName        = "Grammar",
				Direct           = direct,
				PositionalFollow = positionalFollow,
				CSharpScanner    = RoslynCSharpScanner.Instance,
			});

		EmittedCode.Quiet(result.Diagnostics);

		return EmittedCode.Compile(result.Sources[0].Text);
	}

	/// <summary>Every string over <paramref name="letters"/> no longer than <paramref name="longest"/>.</summary>
	static IEnumerable<string> Inputs(char[] letters, int longest)
	{
		var level = new List<string> { "" };

		for (var length = 0; length <= longest; length++)
		{
			foreach (var text in level)
				yield return text;

			level = [.. level.SelectMany(text => letters.Select(letter => text + letter))];
		}
	}
}
