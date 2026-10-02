using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using DotGram.ExpressionLanguage;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Xunit;

namespace DotGram.Tests.ExpressionLanguage;

/// <summary>
/// Object and collection initializers held against Roslyn compiling the same text: what C#
/// accepts this language accepts and runs to the same result, in the same order, and what C#
/// refuses this language refuses.
/// </summary>
/// <remarks>
/// <para>
/// Every text is an expression C# and this language both read, so one string is asked of both.
/// What it is worth is rendered by <see cref="Shown.Of"/>, which also reports what
/// <see cref="Journal"/> saw happen — the getters, setters and arguments an initializer runs,
/// in the order it runs them — so that an initializer written out as statements is held to
/// C#'s order and not only to its result.
/// </para>
/// <para>
/// Read by all three: the generated parser on the tape, the same grammar on the immediate
/// carrier, and the hand-written parser, which <see cref="Both"/> holds to the first.
/// </para>
/// </remarks>
public sealed class InitializerOracleTests
{
	[Theory]
	// Empty braces: an object initializer that sets nothing, after a type that has members or
	// one that is a collection, with or without the parentheses, and nested.
	[InlineData("new Box { }")]
	[InlineData("new List<int> { }")]
	[InlineData("new List<int>() { }")]
	[InlineData("new Holder { Items = { } }")]
	[InlineData("new Holder { Items = { }, Name = \"a\" }")]
	[InlineData("new Holder { Inner = { }, Items = { 1 } }")]
	// Nested and empty, the member is not even read.
	[InlineData("new Journal { Inner = { } }")]
	// An indexer set among members, at the top and nested.
	[InlineData("new Dictionary<int, int> { [1] = 2 }")]
	[InlineData("new Dictionary<int, int> { [1] = 2, [1] = 3, }")]
	[InlineData("new Dictionary<string, List<int>> { [\"a\"] = { } }")]
	[InlineData("new Journal { Map = { [1] = 2, [3] = 4 } }")]
	[InlineData("new Journal { Numbers = { [1] = 7 } }")]
	[InlineData("new Journal { N = 1, [2] = null }")]
	[InlineData("new Spot { [0] = 5, Y = 1 }")]
	[InlineData("new Spot { X = 2, [1] = 6 }")]
	// The order C# runs them in: an indexer's arguments once, before anything nested; the member
	// or indexer read again for every entry inside its braces; nothing read for empty ones.
	[InlineData("new Journal { [Journal.Arg(1)] = { N = 2, Items = { 3, 4 } }, N = 5 }")]
	[InlineData("new Journal { [Journal.Arg(1)] = { } }")]
	[InlineData("new Journal { Inner = { N = 1, [2] = null } }")]
	// A member's own initializer reads the member once for each entry inside it, indexer or none.
	[InlineData("new Journal { Items = { 1, 2 } }")]
	[InlineData("new Journal { Inner = { N = 1, Next = null } }")]
	[InlineData("new Journal { Inner = { Items = { 3 } }, N = 2 }")]
	[InlineData("new Journal { Inner = { Inner = { N = 4 }, Items = { 5, 6 } } }")]
	// An indexer of two arguments, set and nested.
	[InlineData("new Grid { [1, 2] = 3, [Journal.Arg(4), \"a\"] = { N = 5 } }")]
	// The fields of a struct held in a field, set where they stand.
	[InlineData("new Holder2 { S = { X = 1, Y = 2 } }")]
	[InlineData("new Holder2 { S = { [1] = 7 }, Name = \"s\" }")]
	// Required members set, or a constructor that says it sets them.
	[InlineData("new Needs { A = 1 }")]
	[InlineData("new Needs { B = 2, A = 1 }")]
	[InlineData("new NeedsMet()")]
	[InlineData("new Journal { [Journal.Arg(1)] = Journal.Made(2), [Journal.Arg(3)] = { [Journal.Arg(4)] = Journal.Made(5) } }")]
	[InlineData("new Journal { [Journal.Arg(1)] = { [Journal.Arg(2)] = { [Journal.Arg(3)] = Journal.Made(4) } } }")]
	[InlineData("new Journal { Inner = { [Journal.Arg(1)] = Journal.Made(2), [Journal.Arg(3)] = Journal.Made(4) } }")]
	[InlineData("new Journal { [Journal.Arg(1)] = { Next = Journal.Made(2), [Journal.Arg(3)] = { Next = Journal.Made(4) } } }")]
	[InlineData("new Journal { [Journal.Arg(1)] = { Items = { Journal.Arg(5), Journal.Arg(6) } } }")]
	[InlineData("new Journal { Inner = { Inner = { [Journal.Arg(1)] = Journal.Made(2) } }, [Journal.Arg(3)] = Journal.Made(4) }")]
	public void What_CSharp_accepts_reads_and_runs_alike(string expression)
	{
		var text = Usings + "() => Shown.Of(" + expression + ")";

		var expected  = WhatCSharpSays(expression);
		var generated = Both.Compile<Func<string>>(text, typeof(Box).Assembly)();
		var immediate = Run(Immediately(text));

		Assert.Equal(expected, generated);
		Assert.Equal(expected, immediate);
	}

