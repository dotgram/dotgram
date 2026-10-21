using System;
using System.Collections;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Model;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A repetition marked <c>recover</c> inside another one's element or synchronization recovers
/// there, as if it were a rule of its own called at that place, with the enclosing rule's value,
/// trivia and <c>on fail</c> (docs/syntax.md §8.2). It used to be read as a plain repetition, and
/// the outer one stepped over the whole element in its place.
/// </summary>
/// <remarks>
/// <para>
/// Two oracles. The value each input is meant to have, written beside it, is what the grammar
/// says when read as written: what the inner part collects is the enclosing rule's, in place, so
/// an inline <c>@string[]</c> stays flat. The same grammar with the inner part moved into a rule
/// of its own by hand is the oracle for control flow: the same elements stepped over at the same
/// places, the same refusals, read through the names of the rules the twin added, which the inline
/// form says as the enclosing rule's. Where the twin's value has another shape by construction,
/// the case says so and only its control flow is compared.
/// </para>
/// <para>
/// Every case on every carrier, with the methods and without, over a string and over a buffered
/// reader with a buffer of one character.
/// </para>
/// </remarks>
public sealed class NestedRecoveryTests
{
	/// <summary>
	/// One grammar, written with the inner part inline and again with it as a rule of its own.
	/// </summary>
	/// <param name="Origins">
	/// The twin's added rules and the rule of the inline form each one stands in.
	/// </param>
	/// <param name="Rows">Each input and the value it is meant to have: elements joined by commas, or <c>!</c> and the refusal.</param>
	/// <param name="TwinValues">Whether the twin's value is the inline one; false where its shape differs by construction.</param>
	public sealed record Case(
		string Name, string Inline, string? Twin, string[] Origins, (string Input, string Expected)[] Rows,
		bool TwinValues = true, bool Lexical = false)
	{
		public override string ToString()
		{
			return Name;
		}
	}

	const string Words = "Word : @string = t: ['a'..'z']+ => @(t)\n";

	const string Items = Words + "Item : @string = w: Word & ';' => @(w)\n";

	/// <summary>A terminal the lexer reads, for the grammars read over tokens.</summary>
	const string Lexed = "trivia = { ' '* }\nnamespace Lex\n{\n\ttrivia = none\n\tWord = ['a'..'z']+\n}\n";

