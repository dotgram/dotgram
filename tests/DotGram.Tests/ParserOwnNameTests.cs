using System;
using System.Linq;

using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Beside the names it supplies, the generated parser gives parameters of its own to the methods a
/// <c>=&gt;</c> and a <c>when</c> are written into: the tokens of a split grammar, the reading and the
/// place a located value began, the whole input a reader probes. A capture or a parameter of one of
/// those names would be declared beside the generated one, and C# that named one would read the
/// parser's workings; each is refused by name (docs/syntax.md §8.2).
/// </summary>
public sealed class ParserOwnNameTests
{
	static GramDiagnostic[] Told(string grammar)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions { CSharpScanner = RoslynCSharpScanner.Instance });

		return [.. result.Diagnostics.Where(static one => one.Severity == GramSeverity.Error)];
	}

	[Fact]
	public void The_list_is_every_parameter_the_generated_code_names_so()
	{
		Assert.Equal(
			["parserSource", "parserStarts", "parserLengths", "parserReading", "parserLocating", "parserWhole"],
			SuppliedNames.Internal);
		Assert.Empty(SuppliedNames.Internal.Intersect(SuppliedNames.All));
	}

	[Theory]
	[InlineData("parserSource")]
	[InlineData("parserStarts")]
	[InlineData("parserLengths")]
	[InlineData("parserReading")]
	[InlineData("parserLocating")]
	[InlineData("parserWhole")]
	[InlineData("parserText")]
	[InlineData("parserMessage")]
	public void A_capture_may_not_take_one(string name)
	{
		var told = Assert.Single(Told($"Start = {name}: ['a'..'z']+ & eof\nparse Start"));

		Assert.Equal(GrammarNormalizer.ReservedCaptureName, told.Id);
		Assert.Contains($"'{name}'", told.Message, StringComparison.Ordinal);
		Assert.Contains("a capture may not take it", told.Message, StringComparison.Ordinal);
	}

	[Theory]
	[InlineData("parserSource")]
	[InlineData("parserStarts")]
	[InlineData("parserLengths")]
	[InlineData("parserReading")]
	[InlineData("parserLocating")]
	[InlineData("parserWhole")]
	[InlineData("parserText")]
	[InlineData("parserSpan")]
	public void Nor_a_parameter_whether_it_holds_a_value_or_a_recognizer(string name)
	{
		const string Values      = "Start : @string = x: R(2) => @(x)\nR(NAME: @int) : string = ['a'..'z']{NAME}\nparse Start";
		const string Recognizers = "Start = R('a') & R('b')\nR(NAME) = NAME & ';'\nparse Start";

		foreach (var grammar in new[] { Values, Recognizers }.Select(one => one.Replace("NAME", name, StringComparison.Ordinal)))
		{
			// Once, where the parameter is written, however many specializations the rule has.
			var told = Assert.Single(Told(grammar));

			Assert.Equal(GrammarNormalizer.ReservedCaptureName, told.Id);
			Assert.Equal(name, grammar.Substring(told.Position, name.Length));
			Assert.Contains("a parameter may not take it", told.Message, StringComparison.Ordinal);
		}
	}

	[Theory]
	[InlineData("parserSource",   "Start : @string = t: ['a'..'z']+ => @(t + NAME)")]
	[InlineData("parserStarts",   "Start : @string = t: ['a'..'z']+ => @(t + NAME.Length)")]
	[InlineData("parserLengths",  "Start : @string = t: ['a'..'z']+ & when @(NAME != null) => @(t)")]
	[InlineData("parserReading",  "Start : @string = t: ['a'..'z']+ & when @(NAME == 0) => @(t)")]
	[InlineData("parserLocating", "Start : @string = t: ['a'..'z']+ => @(t + NAME)")]
	[InlineData("parserWhole",    "Start : @string[] = (t: ['a'..'z']+ & ';' => @(t))* recover ';' => @(NAME.ToString())")]
	public void And_the_csharp_of_a_grammar_is_not_handed_one(string name, string grammar)
	{
		var told = Assert.Single(Told(grammar.Replace("NAME", name, StringComparison.Ordinal) + "\nparse Start"));

		Assert.Equal(GrammarNormalizer.SuppliedNameNotHere, told.Id);
		Assert.Contains($"'{name}'", told.Message, StringComparison.Ordinal);
		Assert.Contains("keeps for itself", told.Message, StringComparison.Ordinal);
	}

	[Theory]
	[InlineData("parser")]
	[InlineData("parsers")]
	[InlineData("parserTextual")]
	[InlineData("parserwhole")]
	[InlineData("source")]
	[InlineData("reading")]
	public void While_a_name_that_only_looks_like_one_is_the_authors(string name)
	{
		var grammar =
			$"Start : @string = {name}: ['a'..'z']+ & x: R(2) => @({name} + x)\n" +
			$"R({name}: @int) : string = ['a'..'z']{{{name}}}\nparse Start";

		Assert.Empty(Told(grammar));
	}
}
