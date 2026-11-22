using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;

namespace DotGram.Grammar.Emit;

/// <summary>
/// The check at the entry of a rule that may be entered again where it already stands (<see cref="Reentry"/>):
/// a recursion through a recognizer of the host that did not move, refused where it comes back instead of
/// going round for ever.
/// </summary>
/// <remarks>
/// <para>
/// What is compared is where the rule is being read: a live entry of it at the same position, with the same
/// strength where it is read at one. The positions of the live entries of a rule never go down from the
/// outermost to the innermost, so every live entry at the position of a new one is among the latest; a rule
/// read at no strength has one key a position, and its latest live entry is the one to compare with. A rule
/// read at a strength has as many keys a position as strengths it is entered at, and more live entries at one
/// position than that hold two alike — so what is counted is how many stand there, and the check fails the
/// entry past that count, which a parse that would otherwise go round for ever always reaches.
/// </para>
/// <para>
/// The check is the first thing the rule does, before anything it remembers is asked: an entry that came back
/// is refused whatever was remembered of the position. A rule no cycle marks is written as it always was.
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

	/// <summary>Whether some rule of this machine may be entered again where it stands, and so whether a parse it reads may be refused for it.</summary>
	internal bool ComesBack => Reentrant.Count > 0;

	/// <summary>
	/// The fields the reader keeps for the check: where each marked rule's latest live entry stands, one past
	/// the position so that nought is none, and for a rule read at a strength how many stand there. Carried
	/// with the other registers onto a stack of its own and back (<see cref="RenderDeepening"/>).
	/// </summary>
	IEnumerable<(string Type, string Name)> ReentryRegisters()
	{
		foreach (var pair in Reentrant.OrderBy(static pair => pair.Value))
		{
			yield return ("int", $"standing{pair.Value}");
			yield return ("int", $"standingAlike{pair.Value}");
		}
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
		var which    = Reentrant[rule];
		var standing = $"standing{which}";
		var alike    = $"standingAlike{which}";

		file.Line($"var reentry      = {standing};");
		file.Line($"var reentryAlike = {alike};");
		file.Line();

		using (file.Block("if (reentry == pos + 1)"))
		{
			file.Line($"if ({Lacks("pos", Left(rule, "reentryAlike"))})");
			file.Then(Reentry.Aborts ? $"{CSharpEmitter.Inner("Reentered", CSharpEmitter.IdentifierOf(rule))}(pos);" : "return -1;");
			file.Line();
			file.Line($"{alike} = reentryAlike + 1;");
		}

		file.Line("else");

		using (file.Block(""))
		{
			file.Line($"{standing} = pos + 1;");
			file.Line($"{alike} = 1;");
		}

		file.Line();
	}

	/// <summary>What puts a marked rule's record back as it was before its entry, where the rule answers; nothing for any other rule.</summary>
	IReadOnlyList<string> Leaving(RuleSymbol rule)
	{
		if (!Reentrant.TryGetValue(rule, out var which))
			return [];

		return [$"standing{which} = reentry;", $"standingAlike{which} = reentryAlike;"];
	}

	/// <summary>
	/// How much input has to be left past the position for one more live entry of a rule there, given how many
	/// stand there: the bound is the input left plus two entries a key (Reentry.Keys keys a position), one more than
	/// a chain of entries that each read something can reach, which a lookahead between two of them makes room for.
	/// </summary>
	string Left(RuleSymbol rule, string alike)
	{
		var keys = Reentry.Keys(_graph, rule);

		return keys == 1 ? $"{alike} - 1" : $"({alike} - {keys.ToString(CultureInfo.InvariantCulture)}) / {keys.ToString(CultureInfo.InvariantCulture)}";
	}

	/// <summary>
	/// Whether fewer than <paramref name="count"/> items are left past <paramref name="position"/>: over a span its
	/// length says; over a buffer the source is read on to find out, so that a bound short of what has not come
	/// yet is never taken for the end (the reading holds its input from there while it stands).
	/// </summary>
	string Lacks(string position, string count)
	{
		return BufferedInput ? $"!text.Ensure({position}, {count})" : $"text.Length - {position} < {count}";
	}

	/// <summary>
	/// The methods that refuse a parse for a marked rule entered again: one a rule, out of the way of the hot
	/// path, throwing what the reading's entry catches (<see cref="RenderReentryCaught"/>).
	/// </summary>
	void RenderReentryThrows(Writer file)
	{
		foreach (var pair in Reentrant.OrderBy(static pair => pair.Value))
		{
			var said = Reentry.CycleOf(_graph, pair.Key).Said();

			file.Line();
			file.Line($"/// <summary><c>{pair.Key.Name}</c> entered again where it stands: the parse is refused there (docs/syntax.md §7.2).</summary>");
			file.Line("[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]");

			using (file.Block($"static void {CSharpEmitter.Inner("Reentered", CSharpEmitter.IdentifierOf(pair.Key))}(int pos)"))
				file.Line($"throw new {ReenteredType}(pos, {CSharpEmitter.Quoted("\u0000" + said)});");
		}
	}

	/// <summary>The exception a refused re-entry is carried by, from the check to the reading's entry: internal, and never seen by a caller.</summary>
	internal const string ReenteredType = "Reentered_DotGram";

	/// <summary>
	/// A reading's call of its rule, with a re-entry the check refused caught and written on the failure as the
	/// refusal it is (<see cref="CSharpEmitter.ReenteredSupport"/>); the call alone where nothing is marked.
	/// </summary>
	/// <param name="call">The reading, which answers where it ended.</param>
	/// <param name="end">The local the end is declared as.</param>
	/// <param name="failure">The failure the refusal is written on.</param>
	void RenderReentryCaught(Writer file, string call, string end, string failure)
	{
		if (!ComesBack)
		{
			file.Line($"var {end} = {call};");

			return;
		}

		file.Line($"int {end};");
		file.Line();

		using (file.Block("try"))
			file.Line($"{end} = {call};");

		file.Line($"catch ({ReenteredType} reentered)");

		using (file.Block(""))
			file.Line($"{end} = reentered.Refuse(ref {failure});");
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
	/// The check at an engine's call of a marked rule: up the live calls from the one being read, while they
	/// stand where this one would, counting the entries of the rule; as many as it has keys, and the reading is
	/// refused. The calls up the chain never stand further on than the latest, so it ends at the first that
	/// stands short of here, which is nearly always the first one asked.
	/// </summary>
	void EngineReentering(Writer writer, RuleSymbol rule)
	{
		if (!Reentrant.TryGetValue(rule, out var which))
			return;

		var root  = $"reentryRoot == {Mark(Lands, _entries[rule])}";
		var alike = $"{Tag.Named("RecognizeReenters")}(entries, call, p, {root}, {EngineRuleIndex(rule).ToString(CultureInfo.InvariantCulture)})";
		var left  = $"global::System.Math.Max(0, {Left(rule, alike)})";

		writer.Line(
			Reentry.Aborts
				? $"if ({Lacks("p", left)}) {Tag.Named("RecognizeReentered")}(p, {which.ToString(CultureInfo.InvariantCulture)});"
				: $"if ({Lacks("p", left)}) {{ {Cleared()} goto {Label(writer, _fail)}; }}");
	}

	/// <summary>The walk <see cref="EngineReentering"/> asks, and what refuses the reading where it found the rule.</summary>
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

		file.Line();
		file.Line("/// <summary>A marked rule entered again where it stands: the reading is refused there.</summary>");
		file.Line("[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]");

		using (file.Block($"static void {Tag.Named("RecognizeReentered")}(int pos, int which)"))
		{
			using (file.Block("switch (which)"))
				foreach (var pair in Reentrant.OrderBy(static pair => pair.Value))
				{
					var said = CSharpEmitter.Quoted("\u0000" + Reentry.CycleOf(_graph, pair.Key).Said());

					file.Line($"case {pair.Value.ToString(CultureInfo.InvariantCulture)}: throw new {ReenteredType}(pos, {said});");
				}

			file.Line();
			file.Line($"throw new {ReenteredType}(pos, \"\\u0000A rule came back to itself without reading anything.\");");
		}
	}
}
