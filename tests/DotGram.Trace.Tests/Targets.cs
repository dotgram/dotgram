extern alias plain;
extern alias traced;
extern alias unfolded;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using PlainExpressions = plain::DotGram.ExpressionLanguage.ExpressionParser;
using PlainJson        = plain::DotGram.Web.Rfc8259;
using PlainSql         = plain::DotGram.Sql.TransactSql.TransactSqlParser;
using PlainUri         = plain::DotGram.Web.Rfc3986;
using TracedExpressions   = traced::DotGram.ExpressionLanguage.ExpressionParser;
using TracedJson          = traced::DotGram.Web.Rfc8259;
using TracedSql           = traced::DotGram.Sql.TransactSql.TransactSqlParser;
using TracedUri           = traced::DotGram.Web.Rfc3986;
using UnfoldedExpressions = unfolded::DotGram.ExpressionLanguage.ExpressionParser;
using UnfoldedJson        = unfolded::DotGram.Web.Rfc8259;
using UnfoldedSql         = unfolded::DotGram.Sql.TransactSql.TransactSqlParser;
using UnfoldedUri         = unfolded::DotGram.Web.Rfc3986;

namespace DotGram.Trace.Tests;

/// <summary>The publications the gates hold, each the three ways a <see cref="Target"/> reads it.</summary>
static class Targets
{
	static readonly Lazy<IReadOnlyList<Target>> _all = new(Made);

	public static IReadOnlyList<Target> All => _all.Value;

	public static Target Named(string name)
	{
		return All.Single(one => one.Name == name);
	}

	static IReadOnlyList<Target> Made()
	{
		// The expression language is read through the publication the grammar declares, with the
		// reading's own state, rather than through the host's TryParse: that one may answer a text
		// the grammar read with words of its own (a name nothing declares, a factory that threw), and
		// what a trace explains is what the parser said.
		var plainScope    = plain::DotGram.ExpressionLanguage.ResolutionScope.Around(typeof(Targets).Assembly);
		var tracedScope   = traced::DotGram.ExpressionLanguage.ResolutionScope.Around(typeof(Targets).Assembly);
		var unfoldedScope = unfolded::DotGram.ExpressionLanguage.ResolutionScope.Around(typeof(Targets).Assembly);

		Answer PlainExpression(string text)
		{
			return Caught(() => Of(PlainExpressions.TryParseLambda(text, new PlainExpressions.State(plainScope) { Text = text })));
		}

		var sql = new Target(
			"T-SQL",
			text => Of(PlainSql.TryParseSql(text)),
			text => Explain(typeof(TracedSql), () => Of(TracedSql.TryParseSql(text))),
			text => Explain(typeof(UnfoldedSql), () => Of(UnfoldedSql.TryParseSql(text))),
			text => Count(new SqlCount(), sink => TracedSql.Tracing(sink), () => Of(TracedSql.TryParseSql(text))),
			[])
		{
			Cap     = 2000,
			PerSeed = 3,
		};

		return
		[
			new Target(
				"JSON",
				text => Of(PlainJson.TryParseJson(text)),
				text => Explain(typeof(TracedJson), () => Of(TracedJson.TryParseJson(text))),
				text => Explain(typeof(UnfoldedJson), () => Of(UnfoldedJson.TryParseJson(text))),
				text => Count(new JsonCount(), sink => TracedJson.Tracing(sink), () => Of(TracedJson.TryParseJson(text))),
				Corpora.Json),
			new Target(
				"URI",
				text => Of(PlainUri.TryParseUri(text)),
				text => Explain(typeof(TracedUri), () => Of(TracedUri.TryParseUri(text))),
				text => Explain(typeof(UnfoldedUri), () => Of(UnfoldedUri.TryParseUri(text))),
				text => Count(new UriCount(), sink => TracedUri.Tracing(sink), () => Of(TracedUri.TryParseUri(text))),
				Corpora.Uris),
			new Target(
				"EL",
				PlainExpression,
				text => Explain(
					typeof(TracedExpressions),
					() => Caught(() => Of(TracedExpressions.TryParseLambda(text, new TracedExpressions.State(tracedScope) { Text = text })))),
				text => Explain(
					typeof(UnfoldedExpressions),
					() => Caught(() => Of(UnfoldedExpressions.TryParseLambda(text, new UnfoldedExpressions.State(unfoldedScope) { Text = text })))),
				text => Count(
					new ExpressionCount(), sink => TracedExpressions.Tracing(sink),
					() => Caught(() => Of(TracedExpressions.TryParseLambda(text, new TracedExpressions.State(tracedScope) { Text = text })))),
				[])
			{
				Recorded = Corpora.Expressions(),
			},
			sql with { Seeds = Corpora.Batches(text => sql.Plain(text).Ok) },
		];
	}

	/// <summary>A reading whose constructions may throw, as the expression language's host refuses with.</summary>
	static Answer Caught(Func<Answer> read)
	{
		try
		{
			return read();
		}
		catch (Exception thrown) when (thrown is FormatException or InvalidOperationException or OverflowException or ArgumentException)
		{
			return new Answer(false, thrown.GetType().Name + ": " + thrown.Message, -1, Thrown: true);
		}
	}

	static Answer Of<T>(plain::DotGram.Sql.TransactSql.TransactSqlParser.Match<T> match)
	{
		return new Answer(match.IsSuccess, match.Error, match.Position);
	}

