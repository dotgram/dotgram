using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

namespace DotGram.Grammar.Emit;

sealed partial class Machine
{
	/// <summary>
	/// Which carrier the machine was asked for — the immediate one where the host left it to the
	/// generator — and what it gets is <see cref="Carrier"/>.
	/// </summary>
	readonly CarrierKind _carrierKind;

	/// <summary>
	/// Which rules of the graph are read where the reading may not stand, asked once for the
	/// file and handed to every machine the immediate carrier was asked for; null on the tape.
	/// </summary>
	readonly Replay.Report? _replay;

	/// <summary>
	/// Whose value is ever built, asked of the graph the first time a carrier needs it
	/// (<see cref="Demand"/>) — the rules a refused input is read again through counted as read
	/// unbuilt, since that reading asks nothing of them (<see cref="UnaskedEntries"/>), and so
	/// the rules whose loops read unbuilt once a way has been given back
	/// (<see cref="GivingBack"/>).
	/// </summary>
	/// <remarks>
	/// Asked before the readers are settled it knows only the entries: which rules give back is
	/// found by settling them. <see cref="Settle"/> forgets the answer once it knows, and the
	/// carrier forgets what it kept of it (<c>ImmediateCarrier.Resettled</c>), so that the
	/// readers are written against the whole of it.
	/// </remarks>
	Demand.Report Demands => _demands ??= Demand.Of(_graph, ReadUnasked());

	Demand.Report? _demands;

	/// <summary>The rules some reading of which asks nothing of their value: the entries, and the rules that give back.</summary>
	IEnumerable<RuleSymbol> ReadUnasked()
	{
		return GivingBack is { } giving ? UnaskedEntries.Concat(giving) : UnaskedEntries;
	}

	/// <summary>
	/// Whether a reading carried immediately reads unbuilt once a loop has given a way back twice,
	/// until an attempt stands, and builds that attempt by reading it once more: the immediate
	/// carrier, over a grammar with no context and no recovery.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A repetition that gives a turn back reads every turn before it again, and a carrier that
	/// builds as it reads built them again each time: a list of a thousand members refused after
	/// its last one built half a million of them, where the tape builds none. Recognition does
	/// not depend on building — the contract the unasked reading rests on, and what lets a
	/// deferred attempt take the same ways as a built one — so once a loop has given a way back
	/// its further attempts are read with the count raised, and the one that stands is read once
	/// more with it at zero: the ways vector then holds exactly its decisions, and that reading
	/// replays them and builds. A loop that fails puts the count back and fails as before.
	/// </para>
	/// <para>
	/// A reading that records — the one made for a refusal's message, the one reading of a
	/// buffered input — recorded every refusal of the attempt that stood while it was read
	/// unbuilt, exactly as it records the attempt that stands today. The reading that builds it
	/// again records as any reading does — it must, since a reading that records opens a way at a
	/// shut door where a quiet one breaks, and only a reading of the same kind takes the recorded
	/// ways in the same places — and what it recorded is then taken back: the failure is put back
	/// to what it was before the replay, and the ties recorded since are dropped
	/// (<c>Replayed_DotGram</c>). Not with a context, where a construction may write what a guard
	/// reads (the reason <see cref="UnaskedEntries"/> leaves one out), and not with a recovery. A
	/// trace build decides the same and defers nowhere: its sink would hear the attempt that stood
	/// twice (<see cref="Deferral"/>).
	/// </para>
	/// </remarks>
	internal bool DefersOnGiveBack => CarriesImmediately && _graph.Context is null && _graph.Recoveries.Count == 0;

	/// <summary>
	/// Whether some loop this machine wrote defers and so replays (<see cref="RetryLoops"/>, outside
	/// a trace build): then the reader keeps what a replay puts back, and the methods that do it.
	/// Asked once the rules are written, where the reader's own fields and methods are.
	/// </summary>
	internal bool Replays => Tracing is null && RetryLoops.Exists(static one => one.Defers);

	/// <summary>Where a reader writes a loop that asks for another reading.</summary>
	internal enum RetrySite
	{
		/// <summary>A rule's own way back (<c>Read_X</c> around <c>Read_X_Body</c>).</summary>
		Rule,

		/// <summary>An entry's reading of the whole input, or from a position.</summary>
		Entry,

		/// <summary>One alternative of a choice over characters.</summary>
		Alternative,

		/// <summary>One turn of a repetition.</summary>
		Turn,

