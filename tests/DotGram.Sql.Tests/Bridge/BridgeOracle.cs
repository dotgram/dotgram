using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using DotGram.Sql.Standard;
using DotGram.Sql.TransactSql;

using Xunit;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.Tests;

/// <summary>
/// The round trip the bridge is held by: an old tree carried onto the new one, written by
/// <c>Sql2023Writer</c>, read back by <c>TransactSqlParser</c> and held to the tree it came from — and,
/// where the standard's parser reads the same text, the new tree held to the one that parser builds.
/// </summary>
static class BridgeOracle
{
	public enum Status { Converted, Gap, Refused, Unread, Different }

	/// <summary>
	/// What became of one statement.
	/// </summary>
	/// <param name="Root">The old statement's record, as <c>Statement.Select</c>.</param>
	/// <param name="Counted">The record the status is counted under: the gap's, or else the root's.</param>
	/// <param name="ByCase">Converted, and equal to the original only with the case of its key words aside.</param>
	/// <param name="Standard">Whether <c>SqlStandardParser</c> read the statement's text, so that the shape was asked.</param>
	/// <param name="Shape">Read by the standard's parser too, and the bridge built another tree than it.</param>
	/// <param name="Dialect">
	/// Read by the standard's parser too, as another tree because the two dialects give the text different
	/// meanings (<see cref="Meaning"/>): not a shape mismatch, and listed apart.
	/// </param>
	public sealed record Outcome(
		string Root, string Counted, Status Status, string Text,
		string? Reason = null, string? Written = null, string? Detail = null,
		bool ByCase = false, bool Standard = false, bool Shape = false, string? Dialect = null);

	/// <summary>
	/// One statement put to the bridge, the writer and both parsers.
	/// </summary>
	public static Outcome Run(Statement old, string source)
	{
		var root = Bridge.NameOf(old);
		var text = Text(old, source);

		Ast.Statement made;

		try
		{
			made = Bridge.ToAst(old, source);
		}
		catch (BridgeGap gap)
		{
			return new Outcome(root, gap.OldType, Status.Gap, text, gap.Reason);
		}

		var standard = SqlStandardParser.TryParseStatement(text);
		var read     = standard.IsSuccess;
		var shape    = read && !made.Equals(standard.Value);
		var detail   = shape ? "shape: " + Trees.Difference(made, standard.Value, old: false) : null;
		var dialect  = shape ? Meaning(detail!, text) : null;

		if (dialect is not null)
			shape = false;

		string written;

		try
		{
			written = Ast.Sql2023Writer.Write(made);
		}
		catch (Exception refused)
		{
			return new Outcome(root, root, Status.Refused, text, Detail: $"{refused.GetType().Name}: {refused.Message}", Standard: read, Shape: shape, Dialect: dialect);
		}

		var back = TransactSqlParser.TryParseSql(written);

		if (!back.IsSuccess || back.Value!.Length != 1)
			return new Outcome(root, root, Status.Unread, text, Written: written, Detail: detail, Standard: read, Shape: shape, Dialect: dialect);

		if (back.Value[0].Equals(old))
			return new Outcome(root, root, Status.Converted, text, Written: written, Detail: detail, Standard: read, Shape: shape, Dialect: dialect);

		if (Trees.Difference(old, back.Value[0]) is { } difference)
			return new Outcome(root, root, Status.Different, text, Written: written, Detail: difference, Standard: read, Shape: shape, Dialect: dialect);

		return new Outcome(root, root, Status.Converted, text, Written: written, Detail: detail, ByCase: true, Standard: read, Shape: shape, Dialect: dialect);
	}

