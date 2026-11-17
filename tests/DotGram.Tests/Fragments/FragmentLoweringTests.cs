using System;
using System.Linq;

using DotGram.Grammar.Fragments;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests.Fragments;

/// <summary>
/// A host fragment read by C#'s parser into the closed fragment tree: each form it keeps, how C#'s
/// precedence comes through, where each part stands, and each form it refuses.
/// </summary>
public sealed class FragmentLoweringTests
{
	[Theory]
	[InlineData("\"CREATE\"",          "\"CREATE\"")]
	[InlineData("\"a\\tb\\u00e9\"",    "\"a\\u0009b\\u00E9\"")]
	[InlineData("'x'",                 "'x'")]
	[InlineData("128",                 "128")]
	[InlineData("0x7F",                "0x7F")]
	[InlineData("18446744073709551615", "18446744073709551615")]
	[InlineData("1.5",                 "1.5r")]
	[InlineData("true",                "true")]
	[InlineData("false",               "false")]
	[InlineData("null",                "null")]
	[InlineData("name",                "name")]
	[InlineData("string.Empty",        "string.Empty")]
	[InlineData("Syntax.Squared",      "Syntax.Squared")]
	[InlineData("name.Length",         "name.Length")]
	[InlineData("F(a, b)",             "call(F a b)")]
	[InlineData("Build.Values.Text(name)", "call(Build.Values.Text name)")]
	[InlineData("F(a).G(b)",           "call(call(F a).G b)")]
	[InlineData("new Add(left, right)", "new(Add left right)")]
	[InlineData("new Build.Clause.None()", "new(Build.Clause.None)")]
	[InlineData("!F(x)",               "!call(F x)")]
	[InlineData("!(a == b)",           "!((== a b))")]
	[InlineData("!(x is null)",        "!((is-null x))")]
	[InlineData("x is null",           "(is-null x)")]
	[InlineData("x is not null",       "(is-not-null x)")]
	[InlineData("x == null",           "(== x null)")]
	[InlineData("a + b - 1",           "(- (+ a b) 1)")]
	[InlineData("(a)",                 "(a)")]
	public void Every_portable_form_lowers(string text, string expected)
	{
		var lowered = RoslynFragmentLowering.Lower(text);

		Assert.True(lowered.IsPortable, string.Join(", ", lowered.Refusals.Select(static one => one.Form)));
		Assert.Equal(expected, Dump(lowered.Root));
	}

	[Theory]
	// `??` and `?:` are right-associative, and `??` binds tighter than `?:`.
	[InlineData("a ?? b ?? c",         "(?? a (?? b c))")]
	[InlineData("c ? a : b ?? d",      "(?: c a (?? b d))")]
	[InlineData("c ? a : d ? e : f",   "(?: c a (?: d e f))")]
	[InlineData("a == b && c",         "(&& (== a b) c)")]
	[InlineData("a || b && c",         "(|| a (&& b c))")]
	[InlineData("x is null || y",      "(|| (is-null x) y)")]
	[InlineData("a < b == c > d",      "(== (< a b) (> c d))")]
	[InlineData("a + b == c",          "(== (+ a b) c)")]
	[InlineData("a ?? b == c",         "(?? a (== b c))")]
	[InlineData("verb == \"CREATE\" && owner is null", "(&& (== verb \"CREATE\") (is-null owner))")]
	[InlineData("alias ?? name",       "(?? alias name)")]
	[InlineData("name.Length <= 128",  "(<= name.Length 128)")]
	public void Precedence_is_what_CSharp_reads(string text, string expected)
	{
		var lowered = RoslynFragmentLowering.Lower(text);

		Assert.True(lowered.IsPortable);
		Assert.Equal(expected, Dump(lowered.Root));
	}

	[Fact]
	public void Every_part_stands_where_the_text_has_it()
	{
		const string text = " verb == \"CREATE\"  &&  F(owner, 1) is not null ";

		var lowered = RoslynFragmentLowering.Lower(text);
		var root    = Assert.IsType<Fragment.Binary>(lowered.Root);

		Assert.Equal("verb == \"CREATE\"  &&  F(owner, 1) is not null", Of(text, root));

		var left  = Assert.IsType<Fragment.Binary>(root.Left);
		var right = Assert.IsType<Fragment.IsNull>(root.Right);
		var call  = Assert.IsType<Fragment.Call>(right.Operand);

		Assert.Equal("verb == \"CREATE\"", Of(text, left));
		Assert.Equal("\"CREATE\"",         Of(text, left.Right));
		Assert.Equal("F(owner, 1)",        Of(text, call));
		Assert.Equal("owner",              Of(text, call.Arguments[0]));
		Assert.Equal("1",                  Of(text, call.Arguments[1]));
	}

