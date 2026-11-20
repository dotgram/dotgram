using System;
using System.Linq;

using Xunit;

namespace DotGram.QueryLanguage.Tests;

/// <summary>What a text means against the model: names, scopes, rights, types, members, aggregates and warnings.</summary>
public sealed class BinderTests
{
	static readonly QlModel<Shop> Model   = Shop.Model();
	static readonly QlModel<Shop> Without = Shop.Model(salary: false);

	static QlQueryInfo Describe(string text, QlModel<Shop>? model = null)
	{
		return (model ?? Model).Describe(text);
	}

	static QlError Refused(string text, QlModel<Shop>? model = null)
	{
		return Assert.Throws<QlException>(() => Describe(text, model)).Error;
	}

	static void Fails(string text, QlErrorCode code, int position)
	{
		var error = Refused(text);

		Assert.Equal((code, (int?)position), (error.Code, error.Position));
	}

	// ── Names and scopes ────────────────────────────────────────────────────

	[Fact]
	public void WithoutASelectARowIsTheFieldsWithSelect()
	{
		var columns = Describe("from orders").Columns.Select(c => c.Name);

		Assert.Equal(["id", "status", "total", "qty", "big", "score", "created", "shipped", "stamp", "priority", "customer", "note", "ref", "active", "flag", "day", "kind", "blob"], columns);
	}

	[Fact]
	public void AnUnknownEntityIsAnUnknownName()
	{
		Fails("from customers", QlErrorCode.UnknownName, 5);
	}

	[Fact]
	public void NamesIgnoreCase()
	{
		Assert.Equal(["ID", "Total"], Describe("FROM Orders WHERE Qty > 1 SELECT ID, Total").Columns.Select(c => c.Name));
	}

	[Fact]
	public void AfterASelectOnlyItsColumnsAreInScope()
	{
		Describe("from orders select id, total where total > 1 order by id");

		var field = Refused("from orders select id where total > 1");

		Assert.Equal((QlErrorCode.UnknownName, (int?)28, "`total` is not in the `select` at 12."), (field.Code, field.Position, field.Message));

		var alias = Refused("from orders o select id where o.id > 1");

		Assert.Equal((QlErrorCode.UnknownName, (int?)30, "`o` is not available after the `select` at 14."), (alias.Code, alias.Position, alias.Message));
	}

	[Fact]
	public void AFieldIsWrittenAloneOrAfterTheAliasOrTheEntitysName()
	{
		Describe("from orders o where o.qty > 1 && qty > 1 select o.id, total");
		Describe("from orders where orders.qty > 1 select orders.id");

		Assert.Equal(["id", "total"], Describe("from orders o select o.id, o.total").Columns.Select(c => c.Name));
	}

	[Fact]
	public void TheEntityIsNotAValue()
	{
		Fails("from orders o where o > 1", QlErrorCode.Type, 20);
		Fails("from orders o select o", QlErrorCode.Type, 21);
	}

	[Fact]
	public void AnAliasIsNotAField()
	{
		Fails("from orders total", QlErrorCode.Syntax, 12);
	}

	[Fact]
	public void ALetShadowsAFieldWhichTheAliasStillReaches()
	{
		var info = Describe("from orders o let qty = 1.5 where qty > 1 && o.qty > 1 select qty, o.qty as real");

		Assert.Equal([QlType.Decimal, QlType.Int], info.Columns.Select(c => c.Type));
	}

	[Fact]
	public void ALetIsNamedOnce()
	{
		Fails("from orders let a = 1 let a = 2", QlErrorCode.Syntax, 26);
		Fails("from orders o let o = 1",          QlErrorCode.Syntax, 18);
		Fails("from orders select id let id = 1", QlErrorCode.Syntax, 26);
		Describe("from orders select id as x let id = x");
	}

	[Fact]
	public void ASelectNamesItsColumns()
	{
		var info = Describe("from orders select id, qty as x, total * 2 as t");

		Assert.Equal(["id", "x", "t"], info.Columns.Select(c => c.Name));

		Fails("from orders select total * 2",  QlErrorCode.Syntax, 19);
		Fails("from orders select id, id",     QlErrorCode.Syntax, 23);
		Fails("from orders select qty as id, id", QlErrorCode.Syntax, 30);
	}

	[Fact]
	public void StarIsTheColumnsOfTheRowAsItStands()
	{
		Assert.Equal(18, Describe("from orders select *").Columns.Count);
		Assert.Equal(["id", "t"], Describe("from orders select id, total as t select *").Columns.Select(c => c.Name));
	}

