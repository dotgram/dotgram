using System;
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
/// </remarks>
public sealed class IncrementalCachingTests
{
	/// <summary>A host whose grammar is in the attribute.</summary>
	const string Inline = """
		using DotGram;

		[Gram("Start = 'a'+\nparse Start")]
		public partial class Grammar
		{
		}
		""";

	/// <summary>And one whose grammar is a file of its own, which takes a different line map.</summary>
	const string FromFile = """
		using DotGram;

		[Gram]
		public partial class FileGrammar
		{
		}
		""";

	static readonly (string Path, string Text) File = ("FileGrammar.gram", "Start = 'a'+\nparse Start");

	[Fact]
	public void A_file_grammar_is_not_compiled_again_for_the_same_text()
	{
		Assert.Equal(IncrementalStepRunReason.Cached, Compiled(FromFile, FromFile, File));
	}

	[Fact]
	public void A_file_grammar_is_not_compiled_again_for_an_edit_that_leaves_it_where_it_was()
	{
		// Below the class, so the attribute does not move.
		Assert.Equal(IncrementalStepRunReason.Cached, Compiled(FromFile, FromFile + "\n// trailing", File));
	}

	/// <summary>
	/// A grammar written into the ATTRIBUTE is compiled again for any edit to its file, even one
	/// that changes nothing.
	/// </summary>
	/// <remarks>
	/// <b>This is the honest state and not an oversight.</b> <c>InlineLineMap</c> maps a position in
	/// such a grammar onto a line of the C# FILE, which needs that file's text, so the host carries
	/// its own <c>SyntaxTree</c> — and a tree is a new object after every edit. The tree is carried
	/// ONLY where the grammar is inline; a <c>.gram</c> file leaves it null, which is what the two
	/// tests above assert. Taking the mapping out of the compile is the second half of this work,
	/// and when it lands this is the test that changes.
	/// </remarks>
	[Fact]
	public void An_attribute_grammar_is_compiled_again_even_for_the_same_text()
	{
		Assert.NotEqual(IncrementalStepRunReason.Cached, Compiled(Inline, Inline));
	}

	/// <summary>
	/// And an edit that MOVES it still compiles again, which is the honest state of this.
	/// </summary>
	/// <remarks>
	/// The position is real data: the attribute is at a different offset, and a diagnostic pointing
	/// into the grammar has to land in the right place. What makes it cost a compile is that the
	/// position is part of what the compile is given, so a value that differs only in where it is
	/// makes the whole compile unequal. Taking the position out of the compile's input — the
	/// emitted text carrying grammar coordinates and being rewritten at the output step — is the
	/// second half of this work and is not done here. <b>This assertion exists so that half cannot
	/// be forgotten, and so this test is not mistaken for a finished job.</b>
	/// </remarks>
	[Fact]
	public void An_edit_that_moves_the_grammar_still_compiles_again()
	{
		var moved = Compiled(FromFile, FromFile.Replace("using DotGram;", "using DotGram;\n\n", StringComparison.Ordinal), File);

		Assert.NotEqual(IncrementalStepRunReason.Cached, moved);
	}

	/// <summary>
	/// The control: a real change to the grammar must NOT be cached, or the assertions above would
	/// pass just as well on a generator that cached everything and answered the same thing twice.
	/// </summary>
	[Fact]
	public void A_changed_grammar_is_compiled_again()
	{
		var changed = Compiled(Inline, Inline.Replace("'a'+", "'b'+", StringComparison.Ordinal));

		Assert.NotEqual(IncrementalStepRunReason.Cached, changed);
	}

	/// <summary>How the compile step of the second run was reached.</summary>
	static IncrementalStepRunReason Compiled(string before, string after, (string Path, string Text)? file = null)
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
		var run    = driver.RunGenerators(edited, TestContext.Current.CancellationToken).GetRunResult();

		var outputs = run.Results
			.SelectMany(one => one.TrackedSteps.TryGetValue(GramGenerator.CompiledStage, out var steps) ? steps : [])
			.SelectMany(step => step.Outputs)
			.ToImmutableArray();

		// Before the assertion, because a stage renamed away would otherwise report nothing and
		// every test here would pass on an empty set.
		Assert.True(outputs.Length > 0, "No compile step ran at all; has the stage been renamed?");

		return outputs.Select(one => one.Reason).Distinct().Single();
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
