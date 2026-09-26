using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using DotGram.ExpressionLanguage;

using Xunit;

namespace DotGram.Tests.ExpressionLanguage;

/// <summary>
/// A vocabulary of some thousands of real members is kept, not thrown away for being large.
/// </summary>
/// <remarks>
/// <para>
/// The member caches have a bound, and it is there for a reason: a key that comes out of the text
/// is one a text can grow, since every `s.Nothing` anybody types is a name that is not there and
/// an answer that says so. What the bound must not do is throw away what a real host asks for.
/// </para>
/// <para>
/// It did. A FIX 4.4 dictionary compiles 571 check texts and needs about 4,500 distinct member
/// lookups; against a bound of 4,096 counting every answer, the cache filled and cleared once per
/// pass, and the rest of each pass was reflection again. Of what it held, none said nothing. So
/// the bound is on the answers that say nothing, and this is the check that the other kind is
/// kept however many there are.
/// </para>
/// <para>
/// It watches the count rather than the clock: a count needs no quiet machine, and other tests
/// sharing these caches can only add to it. A found answer is never evicted now, so the count is
/// monotonic whatever else is running — which is exactly what this asserts.
/// </para>
/// </remarks>
public sealed class VocabularyTests
{
	/// <summary>How many distinct members this has to reach to say anything.</summary>
	/// <remarks>Past the bound of 4,096, because below it the old behaviour passes too.</remarks>
	const int Enough = 5000;

	[Fact]
	public void A_vocabulary_larger_than_the_bound_is_kept_whole()
	{
		var kept = Cache("_instanceMembers");
		var texts = Texts(out var asked);

		Assert.True(asked >= Enough, $"the probe only found {asked} members to name; it needs {Enough}.");

		var was  = kept.Count;
		var fell = 0;
		var from = 0;

		foreach (var text in texts)
		{
			ExpressionParser.Parse(text, typeof(VocabularyTests).Assembly);

			var now = kept.Count;

			// Step by step, not first against last: a cache that fills and clears ends larger
			// than it began, and only the step that goes backwards says it was thrown away.
			if (now < was)
			{
				fell++;
				from = Math.Max(from, was);
			}

			was = now;
		}

		Assert.True(fell == 0, $"the cache was thrown away {fell} time(s) while a vocabulary was read, from {from}.");

		// And it really did grow past the bound, or the assertion above proves nothing.
		Assert.True(was > 4096, $"only {was} answers were kept, which is under the bound this is about.");
	}

	/// <summary>
	/// A chain whose head is a parameter is never asked whether it names a type.
	/// </summary>
	/// <remarks>
	/// <para>
	/// C#'s order: a simple name that binds to a local, a parameter or a variable of a `foreach`
	/// or a `catch` IS that, and is never looked up as a type or a namespace. So `one.A.B` is a
	/// member access from `one`, and asking the type tables for `one.A.B` — and for it under
	/// every `using` in the text — is work C# does not do.
	/// </para>
	/// <para>
	/// It was most of the work this did. FIX's checks are written over lambda parameters, and
	/// each `context.Validators.X`, `message.Y` and `entry.Z` was asked of the tables and
	/// answered no, three times over for three `using`s; the cache of names that are not there
	/// filled and cleared three times a pass, and a pass took 850 ms where it now takes 173.
	/// </para>
	/// <para>
	/// The count has to stay at nought, which is what reaches the defect: a bound would not have.
	/// </para>
	/// <para>
	/// The text writes `int` and not `var` on purpose. `var` IS asked of the type tables, once
	/// per `using`, because C# asks too — a type actually named `var` would win — and the answer
	/// is kept, so it is four entries in the life of a process rather than a defect. Leaving it
	/// in would have made this assertion about that instead of about what it is for.
	/// </para>
	/// </remarks>
	[Fact]
	public void A_chain_on_a_parameter_asks_the_type_tables_nothing()
	{
		var absent = Cache("_typesAbsent");
		var inside = Cache("_insideAbsent");

		// Many distinct chains on a parameter, under four `using`s: with the defect each one was
		// asked of the type tables five times over, so the count would rise by scores here.
		var text = "using System; using System.Text; using System.Collections.Generic; using System.Linq; " +
			"(System.Text.StringBuilder one) => { " +
			"int a = one.Length; int b = one.Capacity; int c = one.MaxCapacity; " +
			"bool d = one.Equals(one); int e = one.GetHashCode(); " +
			"System.Type f = one.GetType(); string g = one.ToString(); " +
			"int h = one.GetType().Name.Length; int i = one.GetType().Namespace.Length; " +
			"return a + b + c + e + g.Length + h + i; }";

		var wasAbsent  = absent.Count;
		var wasInside  = inside.Count;
		var wereAbsent = absent.Names();
		var wereInside = inside.Names();

		ExpressionParser.Parse(text, typeof(VocabularyTests).Assembly);

		Assert.True(
			absent.Count == wasAbsent,
			$"the type tables were asked about {absent.Count - wasAbsent} name(s) they do not have: " +
			Shown(absent.Names().Except(wereAbsent)));

		// One for each type name the TEXT ITSELF writes — `System.Text.StringBuilder` and
		// `System.Type` — which has to be looked for somewhere, and the calling assembly is
		// asked first and does not declare it. Those are names an author wrote, not chains this
		// invented, and there is one per type however often it is written. What must not appear
		// here is a chain: `one.GetType().Name` and its kind.
		Assert.True(
			inside.Count - wasInside <= 2,
			$"the calling assembly was asked about {inside.Count - wasInside} name(s) it does not have: " +
			Shown(inside.Names().Except(wereInside)));
	}

