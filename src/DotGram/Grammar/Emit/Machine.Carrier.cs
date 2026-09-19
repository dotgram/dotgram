using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;

namespace DotGram.Grammar.Emit;

sealed partial class Machine
{
	/// <summary>Which carrier the host asked for; what it gets is <see cref="Carrier"/>.</summary>
	readonly CarrierKind _carrierKind;

	/// <summary>
	/// Which rules of the graph are read where the reading may not stand, asked once for the
	/// file and handed to every machine that chooses its own carrier; null where none does.
	/// </summary>
	readonly Replay.Report? _replay;

	/// <summary>Whose value is ever built, asked of the graph the first time a carrier needs it (<see cref="Demand"/>).</summary>
	Demand.Report Demands => field ??= Demand.Of(_graph);

	/// <summary>Why the carrier asked for was not the one used, or null.</summary>
	public string? CarrierRefusal { get; private set; }

	/// <summary>What kept a machine left to choose on the tape where the immediate carrier could have carried it.</summary>
	/// <param name="Building">The rules it builds.</param>
	/// <param name="Replayed">Those of them read for derivations that may not stand, the ones to look at first.</param>
	/// <param name="Again">The rules that can be read again after answering, where nothing was replayed.</param>
	internal sealed record Kept(
		IReadOnlyList<RuleSymbol> Building, IReadOnlyList<RuleSymbol> Replayed, IReadOnlyList<RuleSymbol> Again);

	/// <summary>
	/// Why <see cref="CarrierKind.Auto"/> kept this machine on the tape where the immediate
	/// carrier could have carried it, or null: where it chose that carrier, and where there
	/// was nothing to choose between. Said once for a file, however many machines it has
	/// (CSharpEmitter), so it is kept as the rules and not as a sentence.
	/// </summary>
	internal Kept? KeptOnTape { get; private set; }

	/// <summary>
	/// Why the immediate carrier refused a machine left to choose, and what the gates would have
	/// said had it not: the carriers report's, and nothing a diagnostic says (GRAM5012 is silent
	/// where the carrier refuses). Null where it did not refuse, or where there was nothing to choose.
	/// </summary>
	/// <param name="Why">The immediate carrier's refusal, the first it found.</param>
	/// <param name="Otherwise">What would have kept the machine on the tape all the same: empty lists where nothing would.</param>
	internal sealed record RefusedByCarrier(string Why, Kept Otherwise);

	/// <summary>The refusal, where there was one (<see cref="RefusedByCarrier"/>).</summary>
	internal RefusedByCarrier? RefusedOnTape { get; private set; }

	/// <summary>
	/// Whether a build asked for the carriers report: then <see cref="OpenedHere"/> is kept, and
	/// otherwise nothing is.
	/// </summary>
	internal bool Reporting;

	/// <summary>The rules whose own reading opened a way back, before their callers were added: the report's.</summary>
	internal HashSet<RuleSymbol>? OpenedHere { get; private set; }

	/// <summary>
	/// Where each of <see cref="OpenedHere"/> opened its way, as the report says it: the shape of
	/// the node, why the way could not be left out, and the node. Kept where the report was asked
	/// for and nowhere else.
	/// </summary>
	internal Dictionary<RuleSymbol, List<string>>? OpenedAt { get; private set; }

	/// <summary>A choice that opens a way, noted for the report where it was asked for.</summary>
	void OpeningChoice(RuleSymbol rule, IReadOnlyList<Node> alternatives, FollowSets.Continuation following)
	{
		if (Reporting)
			Opening(rule, "choice", WhyChoice(alternatives, following.Plain), new Node.Choice([.. alternatives]));
	}

	/// <summary>A repetition that opens a way, noted for the report where it was asked for.</summary>
	void OpeningRepeat(RuleSymbol rule, Node.Repeat repeat, FollowSets.Continuation following, bool run)
	{
		if (!Reporting)
			return;

		var captured = NodeWalk.Descendants(repeat.Body).Any(static one => one is Node.Capture) ? ", captured" : "";

		var shape = run ? "run" : repeat.Max == 1 ? "optional" : repeat.Max is not null ? "counted" : "turns";

		Opening(rule, shape + captured, WhyRepeat(repeat, following), repeat);
	}

	void Opening(RuleSymbol rule, string form, string why, Node node)
	{
		OpenedAt ??= [];

		if (!OpenedAt.TryGetValue(rule, out var sites))
			OpenedAt[rule] = sites = [];

		// One line of a comment: a literal's line break is shown the way the grammar writes it.
		var shown = Display(node).Replace("\r", "\\r").Replace("\n", "\\n");

		if (shown.Length > 70)
			shown = shown.Substring(0, 70) + "…";

		var line = $"{form}; {why}; {CalledAs(rule)}; {shown}";

		if (!sites.Contains(line))
			sites.Add(line);
	}

