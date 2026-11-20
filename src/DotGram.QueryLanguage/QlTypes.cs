using System;

namespace DotGram.QueryLanguage;

/// <summary>
/// Where an exposed field may be named. A field named where it is not allowed is reported exactly as a
/// name the model does not have — the same code, message and position — so an error never tells whether
/// a field exists.
/// </summary>
/// <remarks>
/// <see cref="Filter"/> and <see cref="Sort"/> disclose a value even without <see cref="Select"/>: a
/// comparison against chosen values finds it, and an order shows how values compare. A field nobody may
/// learn is not exposed at all.
/// </remarks>
[Flags]
public enum QlUse
{
	/// <summary>
	/// In the value a query returns: a column of the last <c>select</c> (<c>*</c> included), or, in a
	/// query with no <c>select</c>, every field with this use.
	/// </summary>
	Select = 1,

	/// <summary>In a value a <c>where</c> tests, directly or through a <c>let</c> or a <c>select</c> column.</summary>
	Filter = 2,

	/// <summary>In a value an <c>order by</c> sorts by, directly or through a <c>let</c> or a <c>select</c> column.</summary>
	Sort = 4,
}

/// <summary>The scalar types of the language. Each is one CLR type; whether a value may be null is the field's.</summary>
public enum QlType
{
	/// <summary><see cref="bool"/>.</summary>
	Bool = 1,

	/// <summary><see cref="int"/>.</summary>
	Int = 2,

	/// <summary><see cref="long"/>.</summary>
	Long = 3,

	/// <summary><see cref="decimal"/>.</summary>
	Decimal = 4,

	/// <summary><see cref="double"/>.</summary>
	Double = 5,

	/// <summary><see cref="string"/>.</summary>
	String = 6,

	/// <summary><see cref="DateTime"/>.</summary>
	Timestamp = 7,

	/// <summary><see cref="DateTimeOffset"/>.</summary>
	TimestampTz = 8,

	/// <summary><see cref="System.Guid"/>.</summary>
	Guid = 9,

	/// <summary>
	/// A CLR type outside the nine above, which <see cref="QlColumnInfo.ClrType"/> or
	/// <see cref="QlParameterInfo.ClrType"/> names. Such a field is compared, sorted and returned as its own
	/// type; what the provider makes of it is the provider's.
	/// </summary>
	Other = 10,
}

/// <summary>The shape of a JSON row: an array in column order, or an object keyed by column name.</summary>
public enum QlJsonFormat
{
	/// <summary><c>[1,"paid"]</c>.</summary>
	Array = 0,

	/// <summary><c>{"id":1,"status":"paid"}</c>.</summary>
	Object = 1,
}

/// <summary>One column of what a query returns.</summary>
public sealed class QlColumnInfo
{
	internal QlColumnInfo(string name, QlType type, Type clrType, bool isNullable)
	{
		Name       = name;
		Type       = type;
		ClrType    = clrType;
		IsNullable = isNullable;
	}

	/// <summary>The column's name: its <c>as</c>, or the last part of the name it reads.</summary>
	public string Name { get; }

	/// <summary>The column's type.</summary>
	public QlType Type { get; }

	/// <summary>The CLR type of its values, without <see cref="Nullable{T}"/>.</summary>
	public Type ClrType { get; }

	/// <summary>Whether a value may be null.</summary>
	public bool IsNullable { get; }
}

/// <summary>One parameter a query text names, with the type the place it stands in gives it.</summary>
public sealed class QlParameterInfo
{
	internal QlParameterInfo(string name, QlType type, Type clrType, bool isList, bool isNullable)
	{
		Name       = name;
		Type       = type;
		ClrType    = clrType;
		IsList     = isList;
		IsNullable = isNullable;
	}

	/// <summary>The name without its <c>@</c>, as the text first spells it.</summary>
	public string Name { get; }

	/// <summary>The type of a value, or of each value of a list.</summary>
	public QlType Type { get; }

	/// <summary>The CLR type of a value, without <see cref="Nullable{T}"/>.</summary>
	public Type ClrType { get; }

	/// <summary>Whether the parameter takes a list: <c>x in @values</c>.</summary>
	public bool IsList { get; }

	/// <summary>Whether a value (or an item of a list) may be null.</summary>
	public bool IsNullable { get; }
}
