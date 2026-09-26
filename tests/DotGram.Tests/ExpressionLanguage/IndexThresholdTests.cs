using System;
using System.Linq;
using System.Reflection;

using DotGram.ExpressionLanguage;
using DotGram.Handwritten;

using Xunit;

namespace DotGram.Tests.ExpressionLanguage;

/// <summary>
/// A text answers the same whether the names in it were indexed or walked.
/// </summary>
/// <remarks>
/// <para>
/// A reading walks what it has written down until it holds more than
/// <c>State.Indexed</c> declarations and indexes past that (7430d16e), because the indexes cost
/// 640 bytes a parse and <c>(int x) =&gt; x</c> declares one name. That leaves two paths through
/// every question about a name, and every ordinary text lands on ONE of them: almost all of them
/// on the walk, a few long ones on the index. Either could be wrong and nothing else here would
/// say so.
/// </para>
/// <para>
/// So the threshold is a knob on the reading, and each text is read twice — once with it at
/// nought, where the first declaration is indexed, and once at <see cref="int.MaxValue"/>, where
/// nothing ever is. The two answers must be the same string, which is the tree as the API prints
/// it for a debugger, or how and where the text was refused. It is a knob on the READING and not
/// a static, so that two readings at once cannot change each other's answer.
/// </para>
/// </remarks>
public sealed class IndexThresholdTests
{
	static readonly Assembly Caller = typeof(IndexThresholdTests).Assembly;

	/// <summary>Enough declarations to cross the threshold the language ships with.</summary>
	const int Past = 20;

	public static TheoryData<string, string, bool> Texts => new()
	{
		{ "one name", "(int x) => x", true },
		{ "two, and no block", "(int a, int b) => a + b * 2 - a / 3", true },
		{ "a block", "(int x) => { var i = 1; System.Math.Abs(i) + x }", true },
		{ "a name the inner block hides", "() => { int t = 1; int a = { int t = 2; t }; t + a }", true },
		{ "a loop", "(int x) => { for (var i = 0; i < 3; i++) System.Math.Abs(i); x }", true },
		{ "a name that is not there", "(int x) => nowhere", false },

		// Past the threshold, where the index answers instead of the walk.
		{
			"the same name in every block, past the threshold",
			"(int x) => { " + string.Concat(Enumerable.Range(0, Past)
				.Select(one => "{ var i = " + one + "; System.Math.Abs(i); } ")) + "x }",
			true
		},
		{
			"a name apiece, past the threshold",
			"(int x) => { " + string.Concat(Enumerable.Range(0, Past)
				.Select(one => "{ var v" + one + " = " + one + "; System.Math.Abs(v" + one + "); } ")) + "x }",
			true
		},
		{
			"one block declaring them all, past the threshold",
			"(int x) => { " + string.Concat(Enumerable.Range(0, Past)
				.Select(one => "var v" + one + " = " + one + "; ")) + "v" + (Past - 1) + " + x }",
			true
		},
		{
			"a name hidden at every depth, past the threshold",
			"(int x) => { var t = 0; { var t = 1; { var t = 2; { var t = 3; { var t = 4; " +
				"System.Math.Abs(t); } System.Math.Abs(t); } System.Math.Abs(t); } System.Math.Abs(t); } t + x }",
			true
		},

		{ "crossing the threshold inside a branch that is read twice", Crossing, true },
	};

	[Theory]
	[MemberData(nameof(Texts))]
	public void A_text_answers_the_same_indexed_as_walked(string what, string text, bool read)
	{
		var indexed = Answer(text, 0);
		var walked  = Answer(text, int.MaxValue);

		Assert.True(indexed == walked, $"Indexed and walked disagree about {what}.\nindexed:\n{indexed}\nwalked:\n{walked}");

		// Or the pair above agrees about a text neither of them reads, and says nothing.
		Assert.True(
			read == !(indexed.StartsWith("refused", StringComparison.Ordinal) ||
				indexed.StartsWith("threw", StringComparison.Ordinal)),
			$"The text for {what} was expected to be {(read ? "read" : "refused")}, and the answer is:\n{indexed}");
	}

	/// <summary>
	/// A text whose declarations pass the threshold inside a reading that is then given up.
	/// </summary>
	/// <remarks>
	/// The parameter and fifteen locals stand before the <c>if</c>, so the sixteenth declaration —
	/// the one inside the branch — is where a reading crosses over and builds the indexes out of
	/// what it has written. An <c>if</c> with no <c>else</c> reads its branch as each of its two
	/// forms, so that crossing happens inside a reading the parse gives up and takes back, and the
	/// branch is read again with the indexes already standing. It is the one place where the two
	/// paths meet in one text, and the place where a rule about what a rollback leaves behind is
	/// answered by an index built before it rather than by the walk.
	/// </remarks>
	const string Crossing =
		"(int x) => { var a0 = 0; var a1 = 1; var a2 = 2; var a3 = 3; var a4 = 4; var a5 = 5; " +
		"var a6 = 6; var a7 = 7; var a8 = 8; var a9 = 9; var a10 = 10; var a11 = 11; var a12 = 12; " +
		"var a13 = 13; var a14 = 14; " +
		"if (x > 0) { var t = 1; System.Math.Abs(t); } " +
		"a14 + x }";

	[Fact]
	public void A_text_that_crosses_the_threshold_where_a_reading_is_given_up_still_answers()
	{
		// At the threshold the language ships with, which is the whole point of this one: it is
		// read the way a consumer's text is read, not with the knob turned to either end.
		Assert.Equal(17, Both.Compile<Func<int, int>>(Crossing)(3));

		// And it really does cross it. Without this the text is one more ordinary fixture the
		// moment somebody raises the threshold, and it would go on passing while testing nothing.
		var state = new ExpressionParser.State(Caller) { Text = Crossing };

		Assert.True(ExpressionParser.TryParseLambda(Crossing, state).IsSuccess);
		Assert.NotNull(Indexes(state));

		// And a text that stays under it does not, which is the other half of the same check.
		var under = new ExpressionParser.State(Caller) { Text = "(int x) => x" };

		Assert.True(ExpressionParser.TryParseLambda("(int x) => x", under).IsSuccess);
		Assert.Null(Indexes(under));
	}

	/// <summary>Whether a reading built the indexes, read the way a measurement reads one.</summary>
	/// <remarks>
	/// By reflection and on purpose, as <see cref="VocabularyTests"/> reads the caches: what is
	/// asserted is that a reading crossed over, and a field a consumer could see would be a field a
	/// consumer could come to depend on.
	/// </remarks>
	static object? Indexes(ExpressionParser.State state)
	{
		return typeof(ExpressionParser.State)
			.GetField("_byPlace", BindingFlags.NonPublic | BindingFlags.Instance)!
			.GetValue(state);
	}

	/// <summary>The tree a reading of that text builds, or how and where it refused it.</summary>
	static string Answer(string text, int indexed)
	{
		return ExpressionCorpus.Answer(
			text,
			ExpressionParser.TryParseLambda,
			new ExpressionParser.State(Caller) { Text = text, Indexed = indexed });
	}
}