	/// <summary>
	/// How the rule is called: <c>entry</c> where no rule calls it, <c>sealed</c> where every call
	/// stands inside an atomic group or a lookahead — whose answer is committed, so nothing asks the
	/// rule again — and <c>open</c> otherwise.
	/// </summary>
	string CalledAs(RuleSymbol rule)
	{
		var open   = 0;
		var closed = 0;

		foreach (var body in _graph.Bodies.Values)
			Count(body, committed: false);

		return open + closed == 0 ? "entry" : open == 0 ? "sealed" : "open";

		void Count(Node node, bool committed)
		{
			if (node is Node.Call(var called, _) && ReferenceEquals(called, rule))
			{
				if (committed)
					closed++;
				else
					open++;
			}

			var inside = committed || node is Node.Atomic or Node.Lookahead;

			switch (node)
			{
				case Node.Atomic one:    Count(one.Body, inside); break;
				case Node.Marked one:    Count(one.Body, inside); break;
				case Node.Repeat one:    Count(one.Body, inside); break;
				case Node.Lookahead one: Count(one.Body, inside); break;
				case Node.Capture one:   Count(one.Body, inside); break;
				case Node.Construct one: Count(one.Body, inside); break;

				default:
					foreach (var child in node.Children)
						Count(child, inside);
					break;
			}
		}
	}

	/// <summary>Whether every way into a node reads the seam first: what a rewrite could take out in front of it.</summary>
	bool LeadsWithSeam(Node node) => node switch
	{
		Node.Capture(_, var body)   => LeadsWithSeam(body),
		Node.Construct(var body, _) => LeadsWithSeam(body),
		Node.Sequence(var parts)    => parts.Count > 0 && LeadsWithSeam(parts[0]),
		Node.Choice(var options)    => options.Count > 0 && options.All(LeadsWithSeam),
		Node.Call(var called, _)    => _seam is { } seam && ReferenceEquals(called, seam),
		_                           => false,
	};

	/// <summary>A node without the captures and constructions around it.</summary>
	static Node Bare(Node node) => node switch
	{
		Node.Capture(_, var body)   => Bare(body),
		Node.Construct(var body, _) => Bare(body),
		_                           => node,
	};

	/// <summary>Whether a node begins with a part that may read nothing, looking through a few calls.</summary>
	bool LedByNothing(Node node, int depth) => node switch
	{
		Node.Capture(_, var body)   => LedByNothing(body, depth),
		Node.Construct(var body, _) => LedByNothing(body, depth),
		Node.Marked(var body, _)    => LedByNothing(body, depth),
		Node.Sequence(var parts)    => parts.Count > 1 &&
		                               (FirstSets.Nullable(parts[0], _graph) || LedByNothing(parts[0], depth)),
		Node.Choice(var options)    => options.Count > 0 && options.All(one => LedByNothing(one, depth)),
		Node.Call(var called, _)    => depth > 0 && _graph.Bodies.TryGetValue(called, out var body) &&
		                               LedByNothing(body, depth - 1),
		_                           => false,
	};

	/// <summary>
	/// Why a choice over characters keeps a way: what <c>LiteralRun</c> could not settle, said
	/// by what the alternatives begin with against each other and against what follows.
	/// </summary>
	string WhyChoice(IReadOnlyList<Node> alternatives, FirstSets.First following)
	{
		if (alternatives.All(static one => one is Node.Literal { IgnoreCase: false }))
			return !following.IsKnown ? "literals, follow unknown" : "literals, a shorter one wanted";

		if (alternatives.Any(static one => one is Node.Literal { IgnoreCase: true }))
			return "an ignore-case literal";

		var empty = alternatives[alternatives.Count - 1] is Node.Empty;
		var begun = alternatives.Where(static one => one is not Node.Empty).ToList();

		if (begun.Exists(one => FirstSets.Nullable(one, _graph)))
			return "an alternative that may read nothing";

		var firsts = begun.Select(one => FirstSets.Of(one, _graph)).ToList();

		for (var i = 0; i < firsts.Count; i++)
			for (var j = i + 1; j < firsts.Count; j++)
				if (firsts[i].Overlaps(firsts[j]))
					return begun.All(LeadsWithSeam) ? "the seam leads every alternative"
						: begun.All(one => LedByNothing(one, 3)) ? "every alternative led by what may read nothing"
						: begun.All(static one => Bare(one) is Node.Literal { IgnoreCase: false }) ? "literals under a capture or construction"
						: "alternatives begin alike";

		if (!empty)
			return "alternatives begin apart";

		if (!following.IsKnown)
			return "optional, follow unknown";

		return firsts.Exists(one => one.Overlaps(following))
			? "optional, what follows begins alike"
			: "optional, what follows begins apart";
	}

	/// <summary>Why a repetition keeps a way: the question <see cref="Determinism.NeverGivesBack"/> answered no to.</summary>
	string WhyRepeat(Node.Repeat repeat, FollowSets.Continuation following)
	{
		var body = repeat.Body;

		if (FirstSets.Nullable(body, _graph))
			return "a turn that may read nothing";

		if (_seam is { } seam && body is Node.Sequence(var parts) && parts.Count > 1 &&
			parts[0] is Node.Call(var called, _) && ReferenceEquals(called, seam))
		{
			var rest = parts.Count == 2 ? parts[1] : new Node.Sequence([.. parts.Skip(1)]);

			if (FirstSets.Nullable(rest, _graph))
				return "seam first, the rest may read nothing";

			if (!following.AfterSeam.IsKnown)
				return "seam first, follow unknown";

			return FirstSets.Of(rest, _graph).Overlaps(following.AfterSeam)
				? "seam first, what follows begins alike past it"
				: "seam first, what follows can begin inside it";
		}

		// The seam in front of every alternative of the turn rather than of the turn: NeverGivesBack
		// compares past it only where it leads the sequence.
		if (LeadsWithSeam(body))
			return "the seam leads every alternative of the turn";

		if (!following.Plain.IsKnown)
			return "follow unknown";

		// A turn guarded by a lookahead, `(?!"*/" & any)*`: the lookahead says where the turns stop,
		// and what it refuses is usually what follows the loop.
		if (Bare(body) is Node.Sequence(var guarded) && guarded.Count > 1 && guarded[0] is Node.Lookahead)
			return "a turn led by a lookahead";

		// A seam of the turn's own: something that may read nothing — `Ows`, `Fws?` — in front
		// of what decides, which what follows the loop usually begins with as well.
		if (LedByNothing(body, 3))
			return "a turn led by what may read nothing";

		return "what follows begins alike";
	}

