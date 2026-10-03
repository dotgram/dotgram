using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// What a rejected element of a recovering repetition is told about where it went wrong
/// (docs/syntax.md §8.2), and what a refusal says around it: the same place in every rendering,
/// and words a person can act on.
/// </summary>
public sealed class RejectionMessageTests
{
	/// <summary>A price list typed by hand: the third line has a comma, the fourth a letter O for a zero.</summary>
	const string Prices =
		"Espresso       2.40\n" +
		"Cappuccino     3.10\n" +
		"Flat white     3,20\n" +
		"Croissant      2.1O\n" +
		"Muffin         2.80\n";

	const string PriceList = """
		PriceList : @string = lines: Entry* recover eol
		    => @(Told(parserLine, parserColumn, parserFailureLine, parserFailureColumn, parserFailurePosition, parserExpected, parserMessage))
		    => @(Join(lines))
		Entry  : @string = item: Item & amount: Amount & ' '* & eol => @(Text(amount))
		Item   = [^ '0'..'9' | '\r' | '\n']+
		Amount = ['0'..'9']+ & '.' & ['0'..'9']{2}

		""";

	const string Helpers = """
		static string Text(string text) { return text; }
		static string Text(global::System.ReadOnlySpan<char> text) { return text.ToString(); }
		static string Text(global::System.ReadOnlySpan<byte> text) { return global::System.Text.Encoding.ASCII.GetString(text.ToArray()); }
		static bool Allowed() { return false; }
		static string Join(string[] parts) { return string.Join("\u0001", parts); }
		static string Told(int line, int column, int failureLine, int failureColumn, long failurePosition, string expected, string message)
		{
			return line + ":" + column + " " + failureLine + ":" + failureColumn + "@" + failurePosition + " " + expected + " | " + message;
		}
		""";

	/// <summary>The renderings that read by methods, whose refusals a choice says over a call (Refuse_DotGram_Over).</summary>
	public static TheoryData<string, bool, CarrierKind> Readers => new()
	{
		{ "tape", true, CarrierKind.Tape },
		{ "immediate", true, CarrierKind.Immediate },
	};

	public static TheoryData<string, bool, CarrierKind> Renderings => new()
	{
		{ "engine", false, CarrierKind.Tape },
		{ "tape", true, CarrierKind.Tape },
		{ "immediate", true, CarrierKind.Immediate },
	};

	/// <summary>
	/// Where the record began, where reading it stopped, and what would have fit there — the comma
	/// sixteen characters into the third line, the O eighteen into the fourth — in every rendering.
	/// </summary>
	[Theory]
	[MemberData(nameof(Renderings))]
	public void A_rejected_record_is_told_where_reading_it_stopped(string rendering, bool direct, CarrierKind carrier)
	{
		var told = Recovered(Compiled(PriceList + "parse PriceList as ParseStart\n", direct, carrier));

		Assert.True(told.Length == 5, rendering + ": " + string.Join(" / ", told));
		Assert.Equal("2.40", told[0]);
		Assert.Equal("3.10", told[1]);
		// The engine names the rule the reading was in; a reader, the character it wanted there.
		Assert.Equal(direct ? "3:1 3:17@56 '.' | Expected '.' at 3:17." : "3:1 3:17@56 Amount | Expected Amount at 3:17.", told[2]);
		Assert.Equal(direct ? "4:1 4:19@78 ['0'..'9'] | Expected ['0'..'9'] at 4:19." : "4:1 4:19@78 Amount | Expected Amount at 4:19.", told[3]);
		Assert.Equal("2.80", told[4]);
	}

	/// <summary>
	/// The same from a <c>TextReader</c>, through a window smaller than a line, which moves under
	/// the record while its end is looked for.
	/// </summary>
	[Theory]
	[InlineData(8)]
	[InlineData(4096)]
	public void A_streamed_record_is_told_the_same_place(int window)
	{
		var host = Compiled(
			PriceList.Replace("PriceList : @string = lines: Entry*", "PriceList : @string[] = Entry*").Replace("\n    => @(Join(lines))", "") +
			"parse PriceList as ReadStart stream yield\n",
			direct: true, CarrierKind.Auto);
		var read = host.GetType("Grammar")!.GetMethods()
			.Single(static one => one.Name == "ReadStart" && one.GetParameters()[0].ParameterType == typeof(TextReader));
		var told = ((IEnumerable)read.Invoke(null, [new StringReader(Prices), window, null])!)
			.Cast<object>().Select(static one => one.ToString()!).ToArray();

		Assert.True(told.Length == 5, string.Join(" / ", told));
		Assert.Equal("3:1 3:17@56 '.' | Expected '.' at 3:17.", told[2]);
		Assert.Equal("4:1 4:19@78 ['0'..'9'] | Expected ['0'..'9'] at 4:19.", told[3]);
	}

