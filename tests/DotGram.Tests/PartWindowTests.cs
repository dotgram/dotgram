using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// What a streamed <c>parse</c> holds while it reads a repetition among its parts: one element of it,
/// or no reader at all, said with GRAM5001.
/// </summary>
/// <remarks>
/// <para>
/// docs/syntax.md §6.3 promises that a reader holds one record's worth for a well-formed feed, and that
/// the overload is offered only where that provably holds. The driver moves its window between the
/// elements of a repetition only where it reads them one at a time, which is a repetition of a rule.
/// A repetition of a group, <c>('t' &amp; eol)*</c>, is one machine run over the whole run, and the
/// window under it grows with every line it reads.
/// </para>
/// <para>
/// The window's size is read from a counter patched into the emitted window, which starts at 64
/// characters so that a record fits and a run of them does not.
/// </para>
/// </remarks>
public sealed class PartWindowTests
{
	public static TheoryData<string> Grammars =>
	[
		// The run is a group, its element a line: read whole by one machine.
		"Feed : @string[] = Hd? & ('t'* & 't' & eol)* & eof\n" +
		"Hd   : @string   = 'h' & eol => @(parserText)\n" +
		"parse Feed\n",

		"Feed : @string[] = Hd? & ('t' & 't' & eol)* & eof\n" +
		"Hd   : @string   = 'h' & eol => @(parserText)\n" +
		"parse Feed\n",

		// A repetition of a rule that hands over nothing, before the one that does.
		"Feed : @string[] = Hd? & Blank* & Row* & eof\n" +
		"Hd   : @string   = 'h' & eol => @(parserText)\n" +
		"Blank = 't' & 't' & eol\n" +
		"Row  : @string   = 'r' & eol => @(parserText)\n" +
		"parse Feed\n",

		// A marked repetition of a rule that hands over nothing: it recovers inside the part.
		"Feed : @string[] = Hd? & Blank* recover eol & Row* & eof\n" +
		"Hd   : @string   = 'h' & eol => @(parserText)\n" +
		"Blank = 't' & 't' & eol\n" +
		"Row  : @string   = 'r' & eol => @(parserText)\n" +
		"parse Feed\n",

		// The controls: a run of records read a record at a time.
		"Feed : @string[] = Hd? & Row* & eof\n" +
		"Hd   : @string   = 'h' & eol => @(parserText)\n" +
		"Row  : @string   = 't' & 't' & eol => @(parserText)\n" +
		"parse Feed\n",

		"Feed : @string[] = Row* recover eol & eof\n" +
		"Row  : @string   = 't' & 't' & eol => @(parserText)\n" +
		"parse Feed\n",
	];

