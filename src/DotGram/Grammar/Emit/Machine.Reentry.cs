using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;

namespace DotGram.Grammar.Emit;

/// <summary>
/// The check at the entry of a rule that may be entered again where it already stands (<see cref="Reentry"/>):
/// a recursion through a recognizer of the host that did not move, cut where going on could find nothing new
/// instead of going round for ever.
/// </summary>
/// <remarks>
/// <para>
/// What is compared is where the rule is being read: the live entries of it at the same position. The positions of
/// the live entries of a rule never go down from the outermost to the innermost, so every live entry at the position
/// of a new one is among the latest, and one register a rule holds where the latest stands.
/// </para>
/// <para>
/// Only a rule with nothing left to read after the call back is marked (<see cref="Reentry.Marked"/>): an entry at
/// the position of a live one is read exactly as that one is, and never comes back, so it fails at once, as an
/// alternative does. A rule with something pending after the call back is not marked, and is written as it always was.
/// </para>
/// <para>
/// The check is the first thing the rule does, before anything it remembers is asked: an entry that came back
/// is cut whatever was remembered of the position. A rule no cycle marks is written as it always was.
/// </para>
/// </remarks>
sealed partial class Machine
{
	Dictionary<RuleSymbol, int>? _reentrant;

	/// <summary>The marked rules this machine reads, numbered for the fields that hold where each stands and for the engine's marks of them.</summary>
	Dictionary<RuleSymbol, int> Reentrant
	{
		get
		{
			if (_reentrant is { } known)
				return known;

			_reentrant = [];

			// In the graph's order, so that the numbers do not depend on how the machine holds its rules.
			if (Reentry.Any(_graph))
				foreach (var rule in Reentry.MarkedRules(_graph))
					if (_rules.Contains(rule))
						_reentrant[rule] = _reentrant.Count;

			return _reentrant;
		}
	}

	/// <summary>
	/// The fields the reader keeps for the check: where each marked rule's latest live entry stands, one past the
	/// position so that nought is none. Carried with the other registers onto a stack of its own and back
	/// (<see cref="RenderDeepening"/>).
	/// </summary>
	IEnumerable<(string Type, string Name)> ReentryRegisters()
	{
		foreach (var pair in Reentrant.OrderBy(static pair => pair.Value))
			yield return ("int", $"standing{pair.Value}");
	}

	/// <summary>
	/// A marked rule's entry for a body written elsewhere: a method in front of the rule, under the rule's own name
	/// (<paramref name="name"/>), so that every call and the stack it is carried onto enter through it. The stack is
	/// probed there where the rule can deepen it, the entry checked and recorded, the rule read by the method under
	/// it, and the record put back.
	/// </summary>
	/// <remarks>
	/// For a rule read by a method of its own, whose body says where it answers in places only its writer knows.
	/// A rule with a way back into it has the check in that method, put back at each of the few places it answers
	/// (<see cref="Entering"/>, <see cref="Leaving"/>): a method in front of it was one more call a level, which the
	/// recursion paid a tenth of its time for.
	/// </remarks>
	void RenderReentryCheck(Writer file, RuleSymbol rule, string name, string strength, bool deepens)
	{
		var body = CSharpEmitter.Inner("Reentering", CSharpEmitter.IdentifierOf(rule));

		file.Line($"/// <summary><c>{rule.Name}</c>, entered where it does not already stand (docs/syntax.md §7.2).</summary>");

		using (file.Block($"public int {name}(int pos{strength})"))
		{
			if (deepens)
				Probe(file, rule);

			Entering(file, rule);
			file.Line($"var read = {body}(pos{(strength.Length > 0 ? ", power" : "")});");
			file.Line();

			foreach (var line in Leaving(rule))
				file.Line(line);

			file.Line();
			file.Line("return read;");
		}

		file.Line();
	}

