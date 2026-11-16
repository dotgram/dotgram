using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;

namespace DotGram.Language;

internal enum DslRecognitionStatus
{
	Success,
	Failure,
	Unsupported,
}

internal readonly record struct DslRecognitionExtent(
	RuleSymbol Rule,
	string? Capture,
	int Position,
	int Length);

internal sealed class DslRecognitionResult(
	DslRecognitionStatus status,
	int failurePosition,
	IReadOnlyList<DslRecognitionExtent> extents,
	IReadOnlyList<string> expected)
{
	public DslRecognitionStatus Status { get; } = status;
	public int FailurePosition { get; } = failurePosition;
	public IReadOnlyList<DslRecognitionExtent> Extents { get; } = extents;
	public IReadOnlyList<string> Expected { get; } = expected;
}

/// <summary>
/// Supplies recognition facts which cannot be derived from a normalized grammar alone.
/// Implementations describe tooling-safe behavior; they must not invoke generated parser
/// methods or consumer code.
/// </summary>
internal interface IDslRecognitionContract
{
	bool TryEvaluateGuard(Node.Guard guard, RuleSymbol owner, int position, out bool accepted);

	bool TryResolveExternal(
		Node.External external,
		RuleSymbol owner,
		out RuleSymbol rule);
}

internal sealed class DslDescriptorRecognitionContract : IDslRecognitionContract
{
	readonly IReadOnlyDictionary<string, bool> _guards;
	readonly IReadOnlyDictionary<string, RuleSymbol> _externals;

	public DslDescriptorRecognitionContract(
		RecognitionGraph graph,
		DslRecognitionContractDefinition definition)
	{
		_guards = definition.Guards;
		var rules = graph.Rules
			.GroupBy(static rule => rule.Name, StringComparer.Ordinal)
			.Where(static group => group.Count() == 1)
			.ToDictionary(static group => group.Key, static group => group.Single(), StringComparer.Ordinal);
		_externals = definition.Externals
			.Where(item => rules.ContainsKey(item.Value))
			.ToDictionary(static item => item.Key, item => rules[item.Value], StringComparer.Ordinal);
	}

	public bool TryEvaluateGuard(Node.Guard guard, RuleSymbol owner, int position, out bool accepted)
	{
		return _guards.TryGetValue(guard.Text, out accepted);
	}

	public bool TryResolveExternal(Node.External external, RuleSymbol owner, out RuleSymbol rule)
	{
		return _externals.TryGetValue(external.Name, out rule!);
	}
}

/// <summary>
/// Interprets the normalized recognition model for editor tooling without executing
/// generated or user code. The successful derivation carries the source extents of every
/// rule and capture that participated in it.
/// </summary>
/// <remarks>
/// <para>
/// A backtracking machine with its stacks on the heap: the continuation is a list of what is
/// left to do, shared between the routes that branch from it, and each route not yet taken
/// is a choice point that remembers where the text, the continuation and the extents stood.
/// Nothing recurses, so a site nested as deep as the text makes it is read on any stack.
/// </para>
/// <para>
/// The first derivation that reads the whole text is the one a generated parser takes, and
/// reading stops there. A rule's derivations that end where an earlier one of the same call
/// ended are not followed: what reads after the rule is the same either way, and it failed
/// already. A call whose every derivation has been tried is remembered by rule and position,
/// and another call there replays the ends it found instead of reading the rule again. The
/// extents are a list that every route shares the front of, built in one step per extent and
/// laid out in order once, at the end.
/// </para>
/// </remarks>
internal static class DslRecognitionTrace
{
	public static DslRecognitionResult Recognize(
		RecognitionGraph graph,
		Publication publication,
		string input,
		IDslRecognitionContract? contract = null)
	{
		if (graph is null)       throw new ArgumentNullException(nameof(graph));
		if (publication is null) throw new ArgumentNullException(nameof(publication));
		if (input is null)       throw new ArgumentNullException(nameof(input));

		if (publication.Kind != DotGram.Grammar.Parsing.PublishKind.Parse)
			return new DslRecognitionResult(DslRecognitionStatus.Unsupported, 0, [], []);

		var matcher = new Matcher(graph, publication, input, contract);

		if (matcher.Run() is { } successful)
			return new DslRecognitionResult(
				DslRecognitionStatus.Success,
				input.Length,
				Flatten(successful),
				[]);

		return new DslRecognitionResult(
			matcher.Unsupported ? DslRecognitionStatus.Unsupported : DslRecognitionStatus.Failure,
			matcher.Furthest,
			Flatten(matcher.Partial),
			matcher.Expected);
	}

	/// <summary>The extents of a derivation, each rule or capture before those inside it, in the order the text has them.</summary>
	static IReadOnlyList<DslRecognitionExtent> Flatten(Trail? head)
	{
		var extents = new List<DslRecognitionExtent>(head?.Total ?? 0);
		var work    = new Stack<(Trail? From, Trail? Stop, Trail? Extent)>();

		work.Push((head, null, null));

		while (work.Count > 0)
		{
			var (from, stop, extent) = work.Pop();

			if (extent is not null)
			{
				extents.Add(extent.Extent);
				continue;
			}

			// The list runs from the newest extent back: pushed that way, the oldest comes off first,
			// and each one's own extents right after it.
			for (var cell = from; cell is not null && cell != stop; cell = cell.Next)
			{
				work.Push((cell.Inner, cell.Stop, null));
				work.Push((null, null, cell));
			}
		}

		return extents;
	}

