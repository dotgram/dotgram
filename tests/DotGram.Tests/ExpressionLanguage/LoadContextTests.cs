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

/// <summary>
/// A caller loaded into a load context of its own — a plugin — has its references found in that
/// context, as the runtime binds them for its own code.
/// </summary>
/// <remarks>
/// The references used to be loaded with <c>Assembly.Load</c>, which binds in the context of the
/// assembly calling it, the expression language's own, so a plugin's references were looked for
/// in the default context, were not there, and nothing in them could be named. The libraries here
/// are compiled in memory so that no context but the test's own can find them.
/// </remarks>
public sealed class LoadContextTests
{
	[Fact]
	public void A_caller_in_a_context_of_its_own_names_what_it_references()
	{
		var first   = Library("First", "AnchorA", 1);
		var second  = Library("Second", "AnchorB", 2);
		var context = new Context(new() { ["First"] = first, ["Second"] = second });
		var caller  = context.LoadFromStream(new MemoryStream(Emit("Caller", "public class Caller { public object A() => new Probe.AnchorA(); }", first)));

		Check();

		// Loaded into the same context, and not referenced by the caller: still nothing it can name.
		context.LoadFromAssemblyName(new AssemblyName("Second"));

		Check();

		void Check()
		{
			Assert.Equal(1, ExpressionParser.Compile<Func<int>>("() => Probe.Shared.Value", caller)());
			Assert.Equal(4, ExpressionParser.Compile<Func<int, int>>("using Probe; (int x) => x.Extra()", caller)(3));
			Assert.False(ExpressionParser.TryParse("using Noise; () => 1", caller).IsSuccess);
		}
	}

	[Fact]
	public void One_assembly_name_in_two_contexts_keeps_each_its_own_types()
	{
		var image   = Emit("Caller", "public class Caller { public object A() => new Probe.AnchorA(); }", Library("Dependency", "AnchorA", 1));
		var callerA = new Context(new() { ["Dependency"] = Library("Dependency", "AnchorA", 1) }).LoadFromStream(new MemoryStream(image));
		var callerB = new Context(new() { ["Dependency"] = Library("Dependency", "AnchorA", 2) }).LoadFromStream(new MemoryStream(image));

		Assert.Equal(1, ExpressionParser.Compile<Func<int>>("() => Probe.Shared.Value", callerA)());
		Assert.Equal(2, ExpressionParser.Compile<Func<int>>("() => Probe.Shared.Value", callerB)());
	}

	static byte[] Library(string name, string anchor, int value)
	{
		return Emit(name, $$"""
			namespace Probe
			{
				public class {{anchor}} { }
				public static class Shared { public static int Value => {{value}}; }
				public static class Extensions { public static int Extra(this int value) => value + {{value}}; }
			}
			{{(value == 2 ? "namespace Noise { public class Unrelated { } }" : "")}}
			""");
	}

	static byte[] Emit(string name, string source, params byte[][] dependencies)
	{
		var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
			.Select(static path => (MetadataReference)MetadataReference.CreateFromFile(path))
			.Concat(dependencies.Select(static image => MetadataReference.CreateFromImage(image)));
		var compilation = CSharpCompilation.Create(
			name, [CSharpSyntaxTree.ParseText(source)], references,
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

		using var stream = new MemoryStream();

		var result = compilation.Emit(stream, cancellationToken: TestContext.Current.CancellationToken);

		Assert.True(result.Success, string.Join("\n", result.Diagnostics));

		return stream.ToArray();
	}

	/// <summary>
	/// A context that finds its libraries in memory and nowhere else. Not collectible: a scope
	/// keeps the assemblies it has answered from, so it could not unload anyway.
	/// </summary>
	sealed class Context(Dictionary<string, byte[]> assemblies) : AssemblyLoadContext
	{
		protected override Assembly? Load(AssemblyName name)
		{
			return assemblies.TryGetValue(name.Name!, out var image)
				? LoadFromStream(new MemoryStream(image))
				: null;
		}
	}
}
