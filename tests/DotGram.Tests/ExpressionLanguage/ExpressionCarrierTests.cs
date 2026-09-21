using System;
using System.Linq.Expressions;

using DotGram.ExpressionLanguage;

using Xunit;

namespace DotGram.Tests.ExpressionLanguage;

/// <summary>
/// The expression language compiled on the tape and compiled immediately read the same
/// language and build the same tree.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="CarrierTests"/> asks this of the shapes the second carrier had to learn one
/// at a time, on grammars written for the question. This asks it of the largest grammar in
/// the repository, which uses the parts those do not: a context the caller hands over
/// (§7.7), marks laid over an operand (§7.8), guards that read what was captured, and a
/// hundred rules of ladder above them.
/// </para>
/// <para>
/// The trees are compared as they print. A <c>LambdaExpression</c> has no equality of its
/// own, and building one to compare by would be writing a second grammar; what it prints
/// is what the API itself offers, and it tells apart everything the two carriers could
/// disagree about — which operator, which operand, which parameter, in what order.
/// </para>
/// </remarks>
public sealed class ExpressionCarrierTests
{
	public static TheoryData<string> Inputs =>
	[
		// The ladder, the nest, the block: what the benchmark reads, and for the same
		// reasons it reads it.
		"(int x) => x",
		"(int x) => x * x - 1",
		"(int x, int y) => (x + y) * 3 - x / 5",
		"(string s) => s.Length",
		"(int x) => { x += 1; x *= 2; return x; }",
		"(int x) => ((((x))))",
		"(int x) => x > 1 ? x : -x",
		"(int x) => { int y = x + 1; return y * y; }",
		"(int x) => { while (x < 10) { x += 1; } return x; }",
		"(int n) => { int sum = 0; for (int i = 0; i < n; i++) { sum += i; } sum }",
		"(double d) => (int)d + 1",

		// A name that is a type and not a variable — read speculatively as a variable
		// first, which is the shape that used to need the tape (docs/next.md).
		"using System; (int x) => Math.Max(x, 1)",
		"(int x) => System.Math.Max(x, 1)",

		// §7.8, which is the thing the immediate carrier learned last.
		"(int x) => checked(x + 1)",
		"(int x) => unchecked(x * 2)",
		"(int x) => checked(x + unchecked(x * 2))",

		// A member that is a call, a member that is a property, and the two together:
		// the shape a reading has to try both ways round.
		"(string s) => s.Trim()",
		"(string s) => s.Trim().Length",
		"(int[] a) => a.Length + a[0]",
		"(int x) => new int[] { x, 1 }.Length",
		"(string s) => s == null ? 0 : s.Length",

		// A jump names the loop it is written in, and a loop knows how far it reaches only
		// once its body is read: the shape a reading that builds where it reads has to be
		// told about before it meets one.
		"(int n) => { int sum = 0; for (int i = 0; i < n; i++) { if (i % 2 == 0) continue; sum += i; } sum }",
		"(int n) => { while (true) { if (n > 3) break; n += 1; } n }",
		"(int n) => { do { n += 1; if (n > 3) break; } while (n < 10); n }",
		"(int n) => { switch (n) { case 1: n = 10; break; default: n = 0; break; } n }",
		"(int n) => { while (n < 9) { while (n < 5) { n += 2; } n += 1; } n }",

		// An integer typed by its value, and the one constant read across a minus.
		"() => 0xFFFFFFFF",
		"() => 9223372036854775808",
		"(int x) => -2147483648 + x",
		"using System; (Exception e) => e.Message.Length",

		// Conversions C# makes unasked, and the overload they let a call find.
		"(int x) => x + 1.5",
		"(string s, int n) => s + n",
		"using System; (int x) => Math.Sqrt(x)",
		"(byte b) => { b += 1; b }",
		"(bool c) => c ? 1 : 2L",
		"() => -3000000000",

		// A generic method whose type arguments nobody wrote, which is LINQ.
		"using System.Collections.Generic; using System.Linq; (List<int> l) => l.Where((int n) => n > 1).Select((int n) => n * 2).ToArray()",
		"using System.Collections.Generic; using System.Linq; (List<int> l) => l.Select((int n) => n.ToString()).ToArray()",

		// A method a `using` brings rather than the type declaring it, plain and guarded.
		"using DotGram.Tests.ExpressionLanguage; (int x) => x.Doubled()",
		"using DotGram.Tests.ExpressionLanguage; (string s) => s?.Shout(\"!\")",

		// A lambda inside an expression: one that closes over what is around it, and two
		// beside each other whose parameters are each their own.
		"(int x) => { var f = (int y) => y + x; f(1) }",
		"(int x) => { var f = (int y) => y + 1; var g = (int y) => y * 2; f(1) + g(2) }",

		// A guard and how far it reaches: one step, a whole chain, an index, a nullable held
		// value, and the ternary whose number keeps its point. A receiver that is never null
		// is not here: that one throws rather than refuses, and this asks about answers.
		"(string s) => s?.Length",
		"(string s) => s?.Trim().Length",
		"(int[] a) => a?[0]",
		"(string s) => (s?.Length)?.ToString()",
		"(int x) => x > 0 ? .5 : 1.5",

		// A declaration whose type is its initializer's, the same inside a `for`, and the word
		// itself used as a name.
		"(double x) => { var half = x / 2.0; return half; }",
		"(int n) => { int sum = 0; for (var i = 0; i < n; i++) { sum += i; } sum }",
		"(int var) => { var += 2; var }",

		// A type where a value is wanted, what it defaults to, and a name asked for as written.
		"using System; (int x) => typeof(int[])",
		"(int x) => default(int) + x",
		"(int x) => nameof(x)",

		// A `using`, a nested type through one, and one that names nothing.
		"using System.Text; (int x) => new StringBuilder(16).Length",
		"using System; (Environment.SpecialFolder f) => f",
		"using System.Nowhere; (int x) => x",

		// The calling assembly's internal type, constructor, field, method and property.
		"using DotGram.Tests.ExpressionLanguage; () => new Hidden(21).Twice() + new Hidden(1).Value + Hidden.Seven",

		// Interpolated and raw strings, whose holes each reading reads over a window of its own.
		"(int x) => $\"a{x}b\"",
		"(int x) => $\"{x,5:D3}\"",
		"(int x) => $@\"a\"\"{x}\"",
		"(int x) => $\"{$\"{x}\"}\"",
		"(int x) => \"\"\"a\"b\"\"\"",
		"(int x) => $$\"\"\"{x} {{x}}\"\"\"",
		"(int x) => $$$\"\"\"{{{x}}}\"\"\"",

		// And the refusals: a refusal is an answer and has to be the same answer.
		"(int x) => x *",
		"(int x) => { x += 1;",
		"(int x) => y",
		"(int x) =>",
		"",
	];

