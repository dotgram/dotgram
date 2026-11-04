using System;

using DotGram;

namespace DotGram.Sql.PostgreSql;

/// <summary>
/// PostgreSQL 18, as the server's own parser reads it: an expression, and the statements as they
/// are written. Written as a grammar that stands on its own, from PostgreSQL's published grammar
/// and lexer, and held to that parser's verdicts.
/// </summary>
/// <remarks>
/// <para>
/// It reads at the standard's levels, with <c>SqlStandardParser</c>'s names and the same tree
/// (<c>DotGram.Sql.Ast</c>): <c>ParseExpression</c> so far. A text two parsers read is the same tree.
/// </para>
/// <para>
/// The rule is the engine's parser's: what gram.y reads, this reads, the refusals its own actions
/// raise included; what parse analysis refuses afterwards — a name that is no table, a type that is
/// none — is not syntax, and is read.
/// </para>
/// </remarks>
// One parser, read through two doors: the methods below, and the same methods in `Located`, which
// tell every value where it was written. The second door is a second compilation of the grammar
// for now: a door asked per call, as T-SQL's is, does not yet compile beside a terminal that binds
// text (the dollar quote). Carried immediately, by name: a
// node is built the moment its rule has been read, and the factories of `Nodes` build records from
// what they are given and read no host state, so a derivation given up leaves nothing a later one
// could read.
[Gram("PostgreSql.gram", Lexical = true, Carrier = GramCarrier.Immediate)]
[GramOptions(LocationType = typeof(ISqlSpan), Suffix = "Located")]
public abstract partial class PostgreSqlParser
{
	// Every entry is static and nothing is made of the class, so nothing outside derives from it either.
	PostgreSqlParser()
	{
	}
}
