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

	static bool MeansAState(string kind)
	{
		try
		{
			return (bool)Layout
				.GetMethod("MeansAState", BindingFlags.NonPublic | BindingFlags.Static)!
				.Invoke(null, [kind])!;
		}
		catch (TargetInvocationException raised)
		{
			throw raised.InnerException!;
		}
	}

	/// <summary>Every kind of arena entry there is, read out of the emitted code itself.</summary>
	/// <remarks>
	/// From a checked-in snapshot rather than from a list written here, which is the whole
	/// point: a kind added to the engine appears in the snapshot on the next run, and this
	/// then asks whether anybody decided what its second field means.
	/// </remarks>
	public static TheoryData<string> Kinds()
	{
		var emitted = File.ReadAllText(
			Path.Combine(SolutionRoot(), "tests", "Snapshots", "Url.gram.g.cs"));

		var data = new TheoryData<string>();

		foreach (Match found in Regex.Matches(emitted, @"internal const int (\w+)\s*=\s*\d+;"))
			data.Add(found.Groups[1].Value);

		Assert.NotEmpty(data);

		return data;
	}

	/// <summary>
	/// Layout rewrites an entry's second field where it is a state, and must not where it
	/// is not.
	/// </summary>
	/// <remarks>
	/// This has already been a silent corruption once: a capture slot that happened to equal
	/// a collapsed state's number came back as that state's, the value it named was never
	/// built, and a construction was handed a null. It was guarded by a list of the eight
	/// kinds that are states, which said nothing about the other ten — so a kind added later
	/// was undecided by default, in whichever direction the next reader assumed. Two kinds
	/// were added this week.
	/// </remarks>
	[Theory]
	[MemberData(nameof(Kinds))]
	public void Every_kind_of_entry_says_what_its_second_field_is(string kind) =>
		MeansAState(kind);

	/// <summary>And a kind nobody decided about is an error, not a guess either way.</summary>
	/// <remarks>
	/// Which is what the theory above rests on: it asserts a decision exists by asking for
	/// one, so the asking has to be what fails. Verified by taking an entry out, which turns
	/// out to break generation itself — loudly, but as a downstream compile error in the
	/// consumer rather than as a sentence naming the kind. Hence both: the throw says which,
	/// and the theory says that every kind reaches it.
	/// </remarks>
	[Fact]
	public void A_kind_nobody_decided_about_is_refused_loudly() =>
		Assert.Contains(
			"No decision",
			Assert.Throws<InvalidOperationException>(() => MeansAState("SomethingNew")).Message);

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
