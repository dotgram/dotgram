using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

using DotGram.Generation;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace DotGram.Tests.SlowShapes;

/// <summary>A kept grammar run through the generator in this process, with the entry counters written.</summary>
static partial class SlowShapeCompiler
{
	static readonly ConcurrentDictionary<string, Lazy<Type>> Compiled = new();

	[GeneratedRegex(@"DotGram\.Examples\.(Formats|Expressions|Languages)")]
	private static partial Regex ExampleNamespace();

	/// <summary>The directory of the kept sources.</summary>
	public static string Sources { get; } = Path.Combine(SourceRoot.Directory, "tests", "DotGram.Tests.Slow", "SlowShapes", "Sources");

	/// <summary>The parser class of a shape, compiled once a process.</summary>
	public static Type Parser(SlowShape shape)
	{
		return Compiled.GetOrAdd(shape.Id, _ => new Lazy<Type>(() => Compile(shape), LazyThreadSafetyMode.ExecutionAndPublication)).Value;
	}

	static Type Compile(SlowShape shape)
	{
		var ns     = "DotGram.SlowShapes." + shape.Id;
		var text   = ExampleNamespace().Replace(File.ReadAllText(Path.Combine(Sources, shape.Id + ".cs.txt")), ns);
		var parse  = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest).WithPreprocessorSymbols("DOTGRAM_COUNTS");
		var tree   = CSharpSyntaxTree.ParseText(text, parse, shape.Id + ".cs");

		var compilation = CSharpCompilation.Create(
			"DotGram.SlowShapes." + shape.Id,
			[tree],
			EmittedCode.References,
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

		CSharpGeneratorDriver
			.Create([new GramGenerator().AsSourceGenerator()], parseOptions: parse)
			.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var reported);

		Assert.DoesNotContain(reported, static one => one.Severity == DiagnosticSeverity.Error);

		// Or the switch did not reach the generator, and nothing below counts anything.
		Assert.Contains(output.SyntaxTrees, static one => one.ToString().Contains("CountEntered_", StringComparison.Ordinal));

		using var stream = new MemoryStream();

		var result = output.Emit(stream);

		Assert.True(
			result.Success,
			$"{shape.Id} did not compile:\n" + string.Join("\n", result.Diagnostics.Where(static one => one.Severity == DiagnosticSeverity.Error).Take(20)));

		return Assembly.Load(stream.ToArray()).GetType(ns + "." + shape.Class)!;
	}
}
