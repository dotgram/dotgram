using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

using DotGram.Generation;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The shipped grammars that keep settled copies, generated from their own files as a build generates
/// them — once as they ship and once under <c>DOTGRAM_NO_SETTLE</c> — and every input answered alike
/// by the two: accepted or not, the value, the message and where it stands.
/// </summary>
/// <remarks>
/// The XML example, the configuration example with its located reading, the notation's own grammar and
/// RFC 7239's Forwarded field: each file on its own, with the usings a project of the repository has
/// implicitly, and the generator's attributes it writes itself.
/// </remarks>
public sealed class SettledShippedFormsTests
{
	static readonly (string File, string Type, string Method, string[] Inputs, string Alphabet, int Longest)[] Readings =
	[
		("examples/DotGram.Examples/Formats/XmlExample.cs", "DotGram.Examples.Formats.XmlParser", "TryParseXml",
			["<a x=\"1\" y='2'><b>text</b><c/></a>", "<a></b>", "<ab=\"1\">", "<ab=\"1\"></a>", "<ab=\"1\"></ab>", "<ab =\"1\">", "<a b=\"1\">",
			 "<" + new string('a', 300) + "!", "<" + new string('a', 300) + " !"],
			"<>=/\"'ab !", 12),
		("examples/DotGram.Examples/Formats/LocatedConfigExample.cs", "DotGram.Examples.Formats.Config", "TryParseFile",
			["a = b\n", "a=b\nc=d\n", "# x\na = b", "a", "=b\n", "a = \n", "[s]\na=b\n"], "ab=# \n[]x", 14),
		("examples/DotGram.Examples/Formats/LocatedConfigExample.cs", "DotGram.Examples.Formats.Config", "TryParseKeySpan",
			["a = b\n", "a=b", "a", ""], "ab=# \n", 10),
		("examples/DotGram.Examples/Formats/LocatedConfigExample.cs", "DotGram.Examples.Formats.Config+Located", "TryParseFile",
			["a = b\n", "a=b\nc=d\n", "# x\na = b", "a", "=b\n", "a = \n"], "ab=# \n[]x", 14),
		("examples/DotGram.Examples/Languages/GramExample.cs", "DotGram.Examples.Languages.GramGrammar", "TryParseFile",
			["A = 'a'\n", "A = 'a' | B\nB = \"b\"*\n", "A = @(x)", "parse A", "A = [a..z]+ & ?!B", "A : @int = 'a' => @(1)"], "Ab=|&'\"/ *;{}@:()+?.\n", 16),
		("src/DotGram.Web/Rfc7239.cs", "DotGram.Web.Rfc7239", "TryParseForwarded",
			["for=192.0.2.43", "for=\"[2001:db8:cafe::17]:4711\"", "for=192.0.2.60;proto=http;by=203.0.113.43", "for=a, for=b", "for=", ";;,"],
			"for=\"a1;,[]:_ .\\", 16),
		("src/DotGram.Web/Rfc7239.cs", "DotGram.Web.Rfc7239", "TryParseNode",
			["192.0.2.43", "\"[2001:db8:cafe::17]:4711\"", "unknown", "_x:1", "[::1]"], "0123456789abcdef:[]_\".", 14),
	];

	public static TheoryData<string> Files()
	{
		return new TheoryData<string>(Readings.Select(static reading => reading.File).Distinct());
	}

	[Theory]
	[MemberData(nameof(Files))]
	public void A_shipped_grammar_with_copies_answers_as_it_does_without_them(string file)
	{
		var settled = Built(file, settle: true, out var withCopies);
		var plain   = Built(file, settle: false, out var withoutCopies);

		Assert.True(withCopies, $"{file}: no settled copy was generated");
		Assert.False(withoutCopies, $"{file}: a copy was generated under DOTGRAM_NO_SETTLE");

		var differ = new List<string>();

		foreach (var reading in Readings.Where(one => one.File == file))
		{
			var random = new Random(20261126);
			var inputs = reading.Inputs
				.Concat(Enumerable.Range(0, 2000).Select(_ => new string([.. Enumerable.Range(0, random.Next(reading.Longest + 1)).Select(_ => reading.Alphabet[random.Next(reading.Alphabet.Length)])])))
				.Distinct()
				.ToList();
			var count  = 0;

			foreach (var input in inputs)
			{
				var expected = Answer(plain, reading.Type, reading.Method, input);
				var got      = Answer(settled, reading.Type, reading.Method, input);

				if (expected != got && count++ < 5)
					differ.Add($"{reading.Type}.{reading.Method} \"{input}\": without the copies {expected} | with them {got}");
			}

			TestContext.Current.SendDiagnosticMessage($"{reading.Type}.{reading.Method}: {inputs.Count} inputs, {count} answered otherwise");
		}

		Assert.True(differ.Count == 0, string.Join("\n", differ));
	}

