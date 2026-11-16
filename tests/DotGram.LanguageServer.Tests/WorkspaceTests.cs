using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using DotGram.Language;

using Xunit;

namespace DotGram.LanguageServer.Tests;

/// <summary>
/// The workspace a grammar's host is looked for in, as the build would see it: one project per
/// <c>.csproj</c>, every C# file of a project with its global usings and its base classes, and the
/// folders the client has now.
/// </summary>
public sealed class WorkspaceTests : IDisposable
{
	readonly string _folder = Path.Combine(Path.GetTempPath(), "DotGram.LanguageServer.Tests", Guid.NewGuid().ToString("N"));

	public WorkspaceTests()
	{
		Directory.CreateDirectory(_folder);
	}

	public void Dispose()
	{
		Directory.Delete(_folder, recursive: true);
	}

	const string Project = "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <PropertyGroup>\n    <TargetFramework>net10.0</TargetFramework>\n  </PropertyGroup>\n</Project>\n";
	const string Dialect = "trivia = [' ']*\nStart = B.Word\nparse Start\n";

	static string Hosts(bool lexical)
	{
		return $$"""
			using DotGram;

			namespace Parsers;

			[Gram("Base.gram", DefaultAlias = "B")]
			public partial class Base;

			[Gram("Dialect.gram", Lexical = {{(lexical ? "true" : "false")}})]
			[GramInclude(typeof(Base))]
			public partial class Dialect;
			""";
	}

	/// <summary>
	/// Two projects in one folder, each with a host of the same name for a grammar of the same name:
	/// each grammar is read with its own project's host and includes its own project's file.
	/// </summary>
	[Fact]
	public async Task Each_project_reads_its_grammar_with_its_own_host()
	{
		foreach (var (name, lexical) in new[] { ("A", true), ("B", false) })
		{
			var directory = Directory.CreateDirectory(Path.Combine(_folder, name)).FullName;

			File.WriteAllText(Path.Combine(directory, name + ".csproj"), Project);
			File.WriteAllText(Path.Combine(directory, "Hosts.cs"), Hosts(lexical));
			File.WriteAllText(Path.Combine(directory, "Base.gram"), "Word = ['a'..'z']+\n");
			File.WriteAllText(Path.Combine(directory, "Dialect.gram"), Dialect);
		}

		await using var client = await LspClient.StartAsync(null, _folder);

		foreach (var (name, lexical) in new[] { ("A", true), ("B", false) })
		{
			var uri = LspClient.Uri(Path.Combine(_folder, name, "Dialect.gram"));

			await client.OpenAsync(uri, Dialect);

			var context = (await HostTests.ResolvedAsync(client.Server.Document(uri)!.Session)).Resolution.Context!.Value;

			Assert.Equal(lexical, context.Lexical);
			Assert.Equal(Path.Combine(_folder, name, "Base.gram"), context.Included.Single().FilePath);
		}
	}

	/// <summary>A host that names <c>[Gram]</c> through a project's <c>global using DotGram;</c> is found.</summary>
	[Fact]
	public async Task A_host_is_read_with_its_projects_global_usings()
	{
		File.WriteAllText(Path.Combine(_folder, "Parsers.csproj"), Project);
		File.WriteAllText(Path.Combine(_folder, "GlobalUsings.cs"), "global using DotGram;\n");
		File.WriteAllText(Path.Combine(_folder, "Hosts.cs"), Hosts(lexical: true).Replace("using DotGram;", "", StringComparison.Ordinal));
		File.WriteAllText(Path.Combine(_folder, "Base.gram"), "Word = ['a'..'z']+\n");
		File.WriteAllText(Path.Combine(_folder, "Dialect.gram"), Dialect);

		var uri = LspClient.Uri(Path.Combine(_folder, "Dialect.gram"));

		await using var client = await LspClient.StartAsync(null, _folder);

		await client.OpenAsync(uri, Dialect);

		var context = (await HostTests.ResolvedAsync(client.Server.Document(uri)!.Session)).Resolution.Context!.Value;

		Assert.True(context.Lexical);
		Assert.Equal(["B"], context.Included.Select(static item => item.Name));
	}

