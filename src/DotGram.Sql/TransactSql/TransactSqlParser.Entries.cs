using System;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.TransactSql;

// The public entries: each reads through the internal entry of the same rule, which builds the T-SQL tree
// (TransactSql.gram, `ParseSyntax…`), and hands back what it read carried onto the common tree, DotGram.Sql.Ast
// (TransactSql/Bridge). A refusal is the internal entry's own, at the same place with the same message. The same
// methods twice, for the plain reader and the located one, at every level the grammar publishes. They go when the
// grammar's rules build the common tree themselves and publish it under these names.
public abstract partial class TransactSqlParser
{
	/// <summary>Reads the whole input as a query statement (no other kind).</summary>
	/// <exception cref="FormatException">The input is not read. <c>TryParseSelect</c> answers instead.</exception>
	public static Ast.Statement ParseSelect(string input)
	{
		return Entries.Parsed(TryParseSelect(input));
	}

	/// <summary>Reads the whole input as a query statement (no other kind), answering rather than throwing.</summary>
	public static Match<Ast.Statement> TryParseSelect(string input)
	{
		return Entries.Carry(TryParseSyntaxSelect(input), input, Located.TryParseSyntaxSelect);
	}

	/// <summary>Reads the whole input as a query statement (no other kind), answering only whether it is read.</summary>
	public static bool TryParseSelect(string input, out Ast.Statement value)
	{
		return Entries.Carry(TryParseSyntaxSelect(input, out var old), old, input, 0, input.Length, Located.TryParseSyntaxSelect, out value);
	}

	/// <summary>Reads a query statement (no other kind) from a position, not looking past what it reads.</summary>
	public static Match<Ast.Statement> TryParseSelect(string input, int at)
	{
		return Entries.Carry(TryParseSyntaxSelect(input, at), input, Located.TryParseSyntaxSelect);
	}

	/// <summary>Reads a query statement (no other kind) from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseSelect(string input, ref int at, out Ast.Statement value)
	{
		var from = at;

		return Entries.Carry(TryParseSyntaxSelect(input, ref at, out var old), old, input, from, at - from, Located.TryParseSyntaxSelect, out value);
	}

	/// <summary>Reads a query statement (no other kind) in a window of the input.</summary>
	public static Match<Ast.Statement> TryParseSelect(string input, int at, int length)
	{
		return Entries.Carry(TryParseSyntaxSelect(input, at, length), input, Located.TryParseSyntaxSelect);
	}

	/// <summary>Reads a query statement (no other kind) in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseSelect(string input, ref int at, int length, out Ast.Statement value)
	{
		var from = at;

		return Entries.Carry(TryParseSyntaxSelect(input, ref at, length, out var old), old, input, from, at - from, Located.TryParseSyntaxSelect, out value);
	}

	/// <summary>Reads the whole input as a search condition.</summary>
	/// <exception cref="FormatException">The input is not read. <c>TryParseSearchCondition</c> answers instead.</exception>
	public static Ast.Expression ParseSearchCondition(string input)
	{
		return Entries.Parsed(TryParseSearchCondition(input));
	}

	/// <summary>Reads the whole input as a search condition, answering rather than throwing.</summary>
	public static Match<Ast.Expression> TryParseSearchCondition(string input)
	{
		return Entries.Carry(TryParseSyntaxCondition(input), input, Located.TryParseSyntaxCondition);
	}

	/// <summary>Reads the whole input as a search condition, answering only whether it is read.</summary>
	public static bool TryParseSearchCondition(string input, out Ast.Expression value)
	{
		return Entries.Carry(TryParseSyntaxCondition(input, out var old), old, input, 0, input.Length, Located.TryParseSyntaxCondition, out value);
	}

	/// <summary>Reads a search condition from a position, not looking past what it reads.</summary>
	public static Match<Ast.Expression> TryParseSearchCondition(string input, int at)
	{
		return Entries.Carry(TryParseSyntaxCondition(input, at), input, Located.TryParseSyntaxCondition);
	}

	/// <summary>Reads a search condition from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseSearchCondition(string input, ref int at, out Ast.Expression value)
	{
		var from = at;

		return Entries.Carry(TryParseSyntaxCondition(input, ref at, out var old), old, input, from, at - from, Located.TryParseSyntaxCondition, out value);
	}

	/// <summary>Reads a search condition in a window of the input.</summary>
	public static Match<Ast.Expression> TryParseSearchCondition(string input, int at, int length)
	{
		return Entries.Carry(TryParseSyntaxCondition(input, at, length), input, Located.TryParseSyntaxCondition);
	}

	/// <summary>Reads a search condition in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseSearchCondition(string input, ref int at, int length, out Ast.Expression value)
	{
		var from = at;

		return Entries.Carry(TryParseSyntaxCondition(input, ref at, length, out var old), old, input, from, at - from, Located.TryParseSyntaxCondition, out value);
	}

	/// <summary>Reads the whole input as an expression.</summary>
	/// <exception cref="FormatException">The input is not read. <c>TryParseExpression</c> answers instead.</exception>
	public static Ast.Expression ParseExpression(string input)
	{
		return Entries.Parsed(TryParseExpression(input));
	}

	/// <summary>Reads the whole input as an expression, answering rather than throwing.</summary>
	public static Match<Ast.Expression> TryParseExpression(string input)
	{
		return Entries.Carry(TryParseSyntaxExpression(input), input, Located.TryParseSyntaxExpression);
	}

	/// <summary>Reads the whole input as an expression, answering only whether it is read.</summary>
	public static bool TryParseExpression(string input, out Ast.Expression value)
	{
		return Entries.Carry(TryParseSyntaxExpression(input, out var old), old, input, 0, input.Length, Located.TryParseSyntaxExpression, out value);
	}

	/// <summary>Reads an expression from a position, not looking past what it reads.</summary>
	public static Match<Ast.Expression> TryParseExpression(string input, int at)
	{
		return Entries.Carry(TryParseSyntaxExpression(input, at), input, Located.TryParseSyntaxExpression);
	}

	/// <summary>Reads an expression from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseExpression(string input, ref int at, out Ast.Expression value)
	{
		var from = at;

		return Entries.Carry(TryParseSyntaxExpression(input, ref at, out var old), old, input, from, at - from, Located.TryParseSyntaxExpression, out value);
	}

	/// <summary>Reads an expression in a window of the input.</summary>
	public static Match<Ast.Expression> TryParseExpression(string input, int at, int length)
	{
		return Entries.Carry(TryParseSyntaxExpression(input, at, length), input, Located.TryParseSyntaxExpression);
	}

	/// <summary>Reads an expression in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseExpression(string input, ref int at, int length, out Ast.Expression value)
	{
		var from = at;

		return Entries.Carry(TryParseSyntaxExpression(input, ref at, length, out var old), old, input, from, at - from, Located.TryParseSyntaxExpression, out value);
	}

	/// <summary>Reads the whole input as one statement.</summary>
	/// <exception cref="FormatException">The input is not read. <c>TryParseStatement</c> answers instead.</exception>
	public static Ast.Statement ParseStatement(string input)
	{
		return Entries.Parsed(TryParseStatement(input));
	}

	/// <summary>Reads the whole input as one statement, answering rather than throwing.</summary>
	public static Match<Ast.Statement> TryParseStatement(string input)
	{
		return Entries.Carry(TryParseSyntaxStatement(input), input, Located.TryParseSyntaxStatement);
	}

	/// <summary>Reads the whole input as one statement, answering only whether it is read.</summary>
	public static bool TryParseStatement(string input, out Ast.Statement value)
	{
		return Entries.Carry(TryParseSyntaxStatement(input, out var old), old, input, 0, input.Length, Located.TryParseSyntaxStatement, out value);
	}

	/// <summary>Reads one statement from a position, not looking past what it reads.</summary>
	public static Match<Ast.Statement> TryParseStatement(string input, int at)
	{
		return Entries.Carry(TryParseSyntaxStatement(input, at), input, Located.TryParseSyntaxStatement);
	}

	/// <summary>Reads one statement from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseStatement(string input, ref int at, out Ast.Statement value)
	{
		var from = at;

		return Entries.Carry(TryParseSyntaxStatement(input, ref at, out var old), old, input, from, at - from, Located.TryParseSyntaxStatement, out value);
	}

	/// <summary>Reads one statement in a window of the input.</summary>
	public static Match<Ast.Statement> TryParseStatement(string input, int at, int length)
	{
		return Entries.Carry(TryParseSyntaxStatement(input, at, length), input, Located.TryParseSyntaxStatement);
	}

	/// <summary>Reads one statement in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseStatement(string input, ref int at, int length, out Ast.Statement value)
	{
		var from = at;

		return Entries.Carry(TryParseSyntaxStatement(input, ref at, length, out var old), old, input, from, at - from, Located.TryParseSyntaxStatement, out value);
	}

	/// <summary>Reads the whole input as one statement at compatibility level 100.</summary>
	/// <exception cref="FormatException">The input is not read. <c>TryParseStatement100</c> answers instead.</exception>
	public static Ast.Statement ParseStatement100(string input)
	{
		return Entries.Parsed(TryParseStatement100(input));
	}

	/// <summary>Reads the whole input as one statement at compatibility level 100, answering rather than throwing.</summary>
	public static Match<Ast.Statement> TryParseStatement100(string input)
	{
		return Entries.Carry(TryParseSyntaxStatement100(input), input, Located.TryParseSyntaxStatement100);
	}

	/// <summary>Reads the whole input as one statement at compatibility level 100, answering only whether it is read.</summary>
	public static bool TryParseStatement100(string input, out Ast.Statement value)
	{
		return Entries.Carry(TryParseSyntaxStatement100(input, out var old), old, input, 0, input.Length, Located.TryParseSyntaxStatement100, out value);
	}

	/// <summary>Reads one statement at compatibility level 100 from a position, not looking past what it reads.</summary>
	public static Match<Ast.Statement> TryParseStatement100(string input, int at)
	{
		return Entries.Carry(TryParseSyntaxStatement100(input, at), input, Located.TryParseSyntaxStatement100);
	}

	/// <summary>Reads one statement at compatibility level 100 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseStatement100(string input, ref int at, out Ast.Statement value)
	{
		var from = at;

		return Entries.Carry(TryParseSyntaxStatement100(input, ref at, out var old), old, input, from, at - from, Located.TryParseSyntaxStatement100, out value);
	}

	/// <summary>Reads one statement at compatibility level 100 in a window of the input.</summary>
	public static Match<Ast.Statement> TryParseStatement100(string input, int at, int length)
	{
		return Entries.Carry(TryParseSyntaxStatement100(input, at, length), input, Located.TryParseSyntaxStatement100);
	}

	/// <summary>Reads one statement at compatibility level 100 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseStatement100(string input, ref int at, int length, out Ast.Statement value)
	{
		var from = at;

		return Entries.Carry(TryParseSyntaxStatement100(input, ref at, length, out var old), old, input, from, at - from, Located.TryParseSyntaxStatement100, out value);
	}

	/// <summary>Reads the whole input as one statement at compatibility level 110.</summary>
	/// <exception cref="FormatException">The input is not read. <c>TryParseStatement110</c> answers instead.</exception>
	public static Ast.Statement ParseStatement110(string input)
	{
		return Entries.Parsed(TryParseStatement110(input));
	}

	/// <summary>Reads the whole input as one statement at compatibility level 110, answering rather than throwing.</summary>
	public static Match<Ast.Statement> TryParseStatement110(string input)
	{
		return Entries.Carry(TryParseSyntaxStatement110(input), input, Located.TryParseSyntaxStatement110);
	}

	/// <summary>Reads the whole input as one statement at compatibility level 110, answering only whether it is read.</summary>
	public static bool TryParseStatement110(string input, out Ast.Statement value)
	{
		return Entries.Carry(TryParseSyntaxStatement110(input, out var old), old, input, 0, input.Length, Located.TryParseSyntaxStatement110, out value);
	}