		/// <summary>An atomic group, sealed once it has answered.</summary>
		Atomic,

		/// <summary>A lookahead's subject, sealed once it has answered.</summary>
		Lookahead,

		/// <summary>The step of a repetition marked <c>recover</c>.</summary>
		Yield,
	}

	/// <summary>
	/// One loop that asks for another reading, and what was decided about it: whether its attempts
	/// after a give-back read unbuilt (<see cref="DefersOnGiveBack"/>), or why not, in the words a
	/// diagnostic could say.
	/// </summary>
	internal sealed record RetryLoop(RuleSymbol Owner, RetrySite Site, string? Excluded)
	{
		/// <summary>Whether the loop defers building on a give-back — in every build but a trace build, which decides the same and defers nowhere.</summary>
		public bool Defers => Excluded is null;
	}

	/// <summary>
	/// Every loop the readers wrote, with its decision (<see cref="Deferral"/>), in the order
	/// written. Filled by the rendering that writes the readers, not by the first pass over them.
	/// </summary>
	internal List<RetryLoop> RetryLoops { get; } = [];

	/// <summary>
	/// What a loop decides about deferring, noted for whoever asks later
	/// (<see cref="RetryLoops"/>). The same in a trace build, which defers nowhere all the same:
	/// the decision is about the grammar and the carrier, and a trace is a way of watching them.
	/// </summary>
	/// <param name="noting">Whether to keep the decision: the rendering that writes the readers, and not the first pass over the rules.</param>
	internal RetryLoop Deferral(RuleSymbol owner, RetrySite site, bool noting)
	{
		var loop = new RetryLoop(owner, site, Excluded(owner, site));

		if (noting)
			RetryLoops.Add(loop);

		return loop;
	}

	/// <summary>Why a loop does not defer: the grammar has a context.</summary>
	internal const string ExcludedByContext = "a context: a construction may write what a guard reads";

	/// <summary>Why a loop does not defer: the grammar has a recovery.</summary>
	internal const string ExcludedByRecovery = "a recovery";

	/// <summary>Why a loop does not defer: the rule's own guard asks for what it has built so far.</summary>
	internal const string ExcludedByOwnGuard = "its own guard asks for its value so far";

	/// <summary>
	/// The loops this machine wrote that rebuild what they read each time a way is given back: a
	/// loop that does not defer for a reason a grammar can have (a context, a recovery, its own
	/// guard asking for its value so far) and whose owner builds. On a refused input such a loop
	/// rebuilds the list it reads once a turn, which costs memory quadratic in its length where
	/// the tape builds nothing. Asked once the readers are written (<see cref="RetryLoops"/>).
	/// </summary>
	internal IEnumerable<RetryLoop> RebuildingLoops()
	{
		if (RetryLoops.Count == 0 || !CarriesImmediately)
			yield break;

		foreach (var loop in RetryLoops)
			if (loop.Excluded is ExcludedByContext or ExcludedByRecovery or ExcludedByOwnGuard && Demands.Builds(loop.Owner))
				yield return loop;
	}

	/// <summary>
	/// The rules of this machine whose constructions name <c>context</c>, and whether a hook
	/// that runs during recognition names it as well: a <c>when</c>, a <c>switch</c> selector
	/// (a guard too), or an external recognizer handed the context.
	/// </summary>
	/// <remarks>
	/// Read and write are not told apart: either way, a construction that runs as it is read
	/// rather than after the parse has accepted runs interleaved with those hooks, and what one
	/// of them sees of the other can change what is accepted or the value that is built.
	/// </remarks>
	internal (List<RuleSymbol> Constructing, bool Hooked) ContextSharing()
	{
		var constructing = new List<RuleSymbol>();
		var hooked       = false;

		if (_graph.Context is null)
			return (constructing, hooked);

		foreach (var rule in _rules)
		{
			if (!_graph.Bodies.TryGetValue(rule, out var body))
				continue;

			var named = _graph.ContextOf(rule) is not null;

			foreach (var node in NodeWalk.Descendants(body))
			{
				switch (node)
				{
					case Node.Construct { How: Construction.Expression expression }
						when named && CSharpEmitter.Uses(_graph, expression.Text, "context"):
						if (!constructing.Contains(rule))
							constructing.Add(rule);
						break;

					case Node.Guard guard when named && CSharpEmitter.Uses(_graph, guard.Text, "context"):
					case Node.External { UsesContext: true }:
						hooked = true;
						break;
				}
			}
		}

		return (constructing, hooked);
	}