	[Theory]
	// An implicitly typed array, of the one type all its elements' types convert to; an element
	// with no type of its own, `null`, is converted to that.
	[InlineData("new[] { 1, 2 }")]
	[InlineData("new[] { 1, 2L, }")]
	[InlineData("new[] { (byte)1, 1 }")]
	[InlineData("new[] { 1.5, 2 }")]
	[InlineData("new[] { null, \"a\" }")]
	[InlineData("new[] { new Box(), null }")]
	[InlineData("new[] { new[] { 1 }, new[] { 2, 3 } }")]
	[InlineData("new[] { new List<int> { 1 }, new List<int>() }")]
	[InlineData("new Holder { Name = new[] { \"a\", \"b\" }[1] }")]
	public void An_implicitly_typed_array_is_of_the_type_CSharp_gives_it(string expression)
	{
		What_CSharp_accepts_reads_and_runs_alike(expression);
	}

	[Theory]
	// `with`: a record class copied by its clone, a struct by being a value, and the members set
	// on the copy in the order written; the runtime type of a record is kept through a base.
	[InlineData("new Pair(1, \"a\") with { A = 2 }")]
	[InlineData("new Pair(1, \"a\") with { }")]
	[InlineData("new Pair(1, \"a\") with { A = 2, } with { B = \"c\" }")]
	[InlineData("new Pair(1, \"a\") with { F = 5, B = null }")]
	[InlineData("new Triple(1, \"a\", 3) with { C = 4, A = 0 }")]
	[InlineData("((Pair)new Triple(1, \"a\", 3)) with { A = 9 }")]
	[InlineData("new Spot { X = 1 } with { Y = 2 }")]
	[InlineData("(new Pair(1, \"a\") with { A = 2 }).A + 1")]
	[InlineData("new Pair(Journal.Arg(1), \"a\") with { A = Journal.Arg(2), F = Journal.Arg(3) }")]
	[InlineData("1 switch { _ => new Pair(1, \"a\") } with { A = 7 }")]
	public void A_copy_with_members_set_is_made_as_CSharp_makes_it(string expression)
	{
		What_CSharp_accepts_reads_and_runs_alike(expression);
	}

	[Theory]
	// `new(…)` with its type left out, made as what the place it stands in asks for: a member,
	// an element, a parameter, a nullable value type's underlying type, the other branch of `?:`.
	[InlineData("new Box { Next = new() }")]
	[InlineData("new Box { Next = new() { Last = new() }, }")]
	[InlineData("new Box[] { new(), null }")]
	[InlineData("new[] { new Box(), new() }")]
	[InlineData("new Dictionary<int, Box> { [1] = new() }")]
	[InlineData("new Holder { Name = new('a', 3) }")]
	[InlineData("Shown.Count(new() { 1, 2 })")]
	[InlineData("Shown.Count(new(8))")]
	[InlineData("Shown.Day(new(2020, 1, 2))")]
	[InlineData("true ? new() : new Box()")]
	[InlineData("new()")]
	[InlineData("(Box)new()")]
	[InlineData("new Box { Next = true ? new() : new() }")]
	[InlineData("new Box { Next = false ? new() : null }")]
	[InlineData("Shown.Count(true ? new() { 1 } : null)")]
	[InlineData("Shown.Day(true ? new() : null)")]
	[InlineData("(object)(true ? 1 : \"a\")")]
	// Overload resolution with optional and params parameters.
	[InlineData("Shown.Opt(new())")]
	[InlineData("Shown.Many(new(), new())")]
	[InlineData("Shown.Need(new() { A = 3 })")]
	public void A_new_with_its_type_left_out_is_what_its_place_asks_for(string expression)
	{
		What_CSharp_accepts_reads_and_runs_alike(expression);
	}