	/// <summary>
	/// One extent in front of those read before it. The extents inside it are the list from
	/// <see cref="Inner"/> back to <see cref="Stop"/>, which is where the list stood when it began.
	/// </summary>
	/// <remarks>
	/// Each cell also counts what it holds, so that the best partial reading is chosen without
	/// walking any list: the extents of rules other than trivia, how far the furthest of them
	/// reaches, and all of them.
	/// </remarks>
	sealed class Trail
	{
		public Trail(DslRecognitionExtent extent, bool meaningful, Trail? inner, Trail? stop, Trail? next)
		{
			Extent = extent;
			Inner  = inner;
			Stop   = stop;

			var innerCount = (inner?.Count ?? 0) - (stop?.Count ?? 0);

			// Every extent inside this one begins where it does, at or after the end of each read before it.
			OwnCount = innerCount + (meaningful ? 1 : 0);
			OwnTotal = (inner?.Total ?? 0) - (stop?.Total ?? 0) + 1;
			OwnEnd   = Math.Max(meaningful ? extent.Position + extent.Length : -1, innerCount > 0 ? inner!.End : -1);

			Next  = next;
			Count = (next?.Count ?? 0) + OwnCount;
			Total = (next?.Total ?? 0) + OwnTotal;
			End   = Math.Max(next?.End ?? -1, OwnEnd);
		}

		/// <summary>The same extent, with what it holds, in front of another list: a remembered call replayed.</summary>
		public Trail(Trail cell, Trail? next)
		{
			Extent   = cell.Extent;
			Inner    = cell.Inner;
			Stop     = cell.Stop;
			OwnCount = cell.OwnCount;
			OwnTotal = cell.OwnTotal;
			OwnEnd   = cell.OwnEnd;

			Next  = next;
			Count = (next?.Count ?? 0) + OwnCount;
			Total = (next?.Total ?? 0) + OwnTotal;
			End   = Math.Max(next?.End ?? -1, OwnEnd);
		}

		public DslRecognitionExtent Extent { get; }

		/// <summary>Where the extent ends.</summary>
		public int Reached => Extent.Position + Extent.Length;

		public Trail?               Inner  { get; }
		public Trail?               Stop   { get; }
		public Trail?               Next   { get; }

		int OwnCount { get; }
		int OwnTotal { get; }
		int OwnEnd   { get; }

		/// <summary>Extents of rules other than trivia, in the whole list.</summary>
		public int Count { get; }

		/// <summary>Where the furthest of those ends; -1 where there is none.</summary>
		public int End { get; }

		/// <summary>Every extent in the whole list.</summary>
		public int Total { get; }
	}

	/// <summary>One call of a rule: where it began, the call it was made from, and the ends its derivations reached.</summary>
	/// <remarks>
	/// What a call reached is held as the rule's extent of each derivation, whose end is where it
	/// ended: one cell where there is one, which is most calls, and a list only beyond.
	/// </remarks>
	sealed class Entry(int rule, int start, Entry? outer, Trail? head, bool silent, int steps)
	{
		Trail?        _first;
		List<Trail>?  _more;
		HashSet<int>? _ends;

		public int    Rule  { get; } = rule;
		public int    Start { get; } = start;
		public Entry? Outer { get; } = outer;

		/// <summary>The extents read before the call.</summary>
		public Trail? Head { get; } = head;

		/// <summary>Whether it was read inside a look, where its failures were not recorded.</summary>
		public bool Silent { get; } = silent;

		/// <summary>How many steps the machine had taken when the call was made.</summary>
		public int Steps { get; } = steps;

		/// <summary>Records a derivation whose extent is <paramref name="cell"/>; false if one ended there before.</summary>
		public bool Reached(Trail cell)
		{
			var end = cell.Reached;

			if (_first is null)
			{
				_first = cell;

				return true;
			}

			if (_ends is not null)
			{
				if (!_ends.Add(end))
					return false;
			}
			else
			{
				if (_first.Reached == end)
					return false;

				if (_more is not null)
					foreach (var seen in _more)
						if (seen.Reached == end)
							return false;

				// Most calls end in one place or two; a long list is looked up instead.
				if (_more is { Count: 8 })
				{
					_ends = [_first.Reached];

					foreach (var seen in _more)
						_ends.Add(seen.Reached);

					_ends.Add(end);
				}
			}

			(_more ??= []).Add(cell);

			return true;
		}

		/// <summary>What the call reached, for <see cref="Matcher"/> to remember: none, one cell, or an array of them.</summary>
		public object Results => _first is null ? None : _more is null ? _first : (Trail[])[_first, .. _more];

		public static readonly Trail[] None = [];
	}

	enum Step
	{
		/// <summary>Read <see cref="Frame.Node"/>.</summary>
		Node,

		/// <summary>Read the operands of a sequence from <see cref="Frame.A"/> on.</summary>
		Sequence,

		/// <summary>The call <see cref="Frame.Entry"/> has read its body.</summary>
		RuleEnd,

		/// <summary>A capture begun at <see cref="Frame.A"/>, with the extents at <see cref="Frame.Mark"/>, has read its body.</summary>
		CaptureEnd,

		/// <summary>A turn of a repetition begun at <see cref="Frame.A"/> has been read; <see cref="Frame.B"/> turns now.</summary>
		Turn,

		/// <summary>An atomic group has matched: the routes inside it, from choice point <see cref="Frame.A"/> on, are dropped.</summary>
		AtomicEnd,

		/// <summary>A look's body has matched; it stood at <see cref="Frame.B"/>, with choice point <see cref="Frame.A"/>.</summary>
		LookEnd,

		/// <summary>The publication has been read: the text must end here.</summary>
		Accept,

		/// <summary>Nothing may follow: an alternative read again only for what it expected.</summary>
		Halt,
	}

