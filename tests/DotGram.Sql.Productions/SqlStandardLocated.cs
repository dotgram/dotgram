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
/// </remarks>
[GramInclude(typeof(SqlStandardParser))]
[Gram("SqlStandardLocated.gram", Lexical = true, Carrier = GramCarrier.Immediate, Portable = false, LocationType = typeof(ISqlSpan))]
public abstract partial class SqlStandardLocated
{
}
