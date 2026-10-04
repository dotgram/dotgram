using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Emit;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Validates that all .gram code snippets compile successfully.
/// </summary>
/// <remarks>
/// Code snippets are templates for users, not production code, but they must be valid
/// and compilable. This test reads the VS Code snippets file, extracts all .gram snippets,
/// expands them with sensible defaults, and verifies they compile without errors.
/// </remarks>
public sealed class CodeSnippetsTests
{
	private static string SnippetsPath
	{
		get
		{
			// Locate tools/snippets/vscode/gram.code-snippets relative to the test assembly
			// Assembly: /ramdisk/agents/dotgram/snippets/tests/DotGram.Tests/bin/Release/net10.0/DotGram.Tests.dll
			// DirectoryName: /ramdisk/agents/dotgram/snippets/tests/DotGram.Tests/bin/Release/net10.0
			// Repo root: /ramdisk/agents/dotgram/snippets (5 levels up from DirectoryName)
			var assembly = typeof(CodeSnippetsTests).Assembly;
			var assemblyDir = new FileInfo(assembly.Location).DirectoryName!;
			var repoRoot = Directory.GetParent(assemblyDir)!  // Release
				.Parent!  // bin
				.Parent!  // DotGram.Tests
				.Parent!  // tests
				.Parent!; // snippets (repo root)
			return Path.Combine(repoRoot.FullName, "tools", "snippets", "vscode", "gram.code-snippets");
		}
	}

	/// <summary>
	/// Expands a snippet template by replacing placeholders with default values.
	/// </summary>
	private static string ExpandSnippet(string body)
	{
		// Body is an array of strings; join them
		var text = string.Join("\n", body);

		// ${1:Default} → Default
		text = System.Text.RegularExpressions.Regex.Replace(
			text,
			@"\$\{\d+:([^}]+)\}",
			m => m.Groups[1].Value);

		// ${1|a,b,c|} → a
		text = System.Text.RegularExpressions.Regex.Replace(
			text,
			@"\$\{\d+\|([^}]+)\|\}",
			m => m.Groups[1].Value.Split(',')[0]);

		// Unescape \t → actual tab, \\ → \
		text = text.Replace("\\t", "\t").Replace("\\\\", "\\");

		return text;
	}

	[Fact]
	public void Snippets_file_exists()
	{
		Assert.True(File.Exists(SnippetsPath), $"Snippets file not found at {SnippetsPath}");
	}

	[Fact]
	public void Snippets_json_is_valid()
	{
		Assert.True(File.Exists(SnippetsPath), $"Snippets file not found at {SnippetsPath}");

		var json = File.ReadAllText(SnippetsPath);
		using var doc = JsonDocument.Parse(json);

		// Just verify it's valid JSON and has content
		Assert.True(doc.RootElement.ValueKind == JsonValueKind.Object);
		Assert.True(doc.RootElement.EnumerateObject().Any());
	}

	[Fact]
	public void All_snippet_names_are_present()
	{
		var json = File.ReadAllText(SnippetsPath);
		using var doc = JsonDocument.Parse(json);

		var snippets = new List<string>();
		foreach (var property in doc.RootElement.EnumerateObject())
		{
			snippets.Add(property.Name);
		}

		// Verify expected snippets exist
		var expected = new[]
		{
			"Grammar skeleton",
			"Typed rule with factory",
			"Rule with value construction",
			"Choice rule",
			"Delimited list (first & rest)",
			"Recover for error handling",
			"Guard with when",
			"On fail message",
			"Namespace with no trivia",
			"Gram-attributed host class (inline)",
			"Gram-attributed host class (AdditionalFile)",
		};

		foreach (var name in expected)
		{
			Assert.Contains(name, snippets);
		}
	}

	[Theory]
	[MemberData(nameof(GramSnippets))]
	public void Gram_snippet_compiles(string name, string grammarText)
	{
		var result = GramCompiler.Compile(
			grammarText,
			new GramCompilerOptions
			{
				CSharpScanner = RoslynCSharpScanner.Instance,
			});

		// There should be no real errors
		var errors = result.Diagnostics
			.Where(d => d.Severity is not GramSeverity.Info && d.Severity is not GramSeverity.Warning)
			.ToArray();

		Assert.True(
			errors.Length == 0,
			$"Snippet '{name}' had errors: " + string.Join("; ", errors.Select(e => e.Message)));
	}

