using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Model;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The generator's own table of Unicode categories, held to the Unicode version it names and to
/// the runtimes that answer from another.
/// </summary>
/// <remarks>
/// The table is what <c>\p{…}</c> means everywhere — the analysis and the emitted test — so what is
/// checked here is the data: that it is Unicode 16.0 where a runtime reads 16.0 too, and, on a
/// runtime that does not, which code units the two disagree about. Those are not failures: they are
/// the characters a parser would have read differently on that runtime before the generator carried
/// a table of its own.
/// </remarks>
public sealed class UnicodeCategoryTests(ITestOutputHelper output)
{
	/// <summary>
	/// The major version of .NET whose <see cref="CharUnicodeInfo"/> reads the same Unicode version as
	/// the table: .NET 10 reads 16.0. .NET 8 and 9 read 15.0 and 15.1, and .NET Framework 8.0.
	/// </summary>
	const int SameVersionRuntime = 10;

	[Fact]
	public void The_table_is_the_runtime_s_where_the_runtime_reads_the_same_Unicode()
	{
		var differing = new List<string>();

		for (var c = 0; c <= char.MaxValue; c++)
		{
			var ours    = UnicodeCategories.Of((char)c);
			var runtime = CharUnicodeInfo.GetUnicodeCategory((char)c);

			if (ours != runtime)
				differing.Add($"U+{c:X4} {runtime} -> {ours}");
		}

		if (Environment.Version.Major == SameVersionRuntime)
		{
			Assert.Empty(differing);

			return;
		}

		output.WriteLine(
			$"On .NET {Environment.Version}, {differing.Count} code units differ from Unicode {UnicodeCategories.UnicodeVersion}:");

		foreach (var one in differing)
			output.WriteLine(one);
	}

	/// <summary>Every name <c>\p{…}</c> accepts: the thirty categories and the seven groups of them.</summary>
	public static TheoryData<string> Names()
	{
		return new TheoryData<string>(
		[
			"Lu", "Ll", "Lt", "Lm", "Lo", "Mn", "Mc", "Me", "Nd", "Nl", "No", "Pc", "Pd", "Ps", "Pe",
			"Pi", "Pf", "Po", "Sm", "Sc", "Sk", "So", "Zs", "Zl", "Zp", "Cc", "Cf", "Cs", "Co", "Cn",
			"L", "M", "N", "P", "S", "Z", "C",
		]);
	}

	/// <summary>
	/// What the analysis takes a category to hold — the first set of an element naming it, and of
	/// one refusing it — is the table's, code unit for code unit, whatever runtime the generator is
	/// on.
	/// </summary>
	[Theory]
	[MemberData(nameof(Names))]
	public void The_analysis_reads_a_category_from_the_table(string name)
	{
		var members = Members([name]);

		Assert.NotEmpty(members);

		var named   = Holds(FirstSets.OfElement(new Node.Element(false, [], [name], [])));
		var refused = Holds(FirstSets.OfElement(new Node.Element(true,  [], [name], [])));

		for (var c = 0; c <= char.MaxValue; c++)
		{
			var inside = members.Contains(UnicodeCategories.Of((char)c));

			if (named[c] != inside)
				Assert.Fail($"\\p{{{name}}} and U+{c:X4}: the first set says {named[c]}.");

			if (refused[c] == inside)
				Assert.Fail($"[^ \\p{{{name}}}] and U+{c:X4}: the first set says {refused[c]}.");
		}
	}

