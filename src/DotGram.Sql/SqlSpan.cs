using System;

namespace DotGram.Sql;

// Where a node was written: shared by every tree a SQL parser builds, the one in SqlSyntax.cs and
// the SQL:2023 tree in Standard/Sql2023Ast.cs, whose ISqlNode extends ISqlSpan.

/// <summary>
/// Where a node was written, in characters of the input the parser was given.
/// </summary>
/// <remarks>
/// The node's own text and not the trivia around it, so that a comment falls between two
/// spans rather than inside one — which is what lets a second pass hand every comment to
/// the innermost node it stands in.
/// </remarks>
public readonly record struct SqlSpan(int At, int Length)
{
	/// <summary>One past the last character, for a reader that wants the other end.</summary>
	public int End => At + Length;

	/// <summary>Whether anything was recorded, which nothing is unless a parser asked for it.</summary>
	public bool Known => Length > 0 || At > 0;
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
/// <b>The last offer wins, so a span is the outermost rule that handed the value back.</b>
/// Where a rule hands back a value another rule made — <c>WhereClause = "WHERE"i &amp;
/// c: SearchCondition =&gt; @(c)</c> gives back the condition — the range covers the
/// keyword the value itself is not. That is a little wide and it is the safe direction:
/// keeping the first offer instead is a little wide nowhere and badly wrong somewhere,
/// because a value completed from a tail — <c>Syntax.Predicated</c> copies one with
/// <c>with</c> — would inherit the tail's range and claim <c>= 2</c> for <c>b = 2</c>.
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
