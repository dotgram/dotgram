using System;
using System.Collections.Generic;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Deep nests whose innermost reading can be split many ways, each refused at the top: what the
/// reader answers for them, written down before anything is done about how long it takes.
/// </summary>
/// <remarks>
/// <para>
/// <b>The shape.</b> <c>Start = P &amp; '!'</c> over <c>P = '(' &amp; P &amp; ')' | Bs &amp; Cs</c>: every
/// way of cutting the innermost run of <c>b</c> between <c>Bs</c> and <c>Cs</c> stands inside the
/// nest, and only the <c>'!'</c> at the top refuses it. A refusal at the top asks the innermost
/// reading again, and every attempt goes down the whole nest to it and back up: the depth times the
/// number of cuts.
/// </para>
/// <para>
/// Each grammar here pins one thing a remedy has to keep: values built while the text is read, with
/// a construction that throws on one cut (<c>Built</c>, <c>BuiltZero</c>, <c>Stood</c> — the last one
/// where a frame in the middle of the nest stops building and builds again once it stands); a guard
/// over a value read before the call and one read after it, as XML closes a tag (<c>Built</c>); a
/// guard over the value the nest itself built, which no remedy that does not look at values may skip
/// (<c>Guard</c>); a repetition before the call (<c>Prefix</c>); braces on the way back up
/// (<c>Sealed</c>); and a nest inside the element of a recovering repetition (<c>Recover</c>).
/// <c>Open</c> and <c>Vary</c> are the controls: one with nothing to read on the way up, one with a
/// single way to read the innermost run.
/// </para>
/// <para>
/// The answers are what the reader gives today, refusals and exceptions included; a change to how
/// it reads these nests must leave every one of them as it is.
/// </para>
/// </remarks>
public sealed class DeepDescentTests
{
	const string Members = """
		static int Split(int b, int c)
		{
			if (b == 3 && c == 1)
				throw new global::System.InvalidOperationException("split 3/1");

			return b * 100 + c;
		}

		static int Empty(int b, int c)
		{
			if (b == 0 && c == 0)
				throw new global::System.InvalidOperationException("empty split");

			return b * 100 + c;
		}

		static int Pair(int x, int y)
		{
			if (x == 1 && y == 3)
				throw new global::System.InvalidOperationException("pair 1/3");

			return x * 100 + y;
		}
		""";

	public const string Split = """
		Start = P & '!'
		P     = '(' & P & ')' | Bs & Cs
		Bs    = 'b'*
		Cs    = 'b'*
		parse Start
		""";

	public const string Open = """
		Start = P & '!'
		P     = '(' & P | Bs & Cs
		Bs    = 'b'*
		Cs    = 'b'*
		parse Start
		""";

	public const string Vary = """
		Start = P & '!'
		P     = '(' & P & ')' | Bs
		Bs    = 'b'*
		parse Start
		""";

	public const string Built = """
		Start : @int = p: P & '!' => @(p)
		P     : @int = t: D & '(' & inner: P & ')' & u: D & when @(t == u) => @(inner + t)
		             | b: Bs & c: Cs => @(Split(b, c))
		D     : @int = d: ['0'..'9'] => @(d[0] - '0')
		Bs    : @int = t: 'b'* => @(t.Length)
		Cs    : @int = t: 'b'* => @(t.Length)
		parse Start
		""";

	public const string BuiltZero = """
		Start : @int = p: P & '!' => @(p)
		P     : @int = t: D & '(' & inner: P & ')' & u: D & when @(t == u) => @(inner + t)
		             | b: Bs & c: Cs => @(Empty(b, c))
		D     : @int = d: ['0'..'9'] => @(d[0] - '0')
		Bs    : @int = t: 'b'* => @(t.Length)
		Cs    : @int = t: 'b'* => @(t.Length)
		parse Start
		""";

	public const string Stood = """
		Start : @int = p: P & '!' => @(p)
		P     : @int = '(' & inner: P & ')' => @(inner)
		             | x: Xs & y: Ys => @(Pair(x, y))
		Xs    : @int = t: 'a'* => @(t.Length)
		Ys    : @int = t: ('a' & 'a' & 'a')* => @(t.Length)
		parse Start
		""";

	public const string Guard = """
		Start : @int = p: P & '!' => @(p)
		P     : @int = '(' & inner: P & ')' & when @(inner >= 0) => @(inner + 1)
		             | b: Bs & c: Cs => @(b - c)
		Bs    : @int = t: 'b'* => @(t.Length)
		Cs    : @int = t: 'b'* => @(t.Length)
		parse Start
		""";