	/// <summary>Typed by the variable it initializes and by the delegate's return, and refused with none.</summary>
	[Fact]
	public void A_new_with_its_type_left_out_is_typed_by_a_declaration_and_a_return()
	{
		Assert.Equal(2, Both.Compile<Func<int>>(Usings + "() => { List<int> list = new() { 1, 2 }; return list.Count; }", typeof(Box).Assembly)());
		Assert.Equal([3], Both.Compile<Func<List<int>>>(Usings + "() => new() { 3 }", typeof(Box).Assembly)());
		Assert.Equal([4], Both.Compile<Func<List<int>>>(Usings + "() => { return new() { 4 }; }", typeof(Box).Assembly)());

		var match = Both.TryParse(Usings + "() => { var made = new(); return 1; }", typeof(Box).Assembly);

		Assert.False(match.IsSuccess);
		Assert.Equal("There is no target type for 'new()'.", match.Error);
	}

	/// <summary>Statements, held against Roslyn running the same body as a property getter.</summary>
	[Theory]
	[InlineData("Box b = true ? new() : null; return Shown.Of(b);")]
	[InlineData("List<int> list = new() { 1 }; return Shown.Of(list);")]
	[InlineData("try { throw new(); } catch (System.Exception e) { return e.GetType().Name + e.Message.Length; }")]
	[InlineData("System.Func<Box> f = () => true ? new() : new(); return Shown.Of(f());")]
	[InlineData("Box b = (Box)new(); return Shown.Of(b);")]
	public void Statements_CSharp_accepts_run_alike(string body)
	{
		var text = Usings + "() => { " + body + " }";

		var expected  = WhatCSharpRuns(body);
		var generated = Both.Compile<Func<string>>(text, typeof(Box).Assembly)();
		var immediate = Run(Immediately(text));

		Assert.Equal(expected, generated);
		Assert.Equal(expected, immediate);
	}

	[Theory]
	[InlineData("var x = true ? new() : new(); return \"\";", "CS0173")]
	[InlineData("var x = new(); return \"\";",                "CS8754")]
	[InlineData("var x = true ? 1 : \"a\"; return \"\";",       "CS0173")]
	public void Statements_CSharp_refuses_are_refused(string body, string diagnostic)
	{
		var text = Usings + "() => { " + body + " }";

		Assert.Equal(diagnostic, WhatCSharpRuns(body));
		Assert.False(Both.TryParse(text, typeof(Box).Assembly).IsSuccess, text);
		Assert.False(Refused(text) is null, text);
	}

	/// <summary>
	/// An indexer that returns a reference is one C# assigns through, and an expression tree has no
	/// node that does: refused, saying so, where C# accepts it.
	/// </summary>
	[Fact]
	public void A_ref_returning_indexer_is_refused_saying_why()
	{
		Assert.Equal("RefBox(42)", WhatCSharpSays("new RefBox { [0] = 42 }"));

		var match = Both.TryParse(Usings + "() => new RefBox { [0] = 42 }", typeof(Box).Assembly);

		Assert.False(match.IsSuccess);
		Assert.Contains("returns a reference", match.Error, StringComparison.Ordinal);
	}

	/// <summary>Nullable receivers and targets: whatever C# answers, accepted alike or refused alike.</summary>
	[Theory]
	// Spelled `System.Nullable<Spot>`: this language reads no `T?` in a type.
	[InlineData("((System.Nullable<Spot>)new Spot { X = 1 }) with { }")]
	[InlineData("((System.Nullable<Spot>)null) with { }")]
	[InlineData("new Holder2 { S = new() { X = 3 } }")]
	[InlineData("new Box { Next = (Box)null }")]
	[InlineData("Shown.Day(new())")]
	[InlineData("new System.Nullable<Spot>[] { new(), null }")]
	public void Nullable_receivers_and_targets_answer_as_CSharp_does(string expression)
	{
		var said = WhatCSharpSays(expression);

		if (said.Length == 6 && said.StartsWith("CS", StringComparison.Ordinal))
		{
			Assert.False(Both.TryParse(Usings + "() => Shown.Of(" + expression + ")", typeof(Box).Assembly).IsSuccess, said);

			return;
		}

		What_CSharp_accepts_reads_and_runs_alike(expression);
	}

