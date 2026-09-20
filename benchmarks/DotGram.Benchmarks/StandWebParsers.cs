using System;
using System.Collections.Generic;

using DotGram.Web;

namespace DotGram.Benchmarks;

static partial class Stand
{
	/// <summary>
	/// The rest of the web's parsers, of two builds against each other: each side's own
	/// <c>DotGram.Web.dll</c> by reflection, and this process's own build of the same parser as the
	/// constant the pair is held against (there is no hand-written parser for these).
	/// </summary>
	static IEnumerable<Workload> PairedWebParsers(PairedSide before, PairedSide after)
	{
		var rows = new (string Name, string Type, string Method, string Text, bool Accepted, Func<string, bool> Own)[]
		{
			("cookie.full",         "SetCookie",       "TryParse",       CookieFull,  true,  static text => SetCookie.TryParse(text, out _)),
			("cookie.short",        "SetCookie",       "TryParse",       CookieShort, true,  static text => SetCookie.TryParse(text, out _)),
			("pointer.full",        "JsonPointer",     "TryParse",       PointerFull, true,  static text => JsonPointer.TryParse(text, out _)),
			("pointer.short",       "JsonPointer",     "TryParse",       PointerShort, true, static text => JsonPointer.TryParse(text, out _)),
			("media-type.plain",    "MediaType",       "TryParse",       "text/html; charset=utf-8", true, static text => MediaType.TryParse(text, out _)),
			("media-type.quoted",   "MediaType",       "TryParse",       "multipart/form-data; boundary=\"----abc123\"; charset=UTF-8", true, static text => MediaType.TryParse(text, out _)),
			("media-type.refused",  "MediaType",       "TryParse",       "text/", false, static text => MediaType.TryParse(text, out _)),
			("addr-spec.plain",     "AddrSpec",        "TryParseStrict", "user@example.com", true, static text => AddrSpec.TryParseStrict(text, out _)),
			("addr-spec.refused",   "AddrSpec",        "TryParseStrict", "user@@example.com", false, static text => AddrSpec.TryParseStrict(text, out _)),
			("date-time.utc",       "Timestamp",       "TryParse",       "2026-09-18T12:34:56Z", true, static text => Timestamp.TryParse(text, out _)),
			("date-time.offset",    "Timestamp",       "TryParse",       "2026-09-18T12:34:56.789+02:00", true, static text => Timestamp.TryParse(text, out _)),
			("language-tag.plain",   "LanguageTag",     "TryParse",       "en-US", true, static text => LanguageTag.TryParse(text, out _)),
			("language-tag.full",    "LanguageTag",     "TryParse",       "sl-Latn-IT-rozaj-1994-u-co-phonebk-x-private", true, static text => LanguageTag.TryParse(text, out _)),
			("language-tag.refused", "LanguageTag",     "TryParse",       "en--US", false, static text => LanguageTag.TryParse(text, out _)),
			("date-time.refused",   "Timestamp",       "TryParse",       "2026-09-18T12:34:56", false, static text => Timestamp.TryParse(text, out _)),
			("content-disposition.full",    "ContentDisposition", "TryParse",      DispositionFull,    true,  static text => ContentDisposition.TryParse(text, out _)),
			("content-disposition.refused", "ContentDisposition", "TryParse",      DispositionRefused, false, static text => ContentDisposition.TryParse(text, out _)),
			("uri-template.full",           "UriTemplate",        "TryParse",      TemplateFull,       true,  static text => UriTemplate.TryParse(text, out _)),
			("uri-template.refused",        "UriTemplate",        "TryParse",      TemplateRefused,    false, static text => UriTemplate.TryParse(text, out _)),
			("json-patch.full",             "JsonPatch",          "Parse",         PatchFull,          true,  static text => JsonPatchParses(text)),
			("forwarded.full",              "ForwardedElement",   "TryParseField", ForwardedFull,      true,  static text => ForwardedElement.TryParseField(text, out _)),
			("forwarded.refused",           "ForwardedElement",   "TryParseField", ForwardedRefused,   false, static text => ForwardedElement.TryParseField(text, out _)),
			("link.full",                   "WebLink",            "TryParseField", LinkFull,           true,  static text => WebLink.TryParseField(text, out _)),
			("link.refused",                "WebLink",            "TryParseField", LinkRefused,        false, static text => WebLink.TryParseField(text, out _)),
			("cookie.refused",              "SetCookie",          "TryParse",      CookieRefused,      false, static text => SetCookie.TryParse(text, out _)),
			("sf.item",             "StructuredField", "TryParseItem",   "42;unit=\"s\";exact=?1", true, static text => StructuredField.TryParseItem(text, out _)),
			("sf.list",             "StructuredField", "TryParseList",   "\"foo\", bar;baz=?1, (1 2 3);q=0.5, :aGVsbG8=:, 10.5", true, static text => StructuredField.TryParseList(text, out _)),
			("sf.dictionary",       "StructuredField", "TryParseDictionary", "a=1, b=?0, c=\"text\", d=:aGVsbG8=:, e=(1 2);f=3", true, static text => StructuredField.TryParseDictionary(text, out _)),
		};

		foreach (var (name, type, method, text, accepted, own) in rows)
		{
			var b = before.WebTry(type, method, text);
			var a = after.WebTry(type, method, text);

			yield return new Workload("web", name,
				[
					new Reading("control", () => own(text) ? 1 : 0),
					new Reading("before", b),
					new Reading("after",  a),
				],
				() => own(text) == accepted && b() == (accepted ? 1 : 0) && a() == (accepted ? 1 : 0)
					? null
					: $"  the row says it {(accepted ? "accepts" : "refuses")} the text, and this build's own reading {(own(text) ? "accepts" : "refuses")}, before {b()}, after {a()}");
		}
	}
}
