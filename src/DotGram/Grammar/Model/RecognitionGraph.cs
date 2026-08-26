using System;
using System.Text;

using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Model;

/// <summary>An inclusive range of input items.</summary>
public readonly record struct CharRange(char From, char To)
{
	public bool IsSingle => From == To;

	public override string ToString() => IsSingle ? Quote(From) : $"{Quote(From)}..{Quote(To)}";

	internal static string Quote(char value) => value switch
	{
		'\n' => @"'\n'",
		'\r' => @"'\r'",
		'\t' => @"'\t'",
		'\'' => @"'\''",
		'\\' => @"'\\'",
		_    => $"'{value}'",
	};
}

/// <summary>
/// What a grammar means, stripped of how it was written.
/// </summary>
/// <remarks>
/// <para>
/// Flat, like the shapes it comes from, and with almost no locations: a graph node is
/// not a place in a file, and diagnostics raised here point at the rule being normalized,
/// whose declaration knows where it is. The exception is the C# a grammar hands over
/// verbatim — a <c>when</c> or a <c>=&gt;</c> — which keeps where it was written, because
/// the C# compiler will have things of its own to say about it and has to say them on the
/// grammar's line (§7.6).
/// </para>
/// <para>
/// Deliberately smaller than the syntax tree: groups are flattened, literals merged,
/// character alternatives folded into sets. What it keeps is rule boundaries — those
/// are what diagnostics are phrased in terms of, so inlining rules the way Roc's macro
/// did would buy speed at the cost of every message.
/// </para>
/// </remarks>
public abstract record Node
{
	/// <summary>
	/// The operands this node is written over, in the order the notation writes them.
	/// </summary>
	/// <remarks>
	/// A record's members are its structure, so this is not an index kept beside the tree
	/// that can drift out of step with it — a node that gains an operand gains it here.
	/// Computed, so it stays out of equality.
	/// </remarks>
	public virtual IEnumerable<Node> Children => [];

	/// <summary>Matches nothing at all, and succeeds. What `none` lowers to.</summary>
	public sealed record Empty : Node
	{
		public static readonly Empty Instance = new();

		public override string ToString() => "none";
	}

	/// <summary>One input item drawn from a set.</summary>
	public sealed record Element(
		bool                     IsNegated,
		IReadOnlyList<CharRange> Ranges,
		IReadOnlyList<string>    Categories,
		IReadOnlyList<Symbol>    References) : Node
	{
		public override string ToString()
		{
			var parts = new List<string>();

			foreach (var range in Ranges)         parts.Add(range.ToString());
			foreach (var category in Categories)  parts.Add($@"\p{{{category}}}");
			foreach (var reference in References) parts.Add(reference.Name);

			return $"[{(IsNegated ? "^ " : "")}{string.Join(" | ", parts)}]";
		}
	}

	/// <summary>A fixed run of input items.</summary>
	public sealed record Literal(string Text) : Node
	{
		/// <summary>Whether this matches without regard to case (`"text"i`, `'x'i`).</summary>
		public bool IgnoreCase { get; init; }

		public override string ToString() =>
			(Text.Length == 1 ? CharRange.Quote(Text[0]) : $"\"{Text}\"") + (IgnoreCase ? "i" : "");
	}

	public sealed record Sequence(IReadOnlyList<Node> Nodes) : Node
	{
		public override IEnumerable<Node> Children => Nodes;

		public override string ToString() => string.Join(" & ", Nodes);
	}

	public sealed record Choice(IReadOnlyList<Node> Nodes) : Node
	{
		public override IEnumerable<Node> Children => Nodes;

		public override string ToString() => $"({string.Join(" | ", Nodes)})";
	}

	/// <summary>An explicit commit-on-success backtracking boundary.</summary>
	public sealed record Atomic(Node Body) : Node
	{
		public override IEnumerable<Node> Children => [Body];

		public override string ToString() => $"{{ {Body} }}";
	}

