using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;

namespace DotGram.Grammar.Emit;

/// <summary>
/// The memo of failures: a reader over tokens remembers where a rule that can reach itself has
/// failed, and answers a second entry there at once. And the memo of successes beside it, for the
/// rules of a recursion an ordered choice reads twice before it decides: where such a rule
/// succeeded, and what it left.
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
/// rule, is remembered), and the C# it runs answers from its arguments and the text (§7.2) — and
/// so does a construction whose value a guard reads, which runs during recognition too. That is a
/// contract and not something this file can check: C# that reads state other than by naming
/// <c>context</c> (a static field, say) is not seen, and a rule that reaches it is remembered all
/// the same. What can be seen is refused, decided over everything a candidate reaches:
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
/// <para>
/// <b>Successes.</b> A failure remembered answers the shape where a bracket is tried as one thing
/// after another and each attempt fails inside it. It does not answer the shape where an attempt
/// <i>succeeds</i> through the nest and is refused only after it: an ordered choice whose
/// alternatives share a prefix holding a nested instance of the recursion they stand in, and
/// whose deciding token comes after that instance — <c>(CASE WHEN … END = 1)</c> read as a value
/// in brackets, refused at <c>=</c>, and read again as a condition in brackets, at every level:
/// two to the depth. <see cref="SharedRecursion"/> finds those choices at generation time, and
/// for the rules of the cycle each stands in the reader remembers successes too: where the rule
/// ended and the value it left, three integers a rule and a token, stamped with the reading's
/// generation rather than cleared. A second entry at a remembered place hands the value over and
/// returns the end, and every level of the nest is read once. Only on the immediate carrier,
/// whose values are objects a register hands on: the tape numbers its records by where they were
/// written, and a reading given up has unwound them. And only for the rules the failure memo
/// remembers, under the same contract (a rule's answer is a function of where it begins), at
/// strength zero, outside a look, and where the reading builds: a success read unbuilt left no
/// value to hand over. What it costs is a store a success on the accepted path, measured at a few
/// per cent of a statement on SQL-92, and 12 bytes a token a rule.
/// </para>
/// </remarks>
sealed partial class Machine
{
	/// <summary>
	/// Whether this machine's reader may remember where its rules failed
	/// (<see cref="GramCompilerOptions.MemoiseFailures"/>).
	/// </summary>
	internal bool MemoisesFailures;

	/// <summary>
	/// Whether this machine's reader may remember where its rules succeeded as well
	/// (<see cref="GramCompilerOptions.MemoiseSuccesses"/>).
	/// </summary>
	internal bool MemoisesSuccesses;

	/// <summary>
	/// The choices the shared-recursion analysis found in the grammar, each with the cycle it stands
	/// in (<see cref="SharedRecursion"/>): what decides which rules' successes are remembered. Null
	/// where the analysis was not run.
	/// </summary>
	internal IReadOnlyList<SharedRecursion.Pair>? SharedChoices;

	/// <summary>The rules whose failures the reader remembers, each with the number of its bit.</summary>
	readonly Dictionary<RuleSymbol, int> _memo = [];

	/// <summary>The rules whose successes the reader remembers too, each with the number of its cell.</summary>
	readonly Dictionary<RuleSymbol, int> _kept = [];

	/// <summary>The candidates the reader does not remember, and why, for the report.</summary>
	readonly List<(RuleSymbol Rule, string Why)> _memoRefused = [];

	/// <summary>Whether the reader remembers any rule's failures.</summary>
	internal bool Memoises => _memo.Count > 0;

	/// <summary>The rules remembered, in the order of their bits.</summary>
	internal IEnumerable<RuleSymbol> Memoised => _memo.OrderBy(static one => one.Value).Select(static one => one.Key);

	/// <summary>The rules that probe the stack and are not remembered, each with its reason.</summary>
	internal IReadOnlyList<(RuleSymbol Rule, string Why)> MemoRefused => _memoRefused;

	/// <summary>Whether the reader remembers any rule's successes.</summary>
	internal bool Keeps => _kept.Count > 0;

