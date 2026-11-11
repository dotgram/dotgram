using System;
using System.Runtime.CompilerServices;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// What the generator writes into a host that a caller outside the assembly can see: no more than
/// the grammar publishes and the host raises.
/// </summary>
public sealed class GeneratedSurfaceTests
{
	/// <summary>
	/// A span the grammar's own C# is handed, and a rule's value nothing public returns, leave
	/// <c>SourceSpan</c> internal.
	/// </summary>
	[Fact]
	public void A_span_no_public_signature_names_is_internal()
	{
		var span = Span("""
			Word : @SourceSpan = ['a'..'z']+
			Item = Word & ';' & when @(parserSpan.Length > 1)
			internal parse Item
			""", "");

		Assert.True(span.IsNestedAssembly);
		Assert.True(span.IsDefined(typeof(IsReadOnlyAttribute), false));
	}

	/// <summary>A public publication whose value is a span, or a property of a rule's type that holds one, makes it public.</summary>
	[Theory]
	[InlineData("Word : @SourceSpan = ['a'..'z']+\nparse Word")]
	[InlineData("Word : @SourceSpan = ['a'..'z']+\nItem = w: Word & ';'\ninternal parse Item")]
	public void A_span_a_public_signature_names_is_public(string grammar)
	{
		Assert.True(Span(grammar, "").IsNestedPublic);
	}

	/// <summary>
	/// A host that declares its part of the span says who may see it, and the generated part says
	/// nothing of its own: raised to public, and readonly from the generated part alone.
	/// </summary>
	[Theory]
	[InlineData("public partial struct SourceSpan { }", true)]
	[InlineData("internal partial struct SourceSpan { }", false)]
	public void A_host_that_declares_the_span_decides_who_sees_it(string declared, bool @public)
	{
		var span = Span("""
			Word = ['a'..'z']+ & when @(parserSpan.Length > 0)
			parse Word
			""", declared + "\n" + (@public ? "public" : "internal") + " static SourceSpan Measure() { return new SourceSpan(1, 2); }");

		Assert.Equal(@public, span.IsNestedPublic);
		Assert.True(span.IsDefined(typeof(IsReadOnlyAttribute), false));
	}

	/// <summary>
	/// A host with several readings has what they read from it written once, and only what at
	/// least one of them needs: a traced reading beside a plain one adds no span and no input.
	/// </summary>
	[Fact]
	public void A_second_reading_adds_to_the_host_only_what_it_needs()
	{
		var plain = GeneratorDriverTests.Build("""
			[DotGram.Gram("W = 'w'\nparse W")]
			[DotGram.GramVariant(Name = "Traced", Trace = true)]
			public static partial class Probe { }
			""");

		Assert.NotNull(plain.GetType("Probe+Match`1"));
		Assert.Null(plain.GetType("Probe+Traced+Match`1"));
		Assert.Null(plain.GetType("Probe+SourceSpan"));
		Assert.Null(plain.GetType("Probe+ParserInput`1"));

		var buffered = GeneratorDriverTests.Build("""
			[DotGram.Gram("W = 'w'\nparse W")]
			[DotGram.GramVariant(Name = "Buffered", BufferedText = true)]
			public static partial class Probe { }
			""");

		Assert.NotNull(buffered.GetType("Probe+ParserInput`1"));
		Assert.Null(buffered.GetType("Probe+Buffered+ParserInput`1"));
	}

	/// <summary>
	/// A host with one reading keeps what it reads in the reading's own class; a second reading
	/// moves it to the host, where both read it.
	/// </summary>
	[Fact]
	public void The_match_of_one_reading_is_its_class_and_of_two_the_hosts()
	{
		var one = GeneratorDriverTests.Build("""
			[DotGram.Gram("W = 'w'\nparse W", Name = "Fast")]
			public static partial class Probe { }
			""");

		Assert.NotNull(one.GetType("Probe+Fast+Match`1"));
		Assert.Null(one.GetType("Probe+Match`1"));

		var two = GeneratorDriverTests.Build("""
			[DotGram.Gram("W = 'w'\nparse W", Name = "Fast")]
			[DotGram.GramVariant(Name = "Slow", Carrier = DotGram.GramCarrier.Deferred)]
			public static partial class Probe { }
			""");

		Assert.NotNull(two.GetType("Probe+Match`1"));
		Assert.Null(two.GetType("Probe+Fast+Match`1"));
	}

	/// <summary>
	/// A host whose one reading goes into a class of its own has its span declared where the author
	/// declared a part of it: in the host, which the reading's class reads it from, or in the reading's
	/// class. Either way there is one span, of the author's accessibility.
	/// </summary>
	[Theory]
	[InlineData("public partial struct SourceSpan { }\npublic static SourceSpan Measure() { return new SourceSpan(1, 2); }", "Probe+SourceSpan", "Probe+Fast+SourceSpan")]
	[InlineData("public static partial class Fast\n{\npublic partial struct SourceSpan { }\npublic static SourceSpan Measure() { return new SourceSpan(1, 2); }\n}", "Probe+Fast+SourceSpan", "Probe+SourceSpan")]
	public void A_span_declared_beside_a_reading_in_a_class_of_its_own_is_one_span(string members, string declared, string absent)
	{
		var assembly = GeneratorDriverTests.Build(
			"[DotGram.Gram(\"\"\"\nWord = ['a'..'z']+ & when @(parserSpan.Length > 0)\nparse Word\n\"\"\", Name = \"Fast\")]\n" +
			"public static partial class Probe\n{\n" + members + "\n}\n");

		Assert.True(assembly.GetType(declared)!.IsNestedPublic);
		Assert.Null(assembly.GetType(absent));
	}

	/// <summary>
	/// A host with several readings has one span, in the host: a part declared there raises it, and a
	/// part declared in the class of a reading would be a second span, which is refused.
	/// </summary>
	[Fact]
	public void A_shared_host_has_its_span_declared_in_the_host()
	{
		const string attributes =
			"[DotGram.Gram(\"\"\"\nWord = ['a'..'z']+ & when @(parserSpan.Length > 0)\nparse Word\n\"\"\")]\n" +
			"[DotGram.GramVariant(Name = \"Slow\", Carrier = DotGram.GramCarrier.Deferred)]\n";

		var assembly = GeneratorDriverTests.Build(
			attributes + "public static partial class Probe\n{\npublic partial struct SourceSpan { }\npublic static SourceSpan Measure() { return new SourceSpan(1, 2); }\n}\n");

		Assert.True(assembly.GetType("Probe+SourceSpan")!.IsNestedPublic);
		Assert.Null(assembly.GetType("Probe+Slow+SourceSpan"));

		var run = GeneratorDriverTests.RunGenerator(
			attributes + "public static partial class Probe\n{\npublic static partial class Slow\n{\npublic partial struct SourceSpan { }\n}\n}\n");

		Assert.Contains(run.Diagnostics, static one => one.Id == "GRAM5018" && one.GetMessage().Contains("SourceSpan"));
	}

	static Type Span(string grammar, string members)
	{
		var assembly = GeneratorDriverTests.Build(
			"[DotGram.Gram(\"\"\"\n" + grammar + "\n\"\"\")]\npublic static partial class Probe\n{\n" + members + "\n}\n");

		return assembly.GetType("Probe+SourceSpan")!;
	}
}
