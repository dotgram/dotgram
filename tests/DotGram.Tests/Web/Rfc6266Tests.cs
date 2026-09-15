using System;

using DotGram.Web;

using Xunit;

namespace DotGram.Tests.Web;

/// <summary>
/// RFC 6266's Content-Disposition header field, held to the examples of its §5 and to Julian Reschke's
/// tc2231 test cases.
/// </summary>
/// <remarks>
/// <para>
/// The test cases at <c>http://test.greenbytes.de/tech/tc2231/</c> are browser tests of the field, by one of
/// RFC 6266's authors. The page carries no licence, so nothing of it is copied but each case's name and the
/// header field value it sends, read from its <c>.asis</c> page as ISO-8859-1 and written here with
/// <c>\u</c> escapes. <c>attonly403</c> is <c>attonly</c> with another status and is left out.
/// </para>
/// <para>
/// Where a case expects a browser to recover a filename from an invalid field, RFC 6266 §3 allows that and
/// does not ask it; this reader follows the ABNF, and the field is refused. The cases that differ so are
/// marked beside them. RFC 2231's continuations are not RFC 6266's: <c>filename*0</c> and the rest are
/// extension parameters, and they give no filename.
/// </para>
/// </remarks>
public sealed class Rfc6266Tests
{
	// ── §5 ───────────────────────────────────────────────────────────────────────

	[Fact]
	public void An_attachment_with_a_filename()
	{
		var field = ContentDisposition.Parse("Attachment; filename=example.html");

		Assert.True(field.IsAttachment);
		Assert.Equal("example.html", field.Filename);
	}

	[Fact]
	public void Inline_with_a_quoted_filename()
	{
		var field = ContentDisposition.Parse("INLINE; FILENAME= \"an example.html\"");

		Assert.True(field.IsInline);
		Assert.Equal("an example.html", field.Filename);
	}

	[Fact]
	public void A_filename_past_ISO_8859_1()
	{
		var field = ContentDisposition.Parse("attachment; filename*= UTF-8''%e2%82%ac%20rates");

		Assert.Equal("\u20AC rates", field.Filename);
		Assert.Equal(new ExtendedValue("UTF-8", null, "\u20AC rates"), field.Find("FILENAME*")!.Extended);
	}

	/// <summary>§4.3: where both are present, <c>filename*</c> is the one taken.</summary>
	[Fact]
	public void Both_filename_parameters()
	{
		// The example's line breaks unfolded to the space a field value carries.
		var field = ContentDisposition.Parse("attachment; filename=\"EURO rates\"; filename*=utf-8''%e2%82%ac%20rates");

		Assert.Equal("\u20AC rates", field.Filename);
		Assert.Equal("EURO rates", field.Find("filename")!.Value);
	}

	[Fact]
	public void A_filename_star_that_does_not_decode_gives_way()
	{
		var field = ContentDisposition.Parse("attachment; filename*=ISO-8859-15''euro%A4; filename=euro");

		Assert.Equal("euro", field.Filename);
		Assert.Null(field.Find("filename*")!.Extended!.Value);
	}

	[Fact]
	public void Equal_whatever_the_case_of_type_and_names()
	{
		var one   = ContentDisposition.Parse("attachment; filename=foo.html");
		var other = ContentDisposition.Parse(" ATTACHMENT ;FILENAME = \"foo.html\" ");

		Assert.Equal(one, other);
		Assert.Equal(one.GetHashCode(), other.GetHashCode());
		Assert.NotEqual(one, ContentDisposition.Parse("attachment; filename=FOO.html"));
	}

	// ── tc2231 ───────────────────────────────────────────────────────────────────