	/// <summary>Reads one statement at compatibility level 110 from a position, not looking past what it reads.</summary>
	public static Match<Ast.Statement> TryParseStatement110(string input, int at)
	{
		return Entries.Carry(TryParseSyntaxStatement110(input, at), input, Located.TryParseSyntaxStatement110);
	}

	/// <summary>Reads one statement at compatibility level 110 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseStatement110(string input, ref int at, out Ast.Statement value)
	{
		var from = at;

		return Entries.Carry(TryParseSyntaxStatement110(input, ref at, out var old), old, input, from, at - from, Located.TryParseSyntaxStatement110, out value);
	}

	/// <summary>Reads one statement at compatibility level 110 in a window of the input.</summary>
	public static Match<Ast.Statement> TryParseStatement110(string input, int at, int length)
	{
		return Entries.Carry(TryParseSyntaxStatement110(input, at, length), input, Located.TryParseSyntaxStatement110);
	}

	/// <summary>Reads one statement at compatibility level 110 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseStatement110(string input, ref int at, int length, out Ast.Statement value)
	{
		var from = at;

		return Entries.Carry(TryParseSyntaxStatement110(input, ref at, length, out var old), old, input, from, at - from, Located.TryParseSyntaxStatement110, out value);
	}

	/// <summary>Reads the whole input as one statement at compatibility level 120.</summary>
	/// <exception cref="FormatException">The input is not read. <c>TryParseStatement120</c> answers instead.</exception>
	public static Ast.Statement ParseStatement120(string input)
	{
		return Entries.Parsed(TryParseStatement120(input));
	}

	/// <summary>Reads the whole input as one statement at compatibility level 120, answering rather than throwing.</summary>
	public static Match<Ast.Statement> TryParseStatement120(string input)
	{
		return Entries.Carry(TryParseSyntaxStatement120(input), input, Located.TryParseSyntaxStatement120);
	}

	/// <summary>Reads the whole input as one statement at compatibility level 120, answering only whether it is read.</summary>
	public static bool TryParseStatement120(string input, out Ast.Statement value)
	{
		return Entries.Carry(TryParseSyntaxStatement120(input, out var old), old, input, 0, input.Length, Located.TryParseSyntaxStatement120, out value);
	}

	/// <summary>Reads one statement at compatibility level 120 from a position, not looking past what it reads.</summary>
	public static Match<Ast.Statement> TryParseStatement120(string input, int at)
	{
		return Entries.Carry(TryParseSyntaxStatement120(input, at), input, Located.TryParseSyntaxStatement120);
	}

	/// <summary>Reads one statement at compatibility level 120 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseStatement120(string input, ref int at, out Ast.Statement value)
	{
		var from = at;

		return Entries.Carry(TryParseSyntaxStatement120(input, ref at, out var old), old, input, from, at - from, Located.TryParseSyntaxStatement120, out value);
	}

	/// <summary>Reads one statement at compatibility level 120 in a window of the input.</summary>
	public static Match<Ast.Statement> TryParseStatement120(string input, int at, int length)
	{
		return Entries.Carry(TryParseSyntaxStatement120(input, at, length), input, Located.TryParseSyntaxStatement120);
	}

	/// <summary>Reads one statement at compatibility level 120 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseStatement120(string input, ref int at, int length, out Ast.Statement value)
	{
		var from = at;

		return Entries.Carry(TryParseSyntaxStatement120(input, ref at, length, out var old), old, input, from, at - from, Located.TryParseSyntaxStatement120, out value);
	}

	/// <summary>Reads the whole input as one statement at compatibility level 130.</summary>
	/// <exception cref="FormatException">The input is not read. <c>TryParseStatement130</c> answers instead.</exception>
	public static Ast.Statement ParseStatement130(string input)
	{
		return Entries.Parsed(TryParseStatement130(input));
	}

	/// <summary>Reads the whole input as one statement at compatibility level 130, answering rather than throwing.</summary>
	public static Match<Ast.Statement> TryParseStatement130(string input)
	{
		return Entries.Carry(TryParseSyntaxStatement130(input), input, Located.TryParseSyntaxStatement130);
	}

	/// <summary>Reads the whole input as one statement at compatibility level 130, answering only whether it is read.</summary>
	public static bool TryParseStatement130(string input, out Ast.Statement value)
	{
		return Entries.Carry(TryParseSyntaxStatement130(input, out var old), old, input, 0, input.Length, Located.TryParseSyntaxStatement130, out value);
	}

	/// <summary>Reads one statement at compatibility level 130 from a position, not looking past what it reads.</summary>
	public static Match<Ast.Statement> TryParseStatement130(string input, int at)
	{
		return Entries.Carry(TryParseSyntaxStatement130(input, at), input, Located.TryParseSyntaxStatement130);
	}

	/// <summary>Reads one statement at compatibility level 130 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseStatement130(string input, ref int at, out Ast.Statement value)
	{
		var from = at;

		return Entries.Carry(TryParseSyntaxStatement130(input, ref at, out var old), old, input, from, at - from, Located.TryParseSyntaxStatement130, out value);
	}

	/// <summary>Reads one statement at compatibility level 130 in a window of the input.</summary>
	public static Match<Ast.Statement> TryParseStatement130(string input, int at, int length)
	{
		return Entries.Carry(TryParseSyntaxStatement130(input, at, length), input, Located.TryParseSyntaxStatement130);
	}

	/// <summary>Reads one statement at compatibility level 130 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseStatement130(string input, ref int at, int length, out Ast.Statement value)
	{
		var from = at;

		return Entries.Carry(TryParseSyntaxStatement130(input, ref at, length, out var old), old, input, from, at - from, Located.TryParseSyntaxStatement130, out value);
	}

	/// <summary>Reads the whole input as one statement at compatibility level 140.</summary>
	/// <exception cref="FormatException">The input is not read. <c>TryParseStatement140</c> answers instead.</exception>
	public static Ast.Statement ParseStatement140(string input)
	{
		return Entries.Parsed(TryParseStatement140(input));
	}

	/// <summary>Reads the whole input as one statement at compatibility level 140, answering rather than throwing.</summary>
	public static Match<Ast.Statement> TryParseStatement140(string input)
	{
		return Entries.Carry(TryParseSyntaxStatement140(input), input, Located.TryParseSyntaxStatement140);
	}

	/// <summary>Reads the whole input as one statement at compatibility level 140, answering only whether it is read.</summary>
	public static bool TryParseStatement140(string input, out Ast.Statement value)
	{
		return Entries.Carry(TryParseSyntaxStatement140(input, out var old), old, input, 0, input.Length, Located.TryParseSyntaxStatement140, out value);
	}

	/// <summary>Reads one statement at compatibility level 140 from a position, not looking past what it reads.</summary>
	public static Match<Ast.Statement> TryParseStatement140(string input, int at)
	{
		return Entries.Carry(TryParseSyntaxStatement140(input, at), input, Located.TryParseSyntaxStatement140);
	}

	/// <summary>Reads one statement at compatibility level 140 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseStatement140(string input, ref int at, out Ast.Statement value)
	{
		var from = at;

		return Entries.Carry(TryParseSyntaxStatement140(input, ref at, out var old), old, input, from, at - from, Located.TryParseSyntaxStatement140, out value);
	}

	/// <summary>Reads one statement at compatibility level 140 in a window of the input.</summary>
	public static Match<Ast.Statement> TryParseStatement140(string input, int at, int length)
	{
		return Entries.Carry(TryParseSyntaxStatement140(input, at, length), input, Located.TryParseSyntaxStatement140);
	}

	/// <summary>Reads one statement at compatibility level 140 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseStatement140(string input, ref int at, int length, out Ast.Statement value)
	{
		var from = at;

		return Entries.Carry(TryParseSyntaxStatement140(input, ref at, length, out var old), old, input, from, at - from, Located.TryParseSyntaxStatement140, out value);
	}

	/// <summary>Reads the whole input as one statement at compatibility level 150.</summary>
	/// <exception cref="FormatException">The input is not read. <c>TryParseStatement150</c> answers instead.</exception>
	public static Ast.Statement ParseStatement150(string input)
	{
		return Entries.Parsed(TryParseStatement150(input));
	}

	/// <summary>Reads the whole input as one statement at compatibility level 150, answering rather than throwing.</summary>
	public static Match<Ast.Statement> TryParseStatement150(string input)
	{
		return Entries.Carry(TryParseSyntaxStatement150(input), input, Located.TryParseSyntaxStatement150);
	}

	/// <summary>Reads the whole input as one statement at compatibility level 150, answering only whether it is read.</summary>
	public static bool TryParseStatement150(string input, out Ast.Statement value)
	{
		return Entries.Carry(TryParseSyntaxStatement150(input, out var old), old, input, 0, input.Length, Located.TryParseSyntaxStatement150, out value);
	}

	/// <summary>Reads one statement at compatibility level 150 from a position, not looking past what it reads.</summary>
	public static Match<Ast.Statement> TryParseStatement150(string input, int at)
	{
		return Entries.Carry(TryParseSyntaxStatement150(input, at), input, Located.TryParseSyntaxStatement150);
	}

	/// <summary>Reads one statement at compatibility level 150 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseStatement150(string input, ref int at, out Ast.Statement value)
	{
		var from = at;

		return Entries.Carry(TryParseSyntaxStatement150(input, ref at, out var old), old, input, from, at - from, Located.TryParseSyntaxStatement150, out value);
	}

	/// <summary>Reads one statement at compatibility level 150 in a window of the input.</summary>
	public static Match<Ast.Statement> TryParseStatement150(string input, int at, int length)
	{
		return Entries.Carry(TryParseSyntaxStatement150(input, at, length), input, Located.TryParseSyntaxStatement150);
	}

	/// <summary>Reads one statement at compatibility level 150 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseStatement150(string input, ref int at, int length, out Ast.Statement value)
	{
		var from = at;

		return Entries.Carry(TryParseSyntaxStatement150(input, ref at, length, out var old), old, input, from, at - from, Located.TryParseSyntaxStatement150, out value);
	}

	/// <summary>Reads the whole input as one statement at compatibility level 160.</summary>
	/// <exception cref="FormatException">The input is not read. <c>TryParseStatement160</c> answers instead.</exception>
	public static Ast.Statement ParseStatement160(string input)
	{
		return Entries.Parsed(TryParseStatement160(input));
	}

	/// <summary>Reads the whole input as one statement at compatibility level 160, answering rather than throwing.</summary>
	public static Match<Ast.Statement> TryParseStatement160(string input)
	{
		return Entries.Carry(TryParseSyntaxStatement160(input), input, Located.TryParseSyntaxStatement160);
	}

	/// <summary>Reads the whole input as one statement at compatibility level 160, answering only whether it is read.</summary>
	public static bool TryParseStatement160(string input, out Ast.Statement value)
	{
		return Entries.Carry(TryParseSyntaxStatement160(input, out var old), old, input, 0, input.Length, Located.TryParseSyntaxStatement160, out value);
	}

	/// <summary>Reads one statement at compatibility level 160 from a position, not looking past what it reads.</summary>
	public static Match<Ast.Statement> TryParseStatement160(string input, int at)
	{
		return Entries.Carry(TryParseSyntaxStatement160(input, at), input, Located.TryParseSyntaxStatement160);
	}

	/// <summary>Reads one statement at compatibility level 160 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseStatement160(string input, ref int at, out Ast.Statement value)
	{
		var from = at;

		return Entries.Carry(TryParseSyntaxStatement160(input, ref at, out var old), old, input, from, at - from, Located.TryParseSyntaxStatement160, out value);
	}

	/// <summary>Reads one statement at compatibility level 160 in a window of the input.</summary>
	public static Match<Ast.Statement> TryParseStatement160(string input, int at, int length)
	{
		return Entries.Carry(TryParseSyntaxStatement160(input, at, length), input, Located.TryParseSyntaxStatement160);
	}

