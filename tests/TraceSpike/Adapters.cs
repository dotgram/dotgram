#if DOTGRAM_GRAMTRACE
using System;

namespace TraceSpike;

sealed class TwiceFull : Twice.GramTrace
{
	readonly Recorder _r;
	public TwiceFull(Recorder r) { _r = r; }
	public override void Enter(int rule, int position) { _r.Enter(rule, position); }
	public override void Exit(int rule, int position, int end) { _r.Exit(rule, position, end); }
	public override void Refused(int position, string[]? expected) { _r.Refused(position, expected); }
}

sealed class TwiceCount : Twice.GramTrace
{
	public readonly long[] Counts = new long[Twice.TraceRules.Length];
	public override void Enter(int rule, int position) { Counts[rule]++; }
}

sealed class TwiceNull : Twice.GramTrace
{
}

static class TwiceSinks
{
	public static Sinks Of()
	{
		return new Sinks(
			Twice.TraceRules, Twice.TraceKinds,
			r => Twice.Tracing(new TwiceFull(r)),
			() => Twice.Tracing(new TwiceNull()),
			() => { var c = new TwiceCount(); return (Twice.Tracing(c), c.Counts); });
	}
}

sealed class UrlFull : Url.GramTrace
{
	readonly Recorder _r;
	public UrlFull(Recorder r) { _r = r; }
	public override void Enter(int rule, int position) { _r.Enter(rule, position); }
	public override void Exit(int rule, int position, int end) { _r.Exit(rule, position, end); }
	public override void Refused(int position, string[]? expected) { _r.Refused(position, expected); }
}

sealed class UrlCount : Url.GramTrace
{
	public readonly long[] Counts = new long[Url.TraceRules.Length];
	public override void Enter(int rule, int position) { Counts[rule]++; }
}

sealed class UrlNull : Url.GramTrace
{
}

static class UrlSinks
{
	public static Sinks Of()
	{
		return new Sinks(
			Url.TraceRules, Url.TraceKinds,
			r => Url.Tracing(new UrlFull(r)),
			() => Url.Tracing(new UrlNull()),
			() => { var c = new UrlCount(); return (Url.Tracing(c), c.Counts); });
	}
}

sealed class NotationFull : Notation.GramTrace
{
	readonly Recorder _r;
	public NotationFull(Recorder r) { _r = r; }
	public override void Enter(int rule, int position) { _r.Enter(rule, position); }
	public override void Exit(int rule, int position, int end) { _r.Exit(rule, position, end); }
	public override void Refused(int position, string[]? expected) { _r.Refused(position, expected); }
}

sealed class NotationCount : Notation.GramTrace
{
	public readonly long[] Counts = new long[Notation.TraceRules.Length];
	public override void Enter(int rule, int position) { Counts[rule]++; }
}

sealed class NotationNull : Notation.GramTrace
{
}

static class NotationSinks
{
	public static Sinks Of()
	{
		return new Sinks(
			Notation.TraceRules, Notation.TraceKinds,
			r => Notation.Tracing(new NotationFull(r)),
			() => Notation.Tracing(new NotationNull()),
			() => { var c = new NotationCount(); return (Notation.Tracing(c), c.Counts); });
	}
}

sealed class JsonFull : global::DotGram.Web.Rfc8259.GramTrace
{
	readonly Recorder _r;
	public JsonFull(Recorder r) { _r = r; }
	public override void Enter(int rule, int position) { _r.Enter(rule, position); }
	public override void Exit(int rule, int position, int end) { _r.Exit(rule, position, end); }
	public override void Refused(int position, string[]? expected) { _r.Refused(position, expected); }
}

sealed class JsonCount : global::DotGram.Web.Rfc8259.GramTrace
{
	public readonly long[] Counts = new long[global::DotGram.Web.Rfc8259.TraceRules.Length];
	public override void Enter(int rule, int position) { Counts[rule]++; }
}

sealed class JsonNull : global::DotGram.Web.Rfc8259.GramTrace
{
}

static class JsonSinks
{
	public static Sinks Of()
	{
		return new Sinks(
			global::DotGram.Web.Rfc8259.TraceRules, global::DotGram.Web.Rfc8259.TraceKinds,
			r => global::DotGram.Web.Rfc8259.Tracing(new JsonFull(r)),
			() => global::DotGram.Web.Rfc8259.Tracing(new JsonNull()),
			() => { var c = new JsonCount(); return (global::DotGram.Web.Rfc8259.Tracing(c), c.Counts); });
	}
}

