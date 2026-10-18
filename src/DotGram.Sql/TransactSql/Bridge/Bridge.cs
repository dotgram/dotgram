using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.TransactSql;

/// <summary>
/// The old T-SQL tree (<c>SqlSyntax.cs</c>, <c>DotGram.Sql</c>) carried onto the new one
/// (<c>SqlAst*.cs</c>, <c>DotGram.Sql.Ast</c>), node by node, so that <c>Sql2023Writer</c> can be held
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

	/// <summary>
	/// The members the statement being carried had read from its text, by name (<see cref="Recovered"/>).
	/// </summary>
	[ThreadStatic] static List<string>? recovered;

	/// <summary>
	/// Whether the new nodes take the spans of the old nodes they are made from: a carriage for
	/// <c>TransactSqlParser.Located</c>.
	/// </summary>
	[ThreadStatic] static bool locating;

	delegate bool Converter(object old, [NotNullWhen(true)] out Ast.ISqlNode? made);

	/// <summary>
	/// The families, in the order they are asked. Each answers only for what it owns.
	/// </summary>
	static readonly Converter[] Families =
	[
		Built.TryConvert,
		Values.TryConvert,
		Queries.TryConvert,
		Dml.TryConvert,
		Tables.TryConvert,
		Routines.TryConvert,
		Procedural.TryConvert,
		Database.TryConvert,
		Monitoring.TryConvert,
	];

	/// <summary>
	/// Every family by name, and the old records it owns, for the baselines and the gap report.
	/// </summary>
	public static readonly IReadOnlyList<(string Family, string[] Owns)> Ownership =
	[
		("Built",      Built.Owns),
		("Values",     Values.Owns),
		("Queries",    Queries.Owns),
		("Dml",        Dml.Owns),
		("Tables",     Tables.Owns),
		("Routines",   Routines.Owns),
		("Procedural", Procedural.Owns),
		("Security",   Security.Owns),
		("Database",   Database.Owns),
		("Server",     Server.Owns),
		("Broker",     Broker.Owns),
		("Monitoring", Monitoring.Owns),
		("External",   External.Owns),
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

		source    = text;
		recovered = [];

		try
		{
			return convert();
		}
		finally
		{
			source = outer;
		}
	}

	/// <summary>
	/// The members of the new tree the last statement carried on this thread had read from its text rather than
	/// from the old tree, one entry a reading: the closed list <see cref="Recoverable"/> names them.
	/// </summary>
	public static IReadOnlyList<string> Recovered => recovered ?? [];

	/// <summary>
	/// Every member the bridge may read from the text, because the old tree does not keep it. Closed: a reading
	/// under any other name fails (<see cref="Recover{T}"/>), and docs/design/sql-tsql-bridge.md lists them.
	/// </summary>
	public static readonly string[] Recoverable =
	[
		"SelectColumn.ExpressionItem.AsKeyword",
		"Clause.DerivedColumn alias = expression",
		"Alias.AsKeyword of a table source",
		"Alias.AsKeyword of MERGE's target",
		"Limit.OffsetRowWord",
		"Limit.Position",
		"Limit.FetchRowWord",
		"WindowReference.NameRef against (name)",
		"Expression.Invocation without brackets",
		"Expression.Trim.FromKeyword",
		"Statement.SetTransaction.Tran",
		"Statement.Delete.FromOmitted",
		"ReferencesSpecification.DeleteRuleFirst",
		"Statement.ExecuteModule.Keyword",
		"Statement.ExecuteString.Exec",
		"RoutineDefinition.Proc",
		"ParameterDefinition.AsKeyword",
		"Statement.ExecuteAs.Exec",
		"RoutineDefinition.Unbracketed",
		"RoutineDefinition.AsOmitted",
		"Statement.Classification.Options",
	];

	/// <summary>
	/// A value read from the text, counted under its member's name where the text was there to read; where it
	/// was not, the default the caller passes stands and nothing is counted.
	/// </summary>
	static T Recover<T>(string member, string? text, Func<string, T> read, T otherwise)
	{
		if (Array.IndexOf(Recoverable, member) < 0)
			throw new ArgumentOutOfRangeException(nameof(member), member, "Not one of Bridge.Recoverable.");

		if (text is null)
			return otherwise;

		recovered?.Add(member);

		return read(text);
	}

	/// <summary>
	/// A conversion of nodes read again from another text than the statement's, with that text as the source their
	/// spans are in; what is read from the text is still counted for the statement.
	/// </summary>
	static T Inside<T>(string text, Func<T> convert)
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
	/// <remarks>
	/// Every node that can hold another of its kind passes here, so this is where a deep tree's carriage checks its
	/// stack: where it runs low, the rest of the carriage goes on with a stack of its own (<see cref="SqlStack"/>), as
	/// the reader that built the tree did, rather than overflowing it.
	/// </remarks>
	static T Convert<T>(object old)
		where T : Ast.ISqlNode
	{
		if (!SqlStack.Enough())
			return Deeper(old, static node => Convert<T>(node));

		foreach (var family in Families)
			if (family(old, out var made))
			{
				if (made is not T typed)
					throw BridgeGap.Open(old, $"converts to {made.GetType().Name}, where a {typeof(T).Name} stands");

				if (locating && old is ISqlSpan { Span: { Known: true, IsStale: false } span })
					Place(typed, span);

				return typed;
			}

		throw BridgeGap.Missing(old, "no family converts it");
	}

	/// <summary>
	/// A conversion on a stack of its own, with this thread's carriage — its text, what it read from it and whether it
	/// locates — taken across. Called where the stack runs low, by <see cref="Convert{T}"/> and by a family's conversion
	/// that recurses on itself.
	/// </summary>
	static T Deeper<T>(object old, Func<object, T> convert)
	{
		var carriage = Carriage.Here();
		var made     = new object?[1];

		SqlStack.Deeper(old, (carriage, convert), made, static (node, state, result) =>
		{
			var outer = Carriage.Here();

			state.carriage.Restore();

			try
			{
				result[0] = state.convert(node);
			}
			finally
			{
				outer.Restore();
			}
		});

		carriage.Restore();

		return (T)made[0]!;
	}

	/// <summary>The thread-static state of a carriage, taken from one thread to another.</summary>
	sealed class Carriage(string? source, List<string>? recovered, bool locating)
	{
		public static Carriage Here()
		{
			return new Carriage(Bridge.source, Bridge.recovered, Bridge.locating);
		}

		public void Restore()
		{
			Bridge.source    = source;
			Bridge.recovered = recovered;
			Bridge.locating  = locating;
		}
	}

	/// <summary>
	/// An old record's name as the inventory writes it: <c>Statement.Select</c>, <c>SetExpression.Locking.LockTimeout</c>.
	/// </summary>
	public static string NameOf(object old)
	{
		return old switch
		{
			Given given               => given.Record,
			Statement.Built made      => made.Record,
			Query.Built query         => query.Record,
			TableReference.Built from => from.Record,
			_                         => NameOf(old.GetType()),
		};
	}

	public static string NameOf(Type type)
	{
		var name = type.Name;

		for (var outer = type.DeclaringType; outer is not null; outer = outer.DeclaringType)
			name = outer.Name + "." + name;

		return name;
	}

	/// <summary>
	/// What says the gap for an old record a family owns and has no arm for, where something says more than the
	/// record's name: the tests set it to name the row of the tree's inventory that the record waits on.
	/// </summary>
	internal static Func<object, BridgeGap>? Explained { get; set; }

	/// <summary>
	/// The gap for an old record a family owns and has no arm for.
	/// </summary>
	static BridgeGap Unconverted(object old)
	{
		return Explained is { } explain ? explain(old) : BridgeGap.Unbridged(old, "the bridge has no arm for it");
	}

	/// <summary>
	/// Whether a name is one of the old records a family lists.
	/// </summary>
	static bool Owned(string[] owns, object old)
	{
		return Array.IndexOf(owns, NameOf(old)) >= 0;
	}

	// ── The words the old tree drops ────────────────────────────────────────────

	/// <summary>Whether text can be cut at a span: there is a text, and the span says where in it.</summary>
	[MemberNotNullWhen(true, nameof(source))]
	static bool Cuttable(SqlSpan span)
	{
		return source is not null && span.Known && !span.IsStale;
	}

	/// <summary>
	/// The text a node was read from, or null where it was not located or no text was handed in.
	/// </summary>
	static string? Spelled(ISqlSpan node)
	{
		var span = node.Span;

		if (!Cuttable(span) || span.End > source.Length)
			return null;

		return source.Substring(span.At, span.Length);
	}

	/// <summary>
	/// The text between the end of one node and the start of the next, or null where either is not
	/// located: what was written between two things the old tree keeps.
	/// </summary>
	static string? Between(ISqlSpan before, ISqlSpan after)
	{
		if (!Cuttable(before.Span) || !Cuttable(after.Span))
			return null;

		var from = before.Span.End;
		var to   = after.Span.At;

		return from <= to && to <= source.Length ? source.Substring(from, to - from) : null;
	}

	/// <summary>
	/// Whether a text the old tree keeps as written reads back as the text the writer prints, as the oracle compares the
	/// two: exactly, or token by token (BridgeText.SameTokens) where the member is free text (BridgeText.Free).
	/// </summary>
	static bool Same(object old, string member, string written, string text)
	{
		return written == text || BridgeText.Free(member) && BridgeText.SameTokens(old, member, text, written);
	}

	/// <summary>
	/// The tokens of a stretch of text, comments apart: a word upper-cased, and a name in brackets or quotes, a
	/// string or a number as written — so that `"as"` is never the word AS.
	/// </summary>
	static List<string> Words(string text)
	{
		var tokens = SqlTokens.Of(text);
		var words  = new List<string>(tokens.Count);

		foreach (var one in tokens)
			words.Add(one.Kind == SqlTokens.Kind.Word ? one.Text.ToUpperInvariant() : one.Text);

		return words;
	}

	/// <summary>
	/// The first token written after a located node, comments and blanks skipped — a word upper-cased — or null
	/// where the text is not there or nothing follows.
	/// </summary>
	static string? WordAfter(ISqlSpan node)
	{
		if (!Cuttable(node.Span) || node.Span.End > source.Length)
			return null;

		return SqlTokens.First(source, node.Span.End) is { } token
			? token.Kind == SqlTokens.Kind.Word ? token.Text.ToUpperInvariant() : token.Text
			: null;
	}

	/// <summary>
	/// The writer's text of a node a conversion built from what was written, to compare the two: where the writer refuses
	/// the node, with its refusals (a rejected argument, an unsupported form), the text has no form the common tree writes
	/// yet, and that is a gap, as every other form the bridge has no place for is.
	/// </summary>
	static string Written(object old, Ast.ISqlNode node, string member)
	{
		try
		{
			return Ast.TransactSqlWriter.Write(node);
		}
		catch (Exception refused) when (refused is ArgumentException or NotSupportedException)
		{
			throw BridgeGap.Unwritten(old, $"{member}, which the writer refuses ({refused.GetType().Name})");
		}
	}

	/// <summary>Each item carried, in order, into a list of its own: no list for none, and no array for one.</summary>
	static SqlList<T> Each<TOld, T>(SqlList<TOld> items, Func<TOld, T> convert)
	{
		switch (items.Count)
		{
			case 0:
				return [];

			case 1:
				return SqlList<T>.Of(convert(items[0]));

			default:
				var made = new T[items.Count];

				for (var at = 0; at < made.Length; at++)
					made[at] = convert(items[at]);

				return SqlList.Own(made);
		}
	}
}
