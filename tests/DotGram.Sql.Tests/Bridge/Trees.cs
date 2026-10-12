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
/// <c>distinct</c>, <c>current_user</c>, a cast's <c>varchar (10)</c> — and the new tree keeps the
/// meaning, which <c>Sql2023Writer</c> prints in capitals. So a statement written in lower case comes back
/// unequal by record equality and equal in everything it says. <see cref="Difference"/> compares the
/// members listed in <see cref="KeyWords"/> without case, and a cast's or a call's word without case or
/// spaces; every other string, and every name, exactly. A statement that passes only so is counted as
/// converted and reported apart (<c>BridgeOracle.Outcome.ByCase</c>).
/// </para>
/// </remarks>
static class Trees
{
	/// <summary>
	/// The members of the old tree that hold a key word as written, as <c>Record.Member</c>.
	/// </summary>
	static readonly HashSet<string> KeyWords = new(StringComparer.Ordinal)
	{
		"Query.Specification.Quantifier",
		"Expression.Quantified.Quantifier",
		"Expression.Match.Qualifier",
		"Clause.Top.With",
		"Clause.GroupBy.With",
		"Clause.SystemTime.Kind",
		"Clause.TableSample.Unit",
		"Statement.Transaction.Kind",
		"Statement.Transaction.Word",
		"Statement.Transaction.Tail",
		"Statement.Insert.Into",
		"Statement.CursorAction.Verb",
		"Statement.Fetch.Orientation",
		"TableReference.Joined.Hint",
		"SetExpression.Transactions.IsolationLevel.Level",
		"Clause.Option.Name",
	};

	/// <summary>
	/// The members that hold key words and a few values or names as written, compared without case or spaces:
	/// a call's one word or a cast's type, the words between a call and its window, and the tails of the
	/// definitions the bridge reads.
	/// </summary>
	static readonly HashSet<string> Squeezed = new(StringComparer.Ordinal)
	{
		"Expression.RoutineInvocation.Word",
		"Expression.WindowFunction.Within",
		"Statement.SequenceDefinition.Tail",
		"Statement.SchemaDefinition.Tail",
	};

	/// <summary>
	/// The calls whose name is a key word in the new tree, a node of its own rather than a name.
	/// </summary>
	static readonly HashSet<string> Keyed = new(StringComparer.OrdinalIgnoreCase)
	{
		"CAST", "TRIM", "AT TIME ZONE", "CURRENT_TIMESTAMP", "CURRENT_DATE", "CURRENT_TIME",
	};

	/// <summary>
	/// The first place two trees differ, or null where they do not; for two old trees, the case of a key word
	/// kept as written aside.
	/// </summary>
	public static string? Difference(object? left, object? right, bool old = true)
	{
		return Difference(left, right, "", old);
	}

	static string? Difference(object? left, object? right, string path, bool old)
	{
		if (ReferenceEquals(left, right))
			return null;

		if (left is null || right is null)
			return $"{path}: {Show(left)} against {Show(right)}";

		if (left.GetType() != right.GetType())
			return $"{path}: {Bridge.NameOf(left)} against {Bridge.NameOf(right)}";

		if (left is string || left.GetType().IsPrimitive || left.GetType().IsEnum)
			return left.Equals(right) ? null : $"{path}: {Show(left)} against {Show(right)}";

		if (left is IEnumerable list && left.GetType().Name.StartsWith("SqlList", StringComparison.Ordinal))
		{
			var a = list.Cast<object?>().ToArray();
			var b = ((IEnumerable)right).Cast<object?>().ToArray();

			if (a.Length != b.Length)
				return $"{path}: {a.Length} items against {b.Length}";

			for (var at = 0; at < a.Length; at++)
				if (Difference(a[at], b[at], $"{path}[{at}]", old) is { } inner)
					return inner;

			return null;
		}

		if (left is not ISqlSpan)
			return left.Equals(right) ? null : $"{path}: {Show(left)} against {Show(right)}";

		var record = Bridge.NameOf(left);

		foreach (var property in Properties(left.GetType()))
		{
			var a     = property.GetValue(left);
			var b     = property.GetValue(right);
			var where = $"{path}/{record}.{property.Name}";

			if (old && a is string one && b is string two && Same(left, record + "." + property.Name, one, two))
				continue;

			if (Difference(a, b, where, old) is { } inner)
				return inner;
		}

		return null;
	}

	/// <summary>
	/// Whether two strings of one member say the same, where the member keeps a key word as written.
	/// </summary>
	static bool Same(object node, string member, string one, string two)
	{
		if (one == two)
			return true;

		if (KeyWords.Contains(member))
			return one.Equals(two, StringComparison.OrdinalIgnoreCase);

		if (Squeezed.Contains(member))
			return Bridge.Values.Squeezed(one) == Bridge.Values.Squeezed(two);

		// The calls the new tree keeps as key words rather than names: `trim (…)` is TRIM.
		if (member == "Expression.RoutineInvocation.Name" && Keyed.Contains(one))
			return one.Equals(two, StringComparison.OrdinalIgnoreCase);

		// A literal's key words — `current_user` — and the `N` in front of a national string.
		if (member == "Expression.Literal.Text" && node is Expression.Literal literal)
			return literal.Kind switch
			{
				SqlLiteralKind.Special  => one.Equals(two, StringComparison.OrdinalIgnoreCase),
				SqlLiteralKind.National => one.Length == two.Length && char.ToUpperInvariant(one[0]) == char.ToUpperInvariant(two[0]) && one[1..] == two[1..],
				_                       => false,
			};

		return false;
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
