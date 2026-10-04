using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// What the buffered engine writes grows as its grammar does, and what it reads is what the
/// semantics reads.
/// </summary>
/// <remarks>
/// <para>
/// <b>What it holds.</b> A choice the engine dispatches on its first character is cut into
/// groups by that character, and an alternative that can begin with characters of two groups
/// is a member of both. Each group was compiled as a chain of its own, and each chain compiled
/// its members again: an alternative was written once per group it was in, and everything
/// inside it as many times. Nested — a rule whose choice calls the next rule, which is inlined
/// and is a choice of the same kind — the copies multiplied level by level. A grammar of four
/// lines made the buffered engine write a method of ten thousand lines that Roslyn did not
/// finish compiling in four minutes. The string forms of the same grammar never showed it:
/// they lower to a plain method, which does not dispatch this way, and over a buffer there is
/// no plain method to lower to. Now the groups share one compilation of each alternative.
/// </para>
/// <para>
/// <b>Measured, not timed.</b> The length of the emitted source at four sizes of grammar a
/// doubling apart, for the buffered engine and the unbuffered one beside it: what a doubling
/// adds may at most double as well, and what the buffered engine adds stays within a constant
/// of what the unbuffered one adds.
/// </para>
/// </remarks>
public sealed partial class BufferedInputTests
{
	/// <summary>The grammar the defect was found with: a fuzz seed's, kept as it came.</summary>
	const string Overlapping = """
		trivia = { ' '* }
		Start = ((('K' | "KA"i | R1) | ('b'i & 'A'i) | ("BA"i | "kA"i | "akK"i | R1)) | (("Kb"i | "BB"i) | ("BA"i | "bKB" | "kkk"i) | 'K' | "1B1"i) | 'K'i | (('B'i | 'A'i) & "Bb"i))
		R1 = ('a'i | 'a'i | ("kk"i & ('a'i & 'a'i) & 'B'i) | (('a'i | 'a'i | 'a'i | 'a'i) | 'a'i | "1B"i | ("BB"i & "a1A")))
		parse Start
		""";

	/// <summary>Four sizes of grammar, a doubling apart.</summary>
	static readonly int[] Depths = [1, 2, 4, 8];

	/// <summary>How much more a doubling may add than the doubling before it.</summary>
	const double GrowthSlack = 2.2;

	/// <summary>
	/// How much more the buffered engine may add for a doubling than the unbuffered engine adds.
	/// </summary>
	/// <remarks>
	/// The unbuffered engine lowers these grammars to a plain method, and the buffered one keeps
	/// them as states of the shared automaton, where every state carries its trace and its way
	/// back: measured at four to eight times the unbuffered addition. The copies this holds out
	/// grew it by four times more at every level.
	/// </remarks>
	const int BufferedRatio = 16;

	/// <summary>
	/// <paramref name="depth"/> rules, each a choice of four alternatives that begin apart and a
	/// fifth, the next rule, that begins with any of them: so a dispatch over four groups, with
	/// the next rule a member of every one.
	/// </summary>
	static string Nested(int depth)
	{
		var text = new StringBuilder("trivia = { ' '* }\nStart = R1\n");

		for (var k = 1; k <= depth; k++)
			text.Append($"R{k} = 'a'i & 'k' | 'b'i & \"k{k}\" | 'c'i & 'k' | 'd'i & 'k' | ")
				.AppendLine(k < depth ? $"R{k + 1} & 'z'" : "'e' & 'z'");

		return text.Append("parse Start\n").ToString();
	}

	static GramCompilerOptions Engine(bool buffered)
	{
		return new GramCompilerOptions
		{
			ClassName = "Grammar", Direct = false, BufferedInput = buffered, BufferSize = 1,
			CSharpScanner = RoslynCSharpScanner.Instance,
		};
	}

	static int EmittedLength(string grammar, bool buffered)
	{
		var result = GramCompiler.Compile(grammar, Engine(buffered));

		EmittedCode.Quiet(result.Diagnostics);

		return Assert.Single(result.Sources).Text.Length;
	}

	[Fact]
	public void What_the_buffered_engine_writes_grows_with_its_grammar()
	{
		var buffered   = new List<int>();
		var unbuffered = new List<int>();

		// Asked after every size and not once at the end, so that a regression fails at the third
		// size rather than running on into the fourth, which it would not finish.
		foreach (var depth in Depths)
		{
			var grammar = Nested(depth);

			buffered.Add(EmittedLength(grammar, buffered: true));
			unbuffered.Add(EmittedLength(grammar, buffered: false));

			if (buffered.Count < 3)
				continue;

			var told = $"the buffered engine wrote {string.Join(", ", buffered)} characters and the unbuffered " +
			           $"{string.Join(", ", unbuffered)} at {string.Join(", ", Depths.Take(buffered.Count))} rules";

			foreach (var (name, sizes) in new[] { ("buffered", buffered), ("unbuffered", unbuffered) })
			{
				var earlier = sizes[^2] - sizes[^3];
				var later   = sizes[^1] - sizes[^2];

				Assert.True(later <= earlier * GrowthSlack, $"{name}: a doubling added {later} where the one before added {earlier}; {told}.");
			}

			Assert.True(
				buffered[^1] - buffered[^2] <= (unbuffered[^1] - unbuffered[^2]) * BufferedRatio,
				$"a doubling added {buffered[^1] - buffered[^2]} to the buffered engine and " +
				$"{unbuffered[^1] - unbuffered[^2]} to the unbuffered; {told}.");
		}
	}