	static readonly Case[] All =
	[
		new(
			"inner in an element",
			Words + "Start : @string[] = ('[' & (Word & ';')* recover ';' & ']' & eol)* recover eol",
			Words + "Inner : @string[] = (Word & ';')* recover ';'\nStart : @object[] = ('[' & Inner & ']' & eol)* recover eol",
			["Inner"],
			[
				("[a;b;]\n[c;9;d;]\n[e;]\n", "a,b,c,d,e"),

				// The inner synchronization is looked for past the end of the line, so the third
				// line's `;` ends the bad element and `e` goes with it.
				("[a;b;]\n[9]\n[e;]\n", "a,b"),
				("", ""),
			]),

		new(
			"inner in a synchronization",
			Words + "Start : @string[] = (Word & eol)* recover ('#' & ('x' & ';')* recover ';' & eol)",
			Words + "Sync = '#' & ('x' & ';')* recover ';' & eol\nStart : @string[] = (Word & eol)* recover Sync",
			["Sync"],
			[
				("a\n9#x;y;x;\nb\n", "a,b"),
				("a\n9#x;x;\nb\n", "a,b"),

				// No synchronization at all: the bad element runs to the end of the input.
				("a\n9\nb\n", "a"),
			]),

		new(
			"inner in the synchronization of a repetition the methods read",
			Words + "Line : @string = w: Word & eol => @(w)\n" +
				"Start : @string[] = Line* recover ('#' & ('x' & ';')* recover ';' & eol) => @(\"!\" + parserText)",
			Words + "Line : @string = w: Word & eol => @(w)\n" +
				"Sync = '#' & ('x' & ';')* recover ';' & eol\nStart : @string[] = Line* recover Sync => @(\"!\" + parserText)",
			["Sync"],
			[
				("a\n9#x;y;x;\nb\n", "a,!9,b"),
				("a\n9#x;yy;x;\nb\n9#;\nc\n", "a,!9,b,!9,c"),
			]),

		new(
			"depth three",
			Words + "Start : @string[] = ('{' & ('[' & (Word & ';')* recover ';' & ']')* recover ',' & '}' & eol)* recover eol",
			Words +
				"Inner : @string[] = (Word & ';')* recover ';'\n" +
				"Middle : @object[] = ('[' & Inner & ']')* recover ','\n" +
				"Start : @object[] = ('{' & Middle & '}' & eol)* recover eol",
			["Inner", "Middle"],
			[
				// `[9]` is stepped over by the innermost, whose `;` is the one in `[e;]`; `{x}` by
				// the outermost, the middle one finding no `,` before the end of the input.
				("{[a;b;][c;9;d;][9],[e;]}\n{[f;]}\n{x}\n{[g;]}\n", "a,b,c,d,f,g"),
			]),

		new(
			"an inner failure factory",
			Items + "Start : @string[] = ('[' & Item* recover ';' => @(\"!\" + parserText + \"@\" + parserPosition + \":\" + parserMessage) & ']' & eol)* recover eol",
			Items +
				"Inner : @string[] = Item* recover ';' => @(\"!\" + parserText + \"@\" + parserPosition + \":\" + parserMessage)\n" +
				"Start : @object[] = ('[' & Inner & ']' & eol)* recover eol",
			["Inner"],
			[
				("[a;b;]\n[c;9;d;]\n[e;]\n", "a,b,c,!9@10:Expected ']' or ['a'..'z'] at 2:4.,d,e"),
				("[a;9]\n[b;]\n", "a,!9]\n[b@3:Expected ']' or ['a'..'z'] at 1:4."),
			]),

		new(
			"through the end of the input inside the inner part",
			Items + "Start : @string[] = ('[' & Item* recover ';' => @(\"!\" + parserText) & ']' & eol)* recover eol",
			Items +
				"Inner : @string[] = Item* recover ';' => @(\"!\" + parserText)\n" +
				"Start : @object[] = ('[' & Inner & ']' & eol)* recover eol",
			["Inner"],
			[
				// The inner one steps over `b` to the end, where `]` is missing and an empty end
				// is never an element: the outer one steps over the line, which has no `=>`.
				("[a;b", ""),
				("[a;b;]\n[c;9", "a,b"),
			]),

		new(
			"an inner synchronization never found",
			Items + "Start : @string[] = ('[' & Item* recover '|' => @(\"!\" + parserText) & ']' & eol)* recover eol",
			Items +
				"Inner : @string[] = Item* recover '|' => @(\"!\" + parserText)\n" +
				"Start : @object[] = ('[' & Inner & ']' & eol)* recover eol",
			["Inner"],
			[("[a;9;]\n[b;]\n", "b")]),

		new(
			"a synchronization shared",
			Words + "Item : @string = ' ' & w: Word => @(w)\nStart : @string[] = (Word & ':' & Item* recover eol & eol)* recover eol",
			Words +
				"Item : @string = ' ' & w: Word => @(w)\n" +
				"Inner : @string[] = Item* recover eol\n" +
				"Start : @object[] = (Word & ':' & Inner & eol)* recover eol",
			["Inner"],
			[
				// The inner one takes the line's end, so the line's own `eol` is not there and the
				// outer one steps over the whole line.
				("a: b c\nd: e 9 f\ng: h\n", "a,b,c,g,h"),
			]),

		new(
			"a capture inside, read by the enclosing rule's =>",
			Words + "Start : @string = (('[' & (w: Word & ';')* recover ';' & ']' & eol)* recover eol) => @(string.Join(\"+\", w))",
			Words + "Inner = (Word & ';')* recover ';'\nStart = ('[' & Inner & ']' & eol)* recover eol",
			["Inner"],
			[("[a;b;]\n[c;9;d;]\n[e;]\n", "a+b+c+d+e")],
			TwinValues: false),

		new(
			"a capture outside, read by an inner guard",
			Words +
				"Count : @int = d: ['0'..'9'] => @(int.Parse(d))\n" +
				"Start : @string = ((n: Count & ':' & (w: Word & when @(w[w.Length - 1].Length <= n[n.Length - 1]) & ';')* recover ';' & eol)* recover eol)" +
				" => @(string.Join(\"+\", w))",
			null,
			[],
			// Both are sequences so far, the current one last: a word longer than its line's count is
			// a bad element of the inner repetition, and the rest of the line is kept.
			[("2:ab;abc;c;\n1:x;yy;\n", "ab+c+x")]),

		new(
			"the enclosing rule's on fail",
			Words +
				"Blocks : @string[] on fail \"Expected a block.\" = ('[' & (Word & ';')+ recover ';' & ']' & eol)+ recover eol\n" +
				"Start : @string[] = \"doc\" & eol & b: Blocks => @(b)",
			Words +
				"Inner : @string[] on fail \"Expected a block.\" = (Word & ';')+ recover ';'\n" +
				"Blocks : @object[] on fail \"Expected a block.\" = ('[' & Inner & ']' & eol)+ recover eol\n" +
				"Start = \"doc\" & eol & Blocks",
			["Inner"],
			[
				("doc\n", "!Expected '[' at 4."),
				("doc\n[a;9;]\n", "a"),
			],
			TwinValues: false),

		new(
			"elements with trivia around them",
			"trivia = { ' '* }\n" + Words +
				"Item : @string = w: Word & ';' => @(w + \"<\" + parserText + \">\" + parserSpan.Start + \"+\" + parserSpan.Length)\n" +
				"Start : @string[] = ('[' & Item* recover ';' & ']' & eol)* recover eol",
			"trivia = { ' '* }\n" + Words +
				"Item : @string = w: Word & ';' => @(w + \"<\" + parserText + \">\" + parserSpan.Start + \"+\" + parserSpan.Length)\n" +
				"Inner : @string[] = Item* recover ';'\n" +
				"Start : @object[] = ('[' & Inner & ']' & eol)* recover eol",
			["Inner"],
			[(" [ a ; 9 x ; b ; ] \n [ c ; ]\n", "a<a ;>3+3,b<b ;>13+3,c<c ;>23+3")]),

		new(
			"a parameterized rule called twice",
			Words +
				"Block(Open, Close) : @string = ((Open & (w: Word & ';')* recover ';' & Close & eol)* recover eol) => @(string.Join(\"+\", w))\n" +
				"Start : @string[] = Block('[', ']') & '-' & eol & Block('(', ')')",
			Words +
				"Inner = (Word & ';')* recover ';'\n" +
				"Block(Open, Close) = (Open & Inner & Close & eol)* recover eol\n" +
				"Start = Block('[', ']') & '-' & eol & Block('(', ')')",
			["Inner"],
			[("[a;9;b;]\n-\n(c;9;d;)\n", "a+b,c+d")],
			TwinValues: false),

		new(
			"over tokens",
			Lexed + "Item : @string = w: Lex.Word & ';' => @(w)\nStart : @string[] = ('[' & Item* recover ';' & ']' & eol)* recover eol",
			Lexed + "Item : @string = w: Lex.Word & ';' => @(w)\n" +
				"Inner : @string[] = Item* recover ';'\nStart : @object[] = ('[' & Inner & ']' & eol)* recover eol",
			["Inner"],
			[("[a;b;]\n[c;x x;d;]\n[e;]\n", "a,b,c,d,e")],
			Lexical: true),
	];

