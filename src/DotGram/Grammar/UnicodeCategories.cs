using System;
using System.Collections.Generic;

namespace DotGram.Grammar;

/// <summary>
/// The regular-expression spelling of a Unicode category, and what .NET calls it.
/// </summary>
/// <remarks>
/// <para>
/// <c>\p{Lu}</c> is written the way .NET regular expressions write it (docs/syntax.md
/// §3.1), but <c>System.Globalization.UnicodeCategory</c> spells the same thing
/// <c>UppercaseLetter</c>. Something has to translate, and it has to be the same
/// something that decides which abbreviations exist at all — otherwise an unknown one
/// is not caught here and turns up as a C# compiler error about a member of an enum
/// the author never mentioned.
/// </para>
/// <para>
/// What each category holds is the generator's own too: Unicode <see cref="UnicodeVersion"/>,
/// written from the Unicode Character Database into <c>UnicodeCategories.Table.cs</c> by
/// <c>UnicodeCategories.generate.cs</c>, and never <c>CharUnicodeInfo</c>'s. The runtime answers
/// from its own Unicode version — 8.0 on .NET Framework, where an editor may host the generator,
/// and a later one with each .NET release — so asking it would make one grammar generate two
/// parsers on two build machines, and a parser that asked at run time accept different characters
/// on different runtimes. The analysis and the emitted test both read <see cref="Of"/>.
/// </para>
/// </remarks>
public static partial class UnicodeCategories
{
	static readonly Dictionary<string, string> _names = new(StringComparer.Ordinal)
	{
		["Lu"] = "UppercaseLetter",           ["Ll"] = "LowercaseLetter",
		["Lt"] = "TitlecaseLetter",           ["Lm"] = "ModifierLetter",
		["Lo"] = "OtherLetter",

		["Mn"] = "NonSpacingMark",            ["Mc"] = "SpacingCombiningMark",
		["Me"] = "EnclosingMark",

		["Nd"] = "DecimalDigitNumber",        ["Nl"] = "LetterNumber",
		["No"] = "OtherNumber",

		["Pc"] = "ConnectorPunctuation",      ["Pd"] = "DashPunctuation",
		["Ps"] = "OpenPunctuation",           ["Pe"] = "ClosePunctuation",
		["Pi"] = "InitialQuotePunctuation",   ["Pf"] = "FinalQuotePunctuation",
		["Po"] = "OtherPunctuation",

		["Sm"] = "MathSymbol",                ["Sc"] = "CurrencySymbol",
		["Sk"] = "ModifierSymbol",            ["So"] = "OtherSymbol",

		["Zs"] = "SpaceSeparator",            ["Zl"] = "LineSeparator",
		["Zp"] = "ParagraphSeparator",

		["Cc"] = "Control",                   ["Cf"] = "Format",
		["Cs"] = "Surrogate",                 ["Co"] = "PrivateUse",
		["Cn"] = "OtherNotAssigned",
	};

	/// <summary>The .NET name, or null when no such category is spelled that way.</summary>
	public static string? NameOf(string abbreviation)
	{
		return abbreviation is not null && _names.TryGetValue(abbreviation, out var name) ? name : null;
	}

	/// <summary>
	/// The groups .NET regular expressions also accept — <c>\p{L}</c> for every letter.
	/// </summary>
	/// <remarks>
	/// Listed separately because one of them is not one category but several, and a
	/// recognizer has to test for any of them.
	/// </remarks>
	static readonly Dictionary<string, string[]> _groups = new(StringComparer.Ordinal)
	{
		["L"] = ["Lu", "Ll", "Lt", "Lm", "Lo"],
		["M"] = ["Mn", "Mc", "Me"],
		["N"] = ["Nd", "Nl", "No"],
		["P"] = ["Pc", "Pd", "Ps", "Pe", "Pi", "Pf", "Po"],
		["S"] = ["Sm", "Sc", "Sk", "So"],
		["Z"] = ["Zs", "Zl", "Zp"],
		["C"] = ["Cc", "Cf", "Cs", "Co", "Cn"],
	};

	/// <summary>Every .NET category name an abbreviation stands for, or empty if none.</summary>
	public static IReadOnlyList<string> Expand(string abbreviation)
	{
		if (NameOf(abbreviation) is { } single)
			return [single];

		if (abbreviation is not null && _groups.TryGetValue(abbreviation, out var members))
		{
			var names = new string[members.Length];

			for (var i = 0; i < members.Length; i++)
				names[i] = _names[members[i]];

			return names;
		}

		return [];
	}

	/// <summary>How a regular expression writes <paramref name="category"/>: <c>Lu</c> for <c>UppercaseLetter</c>.</summary>
	public static string Abbreviation(System.Globalization.UnicodeCategory category)
	{
		var name = category.ToString();

		foreach (var pair in _names)
			if (pair.Value == name)
				return pair.Key;

		throw new ArgumentOutOfRangeException(nameof(category));
	}

	public static bool Exists(string abbreviation)
	{
		return Expand(abbreviation).Count > 0;
	}

	/// <summary>
	/// The general category of <paramref name="c"/> in Unicode <see cref="UnicodeVersion"/>: a
	/// surrogate is <see cref="System.Globalization.UnicodeCategory.Surrogate"/>, a code point not
	/// assigned there <see cref="System.Globalization.UnicodeCategory.OtherNotAssigned"/>.
	/// </summary>
	public static System.Globalization.UnicodeCategory Of(char c)
	{
		return (System.Globalization.UnicodeCategory)Spelled.ByCodeUnit[c];
	}

	/// <summary>The table's runs spelled out, a byte a code unit, built the first time one is asked for.</summary>
	/// <remarks>
	/// A class of its own because the runs are a field of the other half of this partial class, and
	/// the order in which two files' static initializers run is not defined: reading them from
	/// here runs all of the outer class's first.
	/// </remarks>
	static class Spelled
	{
		public static readonly byte[] ByCodeUnit = Build();

		static byte[] Build()
		{
			var table = new byte[char.MaxValue + 1];

			for (var i = 0; i < Runs.Length; i += 2)
			{
				var to = i + 2 < Runs.Length ? Runs[i + 2] : table.Length;

				for (var c = Runs[i]; c < to; c++)
					table[c] = (byte)Runs[i + 1];
			}

			return table;
		}
	}
}
