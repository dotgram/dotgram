using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using DotGram.Sql.Ast;

using Xunit;

namespace DotGram.Sql.Tests;

// Inside this namespace an unqualified name is the OLD tree's, since `DotGram.Sql` encloses
// `DotGram.Sql.Tests`. Both trees have an `Expression`, a `DataType` and a `Statement`, so the names
// are bound here as Both.cs binds them. This is the hazard SKILL.md names as "two trees with the
// same names", met from inside the repository rather than by a consumer.
using DataType = DotGram.Sql.Ast.DataType;
using Expression = DotGram.Sql.Ast.Expression;
using Identifier = DotGram.Sql.Ast.Identifier;
using LiteralValue = DotGram.Sql.Ast.LiteralValue;
using QualifiedName = DotGram.Sql.Ast.QualifiedName;
using Statement = DotGram.Sql.Ast.Statement;

/// <summary>
/// That the writer has text for every value of every enum it prints, and that every flag it is given
/// changes what it prints.
/// </summary>
/// <remarks>
/// <para>
/// Written because it was not there. <c>Sql2023Writer</c> ends seventeen switches in a catch-all that
/// returns a literal and throws nowhere, so a value added to one of those enums printed as the last
/// arm: <c>ComparisonOperator.NotLess</c> and <c>NotGreater</c>, added on 2026-09-26, both wrote as
/// <c>&gt;=</c>. Valid SQL with another meaning, which a round trip accepts as readily as the right
/// text, and nothing failed because nothing built those values yet.
/// </para>
/// <para>
/// So: every value of a covered enum writes something, and no two values of one enum write the same
/// text unless this file says which two and why. A flag is the same defect wearing different clothes
/// — an arm nothing reaches and a flag nothing reads are both a fact the tree holds and the text
/// loses — so each one is asserted to change the output.
/// </para>
/// </remarks>
public sealed class Sql2023WriterCoverageTests
{
	static readonly Expression A = new Expression.Reference(new QualifiedName([new Identifier("a")]));
	static readonly Expression B = new Expression.Reference(new QualifiedName([new Identifier("b")]));

	/// <summary>The enums this file covers, each with the smallest node that puts a value in the text.</summary>
	static readonly Dictionary<Type, Func<object, ISqlNode>> Covered = new()
	{
		[typeof(ComparisonOperator)] = v => new Expression.Comparison(A, (ComparisonOperator)v, B),
		[typeof(BinaryOperator)]     = v => new Expression.Binary(A, (BinaryOperator)v, B),
		[typeof(UnaryOperator)]      = v => new Expression.Unary((UnaryOperator)v, A),
		[typeof(LikeKind)]           = v => new Expression.Like(A, false, (LikeKind)v, B),
		[typeof(NumericLiteralKind)] = v => new Expression.Literal(new LiteralValue.Numeric("1", (NumericLiteralKind)v)),
		[typeof(DateTimeLiteralKind)] = v => new Expression.Literal(new LiteralValue.DateTime((DateTimeLiteralKind)v, "2026-09-26")),
		[typeof(NumericTypeKind)]    = v => new DataType.Numeric((NumericTypeKind)v),
		[typeof(DateTimeTypeKind)]   = v => new DataType.DateTime((DateTimeTypeKind)v),
		[typeof(CharacterTypeKind)]  = v => new DataType.Character((CharacterTypeKind)v),
		[typeof(BinaryTypeKind)]     = v => new DataType.Binary((BinaryTypeKind)v),
		[typeof(IdentifierStyle)]    = v => new Identifier("x", (IdentifierStyle)v),
	};

	/// <summary>
	/// Enums whose values do not decide the text, and why. Distinctness is not asked of them.
	/// </summary>
	/// <remarks>
	/// A kind that the record's own text already says is not a fact the writer has to print: a
	/// <c>LiteralValue.Numeric</c> carries <c>"0xFF"</c> and the writer prints that, so
	/// <c>HexInteger</c> and <c>Decimal</c> write the same thing for the same text and should. Which
	/// also says something useful about the amendments: <c>NumericLiteralKind.Money</c> needs no arm in
	/// the writer at all, where <c>BinaryOperator.Modulo</c> needed one.
	/// </remarks>
	static readonly Dictionary<string, string> TextDecidesInstead = new(StringComparer.Ordinal)
	{
		["NumericLiteralKind"] = "LiteralValue.Numeric carries the literal's text and the kind is derivable from it",
		["IdentifierStyle"] = "the grammar captures a delimited name WITH its delimiters -- `DelimitedIdentifier = '\"' & ... & '\"'` -- so Identifier.Text holds them and the style is derivable from the first character, which is why PutIdentifier prints the text raw and is right to",
	};

	[Fact]
	public void Every_value_of_a_covered_enum_writes_text()
	{
		var silent = new List<string>();

		foreach (var (type, build) in Covered)
			foreach (var value in Enum.GetValues(type))
				if (string.IsNullOrWhiteSpace(Write(build, value)))
					silent.Add(type.Name + "." + value);

		Assert.Empty(silent);
	}

