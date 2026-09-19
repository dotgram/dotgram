using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The point past which each reading is settled (<see cref="Commit"/>): a table of shapes, each
/// with every site's answer written out — the call as <c>Owner.Called</c>, the construction as
/// <c>Owner.=&gt;</c> — and every one checked against <see cref="Replay"/>.
/// </summary>
/// <remarks>
/// An answer is <c>Kind(rule)</c>, the point's kind and the rule it is in, or <c>-</c> where
/// the site is settled only after the parse. Each shape holds a point beside a control that
/// is not one, so that a report calling everything settled, or nothing, fails here.
/// </remarks>
public sealed class CommitTests
{
	public static TheoryData<string, string, string[]> Shapes => new()
	{
		// FIX's shape: past a turn only `recover` takes back, the element is settled. Inside it,
		// what follows the tag can fail only the turn, which recovery then replaces and nothing
		// hands on: Replay keeps the tag, and its own end is its point.
		{
			"a recovering repetition",
			"""
			Tag    : @int      = v: { ['0'..'9']+ } => @(1)
			Field  : @string   = t: Tag & '=' & v: ['a'..'z']+ & ';' => @("f")
			Fields : @string[] = Field* recover ';' => @("!")
			parse Fields
			""",
			["Field.=>: Rule(Field)", "Field.Tag: Rule(Field)", "Fields.=>: Rule(Fields)", "Fields.Field: Turn(Fields)", "Tag.=>: Rule(Tag)"]
		},

		// A repetition NeverGivesBack proves, in a rule that is kept, against one that may give a
		// turn back to what follows it: there the rule's end is the first point.
		{
			"a repetition that never gives back, and one that may",
			"""
			Item  : @string = 'a' & n: ['0'..'9'] & ';' => @("i")
			Kept  : @string = items: Item* & eof => @("k")
			Given : @string = items: Item* & 'a' & eof => @("g")
			Start : @string = 'k' & k: Kept | 'g' & g: Given
			parse Start
			""",
			["Given.=>: Rule(Given)", "Given.Item: Rule(Given)", "Item.=>: Rule(Item)",
				"Kept.=>: Rule(Kept)", "Kept.Item: Turn(Kept)", "Start.Given: Rule(Start)", "Start.Kept: Rule(Start)"]
		},

		// An atomic group is a point where what follows it can only fail the parse; in a rule
		// that is not kept it is not one, whatever follows it (the sequence gives it back whole),
		// and the site is settled wherever that rule's call is.
		{
			"an atomic group, and one in a rule that is not kept",
			"""
			N     : @string = t: 'n' => @("n")
			Held  : @string = 'h' & { n: N } & 'x' => @(n)
			Given : @string = { n: N } & 'x' => @(n)
			Start : @string = h: Held & eof => @(h) | g: Given & 'y' => @(g) | 'n' & 'z' => @("z")
			parse Start
			""",
			["Given.=>: Caller(Given)", "Given.N: Caller(Given)", "Held.=>: Rule(Held)", "Held.N: Atomic(Held)",
				"N.=>: Caller(N)", "Start.=>: Rule(Start)", "Start.=>: Rule(Start)", "Start.=>: Rule(Start)",
				"Start.Given: Rule(Start)", "Start.Held: Rule(Start)"]
		},

		// Neither an alternative a later one can replace nor a lookahead is a point: what an
		// alternative read is settled at the end of what holds the choice, and what a lookahead
		// read is thrown away, so is never settled, and nor is the rule only it calls.
		{
			"a choice, and a lookahead",
			"""
			N     : @string = t: 'n' => @("n")
			L     : @string = t: 'l' => @("l")
			Start : @string = (?= l: L) & 'l' & n: N & 'x' => @(n) | 'l' & 'n' & 'y' => @("y")
			parse Start
			""",
			["L.=>: -", "N.=>: Caller(N)", "Start.=>: Rule(Start)", "Start.=>: Rule(Start)", "Start.L: -", "Start.N: Rule(Start)"]
		},

		// Nesting: a turn inside a rule that is not kept is not a point, and what it reads is
		// settled where the rule's own calls are — through a rule that is itself under another.
		{
			"a turn under a rule that is not kept",
			"""
			Item  : @string = 'a' & t: ['0'..'9'] => @("i")
			Run   : @string = items: Item* & ';' => @("r")
			Start : @string = r: Run & 'x' => @(r) | Run & 'y' => @("y")
			parse Start
			""",
			["Item.=>: Caller(Item)", "Run.=>: Caller(Run)", "Run.Item: Caller(Run)", "Start.=>: Rule(Start)",
				"Start.=>: Rule(Start)", "Start.Run: Rule(Start)", "Start.Run: Rule(Start)"]
		},

		// A rule called from a place with no point and from one with a point is settled only
		// after the parse: its call inside the lookahead has nothing to build at.
		{
			"a rule one of whose calls is not settled",
			"""
			N     : @string = t: 'n' => @("n")
			Start : @string = (?= N) & n: N & 'x' => @(n) | 'm' => @("m")
			parse Start
			""",
			["N.=>: -", "Start.=>: Rule(Start)", "Start.=>: Rule(Start)", "Start.N: -", "Start.N: Rule(Start)"]
		},

		// A recursion settled at the point of the call that entered it: `A` is not kept (the
		// alternative `'x' & 'z'` can replace it), and its inner call, which `'y'` after it can
		// make A give back, is settled where the outer call is. What A gave back is not on the
		// derivation that reaches that point, and nothing built there comes from it.
		{
			"a recursion entered from a point",
			"""
			A     : @string = 'x' & a: A & 'y' => @(a) | 'x' => @("x")
			Start : @string = a: A & ';' => @(a) | 'x' & 'z' => @("z")
			parse Start
			""",
			["A.=>: Caller(A)", "A.=>: Caller(A)", "A.A: Caller(A)", "Start.=>: Rule(Start)", "Start.=>: Rule(Start)", "Start.A: Rule(Start)"]
		},

		// The same recursion entered only from a lookahead has no point anywhere: its inner call
		// holds itself up, and the call that entered it is thrown away.
		{
			"a recursion entered only from a lookahead",
			"""
			A     : @string = 'x' & a: A & 'y' => @(a) | 'x' => @("x")
			Start : @string = (?= A) & 'x' & n: ['a'..'z']* => @("s")
			parse Start
			""",
			["A.=>: -", "A.=>: -", "A.A: -", "Start.=>: Rule(Start)", "Start.A: -"]
		},
	};