	/// <summary>Reads one statement at compatibility level 160 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseStatement160(string input, ref int at, int length, out Ast.Statement value)
	{
		var from = at;

		return Entries.Carry(TryParseSyntaxStatement160(input, ref at, length, out var old), old, input, from, at - from, Located.TryParseSyntaxStatement160, out value);
	}

	/// <summary>Reads the whole input as one statement at compatibility level 170.</summary>
	/// <exception cref="FormatException">The input is not read. <c>TryParseStatement170</c> answers instead.</exception>
	public static Ast.Statement ParseStatement170(string input)
	{
		return Entries.Parsed(TryParseStatement170(input));
	}

	/// <summary>Reads the whole input as one statement at compatibility level 170, answering rather than throwing.</summary>
	public static Match<Ast.Statement> TryParseStatement170(string input)
	{
		return Entries.Carry(TryParseSyntaxStatement170(input), input, Located.TryParseSyntaxStatement170);
	}

	/// <summary>Reads the whole input as one statement at compatibility level 170, answering only whether it is read.</summary>
	public static bool TryParseStatement170(string input, out Ast.Statement value)
	{
		return Entries.Carry(TryParseSyntaxStatement170(input, out var old), old, input, 0, input.Length, Located.TryParseSyntaxStatement170, out value);
	}

	/// <summary>Reads one statement at compatibility level 170 from a position, not looking past what it reads.</summary>
	public static Match<Ast.Statement> TryParseStatement170(string input, int at)
	{
		return Entries.Carry(TryParseSyntaxStatement170(input, at), input, Located.TryParseSyntaxStatement170);
	}

	/// <summary>Reads one statement at compatibility level 170 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseStatement170(string input, ref int at, out Ast.Statement value)
	{
		var from = at;

		return Entries.Carry(TryParseSyntaxStatement170(input, ref at, out var old), old, input, from, at - from, Located.TryParseSyntaxStatement170, out value);
	}

	/// <summary>Reads one statement at compatibility level 170 in a window of the input.</summary>
	public static Match<Ast.Statement> TryParseStatement170(string input, int at, int length)
	{
		return Entries.Carry(TryParseSyntaxStatement170(input, at, length), input, Located.TryParseSyntaxStatement170);
	}

	/// <summary>Reads one statement at compatibility level 170 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseStatement170(string input, ref int at, int length, out Ast.Statement value)
	{
		var from = at;

		return Entries.Carry(TryParseSyntaxStatement170(input, ref at, length, out var old), old, input, from, at - from, Located.TryParseSyntaxStatement170, out value);
	}

	/// <summary>Reads the whole input as a text of statements.</summary>
	/// <exception cref="FormatException">The input is not read. <c>TryParseSql</c> answers instead.</exception>
	public static Ast.Statement[] ParseSql(string input)
	{
		return Entries.Parsed(TryParseSql(input));
	}

	/// <summary>Reads the whole input as a text of statements, answering rather than throwing.</summary>
	public static Match<Ast.Statement[]> TryParseSql(string input)
	{
		return Entries.Carry(TryParseSyntaxSql(input), input, Located.TryParseSyntaxSql);
	}

	/// <summary>Reads the whole input as a text of statements, answering only whether it is read.</summary>
	public static bool TryParseSql(string input, out Ast.Statement[] value)
	{
		return Entries.Carry(TryParseSyntaxSql(input, out var old), old, input, 0, input.Length, Located.TryParseSyntaxSql, out value);
	}

	/// <summary>Reads a text of statements from a position, not looking past what it reads.</summary>
	public static Match<Ast.Statement[]> TryParseSql(string input, int at)
	{
		return Entries.Carry(TryParseSyntaxSql(input, at), input, Located.TryParseSyntaxSql);
	}

	/// <summary>Reads a text of statements from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseSql(string input, ref int at, out Ast.Statement[] value)
	{
		var from = at;

		return Entries.Carry(TryParseSyntaxSql(input, ref at, out var old), old, input, from, at - from, Located.TryParseSyntaxSql, out value);
	}

	/// <summary>Reads a text of statements in a window of the input.</summary>
	public static Match<Ast.Statement[]> TryParseSql(string input, int at, int length)
	{
		return Entries.Carry(TryParseSyntaxSql(input, at, length), input, Located.TryParseSyntaxSql);
	}

	/// <summary>Reads a text of statements in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseSql(string input, ref int at, int length, out Ast.Statement[] value)
	{
		var from = at;

		return Entries.Carry(TryParseSyntaxSql(input, ref at, length, out var old), old, input, from, at - from, Located.TryParseSyntaxSql, out value);
	}

	/// <summary>Reads the whole input as a text of statements at compatibility level 100.</summary>
	/// <exception cref="FormatException">The input is not read. <c>TryParseSql100</c> answers instead.</exception>
	public static Ast.Statement[] ParseSql100(string input)
	{
		return Entries.Parsed(TryParseSql100(input));
	}

	/// <summary>Reads the whole input as a text of statements at compatibility level 100, answering rather than throwing.</summary>
	public static Match<Ast.Statement[]> TryParseSql100(string input)
	{
		return Entries.Carry(TryParseSyntaxSql100(input), input, Located.TryParseSyntaxSql100);
	}

	/// <summary>Reads the whole input as a text of statements at compatibility level 100, answering only whether it is read.</summary>
	public static bool TryParseSql100(string input, out Ast.Statement[] value)
	{
		return Entries.Carry(TryParseSyntaxSql100(input, out var old), old, input, 0, input.Length, Located.TryParseSyntaxSql100, out value);
	}

	/// <summary>Reads a text of statements at compatibility level 100 from a position, not looking past what it reads.</summary>
	public static Match<Ast.Statement[]> TryParseSql100(string input, int at)
	{
		return Entries.Carry(TryParseSyntaxSql100(input, at), input, Located.TryParseSyntaxSql100);
	}

	/// <summary>Reads a text of statements at compatibility level 100 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseSql100(string input, ref int at, out Ast.Statement[] value)
	{
		var from = at;

		return Entries.Carry(TryParseSyntaxSql100(input, ref at, out var old), old, input, from, at - from, Located.TryParseSyntaxSql100, out value);
	}

	/// <summary>Reads a text of statements at compatibility level 100 in a window of the input.</summary>
	public static Match<Ast.Statement[]> TryParseSql100(string input, int at, int length)
	{
		return Entries.Carry(TryParseSyntaxSql100(input, at, length), input, Located.TryParseSyntaxSql100);
	}

	/// <summary>Reads a text of statements at compatibility level 100 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseSql100(string input, ref int at, int length, out Ast.Statement[] value)
	{
		var from = at;

		return Entries.Carry(TryParseSyntaxSql100(input, ref at, length, out var old), old, input, from, at - from, Located.TryParseSyntaxSql100, out value);
	}

	/// <summary>Reads the whole input as a text of statements at compatibility level 110.</summary>
	/// <exception cref="FormatException">The input is not read. <c>TryParseSql110</c> answers instead.</exception>
	public static Ast.Statement[] ParseSql110(string input)
	{
		return Entries.Parsed(TryParseSql110(input));
	}

	/// <summary>Reads the whole input as a text of statements at compatibility level 110, answering rather than throwing.</summary>
	public static Match<Ast.Statement[]> TryParseSql110(string input)
	{
		return Entries.Carry(TryParseSyntaxSql110(input), input, Located.TryParseSyntaxSql110);
	}

	/// <summary>Reads the whole input as a text of statements at compatibility level 110, answering only whether it is read.</summary>
	public static bool TryParseSql110(string input, out Ast.Statement[] value)
	{
		return Entries.Carry(TryParseSyntaxSql110(input, out var old), old, input, 0, input.Length, Located.TryParseSyntaxSql110, out value);
	}

	/// <summary>Reads a text of statements at compatibility level 110 from a position, not looking past what it reads.</summary>
	public static Match<Ast.Statement[]> TryParseSql110(string input, int at)
	{
		return Entries.Carry(TryParseSyntaxSql110(input, at), input, Located.TryParseSyntaxSql110);
	}

	/// <summary>Reads a text of statements at compatibility level 110 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseSql110(string input, ref int at, out Ast.Statement[] value)
	{
		var from = at;

		return Entries.Carry(TryParseSyntaxSql110(input, ref at, out var old), old, input, from, at - from, Located.TryParseSyntaxSql110, out value);
	}

	/// <summary>Reads a text of statements at compatibility level 110 in a window of the input.</summary>
	public static Match<Ast.Statement[]> TryParseSql110(string input, int at, int length)
	{
		return Entries.Carry(TryParseSyntaxSql110(input, at, length), input, Located.TryParseSyntaxSql110);
	}

	/// <summary>Reads a text of statements at compatibility level 110 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseSql110(string input, ref int at, int length, out Ast.Statement[] value)
	{
		var from = at;

		return Entries.Carry(TryParseSyntaxSql110(input, ref at, length, out var old), old, input, from, at - from, Located.TryParseSyntaxSql110, out value);
	}

	/// <summary>Reads the whole input as a text of statements at compatibility level 120.</summary>
	/// <exception cref="FormatException">The input is not read. <c>TryParseSql120</c> answers instead.</exception>
	public static Ast.Statement[] ParseSql120(string input)
	{
		return Entries.Parsed(TryParseSql120(input));
	}

	/// <summary>Reads the whole input as a text of statements at compatibility level 120, answering rather than throwing.</summary>
	public static Match<Ast.Statement[]> TryParseSql120(string input)
	{
		return Entries.Carry(TryParseSyntaxSql120(input), input, Located.TryParseSyntaxSql120);
	}

	/// <summary>Reads the whole input as a text of statements at compatibility level 120, answering only whether it is read.</summary>
	public static bool TryParseSql120(string input, out Ast.Statement[] value)
	{
		return Entries.Carry(TryParseSyntaxSql120(input, out var old), old, input, 0, input.Length, Located.TryParseSyntaxSql120, out value);
	}

	/// <summary>Reads a text of statements at compatibility level 120 from a position, not looking past what it reads.</summary>
	public static Match<Ast.Statement[]> TryParseSql120(string input, int at)
	{
		return Entries.Carry(TryParseSyntaxSql120(input, at), input, Located.TryParseSyntaxSql120);
	}

	/// <summary>Reads a text of statements at compatibility level 120 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseSql120(string input, ref int at, out Ast.Statement[] value)
	{
		var from = at;

		return Entries.Carry(TryParseSyntaxSql120(input, ref at, out var old), old, input, from, at - from, Located.TryParseSyntaxSql120, out value);
	}

	/// <summary>Reads a text of statements at compatibility level 120 in a window of the input.</summary>
	public static Match<Ast.Statement[]> TryParseSql120(string input, int at, int length)
	{
		return Entries.Carry(TryParseSyntaxSql120(input, at, length), input, Located.TryParseSyntaxSql120);
	}

	/// <summary>Reads a text of statements at compatibility level 120 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseSql120(string input, ref int at, int length, out Ast.Statement[] value)
	{
		var from = at;

		return Entries.Carry(TryParseSyntaxSql120(input, ref at, length, out var old), old, input, from, at - from, Located.TryParseSyntaxSql120, out value);
	}

	/// <summary>Reads the whole input as a text of statements at compatibility level 130.</summary>
	/// <exception cref="FormatException">The input is not read. <c>TryParseSql130</c> answers instead.</exception>
	public static Ast.Statement[] ParseSql130(string input)
	{
		return Entries.Parsed(TryParseSql130(input));
	}

	/// <summary>Reads the whole input as a text of statements at compatibility level 130, answering rather than throwing.</summary>
	public static Match<Ast.Statement[]> TryParseSql130(string input)
	{
		return Entries.Carry(TryParseSyntaxSql130(input), input, Located.TryParseSyntaxSql130);
	}

