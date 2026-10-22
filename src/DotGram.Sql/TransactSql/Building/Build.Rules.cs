using System;
using System.Collections.Generic;
using System.Linq;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.TransactSql;

static partial class Build
{
	/// <summary>
	/// What the rules ask of what they read, in their conditions, and the shapes some of them gather it into before a
	/// builder takes it: a source and the pivots and joins after it, a column list, an option's words.
	/// </summary>
	internal static class Rules
	{
		const string ColumnSet = "COLUMN_SET FOR ALL_SPARSE_COLUMNS";

		/// <summary>How a type was written, which is what the front of a column's tail asks.</summary>
		enum TypeWritten
		{
			/// <summary>With no brackets: <c>INT</c>, <c>XML</c>, <c>dbo.t</c>.</summary>
			Bare,

			/// <summary>With numbers in brackets: <c>VARCHAR (10)</c>, <c>DECIMAL (10, 2)</c>.</summary>
			Sized,

			/// <summary>With a word in brackets: <c>VARCHAR (MAX)</c>, <c>XML (CONTENT s)</c>.</summary>
			Worded,
		}

		/// <summary>A session's events and the targets written after them, as its one list of pieces.</summary>
		public static Clause[] Pieces(Clause[]? events, Clause[]? targets)
		{
			return [.. events ?? [], .. targets ?? []];
		}

		/// <summary>Names as sort specifications with no direction, which is how a column list stands.</summary>
		public static Clause[] Columns(string[]? names)
		{
			return names is null ? [] : [.. names.Select(static one => new Clause.SortSpecification(Values.Reference(one), SqlOrder.Unspecified))];
		}

		/// <summary>One word as an option with nothing set, or nothing where none was written.</summary>
		public static Clause[] Unit(string? word)
		{
			return word is null ? [] : [new Clause.Option(Syntax.Squared(word), null, Clause.None)];
		}

		/// <summary>A grouping column with what was written after it: a collation, a hint.</summary>
		public static Ast.Expression Grouped(Ast.Expression column, string? collation, string? hint)
		{
			var made = collation is null ? column : Values.Collated(column, collation);

			return hint is null ? made : new Values.Hinted(made, Syntax.Spaced(hint));
		}

		/// <summary>A table source rooted at a variable, once the variable's name is known.</summary>
		/// <remarks>
		/// The two shapes share their first token and so are read by one rule, which cannot know
		/// the name until the caller hands it over: a table variable is a name, and a
		/// table-valued method on a variable of a user-defined type is a call whose name begins
		/// with one.
		/// </remarks>
		public static TableReference OfVariable(string name, TableReference source)
		{
			return source switch
			{
				TableReference.Named one => one with { Table = name },
				TableReference.FunctionCall two => two with
				{
					Function = two.Function with { Name = name + two.Function.Name },
				},
				_ => source,
			};
		}

		/// <summary>A value with words after it, or the value alone where there were none.</summary>
		public static Ast.Expression Hinted(Ast.Expression value, string? words)
		{
			return words is null || words.Length == 0 ? value : new Values.Hinted(value, Syntax.Spaced(words));
		}

		/// <summary>Whether a value is a column and nothing more, brackets aside.</summary>
		public static bool IsColumn(Ast.Expression? value)
		{
			// A loop down the brackets rather than a call a bracket: the parser reads any number of them.
			while (Values.Bracketed(value) is { } inner)
				value = inner;

			return value is Ast.Expression.Reference or Values.Word;
		}

		/// <summary>
		/// Whether a select list holds T-SQL's <c>IDENTITY(…)</c>, which only a <c>SELECT … INTO</c>
		/// may: the engine refuses one without the <c>INTO</c> when it compiles the statement,
		/// <c>Msg 177</c>.
		/// </summary>
		public static bool HasIdentity(Ast.SelectColumn[] columns)
		{
			foreach (var one in columns)
			{
				// An item with no place on the common tree is asked of the value it was read over, as any other.
				var value = one switch
				{
					Ast.SelectColumn.ExpressionItem item         => item.Expression,
					Queries.Unplaced { Assigns: false } unplaced => unplaced.Value,
					_                                            => null,
				};

				if (value is Values.Call { Name: var name, Word: not null } && string.Equals(name, "IDENTITY", StringComparison.OrdinalIgnoreCase))
					return true;
			}

			return false;
		}

		/// <summary>The arguments that were written, in order, leaving out the optional ones that were not.</summary>
		public static Ast.Expression[] Written(params Ast.Expression?[] arguments)
		{
			return arguments.Where(static one => one is not null).Select(static one => one!).ToArray();
		}

		/// <summary>
		/// Arguments with the clauses written after the last of them — a JSON constructor's
		/// order, null treatment and return type — hung on that last argument, since they are
		/// written where it ends.
		/// </summary>
		public static Ast.Expression[] Tailed(Ast.Expression[] items, params string?[] clauses)
		{
			var words = string.Join(" ", clauses.Where(static one => one is not null).Select(static one => Syntax.Spaced(one!)));

			if (words.Length == 0)
				return items;

			if (items.Length == 0)
				return [Values.Reference(words)];

			var made = (Ast.Expression[])items.Clone();

			made[^1] = new Values.Hinted(made[^1], words);

			return made;
		}

		/// <summary>A value with the unit after it, or the value alone where none was written.</summary>
		public static Ast.Expression Measured(Ast.Expression value, string? unit)
		{
			return unit is null ? value : new Values.Measured(value, Syntax.Squared(unit));
		}

		/// <summary>A <c>REFERENCES</c> from its parts, the actions read back from the words.</summary>
		public static Clause.References Referenced(string table, SqlList<string>? columns, string[]? actions, string? replication = null)
		{
			string? onDelete = null, onUpdate = null;
			var replicated = false;

			foreach (var action in actions ?? [])
			{
				var squared = Syntax.Squared(action);

				if (squared.StartsWith("ON DELETE ", StringComparison.Ordinal))
					onDelete = squared[10..];
				else if (squared.StartsWith("ON UPDATE ", StringComparison.Ordinal))
					onUpdate = squared[10..];
				else
					replicated = true;
			}

			return new Clause.References(table, columns, onDelete, onUpdate, replicated || replication is not null);
		}

		/// <summary>A <c>CONNECTION</c>'s pairs and the actions after them, as one list.</summary>
		public static Clause[] Connected(Clause first, Clause[]? rest, string[]? actions)
		{
			return [.. Syntax.Listed(first, rest), .. (actions ?? []).Select(static one => (Clause)new Clause.Option(Syntax.Squared(one), null, Clause.None))];
		}

		/// <summary>
		/// A query with the order and the shape written inside its brackets, where either was;
		/// the query alone where neither was.
		/// </summary>
		public static Query Ordered(Query query, Clause.OrderBy? by, Clause? shape)
		{
			return by is null && shape is null ? query : new Query.Ordered(query, by, shape);
		}

		/// <summary>Which way a sort specification asked for its rows (§10.10).</summary>
		public static SqlOrder Ordered(string? order)
		{
			return order is null ? SqlOrder.Unspecified :
			(order[0] | 0x20) == 'a' ? SqlOrder.Ascending
									 : SqlOrder.Descending;
		}

		/// <summary>
		/// A table primary and everything written around it, as the one node it is.
		/// </summary>
		/// <remarks>
		/// A name and a bracketed argument list is a table-valued function and not a table, so
		/// which record comes out is decided by whether the brackets were there.
		/// </remarks>
		public static TableReference Sourced(
			string name, Clause? when, bool path, string? call, SqlList<Ast.Expression>? arguments,
			string? alias, SqlList<string>? columns, Clause? sample, SqlList<Clause>? hints, TableReference? pivot)
		{
			TableReference source = call is null
				? new TableReference.Named(name, when, path, alias, columns, sample, hints ?? [])
				: new TableReference.FunctionCall(
					new Values.Call(name, arguments ?? []),
					alias, columns, Clause.None);

			return Pivoted(source, pivot);
		}

