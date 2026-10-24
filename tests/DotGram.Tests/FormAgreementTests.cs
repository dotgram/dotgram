using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// One answer in every form and carrier: what a count of an element that may stand still builds, and
/// where a refusal is, over shapes the other tables do not hold (docs/syntax.md §3.3, §7.5).
/// </summary>
public sealed class FormAgreementTests
{
	const string SpanHost = """
		[System.ThreadStatic] static int calls;
		public static void Reset() { calls = 0; }
		static void Count() { if (++calls > 1000) throw new System.InvalidOperationException("A repetition went round without moving."); }
		static bool Nothing(System.ReadOnlySpan<char> input, ref int p) { Count(); return true; }
		static bool Half(System.ReadOnlySpan<char> input, ref int p) { Count(); if (p < input.Length && input[p] == 'x') p++; return true; }
		""";

	sealed record Config(string Name, bool Direct, CarrierKind Carrier);

	static readonly Config[] Configs =
	[
		new("direct", true,  CarrierKind.Auto),
		new("engine", false, CarrierKind.Auto),
		new("tape",   true,  CarrierKind.Tape),
		new("engine-tape", false, CarrierKind.Tape),
	];

	// --- Standing turns -------------------------------------------------------------------

	/// <summary>A synchronization that itself may stand still, rather than one with a standing loop inside.</summary>
	[Theory]
	[InlineData("T = 'a'* recover @Nothing & eof", "x")]
	[InlineData("T = 'a'* recover @Nothing & eof", "ax")]
	[InlineData("T = 'a'* recover (@Half & ';') & eof", "y;")]
	[InlineData("S = @Nothing\nT = 'a'* recover S & eof", "x")]
	public void A_synchronization_that_stands_still_ends(string grammar, string input)
	{
		foreach (var config in Configs)
		{
			var parser = BuildHosted(grammar, config);

			parser.GetType("Probe")!.GetMethod("Reset")!.Invoke(null, null);

			// Any answer, within the bound.
			_ = Said(parser.GetType("Probe")!.GetMethod("TryParseT", [typeof(string)])!.Invoke(null, [input])!);
		}
	}

	/// <summary>
	/// <c>X{3}</c> is "exactly three" (§3.3) and unrolls to <c>X &amp; X &amp; X</c>: a bounded count over a
	/// recognizer that does not move is three values, as the unrolled sequence builds, not one.
	/// </summary>
	[Theory]
	[InlineData("E : @int = @Nothing => @(1)\nT : @int = xs: E{3} & eof => @(xs.Length)", "", "ok@0 3")]
	[InlineData("E : @int = @Nothing => @(1)\nT : @int = xs: E{3} & eof => @(xs[2])", "", "ok@0 1")]
	[InlineData("E : @int = @Nothing => @(1)\nT : @int = xs: E{2,} & eof => @(xs.Length)", "", "ok@0 2")]
	public void A_bounded_count_of_a_standing_element_holds_its_declared_turns(string grammar, string input, string expected)
	{
		foreach (var config in Configs)
		{
			var parser = BuildHosted(grammar, config);

			parser.GetType("Probe")!.GetMethod("Reset")!.Invoke(null, null);

			Assert.Equal(expected, Said(parser.GetType("Probe")!.GetMethod("TryParseT", [typeof(string)])!.Invoke(null, [input])!));
		}
	}

	// --- Refusal places over shapes and carriers RefusalPlaceTests does not hold ------------------

	static readonly (string Name, string Grammar)[] Shapes =
	[
		("wide",        "T = \"äb\"i & eof"),
		("widefolded",  "T = \"äb\"i & ';'"),
		("spaced",      "trivia = { ' '* }\nT = \"ab\" & \"cd\" & eof"),
		("commented",   "trivia = { (' ' | \"--\" & [^ '\\n']*)* }\nT = 'a' & 'b' & eof"),
		("recovered",   "T = (\"ab\" & ';')* recover ';' & eof"),
		("atomic",      "T = {\"ab\" | 'a'} & 'c' & eof"),
		("threeway",    "T = (\"abc\" | \"abd\" | \"ab\") & 'x' & eof"),
		("caseRun",     "T = (\"ab\"i | \"ac\"i | 'x') & eof"),
		("plusRun",     "T = (\"ab\" | \"cd\")+ & eof"),
		("scanplus",    "W = \"ab\"+ & ';'\nT = W & eof"),
		("scanstar",    "W = ('a' | \"bc\")* & ';'\nT = W & eof"),
	];

	public static TheoryData<string> Names()
	{
		var names = new TheoryData<string>();

		foreach (var (name, _) in Shapes)
			names.Add(name);

		return names;
	}