	/// <summary>Reads the whole input as a text of statements at compatibility level 130, answering only whether it is read.</summary>
	public static bool TryParseSql130(string input, out Ast.Statement[] value)
	{
		return Entries.Carry(TryParseSyntaxSql130(input, out var old), old, input, 0, input.Length, Located.TryParseSyntaxSql130, out value);
	}

	/// <summary>Reads a text of statements at compatibility level 130 from a position, not looking past what it reads.</summary>
	public static Match<Ast.Statement[]> TryParseSql130(string input, int at)
	{
		return Entries.Carry(TryParseSyntaxSql130(input, at), input, Located.TryParseSyntaxSql130);
	}

	/// <summary>Reads a text of statements at compatibility level 130 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseSql130(string input, ref int at, out Ast.Statement[] value)
	{
		var from = at;

		return Entries.Carry(TryParseSyntaxSql130(input, ref at, out var old), old, input, from, at - from, Located.TryParseSyntaxSql130, out value);
	}

	/// <summary>Reads a text of statements at compatibility level 130 in a window of the input.</summary>
	public static Match<Ast.Statement[]> TryParseSql130(string input, int at, int length)
	{
		return Entries.Carry(TryParseSyntaxSql130(input, at, length), input, Located.TryParseSyntaxSql130);
	}

	/// <summary>Reads a text of statements at compatibility level 130 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseSql130(string input, ref int at, int length, out Ast.Statement[] value)
	{
		var from = at;

		return Entries.Carry(TryParseSyntaxSql130(input, ref at, length, out var old), old, input, from, at - from, Located.TryParseSyntaxSql130, out value);
	}

	/// <summary>Reads the whole input as a text of statements at compatibility level 140.</summary>
	/// <exception cref="FormatException">The input is not read. <c>TryParseSql140</c> answers instead.</exception>
	public static Ast.Statement[] ParseSql140(string input)
	{
		return Entries.Parsed(TryParseSql140(input));
	}

	/// <summary>Reads the whole input as a text of statements at compatibility level 140, answering rather than throwing.</summary>
	public static Match<Ast.Statement[]> TryParseSql140(string input)
	{
		return Entries.Carry(TryParseSyntaxSql140(input), input, Located.TryParseSyntaxSql140);
	}

	/// <summary>Reads the whole input as a text of statements at compatibility level 140, answering only whether it is read.</summary>
	public static bool TryParseSql140(string input, out Ast.Statement[] value)
	{
		return Entries.Carry(TryParseSyntaxSql140(input, out var old), old, input, 0, input.Length, Located.TryParseSyntaxSql140, out value);
	}

	/// <summary>Reads a text of statements at compatibility level 140 from a position, not looking past what it reads.</summary>
	public static Match<Ast.Statement[]> TryParseSql140(string input, int at)
	{
		return Entries.Carry(TryParseSyntaxSql140(input, at), input, Located.TryParseSyntaxSql140);
	}

	/// <summary>Reads a text of statements at compatibility level 140 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseSql140(string input, ref int at, out Ast.Statement[] value)
	{
		var from = at;

		return Entries.Carry(TryParseSyntaxSql140(input, ref at, out var old), old, input, from, at - from, Located.TryParseSyntaxSql140, out value);
	}

	/// <summary>Reads a text of statements at compatibility level 140 in a window of the input.</summary>
	public static Match<Ast.Statement[]> TryParseSql140(string input, int at, int length)
	{
		return Entries.Carry(TryParseSyntaxSql140(input, at, length), input, Located.TryParseSyntaxSql140);
	}

	/// <summary>Reads a text of statements at compatibility level 140 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseSql140(string input, ref int at, int length, out Ast.Statement[] value)
	{
		var from = at;

		return Entries.Carry(TryParseSyntaxSql140(input, ref at, length, out var old), old, input, from, at - from, Located.TryParseSyntaxSql140, out value);
	}

	/// <summary>Reads the whole input as a text of statements at compatibility level 150.</summary>
	/// <exception cref="FormatException">The input is not read. <c>TryParseSql150</c> answers instead.</exception>
	public static Ast.Statement[] ParseSql150(string input)
	{
		return Entries.Parsed(TryParseSql150(input));
	}

	/// <summary>Reads the whole input as a text of statements at compatibility level 150, answering rather than throwing.</summary>
	public static Match<Ast.Statement[]> TryParseSql150(string input)
	{
		return Entries.Carry(TryParseSyntaxSql150(input), input, Located.TryParseSyntaxSql150);
	}

	/// <summary>Reads the whole input as a text of statements at compatibility level 150, answering only whether it is read.</summary>
	public static bool TryParseSql150(string input, out Ast.Statement[] value)
	{
		return Entries.Carry(TryParseSyntaxSql150(input, out var old), old, input, 0, input.Length, Located.TryParseSyntaxSql150, out value);
	}

	/// <summary>Reads a text of statements at compatibility level 150 from a position, not looking past what it reads.</summary>
	public static Match<Ast.Statement[]> TryParseSql150(string input, int at)
	{
		return Entries.Carry(TryParseSyntaxSql150(input, at), input, Located.TryParseSyntaxSql150);
	}

	/// <summary>Reads a text of statements at compatibility level 150 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseSql150(string input, ref int at, out Ast.Statement[] value)
	{
		var from = at;

		return Entries.Carry(TryParseSyntaxSql150(input, ref at, out var old), old, input, from, at - from, Located.TryParseSyntaxSql150, out value);
	}

	/// <summary>Reads a text of statements at compatibility level 150 in a window of the input.</summary>
	public static Match<Ast.Statement[]> TryParseSql150(string input, int at, int length)
	{
		return Entries.Carry(TryParseSyntaxSql150(input, at, length), input, Located.TryParseSyntaxSql150);
	}

	/// <summary>Reads a text of statements at compatibility level 150 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseSql150(string input, ref int at, int length, out Ast.Statement[] value)
	{
		var from = at;

		return Entries.Carry(TryParseSyntaxSql150(input, ref at, length, out var old), old, input, from, at - from, Located.TryParseSyntaxSql150, out value);
	}

	/// <summary>Reads the whole input as a text of statements at compatibility level 160.</summary>
	/// <exception cref="FormatException">The input is not read. <c>TryParseSql160</c> answers instead.</exception>
	public static Ast.Statement[] ParseSql160(string input)
	{
		return Entries.Parsed(TryParseSql160(input));
	}

	/// <summary>Reads the whole input as a text of statements at compatibility level 160, answering rather than throwing.</summary>
	public static Match<Ast.Statement[]> TryParseSql160(string input)
	{
		return Entries.Carry(TryParseSyntaxSql160(input), input, Located.TryParseSyntaxSql160);
	}

	/// <summary>Reads the whole input as a text of statements at compatibility level 160, answering only whether it is read.</summary>
	public static bool TryParseSql160(string input, out Ast.Statement[] value)
	{
		return Entries.Carry(TryParseSyntaxSql160(input, out var old), old, input, 0, input.Length, Located.TryParseSyntaxSql160, out value);
	}

	/// <summary>Reads a text of statements at compatibility level 160 from a position, not looking past what it reads.</summary>
	public static Match<Ast.Statement[]> TryParseSql160(string input, int at)
	{
		return Entries.Carry(TryParseSyntaxSql160(input, at), input, Located.TryParseSyntaxSql160);
	}

	/// <summary>Reads a text of statements at compatibility level 160 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseSql160(string input, ref int at, out Ast.Statement[] value)
	{
		var from = at;

		return Entries.Carry(TryParseSyntaxSql160(input, ref at, out var old), old, input, from, at - from, Located.TryParseSyntaxSql160, out value);
	}

	/// <summary>Reads a text of statements at compatibility level 160 in a window of the input.</summary>
	public static Match<Ast.Statement[]> TryParseSql160(string input, int at, int length)
	{
		return Entries.Carry(TryParseSyntaxSql160(input, at, length), input, Located.TryParseSyntaxSql160);
	}

	/// <summary>Reads a text of statements at compatibility level 160 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseSql160(string input, ref int at, int length, out Ast.Statement[] value)
	{
		var from = at;

		return Entries.Carry(TryParseSyntaxSql160(input, ref at, length, out var old), old, input, from, at - from, Located.TryParseSyntaxSql160, out value);
	}

	/// <summary>Reads the whole input as a text of statements at compatibility level 170.</summary>
	/// <exception cref="FormatException">The input is not read. <c>TryParseSql170</c> answers instead.</exception>
	public static Ast.Statement[] ParseSql170(string input)
	{
		return Entries.Parsed(TryParseSql170(input));
	}

	/// <summary>Reads the whole input as a text of statements at compatibility level 170, answering rather than throwing.</summary>
	public static Match<Ast.Statement[]> TryParseSql170(string input)
	{
		return Entries.Carry(TryParseSyntaxSql170(input), input, Located.TryParseSyntaxSql170);
	}

	/// <summary>Reads the whole input as a text of statements at compatibility level 170, answering only whether it is read.</summary>
	public static bool TryParseSql170(string input, out Ast.Statement[] value)
	{
		return Entries.Carry(TryParseSyntaxSql170(input, out var old), old, input, 0, input.Length, Located.TryParseSyntaxSql170, out value);
	}

	/// <summary>Reads a text of statements at compatibility level 170 from a position, not looking past what it reads.</summary>
	public static Match<Ast.Statement[]> TryParseSql170(string input, int at)
	{
		return Entries.Carry(TryParseSyntaxSql170(input, at), input, Located.TryParseSyntaxSql170);
	}

	/// <summary>Reads a text of statements at compatibility level 170 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseSql170(string input, ref int at, out Ast.Statement[] value)
	{
		var from = at;

		return Entries.Carry(TryParseSyntaxSql170(input, ref at, out var old), old, input, from, at - from, Located.TryParseSyntaxSql170, out value);
	}

	/// <summary>Reads a text of statements at compatibility level 170 in a window of the input.</summary>
	public static Match<Ast.Statement[]> TryParseSql170(string input, int at, int length)
	{
		return Entries.Carry(TryParseSyntaxSql170(input, at, length), input, Located.TryParseSyntaxSql170);
	}

	/// <summary>Reads a text of statements at compatibility level 170 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseSql170(string input, ref int at, int length, out Ast.Statement[] value)
	{
		var from = at;

		return Entries.Carry(TryParseSyntaxSql170(input, ref at, length, out var old), old, input, from, at - from, Located.TryParseSyntaxSql170, out value);
	}

	public static partial class Located
	{
		/// <summary>Reads the whole input as a query statement (no other kind).</summary>
		/// <exception cref="FormatException">The input is not read. <c>TryParseSelect</c> answers instead.</exception>
		public static Ast.Statement ParseSelect(string input)
		{
			return Entries.Parsed(TryParseSelect(input));
		}

		/// <summary>Reads the whole input as a query statement (no other kind), answering rather than throwing.</summary>
		public static Match<Ast.Statement> TryParseSelect(string input)
		{
			return Entries.Carry(Located.TryParseSyntaxSelect(input), input, Located.TryParseSyntaxSelect);
		}

		/// <summary>Reads the whole input as a query statement (no other kind), answering only whether it is read.</summary>
		public static bool TryParseSelect(string input, out Ast.Statement value)
		{
			return Entries.Carry(Located.TryParseSyntaxSelect(input, out var old), old, input, 0, input.Length, Located.TryParseSyntaxSelect, out value);
		}

		/// <summary>Reads a query statement (no other kind) from a position, not looking past what it reads.</summary>
		public static Match<Ast.Statement> TryParseSelect(string input, int at)
		{
			return Entries.Carry(Located.TryParseSyntaxSelect(input, at), input, Located.TryParseSyntaxSelect);
		}

		/// <summary>Reads a query statement (no other kind) from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseSelect(string input, ref int at, out Ast.Statement value)
		{
			var from = at;

			return Entries.Carry(Located.TryParseSyntaxSelect(input, ref at, out var old), old, input, from, at - from, Located.TryParseSyntaxSelect, out value);
		}

