using System;
using System.Linq;
using System.Reflection;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// <c>[GramOptions(PerCall = true)]</c>: one parser whose located door is a nested class of the same
/// published methods, on every carrier, beside a reading of its own that locates nothing.
/// </summary>
/// <remarks>
/// The offers stand beside the reader, which the immediate carrier builds values in and which is
/// not generic; the walk that builds a tape's values is. Both have to compile and say the same, and
/// a further <c>[GramOptions]</c> reading must not inherit the per-call location type.
/// </remarks>
public sealed class PerCallLocationTests
{
	static string Source(string carrier)
	{
		return $$""""
			using DotGram;

			namespace PerCallProbe
			{
				public interface ILoc
				{
					void Locate(int at, int length);

					void Locate(int at, int length, int gapStart);
				}

				public sealed class Item : ILoc
				{
					public Item(string key)
					{
						Key = key;
					}

					public string Key { get; }

					public int At = -1;

					public int Length = -1;

					public int Gap = -1;

					public int Offers;

					public void Locate(int at, int length)
					{
						At     = at;
						Length = length;
						Gap    = -2;
						Offers++;
					}

					public void Locate(int at, int length, int gapStart)
					{
						At     = at;
						Length = length;
						Gap    = gapStart;
						Offers++;
					}
				}

				[Gram("""
					using Std;

					trivia = { Spacing* }

					List : @Item[] = Item* & eof

					Item : @Item = k: Identifier & ';' => @(new Item(k))

					parse List
					""", Lexical = true, Carrier = GramCarrier.{{carrier}})]
				[GramOptions(LocationType = typeof(ILoc), Suffix = "Located", PerCall = true)]
				[GramOptions(Suffix = "Other")]
				public static partial class P
				{
				}
			}
			"""";
	}

	[Theory]
	[InlineData("Tape")]
	[InlineData("Immediate")]
	public void A_per_call_reading_compiles_and_locates_on_every_carrier(string carrier)
	{
		var assembly = GeneratorDriverTests.Build(Source(carrier));
		var host     = assembly.GetType("PerCallProbe.P")!;
		var text     = "  ab ; cd;";

		// The tape builds its values in a walk generic over whether the reading locates; the
		// immediate carrier builds them as it reads, and has no walk to call.
		var generated = GeneratorDriverTests.GetGeneratedSource(GeneratorDriverTests.RunGenerator(Source(carrier)), "PerCallProbe.P.g.cs");

		Assert.Contains("Offer_DotGram", generated, StringComparison.Ordinal);
		Assert.Equal(carrier == "Tape", generated.Contains("Reading_DotGram<LocatingOn_DotGram>.Materialize", StringComparison.Ordinal));

		var plain   = Read(host, text);
		var located = Read(host.GetNestedType("Located")!, text);
		var other   = Read(host.GetNestedType("Other")!, text);

		Assert.Equal(["ab", "cd"], plain.Select(Key));
		Assert.Equal(["ab", "cd"], located.Select(Key));
		Assert.Equal(["ab", "cd"], other.Select(Key));

		// The plain door and the other reading say nothing about where.
		Assert.All(plain.Concat(other), static item => Assert.Equal(-1, Field(item, "At")));

		// The located door says where each item is and where the text in front of it began, once.
		Assert.Equal((2, 4, 0, 1), Location(located[0]));
		Assert.Equal((7, 3, 6, 1), Location(located[1]));
	}

	static string FlatSource(string carrier, bool lexical)
	{
		return $$""""
			using DotGram;

			namespace PerCallFlatProbe
			{
				public interface ILoc
				{
					void Locate(int at, int length);

					void Locate(int at, int length, int gapStart);
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

					public void Locate(int at, int length, int gapStart)
					{
						At     = at;
						Length = length;
					}
				}

				public sealed class Pair
				{
					public Pair(Name key, Name value)
					{
						Key   = key;
						Value = value;
					}

					public Name Key { get; }

					public Name Value { get; }
				}

				[Gram("""
					using Std;

					trivia = { Spacing* }

					Entry : @Pair = k: Key & '=' & v: Key & eof => @(new Pair(k, v))
					Key   : @Name = w: Identifier => @(new Name(w))

					parse Entry
					""", Lexical = {{(lexical ? "true" : "false")}}, Carrier = GramCarrier.{{carrier}})]
				[GramOptions(LocationType = typeof(ILoc), Suffix = "Located", PerCall = true)]
				public static partial class P
				{
				}
			}
			"""";
	}

	/// <summary>
	/// A grammar small enough to be compiled flat — no recursion, no backtracking, nothing held
	/// back — still says where its values were read through the per-call door: a flat rendering
	/// keeps no range to offer, so a located rule is not rendered flat, as it is not where every
	/// rule is located.
	/// </summary>
	[Theory]
	[InlineData("Tape", true)]
	[InlineData("Tape", false)]
	[InlineData("Immediate", true)]
	[InlineData("Immediate", false)]
	public void A_per_call_reading_of_a_flat_grammar_locates(string carrier, bool lexical)
	{
		var assembly = GeneratorDriverTests.Build(FlatSource(carrier, lexical), permittedWarning: "GRAM5007");
		var host     = assembly.GetType("PerCallFlatProbe.P")!;
		var text     = "ab = cde";

		var plain   = ReadEntry(host, text);
		var located = ReadEntry(host.GetNestedType("Located")!, text);

		Assert.Equal(-1, Field(Name(plain, "Key"), "At"));
		Assert.Equal(-1, Field(Name(plain, "Value"), "At"));

		Assert.Equal((0, 2), (Field(Name(located, "Key"), "At"), Field(Name(located, "Key"), "Length")));
		Assert.Equal((5, 3), (Field(Name(located, "Value"), "At"), Field(Name(located, "Value"), "Length")));
	}

	static object ReadEntry(Type door, string text)
	{
		var parse = door.GetMethod("ParseEntry", BindingFlags.Public | BindingFlags.Static, [typeof(string)])!;

		return parse.Invoke(null, [text])!;
	}

	static object Name(object pair, string property)
	{
		return pair.GetType().GetProperty(property)!.GetValue(pair)!;
	}

	static object[] Read(Type door, string text)
	{
		var parse = door.GetMethod("ParseList", BindingFlags.Public | BindingFlags.Static, [typeof(string)])!;

		return ((Array)parse.Invoke(null, [text])!).Cast<object>().ToArray();
	}

	static string Key(object item)
	{
		return (string)item.GetType().GetProperty("Key")!.GetValue(item)!;
	}

	static int Field(object item, string name)
	{
		return (int)item.GetType().GetField(name)!.GetValue(item)!;
	}

	static (int, int, int, int) Location(object item)
	{
		return (Field(item, "At"), Field(item, "Length"), Field(item, "Gap"), Field(item, "Offers"));
	}
}
