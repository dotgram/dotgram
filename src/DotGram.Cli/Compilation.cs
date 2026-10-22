using System;
using System.Collections.Immutable;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace DotGram.Cli;

/// <summary>
/// Everything one generation reads, for one target framework: the C# a build would compile, the
/// additional files it would hand the generators, the references, the options and the analyzer
/// configuration the compiler would be given.
/// </summary>
sealed class GenerationInputs
{
	/// <summary>The framework as a project or an option names it: <c>net10.0</c>, <c>net8.0-windows</c>.</summary>
	public required string                             Moniker         { get; init; }
	public required string                             AssemblyName    { get; init; }
	public required CSharpParseOptions                 ParseOptions    { get; init; }
	public required CSharpCompilationOptions           CompilationOptions { get; init; }
	public required ImmutableArray<SyntaxTree>         Sources         { get; init; }

	/// <summary>Every additional file, the grammars among them; the generators choose what they read.</summary>
	public required ImmutableArray<AdditionalText>     AdditionalFiles { get; init; }
	public required ImmutableArray<MetadataReference>  References      { get; init; }

	/// <summary>What the generators are told by the build: global options, and the options of each file.</summary>
	public required ConfiguredOptions                  Options         { get; init; }

	/// <summary>Every file a <c>#line</c> may name: the C# sources and the additional files, as full paths.</summary>
	public IEnumerable<string> InputPaths()
	{
		foreach (var tree in Sources)
			if (tree.FilePath.Length > 0)
				yield return tree.FilePath;

		foreach (var file in AdditionalFiles)
			yield return file.Path;
	}

	public CSharpCompilation Compile()
	{
		return CSharpCompilation.Create(AssemblyName, Sources, References, CompilationOptions);
	}

	/// <summary>The parse options a build of this framework would use.</summary>
	/// <param name="languageVersion">As a project or <c>--langversion</c> writes it, or null for the framework's default.</param>
	public static CSharpParseOptions Parsing(Framework framework, string? languageVersion, IEnumerable<string> symbols)
	{
		var version = framework.DefaultLanguageVersion;

		if (languageVersion is not null && !LanguageVersionFacts.TryParse(languageVersion, out version))
			throw ToolException.Usage($"'{languageVersion}' is not a C# language version this tool knows.");

		return new CSharpParseOptions(
			version,
			DocumentationMode.Parse,
			SourceCodeKind.Regular,
			[.. symbols.Distinct(StringComparer.Ordinal)]);
	}

	/// <summary>Reads and parses C# files, in parallel, each under its full path.</summary>
	public static ImmutableArray<SyntaxTree> Parse(IReadOnlyList<string> paths, CSharpParseOptions options)
	{
		var trees = new SyntaxTree[paths.Count];

		try
		{
			Parallel.For(0, paths.Count, index =>
			{
				trees[index] = CSharpSyntaxTree.ParseText(Read(paths[index]), options, paths[index]);
			});
		}
		catch (AggregateException exception) when (exception.Flatten().InnerExceptions.OfType<ToolException>().FirstOrDefault() is { } first)
		{
			// A file that cannot be read, said as the tool says it, not as an unhandled parallel loop.
			throw first;
		}

		return [.. trees];
	}

	/// <summary>A file's text, its encoding told by its byte order mark and UTF-8 otherwise, as csc reads it.</summary>
	public static SourceText Read(string path)
	{
		try
		{
			using var stream = File.OpenRead(path);

			return SourceText.From(stream, Encoding.UTF8, SourceHashAlgorithm.Sha256);
		}
		catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
		{
			throw ToolException.Failure($"cannot read '{path}': {exception.Message}");
		}
	}

	/// <summary>The references for a framework: the tool's own, then <c>--reference</c> and <c>--package</c>.</summary>
	public static ImmutableArray<MetadataReference> ReferencesFor(Framework framework, IReadOnlyList<string> references, IReadOnlyList<string> packages)
	{
		var all = new List<MetadataReference>(framework.References);

		foreach (var reference in references)
		{
			if (Directory.Exists(reference))
			{
				foreach (var file in Directory.GetFiles(reference, "*.dll").Order(StringComparer.Ordinal))
					all.Add(MetadataReference.CreateFromFile(file));
			}
			else if (File.Exists(reference))
				all.Add(MetadataReference.CreateFromFile(reference));
			else
				throw ToolException.Failure($"--reference '{reference}' is neither a file nor a directory.");
		}

		foreach (var package in packages)
			foreach (var file in Packages.Assemblies(package, framework))
				all.Add(MetadataReference.CreateFromFile(file));

		// One assembly named twice — a package restore resolved and --package as well — is one reference.
		return [.. all.DistinctBy(static reference => reference is PortableExecutableReference { FilePath: { } path } ? path : (object)reference)];
	}
}

/// <summary>A file handed to the generators, as a build hands them an <c>AdditionalFiles</c> item.</summary>
sealed class FileText(string path, SourceText text) : AdditionalText
{
	public override string Path { get; } = path;

	public override SourceText GetText(CancellationToken cancellationToken = default)
	{
		return text;
	}
}
