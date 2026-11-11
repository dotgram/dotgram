using System;
using System.Linq;

using Microsoft.CodeAnalysis;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// What <c>LocationType</c> names is an interface declaring the <c>Locate</c> a reading calls, and a
/// value is told through it: an explicit implementation serves, and a struct is told in place. A
/// type that is no such interface is refused on the attribute (GRAM0011), not as a compiler error
/// in generated code.
/// </summary>
public sealed class LocationTypeTests
{
	const string Grammar = "Item : @Item = n: ['a'..'z']+ => @(new Item(n))\\nparse Item";

	static string Host(string types, string options, string variant = "")
	{
		return types + "\n[DotGram.Gram(\"" + Grammar + "\", " + options + ")]\n" + variant + "\npublic partial class Parser { }";
	}

	/// <summary>A type that is not an interface declaring the right <c>Locate</c> is refused, and nothing is generated for it.</summary>
	[Theory]
	[InlineData("public class ILoc { public void Locate(int at, int length) { } }", "", "void Locate(int at, int length)")]
	[InlineData("public interface ILoc { }", "", "void Locate(int at, int length)")]
	[InlineData("public interface ILoc { void Locate(int at); }", "public void Locate(int at) { }", "void Locate(int at, int length)")]
	[InlineData("public interface ILoc { void Locate(int at, int length); }", "public void Locate(int at, int length) { }", "void Locate(int at, int length, int gapStart)")]
	public void A_type_that_cannot_be_told_a_place_is_refused(string location, string implementation, string wanted)
	{
		var types   = location + "\npublic sealed class Item : ILoc { public Item(string text) { } " + implementation + " }";
		var perCall = wanted.Contains("gapStart", StringComparison.Ordinal);
		var variant = perCall ? "[DotGram.GramParserVariant(\"Located\", LocationType = typeof(ILoc), UsePerCallLocations = true)]" : "";
		var run     = GeneratorDriverTests.RunGenerator(Host(types, perCall ? "Lexical = false" : "LocationType = typeof(ILoc)", variant));
		var refused = Assert.Single(run.Diagnostics, static one => one.Id == "GRAM0011");

		Assert.Equal(DiagnosticSeverity.Error, refused.Severity);
		Assert.Contains("'ILoc'", refused.GetMessage(), StringComparison.Ordinal);
		Assert.Contains(wanted, refused.GetMessage(), StringComparison.Ordinal);
	}

	/// <summary>An explicit implementation of the interface is called, as a public one is.</summary>
	[Fact]
	public void An_explicit_implementation_is_told_where_it_was_written()
	{
		const string types = """
			public interface ILoc
			{
				void Locate(int at, int length);
			}

			public sealed class Item : ILoc
			{
				public Item(string text) { }

				public int At = -1;

				void ILoc.Locate(int at, int length)
				{
					At = at;
				}
			}
			""";

		var assembly = GeneratorDriverTests.Build(Host(types, "LocationType = typeof(ILoc)"));
		var parse    = assembly.GetType("Parser")!.GetMethod("ParseItem", [typeof(string)])!;
		var item     = parse.Invoke(null, ["abc"])!;

		Assert.Equal(0, (int)item.GetType().GetField("At")!.GetValue(item)!);
	}

	/// <summary>Inherited from another interface, the method is still the one called.</summary>
	[Fact]
	public void A_Locate_the_interface_inherits_serves()
	{
		const string types = """
			public interface IPlace
			{
				void Locate(int at, int length);
			}

			public interface ILoc : IPlace
			{
			}

			public sealed class Item : ILoc
			{
				public Item(string text) { }

				public int At = -1;

				public void Locate(int at, int length)
				{
					At = at;
				}
			}
			""";

		var run = GeneratorDriverTests.RunGenerator(Host(types, "LocationType = typeof(ILoc)"));

		Assert.DoesNotContain(run.Diagnostics, static one => one.Id == "GRAM0011");
		GeneratorDriverTests.Build(Host(types, "LocationType = typeof(ILoc)"));
	}
}
