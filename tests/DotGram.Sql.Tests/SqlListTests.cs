using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Xunit;

namespace DotGram.Sql.Tests;

/// <summary>
/// <see cref="SqlList{T}"/>, which holds a list of one reference without an array: the same list either way.
/// </summary>
/// <remarks>
/// The checks are one piece of C#, run twice: compiled here with <c>SqlList.cs</c> as .NET compiles it, and
/// without the <c>NET</c> symbol, as the package's .NET Standard build has it. Each run is a list of what
/// failed.
/// </remarks>
public sealed class SqlListTests
{
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void A_list_of_one_is_the_same_list_as_any_other(bool net)
	{
		Assert.Equal([], Run(net));
	}

	/// <summary>And the shipped list, as the package's own build has it.</summary>
	[Fact]
	public void The_shipped_list_holds_one_reference_without_an_array()
	{
		var one = SqlList.Create<string>(["a"]);

		Assert.Same(one[0], one.AsSpan()[0]);
		Assert.Equal(SqlList.From(new List<string> { "a" }), one);
		Assert.Single(one);
	}

	/// <summary>
	/// A span of a list of one is its own, and reads the element still after the variable it was taken from is
	/// given another list; a span over the list's field would have read the new list's array as the element.
	/// </summary>
	[Fact]
	public void A_span_of_a_list_of_one_outlives_the_list()
	{
		var list = SqlList.Create<string>(["a"]);
		var span = list.AsSpan();

		list = SqlList.Create<string>(["b", "c"]);

		Assert.Equal("a", span[0]);
		Assert.Equal(2, list.Count);
	}

	static string[] Run(bool net)
	{
		var parse = CSharpParseOptions.Default
			.WithLanguageVersion(LanguageVersion.Latest)
			.WithPreprocessorSymbols(net ? new[] { "NET" } : Array.Empty<string>());
		var trees = new[]
		{
			CSharpSyntaxTree.ParseText(File.ReadAllText(Source()), parse, "SqlList.cs"),
			CSharpSyntaxTree.ParseText(Checks, parse, "Checks.cs"),
		};

		// The package's own assembly is left out: the list compiled here is the one checked.
		var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
			.Split(Path.PathSeparator)
			.Where(static path => !Path.GetFileName(path).Equals("DotGram.Sql.dll", StringComparison.OrdinalIgnoreCase))
			.Select(static path => (MetadataReference)MetadataReference.CreateFromFile(path));

		var compilation = CSharpCompilation.Create(
			"SqlListChecks" + (net ? "Net" : "Standard"),
			trees,
			references,
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

		using var stream = new MemoryStream();

		var emitted = compilation.Emit(stream);

		Assert.True(emitted.Success, string.Join("\n", emitted.Diagnostics.Where(static one => one.Severity == DiagnosticSeverity.Error)));

		stream.Position = 0;

		var context = new AssemblyLoadContext("SqlListChecks", isCollectible: true);

		try
		{
			var checks = context.LoadFromStream(stream).GetType("DotGram.Sql.Checks")!;

			return (string[])checks.GetMethod("Run")!.Invoke(null, null)!;
		}
		finally
		{
			context.Unload();
		}
	}

	static string Source([CallerFilePath] string here = "")
	{
		return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "..", "src", "DotGram.Sql", "SqlList.cs"));
	}

	const string Checks = """
		using System;
		using System.Collections.Generic;
		using System.Linq;

		namespace DotGram.Sql;

		public static class Checks
		{
			public static string[] Run()
			{
				var failed = new List<string>();

				void Check(bool ok, string what)
				{
					if (!ok)
						failed.Add(what);
				}

				// A list of one, made each way a list is made.
				var one   = SqlList.Create<string>(new[] { "a" });
				var owned = SqlList.Own(new[] { new string('a', 1) });
				var from  = SqlList.From(new List<string> { "a" });

				Check(one.Count == 1 && one.Length == 1 && !one.IsEmpty, "a list of one counts one");
				Check(one[0] == "a", "a list of one holds its element");
				Check(one.AsSpan().Length == 1 && one.AsSpan()[0] == "a", "a list of one spans its element");
				Check(one.ToArray() is ["a"], "a list of one copies to an array of one");
				Check(one.Equals(owned) && owned.Equals(one) && one.Equals(from), "a list of one equals one made otherwise");
				Check(one.GetHashCode() == owned.GetHashCode() && one.GetHashCode() == from.GetHashCode(), "a list of one hashes as one made otherwise");
				Check(one.ToString() == "[a]", "a list of one is written as one");
				Check(one.Slice(0, 1).Equals(one) && one.Slice(1, 0).IsEmpty, "a list of one slices");
				Check(one.Exists(static x => x == "a") && one.TrueForAll(static x => x == "a"), "a list of one is searched");
				Check(one is ["a"], "a list of one matches a list pattern");
				Check(((IEnumerable<string>)one).ToList() is ["a"], "a list of one enumerates as an interface");

				var walked = new List<string>();

				foreach (var item in one)
					walked.Add(item);

				Check(walked is ["a"], "a list of one walks");

				Check(Throws<IndexOutOfRangeException>(() => one[1]) && Throws<IndexOutOfRangeException>(() => one[-1]), "a list of one has no other place");
				Check(Throws<IndexOutOfRangeException>(() => default(SqlList<string>)[0]), "the empty list has no place");

				// What is not held without an array: null, a value, an element that is itself an array.
				var none = SqlList.Create<string?>(new string?[] { null });

				Check(none.Count == 1 && none[0] is null && !none.IsEmpty, "a list of one null is one, not empty");

				var number = SqlList.Create<int>(new[] { 7 });

				Check(number.Count == 1 && number[0] == 7 && number.AsSpan()[0] == 7, "a list of one value");

				var nested = SqlList.Create<object>(new object[] { new[] { 1, 2 } });

				Check(nested.Count == 1 && nested[0] is int[] { Length: 2 }, "a list of one array is a list of one, not of two");

				// Arrays of a type derived from the element's, which a list over its own array holds as they are.
				var derived = SqlList.Own<object>(new string[] { "a", "b" });

				Check(derived.ToArray() is ["a", "b"] && derived.Slice(1, 1) is ["b"] && derived.Slice(0, 2).Count == 2, "a list over a derived array copies and slices");
				Check(derived.Equals(SqlList.Create<object>(new object[] { "a", "b" })), "a list over a derived array equals one over its own");

				// The empty list, every way.
				Check(default(SqlList<string>).Equals(SqlList.Create<string>(Array.Empty<string>())) && default(SqlList<string>).AsSpan().IsEmpty, "the empty list");
				Check(default(SqlList<string>).GetHashCode() == SqlList.Own<string>(null).GetHashCode(), "the empty list hashes as one");

				// Longer lists, unchanged.
				var two = SqlList.Create<string>(new[] { "a", "b" });

				Check(two.Equals(SqlList.Own(new[] { "a", "b" })) && two.GetHashCode() == SqlList.Own(new[] { "a", "b" }).GetHashCode(), "a list of two");
				Check(!two.Equals(one) && !one.Equals(two) && two.Slice(1, 1).Equals(SqlList.Create<string>(new[] { "b" })), "a list of two against one");

				return failed.ToArray();
			}

			static bool Throws<T>(Func<object?> act) where T : Exception
			{
				try
				{
					act();

					return false;
				}
				catch (T)
				{
					return true;
				}
			}
		}
		""";
}
