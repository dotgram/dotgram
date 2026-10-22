using System;
using System.Text;

namespace DotGram.Cli;

/// <summary>What the input is, told by its extension and by nothing else.</summary>
enum InputKind
{
	Project,
	Grammar,
}

/// <summary>How a <c>#line</c> names the file it points into.</summary>
enum LinePaths
{
	/// <summary>Relative to the output directory, with forward slashes: the default.</summary>
	Relative,

	/// <summary>As the generator wrote it, byte for byte what a build writes.</summary>
	Absolute,
}

/// <summary><c>generate</c>'s arguments, read and checked.</summary>
sealed class GenerateOptions
{
	public required string    Input            { get; init; }
	public required InputKind Kind             { get; init; }
	public required string    Output           { get; init; }
	public string?            Framework        { get; init; }
	public string             Configuration    { get; init; } = "Release";
	public List<string>       References       { get; } = [];
	public List<string>       Packages         { get; } = [];
	public List<string>       Defines          { get; } = [];
	public string?            LanguageVersion  { get; init; }
	public LinePaths          LinePaths        { get; init; }
	public bool               AllowUnresolved  { get; init; }

	// `.gram` input only.
	public string?            Namespace        { get; init; }
	public string?            ClassName        { get; init; }
	public bool               Public           { get; init; }
	public List<(string Name, string Value)> GramOptions { get; } = [];
	public List<string>       Sources          { get; } = [];

	/// <summary>
	/// The arguments as the tool understood them, response files expanded and paths made full: the
	/// first line a run prints, so that an argument a response file dropped or split is seen.
	/// </summary>
	public string Describe()
	{
		var text = new StringBuilder("generate ").Append(Quote(Input)).Append(" --output ").Append(Quote(Output));

		if (Framework is not null)
			text.Append(" --framework ").Append(Framework);

		text.Append(" --configuration ").Append(Quote(Configuration));

		foreach (var reference in References)
			text.Append(" --reference ").Append(Quote(reference));

		foreach (var package in Packages)
			text.Append(" --package ").Append(Quote(package));

		foreach (var define in Defines)
			text.Append(" --define ").Append(Quote(define));

		if (LanguageVersion is not null)
			text.Append(" --langversion ").Append(Quote(LanguageVersion));

		text.Append(" --line-paths ").Append(LinePaths == LinePaths.Relative ? "relative" : "absolute");

		if (AllowUnresolved)
			text.Append(" --allow-unresolved");

		if (Namespace is not null)
			text.Append(" --namespace ").Append(Quote(Namespace));

		if (ClassName is not null)
			text.Append(" --class ").Append(Quote(ClassName));

		if (Public)
			text.Append(" --public");

		foreach (var (name, value) in GramOptions)
			text.Append(" --option ").Append(Quote(name + "=" + value));

		foreach (var source in Sources)
			text.Append(" --sources ").Append(Quote(source));

		return text.ToString();
	}

	static string Quote(string text)
	{
		return text.Length > 0 && text.IndexOfAny([' ', '\t', '"']) < 0 ? text : "\"" + text.Replace("\"", "\\\"") + "\"";
	}

