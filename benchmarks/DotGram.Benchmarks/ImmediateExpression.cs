using System;
using System.Linq.Expressions;
using System.Reflection;

using DotGram.ExpressionLanguage;
using DotGram.Handwritten;

namespace DotGram.Benchmarks;

/// <summary>
/// The expression language's immediate reading, which is the package's own: what a scope is given.
/// </summary>
/// <remarks>
/// Its tape reading is the fixture's (<see cref="TapeExpression"/>); this one is here beside it so
/// that every row reads the two through calls of the same shape, each with a state of its own.
/// </remarks>
static class ImmediateExpression
{
	/// <summary>Whether it read the text, with a state of its own on behalf of <paramref name="caller"/>, as a caller's parse would have.</summary>
	internal static bool Reads(string text, Assembly caller)
	{
		return ExpressionParser.TryParseLambda(text, new ExpressionParser.State(caller) { Text = text }).IsSuccess;
	}

	/// <summary>What it made of the text, put as <see cref="ExpressionCorpus.Answer(Func{string})"/> puts every reading's answer.</summary>
	internal static string Answer(string text, Assembly caller)
	{
		var state = new ExpressionParser.State(caller) { Text = text };

		return ExpressionCorpus.Answer(() =>
		{
			var match = ExpressionParser.TryParseLambda(text, state);

			return match.IsSuccess
				? ExpressionCorpus.Shown(match.Value!)
				: ExpressionCorpus.Refusal(match.Outcome, match.Position, state.RefusedAt, state.Refused());
		});
	}
}
