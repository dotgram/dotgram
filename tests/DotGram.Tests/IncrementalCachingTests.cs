using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using DotGram.Generation;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// What an edit to the host costs: which stages the generator runs again, and which it reuses.
/// </summary>
/// <remarks>
/// <para>
/// A consumer pays this on every keystroke, so it is a property worth a test rather than a
/// measurement taken once. <c>GeneratorDriverTests</c> asks the easier question — an edit to a file
/// no grammar has anything to do with — and nothing asked what editing the host ITSELF cost until
/// this did.
/// </para>
/// <para>
/// <b>Read <c>Cached</c> and <c>Unchanged</c> as the different things they are.</b> `Cached` means
/// the step did not run and its previous output was reused. `Unchanged` means it RAN and its output
/// merely happened to equal the previous one — the work was done. Only the first is a saving, which
/// is why the assertions below name `Cached` and not "not Modified".
/// </para>
/// <para>
/// <b>What made this possible.</b> The pipeline values used to carry a <see cref="Location"/>, which
/// compares by tree and span; a tree is a new object after every edit, so the values were unequal to
/// themselves across two compilations of the same text and nothing downstream could be reused. They
/// carry a <c>Place</c> now — path, span, line span — and the <c>Location</c> is built at delivery,
/// where the trees are combined in and nothing is cached.
/// </para>
/// <para>
/// <b>And then where a host is written left the compile altogether</b> (D145). A grammar written
/// into an attribute has its C# under <c>#line</c>s pointing into the host file, so an edit that
/// moved the attribute changed the right output — and the place was part of what the compile was
/// given. The compile now reads what the host says and not where it says it; the <c>#line</c>s it
/// writes count from the literal, and the <c>Placed</c> step writes the literal's line and column in.
/// That step, a pass over the text, is what an edit above the attribute re-runs.
/// </para>
/// </remarks>
public sealed class IncrementalCachingTests
{
	/// <summary>
	/// Two hosts whose grammars are in the attribute, each with C# of its own under a <c>#line</c>.
	/// </summary>
	/// <remarks>
	/// Both kinds of line a <c>#line</c> can point at: the first is an ordinary string, so its C#
	/// is on the literal's first line and its column counts from where the literal begins; the
	/// second is a raw literal, whose every line of grammar is a line of the file below the one
	/// the literal begins on.
	/// </remarks>
	const string Inline = """"
		using DotGram;

		[Gram("Start : @int = d: ['0'..'9']+ => @(int.Parse(d))\nparse Start")]
		public partial class Grammar
		{
			static int Unrelated()
			{
				return 1;
			}
		}

		[Gram("""
			Start : @int = d: ['0'..'9']+
				=> @(int.Parse(d) + 1)
			parse Start
			""")]
		public partial class Raw
		{
		}
		"""";

	/// <summary>And one whose grammar is a file of its own, which takes a different line map.</summary>
	const string FromFile = """
		using DotGram;

		[Gram]
		public partial class FileGrammar
		{
		}
		""";

	static readonly (string Path, string Text) File = ("FileGrammar.gram", "Start = 'a'+\nparse Start");

	/// <summary>Lines above both attributes, which moves each of them and changes neither.</summary>
	static string Above(string source)
	{
		return "// Two lines above,\n// which move every attribute below them.\n" + source;
	}

	[Fact]
	public void A_file_grammar_is_not_compiled_again_for_the_same_text()
	{
		Assert.Equal(IncrementalStepRunReason.Cached, Reason(Run(FromFile, FromFile, File), GramGenerator.CompiledStage));
	}

	[Fact]
	public void A_file_grammar_is_not_compiled_again_for_an_edit_that_leaves_it_where_it_was()
	{
		// Below the class, so the attribute does not move.
		Assert.Equal(IncrementalStepRunReason.Cached, Reason(Run(FromFile, FromFile + "\n// trailing", File), GramGenerator.CompiledStage));
	}

	/// <summary>
	/// Nor for one that moves its attribute: where the attribute is reaches only the reporting,
	/// and a <c>.gram</c> grammar's <c>#line</c>s point into its own file.
	/// </summary>
	[Fact]
	public void A_file_grammar_is_not_compiled_again_for_an_edit_that_moves_it()
	{
		var run = Run(FromFile, Above(FromFile), File);

		Assert.Equal(IncrementalStepRunReason.Cached, Reason(run, GramGenerator.CompiledStage));
		Assert.NotEqual(IncrementalStepRunReason.Modified, Reason(run, GramGenerator.PlacedStage));
	}

	[Fact]
	public void An_attribute_grammar_is_not_compiled_again_for_the_same_text()
	{
		var run = Run(Inline, Inline);

		Assert.Equal(IncrementalStepRunReason.Cached, Reason(run, GramGenerator.AskedStage));
		Assert.Equal(IncrementalStepRunReason.Cached, Reason(run, GramGenerator.CompiledStage));
		Assert.Equal(IncrementalStepRunReason.Cached, Reason(run, GramGenerator.PlacedStage));
	}

