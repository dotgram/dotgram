using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.Loader;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DotGram.Cli;

/// <summary>What a project's analyzer list comes to for the tool.</summary>
/// <param name="Generators">Every source generator in it except DotGram's, loaded.</param>
/// <param name="DotGramVersion">The version of the assembly named DotGram in the list, or null where there is none.</param>
/// <param name="HasDotGram">Whether the list names an assembly called DotGram.</param>
sealed record AnalyzerSet(IReadOnlyList<ISourceGenerator> Generators, bool HasDotGram, string? DotGramVersion, IReadOnlyList<string> Warnings);

/// <summary>
/// The source generators among a compiler's <c>/analyzer:</c> assemblies, loaded so that they run
/// beside DotGram's, as they run in a build: what they add to the compilation is then there when the
/// project's sources are checked, and what they are told is what the build tells them.
/// </summary>
/// <remarks>
/// Generators do not see each other's output, so running them changes nothing DotGram writes.
/// Diagnostic analyzers are not run.
/// </remarks>
static class AnalyzerGenerators
{
	/// <summary>The assembly the tool carries the generator of.</summary>
	const string DotGramAssembly = "DotGram";

	/// <summary>
	/// Loads the generators of the given analyzer assemblies, all through one loader that knows every
	/// directory they are in, as the compiler does: a dependency of one may sit beside another.
	/// </summary>
	/// <exception cref="ToolException">A generator would not load; the message names its assembly.</exception>
	public static AnalyzerSet Load(IEnumerable<string> assemblies)
	{
		var paths      = assemblies.Distinct(StringComparer.Ordinal).ToList();
		var loader     = new Loader(paths.Select(static path => Path.GetDirectoryName(path)!).Distinct(StringComparer.Ordinal));
		var generators = new List<ISourceGenerator>();
		var warnings   = new List<string>();
		var hasDotGram = false;
		string? dotGramVersion = null;

		foreach (var path in paths)
		{
			if (!File.Exists(path))
			{
				// The compiler warns (CS8034) and goes on.
				warnings.Add($"the analyzer {path} does not exist: its generators are not run.");
				continue;
			}

			var (name, version) = Identity(path);

			// DotGram's own, whichever way the project has it: the tool's generator is the one that runs.
			if (string.Equals(name, DotGramAssembly, StringComparison.Ordinal))
			{
				if (!hasDotGram)
					dotGramVersion = version;

				hasDotGram = true;
				continue;
			}

			if (!HasGenerator(path))
				continue;

			var reference = new AnalyzerFileReference(path, loader);
			var failures  = new List<AnalyzerLoadFailureEventArgs>();

			reference.AnalyzerLoadFailed += (_, failure) => failures.Add(failure);

			System.Collections.Immutable.ImmutableArray<ISourceGenerator> found;

			try
			{
				found = reference.GetGenerators(LanguageNames.CSharp);
			}
			catch (Exception exception) when (exception is not ToolException)
			{
				throw ToolException.Failure($"the generator in {path} could not be loaded: {exception.Message}");
			}

			var fatal = failures.Where(static failure => failure.ErrorCode is not (AnalyzerLoadFailureEventArgs.FailureErrorCode.NoAnalyzers or AnalyzerLoadFailureEventArgs.FailureErrorCode.ReferencesFramework)).ToList();

			if (fatal.Any(static failure => failure.ErrorCode == AnalyzerLoadFailureEventArgs.FailureErrorCode.ReferencesNewerCompiler))
				throw ToolException.Failure($"the generator in {path} was built for a newer compiler than DotGram.Cli's: update DotGram.Cli.");

			if (fatal.Count > 0)
				throw ToolException.Failure($"the generator in {path} could not be loaded: {string.Join("; ", fatal.Select(static failure => failure.Message))}");

			generators.AddRange(found);
		}

		return new AnalyzerSet(generators, hasDotGram, dotGramVersion, warnings);
	}