		/// <summary>A source and the joins hanging off it, where any were written.</summary>
		/// <remarks>
		/// The right side of a join may be joins itself with no brackets saying so — <c>t1 JOIN
		/// t10 LEFT JOIN t11 ON … ON …</c>, <c>t1 JOIN t2 CROSS JOIN t3 ON …</c> — and each tail is
		/// read without its left side, which is everything before it. A tail a pivot was applied to
		/// takes its left side under the pivot.
		/// </remarks>
		public static TableReference Hanging(TableReference source, TableReference[]? tails)
		{
			foreach (var tail in tails ?? [])
				source = Hung(source, tail);

			return source;

			static TableReference Hung(TableReference source, TableReference tail)
			{
				return tail switch
				{
					TableReference.Joined joined => joined with { Left = source },
					TableReference.Pivot turned => turned with { Of = Hung(source, turned.Of) },
					TableReference.Unpivot turned => turned with { Of = Hung(source, turned.Of) },
					_ => source,
				};
			}
		}

		/// <summary>A source and the pivots applied to it, where any were written.</summary>
		/// <remarks>The source goes to the first of them, which is the innermost (<see cref="Chained"/>).</remarks>
		public static TableReference Pivoted(TableReference source, TableReference? pivot)
		{
			// Every table read asks this, and nearly every one has no pivot: nothing to allocate for that.
			if (pivot is not (TableReference.Pivot or TableReference.Unpivot))
				return source;

			// The chain from the last pivot in to the first, then built again from the first out: a loop
			// and not a call a pivot, since a chain of them is as long as the text that writes it.
			var chain = new List<TableReference>();

			for (var at = pivot; at is TableReference.Pivot or TableReference.Unpivot; )
			{
				chain.Add(at);

				at = at switch
				{
					TableReference.Pivot turned   => turned.Of,
					TableReference.Unpivot turned => turned.Of,
					_                             => null,
				};
			}

			var made = source;

			for (var at = chain.Count - 1; at >= 0; at--)
			{
				made = chain[at] switch
				{
					TableReference.Pivot turned   => turned with { Of = made },
					TableReference.Unpivot turned => turned with { Of = made },
					_                             => made,
				};
			}

			return made;
		}

		/// <summary>
		/// Pivots one after another, each applied to the one before it: <c>t PIVOT (…) p UNPIVOT (…)
		/// q</c> is <c>q</c> of <c>p</c>, and <c>p</c>'s source is left for <see cref="Pivoted"/>.
		/// </summary>
		public static TableReference Chained(TableReference first, TableReference[]? rest)
		{
			foreach (var next in rest ?? [])
			{
				first = next switch
				{
					TableReference.Pivot turned   => turned with { Of = first },
					TableReference.Unpivot turned => turned with { Of = first },
					_                             => first,
				};
			}

			return first;
		}

		/// <summary>Whether what stands in a source's brackets may stand there: a join, and not a source alone.</summary>
		/// <remarks>
		/// <c>(t1 JOIN t2 ON …)</c>, <c>(t1 CROSS JOIN t2)</c>, <c>((…))</c> and <c>({ OJ … })</c> are
		/// read. A table, a function, a rowset or a derived table alone in brackets is <c>Msg 102</c>,
		/// and so is a join a pivot ends — <c>(t1 CROSS JOIN t2 PIVOT (…) p)</c>, <c>(t1 JOIN t2 ON …
		/// PIVOT (…) p)</c> — where a pivot before an <c>ON</c> inside them is read. A table variable
		/// is no exception: <c>(@t)</c> and <c>(@t x)</c> are <c>Msg 102</c> once it is declared.
		/// </remarks>
		public static bool Bracketable(TableReference? inner)
		{
			return inner switch
			{
				TableReference.Built built => built.Bracketable,
				TableReference.Joined { Kind: SqlJoin.Cross or SqlJoin.CrossApply or SqlJoin.OuterApply, Right: var right } when Pivots(right) => false,
				TableReference.Joined or TableReference.Parenthesized or TableReference.OdbcJoin => true,
				_ => false,
			};
		}

		/// <summary>Whether a source is a pivot or an unpivot, which an ODBC join's braces may not hold alone.</summary>
		public static bool Pivots(TableReference? source)
		{
			return source is TableReference.Pivot or TableReference.Unpivot or TableReference.Built { Shape: TableReference.Built.Kind.Pivot or TableReference.Built.Kind.Unpivot };
		}

		/// <summary>
		/// Whether <c>JSON_ARRAYAGG</c>'s clauses and what follows its bracket stand as the engine
		/// reads them: the order, the null clause and the return type only after a value; a
		/// <c>WITHIN GROUP</c> only after the bare value and with no <c>OVER</c> after it; and an
		/// <c>OVER</c> after every shape but three — the order alone, and the null clause with the
		/// return type, ordered or not — which the engine answers with <c>Incorrect syntax near
		/// the keyword 'OVER'</c>.
		/// </summary>
		public static bool Aggregated(Ast.Expression[]? items, string? order, string? nulls, string? returning, Values.Windowing? tail)
		{
			if (items is null && (order is not null || nulls is not null || returning is not null))
				return false;

			if (tail is null)
				return true;

			if (tail.Within is not null)
				return order is null && nulls is null && returning is null && tail.Over is null;

			if (order is not null && nulls is null && returning is null)
				return false;

			return nulls is null || returning is null;
		}

		/// <summary>Whether a cursor's words agree with each other and with its <c>FOR</c>.</summary>
		/// <remarks>
		/// <para>
		/// As the engine answers them, every pair put to it (<c>Msg 1048</c>). A word may be said
		/// twice, and two words of one group may not — where the cursor is seen, which way it moves,
		/// what it holds, how its rows are locked. <c>FAST_FORWARD</c> is a way of moving and a way
		/// of holding at once, and refuses <c>SCROLL</c> and every lock but <c>READ_ONLY</c>.
		/// </para>
		/// <para>
		/// <c>FOR UPDATE</c> refuses what cannot be changed through — <c>STATIC</c>,
		/// <c>FAST_FORWARD</c>, <c>READ_ONLY</c> and the standard's <c>INSENSITIVE</c> — and
		/// <c>FOR READ ONLY</c> refuses the locks. A declared cursor refuses <c>READ_ONLY</c> beside
		/// it as well (<c>Msg 1058</c>); a cursor a variable is set to reads the two together.
		/// </para>
		/// </remarks>
		public static bool CursorAgrees(string[]? words, Clause? @for, bool declared = true)
		{
			if (words is null)
				return true;

			foreach (var one in words)
			{
				foreach (var other in words)
					if (one != other && (Group(one) is > 0 and var group && group == Group(other) || Faster(one, other)))
						return false;

				if (@for is Clause.CursorFor(var update, _) && (update ? Fixed(one) : Locked(one, declared)))
					return false;
			}

			return true;

			static int Group(string word)
			{
				return word switch
				{
					"LOCAL" or "GLOBAL" => 1,
					"FORWARD_ONLY" or "SCROLL" => 2,
					"STATIC" or "KEYSET" or "DYNAMIC" or "FAST_FORWARD" => 3,
					"READ_ONLY" or "SCROLL_LOCKS" or "OPTIMISTIC" => 4,
					_ => 0,
				};
			}

			static bool Faster(string one, string other)
			{
				return one == "FAST_FORWARD" && other is "SCROLL" or "SCROLL_LOCKS" or "OPTIMISTIC";
			}

			static bool Fixed(string word)
			{
				return word is "STATIC" or "FAST_FORWARD" or "READ_ONLY" or "INSENSITIVE";
			}

			static bool Locked(string word, bool declared)
			{
				return word is "SCROLL_LOCKS" or "OPTIMISTIC" || declared && word == "READ_ONLY";
			}
		}

