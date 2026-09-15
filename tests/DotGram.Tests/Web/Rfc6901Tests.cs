using System;
using System.Linq;
using System.Text.Json;

using DotGram.Web;

using Xunit;

namespace DotGram.Tests.Web;

/// <summary>
/// RFC 6901, held to the examples of its §5 and §6 evaluated against the document they are given for.
/// </summary>
/// <remarks>
/// The package reads a pointer and says nothing about documents, which would need a JSON library
/// it does not depend on; so the evaluation here is the test's, over <c>System.Text.Json</c>, and
/// it is §4's two rules and no more. What that holds is that the tokens read are the ones the RFC
/// means — every example lands on the value the RFC says it does.
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

	// §5, the JSON strings unescaped as C# strings, and each value as the document writes it.
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

		Assert.Equal(value, Evaluate(parsed));
		Assert.Equal(pointer, parsed.ToString());
	}

	// §6.
	[Theory]
	[InlineData("#",        "/")]
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
		var parsed = Rfc6901.ParseFragment(fragment);

		// `#` alone is the whole document, which the string form spells as nothing at all.
		var expected = fragment == "#" ? JsonPointer.Root : Rfc6901.ParsePointer(pointer);

		Assert.Equal(expected.Tokens, parsed.Tokens);
		Assert.Equal(Evaluate(expected), Evaluate(parsed));
		Assert.Equal(fragment, parsed.ToUriFragment());
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

	/// <summary>§4 over a document: a member by name, an element by index, and nothing else.</summary>
	/// <remarks>The value comes back as the document wrote it, which is what the examples compare.</remarks>
	static string Evaluate(JsonPointer pointer)
	{
		using var document = JsonDocument.Parse(Document);

		var value = document.RootElement;

		foreach (var token in pointer.Tokens)
		{
			value = value.ValueKind switch
			{
				JsonValueKind.Object => value.EnumerateObject().Single(member => member.Name == token).Value,
				JsonValueKind.Array  => value[JsonPointer.ArrayIndex(token) ?? throw new InvalidOperationException($"'{token}' names no element.")],
				_                    => throw new InvalidOperationException($"'{token}' goes into a value that has no parts."),
			};
		}

		return value.GetRawText();
	}
}
