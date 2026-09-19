using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

using DotGram;

namespace DotGram.Web;

/// <summary>A language tag as RFC 5646 divides it into subtags.</summary>
/// <remarks>
/// <para>
/// Every part is the subtag as it was written: a tag is case-insensitive (§2.1.1), and case
/// carries no meaning, so nothing is lost by keeping what the author wrote.
/// <see cref="ToString"/> writes the tag in the case the RFC recommends.
/// </para>
/// <para>
/// A tag read here is <i>well-formed</i> (§2.2.9): it follows the ABNF. Whether it is also
/// <i>valid</i> — every subtag in the IANA registry, no variant or extension twice — needs the
/// registry and is not asked.
/// </para>
/// </remarks>
/// <param name="Language">The primary language subtag, or null in a private-use or grandfathered tag.</param>
/// <param name="ExtendedLanguages">Up to three extended language subtags after a short primary one.</param>
/// <param name="Script">A four-letter script subtag, or null.</param>
/// <param name="Region">A two-letter or three-digit region subtag, or null.</param>
/// <param name="Variants">The variant subtags, in order.</param>
/// <param name="Extensions">The extensions, each a singleton and its subtags, in order.</param>
/// <param name="PrivateUse">The subtags after <c>x</c>, without it.</param>
/// <param name="Grandfathered">
/// The whole tag where it is one of the tags registered under RFC 3066 that §2.1 lists by name,
/// and null otherwise. Such a tag means what its registration says, not what its subtags would.
/// </param>
public sealed record LanguageTag(
	string?                   Language,
	IReadOnlyList<string>     ExtendedLanguages,
	string?                   Script,
	string?                   Region,
	IReadOnlyList<string>     Variants,
	IReadOnlyList<LanguageTag.Extension> Extensions,
	IReadOnlyList<string>     PrivateUse,
	string?                   Grandfathered)
{
	/// <summary>A well-formed language tag (RFC 5646 §2.1), whatever its case.</summary>
	/// <exception cref="FormatException">The text is no well-formed tag; the message says where.</exception>
	public static LanguageTag Parse(string text) =>
		Rfc5646.ParseTag(text ?? throw new ArgumentNullException(nameof(text)));

	/// <summary>A well-formed language tag, or false where the text is not one.</summary>
	public static bool TryParse(string text, [NotNullWhen(true)] out LanguageTag? tag)
	{
		var read = Rfc5646.TryParseTag(text ?? throw new ArgumentNullException(nameof(text)), out var parsed);

		tag = read ? parsed : null;

		return read;
	}

	/// <summary>§2.2.6: a single letter or digit other than <c>x</c>, and the subtags it introduces.</summary>
	/// <remarks>Equal to another whatever the case of either, as a tag is (§2.1.1).</remarks>
	public sealed record Extension(char Singleton, IReadOnlyList<string> Subtags)
	{
		public bool Equals(Extension? other) =>
			other is not null && Lower(Singleton) == Lower(other.Singleton) && Structural.Same(Subtags, other.Subtags, Cases);

		public override int GetHashCode() => Structural.Combine(Lower(Singleton), Structural.Hash(Subtags, Cases));
	}

	/// <summary>Equal to another tag whatever the case of either: case carries no meaning in a tag (§2.1.1).</summary>
	/// <remarks>The parts keep the case they were written in; it is only not asked here.</remarks>
	public bool Equals(LanguageTag? other) =>
		other is not null &&
		Cases.Equals(Language, other.Language) &&
		Structural.Same(ExtendedLanguages, other.ExtendedLanguages, Cases) &&
		Cases.Equals(Script, other.Script) &&
		Cases.Equals(Region, other.Region) &&
		Structural.Same(Variants, other.Variants, Cases) &&
		Structural.Same(Extensions, other.Extensions) &&
		Structural.Same(PrivateUse, other.PrivateUse, Cases) &&
		Cases.Equals(Grandfathered, other.Grandfathered);

	public override int GetHashCode()
	{
		var hash = Cases.GetHashCode(Language ?? "");

		hash = Structural.Combine(hash, Structural.Hash(ExtendedLanguages, Cases));
		hash = Structural.Combine(hash, Cases.GetHashCode(Script ?? ""));
		hash = Structural.Combine(hash, Cases.GetHashCode(Region ?? ""));
		hash = Structural.Combine(hash, Structural.Hash(Variants, Cases));
		hash = Structural.Combine(hash, Structural.Hash(Extensions));
		hash = Structural.Combine(hash, Structural.Hash(PrivateUse, Cases));

		return Structural.Combine(hash, Cases.GetHashCode(Grandfathered ?? ""));
	}

	// Subtags are ASCII, so an ordinal comparison without case is ASCII's.
	static readonly StringComparer Cases = StringComparer.OrdinalIgnoreCase;

	/// <summary>The tag in the case §2.1.1 recommends, which is the registry's.</summary>
	/// <remarks>
	/// Lowercase throughout, but for a subtag that neither begins the tag nor follows a singleton:
	/// there two letters are a region and uppercase, and four are a script and titlecase — as in
	/// <c>sgn-BE-FR</c> and <c>az-Latn-x-latn</c>. The casing is ASCII's, whatever the culture.
	/// </remarks>
	public override string ToString()
	{
		var output = new StringBuilder();

		if (Grandfathered is not null)
		{
			Formatted(output, Grandfathered.Split('-'));
			return output.ToString();
		}

		var subtags = new List<string>();

		if (Language is not null)
		{
			subtags.Add(Language);
			subtags.AddRange(ExtendedLanguages);

			if (Script is not null)
				subtags.Add(Script);

			if (Region is not null)
				subtags.Add(Region);

			subtags.AddRange(Variants);

			foreach (var extension in Extensions)
			{
				subtags.Add(extension.Singleton.ToString());
				subtags.AddRange(extension.Subtags);
			}
		}

		if (PrivateUse.Count > 0)
		{
			subtags.Add("x");
			subtags.AddRange(PrivateUse);
		}

		Formatted(output, subtags);

		return output.ToString();
	}

	static void Formatted(StringBuilder output, IReadOnlyList<string> subtags)
	{
		var afterSingleton = false;

		for (var index = 0; index < subtags.Count; index++)
		{
			var subtag = subtags[index];

			if (index > 0)
				output.Append('-');

			var upper = index > 0 && !afterSingleton && subtag.Length == 2;
			var title = index > 0 && !afterSingleton && subtag.Length == 4;

			for (var at = 0; at < subtag.Length; at++)
			{
				var character = subtag[at];

				output.Append(upper || title && at == 0 ? Upper(character) : Lower(character));
			}

			if (subtag.Length == 1)
				afterSingleton = true;
		}
	}

	static char Lower(char c) => c is >= 'A' and <= 'Z' ? (char)(c + ('a' - 'A')) : c;

	static char Upper(char c) => c is >= 'a' and <= 'z' ? (char)(c - ('a' - 'A')) : c;
}

