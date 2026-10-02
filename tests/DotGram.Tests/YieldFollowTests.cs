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
/// (§6.3), held to the reference interpreter, over a string and over a reader and a stream.
/// </summary>
/// <remarks>
/// What follows an element is the next element, the end of the input, or nothing at all where
/// the caller stops enumerating: an element is read from a position, demanding nothing after it,
/// as a positional <c>parse</c> is. Compiled as though nothing could follow it, a decision taken
/// on the strength of that — a way back to an optional's skip not kept, a repetition's turn
/// taken as final — refuses or shortens the element's first reading.
/// <para>
/// A yield over a string is always read by the engine; over a reader or a stream it is read by the
/// direct reader where that was asked for, a step a call (Machine.Reader's EmitYieldStep), and by
/// the engine otherwise. Each test runs both and asserts which one it ran.
/// </para>
/// </remarks>
public sealed class YieldFollowTests
{
	/// <summary>An element's text, from a string, a span of characters or a span of bytes.</summary>
	const string Helpers = """
		static string Text(string text) { return text; }
		static string Text(global::System.ReadOnlySpan<char> text) { return text.ToString(); }
		static string Text(global::System.ReadOnlySpan<byte> text) { return global::System.Text.Encoding.ASCII.GetString(text.ToArray()); }
		""";

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
					text.Append($"I{index} : @string = v: ('a' & {body}{repeat}{tail}) => @(Text(v))\n");
					text.Append($"F{index} : @string[] = I{index}*\n");
					text.Append(Published(index));
					index++;
				}

		count = index;

		return text.ToString();
	}

	/// <summary>
	/// The publications of collection <c>F{index}</c>: over a string (<c>Y</c>), a reader (<c>C</c>)
	/// and, unless left out, a stream of bytes (<c>B</c>).
	/// </summary>
	static string Published(int index, bool bytes = true)
	{
		return
			$"parse F{index} as Y{index} yield : @string\n" +
			$"parse F{index} as C{index} stream yield : @string\n" +
			(bytes ? $"parse F{index} as B{index} stream bytes yield : @string\n" : "");
	}

	/// <summary>The forms each collection is published in (<see cref="Published"/>).</summary>
	static readonly string[] Forms = ["Y", "C", "B"];

	/// <summary>The element <c>'a' &amp; ('b' &amp; 'c')?</c> on <c>aba</c> yields its <c>a</c>, then refuses the <c>b</c>.</summary>
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void An_optional_at_the_end_of_an_element_keeps_its_skip(bool direct)
	{
		var (assembly, source) = Compiled("I0 : @string = v: ('a' & ('b' & 'c')?) => @(Text(v))\nF0 : @string[] = I0*\n" + Published(0), direct);

		Backend(source, 0, direct);

		foreach (var form in Forms)
		{
			Assert.Equal("a, !", Outcome(assembly, form + "0", "aba"));
			Assert.Equal("abc, a, end", Outcome(assembly, form + "0", "abca"));
		}
	}

	/// <summary>
	/// Every generated element shape, on every short input, yields in every form the elements the
	/// reference interpreter reads one after another, and fails or ends where it does.
	/// </summary>
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Every_element_shape_yields_its_first_readings(bool direct)
	{
		var grammar  = Shapes(out var count);
		var graph    = Graph(grammar);
		var (assembly, source) = Compiled(grammar, direct);
		var wrong              = new List<string>();

		for (var index = 0; index < count; index++)
		{
			var element = graph.Rules.First(rule => rule.Name == $"I{index}");

			Backend(source, index, direct);

			foreach (var text in Inputs(['a', 'b', 'c'], 5))
			{
				var expected = Expected(graph, element, text, recovers: false);

				foreach (var form in Forms)
				{
					var answer = Outcome(assembly, form + index, text);

					if (expected != answer)
						wrong.Add($"I{index} {form} '{text}': expected {expected}, yielded {answer}");
				}
			}
		}

		Assert.True(wrong.Count == 0, $"{wrong.Count} cells differ, the first:\n" + string.Join("\n", wrong.Take(20)) + "\n" + grammar);
	}

	/// <summary>
	/// The same shapes in a collection marked <c>recover ';'</c>: where an element is refused and input
	/// remains, the bad element is stepped over to just past the next <c>;</c>, or to the end, and
	/// <c>!</c> is yielded in its place. The elements between are the reference interpreter's first
	/// readings, in every form.
	/// </summary>
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Every_recovering_element_shape_yields_its_first_readings(bool direct)
	{
		var text  = new StringBuilder("Sub = ('a' | 'c')?\n");
		var count = 0;

		foreach (var body in Bodies)
			foreach (var repeat in new[] { "?", "*" })
				foreach (var tail in new[] { "", " & ?!'c'", " & 'c'?", " & Sub" })
				{
					text.Append($"I{count} : @string = v: ('a' & {body}{repeat}{tail}) => @(Text(v))\n");
					text.Append($"F{count} : @string[] = I{count}* recover ';' => @(\"!\")\n");
					text.Append(Published(count));
					count++;
				}

		var grammar            = text.ToString();
		var graph              = Graph(grammar);
		var (assembly, source) = Compiled(grammar, direct);
		var wrong              = new List<string>();

		for (var index = 0; index < count; index++)
		{
			var element = graph.Rules.First(rule => rule.Name == $"I{index}");

			Backend(source, index, direct);

			foreach (var input in Inputs(['a', 'b', 'c', ';'], 4))
			{
				var expected = Expected(graph, element, input, recovers: true);

				foreach (var form in Forms)
				{
					var answer = Outcome(assembly, form + index, input);

					if (expected != answer)
						wrong.Add($"I{index} {form} '{input}': expected {expected}, yielded {answer}");
				}
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
	/// itself does not run C#. Over a string and a reader: a selector does not read bytes.
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
			var element    = $"{digits}I : @string = v: ({Selectors[index]}) => @(Text(v))\n";
			var positional = Compiled(element + "parse I as P\n", direct, positionalFollow: true).Assembly;
			var yielded    = Compiled(element.Replace("I :", "I0 :") + "F0 : @string[] = I0*\n" + Published(0, bytes: false), direct);

			// Read by the reader where it was asked for, but for the captured bounded repetition,
			// which the reader is not chosen for: said here, so that the day it is this fails.
			Backend(yielded.Source, 0, direct && !Selectors[index].Contains("{0,2}", StringComparison.Ordinal), bytes: false);

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

				foreach (var form in new[] { "Y", "C" })
				{
					var outcome = Outcome(yielded.Assembly, form + "0", text);

					if (expected != outcome)
						wrong.Add($"{Selectors[index]} {form} '{text}': expected {expected}, yielded {outcome}");
				}
			}
		}

		Assert.True(wrong.Count == 0, $"{wrong.Count} cells differ, the first:\n" + string.Join("\n", wrong.Take(20)));
	}

	/// <summary>
	/// The elements the reference interpreter reads one after another, then <c>end</c> or <c>!</c>;
	/// recovering, a refused element is <c>!</c> and reading resumes past the next <c>;</c>.
	/// </summary>
	static string Expected(RecognitionGraph graph, RuleSymbol element, string text, bool recovers)
	{
		var parts = new List<string>();
		var at    = 0;

		while (at < text.Length)
		{
			var end = ReferenceInterpreter.Reads(graph, element, text, at);

			if (end < 0)
			{
				parts.Add("!");

				if (!recovers)
					return string.Join(", ", parts);

				var stop = text.IndexOf(';', at);

				at = stop < 0 ? text.Length : stop + 1;

				continue;
			}

			parts.Add(text.Substring(at, end - at));
			at = end;
		}

		parts.Add("end");

		return string.Join(", ", parts);
	}

	/// <summary>
	/// Whether the reader and the stream forms of collection <c>F{index}</c> are read by the direct
	/// reader, which is where it was asked for, or by the engine; the string form is the engine's.
	/// </summary>
	static void Backend(string source, int index, bool direct, bool bytes = true)
	{
		Assert.Equal(direct, source.Contains($"int Read_I{index}_Buffered_C{index}(", StringComparison.Ordinal));

		if (bytes)
			Assert.Equal(direct, source.Contains($"int Read_I{index}_Buffered_B{index}_Bytes(", StringComparison.Ordinal));
	}

	/// <summary>
	/// What the generated enumeration yields, then <c>end</c> or <c>!</c> where it throws. A method
	/// named <c>C…</c> is handed a reader and <c>B…</c> a stream, each through a buffer of two, so
	/// that an element is read across refills; anything else, the string.
	/// </summary>
	static string Outcome(Assembly assembly, string method, string text)
	{
		var parts    = new List<string>();
		var host     = assembly.GetType("Grammar")!;
		var sequence = method[0] switch
		{
			'C' => (IEnumerable)host.GetMethod(method, [typeof(System.IO.TextReader), typeof(int?), typeof(int?)])!
				.Invoke(null, [new System.IO.StringReader(text), 2, 1 << 16])!,
			'B' => (IEnumerable)host.GetMethod(method, [typeof(System.IO.Stream), typeof(int?), typeof(int?)])!
				.Invoke(null, [new System.IO.MemoryStream(Encoding.ASCII.GetBytes(text)), 2, 1 << 16])!,
			_ => (IEnumerable)host.GetMethod(method, [typeof(string)])!.Invoke(null, [text])!,
		};
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

	static (Assembly Assembly, string Source) Compiled(string grammar, bool direct, bool positionalFollow = false)
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

		return (EmittedCode.Compile(result.Sources[0].Text, declarationMembers: Helpers), result.Sources[0].Text);
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
