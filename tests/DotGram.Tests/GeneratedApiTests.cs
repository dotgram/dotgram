using System;
using System.Collections.Generic;
using System.Linq;

using DotGram;

using Xunit;

namespace DotGram.Tests.Generated;

/// <summary>
/// The grammar attached to a class, compiled by the generator during this project's
/// own build, and called as ordinary C#.
/// </summary>
/// <remarks>
/// <para>
/// The class is named for the grammar rather than for the rule, the way docs/syntax.md
/// §9 names one: the rule <c>Url</c> becomes a type, and a type may not be named after
/// the class that contains it. Nothing below uses reflection — <c>ParseUrl</c> and
/// every property it hands back are resolved by the compiler, and if the generator
/// stopped producing one of them this file would not build.
/// </para>
/// <para>
/// That is the point of it. The other levels compile a grammar by calling into the
/// pipeline and inspect what comes back; this one is a consumer, and it fails the way
/// a consumer would — at their build, not in an assertion.
/// </para>
/// </remarks>
[Gram("Url.gram")]
public partial class UrlGrammar;

public sealed class GeneratedApiTests
{
	[Fact]
	public void The_generated_api_exists_and_is_callable() =>
		Assert.Equal("example.com", UrlGrammar.ParseUrl("https://example.com/a").Authority.Host);

	[Fact]
	public void Parse_throws_the_type_int_Parse_throws() =>
		Assert.Throws<FormatException>(static () => UrlGrammar.ParseUrl("not a url"));

	[Fact]
	public void Try_parse_answers_instead_of_throwing()
	{
		var match = UrlGrammar.TryParseUrl("not a url");

		Assert.False(match.IsSuccess);
		Assert.Null(match.Value);
		Assert.NotNull(match.Error);
	}

	[Fact]
	public void Find_hands_back_a_sequence_and_leaves_the_picking_to_linq()
	{
		var found = UrlGrammar.AllUrls("see http://a.io and https://b.io/c okay").ToList();

		Assert.Equal(["a.io", "b.io"], found.Select(m => m.Value!.Authority.Host));
		Assert.Equal(["",     "/c"],   found.Select(m => m.Value!.Path));

		// Where each one was, which is most of what anybody finds things for.
		Assert.Equal([4, 20], found.Select(m => m.Position));
		Assert.Equal([11, 14], found.Select(m => m.Length));

		Assert.Equal(
			"a.io",
			UrlGrammar.AllUrls("see http://a.io and more").First().Value!.Authority.Host);
	}

	/// <summary>
	/// Every part of a URL, read off the result by name. This is the question a named
	/// group in a regular expression answers with <c>Groups["scheme"].Value</c>, and the
	/// difference is that a wrong name here does not compile.
	/// </summary>
	[Fact]
	public void Every_capture_is_a_property_of_the_declared_type()
	{
		var url = UrlGrammar.ParseUrl("https://user:secret@example.com:8080/a/b?q=1#top");

		Assert.Equal("https",       url.Scheme);
		Assert.Equal("/a/b",        url.Path);
		Assert.Equal("q=1",         url.Query);
		Assert.Equal("top",         url.Fragment);
		Assert.Equal("user:secret", url.Authority.User);
		Assert.Equal("example.com", url.Authority.Host);
		Assert.Equal("8080",        url.Authority.Port);
	}

	/// <summary>
	/// A capture that was never reached is null, not empty: the two are different
	/// answers, and only the grammar knows which one it gave.
	/// </summary>
	[Fact]
	public void What_did_not_match_is_null()
	{
		var url = UrlGrammar.ParseUrl("http://example.com");

		Assert.Null(url.Query);
		Assert.Null(url.Fragment);
		Assert.Null(url.Authority.User);
		Assert.Null(url.Authority.Port);

		// Path is not optional — `('/' & Segment)*` matches, having consumed nothing.
		Assert.Equal("", url.Path);
	}

	[Fact]
	public void The_signatures_are_bcl_types_only_but_for_the_grammar_s_own_types()
	{
		// docs/syntax.md §6.2: the shared support types are emitted internal, so none of
		// them may appear in a public signature. A rule's own type and Match<T> are
		// generated into this assembly from this grammar and have no version to skew, so
		// they can — and the rest is BCL. Checked by asking the compiler: the call would
		// not bind otherwise.
		UrlGrammar.Url                             parsed = UrlGrammar.ParseUrl("ftp://example.com");
		UrlGrammar.Match<UrlGrammar.Url>           match  = UrlGrammar.TryParseUrl("ftp://example.com");
		IEnumerable<UrlGrammar.Match<UrlGrammar.Url>> found = UrlGrammar.AllUrls("ftp://example.com");

		bool    ok       = match.IsSuccess;
		string? error    = match.Error;
		int     position = match.Position;

		Assert.True(ok);
		Assert.Null(error);
		Assert.Equal(0, position);
		Assert.Equal(parsed.Authority.Host, match.Value!.Authority.Host);
		Assert.Single(found);
	}
}
