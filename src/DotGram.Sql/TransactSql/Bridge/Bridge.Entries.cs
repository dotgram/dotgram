using System;
using System.Collections.Concurrent;
using System.Reflection;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.TransactSql;

// How TransactSqlParser's entries hand back the common tree while the grammar still builds the T-SQL tree: each
// public entry reads through the internal one of the same rule, and what it read is carried across here with the text
// it was read from (TransactSqlParser.Entries.cs). A construct the bridge has no place for yet comes back as the
// family's Extension node, naming the old record and why, rather than being thrown out of a TryParse. This goes as the
// grammar's rules come to build the common tree themselves.
static partial class Bridge
{
	/// <summary>
	/// A statement an entry read, on the common tree; one the bridge has no place for, a
	/// <see cref="Ast.Statement.Extension"/> that says which and why.
	/// </summary>
	/// <param name="old">The T-SQL tree the entry read.</param>
	/// <param name="input">The whole text the reading was given; a located node's span is a position in it.</param>
	/// <param name="at">Where the reading began in <paramref name="input"/>.</param>
	/// <param name="length">How much of it the reading read.</param>
	/// <param name="again">The same entry's located door, over a window: what reads the text again where it was read without.</param>
	internal static Ast.Statement Carried(Statement old, string input, int at, int length, Func<string, int, int, TransactSqlParser.Match<Statement>> again)
	{
		return Carry(old, input, at, length, again, out var gap) as Ast.Statement ?? Unplaced(old, gap);
	}

	/// <summary>An expression an entry read, on the common tree, or an <see cref="Ast.Expression.Extension"/>.</summary>
	internal static Ast.Expression Carried(Expression old, string input, int at, int length, Func<string, int, int, TransactSqlParser.Match<Expression>> again)
	{
		return Carry(old, input, at, length, again, out var gap) as Ast.Expression
			?? new Ast.Expression.Extension(Dialect, gap?.Message ?? NameOf(old), []);
	}

	/// <summary>The statements an entry read, on the common tree, each the bridge has no place for an extension.</summary>
	internal static Ast.Statement[] Carried(Statement[] old, string input, int at, int length, Func<string, int, int, TransactSqlParser.Match<Statement[]>> again)
	{
		return (Ast.Statement[])Carry(old, input, at, length, again, out _)!;
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
	/// <remarks>
	/// A located reading is carried with its text, and every new node made from an old one takes its span. A reading
	/// without locations is carried blind, taking the commoner spelling of each word the old tree drops; where it
	/// wanted one, the same window of the text is read again by the same entry with locations and carried with the
	/// text, so that both doors give the same tree. A second reading that is not the same old tree leaves the blind
	/// carriage standing.
	/// </remarks>
	static object? Carry<TOld>(TOld old, string input, int at, int length, Func<string, int, int, TransactSqlParser.Match<TOld>> again, out BridgeGap? gap)
		where TOld : class
	{
		gap = null;

		try
		{
			if (Located(old))
				return Along(input, locate: true, () => Across(old));

			var made = Blind(old, out var asked, out gap);

			if (!asked)
				return made;

			TOld? read;

			// Equality throws on a tree deeper than the stack has room for; such a tree keeps its blind carriage.
			try
			{
				read = again(input, at, length) is { IsSuccess: true } match && Unchanged(match.Value, old) ? match.Value : null;
			}
			catch (InsufficientExecutionStackException)
			{
				read = null;
			}

			if (read is null)
				return made;

			TOld located = read;

			gap = null;

			return Along(input, locate: false, () => Across(located));
		}
		catch (BridgeGap found)
		{
			gap = found;

			return null;
		}
	}

	/// <summary>Whether an old tree was read through the located door: its root says where it was written.</summary>
	static bool Located(object old)
	{
		return old switch
		{
			ISqlSpan node            => node.Span.Known,
			Statement[] { Length: 0 } => false,
			Statement[] statements   => statements[0].Span.Known,
			_                        => false,
		};
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

	/// <summary>
	/// A carriage without a text, which says whether it asked for a word from one; null where it found a gap, which a
	/// carriage with the text may not find.
	/// </summary>
	static object? Blind(object old, out bool asked, out BridgeGap? gap)
	{
		var outerBlind  = blind;
		var outerWanted = wanted;

		asked  = false;
		gap    = null;
		blind  = true;
		wanted = false;

		try
		{
			return Reading(null, () => Across(old));
		}
		catch (BridgeGap found)
		{
			gap = found;

			return null;
		}
		finally
		{
			asked  = wanted;
			blind  = outerBlind;
			wanted = outerWanted;
		}
	}

	/// <summary>Whether a reading again is the same old tree: a statement, an expression or every statement of a text.</summary>
	static bool Unchanged(object again, object old)
	{
		if (again is not Statement[] one || old is not Statement[] two)
			return again.Equals(old);

		if (one.Length != two.Length)
			return false;

		for (var i = 0; i < one.Length; i++)
			if (!one[i].Equals(two[i]))
				return false;

		return true;
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
