using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DotGram.QueryLanguage;

// What a built model knows, apart from the context type: the binder reads only this.

/// <summary>One exposed entity: its name, its rows' type, where its rows come from, and its fields in declaration order.</summary>
sealed class ModelEntity(string name, Type type, Delegate source, ModelField[] fields)
{
	public string       Name   { get; } = name;
	public Type         Type   { get; } = type;
	public Delegate     Source { get; } = source;
	public ModelField[] Fields { get; } = fields;

	readonly Dictionary<string, ModelField> _byName = fields.ToDictionary(f => f.Name, StringComparer.OrdinalIgnoreCase);

	/// <summary>The field of that name, whatever its uses; null where there is none.</summary>
	public ModelField? Find(string name)
	{
		return _byName.TryGetValue(name, out var field) ? field : null;
	}
}

/// <summary>One exposed member of an entity, under one name.</summary>
sealed class ModelField
{
	public ModelField(string name, MemberInfo member, QlUse use, int order)
	{
		Name   = name;
		Member = member;
		Use    = use;
		Order  = order;

		var type = member is PropertyInfo property ? property.PropertyType : ((FieldInfo)member).FieldType;

		Value = ScalarType.Of(type);

		if (!type.IsValueType)
			Value = Value.WithNullable(Nullability.MayBeNull(member));
	}

	public string     Name   { get; }
	public MemberInfo Member { get; }
	public QlUse      Use    { get; }
	public int        Order  { get; }
	public ScalarType Value  { get; }
}

/// <summary>
/// The type of a value as the language sees it: one of its types, or <see cref="QlType.Other"/> with the CLR
/// type; whether it may be null; and the CLR type an expression of it has.
/// </summary>
readonly struct ScalarType(QlType type, Type core, bool nullable)
{
	public QlType Type     { get; } = type;
	public Type   Core     { get; } = core;
	public bool   Nullable { get; } = nullable;

	/// <summary>The CLR type of an expression of this value: <c>int?</c> for a nullable int, <c>string</c> for any string.</summary>
	public Type Clr => Nullable && Core.IsValueType ? typeof(System.Nullable<>).MakeGenericType(Core) : Core;

	public bool IsNumeric => Type is QlType.Int or QlType.Long or QlType.Decimal or QlType.Double;

	public ScalarType WithNullable(bool nullable)
	{
		return new ScalarType(Type, Core, nullable);
	}

	public static ScalarType Of(Type clr)
	{
		var underlying = System.Nullable.GetUnderlyingType(clr);
		var core       = underlying ?? clr;
		var nullable   = underlying is not null || !clr.IsValueType;

		return new ScalarType(TypeOf(core), core, nullable);
	}

	public static ScalarType Of(QlType type, bool nullable)
	{
		return new ScalarType(type, ClrOf(type), nullable);
	}

	static QlType TypeOf(Type core)
	{
		if (core == typeof(bool))           return QlType.Bool;
		if (core == typeof(int))            return QlType.Int;
		if (core == typeof(long))           return QlType.Long;
		if (core == typeof(decimal))        return QlType.Decimal;
		if (core == typeof(double))         return QlType.Double;
		if (core == typeof(string))         return QlType.String;
		if (core == typeof(DateTime))       return QlType.Timestamp;
		if (core == typeof(DateTimeOffset)) return QlType.TimestampTz;
		if (core == typeof(Guid))           return QlType.Guid;

		return QlType.Other;
	}

	static Type ClrOf(QlType type)
	{
		return type switch
		{
			QlType.Bool        => typeof(bool),
			QlType.Int         => typeof(int),
			QlType.Long        => typeof(long),
			QlType.Decimal     => typeof(decimal),
			QlType.Double      => typeof(double),
			QlType.String      => typeof(string),
			QlType.Timestamp   => typeof(DateTime),
			QlType.TimestampTz => typeof(DateTimeOffset),
			QlType.Guid        => typeof(Guid),
			_                  => throw new ArgumentOutOfRangeException(nameof(type)),
		};
	}

	/// <summary>The type's name in a message: the language's word, or the CLR type's short name.</summary>
	public string Name => Type switch
	{
		QlType.Bool        => "bool",
		QlType.Int         => "int",
		QlType.Long        => "long",
		QlType.Decimal     => "decimal",
		QlType.Double      => "double",
		QlType.String      => "string",
		QlType.Timestamp   => "timestamp",
		QlType.TimestampTz => "timestamptz",
		QlType.Guid        => "guid",
		_                  => ShortName(Core),
	};

	/// <summary>A CLR type's name without its namespace or assembly, generic arguments by their short names too.</summary>
	public static string ShortName(Type type)
	{
		if (type.IsArray)
			return ShortName(type.GetElementType()!) + "[]";

		if (!type.IsGenericType)
			return type.Name;

		var name = type.Name;
		var tick = name.IndexOf('`');

		return (tick < 0 ? name : name.Substring(0, tick)) + "<" + string.Join(", ", type.GetGenericArguments().Select(ShortName)) + ">";
	}
}