	/// <summary>The tape a machine choosing its carrier reads with until it knows enough to choose.</summary>
	TapeCarrier? _provisional;

	/// <summary>What a machine choosing its carrier chose, once it has.</summary>
	ValueCarrier? _chosen;

	/// <summary>
	/// What <see cref="CarrierKind.Auto"/> settles on, once a first reading of the rules has
	/// said which of them can be read again after answering.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The immediate carrier where every rule this machine builds is read only for the
	/// derivation that stands or for one the whole parse fails on (<see cref="Replay"/>), and
	/// where no rule opens a way back. The graph cannot see a way back — an alternative that
	/// answered being asked for the next one, a turn given back — which is why the report
	/// alone is not enough and this waits for the reader to have been written once.
	/// </para>
	/// <para>
	/// Otherwise the tape it has been reading with. The reason is kept only where the
	/// immediate carrier could have been asked for instead: a machine that builds nothing has
	/// nothing to carry, and one that carrier would refuse is not offered it.
	/// </para>
	/// </remarks>
	/// <param name="opens">Every rule written with a way back: those that open one, and their callers.</param>
	/// <param name="ownWays">Those that open one themselves.</param>
	void Choose(IReadOnlyList<RuleSymbol> rules, HashSet<RuleSymbol> opens, HashSet<RuleSymbol> ownWays)
	{
		if (_carrierKind != CarrierKind.Auto || _chosen is not null)
			return;

		var tape      = _provisional ??= new TapeCarrier(this);
		var immediate = new ImmediateCarrier(this);
		var building  = rules.Where(rule => _results.QualifiedOf(rule) is not null).ToList();

		_chosen = tape;

		if (building.Count == 0 || _replay is null)
			return;

		// Refused before any gate is asked, as it always was. Where the report is asked for, what the
		// gates would have said is kept beside the refusal, so that it says whether the refusal is
		// all that holds the machine back; otherwise nothing more is asked.
		if (immediate.Refuses() is { } refused)
		{
			if (Reporting)
				RefusedOnTape = new RefusedByCarrier(refused, new Kept(building, Replayed(), ReadAgain(rules, opens, ownWays)));

			return;
		}

		var replayed = Replayed();

		if (replayed.Count > 0)
		{
			KeptOnTape = new Kept(building, replayed, []);

			return;
		}

		var again = ReadAgain(rules, opens, ownWays);

		if (again.Count > 0)
		{
			KeptOnTape = new Kept(building, [], again);

			return;
		}

		_chosen = immediate;

		List<RuleSymbol> Replayed() => building
			.Where(rule => !_replay.Keeps(rule))
			.OrderBy(rule => _replay.Rules.TryGetValue(rule, out var because) && because == Replay.Because.Under ? 1 : 0)
			.ToList();
	}

