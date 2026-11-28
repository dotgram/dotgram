using System;
using System.Globalization;
using System.Text;

using DotGram.Grammar.Model;

namespace DotGram.Ir.Prototype;

/// <summary>
/// What a slot holds, which is also what may be done to it.
/// </summary>
enum IrSlotKind
{
	/// <summary>A position in the input. Assigned once in its lifetime.</summary>
	Position,

	/// <summary>A text read from the input or what a construction built. Assigned once in its lifetime.</summary>
	Value,

	/// <summary>The values a repetition gathered, in order: added to, and cut back where a turn is undone.</summary>
	List,

	/// <summary>
	/// A position a repetition's turns each write again: the ends of a text bound inside a
	/// repetition. The one kind that is not assigned once.
	/// </summary>
	Rebound,
}

/// <summary>
/// A named place a function keeps something in. Its lifetime is the <see cref="IrStmt.Scope"/>
/// that declares it: it holds nothing each time that scope is entered.
/// </summary>
sealed class IrSlot(int index, string name, IrSlotKind kind, string? type)
{
	public int        Index { get; } = index;
	public string     Name  { get; } = name;
	public IrSlotKind Kind  { get; } = kind;

	/// <summary>The host's spelling of what it holds, where the grammar gave one; otherwise null.</summary>
	public string? Type { get; } = type;

	public override string ToString()
	{
		return "s" + Index.ToString(CultureInfo.InvariantCulture) + "_" + Name;
	}
}

/// <summary>
/// A piece of host code the grammar handed over, kept as written and called by number.
/// </summary>
/// <param name="Id">Its number: the rank of where it was written in the grammar.</param>
/// <param name="IsGuard">A <c>when</c>, which answers yes or no; otherwise a construction, which builds a value.</param>
/// <param name="Text">The host's text, opaque here.</param>
/// <param name="At">Where it was written in the grammar.</param>
/// <param name="Parameters">The captures it names, in the order the rule declares them.</param>
/// <param name="Rule">The rule it was written in.</param>
sealed record IrFragment(int Id, bool IsGuard, string Text, int At, IReadOnlyList<string> Parameters, string Rule)
{
	/// <summary>
	/// Whether the text names what the parser supplies — its context or its own state — which no
	/// record of a reading holds: what reads it is read again on every replay, never gone past.
	/// </summary>
	public bool NamesParser { get; init; }

	public string Name => (IsGuard ? "g" : "c") + Id.ToString(CultureInfo.InvariantCulture);
}

/// <summary>What an assignment or an argument reads.</summary>
abstract record IrExpr
{
	/// <summary>The position the reading stands at.</summary>
	public sealed record Position : IrExpr
	{
		public static readonly Position Instance = new();
	}

	/// <summary>What a slot holds; nothing where it was never assigned.</summary>
	public sealed record Slot(IrSlot Of) : IrExpr;

	/// <summary>The input from the position a slot holds to where the reading stands.</summary>
	public sealed record Text(IrSlot From) : IrExpr;

	/// <summary>The values a list slot gathered, as a sequence; an empty one where nothing was ever added.</summary>
	public sealed record Items(IrSlot Of) : IrExpr;

	public sealed record Number(int Value) : IrExpr;

	/// <summary>How many input items stand between the positions two slots hold; none where they hold nothing.</summary>
	public sealed record Length(IrSlot From, IrSlot To) : IrExpr;
}

/// <summary>
/// One step of a function. A step either goes on or fails; a failure is taken by the nearest
/// step around it that says what a failure inside it means, and by the function otherwise.
/// </summary>
abstract record IrStmt
{
	public virtual IEnumerable<IrStmt> Children => [];

	/// <summary>Steps in order; the first that fails fails the block.</summary>
	public sealed record Block(IReadOnlyList<IrStmt> Items) : IrStmt
	{
		public override IEnumerable<IrStmt> Children => Items;
	}

	/// <summary>A lifetime: the slots hold nothing each time the body is entered.</summary>
	public sealed record Scope(IReadOnlyList<IrSlot> Slots, IrStmt Body) : IrStmt
	{
		public override IEnumerable<IrStmt> Children => [Body];
	}

	/// <summary>One input item in (or, negated, outside) a set of ranges.</summary>
	public sealed record MatchSet(IReadOnlyList<CharRange> Ranges, bool IsNegated, int Expected) : IrStmt;

	/// <summary>A fixed run of input items. A refusal is recorded where the input first disagrees.</summary>
	public sealed record MatchText(string Text, bool IgnoreCase, int Expected) : IrStmt;

	/// <summary>The input between two positions, read again here.</summary>
	public sealed record MatchSame(IrSlot From, IrSlot To, int Expected) : IrStmt;

