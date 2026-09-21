using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using DotGram.Finance.Fix;

using Xunit;

namespace DotGram.Finance.Tests;

/// <summary>
/// The FIX 4.4 fixture grammar, held against the field types it builds — without building it.
/// </summary>
/// <remarks>
/// <para>
/// <c>tests/DotGram.Finance.Fix44</c> is the oracle this package is measured against, and it is a
/// project of its own so that the ordinary tests build in seconds (D12). That arrangement has a
/// cost, and it was paid on 2026-09-21: changing the type of two fields left the fixture's grammar
/// calling a converter whose result no longer fit, the package's own suite stayed green because it
/// does not build the fixture, and the solution stopped compiling for everyone else.
/// </para>
/// <para>
/// So this reads the grammar as text, which costs milliseconds, and asks of every rule in it the
/// one question the compiler would have asked: does what <c>FixConvert</c> returns fit what the
/// <c>FixField</c> class takes. Both sides are read by reflection rather than from a table here,
/// so there is nothing in this test to keep up to date — it compares the two things that must
/// agree, through the same types the fixture would be compiled against.
/// </para>
/// </remarks>
public sealed class FixFixtureGrammarTests
{
	static readonly string Grammar = Path.Combine(
		AppContext.BaseDirectory, "..", "..", "..", "..", "DotGram.Finance.Fix44", "FixField.gram");

	[Fact]
	public void Every_field_the_fixture_builds_takes_what_its_converter_returns()
	{
		Assert.True(File.Exists(Grammar), $"the fixture grammar is not where this test looks: {Grammar}");

		var wrong = new List<string>();
		var seen  = 0;

		foreach (Match rule in Regex.Matches(
			File.ReadAllText(Grammar),
			@"new FixField\.(?<field>\w+)\(FixConvert\.(?<converter>\w+)\(value\)\)"))
		{
			var name      = rule.Groups["field"].Value;
			var converter = rule.Groups["converter"].Value;
			var field     = typeof(FixField).GetNestedType(name, BindingFlags.Public);

			seen++;

			if (field is null)
			{
				wrong.Add($"{name}: the package has no such field class, and the fixture builds one.");

				continue;
			}

			var takes = field.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
				.Select(one => one.GetParameters())
				.Where(one => one.Length == 1)
				.Select(one => one[0].ParameterType)
				.ToArray();

			var gives = typeof(FixConvert)
				.GetMethods(BindingFlags.Public | BindingFlags.Static)
				.Where(one => one.Name == converter)
				.Select(one => one.ReturnType)
				.Distinct()
				.ToArray();

			if (gives.Length == 0)
			{
				wrong.Add($"{name}: FixConvert has no {converter}.");

				continue;
			}

			if (!takes.Any(one => gives.Contains(one)))
				wrong.Add($"{name} takes {string.Join(" or ", takes.Select(Spell))}, " +
					$"and FixConvert.{converter} gives {string.Join(" or ", gives.Select(Spell))}.");
		}

		Assert.True(seen > 800, $"only {seen} rules were read from the fixture grammar; it has not been read properly.");
		Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
	}

	static string Spell(Type type)
	{
		return type.IsGenericType
			? type.Name[..type.Name.IndexOf('`')] + "<" + string.Join(", ", type.GetGenericArguments().Select(Spell)) + ">"
			: type.Name;
	}
}