/// <summary>
/// Whether a member of a reference type may be null, as the host's nullable annotations say. The compiler writes
/// them as attributes it declares in each assembly, so they are read by name, the same way on every framework: the
/// member's own <c>NullableAttribute</c>, else the <c>NullableContextAttribute</c> of the nearest type around it.
/// A member without annotations, or one they leave oblivious, may be null.
/// </summary>
static class Nullability
{
	const string NullableAttribute        = "System.Runtime.CompilerServices.NullableAttribute";
	const string NullableContextAttribute = "System.Runtime.CompilerServices.NullableContextAttribute";

	/// <summary>The annotation that says a value is never null.</summary>
	const byte NotNull = 1;

	public static bool MayBeNull(MemberInfo member)
	{
		if (Flag(member, NullableAttribute) is { } own)
			return own != NotNull;

		for (var type = member.DeclaringType; type is not null; type = type.DeclaringType)
			if (Flag(type, NullableContextAttribute) is { } context)
				return context != NotNull;

		return true;
	}

	/// <summary>The annotation of the value itself: the attribute's byte, or the first of its bytes, which is the outer type's.</summary>
	static byte? Flag(MemberInfo member, string attribute)
	{
		foreach (var data in member.GetCustomAttributesData())
		{
			if (data.AttributeType.FullName != attribute || data.ConstructorArguments.Count != 1)
				continue;

			var argument = data.ConstructorArguments[0];

			if (argument.Value is byte flag)
				return flag;

			if (argument.Value is System.Collections.Generic.IReadOnlyCollection<CustomAttributeTypedArgument> { Count: > 0 } flags && flags.First().Value is byte first)
				return first;
		}

		return null;
	}
}

/// <summary>What a name of the language may be.</summary>
static class Names
{
	/// <summary>Names are compared as the language compares them: ignoring case, culture-free.</summary>
	public static readonly StringComparer Comparer = StringComparer.OrdinalIgnoreCase;

	/// <summary>Whether <paramref name="name"/> is one of the three words a name may never be.</summary>
	public static bool IsReserved(string name)
	{
		return Comparer.Equals(name, "null") || Comparer.Equals(name, "true") || Comparer.Equals(name, "false");
	}

	/// <summary>Throws where a declared name could not be written in a text.</summary>
	public static void Check(string name, string what)
	{
		if (!IsName(name))
			throw new ArgumentException($"`{name}` is not a name a query can write: a {what} is named with letters, digits and underscores, not starting with a digit. Give it a name.");

		if (IsReserved(name))
			throw new ArgumentException($"`{name}` is a word of the language and cannot name a {what}: give it another name.");
	}

	static bool IsName(string name)
	{
		if (name.Length == 0 || !(Letter(name[0]) || name[0] == '_'))
			return false;

		foreach (var c in name)
			if (!(Letter(c) || c is >= '0' and <= '9' || c == '_'))
				return false;

		return true;
	}

	static bool Letter(char c)
	{
		return c is >= 'a' and <= 'z' or >= 'A' and <= 'Z';
	}
}
