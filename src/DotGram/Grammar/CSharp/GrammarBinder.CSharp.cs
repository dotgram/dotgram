using System;

using DotGram.Grammar.Parsing;

namespace DotGram.Grammar.Binding;

/// <summary>The binder as a caller that holds a C# resolver, or none, asks for it.</summary>
sealed partial class GrammarBinder
{
	/// <param name="file">The grammar as it was parsed.</param>
	/// <param name="symbols">
	/// The host, for a caller that holds only a C# resolver: C#'s language is made of it. Null
	/// where <paramref name="language"/> is given.
	/// </param>
	/// <param name="own">Where the author's own text ends, where grammars were joined after it.</param>
	/// <param name="language">The language the parser is generated in, which also answers for the host.</param>
	public static GrammarModel Bind(
		GrammarFile file, ISymbolResolver? symbols = null, int? own = null, ITargetLanguage? language = null)
	{
		if (language is not null && symbols is not null)
			throw new ArgumentException("A language answers for the host itself; pass no resolver beside it.", nameof(symbols));

		return BindIn(file, language ?? new CSharpLanguage(symbols), own);
	}
}
