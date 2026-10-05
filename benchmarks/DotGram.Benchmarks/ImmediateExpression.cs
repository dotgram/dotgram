extern alias immediate;

using System;
using System.Linq.Expressions;
using System.Reflection;

using DotGram.Handwritten;

using ImmediateExpressions = immediate::DotGram.ExpressionLanguage.ExpressionParser;

namespace DotGram.Benchmarks;

/// <summary>
/// The expression language's immediate reading, which the package does not carry: it is read from
/// the fixture that compiles the language again with it (tests/DotGram.ExpressionLanguage.Immediate).
/// </summary>
/// <remarks>
/// Every type of the language is in that assembly a second time, under the same full name, so the
/// immediate reading takes a state of the fixture's own and answers a match of the fixture's own:
/// nothing of one assembly is handed to the other, and the readings meet only in their answers. The
/// static caches the reading fills are the fixture's too — a first call of it is a first call of
/// that assembly, whatever the package's own reading has already done in the process.
/// </remarks>
static class ImmediateExpression
{
	/// <summary>Whether it read the text, with a state of its own on behalf of <paramref name="caller"/>, as a caller's parse would have.</summary>
	internal static bool Reads(string text, Assembly caller)
	{
		return ImmediateExpressions.Immediate.TryParseLambda(text, new ImmediateExpressions.State(caller) { Text = text }).IsSuccess;
	}

	/// <summary>What it made of the text, put as <see cref="ExpressionCorpus.Answer(Func{string})"/> puts every reading's answer.</summary>
	internal static string Answer(string text, Assembly caller)
	{
		var state = new ImmediateExpressions.State(caller) { Text = text };

		return ExpressionCorpus.Answer(() =>
		{
			var match = ImmediateExpressions.Immediate.TryParseLambda(text, state);

			return match.IsSuccess
				? ExpressionCorpus.Shown(match.Value!)
				: ExpressionCorpus.Refusal(match.Outcome, match.Position, state.RefusedAt, state.Refused());
		});
	}
}
