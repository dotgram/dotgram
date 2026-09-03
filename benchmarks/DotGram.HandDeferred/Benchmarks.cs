using System;
using System.Text;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;

namespace DotGram.HandDeferred;

/// <summary>
/// <c>ab = 0 + ab = 1 + …</c>, which is the shape that makes the fold long, with one
/// element in sixteen parenthesised and one in two hundred and fifty-six doubly so —
/// enough for the walk to have somewhere to go down into, and not so much that the input
/// becomes a tower.
/// </summary>
static class Inputs
{
	public static string Of(int pairs)
	{
		var written = new StringBuilder();

		for (var i = 0; i < pairs; i++)
		{
			if (i > 0)
				written.Append(" + ");

			var depth = i > 0 && i % 256 == 0 ? 2 : i > 0 && i % 16 == 0 ? 1 : 0;

			written.Append('(', depth).Append("ab = ").Append(i % 10).Append(')', depth);
		}

		return written.ToString();
	}
}

/// <summary>
/// Reading alone: what each representation costs to write down, with nothing of the
/// author's called.
/// </summary>
/// <remarks>
/// <para>
/// This is the half where the representations differ, and it is measured on its own
/// because the other half drowns it. The grammar's own construction is
/// <c>l + "+" + r</c> over a left-leaning fold, quadratic in the pairs: at ten thousand of
/// them one parse allocates half a gigabyte, the collector runs through everything, and
/// the pauses are charged to whoever happens to be running. Hand-timed in one loop it
/// produced two readings that allocate the same bytes to the byte differing by twenty
/// times, which is the measurement failing and not a result.
/// </para>
/// <para>
/// From twenty tokens to forty thousand, because the answer moves, and the column that
/// says why is Gen2:
/// </para>
/// <code>
///                 Pairs         Mean    Ratio      Gen0      Gen1      Gen2   Allocated
/// Tape                5       73.8 ns     1.00    0.0124         -         -       624 B
/// Mixed               5       35.8 ns     0.48    0.0041         -         -       208 B
/// Classes             5       80.5 ns     1.09    0.0116         -         -       584 B
/// Closures            5      163.0 ns     2.21    0.0393    0.0002         -     1,976 B
///
/// Tape            1,000   31,697.8 ns     1.00   31.2195   31.2195   31.2195   196,667 B
/// Mixed           1,000   10,268.4 ns     0.32    1.0529    0.1068         -    53,000 B
/// Arenas          1,000   12,928.6 ns     0.41    1.7242    0.2441         -    86,808 B
/// Classes         1,000   14,576.7 ns     0.46    2.2888    0.7172         -   115,144 B
/// Closures        1,000   36,169.7 ns     1.14    7.9346    4.4556         -   399,656 B
///
/// Tape           10,000  211,024.7 ns     1.00  396.4844  396.4844  396.4844 1,573,110 B
/// Classes        10,000  151,047.0 ns     0.72   22.9492   17.5781         - 1,151,848 B
/// Boxed          10,000  180,129.4 ns     0.85   22.9492   17.5781         - 1,151,848 B
/// Mixed          10,000  228,086.8 ns     1.08  199.9512  199.9512  199.9512   823,931 B
/// Arenas         10,000  256,891.7 ns     1.22  249.7559  249.7559  249.7559 1,344,892 B
/// Closures       10,000  428,714.3 ns     2.03   79.5898   73.7305         - 3,999,176 B
/// </code>
/// <para>
/// <b>The order reverses, and the large object heap is what reverses it.</b> Up to a
/// hundred pairs the readings that keep their derivation in arrays win, exactly as the
/// byte counts say they should: <c>Mixed</c> at half the tape and a third of its
/// allocation. At a thousand the tape has already doubled its one <c>Record[]</c> past
/// eighty-five kilobytes and every parse is now allocating a large object — thirty-one
/// gen2 collections per thousand operations — and it falls behind everything but the
/// closures. At ten thousand the value readings are there too, and the two that allocate
/// an object per node and no array at all, <c>Classes</c> and <c>Boxed</c>, are ahead of
/// all of them.
/// </para>
/// <para>
/// Which is not a verdict against arrays. It is a verdict against <em>growing a fresh one
/// per parse</em>: nothing here pools anything, and an array that were rented and returned
/// would never be allocated at this size at all, where an object per node has to be. The
/// engine's <c>Ways.Rent()</c> is that, and this is the measurement that says it is not an
/// optimization but the thing that keeps the representation standing.
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class Recognizing
{
	[Params(5, 20, 100, 1_000, 10_000)]
	public int Pairs;

	string _input = "";

	[GlobalSetup]
	public void Setup() => _input = Inputs.Of(Pairs);

	[Benchmark(Baseline = true)]
	public bool Tape()
	{
		var reader = new Reader(_input);

		return reader.Recognize();
	}

	[Benchmark] public bool Closures() => new Closures(_input).Recognize();

	[Benchmark] public bool Mixed() => new Mixed(_input).Recognize();

	[Benchmark] public bool Arenas() => new Arenas(_input).Recognize();

	[Benchmark] public bool Boxed() => new Boxed(_input).Recognize();

	[Benchmark] public bool Classes() => new Classes(_input).Recognize();
}

