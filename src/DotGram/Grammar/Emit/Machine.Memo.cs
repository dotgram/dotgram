using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;

namespace DotGram.Grammar.Emit;

/// <summary>
/// The memo of failures: a reader over tokens remembers where a rule that can reach itself has
/// failed, and answers a second entry there at once.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why.</b> A bracket in SQL can be a condition, a value or a subquery, and the reader finds
/// out which by trying them in turn. Each attempt that fails reads everything inside the bracket
/// before it fails, and the next attempt reads all of it again; a nest of brackets read that way
/// costs the cube of its depth, and almost every one of those entries is the same rule entered
/// at the same token after it has already failed there. One bit a rule and a token, set where the
/// rule fails and asked where it is entered, makes every such nest cost its depth.
/// </para>
/// <para>
/// <b>Which rules.</b> The ones the reader already probes the stack in (<see cref="Deepens"/>):
/// the entries of the grammar's recursion, which is where a reading can be repeated without bound.
/// A rule anywhere else is read at most a bounded number of times at a position, and asking a bit
/// there would cost every reading something to save nothing. A grammar without recursion has no
/// such rule, and its reader is written exactly as it was.
/// </para>
/// <para>
/// <b>What a bit may stand for.</b> Only a failure, and only one that a second reading would
/// repeat. Over tokens a rule's answer is a function of where it begins: its arguments are
/// written into it, its strength is a parameter (and only strength zero, where a bracket enters a
/// rule, is remembered), and the C# it runs answers from its arguments and the text (§7.2). So a
/// candidate is refused where that may not hold, decided over everything it reaches:
/// </para>
/// <list type="bullet">
/// <item><description><c>context</c> — a hook that names <c>context</c> or the reading's state,
/// which a reading writes as it goes and never puts back;</description></item>
/// <item><description><c>gives back</c> — a rule marked <c>?</c>, which can be asked for another
/// reading after it answered;</description></item>
/// <item><description><c>recover</c> — every rule of a machine that recovers: a recovery reads its
/// continuation and its element at one position and records what it rejected, which a remembered
/// failure would not record again;</description></item>
/// <item><description><c>buffered</c> — every rule of a machine reading a buffered input, where a
/// failure may be the buffer running out rather than the text;</description></item>
/// <item><description><c>characters</c> — every rule read over characters, where the lexer's
/// context or a window's end can make the same position read differently, and a literal of more
/// than one character over tokens, whose failure says the input ran out.</description></item>
/// </list>
/// <para>
/// <b>What a failure leaves behind.</b> Nothing on either carrier: the tape is put back to the
/// mark of whatever tried the rule, and the immediate carrier built nothing a failure keeps. What
/// it does leave is on the failure: the furthest position and what was expected there. A reading
/// that remembers a failure has already recorded it, and recording it again changes neither — with
/// one exception. Inside a look nothing is recorded, so a failure read inside one is not
/// remembered: met again outside the look, it has to be read to say what it expected.
/// </para>
/// <para>
/// <b>Where it lives.</b> On the <c>Ways</c> the reading rents, a word a token counted from where
/// the reading began, cleared over what the last reading used and kept under the pool's own
/// bound. Each reading clears it, so a parse that reads a refused input twice to say what it
/// expected reads it twice in full.
/// </para>
/// </remarks>
sealed partial class Machine
{
	/// <summary>
	/// Whether this machine's reader may remember where its rules failed
	/// (<see cref="GramCompilerOptions.MemoiseFailures"/>).
	/// </summary>
	internal bool MemoisesFailures;

	/// <summary>The rules whose failures the reader remembers, each with the number of its bit.</summary>
	readonly Dictionary<RuleSymbol, int> _memo = [];

	/// <summary>The candidates the reader does not remember, and why, for the report.</summary>
	readonly List<(RuleSymbol Rule, string Why)> _memoRefused = [];

	/// <summary>Whether the reader remembers any rule's failures.</summary>
	internal bool Memoises => _memo.Count > 0;

	/// <summary>The rules remembered, in the order of their bits.</summary>
	internal IEnumerable<RuleSymbol> Memoised => _memo.OrderBy(static one => one.Value).Select(static one => one.Key);

	/// <summary>The rules that probe the stack and are not remembered, each with its reason.</summary>
	internal IReadOnlyList<(RuleSymbol Rule, string Why)> MemoRefused => _memoRefused;

