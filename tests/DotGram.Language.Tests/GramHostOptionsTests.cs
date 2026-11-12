using System;
using System.Linq;
using System.Reflection;

using DotGram.Grammar.Emit;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Xunit;

namespace DotGram.Language.Tests;

/// <summary>
/// The one reader of a <c>[Gram]</c>'s and a <c>[GramParserVariant]</c>'s named arguments, which the
/// generator compiles with and the editor analyses with.
/// </summary>
public sealed class GramHostOptionsTests
{
	[Fact]
	public void A_reading_that_says_nothing_takes_the_defaults()
	{
		var options = Read("""
			[DotGram.Gram("A = 'a'")]
			public partial class Parser;
			""");

		Assert.Equal(
			new GramHostOptions
			{
				DefaultAlias           = null,
				TargetMethodPartSize   = 0,
				Lexical                = false,
				ExportGrammar          = true,
				LocationType           = null,
				Carrier                = 0,
				MaxExtraStacks         = 0,
				Name                   = null,
				ConstructionsMayThrow  = false,
				UseSpanCaptures        = false,
				BufferedBytes          = false,
				BufferedText           = false,
				MaxBufferedInputLength = int.MaxValue,
				InitialBufferSize      = 4096,
				UsePerCallLocations    = false,
				Trace                  = false,
			},
			options[0]);
	}

	[Theory]
	[InlineData("[DotGram.Gram(\"A = 'a'\")] public partial class Parser;", true)]
	[InlineData("[DotGram.Gram(\"A = 'a'\")] partial class Parser;", false)]
	[InlineData("public static partial class Outer { [DotGram.Gram(\"A = 'a'\")] public partial class Parser; }", true)]
	[InlineData("static partial class Outer { [DotGram.Gram(\"A = 'a'\")] public partial class Parser; }", false)]
	public void A_host_carries_its_grammar_where_anything_outside_can_name_it(string declaration, bool exported)
	{
		var options = Read(declaration);

		Assert.Equal(exported, options.Single().ExportGrammar);
	}

	/// <summary>What was written of <c>ExportGrammar</c> is kept apart from what the class's visibility gave it.</summary>
	[Theory]
	[InlineData("[DotGram.Gram(\"A = 'a'\")] public partial class Parser;", true, null)]
	[InlineData("[DotGram.Gram(\"A = 'a'\", ExportGrammar = true)] public partial class Parser;", true, true)]
	[InlineData("[DotGram.Gram(\"A = 'a'\", ExportGrammar = true)] partial class Parser;", true, true)]
	[InlineData("[DotGram.Gram(\"A = 'a'\", ExportGrammar = false)] public partial class Parser;", false, false)]
	public void What_was_written_of_ExportGrammar_is_told_from_its_default(string declaration, bool exported, bool? written)
	{
		var options = Read(declaration).Single();

		Assert.Equal(exported, options.ExportGrammar);
		Assert.Equal(written, options.ExportGrammarWritten);
	}

	[Fact]
	public void Every_named_argument_is_read_by_its_name()
	{
		var options = Read("""
			namespace N;

			sealed class Where;

			[DotGram.Gram("A = 'a'", DefaultAlias = "Inc", TargetMethodPartSize = 7, Lexical = true, ExportGrammar = false,
				LocationType = typeof(Where), Carrier = DotGram.GramCarrier.Deferred, MaxExtraStacks = 3, Name = "S",
				ConstructionsMayThrow = true, UseSpanCaptures = true, BufferedBytes = true, BufferedText = true,
				MaxBufferedInputLength = 9, InitialBufferSize = 64, Trace = true)]
			[DotGram.GramParserVariant("Located", UsePerCallLocations = true)]
			public partial class Parser;
			""");

		Assert.Equal(
			new GramHostOptions
			{
				DefaultAlias           = "Inc",
				TargetMethodPartSize   = 7,
				Lexical                = true,
				ExportGrammar          = false,
				ExportGrammarWritten   = false,
				LocationType           = "N.Where",
				Carrier                = Carrier("Deferred"),
				MaxExtraStacks         = 3,
				Name                   = "S",
				ConstructionsMayThrow  = true,
				UseSpanCaptures        = true,
				BufferedBytes          = true,
				BufferedText           = true,
				MaxBufferedInputLength = 9,
				InitialBufferSize      = 64,
				UsePerCallLocations    = false,
				Trace                  = true,
			},
			options[0]);
		Assert.Equal("Located", options[1].Name);
		Assert.True(options[1].UsePerCallLocations);
	}

	/// <summary>
	/// An assembly built by DotGram 0.2 carries its attributes under their old names, and a grammar
	/// included from it is still read with the options it was compiled with.
	/// </summary>
	[Fact]
	public void The_names_DotGram_0_2_wrote_are_still_read()
	{
		var compilation = Compile(Legacy + """
			[DotGram.Gram("A = 'a'", IncludedAs = "Inc", PartSize = 7, Portable = false, Stacks = 3, Suffix = "S",
				SpanCaptures = true, BufferedInput = true, MaxRetained = 9, BufferSize = 64)]
			[DotGram.GramOptions(Suffix = "Located", PerCall = true)]
			public partial class Parser;
			""", support: false);
		var type     = compilation.GetTypeByMetadataName("Parser")!;
		var readings = type.GetAttributes().OrderBy(static attribute => attribute.ApplicationSyntaxReference!.Span.Start).ToArray();
		var first    = GramHostOptions.Read(readings[0], type, first: null);
		var second   = GramHostOptions.Read(readings[1], type, first);

		Assert.Equal("Inc", first.DefaultAlias);
		Assert.Equal(7, first.TargetMethodPartSize);
		Assert.False(first.ExportGrammar);
		Assert.Equal(3, first.MaxExtraStacks);
		Assert.Equal("S", first.Name);
		Assert.True(first.UseSpanCaptures);
		Assert.True(first.BufferedText);
		Assert.Equal(9, first.MaxBufferedInputLength);
		Assert.Equal(64, first.InitialBufferSize);
		Assert.Equal("Located", second.Name);
		Assert.True(second.UsePerCallLocations);
		Assert.Equal("Inc", GramIncludes.Named(readings[0], GramIncludes.DefaultAliasArgument, "IncludedAs"));
		Assert.Null(GramIncludes.Primary(type));
	}

