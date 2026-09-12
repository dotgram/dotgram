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

	/// <summary>The pair that says which of two applicable methods is the better one.</summary>
	/// <remarks>
	/// Both apply to an <c>int</c>, and C# takes the one whose type arguments it did not have
	/// to infer. Anything else takes the generic one, there being nothing else to take.
	/// </remarks>
	public static string Kind(this int value) => "int";

	public static string Kind<T>(this T value) => "any";

	/// <summary>An extension no <c>string</c> can call, the constraint being what says so.</summary>
	public static int Sized<T>(this T value) where T : struct => 1;
}

/// <summary>A type with a method an extension beside it would shadow, if extensions could.</summary>
sealed class Held
{
	public int Twice() => 2;
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
	public static string Near(int value) => "Int32";

	public static string Near(long value) => "Int64";

	/// <summary>Neither exact, and one target converts to the other (§12.6.4.6).</summary>
	public static string Wider(long value) => "Int64";

	public static string Wider(double value) => "Double";

	/// <summary>Neither target converts to the other, and one of them is signed.</summary>
	public static string Signed(int value) => "Int32";

	public static string Signed(uint value) => "UInt32";

	/// <summary>One leaves nothing to a default, the other leaves one.</summary>
	public static string Filled(int first) => "1";

	public static string Filled(int first, int second = 5) => "2";

	/// <summary>One applies in its normal form, the other only expanded.</summary>
	public static string Spread(int first, int second) => "normal";

	public static string Spread(int first, params int[] rest) => "expanded";

	/// <summary>A conversion the author wrote against one the language has (§12.6.4.6).</summary>
	public static string Given(Money value) => "Money";

	public static string Given(decimal value) => "Decimal";
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
	public static int Two(int first, long second) => 1;

	public static int Two(long first, int second) => 2;

	/// <summary>A <c>params</c> tail, which is read in the expanded form or as an array.</summary>
	public static int Sum(params int[] values) => values.Length;

	/// <summary>An optional parameter, which is a default where it is left out.</summary>
	public static int Some(int first, int second = 5) => first + second;

	/// <summary>What the literal <c>null</c> can be handed to, and what it cannot.</summary>
	public static string Took(string text) => "string";

	public static string Took(int number) => "int";
}
