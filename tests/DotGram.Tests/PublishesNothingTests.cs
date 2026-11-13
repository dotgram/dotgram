using System;
using System.IO;
using System.Linq;

using DotGram.Generation;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A grammar that publishes nothing gives its class no method to call, and is only of use included:
/// where nothing in the compilation includes it and it does not say it is a library grammar
/// (<c>ExportGrammar = true</c>), the build warns (GRAM4059).
/// </summary>
public sealed class PublishesNothingTests
{
	/// <summary>A host that forgot its <c>parse</c> is told so, public or internal, once, as a warning.</summary>
	[Theory]
	[InlineData("public")]
	[InlineData("internal")]
	public void A_grammar_that_publishes_nothing_and_is_included_by_nothing_is_told(string visibility)
	{
		var warned = Warnings($$"""
			using DotGram;

			[Gram("A = 'a'")]
			{{visibility}} static partial class Forgot { }
			""");

		var warning = Assert.Single(warned);

		Assert.Equal(DiagnosticSeverity.Warning, warning.Severity);
		Assert.Contains("'Forgot'", warning.GetMessage(), StringComparison.Ordinal);
		Assert.Contains("ExportGrammar = true", warning.GetMessage(), StringComparison.Ordinal);
	}

	/// <summary>A class of a namespace, nested in another, is named in full; a variant beside it adds no second warning.</summary>
	[Fact]
	public void A_class_is_told_once_whatever_its_readings()
	{
		var warned = Warnings("""
			using DotGram;

			namespace Outer
			{
				public static partial class Holder
				{
					[Gram("A = 'a'")]
					[GramParserVariant("Fast", Lexical = true)]
					public static partial class Forgot { }
				}
			}
			""");

		Assert.Contains("'Outer.Holder.Forgot'", Assert.Single(warned).GetMessage(), StringComparison.Ordinal);
	}

	/// <summary>What includes a grammar, derives from its class or says it is a library's, and every publication, keeps it quiet.</summary>
	[Theory]
	[InlineData("""
		[Gram("A = 'a'")]
		public static partial class Lib { }

		[GramInclude(typeof(Lib))]
		[Gram("S = Lib.A\nparse S")]
		public static partial class P { }
		""")]
	[InlineData("""
		[Gram("A = 'a'")]
		internal static partial class Lib { }

		[Gram("B = 'b'")]
		[GramInclude(typeof(Lib))]
		internal static partial class Middle { }

		[GramInclude(typeof(Middle))]
		[Gram("S = Middle.B\nparse S")]
		internal static partial class P { }
		""")]
	[InlineData("""
		[Gram("Word = ['a'..'z']+")]
		public partial class Lexemes { }

		[Gram("using Lexemes;\nStart = Word\nparse Start")]
		public partial class Reader : Lexemes { }
		""")]
	[InlineData("""
		[Gram("A = 'a'", ExportGrammar = true)]
		internal static partial class Lib { }
		""")]
	[InlineData("""
		[Gram("A = 'a'", ExportGrammar = true)]
		[GramParserVariant("Fast", Lexical = true)]
		public static partial class Lib { }
		""")]
	[InlineData("""
		[Gram("A = 'a'\nfind A")]
		public static partial class Finds { }
		""")]
	[InlineData("""
		[Gram("A = 'a'\nprivate parse A")]
		public static partial class Private { }
		""")]
	[InlineData("""
		[Gram("A = 'a'\ninternal parse A")]
		public static partial class Internal { }
		""")]
	[InlineData("""
		[Gram("E : @string = x: 'a' => @(x)\nEs : @string[] = E*\nparse Es yield : @string")]
		public static partial class Yields { }
		""")]
	[InlineData("""
		[Gram("A = 'a'\nparse A")]
		[GramParserVariant("Fast", Lexical = true)]
		public static partial class Published { }
		""")]
	[InlineData("""
		[Gram("namespace N { A = 'a'\nparse A }")]
		public static partial class InANamespace { }
		""")]
	[InlineData("""
		[Gram("A = 'a'")]
		public partial class Lib<T> { }

		[Gram("using Lib;\nS = A\nparse S")]
		public partial class P : Lib<int> { }
		""")]
	[InlineData("""
		[Gram("A = 'a'")]
		public partial class Lib<T> { }

		[GramInclude(typeof(Lib<int>))]
		[Gram("S = Lib.A\nparse S")]
		public static partial class P { }
		""")]
	[InlineData("""
		public partial class Outer<T>
		{
			[Gram("A = 'a'")]
			public partial class Lib { }
		}

		[GramInclude(typeof(Outer<int>.Lib))]
		[Gram("S = Lib.A\nparse S")]
		public static partial class P { }
		""")]
	[InlineData("""
		[Gram("A = 'a'", Name = "Fast")]
		public partial class Lib { }

		[Gram("B = 'b'\nparse B")]
		public partial class P : Lib { }
		""")]
	public void A_grammar_that_is_of_use_is_not_told(string hosts)
	{
		Assert.Empty(Warnings("using DotGram;\n\n" + hosts));
	}

