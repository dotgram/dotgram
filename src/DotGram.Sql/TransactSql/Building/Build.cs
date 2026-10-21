using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.TransactSql;

/// <summary>
/// The builders the T-SQL rules hand what they read to, in the action that completes it: each makes the common tree
/// (<c>SqlAst*.cs</c>, <c>DotGram.Sql.Ast</c>) of what its rule gathered.
/// </summary>
/// <remarks>
/// <para>
/// <b>What it does not do.</b> It declares nothing in the common tree and decides no shape: a part whose target is
/// missing, unwritten or undecided throws a <see cref="BuildGap"/> naming it, which the statement it is in holds, and
/// a public entry hands back as an extension. A gap is the answer for that construct until a later task gives it a place.
/// </para>
/// <para>
/// <b>One file a family.</b> Each family — Values, Queries, Dml, Tables, Routines, Procedural, Security, Database,
/// Server, Drop — is a nested class with the builders of its rules and a <c>TryConvert</c> that answers for the records
/// of <c>Build.Read.cs</c> it takes apart and says no to the rest; <see cref="Convert{T}"/> asks them in turn.
/// </para>
/// <para>
/// <b>Words the records leave out.</b> Where a record does not keep a word the common tree records — <c>AS</c> before an
/// alias, <c>ROW</c> against <c>ROWS</c> — the builder reads it from the text the record was read from, through its span:
/// every reading of a public entry is a located one (<see cref="Over"/>), whether it publishes the locations or not.
/// </para>
/// </remarks>
static partial class Build
{
	/// <summary>
	/// The text the reading under way was given, which a builder reads the words a record leaves out from, or null where
	/// it is not known.
	/// </summary>
	static string? source
	{
		get
		{
			return SqlReading.Current?.Source;
		}
		set
		{
			SqlReading.Writable.Source = value;
		}
	}

	/// <summary>
	/// Whether the reading under way publishes where each node was written: <c>TransactSqlParser.Located</c>'s. A node of
	/// the common tree keeps the span it is told for everybody only then (<see cref="SqlLocation.Publishing"/>).
	/// </summary>
	static bool locating => SqlLocation.Publishing;

	delegate bool Converter(object read, [NotNullWhen(true)] out Ast.ISqlNode? made);

	/// <summary>
	/// The families, in the order they are asked. Each answers only for what it owns.
	/// </summary>
	static readonly Converter[] Families =
	[
		Built.TryConvert,
		Queries.TryConvert,
		Dml.TryConvert,
		Tables.TryConvert,
		Routines.TryConvert,
		Procedural.TryConvert,
		Database.TryConvert,
		Monitoring.TryConvert,
	];

	/// <summary>
	/// A value a rule read from the text where the record that holds it leaves it out, as the builder reads it where the
	/// text was there to read; where it was not, the default the caller passes stands.
	/// </summary>
	static T Recover<T>(string? text, Func<string, T> read, T otherwise)
	{
		return text is null ? otherwise : read(text);
	}

	/// <summary>
	/// A word a rule saw written, where a record of an earlier reading read it from the text: what it says, except in a
	/// reading of a part again from a text of its own (<see cref="Reread"/>), where a record has no span to read it by
	/// and the default stands, as it always did.
	/// </summary>
	static bool Recovered(bool read, bool otherwise)
	{
		return unlocated ? otherwise : read;
	}

	/// <summary>
	/// A reading of nodes read again from another text than the statement's, with that text as the source their spans
	/// are in.
	/// </summary>
	static T Inside<T>(string text, Func<T> convert)
	{
		var outer = source;

		source = text;

		try
		{
			return convert();
		}
		finally
		{
			source = outer;
		}
	}

	internal static Ast.Statement StatementOf(Statement read)
	{
		return Convert<Ast.Statement>(read);
	}

	internal static Ast.Query QueryOf(Query read)
	{
		return Convert<Ast.Query>(read);
	}

	internal static Ast.TableSource Source(TableReference read)
	{
		return Convert<Ast.TableSource>(read);
	}

