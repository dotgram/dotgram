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
//     [GramOptions(LocationType = typeof(ILocated), Suffix = "Located")]
//                                                                    Config.Located.ParseFile
//
// `[Gram]` says which grammar, which a class has one of. `[GramOptions]` may be written
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
	public void Locate(int at, int length) => Where = new At(at, length);
}

[Gram("""
	@using DotGram.Examples.Formats;

	using Std;

	trivia = { (Spacing | LineComment("#"))* }

	File : @Entry[] = Entry* & eof

	Entry : @Entry = key: Identifier & '=' & value: Value => @(new Entry(key, value))

	Value : @string = t: [^ '\r' | '\n']* => @(t)

	// No `=>` and no captures, so this rule is what it matched — as two numbers rather
	// than as text (§4.1 case 4).
	KeySpan : @SourceSpan = Identifier

	parse File
	parse KeySpan
	""")]
[GramOptions(LocationType = typeof(ILocated), Suffix = "Located")]
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
