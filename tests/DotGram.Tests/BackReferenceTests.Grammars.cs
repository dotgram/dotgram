using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The grammars bindings were made for, each with what it must read and what it must refuse: every
/// example of docs/syntax.md §3.5.1 as it is printed there, and the rows around them that a matrix
/// of carriers, readers and publications has to agree on.
/// </summary>
/// <remarks>
/// One table, asked three ways: of <see cref="ReferenceInterpreter"/>, which is the meaning
/// written out as recursion (<c>BackReferenceTests.Semantics</c>); of the generated parsers on every
/// carrier with the reader and with the engine (<c>BackReferenceTests.Emitted</c>); and of the
/// documentation, every example of which must be one of these rows word for word.
/// </remarks>
public sealed partial class BackReferenceTests
{
	/// <summary>One grammar, the rule published from it, and the inputs it decides.</summary>
	/// <param name="Name">What the row is called in a test's name.</param>
	/// <param name="Rule">The rule published as <c>parse</c> (and <c>find</c>, where <paramref name="Found"/> is given).</param>
	/// <param name="Grammar">The grammar, without its publications.</param>
	/// <param name="Accepted">Whole inputs that read as <paramref name="Rule"/>.</param>
	/// <param name="Refused">Whole inputs that do not.</param>
	/// <param name="Text">An input for <c>find</c>; null where the row has none.</param>
	/// <param name="Found">Where <c>find</c> finds each occurrence in <paramref name="Text"/>.</param>
	public sealed record Row(
		string   Name,
		string   Rule,
		string   Grammar,
		string[] Accepted,
		string[] Refused,
		string?  Text  = null,
		int[]?   Found = null)
	{
		/// <summary>The grammar with its publications.</summary>
		public string Published()
		{
			return Grammar + $"\nparse {Rule}\n" + (Found is null ? "" : $"find {Rule}\n");
		}

		public override string ToString()
		{
			return Name;
		}
	}

	// ── The examples of docs/syntax.md §3.5.1, as printed ──────────────────────

	const string Quoted = """
		Quoted = ('\'' & $t: Word & '\'' | $t: Word) & " " & $t
		Word   = ['a'..'z']+
		""";

	const string LongBracket = """
		LongString = '[' & $eq: '='* & '[' & { (?!(']' & $eq & ']') & any)* } & ']' & $eq & ']'
		trivia     = { (Std.Spacing | "--" & LongString | Std.LineComment("--"))* }
		""";

	const string RawString = """
		RawString = { $q: '"'{3,} } & { (?!$q & any)* } & $q & ?!'"'
		""";

	const string RustRaw = """
		RustRaw = 'r' & $h: '#'* & '"' & { (?!('"' & $h) & any)* } & '"' & $h
		""";

	const string DollarQuoted = """
		DollarQuoted = '$' & $tag: (Tag?) & '$' & { (?!('$' & $tag & '$') & any)* } & '$' & $tag & '$'
		Tag          = [\p{L} | '_'] & [\p{L} | \p{Nd} | '_']*
		""";

	const string Heredoc = """
		Heredoc = "<<"  & $tag: Word & eol & { (?!($tag & EndLine) & Line)* } & $tag & ?=EndLine
		        | "<<-" & $tag: Word & eol & { (?!('\t'* & $tag & EndLine) & Line)* } & '\t'* & $tag & ?=EndLine
		Word    = ['A'..'Z' | 'a'..'z' | '_']+
		Line    = [^ '\r' | '\n']+ & EndLine | eol
		EndLine = eol | eof
		""";

	const string Fenced = """
		Fenced  = ' '{0,3} & $f: '`'{3,} & [^ '`' | '\r' | '\n']* & eol
		          & { (?!(' '{0,3} & $f & '`'* & ' '* & EndLine) & Line)* }
		          & (' '{0,3} & $f & '`'* & ' '* & EndLine | eof)
		Line    = [^ '\r' | '\n']+ & EndLine | eol
		EndLine = eol | eof
		""";

	const string Heading = """
		Heading = $t: Title & eol & '='{#t,} & eol
		Title   = [^ '\r' | '\n']+
		""";

