using System;
using System.Text;

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

/// <summary>What one item of the argument vector is, as the grammar tells it.</summary>
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

/// <summary>An item as the grammar builds it, at its offset in the joined vector.</summary>
sealed record Item(ItemKind Kind, string? Name, string? Value, int At);

/// <summary>A word of a response file, at its offset in the file's text.</summary>
sealed record Word(string Text, int At);

/// <summary>An item with the argument it was read from, and for an option the argument its value was.</summary>
readonly record struct Option(ItemKind Kind, string? Name, string? Value, Argument Argument, Argument? ValueArgument);

/// <summary>
/// The one place the argument vector is read: response files expanded, the arguments joined by NUL
/// and read by the tool's grammar of its own command line, the command told. Everything after it
/// works on a <see cref="Command"/>.
/// </summary>
[Gram("CommandLine.gram")]
static partial class CommandLine
{
	const char Separator = '\0';

	/// <exception cref="ToolException">An unknown command or option, or a value that does not fit.</exception>
	public static Command Parse(IReadOnlyList<string> arguments, string currentDirectory)
	{
		var expanded = ResponseFile.Expand(arguments, currentDirectory);

		if (expanded.Count == 0)
			return new Command.Usage();

		var options = Read(expanded);
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

	/// <summary>The arguments read into options, each with the argument it came from.</summary>
	public static List<Option> Read(IReadOnlyList<Argument> arguments)
	{
		var joined = new StringBuilder();
		var starts = new int[arguments.Count];

		for (var index = 0; index < arguments.Count; index++)
		{
			if (index > 0)
				joined.Append(Separator);

			starts[index] = joined.Length;
			joined.Append(arguments[index].Text);
		}

		var match = TryParseArguments(joined.ToString());

		if (!match.IsSuccess)
			throw ToolException.Usage("the arguments cannot be read: " + Readable(match.Error ?? "no reason given") + ".");

		var options = new List<Option>();

		foreach (var item in match.Value!)
		{
			var index = Array.BinarySearch(starts, item.At);

			if (index < 0)
				throw new InvalidOperationException($"An item at {item.At} begins no argument.");

			options.Add(new Option(
				item.Kind,
				item.Name,
				item.Value,
				arguments[index],
				item.Kind == ItemKind.Value ? arguments[index + 1] : null));
		}

		return options;
	}

	/// <summary>
	/// A refusal of the grammar in words: the separator is the end of an argument, and the generated
	/// parser marks its own words with a NUL in front, neither of which is ever printed as one.
	/// </summary>
	public static string Readable(string message)
	{
		return message.Replace("'\0'", "end of argument", StringComparison.Ordinal).Replace("\0", "end of argument ", StringComparison.Ordinal);
	}
}
