using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DotGram.Parsers.Sql;

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
/// <see cref="TableReference"/> or a <see cref="Clause"/>. What a node holds of them — one, or
/// an array — is found from its type once and kept, so a record added to the tree is walked
/// without this file hearing of it. Everything else a record holds is its own words and not a
/// node.
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
				switch (field.GetValue(node))
				{
					case ISqlSpan one:
						held.Add(one);
						break;

					case Array many:
						foreach (var item in many)
							if (item is ISqlSpan one)
								held.Add(one);

						break;
				}

			// Backwards onto the stack, so that what the record holds first comes off first.
			for (var at = held.Count - 1; at >= 0; at--)
				pending.Push(held[at]);
		}

		return true;
	}

	/// <summary>The properties of a record that can hold a node, in the order they were declared.</summary>
	static PropertyInfo[] FieldsOf(Type type) => Fields.GetOrAdd(type, static type =>
		[
			.. from property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
			   where property.GetIndexParameters().Length == 0 && Holds(property.PropertyType)
			   orderby property.MetadataToken
			   select property,
		]);

	static bool Holds(Type type) =>
		typeof(ISqlSpan).IsAssignableFrom(type) ||
		type.IsArray && typeof(ISqlSpan).IsAssignableFrom(type.GetElementType());

	static readonly ConcurrentDictionary<Type, PropertyInfo[]> Fields = new();
}
