using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.Loader;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DotGram.Cli;

/// <summary>
/// The project's other source generators — its packages' and its framework's — loaded so that they
/// run beside DotGram's, as they run in a build: what they add to the compilation is then there when
/// the tool asks which names the project's own files cannot resolve.
/// </summary>
/// <remarks>
/// Generators do not see each other's output, so running them changes nothing DotGram writes.
/// Diagnostic analyzers are not run.
/// </remarks>
static class OtherGenerators
{
	/// <summary>The generators of the given analyzer assemblies, and a warning for each that would not load.</summary>
	public static (List<ISourceGenerator> Generators, List<string> Warnings) Load(IEnumerable<string> assemblies)
	{
		var generators = new List<ISourceGenerator>();
		var warnings   = new List<string>();

		foreach (var path in assemblies.Distinct(StringComparer.Ordinal))
		{
			if (!File.Exists(path) || !HasGenerator(path))
				continue;

			// DotGram's own, from a package the project still references: the tool's generator is run already.
			if (string.Equals(Path.GetFileName(path), "DotGram.dll", StringComparison.OrdinalIgnoreCase))
				continue;

			var reference = new AnalyzerFileReference(path, Loader.Instance);
			var failed    = new List<string>();

			reference.AnalyzerLoadFailed += (_, failure) => failed.Add(failure.Message);

			var found = reference.GetGenerators(LanguageNames.CSharp);

			if (failed.Count > 0)
				warnings.Add($"the generator in {path} could not be loaded ({string.Join("; ", failed)}); names it would add count as unresolved.");

			generators.AddRange(found);
		}

		return (generators, warnings);
	}

	/// <summary>
	/// Whether the assembly declares a generator, read from its metadata without loading it: a type
	/// carrying <c>[Generator]</c> or implementing one of the two generator interfaces. Most analyzer
	/// assemblies have none, and are large.
	/// </summary>
	public static bool HasGenerator(string path)
	{
		try
		{
			using var stream = File.OpenRead(path);
			using var pe     = new PEReader(stream);

			if (!pe.HasMetadata)
				return false;

			var reader = pe.GetMetadataReader();

			foreach (var handle in reader.TypeReferences)
			{
				var type = reader.GetTypeReference(handle);

				if (reader.StringComparer.Equals(type.Namespace, "Microsoft.CodeAnalysis") &&
					(reader.StringComparer.Equals(type.Name, "GeneratorAttribute") ||
					 reader.StringComparer.Equals(type.Name, "IIncrementalGenerator") ||
					 reader.StringComparer.Equals(type.Name, "ISourceGenerator")))
					return true;
			}

			return false;
		}
		catch (Exception exception) when (exception is BadImageFormatException or IOException or UnauthorizedAccessException)
		{
			return false;
		}
	}

	/// <summary>
	/// Loads each analyzer assembly in a context of its own, its dependencies from beside it, and
	/// Roslyn's own assemblies from the tool, so that the generator meets the compilation it is run on.
	/// </summary>
	sealed class Loader : IAnalyzerAssemblyLoader
	{
		public static readonly Loader Instance = new();

		readonly ConcurrentDictionary<string, Assembly> _loaded = new(StringComparer.Ordinal);

		public void AddDependencyLocation(string fullPath)
		{
		}

		public Assembly LoadFromPath(string fullPath)
		{
			return _loaded.GetOrAdd(fullPath, static path => new Context(path).LoadFromAssemblyPath(path));
		}

		sealed class Context(string path) : AssemblyLoadContext("DotGram.Cli analyzer " + Path.GetFileName(path))
		{
			readonly string _directory = Path.GetDirectoryName(path)!;

			protected override Assembly? Load(AssemblyName name)
			{
				// The tool's own Roslyn and the runtime: one copy of the types a generator implements and
				// hands back (ImmutableArray among them), whatever copies the package carries.
				if (name.Name is not { } simple ||
					simple.StartsWith("Microsoft.CodeAnalysis", StringComparison.Ordinal) ||
					simple.StartsWith("System.", StringComparison.Ordinal) ||
					simple is "System" or "mscorlib" or "netstandard")
					return null;

				var beside = Path.Combine(_directory, name.Name + ".dll");

				return File.Exists(beside) ? LoadFromAssemblyPath(beside) : null;
			}
		}
	}
}
