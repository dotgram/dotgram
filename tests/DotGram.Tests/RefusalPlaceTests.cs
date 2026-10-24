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
/// Where a refused input stops and why, held alike in every form that reads it (docs/syntax.md §7.5):
/// the outcome and the position do not vary by the input's form, so a string, a reader filled one
/// character at a time, a reader with its own buffer, a stream of bytes and the engine say the same.
/// </summary>
/// <remarks>
/// <para>
/// Exhaustive over short inputs from each shape's own characters and one it does not name, which is
/// where a literal near the end of the input and a scanned rule's refusal were told apart: a literal
/// short of room was refused at its first character as input that ran out, where a character already
/// present did not fit, and a scanned rule over a string lost where it got and that it ran out.
/// </para>
/// <para>
/// The message is not compared: §7.5 lets one form name less than another where it is sound.
/// </para>
/// </remarks>
public sealed class RefusalPlaceTests
{
	static readonly (string Name, string Grammar)[] Shapes =
	[
		// Literals, alone and folded with what follows, and their runs.
		("folded",      "T = \"/*\" & ';'"),
		("comment",     "T = \"/*\" & (?!\"*/\" & any)* & \"*/\" & eof"),
		("alone",       "T = \"abc\""),
		("whole",       "T = \"abc\" & eof"),
		("twice",       "T = \"ab\" & \"cd\" & eof"),
		("after",       "T = 'x' & \"abc\" & eof"),
		("optional",    "T = \"abc\"? & 'a' & eof"),
		("repeated",    "T = \"ab\"* & eof"),
		("choice",      "T = (\"abc\" | \"abd\") & eof"),
		("shorter",     "T = (\"abc\" | 'a') & eof"),
		("prefixed",    "T = ('a' & \"bc\" | 'a' & \"bd\") & eof"),
		("ignored",     "T = \"abc\"i & eof"),
		("keywords",    "T = (\"ab\"i | \"cd\"i) & ';' & eof"),
		("looked",      "W = \"ab\"\nT = ?=W & 'a' & any & eof"),
		("refused",     "T = ?!\"ab\" & any* & eof"),

		// Rules read by a scan, and their tails.
		("until",       "W = (?!';' & any)* & ';'\nT = W & W & eof"),
		("run",         "W = ['a'..'b']+ & ';'\nT = W & W & eof"),
		("tail",        "W = 'a'* & \"ab;\"\nT = W & eof"),
		("closing",     "W = \"/*\" & (?!\"*/\" & any)* & \"*/\"\nT = W & eof"),
		("inline",      "T = (?!\"*/\" & any)* & \"*/\" & eof"),

		// A scan's refusal inside a look, a literal reached behind a scan's way back, a longer
		// literal tried after a shorter one, and a run of literals beside a class.
		("scanlooked",  "W = ?!((?!';' & any)* & ';') & 'x'\nT = W & eof"),
		("given",       "W = 'x'* & \"abc\"\nT = W & eof"),
		("givencase",   "W = 'x'* & \"abc\"i\nT = W & eof"),
		("givenwide",   "W = 'x'* & \"äbc\"\nT = W & eof"),
		("retried",     "T = ('a' | \"abcd\") & eof"),
		("beside",      "T = (\"abcd\" | \"abce\" | ['x'|'y']) & eof"),
	];

	public static TheoryData<string> Names()
	{
		var names = new TheoryData<string>();

		foreach (var (name, _) in Shapes)
			names.Add(name);

		return names;
	}

	[Theory]
	[MemberData(nameof(Names))]
	public void Every_form_refuses_at_one_place_for_one_reason(string name)
	{
		var grammar  = Shapes.Single(one => one.Name == name).Grammar;
		var direct   = Build(grammar, direct: true);
		var engine   = Build(grammar, direct: false);
		var alphabet = Alphabet(grammar);
		var failures = new List<string>();

		foreach (var input in Inputs(alphabet, 5))
		{
			var answers = new List<(string Form, string? Answer)>
			{
				("span",         Answer(direct, input, typeof(string), null)),
				("reader 1",     Answer(direct, input, typeof(TextReader), 1)),
				("reader",       Answer(direct, input, typeof(TextReader), null)),
				("bytes 1",      Answer(direct, input, typeof(Stream), 1)),
				("engine",       Answer(engine, input, typeof(string), null)),
				("engine 1",     Answer(engine, input, typeof(TextReader), 1)),
			};

			var told = answers.Where(static one => one.Answer is not null).ToList();

			if (told.Any(one => one.Answer != told[0].Answer))
				failures.Add($"\"{input}\": " + string.Join(", ", told.Select(static one => $"{one.Form} {one.Answer}")));

			if (failures.Count >= 12)
				break;
		}

		Assert.True(failures.Count == 0, $"{name}: {grammar}\n" + string.Join("\n", failures));
	}