	/// <summary>
	/// The rules a caller can ask again after they answered: those with a way to give, that are
	/// called where the way stays open.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A rule has a way to give where it opened one itself, or where it calls, outside an atomic
	/// group and a lookahead, a rule that has. And it can be asked again where a caller can come
	/// back into it: from an entry, which asks again until the input is read to its end, or from
	/// a call outside an atomic group and a lookahead. Both of those seal the ways opened inside
	/// them once they have answered (<c>ways.Seal</c>), so nothing behind them is asked again.
	/// </para>
	/// <para>
	/// <c>opens</c> said less: every rule with a way, and every rule calling one, which is what
	/// the reader needs to know to write the loops. A comment read inside `trivia = { … }` has
	/// its way, and nobody comes back into it.
	/// </para>
	/// </remarks>
	List<RuleSymbol> ReadAgain(IReadOnlyList<RuleSymbol> rules, HashSet<RuleSymbol> opens, HashSet<RuleSymbol> ownWays)
	{
		var open   = new Dictionary<RuleSymbol, List<RuleSymbol>>();
		var called = new HashSet<RuleSymbol>();

		foreach (var rule in rules)
			if (_graph.Bodies.TryGetValue(rule, out var body))
				OpenCalls(body, rule, committed: false);

		// A way to give, spread from the rules that open one to those that call them openly. A rule
		// that is an atomic group throughout seals what it opened as it answers.
		var gives = new HashSet<RuleSymbol>(ownWays.Where(rule =>
			!_graph.Bodies.TryGetValue(rule, out var body) || Unwrapped(body) is not Node.Atomic));

		for (var more = true; more; )
		{
			more = false;

			foreach (var rule in rules)
				if (!gives.Contains(rule) && open.TryGetValue(rule, out var callees) && callees.Exists(gives.Contains))
					more |= gives.Add(rule);
		}

		var entries = new HashSet<RuleSymbol>(_graph.Publications.Select(static one => one.Rule).OfType<RuleSymbol>());

		return [.. rules.Where(rule => opens.Contains(rule) && gives.Contains(rule) && (entries.Contains(rule) || called.Contains(rule)))];

		static Node Unwrapped(Node node) => node switch
		{
			Node.Capture(_, var body)   => Unwrapped(body),
			Node.Construct(var body, _) => Unwrapped(body),
			_                           => node,
		};

		void OpenCalls(Node node, RuleSymbol owner, bool committed)
		{
			if (!committed && node is Node.Call(var callee, _))
			{
				called.Add(callee);

				if (!open.TryGetValue(owner, out var callees))
					open[owner] = callees = [];

				callees.Add(callee);
			}

			var inside = committed || node is Node.Atomic or Node.Lookahead;

			switch (node)
			{
				case Node.Atomic one:    OpenCalls(one.Body, owner, inside); break;
				case Node.Marked one:    OpenCalls(one.Body, owner, inside); break;
				case Node.Repeat one:    OpenCalls(one.Body, owner, inside); break;
				case Node.Lookahead one: OpenCalls(one.Body, owner, inside); break;
				case Node.Capture one:   OpenCalls(one.Body, owner, inside); break;
				case Node.Construct one: OpenCalls(one.Body, owner, inside); break;

				default:
					foreach (var child in node.Children)
						OpenCalls(child, owner, inside);
					break;
			}
		}
	}

	/// <summary>How this machine's readers carry what they read.</summary>
	/// <remarks>
	/// A property of the machine rather than of a reader because every method of every rule in
	/// a file has to agree on it: a part hands its marks to the body and the entry builds what
	/// the rules recorded. Chosen once, the first time it is asked for, which is after the
	/// machine knows its rules — and the tape where the one asked for cannot carry them, with
	/// the reason kept for whoever asks.
	/// </remarks>
	ValueCarrier Carrier
	{
		get
		{
			if (field is not null)
				return field;

			// Not kept until chosen: what reads before then reads on the tape, and is written
			// again once the choice is made (RenderReader).
			if (_carrierKind == CarrierKind.Auto)
				return _chosen is null ? _provisional ??= new TapeCarrier(this) : field = _chosen;

			if (Asked() is { } asked)
			{
				if (asked.Refuses() is { } why)
					CarrierRefusal = why;
				else
					return field = asked;
			}

			return field = new TapeCarrier(this);

			ValueCarrier? Asked() => _carrierKind switch
			{
				CarrierKind.Immediate => new ImmediateCarrier(this),
				_                     => null,
			};
		}
	}

	/// <summary>Why the carrier named could not carry this machine, or null where it could.</summary>
	/// <remarks>
	/// Asked without choosing it. <see cref="Carrier"/> settles what this machine uses and
	/// keeps the answer; this is the same question about a carrier the machine was not
	/// given, which is what an offer of one has to know before it is made.
	/// </remarks>
	public string? WouldRefuse(CarrierKind kind) => kind switch
	{
		CarrierKind.Immediate => new ImmediateCarrier(this).Refuses(),
		_                     => null,
	};

	/// <summary>Whether values are built as they are read rather than after (<see cref="CarrierKind.Immediate"/>).</summary>
	internal bool CarriesImmediately => Carrier is ImmediateCarrier;

	/// <summary>The class this machine's carrier rents, or nothing where it rents none.</summary>
	internal string CarrierStore(IReadOnlyList<string> valueTypes, string? stateType) =>
		Carrier.RenderStore(valueTypes, stateType);

	/// <summary>
	/// How a reader carries what it read until the derivation is accepted and the author's
	/// constructions can run.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The reader recognizes; something else holds the pieces of the value it is not yet
	/// allowed to build. Today that something is the tape — a log of records and a stack of
	/// gathered references, both on <c>Ways</c> — and until this seam was cut the reader wrote
	/// the tape's own calls at some fifty sites. Behind this type it writes the same fifty
	/// sites, and what they turn into is the carrier's business.
	/// </para>
	/// <para>
	/// Every method returns the C# to emit, or nothing where a carrier has nothing to do at
	/// that site: a carrier that keeps values in locals has no store to mark and nothing to
	/// put back when an alternative fails. The names are for what a site <em>means</em> — a
	/// record begun, a member put in it, the store put back to a mark — and not for how the
	/// tape does it, so that another carrier can answer the same questions differently
	/// (<c>docs/next.md</c>, the redesign; <see cref="CarrierKind"/> for the ones offered).
	/// </para>
	/// <para>
	/// Marks and unwindings come in two because the tape has two stores and the reader marks
	/// them at different sites under different conditions; where a rule gathers, the second
	/// store is the one a failed turn has to be taken back out of, whatever the carrier keeps
	/// it in. A carrier with one store, or none, answers the other with nothing.
	/// </para>
	/// </remarks>
	abstract class ValueCarrier
	{
		// ---- what a reader is handed ---------------------------------------------------------

