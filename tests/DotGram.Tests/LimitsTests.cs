using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Emit;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The limits a caller sets on a grammar class while the program runs (docs/untrusted-input.md): what they start
/// as, what setting them changes, what a call's own argument still overrides, and that a parse running while they
/// are set keeps the ones it began with.
/// </summary>
public sealed class LimitsTests
{
	/// <summary>
	/// Where the grammar says nothing, the limits are none: no bound on what is buffered, nor on stacks.
	/// </summary>
	[Fact]
	public void Without_the_options_the_limits_are_none()
	{
		var host = Compiled(new GramCompilerOptions { BufferedInput = true });

		Assert.Equal(int.MaxValue, Get(host, "MaxBufferedInputLength"));
		Assert.Equal(0,            Get(host, "MaxExtraStacks"));
		Assert.True((bool)Call(host, "IsDefault")!);
	}

	/// <summary>
	/// The limits start as what the grammar was generated with.
	/// </summary>
	[Fact]
	public void The_limits_start_as_the_generated_defaults()
	{
		var host = Compiled(new GramCompilerOptions { BufferedInput = true, MaxRetained = 16, Stacks = 3 });

		Assert.Equal(16, Get(host, "MaxBufferedInputLength"));
		Assert.Equal(3,  Get(host, "MaxExtraStacks"));
		Assert.True((bool)Call(host, "IsDefault")!);
	}

	/// <summary>
	/// The attribute writes them, as it writes the rest of a reading's options.
	/// </summary>
	[Fact]
	public void The_attribute_sets_the_defaults_of_the_limits()
	{
		var run = GeneratorDriverTests.RunGenerator(
			"[DotGram.Gram(\"Row = 'a'\\nparse Row\", BufferedText = true, MaxBufferedInputLength = 16, MaxExtraStacks = 3)]\npublic partial class Host;");
		var source = string.Join("\n", run.Results.SelectMany(one => one.GeneratedSources).Select(one => one.SourceText.ToString()));

		Assert.Contains("public static ParserLimits Default { get; } = new ParserLimits(16, 3);", source, StringComparison.Ordinal);
	}

	/// <summary>
	/// The class a reading is compiled into declares the limits, so it can be called neither of their names: said
	/// against the grammar, rather than as an error in code the author did not write.
	/// </summary>
	[Theory]
	[InlineData("Limits", null)]
	[InlineData("ParserLimits", null)]
	[InlineData("OnReadingLimits", null)]
	[InlineData("Host", "Limits")]
	[InlineData("Host", "ParserLimits")]
	public void A_class_named_like_the_limits_is_refused(string className, string? reading)
	{
		var result = GramCompiler.Compile("R = 'a'\nparse R", new GramCompilerOptions
		{
			ClassName     = className,
			Suffix        = reading,
			CSharpScanner = RoslynCSharpScanner.Instance,
		});

		Assert.Contains(result.Diagnostics, static one => one.Id == CSharpEmitter.GeneratedNameClash);
		Assert.Empty(result.Sources);
	}

	/// <summary>
	/// Nor may it declare them itself, which the generator asks of the class as the author wrote it; the hook the
	/// limits ask is the author's to implement.
	/// </summary>
	[Theory]
	[InlineData("public static int Limits;", true)]
	[InlineData("public sealed class ParserLimits { }", true)]
	[InlineData("static partial void OnReadingLimits(ref ParserLimits limits) { }", false)]
	public void A_class_declaring_the_limits_itself_is_refused(string member, bool refused)
	{
		var run = GeneratorDriverTests.RunGenerator(
			"[DotGram.Gram(\"R = 'a'\\nparse R\")]\npublic static partial class Host\n{\n\t" + member + "\n}");
		var clash = run.Diagnostics.Any(static one => one.Id == CSharpEmitter.GeneratedNameClash);

		Assert.Equal(refused, clash);
	}

	/// <summary>
	/// A value that is no limit at all, or none to hold, is refused where it is made.
	/// </summary>
	[Fact]
	public void A_limit_out_of_range_is_refused()
	{
		var host = Compiled(new GramCompilerOptions { BufferedInput = true });

		Assert.IsType<ArgumentOutOfRangeException>(Thrown(() => Call(host, "SetRetained", 0)));
		Assert.IsType<ArgumentOutOfRangeException>(Thrown(() => Call(host, "SetStacks", -1)));
		Assert.IsType<ArgumentNullException>(Thrown(() => Call(host, "SetNull")));

		// And nothing refused was set.
		Assert.True((bool)Call(host, "IsDefault")!);
	}

