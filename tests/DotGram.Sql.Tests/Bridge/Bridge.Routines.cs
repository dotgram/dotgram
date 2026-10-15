using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.Tests;

static partial class Bridge
{
	/// <summary>
	/// Procedures, functions, triggers, EXECUTE, and the module-like objects defined beside them.
	/// </summary>
	/// <remarks>
	/// Every record throws the gap its inventory row names; sequences, synonyms and types are the Tables family's.
	/// </remarks>
	internal static class Routines
	{
		public static readonly string[] Owns =
		[
			"Statement.CreateProcedure", "Statement.CreateFunction", "Statement.CreateTrigger",
			"Statement.Execute", "Statement.TriggerSwitch", "Statement.AggregateDefinition",
			"Statement.AssemblyDefinition", "Statement.RuleDefinition", "Statement.DefaultDefinition",
			"Statement.XmlSchemaCollectionDefinition", "Statement.AlterXmlSchemaCollection", "Clause.ParameterDeclaration",
			"Clause.ExecutionContext", "Query.FromExecute",
		];

		public static bool TryConvert(object old, [NotNullWhen(true)] out Ast.ISqlNode? made)
		{
			made = old switch
			{
				Statement.Execute execute => Execute(execute),
				Query.FromExecute         => throw BridgeGap.Open(old, "inventory Query.FromExecute (outside an INSERT)"),
				_ when Owned(Owns, old) => throw Inventory.Unconverted(old),
				_                       => null,
			};

			return made is not null;
		}

		/// <summary>
		/// INSERT's rows a call returns, <c>INSERT t EXEC …</c>: the call as the statement it is, never bare.
		/// </summary>
		internal static Ast.InsertSource InsertSource(Query.FromExecute old)
		{
			return Execute(old.Execute, Spelled(old)) switch
			{
				Ast.Statement.ExecuteModule module => new Ast.InsertSource.ExecuteModule(module),
				Ast.Statement.ExecuteString text   => new Ast.InsertSource.ExecuteString(text),
				_                                  => throw BridgeGap.Open(old, "inventory Query.FromExecute"),
			};
		}

		/// <summary>
		/// EXECUTE of a string, <c>EXEC ('…', v) AS USER = 'u' AT s</c>, or of a module, <c>EXEC @rc = p;1 a, @b = c
		/// OUTPUT AT s WITH …</c>, or the bare call. The old tree keeps neither EXEC against EXECUTE, which is read from
		/// the text, nor the WITH but as its text, which is read again here.
		/// </summary>
		static Ast.Statement Execute(Statement.Execute old, string? inside = null)
		{
			// Inside an INSERT the call has no span of its own; the rows' span begins with it.
			var spelled = inside ?? Spelled(old);
			var options = ExecuteOptions(old);

			if (old.Name == "(")
			{
				if (old.Arguments is not [var sql, .. var values] || old.Into is not null || old.Number is not null || old.Server is not null || old.Bare)
					throw BridgeGap.Open(old, "inventory Statement.Execute (a string run with a module's parts)");

				return new Ast.Statement.ExecuteString(Values.Of(sql), Each(values, Values.Of))
				{
					Exec       = Recover("Statement.ExecuteString.Exec", spelled, static text => Words(text) is ["EXEC", ..], false),
					Context    = old.Context is null ? null : Context(old, old.Context),
					At         = old.At is null ? null : Values.Part(old, old.At),
					DataSource = old.DataSource is null ? null : Values.Part(old, old.DataSource),
					Options    = options,
				};
			}

			if (old.Context is not null || old.DataSource is not null)
				throw BridgeGap.Open(old, "inventory Statement.Execute (a module with a string's parts)");

			Ast.ModuleReference module = old.Name.StartsWith('@')
				? new Ast.ModuleReference.Variable(new Ast.Identifier(old.Name))
				: new Ast.ModuleReference.Named(Values.Name(old, old.Name))
				{
					DataSource = old.Server is { Name: var server, Arguments: [var provider, var init] } && Words(server) is ["OPENDATASOURCE"]
						? new Ast.OpenDataSource(Values.Of(provider), Values.Of(init))
						: old.Server is null ? null : throw BridgeGap.Open(old, "inventory Statement.Execute (a server other than OPENDATASOURCE)"),
				};

			var keyword = old.Bare ? (Ast.ExecuteKeyword?)null
				: Recover("Statement.ExecuteModule.Keyword", spelled, static text => Words(text) is ["EXEC", ..] ? Ast.ExecuteKeyword.Exec : Ast.ExecuteKeyword.Execute, Ast.ExecuteKeyword.Execute);

			// SQL:2023 reads `EXECUTE name` alone as its EXECUTE of a prepared statement: another meaning, not one tree.
			if (inside is null && keyword == Ast.ExecuteKeyword.Execute && module is Ast.ModuleReference.Named { DataSource: null }
				&& old is { Arguments.Length: 0, Into: null, Number: null, At: null, Tail: null })
				throw BridgeGap.Open(old, "needs decision: EXECUTE name alone is also the standard's EXECUTE of a prepared statement (shape)");

			return new Ast.Statement.ExecuteModule(module, Each(old.Arguments, one => ExecuteArgument(old, one)))
			{
				Keyword      = keyword,
				ReturnStatus = old.Into is null ? null : new Ast.Identifier(old.Into),
				Number       = old.Number is null ? null : new Ast.LiteralValue.Numeric(old.Number, Ast.NumericLiteralKind.DecimalInteger),
				At           = old.At is null ? null : Values.Part(old, old.At),
				Options      = options,
			};
		}

