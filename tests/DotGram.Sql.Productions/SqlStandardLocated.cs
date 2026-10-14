using DotGram.Sql.Standard;

namespace DotGram.Sql.Productions;

/// <summary>
/// <see cref="SqlStandardParser"/>'s grammar read with locations: every rule offers the value it built
/// the range it was read over, for the tests that ask where the nodes of the SQL:2023 tree were written.
/// </summary>
/// <remarks>
/// The shipped parser does not locate, and no other parser builds this tree yet; the T-SQL reading
/// that will locates through the same <see cref="ISqlSpan.Locate"/>. What is held here is the tree's
/// side of that: a node made where it is read keeps the span offered to it, and a name, which keeps
/// none of its own, spans its parts.
/// <para>
/// <b>No FROM clause.</b> Two rules hand back null for what was not written (a table's correlation
/// name, and a row pattern after it), and a reading that tells every rule's value where it was read
/// tells null too, which throws. A located reading of this grammar wants those rules to hand back a
/// value first; until then the tests read statements, expressions, data types and values without one.
/// </para>
/// </remarks>
[GramInclude(typeof(SqlStandardParser))]
[Gram("SqlStandardLocated.gram", Lexical = true, Carrier = GramCarrier.Immediate, Portable = false, LocationType = typeof(ISqlSpan))]
public abstract partial class SqlStandardLocated
{
}
