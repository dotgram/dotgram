using System;

using DotGram;

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

/// <summary>What one item of the argument vector is.</summary>
enum ItemKind
{
	/// <summary>An option and the argument after it, its value.</summary>
	Value,

	/// <summary>An option that takes a value, given as the last argument.</summary>
	Missing,

	/// <summary>An option that takes none.</summary>
	Flag,

	/// <summary>Something that begins with <c>-</c> and is no option the tool has.</summary>
	Unknown,

	/// <summary>Anything else: the command word, the input, or whatever follows <c>--</c>.</summary>
	Positional,
}

/// <summary>A word of a response file, at its offset in the file's text.</summary>
sealed record Word(string Text, int At);

/// <summary>An item with the argument it was read from, and for an option the argument its value was.</summary>
readonly record struct Option(ItemKind Kind, string? Name, string? Value, Argument Argument, Argument? ValueArgument);

/// <summary>
/// The one place the argument vector is read: response files expanded, each argument told for what
/// it is, the command told. Everything after it works on a <see cref="Command"/>.
/// </summary>
[Gram("CommandLine.gram")]
static partial class CommandLine
{
	/// <summary>The options that take the next argument as their value, whatever it begins with.</summary>
	internal static readonly HashSet<string> Valued =
	[
		"-o", "--output", "--framework", "--configuration", "--reference", "--package", "--define",
		"--langversion", "--line-paths", "--namespace", "--class", "--option", "--sources",
	];

	/// <summary>The options that take none.</summary>
	internal static readonly HashSet<string> Flags = ["--allow-unresolved", "--public", "--help", "-h", "-?", "--version", "--skill"];

	/// <exception cref="ToolException">An unknown command or option, or a value that does not fit.</exception>
	public static Command Parse(IReadOnlyList<string> arguments, string currentDirectory)
	{
		var expanded = ResponseFile.Expand(arguments, currentDirectory);

		if (expanded.Count == 0)
			return new Command.Usage();

		var options = Read(expanded);

		if (options.Count == 0)
			return new Command.Usage();

		var first   = options[0];
		var rest    = options.Skip(1).ToList();

		switch (first.Kind)
		{
			case ItemKind.Flag when first.Name is "--help" or "-h" or "-?":
				return new Command.Help();

			case ItemKind.Flag when first.Name is "--version":
				return new Command.Version();

			case ItemKind.Flag when first.Name is "--skill":
				return new Command.Skill();

			case ItemKind.Positional when first.Value is "help":
				return new Command.Help();

			case ItemKind.Positional when first.Value is "generate":
				return rest.Any(static option => option is { Kind: ItemKind.Flag, Name: "--help" or "-h" or "-?" })
					? new Command.GenerateHelp()
					: new Command.Generate(GenerateOptions.Parse(rest));

			case ItemKind.Positional when first.Value!.Length == 0:
				throw ToolException.Usage($"an empty argument ({first.Argument.Source}) where the command goes.");

			default:
				throw ToolException.Usage($"unknown command '{first.Name ?? first.Value}' ({first.Argument.Source}).");
		}
	}

	/// <summary>
	/// The arguments read into options, each with the argument it came from: an option that takes a
	/// value takes the next argument, <c>--</c> ends the options, and anything else beginning with a
	/// dash is an option the tool does not have.
	/// </summary>
	public static List<Option> Read(IReadOnlyList<Argument> arguments)
	{
		var options = new List<Option>();
		var ended   = false;

		for (var at = 0; at < arguments.Count; at++)
		{
			var argument = arguments[at];
			var text     = argument.Text;

			if (ended)
				options.Add(new Option(ItemKind.Positional, null, text, argument, null));
			else if (text == "--")
				ended = true;
			else if (Valued.Contains(text))
				options.Add(at + 1 < arguments.Count
					? new Option(ItemKind.Value, text, arguments[at + 1].Text, argument, arguments[++at])
					: new Option(ItemKind.Missing, text, null, argument, null));
			else if (Flags.Contains(text))
				options.Add(new Option(ItemKind.Flag, text, null, argument, null));
			else if (text.Length > 1 && text[0] == '-')
				options.Add(new Option(ItemKind.Unknown, text, null, argument, null));
			else
				options.Add(new Option(ItemKind.Positional, null, text, argument, null));
		}

		return options;
	}
}
