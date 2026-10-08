using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using DotGram.Generation;
using DotGram.Web;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// RFC 5322 as it ships, compiled again on the tape: the reading the address grammar had before
/// the generator carried every grammar it could with the immediate carrier, kept to measure the
/// tape on and to hold the shipped reading against.
/// </summary>
/// <remarks>
/// <para>
/// The grammar is read out of <c>src/DotGram.Web/Rfc5322.cs</c> as it stands, with its host class,
/// and given <c>Carrier = GramCarrier.Tape</c>: a copy that is the shipped text and cannot drift
/// from it. It is compiled into an assembly named as the one the package shows its internals to,
/// since the host calls into it.
/// </para>
/// <para>
/// What the tape keeps that the immediate carrier does not — the log of records and the walk at the
/// end — is what <c>PoolRetentionScalingTests</c> measures on the address lists: whether a parse
/// reuses what the parse before it grew. The shipped reading builds as it reads, so the tree is
/// most of what a second parse allocates there and the question has nothing to ask of it.
/// </para>
/// </remarks>
static class Rfc5322Tape
{
	/// <summary>The generated class, in <c>DotGram.Web.TapeBed</c>; its publications are the shipped ones.</summary>
	public static Type Parser => Compiled.Value;

	static readonly Lazy<Type> Compiled = new(Compile, isThreadSafe: true);

	/// <summary>A publication of the tape's reading, as a <c>Try</c> method over a string answers it.</summary>
	public static (bool IsSuccess, string? Value, long Position) Try(string publication, string text)
	{
		return Answer(Parser, publication, text);
	}

	/// <summary>Whether the tape's reading of a publication reads a text, asking nothing more of the answer.</summary>
	public static Func<string, bool> Reads(string publication)
	{
		var method = Parser.GetMethod("Try" + publication, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, [typeof(string)])!;
		var passed = method.ReturnType.GetProperty("IsSuccess")!;

		return text => (bool)passed.GetValue(method.Invoke(null, [text]))!;
	}

	/// <summary>The same of the shipped reading.</summary>
	public static (bool IsSuccess, string? Value, long Position) Shipped(string publication, string text)
	{
		return Answer(typeof(AddrSpec).Assembly.GetType("DotGram.Web.Rfc5322")!, publication, text);
	}

	static (bool IsSuccess, string? Value, long Position) Answer(Type host, string publication, string text)
	{
		var method = host.GetMethod("Try" + publication, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, [typeof(string)])!;
		var match  = method.Invoke(null, [text])!;

		object? Read(string name)
		{
			return match.GetType().GetProperty(name)!.GetValue(match);
		}

		return (bool)Read("IsSuccess")!
			? (true, Shown(Read("Value")), (long)Read("Position")!)
			: (false, (string?)Read("Error"), (long)Read("Position")!);
	}

	/// <summary>A value as text: a list as its items, so that two lists of equal items say so.</summary>
	static string? Shown(object? value)
	{
		return value switch
		{
			null => null,
			string text => text,
			IEnumerable items => "[" + string.Join(", ", items.Cast<object?>().Select(Shown)) + "]",
			_ => value.ToString(),
		};
	}

	static Type Compile()
	{
		var shipped = File.ReadAllText(Source());
		var start   = shipped.IndexOf("[Gram(\"\"\"", StringComparison.Ordinal);
		var end     = shipped.IndexOf("\"\"\")]", start, StringComparison.Ordinal);
		var host    = shipped.IndexOf("static partial class Rfc5322", end, StringComparison.Ordinal);

		Assert.True(start > 0 && end > start && host > end, "src/DotGram.Web/Rfc5322.cs is not shaped as this copy reads it.");

		var copy =
			"using System;\n" +
			"using System.Collections.Generic;\n" +
			"using System.Text;\n" +
			"using DotGram;\n" +
			"using DotGram.Web;\n" +
			"namespace DotGram.Web.TapeBed;\n" +
			shipped.Substring(start, end - start) + "\"\"\", Carrier = GramCarrier.Tape)]\n" +
			"public " + shipped.Substring(host);

		var parse       = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest);
		var package     = typeof(AddrSpec).Assembly.Location;
		var references  = EmittedCode.References
			.Where(reference => !string.Equals(reference.Display, package, StringComparison.OrdinalIgnoreCase))
			.Append(MetadataReference.CreateFromFile(package));
		var compilation = CSharpCompilation.Create(
			"DotGram.Tests",
			[CSharpSyntaxTree.ParseText(copy, parse, "Rfc5322Tape.cs")],
			references,
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

		CSharpGeneratorDriver
			.Create([new GramGenerator().AsSourceGenerator()], parseOptions: parse)
			.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var reported);

		Assert.DoesNotContain(reported, static one => one.Severity == DiagnosticSeverity.Error);

		using var stream = new MemoryStream();

		var result = output.Emit(stream);

		Assert.True(
			result.Success,
			"The tape copy of RFC 5322 did not compile:\n" +
			string.Join("\n", result.Diagnostics.Where(static one => one.Severity == DiagnosticSeverity.Error).Take(20)));

		var parser = Assembly.Load(stream.ToArray()).GetType("DotGram.Web.TapeBed.Rfc5322")!;

		// Or the copy is the immediate reading again, and every comparison with it compares the
		// shipped reading with itself.
		Assert.Contains(output.SyntaxTrees, static tree => tree.ToString().Contains("Materialize_DotGram", StringComparison.Ordinal));

		return parser;
	}

	static string Source([CallerFilePath] string here = "")
	{
		return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "..", "src", "DotGram.Web", "Rfc5322.cs"));
	}
}