	/// <summary>
	/// A rule whose every alternative hands another rule's value on is a value, and the element of
	/// a recovering repetition as written (§6's own spelling), not a choice of its sources.
	/// </summary>
	[Fact]
	public void A_rule_that_forwards_per_alternative_is_an_element_that_recovers()
	{
		const string Lines = """
			Sheet : @string = lines: Line* recover eol => @(Told(parserLine, parserColumn, parserFailureLine, parserFailureColumn, parserFailurePosition, parserExpected, parserMessage)) & eof => @(Join(lines))
			Item   : @string = d: ['0'..'9']+ & eol => @(Text(d))
			Remark : @string = '#' & t: [^ '\n']* & eol => @(Text(t))
			parse Sheet as ParseStart

			""";

		var perAlternative = Compiled("Line : @string = v: Item => @(v) | v: Remark => @(v)\n" + Lines, direct: true, CarrierKind.Auto);
		var grouped        = Compiled("Line : @string = (v: Item | v: Remark) => @(v)\n" + Lines, direct: true, CarrierKind.Auto);

		const string Input = "12\n# note\nx2\n4\n";

		Assert.Equal(Recovered(grouped, Input), Recovered(perAlternative, Input));
		Assert.Equal("3:1 3:1@10", string.Join(" ", Recovered(perAlternative, Input)[2].Split(' ').Take(2)));
	}

	/// <summary><c>when</c> begins a guard; a capture called that is told to take another name, once.</summary>
	[Fact]
	public void A_capture_called_when_is_told_to_rename()
	{
		var result = GramCompiler.Compile("Start : @string = when: 'a' & b: 'b' => @(b)\nparse Start\n", new GramCompilerOptions
		{
			ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance,
		});

		var error = Assert.Single(result.Diagnostics, static one => one.Severity == GramSeverity.Error);

		Assert.Equal("GRAM2003", error.Id);
		Assert.Contains("Rename the capture", error.Message, StringComparison.Ordinal);
	}

	/// <summary>Input left after a whole value is what the end of the input was expected at.</summary>
	[Theory]
	[MemberData(nameof(Renderings))]
	public void Input_left_over_is_where_the_end_was_expected(string rendering, bool direct, CarrierKind carrier)
	{
		var host  = Compiled("Start : @string = '{' & t: ['a'..'z']* & '}' => @(Text(t))\nparse Start as ParseStart\n", direct, carrier);
		var match = EmittedCode.Match(host, "Grammar", "TryParseStart", "{ab}x");

		Assert.False(match.IsSuccess, rendering);
		Assert.Equal(4, match.Position);
		Assert.Equal("Expected end of input.", match.Error);
	}

	/// <summary>
	/// A call whose own first set the choice around it holds is said once, in the choice's words:
	/// <c>Digit</c> is not listed beside the set that holds its digits.
	/// </summary>
	[Theory]
	[MemberData(nameof(Renderings))]
	public void A_set_the_choice_holds_is_not_said_twice(string rendering, bool direct, CarrierKind carrier)
	{
		const string Grammar = """
			trivia = ' '*
			Digit  = ['0'..'9']
			Number : @int = d: Digit+ => @(int.Parse(Text(d)))
			Expr   : @int = '(' & e: Expr & ')' => @(e) | '-' & e: Expr => @(-e) | n: Number => @(n)
			parse Expr as ParseStart

			""";

		var host = Compiled(Grammar, direct, carrier);

		foreach (var input in new[] { "x", "- x", "(x" })
		{
			var match = EmittedCode.Match(host, "Grammar", "TryParseStart", input);

			Assert.False(match.IsSuccess, rendering + ": " + input);
			Assert.Equal("Expected ['(' | '-' | '0'..'9'].", match.Error);
		}
	}

	/// <summary>What a method without <c>Try</c> throws ends where a sentence ends, with the place inside it.</summary>
	[Fact]
	public void A_refusal_thrown_says_where_before_its_full_stop()
	{
		var host  = Compiled("Start : @string = 'a' & t: 'b' => @(Text(t))\nparse Start as ParseStart\n", direct: true, CarrierKind.Auto);
		var parse = host.GetType("Grammar")!.GetMethod("ParseStart", [typeof(string)])!;

		var thrown = Assert.Throws<TargetInvocationException>(() => parse.Invoke(null, ["ax"]));

		Assert.Equal("Expected 'b' at 1.", Assert.IsType<FormatException>(thrown.InnerException).Message);
	}

	const string Where = "@(Told(parserLine, parserColumn, parserFailureLine, parserFailureColumn, parserFailurePosition, parserExpected, parserMessage))";

	/// <summary>
	/// What would have fit is taken where the element failed, not after the synchronization was
	/// looked for: <c>ENX</c> begins to match <c>"END"</c> and refuses further along than the element did.
	/// </summary>
	[Theory]
	[MemberData(nameof(Renderings))]
	public void A_synchronization_that_begins_to_match_does_not_take_the_explanation(string rendering, bool direct, CarrierKind carrier)
	{
		var host = Compiled(
			"Item : @string = t: 'a' & ';' => @(Text(t))\n" +
			"Start : @string = items: Item* recover \"END\" => " + Where + " => @(Join(items))\n" +
			"parse Start as ParseStart\n",
			direct, carrier);

		var told = Recovered(host, "a;b ENX q ENDa;");

		Assert.True(told.Length == 3, rendering + ": " + string.Join(" / ", told));
		Told(told[1], "1:3 1:3@2", "'a'", rendering);
	}

	/// <summary>
	/// Two elements that stop at one place keep what each wanted: the first reads across its line
	/// and fails where the second begins, which fails there too, wanting something else.
	/// </summary>
	[Theory]
	[MemberData(nameof(Renderings))]
	public void Two_elements_that_stop_at_one_place_keep_their_own(string rendering, bool direct, CarrierKind carrier)
	{
		var host = Compiled(
			"Pair : @string = t: 'a' & eol & 'b' & eol => @(Text(t))\n" +
			"Start : @string = pairs: Pair* recover eol => " + Where + " => @(Join(pairs))\n" +
			"parse Start as ParseStart\n",
			direct, carrier);

		var told = Recovered(host, "a\nx\na\nb\n");

		Assert.True(told.Length == 3, rendering + ": " + string.Join(" / ", told));
		Told(told[0], "1:1 2:1@2", "'b'", rendering);
		Told(told[1], "2:1 2:1@2", "'a'", rendering);
		Assert.DoesNotContain("'b'", told[1], StringComparison.Ordinal);
		Assert.Equal("a", told[2]);
	}

