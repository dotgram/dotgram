using System;

using DotGram;

namespace DotGram.Benchmarks;

// The URL grammar of examples/UrlExample.cs, copied rather than referenced.
//
// Copied on purpose. A benchmark that referenced the examples project would measure a
// parser generated with that project's settings, and the thing under test is what the
// generator produces here, in this configuration, for this compilation. It is also the
// honest reading of "no runtime assembly ships": a consumer takes the analyzer and gets
// their own parser, so a benchmark of somebody else's is a benchmark of the wrong thing.
//
// Nothing stops this copy drifting from the example's, and nothing needs to: what the
// benchmark asserts before it times anything is that this grammar and the regular
// expression beside it answer the same, which is the comparison it exists to make. If
// the example changes and this does not, the numbers are still numbers about this
// grammar against that pattern.

[Gram("""
	Url        = scheme: Scheme & "://" & authority: Authority & path: Path
	           & ('?' & query: Rest)? & ('#' & fragment: Rest)?

	Scheme     = "https" | "http" | "ftp"

	Authority  = (user: UserInfo & '@')? & host: Host & (':' & port: Digit+)?
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
public static partial class Urls
{
	// TryParseUrl and the types Url and Authority are generated here.

	[ThreadStatic]
	static Parser? _parser;

	static partial void RentParser(ref Parser parser)
	{
		parser  = _parser!;
		_parser = null;
	}

	static partial void ReturnParser(Parser parser) => _parser = parser;
}
