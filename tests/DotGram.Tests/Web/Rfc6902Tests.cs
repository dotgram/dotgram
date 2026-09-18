using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

using DotGram.Web;

using Xunit;

namespace DotGram.Tests.Web;

/// <summary>
/// RFC 6902's JSON Patch, held to the JSON Patch test suite and to the rules §4 and §5 give.
/// </summary>
/// <remarks>
/// <c>JsonPatch/tests.json</c> and <c>JsonPatch/spec_tests.json</c> are json-patch/json-patch-tests at 2a928f9,
/// under the Apache License 2.0 its README carries. A record with <c>expected</c> has to apply to that document,
/// compared as §4.6 compares; one with <c>error</c> has to fail, in reading or in applying; one with neither has
/// to apply. A record the suite marks <c>disabled</c> runs too: they are disabled for implementations whose JSON
/// reader drops a duplicate member or refuses a scalar document, and this one does neither.
/// </remarks>
public sealed class Rfc6902Tests
{
	// ── The suite ────────────────────────────────────────────────────────────────

	public static TheoryData<string, int> Records()
	{
		var data = new TheoryData<string, int>();

		foreach (var file in new[] { "tests.json", "spec_tests.json" })
		{
			var records = ((JsonValue.Array)Suite(file)).Items;

			for (var index = 0; index < records.Count; index++)
				if (Member(records[index], "patch") is not null)
					data.Add(file, index);
		}

		return data;
	}

	[Theory]
	[MemberData(nameof(Records))]
	public void The_suite(string file, int index)
	{
		var record   = ((JsonValue.Array)Suite(file)).Items[index];
		var document = Member(record, "doc")!;
		var expected = Member(record, "expected");
		var failure  = Member(record, "error");
		var comment  = Member(record, "comment") is JsonValue.String { Value: var text } ? text : "";

		JsonValue? result = null;

		var applied = JsonPatch.TryRead(Member(record, "patch")!, out var patch, out var error) &&
			patch.TryApply(document, out result, out error);

		if (failure is not null)
		{
			Assert.False(applied, $"{comment}: expected '{failure}', and the patch applied.");
			return;
		}

		Assert.True(applied, $"{comment}: {error}");

		if (expected is not null)
			Assert.True(JsonPatch.AreEqual(expected, result!), $"{comment}: {result} is not {expected}.");

		// Written back and read again, the patch is the same patch.
		Assert.Equal(patch, JsonPatch.Parse(patch!.ToString()));
	}

	// ── §4 ───────────────────────────────────────────────────────────────────────

	[Fact]
	public void The_example_of_section_3()
	{
		var patch = JsonPatch.Parse("""
			[
			  { "op": "test", "path": "/a/b/c", "value": "foo" },
			  { "op": "remove", "path": "/a/b/c" },
			  { "op": "add", "path": "/a/b/c", "value": [ "foo", "bar" ] },
			  { "op": "replace", "path": "/a/b/c", "value": 42 },
			  { "op": "move", "from": "/a/b/c", "path": "/a/b/d" },
			  { "op": "copy", "from": "/a/b/d", "path": "/a/b/e" }
			]
			""");

		var result = patch.Apply(Both.Json("""{ "a": { "b": { "c": "foo" } } }"""));

		Assert.Equal(Both.Json("""{ "a": { "b": { "d": 42, "e": 42 } } }"""), result);
	}

	/// <summary>§4: the order of an operation's members does not matter, and a member it does not define is ignored.</summary>
	[Fact]
	public void Members_in_any_order_and_undefined_ones_ignored()
	{
		var patches = new[]
		{
			"""[{ "op": "add", "path": "/a/b/c", "value": "foo" }]""",
			"""[{ "path": "/a/b/c", "op": "add", "value": "foo" }]""",
			"""[{ "value": "foo", "path": "/a/b/c", "op": "add", "from": 1, "from": 2 }]""",
		}
		.Select(JsonPatch.Parse)
		.ToArray();

		Assert.All(patches, patch => Assert.Equal(patches[0], patch));
	}

	[Theory]
	[InlineData("""{ "op": "add", "path": "/a" }""", "'value' is missing")]
	[InlineData("""{ "op": "add", "path": "/a", "value": 1, "value": 2 }""", "'value' is written more than once")]
	[InlineData("""{ "op": "add", "op": "remove", "path": "/a", "value": 1 }""", "'op' is written more than once")]
	[InlineData("""{ "op": "move", "path": "/a" }""", "'from' is missing")]
	[InlineData("""{ "op": "copy", "from": "a", "path": "/a" }""", "'from' is no JSON Pointer")]
	[InlineData("""{ "op": "remove", "path": 1 }""", "'path' is a string")]
	[InlineData("""{ "op": "Add", "path": "/a", "value": 1 }""", "'Add' is no operation")]
	[InlineData("""[]""", "An operation is an object")]
	public void What_is_no_operation(string operation, string reason)
	{
		Assert.False(JsonPatch.TryRead(Both.Json("[" + operation + "]"), out _, out var error));
		Assert.Contains(reason, error!);
	}

	[Fact]
	public void A_patch_document_is_an_array()
	{
		Assert.Throws<JsonPatchException>(() => JsonPatch.Parse("""{ "op": "remove", "path": "" }"""));
	}

