using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests;

/// <summary>The grammars and the asking that the tests of the cap on the ties share.</summary>
internal static class TieShapes
{
	internal const int Cap = 16384;

	internal const string Once = "R0 = ('a'+)* & 'b'\nparse R0\n";

	internal const string Twice = "R0 = (('a' | 'a' & 'a')+)* & 'b'\nparse R0\n";

	/// <summary>A refusal reached in very many ways, with a choice whose widest group is a call refused right there, over the ties.</summary>
	internal static string Dispatch(int levels)
	{
		var text = new StringBuilder();

		text.Append("Start = 'a'* & 'b' | B").Append(levels)
			.Append(" | 'a'* & ['0'..'9' | 'e'..'z'] | 'a'* & 'q' | 'a'* & ['0'..'9' | 'e'..'z'] | 'a'* & 'r'")
			.Append(" | 'a'* & (Wide | '!') | '(' & Start & ')'\n")
			.Append("B0 = 'a'* & 'c' | 'a'* & 'd'\n");

		for (var level = 1; level <= levels; level++)
			text.Append("B").Append(level).Append(" = B").Append(level - 1).Append(" | B").Append(level - 1).Append('\n');

		return text.Append("Wide = Digit? & Alpha\nDigit = ['0'..'9']\nAlpha = ['e'..'z']\nparse Start\n").ToString();
	}

	/// <summary>Three copies of a repetition that can take what the next one takes, valued, so that the direct reader on the immediate carrier replays.</summary>
	internal const string Replay =
		"R0 : @int = 'o'* & 'o'? => @(1)\n" +
		"T : @int = R0 & R0 & R0 & 'a' & eof => @(1)\nparse T\n";

	internal const string Rows =
		"Row : @string = 'a' & ('a'* & 'x')? & 'x' => @(\"long\") | 'a' => @(\"short\") | 'x' => @(\"x\")\n" +
		"Start : @string[] = Row* recover ';' => @(parserMessage)\nparse Start\n";

	internal const string Pairs =
		"Item : @string = 'a' & 'b' => @(\"i\") | 'a' & 'c' => @(\"j\")\n" +
		"Start : @string[] = Item* recover ';' => @(parserMessage)\nparse Start\n";

	internal const string Nested =
		"Cell : @string = 'a' & 'b' => @(\"c\") | 'a' & 'c' => @(\"c\") | 'd' => @(\"c\")\n" +
		"Cells : @string[] = Cell* recover ',' => @(parserMessage)\n" +
		"Line : @string = '[' & Cells & ']' => @(\"L\")\n" +
		"Start : @string[] = Line* recover ';' => @(parserMessage)\nparse Start\n";

	internal const string Cross =
		"Row : @string = ('a'+)* & 'b' => @(\"r\")\n" +
		"Start : @string[] = Row* recover ';' => @(parserMessage)\nparse Start\n";

	internal static readonly (string Name, bool Direct, CarrierKind Carrier)[] Renderings =
	[
		("engine", false, CarrierKind.Auto),
		("tape", true, CarrierKind.Tape),
		("immediate", true, CarrierKind.Immediate),
	];

	static readonly ConcurrentDictionary<(string, bool, CarrierKind, bool, string?), Type> Built = new();

	internal static Type Grammar(string grammar, bool direct, CarrierKind carrier, bool trace = false, string? members = null)
	{
		return Built.GetOrAdd((grammar, direct, carrier, trace, members), static key =>
		{
			var result = GramCompiler.Compile(key.Item1, new GramCompilerOptions
			{
				ClassName      = "Grammar",
				Direct         = key.Item2,
				Carrier        = key.Item3,
				Trace          = key.Item4,
				BufferedInput  = true,
				BufferedBytes  = true,
				SymbolResolver = PermissiveSymbolResolver.Instance,
				CSharpScanner  = RoslynCSharpScanner.Instance,
			});

			// A choice of two to the n copies is more than one method can hold optimized (GRAM5003): the grammar is built to be that large.
			EmittedCode.Quiet(result.Diagnostics.Where(static one => one.Id != GramCompiler.BufferedUnsupported && one.Id != "GRAM5003"));

			return EmittedCode.Compile(result.Sources[0].Text, declarationMembers: key.Item5, sourceParts: result.Sources.Skip(1).Select(static one => one.Text)).GetType("Grammar")!;
		});
	}

	/// <summary>What a parse says: the sets that tied (the length of the list the match keeps), the cap the parser holds, and the message or the value.</summary>
	internal static (int Tied, int Cap, string Said) Ask(Type grammar, string method, string input)
	{
		var match = grammar.GetMethod(method, [typeof(string)])!.Invoke(null, [input])!;
		var tied  = (Array?)match.GetType().GetField("_tied", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(match);
		var cap   = (int)grammar.GetField("TieCap_DotGram", BindingFlags.NonPublic | BindingFlags.Static)!.GetRawConstantValue()!;
		var value = match.GetType().GetProperty("Value")!.GetValue(match);
		var said  = (bool)match.GetType().GetProperty("IsSuccess")!.GetValue(match)!
			? "accepted"
			: (string?)match.GetType().GetProperty("Error")!.GetValue(match);

		return (tied?.Length ?? 0, cap, value is string[] messages ? "[" + string.Join("|", messages) + "]" : said ?? "null");
	}

	internal static string Source(string grammar, bool direct, CarrierKind carrier)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName      = "Grammar",
			Direct         = direct,
			Carrier        = carrier,
			BufferedInput  = true,
			BufferedBytes  = true,
			SymbolResolver = PermissiveSymbolResolver.Instance,
			CSharpScanner  = RoslynCSharpScanner.Instance,
		});

		return string.Concat(result.Sources.Select(static one => one.Text));
	}
}
