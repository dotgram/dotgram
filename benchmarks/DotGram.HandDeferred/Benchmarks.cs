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

	/// <summary>
	/// The same pairs, but nested to a given depth: a run at every level, and the last
	/// element of each run a parenthesised run one level down.
	/// </summary>
	/// <remarks>
	/// <see cref="Of(int)"/> is nearly flat — one element in sixteen parenthesised, and
	/// none at all below sixteen pairs, so the two smallest sizes in every other table
	/// have no recursion in them whatever. This is the other axis: the same amount of
	/// input, folded into itself instead of laid out.
	/// </remarks>
	public static string Of(int pairs, int depth)
	{
		var written = new StringBuilder();

		Emit(written, pairs, depth);

		return written.ToString();

		static void Emit(StringBuilder written, int pairs, int depth)
		{
			var here = depth <= 0 ? pairs : Math.Max(1, pairs / (depth + 1));

			if (here >= pairs)
			{
				here  = pairs;
				depth = 0;
			}

			for (var i = 0; i < here; i++)
			{
				if (i > 0)
					written.Append(" + ");

				written.Append("ab = ").Append(i % 10);
			}

			if (depth <= 0)
				return;

			written.Append(" + (");

			Emit(written, pairs - here, depth - 1);

			written.Append(')');
		}
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
/// Mixed               5       34.8 ns     0.47    0.0041         -         -       208 B
/// Mix2                5       35.6 ns     0.48    0.0062         -         -       312 B
/// Pooled              5       43.1 ns     0.58    0.0036         -         -       184 B
/// Boxed               5       62.1 ns     0.84    0.0116         -         -       584 B
/// Tape                5       73.8 ns     1.00    0.0124         -         -       624 B
/// Closures            5      157.5 ns     2.13    0.0393    0.0002         -     1,976 B
///
/// Pooled          1,000    6,438.6 ns     0.22    0.0763         -         -     3,967 B
/// Mix2            1,000    9,107.5 ns     0.31    1.0681    0.1678         -    53,792 B
/// Mixed           1,000   10,530.3 ns     0.36    1.0529    0.1068         -    53,000 B
/// Arenas          1,000   11,885.6 ns     0.40    1.7242    0.2441         -    86,808 B
/// Classes         1,000   13,463.5 ns     0.46    2.2888    0.7172         -   115,144 B
/// Tape            1,000   29,380.3 ns     1.00   31.2195   31.2195   31.2195   196,666 B
/// Closures        1,000   32,952.7 ns     1.12    7.9346    4.4556         -   399,656 B
///
/// Pooled         10,000   68,104.8 ns     0.35    0.7324    0.1221         -    35,090 B
/// Mix2           10,000   81,864.8 ns     0.42   10.6201    7.4463         -   538,416 B
/// Classes        10,000  149,857.9 ns     0.77   22.9492   17.5781         - 1,151,848 B
/// Boxed          10,000  171,199.0 ns     0.88   22.9492   17.5781         - 1,151,848 B
/// Tape           10,000  195,373.2 ns     1.00  392.3340  392.3340  392.3340 1,573,109 B
/// Mixed          10,000  215,563.9 ns     1.10  199.9512  199.9512  199.9512   823,931 B
/// Arenas         10,000  240,123.7 ns     1.23  249.7559  249.7559  249.7559 1,344,892 B
/// Closures       10,000  413,233.1 ns     2.12   79.5898   73.7305         - 3,999,176 B
/// </code>
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
/// <b>The order changes with the size, and the large object heap is what changes it.</b>
/// Up to a hundred pairs the readings that keep their derivation in arrays win, exactly as
/// the byte counts say they should. At a thousand the tape has already doubled its one
/// <c>Record[]</c> past eighty-five kilobytes and takes thirty-one gen2 collections per
/// thousand parses, and it falls behind everything but the closures. At ten thousand
/// <c>Mixed</c> and <c>Arenas</c> are there too, and the readings that allocate an object
/// per node and no array at all — <c>Classes</c> and <c>Boxed</c> — go past them.
/// </para>
/// <para>
/// Which is not a verdict against arrays but against growing a fresh one per parse.
/// <see cref="Threshold"/> is the experiment that says so, and <see cref="Pooled"/> is the
/// control: the same array, rented and returned, and the cliff is gone and the curve is a
/// straight line. It leads at every size but the smallest, on a thirtieth of the tape's
/// allocation. The engine's <c>Ways.Rent()</c> is that, and this is the measurement saying
/// it is not an optimization but what keeps the representation standing.
/// </para>
/// <para>
/// <see cref="Mix2"/> is the other answer, and it needs nothing to own: no array, so no
/// array to rent — <c>Pair</c> is a class and the run of steps is threaded through the
/// elements. Half the cost of <c>Mixed</c> at ten thousand pairs and two thirds of its
/// allocation, with no lifetime to manage and no pool to be right about.
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class Recognizing
{
	[Params(5, 10, 100, 1_000)]
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

	[Benchmark] public bool Mixed() => new Mixed(_input).Recognize();

	[Benchmark] public bool Mix2() => new Mix2(_input).Recognize();

	[Benchmark]
	public bool Pooled()
	{
		var reading = new Pooled(_input);
		var read    = reading.Recognize();

		reading.Return();

		return read;
	}

	[Benchmark]
	public bool Arenas()
	{
		var reading = new Arenas(_input);
		var read    = reading.Recognize();

		reading.Return();

		return read;
	}

	/// <summary>
	/// The same reading with the same plumbing and the pool never given anything back, so
	/// every parse allocates its arenas afresh.
	/// </summary>
	/// <remarks>
	/// The control for <see cref="Arenas"/>, the way <see cref="Pooled"/> is the control
	/// for <see cref="Mixed"/> — and it earns its place, because pooling the arenas made
	/// them slower and this is what says the plumbing is not why.
	/// </remarks>
	[Benchmark]
	public bool ArenasFresh() => new Arenas(_input).Recognize();

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
	[Params(5, 10, 100, 1_000)]
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
	public string Mixed()
	{
		var reading = new Mixed(_input);

		reading.Recognize();

		return reading.Construct();
	}

	[Benchmark]
	public string Mix2()
	{
		var reading = new Mix2(_input);

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

/// <summary>
/// The one question the other two leave open: was <see cref="Mixed"/>'s slowdown at ten
/// thousand pairs the large object heap, or something else?
/// </summary>
/// <remarks>
/// <para>
/// A <c>Pair</c> is twenty-four bytes and the array of steps doubles, so its capacities
/// are 4, 8, … 2,048 at forty-nine kilobytes and 4,096 at ninety-eight — over the
/// eighty-five kilobyte line. The top-level fold has one step per element, so the first
/// large object is allocated somewhere just past two thousand pairs and not before. Four
/// sizes astride that is the experiment: if the cliff is at the same place as the
/// threshold, the reading was right.
/// </para>
/// <para>
/// <see cref="Pooled"/> is the control, differing from <see cref="Mixed"/> in nothing but
/// renting the array instead of making one, and <see cref="Mix2"/> is the other answer —
/// no array at all, the run threaded through the elements. Recognition only: this is about
/// where a representation is written, not about the author's concatenation.
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class Threshold
{
	[Params(1_000, 2_000, 3_000, 4_000)]
	public int Pairs;

	string _input = "";

	[GlobalSetup]
	public void Setup() => _input = Inputs.Of(Pairs);

	[Benchmark(Baseline = true)]
	public bool Mixed() => new Mixed(_input).Recognize();

	[Benchmark]
	public bool Pooled()
	{
		var reading = new Pooled(_input);
		var read    = reading.Recognize();

		reading.Return();

		return read;
	}

	[Benchmark] public bool Mix2() => new Mix2(_input).Recognize();

	[Benchmark]
	public bool Tape()
	{
		var reader = new Reader(_input);

		return reader.Recognize();
	}
}

/// <summary>
/// The axis the other tables do not have: how deep the input nests, at the same length.
/// </summary>
/// <remarks>
/// <para>
/// Everywhere else the input is a long run with one element in sixteen parenthesised,
/// which below sixteen pairs is no recursion at all. That flatters whatever handles a run
/// well and says nothing about the part of the grammar the whole project turned on — the
/// parenthesis, which is the recursion §4.3 cannot fold away.
/// </para>
/// <para>
/// A thousand pairs in every case, folded one, five and ten levels deep. Each level
/// carries a <c>Sum</c> of its own, so the depth is how many objects the readings that
/// keep a <c>Sum</c> have to make — and, at build time, how deep the walk goes, which is
/// why this measures the whole parse and not reading alone.
/// </para>
/// <para>
/// Reading alone, measured before this class was turned round, went like this — the
/// tape's own cost being flat in depth, everything that makes an object per group getting
/// worse with it, and the arenas holding at a hundred and twenty bytes throughout:
/// </para>
/// <code>
///              Depth         Mean    Ratio   Allocated
/// Pooled           1       5.8 us     0.50       480 B
/// Mix2             1       7.8 us     0.67    48,160 B
/// Arenas           1      10.7 us     0.93       120 B
/// Tape             1      11.6 us     1.00    98,328 B
/// Boxed            1      14.8 us     1.28   112,072 B
///
/// Pooled          10       7.9 us     0.83     2,848 B
/// Mix2            10       7.8 us     0.83    48,952 B
/// Arenas          10       8.9 us     0.94       120 B
/// Tape            10       9.5 us     1.00    98,328 B
/// Boxed           10      15.0 us     1.58   112,504 B
/// </code>
/// <para>
/// Whole, it comes out compressed the way <see cref="Parsing"/> does, and for the same
/// reason — 0.77 to 1.04, against 0.50 to 1.58 for reading alone. What depth moves most is
/// not any representation but the author's own quadratic: one level means two runs of five
/// hundred and ten levels means eleven runs of ninety, so the concatenation has a fifth of
/// the work to do and the whole parse falls from a hundred and forty-nine microseconds to
/// sixty-three. <c>Boxed</c> and <c>Classes</c>, half again worse than the tape at reading
/// ten levels deep, come out level with it once building is included.
/// </para>
/// <para>
/// <c>Pooled16</c> asks for sixteen where a run starts instead of four, on the thought that
/// a group is a run of its own and an input full of groups pays the first doublings once
/// per group. It does not show: 145.6 against 151.6 microseconds at one level, 70.2 against
/// 68.4 at five, 50.4 against 48.4 at ten, with error bars of two to fifteen. The
/// arithmetic says why — eleven groups of ninety pairs skip two doublings each, so
/// twenty-two borrows in a parse of fifty thousand nanoseconds. Kept as a measured
/// negative rather than removed.
/// </para>
/// </remarks>
[MemoryDiagnoser]
[SimpleJob(RunStrategy.Throughput, launchCount: 1, warmupCount: 3, iterationCount: 5)]
public class Nesting
{
	[Params(1, 5, 10)]
	public int Depth;

	string _input = "";

	[GlobalSetup]
	public void Setup() => _input = Inputs.Of(1_000, Depth);

	[Benchmark(Baseline = true)]
	public string Tape()
	{
		var reader = new Reader(_input);

		reader.Recognize();

		return reader.Construct();
	}

	[Benchmark]
	public string Mixed()
	{
		var reading = new Mixed(_input);

		reading.Recognize();

		return reading.Construct();
	}

	[Benchmark]
	public string Mix2()
	{
		var reading = new Mix2(_input);

		reading.Recognize();

		return reading.Construct();
	}

	[Benchmark]
	public string Pooled()
	{
		var reading = new Pooled(_input);

		reading.Recognize();

		return reading.Construct();
	}

	/// <summary>The same, asking for sixteen where a run starts rather than four.</summary>
	[Benchmark]
	public string Pooled16()
	{
		var reading = new Pooled(_input, 16);

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