	/// <summary>One thing left to do, in front of the rest; shared by every route that branched after it was made.</summary>
	/// <remarks>Most are a sequence's next operand, made for every operand read, so they carry no more than that needs.</remarks>
	class Frame(Step step, Node? node, Entry entry, Frame? next)
	{
		public Step   Step  { get; } = step;
		public Node?  Node  { get; } = node;
		public Entry  Entry { get; } = entry;
		public Frame? Next  { get; } = next;

		/// <summary>Set again only by a sequence that moves on while no route but the current one holds the frame.</summary>
		public int A { get; set; }

		/// <summary>How many choice points there were when it was made.</summary>
		public int Made { get; init; }
	}

	/// <summary>A frame that remembers more than one number: a capture, a turn, a look.</summary>
	sealed class WideFrame(Step step, Node? node, Entry entry, Frame? next) : Frame(step, node, entry, next)
	{
		public int     B     { get; init; }
		public int     C     { get; init; }
		public Trail?  Mark  { get; init; }
		public object? Extra { get; init; }
	}

	enum Route
	{
		/// <summary>The next alternative of a choice, <see cref="Point.Index"/>.</summary>
		Alternative,

		/// <summary>A repetition stops here.</summary>
		Stop,

		/// <summary>A look's body has no derivation left.</summary>
		Look,

		/// <summary>Every derivation of the call in <see cref="Point.Extra"/> has been followed.</summary>
		Exhausted,

		/// <summary>The next end of a remembered call, <see cref="Point.Index"/>.</summary>
		Replay,
	}

	/// <summary>A route not yet taken, and where everything stood when it branched.</summary>
	struct Point
	{
		public Route   Route;
		public int     Position;
		public Frame?  Continuation;
		public Trail?  Head;
		public Entry   Entry;
		public int     Looking;
		public Node?   Node;
		public int     Index;
		public object? Extra;
	}

	/// <summary>A repetition's turns, reached at each position once: a later route that reaches one again only repeats what failed.</summary>
	/// <remarks>A repetition turns a few times as a rule, so the first positions are held without a set.</remarks>
	sealed class Turns
	{
		int           _first = -1;
		int           _second = -1;
		HashSet<int>? _reached;

		public bool Reach(int position)
		{
			if (_reached is not null)
				return _reached.Add(position);

			if (position == _first || position == _second)
				return false;

			if (_first < 0)
				_first = position;
			else if (_second < 0)
				_second = position;
			else
				_reached = [_first, _second, position];

			return true;
		}
	}

	sealed class ElementTest(CharRange[] ranges, ulong categories, bool references)
	{
		public CharRange[] Ranges     { get; } = ranges;
		public ulong       Categories { get; } = categories;
		public bool        References { get; } = references;
	}

	/// <summary>What a node can begin with: the characters below 128 as a set, whether any above may, and whether it may read nothing.</summary>
	readonly struct Opening(ulong low, ulong high, bool beyond, bool nullable)
	{
		public static readonly Opening None     = new(0, 0, false, false);
		public static readonly Opening Empty    = new(0, 0, false, true);
		public static readonly Opening Anything = new(ulong.MaxValue, ulong.MaxValue, true, true);

		public ulong Low      { get; } = low;
		public ulong High     { get; } = high;
		public bool  Beyond   { get; } = beyond;
		public bool  Nullable { get; } = nullable;

		public bool Holds(char value)
		{
			return value < 64 ? (Low >> value & 1UL) != 0 :
				value < 128 ? (High >> (value - 64) & 1UL) != 0 :
				Beyond;
		}

		public static Opening Of(char value)
		{
			return value < 64 ? new(1UL << value, 0, false, false) :
				value < 128 ? new(0, 1UL << (value - 64), false, false) :
				new(0, 0, true, false);
		}

		/// <summary>Either of the two.</summary>
		public Opening Or(Opening other)
		{
			return new(Low | other.Low, High | other.High, Beyond || other.Beyond, Nullable || other.Nullable);
		}

		/// <summary>This, and where it may read nothing, what follows it too.</summary>
		public Opening Then(Opening next)
		{
			return Nullable
				? new(Low | next.Low, High | next.High, Beyond || next.Beyond, next.Nullable)
				: this;
		}

		public Opening WithNullable(bool nullable)
		{
			return new(Low, High, Beyond, nullable);
		}

		public bool Same(Opening other)
		{
			return Low == other.Low && High == other.High && Beyond == other.Beyond && Nullable == other.Nullable;
		}
	}

	/// <summary>
	/// What every alternative of every choice in a graph, and the body of every repetition, can
	/// begin with: worked out once a graph, and only read after.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The machine asks it before it tries an alternative or a turn, and passes over one that
	/// cannot begin with the character in front of it — most of a choice between words is that.
	/// It errs one way only: a node it cannot tell about may begin with anything, and may read
	/// nothing.
	/// </para>
	/// <para>
	/// What a rule begins with is the least answer that satisfies every rule at once, worked out
	/// by revisiting the callers of each rule whose answer grew; an answer only grows, and each is
	/// bounded, so it settles.
	/// </para>
	/// </remarks>
	sealed class Openings
	{
		static readonly System.Runtime.CompilerServices.ConditionalWeakTable<RecognitionGraph, Openings> _byGraph = new();

		readonly Dictionary<RuleSymbol, Opening> _rules = [];
		readonly Dictionary<Node, Opening[]>     _choices = new(NodeIdentity.Instance);
		readonly Dictionary<Node, Opening>       _repeats = new(NodeIdentity.Instance);