	/// <summary>Input with lines ended every way <c>eol</c> ends one: <c>\r\n</c>, <c>\r</c> and <c>\n</c>.</summary>
	const string Mixed = "a\r\na\rx\na\r\nx\r\na\r";

	const string Rows = "Row : @string = t: 'a' & eol => @(Text(t))\n";

	/// <summary>A line ends where <c>eol</c> ends one, in the coordinates and in the message, over a string.</summary>
	[Theory]
	[MemberData(nameof(Renderings))]
	public void A_line_ends_where_eol_ends_one(string rendering, bool direct, CarrierKind carrier)
	{
		var host = Compiled(Rows + "Start : @string = rows: Row* recover eol => " + Where + " => @(Join(rows))\nparse Start as ParseStart\n", direct, carrier);
		var told = Recovered(host, Mixed);

		Assert.True(told.Length == 6, rendering + ": " + string.Join(" / ", told));
		Told(told[2], "3:1 3:1@5", "'a'", rendering);
		Told(told[4], "5:1 5:1@10", "'a'", rendering);
	}

	/// <summary>The same from a <c>TextReader</c>, through windows that cut a <c>\r\n</c> in two.</summary>
	[Theory]
	[InlineData(2)]
	[InlineData(3)]
	[InlineData(5)]
	[InlineData(4096)]
	public void A_line_ends_where_eol_ends_one_in_a_stream(int window)
	{
		var host = Compiled(Rows + "Start : @string[] = Row* recover eol => " + Where + "\nparse Start as ReadStart stream yield\n", direct: true, CarrierKind.Auto);
		var told = Read(host, "ReadStart", new StringReader(Mixed), window);

		Assert.True(told.Length == 6, string.Join(" / ", told));
		Told(told[2], "3:1 3:1@5", "'a'", "window " + window);
		Told(told[4], "5:1 5:1@10", "'a'", "window " + window);
	}

	/// <summary>
	/// The same over bytes, where only the message says the place, and over the buffered
	/// <c>TextReader</c> beside it, through buffers that cut a <c>\r\n</c> in two.
	/// </summary>
	[Theory]
	[InlineData(4)]
	[InlineData(5)]
	[InlineData(4096)]
	public void A_line_ends_where_eol_ends_one_over_bytes(int buffer)
	{
		foreach (var (form, over) in new (string, Func<object>)[]
		{
			("stream bytes", () => new MemoryStream(System.Text.Encoding.ASCII.GetBytes(Mixed))),
			("stream", () => new StringReader(Mixed)),
		})
		{
			var host = Compiled(
				Rows + "Start : @string[] = Row* recover eol => @(Text(parserMessage))\nparse Start as ParseStart " + form + "\n",
				direct: true, CarrierKind.Auto);
			var input = over();
			var parse = host.GetType("Grammar")!.GetMethods().Single(one =>
				one.Name == "TryParseStart" && one.GetParameters()[0].ParameterType == (input is Stream ? typeof(Stream) : typeof(TextReader)) &&
				one.ReturnType.Name.StartsWith("Match", StringComparison.Ordinal));
			var arguments = parse.GetParameters().Select((one, at) => at == 0 ? input : one.Name == "bufferSize" ? buffer : one.DefaultValue).ToArray();
			var match = parse.Invoke(null, arguments)!;
			var value = (IEnumerable)match.GetType().GetProperty("Value")!.GetValue(match)!;
			var told  = value.Cast<object>().Select(static one => one.ToString()!).ToArray();

			Assert.True(told.Length == 6, form + ": " + string.Join(" / ", told));
			Assert.EndsWith("'a' at 3:1.", told[2], StringComparison.Ordinal);
			Assert.EndsWith("'a' at 5:1.", told[4], StringComparison.Ordinal);
		}
	}

	/// <summary>Read from a position, or within a window of the input, the place is still the input's own.</summary>
	[Theory]
	[MemberData(nameof(Renderings))]
	public void A_reading_from_a_position_says_the_place_in_the_whole_input(string rendering, bool direct, CarrierKind carrier)
	{
		var host  = Compiled(Rows + "Start : @string = rows: Row* recover eol => " + Where + " & eof => @(Join(rows))\nparse Start as ParseStart\n", direct, carrier);
		var type  = host.GetType("Grammar")!;
		const string Input = "zz\na\nx\na\n";

		foreach (var arguments in new object[][] { [Input, 3], [Input, 3, Input.Length - 3] })
		{
			var match = type.GetMethod("TryParseStart", [.. arguments.Select(static one => one.GetType())])!.Invoke(null, arguments)!;
			var told  = ((string)match.GetType().GetProperty("Value")!.GetValue(match)!).Split('\u0001');

			Assert.True(told.Length == 3, rendering + ": " + string.Join(" / ", told));
			Told(told[1], "3:1 3:1@5", "'a'", rendering);
		}
	}