	[Fact]
	public void ColumnsAreBounded()
	{
		var model = Shop.Model(QlOptions.Default with { MaxColumns = 5 });

		Assert.Equal(QlErrorCode.Limit, Refused("from orders", model).Code);
		Assert.Equal(QlErrorCode.Limit, Refused("from orders select *", model).Code);

		Describe("from orders select id, total", model);
	}

	// ── Rights ──────────────────────────────────────────────────────────────

	/// <summary>A field used where its rights do not reach is refused exactly as a field the model does not have.</summary>
	[Theory]
	[InlineData("from orders where note == 'x'")]
	[InlineData("from orders order by status")]
	[InlineData("from orders order by salary select id")]
	[InlineData("from orders select salary")]
	[InlineData("from orders select rank")]
	[InlineData("from orders o select o.salary")]
	[InlineData("from orders o where o.note == 'x'")]
	[InlineData("from orders let s = salary * 2 select s")]
	[InlineData("from orders let n = note where n == 'x' select id")]
	[InlineData("from orders let r = rank where r > 1 select id")]
	[InlineData("from orders select salary as s where s > 1 select s")]
	[InlineData("from orders select note as n where n == 'x' select id")]
	[InlineData("from orders select status as s order by s")]
	[InlineData("from orders select Sum(salary) as s")]
	[InlineData("from orders select Sum(rank) as s where s > 1")]
	[InlineData("from orders select Count(note) as c where c > 1")]
	[InlineData("from orders select Max(status) as m order by m")]
	[InlineData("from orders where note.Length > 1 select id")]
	[InlineData("from orders where blob == @b select id")]
	[InlineData("from orders order by blob select id")]
	[InlineData("from orders where note in ('a', 'b') select id")]
	[InlineData("from orders where rank between 1 and 2 select id")]
	[InlineData("from orders where note is null select id")]
	public void ANoRightIsAnUnknownName(string text)
	{
		var name   = new[] { "salary", "note", "status", "rank", "blob" }.First(n => text.Contains(n, StringComparison.Ordinal));
		var nowhere = text.Replace(name, "nosuch");
		var error  = Refused(text);
		var absent = Refused(nowhere, Without);

		Assert.Equal(QlErrorCode.UnknownName, error.Code);
		Assert.Equal(text.IndexOf(name, StringComparison.Ordinal), error.Position);
		Assert.Equal((absent.Code, absent.Position, absent.Message.Replace("nosuch", name)), (error.Code, error.Position, error.Message));
	}

	[Fact]
	public void TheErrorSpellsTheNameAsTheTextDoes()
	{
		Assert.Equal("Unknown name `SALARY`.", Refused("from orders select SALARY").Message);
		Assert.Equal("`o` has no field `Salary`.", Refused("from orders o select o.Salary").Message);
	}

	[Theory]
	[InlineData("from orders where salary > 1 select id")]
	[InlineData("from orders select id, salary as s where s > 1 select id")]
	[InlineData("from orders order by rank select id")]
	[InlineData("from orders let r = rank order by r select id")]
	[InlineData("from orders select Count(*) as c")]
	[InlineData("from orders select Sum(salary) as s where s > 1 select Count(*) as c")]
	public void ARightIsNeededOnlyWhereTheValueGoes(string text)
	{
		Describe(text);
	}

	[Fact]
	public void AnUnusedValueNeedsNoRightButTheFieldMustBeExposed()
	{
		Assert.Single(Describe("from orders let s = salary select id").Warnings);
		Assert.Equal(QlErrorCode.UnknownName, Refused("from orders let s = tenant select id").Code);
	}

	// ── Warnings ────────────────────────────────────────────────────────────

	[Fact]
	public void AnUnusedLetOrColumnIsAWarning()
	{
		var warnings = Describe("from orders let a = qty + 1 let b = a * 2 let c = qty select id, total as t, c where c > 1 select id").Warnings;

		Assert.Equal(["`a` is never used.", "`b` is never used.", "`t` is never used."], warnings.Select(w => w.Message));
		Assert.Equal([16, 32, 74], warnings.Select(w => w.Position));
		Assert.All(warnings, w => Assert.Equal(QlWarningCode.UnusedName, w.Code));
	}

	[Fact]
	public void AColumnPassedOnByStarIsUsed()
	{
		Assert.Empty(Describe("from orders select id, total select *").Warnings);
		Assert.Empty(Describe("from orders let a = qty select a").Warnings);
	}

	// ── Types ───────────────────────────────────────────────────────────────