		Openings(RecognitionGraph graph)
		{
			var callers = new Dictionary<RuleSymbol, List<RuleSymbol>>();

			foreach (var rule in graph.Bodies.Keys)
				_rules[rule] = Opening.None;

			foreach (var pair in graph.Bodies)
				foreach (var node in Nodes(pair.Value))
					if (node is Node.Call call)
					{
						if (!callers.TryGetValue(call.Rule, out var list))
							callers.Add(call.Rule, list = []);

						list.Add(pair.Key);
					}

			var pending = new Queue<RuleSymbol>(graph.Bodies.Keys);
			var queued  = new HashSet<RuleSymbol>(graph.Bodies.Keys);

			while (pending.Count > 0)
			{
				var rule = pending.Dequeue();

				queued.Remove(rule);

				var known = _rules[rule];
				var now   = known.Or(Begins(graph.Bodies[rule]));

				if (now.Same(known))
					continue;

				_rules[rule] = now;

				if (callers.TryGetValue(rule, out var calling))
					foreach (var caller in calling)
						if (queued.Add(caller))
							pending.Enqueue(caller);
			}

			foreach (var body in graph.Bodies.Values.Concat(graph.Trivia.Values))
				foreach (var node in Nodes(body))
					switch (node)
					{
						case Node.Choice { Selection: null } choice when !_choices.ContainsKey(choice):
							_choices.Add(choice, [.. choice.Nodes.Select(Begins)]);
							break;

						case Node.Repeat repeat when !_repeats.ContainsKey(repeat):
							_repeats.Add(repeat, Begins(repeat.Body));
							break;
					}
		}

		public static Openings Of(RecognitionGraph graph)
		{
			return _byGraph.GetValue(graph, static one => new Openings(one));
		}

		public Opening[]? Of(Node.Choice choice)
		{
			return _choices.TryGetValue(choice, out var openings) ? openings : null;
		}

		public Opening? Of(Node.Repeat repeat)
		{
			return _repeats.TryGetValue(repeat, out var opening) ? opening : null;
		}

		/// <summary>Every node of a body, without recursion.</summary>
		static IEnumerable<Node> Nodes(Node body)
		{
			var pending = new Stack<Node>();

			pending.Push(body);

			while (pending.Count > 0)
			{
				var node = pending.Pop();

				yield return node;

				foreach (var child in node.Children)
					pending.Push(child);
			}
		}

		Opening Begins(Node node)
		{
			// Follows the nesting of one body, which the grammar's text bounds; a chain of calls is read off the rules.
			if (DeepStack.Low)
				return DeepStack.Run(() => Begins(node));

			switch (node)
			{
				case Node.Empty or Node.Guard or Node.Lookahead or Node.Behind or Node.Glue or Node.Reading or Node.Condition:
					return Opening.Empty;

				case Node.Literal { Text.Length: 0 }:
					return Opening.Empty;

				case Node.Literal literal:
				{
					var opening = Opening.Of(literal.Text[0]);

					if (literal.IgnoreCase)
						foreach (var folded in CaseFold.Of(literal.Text[0]))
							opening = opening.Or(Opening.Of(folded));

					return opening;
				}

				case Node.Element element:
					return Begins(element);

				case Node.Sequence sequence:
				{
					var opening = Opening.Empty;

					foreach (var part in sequence.Nodes)
					{
						opening = opening.Then(Begins(part));

						if (!opening.Nullable)
							break;
					}

					return opening;
				}

				case Node.Choice { Selection: null } choice:
				{
					var opening = Opening.None;

					foreach (var alternative in choice.Nodes)
						opening = opening.Or(Begins(alternative));

					return opening;
				}

				case Node.Atomic atomic:
					return Begins(atomic.Body);

				case Node.Marked marked:
					return Begins(marked.Body);

				case Node.Construct construct:
					return Begins(construct.Body);

				case Node.Capture capture:
					return Begins(capture.Body);

				case Node.Repeat repeat:
				{
					var body = Begins(repeat.Body);

					return repeat.Min == 0 ? body.WithNullable(true) : body;
				}

				case Node.Call call:
					return _rules.TryGetValue(call.Rule, out var called) ? called : Opening.None;

				// What the host computes or a binding holds: anything, and perhaps nothing.
				default:
					return Opening.Anything;
			}
		}

		static Opening Begins(Node.Element element)
		{
			if (element.References.Count > 0 || element.IsNegated || element.Categories.Count > 0)
			{
				// Asked character by character below 128, and allowed anything above.
				if (element.References.Count > 0)
					return Opening.Anything.WithNullable(false);

				var opening = Opening.None;

				for (var value = '\0'; value < 128; value++)
					if (Known(element, value) != element.IsNegated)
						opening = opening.Or(Opening.Of(value));

				return opening.Or(new Opening(0, 0, true, false));
			}

			var ranges = Opening.None;

			foreach (var range in element.Ranges)
			{
				for (var value = range.From; value <= range.To && value < 128; value++)
					ranges = ranges.Or(Opening.Of(value));

				if (range.To >= 128)
					ranges = ranges.Or(new Opening(0, 0, true, false));
			}

			return ranges;
		}

		static bool Known(Node.Element element, char value)
		{
			foreach (var range in element.Ranges)
				if (range.From <= value && value <= range.To)
					return true;

			var category = UnicodeCategories.Of(value).ToString();

			foreach (var item in element.Categories)
				if (UnicodeCategories.Expand(item).Contains(category))
					return true;

			return false;
		}
	}

	sealed class Matcher
	{
		readonly RecognitionGraph         _graph;
		readonly string                   _input;
		readonly IDslRecognitionContract? _contract;
		readonly int                      _reading;
		readonly Entry                    _root;
		readonly Node                     _call;
		readonly Node?                    _trivia;

