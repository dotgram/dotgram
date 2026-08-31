using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;

namespace DotGram.VisualStudio;

internal enum DslRecognitionStatus
{
	Success,
	Failure,
	Unsupported,
}

internal readonly record struct DslRecognitionExtent(
	RuleSymbol Rule,
	string? Capture,
	int Position,
	int Length);

internal sealed class DslRecognitionResult(
	DslRecognitionStatus status,
	int failurePosition,
	IReadOnlyList<DslRecognitionExtent> extents,
	IReadOnlyList<string> expected)
{
	public DslRecognitionStatus Status { get; } = status;
	public int FailurePosition { get; } = failurePosition;
	public IReadOnlyList<DslRecognitionExtent> Extents { get; } = extents;
	public IReadOnlyList<string> Expected { get; } = expected;
}

/// <summary>
/// Supplies recognition facts which cannot be derived from a normalized grammar alone.
/// Implementations describe tooling-safe behavior; they must not invoke generated parser
/// methods or consumer code.
/// </summary>
internal interface IDslRecognitionContract
{
	bool TryEvaluateGuard(Node.Guard guard, RuleSymbol owner, int position, out bool accepted);

	bool TryResolveExternal(
		Node.External external,
		RuleSymbol owner,
		out RuleSymbol rule);
}

internal sealed class DslDescriptorRecognitionContract : IDslRecognitionContract
{
	readonly IReadOnlyDictionary<string, bool> _guards;
	readonly IReadOnlyDictionary<string, RuleSymbol> _externals;

	public DslDescriptorRecognitionContract(
		RecognitionGraph graph,
		DslRecognitionContractDefinition definition)
	{
		_guards = definition.Guards;
		var rules = graph.Rules
			.GroupBy(static rule => rule.Name, StringComparer.Ordinal)
			.Where(static group => group.Count() == 1)
			.ToDictionary(static group => group.Key, static group => group.Single(), StringComparer.Ordinal);
		_externals = definition.Externals
			.Where(item => rules.ContainsKey(item.Value))
			.ToDictionary(static item => item.Key, item => rules[item.Value], StringComparer.Ordinal);
	}

	public bool TryEvaluateGuard(Node.Guard guard, RuleSymbol owner, int position, out bool accepted) =>
		_guards.TryGetValue(guard.Text, out accepted);

	public bool TryResolveExternal(Node.External external, RuleSymbol owner, out RuleSymbol rule) =>
		_externals.TryGetValue(external.Name, out rule!);
}

/// <summary>
/// Interprets the normalized recognition model for editor tooling without executing
/// generated or user code. The successful derivation carries the source extents of every
/// rule and capture that participated in it.
/// </summary>
internal static class DslRecognitionTrace
{
	public static DslRecognitionResult Recognize(
		RecognitionGraph graph,
		Publication publication,
		string input,
		IDslRecognitionContract? contract = null)
	{
		if (graph is null)       throw new ArgumentNullException(nameof(graph));
		if (publication is null) throw new ArgumentNullException(nameof(publication));
		if (input is null)       throw new ArgumentNullException(nameof(input));

		if (publication.Kind != DotGram.Grammar.Parsing.PublishKind.Parse)
			return new DslRecognitionResult(DslRecognitionStatus.Unsupported, 0, [], []);

		var matcher = new Matcher(graph, input, contract);
		var candidates = graph.Trivia.TryGetValue(publication.Rule, out var trivia)
			? matcher.Whole(publication.Rule, trivia).ToArray()
			: matcher.Rule(publication.Rule, 0).ToArray();
		var matches = candidates.Where(match => match.End == input.Length).ToArray();

		foreach (var candidate in candidates)
			if (candidate.End != input.Length)
				matcher.Expect(candidate.End, "end of input");

		if (matches.FirstOrDefault() is { } successful)
			return new DslRecognitionResult(
				DslRecognitionStatus.Success,
				successful.End,
				successful.Extents,
				[]);

		var partial = candidates
			.OrderByDescending(static match => match.End)
			.ThenByDescending(static match => match.Extents.Count)
			.FirstOrDefault() ?? matcher.Best;

		return new DslRecognitionResult(
			matcher.Unsupported ? DslRecognitionStatus.Unsupported : DslRecognitionStatus.Failure,
			matcher.Furthest,
			partial.Extents,
			matcher.Expected);
	}

