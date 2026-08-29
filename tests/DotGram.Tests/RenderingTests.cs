using System;
using System.Linq;
using System.Reflection;

using DotGram.Grammar.Binding;

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
}
