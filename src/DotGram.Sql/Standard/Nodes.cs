using System;
using System.Collections.Generic;

using DotGram.Sql.Ast;

namespace DotGram.Sql.Standard;

/// <summary>
/// How <c>SqlStandard.gram</c> makes the SQL:2023 tree out of what it read: the words and lists a
/// rule captured, turned into nodes (docs/design/sql-ast.md).
/// </summary>
static class Nodes
{
	// ── Names ──────────────────────────────────────────────────────────────────

	/// <summary>
	/// A Unicode delimited identifier, <c>U&amp;"a"</c> or <c>U&amp;"a!0041" UESCAPE '!'</c>: the name as written
	/// up to its closing quote, and the escape character where one was named.
	/// </summary>
	public static Identifier UnicodeIdentifier(string text)
	{
		var close = text.LastIndexOf('"');
		var name  = text.Substring(0, close + 1);
		var rest  = text.Substring(close + 1);
		var quote = rest.IndexOf('\'');

		return new Identifier(name, IdentifierStyle.UnicodeDelimited, quote >= 0 && quote + 1 < rest.Length ? rest[quote + 1] : null);
	}

	/// <summary>A name made of a word read as a key word, <c>MODULE</c>, and the names after it.</summary>
	public static QualifiedName Named(string word, params Identifier[] rest)
	{
		var parts = new Identifier[rest.Length + 1];

		parts[0] = new Identifier(word);
		Array.Copy(rest, 0, parts, 1, rest.Length);

		return new QualifiedName(parts);
	}

	/// <summary>A name and the parts after it, as many as were read.</summary>
	public static QualifiedName Chain(Identifier first, Identifier[]? rest)
	{
		if (rest is not { Length: > 0 })
			return new QualifiedName([first]);

		var parts = new Identifier[rest.Length + 1];

		parts[0] = first;
		Array.Copy(rest, 0, parts, 1, rest.Length);

		return new QualifiedName(parts);
	}

	/// <summary>A name and one optional part after it.</summary>
	public static QualifiedName Chain(Identifier first, Identifier? second) =>
		second is null ? new QualifiedName([first]) : new QualifiedName([first, second]);
}
