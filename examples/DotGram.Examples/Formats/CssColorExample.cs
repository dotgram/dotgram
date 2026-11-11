using DotGram;

namespace DotGram.Examples.Formats;

// The repository README's first example, as a file: a grammar the size of a regular
// expression, and the two calls the README makes against it. A test holds the README's
// grammar to this one and runs the calls, so the first code a reader copies cannot stop
// working unnoticed.
//
// The regular expression it stands for is ^#(?<value>[0-9a-fA-F]{6})$ — and `value:`
// becomes a property of the generated result type rather than a group to look up by name.

[Gram("""
	Hex   = ['0'..'9' | 'a'..'f' | 'A'..'F']
	Color = '#' & value: Hex{6}

	parse Color
	""")]
public static partial class CssColor
{
	// ParseColor, TryParseColor and the result type with its Value are generated into this
	// class. These two are the README's two calls, kept as methods so a test can run them.

	/// <summary>The six digits of a colour; throws when the text is not one.</summary>
	public static string DigitsOf(string text)
	{
		return ParseColor(text).Value;
	}

	/// <summary>Whether the whole text is a colour.</summary>
	public static bool IsColor(string text)
	{
		return TryParseColor(text).IsSuccess;
	}
}