	public sealed record Repeat(Node Body, int Min, int? Max) : Node
	{
		public override IEnumerable<Node> Children => [Body];

		public override string ToString() => (Min, Max) switch
		{
			(0, 1)                         => $"{Repeated}?",
			(0, null)                      => $"{Repeated}*",
			(1, null)                      => $"{Repeated}+",
			var (min, max) when min == max => $"{Repeated}{{{min}}}",
			(var min, null)                => $"{Repeated}{{{min},}}",
			var (min, max)                 => $"{Repeated}{{{min},{max}}}",
		};

		/// <summary>
		/// The body, bracketed where the quantifier would otherwise read as applying to the
		/// last operand alone. A choice brackets itself; a capture binds tighter than a
		/// quantifier anyway (§10), so only a sequence needs it.
		/// </summary>
		string Repeated => Body is Sequence ? $"({Body})" : Body.ToString();
	}

	public sealed record Lookahead(bool IsPositive, Node Body) : Node
	{
		public override IEnumerable<Node> Children => [Body];

		public override string ToString() => $"{(IsPositive ? "?=" : "?!")}{Body}";
	}

	/// <summary>
	/// Matches nothing, and only where the preceding input item, if any, is outside
	/// <paramref name="Test"/>.
	/// </summary>
	/// <remarks>
	/// §4.6's other half. The boundary lookahead keeps a word literal from being the
	/// start of a longer word; this keeps it from being the <em>end</em> of one — the
	/// reading backtracking otherwise finds, where an identifier hands characters back
	/// and a keyword matches mid-word. Woven by the normalizer beside the lookahead and
	/// never written by an author, which is why it needs no surface syntax. The test is
	/// an element rather than a rule: one character of lookbehind is all §4.6 asks, and
	/// all the engine emits — a single comparison against <c>text[p - 1]</c>.
	/// </remarks>
	public sealed record Behind(Element Test) : Node
	{
		public override string ToString() => $"?<!{Test}";
	}

	public sealed record Capture(string Name, Node Body) : Node
	{
		public override IEnumerable<Node> Children => [Body];

		public override string ToString() => $"{Name}: {Body}";
	}

	/// <summary>A `when` guard. Consumes nothing.</summary>
	/// <param name="At">
	/// Where the C# starts in the grammar, so that a C# error in it can be reported there
	/// (§7.6). -1 for text this compiler wrote rather than read.
	/// </param>
	public sealed record Guard(string Text, int At = -1) : Node
	{
		/// <summary>Without the position, for the passes that only read the C#.</summary>
		public void Deconstruct(out string text) => text = Text;

		public override string ToString() => $"when {Text}";
	}

	/// <summary>
	/// A `=>` construction, or one of the four the language supplies in its place.
	/// Consumes nothing, runs after the alternative matched.
	/// </summary>
	public sealed record Construct(Node Body, Construction How) : Node
	{
		public override IEnumerable<Node> Children => [Body];

		public override string ToString() => $"{Body} => {How}";
	}

	/// <summary>
	/// A C# method that reads the input itself (docs/syntax.md §7.1).
	/// </summary>
	/// <remarks>
	/// <para>
	/// <c>bool M(ReadOnlySpan&lt;char&gt; input, ref int pos)</c>. The <c>ref</c> is the
	/// method saying it moves the position, and that is taken at its word: it is handed
	/// the parser's own position and nothing checks what it did with it. A grammar that
	/// reaches into the parse takes the parse's invariants on, which §7.1 says in words.
	/// </para>
	/// <para>
	/// Consumes whatever it says it consumed, and its value is the text it covered — the
	/// same as any rule that captures nothing. §7.1's third row is the same call with an
	/// extra <c>out T value</c> parameter, handing back a value of its own instead —
	/// <see cref="HasValue"/> says which; recognition either way is identical, so the node
	/// stays a name-only leaf and never carries <c>T</c> itself (that lives on the
	/// synthesized <see cref="RuleSymbol"/> a value-returning reference is wrapped in
	/// during lowering, in <see cref="RecognitionGraph.Externals"/>).
	/// </para>
	/// </remarks>
	public sealed record External(string Name) : Node
	{
		/// <summary>Whether this is §7.1's third row rather than its second.</summary>
		public bool HasValue { get; init; }