	[Fact]
	public void ANumberTakesTheTypeOfTheOtherSide()
	{
		var info = Describe("from orders where total > 1 && score > 1 && big > 1 && qty > 1.5 select total + 1 as a, score * 2 as b, big - 1 as c, qty + 1.5 as d, qty / 2 as e");

		Assert.Equal([QlType.Decimal, QlType.Double, QlType.Long, QlType.Decimal, QlType.Int], info.Columns.Select(c => c.Type));
	}

	[Theory]
	[InlineData("from orders where big + score > 1",       22)]
	[InlineData("from orders where total + score > 1",     24)]
	[InlineData("from orders where big > score",           22)]
	[InlineData("from orders where status + 1 == 'x'",     25)]
	[InlineData("from orders where status > 1",            25)]
	[InlineData("from orders where active < true",         25)]
	[InlineData("from orders where ref < @g",              22)]
	[InlineData("from orders where created > stamp",       26)]
	[InlineData("from orders where qty",                   18)]
	[InlineData("from orders where !qty",                  18)]
	[InlineData("from orders where qty > 1 && total",      29)]
	[InlineData("from orders where @a == @b",              18)]
	[InlineData("from orders where qty > null",            22)]
	[InlineData("from orders select null as x",            19)]
	[InlineData("from orders select @p as x",              19)]
	[InlineData("from orders where qty == @p && status == @p", 41)]
	[InlineData("from orders where qty in (1, 'a')",       29)]
	[InlineData("from orders where qty in (1, total)",     29)]
	[InlineData("from orders where -status == 'a'",        18)]
	public void ValuesThatDoNotMeetAreATypeError(string text, int position)
	{
		Fails(text, QlErrorCode.Type, position);
	}

	[Fact]
	public void NullComparesAsCSharpDoes()
	{
		Describe("from orders where status == null || priority != null || qty == null || status is null || priority is not null select id");
	}

	[Fact]
	public void StringsOrderAndCompare()
	{
		Describe("from orders where status > 'a' && 'b' <= status && status == \"x\" select id");
	}

	[Fact]
	public void ConditionsMayBeNullable()
	{
		Describe("from orders where flag && active || !flag select id");
	}

	// ── Literals ────────────────────────────────────────────────────────────

	[Theory]
	[InlineData("'it\\'s'",     "it's")]
	[InlineData("\"say \\\"x\\\"\"", "say \"x\"")]
	[InlineData("'a\\\\b'",     "a\\b")]
	[InlineData("'\\u0041\\u00e9'", "Aé")]
	[InlineData("\"it's\"",     "it's")]
	public void FourEscapes(string literal, string value)
	{
		var bound = Model.Bind("from orders where status == " + literal + " select id");
		var where = (BoundWhere)bound.Clauses[0];
		var right = (BoundBinary)where.Condition;

		Assert.Equal(value, ((BoundLiteral)right.Right).Value);
	}

	[Theory]
	[InlineData("'C:\\temp'",   31)]
	[InlineData("'a\\n'",       30)]
	[InlineData("'\\u12'",      29)]
	public void AnyOtherEscapeIsRefusedWhereItIs(string literal, int position)
	{
		Fails("from orders where status == " + literal + " select id", QlErrorCode.Syntax, position);
	}

	[Fact]
	public void AStringIsBounded()
	{
		var model = Shop.Model(QlOptions.Default with { MaxStringLength = 3 });

		model.Describe("from orders where status == 'abc' select id");

		Assert.Equal(QlErrorCode.Limit, Assert.Throws<QlException>(() => model.Describe("from orders where status == 'abcd' select id")).Error.Code);
	}

	[Fact]
	public void TimestampsAreWrittenWithTheirType()
	{
		var bound = Model.Bind("from orders where created > timestamp '2026-01-02 03:04:05' && stamp > timestamptz '2026-01-02T03:04:05+02:00' && created < timestamp '2026-02-01T00:00:00Z' select id");

		Assert.Empty(bound.Parameters);

		Fails("from orders where created > timestamp '2026-01-02 03:04:05+02:00'", QlErrorCode.Type, 38);
		Fails("from orders where stamp > timestamptz '2026-01-02 03:04:05'",        QlErrorCode.Type, 38);
		Fails("from orders where created > date '2026-01-02'",                      QlErrorCode.Type, 28);
	}