	public static TheoryData<Case> Cases()
	{
		var data = new TheoryData<Case>();

		foreach (var one in All)
			data.Add(one);

		return data;
	}

	static readonly CarrierKind[] Carriers = [CarrierKind.Auto, CarrierKind.Tape, CarrierKind.Immediate];

	[Theory]
	[MemberData(nameof(Cases))]
	public void Reads_as_written_on_every_carrier(Case one)
	{
		foreach (var carrier in Carriers)
			foreach (var direct in new[] { true, false })
			{
				// A buffered reader is not offered over tokens yet (GRAM4026).
				var inline   = Host(one.Inline, carrier, direct, one.Lexical);
				var buffered = one.Lexical ? null : Host(one.Inline, carrier, direct, one.Lexical, buffered: true);
				var where    = $"{one.Name}, {carrier}, direct {direct}";

				foreach (var (input, expected) in one.Rows)
				{
					Assert.True(expected == Outcome(inline, input), $"{where}, [{input}]: {Outcome(inline, input)}");

					if (buffered is not null)
						Assert.True(expected == Buffered(buffered, input), $"{where} buffered, [{input}]: {Buffered(buffered, input)}");
				}
			}
	}

	[Theory]
	[MemberData(nameof(Cases))]
	public void Steps_over_what_its_twin_steps_over(Case one)
	{
		if (one.Twin is not { } twinGrammar)
			return;

		foreach (var carrier in Carriers)
			foreach (var direct in new[] { true, false })
			{
				var where  = $"{one.Name}, {carrier}, direct {direct}";
				var inline = Host(one.Inline, carrier, direct, one.Lexical, traced: true);
				var twin   = Host(twinGrammar, carrier, direct, one.Lexical, traced: true);

				foreach (var (input, expected) in one.Rows)
				{
					Assert.Equal(Through(one, Recorded(twin, input)), Recorded(inline, input));

					var told = Through(one, Outcome(twin, input));

					if (one.TwinValues || expected.StartsWith("!", StringComparison.Ordinal))
						Assert.True(expected == told, $"{where}, [{input}]: twin {told}");
				}
			}
	}

