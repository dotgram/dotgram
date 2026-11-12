using System;
using System.Linq;

using Microsoft.CodeAnalysis;

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

	static Diagnostic[] Warnings(string source)
	{
		var run = GeneratorDriverTests.RunGenerator(source);

		Assert.DoesNotContain(run.Diagnostics, static one => one.Severity == DiagnosticSeverity.Error);

		return [.. run.Diagnostics.Where(static one => one.Id == "GRAM4059")];
	}
}
