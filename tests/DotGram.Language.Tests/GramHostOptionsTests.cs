using System;
using System.Linq;

using DotGram.Grammar.Emit;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Xunit;

namespace DotGram.Language.Tests;

/// <summary>
/// The one reader of a <c>[Gram]</c>'s and a <c>[GramOptions]</c>'s named arguments, which the
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
			new GramHostOptions(
				IncludedAs:            null,
				PartSize:              0,
				Lexical:               false,
				Portable:              true,
				LocationType:          null,
				Carrier:               0,
				Stacks:                0,
				Suffix:                null,
				ConstructionsMayThrow: false,
				SpanCaptures:          false,
				BufferedBytes:         false,
				BufferedInput:         false,
				MaxRetained:           int.MaxValue,
				BufferSize:            4096,
				PerCall:               false,
				Trace:                 false),
			options[0]);
	}

	[Theory]
	[InlineData("[DotGram.Gram(\"A = 'a'\")] public partial class Parser;", true)]
	[InlineData("[DotGram.Gram(\"A = 'a'\")] partial class Parser;", false)]
	[InlineData("public static partial class Outer { [DotGram.Gram(\"A = 'a'\")] public partial class Parser; }", true)]
	[InlineData("static partial class Outer { [DotGram.Gram(\"A = 'a'\")] public partial class Parser; }", false)]
	public void A_host_carries_its_grammar_where_anything_outside_can_name_it(string declaration, bool portable)
	{
		var options = Read(declaration);

		Assert.Equal(portable, options.Single().Portable);
	}

	[Fact]
	public void Every_named_argument_is_read_by_its_name()
	{
		var options = Read("""
			namespace N;

			sealed class Where;

			[DotGram.Gram("A = 'a'", IncludedAs = "Inc", PartSize = 7, Lexical = true, Portable = false,
				LocationType = typeof(Where), Carrier = DotGram.GramCarrier.Tape, Stacks = 3, Suffix = "S",
				ConstructionsMayThrow = true, SpanCaptures = true, BufferedBytes = true, BufferedInput = true,
				MaxRetained = 9, BufferSize = 64, PerCall = true, Trace = true)]
			public partial class Parser;
			""").Single();

		Assert.Equal(
			new GramHostOptions(
				IncludedAs:            "Inc",
				PartSize:              7,
				Lexical:               true,
				Portable:              false,
				LocationType:          "N.Where",
				Carrier:               Carrier("Tape"),
				Stacks:                3,
				Suffix:                "S",
				ConstructionsMayThrow: true,
				SpanCaptures:          true,
				BufferedBytes:         true,
				BufferedInput:         true,
				MaxRetained:           9,
				BufferSize:            64,
				PerCall:               true,
				Trace:                 true),
			options);
	}

	[Fact]
	public void A_later_reading_takes_what_it_does_not_say_from_the_first_but_not_which_reading_it_is()
	{
		var options = Read("""
			[DotGram.Gram("A = 'a'", IncludedAs = "Inc", Lexical = true, PartSize = 7, Stacks = 3, Trace = true, PerCall = true)]
			[DotGram.GramOptions(Suffix = "B", PartSize = 2)]
			public partial class Parser;
			""");

		var second = options[1];

		Assert.True(second.Lexical);
		Assert.Equal(2, second.PartSize);
		Assert.Equal(3, second.Stacks);
		Assert.True(second.Trace);
		Assert.Equal("B", second.Suffix);
		Assert.Null(second.IncludedAs);
		Assert.False(second.PerCall);
	}

	/// <summary>
	/// The reader asks for each argument by the name of its own property, so a property here the
	/// attributes do not declare is an argument nobody can write.
	/// </summary>
	[Fact]
	public void Every_option_is_a_property_of_the_attribute()
	{
		var compilation = Compile("");
		var attribute   = compilation.GetTypeByMetadataName("DotGram.GramAttribute")!;
		var declared    = new[] { attribute, attribute.BaseType! }
			.SelectMany(static type => type.GetMembers().OfType<IPropertySymbol>())
			.Select(static property => property.Name)
			.ToHashSet(StringComparer.Ordinal);

		foreach (var parameter in typeof(GramHostOptions).GetConstructors().Single().GetParameters())
			Assert.Contains(parameter.Name!, declared);
	}

	static GramHostOptions[] Read(string source)
	{
		var compilation = Compile(source);
		var type        = compilation.GetSymbolsWithName("Parser", SymbolFilter.Type, TestContext.Current.CancellationToken)
			.OfType<INamedTypeSymbol>()
			.Single();
		var readings    = type.GetAttributes()
			.Where(static attribute => attribute.AttributeClass?.Name is "GramAttribute" or "GramOptionsAttribute")
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

	static CSharpCompilation Compile(string source)
	{
		var cancellationToken = TestContext.Current.CancellationToken;
		var parse             = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);
		var compilation       = CSharpCompilation.Create(
			"Host",
			[
				CSharpSyntaxTree.ParseText(SupportEmitter.Attributes, parse, cancellationToken: cancellationToken),
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
