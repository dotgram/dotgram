using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Xunit;

namespace DotGram.Sql.Tests;

/// <summary>
/// The package has one SQL tree, <c>DotGram.Sql.Ast</c>, and one writer of it by its plain name: the T-SQL tree that
/// came before it, and the writer's name from when the two stood side by side, are gone and stay gone.
/// </summary>
public sealed class OneTreeTests
{
	/// <summary>The roots of the old tree, and the helpers its grammar built it with, are not public types any more.</summary>
	[Theory]
	[InlineData("Statement")]
	[InlineData("SetExpression")]
	[InlineData("Query")]
	[InlineData("Expression")]
	[InlineData("TableReference")]
	[InlineData("Clause")]
	[InlineData("Syntax")]
	public void No_root_of_the_old_tree_is_public(string name)
	{
		var type = typeof(ISqlSpan).Assembly.GetType("DotGram.Sql." + name);

		Assert.True(type is null || !type.IsVisible, $"DotGram.Sql.{name} is public");
	}

	/// <summary>
	/// No source of the package or its tests names the writer by the name it had beside the old tree's writer. The
	/// release notes and the design history keep it, as written then; code does not.
	/// </summary>
	[Fact]
	public void No_source_names_the_writer_by_its_old_name()
	{
		// Put together, so that this file does not name it either.
		var old  = new Regex(@"\bSql" + "2023" + @"Writer\b");
		var root = Root();

		var naming = new[] { "src", "tests" }
			.SelectMany(top => Directory.EnumerateFiles(Path.Combine(root, top), "*", SearchOption.AllDirectories))
			.Where(static file => file.EndsWith(".cs", StringComparison.Ordinal) || file.EndsWith(".gram", StringComparison.Ordinal))
			.Where(static file => !file.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar, StringComparison.Ordinal)
				&& !file.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar, StringComparison.Ordinal))
			.Where(file => old.IsMatch(File.ReadAllText(file)))
			.Select(file => Path.GetRelativePath(root, file))
			.ToArray();

		Assert.True(Directory.Exists(Path.Combine(root, "src", "DotGram.Sql")), $"{root} is not the repository");
		Assert.Empty(naming);
	}

	static string Root()
	{
		return SourceRoot.Directory;
	}
}
