using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

using DotGram.ExpressionLanguage;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Xunit;

namespace DotGram.Tests.ExpressionLanguage;

public sealed class ResolutionScopeTests
{
	[Fact]
	public void An_unrelated_loaded_library_changes_neither_types_namespaces_nor_extensions()
	{
		var first = Library("First", "AnchorA", 1);
		var second = Library("Second", "AnchorB", 2);
		var callerImage = Emit("Caller", "public class Caller { public object A() => new Probe.AnchorA(); }", first);
		var context = new Context(new() { ["First"] = first, ["Second"] = second });
		try
		{
			var caller = context.LoadFromStream(new MemoryStream(callerImage));
			Check();
			context.LoadFromAssemblyName(new AssemblyName("Second"));
			Check();

			void Check()
			{
				Assert.Equal(1, ExpressionParser.Compile<Func<int>>("() => Probe.Shared.Value", caller)());
				Assert.Equal(4, ExpressionParser.Compile<Func<int, int>>("using Probe; (int x) => x.Extra()", caller)(3));
				Assert.False(ExpressionParser.TryParse("using Noise; () => 1", caller).IsSuccess);
			}
		}
		finally { context.Unload(); }
	}

	[Fact]
	public void Duplicate_full_names_in_referenced_libraries_are_ambiguous()
	{
		var first = Library("First", "AnchorA", 1);
		var second = Library("Second", "AnchorB", 2);
		var callerImage = Emit("Caller", "public class Caller { public object A() => new Probe.AnchorA(); public object B() => new Probe.AnchorB(); }", first, second);
		var context = new Context(new() { ["First"] = first, ["Second"] = second });
		try
		{
			var caller = context.LoadFromStream(new MemoryStream(callerImage));
			var match = ExpressionParser.TryParse("() => Probe.Shared.Value", caller);
			Assert.False(match.IsSuccess);
			Assert.Contains("ambiguous", match.Error, StringComparison.OrdinalIgnoreCase);
		}
		finally { context.Unload(); }
	}

	[Fact]
	public void Identical_assembly_names_in_separate_contexts_keep_their_own_types()
	{
		var first = Library("Dependency", "AnchorA", 1);
		var second = Library("Dependency", "AnchorA", 2);
		var callerImage = Emit("Caller", "public class Caller { public object A() => new Probe.AnchorA(); }", first);
		var left = new Context(new() { ["Dependency"] = first });
		var right = new Context(new() { ["Dependency"] = second });
		try
		{
			var callerA = left.LoadFromStream(new MemoryStream(callerImage));
			var callerB = right.LoadFromStream(new MemoryStream(callerImage));
			Assert.Equal(1, ExpressionParser.Compile<Func<int>>("() => Probe.Shared.Value", callerA)());
			Assert.Equal(2, ExpressionParser.Compile<Func<int>>("() => Probe.Shared.Value", callerB)());
		}
		finally { left.Unload(); right.Unload(); }
	}

	static byte[] Library(string name, string anchor, int value) => Emit(name, $$"""
		namespace Probe
		{
			public class {{anchor}} { }
			public static class Shared { public static int Value => {{value}}; }
			public static class Extensions { public static int Extra(this int value) => value + {{value}}; }
		}
		{{(value == 2 ? "namespace Noise { public class Unrelated { } }" : "")}}
		""");

	static byte[] Emit(string name, string source, params byte[][] dependencies)
	{
		var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
			.Select(path => MetadataReference.CreateFromFile(path)).Cast<MetadataReference>()
			.Concat(dependencies.Select(image => MetadataReference.CreateFromImage(image)));
		var compilation = CSharpCompilation.Create(name, [CSharpSyntaxTree.ParseText(source)], references,
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
		using var stream = new MemoryStream();
		var result = compilation.Emit(stream);
		Assert.True(result.Success, string.Join("\n", result.Diagnostics));
		return stream.ToArray();
	}

	sealed class Context(Dictionary<string, byte[]> assemblies) : AssemblyLoadContext(isCollectible: true)
	{
		protected override Assembly? Load(AssemblyName name) => assemblies.TryGetValue(name.Name!, out var image)
			? LoadFromStream(new MemoryStream(image)) : null;
	}
}
