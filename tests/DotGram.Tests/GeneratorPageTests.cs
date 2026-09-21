using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The generator's own pages, which show a grammar where the other packages show calls.
/// </summary>
/// <remarks>
/// <para>
/// D37 asks that an example in a shipped page exist as a test. For these two pages the smallest
/// whole form of an example is not a call but a grammar, so what is held is what a grammar can be
/// held to: the generator compiles it and says nothing — no error, no warning — and what it
/// compiles into builds beside the class the page declares around it.
/// </para>
/// <para>
/// The C# of these pages is compiled as it is written by <see cref="ShippedPages"/>, like every
/// other package's. This is the other half, and it is the half that could not be borrowed: a
/// grammar in a page is a string until something runs the generator over it.
/// </para>
/// </remarks>
public sealed class GeneratorPageTests
{
	[Theory]
	[MemberData(nameof(Grammars))]
	public void A_grammar_a_page_shows_compiles_and_says_nothing(string page, int block)
	{
		var blocks = ShippedPages.Blocks(page);

		// Standing on what the blocks before it imported, as a reader copying them in order would
		// be: the page writes `using DotGram;` once and goes on.
		var code    = ShippedPages.Inherited(blocks, block) + blocks[block];
		var grammar = ShippedPages.GrammarIn(code)!;
		var host    = ShippedPages.HostIn(code)!;

		// What the host declares is what the grammar may name — the tree its factories build, the
		// helpers they call — so the resolver is built over the page's own class, as it would be
		// over a consumer's.
		var declared = CSharpCompilation.Create(
			host,
			[
				CSharpSyntaxTree.ParseText(code, cancellationToken: TestContext.Current.CancellationToken),
				CSharpSyntaxTree.ParseText(GramCompiler.EmitMarkerAttributes().Text, cancellationToken: TestContext.Current.CancellationToken),
			],
			EmittedCode.References,
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

		var compiled = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName      = host,
			CSharpScanner  = RoslynCSharpScanner.Instance,
			SymbolResolver = new RoslynSymbolResolver(declared, host),
			LineMap        = new GrammarLineMap(grammar, Path.GetFileName(page)),
		});

		// A page that shows a grammar the generator complains about is teaching the complaint.
		EmittedCode.Quiet(compiled.Diagnostics);

		var sources = compiled.Sources;

		Assert.NotEmpty(sources);

		// And what it made of it builds beside the page's own class, which is where it lands.
		var built = CSharpCompilation.Create(
			host + ".Built",
			[
				CSharpSyntaxTree.ParseText(code, cancellationToken: TestContext.Current.CancellationToken),
				CSharpSyntaxTree.ParseText(GramCompiler.EmitMarkerAttributes().Text, cancellationToken: TestContext.Current.CancellationToken),
				CSharpSyntaxTree.ParseText(SupportEmitterText, cancellationToken: TestContext.Current.CancellationToken),
				.. sources.Select(one => CSharpSyntaxTree.ParseText(one.Text, cancellationToken: TestContext.Current.CancellationToken)),
			],
			EmittedCode.References,
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

		var complaints = built
			.GetDiagnostics(TestContext.Current.CancellationToken)
			.Where(static one => one.Severity is DiagnosticSeverity.Error or DiagnosticSeverity.Warning)
			.ToArray();

		Assert.True(
			complaints.Length == 0,
			$"{Path.GetFileName(page)} block {block} ({host}):\n" +
			string.Join("\n", complaints.Select(static one => one.ToString())));
	}