	[Fact]
	public void ArithmeticThatReadsNoRowIsComputedWhenTheTextIsBound()
	{
		var bound = Model.Bind("from orders where qty > 2 * 3 + 1 select id");
		var right = ((BoundBinary)((BoundWhere)bound.Clauses[0]).Condition).Right;

		Assert.Equal(7, ((BoundLiteral)right).Value);

		Fails("from orders where qty > 1 / 0 select id",               QlErrorCode.DivisionByZero, 26);
		Fails("from orders where qty > 2147483647 + 1 select id",      QlErrorCode.Overflow,       35);
		Fails("from orders where total > 1.5 % 0 select id",           QlErrorCode.DivisionByZero, 30);
		Fails("from orders where qty > 99999999999999999999999999999999 select id", QlErrorCode.Overflow, 24);

		Assert.Equal(-2147483648L, ((BoundLiteral)((BoundBinary)((BoundWhere)Model.Bind("from orders where big > -2147483648 select id").Clauses[0]).Condition).Right).Value);
	}

	// ── Parameters ──────────────────────────────────────────────────────────

	[Fact]
	public void AParameterIsTypedWhereItStands()
	{
		var info = Describe("from orders where total > @min && qty in @qs && status == @s && day == @d && customer.StartsWith('x') == false || id == 1 skip @skip take @take select id");
		var parameters = info.Parameters.ToDictionary(p => p.Name);

		Assert.Equal((QlType.Decimal, typeof(decimal),  false, true),  Shape(parameters["min"]));
		Assert.Equal((QlType.Int,     typeof(int),      true,  false), Shape(parameters["qs"]));
		Assert.Equal((QlType.String,  typeof(string),   false, true),  Shape(parameters["s"]));
		Assert.Equal((QlType.Other,   typeof(DateOnly), false, true),  Shape(parameters["d"]));
		Assert.Equal((QlType.Int,     typeof(int),      false, false), Shape(parameters["skip"]));
		Assert.Equal(["min", "qs", "s", "d", "skip", "take"], info.Parameters.Select(p => p.Name));

		static (QlType, Type, bool, bool) Shape(QlParameterInfo parameter)
		{
			return (parameter.Type, parameter.ClrType, parameter.IsList, parameter.IsNullable);
		}
	}

	[Fact]
	public void AParameterInArithmeticOrAMemberIsNeverNull()
	{
		var info = Describe("from orders where total > @k * 2 && customer.StartsWith(@p) select id");

		Assert.All(info.Parameters, p => Assert.False(p.IsNullable));
	}

	[Fact]
	public void OneParameterHasOneType()
	{
		Describe("from orders where qty > @n && priority < @n select id");
		Fails("from orders where status == @n take @n select id", QlErrorCode.Type, 36);
	}

	// ── Members ─────────────────────────────────────────────────────────────

	[Fact]
	public void MembersAreTheClosedTable()
	{
		var info = Describe("from orders select status.Length as l, status.ToUpper().Trim() as u, created.Year as y, shipped.Month as m, stamp.Date as d, customer.Contains('1') as c");

		Assert.Equal(
			[(QlType.Int, true), (QlType.String, true), (QlType.Int, false), (QlType.Int, true), (QlType.Timestamp, false), (QlType.Bool, false)],
			info.Columns.Select(c => (c.Type, c.IsNullable)));
	}

	[Theory]
	[InlineData("from orders where status.Size > 1",            25, QlErrorCode.UnknownName)]
	[InlineData("from orders where qty.Length > 1",             22, QlErrorCode.UnknownName)]
	[InlineData("from orders where status.Length() > 1",        25, QlErrorCode.Type)]
	[InlineData("from orders where status.StartsWith > 1",      25, QlErrorCode.Type)]
	[InlineData("from orders where status.StartsWith(1)",       36, QlErrorCode.Type)]
	[InlineData("from orders where status.StartsWith('a', 'b')", 25, QlErrorCode.Type)]
	[InlineData("from orders where 'abc'.Length > 1",           24, QlErrorCode.Type)]
	[InlineData("from orders where @p.ToUpper() == status",     21, QlErrorCode.Type)]
	[InlineData("from orders where day.Year > 1",               22, QlErrorCode.UnknownName)]
	[InlineData("from orders where Lower(status) == 'a'",       18, QlErrorCode.UnknownName)]
	public void AnythingElseIsRefused(string text, int position, QlErrorCode code)
	{
		Fails(text, code, position);
	}

	[Fact]
	public void MembersIgnoreCase()
	{
		Describe("from orders where status.startswith('a') && CREATED.year > 1 select id");
	}

	// ── Fields outside the language's types ─────────────────────────────────

	[Fact]
	public void AFieldOutsideTheTypesComparesAsItsOwnOperatorsSay()
	{
		var info = Describe("from orders where day == @d && day < @d && kind == @k && kind in @ks && day is not null order by kind, day select day, kind, blob");

		Assert.Equal([typeof(DateOnly), typeof(Kind), typeof(byte[])], info.Columns.Select(c => c.ClrType));
		Assert.All(info.Columns, c => Assert.Equal(QlType.Other, c.Type));

		var error = Refused("from orders where kind < @k select id");

		Assert.Equal((QlErrorCode.Type, "Operator `<` is not defined for `Kind`."), (error.Code, error.Message));

		Fails("from orders where day == '2026-01-01' select id", QlErrorCode.Type, 22);
		Fails("from orders where day == kind select id",         QlErrorCode.Type, 22);
	}

