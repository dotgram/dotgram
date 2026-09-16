using System;
using System.Linq;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

public sealed class ExpectedTablesTests
{
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Sibling_publications_share_diagnostics_without_changing_failures(bool buffered)
	{
		var compiled = GramCompiler.Compile("""
			First = 'a' & when @(true) & 'x'
			Second = 'b' & when @(true) & 'x'
			parse First
			parse Second
			""", new GramCompilerOptions
		{
			BufferedInput = buffered, BufferedBytes = buffered, CSharpScanner = RoslynCSharpScanner.Instance,
		});
		EmittedCode.Quiet(compiled.Diagnostics);
		var members = """
			public static string ErrorOf(string input, int root)
			{
				return root == 0 ? TryParseFirst(input).Error : TryParseSecond(input).Error;
			}
			""";
		if (buffered)
			members += """
			public static string ReaderError(string input, int root)
			{
				using (var reader = new global::System.IO.StringReader(input))
					return root == 0 ? TryParseFirst(reader).Error : TryParseSecond(reader).Error;
			}
			public static string ByteError(string input, int root)
			{
				using (var stream = new global::System.IO.MemoryStream(global::System.Text.Encoding.ASCII.GetBytes(input)))
					return root == 0 ? TryParseFirst(stream).Error : TryParseSecond(stream).Error;
			}
			""";
		var assembly = EmittedCode.Compile(Assert.Single(compiled.Sources).Text, declarationMembers: members);
		var type = assembly.GetType("Grammar")!;
		var tables = type.GetFields(BindingFlags.Static | BindingFlags.NonPublic)
			.Where(field => field.FieldType == typeof(string[]) && field.Name.Contains("_Expected", StringComparison.Ordinal))
			.Select(field => (string[])field.GetValue(null)!).ToArray();

		Assert.Single(tables, table => table.SequenceEqual(new[] { "'x'" }));
		Assert.Equal(tables.Length, tables.Select(table => string.Join("\0", table)).Distinct(StringComparer.Ordinal).Count());

		foreach (var method in buffered ? new[] { "ErrorOf", "ReaderError", "ByteError" } : new[] { "ErrorOf" })
		for (var repeat = 0; repeat < 2; repeat++)
		{
			Assert.Equal("Expected 'x'.", type.GetMethod(method)!.Invoke(null, ["az", 0]));
			Assert.Equal("Expected 'x'.", type.GetMethod(method)!.Invoke(null, ["bz", 1]));
			Assert.Null(type.GetMethod(method)!.Invoke(null, ["ax", 0]));
			Assert.Null(type.GetMethod(method)!.Invoke(null, ["bx", 1]));
		}
	}
}
