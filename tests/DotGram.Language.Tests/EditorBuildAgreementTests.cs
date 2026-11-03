using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DotGram.Grammar;
using DotGram.Grammar.Emit;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using Xunit;

namespace DotGram.Language.Tests;

/// <summary>
/// A grammar in a <c>[Gram]</c> is shown in the C# file what the build reports for it there, and
/// nothing the build does not: what the host's attribute asks for, what the file's <c>#pragma</c>s
/// and the project's configuration make of a diagnostic, and what only the host's C# can answer.
/// </summary>
public sealed class EditorBuildAgreementTests
{
	/// <summary>
	/// A rule with captures and no <c>=&gt;</c> is built by a constructor its captures fill (§7.3),
	/// which the generator finds in the host's C#. The editor does not look, and was showing the
	/// rule an error for a constructor it had not looked for.
	/// </summary>
	[Fact]
	public void A_rule_its_captures_build_is_not_told_it_has_no_constructor()
	{
		const string grammar = """
			trivia = none
			Row : @Row = name: Name & ',' & size: Name
			Name : @string = t: ['a'..'z']+ => @(t)
			parse Row
			""";

		Assert.Contains(Unseen(grammar), static diagnostic => diagnostic.Id == "GRAM4008");

		var shown = Shown(Host(grammar, "", "public sealed record Row(string Name, string Size);"));

		Assert.DoesNotContain(shown, static diagnostic => diagnostic.Id == "GRAM4008");
		Assert.DoesNotContain(shown, static diagnostic => diagnostic.Severity == GramSeverity.Error);
	}

	/// <summary>
	/// A buffered form reads through a recognizer of the host only where the recognizer takes the
	/// generated input view, which the generator asks the host about. The editor does not, and
	/// was refusing every buffered publication over such a recognizer.
	/// </summary>
	[Fact]
	public void A_buffered_form_over_a_recognizer_of_the_host_is_not_refused_for_its_shape()
	{
		const string grammar = """
			Field = 'a' & @ReadData
			parse Field stream
			""";

		Assert.Contains(Unseen(grammar), static diagnostic => diagnostic.Id == "GRAM4026");

		var shown = Shown(Host(
			grammar,
			"",
			"""
			static bool ReadData(DotGram.ParserInput<char> input, ref int position)
			{
				return true;
			}
			"""));

		Assert.DoesNotContain(shown, static diagnostic => diagnostic.Id == "GRAM4026");
	}

	/// <summary>
	/// What a host names for its carrier is what the generator compiles with, and naming one
	/// silences what it says about the one it would have chosen.
	/// </summary>
	[Theory]
	[InlineData("",                                     true)]
	[InlineData(", Carrier = DotGram.GramCarrier.Tape", false)]
	public void The_carrier_the_host_names_is_the_one_analysed(string options, bool told)
	{
		const string grammar = """
			context : @object
			trivia = none
			Ows  = [' ']*
			Item : @string = t: ['a'..'z']+ => @(t)
			Rest : @string = Ows & ',' & Ows & i: Item => @(i)
			List : @string = first: Item & rest: Rest* & Ows => @(first + string.Concat(rest))
			Pair : @string = l: List & ',' & Ows & 'z' & '!' => @(l)
			parse Pair
			""";

		var shown = Shown(Host(grammar, options, ""));

		Assert.Equal(told, shown.Any(static diagnostic => diagnostic.Id == "GRAM5016"));
	}

	/// <summary>
	/// The build applies the C# file's <c>#pragma warning</c> to what the generator reports in it,
	/// and so does the editor.
	/// </summary>
	[Theory]
	[InlineData("#pragma warning disable GRAM4052", "#pragma warning restore GRAM4052", false)]
	[InlineData("#pragma warning disable",          "#pragma warning restore",          false)]
	[InlineData("#pragma warning disable GRAM4051", "#pragma warning restore GRAM4051", true)]
	[InlineData("",                                 "",                                 true)]
	public void A_pragma_around_the_attribute_silences_what_it_names(string before, string after, bool told)
	{
		const string grammar = """
			Run = ('a' | 'a' & 'a')+ & 'b'
			parse Run
			""";

		var shown = Shown(Host(grammar, "", "", before, after));

		Assert.Equal(told, shown.Any(static diagnostic => diagnostic.Id == "GRAM4052"));
	}