	/// <summary>
	/// A buffered parse holds no more than the limits say where the call passes nothing, and what the call passes
	/// wins over them either way.
	/// </summary>
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void A_calls_own_maxRetained_wins_over_the_limits(bool bytes)
	{
		var host   = Compiled(new GramCompilerOptions { BufferedInput = true, MaxRetained = 4 });
		var method = host.GetMethod("All", [bytes ? typeof(Stream) : typeof(TextReader), typeof(int?), typeof(int?)])!;

		object Input()
		{
			return bytes
				? new MemoryStream(System.Text.Encoding.ASCII.GetBytes("abcdefghij;"))
				: new StringReader("abcdefghij;");
		}

		object? Read(int? maxRetained)
		{
			return method.Invoke(null, [Input(), null, maxRetained]);
		}

		// The generated default, four, which a record of ten exceeds.
		Assert.Contains("more than 4 retained", Assert.IsType<IOException>(Thrown(() => Read(null))).Message, StringComparison.Ordinal);

		// Raised by the limits, and the call that says nothing reads it.
		Call(host, "SetRetained", 64);
		Assert.Equal(new[] { 10 }, (int[])Read(null)!);

		// Lowered by the call, which wins.
		Assert.Contains("more than 5 retained", Assert.IsType<IOException>(Thrown(() => Read(5))).Message, StringComparison.Ordinal);

		// Lowered by the limits and raised by the call, which wins again.
		Call(host, "SetRetained", 3);
		Assert.Equal(new[] { 10 }, (int[])Read(64)!);
		Assert.Contains("more than 3 retained", Assert.IsType<IOException>(Thrown(() => Read(null))).Message, StringComparison.Ordinal);
	}

	/// <summary>
	/// A nest deeper than a stack takes as many extra stacks as it needs where nothing limits them, and fails once the
	/// limits allow fewer; changing them back lets it through again.
	/// </summary>
	[Fact]
	public void The_limits_bound_the_stacks_a_parse_takes()
	{
		var host  = Deep();
		var taken = Handed(host, Nest);

		// Deep enough to need two at least, so that a limit of one below it is still a limit.
		Assert.True(taken >= 2, $"the nest took {taken} extra stacks");
		Assert.Equal(Depth, Run(host, Nest));

		Call(host, "SetStacks", taken - 1);
		Assert.Equal("deep", Run(host, Nest));

		Call(host, "SetStacks", taken);
		Assert.Equal(Depth, Run(host, Nest));

		Call(host, "SetStacks", 0);
		Assert.Equal(Depth, Run(host, Nest));
	}

	/// <summary>
	/// A parse reads the limits once, as it begins: set while it runs — here by the parse itself, as it hands its
	/// reading to another stack — they leave it alone and hold the next one.
	/// </summary>
	[Fact]
	public void A_parse_keeps_the_limits_it_began_with()
	{
		var host  = Deep();
		var taken = Handed(host, Nest);

		Assert.True(taken >= 2, $"the nest took {taken} extra stacks");

		// Unlimited as it begins, limited to one stack from its first hand-off on.
		Call(host, "SwapOnHandOff");
		Assert.Equal(Depth, Run(host, Nest));
		Assert.Equal(1, Get(host, "MaxExtraStacks"));

		// The parse begun after the swap reads it.
		Assert.Equal("deep", Run(host, Nest));
	}

	/// <summary>
	/// A refused input is read twice, quietly and then for its message, and both readings are one parse: limits
	/// set during the first leave the second alone, and the call answers with the refusal rather than with the
	/// limits it did not begin with.
	/// </summary>
	[Fact]
	public void The_second_reading_of_a_refused_input_keeps_the_limits_the_parse_began_with()
	{
		var host  = Deep();
		var taken = Handed(host, Nest);
		var open  = new string('(', Depth) + "a";

		// Read twice where refused, the second time handed what the first began with.
		Assert.Contains("Limits = failure.Limits", DeepSource(), StringComparison.Ordinal);
		Assert.True(taken >= 2, $"the nest took {taken} extra stacks");
		Assert.Equal("Starved", Run(host, open, "Try"));

		// Unlimited as it begins, limited to one stack from the first reading's first hand-off on.
		Call(host, "SwapOnHandOff");
		Assert.Equal("Starved", Run(host, open, "Try"));
		Assert.Equal(1, Get(host, "MaxExtraStacks"));

		// The call begun after the swap reads it.
		Assert.Equal("deep", Run(host, open, "Try"));
	}