	/// <summary>
	/// The forwarding rule of §6's spelling is an element in every rendering, alternatives that
	/// begin alike included, and answers as the grouped spelling does.
	/// </summary>
	[Theory]
	[MemberData(nameof(Renderings))]
	public void A_forwarding_rule_recovers_alike_in_every_rendering(string rendering, bool direct, CarrierKind carrier)
	{
		const string Lines = """
			Sheet : @string = lines: Line* recover eol => @(Text(parserMessage)) & eof => @(Join(lines))
			Item  : @string = d: ['0'..'9']+ & 'x' & eol => @(Text(d))
			Note  : @string = d: ['0'..'9']+ & '#' & eol => @(Text(d) + "#")
			parse Sheet as ParseStart

			""";
		const string Input = "1x\n2#\n3?\n4x\n";

		var perAlternative = Recovered(Compiled("Line : @string = v: Item => @(v) | v: Note => @(v)\n" + Lines, direct, carrier), Input);
		var grouped        = Recovered(Compiled("Line : @string = (v: Item | v: Note) => @(v)\n" + Lines, direct, carrier), Input);

		Assert.True(grouped.SequenceEqual(perAlternative), rendering + ": " + string.Join(" / ", perAlternative));
		Assert.Equal(["1", "2#", "4"], new[] { perAlternative[0], perAlternative[1], perAlternative[3] });
		Assert.EndsWith(" at 3:2.", perAlternative[2], StringComparison.Ordinal);
	}

	/// <summary>
	/// What a choice says over the call that begins its widest group, where something else was
	/// recorded at the place first: that stays, said beside the choice's set.
	/// </summary>
	[Theory]
	[MemberData(nameof(Readers))]
	public void A_choice_said_over_its_call_keeps_what_stood_there_before(string rendering, bool direct, CarrierKind carrier)
	{
		var host  = Compiled(Choice + "Start : @int = ('q' & 'z')? & e: Expr => @(e)\nparse Start as ParseStart\n", direct, carrier);
		var error = EmittedCode.Match(host, "Grammar", "TryParseStart", "x").Error!;

		Assert.False(error.Contains("Digit", StringComparison.Ordinal), rendering + ": " + error);
		Assert.Contains("'q'", error, StringComparison.Ordinal);
		Assert.Contains("['(' | '-' | '0'..'9']", error, StringComparison.Ordinal);
	}

	/// <summary>A rule that says its own refusal is heard over the choice that calls it.</summary>
	[Theory]
	[MemberData(nameof(Readers))]
	public void A_choice_said_over_its_call_keeps_the_rules_own_words(string rendering, bool direct, CarrierKind carrier)
	{
		var host = Compiled(
			Choice.Replace("Number : @int =", "Number : @int on fail \"Expected a number.\" =") + "parse Expr as ParseStart\n",
			direct, carrier);

		Assert.True(EmittedCode.Match(host, "Grammar", "TryParseStart", "x").Error == "Expected a number.", rendering);
	}

	/// <summary>A dispatch inside the call, whose own widest group begins with a call too: every set said once.</summary>
	[Theory]
	[MemberData(nameof(Readers))]
	public void A_choice_said_over_a_nested_dispatch_says_each_set_once(string rendering, bool direct, CarrierKind carrier)
	{
		const string Nested = """
			Digit  = ['0'..'9']
			Dec    : @int = d: Digit+ => @(int.Parse(Text(d)))
			Hex    : @int = "0x" & d: ['0'..'9' | 'a'..'f']+ => @(System.Convert.ToInt32(Text(d), 16))
			Value  : @int = '#' & v: Hex => @(v) | '%' & v: Hex => @(-v) | v: Dec => @(v)
			Expr   : @int = '(' & e: Expr & ')' => @(e) | '-' & e: Expr => @(-e) | v: Value => @(v)
			parse Expr as ParseStart

			""";

		var host = Compiled(Nested, direct, carrier);

		foreach (var input in new[] { "x", "-x", "(x" })
		{
			var error = EmittedCode.Match(host, "Grammar", "TryParseStart", input).Error!;
			var items = error.Substring("Expected ".Length).TrimEnd('.').Replace(" or ", ", ").Split(", ");

			Assert.False(error.Contains("Digit", StringComparison.Ordinal), rendering + ": " + error);
			Assert.Equal(items.Length, items.Distinct().Count());
		}
	}

	/// <summary>A group led by a guard that says no is not named, and the rest is said once.</summary>
	[Theory]
	[MemberData(nameof(Readers))]
	public void A_choice_said_over_its_call_under_a_guard_says_each_set_once(string rendering, bool direct, CarrierKind carrier)
	{
		var host = Compiled(
			Choice.Replace("| '-' & e: Expr => @(-e)", "| when @(Allowed()) & '-' & e: Expr => @(-e)") + "parse Expr as ParseStart\n",
			direct, carrier);
		var error = EmittedCode.Match(host, "Grammar", "TryParseStart", "x").Error!;

		Assert.False(error.Contains("Digit", StringComparison.Ordinal), rendering + ": " + error);
		Assert.DoesNotContain("'-'", error, StringComparison.Ordinal);
	}

