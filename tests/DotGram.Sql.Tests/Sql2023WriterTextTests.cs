using System;

using DotGram.Sql.Ast;

using Xunit;

namespace DotGram.Sql.Tests;

// As in Sql2023WriterCoverageTests: inside this namespace an unqualified name is the old tree's.
using Expression = DotGram.Sql.Ast.Expression;
using Identifier = DotGram.Sql.Ast.Identifier;
using LiteralValue = DotGram.Sql.Ast.LiteralValue;
using QualifiedName = DotGram.Sql.Ast.QualifiedName;
using Statement = DotGram.Sql.Ast.Statement;

/// <summary>
/// What <see cref="Sql2023Writer"/> prints for trees the standard's grammar does not build — T-SQL's forms
/// of FETCH, RETURN and the DROP statements — and that no two tokens it prints side by side make a
/// comment of themselves.
/// </summary>
/// <remarks>
/// The standard's parser cannot read these back, so they are asserted as text on a tree built by hand;
/// what the standard does read is held to a round trip in <see cref="SqlStandardTreeTests"/>.
/// </remarks>
public sealed class Sql2023WriterTextTests
{
	static readonly Expression A = Name("a");
	static readonly Expression B = Name("b");

	static Expression Name(string text)
	{
		return new Expression.Reference(new QualifiedName([new Identifier(text)]));
	}

	static Expression Number(string text)
	{
		return new LiteralValue.Numeric(text, NumericLiteralKind.Decimal);
	}

	static Expression Minus(Expression operand)
	{
		return new Expression.Unary(UnaryOperator.Minus, operand);
	}

	static QualifiedName Named(string text)
	{
		return new QualifiedName([new Identifier(text)]);
	}

	static RoutineDesignator Routine(RoutineKind kind, string name)
	{
		return new RoutineDesignator(kind, Named(name));
	}

	// ── FETCH ──────────────────────────────────────────────────────────────────

	[Fact]
	public void A_fetch_without_targets_writes_no_INTO()
	{
		var fetch = new Statement.FetchCursor
		{
			Orientation = new FetchOrientation(FetchOrientationKind.Next),
			FromKeyword = true,
			Cursor      = new CursorReference(Named("c")),
		};

		Assert.Equal("FETCH NEXT FROM c", Sql2023Writer.Write(fetch));
	}

	// ── RETURN ─────────────────────────────────────────────────────────────────

	[Fact]
	public void A_bare_return_writes_no_value()
	{
		Assert.Equal("RETURN", Sql2023Writer.Write(new Statement.Return()));
	}

	[Fact]
	public void A_return_of_NULL_writes_it()
	{
		Assert.Equal("RETURN NULL", Sql2023Writer.Write(new Statement.Return { NullKeyword = true }));
		Assert.Equal("RETURN NULL", Sql2023Writer.Write(new Statement.Return { Value = new LiteralValue.Null() }));
		Assert.Equal("RETURN a", Sql2023Writer.Write(new Statement.Return { Value = A }));
	}

	// ── DROP FUNCTION, DROP PROCEDURE ──────────────────────────────────────────

	[Fact]
	public void A_routine_dropped_if_it_exists_says_so_after_its_kind()
	{
		var drop = new Statement.DropRoutine { Routines = [Routine(RoutineKind.Function, "f")], IfExists = true };

		Assert.Equal("DROP FUNCTION IF EXISTS f", Sql2023Writer.Write(drop));
	}

	[Fact]
	public void A_list_of_routines_names_the_kind_once()
	{
		var drop = new Statement.DropRoutine { Routines = [Routine(RoutineKind.Procedure, "x"), Routine(RoutineKind.Procedure, "y")] };

		Assert.Equal("DROP PROCEDURE x, y", Sql2023Writer.Write(drop));
		Assert.Equal("DROP PROC IF EXISTS x, y", Sql2023Writer.Write(drop with { IfExists = true, Proc = true }));
	}

	// ── DROP TRIGGER ───────────────────────────────────────────────────────────

	[Theory]
	[InlineData(TriggerScope.Database, "DROP TRIGGER IF EXISTS tr ON DATABASE")]
	[InlineData(TriggerScope.AllServer, "DROP TRIGGER IF EXISTS tr ON ALL SERVER")]
	public void A_DDL_trigger_dropped_keeps_its_scope(TriggerScope scope, string expected)
	{
		var drop = new Statement.DropTrigger { Names = [Named("tr")], IfExists = true, On = scope };

		Assert.Equal(expected, Sql2023Writer.Write(drop));
	}

	[Fact]
	public void A_DML_trigger_dropped_has_no_scope()
	{
		Assert.Equal("DROP TRIGGER s.a, b", Sql2023Writer.Write(new Statement.DropTrigger { Names = [new QualifiedName([new Identifier("s"), new Identifier("a")]), Named("b")] }));
	}

	// ── Comment delimiters ─────────────────────────────────────────────────────

	/// <summary>
	/// A sign is written against its operand, and where the operand begins with a sign of its own the two
	/// would read as `--`, which begins a comment: `SELECT --1 FROM t` is `SELECT` and nothing else.
	/// </summary>
	[Theory]
	[MemberData(nameof(Signs))]
	public void No_two_tokens_side_by_side_open_a_comment(Expression expression, string expected)
	{
		var written = Sql2023Writer.Write(expression);

		Assert.Equal(expected, written);
		Assert.DoesNotContain("--", written, StringComparison.Ordinal);
		Assert.DoesNotContain("/*", written, StringComparison.Ordinal);
		Assert.DoesNotContain("*/", written, StringComparison.Ordinal);
	}

	public static TheoryData<Expression, string> Signs()
	{
		return new()
		{
			{ Minus(A), "-a" },
			{ Minus(Minus(A)), "- -a" },
			{ Minus(Minus(Minus(Number("1")))), "- - -1" },
			{ Minus(Number("-1")), "- -1" },
			{ new Expression.Unary(UnaryOperator.Plus, Minus(A)), "+-a" },
			{ new Expression.Binary(A, BinaryOperator.Subtract, Number("-1")), "a - -1" },
			{ new Expression.Binary(A, BinaryOperator.Subtract, Minus(Minus(B))), "a - - -b" },
			{ new Expression.Binary(A, BinaryOperator.Divide, Minus(B)), "a / -b" },
			{ new Expression.Binary(Minus(A), BinaryOperator.Multiply, Minus(B)), "-a * -b" },
			{ new Expression.AtTimeZone(A, Minus(Minus(B))), "a AT TIME ZONE - -b" },
		};
	}
}