	/// <summary>A grammar with an error is told that, and not this as well; nor is <c>ExportGrammar = false</c> a library's mark.</summary>
	[Fact]
	public void Only_a_grammar_nothing_else_is_wrong_with_is_told()
	{
		var run = GeneratorDriverTests.RunGenerator("""
			using DotGram;

			[Gram("A = Missing")]
			public static partial class Broken { }

			[Gram("A = 'a'", ExportGrammar = false)]
			public static partial class Kept { }
			""");

		Assert.Contains(run.Diagnostics, static one => one.Id == "GRAM3002");
		Assert.Contains("'Kept'", Assert.Single(run.Diagnostics, static one => one.Id == "GRAM4059").GetMessage(), StringComparison.Ordinal);
	}

	/// <summary>
	/// A class of a referenced assembly is not this compilation's class of the same name: what derives
	/// from the one says nothing of the other, which is told.
	/// </summary>
	[Fact]
	public void A_referenced_class_of_the_same_name_is_another_class()
	{
		var reported = Reported(
			"""
			using DotGram;

			namespace N
			{
				[Gram("A = 'a'")]
				public partial class Lib { }
			}

			[Gram("S = 's'\nparse S")]
			public partial class P : External.Bridge { }
			""");

		Assert.Contains("'N.Lib'", Assert.Single(reported, static one => one.Id == "GRAM4059").GetMessage(), StringComparison.Ordinal);
	}

	/// <summary>
	/// A host the generator reports an error on — an option under the name it had before 0.3.0 — is
	/// told that (GRAM0012), and not also that its grammar publishes nothing; another host beside it
	/// still is.
	/// </summary>
	[Fact]
	public void A_host_with_an_error_on_its_attributes_is_not_told_as_well()
	{
		var run = GeneratorDriverTests.RunGenerator("""
			using DotGram;

			[Gram("Letter = ['a'..'z']", IncludedAs = "Lex")]
			public static partial class Lexemes { }

			[Gram("A = 'a'")]
			public static partial class Forgot { }
			""");

		Assert.Contains(run.Diagnostics, static one => one.Id == "GRAM0012");
		Assert.Contains("'Forgot'", Assert.Single(run.Diagnostics, static one => one.Id == "GRAM4059").GetMessage(), StringComparison.Ordinal);
	}

	/// <summary>The same where the old name is written on another declaration of the partial class.</summary>
	[Fact]
	public void An_error_on_another_declaration_of_the_host_holds_it_back_too()
	{
		var run = GeneratorDriverTests.RunGenerator("""
			using DotGram;

			[Gram("B = 'b'", ExportGrammar = true)]
			public static partial class Other { }

			[Gram("Letter = ['a'..'z']")]
			public static partial class Lexemes { }

			[GramInclude(typeof(Other), As = "Old")]
			public static partial class Lexemes { }
			""");

		Assert.Contains(run.Diagnostics, static one => one.Id == "GRAM0012");
		Assert.DoesNotContain(run.Diagnostics, static one => one.Id == "GRAM4059");
	}

