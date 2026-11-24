using System;

namespace DotGram.QueryLanguage;

/// <summary>
/// The bounds every query of a model is held to. Each guards a cost the author of a text would otherwise
/// choose: the length and nesting of the text, the size of the expression tree the host's provider
/// translates — work no command timeout covers — and how far into the rows a text may reach.
/// </summary>
/// <remarks>
/// <para>
/// The bounds on the text — its length, nesting, depth, operands, <c>in</c> lists and columns — are counted on
/// the text as written, before anything is looked up. <see cref="MaxNodes"/> counts it once more as it expands:
/// a name counts at every use.
/// </para>
/// <para>
/// The bounds limit cost, not secrecy: what a text may learn is decided by the entity's source and by the
/// uses of its fields, never by how many rows it can reach.
/// </para>
/// </remarks>
public sealed record QlOptions
{
	/// <summary>The bounds a model has when <see cref="QlModelBuilder{TContext}.Build"/> is given none.</summary>
	public static QlOptions Default { get; } = new();

	/// <summary>Characters of text, checked before the text is read. 8 KB by default.</summary>
	public int MaxTextLength { get; init; } = 8 * 1024;

	/// <summary>Nesting of the text as written, parentheses, <c>!</c> and unary minus included. 64 by default.</summary>
	public int MaxSyntaxDepth { get; init; } = 64;

	/// <summary>
	/// Nodes of the query: each clause, operator, name, literal, parameter and member counts one, and so
	/// does a list after <c>in</c> however long it is. A name counts at every use: the name of a <c>let</c> or a
	/// <c>select</c> column counts as the nodes of what it names, so that names cannot hide a large expression.
	/// 100 by default.
	/// </summary>
	public int MaxNodes { get; init; } = 100;

	/// <summary>Depth of one expression, parentheses not counted. 32 by default.</summary>
	public int MaxTreeDepth { get; init; } = 32;

	/// <summary>Operands of one arithmetic expression, through parentheses and unary minus. 8 by default.</summary>
	public int MaxArithmeticOperands { get; init; } = 8;

	/// <summary>Values of one <c>in</c> list, written in the text or passed as a parameter. 100 by default.</summary>
	public int MaxInList { get; init; } = 100;

	/// <summary>Columns of one <c>select</c>, and of the row a query returns. 32 by default.</summary>
	public int MaxColumns { get; init; } = 32;

	/// <summary>
	/// Rows a query returns at most: a <c>Take</c> of this many is the last call before the final
	/// projection. Null adds no <c>Take</c>, and the host bounds the rows itself. 1000 by default.
	/// <see cref="QlModel{TContext}.ToJsonAsync"/> reads one row more than this, to tell whether the result was cut.
	/// </summary>
	public int? MaxRows { get; init; } = 1000;

	/// <summary>Rows all the <c>skip</c>s of a text pass over together. 10,000 by default.</summary>
	public int MaxSkip { get; init; } = 10_000;

	/// <summary>Characters of one string, a literal or a value. 4 KB by default.</summary>
	public int MaxStringLength { get; init; } = 4 * 1024;

	/// <summary>
	/// Bytes of all the values passed with one query: a string as UTF-16, a byte array by its length, a value of the
	/// language's types by its size, any other value as 16 bytes and any other array as 16 bytes an item. 16 KB by
	/// default.
	/// </summary>
	public int MaxParameterBytes { get; init; } = 16 * 1024;
}