	/// <summary>
	/// The one dispatcher: the first family that owns the record converts it, and the node is told where the record was
	/// written where the reading publishes it.
	/// </summary>
	/// <remarks>
	/// Every record that can hold another of its kind passes here — a source a pivot turns, the joins hung on a source —
	/// so this is where a deep one's conversion checks its stack: where it runs low, the rest goes on with a stack of its
	/// own (<see cref="SqlStack"/>), as the reader that read it did, rather than overflowing it.
	/// </remarks>
	static T Convert<T>(object read)
		where T : Ast.ISqlNode
	{
		if (!SqlStack.Enough())
			return Deeper(read, static node => Convert<T>(node));

		foreach (var family in Families)
			if (family(read, out var made))
			{
				if (made is not T typed)
					throw BuildGap.Open(read, $"converts to {made.GetType().Name}, where a {typeof(T).Name} stands");

				if (locating && read is ISqlSpan { Span: { Known: true, IsStale: false } span })
					Place(typed, span);

				return typed;
			}

		throw BuildGap.Missing(read, "no family converts it");
	}

	/// <summary>
	/// A conversion on a stack of its own, with this thread's reading — its text, whether it is skimmed or a plain
	/// reading a builder made, and whether it publishes locations — taken across. Called where the stack runs low, by
	/// <see cref="Convert{T}"/> and by <see cref="Values.Of"/>.
	/// </summary>
	internal static T Deeper<T>(object read, Func<object, T> convert)
	{
		var made = new object?[1];

		// The walk's thread takes the context and not the reading, which is carried by hand, as the parser's hand-off
		// carries it (TransactSqlParser.OnHandingOff).
		SqlStack.Deeper(read, (reading: SqlReading.Current, convert), made, static (node, state, result) =>
		{
			var outer = SqlReading.Swap(state.reading);

			try
			{
				result[0] = state.convert(node);
			}
			finally
			{
				SqlReading.Swap(outer);
			}
		});

		return (T)made[0]!;
	}

	/// <summary>
	/// A record's name as the inventory writes it: <c>Statement.Select</c>, <c>SetExpression.Locking.LockTimeout</c>; a
	/// name the caller has, itself.
	/// </summary>
	public static string NameOf(object read)
	{
		return read switch
		{
			string name               => name,
			Given given               => given.Record,
			Statement.Built made      => made.Record,
			Query.Built query         => query.Record,
			TableReference.Built from => from.Record,
			Values.Held held          => held.Record,
			_                         => NameOf(read.GetType()),
		};
	}

	public static string NameOf(Type type)
	{
		var name = type.Name;

		for (var outer = type.DeclaringType; outer is not null && outer != typeof(Build); outer = outer.DeclaringType)
			name = outer.Name + "." + name;

		return name;
	}

	/// <summary>
	/// The gap for a record a family owns and has no arm for.
	/// </summary>
	static BuildGap Unconverted(object read)
	{
		return BuildGap.Unconverted(read, "the reading has no conversion to the shared tree");
	}

	/// <summary>
	/// Whether a name is one of the records a family lists.
	/// </summary>
	static bool Owned(string[] owns, object read)
	{
		return Array.IndexOf(owns, NameOf(read)) >= 0;
	}

	// ── The words a record leaves out ───────────────────────────────────────────

	/// <summary>Whether text can be cut at a span: there is a text, and the span says where in it.</summary>
	[MemberNotNullWhen(true, nameof(source))]
	static bool Cuttable(SqlSpan span)
	{
		return source is not null && span.Known && !span.IsStale;
	}

	/// <summary>
	/// Where a node was read: a value of the common tree keeps the span its reading told it apart where the reading does
	/// not publish locations (<see cref="SqlLocation.Read"/>).
	/// </summary>
	static SqlSpan SpanOf(ISqlSpan node)
	{
		return node is Ast.Expression value ? value.Read : node.Span;
	}

	/// <summary>
	/// The text a node was read from, or null where it was not located or no text was handed in.
	/// </summary>
	static string? Spelled(ISqlSpan node)
	{
		var span = SpanOf(node);

		if (!Cuttable(span) || span.End > source.Length)
			return null;

		return source.Substring(span.At, span.Length);
	}

	/// <summary>
	/// A statement's own text from the token at <paramref name="from"/> of the tail its rule kept to its end, as it was
	/// written, and where that text starts in the text the reading is over. The tail has its blanks made one space, so a
	/// position in it is no position in the input; the statement's text is cut by tokens instead, and null stands where
	/// it cannot be cut or its tokens are not the tail's.
	/// </summary>
	static (string Text, int At)? AsWritten(ISqlSpan old, string tail, int from)
	{
		var span = SpanOf(old);

		if (!Cuttable(span) || span.End > source.Length)
			return null;

		var whole = source.Substring(span.At, span.Length);
		var kept  = SqlTokens.Located(tail);
		var all   = SqlTokens.Located(whole);
		var count = all.Count;

		// A terminator is the statement's, not the tail's.
		if (count > kept.Count && all[count - 1].Token is { Kind: SqlTokens.Kind.Punctuation, Text: ";" })
			count--;

		if (count < kept.Count || from >= kept.Count)
			return null;

		var first = count - kept.Count;

		for (var i = 0; i < kept.Count; i++)
			if (kept[i].Token != all[first + i].Token)
				return null;

		var start = all[first + from].At;
		var end   = count < all.Count ? all[count].At : whole.Length;

		return (whole[start..end], span.At + start);
	}

