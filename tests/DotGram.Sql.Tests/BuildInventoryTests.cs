using System;
using System.Linq;

using DotGram.Sql.TransactSql;

using Xunit;

namespace DotGram.Sql.Tests;

/// <summary>
/// The records a builder owns, written out in the builder rather than found by reflection there (so that a trimmed
/// or Native AOT application keeps nothing for it), held here to the records themselves: a record added to a family
/// that the list does not name fails this test, not a statement in somebody's application.
/// </summary>
public sealed class BuildInventoryTests
{
	[Fact]
	public void Tables_own_every_table_reference()
	{
		var records = typeof(Build.TableReference).GetNestedTypes()
			.Where(static one => one.IsSubclassOf(typeof(Build.TableReference)) && one != typeof(Build.TableReference.Changed))
			.Select(Build.NameOf)
			.ToArray();

		Assert.NotEmpty(records);
		Assert.Equal(records, Build.Tables.Owns.Where(static name => name.StartsWith("TableReference.", StringComparison.Ordinal)));
	}

	[Fact]
	public void Procedural_owns_every_set_expression()
	{
		var records = typeof(Build.SetExpression).GetNestedTypes()
			.SelectMany(static group => group.GetNestedTypes())
			.Where(static one => one.IsSubclassOf(typeof(Build.SetExpression)) && !one.IsAbstract)
			.Select(Build.NameOf)
			.ToArray();

		Assert.NotEmpty(records);
		Assert.Equal(records, Build.Procedural.Owns.Where(static name => name.StartsWith("SetExpression.", StringComparison.Ordinal)));
	}
}
