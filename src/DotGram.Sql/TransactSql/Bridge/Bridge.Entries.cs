using System;
using System.Collections.Concurrent;
using System.Reflection;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.TransactSql;

// How TransactSqlParser's entries hand back the common tree while the grammar still builds the T-SQL tree: each
// public entry reads through the internal one of the same rule, with locations, and what it read is carried across
// here with the text it was read from (TransactSqlParser.Entries.cs). A construct the bridge has no place for yet
// comes back as the family's Extension node, naming the old record and why, rather than being thrown out of a
// TryParse. This goes as the grammar's rules come to build the common tree themselves.
static partial class Bridge
{
	/// <summary>
	/// A statement an entry read, on the common tree; one the bridge has no place for, a
	/// <see cref="Ast.Statement.Extension"/> that says which and why.
	/// </summary>
	/// <param name="old">The T-SQL tree the entry read, with locations: the bridge reads the words it drops from the text.</param>
	/// <param name="input">The whole text the reading was given; a located node's span is a position in it.</param>
	/// <param name="locate">Whether the new nodes take the old nodes' spans: the located door's tree, and not the plain one's.</param>
	internal static Ast.Statement Carried(Statement old, string input, bool locate)
	{
		return Carry(old, input, locate, out var gap) as Ast.Statement ?? Unplaced(old, gap);
	}

	/// <summary>An expression an entry read, on the common tree, or an <see cref="Ast.Expression.Extension"/>.</summary>
	internal static Ast.Expression Carried(Expression old, string input, bool locate)
	{
		return Carry(old, input, locate, out var gap) as Ast.Expression
			?? new Ast.Expression.Extension(Dialect, gap?.Message ?? NameOf(old), []);
	}

	/// <summary>The statements an entry read, on the common tree, each the bridge has no place for an extension.</summary>
	internal static Ast.Statement[] Carried(Statement[] old, string input, bool locate)
	{
		return (Ast.Statement[])Carry(old, input, locate, out _)!;
	}

	/// <summary>What an extension of the common tree carried across from T-SQL says it is.</summary>
	const string Dialect = "T-SQL";

	static Ast.Statement Unplaced(Statement old, BridgeGap? gap)
	{
		return new Ast.Statement.Extension { Dialect = Dialect, Kind = gap?.Message ?? NameOf(old) };
	}

	/// <summary>
	/// The common tree of what an entry read, or null where the bridge has no place for something in it, and then the
	/// gap that says so.
	/// </summary>
	static object? Carry(object old, string input, bool locate, out BridgeGap? gap)
	{
		gap = null;

		try
		{
			return Along(input, locate, () => Across(old));
		}
		catch (BridgeGap found)
		{
			gap = found;

			return null;
		}
	}

	/// <summary>One old tree an entry reads, carried.</summary>
	static object Across(object old)
	{
		switch (old)
		{
			case Statement statement:
				return StatementOf(statement);

			case Expression expression:
				return ExpressionOf(expression);

			// A text of statements reads whole: each the bridge has no place for stands as an extension among the rest.
			case Statement[] statements:
				var made = new Ast.Statement[statements.Length];

				for (var i = 0; i < statements.Length; i++)
				{
					try
					{
						made[i] = StatementOf(statements[i]);
					}
					catch (BridgeGap gap)
					{
						made[i] = Unplaced(statements[i], gap);
					}
				}

				return made;

			default:
				throw new ArgumentException($"An entry reads no {old.GetType().Name}.", nameof(old));
		}
	}

	/// <summary>A carriage with the text it was read from.</summary>
	static T Along<T>(string text, bool locate, Func<T> convert)
	{
		var outer = locating;

		locating = locate;

		try
		{
			return Reading(text, convert);
		}
		finally
		{
			locating = outer;
		}
	}

	// ── Spans ───────────────────────────────────────────────────────────────────

	/// <summary>How each record of the common tree is told where it was written, by type: null for one that keeps nothing.</summary>
	static readonly ConcurrentDictionary<Type, Action<object, int, int>?> Placers = new();

	/// <summary>
	/// A new node told the span of the old node it was made from: the range the reading offered the old one, without
	/// the gap in front of it, which the common tree does not keep.
	/// </summary>
	static void Place(object made, SqlSpan span)
	{
		if (Placers.GetOrAdd(made.GetType(), Placer) is { } place)
			place(made, span.At, span.Length);
	}

	static Action<object, int, int>? Placer(Type type)
	{
		if (type.IsValueType)
			return null;

		var locate = type.GetMethod("Locate", BindingFlags.Public | BindingFlags.Instance, null, [typeof(int), typeof(int)], null);

		if (locate is null)
			return null;

		return (Action<object, int, int>)typeof(Bridge)
			.GetMethod(nameof(Typed), BindingFlags.NonPublic | BindingFlags.Static)!
			.MakeGenericMethod(locate.DeclaringType!)
			.Invoke(null, [locate])!;
	}

	static Action<object, int, int> Typed<TNode>(MethodInfo locate)
	{
		var typed = (Action<TNode, int, int>)locate.CreateDelegate(typeof(Action<TNode, int, int>));

		return (node, at, length) => typed((TNode)node, at, length);
	}
}
