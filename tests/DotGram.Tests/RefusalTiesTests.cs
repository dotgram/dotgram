using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The sets that tie for the furthest refusal are kept once each in the end, not once per way of reaching it.
/// </summary>
/// <remarks>
/// A repetition inside a repetition refuses at one place by as many ways as it has of splitting
/// its input, and every way says the same sets there. A list that took each way's set held two
/// to the n of them, gigabytes on forty characters; the list is now made to hold each set once
/// whenever it reaches a power of two from a thousand and twenty-four on, so it stays within a few
/// times the number of different sets, however many ways said them. The test asks the length of what the refusal
/// hands over, which is the list itself, and not how long the refusal took.
/// </remarks>
public sealed class RefusalTiesTests
{
	[Theory]
	[InlineData("R0 = ('a'+)* & 'b'\nparse R0\n", 16, "Expected 'a' or 'b'.")]
	[InlineData("R0 = (('a' | 'a' & 'a')+)* & 'b'\nparse R0\n", 12, "Expected ['a'], 'b' or \"aa\".", "Expected 'a', \"aa\" or 'b'.")]
	[InlineData("R0 = (('a' | 'x')+ & ('a' | 'y')*)* & 'b'\nparse R0\n", 12, "Expected ['a' | 'x'] or 'b'.")]
	[InlineData("trivia = ' '*\nR0 = (A+)* & 'b'\nA = 'a' | 'a'\nparse R0\n", 12, "Expected A or 'b'.")]
	public void A_refusal_reached_in_many_ways_ties_each_set_once(string grammar, int length, string message, string? engineMessage = null)
	{
		foreach (var direct in new[] { true, false })
		{
			var result = GramCompiler.Compile(grammar, new GramCompilerOptions
			{
				ClassName      = "Grammar",
				Direct         = direct,
				BufferedInput  = true,
				BufferedBytes  = true,
				SymbolResolver = PermissiveSymbolResolver.Instance,
				CSharpScanner  = RoslynCSharpScanner.Instance,
			});

			EmittedCode.Quiet(result.Diagnostics.Where(static one => one.Id != GramCompiler.BufferedUnsupported));

			var assembly = EmittedCode.Compile(result.Sources[0].Text, sourceParts: result.Sources.Skip(1).Select(static one => one.Text));
			var match    = assembly.GetType("Grammar")!.GetMethod("TryParseR0", [typeof(string)])!.Invoke(null, [new string('a', length) + "c"])!;
			var field    = match.GetType().GetField("_tied", BindingFlags.NonPublic | BindingFlags.Instance);

			Assert.NotNull(field);

			var tied  = (Array?)field.GetValue(match);
			var count = tied?.Length ?? 0;

			// The grammar has two sets to say there at most, and the list is made distinct each time it
			// reaches a power of two from 1024 on; without that it holds 2^length.
			Assert.True(count <= 2048, $"{(direct ? "direct" : "engine")}: {count} sets tied\n{grammar}");
			Assert.False((bool)match.GetType().GetProperty("IsSuccess")!.GetValue(match)!);

			// Every way said the same sets, so the message is the one a single way gives.
			var error = (string?)match.GetType().GetProperty("Error")!.GetValue(match);

			var wanted = direct ? message : engineMessage ?? message;

			Assert.True(error == wanted, $"{(direct ? "direct" : "engine")}: {error}");
		}
	}

	const string Cross =
		"Row : @string = ('a'+)* & 'b' => @(\"r\")\n" +
		"Start : @string[] = Row* recover ';' => @(parserMessage)\nparse Start\n";

	/// <summary>A choice whose widest group is a call that has refused right there, over a refusal reached in very many ways.</summary>
	const string Dispatch =
		"Start = 'a'* & 'b' | B8 | B7 | B6 | B5 | B4 | B3 | B2 | B1 | B0 | 'a'* & 'c' | 'a'* & (Wide | '!') | '(' & Start & ')'\n" +
		"B0 = 'a'* & 'c' | 'a'* & 'd'\n" +
		"B1 = B0 | B0\nB2 = B1 | B1\nB3 = B2 | B2\nB4 = B3 | B3\nB5 = B4 | B4\nB6 = B5 | B5\nB7 = B6 | B6\nB8 = B7 | B7\n" +
		"Wide = Digit? & Alpha\nDigit = ['0'..'9']\nAlpha = ['e'..'z']\nparse Start\n";

