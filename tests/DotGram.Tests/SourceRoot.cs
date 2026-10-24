using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DotGram;

/// <summary>
/// Where the repository's sources are, for a test that reads them.
/// </summary>
/// <remarks>
/// The build says where: <c>Directory.Build.props</c> writes the directory it stands in into every
/// test assembly as <c>DotGramSourceRoot</c>. Walking up from the binary to <c>DotGram.slnx</c> finds
/// the same place only while the binary is built under the repository, and a build whose output is
/// asked to go elsewhere (<c>-p:ArtifactsPath=…</c>, as a build on a memory disk does) has no
/// repository above it: every test that read a source file failed at once, together, and told nothing
/// of why. The walk stays as the fallback, for an assembly built without the property.
/// </remarks>
static class SourceRoot
{
	public static string Directory { get; } = Find();

	static string Find()
	{
		var declared = typeof(SourceRoot).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
			.FirstOrDefault(static one => one.Key == "DotGramSourceRoot")?.Value;

		if (declared is { Length: > 0 } && File.Exists(Path.Combine(declared, "DotGram.slnx")))
			return Path.GetFullPath(declared);

		var at = new DirectoryInfo(AppContext.BaseDirectory);

		while (at is not null && !File.Exists(Path.Combine(at.FullName, "DotGram.slnx")))
			at = at.Parent;

		return at?.FullName ?? throw new InvalidOperationException(
			"The repository root is neither declared by the build nor above the test binaries (" + AppContext.BaseDirectory + ").");
	}
}
