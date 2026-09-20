using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DotGram.Web;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Every public reader of the package, refused after a growing run of several different fillers.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="WebRefusalScalingTests"/> holds four inputs, and every one of them was found the
/// same way: by suspecting a rule and writing an input for it. That is a closed loop — a fix
/// confirmed by the input that found the defect proves that input is no longer slow, not that the
/// shape is gone. Twice in two days the shape had a second way in, and both times the slow way
/// was the one that looked less likely: a route rather than a phrase, spaces rather than commas.
/// Choosing an input by eye was nought for two.
/// </para>
/// <para>
/// So this one chooses nothing. It walks every public reader and every filler, and asks only that
/// a refusal after a run of them is not exponential in the run. It will catch a rule nobody
/// suspected, which is what the four hand-written cases cannot do — and they in turn hold the
/// exact inputs whose numbers are recorded, which this cannot. Two instruments of different kinds
/// over one class.
/// </para>
/// <para>
/// The budget is generous and the lengths are short, because what has to be told apart is a
/// constant from a doubling: at these lengths the known defects cost between 189 ms and 107
/// seconds, and everything healthy costs microseconds.
/// </para>
/// </remarks>
[Collection(nameof(Alone))]
public sealed class WebRefusalSweepTests
{
	static readonly TimeSpan Budget = TimeSpan.FromMilliseconds(1_500);

	/// <summary>Runs of things a grammar repeats: letters, folding, comments, separators, digits.</summary>
	/// <remarks>
	/// `word ` is here because it found what none of the others did: a word may take folding on
	/// both sides, so the gap between two words belongs to either, and a phrase of n words has
	/// 2^n readings. A filler that is a unit AND a separator is the shape that finds it.
	/// </remarks>
	static readonly string[] Fillers = ["a", " ", "(a)", ",", "1", "a.", "%41", "word "];

	[Theory]
	[MemberData(nameof(Readers))]
	public async Task A_refusal_after_a_run_is_not_exponential_in_it(string reader, string filler)
	{
		var refuse = Refusals[reader];

		// Twenty-eight of the filler: where the shape is present this is already seconds, and
		// where it is not it is microseconds.
		var read  = Task.Run(() => refuse(Repeat(filler, 28)), TestContext.Current.CancellationToken);
		var spent = Task.Delay(Budget, TestContext.Current.CancellationToken);

		Assert.True(
			await Task.WhenAny(read, spent) == read,
			$"{reader}, filled with {(filler == " " ? "spaces" : "'" + filler + "'")}: a refusal after " +
			$"twenty-eight of them had not finished in {Budget.TotalSeconds:F1} s. That is `(A+)*` — a " +
			$"repeated rule whose body repeats, so the run can be cut many ways and a refusal tries " +
			$"them all. Seal the inner run: `{{ X+ }}`.");
	}

	static string Repeat(string filler, int times) => string.Concat(System.Linq.Enumerable.Repeat(filler, times));

	/// <summary>A refusal a reader is fed, with the run put where that reader accepts one.</summary>
	static readonly Dictionary<string, Func<string, bool>> Refusals = new()
	{
		["UriTemplate"]    = run => UriTemplate       .TryParse(run + "{unclosed", out _),
		["MediaType"]      = run => MediaType         .TryParse("text/" + run + "\u0001", out _),
		["MediaRange"]     = run => MediaRange        .TryParseAccept("text/" + run + "\u0001", out _),
		["UriReference"]   = run => UriReference      .TryParse("http://" + run + " x", out _),
		["AddrSpec"]       = run => AddrSpec          .TryParse(run + "@ex ample.com", out _),
		["AddressList"]    = run => EmailAddress      .TryParseList(run + "x@ex ample.com", out _),
		["AddressAngle"]   = run => EmailAddress      .TryParseList(run + "<", out _),
		["AddressRoute"]   = run => EmailAddress      .TryParseList("<" + run + "@a:b@c.d", out _),
		["LanguageTag"]    = run => LanguageTag       .TryParse(run + "-\u0001", out _),
		["JsonValue"]      = run => JsonValue         .TryParse("[\"" + run + "\",", out _),
		["SetCookie"]      = run => SetCookie         .TryParse("a=" + run + "; Path=/\u0001", out _),
		["WebLink"]        = run => WebLink           .TryParseField("<" + run + ">; rel=\u0001", out _),
		["Forwarded"]      = run => ForwardedElement  .TryParseField("for=" + run + ";by=\u0001", out _),
		["ContentDisp"]    = run => ContentDisposition.TryParse("attachment; filename=" + run + "\u0001", out _),
		["StructuredList"] = run => StructuredField   .TryParseList(run + ";q=\u0001", out _),
		["JsonPointer"]    = run => JsonPointer       .TryParse("/" + run + "~x", out _),
		["Timestamp"]      = run => Timestamp         .TryParse(run + "-12-19T16:39:57Z", out _),
	};

	public static TheoryData<string, string> Readers
	{
		get
		{
			var data = new TheoryData<string, string>();

			foreach (var reader in Refusals.Keys)
				foreach (var filler in Fillers)
					data.Add(reader, filler);

			return data;
		}
	}
}
