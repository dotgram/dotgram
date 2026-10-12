using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.Tests;

/// <summary>
/// The old T-SQL tree (<c>SqlSyntax.cs</c>, <c>DotGram.Sql</c>) carried onto the new one
/// (<c>Sql2023Ast*.cs</c>, <c>DotGram.Sql.Ast</c>), node by node, so that <c>Sql2023Writer</c> can be held
/// to every statement T-SQL reads before the grammar is switched (docs/design/sql-tsql-bridge.md).
/// </summary>
/// <remarks>
/// <para>
/// <b>What it does not do.</b> It declares nothing in the new tree and decides no shape: an old node
/// whose target is missing, unwritten or undecided throws a <see cref="BridgeGap"/> naming it. A gap is
/// the answer for that construct until a later task gives it a place.
/// </para>
/// <para>
/// <b>One file a family.</b> Each family — Values, Queries, Dml, Tables, Routines, Procedural, Security,
/// Database, Server, Drop — is a nested class with a <c>TryConvert</c> that answers for the old records it
/// owns and says no to the rest; <see cref="Convert{T}"/> asks them in turn. A family is filled in its own
/// file without touching a shared switch.
/// </para>
/// <para>
/// <b>Words the old tree drops.</b> Where the old tree does not keep a word the new tree records — <c>AS</c>
/// before an alias, <c>ROW</c> against <c>ROWS</c> — the bridge reads it from the text the node was read
/// from, through its span, when the statement was read with <c>TransactSqlParser.Located</c> and the text
/// was handed in. Without either it takes the commoner spelling, which the round trip cannot tell from the
/// other.
/// </para>
/// </remarks>
static partial class Bridge
{
	/// <summary>
	/// The text the statement being carried was read from, or null where it is not known.
	/// </summary>
	[ThreadStatic] static string? source;

	delegate bool Converter(object old, [NotNullWhen(true)] out Ast.ISqlNode? made);

	/// <summary>
	/// The families, in the order they are asked. Each answers only for what it owns.
	/// </summary>
	static readonly Converter[] Families =
	[
		Values.TryConvert,
		Queries.TryConvert,
		Dml.TryConvert,
		Tables.TryConvert,
		Routines.TryConvert,
		Procedural.TryConvert,
		Security.TryConvert,
		Database.TryConvert,
		Server.TryConvert,
		Drop.TryConvert,
	];

	/// <summary>
	/// Every family by name, and the old records it owns, for the baselines and the gap report.
	/// </summary>
	public static readonly IReadOnlyList<(string Family, string[] Owns)> Ownership =
	[
		("Values",     Values.Owns),
		("Queries",    Queries.Owns),
		("Dml",        Dml.Owns),
		("Tables",     Tables.Owns),
		("Routines",   Routines.Owns),
		("Procedural", Procedural.Owns),
		("Security",   Security.Owns),
		("Database",   Database.Owns),
		("Server",     Server.Owns),
		("Drop",       Drop.Owns),
	];

	public static Ast.Statement ToAst(Statement old, string? text = null)
	{
		return Reading(text, () => StatementOf(old));
	}

	public static Ast.Query ToAst(Query old, string? text = null)
	{
		return Reading(text, () => QueryOf(old));
	}

	public static Ast.Expression ToAst(Expression old, string? text = null)
	{
		return Reading(text, () => ExpressionOf(old));
	}

	public static Ast.TableSource ToAst(TableReference old, string? text = null)
	{
		return Reading(text, () => Source(old));
	}

	public static Ast.ISqlNode ToAst(Clause old, string? text = null)
	{
		return Reading(text, () => Convert<Ast.ISqlNode>(old));
	}

	static T Reading<T>(string? text, Func<T> convert)
	{
		var outer = source;

		source = text;

		try
		{
			return convert();
		}
		finally
		{
			source = outer;
		}
	}

	internal static Ast.Statement StatementOf(Statement old)
	{
		return Convert<Ast.Statement>(old);
	}

	internal static Ast.Query QueryOf(Query old)
	{
		return Convert<Ast.Query>(old);
	}

	internal static Ast.Expression ExpressionOf(Expression old)
	{
		return Convert<Ast.Expression>(old);
	}

	internal static Ast.TableSource Source(TableReference old)
	{
		return Convert<Ast.TableSource>(old);
	}

