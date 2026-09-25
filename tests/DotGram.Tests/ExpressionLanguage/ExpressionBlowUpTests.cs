using System;
using System.Diagnostics;
using System.Threading;

using DotGram.ExpressionLanguage;

using Xunit;

namespace DotGram.Tests.ExpressionLanguage;

/// <summary>
/// Anything opened and never closed is refused in the time its length takes, not in the time its
/// compositions take (D57).
/// </summary>
/// <remarks>
/// <para>
/// Every string body of the grammar repeats a piece whose plain-text alternative is a run of one
/// or more characters — <c>(A+)*</c>, the shape that reads a text every way it can be cut into
/// pieces once something after it fails. What fails after it is the closing quote, so the input
/// that drives it is a string that never closes: before the runs were made atomic, twenty-four
/// characters after <c>$"</c> took twenty seconds, and each further character doubled it.
/// </para>
/// <para>
/// The plain and verbatim strings have the same shape and were never reachable: they are whole
/// lexemes, read by the lexer's automaton, which has no way back to take. Only the four forms
/// the automaton merely begins — the interpolated ones, where a rule reads the rest — could be
/// driven, and they are the four held here.
/// </para>
/// <para>
/// The bound is a hundredfold of what the fix leaves and a ten-millionth of what the defect
/// took, so a machine under load cannot fail it and the defect cannot pass it. Sixty-four
/// characters is chosen for the same reason: were the shape back, this would not be slow, it
/// would not finish.
/// </para>
/// <para>
/// <b>What the strings above did not cover, and why.</b> They are one INSTANCE of the shape, not
/// the shape. On 2026-09-24 a fresh instance of D57 was written into this same grammar and these
/// tests stayed green: the tuple was added as three alternatives each beginning
/// <c>'(' &amp; Expression</c>, so an unclosed <c>(</c> cost ×2.95 for every one before it, and
/// nothing here reads a bracket. The second theory below is the general form: every way this
/// language has of opening something, driven unclosed, at two sizes.
/// </para>
/// <para>
/// It counts allocated bytes rather than seconds. A count needs no quiet machine, ignores the
/// JIT, and says the thing that matters directly: a quotient that climbs with the size is a
/// higher power, whatever the hardware.
/// </para>
/// <para>
/// Four against eight, and not a larger pair, because a test that FAILS is worth more than one
/// that hangs. At the ×2.95 this was written for, sixteen openers is tens of gigabytes and CI
/// would time out with nothing to read; eight is four megabytes and the quotient is 85 against a
/// bound of 8. The bound has that much slack because the flat shapes are not perfectly flat —
/// first sight of a shape costs something, and the noisiest of them measured 1.9 — so 8 is four
/// times the worst honest reading and a tenth of the cheapest dishonest one.
/// </para>
/// <para>
/// <b>The rule this is the check for</b>, which a test cannot enforce in general and a reviewer
/// can: no two alternatives of one rule may begin with the same terminal followed by the same
/// unbounded thing. Three that begin <c>'(' &amp; Expression</c> read what follows three times
/// over, which is <c>(A+)*</c> with the repetition spelled as alternatives. Where two forms share
/// a beginning, read the beginning once and let the tail say which form it was.
/// </para>
/// </remarks>
public sealed class ExpressionBlowUpTests
{
	[Theory]
	// The four beginnings, with a plain run after each.
	[InlineData("$\"", "a")]
	[InlineData("$@\"", "a")]
	[InlineData("$\"\"\"", "a")]
	[InlineData("$$\"\"\"", "a")]
	// And doubled braces, which are a second shape and not the first one's input: `{{a}}` is
	// two escaped braces around a run, and it was also a hole holding `{a}` until the hole was
	// told it cannot begin with a brace. Found by driving the fixed places with texts other
	// than the one the first defect was found with, which is the only thing that shows a shape
	// is gone rather than an input.
	[InlineData("$\"", "{{a}}")]
	[InlineData("$@\"", "{{a}}")]
	public void An_unclosed_string_is_refused_at_once(string opens, string piece)
	{
		var text  = "(int x) => " + opens + Repeated(piece, 64);
		var watch = Stopwatch.StartNew();

		var match = ExpressionParser.TryParse(text);

		Assert.False(match.IsSuccess);
		Assert.True(
			watch.Elapsed.TotalSeconds < 5,
			$"{opens} with 64 of `{piece}` after it took {watch.Elapsed.TotalSeconds:F1} s.");
	}

	[Theory]
	// Every way this language opens something, written and never closed. The text after the
	// opener is what makes the parse look for a way on: a bare run of openers can be refused
	// by the lexer without ever entering the rules that could multiply.
	[InlineData("a parenthesis",   "(int x) => ", "(", "x")]
	[InlineData("an index",        "(int x) => x", "[", "1")]
	[InlineData("a type argument", "(int x) => x", "<a", "")]
	[InlineData("a block",         "(int x) => ", "{", "x")]
	[InlineData("a hole",          "(int x) => $\"", "{(", "x")]
	[InlineData("an argument list", "(int x) => x", ".M(", "1")]
	public void What_is_opened_and_never_closed_grows_no_faster_than_its_length(
		string what, string head, string opener, string tail)
	{
		// The shape once at a small size, so that what is measured is the reading and not the
		// first sight of the code that does it.
		ExpressionParser.TryParse(Text(head, opener, 2, tail));

		var small = Allocated(Text(head, opener, 4, tail));
		var large = Allocated(Text(head, opener, 8, tail));

		Assert.True(
			large <= small * 8,
			$"{what}: 4 openers allocated {small:N0} bytes and 8 allocated {large:N0}, " +
			$"a factor of {(double)large / small:F1}. Doubling the count may cost a constant or a " +
			"multiple of it, never a power of it.");
	}

