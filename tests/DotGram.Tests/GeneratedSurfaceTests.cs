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

	static Type Span(string grammar, string members)
	{
		var assembly = GeneratorDriverTests.Build(
			"[DotGram.Gram(\"\"\"\n" + grammar + "\n\"\"\")]\npublic static partial class Probe\n{\n" + members + "\n}\n");

		return assembly.GetType("Probe+SourceSpan")!;
	}
}
