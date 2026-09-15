using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;

using DotGram.Web;

using Xunit;

namespace DotGram.Tests.Web;

/// <summary>
/// RFC 9651, held to the test suite the HTTP working group keeps for it.
/// </summary>
/// <remarks>
/// <para>
/// The cases are the community's, not mine: <c>StructuredFields/</c> is
/// httpwg/structured-field-tests as it stood at 1e280c3, under its own licence beside it.
/// A parser that claims a specification is worth what it does with the cases that
/// specification's implementers agreed on.
/// </para>
/// <para>
/// A case that <c>must_fail</c> has to be refused. One that <c>can_fail</c> is a SHOULD — a
/// base64 body without its padding, a String split across two lines — and may be refused;
/// read, it has to read as expected. Every other case has to read, and to read as expected.
/// What <c>canonical</c> says is serialization's, which this does not do yet.
/// </para>
/// </remarks>
public sealed class Rfc9651Tests
{
	[Theory]
	[MemberData(nameof(Cases))]
	public void Every_case_of_the_suite_reads_as_the_suite_says(string file, int index)
	{
		var test     = Load(file)[index];
		var name     = test.GetProperty("name").GetString();
		var field    = Rfc9651.Combine(test.GetProperty("raw").EnumerateArray().Select(line => line.GetString()!));
		var kind     = test.GetProperty("header_type").GetString();
		var mustFail = test.TryGetProperty("must_fail", out var must) && must.GetBoolean();
		var canFail  = test.TryGetProperty("can_fail", out var can) && can.GetBoolean();

		var (read, value) = kind switch
		{
			"item"       => Read(Rfc9651.TryParseItem(field)),
			"list"       => Read(Rfc9651.TryParseList(field)),
			"dictionary" => Read(Rfc9651.TryParseDictionary(field)),
			_            => throw new InvalidOperationException($"{file}: '{name}' is of a kind nobody told this about: {kind}."),
		};

		if (mustFail)
		{
			Assert.False(read, $"{file}: '{name}' must be refused, and was read.");
			return;
		}

		if (!read)
		{
			Assert.True(canFail, $"{file}: '{name}' must be read, and was refused.");
			return;
		}

		var expected = test.GetProperty("expected");
		var problem  = kind switch
		{
			"item"       => SameItem(expected, (Item)value!),
			"list"       => SameList(expected, (IReadOnlyList<Member>)value!),
			_            => SameDictionary(expected, (OrderedMap<Member>)value!),
		};

		Assert.True(problem is null, $"{file}: '{name}': {problem}");
	}

	/// <summary>Where a Dictionary is read by position and by key, and a key written twice is last.</summary>
	[Fact]
	public void A_dictionary_is_read_by_position_and_by_key()
	{
		var dictionary = Rfc9651.ParseDictionary("a=1, b;q=?0, a=(x y);z");

		Assert.Equal(["a", "b"], dictionary.Select(entry => entry.Key));

		var inner = Assert.IsType<InnerList>(dictionary["a"]);

		Assert.Equal(["x", "y"], inner.Items.Select(item => ((BareItem.Token)item.Value).Value));
		Assert.Equal(BareItem.Boolean.True, inner.Parameters["z"]);
		Assert.Equal(BareItem.Boolean.False, dictionary[1].Value.Parameters["q"]);
	}

	/// <summary>And a field in several lines is the lines joined, as §4.2 has them joined.</summary>
	[Fact]
	public void Field_lines_are_one_value()
	{
		var list = Rfc9651.ParseList(Rfc9651.Combine(["sugar, tea", "rum"]));

		Assert.Equal(["sugar", "tea", "rum"], list.Select(member => ((BareItem.Token)((Item)member).Value).Value));
	}

	/// <summary>
	/// What the suite reads serializes back to its canonical form (§4.1): <c>canonical</c> where
	/// the suite gives one, and the field as written where it does not.
	/// </summary>
	/// <remarks>
	/// Built from what the suite expects rather than from what was read, so a serialization
	/// fault is not hidden behind a parse that happened to agree with it.
	/// </remarks>
	[Theory]
	[MemberData(nameof(Cases))]
	public void What_the_suite_reads_serializes_to_its_canonical_form(string file, int index)
	{
		var test = Load(file)[index];

		if (test.TryGetProperty("must_fail", out var must) && must.GetBoolean())
			return;

		var canonical = test.TryGetProperty("canonical", out var given)
			? string.Join(", ", given.EnumerateArray().Select(line => line.GetString()))
			: test.GetProperty("raw")[0].GetString();

		Assert.Equal(canonical, Serialize(test));
	}

	/// <summary>The suite's own serialization cases: values nothing could read, which must be refused.</summary>
	[Theory]
	[MemberData(nameof(SerializationCases))]
	public void Every_serialization_case_of_the_suite_serializes_as_the_suite_says(string file, int index)
	{
		var test = Load(file)[index];
		var name = test.GetProperty("name").GetString();

		if (test.TryGetProperty("must_fail", out var must) && must.GetBoolean())
		{
			Assert.Throws<ArgumentException>(() => Serialize(test));
			return;
		}

		Assert.True(
			test.GetProperty("canonical")[0].GetString() == Serialize(test),
			$"{file}: '{name}' serialized as '{Serialize(test)}'.");
	}

