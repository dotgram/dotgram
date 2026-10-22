using System;

namespace DotGram.Cli;

/// <summary>What <c>--help</c> and <c>--skill</c> print.</summary>
static class Help
{
	public const string Usage = """
		Usage: dotnet dotgram generate <project.csproj | grammar.gram> -o <dir> [options] [@file.rsp]
		       dotnet dotgram --help | generate --help | --skill | --version
		""";

	public const string Generate = """
		dotnet dotgram generate <project.csproj | grammar.gram> -o|--output <dir> [options]

		Runs the DotGram generator outside the build and writes the parsers a build would generate into
		<dir>, with .dotgram.json listing them. The project then compiles those files and does not
		reference the DotGram package: referencing both generates every parser twice.

		The input's kind is its extension:
		  .csproj   the whole project: its C# files, global usings, grammars, LangVersion and symbols,
		            read by an MSBuild evaluation for each target framework (nothing is restored or
		            built); every framework must generate the same files
		  .gram     one grammar, its host class written from the options below (<Name>.Host.g.cs)

		Options:
		  -o, --output <dir>          where the files go (required); nothing else in it is touched
		  --framework <tfm>           one framework only: net8.0, net9.0, net10.0, netstandard2.0
		                              (default: the project's; net10.0 for a .gram)
		  --configuration <name>      evaluated as this configuration (default Release)
		  --reference <file.dll|dir>  an assembly the grammar's types come from (every *.dll of a dir)
		  --package <Name>@<Version>  a package's assemblies, from the global packages folder
		  --define <SYMBOL>           a preprocessor symbol, beside the project's and the framework's
		  --langversion <version>     the C# version (default: the project's, or the SDK's for the framework)
		  --line-paths relative|absolute
		                              #line paths relative to <dir> (default), or as a build writes them
		  --allow-unresolved          generate even where a type a grammar asks about is missing

		For a .gram input only:
		  --namespace <Ns>            the host's namespace (default: none)
		  --class <Name>              the host's name (default: the file's name)
		  --public                    a public host (default: internal)
		  --option <Name>=<value>     a [Gram] property: Lexical=true, Carrier=Immediate,
		                              LocationType=My.ISpan, Suffix=Fast, PartSize=200, ...
		  --sources <file.cs|glob>    C# the grammar builds, and a partial host with the members its
		                              hooks call (no [Gram] on it)

		@file.rsp anywhere is replaced by the arguments in that file: separated by blanks or line
		ends, "quoted" to keep blanks, # starting a comment line, @ nesting. A relative path in it is
		read against the file's own directory.

		Exit codes:
		  0  generated
		  2  grammar errors (nothing written); for a .gram, a parser that does not compile
		  3  usage, input or output, evaluation failure, a type a grammar asks about that cannot be
		     resolved (nothing written), frameworks that generate different files

		Examples:
		  dotnet dotgram generate src/Grammars/Grammars.csproj -o src/Grammars/Generated
		  dotnet dotgram generate @grammars.rsp
		  dotnet dotgram generate calc.gram -o out --namespace Demo --option Lexical=true
		""";

	public static readonly string Text = $"""
		dotnet dotgram — the DotGram parser generator as a command

		{Usage}

		Commands:
		  generate     write the parsers a build would generate (dotnet dotgram generate --help)
		  --skill      print the guide for coding agents (SKILL.md)
		  --version    print the tool's and the generator's versions
		  --help       print this

		{Generate}
		""";

	/// <summary>The tool's <c>SKILL.md</c>, which ships inside it.</summary>
	public static string Skill()
	{
		using var stream = typeof(Help).Assembly.GetManifestResourceStream("DotGram.Cli.SKILL.md")
			?? throw new InvalidOperationException("SKILL.md is not embedded in the tool.");

		using var reader = new StreamReader(stream);

		return reader.ReadToEnd();
	}
}
