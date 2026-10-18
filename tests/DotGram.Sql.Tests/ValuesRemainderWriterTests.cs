using System;

using DotGram.Sql.Ast;
using DotGram.Sql.TransactSql;

using Xunit;

namespace DotGram.Sql.Tests;

using Expression = DotGram.Sql.Ast.Expression;
using Identifier = DotGram.Sql.Ast.Identifier;
using LiteralValue = DotGram.Sql.Ast.LiteralValue;
using QualifiedName = DotGram.Sql.Ast.QualifiedName;
using Query = DotGram.Sql.Ast.Query;
using Statement = DotGram.Sql.Ast.Statement;

/// <summary>
/// The writer for the remainder outside the DDL: each tree written and read back by TransactSqlParser to the
/// same tree, and every tree whose text would read back as another refused.
/// </summary>
public sealed class ValuesRemainderWriterTests
{
	static Identifier Name(string text)
	{
		return new Identifier(text);
	}

	static QualifiedName Qualified(string text)
	{
		return new QualifiedName([Name(text)]);
	}

	static Expression Ref(string word)
	{
		return new Expression.Reference(Qualified(word));
	}

	static Expression Number(string text)
	{
		return new LiteralValue.Numeric(text, NumericLiteralKind.DecimalInteger);
	}

	static Expression Text(string text)
	{
		return new LiteralValue.String("'" + text + "'", StringLiteralKind.Character);
	}

	static Option Set(string name, Expression? value = null)
	{
		return new Option(Name(name), value, []);
	}

	static IndexDefinition Index(params Option[] options)
	{
		return new IndexDefinition(Name("i")) { Columns = [new SortItem(Ref("a"))], Options = [.. options] };
	}

	static Statement Tree(string which)
	{
		return which switch
		{
			"SAVE TRANSACTION numbered" => new Statement.SaveTransaction(new TransactionName(null) { Number = new LiteralValue.Numeric("-5", NumericLiteralKind.DecimalInteger), Qualified = new QualifiedName([Name("a"), Name("b")]) }),
			"COMMIT numbered" => new Statement.Commit { Keyword = TransactionKeyword.Tran, Name = new TransactionName(null) { Number = (LiteralValue.Numeric)Number("1"), Qualified = new QualifiedName([Name("x"), Name("y")]) } },
			"IS DISTINCT FROM SOME" => Where(new Expression.IsDistinct(Ref("a"), false, new Expression.Subquery(new Query.Select { Columns = [new SelectColumn.ExpressionItem(Ref("b"))], From = new FromClause([new TableSource.Named(Qualified("u"))]) })) { Quantifier = Quantifier.Some }),
			"AI_GENERATE_EMBEDDINGS" => new Statement.Select { Query = new Query.Select { Columns = [new SelectColumn.ExpressionItem(new Expression.AiGenerateEmbeddings(Text("x"), new QualifiedName([Name("dbo"), Name("m")])) { Parameters = Ref("p") })] } },
			"AI_GENERATE_CHUNKS" => new Statement.Select { Query = new Query.Select { Columns = [new SelectColumn.All()], From = new FromClause([new TableSource.AiGenerateChunks(Text("x"), Name("fixed"), Number("5")) { Overlap = Number("1"), Alias = new Alias(Name("c"), null, true) }]) } },
			"IDENTITY" => Into(new SelectColumn.ExpressionItem(Identity(Number("1"), new LiteralValue.Numeric("-1.5", NumericLiteralKind.Decimal)), Name("id"), true)),
			"IDENTITY named first" => Into(new SelectColumn.ExpressionItem(new Expression.Identity(new DataType.Numeric(NumericTypeKind.Int)), Name("id")) { AliasFirst = true }),
			"a variable's method" => From(new TableSource.Function(new Expression.Invocation(Qualified("f"), [new Argument(Number("1"))]), new Alias(Name("t"), [Name("c")], true)) { Variable = Name("@v") }),
			"DATEPART of a date part bracketed" => Dated(new Expression.Reference(new QualifiedName([new Identifier("[yy]", IdentifierStyle.Bracketed)]))),
			"DATEPART of a date part in brackets" => Dated(new Expression.Parenthesized(Ref("year"))),
			"DATEPART of a string" => Dated(new LiteralValue.String("'year'", StringLiteralKind.National)),
			"a variable's method named as a built-in" => From(new TableSource.Function(new Expression.Invocation(Qualified("DATEPART"), [new Argument(Ref("foo")), new Argument(Ref("x"))])) { Variable = Name("@v") }),
			"embeddings of a model named PARAMETERS" => new Statement.Select { Query = new Query.Select { Columns = [new SelectColumn.ExpressionItem(new Expression.AiGenerateEmbeddings(Text("x"), Qualified("PARAMETERS")))] } },
			"FREETEXT of a property with a part left out" => new Statement.Select { Query = new Query.Select { Columns = [new SelectColumn.ExpressionItem(Ref("a"))], From = new FromClause([new TableSource.Named(Qualified("t"))]), Where = new Expression.FullTextPredicate(FullTextPredicateKind.FreeText, [new Expression.Invocation(Qualified("PROPERTY"), [new Argument(new Expression.Reference(new QualifiedName([Name("db"), Name(""), Name("t"), Name("c")]))), new Argument(Text("Title"))])], Text("x")) } },
			"CONTAINS of a property" => Where(new Expression.FullTextPredicate(FullTextPredicateKind.Contains, [Property(Text("Title"))], Text("x"))),
			"CONTAINS" => Where(new Expression.FullTextPredicate(FullTextPredicateKind.Contains, [Ref("b"), Ref("c")], Text("x")) { ColumnsBracketed = true, Language = Number("1033") }),
			_ => throw new ArgumentOutOfRangeException(nameof(which), which, null),
		};
	}