		readonly Dictionary<RuleSymbol, int> _numbers = [];
		readonly List<RuleSymbol>            _rules   = [];
		readonly List<Node?>                 _bodies  = [];
		readonly List<bool>                  _isTrivia = [];

		readonly Dictionary<Node, int>         _calls    = new(NodeIdentity.Instance);
		readonly Dictionary<Node, ElementTest> _elements = new(NodeIdentity.Instance);
		readonly Dictionary<Node, string>      _said     = new(NodeIdentity.Instance);
		// Calls every derivation of which has been followed, by rule and position: what Entry.Results says.
		// Those read inside a look are kept apart: replayed outside one, the failures they did not record
		// would be missing from the refusal.
		readonly Dictionary<long, object> _remembered = [];
		readonly Dictionary<long, object> _rememberedSilently = [];
		readonly HashSet<string> _expected = new(StringComparer.Ordinal);

		// Where the machine stands.
		int     _position;
		Frame?  _continuation;
		Trail?  _head;
		Entry   _entry;
		int     _looking;
		Node?   _node;
		bool    _failed;
		int     _steps;
		Point[] _points = new Point[64];
		int     _count;

		// A rule called again where it is being read: the graph was not normalized, and what a call
		// answers then depends on the calls around it, so no call is remembered from then on.
		bool _cyclic;

		// Every alternative and turn is first asked whether it can begin with the character it stands at.
		// Those passed over where a refusal may yet stand are kept, for Explain.
		readonly Openings _openings;
		readonly List<(Node Node, Entry Entry, int Position)> _passedOver = [];
		int  _passedOverKept = 64;
		bool _explaining;

		Trail? _best;
		int    _bestEnd;
		int    _bestCount;
		Trail? _candidate;
		int    _candidateEnd = -1;
		int    _candidateTotal;

		public Matcher(RecognitionGraph graph, Publication publication, string input, IDslRecognitionContract? contract)
		{
			_graph    = graph;
			_input    = input;
			_contract = contract;
			_reading  = publication.Reading;
			_root     = new Entry(Number(publication.Rule), -1, null, null, false, 0);
			_entry    = _root;
			_call     = new Node.Call(publication.Rule, []);
			_trivia   = graph.Trivia.TryGetValue(publication.Rule, out var trivia) ? trivia : null;
			_openings = Openings.Of(graph);
		}

		public int  Furthest    { get; private set; }
		public bool Unsupported { get; private set; }

		/// <summary>
		/// What a refusal highlights: the derivation that read the publication furthest where one
		/// did and the text went on after it, else the route that got furthest with what it read.
		/// </summary>
		public Trail? Partial => _candidate ?? _best;

		public IReadOnlyList<string> Expected => _expected.OrderBy(static item => item, StringComparer.Ordinal).ToArray();

		/// <summary>The extents of the first derivation that reads the whole text, or null where none does.</summary>
		public Trail? Run()
		{
			var accept = new Frame(Step.Accept, null, _root, null);

			_continuation = _trivia is null
				? new Frame(Step.Node, _call, _root, accept)
				: new Frame(Step.Node, _trivia, _root,
					new Frame(Step.Node, _call, _root,
						new Frame(Step.Node, _trivia, _root, accept)));

			if (Drive())
				return _head;

			Explain();

			return null;
		}

		/// <summary>Runs the machine until the text has been read, or no route is left.</summary>
		bool Drive()
		{
			while (true)
			{
				_steps++;

				if (_failed)
				{
					_failed = false;

					if (!Backtrack())
						return false;
				}
				else if (_node is { } node)
				{
					_node = null;
					Read(node);
				}
				else
				{
					var frame = _continuation!;

					_continuation = frame.Next;

					if (Resume(frame))
						return true;
				}
			}
		}

		/// <summary>
		/// Reads the alternatives that were passed over where the refusal stands, for what they
		/// would have said was expected there.
		/// </summary>
		/// <remarks>
		/// An alternative passed over could not begin with the character it stood at, so it fails
		/// right there, and reading it now records what reading it then would have; which failure
		/// is recorded first makes no difference to what is recorded in the end. The partial reading
		/// is the one already chosen; a passed-over alternative reads nothing to add to it.
		/// </remarks>
		void Explain()
		{
			_explaining = true;

			foreach (var (node, entry, position) in _passedOver.ToArray())
			{
				if (position < Furthest)
					continue;

				Cut(0);

				_position     = position;
				_entry        = entry;
				_looking      = 0;
				_head         = null;
				_failed       = false;
				_node         = node;
				_continuation = new Frame(Step.Halt, null, entry, null);

				Drive();
			}
		}

		int Number(RuleSymbol rule)
		{
			if (_numbers.TryGetValue(rule, out var number))
				return number;

			number = _rules.Count;

			_numbers.Add(rule, number);
			_rules.Add(rule);
			_bodies.Add(_graph.Bodies.TryGetValue(rule, out var body) ? body : null);
			_isTrivia.Add(rule.Name == "trivia");

			return number;
		}

		void Fail()
		{
			_failed = true;
		}

		void Push(Route route, Node? node = null, int index = 0, object? extra = null)
		{
			if (_count == _points.Length)
				Array.Resize(ref _points, _points.Length * 2);

			_points[_count++] = new Point
			{
				Route        = route,
				Position     = _position,
				Continuation = _continuation,
				Head         = _head,
				Entry        = _entry,
				Looking      = _looking,
				Node         = node,
				Index        = index,
				Extra        = extra,
			};
		}

