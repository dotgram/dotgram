using System;
using System.Linq;

using DotGram.Web;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A refused list header allocates what its elements need, not their square.
/// </summary>
/// <remarks>
/// <para>
/// <c>ForwardedField</c>'s <c>ElementList</c> and <c>Link</c>'s <c>Field</c> both read a comma
/// list with no closing delimiter: what ends it is running out of list to read. A field of n good
/// elements followed by one bad one (an unclosed value, an unclosed target) leaves that bad tail
/// unconsumed, which only the outermost <c>eof</c> check — outside the list — notices. Before the
/// list was sealed, that failure replayed <c>ElementList</c> / <c>Field</c> from their own start,
/// giving one element back a retry and re-reading, and re-materialising, every element still
/// standing: n retries of O(n), values rebuilt every time. Measured before sealing: 1.17 GB at
/// 1,024 Forwarded elements, 414 MB at 1,024 links, both refused. Sealing the list (<c>{ }</c>)
/// commits it once it has read as many elements as it can, so a failure past it cannot ask for
/// fewer — the greedy reading is the only one a comma list ever meant, and nothing downstream
/// loses by that commit standing.
/// </para>
/// <para>
/// <b>The gate is bytes, not a bound at one size.</b> A quadratic allocator still passes a single
/// large-input ceiling if the ceiling is loose enough to admit ordinary noise, so this asks the
/// same question <see cref="BlockScalingTests"/> does, at four doubling sizes: bytes an element
/// stays flat, rather than reading a total against one number picked to fit a first measurement.
/// </para>
/// <para>
/// <b>Warmed at the top size, not the one being measured.</b> This read intermittently on the
/// GitHub Windows job (0e4af21b, 0d8fa0cd): 128 elements at 721.8 bytes each and 256 at 906.7, 1.26
/// times as many, where the fix leaves both flat. The cause is not the JIT: disabling tiered
/// compilation outright (<c>DOTNET_TieredCompilation=0</c>, and separately <c>DOTNET_TieredPGO=0</c>
/// and <c>DOTNET_TC_QuickJitForLoops=0</c>) reproduces byte-for-byte the same counts as an ordinary
/// run. What moves instead is the generated parser's own state — the value table, the choice
/// stores, the materialization arena — kept in <c>[ThreadStatic]</c> fields and grown by doubling
/// the first time a size asks more of them than a spare already holds, with a spare let go once
/// several parses in a row have not wanted the room it holds. Warming up only at the size about to
/// be measured, as this did, leaves it to chance which of the handful of calls at a size pays for a
/// pool's growth or its release; which size that lands on depends on what earlier work already grew
/// this thread's pools to, not on the count under test — which is why it read fine on Linux and only
/// sometimes on Windows, where the earlier work differs. Running the largest size once before
/// anything is measured settles every pool at the capacity the whole sweep needs, and keeping the
/// smallest of several runs at each size absorbs whatever a release still costs later on: measured
/// this way the four sizes read 600.6, 599.7, 599.3 and 599.2 bytes an element, identical across
/// repeated runs of the process.
/// </para>
/// </remarks>
[Collection(typeof(Alone))]
public sealed class WebListRefusalAllocationTests
{
	/// <summary>Doubling sizes; the note's own worst case (1,024) is the top rung.</summary>
	static readonly int[] Sizes = [128, 256, 512, 1024];

	/// <summary>How many timed runs a size is measured over; the smallest of them is kept.</summary>
	const int Runs = 5;

	[Fact]
	public void A_refused_Forwarded_field_still_refuses_at_every_size()
	{
		foreach (var size in Sizes)
			Assert.False(ForwardedElement.TryParseField(ForwardedRefusal(size), out _), $"{size} elements were read.");
	}

	[Fact]
	public void A_refused_Link_field_still_refuses_at_every_size()
	{
		foreach (var size in Sizes)
			Assert.False(WebLink.TryParseField(LinkRefusal(size), out _), $"{size} links were read.");
	}

	/// <summary>Four times the elements, and what a refusal allocates grows by about four — not sixteen.</summary>
	[Fact]
	public void A_refused_Forwarded_field_allocates_about_as_many_bytes_an_element_at_every_size()
	{
		AssertLinear(ForwardedRefusal, text => ForwardedElement.TryParseField(text, out _));
	}

	/// <summary>Four times the links, and what a refusal allocates grows by about four — not sixteen.</summary>
	[Fact]
	public void A_refused_Link_field_allocates_about_as_many_bytes_a_link_at_every_size()
	{
		AssertLinear(LinkRefusal, text => WebLink.TryParseField(text, out _));
	}