		/// <summary>
		/// What every reader holds beyond the text, the failure and the ways: the carrier's
		/// own store, and whatever the carrier needs the reader to have — the tokens where it
		/// cuts text over kinds, the input and the context where it calls a construction that
		/// asks for them. Fields of the reader, handed to it once when it is made.
		/// </summary>
		public abstract IEnumerable<(string Type, string Name)> ReaderState { get; }

		/// <summary>The state as parameters, each with its leading comma.</summary>
		public string ReaderParameter => string.Concat(ReaderState.Select(one => $", {one.Type} {one.Name}"));

		/// <summary>And as the arguments that fill them.</summary>
		public string ReaderArgument => string.Concat(ReaderState.Select(one => $", {one.Name}"));

		/// <summary>
		/// What the reader keeps for itself and writes as it goes: fields of its own, made
		/// empty when it is made and handed to nobody. A carrier that passes values between
		/// readers through a register puts the register here rather than in its store: the
		/// reader is a <c>ref struct</c> on the stack, and a reference written into it is a
		/// plain store, where one written into an object on the heap goes through the
		/// collector's write barrier — and a parser writes one for every value it builds.
		/// </summary>
		public virtual IEnumerable<(string Type, string Name)> ReaderRegisters => [];

		/// <summary>Methods of the reader's own that what this carrier writes calls, or nothing.</summary>
		public virtual string ReaderMethods => "";

		/// <summary>
		/// What a part of a rule that gathers is handed so that it can gather into the same
		/// place, each item with its leading comma: declarations where <paramref name="declared"/>,
		/// arguments otherwise, named as the body names them where <paramref name="inBody"/>.
		/// </summary>
		public abstract string GatherHanding(RuleSymbol owner, bool declared, bool inBody);

		// ---- marks and unwinding -------------------------------------------------------------

		/// <summary>Locals remembering where the records stood, to put them back to.</summary>
		public abstract IEnumerable<string> MarkRecords(string name);

		/// <summary>
		/// What <see cref="MarkRecords"/> declares, named: a rule read in parts hands its
		/// mark down to them, and how many numbers that is depends on the carrier.
		/// </summary>
		public virtual IReadOnlyList<string> RecordMarks(string name) => [name];

		/// <summary>Locals remembering where the gathered members of the rule stood.</summary>
		public abstract IEnumerable<string> MarkGathered(RuleSymbol? owner, string name);

		/// <summary>The records put back to a mark — and with them whatever a guard built above it.</summary>
		public abstract IEnumerable<string> UnwindRecords(string name);

		/// <summary>The gathered members put back to a mark.</summary>
		public abstract IEnumerable<string> UnwindGathered(RuleSymbol? owner, string name);

		// ---- the locals a value is kept in -----------------------------------------------------

		/// <summary>
		/// The local a captured record is kept in until the rule's own record is written — on
		/// the tape an index, elsewhere the value itself.
		/// </summary>
		/// <param name="optional">Whether the member the slot belongs to may be left out, so that nothing read is a value of its own.</param>
		public abstract string DeclareRecordLocal(int slot, RuleSymbol rule, bool optional);

		/// <summary>The local a fold's value so far is kept in (§4.3).</summary>
		public abstract string DeclareAccumulator(RuleSymbol rule);

		/// <summary>
		/// What a folding rule carries from one turn to the next, and hands its parts by
		/// reference — a turn writes its record inside a part, so whatever the turns share
		/// has to reach it.
		/// </summary>
		/// <remarks>
		/// One accumulator holding the value so far, for a carrier that builds the fold as
		/// it goes or writes each turn on top of the last. A carrier keeping the turns as a
		/// run of their own carries the run instead: where it begins, where it ends, and how
		/// long it is. The names are the carrier's and appear nowhere else.
		/// </remarks>
		public virtual IEnumerable<(string Type, string Name)> FoldState(RuleSymbol owner) =>
			[(RecordLocalType(owner), "fold")];

		/// <summary>What a turn does with the value it has just written, if anything.</summary>
		/// <remarks>
		/// The value so far moves to what the turn wrote, which is what the turn after it
		/// builds on. A carrier keeping the turns as a run has already linked it and has
		/// nothing to say here.
		/// </remarks>
		public virtual string Accumulated(RuleSymbol owner) => $"fold = {Last(owner)};";

		/// <summary>What a folding rule is worth once its turns are done, if anything.</summary>
		/// <remarks>
		/// A carrier that builds the fold as it goes has the value already and says nothing
		/// here. A carrier keeping the turns as a run has the base, the run and its length
		/// in hand and nothing holding them together: this is where they become the rule.
		/// </remarks>
		public virtual string Folded(RuleSymbol owner) => "";

		/// <summary>What the body of a rule that gathers into a slot declares for it, beside the position it keeps.</summary>
		public abstract IEnumerable<string> DeclareGathered(int slot, string elementType);

		/// <summary>The type of a record local, with a trailing space, for a parameter that hands it on.</summary>
		public abstract string RecordLocalType(RuleSymbol rule, bool optional = false);

		/// <summary>A record local put back to nothing, when the part that wrote it failed.</summary>
		public abstract string ResetRecordLocal(int slot, bool optional);

		/// <summary>Whether a record local was never written.</summary>
		public abstract string Absent(RuleSymbol rule, string local);

