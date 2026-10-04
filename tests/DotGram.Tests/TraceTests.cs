using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A trace build (docs/syntax.md, §6.9) over small grammars, one reading shape at a time: what it
/// reports, to which sink, across which boundaries, and that it answers what the parser answers.
/// The shipped grammars are held to the same over corpora in DotGram.Trace.Tests.
/// </summary>
/// <remarks>
/// The sink types are generated into each host, so a test cannot name them: what reads them is
/// compiled into the host beside the parser (<see cref="Probes"/>) and called by reflection.
/// </remarks>
public sealed class TraceTests(ITestOutputHelper output)
{
	const string Twice = """
		trivia = { ' '* }

		Sum : @int = first: Term & rest: More* & when @(first >= 0) => @(first + rest.Length)

		More : @int = '+' & value: Term => @(value)

		Term : @int
			= '(' & inner: Sum & ')'                                 => @(inner)
			| high: Digits & '^' & low: Digits & when @(high >= low) => @(high - low)
			| digits: Digits                                         => @(digits)

		Digits : @int = text: ['0'..'9']+ => @(int.Parse(text))

		parse Sum
		""";

	/// <summary>
	/// What every test host carries beside its parser: a sink that writes each event as a line, and
	/// ways to read a publication with it, with <c>GramWhy</c>, with the log and with the profile.
	/// </summary>
	/// <remarks>Compiled at the C# floor with the generated file, so written as that file is.</remarks>
	const string Probes = """
		public sealed class Recorder : GramTrace
		{
			public readonly global::System.Collections.Generic.List<string> Lines = new global::System.Collections.Generic.List<string>();
			readonly global::System.Collections.Generic.List<GramRead> _reads = new global::System.Collections.Generic.List<GramRead>();

			string Rule(int rule)
			{
				return _reads.Count > 0 ? _reads[_reads.Count - 1].RuleName(rule) : "#" + rule;
			}

			public override void Begin(GramRead read)
			{
				_reads.Add(read);
				Lines.Add("begin " + (read.Quiet ? "quiet " : "recording ") + read.Publication + " " + read.Machine + (read.Finding ? " find" : "") + " at " + read.Start);
			}

			public override void End(GramRead read, int end, int position, string[]? expected, global::System.Collections.Generic.IReadOnlyList<string[]>? expectedMore)
			{
				_reads.RemoveAt(_reads.Count - 1);
				Lines.Add("end " + end + " furthest " + position + (expected != null ? " " + string.Join(",", expected) : ""));
			}

			public override void Enter(int rule, int position)
			{
				Lines.Add("enter " + Rule(rule) + " " + position);
			}

			public override void Exit(int rule, int position, int end)
			{
				Lines.Add("exit " + Rule(rule) + " " + position + " " + end);
			}

			public override void Retracted(int rule, int position)
			{
				Lines.Add("retracted " + Rule(rule) + " " + position);
			}

			public override void Remembered(int rule, int position)
			{
				Lines.Add("remembered " + Rule(rule) + " " + position);
			}

			public override void Guard(int guard, int position, bool passed)
			{
				Lines.Add("guard " + (_reads.Count > 0 ? _reads[_reads.Count - 1].GuardText(guard) : "#" + guard) + " " + position + " " + passed);
			}

			public override void Refused(int position, string[]? expected)
			{
				Lines.Add("refused " + position + (expected != null ? " " + string.Join(",", expected) : ""));
			}

			public override void Recovered(int rule, int from, int to, int reach)
			{
				Lines.Add("recovered " + Rule(rule) + " " + from + " " + to + " " + reach);
			}

			public override void Deepened(int position)
			{
				Lines.Add("deepened " + position);
			}

			public override void Unlexed(string text, int position)
			{
				Lines.Add("unlexed " + position);
			}

			public override void Rejected(long position, string message)
			{
				Lines.Add("rejected " + position + " " + message);
			}
		}

		public static string[] Recorded(global::System.Func<object?> read)
		{
			var recorder = new Recorder();

			using (Tracing(recorder))
				read();

			return recorder.Lines.ToArray();
		}

		public static string Explained(global::System.Func<object?> read)
		{
			var why = new GramWhy();

			using (Tracing(why))
				read();

			return why.ToString();
		}

		public static string Logged(global::System.Func<object?> read)
		{
			var writer = new global::System.IO.StringWriter();

			using (Tracing(new GramTraceLog(writer)))
				read();

			return writer.ToString();
		}

		public static string Profiled(global::System.Func<object?> read)
		{
			var profile = new GramProfile();

			using (Tracing(profile))
				read();

			return profile.ToString();
		}
		""";

	/// <summary>The probes are written as the generated file is, with nullable annotations.</summary>
	const string Nullable = "\n#nullable enable\n";