	[Theory]
	[InlineData("x?.Name",                     "ConditionalAccessExpression", "x?.Name")]
	[InlineData("x!",                          "SuppressNullableWarningExpression", "x!")]
	[InlineData("o is { Bare: true }",         "RecursivePattern", "{ Bare: true }")]
	[InlineData("o is Foo f",                  "DeclarationPattern", "Foo f")]
	[InlineData("o is \"a\" or \"b\"",         "OrPattern", "\"a\" or \"b\"")]
	[InlineData("o is Foo",                    "IsExpression", "o is Foo")]
	[InlineData("o as Foo",                    "AsExpression", "o as Foo")]
	[InlineData("x switch { _ => 1 }",         "SwitchExpression", "x switch { _ => 1 }")]
	[InlineData("(int)x",                      "CastExpression", "(int)x")]
	[InlineData("a[0]",                        "ElementAccessExpression", "a[0]")]
	[InlineData("[a, b]",                      "CollectionExpression", "[a, b]")]
	[InlineData("new Foo { A = 1 }",           "ObjectInitializerExpression", "{ A = 1 }")]
	[InlineData("o with { A = 1 }",            "WithExpression", "o with { A = 1 }")]
	[InlineData("$\"{a}\"",                    "InterpolatedStringExpression", "$\"{a}\"")]
	[InlineData("@\"a\"",                      "VerbatimString", "@\"a\"")]
	[InlineData("\"\"\"a\"\"\"",               "RawString", "\"\"\"a\"\"\"")]
	[InlineData("\"a\"u8",                     "Utf8StringLiteralExpression", "\"a\"u8")]
	[InlineData("x => x",                      "SimpleLambdaExpression", "x => x")]
	[InlineData("a * b",                       "MultiplyExpression", "a * b")]
	[InlineData("a / b",                       "DivideExpression", "a / b")]
	[InlineData("a % b",                       "ModuloExpression", "a % b")]
	[InlineData("a << 1",                      "LeftShiftExpression", "a << 1")]
	[InlineData("a & b",                       "BitwiseAndExpression", "a & b")]
	[InlineData("~a",                          "BitwiseNotExpression", "~a")]
	[InlineData("-a",                          "UnaryMinusExpression", "-a")]
	[InlineData("F<int>(a)",                   "GenericName", "F<int>")]
	[InlineData("x.F<int>(a)",                 "GenericName", "F<int>")]
	[InlineData("new List<int>()",             "GenericName", "List<int>")]
	[InlineData("new()",                       "ImplicitObjectCreationExpression", "new()")]
	[InlineData("new int[] { 1 }",             "ArrayCreationExpression", "new int[] { 1 }")]
	[InlineData("1L",                          "NumericSuffix", "1L")]
	[InlineData("1.5m",                        "NumericSuffix", "1.5m")]
	[InlineData("1e3",                         "Exponent", "1e3")]
	[InlineData("0b101",                       "BinaryInteger", "0b101")]
	[InlineData("1_000",                       "DigitSeparator", "1_000")]
	[InlineData(".5",                          "NumericLiteralExpression", ".5")]
	[InlineData("a /* why */ + b",             "Comment", "/* why */")]
	[InlineData("!x",                          "LogicalNotExpression", "!x")]
	[InlineData("!(a && b)",                   "LogicalNotExpression", "!(a && b)")]
	[InlineData("F(out var r)",                "OutArgument", "out var r")]
	[InlineData("F(name: a)",                  "NamedArgument", "name: a")]
	[InlineData("nameof(a)",                   "NameOf", "nameof(a)")]
	[InlineData("this",                        "ThisExpression", "this")]
	[InlineData("default",                     "DefaultLiteralExpression", "default")]
	[InlineData("typeof(int)",                 "TypeOfExpression", "typeof(int)")]
	[InlineData("global::System.Math.Max(a, b)", "AliasQualifiedName", "global::System")]
	[InlineData("@class",                      "VerbatimIdentifier", "@class")]
	[InlineData("F(a)(b)",                     "DelegateInvocation", "F(a)(b)")]
	[InlineData("a = b",                       "SimpleAssignmentExpression", "a = b")]
	[InlineData("a b",                         "Unparsable", "a b")]
	[InlineData("a +",                         "Unparsable", "a +")]
	public void Every_other_form_is_refused_where_it_stands(string text, string form, string at)
	{
		var lowered = RoslynFragmentLowering.Lower(text);

		Assert.False(lowered.IsPortable);

		var refusal = lowered.Refusals.FirstOrDefault(one => one.Form == form);

		Assert.True(refusal is not null, $"{form} not among: {string.Join(", ", lowered.Refusals.Select(static one => one.Form))}");
		Assert.Equal(at, Of(text, refusal));
	}

	[Fact]
	public void A_form_inside_a_refused_one_is_refused_too()
	{
		// The census counts what blocks a fragment, not only what blocks it first: here the cast
		// and the `?.` inside it, and the `!` inside an argument of the call the `?.` reads.
		const string text = "(Foo)a?.F(b!)";

		var forms = RoslynFragmentLowering.Lower(text).Refusals.Select(static one => one.Form).ToArray();

		Assert.Equal(["CastExpression", "ConditionalAccessExpression", "SuppressNullableWarningExpression"], forms);
	}

