using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using DotGram.Sql.TransactSql;

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
	/// <summary>The members of the old tree that hold text with key words in it (<see cref="BridgeText.FreeText"/>).</summary>
	public static HashSet<string> FreeText => BridgeText.FreeText;

	/// <summary>The key words compared without case (<see cref="BridgeText.KeyWords"/>).</summary>
	public static HashSet<string> KeyWords => BridgeText.KeyWords;

	public const string DropIndexName = BridgeText.DropIndexName;

	public const string SignerName = BridgeText.SignerName;

	public static bool Free(string member)
	{
		return BridgeText.Free(member);
	}

	public static bool SameTokens(object node, string member, string one, string two)
	{
		return BridgeText.SameTokens(node, member, one, two);
	}

	/// <summary>
	/// One step from a node to a member of it, and the item where the member is a list.
	/// </summary>
	public readonly record struct Step(object Node, string Member, int? Index);

	/// <summary>
	/// One place two trees differ: the steps from the root of the left tree down to it, and what differs.
	/// </summary>
	public sealed record Diff(IReadOnlyList<Step> Path, string Text, object? Left = null, object? Right = null)
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
			found.Add(new Diff([.. path], $"{Show(left)} against {Show(right)}", left, right));

			return;
		}

		if (left.GetType() != right.GetType())
		{
			found.Add(new Diff([.. path], $"{Show(left)} against {Show(right)}", left, right));

			return;
		}

		if (left is string || left.GetType().IsPrimitive || left.GetType().IsEnum || left is not ISqlSpan && left is not IEnumerable)
		{
			if (!left.Equals(right))
				found.Add(new Diff([.. path], $"{Show(left)} against {Show(right)}", left, right));

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
				{
					// A list of texts in the closed list compares item by item as one text does.
					if (old && x[at] is string text && y[at] is string other && text != other && FreeText.Contains(member) && SameTokens(left, member, text, other))
					{
						byCase?.Add(member);

						continue;
					}

					Compare(x[at], y[at], [.. path, new Step(left, property.Name, at)], old, byCase, found);
				}

				continue;
			}

			if (old && a is string first && b is string second && first != second)
			{
				var scoped = FreeText.Contains(member) ? member : DatePart(left, path) ? DatePartWord : Dropped(left, path);

				if (scoped is not null && SameTokens(left, scoped, first, second))
				{
					byCase?.Add(scoped);

					continue;
				}
			}

			Compare(a, b, [.. path, new Step(left, property.Name, null)], old, byCase, found);
		}
	}

	/// <summary>
	/// What a date part's word counts as among the members compared without the case of key words.
	/// </summary>
	public const string DatePartWord = "Expression.ColumnReference.Text of a date part";

	/// <summary>
	/// Whether an old node is the column a date part function's first argument is read as, <c>DATEADD (dd, …)</c>,
	/// whose word the new tree keeps without its case.
	/// </summary>
	static bool DatePart(object node, List<Step> path)
	{
		return node is Expression.ColumnReference
			&& path is [.., { Node: Expression.RoutineInvocation call, Member: "Arguments", Index: 0 }]
			&& Syntax.IsDatePartFunction(call.Name.ToUpperInvariant());
	}

	/// <summary>
	/// The scoped member an old column reference's text counts as where it is an index DROP INDEX names or a signer DROP
	/// SIGNATURE names, or null: everywhere else a column reference's text compares exactly.
	/// </summary>
	static string? Dropped(object node, List<Step> path)
	{
		return node is Expression.ColumnReference && path is [.., { Member: "Names" or "By", Index: not null } step]
			? step.Node switch
			{
				Statement.DropIndex     => DropIndexName,
				Statement.DropSignature => SignerName,
				Statement.AddSignature  => SignerName,
				_                       => null,
			}
			: null;
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
