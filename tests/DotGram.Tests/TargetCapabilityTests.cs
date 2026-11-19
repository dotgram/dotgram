using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The forks that stand on what a target language can do, each read by a second target before there
/// is one: a C# made to answer one capability otherwise, whose parser is still C# and so compiles
/// and runs here.
/// </summary>
/// <remarks>
/// Three things are held of a fork that has a second way to write the parser: the other way is taken
/// (alive), it reads what the first reads (correct), and where there is no other way the grammar is
/// told at the publication (loud). This proves the fork in the stages every target shares; it says
/// nothing of how another language's runtime behaves.
/// </remarks>
public class TargetCapabilityTests
{
	/// <summary>
	/// <c>Name</c> matches <c>x</c> first where the input is <c>xy</c>; the reading has to come back
	/// into it for <c>xy</c>. C# reads this by a flat machine, which is states and jumps.
	/// </summary>
	const string Resumed = """
		Start = Name & 'y'
		Name  = "xy" | "x"
		parse Start
		""";

	/// <summary>A sequence rule: C# offers it a window at a time as well, and reads it by the jump machine for that.</summary>
	const string Rows = """
		Rows : @string[] = Row* & eof
		Row  : @string = name: ['a'..'z']+ & ',' & amount: ['0'..'9']+ & eol => @(name + "=" + amount)
		parse Rows
		""";

	[Theory]
	[InlineData(Resumed, "TryParseStart", "xyy", "xy", "x", "", "xyz")]
	[InlineData(Rows, "TryParseRows", "ab,1\ncd,22\n", "", "ab,1", "ab,\n", "ab,1\nc")]
	public void Without_jumps_the_reader_reads_what_a_machine_of_jumps_read(string grammar, string method, params string[] inputs)
	{
		var (withJumps, said)       = Compiled(grammar, CSharp);
		var (withoutJumps, saidToo) = Compiled(grammar, WithoutJumps);

		EmittedCode.Quiet(said);
		EmittedCode.Quiet(saidToo);

		// Alive: C# takes a machine of jumps for this, and the target without jumps does not.
		Assert.Contains("goto ", withJumps, StringComparison.Ordinal);
		Assert.DoesNotContain("goto ", withoutJumps, StringComparison.Ordinal);

		// Correct: what it reads, and where it refuses, is what C#'s reads and where C#'s refuses. Not
		// the words of the refusal: the flat machine and the reader name what was expected at one
		// position differently ("xy" or 'x' against ['x']), which is theirs and not the target's.
		var first  = EmittedCode.Compile(withJumps);
		var second = EmittedCode.Compile(withoutJumps);

		foreach (var input in inputs)
		{
			var expected = EmittedCode.Match(first, "Grammar", method, input);
			var actual   = EmittedCode.Match(second, "Grammar", method, input);

			Assert.Equal(expected.IsSuccess, actual.IsSuccess);
			Assert.Equal(expected.Position, actual.Position);
		}
	}

	/// <summary>A <c>find</c> is read by the jump machine and by nothing else: without jumps it is refused where it is published.</summary>
	[Fact]
	public void Without_jumps_a_publication_only_jumps_read_is_refused_where_it_is_published()
	{
		const string grammar = """
			Name = ['a'..'z']+
			find Name
			""";

		var result = GramCompiler.Compile(grammar, new GramCompilerOptions { Language = WithoutJumps });
		var told   = Assert.Single(result.Diagnostics, static one => one.Severity == GramSeverity.Error);

		Assert.Equal("GRAM5020", told.Id);
		Assert.Equal(grammar.IndexOf("find Name", StringComparison.Ordinal), told.Position);
		Assert.Contains("'Name'", told.Message, StringComparison.Ordinal);

		// And C# itself, which has jumps, is told nothing.
		Assert.DoesNotContain(
			GramCompiler.Compile(grammar, new GramCompilerOptions { Language = CSharp }).Diagnostics,
			static one => one.Id == "GRAM5020");
	}

	/// <summary>
	/// The lexer's tables are UTF-8 literals where the author asks and the target can write them;
	/// where it cannot they are text, exactly as where the author did not ask.
	/// </summary>
	[Fact]
	public void Without_byte_literals_the_tables_asked_for_as_bytes_are_text()
	{
		var grammar = System.IO.File.ReadAllText(System.IO.Path.Combine(SourceRoot.Directory, "tests", "Snapshots", "Lexical.gram"));

		string Emitted(CSharpLanguage language, bool utf8)
		{
			var result = GramCompiler.Compile(grammar, new GramCompilerOptions { Language = language, Lexical = true, Utf8Literals = utf8 });

			Assert.DoesNotContain(result.Diagnostics, static one => one.Severity == GramSeverity.Error);

			return string.Join("\n", result.Sources.Select(static one => one.Text));
		}

		var asked    = Emitted(CSharp, utf8: true);
		var notAsked = Emitted(CSharp, utf8: false);

		Assert.NotEqual(asked, notAsked);
		Assert.Equal(notAsked, Emitted(WithoutByteLiterals, utf8: true));
	}

	/// <summary>
	/// Which capabilities a compilation asks about: the jumps always, since every file chooses its
	/// machines, and the byte literals only where the author asked for them.
	/// </summary>
	[Fact]
	public void A_compilation_asks_only_about_what_it_decides()
	{
		HashSet<string> Asked(bool utf8)
		{
			var reads = new HashSet<string>(StringComparer.Ordinal);

			GramCompiler.Compile(Resumed, new GramCompilerOptions { Language = CSharp.With(CSharp.Can.Recording(reads)), Utf8Literals = utf8 });

			return reads;
		}

		Assert.Equal([nameof(TargetCapabilities.UnstructuredJumps)], Asked(utf8: false));
		Assert.Equal(
			[nameof(TargetCapabilities.ByteStringLiterals), nameof(TargetCapabilities.UnstructuredJumps)],
			Asked(utf8: true).Order(StringComparer.Ordinal));
	}

	static (string Text, IReadOnlyList<GramDiagnostic> Diagnostics) Compiled(string grammar, CSharpLanguage language)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions { Language = language });

		return (Assert.Single(result.Sources).Text, result.Diagnostics);
	}

	static readonly CSharpLanguage CSharp = new(scanner: RoslynCSharpScanner.Instance);

	static readonly CSharpLanguage WithoutJumps = CSharp.With(new TargetCapabilities
	{
		UnstructuredJumps  = false,
		ByteStringLiterals = true,
	});

	static readonly CSharpLanguage WithoutByteLiterals = CSharp.With(new TargetCapabilities
	{
		UnstructuredJumps  = true,
		ByteStringLiterals = false,
	});
}