	/// <summary>
	/// A turn that wants a set said at the same place before it is told that set: an element
	/// that wanted <c>'a'</c> where the one before had wanted <c>'a'</c> too.
	/// </summary>
	[Theory]
	[MemberData(nameof(Renderings))]
	public void A_turn_that_wants_what_was_wanted_there_before_is_told_it(string rendering, bool direct, CarrierKind carrier)
	{
		const string Pair = "Pair : @string = 'a' & eol & 'a' & eol => @(\"ok\")\n";

		foreach (var start in new[]
		{
			"Start : @string[] = Pair* recover eol => @(Text(parserMessage))\n",
			"Start : @string = pairs: Pair* recover eol => @(Text(parserMessage)) => @(Join(pairs))\n",
		})
		{
			var told = Recovered(Compiled(Pair + start + "parse Start as ParseStart\n", direct, carrier), "a\nx\n");

			Assert.True(told.Length == 2, rendering + ": " + string.Join(" / ", told));
			Assert.EndsWith("'a' at 2:1.", told[0], StringComparison.Ordinal);
			Assert.True(told[1].StartsWith("Expected ", StringComparison.Ordinal) && told[1].EndsWith("'a' at 2:1.", StringComparison.Ordinal), rendering + ": " + told[1]);
		}
	}

	/// <summary>
	/// A recovering repetition read inside an element of another keeps to its own turn: the outer
	/// element stopped where it wanted <c>'b'</c>, and the inner repetition, which a look ahead
	/// in the other alternative read, began its turn there and is not the outer one's.
	/// </summary>
	[Theory]
	[MemberData(nameof(Renderings))]
	public void A_recovering_repetition_inside_an_element_keeps_the_outer_turn(string rendering, bool direct, CarrierKind carrier)
	{
		const string Grammar = """
			Row   : @string = 'r' => @("r")
			Inner : @string = rows: Row* recover eol => @(Text(parserMessage)) & 'a' & '\n' => @("")
			Item  : @string = 'a' & '\n' & 'b' => @("") | ?=Inner & 'c' => @("")

			""";

		foreach (var start in new[]
		{
			"Start : @string[] = Item* recover eol => @(Text(parserMessage))\n",
			"Start : @string = items: Item* recover eol => @(Text(parserMessage)) => @(Join(items))\n",
		})
		{
			var told = Recovered(Compiled(Grammar + start + "parse Start as ParseStart\n", direct, carrier), "a\nx\n");

			Assert.True(told.Length == 2, rendering + ": " + string.Join(" / ", told));
			Assert.True(
				told[0].StartsWith("Expected ", StringComparison.Ordinal) && told[0].Contains('b', StringComparison.Ordinal) &&
				told[0].EndsWith(" at 2:1.", StringComparison.Ordinal),
				rendering + ": " + told[0]);
		}
	}

	/// <summary>
	/// A synchronization that recovers in a repetition of its own does not take the place of what
	/// the element it steps over wanted.
	/// </summary>
	[Theory]
	[MemberData(nameof(Renderings))]
	public void A_synchronization_that_recovers_itself_keeps_the_elements_explanation(string rendering, bool direct, CarrierKind carrier)
	{
		const string Grammar = """
			Item : @string = 'a' => @("")
			Row  : @string = 'r' => @("")
			Sync : @string = rows: Row* recover ';' => @(Text(parserMessage)) & ']' => @("")

			""";

		foreach (var start in new[]
		{
			"Start : @string[] = Item* recover Sync => @(Text(parserMessage))\n",
			"Start : @string = items: Item* recover Sync => @(Text(parserMessage)) => @(Join(items))\n",
		})
		{
			var told = Recovered(Compiled(Grammar + start + "parse Start as ParseStart\n", direct, carrier), "x;]");

			Assert.True(told.Length == 1, rendering + ": " + string.Join(" / ", told));
			Assert.True(told[0].StartsWith("Expected ", StringComparison.Ordinal) && told[0].EndsWith("'a' at 1:1.", StringComparison.Ordinal), rendering + ": " + told[0]);
		}
	}

	/// <summary>
	/// A recovering repetition reached again inside its own element, through a look ahead into the
	/// rule that owns it, keeps the outer turn: the outer element stopped where it wanted <c>'b'</c>,
	/// and the inner reading of the same repetition began turns of its own in between.
	/// </summary>
	[Theory]
	[MemberData(nameof(Renderings))]
	public void A_recovering_repetition_reached_again_inside_its_own_element_keeps_the_outer_turn(string rendering, bool direct, CarrierKind carrier)
	{
		const string Item = """
			Item : @string = 'a' & '\n' & 'b' => @("")
			               | 'a' & ?=Start & 'c' => @("")

			""";

		foreach (var start in new[]
		{
			"Start : @string[] = Item* recover eol => @(Text(parserMessage))\n",
			"Start : @string = items: Item* recover eol => @(Text(parserMessage)) => @(Join(items))\n",
		})
		{
			var told = Recovered(Compiled(Item + start + "parse Start as ParseStart\n", direct, carrier), "a\nx\n");

			Assert.True(told.Length == 2, rendering + ": " + string.Join(" / ", told));
			Assert.True(
				told[0].StartsWith("Expected ", StringComparison.Ordinal) && told[0].Contains('b', StringComparison.Ordinal) &&
				told[0].EndsWith(" at 2:1.", StringComparison.Ordinal),
				rendering + ": " + told[0]);
		}
	}

