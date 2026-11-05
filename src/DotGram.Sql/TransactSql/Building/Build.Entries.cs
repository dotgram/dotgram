using System;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.TransactSql;

// How TransactSqlParser's entries hand back the common tree: each public entry reads through the internal one of the
// same rule, with locations, and what the rules built is taken out of the records that hold it here. A construct the
// common tree has no place for yet comes back as the family's Extension node, naming the record and why, rather than
// being thrown out of a TryParse.
static partial class Build
{
	/// <summary>
	/// A statement an entry read, on the common tree; one with something the tree has no place for, a
	/// <see cref="Ast.Statement.Extension"/> that says which and why.
	/// </summary>
	/// <param name="read">What the entry read, told where it was written.</param>
	/// <param name="locate">Whether the extension takes the statement's span: the located door's tree, and not the plain one's.</param>
	internal static Ast.Statement Carried(Statement read, bool locate)
	{
		try
		{
			return StatementOf(read);
		}
		catch (BuildGap gap)
		{
			return Unplaced(read, gap, locate);
		}
	}

	/// <summary>An expression an entry read, on the common tree, or an <see cref="Ast.Expression.Extension"/>.</summary>
	internal static Ast.Expression Carried(Ast.Expression read, bool locate)
	{
		try
		{
			return Values.Of(read);
		}
		catch (BuildGap gap)
		{
			var made = new Ast.Expression.Extension(Dialect, gap.Message, []);

			if (locate && read.Read is { Known: true } span)
				made.Locate(span.At, span.Length);

			return made;
		}
	}

	/// <summary>The statements an entry read, on the common tree, each with something the tree has no place for an extension.</summary>
	internal static Ast.Statement[] Carried(Statement[] read, bool locate)
	{
		var made = new Ast.Statement[read.Length];

		for (var i = 0; i < read.Length; i++)
			made[i] = Carried(read[i], locate);

		return made;
	}

	/// <summary>What an extension of the common tree carried across from T-SQL says it is.</summary>
	const string Dialect = "T-SQL";

	/// <summary>
	/// The extension a statement with no place on the common tree stands as: located where the grammar located the
	/// statement, in the located door's tree, as what it replaces would have been.
	/// </summary>
	static Ast.Statement Unplaced(Statement read, BuildGap gap, bool locate)
	{
		return Placed(new Ast.Statement.Extension { Dialect = Dialect, Kind = gap.Message }, read, locate);
	}

	/// <summary>A node made in place of a record, given the record's span where the reading publishes it.</summary>
	static T Placed<T>(T made, ISqlSpan read, bool locate) where T : class
	{
		if (locate && read.Span is { Known: true, IsStale: false } span)
			Place(made, span);

		return made;
	}

	// ── Spans ───────────────────────────────────────────────────────────────────

	/// <summary>
	/// A new node told the span of the old node it was made from: the range the reading offered the old one, without
	/// the gap in front of it, which the common tree does not keep. A value type keeps nothing: its copy is not the
	/// node the caller holds.
	/// </summary>
	static void Place(object made, SqlSpan span)
	{
		if (made is ISqlSpan located && !made.GetType().IsValueType)
			located.Locate(span.At, span.Length);
	}
}
