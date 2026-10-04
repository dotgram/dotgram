using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Emit;

using Xunit;

namespace DotGram.Tests;

public sealed class ExpectedTablesTests
{
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Sibling_publications_share_diagnostics_without_changing_failures(bool buffered)
	{
		var compiled = GramCompiler.Compile("""
			First = 'a' & when @(true) & 'x'
			Second = 'b' & when @(true) & 'x'
			parse First
			parse Second
			""", new GramCompilerOptions
		{
			BufferedInput = buffered, BufferedBytes = buffered, CSharpScanner = RoslynCSharpScanner.Instance,
		});
		EmittedCode.Quiet(compiled.Diagnostics);
		var members = """
			public static string ErrorOf(string input, int root)
			{
				return root == 0 ? TryParseFirst(input).Error : TryParseSecond(input).Error;
			}
			""";
		if (buffered)
			members += """
			public static string ReaderError(string input, int root)
			{
				using (var reader = new global::System.IO.StringReader(input))
					return root == 0 ? TryParseFirst(reader).Error : TryParseSecond(reader).Error;
			}
			public static string ByteError(string input, int root)
			{
				using (var stream = new global::System.IO.MemoryStream(global::System.Text.Encoding.ASCII.GetBytes(input)))
					return root == 0 ? TryParseFirst(stream).Error : TryParseSecond(stream).Error;
			}
			""";
		var source = Assert.Single(compiled.Sources).Text;
		var assembly = EmittedCode.Compile(source, declarationMembers: members);
		var type = assembly.GetType("Grammar")!;
		// Each set is built the first time a refusal asks for it (D17), by its number.
		var sets = SetsOf(source);
		var tables = Enumerable.Range(0, sets.Count)
			.Where(id => sets[id] is not null)
			.Select(id => Set(type, id))
			.ToArray();

		Assert.Single(tables, table => table.SequenceEqual(new[] { "'x'" }));
		Assert.Equal(tables.Length, tables.Select(table => string.Join("\0", table)).Distinct(StringComparer.Ordinal).Count());

		foreach (var method in buffered ? new[] { "ErrorOf", "ReaderError", "ByteError" } : new[] { "ErrorOf" })
		for (var repeat = 0; repeat < 2; repeat++)
		{
			Assert.Equal("Expected 'x'.", type.GetMethod(method)!.Invoke(null, ["az", 0]));
			Assert.Equal("Expected 'x'.", type.GetMethod(method)!.Invoke(null, ["bz", 1]));
			Assert.Null(type.GetMethod(method)!.Invoke(null, ["ax", 0]));
			Assert.Null(type.GetMethod(method)!.Invoke(null, ["bx", 1]));
		}
	}

	/// <summary>
	/// A set is one array for the life of the process, and two machines that keep sets of their
	/// own keep two arrays for the same items.
	/// </summary>
	/// <remarks>
	/// A refusal tells sets apart by reference: <c>Refuse_DotGram</c> lets the same set said again
	/// at the same place go, and keeps a second set with the same items as a tie of its own. The
	/// sets were properties of each machine, so a token's second reading (the machine that reads a
	/// terminal's value from its text) and the syntax over the tokens named the end of the input
	/// with two arrays. One list of items for the whole class must not make them one.
	/// </remarks>
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void A_set_is_one_array_and_two_machines_keep_two(bool utf8)
	{
		var compiled = GramCompiler.Compile("""
			wordboundary = ['a'..'z']
			trivia = { ' '* }
			namespace Token
			{
				trivia = none
				Bool : @string = t: ("yes"i | "no"i) => @(t)
			}
			Program : @string = b: Token.Bool & ("yes"i | "no"i) & eof => @(b)
			parse Program
			""", new GramCompilerOptions
		{
			Lexical = true, Utf8Literals = utf8, CSharpScanner = RoslynCSharpScanner.Instance,
		});
		EmittedCode.Quiet(compiled.Diagnostics);

		var source = Assert.Single(compiled.Sources).Text;

		Assert.Equal(utf8, source.Contains("\"u8;", StringComparison.Ordinal));

		var type = EmittedCode.Compile(source, utf8Literals: utf8).GetType("Grammar")!;
		var sets = SetsOf(source);
		var ends = Enumerable.Range(0, sets.Count)
			.Where(id => sets[id] is { } set && set.SequenceEqual(new[] { "end of input" }))
			.ToArray();

		// The syntax's and the second reading's: the same items, two numbers.
		Assert.Equal(2, ends.Length);

		// What each number is built into is what the source says it names, and the same array
		// however often it is asked for.
		for (var id = 0; id < sets.Count; id++)
			if (sets[id] is { } set)
			{
				var built = Set(type, id);

				Assert.Equal(set, built);
				Assert.Same(built, Set(type, id));
			}

		var first  = Set(type, ends[0]);
		var second = Set(type, ends[1]);

		Assert.NotSame(first, second);

		// And a refusal holds them apart as it did: the same set again at one place is one
		// thing wanted, the other set with the same items a tie of its own.
		var refuse  = type.GetMethod("Refuse_DotGram", BindingFlags.Static | BindingFlags.NonPublic)!;
		var failure = Activator.CreateInstance(type.GetNestedType("Failure", BindingFlags.Public | BindingFlags.NonPublic)!)!;
		object?[] call = [failure, 3, first];

		refuse.Invoke(null, call);
		refuse.Invoke(null, call);

		Assert.Equal(0, More(call[0]!));

		call[2] = second;
		refuse.Invoke(null, call);
		refuse.Invoke(null, call);

		Assert.Equal(1, More(call[0]!));

		static int More(object failure)
		{
			var more = (List<string[]>?)failure.GetType().GetField("ExpectedMore")!.GetValue(failure);

			return more?.Count ?? 0;
		}
	}

