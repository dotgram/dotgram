using System;
using System.Collections.Generic;
using System.Linq;

using DotGram;

namespace DotGram.Examples;

// An INI file read into the shape a caller actually wants:
//
//     var ini = IniFile.Read(text);
//
//     ini["server"]["port"]        // "8080"
//     ini[""]["name"]              // a key written before any section
//     ini.Sections                 // in the order they were written
//
// The point of this example is the last step rather than the grammar. Parsing INI is
// easy; what makes a parser worth having is that it hands back a dictionary of
// dictionaries instead of a list of nodes for the caller to fold up themselves. The
// grammar collects sections as a sequence (§4.1 case 2) and the `=>` on the top rule
// turns that sequence into the lookup — so the folding happens once, here, rather than
// in every place that reads a setting.
//
// What the format actually needs, and what the grammar therefore says:
//
//   * keys before any section belong to a nameless one, which is how every INI reader
//     behaves and is why `Ini` has a `global` operand before the sections;
//   * `;` and `#` both start a comment, and a comment may sit on its own line;
//   * a value runs to the end of the line, `=` and `;` included — `path=C:\a;b` is one
//     value, because INI has no escaping to say otherwise;
//   * a later key in the same section wins, which is what `ToDictionary` would refuse to
//     do, so the fold is written to overwrite rather than throw.

[Gram("""
	@using DotGram.Examples;

	// Every space here is written down: a value keeps its inner spaces and loses only the
	// ones around it, which no automatic trivia insertion would get right.
	trivia = none

	Ini : @IniFile = global: Entries & sections: Section* & eof
	                   => @(new IniFile(global, sections))

	Section : @IniSection = Space & '[' & name: Name & ']' & Space & eol & entries: Entries
	                          => @(new IniSection(name, entries))

	// §4.1 case 2: the entries are what this collects, and the lines that are not entries
	// produce no value and so join nothing.
	// Every alternative consumes, or the repetition would not terminate — which is why
	// `eol` is the standard rule and not a copy of it with `eof` added. A nullable `eol`
	// would take that check away from every line-oriented grammar at once; the one line
	// allowed to end without a terminator is the last, and `Tail` is where it is said.
	Entries : @IniEntry[] = (Entry | Blank)* & Tail?

	Entry : @IniEntry = Space & key: Key & Space & '=' & value: Value & (eol | ?=eof)
	                      => @(new IniEntry(key, value.Trim()))

	// Not an entry and not a section: a comment, or nothing at all.
	Blank = Space & Comment? & eol

	Tail  = Space & Comment | Comment

	Comment = [';' | '#'] & [^ '\n' | '\r']*

	Key   : @string = text: [^ '=' | '[' | ']' | '\n' | '\r' | ';' | '#']+ => @(text.Trim())
	Name  : @string = text: [^ ']' | '\n' | '\r']+                        => @(text)

	// To the end of the line, whatever it holds: INI has no escape, so `a;b` is a value
	// with a semicolon in it rather than a value and a comment.
	Value : @string = text: [^ '\n' | '\r']*                              => @(text)

	Space = [' ' | '\t']*

	parse Ini
	""")]
public sealed partial class IniParser
{
	public static IniFile Read(string text) => ParseIni(text);
}

/// <summary>One `key = value`.</summary>
public sealed record IniEntry(string Key, string Value);

/// <summary>One `[section]` and what was written under it.</summary>
public sealed record IniSection(string Name, IReadOnlyList<IniEntry> Entries);

/// <summary>
/// A whole file, as the lookup a caller wants rather than the tree a parser produces.
/// </summary>
public sealed class IniFile
{
	readonly Dictionary<string, IReadOnlyDictionary<string, string>> _sections =
		new(StringComparer.OrdinalIgnoreCase);

	/// <param name="global">Entries written before any section, which belong to a nameless one.</param>
	public IniFile(IReadOnlyList<IniEntry> global, IReadOnlyList<IniSection> sections)
	{
		Sections = [.. sections.Select(section => section.Name)];

		_sections[""] = Fold(global);

		foreach (var section in sections)
			_sections[section.Name] = Fold(section.Entries);
	}

	/// <summary>The section of this name; the nameless one is <c>""</c>.</summary>
	public IReadOnlyDictionary<string, string> this[string section] =>
		_sections.TryGetValue(section, out var found) ? found : EmptySection;

	/// <summary>Section names, in the order they were written.</summary>
	public IReadOnlyList<string> Sections { get; }

	public bool Has(string section) => _sections.ContainsKey(section);

	/// <remarks>
	/// A later key wins, which is what every INI reader does and what <c>ToDictionary</c>
	/// would throw over — so the fold is written out rather than borrowed.
	/// </remarks>
	static IReadOnlyDictionary<string, string> Fold(IReadOnlyList<IniEntry> entries)
	{
		var folded = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		foreach (var entry in entries)
			folded[entry.Key] = entry.Value;

		return folded;
	}

	static readonly IReadOnlyDictionary<string, string> EmptySection =
		new Dictionary<string, string>();
}