		/// <summary>Reads a query statement (no other kind) in a window of the input.</summary>
		public static Match<Ast.Statement> TryParseSelect(string input, int at, int length)
		{
			return Entries.Carry(Located.TryParseSyntaxSelect(input, at, length), input, Located.TryParseSyntaxSelect);
		}

		/// <summary>Reads a query statement (no other kind) in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseSelect(string input, ref int at, int length, out Ast.Statement value)
		{
			var from = at;

			return Entries.Carry(Located.TryParseSyntaxSelect(input, ref at, length, out var old), old, input, from, at - from, Located.TryParseSyntaxSelect, out value);
		}

		/// <summary>Reads the whole input as a search condition.</summary>
		/// <exception cref="FormatException">The input is not read. <c>TryParseSearchCondition</c> answers instead.</exception>
		public static Ast.Expression ParseSearchCondition(string input)
		{
			return Entries.Parsed(TryParseSearchCondition(input));
		}

		/// <summary>Reads the whole input as a search condition, answering rather than throwing.</summary>
		public static Match<Ast.Expression> TryParseSearchCondition(string input)
		{
			return Entries.Carry(Located.TryParseSyntaxCondition(input), input, Located.TryParseSyntaxCondition);
		}

		/// <summary>Reads the whole input as a search condition, answering only whether it is read.</summary>
		public static bool TryParseSearchCondition(string input, out Ast.Expression value)
		{
			return Entries.Carry(Located.TryParseSyntaxCondition(input, out var old), old, input, 0, input.Length, Located.TryParseSyntaxCondition, out value);
		}

		/// <summary>Reads a search condition from a position, not looking past what it reads.</summary>
		public static Match<Ast.Expression> TryParseSearchCondition(string input, int at)
		{
			return Entries.Carry(Located.TryParseSyntaxCondition(input, at), input, Located.TryParseSyntaxCondition);
		}

		/// <summary>Reads a search condition from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseSearchCondition(string input, ref int at, out Ast.Expression value)
		{
			var from = at;

			return Entries.Carry(Located.TryParseSyntaxCondition(input, ref at, out var old), old, input, from, at - from, Located.TryParseSyntaxCondition, out value);
		}

		/// <summary>Reads a search condition in a window of the input.</summary>
		public static Match<Ast.Expression> TryParseSearchCondition(string input, int at, int length)
		{
			return Entries.Carry(Located.TryParseSyntaxCondition(input, at, length), input, Located.TryParseSyntaxCondition);
		}

		/// <summary>Reads a search condition in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseSearchCondition(string input, ref int at, int length, out Ast.Expression value)
		{
			var from = at;

			return Entries.Carry(Located.TryParseSyntaxCondition(input, ref at, length, out var old), old, input, from, at - from, Located.TryParseSyntaxCondition, out value);
		}

		/// <summary>Reads the whole input as an expression.</summary>
		/// <exception cref="FormatException">The input is not read. <c>TryParseExpression</c> answers instead.</exception>
		public static Ast.Expression ParseExpression(string input)
		{
			return Entries.Parsed(TryParseExpression(input));
		}

		/// <summary>Reads the whole input as an expression, answering rather than throwing.</summary>
		public static Match<Ast.Expression> TryParseExpression(string input)
		{
			return Entries.Carry(Located.TryParseSyntaxExpression(input), input, Located.TryParseSyntaxExpression);
		}

		/// <summary>Reads the whole input as an expression, answering only whether it is read.</summary>
		public static bool TryParseExpression(string input, out Ast.Expression value)
		{
			return Entries.Carry(Located.TryParseSyntaxExpression(input, out var old), old, input, 0, input.Length, Located.TryParseSyntaxExpression, out value);
		}

		/// <summary>Reads an expression from a position, not looking past what it reads.</summary>
		public static Match<Ast.Expression> TryParseExpression(string input, int at)
		{
			return Entries.Carry(Located.TryParseSyntaxExpression(input, at), input, Located.TryParseSyntaxExpression);
		}

		/// <summary>Reads an expression from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseExpression(string input, ref int at, out Ast.Expression value)
		{
			var from = at;

			return Entries.Carry(Located.TryParseSyntaxExpression(input, ref at, out var old), old, input, from, at - from, Located.TryParseSyntaxExpression, out value);
		}

		/// <summary>Reads an expression in a window of the input.</summary>
		public static Match<Ast.Expression> TryParseExpression(string input, int at, int length)
		{
			return Entries.Carry(Located.TryParseSyntaxExpression(input, at, length), input, Located.TryParseSyntaxExpression);
		}

		/// <summary>Reads an expression in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseExpression(string input, ref int at, int length, out Ast.Expression value)
		{
			var from = at;

			return Entries.Carry(Located.TryParseSyntaxExpression(input, ref at, length, out var old), old, input, from, at - from, Located.TryParseSyntaxExpression, out value);
		}

		/// <summary>Reads the whole input as one statement.</summary>
		/// <exception cref="FormatException">The input is not read. <c>TryParseStatement</c> answers instead.</exception>
		public static Ast.Statement ParseStatement(string input)
		{
			return Entries.Parsed(TryParseStatement(input));
		}

		/// <summary>Reads the whole input as one statement, answering rather than throwing.</summary>
		public static Match<Ast.Statement> TryParseStatement(string input)
		{
			return Entries.Carry(Located.TryParseSyntaxStatement(input), input, Located.TryParseSyntaxStatement);
		}

		/// <summary>Reads the whole input as one statement, answering only whether it is read.</summary>
		public static bool TryParseStatement(string input, out Ast.Statement value)
		{
			return Entries.Carry(Located.TryParseSyntaxStatement(input, out var old), old, input, 0, input.Length, Located.TryParseSyntaxStatement, out value);
		}

		/// <summary>Reads one statement from a position, not looking past what it reads.</summary>
		public static Match<Ast.Statement> TryParseStatement(string input, int at)
		{
			return Entries.Carry(Located.TryParseSyntaxStatement(input, at), input, Located.TryParseSyntaxStatement);
		}

		/// <summary>Reads one statement from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseStatement(string input, ref int at, out Ast.Statement value)
		{
			var from = at;

			return Entries.Carry(Located.TryParseSyntaxStatement(input, ref at, out var old), old, input, from, at - from, Located.TryParseSyntaxStatement, out value);
		}

		/// <summary>Reads one statement in a window of the input.</summary>
		public static Match<Ast.Statement> TryParseStatement(string input, int at, int length)
		{
			return Entries.Carry(Located.TryParseSyntaxStatement(input, at, length), input, Located.TryParseSyntaxStatement);
		}

		/// <summary>Reads one statement in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseStatement(string input, ref int at, int length, out Ast.Statement value)
		{
			var from = at;

			return Entries.Carry(Located.TryParseSyntaxStatement(input, ref at, length, out var old), old, input, from, at - from, Located.TryParseSyntaxStatement, out value);
		}

		/// <summary>Reads the whole input as one statement at compatibility level 100.</summary>
		/// <exception cref="FormatException">The input is not read. <c>TryParseStatement100</c> answers instead.</exception>
		public static Ast.Statement ParseStatement100(string input)
		{
			return Entries.Parsed(TryParseStatement100(input));
		}

		/// <summary>Reads the whole input as one statement at compatibility level 100, answering rather than throwing.</summary>
		public static Match<Ast.Statement> TryParseStatement100(string input)
		{
			return Entries.Carry(Located.TryParseSyntaxStatement100(input), input, Located.TryParseSyntaxStatement100);
		}

		/// <summary>Reads the whole input as one statement at compatibility level 100, answering only whether it is read.</summary>
		public static bool TryParseStatement100(string input, out Ast.Statement value)
		{
			return Entries.Carry(Located.TryParseSyntaxStatement100(input, out var old), old, input, 0, input.Length, Located.TryParseSyntaxStatement100, out value);
		}

		/// <summary>Reads one statement at compatibility level 100 from a position, not looking past what it reads.</summary>
		public static Match<Ast.Statement> TryParseStatement100(string input, int at)
		{
			return Entries.Carry(Located.TryParseSyntaxStatement100(input, at), input, Located.TryParseSyntaxStatement100);
		}

		/// <summary>Reads one statement at compatibility level 100 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseStatement100(string input, ref int at, out Ast.Statement value)
		{
			var from = at;

			return Entries.Carry(Located.TryParseSyntaxStatement100(input, ref at, out var old), old, input, from, at - from, Located.TryParseSyntaxStatement100, out value);
		}

		/// <summary>Reads one statement at compatibility level 100 in a window of the input.</summary>
		public static Match<Ast.Statement> TryParseStatement100(string input, int at, int length)
		{
			return Entries.Carry(Located.TryParseSyntaxStatement100(input, at, length), input, Located.TryParseSyntaxStatement100);
		}

		/// <summary>Reads one statement at compatibility level 100 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseStatement100(string input, ref int at, int length, out Ast.Statement value)
		{
			var from = at;

			return Entries.Carry(Located.TryParseSyntaxStatement100(input, ref at, length, out var old), old, input, from, at - from, Located.TryParseSyntaxStatement100, out value);
		}

		/// <summary>Reads the whole input as one statement at compatibility level 110.</summary>
		/// <exception cref="FormatException">The input is not read. <c>TryParseStatement110</c> answers instead.</exception>
		public static Ast.Statement ParseStatement110(string input)
		{
			return Entries.Parsed(TryParseStatement110(input));
		}

		/// <summary>Reads the whole input as one statement at compatibility level 110, answering rather than throwing.</summary>
		public static Match<Ast.Statement> TryParseStatement110(string input)
		{
			return Entries.Carry(Located.TryParseSyntaxStatement110(input), input, Located.TryParseSyntaxStatement110);
		}

		/// <summary>Reads the whole input as one statement at compatibility level 110, answering only whether it is read.</summary>
		public static bool TryParseStatement110(string input, out Ast.Statement value)
		{
			return Entries.Carry(Located.TryParseSyntaxStatement110(input, out var old), old, input, 0, input.Length, Located.TryParseSyntaxStatement110, out value);
		}

		/// <summary>Reads one statement at compatibility level 110 from a position, not looking past what it reads.</summary>
		public static Match<Ast.Statement> TryParseStatement110(string input, int at)
		{
			return Entries.Carry(Located.TryParseSyntaxStatement110(input, at), input, Located.TryParseSyntaxStatement110);
		}

		/// <summary>Reads one statement at compatibility level 110 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseStatement110(string input, ref int at, out Ast.Statement value)
		{
			var from = at;

			return Entries.Carry(Located.TryParseSyntaxStatement110(input, ref at, out var old), old, input, from, at - from, Located.TryParseSyntaxStatement110, out value);
		}

		/// <summary>Reads one statement at compatibility level 110 in a window of the input.</summary>
		public static Match<Ast.Statement> TryParseStatement110(string input, int at, int length)
		{
			return Entries.Carry(Located.TryParseSyntaxStatement110(input, at, length), input, Located.TryParseSyntaxStatement110);
		}

		/// <summary>Reads one statement at compatibility level 110 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseStatement110(string input, ref int at, int length, out Ast.Statement value)
		{
			var from = at;

			return Entries.Carry(Located.TryParseSyntaxStatement110(input, ref at, length, out var old), old, input, from, at - from, Located.TryParseSyntaxStatement110, out value);
		}

		/// <summary>Reads the whole input as one statement at compatibility level 120.</summary>
		/// <exception cref="FormatException">The input is not read. <c>TryParseStatement120</c> answers instead.</exception>
		public static Ast.Statement ParseStatement120(string input)
		{
			return Entries.Parsed(TryParseStatement120(input));
		}

		/// <summary>Reads the whole input as one statement at compatibility level 120, answering rather than throwing.</summary>
		public static Match<Ast.Statement> TryParseStatement120(string input)
		{
			return Entries.Carry(Located.TryParseSyntaxStatement120(input), input, Located.TryParseSyntaxStatement120);
		}