	/// <summary>
	/// The one dispatcher: the first family that owns the record converts it.
	/// </summary>
	static T Convert<T>(object old)
		where T : Ast.ISqlNode
	{
		foreach (var family in Families)
			if (family(old, out var made))
				return made is T typed ? typed : throw BridgeGap.Open(old, $"converts to {made.GetType().Name}, where a {typeof(T).Name} stands");

		throw BridgeGap.Missing(old, "no family converts it");
	}

	/// <summary>
	/// An old record's name as the inventory writes it: <c>Statement.Select</c>, <c>SetExpression.Locking.LockTimeout</c>.
	/// </summary>
	public static string NameOf(object old)
	{
		return NameOf(old.GetType());
	}

	public static string NameOf(Type type)
	{
		var name = type.Name;

		for (var outer = type.DeclaringType; outer is not null; outer = outer.DeclaringType)
			name = outer.Name + "." + name;

		return name;
	}

	/// <summary>
	/// Whether a name is one of the old records a family lists.
	/// </summary>
	static bool Owned(string[] owns, object old)
	{
		return Array.IndexOf(owns, NameOf(old)) >= 0;
	}

	// ── The words the old tree drops ────────────────────────────────────────────

	/// <summary>
	/// The text a node was read from, or null where it was not located or no text was handed in.
	/// </summary>
	static string? Spelled(ISqlSpan node)
	{
		var span = node.Span;

		if (source is null || !span.Known || span.IsStale || span.End > source.Length)
			return null;

		return source.Substring(span.At, span.Length);
	}

	/// <summary>
	/// The text between the end of one node and the start of the next, or null where either is not
	/// located: what was written between two things the old tree keeps.
	/// </summary>
	static string? Between(ISqlSpan before, ISqlSpan after)
	{
		if (source is null || !before.Span.Known || !after.Span.Known || before.Span.IsStale || after.Span.IsStale)
			return null;

		var from = before.Span.End;
		var to   = after.Span.At;

		return from <= to && to <= source.Length ? source.Substring(from, to - from) : null;
	}

	/// <summary>
	/// The words of a stretch of text, comments and brackets apart, upper-cased.
	/// </summary>
	static string[] Words(string text)
	{
		var words = new List<string>();
		var at    = 0;

		while (at < text.Length)
		{
			var c = text[at];

			if (c == '-' && at + 1 < text.Length && text[at + 1] == '-')
			{
				while (at < text.Length && text[at] != '\n')
					at++;

				continue;
			}

			if (c == '/' && at + 1 < text.Length && text[at + 1] == '*')
			{
				var end = text.IndexOf("*/", at + 2, StringComparison.Ordinal);

				at = end < 0 ? text.Length : end + 2;

				continue;
			}

			if (char.IsLetterOrDigit(c) || c is '_' or '@' or '#' or '$')
			{
				var start = at;

				while (at < text.Length && (char.IsLetterOrDigit(text[at]) || text[at] is '_' or '@' or '#' or '$'))
					at++;

				words.Add(text.Substring(start, at - start).ToUpperInvariant());

				continue;
			}

			if (!char.IsWhiteSpace(c))
				words.Add(c.ToString());

			at++;
		}

		return [.. words];
	}

	/// <summary>
	/// Whether the words between two located nodes include one, or null where the text is not there.
	/// </summary>
	static bool? WrittenBetween(ISqlSpan before, ISqlSpan after, string word)
	{
		return Between(before, after) is { } text ? Words(text).Contains(word) : null;
	}

	/// <summary>
	/// The first word written after a located node, or null where the text is not there.
	/// </summary>
	static string? WordAfter(ISqlSpan node)
	{
		if (source is null || !node.Span.Known || node.Span.IsStale || node.Span.End > source.Length)
			return null;

		var from = node.Span.End;
		var to   = from;

		// One word is enough: skip what stands between, then read up to the next blank or bracket.
		while (to < source.Length && char.IsWhiteSpace(source[to]))
			to++;

		while (to < source.Length && !char.IsWhiteSpace(source[to]) && source[to] is not '(' and not ')' and not ',' and not ';')
			to++;

		var words = Words(source.Substring(from, to - from));

		return words.Length > 0 ? words[0] : null;
	}

	static SqlList<T> Each<TOld, T>(SqlList<TOld> items, Func<TOld, T> convert)
	{
		return items.Select(convert).ToSqlList();
	}
}
