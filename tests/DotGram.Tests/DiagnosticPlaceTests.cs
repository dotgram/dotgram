using System;
using System.Linq;

using DotGram.Generation;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A diagnostic of a grammar written in another file than the host that reads it is placed
/// where the grammar is, and never raised as a failure of the generator.
/// </summary>
/// <remarks>
/// A second declaration of the same class that carries only options reads the grammar the
/// first one wrote (the Tape examples). A warning in that grammar was given the place of the
/// grammar in the second file's text, which is not long enough to hold it.
/// </remarks>
public sealed class DiagnosticPlaceTests
{
	[Fact]
	public void A_warning_in_a_grammar_a_second_declaration_reads_is_placed_in_the_grammar_s_file()
	{
		var parse = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);
		var token = TestContext.Current.CancellationToken;

		// A strength inside an optional reaches no operand: a warning, GRAM4042.
		var grammar = CSharpSyntaxTree.ParseText(
			"""
			namespace N;

			// padding, so that the grammar lies far into its file
			// padding, so that the grammar lies far into its file
			// padding, so that the grammar lies far into its file
			[DotGram.Gram("Start = E\nE = E & '?' & E & (':' & E)? >> 1\n  | ['0'..'9']\nparse Start")]
			public sealed partial class Reader;
			""",
			parse, "Grammar.cs", cancellationToken: token);

		var options = CSharpSyntaxTree.ParseText(
			"""
			namespace N;

			[DotGram.GramOptions(Carrier = DotGram.GramCarrier.Tape, Suffix = "Tape")]
			public sealed partial class Reader;
			""",
			parse, "Options.cs", cancellationToken: token);

		var compilation = CSharpCompilation.Create(
			"DotGram.Tests.DiagnosticPlace",
			[grammar, options],
			EmittedCode.References,
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

		var run = CSharpGeneratorDriver
			.Create([new GramGenerator().AsSourceGenerator()], parseOptions: parse)
			.RunGenerators(compilation, token)
			.GetRunResult();

		Assert.DoesNotContain(run.Diagnostics, static one => one.Id == "GRAM0001");

		Assert.All(
			run.Diagnostics.Where(static one => one.Id == "GRAM4042"),
			one => Assert.Equal("Grammar.cs", one.Location.SourceTree?.FilePath));
		Assert.NotEmpty(run.Diagnostics.Where(static one => one.Id == "GRAM4042"));
	}

	/// <summary>
	/// A host with two readings of one grammar says each of the grammar's warnings once: the
	/// second reading compiles the same text and finds the same warning at the same place,
	/// and a build that printed both showed every line of the grammar's advice twice.
	/// </summary>
	[Fact]
	public void A_warning_two_readings_of_one_grammar_find_is_said_once()
	{
		var parse = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);
		var token = TestContext.Current.CancellationToken;

		var host = CSharpSyntaxTree.ParseText(
			"""
			namespace N;

			[DotGram.Gram("Start = E\nE = E & '?' & E & (':' & E)? >> 1\n  | ['0'..'9']\nparse Start")]
			[DotGram.GramOptions(Carrier = DotGram.GramCarrier.Tape, Suffix = "Tape")]
			public sealed partial class Reader;
			""",
			parse, "Grammar.cs", cancellationToken: token);

		var compilation = CSharpCompilation.Create(
			"DotGram.Tests.DiagnosticPlace",
			[host],
			EmittedCode.References,
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

		var run = CSharpGeneratorDriver
			.Create([new GramGenerator().AsSourceGenerator()], parseOptions: parse)
			.RunGenerators(compilation, token)
			.GetRunResult();

		Assert.Single(run.Diagnostics, static one => one.Id == "GRAM4042");
	}
}
