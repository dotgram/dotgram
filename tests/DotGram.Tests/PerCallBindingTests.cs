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
/// with a <c>parserLocating</c> parameter, and the lexer calls it at two sites without one (CS7036), so the parser
/// does not compile. Without <c>PerCall</c>, or without the binding, it does.
/// </remarks>
public sealed class PerCallBindingTests
{
	static string Source(bool perCall)
	{
		var options = perCall
			? "[GramOptions(LocationType = typeof(ILoc), Suffix = \"Located\", PerCall = true)]"
			: "[GramOptions(LocationType = typeof(ILoc), Suffix = \"Located\")]";

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
					}

					Value : @Text = t: Quoted => @(new Text(t))

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
	[InlineData(false)]
	[InlineData(true)]
	public void A_bound_terminal_is_read_through_a_located_door(bool perCall)
	{
		var assembly = GeneratorDriverTests.Build(Source(perCall));
		var host     = assembly.GetType("PerCallBindingProbe.P")!;
		var door     = host.GetNestedType("Located")!;
		var parse    = door.GetMethod("ParseValue", BindingFlags.Public | BindingFlags.Static, [typeof(string)])!;
		var value    = parse.Invoke(null, ["  $ab$ x $a$ y $ab$"])!;

		Assert.Equal("$ab$ x $a$ y $ab$", value.GetType().GetProperty("Value")!.GetValue(value));
		Assert.Equal(2, value.GetType().GetField("At")!.GetValue(value));
	}
}