	/// <summary>
	/// The same where the element reads the rule again by recursion: each reading of the
	/// repetition tells its own elements what they wanted, the inner one inside the brackets and
	/// the outer one after them.
	/// </summary>
	[Theory]
	[MemberData(nameof(Renderings))]
	public void A_recovering_repetition_read_again_by_recursion_tells_each_element_its_own(string rendering, bool direct, CarrierKind carrier)
	{
		const string Grammar = """
			Item  : @string = '(' & s: List & ')' => @("(" + s + ")") | 'a' & 'b' => @("ab")
			List  : @string = items: Item* recover ';' => @("!" + Text(parserMessage)) => @(string.Join(",", items))
			parse List as ParseStart

			""";

		var host = Compiled(Grammar, direct, carrier);

		foreach (var (input, told) in new[]
		{
			("ab(ax;ab)az;ab", "ab,(!Expected \"ab\" at 1:5.,ab),!Expected \"ab\" at 1:11.,ab"),
			("(a;)x;ab", "(!Expected \"ab\" at 1:3.),!Expected end of input or ['(' | 'a'] at 1:5.,ab"),
		})
		{
			var match = EmittedCode.Match(host, "Grammar", "TryParseStart", input);

			Assert.True(match.IsSuccess && (string?)match.Value == told, $"{rendering} {input}: {match.Value ?? match.Error}");
		}
	}

	/// <summary>
	/// What the reading of the repetition inside a look ahead got to stays inside it: the outer
	/// element stopped at 2, and the inner reading's elements, which stopped further along, do not
	/// lend it their place or take its message, whether the look ahead holds or not.
	/// </summary>
	[Theory]
	[MemberData(nameof(Renderings))]
	public void A_look_ahead_into_the_repetition_keeps_its_places_to_itself(string rendering, bool direct, CarrierKind carrier)
	{
		const string Item = """
			Item : @string = 'a' & '\n' & 'b' => @("")
			               | 'a' & ?=Start & 'c' => @("")

			""";

		foreach (var (told, wanted) in new[]
		{
			("@(parserFailurePosition.ToString())", "2,2,4"),
			("@(Text(parserMessage))", "Expected \"a\\nb\" at 2:1.,Expected '!' or 'a' at 2:1.,Expected '!' or 'a' at 3:1."),
		})
		{
			var host  = Compiled(Item + "Start : @string = items: Item* recover eol => " + told + " & '!' => @(string.Join(\",\", items))\nparse Start as ParseStart\n", direct, carrier);
			var match = EmittedCode.Match(host, "Grammar", "TryParseStart", "a\nx\ny\n!");

			Assert.True(match.IsSuccess && (string?)match.Value == wanted, $"{rendering}: {match.Value ?? match.Error}");
		}
	}

	/// <summary>
	/// A scanned rule a publication's <c>with</c> cloned is named as the author wrote it where a
	/// refusal is recorded at how far it read: <c>Number</c>, never <c>Number_With1</c>.
	/// </summary>
	[Theory]
	[MemberData(nameof(Renderings))]
	public void A_scanned_rule_cloned_by_with_is_named_as_written(string rendering, bool direct, CarrierKind carrier)
	{
		const string Grammar = """
			Digit  = ['0'..'9']
			Other  = ['0'..'8']
			Number = Digit+ & ('.' & Digit+)?
			Start : @string = n: Number => @(Text(n))
			parse Start with (Digit = Other) as ParseStart

			""";

		var host = Compiled(Grammar, direct, carrier);

		foreach (var input in new[] { "1.x", "x" })
		{
			var match = EmittedCode.Match(host, "Grammar", "TryParseStart", input);

			Assert.False(match.IsSuccess, rendering + ": " + input);
			Assert.True(match.Error is { } error && !error.Contains("_With", StringComparison.Ordinal), $"{rendering} {input}: {match.Error}");
		}
	}

	/// <summary>
	/// An optional group that began to match and stopped is where reading got furthest, and a
	/// refusal says so: <c>1.x</c> wanted a digit after the point, not the end of the input after
	/// the <c>1</c>. Over a string, a <c>TextReader</c> and a byte stream, in every rendering, and
	/// with the number read in place, by a rule of its own and inside a value.
	/// </summary>
	[Theory]
	[MemberData(nameof(Renderings))]
	public void An_optional_group_that_stopped_part_way_is_where_the_refusal_is(string rendering, bool direct, CarrierKind carrier)
	{
		foreach (var (grammar, wanted) in new[]
		{
			("Number = ['0'..'9']+ & ('.' & ['0'..'9']+)?\nparse Number as ParseStart", "Expected ['0'..'9']."),
			("Digit = ['0'..'9']\nNumber = Digit+ & ('.' & ['0'..'9']+)?\nparse Number as ParseStart", "Expected ['0'..'9']."),
			("Digit = ['0'..'9']\nNumber = Digit+ & ('.' & ['0'..'9']+)?\nStart = Number\nparse Start as ParseStart", null),
			("Digit = ['0'..'9']\nNumber = Digit+ & ('.' & ['0'..'9']+)?\nStart : @string = n: Number => @(Text(n))\nparse Start as ParseStart", null),
			("Start : @string = n: (['0'..'9']+ & ('.' & ['0'..'9']+)?) => @(Text(n))\nparse Start as ParseStart", "Expected ['0'..'9']."),
		})
		{
			foreach (var form in new[] { " stream", " stream bytes" })
			{
				var host = Compiled(grammar + form + "\n", direct, carrier);

				foreach (var input in new[] { "1.x", "1." })
				{
					foreach (var (over, position, error) in Refusals(host, input))
					{
						var context = $"{rendering} {grammar.Replace('\n', ' ')} {over} {input}";

						Assert.True(position == 2, context + ": refused at " + position + ": " + error);

						if (wanted is not null)
							Assert.True(error == wanted, context + ": " + error);
					}
				}
			}
		}
	}

