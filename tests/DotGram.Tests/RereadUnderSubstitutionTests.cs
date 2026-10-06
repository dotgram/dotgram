using DotGram;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A publication named in an action means it under the same substitution
/// (docs/syntax.md §5.1): an action that reads a piece of the input again with
/// <c>TryParseName</c> reads it with the word its own publication reads with.
/// </summary>
/// <remarks>
/// The text before the bar is read by the rule itself, in the word of the publication, so the
/// substitution reaches <c>Quoted</c> and <c>Braced</c> at all and they are cloned; what follows
/// the bar is read again, and only the publication named in the action can narrow it.
/// Two ways of naming it. <c>Quoted</c> names a publication nobody writes under the
/// substitution, which the generator publishes privately for the clone. <c>Braced</c> names
/// one the author does write, which is the one the clone then calls.
/// </remarks>
public sealed partial class RereadUnderSubstitutionTests
{
	[Gram("""
		trivia = none

		Word      = [\p{L} | '_'] & [\p{L} | \p{Nd} | '_']*
		AsciiWord = ['a'..'z' | 'A'..'Z' | '_'] & ['a'..'z' | 'A'..'Z' | '0'..'9' | '_']*

		Inner = [^ '>' | '}']*

		Name  : @string = w: Word => @(w)
		Label : @string = w: Word => @(w)

		Quoted : @bool = '<' & Word & '|' & text: Inner & '>' => @(TryParseName(text).IsSuccess)
		Braced : @bool = '{' & Word & '|' & text: Inner & '}' => @(TryParseLabel(text).IsSuccess)
		Either : @bool = q: Quoted => @(q) | b: Braced => @(b)

		private parse Name as ParseName
		private parse Label as ParseLabel
		private parse Label with (Word = AsciiWord) as ParseAsciiLabel
		public parse Either as ParseEither
		public parse Either with (Word = AsciiWord) as ParseAsciiEither
		""")]
	static partial class Reread
	{
		// ParseEither and ParseAsciiEither are generated here.
	}

	[Theory]
	[InlineData("<x|abc>",   true,  true)]
	[InlineData("<x|naïve>", true,  false)]
	[InlineData("<x|счёт>",  true,  false)]
	[InlineData("{x|abc}",   true,  true)]
	[InlineData("{x|naïve}", true,  false)]
	[InlineData("{x|счёт}",  true,  false)]
	public void What_is_read_again_is_read_with_the_publications_own_word(string text, bool wide, bool ascii)
	{
		Assert.Equal(wide,  Reread.TryParseEither(text, out var w) && w);
		Assert.Equal(ascii, Reread.TryParseAsciiEither(text, out var a) && a);
	}
}
