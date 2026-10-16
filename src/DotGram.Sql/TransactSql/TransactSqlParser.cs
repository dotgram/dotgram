using System;

using DotGram;
using DotGram.Sql.Standard;

namespace DotGram.Sql.TransactSql;

/// <summary>
/// A first slice of Microsoft's T-SQL, written as a dialect of <see cref="Sql92Parser"/>
/// rather than as a grammar of its own.
/// </summary>
/// <remarks>
/// <c>ParseSql</c> reads what a program sends the server in one call, and reads it as the server
/// does — so a <c>GO</c> there is a word, and <c>SELECT 1</c> and then a line <c>GO</c> is a column
/// called <c>GO</c>. Text from a file or an editor is a script, whose <c>GO</c> lines and sqlcmd
/// commands are the client tool's: read it with <c>ParseScript</c>, or cut it with
/// <see cref="SqlScript.Read"/> and read each batch with <c>TryParseSql(ScriptBatch)</c>.
/// <see cref="SqlScript.HasClientSyntax"/> says whether text of unknown origin is a script.
/// </remarks>
// One parser, read through two doors: the methods below, and the same methods in `Located`,
// which ask the same reading to tell every value where it was written and where the text in
// front of it began. Locations are not free for a recognizer, so whoever needs them asks for
// them by name, `TransactSqlParser.Located.TryParseStatement`. The grammar is compiled once: one
// reader, which asks once a record whether to keep where the record stands, and a walk that builds
// the values generic over whether the reading locates, which the runtime compiles once for each.
// The standard this dialect is written on top of, named rather than inherited: a class has
// one base and as many attributes as it likes, and what a grammar is called inside this one
// — `Sql92.ValueExpression` — is this grammar's business rather than the standard's.
// Carried immediately, by name: every construction runs the moment its alternative is read,
// rather than being recorded on the tape and replayed after the parse is accepted. The gates
// `Auto` asks would keep this grammar on the tape (about half of its building rules are read for
// derivations the parse may give up), and that is what the request costs: a construction runs for
// a reading that is then abandoned, and an exception thrown there escapes the publication. Here
// none can. The factories (SqlSyntax.cs) build records from what they are given and read no host
// state; the dialect's version is a binding fixed at compile time and whether a reading locates is
// a flag of the call, so a discarded derivation leaves nothing behind that a later one could read.
// Held to the tape by an agreement run over the ScriptDom corpus, each statement whole and cut,
// every string of the tests, every accepted text cut at each word boundary and given a stray
// token, through the plain and the located door: the same trees, the same messages, nothing thrown.
// What it buys is the time the tape spent recording and replaying — about half of every T-SQL
// row of the stand — for more bytes allocated on a reading that is abandoned part-way.
[GramInclude(typeof(Sql92Parser), As = "Sql92")]
[Gram("TransactSql.gram", Lexical = true, Carrier = GramCarrier.Immediate)]
[GramOptions(LocationType = typeof(ISqlLocatable), Suffix = "Located", PerCall = true)]
public abstract partial class TransactSqlParser
{
}