	sealed class Matcher(
		RecognitionGraph graph,
		string input,
		IDslRecognitionContract? contract)
	{
		readonly HashSet<(RuleSymbol Rule, int Position)> _active = [];
		readonly HashSet<string> _expected = new(StringComparer.Ordinal);
		readonly HashSet<RuleSymbol> _trivia = graph.Rules
			.Where(static rule => rule.Name == "trivia")
			.ToHashSet();
		int _bestMeaningfulEnd;
		int _bestMeaningfulCount;

		public int Furthest { get; private set; }
		public bool Unsupported { get; private set; }
		public Match Best { get; private set; } = Match.Empty(0);
		public IReadOnlyList<string> Expected => _expected.OrderBy(static item => item, StringComparer.Ordinal).ToArray();

		void Consider(Match match, RuleSymbol owner)
		{
			if (_trivia.Contains(owner))
				return;

			var meaningful = match.Extents.Where(extent => !_trivia.Contains(extent.Rule)).ToArray();
			var end = meaningful.Length == 0
				? match.End
				: meaningful.Max(static extent => extent.Position + extent.Length);
			if (end > _bestMeaningfulEnd ||
				end == _bestMeaningfulEnd && meaningful.Length > _bestMeaningfulCount)
			{
				Best = match;
				_bestMeaningfulEnd = end;
				_bestMeaningfulCount = meaningful.Length;
			}
		}

		public void Expect(int position, string expected, RuleSymbol? owner = null)
		{
			if (owner is not null && _trivia.Contains(owner))
				return;

			if (position < Furthest)
				return;

			if (position > Furthest)
			{
				Furthest = position;
				_expected.Clear();
			}

			_expected.Add(expected);
		}

		public IEnumerable<Match> Whole(RuleSymbol rule, Node trivia)
		{
			foreach (var leading in MatchNode(trivia, 0, rule))
				foreach (var body in Rule(rule, leading.End))
					foreach (var trailing in MatchNode(trivia, body.End, rule))
						yield return leading.Append(body).Append(trailing);
		}

		public IEnumerable<Match> Rule(RuleSymbol rule, int position)
		{
			if (!_active.Add((rule, position)) || !graph.Bodies.TryGetValue(rule, out var body))
				yield break;

			try
			{
				foreach (var match in MatchNode(body, position, rule))
				{
					var extents = new List<DslRecognitionExtent>(match.Extents.Count + 1)
					{
						new(rule, null, position, match.End - position),
					};
					extents.AddRange(match.Extents);

					var complete = new Match(match.End, extents);
					Consider(complete, rule);

					yield return complete;
				}
			}
			finally
			{
				_active.Remove((rule, position));
			}
		}

		IEnumerable<Match> MatchNode(Node node, int position, RuleSymbol owner)
		{
			switch (node)
			{
				case Node.Empty:
					yield return Match.Empty(position);
					yield break;

				case Node.Literal literal:
					if (position + literal.Text.Length <= input.Length &&
						string.Compare(
							input, position, literal.Text, 0, literal.Text.Length,
							literal.IgnoreCase, CultureInfo.InvariantCulture) == 0)
						yield return Match.Empty(position + literal.Text.Length);
					else
					{
						var matched = 0;
						while (matched < literal.Text.Length && position + matched < input.Length &&
							(input[position + matched] == literal.Text[matched] ||
							 literal.IgnoreCase && char.ToUpperInvariant(input[position + matched]) ==
							 char.ToUpperInvariant(literal.Text[matched])))
							matched++;
						Expect(position + matched, literal.ToString(), owner);
					}
					yield break;

				case Node.Element element:
					if (position < input.Length && Element(element, input[position]))
						yield return Match.Empty(position + 1);
					else if (element.References.Count == 0)
						Expect(position, element.ToString(), owner);
					yield break;

				case Node.Sequence sequence:
					foreach (var match in Sequence(sequence.Nodes, 0, Match.Empty(position), owner))
						yield return match;
					yield break;

				case Node.Choice choice:
					foreach (var alternative in choice.Nodes)
						foreach (var match in MatchNode(alternative, position, owner))
							yield return match;
					yield break;

				case Node.Atomic atomic:
					foreach (var match in MatchNode(atomic.Body, position, owner))
					{
						yield return match;
						yield break;
					}
					yield break;

				case Node.Repeat repeat:
					foreach (var match in Repeat(repeat, 0, Match.Empty(position), owner))
						yield return match;
					yield break;

				case Node.Lookahead lookahead:
					var found = MatchNode(lookahead.Body, position, owner).Any();
					if (found == lookahead.IsPositive)
						yield return Match.Empty(position);
					yield break;

				case Node.Capture capture:
					foreach (var match in MatchNode(capture.Body, position, owner))
					{
						var extents = new List<DslRecognitionExtent>(match.Extents.Count + 1)
						{
							new(owner, capture.Name, position, match.End - position),
						};
						extents.AddRange(match.Extents);
						var complete = new Match(match.End, extents);
						Consider(complete, owner);
						yield return complete;
					}
					yield break;

				case Node.Construct construct:
					foreach (var match in MatchNode(construct.Body, position, owner))
						yield return match;
					yield break;

				case Node.Call call:
					foreach (var match in Rule(call.Rule, position))
						yield return match;
					yield break;

				case Node.Guard guard:
					if (contract is null ||
						!contract.TryEvaluateGuard(guard, owner, position, out var accepted))
					{
						Unsupported = true;
						yield break;
					}

					if (accepted)
						yield return Match.Empty(position);
					yield break;

				case Node.External external:
					if (contract is null ||
						!contract.TryResolveExternal(external, owner, out var externalRule))
					{
						Unsupported = true;
						yield break;
					}

					foreach (var match in Rule(externalRule, position))
						yield return match;
					yield break;
			}
		}

		IEnumerable<Match> Sequence(
			IReadOnlyList<Node> nodes,
			int index,
			Match current,
			RuleSymbol owner)
		{
			Consider(current, owner);

			if (index == nodes.Count)
			{
				yield return current;
				yield break;
			}

			foreach (var next in MatchNode(nodes[index], current.End, owner))
				foreach (var complete in Sequence(nodes, index + 1, current.Append(next), owner))
					yield return complete;
		}

		IEnumerable<Match> Repeat(
			Node.Repeat repeat,
			int count,
			Match current,
			RuleSymbol owner)
		{
			if (repeat.Max is null || count < repeat.Max)
				foreach (var next in MatchNode(repeat.Body, current.End, owner))
					if (next.End > current.End)
						foreach (var longer in Repeat(repeat, count + 1, current.Append(next), owner))
							yield return longer;

			if (count >= repeat.Min)
				yield return current;
		}

		bool Element(Node.Element element, char value)
		{
			var known = element.Ranges.Any(range => range.From <= value && value <= range.To);

			if (!known)
			{
				var category = CharUnicodeInfo.GetUnicodeCategory(value).ToString();
				known = element.Categories.Any(item => UnicodeCategories.Expand(item).Contains(category));
			}

			if (!known && element.References.Count > 0)
				Unsupported = true;

			return element.IsNegated ? !known && element.References.Count == 0 : known;
		}

	}

	sealed class Match(int end, IReadOnlyList<DslRecognitionExtent> extents)
	{
		public int End { get; } = end;
		public IReadOnlyList<DslRecognitionExtent> Extents { get; } = extents;

		public static Match Empty(int position) => new(position, []);

		public Match Append(Match next)
		{
			if (Extents.Count == 0) return next;
			if (next.Extents.Count == 0) return new Match(next.End, Extents);

			return new Match(next.End, Extents.Concat(next.Extents).ToArray());
		}
	}
}
