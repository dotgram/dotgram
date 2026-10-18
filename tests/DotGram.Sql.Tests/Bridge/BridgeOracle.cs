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
/// <c>TransactSqlWriter</c>, read back by <c>TransactSqlParser</c> and held to the tree it came from — and,
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
		bool Standard = false, bool Shape = false, IReadOnlyList<string>? Dialects = null, IReadOnlyList<string>? Projected = null)
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

		// The projection below is the standard's form made of T-SQL's, so the bridge must have built T-SQL's: a
		// tree already in the standard's form would compare equal with nothing projected.
		if (Unprojected(made) is { } standardForm)
			return new Outcome(root, root, Status.Different, text, Detail: "T-SQL form: " + standardForm, Gaps: gaps);

		var recovered = Bridge.Recovered.ToArray();
		var standard  = SqlStandardParser.TryParseStatement(text);
		var read      = standard.IsSuccess;
		var shape     = false;
		string? detail = null;
		List<string>? dialects = null;
		var projected = new List<string>();

		if (read && !made.Equals(standard.Value) && Projection(made, projected) is var compared && !compared.Equals(standard.Value))
		{
			var differences = Trees.Differences(compared, standard.Value, old: false);

			dialects = Meaning(differences, out var unexplained);
			shape    = unexplained.Count > 0;
			detail   = "shape: " + (shape ? unexplained[0] : differences[0]);
		}

		string written;

		try
		{
			written = Ast.TransactSqlWriter.Write(made);
		}
		catch (Exception thrown)
		{
			var status = thrown is NotSupportedException or ArgumentException or InvalidOperationException ? Status.Refused : Status.Crashed;

			return new Outcome(root, root, status, text, Detail: $"{thrown.GetType().Name}: {thrown.Message}",
				Recovered: recovered, Gaps: gaps, Standard: read, Shape: shape, Dialects: dialects, Projected: projected);
		}

		var back = TransactSqlParser.TryParseSql(written);

		if (!back.IsSuccess || back.Value!.Length != 1)
			return new Outcome(root, root, Status.Unread, text, Written: written, Detail: detail,
				Recovered: recovered, Gaps: gaps, Standard: read, Shape: shape, Dialects: dialects, Projected: projected);

		var byCase = new List<string>();

		if (!back.Value[0].Equals(old) && Trees.Difference(old, back.Value[0], byCase: byCase) is { } difference)
			return new Outcome(root, root, Status.Different, text, Written: written, Detail: difference,
				Recovered: recovered, Gaps: gaps, Standard: read, Shape: shape, Dialects: dialects, Projected: projected);

		// The words the bridge read from the text are not in the old tree, so the old trees agreeing says nothing of
		// them: the written text, read and carried again, has to give the same new tree.
		if (Again(made, written) is { } lost)
			return new Outcome(root, root, Status.Different, text, Written: written, Detail: "new tree: " + lost,
				Recovered: recovered, Gaps: gaps, Standard: read, Shape: shape, Dialects: dialects, Projected: projected);

		return new Outcome(root, root, Status.Converted, text, Written: written, Detail: detail,
			ByCase: byCase, Recovered: recovered, Gaps: gaps, Standard: read, Shape: shape, Dialects: dialects, Projected: projected);
	}

	public const string ProjectedColumn = "a T-SQL column's options as the standard's members";
	public const string ProjectedKey    = "a key's T-SQL columns as the standard's names";

	public static readonly string[] Projections = [ProjectedColumn, ProjectedKey];

	/// <summary>
	/// The bridge's tree as the standard's parser would hold the same text, where T-SQL keeps a thing its own way
	/// and the standard another: the two projections the shape check makes before it compares, and the only
	/// comparisons it makes that are not plain equality.
	/// <list type="bullet">
	/// <item>A column's options, in the order written, onto the standard's members: a <c>COLLATE</c> right after a
	/// character type into the type, as the standard's grammar reads it there; then a <c>DEFAULT</c> into
	/// <c>Generation</c>, as <c>GENERATED ALWAYS AS ROW START</c> and <c>ROW END</c> are; the constraints in order
	/// into <c>Constraints</c>; a last <c>COLLATE</c> into <c>Collation</c>. Only where the options stand in that
	/// order and each has its counterpart, which is when the standard's parser reads the column at all.</item>
	/// <item>A PRIMARY KEY's or a UNIQUE's <c>Keys</c> onto its <c>Columns</c>, where each is a one-part name with
	/// no direction and the key has no other T-SQL part.</item>
	/// </list>
	/// A column or a key the projection does not apply to is left as it is, and so differs, and the statement is
	/// a shape mismatch. Each projection made is named in <paramref name="projected"/>.
	/// </summary>
	public static Ast.Statement Projection(Ast.Statement made, List<string> projected)
	{
		Ast.ColumnDefinition Column(Ast.ColumnDefinition column)
		{
			if (column.Options.Count == 0 || column.Generation is not null || column.Constraints.Count > 0 || column.Collation is not null)
				return column;

			var options = column.Options;
			var type    = column.Type;
			var at      = 0;

			if (type is Ast.DataType.Character { Collation: null } character && options[0] is Ast.ColumnOption.Collation { Name: null } typed)
			{
				type = character with { Collation = typed.Value };
				at++;
			}

			Ast.ColumnGeneration? generation = null;

			if (at < options.Count && options[at] is { Name: null } first)
			{
				generation = first switch
				{
					Ast.ColumnOption.Default @default                                              => new Ast.ColumnGeneration.Default(@default.Value),
					Ast.ColumnOption.GeneratedAlways { Kind: Ast.GeneratedAlwaysKind.RowStart } => new Ast.ColumnGeneration.RowStart(),
					Ast.ColumnOption.GeneratedAlways { Kind: Ast.GeneratedAlwaysKind.RowEnd }   => new Ast.ColumnGeneration.RowEnd(),
					_                                                                              => null,
				};

				if (generation is not null)
					at++;
			}

			var constraints = new List<Ast.Constraint>();

			for (; at < options.Count && options[at] is Ast.ColumnOption.Constraint { Name: null, Value: var constraint }; at++)
				constraints.Add(Key(constraint));

			Ast.CollationName? collation = null;

			if (at == options.Count - 1 && options[at] is Ast.ColumnOption.Collation { Name: null } last)
			{
				collation = last.Value;
				at++;
			}

			if (at < options.Count)
				return column;

			projected.Add(ProjectedColumn);

			return column with { Type = type, Generation = generation, Constraints = constraints.ToSqlList(), Collation = collation, Options = [] };
		}

		Ast.Constraint Key(Ast.Constraint constraint)
		{
			if (constraint is not Ast.Constraint.Unique { Keys: { Count: > 0 } keys, Clustering: null, Storage: null, Options.Count: 0, OptionsUnbracketed: false, On: null, Columns.Count: 0 } unique
				|| !keys.All(static one => one is { Direction: null, NullOrdering: null, Key: Ast.Expression.Reference { Name.Parts.Count: 1 } }))
				return constraint;

			projected.Add(ProjectedKey);

			return unique with { Keys = null, Columns = keys.Select(static one => ((Ast.Expression.Reference)one.Key).Name.Parts[0]).ToSqlList() };
		}

		Ast.TableElement Element(Ast.TableElement element)
		{
			return element switch
			{
				Ast.TableElement.Column one          => one with { Definition = Column(one.Definition) },
				Ast.TableElement.TableConstraint one => one with { Value = Key(one.Value) },
				_                                    => element,
			};
		}

		Ast.AlterTableAction Action(Ast.AlterTableAction action)
		{
			return action switch
			{
				Ast.AlterTableAction.AddColumn one     => one with { Column = Column(one.Column) },
				Ast.AlterTableAction.AddConstraint one => one with { Constraint = Key(one.Constraint) },
				Ast.AlterTableAction.Add one           => one with { Elements = one.Elements.Select(Element).ToSqlList() },
				_                                      => action,
			};
		}

		return made switch
		{
			Ast.Statement.CreateTable { Contents: Ast.TableContents.Elements elements } table
				=> table with { Contents = elements with { Items = elements.Items.Select(Element).ToSqlList() } },
			Ast.Statement.AlterTable alter => alter with { Actions = alter.Actions.Select(Action).ToSqlList() },
			_                              => made,
		};
	}

	/// <summary>
	/// A node of the bridge's tree in the standard's form where T-SQL has its own, or null: a column with the
	/// standard's <c>Generation</c>, <c>Constraints</c> or <c>Collation</c>, or a collation inside its type, and a
	/// PRIMARY KEY or UNIQUE with the standard's <c>Columns</c>.
	/// </summary>
	public static string? Unprojected(Ast.Statement made)
	{
		foreach (var node in Trees.Nodes(made))
		{
			switch (node)
			{
				case Ast.ColumnDefinition column when column.Generation is not null || column.Constraints.Count > 0 || column.Collation is not null:
					return $"column {column.Name.Text} has the standard's Generation, Constraints or Collation";

				case Ast.ColumnDefinition { Type: Ast.DataType.Character { Collation: not null } } column:
					return $"column {column.Name.Text} has a collation inside its type";

				case Ast.Constraint.Unique { Columns.Count: > 0 }:
					return "a key has the standard's Columns";
			}
		}

		return null;
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
	/// each record and reason, however many nodes throw it. A node that is a part of its parent and no record of
	/// its own is not carried alone: a MERGE arm's UPDATE, DELETE or INSERT, which has no target, and the rows of an
	/// INSERT that are not a query — <c>DEFAULT VALUES</c>, a file, a stream.
	/// </summary>
	static List<(string Record, string Reason)> Census(Statement old, string source)
	{
		var gaps  = new List<(string, string)>();
		var parts = new HashSet<object>(ReferenceEqualityComparer.Instance);

		foreach (var node in Trees.Nodes(old))
		{
			if (node is Clause.MergeWhen when)
				parts.Add(when.Action);

			if (node is Statement.Insert { Rows: Query.DefaultValues or Query.FromFile or Query.FromStream } insert)
				parts.Add(insert.Rows);

			// A signature's signers are carried with it as signers, never alone as values.
			if (node is Statement.AddSignature add)
				foreach (var one in add.By)
					parts.Add(one);

			if (node is Statement.DropSignature drop)
				foreach (var one in drop.Names)
					parts.Add(one);
		}

		foreach (var node in Trees.Nodes(old))
		{
			if (node is not (Statement or Query or Expression or TableReference) || parts.Contains(node))
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
	/// The thirteen constructs the two dialects read with different meanings. Each exempts one subtree of the
	/// bridge's tree, found by its construct and never by the statement's text:
	/// <list type="bullet">
	/// <item><c>TOP (n) a</c> is a row count in T-SQL, and the standard reads a call to <c>TOP</c> aliased <c>a</c>:
	/// a <c>Query.Select</c>'s <c>Top</c> and its first column.</item>
	/// <item><c>SET t.c = …</c> is a qualified column in T-SQL, and a mutated set clause on <c>t</c> in the
	/// standard: an <c>AssignmentTarget</c> whose name has more than one part.</item>
	/// <item><c>GLOBAL c</c> names a cursor of the connection in T-SQL, and is an extended name in the standard: a
	/// <c>CursorReference</c> that is <c>Global</c>.</item>
	/// <item><c>FREETEXT ((c), …)</c> and <c>CONTAINS ((c), …)</c> are T-SQL's full-text predicate, its columns a list,
	/// and a call with a value in brackets in the standard: the predicate, where the call has the same search and
	/// nothing else.</item>
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
	/// <item><c>SELECT x = a</c> names a column in T-SQL, and compares two in the standard: a
	/// <c>SelectColumn.ExpressionItem</c> that is <c>AliasFirst</c>.</item>
	/// <item><c>UPDATE ONLY (t)</c> calls a function <c>ONLY</c> in T-SQL, and is the table <c>t</c> alone in the
	/// standard: the statement's <c>Target</c>.</item>
	/// <item><c>c t DEFAULT v COLLATE x</c> collates the default value in T-SQL, and the column in the standard: the
	/// <c>ColumnDefinition</c> whose default is an <c>Expression.Collate</c>.</item>
	/// <item><c>EXECUTE p</c> calls a procedure in T-SQL, and runs a prepared statement in the standard: the whole
	/// statement, an <c>ExecuteModule</c> with the keyword EXECUTE, a one-part name and nothing else, against the
	/// standard's <c>Execute</c> of that name with nothing else.</item>
	/// </list>
	/// A difference outside every exempted subtree is unexplained, and makes the statement a shape mismatch.
	/// </summary>
	public static List<string> Meaning(List<Trees.Diff> differences, out List<Trees.Diff> unexplained)
	{
		var exempted = new List<string>();
		var subtrees = new List<object>();

		unexplained = [];

		// A collation moved is two differences, the default's value and the column's collation, and either alone is
		// another difference.
		var halves = differences.Select(Moved).Where(static one => one is not null).Select(static one => one!.Value).ToList();

		foreach (var difference in differences)
		{
			if (Moved(difference) is { } moved && halves.Count(one => ReferenceEquals(one.Column, moved.Column)) != 2)
				unexplained.Add(difference);
			else if (Exempting(difference) is not { } found)
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
	public const string FullText = "FREETEXT ((c), ...): T-SQL's full-text predicate, a call with a value in brackets in the standard";
	public const string DatePart = "DATEADD (day, ...): a date part in T-SQL, a column in the standard";
	public const string TypeName = "CAST (a AS money): a type of T-SQL's own in T-SQL, a user-defined type in the standard";

	public const string GroupAll = "GROUP BY ALL a: the empty groups kept in T-SQL, a quantifier in the standard";
	public const string Into     = "SELECT a INTO t: a new table in T-SQL, a target in the standard";
	public const string Option   = "FROM t OPTION (h): a hint in T-SQL, an alias in the standard";
	public const string Named    = "SELECT x = a: an alias in T-SQL, a comparison in the standard";
	public const string Only     = "UPDATE ONLY (t): a call to a function ONLY in T-SQL, the table t alone in the standard";
	public const string Collated = "c t DEFAULT v COLLATE x: the default's collation in T-SQL, the column's in the standard";
	public const string Execute  = "EXECUTE p: a procedure call in T-SQL, a prepared statement in the standard";

	public static readonly string[] Patterns = [Top, SetTc, Global, FullText, DatePart, TypeName, GroupAll, Into, Option, Named, Only, Collated, Execute];

	/// <summary>
	/// The call, where T-SQL's tree is <c>EXECUTE p</c> and nothing else and the standard's runs the prepared statement
	/// <c>p</c> and nothing else; or null.
	/// </summary>
	static Ast.Statement.ExecuteModule? Called(object? left, object? right)
	{
		if (left is not Ast.Statement.ExecuteModule { Module: Ast.ModuleReference.Named { Name.Parts: [var name] } } call || right is not Ast.Statement.Execute prepared)
			return null;

		var alone    = new Ast.Statement.ExecuteModule(new Ast.ModuleReference.Named(new Ast.QualifiedName([name])), []) { Keyword = Ast.ExecuteKeyword.Execute };
		var standard = new Ast.Statement.Execute { Statement = new Ast.StatementReference(name) };

		return call.Equals(alone) && prepared.Equals(standard) ? call : null;
	}

	/// <summary>
	/// The pattern whose subtree a difference lies in, and the node at the subtree's root, or null.
	/// </summary>
	static (string Pattern, object Subtree)? Exempting(Trees.Diff difference)
	{
		// EXECUTE p: the whole statement, a call of a one-part name with nothing else set, where the standard runs the
		// prepared statement of that name with nothing else.
		if (difference.Path is [] && Called(difference.Left, difference.Right) is { } call)
			return (Execute, call);

		// CONTAINS and FREETEXT: T-SQL's predicate where the standard reads a call of the same name.
		// Only the columns differ: the standard's call has the same search and nothing else.
		if (difference.Left is Ast.Expression.FullTextPredicate { Language: null } predicate && difference.Right is Ast.Expression.Invocation { Name.Parts: [var called], Arguments: [_, var searched] } fullTextCall
			&& called.Text.Equals(predicate.Kind == Ast.FullTextPredicateKind.Contains ? "CONTAINS" : "FREETEXT", StringComparison.OrdinalIgnoreCase)
			&& searched is { Name: null, NamedAssignment: false } && searched.Value.Equals(predicate.Search)
			&& fullTextCall is { Quantifier: null, OrderBy: null, Filter: null, Over: null, WithinGroup: null })
			return (FullText, predicate);

		// Two patterns are of one node each, which differs from the standard's in a known way and in no other.
		if (TypeNamed(difference) || DateParted(difference))
			return (TypeNamed(difference) ? TypeName : DatePart, difference.Left!);

		// OPTION: the statement's own hints, which the standard's statement has none of, and the alias the standard
		// reads the word OPTION as.
		if (difference.Path is [{ Node: Ast.Statement.Select { Hints.Count: > 0 } statement, Member: "Hints" }, ..]
			&& difference.Right is null or System.Collections.ICollection { Count: 0 })
			return (Option, statement);

		if (difference.Path is [{ Node: Ast.Statement.Select { Hints.Count: > 0 } }, .., { Member: "Alias" } last]
			&& difference.Right is Ast.Alias { Name.Text: var word } && word.Equals("OPTION", StringComparison.OrdinalIgnoreCase))
			return (Option, last.Node);

		// c t DEFAULT v COLLATE x: the column's default and collation, where T-SQL reads the collation as the default
		// value's own and the standard as the column's. Only the move itself: the default's value is the standard's,
		// and the column's collation the one the value was collated with (Meaning asks for both).
		if (Moved(difference) is { } moved)
			return (Collated, moved.Column);

		// SELECT x = a: the column the alias comes first in, which the standard reads as a comparison.
		if (difference.Path is [.., { Node: Ast.SelectColumn.ExpressionItem { AliasFirst: true } item, Member: "Expression" or "Alias" or "AliasFirst" }])
			return (Named, item);

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

				// UPDATE or DELETE ONLY (t): the target, a call in T-SQL and the standard's ONLY. The standard's INSERT
				// reads no ONLY.
				case Ast.Statement.Update { Target: { } changed } when step.Member == "Target" && CalledOnly(changed):
					return (Only, changed);

				case Ast.Statement.Delete { Target: { } changed } when step.Member == "Target" && CalledOnly(changed):
					return (Only, changed);


			}
		}

		return null;
	}

	/// <summary>
	/// One half of <c>c t DEFAULT v COLLATE x</c> read both ways: the default's value, a collation of the standard's
	/// value, or the column's collation, the standard's and the one the value was collated with; with the column.
	/// </summary>
	static (Ast.ColumnDefinition Column, bool Value)? Moved(Trees.Diff difference)
	{
		var path = difference.Path;

		if (path is [.., { Node: Ast.ColumnDefinition { Generation: Ast.ColumnGeneration.Default { Value: Ast.Expression.Collate collate }, Collation: null } column, Member: "Collation" }]
			&& difference is { Left: null, Right: Ast.CollationName name } && collate.Collation.Equals(name))
			return (column, false);

		if (path is [.., { Node: Ast.ColumnDefinition { Generation: Ast.ColumnGeneration.Default { Value: Ast.Expression.Collate value }, Collation: null } owner, Member: "Generation" }, { Node: Ast.ColumnGeneration.Default, Member: "Value" }]
			&& difference.Right is { } standard && value.Value.Equals(standard))
			return (owner, true);

		return null;
	}

	/// <summary>A data change statement's target that is a call to a function of one part named ONLY.</summary>
	static bool CalledOnly(Ast.TableSource target)
	{
		return target is Ast.TableSource.Function { Invocation.Name.Parts: [var name] } && name.Text.Equals("ONLY", StringComparison.OrdinalIgnoreCase);
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

		if (difference is { Left: Ast.DataType.Named { Name.Parts: [var stamp], Arguments.Count: <= 1 }, Right: Ast.DataType.DateTime { Kind: Ast.DateTimeTypeKind.Timestamp } })
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
