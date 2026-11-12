using System;
using System.Linq;

using Microsoft.CodeAnalysis;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A project still written with a name the attributes had before 0.3.0 is told what it is called
/// now, at the old name (GRAM0012), and not what its grammar then seems to say wrong.
/// </summary>
public sealed class RenamedNameTests
{
	const string Source = """
		using DotGram;

		[Gram("Start = 'a'+\nparse Start", IncludedAs = "X", Portable = false, Carrier = GramCarrier.Tape, PartSize = 10)]
		[GramOptions(Suffix = "Fast", BufferedInput = true)]
		public static partial class Old { }

		[GramInclude(typeof(Old), As = "Y")]
		[Gram("using Y;\nS = Start\nparse S")]
		public static partial class Older { }
		""";

	[Fact]
	public void Every_old_name_is_told_what_it_is_now()
	{
		var run      = GeneratorDriverTests.RunGenerator(Source);
		var renamed  = run.Diagnostics.Where(static one => one.Id == "GRAM0012").OrderBy(static one => one.Location.SourceSpan.Start).ToArray();
		var messages = renamed.Select(static one => one.GetMessage()).ToArray();

		Assert.All(renamed, static one => Assert.Equal(DiagnosticSeverity.Error, one.Severity));
		Assert.Equal(
			[
				"'IncludedAs' was renamed to 'DefaultAlias' in 0.3.0",
				"'Portable' was renamed to 'ExportGrammar' in 0.3.0",
				"'GramCarrier.Tape' was renamed to 'GramCarrier.Deferred' in 0.3.0",
				"'PartSize' was renamed to 'TargetMethodPartSize' in 0.3.0",
				"'GramOptions' was renamed to 'GramParserVariant(\"…\")' in 0.3.0",
				"'As' was renamed to 'Alias' in 0.3.0",
			],
			messages);
		Assert.Equal(Source.IndexOf("IncludedAs", StringComparison.Ordinal), renamed[0].Location.SourceSpan.Start);
	}

	/// <summary>What the dropped names make the grammars seem to say wrong is left unsaid.</summary>
	[Fact]
	public void What_follows_from_an_old_name_is_not_said()
	{
		var run = GeneratorDriverTests.RunGenerator(Source);

		Assert.DoesNotContain(run.Diagnostics, static one => one.Id.StartsWith("GRAM", StringComparison.Ordinal) && one.Id != "GRAM0012");
	}

	/// <summary>
	/// An old name that drops no alias holds nothing back: a grammar's own error beside it is said,
	/// and so is one beside an author's own <c>[GramOptions]</c>, which is no old name at all.
	/// </summary>
	[Theory]
	[InlineData("[Gram(\"S = Missing\\nparse S\", Portable = false)]")]
	[InlineData("[GramOptions]\n[Gram(\"S = Missing\\nparse S\")]")]
	public void An_error_the_rename_does_not_explain_is_said(string attributes)
	{
		var run = GeneratorDriverTests.RunGenerator(
			"using DotGram;\n\n" +
			(attributes.StartsWith("[GramOptions]", StringComparison.Ordinal) ? "sealed class GramOptionsAttribute : System.Attribute { }\n\n" : "") +
			attributes + "\npublic static partial class Broken { }");

		Assert.Contains(run.Diagnostics, static one => one.Id == "GRAM3002");
	}

	/// <summary>An author's own attribute with an option of an old name, or an enum with a member <c>Tape</c>, is left alone.</summary>
	[Fact]
	public void An_authors_own_names_are_left_alone()
	{
		var run = GeneratorDriverTests.RunGenerator("""
			namespace Own
			{
				sealed class GramAttribute : System.Attribute
				{
					public string Suffix { get; set; } = "";
				}

				enum GramCarrier { Tape }

				[Gram(Suffix = "x")]
				static class Mine
				{
					static GramCarrier Carried = GramCarrier.Tape;
				}
			}
			""");

		Assert.DoesNotContain(run.Diagnostics, static one => one.Id == "GRAM0012");
	}

