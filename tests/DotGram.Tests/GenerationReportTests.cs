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
	[InlineData("none", false)]
	[InlineData("false", false)]
	[InlineData("", false)]
	[InlineData("sumary", false)]                  // a word nobody defined is silence, not a guess
	[InlineData("full", true)]
	[InlineData("summary", true)]
	public void Unasked_and_design_time_builds_do_not_emit_reports(string level, bool designTime)
	{
		var run = Run(Source, level, designTime);
		Assert.DoesNotContain(run.GeneratedTrees, tree => tree.FilePath.EndsWith(".DotGramReport.g.cs", StringComparison.Ordinal));
	}

	/// <summary>
	/// The quiet level says the generator ran and what came out of it, and nothing of what it
	/// decided on the way: that is the detail, and a build that did not ask for it does not
	/// have it at all.
	/// </summary>
	[Fact]
	public void The_summary_level_leaves_the_detail_out()
	{
		var report = Report(Run(Source, "summary"));

		Assert.Contains("DotGram: Parser, 1 normalized rules, ", report, StringComparison.Ordinal);
		Assert.Matches(@"[0-9]+\.[0-9]{2} ms generation", report);
		Assert.DoesNotContain("mode=", report, StringComparison.Ordinal);
		Assert.DoesNotContain("options:", report, StringComparison.Ordinal);
		Assert.DoesNotContain(
			Run(Source, "summary").GeneratedTrees,
			tree => tree.FilePath.EndsWith(".DotGramReportDetail.g.cs", StringComparison.Ordinal));
	}

	/// <summary>And the full level writes the detail beside it, in a file of its own.</summary>
	[Fact]
	public void The_full_level_writes_the_detail_beside_it()
	{
		var run = Run(Source, "full");

		Assert.Contains("DotGram: Parser, 1 normalized rules, ", Report(run), StringComparison.Ordinal);
		Assert.Contains("mode=characters", Detail(run), StringComparison.Ordinal);
		Assert.Contains("options: Lexical=", Detail(run), StringComparison.Ordinal);
	}

	/// <summary>`true` is the spelling the levels replaced, and it still means the full report.</summary>
	[Theory]
	[InlineData("true")]
	[InlineData("TRUE")]
	[InlineData("Full")]
	public void The_older_spelling_of_full_is_still_the_full_report(string level)
	{
		var run = Run(Source, level);

		Assert.Contains("DotGram: Parser,", Report(run), StringComparison.Ordinal);
		Assert.Contains("mode=characters", Detail(run), StringComparison.Ordinal);
	}

	[Fact]
	public void Report_describes_the_emitted_parser_without_changing_it()
	{
		var full     = Run(Source, "full");
		var quiet    = Run(Source, "none");
		var parser = full.GeneratedTrees.Single(tree => tree.FilePath.EndsWith("/Parser.g.cs", StringComparison.Ordinal) || tree.FilePath.EndsWith("\\Parser.g.cs", StringComparison.Ordinal)).ToString();
		var baseline = quiet.GeneratedTrees.Single(tree => tree.FilePath.EndsWith("/Parser.g.cs", StringComparison.Ordinal) || tree.FilePath.EndsWith("\\Parser.g.cs", StringComparison.Ordinal)).ToString();
		var report = Report(full);

		Assert.Equal(baseline, parser);
		Assert.Contains("DotGram: Parser, 1 normalized rules, " + Encoding.UTF8.GetByteCount(parser) + " bytes UTF-8 C#", report, StringComparison.Ordinal);
		Assert.Matches(@"[0-9]+\.[0-9]{2} ms generation", report);
		Assert.Contains("mode=characters", Detail(full), StringComparison.Ordinal);
		Assert.DoesNotContain(full.Diagnostics, diagnostic => diagnostic.Id.StartsWith("CS878", StringComparison.Ordinal));
	}

	[Fact]
	public void Failed_generation_does_not_claim_to_have_produced_a_parser()
	{
		var run = Run("[DotGram.Gram(\"Start = Missing\\nparse Start\")] public partial class Parser;", "full");
		Assert.Contains(run.Diagnostics, diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
		Assert.DoesNotContain(run.GeneratedTrees, tree => tree.FilePath.EndsWith(".DotGramReport.g.cs", StringComparison.Ordinal));
	}

	[Fact]
	public void Requested_lexical_mode_is_distinguished_from_character_fallback()
	{
		var run = Run(Source.Replace(")]", ", Lexical = true)]", StringComparison.Ordinal), "full");
		Assert.Contains(run.Diagnostics, diagnostic => diagnostic.Id == "GRAM5004");
		var detail = Detail(run);
		Assert.Contains("mode=characters", detail, StringComparison.Ordinal);
		Assert.Contains("options: Lexical=True,", detail, StringComparison.Ordinal);
	}

	[Fact]
	public void Named_variants_get_separate_reports()
	{
		var run = Run(Source.Replace("public partial", "[DotGram.GramOptions(Suffix = \"Alternate\", Carrier = DotGram.GramCarrier.Tape)] public partial", StringComparison.Ordinal), "full");
		var reports = run.GeneratedTrees.Where(tree => tree.FilePath.EndsWith(".DotGramReport.g.cs", StringComparison.Ordinal)).Select(tree => tree.ToString()).ToArray();
		var details = run.GeneratedTrees.Where(tree => tree.FilePath.EndsWith(".DotGramReportDetail.g.cs", StringComparison.Ordinal)).Select(tree => tree.ToString()).ToArray();
		Assert.Equal(2, reports.Length);
		Assert.Equal(2, details.Length);
		Assert.Contains(reports, report => report.Contains("DotGram: Parser,", StringComparison.Ordinal));
		Assert.Contains(details, detail => detail.Contains("DotGram: Parser.Alternate,", StringComparison.Ordinal) && detail.Contains("Carrier=Tape", StringComparison.Ordinal));
	}

	static string Report(GeneratorDriverRunResult run)
	{
		return run.GeneratedTrees.Single(tree => tree.FilePath.EndsWith(".DotGramReport.g.cs", StringComparison.Ordinal)).ToString();
	}

	static string Detail(GeneratorDriverRunResult run)
	{
		return run.GeneratedTrees.Single(tree => tree.FilePath.EndsWith(".DotGramReportDetail.g.cs", StringComparison.Ordinal)).ToString();
	}

	static GeneratorDriverRunResult Run(string source, string level, bool designTime = false)
	{
		var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);
		var references = AppDomain.CurrentDomain.GetAssemblies()
			.Where(assembly => !assembly.IsDynamic && !string.IsNullOrEmpty(assembly.Location))
			.Select(assembly => MetadataReference.CreateFromFile(assembly.Location));
		var compilation = CSharpCompilation.Create("GenerationReport", [CSharpSyntaxTree.ParseText(source, parseOptions)], references,
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
		return CSharpGeneratorDriver.Create([new GramGenerator().AsSourceGenerator()],
			parseOptions: parseOptions, optionsProvider: new OptionsProvider(level, designTime))
			.RunGenerators(compilation).GetRunResult();
	}

	sealed class OptionsProvider(string level, bool designTime) : AnalyzerConfigOptionsProvider
	{
		public override AnalyzerConfigOptions GlobalOptions { get; } = new Options(level, designTime);
		public override AnalyzerConfigOptions GetOptions(SyntaxTree tree)
		{
			return GlobalOptions;
		}

		public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
		{
			return GlobalOptions;
		}
	}

	sealed class Options(string level, bool designTime) : AnalyzerConfigOptions
	{
		public override bool TryGetValue(string key, out string value)
		{
			value = key switch
			{
				"build_property.DotGramReportGeneration" => level,
				"build_property.DesignTimeBuild" => designTime.ToString(),
				_ => "",
			};
			return value.Length > 0;
		}
	}
}
