using System;

using BenchmarkDotNet.Attributes;

using DotGram;

namespace DotGram.Benchmarks;

/// <summary>
/// What materializing captures costs, isolated from recognition.
/// </summary>
/// <remarks>
/// Same grammar as <see cref="Urls"/>, flattened to one rule and the same seven captures
/// — the only difference from <see cref="NoCaptures"/> is whether anything is captured.
/// <see cref="NoCaptures"/> publishes <c>Url</c> as <c>@SourceSpan</c> and drops every
/// capture name, so recognition runs through the identical automaton shape and writes the
/// identical arena entries, but <c>Accept:</c> has nothing to walk. The difference between
/// the two is materialization's own cost on this input, not a difference in what was
/// recognized.
/// </remarks>
[MemoryDiagnoser]
public partial class MaterializationCost
{
	[Gram("""
		Url        : @UrlParts = scheme: Scheme & "://" & (user: UserInfo & '@')? & host: Host
		           & (':' & port: Digit+)? & path: Path
		           & ('?' & query: Rest)? & ('#' & fragment: Rest)?

		Scheme     = "https" | "http" | "ftp"

		UserInfo   = (Unreserved | SubDelim | PctEncoded | ':')+
		Host       = IPv4 | RegName

		IPv4       = Octet & '.' & Octet & '.' & Octet & '.' & Octet
		Octet      = Digit{1,3}
		RegName    = (Unreserved | SubDelim | PctEncoded)+

		Path       = ('/' & Segment)*
		Segment    = (Unreserved | SubDelim | PctEncoded | ':' | '@')*
		Rest       = (Unreserved | SubDelim | PctEncoded | ':' | '@' | '/' | '?')*
		PctEncoded = '%' & Hex & Hex

		Digit      = ['0'..'9']
		Hex        = [Digit | 'a'..'f' | 'A'..'F']
		Unreserved = [Digit | 'a'..'z' | 'A'..'Z' | '-' | '.' | '_' | '~']
		SubDelim   = ['!' | '$' | '&' | '\'' | '(' | ')' | '*' | '+' | ',' | ';' | '=']

		parse Url
		""")]
	public static partial class WithCaptures
	{
		public sealed record UrlParts(
			string Scheme, string? User, string Host, string? Port, string Path, string? Query,
			string? Fragment);

		[ThreadStatic]
		static Parser? _parser;

		static partial void RentParser(ref Parser parser)
		{
			parser  = _parser!;
			_parser = null;
		}

		static partial void ReturnParser(Parser parser) => _parser = parser;
	}

	[Gram("""
		Url        : @SourceSpan = Scheme & "://" & (UserInfo & '@')? & Host
		           & (':' & Digit+)? & Path
		           & ('?' & Rest)? & ('#' & Rest)?

		Scheme     = "https" | "http" | "ftp"

		UserInfo   = (Unreserved | SubDelim | PctEncoded | ':')+
		Host       = IPv4 | RegName

		IPv4       = Octet & '.' & Octet & '.' & Octet & '.' & Octet
		Octet      = Digit{1,3}
		RegName    = (Unreserved | SubDelim | PctEncoded)+

		Path       = ('/' & Segment)*
		Segment    = (Unreserved | SubDelim | PctEncoded | ':' | '@')*
		Rest       = (Unreserved | SubDelim | PctEncoded | ':' | '@' | '/' | '?')*
		PctEncoded = '%' & Hex & Hex

		Digit      = ['0'..'9']
		Hex        = [Digit | 'a'..'f' | 'A'..'F']
		Unreserved = [Digit | 'a'..'z' | 'A'..'Z' | '-' | '.' | '_' | '~']
		SubDelim   = ['!' | '$' | '&' | '\'' | '(' | ')' | '*' | '+' | ',' | ';' | '=']

		parse Url
		""")]
	public static partial class NoCaptures
	{
		[ThreadStatic]
		static Parser? _parser;

		static partial void RentParser(ref Parser parser)
		{
			parser  = _parser!;
			_parser = null;
		}

		static partial void ReturnParser(Parser parser) => _parser = parser;
	}

	/// <summary>The heaviest-capture input from <see cref="UrlBenchmarks"/> — every part present.</summary>
	const string Input = "https://user@example.com:8080/a/b/c?q=1&r=2#top";

	[GlobalSetup]
	public void CheckTheyAgree()
	{
		var withCaptures = WithCaptures.TryParseUrl(Input);
		var noCaptures   = NoCaptures.TryParseUrl(Input);

		if (withCaptures.IsSuccess != noCaptures.IsSuccess)
			throw new InvalidOperationException(
				"The two grammars disagree about whether the input matches — the comparison " +
				"is not isolating materialization alone.");
	}

	[Benchmark(Baseline = true, Description = "Materialized (7 captures)")]
	public string? WithCapturesRun()
	{
		var match = WithCaptures.TryParseUrl(Input);

		return match.IsSuccess ? match.Value!.Host : null;
	}

	[Benchmark(Description = "SourceSpan only, nothing captured")]
	public int NoCapturesRun()
	{
		var match = NoCaptures.TryParseUrl(Input);

		return match.IsSuccess ? match.Value!.Length : 0;
	}
}
