// The comparisons of run-gates.sh, as a file-based app: `dotnet run Gates.cs -- <command> ...`.
// run-gates.sh copies this file outside the repository and writes the Roslyn version into the package
// line below, so that the repository's Directory.Build.props and central package versions do not apply.
//
//   compare <built> <tool> [--relative-to <dir>]
//       Every file the build generated (its report files aside) against the tool's folder: byte for
//       byte, or, with --relative-to, after each #line path of the tool's text is read against that
//       directory. EmbeddedAttribute is Roslyn's text and is compared without its line ends.
//   g2 <csc-arguments.json> <built> <project-directory>
//       The generator hosted by hand over the compiler's own arguments of a real build (MSBuild's
//       CscCommandLineArgs, written by -getItem), held to what that build generated.
//
// Exit 0: equal; 1: a difference (each one printed); 2: usage.

#:package Microsoft.CodeAnalysis.CSharp@ROSLYN_VERSION
#:property Nullable=enable
#:property ImplicitUsings=enable
#:property TreatWarningsAsErrors=true
#:property PublishAot=false

using System.Collections.Immutable;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

const string Embedded = "Microsoft.CodeAnalysis.EmbeddedAttribute.cs";

if (args.Length >= 3 && args[0] == "compare")
{
	var relative = args.Length == 5 && args[3] == "--relative-to" ? Path.GetFullPath(args[4]) : null;

	return Compare(args[1], args[2], relative);
}

if (args.Length == 4 && args[0] == "g2")
	return HostByHand(args[1], args[2], args[3]);

Console.Error.WriteLine("usage: Gates.cs compare <built> <tool> [--relative-to <dir>] | g2 <csc-arguments.json> <built> <project-directory>");
return 2;

// The build's files, the report files aside: the tool does not write those, by design.
static IEnumerable<string> Built(string folder)
{
	return Directory.EnumerateFiles(folder)
		.Select(static file => Path.GetFileName(file))
		.Where(static name => !name.Contains(".DotGramReport", StringComparison.Ordinal))
		.Order(StringComparer.Ordinal);
}

static int Compare(string built, string tool, string? relative)
{
	foreach (var folder in new[] { built, tool })
	{
		if (!Directory.Exists(folder))
		{
			Console.WriteLine($"  no folder {folder}: nothing was generated there");
			return 1;
		}
	}

	var failures = 0;
	var names    = Built(built).ToList();

	foreach (var name in names)
	{
		var path = Path.Combine(tool, name);

		if (!File.Exists(path))
		{
			Console.WriteLine($"  MISSING {name}: the build generated it, the tool did not");
			failures++;
			continue;
		}

		var expected = File.ReadAllBytes(Path.Combine(built, name));
		var actual   = File.ReadAllBytes(path);

		if (name == Embedded)
		{
			if (expected.AsSpan().SequenceEqual(actual))
				continue;

			if (WithoutCarriageReturns(expected) == WithoutCarriageReturns(actual))
			{
				Console.WriteLine($"  note    {name}: equal but for line ends (Roslyn's text, by design)");
				continue;
			}

			Console.WriteLine($"  DIFFERS {name}: not even without line ends");
			failures++;
			continue;
		}

		if (relative is not null)
		{
			expected = Absolute(expected, relative);
			actual   = Absolute(actual, relative);
		}

		if (!expected.AsSpan().SequenceEqual(actual))
		{
			Console.WriteLine($"  DIFFERS {name}: {FirstDifference(expected, actual)}");
			failures++;
		}
	}

	foreach (var extra in Directory.EnumerateFiles(tool).Select(static file => Path.GetFileName(file)).Order(StringComparer.Ordinal))
	{
		if (extra != ".dotgram.json" && !names.Contains(extra))
		{
			Console.WriteLine($"  EXTRA   {extra}: the tool wrote it, the build did not");
			failures++;
		}
	}

	Console.WriteLine(failures == 0
		? $"  equal: {names.Count} files{(relative is null ? "" : " (#line paths read against the output folder)")}"
		: $"  {failures} differences in {names.Count} files");

	return failures == 0 ? 0 : 1;
}

static string WithoutCarriageReturns(byte[] bytes)
{
	return Encoding.UTF8.GetString(bytes).Replace("\r", "", StringComparison.Ordinal);
}

// The text with every #line path made full: a relative one read against the output folder, an absolute
// one normalized (a build writes `p/../grams/x.gram` as MSBuild gave it, the tool's relative path reads
// back as `grams/x.gram`), the bytes otherwise untouched: the only difference the relative default is
// allowed.
static byte[] Absolute(byte[] bytes, string folder)
{
	var preamble = Encoding.UTF8.GetPreamble();
	var bom      = bytes.AsSpan().StartsWith(preamble);
	var text     = Encoding.UTF8.GetString(bom ? bytes[preamble.Length..] : bytes);

	var rewritten = Regex.Replace(text, "^(#line [^\"\\r\\n]*\")([^\"\\r\\n]*)(\")", match =>
	{
		// Taken as written: on Linux, where this runs, MSBuild cannot build a project whose path has a
		// backslash or a quote, so there is nothing to unescape.
		var path = match.Groups[2].Value;
		var full = Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(folder, path));

		return match.Groups[1].Value + full + match.Groups[3].Value;
	}, RegexOptions.Multiline);

	var body = Encoding.UTF8.GetBytes(rewritten);

	return bom ? [.. preamble, .. body] : body;
}

