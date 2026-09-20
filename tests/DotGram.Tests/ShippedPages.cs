using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The C# a shipped page shows, compiled exactly as it shows it.
/// </summary>
/// <remarks>
/// <para>
/// D37 says an example in a page a package ships exists as a test that compiles and runs it, and
/// the tests that do the running are written by hand, one per example, because they also hold the
/// values the comments claim. This does the other half, and it does it for a reason the hand-written
/// ones cannot: they compile inside a test project, where <c>ImplicitUsings</c> is on, so a page
/// that forgets <c>using System;</c> passes. Our packages ship for <c>netstandard2.0</c> and
/// <c>net472</c> as well, where there are no implicit usings at all — a page that compiles only
/// under a new SDK's defaults lies to the reader aiming at the floor, who is the reader the floor
/// exists for.
/// </para>
/// <para>
/// So a block is compiled on its own, in a compilation with nothing in it but the references, and
/// with the usings the block itself writes and no others. What comes out is whether the page can be
/// copied, not whether it is right — the values stay with the hand-written tests.
/// </para>
/// <para>
/// A block that is not a compilation unit of its own — a field beside a statement, a line of a
/// method — is a fragment, and D37 has already said what to do with one: the page says what it is
/// missing, and the smallest whole form of it lives in a test. Fragments are listed by the test that
/// uses this, with a reason each, and they are not marked in the page: a shipped document does not
/// carry scaffolding for our tests.
/// </para>
/// </remarks>
static class ShippedPages
{
	/// <summary>The fenced C# of a page, in the order it is written.</summary>
	public static IReadOnlyList<string> Blocks(string page)
	{
		var text = File.ReadAllText(page);

		return Regex
			.Matches(text, @"```csharp\r?\n(.*?)```", RegexOptions.Singleline)
			.Select(one => one.Groups[1].Value)
			.ToList();
	}

	/// <summary>
	/// What a block is compiled with beside itself: the using directives every block before it on
	/// the same page wrote.
	/// </summary>
	/// <remarks>
	/// A page is read in order, and a reader who has copied its first block into a file does not
	/// copy its <c>using</c> again for the second. So a later block stands on what the earlier ones
	/// imported — and on nothing else, which is the point: what no block of the page ever wrote is
	/// not there, however much the test project would have supplied.
	/// </remarks>
	public static string Inherited(IReadOnlyList<string> blocks, int upTo) =>
		string.Concat(blocks
			.Take(upTo)
			.SelectMany(static block => block.Split('\n'))
			.Select(static line => line.TrimEnd('\r'))
			.Where(static line =>
				line.StartsWith("using ", StringComparison.Ordinal) && line.EndsWith(";", StringComparison.Ordinal) &&
				// `using var x = ...;` and `using (…)` are statements, not directives. They begin
				// the same way and carrying them forward makes a later block declare a variable
				// twice and name types the block never imported — the checker's own errors, read
				// as the page's.
				!line.StartsWith("using var ", StringComparison.Ordinal) &&
				!line.StartsWith("using (", StringComparison.Ordinal))
			.Distinct()
			.Select(static line => line + Environment.NewLine));

	/// <summary>Every block of every page, as a theory takes them: the page and which block it is.</summary>
	public static TheoryData<string, int> Every(params string[] pages)
	{
		var data = new TheoryData<string, int>();

		foreach (var page in pages)
			for (var at = 0; at < Blocks(page).Count; at++)
				data.Add(page, at);

		return data;
	}

	/// <summary>
	/// Compiles one block as a file of its own and says what the compiler said, or null where it
	/// said nothing.
	/// </summary>
	/// <remarks>
	/// A console application, because a block of a page is statements and that is the shape C#
	/// gives them. Warnings are not errors here, unlike in the emitter's own harness: this is the
	/// consumer's code, and a consumer's unused variable is their business.
	/// </remarks>
	public static string? Compiles(string code)
	{
		var compilation = CSharpCompilation.Create(
			"DotGram.Tests.Page",
			[CSharpSyntaxTree.ParseText(code, cancellationToken: TestContext.Current.CancellationToken)],
			References,
			new CSharpCompilationOptions(OutputKind.ConsoleApplication));

		var complaints = compilation
			.GetDiagnostics(TestContext.Current.CancellationToken)
			.Where(static one => one.Severity == DiagnosticSeverity.Error)
			.ToArray();

		return complaints.Length == 0
			? null
			: string.Join("\n", complaints.Select(static one => one.ToString()));
	}

	/// <summary>Every assembly this process has loaded, which is what a block of a page may name.</summary>
	/// <remarks>
	/// Its own rather than the emitter harness's, which answers the same question for another
	/// reason: this file is linked into DotGram.Finance.Tests, whose process loads other assemblies
	/// and which does not carry that harness. A shared answer here would have tied two test projects
	/// together to save four lines.
	/// </remarks>
	static ImmutableArray<MetadataReference> References { get; } =
	[
		.. AppDomain.CurrentDomain
			.GetAssemblies()
			.Where(static assembly => !assembly.IsDynamic && !string.IsNullOrEmpty(assembly.Location))
			.Select(static assembly => (MetadataReference)MetadataReference.CreateFromFile(assembly.Location)),
	];

	/// <summary>Where a package's pages are, from a type it ships.</summary>
	public static string PageOf(Type shipped, string name) =>
		Path.Combine(Root, "src", shipped.Assembly.GetName().Name!, name);

	static string Root { get; } = Repository();

	static string Repository()
	{
		var at = new DirectoryInfo(AppContext.BaseDirectory);

		while (at is not null && !File.Exists(Path.Combine(at.FullName, "DotGram.slnx")))
			at = at.Parent;

		return at?.FullName ?? throw new InvalidOperationException("The repository root is not above the test binaries.");
	}
}
