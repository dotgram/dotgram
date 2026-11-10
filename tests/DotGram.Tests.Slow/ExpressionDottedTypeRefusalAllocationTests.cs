using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using DotGram.ExpressionLanguage;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A dotted name that names no type, refused, allocates what its parts need, not their square.
/// </summary>
/// <remarks>
/// <para>
/// <c>NamedType</c> reads a word and then as many dotted parts as there are, and asks the reading's
/// context whether the name they make is a type. Where it is not, the repetition gives a part back and
/// the guard asks again about the shorter name, down to the head alone. A parameter typed
/// <c>MissingRoot.A.A.…</c>, whose root is nowhere, is refused only once every one of those prefixes
/// has been asked about.
/// </para>
/// <para>
/// <b>The gate is bytes an element, at four doubling sizes</b>, as in
/// <see cref="WebListRefusalAllocationTests"/>: a reading that built the parts and the name again for
/// every shorter prefix doubles the bytes a part at every doubling of the parts. Measured on the first
/// reading of names no cache has seen, in a scope made for it: what a hostile text costs is what a stream of
/// texts of its own costs, and the name caches answer a text read again for nothing. An 800-part generic
/// name allocated 58 KB read again and 297 MB read first.
/// </para>
/// </remarks>
[Collection(typeof(Alone))]
public sealed class ExpressionDottedTypeRefusalAllocationTests
{
	/// <summary>Doubling sizes, in dotted parts after the head.</summary>
	static readonly int[] Sizes = [100, 200, 400, 800];

	[Fact]
	public void A_parameter_typed_by_an_unresolved_dotted_name_still_refuses_at_every_size()
	{
		foreach (var size in Sizes)
			Assert.False(ExpressionParser.TryParse(Parameter(size)).IsSuccess, $"a type of {size} parts was read.");
	}

	/// <summary>A type whose name has more parts than a name may have before the scope is asked how deep its types go.</summary>
	[Fact]
	public void A_type_with_a_long_name_is_still_named_by_it()
	{
		const string type = "DotGram.Tests.Deep.N1.N2.N3.C1.C2.C3.C4.C5";

		var match = ExpressionParser.TryParse("(" + type + " x) => x");

		Assert.True(match.IsSuccess, match.Error);
		Assert.Equal(typeof(Deep.N1.N2.N3.C1.C2.C3.C4.C5), match.Value.Parameters[0].Type);

		match = ExpressionParser.TryParse("() => " + type + ".Value");

		Assert.True(match.IsSuccess, match.Error);
		Assert.Equal(typeof(int), match.Value.Body.Type);
	}

	/// <summary>
	/// A generic name asks nothing while it is read, so one longer than any type is read to its
	/// arguments and refused where the type is looked for, naming the whole of it.
	/// </summary>
	[Fact]
	public void A_long_generic_name_is_still_refused_where_it_is_built()
	{
		var name  = "MissingRoot" + string.Concat(Enumerable.Repeat(".A", 100));
		var match = ExpressionParser.TryParse("(" + name + "<int> x) => x");

		Assert.False(match.IsSuccess);
		Assert.Equal($"The type or namespace name '{name}<>' could not be found.", match.Error);
	}

	[Fact]
	public void A_parameter_typed_by_an_unresolved_dotted_name_allocates_about_as_many_bytes_a_part_at_every_size()
	{
		AssertLinear("a parameter's type", static (parts, salt) => "(Root" + salt + Parts(parts, salt) + " x) => x");
	}

	/// <summary>
	/// A generic name asks nothing while it is read, and was looked up whole where it was built: every prefix
	/// of it asked of every assembly, 3.2 GB for 1,600 parts. Longer than any type, it is looked up no more.
	/// </summary>
	[Fact]
	public void A_generic_name_longer_than_any_type_allocates_about_as_many_bytes_a_part_at_every_size()
	{
		AssertLinear("a generic parameter type", static (parts, salt) => "(Root" + salt + Parts(parts, salt) + "<int> x) => x");
		AssertLinear("a generic type constructed", static (parts, salt) => "() => new Root" + salt + Parts(parts, salt) + "<int>()");
		AssertLinear("a generic type's member", static (parts, salt) => "() => Root" + salt + Parts(parts, salt) + "<int>.Value");
		AssertLinear("a generic alias", static (parts, salt) => "using X = Root" + salt + Parts(parts, salt) + "<int>; (X x) => x");
	}

	/// <summary>
	/// What a scope pays to know how deep its types go, which a name of more than a few parts asks: each
	/// assembly counted once whatever scopes it is in, so a scope made afresh pays a lookup an assembly
	/// and not a walk of their types. A host that makes one a request paid a quarter of a megabyte a request.
	/// </summary>
	[Fact]
	public void A_fresh_scope_learns_how_deep_its_types_go_for_little()
	{
		Assert.False(ExpressionParser.TryParse("(Warm" + Parts(12, 0) + " x) => x", Fresh()).IsSuccess);

		// A generic name longer than any type asks the scope's depth and looks nothing up, so in a fresh
		// scope the first such text pays for the depth and the second, of other names, does not. The least
		// of several scopes, since a table that grows as scopes are added grows in one of them.
		var cost = long.MaxValue;

		for (var round = 0; round < 5; round++)
		{
			var scope = Fresh();

			// Where it looks, worked out before anything is measured: a scope's own cost, not its depth's.
			Assert.NotEmpty(scope.Assemblies);

			var first = Allocated("(Root" + Parts(40, 10 * round + 1) + "<int> x) => x", scope);
			var again = Allocated("(Root" + Parts(40, 10 * round + 2) + "<int> x) => x", scope);

			cost = Math.Min(cost, first - again);
		}

		TestContext.Current.TestOutputHelper?.WriteLine($"learning a fresh scope's depth: {cost:N0} bytes");
		Assert.True(cost < 16 * 1024, $"Learning a fresh scope's depth cost {cost:N0} bytes.");
	}

