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
/// RFC 6570, held to the test suite its implementers keep.
/// </summary>
/// <remarks>
/// <para>
/// <c>UriTemplates/</c> is uri-templates/uritemplate-test as it stood at 4171dac, under its own
/// licence beside it: every example the RFC gives, section by section, the suite's own further
/// examples, and the templates that must be refused.
/// </para>
/// <para>
/// A case expecting <c>false</c> is an error, and the error may be the template's — refused by
/// <see cref="UriTemplate.TryParse"/> — or the expansion's, a prefix on a composite
/// value. A case expecting a list accepts any one of them, which is how the suite says an
/// associative array may be expanded in any order.
/// </para>
/// </remarks>
public sealed class Rfc6570Tests
{
	[Theory]
	[MemberData(nameof(Cases))]
	public void Every_case_of_the_suite_expands_as_the_suite_says(string file, string group, int index)
	{
		var set       = Load(file).GetProperty(group);
		var test      = set.GetProperty("testcases")[index];
		var text      = test[0].GetString()!;
		var expected  = test[1];
		var variables = Variables(set.GetProperty("variables"));

		var accepted = UriTemplate.TryParse(text, out var template);
		var expanded = default(string);

		if (accepted)
		{
			try
			{
				expanded = template!.Expand(variables);
			}
			catch (ArgumentException)
			{
			}
		}

		switch (expected.ValueKind)
		{
			case JsonValueKind.False:
				Assert.True(expanded is null, $"{file}, {group}: '{text}' must be refused, and expanded to '{expanded}'.");
				break;

			case JsonValueKind.String:
				Assert.True(expanded is not null, $"{file}, {group}: '{text}' was refused.");
				Assert.Equal(expected.GetString(), expanded);
				break;

			default:
				Assert.True(expanded is not null, $"{file}, {group}: '{text}' was refused.");
				Assert.Contains(expanded, expected.EnumerateArray().Select(one => one.GetString()));
				break;
		}
	}

	/// <summary>And what a template is made of, which the expansions only show indirectly.</summary>
	[Fact]
	public void A_template_comes_apart_into_literals_and_expressions()
	{
		var template = UriTemplate.Parse("/users{/id}{?q,page:3,tags*}");

		Assert.Collection(
			template.Parts,
			part => Assert.Equal(new UriTemplate.Literal("/users"), part),
			part =>
			{
				var expression = Assert.IsType<UriTemplate.Expression>(part);

				Assert.Equal('/', expression.Operator);
				Assert.Equal([new UriTemplate.Variable("id", null, false)], expression.Variables);
			},
			part =>
			{
				var expression = Assert.IsType<UriTemplate.Expression>(part);

				Assert.Equal('?', expression.Operator);
				Assert.Equal(
					[
						new UriTemplate.Variable("q", null, false),
						new UriTemplate.Variable("page", 3, false),
						new UriTemplate.Variable("tags", null, true),
					],
					expression.Variables);
			});
	}

	/// <summary>A dictionary is an associative array, and a number is its invariant text.</summary>
	[Fact]
	public void Values_are_what_dotnet_has()
	{
		var template = UriTemplate.Parse("/map{?point*,zoom}");

		Assert.Equal(
			"/map?x=1.5&y=-2&zoom=12",
			template.Expand(new Dictionary<string, object?>
			{
				["point"] = new Dictionary<string, string> { ["x"] = "1.5", ["y"] = "-2" },
				["zoom"]  = 12,
			}));
	}

	/// <summary>A template is equal to another read from the same text.</summary>
	[Fact]
	public void Templates_are_equal_by_what_they_hold()
	{
		const string Text = "/users{/id}{?q,page:3,tags*}";

		Assert.Equal(UriTemplate.Parse(Text), UriTemplate.Parse(Text));
		Assert.Equal(UriTemplate.Parse(Text).GetHashCode(), UriTemplate.Parse(Text).GetHashCode());
		Assert.NotEqual(UriTemplate.Parse(Text), UriTemplate.Parse("/users{/id}{?q,page:4,tags*}"));
	}

	/// <summary>The example the package's README shows, which has to stay true.</summary>
	[Fact]
	public void The_readme_example_expands_as_it_says()
	{
		var template = UriTemplate.Parse("/users{/id}{?fields,page:3}{&tags*}");

		Assert.Equal(
			"/users/igor?fields=name,email&page=123&tags=a&tags=b",
			template.Expand(new Dictionary<string, object?>
			{
				["id"]     = "igor",
				["fields"] = new[] { "name", "email" },
				["page"]   = "12345",
				["tags"]   = new[] { "a", "b" },
			}));
	}

	public static TheoryData<string, string, int> Cases
	{
		get
		{
			var cases = new TheoryData<string, string, int>();

			foreach (var path in Directory.GetFiles(Suite, "*.json").OrderBy(one => one, StringComparer.Ordinal))
			{
				var file = Path.GetFileName(path);

				foreach (var group in Load(file).EnumerateObject())
					for (var index = 0; index < group.Value.GetProperty("testcases").GetArrayLength(); index++)
						cases.Add(file, group.Name, index);
			}

			return cases;
		}
	}

	/// <summary>The suite's JSON values as the .NET values <see cref="UriTemplate.Expand"/> takes.</summary>
	static Dictionary<string, object?> Variables(JsonElement variables)
	{
		var values = new Dictionary<string, object?>(StringComparer.Ordinal);

		foreach (var variable in variables.EnumerateObject())
		{
			values[variable.Name] = variable.Value.ValueKind switch
			{
				JsonValueKind.Null   => null,
				JsonValueKind.Array  => variable.Value.EnumerateArray().Select(Scalar).ToArray(),
				JsonValueKind.Object => variable.Value.EnumerateObject().Select(one => new KeyValuePair<string, string>(one.Name, Scalar(one.Value))).ToList(),
				_                    => Scalar(variable.Value),
			};
		}

		return values;

		// A number is the text the suite wrote, which is what a URI is made of.
		static string Scalar(JsonElement value) =>
			value.ValueKind == JsonValueKind.String ? value.GetString()! : value.GetRawText();
	}

	static JsonElement Load(string file)
	{
		lock (Loaded)
		{
			if (!Loaded.TryGetValue(file, out var root))
			{
				using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(Suite, file)));

				Loaded[file] = root = document.RootElement.Clone();
			}

			return root;
		}
	}

	static readonly Dictionary<string, JsonElement> Loaded = new(StringComparer.Ordinal);

	static string Suite =>
		Path.Combine(Path.GetDirectoryName(ThisFile)!, "UriTemplates");

	static string ThisFile { get; } = FilePath();

	static string FilePath([CallerFilePath] string path = "") => path;
}
