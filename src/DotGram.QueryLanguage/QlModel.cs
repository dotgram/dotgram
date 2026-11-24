using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
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
	internal QlModel(Dictionary<string, ModelEntity> entities, Dictionary<Type, Delegate> formats, LambdaExpression? like, QlOptions options)
	{
		Entities = entities;
		Formats  = formats;
		Like     = like;
		Options  = options;
	}

	internal Dictionary<string, ModelEntity> Entities { get; }
	internal Dictionary<Type, Delegate>      Formats  { get; }

	/// <summary>What <c>like</c> means, as the host declared it; null where it did not.</summary>
	internal LambdaExpression? Like { get; }

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

		return Translator.QueryData(bound, Source(bound.Entity, context), arguments, Options.MaxRows);
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
	/// <remarks>
	/// A row is <c>[1,"Ann",12.50]</c> in the order of the columns, or <c>{"id":1,"name":"Ann","total":12.50}</c>. The
	/// text is written by calls the provider makes as it reads each row, from the values as it reads them, without
	/// boxing them. <c>Count</c> and <c>Any</c> may follow. Paging belongs in the query (<c>skip</c>, <c>take</c>), and so
	/// do a filter and an order: over the text they cannot be translated.
	/// </remarks>
	/// <param name="context">What the entities' sources are called with.</param>
	/// <param name="text">The query.</param>
	/// <param name="values">The values of the parameters the text names, by name without the <c>@</c>.</param>
	/// <param name="format">An array or an object a row.</param>
	/// <exception cref="QlException">The text, or a value, is refused.</exception>
	public IQueryable<string> QueryJsonRow(TContext context, string text, IReadOnlyDictionary<string, object?>? values = null, QlJsonFormat format = QlJsonFormat.Array)
	{
		return JsonRows(context, text, values, format, Options.MaxRows, out _);
	}

	IQueryable<string> JsonRows(TContext context, string text, IReadOnlyDictionary<string, object?>? values, QlJsonFormat format, int? maxRows, out BoundQuery bound)
	{
		if (format is not (QlJsonFormat.Array or QlJsonFormat.Object))
			throw new ArgumentOutOfRangeException(nameof(format), format, "A row is an array or an object.");

		bound = Bind(text);

		var arguments = Arguments.Bind(bound, values, Options);

		return Translator.QueryJsonRow(bound, Source(bound.Entity, context), arguments, maxRows, format, Formats);
	}

	/// <summary>Runs <see cref="QueryJsonRow"/> and returns the whole result as one JSON document.</summary>
	/// <remarks>
	/// The document is <c>{"status":"started","columns":["id","name"],"rows":[[1,"Ann"]],"count":1,"truncated":false,"result":"completed"}</c>,
	/// with the rows as objects in <see cref="QlJsonFormat.Object"/>, and the text's warnings after the columns where
	/// it has any. <c>truncated</c> is true where <see cref="QlOptions.MaxRows"/> left rows out: one row past the bound
	/// is read to tell, and dropped. The rows are read through the provider's <see cref="IAsyncEnumerable{T}"/> where its
	/// query is one, and synchronously where not. The whole document is held in memory: <see cref="QlOptions.MaxRows"/>
	/// bounds the rows, nothing here bounds the size of the values in them. One row more than
	/// <see cref="QlOptions.MaxRows"/> is read, to tell a cut from an exact fit; it is formatted as the others are,
	/// so a host <c>Format&lt;T&gt;</c> that throws on it fails the document, though the row is not written. A refused text or value fails the returned
	/// task with <see cref="QlException"/>, whose <see cref="QlError.ToJson"/> is the document of the refusal; the
	/// provider's errors fail it with the provider's own exceptions, and nothing of them is written.
	/// </remarks>
	/// <param name="context">What the entities' sources are called with.</param>
	/// <param name="text">The query.</param>
	/// <param name="values">The values of the parameters the text names, by name without the <c>@</c>.</param>
	/// <param name="format">An array or an object a row.</param>
	/// <param name="cancellationToken">Stops the reading of rows.</param>
	/// <exception cref="QlException">The text, or a value, is refused.</exception>
	public async Task<string> ToJsonAsync(TContext context, string text, IReadOnlyDictionary<string, object?>? values = null, QlJsonFormat format = QlJsonFormat.Array, CancellationToken cancellationToken = default)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var maxRows = Options.MaxRows;

		// One row past the bound tells a result the bound cut from one that has exactly as many rows. Past int.MaxValue
		// there is no row to ask for, and the sum would wrap.
		var rows     = JsonRows(context, text, values, format, maxRows < int.MaxValue ? maxRows + 1 : maxRows, out var query);
		var document = new StringBuilder();
		var count    = 0;
		var cut      = false;

		QlJsonDocument.Start(document, query.Names, query.Info.Warnings);

		document.Append(",\"rows\":[");

		// The provider's own reading of the rows where it has one that does not block, its plain one where not.
		if (rows is IAsyncEnumerable<string> stream)
		{
			await foreach (var row in stream.WithCancellation(cancellationToken).ConfigureAwait(false))
			{
				if (count == maxRows)
				{
					cut = true;

					break;
				}

				Add(row);
			}
		}
		else
		{
			foreach (var row in rows)
			{
				cancellationToken.ThrowIfCancellationRequested();

				if (count == maxRows)
				{
					cut = true;

					break;
				}

				Add(row);
			}
		}

		document.Append(']');

		QlJsonDocument.Complete(document, count, cut);

		return document.ToString();

		void Add(string row)
		{
			if (count > 0)
				document.Append(',');

			document.Append(row);

			count++;
		}
	}

	/// <summary>Reads and binds <paramref name="text"/> without values: its columns, its parameters and its warnings.</summary>
	/// <remarks><see cref="QlQueryInfo.ToJson"/> is the same as JSON, and <see cref="QlError.ToJson"/> the refusal's.</remarks>
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

		bound = Binder.Bind(Reader.Read(text, Options), Entities, Like, Options);

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

	/// <summary>
	/// The description as JSON: <c>{"status":"started","columns":[{"name":"id","type":"Int","nullable":false},…],
	/// "parameters":[{"name":"low","type":"Decimal","nullable":false,"list":false},…],"result":"completed"}</c>, and
	/// <c>"warnings"</c> after the parameters where there are any. A type is the name of its <see cref="QlType"/>.
	/// A description reads no row, so it is whole when it is written, yet it opens and closes as the document of the
	/// rows does: <c>"status":"started"</c> first and <c>"result":"completed"</c> last.
	/// </summary>
	public string ToJson()
	{
		return QlJsonDocument.Describe(this);
	}
}
