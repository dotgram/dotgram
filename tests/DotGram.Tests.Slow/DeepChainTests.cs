using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Language;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A chain of calls ten thousand long is compiled on a stack of one megabyte, by the compiler
/// on its own and by the generator as Roslyn runs it (<see cref="DeepChains"/>).
/// </summary>
[Collection(typeof(Alone))]
public sealed class DeepChainTests
{
	/// <summary>The whole compile, as the generator runs it with no host around it.</summary>
	[Theory]
	[MemberData(nameof(DeepChains.Names), MemberType = typeof(DeepChains))]
	public void A_long_chain_of_calls_compiles_on_a_small_stack(string shape)
	{
		var grammar  = DeepChains.Shapes[shape](DeepChains.Depth);
		var compiled = DeepChains.OnSmallStack(() => GramCompiler.Compile(
			grammar, new GramCompilerOptions { CSharpScanner = RoslynCSharpScanner.Instance }));

		Assert.False(
			compiled.HasErrors,
			string.Join("\n", compiled.Diagnostics.Where(static d => d.Severity == GramSeverity.Error).Take(5)));

		Assert.NotEmpty(compiled.Sources);
	}

	/// <summary>
	/// A grammar in a <c>[Gram]</c> attribute, through the generator as Roslyn runs it, with the
	/// host's C# resolved by Roslyn and what it wrote compiled with the host.
	/// </summary>
	[Fact]
	public void A_long_chain_of_calls_in_a_host_class_compiles_on_a_small_stack()
	{
		var grammar = DeepChains.Shapes["guard"](DeepChains.Depth);
		var source  = "using DotGram;\n\n[Gram(\"\"\"\n" + grammar + "\"\"\")]\npublic static partial class Deep;\n";

		var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);
		var compilation  = CSharpCompilation.Create(
			"DotGram.Tests.DeepChain", [CSharpSyntaxTree.ParseText(source, parseOptions, "Deep.cs", cancellationToken: TestContext.Current.CancellationToken)],
			EmittedCode.References, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

		var run = DeepChains.OnSmallStack(() =>
		{
			var driver = CSharpGeneratorDriver
				.Create([new GramGenerator().AsSourceGenerator()], parseOptions: parseOptions)
				.RunGeneratorsAndUpdateCompilation(compilation, out var output, out _);

			return (Result: driver.GetRunResult(), Output: output);
		});

		Assert.Empty(run.Result.Diagnostics.Where(static d => d.Severity == DiagnosticSeverity.Error));
		Assert.NotEmpty(run.Result.Results.SelectMany(static r => r.GeneratedSources));

		var errors = DeepChains.OnSmallStack(() =>
			run.Output.GetDiagnostics(TestContext.Current.CancellationToken)
				.Where(static d => d.Severity == DiagnosticSeverity.Error).Take(5).ToArray());

		Assert.Empty(errors);
	}
}