	/// <summary>
	/// A literal near the end of the input: refused at the first character present that does not fit,
	/// and as input that ran out only where every character present fits.
	/// </summary>
	[Theory]
	[InlineData("folded",  "x",  "NoMatch@0")]
	[InlineData("folded",  "/;", "NoMatch@1")]
	[InlineData("folded",  "/",  "Starved@1")]
	[InlineData("folded",  "/x", "NoMatch@1")]
	[InlineData("folded",  "/*", "Starved@2")]
	[InlineData("comment", "x",  "NoMatch@0")]
	[InlineData("whole",   "ax", "NoMatch@1")]
	[InlineData("whole",   "ab", "Starved@2")]
	[InlineData("looked",  "a",  "Starved@0")]
	[InlineData("looked",  "x",  "NoMatch@0")]
	[InlineData("until",   "/",  "Starved@1")]
	[InlineData("until",   "a;b", "Starved@3")]
	[InlineData("scanlooked", "a",   "NoMatch@0")]
	[InlineData("given",      "xa",  "Starved@2")]
	[InlineData("given",      "xaz", "NoMatch@2")]
	[InlineData("givencase",  "xA",  "Starved@2")]
	[InlineData("givencase",  "xaz", "NoMatch@2")]
	[InlineData("givenwide",  "xä",  "Starved@2")]
	[InlineData("givenwide",  "xäz", "NoMatch@2")]
	[InlineData("retried",    "ab",  "Starved@2")]
	[InlineData("retried",    "abx", "NoMatch@2")]
	[InlineData("beside",     "abc", "Starved@3")]
	[InlineData("beside",     "ab",  "Starved@2")]
	public void A_refusal_near_the_end_names_what_is_there(string name, string input, string expected)
	{
		var grammar = Shapes.Single(one => one.Name == name).Grammar;

		foreach (var direct in new[] { true, false })
		{
			var built = Build(grammar, direct);

			Assert.Equal(expected, Answer(built, input, typeof(string), null));
			Assert.Equal(expected, Answer(built, input, typeof(TextReader), 1));
			Assert.Equal(expected, Answer(built, input, typeof(TextReader), null));
			Assert.Equal(expected, Answer(built, input, typeof(Stream), 1) ?? expected);
		}
	}

	static Assembly Build(string grammar, bool direct)
	{
		var result = GramCompiler.Compile(grammar + "\nparse T", new GramCompilerOptions
		{
			ClassName     = "Grammar",
			Direct        = direct,
			BufferedInput = true,
			BufferedBytes = true,
			CSharpScanner = RoslynCSharpScanner.Instance,
		});

		EmittedCode.Quiet(result.Diagnostics.Where(static one => one.Id != GramCompiler.BufferedUnsupported));

		return EmittedCode.Compile(result.Sources[0].Text, sourceParts: result.Sources.Skip(1).Select(static one => one.Text));
	}

	/// <summary>The characters a grammar names in quotes, and one it does not.</summary>
	static char[] Alphabet(string grammar)
	{
		var named = new SortedSet<char> { 'x' };
		var quote = '\0';

		foreach (var one in grammar)
		{
			if (quote != '\0')
			{
				if (one == quote)
					quote = '\0';
				else
					named.Add(one);
			}
			else if (one is '\'' or '"')
				quote = one;
		}

		// A range names its ends, which is enough to stand for it.
		return [.. named];
	}

	static IEnumerable<string> Inputs(char[] alphabet, int longest)
	{
		var level = new List<string> { "" };

		yield return "";

		for (var length = 1; length <= longest; length++)
		{
			var next = new List<string>(level.Count * alphabet.Length);

			foreach (var head in level)
				foreach (var one in alphabet)
				{
					var input = head + one;

					next.Add(input);

					yield return input;
				}

			level = next;
		}
	}

	/// <summary>The outcome and where the reading stopped, or null where the form is not offered.</summary>
	static string? Answer(Assembly parser, string input, Type over, int? buffer)
	{
		var type   = parser.GetType("Grammar")!;
		var method = over == typeof(string)
			? type.GetMethod("TryParseT", [typeof(string)])
			: type.GetMethod("TryParseT", [over, typeof(int?), typeof(int?)]);

		if (method is null)
			return null;

		var match = over == typeof(string)
			? method.Invoke(null, [input])!
			: method.Invoke(null, [over == typeof(Stream) ? new MemoryStream(Encoding.UTF8.GetBytes(input)) : new StringReader(input), buffer, null])!;

		object? Read(string member)
		{
			return match.GetType().GetProperty(member)!.GetValue(match);
		}

		return (bool)Read("IsSuccess")!
			? $"Success@{Convert.ToInt64(Read("Position")) + Convert.ToInt64(Read("Length"))}"
			: $"{Read("Outcome")}@{Read("Position")}";
	}
}
