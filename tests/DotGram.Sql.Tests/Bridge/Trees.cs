using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DotGram.Sql.Tests;

/// <summary>
/// What the bridge's oracle asks of two trees that are not simply equal, and the nodes an old one holds.
/// </summary>
/// <remarks>
/// <para>
/// <b>Key words, not names.</b> The old tree keeps some key words as the text they were written in —
/// <c>distinct</c>, <c>current_user</c>, a cast's <c>varchar (10)</c> — and the new tree keeps the meaning,
/// which <c>Sql2023Writer</c> prints in capitals. So a statement written in lower case comes back unequal by
/// record equality and equal in everything it says. The members that hold such text are a closed list,
/// <see cref="FreeText"/>, and are compared token by token (<see cref="SqlTokens"/>): blanks only between
/// tokens, a word without case only where both are in the closed list <see cref="KeyWords"/> the writer
/// capitalises, the <c>N</c> of a national string without case, and every other token — a name in brackets
/// or quotes, a string, a number, any other word — exactly. Every other member compares exactly. A statement
/// that passes only so is counted as converted and, per member, apart (<c>BridgeOracle.Outcome.ByCase</c>).
/// </para>
/// </remarks>
static class Trees
{
	/// <summary>
	/// The members of the old tree that hold text with key words in it, as <c>Record.Member</c>.
	/// </summary>
	public static readonly HashSet<string> FreeText = new(StringComparer.Ordinal)
	{
		"Query.Specification.Quantifier",
		"Expression.Quantified.Quantifier",
		"Expression.Match.Qualifier",
		"Expression.RoutineInvocation.Word",
		"Expression.RoutineInvocation.Name",
		"Expression.WindowFunction.Within",
		"Expression.Literal.Text",
		"Clause.Top.With",
		"Clause.GroupBy.With",
		"Clause.SystemTime.Kind",
		"Clause.TableSample.Unit",
		"Clause.Option.Name",
		"Clause.Window.Frame",
		"Statement.Transaction.Kind",
		"Statement.Transaction.Word",
		"Statement.Transaction.Tail",
		"Statement.Insert.Into",
		"Statement.CursorAction.Verb",
		"Statement.Fetch.Orientation",
		"Statement.SequenceDefinition.Tail",
		"Statement.SchemaDefinition.Tail",
		"TableReference.Joined.Hint",
		"SetExpression.Transactions.IsolationLevel.Level",
	};

	/// <summary>
	/// The key words <c>Sql2023Writer</c> prints in capitals where the old tree keeps what was written: the
	/// only words <see cref="FreeText"/> compares without case.
	/// </summary>
	public static readonly HashSet<string> KeyWords = new(StringComparer.OrdinalIgnoreCase)
	{
		"ABSOLUTE", "ALL", "AND", "ANY", "AS", "ASC", "AT", "AUTHORIZATION", "BEGIN", "BETWEEN", "BIGINT", "BINARY",
		"BOTH", "BY", "CACHE", "CAST", "CHAR", "CHARACTER", "CLOSE", "COMMIT", "COMMITTED", "CONTAINED", "CONTENT",
		"CONVERT", "CUBE", "CURRENT", "CURRENT_DATE", "CURRENT_TIME", "CURRENT_TIMESTAMP", "CURRENT_USER", "CYCLE",
		"DATE", "DEALLOCATE", "DEC", "DECIMAL", "DELAYED_DURABILITY", "DESC", "DISTINCT", "DOCUMENT", "DOUBLE",
		"FETCH", "FIRST", "FLOAT", "FOLLOWING", "FOR", "FROM", "FULL", "GROUP", "HASH", "IGNORE", "IN", "INCREMENT",
		"INT", "INTEGER", "INTO", "JSON", "LAST", "LEADING", "LOOP", "MARK", "MAX", "MAXVALUE", "MERGE", "MINVALUE",
		"NATIONAL", "NCHAR", "NEXT", "NO", "NULLS", "NUMERIC", "OF", "OFF", "ON", "OPEN", "ORDER", "OVER", "PARSE",
		"PARTIAL", "PERCENT", "PRECEDING", "PRECISION", "PRIOR", "RANGE", "READ", "REAL", "RELATIVE", "REMOTE",
		"REPEATABLE", "RESPECT", "RESTART", "ROLLBACK", "ROLLUP", "ROW", "ROWS", "SAVE", "SERIALIZABLE",
		"SESSION_USER", "SIMPLE", "SMALLINT", "SNAPSHOT", "SOME", "START", "SYSTEM_USER", "TIES", "TIME", "TO",
		"TRAILING", "TRAN", "TRANSACTION", "TRIM", "TRY_CAST", "TRY_CONVERT", "TRY_PARSE", "UNBOUNDED",
		"UNCOMMITTED", "UNIQUE", "USER", "VALUE", "VARBINARY", "VARCHAR", "VARYING", "WITH", "WITHIN",
		"WORK", "XML", "ZONE",
	};

	/// <summary>
	/// One step from a node to a member of it, and the item where the member is a list.
	/// </summary>
	public readonly record struct Step(object Node, string Member, int? Index);

	/// <summary>
	/// One place two trees differ: the steps from the root of the left tree down to it, and what differs.
	/// </summary>
	public sealed record Diff(IReadOnlyList<Step> Path, string Text)
	{
		public override string ToString()
		{
			return string.Concat(Path.Select(static one => $"/{Bridge.NameOf(one.Node)}.{one.Member}{(one.Index is { } at ? $"[{at}]" : "")}")) + ": " + Text;
		}
	}

