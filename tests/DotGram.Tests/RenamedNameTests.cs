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
