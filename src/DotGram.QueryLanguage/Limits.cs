using System;

namespace DotGram.QueryLanguage;

/// <summary>
/// The bounds a query is held to. Each guards a cost an attacker would otherwise choose: the
/// length and depth of the text, the size and depth of the expression tree linq2db translates —
/// whose translation time no command timeout bounds — and what the database returns.
/// </summary>
public sealed record QlLimits
{
	public static QlLimits Default { get; } = new();

	/// <summary>Characters of text, checked before the text is read.</summary>
	public int MaxTextLength { get; init; } = 8 * 1024;

	/// <summary>Nesting of the text as written: parentheses, NOT and unary minus included.</summary>
	public int MaxSyntaxDepth { get; init; } = 64;

	/// <summary>Nodes of the built expression tree, the query's own part of it.</summary>
	public int MaxNodes { get; init; } = 100;

	/// <summary>Depth of the built expression tree.</summary>
	public int MaxTreeDepth { get; init; } = 32;

	/// <summary>Operands of one chain of arithmetic, left-deep as written.</summary>
	public int MaxArithmeticOperands { get; init; } = 8;

	/// <summary>Depth of an arithmetic sub-tree.</summary>
	public int MaxArithmeticDepth { get; init; } = 8;

	/// <summary>Values of one IN list, written or passed.</summary>
	public int MaxInList { get; init; } = 100;

	/// <summary>Fields of the result.</summary>
	public int MaxColumns { get; init; } = 32;

	/// <summary>Rows returned whatever LIMIT says.</summary>
	public int MaxRows { get; init; } = 1000;

	public int MaxOffset { get; init; } = 10_000;

	/// <summary>Characters of one string parameter or literal.</summary>
	public int MaxStringLength { get; init; } = 4 * 1024;

	public TimeSpan CommandTimeout { get; init; } = TimeSpan.FromSeconds(5);
}