	/// <summary>The examples, in the order the section prints them.</summary>
	static readonly string[] Documented =
		[Quoted, LongBracket, RawString, RustRaw, DollarQuoted, Heredoc, Fenced, Heading];

	/// <summary>Every row; the examples first.</summary>
	public static readonly Row[] Rows =
	[
		// ── The section's examples ──

		new("Quoted", "Quoted", Quoted,
			Accepted: ["'ab' ab", "ab ab"],
			Refused:  ["ab ac", "'ab' 'ab'", "'ab ab", "ab  ab"]),

		new("LuaLongString", "LongString", LongBracket,
			Accepted: ["[[]]", "[[a]]", "[==[a]]b]=]c]==]", "[=[\n]]=]", "  [[a]]  -- after"],
			Refused:  ["[==[a]=]", "[=[a]==]", "[[a]]]", "[==[a", "[=[a]=]x", "[=[a] =]"]),

		new("LuaLongComment", "Pair", LongBracket + "\nPair = Name & Name\nName = ['a'..'z']+",
			Accepted: ["a b", "a --[==[ x ]] ]==] b", "a --[[x]]b", "a -- c\nb", "a --[==[ x\n ]==] b"],
			Refused:  ["a --[==[ x ]] b", "a --[==[ x\n ]=] b", "a"]),

		new("CSharpRawString", "RawString", RawString,
			Accepted: ["\"\"\"a\"\"\"", "\"\"\"a\"\"b\"\"\"", "\"\"\"\"a\"\"\"b\"\"\"\"", "\"\"\"\na\n\"\"\""],
			// An opening run is one opening, never three quotes and a quote of content; and a
			// closing run longer than the opening is refused rather than read short.
			Refused:  ["\"\"\"\"a\"\"\"", "\"\"\"a\"\"\"\"", "\"\"\"\"\"\"", "\"\"a\"\"", "\"\"\"a"],
			Text:     "x = \"\"\"a\"\"\"; y = \"\"\"\"b\"\"\"\";",
			Found:    [4, 17]),

		new("RustRaw", "RustRaw", RustRaw,
			Accepted: ["r\"a\"", "r#\"a\"b\"#", "r##\"a\"#b\"##", "r\"\""],
			Refused:  ["r#\"a\"", "r#\"a\"##", "r\"a\"#", "r#\"a"]),

		new("PostgreSqlDollarQuoted", "DollarQuoted", DollarQuoted,
			Accepted: ["$$a$$", "$$$$", "$tag$a$b$tag$", "$t$ $T$ $t$", "$_1$x$_1$", "$$a$b$$"],
			// Tags are compared as written, and a tag cannot begin with a digit.
			Refused:  ["$tag$a$TAG$", "$1$a$1$", "$a$x$b$", "$tag$a", "$$a$"],
			Text:     "a $$x$$ b $t$y$t$",
			Found:    [2, 10]),

		new("Heredoc", "Heredoc", Heredoc,
			Accepted: ["<<EOF\nbody\nEOF", "<<EOF\nEOF", "<<EOF\nEOFX\nEOF", "<<-EOF\n\tbody\n\t\tEOF", "<<EOF\na\n\nb\nEOF", "<<EOF\r\nbody\r\nEOF"],
			// The closing line is the tag alone; the line break after it is not the heredoc's.
			Refused:  ["<<EOF\nbody\n EOF", "<<EOF\nbody", "<<EOF\nbody\neof", "<<EOF body\nEOF", "<<EOF\nbody\nEOF\n", "<<EOF\n\tbody\n\tEOF"]),

		new("MarkdownFence", "Fenced", Fenced,
			Accepted:
			[
				"```\ncode\n```",
				"```cs\ncode\n```",
				"````\n```\n````",
				"```\ncode\n`````",
				"   ```\ncode\n   ```  ",
				"```\n```",
				// Unclosed: the block runs to the end of the input, whether or not its last line ends.
				"```\ncode\n",
				"```\ncode",
				"```\ncode\n``",
			],
			Refused:  ["``\na\n``", "```a`b\nx\n```", "    ```\ncode\n```", "x```\n```"]),

		// Backticks or tildes: one top-level alternative each, so `$f` is two bindings, and a
		// tilde fence is not closed by backticks however many.
		new("MarkdownFenceOfEitherCharacter", "Fence", """
			Fence   = ' '{0,3} & $f: '`'{3,} & [^ '`' | '\r' | '\n']* & eol
			          & { (?!(' '{0,3} & $f & '`'* & ' '* & EndLine) & Line)* }
			          & (' '{0,3} & $f & '`'* & ' '* & EndLine | eof)
			        | ' '{0,3} & $f: '~'{3,} & [^ '\r' | '\n']* & eol
			          & { (?!(' '{0,3} & $f & '~'* & ' '* & EndLine) & Line)* }
			          & (' '{0,3} & $f & '~'* & ' '* & EndLine | eof)
			Line    = [^ '\r' | '\n']+ & EndLine | eol
			EndLine = eol | eof
			""",
			Accepted: ["~~~\ncode\n~~~", "~~~ `info`\na\n~~~~", "~~~~\n~~~\n````\n~~~~", "```\n~~~\n```"],
			Refused:  ["~~\na\n~~", "~~~\na\n~~~\nmore"]),

		new("RestructuredTextHeading", "Heading", Heading,
			Accepted: ["Title\n=====\n", "Title\n=======\n", "T\n=\n"],
			Refused:  ["Title\n====\n", "Title\n=====", "\n=\n"]),

		// ── The other quoting forms the section names ──

		new("CSharpInterpolatedRaw", "Interpolated", """
			Interpolated = $d: '$'+ & { $q: '"'{3,} }
			             & { ('{'{#d} & Hole & '}'{#d} | ?!$q & ?!('{'{#d} & '{'{#d}) & ?!('}'{#d}) & any)* }
			             & $q & ?!'"'
			Hole         = ['a'..'z']+
			""",
			// With n dollars, fewer than n braces are content, n to 2n-1 open a hole after the
			// first k-n, and 2n or more are an error; a closing run outside a hole is too.
			Accepted:
			[
				"$$\"\"\"a{b\"\"\"",
				"$$\"\"\"{{x}}\"\"\"",
				"$$\"\"\"{{{x}}\"\"\"",
				"$$\"\"\"{{x}}}\"\"\"",
				"$$\"\"\"a}b\"\"\"",
				"$\"\"\"{x}\"\"\"",
				"$$\"\"\"\"{{x}}\"\"\"\"",
			],
			Refused:
			[
				"$$\"\"\"{{{{x}}\"\"\"",
				"$$\"\"\"a}}b\"\"\"",
				"$\"\"\"{{x}\"\"\"",
				"$$\"\"\"{{}}\"\"\"",
			]),

		new("CppRaw", "CppRaw", """
			CppRaw = "R\"" & $d: [^ '(' | ')' | '\\' | ' ' | '"']{0,16} & '(' & { (?!(')' & $d & '"') & any)* } & ')' & $d & '"'
			""",
			Accepted: ["R\"(a)\"", "R\"x(a)\"b)x\"", "R\"xy(a)x\")xy\"", "R\"()\""],
			Refused:  ["R\"x(a)y\"", "R\"x(a)\"", "R\"x y(a)x y\"", "R\"abcdefghijklmnopq(a)abcdefghijklmnopq\""]),

		new("SwiftExtendedDelimiter", "SwiftRaw", """
			SwiftRaw = $h: '#'* & '"' & { ('\\' & $h & Escape | ?!('"' & $h) & ?!('\\' & $h) & any)* } & '"' & $h
			Escape   = ['n' | 't' | '\\' | '"']
			""",
			// Without hashes `\n` is an escape and `\q` none; with them a backslash is plain
			// text unless the same hashes follow it.
			Accepted: ["\"a\"", "\"a\\n\"", "\"a\\\"\"", "#\"a\"b\"#", "#\"a\\n\"#", "#\"a\\#n\"#", "##\"a\"#b\"##"],
			Refused:  ["\"a\\q\"", "#\"a\\#q\"#", "#\"a\"", "\"a\"#"]),

		new("InlineCodeSpan", "CodeSpan", """
			CodeSpan = { $b: '`'+ } & { ([^ '`'] | ?!($b & ?!'`') & '`'+)* } & $b & ?!'`'
			""",
			Accepted: ["`a`", "``a``", "``a`b``", "``a```b``", "` `` `"],
			Refused:  ["`a``", "``a`", "```a``", "`a"]),

		new("HeredocWithQuotedTag", "Heredoc", """
			Heredoc = "<<" & ('\'' & $tag: Word & '\'' | $tag: Word) & eol & { (?!($tag & EndLine) & Line)* } & $tag & ?=EndLine
			Word    = ['A'..'Z' | 'a'..'z' | '_']+
			Line    = [^ '\r' | '\n']+ & EndLine | eol
			EndLine = eol | eof
			""",
			Accepted: ["<<'EOF'\nx\nEOF", "<<EOF\nx\nEOF"],
			Refused:  ["<<'EOF'\nx\n'EOF'", "<<'EOF\nx\nEOF"]),

		// Bindings are per call: each element reads its own name back, however deep.
		new("XmlElement", "Element", """
			Element = '<' & $n: Name & '>' & (Element | Text)* & "</" & $n & '>'
			Name    = ['a'..'z']+
			Text    = { [^ '<']+ }
			""",
			Accepted: ["<a></a>", "<a><b></b></a>", "<a>x<b>y</b>z</a>", "<ab><a></a></ab>", "<a><a><a></a></a></a>"],
			Refused:  ["<a><b></a></b>", "<a></b>", "<ab></a>", "<a></ab>", "<a><b></b>"],
			Text:     "x<a><b></b></a> <c></d> <e></e>",
			Found:    [1, 24]),

		// ── Matching again ──

		new("CaseIsTheBindings", "Keyword", """
			Keyword = $k: "end"i & ' ' & $k
			""",
			Accepted: ["End End", "end end", "END END"],
			Refused:  ["End end", "end END"]),

		new("ReferenceRepeated", "R", "R = $q: \"ab\" & $q*",
			Accepted: ["ab", "abab", "ababab"],
			Refused:  ["aba", "a"]),

		new("ReferenceInsideALook", "R", "R = $q: 'a' & 'b'{#q} & ?=$q & any",
			Accepted: ["aba"],
			Refused:  ["abb", "ab"]),

		new("UntilTheSameCharacter", "R", "R = $q: ['a'..'z'] & (?!$q & any)* & $q",
			Accepted: ["axyza", "aa", "b-b"],
			Refused:  ["axyz", "a"]),

		// ── Give-back: a greedy binding hands an item back and the reference is tried again ──

		new("GreedyBindingGivesBack", "R", "R = $q: 'a'+ & 'a' & $q",
			Accepted: ["aaa", "aaaaa"],
			Refused:  ["aaaa", "aa", "a"]),

		new("BindingOfAnyLengthGivesBack", "R", "R = $q: ['a'..'z']+ & $q",
			Accepted: ["abab", "aa", "abcabc"],
			Refused:  ["aba", "abcab", "ab"]),

		new("BindingOfAChoiceGivesBack", "R", "R = $q: (\"ab\" | 'a') & 'b' & $q",
			Accepted: ["aba", "abbab"],
			Refused:  ["abab", "ab"]),

		// Without the atomic opening, `""""a"""` is three quotes and a string that starts with one.
		new("RawStringWithoutAtomicGroups", "R", "R = $q: '\"'{3,} & (?!$q & any)* & $q",
			Accepted: ["\"\"\"\"a\"\"\"", "\"\"\"a\"\"\""],
			Refused:  ["\"\"\"a\"\"\"\"", "\"\"\"a\"\"", "\"\"\"\"\"\"\"\"a\"\"\"\""]),

		// ── After zero turns ──

		new("OptionInsideTheBinding", "R", "R = $q: ('a'?) & 'b' & $q",
			Accepted: ["b", "aba"],
			Refused:  ["ab", "ba", "abaa"]),

		new("StarOverTheBinding", "R", "R = ($q: 'a')* & 'b' & $q",
			Accepted: ["b", "aba", "aabaa"],
			Refused:  ["aaba", "ba", "abaa"]),

		new("BoundedRunOverTheBinding", "R", "R = ($q: 'a'){0,3} & 'b' & $q",
			Accepted: ["b", "aaabaaa"],
			Refused:  ["aaaabaaaa", "aab"]),

		new("PlusOverTheBinding", "R", "R = ($q: 'a')+ & 'b' & $q",
			Accepted: ["aba", "aabaa"],
			Refused:  ["b", "aaba"]),

		new("StarOverABindingOfAValuedRule", "R", "R = ($q: V)* & 'b' & $q\nV : @int = ['0'..'9'] => @(1)",
			Accepted: ["b", "1b1", "12b12"],
			Refused:  ["12b21", "1b"]),

		new("BindingReadInItsOwnTurn", "R", "R = ($q: 'a'+ & '-' & $q & ',')*",
			Accepted: ["", "a-a,", "a-a,aa-aa,"],
			Refused:  ["a-aa,", "aa-a,"]),

		new("BindingInBothRoutesOfANestedChoice", "R", "R = ('x' & $q: 'a'+ | 'y' & $q: 'b'+) & ':' & $q",
			Accepted: ["xaa:aa", "yb:b"],
			Refused:  ["xa:b", "yb:bb"]),

		// ── Counts by a length ──

		new("CountExactly", "R", "R = $q: 'a'+ & 'b'{#q}",
			Accepted: ["ab", "aabb", "aaabbb"],
			Refused:  ["aab", "abb", "a"]),

		new("CountAtLeast", "R", "R = $q: 'a'+ & 'b'{#q,}",
			Accepted: ["ab", "aabb", "aabbb"],
			Refused:  ["aab", "a"]),

		// `{n,#q}` with n over the length is a count nothing meets.
		new("CountFromANumberToALength", "R", "R = $q: 'a'* & ':' & 'b'{2,#q}",
			Accepted: ["aa:bb", "aaa:bb", "aaa:bbb"],
			Refused:  ["a:b", "a:bb", "aaa:bbbb", "aa:b", ":"]),

		new("CountFromALengthToANumber", "R", "R = $q: 'a'* & ':' & 'b'{#q,3}",
			Accepted: [":", "a:b", "a:bbb", "aa:bbb"],
			Refused:  ["aaaa:bbbb", "a:", "a:bbbb"]),

		new("CountBetweenTwoLengths", "R", "R = $q: 'a'* & $r: 'c'* & ':' & 'b'{#q,#r}",
			Accepted: ["acc:b", "acc:bb", ":", "ac:b"],
			Refused:  ["aac:b", "acc:bbb", "acc:", "a:"]),

		new("CountOfAnEmptyBinding", "R", "R = $q: 'a'* & ':' & 'b'{#q}",
			Accepted: [":", "a:b", "aa:bb"],
			Refused:  [":b", "a:"]),

		new("CountFromAValueParameter", "R", "R = P(2)\nP(n) = $q: 'a'+ & ':' & 'b'{n,#q}",
			Accepted: ["aa:bb", "aaa:bb", "aaa:bbb"],
			Refused:  ["a:b", "aa:b", "aa:bbb"]),

		new("CountOfARepeatedBinding", "R", "R = $p: 'n'* & ($q: 'a'){#p} & ':' & $q",
			Accepted: ["nnaa:aa", ":", "na:a"],
			Refused:  ["nnaa:a", "na:"]),

		// ── Trivia: part of a binding's text, and woven around a reference ──

		new("TriviaAroundAReference", "R", "trivia = [' ']*\nR = $q: W & '=' & $q\nW = ['a'..'z']+",
			Accepted: ["ab=ab", "ab = ab", " ab = ab "],
			Refused:  ["ab = ac", "ab = a b"]),

		new("TriviaInsideABinding", "R", "trivia = [' ']*\nR = $q: (W & W) & '=' & $q\nW = ['a'..'z']+",
			Accepted: ["a b=a b", "a b = a b", "ab=ab"],
			Refused:  ["a b = ab", "a b = a  b"]),
	];

	public static TheoryData<string> RowNames()
	{
		return [.. Rows.Select(static row => row.Name)];
	}

	public static TheoryData<string> FindRowNames()
	{
		return [.. Rows.Where(static row => row.Found is not null).Select(static row => row.Name)];
	}

	static Row Named(string name)
	{
		return Rows.Single(row => row.Name == name);
	}
}
