using System;

using DotGram.Sql.TransactSql;

using Xunit;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.Tests;

/// <summary>
/// A statement read through the public located entry, written by <c>TransactSqlWriter</c> and read back as the same
/// tree: the round trip a test of what a text builds holds it to, so that a record or a member the writer does not
/// print, or prints as another, fails where the expected shape alone would pass.
/// </summary>
static class ReadBack
{
	/// <summary>
	/// The statement <paramref name="text"/> reads as, which must be no extension, and whose written text must read
	/// back, as one statement, into an equal tree (spans aside), and so again.
	/// </summary>
	public static Ast.Statement Holds(string text)
	{
		var made = TransactSqlParser.Located.ParseStatement(text);

		Assert.False(made is Ast.Statement.Extension, $"{text}: read as an extension, {made}");

		var written = Ast.TransactSqlWriter.Write(made);
		var back    = TransactSqlParser.Located.TryParseSql(written);

		Assert.True(back.IsSuccess && back.Value!.Length == 1, $"{text}: written as {written}, which does not read back as one statement");
		Assert.True(made.Equals(back.Value![0]), $"{text}: written as {written}, which reads back as {back.Value[0]}");
		Assert.Equal(written, Ast.TransactSqlWriter.Write(back.Value[0]));

		return made;
	}
}
