extern alias plain;
extern alias traced;
extern alias unfolded;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using PlainAccept      = plain::DotGram.Web.Rfc9110;
using PlainCookies     = plain::DotGram.Web.Rfc6265;
using PlainExpressions = plain::DotGram.ExpressionLanguage.ExpressionParser;
using PlainJson        = plain::DotGram.Web.Rfc8259;
using PlainSql         = plain::DotGram.Sql.TransactSql.TransactSqlParser;
using PlainStandard    = plain::DotGram.Sql.Standard.SqlStandardParser;
using PlainUri         = plain::DotGram.Web.Rfc3986;
using TracedAccept        = traced::DotGram.Web.Rfc9110;
using TracedCookies       = traced::DotGram.Web.Rfc6265;
using TracedExpressions   = traced::DotGram.ExpressionLanguage.ExpressionParser;
using TracedJson          = traced::DotGram.Web.Rfc8259;
using TracedSql           = traced::DotGram.Sql.TransactSql.TransactSqlParser;
using TracedStandard      = traced::DotGram.Sql.Standard.SqlStandardParser;
using TracedUri           = traced::DotGram.Web.Rfc3986;
using UnfoldedAccept      = unfolded::DotGram.Web.Rfc9110;
using UnfoldedCookies     = unfolded::DotGram.Web.Rfc6265;
using UnfoldedExpressions = unfolded::DotGram.ExpressionLanguage.ExpressionParser;
using UnfoldedJson        = unfolded::DotGram.Web.Rfc8259;
using UnfoldedSql         = unfolded::DotGram.Sql.TransactSql.TransactSqlParser;
using UnfoldedStandard    = unfolded::DotGram.Sql.Standard.SqlStandardParser;
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

			// Read by the engine: an Accept field, whose ranges a guard checks, and a Cookie field.
			new Target(
				"Accept",
				text => Of(PlainAccept.TryParseAccept(text)),
				text => Explain(typeof(TracedAccept), () => Of(TracedAccept.TryParseAccept(text))),
				text => Explain(typeof(UnfoldedAccept), () => Of(UnfoldedAccept.TryParseAccept(text))),
				text => Count(new AcceptCount(), sink => TracedAccept.Tracing(sink), () => Of(TracedAccept.TryParseAccept(text))),
				Corpora.Accepts),
			new Target(
				"Cookie",
				text => Of(PlainCookies.TryParseCookies(text)),
				text => Explain(typeof(TracedCookies), () => Of(TracedCookies.TryParseCookies(text))),
				text => Explain(typeof(UnfoldedCookies), () => Of(UnfoldedCookies.TryParseCookies(text))),
				text => Count(new CookieCount(), sink => TracedCookies.Tracing(sink), () => Of(TracedCookies.TryParseCookies(text))),
				Corpora.Cookies),

			// Read by one flat method: an SQL value.
			new Target(
				"SQL value",
				text => Of(PlainStandard.TryParseValue(text)),
				text => Explain(typeof(TracedStandard), () => Of(TracedStandard.TryParseValue(text))),
				text => Explain(typeof(UnfoldedStandard), () => Of(UnfoldedStandard.TryParseValue(text))),
				text => Count(new StandardCount(), sink => TracedStandard.Tracing(sink), () => Of(TracedStandard.TryParseValue(text))),
				Corpora.Values),
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

	static Answer Of<T>(PlainAccept.Match<T> match)
	{
		return new Answer(match.IsSuccess, match.Error, match.Position);
	}

	static Answer Of<T>(TracedAccept.Match<T> match)
	{
		return new Answer(match.IsSuccess, match.Error, match.Position);
	}

	static Answer Of<T>(UnfoldedAccept.Match<T> match)
	{
		return new Answer(match.IsSuccess, match.Error, match.Position);
	}

	static Answer Of<T>(PlainCookies.Match<T> match)
	{
		return new Answer(match.IsSuccess, match.Error, match.Position);
	}

	static Answer Of<T>(TracedCookies.Match<T> match)
	{
		return new Answer(match.IsSuccess, match.Error, match.Position);
	}

	static Answer Of<T>(UnfoldedCookies.Match<T> match)
	{
		return new Answer(match.IsSuccess, match.Error, match.Position);
	}

	static Answer Of<T>(PlainStandard.Match<T> match)
	{
		return new Answer(match.IsSuccess, match.Error, match.Position);
	}

	static Answer Of<T>(TracedStandard.Match<T> match)
	{
		return new Answer(match.IsSuccess, match.Error, match.Position);
	}

	static Answer Of<T>(UnfoldedStandard.Match<T> match)
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

	static (Answer Answer, Tally Tally) Count<T>(T sink, Func<T, IDisposable> scope, Func<Answer> read)
		where T : ICounted
	{
		Answer answer;

		using (scope(sink))
			answer = read();

		return (answer, sink.Tally);
	}

	interface ICounted
	{
		Tally Tally { get; }
	}

	/// <summary>
	/// What a counting sink saw: rules entered and left, refusals, and rules retracted — each of
	/// which must be one that read something, was left and was not retracted since.
	/// </summary>
	public sealed class Tally
	{
		readonly Stack<(int Rule, int At)> _open = new();
		readonly List<(int Rule, int At)> _read = [];

		public long Enters { get; private set; }

		public long Exits { get; private set; }

		public long Refusals { get; private set; }

		public long Retractions { get; private set; }

		/// <summary>Exits that left another rule than the innermost open, and retractions of a rule that had not read.</summary>
		public long Unmatched { get; private set; }

		public void Enter(int rule, int position)
		{
			Enters++;
			_open.Push((rule, position));
		}

		public void Exit(int rule, int position, int end)
		{
			Exits++;

			if (_open.Count == 0 || _open.Pop() != (rule, position))
				Unmatched++;
			else if (end >= 0)
				_read.Add((rule, position));
		}

		public void Retracted(int rule, int position)
		{
			Retractions++;

			var at = _read.LastIndexOf((rule, position));

			if (at < 0)
				Unmatched++;
			else
				_read.RemoveAt(at);
		}

		public void Refused()
		{
			Refusals++;
		}
	}

	// One sink a host, each handing what it is told to a tally: a host's sink is its own type.

	sealed class SqlCount : TracedSql.GramTrace, ICounted
	{
		public Tally Tally { get; } = new();

		public override void Enter(int rule, int position)
		{
			Tally.Enter(rule, position);
		}

		public override void Exit(int rule, int position, int end)
		{
			Tally.Exit(rule, position, end);
		}

		public override void Retracted(int rule, int position)
		{
			Tally.Retracted(rule, position);
		}

		public override void Refused(int position, string[]? expected)
		{
			Tally.Refused();
		}
	}

	sealed class JsonCount : TracedJson.GramTrace, ICounted
	{
		public Tally Tally { get; } = new();

		public override void Enter(int rule, int position)
		{
			Tally.Enter(rule, position);
		}

		public override void Exit(int rule, int position, int end)
		{
			Tally.Exit(rule, position, end);
		}

		public override void Retracted(int rule, int position)
		{
			Tally.Retracted(rule, position);
		}

		public override void Refused(int position, string[]? expected)
		{
			Tally.Refused();
		}
	}

	sealed class UriCount : TracedUri.GramTrace, ICounted
	{
		public Tally Tally { get; } = new();

		public override void Enter(int rule, int position)
		{
			Tally.Enter(rule, position);
		}

		public override void Exit(int rule, int position, int end)
		{
			Tally.Exit(rule, position, end);
		}

		public override void Retracted(int rule, int position)
		{
			Tally.Retracted(rule, position);
		}

		public override void Refused(int position, string[]? expected)
		{
			Tally.Refused();
		}
	}

	sealed class ExpressionCount : TracedExpressions.GramTrace, ICounted
	{
		public Tally Tally { get; } = new();

		public override void Enter(int rule, int position)
		{
			Tally.Enter(rule, position);
		}

		public override void Exit(int rule, int position, int end)
		{
			Tally.Exit(rule, position, end);
		}

		public override void Retracted(int rule, int position)
		{
			Tally.Retracted(rule, position);
		}

		public override void Refused(int position, string[]? expected)
		{
			Tally.Refused();
		}
	}

	sealed class AcceptCount : TracedAccept.GramTrace, ICounted
	{
		public Tally Tally { get; } = new();

		public override void Enter(int rule, int position)
		{
			Tally.Enter(rule, position);
		}

		public override void Exit(int rule, int position, int end)
		{
			Tally.Exit(rule, position, end);
		}

		public override void Retracted(int rule, int position)
		{
			Tally.Retracted(rule, position);
		}

		public override void Refused(int position, string[]? expected)
		{
			Tally.Refused();
		}
	}

	sealed class CookieCount : TracedCookies.GramTrace, ICounted
	{
		public Tally Tally { get; } = new();

		public override void Enter(int rule, int position)
		{
			Tally.Enter(rule, position);
		}

		public override void Exit(int rule, int position, int end)
		{
			Tally.Exit(rule, position, end);
		}

		public override void Retracted(int rule, int position)
		{
			Tally.Retracted(rule, position);
		}

		public override void Refused(int position, string[]? expected)
		{
			Tally.Refused();
		}
	}

	sealed class StandardCount : TracedStandard.GramTrace, ICounted
	{
		public Tally Tally { get; } = new();

		public override void Enter(int rule, int position)
		{
			Tally.Enter(rule, position);
		}

		public override void Exit(int rule, int position, int end)
		{
			Tally.Exit(rule, position, end);
		}

		public override void Retracted(int rule, int position)
		{
			Tally.Retracted(rule, position);
		}

		public override void Refused(int position, string[]? expected)
		{
			Tally.Refused();
		}
	}
}
