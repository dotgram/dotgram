using System;
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
	[Params(
		"http://example.com",
		"https://user@example.com:8080/a/b/c?q=1&r=2#top",
		"https://192.168.0.1/",
		"https://example.com/" + "segment/segment/segment/segment/segment/segment/segment/segment/",
		"https://exa mple.com/")]
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
}