	public static TheoryData<string, string, GramSeverity> Configurations()
	{
		var data = new TheoryData<string, string, GramSeverity>();

		string[] pragmas =
		[
			"",
			"#pragma warning disable GRAM9999",
			"#pragma warning disable GRAM9999\n#pragma warning restore GRAM9999",
			"#pragma warning disable",
			"#pragma warning disable GRAM0000, GRAM9999",
			"#pragma warning disable GRAM0000",
		];
		string[] options = ["", "nowarn", "hidden", "error", "warn", "info", "general"];

		foreach (var pragma in pragmas)
			foreach (var option in options)
				foreach (var severity in new[] { GramSeverity.Info, GramSeverity.Warning, GramSeverity.Error })
					data.Add(pragma, option, severity);

		return data;
	}

	/// <summary>
	/// What the editor makes of a diagnostic in a C# file is what Roslyn makes of one reported
	/// there: asked of Roslyn itself, through an analyzer that reports it.
	/// </summary>
	[Theory]
	[MemberData(nameof(Configurations))]
	public async Task A_diagnostic_is_shown_as_Roslyn_reports_it(string pragma, string option, GramSeverity severity)
	{
		const string id = "GRAM9999";

		var source = pragma + "\nclass Host\n{\n\tconst string Text = \"here\";\n}\n";
		var tree   = CSharpSyntaxTree.ParseText(source, cancellationToken: TestContext.Current.CancellationToken);
		var place  = source.IndexOf("here", StringComparison.Ordinal);
		var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

		compilationOptions = option switch
		{
			"nowarn"  => compilationOptions.WithSpecificDiagnosticOptions([new(id, ReportDiagnostic.Suppress)]),
			"hidden"  => compilationOptions.WithSpecificDiagnosticOptions([new(id, ReportDiagnostic.Hidden)]),
			"error"   => compilationOptions.WithSpecificDiagnosticOptions([new(id, ReportDiagnostic.Error)]),
			"warn"    => compilationOptions.WithSpecificDiagnosticOptions([new(id, ReportDiagnostic.Warn)]),
			"info"    => compilationOptions.WithSpecificDiagnosticOptions([new(id, ReportDiagnostic.Info)]),
			"general" => compilationOptions.WithGeneralDiagnosticOption(ReportDiagnostic.Error),
			_         => compilationOptions,
		};

		var compilation = CSharpCompilation.Create(
			"Host", [tree], [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)], compilationOptions);
		var diagnostic  = new GramDiagnostic(id, "message", 0, 4, severity);
		var reported    = (await RoslynReport.Shown(
			compilation, [(diagnostic, tree, new TextSpan(place, 4))], TestContext.Current.CancellationToken))
			.Select(static one => (GramSeverity?)one.Severity)
			.SingleOrDefault();
		var shown       = GramBuildSeverity.Of(
			diagnostic,
			tree,
			place,
			compilationOptions,
			TestContext.Current.CancellationToken);

		Assert.Equal(reported, shown);
	}

	/// <summary>What the compiler reports with the editor's resolver, before the editor takes its noes as unknown.</summary>
	static IReadOnlyList<GramDiagnostic> Unseen(string grammar)
	{
		return GramCompiler.Compile(grammar, new GramCompilerOptions { CSharpScanner = RoslynCSharpScanner.Instance }).Diagnostics;
	}

	static string Host(string grammar, string options, string members, string before = "", string after = "")
	{
		return SupportEmitter.Attributes + SupportEmitter.EmbeddedAttribute + $$""""

			{{before}}
			[DotGram.Gram("""
			{{grammar}}
			"""{{options}})]
			{{after}}
			partial class Parser
			{
				{{members}}
			}
			"""";
	}

	static IReadOnlyList<GramDiagnostic> Shown(string source)
	{
		var tree        = CSharpSyntaxTree.ParseText(source, cancellationToken: TestContext.Current.CancellationToken);
		var compilation = CSharpCompilation.Create(
			"Host",
			[tree],
			[MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
		var analysis    = Assert.Single(EmbeddedGrammarService.Analyze(
			compilation.GetSemanticModel(tree), tree.GetRoot(TestContext.Current.CancellationToken), TestContext.Current.CancellationToken));

		return [.. analysis.Diagnostics.Select(static diagnostic => diagnostic.Diagnostic)];
	}
}
