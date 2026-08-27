using System;
using System.Text;

using BenchmarkDotNet.Attributes;

using DotGram;

namespace DotGram.Benchmarks;

/// <summary>One setting of the document: what the grammar's <c>=&gt;</c> builds.</summary>
public sealed class Setting(string key, string value)
{
	public string Key   { get; } = key;
	public string Value { get; } = value;
}

/// <summary>
/// A document-shaped grammar: spacing and comments between every operand, records
/// collected in reading order, values that are spans of the input.
/// </summary>
/// <remarks>
/// <para>
/// The URL grammar measures a dense, spacing-free line against regular expressions, which
/// is what a reader comparing engines wants. This measures the other everyday shape and the
/// one most grammars actually are: a file, read with a seam at every operand, where the
/// trivia rule is applied far more often than anything else in the grammar and most of its
/// applications find nothing to skip.
/// </para>
/// <para>
/// So it is the instrument for a whole class of work the URL numbers cannot see: what a
/// seam costs when there is no trivia at it, what one costs when there is a comment, and
/// what a record costs that is built from spans rather than from a call's own boundary.
/// Three inputs of the same length say which of the three is being paid for.
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class Documents
{
	const int Entries = 400;

	/// <summary>Nothing to skip at any seam — the commonest case, and the fastest to get wrong.</summary>
	static readonly string Dense = Build(comments: 0, spaced: false);

	/// <summary>A space at every seam, and no comments: the ordinary formatted file.</summary>
	static readonly string Spaced = Build(comments: 0, spaced: true);

	/// <summary>Every fourth entry behind a line comment, every eighth behind a block one.</summary>
	static readonly string Commented = Build(comments: 1, spaced: true);

	static string Build(int comments, bool spaced)
	{
		var text = new StringBuilder();

		for (var i = 0; i < Entries; i++)
		{
			if (comments > 0 && i % 4 == 0)
				text.Append("// setting ").Append(i).Append('\n');

			if (comments > 0 && i % 8 == 0)
				text.Append("/* was ").Append(i - 1).Append(" before the review */\n");

			text.Append("key").Append(i);

			if (spaced)
				text.Append(' ');

			text.Append('=');

			if (spaced)
				text.Append(' ');

			text.Append("value").Append(i).Append(';');

			if (spaced)
				text.Append('\n');
		}

		return text.ToString();
	}

	[Benchmark(Baseline = true, Description = "dense, no seams to skip")]
	public int Dense_() => Config.Read(Dense).Length;

	[Benchmark(Description = "spaced")]
	public int Spaced_() => Config.Read(Spaced).Length;

	[Benchmark(Description = "spaced, with comments")]
	public int Commented_() => Config.Read(Commented).Length;
}

/// <summary>
/// The grammar itself: §4.5's advice taken, so the seam is a rule and every operand is
/// separated by it.
/// </summary>
[Gram("""
	@using DotGram.Benchmarks;

	wordboundary = ['a'..'z' | 'A'..'Z' | '0'..'9' | '_']

	// Nothing between the characters of a token, which is what makes these the
	// lexemes the seam is made of rather than rules the seam is woven into.
	namespace Lexical
	{
		trivia = none

		Space        = [' ' | '\t' | '\r' | '\n']
		LineComment  = "//" & [^ '\n' | '\r']*
		BlockComment = "/*" & (?!"*/" & any)* & "*/"
	}

	trivia = { (Lexical.Space | Lexical.LineComment | Lexical.BlockComment)* }

	Name  : @string = t: ['a'..'z' | 'A'..'Z' | '0'..'9' | '_']+ => @(t)
	Value : @string = t: ['a'..'z' | 'A'..'Z' | '0'..'9']+  => @(t)

	Entry : @Setting  = key: Name & '=' & value: Value & ';' => @(new Setting(key, value))
	File  : @Setting[] = entries: Entry* & eof               => @(entries)

	parse File
	""")]
public partial class Config
{
	public static Setting[] Read(string text) => ParseFile(text);
}