	/// <summary>
	/// The same inside an element of a recovering repetition: what the element is told it wanted
	/// is where its optional group stopped, and that is where its failure is said to be.
	/// </summary>
	[Theory]
	[MemberData(nameof(Renderings))]
	public void An_optional_group_that_stopped_part_way_is_what_a_rejected_element_wanted(string rendering, bool direct, CarrierKind carrier)
	{
		const string Grammar = """
			Item  : @string = 'a' & ('.' & ['b'..'c'])? & ';' => @("a")
			Start : @string = items: Item* recover ';' => @(Told(parserLine, parserColumn, parserFailureLine, parserFailureColumn, parserFailurePosition, parserExpected, parserMessage)) => @(Join(items))
			parse Start as ParseStart

			""";

		var told = Recovered(Compiled(Grammar, direct, carrier), "a;a.x;a.b;");

		Assert.True(told.Length == 3, rendering + ": " + string.Join(" / ", told));
		Told(told[1], "1:3 1:5@4", "['b'..'c']", rendering);
	}

	/// <summary>The calculator of the README, over <c>int</c>.</summary>
	const string Calculator = """
		trivia = Std.Spacing?

		Value : @int = d: Std.Digits => @(int.Parse(d))

		Expr : Value = left: Expr & '+' & right: Expr  << 1 => @(left + right)
		             | left: Expr & '-' & right: Expr  << 1 => @(left - right)
		             | left: Expr & '*' & right: Expr  << 2 => @(left * right)
		             | left: Expr & '/' & right: Expr  << 2 => @(left / right)
		             | left: Expr & '^' & right: Expr  >> 3 => @(Raise(left, right))
		             | '-' & operand: Expr             >> 3 => @(-operand)
		             | '(' & inner: Expr & ')'              => @(inner)
		             | value: Value                         => @(value)

		IntNumber : @int = d: Std.Digits => @(int.Parse(d))

		parse Expr with (Value = IntNumber) as ParseStart

		""";

	/// <summary>
	/// What the calculator of the README can say where an operator or the end was wanted: one item
	/// for each operator, never the source of the alternatives that read them.
	/// </summary>
	[Theory]
	[MemberData(nameof(Renderings))]
	public void An_expected_set_names_what_begins_each_alternative(string rendering, bool direct, CarrierKind carrier)
	{
		var result = GramCompiler.Compile(Calculator, new GramCompilerOptions
		{
			ClassName = "Grammar", Direct = direct, Carrier = carrier, CSharpScanner = RoslynCSharpScanner.Instance,
		});

		EmittedCode.Quiet(result.Diagnostics);

		var source = Assert.Single(result.Sources).Text;

		// Every item of every set is a literal, a class or a name, and no alternative's source.
		foreach (System.Text.RegularExpressions.Match set in System.Text.RegularExpressions.Regex.Matches(source, @"new string\[\] \{ (?<items>[^}]*) \}"))
			Assert.False(
				set.Groups["items"].Value.Contains(" & ", StringComparison.Ordinal) || set.Groups["items"].Value.Contains("=>", StringComparison.Ordinal),
				rendering + ": " + set.Value);
	}

	/// <summary>
	/// What the calculator of the README says where an operator or the end was wanted: each
	/// operator, and not the spaces the seam before one would have taken, in every rendering.
	/// </summary>
	[Theory]
	[MemberData(nameof(Renderings))]
	public void A_choice_of_operators_after_the_seam_says_each_operator(string rendering, bool direct, CarrierKind carrier)
	{
		var host = Compiled(Calculator, direct, carrier, "static int Raise(int left, int right) { return (int)System.Math.Pow(left, right); }");

		foreach (var (input, position, error) in new[]
		{
			("1.5", 1, "Expected '+', '-', '*', '/', '^' or end of input."),
			("(1", 2, "Expected '+', '-', '*', '/', '^' or ')'."),
			("(1 x", 3, "Expected '+', '-', '*', '/', '^' or ')'."),
			("1 x", 2, "Expected '+', '-', '*', '/', '^' or end of input."),
		})
		{
			var match = EmittedCode.Match(host, "Grammar", "TryParseStart", input);

			Assert.False(match.IsSuccess, rendering + ": " + input);
			Assert.True(match.Position == position && match.Error == error, $"{rendering} {input}: {match.Position}: {match.Error}");
		}
	}

	/// <summary>Where and what a refusal says over a string, and over the stream the publication also reads.</summary>
	static (string Over, long Position, string? Error)[] Refusals(Assembly host, string input)
	{
		var type   = host.GetType("Grammar")!;
		var direct = EmittedCode.Match(host, "Grammar", "TryParseStart", input);
		var found  = new System.Collections.Generic.List<(string, long, string?)>();

		Assert.False(direct.IsSuccess, input);
		found.Add(("string", direct.Position, direct.Error));

		foreach (var parse in type.GetMethods().Where(static one =>
			one.Name == "TryParseStart" && one.ReturnType.Name.StartsWith("Match", StringComparison.Ordinal) &&
			(one.GetParameters()[0].ParameterType == typeof(Stream) || one.GetParameters()[0].ParameterType == typeof(TextReader))))
		{
			object over = parse.GetParameters()[0].ParameterType == typeof(Stream)
				? new MemoryStream(System.Text.Encoding.ASCII.GetBytes(input))
				: new StringReader(input);
			var arguments = parse.GetParameters().Select((one, at) => at == 0 ? over : one.DefaultValue).ToArray();
			var match     = parse.Invoke(null, arguments)!;

			object? Read(string name)
			{
				return match.GetType().GetProperty(name)!.GetValue(match);
			}

			Assert.False((bool)Read("IsSuccess")!, input);
			found.Add((over is Stream ? "bytes" : "reader", (long)Read("Position")!, (string?)Read("Error")));
		}

		Assert.True(found.Count == 2, "a stream form was published: " + found.Count);

		return [.. found];
	}

