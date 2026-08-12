using System;
using System.Collections.Generic;
using System.Text;

namespace DotGram.Grammar.Parsing;

/// <summary>
/// One lexeme: what it is, where it came from, and — for the kinds that carry one —
/// its decoded value.
/// </summary>
/// <param name="Value">
/// Decoded, not the source slice: escape sequences are already resolved, quotes and
/// delimiters removed. Null for kinds with a fixed spelling. The source slice is
/// still recoverable from <paramref name="Position"/> and <paramref name="Length"/>.
/// </param>
public readonly record struct Token(TokenKind Kind, int Position, int Length, string? Value)
{
	/// <summary>
	/// Quotes and backslashes in the value are escaped, so a value that itself
	/// contains a quote cannot be read as the end of one.
	/// </summary>
	public override string ToString() =>
		Value is null
			? Kind.ToString()
			: $"{Kind} \"{Value.Replace("\\", @"\\").Replace("\"", "\\\"")}\"";
}

/// <summary>The result of lexing: the tokens, and what went wrong producing them.</summary>
public sealed class TokenList(
	IReadOnlyList<Token>          tokens,
	IReadOnlyList<GramDiagnostic> diagnostics)
{
	public IReadOnlyList<Token>          Tokens      { get; } = tokens;
	public IReadOnlyList<GramDiagnostic> Diagnostics { get; } = diagnostics;

	public int     Count           => Tokens.Count;
	public Token   this[int index] => Tokens[index];
	public bool    HasErrors       => Diagnostics.Count > 0;

	/// <summary>
	/// One token per line, kind first, value quoted when there is one.
	/// </summary>
	/// <remarks>
	/// This is the stage's own contract made visible: a lexing regression shows up as
	/// a line-level diff rather than as a failure three stages later. Diagnostics are
	/// appended so a test can assert on them in the same comparison.
	/// </remarks>
	public override string ToString()
	{
		var text = new StringBuilder();

		foreach (var token in Tokens)
		{
			if (token.Kind == TokenKind.EndOfFile)
				continue;

			text.AppendEndingWith(token.ToString());
		}

		foreach (var diagnostic in Diagnostics)
			text.AppendEndingWith(diagnostic.ToString());

		return text.ToString().TrimEnd();
	}
}
