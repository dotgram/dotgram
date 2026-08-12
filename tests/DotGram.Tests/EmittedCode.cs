using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;

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

		var compilation = CSharpCompilation.Create(
			"DotGram.Tests.Emitted",
			[
				CSharpSyntaxTree.ParseText(declaration),
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

	static ImmutableArray<MetadataReference> References { get; } =
	[
		.. AppDomain.CurrentDomain
			.GetAssemblies()
			.Where(static assembly => !assembly.IsDynamic && !string.IsNullOrEmpty(assembly.Location))
			.Select(static assembly => (MetadataReference)MetadataReference.CreateFromFile(assembly.Location)),
	];
}