		void Restore(in Point point)
		{
			_position     = point.Position;
			_continuation = point.Continuation;
			_head         = point.Head;
			_entry        = point.Entry;
			_looking      = point.Looking;
		}

		/// <summary>Drops the routes from choice point <paramref name="marker"/> on.</summary>
		void Cut(int marker)
		{
			Array.Clear(_points, marker, _count - marker);
			_count = marker;
		}

		/// <summary>Takes the newest route not yet taken; false where none is left.</summary>
		bool Backtrack()
		{
			while (_count > 0)
			{
				ref var point = ref _points[_count - 1];

				switch (point.Route)
				{
					case Route.Alternative:
					{
						var choice = (Node.Choice)point.Node!;
						var index  = point.Index;

						Restore(point);

						var next = Viable(choice, index + 1);

						if (next < choice.Nodes.Count)
							point.Index = next;
						else
							Cut(_count - 1);

						_node = choice.Nodes[index];

						return true;
					}

					case Route.Stop:
						Restore(point);
						Cut(_count - 1);

						return true;

					case Route.Look:
					{
						var look = (Node.Lookahead)point.Node!;

						Restore(point);
						Cut(_count - 1);

						if (!look.IsPositive)
							return true;

						// A refused look fails where it stands, as a generated parser's does.
						Expect(_position, Said(look), _entry.Rule);

						continue;
					}

					case Route.Exhausted:
					{
						var entry = (Entry)point.Extra!;

						Cut(_count - 1);

						if (!_cyclic)
							Remember(entry, entry.Results);

						continue;
					}

					case Route.Replay:
					{
						var results = (Trail[])point.Extra!;
						var index   = point.Index;

						Restore(point);

						if (index + 1 < results.Length)
							point.Index++;
						else
							Cut(_count - 1);

						Replayed(results[index]);

						return true;
					}
				}
			}

			return false;
		}

		/// <summary>How many steps a call has to have taken to be remembered.</summary>
		const int Worth = 16;

		static long Key(int rule, int position)
		{
			return (long)rule << 32 | (uint)position;
		}

		/// <summary>Carries on with what <paramref name="frame"/> says is left to do; true where the text has been read.</summary>
		bool Resume(Frame frame)
		{
			switch (frame.Step)
			{
				case Step.Node:
					_entry = frame.Entry;
					_node  = frame.Node;

					return false;

				case Step.Sequence:
				{
					var sequence = (Node.Sequence)frame.Node!;
					var index    = frame.A;

					_entry = frame.Entry;
					Consider(_entry.Rule);

					// Where every choice point made since the frame was is gone, so is every route that could
					// come back to this operand: the frame moves on instead of being copied.
					if (index + 1 < sequence.Nodes.Count)
					{
						if (frame.Made == _count)
						{
							frame.A       = index + 1;
							_continuation = frame;
						}
						else
							_continuation = new Frame(Step.Sequence, sequence, _entry, _continuation) { A = index + 1, Made = _count };
					}

					_node = sequence.Nodes[index];

					return false;
				}

				case Step.RuleEnd:
				{
					var entry = frame.Entry;
					var cell  = new Trail(
						new DslRecognitionExtent(_rules[entry.Rule], null, entry.Start, _position - entry.Start),
						!_isTrivia[entry.Rule],
						_head,
						entry.Head,
						entry.Head);

					// Another derivation of this call that ends where one did before reads on exactly as it did.
					if (!entry.Reached(cell))
					{
						Fail();

						return false;
					}

					_head = cell;
					Consider(entry.Rule);

					// A body that left no route behind has no derivation after this one: the call is done with
					// now, rather than when the reading backs out past it, and its choice point goes. This need
					// not be its first derivation: the routes that led to the others may have run out before it.
					if (_count > 0 && _points[_count - 1].Extra == entry)
					{
						Cut(_count - 1);

						if (!_cyclic)
							Remember(entry, entry.Results);
					}

					return false;
				}

				case Step.CaptureEnd:
				{
					var owner = frame.Entry.Rule;
					var mark  = ((WideFrame)frame).Mark;

					_head = new Trail(
						new DslRecognitionExtent(_rules[owner], ((Node.Capture)frame.Node!).Name, frame.A, _position - frame.A),
						!_isTrivia[owner],
						_head,
						mark,
						mark);
					Consider(owner);

					return false;
				}

				case Step.Turn:
					// A turn that read nothing would turn for ever.
					if (_position == frame.A)
					{
						Fail();

						return false;
					}

					_entry = frame.Entry;
					Turn((Node.Repeat)frame.Node!, ((WideFrame)frame).B, (Turns?)((WideFrame)frame).Extra);

					return false;

				case Step.AtomicEnd:
					Cut(frame.A);

					return false;

				case Step.LookEnd:
				{
					var look = (Node.Lookahead)frame.Node!;

					// The first derivation of the body answers the look; the others are not wanted.
					Cut(frame.A);

					var wide = (WideFrame)frame;

					_looking  = wide.C;
					_position = wide.B;
					_head     = wide.Mark;

					if (!look.IsPositive)
					{
						Expect(_position, Said(look), frame.Entry.Rule);
						Fail();
					}

					return false;
				}

				case Step.Accept:
					if (_position == _input.Length)
						return true;

					Expect(_position, "end of input", null);

					if (_position > _candidateEnd || _position == _candidateEnd && (_head?.Total ?? 0) > _candidateTotal)
					{
						_candidate      = _head;
						_candidateEnd   = _position;
						_candidateTotal = _head?.Total ?? 0;
					}

					Fail();

					return false;

				case Step.Halt:
					Fail();

					return false;

				default:
					throw new InvalidOperationException($"Unknown step {frame.Step}.");
			}
		}