	/// <summary>An error of any reading of the host — a variant's own option — holds back what the first is told.</summary>
	[Fact]
	public void An_error_in_a_variant_holds_it_back()
	{
		var run = GeneratorDriverTests.RunGenerator("""
			using DotGram;

			[Gram("A = 'a'")]
			[GramParserVariant("Bad", InitialBufferSize = 0)]
			public static partial class Lib { }
			""");

		Assert.Contains(run.Diagnostics, static one => one.Id == "GRAM0009");
		Assert.DoesNotContain(run.Diagnostics, static one => one.Id == "GRAM4059");
	}

	/// <summary>
	/// An error is the host's it is written on, and not another's at the same place of another file of
	/// the same path: two files with no path at all, the default, each with a host at the same offsets.
	/// </summary>
	[Fact]
	public void An_error_is_the_hosts_and_not_one_at_the_same_place_in_another_file()
	{
		var run = Run(
			("", "using DotGram;\n[Gram(\"A = 'a'\", IncludedAs = \"X\")]\npublic static partial class One { }\n"),
			("", "using DotGram;\n[Gram(\"A = 'a'\", DefaultAlias = \"X\")]\npublic static partial class Two { }\n"));

		Assert.Contains(run.Diagnostics, static one => one.Id == "GRAM0012");
		Assert.Contains("'Two'", Assert.Single(run.Diagnostics, static one => one.Id == "GRAM4059").GetMessage(), StringComparison.Ordinal);
	}

	/// <summary>An old name on a declaration of the partial class in another file holds it back as well.</summary>
	[Fact]
	public void An_error_on_a_declaration_in_another_file_holds_it_back()
	{
		var run = Run(
			("Lexemes.cs", "using DotGram;\n[Gram(\"Letter = ['a'..'z']\")]\npublic static partial class Lexemes { }\n[Gram(\"B = 'b'\", ExportGrammar = true)]\npublic static partial class Other { }\n"),
			("Lexemes.Include.cs", "using DotGram;\n[GramInclude(typeof(Other), As = \"Old\")]\npublic static partial class Lexemes { }\n"));

		Assert.Contains(run.Diagnostics, static one => one.Id == "GRAM0012");
		Assert.DoesNotContain(run.Diagnostics, static one => one.Id == "GRAM4059");
	}

	static GeneratorDriverRunResult Run(params (string Path, string Text)[] files)
	{
		var parse       = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);
		var compilation = CSharpCompilation.Create(
			"Files",
			[.. files.Select(file => CSharpSyntaxTree.ParseText(file.Text, parse, file.Path, cancellationToken: TestContext.Current.CancellationToken))],
			EmittedCode.References,
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

		return CSharpGeneratorDriver
			.Create([new GramGenerator().AsSourceGenerator()], parseOptions: parse)
			.RunGenerators(compilation, TestContext.Current.CancellationToken)
			.GetRunResult();
	}

	/// <summary>
	/// A warning of the generator's on the host's attributes is no reason to hold this back: it is said
	/// beside it.
	/// </summary>
	[Fact]
	public void A_warning_beside_it_holds_nothing_back()
	{
		var run = GeneratorDriverTests.RunGenerator("""
			using DotGram;

			[Gram("A = 'a'")]
			[GramLanguage("com.example.test")]
			[GramClassify("Nowhere", GramClassification.Keyword)]
			public static partial class Forgot { }
			""");

		Assert.Contains(run.Diagnostics, static one => one.Id == "GRAM6002" && one.Severity == DiagnosticSeverity.Warning);
		Assert.Single(run.Diagnostics, static one => one.Id == "GRAM4059");
	}

