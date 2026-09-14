using System;

using DotGram;

namespace DotGram.Sql.Standard;

/// <summary>
/// ISO/IEC 9075-2:2023, SQL/Foundation: the standard's grammar, written from its BNF.
/// </summary>
/// <remarks>
/// <para>
/// <b>The BNF is the authority, and it is asked.</b> The standard has no engine to put a
/// statement to, so <c>--standard</c> in DotGram.Benchmarks reads the published BNF with an
/// Earley recognizer and says, line by line, whether a text is the production named — and
/// whether this grammar's rule of the same name says the same.
/// </para>
/// <para>
/// <b>The rule names are the standard's</b>, production for production: <c>&lt;query
/// expression&gt;</c> is <c>QueryExpression</c>. A rule that says a production in another shape —
/// because an ordered choice commits where the BNF's does not — says why above it.
/// </para>
/// <para>
/// Written from the newest edition down, chapter by chapter: so far §5, the lexical elements, §6,
/// scalar expressions, §7, query expressions with row pattern recognition, §8, predicates, and §10.9,
/// aggregates — but the JSON functions. Nothing is built yet: the
/// grammar recognizes, and the tree follows the standard's shape once the language is right.
/// </para>
/// <para>
/// <b>The towers are carried, not tried.</b> The BNF types its value expressions — numeric,
/// character, datetime, interval — as towers that meet only in a primary, and a parser has no
/// types; an expression is read once and <see cref="Towers"/> says which towers it still belongs to.
/// </para>
/// </remarks>
[Gram("SqlStandard.gram")]
public abstract partial class SqlStandardParser
{
}