	/// <summary>
	/// The sets a call added to the ties are taken back where the call's own refusal is said over
	/// them, by the length the list had before the call: a list that is not moved keeps that count true.
	/// </summary>
	[Theory]
	[InlineData(true, "aaa?", "Expected 'b', 'c', 'd' or ['0'..'9' | 'e'..'z' | '!'].")]
	[InlineData(false, "aaa?", "Expected 'b', 'c', 'd' or ['0'..'9' | 'e'..'z' | '!'].")]
	[InlineData(true, "aaaaaaaaaaaaaaaaaaaaaa?", "Expected 'b', 'c', 'd' or ['0'..'9' | 'e'..'z' | '!'].")]
	[InlineData(false, "aaaaaaaaaaaaaaaaaaaaaa?", "Expected 'b', 'c', 'd' or ['0'..'9' | 'e'..'z' | '!'].")]
	public void A_call_refused_over_a_long_list_of_ties_is_told_as_it_was(bool direct, string input, string message)
	{
		var result = GramCompiler.Compile(Dispatch, new GramCompilerOptions
		{
			ClassName      = "Grammar",
			Direct         = direct,
			BufferedInput  = true,
			BufferedBytes  = true,
			SymbolResolver = PermissiveSymbolResolver.Instance,
			CSharpScanner  = RoslynCSharpScanner.Instance,
		});

		EmittedCode.Quiet(result.Diagnostics.Where(static one => one.Id != GramCompiler.BufferedUnsupported));

		var assembly = EmittedCode.Compile(result.Sources[0].Text, sourceParts: result.Sources.Skip(1).Select(static one => one.Text));
		var read     = EmittedCode.Match(assembly, "Grammar", "TryParseStart", input);

		Assert.True(!read.IsSuccess && read.Error == message, read.Error);
	}

	const string Rows =
		"Row : @string = 'a' & ('a'* & 'x')? & 'x' => @(\"long\") | 'a' => @(\"short\") | 'x' => @(\"x\")\n" +
		"Start : @string[] = Row* recover ';' => @(parserMessage)\nparse Start\n";

	const string Pairs =
		"Item : @string = 'a' & 'b' => @(\"i\") | 'a' & 'c' => @(\"j\")\n" +
		"Start : @string[] = Item* recover ';' => @(parserMessage)\nparse Start\n";

	const string Nested =
		"Cell : @string = 'a' & 'b' => @(\"c\") | 'a' & 'c' => @(\"c\") | 'd' => @(\"c\")\n" +
		"Cells : @string[] = Cell* recover ',' => @(parserMessage)\n" +
		"Line : @string = '[' & Cells & ']' => @(\"L\")\n" +
		"Start : @string[] = Line* recover ';' => @(parserMessage)\nparse Start\n";

	/// <summary>
	/// A set said before a turn of a recovering repetition began is said again inside it, where
	/// the turn's element is told what would have fit: dropping it as a repeat lost the word.
	/// What each recovery says is held in both forms.
	/// </summary>
	[Theory]
	[InlineData(Rows, "aaa?;", "[short|short|short|Expected end of input or 'x' at 1:4.]")]
	[InlineData(Rows, "ax?;a;", "[long|Expected end of input or 'x' at 1:3.|short|Expected end of input or 'x' at 1:6.]")]
	[InlineData(Rows, "xx;ax", "[x|x|Expected end of input or 'x' at 1:3.|long]")]
	[InlineData(Pairs, "ab?;ac;ad;", "[i|Expected end of input or \"ac\" at 1:3.|j|Expected end of input or \"ac\" at 1:7.|Expected \"ab\" or \"ac\" at 1:9.]")]
	[InlineData(Pairs, "abab;a", "[i|i|Expected end of input or \"ac\" at 1:5.|Expected \"ab\" or \"ac\" at 1:7.]")]
	[InlineData(Cross, "aaaaaaaaaaaaaaa?;aaaaaaaaaaaaaaaaaaa?;", "[Expected 'a' or 'b' at 1:16.|Expected 'a' or 'b' at 1:37.]")]
	[InlineData(Cross, "aaaaaaaaaaaaaaa?;b;", "[Expected 'a' or 'b' at 1:16.|r|Expected end of input or 'b' at 1:19.]")]
	[InlineData(Nested, "[ab,ad,ac];[ab,a,d];", "[L|Expected end of input or '[' at 1:11.|L|Expected end of input or '[' at 1:20.]")]
	[InlineData(Nested, "[ab,ax];[", "[Expected ']', \"ab\", \"ac\" or 'd' at 1:10.|Expected ']', \"ab\", \"ac\" or 'd' at 1:10.]")]
	public void What_a_recovery_says_was_wanted_is_told_in_both_forms(string grammar, string input, string expected)
	{
		var told = new List<string>();

		foreach (var direct in new[] { true, false })
		{
			var result = GramCompiler.Compile(grammar, new GramCompilerOptions
			{
				ClassName      = "Grammar",
				Direct         = direct,
				BufferedInput  = true,
				BufferedBytes  = true,
				SymbolResolver = PermissiveSymbolResolver.Instance,
				CSharpScanner  = RoslynCSharpScanner.Instance,
			});

			EmittedCode.Quiet(result.Diagnostics.Where(static one => one.Id != GramCompiler.BufferedUnsupported));

			var assembly = EmittedCode.Compile(result.Sources[0].Text, sourceParts: result.Sources.Skip(1).Select(static one => one.Text));
			var read     = EmittedCode.Match(assembly, "Grammar", "TryParseStart", input);

			told.Add(read.IsSuccess ? Flatten(read.Value) : "refused: " + read.Error);
		}

		Assert.True(told[0] == expected && told[1] == expected, $"direct: {told[0]}\nengine: {told[1]}");
	}

	static string Flatten(object? value)
	{
		return value is System.Collections.IEnumerable items and not string
			? "[" + string.Join("|", items.Cast<object?>().Select(Flatten)) + "]"
			: value?.ToString() ?? "null";
	}
}