		/// <summary>Reads the whole input as one statement at compatibility level 120, answering only whether it is read.</summary>
		public static bool TryParseStatement120(string input, out Ast.Statement value)
		{
			return Entries.Carry(Located.TryParseSyntaxStatement120(input, out var old), old, input, 0, input.Length, Located.TryParseSyntaxStatement120, out value);
		}

		/// <summary>Reads one statement at compatibility level 120 from a position, not looking past what it reads.</summary>
		public static Match<Ast.Statement> TryParseStatement120(string input, int at)
		{
			return Entries.Carry(Located.TryParseSyntaxStatement120(input, at), input, Located.TryParseSyntaxStatement120);
		}

		/// <summary>Reads one statement at compatibility level 120 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseStatement120(string input, ref int at, out Ast.Statement value)
		{
			var from = at;

			return Entries.Carry(Located.TryParseSyntaxStatement120(input, ref at, out var old), old, input, from, at - from, Located.TryParseSyntaxStatement120, out value);
		}

		/// <summary>Reads one statement at compatibility level 120 in a window of the input.</summary>
		public static Match<Ast.Statement> TryParseStatement120(string input, int at, int length)
		{
			return Entries.Carry(Located.TryParseSyntaxStatement120(input, at, length), input, Located.TryParseSyntaxStatement120);
		}

		/// <summary>Reads one statement at compatibility level 120 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseStatement120(string input, ref int at, int length, out Ast.Statement value)
		{
			var from = at;

			return Entries.Carry(Located.TryParseSyntaxStatement120(input, ref at, length, out var old), old, input, from, at - from, Located.TryParseSyntaxStatement120, out value);
		}

		/// <summary>Reads the whole input as one statement at compatibility level 130.</summary>
		/// <exception cref="FormatException">The input is not read. <c>TryParseStatement130</c> answers instead.</exception>
		public static Ast.Statement ParseStatement130(string input)
		{
			return Entries.Parsed(TryParseStatement130(input));
		}

		/// <summary>Reads the whole input as one statement at compatibility level 130, answering rather than throwing.</summary>
		public static Match<Ast.Statement> TryParseStatement130(string input)
		{
			return Entries.Carry(Located.TryParseSyntaxStatement130(input), input, Located.TryParseSyntaxStatement130);
		}

		/// <summary>Reads the whole input as one statement at compatibility level 130, answering only whether it is read.</summary>
		public static bool TryParseStatement130(string input, out Ast.Statement value)
		{
			return Entries.Carry(Located.TryParseSyntaxStatement130(input, out var old), old, input, 0, input.Length, Located.TryParseSyntaxStatement130, out value);
		}

		/// <summary>Reads one statement at compatibility level 130 from a position, not looking past what it reads.</summary>
		public static Match<Ast.Statement> TryParseStatement130(string input, int at)
		{
			return Entries.Carry(Located.TryParseSyntaxStatement130(input, at), input, Located.TryParseSyntaxStatement130);
		}

		/// <summary>Reads one statement at compatibility level 130 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseStatement130(string input, ref int at, out Ast.Statement value)
		{
			var from = at;

			return Entries.Carry(Located.TryParseSyntaxStatement130(input, ref at, out var old), old, input, from, at - from, Located.TryParseSyntaxStatement130, out value);
		}

		/// <summary>Reads one statement at compatibility level 130 in a window of the input.</summary>
		public static Match<Ast.Statement> TryParseStatement130(string input, int at, int length)
		{
			return Entries.Carry(Located.TryParseSyntaxStatement130(input, at, length), input, Located.TryParseSyntaxStatement130);
		}

		/// <summary>Reads one statement at compatibility level 130 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseStatement130(string input, ref int at, int length, out Ast.Statement value)
		{
			var from = at;

			return Entries.Carry(Located.TryParseSyntaxStatement130(input, ref at, length, out var old), old, input, from, at - from, Located.TryParseSyntaxStatement130, out value);
		}

		/// <summary>Reads the whole input as one statement at compatibility level 140.</summary>
		/// <exception cref="FormatException">The input is not read. <c>TryParseStatement140</c> answers instead.</exception>
		public static Ast.Statement ParseStatement140(string input)
		{
			return Entries.Parsed(TryParseStatement140(input));
		}

		/// <summary>Reads the whole input as one statement at compatibility level 140, answering rather than throwing.</summary>
		public static Match<Ast.Statement> TryParseStatement140(string input)
		{
			return Entries.Carry(Located.TryParseSyntaxStatement140(input), input, Located.TryParseSyntaxStatement140);
		}

		/// <summary>Reads the whole input as one statement at compatibility level 140, answering only whether it is read.</summary>
		public static bool TryParseStatement140(string input, out Ast.Statement value)
		{
			return Entries.Carry(Located.TryParseSyntaxStatement140(input, out var old), old, input, 0, input.Length, Located.TryParseSyntaxStatement140, out value);
		}

		/// <summary>Reads one statement at compatibility level 140 from a position, not looking past what it reads.</summary>
		public static Match<Ast.Statement> TryParseStatement140(string input, int at)
		{
			return Entries.Carry(Located.TryParseSyntaxStatement140(input, at), input, Located.TryParseSyntaxStatement140);
		}

		/// <summary>Reads one statement at compatibility level 140 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseStatement140(string input, ref int at, out Ast.Statement value)
		{
			var from = at;

			return Entries.Carry(Located.TryParseSyntaxStatement140(input, ref at, out var old), old, input, from, at - from, Located.TryParseSyntaxStatement140, out value);
		}

		/// <summary>Reads one statement at compatibility level 140 in a window of the input.</summary>
		public static Match<Ast.Statement> TryParseStatement140(string input, int at, int length)
		{
			return Entries.Carry(Located.TryParseSyntaxStatement140(input, at, length), input, Located.TryParseSyntaxStatement140);
		}

		/// <summary>Reads one statement at compatibility level 140 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseStatement140(string input, ref int at, int length, out Ast.Statement value)
		{
			var from = at;

			return Entries.Carry(Located.TryParseSyntaxStatement140(input, ref at, length, out var old), old, input, from, at - from, Located.TryParseSyntaxStatement140, out value);
		}

		/// <summary>Reads the whole input as one statement at compatibility level 150.</summary>
		/// <exception cref="FormatException">The input is not read. <c>TryParseStatement150</c> answers instead.</exception>
		public static Ast.Statement ParseStatement150(string input)
		{
			return Entries.Parsed(TryParseStatement150(input));
		}

		/// <summary>Reads the whole input as one statement at compatibility level 150, answering rather than throwing.</summary>
		public static Match<Ast.Statement> TryParseStatement150(string input)
		{
			return Entries.Carry(Located.TryParseSyntaxStatement150(input), input, Located.TryParseSyntaxStatement150);
		}

		/// <summary>Reads the whole input as one statement at compatibility level 150, answering only whether it is read.</summary>
		public static bool TryParseStatement150(string input, out Ast.Statement value)
		{
			return Entries.Carry(Located.TryParseSyntaxStatement150(input, out var old), old, input, 0, input.Length, Located.TryParseSyntaxStatement150, out value);
		}

		/// <summary>Reads one statement at compatibility level 150 from a position, not looking past what it reads.</summary>
		public static Match<Ast.Statement> TryParseStatement150(string input, int at)
		{
			return Entries.Carry(Located.TryParseSyntaxStatement150(input, at), input, Located.TryParseSyntaxStatement150);
		}

		/// <summary>Reads one statement at compatibility level 150 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseStatement150(string input, ref int at, out Ast.Statement value)
		{
			var from = at;

			return Entries.Carry(Located.TryParseSyntaxStatement150(input, ref at, out var old), old, input, from, at - from, Located.TryParseSyntaxStatement150, out value);
		}

		/// <summary>Reads one statement at compatibility level 150 in a window of the input.</summary>
		public static Match<Ast.Statement> TryParseStatement150(string input, int at, int length)
		{
			return Entries.Carry(Located.TryParseSyntaxStatement150(input, at, length), input, Located.TryParseSyntaxStatement150);
		}

		/// <summary>Reads one statement at compatibility level 150 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseStatement150(string input, ref int at, int length, out Ast.Statement value)
		{
			var from = at;

			return Entries.Carry(Located.TryParseSyntaxStatement150(input, ref at, length, out var old), old, input, from, at - from, Located.TryParseSyntaxStatement150, out value);
		}

		/// <summary>Reads the whole input as one statement at compatibility level 160.</summary>
		/// <exception cref="FormatException">The input is not read. <c>TryParseStatement160</c> answers instead.</exception>
		public static Ast.Statement ParseStatement160(string input)
		{
			return Entries.Parsed(TryParseStatement160(input));
		}

		/// <summary>Reads the whole input as one statement at compatibility level 160, answering rather than throwing.</summary>
		public static Match<Ast.Statement> TryParseStatement160(string input)
		{
			return Entries.Carry(Located.TryParseSyntaxStatement160(input), input, Located.TryParseSyntaxStatement160);
		}

		/// <summary>Reads the whole input as one statement at compatibility level 160, answering only whether it is read.</summary>
		public static bool TryParseStatement160(string input, out Ast.Statement value)
		{
			return Entries.Carry(Located.TryParseSyntaxStatement160(input, out var old), old, input, 0, input.Length, Located.TryParseSyntaxStatement160, out value);
		}

		/// <summary>Reads one statement at compatibility level 160 from a position, not looking past what it reads.</summary>
		public static Match<Ast.Statement> TryParseStatement160(string input, int at)
		{
			return Entries.Carry(Located.TryParseSyntaxStatement160(input, at), input, Located.TryParseSyntaxStatement160);
		}

		/// <summary>Reads one statement at compatibility level 160 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseStatement160(string input, ref int at, out Ast.Statement value)
		{
			var from = at;

			return Entries.Carry(Located.TryParseSyntaxStatement160(input, ref at, out var old), old, input, from, at - from, Located.TryParseSyntaxStatement160, out value);
		}

		/// <summary>Reads one statement at compatibility level 160 in a window of the input.</summary>
		public static Match<Ast.Statement> TryParseStatement160(string input, int at, int length)
		{
			return Entries.Carry(Located.TryParseSyntaxStatement160(input, at, length), input, Located.TryParseSyntaxStatement160);
		}

		/// <summary>Reads one statement at compatibility level 160 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseStatement160(string input, ref int at, int length, out Ast.Statement value)
		{
			var from = at;

			return Entries.Carry(Located.TryParseSyntaxStatement160(input, ref at, length, out var old), old, input, from, at - from, Located.TryParseSyntaxStatement160, out value);
		}

		/// <summary>Reads the whole input as one statement at compatibility level 170.</summary>
		/// <exception cref="FormatException">The input is not read. <c>TryParseStatement170</c> answers instead.</exception>
		public static Ast.Statement ParseStatement170(string input)
		{
			return Entries.Parsed(TryParseStatement170(input));
		}

		/// <summary>Reads the whole input as one statement at compatibility level 170, answering rather than throwing.</summary>
		public static Match<Ast.Statement> TryParseStatement170(string input)
		{
			return Entries.Carry(Located.TryParseSyntaxStatement170(input), input, Located.TryParseSyntaxStatement170);
		}

		/// <summary>Reads the whole input as one statement at compatibility level 170, answering only whether it is read.</summary>
		public static bool TryParseStatement170(string input, out Ast.Statement value)
		{
			return Entries.Carry(Located.TryParseSyntaxStatement170(input, out var old), old, input, 0, input.Length, Located.TryParseSyntaxStatement170, out value);
		}

		/// <summary>Reads one statement at compatibility level 170 from a position, not looking past what it reads.</summary>
		public static Match<Ast.Statement> TryParseStatement170(string input, int at)
		{
			return Entries.Carry(Located.TryParseSyntaxStatement170(input, at), input, Located.TryParseSyntaxStatement170);
		}

