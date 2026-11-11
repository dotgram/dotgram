using System;
using System.Reflection;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A per-call located door beside a terminal the lexer reads by a binding: the grammar compiles once and its
/// <c>Located</c> class asks for locations per call, as <c>TransactSqlParser</c> does, and a terminal that binds its
/// opening and matches it again at its close — PostgreSQL's dollar quote — is read whole by its own rule.
/// </summary>
/// <remarks>
/// Seen in the PostgreSQL grammar: with <c>PerCall = true</c> the measure method of the bound terminal is emitted
/// with a <c>parserLocating</c> parameter, and the lexer called it without one (CS7036), so the parser did not
/// compile. Without <c>PerCall</c>, or without the binding, it did. Read both where no other token begins as the
/// terminal does, and where one does — PostgreSQL's positional parameter, <c>$1</c>, here a bare <c>$</c> as
/// well — so that a refusal of the terminal's rule is read again as the other token: the lexer measures the
/// terminal on each of those paths.
/// </remarks>
public sealed class PerCallBindingTests
{
	static string Source(bool perCall, bool shared)
	{
		var options = perCall
			? "[GramVariant(LocationType = typeof(ILoc), Name = \"Located\", UsePerCallLocations = true)]"
			: "[GramVariant(LocationType = typeof(ILoc), Name = \"Located\")]";
		var param   = shared ? "Param  = '$' & ['0'..'9']*" : "";
		var value   = shared
			? "Value : @Text = t: Quoted => @(new Text(t)) | t: Param => @(new Text(t))"
			: "Value : @Text = t: Quoted => @(new Text(t))";

		return $$""""
			using DotGram;

			namespace PerCallBindingProbe
			{
				public interface ILoc
				{
					void Locate(int at, int length);

					void Locate(int at, int length, int gapStart);
				}

				public sealed class Text : ILoc
				{
					public Text(string value)
					{
						Value = value;
					}

					public string Value { get; }

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

				[Gram("""
					using Lexical;

					trivia = { Lexical.Space* }

					namespace Lexical
					{
						trivia = none

						Space  = ' '
						Quoted = { '$' & $tag: (['a'..'z']*) & '$' & (?!('$' & $tag & '$') & any)* & '$' & $tag & '$' }
						{{param}}
					}

					{{value}}

					parse Value
					""", Lexical = true)]
				{{options}}
				public static partial class P
				{
				}
			}
			"""";
	}

	[Theory]
	[InlineData(false, false)]
	[InlineData(true,  false)]
	[InlineData(false, true)]
	[InlineData(true,  true)]
	public void A_bound_terminal_is_read_through_a_located_door(bool perCall, bool shared)
	{
		var assembly = GeneratorDriverTests.Build(Source(perCall, shared));
		var host     = assembly.GetType("PerCallBindingProbe.P")!;
		var door     = host.GetNestedType("Located")!;

		AssertRead(door, "  $ab$ x $a$ y $ab$", "$ab$ x $a$ y $ab$", 2);

		// The plain door is told nothing, with the located one beside it or compiled apart.
		AssertRead(host, "  $ab$ x $a$ y $ab$", "$ab$ x $a$ y $ab$", -1);

		if (shared)
		{
			// The terminal's rule refuses at the token's start, and the position is read again as the other token.
			AssertRead(door, "  $1", "$1", 2);
			AssertRead(host, "  $1", "$1", -1);
			AssertRead(door, "  $", "$", 2);
			AssertRead(host, "  $", "$", -1);
		}
	}

	static void AssertRead(Type door, string input, string text, int at)
	{
		var parse = door.GetMethod("ParseValue", BindingFlags.Public | BindingFlags.Static, [typeof(string)])!;
		var value = parse.Invoke(null, [input])!;

		Assert.Equal(text, value.GetType().GetProperty("Value")!.GetValue(value));
		Assert.Equal(at, value.GetType().GetField("At")!.GetValue(value));
	}
}