	/// <summary>Why a loop does not defer building on a give-back, or null where it does.</summary>
	string? Excluded(RuleSymbol owner, RetrySite site)
	{
		if (site == RetrySite.Lookahead)
			return "a lookahead: nothing under it is built";

		if (site == RetrySite.Yield)
			return "the step of a repetition that recovers";

		if (!CarriesImmediately)
			return "the tape builds nothing while it reads";

		if (_graph.Context is not null)
			return ExcludedByContext;

		if (_graph.Recoveries.Count > 0)
			return ExcludedByRecovery;

		var demands = Demands;

		if (!demands.ReadUnasked(owner))
			return ExcludedByOwnGuard;

		if (!demands.Builds(owner))
			return "it builds nothing";

		return null;
	}

	/// <summary>
	/// The rules whose readers write a loop that asks for another reading — the rule's own way
	/// back (every rule of <see cref="_opens"/>), and the loop around an alternative, a turn or an
	/// atomic group whose reading can open a way (<see cref="_retrySites"/>) — where the carrier
	/// defers building on a give-back; null before the readers are settled, and where it does not.
	/// </summary>
	/// <remarks>
	/// The seed <see cref="Demands"/> takes beside the entries: a deferred attempt is a reading of
	/// the rule nobody asks the value of, so the rule and what inherits from it are read unbuilt,
	/// and the checks that says so are written under them. Exactly the rules whose loops get the
	/// hooks, so that nothing is checked for a reading that never defers: a loop is written where
	/// its part opens a way itself — then its owner opened one, and is in <see cref="_opens"/> —
	/// or calls a rule that does, which <see cref="Opens(Node)"/> answers once the openers are known.
	/// </remarks>
	HashSet<RuleSymbol>? GivingBack
	{
		get
		{
			if (field is not null || _opens is null || !DefersOnGiveBack)
				return field;

			field = new HashSet<RuleSymbol>(_opens);

			if (_retrySites is not null)
				foreach (var (owner, part) in _retrySites)
					if (Opens(part))
						field.Add(owner);

			return field;
		}
	}

	/// <summary>
	/// Where the readers wrote a loop around an alternative, a turn or an atomic group that may
	/// open a way, noted by the first pass over the rules (<see cref="Settle"/>): the owner, and
	/// the part the loop asks again. Whether the loop is written at all is known only once the
	/// openers are (<see cref="GivingBack"/>).
	/// </summary>
	List<(RuleSymbol Owner, Node Part)>? _retrySites;

	/// <summary>A loop around a part that may open a way, noted while the rules are read for the first time.</summary>
	void NoteRetrySite(RuleSymbol owner, Node part)
	{
		(_retrySites ??= []).Add((owner, part));
	}

	/// <summary>
	/// The rules this machine publishes a parse of whose entry, having read an input quietly and
	/// refused it, reads it a second time only for what the refusal says: nothing reads the value
	/// that reading would build, so the entry says so on the failure (<c>Failure.Unasked</c>) and
	/// a carrier that builds as it reads may leave the constructions out.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The entries that read quietly first and read again are those of a grammar with no context
	/// and no recovery (<c>CSharpEmitter.ReadsQuietlyFirst</c>). A context that the parser can put
	/// back reads quietly first too, and is left out here: a construction may write into the
	/// context and a guard read what it wrote, so a second reading without the constructions is
	/// not the first reading over again. A buffered publication is read elsewhere, once, recording.
	/// </para>
	/// <para>
	/// Asked of the graph, not of the carrier chosen: the publications are written before a
	/// machine left to choose has read its rules once and chosen, so what they say of the reading
	/// cannot depend on the carrier. The tape builds nothing while it reads whatever the failure
	/// says, and ignores what is read unbuilt; the immediate reader is the one that acts on it.
	/// </para>
	/// </remarks>
	IReadOnlyCollection<RuleSymbol> UnaskedEntries
	{
		get
		{
			if (field is not null)
				return field;

			if (_graph.Context is not null || _graph.Recoveries.Count > 0 || BufferedInput)
				return field = [];

			return field = new HashSet<RuleSymbol>(_graph.Publications
				.Where(publication => publication.Kind == PublishKind.Parse && !publication.BufferedInput && _rules.Contains(publication.Rule))
				.Select(static publication => publication.Rule));
		}
	}