	/// <summary>
	/// Reads snippets from the JSON file and yields complete, compilable grammars
	/// for each .gram scope snippet.
	/// </summary>
	public static TheoryData<string, string> GramSnippets
	{
		get
		{
			var data = new TheoryData<string, string>();

			if (!File.Exists(SnippetsPath))
			{
				return data;
			}

			var json = File.ReadAllText(SnippetsPath);
			using var doc = JsonDocument.Parse(json);

			foreach (var property in doc.RootElement.EnumerateObject())
			{
				var name = property.Name;
				var snippet = property.Value;

				if (!snippet.TryGetProperty("scope", out var scope))
					continue;

				var scopeText = scope.GetString() ?? "";

				// Only test .gram scope snippets
				if (!scopeText.Contains("gram") || scopeText.Contains("csharp"))
					continue;

				if (!snippet.TryGetProperty("body", out var body))
					continue;

				// Collect and expand the body
				var lines = new List<string>();
				foreach (var element in body.EnumerateArray())
				{
					if (element.ValueKind == JsonValueKind.String)
					{
						lines.Add(element.GetString() ?? "");
					}
				}

				var expanded = ExpandSnippet(string.Join("\n", lines));

				// Wrap snippets as needed to create complete, compilable grammars
				var grammarText = BuildCompilableGrammar(name, expanded);

				data.Add(name, grammarText);
			}

			return data;
		}
	}

	/// <summary>
	/// Takes a snippet (which may be just a rule pattern) and wraps it into
	/// a complete, compilable grammar.
	/// </summary>
	private static string BuildCompilableGrammar(string snippetName, string expanded)
	{
		// Snippets that are already complete rule patterns
		if (snippetName.Equals("Grammar skeleton", StringComparison.Ordinal))
		{
			// Already complete
			return expanded;
		}

		// Snippets that are just single-rule definitions - wrap them
		if (snippetName.Equals("Typed rule with factory", StringComparison.Ordinal))
		{
			return $"""
			@using System;

			trivia = Std.Spacing?

			SomeRule = "test"

			Factory(v) = v

			{expanded}

			parse Rule
			""";
		}

		if (snippetName.Equals("Rule with value construction", StringComparison.Ordinal))
		{
			return $"""
			@using System;

			trivia = Std.Spacing?

			Operand = ['0'..'9']+
			Operator = '+' | '-'

			{expanded}

			parse Rule
			""";
		}

		if (snippetName.Equals("Choice rule", StringComparison.Ordinal))
		{
			return $"""
			@using System;

			trivia = Std.Spacing?

			OptionA = "a"
			OptionB = "b"
			OptionC = "c"

			{expanded}

			parse Rule
			""";
		}

		if (snippetName.Equals("Delimited list (first & rest)", StringComparison.Ordinal))
		{
			return $"""
			@using System;

			trivia = Std.Spacing?

			Item = ['a'..'z']+

			MakeList(f, r) = r

			{expanded}

			parse Items
			""";
		}

		if (snippetName.Equals("Guard with when", StringComparison.Ordinal))
		{
			return $"""
			@using System;

			trivia = Std.Spacing?

			Capture = ['0'..'9']+

			{expanded}

			parse Rule
			""";
		}

		if (snippetName.Equals("On fail message", StringComparison.Ordinal))
		{
			return $"""
			@using System;

			trivia = Std.Spacing?

			pattern = "test"

			{expanded}

			parse Rule
			""";
		}

		if (snippetName.Equals("Namespace with no trivia", StringComparison.Ordinal))
		{
			return $"""
			@using System;

			{expanded}
			""";
		}

		if (snippetName.Equals("Recover for error handling", StringComparison.Ordinal))
		{
			return $"""
			@using System;

			trivia = Std.Spacing?

			Row = ['0'..'9']+
			eol = "eol"

			{expanded}

			parse Rows
			""";
		}

		// Fallback: assume it's a pattern and add a simple parse
		return $"""
		@using System;

		trivia = Std.Spacing?

		{expanded}

		parse Item
		""";
	}
}