	/// <summary>Reads <c>generate</c>'s options, everything after the command word, as the grammar split them.</summary>
	/// <exception cref="ToolException">An unknown option, a missing or empty value, a value that does not fit.</exception>
	public static GenerateOptions Parse(IReadOnlyList<Option> arguments)
	{
		string?   input           = null;
		string?   output          = null;
		string?   framework       = null;
		string?   configuration   = null;
		string?   languageVersion = null;
		LinePaths? linePaths      = null;
		var       allowUnresolved = false;
		string?   @namespace      = null;
		string?   className       = null;
		var       @public         = false;

		var references  = new List<string>();
		var packages    = new List<string>();
		var defines     = new List<string>();
		var gramOptions = new List<(string, string)>();
		var sources     = new List<string>();
		var gramOnly    = new List<string>();

		foreach (var argument in arguments)
		{
			var name = argument.Name ?? "";

			switch (argument.Kind)
			{
				case ItemKind.Unknown:
					throw ToolException.Usage($"unknown option '{name}' ({argument.Argument.Source}).");

				case ItemKind.Missing:
					throw ToolException.Usage($"{name} needs a value ({argument.Argument.Source}).");

				case ItemKind.Positional:
					if (argument.Value!.Length == 0)
						throw ToolException.Usage($"an empty argument ({argument.Argument.Source}).");

					if (input is not null)
						throw ToolException.Usage($"one input only: '{input}' and '{argument.Value}' ({argument.Argument.Source}) were both given.");

					input = Path.GetFullPath(argument.Value, argument.Argument.BaseDirectory);
					continue;

				case ItemKind.Flag:
					switch (name)
					{
						case "--allow-unresolved": allowUnresolved = true; break;
						case "--public":           gramOnly.Add(name); @public = true; break;
						default:                   throw ToolException.Usage($"{name} is not an option of generate ({argument.Argument.Source}).");
					}

					continue;
			}

			// An option with its value, which the grammar took from the next argument.
			var value = argument.Value!;

			if (value.Length == 0)
				throw ToolException.Usage($"{name} has an empty value ({argument.ValueArgument!.Value.Source}).");

			var full = Path.GetFullPath(value, argument.ValueArgument!.Value.BaseDirectory);

			switch (name)
			{
				case "-o" or "--output":
					output = Once(output, name, full);
					break;

				case "--framework":
					framework = Once(framework, name, value);
					break;

				case "--configuration":
					configuration = Once(configuration, name, value);
					break;

				case "--reference":
					references.Add(full);
					break;

				case "--package":
					if (!global::DotGram.Cli.Packages.TryParse(value, out _, out _))
						throw ToolException.Usage($"--package takes Name@Version, not '{value}'.");

					packages.Add(value);
					break;

				case "--define":
					foreach (var symbol in value.Split([';', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
						defines.Add(symbol);

					break;

				case "--langversion":
					languageVersion = Once(languageVersion, name, value);
					break;

				case "--line-paths":
					linePaths = Once(linePaths, name, value switch
					{
						"relative" => LinePaths.Relative,
						"absolute" => LinePaths.Absolute,
						var other  => throw ToolException.Usage($"--line-paths takes 'relative' or 'absolute', not '{other}'."),
					});
					break;

				case "--namespace":
					gramOnly.Add(name);
					@namespace = Once(@namespace, name, value);
					break;

				case "--class":
					gramOnly.Add(name);
					className = Once(className, name, value);
					break;

				case "--option":
				{
					gramOnly.Add(name);

					var equals = value.IndexOf('=');

					if (equals <= 0)
						throw ToolException.Usage($"--option takes Name=value, not '{value}'.");

					gramOptions.Add((value.Substring(0, equals).Trim(), value.Substring(equals + 1)));
					break;
				}

				case "--sources":
					gramOnly.Add(name);
					sources.Add(full);
					break;

				default:
					throw new InvalidOperationException($"The grammar takes a value for {name}, and nothing here reads it.");
			}
		}

		if (input is null)
			throw ToolException.Usage("no input: name a project (.csproj) or a grammar (.gram).");

		if (output is null)
			throw ToolException.Usage("no output directory: pass -o <dir>.");

		var kind = Path.GetExtension(input).ToLowerInvariant() switch
		{
			".csproj" => InputKind.Project,
			".gram"   => InputKind.Grammar,
			_         => throw ToolException.Usage($"'{input}' is neither a project (.csproj) nor a grammar (.gram): the input's kind is its extension."),
		};

		if (kind == InputKind.Project && gramOnly.Count > 0)
			throw ToolException.Usage($"{string.Join(", ", gramOnly.Distinct())}: only for a .gram input; a project's hosts say these themselves.");

		var options = new GenerateOptions
		{
			Input           = input,
			Kind            = kind,
			Output          = output,
			Framework       = framework,
			Configuration   = configuration ?? "Release",
			LanguageVersion = languageVersion,
			LinePaths       = linePaths ?? LinePaths.Relative,
			AllowUnresolved = allowUnresolved,
			Namespace       = @namespace,
			ClassName       = className,
			Public          = @public,
		};

		options.References.AddRange(references);
		options.Packages.AddRange(packages);
		options.Defines.AddRange(defines);
		options.GramOptions.AddRange(gramOptions);
		options.Sources.AddRange(sources);

		return options;
	}

	static T Once<T>(T? already, string option, T value)
	{
		if (already is not null)
			throw ToolException.Usage($"{option} given twice.");

		return value;
	}
}
