using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using BenchmarkDotNet.Attributes;

using DotGram;

namespace DotGram.Benchmarks;

/// <summary>
/// The URL grammar of <c>examples/UrlExample.cs</c> against the same language written as
/// a regular expression, interpreted and compiled.
/// </summary>
/// <remarks>
/// <para>
/// The comparison is only worth making if both sides answer the same question, so the
/// pattern is not a loose URL-shaped regex: it is this grammar transcribed, rule by rule,
/// with the same character classes and the same named groups. A run asserts that the two
/// agree on every input before any of it is timed — a benchmark of two things that do not
/// do the same work is a number about nothing.
/// </para>
/// <para>
/// Both sides are asked for the parts, not merely for a yes. Answering "is this a URL" is
/// a different and much cheaper question than "what are its scheme, host, port and path",
/// and the second is what a parser is for.
/// </para>
/// <para>
/// Asked twice, because "the parts" is two questions and the two engines answer them
/// differently. Reading <b>one</b> part is what the first pair does, and it is the
/// pattern's shape of question: <c>Group.Value</c> stores where a capture was and cuts the
/// string on access, so one group asked for is one string built. Reading <b>every</b> part
/// is the second pair, and it is this project's: a publication hands back a record with all
/// of them already in it, so one asked for is seven built either way.
/// </para>
/// <para>
/// Neither pair is the honest one on its own. The first flatters the pattern by asking for
/// the one thing it defers; the second flatters this by asking for everything it built
/// anyway. Together they say what each design costs where, which is what a comparison is
/// for.
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class UrlBenchmarks
{
	/// <summary>Transcribed from the grammar, rule for rule.</summary>
	/// <remarks>
	/// <c>RegexOptions.ExplicitCapture</c> so the unnamed groups the transcription needs
	/// for grouping do not each become a capture the engine has to record — without it the
	/// regex is doing bookkeeping the grammar is not, and the comparison drifts.
	/// </remarks>
	const string Pattern =
		@"^(?<scheme>https|http|ftp)://" +
		@"(?:(?<user>(?:[0-9a-zA-Z\-._~!$&'()*+,;=:]|%[0-9a-fA-F]{2})+)@)?" +
		@"(?<host>(?:[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3})" +
		@"|(?:[0-9a-zA-Z\-._~!$&'()*+,;=]|%[0-9a-fA-F]{2})+)" +
		@"(?::(?<port>[0-9]+))?" +
		@"(?<path>(?:/(?:[0-9a-zA-Z\-._~!$&'()*+,;=:@]|%[0-9a-fA-F]{2})*)*)" +
		@"(?:\?(?<query>(?:[0-9a-zA-Z\-._~!$&'()*+,;=:@/?]|%[0-9a-fA-F]{2})*))?" +
		@"(?:\#(?<fragment>(?:[0-9a-zA-Z\-._~!$&'()*+,;=:@/?]|%[0-9a-fA-F]{2})*))?$";

	static readonly Regex Interpreted =
		new(Pattern, RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant);

	static readonly Regex Compiled =
		new(Pattern, RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant | RegexOptions.Compiled);

	/// <summary>
	/// The shapes a URL parser meets, and one it fails on.
	/// </summary>
	/// <remarks>
	/// The failing one is not padding. A parser that is quick to say yes and slow to say
	/// no is a parser that is quick on the input nobody sends: refusal is where a
	/// backtracking engine does its worst work, and leaving it out of a benchmark is how
	/// that goes unnoticed.
	/// </remarks>
	/// <remarks>
	/// A source rather than constants in the attribute so that <see cref="Against"/> can
	/// measure the same five without a second copy of them to keep in step.
	/// </remarks>
	public static IEnumerable<string> Inputs =>
	[
		"http://example.com",
		"https://user@example.com:8080/a/b/c?q=1&r=2#top",
		"https://192.168.0.1/",
		"https://example.com/" + "segment/segment/segment/segment/segment/segment/segment/segment/",
		"https://exa mple.com/",
	];

	[ParamsSource(nameof(Inputs))]
	public string Input { get; set; } = "";

	[GlobalSetup]
	public void CheckTheyAgree()
	{
		var mine  = Urls.TryParseUrl(Input);
		var match = Compiled.Match(Input);

		if (mine.IsSuccess != match.Success)
			throw new InvalidOperationException(
				$"The grammar and the pattern disagree about whether '{Input}' is a URL: " +
				$"{mine.IsSuccess} against {match.Success}.");

		if (!mine.IsSuccess)
			return;

		Same("scheme", mine.Value!.Scheme,           match.Groups["scheme"].Value);
		Same("host",   mine.Value!.Authority.Host,   match.Groups["host"].Value);
		Same("path",   mine.Value!.Path,             match.Groups["path"].Value);

		// A part that never appeared is null on one side and an empty non-success group
		// on the other, which is the same answer said two ways.
		Same("user",   mine.Value!.Authority.User ?? "", match.Groups["user"].Value);
		Same("port",   mine.Value!.Authority.Port ?? "", match.Groups["port"].Value);
		Same("query",  mine.Value!.Query          ?? "", match.Groups["query"].Value);

		// And the every-part pair against each other, which is the same rule applied to
		// what those two actually return: a total over every part, so a benchmark of one
		// side reading six of them and the other seven is refused rather than timed.
		if (GrammarEveryPart() != RegexCompiledEveryPart())
			throw new InvalidOperationException(
				$"The grammar and the pattern disagree about the total length of the parts of " +
				$"'{Input}': {GrammarEveryPart()} against {RegexCompiledEveryPart()}.");

		void Same(string part, string ours, string theirs)
		{
			if (ours != theirs)
				throw new InvalidOperationException(
					$"The grammar and the pattern disagree about the {part} of '{Input}': " +
					$"'{ours}' against '{theirs}'.");
		}
	}

	[Benchmark(Baseline = true, Description = ".Gram")]
	public string? Grammar()
	{
		var match = Urls.TryParseUrl(Input);

		return match.IsSuccess ? match.Value!.Authority.Host : null;
	}

	[Benchmark(Description = "Regex")]
	public string? RegexInterpreted()
	{
		var match = Interpreted.Match(Input);

		return match.Success ? match.Groups["host"].Value : null;
	}

	[Benchmark(Description = "Regex, compiled")]
	public string? RegexCompiled()
	{
		var match = Compiled.Match(Input);

		return match.Success ? match.Groups["host"].Value : null;
	}

	// ── And the same three asked for every part ──────────────────────────────────
	//
	// Summed rather than collected: what is being measured is building each part, and a
	// list or a concatenation would put its own allocation in front of that. The lengths
	// are what force every string into existence without adding anything of their own.

	[Benchmark(Description = ".Gram, every part")]
	public int GrammarEveryPart()
	{
		var match = Urls.TryParseUrl(Input);

		if (!match.IsSuccess)
			return 0;

		var url = match.Value!;

		return url.Scheme.Length + url.Authority.Host.Length + url.Path.Length +
			(url.Authority.User?.Length  ?? 0) +
			(url.Authority.Port?.Length  ?? 0) +
			(url.Query?.Length           ?? 0) +
			(url.Fragment?.Length        ?? 0);
	}

	[Benchmark(Description = "Regex, every part")]
	public int RegexInterpretedEveryPart() => EveryPart(Interpreted.Match(Input));

	[Benchmark(Description = "Regex compiled, every part")]
	public int RegexCompiledEveryPart() => EveryPart(Compiled.Match(Input));

	static int EveryPart(Match match)
	{
		if (!match.Success)
			return 0;

		var groups = match.Groups;

		return groups["scheme"].Value.Length + groups["host"].Value.Length +
			groups["path"].Value.Length + groups["user"].Value.Length +
			groups["port"].Value.Length + groups["query"].Value.Length +
			groups["fragment"].Value.Length;
	}
}
