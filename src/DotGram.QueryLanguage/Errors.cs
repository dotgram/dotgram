using System;

namespace DotGram.QueryLanguage;

public enum QlErrorKind
{
	/// <summary>The text is not the language.</summary>
	Syntax,

	/// <summary>A name the model does not expose, or does not expose for that use.</summary>
	UnknownName,

	/// <summary>Operands the language does not combine.</summary>
	Type,

	/// <summary>A limit of the text, the tree, a list or the result.</summary>
	Limit,

	/// <summary>A parameter missing, of the wrong type, or out of its bounds.</summary>
	Parameter,

	/// <summary>The audit of the built tree refused a node: a defect of the translator, never of the text.</summary>
	Audit,

	DivisionByZero,
	Overflow,
	Timeout,
	Canceled,

	/// <summary>The database refused the command; what it said goes only to the host's log.</summary>
	Database,
}

/// <summary>
/// What the caller of a query is told. It names only what the text and the model name: never a
/// CLR type, a member of the entity, the SQL or a value.
/// </summary>
public sealed class QlError(QlErrorKind kind, string message, int? position = null)
{
	public QlErrorKind Kind     { get; } = kind;
	public string      Message  { get; } = message;
	public int?        Position { get; } = position;

	public override string ToString()
	{
		return Position is { } at ? $"{Kind} at {at}: {Message}" : $"{Kind}: {Message}";
	}
}

public sealed class QlException : Exception
{
	public QlException(QlError error, Exception? inner = null)
		: base(error.ToString(), inner)
	{
		Error = error;
	}

	public QlError Error { get; }
}
