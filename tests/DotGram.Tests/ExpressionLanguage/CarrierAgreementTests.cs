extern alias immediate;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using DotGram.ExpressionLanguage;
using DotGram.Handwritten;

using ImmediateExpressions = immediate::DotGram.ExpressionLanguage.ExpressionParser;

using Xunit;

namespace DotGram.Tests.ExpressionLanguage;

/// <summary>
/// Two things about how this language answers, over every text the corpus can be cut into: the
/// two carriers answer alike, and nothing a consumer can call throws.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ExpressionRefusalTests"/> records what each refusal SAYS, which is why it walks a
/// sample — every fifth prefix, every seventh deletion — and keeps the words in a file. This
/// walks the whole of it and keeps nothing, because what it asks has one answer either way.
/// </para>
/// <para>
/// The first assertion is about a hazard the grammar guards by design and could stop guarding.
/// A construction runs where the reading is chosen on the tape, and where it is READ on the
/// immediate carrier — so a factory that throws on an alternative the parse later abandons
/// escapes on one carrier and not on the other. §7.5 says the parser never lets such an
/// exception escape. Where the language is ambiguous the grammar asks a guard instead, `Target`
/// and `NamedType` being the two; a construction added to an abandonable alternative without one
/// would show here as a disagreement, and nowhere else.
/// </para>
/// <para>
/// No difference is allowed. One used to be: where the tape refused the text, the immediate
/// carrier could throw instead, because a parse that fails had already run the constructions of
/// what it read before failing — `s.Trim)` built `s.Trim` and was told there is no such property
/// before the `)` refused it. The immediate carrier now holds what a construction throws and
/// drops it with the reading the parse gives up, throwing it only where the tape would have
/// built the value (§7.5); so the two carriers must agree on every text, throwers included,
/// and this test is the proof of that over the whole corpus. That the corpus has throwers at
/// all is asserted beside it, so that agreement is not covering for a corpus nothing throws on:
/// before the change, 654 of these texts threw on the immediate carrier where the tape refused.
/// </para>
/// <para>
/// The second is the contract of the public surface. D137 (2026-09-24) settled that a host's
/// exception propagates from every publication, <c>Try</c> or not; EL's own <c>TryParse</c> is a
/// layer above that and catches its own semantic refusals, and that is what a consumer holding
/// text somebody typed relies on. Nothing here may throw out of it.
/// </para>
/// </remarks>
public sealed class CarrierAgreementTests
{
	[Fact]
	public void The_two_carriers_answer_every_text_alike()
	{
		var differed = new List<string>();
		var threw    = 0;

		foreach (var text in Texts())
		{
			var tape      = Answer(text, ExpressionParser.TryParseLambda);
			var immediate = Answer(text, ImmediateExpressions.Immediate.TryParseLambda);

			if (tape.StartsWith(Threw, StringComparison.Ordinal))
				threw++;

			if (tape != immediate)
				differed.Add($"{Escaped(text)}\n  tape:      {tape}\n  immediate: {immediate}");
		}

		// Not a bound: a corpus that grows grows this. That there are some says the corpus has
		// constructions that throw, so that agreement is not covering for nothing.
		Assert.True(threw > 0, "No text threw on the tape.");

		Assert.True(differed.Count == 0,
			$"{differed.Count} texts told the carriers apart; the first three:\n" +
			string.Join("\n", differed.Take(3)));
	}

	[Fact]
	public void Nothing_a_consumer_can_call_throws()
	{
		var threw = new List<string>();

		foreach (var text in Texts())
		{
			try
			{
				ExpressionParser.TryParse(text, typeof(CarrierAgreementTests).Assembly);
			}
			catch (Exception thrown)
			{
				threw.Add($"{Escaped(text)} — {thrown.GetType().Name}: {thrown.Message.Split('\n')[0]}");
			}
		}

		Assert.True(threw.Count == 0,
			$"TryParse threw on {threw.Count} texts; the first three:\n" + string.Join("\n", threw.Take(3)));
	}

	/// <summary>Every shape, every prefix of one, and every one-character deletion from one.</summary>
	static IEnumerable<string> Texts()
	{
		var seen = new HashSet<string>(StringComparer.Ordinal);

		foreach (var shape in ExpressionCorpus.Shapes)
		{
			if (seen.Add(shape))
				yield return shape;

			for (var length = 1; length <= shape.Length; length++)
				if (seen.Add(shape.Substring(0, length)))
					yield return shape.Substring(0, length);

			for (var at = 0; at < shape.Length; at++)
				if (seen.Add(shape.Remove(at, 1)))
					yield return shape.Remove(at, 1);
		}
	}

	/// <summary>What one carrier made of it: the answer, or the refusal it threw.</summary>
	static string Answer(
		string text, Func<string, ExpressionParser.State, ExpressionParser.Match<LambdaExpression>> read)
	{
		var state = new ExpressionParser.State(typeof(CarrierAgreementTests).Assembly) { Text = text };

		return Caught(() =>
		{
			var match = read(text, state);

			return match.IsSuccess ? Accepted : $"{match.Outcome} at {match.Position}: {match.Error}";
		});
	}

	/// <summary>
	/// The same of the immediate reading, which is the fixture's and so has a state and a match of
	/// its own types: the two meet in the answer, never in a value.
	/// </summary>
	static string Answer(
		string text, Func<string, ImmediateExpressions.State, ImmediateExpressions.Match<LambdaExpression>> read)
	{
		var state = new ImmediateExpressions.State(typeof(CarrierAgreementTests).Assembly) { Text = text };

		return Caught(() =>
		{
			var match = read(text, state);

			return match.IsSuccess ? Accepted : $"{match.Outcome} at {match.Position}: {match.Error}";
		});
	}

	static string Caught(Func<string> answer)
	{
		try
		{
			return answer();
		}
		catch (Exception thrown) when (thrown is FormatException or InvalidOperationException or OverflowException ||
			thrown is ArgumentException and not ArgumentNullException)
		{
			return Threw + thrown.GetType().Name + ": " + thrown.Message;
		}
	}

	const string Accepted = "accepted";

	const string Threw = "threw ";

	/// <summary>The text as a message can carry it, its line breaks and quotes written out.</summary>
	static string Escaped(string input)
	{
		return "\"" + input
			.Replace("\\", "\\\\")
			.Replace("\"", "\\\"")
			.Replace("\n", "\\n")
			.Replace("\r", "\\r") + "\"";
	}
}
