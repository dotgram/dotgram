using System;
using System.Collections.Generic;

namespace DotGram.Tests;

/// <summary>
/// Delta debugging: the smallest text that still does the interesting thing.
/// </summary>
/// <remarks>
/// <para>
/// Twice in this repository a defect arrived as a large grammar and left as a dozen
/// characters — a materializer crash shrank to <c>A=x&lt;&lt;1=&gt;@()</c>, a hang to a
/// four-line grammar — and both times the shrinker was written on the spot and thrown
/// away. This is that tool, kept.
/// </para>
/// <para>
/// The predicate says what "interesting" means, and writing it well is the whole craft:
/// a shrink hunting a wrong answer must assert the answer is <em>still wrong the same
/// way</em>, or it will happily converge on a text that fails for a fresh reason. The
/// hang shrank to nonsense on its first run for exactly that — the predicate said
/// "fails" when it needed to say "reads two but refuses three".
/// </para>
/// <para>
/// A predicate that may hang belongs behind <see cref="Within"/>, which turns a budget
/// into a verdict of "not interesting" — a shrinker hunting a hang would otherwise sit
/// inside one.
/// </para>
/// </remarks>
public static class Shrink
{
	/// <summary>
	/// The fewest lines that keep <paramref name="interesting"/> true.
	/// </summary>
	/// <remarks>
	/// The right granularity for a grammar, where a line is a declaration and dropping
	/// half of one is just a syntax error the predicate has to reject anyway.
	/// </remarks>
	public static string Lines(string text, Func<string, bool> interesting)
	{
		if (text is null)        throw new ArgumentNullException(nameof(text));
		if (interesting is null) throw new ArgumentNullException(nameof(interesting));
		if (!interesting(text))  throw new ArgumentException("The starting text is not interesting.", nameof(text));

		var lines   = new List<string>(text.Split('\n'));
		var changed = true;

		while (changed)
		{
			changed = false;

			for (var i = lines.Count - 1; i >= 0; i--)
			{
				var without = new List<string>(lines);

				without.RemoveAt(i);

				if (!interesting(string.Join("\n", without)))
					continue;

				lines   = without;
				changed = true;
			}
		}

		return string.Join("\n", lines);
	}

	/// <summary>
	/// The fewest characters that keep <paramref name="interesting"/> true.
	/// </summary>
	/// <remarks>
	/// The right granularity for an input, and a finisher for a grammar a
	/// <see cref="Lines"/> pass has already cut down.
	/// </remarks>
	public static string Chars(string text, Func<string, bool> interesting)
	{
		if (text is null)        throw new ArgumentNullException(nameof(text));
		if (interesting is null) throw new ArgumentNullException(nameof(interesting));
		if (!interesting(text))  throw new ArgumentException("The starting text is not interesting.", nameof(text));

		var changed = true;

		while (changed)
		{
			changed = false;

			// Halves and quarters before single characters: a text that is mostly
			// removable goes in a few probes instead of one per character.
			for (var chunk = Math.Max(1, text.Length / 2); chunk >= 1; chunk /= 2)
			{
				for (var at = text.Length - chunk; at >= 0; at -= chunk)
				{
					var without = text.Remove(at, chunk);

					if (!interesting(without))
						continue;

					text    = without;
					changed = true;
				}
			}
		}

		return text;
	}

	/// <summary>
	/// A predicate wrapped in a time budget: slower than the budget is not interesting.
	/// </summary>
	/// <remarks>
	/// For hunting everything except a hang, where the polarity flips —
	/// there, "took the whole budget" is the interesting thing, and the caller writes
	/// that predicate directly with a thread or a process of their own.
	/// </remarks>
	public static Func<string, bool> Within(TimeSpan budget, Func<string, bool> interesting)
	{
		if (interesting is null)
			throw new ArgumentNullException(nameof(interesting));

		return text =>
		{
			var task = System.Threading.Tasks.Task.Run(() => interesting(text));

			return task.Wait(budget) && task.Result;
		};
	}
}