	/// <summary>
	/// Whether this machine's published parses, reading a refused input a second time for its
	/// message, say so on the failure (<c>Failure.Unasked</c>). The carrier that builds as it reads
	/// then builds nothing the reading does not need: its first reading has already built and
	/// thrown away the prefix the input was accepted up to, and building it again was most of what
	/// a short refusal cost over the tape (<see cref="ImmediateCarrier"/>).
	/// </summary>
	internal bool ReplaysUnasked => UnaskedEntries.Count > 0;

	/// <summary>Why the carrier asked for was not the one used, or null.</summary>
	public string? CarrierRefusal { get; private set; }

	/// <summary>What the gates say of a machine: the rules whose constructions may run for a reading that does not stand.</summary>
	/// <param name="Building">The rules it builds.</param>
	/// <param name="Replayed">Those of them read for derivations that may not stand, the ones to look at first.</param>
	/// <param name="Again">The rules that can be read again after answering, where nothing was replayed.</param>
	internal sealed record Kept(
		IReadOnlyList<RuleSymbol> Building, IReadOnlyList<RuleSymbol> Replayed, IReadOnlyList<RuleSymbol> Again);

	/// <summary>
	/// What the gates say of a machine asked for the immediate carrier — by name, or left to the
	/// generator, which asks for it — or null where they hold nothing back: the rules whose
	/// constructions may run for a reading the parse then gives up, which the tape would have held
	/// back until the parse was accepted.
	/// </summary>
	/// <remarks>
	/// The gates choose nothing: the carrier is the immediate one wherever it does not refuse the
	/// machine. Their answer is what a grammar left to choose is told (GRAM5016, GRAM5012) and what
	/// the carriers report says, and it is kept for a machine the carrier refused too, where the
	/// report says what would hold it back were the refusal lifted. A diagnostic reads it only of a
	/// machine the immediate carrier carries.
	/// </remarks>
	internal Kept? GateReasons { get; private set; }

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
	bool LeadsWithSeam(Node node)
	{
		return node switch
		{
			Node.Capture(_, var body) => LeadsWithSeam(body),
			Node.Construct(var body, _) => LeadsWithSeam(body),
			Node.Sequence(var parts) => parts.Count > 0 && LeadsWithSeam(parts[0]),
			Node.Choice(var options) => options.Count > 0 && options.All(LeadsWithSeam),
			Node.Call(var called, _) => _seam is { } seam && ReferenceEquals(called, seam),
			_ => false,
		};
	}

	/// <summary>A node without the captures and constructions around it.</summary>
	static Node Bare(Node node)
	{
		return node switch
		{
			Node.Capture(_, var body) => Bare(body),
			Node.Construct(var body, _) => Bare(body),
			_ => node,
		};
	}

	/// <summary>Whether a node begins with a part that may read nothing, looking through a few calls.</summary>
	bool LedByNothing(Node node, int depth)
	{
		return node switch
		{
			Node.Capture(_, var body) => LedByNothing(body, depth),
			Node.Construct(var body, _) => LedByNothing(body, depth),
			Node.Marked(var body, _) => LedByNothing(body, depth),
			Node.Sequence(var parts) => parts.Count > 1 &&
										   (FirstSets.Nullable(parts[0], _graph) || LedByNothing(parts[0], depth)),
			Node.Choice(var options) => options.Count > 0 && options.All(one => LedByNothing(one, depth)),
			Node.Call(var called, _) => depth > 0 && _graph.Bodies.TryGetValue(called, out var body) &&
										   LedByNothing(body, depth - 1),
			_ => false,
		};
	}

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

