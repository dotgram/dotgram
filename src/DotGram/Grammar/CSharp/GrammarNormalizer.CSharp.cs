using System;

using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Model;

/// <summary>The normalizer as a caller that holds a C# resolver and scanner, or neither, asks for it.</summary>
sealed partial class GrammarNormalizer
{
	/// <param name="model">The grammar as it was bound.</param>
	/// <param name="resolver">
	/// The host, for a caller that holds only a C# resolver: C#'s language is made of it and of
	/// <paramref name="scanner"/>. Null where <paramref name="language"/> is given.
	/// </param>
	/// <param name="scanner">
	/// Reads each embedded C# expression, for the same caller. Optional, and where it is absent
	/// the emitter falls back to searching the spelling — which is what it used to do
	/// always, and what got `@(Log("parserInput"))` counted as asking for the whole input.
	/// </param>
	/// <param name="positionalFollow">What <see cref="RecognitionGraph.PositionalFollow"/> says.</param>
	/// <param name="positionalSplit">What <see cref="RecognitionGraph.PositionalSplit"/> says.</param>
	/// <param name="collapse">
	/// Whether a call to a rule that only forwards another's value becomes the choice it forwards.
	/// </param>
	/// <param name="language">
	/// The language the parser is generated in, which reads the host fragments and answers for the
	/// host itself. Where it is null, C#'s is made of <paramref name="resolver"/> and
	/// <paramref name="scanner"/>.
	/// </param>
	/// <param name="settle">Whether settled copies are made (<see cref="SettleRunTails"/>).</param>
	public static RecognitionGraph Normalize(
		GrammarModel model, ISymbolResolver? resolver = null, ICSharpScanner? scanner = null,
		bool positionalFollow = false, bool positionalSplit = false,
		bool collapse = true, ITargetLanguage? language = null, bool settle = false)
	{
		if (language is not null && (resolver is not null || scanner is not null))
			throw new ArgumentException("A language reads its own host fragments and answers for the host; pass neither a resolver nor a scanner beside it.", nameof(language));

		return NormalizeIn(model, language ?? new CSharpLanguage(resolver, scanner), positionalFollow, positionalSplit, collapse, settle);
	}
}
