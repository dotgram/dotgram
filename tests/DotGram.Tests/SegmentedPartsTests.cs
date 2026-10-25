using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A grammar that names <c>eos</c> and is written in several files (a small <c>SourceFileSize</c>) reads the same as
/// the one written whole: a scanner is written where some state calls it, and the state may be in a file of its own.
/// </summary>
public sealed class SegmentedPartsTests
{
	/// <summary>The scanner of <c>Word</c> is called from the engine, which is a file of its own.</summary>
	const string Rows = """
		Rows : @int[] = Row* recover eos
		Row  : @int   = Word & eos => @(1)
		Word = { 'a'+ }
		parse Rows
		""";

	const string Sync = """
		Rows : @int[] = Row* recover Rest => @(0)
		Row  : @int   = Word & eos => @(1)
		Word = { 'a'+ }
		Rest = any* & eos
		parse Rows
		""";

	const string Look = """
		Rows : @int[] = Row* recover eos => @(0)
		Row  : @int   = ?!"x" & Word & ?=eos & eos => @(1)
		Word = { 'a'+ }
		parse Rows
		""";

	const string Trivia = """
		trivia = { ' '* }
		Rows : @int[] = Row* recover eos
		Row  : @int   = Word & Word? & eos => @(1)
		Word = { 'a'+ }
		parse Rows
		""";

	const string Finds = """
		Row  : @int = Word & eos => @(1)
		Word = { 'a'+ }
		find Row as Found
		parse Row
		""";

	const string Several = """
		Rows : @int[] = Row* recover eos => @(0)
		Row  : @int   = Key & ':' & Word & eos => @(1)
		Key  = { ['a'..'z']+ }
		Word = { ['0'..'9']+ }
		parse Rows
		""";

	[Theory]
	[InlineData(Rows, new[] { "a" }, "1")]
	[InlineData(Rows, new[] { "a", "aa", "b", "a" }, "1,1,1")]
	[InlineData(Sync, new[] { "a", "b", "aa" }, "1,0,1")]
	[InlineData(Look, new[] { "a", "xa", "aa" }, "1,0,1")]
	[InlineData(Trivia, new[] { "a", "a a", "  aa  " }, "1,1,1")]
	[InlineData(Several, new[] { "k:1", "k:x", "kk:22" }, "1,0,1")]
	public void A_split_grammar_reads_as_the_whole_one(string grammar, string[] segments, string expected)
	{
		var whole = Build(grammar, 0);
		var split = Build(grammar, 1);

		Assert.Equal(expected, Read(whole, segments));
		Assert.Equal(expected, Read(split, segments));
	}

	/// <summary>A <c>find</c> calls the scanner from an engine of its own, in a file of its own.</summary>
	[Fact]
	public void A_split_find_reads_as_the_whole_one()
	{
		foreach (var size in new[] { 0, 1 })
		{
			var parser = Build(Finds, size);
			var found  = parser.GetType("Grammar")!.GetMethod("Found", [typeof(IReadOnlyList<string>)]);

			Assert.NotNull(found);
			Assert.Equal(2, ((System.Collections.IEnumerable)found!.Invoke(null, [new[] { "a", "b", "aa" }])!).Cast<object>().Count());
		}
	}

	static string Read(Assembly parser, string[] segments)
	{
		var read = (IEnumerable<int>)parser.GetType("Grammar")!.GetMethod("ParseRows", [typeof(IReadOnlyList<string>)])!.Invoke(null, [segments])!;

		return string.Join(",", read);
	}

	static Assembly Build(string grammar, int size)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName = "Grammar", Direct = false, SourceFileSize = size, CSharpScanner = RoslynCSharpScanner.Instance,
		});

		EmittedCode.Quiet(result.Diagnostics);

		if (size > 0)
			Assert.True(result.Sources.Count > 1);

		return EmittedCode.Compile(result.Sources[0].Text, sourceParts: result.Sources.Skip(1).Select(source => source.Text));
	}
}