	static string Text(string head, string opener, int times, string tail)
	{
		return head + Repeated(opener, times) + tail;
	}

	/// <summary>
	/// One reading, thrown away, so the process pays the cost of first sight once and no
	/// measurement carries it.
	/// </summary>
	/// <remarks>
	/// Without it that cost lands on whichever reading happens to run first, which is decided
	/// by what the suite scheduled before: the parenthesis case was seen to read 164,744 bytes
	/// at four openers against 7,728 at eight, an order the wrong way round and the last
	/// non-deterministic thing left in this measurement. It is touched on the caller's thread,
	/// before the measuring thread is started, so that the initializer runs outside what is
	/// being counted.
	/// </remarks>
	static readonly bool Warmed = Warm();

	/// <summary>
	/// Present so the type is not <c>beforefieldinit</c>, which would let the runtime run
	/// <see cref="Warmed"/>'s initializer whenever it liked and let a JIT drop a dead read of it
	/// along with the initialization that read would have forced. Neither happens in practice;
	/// both would put the cost of first sight back inside a count, silently (expr, 2026-09-25).
	/// </summary>
	static ExpressionBlowUpTests()
	{
	}

	static bool Warm()
	{
		ExpressionParser.TryParse("(int x) => x");

		return true;
	}

	/// <summary>What one reading allocates, which is a count and so needs no quiet machine.</summary>
	/// <remarks>
	/// <para>
	/// <b>On a thread of its own, so that the reading begins from an empty pool.</b> The parser
	/// keeps its rented stores in <c>[ThreadStatic]</c> fields, so a reading taken on the
	/// suite's own thread measures whatever that thread's pool happened to be holding — which
	/// is decided by whichever unrelated case ran there before. Two readings taken that way can
	/// land in different regimes: one reuses a store and answers a kilobyte, the other grows one
	/// and answers thirty, and their ratio is then a property of the schedule and not of the
	/// parser. That is how this test came to read a CLIFF as a POWER, failing only at a thread
	/// count equal to the machine's cores and passing at every smaller one (performance-9f,
	/// 2026-09-25: the same input read 25.4x, 26.2x, 26.5x, 27.9x and 39.0x across runs, which
	/// no power does).
	/// </para>
	/// <para>
	/// A thread apiece makes the two readings symmetric and independent of how many threads the
	/// suite runs. <b>It is not the same as warming them</b>, which was the first fix tried and
	/// is worse: warming moves the store growth a parse genuinely needs out of BOTH readings,
	/// and a defect whose cost is exactly that growth would then be invisible. Here each reading
	/// still carries whatever it really has to allocate.
	/// </para>
	/// <para>
	/// <b>The form understates a defect, and that is the price of it.</b> A fresh thread pays
	/// about 8 KB for a pool of its own on BOTH sides, which is a constant added to numerator
	/// and denominator alike and so pulls every ratio toward one.
	/// </para>
	/// <para>
	/// <b>Measured against a real blow-up rather than argued.</b> The stand is the parser at
	/// <c>6c228d42</c> — the commit before <c>3a8d5bcd</c> read a parenthesis and a tuple once
	/// instead of as three alternatives each beginning <c>'(' &amp; Expression</c> — with THIS
	/// file laid over it. <b>It has to be this file, because this theory did not exist before</b>
	/// <b><c>3a8d5bcd</c>, which added it in the same commit as the fix</b>; an earlier tree has
	/// nothing to run, and two clean full-suite runs were spent discovering that from the other
	/// end. A comment that licenses a bound is the last place to carry a revision nobody can
	/// reproduce from.
	/// </para>
	/// <para>
	/// There the parenthesis fails at <b>85.0x</b> read on the suite's own thread, and at
	/// <b>71.4x</b> and <b>74.3x</b> in this form at one thread and at thirty-two (expr, on
	/// their own tree and by their own run: 75.0x, byte-identical at both counts). The five
	/// neighbouring shapes pass. So the teeth hold against the x8 bound by an order of
	/// magnitude — but a future bound must be set knowing that what this form sees is smaller
	/// than what is there.
	/// </para>
	/// </remarks>
	static long Allocated(string text)
	{
		// Touched here, on the caller's thread, so the initializer never runs inside the count.
		_ = Warmed;

		var allocated = 0L;

		// An explicit stack, because whoever extends this theory to larger counts will nest
		// deeper, and a stack overflow on a measuring thread takes the process down with no
		// message at all. Four and eight openers need none of it.
		var thread = new Thread(
			() =>
			{
				var before = GC.GetAllocatedBytesForCurrentThread();

				ExpressionParser.TryParse(text);

				allocated = GC.GetAllocatedBytesForCurrentThread() - before;
			},
			16 * 1024 * 1024);

		thread.Start();
		thread.Join();

		return allocated;
	}

	static string Repeated(string piece, int times)
	{
		var built = new System.Text.StringBuilder(piece.Length * times);

		for (var at = 0; at < times; at++)
			built.Append(piece);

		return built.ToString();
	}
}