		void Read(Node node)
		{
			switch (node)
			{
				case Node.Empty:
					return;

				case Node.Literal literal:
				{
					var text = literal.Text;

					if (_position + text.Length <= _input.Length &&
						(literal.IgnoreCase
							? CaseFold.Matches(text, _input.AsSpan(_position, text.Length))
							: _input.AsSpan(_position, text.Length).SequenceEqual(text.AsSpan())))
					{
						_position += text.Length;

						return;
					}

					if (Records(_position, _entry.Rule))
					{
						var matched = 0;

						while (matched < text.Length && _position + matched < _input.Length &&
							(literal.IgnoreCase
								? CaseFold.Matches(text[matched], _input[_position + matched])
								: text[matched] == _input[_position + matched]))
							matched++;

						Expect(_position + matched, SaidOf(literal), _entry.Rule);
					}

					Fail();

					return;
				}

				case Node.Element element:
					if (_position < _input.Length && Matches(element, _input[_position]))
					{
						_position++;

						return;
					}

					if (element.References.Count == 0 && Records(_position, _entry.Rule))
						Expect(_position, SaidOf(element), _entry.Rule);

					Fail();

					return;

				case Node.Sequence sequence:
					Consider(_entry.Rule);

					if (sequence.Nodes.Count == 0)
						return;

					if (sequence.Nodes.Count > 1)
						_continuation = new Frame(Step.Sequence, sequence, _entry, _continuation) { A = 1, Made = _count };

					_node = sequence.Nodes[0];

					return;

				case Node.Choice { Selection: not null }:
					// A dispatch on a value the host's C# computes, which nothing here may run.
					Unsupported = true;
					Fail();

					return;

				case Node.Choice choice:
				{
					var first = Viable(choice, 0);

					if (first == choice.Nodes.Count)
					{
						Fail();

						return;
					}

					// The next alternative is found now, so that a choice left with one is no choice point.
					var next = Viable(choice, first + 1);

					if (next < choice.Nodes.Count)
						Push(Route.Alternative, choice, next);

					_node = choice.Nodes[first];

					return;
				}

				case Node.Atomic atomic:
					_continuation = new Frame(Step.AtomicEnd, atomic, _entry, _continuation) { A = _count };
					_node         = atomic.Body;

					return;

				case Node.Repeat repeat:
					// A turn that can end in one place only is never reached again by another route.
					Turn(repeat, 0, repeat.Max is null && repeat.Body is not (Node.Element or Node.Literal or Node.Atomic) ? new Turns() : null);

					return;

				case Node.Lookahead look:
					Push(Route.Look, look);

					_continuation = new WideFrame(Step.LookEnd, look, _entry, _continuation)
					{
						A    = _count - 1,
						B    = _position,
						C    = _looking,
						Mark = _head,
					};

					// A look does not report how far it looked: what fails inside one is no refusal of the text.
					_looking++;
					_node = look.Body;

					return;

				case Node.Capture capture:
					_continuation = new WideFrame(Step.CaptureEnd, capture, _entry, _continuation) { A = _position, Mark = _head };
					_node         = capture.Body;

					return;

				case Node.Construct construct:
					_node = construct.Body;

					return;

				case Node.Marked marked:
					_node = marked.Body;

					return;

				case Node.Call call:
					if (!_calls.TryGetValue(call, out var called))
						_calls.Add(call, called = Number(call.Rule));

					Enter(called);

					return;

				case Node.Behind behind:
					// The other half of a word literal's boundary: the item before it is no word character.
					if (_position == 0 || !Matches(behind.Test, _input[_position - 1]))
						return;

					Expect(_position, null, _entry.Rule);
					Fail();

					return;

				case Node.Glue:
					// Over characters adjacency is the absence of a seam, which the normalizer already left out.
					return;

				case Node.Reading which:
					if (((which.Readings >> _reading) & 1UL) == 0UL)
						Fail();

					return;

				case Node.Guard guard:
					if (_contract is null ||
						!_contract.TryEvaluateGuard(guard, _rules[_entry.Rule], _position, out var accepted))
					{
						Unsupported = true;
						Fail();

						return;
					}

					if (!accepted)
						Fail();

					return;

				case Node.External external:
					if (_contract is null ||
						!_contract.TryResolveExternal(external, _rules[_entry.Rule], out var externalRule))
					{
						Unsupported = true;
						Fail();

						return;
					}

					Enter(Number(externalRule));

					return;

				// Text bindings are read against what an earlier operand matched on this route, which
				// this model does not keep; a condition left in the graph is one the normalizer did not decide.
				case Node.Bind or Node.Same or Node.Counted or Node.Condition:
					Unsupported = true;
					Fail();

					return;

				default:
					// A kind this model does not read: the site is one it cannot read, not one the text fails.
					Unsupported = true;
					Fail();

					return;
			}
		}

		void Enter(int rule)
		{
			if (_bodies[rule] is not { } body)
			{
				Fail();

				return;
			}

			// A rule called again where it is being read, before it has read anything, would never end.
			for (var entry = _entry; entry is not null && entry.Start == _position; entry = entry.Outer)
				if (entry.Rule == rule)
				{
					_cyclic = true;
					Fail();

					return;
				}

			if (!_cyclic && Remembered(rule) is { } remembered)
			{
				if (remembered is Trail one)
				{
					Replayed(one);

					return;
				}

				var results = (Trail[])remembered;

				if (results.Length == 0)
				{
					Fail();

					return;
				}

				Push(Route.Replay, index: 1, extra: results);
				Replayed(results[0]);

				return;
			}

			var called = new Entry(rule, _position, _entry, _head, _looking > 0, _steps);

			Push(Route.Exhausted, extra: called);

			_continuation = new Frame(Step.RuleEnd, null, called, _continuation);
			_entry        = called;
			_node         = body;
		}