		/// <summary>
		/// The one of a member's record slots that was written: the same name in two
		/// alternatives is one member with a slot per alternative, and the record takes
		/// whichever is set.
		/// </summary>
		public abstract string FirstRecord(IReadOnlyList<int> slots, RuleSymbol rule);

		// ---- a record -----------------------------------------------------------------------

		/// <summary>
		/// A record of one alternative of a rule begun, with the span it stands on where those
		/// are kept. What follows, up to <see cref="End"/>, is its members in the order the rule
		/// lists them.
		/// </summary>
		public abstract string Begin(RuleSymbol rule, int factory, string? start, string? end);

		/// <summary>The value so far, as a fold step's first member (§4.3).</summary>
		public abstract string PutAccumulator();

		/// <summary>A member that is a span of text.</summary>
		/// <param name="cached">
		/// A guard's local holding the same member already cut, with its <c>From</c> and
		/// <c>To</c> beside it, where one stands in this method: a carrier that cuts the text
		/// where it builds takes it when the positions are the ones it would cut.
		/// </param>
		public abstract string PutText(DirectMember member, string from, string to, string? cached = null);

		/// <summary>A member that is another record.</summary>
		public abstract string PutRecord(DirectMember member, string record);

		/// <summary>A member gathered across a repetition: everything pushed since the rule began, in its slots.</summary>
		public abstract string Collect(DirectMember member, string from, bool pairs);

		/// <summary>The record closed.</summary>
		public abstract string End(string gatheredFrom);

		/// <summary>An expression for the value of the record most recently closed, of the type.</summary>
		public abstract string Last(RuleSymbol rule);

		// ---- gathering ----------------------------------------------------------------------

		/// <summary>One piece of text pushed for a member gathered across turns.</summary>
		public abstract string PushText(int slot, string from, string to);

		/// <summary>One record pushed for a member gathered across turns.</summary>
		public abstract string PushRecord(int slot, RuleSymbol rule);

		/// <summary>A §7.8 mark, opened or closed, at the position.</summary>
		public abstract string Mark(int kind, int site);

		/// <summary>
		/// A bad element of a repetition marked <c>recover</c>, stepped over: what the method that
		/// steps over it writes once it knows where the element began (<c>pos</c>), where the
		/// synchronization began (<c>to</c>), how far the element got (<c>reach</c>) and how many
		/// elements came before it (<c>ordinal</c>) — the element, gathered where its siblings are.
		/// </summary>
		public abstract IEnumerable<string> Recovered(RecoveryPlan plan, int slot, RuleSymbol element, bool positions);

		// ---- building -----------------------------------------------------------------------

		/// <summary>A record built into a value where the reader is, for a guard that asks (§3.6); nothing where it already is one.</summary>
		public abstract string Materialize(string record, string sinceMark);

		/// <summary>The value a record holds, as a guard sees it.</summary>
		public abstract string ValueOf(RuleSymbol rule, string record);

		/// <summary>
		/// The gathered members of the given slots as one array, for a guard that names a
		/// sequence member; <paramref name="text"/> where the member is pieces of text rather
		/// than records.
		/// </summary>
		public abstract void Gathered(Writer code, string from, IReadOnlyList<int> slots, string handed, string type, string build, bool text);

		/// <summary>What an entry rents before reading, beside the ways.</summary>
		public abstract IEnumerable<string> Rent();

		/// <summary>And returns after.</summary>
		public abstract IEnumerable<string> Return();

		/// <summary>The whole derivation built into the entry's value.</summary>
		public abstract IEnumerable<string> BuildRoot(RuleSymbol rule, string type, bool extent);

		/// <summary>The code that builds records into values, once per file; nothing where values are built as they are read.</summary>
		public abstract string RenderBuilder(IReadOnlyList<RuleSymbol> rules);

		/// <summary>The class a parse rents to carry with, where the carrier rents one.</summary>
		/// <remarks>
		/// Written once for the file however many machines are in it, so two machines
		/// carrying the same way must render the same text — which they do, both being
		/// asked with the file's own union of value types. A carrier that keeps everything
		/// in the reader rents nothing and renders nothing.
		/// </remarks>
		public virtual string RenderStore(IReadOnlyList<string> valueTypes, string? stateType) => "";

		/// <summary>
		/// Whether a value handed up unchanged is already where the caller will look for it.
		/// </summary>
		/// <remarks>
		/// The tape and the immediate carrier hand a value between rules through a register
		/// of its <em>type</em>, so <c>A = a: B =&gt; @(a)</c> leaves it exactly where a
		/// caller capturing an <c>A</c> reads, and the alternative writes nothing of its own
		/// — a rule and a method saved at every level of a ladder. A carrier that hands it
		/// through a register of the <em>rule</em> has two registers there and no such luck.
		/// </remarks>
		public virtual bool ForwardsInPlace => true;

		/// <summary>
		/// Whether a capture is asked about by the rule read there rather than by the rule
		/// the member names.
		/// </summary>
		/// <remarks>
		/// One member may be captured in two places that read two different rules —
		/// `t: UnsignedLiteral =&gt; @(t)` beside `t: GeneralValueSpecification =&gt; @(t)` —
		/// and the results name one of them for the member. A carrier handing values about by
		/// value type may take that name: both rules build the type the member is declared
		/// as, so the register is the same register either way. One keeping a shape per rule
		/// may not, the two shapes being two types.
		/// </remarks>
		public virtual bool ByPlace => false;

