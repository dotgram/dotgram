using System;
using System.Linq.Expressions;
using System.Reflection;

namespace DotGram.ExpressionLanguage;

// What a member of a value or of a type is: the property, the field, the element.
//
// Which of them a name means is the type's answer and not the syntax's — an array's
// length is a node of its own where every other type's is a property — so the asking
// is here and the grammar says only `a.b`.

public static partial class ExpressionParser
{
	/// <summary>Whether <see cref="Member"/> would have something to build, asked before it runs.</summary>
	/// <remarks>
	/// The same search <see cref="Member"/> makes, because what this answers has to be what
	/// that one does. No member is not an error here: it means this reading is not a member
	/// access, and something else will read the text.
	/// </remarks>
	public static bool Has(Expression target, string? name, Assembly caller)
	{
		if (target is null)
			throw new ArgumentNullException(nameof(target));

		return
			name is null ||
			target.Type.IsArray && string.Equals(name, "Length", StringComparison.Ordinal) ||
			InstanceMember(target.Type, name, caller) is not null;
	}

	/// <summary>What <c>a.b</c> reads, which the type of <c>a</c> decides.</summary>
	/// <remarks>
	/// An array's length is a node of this tree — <c>ArrayLength</c> — where every other
	/// type's is a property, and nothing in the syntax says which. It could not be a guard
	/// either: a guard runs while the text is read and the operand of a fold is not built
	/// until after, so the only place that can ask the operand what it is, is here.
	/// </remarks>
	/// <exception cref="FormatException">The type has no such property or field.</exception>
	public static Expression Member(Expression target, string name, Assembly caller)
	{
		if (target is null)
			throw new ArgumentNullException(nameof(target));

		if (target.Type.IsArray && string.Equals(name, "Length", StringComparison.Ordinal))
			return Expression.ArrayLength(target);

		return InstanceMember(target.Type, name, caller) switch
		{
			PropertyInfo property => Expression.Property(target, property),
			FieldInfo    field    => Expression.Field(target, field),
			_ => throw new FormatException($"'{target.Type.Name}' has no property or field named '{name}'."),
		};
	}

	/// <summary>
	/// The instance property or field a name means on a type, where C# in the calling
	/// assembly could reach it — public, or internal to that assembly — or null.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Not <c>Expression.PropertyOrField</c>, which answers a different question in two
	/// ways. It looks through no interface: an interface's base interfaces are not its base
	/// type, so `Count` on an `IList&lt;int&gt;` — which is `ICollection&lt;T&gt;`'s — was no
	/// member at all. And where nothing public matches it goes on to what is not public,
	/// which no C# written outside the type can read.
	/// </para>
	/// <para>
	/// The name exactly as written, as C# reads one. <c>PropertyOrField</c> goes on to the
	/// same name in another case, and <c>Expression.Call</c> does the same for a method, so
	/// both of them read `s.length` as `s.Length`.
	/// </para>
	/// </remarks>
	static MemberInfo? InstanceMember(Type type, string name, Assembly caller) =>
		Cached(
			_instanceMembers, (type, name, caller),
			static key => SearchedMember(key.Item1, key.Item2, key.Item3));

	/// <summary>The search <see cref="InstanceMember"/> makes, once for each type, name and caller.</summary>
	static MemberInfo? SearchedMember(Type type, string name, Assembly caller)
	{
		const BindingFlags Declared =
			BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

		// The type and what it derives from, and — for an interface — what it inherits.
		for (var each = type; each is not null; each = each.BaseType)
			if (DeclaredOn(each, name, Declared, caller) is { } found)
				return found;

		if (type.IsInterface)
			foreach (var inherited in type.GetInterfaces())
				if (DeclaredOn(inherited, name, Declared, caller) is { } found)
					return found;

		return null;
	}

	/// <summary>
	/// A field or a property that takes no index, declared by that type itself and reachable
	/// from the calling assembly. An array rather than <c>GetProperty</c>, which throws where
	/// it finds more than one.
	/// </summary>
	static MemberInfo? DeclaredOn(Type type, string name, BindingFlags flags, Assembly caller)
	{
		foreach (var member in type.GetMember(name, MemberTypes.Property | MemberTypes.Field, flags))
			if (Reachable(member, caller) &&
				(member is FieldInfo || member is PropertyInfo property && property.GetIndexParameters().Length == 0))
				return member;

		return null;
	}

