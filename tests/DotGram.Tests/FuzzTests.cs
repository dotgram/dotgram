using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// One property, over grammars nobody wrote on purpose.
/// </summary>
/// <remarks>
/// <para>
/// A compiler for a language people type has to answer every text, and the two acceptable
/// answers are a parser and a diagnostic. What it may never do is fail in a way that is
/// nobody's fault but its own: an index out of range, a null, a stack that runs out. The
/// Roslyn shell catches those and reports `GRAM0001`, which keeps a consumer's build alive
/// and tells them nothing they can act on — so it is a last resort and not a design.
/// </para>
/// <para>
/// The grammars come from mutating ones that are known good, a character at a time. That
/// finds a different kind of defect from writing tests by hand: what breaks a compiler is
/// rarely a feature, it is two features meeting. The atomic group that lost its captures
/// was exactly that — atomic worked, capture worked, and neither test had a reason to put
/// them together.
/// </para>
/// <para>
/// Seeded, so a failure is a failure again tomorrow, and the seed is in the message.
/// </para>
/// </remarks>
public sealed class FuzzTests
{
	/// <summary>Characters a grammar is made of, for insertions to reach for.</summary>
	const string Alphabet = "abz09 \t\r\n:=|&()[]{}*+?!<>'\"@.,;-_\\/^#$~";

	[Theory]
	[InlineData(1)]
	[InlineData(2)]
	[InlineData(3)]
	[InlineData(4)]
	[InlineData(5)]
	[InlineData(6)]
	[InlineData(7)]
	[InlineData(8)]
	[InlineData(9)]
	[InlineData(10)]
	[InlineData(11)]
	[InlineData(12)]
	public void A_mutated_grammar_is_answered_rather_than_survived(int seed)
	{
		var random = new Random(seed);
		var seeds  = Seeds();

		for (var round = 0; round < 400; round++)
		{
			var start   = seeds[random.Next(seeds.Count)];
			var mutated = Mutate(start, random);

			try
			{
				// Everything up to and including emission, because a defect that only shows
				// while writing the parser out is still a defect the consumer would meet.
				GramCompiler.Compile(mutated, new GramCompilerOptions { ClassName = "Fuzz" });
			}
			catch (Exception failure)
			{
				// The whole exception, stack and all. A failure here says a place in the compiler
				// indexed something it should have checked, and the place is the only part of that
				// worth having.
				Assert.Fail(
					$"seed {seed}, round {round}\n\n{failure}\n\n--- grammar ---\n" + mutated);
			}
		}
	}

	/// <summary>The grammars checked in beside their generated output, as starting points.</summary>
	static IReadOnlyList<string> Seeds()
	{
		var texts = new List<string>();

		foreach (var path in Directory.GetFiles(
			Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Snapshots"), "*.gram"))
		{
			texts.Add(File.ReadAllText(path));
		}

		Assert.NotEmpty(texts);

		return texts;
	}

	/// <summary>
	/// One to three edits of a single character each: what a half-finished line looks like.
	/// </summary>
	/// <remarks>
	/// Small on purpose. A text mangled beyond recognition is refused by the lexer and never
	/// reaches the parts of the compiler worth testing; a text one character from correct
	/// gets all the way in, which is where two features meet.
	/// </remarks>
	static string Mutate(string text, Random random)
	{
		var built = new StringBuilder(text);

		for (var edit = random.Next(1, 4); edit > 0; edit--)
		{
			var at = random.Next(built.Length);

			switch (random.Next(3))
			{
				case 0:
					built.Remove(at, 1);
					break;

				case 1:
					built.Insert(at, Alphabet[random.Next(Alphabet.Length)]);
					break;

				default:
					built[at] = Alphabet[random.Next(Alphabet.Length)];
					break;
			}

			if (built.Length == 0)
				return "";
		}

		return built.ToString();
	}
}
