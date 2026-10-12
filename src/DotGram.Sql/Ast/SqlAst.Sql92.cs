using System;

namespace DotGram.Sql.Ast;

// SQL-92's own nodes: what ISO/IEC 9075:1992 has and SQL:2003 removed, which Sql92Parser reads and
// SqlStandardParser does not. Every record here carries `// SQL-92:` with its clause, so a reader can find it in
// the 1992 text, and says that SQL:2003 removed it. The one value of an existing enum, JoinKind.Union, is in
// SqlAst.cs beside the others, an enum not being partial.

public abstract partial record DataType
{
	/// <summary>
	/// SQL-92: §6.1 &lt;data type&gt;, &lt;bit string type&gt;; removed in SQL:2003. <c>BIT</c> and
	/// <c>BIT VARYING</c>, with a length where one was written, as <see cref="Character"/> holds its kind.
	/// </summary>
	/// <remarks>
	/// A string of bits, not T-SQL's <c>bit</c>, which is an integer that is 0 or 1 and is
	/// <see cref="Named"/>.
	/// </remarks>
	public record BitString(BitStringTypeKind Kind, int? Length = null) : DataType;
}

/// <summary>SQL-92: §6.1 &lt;bit string type&gt;; removed in SQL:2003. <c>BIT</c> or <c>BIT VARYING</c>.</summary>
public enum BitStringTypeKind { Bit, BitVarying }

public abstract partial record LiteralValue
{
	/// <summary>
	/// SQL-92: §5.3 &lt;bit string literal&gt;; removed in SQL:2003. <c>B'0101'</c>: the text from its first quote,
	/// as <see cref="Binary"/> holds <c>X'…'</c>.
	/// </summary>
	public record BitString(string Text) : LiteralValue;
}