	/// <summary>A host that names <c>[Gram]</c> through its project file's <c>&lt;Using Include="DotGram" /&gt;</c> is found.</summary>
	[Fact]
	public async Task A_host_is_read_with_its_project_files_usings()
	{
		File.WriteAllText(Path.Combine(_folder, "Parsers.csproj"), Project.Replace("</Project>", "  <ItemGroup>\n    <Using Include=\"DotGram\" />\n  </ItemGroup>\n</Project>", StringComparison.Ordinal));
		File.WriteAllText(Path.Combine(_folder, "Hosts.cs"), Hosts(lexical: true).Replace("using DotGram;", "", StringComparison.Ordinal));
		File.WriteAllText(Path.Combine(_folder, "Base.gram"), "Word = ['a'..'z']+\n");
		File.WriteAllText(Path.Combine(_folder, "Dialect.gram"), Dialect);

		var uri = LspClient.Uri(Path.Combine(_folder, "Dialect.gram"));

		await using var client = await LspClient.StartAsync(null, _folder);

		await client.OpenAsync(uri, Dialect);

		var context = (await HostTests.ResolvedAsync(client.Server.Document(uri)!.Session)).Resolution.Context!.Value;

		Assert.True(context.Lexical);
		Assert.Equal(["B"], context.Included.Select(static item => item.Name));
	}

	/// <summary>
	/// A grammar a host takes from a base class of its base class is included, where the class between
	/// the two declares no grammar of its own and names no grammar attribute.
	/// </summary>
	[Fact]
	public async Task A_grammar_is_included_through_a_base_class_without_a_grammar()
	{
		File.WriteAllText(Path.Combine(_folder, "Parsers.csproj"), Project);
		File.WriteAllText(Path.Combine(_folder, "Base.cs"), "using DotGram;\n\nnamespace Parsers;\n\n[Gram(\"Base.gram\", DefaultAlias = \"B\")]\npublic abstract partial class Base;\n");
		File.WriteAllText(Path.Combine(_folder, "Middle.cs"), "namespace Parsers;\n\npublic abstract partial class Middle : Base;\n");
		File.WriteAllText(Path.Combine(_folder, "Dialect.cs"), "using DotGram;\n\nnamespace Parsers;\n\n[Gram(\"Dialect.gram\")]\npublic partial class Dialect : Middle;\n");
		File.WriteAllText(Path.Combine(_folder, "Base.gram"), "Word = ['a'..'z']+\n");
		File.WriteAllText(Path.Combine(_folder, "Dialect.gram"), Dialect);

		var uri = LspClient.Uri(Path.Combine(_folder, "Dialect.gram"));

		await using var client = await LspClient.StartAsync(null, _folder);

		await client.OpenAsync(uri, Dialect);

		var context = (await HostTests.ResolvedAsync(client.Server.Document(uri)!.Session)).Resolution.Context!.Value;

		Assert.Equal(["B"], context.Included.Select(static item => item.Name));
	}

	[Fact]
	public async Task The_server_says_it_follows_the_workspace_folders()
	{
		await using var client = new LspClient();

		var result  = await client.InitializeAsync(_folder);
		var folders = result.GetProperty("capabilities").GetProperty("workspace").GetProperty("workspaceFolders");

		Assert.True(folders.GetProperty("supported").GetBoolean());
		Assert.True(folders.GetProperty("changeNotifications").GetBoolean());
	}

