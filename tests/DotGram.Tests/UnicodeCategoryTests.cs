using System;
using System.Collections.Generic;
using System.Globalization;

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
		var members = new HashSet<UnicodeCategory>();

		foreach (var member in UnicodeCategories.Expand(name))
			members.Add(Enum.Parse<UnicodeCategory>(member));

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
