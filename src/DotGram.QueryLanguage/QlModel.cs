using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DotGram.QueryLanguage;

/// <summary>
/// What query texts may name, frozen: reads a text, binds it to the model and the values passed with it, and
/// returns the query as an <see cref="IQueryable{T}"/> for the host to run. Thread-safe.
/// </summary>
/// <remarks>
/// Every method reads and binds the text (the binding is kept, by the exact text, for the next call with it),
/// checks the values, calls the sources of the entities the text names with the context, and builds a new
/// tree. Nothing runs: the host executes what is returned, with its provider, its timeout and its handling of
/// the provider's errors. A text or a value the language refuses throws <see cref="QlException"/>.
/// </remarks>
public sealed class QlModel<TContext>
{
	internal QlModel(Dictionary<string, ModelEntity> entities, Dictionary<Type, Delegate> formats, QlOptions options)
	{
		Entities = entities;
		Formats  = formats;
		Options  = options;
	}

	internal Dictionary<string, ModelEntity> Entities { get; }
	internal Dictionary<Type, Delegate>      Formats  { get; }

	/// <summary>The bounds every query of this model is held to.</summary>
	public QlOptions Options { get; }

	/// <summary>The rows, as arrays of values in the order of the columns, filled straight from the reader.</summary>
	/// <param name="context">What the entities' sources are called with.</param>
	/// <param name="text">The query.</param>
	/// <param name="values">The values of the parameters the text names, by name without the <c>@</c>.</param>
	/// <exception cref="QlException">The text, or a value, is refused.</exception>
	public IQueryable<object?[]> QueryData(TContext context, string text, IReadOnlyDictionary<string, object?>? values = null)
	{
		var bound     = Bind(text);
		var arguments = Arguments.Bind(bound, values, Options);

		return Translator.QueryData(bound, Source(bound.Entity, context), arguments, Options);
	}

	/// <summary>The rows of an entity for this context: its source, called for every query, so that it sees the context as it is now.</summary>
	static IQueryable Source(ModelEntity entity, TContext context)
	{
		var source = ((Func<TContext, IQueryable>)entity.Source)(context);

		if (source is null || source.ElementType != entity.Type)
			throw new InvalidOperationException($"The source of `{entity.Name}` returned {(source is null ? "null" : "rows of " + source.ElementType.Name)}.");

		return source;
	}

	/// <summary>The rows as JSON text, one element a row, written in the provider's reading of the rows.</summary>
	/// <param name="context">What the entities' sources are called with.</param>
	/// <param name="text">The query.</param>
	/// <param name="values">The values of the parameters the text names, by name without the <c>@</c>.</param>
	/// <param name="format">An array or an object a row.</param>
	/// <exception cref="QlException">The text, or a value, is refused.</exception>
	public IQueryable<string> QueryJsonRow(TContext context, string text, IReadOnlyDictionary<string, object?>? values = null, QlJsonFormat format = QlJsonFormat.Array)
	{
		throw new NotImplementedException();
	}

	/// <summary>Runs <see cref="QueryJsonRow"/> and returns the whole result as one JSON document.</summary>
	/// <param name="context">What the entities' sources are called with.</param>
	/// <param name="text">The query.</param>
	/// <param name="values">The values of the parameters the text names, by name without the <c>@</c>.</param>
	/// <param name="format">An array or an object a row.</param>
	/// <param name="cancellationToken">Stops the reading of rows.</param>
	/// <exception cref="QlException">The text, or a value, is refused.</exception>
	public Task<string> ToJsonAsync(TContext context, string text, IReadOnlyDictionary<string, object?>? values = null, QlJsonFormat format = QlJsonFormat.Array, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	/// <summary>Reads and binds <paramref name="text"/> without values: its columns, its parameters and its warnings.</summary>
	/// <exception cref="QlException">The text is refused.</exception>
	public QlQueryInfo Describe(string text)
	{
		return Bind(text).Info;
	}

	/// <summary>
	/// How many bound texts are kept. The key is a text somebody sent, so the cache is bounded: past the bound it is
	/// cleared whole, as the expression language's caches of what a text asked are, and the next calls bind again.
	/// What is kept is the text's binding alone — never a context, a value or a provider's query.
	/// </summary>
	internal const int CacheSize = 1024;

	readonly ConcurrentDictionary<string, BoundQuery> _bound = new(StringComparer.Ordinal);

	internal int CacheCount => _bound.Count;

	/// <summary>The text read and bound, from the cache where it was bound before.</summary>
	internal BoundQuery Bind(string text)
	{
		if (text is null)
			throw new ArgumentNullException(nameof(text));

		if (_bound.TryGetValue(text, out var bound))
			return bound;

		bound = Binder.Bind(Reader.Read(text, Options), Entities, Options);

		// Asked only on a miss: Count takes every lock of the dictionary.
		if (_bound.Count >= CacheSize)
			_bound.Clear();

		return _bound.GetOrAdd(text, bound);
	}
}

/// <summary>What a query text returns and needs, known before any value is passed.</summary>
public sealed class QlQueryInfo
{
	internal QlQueryInfo(IReadOnlyList<QlColumnInfo> columns, IReadOnlyList<QlParameterInfo> parameters, IReadOnlyList<QlWarning> warnings)
	{
		Columns    = columns;
		Parameters = parameters;
		Warnings   = warnings;
	}

	/// <summary>The columns of a row: the last <c>select</c>'s, or the fields with <see cref="QlUse.Select"/> of the entity.</summary>
	public IReadOnlyList<QlColumnInfo> Columns { get; }

	/// <summary>The parameters the text names, in the order it first names them.</summary>
	public IReadOnlyList<QlParameterInfo> Parameters { get; }

	/// <summary>What is worth telling whoever wrote the text.</summary>
	public IReadOnlyList<QlWarning> Warnings { get; }
}
