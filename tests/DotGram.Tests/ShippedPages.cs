using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
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
/// <para>
/// **This file is a shared agreement, and is linked into the test project of every package rather
/// than copied.** What it accepts and rejects is the rule about pages, so changing that is changing
/// the rule: mending a mistake in it is mending, but moving the line between a page that passes and
/// a page that does not goes past the architect first. Otherwise the rule belongs to whoever
/// touched the file last, and the pages of four packages are held to it.
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
	public static string Inherited(IReadOnlyList<string> blocks, int upTo)
	{
		// What the block writes itself is not written for it again: a page that repeats an import
		// where it wants to be read on its own is not making a mistake, and a using written twice
		// is a warning, which here is a failure.
		var own = new HashSet<string>(Usings(blocks[upTo]), StringComparer.Ordinal);

		return string.Concat(blocks
			.Take(upTo)
			.SelectMany(Usings)
			.Distinct()
			.Where(line => !own.Contains(line))
			.Select(static line => line + Environment.NewLine));
	}

	/// <summary>The using directives a block writes, as it writes them.</summary>
	static IEnumerable<string> Usings(string block)
	{
		return block
			.Split('\n')
			.Select(static line => line.TrimEnd('\r'))
			.Where(static line =>
				line.StartsWith("using ", StringComparison.Ordinal) && line.EndsWith(";", StringComparison.Ordinal) &&
				// `using var x = ...;` and `using (…)` are statements, not directives. They begin
				// the same way and carrying them forward makes a later block declare a variable
				// twice and name types the block never imported — the checker's own errors, read
				// as the page's.
				!line.StartsWith("using var ", StringComparison.Ordinal) &&
				!line.StartsWith("using (", StringComparison.Ordinal));
	}

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

	/// <summary>Everything this test assembly names, which is what a block of a page may name.</summary>
	/// <remarks>
	/// <para>
	/// Its own rather than the emitter harness's, which answers the same question for another
	/// reason: this file is linked into other packages' test projects, whose processes load other
	/// assemblies and which do not carry that harness. A shared answer here would have tied two test
	/// projects together to save four lines.
	/// </para>
	/// <para>
	/// **What this must not be is "every assembly the process has loaded".** It was that, and it
	/// made the answer depend on which test ran first: a reference assembly is loaded lazily, at its
	/// first use, so whether the assembly a page names was in the list depended on whether some
	/// earlier test in the same process had happened to touch it. The check then answered a question
	/// about load order rather than about the page, and passed by luck — which is a failure waiting
	/// for the day someone else's change reorders the run, and it would be looked for in their
	/// change.
	/// </para>
	/// <para>
	/// So the set is named rather than gathered: everything the test assembly references,
	/// transitively, is loaded first, and the list is taken after that. What a page may name is what
	/// its own test project was built against, which is the same set on every run and in every
	/// order.
	/// </para>
	/// </remarks>
	static ImmutableArray<MetadataReference> References { get; } = Named();

	static ImmutableArray<MetadataReference> Named()
	{
		var seen  = new HashSet<string>(StringComparer.Ordinal);
		var queue = new Queue<Assembly>();

		queue.Enqueue(typeof(ShippedPages).Assembly);

		while (queue.Count > 0)
		{
			var one = queue.Dequeue();

			if (!seen.Add(one.FullName ?? one.ToString()))
				continue;

			foreach (var named in one.GetReferencedAssemblies())
			{
				try
				{
					queue.Enqueue(Assembly.Load(named));
				}
				catch (Exception)
				{
					// A reference the runtime cannot resolve is one no page can name either, so it
					// is not this check's business to fail over it.
				}
			}
		}

		return
		[
			.. AppDomain.CurrentDomain
				.GetAssemblies()
				.Where(static assembly => !assembly.IsDynamic && !string.IsNullOrEmpty(assembly.Location))
				.Select(static assembly => (MetadataReference)MetadataReference.CreateFromFile(assembly.Location)),
		];
	}

	/// <summary>The grammar a block holds inside its <c>[Gram(…)]</c>, or null where it holds none.</summary>
	/// <remarks>
	/// Taken as text rather than compiled as C#, so the indentation a raw string would strip is
	/// stripped here too: what the generator is handed has to be what the reader's compiler would
	/// hand it, and a grammar indented one tab further than it says is a different grammar.
	/// </remarks>
	public static string? GrammarIn(string block)
	{
		var opens = block.IndexOf("[Gram(" + Quotes, StringComparison.Ordinal);

		if (opens < 0)
			return null;

		var lines  = block.Substring(opens).Split('\n').Select(static line => line.TrimEnd('\r')).ToList();
		var closes = lines.FindIndex(static line => line.TrimStart().StartsWith(Quotes + ")", StringComparison.Ordinal));

		if (closes < 0)
			return null;

		var margin = lines[closes].Length - lines[closes].TrimStart().Length;

		return string.Join(
			Environment.NewLine,
			lines
				.Skip(1)
				.Take(closes - 1)
				.Select(line => line.Length >= margin ? line.Substring(margin) : line.TrimStart()));
	}

	/// <summary>The class a block declares, which is the host a grammar of that block is compiled against.</summary>
	public static string? HostIn(string block)
	{
		return Regex.Match(block, "partial class ([A-Za-z_][A-Za-z0-9_]*)") is { Success: true } named
			? named.Groups[1].Value
			: null;
	}

	/// <summary>The three quotes a raw string opens and closes with.</summary>
	const string Quotes = "\"\"\"";

	/// <summary>Where a package's pages are, from a type it ships.</summary>
	public static string PageOf(Type shipped, string name)
	{
		return Path.Combine(Root, "src", shipped.Assembly.GetName().Name!, name);
	}

	static string Root { get; } = Repository();

	static string Repository()
	{
		var at = new DirectoryInfo(AppContext.BaseDirectory);

		while (at is not null && !File.Exists(Path.Combine(at.FullName, "DotGram.slnx")))
			at = at.Parent;

		return at?.FullName ?? throw new InvalidOperationException("The repository root is not above the test binaries.");
	}
}
