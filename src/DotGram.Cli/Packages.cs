using System;

namespace DotGram.Cli;

/// <summary>
/// <c>--package Name@Version</c>: a package's assemblies from the global packages folder, without a
/// restore.
/// </summary>
static class Packages
{
	public static bool TryParse(string text, out string name, out string version)
	{
		var at = text.LastIndexOf('@');

		name    = at > 0 ? text.Substring(0, at).Trim() : "";
		version = at > 0 ? text.Substring(at + 1).Trim() : "";

		return name.Length > 0 && version.Length > 0;
	}

	/// <summary>Where NuGet keeps what it restored: <c>NUGET_PACKAGES</c>, or the user's default.</summary>
	public static string Folder()
	{
		var configured = Environment.GetEnvironmentVariable("NUGET_PACKAGES");

		return !string.IsNullOrWhiteSpace(configured)
			? Path.GetFullPath(configured)
			: Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");
	}

	/// <summary>The assemblies of the package's <c>lib/</c> folder nearest to the framework.</summary>
	/// <exception cref="ToolException">The package is not there, or has nothing for the framework.</exception>
	public static IReadOnlyList<string> Assemblies(string package, Framework framework)
	{
		TryParse(package, out var name, out var version);

		var root      = Folder();
		var directory = Path.Combine(root, name.ToLowerInvariant(), version.ToLowerInvariant());

		if (!Directory.Exists(directory))
			throw ToolException.Failure(
				$"package {name} {version} is not in the global packages folder ({root}). " +
				"Run `dotnet restore` on a project that references it, or pass the assembly with --reference.");

		var lib = Path.Combine(directory, "lib");

		foreach (var folder in framework.PackageFolders)
		{
			var candidate = Path.Combine(lib, folder);

			if (Directory.Exists(candidate))
			{
				var assemblies = Directory.GetFiles(candidate, "*.dll").Order(StringComparer.Ordinal).ToArray();

				// `_._` is NuGet's "the framework has this already": nothing to reference.
				if (assemblies.Length > 0 || File.Exists(Path.Combine(candidate, "_._")))
					return assemblies;
			}
		}

		throw ToolException.Failure($"package {name} {version} has no lib/ folder for {framework.Moniker} (looked in {lib}).");
	}
}
