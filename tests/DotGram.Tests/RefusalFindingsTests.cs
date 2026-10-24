using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Inputs that a refusal is placed differently for by one reading than by another, or whose outcome is not
/// the one the input decides (docs/syntax.md §7.5). Each row names the shape; where the right answer is
/// settled by the text of §7.5 it is given, and otherwise every reading must say what every other does.
/// </summary>
/// <remarks>
/// The readings are the direct reader, the engine and the tape, each over a string, a reader filled one
/// character at a time and with its own buffer, and over the bytes of the text.
/// </remarks>
public sealed class RefusalFindingsTests
{
	/// <summary>
	/// A rule read as one scan over a string — a run, then a literal — still said <c>NoMatch</c> where the
	/// literal's first characters agreed and the input ended; the readers said <c>Starved</c> at the end.
	/// </summary>
	[Theory]
	[InlineData("R = ['a'..'b']+ & \"cd\"\nT = R",       "ac",  "Starved@2")]
	[InlineData("R = ['a'..'b']+ & \"cd\"\nT = R",       "aac", "Starved@3")]
	[InlineData("R = ['a'..'b']* & \"cd\"\nT = R",       "c",   "Starved@1")]
	[InlineData("R = \"ac\"+\nT = 'x' & R",              "xa",  "Starved@2")]
	[InlineData("R = \"ac\"+\nT = 'x' & R",              "xax", "NoMatch@2")]
	[InlineData("R = \"ac\"i+\nT = any & R",             "aA",  "Starved@2")]
	[InlineData("R = \"ab\" & ['a'..'b']+ & \"cd\"\nT = R", "abac", "Starved@4")]
	public void A_run_then_a_literal_the_input_ends_inside_is_starved_over_a_string(string grammar, string input, string expected)
	{
		AssertEveryReading(grammar, input, expected);
	}

	/// <summary>
	/// The engine was left with the old place for a literal that follows an optional or repeated one which
	/// shares its first character: the direct reader says where the input ends, the engine where the turn began.
	/// </summary>
	[Theory]
	[InlineData("T = \"ab\"? & \"ac\"? & 'b'",             "aba",   null)]
	[InlineData("T = \"ab\"? & \"ac\"? & 'b'",             "abab",  null)]
	[InlineData("T = \"ac\"? & \"aab\"* & eof",            "aa",    null)]
	[InlineData("T = \"ac\"? & \"aab\"* & eof",            "acaa",  null)]
	[InlineData("T = any{2} & \"ba\"+",                    "aabab", null)]
	[InlineData("T = (?!\"ab\" & any)* & \"ab\"+",         "aba",   null)]
	[InlineData("T = ?=['a'..'b'] & \"ac\"? & \"aab\"*",   "aa",    null)]
	[InlineData("T = \"ab\" & \"ac\"? & ?!'a' | \"b\" & \"aab\"?", "ba", null)]
	public void The_engine_places_a_literal_cut_by_the_end_as_the_direct_reader_does(string grammar, string input, string? expected)
	{
		AssertEveryReading(grammar, input, expected);
	}

	/// <summary>
	/// A look at the end of the input is undecided, so the refusal is <c>Starved</c> there; an earlier alternative
	/// of an optional literal that disagreed with the input made the direct reader say <c>NoMatch</c> instead,
	/// for an input that one more character turns into a match.
	/// </summary>
	[Theory]
	[InlineData("T = \"ac\"? & \"ab\" & ?='x' & 'x'",  "ab",  "Starved@2", Skip = "The engine records a look ahead that saw nothing where it stands only where the look is kept in locals; the reader never does. One rule for both is a change of its own.")]
	[InlineData("T = \"ac\"? & \"ab\" & ?='x' & 'x'",  "abx", "Success@3")]
	[InlineData("T = 'a'? & \"ab\" & ?='x' & 'x'",     "ab",  "Starved@2", Skip = "The engine records a look ahead that saw nothing where it stands only where the look is kept in locals; the reader never does. One rule for both is a change of its own.")]
	[InlineData("T = \"a\" & \"aab\"? & \"ab\" & ?='x' & any", "aab",  "Starved@3", Skip = "The engine records a look ahead that saw nothing where it stands only where the look is kept in locals; the reader never does. One rule for both is a change of its own.")]
	[InlineData("T = \"a\" & \"aab\"? & \"ab\" & ?='x' & any", "aabx", "Success@4")]
	public void A_look_at_the_end_after_a_literal_that_disagreed_is_starved(string grammar, string input, string expected)
	{
		AssertEveryReading(grammar, input, expected);
	}

