using System;
using System.Linq;

using Microsoft.CodeAnalysis;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// What a <c>[GramClassify]</c> names is held to the grammar by the build too, with the binder the
/// editor uses (GRAM6001–GRAM6005), and each is underlined inside the attribute's target.
/// </summary>
public sealed class ClassificationTargetTests
{
	const string Grammar = "namespace One { Value = 'one' }\\nnamespace Two { Value = 'two' }\\nStart = name: Name\\nName = ['a'..'z']+\\nparse Start";

	[Theory]
	[InlineData("Start", null)]
	[InlineData("Start.name", null)]
	[InlineData("Start.", "GRAM6001")]
	[InlineData("Nowhere", "GRAM6002")]
	[InlineData("Value", "GRAM6003")]
	[InlineData("Start.missing", "GRAM6004")]
	public void A_target_the_grammar_does_not_have_is_told_where_it_is_written(string target, string? id)
	{
		var source = "[DotGram.Gram(\"" + Grammar + "\")]\n" +
			"[DotGram.GramLanguage(\"com.example.test\")]\n" +
			"[DotGram.GramClassify(\"" + target + "\", DotGram.GramClassification.Keyword)]\n" +
			"public partial class Parser;";
		var run    = GeneratorDriverTests.RunGenerator(source);
		var told   = run.Diagnostics.Where(static one => one.Id.StartsWith("GRAM600", StringComparison.Ordinal)).ToArray();

		if (id is null)
		{
			Assert.Empty(told);

			return;
		}

		var one = Assert.Single(told);

		Assert.Equal(id, one.Id);
		Assert.Equal(DiagnosticSeverity.Warning, one.Severity);
		Assert.Equal(source.IndexOf("\"" + target + "\"", StringComparison.Ordinal) + 1, one.Location.SourceSpan.Start);
		Assert.Equal(target.Length, one.Location.SourceSpan.Length);
	}

	/// <summary>A target written twice is told the second time, and only that once.</summary>
	[Fact]
	public void A_target_named_twice_is_told_once()
	{
		var run = GeneratorDriverTests.RunGenerator(
			"[DotGram.Gram(\"" + Grammar + "\")]\n" +
			"[DotGram.GramLanguage(\"com.example.test\")]\n" +
			"[DotGram.GramClassify(\"Name\", DotGram.GramClassification.Keyword)]\n" +
			"[DotGram.GramClassify(\"Name\", DotGram.GramClassification.Identifier)]\n" +
			"public partial class Parser;");

		Assert.Equal("GRAM6005", Assert.Single(run.Diagnostics, static one => one.Id.StartsWith("GRAM600", StringComparison.Ordinal)).Id);
	}

	/// <summary>A second reading of the class does not say it again: the classification is the language's.</summary>
	[Fact]
	public void A_class_with_two_readings_is_told_once()
	{
		var run = GeneratorDriverTests.RunGenerator(
			"[DotGram.Gram(\"" + Grammar + "\")]\n" +
			"[DotGram.GramVariant(Name = \"Second\", Carrier = DotGram.GramCarrier.Deferred)]\n" +
			"[DotGram.GramLanguage(\"com.example.test\")]\n" +
			"[DotGram.GramClassify(\"Nowhere\", DotGram.GramClassification.Keyword)]\n" +
			"public partial class Parser;");

		Assert.Single(run.Diagnostics, static one => one.Id == "GRAM6002");
	}
}
