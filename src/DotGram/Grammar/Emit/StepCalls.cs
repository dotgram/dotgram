using System;
using System.Collections.Generic;

namespace DotGram.Grammar.Emit;

/// <summary>
/// The engine's step trace, written into a generated file only where the compilation asks for it.
/// </summary>
/// <remarks>
/// <para>
/// The emitter writes a call of <c>Step_DotGram(...)</c> at every step of the automaton, and the support
/// code the methods it calls, marked <c>[Conditional("DOTGRAM_STEPS")]</c>, so that in a consumer's
/// build the text is there and compiles to nothing: tens of kilobytes of a large grammar parsed on
/// every build for no code. Where the compilation does not define the symbol, each such call and
/// each such method goes here, which changes nothing that runs; where it does, the text is kept as
/// written, under the same attribute, so the symbol still decides.
/// </para>
/// <para>
/// A pass over the finished text, for the same reason as <see cref="CheckBlocks"/>. A call is a
/// statement on lines of its own, and the name it calls ends in <c>_DotGram</c> as every name the
/// generator keeps for itself does, so that a host's own <c>Trace</c> in an expression it hands
/// across is never taken for one; a line of a host's multi-line string literal that is itself such a call
/// of a name reserved for the generator is the one thing this does not tell apart. The pass refuses a call that is the whole body of a statement
/// above it, which removing would give a different body, rather than guess.
/// </para>
/// </remarks>
static class StepCalls
{
	const string Call = "Step_DotGram(";

	const string Marked = "[global::System.Diagnostics.Conditional(\"DOTGRAM_STEPS\")]";

	/// <summary><paramref name="text"/> with its step calls and their methods, or as it is where <paramref name="steps"/> is true.</summary>
	public static string Settle(string text, bool steps)
	{
		if (steps || (text.IndexOf(Call, StringComparison.Ordinal) < 0 && text.IndexOf(Marked, StringComparison.Ordinal) < 0))
			return text;

		var lines  = text.Split('\n');
		var kept   = new List<string>(lines.Length);
		var at     = 0;

		while (at < lines.Length)
		{
			var line = lines[at];
			var code = line.Trim();

			if (code == Marked)
			{
				// The method: its documentation above, then the lines to the brace that closes it.
				while (kept.Count > 0 && kept[^1].Trim().StartsWith("///", StringComparison.Ordinal))
					kept.RemoveAt(kept.Count - 1);

				var indent = line.Substring(0, line.Length - line.TrimStart().Length);

				at++;
				while (at < lines.Length && lines[at].TrimEnd('\r') != indent + "}")
					at++;

				at++;

				// And the blank line that parted it from what follows.
				if (at < lines.Length && lines[at].Trim().Length == 0 && kept.Count > 0 && kept[^1].Trim().Length == 0)
					at++;

				continue;
			}

			if (code.StartsWith(Call, StringComparison.Ordinal))
			{
				var previous = kept.Count > 0 ? kept[^1].Trim() : "";

				if (Heads(previous))
					throw new InvalidOperationException("A step call is the whole body of a statement, which removing it would change.");

				while (!lines[at].TrimEnd().EndsWith(";", StringComparison.Ordinal))
					at++;

				at++;
				continue;
			}

			kept.Add(line);
			at++;
		}

		return string.Join("\n", kept);
	}

	/// <summary>Whether <paramref name="code"/> opens a statement whose body is the next one, with no brace.</summary>
	static bool Heads(string code)
	{
		code = code.TrimEnd('\r');

		return code == "else" ||
			((code.StartsWith("if (", StringComparison.Ordinal) || code.StartsWith("else if (", StringComparison.Ordinal) ||
			  code.StartsWith("while (", StringComparison.Ordinal) || code.StartsWith("for (", StringComparison.Ordinal) ||
			  code.StartsWith("foreach (", StringComparison.Ordinal)) && code.EndsWith(")", StringComparison.Ordinal));
	}
}
