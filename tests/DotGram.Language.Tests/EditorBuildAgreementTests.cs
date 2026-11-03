using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

		Assert.Contains(Permissive(grammar), static diagnostic => diagnostic.Id == "GRAM4008");

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

		Assert.Contains(Permissive(grammar), static diagnostic => diagnostic.Id == "GRAM4026");

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
	/// A type of C#'s own is built by no constructor of a grammar's captures, and needs no host to
	/// say so: the editor still tells a rule that declares one and does not build it.
	/// </summary>
	[Theory]
	[InlineData("@string")]
	[InlineData("@int")]
	[InlineData("@System.Int32")]
	[InlineData("@long")]
	public void A_rule_of_a_type_of_C_sharps_own_is_still_told_it_does_not_build_it(string type)
	{
		var grammar = $$"""
			trivia = none
			Row : {{type}} = a: Name & ',' & b: Name
			Name : @string = t: ['a'..'z']+ => @(t)
			parse Row
			""";

		Assert.Contains(Shown(Host(grammar, "", "")), static diagnostic => diagnostic.Id == "GRAM4008");
	}

	/// <summary>
	/// Only the recognizer's shape is left to the build: what else a buffered form refuses is the
	/// grammar's own, and the editor still tells it — here a literal the byte form cannot read.
	/// </summary>
	[Fact]
	public void A_buffered_form_over_a_recognizer_of_the_host_is_still_refused_for_the_rest()
	{
		const string grammar = """
			Field = 'a' & @ReadData & 'Ā'
			parse Field stream bytes
			""";

		var refused = Permissive(grammar.Replace(" & @ReadData", "")).Where(static diagnostic => diagnostic.Id == "GRAM4026").ToArray();
		var shown   = Shown(Host(grammar, "", "")).Where(static diagnostic => diagnostic.Id == "GRAM4026").ToArray();

		Assert.NotEmpty(refused);
		Assert.Equal(refused.Select(static diagnostic => diagnostic.Message), shown.Select(static diagnostic => diagnostic.Message));
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

	public static TheoryData<string> Pragmas()
	{
		return
		[
			"",
			"#pragma warning disable GRAM9999",
			"#pragma warning disable GRAM9999\n#pragma warning restore GRAM9999",
			"#pragma warning disable",
			"#pragma warning disable GRAM0000, GRAM9999",
			"#pragma warning disable GRAM0000",
			"#pragma warning disable GRAM9999\n#pragma warning restore",
		];
	}

	/// <summary>
	/// Every spelling of a <c>#pragma warning</c> before a diagnostic silences it where Roslyn's does,
	/// whatever its severity.
	/// </summary>
	[Theory]
	[MemberData(nameof(Pragmas))]
	public async Task A_pragma_is_read_as_Roslyn_reads_it(string pragma)
	{
		var source = pragma + "\nclass Host\n{\n\tconst string Text = \"here\";\n}\n";
		var tree   = CSharpSyntaxTree.ParseText(source, cancellationToken: TestContext.Current.CancellationToken);
		var place  = source.IndexOf("here", StringComparison.Ordinal);

		foreach (var severity in new[] { GramSeverity.Info, GramSeverity.Warning, GramSeverity.Error })
		{
			var diagnostic = new GramDiagnostic("GRAM9999", "message", 0, 4, severity);

			await Agrees(Compilation(tree, Options()), [(diagnostic, tree, new TextSpan(place, 4))]);
		}
	}

	/// <summary>
	/// What the editor makes of a diagnostic is what Roslyn makes of one reported at the same place,
	/// under every combination of what decides it: the project's warning level and general option,
	/// its option for the number (<c>WarningsNotAsErrors</c> included, which writes
	/// <see cref="ReportDiagnostic.Default"/>), the file's and the global configuration, a
	/// <c>#pragma</c>, and the severity the generator reports it at; in a C# file and in a grammar
	/// file of its own, which no <c>#pragma</c> or file's configuration reaches.
	/// </summary>
	/// <remarks>
	/// One compilation per project-wide setting, each diagnostic under a number of its own, so that
	/// every number's options can differ while Roslyn is asked once.
	/// </remarks>
	[Fact]
	public async Task A_diagnostic_is_shown_as_Roslyn_reports_it_under_every_configuration()
	{
		ReportDiagnostic?[] specifics = [null, ReportDiagnostic.Suppress, ReportDiagnostic.Default, ReportDiagnostic.Error,
			ReportDiagnostic.Warn, ReportDiagnostic.Info, ReportDiagnostic.Hidden];
		ReportDiagnostic?[] files     = [null, ReportDiagnostic.Error, ReportDiagnostic.Warn, ReportDiagnostic.Info, ReportDiagnostic.Suppress];
		ReportDiagnostic?[] globals   = [null, ReportDiagnostic.Error, ReportDiagnostic.Warn, ReportDiagnostic.Suppress];
		GramSeverity[]      severities = [GramSeverity.Info, GramSeverity.Warning, GramSeverity.Error];

		var cases = new List<(string Id, bool Pragma, bool InFile, ReportDiagnostic? Specific, ReportDiagnostic? File, ReportDiagnostic? Global, GramSeverity Severity)>();

		foreach (var inFile in new[] { true, false })
			foreach (var pragma in inFile ? new[] { false, true } : [false])
				foreach (var specific in specifics)
					foreach (var file in inFile ? files : [null])
						foreach (var global in globals)
							foreach (var severity in severities)
								cases.Add(($"GRAM{cases.Count:D4}", pragma, inFile, specific, file, global, severity));

		var disabled = cases.Where(static one => one.Pragma).Select(static one => one.Id).ToArray();
		var source   = $"#pragma warning disable {string.Join(", ", disabled)}\nclass Host\n{{\n\tconst string Text = \"here\";\n}}\n";
		var tree     = CSharpSyntaxTree.ParseText(source, cancellationToken: TestContext.Current.CancellationToken);
		var place    = source.IndexOf("here", StringComparison.Ordinal);
		var provider = new Configuration(
			tree,
			cases.Where(static one => one.File is not null).ToDictionary(static one => one.Id, static one => one.File!.Value),
			cases.Where(static one => one.Global is not null).ToDictionary(static one => one.Id, static one => one.Global!.Value));
		var numbered = cases.Where(static one => one.Specific is not null)
			.Select(static one => new KeyValuePair<string, ReportDiagnostic>(one.Id, one.Specific!.Value))
			.ToArray();
		var reported = cases
			.Select(one => (new GramDiagnostic(one.Id, "message", 0, 4, one.Severity), one.InFile ? tree : null, new TextSpan(place, 4)))
			.ToArray();

		foreach (var general in new[] { ReportDiagnostic.Default, ReportDiagnostic.Error, ReportDiagnostic.Suppress })
			foreach (var level in new[] { 4, 1, 0 })
				await Agrees(
					Compilation(tree, Options()
						.WithGeneralDiagnosticOption(general)
						.WithWarningLevel(level)
						.WithSpecificDiagnosticOptions(numbered)
						.WithSyntaxTreeOptionsProvider(provider)),
					reported,
					$"general {general}, warning level {level}");
	}

	/// <summary>
	/// A grammar that is a file of its own is shown its diagnostics as its project's options make
	/// them: what the generator reports in a <c>.gram</c> file is filtered like any other.
	/// </summary>
	[Theory]
	[InlineData("",            GramSeverity.Warning)]
	[InlineData("nowarn",      null)]
	[InlineData("warnaserror", GramSeverity.Error)]
	[InlineData("level0",      null)]
	public void A_grammar_file_is_shown_what_its_project_makes_of_a_diagnostic(string option, GramSeverity? expected)
	{
		const string grammar = """
			Run = ('a' | 'a' & 'a')+ & 'b'
			parse Run
			""";

		var build = option switch
		{
			"nowarn"      => Options().WithSpecificDiagnosticOptions([new("GRAM4052", ReportDiagnostic.Suppress)]),
			"warnaserror" => Options().WithGeneralDiagnosticOption(ReportDiagnostic.Error),
			"level0"      => Options().WithWarningLevel(0),
			_             => Options(),
		};
		var shown = GramLanguageService.Analyze(grammar, new GramAnalysisOptions { Build = build }).Diagnostics
			.Where(static diagnostic => diagnostic.Id == "GRAM4052")
			.Select(static diagnostic => (GramSeverity?)diagnostic.Severity)
			.SingleOrDefault();

		Assert.Equal(expected, shown);
	}

	/// <summary>That the editor shows each of these at the severity Roslyn shows it at, or not at all where Roslyn does not.</summary>
	static async Task Agrees(
		CSharpCompilation compilation,
		IReadOnlyList<(GramDiagnostic Diagnostic, SyntaxTree? Tree, TextSpan Span)> reported,
		string setting = "")
	{
		var built = (await RoslynReport.Shown(compilation, reported, TestContext.Current.CancellationToken))
			.ToDictionary(static one => one.Id, static one => one.Severity);
		var builds = new Dictionary<SyntaxTree, GramBuildSeverity>();
		var wrong  = new List<string>();

		foreach (var (diagnostic, tree, span) in reported)
		{
			var build = tree is null
				? new GramBuildSeverity(null, compilation.Options)
				: builds.TryGetValue(tree, out var known) ? known : builds[tree] = new GramBuildSeverity(tree, compilation.Options);
			var shown = build.Of(diagnostic, span.Start, TestContext.Current.CancellationToken);
			var roslyn = built.TryGetValue(diagnostic.Id, out var severity) ? severity : (GramSeverity?)null;

			if (shown != roslyn)
				wrong.Add($"{diagnostic.Id} ({diagnostic.Severity}{(tree is null ? ", grammar file" : "")}): Roslyn {roslyn?.ToString() ?? "hides it"}, the editor {shown?.ToString() ?? "hides it"}");
		}

		Assert.True(wrong.Count == 0, setting + "\n" + string.Join("\n", wrong.Take(30)));
	}

	static CSharpCompilationOptions Options()
	{
		return new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
	}

	static CSharpCompilation Compilation(SyntaxTree tree, CSharpCompilationOptions options)
	{
		return CSharpCompilation.Create("Host", [tree], [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)], options);
	}

	/// <summary>An <c>.editorconfig</c> for one file and a global analyzer configuration, as Roslyn is handed them.</summary>
	sealed class Configuration(
		SyntaxTree tree,
		IReadOnlyDictionary<string, ReportDiagnostic> file,
		IReadOnlyDictionary<string, ReportDiagnostic> global) : SyntaxTreeOptionsProvider
	{
		public override GeneratedKind IsGenerated(SyntaxTree syntaxTree, CancellationToken cancellationToken)
		{
			return GeneratedKind.NotGenerated;
		}

		public override bool TryGetDiagnosticValue(
			SyntaxTree syntaxTree, string diagnosticId, CancellationToken cancellationToken, out ReportDiagnostic severity)
		{
			severity = default;

			return syntaxTree == tree && file.TryGetValue(diagnosticId, out severity);
		}

		public override bool TryGetGlobalDiagnosticValue(string diagnosticId, CancellationToken cancellationToken, out ReportDiagnostic severity)
		{
			return global.TryGetValue(diagnosticId, out severity);
		}
	}

	/// <summary>What the compiler reports with the permissive resolver, whose noes it takes as facts.</summary>
	static IReadOnlyList<GramDiagnostic> Permissive(string grammar)
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
