using System;

namespace DotGram.Sql;

// Where a node was written: shared by every tree a SQL parser builds, the one in SqlSyntax.cs and
// the SQL:2023 tree in Standard/Sql2023Ast.cs, whose ISqlNode extends ISqlSpan.

/// <summary>
/// Where a node was written, in characters of the input the parser was given.
/// </summary>
/// <remarks>
/// <para>
/// The node's own text and not the trivia around it, so that a comment falls between two
/// spans rather than inside one — which is what lets a second pass hand every comment to
/// the innermost node it stands in.
/// </para>
/// <para>
/// A located T-SQL reading also says where the text in front of the node begins
/// (<see cref="GapStart"/>): the end of the token before the node's first, or where the reading
/// began. What lies between it and <see cref="At"/> is trivia — spacing and comments — and
/// nothing else, so a comment in front of a node is found without reading anything but that
/// gap. Which part of it belongs to the token before (the rest of its line) and which to the
/// node is a question for whoever reads the comments, not for the reading.
/// </para>
/// </remarks>
public readonly record struct SqlSpan
{
	/// <summary>
	/// The length, or its complement where the span is stale: a node copied with <c>with</c>
	/// keeps where its original was written, and says that the copy may no longer be that text.
	/// </summary>
	readonly int _length;

	/// <summary>A span of <paramref name="length"/> characters at <paramref name="at"/>, with nothing in front of it.</summary>
	public SqlSpan(int at, int length)
	{
		At       = at;
		_length  = length;
		GapStart = at;
	}

	/// <summary>
	/// A span of <paramref name="length"/> characters at <paramref name="at"/>, the text in front of
	/// which begins at <paramref name="gapStart"/>.
	/// </summary>
	public SqlSpan(int at, int length, int gapStart)
	{
		At       = at;
		_length  = length;
		GapStart = gapStart;
	}

	SqlSpan(int at, int length, int gapStart, bool stale)
	{
		At       = at;
		_length  = stale ? ~length : length;
		GapStart = gapStart;
	}

	/// <summary>The first character.</summary>
	public int At { get; }

	/// <summary>How many characters.</summary>
	public int Length => _length < 0 ? ~_length : _length;

	/// <summary>
	/// Where the text in front of the node begins: the end of the token before it, or where the
	/// reading began. <see cref="At"/> where nothing was said, which is an empty gap.
	/// </summary>
	public int GapStart { get; }

	/// <summary>
	/// Whether the node was copied with <c>with</c> since it was read. The span is still where its
	/// original was written, and the gap still what stood in front of it; the copy's text may differ.
	/// </summary>
	public bool IsStale => _length < 0;

	/// <summary>One past the last character, for a reader that wants the other end.</summary>
	public int End => At + Length;

	/// <summary>Whether anything was recorded, which nothing is unless a parser asked for it.</summary>
	public bool Known => Length > 0 || At > 0;

	/// <summary>The same span, said to be stale.</summary>
	internal SqlSpan Stale()
	{
		return new SqlSpan(At, Length, GapStart, stale: true);
	}

	/// <summary>Where and how much, as the span was once written as a pair.</summary>
	public void Deconstruct(out int at, out int length)
	{
		at     = At;
		length = Length;
	}
}

/// <summary>
/// What a node of the T-SQL tree holds its span in: the span, kept out of the record's equality.
/// </summary>
/// <remarks>
/// <para>
/// Two trees of the same text read the same whether or not anybody asked where they were written,
/// and whatever spacing and comments stood between their parts: equality is about what was said.
/// A record compares every field it has, so the field holding the span is of a type all of whose
/// values are equal. Two equal subtrees written in different places therefore compare equal too —
/// a table keyed by node has to be keyed by reference.
/// </para>
/// <para>
/// A copy made with <c>with</c> passes through the record's copy constructor, which keeps where
/// the original was written and marks it stale (<see cref="SqlSpan.IsStale"/>).
/// </para>
/// </remarks>
internal readonly struct SqlLocation : IEquatable<SqlLocation>
{
	public SqlLocation(SqlSpan span)
	{
		Span = span;
	}

	public SqlSpan Span { get; }

	/// <summary>What a copy holds: the same place, stale.</summary>
	public SqlLocation Copied()
	{
		return Span.Known || Span.GapStart != 0 ? new SqlLocation(Span.Stale()) : this;
	}

	public bool Equals(SqlLocation other)
	{
		return true;
	}

	public override bool Equals(object? obj)
	{
		return obj is SqlLocation;
	}

	public override int GetHashCode()
	{
		return 0;
	}
}

/// <summary>
/// What a T-SQL reading that was asked for locations offers a node: its span and where the text
/// in front of it begins.
/// </summary>
/// <remarks>
/// The interface the T-SQL grammar names for its located reading, internal because the reader is
/// the only caller; <see cref="ISqlSpan"/> is what everybody else sees.
/// </remarks>
internal interface ISqlLocatable : ISqlSpan
{
	/// <summary>Offer a range and the start of the gap in front of it; the last offer is kept.</summary>
	void Locate(int at, int length, int gapStart);
}

/// <summary>
/// A node that can say where it was written.
/// </summary>
/// <remarks>
/// <para>
/// <b>A method and not a setter, because the policy is not the parser's.</b> The reader offers
/// every rule's range to the value that came out of it, innermost first, and what to do with
/// the offer is decided here.
/// </para>
/// <para>
/// <b>The last offer wins, so a span is the outermost rule that built the value.</b> A
/// value completed from a tail — <c>Syntax.Predicated</c> copies one with <c>with</c> — is
/// offered again by the rule that completed it, and claims <c>b = 2</c> rather than the
/// tail's <c>= 2</c>. A rule that only hands back a value another rule made —
/// <c>WhereClause = "WHERE"i &amp; c: SearchCondition =&gt; @(c)</c> — offers nothing in
/// T-SQL's located reading, so the condition does not claim the <c>WHERE</c> in front of it:
/// every clause's keyword stays outside the value it introduces.
/// </para>
/// <para>
/// It mutates, once, on a node the reader has just made and nothing has yet seen. An
/// <c>init</c> would mean <c>with</c>, and <c>with</c> would copy every node of every tree to
/// record a number that was already known.
/// </para>
/// <para>
/// A grammar asks for this by naming the interface —
/// <c>[Gram("X.gram", LocationType = typeof(ISqlSpan))]</c> — and a grammar that does not ask
/// pays nothing: <c>Rfc3986</c> and <c>ExpressionParser</c> are recognizers and want no
/// positions, which is what <c>docs/ast.md</c> says and stays true where it was right.
/// </para>
/// </remarks>
public interface ISqlSpan
{
	/// <summary>Where this was written, once a reader has said so.</summary>
	SqlSpan Span { get; }

	/// <summary>
	/// Offer a range: the reader calls this for every rule the value came out of, innermost
	/// first, and the last of them is the one kept.
	/// </summary>
	void Locate(int at, int length);
}
