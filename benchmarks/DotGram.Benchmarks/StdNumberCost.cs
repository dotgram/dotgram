using System;
using System.Linq;

using BenchmarkDotNet.Attributes;

using DotGram;

namespace DotGram.Benchmarks;

/// <summary>
/// What the standard library's numbers cost since they refuse an overflow, piece by piece:
/// the atomic run, the guard, and the guard's own conversion.
/// </summary>
/// <remarks>
/// <para>
/// <c>Std.Integer</c> was <c>Digits =&gt; @(int.Parse(parserText, …))</c>. It is now an atomic
/// <c>Digits</c>, captured, a <c>when</c> that refuses a number too wide for <c>int</c>, and the
/// same <c>Parse</c> over the capture. Each of those is a method in the generated reader — the
/// atomic run a part (<c>ReadPart…</c>), the guard a static helper (<c>RecognizeGuard…</c>) — and
/// the first reading of a single-number regression was that each method boundary cost
/// 13–15 ns. The rows below take the change apart: <c>Old</c> holds a copy of the rules as they
/// were, <c>New</c> calls <c>Std</c>, and <c>Parts</c> isolates one piece per rule.
/// </para>
/// <para>
/// <b>The boundaries cost nothing.</b> The JIT inlines the part, the guard and the construction
/// into the publication (a single-number parse) and into the loop that reads a list (the
/// <c>Items</c> rows): the optimized code of <c>ReadPart0_Items</c> calls nothing of the grammar's,
/// only the string constructor and the BCL's parse (2026-11-13, <c>DOTNET_JitDisasm</c>). An
/// atomic run alone and a guard of <c>@(true)</c> alone read within a nanosecond of the plain rule.
/// </para>
/// <para>
/// What a guard naming its capture did cost, about 3 ns of a 30 ns parse, was the construction's
/// fallback cut written into the same method: a second span of the input turned into a string
/// made the JIT keep the guard's span in memory, a store and a load around the cut. That cut is a
/// reader method of its own now (<c>Recut_DotGram</c>), and <c>GuardOnCapture</c> reads as
/// <c>AtomicOnly</c> does.
/// </para>
/// <para>
/// What remains is the guard's own work, and it is the grammar's, not the generator's. A number
/// the guard cannot clear from its length — ten digits for <c>int</c>, nineteen for <c>long</c>, a
/// <c>double</c> longer than four characters — is converted twice, <c>TryParse</c> in the guard and
/// <c>Parse</c> in the construction.
/// </para>
/// <para>
/// Measured 2026-11-13 (BDN short job, a shared window, the generator with the out-of-line cut):
/// <c>Old_Integer</c> and <c>New_Integer</c> 27.5 ns both; <c>Parts_Text</c>, <c>AtomicOnly</c>,
/// <c>GuardTrue</c> and <c>GuardOnCapture</c> 20.0–21.7 ns; <c>Items20</c> 385 and 386 ns. Converted
/// twice: <c>Integer10</c> 34.1 → 48.6 ns, <c>Long19</c> 45.2 → 65.1, <c>DoubleExponent</c> 55.2 → 82.9.
/// <c>GuardOnText</c>, 29.5 ns and twice the bytes, is a guard naming <c>parserText</c>: its text is
/// cut for the guard and again for the construction.
/// </para>
/// </remarks>
[MemoryDiagnoser]
public partial class StdNumberCost
{
	[Gram("""
		Digits  = Std.Digit+
		Integer : int     = Digits => @(int.Parse(parserText, global::System.Globalization.CultureInfo.InvariantCulture))
		Long    : long    = Digits => @(long.Parse(parserText, global::System.Globalization.CultureInfo.InvariantCulture))
		Decimal : decimal = Digits & ('.' & Digits)?
			=> @(decimal.Parse(parserText, global::System.Globalization.NumberStyles.AllowDecimalPoint, global::System.Globalization.CultureInfo.InvariantCulture))
		Double  : double  = Digits & ('.' & Digits)? & ('e'i & ['+' | '-']? & Digits)?
			=> @(double.Parse(parserText, global::System.Globalization.NumberStyles.Float, global::System.Globalization.CultureInfo.InvariantCulture))

		I     : @int     = n: Integer & eof => @(n)
		L     : @long    = n: Long    & eof => @(n)
		M     : @decimal = n: Decimal & eof => @(n)
		D     : @double  = n: Double  & eof => @(n)
		Item  : @int     = v: Integer & ',' => @(v)
		Items : @int     = xs: Item* & eof => @(xs.Length)
		parse I
		parse L
		parse M
		parse D
		parse Items
		""")]
	public static partial class Old;

