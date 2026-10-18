using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.TransactSql;

// The statements whose rules build the common tree themselves. Such a rule hands what it read to a builder of its
// family, which makes the statement of the common tree from it, its parts that are still the T-SQL tree's carried
// across as the bridge carries them; the statement stands where the T-SQL tree holds statements as a Statement.Built,
// which the bridge carries as itself. A reading is carried with the text it is read from, set before it starts
// (Over), so that what a builder reads from the text it can read while the statement is read.
static partial class Bridge
{
	/// <summary>
	/// What a rule hands a builder in place of the T-SQL tree's record: the record's name, under which the gaps of the
	/// statement are counted and said, and where it was written.
	/// </summary>
	internal abstract record Given(string Record) : ISqlSpan
	{
		/// <summary>Where the statement was written, where the rule said so.</summary>
		public SqlSpan Span { get; init; }

		/// <summary>Nothing locates it: it is no node of any tree.</summary>
		void ISqlSpan.Locate(int at, int length)
		{
		}
	}

	/// <summary>The span a rule's own extent says, as the T-SQL tree's records are given it.</summary>
	internal static SqlSpan At(TransactSqlParser.SourceSpan span)
	{
		return new SqlSpan(span.Start, span.Length);
	}

	/// <summary>
	/// A statement its rule builds on the common tree, held where the T-SQL tree holds statements: what the builder
	/// made, or the gap it found, which carrying the statement throws as the bridge would have.
	/// </summary>
	/// <remarks>
	/// What building it read from the text is taken off this reading's count and kept with the statement, which puts it
	/// back when it is carried: a statement read and then given up leaves nothing counted.
	/// </remarks>
	static Statement Hold(Given given, Func<Ast.Statement> build)
	{
		var mark = recovered?.Count ?? 0;

		try
		{
			var made = build();

			return new Statement.Built(made, null, Taken(mark), given.Record);
		}
		catch (BridgeGap gap)
		{
			return new Statement.Built(null, gap, Taken(mark), given.Record);
		}
	}

	static string[]? Taken(int mark)
	{
		if (recovered is not { } list || list.Count <= mark)
			return null;

		var taken = list.GetRange(mark, list.Count - mark).ToArray();

		list.RemoveRange(mark, list.Count - mark);

		return taken;
	}

	/// <summary>
	/// A reading carried with its text: the text the rules' builders read from, and whether the nodes they carry take
	/// their spans, for as long as the reading lasts.
	/// </summary>
	internal static Carriage Over(string text, bool locate)
	{
		var outer = Carriage.Here();

		source    = text;
		recovered = [];
		locating  = locate;

		return outer;
	}

	/// <summary>The statements whose rules built them on the common tree: each carried as what it holds.</summary>
	internal static class Built
	{
		public static readonly string[] Owns = ["Statement.Built"];

		public static bool TryConvert(object old, [NotNullWhen(true)] out Ast.ISqlNode? made)
		{
			made = null;

			if (old is not Statement.Built built)
				return false;

			if (built.Recovered is { } read)
				recovered?.AddRange(read);

			if (built.Gap is { } gap)
				throw gap;

			made = built.Made!;

			return true;
		}
	}
}