	/// <summary>
	/// Every place two trees differ, or none where they do not. For two old trees the members of
	/// <see cref="FreeText"/> compare token by token, and each that passed only so is added to
	/// <paramref name="byCase"/>.
	/// </summary>
	public static List<Diff> Differences(object? left, object? right, bool old = true, List<string>? byCase = null)
	{
		var found = new List<Diff>();

		Compare(left, right, [], old, byCase, found);

		return found;
	}

	/// <summary>The first place two trees differ, as text, or null.</summary>
	public static string? Difference(object? left, object? right, bool old = true, List<string>? byCase = null)
	{
		return Differences(left, right, old, byCase).FirstOrDefault()?.ToString();
	}

	static void Compare(object? left, object? right, List<Step> path, bool old, List<string>? byCase, List<Diff> found)
	{
		if (ReferenceEquals(left, right))
			return;

		if (left is null || right is null)
		{
			found.Add(new Diff([.. path], $"{Show(left)} against {Show(right)}"));

			return;
		}

		if (left.GetType() != right.GetType())
		{
			found.Add(new Diff([.. path], $"{Show(left)} against {Show(right)}"));

			return;
		}

		if (left is string || left.GetType().IsPrimitive || left.GetType().IsEnum || left is not ISqlSpan && left is not IEnumerable)
		{
			if (!left.Equals(right))
				found.Add(new Diff([.. path], $"{Show(left)} against {Show(right)}"));

			return;
		}

		if (left is not ISqlSpan)
			return;

		var record = Bridge.NameOf(left);

		foreach (var property in Properties(left.GetType()))
		{
			var a      = property.GetValue(left);
			var b      = property.GetValue(right);
			var member = record + "." + property.Name;

			if (a is IEnumerable one && b is IEnumerable two && a is not string && a.GetType().Name.StartsWith("SqlList", StringComparison.Ordinal))
			{
				var x = one.Cast<object?>().ToArray();
				var y = two.Cast<object?>().ToArray();

				if (x.Length != y.Length)
				{
					found.Add(new Diff([.. path, new Step(left, property.Name, null)], $"{x.Length} items against {y.Length}"));

					continue;
				}

				for (var at = 0; at < x.Length; at++)
					Compare(x[at], y[at], [.. path, new Step(left, property.Name, at)], old, byCase, found);

				continue;
			}

			if (old && a is string first && b is string second && first != second && FreeText.Contains(member) && SameTokens(left, member, first, second))
			{
				byCase?.Add(member);

				continue;
			}

			Compare(a, b, [.. path, new Step(left, property.Name, null)], old, byCase, found);
		}
	}

	/// <summary>
	/// Whether two texts of a free-text member say the same: token by token, a word without case only where
	/// both are key words, a national string's <c>N</c> without case, everything else exactly.
	/// </summary>
	public static bool SameTokens(object node, string member, string one, string two)
	{
		// A literal's text is free only where it is a key word (`current_user`) or a national string.
		if (member == "Expression.Literal.Text" && node is Expression.Literal { Kind: not (SqlLiteralKind.Special or SqlLiteralKind.National) })
			return false;

		var x = SqlTokens.Of(one);
		var y = SqlTokens.Of(two);

		if (x.Count != y.Count)
			return false;

		for (var at = 0; at < x.Count; at++)
		{
			var a = x[at];
			var b = y[at];

			if (a.Kind != b.Kind)
				return false;

			if (a.Text == b.Text)
				continue;

			var same = a.Kind switch
			{
				SqlTokens.Kind.Word   => KeyWords.Contains(a.Text) && a.Text.Equals(b.Text, StringComparison.OrdinalIgnoreCase),
				SqlTokens.Kind.String => a.Text.Length == b.Text.Length && a.Text[0] is 'N' or 'n' && b.Text[0] is 'N' or 'n' && a.Text[1..] == b.Text[1..],
				_                     => false,
			};

			if (!same)
				return false;
		}

		return true;
	}

	static string Show(object? value)
	{
		return value switch
		{
			null      => "null",
			string s  => $"\"{s}\"",
			ISqlSpan  => Bridge.NameOf(value),
			_         => value.ToString() ?? "",
		};
	}

	static readonly Dictionary<Type, PropertyInfo[]> Known = [];

	/// <summary>
	/// A record's members that say what was written: not its span, nor what it derives.
	/// </summary>
	static PropertyInfo[] Properties(Type type)
	{
		lock (Known)
		{
			if (!Known.TryGetValue(type, out var properties))
				Known[type] = properties = [.. type
					.GetProperties(BindingFlags.Public | BindingFlags.Instance)
					.Where(static one => one.GetIndexParameters().Length == 0 && one.Name is not "Span" and not "Category" and not "EqualityContract")];

			return properties;
		}
	}

	/// <summary>
	/// Every node of an old tree, the root among them.
	/// </summary>
	public static IEnumerable<object> Nodes(object root)
	{
		var pending = new Stack<object>();

		pending.Push(root);

		while (pending.Count > 0)
		{
			var node = pending.Pop();

			yield return node;

			foreach (var property in Properties(node.GetType()))
			{
				var value = property.GetValue(node);

				if (value is ISqlSpan child)
					pending.Push(child);
				else if (value is IEnumerable list and not string)
					foreach (var item in list)
						if (item is ISqlSpan one)
							pending.Push(one);
			}
		}
	}
}