	/// <summary>
	/// The grammar that was found and a nest of the shape that multiplied, compiled for the buffered
	/// engine and read through a reader that hands over one character a call, at every capacity
	/// from one to past the input: whether it reads the input is the semantics' answer, where it
	/// stops is the string form's, and what it says is the same at every capacity.
	/// </summary>
	[Fact]
	public void The_buffered_engine_reads_what_the_semantics_reads()
	{
		var random = new Random(6);
		var asked  = 0;

		// The found grammar spelled from its literals, as many as four in a row; the nest from its
		// own sentences, one at a time, since it reads one.
		foreach (var (grammar, words, most) in new[] { (Overlapping, Words(Overlapping), 4), (Nested(3), Sentences(1, 3), 1) })
		{
			var result = GramCompiler.Compile(grammar, Engine(buffered: true));

			EmittedCode.Quiet(result.Diagnostics);

			var assembly = EmittedCode.Compile(Assert.Single(result.Sources).Text);
			var graph    = GrammarNormalizer.Normalize(
				GrammarBinder.Bind(GramParser.Parse(GramLexer.Tokenize(grammar, RoslynCSharpScanner.Instance)).File));
			var start    = graph.Rules.First(static rule => rule.Name == "Start");
			var accepted = 0;

			for (var round = 0; round < 200; round++)
			{
				var input  = Spelled(random, words, most);
				var oracle = ReferenceInterpreter.Parses(graph, start, input);
				var whole  = EmittedCode.Match(assembly, "Grammar", "TryParseStart", input);

				Assert.True(whole.IsSuccess == oracle, $"string form: {whole.IsSuccess}, semantics {oracle}; input \"{input}\"\n{grammar}");

				string? said = null;

				for (var capacity = 1; capacity <= input.Length + 1; capacity++)
				{
					var read = Read(assembly, new ShortReader(input, 1), capacity);

					// Where and whether, against the string form. Not what a refusal says: the string
					// form is a lowered method, which words its expected list as the engine does not.
					// What the engine says it says at every capacity.
					Assert.True(
						(read.Success, read.Position) == (whole.IsSuccess, whole.Position),
						$"capacity {capacity}: read {(read.Success, read.Position)}, the string form " +
						$"{(whole.IsSuccess, whole.Position)}; input \"{input}\"\n{grammar}");

					if (capacity == 1)
						said = read.Error;

					Assert.True(read.Error == said, $"capacity {capacity}: \"{read.Error}\", at capacity 1 \"{said}\"; input \"{input}\"\n{grammar}");

					asked++;
				}

				if (oracle)
					accepted++;
			}

			// Or the inputs never reach what the grammar reads and the comparison holds of refusals alone.
			Assert.True(accepted >= 20, $"only {accepted} of 200 inputs were accepted\n{grammar}");
		}

		Assert.True(asked > 1000, $"only {asked} readings were asked.");
	}

	/// <summary>The literals a grammar is written with, each a word an input may be spelled from.</summary>
	static string[] Words(string grammar)
	{
		var words = new List<string>();

		for (var at = 0; at < grammar.Length; at++)
		{
			if (grammar[at] is not ('\'' or '"'))
				continue;

			var end = grammar.IndexOf(grammar[at], at + 1);

			if (end > at + 1 && grammar.Substring(at + 1, end - at - 1) is var word && word != " ")
				words.Add(word);

			at = end;
		}

		return [.. words.Distinct(StringComparer.Ordinal)];
	}

	/// <summary>Every sentence <see cref="Nested"/> reads from rule <paramref name="k"/> on, its letters as written.</summary>
	static string[] Sentences(int k, int depth)
	{
		var deeper = k < depth ? Sentences(k + 1, depth) : ["e"];

		return ["ak", "bk" + k, "ck", "dk", .. deeper.Select(static one => one + "z")];
	}

	/// <summary>
	/// One to <paramref name="most"/> of the words, each letter as written or in its other case,
	/// now and then with a space after it or a letter replaced; or a few letters of the alphabet
	/// at random.
	/// </summary>
	static string Spelled(Random random, string[] words, int most)
	{
		var text     = new StringBuilder();
		var alphabet = string.Concat(words).ToUpperInvariant() + string.Concat(words).ToLowerInvariant() + " ";

		if (random.Next(5) == 0)
		{
			for (var length = random.Next(0, 6); length > 0; length--)
				text.Append(alphabet[random.Next(alphabet.Length)]);

			return text.ToString();
		}

		for (var count = random.Next(1, most + 1); count > 0; count--)
		{
			foreach (var one in words[random.Next(words.Length)])
			{
				text.Append(random.Next(10) switch
				{
					0 or 1 or 2 => char.ToUpperInvariant(one),
					3 or 4 or 5 => char.ToLowerInvariant(one),
					6           => alphabet[random.Next(alphabet.Length)],
					_           => one,
				});
			}

			if (random.Next(4) == 0)
				text.Append(' ');
		}

		return text.ToString();
	}
}
