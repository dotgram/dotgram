using System;

using DotGram;

namespace DotGram.Cli;

/// <summary>
/// One argument as the command line or a response file gave it, with the directory a relative path
/// in it is read against.
/// </summary>
/// <param name="Text">The argument, quotes removed.</param>
/// <param name="BaseDirectory">
/// The current directory for an argument typed on the command line, and the response file's own
/// directory for one read from it.
/// </param>
readonly record struct Argument(string Text, string BaseDirectory);

/// <summary>
/// <c>@file</c> arguments, replaced by what the file says: csc's syntax — blanks and line ends
/// between arguments, double quotes to keep a blank inside one, a line beginning with <c>#</c> a
/// comment, and <c>@file</c> again inside a file.
/// </summary>
/// <remarks>
/// One thing differs from csc, on purpose: a relative path in the file is read against the file's
/// directory and not against the current one, so that a response file committed beside a project
/// means the same from wherever it is run.
/// </remarks>
[Gram("ResponseFile.gram")]
static partial class ResponseFile
{
	/// <summary>The arguments with every <c>@file</c> among them replaced, in order.</summary>
	/// <exception cref="ToolException">A file that cannot be read, or one that includes itself.</exception>
	public static List<Argument> Expand(IEnumerable<string> arguments, string currentDirectory)
	{
		var expanded = new List<Argument>();
		var open     = new Stack<string>();

		foreach (var argument in arguments)
			Add(new Argument(argument, currentDirectory), expanded, open);

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
			throw ToolException.Usage($"cannot read response file '{path}': {exception.Message}");
		}

		var directory = Path.GetDirectoryName(path)!;

		open.Push(path);

		foreach (var token in Split(text))
			Add(new Argument(token, directory), expanded, open);

		open.Pop();
	}

	/// <summary>The arguments a response file's text holds, comments left out.</summary>
	public static List<string> Split(string text)
	{
		var arguments = new List<string>();

		foreach (var line in text.Split(LineEnds, StringSplitOptions.None))
		{
			var trimmed = line.TrimStart(' ', '\t');

			if (trimmed.Length == 0 || trimmed[0] == '#')
				continue;

			arguments.AddRange(ParseLine(line));
		}

		return arguments;
	}

	static readonly string[] LineEnds = ["\r\n", "\n", "\r"];
}
