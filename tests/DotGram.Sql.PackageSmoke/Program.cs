using System;
using System.Runtime.InteropServices;

using DotGram.Sql;
using DotGram.Sql.Standard;
using DotGram.Sql.TransactSql;

var select = TransactSqlParser.ParseSelect("select name from Users where id > @id");
if (select is not Statement.Select { Of: Query.Specification })
	throw new Exception("Expected a T-SQL select.");
var written = SqlWriter.Write(select);
if (SqlWriter.Write(TransactSqlParser.ParseSelect(written)) != written)
	throw new Exception("T-SQL did not survive a writer round trip.");
var standard = SqlStandardParser.ParseQueryExpression("SELECT a FROM t WHERE a > 1");
var standardText = DotGram.Sql.Ast.Sql2023Writer.Write(standard);
if (DotGram.Sql.Ast.Sql2023Writer.Write(SqlStandardParser.ParseQueryExpression(standardText)) != standardText)
	throw new Exception("SQL:2023 did not survive a writer round trip.");
if (Sql92Parser.ParseValueExpression("1 + 2") is null)
	throw new Exception("Expected a SQL-92 expression.");
Console.WriteLine($"DotGram.Sql package smoke: three dialects parsed ({RuntimeInformation.FrameworkDescription}).");