	/// <summary>
	/// The classes of categories the shipped grammars write, beside the names alone: an identifier's
	/// start and part in SQL, T-SQL, the expression language and the standard library, and the
	/// separators of T-SQL's white space.
	/// </summary>
	static readonly (string Rule, string Class, string[] Categories, string Others)[] InUse =
	[
		("Name",       @"[\p{L} | '_']",                                                          ["L"],                                      "_"),
		("NamePart",   @"[\p{L} | \p{Nd} | '_']",                                                 ["L", "Nd"],                                "_"),
		("SqlStart",   @"[\p{L} | \p{Nl}]",                                                       ["L", "Nl"],                                ""),
		("SqlPart",    @"[\p{L} | \p{Nl} | '·' | \p{Mn} | \p{Mc} | \p{Nd} | \p{Pc} | \p{Cf}]",   ["L", "Nl", "Mn", "Mc", "Nd", "Pc", "Cf"], "·"),
		("TsqlPart",   @"[\p{L} | \p{Nd} | '_' | '@' | '$' | '#']",                               ["L", "Nd"],                                "_@$#"),
		("Separator",  @"['\t'..'\r' | '\u0085' | \p{Zs} | \p{Zl} | \p{Zp}]",                     ["Zs", "Zl", "Zp"],                         "\t\n\u000B\f\r\u0085"),
	];

	delegate bool TryParse(string input, out string value);

	/// <summary>
	/// What the emitted test admits is the table, for every code unit: each name alone and refused, a
	/// single character and a run of them, and each class the shipped grammars write — the same
	/// answer on every runtime, since the parser carries the bits and asks the runtime nothing.
	/// </summary>
	/// <remarks>
	/// One grammar of every reading, compiled once: a compilation a reading would cost more than the
	/// readings themselves. <c>Cn</c>, the category that moves most between Unicode versions, is
	/// among the names, and so are the surrogates, which a recognizer over UTF-16 reads as code units
	/// of their own.
	/// </remarks>
	[Fact]
	public void The_emitted_test_admits_what_the_table_holds()
	{
		var grammar  = new System.Text.StringBuilder();
		var readings = new List<(string Rule, Func<char, bool> Expected, string Suffix)>();

		foreach (var row in Names())
		{
			var name    = (string)row.Data;
			var members = Members([name]);

			grammar.Append($"In{name}  : @string = t: [\\p{{{name}}}]    => @(t)\n");
			grammar.Append($"Out{name} : @string = t: [^ \\p{{{name}}}] => @(t)\n");
			grammar.Append($"Run{name} : @string = t: [\\p{{{name}}}]+   => @(t)\n");
			readings.Add(($"In{name}",  c => members.Contains(UnicodeCategories.Of(c)),  ""));
			readings.Add(($"Out{name}", c => !members.Contains(UnicodeCategories.Of(c)), ""));
			readings.Add(($"Run{name}", c => members.Contains(UnicodeCategories.Of(c)),  ""));
		}

		foreach (var (rule, @class, categories, others) in InUse)
		{
			var members = Members(categories);
			var also    = others;

			grammar.Append($"{rule} : @string = t: {@class} => @(t)\n");
			grammar.Append($"{rule}Run : @string = t: {@class}+ & ';' => @(t)\n");
			readings.Add((rule,         c => also.IndexOf(c) >= 0 || members.Contains(UnicodeCategories.Of(c)), ""));
			readings.Add((rule + "Run", c => also.IndexOf(c) >= 0 || members.Contains(UnicodeCategories.Of(c)), ";"));
		}

		foreach (var (rule, _, _) in readings)
			grammar.Append($"parse {rule}\n");

		var result = GramCompiler.Compile(grammar.ToString(), new GramCompilerOptions
		{
			CSharpScanner = RoslynCSharpScanner.Instance,
		});

		EmittedCode.Quiet(result.Diagnostics);

		var source = string.Concat(result.Sources.Select(static one => one.Text));

		Assert.DoesNotContain("CharUnicodeInfo", source, StringComparison.Ordinal);
		Assert.DoesNotContain("GetUnicodeCategory", source, StringComparison.Ordinal);

		var assembly = EmittedCode.Compile(
			result.Sources[0].Text, sourceParts: result.Sources.Skip(1).Select(static one => one.Text));
		var type     = assembly.GetType("Grammar")!;
		var failures = new List<string>();

		foreach (var (rule, expected, suffix) in readings)
		{
			var method = (TryParse)Delegate.CreateDelegate(
				typeof(TryParse), type.GetMethod("TryParse" + rule, [typeof(string), typeof(string).MakeByRefType()])!);

			for (var c = 0; c <= char.MaxValue; c++)
			{
				var input = ((char)c).ToString() + suffix;

				if (method(input, out _) != expected((char)c) && failures.Count < 50)
					failures.Add($"{rule}: U+{c:X4} read {!expected((char)c)}, the table says {expected((char)c)}");
			}
		}

		Assert.True(failures.Count == 0, string.Join("\n", failures));
	}

