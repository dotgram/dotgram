using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

using DotGram.QueryLanguage.Syntax;

namespace DotGram.QueryLanguage;

/// <summary>
/// What a provider adds to the language: LIKE in its dialect, the methods that build it, and
/// execution with its timeout, caps and the translation of what it throws. The roots, their
/// mapping and the provider's configuration are the host's trusted code.
/// </summary>
public interface IQlAdapter
{
	/// <summary>A portable pattern ('%', '_', the text's escape) as the provider's own.</summary>
	string RewriteLikePattern(string pattern, char? escape);

	/// <summary>The test <c>value LIKE pattern</c>, over a pattern already rewritten.</summary>
	Expression Like(Expression value, Expression pattern);

	/// <summary>Whether the audit lets a call of this method stand: what <see cref="Like"/> builds.</summary>
	bool IsAllowed(MethodInfo method);

	IAsyncEnumerable<object?[]> ExecuteAsync(IQueryable<object?[]> query, QlLimits limits, CancellationToken cancellationToken);
}

/// <summary>
/// Where each entity's rows come from for a request: the host's queryable, with its policy.
/// Resolved per request, so a policy is never part of what is cached.
/// </summary>
public sealed class QlRoots<TContext>
{
	readonly Dictionary<QlEntity, Func<TContext, IQueryable>> _roots = [];

	public QlRoots<TContext> Root<T>(QlEntity<T> entity, Func<TContext, IQueryable<T>> root)
	{
		_roots.Add(entity, context => root(context));

		return this;
	}

	internal IQueryable Resolve(QlEntity entity, TContext context)
	{
		if (!_roots.TryGetValue(entity, out var root))
			throw new QlException(new QlError(QlErrorKind.UnknownName, $"Unknown source '{entity.Name}'."));

		return root(context);
	}
}

/// <summary>
/// A query read and bound: valid for one model, and holding nothing of a request.
/// </summary>
public sealed class QlQuery
{
	QlQuery(BoundQuery bound, QlBuiltModel model, QlLimits limits)
	{
		Bound   = bound;
		Model   = model;
		Limits  = limits;
		Columns = bound.Outputs.Select(o => o.Name).ToArray();
	}

	internal BoundQuery Bound  { get; }

	public QlBuiltModel          Model   { get; }
	public QlLimits              Limits  { get; }
	public IReadOnlyList<string> Columns { get; }

	public static QlQuery Parse(string text, QlBuiltModel model, QlLimits? limits = null)
	{
		limits ??= QlLimits.Default;

		var syntax = Read(text, limits);

		return new QlQuery(Binder.Bind(syntax, model, limits), model, limits);
	}

	internal static QuerySyntax Read(string text, QlLimits limits)
	{
		if (text.Length > limits.MaxTextLength)
			throw new QlException(new QlError(QlErrorKind.Limit, $"The query is longer than {limits.MaxTextLength} characters."));

		var match = QlGrammar.TryParseQuery(text);

		if (!match.IsSuccess)
			throw new QlException(new QlError(QlErrorKind.Syntax, match.Error ?? "Not a query.", (int)match.Position));

		return match.Value;
	}

	public IAsyncEnumerable<object?[]> ExecuteAsync<TContext>(
		QlRoots<TContext>                     roots,
		TContext                              context,
		IReadOnlyDictionary<string, object?>? parameters,
		IQlAdapter                            adapter,
		CancellationToken                     cancellationToken = default)
	{
		var query = Translate(roots.Resolve(Bound.Entity, context), parameters, adapter, out _);

		return adapter.ExecuteAsync(query, Limits, cancellationToken);
	}

	/// <summary>
	/// The queryable for one request, audited. Internal: handing it out would let a caller
	/// enumerate it past the adapter's controls.
	/// </summary>
	internal IQueryable<object?[]> Translate(IQueryable root, IReadOnlyDictionary<string, object?>? parameters, IQlAdapter adapter, out Audit.Stats stats)
	{
		if (root.ElementType != Bound.Entity.ClrType)
			throw new ArgumentException($"The root of '{Bound.Entity.Name}' is not a queryable of {Bound.Entity.ClrType}.", nameof(root));

		var values = Arguments.Resolve(Bound, parameters ?? EmptyParameters, adapter, Limits, out var take);
		var tree   = Translator.Translate(Bound, root.Expression, values, take, adapter);

		stats = Audit.Check(tree, root.Expression, Bound.Entity, adapter, Limits);

		return root.Provider.CreateQuery<object?[]>(tree);
	}

	static readonly IReadOnlyDictionary<string, object?> EmptyParameters = new Dictionary<string, object?>();
}

/// <summary>
/// The text's literals and the request's parameters, each checked against its slot and made the
/// slot's CLR type. A value that does not fit is refused here, before anything is translated.
/// </summary>
static class Arguments
{
	public static object?[] Resolve(BoundQuery query, IReadOnlyDictionary<string, object?> parameters, IQlAdapter adapter, QlLimits limits, out int take)
	{
		var values = new object?[query.Slots.Count];

		foreach (var slot in query.Slots)
		{
			values[slot.Index] = slot.Kind switch
			{
				SlotKind.Scalar      => Scalar(slot, slot.Parameter is null ? slot.Literal : Get(parameters, slot.Parameter, slot.Position), slot.Parameter, limits),
				SlotKind.List        => List(slot, parameters, limits),
				SlotKind.LikePattern => Pattern(slot, parameters, adapter, limits),
				_                    => Count(slot, parameters, limits, slot == query.Offset),
			};
		}

		var cap = limits.MaxRows + 1;

		take = query.Limit is { } limit ? Math.Min((int)values[limit.Index]!, cap) : cap;

