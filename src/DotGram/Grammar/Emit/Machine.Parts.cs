using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Emit;

/// <summary>
/// How many methods the state table is written in, and which states go in each.
/// </summary>
/// <remarks>
/// <para>
/// RyuJIT stops optimizing a method past about two thousand basic blocks — measured, on two
/// unrelated grammar shapes that agreed on the block count to within 2% while differing by
/// 57% in size, so it is the branches it counts and not the bytes. A recognizer past that
/// line is compiled the way a method is on its first call and stays that way: no common
/// subexpression elimination, no bounds-check elimination, nothing. Every real parser here
/// is past it.
/// </para>
/// <para>
/// The limit is per method, so the answer is more methods, and they are written as local
/// functions — the C# compiler gives each one its own method, and so its own budget, while
/// carrying what crosses between them in a frame it writes itself. Nothing about the states
/// changes: they go on reading and writing the same variables, which are now the enclosing
/// method's rather than their own.
/// </para>
/// <para>
/// What matters is where the line falls. Control reaches the dispatch about four times in a
/// whole parse — measured, on a URL against `Rfc3986` — so a crossing that goes through it
/// costs nothing worth counting. A <c>goto</c> that crosses is another matter: those run per
/// character. So the cut is made where no jump crosses it at all, and the budget only says
/// roughly where to look.
/// </para>
/// </remarks>
sealed partial class Machine
{
	/// <summary>The states each part is written with, in the order they are written.</summary>
	List<List<int>> _parts = [];

	/// <summary>What each part is estimated to cost, aligned with <see cref="_parts"/>.</summary>
	List<int> _partCosts = [];

	/// <summary>
	/// Every method this machine had to leave past the limit, told as warnings.
	/// </summary>
	/// <remarks>
	/// Warnings and not errors: the parser is correct, it is merely compiled the way a
	/// method is on its first call and stays that way, several times slower than the same
	/// parser under the limit. The message names the numbers it acted under, because the
	/// author's remedy — splitting a rule, simplifying an alternative — is chosen against
	/// them.
	/// </remarks>
	public List<GramDiagnostic> Oversized { get; } = [];

	/// <summary>Where a machine-wide warning points: the rule this machine was built for.</summary>
	public RuleSymbol? Anchor { get; set; }

	void Oversize(string message, RuleSymbol? rule = null)
	{
		var at = (rule ?? Anchor)?.Declaration?.At ?? default;

		Oversized.Add(new GramDiagnostic(
			Unoptimized, message, at.Position, at.Length, GramSeverity.Warning));
	}

	/// <summary>Which part a state is written in, by its index.</summary>
	int[] _partOf = [];

	/// <summary>Whether the table is written in more than one method at all.</summary>
	bool Divided => _parts.Count > 1;

	/// <summary>Which part a state is written in, or 0 for anything not written.</summary>
	int PartOf(int state) =>
		state - First is var index && index >= 0 && index < _partOf.Length ? _partOf[index] : 0;

	/// <summary>
	/// Divides the states between as few methods as will each stay inside the budget.
	/// </summary>
	/// <remarks>
	/// Evenly rather than greedily: filling each part to the budget before starting the next
	/// leaves the last nearly empty and the rest on the line, and being on the line is what
	/// the budget is set below the limit to avoid. Then each cut is moved to the nearest
	/// place no jump crosses — the layout has already threaded the states into chains, so
	/// those places are the gaps between chains, and a cut in one costs only what going
	/// through the dispatch costs anyway.
	/// </remarks>
	void PlanParts()
	{
		_parts  = [];
		_partOf = new int[_states.Count];

		var whole = 0;

		foreach (var index in _order)
			whole += Branches(_bodies[index]);

		// One method for as long as it fits, which is every grammar small enough not to
		// care — and byte for byte what was written before any of this.
		_partCosts = [];

		if (whole <= Budget || _order.Count < 2)
		{
			_parts.Add(_order);
			_partCosts.Add(whole);

			return;
		}

		// Where each state stands in the running total, so that a cut can be asked for by
		// cost rather than found by walking to it — walking accumulates drift every time the
		// cut moves, and drift is what puts a part over the budget it was divided to keep.
		var prefix = new int[_order.Count + 1];

		for (var i = 0; i < _order.Count; i++)
			prefix[i + 1] = prefix[i] + Branches(_bodies[_order[i]]);

		// A tenth under the part size, because the cut moves to where it is cleanest and
		// that costs a little either way. Dividing to the size exactly and then moving the
		// cut is how a part ends up over it.
		//
		// The size, not the budget: whether to divide at all and how large a part should be
		// are two questions, and using one number for both is what made a machine only just
		// over the budget come out in two parts — the worst arrangement there is, slower
		// than one part and slower than many. See <see cref="Part"/>.
		//
		// Asked for rather than obeyed. `PartSize` is whatever a consumer wrote in their
		// build, and a build property is a wish: nothing they can type may stop a parser
		// being generated. Nought and less mean the finest division this machine has —
		// every state its own part — and anything past the whole means one part, which is
		// what asking for a piece larger than there is can only mean. Between those it is
		// taken at its word.
		var size     = Math.Max(1, Math.Min(PartSize, whole));
		var aimed    = Math.Max(1, size * 9 / 10);
		var parts    = (whole + aimed - 1) / aimed;
		var crossing = Crossings();
		var cuts     = new List<int>();

		for (var k = 1; k < parts; k++)
		{
			var wanted = Position(prefix, whole * k / parts);
			var cut    = Clearest(crossing, prefix, wanted, whole / parts / 8);

			if (cut > (cuts.Count > 0 ? cuts[^1] : 0) && cut < _order.Count)
				cuts.Add(cut);
		}

		var from = 0;

		foreach (var cut in cuts)
		{
			Take(from, cut);

			from = cut;
		}

		Take(from, _order.Count);

		void Take(int start, int end)
		{
			var part = new List<int>(end - start);
			var cost = 0;

			for (var at = start; at < end; at++)
			{
				_partOf[_order[at]] = _parts.Count;

				part.Add(_order[at]);

				cost += Branches(_bodies[_order[at]]);
			}

			if (part.Count > 0)
			{
				_parts.Add(part);
				_partCosts.Add(cost);
			}
		}
	}