	/// <summary>
	/// The author may answer the limits from a setting of their own: what the hook says is what the class's
	/// <c>Limits</c> reads and what a parse is held to.
	/// </summary>
	[Fact]
	public void The_limits_may_be_answered_by_the_author()
	{
		var host = Deep(Answered);

		Assert.Equal(Depth, Run(host, Nest));

		Call(host, "Answer", 1);
		Assert.Equal(1, Get(host, "MaxExtraStacks"));
		Assert.Equal("deep", Run(host, Nest));

		// Setting the class's own changes nothing while the hook answers.
		Call(host, "SetStacks", 0);
		Assert.Equal(1, Get(host, "MaxExtraStacks"));
		Assert.Equal("deep", Run(host, Nest));
	}

	/// <summary>
	/// Limits set over and over from one thread while others parse: every parse ends as one of the two settings
	/// says, and none fails any other way.
	/// </summary>
	[Fact]
	public void Limits_set_while_others_parse_leave_each_parse_whole()
	{
		var host  = Deep();
		var taken = Handed(host, Nest);
		var stop  = 0;
		var odd   = new System.Collections.Concurrent.ConcurrentBag<object?>();
		var seen  = new System.Collections.Concurrent.ConcurrentDictionary<object, int>();

		var setter = new Thread(() =>
		{
			while (Volatile.Read(ref stop) == 0)
			{
				Call(host, "SetStacks", 1);
				Call(host, "SetStacks", 0);
			}
		});

		var parsers = Enumerable.Range(0, 4).Select(_ => new Thread(() =>
		{
			for (var turn = 0; turn < 8; turn++)
			{
				object? answer;

				try
				{
					answer = Call(host, "Run", Nest);
				}
				catch (Exception thrown)
				{
					answer = thrown;
				}

				if (answer is int or "deep")
					seen.AddOrUpdate(answer, 1, static (_, count) => count + 1);
				else
					odd.Add(answer);
			}
		}, 256 * 1024)).ToArray();

		setter.Start();

		foreach (var parser in parsers)
			parser.Start();

		foreach (var parser in parsers)
			parser.Join();

		Volatile.Write(ref stop, 1);
		setter.Join();

		Assert.True(taken >= 2, $"the nest took {taken} extra stacks");
		Assert.Empty(odd);
		Assert.Equal(32, seen.Values.Sum());

		Call(host, "SetStacks", 0);
		Assert.Equal(Depth, Run(host, Nest));
	}

	/// <summary>
	/// How deep <see cref="Nest"/> is.
	/// </summary>
	const int Depth = 400_000;

	/// <summary>
	/// Brackets around a name, deeper than one extra stack holds.
	/// </summary>
	static readonly string Nest = new string('(', Depth) + "a" + new string(')', Depth);

	/// <summary>
	/// The members the tests call the generated class through, written beside it as an author would.
	/// </summary>
	const string Members =
		"\n#nullable enable\n" +
		"public static void SetStacks(int most) { Limits = Limits.WithMaxExtraStacks(most); }\n" +
		"public static void SetRetained(int most) { Limits = Limits.WithMaxBufferedInputLength(most); }\n" +
		"public static void SetNull() { Limits = null!; }\n" +
		"public static bool IsDefault() { return Limits == ParserLimits.Default; }\n" +
		"public static int MaxExtraStacks() { return Limits.MaxExtraStacks; }\n" +
		"public static int MaxBufferedInputLength() { return Limits.MaxBufferedInputLength; }\n";

