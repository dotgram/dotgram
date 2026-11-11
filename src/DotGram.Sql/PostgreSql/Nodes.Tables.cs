using System;
using System.Collections.Generic;

using DotGram.Sql.Ast;

namespace DotGram.Sql.PostgreSql;

// Inside the namespace, where they are asked before DotGram.Sql's own Expression, Query and Statement.
using Expression = DotGram.Sql.Ast.Expression;
using MatchType = DotGram.Sql.Ast.MatchType;

// The factories of the table statements: CREATE TABLE, its columns, constraints and partitions, and the key an index,
// an exclusion constraint, a partition key and ON CONFLICT share.
static partial class Nodes
{
	// ── Keys ───────────────────────────────────────────────────────────────────

	/// <summary>One key of an index_elem: the key, its collation, its operator class and that class's options, its order.</summary>
	public static IndexElement Indexed(Expression key, QualifiedName? collation, QualifiedName? operatorClass, Option? first, Option[]? rest, string? direction, string? nulls)
	{
		return new IndexElement(
			key,
			collation is { } named ? new CollationName(named) : null,
			operatorClass,
			List(first, rest),
			direction is null ? null : (direction[0] | 0x20) == 'a' ? SortDirection.Asc : SortDirection.Desc,
			nulls is null ? null : (nulls[0] | 0x20) == 'f' ? NullOrdering.First : NullOrdering.Last);
	}

	// ── CREATE TABLE ───────────────────────────────────────────────────────────

	/// <summary>A list of qualified names, the first and the rest.</summary>
	public static SqlList<QualifiedName> Names(QualifiedName? first, QualifiedName[]? rest)
	{
		if (first is not { } one)
			return [];

		var names = new QualifiedName[1 + (rest?.Length ?? 0)];

		names[0] = one;
		rest?.CopyTo(names, 1);

		return SqlList.Own(names);
	}

	/// <summary>Columns in brackets, with a period after them where one was named.</summary>
	public sealed record KeyColumns(Identifier[] Columns, Identifier? Period);

	/// <summary>SELECT INTO's table: its scope and spelling, UNLOGGED, the word TABLE, the name.</summary>
	public static SelectIntoTable Into(string? scope, string? word, bool unlogged, bool tableKeyword, QualifiedName name)
	{
		return new SelectIntoTable(name, null)
		{
			Scope        = word is null ? null : scope is null ? TableScope.Temporary : (scope[0] | 0x20) == 'g' ? TableScope.GlobalTemporary : TableScope.LocalTemporary,
			TempWord     = word is { Length: 4 },
			Unlogged     = unlogged,
			TableKeyword = tableKeyword,
		};
	}

	/// <summary>gram.y OptTemp: GLOBAL or LOCAL, TEMPORARY or TEMP, or UNLOGGED.</summary>
	public sealed record Temp(string? Scope, string? Word, bool Unlogged);

	/// <summary>What stands after a table's name.</summary>
	public abstract record TableBody
	{
		/// <summary>A parenthesis of elements, or of names alone, which is CREATE TABLE AS's.</summary>
		public sealed record Elements(TableItem[] Items) : TableBody;

		/// <summary><c>OF type</c> and the elements after it.</summary>
		public sealed record Of(QualifiedName Type, TableElement[]? Items) : TableBody;

		/// <summary><c>PARTITION OF parent</c>, the elements after it and the bound.</summary>
		public sealed record PartitionOf(QualifiedName Parent, TableElement[]? Items, PartitionBound Bound) : TableBody;

		/// <summary>Nothing: CREATE TABLE AS with no column list.</summary>
		public sealed record None : TableBody;
	}

	/// <summary>One item of a table's parenthesis: an element, or a name alone.</summary>
	public abstract record TableItem
	{
		public sealed record Element(TableElement Value) : TableItem;

		public sealed record Name(Identifier Value) : TableItem;
	}

	/// <summary>gram.y OptWith: the storage parameters, or WITHOUT OIDS.</summary>
	public sealed record With(SqlList<Option> Options, bool WithoutOids);

