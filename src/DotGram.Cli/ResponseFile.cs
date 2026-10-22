using System;
using System.Globalization;

namespace DotGram.Cli;

/// <summary>
/// One argument as the command line or a response file gave it, with the directory a relative path
/// in it is read against and where it came from, for messages.
/// </summary>
/// <param name="Text">The argument, quotes removed.</param>
/// <param name="BaseDirectory">
/// The current directory for an argument typed on the command line, and the response file's own
/// directory for one read from it.
/// </param>
/// <param name="Source">"argument 3", or "grammars.rsp:4".</param>
readonly record struct Argument(string Text, string BaseDirectory, string Source);

/// <summary>
/// <c>@file</c> arguments, replaced by what the file says: csc's syntax — blanks and line ends
/// between arguments, double quotes to keep a blank inside one, a line beginning with <c>#</c> a
/// comment, and <c>@file</c> again inside a file. An <c>@</c> counts only at the start of an argument.
/// </summary>
/// <remarks>
/// One thing differs from csc, on purpose: a relative path in the file is read against the file's
/// directory and not against the current one, so that a response file committed beside a project
/// means the same from wherever it is run.
/// </remarks>
static class ResponseFile
{
	/// <summary>The arguments with every <c>@file</c> among them replaced, in order.</summary>
	/// <exception cref="ToolException">A file that cannot be read, one that includes itself, one that is not text.</exception>
	public static List<Argument> Expand(IReadOnlyList<string> arguments, string currentDirectory)
	{
		var expanded = new List<Argument>();
		var open     = new Stack<string>();

		for (var index = 0; index < arguments.Count; index++)
			Add(new Argument(arguments[index], currentDirectory, "argument " + (index + 1).ToString(CultureInfo.InvariantCulture)), expanded, open);

		return expanded;
	}

	static void Add(Argument argument, List<Argument> expanded, Stack<string> open)
	{
		if (argument.Text.Length < 2 || argument.Text[0] != '@')
		{
			expanded.Add(argument);
			return;
		}

		var path = Path.GetFullPath(argument.Text.Substring(1), argument.BaseDirectory);

		if (open.Contains(path, StringComparer.Ordinal))
			throw ToolException.Usage($"response file '{path}' includes itself (through {string.Join(" -> ", open.Reverse())}).");

		string text;

		try
		{
			text = File.ReadAllText(path);
		}
		catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
		{
			throw ToolException.Usage($"cannot read response file '{path}' ({argument.Source}): {exception.Message}");
		}

		var directory = Path.GetDirectoryName(path)!;
		var name      = Path.GetFileName(path);

		open.Push(path);

		foreach (var word in Split(text, path))
			Add(new Argument(word.Text, directory, name + ":" + LineOf(text, word.At).ToString(CultureInfo.InvariantCulture)), expanded, open);

		open.Pop();
	}

	/// <summary>The words of a response file's text, comments left out.</summary>
	/// <exception cref="ToolException">A text holding NUL: no response file does, and one in UTF-16 without a byte order mark looks so.</exception>
	public static IReadOnlyList<Word> Split(string text, string path = "the response file")
	{
		var nul = text.IndexOf('\0');

		if (nul >= 0)
			throw ToolException.Usage(
				$"{path}:{LineOf(text, nul).ToString(CultureInfo.InvariantCulture)} holds a NUL character, which no argument can: " +
				"the file is probably UTF-16 without a byte order mark. Save it as UTF-8.");

		var match = CommandLine.TryParseResponseText(text);

		if (!match.IsSuccess)
			throw ToolException.Usage($"{path} cannot be read: {match.Error ?? "no reason given"}.");

		return match.Value!;
	}

	/// <summary>The one-based line of an offset into a text.</summary>
	static int LineOf(string text, int at)
	{
		var line = 1;

		for (var index = 0; index < at && index < text.Length; index++)
			if (text[index] == '\n' || (text[index] == '\r' && (index + 1 >= text.Length || text[index + 1] != '\n')))
				line++;

		return line;
	}
}