	[Theory]
	[MemberData(nameof(Grammars))]
	public void A_reader_holds_one_element_of_a_run_or_is_not_offered(string grammar)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions { ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance });

		Assert.DoesNotContain(result.Diagnostics, static one => one.Severity == GramSeverity.Error);

		var assembly = EmittedCode.Compile(
			Patch(result.Sources[0].Text), declarationMembers: "internal static int Peak_Test = 0;",
			sourceParts: result.Sources.Skip(1).Select(static one => Patch(one.Text)));

		var type = assembly.GetType("Grammar")!;

		if (type.GetMethod("ParseFeed", [typeof(TextReader)]) is null)
		{
			Assert.Contains(result.Diagnostics, static one => one.Id == "GRAM5001");

			return;
		}

		Assert.DoesNotContain(result.Diagnostics, static one => one.Id == "GRAM5001");

		var small = Peak(type, Lines(1_000));
		var large = Peak(type, Lines(50_000));

		// Fifty times the feed does not make the window any larger than a record needs.
		Assert.True(large <= 4096 && large <= small * 4, $"the window peaked at {small} characters for 1,000 lines and {large} for 50,000");

		// And it reads what the string reads.
		var text = Lines(100);

		Assert.Equal(
			Strings(type.GetMethod("ParseFeed", [typeof(string)])!.Invoke(null, [text])),
			Strings(type.GetMethod("ParseFeed", [typeof(TextReader)])!.Invoke(null, [new StringReader(text)])));
	}

	/// <summary>
	/// A turn whose first literal is a word under <c>wordboundary</c>, read where the turn before it ended
	/// on a word character: the literal's look behind it refuses there. Where the turn runs into the end
	/// of the window and the window is refilled, what lies before the turn has to be kept for that look.
	/// </summary>
	public static TheoryData<string, string, bool> LookingBehind =>
	new()
	{
		{
			"a repeated group",
			"wordboundary = ['a'..'z']\n" +
			"Hd : @string = '.'+ => @(\"h\")\n" +
			"Start : @string[] = Hd? & { (\"a\" & '!' | ['a'] & '!' & '!') & ['x'] }* & eof\n",
			true
		},
		{
			"a repeated rule",
			"wordboundary = ['a'..'z']\n" +
			"Hd : @string = '.'+ => @(\"h\")\n" +
			"Row : @string = { (\"a\" & '!' | ['a'] & '!' & '!') & ['x'] } => @(\"r\")\n" +
			"Start : @string[] = Hd? & Row* & eof\n",
			true
		},
		{
			"a repeated rule through the seams of a spaced collection",
			"trivia = { ' '* }\n" +
			"wordboundary = ['a'..'z']\n" +
			"Hd : @string = { '.'+ } => @(\"h\")\n" +
			"Item : @string = { (\"a\" & '!' | ['a'] & '!' & '!') & ['x'] } => @(\"i\")\n" +
			"Start : @string[] = Hd? & Item* & eof\n",
			true
		},
		{
			"a repeated group whose turn begins with the trivia",
			"trivia = { ' '* }\n" +
			"wordboundary = ['a'..'z']\n" +
			"Hd : @string = { '.'+ } => @(\"h\")\n" +
			"Start : @string[] = Hd? & (\"a\" & { '!' | '?' & '!' } & { ['x'] })* & eof\n",
			false
		},
		{
			"a repeated group whose turn begins with spaces of its own",
			"wordboundary = ['a'..'z']\n" +
			"Hd : @string = '.'+ => @(\"h\")\n" +
			"Start : @string[] = Hd? & { ' '* & (\"a\" & '!' | ['a'] & '!' & '!') & ['x'] }* & eof\n",
			true
		},
	};

	[Theory]
	[MemberData(nameof(LookingBehind))]
	public void A_turn_refilled_at_the_window_end_still_looks_behind_it(string name, string grammar, bool streams)
	{
		var result = GramCompiler.Compile(grammar + "parse Start\n", new GramCompilerOptions { ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance });

		Assert.DoesNotContain(result.Diagnostics, static one => one.Severity == GramSeverity.Error);

		var assembly = EmittedCode.Compile(result.Sources[0].Text, sourceParts: result.Sources.Skip(1).Select(static one => one.Text));
		var type     = assembly.GetType("Grammar")!;
		var reader   = type.GetMethod("ParseStart", [typeof(TextReader)]);

		Assert.True(streams == reader is not null, name);
		Assert.Equal(!streams, result.Diagnostics.Any(static one => one.Id == "GRAM5001"));

		if (reader is null)
			return;

		// Every place the second turn can begin about the end of the default window of 4096.
		foreach (var tail in new[] { "a!xa!x", "a!x a!x", "a!x a!!x", "a!xa!!x" })
			for (var pad = 4080; pad <= 4100; pad++)
			{
				var text = new string('.', pad) + tail;

				Assert.Equal(Outcome(type.GetMethod("ParseStart", [typeof(string)])!, text), Outcome(reader, new StringReader(text)));
			}
	}

	static string Outcome(MethodInfo method, object input)
	{
		try
		{
			return string.Join(",", Strings(method.Invoke(null, [input])));
		}
		catch (TargetInvocationException thrown) when (thrown.InnerException is FormatException)
		{
			return "refused";
		}
		catch (FormatException)
		{
			return "refused";
		}
	}

	static string Lines(int count)
	{
		return string.Concat(Enumerable.Repeat("tt\n", count));
	}

	static string Patch(string source)
	{
		return Regex.Replace(
			source.Replace("new Window_DotGram(input, 4096)", "new Window_DotGram(input, 64)"),
			@"_filled \+= read;",
			"_filled += read; if (_filled > Peak_Test) Peak_Test = _filled;");
	}

	static int Peak(Type type, string text)
	{
		var field = type.GetField("Peak_Test", BindingFlags.NonPublic | BindingFlags.Static)!;

		field.SetValue(null, 0);

		foreach (var _ in (IEnumerable)type.GetMethod("ParseFeed", [typeof(TextReader)])!.Invoke(null, [new StringReader(text)])!)
		{
		}

		return (int)field.GetValue(null)!;
	}

	static string[] Strings(object? values)
	{
		return [.. ((IEnumerable)values!).Cast<object?>().Select(static one => one?.ToString() ?? "")];
	}
}