	/// <summary>A case whose field is valid: the type it reads, whether that is an attachment, and the filename.</summary>
	[Theory]
	[InlineData("inlonly", "inline", null)]
	[InlineData("inlwithasciifilename", "inline; filename=\"foo.html\"", "foo.html")]
	[InlineData("inlwithfnattach", "inline; filename=\"Not an attachment!\"", "Not an attachment!")]
	[InlineData("inlwithasciifilenamepdf", "inline; filename=\"foo.pdf\"", "foo.pdf")]
	[InlineData("attonly", "attachment", null)]
	[InlineData("attonlyucase", "ATTACHMENT", null)]
	[InlineData("attwithasciifilename", "attachment; filename=\"foo.html\"", "foo.html")]
	[InlineData("attwithasciifilename25", "attachment; filename=\"0000000000111111111122222\"", "0000000000111111111122222")]
	[InlineData("attwithasciifilename35", "attachment; filename=\"00000000001111111111222222222233333\"", "00000000001111111111222222222233333")]
	[InlineData("attwithasciifnescapedchar", "attachment; filename=\"f\\oo.html\"", "foo.html")]
	[InlineData("attwithasciifnescapedquote", "attachment; filename=\"\\\"quoting\\\" tested.html\"", "\"quoting\" tested.html")]
	[InlineData("attwithquotedsemicolon", "attachment; filename=\"Here's a semicolon;.html\"", "Here's a semicolon;.html")]
	[InlineData("attwithfilenameandextparam", "attachment; foo=\"bar\"; filename=\"foo.html\"", "foo.html")]
	[InlineData("attwithfilenameandextparamescaped", "attachment; foo=\"\\\"\\\\\";filename=\"foo.html\"", "foo.html")]
	[InlineData("attwithasciifilenameucase", "attachment; FILENAME=\"foo.html\"", "foo.html")]
	[InlineData("attwithasciifilenamenq", "attachment; filename=foo.html", "foo.html")]
	[InlineData("attwithfntokensq", "attachment; filename='foo.bar'", "'foo.bar'")]
	[InlineData("attwithisofnplain", "attachment; filename=\"foo-\u00E4.html\"", "foo-\u00E4.html")]
	[InlineData("attwithutf8fnplain", "attachment; filename=\"foo-\u00C3\u00A4.html\"", "foo-\u00C3\u00A4.html")]
	[InlineData("attwithfnrawpctenca", "attachment; filename=\"foo-%41.html\"", "foo-%41.html")]
	[InlineData("attwithfnusingpct", "attachment; filename=\"50%.html\"", "50%.html")]
	[InlineData("attwithfnrawpctencaq", "attachment; filename=\"foo-%\\41.html\"", "foo-%41.html")]
	[InlineData("attwithnamepct", "attachment; name=\"foo-%41.html\"", null)]
	[InlineData("attwithfilenamepctandiso", "attachment; filename=\"\u00E4-%41.html\"", "\u00E4-%41.html")]
	[InlineData("attwithfnrawpctenclong", "attachment; filename=\"foo-%c3%a4-%e2%82%ac.html\"", "foo-%c3%a4-%e2%82%ac.html")]
	[InlineData("attwithasciifilenamews1", "attachment; filename =\"foo.html\"", "foo.html")]
	[InlineData("attconfusedparam", "attachment; xfilename=foo.html", null)]
	[InlineData("attabspath", "attachment; filename=\"/foo.html\"", "/foo.html")]
	[InlineData("attabspathwin", "attachment; filename=\"\\\\foo.html\"", "\\foo.html")]
	[InlineData("attcdate", "attachment; creation-date=\"Wed, 12 Feb 1997 16:29:51 -0500\"", null)]
	[InlineData("attmdate", "attachment; modification-date=\"Wed, 12 Feb 1997 16:29:51 -0500\"", null)]
	[InlineData("dispext", "foobar", null)]
	[InlineData("dispextbadfn", "attachment; example=\"filename=example.txt\"", null)]
	[InlineData("attwithisofn2231iso", "attachment; filename*=iso-8859-1''foo-%E4.html", "foo-\u00E4.html")]
	[InlineData("attwithfn2231utf8", "attachment; filename*=UTF-8''foo-%c3%a4-%e2%82%ac.html", "foo-\u00E4-\u20AC.html")]
	[InlineData("attwithfn2231utf8comp", "attachment; filename*=UTF-8''foo-a%cc%88.html", "foo-a\u0308.html")]
	[InlineData("attwithfn2231ws2", "attachment; filename*= UTF-8''foo-%c3%a4.html", "foo-\u00E4.html")]
	[InlineData("attwithfn2231ws3", "attachment; filename* =UTF-8''foo-%c3%a4.html", "foo-\u00E4.html")]
	[InlineData("attwithfn2231dpct", "attachment; filename*=UTF-8''A-%2541.html", "A-%41.html")]
	[InlineData("attwithfn2231abspathdisguised", "attachment; filename*=UTF-8''%5cfoo.html", "\\foo.html")]
	[InlineData("attfncont", "attachment; filename*0=\"foo.\"; filename*1=\"html\"", null)]
	[InlineData("attfncontqs", "attachment; filename*0=\"foo\"; filename*1=\"\\b\\a\\r.html\"", null)]
	[InlineData("attfncontenc", "attachment; filename*0*=UTF-8''foo-%c3%a4; filename*1=\".html\"", null)]
	[InlineData("attfncontlz", "attachment; filename*0=\"foo\"; filename*01=\"bar\"", null)]
	[InlineData("attfncontnc", "attachment; filename*0=\"foo\"; filename*2=\"bar\"", null)]
	[InlineData("attfnconts1", "attachment; filename*1=\"foo.\"; filename*2=\"html\"", null)]
	[InlineData("attfncontord", "attachment; filename*1=\"bar\"; filename*0=\"foo\"", null)]
	[InlineData("attfnboth", "attachment; filename=\"foo-ae.html\"; filename*=UTF-8''foo-%c3%a4.html", "foo-\u00E4.html")]
	[InlineData("attfnboth2", "attachment; filename*=UTF-8''foo-%c3%a4.html; filename=\"foo-ae.html\"", "foo-\u00E4.html")]
	[InlineData("attfnboth3", "attachment; filename*0*=ISO-8859-15''euro-sign%3d%a4; filename*=ISO-8859-1''currency-sign%3d%a4", "currency-sign=\u00A4")]
	[InlineData("attnewandfn", "attachment; foobar=x; filename=\"foo.html\"", "foo.html")]
	[InlineData("attrfc2047quoted", "attachment; filename=\"=?ISO-8859-1?Q?foo-=E4.html?=\"", "=?ISO-8859-1?Q?foo-=E4.html?=")]
	public void A_valid_case(string name, string value, string? filename)
	{
		Assert.True(ContentDisposition.TryParse(value, out var parsed), $"{name}: '{value}' was not read.");

		var field = parsed!;

		Assert.Equal(value.Split(';')[0].Trim(), field.Type);
		Assert.Equal(!value.StartsWith("inline", StringComparison.Ordinal), field.IsAttachment);
		Assert.Equal(filename, field.Filename);

		// Written back and read again, the field is the same field.
		Assert.Equal(field, ContentDisposition.Parse(field.ToString()));
	}