	[Theory]
	[MemberData(nameof(Shapes))]
	public void Each_site_is_settled_at_its_innermost_point(string name, string grammar, string[] expected)
	{
		var graph  = Graph(grammar);
		var replay = Replay.Of(graph);
		var report = Commit.Of(graph, replay);

		Assert.True(expected.SequenceEqual(Described(report)), $"{name}:\n  " + string.Join("\n  ", Described(report)));

		Consistent(graph, replay, report);
	}

	/// <summary>
	/// A guard over a value built on the spot: Demand says <see cref="Demand.Kind.Always"/> for
	/// the call the guard is handed, and it is built where it is read, as now; the point is
	/// where what waits for it is built, and the guard's value was built before it.
	/// </summary>
	[Fact]
	public void A_guard_is_handed_a_value_built_before_the_point()
	{
		var graph = Graph(
			"""
			Tag    : @int      = v: ['0'..'9']+ => @(1)
			Field  : @string   = t: Tag & '=' & switch @(t) { case 1: 'a' default: 'b' } & ';' => @("f")
			Fields : @string[] = Field* recover ';' => @("!")
			parse Fields
			""");
		var report = Commit.Of(graph, Replay.Of(graph));
		var tag    = Calls(graph, "Field").Single(call => call.Rule.Name == "Tag");
		var field  = Calls(graph, "Fields").Single(call => call.Rule.Name == "Field");

		Assert.Equal(Demand.Kind.Always, Demand.Of(graph).Of(tag));
		Assert.True(report.TryGet(tag, out var point));
		Assert.Equal(Commit.Kind.Rule, point.Kind);
		Assert.True(report.TryGet(field, out var turn));
		Assert.Equal(Commit.Kind.Turn, turn.Kind);
	}