	/// <summary>
	/// The hand-off hook, which counts the stacks taken and swaps the limits where a test asks.
	/// </summary>
	const string Hook =
		"public static int Handed;\n" +
		"static bool swap;\n" +
		"public static void SwapOnHandOff() { Limits = ParserLimits.Default; swap = true; }\n" +
		"static partial void OnHandingOff(ref object? state)\n" +
		"{\n" +
		"\tglobal::System.Threading.Interlocked.Increment(ref Handed);\n" +
		"\tif (swap) { swap = false; Limits = Limits.WithMaxExtraStacks(1); }\n" +
		"}\n" +
		"public static object Try(string text)\n" +
		"{\n" +
		"\ttry { return TryParseStart(text).Outcome.ToString(); }\n" +
		"\tcatch (global::System.InsufficientExecutionStackException) { return \"deep\"; }\n" +
		"}\n" +
		"public static object Run(string text)\n" +
		"{\n" +
		"\ttry { return ParseStart(text); }\n" +
		"\tcatch (global::System.InsufficientExecutionStackException) { return \"deep\"; }\n" +
		"}\n";

	/// <summary>
	/// A grammar of rows, over a reader, to hold buffered input to the limits.
	/// </summary>
	static Type Compiled(GramCompilerOptions options)
	{
		options.CSharpScanner = RoslynCSharpScanner.Instance;

		var result = GramCompiler.Compile("""
			Row : @int = t: ['a'..'z']+ & ';' => @(t.Length)
			Rows : @int[] = Row*
			parse Rows as All stream bytes
			""", options);

		EmittedCode.Quiet(result.Diagnostics);

		return EmittedCode.Compile(Assert.Single(result.Sources).Text, declarationMembers: Members).GetType("Grammar")!;
	}

	/// <summary>
	/// A grammar that recurses, read by methods that take another stack where theirs runs low.
	/// </summary>
	static Type Deep(string more = "")
	{
		return EmittedCode.Compile(DeepSource(), declarationMembers: Members + Hook + more).GetType("Grammar")!;
	}

	/// <summary>
	/// What <see cref="Deep"/> compiles.
	/// </summary>
	static string DeepSource()
	{
		var grammar =
			"trivia = { ' '* }\nnamespace Lexical\n{\n\ttrivia = none\n\tName = ['a'..'z']+\n}\n" +
			"Start : @int = only: Value & eof => @(only)\n" +
			"Value : @int = '(' & v: Value & ')' => @(v + 1) | Lexical.Name => @(0)\n" +
			"parse Start";
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName     = "Grammar",
			CSharpScanner = RoslynCSharpScanner.Instance,
			Lexical       = true,
		});

		EmittedCode.Quiet(result.Diagnostics);

		return Assert.Single(result.Sources).Text;
	}

	/// <summary>
	/// The author's own setting, which the limits hook answers from once it is set.
	/// </summary>
	const string Answered =
		"static ParserLimits? answer;\n" +
		"public static void Answer(int most) { answer = ParserLimits.Default.WithMaxExtraStacks(most); }\n" +
		"static partial void OnReadingLimits(ref ParserLimits limits) { if (answer != null) limits = answer; }\n";

	/// <summary>
	/// How many extra stacks a parse of the text takes, with nothing limiting them.
	/// </summary>
	static int Handed(Type host, string text)
	{
		var field = host.GetField("Handed")!;

		Call(host, "SetStacks", 0);
		field.SetValue(null, 0);
		Assert.Equal(Depth, Run(host, text));

		return (int)field.GetValue(null)!;
	}

	/// <summary>
	/// Reads the text on a thread whose stack runs low early, so that the reading moves.
	/// </summary>
	static object? Run(Type host, string text, string method = "Run")
	{
		object? answer = null;
		Exception? thrown = null;

		var thread = new Thread(() =>
		{
			try
			{
				answer = Call(host, method, text);
			}
			catch (Exception caught)
			{
				thrown = caught;
			}
		}, 256 * 1024);

		thread.Start();
		thread.Join();

		Assert.Null(thrown);

		return answer;
	}

	static int Get(Type host, string limit)
	{
		return (int)Call(host, limit)!;
	}

	static object? Call(Type host, string method, params object?[] arguments)
	{
		try
		{
			return host.GetMethod(method)!.Invoke(null, arguments);
		}
		catch (TargetInvocationException thrown) when (thrown.InnerException is not null)
		{
			throw thrown.InnerException;
		}
	}

	static Exception Thrown(Func<object?> action)
	{
		var thrown = Record.Exception(action);

		Assert.NotNull(thrown);

		return thrown is TargetInvocationException { InnerException: { } inner } ? inner : thrown;
	}
}
