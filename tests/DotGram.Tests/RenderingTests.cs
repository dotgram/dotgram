using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Every name the language supplies, against every way a parser is written out.
/// </summary>
/// <remarks>
/// <para>
/// The defect this exists for came four times in one week and was the same defect each
/// time: a name threaded through the rendering it was built in and silently absent from
/// the rest, so a valid grammar produced C# that does not compile. Every instance was
/// found by somebody compiling the wrong-shaped grammar by hand.
/// </para>
/// <para>
/// So the test is not "does `context` work" — that one existed and passed while three
/// renderings could not hand it over. It is: **is there a decision at all**, for every
/// name and every rendering. A name added to a factory's signature without a decision
/// fails here rather than in a consumer's build.
/// </para>
/// <para>
/// By reflection because the table is internal to the generator and this asks a question
/// about its completeness rather than about a grammar. That is the one thing worth
/// reaching in for: the table's value is entirely that nothing is missing from it.
/// </para>
/// </remarks>
public sealed class RenderingTests
{
	static readonly Type Renderings =
		typeof(GrammarBinder).Assembly.GetType("DotGram.Grammar.Emit.Renderings")!;

	static readonly Type Rendering = Renderings.GetNestedType("Rendering")!;

	static string? Reason(object rendering, string name)
	{
		try
		{
			return (string?)Renderings
				.GetMethod("Reason", BindingFlags.Public | BindingFlags.Static)!
				.Invoke(null, [rendering, name]);
		}
		catch (TargetInvocationException raised)
		{
			throw raised.InnerException!;
		}
	}

	public static TheoryData<string, string> Everything()
	{
		var data = new TheoryData<string, string>();

		foreach (var rendering in Enum.GetNames(Rendering))
			foreach (var name in SuppliedNames.All.Concat(["context"]))
				data.Add(rendering, name);

		return data;
	}

	/// <summary>
	/// Each rendering either hands a supplied name over or refuses it, and says which.
	/// </summary>
	/// <remarks>
	/// There is no third answer, and an undecided pair throws rather than defaulting to
	/// "no" — a default would make an oversight look like a decision, which is exactly how
	/// this went wrong before.
	/// </remarks>
	[Theory]
	[MemberData(nameof(Everything))]
	public void Every_supplied_name_is_answered_for_by_every_rendering(string rendering, string name)
	{
		var reason = Reason(Enum.Parse(Rendering, rendering), name);

		// Where it cannot, it says why in words an author of this compiler can act on.
		if (reason is not null)
			Assert.InRange(reason.Length, 20, 200);
	}

	/// <summary>The engine hands over everything, which is what makes it the measure.</summary>
	/// <remarks>
	/// A name the general rendering could not supply would be a name the language does not
	/// really have — every other rendering is a specialization proved against this one, so
	/// a gap here is a gap everywhere.
	/// </remarks>
	[Fact]
	public void The_engine_supplies_all_of_them() =>
		Assert.All(
			SuppliedNames.All.Concat(["context"]),
			name => Assert.Null(Reason(Enum.Parse(Rendering, "Engine"), name)));

	/// <summary>And a name nobody has decided about is an error, not a refusal.</summary>
	[Fact]
	public void A_name_nobody_decided_about_is_refused_loudly() =>
		Assert.Contains(
			"No decision",
			Assert.Throws<InvalidOperationException>(
				() => Reason(Enum.Parse(Rendering, "Flat"), "parserSomethingNew")).Message);

	// ── What an arena entry's second field means ────────────────────────────────

	static readonly Type Layout =
		typeof(GrammarBinder).Assembly.GetType("DotGram.Grammar.Emit.Machine")!;

