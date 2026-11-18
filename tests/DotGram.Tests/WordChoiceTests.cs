using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A choice of words — <c>("add"i | "alter"i | …) &amp; ?!WordChar</c>, or a list of words in a
/// namespace with a <c>wordboundary</c> — whose groups the reader tries through a table of the
/// words (Machine.Words). It must answer as the alternatives tried one after another did, word for
/// word: whether, how far, the outcome, and the refusal's message in full, its items in the order
/// they were said.
/// </summary>
/// <remarks>
/// The other side is the same grammar compiled with the table turned off
/// (<see cref="GramCompilerOptions.WordTables"/>), and the trace build, which never reads one. The
/// rows hold the shapes the reading rests on and the places the alternatives came back to: a word
/// that begins another, tried first (<c>exec</c> before <c>execute</c>) and after it; words
/// compared with case and without; groups read with a way back and without; a word with a run of
/// word characters after it and an alternative of another shape among the words, whose groups are
/// written as they were; the checks of §4.6 on both sides of every word, and a word right after a
/// number; and the choice inside braces and inside a look either way.
/// </remarks>
public sealed class WordChoiceTests
{
	sealed record Row(string Name, string Grammar, string[] Pieces);

	const string Char = "Char  = ['a'..'z' | 'A'..'Z' | '0'..'9' | '_']\n";

	static readonly Row[] Rows =
	[
		new("words that begin others",
			Char + """
			Word  = ("int"i | "intersect"i | "into"i | "is"i | "exec"i | "execute"i | "set"i | "setuser"i) & ?!Char
			Start = (Word & [' ' | ','])* & '.'
			""",
			["int", "intersect", "into", "is", "inte", "exec", "EXECUTE", "execut", "executex", "INT", "set", "setuser", "sets", " ", ",", ".", "!", "x", "1"]),

		new("longer words first",
			Char + """
			Word  = ("intersect"i | "execute"i | "into"i | "int"i | "exec"i | "is"i) & ?!Char
			Blank = ' '
			Start = (Word & (Blank | ','))* & '.'
			""",
			["int", "intersect", "into", "is", "inte", "exec", "execute", "exe", " ", ",", ".", "!", "x"]),

		new("words with case",
			"""
			Char  = ['a'..'z' | 'A'..'Z' | '_']
			Word  = ("Add" | "alter" | "ALTER" | "drop" | "dropped" | "deny") & ?!Char
			Start = (Word & ' ')* & '.'
			""",
			["Add", "add", "ADD", "alter", "Alter", "ALTER", "drop", "dropped", "drops", "dro", "deny", " ", ".", "A"]),

		new("words no two of which begin alike",
			Char + """
			Word  = ("drop"i | "deny"i | "dump"i | "alter"i | "add"i | "kill"i) & ?!Char
			Start = (Word & ' ')* & '.'
			""",
			["drop", "deny", "dump", "alter", "add", "kill", "dr", "ad", "killx", " ", ".", "!"]),

		new("words in braces",
			Char + """
			Word  = ("int"i | "intersect"i | "into"i | "is"i | "exec"i | "execute"i) & ?!Char
			Start = { Word } & ':' & ' '*
			""",
			["int", "intersect", "into", "is", "inte", "exec", "execute", ":", " ", ".", "!", "x"]),

		new("another alternative that begins as a word does",
			Char + """
			Word  = ("pg_"i | "open"i | "kill"i | "cron"i | "pg_"i & Char+ | "kind"i | "opt"i) & ?!Char
			Start = (Word & ' ')* & '.'
			""",
			["pg_", "pg_x", "open", "kill", "cron", "kind", "opt", "op", " ", ".", "!", "x"]),

		new("beginnings and another shape",
			Char + """
			Word  = ("open"i | "dblink"i & Char* | "pg_"i & (?!"slot"i & Char)* & "slot"i & Char* | "openquery"i | "kill"i | "cron"i & Char* | "alter"i | "add"i) & ?!Char
			Start = (Word & ' ')* & '.'
			""",
			["open", "openquery", "openq", "dblink", "dblinkx", "dblin", "pg_", "pg_aslot", "pg_slotx", "pg_x", "kill", "killer", "cron", "crontab", "cro", "alter", "add", " ", ".", "_"]),

		new("words in a look",
			Char + """
			Word  = ("int"i | "intersect"i | "into"i | "is"i | "exec"i | "execute"i) & ?!Char
			Start = (?=Word) & '#' | 'z' | ?=Word & Char+ & '.'
			""",
			["int", "intersect", "into", "inter", "exec", "execute", "#", ".", "z", "x"]),

		new("words a name is not",
			Char + """
			Word  = ("int"i | "intersect"i | "into"i | "is"i | "exec"i | "execute"i) & ?!Char
			Ident = ?!Word & { Char+ }
			Start = (Ident & ' ')* & '.'
			""",
			["int", "intersect", "into", "inter", "is", "isx", "exec", "execute", "x", " ", "."]),

		new("words at the end",
			Char + """
			Word  = ("int"i | "intersect"i | "into"i | "is"i | "exec"i | "execute"i) & ?!Char
			Start = Word & (' ' & Word)* & ' '?
			""",
			["int", "intersect", "into", "inter", "exec", "execute", " ", "x"]),

		new("words beside other letters",
			"""
			Char  = [\p{L} | '0'..'9' | '_']
			Word  = ("int"i | "intersect"i | "set"i | "kill"i | "ss"i | "k"i) & ?!Char
			Start = (Word & ' ')* & '.'
			""",
			["int", "İNT", "ınt", "ſet", "K", "ß", "SS", "ẞ", "k", "K", "kill", " ", ".", "é"]),

		new("words with the checks of a word boundary",
			"""
			wordboundary = ['a'..'z' | 'A'..'Z' | '_']
			trivia = { ' '* }
			Key   = "select"i | "selected"i | "from"i | "fromage"i | "where"i | "with"i
			Start = (Key | Name)+ & '.'
			Name  = { ['a'..'z' | 'A'..'Z' | '_']+ } & ?!Key
			""",
			["select", "SELECTED", "selec", "from", "fromage", "frome", "where", "With", "withx", "x", "a", " ", ".", "_"]),

		new("words right after a number",
			"""
			wordboundary = ['a'..'z' | 'A'..'Z' | '0'..'9' | '_']
			trivia = { ' '* }
			Key   = "int"i | "intersect"i | "into"i | "is"i | "exec"i | "execute"i
			Start = (Key | Num)+ & '.'
			Num   = { ['0'..'9']+ }
			""",
			["int", "intersect", "into", "is", "exec", "execute", "1", " ", ".", "x", "inte"]),
	];

