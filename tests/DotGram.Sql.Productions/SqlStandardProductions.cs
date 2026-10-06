using DotGram.Sql.Standard;

namespace DotGram.Sql.Productions;

/// <summary>
/// The productions of ISO/IEC 9075-2:2023 one by one: <see cref="SqlStandardParser"/>'s grammar,
/// included and published again production by production, for the BNF oracle and the tests that read
/// one production at a time.
/// </summary>
/// <remarks>
/// <para>
/// <b>A sibling, not the shipped parser.</b> An including class compiles a parser of its own from the
/// included text — its own kinds, lexical automaton, machines and memo — so what is tested here is the
/// grammar and the include, not the binary that ships. The six levels <see cref="SqlStandardParser"/>
/// publishes are published here too, under the same names, and the tests hold the two to each other:
/// the same trees and the same messages. A difference between them is a defect of the include.
/// </para>
/// <para>
/// On the tape, which is what the shipped parser takes too, and not portable: the class is internal to
/// the tests, and carrying the included grammar's text a second time would only weigh the assembly.
/// </para>
/// </remarks>
[GramInclude(typeof(SqlStandardParser))]
[Gram("SqlStandardProductions.gram", Lexical = true, Carrier = GramCarrier.Tape, Portable = false)]
public abstract partial class SqlStandardProductions
{
}