	/// <summary>
	/// An old name is found however DotGram's attribute or enum is spelled — through an alias — and
	/// <c>GramCarrier.Tape</c> wherever it is written.
	/// </summary>
	[Fact]
	public void An_old_name_is_found_through_an_alias_and_outside_attributes()
	{
		const string source = """
			using G = DotGram.GramAttribute;
			using C = DotGram.GramCarrier;

			[G("Start = 'a'+\nparse Start", Portable = false, Carrier = C.Tape)]
			public static partial class Aliased
			{
				static object Carried = DotGram.GramCarrier.Tape;
			}
			""";

		var renamed = GeneratorDriverTests.RunGenerator(source).Diagnostics
			.Where(static one => one.Id == "GRAM0012")
			.OrderBy(static one => one.Location.SourceSpan.Start)
			.Select(static one => one.GetMessage())
			.ToArray();

		Assert.Equal(
			[
				"'Portable' was renamed to 'ExportGrammar' in 0.3.0",
				"'GramCarrier.Tape' was renamed to 'GramCarrier.Deferred' in 0.3.0",
				"'GramCarrier.Tape' was renamed to 'GramCarrier.Deferred' in 0.3.0",
			],
			renamed);
	}

	const string Lib = """
		using DotGram;

		[Gram("A = 'a'\nparse A")]
		public static partial class Lib { }

		static class Names { public const string Old = "Old"; }

		""";

	/// <summary>
	/// An old <c>As</c> holds back only what the alias explains: the namespace it named, and the names
	/// a grammar opening it reaches; an error of its own beside it is said, as it is with no old name.
	/// </summary>
	[Theory]
	[InlineData("As = \"Old\"", "using Old;\\nS = A\\nparse S", new string[0])]
	[InlineData("As = \"Old\"", "S = Typo\\nparse S", new[] { "GRAM3002" })]
	[InlineData("As = \"Old\"", "S = Old.A & Typo\\nparse S", new[] { "GRAM3002" })]
	[InlineData("Alias = \"Old\"", "S = Typo\\nparse S", new[] { "GRAM3002" })]
	[InlineData("As = Names.Old", "using Old;\\nS = A\\nparse S", new string[0])]
	[InlineData("As = nameof(Names.Old)", "using Old;\\nS = A\\nparse S", new string[0])]
	[InlineData("As = Names.Old + \"er\"", "using Older;\\nS = A\\nparse S", new string[0])]
	public void Only_what_a_dropped_alias_explains_is_held_back(string alias, string grammar, string[] said)
	{
		var run = GeneratorDriverTests.RunGenerator(Lib +
			"[GramInclude(typeof(Lib), " + alias + ")]\n[Gram(\"" + grammar + "\")]\npublic static partial class P { }");

		Assert.Equal(
			said,
			run.Diagnostics.Where(static one => one.Id.StartsWith("GRAM", StringComparison.Ordinal) && one.Id != "GRAM0012" && one.Severity == DiagnosticSeverity.Error)
				.Select(static one => one.Id)
				.Distinct()
				.ToArray());
	}

	/// <summary>An old name written on the other attribute, <c>As</c> on <c>[Gram]</c>, is no old name of it: nothing is held back, nor said by GRAM0012.</summary>
	[Fact]
	public void An_alias_on_the_wrong_attribute_holds_nothing_back()
	{
		var run = GeneratorDriverTests.RunGenerator(Lib +
			"[GramInclude(typeof(Lib), IncludedAs = \"Old\")]\n[Gram(\"using Old;\\nS = A\\nparse S\", As = \"Old\")]\npublic static partial class P { }");

		Assert.DoesNotContain(run.Diagnostics, static one => one.Id == "GRAM0012");
		Assert.Contains(run.Diagnostics, static one => one.Id == "GRAM3003");
	}

	/// <summary>
	/// An old <c>IncludedAs</c> on a grammar that has a <c>Name</c> is never what an include is called, as
	/// such a grammar is never what is included: what its host then fails to reach is said.
	/// </summary>
	[Fact]
	public void An_old_alias_on_a_named_grammar_holds_nothing_back()
	{
		var run = GeneratorDriverTests.RunGenerator("""
			using DotGram;

			[Gram("A = 'a'\nparse A", Name = "Other", IncludedAs = "Old")]
			public static partial class Lib { }

			[GramInclude(typeof(Lib))]
			[Gram("S = Old.A\nparse S")]
			public static partial class P { }
			""");

		Assert.Contains(run.Diagnostics, static one => one.Id == "GRAM0012");
		Assert.Contains(run.Diagnostics, static one => one.Id == "GRAM3002");
	}

