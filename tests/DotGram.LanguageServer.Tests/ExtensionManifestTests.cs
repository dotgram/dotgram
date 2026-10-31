using System;
using System.IO;
using System.Linq;
using System.Text.Json;

using Xunit;

namespace DotGram.LanguageServer.Tests;

/// <summary>What the extension's manifest promises about the program it runs.</summary>
public sealed class ExtensionManifestTests
{
	static readonly Lazy<JsonElement> Manifest = new(static () =>
		JsonDocument.Parse(File.ReadAllText(Path.Combine(LspClient.SourceRoot(), "editors", "vscode", "package.json"))).RootElement.Clone());

	/// <summary>Which program runs the server is the machine's to say, never a workspace's: a cloned folder cannot choose an executable.</summary>
	[Fact]
	public void The_settings_that_choose_a_program_are_the_machines()
	{
		var properties = Manifest.Value.GetProperty("contributes").GetProperty("configuration").GetProperty("properties");
		var restricted = Manifest.Value.GetProperty("capabilities").GetProperty("untrustedWorkspaces").GetProperty("restrictedConfigurations")
			.EnumerateArray().Select(static item => item.GetString()).ToArray();

		foreach (var setting in new[] { "dotgram.server.path", "dotgram.dotnetPath" })
		{
			Assert.Equal("machine", properties.GetProperty(setting).GetProperty("scope").GetString());
			Assert.Contains(setting, restricted);
		}
	}
}