	/// <summary>The end of the input.</summary>
	public sealed record MatchEnd(int Expected) : IrStmt;

	/// <summary>
	/// Another function, read from here. Its value goes into <paramref name="Into"/>, or is added
	/// to it where <paramref name="Appends"/>.
	/// </summary>
	public sealed record Call(IrFunction Target, IrSlot? Into, bool Appends) : IrStmt
	{
		/// <summary>
		/// The set recorded where the call was made, where the call fails: what the choice it begins the
		/// widest part of wanted there (<see cref="IrDispatch.Called"/>); -1 for none.
		/// </summary>
		public int RefusesWith { get; init; } = -1;

		/// <summary>
		/// The set the call says where it refuses at the place it was made, which <see cref="RefusesWith"/> is
		/// said over: dropped for it there; -1 for none.
		/// </summary>
		public int Over { get; init; } = -1;

		/// <summary>
		/// The rule's own words, recorded where the call was made where it fails and the furthest refusal
		/// has not passed that place: the rule refused there, having read nothing; -1 for none.
		/// </summary>
		public int Says { get; init; } = -1;
	}

	/// <summary>
	/// The first alternative that reads, each from where the choice began. An alternative that
	/// fails leaves nothing behind: the position and the slots of <paramref name="Resets"/> for it
	/// are put back.
	/// </summary>
	/// <param name="Resumable">
	/// Whether the alternatives after the one that read stay open: the choice writes its decision
	/// on the tape, and a failure after it may ask it for the next one.
	/// </param>
	public sealed record Choice(
		IReadOnlyList<IrStmt> Alternatives, IReadOnlyList<IReadOnlyList<IrSlot>> Resets, bool Resumable) : IrStmt
	{
		/// <summary>
		/// Where the alternatives begin apart: the one the next input item begins is the only one
		/// read, and where it begins none the choice says one set of its own and nothing is tried.
		/// </summary>
		public IrDispatch? Dispatch { get; init; }

		/// <summary>
		/// What the next input item must begin for the alternatives to be tried at all: where it begins
		/// none, the set is recorded and the choice fails (or, an optional's door, the set is noted and the
		/// choice reads nothing).
		/// </summary>
		public IrGate? Gate { get; init; }

		/// <summary>
		/// Whether an alternative that fails is read again in place with each of its own decisions that
		/// can still move before the choice moves on — the last alternative too, before the choice
		/// fails: what the generated reader does around every alternative that writes a way. Only of a
		/// choice that is <paramref name="Resumable"/>; the reader reads a closed one once through.
		/// </summary>
		public bool Retries { get; init; }

		/// <summary>Whether that reading again stops building after the second decision it gives back (<see cref="Replay.Defers"/>).</summary>
		public bool Defers { get; init; }

		public override IEnumerable<IrStmt> Children => Alternatives;
	}

	/// <summary>
	/// As many turns of the body as read, between two bounds. A turn that fails is undone as an
	/// alternative is, and a turn that read nothing is the last.
	/// </summary>
	/// <param name="Resumable">
	/// Whether each turn past the least may be given back: every such turn writes a decision on
	/// the tape, taken or not.
	/// </param>
	public sealed record Repeat(
		IrStmt Body, IrExpr Min, IrExpr? Max, IReadOnlyList<IrSlot> Resets, bool Resumable) : IrStmt
	{
		/// <summary>
		/// What a turn past the least must begin with to be tried at all: behind a shut door no
		/// turn is read and nothing of it is said.
		/// </summary>
		public IReadOnlyList<CharRange>? Door { get; init; }

		/// <summary>Whether the door is shut only once the least turns are read (otherwise at every turn).</summary>
		public bool DoorPastLeast { get; init; } = true;

		/// <summary>Whether the door is shut on the reading that records nothing alone (<c>ParseQuietlyFirst</c>'s first).</summary>
		public bool DoorQuietOnly { get; init; }

		/// <summary>What is recorded where the door shuts below the least turns; -1 for nothing.</summary>
		public int DoorShort { get; init; } = -1;

		/// <summary>
		/// Whether a turn that fails is read again in place with each of its own decisions that can still
		/// move before it is given up — every turn, below the fewest too, and whether or not the
		/// repetition is <paramref name="Resumable"/>: what the generated reader does around every turn
		/// that writes a way, whatever it decided about the repetition's own.
		/// </summary>
		public bool Retries { get; init; }

		/// <summary>Whether that reading again stops building after the second decision it gives back (<see cref="Replay.Defers"/>).</summary>
		public bool Defers { get; init; }

		public override IEnumerable<IrStmt> Children => [Body];
	}