	/// <summary>
	/// The same as its twin over inputs nobody chose: strings of the characters a case's rows are
	/// made of, short enough to put a bad element anywhere, on the methods and on the tape.
	/// </summary>
	[Theory]
	[MemberData(nameof(Cases))]
	public void Agrees_with_its_twin_on_input_nobody_chose(Case one)
	{
		if (one.Twin is not { } twinGrammar)
			return;

		var alphabet = one.Rows.SelectMany(static row => row.Input).Distinct().OrderBy(static c => c).ToArray();
		var random   = new Random(one.Name.Length * 7919);
		var inputs   = Enumerable.Range(0, 150)
			.Select(_ => new string([.. Enumerable.Range(0, random.Next(0, 16)).Select(_ => alphabet[random.Next(alphabet.Length)])]))
			.Concat(one.Rows.Select(static row => row.Input))
			.ToArray();

		foreach (var (carrier, direct) in new[] { (CarrierKind.Auto, true), (CarrierKind.Tape, false) })
		{
			var inline = Host(one.Inline, carrier, direct, one.Lexical, traced: true);
			var twin   = Host(twinGrammar, carrier, direct, one.Lexical, traced: true);

			foreach (var input in inputs)
			{
				Assert.Equal(Through(one, Recorded(twin, input)), Recorded(inline, input));

				var told  = Outcome(inline, input);
				var twins = Through(one, Outcome(twin, input));

				if (one.TwinValues || told.StartsWith("!", StringComparison.Ordinal) || twins.StartsWith("!", StringComparison.Ordinal))
					Assert.True(told == twins, $"{one.Name}, {carrier}, direct {direct}, [{input}]: {told} where the twin {twins}");
			}
		}
	}

	/// <summary>The lexical cases are read over tokens, not over characters for a reason of their own.</summary>
	[Fact]
	public void The_cases_over_tokens_are_read_over_tokens()
	{
		foreach (var one in All.Where(static one => one.Lexical))
			Assert.DoesNotContain(Compiled(one.Inline, CarrierKind.Auto, true, true, false, false).Diagnostics, static told => told.Id == "GRAM5004");
	}

