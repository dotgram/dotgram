using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using DotGram.Finance.Fix;

using Xunit;

namespace DotGram.Finance.Tests;

/// <summary>
/// The generated validator of each type, held against the walk that answers today.
/// </summary>
/// <remarks>
/// <para>
/// <c>FixMessageValidation.g.cs</c> is FIX 4.4 written out type by type: no schema, no table, no
/// lookup. Nothing yet calls it — <see cref="FixMessage.Validate()"/> is still the walk — and that
/// is deliberate, because while both exist the new one can be held against the old over every
/// fixture this package keeps. A transfer from a specification that has never been compared is a
/// hope; this is the comparison, and it is why the walk is removed in a later commit than the one
/// that generates its replacement.
/// </para>
/// <para>
/// The two must answer the same findings in the same order. Order is part of the answer: a reader
/// of a diagnostic reads the first line, and two implementations that report the same set in
/// different orders are two different answers to a consumer.
/// </para>
/// </remarks>
public sealed class FixGeneratedRuleTests
{
	/// <summary>
	/// Every fixture, validated twice: by the walk and by the generated rule of its own class.
	/// </summary>
	[Theory]
	[MemberData(nameof(FixFixtures.Messages), MemberType = typeof(FixFixtures))]
	public void The_generated_rule_answers_as_the_walk_does(string name, string wire)
	{
		var message = FixParser.ParseMessage(wire);

		var walked    = message.Validate();
		var generated = Generated(message);

		Assert.Equal(Lines(walked), Lines(generated));
		Assert.NotNull(name);
	}

	/// <summary>
	/// And every one of the ninety-three types has a generated rule to be held to.
	/// </summary>
	/// <remarks>
	/// A fixture exercises the types somebody wrote a fixture for. This asks the other question —
	/// whether the generator wrote a rule for every class at all — so that a type nobody fixtured
	/// cannot go missing quietly.
	/// </remarks>
	[Fact]
	public void Every_message_class_has_one()
	{
		var without = new List<string>();

		foreach (var type in typeof(FixMessage).GetNestedTypes(BindingFlags.Public)
			.Where(one => one.IsSubclassOf(typeof(FixMessage)) && one.Name != nameof(FixMessage.Custom)))
		{
			var rule      = type.GetMethod("ValidateDefault", BindingFlags.Public | BindingFlags.Static);
			var validator = type.GetField("Validator", BindingFlags.Public | BindingFlags.Static);

			if (rule is null || validator is null)
				without.Add(type.Name);
		}

		Assert.True(without.Count == 0, "no generated rule for: " + string.Join(", ", without));
		Assert.Equal(93, typeof(FixMessage).GetNestedTypes(BindingFlags.Public)
			.Count(one => one.IsSubclassOf(typeof(FixMessage)) && one.Name != nameof(FixMessage.Custom)));
	}

	/// <summary>The generated rule of this message's own class, called through its own signature.</summary>
	static FixFinding[] Generated(FixMessage message)
	{
		var rule = message.GetType().GetMethod("ValidateDefault", BindingFlags.Public | BindingFlags.Static);

		Assert.NotNull(rule);

		return (FixFinding[])rule!.Invoke(null, [message])!;
	}

	/// <summary>
	/// A finding as one line, so that a difference is read rather than counted.
	/// </summary>
	static string[] Lines(FixFinding[] found)
	{
		return found
			.Select(one => $"{one.Rule} {one.Scope}"
				+ (one.GroupTag == 0 ? "" : "/" + one.GroupTag)
				+ (one.EntryIndex < 0 ? "" : "[" + one.EntryIndex + "]")
				+ " tag " + one.Tag
				+ " at " + one.Position
				+ ": " + one.Reason)
			.ToArray();
	}
}
