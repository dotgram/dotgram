using System;
using System.Collections.Generic;

namespace DotGram.Tests.ExpressionLanguage;

/// <summary>Extension methods for the language to find, declared where a text can name them.</summary>
/// <remarks>
/// Internal, and so reachable only because the types of the calling assembly are — which the
/// test assembly is. A text says `using DotGram.Tests.ExpressionLanguage;` and then writes
/// `x.Doubled()`, exactly as C# would.
/// </remarks>
static class Extras
{
	public static int Doubled(this int value)
	{
		return value * 2;
	}

	public static string Shout(this string text, string mark)
	{
		return text + mark;
	}

	/// <summary>Never the one chosen: <see cref="Held"/> has a method of its own by that name.</summary>
	/// <remarks>
	/// C# looks for an extension only where no instance method fits, and this is how that is
	/// asked rather than assumed.
	/// </remarks>
	public static int Twice(this Held held)
	{
		return -1;
	}

	/// <summary>The pair that says which of two applicable methods is the better one.</summary>
	/// <remarks>
	/// Both apply to an <c>int</c>, and C# takes the one whose type arguments it did not have
	/// to infer. Anything else takes the generic one, there being nothing else to take.
	/// </remarks>
	public static string Kind(this int value)
	{
		return "int";
	}

	public static string Kind<T>(this T value)
	{
		return "any";
	}

	/// <summary>An extension no <c>string</c> can call, the constraint being what says so.</summary>
	public static int Sized<T>(this T value) where T : struct
	{
		return 1;
	}
}

/// <summary>A type with a method an extension beside it would shadow, if extensions could.</summary>
sealed class Held
{
	public int Twice()
	{
		return 2;
	}
}

/// <summary>Generic methods that say what was inferred for them, so the compiler can be asked.</summary>
/// <remarks>
/// Each answers with the name of its own type argument, which is what the compiler worked
/// out — so a test calls the method in ordinary C# to learn what C# infers, and asks the
/// resolver the same question.
/// </remarks>
static class Inferring
{
	/// <summary>One type parameter bound twice, which is what fixing is for (§12.6.3).</summary>
	public static string Both<T>(T first, T second)
	{
		return typeof(T).Name;
	}

	/// <summary>Bound through an array and directly, which must agree.</summary>
	public static string Array<T>(T[] values, T one)
	{
		return typeof(T).Name;
	}

	/// <summary>Bound through an interface the argument implements.</summary>
	public static string Sequence<T>(IEnumerable<T> values)
	{
		return typeof(T).Name;
	}

	/// <summary>Bound once, or not at all where the argument has no type of its own.</summary>
	public static string One<T>(T only)
	{
		return typeof(T).Name;
	}
}

/// <summary>Overloads whose choosing the C# compiler itself can be asked about.</summary>
/// <remarks>
/// Each pair is one rule of §12.6.4, and each overload answers with what tells it from the
/// other. A test calls the pair in ordinary C# — where the compiler chooses — and asks the
/// resolver the same question, so what is asserted is that the two agree. Written this way
/// because a test that encodes my reading of the specification proves only that I read it
/// the same way twice.
/// </remarks>
static class Choosing
{
	/// <summary>An exact match against a widening (§12.6.4.4).</summary>
	public static string Near(int value)
	{
		return "Int32";
	}

	public static string Near(long value)
	{
		return "Int64";
	}

	/// <summary>Neither exact, and one target converts to the other (§12.6.4.6).</summary>
	public static string Wider(long value)
	{
		return "Int64";
	}

	public static string Wider(double value)
	{
		return "Double";
	}

	/// <summary>Neither target converts to the other, and one of them is signed.</summary>
	public static string Signed(int value)
	{
		return "Int32";
	}

	public static string Signed(uint value)
	{
		return "UInt32";
	}

	/// <summary>One leaves nothing to a default, the other leaves one.</summary>
	public static string Filled(int first)
	{
		return "1";
	}

	public static string Filled(int first, int second = 5)
	{
		return "2";
	}

