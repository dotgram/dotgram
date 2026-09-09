using System;
using System.IO;
using System.Linq;

using DotGram;

namespace DotGram.Examples.Formats;

// A path, split into names that the platform would actually accept:
//
//     reports/2026/september.txt      →  reports | 2026 | september.txt
//     bad:name                        →  no match
//     CON                             →  no match
//
// The interesting part is the set of characters a name may be made of. It is not a range
// and cannot be written as one: what is forbidden is whatever `Path.GetInvalidFileNameChars`
// says today, on this platform — data rather than syntax. So the grammar asks C#:
//
//     Segment = t: [@IsAllowed]+
//
// `[@M]` inside an element set is a **predicate over one character**, `bool M(char c)`.
// It is a member of the set like a range or a literal is, and can stand beside them —
// `[@IsAllowed | '-']` is a perfectly ordinary set. The generator never looks at the
// method to decide that; the position does (docs/syntax.md §7.1): inside brackets it is
// `M(c)`, and a bare `@M` outside them would be an external recognizer with a different
// signature entirely.
//
// That is the whole rule for the C# seam, and it is worth learning once:
//
//     [@M]                  bool M(char c)                                     — a set member
//     @M                    bool M(ReadOnlySpan<char> input, ref int pos)      — a recognizer
//     when @M(x)            any bool                                           — a guard
//     => @M(x)              any value                                          — a construction
//
// The reserved names are a guard rather than a set, because "not CON" is a question about
// the whole segment and a set is asked one character at a time. Which is the other half
// of the lesson: a set decides what may appear, a guard decides what it may have added up
// to. `NetstringExample` is the third of the three — a length that decides how much to
// read next, which neither a set nor a guard can express and an external recognizer can.

[Gram("""
	using Std;

	trivia = none

	// The set is a method, and the guard is another: one asks about a character, the
	// other about what they came to.
	Segment : @string = t: [@IsAllowed]+ & when @(NotReserved(t!)) => @(t)

	Route : @string[] = Segment & ('/' & Segment)* & eof

	parse Route
	parse Segment
	""")]
public static partial class FileNames
{
	static readonly char[] Invalid = Path.GetInvalidFileNameChars();

	/// <summary>The names Windows keeps for itself, whatever the extension.</summary>
	static readonly string[] Reserved = ["CON", "PRN", "AUX", "NUL", "COM1", "LPT1"];

	/// <summary>Whether a character may stand in a name — asked once per character.</summary>
	static bool IsAllowed(char c) => Array.IndexOf(Invalid, c) < 0 && c != '/';

	/// <summary>Whether the name they added up to is one somebody may use.</summary>
	static bool NotReserved(string segment) => !Reserved.Contains(segment.ToUpperInvariant());

	/// <summary>Whether a path is one this platform would accept, in full.</summary>
	public static bool IsUsable(string path) => TryParseRoute(path).IsSuccess;
}