	/// <summary>
	/// Asks the gates of a machine the immediate carrier was asked for, once a first reading of the
	/// rules has said which of them can be read again after answering (<see cref="GateReasons"/>).
	/// </summary>
	/// <remarks>
	/// <para>
	/// The first gate holds a rule this machine builds that is read for a derivation that may not
	/// stand (<see cref="Replay"/>); the second, where the first holds nothing, a rule a caller can
	/// ask again after it answered. The graph cannot see a way back — an alternative that answered
	/// being asked for the next one, a turn given back — which is why the report alone is not
	/// enough and this waits for the reader to have been written once.
	/// </para>
	/// <para>
	/// What is asked is not a reading that is thrown away but something already built for one:
	/// the question is asked of the rules whose value this machine constructs, so where a reading
	/// builds nothing there is nothing to hold back. A lookahead is where that tells: <c>Replay</c>
	/// calls what a look reads thrown away — it is — and a look over a rule that builds nothing
	/// holds nothing back. Not the same question as whether the body is read silently: a
	/// host-decided <c>switch</c> under a look is never silent and holds nothing back, because
	/// silence is about what the machine writes down and this is about what the author's
	/// constructions have already run.
	/// </para>
	/// <para>
	/// Nothing reads the answer to decide anything: a carrier is the one asked for, the tape where
	/// it refuses. A machine that builds nothing, and one asked for the tape, are not asked.
	/// </para>
	/// </remarks>
	/// <param name="opens">Every rule written with a way back: those that open one, and their callers.</param>
	/// <param name="ownWays">Those that open one themselves.</param>
	void AskGates(IReadOnlyList<RuleSymbol> rules, HashSet<RuleSymbol> opens, HashSet<RuleSymbol> ownWays)
	{
		if (_carrierKind != CarrierKind.Immediate || _replay is null || GateReasons is not null)
			return;

		var building = rules.Where(rule => _results.QualifiedOf(rule) is not null).ToList();

		if (building.Count == 0)
			return;

		var replayed = building
			.Where(rule => !_replay.Keeps(rule))
			.OrderBy(rule => _replay.Rules.TryGetValue(rule, out var because) && because == Replay.Because.Under ? 1 : 0)
			.ToList();
		var again    = replayed.Count > 0 ? [] : ReadAgain(rules, opens, ownWays);

		if (replayed.Count > 0 || again.Count > 0)
			GateReasons = new Kept(building, replayed, again);
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

		static Node Unwrapped(Node node)
		{
			return node switch
			{
				Node.Capture(_, var body) => Unwrapped(body),
				Node.Construct(var body, _) => Unwrapped(body),
				_ => node,
			};
		}

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
	/// the rules recorded. Settled once, the first time it is asked for, which is after the
	/// machine knows its rules — and the tape where the one asked for cannot carry them, with
	/// the reason kept for whoever asks (<see cref="CarrierRefusal"/>).
	/// </remarks>
	ValueCarrier Carrier
	{
		get
		{
			if (field is not null)
				return field;

			if (_carrierKind == CarrierKind.Immediate)
			{
				var asked = new ImmediateCarrier(this);

				if (asked.Refuses() is { } why)
					CarrierRefusal = why;
				else
					return field = asked;
			}

			return field = new TapeCarrier(this);
		}
	}

	/// <summary>Why the carrier named could not carry this machine, or null where it could.</summary>
	/// <remarks>
	/// Asked without choosing it. <see cref="Carrier"/> settles what this machine uses and
	/// keeps the answer; this is the same question about a carrier the machine was not
	/// given, which is what an offer of one has to know before it is made.
	/// </remarks>
	public string? WouldRefuse(CarrierKind kind)
	{
		return kind switch
		{
			CarrierKind.Immediate => new ImmediateCarrier(this).Refuses(),
			_ => null,
		};
	}

	/// <summary>
	/// Whether this machine's readers will carry immediately, asked before they are written: the
	/// immediate carrier where it was asked for and does not refuse the machine. The tape otherwise.
	/// </summary>
	/// <remarks>
	/// Settled by the refusal alone, which needs nothing but the rules. The emitter asks this before
	/// it folds machines together, so that a machine that would have carried immediately on its own
	/// is never put on the tape by sharing one.
	/// </remarks>
	internal bool WillCarryImmediately => _carrierKind == CarrierKind.Immediate && WouldRefuse(CarrierKind.Immediate) is null;

	/// <summary>Whether values are built as they are read rather than after (<see cref="CarrierKind.Immediate"/>).</summary>
	internal bool CarriesImmediately => Carrier is ImmediateCarrier;

	/// <summary>The class this machine's carrier rents, or nothing where it rents none.</summary>
	internal string CarrierStore(IReadOnlyList<string> valueTypes, string? stateType)
	{
		return Carrier.RenderStore(valueTypes, stateType);
	}

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
		public virtual IReadOnlyList<string> RecordMarks(string name)
		{
			return [name];
		}

		/// <summary>The type of each of <see cref="RecordMarks"/>, as a part declares it.</summary>
		public virtual string RecordMarkType => "int";

		/// <summary>Locals remembering where the gathered members of the rule stood.</summary>
		public abstract IEnumerable<string> MarkGathered(RuleSymbol? owner, string name);

		/// <summary>The records put back to a mark — and with them whatever a guard built above it.</summary>
		public abstract IEnumerable<string> UnwindRecords(string name);

		/// <summary>The gathered members put back to a mark.</summary>
		public abstract IEnumerable<string> UnwindGathered(RuleSymbol? owner, string name);

		/// <summary>
		/// What a rule that fails gives back, written at the exits of its body that stand past a
		/// push: the gathered members put back to the rule's own mark, where a carrier's collector
		/// would otherwise take what a failed rule left; nothing, where it takes only its own.
		/// </summary>
		public virtual IEnumerable<string> GiveBackGathered(RuleSymbol owner, string name)
		{
			return [];
		}

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
		public virtual IEnumerable<(string Type, string Name)> FoldState(RuleSymbol owner)
		{
			return [(RecordLocalType(owner), "fold")];
		}

		/// <summary>What a turn does with the value it has just written, if anything.</summary>
		/// <remarks>
		/// The value so far moves to what the turn wrote, which is what the turn after it
		/// builds on. A carrier keeping the turns as a run has already linked it and has
		/// nothing to say here.
		/// </remarks>
		public virtual string Accumulated(RuleSymbol owner)
		{
			return $"fold = {Last(owner)};";
		}

		/// <summary>What a folding rule is worth once its turns are done, if anything.</summary>
		/// <remarks>
		/// A carrier that builds the fold as it goes has the value already, and says only what
		/// hands it on where a step given back may have written over it. A carrier keeping the
		/// turns as a run has the base, the run and its length in hand and nothing holding them
		/// together: this is where they become the rule.
		/// </remarks>
		public virtual string Folded(RuleSymbol owner)
		{
			return "";
		}

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

		/// <summary>
		/// The same for a member gathered from operands of several types
		/// (<see cref="ResultMember.Element"/>): the record kept with the slot it was read in,
		/// which is what says whose table holds it.
		/// </summary>
		public virtual string PushTagged(int slot)
		{
			throw new InvalidOperationException("This carrier does not gather values of several types into one sequence.");
		}

		/// <summary>A §7.8 mark, opened or closed, at the position.</summary>
		public abstract string Mark(int kind, int site);

		/// <summary>
		/// A bad element of a repetition marked <c>recover</c>, stepped over: what the method that
		/// steps over it writes once it knows where the element began (<c>pos</c>), where the
		/// synchronization began (<c>to</c>), how far the element got (<c>reach</c>) and how many
		/// elements came before it (<c>ordinal</c>) — the element, gathered where its siblings are.
		/// </summary>
		/// <param name="tagged">
		/// Whether the slot's member gathers values of several types, so that the rejection is
		/// kept with its slot as a read element is (<see cref="PushTagged"/>).
		/// </param>
		public abstract IEnumerable<string> Recovered(RecoveryPlan plan, int slot, RuleSymbol element, bool positions, bool tagged);

		// ---- building -----------------------------------------------------------------------

		/// <summary>A record built into a value where the reader is, for a guard that asks (§3.6); nothing where it already is one.</summary>
		public abstract string Materialize(string record, string sinceMark);

		/// <summary>The value so far built for a guard in a fold's step that names it, from the rule's mark.</summary>
		public virtual string BuildFold(RuleSymbol owner, string sinceMark)
		{
			return Materialize("fold", sinceMark);
		}

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
		public virtual string RenderStore(IReadOnlyList<string> valueTypes, string? stateType)
		{
			return "";
		}

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
		public virtual (string Before, string After)? AroundCall(RuleSymbol owner, Node.Call call, string local)
		{
			return null;
		}

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

		/// <summary>Whether a machine sharing the store walks by the index of where records begin.</summary>
		internal bool IndexedStore;

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

				if (machine.UsesLocating)
					yield return ("int", "parserLocating");
			}
		}

		/// <remarks>
		/// Where the rule gathers across turns, what a record collects is everything pushed
		/// since the rule began — not since the part did — so the rule's mark is handed on.
		/// </remarks>
		public override string GatherHanding(RuleSymbol owner, bool declared, bool inBody)
		{
			return declared ? ", int refs" : inBody ? ", rb" : ", refs";
		}

		/// <remarks>
		/// Three numbers — how much of the log is written, how many records it holds, and the last
		/// record closed — taken and put back together as one <c>Ways.Snapshot</c>, by value.
		/// </remarks>
		public override string RecordMarkType => Machine.WaysType + ".Snapshot";

		public override IEnumerable<string> MarkRecords(string name)
		{
			yield return $"var {name} = ways.Snap();";
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
			// Only where a mark can exist: a grammar that declares no `state` places none, and the
			// journal it would unwind is never written. Asked of the declaration rather than of the
			// sites, which are numbered as the reader is emitted and so are not all known yet here.
			if (machine._graph.State is not null)
				yield return $"ways.MarksBackTo({name}.LogCount);";

			yield return machine._directBuilds ? $"ways.RewindBuilt({name});" : $"ways.Rewind({name});";
		}

		public override IEnumerable<string> UnwindGathered(RuleSymbol? owner, string name)
		{
			yield return $"ways.RefsCount = {name};";
		}

		public override string DeclareRecordLocal(int slot, RuleSymbol rule, bool optional)
		{
			return $"var r{slot} = -1;";
		}

		public override string DeclareAccumulator(RuleSymbol rule)
		{
			return machine.FoldAsked(rule)
				? "var fold = -1; var foldSince = ways.Snap(); var foldEnd = foldSince;"
				: "var fold = -1;";
		}

		/// <remarks>
		/// Where a guard in a step names the value so far, the log's mark after the last fold
		/// record a guard built (<c>foldSince</c>) and after the newest one (<c>foldEnd</c>) go
		/// with it: the next guard builds from the first, not from the rule's mark, since
		/// everything before it is built and walking it again at every step is quadratic in
		/// the steps.
		/// </remarks>
		public override IEnumerable<(string Type, string Name)> FoldState(RuleSymbol owner)
		{
			return machine.FoldAsked(owner)
				? [("int ", "fold"), (RecordMarkType + " ", "foldSince"), (RecordMarkType + " ", "foldEnd")]
				: base.FoldState(owner);
		}

		public override string Accumulated(RuleSymbol owner)
		{
			return machine.FoldAsked(owner)
				? "fold = ways.Last; foldEnd = ways.Snap();"
				: base.Accumulated(owner);
		}

		public override string BuildFold(RuleSymbol owner, string sinceMark)
		{
			return machine.FoldAsked(owner)
				? Materialize("fold", "foldSince") + " foldSince = foldEnd;"
				: base.BuildFold(owner, sinceMark);
		}

		public override IEnumerable<string> DeclareGathered(int slot, string elementType)
		{
			return [];
		}

		public override string RecordLocalType(RuleSymbol rule, bool optional = false)
		{
			return "int ";
		}

		public override string ResetRecordLocal(int slot, bool optional)
		{
			return $"r{slot} = -1;";
		}

		public override string Absent(RuleSymbol rule, string local)
		{
			return $"{local} < 0";
		}

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

			if (start is null)
				return $"ways.Begin({machine.DirectArm(rule, factory)});";

			// Where only a located reading needs where the record stands, the other one writes
			// the record a plain parser writes. The reader is not generic, so it asks its reading's
			// own number, once a record; the walk that reads the record back is, and asks nothing.
			if (machine.PositionsPerCall(machine._directRules))
				return $"if (parserLocating >= 0) ways.Begin({machine.DirectArm(rule, factory)}, {start}, {end}); " +
					$"else ways.Begin({machine.DirectArm(rule, factory)});";

			return $"ways.Begin({machine.DirectArm(rule, factory)}, {start}, {end});";
		}

		/// <summary>The rule whose record is open, for <see cref="End"/> to name it by.</summary>
		RuleSymbol? _rule;

		public override string PutAccumulator()
		{
			return "ways.Put(fold);";
		}

		public override string PutText(DirectMember member, string from, string to, string? cached = null)
		{
			return $"ways.Put({from}, {to});";
		}

		public override string PutRecord(DirectMember member, string record)
		{
			return $"ways.Put({record});";
		}

		public override string Collect(DirectMember member, string from, bool pairs)
		{
			return $"ways.Collect({from}, {member.Mask}L, {(pairs ? "true" : "false")});";
		}

		public override string End(string gatheredFrom)
		{
			return _rule is { } rule && machine.IsExtent(rule)
				? $"ways.EndAt({gatheredFrom});"
				: $"ways.End({gatheredFrom});";
		}

		public override string Last(RuleSymbol rule)
		{
			return "ways.Last";
		}

		public override string PushText(int slot, string from, string to)
		{
			return $"ways.Push({slot}, {from}, {to});";
		}

		public override string PushRecord(int slot, RuleSymbol rule)
		{
			return $"ways.Push({slot}, ways.Last, -1);";
		}

		public override string PushTagged(int slot)
		{
			return $"ways.Push({slot}, ways.Last, {slot});";
		}

		public override string Mark(int kind, int site)
		{
			return $"ways.Mark({kind}, {site}, p);";
		}

		/// <remarks>A record of its own, under its own arm, which the walk builds (Machine.MaterializeRecoveryArm).</remarks>
		public override IEnumerable<string> Recovered(RecoveryPlan plan, int slot, RuleSymbol element, bool positions, bool tagged)
		{
			yield return positions
				? $"ways.Begin({machine.RecoveryArm(plan)}, pos, to);"
				: $"ways.Begin({machine.RecoveryArm(plan)});";
			yield return "ways.Put(pos, to);";
			yield return "ways.Put(reach, ordinal);";
			yield return "ways.End(ways.RefsCount);";

			// A yield's step keeps its element as the item it hands out, gathering nothing.
			if (!plan.Recovery.YieldStep)
				yield return tagged ? PushTagged(slot) : PushRecord(slot, element);
		}

		public override string Materialize(string record, string sinceMark)
		{
			return machine.Materializing(
				$"{machine.DirectMaterializer}(ways, text, values, {record}, {sinceMark}" +
				$"{machine.TokensArgument}{machine.ContextArgument}{machine.ReadingArgument});");
		}

		/// <summary>From the tables, or for an extent the record itself.</summary>
		public override string ValueOf(RuleSymbol rule, string record)
		{
			return ValueOfType(machine._results.ValueOf(rule), record);
		}

		string ValueOfType(string type, string record)
		{
			return type == "SourceSpan"
				? machine.RecordValue(type, record).Replace("log[", "ways.Log[")
				: machine.DenseDirectValues
					? $"values.V{TableName(type)}[values.Starts[{record}]].Value"
					: $"values.V{TableName(type)}[{record}].Value";
		}

		/// <remarks>
		/// Gathered turn by turn on the tape, and collected here the way the rule's end would
		/// collect them: counted first so the array is the right size, then visited.
		/// </remarks>
		public override void Gathered(Writer code, string from, IReadOnlyList<int> slots, string handed, string type, string build, bool text)
		{
			var bits    = MaskOf(slots);

			code.Line($"var {handed}Count = 0;");
			code.Line($"for (var at = {from}; at < ways.RefsCount; at += 3)");
			code.Then($"if (({bits}L & (1L << ways.Refs[at])) != 0) {handed}Count++;");
			code.Line($"var {handed} = {CSharpEmitter.FilledArray(type, $"{handed}Count")};");
			code.Line($"{handed}Count = 0;");

			// Built in one walk with every element a root, not in one walk an element: each of those
			// walked the whole rule from its mark, and a list of a thousand was a million records.
			// And no walk for a list of none, as none was walked an element at a time: a guard handed
			// a list that is mostly empty — the expression language's dotted name, at every name —
			// paid a walk each time for nothing to build.
			if (build.Length > 0)
			{
				var whole = string.Format(build, "-1");

				// Every call in it, where the reading picks one of two (Machine.Materializing).
				code.Line($"if ({handed}.Length > 0) " + whole.Replace(");", $", roots: {from}, rootSlots: {bits}L);"));
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
			yield return machine.Materializing(
				$"{machine.DirectMaterializer}(ways, text, values, ways.Last, default" +
				$"{machine.InputArgument}{machine.TokensArgument}{machine.ContextArgument}{machine.ReadingArgument});");

			yield return
				$"value = {(extent ? machine.RecordValue(type, "ways.Last").Replace("log[", "ways.Log[") : ValueOfType(type, "ways.Last"))};";
		}

		public override string RenderBuilder(IReadOnlyList<RuleSymbol> rules)
		{
			return machine.RenderDirectMaterializer(rules);
		}

		public override string RenderStore(IReadOnlyList<string> valueTypes, string? stateType)
		{
			return CSharpEmitter.DirectValuesClass(valueTypes, stateType, DenseStore, AdaptiveStore, NamesMarks(machine._graph), IndexedStore);
		}

		public override string? Refuses()
		{
			return null;
		}
	}
}
