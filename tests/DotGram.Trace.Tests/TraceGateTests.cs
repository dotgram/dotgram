using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace DotGram.Trace.Tests;

/// <summary>
/// A trace build of the shipped grammars, held to the libraries themselves over corpora of
/// refused input: it answers what they answer, its explanation says what their message says, and
/// it names every rule a parser calling its forwarding rules as written would name.
/// </summary>
public sealed class TraceGateTests(ITestOutputHelper output)
{
	public static TheoryData<string> Names => new(Targets.All.Select(static one => one.Name));

	/// <summary>
	/// The trace build with a sink attached answers every refused input as the library does, word
	/// for word and at the same position: nothing a trace adds changes what is read.
	/// </summary>
	[Theory]
	[MemberData(nameof(Names))]
	public void A_trace_build_answers_as_the_library_does(string name)
	{
		var target = Targets.Named(name);
		var rows   = target.Refused;
		var wrong  = new List<string>();

		Assert.NotEmpty(rows);

		foreach (var text in rows.Concat(target.Seeds))
		{
			var plain  = target.Plain(text);
			var traced = target.Traced(text).Answer;

			if (plain != traced)
				wrong.Add($"{Shown(text)}: {plain} / {traced}");
		}

		output.WriteLine($"{name}: {rows.Count} refused, {target.Seeds.Count} accepted, {wrong.Count} answered otherwise");

		Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong.Take(20)));
	}

	/// <summary>
	/// <c>GramWhy</c>'s message is the match's message and its position the match's, on every row,
	/// and every row is explained: by the rules reading where it was refused, by a guard, or by a
	/// character no token begins with.
	/// </summary>
	[Theory]
	[MemberData(nameof(Names))]
	public void Every_refusal_is_explained_in_the_words_of_its_message(string name)
	{
		var target   = Targets.Named(name);
		var wrong    = new List<string>();
		var causes   = new Dictionary<string, int>(StringComparer.Ordinal);
		var withPath = 0;

		var thrown   = 0;

		foreach (var text in target.Refused)
		{
			var why = target.Traced(text);

			// Read, and then refused by a construction rather than by the grammar: nothing to explain.
			if (why.Answer.Thrown)
			{
				thrown++;

				continue;
			}

			causes[why.Cause] = causes.TryGetValue(why.Cause, out var seen) ? seen + 1 : 1;

			if (why.Stacks.Length > 0)
				withPath++;

			if (!why.Refused || why.Message != why.Answer.Error || why.Position != why.Answer.Position)
				wrong.Add($"{Shown(text)}: match '{why.Answer.Error}' at {why.Answer.Position}, why '{why.Message}' at {why.Position}");
			else if (why.Cause.Length == 0 || why.Stacks.Length == 0 && !Unread(why.Cause))
				wrong.Add($"{Shown(text)}: '{why.Message}' unexplained: {why.Cause}");
		}

		output.WriteLine($"{name}: {target.Refused.Count} refused, {thrown} of them by a construction that threw, {withPath} with the rules reading there");

		foreach (var cause in causes.OrderByDescending(static one => one.Value))
			output.WriteLine($"  {cause.Value}\t{cause.Key}");

		Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong.Take(20)));

		static bool Unread(string cause)
		{
			return cause.Contains("token", StringComparison.Ordinal);
		}
	}

	/// <summary>
	/// Every rule a parser that calls its forwarding rules as written has on the stacks that explain
	/// a refusal, the trace build has on its own: the frames a forwarding rule leaves where it was
	/// collapsed are the frames it would have had. Held where the two answer alike, which is nearly
	/// everywhere — the two are different parsers, and the order of an expected list can differ.
	/// </summary>
	[Theory]
	[MemberData(nameof(Names))]
	public void A_collapsed_forwarding_rule_leaves_no_frame_missing(string name)
	{
		var target  = Targets.Named(name);
		var missing = new List<string>();
		var held    = 0;
		var frames  = 0;

		foreach (var text in target.Refused)
		{
			var traced   = target.Traced(text);
			var unfolded = target.Unfolded(text);

			if (traced.Answer.Thrown || traced.Message != unfolded.Message || traced.Position != unfolded.Position)
				continue;

			held++;

			var named = new HashSet<string>(traced.Stacks.SelectMany(static stack => stack), StringComparer.Ordinal);

			foreach (var rule in unfolded.Stacks.SelectMany(static stack => stack).Distinct(StringComparer.Ordinal))
			{
				frames++;

				if (!named.Contains(rule))
					missing.Add($"{Shown(text)}: {rule}");
			}
		}

		output.WriteLine($"{name}: {held} of {target.Refused.Count} refusals answered alike, {frames} rules on their stacks, {missing.Count} missing");

		Assert.True(missing.Count == 0, string.Join(Environment.NewLine, missing.Take(30)));
	}

	/// <summary>
	/// Every rule entered is left, innermost first, on every row, and every rule the engine retracts
	/// is one that read something: a return the emitter wrote without its exit, or an entry of the
	/// engine's arena taken for another's, would show here.
	/// </summary>
	[Theory]
	[MemberData(nameof(Names))]
	public void Every_rule_entered_is_left(string name)
	{
		var target  = Targets.Named(name);
		var wrong   = new List<string>();
		var enters  = 0L;
		var retract = 0L;

		foreach (var text in target.Refused.Concat(target.Seeds))
		{
			var (answer, tally) = target.Counted(text);

			// A construction that threw leaves its rules entered, which a sink is to tolerate.
			if (answer.Thrown)
				continue;

			enters  += tally.Enters;
			retract += tally.Retractions;

			if (tally.Enters != tally.Exits || tally.Unmatched != 0)
				wrong.Add($"{Shown(text)}: {tally.Enters} entered, {tally.Exits} left, {tally.Unmatched} out of order");
		}

		output.WriteLine($"{name}: {enters} rules entered, {retract} retracted");

		Assert.True(enters > 0);
		Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong.Take(20)));
	}

	static string Shown(string text)
	{
		var shown = text.Length > 80 ? text.Substring(0, 80) + "..." : text;

		return "\"" + shown.Replace("\r", "\\r").Replace("\n", "\\n") + "\"";
	}
}
