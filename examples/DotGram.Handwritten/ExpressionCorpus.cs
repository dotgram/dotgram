using System;
using System.Linq.Expressions;
using System.Reflection;

using DotGram.ExpressionLanguage;

namespace DotGram.Handwritten;

/// <summary>
/// The shapes of the expression language <see cref="HandExpression"/> is held to the grammar on.
/// </summary>
/// <remarks>
/// One list, read by the conformance test in the ordinary suite and by the benchmark before it
/// times anything: a shape that told the two apart once stays here, so that it cannot come back
/// unnoticed on either side. Every part of the language is written here at least once, and so
/// are the refusals, since a refusal is an answer and has to be the same answer.
/// </remarks>
public static class ExpressionCorpus
{
	public static readonly string[] Shapes =
	[
		// The floor, the ladder, and every level of it.
		"(int x) => x",
		"(int x) => x + 1",
		"(int x) => x * x - 1",
		"(int x) => x * (x - 1)",
		"(int x, int y) => (x + y) * 3 - x / 5",
		"(int x) => x % 3",
		"(int x) => x < 1 || x > 9",
		"(int x) => x > 0 && x < 9",
		"(int x) => x >= 0 && x <= 9",
		"(int x) => x == 1 || x != 2",
		"(int x) => (x & 3) | (x ^ 1)",
		"(int x) => x << 2",
		"(int x) => x >> 2",
		"(int x) => x << 2 >> 1",
		"(int x) => -x",
		"(int x) => +x",
		"(int x) => ~x",
		"(bool b) => !b",
		"(int x) => - -x",
		"(int x) => 1 - -x",

		// Association and precedence, on every pair of levels that could be confused.
		"(int a, int b, int c) => a - b + c",
		"(int a, int b, int c) => a + b * c",
		"(int a, int b, int c) => a * b / c",
		"(int a, int b, int c) => a / b * c",
		"(int a, int b, int c) => a << b >> c",
		"(bool a, bool b, bool c) => a || b && c",
		"(bool a, bool b, bool c) => a && b || c",
		"(int a, int b) => a < b == true",
		"(int a, int b, int c) => a & b | c",
		"(int a, int b, int c) => a | b ^ c",

		// Right associativity, and the two that have it.
		"(int x) => x > 1 ? x : -x",
		"(int x) => x > 1 ? 1 : x > 0 ? 2 : 3",
		"(string s) => s ?? \"none\"",
		"(string s, string t) => s ?? t ?? \"none\"",

		// Assignment, plain and compound, to a name, a member and an element.
		"(int x) => { x = 1; x }",
		"(int x) => { x += 1; x -= 2; x *= 3; x /= 4; x %= 5; x }",
		"(int x) => { x &= 1; x |= 2; x ^= 3; x <<= 1; x >>= 1; x }",
		"(int[] a) => { a[0] = 1; a[0] }",

		// Members, calls, indices, and the chains they make.
		"(string s) => s.Length",
		"(string s) => s.Trim()",
		"(string s) => s.Trim().Length",
		"(string s) => s.Substring(1, 2)",
		"(int[] a) => a.Length + a[0]",
		"(int[][] a) => a[0][1]",
		"using System; (int x) => Math.Max(x, 1)",
		"(int x) => System.Math.Max(x, 1)",
		"using System; (int x) => Math.PI > x",
		"(string s) => string.Concat(s, s)",

		// The types, which are what a cast and a declaration are told apart by.
		"(int x) => (long)x",
		"(double d) => (int)d + 1",
		"(int x) => (object)x",
		"(object o) => o as string",
		"(object o) => o is string",
		"(int x) => new int[3]",
		"(int x) => new int[] { x, 1 }",
		"(int x) => new int[] { }",
		"(int x) => new string('a', x)",

		// Statements, and the scopes they open.
		"(int x) => { int y = x + 1; return y * y; }",
		"(int x) => { int y = 1; { int z = 2; x += z; } x + y }",
		"(int x) => { if (x > 0) return 1; return 0; }",
		"(int x) => { if (x > 0) x = 1; else x = 2; x }",
		"(int x) => if (x > 0) 1 else 2",
		"(int x) => { int y = if (x > 0) 1 else 2; y }",
		"(int x) => { while (x < 10) { x += 1; } return x; }",
		"(int x) => { do { x += 1; } while (x < 10); x }",
		"(int n) => { int sum = 0; for (int i = 0; i < n; i++) { sum += i; } sum }",
		"(int n) => { int sum = 0; for (int i = 0; i < n; i++) { if (i % 2 == 0) continue; sum += i; } sum }",
		"(int n) => { while (true) { if (n > 3) break; n += 1; } n }",
		"(int n) => { switch (n) { case 1: n = 10; break; default: n = 0; break; } n }",
		"(int n) => { switch (n) { case 1 or 2: n = 10; break; case 3: case 4: n = 30; break; } n }",
		"(int n, int limit) => n switch { 1 or 2 => 10, limit => 20, _ => -1 }",

		// `foreach`, which the API has no node for: over an array, over a list through the
		// enumerator it declares, over a string, with the element type written and with `var`,
		// and the jumps that name the loop it makes.
		"(int[] a) => { int sum = 0; foreach (int n in a) sum += n; sum }",
		"(int[] a) => { int sum = 0; foreach (var n in a) { sum += n; } sum }",
		"(string s) => { int n = 0; foreach (char c in s) n += 1; n }",
		"using System.Collections.Generic; (List<int> l) => { int sum = 0; foreach (var n in l) sum += n; sum }",
		"(int[] a) => { int sum = 0; foreach (var n in a) { if (n < 0) continue; if (n > 3) break; sum += n; } sum }",

		// The marks, which change what is built and nothing about what is read.
		"(int x) => checked(x + 1)",
		"(int x) => unchecked(x * 2)",
		"(int x) => checked(x + unchecked(x * 2))",
		"(int x) => checked(x + 1) + x",

		// A type where a value is wanted, what a type defaults to, and a name as written.
		"(int x) => typeof(int)",
		"(int x) => typeof(int[])",
		"(int x) => default(int) + x",
		"(int x) => nameof(x)",

		// LINQ: an extension method through a `using`, its type arguments inferred, taking a
		// lambda written where a value is wanted.
		"using System.Collections.Generic; using System.Linq; (List<int> l) => l.Where((int n) => n > 1).Select((int n) => n * 2).ToArray()",

		// A lambda inside an expression: one that closes over what is around it, and two
		// beside each other whose parameters are each their own.
		"(int x) => { var f = (int y) => y + x; f(1) }",
		"(int x) => { var f = (int y) => y + 1; var g = (int y) => y * 2; f(1) + g(2) }",

		// And a `return` inside one, which leaves that lambda and not the one around it:
		// alone, and beside a `return` of the outer lambda's own, which is two labels.
		"(int x) => { var f = (int y) => { return y + 1; }; f(x) }",
		"(int x) => { var f = (int y) => { return y * 2; }; return f(x) + 1; }",

		// A guard and how far it reaches: one step, a whole chain protected by it, an index,
		// what a nullable holds, and the ternary whose number keeps its point.
		"(string s) => s?.Length",
		"(string s) => s?.Trim().Length",
		"(int[] a) => a?[0]",
		"(string s) => (s?.Length)?.ToString()",
		"(int x) => x > 0 ? .5 : 1.5",

		// A declaration whose type is its initializer's, the same inside a `for`, and the
		// word itself as a name — which is what makes `var` contextual rather than reserved.
		"(int x) => { var doubled = x * 2; doubled }",
		"(double x) => { var half = x / 2.0; return half; }",
		"(int n) => { int sum = 0; for (var i = 0; i < n; i++) { sum += i; } sum }",
		"(int var) => { var += 2; var }",
		"(int x) => { var nothing = null; x }",

		// The constants, every form of them.
		"(int x) => 1",
		"(int x) => 2147483648",
		"(int x) => 1u",
		"(int x) => 1L",
		"(int x) => 1UL",
		"(int x) => 1_000_000",
		"(int x) => 0x1F",
		"(int x) => 0xFFu",
		"(int x) => 0b1010",
		"(int x) => 1.5",
		"(int x) => .5",
		"(int x) => 1e3",
		"(int x) => 1.5e-3",
		"(int x) => 1.5f",
		"(int x) => 1.5d",
		"(int x) => 1.5m",
		"(int x) => (byte)x",
		"() => { sbyte s = -1; s }",
		"(int x) => 1m",
		"(bool b) => true",
		"(bool b) => false",
		"(string s) => null",
		"(string s) => \"text\"",
		"(string s) => \"a\\tb\\n\"",
		"(string s) => \"\\u0041\"",
		"(string s) => @\"a\"\"b\"",
		"(char c) => 'a'",
		"(char c) => '\\n'",

		// What is left of the language: the constructs a person writes rarely and a
		// parser has to read all the same.
		"using System; (int x) => { try { x += 1; } catch (Exception e) { x = 0; } x }",
		"using System; (int x) => { try { x += 1; } catch (Exception e) { x = 0; } finally { x += 1; } x }",
		"(int x) => { try { x += 1; } finally { x += 1; } x }",
		"using System; (int x) => { if (x < 0) throw new Exception(\"no\"); x }",
		"(int x) => new System.Collections.Generic.List<int>()",
		"(int x) => new System.Collections.Generic.List<int> { x, 1 }",
		"(int x) => new System.Collections.Generic.Dictionary<int, string> { { x, \"a\" } }",
		"(int x) => new System.Text.StringBuilder(16).Length",
		"(int x) => System.Convert.ToString(x)",

		// A `using`, two of them, and a nested type reached through one.
		"using System.Text; (int x) => new StringBuilder(16).Length",
		"using System; using System.Text; (int x) => Math.Max(new StringBuilder(x).Length, 1)",
		"using System; (Environment.SpecialFolder f) => f",
		"(int x) => { int[] a = new int[2]; a[0] = x; a[0] }",
		"(int x) => ++x",
		"(int x) => --x",
		"(int x) => x++",
		"(int x) => x--",

		// And the refusals: a refusal is an answer, and has to be the same answer.
		"(int x) => x *",
		"(int x) =>",
		"(int x) => { x += 1;",
		"(int x) => y",
		"(int x) => x.NoSuchMember",
		"using System.Nowhere; (int x) => x",
		"(int x) => Math.Max(x, 1)",
		"(int x) => (",
		"(int x) => )",
		"int x => x",
		"(int x) x",
		"",
		"(int x) => x > > 1",
		"(int x) => x | | 1",

		// A keyword is no name wherever a name is given.
		"(int if) => 1",
		"(int x) => { int return = 1; x }",
		"(string s) => s.int",
		"(int x) => nameof(if)",
		"(int iff) => { int returned = iff; returned }",

		// Escapes, and the characters that are not.
		"(int x) => \"\\x41g\"",
		"(int x) => \"\\x41424\"",
		"(int x) => \"\\U0001F600\"",
		"(int x) => \"\\U0011FFFF\"",
		"(int x) => '\\u0041'",
		"(int x) => '\\x41'",
		"(int x) => '\\U0001F600'",
		"(int x) => \"\\q\"",
		"(int x) => \"\\u41\"",
		"(int x) => \"\\x\"",
		"(int x) => 'ab'",
		"(int x) => ''",
		"(int x) => \"abc",
		"(int x) => @\"a\"\"",

		// Numbers where a separator or a base has nothing after it.
		"(int x) => 1_",
		"(int x) => 1__0",
		"(int x) => 0x",
		"(int x) => 0x_1F",
		"(int x) => 0b",
		"(int x) => 0b102",
		"(int x) => 1.5e",
		"(int x) => 1e+5",
		"(int x) => 1..5",
		"(int x) => 0xFFFFFFFFFFFFFFFFFF",

		// Interpolated strings: holes, formats, alignments, braces, the verbatim spellings, and
		// strings inside holes.
		"(int x) => $\"a{x}b\"",
		"(int x) => $\"{x,5:D3}\"",
		"(int x) => $\"{x:}\"",
		"(int x) => $\"{{{x}}}\"",
		"(int x) => $\"\"",
		"(int x) => $\"a\\tb{x}\"",
		"(int x) => $\"a\\{x}\"",
		"(int x) => $@\"a\"\"{x}\\\"",
		"(int x) => @$\"{x}\\n\"",
		"(string s) => $\"{s.Length:x}\"",
		"(int x) => $\"{(x > 0 ? 1 : 2)}\"",
		"(int x) => $\"{x > 0 ? 1 : 2}\"",
		"(int x) => $\"{$\"{x}\"}\"",
		"(int x) => $\"{\"a\" + x}\"",
		"(int x) => $\"{'a'}{@\"b\"}\"",
		"(int x) => $\"{new int[] { x }[0]}\"",
		"(int x) => $\"{x\"",
		"(int x) => $\"a}\"",
		"(int x) => $\"{y}\"",
		"(int x) => $\"{x +}\"",
		"(int x) => $\"{x, y}\"",
		"(int x) => $$\"{x}\"",
		"(int x) => $\"a{x}\" + \"b\"",

		// Raw strings, of every number of quotes and dollars.
		"(int x) => \"\"\"abc\"\"\"",
		"(int x) => \"\"\"a\"b\"\"c\"\"\"",
		"(int x) => \"\"\"\"a\"\"\"b\"\"\"\"",
		"(int x) => \"\"\"\"\"a\"\"\"\"\"",
		"(int x) => \"\"\"\"\"\"a\"\"\"\"\"\"",
		"(int x) => \"\"\"\n    a\n      b\n    \"\"\"",
		"(int x) => \"\"\"\n  a\n \"\"\"",
		"(int x) => \"\"\"a\"\"\"\"",
		"(int x) => \"\"\"abc",
		"(int x) => $\"\"\"{x}\"\"\"",
		"(int x) => $\"\"\"\"{x}\"\"\"\"",
		"(int x) => $$\"\"\"{x} {{x}}\"\"\"",
		"(int x) => $$\"\"\"{{{x}}}\"\"\"",
		"(int x) => $$$\"\"\"{{{x}}} {x}\"\"\"",
		"(int x) => $\"\"\"\"\"\"{x}\"\"\"\"\"\"",
		"(int x) => $\"\"\"a}\"\"\"",
		"(int x) => $\"\"\"{x\"\"\"",

		// Lambdas that say no types: taken from the overload they are handed to, their bodies
		// read before the types are known and again after.
		"using System.Linq; (int[] a) => a.Select(n => n * 2).Sum()",
		"using System.Linq; (int[] a) => a.Aggregate((s, n) => s + n)",
		"using System.Linq; (int[] a) => a.Where(n => n > 1).Count()",
		"using System.Linq; (int[] a) => a.Select(n => { var m = n * 2; return m; }).Sum()",
		"using System.Linq; (int[] a) => a.Select(n => { int m = n * 2; return m; }).Sum()",
		"using System.Linq; (int[][] a) => a.Select(r => { int t = 0; foreach (var n in r) t += n; return t; }).Sum()",
		"using System.Linq; (int[] a) => a.Select(n => $\"{n}\").First()",
		"using System.Linq; (int[] a) => a.Select(n => a.Where(m => m > n).Count()).Sum()",
		"(int x) => { var f = n => n; return x; }",
		"using System.Linq; (int[] a) => a.Select(n => y).Sum()",
		"using System.Linq; (int[] a) => a.Select((n) => n).Sum()",
		"using System.Linq; (int[] a) => a.Select(n =>).Sum()",
		"(int x) => (x) => x",
		"x => x",
	];