	/// <summary>Words a token: one for every sixty-four rules remembered.</summary>
	int MemoWords => (_memo.Count + 63) / 64;

	/// <summary>Which of the rules that probe the stack the reader remembers, and why not the rest.</summary>
	void ChooseMemo(IReadOnlyList<RuleSymbol> rules)
	{
		_memo.Clear();
		_memoRefused.Clear();

		if (!MemoisesFailures)
			return;

		var candidates = rules.Where(Deepens).ToList();

		if (candidates.Count == 0)
			return;

		var whole =
			!OverKinds ? "characters" :
			BufferedInput || BufferedBytes ? "buffered" :
			ReadsRecovery ? "recover" :
			null;

		var reach = whole is null ? MemoReach(rules) : null;

		foreach (var rule in candidates)
		{
			var why = whole ??
				(_opens is not null && _opens.Contains(rule) ? "gives back" :
				reach!.TryGetValue(rule, out var found) ? found : null);

			if (why is null)
				_memo[rule] = _memo.Count;
			else
				_memoRefused.Add((rule, why));
		}
	}

	/// <summary>
	/// For every rule the reader reaches, why its reading could depend on more than where it
	/// begins, or nothing where it could not: its own body, and everything it calls, to a fixpoint.
	/// </summary>
	Dictionary<RuleSymbol, string?> MemoReach(IReadOnlyList<RuleSymbol> rules)
	{
		var verdict = new Dictionary<RuleSymbol, string?>();
		var calls   = new Dictionary<RuleSymbol, List<RuleSymbol>>();
		var pending = new Stack<RuleSymbol>(rules);

		while (pending.Count > 0)
		{
			var rule = pending.Pop();

			if (verdict.ContainsKey(rule))
				continue;

			verdict[rule] = null;
			calls[rule]   = [];

			if (!_graph.Bodies.TryGetValue(rule, out var body))
				continue;

			foreach (var node in NodeWalk.Descendants(body))
			{
				if (verdict[rule] is null && MemoRefusal(node) is { } why)
					verdict[rule] = why;

				if (node is Node.Call(var called, _))
				{
					calls[rule].Add(called);
					pending.Push(called);
				}
			}
		}

		// A rule is what it calls: one refusal anywhere below is the rule's own.
		for (var changed = true; changed; )
		{
			changed = false;

			foreach (var pair in calls)
			{
				if (verdict[pair.Key] is not null)
					continue;

				foreach (var one in pair.Value)
				{
					if (verdict[one] is { } why)
					{
						verdict[pair.Key] = why;
						changed       = true;

						break;
					}
				}
			}
		}

		return verdict;
	}

	/// <summary>Why one node makes a reading depend on more than where it begins, or null.</summary>
	string? MemoRefusal(Node node)
	{
		switch (node)
		{
			// A `when`, and a `switch`'s selector, which is one.
			case Node.Guard guard when NamesState(guard.Text):
				return "context";

			// A `=>` runs during recognition on the immediate carrier, and on the tape wherever a
			// guard asks for its value.
			case Node.Construct { How: Construction.Expression expression } when NamesState(expression.Text):
				return "context";

			case Node.External { UsesContext: true }:
				return "context";

			// Over tokens a literal is one kind; one of more is read as characters, and the input
			// running out inside it is recorded beside the refusal.
			case Node.Literal { Text.Length: > 1 }:
				return "characters";

			default:
				return null;
		}
	}

	/// <summary>Whether C# a hook runs names what a reading writes as it goes.</summary>
	bool NamesState(string text)
	{
		return CSharpEmitter.Uses(_graph, text, "context") ||
			CSharpEmitter.Uses(_graph, text, "parserState") ||
			CSharpEmitter.Uses(_graph, text, "parserMarks");
	}

	/// <summary>Where the memo word of a rule entered at <c>pos</c> is, as C#.</summary>
	string MemoIndex(int slot)
	{
		return MemoWords == 1 ? "pos - memoAt" : $"(pos - memoAt) * {MemoWords} + {slot / 64}";
	}

	/// <summary>The bit of a rule in its word, as C#.</summary>
	static string MemoBit(int slot)
	{
		return "0x" + (1UL << (slot % 64)).ToString("X", System.Globalization.CultureInfo.InvariantCulture) + "UL";
	}