	/// <summary>A case whose field the ABNF does not make.</summary>
	[Theory]
	[InlineData("inlonlyquoted", "\"inline\"")]
	[InlineData("attonlyquoted", "\"attachment\"")]
	[InlineData("attwithtokfncommanq", "attachment; filename=foo,bar.html")]
	[InlineData("attwithasciifilenamenqs", "attachment; filename=foo.html ;")]
	[InlineData("attemptyparam", "attachment; ;filename=foo")]
	[InlineData("attwithasciifilenamenqws", "attachment; filename=foo bar.html")]
	[InlineData("attwith2filenames", "attachment; filename=\"foo.html\"; filename=\"bar.html\"")]
	[InlineData("attfnbrokentoken", "attachment; filename=foo[1](2).html")]
	[InlineData("attfnbrokentokeniso", "attachment; filename=foo-\u00E4.html")]
	[InlineData("attfnbrokentokenutf", "attachment; filename=foo-\u00C3\u00A4.html")]
	[InlineData("attmissingdisposition", "filename=foo.html")]
	[InlineData("attmissingdisposition2", "x=y; filename=foo.html")]
	[InlineData("attmissingdisposition3", "\"foo; filename=bar;baz\"; filename=qux")]
	[InlineData("attmissingdisposition4", "filename=foo.html, filename=bar.html")]
	[InlineData("emptydisposition", "; filename=foo.html")]
	[InlineData("doublecolon", ": inline; attachment; filename=foo.html")]
	[InlineData("attandinline", "inline; attachment; filename=foo.html")]
	[InlineData("attandinline2", "attachment; inline; filename=foo.html")]
	[InlineData("attbrokenquotedfn", "attachment; filename=\"foo.html\".txt")]
	[InlineData("attbrokenquotedfn2", "attachment; filename=\"bar")]
	[InlineData("attbrokenquotedfn3", "attachment; filename=foo\"bar;baz\"qux")]
	[InlineData("attmultinstances", "attachment; filename=foo.html, attachment; filename=bar.html")]
	[InlineData("attmissingdelim", "attachment; foo=foo filename=bar")]
	[InlineData("attmissingdelim2", "attachment; filename=bar foo=foo ")]
	[InlineData("attmissingdelim3", "attachment filename=bar")]
	[InlineData("attreversed", "filename=foo.html; attachment")]
	[InlineData("attrfc2047token", "attachment; filename==?ISO-8859-1?Q?foo-=E4.html?=")]
	// tc2231 calls what these mean undefined, or recovers the type and ignores the parameter.
	[InlineData("attwithfn2231noc", "attachment; filename*=''foo-%c3%a4-%e2%82%ac.html")]
	[InlineData("attwithfn2231ws1", "attachment; filename *=UTF-8''foo-%c3%a4.html")]
	[InlineData("attwithfn2231quot", "attachment; filename*=\"UTF-8''foo-%c3%a4.html\"")]
	[InlineData("attwithfn2231quot2", "attachment; filename*=\"foo%20bar.html\"")]
	[InlineData("attwithfn2231singleqmissing", "attachment; filename*=UTF-8'foo-%c3%a4.html")]
	[InlineData("attwithfn2231nbadpct1", "attachment; filename*=UTF-8''foo%")]
	[InlineData("attwithfn2231nbadpct2", "attachment; filename*=UTF-8''f%oo.html")]
	public void An_invalid_case(string name, string value)
	{
		Assert.False(ContentDisposition.TryParse(value, out _), $"{name}: '{value}' was read.");
	}

	[Theory]
	[InlineData("attachment; FileName=a; FILENAME=b")]
	[InlineData("attachment; filename=\"\u0100\"")]
	[InlineData("attachment; filename=")]
	[InlineData("attachment;")]
	[InlineData("")]
	public void What_else_is_no_field(string value)
	{
		Assert.False(ContentDisposition.TryParse(value, out _), $"'{value}' was read.");
	}
}
