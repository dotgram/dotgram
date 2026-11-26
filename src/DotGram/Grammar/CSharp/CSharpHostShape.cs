using System;
using System.Collections.Generic;

namespace DotGram.Grammar;

/// <summary>
/// The shape of the C# the parser is generated into: the class and its readings, what the host
/// declares itself, what it is asked through, and the interface or class it names for locations.
/// </summary>
/// <remarks>
/// C#'s, and only C#'s: another target has a host of another shape. <see cref="CSharpLanguage"/> is made
/// of it (<see cref="GramCompilerOptions"/>).
/// </remarks>
sealed class CSharpHostShape
{
	/// <summary>The partial class the generated members go into (§1).</summary>
	public string ClassName { get; set; } = "Grammar";

	/// <summary>Its namespace; null for the global one.</summary>
	public string? Namespace { get; set; }

	/// <summary>
	/// Resolves the C# names a grammar refers to with <c>@</c>. Defaults to a resolver
	/// that accepts everything, which is right for tests of the grammar side and wrong
	/// for real generation.
	/// </summary>
	public ISymbolResolver SymbolResolver { get; set; } = PermissiveSymbolResolver.Instance;

	/// <summary>
	/// Finds where an inline <c>@(...)</c> expression ends. Null means the grammar may
	/// not use one — a diagnostic, not a crash.
	/// </summary>
	public ICSharpScanner? CSharpScanner { get; set; }

	/// <summary>
	/// Where the C# a grammar hands over is, for the <c>#line</c> directives of §7.6.
	/// </summary>
	/// <remarks>
	/// Null emits none, and that is the right answer for a caller with nothing to point
	/// at — a grammar compiled from a string in a test has no file an editor could open,
	/// and a directive naming one that does not exist is worse than none.
	/// </remarks>
	public ILineMap? LineMap { get; set; }

	/// <summary>
	/// An interface whose implementors are told where they were written, by qualified name.
	/// </summary>
	/// <remarks>
	/// What <c>[Gram(…, LocationType = typeof(ISqlSpan))]</c> names. A rule whose value is
	/// assignable to it has its construction handed the span the rule stands on; a rule whose
	/// value is not, and a grammar that names nothing here, are compiled exactly as before.
	/// </remarks>
	public string? LocationType { get; set; }

	/// <summary>
	/// Whether <see cref="LocationType"/> is a class rather than an interface: a located value is then
	/// told through the class, which a value may only derive from, and not through an interface it
	/// may implement explicitly.
	/// </summary>
	public bool LocationTypeIsClass { get; set; }

	/// <summary>
	/// The nested class whose published methods call this parser with locations on, where
	/// <see cref="LocationType"/> is decided per call rather than by a second compilation
	/// (<c>[GramParserVariant("Located", UsePerCallLocations = true)]</c>). Null compiles locations in, as before.
	/// </summary>
	public string? LocatedFacade { get; set; }

	/// <summary>Whether the host declares <see cref="LocatedFacade"/> itself.</summary>
	public bool LocatedFacadeDeclared { get; set; }

	/// <summary>
	/// The classes whose static members the generated code may name, each as a <c>using static</c>
	/// names it: rooted, <c>global::App.G</c>, or <c>X::App.G</c> under an extern alias the
	/// generated file then declares.
	/// </summary>
	/// <remarks>
	/// One per grammar this one is built on. A rule that comes from another grammar goes on
	/// calling the helpers its author wrote, and those live beside that grammar's own host —
	/// so the generated file names them with <c>using static</c> rather than requiring the
	/// including class to derive from something for the sake of what its methods can see.
	/// </remarks>
	public IReadOnlyList<string> StaticImports { get; set; } = [];

	/// <summary>
	/// The nested class this compilation goes into, where the host holds more than one
	/// (<c>[Gram(Name = "...")]</c>). Null puts it in the host class itself.
	/// </summary>
	/// <remarks>
	/// A host with two grammars — the same one compiled two ways, usually — cannot put
	/// both in one scope: a file's support is written once and named for what it is, so a
	/// second copy of <c>Match&lt;T&gt;</c>, of the failure, of the lexer and of the value
	/// tables would collide with the first, name for name. A class of its own is a scope
	/// of its own. What the host declares stays in reach either way: a nested class reads
	/// the static members of the class around it by their simple names, which is what a
	/// grammar's <c>=&gt;</c> calls are.
	/// </remarks>
	public string? Suffix { get; set; }

	/// <summary>
	/// Whether the host declares the <see cref="Suffix"/> class itself, and so decides who may
	/// see it.
	/// </summary>
	/// <remarks>
	/// A nested class written by the author as <c>internal static partial class Immediate { }</c>
	/// is the author's to make internal, and the generated part then says nothing of its own
	/// about accessibility. Where the author declares nothing the part is <c>public</c>, which
	/// is what it has always been — a nested class would otherwise default to private.
	/// </remarks>
	public bool SuffixDeclared { get; set; }