	/// <summary>The array a generated class builds for the set numbered <paramref name="id"/>.</summary>
	static string[] Set(Type type, int id)
	{
		return (string[])type.GetMethod("ExpectedSet_DotGram", BindingFlags.Static | BindingFlags.NonPublic)!.Invoke(null, [id])!;
	}

	/// <summary>
	/// The sets a generated source names, by number, read out of its text the way its accessor
	/// reads them at run time; null for a number nothing in the source asks for.
	/// </summary>
	/// <remarks>
	/// What a set holds is data in the generated file, not text a pattern can find beside the
	/// site that names it, so a test of what a site refuses with reads it here.
	/// </remarks>
	internal static IReadOnlyList<string[]?> SetsOf(string source)
	{
		var data  = Regex.Match(source, @"var data = ""([^""]*)""(?:u8)?;");
		var count = Regex.Match(source, @"new string\[\]\?\[(\d+)\]");

		if (!data.Success || !count.Success)
			return [];

		var bytes = Regex.Replace(
			data.Groups[1].Value,
			@"\\u([0-9A-F]{4})",
			static escape => ((char)int.Parse(escape.Groups[1].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture)).ToString());

		var listed = source.Substring(source.IndexOf("ExpectedItems_DotGram, new string[]", StringComparison.Ordinal));

		listed = listed.Substring(0, listed.IndexOf("}, null) ?? ExpectedItems_DotGram!;", StringComparison.Ordinal));

		var items = Regex.Matches(listed, @"(?m)^\s*""((?:[^""\\]|\\.)*)"",\r?$")
			.Select(static item => Unescaped(item.Groups[1].Value))
			.ToArray();

		var numbers = new List<int>();
		var ends    = new List<int>();
		var value   = 0;

		for (var at = 0; at < bytes.Length; at++)
		{
			value = value << SpelledNumbers.Bits | bytes[at] & ((1 << SpelledNumbers.Bits) - 1);

			if (bytes[at] < SpelledNumbers.More)
			{
				numbers.Add(value - 1);
				ends.Add(at + 1);
				value = 0;
			}
		}

		var sets   = new string[]?[int.Parse(count.Groups[1].Value, CultureInfo.InvariantCulture)];
		var header = ends[sets.Length - 1];

		for (var id = 0; id < sets.Length; id++)
		{
			if (numbers[id] < 0)
				continue;

			var first = ends.IndexOf(header + numbers[id]) + 1;

			sets[id] = [.. numbers.Skip(first + 1).Take(numbers[first]).Select(index => items[index])];
		}

		return sets;

		static string Unescaped(string text)
		{
			var plain = new StringBuilder(text.Length);

			for (var at = 0; at < text.Length; at++)
			{
				if (text[at] != '\\')
				{
					plain.Append(text[at]);

					continue;
				}

				var escaped = text[++at];

				if (escaped == 'u')
				{
					plain.Append((char)int.Parse(text.Substring(at + 1, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture));
					at += 4;
				}
				else
					plain.Append(escaped switch { 'r' => '\r', 'n' => '\n', 't' => '\t', '0' => '\0', _ => escaped });
			}

			return plain.ToString();
		}
	}
}