		/// <summary>A string's context, <c>AS LOGIN = 'x'</c> or <c>AS USER = N'u'</c>.</summary>
		static Ast.ExecutionContext Context(Statement old, Clause context)
		{
			return context switch
			{
				Clause.ExecutionContext("LOGIN", var name) => new Ast.ExecutionContext(Ast.ExecutionContextKind.Login, Values.Of(name)),
				Clause.ExecutionContext("USER", var name)  => new Ast.ExecutionContext(Ast.ExecutionContextKind.User, Values.Of(name)),
				_ => throw BridgeGap.Open(old, $"inventory {NameOf(context)}"),
			};
		}

		/// <summary>One value passed: <c>@p = v</c>, and OUTPUT or OUT after it, which the old tree keeps as a hint.</summary>
		static Ast.ExecuteArgument ExecuteArgument(Statement old, Expression value)
		{
			Ast.OutputKeyword? output = null;

			if (value is Expression.Hinted(var hinted, var words))
			{
				output = Words(words) switch
				{
					["OUTPUT"] => Ast.OutputKeyword.Output,
					["OUT"]    => Ast.OutputKeyword.Out,
					_          => throw BridgeGap.Open(old, $"inventory Expression.Hinted (`{words}` after an argument)"),
				};

				value = hinted;
			}

			return value is Expression.NamedArgument(var name, var named)
				? new Ast.ExecuteArgument(Values.Of(named)) { Name = new Ast.Identifier(name), Output = output }
				: new Ast.ExecuteArgument(Values.Of(value)) { Output = output };
		}