	/// <summary>Whether C# written in the calling assembly could reach that member.</summary>
	/// <remarks>
	/// Public, or internal — `protected internal` included, being internal as well — to the
	/// assembly that calls. Never private or protected alone: nothing here is written inside
	/// the type or one derived from it. A property is reachable where either of its accessors
	/// is, as C# declares a property's accessibility and lets an accessor narrow it.
	/// </remarks>
	static bool Reachable(MemberInfo? member, Assembly caller) => member switch
	{
		FieldInfo field =>
			field.IsPublic || (field.IsAssembly || field.IsFamilyOrAssembly) && field.DeclaringType!.Assembly == caller,
		MethodBase method =>
			method.IsPublic || (method.IsAssembly || method.IsFamilyOrAssembly) && method.DeclaringType!.Assembly == caller,
		PropertyInfo property =>
			Reachable(property.GetMethod, caller) || Reachable(property.SetMethod, caller),
		_ => false,
	};

	/// <summary>The same element as a place to write rather than a value to read.</summary>
	/// <remarks>
	/// The API keeps the two apart where C# does not: <c>ArrayIndex</c> answers with a
	/// value and cannot be assigned to, <c>ArrayAccess</c> answers with the element itself.
	/// Which one `a[0]` means is decided by which side of the `=` it stands on, which the
	/// grammar knows and the API cannot.
	/// </remarks>
	public static Expression Place(Expression target, Expression[] at, Assembly caller)
	{
		if (target is null)
			throw new ArgumentNullException(nameof(target));

		if (at is null)
			throw new ArgumentNullException(nameof(at));

		return target.Type.IsArray
			? Expression.ArrayAccess(target, Converted(at, typeof(int)))
			: Indexed(target, at, caller);
	}

	/// <summary>What <c>a[i]</c> reads, likewise.</summary>
	/// <remarks>
	/// An array's element is a node of this tree and anything else's is an indexer — whose
	/// name is not always <c>Item</c>, `string` calling its own <c>Chars</c>. The type says
	/// which through its default member, which is what an indexer is, and which of several
	/// is meant is the same overload resolution a call makes.
	/// </remarks>
	public static Expression Indexed(Expression target, Expression[] at, Assembly caller)
	{
		if (target is null)
			throw new ArgumentNullException(nameof(target));

		if (at is null)
			throw new ArgumentNullException(nameof(at));

		if (target.Type.IsArray)
			return Expression.ArrayIndex(target, Converted(at, typeof(int)));

		var chosen = Resolved(Indexers(target.Type, at, caller), at, $"'{target.Type.Name}' has no indexer");

		return Expression.Property(target, (PropertyInfo)chosen.Member, Passed(chosen, at));
	}
	/// <summary>A static property or a static field, whichever that name is.</summary>
	/// <remarks>
	/// Two factories and one syntax: `T.Name` says nothing about which, and the type does.
	/// Looked for up the base types, as C# finds a static member through a derived type, and
	/// among the members the calling assembly could reach — the instance form's rule, which
	/// is <see cref="InstanceMember"/>.
	/// </remarks>
	public static Expression StaticMember(Type type, string name, Assembly caller)
	{
		if (type is null)
			throw new ArgumentNullException(nameof(type));

		return Cached(
			_staticMembers, (type, name, caller), static key => SearchedStatic(key.Item1, key.Item2, key.Item3))
			switch
			{
				PropertyInfo property => Expression.Property(null, property),
				FieldInfo    field    => Expression.Field(null, field),
				_                     => throw new FormatException($"'{type.Name}' has no static '{name}'."),
			};
	}

	/// <summary>The search <see cref="StaticMember"/> makes, once for each type, name and caller.</summary>
	static MemberInfo? SearchedStatic(Type type, string name, Assembly caller)
	{
		const BindingFlags Statics =
			BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly;

		for (var each = type; each is not null; each = each.BaseType)
			foreach (var member in each.GetMember(name, MemberTypes.Property | MemberTypes.Field, Statics))
				if (Reachable(member, caller))
					return member;

		return null;
	}
}
