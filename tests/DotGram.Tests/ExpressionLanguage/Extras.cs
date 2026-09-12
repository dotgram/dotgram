using System;

namespace DotGram.Tests.ExpressionLanguage;

/// <summary>Extension methods for the language to find, declared where a text can name them.</summary>
/// <remarks>
/// Internal, and so reachable only because the types of the calling assembly are — which the
/// test assembly is. A text says `using DotGram.Tests.ExpressionLanguage;` and then writes
/// `x.Doubled()`, exactly as C# would.
/// </remarks>
static class Extras
{
	public static int Doubled(this int value) => value * 2;

	public static string Shout(this string text, string mark) => text + mark;

	/// <summary>Never the one chosen: <see cref="Held"/> has a method of its own by that name.</summary>
	/// <remarks>
	/// C# looks for an extension only where no instance method fits, and this is how that is
	/// asked rather than assumed.
	/// </remarks>
	public static int Twice(this Held held) => -1;
}

/// <summary>A type with a method an extension beside it would shadow, if extensions could.</summary>
sealed class Held
{
	public int Twice() => 2;
}