	/// <summary>The first few of them, because a message naming eighty is a message nobody reads.</summary>
	static string Shown(IEnumerable<string> names)
	{
		var all   = names.ToList();
		var first = string.Join(", ", all.Take(6));

		return all.Count <= 6 ? first : first + $", and {all.Count - 6} more";
	}

	/// <summary>One lambda per type, naming every member of it this can name, and how many that was.</summary>
	static List<string> Texts(out int asked)
	{
		var texts = new List<string>();
		var seen  = 0;

		foreach (var type in Types())
		{
			var names = Members(type);

			if (names.Count == 0)
				continue;

			var body = new StringBuilder("(").Append(type.FullName).Append(" one) => {");

			foreach (var name in names)
				body.Append(" var ").Append("x").Append(seen++).Append(" = one.").Append(name).Append(';');

			texts.Add(body.Append(" return 0; }").ToString());

			if (seen >= Enough)
				break;
		}

		asked = seen;

		return texts;
	}

	/// <summary>Types a text can name: public, not generic, not nested, with a writable full name.</summary>
	static IEnumerable<Type> Types()
	{
		foreach (var assembly in ResolutionScope.Around(typeof(VocabularyTests).Assembly).Assemblies)
		{
			Type[] types;

			try
			{
				types = assembly.GetExportedTypes();
			}
			catch (Exception thrown) when (thrown is NotSupportedException or TypeLoadException or
				ReflectionTypeLoadException or System.IO.FileNotFoundException or System.IO.FileLoadException)
			{
				continue;
			}

			foreach (var type in types)
				if (!type.IsGenericTypeDefinition && !type.IsNested && !type.IsByRefLike &&
					type.FullName is { } name && !name.Contains('+') && !name.Contains('`'))
					yield return type;
		}
	}

	/// <summary>The instance properties of it a text may read, by name and each once.</summary>
	static List<string> Members(Type type)
	{
		var names = new List<string>();
		var seen  = new HashSet<string>(StringComparer.Ordinal);

		foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
			if (property.GetMethod is { IsPublic: true } && property.GetIndexParameters().Length == 0 &&
				!property.PropertyType.IsByRefLike && seen.Add(property.Name))
				names.Add(property.Name);

		return names;
	}

	/// <summary>One of the parser's caches, read the way a measurement reads it.</summary>
	/// <remarks>
	/// By reflection and on purpose. What is asserted is a property of the cache, and a cache a
	/// consumer could see would be a cache a consumer could come to depend on.
	/// </remarks>
	static ICounted Cache(string name)
	{
		// Nested types too: `_typesAbsent` and its kind live in `Loaded`, inside the parser, and
		// a search that reads only the parser's own fields finds nothing and says nothing —
		// which is exactly how the first measurement of this defect missed where it was.
		foreach (var holder in new[] { typeof(ExpressionParser) }.Concat(
			typeof(ExpressionParser).GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public)))
		{
			if (holder.GetField(name, BindingFlags.NonPublic | BindingFlags.Static) is { } field)
				return new Counted(field.GetValue(null)!);
		}

		Assert.Fail($"there is no cache called {name} any more; this test names it by hand.");

		return null!;
	}

	/// <summary>Whatever the cache is, asked only how many answers it holds.</summary>
	sealed class Counted(object cache) : ICounted
	{
		public int Count => (int)cache.GetType().GetProperty("Count")!.GetValue(cache)!;

		/// <summary>The names it was asked about, so a failure says WHICH rather than how many.</summary>
		public List<string> Names()
		{
			var names = new List<string>();

			foreach (var entry in (System.Collections.IEnumerable)cache)
				names.Add(entry.GetType().GetProperty("Key")!.GetValue(entry)!.ToString()!);

			return names;
		}
	}

	/// <summary>The one question a measurement asks of a cache.</summary>
	interface ICounted
	{
		int Count { get; }

		List<string> Names();
	}
}