	/// <summary>
	/// What the dispatch has a case for, and which part each of them is written in.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Keyed by what an arena entry or a departure actually says, which is not always where
	/// control ends up: a state that does nothing but jump somewhere is collapsed, and the
	/// dispatch answers to its old number while going to the state it really is. Keying this
	/// by the resolved state instead loses the case for every collapsed one — the dispatch
	/// then falls to its default and the parse refuses input it should accept.
	/// </para>
	/// <para>
	/// Two things need a case. A resume lands on one, which is what <see cref="Dispatched"/>
	/// already names. And a jump that leaves the part it is written in cannot be a jump any
	/// more, so it becomes a departure and comes back through here.
	/// </para>
	/// </remarks>
	SortedDictionary<int, int> Dispatching() => _dispatching ??= DispatchingNow();

	/// <summary>
	/// Cleared when a layout is planned, which is the only thing that can change the
	/// answer, and held between the many places that ask for it.
	/// </summary>
	/// <remarks>
	/// It used to be worked out afresh at each of them, and two of them are per part: the
	/// switch at the head of a part and the labels <see cref="RenderStates"/> names. Each
	/// answer walks every dispatched state and every jump of every body, so recomputing it
	/// per part is quadratic in the size of the machine — measured at a second per part
	/// pair on a grammar of eight hundred rules, which is what stopped a SQL-sized grammar
	/// being generated in a reasonable time.
	/// </remarks>
	SortedDictionary<int, int>? _dispatching;

	SortedDictionary<int, int> DispatchingNow()
	{
		var cases = new SortedDictionary<int, int>();

		foreach (var state in Dispatched())
			if (Resolved(state) is var landed && landed >= First && Written(landed))
				cases[state] = PartOf(landed);

		foreach (var index in _order)
			foreach (var jump in Recorded(index).Jumps)
				if (Resolved(jump) is var target && target >= First && Written(target) &&
					PartOf(target) != PartOf(index + First))
				{
					cases[target] = PartOf(target);
				}

		return cases;
	}

	/// <summary>Whether a jump from one state to another leaves the part it is written in.</summary>
	bool Departs(int index, int target) =>
		target >= First && PartOf(target) != PartOf(index + First);

	/// <summary>
	/// The same text with every jump that leaves the part turned into a departure.
	/// </summary>
	/// <remarks>
	/// A <c>goto</c> cannot leave a method, so a jump to a state written elsewhere becomes
	/// the state it was going to and a return: the dispatch calls whichever part that state
	/// is written in, and control carries on where it was going. The three fixed labels are
	/// the same thing — they belong to the method the parts are written inside, not to any
	/// part — and <c>Fail</c> keeps whatever the state was expecting, because the variable
	/// holding it is one of the ones the parts share.
	/// </remarks>
	string Departing(string body, int index)
	{
		body = Gotos.Replace(body, match =>
			int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture) is var target &&
			Departs(index, target)
				? $"{{ state = {target}; goto Leave; }}"
				: match.Value);

