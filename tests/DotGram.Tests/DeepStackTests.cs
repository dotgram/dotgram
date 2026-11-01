using System;
using System.Linq;
using System.Reflection;
using System.Threading;

using DotGram.Grammar;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// What the generator writes is the same whether or not its deep walks move to a fresh stack.
/// </summary>
/// <remarks>
/// <para>
/// A walk that runs low on stack carries on on another thread (<c>DeepStack</c>), and anything it
/// kept per thread outside the one object that goes with it would be empty there: the first
/// version of the move lost the tests of Unicode categories and the methods that call the host's,
/// which the file declares at its end from what was recorded while it was written, and a grammar
/// as small as <c>A = [\p{Lu}] &amp; 'x'</c> came out calling a method it never declared.
/// </para>
/// <para>
/// So each grammar here is compiled twice, once as it always is and once with every walk that can
/// move made to move at every other step, read by methods and by the engine, and the two files are
/// held to be the same to the byte; the moved one is also compiled, with the host's members.
/// </para>
/// </remarks>
public sealed class DeepStackTests
{
	/// <summary>The host's members the grammars below call.</summary>
	const string Members = """
		static bool IsVowel(char c) { return "aeiou".IndexOf(c) >= 0; }
		static bool IsStop(char c) { return c == '.'; }
		static bool Blob(global::System.ReadOnlySpan<char> text, ref int at)
		{
			if (at < text.Length && text[at] == '#')
			{
				at++;

				return true;
			}

			return false;
		}
		""";

	public static TheoryData<string, bool, bool> Grammars()
	{
		var data = new TheoryData<string, bool, bool>();

		foreach (var grammar in new[]
		{
			// A test of categories, declared at the end of the file from what was recorded.
			"A = [\\p{Lu}] & 'x'\nparse A",

			// The host's predicates and recognizer, each called through a method the file declares.
			"Start = ([@IsVowel] | ['0'..'9'])+ & @Blob & [@IsStop]\nparse Start",

			// Values, built through calls.
			"Num : @int = d:['0'..'9']+ => @(global::System.Int32.Parse(d))\n" +
			"Sum : @int = n:Num & ('+' & Num)* => @(n)\nparse Sum",

			// A seam, its scanner, and a list.
			"trivia = { ' '* }\nWord = ['a'..'z']+\nList = Word & (',' & Word)*\nparse List",

			// A rule that reaches itself.
			"E = '(' & E & ')' | 'x' & [\\p{Ll}]?\nparse E",
		})
		{
			data.Add(grammar, true, false);
			data.Add(grammar, false, false);
			data.Add(grammar, true, true);
		}

		return data;
	}

	[Theory]
	[MemberData(nameof(Grammars))]
	public void A_walk_that_moves_to_a_fresh_stack_writes_what_it_would_have_written(string grammar, bool direct, bool lexical)
	{
		var stayed = Compiled(grammar, direct, lexical, moving: false);
		var moved  = Compiled(grammar, direct, lexical, moving: true);

		Assert.Equal(stayed, moved);

		EmittedCode.Compile(moved, declarationMembers: Members);
	}

	static string Compiled(string grammar, bool direct, bool lexical, bool moving)
	{
		var always = (AsyncLocal<bool>)typeof(GramCompiler).Assembly.GetType("DotGram.Grammar.DeepStack", throwOnError: true)!
			.GetField("Always", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;

		always.Value = moving;

		try
		{
			var compiled = GramCompiler.Compile(grammar, new GramCompilerOptions
			{
				CSharpScanner = RoslynCSharpScanner.Instance,
				Direct        = direct,
				Lexical       = lexical,
			});

			Assert.False(
				compiled.HasErrors,
				string.Join("\n", compiled.Diagnostics.Where(static one => one.Severity == GramSeverity.Error)));

			return Assert.Single(compiled.Sources).Text;
		}
		finally
		{
			always.Value = false;
		}
	}
}