	/// <summary>
	/// The classes nested in the host that its readings are compiled into — every reading's
	/// <see cref="Suffix"/>, this one's among them — and the class the methods of a per-call
	/// reading stand in. Empty where the host has one reading.
	/// </summary>
	/// <remarks>
	/// What a member of the host or of a reading may not be called, as the host itself may not:
	/// a publication named after a reading's class is a method beside a class of one name. And
	/// what a rule's type is named against, by every reading alike, so that the type of a rule
	/// is one name in every reading of the host — <c>Url.UrlValue</c> and
	/// <c>Url.Traced.UrlValue</c>, not <c>Url.Traced.Url</c>, which a reading named against its
	/// own class alone would leave it.
	/// </remarks>
	public IReadOnlyList<string> Readings { get; set; } = [];

	/// <summary>
	/// Whether the host declares a part of <c>SourceSpan</c> itself, and so says how visible it is.
	/// </summary>
	/// <remarks>
	/// The span is <c>internal</c> unless a public signature the generator writes names it — a
	/// publication's value, a property of a rule's type. An author whose own public API hands
	/// one out raises it with <c>public partial struct SourceSpan { }</c> in the host, and the
	/// part generated beside it then says nothing of its own about accessibility, as a reading's
	/// class declared by the author does (<see cref="SuffixDeclared"/>).
	/// </remarks>
	public bool SpanDeclared { get; set; }

	/// <summary>
	/// Whether the class of this reading (<see cref="Suffix"/>), declared by the author, declares a
	/// part of <c>SourceSpan</c>.
	/// </summary>
	/// <remarks>
	/// A host with one reading in a class of its own has the span written where the author declared a
	/// part of it — in the host, which the reading's class reads it from, or in the reading's class —
	/// so that there is one span, of the author's accessibility. A host with several readings has one
	/// span, in the host, and a part declared in a reading's class would be a second: refused
	/// (GRAM5018).
	/// </remarks>
	public bool SpanDeclaredInReading { get; set; }

	/// <summary>
	/// The members the author already declares, in the class the limits of this reading are declared
	/// in — the class it is compiled into, or the host where it shares the host's (<see cref="SharesLimits"/>)
	/// — under a name the generated part declares for its callers (<c>Limits</c>, <c>ParserLimits</c>):
	/// two of one name in one class, refused (GRAM5018).
	/// </summary>
	public IReadOnlyList<string> MembersDeclared { get; set; } = [];

	/// <summary>
	/// Whether this reading reads the limits a caller sets from the host, declared there once for every
	/// reading of the host whose defaults are the first reading's, rather than declaring limits of its
	/// own (docs/syntax.md §6.5). Only where the host has several readings (<see cref="SharedTypes"/>),
	/// and then for each that leaves <c>MaxBufferedInputLength</c> and <c>MaxExtraStacks</c> as the first
	/// says them: one setting bounds every reading of the grammar, and a reading that says other defaults
	/// keeps a setting of its own, since one value cannot default to two.
	/// </summary>
	public bool SharesLimits { get; set; }

	/// <summary>
	/// The members the author declares in the class of this reading (<see cref="Suffix"/>) under a name
	/// of the limits, where the reading shares the host's (<see cref="SharesLimits"/>): a member there
	/// would stand between the reading and the host's, and is refused (GRAM5018).
	/// </summary>
	public IReadOnlyList<string> MembersDeclaredInReading { get; set; } = [];

	/// <summary>
	/// Whether the host has several readings, so that the types the host's own C# names — the
	/// span (§7.5), the match (§6.1) and the input a recognizer reads (§7.1) — are not written
	/// by this compilation but once for every reading, in a file of the host's own, from what
	/// each reading says it needs (<see cref="GramCompilation.SharedNeeds"/>). False where the
	/// host has one grammar and they are its own.
	/// </summary>
	public bool SharedTypes { get; set; }

	/// <summary>
	/// Whether the host inherits a grammar from a base class, so that what this
	/// compilation writes stands beside a parser the base already wrote.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A dialect is a class that inherits a grammar and publishes its own reading of
	/// it — T-SQL over standard SQL. Both classes are whole parsers, and the derived
	/// one names its members what the base named its own: <c>ParseSelect</c> is
	/// <c>ParseSelect</c> in either. In C# that is hiding, and hiding is exactly what
	/// a dialect means — <c>TransactSql.ParseSelect</c> reads T-SQL — so the warning
	/// asking whether it was intended is answered here rather than left to the
	/// consumer, who did not write the code it is about.
	/// </para>
	/// <para>
	/// The support types are hidden too, and are not shared instead: a base declares
	/// them only where it publishes something (<see cref="SharedTypes"/>), so a host
	/// that went without its own would have none at all wherever it inherits a grammar
	/// that publishes nothing — which is the ordinary shape of a grammar written to be
	/// inherited. Each class carries its own, and the values they hold — the trees the
	/// author's own <c>=&gt;</c> builds — are the same types either way.
	/// </para>
	/// </remarks>
	public bool Inherits { get; set; }
}