	/// <summary>An old <c>As</c> between two included grammars explains what the outer host meets through it too.</summary>
	[Fact]
	public void An_old_alias_on_an_include_of_an_include_is_held_back_for_the_outer_host()
	{
		var run = GeneratorDriverTests.RunGenerator(Lib + """
			[GramInclude(typeof(Lib), As = "Old")]
			[Gram("M = 'm'\nparse M")]
			public static partial class Middle { }

			[GramInclude(typeof(Middle))]
			[Gram("using Old;\nS = A\nparse S")]
			public static partial class P { }
			""");

		Assert.Single(run.Diagnostics, static one => one.Id == "GRAM0012");
		Assert.DoesNotContain(run.Diagnostics, static one => one.Id is "GRAM3002" or "GRAM3003");
	}

	/// <summary>
	/// <c>GramCarrier.Tape</c> imported by <c>using static</c>, and <c>[GramOptions]</c> reached through an
	/// alias, are old names too; a name qualified by a namespace of the author's own is not.
	/// </summary>
	[Theory]
	[InlineData("using static DotGram.GramCarrier;\n\nstatic class X { static object Carried = Tape; }", "GramCarrier.Tape")]
	[InlineData("using O = DotGram.GramOptionsAttribute;\n\n[O(Suffix = \"Fast\")]\n[DotGram.Gram(\"A = 'a'\\nparse A\")]\npublic static partial class X { }", "GramOptions")]
	[InlineData("namespace Own { }\n\n[Own.GramOptions]\n[DotGram.Gram(\"A = 'a'\\nparse A\")]\npublic static partial class X { }", null)]
	[InlineData("static class X { static object Tape = 1; static object Carried = Tape; }", null)]
	[InlineData("global using static DotGram.GramCarrier;\n\nstatic class X { static object Carried = Tape; }", "GramCarrier.Tape")]
	[InlineData("using static DotGram.GramCarrier;\n\nclass Tape { }\n\nstatic class X { static Tape Value; }", null)]
	[InlineData("using static DotGram.GramCarrier;\n\nstatic class X { static Tape Value; static System.Collections.Generic.List<Tape> Values; }", null)]
	[InlineData("static class X { static object Carried = Tape; }", null)]
	[InlineData("namespace Own { namespace DotGram { } namespace Consumer { using O = DotGram.GramOptionsAttribute; [O] class X { } } }", null)]
	[InlineData("namespace Own { namespace DotGram { } namespace Consumer { using O = global::DotGram.GramOptionsAttribute; [O] class X { } } }", "GramOptions")]
	[InlineData("global using O = DotGram.GramOptionsAttribute;\n\n[O(Suffix = \"Fast\")]\n[DotGram.Gram(\"A = 'a'\\nparse A\")]\npublic static partial class X { }", "GramOptions")]
	[InlineData("[GramOptions]\nclass X { }", null)]
	[InlineData("using DotGram;\n\n[GramOptions]\nclass X { }", "GramOptions")]
	public void An_old_name_is_found_as_C_sharp_reaches_it(string source, string? old)
	{
		var renamed = GeneratorDriverTests.RunGenerator(source).Diagnostics.Where(static one => one.Id == "GRAM0012").ToArray();

		if (old is null)
			Assert.Empty(renamed);
		else
			Assert.StartsWith("'" + old + "'", Assert.Single(renamed).GetMessage(), StringComparison.Ordinal);
	}

	/// <summary>A type of the author's own that happens to be called <c>GramOptions</c> is not taken for the old attribute.</summary>
	[Fact]
	public void An_authors_own_GramOptions_is_left_alone()
	{
		var run = GeneratorDriverTests.RunGenerator("""
			sealed class GramOptionsAttribute : System.Attribute { }

			[GramOptions]
			[DotGram.Gram("Start = 'a'+\nparse Start")]
			public static partial class Mine { }
			""");

		Assert.DoesNotContain(run.Diagnostics, static one => one.Id == "GRAM0012");
	}
}
