using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DotGram.Sql;

/// <summary>
/// Every node of a tree, each put to one question.
/// </summary>
/// <remarks>
/// <para>
/// One walker and as many checks as anybody needs, rather than a method per check: a check is
/// a lambda that matches the nodes it is about by pattern and says whether to go on. An option
/// written twice, a table referenced by a name somebody forbids, a hint where a hint is
/// unwelcome — none of them is the parser's business, which reads what may be written and not
/// what should be, and each of them is a few lines against the nodes the tree already has.
/// </para>
/// <para>
/// A node is anything the five roots derive from, which is <see cref="ISqlSpan"/>: a
/// <see cref="Statement"/>, a <see cref="Query"/>, an <see cref="Expression"/>, a
/// <see cref="TableReference"/> or a <see cref="Clause"/> — and every node of the SQL:2023 tree,
/// whose <c>ISqlNode</c> is one too. What a node holds of them — one, an array, or a list, of nodes
/// or of lists of them — is found from its type once and kept, so a record added to either tree is
/// walked without this file hearing of it. Everything else a record holds is its own words and not
/// a node.
/// </para>
/// </remarks>
public static class SqlWalker
{
	/// <summary>
	/// Hands every node under <paramref name="root"/> to <paramref name="visit"/>, the root
	/// first and each node before what it holds, in the order the record holds it — until
	/// <paramref name="visit"/> answers false.
	/// </summary>
	/// <returns>Whether the walk went to the end: false where <paramref name="visit"/> ended it.</returns>
	public static bool Walk(ISqlSpan root, Func<ISqlSpan, bool> visit)
	{
		if (root is null)
			throw new ArgumentNullException(nameof(root));

		if (visit is null)
			throw new ArgumentNullException(nameof(visit));

		var pending = new Stack<ISqlSpan>();
		var held    = new List<ISqlSpan>();

		pending.Push(root);

		while (pending.Count > 0)
		{
			var node = pending.Pop();

			if (!visit(node))
				return false;

			held.Clear();

			foreach (var field in FieldsOf(node.GetType()))
				Gather(field.GetValue(node), held);

			// Backwards onto the stack, so that what the record holds first comes off first.
			for (var at = held.Count - 1; at >= 0; at--)
				pending.Push(held[at]);
		}

		return true;
	}

	/// <summary>The nodes a value is or holds: a node, or a list or an array of them — or of lists of them, as a session's transaction modes are.</summary>
	static void Gather(object? value, List<ISqlSpan> held)
	{
		switch (value)
		{
			case ISqlSpan one:
				held.Add(one);
				break;

			case string:
				break;

			case System.Collections.IEnumerable many:
				foreach (var item in many)
					Gather(item, held);

				break;
		}
	}

	/// <summary>The properties of a record that can hold a node, in the order they were declared.</summary>
	static PropertyInfo[] FieldsOf(Type type)
	{
		return Fields.GetOrAdd(type, static type =>
		[
			.. from property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
			   where property.GetIndexParameters().Length == 0 && Holds(property.PropertyType)
			   orderby property.MetadataToken
			   select property,
		]);
	}

	// A node, or anything enumerable whose elements can hold one: an array, an IReadOnlyList.
	static bool Holds(Type type)
	{
		return typeof(ISqlSpan).IsAssignableFrom(type) ||
		type != typeof(string) && ElementOf(type) is { } element && Holds(element);
	}

	static Type? ElementOf(Type type)
	{
		return type.IsArray
			? type.GetElementType()
			: (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>) ? type : type.GetInterfaces().FirstOrDefault(static one => one.IsGenericType && one.GetGenericTypeDefinition() == typeof(IEnumerable<>)))?.GetGenericArguments()[0];
	}

	static readonly ConcurrentDictionary<Type, PropertyInfo[]> Fields = new();
}