		/// <summary>Why this carrier cannot carry the machine's rules, or null where it can.</summary>
		public abstract string? Refuses();

		/// <summary>
		/// What a call is wrapped in, where the carrier builds as it reads and the value the
		/// call reads is not built the way the reading around it is (<see cref="Demand"/>);
		/// null where it is written as it always was. <paramref name="local"/> is a name the
		/// call may take for itself.
		/// </summary>
		public virtual (string Before, string After)? AroundCall(RuleSymbol owner, Node.Call call, string local) => null;

		/// <summary>The slots of a member, as a mask the tape collects by.</summary>
		protected static long MaskOf(IReadOnlyList<int> slots)
		{
			var mask = 0L;

			foreach (var slot in slots)
				mask |= 1L << slot;

			return mask;
		}
	}

	/// <summary>
	/// The tape: records in a log and gathered references on a stack, both on <c>Ways</c>,
	/// built into values by a walk over the log once the derivation is accepted.
	/// </summary>
	/// <remarks>
	/// Every string here is what the reader wrote itself before the seam was cut, character
	/// for character, and the snapshots are what say so. The tape is the carrier that
	/// streams, finds and recovers — the others cannot yet — and it stays behind the seam
	/// for as long as that is true (<c>docs/next.md</c>).
	/// </remarks>
	sealed class TapeCarrier(Machine machine) : ValueCarrier
	{
		internal bool DenseStore;
		internal bool AdaptiveStore => machine._adaptiveStore;

		/// <remarks>
		/// The tables where a guard builds; the tokens where a guard or a glue asks about
		/// text over kinds; the context where a guard names it or builds a value whose
		/// factory might. Nothing else, because the tape cuts no text and calls no
		/// construction in a reader — the walk at the end has its own parameters.
		/// </remarks>
		public override IEnumerable<(string Type, string Name)> ReaderState
		{
			get
			{
				if (machine._directBuilds)
					yield return ("DirectValues", "values");

				if ((machine._directGuards || machine._directGlue) && machine.OverKinds)
					foreach (var token in Machine.TokenState)
						yield return token;

				if (machine.DirectReaderContext)
					yield return (machine._graph.Context!, "context");

				if (machine.UsesReading)
					yield return ("int", "parserReading");
			}
		}

		/// <remarks>
		/// Where the rule gathers across turns, what a record collects is everything pushed
		/// since the rule began — not since the part did — so the rule's mark is handed on.
		/// </remarks>
		public override string GatherHanding(RuleSymbol owner, bool declared, bool inBody) =>
			declared ? ", int refs" : inBody ? ", rb" : ", refs";

		public override IReadOnlyList<string> RecordMarks(string name) => [name, name + "R"];

		public override IEnumerable<string> MarkRecords(string name)
		{
			yield return $"var {name}  = ways.LogCount;";
			yield return $"var {name}R = ways.Records;";
		}

		public override IEnumerable<string> MarkGathered(RuleSymbol? owner, string name)
		{
			yield return $"var {name} = ways.RefsCount;";
		}

		/// <remarks>
		/// With the watermark of what a guard built, where anything builds: a record above
		/// the watermark is one written since, and a value a guard built in a derivation that
		/// was then abandoned is not the value of the record the next derivation writes at
		/// the same place.
		/// </remarks>
		public override IEnumerable<string> UnwindRecords(string name)
		{
			yield return $"ways.LogCount  = {name};";
			yield return $"ways.Records   = {name}R;";

			if (machine._directBuilds)
				yield return $"if (ways.Built > {name}R) ways.Built = {name}R;";
		}

		public override IEnumerable<string> UnwindGathered(RuleSymbol? owner, string name)
		{
			yield return $"ways.RefsCount = {name};";
		}

		public override string DeclareRecordLocal(int slot, RuleSymbol rule, bool optional) => $"var r{slot} = -1;";

		public override string DeclareAccumulator(RuleSymbol rule) => "var fold = -1;";

		public override IEnumerable<string> DeclareGathered(int slot, string elementType) => [];

		public override string RecordLocalType(RuleSymbol rule, bool optional = false) => "int ";

		public override string ResetRecordLocal(int slot, bool optional) => $"r{slot} = -1;";

		public override string Absent(RuleSymbol rule, string local) => $"{local} < 0";

		public override string FirstRecord(IReadOnlyList<int> slots, RuleSymbol rule)
		{
			if (slots.Count == 1)
				return $"r{slots[0]}";

			var chain = "-1";

			for (var i = slots.Count - 1; i >= 0; i--)
				chain = $"r{slots[i]} >= 0 ? r{slots[i]} : {chain}";

			return $"({chain})";
		}

		public override string Begin(RuleSymbol rule, int factory, string? start, string? end)
		{
			_rule = rule;

			return start is null
					? $"ways.Begin({machine.DirectArm(rule, factory)});"
					: $"ways.Begin({machine.DirectArm(rule, factory)}, {start}, {end});";
		}

		/// <summary>The rule whose record is open, for <see cref="End"/> to name it by.</summary>
		RuleSymbol? _rule;

