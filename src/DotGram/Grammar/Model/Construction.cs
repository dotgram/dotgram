using System;

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
public abstract record Construction
{
	/// <summary>The C# the grammar wrote, and where it wrote it (§7.6).</summary>
	/// <param name="At">-1 for text this compiler wrote rather than read.</param>
	public sealed record Expression(string Text, int At = -1) : Construction
	{
		public override string ToString() => Text;
	}

	/// <summary>§4.1 case 2: everything the rule is made of, in order.</summary>
	public sealed record Sequence : Construction
	{
		public static readonly Sequence Instance = new();

		public override string ToString() => "<sequence>";
	}

	/// <summary>
	/// §4.1 case 3: the rule's value is the one operand of it that produces one.
	/// </summary>
	public sealed record Operand : Construction
	{
		public static readonly Operand Instance = new();

		public override string ToString() => "<operand>";
	}

	/// <summary>
	/// §7.3's first way: the declared type's constructor, with the captures that fill it
	/// in the constructor's own order.
	/// </summary>
	public sealed record Constructor(IReadOnlyList<string> Arguments) : Construction
	{
		public override string ToString() => "<constructor>";
	}

	/// <summary>
	/// §7.3's second way: the value is made and then written into, and this says what
	/// goes where.
	/// </summary>
	public sealed record Initializer(IReadOnlyList<PropertyBinding> Bindings) : Construction
	{
		public override string ToString() => "<initializer>";
	}
}

/// <summary>One property of a result written from one capture (§7.3).</summary>
public readonly record struct PropertyBinding(string Property, string Capture);