sealed class UriFull : global::DotGram.Web.Rfc3986.GramTrace
{
	readonly Recorder _r;
	public UriFull(Recorder r) { _r = r; }
	public override void Enter(int rule, int position) { _r.Enter(rule, position); }
	public override void Exit(int rule, int position, int end) { _r.Exit(rule, position, end); }
	public override void Refused(int position, string[]? expected) { _r.Refused(position, expected); }
}

sealed class UriCount : global::DotGram.Web.Rfc3986.GramTrace
{
	public readonly long[] Counts = new long[global::DotGram.Web.Rfc3986.TraceRules.Length];
	public override void Enter(int rule, int position) { Counts[rule]++; }
}

sealed class UriNull : global::DotGram.Web.Rfc3986.GramTrace
{
}

static class UriSinks
{
	public static Sinks Of()
	{
		return new Sinks(
			global::DotGram.Web.Rfc3986.TraceRules, global::DotGram.Web.Rfc3986.TraceKinds,
			r => global::DotGram.Web.Rfc3986.Tracing(new UriFull(r)),
			() => global::DotGram.Web.Rfc3986.Tracing(new UriNull()),
			() => { var c = new UriCount(); return (global::DotGram.Web.Rfc3986.Tracing(c), c.Counts); });
	}
}

sealed class ElFull : global::DotGram.ExpressionLanguage.ExpressionParser.GramTrace
{
	readonly Recorder _r;
	public ElFull(Recorder r) { _r = r; }
	public override void Enter(int rule, int position) { _r.Enter(rule, position); }
	public override void Exit(int rule, int position, int end) { _r.Exit(rule, position, end); }
	public override void Refused(int position, string[]? expected) { _r.Refused(position, expected); }
}

sealed class ElCount : global::DotGram.ExpressionLanguage.ExpressionParser.GramTrace
{
	public readonly long[] Counts = new long[global::DotGram.ExpressionLanguage.ExpressionParser.TraceRules.Length];
	public override void Enter(int rule, int position) { Counts[rule]++; }
}

sealed class ElNull : global::DotGram.ExpressionLanguage.ExpressionParser.GramTrace
{
}

static class ElSinks
{
	public static Sinks Of()
	{
		return new Sinks(
			global::DotGram.ExpressionLanguage.ExpressionParser.TraceRules, global::DotGram.ExpressionLanguage.ExpressionParser.TraceKinds,
			r => global::DotGram.ExpressionLanguage.ExpressionParser.Tracing(new ElFull(r)),
			() => global::DotGram.ExpressionLanguage.ExpressionParser.Tracing(new ElNull()),
			() => { var c = new ElCount(); return (global::DotGram.ExpressionLanguage.ExpressionParser.Tracing(c), c.Counts); });
	}
}

sealed class SqlFull : global::DotGram.Sql.TransactSql.TransactSqlParser.GramTrace
{
	readonly Recorder _r;
	public SqlFull(Recorder r) { _r = r; }
	public override void Enter(int rule, int position) { _r.Enter(rule, position); }
	public override void Exit(int rule, int position, int end) { _r.Exit(rule, position, end); }
	public override void Refused(int position, string[]? expected) { _r.Refused(position, expected); }
}

sealed class SqlCount : global::DotGram.Sql.TransactSql.TransactSqlParser.GramTrace
{
	public readonly long[] Counts = new long[global::DotGram.Sql.TransactSql.TransactSqlParser.TraceRules.Length];
	public override void Enter(int rule, int position) { Counts[rule]++; }
}

sealed class SqlNull : global::DotGram.Sql.TransactSql.TransactSqlParser.GramTrace
{
}

static class SqlSinks
{
	public static Sinks Of()
	{
		return new Sinks(
			global::DotGram.Sql.TransactSql.TransactSqlParser.TraceRules, global::DotGram.Sql.TransactSql.TransactSqlParser.TraceKinds,
			r => global::DotGram.Sql.TransactSql.TransactSqlParser.Tracing(new SqlFull(r)),
			() => global::DotGram.Sql.TransactSql.TransactSqlParser.Tracing(new SqlNull()),
			() => { var c = new SqlCount(); return (global::DotGram.Sql.TransactSql.TransactSqlParser.Tracing(c), c.Counts); });
	}
}

#endif
