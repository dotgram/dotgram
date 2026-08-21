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
/// </remarks>
[MemoryDiagnoser]
public partial class CallCost
{
	const int Letters = 40;

	[Gram("""
		Start  : @string = t: Letter{40} => @(t)
		Letter = ['a'..'z']
		parse Start
		""")]
	public sealed partial class Inlined
	{
	}

	[Gram("""
		Start  : @string = t: Letter{40} => @(t)
		Letter = ['a'..'z'] | ('!' & Letter)
		parse Start
		""")]
	public sealed partial class Called
	{
	}

	/// <summary>
	/// The same grammar again, with the pooling hooks the generated parser offers filled in.
	/// </summary>
	/// <remarks>
	/// A one-slot pool, taken out while it is in use so that a parse reached from inside
	/// another gets its own. What it is here to answer is what the machinery costs when it is
	/// not rebuilt from nothing on every call.
	/// </remarks>
	[Gram("""
		Start  : @string = t: Letter{40} => @(t)
		Letter = ['a'..'z'] | ('!' & Letter)
		parse Start
		""")]
	public sealed partial class Pooled
	{
		[ThreadStatic]
		static Parser? _mine;

		static partial void RentParser(ref Parser parser)
		{
			parser = _mine!;
			_mine = null;
		}

		static partial void ReturnParser(Parser parser) => _mine = parser;
	}

	/// <summary>
	/// And again with the hooks answering that the caller supplies its own — which is a fresh
	/// one every time, so this is the machinery built from nothing, as the default used to be.
	/// </summary>
	[Gram("""
		Start  : @string = t: Letter{40} => @(t)
		Letter = ['a'..'z'] | ('!' & Letter)
		parse Start
		""")]
	public sealed partial class Unpooled
	{
		static partial void RentParser(ref Parser parser) => parser = new Parser();

		static partial void ReturnParser(Parser parser)
		{
		}
	}

	static readonly string Input = new('x', Letters);

	[Benchmark(Baseline = true)]
	public string? Compiled_in_place() => Inlined.ParseStart(Input);

	[Benchmark]
	public string? Called_as_a_rule() => Called.ParseStart(Input);

	[Benchmark]
	public string? Called_with_pooling() => Pooled.ParseStart(Input);

	[Benchmark]
	public string? Called_without_pooling() => Unpooled.ParseStart(Input);
}
