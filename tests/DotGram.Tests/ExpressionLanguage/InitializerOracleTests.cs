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
	public void Only_an_indexer_writes_the_initializer_out()
	{
		Assert.Equal(
			ExpressionType.MemberInit,
			Body(Usings + "() => new Holder { Items = { 1 }, Inner = { Count = 2 } }").NodeType);
		Assert.Equal(ExpressionType.MemberInit, Body(Usings + "() => new List<int> { }").NodeType);
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
		var source = "#nullable disable\nusing System.Collections.Generic;\nusing DotGram.Tests.ExpressionLanguage;\n" +
			"public static class Asked { public static string Value => Shown.Of(" + expression + "); }";

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