	/// <summary>Four times the characters in a range's subtype, and what a refusal allocates grows by about four — not sixteen.</summary>
	/// <remarks>
	/// The same square from another place. An Accept range is a token, a slash and a token, and then a
	/// guard over the three it has read. A field refused after a long subtype (<c>text/aaa…a;q=</c>) gave
	/// the subtype back one character at a time, and every shorter reading ran the guard again on a
	/// fresh copy of what was left of it: 4.3 GB and 239 ms at 65,536 characters, four times as much at
	/// every doubling. A token reads to its end — nothing that can follow one begins with a token
	/// character — so the rule commits it, and the same refusal allocates 262 KB.
	/// </remarks>
	[Fact]
	public void A_refused_Accept_field_allocates_about_as_many_bytes_a_subtype_character_at_every_size()
	{
		AssertLinear(AcceptRefusal, text => MediaRange.TryParseAccept(text, out _));
	}

	[Fact]
	public void A_refused_Accept_field_still_refuses_at_every_size()
	{
		foreach (var size in Sizes)
			Assert.False(MediaRange.TryParseAccept(AcceptRefusal(size), out _), $"a subtype of {size} characters was read.");
	}

	/// <summary>A long token in a link parameter, refused, allocates a few bytes a character — it did 192 — and takes no more than a linear time.</summary>
	/// <remarks>
	/// A token reads to its end in every Web grammar: nothing that can follow one begins with a token
	/// character, so a refusal after a long one has no shorter reading to try. A link parameter's name and
	/// value gave the token back a character at a time, with the choice state of the longer reading each
	/// try left behind: 192 bytes a character at every size, and 181 ms at 16,384 characters, which
	/// is more than four times what 4,096 took. The bytes were flat, so the ceiling is what catches it.
	/// </remarks>
	[Fact]
	public void A_refused_Link_parameter_allocates_a_few_bytes_a_token_character_at_every_size()
	{
		AssertLinear(LinkNameRefusal, text => WebLink.TryParseField(text, out _), ceiling: 16);
		AssertLinear(LinkValueRefusal, text => WebLink.TryParseField(text, out _), ceiling: 16);
	}

	[Fact]
	public void A_refused_Forwarded_token_allocates_about_as_many_bytes_a_character_at_every_size()
	{
		AssertLinear(ForwardedTokenRefusal, text => ForwardedElement.TryParseField(text, out _), ceiling: 16);
	}

	[Fact]
	public void A_refused_Content_Disposition_token_allocates_about_as_many_bytes_a_character_at_every_size()
	{
		AssertLinear(DispositionTokenRefusal, text => ContentDisposition.TryParse(text, out _), ceiling: 16);
		AssertLinear(DispositionNameRefusal, text => ContentDisposition.TryParse(text, out _), ceiling: 16);
	}

	[Fact]
	public void A_refused_Cookie_name_allocates_about_as_many_bytes_a_character_at_every_size()
	{
		AssertLinear(CookieNameRefusal, text => CookiePair.TryParseField(text, out _), ceiling: 16);
	}

	[Fact]
	public void A_refused_token_still_refuses_at_every_size()
	{
		foreach (var size in Sizes)
		{
			Assert.False(WebLink.TryParseField(LinkNameRefusal(size), out _), $"a link parameter name of {size} characters was read.");
			Assert.False(WebLink.TryParseField(LinkValueRefusal(size), out _), $"a link parameter value of {size} characters was read.");
			Assert.False(ForwardedElement.TryParseField(ForwardedTokenRefusal(size), out _), $"a forwarded value of {size} characters was read.");
			Assert.False(ContentDisposition.TryParse(DispositionTokenRefusal(size), out _), $"a disposition value of {size} characters was read.");
			Assert.False(ContentDisposition.TryParse(DispositionNameRefusal(size), out _), $"a disposition parameter name of {size} characters was read.");
			Assert.False(CookiePair.TryParseField(CookieNameRefusal(size), out _), $"a cookie name of {size} characters was read.");
		}
	}

	/// <summary>The same tokens, read to the end, are accepted and keep their values.</summary>
	[Fact]
	public void A_long_token_is_still_read_whole()
	{
		var token = new string('a', 1024);

		Assert.True(WebLink.TryParseField("<https://x>; " + token + "=" + token, out var links));
		Assert.Equal(token, links[0].Parameters[0].Name);
		Assert.Equal(token, links[0].Parameters[0].Value);

		Assert.True(ForwardedElement.TryParseField(token + "=" + token, out var elements));
		Assert.Equal(token, elements[0].Pairs[0].Name);

		Assert.True(ContentDisposition.TryParse(token + "; " + token + "=" + token, out var disposition));
		Assert.Equal(token, disposition.Type);

		Assert.True(CookiePair.TryParseField(token + "=" + token, out var pairs));
		Assert.Equal(token, pairs[0].Name);
	}

