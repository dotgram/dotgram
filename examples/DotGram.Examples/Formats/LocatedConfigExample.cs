using System;

using DotGram;

namespace DotGram.Examples.Formats;

// A settings file, read twice by one grammar: once for what it says, and once for what
// it says *and where*.
//
//     # a comment
//     host = localhost
//     port = 8080
//
// A tool that only applies the settings wants the first. A tool that reports "unknown
// key 'prot' at line 3" wants the second, and the difference is not a second grammar:
//
//     [Gram("…")]                                                    Config.ParseFile
//     [GramParserVariant("Located", LocationType = typeof(ILocated))]
//                                                                    Config.Located.ParseFile
//
// `[Gram]` says which grammar, which a class has one of. `[GramParserVariant]` may be written
// as often as there are further readings wanted, and each is a compilation of its own
// into the nested class it names (docs/syntax.md §6.6). Everything it does not say it
// takes from the `[Gram]` above it; what it does say is the difference.
//
// `LocationType` names an interface the values implement. The reader offers a range to
// every rule a value came out of, innermost first, and the last offer is the one kept —
// so a value carries the extent of the outermost rule that built it. A grammar that does
// not ask for locations calls nothing and carries nothing: the plain reading below leaves
// `Where` exactly as the record was constructed with it.
//
// One thing to weigh before putting the location in the value: these are records, and
// a record that carries where it was written is no longer equal to one built anywhere
// else. Two readings of the same file are equal; a reading and a literal are not.
//
// Separately, `: @SourceSpan` is the other way to get at where something was, and a much
// smaller one: §4.1 case 4 says a rule with no `=>` and no captures is its matched extent
// — `string` gives the text of it and `SourceSpan` the two numbers. `KeySpan` below is
// that, and needs no interface, no option and no second reading.

/// <summary>Where something was written: an offset and a length into the input.</summary>
public readonly record struct At(int Offset, int Length);

/// <summary>What a value implements to be offered where it was written.</summary>
/// <remarks>
/// The two members are the contract: the reader calls <see cref="Locate"/> and the caller
/// reads <see cref="Where"/>. Neither name is the notation's — `LocationType` names this
/// interface and the generated code calls what is on it.
/// </remarks>
public interface ILocated
{
	/// <summary>Where this was written, once a reader has said so.</summary>
	At Where { get; }

	/// <summary>Offered by the reader for every rule this value came out of.</summary>
	void Locate(int at, int length);
}

/// <summary>One `key = value` line.</summary>
public sealed record Entry(string Key, string Value) : ILocated
{
	/// <inheritdoc cref="ILocated.Where"/>
	public At Where { get; private set; }

	/// <inheritdoc cref="ILocated.Locate"/>
	public void Locate(int at, int length)
	{
		Where = new At(at, length);
	}
}

[Gram("""
	@using DotGram.Examples.Formats;

	using Std;

	trivia = { (Spacing | LineComment("#"))* }

	File : @Entry[] = Entry* & eof

	Entry : @Entry = key: Identifier & '=' & value: Value => @(new Entry(key, value))

	// A value runs to the end of its line when an entry or the end of the file comes next. If not,
	// it stops at the last key that the next line finishes: `a=b=` and `c` is `a` with no value,
	// and `b` with the value `c`. A key whose equals sign is on the next line is taken before one
	// that has its own, even if that one stands later, in a comment: `a=a#a=` and `=` is `a` with
	// no value and then `a` whose equals sign is the second line's. Each choice is atomic, so
	// that a refused file is not read again for every way its lines could be cut.
	Value : @string = t: { Rest } ~ ?=Next => @(t)
	                | ?!({ Rest } ~ ?=Next) ~ t: Scan.Stop2 => @(t)
	                | ?!({ Rest } ~ ?=Next) ~ (?!Scan.Stop2) ~ t: Scan.Stop1 => @(t)

	Rest = [^ '\r' | '\n']*

	Char = [^ '\r' | '\n']

	Next = trivia & (Identifier & '=' | eof)

	// Up to the last place in the line where a key stands that the next line finishes, with the
	// equals sign (1) or without it (2).
	namespace Scan
	{
		trivia = none

		Stop1 = { ((?!Last1) & Char)* } & ?=Last1
		Stop2 = { ((?!Last2) & Char)* } & ?=Last2

		Last1 = Cut1 & ?!Later1
		Last2 = Cut2 & ?!Later2

		Later1 = Char & { ((?!Cut1) & Char)* } & Cut1
		Later2 = Char & { ((?!Cut2) & Char)* } & Cut2
	}

	// What is left of a line at such a place: the last letter of a key and the digits after it (any
	// earlier letter would be an earlier place), perhaps its equals sign, and nothing else but
	// blanks and a comment. The key is then checked whole, with its equals sign wherever the
	// trivia puts it, so that a name inside a comment is not taken for one.
	Cut1 = ?=Dangling1 ~ ?=(Identifier & '=')
	Cut2 = ?=Dangling2 ~ ?=(Identifier & '=')

	Dangling1 = [\p{L} | '_'] ~ [\p{Nd}]* ~ [' ' | '\t']* ~ '=' ~ [' ' | '\t']* ~ ('#' ~ Rest)? ~ ?=(eol | eof)
	Dangling2 = [\p{L} | '_'] ~ [\p{Nd}]* ~ [' ' | '\t']* ~ ('#' ~ Rest)? ~ ?=(eol | eof)

	// No `=>` and no captures, so this rule is what it matched — as two numbers rather
	// than as text (§4.1 case 4).
	KeySpan : @SourceSpan = Identifier

	parse File
	parse KeySpan
	""")]
[GramParserVariant("Located", LocationType = typeof(ILocated))]
public static partial class Config
{
	/// <summary>An offset, as a person would give it: a line and a column, both from 1.</summary>
	/// <remarks>
	/// The parser hands back offsets because that is what it has; turning one into a place
	/// in a file is counting line endings, and counting them is the caller's business and
	/// not the grammar's.
	/// </remarks>
	public static (int Line, int Column) PlaceOf(string text, int offset)
	{
		var line   = 1;
		var column = 1;

		for (var i = 0; i < offset && i < text.Length; i++)
		{
			if (text[i] == '\n')
			{
				line++;
				column = 1;
			}
			else
			{
				column++;
			}
		}

		return (line, column);
	}

	/// <summary>The keys this file does not know, and where each of them was written.</summary>
	public static string[] Unknown(string text, params string[] known)
	{
		var said = new System.Collections.Generic.List<string>();

		foreach (var entry in Located.ParseFile(text))
		{
			if (Array.IndexOf(known, entry.Key) >= 0)
				continue;

			var (line, column) = PlaceOf(text, entry.Where.Offset);

			said.Add($"unknown key '{entry.Key}' at line {line}, column {column}");
		}

		return [.. said];
	}
}
