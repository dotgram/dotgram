using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DotGram.Tests.RuleValues;

/// <summary>Where the value of a rule comes from, or why it has none.</summary>
enum Source
{
	/// <summary>A string rule whose pieces are not all text: it is made by joining them.</summary>
	Joined,

	/// <summary>The rule's own text (a string rule whose pieces are all text), or its bounds (a span).</summary>
	Extent,

	/// <summary>An array of the calls of the element type.</summary>
	Collected,

	/// <summary>The one operand of the rule's type.</summary>
	Operand,

	/// <summary>Built from the names the rule writes.</summary>
	Constructed,

	/// <summary>The grammar is refused: <see cref="Expectation.ErrorId"/> says why.</summary>
	Error,

	/// <summary>The rule gives no answer: <see cref="Expectation.Question"/> says what is missing.</summary>
	Undecided,

	/// <summary>The grammar does not read the input (a body that cannot succeed on the way a route was written).</summary>
	NoMatch,
}

/// <summary>
/// What the oracle expects of one input.
/// </summary>
/// <param name="Source">Where the value comes from.</param>
/// <param name="Value">The value as the runner prints it (null unless the source gives one).</param>
/// <param name="ErrorId">The identifier of the refusal, for <see cref="Source.Error"/>.</param>
/// <param name="Question">What the rule does not say, for <see cref="Source.Undecided"/>.</param>
/// <param name="Row">The kind of the rule that decided it: C, S, A or O.</param>
sealed record Expectation(Source Source, string? Value, string? ErrorId, string? Question, string Row)
{
	/// <summary>The grammar is built and gives <see cref="Value"/>.</summary>
	public bool IsValue => Value is not null && Source is not (Source.Error or Source.Undecided or Source.NoMatch);

	/// <summary>The grammar is refused.</summary>
	public bool IsError => Source == Source.Error;

	/// <summary>The oracle has no answer to compare with.</summary>
	public bool IsUnknown => Source == Source.Undecided;

	public override string ToString()
	{
		return Source switch
		{
			Source.Error     => "error " + ErrorId,
			Source.Undecided => "undecided " + Question,
			_                => Value + " (" + Source + ", rule " + Row + ")",
		};
	}
}

/// <summary>A value as the runner prints it, with what a constructor or a collection needs of it.</summary>
sealed record Val(string Type, string Raw, string Print, IReadOnlyList<Val>? Items = null)
{
	public static Val Str(string text)
	{
		return new Val("string", text, "\"" + RuleRunner.Escape(text) + "\"");
	}

	public static Val Bytes(string text)
	{
		return new Val("bytes", text, "bytes\"" + RuleRunner.Escape(text) + "\"");
	}

	public static Val Span(int start, int end)
	{
		var print = "[" + start.ToString(CultureInfo.InvariantCulture) + ".." + end.ToString(CultureInfo.InvariantCulture) + ")";

		return new Val("SourceSpan", print, print);
	}

	public static Val Int(int number)
	{
		var text = number.ToString(CultureInfo.InvariantCulture);

		return new Val("int", text, text);
	}

	public static Val Char(char c)
	{
		return new Val("char", c.ToString(), "'" + RuleRunner.Escape(c.ToString()) + "'");
	}

	public static Val Host(string type, string print)
	{
		return new Val(type, print, print);
	}

	public static Val Array(IReadOnlyList<Val> items)
	{
		return new Val("array", "", "[" + string.Join(",", items.Select(static i => i.Print)) + "]", items);
	}
}

/// <summary>The source and the decision of one rule, before any input.</summary>
sealed record Decision(Source Source, string Row, string? ErrorId, string? Question)
{
	public static Decision Of(Source source, string row)
	{
		return new Decision(source, row, null, null);
	}

	public static Decision Fail(string row, string id)
	{
		return new Decision(Source.Error, row, id, null);
	}

	public static Decision Ask(string row, string question)
	{
		return new Decision(Source.Undecided, row, null, question);
	}
}