	/// <summary>What is read from the same field is equal, lists, maps and bytes included.</summary>
	[Fact]
	public void Values_are_equal_by_what_they_hold()
	{
		const string Field = "a=(1 2);x=?0, b=:aGVsbG8=:, c=%\"f%c3%bc\";y";

		var first  = Rfc9651.ParseDictionary(Field);
		var second = Rfc9651.ParseDictionary(Field);

		Assert.True(first.Equals(second));
		Assert.Equal(first.GetHashCode(), second.GetHashCode());
		Assert.Equal(first["a"], second["a"]);
		Assert.Equal(new BareItem.ByteSequence([1, 2]), new BareItem.ByteSequence([1, 2]));

		Assert.False(first.Equals(Rfc9651.ParseDictionary("a=(1 3);x=?0, b=:aGVsbG8=:, c=%\"f%c3%bc\";y")));
		Assert.False(Rfc9651.ParseDictionary("a, b").Equals(Rfc9651.ParseDictionary("b, a")));
	}

	/// <summary>The serialization the package's README shows, which has to stay true and compile.</summary>
	[Fact]
	public void The_readme_serialization_writes_what_it_says()
	{
		var priority = new OrderedMap<Member>(
		[
			new("u", new Item(new BareItem.Integer(3), new OrderedMap<BareItem>([]))),
			new("i", new Item(BareItem.Boolean.True, new OrderedMap<BareItem>([]))),
		]);

		Assert.Equal("u=3, i", Rfc9651.SerializeDictionary(priority));
	}

	public static TheoryData<string, int> Cases => Suited("");

	public static TheoryData<string, int> SerializationCases => Suited("serialisation-tests");

	static TheoryData<string, int> Suited(string directory)
	{
		var cases = new TheoryData<string, int>();

		foreach (var path in Directory.GetFiles(Path.Combine(Suite, directory), "*.json").OrderBy(one => one, StringComparer.Ordinal))
		{
			var file = directory.Length == 0 ? Path.GetFileName(path) : directory + "/" + Path.GetFileName(path);

			for (var index = 0; index < Load(file).Count; index++)
				cases.Add(file, index);
		}

		return cases;
	}

	// ── The suite's JSON as a value to serialize ─────────────────────────────────

	static string Serialize(JsonElement test)
	{
		var expected = test.GetProperty("expected");

		return test.GetProperty("header_type").GetString() switch
		{
			"item" => Rfc9651.SerializeItem(ToItem(expected)),
			"list" => Rfc9651.SerializeList([.. expected.EnumerateArray().Select(ToMember)]),
			_      => Rfc9651.SerializeDictionary(new OrderedMap<Member>(
				expected.EnumerateArray().Select(entry => new KeyValuePair<string, Member>(entry[0].GetString()!, ToMember(entry[1]))))),
		};
	}

	static Member ToMember(JsonElement member) =>
		member[0].ValueKind == JsonValueKind.Array
			? new InnerList([.. member[0].EnumerateArray().Select(ToItem)], ToParameters(member[1]))
			: ToItem(member);

	static Item ToItem(JsonElement item) =>
		new(ToBare(item[0]), ToParameters(item[1]));

	static OrderedMap<BareItem> ToParameters(JsonElement parameters) =>
		new(parameters.EnumerateArray().Select(entry => new KeyValuePair<string, BareItem>(entry[0].GetString()!, ToBare(entry[1]))));

	static BareItem ToBare(JsonElement value) => value.ValueKind switch
	{
		JsonValueKind.True   => BareItem.Boolean.True,
		JsonValueKind.False  => BareItem.Boolean.False,
		JsonValueKind.String => new BareItem.String(value.GetString()!),

		JsonValueKind.Number => value.GetRawText().IndexOfAny(['.', 'e', 'E']) >= 0
			? new BareItem.Decimal(value.GetDecimal())
			: new BareItem.Integer(value.GetInt64()),

		_ => value.GetProperty("__type").GetString() switch
		{
			"token"         => new BareItem.Token(value.GetProperty("value").GetString()!),
			"binary"        => new BareItem.ByteSequence(Base32(value.GetProperty("value").GetString()!)),
			"date"          => new BareItem.Date(value.GetProperty("value").GetInt64()),
			"displaystring" => new BareItem.DisplayString(value.GetProperty("value").GetString()!),
			var other       => throw new InvalidOperationException($"A value of a type nobody told this about: {other}."),
		},
	};

	static (bool Read, object? Value) Read<T>(Rfc9651.Match<T> match) =>
		(match.IsSuccess, match.IsSuccess ? match.Value : null);

	// ── The suite's JSON, held against what was read ─────────────────────────────

	static string? SameList(JsonElement expected, IReadOnlyList<Member> list)
	{
		if (expected.GetArrayLength() != list.Count)
			return $"{list.Count} members where {expected.GetArrayLength()} were expected";

		var index = 0;

		foreach (var member in expected.EnumerateArray())
			if (SameMember(member, list[index++]) is { } problem)
				return $"member {index - 1}: {problem}";

		return null;
	}

