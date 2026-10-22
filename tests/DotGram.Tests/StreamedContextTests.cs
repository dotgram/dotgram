using System;
using System.IO;
using System.Linq;
using System.Reflection;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A parse read over a reader, whose rule has a repetition marked <c>recover</c>, in a grammar whose
/// constructions read its context: the recognizers the stages call are handed the context as the
/// ones of the string form are, and the readings agree. The context is only read: a construction
/// that changes it is run again for a result a window may still extend, and counts it twice.
/// </summary>
public sealed class StreamedContextTests
{
	static string Source(bool throwing)
	{
		return $$""""
			using DotGram;

			namespace StreamedContextProbe
			{
				public sealed class Counter
				{
					public int Factor = 10;
				}

				[Gram("""
					using Std;
					context : @Counter
					Row   : @int   = d: Digits & eol => @(d.Length * context.Factor)
					Rows  : @int[] = (Row)* recover eol & eof
					Word  : @int   = w: ['a'..'z']+ => @(w.Length * context.Factor)
					parse Rows as ReadRows
					find Word as FindWords
					""", Portable = true{{(throwing ? ", ConstructionsMayThrow = true" : "")}})]
				public static partial class P
				{
				}
			}
			"""";
	}

	static object Door(Type host, string name, Type input)
	{
		return host.GetMethods().Single(
			method => method.Name == name && method.GetParameters() is [var first, var second] &&
				first.ParameterType == input && second.ParameterType.Name == "Counter");
	}

	static object? Run(Assembly assembly, string name, Type input, object argument)
	{
		var host    = assembly.GetType("StreamedContextProbe.P")!;
		var counter = Activator.CreateInstance(assembly.GetType("StreamedContextProbe.Counter")!)!;

		counter.GetType().GetField("Factor")!.SetValue(counter, 7);
		var result  = ((MethodInfo)Door(host, name, input)).Invoke(null, [argument, counter]);

		if (result is System.Collections.IEnumerable sequence and not Array and not string)
			result = sequence.Cast<object>().Select(
				static one => one.GetType().GetProperty("Value") is { } value ? value.GetValue(one) : one).ToArray();

		return result;
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void A_reader_with_a_context_reads_what_the_string_reads(bool throwing)
	{
		var assembly = GeneratorDriverTests.Build(Source(throwing));
		var text     = "12\nab\n345\n6\n";

		var viaReader = Run(assembly, "ReadRows", typeof(TextReader), new StringReader(text));
		var viaString = Run(assembly, "ReadRows", typeof(string), text);
		var viaLines  = Run(assembly, "ReadRows", typeof(System.Collections.Generic.IEnumerable<string>), text.TrimEnd('\n').Split('\n'));

		Assert.Equal(viaString, viaReader);
		Assert.Equal(viaString, viaLines);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void A_find_over_a_reader_with_a_context_finds_what_the_string_finds(bool throwing)
	{
		var assembly = GeneratorDriverTests.Build(Source(throwing));
		var text     = "ab 12 cde";

		var viaReader = Run(assembly, "FindWords", typeof(TextReader), new StringReader(text));
		var viaString = Run(assembly, "FindWords", typeof(string), text);

		Assert.Equal(viaString, viaReader);
	}

	static string Counting(string change)
	{
		return $$""""
			using DotGram;

			namespace StreamedContextProbe
			{
				public sealed class Counter
				{
					public int Rows;
				}

				[Gram("""
					using Std;
					context : @Counter
					Row  : @int   = d: Digits & eol => @(d.Length + {{change}})
					Rows : @int[] = (Row)* recover eol & eof
					parse Rows as ReadRows
					""", Portable = true)]
				public static partial class P
				{
				}
			}
			"""";
	}

	/// <summary>
	/// A construction that changes the context is told it may run a different number of times in a
	/// streamed reading (GRAM5017); one that only reads it is not.
	/// </summary>
	[Theory]
	[InlineData("context.Rows++", true)]
	[InlineData("++context.Rows", true)]
	[InlineData("(context.Rows = 3)", true)]
	[InlineData("(context.Rows += 3)", true)]
	[InlineData("(context.Rows)++", true)]
	[InlineData("context.@Rows++", true)]
	[InlineData("context!.Rows++", true)]
	[InlineData("context?.Rows++", true)]
	[InlineData("(context.Rows /* once */)++", true)]
	[InlineData("context.Rows /* not context.Rows++ */", false)]
	[InlineData("context.Rows * 2", false)]
	[InlineData("(context.Rows == 3 ? 1 : 0)", false)]
	[InlineData("(context.Rows >= 3 ? 1 : 0)", false)]
	public void A_construction_that_changes_the_context_is_told_about_streamed_readings(string change, bool told)
	{
		var run = GeneratorDriverTests.RunGenerator(Counting(change));
		var info = run.Diagnostics.Where(static one => one.Id == "GRAM5017").ToArray();

		Assert.Equal(told, info.Length == 1);

		if (told)
			Assert.Equal(Microsoft.CodeAnalysis.DiagnosticSeverity.Info, info[0].Severity);
	}
}
