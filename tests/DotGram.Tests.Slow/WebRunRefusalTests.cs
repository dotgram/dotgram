using System;
using System.Diagnostics;
using System.Threading.Tasks;

using DotGram.Web;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A header field refused after a long run costs what the run is long, not its square or its cube.
/// </summary>
/// <remarks>
/// <para>
/// HTTP's optional whitespace sits on both sides of a reading that may take nothing: an empty
/// parameter in a media type (<c>;   ;</c>), a Link parameter with no value, an empty Forwarded
/// element at the head of the field. While a run of OWS could hand characters back, a refused field
/// tried every way of sharing a run of spaces between the OWS before such a reading and the OWS
/// after it, and each try read the field again from its start. That is cubic in the run: 1,600
/// spaces cost 2.6 s in a Content-Type, 2.9 s in a Forwarded field and 0.7 s in a Link field, and a
/// header line of the 8 KB a server commonly allows would have cost minutes of a core per request.
/// The whitespace is interior in two of the three, so trimming the field does not help.
/// </para>
/// <para>
/// The refusal ladders hold the same shapes to a class, but only report a class that got worse.
/// This holds them to a budget at a header's length, which fails, and in a second and a half, the
/// day a run of OWS gives characters back again. The abandoned read goes on until the process ends,
/// as in <see cref="WebRefusalScalingTests"/>.
/// </para>
/// </remarks>
[Collection(typeof(Alone))]
public sealed class WebRunRefusalTests
{
	static readonly TimeSpan Budget = TimeSpan.FromMilliseconds(1_500);

	/// <summary>A header line of the length servers commonly allow.</summary>
	const int Header = 8_000;

	[Theory]
	[InlineData("a media type with spaces between two parameters")]
	[InlineData("an Accept field with spaces after a parameter")]
	[InlineData("a Forwarded field that begins with spaces")]
	[InlineData("a Link field with spaces after a parameter name")]
	public async Task A_refusal_after_a_long_run_is_linear_in_it(string shape)
	{
		var refuse = Reader(shape);

		// A short one first, to compile the path: quick even where the grammar is wrong.
		Assert.False(refuse(4));

		// One read at a header's length, abandoned if it outstays the budget. The timed reads come
		// after it, because on a cubic reader even a quarter of the length is seconds a read.
		var read  = Task.Run(() => refuse(Header), TestContext.Current.CancellationToken);
		var spent = Task.Delay(Budget, TestContext.Current.CancellationToken);

		Assert.True(
			await Task.WhenAny(read, spent) == read,
			$"{shape}: a refusal after {Header:N0} spaces had not finished in {Budget.TotalSeconds:F1} s. " +
			$"A run of OWS hands spaces back to the OWS on the other side of a reading that takes nothing; " +
			$"make it atomic: `{{ [' ' | '\\t']* }}`.");

		// Four times the spaces is four times the work when the reading is linear, and sixteen when
		// it is quadratic.
		var shorter = Best(refuse, Header / 4);
		var longer  = Best(refuse, Header);

		Assert.True(
			longer / Math.Max(shorter, 1) < 10,
			$"{shape}: four times the spaces took {longer / Math.Max(shorter, 1):F1} times as long " +
			$"({shorter:F0} µs against {longer:F0} µs).");
	}

	static Func<int, bool> Reader(string shape)
	{
		var cut = ((char)1).ToString();

		return shape[..4] switch
		{
			"a me" => length => MediaType.TryParse("text/html; a=1;" + new string(' ', length) + ";b=\"x\"" + cut, out _),
			"an A" => length => MediaRange.TryParseAccept("text/html;" + new string(' ', length) + ",q=0.5, text/x" + cut, out _),
			"a Fo" => length => ForwardedElement.TryParseField(new string(' ', length) + cut + "for=192.0.2.1;proto=https, for=x", out _),
			_      => length => WebLink.TryParseField("<https://x/1>; rel" + new string(' ', length) + "=\"next\"; a=b, <https://y>" + cut, out _),
		};
	}

	/// <summary>The fastest of several reads, in microseconds, after one to compile it.</summary>
	static double Best(Func<int, bool> refuse, int length)
	{
		Assert.False(refuse(length));

		var best = double.MaxValue;

		for (var run = 0; run < 11; run++)
		{
			var watch = Stopwatch.StartNew();

			refuse(length);

			best = Math.Min(best, watch.Elapsed.TotalMilliseconds * 1000);
		}

		return best;
	}
}