		return body
			.Replace("goto Dispatch;", "goto Leave;")
			.Replace("goto Return;", $"{{ state = {Return}; goto Leave; }}")
			.Replace("goto Accept;", $"{{ state = {Accept}; goto Leave; }}")
			.Replace("goto Fail;", $"{{ state = {Fail}; goto Leave; }}");
	}

	/// <summary>
	/// How many jumps cross each position in the written order.
	/// </summary>
	/// <remarks>
	/// A jump from a state written before a position to one written at or after it crosses
	/// that position, and so does one the other way. Resumes are not counted: those go
	/// through the dispatch already, and going through it from another method costs a call
	/// on a path that is taken a handful of times in a whole parse.
	/// </remarks>
	int[] Crossings()
	{
		var at = new Dictionary<int, int>(_order.Count);

		for (var i = 0; i < _order.Count; i++)
			at[_order[i] + First] = i;

		// A difference array: an edge adds one to every position between its two ends.
		var crossing = new int[_order.Count + 1];

		foreach (var index in _order)
		{
			if (!at.TryGetValue(index + First, out var from))
				continue;

			foreach (var jump in Recorded(index).Jumps)
			{
				if (!at.TryGetValue(Resolved(jump), out var to) || to == from)
					continue;

				crossing[Math.Min(from, to) + 1]++;
				crossing[Math.Max(from, to) + 1]--;
			}
		}

		for (var i = 1; i < crossing.Length; i++)
			crossing[i] += crossing[i - 1];

		return crossing;
	}

	/// <summary>
	/// The position nearest <paramref name="wanted"/> that the fewest jumps cross.
	/// </summary>
	/// <remarks>
	/// Near rather than anywhere: a cut far from where the budget asked for one makes the
	/// parts uneven, and an uneven part is one closer to the limit. A quarter of a part
	/// either way is as far as it looks, and a clean cut inside that is the common case
	/// because the layout has already put the chains side by side.
	/// </remarks>
	static int Clearest(int[] crossing, int[] prefix, int wanted, int slack)
	{
		var best = wanted;

		for (var away = 1; away < prefix.Length; away++)
		{
			var moved = false;

			foreach (var at in new[] { wanted - away, wanted + away })
			{
				if (at <= 0 || at >= crossing.Length || Math.Abs(prefix[at] - prefix[wanted]) > slack)
					continue;

				moved = true;

				if (crossing[at] < crossing[best])
					best = at;

				if (crossing[best] == 0)
					return best;
			}

			if (!moved)
				break;
		}

		return best;
	}

	/// <summary>The first position at which the running total has reached a cost.</summary>
	static int Position(int[] prefix, int cost)
	{
		for (var i = 1; i < prefix.Length; i++)
			if (prefix[i] >= cost)
				return i;

		return prefix.Length - 1;
	}

	/// <summary>
	/// What a state is estimated to cost the compiler below, in basic blocks.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A basic block begins after every branch and at every place one can land, so counting
	/// what branches counts the blocks near enough: an <c>if</c>, each <c>&amp;&amp;</c> and
	/// <c>||</c> inside one — every short circuit is a branch of its own — every
	/// <c>goto</c>, every <c>case</c> of a dispatch, every <c>break</c>, a loop for the
	/// three blocks its condition and its step are, and a conditional expression for the
	/// two arms it is.
	/// </para>
	/// <para>
	/// Held against the real thing over both shapes there are: the recognizers, which are
	/// <c>if</c>/<c>goto</c>/<c>case</c>, read 3348 against 3918 measured, 6654 against
	/// 6518, 4932 against 4275; the materializer, which is loops and conditional
	/// expressions and read half its real count until those were counted, reads 2126
	/// against 2004. Within 15% either way, and mostly high — the direction that costs
	/// nothing, because it divides a little sooner than it had to.
	/// </para>
	/// </remarks>
	public static int Branches(string body)
	{
		var branches = 0;

		for (var i = 0; i < body.Length; i++)
			switch (body[i])
			{
				case '|' when i + 1 < body.Length && body[i + 1] == '|':
				case '&' when i + 1 < body.Length && body[i + 1] == '&':
					branches++;
					i++;

					break;

				// The two arms it chooses between. ` ? ` and not `?`, so that a nullable
				// annotation or a `??` in the same text says nothing here.
				case '?' when i > 0 && body[i - 1] == ' ' &&
					i + 1 < body.Length && body[i + 1] == ' ':
					branches += 2;

					break;

				// Condition, body, step: what falls out of a loop's shape however it runs.
				case 'f' when Word(body, i, "for ("):
				case 'w' when Word(body, i, "while ("):
					branches += 3;

					break;

				case 'i' when Word(body, i, "if ("):
				case 'g' when Word(body, i, "goto "):
				case 'c' when Word(body, i, "case "):
				case 'b' when Word(body, i, "break;"):
					branches++;

					break;
			}

		return branches;
	}

	/// <summary>Whether a word stands here, and is a word rather than the end of one.</summary>
	static bool Word(string body, int at, string word)
	{
		if (at + word.Length > body.Length ||
			at > 0 && (char.IsLetterOrDigit(body[at - 1]) || body[at - 1] == '_'))
		{
			return false;
		}

		for (var i = 0; i < word.Length; i++)
			if (body[at + i] != word[i])
				return false;

		return true;
	}
}