	[Theory]
	[MemberData(nameof(Names))]
	public void Every_form_and_carrier_refuses_at_one_place(string name)
	{
		var grammar  = Shapes.Single(one => one.Name == name).Grammar;
		var built    = Configs.Select(config => (config.Name, Parser: Build(grammar, config))).ToArray();
		var alphabet = Alphabet(grammar);
		var failures = new List<string>();

		foreach (var input in Inputs(alphabet, 4))
		{
			var answers = new List<(string Form, string? Answer)>();

			foreach (var (config, parser) in built)
			{
				answers.Add(($"{config} span", Answer(parser, input, typeof(string), null)));
				answers.Add(($"{config} reader 1", Answer(parser, input, typeof(TextReader), 1)));
				answers.Add(($"{config} reader", Answer(parser, input, typeof(TextReader), null)));

				if (input.All(static one => one < 0x80))
					answers.Add(($"{config} bytes 1", Answer(parser, input, typeof(Stream), 1)));
			}

			var told = answers.Where(static one => one.Answer is not null).ToList();

			if (told.Any(one => one.Answer != told[0].Answer))
				failures.Add($"\"{input}\": " + string.Join(", ", told.Select(static one => $"{one.Form} {one.Answer}")));

			if (failures.Count >= 6)
				break;
		}

		Assert.True(failures.Count == 0, $"{name}: {grammar}\n" + string.Join("\n", failures));
	}

	/// <summary>A comment in the trivia that the input ends inside, or stops agreeing with, is refused there in every form (§7.5).</summary>
	[Theory]
	[InlineData("-",   "Starved@1")]
	[InlineData("- ",  "NoMatch@1")]
	[InlineData("a -", "Starved@3")]
	public void A_comment_in_the_trivia_is_refused_where_it_stops_agreeing(string input, string expected)
	{
		const string grammar = "trivia = { (' ' | \"--\" & [^ '\\n']*)* }\nT = 'a' & 'b' & eof";

		foreach (var config in Configs)
		{
			var parser = Build(grammar, config);

			Assert.Equal(expected, Answer(parser, input, typeof(TextReader), 1));
			Assert.Equal(expected, Answer(parser, input, typeof(string), null));
		}
	}

	static Assembly Build(string grammar, Config config)
	{
		var result = GramCompiler.Compile(grammar + "\nparse T", new GramCompilerOptions
		{
			ClassName     = "Grammar",
			Direct        = config.Direct,
			Carrier       = config.Carrier,
			BufferedInput = true,
			BufferedBytes = true,
			CSharpScanner = RoslynCSharpScanner.Instance,
		});

		EmittedCode.Quiet(result.Diagnostics.Where(static one => one.Id != GramCompiler.BufferedUnsupported));

		return EmittedCode.Compile(result.Sources[0].Text, sourceParts: result.Sources.Skip(1).Select(static one => one.Text));
	}

	static Assembly BuildHosted(string grammar, Config config)
	{
		var result = GramCompiler.Compile(grammar + "\nparse T", new GramCompilerOptions
		{
			ClassName      = "Probe",
			Direct         = config.Direct,
			Carrier        = config.Carrier,
			SymbolResolver = PermissiveSymbolResolver.Instance,
			CSharpScanner  = RoslynCSharpScanner.Instance,
		});

		EmittedCode.Quiet(result.Diagnostics);

		return EmittedCode.Compile(
			result.Sources[0].Text, className: "Probe", declarationMembers: SpanHost,
			sourceParts: result.Sources.Skip(1).Select(static one => one.Text));
	}

	static char[] Alphabet(string grammar)
	{
		var named = new SortedSet<char> { 'x' };
		var quote = '\0';

		foreach (var one in grammar)
		{
			if (quote != '\0')
			{
				if (one == quote)
					quote = '\0';
				else if (one != '\\')
					named.Add(one);
			}
			else if (one is '\'' or '"')
				quote = one;
		}

		return [.. named];
	}

	static IEnumerable<string> Inputs(char[] alphabet, int longest)
	{
		var level = new List<string> { "" };

		yield return "";

		for (var length = 1; length <= longest; length++)
		{
			var next = new List<string>(level.Count * alphabet.Length);

			foreach (var head in level)
				foreach (var one in alphabet)
				{
					var input = head + one;

					next.Add(input);

					yield return input;
				}

			level = next;
		}
	}

	static string? Answer(Assembly parser, string input, Type over, int? buffer)
	{
		var type   = parser.GetType("Grammar")!;
		var method = over == typeof(string)
			? type.GetMethod("TryParseT", [typeof(string)])
			: type.GetMethod("TryParseT", [over, typeof(int?), typeof(int?)]);

		if (method is null)
			return null;

		var match = over == typeof(string)
			? method.Invoke(null, [input])!
			: method.Invoke(null, [over == typeof(Stream) ? new MemoryStream(Encoding.UTF8.GetBytes(input)) : new StringReader(input), buffer, null])!;

		object? Read(string member)
		{
			return match.GetType().GetProperty(member)!.GetValue(match);
		}

		return (bool)Read("IsSuccess")!
			? $"Success@{Convert.ToInt64(Read("Position")) + Convert.ToInt64(Read("Length"))}"
			: $"{Read("Outcome")}@{Read("Position")}";
	}

	static string Said(object match)
	{
		object? Read(string name)
		{
			return match.GetType().GetProperty(name)!.GetValue(match);
		}

		return (bool)Read("IsSuccess")!
			? $"ok@{Convert.ToInt64(Read("Position")) + Convert.ToInt64(Read("Length"))}" + (Read("Value") is int count ? $" {count}" : "")
			: $"no@{Read("Position")} {Read("Outcome")}";
	}
}