	/// <summary>What follows a table's elements, each where it was written.</summary>
	public sealed record TableTail(SqlList<QualifiedName> Inherits, PartitionSpecification? PartitionBy, Identifier? AccessMethod, With? With,
		TableCommitAction? OnCommit, Identifier? Tablespace, Statement? Query, WithDataMode? Data);

	/// <summary>
	/// Whether the parenthesis and what follows it are one of gram.y's shapes: CREATE TABLE AS with names alone or no
	/// parenthesis and no INHERITS or PARTITION BY; every other form with no name alone among its elements, and OF and
	/// PARTITION OF with no INHERITS.
	/// </summary>
	public static bool TableFits(TableBody body, TableTail tail)
	{
		if (tail.Query is not null)
		{
			if (tail.Inherits.Count > 0 || tail.PartitionBy is not null)
				return false;

			return body is TableBody.None
				|| body is TableBody.Elements { Items.Length: > 0 } names && System.Array.TrueForAll(names.Items, static one => one is TableItem.Name);
		}

		if (tail.Data is not null)
			return false;

		return body switch
		{
			TableBody.Elements elements => !System.Array.Exists(elements.Items, static one => one is TableItem.Name),
			TableBody.Of or TableBody.PartitionOf => tail.Inherits.Count == 0,
			_ => false,
		};
	}

	/// <summary>A CREATE TABLE of any of gram.y's shapes, in the standard's record with PostgreSQL's members.</summary>
	public static Statement Created(Temp? temp, bool ifNotExists, QualifiedName name, TableBody body, TableTail tail)
	{
		TableContents contents;
		PartitionParent? parent = null;

		switch (body)
		{
			case TableBody.Elements elements when tail.Query is not null:
				contents = new TableContents.AsQuery(SqlList.Own(System.Array.ConvertAll(elements.Items, static one => ((TableItem.Name)one).Value)), ((Statement.Select)tail.Query).Query, tail.Data);
				break;

			case TableBody.None:
				contents = new TableContents.AsQuery([], ((Statement.Select)tail.Query!).Query, tail.Data);
				break;

			case TableBody.Elements elements:
				contents = new TableContents.Elements(SqlList.Own(System.Array.ConvertAll(elements.Items, static one => ((TableItem.Element)one).Value)));
				break;

			case TableBody.Of of:
				contents = new TableContents.Typed(of.Type, null, List(of.Items));
				break;

			default:
				var partition = (TableBody.PartitionOf)body;

				contents = new TableContents.Elements(List(partition.Items));
				parent   = new PartitionParent(partition.Parent, partition.Bound);
				break;
		}

		return new Statement.CreateTable
		{
			Scope        = temp?.Word is null ? null : temp.Scope is null ? TableScope.Temporary : (temp.Scope[0] | 0x20) == 'g' ? TableScope.GlobalTemporary : TableScope.LocalTemporary,
			TempWord     = temp?.Word is { Length: 4 },
			Unlogged     = temp?.Unlogged == true,
			IfNotExists  = ifNotExists,
			Name         = name,
			Contents     = contents,
			PartitionOf  = parent,
			Inherits     = tail.Inherits,
			PartitionBy  = tail.PartitionBy,
			AccessMethod = tail.AccessMethod,
			Options      = tail.With?.Options ?? [],
			WithoutOids  = tail.With?.WithoutOids == true,
			OnCommit     = tail.OnCommit,
			Tablespace   = tail.Tablespace,
		};
	}

	/// <summary>gram.y parsePartitionStrategy: list, range or hash, in any case.</summary>
	public static bool IsStrategy(Identifier strategy)
	{
		var text = strategy.Style switch
		{
			IdentifierStyle.Delimited        => strategy.Text[1..^1].Replace("\"\"", "\""),
			IdentifierStyle.UnicodeDelimited => Unescaped(strategy.Text[3..^1].Replace("\"\"", "\""), strategy.UnicodeEscape ?? '\\'),
			_                                => strategy.Text,
		};

		return text.Equals("list", StringComparison.OrdinalIgnoreCase) || text.Equals("range", StringComparison.OrdinalIgnoreCase) || text.Equals("hash", StringComparison.OrdinalIgnoreCase);
	}

