using System;
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
/// What is compared is where the rule is being read: the live entries of it at the same position, with the same
/// strength where it is read at one. The positions of the live entries of a rule never go down from the
/// outermost to the innermost, so every live entry at the position of a new one is among the latest, and one
/// register a rule holds where the latest stands and how many stand there.
/// </para>
/// <para>
/// Where nothing is left to read after the call back (<see cref="Reentry.Tail"/>), an entry at the position of a
/// live one is read exactly as that one is, and never comes back: it fails at once, as an alternative does.
/// Elsewhere what is pending after each call back grows, and a parse may need one more entry at the position for
/// each thing pending — <c>1+2+3</c> through <c>Sum = Num | @Ws &amp; Sum &amp; '+' &amp; Num</c> needs two —
/// so the entries standing there are counted against the input left: past the input left plus two a key, none of
/// them could read what each pending piece needs, and the entry fails as an alternative does. That search can
/// still take time exponential in the input, so a reading also counts every entry that came back, and past a
/// ceiling quadratic in the input throws instead of reading on.
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

	/// <summary>Whether some marked rule of this machine has its entries counted, and so whether a reading may reach the ceiling.</summary>
	bool Counts => Reentrant.Keys.Any(rule => !Reentry.Tail(_graph, rule));

	/// <summary>
	/// The fields the reader keeps for the check: where each marked rule's latest live entry stands, one past
	/// the position so that nought is none; for a rule whose entries are counted, how many stand there, and how many
	/// entries came back in the whole reading. Carried with the other registers onto a stack of its own and back
	/// (<see cref="RenderDeepening"/>).
	/// </summary>
	IEnumerable<(string Type, string Name)> ReentryRegisters()
	{
		foreach (var pair in Reentrant.OrderBy(static pair => pair.Value))
		{
			yield return ("int", $"standing{pair.Value}");

			if (!Reentry.Tail(_graph, pair.Key))
				yield return ("int", $"standingAlike{pair.Value}");
		}

		if (Counts)
			yield return ("long", "reentries");
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

		if (Reentry.Tail(_graph, rule))
		{
			// Nothing is left to read after the call back: an entry where a live one stands reads what that one reads.
			file.Line($"var reentry = {standing};");
			file.Line();
			file.Line("if (reentry == pos + 1)");
			file.Then("return -1;");
			file.Line();
			file.Line($"{standing} = pos + 1;");
			file.Line();

			return;
		}

		file.Line($"var reentry      = {standing};");
		file.Line($"var reentryAlike = {alike};");
		file.Line();

		using (file.Block("if (reentry == pos + 1)"))
		{
			file.Line($"if ({Ceiling("reentries", BufferedInput ? "text.End" : "text.Length")})");
			file.Then($"{CSharpEmitter.Inner("Reentered", CSharpEmitter.IdentifierOf(rule))}(pos);");
			file.Line();
			file.Line($"if ({Lacks("pos", Left(rule, "reentryAlike"))})");
			file.Then("return -1;");
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

		return Reentry.Tail(_graph, rule)
			? [$"standing{which} = reentry;"]
			: [$"standing{which} = reentry;", $"standingAlike{which} = reentryAlike;"];
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
	/// Whether one more entry that came back takes the reading past its ceiling: 2²⁴ entries, or four for every
	/// square of the input's length where that is more — of what has been read of it, over a buffer. A grammar whose
	/// valid input needs one entry that came back for each pair of positions reads to the end under it; one whose
	/// search doubles with every item does not.
	/// </summary>
	/// <param name="count">The count, raised by one here.</param>
	/// <param name="length">The input's length.</param>
	static string Ceiling(string count, string length)
	{
		return Reentry.Ceiling is { } ceiling
			? $"++{count} > {ceiling.ToString(CultureInfo.InvariantCulture)}L"
			: $"++{count} > 16777216L && {count} > 4L * {length} * {length}";
	}

	/// <summary>
	/// The methods that throw where a reading reached its ceiling, one a counted rule, out of the way of the hot path:
	/// an <see cref="InvalidOperationException"/>, which is no refusal — the input may be valid; it is too costly to
	/// read with this grammar.
	/// </summary>
	void RenderReentryThrows(Writer file)
	{
		foreach (var pair in Reentrant.OrderBy(static pair => pair.Value))
		{
			if (Reentry.Tail(_graph, pair.Key))
				continue;

			file.Line();
			file.Line($"/// <summary><c>{pair.Key.Name}</c> came back too many times in one reading (docs/syntax.md §7.2).</summary>");
			file.Line("[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]");

			using (file.Block($"static void {CSharpEmitter.Inner("Reentered", CSharpEmitter.IdentifierOf(pair.Key))}(int pos)"))
				file.Line($"throw {TooCostly(pair.Key)};");
		}
	}

	/// <summary>The exception a counted rule throws at its ceiling, saying where.</summary>
	string TooCostly(RuleSymbol rule)
	{
		var (before, after) = Reentry.CycleOf(_graph, rule).TooCostly();

		return $"new global::System.InvalidOperationException({CSharpEmitter.Quoted(before)} + " +
			$"pos.ToString(global::System.Globalization.CultureInfo.InvariantCulture) + {CSharpEmitter.Quoted(after)})";
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
	/// stand where this one would, counting the entries of the rule. Where nothing is left to read after the call
	/// back, one there fails the call; elsewhere one there is one more that came back, and as many as the input left
	/// allows fail it. The calls up the chain never stand further on than the latest, so the walk ends at the first
	/// that stands short of here, which is nearly always the first one asked.
	/// </summary>
	void EngineReentering(Writer writer, RuleSymbol rule)
	{
		if (!Reentrant.TryGetValue(rule, out var which))
			return;

		var root  = $"reentryRoot == {Mark(Lands, _entries[rule])}";
		var alike = $"{Tag.Named("RecognizeReenters")}(entries, call, p, {root}, {EngineRuleIndex(rule).ToString(CultureInfo.InvariantCulture)})";

		if (Reentry.Tail(_graph, rule))
		{
			writer.Line($"if ({alike} > 0) {{ {Cleared()} goto {Label(writer, _fail)}; }}");

			return;
		}

		var left    = $"global::System.Math.Max(0, {Left(rule, "reentryAlike")})";
		var ceiling = Ceiling("reentries", BufferedInput ? "text.End" : "text.Length");

		writer.Line($"reentryAlike = {alike};");
		writer.Line(
			$"if (reentryAlike > 0 && {ceiling}) {Tag.Named("RecognizeReentered")}(p, {which.ToString(CultureInfo.InvariantCulture)});");
		writer.Line($"if ({Lacks("p", left)}) {{ {Cleared()} goto {Label(writer, _fail)}; }}");
	}

	/// <summary>
	/// The locals the engine's reading keeps for the check: where the reading began, which says whether its root entry
	/// reads a marked rule; and where an entry is counted, how many stand at a call and how many came back.
	/// </summary>
	void RenderEngineReentryLocals(Writer file)
	{
		if (!EngineComesBack)
			return;

		file.Line("var reentryRoot = state;");

		if (Counts)
		{
			file.Line("var reentryAlike = 0;");
			file.Line("var reentries = 0L;");
		}
	}

	/// <summary>The walk <see cref="EngineReentering"/> asks, and what throws where a reading reached its ceiling.</summary>
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

		if (!Counts)
			return;

		file.Line();
		file.Line("/// <summary>A marked rule came back too many times in one reading (docs/syntax.md §7.2).</summary>");
		file.Line("[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]");

		using (file.Block($"static void {Tag.Named("RecognizeReentered")}(int pos, int which)"))
		{
			using (file.Block("switch (which)"))
				foreach (var pair in Reentrant.OrderBy(static pair => pair.Value))
					if (!Reentry.Tail(_graph, pair.Key))
						file.Line($"case {pair.Value.ToString(CultureInfo.InvariantCulture)}: throw {TooCostly(pair.Key)};");

			file.Line();
			file.Line("throw new global::System.InvalidOperationException(\"A rule came back to itself too many times without reading anything.\");");
		}
	}
}