	static Expression.Identity Identity(Expression seed, LiteralValue.Numeric increment)
	{
		return new Expression.Identity(new DataType.Numeric(NumericTypeKind.Int)) { Seed = (LiteralValue.Numeric)seed, Increment = increment };
	}

	static Statement Into(SelectColumn column, bool into = true)
	{
		return new Statement.Select { Query = new Query.Select { Columns = [column], IntoTable = into ? new SelectIntoTable(Qualified("#t"), null) : null, From = new FromClause([new TableSource.Named(Qualified("t"))]) } };
	}

	static LiteralValue.Numeric Num(string text)
	{
		return new LiteralValue.Numeric(text, NumericLiteralKind.DecimalInteger);
	}

	static Statement From(TableSource source)
	{
		return new Statement.Select { Query = new Query.Select { Columns = [new SelectColumn.ExpressionItem(Ref("c1"))], From = new FromClause([source]) } };
	}

	static Statement Dated(Expression part)
	{
		return new Statement.Select { Query = new Query.Select { Columns = [new SelectColumn.ExpressionItem(new Expression.Invocation(Qualified("DATEPART"), [new Argument(part), new Argument(Ref("x"))]))], From = new FromClause([new TableSource.Named(Qualified("t"))]) } };
	}

	static Expression Property(Expression name)
	{
		return new Expression.Invocation(Qualified("PROPERTY"), [new Argument(Ref("c")), new Argument(name)]);
	}

	static Statement Where(Expression condition)
	{
		return new Statement.Select { Query = new Query.Select { Columns = [new SelectColumn.ExpressionItem(Ref("a"))], From = new FromClause([new TableSource.Named(Qualified("t"))]), Where = condition } };
	}