	/// <summary>A Unicode name's text with its escapes decoded: the escape and four hexits, or the escape, a plus and six.</summary>
	static string Unescaped(string text, char escape)
	{
		var decoded = new System.Text.StringBuilder(text.Length);

		for (var at = 0; at < text.Length; at++)
		{
			if (text[at] != escape || at + 1 >= text.Length)
			{
				decoded.Append(text[at]);
				continue;
			}

			if (text[at + 1] == escape)
			{
				decoded.Append(escape);
				at++;
			}
			else if (text[at + 1] == '+' && at + 8 <= text.Length && int.TryParse(text.Substring(at + 2, 6), System.Globalization.NumberStyles.HexNumber, null, out var wide))
			{
				decoded.Append(char.ConvertFromUtf32(wide));
				at += 7;
			}
			else if (at + 5 <= text.Length && int.TryParse(text.Substring(at + 1, 4), System.Globalization.NumberStyles.HexNumber, null, out var code))
			{
				decoded.Append((char)code);
				at += 4;
			}
			else
			{
				decoded.Append(text[at]);
			}
		}

		return decoded.ToString();
	}

	/// <summary>gram.y's hash bound: <c>modulus</c> and <c>remainder</c>, each once, and nothing else.</summary>
	public static bool HashFits(Option first, Option[]? rest)
	{
		return HashFits(SqlList.Own(Syntax.Listed(first, rest)));
	}

	/// <summary>gram.y's hash bound: <c>modulus</c> and <c>remainder</c>, each once with a whole number, and nothing else.</summary>
	public static bool HashFits(SqlList<Option> options)
	{
		var modulus   = 0;
		var remainder = 0;

		foreach (var option in options)
		{
			if (option is not { Value: LiteralValue.Numeric { Kind: NumericLiteralKind.DecimalInteger or NumericLiteralKind.HexInteger or NumericLiteralKind.OctalInteger or NumericLiteralKind.BinaryInteger }, Options.Count: 0, Words.Count: 0 })
				return false;

			// The server compares the folded name: a plain one in any case, a quoted one as written.
			var name = option.Name.Style == IdentifierStyle.Delimited ? option.Name.Text[1..^1] : option.Name.Text.ToLowerInvariant();

			if (name == "modulus")
				modulus++;
			else if (name == "remainder")
				remainder++;
			else
				return false;
		}

		return modulus == 1 && remainder == 1;
	}

	/// <summary>One of a column's qualifiers in gram.y's ColQualList.</summary>
	public abstract record Qual
	{
		public sealed record Constraint(Ast.Constraint Value) : Qual;

		/// <summary>DEFAULT, an identity or a generation expression, and the name CONSTRAINT gave it, which the tree has no slot for.</summary>
		public sealed record Generation(ColumnGeneration Value, Identifier? Name) : Qual;

		/// <summary>A constraint attribute, which is the constraint's before it.</summary>
		public sealed record Attribute(string Word) : Qual;

		public sealed record Collate(CollationName Name) : Qual;
	}

	/// <summary>A qualifier with the name CONSTRAINT gave it.</summary>
	public static Qual QualNamed(Qual qual, Identifier name)
	{
		return qual switch
		{
			Qual.Constraint constraint => new Qual.Constraint(constraint.Value with { Name = new QualifiedName(name) }),
			Qual.Generation generation => generation with { Name = name },
			_                          => qual,
		};
	}

	/// <summary>
	/// Whether a default reads back as itself after DEFAULT, where gram.y reads a b_expr (DefaultExpr): one comparison, or IS
	/// [NOT] DISTINCT FROM or IS [NOT] DOCUMENT after one, over arithmetic with no COLLATE, no AT TIME ZONE, no AND, OR or
	/// NOT and no predicate outside brackets and calls. The writer brackets any other.
	/// </summary>
	public static bool IsBExpr(Expression value)
	{
		return value switch
		{
			Expression.IsDistinct distinct => IsCompared(distinct.Left) && IsCompared(distinct.Right),
			Expression.IsDocument document => IsCompared(document.Value),
			_                              => IsCompared(value),
		};
	}

