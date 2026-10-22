using System;

namespace DotGram.Sql;

/// <summary>
/// What every hierarchy needs and none of them owns: the lists a grammar gathers, and the
/// words it matched read back as the constants they stand for.
/// </summary>
/// <remarks>
/// The words are told apart by length and first letter, which costs nothing and allocates
/// nothing — the alternative is the matched text, and a capture cuts a string.
/// </remarks>
internal static class Syntax
{
	/// <summary>
	/// A node a reading rebuilt with <c>with</c> out of nodes it had already located, told the
	/// range from the first of them to the last: a copy made while reading is not an edit, and
	/// is not left saying it is stale.
	/// </summary>
	/// <remarks>
	/// Where nothing was located — a plain reading — the copy is handed back as it is.
	/// </remarks>
	internal static T Joined<T>(T made, ISqlSpan first, ISqlSpan last) where T : ISqlLocatable
	{
		if (!first.Span.Known && !last.Span.Known)
			return made;

		var from = first.Span.Known ? first.Span : last.Span;
		var to   = last.Span.Known ? last.Span : first.Span;

		made.Locate(from.At, to.End - from.At, from.GapStart);

		return made;
	}

	/// <summary>What a call with no names is handed, once rather than per call.</summary>
	public static readonly SqlList<string> NoNames = [];

	/// <summary>A head and a tail as one array, which is what a separated list comes to.</summary>
	public static T[] Listed<T>(T first, T[]? rest)
	{
		if (rest is null || rest.Length == 0)
			return [first];

		var all = new T[rest.Length + 1];

		all[0] = first;
		rest.CopyTo(all, 1);

		return all;
	}

	/// <summary>A session's events and the targets written after them, as its one list of pieces.</summary>
	public static Clause[] Pieces(Clause[]? events, Clause[]? targets)
	{
		return [.. events ?? [], .. targets ?? []];
	}

	/// <summary>A head and a tail of names as one array, the way <see cref="Listed"/> does nodes.</summary>
	public static string[] Named(string first, string[]? rest)
	{
		return Listed(first, rest);
	}

	/// <summary>Names as sort specifications with no direction, which is how a column list stands.</summary>
	public static Clause[] Columns(string[]? names)
	{
		return names is null ? [] : [.. names.Select(static one => new Clause.SortSpecification(new Expression.ColumnReference(one), SqlOrder.Unspecified))];
	}

	/// <summary>One word as an option with nothing set, or nothing where none was written.</summary>
	public static Clause[] Unit(string? word)
	{
		return word is null ? [] : [new Clause.Option(Squared(word), null, Clause.None)];
	}

	/// <summary>A grouping column with what was written after it: a collation, a hint.</summary>
	public static Expression Grouped(Expression column, string? collation, string? hint)
	{
		var made = collation is null ? column : new Expression.Collated(column, collation);

		return hint is null ? made : new Expression.Hinted(made, Spaced(hint));
	}