		/// <summary>Whether a query may be a cursor's.</summary>
		/// <remarks>
		/// A select, and one that makes no table: the engine refuses <c>INTO</c> in a cursor's query
		/// (<c>Msg 154</c>) and <c>VALUES</c> standing for one (<c>Msg 156</c>), in brackets or in a
		/// union as well.
		/// </remarks>
		public static bool Cursorable(Query? query)
		{
			// A query its rule built already says so itself.
			if (query is Query.Built built)
				return (built.Is & Query.Built.Facts.Cursorable) != 0;

			// One query, which is nearly always what this is asked about, without the stack below.
			if (query is not (Query.Union or Query.Except or Query.Intersect or Query.Parenthesized or Query.Ordered))
				return query is not (null or Query.Specification { Into: not null } or Query.TableValueConstructor);

			// Every query the operators and brackets lead to, kept on a stack of its own: the parser
			// reads a chain of `UNION` as long as the text, deeper than a call each would have room for.
			var pending = new Stack<Query?>();

			pending.Push(query);

			while (pending.Count > 0)
			{
				var next = pending.Pop();

				if (next is null or Query.Specification { Into: not null } or Query.TableValueConstructor || next is Query.Built { Is: var facts } && (facts & Query.Built.Facts.Cursorable) == 0)
					return false;

				if (Combined(next) is (var left, var right))
				{
					pending.Push(right);
					pending.Push(left);
				}
				else if (next is Query.Parenthesized(var bracketed))
					pending.Push(bracketed);
				else if (next is Query.Ordered(var ordered, _, _))
					pending.Push(ordered);
			}

			return true;
		}

		/// <summary>Whether a routine's parameters pass a cursor, which a function may not.</summary>
		public static bool HasCursorParameter(Clause[]? parameters)
		{
			return parameters is not null &&
			Array.Exists(parameters, static one => one is Clause.ParameterDeclaration(_, "CURSOR", _, _, _, _));
		}

		/// <summary>Whether a routine's parameters hand a value out, which a function may not.</summary>
		public static bool HasOutputParameter(Clause[]? parameters)
		{
			return parameters is not null &&
			Array.Exists(parameters, static one => one is Clause.ParameterDeclaration { Ways: { } ways }
				&& ways.Any(static way => way is "OUTPUT" or "OUT"));
		}

		/// <summary>
		/// Whether a database attached names each option once and a broker option at most once —
		/// <c>WITH ENABLE_BROKER, NEW_BROKER</c> is <c>Msg 102</c>.
		/// </summary>
		public static bool Attached(Clause? first, Clause[]? rest)
		{
			if (!NamedOnce(first, rest))
				return false;

			var brokers = 0;

			foreach (var option in first is null ? [] : Syntax.Listed(first, rest))
				if (option is Clause.Option { Name: var name } && Array.Exists(Brokers, one => string.Equals(one, name, StringComparison.OrdinalIgnoreCase)))
					brokers++;

			return brokers <= 1;
		}

		static readonly string[] Brokers = ["ENABLE_BROKER", "NEW_BROKER", "ERROR_BROKER_CONVERSATIONS"];

		/// <summary>Whether a query assigns a variable in its select list, anywhere down its brackets.</summary>
		public static bool Assigns(Query? query)
		{
			while (query is Query.Parenthesized(var inner))
				query = inner;

			if (query is Query.Built built)
				return (built.Is & Query.Built.Facts.Assigns) != 0;

			return query is Query.Specification { Columns: var columns } && columns.Exists(static one => Queries.Assigns(one));
		}

		/// <summary>
		/// Whether a query combines, by <c>UNION</c>, <c>EXCEPT</c> or <c>INTERSECT</c>, a select that
		/// assigns a variable — on either side, in brackets, or under another such operator.
		/// </summary>
		public static bool AssignsCombined(Query? query)
		{
			// A query its rule built already says so itself.
			if (query is Query.Built built)
				return (built.Is & Query.Built.Facts.AssignsCombined) != 0;

			// Every select statement asks this, and nearly every one is one query: no stack for that.
			if (query is not (Query.Union or Query.Except or Query.Intersect or Query.Parenthesized))
				return false;

			// On a stack of its own, as Cursorable is: a chain of operators is as deep as the text is long.
			var pending = new Stack<Query?>();

			pending.Push(query);

			while (pending.Count > 0)
			{
				var next = pending.Pop();

				if (next is Query.Built { Is: var facts })
				{
					if ((facts & Query.Built.Facts.AssignsCombined) != 0)
						return true;
				}
				else if (Combined(next) is (var left, var right))
				{
					if (Assigns(left) || Assigns(right))
						return true;

					pending.Push(right);
					pending.Push(left);
				}
				else if (next is Query.Parenthesized(var inner))
					pending.Push(inner);
			}

			return false;
		}

		/// <summary>Whether a query assigns a variable and has nothing to select from — which no `ORDER BY` may follow.</summary>
		public static bool AssignsFromNothing(Query? query)
		{
			if (query is Query.Built built)
				return (built.Is & Query.Built.Facts.AssignsFromNothing) != 0;

			return query is Query.Specification { From.IsEmpty: true } && Assigns(query);
		}

		/// <summary>Whether no assignment of a list goes through a column — `@a = c = 1`, which a `MERGE` refuses.</summary>
		public static bool Unchained(Clause[]? sets)
		{
			return sets is null || Array.TrueForAll(sets, static one => one is not Clause.Set { Through: not null });
		}

		/// <summary>Whether a <c>RECEIVE</c>'s list assigns variables or retrieves columns, and not both.</summary>
		public static bool OneWay(Ast.SelectColumn? first, Ast.SelectColumn[]? rest)
		{
			var assigns  = false;
			var retrieves = false;

			foreach (var one in first is null ? [] : Syntax.Listed(first, rest))
				if (Queries.Assigns(one))
					assigns = true;
				else
					retrieves = true;

			return !(assigns && retrieves);
		}

		/// <summary>
		/// Whether a rowset call names its provider as the engine takes it: <c>OPENROWSET</c> over a
		/// provider is three arguments, the first two strings — one of them possibly in pieces — and
		/// the third a string or an object's name; a bulk rowset and every other function are their
		/// own business.
		/// </summary>
		public static bool Provided(string? name, Ast.Expression[]? arguments)
		{
			if (!string.Equals(name, "OPENROWSET", StringComparison.OrdinalIgnoreCase) || arguments is null)
				return true;

			if (arguments.Length > 0 && arguments[0] is Values.Prefixed(var word, _) && string.Equals(word, "BULK", StringComparison.OrdinalIgnoreCase))
				return true;

			// The object is a name in as many parts as it likes, which the arguments read as any
			// value and not as a string: so a string, or whatever is not one.
			return arguments.Length == 3 && Stringy(arguments[0]) && Stringy(arguments[1])
				&& (Stringy(arguments[2]) || !Values.IsLiteral(arguments[2]));

			static bool Stringy(Ast.Expression one)
			{
				return one switch
				{
					Ast.LiteralValue.String => true,
					Values.Pieced(var parts) => parts.TrueForAll(Stringy),
					_ => false,
				};
			}
		}

		/// <summary>Whether no option of a list is named twice.</summary>
		public static bool NamedOnce(Clause? first, Clause[]? rest)
		{
			var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			return Fresh(first) && Array.TrueForAll(rest ?? [], Fresh);

			bool Fresh(Clause? one)
			{
				return one is not Clause.Option { Name: var name } || seen.Add(name);
			}
		}

		/// <summary>Whether a list of options names one.</summary>
		public static bool HasOption(ReadOnlySpan<Clause> options, string name)
		{
			foreach (var one in options)
			{
				if (one is Clause.Option { Name: var named } && string.Equals(named, name, StringComparison.OrdinalIgnoreCase))
					return true;
			}

			return false;
		}

		/// <summary>Whether a column's encryption names its key, its type and its algorithm.</summary>
		public static bool Encrypts(Clause? first, Clause[]? rest)
		{
			if (first is null)
				return false;

			var options = Syntax.Listed(first, rest);

			return HasOption(options, "COLUMN_ENCRYPTION_KEY") && HasOption(options, "ENCRYPTION_TYPE") &&
				HasOption(options, "ALGORITHM");
		}

		/// <summary>Whether a natively compiled block's list names an option twice, which the engine refuses (<c>Msg 1039</c>).</summary>
		public static bool Repeats(Clause[]? strict)
		{
			var seen = new HashSet<string>(StringComparer.Ordinal);

			return Array.Exists(strict ?? [], one => one is Clause.Option { Name: var name } && !seen.Add(AtomicKey(name)));
		}