	[Theory]
	[InlineData("(List<int>)x",              "CastExpression")]
	[InlineData("new int[] { 1, 2 }",        "ArrayCreationExpression")]
	[InlineData("new int[0]",                "ArrayCreationExpression")]
	[InlineData("o with { A = 1, B = c }",   "WithExpression")]
	[InlineData("new Foo(a) { A = 1 }",      "ObjectInitializerExpression")]
	[InlineData("new Foo(a) { A = x?.B }",   "ObjectInitializerExpression ConditionalAccessExpression")]
	[InlineData("new[] { a, b! }",           "ImplicitArrayCreationExpression SuppressNullableWarningExpression")]
	[InlineData("a = b",                     "SimpleAssignmentExpression")]
	public void What_a_refused_form_is_written_with_adds_nothing_but_its_values_count(string text, string expected)
	{
		// A type, an initializer's braces and the `A =` of the members it sets are the refused
		// form's own; the values it holds are lowered and refused on their own.
		var forms = RoslynFragmentLowering.Lower(text).Refusals.Select(static one => one.Form);

		Assert.Equal(expected, string.Join(" ", forms));
	}

	[Fact]
	public void Literal_values_are_decoded()
	{
		var text = Assert.IsType<Fragment.Text>(RoslynFragmentLowering.Lower("\"a\\\"b\\\\c\\u00e9\\U0001F600\\0\"").Root);

		Assert.Equal("a\"b\\cé\U0001F600\0", text.Value);

		var character = Assert.IsType<Fragment.Character>(RoslynFragmentLowering.Lower("'\\''").Root);

		Assert.Equal('\'', character.Value);

		var hex = Assert.IsType<Fragment.Integer>(RoslynFragmentLowering.Lower("0xFFFFFFFF").Root);

		Assert.Equal(0xFFFFFFFFUL, hex.Value);
		Assert.True(hex.IsHexadecimal);
	}

	[Fact]
	public void The_language_version_is_fixed()
	{
		// The oldest Roslyn the generator loads into knows C# 13 and no later; a build on a newer
		// SDK and the tool on the newest Roslyn read a fragment as it does.
		Assert.Equal(Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp13, RoslynFragmentLowering.Options.LanguageVersion);
	}

	/// <summary>
	/// The tree as a short S-expression: what a test can state in one line.
	/// </summary>
	internal static string Dump(Fragment fragment)
	{
		return fragment switch
		{
			Fragment.Text text              => "\"" + string.Concat(text.Value.Select(static c => c is >= ' ' and < '\x7F' and not '"' and not '\\' ? c.ToString() : $"\\u{(int)c:X4}")) + "\"",
			Fragment.Character character    => $"'{character.Value}'",
			Fragment.Integer { IsHexadecimal: true } integer => $"0x{integer.Value:X}",
			Fragment.Integer integer        => integer.Value.ToString(System.Globalization.CultureInfo.InvariantCulture),
			Fragment.Real real              => real.Digits + "r",
			Fragment.Boolean boolean        => boolean.Value ? "true" : "false",
			Fragment.Null                   => "null",
			Fragment.Name name              => name.Identifier,
			Fragment.Member member          => $"{Dump(member.Target)}.{member.Identifier}",
			Fragment.Call call              => $"call({string.Join(" ", new[] { Dump(call.Target) }.Concat(call.Arguments.Select(Dump)))})",
			Fragment.New creation           => $"new({string.Join(" ", new[] { Dump(creation.Type) }.Concat(creation.Arguments.Select(Dump)))})",
			Fragment.Binary binary          => $"({Operator(binary.Operator)} {Dump(binary.Left)} {Dump(binary.Right)})",
			Fragment.Not not                => "!" + Dump(not.Operand),
			Fragment.Coalesce coalesce      => $"(?? {Dump(coalesce.Left)} {Dump(coalesce.Right)})",
			Fragment.Conditional choice     => $"(?: {Dump(choice.Condition)} {Dump(choice.WhenTrue)} {Dump(choice.WhenFalse)})",
			Fragment.IsNull test            => $"({(test.IsNegated ? "is-not-null" : "is-null")} {Dump(test.Operand)})",
			Fragment.Parenthesized inner    => $"({Dump(inner.Inner)})",
			Fragment.Unportable refused     => $"<{refused.Form}>",
			_                               => throw new ArgumentOutOfRangeException(nameof(fragment), fragment, null),
		};

		static string Operator(FragmentOperator operation)
		{
			return operation switch
			{
				FragmentOperator.And            => "&&",
				FragmentOperator.Or             => "||",
				FragmentOperator.Equal          => "==",
				FragmentOperator.NotEqual       => "!=",
				FragmentOperator.Less           => "<",
				FragmentOperator.LessOrEqual    => "<=",
				FragmentOperator.Greater        => ">",
				FragmentOperator.GreaterOrEqual => ">=",
				FragmentOperator.Add            => "+",
				FragmentOperator.Subtract       => "-",
				_                               => throw new ArgumentOutOfRangeException(nameof(operation)),
			};
		}
	}

	static string Of(string text, Fragment fragment)
	{
		return text.Substring(fragment.Span.Start, fragment.Span.Length);
	}
}