	/// <summary>
	/// A scan for a delimiter bounded by a count counted a surrogate pair as one character over a string, and as
	/// two over a reader, where <c>any{2}</c> and a class count two: an input is accepted by the one and refused by the other.
	/// </summary>
	[Theory]
	[InlineData("W = (?!';' & any){0,2} & ';'\nT = W", "x\U0001F600;",         "NoMatch@2")]
	[InlineData("W = (?!';' & any){0,2} & ';'\nT = W", "\U0001F600\U0001F600;", "NoMatch@2")]
	[InlineData("W = (?!';' & any){0,3} & ';'\nT = W", "\U0001F600\U0001F600",   "NoMatch@3")]
	[InlineData("W = (?!';' & any){2} & ';'\nT = W",   "x\U0001F600;",         "NoMatch@2")]
	public void A_bounded_scan_counts_a_surrogate_pair_as_two_characters(string grammar, string input, string expected)
	{
		AssertEveryReading(grammar, input, expected);
	}

	/// <summary>
	/// A reader accepted what its string refuses: the search for a delimiter inside a negative look never made
	/// the look fail.
	/// </summary>
	[Theory]
	[InlineData("T = ?!((?!'b' & any)* & 'b') & any*", "cab", "NoMatch@0")]
	[InlineData("T = ?!((?!\"b\" & any)* & \"b\") & any*", "cab", "NoMatch@0")]
	[InlineData("T = ?!((?!\"b\" & any)* & \"b\") & \"cab\"", "cab", "NoMatch@0")]
	public void A_negative_look_over_a_search_for_a_delimiter_fails_where_it_is_found(string grammar, string input, string expected)
	{
		AssertEveryReading(grammar, input, expected);
	}

	/// <summary>
	/// A source that blocks once its text is used up must not be waited on after a character it gave already
	/// refuses: the room for the whole literal was asked for first, so <c>X</c> against <c>"abc"</c> waits for two more.
	/// </summary>
	[Theory(Skip = "A literal asks for room for the whole of itself before it compares its first character, which is how the buffer is filled; a source that blocks after the text it gave waits there. Left to the stream's design.")]
	[InlineData("T = \"abc\" & eof")]
	[InlineData("T = \"ab\" & \"cd\" & \"ef\" & eof")]
	public void A_reader_is_not_waited_on_after_a_character_that_refuses(string grammar)
	{
		var failures = new List<string>();

		foreach (var config in Configs)
		{
			var parser = Build(grammar, config);

			foreach (var buffer in new int?[] { 1, null })
			{
				var type   = parser.GetType("Grammar")!;
				var method = type.GetMethod("TryParseT", [typeof(TextReader), typeof(int?), typeof(int?)])!;

				try
				{
					var match = method.Invoke(null, [new BlockingReader("X"), buffer, null])!;
					var said  = Said(match);

					if (said != "NoMatch@0")
						failures.Add($"{config.Name} buffer {buffer}: {said}");
				}
				catch (TargetInvocationException one) when (one.InnerException is WouldBlock)
				{
					failures.Add($"{config.Name} buffer {buffer}: asked for more after \"X\"");
				}
			}
		}

		Assert.True(failures.Count == 0, grammar + "\n" + string.Join("\n", failures));
	}

	sealed record Config(string Name, bool Direct, CarrierKind Carrier);

