using System;
using System.Collections.Generic;

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

	// ── Host fragments: the text of the target language a grammar hands over ─────────
	//
	// Inline in `@(...)`, after `=>`, in a `when`, as a mark and as a recovery's factory. The
	// grammar's stages never read one themselves: they ask where one ends, which names it uses,
	// and for a copy with names replaced.

	/// <summary>
	/// Whether this language can read a host fragment at all. Where it cannot, an inline one is
	/// refused, and every question below answers that it cannot say.
	/// </summary>
	bool ReadsFragments { get; }

	/// <summary>
	/// Where the host fragment whose opening bracket is at <paramref name="open"/> closes: the
	/// index of its closing bracket, found past whatever literal or comment could hide one.
	/// </summary>
	/// <returns><c>false</c> where it does not close, or where fragments cannot be read.</returns>
	bool TryFindFragmentEnd(string text, int open, out int close);

	/// <summary>
	/// The names a fragment uses and does not introduce itself, or null where it cannot be read:
	/// a caller then falls back to <see cref="Spells"/> rather than concluding it uses none.
	/// </summary>
	IReadOnlyCollection<string>? NamesUsed(string fragment);

	/// <summary>
	/// The names a fragment uses as something other than a value — called, written as a type, or
	/// declared — or null where it cannot be read.
	/// </summary>
	IReadOnlyCollection<string>? NamesNotUsedAsValues(string fragment);

	/// <summary>
	/// The fragment with each name it uses that is a key of <paramref name="names"/> replaced by
	/// the value, or null where it cannot be read.
	/// </summary>
	string? Renamed(string fragment, IReadOnlyDictionary<string, string> names);

	/// <summary>
	/// Whether a name appears in a fragment's spelling, searched as <paramref name="search"/> says:
	/// the answer for a fragment <see cref="NamesUsed"/> cannot read.
	/// </summary>
	bool Spells(string fragment, string name, NameSearch search);

	/// <summary>
	/// The name a fragment is, where it is one name and nothing else — bracketed as often as it
	/// likes — or null: what a construction that hands a capture back as it stands is written as.
	/// </summary>
	string? SoleName(string fragment);

	/// <summary>Whether <see cref="HostAnswersTo"/> and <see cref="NamesUnboundIn"/> can say anything.</summary>
	bool KnowsHostNames { get; }

	/// <summary>
	/// Whether anything of the host answers to a simple name where the generated code stands,
	/// beside the grammar's imports. False where <see cref="KnowsHostNames"/> is.
	/// </summary>
	bool HostAnswersTo(string name, IReadOnlyList<string> imports);

	/// <summary>
	/// The simple names in a fragment that bind to nothing where the generated code stands, beside
	/// the grammar's imports; null where this cannot say.
	/// </summary>
	IReadOnlyCollection<string>? NamesUnboundIn(string fragment, IReadOnlyList<string> imports);
}

/// <summary>How <see cref="ITargetLanguage.Spells"/> looks for a name in the spelling of a fragment.</summary>
enum NameSearch
{
	/// <summary>Anywhere, as a substring.</summary>
	Substring,

	/// <summary>As a whole word: not inside a longer name.</summary>
	Word,

	/// <summary>As a whole word that is not a member of something else: not after a dot.</summary>
	WordNotMember,
}
