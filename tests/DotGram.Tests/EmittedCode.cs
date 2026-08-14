using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;

using DotGram.Grammar;
using DotGram.Grammar.Emit;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Compiles what the emitter wrote.
/// </summary>
/// <remarks>
/// Asserting on generated text says only that the generator agrees with itself.
/// Compiling it says it is valid C# — which is the claim that matters, since the
/// consumer's build is where it would otherwise fail.
/// </remarks>
static class EmittedCode
{
	/// <summary>The generated source, plus the partial class it is a half of.</summary>
	public static Assembly Compile(string source, string className = "Grammar", string? @namespace = null)
	{
		var declaration = @namespace is null
			? $"public partial class {className} {{ }}"
			: $"namespace {@namespace} {{ public partial class {className} {{ }} }}";

		// `[Gram]` and `SourceSpan` come with every generated parser in a real build, so
		// they come with one here too. A grammar that names `span` in a `recover` reaches
		// for `DotGram.SourceSpan`, and a harness without it would fail on code that
		// compiles perfectly well where it is actually emitted.
		var compilation = CSharpCompilation.Create(
			"DotGram.Tests.Emitted",
			[
				CSharpSyntaxTree.ParseText(declaration),
				CSharpSyntaxTree.ParseText(GramCompiler.EmitMarkerAttributes().Text),
				CSharpSyntaxTree.ParseText(source),
			],
			References,
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

		using var stream = new MemoryStream();

		var result = compilation.Emit(stream, cancellationToken: TestContext.Current.CancellationToken);

		// Warnings count as failure. Generated code lands in someone else's build, where
		// a warning is noise the author cannot fix and did not ask for — an unused
		// variable in a recognizer is our bug, and it should read as one here.
		var complaints = result.Diagnostics
			.Where(static diagnostic => diagnostic.Severity is DiagnosticSeverity.Error or DiagnosticSeverity.Warning)
			.ToArray();

		Assert.True(
			complaints.Length == 0,
			"Emitted source did not compile cleanly:\n" +
			string.Join("\n", complaints.Select(static diagnostic => diagnostic.ToString())) +
			"\n\n" + source);

		return Assembly.Load(stream.ToArray());
	}

	/// <summary>
	/// Calls a generated <c>Try…</c> method and reads the <c>Match&lt;T&gt;</c> it hands
	/// back.
	/// </summary>
	/// <remarks>
	/// By reflection because the types did not exist when this was compiled.
	/// <c>GeneratedApiTests</c> and <c>ExampleTests</c> ask the compiler the same
	/// questions instead, over grammars the generator ran on during this build.
	/// </remarks>
	public static (bool IsSuccess, object? Value, string? Error, long Position) Match(
		Assembly assembly, string className, string method, string input)
	{
		var type  = assembly.GetType(className)!;
		var match = type.GetMethod(method)!.Invoke(null, [input])!;

		object? Read(string name) => match.GetType().GetProperty(name)!.GetValue(match);

		return ((bool)Read("IsSuccess")!, Read("Value"), (string?)Read("Error"), (long)Read("Position")!);
	}

	/// <summary>Calls a generated <c>find</c> method and reads the values it yields.</summary>
	/// <param name="over">
	/// Which overload to call: the whole input at once, or a reader read through a window
	/// (§6.3). Named rather than inferred from the argument, because both are asked the
	/// same question and the point of the pair is that they answer it the same way.
	/// </param>
	public static object?[] Found(
		Assembly assembly, string className, string method, string input, Type? over = null)
	{
		var type   = assembly.GetType(className)!;
		var taking = over ?? typeof(string);

		var found = (System.Collections.IEnumerable)type
			.GetMethod(method, [taking])!
			.Invoke(null, [over is null ? input : new StringReader(input)])!;

		var values = new System.Collections.Generic.List<object?>();

		foreach (var match in found)
			values.Add(match!.GetType().GetProperty("Value")!.GetValue(match));

		return [.. values];
	}

	/// <summary>Where a generated <c>find</c> says each of its occurrences was.</summary>
	public static long[] FoundAt(
		Assembly assembly, string className, string method, string input, Type? over = null)
	{
		var type   = assembly.GetType(className)!;
		var taking = over ?? typeof(string);

		var found = (System.Collections.IEnumerable)type
			.GetMethod(method, [taking])!
			.Invoke(null, [over is null ? input : new StringReader(input)])!;

		var positions = new System.Collections.Generic.List<long>();

		foreach (var match in found)
			positions.Add((long)match!.GetType().GetProperty("Position")!.GetValue(match)!);

		return [.. positions];
	}

	static ImmutableArray<MetadataReference> References { get; } =
	[
		.. AppDomain.CurrentDomain
			.GetAssemblies()
			.Where(static assembly => !assembly.IsDynamic && !string.IsNullOrEmpty(assembly.Location))
			.Select(static assembly => (MetadataReference)MetadataReference.CreateFromFile(assembly.Location)),
	];
}