	/// <summary>
	/// A binding in a recovering repetition is refused wherever it stands (GRAM4034), the inner one
	/// included: one bound inside and read after the inner part, and one bound outside and read
	/// inside it.
	/// </summary>
	[Theory]
	[InlineData("Start = ('[' & ($q: Word & ';')* recover ';' & '=' & $q & ']' & eol)* recover eol", "$q: Word")]
	[InlineData("Start = ($q: Word & ':' & ($q & ';')* recover ';' & eol)* recover eol", "$q & ';'")]
	public void A_binding_across_the_inner_part_is_refused_where_it_stands(string rule, string at)
	{
		var grammar = "Word = ['a'..'z']+\n" + rule + "\nparse Start";
		var told    = GramCompiler.Compile(grammar, Options(CarrierKind.Auto, true, false, false, false)).Diagnostics;

		Assert.Contains(
			told, one => one.Id == "GRAM4034" && one.Severity == GramSeverity.Error &&
				grammar.Substring(one.Position).StartsWith(at.Substring(0, 2), StringComparison.Ordinal) &&
				grammar.IndexOf(at, StringComparison.Ordinal) <= one.Position);
	}

	/// <summary>
	/// Over tokens, a <c>recover</c> in a rule the lexer reads is refused where it is written, the inner
	/// one as well as the outer: a token is the longest match of a regular language, and the lexer read
	/// the repetition as a plain one, so <c>ab;x;</c> was refused at the <c>x</c>.
	/// </summary>
	[Theory]
	[InlineData("Tok = ('a' & ('b' & ';')* recover ';')* recover ','", "('a' & ('b' & ';')* recover ';')* recover ','|('b' & ';')* recover ';'")]
	[InlineData("Tok = ('a' & 'b')* recover ','", "('a' & 'b')* recover ','")]
	public void A_recover_the_lexer_would_read_is_refused_where_it_stands(string rule, string at)
	{
		var grammar = "trivia = { ' '* }\nnamespace Lex\n{\n\ttrivia = none\n\t" + rule + "\n}\nStart = Lex.Tok & eol\nparse Start";
		var told    = GramCompiler.Compile(grammar, Options(CarrierKind.Auto, true, true, false, false)).Diagnostics
			.Where(static one => one.Id == GrammarNormalizer.RecoveryInToken)
			.Select(one => grammar.Substring(one.Position, one.Length))
			.OrderBy(static one => one, StringComparer.Ordinal);

		Assert.Equal(at.Split('|').OrderBy(static one => one, StringComparer.Ordinal), told);
	}

	/// <summary>
	/// A failure factory on a repetition that collects no values is refused (GRAM4010) where it stands
	/// in a synchronization as where it stands in an element, as its twin's is.
	/// </summary>
	[Theory]
	[InlineData(
		"Start : @string[] = (Word & eol)* recover ('#' & (Word & ';')* recover ';' => @(\"!\") & eol)",
		"Sync = '#' & (Word & ';')* recover ';' => @(\"!\") & eol\nStart : @string[] = (Word & eol)* recover Sync")]
	[InlineData(
		"Start : @string[] = ('[' & (Word & ';')* recover ';' => @(\"!\") & ']' & eol)* recover eol",
		"Inner = (Word & ';')* recover ';' => @(\"!\")\nStart : @string[] = ('[' & Inner & ']' & eol)* recover eol")]
	public void A_factory_with_nowhere_to_go_is_refused_inside_as_outside(string inline, string twin)
	{
		foreach (var grammar in new[] { inline, twin })
			Assert.Contains(
				Compiled(Words + grammar, CarrierKind.Auto, true, false, false, false).Diagnostics,
				static one => one.Id == "GRAM4010");
	}

	/// <summary>A <c>recover</c> in a lookahead inside another one's element is still refused there.</summary>
	[Fact]
	public void A_recover_in_a_lookahead_inside_another_is_still_refused()
	{
		var grammar = Words + "Start : @string[] = (?=((Word & ';')* recover ';' & ']') & Word & eol)* recover eol\nparse Start";
		var told    = GramCompiler.Compile(grammar, Options(CarrierKind.Auto, true, false, false, false)).Diagnostics;
		var refused = Assert.Single(told, static one => one.Id == GrammarNormalizer.RecoveryInLookahead);

		Assert.Equal("(Word & ';')* recover ';'", grammar.Substring(refused.Position, refused.Length));
	}

