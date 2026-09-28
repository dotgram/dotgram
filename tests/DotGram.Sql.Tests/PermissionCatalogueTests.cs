using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using DotGram.Sql.Ast;

using Xunit;

namespace DotGram.Sql.Tests;

// Inside this namespace an unqualified name is the OLD tree's; see Sql2023WriterCoverageTests.
using Identifier = DotGram.Sql.Ast.Identifier;
using QualifiedName = DotGram.Sql.Ast.QualifiedName;
using Statement = DotGram.Sql.Ast.Statement;

/// <summary>
/// That the tree's <see cref="PrivilegeKind"/> and <see cref="SecurableClass"/> are the permission
/// catalogue, and that the T-SQL grammar's two lists are the same catalogue (D148).
/// </summary>
/// <remarks>
/// <para>
/// Three things say which permissions there are, and each was written from the server at a different
/// time: <c>PermissionCatalogue</c>, whose pairs the parser's guard reads; the grammar's
/// <c>PermissionName</c> and <c>SecurableClassName</c>, which decide what is read at all; and the two
/// enums, which are what a consumer holds. <c>permissions.py</c> writes the first and the third from one
/// answer, and nothing writes the second. So this holds all three to one another: a permission the
/// grammar reads and the enum lacks would be a statement read and then not built, and one the enum has and
/// the grammar refuses would be a member no text produces.
/// </para>
/// <para>
/// The enums are asked through the public writer, not the internal table beside them, because what a
/// member is written as is the claim, and the writer is where it is made. The catalogue is internal and
/// is read by reflection, which is the price of not opening the package's internals to this project.
/// </para>
/// </remarks>
public sealed class PermissionCatalogueTests
{
	/// <summary>
	/// The standard's actions that no row of the catalogue has, each for its own reason.
	/// </summary>
	/// <remarks>
	/// <c>ALL PRIVILEGES</c> is every class's and so no class's row, as <c>ALL</c> and <c>EXEC</c> are;
	/// <c>USAGE</c>, <c>TRIGGER</c> and <c>UNDER</c> are the standard's and SQL Server has none of them.
	/// </remarks>
	static readonly string[] StandardOnly = ["ALL PRIVILEGES", "TRIGGER", "UNDER", "USAGE"];

	/// <summary>
	/// What the grammar reads besides the catalogue: every class's two, and EXECUTE's other spelling.
	/// </summary>
	static readonly string[] ReadOfEveryClass = ["ALL", "ALL PRIVILEGES", "EXEC"];

	[Fact]
	public void Every_permission_of_the_catalogue_is_exactly_one_member()
	{
		var written = Enum.GetValues<PrivilegeKind>().ToLookup(Words, StringComparer.Ordinal);
		var missing = Permissions().Where(one => written[one].Count() != 1).ToList();

		Assert.Empty(missing);
	}

	[Fact]
	public void Every_member_is_a_permission_of_the_catalogue_or_the_standard()
	{
		var known = Permissions().Concat(StandardOnly).ToHashSet(StringComparer.Ordinal);
		var extra = Enum.GetValues<PrivilegeKind>().Select(Words).Where(one => !known.Contains(one)).ToList();

		Assert.Empty(extra);
	}

	[Fact]
	public void Every_class_of_the_catalogue_is_one_member_and_there_are_no_others()
	{
		Assert.Equal(
			Classes().Order(StringComparer.Ordinal),
			Enum.GetValues<SecurableClass>().Select(Words).Order(StringComparer.Ordinal));
	}

	/// <summary>
	/// That a member's name is its words run together, so that nothing about a name was chosen by hand and
	/// the words can be had back from it — which is what keeps a later move to plain strings mechanical.
	/// </summary>
	[Fact]
	public void A_member_is_named_for_its_words()
	{
		var misnamed = Enum.GetValues<PrivilegeKind>().Select(one => (Name: one.ToString(), Words: Words(one)))
			.Concat(Enum.GetValues<SecurableClass>().Select(one => (Name: one.ToString(), Words: Words(one))))
			.Where(one => one.Name != Run(one.Words))
			.Select(one => one.Name + " is written " + one.Words)
			.ToList();

		Assert.Empty(misnamed);
	}