	[Theory]
	[InlineData("SAVE TRANSACTION numbered", "SAVE TRANSACTION -5 : a.b")]
	[InlineData("COMMIT numbered", "COMMIT TRAN 1 : x.y")]
	[InlineData("CONTAINS", "SELECT a FROM t WHERE CONTAINS((b, c), 'x', LANGUAGE 1033)")]
	[InlineData("IS DISTINCT FROM SOME", "SELECT a FROM t WHERE a IS DISTINCT FROM SOME (SELECT b FROM u)")]
	[InlineData("AI_GENERATE_EMBEDDINGS", "SELECT AI_GENERATE_EMBEDDINGS('x' USE MODEL dbo.m PARAMETERS p)")]
	[InlineData("IDENTITY", "SELECT IDENTITY(INT, 1, -1.5) AS id INTO #t FROM t")]
	[InlineData("IDENTITY named first", "SELECT id = IDENTITY(INT) INTO #t FROM t")]
	[InlineData("a variable's method", "SELECT c1 FROM @v.f(1) AS t (c)")]
	[InlineData("DATEPART of a date part bracketed", "SELECT DATEPART([yy], x) FROM t")]
	[InlineData("DATEPART of a date part in brackets", "SELECT DATEPART((year), x) FROM t")]
	[InlineData("DATEPART of a string", "SELECT DATEPART(N'year', x) FROM t")]
	[InlineData("a variable's method named as a built-in", "SELECT c1 FROM @v.DATEPART(foo, x)")]
	[InlineData("embeddings of a model named PARAMETERS", "SELECT AI_GENERATE_EMBEDDINGS('x' USE MODEL PARAMETERS)")]
	[InlineData("CONTAINS of a property", "SELECT a FROM t WHERE CONTAINS(PROPERTY(c, 'Title'), 'x')")]
	[InlineData("FREETEXT of a property with a part left out", "SELECT a FROM t WHERE FREETEXT(PROPERTY(db..t.c, 'Title'), 'x')")]
	[InlineData("AI_GENERATE_CHUNKS", "SELECT * FROM AI_GENERATE_CHUNKS(SOURCE = 'x', CHUNK_TYPE = fixed, CHUNK_SIZE = 5, OVERLAP = 1) AS c")]
	public void Each_tree_writes_its_text(string which, string text)
	{
		Assert.Equal(text, TransactSqlWriter.Write(Tree(which)));
	}

	[Theory]
	[InlineData("SAVE TRANSACTION numbered")]
	[InlineData("COMMIT numbered")]
	[InlineData("CONTAINS")]
	[InlineData("IS DISTINCT FROM SOME")]
	[InlineData("AI_GENERATE_EMBEDDINGS")]
	[InlineData("AI_GENERATE_CHUNKS")]
	[InlineData("IDENTITY")]
	[InlineData("IDENTITY named first")]
	[InlineData("a variable's method")]
	[InlineData("DATEPART of a date part bracketed")]
	[InlineData("DATEPART of a date part in brackets")]
	[InlineData("DATEPART of a string")]
	[InlineData("a variable's method named as a built-in")]
	[InlineData("embeddings of a model named PARAMETERS")]
	[InlineData("CONTAINS of a property")]
	[InlineData("FREETEXT of a property with a part left out")]
	public void Each_tree_reads_back_as_itself(string which)
	{
		var tree = Tree(which);
		var text = TransactSqlWriter.Write(tree);

		Assert.Equal(tree, Bridge.ToAst(TransactSqlParser.Located.ParseStatement(text), text));
	}