		/// <summary>
		/// Whether a natively compiled block's list names its isolation level and its language, which
		/// it must: without either it is <c>Msg 10784</c>.
		/// </summary>
		public static bool Atomically(Clause[] options)
		{
			return Array.Exists(options, static one => one is Clause.Option { Name: var name } && AtomicKey(name) == "TRANSACTION ISOLATION LEVEL") &&
			Array.Exists(options, static one => one is Clause.Option { Name: var name } && AtomicKey(name) == "LANGUAGE");
		}

		/// <summary>Whether a module's body is the one its options call for.</summary>
		/// <remarks>
		/// A module compiled natively has one body, an atomic block — <c>Msg 10783</c> for any other —
		/// and an atomic block is no other module's body, <c>Msg 10782</c>.
		/// </remarks>
		public static bool Natively(Clause[]? options, bool atomic)
		{
			return atomic == (options is not null && Array.Exists(options, static one =>
				one is Clause.Option { Name: var name } && name.Equals("NATIVE_COMPILATION", StringComparison.OrdinalIgnoreCase)));
		}

		/// <summary>An option's name as the engine matches it: its words, in capitals, and <c>TRAN</c> as <c>TRANSACTION</c>.</summary>
		static string AtomicKey(string name)
		{
			// A comment between the words is none of them: `TRANSACTION  ISOLATION /**/ LEVEL` is read, and so is one
			// that holds others, `/* a /* b */ c */`, which is one comment as the grammar reads it. What parts the words
			// is the grammar's spacing.
			var text = new System.Text.StringBuilder(name.Length);

			for (var at = 0; at < name.Length; at++)
			{
				if (name[at] == '-' && at + 1 < name.Length && name[at + 1] == '-')
				{
					while (at < name.Length && name[at] is not ('\r' or '\n'))
						at++;

					text.Append(' ');
				}
				else if (name[at] == '/' && at + 1 < name.Length && name[at + 1] == '*')
				{
					var depth = 0;

					for (; at < name.Length; at++)
					{
						if (name[at] == '/' && at + 1 < name.Length && name[at + 1] == '*')
						{
							depth++;
							at++;
						}
						else if (name[at] == '*' && at + 1 < name.Length && name[at + 1] == '/')
						{
							at++;

							if (--depth == 0)
								break;
						}
					}

					text.Append(' ');
				}
				else
					text.Append(TransactSql.TransactSqlParser.Spacing(name[at]) ? ' ' : name[at]);
			}

			var words = text.ToString().ToUpperInvariant().Split([' '], StringSplitOptions.RemoveEmptyEntries);

			if (words is ["TRAN", ..])
				words[0] = "TRANSACTION";

			return string.Join(" ", words);
		}

		/// <summary>
		/// Whether an argument is passed back only where it can be: <c>OUTPUT</c> after a variable,
		/// and <c>Msg 179</c> after anything else.
		/// </summary>
		public static bool Passes(Ast.Expression? value, string? back)
		{
			return string.IsNullOrEmpty(back) || Values.IsParameter(value);
		}

		/// <summary>Whether what a rule or a default is made of is what the engine binds.</summary>
		/// <remarks>
		/// Asked of what stands outside every subquery, and a subquery's inside is its own. A rule's
		/// condition names one variable, whatever its case — <c>@@</c> ones are not variables here
		/// — and with none it must hold a subquery (<c>Msg 160</c>); two are <c>Msg 161</c>. A
		/// default's value names what it likes. Neither draws from a sequence (<c>Msg 11719</c>).
		/// </remarks>
		public static bool Binds(Ast.Expression? value, bool rule)
		{
			if (value is null)
				return false;

			var inside    = new HashSet<ISqlSpan>(ByReference.Instance);
			var variables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			var queried   = false;
			var drawn     = false;

			SqlWalker.Walk(value, node =>
			{
				if (inside.Contains(node))
					return true;

				switch (node)
				{
					case Ast.Query query:
						queried = true;
						SqlWalker.Walk(query, held => inside.Add(held) || true);
						break;

					// A value over a query that has no place on the common tree holds the query all the same.
					case Values.Unbuilt { HoldsQuery: true }:
						queried = true;
						break;

					case Ast.Expression.Variable(var variable)
						when variable.Text.StartsWith("@", StringComparison.Ordinal) && !variable.Text.StartsWith("@@", StringComparison.Ordinal):
						variables.Add(variable.Text);
						break;

					case Values.Call { Name: var called } when Syntax.Squared(called) == "NEXT VALUE FOR":
					case Ast.Expression.NextValue:
						drawn = true;
						break;
				}

				return true;
			});

			return !drawn && (!rule || variables.Count == 1 || variables.Count == 0 && queried);
		}

		/// <summary>
		/// Whether the files an external language is made of each say what they hold, and are for
		/// two platforms where there are two.
		/// </summary>
		/// <remarks>
		/// Each names its content and its file (<c>Msg 39123</c>). Of two, the platforms differ
		/// (<c>Msg 39112</c>) — and the engine takes a platform twice in one file, so what differs
		/// is the set: a file that names none is for Windows, and one that names both is for the
		/// two together, which is neither of them. <c>(… PLATFORM = WINDOWS, PLATFORM = LINUX), (…
		/// PLATFORM = WINDOWS)</c> is read, and <c>(…), (… PLATFORM = WINDOWS)</c> is refused.
		/// </remarks>
		public static bool LanguageFiles(Clause[]? file, Clause[]? other)
		{
			return Whole(file) && (other is null || Whole(other) && Platforms(file!) != Platforms(other));

			static bool Whole(Clause[]? one)
			{
				return HasOption(one, "CONTENT") && HasOption(one, "FILE_NAME");
			}

			static int Platforms(Clause[] one)
			{
				var platforms = 0;

				foreach (var option in one)
					if (option is Clause.Option { Name: "PLATFORM", Value: var value } && Values.Wording(value) is { } platform)
						platforms |= platform == "LINUX" ? 2 : 1;

				return platforms == 0 ? 1 : platforms;
			}
		}

		/// <summary>Whether a spatial index takes an option set so, as the engine reads its list.</summary>
		/// <remarks>
		/// <para>
		/// The engine reads a spatial index's options as it reads no other list: any name set to
		/// any plain value, and then a word about a few names. <c>XML_COMPRESSION</c> and
		/// <c>STATISTICS_INCREMENTAL</c> are refused whatever they are set to (<c>Msg 155</c>). A bare
		/// <c>ON</c> or <c>OFF</c> is taken by the switches alone — <c>FOO = ON</c> is <c>Msg 155</c> where
		/// <c>FOO = BAR</c> is read — and <c>ON</c> is refused to the four a spatial index cannot be
		/// (<c>Msg 153</c>), though <c>[ON]</c>, a name, is not.
		/// </para>
		/// <para>
		/// A fill factor is a number with no exponent and not money; a whole one from 1 to 100
		/// (<c>Msg 129</c>), and one with a fraction whatever it is. A degree of parallelism is a
		/// whole number from 0 to 32767 (<c>Msg 304</c>), or a switch. A compression is one of its
		/// words, in brackets or not. None of the three is a list.
		/// </para>
		/// </remarks>
		public static bool Spatial(string? name, Clause.Option? set)
		{
			if (name is null || set is null)
				return false;

			var key   = name.ToUpperInvariant();
			var value = set.Value;
			var on    = Values.Wording(value) == "ON";
			var off   = Values.Wording(value) == "OFF";

			switch (key)
			{
				case "XML_COMPRESSION":
				case "STATISTICS_INCREMENTAL":
					return false;

				case "FILLFACTOR":
					return value is Ast.LiteralValue.Numeric { Text: var fill } && Plain(fill) &&
						(fill.IndexOf('.') >= 0 || Whole(fill) is >= 1 and <= 100);

				case "MAXDOP":
					return on || off ||
						value is Ast.LiteralValue.Numeric { Text: var degree } && Plain(degree) &&
						degree.IndexOf('.') < 0 && Whole(degree) is >= 0 and <= 32767;

				case "DATA_COMPRESSION":
					return Values.Wording(value) is { } word &&
						Array.Exists(Compressions, one => string.Equals(one, word.Trim('[', ']', '"'), StringComparison.OrdinalIgnoreCase));
			}

			return !(on || off) || Array.IndexOf(Switches, key) >= 0 && !(on && Array.IndexOf(Unswitched, key) >= 0);

			// Digits, perhaps a point and a sign: no exponent, and no currency in front.
			static bool Plain(string number)
			{
				return number.IndexOfAny(['e', 'E']) < 0 && number.TrimStart('-') is { Length: > 0 } digits && (char.IsDigit(digits[0]) || digits[0] == '.');
			}

			static long Whole(string number)
			{
				return long.TryParse(number, System.Globalization.NumberStyles.AllowLeadingSign, System.Globalization.CultureInfo.InvariantCulture, out var whole)
					? whole
					: long.MinValue;
			}
		}