	/// <summary>
	/// The case this is all for (D145): an edit above the attribute moves every <c>#line</c> the
	/// grammar's C# is under, and the compile is still reused. Only the step that writes the
	/// numbers in runs again, and what it writes is what a generation from scratch writes.
	/// </summary>
	[Fact]
	public void An_edit_above_an_attribute_grammar_places_it_again_without_compiling_it()
	{
		var run = Run(Inline, Above(Inline));

		Assert.Equal(IncrementalStepRunReason.Cached,   Reason(run, GramGenerator.AskedStage));
		Assert.Equal(IncrementalStepRunReason.Cached,   Reason(run, GramGenerator.CompiledStage));
		Assert.Equal(IncrementalStepRunReason.Modified, Reason(run, GramGenerator.PlacedStage));

		var before = Generated(Run(Inline, Inline));
		var after  = Generated(run);
		var fresh  = Generated(Run(Above(Inline), Above(Inline)));

		Assert.Equal(fresh, after);

		// And the numbers did move, by the two lines written above: the test would pass on a
		// generator that wrote none at all otherwise.
		foreach (var (name, text) in before)
		{
			var was = Lines(text);
			var now = Lines(after[name]);

			Assert.NotEmpty(was);
			Assert.Equal(was.Select(static line => line + 2), now);
		}
	}

	/// <summary>
	/// On the attribute's own line the column moves too, and the line of C# under a <c>#line</c>
	/// is padded out to it.
	/// </summary>
	[Fact]
	public void An_edit_before_an_attribute_on_its_own_line_moves_its_columns()
	{
		var edited = Inline.Replace("[Gram(\"Start", "   [Gram(\"Start", StringComparison.Ordinal);
		var run    = Run(Inline, edited);

		Assert.Equal(IncrementalStepRunReason.Cached,   Reason(run, GramGenerator.CompiledStage));
		Assert.Contains(Runs(run, GramGenerator.PlacedStage), static reason => reason == IncrementalStepRunReason.Modified);
		Assert.Equal(Generated(Run(edited, edited)), Generated(run));
	}

	[Theory]
	[InlineData("\n// trailing")]
	[InlineData("method")]
	public void An_edit_below_or_beside_an_attribute_grammar_runs_nothing_again(string edit)
	{
		var edited = edit == "method"
			? Inline.Replace("return 1;", "return 2;", StringComparison.Ordinal)
			: Inline + edit;
		var run    = Run(Inline, edited);

		Assert.Equal(IncrementalStepRunReason.Cached, Reason(run, GramGenerator.CompiledStage));
		Assert.All(
			Runs(run, GramGenerator.PlacedStage),
			static reason => Assert.True(reason is IncrementalStepRunReason.Cached or IncrementalStepRunReason.Unchanged, reason.ToString()));
	}

	/// <summary>
	/// The control: a real change to the grammar, or to how it is compiled, must NOT be cached, or
	/// the assertions above would pass just as well on a generator that cached everything and
	/// answered the same thing twice.
	/// </summary>
	[Theory]
	[InlineData("'0'..'9'", "'1'..'9'")]
	[InlineData("public partial class Grammar", "[GramOptions(Suffix = \"Twice\")]\npublic partial class Grammar")]
	[InlineData("parse Start\")]", "parse Start\", PartSize = 7)]")]
	public void A_changed_grammar_is_compiled_again(string was, string now)
	{
		var run = Run(Inline, Inline.Replace(was, now, StringComparison.Ordinal));

		// Ran, whatever it answered: a part size a grammar this small never reaches compiles to
		// the same text, and `Unchanged` is the compile run and found equal.
		Assert.Contains(
			Runs(run, GramGenerator.CompiledStage),
			static reason => reason != IncrementalStepRunReason.Cached);
	}

	/// <summary>
	/// What the reuse must not cost: a diagnostic made by a compile run before the edit lands where
	/// a compile of the edited file would put it, and so does a C# error under a <c>#line</c>.
	/// </summary>
	/// <remarks>
	/// The report is made by the compile and carries no place in the host file, only which
	/// attribute it is about and where in the grammar; it is placed at the reporting step, from the
	/// tree of the compilation being reported (D145). A <c>#pragma</c> suppression is keyed to that
	/// location, which is why it has to be the real one.
	/// </remarks>
	[Fact]
	public void After_an_edit_above_the_diagnostics_land_where_a_fresh_run_puts_them()
	{
		// A rule nothing reaches, which is a warning placed in the first literal, and a name C#
		// does not know, which is an error under the second's `#line`.
		var source = Inline
			.Replace("\\nparse Start\")]", "\\nUnused = 'z'\\nparse Start\")]", StringComparison.Ordinal)
			.Replace("int.Parse(d) + 1", "Missing(d)", StringComparison.Ordinal);

		var run   = Run(source, Above(source), out var output);
		var fresh = Run(Above(source), Above(source), out var freshOutput);

		Assert.Equal(IncrementalStepRunReason.Cached, Reason(run, GramGenerator.CompiledStage));

		var reported = Placed(run.Diagnostics);

		Assert.Contains(reported, static one => one.Id == "GRAM4018" && one.Line == 4);
		Assert.Equal(Placed(fresh.Diagnostics), reported);

		var errors = Placed(Errors(output));

		// On the line `Missing` is written on: the raw literal's second line of grammar, which is
		// the file's own line 15 (0-based) once two are written above it.
		Assert.Contains(errors, static one => one.Path == "Host.cs" && one.Line == 15);
		Assert.Equal(Placed(Errors(freshOutput)), errors);
	}

