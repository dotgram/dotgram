using System;

using DotGram;

using Xunit;

namespace DotGram.Tests.Generated;

/// <summary>
/// The grammar attached to a class, compiled by the generator during this project's
/// own build, and called as ordinary C#.
/// </summary>
/// <remarks>
/// <para>
/// [Gram] with no argument looks for the file named after the class, and finds
/// ../Snapshots/Url.gram because the project lists it as an additional file. Nothing
/// below uses reflection: <c>ParseUrl</c> is resolved by the compiler, and if the
/// generator stopped producing it this file would not build.
/// </para>
/// <para>
/// That is the point of it. The other levels compile a grammar by calling into the
/// pipeline and inspect what comes back; this one is a consumer, and it fails the way
/// a consumer would — at their build, not in an assertion.
/// </para>
/// </remarks>
[Gram]
public partial class Url;

public sealed class GeneratedApiTests
{
	[Fact]
	public void The_generated_api_exists_and_is_callable() =>
		Assert.Equal("https://example.com/a", Url.ParseUrl("https://example.com/a"));

	[Fact]
	public void Parse_throws_the_type_int_Parse_throws() =>
		Assert.Throws<FormatException>(static () => Url.ParseUrl("not a url"));

	[Fact]
	public void Try_parse_answers_instead_of_throwing()
	{
		Assert.False(Url.TryParseUrl("not a url", out _, out var error, out _));
		Assert.NotNull(error);
	}

	[Fact]
	public void Find_all_returns_an_array_of_what_it_found() =>
		Assert.Equal(
			["http://a.io", "https://b.io/c"],
			Url.AllUrls("see http://a.io and https://b.io/c okay"));

	[Fact]
	public void The_signatures_are_bcl_types_only()
	{
		// docs/syntax.md §6.1: support types are emitted internal, so nothing of ours
		// may appear in a public signature. Checked by asking the compiler — every type
		// named here comes from the BCL, and the call would not bind otherwise.
		string   parsed = Url.ParseUrl("ftp://example.com");
		string[] found  = Url.AllUrls("ftp://example.com");
		bool     ok     = Url.TryParseUrl("ftp://example.com", out string value, out string? error, out int position);

		Assert.True(ok);
		Assert.Null(error);
		Assert.Equal(0, position);
		Assert.Equal(parsed, value);
		Assert.Single(found);
	}
}
