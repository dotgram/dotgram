using System;
using System.Collections.Generic;

namespace DotGram.Grammar.Model;

/// <summary>
/// How a rule's value is arrived at — one case per way the language offers.
/// </summary>
/// <remarks>
/// <para>
/// The language gives five answers to one question, and they are exhaustive: the author
/// wrote an expression (§3.7), the rule collects its operands into a sequence (§4.1 case
/// 2), it hands one operand back (§4.1 case 3, and <c>: item</c> of §4.2), or its captures
/// fill a declared type — by its constructor or by writing into it (§7.3).
/// </para>
/// <para>
/// One type rather than a marker string in the expression's place, which is what this was
/// while the cases were arriving one at a time. The difference is not tidiness: a case
/// carries exactly what it needs, so a constructor's argument order cannot go missing and
/// a sequence cannot acquire one; the emitter's <c>switch</c> is checked for exhaustiveness
/// rather than falling through four string comparisons to a default; and there is no state
/// in which an author's expression happens to spell <c>&lt;sequence&gt;</c>.
/// </para>
/// </remarks>
abstract record Construction
{
	/// <summary>The C# the grammar wrote, and where it wrote it (§7.6).</summary>
	/// <param name="At">-1 for text this compiler wrote rather than read.</param>
	public sealed record Expression(string Text, int At = -1) : Construction
	{
		public override string ToString()
		{
			return Text;
		}
	}

	/// <summary>§4.1 case 2: everything the rule is made of, in order.</summary>
	/// <param name="Taken">
	/// The captures that are collected, in the order the rule has them: the operands, whether the author named them or
	/// not. A name is never part of an implicit value (§4.1), so a capture the author wrote of something that is not an
	/// operand is a member of the rule and is not collected. Null where every member is: a construction this compiler
	/// wrote for a rule of its own.
	/// </param>
	public sealed record Sequence(IReadOnlyList<string>? Taken = null) : Construction
	{
		public static readonly Sequence Instance = new();

		public override string ToString()
		{
			return "<sequence>";
		}
	}

	/// <summary>
	/// §4.1 case 3: the rule's value is the one operand of it that produces one.
	/// </summary>
	/// <param name="Captures">
	/// The captures the operand is made a capture by, which it is handed back from: the one that holds it on
	/// the route that ran. Named here and not found by what their names begin with, since a capture the author
	/// wrote may be named anything. More than one where the operands of different routes are held differently,
	/// the value of a rule that builds one and the text of a rule that matched it.
	/// </param>
	public sealed record Operand(IReadOnlyList<string> Captures) : Construction
	{
		/// <summary>The operand held by one capture.</summary>
		public Operand(string capture)
			: this([capture])
		{
		}

		public override string ToString()
		{
			return "<operand>";
		}
	}

	/// <summary>
	/// §4.1's string without <c>=&gt;</c> where a piece is not its own text: the pieces the route read, joined in order.
	/// </summary>
	/// <param name="Parts">
	/// What is joined, in order: text the grammar fixes (a literal that matches its own case), or a capture, which holds
	/// the text of a piece, the value of a string rule, the character of a char rule, or the values of the turns of a
	/// repetition. Trivia, lookaheads and guards are no part.
	/// </param>
	public sealed record Join(IReadOnlyList<JoinPart> Parts) : Construction
	{
		public override string ToString()
		{
			return "<join>";
		}
	}

	/// <summary>
	/// §7.3's first way: the declared type's constructor, with the captures that fill it
	/// in the constructor's own order.
	/// </summary>
	public sealed record Constructor(IReadOnlyList<string> Arguments) : Construction
	{
		public override string ToString()
		{
			return "<constructor>";
		}
	}

	/// <summary>
	/// §7.3's second way: the value is made and then written into, and this says what
	/// goes where.
	/// </summary>
	public sealed record Initializer(IReadOnlyList<PropertyBinding> Bindings) : Construction
	{
		public override string ToString()
		{
			return "<initializer>";
		}
	}
}

/// <summary>One part of a <see cref="Construction.Join"/>: fixed text, or the capture that holds the part.</summary>
/// <param name="Text">The text, where the grammar fixes it; null for a capture.</param>
/// <param name="Capture">The capture, where the part is one; null for fixed text.</param>
/// <param name="Trivia">
/// Whether the capture holds trivia, which gives its text between two pieces that matched anything and nothing at either
/// edge of the rule.
/// </param>
readonly record struct JoinPart(string? Text, string? Capture, bool Trivia = false)
{
	public static JoinPart Seam(string capture)
	{
		return new JoinPart(null, capture, true);
	}

	public static JoinPart Fixed(string text)
	{
		return new JoinPart(text, null);
	}

	public static JoinPart Held(string capture)
	{
		return new JoinPart(null, capture);
	}
}

/// <summary>One property of a result written from one capture (§7.3).</summary>
readonly record struct PropertyBinding(string Property, string Capture);
