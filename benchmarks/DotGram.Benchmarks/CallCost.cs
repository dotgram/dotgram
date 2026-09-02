using System;

using BenchmarkDotNet.Attributes;

using DotGram;

namespace DotGram.Benchmarks;

/// <summary>
/// What a rule boundary costs, measured rather than reasoned about.
/// </summary>
/// <remarks>
/// <para>
/// The engine used to be methods — a rule was a C# method and a call was a call, so the
/// machine's own stack held the return address and its own registers held the position.
/// It is now one method and an explicit arena, because a method cannot be suspended and
/// resumed, and resuming is what backtracking across a rule boundary is. The price of that
/// is paid per call, and this is what it comes to.
/// </para>
/// <para>
/// Both grammars recognize the same forty letters and keep the same one string. They differ
/// in one thing: <c>Letter</c> in the second names itself in an alternative that the input
/// never reaches, which is enough to stop it being compiled into its caller. So one runs the
/// character tests as its caller's own control flow and the other calls a rule forty times,
/// and the difference divided by forty is the boundary.
/// </para>
/// <para>
/// <b>That pair prices the cheapest boundary there is, and for a long time it was read as
/// the price of a boundary.</b> A valueless callee writes a <c>Call</c> entry that its own
/// return takes straight back off the arena, and it leaves no <c>RuleCapture</c> behind —
/// so what those two measure is the dispatch and nothing else. Every rule in a grammar
/// that builds anything is the other kind: the <c>Call</c> entry is rewritten in place to
/// <c>Completed</c> rather than removed, a <c>RuleCapture</c> is appended naming it, and
/// the value is built from those records once the parse is accepted. <c>Valued</c> below
/// is that boundary, over the same forty letters, and the two differences are meant to be
/// read against each other.
/// </para>
/// <para>
/// The valued figure carries the value's own construction with it — forty one-character
/// strings — and cannot be made not to: a publication materializes what it recognized, and
/// a rule that builds nothing is the row above. So it is the boundary and its value
/// together, which is what a grammar actually pays per rule, and not an attempt to price
/// the arena traffic alone.
/// </para>
/// <para>
/// Measured 2026-08-31: 648 ns in place, 840 called, 1,823 called and valued, over forty
/// letters. So a valueless boundary is <b>4.8 ns</b> and a valued one <b>29 ns</b>, six
/// times it — and the second is the one a grammar is made of. What that buys is a scale
/// for the self-hosting gap: <c>GramGrammar</c> writes 1,014 rule captures reading
/// <c>Url.gram</c>, so its valued boundaries come to about 30 of the 113 microseconds it
/// is behind the hand-written parser. A quarter, and worth knowing before anything is
/// rebuilt to avoid them.
/// </para>
/// <para>
/// <b>2026-09-03.</b> These grammars are read by methods again — the direct rendering,
/// <c>Machine.Direct.cs</c> — and a file rendered that way has no arena and no
/// <c>Parser</c> to pool, so the two rows that priced the pooling are gone with it. The
/// figures above are what the engine cost when the engine was what ran; the three rows
/// that remain ask the same question of the methods.
/// </para>
/// </remarks>
[MemoryDiagnoser]
public partial class CallCost
{
	const int Letters = 40;

	[Gram("""
		Start  : @string = t: Letter+ => @(t)
		Letter = ['a'..'z']
		parse Start
		""")]
	public sealed partial class Inlined
	{
	}

	[Gram("""
		Start  : @string = t: Letter+ => @(t)
		Letter = ['a'..'z'] | ('!' & Letter)
		parse Start
		""")]
	public sealed partial class Called
	{
	}

	/// <summary>
	/// The same forty letters through a callee that builds a value, which is what a rule in
	/// a real grammar is.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <c>Letter</c> captures, so it has a value of its own: its <c>Call</c> entry survives
	/// its return as a <c>Completed</c>, a <c>RuleCapture</c> is appended naming it, and
	/// <c>Start</c>'s <c>t</c> becomes a collection of its values rather than one span of
	/// text. Those are the two records the valueless rows never write. The self-recursive
	/// alternative is kept exactly as it is above, so the value is the only thing that
	/// changed.
	/// </para>
	/// <para>
	/// The capture is what makes it valued, and writing <c>Letter : @string = ['a'..'z'] |
	/// '!' &amp; Letter</c> instead does not: a rule that captures nothing is worth the text
	/// it matched (§4.1 case 4), the declaration is dropped, and the grammar normalizes to
	/// the row above character for character. It was written that way here first, and the
	/// two rows measured the same thing under different names until the normalized shapes
	/// were compared.
	/// </para>
	/// </remarks>
	[Gram("""
		Letter : @string = c: ['a'..'z'] => @(c) | '!' & inner: Letter => @(inner)
		Start  : @string = t: Letter+ => @(string.Concat(t))
		parse Start
		""")]
	public sealed partial class Valued
	{
	}

	static readonly string Input = new('x', Letters);

	[Benchmark(Baseline = true)]
	public string? Compiled_in_place() => Inlined.ParseStart(Input);

	[Benchmark]
	public string? Called_as_a_rule() => Called.ParseStart(Input);

	[Benchmark]
	public string? Called_as_a_valued_rule() => Valued.ParseStart(Input);
}