	/// <summary>`with` is no keyword, as it is none in C#: a variable may still be called so.</summary>
	[Fact]
	public void With_is_still_a_name()
	{
		Assert.Equal(4, Both.Compile<Func<int, int>>("(int with) => with + 1", typeof(Box).Assembly)(3));
		Assert.Equal(3, Both.Compile<Func<int, int>>("(int with) => { int w = with; return w; }", typeof(Box).Assembly)(3));
	}

	[Theory]
	// A member set twice in one initializer (CS1912), plainly, nested, and where an indexer
	// writes the initializer out; an indexer may be set as often as it is written.
	[InlineData("new Box { Next = null, Next = null }",          "CS1912")]
	[InlineData("new Box { Next = { }, Next = { } }",            "CS1912")]
	[InlineData("new Box { Next = { Next = null, Next = null } }", "CS1912")]
	[InlineData("new Journal { [0] = null, N = 1, N = 2 }",      "CS1912")]
	// An indexer after an element and an element after an indexer (CS0747).
	[InlineData("new List<int> { 1, [0] = 2 }",                 "CS0747")]
	[InlineData("new List<int> { [0] = 1, 2 }",                 "CS0747")]
	// Members of a property of a value type, which would be set on a copy (CS1918).
	[InlineData("new Journal { Place = { } }",                  "CS1918")]
	[InlineData("new Journal { Place = { X = 1 } }",            "CS1918")]
	[InlineData("new Spot { [0] = { } }",                       "CS1918")]
	// A member the type does not have, empty braces or not.
	[InlineData("new Box { Nope = { } }",                       "CS0117")]
	// An implicitly typed array with no best type (CS0826), or one an element does not convert to.
	[InlineData("new[] { }",                                    "CS0826")]
	[InlineData("new[] { null }",                               "CS0826")]
	[InlineData("new[] { 1u, 1 }",                              "CS0826")]
	[InlineData("new[] { 1, \"a\" }",                           "CS0826")]
	[InlineData("new[] { 1, null }",                            "CS0037")]
	// A copy of what is neither a record nor a struct (CS8858), of a member set twice, of one that
	// cannot be written or is not there, and with what a copy does not take.
	[InlineData("new Box() with { }",                           "CS8858")]
	[InlineData("((object)new Pair(1, \"a\")) with { }",         "CS8858")]
	[InlineData("new Pair(1, \"a\") with { A = 2, A = 3 }",      "CS1912")]
	[InlineData("new Pair(1, \"a\") with { G = 1 }",             "CS0191")]
	[InlineData("new Pair(1, \"a\") with { Nope = 1 }",          "CS0117")]
	[InlineData("new Pair(1, \"a\") with { [0] = 1 }",           "CS0131")]
	[InlineData("new Pair(1, \"a\") with { L = { 1 } }",         "CS1525")]
	[InlineData("new Pair(1, \"a\") with { L = { } }",           "CS1525")]
	// A `new(…)` with its type left out: two places it converts to equally (CS0121), a type with no
	// such constructor (CS1729) or no such member (CS0117), what cannot be made so, and no place.
	[InlineData("Shown.Pick(new())",                            "CS0121")]
	[InlineData("Shown.Many2(new())",                           "CS0121")]
	[InlineData("new Box { Next = new(1) }",                    "CS1729")]
	[InlineData("Shown.Count(new() { Next = null })",           "CS0117")]
	[InlineData("Shown.Seq(new())",                             "CS0144")]
	[InlineData("new Box[][] { new() }",                        "CS8752")]
	[InlineData("new[] { new() }",                              "CS0826")]
	[InlineData("new().ToString()",                             "CS8754")]
	[InlineData("1 + new()",                                    "CS8310")]
	[InlineData("\"a\" + new()",                                "CS8310")]
	[InlineData("new Box() == new()",                           "CS8310")]
	[InlineData("-new()",                                       "CS8754")]
	// A collection initializer of a type that is not enumerable (CS1922), whatever `Add` it has.
	[InlineData("new Host { [0] = 0, Items = { 1 } }",          "CS1922")]
	[InlineData("new Host { Items = { 1 } }",                   "CS1922")]
	[InlineData("new Adder { 1 }",                              "CS1922")]
	// The accessor used has to be one the text may call: a private init or set, or none at all
	// (both CS0200), a read-only field (CS0191).
	[InlineData("new Guarded() with { P = 1 }",                 "CS0200")]
	[InlineData("new Guarded { P = 1 }",                        "CS0200")]
	[InlineData("new Guarded { [0] = 1 }",                      "CS0200")]
	[InlineData("new Guarded { Q = 1 }",                        "CS0200")]
	[InlineData("new Pair(1, \"a\") { G = 1 }",                 "CS0191")]
	// A required member left unset (CS9035).
	[InlineData("new Needs()",                                  "CS9035")]
	[InlineData("new Needs { }",                                "CS9035")]
	[InlineData("new Needs { B = 1 }",                          "CS9035")]
	[InlineData("Shown.Need(new())",                            "CS9035")]
	public void What_CSharp_refuses_is_refused(string expression, string diagnostic)
	{
		var text = Usings + "() => Shown.Of(" + expression + ")";

		Assert.Equal(diagnostic, WhatCSharpSays(expression));
		Assert.False(Both.TryParse(text, typeof(Box).Assembly).IsSuccess, text);
		Assert.False(Refused(text) is null, text);
	}