	/// <summary>
	/// Every entry that resumes at a state says so, so that layout can move it.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A state's number is not final where it is written — layout collapses the ones that
	/// only point somewhere and merges the ones that do the same thing — so a site that
	/// writes the number instead of asking <c>Resuming</c> for a mark leaves a number
	/// nothing will move. What that costs is exact and quiet: the arena resumes at a state
	/// that was not written, the dispatch has no case for it, and a parse that should have
	/// carried on refuses input it ought to accept.
	/// </para>
	/// <para>
	/// The reverse hazard is gone by construction and was the reason the pair of tests this
	/// replaces existed: a capture slot could be rewritten as a state when a table of kinds
	/// said the wrong thing. Nothing rewrites a plain number now — only a mark, and only
	/// <c>Resuming</c> makes one — so a slot cannot be mistaken for a state whatever anyone
	/// forgets. What is left to guard is the forgetting itself, and it is guarded here,
	/// against the emitter's own source: a second field written as an interpolation must be
	/// a mark. The four sites that write a runtime expression there — <c>entry.State</c> and
	/// its like — are not interpolations and name no state the generator knows.
	/// </para>
	/// </remarks>
	[Fact]
	public void Every_entry_that_resumes_at_a_state_asks_for_a_mark()
	{
		var kinds = string.Join(
			"|",
			"Choice", "Call", "Lookahead", "Completed", "Dead", "Run", "PendingRecovery", "LoopExit");

		var written = new Regex(
			@"new ParserEntry\(ParserEntry\.(" + kinds + @"), \{([^}]*)\}");

		var bare = new List<string>();

		foreach (var file in Directory.GetFiles(
			Path.Combine(SolutionRoot(), "src", "DotGram", "Grammar", "Emit"), "Machine*.cs"))
		{
			foreach (Match found in written.Matches(File.ReadAllText(file)))
			{
				var second = found.Groups[2].Value;

				// The three fixed states are never collapsed and never merged — they are
				// labels rather than numbered states — so there is nothing for a mark to
				// move and writing one would leak it: settling runs over the state bodies,
				// and the root call is written into the file.
				if (second is "Return" or "Accept" or "Fail")
					continue;

				if (!second.StartsWith("Resuming(", StringComparison.Ordinal))
					bare.Add($"{Path.GetFileName(file)}: {found.Value}");
			}
		}

		Assert.True(
			bare.Count == 0,
			"An arena entry names a state without asking `Resuming` for a mark, so layout " +
			"cannot move it where the state is collapsed:\n" + string.Join("\n", bare));
	}

	static DotGram.Grammar.Parsing.GrammarFile Parsed(string grammar) =>
		DotGram.Grammar.Parsing.GramParser.Parse(
			DotGram.Grammar.Parsing.GramLexer.Tokenize(grammar)).File!;

	static string SolutionRoot()
	{
		var at = AppContext.BaseDirectory;

		while (at is not null && !File.Exists(Path.Combine(at, "DotGram.slnx")))
			at = Path.GetDirectoryName(at);

		return at ?? throw new InvalidOperationException("No solution root above the test binary.");
	}

	// ── What a rebuilt node has to carry with it ────────────────────────────────

	/// <summary>
	/// Every table keyed by which node is one a rebuild hands on.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Passes here record what they worked out against the node they worked it out on, by
	/// reference, and a pass that rebuilds a node has to hand those on or the fact is left
	/// naming a node no body holds. The fold went unhanded for a long time, and what it cost
	/// is in `GrammarNormalizer.Recursion`'s own remarks: C# the *consumer* could not
	/// compile, in a file they never wrote.
	/// </para>
	/// <para>
	/// So the question is not whether the three known tables are carried — they are, and
	/// were before this. It is whether a fourth could be added without being. Reflection
	/// finds every field of the shape and the registry says which are handled; a table
	/// declared and not registered fails here.
	/// </para>
	/// </remarks>
	[Fact]
	public void Every_table_keyed_by_a_node_is_carried_through_a_rebuild()
	{
		var normalizer = typeof(GrammarNormalizer);
		var node       = typeof(GrammarNormalizer).Assembly.GetType("DotGram.Grammar.Model.Node")!;

		var declared = normalizer
			.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
			.Where(field =>
				field.FieldType.IsGenericType &&
				field.FieldType.GetGenericTypeDefinition() == typeof(Dictionary<,>) &&
				field.FieldType.GetGenericArguments()[0] == node)
			.ToArray();

		Assert.NotEmpty(declared);

		// A real instance, built through the private constructor, because the field
		// initializers are what make the tables distinct objects. The first version of this
		// used an uninitialized one and was worthless: every field was null, the registry
		// was a list of nulls, and "is this null among those nulls" is true of anything.
		var instance = (GrammarNormalizer)Activator.CreateInstance(
			normalizer,
			BindingFlags.NonPublic | BindingFlags.Instance,
			binder: null,
			[GrammarBinder.Bind(Parsed("Start = 'a'")), null],
			culture: null)!;

		// `Item1`, not `Table`: a tuple's element names live in the compiler and not in the
		// metadata, and the list holds value tuples, which is why the cast below is the
		// non-generic one — an `IEnumerable<ValueTuple<…>>` is not an `IEnumerable<object>`.
		var registered = ((System.Collections.IEnumerable)normalizer
			.GetProperty("Annotations", BindingFlags.NonPublic | BindingFlags.Instance)!
			.GetValue(instance)!)
			.Cast<object>()
			.Select(entry => entry.GetType().GetField("Item1")!.GetValue(entry))
			.ToArray();

		Assert.All(
			declared,
			field => Assert.Contains(
				field.GetValue(instance),
				registered,
				ReferenceEqualityComparer.Instance));
	}
}