	/// <summary>
	/// Asserts bytes an element stay within 15% of one another at every doubling in <see cref="Sizes"/>. A
	/// quadratic allocator doubles bytes an element at every doubling in the count; this catches that at
	/// each of three steps rather than only between the first and the last.
	/// </summary>
	static void AssertLinear(Func<int, string> textFor, Func<string, bool> parse, double ceiling = double.PositiveInfinity)
	{
		var texts = Sizes.Select(textFor).ToArray();

		// The largest size, run once and discarded, before any size below it is measured: whatever
		// growing the parser's thread-static pools to fit it costs is paid here, not by whichever
		// size's measured run happens to ask a pool for more room next.
		parse(texts[^1]);

		var perElement = new double[Sizes.Length];

		for (var i = 0; i < Sizes.Length; i++)
			perElement[i] = Allocated(texts[i], parse) / (double)Sizes[i];

		Assert.True(
			perElement[^1] <= ceiling,
			$"At {Sizes[^1]} characters a refusal allocated {perElement[^1]:F1} bytes each, where more than {ceiling:F0} means a token was given back a character at a time.");

		for (var i = 1; i < Sizes.Length; i++)
			Assert.True(
				perElement[i] <= perElement[i - 1] * 1.15,
				$"At {Sizes[i - 1]} elements a refusal allocated {perElement[i - 1]:F1} bytes each, and at " +
				$"{Sizes[i]} it allocated {perElement[i]:F1} bytes each — {perElement[i] / perElement[i - 1]:F2} " +
				"times as many, where a list that does not re-read itself would stay flat.");
	}

	/// <summary>
	/// The fewest bytes this thread allocates running <paramref name="parse"/> over <paramref name="text"/>,
	/// across <see cref="Runs"/> runs. The fewest and not an average or a last: a pool's one-time growth or
	/// release can still land on any one of them, and the fewest is the run neither cost reached.
	/// </summary>
	static long Allocated(string text, Func<string, bool> parse)
	{
		var fewest = long.MaxValue;

		for (var run = 0; run < Runs; run++)
		{
			var before = GC.GetAllocatedBytesForCurrentThread();

			parse(text);

			fewest = Math.Min(fewest, GC.GetAllocatedBytesForCurrentThread() - before);
		}

		return fewest;
	}

	/// <summary><paramref name="elements"/> valid Forwarded elements, then one with no value after <c>for=</c>.</summary>
	static string ForwardedRefusal(int elements)
	{
		return string.Join(", ", Enumerable.Range(0, elements).Select(GoodElement)) + ", for=";
	}

	static string GoodElement(int index)
	{
		return "for=192.0." + (index / 250 % 250) + "." + (index % 250 + 1);
	}

	/// <summary>A range whose subtype is <paramref name="characters"/> long, then a weight with no value.</summary>
	static string AcceptRefusal(int characters)
	{
		return "text/" + new string('a', characters) + ";q=";
	}

	/// <summary>A link whose parameter name is <paramref name="characters"/> long, then a control character.</summary>
	static string LinkNameRefusal(int characters)
	{
		return "<https://x>; " + new string('a', characters) + "\u0001";
	}

	/// <summary>A link whose parameter value is <paramref name="characters"/> long, then a control character.</summary>
	static string LinkValueRefusal(int characters)
	{
		return "<https://x>; rel=" + new string('a', characters) + "\u0001";
	}

	static string ForwardedTokenRefusal(int characters)
	{
		return "for=" + new string('a', characters) + "\u0001";
	}

	static string DispositionTokenRefusal(int characters)
	{
		return "attachment; filename=" + new string('a', characters) + "\u0001";
	}

	static string DispositionNameRefusal(int characters)
	{
		return "attachment; " + new string('a', characters) + "=";
	}

	static string CookieNameRefusal(int characters)
	{
		return new string('a', characters) + "=\u0001";
	}

	/// <summary><paramref name="links"/> valid link-values, then one whose target is never closed.</summary>
	static string LinkRefusal(int links)
	{
		return string.Join(", ", Enumerable.Range(0, links).Select(GoodLink)) + ", <unclosed";
	}

	static string GoodLink(int index)
	{
		return "<https://example.com/" + index + ">; rel=next";
	}
}