	/// <summary>
	/// A part read again from a text cut out of the statement, whose nodes were told where they stand in that text: told
	/// where they stand in the text the reading is over, where the reading keeps the spans it tells.
	/// </summary>
	static void Shift<T>(IEnumerable<T> nodes, int by)
		where T : ISqlSpan
	{
		if (by == 0 || !SqlReading.Publishes)
			return;

		var seen = new HashSet<ISqlSpan>(Rules.ByReference.Instance);

		foreach (var root in nodes)
		{
			SqlWalker.Walk(root, node =>
			{
				if (node.Span is { Known: true } span && seen.Add(node))
				{
					if (node is ISqlLocatable located)
						located.Locate(span.At + by, span.Length, span.GapStart + by);
					else
						node.Locate(span.At + by, span.Length);
				}

				return true;
			});
		}
	}

	/// <summary>
	/// The text between the end of one node and the start of the next, or null where either is not
	/// located: what was written between two things a record keeps.
	/// </summary>
	static string? Between(ISqlSpan before, ISqlSpan after)
	{
		var first = SpanOf(before);
		var next  = SpanOf(after);

		if (!Cuttable(first) || !Cuttable(next))
			return null;

		var from = first.End;
		var to   = next.At;

		return from <= to && to <= source.Length ? source.Substring(from, to - from) : null;
	}

	/// <summary>
	/// Whether a text the old tree keeps as written reads back as the text the writer prints, as the oracle compares the
	/// two: exactly, or token by token (BuildText.SameTokens) where the member is free text (BuildText.Free).
	/// </summary>
	static bool Same(object old, string member, string written, string text)
	{
		return written == text || BuildText.Free(member) && BuildText.SameTokens(old, member, text, written);
	}

	/// <summary>
	/// The tokens of a stretch of text, comments apart: a word upper-cased, and a name in brackets or quotes, a
	/// string or a number as written — so that `"as"` is never the word AS.
	/// </summary>
	static List<string> Words(string text)
	{
		var tokens = SqlTokens.Of(text);
		var words  = new List<string>(tokens.Count);

		foreach (var one in tokens)
			words.Add(one.Kind == SqlTokens.Kind.Word ? one.Text.ToUpperInvariant() : one.Text);

		return words;
	}

	/// <summary>
	/// The first token written after a located node, comments and blanks skipped — a word upper-cased — or null
	/// where the text is not there or nothing follows.
	/// </summary>
	static string? WordAfter(ISqlSpan node)
	{
		var span = SpanOf(node);

		if (!Cuttable(span) || span.End > source.Length)
			return null;

		return SqlTokens.First(source, span.End) is { } token
			? token.Kind == SqlTokens.Kind.Word ? token.Text.ToUpperInvariant() : token.Text
			: null;
	}

	/// <summary>
	/// The writer's text of a node a conversion built from what was written, to compare the two: where the writer refuses
	/// the node, with its refusals (a rejected argument, an unsupported form), the text has no form the common tree writes
	/// yet, and that is a gap, as every other form the common tree has no place for is.
	/// </summary>
	static string Written(object old, Ast.ISqlNode node, string member)
	{
		try
		{
			return Ast.TransactSqlWriter.Write(node);
		}
		catch (Exception refused) when (refused is ArgumentException or NotSupportedException)
		{
			throw BuildGap.Unwritten(old, $"{member}, which the writer refuses ({refused.GetType().Name})");
		}
	}

	/// <summary>Each item carried, in order, into a list of its own: no list for none, and no array for one.</summary>
	static SqlList<T> Each<TOld, T>(SqlList<TOld> items, Func<TOld, T> convert)
	{
		switch (items.Count)
		{
			case 0:
				return [];

			case 1:
				return SqlList<T>.Of(convert(items[0]));

			default:
				var made = new T[items.Count];

				for (var at = 0; at < made.Length; at++)
					made[at] = convert(items[at]);

				return SqlList.Own(made);
		}
	}
}
