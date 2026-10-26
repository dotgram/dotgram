using System;
using System.Diagnostics;
using System.Text;
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
/// A Structured Field's key and token were the milder form of the same thing: a run that could
/// hand characters back where nothing after it could take one, so a refusal after a long key or
/// token gave it back a character at a time and read the field again each time. Quadratic, 25 ms
/// at 6,400 characters.
/// </para>
/// <para>
/// A run of anything a reading gives back one at a time is the same thing again, where each piece
/// it gives back makes the field be read from its start once more: the parameters of a media type
/// (<c>a/b;;;</c>), the members of a Structured Field dictionary, the inside of an unclosed
/// comment, quoted string or domain literal in an address, and the commas of an address list's
/// null members. Quadratic: 0.9 s explaining the refusal of a media type of eight thousand
/// <c>;</c>, and 1.3 s refusing an address list that was <c>(</c> and eight thousand characters.
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
	[InlineData("a Structured Field item that is a long token")]
	[InlineData("a Structured Field dictionary with a long key")]
	[InlineData("a media type with many empty parameters")]
	[InlineData("a media type with many empty parameters, explained")]
	[InlineData("a Structured Field dictionary with many members")]
	[InlineData("a Structured Field dictionary with many members, explained")]
	[InlineData("an address list with an unclosed comment")]
	[InlineData("a mailbox with an unclosed quoted string")]
	[InlineData("an addr-spec with an unclosed comment")]
	[InlineData("an addr-spec with an unclosed domain literal")]
	[InlineData("a mailbox list of null members")]
	[InlineData("a group of null members")]
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
			$"{shape}: a refusal after a run of {Header:N0} characters had not finished in {Budget.TotalSeconds:F1} s. " +
			$"A run that hands characters back where nothing after it can take one costs a read of the field " +
			$"for every character; make it atomic, as `{{ [' ' | '\\t']* }}` or `{{ slots: ParameterSlot* }}`.");

		// Eight times the run is eight times the work when the reading is linear, and sixty-four
		// times when it is quadratic. The shorter is read again after the longer: its first reads
		// can be in code the tiered compiler has not finished with, which would flatten the ratio.
		var shorter = Best(refuse, Header / 8);
		var longer  = Best(refuse, Header);

		shorter = Math.Min(shorter, Best(refuse, Header / 8));

		Assert.True(
			longer / Math.Max(shorter, 1) < 24,
			$"{shape}: eight times the run took {longer / Math.Max(shorter, 1):F1} times as long " +
			$"({shorter:F0} µs against {longer:F0} µs).");
	}

	static Func<int, bool> Reader(string shape)
	{
		var cut = ((char)1).ToString();

		return shape switch
		{
			"a media type with spaces between two parameters" => length => MediaType.TryParse("text/html; a=1;" + new string(' ', length) + ";b=\"x\"" + cut, out _),
			"an Accept field with spaces after a parameter"   => length => MediaRange.TryParseAccept("text/html;" + new string(' ', length) + ",q=0.5, text/x" + cut, out _),
			"a Forwarded field that begins with spaces"       => length => ForwardedElement.TryParseField(new string(' ', length) + cut + "for=192.0.2.1;proto=https, for=x", out _),
			"a Link field with spaces after a parameter name" => length => WebLink.TryParseField("<https://x/1>; rel" + new string(' ', length) + "=\"next\"; a=b, <https://y>" + cut, out _),
			"a Structured Field item that is a long token"    => length => StructuredField.TryParseItem(new string('a', length) + cut, out _),
			"a Structured Field dictionary with a long key"   => length => StructuredField.TryParseDictionary(new string('a', length) + cut, out _),

			"a media type with many empty parameters"                    => length => MediaType.TryParse("a/b;" + new string(';', length) + cut, out _),
			"a media type with many empty parameters, explained"         => length => Explained(() => MediaType.Parse("a/b;" + new string(';', length) + cut)),
			"a Structured Field dictionary with many members"            => length => StructuredField.TryParseDictionary(Copies("x,", length) + cut, out _),
			"a Structured Field dictionary with many members, explained" => length => Explained(() => StructuredField.ParseDictionary(Copies("x,", length) + cut)),
			"an address list with an unclosed comment"                   => length => EmailAddress.TryParseList("(" + new string('a', length) + cut, out _),
			"a mailbox with an unclosed quoted string"                   => length => EmailAddress.Mailbox.TryParseStrict("\"" + Copies("x,", length) + cut, out _),
			"an addr-spec with an unclosed comment"                      => length => AddrSpec.TryParse("(" + new string('a', length) + cut, out _),
			"an addr-spec with an unclosed domain literal"               => length => AddrSpec.TryParse("a@[" + new string('a', length) + cut, out _),
			"a mailbox list of null members"                             => length => EmailAddress.TryParseMailboxList(new string(',', length) + cut, out _),
			"a group of null members"                                    => length => EmailAddress.TryParseList("a:" + new string(',', length) + cut, out _),
			_ => throw new ArgumentOutOfRangeException(nameof(shape), shape, null),
		};
	}

	/// <summary>A text of the given length, the piece repeated.</summary>
	static string Copies(string piece, int length)
	{
		var text = new StringBuilder(length + piece.Length);

		while (text.Length < length)
			text.Append(piece);

		return text.ToString();
	}

	/// <summary>Whether a Parse accepted, where its refusal is a message that says where and why.</summary>
	static bool Explained(Action parse)
	{
		try
		{
			parse();

			return true;
		}
		catch (FormatException)
		{
			return false;
		}
	}

	/// <summary>The fastest of several reads, in microseconds, after one to compile it.</summary>
	static double Best(Func<int, bool> refuse, int length)
	{
		Assert.False(refuse(length));

		var best = double.MaxValue;

		for (var run = 0; run < 31; run++)
		{
			var watch = Stopwatch.StartNew();

			refuse(length);

			best = Math.Min(best, watch.Elapsed.TotalMilliseconds * 1000);
		}

		return best;
	}
}