	static Answer Of<T>(traced::DotGram.Sql.TransactSql.TransactSqlParser.Match<T> match)
	{
		return new Answer(match.IsSuccess, match.Error, match.Position);
	}

	static Answer Of<T>(unfolded::DotGram.Sql.TransactSql.TransactSqlParser.Match<T> match)
	{
		return new Answer(match.IsSuccess, match.Error, match.Position);
	}

	static Answer Of<T>(PlainJson.Match<T> match)
	{
		return new Answer(match.IsSuccess, match.Error, match.Position);
	}

	static Answer Of<T>(TracedJson.Match<T> match)
	{
		return new Answer(match.IsSuccess, match.Error, match.Position);
	}

	static Answer Of<T>(UnfoldedJson.Match<T> match)
	{
		return new Answer(match.IsSuccess, match.Error, match.Position);
	}

	static Answer Of<T>(PlainUri.Match<T> match)
	{
		return new Answer(match.IsSuccess, match.Error, match.Position);
	}

	static Answer Of<T>(TracedUri.Match<T> match)
	{
		return new Answer(match.IsSuccess, match.Error, match.Position);
	}

	static Answer Of<T>(UnfoldedUri.Match<T> match)
	{
		return new Answer(match.IsSuccess, match.Error, match.Position);
	}

	static Answer Of<T>(PlainExpressions.Match<T> match)
	{
		return new Answer(match.IsSuccess, match.Error, match.Position);
	}

	static Answer Of<T>(TracedExpressions.Match<T> match)
	{
		return new Answer(match.IsSuccess, match.Error, match.Position);
	}

	static Answer Of<T>(UnfoldedExpressions.Match<T> match)
	{
		return new Answer(match.IsSuccess, match.Error, match.Position);
	}

	/// <summary>
	/// A reading with the host's <c>GramWhy</c> attached, and what it said: read by reflection,
	/// since every traced host has a <c>GramWhy</c> of its own and they share no type.
	/// </summary>
	public static Explained Explain(Type host, Func<Answer> read)
	{
		var why = Activator.CreateInstance(host.GetNestedType("GramWhy")!)!;

		Answer answer;

		using (Scope(host, why))
			answer = read();

		object? Read(object of, string name)
		{
			return of.GetType().GetProperty(name)!.GetValue(of);
		}

		var stacks = ((System.Collections.IEnumerable)Read(why, "Paths")!)
			.Cast<object>()
			.Select(path => ((IEnumerable<string>)Read(path, "Rules")!).ToArray())
			.ToArray();

		return new Explained(
			answer,
			(bool)Read(why, "IsRefused")!,
			(string?)Read(why, "Message"),
			(long)Read(why, "Position")!,
			(string)Read(why, "Cause")!,
			stacks,
			((System.Collections.ICollection)Read(why, "Elements")!).Count);
	}

	/// <summary>The host's <c>Tracing</c> scope over a sink, by reflection.</summary>
	public static IDisposable Scope(Type host, object sink)
	{
		return (IDisposable)host.GetMethod("Tracing", BindingFlags.Public | BindingFlags.Static)!.Invoke(null, [sink])!;
	}

	static (Answer Answer, long Enters, long Exits, long Refusals) Count<T>(T sink, Func<T, IDisposable> scope, Func<Answer> read)
		where T : ICounted
	{
		Answer answer;

		using (scope(sink))
			answer = read();

		return (answer, sink.Enters, sink.Exits, sink.Refusals);
	}

	interface ICounted
	{
		long Enters { get; }

		long Exits { get; }

		long Refusals { get; }
	}

	// One sink a host, each counting the same three things: a host's sink is its own type.

	sealed class SqlCount : TracedSql.GramTrace, ICounted
	{
		public long Enters { get; private set; }

		public long Exits { get; private set; }

		public long Refusals { get; private set; }

		public override void Enter(int rule, int position)
		{
			Enters++;
		}

		public override void Exit(int rule, int position, int end)
		{
			Exits++;
		}

		public override void Refused(int position, string[]? expected)
		{
			Refusals++;
		}
	}

	sealed class JsonCount : TracedJson.GramTrace, ICounted
	{
		public long Enters { get; private set; }

		public long Exits { get; private set; }

		public long Refusals { get; private set; }

		public override void Enter(int rule, int position)
		{
			Enters++;
		}

		public override void Exit(int rule, int position, int end)
		{
			Exits++;
		}

		public override void Refused(int position, string[]? expected)
		{
			Refusals++;
		}
	}

	sealed class UriCount : TracedUri.GramTrace, ICounted
	{
		public long Enters { get; private set; }

		public long Exits { get; private set; }

		public long Refusals { get; private set; }

		public override void Enter(int rule, int position)
		{
			Enters++;
		}

		public override void Exit(int rule, int position, int end)
		{
			Exits++;
		}

		public override void Refused(int position, string[]? expected)
		{
			Refusals++;
		}
	}

	sealed class ExpressionCount : TracedExpressions.GramTrace, ICounted
	{
		public long Enters { get; private set; }

		public long Exits { get; private set; }

		public long Refusals { get; private set; }

		public override void Enter(int rule, int position)
		{
			Enters++;
		}

		public override void Exit(int rule, int position, int end)
		{
			Exits++;
		}

		public override void Refused(int position, string[]? expected)
		{
			Refusals++;
		}
	}
}