	/// <summary>
	/// Whether the body reads here, or (not positive) whether it does not. Nothing of it is kept:
	/// the position is put back, nothing is built inside, no refusal inside is recorded and no
	/// decision made inside is left on the tape.
	/// </summary>
	public sealed record Look(bool IsPositive, IrStmt Body, IReadOnlyList<IrSlot> Resets, int Expected) : IrStmt
	{
		public override IEnumerable<IrStmt> Children => [Body];
	}

	/// <summary>
	/// The body's first reading, and no other: the decisions made inside are taken off the tape
	/// once it has read. Before that it is its own replay region.
	/// </summary>
	public sealed record Commit(IrStmt Body, IReadOnlyList<IrSlot> Resets) : IrStmt
	{
		/// <summary>Whether its own reading again stops building after the second decision it gives back (<see cref="Replay.Defers"/>).</summary>
		public bool Defers { get; init; }

		public override IEnumerable<IrStmt> Children => [Body];
	}

	/// <summary>
	/// A replay region: where the body fails and a decision written on the tape since it began
	/// can still be moved on, the last such decision is, and the body is run again from where it
	/// began — with the same decisions before that one, read back from the tape.
	/// </summary>
	public sealed record Replay(IrStmt Body) : IrStmt
	{
		/// <summary>
		/// Whether, once it has given a decision back twice, the region reads on without building —
		/// and where a reading then stands, reads it once more to build it, with what that reading
		/// says of a refusal taken back: so a refused list builds its first two attempts and no more.
		/// What the plan decided for the region's rule and the kind of its loop, as the generated
		/// parser decides it.
		/// </summary>
		public bool Defers { get; init; }

		public override IEnumerable<IrStmt> Children => [Body];
	}

	public sealed record Assign(IrSlot Slot, IrExpr Value) : IrStmt;

	public sealed record Append(IrSlot List, IrExpr Value) : IrStmt;

	/// <summary>A question to the host. No: a refusal is recorded and the step fails.</summary>
	public sealed record Guard(IrFragment Fragment, IReadOnlyList<IrExpr> Arguments, int Expected) : IrStmt;

	/// <summary>
	/// The host builds a value. Runs where the reading stands when it is reached, each time it is
	/// reached, and not at all inside a <see cref="Look"/>.
	/// </summary>
	public sealed record Construct(IrFragment Fragment, IReadOnlyList<IrExpr> Arguments, IrSlot Into) : IrStmt;

	/// <summary>Something of the graph this sketch does not lower. Counted, never run.</summary>
	public sealed record Unsupported(string What) : IrStmt;
}

/// <summary>What each alternative of a choice begins with, and the set said where the input begins none of them.</summary>
sealed record IrDispatch(IReadOnlyList<IReadOnlyList<CharRange>> Begins, int Otherwise)
{
	/// <summary>
	/// The alternative read where the next item begins none of the others, by the call it begins with,
	/// which tests the item itself; -1 where every alternative is tested.
	/// </summary>
	public int Called { get; init; } = -1;
}

/// <summary>A choice's door: what the next item must begin, the set said where it does not, and whether that ends the choice as nothing read.</summary>
sealed record IrGate(IReadOnlyList<CharRange> Begins, int Expected, bool Empty);

/// <summary>
/// One rule (or one specialization of one) as a function from a position to the position its
/// reading ends at, or to a failure.
/// </summary>
sealed class IrFunction(int index, string name)
{
	public int    Index { get; } = index;
	public string Name  { get; } = name;

	public IrStmt Body { get; set; } = new IrStmt.Block([]);

	/// <summary>The slot whose value is the function's when it has read, or null where its value is the text it read.</summary>
	public IrSlot? Result { get; set; }

	public int SlotCount { get; set; }

	/// <summary>Whether a reading of it can leave a decision on the tape.</summary>
	public bool Decides { get; set; }

	/// <summary>Whether everything in its body was lowered.</summary>
	public bool Complete { get; set; } = true;
}

/// <summary>A grammar as functions, the host fragments they call and the names of what they expect.</summary>
sealed class IrProgram
{
	public List<IrFunction> Functions { get; } = [];
	public List<IrFragment> Fragments { get; } = [];
	public List<string>     Expected  { get; } = [];

	/// <summary>The expected sets that are a guard's: said once at a place however often the guard is asked there.</summary>
	public HashSet<int> Guarding { get; } = [];

	/// <summary>A function per publication, by the publication's method name: the whole input, read as the rule.</summary>
	public Dictionary<string, IrFunction> Entries { get; } = new(StringComparer.Ordinal);

	/// <summary>How many statements and expressions the functions hold.</summary>
	public int NodeCount { get; set; }

	/// <summary>How many graph nodes were met, and how many of them had no lowering.</summary>
	public int GraphNodes { get; set; }

	public int UnsupportedNodes { get; set; }

