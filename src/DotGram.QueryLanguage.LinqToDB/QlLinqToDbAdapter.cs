using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using LinqToDB;
using LinqToDB.Async;
using LinqToDB.Data;
using LinqToDB.Internal.Linq;

namespace DotGram.QueryLanguage.LinqToDB;

/// <summary>
/// Which characters besides '%' and '_' a provider's LIKE gives a meaning to, and so has to
/// see escaped when the text means them literally.
/// </summary>
public enum QlLikeDialect
{
	/// <summary>'%' and '_' only: SQLite, PostgreSQL, the standard.</summary>
	Standard,

	/// <summary>Also '[' (a character class) and so ']' and '^' with it: SQL Server, Sybase.</summary>
	SqlServer,
}

/// <summary>
/// The query language over linq2db: LIKE through <see cref="Sql.Like(string, string, char?)"/>,
/// execution with a timeout, a row cap and a bound on how many run at once, and what execution
/// throws turned into a <see cref="QlError"/> that names nothing of the host. What was thrown is
/// handed to <see cref="Log"/> whole, for the host's log.
/// </summary>
public sealed class QlLinqToDbAdapter(QlLikeDialect dialect, int maxConcurrency = 0) : IQlAdapter
{
	const char Escape = '\\';

	static readonly MethodInfo LikeMethod = typeof(Sql).GetMethod(nameof(Sql.Like), [typeof(string), typeof(string), typeof(char?)])!;

	readonly SemaphoreSlim _running = new(maxConcurrency > 0 ? maxConcurrency : Math.Max(1, Environment.ProcessorCount / 2));

	/// <summary>Everything execution threw, before it became a <see cref="QlError"/>.</summary>
	public Action<Exception>? Log { get; init; }

	public string RewriteLikePattern(string pattern, char? escape)
	{
		var result = new StringBuilder(pattern.Length + 8);

		foreach (var (text, any) in LikePatterns.Split(pattern, escape))
		{
			if (text is null)
			{
				result.Append(any ? '%' : '_');
				continue;
			}

			foreach (var c in text)
			{
				if (c is '%' or '_' or Escape || dialect == QlLikeDialect.SqlServer && c is '[' or ']' or '^')
					result.Append(Escape);

				result.Append(c);
			}
		}

		return result.ToString();
	}

	public Expression Like(Expression value, Expression pattern)
	{
		return Expression.Call(LikeMethod, value, pattern, Expression.Constant(Escape, typeof(char?)));
	}

	public bool IsAllowed(MethodInfo method)
	{
		return method == LikeMethod;
	}

	public async IAsyncEnumerable<object?[]> ExecuteAsync(IQueryable<object?[]> query, QlLimits limits, [EnumeratorCancellation] CancellationToken cancellationToken)
	{
		using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

		timeout.CancelAfter(limits.CommandTimeout);

		// The command's own timeout as well: a provider that cannot be cancelled may still honour it.
		if (query is IExpressionQuery { DataContext: DataConnection connection })
			connection.CommandTimeout = Math.Max(1, (int)Math.Ceiling(limits.CommandTimeout.TotalSeconds));

		await Wait(timeout.Token, cancellationToken).ConfigureAwait(false);

		try
		{
			IAsyncEnumerator<object?[]> rows;

			try
			{
				rows = query.AsAsyncEnumerable().GetAsyncEnumerator(timeout.Token);
			}
			catch (Exception e)
			{
				throw Translate(e, cancellationToken, timeout.Token);
			}

			await using (rows.ConfigureAwait(false))
			{
				for (var count = 0; count < limits.MaxRows; count++)
				{
					bool next;

					try
					{
						next = await rows.MoveNextAsync().ConfigureAwait(false);
					}
					catch (Exception e)
					{
						throw Translate(e, cancellationToken, timeout.Token);
					}

					if (!next)
						yield break;

					yield return rows.Current;
				}
			}
		}
		finally
		{
			_running.Release();
		}
	}

	async Task Wait(CancellationToken timeout, CancellationToken caller)
	{
		try
		{
			await _running.WaitAsync(timeout).ConfigureAwait(false);
		}
		catch (OperationCanceledException e)
		{
			throw Translate(e, caller, timeout);
		}
	}

	QlException Translate(Exception e, CancellationToken caller, CancellationToken timeout)
	{
		if (e is QlException ql)
			return ql;

		Log?.Invoke(e);

		// Whatever a cancelled command throws — OperationCanceledException, or the provider's own
		// exception for "cancelled by user" — the reason is the token that fired.
		var error = e switch
		{
			_ when caller.IsCancellationRequested                          => new QlError(QlErrorKind.Canceled, "The query was canceled."),
			_ when timeout.IsCancellationRequested                         => new QlError(QlErrorKind.Timeout,  "The query took too long."),
			OperationCanceledException                                     => new QlError(QlErrorKind.Timeout,  "The query took too long."),
			DivideByZeroException                                          => new QlError(QlErrorKind.DivisionByZero, "Division by zero."),
			OverflowException                                              => new QlError(QlErrorKind.Overflow, "Arithmetic overflow."),
			DbException db when IsTimeout(db)                              => new QlError(QlErrorKind.Timeout,  "The query took too long."),
			DbException db when IsDivideByZero(db)                         => new QlError(QlErrorKind.DivisionByZero, "Division by zero."),
			DbException db when IsOverflow(db)                             => new QlError(QlErrorKind.Overflow, "Arithmetic overflow."),
			_                                                              => new QlError(QlErrorKind.Database, "The database refused the query."),
		};

		return new QlException(error, e);
	}

	// Provider errors are told apart by their numbers where the provider has them; the message
	// is never passed on.
	static bool IsTimeout(DbException e)
	{
		return e.GetType().Name == "SqlException" && Number(e) == -2;
	}

	static bool IsDivideByZero(DbException e)
	{
		return e.GetType().Name == "SqlException" && Number(e) == 8134;
	}

	static bool IsOverflow(DbException e)
	{
		return e.GetType().Name == "SqlException" && Number(e) is 8115 or 220 or 232;
	}

	static int Number(DbException e)
	{
		return e.GetType().GetProperty("Number")?.GetValue(e) is int number ? number : 0;
	}
}
