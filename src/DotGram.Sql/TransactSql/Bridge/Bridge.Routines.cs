using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.TransactSql;

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
				Query.FromExecute         => throw BridgeGap.Open(old, "inventory Query.FromExecute (outside an INSERT)"),
				_ when Owned(Owns, old) => throw Unconverted(old),
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
		/// <summary>EXECUTE, on the common tree, from what its rules read: it stands where the rule says it was written.</summary>
		internal static Statement Execute(Statement.Execute read, SqlSpan at)
		{
			return Carry(read, at, static old => Execute(old));
		}

		/// <summary>ENABLE and DISABLE TRIGGER, on the common tree.</summary>
		internal static Statement Switch(bool enable, SqlList<string>? names, string on)
		{
			var read = new Statement.TriggerSwitch(enable, names, on);

			return Carry(read, static old => Switch(old, old.Enable, old.Triggers, old.On));
		}

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

			return new Ast.Statement.ExecuteModule(module, Each(old.Arguments, one => ExecuteArgument(old, one)))
			{
				Keyword      = keyword,
				ReturnStatus = old.Into is null ? null : new Ast.Identifier(old.Into),
				Number       = old.Number is null ? null : new Ast.LiteralValue.Numeric(old.Number, Ast.NumericLiteralKind.DecimalInteger),
				At           = old.At is null ? null : Values.Part(old, old.At),
				Options      = options,
			};
		}

		/// <summary>
		/// A procedure, a function, a trigger or a view, on the common tree: what its rules read, and the word it opened
		/// with; it stands where the statement was written, as the text it was read from says.
		/// </summary>
		internal static Statement Routine(Statement body, string verb, SqlSpan at)
		{
			var read = Syntax.Verbed(body, verb);

			read.Locate(at.At, at.Length);

			return Hold(new Read<Statement>(NameOf(read), read), () => read switch
			{
				Statement.CreateProcedure procedure => Procedure(procedure),
				Statement.CreateFunction function   => Function(function),
				Statement.CreateTrigger trigger     => Trigger(trigger),
				Statement.ViewDefinition view       => Tables.View(view),
				_                                   => throw new ArgumentException($"A routine's rule read {NameOf(read)}.", nameof(body)),
			});
		}

		/// <summary>
		/// <c>CREATE [OR ALTER] | ALTER PROC[EDURE] p[;n] @a … WITH … FOR REPLICATION AS …</c>. PROC against PROCEDURE is read
		/// from the text.
		/// </summary>
		static Ast.Statement Procedure(Statement.CreateProcedure old)
		{
			var definition = new Ast.RoutineDefinition(Ast.RoutineKind.Procedure, Values.Name(old, old.Name), Parameters(old, old.Parameters), null, [],
				old.External is { } external ? new Ast.RoutineBody.ExternalName(Values.Name(old, external)) : new Ast.RoutineBody.Statements(Each(old.Body, StatementOf)))
			{
				Options        = RoutineOptions(old, old.Options),
				ForReplication = old.ForReplication,
				Number         = old.Number is null ? null : new Ast.LiteralValue.Numeric(old.Number, Ast.NumericLiteralKind.DecimalInteger),
				Proc           = Recover("RoutineDefinition.Proc", Spelled(old), static text => Words(text) is [.., "PROC"] or ["CREATE" or "ALTER", "PROC", ..] or ["CREATE", "OR", "ALTER", "PROC", ..], false),
				Unbracketed    = old.Parameters.Length > 0 && Recover("RoutineDefinition.Unbracketed", Spelled(old), static text => !Bracketed(text), false),
			};

			return new Ast.Statement.CreateRoutine { Definition = definition, Verb = Verb(old, old.Verb) };
		}

		/// <summary>
		/// <c>CREATE FUNCTION f (…) RETURNS …</c>: a type and a body or an external name; TABLE and RETURN a query; TABLE and
		/// its columns, ORDER and an external name; or a variable and its table, and a body.
		/// </summary>
		static Ast.Statement Function(Statement.CreateFunction old)
		{
			Ast.ReturnsDefinition returns;
			Ast.RoutineBody body;

			if (old.Returns is not { } type)
				throw BridgeGap.Open(old, "inventory Statement.CreateFunction (no RETURNS)");

			if (old.Variable is { } variable)
			{
				returns = new Ast.ReturnsDefinition(null, null) { Variable = new Ast.Identifier(variable), Elements = Elements(old.Columns ?? []) };
				body    = new Ast.RoutineBody.Statements(Each(old.Body, StatementOf));
			}
			else if (type.Equals("TABLE", StringComparison.OrdinalIgnoreCase) && old.Columns is { } columns)
			{
				returns = new Ast.ReturnsDefinition(null, null) { TableKeyword = true, Elements = Elements(columns) };
				body    = old.External is { } external ? new Ast.RoutineBody.ExternalName(Values.Name(old, external)) : throw BridgeGap.Open(old, "inventory Statement.CreateFunction (a CLR table without EXTERNAL NAME)");
			}
			else if (type.Equals("TABLE", StringComparison.OrdinalIgnoreCase))
			{
				returns = new Ast.ReturnsDefinition(null, null) { TableKeyword = true };
				body    = old.Body is [Statement.Select select] && StatementOf(select) is Ast.Statement.Select query
					? new Ast.RoutineBody.Inline(query)
					: throw BridgeGap.Open(old, "inventory Statement.CreateFunction (an inline body that is not one query)");
			}
			else
			{
				returns = new Ast.ReturnsDefinition(Printed(old, type, "Statement.CreateFunction.Returns"), null);
				body    = old.External is { } external ? new Ast.RoutineBody.ExternalName(Values.Name(old, external)) : new Ast.RoutineBody.Statements(Each(old.Body, StatementOf));
			}

			if (old.Parameters.Length > 0 && Spelled(old) is { } spelled && !Bracketed(spelled))
				throw BridgeGap.Open(old, "inventory Statement.CreateFunction (parameters without brackets, which RoutineDefinition.Unbracketed is refused on)");

			var definition = new Ast.RoutineDefinition(Ast.RoutineKind.Function, Values.Name(old, old.Name), Parameters(old, old.Parameters), returns, [], body)
			{
				AsOmitted = Recover("RoutineDefinition.AsOmitted", Spelled(old), text => !AsBefore(old, text, body), false),
				Options = RoutineOptions(old, old.Options),
				Order   = Each(old.Order ?? [], one => one is Clause.SortSpecification sort ? Queries.Sort(sort) : throw BridgeGap.Open(old, $"inventory {NameOf(one)} (in ORDER)")),
			};

			return new Ast.Statement.CreateRoutine { Definition = definition, Verb = Verb(old, old.Verb) };
		}

		/// <summary>
		/// Whether a routine's parameters are written in brackets: the token after its name, and after <c>;n</c> where it has
		/// one, opens them.
		/// </summary>
		static bool Bracketed(string text)
		{
			var tokens = SqlTokens.Of(text);
			var at     = tokens.FindIndex(static one => one.Kind == SqlTokens.Kind.Word && one.Text.ToUpperInvariant() is "PROC" or "PROCEDURE" or "FUNCTION") + 1;

			// The name: parts between dots, a part left out between two of them, or before the first.
			if (at < tokens.Count && tokens[at].Text != ".")
				at++;

			while (at < tokens.Count && tokens[at].Text == ".")
			{
				at++;

				if (at < tokens.Count && tokens[at].Text != ".")
					at++;
			}

			if (at < tokens.Count && tokens[at].Text == ";")
				at += 2;

			return at < tokens.Count && tokens[at].Text == "(";
		}

		/// <summary>
		/// Whether AS stands before a function's body: the last word before its statements, before the RETURN of an inline
		/// query, or before EXTERNAL NAME.
		/// </summary>
		static bool AsBefore(Statement.CreateFunction old, string text, Ast.RoutineBody body)
		{
			if (body is Ast.RoutineBody.ExternalName)
			{
				var all = Words(text);

				for (var at = all.Count - 2; at > 0; at--)
				{
					if (all[at] == "EXTERNAL" && all[at + 1] == "NAME")
						return all[at - 1] == "AS";
				}

				return true;
			}

			// An inline query is built by its rule, unlocated: its RETURN is the text's first, which no query and no option holds.
			if (body is Ast.RoutineBody.Inline)
			{
				var all = Words(text);
				var at  = all.IndexOf("RETURN");

				return at <= 0 || all[at - 1] == "AS";
			}

			if (old.Body is not [var first, ..] || !first.Span.Known || !old.Span.Known)
				return true;

			return Words(text[..(first.Span.At - old.Span.At)]) is not [.., var last] || last == "AS";
		}

		/// <summary>
		/// <c>CREATE TRIGGER t ON table | DATABASE | ALL SERVER WITH … FOR | AFTER | INSTEAD OF events … AS …</c>: a DML
		/// trigger's statements, or a DDL or logon trigger's event names, on whichever target was written: TriggerEvents
		/// reads either list on either, and the engine answers the wrong kind only after reading it (Msg 1098). A list of
		/// both, which the grammar refuses, is a shape no member holds.
		/// </summary>
		static Ast.Statement Trigger(Statement.CreateTrigger old)
		{
			Ast.TriggerScope? scope = old.On switch
			{
				"DATABASE"   => Ast.TriggerScope.Database,
				"ALL SERVER" => Ast.TriggerScope.AllServer,
				_            => null,
			};

			var statements = old.Events.All(static one => one is "INSERT" or "UPDATE" or "DELETE");

			if (!statements && old.Events.Any(static one => one is "INSERT" or "UPDATE" or "DELETE"))
				throw BridgeGap.Missing(old, "Statement.CreateTrigger (statement kinds and event names in one list)");

			var (time, @for) = string.Join(' ', Words(old.When)) switch
			{
				"AFTER"      => (Ast.TriggerTime.After, false),
				"FOR"        => (Ast.TriggerTime.After, true),
				"INSTEAD OF" => (Ast.TriggerTime.InsteadOf, false),
				_            => throw BridgeGap.Open(old, $"inventory Statement.CreateTrigger (`{old.When}`)"),
			};

			return new Ast.Statement.CreateTrigger
			{
				Name              = Values.Name(old, old.Name),
				Time              = time,
				Event             = null,
				Table             = scope is null ? Values.Name(old, old.On) : null,
				Action            = null,
				Verb              = Verb(old, old.Verb),
				Scope             = scope,
				Events            = statements ? Each(old.Events, static one => new Ast.TriggerEvent(one switch
				{
					"INSERT" => Ast.TriggerEventKind.Insert,
					"UPDATE" => Ast.TriggerEventKind.Update,
					_        => Ast.TriggerEventKind.Delete,
				}, [])) : [],
				EventNames        = statements ? [] : Each(old.Events, one => Values.Part(old, one)),
				For               = @for,
				Options           = RoutineOptions(old, old.Options),
				Append            = old.Append,
				NotForReplication = old.NotForReplication,
				Body              = old.External is { } external ? new Ast.RoutineBody.ExternalName(Values.Name(old, external)) : new Ast.RoutineBody.Statements(Each(old.Body, StatementOf)),
			};
		}

		/// <summary><c>ENABLE | DISABLE TRIGGER names | ALL ON object | DATABASE | ALL SERVER</c>.</summary>
		static Ast.Statement Switch(object old, bool enable, SqlList<string>? names, string on)
		{
			var triggers = Each(names ?? [], one => Values.Name(old, one));
			var all      = names is null;

			Ast.TriggerScope? scope = on switch
			{
				"DATABASE"   => Ast.TriggerScope.Database,
				"ALL SERVER" => Ast.TriggerScope.AllServer,
				_            => null,
			};

			Ast.QualifiedName? target = scope is null ? Values.Name(old, on) : null;

			return enable
				? new Ast.Statement.EnableTrigger(triggers) { All = all, On = target, Scope = scope }
				: new Ast.Statement.DisableTrigger(triggers) { All = all, On = target, Scope = scope };
		}

		// ── The module-like objects, which the old tree keeps as a name and the text after it ──────────────

		/// <summary>
		/// <c>CREATE ASSEMBLY a [AUTHORIZATION o] FROM v, … [WITH PERMISSION_SET = …]</c>, or ALTER ASSEMBLY's parts in their
		/// order: FROM, WITH, DROP FILE, ADD FILE.
		/// </summary>
		internal static Ast.Statement Assembly(Defining old)
		{
			var tail   = old.Tail ?? throw BridgeGap.Open(old, "inventory Statement.AssemblyDefinition (nothing after the name)");
			var tokens = Located(tail);
			var name   = Values.Part(old, old.Name);

			if (old.Verb == "CREATE")
			{
				var from  = Index(tokens, "FROM");
				var with  = Index(tokens, "WITH");
				var owner = Words(tail) is ["AUTHORIZATION", _, ..] ? Values.Part(old, tokens[1].Token.Text) : null;

				if (from < 0)
					throw BridgeGap.Open(old, $"inventory Statement.AssemblyDefinition ({tail})");

				var values = Slice(tail, tokens, from + 1, with < 0 ? tokens.Count : with);

				return Checked(old, new Ast.Statement.CreateAssembly(name, [.. Listed(values).Select(one => ValueOf(old, one))])
				{
					Authorization = owner,
					PermissionSet = with < 0 ? null : Permission(old, tokens[^1].Token.Text),
				});
			}

			// ALTER's own FROM stands first; the FROM of ADD FILE FROM is ADD's.
			var parts = new[] { Words(tail) is ["FROM", ..] ? 0 : -1, Index(tokens, "WITH"), Index(tokens, "DROP"), Index(tokens, "ADD") };
			var made  = new Ast.Statement.AlterAssembly(name);

			string Part(int which)
			{
				var end = parts.Skip(which + 1).Where(static one => one >= 0).DefaultIfEmpty(tokens.Count).First();

				return Slice(tail, tokens, parts[which] + 1, end);
			}

			if (parts[0] >= 0)
				made = made with { From = ValueOf(old, Part(0)) };

			if (parts[1] >= 0)
			{
				var options = (old.Options ?? []).Select(static one => one is Clause.Option { Name: var named, Value: var value } ? (named, value) : ("", null)).ToArray();

				if (!options.Select(static one => one.Item1).SequenceEqual(new[] { "PERMISSION_SET", "VISIBILITY", "UNCHECKED DATA" }.Where(word => options.Any(one => one.Item1 == word))))
					throw BridgeGap.Lost(old, "Statement.AssemblyDefinition (its options in another order)");

				foreach (var (named, value) in options)
					made = named switch
					{
						"PERMISSION_SET" when value is Expression.ColumnReference(var word) => made with { PermissionSet = Permission(old, word) },
						"VISIBILITY" when value is Expression.ColumnReference(var word)     => made with { Visibility = word == "ON" },
						"UNCHECKED DATA"                                                    => made with { UncheckedData = true },
						_ => throw BridgeGap.Open(old, $"inventory Statement.AssemblyDefinition (the option {named})"),
					};
			}

			if (parts[2] >= 0)
			{
				var files = Part(2);

				made = Words(files) switch
				{
					["FILE", "ALL"] => made with { DropAllFiles = true },
					["FILE", ..]    => made with { DropFiles = [.. Listed(Slice(files, Located(files), 1, Located(files).Count)).Select(one => ValueOf(old, one))] },
					_               => throw BridgeGap.Open(old, $"inventory Statement.AssemblyDefinition (DROP {files})"),
				};
			}

			if (parts[3] >= 0)
			{
				var files = Part(3);
				var read  = Located(files);

				if (Words(files) is not ["FILE", "FROM", ..])
					throw BridgeGap.Open(old, $"inventory Statement.AssemblyDefinition (ADD {files})");

				made = made with
				{
					AddFiles = [.. Listed(Slice(files, read, 2, read.Count)).Select(one =>
					{
						var each = Located(one);
						var @as  = Index(each, "AS");

						return @as < 0
							? new Ast.AssemblyFile(ValueOf(old, one))
							: new Ast.AssemblyFile(ValueOf(old, Slice(one, each, 0, @as))) { Name = Values.String(Slice(one, each, @as + 1, each.Count)) };
					})],
				};
			}

			return Checked(old, made);
		}

		static Ast.AssemblyPermissionSet Permission(ISqlSpan old, string word)
		{
			return word.ToUpperInvariant() switch
			{
				"SAFE"            => Ast.AssemblyPermissionSet.Safe,
				"EXTERNAL_ACCESS" => Ast.AssemblyPermissionSet.ExternalAccess,
				"UNSAFE"          => Ast.AssemblyPermissionSet.Unsafe,
				_                 => throw BridgeGap.Open(old, $"inventory Statement.AssemblyDefinition (PERMISSION_SET = {word})"),
			};
		}

		/// <summary><c>CREATE RULE r AS condition</c>, the condition read again as a WHERE.</summary>
		internal static Ast.Statement Rule(Defining old)
		{
			var tail = old.Tail ?? "";

			return Words(tail) is ["AS", ..]
				? Checked(old, new Ast.Statement.CreateRule(Values.Name(old, old.Name), ConditionOf(old, Slice(tail, Located(tail), 1, Located(tail).Count))))
				: throw BridgeGap.Open(old, $"inventory Statement.RuleDefinition ({tail})");
		}

		/// <summary><c>CREATE DEFAULT d AS value</c>, the value read again as a select list's.</summary>
		internal static Ast.Statement Default(Defining old)
		{
			var tail = old.Tail ?? "";

			return Words(tail) is ["AS", ..]
				? Checked(old, new Ast.Statement.CreateDefault(Values.Name(old, old.Name), ValueOf(old, Slice(tail, Located(tail), 1, Located(tail).Count))))
				: throw BridgeGap.Open(old, $"inventory Statement.DefaultDefinition ({tail})");
		}

		/// <summary><c>CREATE AGGREGATE a (@p [AS] type [NULL | NOT NULL], …) RETURNS type EXTERNAL NAME asm[.class]</c>.</summary>
		internal static Ast.Statement Aggregate(Defining old)
		{
			var tail    = old.Tail ?? "";
			var tokens  = Located(tail);
			var close   = tokens.Count > 0 && tokens[0].Token.Text == "(" ? Closing(tokens, 0) : -1;
			var returns = Index(tokens, "RETURNS");
			var name    = Index(tokens, "EXTERNAL");

			if (close < 0 || returns != close + 1 || name < 0 || tokens.Count <= name + 2 || !tokens[name + 1].Token.Text.Equals("NAME", StringComparison.OrdinalIgnoreCase))
				throw BridgeGap.Open(old, $"inventory Statement.AggregateDefinition ({tail})");

			var parameters = Listed(Slice(tail, tokens, 1, close)).Select(one =>
			{
				var each  = Located(one);
				var words = Words(one);
				var @as   = words is [_, "AS", ..];
				var end   = words is [.., "NOT", "NULL"] ? each.Count - 2 : words is [.., "NULL"] ? each.Count - 1 : each.Count;
				var type  = Slice(one, each, @as ? 2 : 1, end);

				return new Ast.ParameterDefinition(null, new Ast.Identifier(each[0].Token.Text), Printed(old, type, "Statement.AggregateDefinition.Tail"), false, null,
					Nullable: end == each.Count ? null : end == each.Count - 1)
				{
					AsKeyword = @as,
				};
			}).ToSqlList();

			return Checked(old, new Ast.Statement.CreateAggregate(
				Values.Name(old, old.Name),
				parameters,
				Printed(old, Slice(tail, tokens, returns + 1, name), "Statement.AggregateDefinition.Tail"),
				Values.Name(old, Slice(tail, tokens, name + 2, tokens.Count))));
		}

		/// <summary><c>CREATE XML SCHEMA COLLECTION c AS value</c> and <c>ALTER … ADD value</c>.</summary>
		internal static Ast.Statement SchemaCollection(Defining old, bool alter)
		{
			var tail   = old.Tail ?? "";
			var tokens = Located(tail);

			if (Words(tail) is not [var word, ..] || word != (alter ? "ADD" : "AS"))
				throw BridgeGap.Open(old, $"inventory {NameOf(old)} ({tail})");

			var schema = ValueOf(old, Slice(tail, tokens, 1, tokens.Count));

			return Checked(old, alter
				? new Ast.Statement.AlterXmlSchemaCollection(Values.Name(old, old.Name), schema)
				: new Ast.Statement.CreateXmlSchemaCollection(Values.Name(old, old.Name), schema));
		}

		/// <summary><c>CREATE TYPE t EXTERNAL NAME asm[.class]</c>, which the Tables family hands on.</summary>
		internal static Ast.Statement ClrType(Defining old)
		{
			var tail   = old.Tail ?? "";
			var tokens = Located(tail);

			return Checked(old, new Ast.Statement.CreateClrType(Values.Name(old, old.Name), Values.Name(old, Slice(tail, tokens, 2, tokens.Count))));
		}

		/// <summary>
		/// A statement made of a tail the old tree keeps as written and compares as the oracle does: only one whose text,
		/// written and read again, keeps the same tail carries over.
		/// </summary>
		static Ast.Statement Checked(Defining old, Ast.Statement made)
		{
			string text;

			try
			{
				text = Ast.TransactSqlWriter.Write(made);
			}
			catch (Exception refused) when (refused is ArgumentException or NotSupportedException)
			{
				return made;
			}

			return Skim(text) is [Statement.Built { Given: Defining { Tail: var tail } }]
				&& (tail == old.Tail || tail is not null && old.Tail is not null && Same(old, NameOf(old) + ".Tail", tail, old.Tail))
				? made
				: throw BridgeGap.Open(old, $"Trees.FreeText ({NameOf(old)}.Tail, written otherwise than the writer prints it)");
		}

		/// <summary>A value kept as text, read again as a select list's one value.</summary>
		static Ast.Expression ValueOf(object owner, string text)
		{
			return TransactSql.TransactSqlParser.TryParseSyntaxSql("SELECT " + text.Trim()) is { IsSuccess: true, Value: [Statement.Select { Of: Query.Specification { Columns: [Clause.DerivedColumn { Value: var value, Name: null }] } }] }
				? Values.Of(value)
				: throw BridgeGap.Open(owner, $"inventory {NameOf(owner)} (a value `{text.Trim()}`)");
		}

		/// <summary>A search condition kept as text, read again as a query's WHERE.</summary>
		static Ast.Expression ConditionOf(object owner, string text)
		{
			return TransactSql.TransactSqlParser.TryParseSyntaxSql("SELECT 1 WHERE " + text.Trim()) is { IsSuccess: true, Value: [Statement.Select { Of: Query.Specification { Where: { } where } }] }
				? Values.Of(where)
				: throw BridgeGap.Open(owner, $"inventory {NameOf(owner)} (a condition `{text.Trim()}`)");
		}

		/// <summary>The tokens of a text and where each begins.</summary>
		static List<(SqlTokens.Token Token, int At)> Located(string text)
		{
			var located = new List<(SqlTokens.Token, int)>();
			var at      = 0;

			foreach (var token in SqlTokens.Of(text))
			{
				var found = text.IndexOf(token.Text, at, StringComparison.Ordinal);

				located.Add((token, found));
				at = found + token.Text.Length;
			}

			return located;
		}

		/// <summary>The first word outside brackets that is <paramref name="word"/>, or -1.</summary>
		static int Index(List<(SqlTokens.Token Token, int At)> tokens, string word)
		{
			var depth = 0;

			for (var at = 0; at < tokens.Count; at++)
			{
				var token = tokens[at].Token;

				if (token is { Kind: SqlTokens.Kind.Punctuation, Text: "(" })
					depth++;
				else if (token is { Kind: SqlTokens.Kind.Punctuation, Text: ")" })
					depth--;
				else if (depth == 0 && token.Kind == SqlTokens.Kind.Word && token.Text.Equals(word, StringComparison.OrdinalIgnoreCase))
					return at;
			}

			return -1;
		}

		static int Closing(List<(SqlTokens.Token Token, int At)> tokens, int open)
		{
			var depth = 0;

			for (var at = open; at < tokens.Count; at++)
			{
				if (tokens[at].Token is { Kind: SqlTokens.Kind.Punctuation, Text: "(" })
					depth++;
				else if (tokens[at].Token is { Kind: SqlTokens.Kind.Punctuation, Text: ")" } && --depth == 0)
					return at;
			}

			return -1;
		}

		/// <summary>The text from the token <paramref name="from"/> up to the token <paramref name="to"/>, trimmed.</summary>
		static string Slice(string text, List<(SqlTokens.Token Token, int At)> tokens, int from, int to)
		{
			if (from >= tokens.Count || from >= to)
				return "";

			var end = to < tokens.Count ? tokens[to].At : text.Length;

			return text[tokens[from].At..end].Trim();
		}

		/// <summary>A text cut at its commas outside brackets.</summary>
		static List<string> Listed(string text)
		{
			var pieces = new List<string>();
			var depth  = 0;
			var from   = 0;

			foreach (var (token, at) in Located(text))
			{
				if (token is { Kind: SqlTokens.Kind.Punctuation, Text: "(" })
					depth++;
				else if (token is { Kind: SqlTokens.Kind.Punctuation, Text: ")" })
					depth--;
				else if (depth == 0 && token is { Kind: SqlTokens.Kind.Punctuation, Text: "," })
				{
					pieces.Add(text[from..at].Trim());
					from = at + 1;
				}
			}

			pieces.Add(text[from..].Trim());

			return pieces;
		}

		/// <summary>A routine's verb: null for the plain CREATE, the standard's.</summary>
		static Ast.DefinitionVerb? Verb(ISqlSpan old, string verb)
		{
			return string.Join(' ', Words(verb)) switch
			{
				"CREATE"          => null,
				"ALTER"           => Ast.DefinitionVerb.Alter,
				"CREATE OR ALTER" => Ast.DefinitionVerb.CreateOrAlter,
				_                 => throw BridgeGap.Open(old, $"inventory {NameOf(old)} (the verb {verb})"),
			};
		}

		/// <summary>
		/// A type the old tree keeps as written and compares as the oracle does: only one written as the writer prints it,
		/// token by token where the member is free text, carries over.
		/// </summary>
		static Ast.DataType Printed(ISqlSpan old, string text, string member)
		{
			var type = Values.Type(old, text);

			return Same(old, member, Ast.TransactSqlWriter.Write(type), text) ? type : throw BridgeGap.Open(old, $"Trees.FreeText ({member}, a type written otherwise than the writer prints it)");
		}

		/// <summary>A returned table's columns, read as CREATE TABLE's body is.</summary>
		static SqlList<Ast.TableElement> Elements(SqlList<Clause> columns)
		{
			return Tables.Elements(columns);
		}

		/// <summary>
		/// The parameters: <c>@a [AS] type [NULL] [= v] [OUTPUT | OUT] [READONLY]</c>, or a cursor's <c>CURSOR VARYING
		/// OUTPUT</c>. AS is read from the text; the words after the value carry over only in the order the writer prints.
		/// </summary>
		static SqlList<Ast.ParameterDefinition> Parameters(ISqlSpan old, SqlList<Clause> parameters)
		{
			return Each(parameters, one =>
			{
				if (one is not Clause.ParameterDeclaration(var name, { } type, var value, var nullability, var varying, var ways))
					throw BridgeGap.Open(one, "inventory Clause.ParameterDeclaration");

				var words = ways ?? [];
				var back  = words.Where(static word => word is "OUTPUT" or "OUT").ToArray();
				var only  = words.Count(static word => word == "READONLY");

				if (back.Length > 1 || only > 1 || words.Length > 0 && !words.SequenceEqual([.. back, .. words.Where(static word => word == "READONLY")]))
					throw BridgeGap.Lost(one, "Clause.ParameterDeclaration (OUTPUT and READONLY in another order, or twice)");

				var cursor = varying && type == "CURSOR";

				return new Ast.ParameterDefinition(
					back.Length > 0 ? Ast.ParameterMode.Out : null,
					new Ast.Identifier(name),
					cursor ? new Ast.DataType.Named(new Ast.QualifiedName([new Ast.Identifier("CURSOR")]), []) : Printed(old, type, "Clause.ParameterDeclaration.Type"),
					false,
					value is null ? null : Values.Of(value),
					ReadOnly: only > 0,
					Varying: varying,
					Nullable: nullability is null ? null : Words(nullability) switch
					{
						["NULL"]        => true,
						["NOT", "NULL"] => false,
						_               => throw BridgeGap.Open(one, $"inventory Clause.ParameterDeclaration (`{nullability}`)"),
					})
				{
					ShortOutput = back is ["OUT"],
					AsKeyword   = Recover("ParameterDefinition.AsKeyword", Spelled(one), static text => Words(text) is [_, "AS", ..], false),
				};
			});
		}

		/// <summary>
		/// A module's WITH: EXECUTE AS, the two ON NULL INPUT forms, and every other a setting. The old tree keeps EXECUTE
		/// AS's word as written; CALLER, SELF and OWNER are key words at that place, whatever their case.
		/// </summary>
		static SqlList<Ast.RoutineOption> RoutineOptions(ISqlSpan old, SqlList<Clause>? options)
		{
			return Each<Clause, Ast.RoutineOption>(options ?? [], one => one switch
			{
				Clause.Option { Name: var name, Value: Expression.ColumnReference(var who) } when Words(name) is ["EXECUTE", "AS"] => new Ast.RoutineOption.ExecuteAs(who.ToUpperInvariant() switch
				{
					"CALLER" => new Ast.ExecutionContext(Ast.ExecutionContextKind.Caller, null),
					"SELF"   => new Ast.ExecutionContext(Ast.ExecutionContextKind.Self, null),
					"OWNER"  => new Ast.ExecutionContext(Ast.ExecutionContextKind.Owner, null),
					_ when who.StartsWith('\'') || who.StartsWith("N'", StringComparison.OrdinalIgnoreCase)
						=> new Ast.ExecutionContext(Ast.ExecutionContextKind.User, Values.String(who)),
					_ => throw BridgeGap.Open(one, $"Trees.FreeText (EXECUTE AS `{who}`, a word not in capitals)"),
				}),
				Clause.Option { Name: var name, Value: null } when Words(name) is ["RETURNS", "NULL", "ON", "NULL", "INPUT"] => new Ast.RoutineOption.NullCall(Ast.NullCallMode.ReturnsNullOnNullInput),
				Clause.Option { Name: var name, Value: null } when Words(name) is ["CALLED", "ON", "NULL", "INPUT"]          => new Ast.RoutineOption.NullCall(Ast.NullCallMode.CalledOnNullInput),
				// INLINE ON: a value after the name with no =, which the catalogue's reading leaves to the place that has it.
				Clause.Option { Bare: true, Value: not null } option => new Ast.RoutineOption.Setting(Tables.Option(option with { Bare = false }) with { EqualsOmitted = true }),
				Clause.Option option => new Ast.RoutineOption.Setting(Tables.GenericOption(option)),
				_ => throw BridgeGap.Open(one, $"inventory {NameOf(one)} (a module's option)"),
			});
		}

		/// <summary>A string's context, <c>AS LOGIN = 'x'</c> or <c>AS USER = N'u'</c>.</summary>
		static Ast.ExecutionContext Context(ISqlSpan old, Clause context)
		{
			return context switch
			{
				Clause.ExecutionContext("LOGIN", var name) => new Ast.ExecutionContext(Ast.ExecutionContextKind.Login, Values.Of(name)),
				Clause.ExecutionContext("USER", var name)  => new Ast.ExecutionContext(Ast.ExecutionContextKind.User, Values.Of(name)),
				_ => throw BridgeGap.Open(old, $"inventory {NameOf(context)}"),
			};
		}

		/// <summary>One value passed: <c>@p = v</c>, and OUTPUT or OUT after it, which the old tree keeps as a hint.</summary>
		static Ast.ExecuteArgument ExecuteArgument(ISqlSpan old, Expression value)
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
			var printed = Ast.TransactSqlWriter.Write(new Ast.Statement.ExecuteModule(new Ast.ModuleReference.Named(new Ast.QualifiedName([new Ast.Identifier("p")])), []) { Keyword = Ast.ExecuteKeyword.Exec, Options = made });

			if (!printed.StartsWith("EXEC p ", StringComparison.Ordinal) || !Same(old, "Statement.Execute.Tail", printed["EXEC p ".Length..], tail))
				throw BridgeGap.Open(old, "Trees.FreeText (Statement.Execute.Tail, written otherwise than the writer prints it)");

			return made;
		}

		/// <summary>One result set: its columns, read again as a table's, or AS OBJECT, AS TYPE, AS FOR XML.</summary>
		static Ast.ResultSet ResultSet(ISqlSpan old, List<SqlTokens.Token> set)
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

					return TransactSql.TransactSqlParser.TryParseSyntaxSql("CREATE TABLE t (" + body + ")") is { IsSuccess: true, Value: [Statement.Built table] }
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
