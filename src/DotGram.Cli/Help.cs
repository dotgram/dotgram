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
		  .csproj   the whole project, as its own build gives the compiler its inputs. The tool does not
		            compile it: it asks MSBuild for those inputs, for each target framework. That runs
		            the project's InitialTargets and the targets that run before CoreCompile (generating
		            AssemblyInfo, global usings, the analyzer config under obj/, and any of the project's
		            own targets hooked there), not Build, AfterBuild or anything after the compiler.
		            The project must be restored (dotnet restore: the tool never restores); referenced
		            projects are built if not up to date; needs .NET SDK 8.0.100 or newer. Every
		            framework must generate the same files. The project's other generators run beside
		            DotGram's; the language version, symbols, references, packages and source encoding
		            (CodePage) are the build's to decide.
		  .gram     one grammar, its host class written from the options below (<Name>.Host.g.cs)

		What a .csproj run promises: it writes the files DotGram would generate in `dotnet build` of the
		project (configuration Release unless --configuration says otherwise), byte for byte, in the
		encoding the compiler writes them (UTF-8 with a byte order mark), with the tool's DotGram, for
		a restored project, built with the same SDK, whose own targets do not change the compiler's
		inputs outside the IDE's design-time pass. A difference on a project inside that condition,
		which every ordinary SDK-style project is, is a defect in the tool: report it.

		Two differences by design: #line paths are relative to the output folder (--line-paths absolute
		gives the build's text), and the generation report files (DotGramReportGeneration) are not
		written. The EmbeddedAttribute definition is the compiler's text and may differ in line endings
		from your SDK's. The diagnostics of DotGram itself are shown at the severities the build gives
		them (.editorconfig, /nowarn, /warnaserror); an error among them stops the run (exit 2).
		The SDK's compiler must be at least as new as the one DotGram was built for, as for the build,
		which skips a generator built for a newer compiler (CS9057): the run then exits 3.

		What it does not do: it does not compile the project and does not tell you whether it builds;
		errors in grammar actions, in the generated parser or anywhere else in the project show when
		your build compiles the files. It reports the errors in your own sources that it sees, at the
		compiler's default severities (/warnaserror, /nowarn, rulesets and .editorconfig severities are
		not applied), and stops there as the build would; it does not check the project as a whole.
		--allow-unresolved generates anyway. It runs what an IDE runs when it opens the project:
		MSBuild's Compile target with the compiler skipped, as a design-time build
		(DesignTimeBuild=true), so a custom target that does not skip design-time builds runs too,
		and one that follows the compiler finds no new assembly. It does not restore: after changing
		package references, run dotnet restore first. When this is not good enough, build with EmitCompilerGeneratedFiles=true and
		copy the generated files.

		Options:
		  -o, --output <dir>          where the files go (required); nothing else in it is touched
		  --framework <tfm>           one framework only: net8.0, net9.0, net10.0, netstandard2.0
		                              (default: the project's; net10.0 for a .gram)
		  --configuration <name>      the project's configuration (default Release)
		  --line-paths relative|absolute
		                              #line paths relative to <dir> (default), or as a build writes them
		  --allow-unresolved          generate even where the project's code has errors (reported as warnings)

		For a .gram input only (a project's build decides these for a project):
		  --reference <file.dll|dir>  an assembly the grammar's types come from (every *.dll of a dir)
		  --package <Name>@<Version>  a package's assemblies, from the global packages folder
		  --define <SYMBOL>           a preprocessor symbol, beside the framework's
		  --langversion <version>     the C# version (default: the SDK's for the framework)
		  --namespace <Ns>            the host's namespace (default: none)
		  --class <Name>              the host's name (default: the file's name)
		  --public                    a public host (default: internal)
		  --option <Name>=<value>     a [Gram] property: Lexical=true, Carrier=Immediate,
		                              LocationType=My.ISpan, Suffix=Fast, PartSize=200, ...
		  --sources <file.cs|glob>    C# the grammar builds, and a partial host with the members its
		                              hooks call (no [Gram] on it)

		-- ends the options: every argument after it is the input, a dash in front or not.

		@file.rsp anywhere is replaced by the arguments in that file: separated by blanks or line
		ends, "quoted" to keep blanks, # starting a comment line, @ nesting. A relative path in it is
		read against the file's own directory.

		Exit codes:
		  0  generated
		  2  grammar errors (nothing written); for a .gram, a parser that does not compile
		  3  usage, input or output, a project not restored, MSBuild failing for a framework, a generator
		     that will not load, an error the project's build would report too (nothing written),
		     frameworks that generate different files

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
