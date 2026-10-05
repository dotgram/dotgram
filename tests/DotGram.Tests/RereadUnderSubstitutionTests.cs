using DotGram;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A publication named in an action means it under the same substitution
/// (docs/syntax.md §5.1): an action that reads a piece of the input again with
/// <c>TryParseName</c> reads it with the word its own publication reads with.
/// </summary>
/// <remarks>
/// Two ways of naming it. <c>Quoted</c> names a publication nobody writes under the
/// substitution, which the generator publishes privately for the clone. <c>Braced</c> names
/// one the author does write, which is the one the clone then calls.
/// </remarks>
public sealed class RereadUnderSubstitutionTests
{
	[Gram("""
		trivia = none

		Word      = [\p{L} | '_'] & [\p{L} | \p{Nd} | '_']*
		AsciiWord = ['a'..'z' | 'A'..'Z' | '_'] & ['a'..'z' | 'A'..'Z' | '0'..'9' | '_']*

		Inner = [^ '>' | '}']*

		Name  : @string = w: Word => @(w)
		Label : @string = w: Word => @(w)

		Quoted : @bool = '<' & text: Inner & '>' => @(TryParseName(text).IsSuccess)
		Braced : @bool = '{' & text: Inner & '}' => @(TryParseLabel(text).IsSuccess)
		Either : @bool = Quoted | Braced

		private parse Name as ParseName
		private parse Label as ParseLabel
		private parse Label with (Word = AsciiWord) as ParseAsciiLabel
		public parse Either as ParseEither
		public parse Either with (Word = AsciiWord) as ParseAsciiEither
		""", Lexical = true)]
	static partial class Reread
	{
		// ParseEither and ParseAsciiEither are generated here.
	}

	[Theory]
	[InlineData("<abc>",   true,  true)]
	[InlineData("<naïve>", true,  false)]
	[InlineData("<счёт>",  true,  false)]
	[InlineData("{abc}",   true,  true)]
	[InlineData("{naïve}", true,  false)]
	[InlineData("{счёт}",  true,  false)]
	public void What_is_read_again_is_read_with_the_publications_own_word(string text, bool wide, bool ascii)
	{
		Assert.Equal(wide,  Reread.TryParseEither(text, out var w) && w);
		Assert.Equal(ascii, Reread.TryParseAsciiEither(text, out var a) && a);
	}
}