	static HashSet<UnicodeCategory> Members(IEnumerable<string> names)
	{
		var members = new HashSet<UnicodeCategory>();

		foreach (var name in names)
			foreach (var member in UnicodeCategories.Expand(name))
				members.Add(Enum.Parse<UnicodeCategory>(member));

		return members;
	}

	/// <summary>The code units a first set holds, one flag each.</summary>
	static bool[] Holds(FirstSets.First first)
	{
		Assert.False(first.Anything);

		var holds = new bool[char.MaxValue + 1];

		foreach (var range in first.Ranges)
			for (int c = range.From; c <= range.To; c++)
				holds[c] = true;

		return holds;
	}

	[Fact]
	public void The_table_is_the_version_case_folding_reads()
	{
		Assert.Equal("16.0.0", UnicodeCategories.UnicodeVersion);
		Assert.Equal(CaseFold.UnicodeVersion, UnicodeCategories.UnicodeVersion);
	}

	/// <summary>
	/// What the file the table is written from does not say outright: the ranges it gives by their
	/// ends, the surrogates, and every code point it does not list.
	/// </summary>
	[Theory]
	[InlineData(0x0041, UnicodeCategory.UppercaseLetter)]
	[InlineData(0x4E00, UnicodeCategory.OtherLetter)]          // <CJK Ideograph, First>
	[InlineData(0x7000, UnicodeCategory.OtherLetter)]          // inside that range
	[InlineData(0x9FFF, UnicodeCategory.OtherLetter)]          // <CJK Ideograph, Last>
	[InlineData(0xAC00, UnicodeCategory.OtherLetter)]          // <Hangul Syllable, First>
	[InlineData(0xD7A3, UnicodeCategory.OtherLetter)]          // <Hangul Syllable, Last>
	[InlineData(0xD7A4, UnicodeCategory.OtherNotAssigned)]
	[InlineData(0xD800, UnicodeCategory.Surrogate)]
	[InlineData(0xDBFF, UnicodeCategory.Surrogate)]
	[InlineData(0xDC00, UnicodeCategory.Surrogate)]
	[InlineData(0xDFFF, UnicodeCategory.Surrogate)]
	[InlineData(0xE000, UnicodeCategory.PrivateUse)]
	[InlineData(0xF8FF, UnicodeCategory.PrivateUse)]
	[InlineData(0x0378, UnicodeCategory.OtherNotAssigned)]
	[InlineData(0xFFFF, UnicodeCategory.OtherNotAssigned)]
	[InlineData(0x1C89, UnicodeCategory.UppercaseLetter)]      // new in Unicode 16.0
	[InlineData(0xA7CD, UnicodeCategory.LowercaseLetter)]      // new in Unicode 16.0
	[InlineData(0x0897, UnicodeCategory.NonSpacingMark)]       // new in Unicode 16.0
	[InlineData(0x08BE, UnicodeCategory.OtherLetter)]          // new since Unicode 8.0, .NET Framework's
	public void A_code_unit_has_its_Unicode_16_category(int code, UnicodeCategory expected)
	{
		Assert.Equal(expected, UnicodeCategories.Of((char)code));
	}
}