	/// <summary>The rules whose successes are remembered, in the order of their cells.</summary>
	internal IEnumerable<RuleSymbol> KeptRules => _kept.OrderBy(static one => one.Value).Select(static one => one.Key);

	/// <summary>The rules of a cycle whose successes this reader remembers, in the order of their cells.</summary>
	internal List<RuleSymbol> KeptOf(IReadOnlyList<RuleSymbol> cycle)
	{
		return KeptRules.Where(cycle.Contains).ToList();
	}

	/// <summary>Why this reader remembers no success of a cycle's rules, or null where it reads none of them.</summary>
	internal string? WhyNotKept(IReadOnlyList<RuleSymbol> cycle)
	{
		if (_directRules is null || !cycle.Any(_directRules.Contains))
			return null;

		if (!MemoisesSuccesses)
			return "the memo of successes is off (DOTGRAM_NO_SUCCESS_MEMO)";

		if (Tracing is not null)
			return "a trace build enters and leaves every rule it reads";

		if (Carrier is not ImmediateCarrier)
			return "the rules are read on the tape, whose records a reading given up has unwound";

		if (!OverKinds)
			return "the rules are read over characters";

		if (BufferedInput || BufferedBytes)
			return "the rules read a buffered input";

		if (ReadsRecovery)
			return "the reader recovers";

		var refused = _memoRefused.Where(one => cycle.Contains(one.Rule)).Select(static one => one.Why).Distinct().ToList();

		return refused.Count > 0
			? "a rule of the recursion " + string.Join(", ", refused.Select(static why => why switch
			{
				"context"    => "reaches a hook that names its context",
				"gives back" => "is marked '?' and can be asked for another reading",
				"characters" => "reads a literal of more than one character",
				_            => why,
			}))
			: "no rule of the recursion is an entry of it that the failure memo remembers";
	}

	/// <summary>Whether the reader could remember successes at all: over tokens, immediately carried, not traced.</summary>
	/// <remarks>
	/// A trace build says what it entered and left, and an entry answered from the memo would be a
	/// frame it never opened; the tape's records are unwound with the reading that wrote them.
	/// </remarks>
	internal bool CouldKeep => MemoisesSuccesses && Memoises && Tracing is null && Carrier is ImmediateCarrier;

	/// <summary>Whether anything this machine's reader writes is read inside a look: set where a look is written.</summary>
	/// <remarks>
	/// Kept as the reader is written rather than asked of the text afterwards: the text of a large
	/// grammar's reader is fifteen megabytes in a builder of thousands of chunks, and a search of it a
	/// character at a time cost seconds of the generator's time.
	/// </remarks>
	bool _looks;

	/// <summary>Words a token: one bit for every rule remembered, and one more for every rule whose successes are, sixty-four to a word.</summary>
	int MemoWords => (_memo.Count + _kept.Count + 63) / 64;

	/// <summary>The bit that says a rule's success is remembered at a token: after the failures' bits, in the same words.</summary>
	int KeptSlot(int cell)
	{
		return _memo.Count + cell;
	}

	/// <summary>
	/// The shortest reading worth remembering, in tokens. A rule read again over one or two tokens costs
	/// what the store would, and the nest the memo is for re-reads its longer levels: the innermost
	/// ones are read once per alternative, a bounded number of times, and every level above them is
	/// answered. So the hot, short successes — a name, a literal, a bracket around one — are not stored.
	/// </summary>
	const int KeptSpan = 4;

	/// <summary>Which of the rules that probe the stack the reader remembers, and why not the rest.</summary>
	void ChooseMemo(IReadOnlyList<RuleSymbol> rules)
	{
		_memo.Clear();
		_memoRefused.Clear();
		_kept.Clear();

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

		ChooseKept();
	}

