using System.Linq;
using System.Threading.Tasks;

using Xunit;

namespace DotGram.GeneratedAnalyzers.Tests;

public class Dga003Tests
{
	static string SwitchWithArms(int arms)
	{
		var cases = string.Join("\n", Enumerable.Range(0, arms).Select(i => $"\t\t\t\tcase {i}: return \"v{i}\";"));

		return $$"""
			class C
			{
				static string Recognize_Dialect_Table(int tag)
				{
					switch (tag)
					{
			{{cases}}
						default: return "";
					}
				}
			}
			""";
	}

	[Fact]
	public async Task Positive_switch_over_32_constant_arms()
	{
		var diagnostics = await AnalyzerHarness.RunAsync(SwitchWithArms(40));

		var found = Assert.Single(diagnostics, d => d.Id == GeneratedCodeAnalyzer.Dga003);
		Assert.Equal(Microsoft.CodeAnalysis.DiagnosticSeverity.Info, found.Severity);
		Assert.Contains("Recognize_Dialect_Table", found.GetMessage());
	}

	[Fact]
	public async Task Negative_switch_at_or_under_the_threshold()
	{
		var diagnostics = await AnalyzerHarness.RunAsync(SwitchWithArms(32));

		Assert.DoesNotContain(diagnostics, d => d.Id == GeneratedCodeAnalyzer.Dga003);
	}

	[Fact]
	public async Task Silent_inside_a_line_mapped_region()
	{
		var cases = string.Join("\n", Enumerable.Range(0, 40).Select(i => $"\t\t\tcase {i}: return \"v{i}\";"));
		var source = $$"""
			class C
			{
				static string M(int tag)
				{
			#line 1 "Grammar.gram"
					switch (tag)
					{
			{{cases}}
						default: return "";
					}
			#line default
				}
			}
			""";

		var diagnostics = await AnalyzerHarness.RunAsync(source);

		Assert.DoesNotContain(diagnostics, d => d.Id == GeneratedCodeAnalyzer.Dga003);
	}
}
