using System;

using DotGram.Web;

using Xunit;

namespace DotGram.Tests.Web;

/// <summary>
/// RFC 6901, held to the examples of its §5 and §6 resolved against the document they are given for.
/// </summary>
/// <remarks>
/// The document is read by <see cref="Rfc8259"/>, and every example has to land on the value the RFC
/// names — compared as JSON, so that what is compared is the value and not how it was spaced.
/// </remarks>
public sealed class Rfc6901Tests
{
	const string Document = """
		{
		   "foo": ["bar", "baz"],
		   "": 0,
		   "a/b": 1,
		   "c%d": 2,
		   "e^f": 3,
		   "g|h": 4,
		   "i\\j": 5,
		   "k\"l": 6,
		   " ": 7,
		   "m~n": 8
		}
		""";

	// §5, the JSON strings unescaped as C# strings.
	[Theory]
	[InlineData("",       Document)]
	[InlineData("/foo",   "[\"bar\", \"baz\"]")]
	[InlineData("/foo/0", "\"bar\"")]
	[InlineData("/",      "0")]
	[InlineData("/a~1b",  "1")]
	[InlineData("/c%d",   "2")]
	[InlineData("/e^f",   "3")]
	[InlineData("/g|h",   "4")]
	[InlineData("/i\\j",  "5")]
	[InlineData("/k\"l",  "6")]
	[InlineData("/ ",     "7")]
	[InlineData("/m~0n",  "8")]
	public void The_string_examples_reach_what_the_RFC_says(string pointer, string value)
	{
		var parsed = Rfc6901.ParsePointer(pointer);

		Assert.Equal(Rfc8259.ParseJson(value).ToString(), parsed.Resolve(Rfc8259.ParseJson(Document))?.ToString());
		Assert.Equal(pointer, parsed.ToString());
	}

	// §6.
	[Theory]
	[InlineData("#",        "")]
	[InlineData("#/foo",    "/foo")]
	[InlineData("#/foo/0",  "/foo/0")]
	[InlineData("#/",       "/")]
	[InlineData("#/a~1b",   "/a~1b")]
	[InlineData("#/c%25d",  "/c%d")]
	[InlineData("#/e%5Ef",  "/e^f")]
	[InlineData("#/g%7Ch",  "/g|h")]
	[InlineData("#/i%5Cj",  "/i\\j")]
	[InlineData("#/k%22l",  "/k\"l")]
	[InlineData("#/%20",    "/ ")]
	[InlineData("#/m~0n",   "/m~0n")]
	public void The_fragment_examples_are_the_same_pointers(string fragment, string pointer)
	{
		var parsed   = Rfc6901.ParseFragment(fragment);
		var expected = Rfc6901.ParsePointer(pointer);
		var document = Rfc8259.ParseJson(Document);

		Assert.Equal(expected.Tokens, parsed.Tokens);
		Assert.Equal(expected.Resolve(document)?.ToString(), parsed.Resolve(document)?.ToString());
		Assert.Equal(fragment, parsed.ToUriFragment());
	}

	/// <summary>Where a pointer refers to nothing, it resolves to nothing — which is not JSON's null.</summary>
	[Theory]
	[InlineData("/nothing")]              // no member of the name
	[InlineData("/foo/2")]                // past the end of the array
	[InlineData("/foo/-")]                // the element after the last, which never exists
	[InlineData("/foo/01")]               // a leading zero is no index
	[InlineData("/foo/x")]                // an array takes no name
	[InlineData("/foo/0/x")]              // a string has no members
	[InlineData("/twice")]                // a name that is not unique names an undefined member (§4)
	public void A_pointer_that_refers_to_nothing_resolves_to_nothing(string pointer)
	{
		var document = Rfc8259.ParseJson("""{ "foo": ["bar", "baz"], "twice": 1, "twice": 2, "nothing at all": null }""");

		Assert.Null(Rfc6901.ParsePointer(pointer).Resolve(document));
	}

	[Fact]
	public void A_member_whose_value_is_null_resolves_to_null_the_value()
	{
		var document = Rfc8259.ParseJson("""{ "empty": null }""");

		Assert.Same(JsonValue.Null.Instance, Rfc6901.ParsePointer("/empty").Resolve(document));
	}

	/// <summary>A pointer is equal to another with the same tokens, whichever form it was read from.</summary>
	[Fact]
	public void Pointers_are_equal_by_their_tokens()
	{
		Assert.Equal(Rfc6901.ParsePointer("/a~1b/0"), Rfc6901.ParseFragment("#/a~1b/0"));
		Assert.Equal(Rfc6901.ParsePointer("/c%d").GetHashCode(), Rfc6901.ParseFragment("#/c%25d").GetHashCode());
		Assert.NotEqual(Rfc6901.ParsePointer("/a/0"), Rfc6901.ParsePointer("/a/1"));
	}

	/// <summary>`~1` is undone before `~0`, so `~01` is `~1` and never `/` (§4).</summary>
	[Fact]
	public void Escapes_are_undone_in_the_order_the_RFC_gives()
	{
		Assert.Equal(["~1"], Rfc6901.ParsePointer("/~01").Tokens);
		Assert.Equal(["/~"], Rfc6901.ParsePointer("/~1~0").Tokens);
		Assert.Equal("/~01", new JsonPointer(["~1"]).ToString());
	}

	[Theory]
	[InlineData("foo")]         // a token begins with `/`
	[InlineData("/~")]          // a `~` with nothing after it
	[InlineData("/~2")]         // a `~` that escapes nothing
	[InlineData("/a~b")]
	public void A_pointer_the_ABNF_does_not_make_is_refused(string pointer) =>
		Assert.False(Rfc6901.TryParsePointer(pointer).IsSuccess, $"'{pointer}' was read.");

	[Theory]
	[InlineData("/foo")]        // a fragment begins with `#`
	[InlineData("#/ ")]         // a space is not in a fragment unencoded
	[InlineData("#/%zz")]       // not a triplet
	[InlineData("#/%C3")]       // a byte that begins a character and no more
	[InlineData("#foo")]        // a fragment whose text is no pointer
	[InlineData("#/%7E2")]      // `~2`, encoded, is still no escape
	public void A_fragment_that_holds_no_pointer_is_refused(string fragment) =>
		Assert.False(Rfc6901.TryParseFragment(fragment).IsSuccess, $"'{fragment}' was read.");

	[Theory]
	[InlineData("0",   0)]
	[InlineData("10",  10)]
	[InlineData("01",  null)]
	[InlineData("-",   null)]
	[InlineData("",    null)]
	[InlineData("1a",  null)]
	[InlineData("99999999999", null)]
	public void An_array_index_is_zero_or_digits_without_a_leading_zero(string token, int? index)
	{
		Assert.Equal(index, JsonPointer.ArrayIndex(token));
		Assert.Equal(token == "-", JsonPointer.IsPastTheEnd(token));
	}
}