	/// <summary>§5: a patch that fails leaves the document as it was, and says which operation failed.</summary>
	[Fact]
	public void A_failed_patch_changes_nothing()
	{
		var document = Both.Json("""{ "a": { "b": { "c": "foo" } } }""");
		var patch    = JsonPatch.Parse("""[{ "op": "replace", "path": "/a/b/c", "value": 42 }, { "op": "test", "path": "/a/b/c", "value": "C" }]""");

		var failure = Assert.Throws<JsonPatchException>(() => patch.Apply(document));

		Assert.StartsWith("Operation 1 ", failure.Message);
		Assert.Equal(Both.Json("""{ "a": { "b": { "c": "foo" } } }"""), document);
	}

	/// <summary>§4.1: adding needs the object or array the new value goes into.</summary>
	[Fact]
	public void Adding_needs_the_parent()
	{
		var patch = JsonPatch.Parse("""[{ "op": "add", "path": "/a/b", "value": 1 }]""");

		Assert.Equal(Both.Json("""{ "a": { "foo": 1, "b": 1 } }"""), patch.Apply(Both.Json("""{ "a": { "foo": 1 } }""")));
		Assert.False(patch.TryApply(Both.Json("""{ "q": { "bar": 2 } }"""), out _, out _));
	}

	/// <summary>§4.4: a location cannot be moved into one of its children; moved onto itself, nothing changes.</summary>
	[Fact]
	public void Moving_into_a_child()
	{
		var document = Both.Json("""{ "a": { "b": 1 } }""");

		Assert.False(JsonPatch.Parse("""[{ "op": "move", "from": "/a", "path": "/a/c" }]""").TryApply(document, out _, out _));
		Assert.Same(document, JsonPatch.Parse("""[{ "op": "move", "from": "/a", "path": "/a" }]""").Apply(document));
	}

	/// <summary>Held for document update, erratum 4787: the whole document cannot be removed.</summary>
	[Fact]
	public void The_whole_document_cannot_be_removed()
	{
		Assert.False(JsonPatch.Parse("""[{ "op": "remove", "path": "" }]""").TryApply(Both.Json("1"), out _, out _));
	}

	/// <summary>A name written twice is a location that does not exist (RFC 6901 §4).</summary>
	[Fact]
	public void A_name_written_twice_is_no_location()
	{
		var document = Both.Json("""{ "a": 1, "a": 2 }""");

		Assert.False(JsonPatch.Parse("""[{ "op": "remove", "path": "/a" }]""").TryApply(document, out _, out _));
		Assert.False(JsonPatch.Parse("""[{ "op": "test", "path": "/a", "value": 1 }]""").TryApply(document, out _, out _));
	}

	[Fact]
	public void What_the_patch_did_not_touch_is_shared()
	{
		var document = (JsonValue.Object)Both.Json("""{ "kept": [1, 2, 3], "changed": { "x": 1 } }""");
		var result   = (JsonValue.Object)JsonPatch.Parse("""[{ "op": "add", "path": "/changed/y", "value": 2 }]""").Apply(document);

		Assert.Same(document.Members[0].Value, result.Members[0].Value);
	}

	// ── §4.6 ─────────────────────────────────────────────────────────────────────

	[Theory]
	[InlineData("1", "1.0")]
	[InlineData("1", "10e-1")]
	[InlineData("100", "1E2")]
	[InlineData("0", "-0.0")]
	[InlineData("-1.50", "-15e-1")]
	[InlineData("123456789012345678901234567890", "1.23456789012345678901234567890e29")]
	[InlineData("""{ "a": 1, "b": [true, null] }""", """{ "b": [true, null], "a": 1.0 }""")]
	[InlineData("""{ "a": 1, "a": 2 }""", """{ "a": 2, "a": 1 }""")]
	[InlineData("\"\\u00e9\"", "\"\u00E9\"")]
	public void Values_that_are_equal(string left, string right)
	{
		Assert.True(JsonPatch.AreEqual(Both.Json(left), Both.Json(right)));
	}

	[Theory]
	[InlineData("1", "\"1\"")]
	[InlineData("1", "-1")]
	[InlineData("123456789012345678901234567890", "123456789012345678901234567891")]
	[InlineData("0.1", "1e-2")]
	[InlineData("[1, 2]", "[2, 1]")]
	[InlineData("""{ "a": 1 }""", """{ "a": 1, "b": 2 }""")]
	[InlineData("""{ "a": 1, "a": 1 }""", """{ "a": 1, "b": 1 }""")]
	[InlineData("null", "false")]
	[InlineData("\"e\\u0301\"", "\"\\u00e9\"")]
	public void Values_that_are_not_equal(string left, string right)
	{
		Assert.False(JsonPatch.AreEqual(Both.Json(left), Both.Json(right)));
	}

	// ── The suite's files ────────────────────────────────────────────────────────

	static readonly Dictionary<string, JsonValue> Suites = [];

	static JsonValue Suite(string file)
	{
		lock (Suites)
		{
			if (!Suites.TryGetValue(file, out var suite))
				Suites.Add(file, suite = Both.Json(File.ReadAllText(Path.Combine(Path.GetDirectoryName(ThisFile)!, "JsonPatch", file))));

			return suite;
		}
	}

	static JsonValue? Member(JsonValue record, string name) =>
		((JsonValue.Object)record).Members.FirstOrDefault(member => member.Key == name).Value;

	static string ThisFile { get; } = FilePath();

	static string FilePath([CallerFilePath] string path = "") => path;
}
