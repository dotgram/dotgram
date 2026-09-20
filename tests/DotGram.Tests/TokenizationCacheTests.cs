using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Emit;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The cutting a reading is using is never handed to another reading.
/// </summary>
/// <remarks>
/// A reading that begins where it is told cuts the whole input and keeps the cutting for
/// the next reading of the same string. Two strings are kept, and a third pushes the first
/// out. What the reading that is still holding the first one must not see is its cutting
/// handed to the reading that pushed it out: the outer reading goes on reading kinds and
/// slicing text by offsets that now belong to another text, and answers — without refusing
/// and without throwing — about a document it never read.
/// </remarks>
public sealed class TokenizationCacheTests
{
	/// <summary>Ten words, and the same text as many characters as <see cref="Deeper"/>'s last.</summary>
	const string Outer = "aaa bbb ccc ddd eee fff ggg hhh iii jjj";

	[Theory]
	[InlineData(CarrierKind.Tape)]
	[InlineData(CarrierKind.Immediate)]
	public void A_reading_inside_another_does_not_take_the_cutting_it_is_reading(CarrierKind carrier)
	{
		// Three readings inside the outer one: the first two take the two slots, and the third
		// pushes the outer reading's own cutting out of the one it took.
		const string Deeper = """
			public static bool Nested;

			public static readonly string[] Inner =
			{
				"bb",
				"cc",
				"z z z z z z z z z z z z z z z z z z z",
			};

			public static string Read(string token)
			{
				if (!Nested)
				{
					Nested = true;

					foreach (var text in Inner)
					{
						var at = 0;

						TryParseStart(text, ref at, out _);
					}
				}

				return token;
			}
			""";

		var compiled = GramCompiler.Compile("""
			wordboundary = ['a'..'z']
			trivia = { ' '* }
			namespace Tokens
			{
				trivia = none
				Name : @string = ['a'..'z'] & ['a'..'z']* => @(parserText)
			}
			Start : @string = w: Word+ => @(string.Concat(w))
			Word  : @string = t: Tokens.Name => @(Read(t))
			parse Start
			""", new GramCompilerOptions
		{
			ClassName = "Grammar", Lexical = true, Carrier = carrier,
			CSharpScanner = RoslynCSharpScanner.Instance,
		});

		EmittedCode.Quiet(compiled.Diagnostics);

		var source = compiled.Sources.Single().Text;

		// The grammar is read over kinds, so there is a cutting to keep in the first place.
		Assert.Contains("Tokenized_DotGram", source);

		var assembly = EmittedCode.Compile(source, declarationMembers: Deeper);
		var answer   = EmittedCode.Answered(assembly, "Grammar", "TryParseStart", Outer, 0);

		Assert.True(answer.Read);
		Assert.Equal(string.Concat(Outer.Split(' ')), answer.Value);
		Assert.Equal(Outer.Length, answer.At);
	}
}
