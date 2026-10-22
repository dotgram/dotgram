using System;

namespace DotGram.Cli;

/// <summary>What a command line asks for, read and checked.</summary>
abstract record Command
{
	/// <summary>No command at all.</summary>
	public sealed record Usage : Command;

	public sealed record Help : Command;

	public sealed record GenerateHelp : Command;

	public sealed record Version : Command;

	public sealed record Skill : Command;

	public sealed record Generate(GenerateOptions Options) : Command;
}

/// <summary>
/// The one place the argument vector is read: response files expanded, the command told, and
/// <c>generate</c>'s options handed to <see cref="GenerateOptions.Parse"/>. Everything after it works
/// on a <see cref="Command"/>, so how the vector is read can change here and nowhere else.
/// </summary>
static class CommandLine
{
	/// <exception cref="ToolException">An unknown command or option, or a value that does not fit.</exception>
	public static Command Parse(IReadOnlyList<string> arguments, string currentDirectory)
	{
		var expanded = ResponseFile.Expand(arguments, currentDirectory);

		if (expanded.Count == 0)
			return new Command.Usage();

		var rest = expanded.Skip(1).ToList();

		return expanded[0].Text switch
		{
			"-h" or "--help" or "-?" or "help" => new Command.Help(),
			"--version"                        => new Command.Version(),
			"--skill"                          => new Command.Skill(),
			"generate" when rest.Any(static argument => argument.Text is "-h" or "--help" or "-?")
			                                   => new Command.GenerateHelp(),
			"generate"                         => new Command.Generate(GenerateOptions.Parse(rest)),
			var other                          => throw ToolException.Usage($"unknown command '{other}'."),
		};
	}
}