		static readonly string[] Switches =
		[
			"PAD_INDEX", "SORT_IN_TEMPDB", "IGNORE_DUP_KEY", "STATISTICS_NORECOMPUTE", "DROP_EXISTING", "ONLINE",
			"ALLOW_ROW_LOCKS", "ALLOW_PAGE_LOCKS", "RESUMABLE", "OPTIMIZE_FOR_SEQUENTIAL_KEY",
		];

		static readonly string[] Unswitched   = ["ONLINE", "IGNORE_DUP_KEY", "RESUMABLE", "OPTIMIZE_FOR_SEQUENTIAL_KEY"];

		static readonly string[] Compressions = ["NONE", "ROW", "PAGE", "COLUMNSTORE", "COLUMNSTORE_ARCHIVE"];

		/// <summary>
		/// Whether a statement standing where a table does returns its rows as one: an
		/// <c>OUTPUT</c>, one, sent nowhere else, and no hints.
		/// </summary>
		/// <remarks>
		/// Without an <c>OUTPUT</c> it returns nothing (<c>Msg 10716</c>), and <c>OUTPUT … INTO</c>
		/// sends the rows elsewhere, which the engine answers the same; a second <c>OUTPUT</c> is
		/// refused, and so are the statement's own hints (<c>Msg 10718</c>).
		/// </remarks>
		public static bool Returns(Statement? statement)
		{
			return statement is
				Statement.Delete { Output: Clause.Output { Target: null, Next: null }, Options: null } or
				Statement.Update { Output: Clause.Output { Target: null, Next: null }, Options: null } or
				Statement.Merge { Output: Clause.Output { Target: null, Next: null }, Options: null } or
				Statement.Insert { Output: Clause.Output { Target: null, Next: null }, Options: null };
		}

		/// <summary>
		/// Whether a call read where an insert's column list stands names its columns as an insert
		/// may: a <c>$</c> name alone, never under a prefix.
		/// </summary>
		/// <remarks>
		/// <c>INSERT t1 (a.$ACTION) VALUES (1)</c> is <c>Msg 102</c>, and so is the same list after a
		/// function — the engine refuses the shape wherever the target came from. The brackets are
		/// read as arguments here, one reading having failed, so the question is put to them; as a
		/// value the name is ordinary, and <c>SELECT dbo.f (a.$ACTION)</c> is read.
		/// </remarks>
		public static bool Written(TableReference? target)
		{
			return target is not TableReference.FunctionCall { Function.Arguments: var arguments } ||
			arguments.TrueForAll(static one => Values.Wording(one) is not { } text || Written(text));
		}

		/// <summary>Whether a name written as a column carries no <c>$</c> part under a prefix.</summary>
		static bool Written(string text)
		{
			var part  = true;
			var first = true;

			for (var i = 0; i < text.Length; i++)
			{
				var c = text[i];

				if (c is '[')
				{
					while (++i < text.Length && text[i] is not ']') { }

					part = false;
					continue;
				}

				if (c is '"')
				{
					while (++i < text.Length && text[i] is not '"') { }

					part = false;
					continue;
				}

				if (c is '.')
				{
					part  = true;
					first = false;
					continue;
				}

				if (part && c is '$' && !first)
					return false;

				part = false;
			}

			return true;
		}

		/// <summary>
		/// Whether a web method written bare stands where one may: under a created endpoint and not
		/// under an altered one.
		/// </summary>
		/// <remarks>
		/// <c>CREATE ENDPOINT … FOR SOAP (WEBMETHOD 'm1'(…))</c> is read and <c>ALTER ENDPOINT …
		/// FOR SOAP (WEBMETHOD 'm1'(…))</c> is <c>Msg 7803</c>, "The WEBMETHOD clause is not allowed
		/// in ALTER ENDPOINT"; altered, a method is added, altered or dropped by name. The verb is
		/// read by the statement and the list by a rule of its own, so the question is put here.
		/// </remarks>
		public static bool Created(string? verb, Clause.Option? spoken)
		{
			return string.Equals(verb, "CREATE", StringComparison.OrdinalIgnoreCase) ||
			spoken?.Options is not { } options ||
			!options.Exists(static one => one is Clause.Option { Name: "WEBMETHOD" });
		}

		/// <summary>Whether a string written out is one of a catalogue, whatever its quotes and case.</summary>
		/// <remarks>
		/// <c>DATAFILETYPE = 'char'</c> and <c>FORMAT = 'csv'</c> name closed lists — <c>'foo'</c>
		/// and <c>'parquet'</c> are <c>Msg 102</c> — and the list is short enough to ask about
		/// here rather than spell as alternatives, the quotes being the lexeme's and not the
		/// value's.
		/// </remarks>
		public static bool OneOf(Ast.Expression? value, params string[] names)
		{
			if (Values.LiteralText(value) is not { } text)
				return false;

			var start = text.IndexOf('\'');
			var end   = text.LastIndexOf('\'');
			var said  = start >= 0 && end > start ? text.Substring(start + 1, end - start - 1) : text;

			return Array.Exists(names, one => string.Equals(one, said, StringComparison.OrdinalIgnoreCase));
		}

		/// <summary>
		/// Whether a procedure written against somebody else's code carries only the options it may.
		/// </summary>
		/// <remarks>
		/// <c>AS EXTERNAL NAME</c> takes <c>EXECUTE AS</c> and nothing else: <c>WITH RECOMPILE</c>
		/// and <c>WITH ENCRYPTION</c> are <c>Msg 155</c> there and read of a procedure whose body is
		/// written out.
		/// </remarks>
		public static bool Externally(Clause[]? options)
		{
			return options is null ||
			Array.TrueForAll(options, static one =>
				one is Clause.Option { Name: var name } && name.StartsWith("EXECUTE AS", StringComparison.OrdinalIgnoreCase));
		}

		/// <summary>Whether a function returning a query is told nothing about a null argument.</summary>
		/// <remarks>
		/// <c>CALLED ON NULL INPUT</c> and <c>RETURNS NULL ON NULL INPUT</c> are <c>Msg 487</c> of an
		/// inline table function and read of a scalar one.
		/// </remarks>
		public static bool Inlined(Clause[]? options)
		{
			return options is null ||
			Array.TrueForAll(options, static one =>
				one is not Clause.Option { Name: var name } ||
				!(name.EndsWith("ON NULL INPUT", StringComparison.OrdinalIgnoreCase)));
		}

		/// <summary>Whether a table computing a column has one of its own to compute it from.</summary>
		/// <remarks>
		/// <c>CREATE TABLE t (x AS 1)</c> is <c>Msg 102</c> and <c>(x AS 1, y)</c> is read, in
		/// either order. Only a column answers for it: <c>(x AS 1, INDEX i (x))</c> and <c>(x AS 1,
		/// PERIOD FOR SYSTEM_TIME (a, b))</c> are refused. A table computing nothing is asked
		/// nothing — <c>CREATE TABLE e (CONSTRAINT c CONNECTION (a TO b)) AS EDGE</c> has no column
		/// at all and is read. Asked of a table variable, a table type and a function's returned
		/// table as well, and not of <c>ALTER TABLE … ADD</c>, whose table has its columns already.
		/// </remarks>
		public static bool Stored(Clause[]? body)
		{
			return body is null ||
			!Array.Exists(body, static one => one is Clause.ColumnDefinition(_, _, not null, _)) ||
			Array.Exists(body, static one => one is Clause.ColumnDefinition(_, _, null, _));
		}