	[Gram("""
		I     : @int     = n: Std.Integer & eof => @(n)
		L     : @long    = n: Std.Long    & eof => @(n)
		M     : @decimal = n: Std.Decimal & eof => @(n)
		D     : @double  = n: Std.Double  & eof => @(n)
		Item  : @int     = v: Std.Integer & ',' => @(v)
		Items : @int     = xs: Item* & eof => @(xs.Length)
		parse I
		parse L
		parse M
		parse D
		parse Items
		""")]
	public static partial class New;

	/// <summary>One piece of the new <c>Integer</c> at a time, over a plain and an atomic run.</summary>
	[Gram("""
		Plain  = Std.Digit+
		Atomic = { Std.Digit+ }

		Text           : int = Plain => @(int.Parse(parserText, global::System.Globalization.CultureInfo.InvariantCulture))
		AtomicOnly     : int = Atomic => @(int.Parse(parserText, global::System.Globalization.CultureInfo.InvariantCulture))
		GuardTrue      : int = n: Atomic & when @(true) => @(int.Parse(n, global::System.Globalization.CultureInfo.InvariantCulture))
		GuardOnCapture : int = n: Atomic & when @(n.Length <= 9) => @(int.Parse(n, global::System.Globalization.CultureInfo.InvariantCulture))
		GuardOnText    : int = Plain & when @(parserText.Length <= 9) => @(int.Parse(parserText, global::System.Globalization.CultureInfo.InvariantCulture))
		parse Text
		parse AtomicOnly
		parse GuardTrue
		parse GuardOnCapture
		parse GuardOnText
		""")]
	public static partial class Parts;

	// Instance fields, which the JIT cannot read as constants: a literal (or a static readonly
	// string) handed to a publication inlined whole lets it fold the input's length into the
	// parse, and the rows then measure what the inliner managed to fold rather than the parser.
	// With literals the plain rule read 20 ns and the same rule under a guard of @(true) 35.
	readonly string _integer   = "12345";
	readonly string _integer10 = "1234567890";
	readonly string _long19    = "1234567890123456789";
	readonly string _decimal   = "3.25";
	readonly string _exponent  = "1.5e3";
	readonly string _items     = string.Concat(Enumerable.Repeat("12345,", 20));

	[Benchmark(Baseline = true)]
	public int Old_Integer()
	{
		return Old.ParseI(_integer);
	}

	[Benchmark]
	public int New_Integer()
	{
		return New.ParseI(_integer);
	}

	[Benchmark]
	public int Old_Integer10()
	{
		return Old.ParseI(_integer10);
	}

	[Benchmark]
	public int New_Integer10()
	{
		return New.ParseI(_integer10);
	}

	[Benchmark]
	public long Old_Long19()
	{
		return Old.ParseL(_long19);
	}

	[Benchmark]
	public long New_Long19()
	{
		return New.ParseL(_long19);
	}

	[Benchmark]
	public decimal Old_Decimal()
	{
		return Old.ParseM(_decimal);
	}

	[Benchmark]
	public decimal New_Decimal()
	{
		return New.ParseM(_decimal);
	}

	[Benchmark]
	public double Old_Double()
	{
		return Old.ParseD(_decimal);
	}

	[Benchmark]
	public double New_Double()
	{
		return New.ParseD(_decimal);
	}

	[Benchmark]
	public double Old_DoubleExponent()
	{
		return Old.ParseD(_exponent);
	}

	[Benchmark]
	public double New_DoubleExponent()
	{
		return New.ParseD(_exponent);
	}

	[Benchmark]
	public int Old_Items20()
	{
		return Old.ParseItems(_items);
	}

	[Benchmark]
	public int New_Items20()
	{
		return New.ParseItems(_items);
	}

	[Benchmark]
	public int Parts_Text()
	{
		return Parts.ParseText(_integer);
	}

	[Benchmark]
	public int Parts_AtomicOnly()
	{
		return Parts.ParseAtomicOnly(_integer);
	}

	[Benchmark]
	public int Parts_GuardTrue()
	{
		return Parts.ParseGuardTrue(_integer);
	}

	[Benchmark]
	public int Parts_GuardOnCapture()
	{
		return Parts.ParseGuardOnCapture(_integer);
	}

	[Benchmark]
	public int Parts_GuardOnText()
	{
		return Parts.ParseGuardOnText(_integer);
	}
}
