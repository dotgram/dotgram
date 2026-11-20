using System;
using System.Linq.Expressions;
using System.Reflection;

namespace DotGram.QueryLanguage;

/// <summary>
/// The members a text may call, a closed table: the receiver's type, the name, the arguments, and exactly the CLR
/// member C# would choose for them. A name is looked for here and nowhere else — never by reflection over what
/// the text says.
/// </summary>
static class Members
{
	/// <summary>
	/// One member of the table. <c>Parameters</c> are the types of its arguments, null for a property; <c>Lifted</c>
	/// says whether a null receiver makes a null result, so that the result is declared nullable.
	/// </summary>
	public sealed record Entry(QlType Receiver, string Name, QlType[]? Parameters, QlType Result, bool Lifted)
	{
		/// <summary>The CLR member over a receiver of <paramref name="core"/>.</summary>
		public MemberInfo Member(Type core)
		{
			if (Parameters is null)
				return core.GetProperty(Name)!;

			return core.GetMethod(Name, Parameters.Length == 0 ? Type.EmptyTypes : [typeof(string)])!;
		}
	}

	static readonly Entry[] Table =
	[
		new(QlType.String, "Length",     null,              QlType.Int,     true),
		new(QlType.String, "Contains",   [QlType.String],   QlType.Bool,    false),
		new(QlType.String, "StartsWith", [QlType.String],   QlType.Bool,    false),
		new(QlType.String, "EndsWith",   [QlType.String],   QlType.Bool,    false),
		new(QlType.String, "ToUpper",    [],                QlType.String,  true),
		new(QlType.String, "ToLower",    [],                QlType.String,  true),
		new(QlType.String, "Trim",       [],                QlType.String,  true),
		.. Parts(QlType.Timestamp),
		.. Parts(QlType.TimestampTz),
	];

	static Entry[] Parts(QlType receiver)
	{
		return
		[
			new(receiver, "Year",   null, QlType.Int,       true),
			new(receiver, "Month",  null, QlType.Int,       true),
			new(receiver, "Day",    null, QlType.Int,       true),
			new(receiver, "Hour",   null, QlType.Int,       true),
			new(receiver, "Minute", null, QlType.Int,       true),
			new(receiver, "Second", null, QlType.Int,       true),
			new(receiver, "Date",   null, QlType.Timestamp, true),
		];
	}

	/// <summary>The member of that name, ignoring case as every name of the language does; null where there is none.</summary>
	public static Entry? Find(QlType receiver, string name)
	{
		foreach (var entry in Table)
			if (entry.Receiver == receiver && Names.Comparer.Equals(entry.Name, name))
				return entry;

		return null;
	}
}

/// <summary>The aggregates a <c>select</c> may hold.</summary>
static class Aggregates
{
	static readonly string[] All = ["Count", "Sum", "Avg", "Min", "Max"];

	public static bool Is(string name)
	{
		return Array.Exists(All, a => Names.Comparer.Equals(a, name));
	}

	/// <summary>The aggregate's own spelling, which names its column where the text gives none.</summary>
	public static string Name(string name)
	{
		return Array.Find(All, a => Names.Comparer.Equals(a, name))!;
	}

	public static AggregateKind KindOf(string name, bool star)
	{
		return Name(name) switch
		{
			"Count" => star ? AggregateKind.CountAll : AggregateKind.Count,
			"Sum"   => AggregateKind.Sum,
			"Avg"   => AggregateKind.Average,
			"Min"   => AggregateKind.Min,
			_       => AggregateKind.Max,
		};
	}
}

/// <summary>Whether a type outside the language has an operator, asked of the expression API that will build it.</summary>
static class Operators
{
	public static bool Exist(ExpressionType kind, Type type)
	{
		try
		{
			Expression.MakeBinary(kind, Expression.Parameter(type), Expression.Parameter(type));

			return true;
		}
		catch (InvalidOperationException)
		{
			return false;
		}
	}
}
