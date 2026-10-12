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

		// The words the bridge read from the text are not in the old tree, so the old trees agreeing says nothing of
		// them: the written text, read and carried again, has to give the same new tree.
		if (Again(made, written) is { } lost)
			return new Outcome(root, root, Status.Different, text, Written: written, Detail: "new tree: " + lost,
				Recovered: recovered, Gaps: gaps, Standard: read, Shape: shape, Dialects: dialects);

		return new Outcome(root, root, Status.Converted, text, Written: written, Detail: detail,
			ByCase: byCase, Recovered: recovered, Gaps: gaps, Standard: read, Shape: shape, Dialects: dialects);
	}

	/// <summary>
	/// Where the written text, read back located and carried with that text as its source, builds another new tree
	/// than the one written: the first difference, or null where they are equal, spans aside.
	/// </summary>
	public static string? Again(Ast.Statement made, string written)
	{
		var back = TransactSqlParser.Located.TryParseSql(written);

		if (!back.IsSuccess || back.Value!.Length != 1)
			return "the written text does not read located";

		Ast.Statement again;

		try
		{
			again = Bridge.ToAst(back.Value[0], written);
		}
		catch (BridgeGap gap)
		{
			return "the written text is a gap: " + gap.Reason;
		}

		return again.Equals(made) ? null : Trees.Difference(made, again, old: false) ?? "unequal, with no difference found";
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
	/// The nine constructs the two dialects read with different meanings. Each exempts one subtree of the
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
	/// <item><c>DATEADD (day, …)</c> takes a date part in T-SQL, and a column in the standard: the first argument of
	/// a date part function, against a column of one part whose text is the part's word.</item>
	/// <item><c>CAST (a AS money)</c> names one of T-SQL's system types, and a user-defined type in the standard: a
	/// system type with no arguments — a <c>Named</c> on the list, a standard record written as a name, an
	/// <c>nvarchar</c> with no length — against a user-defined type of the same name, or for <c>timestamp</c> the
	/// standard's own.</item>
	/// <item><c>GROUP BY ALL a</c> keeps the empty groups in T-SQL, and is the quantifier that changes nothing in
	/// the standard: the <c>Quantifier</c> and <c>AllKeyword</c> of a <c>GroupByClause</c> that is <c>AllKeyword</c>.</item>
	/// <item><c>SELECT a INTO t</c> makes a table in T-SQL, and assigns to a target in the standard: a
	/// <c>Query.Select</c>'s <c>IntoTable</c> with no filegroup, and its <c>Into</c> where the standard's one
	/// target names the same table.</item>
	/// <item><c>FROM t OPTION (h)</c> is a hint in T-SQL, and an alias with its columns in the standard: a
	/// statement's <c>Hints</c>, and an <c>Alias</c> the standard reads the word OPTION as.</item>
	/// </list>
	/// A difference outside every exempted subtree is unexplained, and makes the statement a shape mismatch.
	/// </summary>
	public static List<string> Meaning(List<Trees.Diff> differences, out List<Trees.Diff> unexplained)
	{
		var exempted = new List<string>();
		var subtrees = new List<object>();

		unexplained = [];

		foreach (var difference in differences)
		{
			if (Exempting(difference) is not { } found)
				unexplained.Add(difference);
			// One entry for each subtree exempted, however many differences lie in it.
			else if (!subtrees.Any(one => ReferenceEquals(one, found.Subtree)))
			{
				subtrees.Add(found.Subtree);
				exempted.Add(found.Pattern);
			}
		}

		return exempted;
	}

	public const string Top      = "TOP (n) a: a row count in T-SQL, a call to TOP in the standard";
	public const string SetTc    = "SET t.c: a qualified column in T-SQL, a mutated set clause in the standard";
	public const string Global   = "GLOBAL c: a cursor of the connection in T-SQL, an extended name in the standard";
	public const string FullText = "FREETEXT ((c), ...): a column list in T-SQL, a value in brackets in the standard";
	public const string DatePart = "DATEADD (day, ...): a date part in T-SQL, a column in the standard";
	public const string TypeName = "CAST (a AS money): a type of T-SQL's own in T-SQL, a user-defined type in the standard";

	public const string GroupAll = "GROUP BY ALL a: the empty groups kept in T-SQL, a quantifier in the standard";
	public const string Into     = "SELECT a INTO t: a new table in T-SQL, a target in the standard";
	public const string Option   = "FROM t OPTION (h): a hint in T-SQL, an alias in the standard";

	public static readonly string[] Patterns = [Top, SetTc, Global, FullText, DatePart, TypeName, GroupAll, Into, Option];

	/// <summary>
	/// The pattern whose subtree a difference lies in, and the node at the subtree's root, or null.
	/// </summary>
	static (string Pattern, object Subtree)? Exempting(Trees.Diff difference)
	{
		// Two patterns are of one node each, which differs from the standard's in a known way and in no other.
		if (TypeNamed(difference) || DateParted(difference))
			return (TypeNamed(difference) ? TypeName : DatePart, difference.Left!);

		// OPTION: the statement's own hints, and the alias the standard reads the word OPTION as.
		if (difference.Path is [{ Node: Ast.Statement.Select { Hints.Count: > 0 } statement, Member: "Hints" }, ..])
			return (Option, statement);

		if (difference.Path is [{ Node: Ast.Statement.Select { Hints.Count: > 0 } }, .., { Member: "Alias" } last]
			&& difference.Right is Ast.Alias { Name.Text: var word } && word.Equals("OPTION", StringComparison.OrdinalIgnoreCase))
			return (Option, last.Node);

		// GROUP BY ALL: the quantifier the standard reads ALL as, and the flag that holds it.
		if (difference.Path is [.., { Node: Ast.GroupByClause { AllKeyword: true } group, Member: "Quantifier" or "AllKeyword" }])
			return (GroupAll, group);

		// SELECT … INTO t: the new table, and the standard's single target where it names the same table.
		if (difference.Path is [.., { Node: Ast.Query.Select { IntoTable: { FileGroup: null } table } select, Member: var member }]
			&& (member == "IntoTable" || member == "Into" && difference.Right is Ast.IntoClause { Targets: [Ast.Expression.Reference { Name: var target }] } && target.Equals(table.Table)))
			return (Into, select);

		foreach (var step in difference.Path)
		{
			switch (step.Node)
			{
				case Ast.Query.Select { Top: not null } when step.Member == "Top" || step.Member == "Columns" && step.Index == 0:
					return (Top, step.Node);

				case Ast.AssignmentTarget { Name.Parts.Count: > 1 }:
					return (SetTc, step.Node);

				case Ast.CursorReference { Global: true }:
					return (Global, step.Node);

				case Ast.Expression.Invocation { Name.Parts: [var name] } when step.Member == "Arguments" && step.Index == 0
					&& (name.Text.Equals("FREETEXT", StringComparison.OrdinalIgnoreCase) || name.Text.Equals("CONTAINS", StringComparison.OrdinalIgnoreCase)):
					return (FullText, step.Node);

			}
		}

		return null;
	}

	/// <summary>
	/// A T-SQL system type with no arguments where the standard reads a user-defined type of the same name: a
	/// <c>Named</c> of one part, or <c>sys</c> and a name, on the list of system types; a standard record written as
	/// a name (<c>WrittenName</c>) with no length, precision or schema; an <c>nvarchar</c> with no length; a bare
	/// <c>xml</c> with no schema, never a <c>Named</c> one, which is never built. And T-SQL's <c>timestamp</c>, where the
	/// standard reads its own TIMESTAMP.
	/// </summary>
	static bool TypeNamed(Trees.Diff difference)
	{
		// The standard's user-defined type has no arguments, and neither has a name it is held against.
		static bool Bare(Ast.DataType type)
		{
			return type switch
			{
				Ast.DataType.Character one => one is { Length: null, Max: false },
				Ast.DataType.Binary one    => one is { Length: null, Max: false },
				Ast.DataType.Numeric one   => one is { Precision: null, Scale: null },
				Ast.DataType.DateTime one  => one is { Precision: null },
				Ast.DataType.Xml one       => one is { Modifier: null, Schema: null },
				_                          => true,
			};
		}

		if (difference is { Left: Ast.DataType.Named { Name.Parts: [var stamp], Arguments.Count: 0 }, Right: Ast.DataType.DateTime { Kind: Ast.DateTimeTypeKind.Timestamp } })
			return Bridge.Values.Unquoted(stamp.Text).Equals("timestamp", StringComparison.OrdinalIgnoreCase);

		if (difference.Right is not Ast.DataType.UserDefined { Name: var read })
			return false;

		static bool Same(Ast.QualifiedName one, Ast.QualifiedName two)
		{
			return one.Parts.Count == two.Parts.Count
				&& one.Parts.Zip(two.Parts).All(static pair => pair.First.Text.Equals(pair.Second.Text, StringComparison.OrdinalIgnoreCase));
		}

		static bool System(Ast.QualifiedName name)
		{
			var last = Bridge.Values.Unquoted(name.Parts[^1].Text);

			return !last.Equals("xml", StringComparison.OrdinalIgnoreCase)
				&& Bridge.Values.SystemTypes.Contains(last)
				&& (name.Parts.Count == 1 || name.Parts.Count == 2 && Bridge.Values.Unquoted(name.Parts[0].Text).Equals("sys", StringComparison.OrdinalIgnoreCase));
		}

		return difference.Left switch
		{
			Ast.DataType.Named { Arguments.Count: 0, Name: var name }                       => System(name) && Same(name, read),
			Ast.DataType { WrittenName: { } written } type when Bare(type)                  => Same(written, read),
			Ast.DataType.Xml { WrittenName: null, Modifier: null, Schema: null }            =>
				read.Parts is [var word] && word.Text.Equals("xml", StringComparison.OrdinalIgnoreCase),
			Ast.DataType.Character { Kind: Ast.CharacterTypeKind.Nvarchar, Length: null, Max: false } =>
				read.Parts is [var part] && part.Text.Equals("nvarchar", StringComparison.OrdinalIgnoreCase),
			_                                                                                => false,
		};
	}

	/// <summary>
	/// The date part that is the first argument of a date part function, where the standard reads a column of one
	/// part whose text is the part's word in the reference's table.
	/// </summary>
	static bool DateParted(Trees.Diff difference)
	{
		return difference is { Left: Ast.Expression.DatePart part, Right: Ast.Expression.Reference { Name.Parts: [var word] } }
			&& difference.Path is [.., { Node: Ast.Expression.Invocation { Name.Parts: [var function] }, Member: "Arguments", Index: 0 }, { Node: Ast.Argument, Member: "Value" }]
			&& Syntax.IsDatePartFunction(function.Text.ToUpperInvariant())
			&& Bridge.Values.DateParts.TryGetValue(word.Text, out var read) && read == (part.Kind, part.Spelling);
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