	/// <summary>A member set twice says so, in C#'s words.</summary>
	[Fact]
	public void A_member_set_twice_is_refused_in_CSharp_words()
	{
		var match = Both.TryParse(Usings + "() => new Box { Next = null, Last = null, Next = null }", typeof(Box).Assembly);

		Assert.Equal("Duplicate initialization of member 'Next'.", match.Error);
	}

	/// <summary>An initializer that sets an indexer is written out as statements; one that does not is left as C# builds it.</summary>
	[Fact]
	public void Only_values_keep_the_member_initializer()
	{
		Assert.Equal(ExpressionType.MemberInit, Body(Usings + "() => new Box { Next = null, Last = null }").NodeType);
		Assert.Equal(ExpressionType.MemberInit, Body(Usings + "() => new Holder { Items = { }, Name = \"a\" }").NodeType);
		Assert.Equal(ExpressionType.MemberInit, Body(Usings + "() => new List<int> { }").NodeType);
		Assert.Equal(ExpressionType.ListInit, Body(Usings + "() => new List<int> { 1 }").NodeType);
		Assert.Equal(ExpressionType.Block, Body(Usings + "() => new Holder { Items = { 1 } }").NodeType);
		Assert.Equal(ExpressionType.Block, Body(Usings + "() => new Holder { Inner = { Count = 2 } }").NodeType);
		Assert.Equal(ExpressionType.Block, Body(Usings + "() => new Dictionary<int, int> { [1] = 2 }").NodeType);
		Assert.Equal(ExpressionType.Block, Body(Usings + "() => new Journal { Inner = { [1] = null } }").NodeType);
	}

	const string Usings = "using System.Collections.Generic; using DotGram.Tests.ExpressionLanguage; ";

	static Expression Body(string text)
	{
		return Both.Parse(text, typeof(Box).Assembly).Body;
	}

	/// <summary>The text read on the immediate carrier, where constructions run as they are read.</summary>
	static LambdaExpression Immediately(string text)
	{
		var match = ExpressionParser.Immediate.TryParseLambda(
			text, new ExpressionParser.State(typeof(Box).Assembly) { Text = text });

		Assert.True(match.IsSuccess, match.Error);

		return match.Value!;
	}

	/// <summary>What the immediate carrier makes of a text C# refuses: why, or null where it read it.</summary>
	static string? Refused(string text)
	{
		try
		{
			var match = ExpressionParser.Immediate.TryParseLambda(
				text, new ExpressionParser.State(typeof(Box).Assembly) { Text = text });

			return match.IsSuccess ? null : match.Error;
		}
		catch (Exception thrown) when (thrown is FormatException or InvalidOperationException or ArgumentException)
		{
			return thrown.Message;
		}
	}

	static string Run(LambdaExpression lambda)
	{
		return ((Func<string>)lambda.Compile())();
	}

	/// <summary>What Roslyn makes of the same expression: what it is worth, or the first error's id.</summary>
	static string WhatCSharpSays(string expression)
	{
		return WhatCSharpRuns("return Shown.Of(" + expression + ");");
	}

