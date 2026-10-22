using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using DotGram.Sql.TransactSql;

using Xunit;

namespace DotGram.Sql.Tests;

/// <summary>
/// The spellings of a data type, a table name, an alias and a column reference that the builder takes apart again from
/// text: every standard and system spelling, bracketed, quoted and qualified ones, user types with arguments and
/// <c>MAX</c>, and the shapes of names, in the places a type or a name stands. They are held to what every reading of a
/// text has to agree on: nothing throws, and the plain and the located reading make equal trees. The same file is the input
/// a change to how the builder reads these spellings is dumped over before and after.
/// </summary>
public sealed class BuilderShapeTests
{
	public static IEnumerable<object[]> Inputs()
	{
		var path = Path.Combine(Path.GetDirectoryName(Here())!, "BuilderShapes.txt");

		foreach (var line in File.ReadAllLines(path, Encoding.UTF8))
			if (line.Length > 0)
				yield return [Unescaped(line)];
	}

	[Fact]
	public void The_file_holds_the_shapes()
	{
		Assert.True(Inputs().Count() > 3000);
	}

	[Theory]
	[MemberData(nameof(Inputs))]
	public void Every_shape_reads_the_same_plain_and_located(string text)
	{
		var plain   = TransactSqlParser.TryParseSql(text);
		var located = TransactSqlParser.Located.TryParseSql(text);

		Assert.Equal(plain.IsSuccess, located.IsSuccess);

		if (!plain.IsSuccess)
			return;

		Assert.Equal(plain.Value.Length, located.Value.Length);

		for (var at = 0; at < plain.Value.Length; at++)
			Assert.Equal(plain.Value[at], located.Value[at]);
	}

	static string Unescaped(string line)
	{
		var made = new StringBuilder(line.Length);

		for (var at = 0; at < line.Length; at++)
		{
			if (line[at] != '\\' || at + 1 >= line.Length)
			{
				made.Append(line[at]);

				continue;
			}

			var next = line[++at];

			made.Append(next switch { 'n' => '\n', 'r' => '\r', 't' => '\t', _ => next });
		}

		return made.ToString();
	}

	static string Here([System.Runtime.CompilerServices.CallerFilePath] string here = "")
	{
		return here;
	}
}