	/// <summary>
	/// The scenario's own test beside the table: a recovering repetition is taken only where its
	/// continuation is known whole — followed by nothing, or by parts ending in <c>eof</c>, in a
	/// rule no rule calls. The continuation begins as an element does, so that NeverGivesBack does
	/// not make the turn a point on its own account.
	/// </summary>
	[Theory]
	[InlineData("Fields : @string[] = Field* recover ';' => @(\"!\")\nparse Fields", true)]
	[InlineData("Fields : @string = f: Field* recover ';' => @(\"!\") & 'a' & eof => @(\"t\")\nparse Fields", true)]
	[InlineData("Fields : @string = f: Field* recover ';' => @(\"!\") & 'a' => @(\"t\")\nparse Fields", false)]
	[InlineData("Fields : @string[] = Field* recover ';' => @(\"!\")\nTwice : @string = Fields & Fields => @(\"t\")\nparse Twice", false)]
	public void A_recovering_turn_is_a_point_only_with_its_continuation_known(string rest, bool taken)
	{
		var graph  = Graph("Field : @string = t: ['a'..'z']+ & ';' => @(\"f\")\n" + rest);
		var report = Commit.Of(graph, Replay.Of(graph));
		var field  = Calls(graph, "Fields").Single(call => call.Rule.Name == "Field");

		Assert.Equal(taken, report.TryGet(field, out var point) && point.Kind == Commit.Kind.Turn);
	}

	/// <summary>
	/// What the answer has to agree with: a rule's end is a point only in a rule Replay keeps, and
	/// in such a rule every site not under a lookahead or an argument has a point in that rule; a
	/// site settled at its callers is in a rule whose end is no point.
	/// </summary>
	static void Consistent(RecognitionGraph graph, Replay.Report replay, Commit.Report report)
	{
		var standing = new HashSet<RuleSymbol>();

		foreach (var site in report.Sites)
			if (report.TryGet(site.Node, out var point) && point.Kind == Commit.Kind.Rule)
			{
				Assert.True(replay.Keeps(site.Owner), $"{site.Owner.Name}: its end is a point, and Replay does not keep it");
				standing.Add(site.Owner);
			}

		foreach (var site in report.Sites)
		{
			var has = report.TryGet(site.Node, out var point);

			if (has && point.Kind == Commit.Kind.Caller)
				Assert.DoesNotContain(site.Owner, standing);

			if (standing.Contains(site.Owner) && !Hidden(graph.Bodies[site.Owner], site.Node))
				Assert.True(has && point.Rule == site.Owner, $"{site.Owner.Name}: its end is a point, and a site in it has none in it");
		}
	}

	/// <summary>Whether the node is under a lookahead or in a call's arguments.</summary>
	static bool Hidden(Node body, Node node)
	{
		foreach (var one in Descendants(body))
		{
			if (one is Node.Lookahead(_, var looked) && Descendants(looked).Any(inner => ReferenceEquals(inner, node)))
				return true;

			if (one is Node.Call(_, var arguments) && arguments.Any(argument => Descendants(argument).Any(inner => ReferenceEquals(inner, node))))
				return true;
		}

		return false;
	}

	static IEnumerable<string> Described(Commit.Report report)
	{
		return report.Sites
			.Select(site => $"{site.Owner.Name}.{(site.Node is Node.Call call ? call.Rule.Name : "=>")}: " +
				(report.TryGet(site.Node, out var point) ? $"{point.Kind}({point.Rule.Name})" : "-"))
			.Where(line => !line.Contains(".eof:"))
			.OrderBy(line => line, System.StringComparer.Ordinal);
	}

	static IEnumerable<Node> Descendants(Node node)
	{
		yield return node;

		foreach (var child in node.Children)
			foreach (var one in Descendants(child))
				yield return one;
	}

	static IEnumerable<Node.Call> Calls(RecognitionGraph graph, string rule)
	{
		return Descendants(graph.Bodies.Single(pair => pair.Key.Name == rule).Value).OfType<Node.Call>();
	}

	static RecognitionGraph Graph(string grammar)
	{
		return GrammarNormalizer.Normalize(
			GrammarBinder.Bind(
				GramParser.Parse(
					GramLexer.Tokenize(grammar, DotGram.Generation.RoslynCSharpScanner.Instance)).File!));
	}
}
