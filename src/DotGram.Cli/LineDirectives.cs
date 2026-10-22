using System;
using System.Text;

namespace DotGram.Cli;

/// <summary>
/// The paths in a generated file's <c>#line</c> directives, made relative to the folder the file is
/// written into.
/// </summary>
/// <remarks>
/// <para>
/// The generator writes the full path of the grammar (or of the C# file an inline grammar is in)
/// into every <c>#line</c>, which is right inside a build and wrong in a file that is committed and
/// cloned somewhere else. The C# compiler reads a relative <c>#line</c> path against the directory
/// of the file holding it, so a path relative to the output folder sends an error in a hook, and
/// the debugger, to the real grammar wherever the repository is.
/// </para>
/// <para>
/// Only lines that are exactly <c>#line n "path"</c>, and only where the path is one of the files
/// the compilation read: a hook's string that happens to hold <c>#line 1 "x"</c> on a line of its
/// own names no input and is left as it is. Every other byte is untouched.
/// </para>
/// </remarks>
static class LineDirectives
{
	const string Directive = "#line ";

	public static string Relative(string text, IReadOnlySet<string> inputs, string outputDirectory)
	{
		StringBuilder? rewritten = null;

		var copied = 0;
		var at     = 0;

		while (at < text.Length)
		{
			var end     = text.IndexOf('\n', at);
			var lineEnd = end < 0 ? text.Length : end;
			var content = lineEnd > at && text[lineEnd - 1] == '\r' ? lineEnd - 1 : lineEnd;

			if (string.CompareOrdinal(text, at, Directive, 0, Directive.Length) == 0 &&
				PathIn(text, at + Directive.Length, content) is var (pathStart, pathEnd))
			{
				var path = text.Substring(pathStart, pathEnd - pathStart);

				if (inputs.Contains(path))
				{
					rewritten ??= new StringBuilder(text.Length);

					rewritten.Append(text, copied, pathStart - copied)
						.Append(Path.GetRelativePath(outputDirectory, path).Replace('\\', '/'));

					copied = pathEnd;
				}
			}

			at = end < 0 ? text.Length : end + 1;
		}

		return rewritten is null ? text : rewritten.Append(text, copied, text.Length - copied).ToString();
	}

	/// <summary>Where the path of <c>n "path"</c> lies between <paramref name="from"/> and <paramref name="to"/>, if it is that.</summary>
	static (int Start, int End)? PathIn(string text, int from, int to)
	{
		var at = from;

		while (at < to && text[at] is >= '0' and <= '9')
			at++;

		if (at == from || at + 2 > to || text[at] != ' ' || text[at + 1] != '"' || text[to - 1] != '"' || to - 1 < at + 2)
			return null;

		return (at + 2, to - 1);
	}
}