	/// <summary>
	/// A stream steps over the bad elements of the repetition it hands back, so one marked inside
	/// that repetition's synchronization is the repetition's own and the rule gets its reader:
	/// read through a window of four characters, with the inner bad element across the window's
	/// edge, it hands back what the string form does.
	/// </summary>
	[Theory]
	[InlineData("a\n9#x;y;x;\nb\n", "a,b")]
	[InlineData("a\nb\n99#x;x;yy;x;x;\nc\n", "a,b,c")]
	[InlineData("a\n9#yyyyyyyy;x;\nb\n9\n", "a,b")]
	public void One_marked_in_the_synchronization_leaves_the_rule_its_reader(string input, string expected)
	{
		const string Grammar = Words + "Line : @string = w: Word & eol => @(w)\n" +
			"Start : @string[] = Line* recover ('#' & ('x' & ';')* recover ';' & eol)\nparse Start";

		foreach (var carrier in Carriers)
			foreach (var direct in new[] { true, false })
			{
				var options = Options(carrier, direct, false, false, false);

				options.PartSize = 4;

				var result = GramCompiler.Compile(Grammar, options);

				Assert.DoesNotContain(result.Diagnostics, static one => one.Id == "GRAM5001");
				EmittedCode.Quiet(result.Diagnostics);

				var host   = EmittedCode.Compile(Assert.Single(result.Sources).Text).GetType("Grammar")!;
				var whole  = Show(host.GetMethod("ParseStart", [typeof(string)])!.Invoke(null, [input]));
				var reader = host.GetMethod("ParseStart", [typeof(TextReader)])!.Invoke(null, [new StringReader(input)]);

				Assert.Equal(expected, whole);
				Assert.Equal(whole, Show(reader));
			}
	}

	/// <summary>
	/// What still gets no reader: a rule that marks two repetitions side by side, and one whose
	/// marked repetition is not of a rule, since a stream steps over a bad element only in the
	/// repetition it hands back. A marked one inside the element is the outer one's and is not
	/// counted as a second.
	/// </summary>
	[Theory]
	[InlineData(
		"Start : @string[] = Line* recover eol & '-' & eol & Line* recover ';'",
		"marks more than one repetition with 'recover'")]
	[InlineData(
		"Start : @string[] = Line* & ('[' & (Word & ';')* recover ';' & ']' & eol)* recover eol",
		"marks a repetition with 'recover' that is not a repetition of a rule")]
	[InlineData(
		"Start : @string[] = Line* & ('[' & Word & ']' & eol)* recover eol",
		"marks a repetition with 'recover' that is not a repetition of a rule")]
	public void A_stream_is_refused_where_it_would_step_over_the_wrong_thing(string rule, string why)
	{
		var grammar = Words + "Line : @string = w: Word & eol => @(w)\n" + rule + "\nparse Start";
		var told    = GramCompiler.Compile(grammar, Options(CarrierKind.Auto, true, false, false, false)).Diagnostics;

		Assert.Contains(told, one => one.Id == "GRAM5001" && one.Message.Contains(why, StringComparison.Ordinal));
	}

	/// <summary>The twin's names said as the inline form's: every rule the twin added is the enclosing one's.</summary>
	static string Through(Case one, string told)
	{
		foreach (var origin in one.Origins)
			told = System.Text.RegularExpressions.Regex.Replace(told, $@"\b{origin}\b", Enclosing(one, origin));

		return told;
	}

	static string[] Through(Case one, string[] told)
	{
		return [.. told.Select(line => Through(one, line))];
	}

	/// <summary>
	/// The rule of the inline form that a twin's added rule stands in: the one that calls it, or, for
	/// a rule that calls another added one, what that one stands in.
	/// </summary>
	static string Enclosing(Case one, string origin)
	{
		var twin = one.Twin!;

		foreach (var line in twin.Split('\n'))
		{
			var name = line.Split(' ', ':', '=', '(')[0];

			if (name.Length > 0 && name != origin && System.Text.RegularExpressions.Regex.IsMatch(line.Substring(name.Length), $@"\b{origin}\b"))
				return one.Origins.Contains(name) ? Enclosing(one, name) : name;
		}

		throw new InvalidOperationException($"Nothing in the twin of '{one.Name}' calls '{origin}'.");
	}