		return values;
	}

	static object? Get(IReadOnlyDictionary<string, object?> parameters, string name, int position)
	{
		if (!parameters.TryGetValue(name, out var value))
			throw Error($"@{name} has no value.", position);

		return value;
	}

	static QlException Error(string message, int position)
	{
		return new QlException(new QlError(QlErrorKind.Parameter, message, position));
	}

	static object? Scalar(Slot slot, object? value, string? name, QlLimits limits)
	{
		var label = name is null ? "The value" : "@" + name;

		if (value is null)
		{
			if (!slot.Nullable)
				throw Error($"{label} may not be NULL here.", slot.Position);

			return null;
		}

		object? converted = (slot.Type, value) switch
		{
			(QlType.Bool,        bool b)           => b,
			(QlType.Int,         int i)            => i,
			(QlType.Long,        long l)           => l,
			(QlType.Long,        int i)            => (long)i,
			(QlType.Decimal,     decimal d)        => d,
			(QlType.Decimal,     int i)            => (decimal)i,
			(QlType.Decimal,     long l)           => (decimal)l,
			(QlType.Double,      double d)         => d,
			(QlType.Double,      int i)            => (double)i,
			(QlType.String,      string s)         => s,
			(QlType.Timestamp,   DateTime t)       => t,
			(QlType.TimestampTz, DateTimeOffset t) => t,
			(QlType.Guid,        Guid g)           => g,
			_                                      => null,
		};

		if (converted is null)
			throw Error($"{label} is not a {QlTypes.Name(slot.Type)}.", slot.Position);

		if (converted is double d2 && !double.IsFinite(d2))
			throw Error($"{label} is not a finite number.", slot.Position);

		if (converted is string s2 && s2.Length > limits.MaxStringLength)
			throw new QlException(new QlError(QlErrorKind.Limit, $"{label} is longer than {limits.MaxStringLength} characters.", slot.Position));

		return converted;
	}

	static Array List(Slot slot, IReadOnlyDictionary<string, object?> parameters, QlLimits limits)
	{
		var element = QlTypes.Clr(slot.Type, slot.Nullable);

		if (slot.Items is { } items)
		{
			var array = Array.CreateInstance(element, items.Length);

			for (var i = 0; i < items.Length; i++)
			{
				var item  = items[i];
				var value = item.Parameter is null ? item.Literal : Get(parameters, item.Parameter, item.Position);

				array.SetValue(Scalar(new Slot(slot.Index, SlotKind.Scalar, slot.Type, item.Parameter is not null && slot.Nullable, item.Position), value, item.Parameter, limits), i);
			}

			return array;
		}

		var given = Get(parameters, slot.Parameter!, slot.Position);

		// A list arrives whole: an array, counted before it is read, never a sequence that would
		// run the host's code as it is enumerated.
		if (given is not Array values || values.Rank != 1)
			throw Error($"@{slot.Parameter} is a list and wants an array.", slot.Position);

		if (values.Length > limits.MaxInList)
			throw new QlException(new QlError(QlErrorKind.Limit, $"@{slot.Parameter} has {values.Length} values; at most {limits.MaxInList} are allowed.", slot.Position));

		var result = Array.CreateInstance(element, values.Length);

		for (var i = 0; i < values.Length; i++)
			result.SetValue(Scalar(slot, values.GetValue(i), slot.Parameter, limits), i);

		return result;
	}

	static string Pattern(Slot slot, IReadOnlyDictionary<string, object?> parameters, IQlAdapter adapter, QlLimits limits)
	{
		var pattern = slot.Parameter is null ? (string)slot.Literal! : Get(parameters, slot.Parameter, slot.Position) as string;

		if (pattern is null)
			throw Error($"@{slot.Parameter} is a LIKE pattern and wants a string.", slot.Position);

		if (pattern.Length > limits.MaxStringLength)
			throw new QlException(new QlError(QlErrorKind.Limit, $"The pattern is longer than {limits.MaxStringLength} characters.", slot.Position));

		if (LikePatterns.Check(pattern, slot.Escape) is { } problem)
			throw Error(problem, slot.Position);

		return adapter.RewriteLikePattern(pattern, slot.Escape);
	}

	static int Count(Slot slot, IReadOnlyDictionary<string, object?> parameters, QlLimits limits, bool offset)
	{
		var value = slot.Parameter is null ? slot.Literal : Get(parameters, slot.Parameter, slot.Position);

		if (value is not int count || count < 0)
			throw Error($"{(slot.Parameter is null ? "The count" : "@" + slot.Parameter)} wants a non-negative int.", slot.Position);

		if (offset && count > limits.MaxOffset)
			throw new QlException(new QlError(QlErrorKind.Limit, $"OFFSET is at most {limits.MaxOffset}.", slot.Position));

		return count;
	}
}

/// <summary>
/// Bound queries by their exact text, for one model and one set of limits. A query is admitted
/// only once it has been read and bound; what was refused is not kept. Bounded: when full, it
/// starts again empty, which a spike can afford and a release would replace with eviction.
/// </summary>
public sealed class QlQueryCache(QlBuiltModel model, QlLimits limits, int capacity = 1024)
{
	readonly ConcurrentDictionary<string, QlQuery> _queries = new(StringComparer.Ordinal);

	public int Count
	{
		get
		{
			return _queries.Count;
		}
	}

	public QlQuery Get(string text)
	{
		if (_queries.TryGetValue(text, out var query))
			return query;

		query = QlQuery.Parse(text, model, limits);

		if (_queries.Count >= capacity)
			_queries.Clear();

		_queries.TryAdd(text, query);

		return query;
	}
}