	static bool IsCompared(Expression value)
	{
		return value is Expression.Comparison comparison ? IsArithmetic(comparison.Left) && IsArithmetic(comparison.Right) : IsArithmetic(value);
	}

	static bool IsArithmetic(Expression value)
	{
		return value switch
		{
			Expression.AtTimeZone or Expression.Collate or Expression.Comparison or Expression.IsDistinct or Expression.IsDocument or Expression.IsNull
				or Expression.IsTruth or Expression.IsNormalized or Expression.Between or Expression.In or Expression.Like or Expression.Overlaps
				or Expression.QuantifiedComparison or Expression.QuantifiedOperation => false,
			Expression.Binary { Operator: BinaryOperator.And or BinaryOperator.Or } => false,
			Expression.Binary binary                                                  => IsArithmetic(binary.Left) && IsArithmetic(binary.Right),
			Expression.Unary { Operator: UnaryOperator.Not }                          => false,
			Expression.Unary unary                                                    => IsArithmetic(unary.Operand),
			Expression.OperatorCall call                                              => (call.Left is null || IsArithmetic(call.Left)) && IsArithmetic(call.Right),
			Expression.Cast { Spelling: CastSpelling.DoubleColon } cast               => IsArithmetic(cast.Value),
			_                                                                         => true,
		};
	}

	/// <summary>An identity column's generation and its sequence options.</summary>
	public static Qual Identity(string generation, SequenceOption? first, SequenceOption[]? rest)
	{
		return new Qual.Generation(new ColumnGeneration.Identity((generation[0] | 0x20) == 'a' ? IdentityGeneration.Always : IdentityGeneration.ByDefault, List(first, rest)), null);
	}

	/// <summary>A generated column's expression, STORED or VIRTUAL.</summary>
	public static Qual Generated(Expression expression, string? storage)
	{
		return new Qual.Generation(
			new ColumnGeneration.Generated(expression) { Storage = storage is null ? null : (storage[0] | 0x20) == 's' ? GenerationStorage.Stored : GenerationStorage.Virtual },
			null);
	}

	/// <summary>A PRIMARY KEY or UNIQUE on a column, or at the table with no columns of its own named here.</summary>
	public static Constraint Key(UniqueKind kind, NullDistinctness? nulls, SqlList<Identifier> columns, Option[]? with, Identifier? space)
	{
		return new Constraint.Unique(kind, nulls, columns) { Options = List(with), IndexTablespace = space };
	}

	/// <summary>A table's PRIMARY KEY or UNIQUE over its columns, the last a period WITHOUT OVERLAPS where one was.</summary>
	public static Constraint TableKey(string kind, NullDistinctness? nulls, Identifier first, Identifier[]? rest, bool overlaps, Identifier[]? include, Option[]? with, Identifier? space)
	{
		var columns = Syntax.Listed(first, rest);
		var period  = overlaps ? columns[^1] : null;

		if (overlaps)
			System.Array.Resize(ref columns, columns.Length - 1);

		return new Constraint.Unique((kind[0] | 0x20) == 'u' ? UniqueKind.Unique : UniqueKind.PrimaryKey, nulls, SqlList.Own(columns), period)
		{
			Include         = List(include),
			Options         = List(with),
			IndexTablespace = space,
		};
	}

	/// <summary>An EXCLUDE constraint.</summary>
	public static Constraint Excluded(Identifier? method, ExclusionElement first, ExclusionElement[]? rest, Identifier[]? include, Option[]? with, Identifier? space, Expression? where)
	{
		return new Constraint.Exclude(method, List(first, rest), where) { Include = List(include), Options = List(with), IndexTablespace = space };
	}

	/// <summary>A constraint with the name CONSTRAINT gave it.</summary>
	public static Constraint Named(Constraint constraint, Identifier? name)
	{
		return name is null ? constraint : constraint with { Name = new QualifiedName(name) };
	}