	static string? SameDictionary(JsonElement expected, OrderedMap<Member> dictionary)
	{
		if (expected.GetArrayLength() != dictionary.Count)
			return $"{dictionary.Count} members where {expected.GetArrayLength()} were expected";

		var index = 0;

		foreach (var entry in expected.EnumerateArray())
		{
			var (key, member) = dictionary[index++];

			if (entry[0].GetString() != key)
				return $"key '{key}' where '{entry[0].GetString()}' was expected";

			if (SameMember(entry[1], member) is { } problem)
				return $"'{key}': {problem}";
		}

		return null;
	}

	static string? SameMember(JsonElement expected, Member member) =>
		expected[0].ValueKind == JsonValueKind.Array
			? member is InnerList inner ? SameInnerList(expected, inner) : $"an item where an inner list was expected"
			: member is Item item ? SameItem(expected, item) : $"an inner list where an item was expected";

	static string? SameInnerList(JsonElement expected, InnerList inner)
	{
		var items = expected[0];

		if (items.GetArrayLength() != inner.Items.Count)
			return $"{inner.Items.Count} items where {items.GetArrayLength()} were expected";

		var index = 0;

		foreach (var item in items.EnumerateArray())
			if (SameItem(item, inner.Items[index++]) is { } problem)
				return $"item {index - 1}: {problem}";

		return SameParameters(expected[1], inner.Parameters);
	}

	static string? SameItem(JsonElement expected, Item item) =>
		SameBare(expected[0], item.Value) ?? SameParameters(expected[1], item.Parameters);

	static string? SameParameters(JsonElement expected, OrderedMap<BareItem> parameters)
	{
		if (expected.GetArrayLength() != parameters.Count)
			return $"{parameters.Count} parameters where {expected.GetArrayLength()} were expected";

		var index = 0;

		foreach (var parameter in expected.EnumerateArray())
		{
			var (key, value) = parameters[index++];

			if (parameter[0].GetString() != key)
				return $"parameter '{key}' where '{parameter[0].GetString()}' was expected";

			if (SameBare(parameter[1], value) is { } problem)
				return $"parameter '{key}': {problem}";
		}

		return null;
	}

	static string? SameBare(JsonElement expected, BareItem value)
	{
		var same = expected.ValueKind switch
		{
			JsonValueKind.True   => value is BareItem.Boolean { Value: true },
			JsonValueKind.False  => value is BareItem.Boolean { Value: false },
			JsonValueKind.String => value is BareItem.String text && text.Value == expected.GetString(),

			// A number is a Decimal where the suite wrote a point, and an Integer where it did not.
			JsonValueKind.Number => expected.GetRawText().IndexOfAny(['.', 'e', 'E']) >= 0
				? value is BareItem.Decimal fraction && fraction.Value == expected.GetDecimal()
				: value is BareItem.Integer whole && whole.Value == expected.GetInt64(),

			JsonValueKind.Object => expected.GetProperty("__type").GetString() switch
			{
				"token"         => value is BareItem.Token token && token.Value == expected.GetProperty("value").GetString(),
				"binary"        => value is BareItem.ByteSequence bytes && bytes.Value.SequenceEqual(Base32(expected.GetProperty("value").GetString()!)),
				"date"          => value is BareItem.Date date && date.Value == expected.GetProperty("value").GetInt64(),
				"displaystring" => value is BareItem.DisplayString display && display.Value == expected.GetProperty("value").GetString(),
				var other       => throw new InvalidOperationException($"A value of a type nobody told this about: {other}."),
			},

			_ => throw new InvalidOperationException($"A value nobody told this about: {expected.GetRawText()}."),
		};

		return same ? null : $"{value} where {expected.GetRawText()} was expected";
	}

	/// <summary>RFC 4648 §6, which is how the suite writes bytes.</summary>
	static byte[] Base32(string text)
	{
		const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

		var bytes  = new List<byte>();
		var buffer = 0;
		var bits   = 0;

		foreach (var character in text.TrimEnd('='))
		{
			buffer = (buffer << 5) | Alphabet.IndexOf(character);
			bits  += 5;

			if (bits >= 8)
			{
				bits -= 8;
				bytes.Add((byte)(buffer >> bits));
				buffer &= (1 << bits) - 1;
			}
		}

		return [.. bytes];
	}

	static List<JsonElement> Load(string file)
	{
		lock (Loaded)
		{
			if (!Loaded.TryGetValue(file, out var cases))
			{
				using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(Suite, file)));

				Loaded[file] = cases = [.. document.RootElement.EnumerateArray().Select(one => one.Clone())];
			}

			return cases;
		}
	}

	static readonly Dictionary<string, List<JsonElement>> Loaded = new(StringComparer.Ordinal);

	static string Suite =>
		Path.Combine(Path.GetDirectoryName(ThisFile)!, "StructuredFields");

	static string ThisFile { get; } = FilePath();

	static string FilePath([CallerFilePath] string path = "") => path;
}
