using System;
using System.Collections.Generic;

using DotGram;

namespace DotGram.Examples;

// Netstrings — the one shape a grammar genuinely cannot express:
//
//     5:hello,7:goodbye,0:,
//
// Each frame says how long it is, and the length decides how much to read next. No
// context-free rule can say that: `['0'..'9']+ & ':' & any{n}` needs `n` to be the
// number that was just matched, and a grammar has no way to carry a value from one
// operand into the shape of the next. The counts of §4.2 are compile-time — `Digits(4)`
// is four because the call site said four — which is a different thing entirely.
//
// So this is what §7.1's second row is for. `@Frame` is a C# method that reads the input
// itself:
//
//     static bool Frame(ReadOnlySpan<char> input, ref int pos)
//
// It is handed the parser's own position and moves it. Everything else stays a grammar:
// the repetition, the end-of-input check, the capture, and the `=>` that turns each
// frame into a value. Only the one step that needs a value from the input to decide a
// length is C#, and it is the smallest such step that could be written.
//
// **The method is trusted absolutely** (§7.1). Nothing checks what it did with `pos`:
// moving it backwards or past the end is a bug in the method, not a case the parser
// defends against — the whole point of the seam is that it costs nothing on the way in.
// So it is written to move the position only when it returns true, and never past the
// end of what it was given.

[Gram("""
	@using DotGram.Examples;

	trivia = none

	Stream : @string[] = Frame* & eof

	// The method consumes `5:hello,` and the capture is the text it covered, so the
	// grammar still holds where each frame is and what it spans.
	Frame : @string = whole: @ReadFrame => @(Payload(whole))

	parse Stream
	""")]
public sealed partial class Netstrings
{
	/// <summary>Reads a whole stream of netstrings into their payloads.</summary>
	public static IReadOnlyList<string> Read(string text) => ParseStream(text);

	/// <summary>
	/// One frame: digits, a colon, that many characters, a comma.
	/// </summary>
	/// <remarks>
	/// The contract of §7.1: answer whether a frame is here, and move <paramref name="pos"/>
	/// past it only if one is. A frame whose length runs past the end of the input is not
	/// a frame — saying so is how a truncated stream fails rather than reading whatever
	/// happens to follow.
	/// </remarks>
	static bool ReadFrame(ReadOnlySpan<char> input, ref int pos)
	{
		var at     = pos;
		var length = 0;
		var digits = 0;

		while (at < input.Length && input[at] >= '0' && input[at] <= '9')
		{
			// A length nobody could mean, and the multiplication that would overflow.
			if (length > (int.MaxValue - 9) / 10)
				return false;

			length = length * 10 + (input[at] - '0');

			at++;
			digits++;
		}

		if (digits == 0 || at >= input.Length || input[at] != ':')
			return false;

		at++;

		if (at + length >= input.Length || input[at + length] != ',')
			return false;

		pos = at + length + 1;

		return true;
	}

	/// <summary>The payload of a frame the grammar matched whole.</summary>
	static string Payload(string frame)
	{
		var colon = frame.IndexOf(':');

		return frame.Substring(colon + 1, frame.Length - colon - 2);
	}
}