	public const string Prefix = """
		Start = P & '!'
		P     = '(' & Item* & P & ')' | Bs & Cs
		Item  = 'a'
		Bs    = 'b'*
		Cs    = 'b'*
		parse Start
		""";

	public const string Sealed = """
		Start = P & '!'
		P     = '(' & P & { A } & ')' | Bs & Cs
		A     = 'a'* & 'a'
		Bs    = 'b'*
		Cs    = 'b'*
		parse Start
		""";

	public const string Recover = """
		Start : @int[] = Element* recover ';' & eof
		Element : @int = P & '!' & ';' => @(1)
		P     = '(' & P & ')' | Bs & Cs
		Bs    = 'b'*
		Cs    = 'b'*
		parse Start
		""";

	static readonly Dictionary<string, string> _grammars = new()
	{
		["Split"]     = Split,
		["Open"]      = Open,
		["Vary"]      = Vary,
		["Built"]     = Built,
		["BuiltZero"] = BuiltZero,
		["Stood"]     = Stood,
		["Guard"]     = Guard,
		["Prefix"]    = Prefix,
		["Sealed"]    = Sealed,
		["Recover"]   = Recover,
	};

	[Theory]
	[InlineData("Split",     "((bbb))!",           "accepted 0..8")]
	[InlineData("Split",     "((bbb))",            "refused at 7: Expected '!'.")]
	[InlineData("Split",     "((bbb)",             "refused at 6: Expected ')'.")]
	[InlineData("Open",      "((bbb!",             "accepted 0..6")]
	[InlineData("Open",      "((bbb",              "refused at 5: Expected '!'.")]
	[InlineData("Vary",      "((bbb))",            "refused at 7: Expected '!'.")]
	[InlineData("Built",     "1(2(bbbbb)2)1!",     "accepted 503")]
	[InlineData("Built",     "1(2(bbbb)2)1",       "throws split 3/1")]
	[InlineData("Built",     "1(2(bbbbb)2)1",      "refused at 13: Expected '!'.")]
	[InlineData("Built",     "1(2(bb)1)1!",        "refused at 8: when @(t == u) said no.")]
	[InlineData("BuiltZero", "1(1(bbbb)1)1",       "refused at 12: Expected '!'.")]
	[InlineData("BuiltZero", "1(bbbb)1!",          "accepted 401")]
	[InlineData("Stood",     "(aaaa)",             "throws pair 1/3")]
	[InlineData("Stood",     "((aaaaaa))!",        "accepted 600")]
	[InlineData("Guard",     "((bbb))!",           "accepted 5")]
	[InlineData("Guard",     "((bbb))",            "refused at 7: Expected '!'.")]
	[InlineData("Prefix",    "(a(aa(bbb)))",       "refused at 12: Expected '!'.")]
	[InlineData("Prefix",    "(a(aa(bbb)))!",      "accepted 0..13")]
	[InlineData("Sealed",    "((bbbaa)aa)",        "refused at 11: Expected '!'.")]
	[InlineData("Sealed",    "((bbbaa)aa)!",       "accepted 0..12")]
	[InlineData("Recover",   "((bb))!;(b);(()!;",  "accepted 1")]
	public void A_nest_is_answered_as_it_was(string grammar, string input, string answer)
	{
		Assert.Equal(answer, Answer(Compiled(_grammars[grammar]), input));
	}

	/// <summary>What one reading answers, in a line: the value or the span, the refusal, or what was thrown.</summary>
	static string Answer(Type parser, string input)
	{
		try
		{
			var match = EmittedCode.Match(parser.Assembly, "Grammar", "TryParseStart", input);

			if (!match.IsSuccess)
				return $"refused at {match.Position}: {match.Error}";

			return match.Value switch
			{
				int[] values => "accepted " + string.Join(",", values),
				string text  => $"accepted 0..{text.Length}",
				var value    => "accepted " + value,
			};
		}
		catch (TargetInvocationException thrown) when (thrown.InnerException is InvalidOperationException inner)
		{
			return "throws " + inner.Message;
		}
	}

	static readonly Dictionary<string, Type> _compiled = [];

	static Type Compiled(string grammar)
	{
		lock (_compiled)
		{
			if (!_compiled.TryGetValue(grammar, out var parser))
			{
				var result = GramCompiler.Compile(grammar, new GramCompilerOptions
				{
					CSharpScanner = RoslynCSharpScanner.Instance,
					Carrier       = CarrierKind.Immediate,
				});

				EmittedCode.Quiet(result.Diagnostics);

				_compiled[grammar] = parser = EmittedCode.Compile(Assert.Single(result.Sources).Text, declarationMembers: Members).GetType("Grammar")!;
			}

			return parser;
		}
	}
}
