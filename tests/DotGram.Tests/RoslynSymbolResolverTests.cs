using System;

using DotGram.Generation;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Xunit;

namespace DotGram.Tests;

public sealed class RoslynSymbolResolverTests
{
	[Fact]
	public void A_missing_type_can_appear_in_the_next_compilation()
	{
		var original = Compilation("public class Host { }");
		var before = new RoslynSymbolResolver(original, "Host");

		Assert.False(before.TypeExists("Row"));
		Assert.False(before.TypeExists("Row"));

		var edited = original.AddSyntaxTrees(CSharpSyntaxTree.ParseText("public class Row { }", cancellationToken: TestContext.Current.CancellationToken));
		var after = new RoslynSymbolResolver(edited, "Host");

		Assert.True(after.TypeExists("Row"));
		Assert.True(after.IsAssignable("Row", "object"));
		Assert.False(before.TypeExists("Row"));
	}

	[Fact]
	public void A_resolved_type_can_disappear_in_the_next_compilation()
	{
		var original = Compilation("public class Row { }");
		var before = new RoslynSymbolResolver(original);

		Assert.True(before.TypeExists("Row"));
		Assert.True(before.TypeExists("Row"));

		var after = new RoslynSymbolResolver(original.RemoveAllSyntaxTrees());

		Assert.False(after.TypeExists("Row"));
		Assert.True(before.TypeExists("Row"));
	}

	[Fact]
	public void The_same_short_name_resolves_separately_for_each_host()
	{
		var compilation = Compilation("""
			public class First { public class Row { } }
			public class Second { public class Row { } }
			""");
		var first = new RoslynSymbolResolver(compilation, "First");
		var second = new RoslynSymbolResolver(compilation, "Second");

		Assert.True(first.TypeExists("Row"));
		Assert.True(second.TypeExists("Row"));
		Assert.True(first.IsAssignable("Row", "First.Row"));
		Assert.False(first.IsAssignable("Row", "Second.Row"));
		Assert.True(second.IsAssignable("Row", "Second.Row"));
		Assert.False(second.IsAssignable("Row", "First.Row"));
	}

	static CSharpCompilation Compilation(string source)
	{
		return CSharpCompilation.Create(
		"ResolverTests",
		[CSharpSyntaxTree.ParseText(source, cancellationToken: TestContext.Current.CancellationToken)],
		[MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
		new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
	}
}