	/// <summary>The name and the informational version of an assembly, read from its metadata without loading it.</summary>
	static (string? Name, string? Version) Identity(string path)
	{
		try
		{
			using var stream = File.OpenRead(path);
			using var pe     = new PEReader(stream);

			if (!pe.HasMetadata)
				return (null, null);

			var reader = pe.GetMetadataReader();

			if (!reader.IsAssembly)
				return (null, null);

			var definition = reader.GetAssemblyDefinition();
			string? informational = null;

			foreach (var handle in definition.GetCustomAttributes())
			{
				var attribute = reader.GetCustomAttribute(handle);

				if (TypeNameOf(reader, attribute) != "AssemblyInformationalVersionAttribute")
					continue;

				var blob = reader.GetBlobReader(attribute.Value);

				if (blob.ReadUInt16() == 1)
					informational = blob.ReadSerializedString();
			}

			var version = informational ?? definition.Version.ToString();
			var plus    = version.IndexOf('+');

			return (reader.GetString(definition.Name), plus < 0 ? version : version.Substring(0, plus));
		}
		catch (Exception exception) when (exception is BadImageFormatException or IOException or UnauthorizedAccessException)
		{
			return (null, null);
		}
	}

	static string? TypeNameOf(MetadataReader reader, CustomAttribute attribute)
	{
		switch (attribute.Constructor.Kind)
		{
			case HandleKind.MemberReference:
			{
				var parent = reader.GetMemberReference((MemberReferenceHandle)attribute.Constructor).Parent;

				return parent.Kind == HandleKind.TypeReference ? reader.GetString(reader.GetTypeReference((TypeReferenceHandle)parent).Name) : null;
			}

			case HandleKind.MethodDefinition:
			{
				var type = reader.GetMethodDefinition((MethodDefinitionHandle)attribute.Constructor).GetDeclaringType();

				return reader.GetString(reader.GetTypeDefinition(type).Name);
			}

			default:
				return null;
		}
	}

	/// <summary>
	/// Whether the assembly declares a generator, read from its metadata without loading it: a type
	/// carrying <c>[Generator]</c> or implementing one of the two generator interfaces. Most analyzer
	/// assemblies have none, and are large. One that cannot be read at all is not ruled out: loading it
	/// says why it will not.
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
			return true;
		}
	}

	/// <summary>
	/// Loads each analyzer assembly in a context of its own directory, its dependencies from beside it
	/// or, failing that, from beside another analyzer, and Roslyn's own assemblies from the tool, so
	/// that the generator meets the compilation it is run on.
	/// </summary>
	sealed class Loader(IEnumerable<string> directories) : IAnalyzerAssemblyLoader
	{
		readonly string[] _directories = [.. directories];

		readonly ConcurrentDictionary<string, Context>  _contexts = new(StringComparer.Ordinal);
		readonly ConcurrentDictionary<string, Assembly> _loaded   = new(StringComparer.Ordinal);

		public void AddDependencyLocation(string fullPath)
		{
		}

		public Assembly LoadFromPath(string fullPath)
		{
			return _loaded.GetOrAdd(fullPath, path => ContextOf(Path.GetDirectoryName(path)!).LoadFromAssemblyPath(path));
		}

		Context ContextOf(string directory)
		{
			return _contexts.GetOrAdd(directory, path => new Context(this, path));
		}

		sealed class Context(Loader loader, string directory) : AssemblyLoadContext("DotGram.Cli analyzers in " + directory)
		{
			protected override Assembly? Load(AssemblyName name)
			{
				// The tool's own Roslyn and the runtime: one copy of the types a generator implements and
				// hands back (ImmutableArray among them), whatever copies the package carries.
				if (name.Name is not { } simple ||
					simple.StartsWith("Microsoft.CodeAnalysis", StringComparison.Ordinal) ||
					simple.StartsWith("System.", StringComparison.Ordinal) ||
					simple is "System" or "mscorlib" or "netstandard")
					return null;

				var beside = Path.Combine(directory, simple + ".dll");

				if (File.Exists(beside))
					return LoadFromAssemblyPath(beside);

				foreach (var other in loader._directories)
				{
					var elsewhere = Path.Combine(other, simple + ".dll");

					if (File.Exists(elsewhere))
						return loader.ContextOf(other).LoadFromAssemblyPath(elsewhere);
				}

				return null;
			}
		}
	}
}