		public override string ToString() => "@" + Name;
	}

	/// <summary>A call to another rule; rule boundaries survive normalization.</summary>
	public sealed record Call(RuleSymbol Rule, IReadOnlyList<Node> Arguments) : Node
	{
		/// <remarks>
		/// The arguments, not the called rule's body: a call is a boundary, and what is on
		/// the other side of it belongs to that rule (§4).
		/// </remarks>
		public override IEnumerable<Node> Children => Arguments;

		public override string ToString() =>
			Arguments.Count == 0 ? Rule.Name : $"{Rule.Name}({string.Join(", ", Arguments)})";
	}
}

/// <summary>
/// The whole grammar after normalization: one node per rule, plus what was worked out
/// about them.
/// </summary>
public sealed class RecognitionGraph(
	IReadOnlyList<RuleSymbol>                                    rules,
	IReadOnlyDictionary<RuleSymbol, Node>                        bodies,
	IReadOnlyDictionary<RuleSymbol, bool>                        nullable,
	IReadOnlyDictionary<RuleSymbol, IReadOnlyList<ResultMember>> results,
	IReadOnlyDictionary<RuleSymbol, string>                      types,
	IReadOnlyList<string>                                        cSharpImports,
	IReadOnlyList<Publication>                                   publications,
	IReadOnlyList<GramDiagnostic>                                diagnostics)
{
	/// <summary>
	/// The C# type a rule declared for itself with <c>: @T</c>, where one did. Written as
	/// the grammar wrote it: what it resolves to is C#'s business (§7.4).
	/// </summary>
	public IReadOnlyDictionary<RuleSymbol, string> Types { get; } = types;

	/// <summary>The grammar's <c>@using</c> directives, which the generated file needs.</summary>
	public IReadOnlyList<string> CSharpImports { get; } = cSharpImports;

	/// <summary>
	/// The <c>trivia</c> each rule sees, where it matches anything at all (§4.5).
	/// </summary>
	/// <remarks>
	/// Normalization inserts it between operands, which leaves the two ends of a whole
	/// parse: nothing precedes the first operand and nothing follows the last. Publication
	/// is where those are, so publication is where this is needed.
	/// </remarks>
	public IReadOnlyDictionary<RuleSymbol, Node> Trivia { get; init; } =
		new Dictionary<RuleSymbol, Node>();

	/// <summary>
	/// What a left-recursive rule was rewritten into (§4.3), for the rules that were.
	/// </summary>
	public IReadOnlyDictionary<RuleSymbol, Fold> Folds { get; init; } =
		new Dictionary<RuleSymbol, Fold>();

	/// <summary>The repetitions marked <c>recover</c>, by the repetition (§8.2).</summary>
	public IReadOnlyDictionary<Node, Recovery> Recoveries { get; init; } =
		new Dictionary<Node, Recovery>();

	/// <summary>
	/// The rule synthesized for a value-returning external recognizer (§7.1's third
	/// row), by the C# method name it wraps.
	/// </summary>
	/// <remarks>
	/// One rule per distinct method name, its body a <see cref="Node.External"/> with
	/// <see cref="Node.External.HasValue"/> set and its declared type in <see
	/// cref="Types"/> already — everything downstream (<c>BuildsValue</c>,
	/// <c>ValueRule</c>, materialization) sees an ordinary typed rule and needs no case
	/// of its own. This map is what tells the materializer to recover the value by
	/// re-invoking the method rather than by walking a <c>Construct</c> factory.
	/// </remarks>
	public IReadOnlyDictionary<RuleSymbol, string> Externals { get; init; } =
		new Dictionary<RuleSymbol, string>();

	/// <summary>
	/// The rules written with binding powers (§4.3.1), and the strength each alternative
	/// of such a rule may be entered at.
	/// </summary>
	/// <remarks>
	/// Being in here is the one thing that decides whether a rule's recognizer takes a
	/// strength to parse at. A rule of levels does not, which is every rule that does not
	/// say <c>&lt;&lt;</c> or <c>&gt;&gt;</c> — so a grammar that never reaches for
	/// binding powers is generated exactly as it was before they existed.
	/// </remarks>
	public IReadOnlyDictionary<RuleSymbol, IReadOnlyDictionary<Node, int>> Climbing { get; init; } =
		new Dictionary<RuleSymbol, IReadOnlyDictionary<Node, int>>();

	/// <summary>
	/// The strength each call to a climbing rule's own self parses its operand at.
	/// </summary>
	/// <remarks>
	/// The whole of the difference between the two markers: <c>&lt;&lt; n</c> records
	/// <c>n + 1</c>, so the same operator cannot appear in its own right operand and the
	/// grouping goes left; <c>&gt;&gt; n</c> records <c>n</c>, so it can, and the grouping
	/// goes right.
	/// </remarks>
	public IReadOnlyDictionary<Node, int> Powers { get; init; } = new Dictionary<Node, int>();

	public IReadOnlyList<RuleSymbol>             Rules       { get; } = rules;
	public IReadOnlyDictionary<RuleSymbol, Node> Bodies      { get; } = bodies;
	public IReadOnlyDictionary<RuleSymbol, bool> Nullable    { get; } = nullable;
	public IReadOnlyList<GramDiagnostic>         Diagnostics { get; } = diagnostics;

	/// <summary>
	/// What each rule's value is made of: one member per capture name, in the order the
	/// notation writes them. Empty for a rule that captures nothing — its value is the
	/// text it matched (§4.1 case 4).
	/// </summary>
	public IReadOnlyDictionary<RuleSymbol, IReadOnlyList<ResultMember>> Results { get; } = results;

	/// <summary>The public API this grammar asked for — carried through unchanged (§6).</summary>
	public IReadOnlyList<Publication> Publications { get; } = publications;

	public bool HasErrors => Diagnostics.Count > 0;

	/// <summary>
	/// A rule as the parts it is read in, which is what a streamed parse runs one at a
	/// time and what the analysis of §6.3 measures one at a time.
	/// </summary>
	/// <remarks>
	/// <para>
	/// One line of it matters: a rule that builds a sequence (§4.1 case 2) has a <c>=&gt;</c>
	/// wrapped round its whole body, and a body that is not unwrapped is one part — itself.
	/// The analysis then measures a whole feed against one window and says it cannot be
	/// streamed, while the emitter, which did unwrap, streams it. Both read this now, so
	/// the two cannot answer differently.
	/// </para>
	/// <para>
	/// What each side makes of the parts is its own: the analysis asks how much input each
	/// may hold, the emitter asks which are rules it can hand back one at a time.
	/// </para>
	/// </remarks>
	public IReadOnlyList<Node> PartsOf(RuleSymbol rule)
	{
		var body = Bodies[rule];

		if (body is Node.Construct(var built, _))
			body = built;

		return body is Node.Sequence(var sequence) ? sequence : [body];
	}

	/// <summary>
	/// The rules that can reach themselves through calls, directly or round a cycle.
	/// </summary>
	/// <remarks>
	/// <para>
	/// What it decides is how a call is executed. A rule outside every cycle can be
	/// compiled into its caller — the expansion terminates, because the call graph beneath
	/// it is a DAG — and a call that becomes ordinary control flow costs no frame, no arena
	/// entry and no dispatch. A rule inside one has to keep the frame, since the only bound
	/// on its depth is the input.
	/// </para>
	/// <para>
	/// Computed once and cached: it is a property of the graph, and the emitter asks about
	/// it at every call site.
	/// </para>
	/// </remarks>
	public IReadOnlyCollection<RuleSymbol> Recursive => field ??= FindRecursive();

	HashSet<RuleSymbol> FindRecursive()
	{
		var calls = new Dictionary<RuleSymbol, List<RuleSymbol>>();

		foreach (var rule in Rules)
		{
			var called = new List<RuleSymbol>();

			if (Bodies.TryGetValue(rule, out var body))
				foreach (var node in NodeWalk.Descendants(body))
					if (node is Node.Call(var target, _) && !called.Contains(target))
						called.Add(target);

			calls[rule] = called;
		}

		var recursive = new HashSet<RuleSymbol>();

		// Reachability from each rule to itself. Quadratic in the number of rules and run
		// once per grammar, which is nothing beside what it saves at every call site.
		foreach (var rule in Rules)
		{
			var seen    = new HashSet<RuleSymbol>();
			var pending = new Stack<RuleSymbol>();

			foreach (var target in calls[rule])
				pending.Push(target);

			while (pending.Count > 0)
			{
				var at = pending.Pop();

				if (ReferenceEquals(at, rule))
				{
					recursive.Add(rule);

					break;
				}

				if (!seen.Add(at) || !calls.TryGetValue(at, out var next))
					continue;

				foreach (var target in next)
					pending.Push(target);
			}
		}

		return recursive;
	}

	/// <summary>
	/// Every node this graph says something about that is no longer in it, named. Empty
	/// for a graph that holds together, which is every graph this compiler should build.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Three of the maps here are keyed by a node's identity — a repetition recovers, a
	/// call parses at a strength, a construction may refuse — and normalization rewrites
	/// nodes. Rewrite one that is a key and the entry is still in the map, still keyed by
	/// the node that was replaced, and nothing reads it again: the <c>recover</c> quietly
	/// stops recovering, and the grammar still compiles.
	/// </para>
	/// <para>
	/// That has happened once already, when sequence results began rewriting repetitions.
	/// It is not a class of bug that can be found by reading, because the symptom is a
	/// missing effect rather than a wrong one — so it is stated here instead, and the
	/// tests hold every grammar they build to it.
	/// </para>
	/// </remarks>
	public IReadOnlyList<string> Orphans()
	{
		var live = NodeWalk.ByIdentity(NodeWalk.Descendants(Reachable()));
		var lost = new List<string>();

		foreach (var node in Recoveries.Keys)
			Check(node, "recover");

		foreach (var node in Powers.Keys)
			Check(node, "binding power");

		foreach (var levels in Climbing)
			foreach (var node in levels.Value.Keys)
				Check(node, $"strength in {levels.Key.Name}");

		return lost;

		void Check(Node node, string what)
		{
			if (!live.Contains(node))
				lost.Add($"{what}: {node}");
		}
	}

	/// <summary>
	/// The trees a parse actually runs: the rules, plus what left recursion left behind
	/// and the trivia the ends of a parse are given.
	/// </summary>
	IEnumerable<Node> Reachable()
	{
		foreach (var body in Bodies.Values)
			yield return body;

		foreach (var fold in Folds.Values)
			yield return fold.Loop;

		foreach (var trivia in Trivia.Values)
			yield return trivia;
	}

	/// <summary>
	/// One rule per line, rendered back into notation.
	/// </summary>
	/// <remarks>
	/// Rendering the normalized form as text is what makes a normalization regression
	/// legible: a test states the grammar as written and the grammar as normalized, and
	/// a diff between them says exactly which fold changed. Roc's macro tested itself
	/// this way and it is the one thing from that project that transfers unchanged.
	/// </remarks>
	public override string ToString()
	{
		var text = new StringBuilder();

		foreach (var rule in Rules)
			text.Append(rule.Name).Append(" = ").AppendEndingWith(Bodies[rule].ToString());

		foreach (var publication in Publications)
			text.Append("publish ").AppendEndingWith(publication.ToString());

		foreach (var diagnostic in Diagnostics)
			text.AppendEndingWith(diagnostic.ToString());

		return text.ToString().TrimEnd();
	}
}
