using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DotGram.QueryLanguage;

/// <summary>
/// Where a field may be used. A field used where it is not allowed is reported exactly as one
/// that does not exist, so that a caller cannot learn what is hidden from them.
/// </summary>
[Flags]
public enum QlUse
{
	None   = 0,
	Select = 1,
	Filter = 2,
	Sort   = 4,
	All    = Select | Filter | Sort,
}

/// <summary>
/// The scalar types of the language. Each is one CLR type; nullability is the field's.
/// </summary>
public enum QlType
{
	Bool,
	Int,
	Long,
	Decimal,
	Double,
	String,
	Timestamp,
	TimestampTz,
	Guid,
}

/// <summary>
/// What the text may name: the roots, and on each the fields exposed and where each may be used.
/// Only this is visible to the binder; a member of the entity that is not declared here does not
/// exist for the text.
/// </summary>
public sealed class QlModel
{
	QlModel()
	{
	}

	readonly List<QlEntity> _entities = [];
	bool                    _built;

	public static QlModel Create()
	{
		return new QlModel();
	}

	public QlEntity<T> Entity<T>(string name, Expression<Func<T, object?>> key)
	{
		if (_built)
			throw new InvalidOperationException("The model has been built.");

		QlNames.Check(name);

		foreach (var existing in _entities)
			if (QlNames.Comparer.Equals(existing.Name, name))
				throw new ArgumentException($"An entity named '{name}' is declared already.", nameof(name));

		var entity = new QlEntity<T>(name, QlEntity.MemberOf(key, nameof(key)));

		_entities.Add(entity);

		return entity;
	}

	/// <summary>
	/// The model as the binder reads it: frozen, and the identity the query cache is keyed by.
	/// </summary>
	public QlBuiltModel Build()
	{
		_built = true;

		var entities = new Dictionary<string, QlEntity>(QlNames.Comparer);

		foreach (var entity in _entities)
		{
			entity.Freeze();
			entities.Add(entity.Name, entity);
		}

		return new QlBuiltModel(entities);
	}
}

/// <summary>
/// The model once built: immutable, and what a cached query is valid for.
/// </summary>
public sealed class QlBuiltModel
{
	internal QlBuiltModel(Dictionary<string, QlEntity> entities)
	{
		_entities = entities;
	}

	readonly Dictionary<string, QlEntity> _entities;

	internal QlEntity? FindEntity(string name)
	{
		return _entities.TryGetValue(name, out var entity) ? entity : null;
	}
}

/// <summary>
/// An exposed field: a name of the language over a member of the entity mapped directly.
/// </summary>
public sealed class QlColumn
{
	internal QlColumn(string name, MemberInfo member, QlType type, bool nullable, QlUse uses, int ordinal)
	{
		Name     = name;
		Member   = member;
		Type     = type;
		Nullable = nullable;
		Uses     = uses;
		Ordinal  = ordinal;
	}

	public string Name     { get; }
	public QlType Type     { get; }
	public bool   Nullable { get; }
	public QlUse  Uses     { get; }

	internal MemberInfo Member  { get; }
	internal int        Ordinal { get; }

	internal Type ClrType
	{
		get
		{
			return QlTypes.Clr(Type, Nullable);
		}
	}
}

public abstract class QlEntity
{
	private protected QlEntity(string name, Type clrType, MemberInfo key)
	{
		Name    = name;
		ClrType = clrType;
		Key     = key;
	}

	readonly List<QlColumn>               _columns = [];
	readonly Dictionary<string, QlColumn> _byName  = new(QlNames.Comparer);
	bool                                  _frozen;

	public string Name { get; }

	internal Type       ClrType { get; }
	internal MemberInfo Key     { get; }

	internal IReadOnlyList<QlColumn> Columns
	{
		get
		{
			return _columns;
		}
	}

	internal QlColumn? FindColumn(string name)
	{
		return _byName.TryGetValue(name, out var column) ? column : null;
	}

	internal void Freeze()
	{
		_frozen = true;
	}

	private protected void AddColumn(string name, LambdaExpression selector, QlUse uses)
	{
		if (_frozen)
			throw new InvalidOperationException("The model has been built.");

		QlNames.Check(name);

		if (_byName.ContainsKey(name))
			throw new ArgumentException($"A field named '{name}' is declared already on '{Name}'.", nameof(name));

		var member = MemberOf(selector, "selector");
		var clr    = member is PropertyInfo property ? property.PropertyType : ((FieldInfo)member).FieldType;

		if (!QlTypes.TryFromClr(clr, out var type, out var nullable))
			throw new ArgumentException($"'{member.Name}' is of type {clr}, which the language has no type for.", "selector");

		var column = new QlColumn(name, member, type, nullable, uses, _columns.Count);

		_columns.Add(column);
		_byName.Add(name, column);
	}

