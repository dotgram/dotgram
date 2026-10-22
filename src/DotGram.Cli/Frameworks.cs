using System;
using System.Collections.Immutable;
using System.Globalization;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DotGram.Cli;

/// <summary>
/// A target framework as the SDK would compile for it: the reference assemblies the tool carries for
/// it, the symbols the SDK defines for it, and the C# version the SDK picks for it when a project
/// names none.
/// </summary>
/// <param name="Moniker">As written: <c>net10.0</c>, <c>netstandard2.0</c>.</param>
/// <param name="ImplicitSymbols">
/// What the SDK adds to <c>DefineConstants</c> for the framework — <c>NET10_0</c>,
/// <c>NET10_0_OR_GREATER</c> and the rest — for a <c>.gram</c> input, where there is no project to
/// ask. A project's are asked of the SDK itself (ProjectEvaluation).
/// </param>
/// <param name="DefaultLanguageVersion">The SDK's default <c>LangVersion</c> for the framework.</param>
/// <param name="PackageFolders">
/// The <c>lib/</c> folders of a package that fit, nearest first: what NuGet would pick.
/// </param>
sealed record Framework(
	string                           Moniker,
	ImmutableArray<MetadataReference> References,
	ImmutableArray<string>           ImplicitSymbols,
	LanguageVersion                  DefaultLanguageVersion,
	ImmutableArray<string>           PackageFolders)
{
	/// <summary>What <c>--framework</c> takes, and what a project may target.</summary>
	public const string Supported = "net8.0, net9.0, net10.0 (and later .NET, compiled against net10.0's references), netstandard2.0";

	static readonly Lazy<ImmutableArray<MetadataReference>> Net80 = new(static () =>
		[.. Basic.Reference.Assemblies.Net80.References.All]);

	static readonly Lazy<ImmutableArray<MetadataReference>> Net90 = new(static () =>
		[.. Basic.Reference.Assemblies.Net90.References.All]);

	static readonly Lazy<ImmutableArray<MetadataReference>> Net100 = new(static () =>
		[.. Basic.Reference.Assemblies.Net100.References.All]);

	static readonly Lazy<ImmutableArray<MetadataReference>> NetStandard20 = new(static () =>
		[.. Basic.Reference.Assemblies.NetStandard20.References.All]);

	/// <summary>The framework a moniker names, or null for one the tool carries no references for.</summary>
	public static Framework? Of(string moniker)
	{
		var name = moniker.Trim().ToLowerInvariant();

		// A platform suffix (net10.0-windows) adds the platform's own references, which are not
		// carried; the framework under it is what the grammar's types come from.
		var dash = name.IndexOf('-');

		if (dash > 0)
			name = name.Substring(0, dash);

		if (name == "netstandard2.0")
			return new Framework(
				moniker,
				NetStandard20.Value,
				["NETSTANDARD", "NETSTANDARD2_0", .. Upward("NETSTANDARD", ["1_0", "1_1", "1_2", "1_3", "1_4", "1_5", "1_6", "2_0"])],
				LanguageVersion.CSharp7_3,
				["netstandard2.0", "netstandard1.6", "netstandard1.5", "netstandard1.4", "netstandard1.3", "netstandard1.2", "netstandard1.1", "netstandard1.0"]);

		if (name.StartsWith("net", StringComparison.Ordinal) &&
			name.EndsWith(".0", StringComparison.Ordinal) &&
			int.TryParse(name.AsSpan(3, name.Length - 5), NumberStyles.None, CultureInfo.InvariantCulture, out var major) &&
			major >= 8)
		{
			var versions = new List<string>();

			for (var version = 5; version <= major; version++)
				versions.Add(version.ToString(CultureInfo.InvariantCulture) + "_0");

			var folders = new List<string>();

			for (var version = major; version >= 5; version--)
				folders.Add("net" + version.ToString(CultureInfo.InvariantCulture) + ".0");

			folders.AddRange(["netcoreapp3.1", "netcoreapp3.0", "netcoreapp2.2", "netcoreapp2.1", "netcoreapp2.0", "netstandard2.1"]);
			folders.AddRange(Of("netstandard2.0")!.PackageFolders);

			var majorText = major.ToString(CultureInfo.InvariantCulture);

			// Each framework's own references, so that a type a later framework added is missing where it
			// is missing in a build; a framework newer than the tool knows takes the newest it carries.
			var references = major switch
			{
				8 => Net80.Value,
				9 => Net90.Value,
				_ => Net100.Value,
			};

			return new Framework(
				moniker,
				references,
				[
					"NET", "NET" + majorText + "_0", "NETCOREAPP",
					.. Upward("NET", versions),
					.. Upward("NETCOREAPP", ["1_0", "1_1", "2_0", "2_1", "2_2", "3_0", "3_1"]),
				],
				major switch
				{
					8  => LanguageVersion.CSharp12,
					9  => LanguageVersion.CSharp13,
					_  => LanguageVersion.CSharp14,
				},
				[.. folders]);
		}

		return null;
	}

	static IEnumerable<string> Upward(string prefix, IEnumerable<string> versions)
	{
		foreach (var version in versions)
			yield return prefix + version + "_OR_GREATER";
	}
}