		/// <summary>Reads one statement at compatibility level 170 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseStatement170(string input, ref int at, out Ast.Statement value)
		{
			var from = at;

			return Entries.Carry(Located.TryParseSyntaxStatement170(input, ref at, out var old), old, input, from, at - from, Located.TryParseSyntaxStatement170, out value);
		}

		/// <summary>Reads one statement at compatibility level 170 in a window of the input.</summary>
		public static Match<Ast.Statement> TryParseStatement170(string input, int at, int length)
		{
			return Entries.Carry(Located.TryParseSyntaxStatement170(input, at, length), input, Located.TryParseSyntaxStatement170);
		}

		/// <summary>Reads one statement at compatibility level 170 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseStatement170(string input, ref int at, int length, out Ast.Statement value)
		{
			var from = at;

			return Entries.Carry(Located.TryParseSyntaxStatement170(input, ref at, length, out var old), old, input, from, at - from, Located.TryParseSyntaxStatement170, out value);
		}

		/// <summary>Reads the whole input as a text of statements.</summary>
		/// <exception cref="FormatException">The input is not read. <c>TryParseSql</c> answers instead.</exception>
		public static Ast.Statement[] ParseSql(string input)
		{
			return Entries.Parsed(TryParseSql(input));
		}

		/// <summary>Reads the whole input as a text of statements, answering rather than throwing.</summary>
		public static Match<Ast.Statement[]> TryParseSql(string input)
		{
			return Entries.Carry(Located.TryParseSyntaxSql(input), input, Located.TryParseSyntaxSql);
		}

		/// <summary>Reads the whole input as a text of statements, answering only whether it is read.</summary>
		public static bool TryParseSql(string input, out Ast.Statement[] value)
		{
			return Entries.Carry(Located.TryParseSyntaxSql(input, out var old), old, input, 0, input.Length, Located.TryParseSyntaxSql, out value);
		}

		/// <summary>Reads a text of statements from a position, not looking past what it reads.</summary>
		public static Match<Ast.Statement[]> TryParseSql(string input, int at)
		{
			return Entries.Carry(Located.TryParseSyntaxSql(input, at), input, Located.TryParseSyntaxSql);
		}

		/// <summary>Reads a text of statements from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseSql(string input, ref int at, out Ast.Statement[] value)
		{
			var from = at;

			return Entries.Carry(Located.TryParseSyntaxSql(input, ref at, out var old), old, input, from, at - from, Located.TryParseSyntaxSql, out value);
		}

		/// <summary>Reads a text of statements in a window of the input.</summary>
		public static Match<Ast.Statement[]> TryParseSql(string input, int at, int length)
		{
			return Entries.Carry(Located.TryParseSyntaxSql(input, at, length), input, Located.TryParseSyntaxSql);
		}

		/// <summary>Reads a text of statements in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseSql(string input, ref int at, int length, out Ast.Statement[] value)
		{
			var from = at;

			return Entries.Carry(Located.TryParseSyntaxSql(input, ref at, length, out var old), old, input, from, at - from, Located.TryParseSyntaxSql, out value);
		}

		/// <summary>Reads the whole input as a text of statements at compatibility level 100.</summary>
		/// <exception cref="FormatException">The input is not read. <c>TryParseSql100</c> answers instead.</exception>
		public static Ast.Statement[] ParseSql100(string input)
		{
			return Entries.Parsed(TryParseSql100(input));
		}

		/// <summary>Reads the whole input as a text of statements at compatibility level 100, answering rather than throwing.</summary>
		public static Match<Ast.Statement[]> TryParseSql100(string input)
		{
			return Entries.Carry(Located.TryParseSyntaxSql100(input), input, Located.TryParseSyntaxSql100);
		}

		/// <summary>Reads the whole input as a text of statements at compatibility level 100, answering only whether it is read.</summary>
		public static bool TryParseSql100(string input, out Ast.Statement[] value)
		{
			return Entries.Carry(Located.TryParseSyntaxSql100(input, out var old), old, input, 0, input.Length, Located.TryParseSyntaxSql100, out value);
		}

		/// <summary>Reads a text of statements at compatibility level 100 from a position, not looking past what it reads.</summary>
		public static Match<Ast.Statement[]> TryParseSql100(string input, int at)
		{
			return Entries.Carry(Located.TryParseSyntaxSql100(input, at), input, Located.TryParseSyntaxSql100);
		}

		/// <summary>Reads a text of statements at compatibility level 100 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseSql100(string input, ref int at, out Ast.Statement[] value)
		{
			var from = at;

			return Entries.Carry(Located.TryParseSyntaxSql100(input, ref at, out var old), old, input, from, at - from, Located.TryParseSyntaxSql100, out value);
		}

		/// <summary>Reads a text of statements at compatibility level 100 in a window of the input.</summary>
		public static Match<Ast.Statement[]> TryParseSql100(string input, int at, int length)
		{
			return Entries.Carry(Located.TryParseSyntaxSql100(input, at, length), input, Located.TryParseSyntaxSql100);
		}

		/// <summary>Reads a text of statements at compatibility level 100 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseSql100(string input, ref int at, int length, out Ast.Statement[] value)
		{
			var from = at;

			return Entries.Carry(Located.TryParseSyntaxSql100(input, ref at, length, out var old), old, input, from, at - from, Located.TryParseSyntaxSql100, out value);
		}

		/// <summary>Reads the whole input as a text of statements at compatibility level 110.</summary>
		/// <exception cref="FormatException">The input is not read. <c>TryParseSql110</c> answers instead.</exception>
		public static Ast.Statement[] ParseSql110(string input)
		{
			return Entries.Parsed(TryParseSql110(input));
		}

		/// <summary>Reads the whole input as a text of statements at compatibility level 110, answering rather than throwing.</summary>
		public static Match<Ast.Statement[]> TryParseSql110(string input)
		{
			return Entries.Carry(Located.TryParseSyntaxSql110(input), input, Located.TryParseSyntaxSql110);
		}

		/// <summary>Reads the whole input as a text of statements at compatibility level 110, answering only whether it is read.</summary>
		public static bool TryParseSql110(string input, out Ast.Statement[] value)
		{
			return Entries.Carry(Located.TryParseSyntaxSql110(input, out var old), old, input, 0, input.Length, Located.TryParseSyntaxSql110, out value);
		}

		/// <summary>Reads a text of statements at compatibility level 110 from a position, not looking past what it reads.</summary>
		public static Match<Ast.Statement[]> TryParseSql110(string input, int at)
		{
			return Entries.Carry(Located.TryParseSyntaxSql110(input, at), input, Located.TryParseSyntaxSql110);
		}

		/// <summary>Reads a text of statements at compatibility level 110 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseSql110(string input, ref int at, out Ast.Statement[] value)
		{
			var from = at;

			return Entries.Carry(Located.TryParseSyntaxSql110(input, ref at, out var old), old, input, from, at - from, Located.TryParseSyntaxSql110, out value);
		}

		/// <summary>Reads a text of statements at compatibility level 110 in a window of the input.</summary>
		public static Match<Ast.Statement[]> TryParseSql110(string input, int at, int length)
		{
			return Entries.Carry(Located.TryParseSyntaxSql110(input, at, length), input, Located.TryParseSyntaxSql110);
		}

		/// <summary>Reads a text of statements at compatibility level 110 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseSql110(string input, ref int at, int length, out Ast.Statement[] value)
		{
			var from = at;

			return Entries.Carry(Located.TryParseSyntaxSql110(input, ref at, length, out var old), old, input, from, at - from, Located.TryParseSyntaxSql110, out value);
		}

		/// <summary>Reads the whole input as a text of statements at compatibility level 120.</summary>
		/// <exception cref="FormatException">The input is not read. <c>TryParseSql120</c> answers instead.</exception>
		public static Ast.Statement[] ParseSql120(string input)
		{
			return Entries.Parsed(TryParseSql120(input));
		}

		/// <summary>Reads the whole input as a text of statements at compatibility level 120, answering rather than throwing.</summary>
		public static Match<Ast.Statement[]> TryParseSql120(string input)
		{
			return Entries.Carry(Located.TryParseSyntaxSql120(input), input, Located.TryParseSyntaxSql120);
		}

		/// <summary>Reads the whole input as a text of statements at compatibility level 120, answering only whether it is read.</summary>
		public static bool TryParseSql120(string input, out Ast.Statement[] value)
		{
			return Entries.Carry(Located.TryParseSyntaxSql120(input, out var old), old, input, 0, input.Length, Located.TryParseSyntaxSql120, out value);
		}

		/// <summary>Reads a text of statements at compatibility level 120 from a position, not looking past what it reads.</summary>
		public static Match<Ast.Statement[]> TryParseSql120(string input, int at)
		{
			return Entries.Carry(Located.TryParseSyntaxSql120(input, at), input, Located.TryParseSyntaxSql120);
		}

		/// <summary>Reads a text of statements at compatibility level 120 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseSql120(string input, ref int at, out Ast.Statement[] value)
		{
			var from = at;

			return Entries.Carry(Located.TryParseSyntaxSql120(input, ref at, out var old), old, input, from, at - from, Located.TryParseSyntaxSql120, out value);
		}

		/// <summary>Reads a text of statements at compatibility level 120 in a window of the input.</summary>
		public static Match<Ast.Statement[]> TryParseSql120(string input, int at, int length)
		{
			return Entries.Carry(Located.TryParseSyntaxSql120(input, at, length), input, Located.TryParseSyntaxSql120);
		}

		/// <summary>Reads a text of statements at compatibility level 120 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseSql120(string input, ref int at, int length, out Ast.Statement[] value)
		{
			var from = at;

			return Entries.Carry(Located.TryParseSyntaxSql120(input, ref at, length, out var old), old, input, from, at - from, Located.TryParseSyntaxSql120, out value);
		}

		/// <summary>Reads the whole input as a text of statements at compatibility level 130.</summary>
		/// <exception cref="FormatException">The input is not read. <c>TryParseSql130</c> answers instead.</exception>
		public static Ast.Statement[] ParseSql130(string input)
		{
			return Entries.Parsed(TryParseSql130(input));
		}

		/// <summary>Reads the whole input as a text of statements at compatibility level 130, answering rather than throwing.</summary>
		public static Match<Ast.Statement[]> TryParseSql130(string input)
		{
			return Entries.Carry(Located.TryParseSyntaxSql130(input), input, Located.TryParseSyntaxSql130);
		}

		/// <summary>Reads the whole input as a text of statements at compatibility level 130, answering only whether it is read.</summary>
		public static bool TryParseSql130(string input, out Ast.Statement[] value)
		{
			return Entries.Carry(Located.TryParseSyntaxSql130(input, out var old), old, input, 0, input.Length, Located.TryParseSyntaxSql130, out value);
		}

		/// <summary>Reads a text of statements at compatibility level 130 from a position, not looking past what it reads.</summary>
		public static Match<Ast.Statement[]> TryParseSql130(string input, int at)
		{
			return Entries.Carry(Located.TryParseSyntaxSql130(input, at), input, Located.TryParseSyntaxSql130);
		}

		/// <summary>Reads a text of statements at compatibility level 130 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseSql130(string input, ref int at, out Ast.Statement[] value)
		{
			var from = at;

			return Entries.Carry(Located.TryParseSyntaxSql130(input, ref at, out var old), old, input, from, at - from, Located.TryParseSyntaxSql130, out value);
		}

		/// <summary>Reads a text of statements at compatibility level 130 in a window of the input.</summary>
		public static Match<Ast.Statement[]> TryParseSql130(string input, int at, int length)
		{
			return Entries.Carry(Located.TryParseSyntaxSql130(input, at, length), input, Located.TryParseSyntaxSql130);
		}

		/// <summary>Reads a text of statements at compatibility level 130 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseSql130(string input, ref int at, int length, out Ast.Statement[] value)
		{
			var from = at;

			return Entries.Carry(Located.TryParseSyntaxSql130(input, ref at, length, out var old), old, input, from, at - from, Located.TryParseSyntaxSql130, out value);
		}

