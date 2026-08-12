using System;
using System.Collections.Generic;

namespace DotGram.Grammar;

/// <summary>
/// The regular-expression spelling of a Unicode category, and what .NET calls it.
/// </summary>
/// <remarks>
/// <c>\p{Lu}</c> is written the way .NET regular expressions write it (docs/syntax.md
/// §3.1), but <c>System.Globalization.UnicodeCategory</c> spells the same thing
/// <c>UppercaseLetter</c>. Something has to translate, and it has to be the same
/// something that decides which abbreviations exist at all — otherwise an unknown one
/// is not caught here and turns up as a C# compiler error about a member of an enum
/// the author never mentioned.
/// </remarks>
public static class UnicodeCategories
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
	public static string? NameOf(string abbreviation) =>
		abbreviation is not null && _names.TryGetValue(abbreviation, out var name) ? name : null;

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

	public static bool Exists(string abbreviation) => Expand(abbreviation).Count > 0;
}
