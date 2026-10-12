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
	/// <summary>
	/// What became of a statement. <see cref="Refused"/> is the writer saying no — <c>NotSupportedException</c>,
	/// an argument or an operation it rejects; <see cref="Crashed"/> is anything else it threw.
	/// </summary>
	public enum Status { Converted, Gap, Refused, Crashed, Unread, Different }

	/// <summary>
	/// What became of one statement.
	/// </summary>
	/// <param name="Root">The old statement's record, as <c>Statement.Select</c>.</param>
	/// <param name="Counted">The record the status is counted under: the gap's, or else the root's.</param>
	/// <param name="ByCase">
	/// Converted, and equal to the original only with the case of key words aside: the members that needed it,
	/// one entry each time (<see cref="Trees.FreeText"/>).
	/// </param>
	/// <param name="Recovered">The members the bridge read from the text (<see cref="Bridge.Recoverable"/>).</param>
	/// <param name="Gaps">Every gap of the statement's own nodes, by record and reason, the first among them.</param>
	/// <param name="Standard">Whether <c>SqlStandardParser</c> read the statement's text, so that the shape was asked.</param>
	/// <param name="Shape">Read by the standard's parser too, and the bridge built another tree than it.</param>
	/// <param name="Dialects">
	/// Read by the standard's parser too, as another tree only where the two dialects give the text different
	/// meanings (<see cref="Meaning"/>), one entry for each exempted subtree: not a shape mismatch, and listed
	/// apart.
	/// </param>
	public sealed record Outcome(
		string Root, string Counted, Status Status, string Text,
		string? Reason = null, string? Written = null, string? Detail = null,
		IReadOnlyList<string>? ByCase = null, IReadOnlyList<string>? Recovered = null,
		IReadOnlyList<(string Record, string Reason)>? Gaps = null,
		bool Standard = false, bool Shape = false, IReadOnlyList<string>? Dialects = null)
	{
		public bool Dialect => Dialects is { Count: > 0 };
	}

	/// <summary>
	/// One statement put to the bridge, the writer and both parsers.
	/// </summary>
	public static Outcome Run(Statement old, string source)
	{
		var root = Bridge.NameOf(old);
		var text = Text(old, source);
		var gaps = Census(old, source);

		Ast.Statement made;

		try
		{
			made = Bridge.ToAst(old, source);
		}
		catch (BridgeGap gap)
		{
			return new Outcome(root, gap.OldType, Status.Gap, text, gap.Reason, Gaps: gaps);
		}

		var recovered = Bridge.Recovered.ToArray();
		var standard  = SqlStandardParser.TryParseStatement(text);
		var read      = standard.IsSuccess;
		var shape     = false;
		string? detail = null;
		List<string>? dialects = null;

		if (read && !made.Equals(standard.Value))
		{
			var differences = Trees.Differences(made, standard.Value, old: false);

			dialects = Meaning(differences, out var unexplained);
			shape    = unexplained.Count > 0;
			detail   = "shape: " + (shape ? unexplained[0] : differences[0]);
		}

		string written;

		try
		{
			written = Ast.Sql2023Writer.Write(made);
		}
		catch (Exception thrown)
		{
			var status = thrown is NotSupportedException or ArgumentException or InvalidOperationException ? Status.Refused : Status.Crashed;

			return new Outcome(root, root, status, text, Detail: $"{thrown.GetType().Name}: {thrown.Message}",
				Recovered: recovered, Gaps: gaps, Standard: read, Shape: shape, Dialects: dialects);
		}

		var back = TransactSqlParser.TryParseSql(written);

		if (!back.IsSuccess || back.Value!.Length != 1)
			return new Outcome(root, root, Status.Unread, text, Written: written, Detail: detail,
				Recovered: recovered, Gaps: gaps, Standard: read, Shape: shape, Dialects: dialects);

		var byCase = new List<string>();

		if (!back.Value[0].Equals(old) && Trees.Difference(old, back.Value[0], byCase: byCase) is { } difference)
			return new Outcome(root, root, Status.Different, text, Written: written, Detail: difference,
				Recovered: recovered, Gaps: gaps, Standard: read, Shape: shape, Dialects: dialects);

		return new Outcome(root, root, Status.Converted, text, Written: written, Detail: detail,
			ByCase: byCase, Recovered: recovered, Gaps: gaps, Standard: read, Shape: shape, Dialects: dialects);
	}

	/// <summary>
	/// Every gap a statement holds, not only the first: each node of it that is a statement, a query, a value or
	/// a table source is carried on its own, and every gap any of them throws is kept — once per statement for
	/// each record and reason, however many nodes throw it.
	/// </summary>
	static List<(string Record, string Reason)> Census(Statement old, string source)
	{
		var gaps = new List<(string, string)>();

		foreach (var node in Trees.Nodes(old))
		{
			if (node is not (Statement or Query or Expression or TableReference))
				continue;

			try
			{
				_ = node switch
				{
					Statement statement => (object)Bridge.ToAst(statement, source),
					Query query         => Bridge.ToAst(query, source),
					Expression value    => Bridge.ToAst(value, source),
					_                   => Bridge.ToAst((TableReference)node, source),
				};
			}
			catch (BridgeGap gap)
			{
				if (!gaps.Contains((gap.OldType, gap.Reason)))
					gaps.Add((gap.OldType, gap.Reason));
			}
		}

		return gaps;
	}

	/// <summary>
	/// The four constructs the two dialects read with different meanings. Each exempts one subtree of the
	/// bridge's tree, found by its construct and never by the statement's text:
	/// <list type="bullet">
	/// <item><c>TOP (n) a</c> is a row count in T-SQL, and the standard reads a call to <c>TOP</c> aliased <c>a</c>:
	/// a <c>Query.Select</c>'s <c>Top</c> and its first column.</item>
	/// <item><c>SET t.c = …</c> is a qualified column in T-SQL, and a mutated set clause on <c>t</c> in the
	/// standard: an <c>AssignmentTarget</c> whose name has more than one part.</item>
	/// <item><c>GLOBAL c</c> names a cursor of the connection in T-SQL, and is an extended name in the standard: a
	/// <c>CursorReference</c> that is <c>Global</c>.</item>
	/// <item><c>FREETEXT ((c), …)</c> and <c>CONTAINS ((c), …)</c> take a column list in T-SQL, and a value in
	/// brackets in the standard: the first argument of an invocation of either.</item>
	/// </list>
	/// A difference outside every exempted subtree is unexplained, and makes the statement a shape mismatch.
	/// </summary>
	public static List<string> Meaning(List<Trees.Diff> differences, out List<Trees.Diff> unexplained)
	{
		var exempted = new List<string>();

		unexplained = [];

		foreach (var difference in differences)
		{
			if (Exempting(difference) is { } pattern)
				exempted.Add(pattern);
			else
				unexplained.Add(difference);
		}

		return exempted;
	}

	public const string Top      = "TOP (n) a: a row count in T-SQL, a call to TOP in the standard";
	public const string SetTc    = "SET t.c: a qualified column in T-SQL, a mutated set clause in the standard";
	public const string Global   = "GLOBAL c: a cursor of the connection in T-SQL, an extended name in the standard";
	public const string FullText = "FREETEXT ((c), ...): a column list in T-SQL, a value in brackets in the standard";

	public static readonly string[] Patterns = [Top, SetTc, Global, FullText];

	/// <summary>
	/// The pattern whose subtree a difference lies in, or null.
	/// </summary>
	static string? Exempting(Trees.Diff difference)
	{
		foreach (var step in difference.Path)
		{
			switch (step.Node)
			{
				case Ast.Query.Select { Top: not null } when step.Member == "Top" || step.Member == "Columns" && step.Index is 0 or null:
					return Top;

				case Ast.AssignmentTarget { Name.Parts.Count: > 1 }:
					return SetTc;

				case Ast.CursorReference { Global: true }:
					return Global;

				case Ast.Expression.Invocation { Name.Parts: [var name] } when step.Member == "Arguments" && step.Index == 0
					&& (name.Text.Equals("FREETEXT", StringComparison.OrdinalIgnoreCase) || name.Text.Equals("CONTAINS", StringComparison.OrdinalIgnoreCase)):
					return FullText;
			}
		}

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
