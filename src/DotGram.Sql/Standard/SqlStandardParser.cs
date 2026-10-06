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
/// whether this grammar's rule of the same name says the same, asked of a test fixture that includes
/// the grammar and publishes its productions one by one.
/// </para>
/// <para>
/// <b>It reads at six levels</b>, not production by production: <c>ParseValue</c>, a literal, signed
/// numbers and <c>NULL</c> among them; <c>ParseDataType</c>; <c>ParseExpression</c>;
/// <c>ParseSearchCondition</c>; <c>ParseStatement</c>, one statement of any kind, its semicolon
/// optional; and <c>ParseSql</c>, statements each ended by a semicolon or by the end of the text. A
/// statement's kind is the type of its node, and a query is a <c>Statement.Select</c>.
/// </para>
/// <para>
/// <b>The rule names are the standard's</b>, production for production: <c>&lt;query
/// expression&gt;</c> is <c>QueryExpression</c>. A rule that says a production in another shape —
/// because an ordered choice commits where the BNF's does not — says why above it.
/// </para>
/// <para>
/// Written from the newest edition down, chapter by chapter: so far §5, the lexical elements, §6,
/// scalar expressions, §7, query expressions with row pattern recognition, §8, predicates, and §10.9,
/// aggregates, with the JSON functions as far as the BNF spells them, §11 and §12's whole schema, §14's
/// data change statements and cursors, and the control, transaction, connection, session, dynamic,
/// direct and diagnostics statements.
/// It builds the SQL:2023 tree of <c>Sql2023Ast.cs</c> (docs/design/sql-ast.md): names, literals,
/// data types, expressions, queries, data change statements, schema definitions and the other
/// statements above. <c>DotGram.Sql.Ast.Sql2023Writer</c> writes the tree back as SQL.
/// </para>
/// <para>
/// <b>The towers are carried, not tried.</b> The BNF types its value expressions — numeric,
/// character, datetime, interval — as towers that meet only in a primary, and a parser has no
/// types; an expression is read once and <see cref="Towers"/> says which towers it still belongs to.
/// </para>
/// <para>
/// <b>It is carried immediately, by name.</b> A node is built the moment its rule has been read, and
/// nothing is recorded on a tape and replayed once the parse is accepted. Left to choose, the
/// generator would keep this grammar on the tape: some three hundred of its five hundred building
/// rules are read for derivations the parse may give up — an ordered choice tries the longer
/// alternative first — and that is what the request costs: a construction runs for a reading that is
/// then abandoned, and an exception thrown there would escape the publication. Here none can. The
/// factories of <c>Nodes</c> and <see cref="Towers"/> build records from what they are given and
/// read no host state, and the guards read a tower's roles and nothing else, so a derivation given
/// up leaves nothing a later one could read. Held to the tape by an agreement run over the ScriptDom
/// corpus, each statement whole and cut, every string of the tests, every accepted text cut at each
/// word boundary and given a stray token, through all six entries: the same trees, the same
/// messages, nothing thrown; and <c>tests/DotGram.Sql.Productions</c> compiles the same grammar on
/// the tape, so the two carriers are held to each other on every test run. What it buys is the time
/// the tape spent recording and replaying, for more bytes allocated where a reading is abandoned
/// part-way.
/// </para>
/// </remarks>
[Gram("SqlStandard.gram", Lexical = true, Carrier = GramCarrier.Immediate)]
public abstract partial class SqlStandardParser
{
}