	/// <summary>
	/// Which of the remembered rules remember their successes too: for every choice that reads a
	/// nested instance of its cycle before it decides (<see cref="SharedRecursion"/>, levels 2 and
	/// 3), the first rule of the cycle both alternatives enter at one token that the failure memo
	/// remembers — from that entry on the two readings are one reading, and a second entry there is
	/// answered without the nest being read — or, where no such rule is remembered, every
	/// remembered rule of the cycle, each of which some way round it passes.
	/// </summary>
	/// <remarks>
	/// The fewest rules that answer the nest, because every rule remembered costs a store at each
	/// of its successes on the accepted path: the whole cycle read +13 % to +34 % on SQL:2023's
	/// nests of brackets, the one rule both alternatives enter does not.
	/// </remarks>
	void ChooseKept()
	{
		if (!CouldKeep || SharedChoices is null)
			return;

		var kept = new HashSet<RuleSymbol>();

		foreach (var pair in SharedChoices)
		{
			if (pair.Level < SharedRecursion.Remembered)
				continue;

			var found = false;

			foreach (var joint in pair.Joint)
			{
				foreach (var rule in joint)
					if (_memo.ContainsKey(rule))
					{
						kept.Add(rule);
						found = true;
					}

				if (found)
					break;
			}

			if (!found)
				foreach (var rule in pair.Cycle)
					if (_memo.ContainsKey(rule))
						kept.Add(rule);
		}

		foreach (var rule in Memoised)
			if (kept.Contains(rule))
				_kept[rule] = _kept.Count;
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
				if (verdict[rule] is null && MemoRefusal(rule, node) is { } why)
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

	/// <summary>Why one node of a rule makes its reading depend on more than where it begins, or null.</summary>
	string? MemoRefusal(RuleSymbol rule, Node node)
	{
		switch (node)
		{
			// A `when`, and a `switch`'s selector, which is one.
			case Node.Guard guard when NamesState(rule, guard.Text):
				return "context";

			// A `=>` runs during recognition on the immediate carrier, and on the tape wherever a
			// guard asks for its value.
			case Node.Construct { How: Construction.Expression expression } when NamesState(rule, expression.Text):
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

	/// <summary>Whether C# a hook in a rule runs names what a reading writes as it goes.</summary>
	/// <remarks>
	/// <c>context</c> is the reading's only where the rule has one declared; elsewhere it is a name
	/// like any other — T-SQL's <c>EXECUTE</c> captures its <c>context</c> clause under it.
	/// </remarks>
	bool NamesState(RuleSymbol rule, string text)
	{
		return _graph.ContextOf(rule) is not null && CSharpEmitter.Uses(_graph, text, "context") ||
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
	/// <param name="body">The rule's body as rendered, which says whether it ever succeeds and whether it is read unbuilt.</param>
	void MemoAsked(Writer file, RuleSymbol rule, int slot, string body)
	{
		var index = MemoIndex(slot);
		var power = _graph.Climbing.ContainsKey(rule) ? "power == 0 && " : "";

		if (_kept.TryGetValue(rule, out var cell) && body.Contains("return p;", StringComparison.Ordinal))
		{
			KeptAsked(file, rule, slot, cell, body);

			return;
		}

		file.Line("// Failed here already in this reading, and a second reading would fail again (Machine.Memo.cs).");
		file.Line(
			$"if ({power}(uint)({index}) < (uint)memo.Length && " +
			$"(memo[{index}] & {MemoBit(slot)}) != 0)");

		if (CountsRules || Tracing is not null)
		{
			using (file.Block(""))
			{
				if (CountsRules)
				{
					file.Line("#if DOTGRAM_COUNTS");
					file.Line($"{EnteredOf(rule)}++;");
					file.Line("#endif");
					file.Line();
				}

				// Answered without being read: neither entered nor left, and said so.
				if (Tracing is not null)
					file.Line($"failure.Trace?.Remembered({Tracing.RuleOf(rule)}, pos);");

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
	/// <remarks>
	/// A trace build's body was written sending its failures here already (ReaderWriter.Fails),
	/// each closing the frames open where it stood, and the rule is left here once.
	/// </remarks>
	bool MemoBody(Writer file, RuleSymbol rule, int slot, string body)
	{
		const string Refused   = "return -1;";
		const string Succeeded = "return p;";

		if (!body.Contains(Tracing is null ? Refused : "goto Failed;", StringComparison.Ordinal))
			return false;

		// A success leaves through one place too, where it is remembered: every `return p;` of the
		// body is the rule succeeding at p, and a body with none never succeeds.
		var kept = _kept.TryGetValue(rule, out var cell) && body.Contains(Succeeded, StringComparison.Ordinal);

		file.Write(Tracing is null ? (kept ? body.Replace(Refused, "goto Failed;").Replace(Succeeded, "goto Kept;") : body.Replace(Refused, "goto Failed;")) : body);
		file.Line();

		if (kept)
			KeptLeft(file, rule, cell, body);

		file.Line("Failed:");

		var call = $"MemoFailed(pos, {slot / 64}, {MemoBit(slot)});";

		// `power` is the strength the rule was entered at, as at the top of the method: the
		// ceiling a climbing rule carries across its turns is a local of its own (Machine.Ceils)
		// and never folded into it, so a failure after a turn has set one is still remembered.
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
		file.Line(Tracing is null ? Refused : $"return Exited_DotGram({Tracing.RuleOf(rule)}, pos, -1);");

		return true;
	}

	/// <summary>The reader's fields the remembered rules read, and where they come from.</summary>
	void MemoFields(Writer header)
	{
		header.Line("/// <summary>Where each remembered rule has failed in this reading: a bit a rule, from <c>Ways_DotGram.Memo</c>.</summary>");
		header.Line("readonly ulong[] memo;");
		header.Line("/// <summary>The token the memo's first word stands for: where the reading began.</summary>");
		header.Line("readonly int memoAt;");

		if (Keeps)
		{
			header.Line("/// <summary>Where each remembered rule's successes ended and what they left: two integers a token, a run of them a rule, from <c>Ways_DotGram.Kept</c>.</summary>");
			header.Line("readonly int[] kept;");
			header.Line("/// <summary>How long one rule's run is: two integers a token of the reading.</summary>");
			header.Line("readonly int keptRun;");
		}
	}

	/// <summary>The constructor's half: the memo is the tape's, and so travels with it to another stack.</summary>
	void MemoTaken(Writer header)
	{
		header.Line("this.memo    = ways.Memo;");
		header.Line("this.memoAt  = ways.MemoAt;");

		if (Keeps)
		{
			header.Line("this.kept    = ways.Kept;");
			header.Line("this.keptRun = ways.KeptRun;");
		}
	}

	/// <summary>Where a remembered rule's failure is written down.</summary>
	/// <param name="looks">Whether anything this reader reads is read inside a look.</param>
	void MemoFailedMethod(Writer header, bool looks)
	{
		if (Keeps)
			KeptMethod(header);

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

		if (Keeps)
			file.Line($"ways.KeepRoom({_kept.Count}, text.Length - pos + 1);");
	}

	/// <summary>The register a remembered rule's value is handed through, or null where it has none.</summary>
	string? KeptRegister(RuleSymbol rule)
	{
		return Valued(rule) ? Carrier.Last(rule) : null;
	}

	/// <summary>Whether a rule's body can be read unbuilt — inside a look, or after a loop gave a way back twice — where its construction does not run.</summary>
	static bool ReadUnbuilt(string body)
	{
		return body.Contains("unbuilt", StringComparison.Ordinal);
	}

	/// <summary>
	/// The memo asked at the top of a rule whose successes are remembered too: the word of its two
	/// bits read once — failed here already, fail again; succeeded here already, hand over what it
	/// left and return where it ended.
	/// </summary>
	/// <remarks>
	/// A success read unbuilt left no value, and is remembered as that: a reading that builds
	/// reads the rule again rather than take it, and a reading that does not build takes any.
	/// So the probe answers in every reading, and a nest the reader reads unbuilt — a refused
	/// text read again for its message, behind a look — costs its depth as an accepted one does.
	/// </remarks>
	void KeptAsked(Writer file, RuleSymbol rule, int slot, int cell, string body)
	{
		var register = KeptRegister(rule);
		var unbuilt  = register is not null && ReadUnbuilt(body);
		var power    = _graph.Climbing.ContainsKey(rule) ? "power == 0 && " : "";
		var built    = unbuilt ? " && (unbuilt != 0 || kept[keptAt + 1] != -2)" : "";
		var index    = MemoIndex(slot);
		var sameWord = slot / 64 == KeptSlot(cell) / 64;

		file.Line("// Failed here already in this reading, and a second reading would fail again; succeeded here");
		file.Line("// already, and the same reading would end at the same place with the same value (Machine.Memo.cs).");

		using (file.Block($"if ({power}(uint)({index}) < (uint)memo.Length)"))
		{
			file.Line($"var word = memo[{index}];");
			file.Line();
			file.Line($"if ((word & {MemoBit(slot)}) != 0)");

			if (CountsRules || Tracing is not null)
			{
				using (file.Block(""))
				{
					if (CountsRules)
					{
						file.Line("#if DOTGRAM_COUNTS");
						file.Line($"{EnteredOf(rule)}++;");
						file.Line("#endif");
						file.Line();
					}

					if (Tracing is not null)
						file.Line($"failure.Trace?.Remembered({Tracing.RuleOf(rule)}, pos);");

					file.Line("return -1;");
				}
			}
			else
			{
				file.Then("return -1;");
			}

			file.Line();
			file.Line($"var keptAt = {cell} * keptRun + (pos - memoAt) * 2;");
			file.Line();

			using (file.Block($"if (({(sameWord ? "word" : $"memo[{MemoIndex(KeptSlot(cell))}]")} & {MemoBit(KeptSlot(cell))}) != 0{built})"))
			{
				if (CountsRules)
				{
					file.Line("#if DOTGRAM_COUNTS");
					file.Line($"{EnteredOf(rule)}++;");
					file.Line("#endif");
					file.Line();
				}

				if (register is not null)
					file.Line($"{(unbuilt ? "if (unbuilt == 0) " : "")}{register} = kept[keptAt + 1] >= 0 ? ({_results.ValueOf(rule)})ways.KeptValues[kept[keptAt + 1]]! : default!;");

				file.Line("return kept[keptAt];");
			}
		}

		file.Line();
	}

	/// <summary>The one place a remembered rule's body succeeds through: the success written down, then returned.</summary>
	/// <remarks>
	/// At the strength the rule was entered at, as at the top of the method (MemoBody says why); and
	/// not inside a look, where nothing it refused on the way was recorded, so that a reading outside
	/// the look, which must record it, reads it.
	/// </remarks>
	void KeptLeft(Writer file, RuleSymbol rule, int cell, string body)
	{
		var register = KeptRegister(rule);
		var when     = new List<string>();

		if (_graph.Climbing.ContainsKey(rule))
			when.Add("power == 0");

		if (_looks)
			when.Add("failure.Looking == 0");

		var value = register is null
			? "null, true"
			: ReadUnbuilt(body) ? $"(object?){register}, unbuilt == 0" : $"(object?){register}, true";

		when.Add($"p - pos >= {KeptSpan}");

		file.Line("Kept:");
		file.Line($"if ({string.Join(" && ", when)}) Kept(pos, {KeptSlot(cell) / 64}, {MemoBit(KeptSlot(cell))}, {cell}, p, {value});");
		file.Line("return p;");
		file.Line();
	}

	/// <summary>Where a remembered rule's success is written down.</summary>
	void KeptMethod(Writer header)
	{
		header.Line();
		header.Line("/// <summary>Remembers that a rule entered at <paramref name=\"pos\"/> succeeded there, where it ended and what it left (Machine.Memo.cs).</summary>");
		header.Line("/// <param name=\"built\">Whether the reading built: read unbuilt, the rule left no value, and the cell says so (-2) for a reading that builds to read it again.</param>");

		using (header.Block("void Kept(int pos, int word, ulong bit, int cell, int end, object? value, bool built)"))
		{
			header.Line($"var at = (pos - memoAt) * {MemoWords} + word;");
			header.Line();

			using (header.Block("if ((uint)at < (uint)memo.Length)"))
			{
				header.Line("memo[at] |= bit;");
				header.Line();
				header.Line("if (at >= ways.MemoUsed)");
				header.Then("ways.MemoUsed = at + 1;");
				header.Line();
				header.Line("var keptAt = cell * keptRun + (pos - memoAt) * 2;");
				header.Line();
				header.Line("kept[keptAt]     = end;");
				header.Line("kept[keptAt + 1] = !built ? -2 : value == null ? -1 : ways.Keep(value);");
			}
		}
	}
}