	public static TheoryData<string> Names => new(Rows.Select(static row => row.Name));

	[Theory]
	[MemberData(nameof(Names))]
	public void The_table_answers_as_the_alternatives_did(string name)
	{
		var row     = Rows.Single(one => one.Name == name);
		var grammar = row.Grammar + "\nparse Start\n";
		var graph   = Normalized(grammar);
		var start   = graph.Rules.First(rule => rule.Name == "Start");
		var trace   = Build(grammar, CarrierKind.Deferred, tables: true, trace: true, expected: false);

		foreach (var carrier in new[] { CarrierKind.Deferred, CarrierKind.Immediate })
		{
			var table  = Build(grammar, carrier, tables: true, trace: false, expected: true);
			var listed = Build(grammar, carrier, tables: false, trace: false, expected: false);

			foreach (var input in Inputs(row))
			{
				var expected = Answer(listed, input);
				var actual   = Answer(table, input);

				Assert.True(
					expected == actual,
					$"{row.Name}, {carrier}, \"{input}\": the alternatives say\n{expected}\nthe table\n{actual}");

				if (carrier == CarrierKind.Deferred)
				{
					var traced = Answer(trace, input);

					Assert.True(
						traced == actual,
						$"{row.Name}, \"{input}\": the trace build says\n{traced}\nthe table\n{actual}");

					Assert.True(
						actual.StartsWith(ReferenceInterpreter.Parses(graph, start, input) ? "True" : "False", StringComparison.Ordinal),
						$"{row.Name}, \"{input}\": the table says {actual}");
				}
			}
		}
	}

	/// <summary>
	/// A choice of three words is left to the alternatives, and so is one whose words each hold a
	/// character beyond ASCII, whose case the table does not fold.
	/// </summary>
	[Theory]
	[InlineData("""Word = ("add"i | "alter"i | "drop"i) & ?!['a'..'z' | 'A'..'Z']""")]
	[InlineData("""Word = ("ädd"i | "älter"i | "dröp"i | "sïgn"i) & ?!['a'..'z' | 'A'..'Z']""")]
	public void A_choice_the_table_cannot_answer_for_is_left_alone(string rule)
	{
		Build(rule + "\nStart = Word & '.'\nparse Start\n", CarrierKind.Deferred, tables: true, trace: false, expected: false);
	}

	/// <param name="expected">Whether the emitted source must try words through the table.</param>
	static Assembly Build(string grammar, CarrierKind carrier, bool tables, bool trace, bool expected)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName = "Grammar", Carrier = carrier, Direct = true, WordTables = tables, Trace = trace, CSharpScanner = RoslynCSharpScanner.Instance,
		});

		EmittedCode.Quiet(result.Diagnostics);

		var source = Assert.Single(result.Sources).Text;

		Assert.True(
			expected == source.Contains("RecognizeWord", StringComparison.Ordinal),
			$"Expected the table {(expected ? "used" : "not used")} in:\n{grammar}");

		return EmittedCode.Compile(source);
	}

	/// <summary>
	/// Everything a publication answers, as one line: whether, how far, the outcome, the value and the message.
	/// </summary>
	static string Answer(Assembly assembly, string input)
	{
		var match = EmittedCode.Match(assembly, "Grammar", "TryParseStart", input);

		return $"{match.IsSuccess} {match.Position} {EmittedCode.Outcome(assembly, "Grammar", "TryParseStart", input)} {match.Value} {match.Error}";
	}

	/// <summary>
	/// Every piece alone, followed by the end and by what does not continue it, and random runs of the pieces.
	/// </summary>
	static IEnumerable<string> Inputs(Row row)
	{
		var random = new Random(row.Name.Length * 7919 + row.Grammar.Length);
		var made   = new List<string> { "", "." };

		foreach (var piece in row.Pieces)
		{
			made.Add(piece);
			made.Add(piece + ".");
			made.Add(piece + " .");
			made.Add(piece + "!");
		}

		for (var i = 0; i < 400; i++)
		{
			var built = new StringBuilder();

			for (var length = random.Next(1, 7); length > 0; length--)
				built.Append(row.Pieces[random.Next(row.Pieces.Length)]);

			if (random.Next(3) != 0)
				built.Append(random.Next(2) == 0 ? "." : " ");

			made.Add(built.ToString());
		}

		return made;
	}

	static RecognitionGraph Normalized(string text)
	{
		return GrammarNormalizer.Normalize(
			GrammarBinder.Bind(
				GramParser.Parse(GramLexer.Tokenize(text, RoslynCSharpScanner.Instance)).File));
	}
}
