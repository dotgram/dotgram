using System;
using System.Collections.Generic;

using DotGram.ExpressionLanguage;
using DotGram.Sql.Standard;

namespace DotGram.Benchmarks;

static partial class Stand
{
	/// <summary>
	/// Nesting deep enough that a cost quadratic in DEPTH is visible, at two depths four times apart,
	/// so that a climbing ratio between them shows up inside one run.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <b>These rows exist because a quadratic was removed and no row could see it.</b> The
	/// materializer's fast path scanned the built flags from the walk's start to the root, so a
	/// tower of n guards walked a range growing with n, n times. Counted, that is 33,451,363
	/// elements at 800 levels against 4,050 after (benchmarks/results/allbuilt-2026-09-25), and the
	/// whole paired stand moved by less than its own spread — because the deepest thing on it nests
	/// seven levels. A change can only be seen by a row that pays for it.
	/// </para>
	/// <para>
	/// It is the same hole 584a7c1f fell into from the other side: that commit made the tape
	/// quadratic in a list's terms and no pair saw it, because no row was longer than twenty terms.
	/// The remedy there was `el/terms1000`; this is the remedy for depth, done once rather than
	/// after the next one.
	/// </para>
	/// <para>
	/// <b>Two depths, not one, and ten times apart.</b> A single deep row says a number that only
	/// means something against another run; two say an exponent inside this run, which survives a
	/// machine that was busy and a reader a month later. Quadratic reads about sixteen times from
	/// 100 to 400, linear about four.
	/// </para>
	/// <para>
	/// <b>The base is `control` and not `hand`.</b> The hand-written parsers have no depth guard at
	/// all — no hand-off, no <c>TryEnsureSufficientExecutionStack</c> — and die of a stack overflow
	/// somewhere past seventy levels on an ordinary thread
	/// (benchmarks/results/hand-against-generated-2026-09-24). A hand reading here would not be a
	/// slow row, it would be the stand's process gone with no verdict printed. The generated
	/// readers survive because they carry the reading onto a stack of their own.
	/// </para>
	/// <para>
	/// <b>The form is in the name, and the name is <c>.match</c> because that is what these are.</b>
	/// <c>TryParseSearchCondition(string)</c> and <c>TryParseLambda(string, State)</c> return a
	/// <c>Match</c> however their names read, and on a REFUSED input the <c>Match</c> form reads the
	/// input a SECOND time to record what was expected. That is a flat factor of two which belongs
	/// to the call and not to any commit, and it is the one thing that has already gone wrong in
	/// reading this stand: the <c>.bool</c> rows put the match form on one side and the quiet form
	/// on the other, and their movement was quoted as a result. Here every side makes the identical
	/// call, and the name says which, so nobody has to take that on trust.
	/// </para>
	/// <para>
	/// The expression language's rows carry the immediate carrier beside the tape. It has no walk to
	/// skip, so a change to the materializer cannot touch it: its before against its after is an A/A
	/// taken in the same process, in the same round order, and it is what says how much of a tape
	/// row's movement was the machine.
	/// </para>
	/// </remarks>
	static IEnumerable<Workload> PairedNesting(PairedSide before, PairedSide after)
	{
		foreach (var depth in new[] { 100, 400 })
		{
			var closed = new string('(', depth) + "a = 1" + new string(')', depth);
			var open   = new string('(', depth) + "a = 1";

			yield return Sql($"nested-{depth}.match",          closed);
			yield return Sql($"nested-refused-{depth}.match",  open);

			var lambda      = "(int x) => " + new string('(', depth) + "x" + new string(')', depth);
			var lambdaOpen  = "(int x) => " + new string('(', depth) + "x";

			yield return Expression($"nested-{depth}.match",         lambda);
			yield return Expression($"nested-refused-{depth}.match", lambdaOpen);
		}

		Workload Sql(string name, string text)
		{
			var accepts = !name.Contains("refused", StringComparison.Ordinal);
			var own     = (Func<int>)(() => SqlStandardParser.TryParseSearchCondition(text).IsSuccess ? 1 : 0);
			var b       = before.Sql("TryParseSearchCondition", text);
			var a       = after.Sql("TryParseSearchCondition", text);

			return new Workload("sql", name,
				[
					new Reading("control", own),
					new Reading("before",  b),
					new Reading("after",   a),
				],
				() => own() == b() && b() == a()
					? null
					: $"  control reads {own()}, before {b()}, after {a()}, and the row says it is {(accepts ? "accepted" : "refused")}");
		}

		Workload Expression(string name, string text)
		{
			var own = (Func<int>)(() => ExpressionParser.TryParseLambda(text, new ExpressionParser.State(Caller) { Text = text }).IsSuccess ? 1 : 0);
			var b   = before.El("TryParseLambda", text, immediate: false);
			var a   = after.El("TryParseLambda", text, immediate: false);
			var bi  = before.El("TryParseLambda", text, immediate: true);
			var ai  = after.El("TryParseLambda", text, immediate: true);

			return new Workload("el", name,
				[
					new Reading("control",          own),
					new Reading("before",           b),
					new Reading("after",            a),
					new Reading("before-immediate", bi),
					new Reading("after-immediate",  ai),
				],
				() => own() == b() && b() == a() && a() == bi() && bi() == ai()
					? null
					: $"  control reads {own()}, before {b()}, after {a()}, before-immediate {bi()}, after-immediate {ai()}");
		}
	}
}