		/// <summary>Whether a key's two unbracketed options are two different ones.</summary>
		/// <remarks>
		/// The pair is a <c>FILLFACTOR</c> and one of the two old words, in either order:
		/// <c>WITH sorted_data fillfactor = 12</c> and <c>WITH fillfactor = 12
		/// sorted_data_reorg</c> are read. The same one said twice is not — <c>WITH fillfactor =
		/// 12 fillfactor = 13</c> is <c>Msg 156</c> and <c>WITH sorted_data sorted_data</c> is
		/// <c>Msg 102</c> — and neither is a third.
		/// </remarks>
		public static bool Paired(Clause first, Clause? second)
		{
			return second is null ||
			first is Clause.Option { Name: var one } && second is Clause.Option { Name: var other } &&
			!string.Equals(one, other, StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>Whether a column takes the collation it was given.</summary>
		/// <remarks>
		/// An exact numeric given both its numbers does not: <c>DECIMAL (10, 3) COLLATE
		/// Latin1_General_CI_AS</c> and <c>DEC (10, 3)</c>'s are <c>Msg 156</c>. Given one number,
		/// none at all, or <c>(MAX)</c>, it does — <c>DECIMAL (10)</c>, <c>DECIMAL</c> and
		/// <c>DECIMAL (MAX)</c> are all read with a collation, and so are <c>INT</c>, <c>FLOAT</c>,
		/// <c>MONEY</c>, <c>VARCHAR</c> and <c>XML</c>. Asked of the type as written, which is
		/// where the two numbers show.
		/// </remarks>
		public static bool Collated(string? type, Clause[]? tail)
		{
			if (type is null || tail is null)
				return true;

			var word = type.TrimStart();
			var end  = word.IndexOfAny(new[] { ' ', '\t', '(' });
			var name = end > 0 ? word.Substring(0, end) : word;

			if (!(string.Equals(name, "DECIMAL", StringComparison.OrdinalIgnoreCase) ||
				  string.Equals(name, "NUMERIC", StringComparison.OrdinalIgnoreCase) ||
				  string.Equals(name, "DEC", StringComparison.OrdinalIgnoreCase)))
				return true;

			var open = word.IndexOf('(');

			if (open < 0 || word.IndexOf(',', open) < 0)
				return true;

			return !Array.Exists(tail, static one => one is Clause.ColumnOption { Kind: "COLLATE" });
		}

		/// <summary>Whether what is said about a column after its type comes in the engine's order.</summary>
		/// <remarks>
		/// <para>
		/// Asked of the engine by every pair of twenty-four things a column may be told, by twenty-seven
		/// ways of writing its type, and in each place a column is declared (docs/next.md, 2026-09-13).
		/// A tail has a front, which is the column's storage, and then a free list.
		/// </para>
		/// <para>
		/// The front is a collation, and then one of three: a generation, <c>GENERATED ALWAYS AS …
		/// [HIDDEN]</c>; a column set; or <c>FILESTREAM</c> and <c>SPARSE</c>, each once and in
		/// either order, and after them a mask or an encryption, not both. What the type was written
		/// with decides which. A <c>FILESTREAM</c> follows a type whose brackets hold a word —
		/// <c>VARBINARY (MAX)</c>, <c>XML (CONTENT s)</c> — and nothing else (<c>Msg 102</c>), and
		/// is never encrypted; a generation follows any type but that one; a column set follows a type
		/// written with no brackets at all, <c>XML</c>, <c>INT</c>. It is how the type was written and
		/// not what it is: <c>DECIMAL (MAX)</c> takes a <c>FILESTREAM</c> and <c>VARCHAR (10)</c> no
		/// column set. A collation after any of the front is <c>Msg 156</c>, and any of it after the
		/// free list is <c>Msg 102</c>: <c>NULL MASKED WITH (…)</c>, <c>IDENTITY SPARSE</c>.
		/// </para>
		/// <para>
		/// The free list is a default, an identity, a nullability, <c>ROWGUIDCOL</c>, the constraints
		/// and an index, in any order and as often as wanted — <c>NULL NULL</c> and <c>INDEX a INDEX
		/// b</c> are read. <c>NOT FOR REPLICATION</c> stands directly after an identity, once. A
		/// computed column's tail is <c>PERSISTED</c> or not, and then the same free list; nothing of
		/// the front.
		/// </para>
		/// </remarks>
		public static bool Tailed(string? type, Clause[]? tail)
		{
			if (tail is null)
				return true;

			var at = 0;

			if (type is null)
			{
				if (Is(at, "PERSISTED"))
					at++;
			}
			else
			{
				var written = HowWritten(type);

				if (Is(at, "COLLATE"))
					at++;

				if (at < tail.Length && Kind(tail[at]) is { } kind && kind.StartsWith("GENERATED", StringComparison.Ordinal))
				{
					if (written == TypeWritten.Worded)
						return false;

					at++;
				}
				else if (Is(at, ColumnSet))
				{
					if (written != TypeWritten.Bare)
						return false;

					at++;
				}
				else
				{
					var filestream = false;
					var sparse     = false;

					for (; at < tail.Length; at++)
					{
						if (!filestream && written == TypeWritten.Worded && Is(at, "FILESTREAM"))
							filestream = true;
						else if (!sparse && Is(at, "SPARSE"))
							sparse = true;
						else
							break;
					}

					if (Is(at, "MASKED") || !filestream && Is(at, "ENCRYPTED"))
						at++;
				}
			}

			for (; at < tail.Length; at++)
			{
				var kind = Kind(tail[at]);

				if (kind is not null && (Fronting(kind) || kind == "PERSISTED"))
					return false;

				if (kind == "NOT FOR REPLICATION" && (at == 0 || Kind(tail[at - 1]) != "IDENTITY"))
					return false;
			}

			return true;

			bool Is(int index, string wanted)
			{
				return index < tail.Length && Kind(tail[index]) == wanted;
			}

			static string? Kind(Clause one)
			{
				return one is Clause.ColumnOption { Kind: var kind } ? kind : null;
			}
		}

		/// <summary>Whether an altered column is told only what an altered column may be.</summary>
		/// <remarks>
		/// The front of a tail, and one nullability at its end: <c>ALTER COLUMN c INT DEFAULT 1</c>,
		/// <c>… IDENTITY</c>, <c>… ROWGUIDCOL</c>, <c>… PRIMARY KEY</c> and <c>… NULL NULL</c> are
		/// <c>Msg 156</c>, <c>… INDEX ix</c> <c>Msg 1018</c>, and <c>… PERSISTED</c> and <c>… NOT FOR
		/// REPLICATION</c> <c>Msg 102</c>.
		/// </remarks>
		public static bool AlteredColumn(Clause? column)
		{
			if (column is not Clause.ColumnDefinition { Options: var options })
				return true;

			for (var at = 0; at < options.Length; at++)
			{
				if (options[at] is not Clause.ColumnOption { Kind: var kind, ConstraintName: null })
					return false;

				if (Fronting(kind) || at == options.Length - 1 && kind is "NULL" or "NOT NULL")
					continue;

				return false;
			}

			return true;
		}

		/// <summary>Whether something said about a column belongs to the front of its tail.</summary>
		static bool Fronting(string kind)
		{
			return kind is "COLLATE" or "FILESTREAM" or "SPARSE" or "MASKED" or "ENCRYPTED" or ColumnSet ||
			kind.StartsWith("GENERATED", StringComparison.Ordinal);
		}

		static TypeWritten HowWritten(string type)
		{
			var until = '\0';
			var open  = -1;

			for (var at = 0; at < type.Length && open < 0; at++)
			{
				var c = type[at];

				if (until != '\0')
				{
					if (c == until)
						until = '\0';
				}
				else if (c is '[')
					until = ']';
				else if (c is '"')
					until = '"';
				else if (c is '(')
					open = at;
			}

			if (open < 0)
				return TypeWritten.Bare;

			var close = type.LastIndexOf(')');

			for (var at = open + 1; at < close; at++)
				if (char.IsLetter(type[at]))
					return TypeWritten.Worded;

			return TypeWritten.Sized;
		}

		/// <summary>Whether every column of a table's body has a type, or a value it is computed from.</summary>
		/// <remarks>Asked of a result set, which unlike a table may not leave a type out (<c>Msg 102</c>).</remarks>
		public static bool Typed(Clause[]? body)
		{
			return body is not null && Array.TrueForAll(body, static one => one is not Clause.ColumnDefinition(_, null, null, _));
		}

		/// <summary>Whether what follows a column written without a type is what the engine reads there.</summary>
		/// <remarks>
		/// Its constraints, its identity and its nullability, and nothing that belongs to a type: a
		/// collation (<c>Msg 156</c>), <c>NOT FOR REPLICATION</c>, a mask, an encryption or a
		/// generation (<c>Msg 102</c>, <c>156</c>).
		/// </remarks>
		public static bool Untyped(Clause[]? tail)
		{
			return tail is null || Array.TrueForAll(tail, static one =>
				one is not Clause.ColumnOption { Kind: var kind } ||
				!(kind is "COLLATE" or "NOT FOR REPLICATION" or "MASKED" or "ENCRYPTED" ||
				  kind.StartsWith("GENERATED", StringComparison.Ordinal)));
		}

		/// <summary>Whether a column's flag, added or dropped, may be so with the options said.</summary>
		/// <remarks>
		/// Online is refused to a row GUID, <c>NOT FOR REPLICATION</c>, persistence and hiding
		/// (<c>Msg 153</c>) — offline is not. A sparse column and a mask may be changed online.
		/// </remarks>
		public static bool FlagsOnline(string? flag, Clause[]? options)
		{
			if (flag is null || options is null)
				return true;

			return options is not [Clause.Option { Value: var value }] || Values.Wording(value) != "ON" ||
				!(flag.EndsWith("ROWGUIDCOL", StringComparison.Ordinal) || flag.EndsWith("NOT FOR REPLICATION", StringComparison.Ordinal) ||
				  flag.EndsWith("PERSISTED", StringComparison.Ordinal) || flag.EndsWith("HIDDEN", StringComparison.Ordinal));
		}

		/// <summary>Whether a rowset function takes the schema written after it, and in that shape.</summary>
		/// <remarks>
		/// <para>
		/// Two of them do. <c>OPENXML</c> takes a schema with a pattern in each column or none, and
		/// takes a name instead of the brackets; <c>OPENROWSET (BULK …)</c> takes one with a number
		/// or a path in each column or none — <c>[c] VARCHAR (50) '$.stateName'</c>, a collation
		/// before either, never both (<c>Msg 102</c>) — and no name. Every other function refuses the clause altogether
		/// — <c>CHANGETABLE</c> (<c>Msg 22104</c>), <c>STRING_SPLIT</c>, <c>GENERATE_SERIES</c> and
		/// <c>OPENQUERY</c> (<c>Msg 319</c>), and <c>OPENROWSET</c> over a provider (<c>Msg 102</c>).
		/// </para>
		/// <para>
		/// A bulk rowset read as one value — <c>SINGLE_CLOB</c>, <c>SINGLE_BLOB</c>,
		/// <c>SINGLE_NCLOB</c> — has one column of its own and takes no schema (<c>Msg 5340</c>),
		/// unless a format is named beside it, which the engine reads.
		/// </para>
		/// </remarks>
		public static bool Schemas(Values.Call? call, Clause[]? schema)
		{
			if (schema is null)
				return true;

			if (call is null)
				return false;

			if (string.Equals(call.Name, "OPENXML", StringComparison.OrdinalIgnoreCase))
				return Array.TrueForAll(schema, static one => one is not Clause.JsonColumn { Ordinal: not null });

			if (!string.Equals(call.Name, "OPENROWSET", StringComparison.OrdinalIgnoreCase))
				return false;

			var bulk   = false;
			var single = false;
			var format = false;

			foreach (var argument in call.Arguments)
				switch (argument)
				{
					case Values.Prefixed(var word, _) when string.Equals(word, "BULK", StringComparison.OrdinalIgnoreCase):
						bulk = true;
						break;

					case Values.NamedArgument(var named, _) when string.Equals(named, "FORMAT", StringComparison.OrdinalIgnoreCase):
						format = true;
						break;

					case var other when Values.Wording(other) is { } lob && Array.Exists(Singles, one => string.Equals(one, lob, StringComparison.OrdinalIgnoreCase)):
						single = true;
						break;
				}

			return bulk && (format || !single) &&
				Array.TrueForAll(schema, static one => one is Clause.JsonColumn { Type: not null } and not Clause.JsonColumn { Path: not null, Ordinal: not null });
		}

		static readonly string[] Singles = ["SINGLE_CLOB", "SINGLE_BLOB", "SINGLE_NCLOB"];

		/// <summary>Whether a queue created has what its activation needs.</summary>
		/// <remarks>
		/// A procedure, how many read at once and whom they run as, all three, where an altered
		/// queue may change any of them or drop the activation altogether.
		/// </remarks>
		public static bool Activates(Clause[]? options)
		{
			if (options is null)
				return true;

			foreach (var one in options)
				if (one is Clause.Option { Name: "ACTIVATION", Options: var inner } &&
					!(HasOption(inner.AsSpan(), "PROCEDURE_NAME") && HasOption(inner.AsSpan(), "MAX_QUEUE_READERS") &&
					  HasOption(inner.AsSpan(), "EXECUTE AS")))
				{
					return false;
				}

			return true;
		}

		/// <summary>Whether a dialog's options are each said once, and relate it one way at most.</summary>
		public static bool DialogAgrees(Clause[]? options)
		{
			return NamedOnce(null, options) &&
			!(HasOption(options, "RELATED_CONVERSATION") && HasOption(options, "RELATED_CONVERSATION_GROUP"));
		}

		/// <summary>Whether <c>PREDICT</c>'s arguments are ones it takes.</summary>
		/// <remarks>
		/// A model and data, both by name and both required (<c>Msg 39036</c>), and nothing else:
		/// <c>RUNTIME = ONNX</c> is Azure SQL Edge's and Synapse's, and SQL Server refuses it
		/// (<c>Msg 102</c>). The model is any value but a subquery.
		/// </remarks>
		public static bool Predicts(Ast.Expression[]? arguments)
		{
			if (arguments is null)
				return false;

			var model = false;
			var data  = false;

			foreach (var one in arguments)
				switch (one)
				{
					case Values.NamedArgument(var name, var value)
						when string.Equals(name, "MODEL", StringComparison.OrdinalIgnoreCase):
						if (value is Ast.Expression.Subquery or Values.Unbuilt { Shape: "Expression.Subquery" })
							return false;

						model = true;
						break;

					case Values.NamedArgument(var name, _)
						when string.Equals(name, "DATA", StringComparison.OrdinalIgnoreCase):
						data = true;
						break;

					default:
						return false;
				}

			return model && data;
		}


		/// <summary>Whether a table's body may be a table type's.</summary>
		/// <remarks>
		/// <para>
		/// A table type holds columns, keys, checks and indexes, and nothing that ties it to another
		/// table or names what it holds: a foreign key and a reference are refused, a constraint's
		/// name is — an index's is its own and is read — and so is a sparse column (<c>Msg 156</c>,
		/// <c>102</c>).
		/// </para>
		/// <para>
		/// And a key or an index of its own may be told four things and no more. Unbracketed, the
		/// two words older than the brackets: <c>PRIMARY KEY WITH sorted_data</c>, <c>WITH
		/// sorted_data_reorg</c>, at the column or at the table, of a key or of a unique
		/// constraint. Bracketed, <c>IGNORE_DUP_KEY</c> and <c>BUCKET_COUNT</c>: <c>WITH
		/// (IGNORE_DUP_KEY = ON)</c>, <c>WITH (BUCKET_COUNT = 8)</c>, and an index's own <c>WITH
		/// (IGNORE_DUP_KEY = ON)</c> too. Everything else is <c>Msg 155</c> — <c>FILLFACTOR</c>,
		/// <c>PAD_INDEX</c>, <c>STATISTICS_NORECOMPUTE</c>, <c>DATA_COMPRESSION</c> — and the list
		/// is judged by each of its members rather than by its first: <c>WITH (IGNORE_DUP_KEY =
		/// ON, FILLFACTOR = 12)</c> is refused. <c>WITH pad_index</c> unbracketed is <c>Msg
		/// 102</c>. A table variable's key takes the lot; this is the type's own narrowness.
		/// </para>
		/// </remarks>
		public static bool TableTyped(Clause[]? body)
		{
			return body is not null && Array.TrueForAll(body, Typed);

			static bool Typed(Clause one)
			{
				return one switch
				{
					Clause.ConstraintDefinition { Kind: "INDEX" or "UNIQUE INDEX" } index => Told(index),
					Clause.ConstraintDefinition { Name: not null } => false,
					Clause.ConstraintDefinition { Kind: "FOREIGN KEY" or "REFERENCES" } => false,
					Clause.ConstraintDefinition { Kind: "PRIMARY KEY" or "UNIQUE" } key => Told(key),
					Clause.ColumnOption { ConstraintName: not null } => false,
					Clause.ColumnOption { Kind: "SPARSE" or "FILESTREAM" or "ENCRYPTED" or "NOT FOR REPLICATION" or ColumnSet } => false,
					Clause.ColumnDefinition { Options: var options } => options.TrueForAll(Typed),
					_ => true,
				};
			}

			// What a key or an index of a table type may be told, which is the brackets' question
			// as much as the name's: the constraint holds the list itself and says on `Bracketed`
			// which spelling it was written in.
			static bool Told(Clause.ConstraintDefinition constraint)
			{
				return constraint.Options is not { } options ||
				options.TrueForAll(one =>
					one is Clause.Option { Name: var name } &&
					Array.Exists(
						constraint.Bracketed ? TypeKeyBracketed : TypeKeyBare,
						word => string.Equals(word, name, StringComparison.OrdinalIgnoreCase)));
			}
		}

		/// <summary>Whether a table's body may be a table variable's or a returned table's.</summary>
		/// <remarks>
		/// A constraint there may not be named. <c>DECLARE @t TABLE (a1 INT CONSTRAINT C8 PRIMARY
		/// KEY)</c> is <c>Msg 156</c> and so are a named <c>UNIQUE</c>, <c>CHECK</c> and
		/// <c>DEFAULT</c>, at the column or at the table, and the same in a function's
		/// <c>RETURNS @t TABLE</c>. Without the name every one of them is read. Nor may a column be
		/// <c>FILESTREAM</c> there, or kept from replication (<c>Msg 102</c>), or refer to another
		/// table (<c>Msg 156</c>). An index keeps its
		/// own name — <c>INDEX i1 (a1)</c> is read — and a name in front of one is refused apart
		/// (<c>Msg 1018</c>). <c>CREATE TABLE</c> and <c>ALTER TABLE … ADD</c> take names as they
		/// always did, which is why this is asked of the two bodies and not of <c>DeclaredBody</c>.
		/// </remarks>
		public static bool Declared(Clause[]? body)
		{
			return body is not null && Array.TrueForAll(body, Nameless);

			static bool Nameless(Clause one)
			{
				return one switch
				{
					Clause.ConstraintDefinition { Kind: "INDEX" or "UNIQUE INDEX" } => true,
					Clause.ConstraintDefinition { Name: not null } => false,
					Clause.ColumnOption { ConstraintName: not null } => false,
					Clause.ColumnOption { Kind: "FILESTREAM" or "NOT FOR REPLICATION" } => false,
					Clause.ConstraintDefinition { Kind: "REFERENCES" } => false,
					Clause.ColumnDefinition { Options: var options } => options.TrueForAll(Nameless),
					_ => true,
				};
			}
		}

		/// <summary>Whether a table's body may be a function's returned table.</summary>
		/// <remarks>
		/// Narrower than a table variable, and the same whether the function is written in T-SQL or
		/// in a CLR assembly: a returned column is not sparse, not a column set and not
		/// <c>FILESTREAM</c> (<c>Msg 102</c>), which a table variable's may be.
		/// </remarks>
		public static bool Returned(Clause[]? body)
		{
			return body is not null && Array.TrueForAll(body, Kept);

			static bool Kept(Clause one)
			{
				return one switch
				{
					Clause.ColumnOption { Kind: "SPARSE" or "FILESTREAM" or "NOT FOR REPLICATION" or ColumnSet } => false,
					Clause.ConstraintDefinition { Kind: "REFERENCES" } => false,
					Clause.ColumnDefinition { Options: var options } => options.TrueForAll(Kept),
					_ => true,
				};
			}
		}

		/// <summary>What a table type's key or index may be told inside brackets.</summary>
		static readonly string[] TypeKeyBracketed = ["IGNORE_DUP_KEY", "BUCKET_COUNT"];

		/// <summary>And what it may be told without them, which is what `CREATE INDEX` said in 1998.</summary>
		static readonly string[] TypeKeyBare = ["SORTED_DATA", "SORTED_DATA_REORG"];

		/// <summary>A cursor's query and what stands around it, its words yet to be said.</summary>
		public static Clause.CursorDefinition Cursor(
			SqlList<Clause>? with, Query query, Clause? @for, SqlList<Clause>? hints, bool forFirst)
		{
			return new([], [], with ?? [], query, @for, hints ?? [], forFirst);
		}

		/// <summary>
		/// One setting of a catalogue's list as a node, from the words its line read: the name up
		/// to the first <c>=</c> outside brackets, and the rest as the value's words.
		/// </summary>
		/// <remarks>
		/// For a statement that keeps its words as a tail and its settings beside them as nodes,
		/// so that a check has names to match. <c>DECRYPTION BY PASSWORD = 'p'</c> is named by
		/// its three words; <c>ENCLAVE_COMPUTATIONS (SIGNATURE = 0x01)</c>, with no <c>=</c> of
		/// its own, by the word before its bracket.
		/// </remarks>
		public static Clause.Option Setting(string words)
		{
			var depth = 0;

			for (var at = 0; at < words.Length; at++)
				switch (words[at])
				{
					case '(':
						depth++;
						break;

					case ')':
						depth--;
						break;

					case '=' when depth == 0:
						return new Clause.Option(
							Syntax.Squared(words.Substring(0, at)), Values.Reference(words.Substring(at + 1).Trim()), Clause.None);
				}

			var open = words.IndexOf('(');

			return new Clause.Option(Syntax.Squared(open < 0 ? words : words.Substring(0, open)), null, Clause.None);
		}

		/// <summary>
		/// The named queries written in front of a statement, put where they belong.
		/// </summary>
		/// <remarks>
		/// A <c>WITH</c> may precede five statements, and each of the five keeps it; any other
		/// statement has no <c>WITH</c> to be given, and the grammar does not offer it one.
		/// </remarks>
		public static Statement Preceded(SqlList<Clause>? with, Statement statement)
		{
			return with is not { Length: > 0 } ? statement : statement switch
			{
				Statement.Insert insert => insert with { With = with },
				Statement.Update update => update with { With = with },
				Statement.Delete delete => delete with { With = with },
				Statement.Merge merge => merge with { With = with },
				_ => statement,
			};
		}

		/// <summary>
		/// A routine with the word it was opened with, where that is not the record's own.
		/// </summary>
		public static Statement Verbed(Statement routine, string verb)
		{
			return routine switch
			{
				Statement.CreateProcedure one => one with { Verb = Syntax.Squared(verb) },
				Statement.CreateFunction one => one with { Verb = Syntax.Squared(verb) },
				Statement.CreateTrigger one => one with { Verb = Syntax.Squared(verb) },
				Statement.ViewDefinition one => one with { Verb = Syntax.Squared(verb) },
				_ => routine,
			};
		}

		static (Query Left, Query Right)? Combined(Query? query)
		{
			return query switch
			{
				Query.Union(var left, var right, _)     => (left, right),
				Query.Except(var left, var right, _)    => (left, right),
				Query.Intersect(var left, var right, _) => (left, right),
				_                                       => null,
			};
		}

		/// <summary>Nodes told apart by identity, which a record's own equality does not do.</summary>
		internal sealed class ByReference : IEqualityComparer<ISqlSpan>
		{
			public static readonly ByReference Instance = new();

			public bool Equals(ISqlSpan? x, ISqlSpan? y)
			{
				return ReferenceEquals(x, y);
			}

			public int GetHashCode(ISqlSpan obj)
			{
				return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
			}
		}
	}
}