	/// <summary>One applies in its normal form, the other only expanded.</summary>
	public static string Spread(int first, int second)
	{
		return "normal";
	}

	public static string Spread(int first, params int[] rest)
	{
		return "expanded";
	}

	/// <summary>A conversion the author wrote against one the language has (§12.6.4.6).</summary>
	public static string Given(Money value)
	{
		return "Money";
	}

	public static string Given(decimal value)
	{
		return "Decimal";
	}

	/// <summary>
	/// A lambda with no types, handed to two delegates that take the same and give back
	/// different things: the body worth exactly one of them chooses it (§12.6.4.5).
	/// </summary>
	public static string Measured(Func<string, int> measure)
	{
		return "Int32";
	}

	public static string Measured(Func<string, long> measure)
	{
		return "Int64";
	}

	/// <summary>Neither is exact, and one return type converts to the other.</summary>
	public static string Widened(Func<string, long> measure)
	{
		return "Int64";
	}

	public static string Widened(Func<string, double> measure)
	{
		return "Double";
	}

	/// <summary>One gives something back and the other gives nothing.</summary>
	public static string Kept(Func<string, string> trim)
	{
		return "String";
	}

	public static string Kept(Action<string> trim)
	{
		return "Void";
	}

	/// <summary>A body that cannot be what one of them gives back leaves that one out.</summary>
	public static string Counted(Func<string, bool> count)
	{
		return "Boolean";
	}

	public static string Counted(Func<string, int> count)
	{
		return "Int32";
	}

	/// <summary>
	/// Two generic methods that become the same once inferred, one written more specifically
	/// than the other (§12.6.4.3) — the shape of `Max(Func&lt;TSource, double&gt;)` beside
	/// `Max&lt;TSource, TResult&gt;(Func&lt;TSource, TResult&gt;)`.
	/// </summary>
	public static string Specific<T>(T value, Func<T, double> map)
	{
		return "double";
	}

	public static string Specific<T, TResult>(T value, Func<T, TResult> map)
	{
		return "any";
	}
}

/// <summary>A type with a conversion of its own, for the rule about conversions of one's own.</summary>
readonly struct Money(decimal amount)
{
	public decimal Amount { get; } = amount;

	public static implicit operator Money(int value) => new(value);

	public static implicit operator decimal(Money value) => value.Amount;
}

/// <summary>Overloads written to be chosen between, for asking the resolver without a text.</summary>
/// <remarks>
/// Each pair is one question C#'s overload resolution answers: which is better where both
/// apply, what a <c>params</c> tail does, what an omitted default becomes, and what the
/// literal <c>null</c> can be handed to.
/// </remarks>
static class Choices
{
	/// <summary>Neither is better: each converts one argument exactly and widens the other.</summary>
	public static int Two(int first, long second)
	{
		return 1;
	}

	public static int Two(long first, int second)
	{
		return 2;
	}

	/// <summary>A <c>params</c> tail, which is read in the expanded form or as an array.</summary>
	public static int Sum(params int[] values)
	{
		return values.Length;
	}

	/// <summary>An optional parameter, which is a default where it is left out.</summary>
	public static int Some(int first, int second = 5)
	{
		return first + second;
	}

	/// <summary>What the literal <c>null</c> can be handed to, and what it cannot.</summary>
	public static string Took(string text)
	{
		return "string";
	}

	public static string Took(int number)
	{
		return "int";
	}
}

/// <summary>What takes an interpolated string as more than a string, and what C# hands it.</summary>
static class Formats
{
	/// <summary>Both apply to an interpolated string, and C# takes the string.</summary>
	public static string Kind(string text)
	{
		return "string";
	}

	public static string Kind(FormattableString text)
	{
		return "formattable";
	}

	/// <summary>What the formattable string was made of.</summary>
	public static string Shape(FormattableString text)
	{
		return text.Format + "|" + text.ArgumentCount;
	}

	/// <summary>An interpolated string formatted later, in a culture of the caller's choosing.</summary>
	public static string Invariant(IFormattable text)
	{
		return text.ToString(null, System.Globalization.CultureInfo.InvariantCulture);
	}
}
