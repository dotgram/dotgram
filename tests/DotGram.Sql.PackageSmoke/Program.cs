using System;
using System.Runtime.InteropServices;

using DotGram.Sql.Standard;
using DotGram.Sql.TransactSql;

var select = TransactSqlParser.ParseSelect("select name from Users where id > @id");
if (select is not DotGram.Sql.Ast.Statement.Select { Query: DotGram.Sql.Ast.Query.Select })
	throw new Exception("Expected a T-SQL select.");
var written = DotGram.Sql.Ast.TransactSqlWriter.Write(select);
if (DotGram.Sql.Ast.TransactSqlWriter.Write(TransactSqlParser.ParseSelect(written)) != written)
	throw new Exception("T-SQL did not survive a writer round trip.");
var standard = SqlStandardParser.ParseStatement("SELECT a FROM t WHERE a > 1");
var standardText = DotGram.Sql.Ast.SqlWriter.Write(standard);
if (DotGram.Sql.Ast.SqlWriter.Write(SqlStandardParser.ParseStatement(standardText)) != standardText)
	throw new Exception("SQL:2023 did not survive a writer round trip.");
if (Sql92Parser.ParseValueExpression("1 + 2") is null)
	throw new Exception("Expected a SQL-92 expression.");
Console.WriteLine($"DotGram.Sql package smoke: three dialects parsed ({RuntimeInformation.FrameworkDescription}).");
