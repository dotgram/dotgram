using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
/// A node is anything that is an <see cref="ISqlSpan"/>, which every node of the tree is: its
/// <see cref="Ast.ISqlNode"/> is one. What a node holds of them — one, or a list of nodes or of lists
/// of them, which may be absent — is found from its type once and kept, so a record added to the tree is
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

			if (node is Ast.QualifiedName { Count: 0 })
				throw new InvalidOperationException("A name of no parts, default(QualifiedName), is no name and is not walked.");

			if (!visit(node))
				return false;

			// A name is a value of its parts, which its properties hand out more than once (First,
			// Last, Parts): its parts are what it holds, each once, read by the indexer.
			if (node is Ast.QualifiedName name)
			{
				for (var at = name.Count - 1; at >= 0; at--)
					pending.Push(name[at]);

				continue;
			}

			held.Clear();

			foreach (var field in FieldsOf(node.GetType()))
				Gather(field.GetValue(node), held);

			// Backwards onto the stack, so that what the record holds first comes off first.
			for (var at = held.Count - 1; at >= 0; at--)
				pending.Push(held[at]);
		}

		return true;
	}

	/// <summary>The nodes a value is or holds: a node, or a list of them — or of lists of them, as a session's transaction modes are.</summary>
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

	/// <summary>
	/// The properties of a record that can hold a node, in the order they were declared — a base's
	/// first, except those it marks <see cref="WalkedLastAttribute"/>, which follow the record's own.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The type is a node's, and every node's type keeps its public properties where the application
	/// is trimmed: <see cref="ISqlSpan"/> asks for them for every type that implements it.
	/// </para>
	/// <para>
	/// The order is the base's first, by how far down the hierarchy each property is declared, and
	/// within one type the order reflection gives, which is the order of declaration. Not by metadata
	/// token: Native AOT keeps no tokens, and asking for one there throws.
	/// </para>
	/// </remarks>
	static PropertyInfo[] FieldsOf([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type type)
	{
		if (Fields.TryGetValue(type, out var known))
			return known;

		PropertyInfo[] found =
		[
			.. from property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
			   where property.GetIndexParameters().Length == 0 && Holds(property.PropertyType)
			   orderby property.IsDefined(typeof(WalkedLastAttribute), true) ? 1 : 0, Depth(property.DeclaringType)
			   select property,
		];

		return Fields.GetOrAdd(type, found);
	}

	/// <summary>How many types stand above this one, itself among them: <see cref="object"/> is 1.</summary>
	static int Depth(Type? type)
	{
		var depth = 0;

		for (; type is not null; type = type.BaseType)
			depth++;

		return depth;
	}

	// A node, or anything enumerable whose elements can hold one: a SqlList, an array, an
	// IReadOnlyList — or a list a record may leave out, which is a SqlList<T>? and so a Nullable.
	//
	// The element type is read off the list's own type, never off the interfaces it implements: a
	// trimmed application keeps a type's interfaces only where something asks for them, and
	// GetInterfaces cannot say what it needs. An enumerable of no shape known here is walked, and
	// Gather keeps whatever of it is a node: the answer is the same, only found later.
	static bool Holds(Type type)
	{
		type = Nullable.GetUnderlyingType(type) ?? type;

		if (typeof(ISqlSpan).IsAssignableFrom(type))
			return true;

		if (type == typeof(string))
			return false;

		if (ElementOf(type) is { } element)
			return Holds(element);

		return typeof(System.Collections.IEnumerable).IsAssignableFrom(type);
	}

	static Type? ElementOf(Type type)
	{
		if (type.IsArray)
			return type.GetElementType();

		if (!type.IsGenericType)
			return null;

		var definition = type.GetGenericTypeDefinition();

		return definition == typeof(SqlList<>) || definition == typeof(IEnumerable<>) || definition == typeof(IReadOnlyList<>)
			|| definition == typeof(IReadOnlyCollection<>) || definition == typeof(IList<>) || definition == typeof(ICollection<>)
			|| definition == typeof(List<>)
			? type.GetGenericArguments()[0]
			: null;
	}

	static readonly ConcurrentDictionary<Type, PropertyInfo[]> Fields = new();
}

/// <summary>
/// Marks a property of a family's base record that is written after what each member of the family
/// holds, so that <see cref="SqlWalker"/> visits it there too: a query's set operations and the clauses
/// of its whole result follow its select list or its brackets, where its WITH comes before them.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
internal sealed class WalkedLastAttribute : Attribute;
