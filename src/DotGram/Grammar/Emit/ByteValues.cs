using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;

namespace DotGram.Grammar.Emit;

/// <summary>
/// The strings the compiler makes of text the rule matched, as the byte form holds them: the bytes, not a string made of them.
/// </summary>
/// <remarks>
/// <para>
/// Over bytes a string rule that is its own text is the bytes it matched, and nothing decodes them. Some such rules are given
/// a construction (<c>parserText.ToString()</c>) only so that a value can be had of them: a rule a guard reads a name of, and the
/// rule an array collects a text rule through. The byte form of those holds the bytes, and the array that collects them is
/// an array of bytes.
/// </para>
/// <para>
/// A rule only goes that way where nothing is written against the string: every rule that holds the value of one is an array
/// that collects only such values. The C# the author wrote is handed a string, so a value it takes cannot be bytes, and a
/// string beside the bytes in one array would take a decoding that is not made. Such a grammar is refused, as it was.
/// </para>
/// </remarks>
static class ByteValues
{
	/// <summary>The text of a rule the compiler built from what the rule matched, its construction.</summary>
	internal const string MatchedText = "parserText.ToString()";

	/// <summary>The type a rule's value has over bytes where that is not the type it has over characters.</summary>
	internal static IReadOnlyDictionary<RuleSymbol, string> Of(RecognitionGraph graph)
	{
		var language = graph.Language;
		var bytes    = language.SequenceOf(language.Spelling(KnownType.Byte));
		var arrays   = language.SequenceOf(bytes);

		var (texts, lists) = Candidates(graph);

		// What is left is what every rule that holds one of them can hold as bytes: a rule that holds the value of a text
		// rule is an array of such values, and one that holds an array is one of them.
		for (var changed = true; changed;)
		{
			changed = false;

			foreach (var rule in graph.Rules)
			{
				if (!graph.Results.TryGetValue(rule, out var members))
					continue;

				var collected = lists.Contains(rule) ? CollectedBy(graph.Bodies[rule]) : null;

				foreach (var member in members)
				{
					var held = member.Rule;
					var ours = held is not null && (texts.Contains(held) || lists.Contains(held));

					// The value of one is held by a rule that is not such an array, and is written against a string.
					if (ours && !lists.Contains(rule))
						changed |= texts.Remove(held!) | lists.Remove(held!);

					// An array that collects anything else: text of no rule, values of several kinds, a value that is no byte value.
					// A member it does not collect is only handed on, and its type is its own. A member gathers every
					// capture it names, and each of them has to be one.
					if (lists.Contains(rule) && (collected is null || collected.Contains(member.Name) || member.Joins.Any(collected.Contains)) &&
						(!ours || member.Element is not null || member.Joins.Count > 0 && !Gathers(graph.Bodies[rule], member, texts, lists)))
						changed |= lists.Remove(rule);
				}
			}
		}

		var types = new Dictionary<RuleSymbol, string>();

		foreach (var rule in texts)
			types[rule] = bytes;

		foreach (var rule in lists)
			types[rule] = arrays;

		return types;
	}

	/// <summary>
	/// The rules whose value is read where a publication is: its own; of a rule the grammar generates a type for, the value of
	/// each rule it holds as a member, and a call it does not name is only recognized; and of any other rule, a string, an array
	/// or one that builds with <c>=&gt;</c>, the value of every rule it calls, for what it joins, collects or hands the
	/// construction is read as it stands.
	/// </summary>
	internal static HashSet<RuleSymbol> Read(RecognitionGraph graph, RuleSymbol root)
	{
		var read  = new HashSet<RuleSymbol> { root };
		var queue = new Queue<RuleSymbol>();

		queue.Enqueue(root);

		while (queue.Count > 0)
		{
			var rule = queue.Dequeue();

			if (!graph.Results.TryGetValue(rule, out var members) || !graph.Bodies.TryGetValue(rule, out var body))
				continue;

			var held = members.Count > 0 && !graph.Types.ContainsKey(rule)
				? members.Select(static member => member.Rule)
				: NodeWalk.Descendants(body).OfType<Node.Call>().Select(static call => (RuleSymbol?)call.Rule);

			foreach (var one in held)
				if (one is not null && read.Add(one))
					queue.Enqueue(one);
		}

		return read;
	}

