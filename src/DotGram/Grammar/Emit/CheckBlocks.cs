using System;
using System.Text;

namespace DotGram.Grammar.Emit;

/// <summary>
/// The walk's invariants, written into a generated file only where the compilation asks for them.
/// </summary>
/// <remarks>
/// <para>
/// The emitter writes every check inside <c>#if DOTGRAM_CHECKS</c>, a symbol no consumer defines, so
/// in a consumer's build the text is there and compiles to nothing: hundreds of kilobytes of a large
/// grammar parsed on every build for no code. Where the compilation does not define the symbol, each
/// such block goes here, directive lines and all, which changes nothing that runs; where it does, the
/// block is kept as written, under the same directive, so the symbol still decides.
/// </para>
/// <para>
/// A pass over the finished text rather than a question at each place that writes a check, for the
/// same reason the documentation is settled at the end (<see cref="PublicDocs"/>): one place, and
/// nothing a writer has to remember. A block holds no <c>#else</c>, and the pass refuses one rather
/// than guess which branch is meant.
/// </para>
/// </remarks>
static class CheckBlocks
{
	const string Opens = "#if DOTGRAM_CHECKS";

	/// <summary><paramref name="text"/> with its check blocks, or without them where <paramref name="checks"/> is false.</summary>
	public static string Settle(string text, bool checks)
	{
		if (checks || text.IndexOf(Opens, StringComparison.Ordinal) < 0)
			return text;

		var output = new StringBuilder(text.Length);
		var inside = 0;
		var at     = 0;

		while (at < text.Length)
		{
			var end  = text.IndexOf('\n', at);
			var next = end < 0 ? text.Length : end + 1;
			var code = text.Substring(at, next - at).Trim();

			if (inside == 0 && code == Opens)
				inside = 1;
			else if (inside > 0 && code.StartsWith("#if", StringComparison.Ordinal))
				inside++;
			else if (inside == 1 && (code.StartsWith("#else", StringComparison.Ordinal) || code.StartsWith("#elif", StringComparison.Ordinal)))
				throw new InvalidOperationException("A DOTGRAM_CHECKS block holds an #else, which the generator does not write.");
			else if (inside > 0 && code.StartsWith("#endif", StringComparison.Ordinal))
				inside--;
			else if (inside == 0)
				output.Append(text, at, next - at);

			at = next;
		}

		return output.ToString();
	}
}