	/// <summary>
	/// The check at a marked rule's entry, and the record of the entry: after the stack is probed, since a reading
	/// carried onto a stack of its own enters the rule again there and has to find the record as it was before this
	/// entry; before anything the rule remembers is asked.
	/// </summary>
	/// <remarks>
	/// The record is put back where the rule answers (<see cref="Leaving"/>) and not where an exception leaves it:
	/// nothing inside a reading catches what leaves a rule and reads on — the reading ends, its reader with it, and a
	/// reading carried onto another stack hands the exception back to be thrown again. A <c>try</c> around the rule
	/// cost the recursion a fifth of its time.
	/// </remarks>
	void Entering(Writer file, RuleSymbol rule)
	{
		var standing = $"standing{Reentrant[rule]}";

		// Nothing is left to read after the call back: an entry where a live one stands reads what that one reads.
		file.Line($"var reentry = {standing};");
		file.Line();
		file.Line("if (reentry == pos + 1)");
		file.Then("return -1;");
		file.Line();
		file.Line($"{standing} = pos + 1;");
		file.Line();
	}

	/// <summary>What puts a marked rule's record back as it was before its entry, where the rule answers; nothing for any other rule.</summary>
	IReadOnlyList<string> Leaving(RuleSymbol rule)
	{
		if (!Reentrant.TryGetValue(rule, out var which))
			return [];

		return [$"standing{which} = reentry;"];
	}

	/// <summary>Whether the engine this machine writes reads a marked rule.</summary>
	bool EngineComesBack => Reentrant.Count > 0;

	/// <summary>
	/// What an engine's call entry says it calls: a rule's number where its value is built, as always; for a
	/// marked rule that builds none, a number below every other, which no other place reads, so that the walk
	/// up the live calls can tell its entries (<see cref="EngineReentering"/>).
	/// </summary>
	int EngineRuleIndex(RuleSymbol rule)
	{
		var index = ValueRule(rule);

		return index < 0 && Reentrant.TryGetValue(rule, out var which) ? -3 - which : index;
	}

	/// <summary>
	/// The check at an engine's call of a marked rule: up the live calls from the one being read, while they stand
	/// where this one would, looking for an entry of the rule; one there fails the call. The calls up the chain never
	/// stand further on than the latest, so the walk ends at the first that stands short of here, which is nearly always
	/// the first one asked.
	/// </summary>
	/// <remarks>
	/// The cut is recorded as a failure that is not a terminal test, with nothing expected (<see cref="Cleared"/>): the
	/// engine's alternatives at the position may have been passed over by what their first character predicted, so a
	/// cut that recorded nothing would leave the refusal short of where the reading got.
	/// </remarks>
	void EngineReentering(Writer writer, RuleSymbol rule)
	{
		if (!Reentrant.ContainsKey(rule))
			return;

		var root  = $"reentryRoot == {Mark(Lands, _entries[rule])}";
		var alike = $"{Tag.Named("RecognizeReenters")}(entries, call, p, {root}, {EngineRuleIndex(rule).ToString(CultureInfo.InvariantCulture)})";

		writer.Line($"if ({alike} > 0) {{ {Cleared()} goto {Label(writer, _fail)}; }}");
	}

	/// <summary>The local the engine's reading keeps for the check: where the reading began, which says whether its root entry reads a marked rule.</summary>
	void RenderEngineReentryLocals(Writer file)
	{
		if (!EngineComesBack)
			return;

		file.Line("var reentryRoot = state;");
	}

	/// <summary>The walk <see cref="EngineReentering"/> asks.</summary>
	void RenderEngineReentry(Writer file)
	{
		if (!EngineComesBack)
			return;

		file.Line();
		file.Line("/// <summary>How many live entries of a marked rule stand at a position, up the calls being read (docs/syntax.md §7.2).</summary>");

		using (file.Block(
			$"static int {Tag.Named("RecognizeReenters")}(ParserArena_DotGram entries, int call, int p, bool root, int rule)"))
		{
			file.Line("var alike = 0;");
			file.Line();

			using (file.Block("for (var up = call; up >= 0; up = entries[up].CallIndex)"))
			{
				file.Line("var entry = entries[up];");
				file.Line();
				file.Line("if (entry.Position != p)");
				file.Then("break;");
				file.Line();
				file.Line("if (up == 0 ? root : entry.RuleIndex == rule)");
				file.Then("alike++;");
			}

			file.Line();
			file.Line("return alike;");
		}
	}
}