	/// <summary>
	/// A block that calls what the generator made compiles beside it, as the reader who copied
	/// the block above it would have.
	/// </summary>
	/// <remarks>
	/// The plain page check cannot read these: they name members no compiler has seen — the page
	/// declared a grammar a block earlier and is now using what it produced. So the grammar above
	/// is run, and the calls are compiled against the class, the generated file and the marker
	/// attributes together.
	/// </remarks>
	[Theory]
	[MemberData(nameof(Uses))]
	public void A_block_that_uses_what_the_generator_made_compiles(string page, int block)
	{
		if (Fragments.TryGetValue((Path.GetFileName(page), block), out var why))
		{
			// A fragment is not a file and is not asked to be one; the reason stands beside it,
			// here rather than in the page, which carries no scaffolding for our tests (D37).
			Assert.NotNull(why);

			return;
		}

		var blocks  = ShippedPages.Blocks(page);
		var above   = Above(blocks, block);
		var host    = ShippedPages.Inherited(blocks, above) + blocks[above];
		var grammar = ShippedPages.GrammarIn(host)!;

		var compiled = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName     = ShippedPages.HostIn(host)!,
			CSharpScanner = RoslynCSharpScanner.Instance,
			SymbolResolver = new RoslynSymbolResolver(
				CSharpCompilation.Create(
					"Host",
					[
						CSharpSyntaxTree.ParseText(host, cancellationToken: TestContext.Current.CancellationToken),
						CSharpSyntaxTree.ParseText(GramCompiler.EmitMarkerAttributes().Text, cancellationToken: TestContext.Current.CancellationToken),
					],
					EmittedCode.References,
					new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)),
				ShippedPages.HostIn(host)!),
		});

		EmittedCode.Quiet(compiled.Diagnostics);

		var uses = ShippedPages.Inherited(blocks, block) + blocks[block];

		var built = CSharpCompilation.Create(
			"Uses",
			[
				CSharpSyntaxTree.ParseText(uses, cancellationToken: TestContext.Current.CancellationToken),
				CSharpSyntaxTree.ParseText(host, cancellationToken: TestContext.Current.CancellationToken),
				CSharpSyntaxTree.ParseText(GramCompiler.EmitMarkerAttributes().Text, cancellationToken: TestContext.Current.CancellationToken),
				CSharpSyntaxTree.ParseText(SupportEmitterText, cancellationToken: TestContext.Current.CancellationToken),
				.. compiled.Sources.Select(one => CSharpSyntaxTree.ParseText(one.Text, cancellationToken: TestContext.Current.CancellationToken)),
			],
			EmittedCode.References,
			new CSharpCompilationOptions(OutputKind.ConsoleApplication));

		var complaints = built
			.GetDiagnostics(TestContext.Current.CancellationToken)
			.Where(static one => one.Severity == DiagnosticSeverity.Error)
			.ToArray();

		Assert.True(
			complaints.Length == 0,
			$"{Path.GetFileName(page)} block {block}:" + Environment.NewLine +
			string.Join(Environment.NewLine, complaints.Select(static one => one.ToString())) +
			Environment.NewLine + "----" + Environment.NewLine + uses);
	}

	/// <summary>The blocks that are not files of their own, and why each one is not.</summary>
	static readonly Dictionary<(string Page, int Block), string> Fragments = new()
	{
		[("SKILL.md", 3)] =
			"The attribute's options with the grammar itself elided: the page is showing what may be " +
			"written beside a grammar, and writing one out there would bury the list it is making. " +
			"Each option in it is held by a test of its own elsewhere.",
	};

	/// <summary>The block a use stands on: the nearest one above it that declares a grammar.</summary>
	static int Above(IReadOnlyList<string> blocks, int block)
	{
		for (var at = block - 1; at >= 0; at--)
			if (ShippedPages.GrammarIn(blocks[at]) is not null && ShippedPages.HostIn(blocks[at]) is not null)
				return at;

		throw new InvalidOperationException($"Block {block} uses what nothing above it declared.");
	}

	/// <summary>Every block that calls what a grammar above it produced.</summary>
	public static TheoryData<string, int> Uses
	{
		get
		{
			var data = new TheoryData<string, int>();

			foreach (var page in new[] { Page("README.md"), Page("SKILL.md") })
			{
				var blocks = ShippedPages.Blocks(page);
				var seen   = false;

				for (var at = 0; at < blocks.Count; at++)
					if (ShippedPages.GrammarIn(blocks[at]) is not null && ShippedPages.HostIn(blocks[at]) is not null)
						seen = true;
					else if (seen && ShippedPages.HostIn(blocks[at]) is null)
						data.Add(page, at);
			}

			return data;
		}
	}

	/// <summary>What the generator asks Roslyn for beside the marker attributes, as every build brings it.</summary>
	static string SupportEmitterText { get; } = DotGram.Grammar.Emit.SupportEmitter.EmbeddedAttribute;

	/// <summary>Every block of either page that holds a grammar.</summary>
	public static TheoryData<string, int> Grammars
	{
		get
		{
			var data = new TheoryData<string, int>();

			foreach (var page in new[] { Page("README.md"), Page("SKILL.md") })
			{
				var blocks = ShippedPages.Blocks(page);

				for (var at = 0; at < blocks.Count; at++)
					if (ShippedPages.GrammarIn(blocks[at]) is not null && ShippedPages.HostIn(blocks[at]) is not null)
						data.Add(page, at);
			}

			return data;
		}
	}

	static string Page(string name)
	{
		return ShippedPages.PageOf(typeof(GramCompiler), name);
	}
}