	[Fact]
	public void The_grammar_reads_the_catalogue_permissions_and_no_others()
	{
		Assert.Equal(
			Permissions().Concat(ReadOfEveryClass).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal),
			Alternatives("PermissionName").Order(StringComparer.Ordinal));
	}

	[Fact]
	public void The_grammar_reads_the_catalogue_classes_and_no_others()
	{
		Assert.Equal(
			Classes().Order(StringComparer.Ordinal),
			Alternatives("SecurableClassName").Order(StringComparer.Ordinal));
	}

	/// <summary>
	/// What the writer prints for a permission, alone in a GRANT.
	/// </summary>
	static string Words(PrivilegeKind kind)
	{
		var granted = Sql2023Writer.Write(new Statement.Grant
		{
			Body = new GrantBody.Privileges([new Privilege(kind, [], [])], null, [new Grantee.Public()], false, false, null),
		});

		return Regex.Match(granted, "^GRANT (.+) TO PUBLIC").Groups[1].Value;
	}

	/// <summary>
	/// What the writer prints for a class, before the <c>::</c>.
	/// </summary>
	static string Words(SecurableClass kind)
	{
		var on      = new PrivilegeObject(PrivilegeObjectKind.Table, new QualifiedName([new Identifier("s")]), Class: kind);
		var granted = Sql2023Writer.Write(new Statement.Grant
		{
			Body = new GrantBody.Privileges(
				[new Privilege(PrivilegeKind.Control, [], [])], on, [new Grantee.Public()], false, false, null),
		});

		return Regex.Match(granted, " ON (.+)::s TO ").Groups[1].Value;
	}

	static string Run(string words)
	{
		return string.Concat(words.Split(' ').Select(one => one[..1] + one[1..].ToLowerInvariant()));
	}

	/// <summary>
	/// The catalogue's pairs, <c>CLASS|PERMISSION</c>, as the compiled package holds them.
	/// </summary>
	static IReadOnlyList<string[]> Pairs()
	{
		var catalogue = typeof(Sql2023Writer).Assembly.GetType("DotGram.Sql.TransactSql.PermissionCatalogue", throwOnError: true)!;
		var pairs     = (IEnumerable<string>)catalogue.GetField("Pairs", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;

		return pairs.Select(one => one.Split('|')).ToList();
	}

	static IEnumerable<string> Permissions()
	{
		return Pairs().Select(one => one[1]).Distinct(StringComparer.Ordinal);
	}

	static IEnumerable<string> Classes()
	{
		return Pairs().Select(one => one[0]).Distinct(StringComparer.Ordinal);
	}

	/// <summary>
	/// Every run of words a rule of <c>TransactSql.gram</c> reads, where the rule is alternatives of
	/// literal words and nothing else: an optional word gives both runs.
	/// </summary>
	static List<string> Alternatives(string rule)
	{
		var lines = File.ReadAllLines(Path.Combine(Repository(), "src", "DotGram.Sql", "TransactSql", "TransactSql.gram"))
			.SkipWhile(one => one != rule)
			.Skip(1)
			.TakeWhile(one => one.Trim().Length > 0)
			.ToList();

		Assert.NotEmpty(lines);

		var read = new List<string>();

		foreach (var line in lines)
		{
			var words = Regex.Matches(line, "\"([A-Za-z_]+)\"i(\\?)?");

			// A line that is anything but words and `&` would be a rule this reading cannot expand, and
			// skipping it would make the comparison below a comparison with part of the rule.
			Assert.True(
				Regex.Replace(line, "\"([A-Za-z_]+)\"i(\\?)?|[=|&]", "").Trim().Length == 0,
				rule + " has an alternative this test does not read: " + line.Trim());

			var runs = new List<string> { "" };

			foreach (Match word in words)
			{
				var longer = runs.Select(one => (one + " " + word.Groups[1].Value.ToUpperInvariant()).TrimStart()).ToList();

				runs = word.Groups[2].Success ? [.. runs, .. longer] : longer;
			}

			read.AddRange(runs);
		}

		return read;
	}

	static string Repository([System.Runtime.CompilerServices.CallerFilePath] string here = "")
	{
		return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", ".."));
	}
}