	/// <summary>
	/// Where a statement reads in both dialects as two trees because the two give its text different meanings,
	/// which: the bridge builds T-SQL's and the standard's parser the standard's, and neither is wrong.
	/// </summary>
	/// <remarks>
	/// Each is recognized by where the trees first differ, and is a reading of the text, not of the bridge:
	/// <list type="bullet">
	/// <item>T-SQL's <c>TOP (n) a</c> is a row count, and the standard reads a call to <c>TOP</c> with the alias <c>a</c>.</item>
	/// <item>T-SQL's <c>SET t.c = …</c> is a column qualified by its table, and the standard's is a mutated set clause on <c>t</c>.</item>
	/// <item>T-SQL's <c>OPEN GLOBAL c</c> names a cursor of the connection, and the standard's <c>GLOBAL c</c> is an extended name.</item>
	/// <item>T-SQL's <c>FREETEXT ((c), …)</c> and <c>CONTAINS ((c), …)</c> take a column list, and the standard reads a value in brackets.</item>
	/// </list>
	/// </remarks>
	public static string? Meaning(string difference, string text)
	{
		if (difference.EndsWith("/Query.Select.Top: TopClause against null", StringComparison.Ordinal))
			return "TOP (n) is a row count in T-SQL and a call in the standard";

		if (difference.Contains("/AssignmentTarget.Name/QualifiedName.Parts:", StringComparison.Ordinal) && difference.EndsWith(" against 1", StringComparison.Ordinal))
			return "SET t.c is a qualified column in T-SQL and a mutated set clause in the standard";

		if (difference.EndsWith("/CursorReference.Name: QualifiedName against null", StringComparison.Ordinal))
			return "GLOBAL c names a cursor in T-SQL and an extended name in the standard";

		if (difference.EndsWith("Argument.Value: Expression.Row against Expression.Parenthesized", StringComparison.Ordinal)
			&& (text.Contains("FREETEXT", StringComparison.OrdinalIgnoreCase) || text.Contains("CONTAINS", StringComparison.OrdinalIgnoreCase)))
			return "a full-text predicate's (c) is a column list in T-SQL and a value in brackets in the standard";

		return null;
	}

	/// <summary>
	/// The statement's own text, cut from the batch by its span.
	/// </summary>
	static string Text(Statement old, string source)
	{
		var span = old.Span;

		return span.Known && span.End <= source.Length ? source.Substring(span.At, span.Length) : source;
	}

	// ── The statements put to it ────────────────────────────────────────────────

	public static string Corpus([System.Runtime.CompilerServices.CallerFilePath] string here = "")
	{
		return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "..", "Corpus", "ScriptDom"));
	}

	/// <summary>
	/// Every statement of the ScriptDom corpus, enumerated as <c>TreeEqualityTests</c> does: each file read
	/// as a script, each batch read whole, each statement of a batch that reads. Located, so that the bridge
	/// can read what the old tree does not keep.
	/// </summary>
	public static IEnumerable<(Statement Statement, string Source)> CorpusStatements()
	{
		foreach (var file in Directory.GetFiles(Corpus(), "*.sql", SearchOption.AllDirectories).OrderBy(static one => one, StringComparer.Ordinal))
			foreach (var batch in SqlScript.Read(File.ReadAllText(file)).Batches)
			{
				var read = TransactSqlParser.Located.TryParseSql(batch);

				if (!read.IsSuccess)
					continue;

				foreach (var statement in read.Value!)
					yield return (statement, batch.Text);
			}
	}

	/// <summary>
	/// The statements <c>TreeEqualityTests</c> counts, read the way it reads them: not located.
	/// </summary>
	public static int CorpusCount()
	{
		var count = 0;

		foreach (var file in Directory.GetFiles(Corpus(), "*.sql", SearchOption.AllDirectories))
			foreach (var batch in SqlScript.Read(File.ReadAllText(file)).Batches)
			{
				var read = TransactSqlParser.TryParseSql(batch);

				if (read.IsSuccess)
					count += read.Value!.Length;
			}

		return count;
	}

	/// <summary>
	/// Every input <c>TransactSqlTests</c> expects to read, gathered by reflection over the
	/// <c>[InlineData]</c> of its theories: a theory whose name says it refuses is left out, and so is a
	/// row whose <c>bool</c> says it is not read. Of what is left, what reads as statements is kept — a
	/// value-level fragment is read by a theory through another door and is not a statement here.
	/// </summary>
	public static IEnumerable<(Statement Statement, string Source)> TestStatements()
	{
		var seen = new HashSet<string>(StringComparer.Ordinal);

		foreach (var method in typeof(TransactSqlTests).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).OrderBy(static one => one.Name, StringComparer.Ordinal))
		{
			if (method.Name.Contains("refuse", StringComparison.OrdinalIgnoreCase))
				continue;

			foreach (var row in method.GetCustomAttributes<InlineDataAttribute>())
			{
				if (row.Data is not [string input, ..] data || data.Skip(1).Any(static one => one is false))
					continue;

				if (!seen.Add(input))
					continue;

				var read = TransactSqlParser.Located.TryParseSql(input);

				if (!read.IsSuccess)
					continue;

				foreach (var statement in read.Value!)
					yield return (statement, input);
			}
		}
	}
}