		public override string PutAccumulator() => "ways.Put(fold);";

		public override string PutText(DirectMember member, string from, string to, string? cached = null) => $"ways.Put({from}, {to});";

		public override string PutRecord(DirectMember member, string record) => $"ways.Put({record});";

		public override string Collect(DirectMember member, string from, bool pairs) =>
			$"ways.Collect({from}, {member.Mask}L, {(pairs ? "true" : "false")});";

		public override string End(string gatheredFrom) =>
			_rule is { } rule && machine.IsExtent(rule)
				? $"ways.EndAt({gatheredFrom});"
				: $"ways.End({gatheredFrom});";

		public override string Last(RuleSymbol rule) => "ways.Last";

		public override string PushText(int slot, string from, string to) => $"ways.Push({slot}, {from}, {to});";

		public override string PushRecord(int slot, RuleSymbol rule) => $"ways.Push({slot}, ways.Last, -1);";

		public override string Mark(int kind, int site) => $"ways.Mark({kind}, {site}, p);";

		/// <remarks>A record of its own, under its own arm, which the walk builds (Machine.MaterializeRecoveryArm).</remarks>
		public override IEnumerable<string> Recovered(RecoveryPlan plan, int slot, RuleSymbol element, bool positions)
		{
			yield return positions
				? $"ways.Begin({machine.RecoveryArm(plan)}, pos, to);"
				: $"ways.Begin({machine.RecoveryArm(plan)});";
			yield return "ways.Put(pos, to);";
			yield return "ways.Put(reach, ordinal);";
			yield return "ways.End(ways.RefsCount);";
			yield return PushRecord(slot, element);
		}

		public override string Materialize(string record, string sinceMark) =>
			$"{machine.DirectMaterializer}(ways, text, values, {record}, {sinceMark}, {sinceMark}R" +
			$"{machine.TokensArgument}{machine.ContextArgument}{machine.ReadingArgument});";

		/// <summary>From the tables, or for an extent the record itself.</summary>
		public override string ValueOf(RuleSymbol rule, string record) =>
			ValueOfType(machine._results.ValueOf(rule), record);

		string ValueOfType(string type, string record) =>
			type == "SourceSpan"
				? machine.RecordValue(type, record).Replace("log[", "ways.Log[")
				: machine.DenseDirectValues
					? $"values.V{TableName(type)}[values.Starts[{record}]].Value"
					: $"values.V{TableName(type)}[{record}].Value";

		/// <remarks>
		/// Gathered turn by turn on the tape, and collected here the way the rule's end would
		/// collect them: counted first so the array is the right size, then visited.
		/// </remarks>
		public override void Gathered(Writer code, string from, IReadOnlyList<int> slots, string handed, string type, string build, bool text)
		{
			var bits    = MaskOf(slots);
			var bracket = type.IndexOf('[');

			code.Line($"var {handed}Count = 0;");
			code.Line($"for (var at = {from}; at < ways.RefsCount; at += 3)");
			code.Then($"if (({bits}L & (1L << ways.Refs[at])) != 0) {handed}Count++;");
			code.Line(
				$"var {handed} = new {(bracket < 0 ? type : type.Substring(0, bracket))}[{handed}Count]" +
				$"{(bracket < 0 ? "" : type.Substring(bracket))};");
			code.Line($"{handed}Count = 0;");

			// Built in one walk with every element a root, not in one walk an element: each of those
			// walked the whole rule from its mark, and a list of a thousand was a million records.
			if (build.Length > 0)
			{
				var whole = string.Format(build, "-1");

				code.Line(whole.Substring(0, whole.Length - 2) + $", roots: {from}, rootSlots: {bits}L);");
			}

			using (code.Block($"for (var at = {from}; at < ways.RefsCount; at += 3)"))
			{
				code.Line($"if (({bits}L & (1L << ways.Refs[at])) == 0) continue;");
				code.Line($"{handed}[{handed}Count++] = {ValueOfType(type, "ways.Refs[at + 1]")};");
			}
		}

		public override IEnumerable<string> Rent()
		{
			yield return "var values = DirectValues.Rent();";
		}

		public override IEnumerable<string> Return()
		{
			yield return "DirectValues.Return(values);";
		}

		/// <remarks>
		/// An extent's value is the span its record stands on; every other value is in the
		/// tables the walk filled.
		/// </remarks>
		public override IEnumerable<string> BuildRoot(RuleSymbol rule, string type, bool extent)
		{
			yield return
				$"{machine.DirectMaterializer}(ways, text, values, ways.Last, 0, 0" +
				$"{machine.InputArgument}{machine.TokensArgument}{machine.ContextArgument}{machine.ReadingArgument});";

			yield return
				$"value = {(extent ? machine.RecordValue(type, "ways.Last").Replace("log[", "ways.Log[") : ValueOfType(type, "ways.Last"))};";
		}

		public override string RenderBuilder(IReadOnlyList<RuleSymbol> rules) => machine.RenderDirectMaterializer(rules);

		public override string RenderStore(IReadOnlyList<string> valueTypes, string? stateType) =>
			CSharpEmitter.DirectValuesClass(valueTypes, stateType, DenseStore, AdaptiveStore, NamesMarks(machine._graph));

		public override string? Refuses() => null;
	}
}