		/// <summary>Reads the whole input as a text of statements at compatibility level 140.</summary>
		/// <exception cref="FormatException">The input is not read. <c>TryParseSql140</c> answers instead.</exception>
		public static Ast.Statement[] ParseSql140(string input)
		{
			return Entries.Parsed(TryParseSql140(input));
		}

		/// <summary>Reads the whole input as a text of statements at compatibility level 140, answering rather than throwing.</summary>
		public static Match<Ast.Statement[]> TryParseSql140(string input)
		{
			return Entries.Carry(Located.TryParseSyntaxSql140(input), input, Located.TryParseSyntaxSql140);
		}

		/// <summary>Reads the whole input as a text of statements at compatibility level 140, answering only whether it is read.</summary>
		public static bool TryParseSql140(string input, out Ast.Statement[] value)
		{
			return Entries.Carry(Located.TryParseSyntaxSql140(input, out var old), old, input, 0, input.Length, Located.TryParseSyntaxSql140, out value);
		}

		/// <summary>Reads a text of statements at compatibility level 140 from a position, not looking past what it reads.</summary>
		public static Match<Ast.Statement[]> TryParseSql140(string input, int at)
		{
			return Entries.Carry(Located.TryParseSyntaxSql140(input, at), input, Located.TryParseSyntaxSql140);
		}

		/// <summary>Reads a text of statements at compatibility level 140 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseSql140(string input, ref int at, out Ast.Statement[] value)
		{
			var from = at;

			return Entries.Carry(Located.TryParseSyntaxSql140(input, ref at, out var old), old, input, from, at - from, Located.TryParseSyntaxSql140, out value);
		}

		/// <summary>Reads a text of statements at compatibility level 140 in a window of the input.</summary>
		public static Match<Ast.Statement[]> TryParseSql140(string input, int at, int length)
		{
			return Entries.Carry(Located.TryParseSyntaxSql140(input, at, length), input, Located.TryParseSyntaxSql140);
		}

		/// <summary>Reads a text of statements at compatibility level 140 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseSql140(string input, ref int at, int length, out Ast.Statement[] value)
		{
			var from = at;

			return Entries.Carry(Located.TryParseSyntaxSql140(input, ref at, length, out var old), old, input, from, at - from, Located.TryParseSyntaxSql140, out value);
		}

		/// <summary>Reads the whole input as a text of statements at compatibility level 150.</summary>
		/// <exception cref="FormatException">The input is not read. <c>TryParseSql150</c> answers instead.</exception>
		public static Ast.Statement[] ParseSql150(string input)
		{
			return Entries.Parsed(TryParseSql150(input));
		}

		/// <summary>Reads the whole input as a text of statements at compatibility level 150, answering rather than throwing.</summary>
		public static Match<Ast.Statement[]> TryParseSql150(string input)
		{
			return Entries.Carry(Located.TryParseSyntaxSql150(input), input, Located.TryParseSyntaxSql150);
		}

		/// <summary>Reads the whole input as a text of statements at compatibility level 150, answering only whether it is read.</summary>
		public static bool TryParseSql150(string input, out Ast.Statement[] value)
		{
			return Entries.Carry(Located.TryParseSyntaxSql150(input, out var old), old, input, 0, input.Length, Located.TryParseSyntaxSql150, out value);
		}

		/// <summary>Reads a text of statements at compatibility level 150 from a position, not looking past what it reads.</summary>
		public static Match<Ast.Statement[]> TryParseSql150(string input, int at)
		{
			return Entries.Carry(Located.TryParseSyntaxSql150(input, at), input, Located.TryParseSyntaxSql150);
		}

		/// <summary>Reads a text of statements at compatibility level 150 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseSql150(string input, ref int at, out Ast.Statement[] value)
		{
			var from = at;

			return Entries.Carry(Located.TryParseSyntaxSql150(input, ref at, out var old), old, input, from, at - from, Located.TryParseSyntaxSql150, out value);
		}

		/// <summary>Reads a text of statements at compatibility level 150 in a window of the input.</summary>
		public static Match<Ast.Statement[]> TryParseSql150(string input, int at, int length)
		{
			return Entries.Carry(Located.TryParseSyntaxSql150(input, at, length), input, Located.TryParseSyntaxSql150);
		}

		/// <summary>Reads a text of statements at compatibility level 150 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseSql150(string input, ref int at, int length, out Ast.Statement[] value)
		{
			var from = at;

			return Entries.Carry(Located.TryParseSyntaxSql150(input, ref at, length, out var old), old, input, from, at - from, Located.TryParseSyntaxSql150, out value);
		}

		/// <summary>Reads the whole input as a text of statements at compatibility level 160.</summary>
		/// <exception cref="FormatException">The input is not read. <c>TryParseSql160</c> answers instead.</exception>
		public static Ast.Statement[] ParseSql160(string input)
		{
			return Entries.Parsed(TryParseSql160(input));
		}

		/// <summary>Reads the whole input as a text of statements at compatibility level 160, answering rather than throwing.</summary>
		public static Match<Ast.Statement[]> TryParseSql160(string input)
		{
			return Entries.Carry(Located.TryParseSyntaxSql160(input), input, Located.TryParseSyntaxSql160);
		}

		/// <summary>Reads the whole input as a text of statements at compatibility level 160, answering only whether it is read.</summary>
		public static bool TryParseSql160(string input, out Ast.Statement[] value)
		{
			return Entries.Carry(Located.TryParseSyntaxSql160(input, out var old), old, input, 0, input.Length, Located.TryParseSyntaxSql160, out value);
		}

		/// <summary>Reads a text of statements at compatibility level 160 from a position, not looking past what it reads.</summary>
		public static Match<Ast.Statement[]> TryParseSql160(string input, int at)
		{
			return Entries.Carry(Located.TryParseSyntaxSql160(input, at), input, Located.TryParseSyntaxSql160);
		}

		/// <summary>Reads a text of statements at compatibility level 160 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseSql160(string input, ref int at, out Ast.Statement[] value)
		{
			var from = at;

			return Entries.Carry(Located.TryParseSyntaxSql160(input, ref at, out var old), old, input, from, at - from, Located.TryParseSyntaxSql160, out value);
		}

		/// <summary>Reads a text of statements at compatibility level 160 in a window of the input.</summary>
		public static Match<Ast.Statement[]> TryParseSql160(string input, int at, int length)
		{
			return Entries.Carry(Located.TryParseSyntaxSql160(input, at, length), input, Located.TryParseSyntaxSql160);
		}

		/// <summary>Reads a text of statements at compatibility level 160 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseSql160(string input, ref int at, int length, out Ast.Statement[] value)
		{
			var from = at;

			return Entries.Carry(Located.TryParseSyntaxSql160(input, ref at, length, out var old), old, input, from, at - from, Located.TryParseSyntaxSql160, out value);
		}

		/// <summary>Reads the whole input as a text of statements at compatibility level 170.</summary>
		/// <exception cref="FormatException">The input is not read. <c>TryParseSql170</c> answers instead.</exception>
		public static Ast.Statement[] ParseSql170(string input)
		{
			return Entries.Parsed(TryParseSql170(input));
		}

		/// <summary>Reads the whole input as a text of statements at compatibility level 170, answering rather than throwing.</summary>
		public static Match<Ast.Statement[]> TryParseSql170(string input)
		{
			return Entries.Carry(Located.TryParseSyntaxSql170(input), input, Located.TryParseSyntaxSql170);
		}

		/// <summary>Reads the whole input as a text of statements at compatibility level 170, answering only whether it is read.</summary>
		public static bool TryParseSql170(string input, out Ast.Statement[] value)
		{
			return Entries.Carry(Located.TryParseSyntaxSql170(input, out var old), old, input, 0, input.Length, Located.TryParseSyntaxSql170, out value);
		}

		/// <summary>Reads a text of statements at compatibility level 170 from a position, not looking past what it reads.</summary>
		public static Match<Ast.Statement[]> TryParseSql170(string input, int at)
		{
			return Entries.Carry(Located.TryParseSyntaxSql170(input, at), input, Located.TryParseSyntaxSql170);
		}

		/// <summary>Reads a text of statements at compatibility level 170 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseSql170(string input, ref int at, out Ast.Statement[] value)
		{
			var from = at;

			return Entries.Carry(Located.TryParseSyntaxSql170(input, ref at, out var old), old, input, from, at - from, Located.TryParseSyntaxSql170, out value);
		}

		/// <summary>Reads a text of statements at compatibility level 170 in a window of the input.</summary>
		public static Match<Ast.Statement[]> TryParseSql170(string input, int at, int length)
		{
			return Entries.Carry(Located.TryParseSyntaxSql170(input, at, length), input, Located.TryParseSyntaxSql170);
		}

		/// <summary>Reads a text of statements at compatibility level 170 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseSql170(string input, ref int at, int length, out Ast.Statement[] value)
		{
			var from = at;

			return Entries.Carry(Located.TryParseSyntaxSql170(input, ref at, length, out var old), old, input, from, at - from, Located.TryParseSyntaxSql170, out value);
		}
	}

	/// <summary>What the public entries share: a reading of the T-SQL tree carried onto the common tree.</summary>
	static class Entries
	{
		/// <summary>What a refusal says where the reading had nothing of its own to say.</summary>
		const string Otherwise = "Input does not match.";

		public static Match<Ast.Statement> Carry(Match<Statement> read, string input, Func<string, int, int, Match<Statement>> again)
		{
			return read.IsSuccess
				? Match<Ast.Statement>.Success(Bridge.Carried(read.Value, input, (int)read.Position, read.Length, again), read.Position, read.Length, read.ReadingEnd)
				: read.Refusal<Ast.Statement>(read.Position, Otherwise);
		}

		public static Match<Ast.Expression> Carry(Match<Expression> read, string input, Func<string, int, int, Match<Expression>> again)
		{
			return read.IsSuccess
				? Match<Ast.Expression>.Success(Bridge.Carried(read.Value, input, (int)read.Position, read.Length, again), read.Position, read.Length, read.ReadingEnd)
				: read.Refusal<Ast.Expression>(read.Position, Otherwise);
		}

		public static Match<Ast.Statement[]> Carry(Match<Statement[]> read, string input, Func<string, int, int, Match<Statement[]>> again)
		{
			return read.IsSuccess
				? Match<Ast.Statement[]>.Success(Bridge.Carried(read.Value, input, (int)read.Position, read.Length, again), read.Position, read.Length, read.ReadingEnd)
				: read.Refusal<Ast.Statement[]>(read.Position, Otherwise);
		}

		public static bool Carry(bool read, Statement old, string input, int at, int length, Func<string, int, int, Match<Statement>> again, out Ast.Statement value)
		{
			value = read ? Bridge.Carried(old, input, at, length, again) : null!;

			return read;
		}

		public static bool Carry(bool read, Expression old, string input, int at, int length, Func<string, int, int, Match<Expression>> again, out Ast.Expression value)
		{
			value = read ? Bridge.Carried(old, input, at, length, again) : null!;

			return read;
		}

		public static bool Carry(bool read, Statement[] old, string input, int at, int length, Func<string, int, int, Match<Statement[]>> again, out Ast.Statement[] value)
		{
			value = read ? Bridge.Carried(old, input, at, length, again) : null!;

			return read;
		}

		/// <summary>The value of a reading, or its refusal thrown as the generated entries throw one.</summary>
		public static T Parsed<T>(Match<T> match)
		{
			if (match.IsSuccess)
				return match.Value;

			var error = match.Error!;

			throw new FormatException(
				(error.EndsWith(".", StringComparison.Ordinal) ? error.Substring(0, error.Length - 1) : error) +
				" at " + match.Position.ToString(System.Globalization.CultureInfo.InvariantCulture) + ".");
		}
	}
}