	/// <summary>What Roslyn makes of a body of statements: what it returns, or the first error's id.</summary>
	static string WhatCSharpRuns(string body)
	{
		var source = "#nullable disable\nusing System.Collections.Generic;\nusing DotGram.Tests.ExpressionLanguage;\n" +
			"public static class Asked { public static string Value { get { " + body + " } } }";

		var compilation = CSharpCompilation.Create(
			"InitializerOracle",
			[CSharpSyntaxTree.ParseText(source)],
			References(),
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

		var refused = compilation.GetDiagnostics()
			.FirstOrDefault(static one => one.Severity == DiagnosticSeverity.Error);

		if (refused is not null)
			return refused.Id;

		using var stream = new System.IO.MemoryStream();

		Assert.True(compilation.Emit(stream).Success);

		return (string)System.Reflection.Assembly.Load(stream.ToArray())
			.GetType("Asked")!
			.GetProperty("Value")!
			.GetValue(null)!;
	}

	static ImmutableArray<MetadataReference> References()
	{
		return
		[
			.. AppDomain.CurrentDomain.GetAssemblies()
				.Where(static one => !one.IsDynamic && one.Location.Length > 0)
				.Select(static one => (MetadataReference)MetadataReference.CreateFromFile(one.Location)),
		];
	}
}

/// <summary>A value rendered the same way whoever built it, and what <see cref="Journal"/> saw on the way.</summary>
public static class Shown
{
	public static string Count(List<int> list)
	{
		return list.Count + " of " + list.Capacity;
	}

	public static string Pick(Box box)
	{
		return "box";
	}

	public static string Pick(List<int> list)
	{
		return "list";
	}

	public static string Day(DateTime? day)
	{
		return day?.Day.ToString() ?? "none";
	}

	public static string Seq(IEnumerable<int> items)
	{
		return "seq";
	}

	public static string Opt(Box box)
	{
		return "one";
	}

	public static string Opt(Box box, int more = 1)
	{
		return "optional " + more;
	}

	public static string Many(params Box[] boxes)
	{
		return "many " + boxes.Length;
	}

	public static string Many2(Box box)
	{
		return "one";
	}

	public static string Many2(params Box[] boxes)
	{
		return "many " + boxes.Length;
	}

	public static string Need(Needs needs)
	{
		return "needs " + needs.A;
	}

	public static string Of(object? value)
	{
		var seen = Journal.Taken();
		var text = Rendered(value);

		return seen.Length == 0 ? text : seen + " | " + text;
	}

	static string Rendered(object? value)
	{
		switch (value)
		{
			case null:
				return "null";
			case string text:
				return text;
			case Box box:
				return "Box(" + Rendered(box.Next) + ", " + Rendered(box.Last) + ")";
			case Holder holder:
				return "Holder(" + holder.Name + ", " + holder.Inner.Count + ", " + Rendered(holder.Items) + ")";
			case Journal journal:
				return "Journal(" + journal.Value + ", " + Rendered(journal.Values) + ", " + Rendered(journal.Entries) + ", " +
					Rendered(journal.Numbers) + ")";
			case Array array:
			{
				var built = new StringBuilder(array.GetType().Name).Append(' ').Append('[');
				var first = true;

				foreach (var item in array)
				{
					built.Append(first ? "" : ", ").Append(Rendered(item));
					first = false;
				}

				return built.Append(']').ToString();
			}
			case Spot spot:
				return "Spot(" + spot.X + ", " + spot.Y + ")";
			case IDictionary map:
			{
				var built = new StringBuilder("{");

				foreach (DictionaryEntry entry in map)
					built.Append(built.Length > 1 ? ", " : "").Append(Rendered(entry.Key)).Append(": ").Append(Rendered(entry.Value));

				return built.Append('}').ToString();
			}
			case IEnumerable items:
			{
				var built = new StringBuilder("[");

				foreach (var item in items)
					built.Append(built.Length > 1 ? ", " : "").Append(Rendered(item));

				return built.Append(']').ToString();
			}
			default:
				return value.ToString() ?? "";
		}
	}
}

/// <summary>A type that writes down every getter, setter and argument an initializer runs on it.</summary>
/// <remarks>
/// The record is per thread: a test reads and runs its text on one thread, and others may be
/// running beside it.
/// </remarks>
public sealed class Journal
{
	[ThreadStatic]
	static List<string>? t_log;

	static List<string> Log => t_log ??= [];

	readonly List<int> _items = [];

