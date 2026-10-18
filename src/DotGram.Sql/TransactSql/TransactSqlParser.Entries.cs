using System;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.TransactSql;

// The public entries: each reads through the internal entry of the same rule, which builds the T-SQL tree
// (TransactSql.gram, `ParseSyntax…`), and hands back what it read carried onto the common tree, DotGram.Sql.Ast
// (TransactSql/Bridge). A refusal is the internal entry's own, at the same place with the same message. The same
// methods twice, for the plain reader and the located one, at every level the grammar publishes; both read with
// locations, which the bridge needs, and the plain one hands back a tree that says none. Each reading is carried with
// its text from before it starts (Bridge.Over): a rule that builds the common tree itself carries its parts across while
// the statement is read. They go when the grammar's rules build the common tree themselves and publish it under these
// names.
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
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSelect(input), input, locate: false);
	}

	/// <summary>Reads the whole input as a query statement (no other kind), answering only whether it is read.</summary>
	public static bool TryParseSelect(string input, out Ast.Statement value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSelect(input, out var old), old, input, false, out value);
	}

	/// <summary>Reads a query statement (no other kind) from a position, not looking past what it reads.</summary>
	public static Match<Ast.Statement> TryParseSelect(string input, int at)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSelect(input, at), input, locate: false);
	}

	/// <summary>Reads a query statement (no other kind) from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseSelect(string input, ref int at, out Ast.Statement value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSelect(input, ref at, out var old), old, input, false, out value);
	}

	/// <summary>Reads a query statement (no other kind) in a window of the input.</summary>
	public static Match<Ast.Statement> TryParseSelect(string input, int at, int length)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSelect(input, at, length), input, locate: false);
	}

	/// <summary>Reads a query statement (no other kind) in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseSelect(string input, ref int at, int length, out Ast.Statement value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSelect(input, ref at, length, out var old), old, input, false, out value);
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
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxCondition(input), input, locate: false);
	}

	/// <summary>Reads the whole input as a search condition, answering only whether it is read.</summary>
	public static bool TryParseSearchCondition(string input, out Ast.Expression value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxCondition(input, out var old), old, input, false, out value);
	}

	/// <summary>Reads a search condition from a position, not looking past what it reads.</summary>
	public static Match<Ast.Expression> TryParseSearchCondition(string input, int at)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxCondition(input, at), input, locate: false);
	}

	/// <summary>Reads a search condition from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseSearchCondition(string input, ref int at, out Ast.Expression value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxCondition(input, ref at, out var old), old, input, false, out value);
	}

	/// <summary>Reads a search condition in a window of the input.</summary>
	public static Match<Ast.Expression> TryParseSearchCondition(string input, int at, int length)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxCondition(input, at, length), input, locate: false);
	}

	/// <summary>Reads a search condition in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseSearchCondition(string input, ref int at, int length, out Ast.Expression value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxCondition(input, ref at, length, out var old), old, input, false, out value);
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
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxExpression(input), input, locate: false);
	}

	/// <summary>Reads the whole input as an expression, answering only whether it is read.</summary>
	public static bool TryParseExpression(string input, out Ast.Expression value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxExpression(input, out var old), old, input, false, out value);
	}

	/// <summary>Reads an expression from a position, not looking past what it reads.</summary>
	public static Match<Ast.Expression> TryParseExpression(string input, int at)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxExpression(input, at), input, locate: false);
	}

	/// <summary>Reads an expression from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseExpression(string input, ref int at, out Ast.Expression value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxExpression(input, ref at, out var old), old, input, false, out value);
	}

	/// <summary>Reads an expression in a window of the input.</summary>
	public static Match<Ast.Expression> TryParseExpression(string input, int at, int length)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxExpression(input, at, length), input, locate: false);
	}

	/// <summary>Reads an expression in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseExpression(string input, ref int at, int length, out Ast.Expression value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxExpression(input, ref at, length, out var old), old, input, false, out value);
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
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement(input), input, locate: false);
	}

	/// <summary>Reads the whole input as one statement, answering only whether it is read.</summary>
	public static bool TryParseStatement(string input, out Ast.Statement value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement(input, out var old), old, input, false, out value);
	}

	/// <summary>Reads one statement from a position, not looking past what it reads.</summary>
	public static Match<Ast.Statement> TryParseStatement(string input, int at)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement(input, at), input, locate: false);
	}

	/// <summary>Reads one statement from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseStatement(string input, ref int at, out Ast.Statement value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement(input, ref at, out var old), old, input, false, out value);
	}

	/// <summary>Reads one statement in a window of the input.</summary>
	public static Match<Ast.Statement> TryParseStatement(string input, int at, int length)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement(input, at, length), input, locate: false);
	}

	/// <summary>Reads one statement in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseStatement(string input, ref int at, int length, out Ast.Statement value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement(input, ref at, length, out var old), old, input, false, out value);
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
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement100(input), input, locate: false);
	}

	/// <summary>Reads the whole input as one statement at compatibility level 100, answering only whether it is read.</summary>
	public static bool TryParseStatement100(string input, out Ast.Statement value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement100(input, out var old), old, input, false, out value);
	}

	/// <summary>Reads one statement at compatibility level 100 from a position, not looking past what it reads.</summary>
	public static Match<Ast.Statement> TryParseStatement100(string input, int at)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement100(input, at), input, locate: false);
	}

	/// <summary>Reads one statement at compatibility level 100 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseStatement100(string input, ref int at, out Ast.Statement value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement100(input, ref at, out var old), old, input, false, out value);
	}

	/// <summary>Reads one statement at compatibility level 100 in a window of the input.</summary>
	public static Match<Ast.Statement> TryParseStatement100(string input, int at, int length)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement100(input, at, length), input, locate: false);
	}

	/// <summary>Reads one statement at compatibility level 100 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseStatement100(string input, ref int at, int length, out Ast.Statement value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement100(input, ref at, length, out var old), old, input, false, out value);
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
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement110(input), input, locate: false);
	}

	/// <summary>Reads the whole input as one statement at compatibility level 110, answering only whether it is read.</summary>
	public static bool TryParseStatement110(string input, out Ast.Statement value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement110(input, out var old), old, input, false, out value);
	}

	/// <summary>Reads one statement at compatibility level 110 from a position, not looking past what it reads.</summary>
	public static Match<Ast.Statement> TryParseStatement110(string input, int at)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement110(input, at), input, locate: false);
	}

	/// <summary>Reads one statement at compatibility level 110 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseStatement110(string input, ref int at, out Ast.Statement value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement110(input, ref at, out var old), old, input, false, out value);
	}

	/// <summary>Reads one statement at compatibility level 110 in a window of the input.</summary>
	public static Match<Ast.Statement> TryParseStatement110(string input, int at, int length)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement110(input, at, length), input, locate: false);
	}

	/// <summary>Reads one statement at compatibility level 110 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseStatement110(string input, ref int at, int length, out Ast.Statement value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement110(input, ref at, length, out var old), old, input, false, out value);
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
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement120(input), input, locate: false);
	}

	/// <summary>Reads the whole input as one statement at compatibility level 120, answering only whether it is read.</summary>
	public static bool TryParseStatement120(string input, out Ast.Statement value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement120(input, out var old), old, input, false, out value);
	}

	/// <summary>Reads one statement at compatibility level 120 from a position, not looking past what it reads.</summary>
	public static Match<Ast.Statement> TryParseStatement120(string input, int at)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement120(input, at), input, locate: false);
	}

	/// <summary>Reads one statement at compatibility level 120 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseStatement120(string input, ref int at, out Ast.Statement value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement120(input, ref at, out var old), old, input, false, out value);
	}

	/// <summary>Reads one statement at compatibility level 120 in a window of the input.</summary>
	public static Match<Ast.Statement> TryParseStatement120(string input, int at, int length)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement120(input, at, length), input, locate: false);
	}

	/// <summary>Reads one statement at compatibility level 120 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseStatement120(string input, ref int at, int length, out Ast.Statement value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement120(input, ref at, length, out var old), old, input, false, out value);
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
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement130(input), input, locate: false);
	}

	/// <summary>Reads the whole input as one statement at compatibility level 130, answering only whether it is read.</summary>
	public static bool TryParseStatement130(string input, out Ast.Statement value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement130(input, out var old), old, input, false, out value);
	}

	/// <summary>Reads one statement at compatibility level 130 from a position, not looking past what it reads.</summary>
	public static Match<Ast.Statement> TryParseStatement130(string input, int at)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement130(input, at), input, locate: false);
	}

	/// <summary>Reads one statement at compatibility level 130 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseStatement130(string input, ref int at, out Ast.Statement value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement130(input, ref at, out var old), old, input, false, out value);
	}

	/// <summary>Reads one statement at compatibility level 130 in a window of the input.</summary>
	public static Match<Ast.Statement> TryParseStatement130(string input, int at, int length)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement130(input, at, length), input, locate: false);
	}

	/// <summary>Reads one statement at compatibility level 130 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseStatement130(string input, ref int at, int length, out Ast.Statement value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement130(input, ref at, length, out var old), old, input, false, out value);
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
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement140(input), input, locate: false);
	}

	/// <summary>Reads the whole input as one statement at compatibility level 140, answering only whether it is read.</summary>
	public static bool TryParseStatement140(string input, out Ast.Statement value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement140(input, out var old), old, input, false, out value);
	}

	/// <summary>Reads one statement at compatibility level 140 from a position, not looking past what it reads.</summary>
	public static Match<Ast.Statement> TryParseStatement140(string input, int at)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement140(input, at), input, locate: false);
	}

	/// <summary>Reads one statement at compatibility level 140 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseStatement140(string input, ref int at, out Ast.Statement value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement140(input, ref at, out var old), old, input, false, out value);
	}

	/// <summary>Reads one statement at compatibility level 140 in a window of the input.</summary>
	public static Match<Ast.Statement> TryParseStatement140(string input, int at, int length)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement140(input, at, length), input, locate: false);
	}

	/// <summary>Reads one statement at compatibility level 140 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseStatement140(string input, ref int at, int length, out Ast.Statement value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement140(input, ref at, length, out var old), old, input, false, out value);
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
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement150(input), input, locate: false);
	}

	/// <summary>Reads the whole input as one statement at compatibility level 150, answering only whether it is read.</summary>
	public static bool TryParseStatement150(string input, out Ast.Statement value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement150(input, out var old), old, input, false, out value);
	}

	/// <summary>Reads one statement at compatibility level 150 from a position, not looking past what it reads.</summary>
	public static Match<Ast.Statement> TryParseStatement150(string input, int at)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement150(input, at), input, locate: false);
	}

	/// <summary>Reads one statement at compatibility level 150 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseStatement150(string input, ref int at, out Ast.Statement value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement150(input, ref at, out var old), old, input, false, out value);
	}

	/// <summary>Reads one statement at compatibility level 150 in a window of the input.</summary>
	public static Match<Ast.Statement> TryParseStatement150(string input, int at, int length)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement150(input, at, length), input, locate: false);
	}

	/// <summary>Reads one statement at compatibility level 150 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseStatement150(string input, ref int at, int length, out Ast.Statement value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement150(input, ref at, length, out var old), old, input, false, out value);
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
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement160(input), input, locate: false);
	}

	/// <summary>Reads the whole input as one statement at compatibility level 160, answering only whether it is read.</summary>
	public static bool TryParseStatement160(string input, out Ast.Statement value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement160(input, out var old), old, input, false, out value);
	}

	/// <summary>Reads one statement at compatibility level 160 from a position, not looking past what it reads.</summary>
	public static Match<Ast.Statement> TryParseStatement160(string input, int at)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement160(input, at), input, locate: false);
	}

	/// <summary>Reads one statement at compatibility level 160 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseStatement160(string input, ref int at, out Ast.Statement value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement160(input, ref at, out var old), old, input, false, out value);
	}

	/// <summary>Reads one statement at compatibility level 160 in a window of the input.</summary>
	public static Match<Ast.Statement> TryParseStatement160(string input, int at, int length)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement160(input, at, length), input, locate: false);
	}

	/// <summary>Reads one statement at compatibility level 160 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseStatement160(string input, ref int at, int length, out Ast.Statement value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement160(input, ref at, length, out var old), old, input, false, out value);
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
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement170(input), input, locate: false);
	}

	/// <summary>Reads the whole input as one statement at compatibility level 170, answering only whether it is read.</summary>
	public static bool TryParseStatement170(string input, out Ast.Statement value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement170(input, out var old), old, input, false, out value);
	}

	/// <summary>Reads one statement at compatibility level 170 from a position, not looking past what it reads.</summary>
	public static Match<Ast.Statement> TryParseStatement170(string input, int at)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement170(input, at), input, locate: false);
	}

	/// <summary>Reads one statement at compatibility level 170 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseStatement170(string input, ref int at, out Ast.Statement value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement170(input, ref at, out var old), old, input, false, out value);
	}

	/// <summary>Reads one statement at compatibility level 170 in a window of the input.</summary>
	public static Match<Ast.Statement> TryParseStatement170(string input, int at, int length)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement170(input, at, length), input, locate: false);
	}

	/// <summary>Reads one statement at compatibility level 170 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseStatement170(string input, ref int at, int length, out Ast.Statement value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxStatement170(input, ref at, length, out var old), old, input, false, out value);
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
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql(input), input, locate: false);
	}

	/// <summary>Reads the whole input as a text of statements, answering only whether it is read.</summary>
	public static bool TryParseSql(string input, out Ast.Statement[] value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql(input, out var old), old, input, false, out value);
	}

	/// <summary>Reads a text of statements from a position, not looking past what it reads.</summary>
	public static Match<Ast.Statement[]> TryParseSql(string input, int at)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql(input, at), input, locate: false);
	}

	/// <summary>Reads a text of statements from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseSql(string input, ref int at, out Ast.Statement[] value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql(input, ref at, out var old), old, input, false, out value);
	}

	/// <summary>Reads a text of statements in a window of the input.</summary>
	public static Match<Ast.Statement[]> TryParseSql(string input, int at, int length)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql(input, at, length), input, locate: false);
	}

	/// <summary>Reads a text of statements in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseSql(string input, ref int at, int length, out Ast.Statement[] value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql(input, ref at, length, out var old), old, input, false, out value);
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
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql100(input), input, locate: false);
	}

	/// <summary>Reads the whole input as a text of statements at compatibility level 100, answering only whether it is read.</summary>
	public static bool TryParseSql100(string input, out Ast.Statement[] value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql100(input, out var old), old, input, false, out value);
	}

	/// <summary>Reads a text of statements at compatibility level 100 from a position, not looking past what it reads.</summary>
	public static Match<Ast.Statement[]> TryParseSql100(string input, int at)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql100(input, at), input, locate: false);
	}

	/// <summary>Reads a text of statements at compatibility level 100 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseSql100(string input, ref int at, out Ast.Statement[] value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql100(input, ref at, out var old), old, input, false, out value);
	}

	/// <summary>Reads a text of statements at compatibility level 100 in a window of the input.</summary>
	public static Match<Ast.Statement[]> TryParseSql100(string input, int at, int length)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql100(input, at, length), input, locate: false);
	}

	/// <summary>Reads a text of statements at compatibility level 100 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseSql100(string input, ref int at, int length, out Ast.Statement[] value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql100(input, ref at, length, out var old), old, input, false, out value);
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
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql110(input), input, locate: false);
	}

	/// <summary>Reads the whole input as a text of statements at compatibility level 110, answering only whether it is read.</summary>
	public static bool TryParseSql110(string input, out Ast.Statement[] value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql110(input, out var old), old, input, false, out value);
	}

	/// <summary>Reads a text of statements at compatibility level 110 from a position, not looking past what it reads.</summary>
	public static Match<Ast.Statement[]> TryParseSql110(string input, int at)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql110(input, at), input, locate: false);
	}

	/// <summary>Reads a text of statements at compatibility level 110 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseSql110(string input, ref int at, out Ast.Statement[] value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql110(input, ref at, out var old), old, input, false, out value);
	}

	/// <summary>Reads a text of statements at compatibility level 110 in a window of the input.</summary>
	public static Match<Ast.Statement[]> TryParseSql110(string input, int at, int length)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql110(input, at, length), input, locate: false);
	}

	/// <summary>Reads a text of statements at compatibility level 110 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseSql110(string input, ref int at, int length, out Ast.Statement[] value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql110(input, ref at, length, out var old), old, input, false, out value);
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
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql120(input), input, locate: false);
	}

	/// <summary>Reads the whole input as a text of statements at compatibility level 120, answering only whether it is read.</summary>
	public static bool TryParseSql120(string input, out Ast.Statement[] value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql120(input, out var old), old, input, false, out value);
	}

	/// <summary>Reads a text of statements at compatibility level 120 from a position, not looking past what it reads.</summary>
	public static Match<Ast.Statement[]> TryParseSql120(string input, int at)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql120(input, at), input, locate: false);
	}

	/// <summary>Reads a text of statements at compatibility level 120 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseSql120(string input, ref int at, out Ast.Statement[] value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql120(input, ref at, out var old), old, input, false, out value);
	}

	/// <summary>Reads a text of statements at compatibility level 120 in a window of the input.</summary>
	public static Match<Ast.Statement[]> TryParseSql120(string input, int at, int length)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql120(input, at, length), input, locate: false);
	}

	/// <summary>Reads a text of statements at compatibility level 120 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseSql120(string input, ref int at, int length, out Ast.Statement[] value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql120(input, ref at, length, out var old), old, input, false, out value);
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
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql130(input), input, locate: false);
	}

	/// <summary>Reads the whole input as a text of statements at compatibility level 130, answering only whether it is read.</summary>
	public static bool TryParseSql130(string input, out Ast.Statement[] value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql130(input, out var old), old, input, false, out value);
	}

	/// <summary>Reads a text of statements at compatibility level 130 from a position, not looking past what it reads.</summary>
	public static Match<Ast.Statement[]> TryParseSql130(string input, int at)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql130(input, at), input, locate: false);
	}

	/// <summary>Reads a text of statements at compatibility level 130 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseSql130(string input, ref int at, out Ast.Statement[] value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql130(input, ref at, out var old), old, input, false, out value);
	}

	/// <summary>Reads a text of statements at compatibility level 130 in a window of the input.</summary>
	public static Match<Ast.Statement[]> TryParseSql130(string input, int at, int length)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql130(input, at, length), input, locate: false);
	}

	/// <summary>Reads a text of statements at compatibility level 130 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseSql130(string input, ref int at, int length, out Ast.Statement[] value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql130(input, ref at, length, out var old), old, input, false, out value);
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
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql140(input), input, locate: false);
	}

	/// <summary>Reads the whole input as a text of statements at compatibility level 140, answering only whether it is read.</summary>
	public static bool TryParseSql140(string input, out Ast.Statement[] value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql140(input, out var old), old, input, false, out value);
	}

	/// <summary>Reads a text of statements at compatibility level 140 from a position, not looking past what it reads.</summary>
	public static Match<Ast.Statement[]> TryParseSql140(string input, int at)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql140(input, at), input, locate: false);
	}

	/// <summary>Reads a text of statements at compatibility level 140 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseSql140(string input, ref int at, out Ast.Statement[] value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql140(input, ref at, out var old), old, input, false, out value);
	}

	/// <summary>Reads a text of statements at compatibility level 140 in a window of the input.</summary>
	public static Match<Ast.Statement[]> TryParseSql140(string input, int at, int length)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql140(input, at, length), input, locate: false);
	}

	/// <summary>Reads a text of statements at compatibility level 140 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseSql140(string input, ref int at, int length, out Ast.Statement[] value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql140(input, ref at, length, out var old), old, input, false, out value);
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
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql150(input), input, locate: false);
	}

	/// <summary>Reads the whole input as a text of statements at compatibility level 150, answering only whether it is read.</summary>
	public static bool TryParseSql150(string input, out Ast.Statement[] value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql150(input, out var old), old, input, false, out value);
	}

	/// <summary>Reads a text of statements at compatibility level 150 from a position, not looking past what it reads.</summary>
	public static Match<Ast.Statement[]> TryParseSql150(string input, int at)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql150(input, at), input, locate: false);
	}

	/// <summary>Reads a text of statements at compatibility level 150 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseSql150(string input, ref int at, out Ast.Statement[] value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql150(input, ref at, out var old), old, input, false, out value);
	}

	/// <summary>Reads a text of statements at compatibility level 150 in a window of the input.</summary>
	public static Match<Ast.Statement[]> TryParseSql150(string input, int at, int length)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql150(input, at, length), input, locate: false);
	}

	/// <summary>Reads a text of statements at compatibility level 150 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseSql150(string input, ref int at, int length, out Ast.Statement[] value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql150(input, ref at, length, out var old), old, input, false, out value);
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
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql160(input), input, locate: false);
	}

	/// <summary>Reads the whole input as a text of statements at compatibility level 160, answering only whether it is read.</summary>
	public static bool TryParseSql160(string input, out Ast.Statement[] value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql160(input, out var old), old, input, false, out value);
	}

	/// <summary>Reads a text of statements at compatibility level 160 from a position, not looking past what it reads.</summary>
	public static Match<Ast.Statement[]> TryParseSql160(string input, int at)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql160(input, at), input, locate: false);
	}

	/// <summary>Reads a text of statements at compatibility level 160 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseSql160(string input, ref int at, out Ast.Statement[] value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql160(input, ref at, out var old), old, input, false, out value);
	}

	/// <summary>Reads a text of statements at compatibility level 160 in a window of the input.</summary>
	public static Match<Ast.Statement[]> TryParseSql160(string input, int at, int length)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql160(input, at, length), input, locate: false);
	}

	/// <summary>Reads a text of statements at compatibility level 160 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseSql160(string input, ref int at, int length, out Ast.Statement[] value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql160(input, ref at, length, out var old), old, input, false, out value);
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
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql170(input), input, locate: false);
	}

	/// <summary>Reads the whole input as a text of statements at compatibility level 170, answering only whether it is read.</summary>
	public static bool TryParseSql170(string input, out Ast.Statement[] value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql170(input, out var old), old, input, false, out value);
	}

	/// <summary>Reads a text of statements at compatibility level 170 from a position, not looking past what it reads.</summary>
	public static Match<Ast.Statement[]> TryParseSql170(string input, int at)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql170(input, at), input, locate: false);
	}

	/// <summary>Reads a text of statements at compatibility level 170 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseSql170(string input, ref int at, out Ast.Statement[] value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql170(input, ref at, out var old), old, input, false, out value);
	}

	/// <summary>Reads a text of statements at compatibility level 170 in a window of the input.</summary>
	public static Match<Ast.Statement[]> TryParseSql170(string input, int at, int length)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql170(input, at, length), input, locate: false);
	}

	/// <summary>Reads a text of statements at compatibility level 170 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
	public static bool TryParseSql170(string input, ref int at, int length, out Ast.Statement[] value)
	{
		using var reading = Bridge.Over(input, false);

		return Entries.Carry(Located.TryParseSyntaxSql170(input, ref at, length, out var old), old, input, false, out value);
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
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSelect(input), input, locate: true);
		}

		/// <summary>Reads the whole input as a query statement (no other kind), answering only whether it is read.</summary>
		public static bool TryParseSelect(string input, out Ast.Statement value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSelect(input, out var old), old, input, true, out value);
		}

		/// <summary>Reads a query statement (no other kind) from a position, not looking past what it reads.</summary>
		public static Match<Ast.Statement> TryParseSelect(string input, int at)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSelect(input, at), input, locate: true);
		}

		/// <summary>Reads a query statement (no other kind) from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseSelect(string input, ref int at, out Ast.Statement value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSelect(input, ref at, out var old), old, input, true, out value);
		}

		/// <summary>Reads a query statement (no other kind) in a window of the input.</summary>
		public static Match<Ast.Statement> TryParseSelect(string input, int at, int length)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSelect(input, at, length), input, locate: true);
		}

		/// <summary>Reads a query statement (no other kind) in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseSelect(string input, ref int at, int length, out Ast.Statement value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSelect(input, ref at, length, out var old), old, input, true, out value);
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
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxCondition(input), input, locate: true);
		}

		/// <summary>Reads the whole input as a search condition, answering only whether it is read.</summary>
		public static bool TryParseSearchCondition(string input, out Ast.Expression value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxCondition(input, out var old), old, input, true, out value);
		}

		/// <summary>Reads a search condition from a position, not looking past what it reads.</summary>
		public static Match<Ast.Expression> TryParseSearchCondition(string input, int at)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxCondition(input, at), input, locate: true);
		}

		/// <summary>Reads a search condition from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseSearchCondition(string input, ref int at, out Ast.Expression value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxCondition(input, ref at, out var old), old, input, true, out value);
		}

		/// <summary>Reads a search condition in a window of the input.</summary>
		public static Match<Ast.Expression> TryParseSearchCondition(string input, int at, int length)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxCondition(input, at, length), input, locate: true);
		}

		/// <summary>Reads a search condition in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseSearchCondition(string input, ref int at, int length, out Ast.Expression value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxCondition(input, ref at, length, out var old), old, input, true, out value);
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
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxExpression(input), input, locate: true);
		}

		/// <summary>Reads the whole input as an expression, answering only whether it is read.</summary>
		public static bool TryParseExpression(string input, out Ast.Expression value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxExpression(input, out var old), old, input, true, out value);
		}

		/// <summary>Reads an expression from a position, not looking past what it reads.</summary>
		public static Match<Ast.Expression> TryParseExpression(string input, int at)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxExpression(input, at), input, locate: true);
		}

		/// <summary>Reads an expression from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseExpression(string input, ref int at, out Ast.Expression value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxExpression(input, ref at, out var old), old, input, true, out value);
		}

		/// <summary>Reads an expression in a window of the input.</summary>
		public static Match<Ast.Expression> TryParseExpression(string input, int at, int length)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxExpression(input, at, length), input, locate: true);
		}

		/// <summary>Reads an expression in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseExpression(string input, ref int at, int length, out Ast.Expression value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxExpression(input, ref at, length, out var old), old, input, true, out value);
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
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement(input), input, locate: true);
		}

		/// <summary>Reads the whole input as one statement, answering only whether it is read.</summary>
		public static bool TryParseStatement(string input, out Ast.Statement value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement(input, out var old), old, input, true, out value);
		}

		/// <summary>Reads one statement from a position, not looking past what it reads.</summary>
		public static Match<Ast.Statement> TryParseStatement(string input, int at)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement(input, at), input, locate: true);
		}

		/// <summary>Reads one statement from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseStatement(string input, ref int at, out Ast.Statement value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement(input, ref at, out var old), old, input, true, out value);
		}

		/// <summary>Reads one statement in a window of the input.</summary>
		public static Match<Ast.Statement> TryParseStatement(string input, int at, int length)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement(input, at, length), input, locate: true);
		}

		/// <summary>Reads one statement in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseStatement(string input, ref int at, int length, out Ast.Statement value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement(input, ref at, length, out var old), old, input, true, out value);
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
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement100(input), input, locate: true);
		}

		/// <summary>Reads the whole input as one statement at compatibility level 100, answering only whether it is read.</summary>
		public static bool TryParseStatement100(string input, out Ast.Statement value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement100(input, out var old), old, input, true, out value);
		}

		/// <summary>Reads one statement at compatibility level 100 from a position, not looking past what it reads.</summary>
		public static Match<Ast.Statement> TryParseStatement100(string input, int at)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement100(input, at), input, locate: true);
		}

		/// <summary>Reads one statement at compatibility level 100 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseStatement100(string input, ref int at, out Ast.Statement value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement100(input, ref at, out var old), old, input, true, out value);
		}

		/// <summary>Reads one statement at compatibility level 100 in a window of the input.</summary>
		public static Match<Ast.Statement> TryParseStatement100(string input, int at, int length)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement100(input, at, length), input, locate: true);
		}

		/// <summary>Reads one statement at compatibility level 100 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseStatement100(string input, ref int at, int length, out Ast.Statement value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement100(input, ref at, length, out var old), old, input, true, out value);
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
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement110(input), input, locate: true);
		}

		/// <summary>Reads the whole input as one statement at compatibility level 110, answering only whether it is read.</summary>
		public static bool TryParseStatement110(string input, out Ast.Statement value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement110(input, out var old), old, input, true, out value);
		}

		/// <summary>Reads one statement at compatibility level 110 from a position, not looking past what it reads.</summary>
		public static Match<Ast.Statement> TryParseStatement110(string input, int at)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement110(input, at), input, locate: true);
		}

		/// <summary>Reads one statement at compatibility level 110 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseStatement110(string input, ref int at, out Ast.Statement value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement110(input, ref at, out var old), old, input, true, out value);
		}

		/// <summary>Reads one statement at compatibility level 110 in a window of the input.</summary>
		public static Match<Ast.Statement> TryParseStatement110(string input, int at, int length)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement110(input, at, length), input, locate: true);
		}

		/// <summary>Reads one statement at compatibility level 110 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseStatement110(string input, ref int at, int length, out Ast.Statement value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement110(input, ref at, length, out var old), old, input, true, out value);
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
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement120(input), input, locate: true);
		}

		/// <summary>Reads the whole input as one statement at compatibility level 120, answering only whether it is read.</summary>
		public static bool TryParseStatement120(string input, out Ast.Statement value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement120(input, out var old), old, input, true, out value);
		}

		/// <summary>Reads one statement at compatibility level 120 from a position, not looking past what it reads.</summary>
		public static Match<Ast.Statement> TryParseStatement120(string input, int at)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement120(input, at), input, locate: true);
		}

		/// <summary>Reads one statement at compatibility level 120 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseStatement120(string input, ref int at, out Ast.Statement value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement120(input, ref at, out var old), old, input, true, out value);
		}

		/// <summary>Reads one statement at compatibility level 120 in a window of the input.</summary>
		public static Match<Ast.Statement> TryParseStatement120(string input, int at, int length)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement120(input, at, length), input, locate: true);
		}

		/// <summary>Reads one statement at compatibility level 120 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseStatement120(string input, ref int at, int length, out Ast.Statement value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement120(input, ref at, length, out var old), old, input, true, out value);
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
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement130(input), input, locate: true);
		}

		/// <summary>Reads the whole input as one statement at compatibility level 130, answering only whether it is read.</summary>
		public static bool TryParseStatement130(string input, out Ast.Statement value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement130(input, out var old), old, input, true, out value);
		}

		/// <summary>Reads one statement at compatibility level 130 from a position, not looking past what it reads.</summary>
		public static Match<Ast.Statement> TryParseStatement130(string input, int at)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement130(input, at), input, locate: true);
		}

		/// <summary>Reads one statement at compatibility level 130 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseStatement130(string input, ref int at, out Ast.Statement value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement130(input, ref at, out var old), old, input, true, out value);
		}

		/// <summary>Reads one statement at compatibility level 130 in a window of the input.</summary>
		public static Match<Ast.Statement> TryParseStatement130(string input, int at, int length)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement130(input, at, length), input, locate: true);
		}

		/// <summary>Reads one statement at compatibility level 130 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseStatement130(string input, ref int at, int length, out Ast.Statement value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement130(input, ref at, length, out var old), old, input, true, out value);
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
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement140(input), input, locate: true);
		}

		/// <summary>Reads the whole input as one statement at compatibility level 140, answering only whether it is read.</summary>
		public static bool TryParseStatement140(string input, out Ast.Statement value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement140(input, out var old), old, input, true, out value);
		}

		/// <summary>Reads one statement at compatibility level 140 from a position, not looking past what it reads.</summary>
		public static Match<Ast.Statement> TryParseStatement140(string input, int at)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement140(input, at), input, locate: true);
		}

		/// <summary>Reads one statement at compatibility level 140 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseStatement140(string input, ref int at, out Ast.Statement value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement140(input, ref at, out var old), old, input, true, out value);
		}

		/// <summary>Reads one statement at compatibility level 140 in a window of the input.</summary>
		public static Match<Ast.Statement> TryParseStatement140(string input, int at, int length)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement140(input, at, length), input, locate: true);
		}

		/// <summary>Reads one statement at compatibility level 140 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseStatement140(string input, ref int at, int length, out Ast.Statement value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement140(input, ref at, length, out var old), old, input, true, out value);
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
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement150(input), input, locate: true);
		}

		/// <summary>Reads the whole input as one statement at compatibility level 150, answering only whether it is read.</summary>
		public static bool TryParseStatement150(string input, out Ast.Statement value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement150(input, out var old), old, input, true, out value);
		}

		/// <summary>Reads one statement at compatibility level 150 from a position, not looking past what it reads.</summary>
		public static Match<Ast.Statement> TryParseStatement150(string input, int at)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement150(input, at), input, locate: true);
		}

		/// <summary>Reads one statement at compatibility level 150 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseStatement150(string input, ref int at, out Ast.Statement value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement150(input, ref at, out var old), old, input, true, out value);
		}

		/// <summary>Reads one statement at compatibility level 150 in a window of the input.</summary>
		public static Match<Ast.Statement> TryParseStatement150(string input, int at, int length)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement150(input, at, length), input, locate: true);
		}

		/// <summary>Reads one statement at compatibility level 150 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseStatement150(string input, ref int at, int length, out Ast.Statement value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement150(input, ref at, length, out var old), old, input, true, out value);
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
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement160(input), input, locate: true);
		}

		/// <summary>Reads the whole input as one statement at compatibility level 160, answering only whether it is read.</summary>
		public static bool TryParseStatement160(string input, out Ast.Statement value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement160(input, out var old), old, input, true, out value);
		}

		/// <summary>Reads one statement at compatibility level 160 from a position, not looking past what it reads.</summary>
		public static Match<Ast.Statement> TryParseStatement160(string input, int at)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement160(input, at), input, locate: true);
		}

		/// <summary>Reads one statement at compatibility level 160 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseStatement160(string input, ref int at, out Ast.Statement value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement160(input, ref at, out var old), old, input, true, out value);
		}

		/// <summary>Reads one statement at compatibility level 160 in a window of the input.</summary>
		public static Match<Ast.Statement> TryParseStatement160(string input, int at, int length)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement160(input, at, length), input, locate: true);
		}

		/// <summary>Reads one statement at compatibility level 160 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseStatement160(string input, ref int at, int length, out Ast.Statement value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement160(input, ref at, length, out var old), old, input, true, out value);
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
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement170(input), input, locate: true);
		}

		/// <summary>Reads the whole input as one statement at compatibility level 170, answering only whether it is read.</summary>
		public static bool TryParseStatement170(string input, out Ast.Statement value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement170(input, out var old), old, input, true, out value);
		}

		/// <summary>Reads one statement at compatibility level 170 from a position, not looking past what it reads.</summary>
		public static Match<Ast.Statement> TryParseStatement170(string input, int at)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement170(input, at), input, locate: true);
		}

		/// <summary>Reads one statement at compatibility level 170 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseStatement170(string input, ref int at, out Ast.Statement value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement170(input, ref at, out var old), old, input, true, out value);
		}

		/// <summary>Reads one statement at compatibility level 170 in a window of the input.</summary>
		public static Match<Ast.Statement> TryParseStatement170(string input, int at, int length)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement170(input, at, length), input, locate: true);
		}

		/// <summary>Reads one statement at compatibility level 170 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseStatement170(string input, ref int at, int length, out Ast.Statement value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxStatement170(input, ref at, length, out var old), old, input, true, out value);
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
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql(input), input, locate: true);
		}

		/// <summary>Reads the whole input as a text of statements, answering only whether it is read.</summary>
		public static bool TryParseSql(string input, out Ast.Statement[] value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql(input, out var old), old, input, true, out value);
		}

		/// <summary>Reads a text of statements from a position, not looking past what it reads.</summary>
		public static Match<Ast.Statement[]> TryParseSql(string input, int at)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql(input, at), input, locate: true);
		}

		/// <summary>Reads a text of statements from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseSql(string input, ref int at, out Ast.Statement[] value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql(input, ref at, out var old), old, input, true, out value);
		}

		/// <summary>Reads a text of statements in a window of the input.</summary>
		public static Match<Ast.Statement[]> TryParseSql(string input, int at, int length)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql(input, at, length), input, locate: true);
		}

		/// <summary>Reads a text of statements in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseSql(string input, ref int at, int length, out Ast.Statement[] value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql(input, ref at, length, out var old), old, input, true, out value);
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
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql100(input), input, locate: true);
		}

		/// <summary>Reads the whole input as a text of statements at compatibility level 100, answering only whether it is read.</summary>
		public static bool TryParseSql100(string input, out Ast.Statement[] value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql100(input, out var old), old, input, true, out value);
		}

		/// <summary>Reads a text of statements at compatibility level 100 from a position, not looking past what it reads.</summary>
		public static Match<Ast.Statement[]> TryParseSql100(string input, int at)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql100(input, at), input, locate: true);
		}

		/// <summary>Reads a text of statements at compatibility level 100 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseSql100(string input, ref int at, out Ast.Statement[] value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql100(input, ref at, out var old), old, input, true, out value);
		}

		/// <summary>Reads a text of statements at compatibility level 100 in a window of the input.</summary>
		public static Match<Ast.Statement[]> TryParseSql100(string input, int at, int length)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql100(input, at, length), input, locate: true);
		}

		/// <summary>Reads a text of statements at compatibility level 100 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseSql100(string input, ref int at, int length, out Ast.Statement[] value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql100(input, ref at, length, out var old), old, input, true, out value);
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
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql110(input), input, locate: true);
		}

		/// <summary>Reads the whole input as a text of statements at compatibility level 110, answering only whether it is read.</summary>
		public static bool TryParseSql110(string input, out Ast.Statement[] value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql110(input, out var old), old, input, true, out value);
		}

		/// <summary>Reads a text of statements at compatibility level 110 from a position, not looking past what it reads.</summary>
		public static Match<Ast.Statement[]> TryParseSql110(string input, int at)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql110(input, at), input, locate: true);
		}

		/// <summary>Reads a text of statements at compatibility level 110 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseSql110(string input, ref int at, out Ast.Statement[] value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql110(input, ref at, out var old), old, input, true, out value);
		}

		/// <summary>Reads a text of statements at compatibility level 110 in a window of the input.</summary>
		public static Match<Ast.Statement[]> TryParseSql110(string input, int at, int length)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql110(input, at, length), input, locate: true);
		}

		/// <summary>Reads a text of statements at compatibility level 110 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseSql110(string input, ref int at, int length, out Ast.Statement[] value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql110(input, ref at, length, out var old), old, input, true, out value);
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
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql120(input), input, locate: true);
		}

		/// <summary>Reads the whole input as a text of statements at compatibility level 120, answering only whether it is read.</summary>
		public static bool TryParseSql120(string input, out Ast.Statement[] value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql120(input, out var old), old, input, true, out value);
		}

		/// <summary>Reads a text of statements at compatibility level 120 from a position, not looking past what it reads.</summary>
		public static Match<Ast.Statement[]> TryParseSql120(string input, int at)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql120(input, at), input, locate: true);
		}

		/// <summary>Reads a text of statements at compatibility level 120 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseSql120(string input, ref int at, out Ast.Statement[] value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql120(input, ref at, out var old), old, input, true, out value);
		}

		/// <summary>Reads a text of statements at compatibility level 120 in a window of the input.</summary>
		public static Match<Ast.Statement[]> TryParseSql120(string input, int at, int length)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql120(input, at, length), input, locate: true);
		}

		/// <summary>Reads a text of statements at compatibility level 120 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseSql120(string input, ref int at, int length, out Ast.Statement[] value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql120(input, ref at, length, out var old), old, input, true, out value);
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
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql130(input), input, locate: true);
		}

		/// <summary>Reads the whole input as a text of statements at compatibility level 130, answering only whether it is read.</summary>
		public static bool TryParseSql130(string input, out Ast.Statement[] value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql130(input, out var old), old, input, true, out value);
		}

		/// <summary>Reads a text of statements at compatibility level 130 from a position, not looking past what it reads.</summary>
		public static Match<Ast.Statement[]> TryParseSql130(string input, int at)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql130(input, at), input, locate: true);
		}

		/// <summary>Reads a text of statements at compatibility level 130 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseSql130(string input, ref int at, out Ast.Statement[] value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql130(input, ref at, out var old), old, input, true, out value);
		}

		/// <summary>Reads a text of statements at compatibility level 130 in a window of the input.</summary>
		public static Match<Ast.Statement[]> TryParseSql130(string input, int at, int length)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql130(input, at, length), input, locate: true);
		}

		/// <summary>Reads a text of statements at compatibility level 130 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseSql130(string input, ref int at, int length, out Ast.Statement[] value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql130(input, ref at, length, out var old), old, input, true, out value);
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
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql140(input), input, locate: true);
		}

		/// <summary>Reads the whole input as a text of statements at compatibility level 140, answering only whether it is read.</summary>
		public static bool TryParseSql140(string input, out Ast.Statement[] value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql140(input, out var old), old, input, true, out value);
		}

		/// <summary>Reads a text of statements at compatibility level 140 from a position, not looking past what it reads.</summary>
		public static Match<Ast.Statement[]> TryParseSql140(string input, int at)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql140(input, at), input, locate: true);
		}

		/// <summary>Reads a text of statements at compatibility level 140 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseSql140(string input, ref int at, out Ast.Statement[] value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql140(input, ref at, out var old), old, input, true, out value);
		}

		/// <summary>Reads a text of statements at compatibility level 140 in a window of the input.</summary>
		public static Match<Ast.Statement[]> TryParseSql140(string input, int at, int length)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql140(input, at, length), input, locate: true);
		}

		/// <summary>Reads a text of statements at compatibility level 140 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseSql140(string input, ref int at, int length, out Ast.Statement[] value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql140(input, ref at, length, out var old), old, input, true, out value);
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
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql150(input), input, locate: true);
		}

		/// <summary>Reads the whole input as a text of statements at compatibility level 150, answering only whether it is read.</summary>
		public static bool TryParseSql150(string input, out Ast.Statement[] value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql150(input, out var old), old, input, true, out value);
		}

		/// <summary>Reads a text of statements at compatibility level 150 from a position, not looking past what it reads.</summary>
		public static Match<Ast.Statement[]> TryParseSql150(string input, int at)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql150(input, at), input, locate: true);
		}

		/// <summary>Reads a text of statements at compatibility level 150 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseSql150(string input, ref int at, out Ast.Statement[] value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql150(input, ref at, out var old), old, input, true, out value);
		}

		/// <summary>Reads a text of statements at compatibility level 150 in a window of the input.</summary>
		public static Match<Ast.Statement[]> TryParseSql150(string input, int at, int length)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql150(input, at, length), input, locate: true);
		}

		/// <summary>Reads a text of statements at compatibility level 150 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseSql150(string input, ref int at, int length, out Ast.Statement[] value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql150(input, ref at, length, out var old), old, input, true, out value);
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
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql160(input), input, locate: true);
		}

		/// <summary>Reads the whole input as a text of statements at compatibility level 160, answering only whether it is read.</summary>
		public static bool TryParseSql160(string input, out Ast.Statement[] value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql160(input, out var old), old, input, true, out value);
		}

		/// <summary>Reads a text of statements at compatibility level 160 from a position, not looking past what it reads.</summary>
		public static Match<Ast.Statement[]> TryParseSql160(string input, int at)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql160(input, at), input, locate: true);
		}

		/// <summary>Reads a text of statements at compatibility level 160 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseSql160(string input, ref int at, out Ast.Statement[] value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql160(input, ref at, out var old), old, input, true, out value);
		}

		/// <summary>Reads a text of statements at compatibility level 160 in a window of the input.</summary>
		public static Match<Ast.Statement[]> TryParseSql160(string input, int at, int length)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql160(input, at, length), input, locate: true);
		}

		/// <summary>Reads a text of statements at compatibility level 160 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseSql160(string input, ref int at, int length, out Ast.Statement[] value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql160(input, ref at, length, out var old), old, input, true, out value);
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
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql170(input), input, locate: true);
		}

		/// <summary>Reads the whole input as a text of statements at compatibility level 170, answering only whether it is read.</summary>
		public static bool TryParseSql170(string input, out Ast.Statement[] value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql170(input, out var old), old, input, true, out value);
		}

		/// <summary>Reads a text of statements at compatibility level 170 from a position, not looking past what it reads.</summary>
		public static Match<Ast.Statement[]> TryParseSql170(string input, int at)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql170(input, at), input, locate: true);
		}

		/// <summary>Reads a text of statements at compatibility level 170 from a position, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseSql170(string input, ref int at, out Ast.Statement[] value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql170(input, ref at, out var old), old, input, true, out value);
		}

		/// <summary>Reads a text of statements at compatibility level 170 in a window of the input.</summary>
		public static Match<Ast.Statement[]> TryParseSql170(string input, int at, int length)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql170(input, at, length), input, locate: true);
		}

		/// <summary>Reads a text of statements at compatibility level 170 in a window of the input, answering only whether it is read; <paramref name="at"/> moves past it.</summary>
		public static bool TryParseSql170(string input, ref int at, int length, out Ast.Statement[] value)
		{
			using var reading = Bridge.Over(input, true);

			return Entries.Carry(Located.TryParseSyntaxSql170(input, ref at, length, out var old), old, input, true, out value);
		}
	}

	/// <summary>
	/// What the public entries share: a reading of the T-SQL tree carried onto the common tree. Every entry reads through
	/// the located door, since the bridge reads the words the T-SQL tree drops from the text at the old nodes' spans; the
	/// plain door's tree is told no span.
	/// </summary>
	static class Entries
	{
		/// <summary>What a refusal says where the reading had nothing of its own to say.</summary>
		const string Otherwise = "Input does not match.";

		public static Match<Ast.Statement> Carry(Match<Statement> read, string input, bool locate)
		{
			return read.IsSuccess
				? Match<Ast.Statement>.Success(Bridge.Carried(read.Value, input, locate), read.Position, read.Length, read.ReadingEnd)
				: read.Refusal<Ast.Statement>(read.Position, Otherwise);
		}

		public static Match<Ast.Expression> Carry(Match<Expression> read, string input, bool locate)
		{
			return read.IsSuccess
				? Match<Ast.Expression>.Success(Bridge.Carried(read.Value, input, locate), read.Position, read.Length, read.ReadingEnd)
				: read.Refusal<Ast.Expression>(read.Position, Otherwise);
		}

		public static Match<Ast.Statement[]> Carry(Match<Statement[]> read, string input, bool locate)
		{
			return read.IsSuccess
				? Match<Ast.Statement[]>.Success(Bridge.Carried(read.Value, input, locate), read.Position, read.Length, read.ReadingEnd)
				: read.Refusal<Ast.Statement[]>(read.Position, Otherwise);
		}

		public static bool Carry(bool read, Statement old, string input, bool locate, out Ast.Statement value)
		{
			value = read ? Bridge.Carried(old, input, locate) : null!;

			return read;
		}

		public static bool Carry(bool read, Expression old, string input, bool locate, out Ast.Expression value)
		{
			value = read ? Bridge.Carried(old, input, locate) : null!;

			return read;
		}

		public static bool Carry(bool read, Statement[] old, string input, bool locate, out Ast.Statement[] value)
		{
			value = read ? Bridge.Carried(old, input, locate) : null!;

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
