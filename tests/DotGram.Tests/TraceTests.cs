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

	static Assembly Traced(string grammar, string? members = null, bool lexical = false, CarrierKind carrier = CarrierKind.Auto)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName     = "Grammar",
			CSharpScanner = RoslynCSharpScanner.Instance,
			Lexical       = lexical,
			Carrier       = carrier,
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
	/// character no token begins with is said before any rule is read.
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
		Assert.Contains("No token of the grammar begins at that character", stopped, StringComparison.Ordinal);
		Assert.Equal("unlexed 8", lines[0]);
		Assert.DoesNotContain(lines, static line => line.StartsWith("begin ", StringComparison.Ordinal));
	}

	/// <summary>A reading on the engine reports its beginning and its end, and the explanation says it runs there.</summary>
	[Fact]
	public void A_reading_on_the_engine_says_so()
	{
		var assembly = Traced(Snapshot("Url"));
		var why      = Probe<string>(assembly, "Explained", () => Read(assembly, "TryParseUrl", "http://"));
		var lines    = Probe<string[]>(assembly, "Recorded", () => Read(assembly, "TryParseUrl", "http://"));

		output.WriteLine(why);

		Assert.Contains("runs on the engine", why, StringComparison.Ordinal);
		Assert.Contains(lines, static line => line.StartsWith("begin recording Url engine", StringComparison.Ordinal));
		Assert.DoesNotContain(lines, static line => line.StartsWith("enter ", StringComparison.Ordinal));
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

	/// <summary>Every rule entered is left: on a refusal, on an acceptance, and through every return a rule's method has.</summary>
	static void Balanced(IEnumerable<string> lines)
	{
		var open = new Stack<string>();

		foreach (var line in lines)
		{
			var words = line.Split(' ');

			if (words[0] == "enter")
				open.Push(words[1] + " " + words[2]);
			else if (words[0] == "exit")
				Assert.Equal(open.Pop(), words[1] + " " + words[2]);
		}

		Assert.Empty(open);
	}
}