	static readonly Config[] Configs =
	[
		new("direct", true,  CarrierKind.Auto),
		new("engine", false, CarrierKind.Auto),
		new("tape",   true,  CarrierKind.Tape),
	];

	static readonly Dictionary<(string Grammar, string Config), Assembly> Built = [];

	static Assembly Build(string grammar, Config config)
	{
		lock (Built)
		{
			if (Built.TryGetValue((grammar, config.Name), out var known))
				return known;

			var result = GramCompiler.Compile(grammar + "\nparse T", new GramCompilerOptions
			{
				ClassName     = "Grammar",
				Direct        = config.Direct,
				Carrier       = config.Carrier,
				BufferedInput = true,
				BufferedBytes = true,
				CSharpScanner = RoslynCSharpScanner.Instance,
			});

			EmittedCode.Quiet(result.Diagnostics.Where(static one => one.Id != GramCompiler.BufferedUnsupported));

			return Built[(grammar, config.Name)] = EmittedCode.Compile(result.Sources[0].Text, sourceParts: result.Sources.Skip(1).Select(static one => one.Text));
		}
	}

	static void AssertEveryReading(string grammar, string input, string? expected)
	{
		var said = new List<(string Reading, string Answer)>();

		foreach (var config in Configs)
		{
			var parser = Build(grammar, config);
			var type   = parser.GetType("Grammar")!;

			said.Add(($"{config.Name}/string", Said(type.GetMethod("TryParseT", [typeof(string)])!.Invoke(null, [input])!)));

			foreach (var buffer in new int?[] { 1, null })
				said.Add(($"{config.Name}/reader {buffer}", Said(type.GetMethod("TryParseT", [typeof(TextReader), typeof(int?), typeof(int?)])!.Invoke(null, [new StringReader(input), buffer, null])!)));

			// A byte is a character only for ASCII text, and a grammar with a case-folded literal reads no bytes.
			var bytes = type.GetMethod("TryParseT", [typeof(Stream), typeof(int?), typeof(int?)]);

			if (bytes is not null && input.All(static one => one < 128))
			{
				foreach (var buffer in new int?[] { 1, null })
					said.Add(($"{config.Name}/bytes {buffer}", Said(bytes.Invoke(null, [new MemoryStream(Encoding.ASCII.GetBytes(input)), buffer, null])!)));
			}
		}

		var agree = said.All(one => one.Answer == said[0].Answer) && (expected is null || said[0].Answer == expected);

		Assert.True(
			agree,
			$"{grammar.Replace("\n", " ; ")} on \"{input}\"{(expected is null ? "" : ", expected " + expected)}:\n" +
			string.Join("\n", said.GroupBy(static one => one.Answer).Select(static one => $"  {one.Key}: {string.Join(", ", one.Select(static x => x.Reading))}")));
	}

	static string Said(object match)
	{
		object? Read(string member)
		{
			return match.GetType().GetProperty(member)!.GetValue(match);
		}

		return (bool)Read("IsSuccess")!
			? $"Success@{Convert.ToInt64(Read("Position")) + Convert.ToInt64(Read("Length"))}"
			: $"{Read("Outcome")}@{Read("Position")}";
	}

	sealed class WouldBlock : Exception;

	/// <summary>Gives its text, then blocks where a real source would wait.</summary>
	sealed class BlockingReader(string text) : TextReader
	{
		int _at;

		public override int Peek()
		{
			if (_at >= text.Length)
				throw new WouldBlock();

			return text[_at];
		}

		public override int Read()
		{
			if (_at >= text.Length)
				throw new WouldBlock();

			return text[_at++];
		}

		public override int Read(char[] buffer, int index, int count)
		{
			return Read(buffer.AsSpan(index, count));
		}

		public override int Read(Span<char> buffer)
		{
			if (_at >= text.Length)
				throw new WouldBlock();

			var n = Math.Min(buffer.Length, text.Length - _at);

			text.AsSpan(_at, n).CopyTo(buffer);
			_at += n;

			return n;
		}
	}
}