	/// <summary>
	/// The first of these rules that is an array collecting both a piece of the input, the value of a text rule, and a string
	/// or a character a rule builds, on any route and through what it reads in place; with the first call of each kind and
	/// whether the built one is a character. Null where there is none. Over bytes the piece is its bytes and the built value a
	/// string, and one array cannot hold both without decoding the one.
	/// </summary>
	internal static (RuleSymbol Array, RuleSymbol Text, RuleSymbol Built, bool Character)? Mixed(RecognitionGraph graph, IEnumerable<RuleSymbol> rules)
	{
		var language = graph.Language;
		var texts    = Candidates(graph).Texts;
		var kept     = Of(graph);

		foreach (var rule in rules)
		{
			if (Made(rule) || kept.ContainsKey(rule) || !Collects(rule, out var element))
				continue;

			var text  = (RuleSymbol?)null;
			var built = (RuleSymbol?)null;
			var isChar = false;

			Visit(rule, element, []);

			if (text is not null && built is not null)
				return (rule, text, built, isChar);

			// The calls an array collects, in the order its members are; a rule the compiler made to read one in place is
			// gone into.
			void Visit(RuleSymbol array, string elementType, HashSet<RuleSymbol> seen)
			{
				// A rule read in place may read another in place, as deep as the grammar nests them.
				if (DeepStack.Low)
				{
					DeepStack.Run(() =>
					{
						Visit(array, elementType, seen);

						return true;
					});

					return;
				}

				if (!seen.Add(array) || !graph.Results.TryGetValue(array, out var members))
					return;

				var collected = CollectedBy(graph.Bodies[array]);

				foreach (var member in members)
				{
					if (member.Rule is not { } held || collected is not null && !collected.Contains(member.Name) && !member.Joins.Any(collected.Contains))
						continue;

					foreach (var one in Held(array, member))
					{
						if (Made(one) && Collects(one, out var inner) && inner == elementType)
							Visit(one, elementType, seen);
						else if (texts.Contains(one))
							text ??= one;
						else if (graph.Types.TryGetValue(one, out var heldType) && built is null &&
							(IsText(language, heldType) || language.Classify(heldType, out _) == KnownType.Char))
							(built, isChar) = (one, !IsText(language, heldType));
					}
				}
			}
		}

		return null;

		// A rule the compiler made for a part of another, which bears the name of the one it was made from.
		static bool Made(RuleSymbol rule)
		{
			return rule.Declaration is { } declaration && declaration.Name != rule.Name;
		}

		bool Collects(RuleSymbol rule, out string element)
		{
			element = "";

			if (!graph.Types.TryGetValue(rule, out var type) || language.ElementOf(type) is not { } of || !IsText(language, of))
				return false;

			element = of;

			return true;
		}

		// The rules a member's slots hold: its own and those of every capture it gathers.
		IEnumerable<RuleSymbol> Held(RuleSymbol array, ResultMember member)
		{
			if (member.Joins.Count == 0)
				return [member.Rule!];

			var names = new HashSet<string>([member.Name, .. member.Joins], System.StringComparer.Ordinal);

			return NodeWalk.Descendants(graph.Bodies[array])
				.OfType<Node.Capture>()
				.Where(capture => names.Contains(capture.Name))
				.Select(static capture => (capture.Body as Node.Call)?.Rule)
				.OfType<RuleSymbol>();
		}
	}

	/// <summary>The rules the compiler gave a string of the text they matched, and the arrays of strings that collect only values.</summary>
	static (HashSet<RuleSymbol> Texts, HashSet<RuleSymbol> Lists) Candidates(RecognitionGraph graph)
	{
		var language = graph.Language;
		var texts    = new HashSet<RuleSymbol>();
		var lists    = new HashSet<RuleSymbol>();

		foreach (var rule in graph.Rules)
		{
			if (!graph.Types.TryGetValue(rule, out var type) || !graph.Bodies.TryGetValue(rule, out var body))
				continue;

			var built = NodeWalk.Descendants(body).OfType<Node.Construct>().ToList();

			if (built.Count == 0)
				continue;

			if (IsText(language, type))
			{
				if (built.All(static one => one.How is Construction.Expression { At: < 0, Text: MatchedText }))
					texts.Add(rule);
			}
			else if (language.ElementOf(type) is { } element && IsText(language, element))
			{
				if (built.All(static one => one.How is Construction.Sequence or Construction.Operand))
					lists.Add(rule);
			}
		}

		return (texts, lists);
	}

	/// <summary>The captures an array rule collects, whichever route is read; null where it collects every one it has.</summary>
	static HashSet<string>? CollectedBy(Node body)
	{
		var names = new HashSet<string>(System.StringComparer.Ordinal);

		foreach (var node in NodeWalk.Descendants(body))
			switch ((node as Node.Construct)?.How)
			{
				case Construction.Sequence { Taken: null }:
					return null;

				case Construction.Sequence { Taken: { } taken }:
					names.UnionWith(taken);

					break;

				case Construction.Operand(var captures):
					names.UnionWith(captures);

					break;
			}

		return names;
	}

	/// <summary>Whether every capture a member gathers is a call of a rule that holds bytes.</summary>
	static bool Gathers(Node body, ResultMember member, HashSet<RuleSymbol> texts, HashSet<RuleSymbol> lists)
	{
		var found = 0;

		foreach (var node in NodeWalk.Descendants(body))
			if (node is Node.Capture(var name, var held) && member.Takes(name))
			{
				found++;

				if (Called(held) is not { } rule || !texts.Contains(rule) && !lists.Contains(rule))
					return false;
			}

		return found > 0;
	}

	/// <summary>The rule a capture holds a call of, through the repetition and grouping around it; null where it holds anything else.</summary>
	static RuleSymbol? Called(Node node)
	{
		while (true)
		{
			switch (node)
			{
				case Node.Call(var rule, _):
					return rule;

				case Node.Repeat(var inner, _, _):
					node = inner;

					break;

				case Node.Counted(var inner, _, _):
					node = inner;

					break;

				case Node.Atomic(var inner):
					node = inner;

					break;

				case Node.Sequence { Nodes.Count: 1 } one:
					node = one.Nodes[0];

					break;

				default:
					return null;
			}
		}
	}

	static bool IsText(ITargetLanguage language, string type)
	{
		return language.Classify(type, out var form) == KnownType.Text && form is TypeSpellingForm.Syntax or TypeSpellingForm.Global;
	}
}