	/// <summary>Once the old name is put right, the warning the error held back is said.</summary>
	[Fact]
	public void The_warning_comes_back_once_the_error_is_fixed()
	{
		var parse       = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);
		var host        = CSharpSyntaxTree.ParseText("using DotGram;\n\n[Gram(\"Letter = ['a'..'z']\", IncludedAs = \"Lex\")]\npublic static partial class Lexemes { }\n", parse, "Lexemes.cs", cancellationToken: TestContext.Current.CancellationToken);
		var compilation = CSharpCompilation.Create("Fixed", [host], EmittedCode.References, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
		var driver      = (GeneratorDriver)CSharpGeneratorDriver.Create([new GramGenerator().AsSourceGenerator()], parseOptions: parse);

		driver = driver.RunGenerators(compilation, TestContext.Current.CancellationToken);

		Assert.Contains(driver.GetRunResult().Diagnostics, static one => one.Id == "GRAM0012");
		Assert.DoesNotContain(driver.GetRunResult().Diagnostics, static one => one.Id == "GRAM4059");

		var fixedRun = driver
			.RunGenerators(
				compilation.ReplaceSyntaxTree(
					host,
					CSharpSyntaxTree.ParseText("using DotGram;\n\n[Gram(\"Letter = ['a'..'z']\", DefaultAlias = \"Lex\")]\npublic static partial class Lexemes { }\n", parse, "Lexemes.cs", cancellationToken: TestContext.Current.CancellationToken)),
				TestContext.Current.CancellationToken)
			.GetRunResult();

		Assert.DoesNotContain(fixedRun.Diagnostics, static one => one.Id == "GRAM0012");
		Assert.Single(fixedRun.Diagnostics, static one => one.Id == "GRAM4059");
	}

	/// <summary>
	/// The same names the other way round: a referenced class reached first does not stand for this
	/// compilation's class of the same name, which is included as well and so is not told.
	/// </summary>
	[Fact]
	public void A_referenced_class_of_the_same_name_does_not_hide_an_include_of_the_local_one()
	{
		Assert.DoesNotContain(Reported(
			"""
			using DotGram;

			namespace N
			{
				[Gram("A = 'a'")]
				public partial class Lib { }
			}

			[GramInclude(typeof(N.Lib), Alias = "Local")]
			[Gram("S = Local.A\nparse S")]
			public partial class P : External.Bridge { }
			"""),
			static one => one.Id == "GRAM4059");
	}

	/// <summary>A class that includes a constructed form of itself is not what includes it.</summary>
	[Fact]
	public void A_class_does_not_include_itself()
	{
		var warned = Warnings("""
			using DotGram;

			[GramInclude(typeof(Lib<int>))]
			[Gram("A = 'a'", Name = "Fast")]
			public partial class Lib<T> { }
			""");

		Assert.Contains("'Lib<T>'", Assert.Single(warned).GetMessage(), StringComparison.Ordinal);
	}

	static Diagnostic[] Warnings(string source)
	{
		var run = GeneratorDriverTests.RunGenerator(source);

		Assert.DoesNotContain(run.Diagnostics, static one => one.Severity == DiagnosticSeverity.Error);

		return [.. run.Diagnostics.Where(static one => one.Id == "GRAM4059")];
	}

	/// <summary>
	/// What the generator reports of a compilation that references an assembly built with it: <c>N.Lib</c>,
	/// whose grammar publishes, and <c>External.Bridge</c> derived from it.
	/// </summary>
	static Diagnostic[] Reported(string source)
	{
		var parse    = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);
		var external = CSharpCompilation.Create(
			"External",
			[
				CSharpSyntaxTree.ParseText(
					"namespace N { [DotGram.Gram(\"A = 'a'\\nparse A\")] public partial class Lib { } }\nnamespace External { public class Bridge : N.Lib { } }",
					parse,
					cancellationToken: TestContext.Current.CancellationToken),
			],
			EmittedCode.References,
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

		CSharpGeneratorDriver
			.Create([new GramGenerator().AsSourceGenerator()], parseOptions: parse)
			.RunGeneratorsAndUpdateCompilation(external, out var generated, out _, TestContext.Current.CancellationToken);

		using var image = new MemoryStream();

		Assert.True(generated.Emit(image, cancellationToken: TestContext.Current.CancellationToken).Success);

		var compilation = CSharpCompilation.Create(
			"Local",
			[CSharpSyntaxTree.ParseText(source, parse, cancellationToken: TestContext.Current.CancellationToken)],
			[.. EmittedCode.References, MetadataReference.CreateFromImage(image.ToArray())],
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

		var run = CSharpGeneratorDriver
			.Create([new GramGenerator().AsSourceGenerator()], parseOptions: parse)
			.RunGenerators(compilation, TestContext.Current.CancellationToken)
			.GetRunResult();

		Assert.DoesNotContain(run.Diagnostics, static one => one.Severity == DiagnosticSeverity.Error);

		return [.. run.Diagnostics];
	}

}