	[Fact]
	public void No_two_values_of_one_enum_write_the_same_text()
	{
		var shared = new List<string>();

		foreach (var (type, build) in Covered)
		{
			if (TextDecidesInstead.ContainsKey(type.Name))
				continue;

			var seen = new Dictionary<string, string>(StringComparer.Ordinal);

			foreach (var value in Enum.GetValues(type))
			{
				var text = Write(build, value) ?? "";
				var name = type.Name + "." + value;

				if (seen.TryGetValue(text, out var first))
					shared.Add(name + " writes what " + first + " writes: " + text);
				else
					seen[text] = name;
			}
		}

		Assert.Empty(shared);
	}

	/// <summary>A supplement, not the gate: that no switch in the writer ends in a catch-all.</summary>
	/// <remarks>
	/// <b>This is not the check that protects the writer.</b> It reads the source for <c>_ =&gt; "..."</c>,
	/// and it passes on <c>Word(op == UnaryOperator.Minus ? "-" : "+")</c>, which wrote T-SQL's <c>~</c>
	/// as <c>+</c> — the same defect with no catch-all in it. The gate is
	/// <see cref="No_two_values_of_one_enum_write_the_same_text"/>, which walks values through the writer
	/// and reads what comes out. This one only keeps a catch-all from being reintroduced, which the
	/// output check cannot see until an enum gains a value.
	/// </remarks>
	[Fact]
	public void No_switch_in_the_writer_ends_in_a_catch_all()
	{
		var source = File.ReadAllText(Path.Combine(Repository(), "src", "DotGram.Sql", "Standard", "Sql2023Writer.cs"));
		var lines  = source.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
		var loose  = new List<string>();

		// Padded to align with the arms above it, so the spelling is matched loosely: a detector that
		// only knew `_ => "` counted 17 of these where there were 31.
		for (var at = 0; at < lines.Length; at++)
			if (System.Text.RegularExpressions.Regex.IsMatch(lines[at], "_ +=> +\"|_ => \""))
				loose.Add("Sql2023Writer.cs(" + (at + 1) + "): " + lines[at].Trim());

		Assert.Empty(loose);
	}

	static string Repository([System.Runtime.CompilerServices.CallerFilePath] string here = "")
	{
		return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", ".."));
	}

	/// <summary>That each named exception is true of the writer, and not merely asserted by this file.</summary>
	/// <remarks>
	/// An exception says the enum does not decide the text because the record carries it. That claim is
	/// testable: change the text and the output must change. Without this the list could excuse a real
	/// defect by describing one, which is how a skip with a reason becomes a skip.
	/// </remarks>
	[Fact]
	public void Where_the_text_decides_it_decides()
	{
		var claims = new Dictionary<string, (ISqlNode One, ISqlNode Two)>(StringComparer.Ordinal)
		{
			["NumericLiteralKind"] = (
				new Expression.Literal(new LiteralValue.Numeric("1", NumericLiteralKind.Decimal)),
				new Expression.Literal(new LiteralValue.Numeric("0xFF", NumericLiteralKind.Decimal))),
			["IdentifierStyle"] = (
				new Identifier("a", IdentifierStyle.Delimited),
				new Identifier("\"my column\"", IdentifierStyle.Delimited)),
		};

		// Every exception is asserted and nothing else is: a new one cannot be added without its claim.
		Assert.Equal(
			TextDecidesInstead.Keys.OrderBy(one => one, StringComparer.Ordinal),
			claims.Keys.OrderBy(one => one, StringComparer.Ordinal));

		foreach (var (name, pair) in claims)
			Assert.NotEqual(Sql2023Writer.Write(pair.One), Sql2023Writer.Write(pair.Two));

		// And the delimiters the style is derived from are in the text, which is why they are written.
		Assert.Contains("\"", Sql2023Writer.Write(claims["IdentifierStyle"].Two), StringComparison.Ordinal);
	}

	[Theory]
	[InlineData("Max on a character type")]
	[InlineData("Max on a binary type")]
	[InlineData("Exclamation on a comparison")]
	public void A_flag_changes_what_is_written(string which)
	{
		var (without, with) = which switch
		{
			"Max on a character type" => (
				Sql2023Writer.Write(new DataType.Character(CharacterTypeKind.Varchar)),
				Sql2023Writer.Write(new DataType.Character(CharacterTypeKind.Varchar, Max: true))),
			"Max on a binary type" => (
				Sql2023Writer.Write(new DataType.Binary(BinaryTypeKind.Varbinary)),
				Sql2023Writer.Write(new DataType.Binary(BinaryTypeKind.Varbinary, Max: true))),
			_ => (
				Sql2023Writer.Write(new Expression.Comparison(A, ComparisonOperator.NotEqual, B)),
				Sql2023Writer.Write(new Expression.Comparison(A, ComparisonOperator.NotEqual, B, Exclamation: true))),
		};

		Assert.NotEqual(without, with);
	}

	// A value the writer refuses outright is not a silent one, so a throw is read as "it has text for
	// this and declines to print it here", which is what the throwing default is for.
	static string? Write(Func<object, ISqlNode> build, object value)
	{
		try
		{
			return Sql2023Writer.Write(build(value));
		}
		catch (Exception)
		{
			return "(refused)";
		}
	}
}
