using System;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Model;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Trivia that is not atomic, where a class, <c>any</c> or a negated class beside one of its
/// seams reads a character trivia reads, is told (GRAM4051): a run of such characters can be
/// split between the two in as many ways as it is long, and a refused input is read again
/// for every split at every seam.
/// </summary>
/// <remarks>
/// The CSV shape, <c>trivia = ' '*</c> beside <c>Field = [^ ',' | '\n']+</c>, is the one that
/// was met: a few refused rows took seconds and six over a minute. Braces around trivia
/// commit what it read and silence it; what does not overlap is not told.
/// </remarks>
public sealed class TriviaOverlapTests
{
	const string Csv =
		"Field = [^ ',' | '\\n']+\n" +
		"Row   = Field & (',' & Field)* & '\\n'\n" +
		"File  = Row* & eof\n" +
		"parse File\n";

	[Fact]
	public void The_csv_shape_is_told_at_the_trivia_it_names()
	{
		var grammar = "trivia = ' '*\n" + Csv;
		var told    = Assert.Single(Told(grammar));

		Assert.Equal(GramSeverity.Warning, told.Severity);
		Assert.Equal(0, told.Position);
		Assert.Contains("'trivia' is not atomic and reads ' '", told.Message, StringComparison.Ordinal);
		Assert.Contains("[^ '\\n' | ','] in 'Field'", told.Message, StringComparison.Ordinal);
		Assert.Contains("seam in 'Row'", told.Message, StringComparison.Ordinal);
		Assert.Contains("'trivia = { … }'", told.Message, StringComparison.Ordinal);
	}

	[Fact]
	public void Braces_silence_it()
	{
		Assert.Empty(Told("trivia = { ' '* }\n" + Csv));
	}

	[Fact]
	public void Braces_on_the_rule_trivia_forwards_to_silence_it()
	{
		Assert.Empty(Told("trivia = Blank\nBlank = { ' '* }\n" + Csv));
	}

	[Fact]
	public void A_rule_trivia_forwards_to_without_braces_is_told()
	{
		var grammar = "trivia = Blank\nBlank = ' '*\n" + Csv;
		var told    = Assert.Single(Told(grammar));

		Assert.Equal(0, told.Position);
		Assert.Contains("'Field'", told.Message, StringComparison.Ordinal);
	}

	[Theory]

	// A class at the end of the operand before a seam, and at the start of the one after it.
	[InlineData("trivia = ' '*", "Key = ['a'..'z' | ' ']+\nStart = Key & '=' & 'x' & eof")]
	[InlineData("trivia = ' '*", "Key = ['a'..'z' | ' ']+\nStart = 'x' & '=' & Key & eof")]

	// `any` reads every character.
	[InlineData("trivia = ' '*", "Start = '\"' & any & '\"' & eof")]

	// What trivia begins with is what counts: a comment's opening character.
	[InlineData("trivia = (' ' | '#' & [^ '\\n']* & '\\n')*", "Word = ['a'..'z' | '#']+\nStart = Word & (',' & Word)* & eof")]

	// The end of one turn of a spaced repetition stands beside the start of the next.
	[InlineData("trivia = '\\t'*", "Cell = ['a'..'z'] & ['a'..'z' | '\\t']*\nStart = 'x' & (';' & Cell)* & eof")]

	// A category: Zs holds the space.
	[InlineData("trivia = ' '*", "Start = [\\p{Zs}] & 'x' & eof")]
	public void An_overlapping_neighbour_is_told(string trivia, string rules)
	{
		Assert.Single(Told(trivia + "\n" + rules + "\nparse Start\n"));
	}

	[Theory]

	// Nothing beside a seam reads a space.
	[InlineData("trivia = ' '*", "Word = ['a'..'z']+\nStart = Word & (',' & Word)* & eof")]
	[InlineData("trivia = Std.Whitespace*", "Word = ['a'..'z']+\nStart = Word & (',' & Word)* & eof")]

	// A negated class that leaves the space out.
	[InlineData("trivia = ' '*", "Field = [^ ',' | ' ' | '\\n']+\nStart = Field & (',' & Field)* & eof")]

	// Joined with `~`, the class has no seam beside it.
	[InlineData("trivia = ' '*", "Start = '\"' ~ [^ '\"']* ~ '\"' & eof")]

	// A literal beside a seam is text the author wrote at that place.
	[InlineData("trivia = ' '*", "Start = 'a' & ' ' & 'b' & eof")]

	// A comment's interior is not where trivia begins.
	[InlineData("trivia = (' ' | '#' & [^ '\\n']* & '\\n')*", "Word = ['a'..'z']+\nStart = Word & (',' & Word)* & eof")]

	// A class inside a lexical namespace has no seam beside it, and the call to its rule is
	// one operand whose edges read letters.
	[InlineData("trivia = ' '*", "namespace Lexical { trivia = none\n Name = ['a'..'z'] & [^ ',']* & ['a'..'z'] }\nStart = Lexical.Name & (',' & Lexical.Name)* & eof")]

	// No trivia at all.
	[InlineData("trivia = none", "Field = [^ ',' | '\\n']+\nStart = Field & (',' & Field)* & eof")]
	public void What_cannot_be_split_is_not_told(string trivia, string rules)
	{
		Assert.Empty(Told(trivia + "\n" + rules + "\nparse Start\n"));
	}

	[Fact]
	public void One_trivia_is_told_once_however_many_namespaces_see_it()
	{
		var told = Told(
			"trivia = ' '*\n" +
			"namespace A { X = [^ ',']+\n Start = X & ',' & X }\n" +
			"namespace B { Y = [^ ';']+\n Start = Y & ';' & Y }\n" +
			"Start = A.Start & B.Start & eof\n" +
			"parse Start\n");

		Assert.Single(told);
	}

	/// <summary>The path a build takes: the generator over a host file, its warning reported.</summary>
	[Fact]
	public void The_generator_reports_it_in_a_build()
	{
		var parse  = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);
		var source =
			"[DotGram.Gram(\"trivia = ' '*\\nField = [^ ',' | '\\\\n']+\\nRow = Field & (',' & Field)* & '\\\\n'\\n" +
			"File = Row* & eof\\nparse File as Read\")] public partial class Parser;";

		var compilation = CSharpCompilation.Create(
			"DotGram.Tests.TriviaOverlap",
			[CSharpSyntaxTree.ParseText(source, parse, cancellationToken: TestContext.Current.CancellationToken)],
			EmittedCode.References,
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

		var run = CSharpGeneratorDriver
			.Create([new GramGenerator().AsSourceGenerator()], parseOptions: parse)
			.RunGenerators(compilation, TestContext.Current.CancellationToken)
			.GetRunResult();

		var told = Assert.Single(run.Diagnostics, static one => one.Id == TriviaOverlap.Overlapping);

		Assert.Equal(DiagnosticSeverity.Warning, told.Severity);
	}

	static GramDiagnostic[] Told(string grammar)
	{
		var all = GramCompiler.Compile(
				grammar,
				new GramCompilerOptions { ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance })
			.Diagnostics;

		Assert.DoesNotContain(all, static one => one.Severity == GramSeverity.Error);

		return [.. all.Where(static one => one.Id == TriviaOverlap.Overlapping)];
	}
}
