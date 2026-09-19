using System.IO;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>The buffered-input test that costs a compilation of nine hundred rules (D12).</summary>
public sealed partial class BufferedInputTests
{
	[Fact]
	public void Large_split_dispatch_keeps_char_and_byte_backtracking_correct()
	{
		const int count = 912;
		var rules = Enumerable.Range(0, count).Select(i => $"R{i} : @int = \"{i}=\" & ['0'..'9']+ & ';' => @({i})");
		var grammar = string.Join("\n", rules) + "\nItem : @int = (" +
			string.Join(" | ", Enumerable.Range(0, count).Select(i => $"v: R{i}")) +
			") => @(v)\nStart : @int[] = Item+\nparse Start stream bytes";
		var options = new GramCompilerOptions
		{
			BufferedInput = true, Direct = false, PartSize = 128, CSharpScanner = RoslynCSharpScanner.Instance,
		};
		var single = GramCompiler.Compile(grammar, options);
		EmittedCode.Quiet(single.Diagnostics);
		Assert.Single(single.Sources);
		options.SourceFileSize = 2_000_000;
		var compilation = GramCompiler.Compile(grammar, options);
		EmittedCode.Quiet(compilation.Diagnostics);
		Assert.True(compilation.Sources.Count > 1);
		var source = compilation.Sources[0].Text;
		Assert.Contains("_Dispatch = new int[]", source);
		Assert.All(compilation.Sources, part => Assert.DoesNotContain("switch (chosen)", part.Text));
		var assembly = EmittedCode.Compile(source, sourceParts: compilation.Sources.Skip(1).Select(part => part.Text));
		var expected = Enumerable.Range(0, count).Reverse().ToArray();
		var text = string.Concat(expected.Select(i => $"{i}=123;"));
		Assert.Equal(expected, Assert.IsType<int[]>(EmittedCode.Match(assembly, "Grammar", "TryParseStart", text).Value));
		Assert.Equal(expected, Assert.IsType<int[]>(Read(assembly, new ShortReader(text, 3), 2).Value));
		using var bytes = new ShortStream(text.Select(c => (byte)c).ToArray());
		var method = assembly.GetType("Grammar")!.GetMethod("TryParseStart", [typeof(Stream), typeof(int?), typeof(int?)])!;
		var result = method.Invoke(null, [bytes, 2, text.Length + 1])!;
		Assert.True((bool)result.GetType().GetProperty("IsSuccess")!.GetValue(result)!);
		Assert.Equal(expected, Assert.IsType<int[]>(result.GetType().GetProperty("Value")!.GetValue(result)));
		Assert.False(Read(assembly, new ShortReader(text + "599=bad;", 3), 2).Success);
	}
}
