using System;
using System.Collections.Generic;

namespace DotGram.Grammar.Fragments;

/// <summary>
/// Where a part of a host fragment stands: an offset into the fragment's own text, the text
/// between <c>@(</c> and its <c>)</c>, and how long it is.
/// </summary>
/// <remarks>
/// The fragment's text, not the grammar's: a grammar position is the fragment's position plus
/// two (<c>@(</c>) plus <see cref="Start"/>, and only the caller knows the first of those.
/// </remarks>
readonly record struct FragmentSpan(int Start, int Length)
{
	public int End => Start + Length;
}

/// <summary>
/// The operators a fragment may use and still be read as written by any target.
/// </summary>
enum FragmentOperator
{
	And,
	Or,
	Equal,
	NotEqual,
	Less,
	LessOrEqual,
	Greater,
	GreaterOrEqual,
	Add,
	Subtract,
}

/// <summary>
/// A host fragment read into the closed set of forms every target can be asked to spell.
/// </summary>
/// <remarks>
/// <para>
/// The author writes C#, and C#'s parser decides what was written; this is what that reading is
/// lowered to. The set is closed on purpose: a literal, a name, a member read, a call, a
/// construction, the operators of <see cref="FragmentOperator"/>, a <c>!</c> before a
/// parenthesised comparison or a call, <c>??</c>, <c>?:</c>, <c>is null</c>, <c>is not null</c>
/// and parentheses. Everything else C# can say is an <see cref="Unportable"/> that names the form
/// and where it stands, and a printer for another language refuses it there. C# itself never
/// reads this tree to print a fragment it can print as written.
/// </para>
/// <para>
/// Literal values are decoded, never kept as their C# spelling: a printer re-escapes them for
/// its own language.
/// </para>
/// </remarks>
abstract record Fragment
{
	public required FragmentSpan Span { get; init; }

	/// <summary>
	/// A regular string literal, its value decoded.
	/// </summary>
	public sealed record Text(string Value) : Fragment;

	/// <summary>
	/// A character literal, its value decoded.
	/// </summary>
	public sealed record Character(char Value) : Fragment;

	/// <summary>
	/// An integer written in decimal or hexadecimal, with no suffix and no digit separator.
	/// </summary>
	public sealed record Integer(ulong Value, bool IsHexadecimal) : Fragment;

	/// <summary>
	/// A number with a fraction and nothing else — digits, a point, digits — kept as written.
	/// </summary>
	public sealed record Real(string Digits) : Fragment;

	public sealed record Boolean(bool Value) : Fragment;

	public sealed record Null : Fragment;

	/// <summary>
	/// A name standing on its own: a capture, a supplied name, or the first part of a host name.
	/// </summary>
	public sealed record Name(string Identifier) : Fragment;

	/// <summary>
	/// <c>Target.Identifier</c>: a member read, or one more part of a dotted name.
	/// </summary>
	public sealed record Member(Fragment Target, string Identifier) : Fragment;

	/// <summary>
	/// A call of a name or of a member, with positional arguments.
	/// </summary>
	public sealed record Call(Fragment Target, IReadOnlyList<Fragment> Arguments) : Fragment;

	/// <summary>
	/// <c>new T(args)</c>, where <see cref="Type"/> is a name or a dotted name.
	/// </summary>
	public sealed record New(Fragment Type, IReadOnlyList<Fragment> Arguments) : Fragment;

	public sealed record Binary(FragmentOperator Operator, Fragment Left, Fragment Right) : Fragment;

	/// <summary>
	/// <c>!</c> — only before a parenthesised comparison or a call.
	/// </summary>
	public sealed record Not(Fragment Operand) : Fragment;

	public sealed record Coalesce(Fragment Left, Fragment Right) : Fragment;

	public sealed record Conditional(Fragment Condition, Fragment WhenTrue, Fragment WhenFalse) : Fragment;

	/// <summary>
	/// <c>x is null</c>, or <c>x is not null</c> where <see cref="IsNegated"/>. <c>x == null</c> is
	/// a <see cref="Binary"/> against a <see cref="Null"/>, as written.
	/// </summary>
	public sealed record IsNull(Fragment Operand, bool IsNegated) : Fragment;

	public sealed record Parenthesized(Fragment Inner) : Fragment;

	/// <summary>
	/// A form outside the closed set, named by <see cref="Form"/>, at <see cref="Fragment.Span"/>.
	/// </summary>
	/// <remarks>
	/// The name is C#'s own name for the syntax (<c>ConditionalAccessExpression</c>,
	/// <c>RecursivePattern</c>, <c>MultiplyExpression</c>), or, where C# has one name for a form
	/// some of whose spellings port and some do not, a narrower one: <c>NumericSuffix</c>,
	/// <c>VerbatimString</c>, <c>RawString</c>, <c>Comment</c>, <c>Unparsable</c> and the like.
	/// </remarks>
	public sealed record Unportable(string Form) : Fragment;
}

/// <summary>
/// A fragment's text, the tree it was lowered to, and every part of it that is not portable.
/// </summary>
/// <param name="Refusals">
/// The <see cref="Fragment.Unportable"/> nodes of the tree in the order they stand in the text,
/// and with them what the tree has no place for: a comment. Empty where the fragment is portable
/// as written.
/// </param>
sealed record LoweredFragment(string Text, Fragment Root, IReadOnlyList<Fragment.Unportable> Refusals)
{
	public bool IsPortable => Refusals.Count == 0;
}
