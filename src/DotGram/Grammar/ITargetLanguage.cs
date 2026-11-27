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
	/// <summary>
	/// What one item of the input a generated parser reads is, and so what a character class and a
	/// literal are ranges and sequences of (<see cref="Model.CharRange"/>).
	/// </summary>
	TextUnit TextUnit { get; }

	/// <summary>
	/// The language that reads the standard library's own host fragments, whatever reads the
	/// author's: the library comes along with every grammar, and is written to be read by this.
	/// </summary>
	ITargetLanguage ForLibrary { get; }

	/// <summary>How this language is named in what a grammar's author is told.</summary>
	TargetWording Wording { get; }

	/// <summary>What this language and its environment can do, where a decision forks on it.</summary>
	TargetCapabilities Can { get; }

	/// <summary>Which known type a spelling names, and how; <see cref="KnownType.None"/> for any other.</summary>
	/// <remarks>
	/// Exact: a spelling is classified as written, with no import or alias applied. A sequence is
	/// <see cref="KnownType.Sequence"/> whatever its element is.
	/// </remarks>
	KnownType Classify(string spelling, out TypeSpellingForm form);

	/// <summary>
	/// Which known type a spelling names read loosely, as a stage that cannot see the host reads
	/// it: the prefixes that name the global and the language's own namespace taken off, each where
	/// it is there, and what is left a keyword or a simple name. <see cref="KnownType.None"/> for any other.
	/// </summary>
	/// <remarks>
	/// Looser than <see cref="Classify"/>, and on purpose: it answers "is this certainly one of the
	/// language's own types", where an over-generous yes costs nothing but a question not asked.
	/// </remarks>
	KnownType ClassifyLoosely(string spelling);

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

	// ── The generated code ────────────────────────────────────────────────────────────

	/// <summary>
	/// The parser, written for a sound grammar's graph; null where the plan refused what the grammar
	/// asks of this target, which is then said in <see cref="EmitContext.Diagnostics"/> and not written.
	/// </summary>
	/// <remarks>What does not fit one file is handed out through <see cref="EmitContext.SourceParts"/>.</remarks>
	string? Emit(Model.RecognitionGraph graph, GramCompilerOptions options, EmitContext context);

	/// <summary>
	/// What the file of the parser is called: <paramref name="part"/> 0 the file itself, and 1 on the
	/// parts <see cref="Emit"/> handed out beside it.
	/// </summary>
	string SourceName(GramCompilerOptions options, int part);

	/// <summary>
	/// What the generated code would declare twice, or declare where the host already has the name:
	/// what only the author can rename, said before anything is written.
	/// </summary>
	IReadOnlyList<GramDiagnostic> NameClashes(Model.RecognitionGraph graph, GramCompilerOptions options);

	/// <summary>
	/// The author's names the generated code is written under — for a test that holds the generated
	/// names apart — over the graph it was written from and the ones a cut grammar was made of.
	/// </summary>
	GramNames NamesOf(Model.RecognitionGraph graph, IReadOnlyList<Model.RecognitionGraph> more, GramCompilerOptions options);

	/// <summary>
	/// Whether the seam between the tokens of a grammar cut in two can be skipped by the code this
	/// language writes for it: some of its trivia read by committing to a first reading.
	/// </summary>
	bool ScansSeam(Model.LexicalSplit split);

	// ── Values a grammar writes, rendered as the generated code writes them ───────────

	/// <summary>A host fragment handed over as it stands, as one operand: <c>@(x + 1)</c>.</summary>
	string RenderFragment(string fragment);

	/// <summary>A host member or type by its name, with the type arguments written after it, rendered already.</summary>
	string RenderReference(string name, IReadOnlyList<string> typeArguments);

	/// <summary>A value of a type built from arguments, rendered already: what <c>=&gt; Pair(a, b)</c> writes.</summary>
	string RenderConstruction(string type, IReadOnlyList<string> arguments);

	/// <summary>A call of a target, rendered already, with arguments, rendered already.</summary>
	string RenderCall(string target, IReadOnlyList<string> arguments);

	/// <summary>
	/// A character literal of the grammar's, its text as the lexer kept it: one character, or none
	/// where the file is broken and has been told so.
	/// </summary>
	string RenderCharacter(string text);

	/// <summary>A text literal of the grammar's.</summary>
	string RenderText(string text);

	// ── Names the generated code gives ────────────────────────────────────────────────

	/// <summary>
	/// The name the generated code gives the type the grammar generates for a rule, made from its
	/// captures (§4.1 case 3): what a construction <c>Pair(a, b)</c> builds and what a capture of the
	/// rule holds.
	/// </summary>
	string GeneratedTypeName(Binding.RuleSymbol rule);

	// ── Located values ────────────────────────────────────────────────────────────────

	/// <summary>Whether the host asked for values to be told where they were written.</summary>
	bool LocationsRequested { get; }

	/// <summary>
	/// Whether it asked for that per call — a reading asks or does not — rather than compiled into
	/// every reading.
	/// </summary>
	bool LocationsPerCall { get; }

	/// <summary>
	/// Whether a value of this type is one the host asked to be told where it was written, the type
	/// asked bare and then under each of <paramref name="imports"/>. What it is told through is the
	/// host's shape, which this language knows and the grammar's stages do not.
	/// </summary>
	bool IsLocated(string type, IReadOnlyList<string> imports);

	// ── Optional values ───────────────────────────────────────────────────────────────

	/// <summary>
	/// The type a spelling makes optional by the language's own notation — <c>int</c> of <c>int?</c> —
	/// or null where the spelling is not written optional.
	/// </summary>
	string? OptionalElementOf(string spelling);

	/// <summary>
	/// Whether a value of a type can be null as it stands, needing neither a wrapper to be made
	/// optional nor a cast to be held as one: text, object, a sequence, or one of
	/// <paramref name="references"/>, the spellings the host said are references
	/// (<see cref="Model.RecognitionGraph.ReferenceTypes"/>). Not knowing is answered no.
	/// </summary>
	bool CanBeNull(string spelling, IReadOnlyCollection<string> references);

	/// <summary>
	/// Whether the host's type of this spelling is a reference, asked the way the generated code binds
	/// the name: by <paramref name="definition"/>, the spelling's generic definition where the grammar
	/// wrote it, the first of bare and then each of <paramref name="imports"/> that names a type
	/// decides; where the grammar did not write it, any spelling that is a reference does.
	/// </summary>
	bool IsReferenceAsWritten(string type, string? definition, IReadOnlyList<string> imports);

	/// <summary>
	/// Whether the host's type of this spelling is the language's optional representation of a value
	/// type, asked as <see cref="IsReferenceAsWritten"/> asks.
	/// </summary>
	bool IsOptionalAsWritten(string type, string? definition, IReadOnlyList<string> imports);

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
	/// What is left of a fragment once the brackets around it are taken off as
	/// <paramref name="rule"/> says, or null where the rule does not take it for one name: what a
	/// construction that hands a capture back as it stands is written as.
	/// </summary>
	/// <remarks>
	/// One question asked by several stages, each by a rule of its own, and the rules differ in
	/// what they answer about a fragment such as <c>((x))</c>; which stage asks by which is part
	/// of what the generated code is.
	/// </remarks>
	string? SoleName(string fragment, SoleNameRule rule);

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

	// ── The host: what only the code the parser is generated into can answer ──────────
	//
	// Types are named as the grammar wrote them, bare or under one of its imports. A language
	// with no host answers every question permissively, and that is a legal answer: it is what
	// a grammar compiled on its own, with nothing around it, gets.

	/// <summary>
	/// Whether a host was given at all. Where none was, the questions only a host can settle —
	/// which rules are located, which types are classes, which are optional — are not asked.
	/// </summary>
	bool HasHost { get; }

	/// <summary>
	/// Whether the host's answers are stand-ins, given without seeing its code: a no from one is
	/// not a refusal, and what it would settle is left for the build that sees the host.
	/// </summary>
	bool HostUnseen { get; }

	/// <summary>Whether a type of this name is in scope in the host.</summary>
	bool TypeExists(string name);

	/// <summary>Whether a value of type <paramref name="from"/> may be put where <paramref name="to"/> is expected.</summary>
	bool Fits(string from, string to);

	/// <summary>
	/// The ways a type can be built from named values: each a list of the values it takes, in order.
	/// </summary>
	/// <returns><c>false</c> where the type is not in scope, or offers no way to build it.</returns>
	bool TryWaysToBuild(string type, out IReadOnlyList<IReadOnlyList<MethodParameter>> ways);

	/// <summary>
	/// Whether a type the host gave a parameter or a member takes the characters of the input as a view of them, which a capture of
	/// text may be handed as it is where captures are views, rather than as a string.
	/// </summary>
	bool TakesCharacterView(string type);

	/// <summary>The members of a type that can be set as it is made.</summary>
	/// <returns><c>false</c> where the type is not in scope, or nothing about it can be set.</returns>
	bool TrySettableMembers(string type, out IReadOnlyList<ObjectMember> members);

	/// <summary>
	/// Whether an external recognizer of this name hands back a value of its own, and its type
	/// where it does; <paramref name="against"/> is the type it is held to, where there is one.
	/// </summary>
	ExternalValueResolution ExternalValue(string method, string? against, out string? valueType);

	/// <summary>The shape of an external recognizer or predicate the grammar calls in the given role.</summary>
	ExternalMethodResolution ExternalMethod(string method, ExternalMethodRole role);

	/// <summary>Whether a value of the context type can be put back as it was.</summary>
	bool Rewinds(string type);

	/// <summary>Whether a type is one whose values are references, rather than values copied whole.</summary>
	bool IsReferenceType(string type);

	/// <summary>Whether a type is the language's optional representation of a value type.</summary>
	bool IsOptional(string type);
}

/// <summary>What one item of a generated parser's input is (<see cref="ITargetLanguage.TextUnit"/>).</summary>
enum TextUnit
{
	/// <summary>A UTF-16 code unit: a supplementary character is two of them, each read on its own.</summary>
	Utf16CodeUnit,
}

/// <summary>How <see cref="ITargetLanguage.SoleName"/> takes a fragment's brackets off, and what it takes for a name.</summary>
enum SoleNameRule
{
	/// <summary>One pair of brackets at most, matched or not, and whatever is left: <c>((x))</c> is <c>(x)</c>.</summary>
	OneBracket,

	/// <summary>Every outer pair, matched or not, and whatever is left.</summary>
	EveryBracket,

	/// <summary>Every outer pair, matched or not, and what is left only where it is an identifier.</summary>
	EveryBracketIdentifier,

	/// <summary>
	/// Every outer pair that wraps the whole — <c>(a) + (b)</c> keeps its brackets — and what is
	/// left only where it is letters, digits and underscores.
	/// </summary>
	WrappingBrackets,
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