static string FirstDifference(byte[] expected, byte[] actual)
{
	var left  = Encoding.UTF8.GetString(expected).Split('\n');
	var right = Encoding.UTF8.GetString(actual).Split('\n');

	for (var i = 0; i < Math.Min(left.Length, right.Length); i++)
	{
		if (left[i] != right[i])
			return $"line {i + 1}: build '{Clip(left[i])}', tool '{Clip(right[i])}'";
	}

	if (left.Length != right.Length)
		return $"{left.Length} lines in the build's, {right.Length} in the tool's";

	return $"same lines, other bytes ({expected.Length} vs {actual.Length})";
}

static string Clip(string line)
{
	line = line.TrimEnd('\r');

	return line.Length > 160 ? line[..160] + "..." : line;
}

static int HostByHand(string argumentsFile, string built, string projectDirectory)
{
	using var json = JsonDocument.Parse(File.ReadAllText(argumentsFile));

	var arguments = json.RootElement.GetProperty("Items").GetProperty("CscCommandLineArgs")
		.EnumerateArray()
		.Select(static item => item.GetProperty("Identity").GetString()!)
		.ToArray();

	if (arguments.Length == 0)
	{
		Console.WriteLine("  no compiler arguments: the build did not run the compiler");
		return 1;
	}

	var parsed = CSharpCommandLineParser.Default.Parse(arguments, projectDirectory, sdkDirectory: null);

	foreach (var error in parsed.Errors.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error))
		Console.WriteLine("  argument: " + error);

	var encoding = parsed.Encoding ?? new UTF8Encoding(false);

	var trees = parsed.SourceFiles
		.AsParallel().AsOrdered()
		.Select(file =>
		{
			using var stream = File.OpenRead(file.Path);

			return CSharpSyntaxTree.ParseText(SourceText.From(stream, encoding, parsed.ChecksumAlgorithm), parsed.ParseOptions, file.Path);
		})
		.ToList();

	var references = parsed.MetadataReferences
		.Select(static reference => (MetadataReference)MetadataReference.CreateFromFile(reference.Reference, reference.Properties))
		.ToList();

	var compilation = CSharpCompilation.Create(parsed.CompilationName, trees, references, parsed.CompilationOptions);

	var configs = parsed.AnalyzerConfigPaths
		.Select(static path => AnalyzerConfig.Parse(SourceText.From(File.ReadAllText(path)), path))
		.ToList();

	var set = AnalyzerConfigSet.Create(configs);

	var additional = parsed.AdditionalFiles
		.Select(static file => (AdditionalText)new FileText(file.Path))
		.ToImmutableArray();

	var dotgram = parsed.AnalyzerReferences
		.Select(static reference => reference.FilePath)
		.SingleOrDefault(static path => Path.GetFileName(path).Equals("DotGram.dll", StringComparison.OrdinalIgnoreCase));

	if (dotgram is null)
	{
		Console.WriteLine("  the build ran no DotGram generator");
		return 1;
	}

	var generator = Assembly.LoadFrom(dotgram).GetTypes()
		.Where(static type => typeof(IIncrementalGenerator).IsAssignableFrom(type) && type.GetCustomAttribute<GeneratorAttribute>() is not null)
		.Select(static type => ((IIncrementalGenerator)Activator.CreateInstance(type)!).AsSourceGenerator())
		.Single();

	var driver = CSharpGeneratorDriver.Create([generator], additional, (CSharpParseOptions)parsed.ParseOptions, new Provider(set));
	var result = driver.RunGenerators(compilation).GetRunResult().Results.Single();

	if (result.Exception is not null)
	{
		Console.WriteLine("  the generator threw: " + result.Exception);
		return 1;
	}

	var failures = 0;
	var produced = result.GeneratedSources.ToDictionary(static source => source.HintName, static source => source.SourceText.ToString(), StringComparer.Ordinal);
	var names    = Built(built).ToList();

	foreach (var name in names)
	{
		var bytes    = File.ReadAllBytes(Path.Combine(built, name));
		var preamble = Encoding.UTF8.GetPreamble();
		var text     = Encoding.UTF8.GetString(bytes.AsSpan().StartsWith(preamble) ? bytes[preamble.Length..] : bytes);

		if (!produced.TryGetValue(name, out var own))
		{
			Console.WriteLine($"  MISSING {name}");
			failures++;
		}
		else if (name == Embedded ? text.Replace("\r", "", StringComparison.Ordinal) != own.Replace("\r", "", StringComparison.Ordinal) : text != own)
		{
			Console.WriteLine($"  DIFFERS {name}: {FirstDifference(Encoding.UTF8.GetBytes(text), Encoding.UTF8.GetBytes(own))}");
			failures++;
		}
	}

	foreach (var extra in produced.Keys.Where(name => !name.Contains(".DotGramReport", StringComparison.Ordinal) && !names.Contains(name)))
	{
		Console.WriteLine($"  EXTRA   {extra}");
		failures++;
	}

	Console.WriteLine(failures == 0 ? $"  equal: {names.Count} files" : $"  {failures} differences in {names.Count} files");

	return failures == 0 ? 0 : 1;
}

sealed class FileText(string path) : AdditionalText
{
	public override string Path { get; } = path;

	public override SourceText GetText(CancellationToken cancellationToken = default)
	{
		using var stream = File.OpenRead(Path);

		return SourceText.From(stream);
	}
}

sealed class Options(ImmutableDictionary<string, string> values) : AnalyzerConfigOptions
{
	public override IEnumerable<string> Keys
	{
		get { return values.Keys; }
	}

	public override bool TryGetValue(string key, out string value)
	{
		return values.TryGetValue(key, out value!);
	}
}

sealed class Provider(AnalyzerConfigSet set) : AnalyzerConfigOptionsProvider
{
	public override AnalyzerConfigOptions GlobalOptions { get; } = new Options(set.GlobalConfigOptions.AnalyzerOptions);

	public override AnalyzerConfigOptions GetOptions(SyntaxTree tree)
	{
		return new Options(set.GetOptionsForSourcePath(tree.FilePath).AnalyzerOptions);
	}

	public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
	{
		return new Options(set.GetOptionsForSourcePath(textFile.Path).AnalyzerOptions);
	}
}