	// ── Aggregates ──────────────────────────────────────────────────────────

	[Fact]
	public void AggregatesAreColumnsOfASelect()
	{
		var info = Describe("from orders select Count(*), Count(priority) as p, Sum(qty) as s, Avg(qty) as a, Avg(total) as t, Min(status) as mn, Max(day) as mx, Sum(score) as ss");

		Assert.Equal(["Count", "p", "s", "a", "t", "mn", "mx", "ss"], info.Columns.Select(c => c.Name));

		Assert.Equal(
			[(QlType.Int, false), (QlType.Int, false), (QlType.Int, true), (QlType.Double, true), (QlType.Decimal, true), (QlType.String, true), (QlType.Other, true), (QlType.Double, true)],
			info.Columns.Select(c => (c.Type, c.IsNullable)));
	}

	[Theory]
	[InlineData("from orders select Count(*), id",           29, QlErrorCode.Type)]
	[InlineData("from orders select id, Count(*)",           19, QlErrorCode.Type)]
	[InlineData("from orders where Count(*) > 1",            18, QlErrorCode.Type)]
	[InlineData("from orders select Count(*) + 1 as c",      19, QlErrorCode.Type)]
	[InlineData("from orders select Sum(status) as s",       23, QlErrorCode.Type)]
	[InlineData("from orders select Avg(day) as a",          23, QlErrorCode.Type)]
	[InlineData("from orders select Sum(*) as s",            19, QlErrorCode.Type)]
	[InlineData("from orders select Count(id, qty) as c",    19, QlErrorCode.Type)]
	[InlineData("from orders select Sum(1) as s",            23, QlErrorCode.Type)]
	[InlineData("from orders select Sum(Count(*)) as s",     23, QlErrorCode.Type)]
	[InlineData("from orders select Count(*), Count(id)",    29, QlErrorCode.Syntax)]
	public void AggregatesStandAlone(string text, int position, QlErrorCode code)
	{
		Fails(text, code, position);
	}

	[Fact]
	public void AnAggregateRowGoesOnLikeAnyOther()
	{
		var info = Describe("from orders where qty > 1 select Count(*) as c, Sum(total) as s where c > 1 select s / c as average");

		Assert.Equal((QlType.Decimal, true), (info.Columns[0].Type, info.Columns[0].IsNullable));
	}

	// ── Bounds the binding knows ────────────────────────────────────────────

	[Fact]
	public void PagingLevelsAreBounded()
	{
		var model = Shop.Model(QlOptions.Default with { MaxPagingLevels = 2 });

		model.Describe("from orders take 10 where qty > 1 skip 1 take 5 order by id take 3 skip 1 select id");

		var error = Assert.Throws<QlException>(() => model.Describe("from orders take 10 where qty > 1 skip 1 select id take 5 order by id take 3 where id > 1")).Error;

		Assert.Equal((QlErrorCode.Limit, (int?)70), (error.Code, error.Position));
	}

	[Fact]
	public void ACountIsAnInt()
	{
		Fails("from orders take 2147483648", QlErrorCode.Limit, 17);
	}

	[Fact]
	public void ARowHoldsAtMostThirtyTwoValuesOfAKind()
	{
		var lets = string.Join(" ", Enumerable.Range(0, 32).Select(i => $"let v{i} = qty + {i}"));

		var model = Shop.Model(QlOptions.Default with { MaxNodes = 1000 });

		model.Describe("from orders " + lets + " select id");

		Assert.Equal(QlErrorCode.Limit, Refused("from orders " + lets + " let w = qty select id", model).Code);
	}

	[Fact]
	public void BoundTextsAreKeptUpToABound()
	{
		var model = Shop.Model();

		for (var i = 0; i < QlModel<Shop>.CacheSize * 2 + 10; i++)
			model.Describe($"from orders where qty > {i} select id");

		Assert.InRange(model.CacheCount, 1, QlModel<Shop>.CacheSize);
	}

	[Fact]
	public void ABoundTextIsKeptByItsExactText()
	{
		var model = Shop.Model();

		Assert.Same(model.Describe("from orders select id"), model.Describe("from orders select id"));
		Assert.NotSame(model.Describe("from orders select id"), model.Describe("from orders  select id"));
	}
}