	/// <summary>
	/// The memo asked at the top of a remembered rule's method: failed here already, fail again.
	/// </summary>
	/// <remarks>
	/// Before the stack probe, since an answer that reads nothing needs no stack. Where entries are
	/// counted, the answer is one entry, as a reading would have been.
	/// </remarks>
	void MemoAsked(Writer file, RuleSymbol rule, int slot)
	{
		var index = MemoIndex(slot);
		var power = _graph.Climbing.ContainsKey(rule) ? "power == 0 && " : "";

		file.Line("// Failed here already in this reading, and a second reading would fail again (Machine.Memo.cs).");
		file.Line(
			$"if ({power}(uint)({index}) < (uint)memo.Length && " +
			$"(memo[{index}] & {MemoBit(slot)}) != 0)");

		if (CountsRules)
		{
			using (file.Block(""))
			{
				file.Line("#if DOTGRAM_COUNTS");
				file.Line($"{EnteredOf(rule)}++;");
				file.Line("#endif");
				file.Line();
				file.Line("return -1;");
			}
		}
		else
		{
			file.Then("return -1;");
		}

		file.Line();
	}

	/// <summary>
	/// A remembered rule's body: every failure goes through one place, which remembers it.
	/// </summary>
	/// <returns>False where the body never fails, and so has nothing to remember.</returns>
	bool MemoBody(Writer file, RuleSymbol rule, int slot, string body)
	{
		const string Refused = "return -1;";

		if (!body.Contains(Refused, StringComparison.Ordinal))
			return false;

		file.Write(body.Replace(Refused, "goto Failed;"));
		file.Line();
		file.Line("Failed:");

		var call = $"MemoFailed(pos, {slot / 64}, {MemoBit(slot)});";

		if (_graph.Climbing.ContainsKey(rule))
		{
			file.Line("if (power == 0)");
			file.Then(call);
		}
		else
		{
			file.Line(call);
		}

		file.Line();
		file.Line(Refused);

		return true;
	}

	/// <summary>The reader's two fields the remembered rules read, and where they come from.</summary>
	static void MemoFields(Writer header)
	{
		header.Line("/// <summary>Where each remembered rule has failed in this reading: a bit a rule, from <c>Ways.Memo</c>.</summary>");
		header.Line("readonly ulong[] memo;");
		header.Line("/// <summary>The token the memo's first word stands for: where the reading began.</summary>");
		header.Line("readonly int memoAt;");
	}

	/// <summary>The constructor's half: the memo is the tape's, and so travels with it to another stack.</summary>
	static void MemoTaken(Writer header)
	{
		header.Line("this.memo    = ways.Memo;");
		header.Line("this.memoAt  = ways.MemoAt;");
	}

	/// <summary>Where a remembered rule's failure is written down.</summary>
	/// <param name="looks">Whether anything this reader reads is read inside a look.</param>
	void MemoFailedMethod(Writer header, bool looks)
	{
		header.Line();
		header.Line("/// <summary>Remembers that a rule entered at <paramref name=\"pos\"/> failed there (Machine.Memo.cs).</summary>");

		using (header.Block("void MemoFailed(int pos, int word, ulong bit)"))
		{
			if (looks)
			{
				header.Line("// Nothing a look refuses is recorded; a reading outside it that met the bit would record");
				header.Line("// nothing either, and what was expected there would go unsaid.");
				header.Line("if (failure.Looking > 0)");
				header.Then("return;");
				header.Line();
			}

			header.Line($"var at = (pos - memoAt) * {MemoWords} + word;");
			header.Line();

			using (header.Block("if ((uint)at < (uint)memo.Length)"))
			{
				header.Line("memo[at] |= bit;");
				header.Line();
				header.Line("if (at >= ways.MemoUsed)");
				header.Then("ways.MemoUsed = at + 1;");
			}
		}
	}

	/// <summary>The entry's half: what an earlier reading remembered is not this one's.</summary>
	void MemoBegun(Writer file)
	{
		file.Line("// What an earlier reading remembered is not this one's: each reading begins with nothing failed.");
		file.Line($"ways.Memoise(pos, (text.Length - pos + 1) * {MemoWords});");
	}
}
