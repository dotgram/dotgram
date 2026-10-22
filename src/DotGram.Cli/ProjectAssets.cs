using System;
using System.Globalization;
using System.Text.Json;

namespace DotGram.Cli;

/// <summary>
/// What restore resolved for one framework of a project, read from <c>obj/project.assets.json</c>: the
/// assemblies its packages give the compiler, and the analyzers they give it — chosen by NuGet's
/// rules, as the SDK chooses them, without running a target.
/// </summary>
sealed class ProjectAssets
{
	/// <summary>The packages' compile-time assemblies, full paths.</summary>
	public required List<string> References { get; init; }

	/// <summary>The packages' analyzer assemblies for C#, full paths; which of them hold generators is told later.</summary>
	public required List<string> Analyzers { get; init; }

	/// <summary>The version of the .NET targeting pack restore downloaded for the framework, where it did.</summary>
	public required string? TargetingPackVersion { get; init; }

	/// <summary>The newest Roslyn a versioned analyzer folder may ask for: the tool's own.</summary>
	static readonly Version Roslyn = typeof(Microsoft.CodeAnalysis.Compilation).Assembly.GetName().Version!;

	/// <exception cref="ToolException">No assets file — the project was not restored — or none for this framework.</exception>
	public static ProjectAssets Read(string? path, string framework, string project)
	{
		if (path is null || !File.Exists(path))
			throw ToolException.Failure(
				$"{project} has not been restored ({path ?? "no project.assets.json"} is missing): run `dotnet restore` on it first. " +
				"The tool reads what restore resolved and never restores itself.");

		using var json = JsonDocument.Parse(File.ReadAllText(path));

		var root    = json.RootElement;
		var folders = root.GetProperty("packageFolders").EnumerateObject().Select(static folder => folder.Name).ToList();

		if (!root.GetProperty("targets").TryGetProperty(framework, out var target))
			throw ToolException.Failure($"{path} holds nothing for {framework}: run `dotnet restore` on {project} again.");

		var libraries   = root.GetProperty("libraries");
		var frameworks  = root.GetProperty("project").GetProperty("frameworks");
		var direct      = frameworks.TryGetProperty(framework, out var own) && own.TryGetProperty("dependencies", out var dependencies) ? dependencies : default;
		var references  = new List<string>();
		var analyzers   = new List<string>();

		foreach (var package in target.EnumerateObject())
		{
			if (!package.Value.TryGetProperty("type", out var type) || type.GetString() != "package")
				continue;

			var name   = package.Name.Substring(0, package.Name.IndexOf('/'));
			var folder = libraries.TryGetProperty(package.Name, out var library) && library.TryGetProperty("path", out var relative)
				? Locate(folders, relative.GetString()!)
				: null;

			if (folder is null)
				continue;

			if (package.Value.TryGetProperty("compile", out var compile))
				foreach (var file in compile.EnumerateObject())
					if (file.Name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
						references.Add(Path.GetFullPath(Path.Combine(folder, file.Name)));

			if (Excludes(direct, name, "Analyzers"))
				continue;

			if (library.TryGetProperty("files", out var files))
				analyzers.AddRange(AnalyzerFiles([.. files.EnumerateArray().Select(static file => file.GetString()!)]).Select(file => Path.GetFullPath(Path.Combine(folder, file))));
		}

		string? pack = null;

		if (own.ValueKind == JsonValueKind.Object && own.TryGetProperty("downloadDependencies", out var downloads))
			foreach (var download in downloads.EnumerateArray())
				if (string.Equals(download.GetProperty("name").GetString(), "Microsoft.NETCore.App.Ref", StringComparison.OrdinalIgnoreCase))
					pack = download.GetProperty("version").GetString()!.Trim('[', ']', ' ').Split(',')[0].Trim();

		return new ProjectAssets { References = references, Analyzers = analyzers, TargetingPackVersion = pack };
	}

	/// <summary>The first packages folder that holds the package, as restore lists them.</summary>
	static string? Locate(List<string> folders, string relative)
	{
		foreach (var folder in folders)
		{
			var path = Path.Combine(folder, relative);

			if (Directory.Exists(path))
				return path;
		}

		return null;
	}

	/// <summary>Whether a package the project names itself keeps an asset out (<c>ExcludeAssets</c>, <c>IncludeAssets</c>).</summary>
	static bool Excludes(JsonElement direct, string package, string asset)
	{
		if (direct.ValueKind != JsonValueKind.Object || !direct.TryGetProperty(package, out var dependency))
			return false;

		static bool Names(JsonElement dependency, string property, string asset)
		{
			return dependency.TryGetProperty(property, out var value) &&
				value.GetString()!.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
					.Any(one => one.Equals(asset, StringComparison.OrdinalIgnoreCase) || one.Equals("All", StringComparison.OrdinalIgnoreCase));
		}

		if (Names(dependency, "exclude", asset))
			return true;

		return dependency.TryGetProperty("include", out _) && !Names(dependency, "include", asset);
	}

	/// <summary>
	/// A package's analyzers for C#, by NuGet's folder rules: <c>analyzers/dotnet/cs/</c> and
	/// <c>analyzers/dotnet/</c>, or — where the package has folders for Roslyn versions — only the
	/// newest <c>analyzers/dotnet/roslyn&lt;major&gt;.&lt;minor&gt;/</c> the compiler is not older than.
	/// </summary>
	public static List<string> AnalyzerFiles(IReadOnlyList<string> files)
	{
		var plain     = new List<string>();
		var versioned = new List<(Version Version, string File)>();

		foreach (var file in files)
		{
			var parts = file.Split('/');

			if (parts.Length < 3 || parts[0] != "analyzers" || parts[1] != "dotnet" || !parts[^1].EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
				continue;

			var at      = 2;
			Version? version = null;

			if (parts[at].StartsWith("roslyn", StringComparison.OrdinalIgnoreCase) &&
				Version.TryParse(parts[at].Substring("roslyn".Length), out var parsed))
			{
				version = parsed;
				at++;
			}

			// What is left is the file, or a language folder and the file.
			var language = parts.Length - at == 2 ? parts[at] : parts.Length - at == 1 ? null : "?";

			if (language is not (null or "cs"))
				continue;

			if (version is null)
				plain.Add(file);
			else
				versioned.Add((version, file));
		}

		if (versioned.Count == 0)
			return plain;

		var best = versioned
			.Select(static one => one.Version)
			.Where(static version => version <= new Version(Roslyn.Major, Roslyn.Minor))
			.DefaultIfEmpty()
			.Max();

		return best is null ? [] : [.. versioned.Where(one => one.Version == best).Select(static one => one.File)];
	}

	/// <summary>
	/// The framework's own generators — regular expressions, JSON, interop — from its targeting pack:
	/// the one restore downloaded, or the SDK's own for the framework it bundles.
	/// </summary>
	public static List<string> FrameworkAnalyzers(ProjectEvaluation evaluation, ProjectAssets assets, IReadOnlyList<string> packageFolders)
	{
		if (evaluation.FrameworkIdentifier != ".NETCoreApp" ||
			!Version.TryParse(evaluation.FrameworkVersion?.TrimStart('v', 'V'), out var version) || version.Major < 5)
			return [];

		var candidates = new List<string>();

		if (assets.TargetingPackVersion is { } downloaded)
			foreach (var folder in packageFolders)
				candidates.Add(Path.Combine(folder, "microsoft.netcore.app.ref", downloaded.ToLowerInvariant()));

		if (evaluation.TargetingPacks is { } packs)
		{
			var root = Path.Combine(packs, "Microsoft.NETCore.App.Ref");

			var major = version.Major.ToString(CultureInfo.InvariantCulture) + ".";

			if (evaluation.BundledVersion is { } bundled && bundled.StartsWith(major, StringComparison.Ordinal))
				candidates.Add(Path.Combine(root, bundled));

			if (Directory.Exists(root))
				candidates.AddRange(Directory.GetDirectories(root)
					.Where(path => Path.GetFileName(path).StartsWith(major, StringComparison.Ordinal))
					.OrderByDescending(static path => Path.GetFileName(path), StringComparer.Ordinal));
		}

		foreach (var candidate in candidates)
		{
			var analyzers = Path.Combine(candidate, "analyzers", "dotnet", "cs");

			if (Directory.Exists(analyzers))
				return [.. Directory.GetFiles(analyzers, "*.dll").Order(StringComparer.Ordinal)];
		}

		return [];
	}

	/// <summary>The packages folders an assets file names.</summary>
	public static List<string> PackageFolders(string path)
	{
		using var json = JsonDocument.Parse(File.ReadAllText(path));

		return [.. json.RootElement.GetProperty("packageFolders").EnumerateObject().Select(static folder => folder.Name)];
	}
}