	/// <summary>
	/// The member a selector reads, refusing anything but a direct read of a member of the
	/// lambda's own parameter: a computation is not a column.
	/// </summary>
	internal static MemberInfo MemberOf(LambdaExpression selector, string argument)
	{
		var body = selector.Body;

		if (body is UnaryExpression { NodeType: ExpressionType.Convert } convert)
			body = convert.Operand;

		if (body is MemberExpression { Member: PropertyInfo or FieldInfo } member && member.Expression == selector.Parameters[0])
			return member.Member;

		throw new ArgumentException("A column is a member of the entity read directly, as in `o => o.Total`.", argument);
	}
}

public sealed class QlEntity<T> : QlEntity
{
	internal QlEntity(string name, MemberInfo key)
		: base(name, typeof(T), key)
	{
	}

	public QlEntity<T> Column<TValue>(string name, Expression<Func<T, TValue>> selector, QlUse uses = QlUse.All)
	{
		AddColumn(name, selector, uses);

		return this;
	}
}

static class QlNames
{
	public static readonly StringComparer Comparer = StringComparer.OrdinalIgnoreCase;

	static readonly Regex Valid = new("^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.CultureInvariant);

	public static void Check(string name)
	{
		if (!Valid.IsMatch(name))
			throw new ArgumentException($"'{name}' is not a name of the model: ASCII letters, digits and '_', not starting with a digit.", nameof(name));
	}
}

static class QlTypes
{
	public static Type Clr(QlType type, bool nullable)
	{
		var clr = type switch
		{
			QlType.Bool        => typeof(bool),
			QlType.Int         => typeof(int),
			QlType.Long        => typeof(long),
			QlType.Decimal     => typeof(decimal),
			QlType.Double      => typeof(double),
			QlType.String      => typeof(string),
			QlType.Timestamp   => typeof(DateTime),
			QlType.TimestampTz => typeof(DateTimeOffset),
			_                  => typeof(Guid),
		};

		return nullable && clr.IsValueType ? typeof(Nullable<>).MakeGenericType(clr) : clr;
	}

	public static bool TryFromClr(Type clr, out QlType type, out bool nullable)
	{
		var underlying = Nullable.GetUnderlyingType(clr);

		nullable = underlying is not null || !clr.IsValueType;

		var core = underlying ?? clr;

		QlType? found =
			core == typeof(bool)           ? QlType.Bool        :
			core == typeof(int)            ? QlType.Int         :
			core == typeof(long)           ? QlType.Long        :
			core == typeof(decimal)        ? QlType.Decimal     :
			core == typeof(double)         ? QlType.Double      :
			core == typeof(string)         ? QlType.String      :
			core == typeof(DateTime)       ? QlType.Timestamp   :
			core == typeof(DateTimeOffset) ? QlType.TimestampTz :
			core == typeof(Guid)           ? QlType.Guid        :
			                                 null;

		type = found ?? default;

		return found is not null;
	}

	public static bool IsNumeric(QlType type)
	{
		return type is QlType.Int or QlType.Long or QlType.Decimal or QlType.Double;
	}

	/// <summary>
	/// The type two numeric operands meet in, or null where the language converts neither
	/// implicitly: int to long to decimal, and int to double, and nothing that loses precision.
	/// </summary>
	public static QlType? Widen(QlType a, QlType b)
	{
		if (a == b)
			return a;

		return (a, b) switch
		{
			(QlType.Int, QlType.Long)     or (QlType.Long, QlType.Int)     => QlType.Long,
			(QlType.Int, QlType.Decimal)  or (QlType.Decimal, QlType.Int)  => QlType.Decimal,
			(QlType.Long, QlType.Decimal) or (QlType.Decimal, QlType.Long) => QlType.Decimal,
			(QlType.Int, QlType.Double)   or (QlType.Double, QlType.Int)   => QlType.Double,
			_                                                              => null,
		};
	}

	public static string Name(QlType type)
	{
		return type switch
		{
			QlType.Bool        => "boolean",
			QlType.Int         => "int",
			QlType.Long        => "long",
			QlType.Decimal     => "decimal",
			QlType.Double      => "double",
			QlType.String      => "string",
			QlType.Timestamp   => "timestamp",
			QlType.TimestampTz => "timestamptz",
			_                  => "guid",
		};
	}
}