	/// <summary>A table's FOREIGN KEY: its columns and period, and the REFERENCES with the referenced columns and period.</summary>
	public static Constraint Foreign(KeyColumns keys, QualifiedName table, KeyColumns? referenced, MatchType? match, KeyActions? actions)
	{
		return new Constraint.ForeignKey(SqlList.Own(keys.Columns), keys.Period, References(table, referenced is null ? [] : SqlList.Own(referenced.Columns), referenced?.Period, match, actions));
	}

	/// <summary>gram.y key_match.</summary>
	public static MatchType MatchOf(string word)
	{
		return (word[0] | 0x20) switch { 'f' => MatchType.Full, 'p' => MatchType.Partial, _ => MatchType.Simple };
	}

	/// <summary>One referential action, and the columns SET NULL or SET DEFAULT names.</summary>
	public sealed record KeyAction(ReferentialAction Action, SqlList<Identifier> Columns);

	/// <summary>gram.y key_actions: the update and the delete rule, and whether the delete rule was written first.</summary>
	public sealed record KeyActions(KeyAction? Update, KeyAction? Delete, bool DeleteFirst);

	/// <summary>A REFERENCES clause.</summary>
	public static ReferencesSpecification References(QualifiedName table, SqlList<Identifier> columns, Identifier? period, MatchType? match, KeyActions? actions)
	{
		return new ReferencesSpecification(table, columns, period, match, actions?.Update?.Action, actions?.Delete?.Action, actions?.DeleteFirst ?? false)
		{
			OnDeleteColumns = actions?.Delete?.Columns ?? [],
		};
	}

	/// <summary>gram.y TableLikeOption, INCLUDING or EXCLUDING.</summary>
	public static LikeOption LikeOptionOf(string inclusion, string kind)
	{
		var including = (inclusion[0] | 0x20) == 'i';

		return kind.ToUpperInvariant() switch
		{
			"IDENTITY"    => including ? LikeOption.IncludingIdentity : LikeOption.ExcludingIdentity,
			"DEFAULTS"    => including ? LikeOption.IncludingDefaults : LikeOption.ExcludingDefaults,
			"GENERATED"   => including ? LikeOption.IncludingGenerated : LikeOption.ExcludingGenerated,
			"COMMENTS"    => including ? LikeOption.IncludingComments : LikeOption.ExcludingComments,
			"COMPRESSION" => including ? LikeOption.IncludingCompression : LikeOption.ExcludingCompression,
			"CONSTRAINTS" => including ? LikeOption.IncludingConstraints : LikeOption.ExcludingConstraints,
			"INDEXES"     => including ? LikeOption.IncludingIndexes : LikeOption.ExcludingIndexes,
			"STATISTICS"  => including ? LikeOption.IncludingStatistics : LikeOption.ExcludingStatistics,
			"STORAGE"     => including ? LikeOption.IncludingStorage : LikeOption.ExcludingStorage,
			_             => including ? LikeOption.IncludingAll : LikeOption.ExcludingAll,
		};
	}

	// ── Constraint attributes ──────────────────────────────────────────────────

	/// <summary>Which attributes a kind of table constraint takes, as gram.y's processCASbits allows them.</summary>
	public enum Takes { Check, NotNull, Key, ForeignKey }

	/// <summary>
	/// Whether a table constraint's attributes fit it: no two that conflict (gram.y's ConstraintAttributeSpec), only those its
	/// kind takes (processCASbits) — NOT DEFERRABLE and INITIALLY IMMEDIATE, which say the default, are taken by every kind —
	/// and none said twice, which the tree has one slot for.
	/// </summary>
	public static bool AttributesFit(string[]? words, Takes takes)
	{
		if (words is null)
			return true;

		var seen = new HashSet<string>(StringComparer.Ordinal);

		foreach (var word in words)
		{
			var one = Attribute(word);

			if (!seen.Add(one))
				return false;

			var taken = one switch
			{
				"DEFERRABLE" or "INITIALLY DEFERRED" => takes is Takes.Key or Takes.ForeignKey,
				"ENFORCED" or "NOT ENFORCED"         => takes is Takes.Check or Takes.ForeignKey,
				"NOT VALID"                          => takes is Takes.Check or Takes.NotNull or Takes.ForeignKey,
				"NO INHERIT"                         => takes is Takes.Check or Takes.NotNull,
				_                                    => true,
			};

			if (!taken)
				return false;
		}

		return !(seen.Contains("NOT DEFERRABLE") && (seen.Contains("DEFERRABLE") || seen.Contains("INITIALLY DEFERRED"))
			|| seen.Contains("INITIALLY IMMEDIATE") && seen.Contains("INITIALLY DEFERRED")
			|| seen.Contains("ENFORCED") && seen.Contains("NOT ENFORCED"));
	}