	static Assembly Traced(
		string grammar, string? members = null, bool lexical = false, CarrierKind carrier = CarrierKind.Auto, bool buffered = false)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName     = "Grammar",
			CSharpScanner = RoslynCSharpScanner.Instance,
			Lexical       = lexical,
			Carrier       = carrier,
			BufferedInput = buffered,
			Trace         = true,
		});

		// A warning is the grammar's business, and some of these grammars are written to have one.
		Assert.DoesNotContain(result.Diagnostics, static one => one.Severity == GramSeverity.Error);

		return EmittedCode.Compile(
			result.Sources[0].Text, "Grammar", declarationMembers: Nullable + Probes + (members ?? ""),
			sourceParts: result.Sources.Skip(1).Select(static source => source.Text));
	}

	static Assembly Untraced(string grammar, bool lexical = false, CarrierKind carrier = CarrierKind.Auto)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName     = "Grammar",
			CSharpScanner = RoslynCSharpScanner.Instance,
			Lexical       = lexical,
			Carrier       = carrier,
		});

		EmittedCode.Quiet(result.Diagnostics);

		return EmittedCode.Compile(result.Sources[0].Text, "Grammar");
	}

	/// <summary>A static method of the host, by name and argument count.</summary>
	static object? Invoke(Assembly assembly, string method, params object?[] arguments)
	{
		var found = assembly.GetType("Grammar")!.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
			.First(one => one.Name == method && one.GetParameters().Length == arguments.Length &&
				one.GetParameters().Zip(arguments).All(pair => pair.Second is null || pair.First.ParameterType.IsInstanceOfType(pair.Second)));

		return found.Invoke(null, arguments);
	}

	/// <summary>A probe over a read the test describes: <c>Recorded</c>, <c>Explained</c>, <c>Logged</c> or <c>Profiled</c>.</summary>
	static T Probe<T>(Assembly assembly, string probe, Func<object?> read)
	{
		return (T)Invoke(assembly, probe, read)!;
	}

	static (bool Ok, string? Error, long Position) Read(Assembly assembly, string method, string input)
	{
		var (ok, _, error, position) = EmittedCode.Match(assembly, "Grammar", method, input);

		return (ok, error, position);
	}

	/// <summary>
	/// A refusal of a <c>TryParse</c> is read twice, quietly and then recording; only the second
	/// tells its refusals, every rule entered is left, and the call ends saying what the match says.
	/// </summary>
	[Fact]
	public void A_refused_parse_reads_quietly_then_recording_and_says_what_the_match_says()
	{
		var assembly = Traced(Twice);
		var lines    = Probe<string[]>(assembly, "Recorded", () => Read(assembly, "TryParseSum", "1+(2+"));
		var match    = Read(assembly, "TryParseSum", "1+(2+");

		output.WriteLine(string.Join(Environment.NewLine, lines));

		var begins = lines.Where(static line => line.StartsWith("begin ", StringComparison.Ordinal)).ToList();

		Assert.Equal(["begin quiet Sum methods at 0", "begin recording Sum methods at 0"], begins);
		Balanced(lines);

		var recording = lines.SkipWhile(static line => !line.StartsWith("begin recording", StringComparison.Ordinal)).ToList();
		var quiet     = lines.TakeWhile(static line => !line.StartsWith("begin recording", StringComparison.Ordinal)).ToList();

		Assert.DoesNotContain(quiet, static line => line.StartsWith("refused ", StringComparison.Ordinal));
		Assert.Contains(recording, static line => line.StartsWith("refused 5 ", StringComparison.Ordinal));
		Assert.Equal($"rejected {match.Position} {match.Error}", lines[^1]);
	}

	/// <summary>A reading that is accepted is read once, quietly, and says nothing is rejected.</summary>
	[Fact]
	public void An_accepted_parse_is_read_once()
	{
		var assembly = Traced(Twice);
		var lines    = Probe<string[]>(assembly, "Recorded", () => Read(assembly, "TryParseSum", "1+(2+3)"));

		Assert.Single(lines, static line => line.StartsWith("begin ", StringComparison.Ordinal));
		Assert.DoesNotContain(lines, static line => line.StartsWith("rejected ", StringComparison.Ordinal));
		Assert.StartsWith("end 7 ", lines[^1], StringComparison.Ordinal);
		Balanced(lines);
	}

	/// <summary>GramWhy's message is the match's, and its paths are the rules open where the input stopped.</summary>
	[Fact]
	public void Why_says_the_message_and_the_rules_reading_there()
	{
		var assembly = Traced(Twice);
		var why      = Probe<string>(assembly, "Explained", () => Read(assembly, "TryParseSum", "1+(2+"));
		var match    = Read(assembly, "TryParseSum", "1+(2+");

		output.WriteLine(why);

		Assert.StartsWith(match.Error + " (line 1, column 6)", why, StringComparison.Ordinal);
		Assert.Contains("in Sum 1:1 > More 1:2 > Term 1:3 > Sum 1:4 > More 1:5 > Term 1:6", why, StringComparison.Ordinal);
	}

	/// <summary>What the package's pages show GramWhy saying, said.</summary>
	[Fact]
	public void Why_says_what_the_pages_show()
	{
		var assembly = Traced("""
			Value : @int = '(' & inner: Value & ')' => @(inner) | digits: ['0'..'9']+ => @(int.Parse(digits))

			parse Value
			""");

		var why = Probe<string>(assembly, "Explained", () => Read(assembly, "TryParseValue", "((1"));

		Assert.Equal(
			"Expected ')'. (line 1, column 4)" + Environment.NewLine +
			"The rules below were reading there, each wanting what it names." + Environment.NewLine +
			"  wanted ')' in Value 1:1 > Value 1:2",
			why);
	}

	/// <summary>A guard that says no is named by its C#, at the place it was asked.</summary>
	[Fact]
	public void A_guard_that_refused_is_named()
	{
		var assembly = Traced(Twice);
		var why      = Probe<string>(assembly, "Explained", () => Read(assembly, "TryParseSum", "1^2"));

		output.WriteLine(why);

		Assert.Contains("when @(high >= low) said no in Sum 1:1 > Term 1:1", why, StringComparison.Ordinal);
	}

	/// <summary>The trace build answers every input the untraced one does, refusals word for word, with and without a sink.</summary>
	[Theory]
	[InlineData("1")]
	[InlineData("1+(2+")]
	[InlineData("1^2")]
	[InlineData("(1+2")]
	[InlineData("")]
	[InlineData("12 + (34 + 5^5) + 6")]
	public void A_trace_build_answers_as_the_parser_does(string input)
	{
		var traced   = Traced(Twice);
		var untraced = Untraced(Twice);

		Assert.Equal(Read(untraced, "TryParseSum", input), Read(traced, "TryParseSum", input));

		var withSink = default((bool, string?, long));

		Probe<string>(traced, "Explained", () => withSink = Read(traced, "TryParseSum", input));

		Assert.Equal(Read(untraced, "TryParseSum", input), withSink);
	}

	/// <summary>The log indents what a rule reads under it and says each refusal and guard.</summary>
	[Fact]
	public void The_log_shows_rules_entered_and_left()
	{
		var assembly = Traced(Twice);
		var log      = Probe<string>(assembly, "Logged", () => Read(assembly, "TryParseSum", "1^2"));

		output.WriteLine(log);

		Assert.Contains("recording reading of Sum at 1:1", log, StringComparison.Ordinal);
		Assert.Contains("  Term 1:1 \"1^2\"", log, StringComparison.Ordinal);
		Assert.Contains("when @(high >= low) at 1:4: no", log, StringComparison.Ordinal);
		Assert.Contains("refused at 3: Expected more input.", log, StringComparison.Ordinal);
	}

	/// <summary>The profile counts quiet readings apart from recording ones.</summary>
	[Fact]
	public void The_profile_counts_quiet_and_recording_readings_apart()
	{
		var assembly = Traced(Twice);
		var profile  = Probe<string>(assembly, "Profiled", () => Read(assembly, "TryParseSum", "1+(2+"));

		output.WriteLine(profile);

		var recording = profile.Substring(0, profile.IndexOf("quiet readings:", StringComparison.Ordinal));
		var quiet     = profile.Substring(profile.IndexOf("quiet readings:", StringComparison.Ordinal));

		Assert.Contains("\nSum\t", recording, StringComparison.Ordinal);
		Assert.Contains("\nSum\t", quiet, StringComparison.Ordinal);
		Assert.Contains("\nDigits\t", recording, StringComparison.Ordinal);
	}

	/// <summary>
	/// A rule that only hands on another's value is compiled into its callers and is still on the
	/// stack where it stood: the paths are those of a parser that calls it as written.
	/// </summary>
	[Theory]
	[InlineData("(1")]
	[InlineData("((1)")]
	[InlineData("(x")]
	[InlineData("")]
	[InlineData("1)")]
	public void A_forwarding_rule_is_framed_where_it_stood(string input)
	{
		const string forwarding = """
			Expr    : @int = e: Operand => @(e)
			Operand : @int = n: Number => @(n) | g: Group => @(g)
			Number  : @int = t: ['0'..'9']+ => @(int.Parse(t))
			Group   : @int = '(' & e: Expr & ')' => @(e)
			parse Expr
			""";

		var collapsed = Traced(forwarding);
		var written   = TracedAsWritten(forwarding);

		var said    = Probe<string>(collapsed, "Explained", () => Read(collapsed, "TryParseExpr", input));
		var wanted  = Probe<string>(written, "Explained", () => Read(written, "TryParseExpr", input));
		var lines   = Probe<string[]>(collapsed, "Recorded", () => Read(collapsed, "TryParseExpr", input));

		output.WriteLine(said);
		output.WriteLine(wanted);

		Assert.Equal(wanted, said);
		Balanced(lines);

		// Where the refusal is inside what the forwarding rule stood for, it is on the path.
		if (input != "1)")
			Assert.Contains(" > Operand 1:1", said, StringComparison.Ordinal);
	}

	/// <summary>
	/// A reading deep enough to run its stack low goes on on a stack of its own, and the sink goes
	/// with it: told of the move, told every rule entered there, and every one of them left.
	/// </summary>
	[Fact]
	public void A_reading_carried_onto_another_stack_reports_from_there()
	{
		const string nested = """
			Value : @int = '[' & inner: Value & ']' => @(inner + 1) | '1' => @(1)
			parse Value
			""";

		var assembly = Traced(nested);
		var input    = new string('[', 100_000);
		var lines    = Probe<string[]>(assembly, "Recorded", () => Read(assembly, "TryParseValue", input));
		var why      = Probe<string>(assembly, "Explained", () => Read(assembly, "TryParseValue", input));
		var match    = Read(assembly, "TryParseValue", input);

		Assert.Contains(lines, static line => line.StartsWith("deepened ", StringComparison.Ordinal));
		Balanced(lines);
		Assert.StartsWith(match.Error!, why, StringComparison.Ordinal);
		Assert.Matches(@"\.\.\. 9\d{4} more > Value", why);
	}

	/// <summary>A rule entered where it already failed is answered from memory, and says so rather than entering.</summary>
	[Fact]
	public void A_remembered_failure_is_said_and_not_entered()
	{
		const string tried = """
			trivia = { ' '* }
			namespace Lexical
			{
				trivia = none
				Name = ['a'..'z']+
			}
			Start = C & eof
			C = P & '+' | P & '-' | P
			P = '(' & C & ')' | Lexical.Name
			parse Start
			""";

		var assembly = Traced(tried, lexical: true);
		var lines    = Probe<string[]>(assembly, "Recorded", () => Read(assembly, "TryParseStart", "((((a"));
		var profile  = Probe<string>(assembly, "Profiled", () => Read(assembly, "TryParseStart", "((((a"));

		output.WriteLine(profile);

		Assert.Contains(lines, static line => line.StartsWith("remembered ", StringComparison.Ordinal));
		Balanced(lines);
	}

	/// <summary>
	/// Over tokens a position is a token, and the explanation says where it is in characters; a
	/// character no token begins with is said before any rule is read, and then the tokens before
	/// it are read for the sink, which is told which rules were reading where they ran out.
	/// </summary>
	[Fact]
	public void Over_tokens_a_position_is_said_in_characters()
	{
		var assembly = Traced(Snapshot("Lexical"), lexical: true);
		var refused  = "let a = 1;\nlet b = 2 + ;";
		var match    = Read(assembly, "TryParseProgram", refused);
		var why      = Probe<string>(assembly, "Explained", () => Read(assembly, "TryParseProgram", refused));
		var stopped  = Probe<string>(assembly, "Explained", () => Read(assembly, "TryParseProgram", "let a = #;"));
		var lines    = Probe<string[]>(assembly, "Recorded", () => Read(assembly, "TryParseProgram", "let a = #;"));

		output.WriteLine(why);
		output.WriteLine(stopped);

		Assert.StartsWith($"{match.Error} (line 2, column 13)", why, StringComparison.Ordinal);
		Assert.Contains("Statement 2:1", why, StringComparison.Ordinal);
		Assert.StartsWith("Unexpected character '#'. (line 1, column 9)", stopped, StringComparison.Ordinal);
		Assert.Contains("No token of the grammar begins at that character. The rules below were reading up to it", stopped, StringComparison.Ordinal);
		Assert.Contains("in Program 1:1 > Statement 1:1", stopped, StringComparison.Ordinal);
		Assert.Equal("unlexed 8", lines[0]);
		Assert.Equal("begin recording Program methods at 0", lines[1]);
		Assert.Equal("rejected 8 Unexpected character '#'.", lines[^1]);
		Balanced(lines);

		// Where the tokens before it are refused before they run out, or read whole and so refused
		// nothing there, the stop is all there is to say.
		foreach (var input in new[] { "let = 1; #", "let a = 1; #" })
		{
			var alone = Probe<string>(assembly, "Explained", () => Read(assembly, "TryParseProgram", input));

			output.WriteLine(alone);

			Assert.Contains("No token of the grammar begins at that character, so no rule was read.", alone, StringComparison.Ordinal);
		}
	}

	/// <summary>
	/// A reading on the engine reports the rules it calls as methods do, and the explanation names
	/// the rules reading where it refused.
	/// </summary>
	[Fact]
	public void A_reading_on_the_engine_reports_its_rules()
	{
		var assembly = Traced(Snapshot("Url"));
		var match    = Read(assembly, "TryParseUrl", "http://");
		var why      = Probe<string>(assembly, "Explained", () => Read(assembly, "TryParseUrl", "http://"));
		var lines    = Probe<string[]>(assembly, "Recorded", () => Read(assembly, "TryParseUrl", "http://"));

		output.WriteLine(why);
		output.WriteLine(string.Join(Environment.NewLine, lines));

		Assert.StartsWith(match.Error + " (line 1, column 8)", why, StringComparison.Ordinal);
		Assert.DoesNotContain("engine", why, StringComparison.Ordinal);
		Assert.Contains("in Url 1:1 > Authority 1:8", why, StringComparison.Ordinal);
		Assert.Contains(lines, static line => line.StartsWith("begin recording Url engine", StringComparison.Ordinal));
		Assert.Contains("enter Url 0", lines);
		Balanced(lines);
	}

	/// <summary>A grammar the engine reads, a <c>find</c> keeping it there, whose rules return and are gone back over.</summary>
	const string Pairs = """
		Pair : @string
			= left: Item & ',' & right: Item => @(left + right)
			| only: Item & ';'               => @(only)

		Item : @string = name: ['a'..'z']+ & ('(' & inner: Item & ')')? => @(name)

		parse Pair
		find Pair as AllPairs
		""";

	/// <summary>
	/// Where the engine goes back past a rule that returned, the rule is retracted and stays left;
	/// where it goes back into one, the rule is retracted and entered again where it was entered.
	/// </summary>
	[Fact]
	public void The_engine_retracts_what_it_goes_back_over()
	{
		var assembly = Traced(Pairs);
		var past     = Probe<string[]>(assembly, "Recorded", () => Read(assembly, "TryParsePair", "ab;"));
		var into     = Probe<string[]>(assembly, "Recorded", () => Read(assembly, "TryParsePair", "a(b);"));

		output.WriteLine(string.Join(Environment.NewLine, past));
		output.WriteLine("");
		output.WriteLine(string.Join(Environment.NewLine, into));

		Assert.Equal("begin quiet Pair engine at 0", past[0]);

		// Read, then given up for the second alternative, which reads it again.
		var back = Array.IndexOf(past, "retracted Item 0");

		Assert.True(back > 0);
		Assert.Equal("exit Item 0 2", past[..back].Last(static line => line.StartsWith("exit Item", StringComparison.Ordinal)));
		Assert.Equal("enter Item 0", past[(back + 1)..].First(static line => line.StartsWith("enter ", StringComparison.Ordinal)));
		Balanced(past);
		Balanced(into);
	}

	/// <summary>A refusal on the engine is explained by the rules reading where it was refused, in the match's words.</summary>
	[Theory]
	[InlineData("ab")]
	[InlineData("a(;")]
	[InlineData("a,b(")]
	[InlineData("a(b(c)")]
	[InlineData("")]
	public void Why_explains_the_engine_in_the_words_of_its_message(string input)
	{
		var assembly = Traced(Pairs);
		var match    = Read(assembly, "TryParsePair", input);
		var why      = Probe<string>(assembly, "Explained", () => Read(assembly, "TryParsePair", input));

		output.WriteLine(why);

		Assert.False(match.Ok);
		Assert.StartsWith(match.Error!, why, StringComparison.Ordinal);
		Assert.Contains(" in Pair 1:1", why, StringComparison.Ordinal);
		Assert.Equal(Read(Untraced(Pairs), "TryParsePair", input), match);
	}

	/// <summary>
	/// A grammar read as one flat method reports the rule it reads, what it refused and the rules
	/// it scans, and is explained in the words of its message.
	/// </summary>
	[Fact]
	public void A_flat_reading_reports_its_rule()
	{
		const string grammar = """
			Word = Letter+ & '!'
			Letter = ['a'..'z']
			parse Word
			""";

		var assembly = Traced(grammar);
		var match    = Read(assembly, "TryParseWord", "ab?");
		var lines    = Probe<string[]>(assembly, "Recorded", () => Read(assembly, "TryParseWord", "ab?"));
		var why      = Probe<string>(assembly, "Explained", () => Read(assembly, "TryParseWord", "ab?"));

		output.WriteLine(string.Join(Environment.NewLine, lines));
		output.WriteLine(why);

		Assert.Contains(lines, static line => line.StartsWith("begin recording Word flat at 0", StringComparison.Ordinal));
		Assert.Contains("enter Word 0", lines);
		Assert.Contains("exit Word 0 -1", lines);
		Assert.StartsWith(match.Error + " (line 1, column 3)", why, StringComparison.Ordinal);
		Assert.Contains(" in Word 1:1", why, StringComparison.Ordinal);
		Balanced(lines);
	}

	/// <summary>
	/// A reading over a reader is traced as the reading over a string is, by methods and on the
	/// engine alike, and explained in the words of its message.
	/// </summary>
	[Theory]
	[InlineData("Twice", "Sum", "1+(2+")]
	[InlineData("Twice", "Sum", "1+2")]
	[InlineData("Pairs", "Pair", "a(b(c)")]
	[InlineData("Pairs", "Pair", "a(b);")]
	public void A_buffered_reading_is_traced_as_the_string_is(string grammar, string rule, string input)
	{
		var assembly = Traced(grammar == "Twice" ? Twice : Pairs, buffered: true);
		var answer   = default(object);
		var lines    = Probe<string[]>(assembly, "Recorded", () => answer = Buffered(assembly, rule, input));
		var why      = Probe<string>(assembly, "Explained", () => Buffered(assembly, rule, input));
		var ok       = (bool)answer!.GetType().GetProperty("IsSuccess")!.GetValue(answer)!;
		var error    = (string?)answer.GetType().GetProperty("Error")!.GetValue(answer);

		output.WriteLine(string.Join(Environment.NewLine, lines));
		output.WriteLine(why);

		Assert.Contains(lines, line => line.StartsWith($"begin recording {rule} ", StringComparison.Ordinal));
		Assert.Contains($"enter {rule} 0", lines);
		Balanced(lines);

		if (ok)
			return;

		Assert.StartsWith(error!, why, StringComparison.Ordinal);
		Assert.Contains($" in {rule} 1:1", why, StringComparison.Ordinal);
	}

	static object Buffered(Assembly assembly, string rule, string input)
	{
		return Invoke(assembly, "TryParse" + rule, new System.IO.StringReader(input), null, null)!;
	}

	/// <summary>
	/// A <c>find</c> over a reader reads through a window that moves, and every position it
	/// reports is counted from the beginning of the input, as the matches' are.
	/// </summary>
	[Fact]
	public void A_streamed_find_reports_positions_in_the_whole_input()
	{
		const string members = """
			public static string[] FoundInReader(string input)
			{
				var recorder = new Recorder();
				global::System.Collections.Generic.IEnumerable<Match<string>> found;

				using (Tracing(recorder))
					found = FindName(new global::System.IO.StringReader(input));

				foreach (var one in found)
					recorder.Lines.Add("found " + one.Position);

				return recorder.Lines.ToArray();
			}
			""";

		var rows     = string.Concat(Enumerable.Range(0, 600).Select(static at => $"R|name{at}|{at}.25\n"));
		var input    = "H|2026-10-04\n" + rows + "T|600\n";
		var assembly = Traced(Snapshot("Feed"), members);
		var lines    = (string[])Invoke(assembly, "FoundInReader", input)!;
		var found    = lines.Where(static line => line.StartsWith("found ", StringComparison.Ordinal)).Select(static line => line[6..]).ToList();

		// Each occurrence is read from where it begins, which is where the match says it is.
		var begun = new HashSet<string>(lines
			.Where(static line => line.StartsWith("begin ", StringComparison.Ordinal))
			.Select(static line => line[(line.LastIndexOf(' ') + 1)..]));

		Assert.Equal(2 + 600 * 3 + 2, found.Count);
		Assert.All(found, at => Assert.Contains(at, begun));
		Assert.Contains(found, static at => int.Parse(at, System.Globalization.CultureInfo.InvariantCulture) > 4096);
		Balanced(lines);
	}

	/// <summary>
	/// A <c>parse</c> over a reader reads its parts one at a time, each a reading of its own told
	/// to the sink set where the call is made; a part the repetition steps over is refused there.
	/// </summary>
	[Fact]
	public void A_streamed_parse_reports_each_part()
	{
		const string members = """
			public static string[] SheetFromReader(string input)
			{
				var recorder = new Recorder();
				global::System.Collections.Generic.IEnumerable<string> rows;

				using (Tracing(recorder))
					rows = ParseSheet(new global::System.IO.StringReader(input));

				foreach (var one in rows)
					recorder.Lines.Add("row " + one);

				return recorder.Lines.ToArray();
			}
			""";

		var assembly = Traced(Snapshot("Minimal"), members);
		var lines    = (string[])Invoke(assembly, "SheetFromReader", "ab;1;cd;")!;

		output.WriteLine(string.Join(Environment.NewLine, lines));

		Assert.Contains(lines, static line => line.StartsWith("begin recording Row engine at 0", StringComparison.Ordinal));
		Assert.Contains(lines, static line => line.StartsWith("begin recording Row engine at 3", StringComparison.Ordinal));
		Assert.Contains("row ab", lines);
		Assert.Contains("row cd", lines);
		Balanced(lines);
	}

	/// <summary>A repetition marked <c>recover</c> on the engine tells each element it steps over, as methods do.</summary>
	[Fact]
	public void The_engine_tells_an_element_it_steps_over()
	{
		var assembly = Traced(Snapshot("Minimal"));
		var lines    = Probe<string[]>(assembly, "Recorded", () => Read(assembly, "TryParseSheet", "ab;1;cd;"));

		output.WriteLine(string.Join(Environment.NewLine, lines));

		Assert.Contains(lines, static line => line.StartsWith("begin recording Sheet engine", StringComparison.Ordinal) || line.StartsWith("begin quiet Sheet engine", StringComparison.Ordinal));
		Assert.Contains(lines, static line => line.StartsWith("recovered Sheet 3 5 ", StringComparison.Ordinal));
		Balanced(lines);
	}

	/// <summary>The forms read from a position and inside a window report in offsets of the whole input.</summary>
	[Fact]
	public void A_reading_from_a_position_reports_in_offsets_of_the_input()
	{
		var assembly = Traced(Twice);
		var input    = "xx (2+";
		var lines    = Probe<string[]>(assembly, "Recorded", () => Invoke(assembly, "TryParseSum", input, 3));
		var windowed = Probe<string[]>(assembly, "Recorded", () => Invoke(assembly, "TryParseSum", input, 3, 3));

		output.WriteLine(string.Join(Environment.NewLine, lines));

		Assert.Contains("begin recording Sum methods at 3", lines);
		Assert.Contains(lines, static line => line.StartsWith("refused 6 ", StringComparison.Ordinal));
		Assert.Contains("begin recording Sum methods at 3", windowed);
		Balanced(lines);
		Balanced(windowed);
	}

	/// <summary>The immediate carrier is traced as the tape is: the same rules, the same answer.</summary>
	[Fact]
	public void The_immediate_carrier_is_traced_as_the_tape_is()
	{
		var tape      = Traced(Twice, carrier: CarrierKind.Tape);
		var immediate = Traced(Twice, carrier: CarrierKind.Immediate);
		var input     = "1+(2^1+";

		Assert.Equal(
			Probe<string>(tape, "Explained", () => Read(tape, "TryParseSum", input)),
			Probe<string>(immediate, "Explained", () => Read(immediate, "TryParseSum", input)));

		Balanced(Probe<string[]>(immediate, "Recorded", () => Read(immediate, "TryParseSum", input)));
	}

	/// <summary>
	/// A parse begun inside another — by a guard, here — is bracketed by its own beginning and end,
	/// and GramWhy explains the outermost.
	/// </summary>
	[Fact]
	public void A_nested_reading_is_bracketed_and_the_outermost_explained()
	{
		const string grammar = """
			Item = t: ['a'..'z']+ & when @(Inner(t)) & ';'
			Word = ['a'..'z']+ & 'x'
			parse Item
			parse Word
			""";

		const string inner = """
			static bool Inner(string text)
			{
				return TryParseWord(text).IsSuccess;
			}
			""";

		var assembly = Traced(grammar, inner);
		var lines    = Probe<string[]>(assembly, "Recorded", () => Read(assembly, "TryParseItem", "abx"));
		var why      = Probe<string>(assembly, "Explained", () => Read(assembly, "TryParseItem", "abx"));
		var match    = Read(assembly, "TryParseItem", "abx");

		output.WriteLine(string.Join(Environment.NewLine, lines));
		output.WriteLine(why);

		Assert.Contains(lines, static line => line.StartsWith("begin quiet Word", StringComparison.Ordinal));
		Assert.Equal("begin quiet Item methods at 0", lines[0]);
		Assert.StartsWith(match.Error!, why, StringComparison.Ordinal);
		Balanced(lines);
	}

	/// <summary>The scope reaches a reading in a task begun inside it, and nothing once it is disposed.</summary>
	[Fact]
	public void The_sink_flows_into_a_task_and_is_gone_after_the_scope()
	{
		const string members = """
			public static string[] After(string input)
			{
				var recorder = new Recorder();

				using (Tracing(recorder))
				{
				}

				TryParseSum(input);

				return recorder.Lines.ToArray();
			}
			""";

		var assembly = Traced(Twice, members);
		var inTask   = Probe<string[]>(assembly, "Recorded", () => System.Threading.Tasks.Task.Run(() => Read(assembly, "TryParseSum", "1+")).Result);
		var after    = (string[])Invoke(assembly, "After", "1+")!;

		Assert.Contains(inTask, static line => line.StartsWith("enter Sum", StringComparison.Ordinal));
		Assert.Empty(after);
	}

	/// <summary>
	/// A <c>find</c> takes the sink set where it is called, though its body runs when it is
	/// enumerated; the starts it refuses are summed up in the log.
	/// </summary>
	[Fact]
	public void A_find_takes_the_sink_where_it_is_called()
	{
		const string grammar = """
			Item = ['0'..'9']+ & '!'
			find Item as Items
			""";

		const string members = """
			public static string[] FoundLater(string input)
			{
				var recorder = new Recorder();
				global::System.Collections.IEnumerable found;

				using (Tracing(recorder))
					found = Items(input);

				foreach (var one in found)
				{
				}

				return recorder.Lines.ToArray();
			}

			public static string LoggedFind(string input)
			{
				var writer = new global::System.IO.StringWriter();

				using (Tracing(new GramTraceLog(writer)))
					foreach (var one in Items(input))
					{
					}

				return writer.ToString();
			}
			""";

		var assembly = Traced(grammar, members);
		var lines    = (string[])Invoke(assembly, "FoundLater", "ab 12! c 3!")!;
		var log      = (string)Invoke(assembly, "LoggedFind", "ab 12! c 3!")!;

		output.WriteLine(log);

		Assert.Contains(lines, static line => line.StartsWith("begin quiet Item engine find at 3", StringComparison.Ordinal));
		Assert.Contains("3 starts refused from 0 to 2", log, StringComparison.Ordinal);
		Balanced(lines);
	}

	/// <summary>A <c>yield</c> takes the sink where it is called, and a bad element is said before it is thrown.</summary>
	[Fact]
	public void A_yield_takes_the_sink_where_it_is_called()
	{
		const string grammar = """
			Item : @int = value: ['a'..'z']+ & ';' => @(value.Length)
			Feed : @int[] = Item*
			parse Feed as Lazy yield : @int
			""";

		const string members = """
			public static string[] YieldedLater(string input)
			{
				var recorder = new Recorder();
				global::System.Collections.Generic.IEnumerable<int> found;

				using (Tracing(recorder))
					found = Lazy(input);

				try
				{
					foreach (var one in found)
					{
					}
				}
				catch (global::System.FormatException)
				{
				}

				return recorder.Lines.ToArray();
			}
			""";

		var assembly = Traced(grammar, members);
		var lines    = (string[])Invoke(assembly, "YieldedLater", "ab;c;1")!;

		output.WriteLine(string.Join(Environment.NewLine, lines));

		Assert.Contains(lines, static line => line.StartsWith("begin recording Item", StringComparison.Ordinal));
		Assert.Contains(lines, static line => line.StartsWith("rejected ", StringComparison.Ordinal) && line.Contains("Invalid element at offset", StringComparison.Ordinal));
		Balanced(lines);
	}

	/// <summary>A repetition marked <c>recover</c> is explained element by element.</summary>
	[Fact]
	public void A_recovered_element_is_explained_on_its_own()
	{
		const string grammar = """
			Field  : @string = t: ['a'..'z']+ & '=' & v: ['0'..'9']+ & ';' => @(t + v)
			Fields : @string = fields: Field* recover ';' => @(parserText) & eof => @(string.Join(",", fields))
			parse Fields
			""";

		var assembly = Traced(grammar);
		var lines    = Probe<string[]>(assembly, "Recorded", () => Read(assembly, "TryParseFields", "a=1;bad;c=3;"));
		var why      = Probe<string>(assembly, "Explained", () => Read(assembly, "TryParseFields", "a=1;bad;c=3;"));

		output.WriteLine(string.Join(Environment.NewLine, lines));
		output.WriteLine(why);

		Assert.Contains(lines, static line => line.StartsWith("recovered Fields 4 8 7", StringComparison.Ordinal));
		Assert.Contains("Stepped over an element of Fields from 4 to 8:", why, StringComparison.Ordinal);
		Balanced(lines);
	}

	/// <summary>
	/// A reading an exception leaves ends for the sink all the same: the next call is explained as
	/// a call of its own, not as one nested in a reading that never ended.
	/// </summary>
	[Fact]
	public void A_reading_an_exception_leaves_still_ends()
	{
		const string grammar = """
			Start : @int = t: ['0'..'9']+ => @(Checked(t))
			parse Start
			""";

		const string members = """
			static int Checked(string text)
			{
				if (text == "0")
					throw new global::System.InvalidOperationException("No zero.");

				return text.Length;
			}
			""";

		var assembly = Traced(grammar, members);
		var match    = Read(assembly, "TryParseStart", "x");
		var lines    = Probe<string[]>(assembly, "Recorded", () =>
		{
			Assert.Throws<TargetInvocationException>(() => Read(assembly, "TryParseStart", "0"));

			return Read(assembly, "TryParseStart", "x");
		});
		var why = Probe<string>(assembly, "Explained", () =>
		{
			Assert.Throws<TargetInvocationException>(() => Read(assembly, "TryParseStart", "0"));

			return Read(assembly, "TryParseStart", "x");
		});

		output.WriteLine(string.Join(Environment.NewLine, lines));

		Assert.Equal(lines.Count(static line => line.StartsWith("begin ", StringComparison.Ordinal)), lines.Count(static line => line.StartsWith("end ", StringComparison.Ordinal)));
		Assert.StartsWith(match.Error!, why, StringComparison.Ordinal);
	}

	/// <summary>
	/// An exception leaves every rule it was thrown inside, for the sink: each rule entered and
	/// each frame of a forwarding rule is left, innermost first, as thrown, and the reading ends.
	/// A guard that throws is asked while the rules are open on every carrier; a construction
	/// that throws is, on the immediate carrier, and the tape builds once the reading is done.
	/// </summary>
	[Theory]
	[InlineData(CarrierKind.Tape, "[7!]")]
	[InlineData(CarrierKind.Tape, "[8!]")]
	[InlineData(CarrierKind.Immediate, "[7!]")]
	[InlineData(CarrierKind.Immediate, "[8!]")]
	public void An_exception_leaves_every_rule_it_was_thrown_inside(CarrierKind carrier, string input)
	{
		const string grammar = """
			Top   : @int = '[' & v: Inner & ']' => @(v)
			Inner : @int = v: Leaf => @(v)
			Leaf  : @int = d: ['0'..'9']+ & when @(Asked(d)) & '!' => @(Built(d))
			parse Top
			""";

		const string members = """
			static bool Asked(string digits)
			{
				if (digits == "7")
					throw new global::System.InvalidOperationException("Asked about seven.");

				return true;
			}

			static int Built(string digits)
			{
				if (digits == "8")
					throw new global::System.InvalidOperationException("Built eight.");

				return digits.Length;
			}
			""";

		var assembly = Traced(grammar, members, carrier: carrier);
		var lines    = Probe<string[]>(assembly, "Recorded", () =>
		{
			Assert.Throws<TargetInvocationException>(() => Read(assembly, "TryParseTop", input));

			return null;
		});

		output.WriteLine(string.Join(Environment.NewLine, lines));

		Balanced(lines);
		Assert.Equal(
			lines.Count(static line => line.StartsWith("begin ", StringComparison.Ordinal)),
			lines.Count(static line => line.StartsWith("end ", StringComparison.Ordinal)));

		// Where the guard throws, the rules and the frame around it are still open, and are left as thrown.
		if (input == "[7!]" || carrier == CarrierKind.Immediate)
		{
			Assert.Contains("exit Leaf 1 -2", lines);
			Assert.Contains("exit Inner 1 -2", lines);
			Assert.Contains("exit Top 0 -2", lines);
		}
	}

	/// <summary>
	/// A sink is the host's own code: what it throws goes out of the call, as what a guard throws
	/// does, and the sink is still told that every rule it was told of is left and the reading ends.
	/// </summary>
	[Fact]
	public void What_a_sink_throws_leaves_the_call_and_the_sink_is_told_the_reading_ended()
	{
		const string members = """
			public sealed class Throwing : GramTrace
			{
				public int Enters, Exits, Begins, Ends;
				int _left = 3;

				public override void Begin(GramRead read)
				{
					Begins++;
				}

				public override void End(GramRead read, int end, int position, string[]? expected, global::System.Collections.Generic.IReadOnlyList<string[]>? expectedMore)
				{
					Ends++;
				}

				public override void Enter(int rule, int position)
				{
					Enters++;

					if (--_left == 0)
						throw new global::System.InvalidOperationException("The sink threw.");
				}

				public override void Exit(int rule, int position, int end)
				{
					Exits++;
				}
			}

			public static string Thrower(string input)
			{
				var sink = new Throwing();

				try
				{
					using (Tracing(sink))
						TryParseSum(input);

					return "nothing thrown";
				}
				catch (global::System.InvalidOperationException caught)
				{
					return caught.Message + " " + sink.Enters + "/" + sink.Exits + " " + sink.Begins + "/" + sink.Ends;
				}
			}
			""";

		var assembly = Traced(Twice, members);

		Assert.Equal("The sink threw. 3/3 1/1", Invoke(assembly, "Thrower", "1+2"));
	}

	/// <summary>
	/// A sink whose <c>Begin</c> throws is told the reading ended all the same, and what it threw
	/// leaves the call: for the outermost reading, and for one a guard begins inside another,
	/// where the reading outside is left as well.
	/// </summary>
	[Theory]
	[InlineData(1, "The sink threw. 1/1 0/0")]
	[InlineData(2, "The sink threw. 2/2 1/1")]
	public void A_sink_whose_beginning_throws_is_told_the_reading_ended(int throwAt, string expected)
	{
		const string grammar = """
			Item = t: ['a'..'z']+ & when @(Inner(t)) & ';'
			Word = ['a'..'z']+ & 'x'
			parse Item
			parse Word
			""";

		const string members = """
			static bool Inner(string text)
			{
				return TryParseWord(text).IsSuccess;
			}

			public sealed class Throwing : GramTrace
			{
				public int Enters, Exits, Begins, Ends;
				readonly int _throwAt;

				public Throwing(int throwAt)
				{
					_throwAt = throwAt;
				}

				public override void Begin(GramRead read)
				{
					if (++Begins == _throwAt)
						throw new global::System.InvalidOperationException("The sink threw.");
				}

				public override void End(GramRead read, int end, int position, string[]? expected, global::System.Collections.Generic.IReadOnlyList<string[]>? expectedMore)
				{
					Ends++;
				}

				public override void Enter(int rule, int position)
				{
					Enters++;
				}

				public override void Exit(int rule, int position, int end)
				{
					Exits++;
				}
			}

			public static string BeginThrows(string input, int throwAt)
			{
				var sink = new Throwing(throwAt);

				try
				{
					using (Tracing(sink))
						TryParseItem(input);

					return "nothing thrown";
				}
				catch (global::System.InvalidOperationException caught)
				{
					return caught.Message + " " + sink.Begins + "/" + sink.Ends + " " + sink.Enters + "/" + sink.Exits;
				}
			}
			""";

		var assembly = Traced(grammar, members);

		Assert.Equal(expected, Invoke(assembly, "BeginThrows", "ax;", throwAt));
	}

	/// <summary>
	/// The ready-made sinks keep their books whatever they are told: an exit with no entry, an end
	/// with no beginning, a number no rule has throw nothing, and the next call is read as its own.
	/// </summary>
	[Fact]
	public void The_ready_made_sinks_are_not_thrown_by_what_they_are_told()
	{
		const string members = """
			public static string Odd(string input)
			{
				var why     = new GramWhy();
				var profile = new GramProfile();
				var writer  = new global::System.IO.StringWriter();
				var log     = new GramTraceLog(writer);

				foreach (var sink in new GramTrace[] { why, profile, log })
				{
					sink.Exit(0, 0, 1);
					sink.Exit(-1, 0, -2);
					sink.End(null!, -1, 0, null, null);
					sink.Enter(-1, 0);
					sink.Enter(100000, 0);
					sink.Exit(100000, 0, GramTrace.Thrown);
					sink.Remembered(-7, 0);
					sink.Recovered(-7, 0, 1, 1);
					sink.Guard(-3, 0, false);
					sink.Refused(-1, null);
					sink.Deepened(-1);
					sink.Rejected(-1, "");
					sink.End(null!, 5, 0, null, null);
				}

				using (Tracing(why))
					TryParseSum(input);

				profile.ToString();

				return why.ToString();
			}
			""";

		var assembly = Traced(Twice, members);
		var match    = Read(assembly, "TryParseSum", "1+(2+");
		var why      = (string)Invoke(assembly, "Odd", "1+(2+")!;

		output.WriteLine(why);

		Assert.StartsWith(match.Error + " (line 1, column 6)", why, StringComparison.Ordinal);
		Assert.Contains("in Sum 1:1 > More 1:2 > Term 1:3 > Sum 1:4 > More 1:5 > Term 1:6", why, StringComparison.Ordinal);
	}

	/// <summary>
	/// A <c>find</c> enumerated under another sink than the one set where it was called reports
	/// every reading it begins to the first — a parse a guard starts as well — and leaves the
	/// second set where it is enumerated.
	/// </summary>
	[Fact]
	public void A_find_enumerated_elsewhere_reports_nested_readings_to_its_own_sink()
	{
		const string grammar = """
			Item = t: ['a'..'z']+ & when @(Inner(t)) & ';'
			Word = ['a'..'z']+ & 'x'
			find Item as Items
			parse Word
			""";

		const string members = """
			static bool Inner(string text)
			{
				return TryParseWord(text).IsSuccess;
			}

			public static string[] Elsewhere(string input)
			{
				var called     = new Recorder();
				var enumerated = new Recorder();
				global::System.Collections.IEnumerable found;

				using (Tracing(called))
					found = Items(input);

				using (Tracing(enumerated))
				{
					foreach (var one in found)
					{
					}

					TryParseWord("ax");
				}

				return new[] { string.Join("|", called.Lines), string.Join("|", enumerated.Lines) };
			}
			""";

		var assembly = Traced(grammar, members);
		var lines    = (string[])Invoke(assembly, "Elsewhere", "ax; bx; c;")!;

		output.WriteLine(lines[0]);
		output.WriteLine(lines[1]);

		Assert.Contains("begin quiet Word", lines[0], StringComparison.Ordinal);
		Assert.Contains("begin quiet Item", lines[0], StringComparison.Ordinal);

		// What the scope it was enumerated in saw is the parse made there after the loop, and only that.
		Assert.StartsWith("begin quiet Word", lines[1], StringComparison.Ordinal);
		Assert.Single(lines[1].Split('|'), static line => line.StartsWith("begin ", StringComparison.Ordinal));
	}

	/// <summary>A <c>yield</c> does the same as a <c>find</c>: its nested readings go to the sink set where it was called.</summary>
	[Fact]
	public void A_yield_enumerated_elsewhere_reports_nested_readings_to_its_own_sink()
	{
		const string grammar = """
			Item : @int = value: ['a'..'z']+ & when @(Inner(value)) & ';' => @(value.Length)
			Feed : @int[] = Item*
			Word = ['a'..'z']+ & 'x'
			parse Feed as Lazy yield : @int
			parse Word
			""";

		const string members = """
			static bool Inner(string text)
			{
				return TryParseWord(text).IsSuccess;
			}

			public static string[] Elsewhere(string input)
			{
				var called     = new Recorder();
				var enumerated = new Recorder();
				global::System.Collections.Generic.IEnumerable<int> found;

				using (Tracing(called))
					found = Lazy(input);

				using (Tracing(enumerated))
				{
					foreach (var one in found)
					{
					}

					TryParseWord("ax");
				}

				return new[] { string.Join("|", called.Lines), string.Join("|", enumerated.Lines) };
			}
			""";

		var assembly = Traced(grammar, members);
		var lines    = (string[])Invoke(assembly, "Elsewhere", "ax;bx;")!;

		output.WriteLine(lines[0]);
		output.WriteLine(lines[1]);

		Assert.Contains("begin quiet Word", lines[0], StringComparison.Ordinal);
		Assert.Contains("begin recording Item", lines[0], StringComparison.Ordinal);
		Assert.StartsWith("begin quiet Word", lines[1], StringComparison.Ordinal);
		Assert.Single(lines[1].Split('|'), static line => line.StartsWith("begin ", StringComparison.Ordinal));
	}

	/// <summary>
	/// A chain of forwarding rules is framed whole whatever order its rules are declared in: the
	/// paths are those of a parser that calls every one of them as written.
	/// </summary>
	[Theory]
	[InlineData("A", "B", "C")]
	[InlineData("A", "C", "B")]
	[InlineData("B", "A", "C")]
	[InlineData("B", "C", "A")]
	[InlineData("C", "A", "B")]
	[InlineData("C", "B", "A")]
	public void A_chain_of_forwarding_rules_is_framed_whole_in_any_order(string first, string second, string third)
	{
		var rules = new Dictionary<string, string>
		{
			["A"] = "A    : @int = v: B => @(v)",
			["B"] = "B    : @int = v: C => @(v)",
			["C"] = "C    : @int = v: Leaf => @(v) | w: Name => @(w)",
		};

		var grammar = string.Join("\n",
			"Top  : @int = '[' & v: A & ']' => @(v)",
			rules[first],
			rules[second],
			rules[third],
			"Leaf : @int = d: ['0'..'9']+ & '!' => @(d.Length) | '(' & v: A & ')' => @(v)",
			"Name : @int = n: ['a'..'z']+ & '?' => @(n.Length)",
			"parse Top");

		var collapsed = Traced(grammar);
		var written   = TracedAsWritten(grammar);

		foreach (var input in new[] { "[1", "[a", "[", "[1!", "[(1", "[(a?" })
		{
			var said   = Probe<string>(collapsed, "Explained", () => Read(collapsed, "TryParseTop", input));
			var wanted = Probe<string>(written, "Explained", () => Read(written, "TryParseTop", input));

			output.WriteLine(said);

			Assert.Equal(wanted, said);
			Balanced(Probe<string[]>(collapsed, "Recorded", () => Read(collapsed, "TryParseTop", input)));
		}

		Assert.Contains(
			"Top 1:1 > A 1:2 > B 1:2 > C 1:2 > Leaf 1:2 > A 1:3 > B 1:3 > C 1:3 > Leaf 1:3",
			Probe<string>(collapsed, "Explained", () => Read(collapsed, "TryParseTop", "[(1")),
			StringComparison.Ordinal);
	}

	/// <summary>A grammar compiled with its forwarding rules called as written: the reference for the frames a trace reports.</summary>
	static Assembly TracedAsWritten(string grammar)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName          = "Grammar",
			CSharpScanner      = RoslynCSharpScanner.Instance,
			Trace              = true,
			CollapseForwarders = false,
		});

		EmittedCode.Quiet(result.Diagnostics);

		return EmittedCode.Compile(result.Sources[0].Text, "Grammar", declarationMembers: Nullable + Probes);
	}

	/// <summary>The log is cut at its budget, and says how much it did not write.</summary>
	[Fact]
	public void The_log_keeps_to_its_budget()
	{
		const string members = """
			public static string Budgeted(string input)
			{
				var writer = new global::System.IO.StringWriter();

				using (Tracing(new GramTraceLog(writer, 5)))
					TryParseSum(input);

				return writer.ToString();
			}
			""";

		var assembly = Traced(Twice, members);
		var log      = (string)Invoke(assembly, "Budgeted", "1+2+3+(4")!;

		var lines = log.Split('\n', StringSplitOptions.RemoveEmptyEntries);

		output.WriteLine(log);

		Assert.Equal(5, lines.Count(static line => !line.StartsWith("...", StringComparison.Ordinal)));
		Assert.Contains("more lines, past the budget of 5", log, StringComparison.Ordinal);
	}

	static string Snapshot(string name)
	{
		return System.IO.File.ReadAllText(System.IO.Path.Combine(SnapshotDirectory(), name + ".gram"));
	}

	static string SnapshotDirectory([System.Runtime.CompilerServices.CallerFilePath] string here = "")
	{
		return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(here)!, "..", "Snapshots");
	}

	/// <summary>
	/// Every rule entered is left, innermost first, and every rule retracted is one that read
	/// something, left and not retracted since.
	/// </summary>
	static void Balanced(IEnumerable<string> lines)
	{
		var open = new Stack<string>();
		var read = new List<string>();

		foreach (var line in lines)
		{
			var words = line.Split(' ');

			if (words[0] == "enter")
				open.Push(words[1] + " " + words[2]);
			else if (words[0] == "exit")
			{
				var frame = words[1] + " " + words[2];

				Assert.Equal(open.Pop(), frame);

				if (int.Parse(words[3], System.Globalization.CultureInfo.InvariantCulture) >= 0)
					read.Add(frame);
			}
			else if (words[0] == "retracted")
			{
				var frame = words[1] + " " + words[2];
				var at    = read.LastIndexOf(frame);

				Assert.True(at >= 0, $"{frame} retracted without having read anything");
				read.RemoveAt(at);
			}
		}

		Assert.Empty(open);
	}
}
