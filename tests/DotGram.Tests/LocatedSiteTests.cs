using System;
using System.Linq;
using System.Reflection;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A located value made by a rule whose one construction reads only its own captures: where every rule
/// is told where it was read (<c>LocationType</c> on the attribute), the construction is handed its
/// range, so its call keeps the rule's boundary rather than standing in place of it.
/// </summary>
/// <remarks>
/// A call compiled in place has no range of its own to hand on. Such a callee was compiled in place all
/// the same, and the call written for it missed the range, which failed the consumer's build with
/// CS7036. Seen in a located reading of the SQL:2023 grammar, whose character set names end in a
/// language identifier a rule of its own makes.
/// </remarks>
public sealed class LocatedSiteTests
{
	static string Source(string carrier, bool lexical)
	{
		return $$""""
			using DotGram;

			namespace LocatedSiteProbe
			{
				public interface ILoc
				{
					void Locate(int at, int length);
				}

				public sealed class Name : ILoc
				{
					public Name(string text)
					{
						Text = text;
					}

					public string Text { get; }

					public int At = -1;

					public int Length = -1;

					public void Locate(int at, int length)
					{
						At     = at;
						Length = length;
					}
				}

				public sealed class Pair : ILoc
				{
					public Pair(Name key, Name value)
					{
						Key   = key;
						Value = value;
					}

					public Name Key { get; }

					public Name Value { get; }

					public void Locate(int at, int length)
					{
					}
				}

				[Gram("""
					using Std;

					trivia = { Spacing* }

					Entry : @Pair = k: Key & '=' & v: Key & eof => @(new Pair(k, v))
					Key   : @Name = w: Word => @(new Name(w))
					Word  = ['a'..'z']+

					parse Entry
					""", Lexical = {{(lexical ? "true" : "false")}}, Carrier = GramCarrier.{{carrier}}, LocationType = typeof(ILoc))]
				public static partial class P
				{
				}
			}
			"""";
	}

	[Theory]
	[InlineData("Tape", true)]
	[InlineData("Tape", false)]
	[InlineData("Immediate", true)]
	[InlineData("Immediate", false)]
	public void A_located_value_of_a_flat_rule_is_told_where_it_was_read(string carrier, bool lexical)
	{
		var assembly = GeneratorDriverTests.Build(Source(carrier, lexical));
		var host     = assembly.GetType("LocatedSiteProbe.P")!;
		var parse    = host.GetMethod("ParseEntry", BindingFlags.Public | BindingFlags.Static, [typeof(string)])!;
		var pair     = parse.Invoke(null, ["ab = cde"])!;

		Assert.Equal((0, 2), Location(Property(pair, "Key")));
		Assert.Equal((5, 3), Location(Property(pair, "Value")));
	}

	static object Property(object owner, string name)
	{
		return owner.GetType().GetProperty(name)!.GetValue(owner)!;
	}

	static (int, int) Location(object name)
	{
		return ((int)name.GetType().GetField("At")!.GetValue(name)!, (int)name.GetType().GetField("Length")!.GetValue(name)!);
	}
}