	/// <summary>What a reading made of a text, as one string that two readings are compared by.</summary>
	/// <remarks>
	/// The tree as the API prints it for a debugger, which says every type, conversion, label
	/// and variable — what <c>ToString</c> leaves out. A refusal is how it was refused and
	/// where, with the furthest name the reading refused, which is what the language's own
	/// <c>TryParse</c> reports; and a text refused by the API is the type of what it threw.
	/// The wording of a message is no part of the answer.
	/// </remarks>
	internal static string Answer(
		string text, Func<string, ExpressionParser.State, ExpressionParser.Match<LambdaExpression>> read, ExpressionParser.State state)
	{
		try
		{
			var match = read(text, state);

			return match.IsSuccess
				? (string)DebugView.GetValue(match.Value)!
				: $"refused ({match.Outcome}) at {match.Position}, a name refused at {state.RefusedAt}: {state.Refused()}";
		}
		catch (Exception thrown) when (thrown is FormatException or InvalidOperationException or OverflowException ||
			thrown is ArgumentException and not ArgumentNullException)
		{
			return "threw " + thrown.GetType().Name;
		}
	}

	static readonly PropertyInfo DebugView =
		typeof(Expression).GetProperty("DebugView", BindingFlags.Instance | BindingFlags.NonPublic)!;
}