	// ── The driver ────────────────────────────────────────────────────────────────

	/// <summary>How a stage of the second run was reached, the same for every host or it fails.</summary>
	static IncrementalStepRunReason Reason(GeneratorDriverRunResult run, string stage)
	{
		return Runs(run, stage).Distinct().Single();
	}

	static ImmutableArray<IncrementalStepRunReason> Runs(GeneratorDriverRunResult run, string stage)
	{
		var outputs = run.Results
			.SelectMany(one => one.TrackedSteps.TryGetValue(stage, out var steps) ? steps : [])
			.SelectMany(step => step.Outputs)
			.Select(static output => output.Reason)
			.ToImmutableArray();

		// Before the assertion, because a stage renamed away would otherwise report nothing and
		// every test here would pass on an empty set.
		Assert.True(outputs.Length > 0, $"No '{stage}' step ran at all; has the stage been renamed?");

		return outputs;
	}

	/// <summary>The generated parsers of a run, by hint name.</summary>
	static Dictionary<string, string> Generated(GeneratorDriverRunResult run)
	{
		return run.Results
			.SelectMany(static one => one.GeneratedSources)
			.Where(static source => !source.HintName.StartsWith("DotGram.", StringComparison.Ordinal) &&
				!source.HintName.StartsWith("Microsoft.", StringComparison.Ordinal))
			.ToDictionary(static source => source.HintName, static source => source.SourceText.ToString(), StringComparer.Ordinal);
	}

	/// <summary>The line of every <c>#line</c> into the host, in order.</summary>
	static int[] Lines(string generated)
	{
		return
		[
			.. generated
				.Split('\n')
				.Where(static line => line.StartsWith("#line ", StringComparison.Ordinal) && line.Contains("\"Host.cs\"", StringComparison.Ordinal))
				.Select(static line => int.Parse(line.Split(' ')[1], System.Globalization.CultureInfo.InvariantCulture)),
		];
	}

	/// <summary>Where each diagnostic is, as a value two runs can be compared by.</summary>
	static (string Id, string Path, int Line, int Column, int Length)[] Placed(IEnumerable<Diagnostic> diagnostics)
	{
		return
		[
			.. diagnostics
				.Select(static one => (one, at: one.Location.GetMappedLineSpan()))
				.Select(static pair => (pair.one.Id, pair.at.Path, pair.at.StartLinePosition.Line, pair.at.StartLinePosition.Character, pair.one.Location.SourceSpan.Length))
				.OrderBy(static one => one.Line)
				.ThenBy(static one => one.Character)
				.ThenBy(static one => one.Id, StringComparer.Ordinal),
		];
	}

	/// <summary>The C# errors of the edited host compiled with what the second run generated.</summary>
	static IEnumerable<Diagnostic> Errors(Compilation output)
	{
		return output
			.GetDiagnostics(TestContext.Current.CancellationToken)
			.Where(static one => one.Severity == DiagnosticSeverity.Error);
	}

	static GeneratorDriverRunResult Run(string before, string after, (string Path, string Text)? file = null)
	{
		return Run(before, after, out _, file);
	}

	/// <summary>Runs the generator over <paramref name="before"/>, then again over <paramref name="after"/>.</summary>
	/// <param name="output">The edited host with what the second run generated.</param>
	static GeneratorDriverRunResult Run(string before, string after, out Compilation output, (string Path, string Text)? file = null)
	{
		var parse = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);
		var host  = CSharpSyntaxTree.ParseText(before, parse, "Host.cs");

		var compilation = CSharpCompilation.Create(
			"DotGram.Tests.Incremental",
			[host],
			EmittedCode.References,
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

		var driver = (GeneratorDriver)CSharpGeneratorDriver.Create(
			[new GramGenerator().AsSourceGenerator()],
			additionalTexts: file is { } one ? [new Additional(one.Path, one.Text)] : [],
			parseOptions: parse,
			driverOptions: new GeneratorDriverOptions(
				IncrementalGeneratorOutputKind.None,
				trackIncrementalGeneratorSteps: true));

		driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

		var edited = compilation.ReplaceSyntaxTree(host, CSharpSyntaxTree.ParseText(after, parse, "Host.cs"));

		return driver
			.RunGeneratorsAndUpdateCompilation(edited, out output, out _, TestContext.Current.CancellationToken)
			.GetRunResult();
	}

	sealed class Additional(string path, string text) : AdditionalText
	{
		public override string Path { get; } = path;

		public override SourceText GetText(CancellationToken cancellationToken = default)
		{
			return SourceText.From(text);
		}
	}
}