	/// <summary>How many choices and repetitions the lowering asked the plan about, and how many of them the plan had no answer for.</summary>
	public int PlanAsked { get; set; }

	public int PlanMisses { get; set; }

	/// <summary>How many places that record a refusal the lowering asked the plan about, the places the plan holds that it did not read among them.</summary>
	public int SitesAsked { get; set; }

	/// <summary>
	/// How many of those the plan had no answer for, or held an answer the lowering does not read: a place of
	/// a kind it does not lower is said otherwise than the generated reader says it (<see cref="UnreadKinds"/>).
	/// </summary>
	public int SiteMisses { get; set; }

	/// <summary>The places the plan holds and the lowering did not read, by their kind.</summary>
	public SortedDictionary<string, int> UnreadKinds { get; } = new(StringComparer.Ordinal);

	/// <summary>
	/// How many repetitions the plan left without a way that the lowering opened all the same: a turn
	/// past the fewest that can fail after a decision of its own is read again with that decision moved
	/// before it is given up, and the tree has no such reading but the repetition's own way.
	/// </summary>
	public int Reopened { get; set; }

	/// <summary>How many turns and alternatives are read again in place over their own decisions (<see cref="IrStmt.Repeat.Retries"/>, <see cref="IrStmt.Choice.Retries"/>).</summary>
	public int Retrying { get; set; }

	/// <summary>How many loops that ask for another reading the plan had no single answer for, whether they stop building: those build throughout.</summary>
	public int DeferralMisses { get; set; }

	public Dictionary<string, int> UnsupportedKinds { get; } = new(StringComparer.Ordinal);

	/// <summary>The functions one to a paragraph, a statement to a line.</summary>
	public string Dump(bool fragmentTexts = true)
	{
		var text = new StringBuilder();

		foreach (var function in Functions)
		{
			text.Append(function.Name).Append(function.Decides ? " (decides)" : "").Append(":\n");
			Write(function.Body, 1);
		}

		return text.ToString();

		void Write(IrStmt stmt, int depth)
		{
			text.Append(' ', depth * 2).Append(stmt.GetType().Name);

			switch (stmt)
			{
				case IrStmt.Scope scope:      text.Append(' ').Append(string.Join(" ", scope.Slots)); break;
				case IrStmt.MatchSet set:     text.Append(' ').Append(Expected[set.Expected]); break;
				case IrStmt.MatchText lit:    text.Append(' ').Append(Expected[lit.Expected]); break;
				case IrStmt.MatchSame same:   text.Append(' ').Append(same.From).Append("..").Append(same.To); break;
				case IrStmt.Call call:        text.Append(' ').Append(call.Target.Name).Append(call.Into is null ? "" : (call.Appends ? " +> " : " -> ") + call.Into); break;
				case IrStmt.Choice choice:    text.Append(choice.Resumable ? " resumable" : ""); break;
				case IrStmt.Repeat repeat:    text.Append(' ').Append(Show(repeat.Min)).Append("..").Append(repeat.Max is null ? "*" : Show(repeat.Max)).Append(repeat.Resumable ? " resumable" : ""); break;
				case IrStmt.Look look:        text.Append(look.IsPositive ? " positive" : " negative"); break;
				case IrStmt.Assign assign:    text.Append(' ').Append(assign.Slot).Append(" = ").Append(Show(assign.Value)); break;
				case IrStmt.Append append:    text.Append(' ').Append(append.List).Append(" += ").Append(Show(append.Value)); break;
				case IrStmt.Guard guard:      text.Append(' ').Append(guard.Fragment.Name).Append('(').Append(string.Join(", ", guard.Arguments.Select(Show))).Append(')').Append(fragmentTexts ? " " + guard.Fragment.Text : ""); break;
				case IrStmt.Construct build:  text.Append(' ').Append(build.Into).Append(" = ").Append(build.Fragment.Name).Append('(').Append(string.Join(", ", build.Arguments.Select(Show))).Append(')').Append(fragmentTexts ? " " + build.Fragment.Text : ""); break;
				case IrStmt.Unsupported none: text.Append(' ').Append(none.What); break;
			}

			text.Append('\n');

			foreach (var child in stmt.Children)
				Write(child, depth + 1);
		}
	}

	public static string Show(IrExpr expr)
	{
		return expr switch
		{
			IrExpr.Position         => "p",
			IrExpr.Slot slot        => slot.Of.ToString(),
			IrExpr.Text text        => "text(" + text.From + "..p)",
			IrExpr.Items items      => "items(" + items.Of + ")",
			IrExpr.Number number    => number.Value.ToString(CultureInfo.InvariantCulture),
			IrExpr.Length length    => "length(" + length.From + ".." + length.To + ")",
			_                       => throw new ArgumentOutOfRangeException(nameof(expr)),
		};
	}
}