	/// <summary>
	/// A long member chain on a parameter names no type, and so walks no closure: a scope looks for its
	/// assemblies only when a text names a type (ResolutionScope.Around), and a name whose head the text
	/// declared is not one, however long it is.
	/// </summary>
	[Fact]
	public void A_long_member_chain_on_a_parameter_walks_no_closure()
	{
		var scope   = Fresh();
		var walked  = (Lazy<Assembly[]>)typeof(ResolutionScope).GetField("_assemblies", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(scope)!;
		var chained = ExpressionParser.Compile<Func<Deep.N1.N2.N3.Chain, Deep.N1.N2.N3.Chain>>("c => c" + string.Concat(Enumerable.Repeat(".A", 40)), scope);
		var chain   = new Deep.N1.N2.N3.Chain();

		Assert.Same(chain, chained(chain));
		Assert.False(walked.IsValueCreated, "A member chain on a parameter walked the scope's closure.");
	}

	/// <summary>The same chain, accepted, allocates about as much a member at every length: it is read once, not given back a part at a time.</summary>
	[Fact]
	public void A_long_member_chain_on_a_parameter_allocates_about_as_many_bytes_a_member_at_every_size()
	{
		AssertLinear("a member chain", static (parts, _) => "(DotGram.Tests.Deep.N1.N2.N3.Chain c) => c" + string.Concat(Enumerable.Repeat(".A", parts)), accepted: true);
	}

	/// <summary>
	/// Asserts bytes a part stay within a quarter of one another at every doubling in <see cref="Sizes"/>, on
	/// the first reading of names no cache has seen, in a scope of its own: where the cost is in caches filled
	/// by name, a reading of names seen before measures what is left once they are full.
	/// </summary>
	static void AssertLinear(string shape, Func<int, int, string> textFor, bool accepted = false)
	{
		// What every reading pays once, in every scope and process, paid before anything is measured.
		ExpressionParser.TryParse(textFor(Sizes[^1], -1), Fresh());

		var perPart = new double[Sizes.Length];

		for (var i = 0; i < Sizes.Length; i++)
		{
			var scope = Fresh();

			// What a scope pays once — its namespaces, its depth — and the room the reader's pools need for a
			// text this long, paid by another text of the same shape and other names in it first.
			ExpressionParser.TryParse(textFor(Sizes[i], -2 - i), scope);

			var text  = textFor(Sizes[i], Sizes[i]);
			var clock = Stopwatch.StartNew();
			var bytes = Allocated(text, scope);

			Assert.Equal(accepted, ExpressionParser.TryParse(text, scope).IsSuccess);

			perPart[i] = bytes / (double)Sizes[i];
			TestContext.Current.TestOutputHelper?.WriteLine(
				$"{shape}: {Sizes[i]} parts, {bytes:N0} bytes, {perPart[i]:F1} a part, {clock.Elapsed.TotalMilliseconds:F2} ms");
		}

		for (var i = 1; i < Sizes.Length; i++)
			Assert.True(
				perPart[i] <= perPart[i - 1] * 1.25,
				$"{shape}: at {Sizes[i - 1]} parts a refusal allocated {perPart[i - 1]:F1} bytes each, and at " +
				$"{Sizes[i]} it allocated {perPart[i]:F1} bytes each — {perPart[i] / perPart[i - 1]:F2} " +
				"times as many, where a name asked about once a prefix would stay flat.");
	}

	/// <summary>What one reading of <paramref name="text"/> allocates on this thread.</summary>
	static long Allocated(string text, ResolutionScope scope)
	{
		var before = GC.GetAllocatedBytesForCurrentThread();

		ExpressionParser.TryParse(text, scope);

		return GC.GetAllocatedBytesForCurrentThread() - before;
	}

	/// <summary>A scope of its own: this assembly, what it references, and LINQ, made afresh on every call.</summary>
	static ResolutionScope Fresh()
	{
		return ResolutionScope.Of(typeof(ExpressionDottedTypeRefusalAllocationTests).Assembly, typeof(Enumerable).Assembly);
	}

	/// <summary><paramref name="parts"/> dotted parts, named by <paramref name="salt"/> so that no cache has seen them.</summary>
	static string Parts(int parts, int salt)
	{
		return string.Concat(Enumerable.Repeat(".A" + salt, parts));
	}

	/// <summary>A lambda whose parameter is typed by a name of <paramref name="parts"/> parts after a root nothing declares.</summary>
	static string Parameter(int parts)
	{
		return "(MissingRoot" + string.Concat(Enumerable.Repeat(".A", parts)) + " x) => x";
	}
}