	[Theory]
	[InlineData("a numbered name with a plus")]
	[InlineData("a numbered name of one part")]
	[InlineData("a numbered name beside a name")]
	[InlineData("a number with no name")]
	[InlineData("CONTAINS of two columns unbracketed")]
	[InlineData("CONTAINS of * beside a column")]
	[InlineData("CONTAINS searching a number")]
	[InlineData("CONTAINS in a language of a name")]
	[InlineData("CONTAINS where a value stands")]
	[InlineData("IS DISTINCT FROM ALL a value")]
	[InlineData("AI_GENERATE_EMBEDDINGS a model with an empty part")]
	[InlineData("DISTRIBUTED_AGG on a list in brackets")]
	[InlineData("chunks of another type")]
	[InlineData("chunks with a set id and no overlap")]
	[InlineData("chunks with an alias's columns")]
	[InlineData("IDENTITY with no INTO")]
	[InlineData("IDENTITY unnamed")]
	[InlineData("IDENTITY inside a value")]
	[InlineData("IDENTITY in a WHERE")]
	[InlineData("IDENTITY with a seed and no increment")]
	[InlineData("IDENTITY seeded by a hex number")]
	[InlineData("a variable's method of two parts")]
	[InlineData("a variable's method with no @")]
	[InlineData("a variable's method as an INSERT's target")]
	[InlineData("DATEPART of a date part's word as a column")]
	[InlineData("DATEPART of a date part in brackets")]
	[InlineData("DATEPART of a bracketed name that is no date part")]
	[InlineData("DATEPART of a qualified column")]
	[InlineData("DATEPART of nothing")]
	[InlineData("CONTAINS of a property of a number")]
	[InlineData("CONTAINS of a property of one argument")]
	[InlineData("CONTAINS of a property with DISTINCT")]
	[InlineData("CONTAINS of another call")]
	[InlineData("a variable's method with OVER")]
	[InlineData("a variable's method of a date part")]
	[InlineData("a date part outside a call")]
	[InlineData("CONTAINS of a property whose last part is left out")]
	public void A_tree_with_no_text_is_refused(string which)
	{
		Statement node = which switch
		{
			// TranName: '-'? & Lexical.Digits; a plus sign is Msg 102.
			"a numbered name with a plus" => new Statement.SaveTransaction(new TransactionName(null) { Number = new LiteralValue.Numeric("+5", NumericLiteralKind.DecimalInteger), Qualified = new QualifiedName([Name("a"), Name("b")]) }),
			// TranName: a name of two parts and no more.
			"a numbered name of one part" => new Statement.SaveTransaction(new TransactionName(null) { Number = (LiteralValue.Numeric)Number("5"), Qualified = Qualified("a") }),
			// TranName: one of the three forms.
			"a numbered name beside a name" => new Statement.SaveTransaction(new TransactionName(Name("t")) { Number = (LiteralValue.Numeric)Number("5"), Qualified = new QualifiedName([Name("a"), Name("b")]) }),
			// FullTextColumns: two columns or more in brackets.
			"CONTAINS of two columns unbracketed" => Where(new Expression.FullTextPredicate(FullTextPredicateKind.Contains, [Ref("b"), Ref("c")], Text("x"))),
			// FullTextAll: * alone.
			"CONTAINS of * beside a column" => Where(new Expression.FullTextPredicate(FullTextPredicateKind.Contains, [new Expression.Asterisk(), Ref("c")], Text("x")) { ColumnsBracketed = true }),
			// FullTextSearch: a string or a variable.
			"CONTAINS searching a number" => Where(new Expression.FullTextPredicate(FullTextPredicateKind.Contains, [Ref("b")], Number("1"))),
			// SearchLanguage: digits, a binary number, a string or a variable.
			"CONTAINS in a language of a name" => Where(new Expression.FullTextPredicate(FullTextPredicateKind.Contains, [Ref("b")], Text("x")) { Language = Ref("English") }),
			// TSqlPredicate: a condition, never a value (a select list's column).
			"CONTAINS where a value stands" => new Statement.Select { Query = new Query.Select { Columns = [new SelectColumn.ExpressionItem(new Expression.FullTextPredicate(FullTextPredicateKind.Contains, [Ref("b")], Text("x")))] } },
			// DistinctTail: Quantifier & TableSubquery.
			"IS DISTINCT FROM ALL a value" => Where(new Expression.IsDistinct(Ref("a"), false, Number("1")) { Quantifier = Quantifier.All }),
			// UseModel: TSqlQualifiedName, no part left out.
			"AI_GENERATE_EMBEDDINGS a model with an empty part" => new Statement.Select { Query = new Query.Select { Columns = [new SelectColumn.ExpressionItem(new Expression.AiGenerateEmbeddings(Text("x"), new QualifiedName([Name(""), Name("m")])))] } },
			// TSqlGroupingColumn: the hint after a column, never after a list in brackets.
			"DISTRIBUTED_AGG on a list in brackets" => new Statement.Select { Query = new Query.Select { Columns = [new SelectColumn.ExpressionItem(Ref("a"))], From = new FromClause([new TableSource.Named(Qualified("t"))]), GroupBy = new GroupByClause(null, [new GroupingElement.Rollup([new GroupingElement.Ordinary([Ref("a"), Ref("b")], true) { DistributedAggregation = true }])]) } },
			// ChunksType: the bare word FIXED, '[fixed]' refused.
			"chunks of another type" => new Statement.Select { Query = new Query.Select { Columns = [new SelectColumn.All()], From = new FromClause([new TableSource.AiGenerateChunks(Text("x"), new Identifier("[fixed]", IdentifierStyle.Bracketed), Number("5"))]) } },
			// ChunksFunction: ENABLE_CHUNK_SET_ID after OVERLAP.
			"chunks with a set id and no overlap" => new Statement.Select { Query = new Query.Select { Columns = [new SelectColumn.All()], From = new FromClause([new TableSource.AiGenerateChunks(Text("x"), Name("fixed"), Number("5")) { EnableChunkSetId = Number("1") }]) } },
			// After the call, AS c (a, b) is Msg 102.
			"chunks with an alias's columns" => new Statement.Select { Query = new Query.Select { Columns = [new SelectColumn.All()], From = new FromClause([new TableSource.AiGenerateChunks(Text("x"), Name("fixed"), Number("5")) { Alias = new Alias(Name("c"), [Name("a")]) }]) } },
			// IdentityFunction: only with an INTO (Msg 177).
			"IDENTITY with no INTO" => Into(new SelectColumn.ExpressionItem(Identity(Number("1"), Num("1")), Name("id"), true), into: false),
			// TSqlSelectSublist: an IdentityFunction is always named.
			"IDENTITY unnamed" => Into(new SelectColumn.ExpressionItem(Identity(Number("1"), Num("1")))),
			// IdentityFunction: a whole item of the list and never part of a value.
			"IDENTITY inside a value" => Into(new SelectColumn.ExpressionItem(new Expression.Binary(Identity(Number("1"), Num("1")), BinaryOperator.Add, Number("1")), Name("id"), true)),
			"IDENTITY in a WHERE" => Where(new Expression.Comparison(Identity(Number("1"), Num("1")), ComparisonOperator.Equal, Number("1"))),
			// IdentityFunction: a seed and a step, both or neither.
			"IDENTITY with a seed and no increment" => Into(new SelectColumn.ExpressionItem(new Expression.Identity(new DataType.Numeric(NumericTypeKind.Int)) { Seed = Num("1") }, Name("id"), true)),
			// IdentityNumber: Lexical.UnsignedNumericLiteral, which reads no hexadecimal number.
			"IDENTITY seeded by a hex number" => Into(new SelectColumn.ExpressionItem(Identity(new LiteralValue.Numeric("0x1", NumericLiteralKind.HexInteger), Num("1")), Name("id"), true)),
			// VariableSource: '.' & TSqlIdentifier, a method's name of one part.
			"a variable's method of two parts" => From(new TableSource.Function(new Expression.Invocation(new QualifiedName([Name("s"), Name("f")]), [])) { Variable = Name("@v") }),
			// TSql.Variable: '@' first.
			"a variable's method with no @" => From(new TableSource.Function(new Expression.Invocation(Qualified("f"), [])) { Variable = Name("v") }),
			// TSqlDmlTarget: a function's call, never a variable's method.
			"a variable's method as an INSERT's target" => new Statement.Insert { Target = new TableSource.Function(new Expression.Invocation(Qualified("f"), [])) { Variable = Name("@v") } },
			// DatePart: a plain word of the table reads back as Expression.DatePart.
			"DATEPART of a date part's word as a column" => Dated(Ref("yy")),
			// DatePart: '(' & DatePart & ')' reads back as a column in brackets, not as Expression.DatePart.
			"DATEPART of a date part in brackets" => Dated(new Expression.Parenthesized(new Expression.DatePart(DatePartKind.Year))),
			// DatePart: a name of the closed list only, bracketed or not (Msg 155).
			"DATEPART of a bracketed name that is no date part" => Dated(new Expression.Reference(new QualifiedName([new Identifier("[foo]", IdentifierStyle.Bracketed)]))),
			"DATEPART of a qualified column" => Dated(new Expression.Reference(new QualifiedName([Name("t"), Name("yy")]))),
			// DatePartCall: '(' & part: DatePart, never empty.
			"DATEPART of nothing" => new Statement.Select { Query = new Query.Select { Columns = [new SelectColumn.ExpressionItem(new Expression.Invocation(Qualified("DATEPART"), []))] } },
			// FullTextColumns: PROPERTY (TSqlQualifiedName, Quoted), and nothing else of a call.
			"CONTAINS of a property of a number" => Where(new Expression.FullTextPredicate(FullTextPredicateKind.Contains, [Property(Number("1"))], Text("x"))),
			"CONTAINS of a property of one argument" => Where(new Expression.FullTextPredicate(FullTextPredicateKind.Contains, [new Expression.Invocation(Qualified("PROPERTY"), [new Argument(Ref("c"))])], Text("x"))),
			"CONTAINS of a property with DISTINCT" => Where(new Expression.FullTextPredicate(FullTextPredicateKind.Contains, [((Expression.Invocation)Property(Text("Title"))) with { Quantifier = SetQuantifier.Distinct }], Text("x"))),
			"CONTAINS of another call" => Where(new Expression.FullTextPredicate(FullTextPredicateKind.Contains, [new Expression.Invocation(Qualified("f"), [new Argument(Ref("c")), new Argument(Text("Title"))])], Text("x"))),
			// VariableSource: '.' & TSqlIdentifier & '(' & Arguments? & ')', nothing after the bracket.
			"a variable's method with OVER" => From(new TableSource.Function(new Expression.Invocation(Qualified("f"), []) { Over = new WindowReference.NameRef(Name("w")) }) { Variable = Name("@v") }),
			// DatePartCall: a date part's word reads back as one only first in DATEPART or its kin; a method's argument is a value.
			"a variable's method of a date part" => From(new TableSource.Function(new Expression.Invocation(Qualified("DATEPART"), [new Argument(new Expression.DatePart(DatePartKind.Year)), new Argument(Ref("x"))])) { Variable = Name("@v") }),
			"a date part outside a call" => Where(new Expression.Comparison(new Expression.DatePart(DatePartKind.Year), ComparisonOperator.Equal, Number("1"))),
			// TSqlQualifiedName: a part may be left out, its last never.
			"CONTAINS of a property whose last part is left out" => Where(new Expression.FullTextPredicate(FullTextPredicateKind.Contains, [new Expression.Invocation(Qualified("PROPERTY"), [new Argument(new Expression.Reference(new QualifiedName([Name("t"), Name("")]))), new Argument(Text("Title"))])], Text("x"))),
			"a number with no name" => new Statement.SaveTransaction(new TransactionName(null) { Number = (LiteralValue.Numeric)Number("5") }),
			_ => throw new ArgumentOutOfRangeException(nameof(which), which, null),
		};

		Assert.ThrowsAny<ArgumentException>(() => TransactSqlWriter.Write(node));
	}
}