/// <summary>
/// Reading and building: the whole parse, which is what a caller actually asks for.
/// </summary>
/// <remarks>
/// <para>
/// What construction costs is this less <see cref="Recognizing"/>, and past a hundred
/// pairs it is nearly all of this — so the column says how much of the author's own
/// quadratic a representation manages to get out of the way of, which is not much and
/// should not be. A short job, because at ten thousand pairs one operation is fifty
/// milliseconds.
/// </para>
/// <code>
///                 Pairs         Mean    Ratio   Allocated
/// Tape                5      193.1 ns     1.00     1,432 B
/// Mixed               5      135.7 ns     0.70       880 B
/// Classes             5      176.0 ns     0.91     1,256 B
/// Arenas              5      240.8 ns     1.25     1,408 B
/// Closures            5      279.2 ns     1.45     2,648 B
///
/// Tape              100    5,968.6 ns     1.00    78,424 B
/// Mixed             100    4,467.0 ns     0.75    69,480 B
/// Boxed             100    5,516.3 ns     0.92    74,376 B
/// Arenas            100    6,764.9 ns     1.13    74,728 B
/// Closures          100    8,047.6 ns     1.35   102,760 B
///
/// Tape            1,000  325,772.3 ns     1.00 5,477,162 B
/// Mixed           1,000  254,517.3 ns     0.78 5,300,432 B
/// Arenas          1,000  280,796.4 ns     0.86 5,342,536 B
/// Classes         1,000  283,258.1 ns     0.87 5,362,576 B
/// Closures        1,000  305,855.2 ns     0.94 5,647,088 B
/// </code>
/// <para>
/// The spread closes as the input grows, which is the point: at five pairs the
/// representations are 0.70 to 1.45 of each other, at a thousand they are 0.78 to 0.94,
/// and the reason is in the allocation column — five megabytes of it, the same five for
/// everybody, and none of it theirs. Whatever a generator does about carrying a deferred
/// call, it is arguing over the tenth of a parse that is not the author's own code.
/// </para>
/// <para>
/// The ten thousand row is left out of the table above deliberately. Half a gigabyte an
/// operation puts the process in permanent collection, the readings come out between 0.36
/// and 1.00 with no order that survives a second run, and what is being measured there is
/// <c>string</c> concatenation.
/// </para>
/// </remarks>
[MemoryDiagnoser]
[SimpleJob(RunStrategy.Throughput, launchCount: 1, warmupCount: 3, iterationCount: 5)]
public class Parsing
{
	[Params(5, 20, 100, 1_000, 10_000)]
	public int Pairs;

	string _input = "";

	[GlobalSetup]
	public void Setup() => _input = Inputs.Of(Pairs);

	[Benchmark(Baseline = true)]
	public string Tape()
	{
		var reader = new Reader(_input);

		reader.Recognize();

		return reader.Construct();
	}

	/// <summary>The tape again, built on a stack rather than into a table as long as it.</summary>
	[Benchmark]
	public string TapeOnAStack()
	{
		var reader = new Reader(_input);

		reader.Recognize();

		return reader.ConstructOnAStack();
	}

	[Benchmark]
	public string Closures()
	{
		var reading = new Closures(_input);

		reading.Recognize();

		return reading.Construct();
	}

	[Benchmark]
	public string Mixed()
	{
		var reading = new Mixed(_input);

		reading.Recognize();

		return reading.Construct();
	}

	[Benchmark]
	public string Arenas()
	{
		var reading = new Arenas(_input);

		reading.Recognize();

		return reading.Construct();
	}

	[Benchmark]
	public string Boxed()
	{
		var reading = new Boxed(_input);

		reading.Recognize();

		return reading.Construct();
	}

	[Benchmark]
	public string Classes()
	{
		var reading = new Classes(_input);

		reading.Recognize();

		return reading.Construct();
	}
}
