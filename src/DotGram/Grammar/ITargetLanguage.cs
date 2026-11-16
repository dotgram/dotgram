using System;

namespace DotGram.Grammar;

/// <summary>
/// What the grammar's own stages need to know about the language the parser is generated in.
/// </summary>
/// <remarks>
/// <para>
/// The parsing, binding and normalizing of a grammar is about the grammar; the types it names
/// are spelled in the target language, and so is the default a rule's value takes. Every
/// question those stages ask about a spelling — is this text, is it a sequence and of what,
/// how is a sequence of this written — is asked here, so that no stage compares or builds a
/// spelling of its own.
/// </para>
/// <para>
/// One instance flows from the compiler's options into the binder and the normalizer, and is
/// carried on the graph for the checks made of it afterwards. <see cref="CSharpLanguage"/> is the
/// default and, for now, the only one.
/// </para>
/// </remarks>
interface ITargetLanguage
{
	/// <summary>Which known type a spelling names, and how; <see cref="KnownType.None"/> for any other.</summary>
	/// <remarks>
	/// Exact: a spelling is classified as written, with no import or alias applied. A sequence is
	/// <see cref="KnownType.Sequence"/> whatever its element is.
	/// </remarks>
	KnownType Classify(string spelling, out TypeSpellingForm form);

	/// <summary>
	/// Whether a spelling is one of the language's keywords for a type that a grammar may write
	/// without <c>@</c>, as a type and never a rule's name (§2).
	/// </summary>
	bool IsGrammarKeyword(string spelling);

	/// <summary>Whether a spelling is a sequence of some element.</summary>
	bool IsSequence(string spelling);

	/// <summary>The element of a sequence, or null where the spelling is not one.</summary>
	string? ElementOf(string spelling);

	/// <summary>The spelling of a sequence of <paramref name="element"/>.</summary>
	string SequenceOf(string element);

	/// <summary>How the generated code spells a known type.</summary>
	/// <exception cref="ArgumentOutOfRangeException">
	/// <see cref="KnownType.None"/> or <see cref="KnownType.Sequence"/>, which have no spelling of their own.
	/// </exception>
	string Spelling(KnownType kind);
}