		void Remember(Entry entry, object results)
		{
			// A call read in a few steps is read again as cheaply as it is replayed.
			if (_steps - entry.Steps < Worth)
				return;

			(entry.Silent ? _rememberedSilently : _remembered)[Key(entry.Rule, entry.Start)] = results;
		}

		object? Remembered(int rule)
		{
			var key = Key(rule, _position);

			return _remembered.TryGetValue(key, out var remembered) ||
				_looking > 0 && _rememberedSilently.TryGetValue(key, out remembered)
					? remembered
					: null;
		}

		void Replayed(Trail result)
		{
			_head     = new Trail(result, _head);
			_position = result.Reached;
		}

		/// <summary>Arrives at a repetition with <paramref name="count"/> turns read: one more first, then stopping.</summary>
		void Turn(Node.Repeat repeat, int count, Turns? turns)
		{
			var stops = count >= repeat.Min;
			var turn  = repeat.Max is not { } most || count < most;

			if (turns is not null && stops && !turns.Reach(_position))
			{
				Fail();

				return;
			}

			if (!turn)
			{
				if (!stops)
					Fail();

				return;
			}

			// A turn that cannot begin here leaves stopping as the one route, and no choice point.
			if (_openings.Of(repeat) is { } opening && !Opens(opening, repeat.Body))
			{
				if (!stops)
					Fail();

				return;
			}

			if (stops)
				Push(Route.Stop);

			_continuation = new WideFrame(Step.Turn, repeat, _entry, _continuation) { A = _position, B = count + 1, Extra = turns };
			_node         = repeat.Body;
		}

		/// <summary>The first alternative of <paramref name="choice"/> from <paramref name="from"/> on that can begin here; the count where none can.</summary>
		int Viable(Node.Choice choice, int from)
		{
			if (_explaining || _openings.Of(choice) is not { } openings)
				return from;

			for (var index = from; index < openings.Length; index++)
				if (Opens(openings[index], choice.Nodes[index]))
					return index;

			return openings.Length;
		}

		/// <summary>Whether what <paramref name="opening"/> describes can begin at the current position; if not, <paramref name="node"/> is passed over.</summary>
		bool Opens(in Opening opening, Node node)
		{
			if (opening.Nullable || _explaining)
				return true;

			if (_position < _input.Length && opening.Holds(_input[_position]))
				return true;

			// Only where a failure would still be recorded does what the node expected matter.
			if (_looking == 0 && _position >= Furthest)
			{
				// Those the refusal has moved past are dropped now and then, so that the list stays short.
				if (_passedOver.Count == _passedOverKept)
				{
					_passedOver.RemoveAll(one => one.Position < Furthest);
					_passedOverKept = Math.Max(64, _passedOver.Count * 2);
				}

				_passedOver.Add((node, _entry, _position));
			}

			return false;
		}

		void Consider(int owner)
		{
			if (_isTrivia[owner] || _explaining)
				return;

			var count = _head?.Count ?? 0;
			var end   = count == 0 ? _position : _head!.End;

			if (end > _bestEnd || end == _bestEnd && count > _bestCount)
			{
				_best      = _head;
				_bestEnd   = end;
				_bestCount = count;
			}
		}

		/// <summary>Whether a failure at <paramref name="position"/> in <paramref name="owner"/> would be recorded.</summary>
		bool Records(int position, int owner)
		{
			return _looking == 0 && position >= Furthest && !_isTrivia[owner];
		}

		/// <summary>
		/// Records a failure at <paramref name="position"/>; <paramref name="expected"/> null places it
		/// without naming anything, as a refused look does.
		/// </summary>
		void Expect(int position, string? expected, int? owner)
		{
			if (owner is { } rule && _isTrivia[rule])
				return;

			if (_looking > 0 || position < Furthest)
				return;

			if (position > Furthest)
			{
				Furthest = position;
				_expected.Clear();
			}

			if (expected is not null)
				_expected.Add(expected);
		}

		string SaidOf(Node node)
		{
			if (!_said.TryGetValue(node, out var said))
				_said.Add(node, said = node.ToString());

			return said;
		}

		/// <summary>
		/// What a refused look names: <c>eof</c> the end of the input, any other look nothing — what
		/// was wanted there is said by what else could have been read.
		/// </summary>
		static string? Said(Node.Lookahead look)
		{
			return look.Body is Node.Element { IsNegated: true, Ranges.Count: 0, Categories.Count: 0, References.Count: 0 }
				? "end of input"
				: null;
		}

		bool Matches(Node.Element element, char value)
		{
			if (!_elements.TryGetValue(element, out var test))
				_elements.Add(element, test = Prepared(element));

			var known = false;

			foreach (var range in test.Ranges)
				if (range.From <= value && value <= range.To)
				{
					known = true;
					break;
				}

			if (!known && test.Categories != 0)
				known = (test.Categories >> (int)UnicodeCategories.Of(value) & 1UL) != 0;

			if (!known && test.References)
				Unsupported = true;

			return element.IsNegated ? !known && !test.References : known;
		}

		static ElementTest Prepared(Node.Element element)
		{
			var categories = 0UL;

			foreach (var item in element.Categories)
				foreach (var name in UnicodeCategories.Expand(item))
					if (Enum.TryParse<UnicodeCategory>(name, out var category))
						categories |= 1UL << (int)category;

			return new ElementTest([.. element.Ranges], categories, element.References.Count > 0);
		}
	}
}