		/// <summary>
		/// The WITH after a call, which the old tree keeps as written: cut at its commas outside brackets, each part a
		/// word or RESULT SETS, a set's columns read again as a table's. Only a WITH written as the writer prints it
		/// carries over, the old tree comparing it exactly.
		/// </summary>
		static SqlList<Ast.ExecuteOption> ExecuteOptions(Statement.Execute old)
		{
			if (old.Tail is not { } tail)
				return [];

			var tokens = SqlTokens.Of(tail);

			if (tokens is not [{ Kind: SqlTokens.Kind.Word } with, ..] || !with.Text.Equals("WITH", StringComparison.OrdinalIgnoreCase))
				throw BridgeGap.Open(old, $"inventory Statement.Execute (the tail `{tail}`)");

			var options = new List<Ast.ExecuteOption>();

			foreach (var part in Parts(tokens, 1, tokens.Count))
			{
				var words = part.Select(static one => one.Kind == SqlTokens.Kind.Word ? one.Text.ToUpperInvariant() : one.Text).ToArray();

				options.Add(words switch
				{
					[_] when part[0].Kind is SqlTokens.Kind.Word or SqlTokens.Kind.Delimited => new Ast.ExecuteOption.Named(Values.Part(old, part[0].Text)),
					["RESULT", "SETS", "UNDEFINED"] => new Ast.ExecuteOption.ResultSets(Ast.ResultSetsKind.Undefined, []),
					["RESULT", "SETS", "NONE"]      => new Ast.ExecuteOption.ResultSets(Ast.ResultSetsKind.None, []),
					["RESULT", "SETS", "(", .., ")"] => new Ast.ExecuteOption.ResultSets(Ast.ResultSetsKind.Described, [.. Parts(part, 3, part.Count - 1).Select(set => ResultSet(old, set))]),
					_ => throw BridgeGap.Open(old, $"inventory Statement.Execute (the option `{string.Join(' ', words)}`)"),
				});
			}

			var made    = options.ToSqlList();
			var printed = Ast.Sql2023Writer.Write(new Ast.Statement.ExecuteModule(new Ast.ModuleReference.Named(new Ast.QualifiedName([new Ast.Identifier("p")])), []) { Keyword = Ast.ExecuteKeyword.Exec, Options = made });

			if (printed != "EXEC p " + tail)
				throw BridgeGap.Open(old, "Trees.FreeText (Statement.Execute.Tail, written otherwise than the writer prints it)");

			return made;
		}

		/// <summary>One result set: its columns, read again as a table's, or AS OBJECT, AS TYPE, AS FOR XML.</summary>
		static Ast.ResultSet ResultSet(Statement old, List<SqlTokens.Token> set)
		{
			var words = set.Select(static one => one.Kind == SqlTokens.Kind.Word ? one.Text.ToUpperInvariant() : one.Text).ToArray();
			var rest  = string.Join(' ', set.Skip(2).Select(static one => one.Text));

			switch (words)
			{
				case ["AS", "OBJECT", ..]:
					return new Ast.ResultSet.AsObject(Values.Name(old, rest.Replace(" . ", ".", StringComparison.Ordinal)));

				case ["AS", "TYPE", ..]:
					return new Ast.ResultSet.AsType(Values.Name(old, rest.Replace(" . ", ".", StringComparison.Ordinal)));

				case ["AS", "FOR", "XML"]:
					return new Ast.ResultSet.AsForXml();

				case ["(", .., ")"]:
					var body = string.Join(' ', set.Skip(1).Take(set.Count - 2).Select(static one => one.Text));

					return TransactSql.TransactSqlParser.TryParseSql("CREATE TABLE t (" + body + ")") is { IsSuccess: true, Value: [Statement.TableDefinition table] }
						&& StatementOf(table) is Ast.Statement.CreateTable { Contents: Ast.TableContents.Elements(var elements) }
						? new Ast.ResultSet.Columns(elements)
						: throw BridgeGap.Open(old, $"inventory Statement.Execute (a result set `{body}`)");

				default:
					throw BridgeGap.Open(old, $"inventory Statement.Execute (a result set `{string.Join(' ', words)}`)");
			}
		}

		/// <summary>The tokens from <paramref name="from"/> to <paramref name="to"/> cut at their commas outside brackets.</summary>
		static List<List<SqlTokens.Token>> Parts(List<SqlTokens.Token> tokens, int from, int to)
		{
			var parts = new List<List<SqlTokens.Token>> { new() };
			var depth = 0;

			for (var at = from; at < to; at++)
			{
				var token = tokens[at];

				if (token.Kind == SqlTokens.Kind.Punctuation)
				{
					if (token.Text == "(")
						depth++;
					else if (token.Text == ")")
						depth--;
					else if (token.Text == "," && depth == 0)
					{
						parts.Add([]);
						continue;
					}
				}

				parts[^1].Add(token);
			}

			return parts;
		}
	}
}
