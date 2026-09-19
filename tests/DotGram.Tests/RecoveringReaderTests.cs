using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A repetition marked <c>recover</c> read by methods (docs/design/fix-reader-2026-09-18.md §8,
/// the scenario of a repetition whose only way back is <c>recover</c>), held to the engine: the
/// same answer on every input, the values the <c>recover</c> factories built included.
/// </summary>
/// <remarks>
/// The inputs are made from the accepted ones as the refusal record makes them — every proper
/// prefix, each with a character left out and with one too many — which is where an element
/// breaks in its middle, a trailer is short, and a synchronization is missing at the end. Each
/// factory is handed everything a reader supplies (text, ordinal, line, span, message), so that
/// a place counted differently anywhere shows.
/// </remarks>
public sealed class RecoveringReaderTests
{
	const string Helpers = """
		static string Text(string text) { return text; }
		static string Text(global::System.ReadOnlySpan<char> text) { return text.ToString(); }
		static string Bad(string text, int ordinal, int line, SourceSpan span, string message) { return "!" + text + "@" + ordinal + "/" + line + "/" + span.Start + "+" + span.Length + "/" + message; }
		static string Bad(global::System.ReadOnlySpan<char> text, int ordinal, int line, SourceSpan span, string message) { return Bad(text.ToString(), ordinal, line, span, message); }
		""";

	const string Supplied = "@(Bad(parserText, parserOrdinal, parserLine, parserSpan, parserMessage))";

	public static TheoryData<string, string, string[]> Shapes => new()
	{
		// A trailer after the repetition, which begins as an element does and fails as one
		// (the stock count): tried first at every boundary.
		{
			"a trailer that begins like an element",
			"Line : @string = n: ['a'..'z' | 'A'..'Z']+ & ':' & ' ' & d: ['0'..'9']+ & eol => @(Text(n) + \"=\" + Text(d))\n" +
			"Total : @string = \"END\" & ' ' & d: ['0'..'9']+ & eol? => @(\"END\" + Text(d))\n" +
			"Count : @string = lines: Line* recover eol => " + Supplied + " & total: Total & eof => @(string.Join(\"|\", lines) + \"#\" + total)\n" +
			"parse Count",
			["apples: 12\npears: 7\nEND 2\n", "a 1\nb: 2\nEND 2\n", "END 3\nEND 3\n", "a: 1END 3\n", "END 0\n"]
		},

		// Nothing but the end after it, as FIX's fields have. A repetition that is the whole
		// of a rule building a sequence is read a window at a time instead, where the engine
		// keeps it; FixGrammar's own suite holds the reader over that one.
		{
			"a repetition followed by the end",
			"Field : @string = t: ['a'..'z']+ & '=' & v: ['0'..'9']+ & ';' => @(Text(t) + Text(v))\n" +
			"Fields : @string = fields: Field* recover ';' => " + Supplied + " & eof => @(string.Join(\",\", fields))\n" +
			"parse Fields",
			["a=1;b=2;", "a=1;bad;c=3;", "=;x=9;", "a=1;b="]
		},

		// A continuation that holds at every boundary: the first one ends the repetition.
		{
			"a continuation that holds at every boundary",
			"Item : @string = 'a' & eol => @(\"a\")\n" +
			"Start : @string = items: Item* recover eol => " + Supplied + " & rest: (Item*) & eof => @(string.Join(\",\", items) + \"|\" + Text(rest))\n" +
			"parse Start",
			["a\na\n", "a\nb\na\n", "b\n"]
		},

		// A header before it, at least one element, a trailer that cannot begin one.
		{
			"a header, at least one row, a trailer",
			"Row : @string = 'R' & t: ['a'..'z']+ & eol => @(Text(t))\n" +
			"Feed : @string = 'H' & eol & rows: Row+ recover eol => " + Supplied + " & 'T' & eol & eof => @(string.Join(\",\", rows))\n" +
			"parse Feed",
			["H\nRa\nRb\nT\n", "H\nRa\nX\nRc\nT\n", "H\nT\n"]
		},

		// Two marked repetitions in one rule: the first's continuation holds the second.
		{
			"two recovering repetitions in one rule",
			"Row : @string = t: ['a'..'z']+ & eol => @(Text(t))\n" +
			"Sheet : @string = 'H' & eol & head: Row* recover eol => " + Supplied + "\n" +
			"      & 'B' & eol & body: Row* recover eol => " + Supplied + "\n" +
			"      & eof => @(string.Join(\",\", head) + \"|\" + string.Join(\",\", body))\n" +
			"parse Sheet",
			["H\na\nB\nb\n", "H\na\n1\nB\n2\nb\n", "H\nB\n", "H\nB1\nB\nc\n"]
		},
	};

	[Theory]
	[MemberData(nameof(Shapes))]
	public void The_reader_answers_as_the_engine(string name, string grammar, string[] accepted)
	{
		var reader = Compile(grammar, direct: true);
		var engine = Compile(grammar, direct: false);
		var told   = new List<string>();

		foreach (var input in Made(accepted))
		{
			var expected = Answer(engine, input);
			var actual   = Answer(reader, input);

			if (expected != actual)
				told.Add($"{Escaped(input)}\n  engine: {expected}\n  reader: {actual}");
		}

		Assert.True(told.Count == 0, $"{name}: {told.Count} inputs told the two apart:\n" + string.Join("\n", told.Take(8)));
	}

	/// <summary>What was recovered, where the parse accepted; where it refused, where it did not.</summary>
	static string Answer(Assembly assembly, string input)
	{
		var match = EmittedCode.Match(assembly, "Grammar", "TryParseStart", input);

		// Where it refused and not in what words: how a reader words what it expected is held
		// to the engine's in the refusal record, for every grammar at once.
		if (!match.IsSuccess)
			return $"refused at {match.Position}";

		return match.Value is IEnumerable values and not string
			? string.Join(" | ", values.Cast<object>())
			: match.Value?.ToString() ?? "<null>";
	}

	static Assembly Compile(string grammar, bool direct)
	{
		var published = grammar.Replace("parse Count", "parse Count as ParseStart")
			.Replace("parse Fields", "parse Fields as ParseStart")
			.Replace("parse Feed", "parse Feed as ParseStart")
			.Replace("parse Sheet", "parse Sheet as ParseStart");
		var result = GramCompiler.Compile(published, new GramCompilerOptions
		{
			ClassName = "Grammar", Direct = direct, CSharpScanner = RoslynCSharpScanner.Instance,
		});

		EmittedCode.Quiet(result.Diagnostics);

		var source = Assert.Single(result.Sources).Text;

		// Held to what it is about: the reader wrote the recovering loop, the engine did not.
		Assert.Equal(direct, source.Contains("failure.Reach = p;", StringComparison.Ordinal));

		return EmittedCode.Compile(source, declarationMembers: Helpers);
	}

	/// <summary>Every input made from the accepted ones, each once and in order (RefusalCorpus).</summary>
	static IEnumerable<string> Made(string[] accepted)
	{
		var seen = new HashSet<string>(StringComparer.Ordinal);

		foreach (var input in accepted)
		{
			if (seen.Add(input))
				yield return input;

			for (var length = 0; length < input.Length; length++)
				if (seen.Add(input.Substring(0, length)))
					yield return input.Substring(0, length);

			for (var at = 0; at < input.Length; at++)
				if (seen.Add(input.Remove(at, 1)))
					yield return input.Remove(at, 1);

			if (input.Length > 0 && seen.Add(input + input.Substring(input.Length - 1)))
				yield return input + input.Substring(input.Length - 1);
		}
	}

	static string Escaped(string text) => "\"" + text.Replace("\r", "\\r").Replace("\n", "\\n") + "\"";
}