	readonly Dictionary<int, int> _map = [];

	public int[] Numbers = new int[3];

	public int Value;

	public Spot Place { get; set; }

	public int N
	{
		get
		{
			return Value;
		}
		set
		{
			Log.Add("N = " + value);
			Value = value;
		}
	}

	public List<int> Items
	{
		get
		{
			Log.Add("Items");

			return _items;
		}
	}

	public List<int> Values => _items;

	public Dictionary<int, int> Entries => _map;

	public Dictionary<int, int> Map
	{
		get
		{
			Log.Add("Map");

			return _map;
		}
	}

	public Journal? Next
	{
		get
		{
			Log.Add("get Next");

			return null;
		}
		set
		{
			Log.Add("set Next");
		}
	}

	public Journal Inner
	{
		get
		{
			Log.Add("Inner");

			return this;
		}
	}

	public Journal? this[int at]
	{
		get
		{
			Log.Add("get [" + at + "]");

			return this;
		}
		set
		{
			Log.Add("set [" + at + "]");
		}
	}

	public static int Arg(int value)
	{
		Log.Add("arg " + value);

		return value;
	}

	public static Journal? Made(int value)
	{
		Log.Add("made " + value);

		return null;
	}

	/// <summary>What was written down since the last time, which is then forgotten.</summary>
	public static string Taken()
	{
		var taken = string.Join(", ", Log);

		Log.Clear();

		return taken;
	}
}

/// <summary>An indexer of two arguments, of either kind, written down in the journal.</summary>
public sealed class Grid
{
	readonly List<string> _set = [];

	public int this[int x, int y]
	{
		get
		{
			return 0;
		}
		set
		{
			_set.Add(x + "," + y + "=" + value);
		}
	}

	public Journal this[int x, string y]
	{
		get
		{
			Journal.Arg(x);

			return new Journal();
		}
	}

	public override string ToString()
	{
		return "Grid(" + string.Join(" ", _set) + ")";
	}
}

/// <summary>A struct held in a field, whose fields an initializer sets where they stand.</summary>
public sealed class Holder2
{
	public Spot S;

	public string Name = "";

	public override string ToString()
	{
		return "Holder2(" + S.X + ", " + S.Y + ", " + Name + ")";
	}
}

/// <summary>A type with an `Add` and no <c>IEnumerable</c>, which no collection initializer may use.</summary>
public sealed class Adder
{
	public void Add(int value)
	{
	}
}

/// <summary>Holds an <see cref="Adder"/>, and has an indexer so that an initializer of it is written out.</summary>
public sealed class Host
{
	public Adder Items { get; } = new();

	public int this[int at]
	{
		get
		{
			return at;
		}
		set
		{
		}
	}
}

/// <summary>Accessors the text may not call: a private init, a private set, and none.</summary>
public sealed record Guarded
{
	public int P { get; private init; }

	public int Q { get; }

	public int this[int at]
	{
		get
		{
			return at;
		}
		private set
		{
		}
	}
}

/// <summary>An indexer that returns a reference, which C# assigns through.</summary>
public sealed class RefBox
{
	readonly int[] _held = new int[2];

	public ref int this[int at] => ref _held[at];

	public override string ToString()
	{
		return "RefBox(" + _held[0] + ")";
	}
}

/// <summary>A required member, which every object initializer has to set.</summary>
public class Needs
{
	public required int A { get; set; }

	public int B { get; set; }

	public override string ToString()
	{
		return "Needs(" + A + ", " + B + ")";
	}
}

/// <summary>The same, made by a constructor that says it sets them.</summary>
public sealed class NeedsMet : Needs
{
	[System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
	public NeedsMet()
	{
		A = 9;
	}
}

/// <summary>A record class, which `with` copies through the clone the compiler writes for it.</summary>
public record Pair(int A, string? B)
{
	public int F;

	public readonly int G;

	public List<int> L { get; init; } = [];
}

/// <summary>One derived from it, whose clone is the one a copy through the base still calls.</summary>
public record Triple(int A, string? B, int C) : Pair(A, B);

/// <summary>A value type with an indexer, which an initializer sets on its own copy.</summary>
public struct Spot
{
	public int X;

	public int Y;

	public int this[int at]
	{
		readonly get
		{
			return at == 0 ? X : Y;
		}
		set
		{
			if (at == 0)
				X = value;
			else
				Y = value;
		}
	}
}