// RFC 5646, Tags for Identifying Languages — BCP 47. The grammar is Figure 1, §2.1's ABNF,
// production for production, and it reads a tag as well-formed. Two things the ABNF says
// without saying them are said here:
//
//   * Case does not matter anywhere (§2.1.1): every letter is either case, and the
//     grandfathered tags are literals compared without it.
//   * A subtag's kind is its length and its content, and a subtag ends at a hyphen or at the
//     end of the tag. ABNF's repetitions can stop anywhere; ordered choice must be told where
//     a subtag stops, or `en-Latn` would read `Lat` as an extended language and leave the `n`.
//     `End` is that: no letter or digit follows.
//
// The grandfathered tags are asked first, and whole: `zh-min-nan` before `zh-min`, and each only
// where the tag ends there, so `zh-min-xyz` goes on to be read as an ordinary tag.

[Gram("""
	@using System;
	@using DotGram.Web;

	trivia = none

	Alpha    = ['a'..'z' | 'A'..'Z']
	Digit    = ['0'..'9']
	Alphanum = [Alpha | Digit]

	// Where a subtag stops: nothing that could go on with it.
	End = ?!Alphanum

	// ── Language-Tag ─────────────────────────────────────────────────────────────

	LanguageTag : @LanguageTag
		= registered: Grandfathered & eof => @(Rfc5646.Registered(registered))
		| tag: LangTag                    => @(tag)
		| subtags: PrivateUse             => @(new LanguageTag(null, [], null, null, [], [], subtags, null))

	// langtag: a short language may carry extended languages, a long one may not.
	LangTag : @LanguageTag
		= language: ShortLanguage & extended: ExtLang{0,3} & tail: Tail => @(tail with { Language = language, ExtendedLanguages = extended })
		| language: LongLanguage & tail: Tail                          => @(tail with { Language = language })

	ShortLanguage = Alpha{2,3} & End

	// Four letters are reserved and five to eight registered: both are a language here.
	LongLanguage = Alpha{4,8} & End

	ExtLang : @string = '-' & subtag: ExtLangText => @(subtag)
	ExtLangText = Alpha{3} & End

	Tail : @LanguageTag
		= ('-' & script: Script)? & ('-' & region: Region)? & variants: Variant* & extensions: Extension* & ('-' & use: PrivateUse)?
		=> @(new LanguageTag(null, [], script, region, variants, extensions, use ?? [], null))

	Script = Alpha{4} & End
	Region = (Alpha{2} | Digit{3}) & End

	// A variant beginning with a digit is four characters at least, one beginning with a letter five.
	Variant : @string = '-' & subtag: VariantText => @(subtag)
	VariantText = (Alphanum{5,8} | Digit & Alphanum{3}) & End

	// ── Extensions and private use ───────────────────────────────────────────────

	Extension : @LanguageTag.Extension
		= '-' & singleton: Singleton & subtags: ExtensionSubtag+
		=> @(new LanguageTag.Extension(singleton[0], subtags))

	// Any letter or digit but `x`, which is private use's.
	Singleton = [Digit | 'a'..'w' | 'y'..'z' | 'A'..'W' | 'Y'..'Z'] & End

	ExtensionSubtag : @string = '-' & subtag: ExtensionText => @(subtag)
	ExtensionText = Alphanum{2,8} & End

	PrivateUse : @string[] = ['x' | 'X'] & End & subtags: PrivateSubtag+ => @(subtags)

	PrivateSubtag : @string = '-' & subtag: PrivateText => @(subtag)
	PrivateText = Alphanum{1,8} & End

	// ── grandfathered ────────────────────────────────────────────────────────────

	// irregular, then regular, as the ABNF lists them — but for `zh-min-nan`, which is tried
	// before the `zh-min` it begins with.
	Grandfathered
		= "en-GB-oed"i | "i-ami"i | "i-bnn"i | "i-default"i | "i-enochian"i | "i-hak"i
		| "i-klingon"i | "i-lux"i | "i-mingo"i | "i-navajo"i | "i-pwn"i | "i-tao"i | "i-tay"i
		| "i-tsu"i | "sgn-BE-FR"i | "sgn-BE-NL"i | "sgn-CH-DE"i
		| "art-lojban"i | "cel-gaulish"i | "no-bok"i | "no-nyn"i | "zh-guoyu"i | "zh-hakka"i
		| "zh-min-nan"i | "zh-min"i | "zh-xiang"i

	parse LanguageTag as ParseTag
	""")]
static partial class Rfc5646
{
	// ParseTag and TryParseTag are generated here.

	internal static LanguageTag Registered(string tag) =>
		new(null, [], null, null, [], [], [], tag);
}
