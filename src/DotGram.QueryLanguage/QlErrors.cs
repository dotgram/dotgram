using System;

namespace DotGram.QueryLanguage;

/// <summary>What is wrong with a query text or with the values passed with it.</summary>
public enum QlErrorCode
{
	/// <summary>The text is not the language.</summary>
	Syntax = 1,

	/// <summary>A name the model does not expose, or does not expose for the use it is put to.</summary>
	UnknownName = 2,

	/// <summary>Values the language does not combine.</summary>
	Type = 3,

	/// <summary>Past one of the bounds of <see cref="QlOptions"/>.</summary>
	Limit = 4,

	/// <summary>A value missing, of a type that does not fit, or out of its range.</summary>
	Parameter = 5,

	/// <summary>A division by zero in a part of the query that reads no row.</summary>
	DivisionByZero = 6,

	/// <summary>An overflow in a part of the query that reads no row.</summary>
	Overflow = 7,
}

/// <summary>
/// One error of a query text or of its values. It speaks the language's words only — the names the text
/// and the model use, the language's types — and is meant to be shown to whoever wrote the text.
/// </summary>
public sealed class QlError
{
	internal QlError(QlErrorCode code, string message, int? position)
	{
		Code     = code;
		Message  = message;
		Position = position;
	}

	/// <summary>What kind of error it is.</summary>
	public QlErrorCode Code { get; }

	/// <summary>What is wrong, in a sentence.</summary>
	public string Message { get; }

	/// <summary>The zero-based offset in the text of the character the error is about, where there is one.</summary>
	public int? Position { get; }
}

/// <summary>A query text or its values were refused. Nothing has run.</summary>
public sealed class QlException : Exception
{
	internal QlException(QlErrorCode code, string message, int? position)
		: base(message)
	{
		Error = new QlError(code, message, position);
	}

	/// <summary>The error.</summary>
	public QlError Error { get; }
}

/// <summary>What is worth telling whoever wrote a query that runs as it is.</summary>
public enum QlWarningCode
{
	/// <summary>A <c>let</c> or a <c>select</c> column that nothing after it reads.</summary>
	UnusedName = 1,
}

/// <summary>One warning about a query text. A warning never stops a query.</summary>
public sealed class QlWarning
{
	internal QlWarning(QlWarningCode code, string message, int? position)
	{
		Code     = code;
		Message  = message;
		Position = position;
	}

	/// <summary>What kind of warning it is.</summary>
	public QlWarningCode Code { get; }

	/// <summary>What the warning is about, in a sentence.</summary>
	public string Message { get; }

	/// <summary>The zero-based offset in the text of the name the warning is about.</summary>
	public int? Position { get; }
}
