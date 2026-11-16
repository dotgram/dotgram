using System;

namespace DotGram.Grammar;

/// <summary>
/// The types the grammar's own stages recognise, or produce, by how they are spelled.
/// </summary>
/// <remarks>
/// <para>
/// A grammar carries its types as the text it wrote and the text the generated file will
/// declare, and that stays so: a host's type is a name only the host can answer for. What the
/// grammar itself knows is a handful of types every target has — text, the numbers, a span of
/// the input, a sequence of something — and which spelling names one of them is the target
/// language's to say (<see cref="ITargetLanguage.Classify"/>). The stages ask about these, never
/// about the spellings.
/// </para>
/// <para>
/// The order is relied on: <see cref="Text"/> to <see cref="Decimal"/> are the types a grammar
/// may write by keyword alone, and <see cref="Text"/> to <see cref="NUInt"/> every value type a
/// stage knows without asking the host.
/// </para>
/// </remarks>
enum KnownType
{
	/// <summary>None of these: a type of the host's, one the grammar generates, or a parameter.</summary>
	None,

	/// <summary>Text: what a rule that builds nothing yields by default (§4.1 case 4).</summary>
	Text,

	/// <summary>The type every other converts to.</summary>
	Object,

	Boolean,
	Char,
	SByte,
	Byte,
	Int16,
	UInt16,
	Int32,
	UInt32,
	Int64,
	UInt64,
	Single,
	Double,
	Decimal,

	/// <summary>A signed integer the size of a pointer.</summary>
	NInt,

	/// <summary>An unsigned integer the size of a pointer.</summary>
	NUInt,

	/// <summary>No value at all.</summary>
	Void,

	/// <summary>Where in the input a rule matched: the one type the notation names itself (§4.1 case 4).</summary>
	SourceSpan,

	/// <summary>A sequence of another type, whose element <see cref="ITargetLanguage.ElementOf"/> gives.</summary>
	Sequence,
}

/// <summary>How a spelling names a <see cref="KnownType"/>.</summary>
/// <remarks>
/// The stages differ in which forms they trust, and for a reason: a keyword names one type
/// whatever the grammar imports, a name from the global namespace does too, and a bare name may
/// be a type of the host's that happens to be called the same.
/// </remarks>
enum TypeSpellingForm
{
	/// <summary>Not a known type.</summary>
	None,

	/// <summary>The language's own syntax: a keyword, or a sequence's notation.</summary>
	Syntax,

	/// <summary>The type's simple name, <c>String</c>.</summary>
	Simple,

	/// <summary>Its name in its namespace, <c>System.String</c>.</summary>
	Qualified,

	/// <summary>Its name from the global namespace, <c>global::System.String</c>.</summary>
	Global,
}