	/// <summary>Lambdas that say no types, whose bodies are read before the types are known and again after.</summary>
	public static TheoryData<string> Untyped =>
	[
		"using System.Linq; (int[] a) => a.Select(n => n * 2).Sum()",
		"using System.Linq; (int[] a) => a.Aggregate((s, n) => s + n)",
		"using System.Linq; (int[] a) => a.Select(n => { var m = n * 2; return m; }).Sum()",
		"using System.Linq; (int[][] a) => a.Select(r => { int t = 0; foreach (var n in r) t += n; return t; }).Sum()",
	];

	[Theory]
	[MemberData(nameof(Inputs))]
	public void The_two_carriers_read_one_language(string input)
	{
		Agree(input);
	}

	[Theory]
	[MemberData(nameof(Untyped))]
	public void And_where_a_lambda_says_no_types(string input)
	{
		Agree(input);
	}

	/// <summary>
	/// The form that answers only whether, over kinds and with a context: the same answer and
	/// the same tree as the form that says why, on both carriers, and a refusal that is only
	/// <c>false</c>. The lexer's own refusal, a character no token begins with, is one too.
	/// </summary>
	[Theory]
	[MemberData(nameof(Inputs))]
	[InlineData("(int x) => x +")]
	[InlineData("(in x) => x")]
	[InlineData("(int x) => x # 1")]
	public void The_form_that_answers_only_whether_says_what_the_match_says(string input)
	{
		var match = ExpressionParser.TryParseLambda(input, new ExpressionParser.State { Text = input });

		Assert.Equal(match.IsSuccess, ExpressionParser.TryParseLambda(input, new ExpressionParser.State { Text = input }, out var tape));
		Assert.Equal(match.IsSuccess, ExpressionParser.Immediate.TryParseLambda(input, new ExpressionParser.State { Text = input }, out var immediate));

		Assert.Equal(Shown(match.IsSuccess ? match.Value : null), Shown(tape));
		Assert.Equal(Shown(match.IsSuccess ? match.Value : null), Shown(immediate));
	}

	static void Agree(string input)
	{
		var tape      = ExpressionParser.TryParseLambda(input, new ExpressionParser.State { Text = input });
		var immediate = ExpressionParser.Immediate.TryParseLambda(input, new ExpressionParser.State { Text = input });

		Assert.Equal(tape.IsSuccess, immediate.IsSuccess);

		if (tape.IsSuccess)
			Assert.Equal(Shown(tape.Value), Shown(immediate.Value));
	}

	static string Shown(LambdaExpression? lambda)
	{
		return lambda?.ToString() ?? "<none>";
	}
}