	const string Choice = """
		trivia = ' '*
		Digit  = ['0'..'9']
		Number : @int = d: Digit+ => @(int.Parse(Text(d)))
		Expr   : @int = '(' & e: Expr & ')' => @(e) | '-' & e: Expr => @(-e) | n: Number => @(n)

		""";

	/// <summary>
	/// What a parse that rejected many elements kept for them goes with the parse: the pooled
	/// parser and tape a later parse is handed do not hold it.
	/// </summary>
	[Theory]
	[MemberData(nameof(Renderings))]
	public void What_was_kept_for_many_rejections_is_not_pooled(string rendering, bool direct, CarrierKind carrier)
	{
		var host  = Compiled(Rows + "Start : @string = rows: Row* recover eol => " + Where + " => @(Join(rows))\nparse Start as ParseStart\n", direct, carrier);
		var heavy = string.Concat(Enumerable.Repeat("x\n", 1000));

		Assert.Equal(1000, Recovered(host, heavy).Length);
		Assert.Single(Recovered(host, "a\n"));

		foreach (var pooled in Pooled(host.GetType("Grammar")!))
		{
			var kept = pooled.GetType().GetField("Expectations", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!.GetValue(pooled);

			// Its room, which a dictionary cleared keeps, and not only its count.
			var room = kept is null ? 0 : (int)kept.GetType().GetMethod("EnsureCapacity")!.Invoke(kept, [0])!;

			Assert.True(room < 1000, rendering + ": a pooled dictionary keeps room for " + room);
		}

		// The immediate carrier keeps nothing past the element it builds.
		if (carrier == CarrierKind.Immediate)
			return;

		Assert.NotEmpty(Pooled(host.GetType("Grammar")!));
	}

	/// <summary>The spare parser and tape a parse leaves pooled, where this thread has one.</summary>
	static object[] Pooled(Type host)
	{
		var found = new System.Collections.Generic.List<object>();

		foreach (var type in host.GetNestedTypes(BindingFlags.NonPublic).Append(host))
			foreach (var field in type.GetFields(BindingFlags.Static | BindingFlags.NonPublic))
				if (field.FieldType.GetField("Expectations", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public) is not null &&
					field.GetValue(null) is { } spare)
					found.Add(spare);

		return [.. found];
	}

	static string[] Read(Assembly host, string method, TextReader reader, int window)
	{
		var read = host.GetType("Grammar")!.GetMethods()
			.Single(one => one.Name == method && one.GetParameters()[0].ParameterType == typeof(TextReader));

		return [.. ((IEnumerable)read.Invoke(null, [reader, window, null])!).Cast<object>().Select(static one => one.ToString()!)];
	}

	/// <summary>
	/// A rejection told as <see cref="Where"/> tells it: the places exactly, and of what would
	/// have fit, that it ends with <paramref name="wanted"/> — before it may stand what the
	/// continuation tried at the same place wanted, which renderings say differently.
	/// </summary>
	static void Told(string told, string places, string wanted, string context)
	{
		var bar = told.IndexOf(" | ", StringComparison.Ordinal);
		var at  = places.Split(' ')[1].Split('@')[0];

		Assert.True(bar > places.Length && told.StartsWith(places + " ", StringComparison.Ordinal), context + ": " + told);
		Assert.True(told.Substring(places.Length + 1, bar - places.Length - 1).EndsWith(wanted, StringComparison.Ordinal), context + ": " + told);
		Assert.True(told.Substring(bar + 3).StartsWith("Expected ", StringComparison.Ordinal), context + ": " + told);
		Assert.True(told.EndsWith(wanted + " at " + at + ".", StringComparison.Ordinal), context + ": " + told);
	}

	static Assembly Compiled(string grammar, bool direct, CarrierKind carrier, string helpers = "")
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName = "Grammar", Direct = direct, Carrier = carrier, CSharpScanner = RoslynCSharpScanner.Instance,
		});

		EmittedCode.Quiet(result.Diagnostics);

		var source = Assert.Single(result.Sources).Text;

		// Held to what it is about: where a reader was asked for, a recovering repetition of the
		// root is the reader's and not the engine's (RecoveringReaderTests says the same). One in
		// another rule as well keeps the engine (Machine.UnreadRecovery), and so does the root's
		// where the rule is read again inside its own element.
		if (direct && grammar.Split("recover").Length == 2 && !grammar.Contains("?=Start", StringComparison.Ordinal) && System.Text.RegularExpressions.Regex.IsMatch(grammar, @"^\s*(Start|PriceList|Sheet)\s*: @string = \w+: \w+\* recover", System.Text.RegularExpressions.RegexOptions.Multiline))
			Assert.Contains("failure.Reach = p;", source, StringComparison.Ordinal);

		return EmittedCode.Compile(source, declarationMembers: Helpers + helpers);
	}

	static string[] Recovered(Assembly host, string input = Prices)
	{
		var match = EmittedCode.Match(host, "Grammar", "TryParseStart", input);

		Assert.True(match.IsSuccess, match.Error);

		return match.Value is string joined
			? joined.Split('\u0001')
			: [.. ((IEnumerable)match.Value!).Cast<object>().Select(static one => one?.ToString() ?? "<null>")];
	}
}
