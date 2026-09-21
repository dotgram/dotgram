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

	/// <summary>
	/// The same on the immediate carrier (C4b): each element is built where its rule ends, a
	/// bad one where it is stepped over, and the answers are the engine's.
	/// </summary>
	[Theory]
	[MemberData(nameof(Shapes))]
	public void The_immediate_reader_answers_as_the_engine(string name, string grammar, string[] accepted)
	{
		var reader = Compile(grammar, direct: true, CarrierKind.Immediate);
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

	/// <summary>
	/// What the immediate carrier builds, against the tape (the CarrierDemandTests way): the same
	/// factories as often where every element is good; where one is bad, and only there, the
	/// tag it read before it failed as well — a reading the turn's recovery replaces and whose
	/// value nobody is handed, which is the measure the carrier is chosen by (Replay.Keeps).
	/// </summary>
	[Theory]
	[InlineData("1=a;2=b;", "Field×2, Tag×2", "")]
	[InlineData("1=a;2=;3=c;", "Bad×1, Field×2, Tag×2", "Tag")]
	[InlineData("x;1=a;", "Bad×1, Field×1, Tag×1", "")]
	public void The_immediate_reader_builds_what_the_tape_builds(string input, string built, string extra)
	{
		const string grammar = """
			Tag    : @string   = v: ['0'..'9']+ => @(Log("Tag", Text(v)))
			Field  : @string   = t: Tag & '=' & v: ['a'..'z']+ & ';' => @(Log("Field", t + Text(v)))
			Fields : @string   = fields: Field* recover ';' => @(Log("Bad", Text(parserText))) & eof => @(string.Join(",", fields))
			parse Fields
			""";

		var tape      = Built(Compile(grammar, direct: true, CarrierKind.Tape, Logged), input);
		var immediate = Built(Compile(grammar, direct: true, CarrierKind.Immediate, Logged), input);

		Assert.Equal(built, tape);
		Assert.Equal(extra.Length == 0 ? built : built.Replace(extra + "×2", extra + "×3"), immediate);
	}

	const string Logged = Helpers + """
		public static readonly System.Collections.Generic.List<string> Logs = new System.Collections.Generic.List<string>();
		static string Log(string name, string value) { Logs.Add(name); return value; }
		""";

	/// <summary>
	/// A repetition marked <c>recover</c> published as a <c>yield</c> over a reader: the driver asks the
	/// reader for one turn at a time (Machine.Reader EmitYieldStep), and the elements it hands out,
	/// the recovered ones with the ordinal the driver counts, are the engine's, one for one.
	/// </summary>
	[Theory]
	[InlineData("a=1;b=2;")]
	[InlineData("a=1;bad;c=3;")]
	[InlineData("=;x=9;")]
	[InlineData("a=1;b=")]
	[InlineData("bad;bad;a=1;")]
	[InlineData("")]
	public void The_reader_yields_as_the_engine(string input)
	{
		const string grammar = """
			Field : @string = t: ['a'..'z']+ & '=' & v: ['0'..'9']+ & ';' => @(Text(t) + Text(v))
			Fields : @string[] = Field* recover ';' => @(Bad(parserText, parserOrdinal, parserLine, parserSpan, parserMessage))
			parse Fields as ReadStart stream yield : @string
			""";

		var reader = Yielded(grammar, direct: true, input);
		var engine = Yielded(grammar, direct: false, input);

		Assert.Equal(engine, reader);
	}

	/// <summary>What a yield over a reader hands out, joined, or the exception it ends in.</summary>
	static string Yielded(string grammar, bool direct, string input)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName = "Grammar", Direct = direct, BufferedInput = true, CSharpScanner = RoslynCSharpScanner.Instance,
		});

		EmittedCode.Quiet(result.Diagnostics);

		var source = Assert.Single(result.Sources).Text;

		// Held to what it is about: the step is the reader's where the reader was asked for.
		Assert.Equal(direct, source.Contains("public int Read_Fields_YieldStep", StringComparison.Ordinal));

		var host   = EmittedCode.Compile(source, declarationMembers: Helpers).GetType("Grammar")!;
		var method = host.GetMethods().Single(one => one.Name == "ReadStart" && one.GetParameters()[0].ParameterType == typeof(System.IO.TextReader));
		var told   = new List<string>();

		try
		{
			foreach (var one in (IEnumerable)method.Invoke(null, [new System.IO.StringReader(input), 1, 1 << 16])!)
				told.Add(one?.ToString() ?? "<null>");
		}
		catch (TargetInvocationException error)
		{
			told.Add(error.InnerException!.GetType().Name);
		}
		catch (Exception error)
		{
			told.Add(error.GetType().Name);
		}

		return string.Join(" | ", told);
	}

	/// <summary>The factories a parse ran, counted by name.</summary>
	static string Built(Assembly assembly, string input)
	{
		var logs = (IList)assembly.GetType("Grammar")!.GetField("Logs", BindingFlags.Public | BindingFlags.Static)!.GetValue(null)!;

		logs.Clear();

		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryParseStart", input).IsSuccess);

		return string.Join(", ", logs.Cast<string>().GroupBy(static one => one).OrderBy(static one => one.Key, StringComparer.Ordinal)
			.Select(static one => one.Key + "×" + one.Count()));
	}

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

	static Assembly Compile(string grammar, bool direct, CarrierKind carrier = CarrierKind.Auto, string helpers = Helpers)
	{
		var published = grammar.Replace("parse Count", "parse Count as ParseStart")
			.Replace("parse Fields", "parse Fields as ParseStart")
			.Replace("parse Feed", "parse Feed as ParseStart")
			.Replace("parse Sheet", "parse Sheet as ParseStart");
		var result = GramCompiler.Compile(published, new GramCompilerOptions
		{
			ClassName = "Grammar", Direct = direct, Carrier = carrier, CSharpScanner = RoslynCSharpScanner.Instance,
		});

		EmittedCode.Quiet(result.Diagnostics);

		var source = Assert.Single(result.Sources).Text;

		// Held to what it is about: the reader wrote the recovering loop, the engine did not; and
		// where the immediate carrier was asked for, it is the one that carries.
		Assert.Equal(direct, source.Contains("failure.Reach = p;", StringComparison.Ordinal));

		// Two recovering repetitions of one rule gather onto one stack, which the immediate
		// carrier refuses (SharedStackTests): that shape is read on the tape, and said so.
		if (carrier == CarrierKind.Immediate && !result.Diagnostics.Any(static one => one.Message.Contains("onto one stack")))
			Assert.DoesNotContain("DirectValues.Rent()", source, StringComparison.Ordinal);

		return EmittedCode.Compile(source, declarationMembers: helpers);
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

	static string Escaped(string text)
	{
		return "\"" + text.Replace("\r", "\\r").Replace("\n", "\\n") + "\"";
	}
}