	/// <summary>The attributes as DotGram 0.2 emitted them, by the names that matter here.</summary>
	const string Legacy = """
		namespace DotGram
		{
			internal class GramOptionsAttribute : System.Attribute
			{
				public string Suffix { get; set; }
				public int PartSize { get; set; }
				public bool BufferedInput { get; set; }
				public bool SpanCaptures { get; set; }
				public int MaxRetained { get; set; } = int.MaxValue;
				public int BufferSize { get; set; } = 4096;
				public int Stacks { get; set; }
				public bool PerCall { get; set; }
				public bool Portable { get; set; } = true;
			}

			internal sealed class GramAttribute : GramOptionsAttribute
			{
				public GramAttribute(string source) { Source = source; }
				public string Source { get; }
				public string IncludedAs { get; set; }
			}
		}

		""";

	[Fact]
	public void A_later_reading_takes_what_it_does_not_say_from_the_first_but_not_which_reading_it_is()
	{
		var options = Read("""
			[DotGram.Gram("A = 'a'", DefaultAlias = "Inc", Lexical = true, TargetMethodPartSize = 7, MaxExtraStacks = 3, Trace = true)]
			[DotGram.GramParserVariant("B", TargetMethodPartSize = 2)]
			public partial class Parser;
			""");

		var second = options[1];

		Assert.True(second.Lexical);
		Assert.Equal(2, second.TargetMethodPartSize);
		Assert.Equal(3, second.MaxExtraStacks);
		Assert.True(second.Trace);
		Assert.Equal("B", second.Name);
		Assert.Null(second.DefaultAlias);
		Assert.False(second.UsePerCallLocations);
	}

	/// <summary>
	/// The reader asks for each argument by the name of its own property, so a property here the
	/// attributes do not declare is an argument nobody can write. Except what one of them was written
	/// as, which tells an option written from one left to its default (<c>…Written</c>).
	/// </summary>
	[Fact]
	public void Every_option_is_a_property_of_the_attribute()
	{
		var compilation = Compile("");
		var attribute   = compilation.GetTypeByMetadataName("DotGram.GramAttribute")!;
		var variant     = compilation.GetTypeByMetadataName("DotGram.GramParserVariantAttribute")!;
		var declared    = new[] { attribute, attribute.BaseType!, variant }
			.SelectMany(static type => type.GetMembers().OfType<IPropertySymbol>())
			.Select(static property => property.Name)
			.ToHashSet(StringComparer.Ordinal);

		foreach (var property in typeof(GramHostOptions).GetProperties(BindingFlags.Public | BindingFlags.Instance))
			Assert.Contains(property.Name.EndsWith("Written", StringComparison.Ordinal) ? property.Name.Substring(0, property.Name.Length - "Written".Length) : property.Name, declared);
	}

	static GramHostOptions[] Read(string source)
	{
		var compilation = Compile(source);
		var type        = compilation.GetSymbolsWithName("Parser", SymbolFilter.Type, TestContext.Current.CancellationToken)
			.OfType<INamedTypeSymbol>()
			.Single();
		var readings    = type.GetAttributes()
			.Where(static attribute => attribute.AttributeClass?.Name is "GramAttribute" or "GramParserVariantAttribute")
			.OrderBy(static attribute => attribute.ApplicationSyntaxReference!.Span.Start)
			.ToArray();

		var first  = GramHostOptions.Read(readings[0], type, first: null);
		var result = new GramHostOptions[readings.Length];

		result[0] = first;

		for (var index = 1; index < readings.Length; index++)
			result[index] = GramHostOptions.Read(readings[index], type, first);

		return result;
	}

	static int Carrier(string name)
	{
		var carrier = Compile("").GetTypeByMetadataName("DotGram.GramCarrier")!;

		return (int)carrier.GetMembers(name).OfType<IFieldSymbol>().Single().ConstantValue!;
	}

	static CSharpCompilation Compile(string source, bool support = true)
	{
		var cancellationToken = TestContext.Current.CancellationToken;
		var parse             = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);
		var compilation       = CSharpCompilation.Create(
			"Host",
			[
				CSharpSyntaxTree.ParseText(support ? SupportEmitter.Attributes : "", parse, cancellationToken: cancellationToken),
				CSharpSyntaxTree.ParseText(SupportEmitter.EmbeddedAttribute, parse, cancellationToken: cancellationToken),
				CSharpSyntaxTree.ParseText(source, parse, cancellationToken: cancellationToken),
			],
			[MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

		Assert.DoesNotContain(
			compilation.GetDiagnostics(cancellationToken), static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);

		return compilation;
	}
}
