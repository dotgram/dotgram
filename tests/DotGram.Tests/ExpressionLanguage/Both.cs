using System;
using System.Linq.Expressions;
using System.Reflection;

using DotGram.ExpressionLanguage;
using DotGram.Handwritten;

using Xunit;

namespace DotGram.Tests.ExpressionLanguage;

/// <summary>
/// Both readings of the expression language at once: the generated <see cref="ExpressionParser"/>
/// and the handwritten <see cref="HandExpression"/>, which must answer the same.
/// </summary>
/// <remarks>
/// <para>
/// The handwritten parser is what the generated one is measured against, and a ratio between two
/// parsers that read different languages says nothing (docs/design/architecture-decisions.md, D1).
/// So the suite reads through here: every text <see cref="ExpressionParserTests"/> hands the
/// language is put to both, and a text that tells them apart fails where it stands rather than in
/// a benchmark nobody ran.
/// </para>
/// <para>
/// What comes back is the generated parser's own answer, so the assertions after the call are
/// about the parser the tests were written for. The handwritten one is held to it, not the other
/// way round, and to the tape, which is what ships.
/// </para>
/// </remarks>
static class Both
{
	static readonly Assembly Caller = typeof(Both).Assembly;

	/// <summary>The two answering the same about a text: the tree, or how and where it was refused.</summary>
	public static void Agree(string text, Assembly caller, bool ascii = false, bool withText = true)
	{
		var generated = ExpressionCorpus.Answer(
			text, ascii ? ExpressionParser.TryParseAsciiLambda : ExpressionParser.TryParseLambda, State(text, caller, withText));
		var hand = ExpressionCorpus.Answer(
			text, ascii ? HandExpression.TryParseAsciiLambda : HandExpression.TryParseLambda, State(text, caller, withText));

		if (generated != hand)
			Assert.Fail(
				$"The generated parser and the hand-written one disagree about \"{text}\"" +
				(ascii ? " read in ASCII" : "") + ".\n" +
				$"generated:\n{generated}\n" +
				$"by hand:\n{hand}");
	}

	public static void Agree(string text)
	{
		Agree(text, Caller);
	}

	public static LambdaExpression Parse(string text)
	{
		Agree(text);

		return ExpressionParser.Parse(text, Caller);
	}

	public static LambdaExpression Parse(string text, Assembly caller)
	{
		Agree(text, caller);

		return ExpressionParser.Parse(text, caller);
	}

	public static ExpressionParser.Match<LambdaExpression> TryParse(string text)
	{
		Agree(text);

		return ExpressionParser.TryParse(text, Caller);
	}

	public static ExpressionParser.Match<LambdaExpression> TryParse(string text, Assembly caller)
	{
		Agree(text, caller);

		return ExpressionParser.TryParse(text, caller);
	}

	public static TDelegate Compile<TDelegate>(string text)
		where TDelegate : Delegate
	{
		Agree(text);

		return ExpressionParser.Compile<TDelegate>(text, Caller);
	}

	public static TDelegate Compile<TDelegate>(string text, Assembly caller)
		where TDelegate : Delegate
	{
		Agree(text, caller);

		return ExpressionParser.Compile<TDelegate>(text, caller);
	}

	public static ExpressionParser.Match<LambdaExpression> TryParseLambda(string text, ExpressionParser.State state)
	{
		Agree(text, state.Caller, withText: state.Text is not null);

		return ExpressionParser.TryParseLambda(text, state);
	}

	public static ExpressionParser.Match<LambdaExpression> TryParseAsciiLambda(string text, ExpressionParser.State state)
	{
		Agree(text, state.Caller, ascii: true, withText: state.Text is not null);

		return ExpressionParser.TryParseAsciiLambda(text, state);
	}

	static ExpressionParser.State State(string text, Assembly caller, bool withText)
	{
		return new ExpressionParser.State(caller) { Text = withText ? text : null };
	}
}