	/// <summary>An attribute's words, upper case and one space apart.</summary>
	static string Attribute(string word)
	{
		return string.Join(" ", word.ToUpperInvariant().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
	}

	/// <summary>A table constraint with its attributes: the characteristics, NOT VALID and NO INHERIT.</summary>
	public static Constraint Attributed(Constraint constraint, string[]? words)
	{
		if (words is null)
			return constraint;

		var attributes = System.Array.ConvertAll(words, Attribute);
		var valid      = System.Array.IndexOf(attributes, "NOT VALID") < 0;
		var inherit    = System.Array.IndexOf(attributes, "NO INHERIT") < 0;

		return constraint with
		{
			Characteristics = Characteristics(attributes),
			NotValid        = !valid,
			NoInherit       = constraint.NoInherit || !inherit,
		};
	}

	/// <summary>
	/// The characteristics the attributes say, or null where they say none: the timing, deferrable or not, enforced or not,
	/// and whether DEFERRABLE stood before the timing, as the standard's record keeps it.
	/// </summary>
	static ConstraintCharacteristics? Characteristics(IReadOnlyList<string> attributes)
	{
		ConstraintTiming? timing     = null;
		bool?             deferrable = null;
		bool?             enforced   = null;
		var               first      = false;

		foreach (var attribute in attributes)
		{
			switch (attribute)
			{
				case "DEFERRABLE":
				case "NOT DEFERRABLE":
					deferrable = attribute == "DEFERRABLE";
					first      = timing is null;
					break;

				case "INITIALLY DEFERRED":
				case "INITIALLY IMMEDIATE":
					timing = attribute == "INITIALLY DEFERRED" ? ConstraintTiming.InitiallyDeferred : ConstraintTiming.InitiallyImmediate;
					break;

				case "ENFORCED":
				case "NOT ENFORCED":
					enforced = attribute == "ENFORCED";
					break;
			}
		}

		return timing is null && deferrable is null && enforced is null ? null : new ConstraintCharacteristics(timing, deferrable, enforced, deferrable is not null && first);
	}

	// ── Columns ────────────────────────────────────────────────────────────────

	/// <summary>
	/// Whether a column's qualifiers are what the server takes and the tree holds: one default or generation, with no name
	/// (CONSTRAINT before DEFAULT has no slot); not NULL beside NOT NULL; and each attribute after a constraint that takes it,
	/// once (transformConstraintAttrs) — DEFERRABLE and INITIALLY after a key or a foreign key, ENFORCED after a foreign key
	/// or a CHECK, NOT DEFERRABLE with no INITIALLY DEFERRED.
	/// </summary>
	public static bool QualsFit(Qual[]? quals)
	{
		if (quals is null)
			return true;

		var generations = 0;
		var notNull     = false;
		var nullable    = false;

		Constraint? last = null;
		var seen = new HashSet<string>(StringComparer.Ordinal);

		foreach (var qual in quals)
		{
			switch (qual)
			{
				case Qual.Generation generation:
					if (++generations > 1)
						return false;

					last = null;
					break;

				case Qual.Constraint constraint:
					notNull  |= constraint.Value is Constraint.NotNull;
					nullable |= constraint.Value is Constraint.Null;
					last      = constraint.Value;
					seen.Clear();
					break;

				case Qual.Attribute attribute:
					var one = Attribute(attribute.Word);

					var taken = one switch
					{
						"DEFERRABLE" or "NOT DEFERRABLE" or "INITIALLY DEFERRED" or "INITIALLY IMMEDIATE" => last is Constraint.Unique or Constraint.ForeignKey,
						_                                                                               => last is Constraint.ForeignKey or Constraint.Check,
					};

					// One of each axis: DEFERRABLE or NOT DEFERRABLE, one INITIALLY, one ENFORCED or NOT ENFORCED.
					var axis = one.EndsWith("DEFERRABLE", StringComparison.Ordinal) ? "d" : one.StartsWith("INITIALLY", StringComparison.Ordinal) ? "i" : "e";

					if (!taken || !seen.Add(axis))
						return false;

					if (one == "NOT DEFERRABLE" && seen.Contains("i!") || one == "INITIALLY DEFERRED" && seen.Contains("d!"))
						return false;

					if (one == "NOT DEFERRABLE")
						seen.Add("d!");

					if (one == "INITIALLY DEFERRED")
						seen.Add("i!");

					break;
			}
		}

		return !(notNull && nullable) && Collations(quals) <= 1;
	}

	/// <summary>How many COLLATE clauses the qualifiers hold; gram.y refuses a second (multiple COLLATE clauses not allowed).</summary>
	static int Collations(Qual[] quals)
	{
		var count = 0;

		foreach (var qual in quals)
		{
			if (qual is Qual.Collate)
				count++;
		}

		return count;
	}

	/// <summary>A column of gram.y's columnDef, its qualifiers in the standard's slots.</summary>
	public static ColumnDefinition Column(Identifier name, DataType type, Identifier? storage, Identifier? compression, Qual[]? quals)
	{
		var (generation, constraints, collation) = Split(quals);

		return new ColumnDefinition(name, type, generation, constraints, collation) { Storage = storage, Compression = compression };
	}

	/// <summary>
	/// Whether a typed table's column options are what the tree's options hold: the qualifiers a column takes, with a plain
	/// DEFAULT and no identity, generation or collation.
	/// </summary>
	public static bool OptionsFit(Qual[]? quals)
	{
		return QualsFit(quals) && (quals is null || System.Array.TrueForAll(quals, static one => one is not Qual.Generation { Value: not ColumnGeneration.Default } and not Qual.Generation { Name: not null }));
	}

	/// <summary>A typed table's column options: the default and the constraints.</summary>
	public static TableElement ColumnOptions(Identifier name, bool withOmitted, Qual[]? quals)
	{
		var (generation, constraints, collation) = Split(quals);

		return new TableElement.ColumnOptions(name, new ColumnOptionList(null, (generation as ColumnGeneration.Default)?.Value, constraints) { Collation = collation }) { WithOptionsOmitted = withOmitted };
	}

	/// <summary>The qualifiers in the standard's slots: the generation, the constraints with their attributes, the collation.</summary>
	static (ColumnGeneration? Generation, SqlList<Constraint> Constraints, CollationName? Collation) Split(Qual[]? quals)
	{
		if (quals is null)
			return (null, [], null);

		ColumnGeneration? generation = null;
		CollationName?    collation  = null;

		var constraints = new List<Constraint>();
		var attributes  = new List<string>();

		void Flush()
		{
			if (attributes.Count > 0)
			{
				constraints[^1] = constraints[^1] with { Characteristics = Characteristics(attributes) };
				attributes.Clear();
			}
		}

		foreach (var qual in quals)
		{
			switch (qual)
			{
				case Qual.Generation one:
					generation = one.Name is null ? one.Value : one.Value with { Name = new QualifiedName(one.Name) };
					break;

				case Qual.Constraint one:
					Flush();
					constraints.Add(one.Value);
					break;

				case Qual.Attribute one:
					attributes.Add(Attribute(one.Word));
					break;

				case Qual.Collate one:
					collation = one.Name;
					break;
			}
		}

		Flush();

		return (generation, SqlList.Own(constraints.ToArray()), collation);
	}
}
