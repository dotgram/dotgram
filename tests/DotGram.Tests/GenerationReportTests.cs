using System;
using System.Linq;
using System.Text;

using DotGram.Generation;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace DotGram.Tests;

public sealed class GenerationReportTests
{
	const string Source = "[DotGram.Gram(\"Start = 'x'+\\nparse Start as Read\")] public partial class Parser;";

	[Theory]
	[InlineData(false, false)]
	[InlineData(true, true)]
	public void Disabled_and_design_time_builds_do_not_emit_reports(bool enabled, bool designTime)
	{
		var run = Run(Source, enabled, designTime);
		Assert.DoesNotContain(run.GeneratedTrees, tree => tree.FilePath.EndsWith(".DotGramReport.g.cs", StringComparison.Ordinal));
	}

	[Fact]
	public void Report_describes_the_emitted_parser_without_changing_it()
	{
		var enabled = Run(Source, true);
		var disabled = Run(Source, false);
		var parser = enabled.GeneratedTrees.Single(tree => tree.FilePath.EndsWith("/Parser.g.cs", StringComparison.Ordinal) || tree.FilePath.EndsWith("\\Parser.g.cs", StringComparison.Ordinal)).ToString();
		var baseline = disabled.GeneratedTrees.Single(tree => tree.FilePath.EndsWith("/Parser.g.cs", StringComparison.Ordinal) || tree.FilePath.EndsWith("\\Parser.g.cs", StringComparison.Ordinal)).ToString();
		var report = Report(enabled);

		Assert.Equal(baseline, parser);
		Assert.Contains("DotGram: Parser, 1 normalized rules, " + Encoding.UTF8.GetByteCount(parser) + " bytes UTF-8 C#", report, StringComparison.Ordinal);
		Assert.Matches(@"[0-9]+\.[0-9]{2} ms generation", report);
		Assert.Contains("mode=characters", report, StringComparison.Ordinal);
		Assert.DoesNotContain(enabled.Diagnostics, diagnostic => diagnostic.Id.StartsWith("CS878", StringComparison.Ordinal));
	}

	[Fact]
	public void Failed_generation_does_not_claim_to_have_produced_a_parser()
	{
		var run = Run("[DotGram.Gram(\"Start = Missing\\nparse Start\")] public partial class Parser;", true);
		Assert.Contains(run.Diagnostics, diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
		Assert.DoesNotContain(run.GeneratedTrees, tree => tree.FilePath.EndsWith(".DotGramReport.g.cs", StringComparison.Ordinal));
	}

	[Fact]
	public void Requested_lexical_mode_is_distinguished_from_character_fallback()
	{
		var run = Run(Source.Replace(")]", ", Lexical = true)]", StringComparison.Ordinal), true);
		Assert.Contains(run.Diagnostics, diagnostic => diagnostic.Id == "GRAM5004");
		var report = Report(run);
		Assert.Contains("mode=characters", report, StringComparison.Ordinal);
		Assert.Contains("options: Lexical=True,", report, StringComparison.Ordinal);
	}

	[Fact]
	public void Named_variants_get_separate_reports()
	{
		var run = Run(Source.Replace("public partial", "[DotGram.GramOptions(Suffix = \"Alternate\", Carrier = DotGram.GramCarrier.Tape)] public partial", StringComparison.Ordinal), true);
		var reports = run.GeneratedTrees.Where(tree => tree.FilePath.EndsWith(".DotGramReport.g.cs", StringComparison.Ordinal)).Select(tree => tree.ToString()).ToArray();
		Assert.Equal(2, reports.Length);
		Assert.Contains(reports, report => report.Contains("DotGram: Parser,", StringComparison.Ordinal));
		Assert.Contains(reports, report => report.Contains("DotGram: Parser.Alternate,", StringComparison.Ordinal) && report.Contains("Carrier=Tape", StringComparison.Ordinal));
	}

	static string Report(GeneratorDriverRunResult run) => run.GeneratedTrees.Single(tree => tree.FilePath.EndsWith(".DotGramReport.g.cs", StringComparison.Ordinal)).ToString();

	static GeneratorDriverRunResult Run(string source, bool enabled, bool designTime = false)
	{
		var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);
		var references = AppDomain.CurrentDomain.GetAssemblies()
			.Where(assembly => !assembly.IsDynamic && !string.IsNullOrEmpty(assembly.Location))
			.Select(assembly => MetadataReference.CreateFromFile(assembly.Location));
		var compilation = CSharpCompilation.Create("GenerationReport", [CSharpSyntaxTree.ParseText(source, parseOptions)], references,
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
		return CSharpGeneratorDriver.Create([new GramGenerator().AsSourceGenerator()],
			parseOptions: parseOptions, optionsProvider: new OptionsProvider(enabled, designTime))
			.RunGenerators(compilation).GetRunResult();
	}

	sealed class OptionsProvider(bool enabled, bool designTime) : AnalyzerConfigOptionsProvider
	{
		public override AnalyzerConfigOptions GlobalOptions { get; } = new Options(enabled, designTime);
		public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => GlobalOptions;
		public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => GlobalOptions;
	}

	sealed class Options(bool enabled, bool designTime) : AnalyzerConfigOptions
	{
		public override bool TryGetValue(string key, out string value)
		{
			value = key switch
			{
				"build_property.DotGramReportGeneration" => enabled.ToString(),
				"build_property.DesignTimeBuild" => designTime.ToString(),
				_ => "",
			};
			return value.Length > 0;
		}
	}
}
