using System;
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
		throw new NotImplementedException();
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
		throw new NotImplementedException();
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
