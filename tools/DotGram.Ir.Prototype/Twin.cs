using System;
using System.Globalization;
using System.Text;

namespace DotGram.Ir.Prototype;

/// <summary>
/// What a numbered construction builds: the text that says which one it was and what it was handed.
/// </summary>
sealed class V(string text)
{
	public string Text { get; } = text;

	public override string ToString()
	{
		return Text;
	}
}

/// <summary>
/// The same as <see cref="V"/>, a type of its own: what a rule builds where the generated C#
/// parser has to tell it from <see cref="V"/> — two lists of one type in one rule are carried
/// only deferred there, and the grammars that are to be held to the immediate carrier keep them apart.
/// </summary>
sealed class W(string text)
{
	public string Text { get; } = text;

	public override string ToString()
	{
		return Text;
	}
}

/// <summary>Thrown by a construction a test has asked to fail.</summary>
sealed class TwinException(string call) : Exception(call);

/// <summary>
/// The host of the C# parsers of <see cref="Grammars"/>: every construction and every guard is a
/// call by number, answered with a text in one spelling and written down in the order it ran.
/// </summary>
/// <remarks>
/// The spelling is what the evaluator and the parsers printed in other languages are compared in:
/// a text read from the input in quotes, <c>null</c> for a capture nothing filled, a sequence in
/// square brackets, a number in digits, and what a construction built as <c>c3(…)</c>.
/// </remarks>
static class Twin
{
	[ThreadStatic] static List<string>? _log;
	[ThreadStatic] static string?       _throwOn;

	/// <summary>Runs a parse with the calls it makes written down, and says what they were.</summary>
	public static List<string> Record(Action parse, string? throwOn = null)
	{
		var log = new List<string>();

		_log     = log;
		_throwOn = throwOn;

		try
		{
			parse();
		}
		finally
		{
			_log     = null;
			_throwOn = null;
		}

		return log;
	}

	public static V C(int id, params object?[] args)
	{
		var call = "c" + id.ToString(CultureInfo.InvariantCulture) + "(" + string.Join(",", args.Select(Canon)) + ")";

		_log?.Add(call);

		if (call == _throwOn)
			throw new TwinException(call);

		return new V(call);
	}

	/// <summary><see cref="C"/>, building a <see cref="W"/>.</summary>
	public static W D(int id, params object?[] args)
	{
		return new W(C(id, args).Text);
	}

	public static bool G(int id, bool answer, params object?[] args)
	{
		_log?.Add(GuardCall(id, args.Select(Canon)) + "=" + (answer ? "true" : "false"));

		return answer;
	}

	public static string GuardCall(int id, IEnumerable<string> args)
	{
		return "g" + id.ToString(CultureInfo.InvariantCulture) + "(" + string.Join(",", args) + ")";
	}

	public static string Canon(object? value)
	{
		return value switch
		{
			null                => "null",
			V built             => built.Text,
			W other             => other.Text,
			string text         => Quoted(text),
			int number          => number.ToString(CultureInfo.InvariantCulture),
			bool flag           => flag ? "true" : "false",
			IEnumerable<object?> many => "[" + string.Join(",", many.Select(Canon)) + "]",
			_                   => throw new ArgumentException("No spelling for " + value.GetType().Name + "."),
		};
	}

	/// <summary>A text in quotes, with only what would break a line or the quotes escaped.</summary>
	public static string Quoted(string text)
	{
		var quoted = new StringBuilder(text.Length + 2).Append('"');

		foreach (var unit in text)
		{
			if (unit is '"' or '\\' || unit < ' ' || unit > '~')
				quoted.Append("\\u").Append(((int)unit).ToString("x4", CultureInfo.InvariantCulture));
			else
				quoted.Append(unit);
		}

		return quoted.Append('"').ToString();
	}
}
