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

	/// <summary>
	/// The same seven parts kept, and not one string built for them.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Every captured rule declares its value as its own extent, so what a capture keeps is
	/// the two numbers the entry it completed into already holds (§4.1 case 3) rather than
	/// a slice of the input made into a string.
	/// </para>
	/// <para>
	/// <b>It does not isolate what the strings cost, although it was written to.</b> A rule
	/// with a value has a boundary of its own, so declaring seven of them <c>:
	/// @SourceSpan</c> buys seven rule frames <see cref="WithCaptures"/> does not pay for,
	/// and reads every captured value back through another arena entry. While the
	/// per-member capture walk dominated both, that was lost in the noise and this came out
	/// the cheaper of the two; once one walk replaced the several, it came out the dearer.
	/// Read the two against <see cref="NoCaptures"/> rather than against each other.
	/// </para>
	/// </remarks>
	[Gram("""
		Url        : @UrlSpans = scheme: Scheme & "://" & (user: UserInfo & '@')? & host: Host
		           & (':' & port: Port)? & path: Path
		           & ('?' & query: Rest)? & ('#' & fragment: Rest)?

		Scheme     : @SourceSpan = "https" | "http" | "ftp"

		UserInfo   : @SourceSpan = (Unreserved | SubDelim | PctEncoded | ':')+
		Host       : @SourceSpan = IPv4 | RegName
		Port       : @SourceSpan = Digit+

		IPv4       = Octet & '.' & Octet & '.' & Octet & '.' & Octet
		Octet      = Digit{1,3}
		RegName    = (Unreserved | SubDelim | PctEncoded)+

		Path       : @SourceSpan = ('/' & Segment)*
		Segment    = (Unreserved | SubDelim | PctEncoded | ':' | '@')*
		Rest       : @SourceSpan = (Unreserved | SubDelim | PctEncoded | ':' | '@' | '/' | '?')*
		PctEncoded = '%' & Hex & Hex

		Digit      = ['0'..'9']
		Hex        = [Digit | 'a'..'f' | 'A'..'F']
		Unreserved = [Digit | 'a'..'z' | 'A'..'Z' | '-' | '.' | '_' | '~']
		SubDelim   = ['!' | '$' | '&' | '\'' | '(' | ')' | '*' | '+' | ',' | ';' | '=']

		parse Url
		""")]
	public static partial class SpanCaptures
	{
		public sealed record UrlSpans(
			SourceSpan Scheme, SourceSpan? User, SourceSpan Host, SourceSpan? Port,
			SourceSpan Path, SourceSpan? Query, SourceSpan? Fragment);

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
		var spanCaptures = SpanCaptures.TryParseUrl(Input);

		if (withCaptures.IsSuccess != noCaptures.IsSuccess ||
			withCaptures.IsSuccess != spanCaptures.IsSuccess)
		{
			throw new InvalidOperationException(
				"The three grammars disagree about whether the input matches — the comparison " +
				"is not isolating materialization alone.");
		}

		// The same host, said two ways: one grammar keeps the text and the other keeps
		// where it was. A run where those disagree is measuring two different parses.
		var kept = Input.Substring(spanCaptures.Value!.Host.Start, spanCaptures.Value!.Host.Length);

		if (withCaptures.Value!.Host != kept)
			throw new InvalidOperationException(
				$"The captured host differs: '{withCaptures.Value!.Host}' against '{kept}'.");
	}

	[Benchmark(Baseline = true, Description = "Materialized (7 captures)")]
	public string? WithCapturesRun()
	{
		var match = WithCaptures.TryParseUrl(Input);

		return match.IsSuccess ? match.Value!.Host : null;
	}

	[Benchmark(Description = "Captured as spans, no strings built")]
	public int SpanCapturesRun()
	{
		var match = SpanCaptures.TryParseUrl(Input);

		return match.IsSuccess ? match.Value!.Host.Length : 0;
	}

	[Benchmark(Description = "SourceSpan only, nothing captured")]
	public int NoCapturesRun()
	{
		var match = NoCaptures.TryParseUrl(Input);

		return match.IsSuccess ? match.Value!.Length : 0;
	}
}
