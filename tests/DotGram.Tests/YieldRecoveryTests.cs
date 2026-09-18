using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

public sealed class YieldRecoveryTests
{
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Recovery_is_lazy_and_preserves_ordinals_positions_and_eof(bool spans)
	{
		var result = GramCompiler.Compile("""
			End = '|'
			Row : @string = 'R' & text: ['a'..'z']+ & ';' => @(Text(text))
			Rows : @string[] = Row* recover End => @(Bad(parserText, parserPosition, parserOrdinal))
			parse Rows with (End = ';') as All
			parse Rows with (End = ';') as Read stream bytes yield : @string
			""", new GramCompilerOptions { BufferedInput = true, BufferedBytes = true, SpanCaptures = spans, CSharpScanner = RoslynCSharpScanner.Instance });
		EmittedCode.Quiet(result.Diagnostics);
		var assembly = EmittedCode.Compile(Assert.Single(result.Sources).Text, declarationMembers: """
			static string Text(string value) => value;
			static string Text(global::System.ReadOnlySpan<char> value) => value.ToString();
			static string Text(global::System.ReadOnlySpan<byte> value) => global::System.Text.Encoding.ASCII.GetString(value.ToArray());
			static string Bad(string text, long position, int ordinal) => ordinal + ":" + position + ":" + text;
			static string Bad(global::System.ReadOnlySpan<char> text, long position, int ordinal) => Bad(text.ToString(), position, ordinal);
			static string Bad(global::System.ReadOnlySpan<byte> text, long position, int ordinal) => Bad(Text(text), position, ordinal);
			""");
		var host = assembly.GetType("Grammar")!;
		const string input = "Ra;X;Rb;R1;Y";
		var expected = new[] { "a", "1:3:X", "b", "3:8:R1", "4:11:Y" };
		Assert.Equal(expected, (string[])host.GetMethod("All", new[] { typeof(string) })!.Invoke(null, new object[] { input })!);
		Assert.Equal(expected, (IEnumerable<string>)host.GetMethod("Read", new[] { typeof(string) })!.Invoke(null, new object[] { input })!);
		using var reader = new StringReader(input);
		using var stream = new MemoryStream(Encoding.ASCII.GetBytes(input));
		foreach (var source in new object[] { reader, stream })
		{
			var domain = source is Stream ? typeof(Stream) : typeof(TextReader);
			var values = (IEnumerable<string>)host.GetMethod("Read", new[] { domain, typeof(int?), typeof(int?) })!.Invoke(null, new object[] { source, 1, 16 })!;
			Assert.Equal(expected, values.ToArray());
		}
		Assert.True(stream.CanRead);
		using var many = new MemoryStream(Encoding.ASCII.GetBytes(string.Concat(Enumerable.Repeat(input + ";", 100))));
		var bounded = (IEnumerable<string>)host.GetMethod("Read", new[] { typeof(Stream), typeof(int?), typeof(int?) })!
			.Invoke(null, new object[] { many, 1, 16 })!;
		Assert.Equal(500, bounded.Count());
	}
	[Fact]
	public void Recovery_without_a_factory_drops_bad_elements_and_honors_the_minimum()
	{
		var result = GramCompiler.Compile("""
			Row : @string = 'R' & text: ['a'..'z']+ & ';' => @(text)
			Rows : @string[] = Row+ recover ';'
			parse Rows as Read yield : @string
			""", new GramCompilerOptions { CSharpScanner = RoslynCSharpScanner.Instance });
		EmittedCode.Quiet(result.Diagnostics);
		var assembly = EmittedCode.Compile(Assert.Single(result.Sources).Text);
		var method = assembly.GetType("Grammar")!.GetMethod("Read", new[] { typeof(string) })!;
		Assert.Equal(new[] { "a", "b" }, ((IEnumerable<string>)method.Invoke(null, new object[] { "X;Ra;Y;Rb;Z" })!).ToArray());
		Assert.Throws<FormatException>(() => ((IEnumerable<string>)method.Invoke(null, new object[] { "" })!).ToArray());
	}
}
