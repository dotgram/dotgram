using System;
using System.Linq;
using System.Reflection;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A located rule that hands back null for what was not written: the reading tells every value it
/// made where it was read, and null is no value made, so nothing is told — the parse returns the
/// null rather than throwing <see cref="NullReferenceException"/> from inside the construction.
/// </summary>
/// <remarks>
/// Seen in a located reading of the SQL:2023 grammar, whose correlation name is <c>null!</c> where a
/// table is written without one: every <c>FROM t</c> threw.
/// </remarks>
public sealed class LocatedNullTests
{
	static string Source(string carrier, bool lexical, bool perCall)
	{
		var options = perCall
			? "[GramOptions(LocationType = typeof(ILoc), Suffix = \"Located\", PerCall = true)]"
			: "";
		var located = perCall ? "" : ", LocationType = typeof(ILoc)";

		return $$""""
			using DotGram;

			namespace LocatedNullProbe
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

					public void Locate(int at, int length)
					{
						At = at;
					}

					public void Locate(int at, int length, int gapStart)
					{
						At = at;
					}
				}

				public sealed class Table : ILoc
				{
					public Table(Name name, Name alias)
					{
						Name  = name;
						Alias = alias;
					}

					public Name Name { get; }

					public Name Alias { get; }

					public void Locate(int at, int length)
					{
					}

					public void Locate(int at, int length, int gapStart)
					{
					}
				}

				[Gram("""
					using Std;

					trivia = { Spacing* }

					List  : @Table[] = Entry* & eof
					Entry : @Table   = n: Key & a: Alias & ';' => @(new Table(n, a))
					Key   : @Name    = w: Identifier => @(new Name(w))
					Alias : @Name    = "AS" & w: Identifier => @(new Name(w))
					                 | none => @(null!)

					parse List
					""", Lexical = {{(lexical ? "true" : "false")}}, Carrier = GramCarrier.{{carrier}}{{located}})]
				{{options}}
				public static partial class P
				{
				}
			}
			"""";
	}

	[Theory]
	[InlineData("Tape", true, false)]
	[InlineData("Tape", false, false)]
	[InlineData("Immediate", true, false)]
	[InlineData("Immediate", false, false)]
	[InlineData("Tape", true, true)]
	[InlineData("Immediate", true, true)]
	public void A_located_rule_that_hands_back_null_is_not_told_where(string carrier, bool lexical, bool perCall)
	{
		var assembly = GeneratorDriverTests.Build(Source(carrier, lexical, perCall));
		var host     = assembly.GetType("LocatedNullProbe.P")!;
		var door     = perCall ? host.GetNestedType("Located")! : host;
		var parse    = door.GetMethod("ParseList", BindingFlags.Public | BindingFlags.Static, [typeof(string)])!;

		// Written without one, the alias is null and the name is still told where it was read.
		var bare = Entries(parse, "t;");

		Assert.Null(Property(bare[0], "Alias"));
		Assert.Equal(0, Field(Property(bare[0], "Name")!, "At"));

		// The alias rule's range begins at its key word.
		var aliased = Entries(parse, "t AS u; v;");

		Assert.Equal(2, Field(Property(aliased[0], "Alias")!, "At"));
		Assert.Null(Property(aliased[1], "Alias"));
		Assert.Equal(8, Field(Property(aliased[1], "Name")!, "At"));
	}

	static object[] Entries(MethodInfo parse, string text)
	{
		return [.. ((Array)parse.Invoke(null, [text])!).Cast<object>()];
	}

	static object? Property(object owner, string name)
	{
		return owner.GetType().GetProperty(name)!.GetValue(owner);
	}

	static int Field(object owner, string name)
	{
		return (int)owner.GetType().GetField(name)!.GetValue(owner)!;
	}
}