	static GramCompilerOptions Options(CarrierKind carrier, bool direct, bool lexical, bool buffered, bool traced)
	{
		return new GramCompilerOptions
		{
			ClassName     = "Grammar",
			Carrier       = carrier,
			Direct        = direct,
			Lexical       = lexical,
			BufferedInput = buffered,
			Trace         = traced,
			CSharpScanner = RoslynCSharpScanner.Instance,
		};
	}

	static GramCompilation Compiled(string grammar, CarrierKind carrier, bool direct, bool lexical, bool buffered, bool traced)
	{
		return GramCompiler.Compile(grammar + "\nparse Start", Options(carrier, direct, lexical, buffered, traced));
	}

	/// <summary>Compiled once per text: a carrier that yields to another writes the same source.</summary>
	static readonly ConcurrentDictionary<string, Type> Hosts = new();

	static Type Host(string grammar, CarrierKind carrier, bool direct, bool lexical, bool buffered = false, bool traced = false)
	{
		var result = Compiled(grammar, carrier, direct, lexical, buffered, traced);

		Assert.DoesNotContain(result.Diagnostics, static one => one.Severity == GramSeverity.Error);

		if (!traced)
			EmittedCode.Quiet(result.Diagnostics.Where(static one => one.Id is not ("GRAM5004" or "GRAM5005")));

		var text = string.Concat(result.Sources.Select(static source => source.Text));

		return Hosts.GetOrAdd(text, _ => EmittedCode.Compile(
			result.Sources[0].Text, "Grammar", declarationMembers: traced ? "\n#nullable enable\n" + Recorder : null,
			sourceParts: result.Sources.Skip(1).Select(static source => source.Text)).GetType("Grammar")!);
	}

	/// <summary>What a string answers: the elements, flattened, or <c>!</c> and why it refused.</summary>
	static string Outcome(Type host, string input)
	{
		return Called(() => host.GetMethod("ParseStart", [typeof(string)])!.Invoke(null, [input]));
	}

	/// <summary>The same over a buffered reader whose buffer holds one character.</summary>
	static string Buffered(Type host, string input)
	{
		return Called(() => host.GetMethod("ParseStart", [typeof(TextReader), typeof(int?), typeof(int?)])!
			.Invoke(null, [new StringReader(input), 1, null]));
	}

	static string Called(Func<object?> read)
	{
		try
		{
			return Show(read());
		}
		catch (TargetInvocationException thrown) when (thrown.InnerException is FormatException refused)
		{
			return "!" + refused.Message;
		}
	}

	/// <summary>Every element the trace says was stepped over, in the order it was.</summary>
	static string[] Recorded(Type host, string input)
	{
		var parse = host.GetMethod("ParseStart", [typeof(string)])!;

		return (string[])host.GetMethod("Recorded")!.Invoke(null, [(Func<object?>)(() => Called(() => parse.Invoke(null, [input])))])!;
	}

	/// <summary>A value as a line: a sequence's elements joined by commas, a nested one flattened into it.</summary>
	static string Show(object? value)
	{
		return value switch
		{
			null              => "null",
			string text       => text,
			IEnumerable items => string.Join(",", items.Cast<object?>().Select(Show).Where(static shown => shown.Length > 0)),
			_                 => value.ToString() ?? "",
		};
	}

	/// <summary>A sink that writes each element stepped over as a line, compiled with the parser.</summary>
	const string Recorder = """
		public sealed class Recorder : GramTrace
		{
			public readonly global::System.Collections.Generic.List<string> Lines = new global::System.Collections.Generic.List<string>();

			GramRead? _read;

			public override void Begin(GramRead read)
			{
				_read = read;
			}

			public override void Recovered(int rule, int from, int to, int reach)
			{
				Lines.Add((_read != null ? _read.RuleName(rule) : "#" + rule) + " " + from + " " + to + " " + reach);
			}
		}

		public static string[] Recorded(global::System.Func<object?> read)
		{
			var recorder = new Recorder();

			using (Tracing(recorder))
				read();

			return recorder.Lines.ToArray();
		}
		""";
}