	/// <summary>A folder added to the workspace while a grammar in it is open: the grammar's host is looked for there.</summary>
	[Fact]
	public async Task A_folder_added_to_the_workspace_has_its_hosts_found()
	{
		var other  = Directory.CreateDirectory(Path.Combine(_folder, "other")).FullName;
		var added  = Directory.CreateDirectory(Path.Combine(_folder, "added")).FullName;
		var path   = Path.Combine(added, "Dialect.gram");
		var uri    = LspClient.Uri(path);

		File.WriteAllText(Path.Combine(added, "Hosts.cs"), Hosts(lexical: true));
		File.WriteAllText(Path.Combine(added, "Base.gram"), "Word = ['a'..'z']+\n");
		File.WriteAllText(path, Dialect);

		await using var client = await LspClient.StartAsync(null, other);

		await client.OpenAsync(uri, Dialect);
		await client.DiagnosticsAsync(uri, 1);

		Assert.True(client.Server.Document(uri)!.Session.Store.IsCopy(path));

		await client.NotifyAsync("workspace/didChangeWorkspaceFolders", new
		{
			@event = new
			{
				added   = new[] { new { uri = LspClient.Uri(added), name = "added" } },
				removed = Array.Empty<object>(),
			},
		});

		var context = (await ResolvedAsync(client, uri)).Resolution.Context!.Value;

		Assert.True(context.Lexical);
		Assert.Equal(Path.Combine(added, "Base.gram"), context.Included.Single().FilePath);
	}

	/// <summary>A folder taken out of the workspace: its grammars are read as they stand, and its C# is no longer asked.</summary>
	[Fact]
	public async Task A_folder_removed_from_the_workspace_no_longer_hosts()
	{
		var other   = Directory.CreateDirectory(Path.Combine(_folder, "other")).FullName;
		var removed = Directory.CreateDirectory(Path.Combine(_folder, "removed")).FullName;
		var path    = Path.Combine(removed, "Dialect.gram");
		var uri     = LspClient.Uri(path);

		File.WriteAllText(Path.Combine(removed, "Hosts.cs"), Hosts(lexical: true));
		File.WriteAllText(Path.Combine(removed, "Base.gram"), "Word = ['a'..'z']+\n");
		File.WriteAllText(path, Dialect);

		await using var client = await LspClient.StartAsync(null, other, removed);

		await client.OpenAsync(uri, Dialect);
		await ResolvedAsync(client, uri);

		await client.NotifyAsync("workspace/didChangeWorkspaceFolders", new
		{
			@event = new
			{
				added   = Array.Empty<object>(),
				removed = new[] { new { uri = LspClient.Uri(removed), name = "removed" } },
			},
		});

		var deadline = DateTime.UtcNow + LspClient.Timeout;

		while (!(client.Server.Document(uri) is { } document && document.Session.Store.IsCopy(path) && document.Session.Result is not null))
		{
			Assert.True(DateTime.UtcNow < deadline, "The grammar is still read with the removed folder's host.");
			await Task.Delay(50, TestContext.Current.CancellationToken);
		}

		var folder = await client.Server.Folder;

		Assert.False(folder.Contains(path));
		Assert.DoesNotContain(
			folder.Workspace.CurrentSolution.Projects.SelectMany(static project => project.Documents),
			document => document.FilePath is { } file && file.StartsWith(removed, StringComparison.Ordinal));
	}

	/// <summary>The grammar's session settled with its host found; the document may have been opened again by the server meanwhile.</summary>
	static async Task<GramAnalysisResult<GramVersion>> ResolvedAsync(LspClient client, string uri)
	{
		var deadline = DateTime.UtcNow + LspClient.Timeout;

		while (DateTime.UtcNow < deadline)
		{
			if (client.Server.Document(uri)?.Session.Result is { Resolution.State: StandaloneGrammarState.Resolved } result)
				return result;

			await Task.Delay(50, TestContext.Current.CancellationToken);
		}

		Assert.Fail($"The host is {client.Server.Document(uri)?.Session.Resolution.State}, not resolved.");
		return null;
	}
}
