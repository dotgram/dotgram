using System;

using Xunit;

namespace DotGram.Cli.Tests;

/// <summary>
/// The generated parsers in a project that is itself a Roslyn analyzer or source generator: the
/// extended analyzer rules ban APIs there (RS1035, culture, environment, file and console access),
/// and the files the generator writes have no folder of their own for an editorconfig to relax them
/// in, so a banned API in them would fail such a build with warnings as errors, with nothing the
/// author could narrowly suppress.
/// </summary>
public sealed class AnalyzerProjectTests
{
	/// <summary>A recursive grammar: its readings move to a stack of their own where the stack runs low.</summary>
	const string Nest = "Nest = '(' & Nest* & ')' | Word\n\nWord = ['a'..'z']+\n\nparse Nest\n";

	/// <summary>On the floor an analyzer targets, and on a current framework.</summary>
	[Theory]
	[InlineData("netstandard2.0", "<LangVersion>8.0</LangVersion>", "<PackageReference Include=\"System.Memory\" Version=\"4.6.3\" />")]
	[InlineData("net10.0", "", "")]
	public void Generated_parsers_pass_the_extended_analyzer_rules_with_warnings_as_errors(string framework, string language, string memory)
	{
		using var scratch = new Scratch();

		var project = SnapshotGrammarTests.Project(
			scratch,
			framework,
			$"""
			{language}
			    <IsRoslynComponent>true</IsRoslynComponent>
			    <EnforceExtendedAnalyzerRules>true</EnforceExtendedAnalyzerRules>
			    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
			    <!-- What the snapshot grammars say about themselves (they are written to show a shape, not to be
			         clean grammars), not what the analyzer rules say about the files written for them. -->
			    <NoWarn>$(NoWarn);GRAM4052;GRAM4059;GRAM5016</NoWarn>
			""",
			$"""
			{memory}
			    <PackageReference Include="Microsoft.CodeAnalysis.Analyzers" Version="5.9.0" PrivateAssets="all" />
			""");

		scratch.Write("Grammars/Nest.gram", Nest);
		scratch.Write("NestHost.cs", "using DotGram;\n\nnamespace DotGram.Snapshots\n{\n\t[Gram(\"Nest.gram\")]\n\tpublic partial class Nest { }\n}\n");

		var (exit, output) = Build.Try(project, "Configuration=Release");

		Assert.True(exit == 0, output);
	}
}