	/// <summary>
	/// A routine with the word it was opened with, where that is not the record's own.
	/// </summary>
	/// <remarks>
	/// The four are one syntax with four bodies — <c>CREATE OR ALTER</c> stands in front of
	/// all of them and is read once — so which word was written is a fact about the statement
	/// rather than about which of the four it is.
	/// </remarks>
	public static Statement Verbed(Statement routine, string verb)
	{
		return routine switch
		{
			Statement.CreateProcedure one => one with { Verb = Squared(verb) },
			Statement.CreateFunction one => one with { Verb = Squared(verb) },
			Statement.CreateTrigger one => one with { Verb = Squared(verb) },
			Statement.ViewDefinition one => one with { Verb = Squared(verb) },
			_ => routine,
		};
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
	public static Expression Hinted(Expression value, string? words)
	{
		return words is null || words.Length == 0 ? value : new Expression.Hinted(value, Spaced(words));
	}

	/// <summary>
	/// What a catalogue statement wrote after its name, as the words were written — or null
	/// where it wrote nothing (<see cref="Statement.Definition.Tail"/>).
	/// </summary>
	public static string? Tail(string? words)
	{
		return words is null || words.Length == 0 ? null : Spaced(words);
	}

	/// <summary>Words as written, with the whitespace between them made one space.</summary>
	public static string Spaced(string words)
	{
		return Run(words, false);
	}

	/// <summary>Text with every whitespace character taken out: <c>(1 . 2 . 3 . 4)</c> as <c>(1.2.3.4)</c>.</summary>
	/// <summary>
	/// Whether a name may be called as a function: a delimited name of one part may not. The
	/// engine refuses <c>[LEN]('a')</c> and <c>"LEN"('a')</c>, and reads <c>dbo.[LEN]('a')</c>.
	/// </summary>
	public static bool Callable(string name)
	{
		// Not through an empty part: `.f2 (1)` and `db..f2 (1)` are `Msg 102` as a value, where
		// `FROM .f2 (1)` and `FROM dbo..f2 (1)` read a table-valued function — that is another
		// rule's business.
		if (name.Length > 0 && (name[0] == '.' || name.IndexOf("..", StringComparison.Ordinal) >= 0))
			return false;

		if (name.Length == 0 || (name[0] != '[' && name[0] != '"'))
			return true;

		var close = name[0] == '[' ? ']' : '"';

		for (var at = 1; at < name.Length; at++)
		{
			if (name[at] != close)
				continue;

			if (at + 1 < name.Length && name[at + 1] == close)
			{
				at++;
				continue;
			}

			return at + 1 < name.Length;
		}

		return true;
	}

	/// <summary>
	/// The names a date part is written with: the published ones and their abbreviations, and
	/// <c>w</c> beside them, which the engine reads for <c>weekday</c> and the table leaves out.
	/// </summary>
	static readonly HashSet<string> DateParts = new(StringComparer.OrdinalIgnoreCase)
	{
		"year", "yy", "yyyy", "quarter", "qq", "q", "month", "mm", "m", "dayofyear", "dy", "y",
		"day", "dd", "d", "week", "wk", "ww", "weekday", "dw", "w", "hour", "hh", "minute", "mi",
		"n", "second", "ss", "s", "millisecond", "ms", "microsecond", "mcs", "nanosecond", "ns",
		"tzoffset", "tz", "iso_week", "isowk", "isoww",
	};

	/// <summary>The functions whose first argument is a date part.</summary>
	static readonly HashSet<string> DatePartFunctions = new(StringComparer.OrdinalIgnoreCase)
	{
		"DATEPART", "DATENAME", "DATEADD", "DATEDIFF", "DATEDIFF_BIG", "DATETRUNC", "DATE_BUCKET",
	};

	/// <summary>Whether a name, bracketed, quoted or neither, is a date part's.</summary>
	public static bool IsDatePart(string? name)
	{
		return name is not null && DateParts.Contains(Unquoted(name));
	}

	/// <summary>Whether a name of one part is a function whose first argument is a date part.</summary>
	public static bool IsDatePartFunction(string name)
	{
		return DatePartFunctions.Contains(name);
	}

	/// <summary>Whether a value is a column and nothing more, brackets aside.</summary>
	public static bool IsColumn(Expression? value)
	{
		// A loop down the brackets rather than a call a bracket: the parser reads any number of them.
		while (value is Expression.Parenthesized(var inner))
			value = inner;

		return value is Expression.ColumnReference;
	}

	static string Unquoted(string name)
	{
		return name.Length >= 2 && (name[0] == '[' && name[^1] == ']' || name[0] == '"' && name[^1] == '"')
			? name[1..^1]
			: name;
	}

	/// <summary>
	/// Whether a select list holds T-SQL's <c>IDENTITY(…)</c>, which only a <c>SELECT … INTO</c>
	/// may: the engine refuses one without the <c>INTO</c> when it compiles the statement,
	/// <c>Msg 177</c>.
	/// </summary>
	public static bool HasIdentity(Clause[] columns)
	{
		foreach (var one in columns)
			if (one is Clause.DerivedColumn { Value: Expression.RoutineInvocation { Name: var name, Word: not null } }
				&& string.Equals(name, "IDENTITY", StringComparison.OrdinalIgnoreCase))
				return true;

		return false;
	}

	public static string Compacted(string text)
	{
		var kept = new char[text.Length];
		var at   = 0;

		foreach (var one in text)
			if (!char.IsWhiteSpace(one))
				kept[at++] = one;

		return new string(kept, 0, at);
	}

	/// <summary>
	/// Whether a bracket holds an IPv4 address: four runs of digits and the three dots between
	/// them, however spaced, and after a colon four more for the mask — which is how an
	/// endpoint's <c>LISTENER_IP</c> is written. The lexer reads <c>1.2</c> as one number, so
	/// the parts are counted here and not by a rule.
	/// </summary>
	public static bool IsAddress(string text)
	{
		var inside = Compacted(text);

		if (inside.Length < 2 || inside[0] != '(' || inside[inside.Length - 1] != ')')
			return false;

		var halves = inside.Substring(1, inside.Length - 2).Split(':');

		return halves.Length <= 2 && Array.TrueForAll(halves, IsFourPart);
	}

	/// <summary>Four runs of digits with a dot between each two, and nothing else.</summary>
	static bool IsFourPart(string text)
	{
		var parts  = 1;
		var digits = 0;

		foreach (var one in text)
		{
			if (one == '.')
			{
				if (digits == 0)
					return false;

				parts++;
				digits = 0;
			}
			else if (one is >= '0' and <= '9')
				digits++;
			else
				return false;
		}

		return parts == 4 && digits > 0;
	}

	/// <summary>The arguments that were written, in order, leaving out the optional ones that were not.</summary>
	public static Expression[] Written(params Expression?[] arguments)
	{
		return arguments.Where(static one => one is not null).Select(static one => one!).ToArray();
	}

	/// <summary>
	/// A run of words, one space between them — and what stands inside quotes copied
	/// character for character.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A literal is not a run of words. <c>'a  b'</c> is one value with two spaces in it and
	/// <c>'default()'</c> is not a keyword to be raised, so neither the spacing nor the case of
	/// anything between quotes is this function's to change — which is what <c>MASKED WITH
	/// (FUNCTION = 'default()')</c> came back from as <c>'DEFAULT()'</c>. A closing quote written twice is
	/// one inside the quotes, as T-SQL reads <c>[a]]  b]</c>, <c>"a""  b"</c> and <c>'a''  b'</c>.
	/// </para>
	/// <para>
	/// A line comment ends where its line does, so the line end after one stays a line end: made one
	/// space, <c>--x</c> and the line under it would be one comment, which swallows what the line under
	/// it says. A block comment, the comments inside it counted, is copied as it stands, so that a quote or a
	/// <c>--</c> inside it is neither. A space is what the grammar's spacing is
	/// (<see cref="TransactSql.TransactSqlParser.Spacing"/>).
	/// </para>
	/// </remarks>
	static string Run(string words, bool raised)
	{
		var made  = new System.Text.StringBuilder(words.Length);
		var until = '\0';

		for (var at = 0; at < words.Length; at++)
		{
			var c = words[at];

			if (until != '\0')
			{
				made.Append(c);

				if (c == until)
				{
					// `]]`, `""` and `''`: the quote written twice, and still inside.
					if (at + 1 < words.Length && words[at + 1] == until)
						made.Append(words[++at]);
					else
						until = '\0';
				}

				continue;
			}

			// A block comment, the comments inside it counted, kept as it was written: a quote or a `--` inside it is
			// neither.
			if (c == '/' && at + 1 < words.Length && words[at + 1] == '*')
			{
				var depth = 0;

				for (; at < words.Length; at++)
				{
					made.Append(raised ? char.ToUpperInvariant(words[at]) : words[at]);

					if (words[at] == '/' && at + 1 < words.Length && words[at + 1] == '*')
					{
						made.Append('*');
						depth++;
						at++;
					}
					else if (words[at] == '*' && at + 1 < words.Length && words[at + 1] == '/')
					{
						made.Append('/');
						at++;

						if (--depth == 0)
							break;
					}
				}

				continue;
			}

			// A quote of any of the three kinds, and what it is closed by.
			if (c is '\'' or '"' or '[')
			{
				made.Append(c);
				until = c == '[' ? ']' : c;

				continue;
			}

			// A line comment, to the end of its line, and that end as one line end.
			if (c == '-' && at + 1 < words.Length && words[at + 1] == '-')
			{
				for (; at < words.Length && words[at] is not ('\r' or '\n'); at++)
					made.Append(raised ? char.ToUpperInvariant(words[at]) : words[at]);

				if (at < words.Length)
					made.Append('\n');

				continue;
			}

			if (TransactSql.TransactSqlParser.Spacing(c))
			{
				if (made.Length > 0 && made[made.Length - 1] is not (' ' or '\n'))
					made.Append(' ');
			}
			else
			{
				made.Append(raised ? char.ToUpperInvariant(c) : c);
			}
		}

		return made.ToString().TrimEnd();
	}

	/// <summary>
	/// Arguments with the clauses written after the last of them — a JSON constructor's
	/// order, null treatment and return type — hung on that last argument, since they are
	/// written where it ends.
	/// </summary>
	public static Expression[] Tailed(Expression[] items, params string?[] clauses)
	{
		var words = string.Join(" ", clauses.Where(static one => one is not null).Select(static one => Spaced(one!)));

		if (words.Length == 0)
			return items;

		if (items.Length == 0)
			return [new Expression.ColumnReference(words)];

		var made = (Expression[])items.Clone();

		made[^1] = new Expression.Hinted(made[^1], words);

		return made;
	}

	/// <summary>A value with the unit after it, or the value alone where none was written.</summary>
	public static Expression Measured(Expression value, string? unit)
	{
		return unit is null ? value : new Expression.Measured(value, Squared(unit));
	}

	/// <summary>Whether a key is enforced: null where nothing was said, false for <c>NOT ENFORCED</c>.</summary>
	public static bool? Enforced(string? words)
	{
		return words is null ? null : !Squared(words).StartsWith("NOT", StringComparison.Ordinal);
	}

	/// <summary>A <c>REFERENCES</c> from its parts, the actions read back from the words.</summary>
	public static Clause.References Referenced(string table, SqlList<string>? columns, string[]? actions, string? replication = null)
	{
		string? onDelete = null, onUpdate = null;
		var replicated = false;

		foreach (var action in actions ?? [])
		{
			var squared = Squared(action);

			if (squared.StartsWith("ON DELETE ", StringComparison.Ordinal))
				onDelete = squared[10..];
			else if (squared.StartsWith("ON UPDATE ", StringComparison.Ordinal))
				onUpdate = squared[10..];
			else
				replicated = true;
		}

		return new Clause.References(table, columns, onDelete, onUpdate, replicated || replication is not null);
	}

	/// <summary>Whether a foreign key says what it does on a delete, and on an update, once each at most.</summary>
	public static bool ActedOnce(string[]? actions)
	{
		var deletes = 0;
		var updates = 0;

		foreach (var action in actions ?? [])
		{
			var squared = Squared(action);

			if (squared.StartsWith("ON DELETE ", StringComparison.Ordinal))
				deletes++;
			else if (squared.StartsWith("ON UPDATE ", StringComparison.Ordinal))
				updates++;
		}

		return deletes <= 1 && updates <= 1;
	}

	/// <summary>A <c>CONNECTION</c>'s pairs and the actions after them, as one list.</summary>
	public static Clause[] Connected(Clause first, Clause[]? rest, string[]? actions)
	{
		return [.. Listed(first, rest), .. (actions ?? []).Select(static one => (Clause)new Clause.Option(Squared(one), null, Clause.None))];
	}

	/// <summary>Whether a word that is <c>ON</c> or <c>OFF</c> was the first of the two.</summary>
	public static bool Switched(string word)
	{
		return (word[0] | 0x20) == 'o' && word.Length == 2;
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

	/// <summary>Which comparison operator was written (§8.2's <c>&lt;comp op&gt;</c>).</summary>
	/// <summary>A <c>BETWEEN</c>, <c>IN</c> or <c>LIKE</c> predicate with the <c>NOT</c> read in front of it.</summary>
	public static Expression Negated(Expression predicate, bool not)
	{
		return !not ? predicate : predicate switch
		{
			Expression.Between between => between with { Negated = true },
			Expression.In @in => @in with { Negated = true },
			Expression.Like like => like with { Negated = true },
			_ => predicate,
		};
	}

	public static SqlComparison Compared(string operatorText)
	{
		return Compacted(operatorText) switch
		{
			"=" => SqlComparison.Equal,
			"<>" => SqlComparison.NotEqual,
			"!=" => SqlComparison.NotEqualBang,
			"<" => SqlComparison.Less,
			"<=" => SqlComparison.LessOrEqual,
			"!<" => SqlComparison.NotLess,
			">" => SqlComparison.Greater,
			"!>" => SqlComparison.NotGreater,
			_ => SqlComparison.GreaterOrEqual,
		};
	}

	/// <summary>Which join was written, from the words in front of <c>JOIN</c>.</summary>
	/// <remarks><c>OUTER</c> is noise beside <c>LEFT</c>, <c>RIGHT</c> and <c>FULL</c> (§7.7).</remarks>
	/// <summary>Whether <c>OUTER</c> was written, which is a word and not a meaning.</summary>
	public static bool Outer(string? kind)
	{
		return kind is not null && kind.EndsWith("OUTER", StringComparison.OrdinalIgnoreCase);
	}

	public static SqlJoin Joined(string? kind)
	{
		// The text is the whole of what was written, `LEFT OUTER` and not `LEFT`, so the
		// first word is what is asked about.
		if (kind is null)
			return SqlJoin.Unspecified;

		if (kind.StartsWith("LEFT", StringComparison.OrdinalIgnoreCase))
			return SqlJoin.Left;

		if (kind.StartsWith("RIGHT", StringComparison.OrdinalIgnoreCase))
			return SqlJoin.Right;

		return kind.StartsWith("FULL", StringComparison.OrdinalIgnoreCase)
			? SqlJoin.Full
			: SqlJoin.Inner;
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
		return with is not { Length: > 0 } list ? statement : statement switch
		{
			Statement.Select select => select with { With = list },
			Statement.Insert insert => insert with { With = with },
			Statement.Update update => update with { With = with },
			Statement.Delete delete => delete with { With = with },
			Statement.Merge merge => merge with { With = with },
			_ => statement,
		};
	}

	/// <summary>
	/// A table primary and everything written around it, as the one node it is.
	/// </summary>
	/// <remarks>
	/// A name and a bracketed argument list is a table-valued function and not a table, so
	/// which record comes out is decided by whether the brackets were there.
	/// </remarks>
	public static TableReference Sourced(
		string name, Clause? when, bool path, string? call, SqlList<Expression>? arguments,
		string? alias, SqlList<string>? columns, Clause? sample, SqlList<Clause>? hints, TableReference? pivot)
	{
		TableReference source = call is null
			? new TableReference.Named(name, when, path, alias, columns, sample, hints ?? [])
			: new TableReference.FunctionCall(
				new Expression.RoutineInvocation(name, arguments ?? []),
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
			TableReference.Joined { Kind: SqlJoin.Cross or SqlJoin.CrossApply or SqlJoin.OuterApply, Right: var right } when Pivots(right) => false,
			TableReference.Joined or TableReference.Parenthesized or TableReference.OdbcJoin => true,
			_ => false,
		};
	}

	/// <summary>Whether a source is a pivot or an unpivot, which an ODBC join's braces may not hold alone.</summary>
	public static bool Pivots(TableReference? source)
	{
		return source is TableReference.Pivot or TableReference.Unpivot;
	}

	/// <summary>A call and what was written after it, where anything was.</summary>
	public static Expression Called(Expression call, Expression.WindowFunction? tail)
	{
		return tail is null ? call : tail with { Function = call };
	}

	/// <summary>
	/// Whether <c>JSON_ARRAYAGG</c>'s clauses and what follows its bracket stand as the engine
	/// reads them: the order, the null clause and the return type only after a value; a
	/// <c>WITHIN GROUP</c> only after the bare value and with no <c>OVER</c> after it; and an
	/// <c>OVER</c> after every shape but three — the order alone, and the null clause with the
	/// return type, ordered or not — which the engine answers with <c>Incorrect syntax near
	/// the keyword 'OVER'</c>.
	/// </summary>
	public static bool Aggregated(Expression[]? items, string? order, string? nulls, string? returning, Expression.WindowFunction? tail)
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

	/// <summary>
	/// A primary and what was reached through it: the members written after it, and the zones
	/// it is read in, each with the collation written in front of it. Each member and each
	/// zone was built with its own left side null, the way a predicate tail is, so the chain
	/// is closed here.
	/// </summary>
	public static Expression Reached(
		Expression primary, Expression.Member[]? members, Expression[]? zones)
	{
		if (members is not null)
			foreach (var member in members)
				primary = Syntax.Joined(member with { Of = primary }, primary, member);

		if (zones is not null)
			foreach (var zone in zones)
			{
				var at = (Expression.RoutineInvocation)zone;
				var of = at.Arguments[0] is Expression.Collated how ? Syntax.Joined(how with { Value = primary }, primary, how) : primary;

				primary = Syntax.Joined(at with { Arguments = [of, at.Arguments[1]] }, primary, at);
			}

		return primary;
	}

	/// <summary>A word the grammar matched, as the one constant that stands for it.</summary>
	public static SqlTruth TruthOf(string word)
	{
		return (word[0] | 0x20) switch
		{
			't' => SqlTruth.True,
			'f' => SqlTruth.False,
			_ => SqlTruth.Unknown,
		};
	}

	public static string Aggregate(string word)
	{
		return (word[0] | 0x20) switch
		{
			'a' => "AVG",
			's' => "SUM",
			'c' => "COUNT",
			'm' => (word[1] | 0x20) == 'a' ? "MAX" : "MIN",
			_ => word,
		};
	}

	public static string Quantified(string word)
	{
		return (word[0] | 0x20) switch
		{
			'a' => (word[1] | 0x20) == 'l' ? "ALL" : "ANY",
			's' => "SOME",
			_ => word,
		};
	}

	/// <summary>
	/// A set quantifier, as it was written.
	/// </summary>
	/// <remarks>
	/// It used to come back as one of two constants, and that was a small lie of the kind
	/// this tree is not allowed to tell: <c>SELECT ALL a</c> and <c>SELECT a</c> are two
	/// texts, and a parser that answers <c>null</c> for the first has decided something the
	/// author did not ask it to decide. What a missing <c>ALL</c> <em>means</em> is a question
	/// for whatever reads the tree.
	/// </remarks>
	public static string? Distinctly(string? word)
	{
		return word;
	}

	/// <summary>What a match predicate was qualified by, as one word or two.</summary>
	public static string? Matched(string? unique, string? kind)
	{
		var partial = kind is not null && (kind[0] | 0x20) == 'p';

		return unique is null
			? kind is null ? null : partial ? "PARTIAL" : "FULL"
			: kind is null ? "UNIQUE" : partial ? "UNIQUE PARTIAL" : "UNIQUE FULL";
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

			if (next is null or Query.Specification { Into: not null } or Query.TableValueConstructor)
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

	/// <summary>Whether a trigger's events are of the kind its time takes: <c>INSTEAD OF</c> takes statements alone.</summary>
	public static bool Fired(string time, string[] events)
	{
		return time != "INSTEAD OF" || events.All(static one => one is "INSERT" or "UPDATE" or "DELETE");
	}

	/// <summary>Whether no word of a list is repeated.</summary>
	public static bool Distinct(string first, string[]? rest)
	{
		return Listed(first, rest).Distinct(StringComparer.Ordinal).Count() == 1 + (rest?.Length ?? 0);
	}

	/// <summary>Whether a name was written bare, rather than in brackets or quotes.</summary>
	public static bool IsBare(string? name)
	{
		return name is { Length: > 0 } && name[0] is not ('[' or '"');
	}

	/// <summary>
	/// Whether a type is one <c>JSON_VALUE</c> may return: any but the large and the special —
	/// <c>XML</c>, <c>TEXT</c>, <c>NTEXT</c>, <c>IMAGE</c>, <c>SQL_VARIANT</c>, <c>JSON</c>, a
	/// vector, the CLR types and a type of the user's — each <c>Msg 102</c>.
	/// </summary>
	public static bool JsonValued(string? type)
	{
		if (type is null || type.IndexOf('.') >= 0)
			return false;

		var end  = type.IndexOfAny([' ', '(', '\t', '\r', '\n']);
		var word = end < 0 ? type : type.Substring(0, end);

		return !Array.Exists(NotJsonValued, one => string.Equals(one, word, StringComparison.OrdinalIgnoreCase));
	}

	static readonly string[] NotJsonValued =
		["XML", "TEXT", "NTEXT", "IMAGE", "SQL_VARIANT", "JSON", "VECTOR", "GEOGRAPHY", "GEOMETRY", "HIERARCHYID"];

	/// <summary>
	/// Whether a database attached names each option once and a broker option at most once —
	/// <c>WITH ENABLE_BROKER, NEW_BROKER</c> is <c>Msg 102</c>.
	/// </summary>
	public static bool Attached(Clause? first, Clause[]? rest)
	{
		if (!NamedOnce(first, rest))
			return false;

		var brokers = 0;

		foreach (var option in first is null ? [] : Listed(first, rest))
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

		return query is Query.Specification { Columns: var columns } && columns.Exists(static one => one is Clause.VariableAssignment);
	}

	/// <summary>
	/// Whether a query combines, by <c>UNION</c>, <c>EXCEPT</c> or <c>INTERSECT</c>, a select that
	/// assigns a variable — on either side, in brackets, or under another such operator.
	/// </summary>
	public static bool AssignsCombined(Query? query)
	{
		// Every select statement asks this, and nearly every one is one query: no stack for that.
		if (query is not (Query.Union or Query.Except or Query.Intersect or Query.Parenthesized))
			return false;

		// On a stack of its own, as Cursorable is: a chain of operators is as deep as the text is long.
		var pending = new Stack<Query?>();

		pending.Push(query);

		while (pending.Count > 0)
		{
			var next = pending.Pop();

			if (Combined(next) is (var left, var right))
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

	/// <summary>The two sides of a <c>UNION</c>, <c>EXCEPT</c> or <c>INTERSECT</c>, or null for any other query.</summary>
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

	/// <summary>Whether a query assigns a variable and has nothing to select from — which no `ORDER BY` may follow.</summary>
	public static bool AssignsFromNothing(Query? query)
	{
		return query is Query.Specification { From.IsEmpty: true } && Assigns(query);
	}

	/// <summary>Whether no assignment of a list goes through a column — `@a = c = 1`, which a `MERGE` refuses.</summary>
	public static bool Unchained(Clause[]? sets)
	{
		return sets is null || Array.TrueForAll(sets, static one => one is not Clause.Set { Through: not null });
	}

	/// <summary>Whether a <c>RECEIVE</c>'s list assigns variables or retrieves columns, and not both.</summary>
	public static bool OneWay(Clause? first, Clause[]? rest)
	{
		var assigns  = false;
		var retrieves = false;

		foreach (var one in first is null ? [] : Listed(first, rest))
			if (one is Clause.VariableAssignment)
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
	public static bool Provided(string? name, Expression[]? arguments)
	{
		if (!string.Equals(name, "OPENROWSET", StringComparison.OrdinalIgnoreCase) || arguments is null)
			return true;

		if (arguments.Length > 0 && arguments[0] is Expression.Prefixed(var word, _) && string.Equals(word, "BULK", StringComparison.OrdinalIgnoreCase))
			return true;

		// The object is a name in as many parts as it likes, which the arguments read as any
		// value and not as a string: so a string, or whatever is not one.
		return arguments.Length == 3 && Stringy(arguments[0]) && Stringy(arguments[1])
			&& (Stringy(arguments[2]) || arguments[2] is not Expression.Literal);

		static bool Stringy(Expression one)
		{
			return one switch
			{
				Expression.Literal { Kind: SqlLiteralKind.Text or SqlLiteralKind.National } => true,
				Expression.Pieced(var parts) => parts.TrueForAll(Stringy),
				_ => false,
			};
		}
	}

	/// <summary>
	/// Whether a statement's words hold no local variable: an <c>@</c> that opens a name, outside
	/// a string, and is not the <c>@@</c> of the server's own.
	/// </summary>
	public static bool WithoutVariable(string? words)
	{
		if (words is null)
			return true;

		var inString = false;

		for (var at = 0; at < words.Length; at++)
		{
			var one = words[at];

			if (one == '\'')
			{
				inString = !inString;
				continue;
			}

			if (inString || one != '@')
				continue;

			if (at + 1 < words.Length && words[at + 1] == '@')
			{
				at++;
				continue;
			}

			if (at > 0 && (char.IsLetterOrDigit(words[at - 1]) || words[at - 1] is '_' or '$' or '#' or '"' or ']'))
				continue;

			return false;
		}

		return true;
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

		var options = Listed(first, rest);

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
	public static bool Passes(Expression? value, string? back)
	{
		return string.IsNullOrEmpty(back) || value is Expression.Literal(SqlLiteralKind.Parameter, _);
	}

	/// <summary>Whether what a rule or a default is made of is what the engine binds.</summary>
	/// <remarks>
	/// Asked of what stands outside every subquery, and a subquery's inside is its own. A rule's
	/// condition names one variable, whatever its case — <c>@@</c> ones are not variables here
	/// — and with none it must hold a subquery (<c>Msg 160</c>); two are <c>Msg 161</c>. A
	/// default's value names what it likes. Neither draws from a sequence (<c>Msg 11719</c>).
	/// </remarks>
	public static bool Binds(Expression? value, bool rule)
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
				case Query query:
					queried = true;
					SqlWalker.Walk(query, held => inside.Add(held) || true);
					break;

				case Expression.Literal(SqlLiteralKind.Parameter, var name)
					when name.StartsWith("@", StringComparison.Ordinal) && !name.StartsWith("@@", StringComparison.Ordinal):
					variables.Add(name);
					break;

				case Expression.RoutineInvocation { Name: var called } when Squared(called) == "NEXT VALUE FOR":
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
				if (option is Clause.Option { Name: "PLATFORM", Value: Expression.ColumnReference(var platform) })
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
		var on    = value is Expression.ColumnReference("ON");
		var off   = value is Expression.ColumnReference("OFF");

		switch (key)
		{
			case "XML_COMPRESSION":
			case "STATISTICS_INCREMENTAL":
				return false;

			case "FILLFACTOR":
				return value is Expression.Literal(SqlLiteralKind.Number, var fill) && Plain(fill) &&
					(fill.IndexOf('.') >= 0 || Whole(fill) is >= 1 and <= 100);

			case "MAXDOP":
				return on || off ||
					value is Expression.Literal(SqlLiteralKind.Number, var degree) && Plain(degree) &&
					degree.IndexOf('.') < 0 && Whole(degree) is >= 0 and <= 32767;

			case "DATA_COMPRESSION":
				return value is Expression.ColumnReference(var word) &&
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
		arguments.TrueForAll(static one => one is not Expression.ColumnReference(var text) || Written(text));
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
	public static bool OneOf(Expression? value, params string[] names)
	{
		if (value is not Expression.Literal(_, var text))
			return false;

		var start = text.IndexOf('\'');
		var end   = text.LastIndexOf('\'');
		var said  = start >= 0 && end > start ? text.Substring(start + 1, end - start - 1) : text;

		return Array.Exists(names, one => string.Equals(one, said, StringComparison.OrdinalIgnoreCase));
	}

	/// <summary>Whether a number is written without an exponent, as a bulk insert's counts are.</summary>
	/// <remarks>
	/// <c>BATCHSIZE = 1e3</c> is <c>Msg 102</c> where <c>100.5</c> is read. The literal holds
	/// its own exponent, so this is a question about what was read rather than about what
	/// follows it.
	/// </remarks>
	public static bool Plain(string? text)
	{
		return text is not null && text.IndexOf('e') < 0 && text.IndexOf('E') < 0;
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

	/// <summary>Whether a type named through its schema takes the tail it was given.</summary>
	/// <remarks>
	/// <para>
	/// A dotted name is read wherever a type stands — <c>sys.int</c>, <c>sys..int</c>,
	/// <c>master.sys.int</c>, <c>[sys].[int]</c>, <c>dbo.int</c>, <c>sys.nosuchtype</c> — and
	/// what may follow it is the last part's own business. Asked of the engine one tail at a
	/// time, at every level from 100 to 170.
	/// </para>
	/// <para>
	/// <c>VARYING</c> follows <c>char</c>, <c>character</c>, <c>nchar</c> and <c>binary</c>,
	/// with a length or without: <c>sys.char varying (MAX)</c>, <c>[sys].[char] varying
	/// (10)</c>, <c>sys."Char" varying</c> are read. It follows nothing else —
	/// <c>sys.text varying</c>, <c>sys.varchar varying</c>, <c>sys.varbinary varying</c>,
	/// <c>sys.image varying</c>, <c>sys.int varying</c>, <c>sys.nosuchtype varying</c> and
	/// <c>dbo.mytype varying (10)</c> are each <c>Msg 102</c>.
	/// </para>
	/// <para>
	/// <c>NATIONAL</c> stands in front of <c>char</c>, <c>character</c> and <c>text</c>:
	/// <c>national sys.char varying (10)</c>, <c>national sys.character</c>, <c>national
	/// sys.text (10)</c>, <c>national [sys].[char]</c> are read. Before anything else it is
	/// <c>Msg 102</c> — <c>national sys.nchar</c>, <c>national sys.ntext</c>, <c>national
	/// sys.varchar</c>, <c>national sys.int</c>, <c>national dbo.mytype</c> — and
	/// <c>national sys.text varying</c> is refused as well, <c>TEXT</c> taking the word in
	/// front and not the one behind.
	/// </para>
	/// <para>
	/// Numbers in brackets are nobody's business but the length's: <c>sys.nosuchtype (10)</c>,
	/// <c>sys.nosuchtype (10, 2)</c>, <c>dbo.mytype (10, 2)</c> and <c>dbo.mytype (MAX)</c>
	/// are all read. What is in brackets and is not a number belongs to <c>xml</c> alone —
	/// <c>sys.xml (CONTENT dbo.xsd1)</c>, <c>(DOCUMENT dbo.xsd1)</c> and <c>(dbo.xsd1)</c>
	/// are read, and <c>dbo.mytype (CONTENT dbo.xsd1)</c> is <c>Msg 102</c>.
	/// </para>
	/// </remarks>
	public static bool Dotted(string? text)
	{
		if (text is null)
			return false;

		var rest     = text.TrimStart();
		var national = rest.StartsWith("NATIONAL", StringComparison.OrdinalIgnoreCase);

		if (national)
			rest = rest.Substring("NATIONAL".Length).TrimStart();

		// The name ends at the first space or bracket standing outside a quoted part, so that
		// `[sys]."Char" varying` is the name `[sys]."Char"` and the tail `varying`.
		var at    = 0;
		var until = '\0';

		while (at < rest.Length)
		{
			var one = rest[at];

			if (until != '\0')
			{
				if (one == until)
					until = '\0';
			}
			else if (one == '[')
			{
				until = ']';
			}
			else if (one == '"')
			{
				until = '"';
			}
			else if (char.IsWhiteSpace(one) || one == '(')
			{
				break;
			}

			at++;
		}

		var name = rest.Substring(0, at);
		var tail = rest.Substring(at).TrimStart();
		var dot  = name.LastIndexOf('.');
		var last = Unquoted(dot < 0 ? name : name.Substring(dot + 1));

		if (national && !Array.Exists(Nationally, one => string.Equals(one, last, StringComparison.OrdinalIgnoreCase)))
			return false;

		if (tail.Length == 0)
			return true;

		if (tail.StartsWith("VARYING", StringComparison.OrdinalIgnoreCase))
			return national
				? !string.Equals("TEXT", last, StringComparison.OrdinalIgnoreCase)
				: Array.Exists(Varyingly, one => string.Equals(one, last, StringComparison.OrdinalIgnoreCase));

		// A fixed-length string or binary is never `MAX`, however its name is written: `[char]
		// (max)`, `dbo.char (max)` and `"char" (max)` are `Msg 156` as `char (max)` is.
		if (Array.Exists(Varyingly, one => string.Equals(one, last, StringComparison.OrdinalIgnoreCase)) &&
			Array.Exists(tail.Trim('(', ')').Split(','), static piece => string.Equals(piece.Trim(), "MAX", StringComparison.OrdinalIgnoreCase)))
			return false;

		// A bracket holding anything but numbers is the xml content's.
		return Array.TrueForAll(tail.Trim('(', ')').Split(','), static piece => Numbered(piece)) ||
			   string.Equals("XML", last, StringComparison.OrdinalIgnoreCase);

		static bool Numbered(string piece)
		{
			var word = piece.Trim();

			return string.Equals(word, "MAX", StringComparison.OrdinalIgnoreCase) ||
				   word.Length > 0 && Array.TrueForAll(word.ToCharArray(), char.IsDigit);
		}
	}

	/// <summary>The types a dotted name may put <c>VARYING</c> after.</summary>
	static readonly string[] Varyingly = ["CHAR", "CHARACTER", "NCHAR", "NCHARACTER", "BINARY"];

	/// <summary>The types a dotted name may put <c>NATIONAL</c> in front of.</summary>
	static readonly string[] Nationally = ["CHAR", "CHARACTER", "TEXT"];

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

	/// <summary>Whether a kept sample percentage stands beside the scan it was kept from.</summary>
	/// <remarks>
	/// <c>PERSIST_SAMPLE_PERCENT</c> is <c>Msg 153</c> alone and read beside <c>FULLSCAN</c>,
	/// <c>SAMPLE … PERCENT</c>, <c>SAMPLE … ROWS</c> or <c>RESAMPLE</c>; <c>NORECOMPUTE</c>,
	/// <c>ALL</c> and <c>MAXDOP</c> will not do. Asked of the list as written, and the long name
	/// is struck out of it first: <c>SAMPLE</c> is a part of the name being asked about.
	/// </remarks>
	public static bool Sampled(string? text)
	{
		const string Kept = "PERSIST_SAMPLE_PERCENT";

		if (text is null || text.IndexOf(Kept, StringComparison.OrdinalIgnoreCase) < 0)
			return true;

		var rest = text;

		for (var at = rest.IndexOf(Kept, StringComparison.OrdinalIgnoreCase); at >= 0;
				 at = rest.IndexOf(Kept, StringComparison.OrdinalIgnoreCase))
			rest = rest.Remove(at, Kept.Length);

		return rest.IndexOf("FULLSCAN", StringComparison.OrdinalIgnoreCase) >= 0 ||
			   rest.IndexOf("SAMPLE", StringComparison.OrdinalIgnoreCase) >= 0;
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

	/// <summary>Whether no more than one of a list's items, as written, begins with a word.</summary>
	public static bool Once(string word, string? first, string[]? rest)
	{
		var said = Begins(first) ? 1 : 0;

		foreach (var one in rest ?? [])
			if (Begins(one))
				said++;

		return said <= 1;

		bool Begins(string? one)
		{
			return one is not null && one.TrimStart().StartsWith(word, StringComparison.OrdinalIgnoreCase);
		}
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

		return options is not [Clause.Option { Value: Expression.ColumnReference("ON") }] ||
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
	public static bool Schemas(Expression.RoutineInvocation? call, Clause[]? schema)
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
				case Expression.Prefixed(var word, _) when string.Equals(word, "BULK", StringComparison.OrdinalIgnoreCase):
					bulk = true;
					break;

				case Expression.NamedArgument(var named, _) when string.Equals(named, "FORMAT", StringComparison.OrdinalIgnoreCase):
					format = true;
					break;

				case Expression.ColumnReference(var lob) when Array.Exists(Singles, one => string.Equals(one, lob, StringComparison.OrdinalIgnoreCase)):
					single = true;
					break;
			}

		return bulk && (format || !single) &&
			Array.TrueForAll(schema, static one => one is Clause.JsonColumn { Type: not null } and not Clause.JsonColumn { Path: not null, Ordinal: not null });
	}

	static readonly string[] Singles = ["SINGLE_CLOB", "SINGLE_BLOB", "SINGLE_NCLOB"];

	/// <summary>
	/// Whether a column's number in a file has the shape of one: a whole number an <c>int</c>
	/// holds, signed or not. Zero and a negative are read by the engine and objected to after
	/// the parse (<c>Msg 16204</c>); a number past an <c>int</c> is <c>Msg 102</c>.
	/// </summary>
	public static bool Ranked(string? ordinal)
	{
		return int.TryParse(ordinal, System.Globalization.NumberStyles.AllowLeadingSign, System.Globalization.CultureInfo.InvariantCulture, out _);
	}

	/// <summary>Nodes told apart by identity, which a record's own equality does not do.</summary>
	sealed class ByReference : IEqualityComparer<ISqlSpan>
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

	/// <summary>A variable and the members named after it, joined as one name.</summary>
	public static string Member(string variable, string[]? members)
	{
		return members is null or { Length: 0 } ? variable : variable + "." + string.Join(".", members);
	}

	/// <summary>Whether <c>PREDICT</c>'s arguments are ones it takes.</summary>
	/// <remarks>
	/// A model and data, both by name and both required (<c>Msg 39036</c>), and nothing else:
	/// <c>RUNTIME = ONNX</c> is Azure SQL Edge's and Synapse's, and SQL Server refuses it
	/// (<c>Msg 102</c>). The model is any value but a subquery.
	/// </remarks>
	public static bool Predicts(Expression[]? arguments)
	{
		if (arguments is null)
			return false;

		var model = false;
		var data  = false;

		foreach (var one in arguments)
			switch (one)
			{
				case Expression.NamedArgument(var name, var value)
					when string.Equals(name, "MODEL", StringComparison.OrdinalIgnoreCase):
					if (value is Expression.Subquery)
						return false;

					model = true;
					break;

				case Expression.NamedArgument(var name, _)
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

	/// <summary>Whether a variable is one of the server's, <c>@@ROWCOUNT</c> and its kind.</summary>
	/// <remarks>
	/// They read as values and refuse to be anything else: a cursor, a row's destination or a
	/// row's number (<c>Msg 102</c>), every one of the published names put to the engine. A name
	/// the server has not heard of, <c>@@x</c>, is a variable like any other.
	/// </remarks>
	public static bool IsServerVariable(string? variable)
	{
		return variable is not null && ServerVariables.Contains(variable);
	}

	static readonly HashSet<string> ServerVariables = new(StringComparer.OrdinalIgnoreCase)
	{

		"@@CONNECTIONS", "@@CPU_BUSY", "@@CURSOR_ROWS", "@@DATEFIRST", "@@DBTS", "@@DEFAULT_LANGID",
		"@@DEF_SORTORDER_ID", "@@ERROR", "@@FETCH_STATUS", "@@IDENTITY", "@@IDLE", "@@IO_BUSY",
		"@@LANGID", "@@LANGUAGE", "@@LOCK_TIMEOUT", "@@MAX_CONNECTIONS", "@@MAX_PRECISION",
		"@@MICROSOFTVERSION", "@@NESTLEVEL", "@@OPTIONS", "@@PACKET_ERRORS", "@@PACK_RECEIVED",
		"@@PACK_SENT", "@@PROCID", "@@REMSERVER", "@@ROWCOUNT", "@@SERVERNAME", "@@SERVICENAME",
		"@@SPID", "@@TEXTSIZE", "@@TIMETICKS", "@@TOTAL_ERRORS", "@@TOTAL_READ", "@@TOTAL_WRITE",
		"@@TRANCOUNT", "@@VERSION",
	};

	/// <summary>A run of words as one upper-case word per space.</summary>
	public static string Squared(string words)
	{
		return Run(words, true);
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
						Squared(words.Substring(0, at)), new Expression.ColumnReference(words.Substring(at + 1).Trim()), Clause.None);
			}

		var open = words.IndexOf('(');

		return new Clause.Option(Squared(open < 0 ? words : words.Substring(0, open)), null, Clause.None);
	}

	/// <summary>A word the grammar read and this file has no record for.</summary>
	/// <remarks>
	/// The grammar and the switches in this file are two spellings of one catalogue and have to
	/// agree; where they have drifted apart this says so rather than quietly building the wrong
	/// node. A defect in this file, not in anybody's SQL.
	/// </remarks>
	public static ArgumentOutOfRangeException Unknown(string word)
	{
		return new(nameof(word), word, "The grammar reads this and the tree has no record for it.");
	}
}