	/// <summary>The file generated and compiled, with the copies or under <c>DOTGRAM_NO_SETTLE</c>, in a context of its own.</summary>
	static Assembly Built(string file, bool settle, out bool copied)
	{
		var parse = CSharpParseOptions.Default
			.WithLanguageVersion(LanguageVersion.Latest)
			.WithPreprocessorSymbols(settle ? Array.Empty<string>() : ["DOTGRAM_NO_SETTLE"]);
		// A grammar of the Web package calls the others' rules, so the package is compiled whole; an
		// example stands alone.
		var sources = file.StartsWith("src/", StringComparison.Ordinal)
			? Directory.EnumerateFiles(Path.Combine(SourceRoot.Directory, Path.GetDirectoryName(file)!), "*.cs").Order(StringComparer.Ordinal).ToArray()
			: [Path.Combine(SourceRoot.Directory, file)];
		var trees   = sources
			.Select(source => CSharpSyntaxTree.ParseText(File.ReadAllText(source), parse, Path.GetFileName(source), cancellationToken: TestContext.Current.CancellationToken))
			.Append(CSharpSyntaxTree.ParseText(Usings, parse, "Usings.cs", cancellationToken: TestContext.Current.CancellationToken))
			.ToArray();

		var compilation = CSharpCompilation.Create(
			"DotGram.Tests.Shipped" + (settle ? "Settled" : "Plain"),
			trees,
			References(),
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

		CSharpGeneratorDriver
			.Create([new GramGenerator().AsSourceGenerator()], parseOptions: parse)
			.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var generated, TestContext.Current.CancellationToken);

		Assert.DoesNotContain(generated, static one => one.Severity == DiagnosticSeverity.Error);

		copied = output.SyntaxTrees.Except(trees).Any(static tree => tree.ToString().Contains("_Settled(", StringComparison.Ordinal));

		using var stream = new MemoryStream();

		var emitted = output.Emit(stream, cancellationToken: TestContext.Current.CancellationToken);

		Assert.True(emitted.Success, string.Join("\n", emitted.Diagnostics.Where(static one => one.Severity == DiagnosticSeverity.Error).Take(10)));

		stream.Position = 0;

		return new AssemblyLoadContext(compilation.AssemblyName, isCollectible: true).LoadFromStream(stream);
	}

	/// <summary>What a project of the repository has without writing it (ImplicitUsings).</summary>
	const string Usings = """
		global using System;
		global using System.Collections.Generic;
		global using System.IO;
		global using System.Linq;
		global using System.Net.Http;
		global using System.Threading;
		global using System.Threading.Tasks;
		""";

	static ImmutableArray<MetadataReference> References()
	{
		return
		[
			.. AppDomain.CurrentDomain
				.GetAssemblies()
				.Where(static assembly => !assembly.IsDynamic && !string.IsNullOrEmpty(assembly.Location) &&
					assembly.GetName().Name is not ("DotGram.Examples" or "DotGram.Web"))
				.Select(static assembly => (MetadataReference)MetadataReference.CreateFromFile(assembly.Location)),
		];
	}

	static string Answer(Assembly assembly, string type, string method, string input)
	{
		var read  = assembly.GetType(type, throwOnError: true)!
			.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
			.First(one => one.Name == method && one.GetParameters() is [{ ParameterType: var only }] && only == typeof(string));
		var match = read.Invoke(null, [input])!;
		var kind  = match.GetType();

		string? Read(string property)
		{
			return kind.GetProperty(property)?.GetValue(match) is { } value ? Show(value) : null;
		}

		return (bool)kind.GetProperty("IsSuccess")!.GetValue(match)! ? $"ok {Read("Value")}" : $"no {Read("Error")} @{Read("Position")}";
	}

	static string Show(object value)
	{
		return value switch
		{
			string text => text,
			IEnumerable items => "[" + string.Join(", ", items.Cast<object>().Select(Show)) + "]",
			_ => value.ToString() ?? "",
		};
	}
}
