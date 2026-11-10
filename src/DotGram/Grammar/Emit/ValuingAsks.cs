using System;

namespace DotGram.Grammar.Emit;

/// <summary>
/// What the machine that reads a token's text again over characters asks of every call into it besides that text: the
/// grammar's context, the reading this parser is, and whether the reading locates.
/// </summary>
/// <remarks>
/// <para>
/// That machine is called from outside itself in two places: by the tokenizer, which measures a terminal the automaton
/// only begins, and by the syntactic machine, which has a terminal's value built from the token (<c>Value_</c>). Each of
/// those is a method of its own that has to take what it hands on, and each call of theirs has to hand it. A parameter
/// the machine takes and one of them forgets is a parser that does not compile, which is how the context and the reading
/// were each missed at a call the located argument had already been added to. So what is taken and what is handed are
/// both written here, in one order, and every one of those methods and calls is written from this.
/// </para>
/// <para>
/// Known before the machine is made — the syntactic machine, made first, has to have the context and the reading in
/// scope wherever it builds a token's value — and checked against the machine once it is
/// (<see cref="Of(Machine)"/>).
/// </para>
/// </remarks>
readonly record struct ValuingAsks(bool Context, bool Reading, bool Locating)
{
	/// <summary>What the machine itself asks for, as its wrappers declare it (<see cref="Machine.RenderWrapper"/>).</summary>
	public static ValuingAsks Of(Machine machine)
	{
		return new ValuingAsks(machine.UsesContext, machine.UsesReading, machine.UsesLocating);
	}

	/// <summary>
	/// The parameters of a method that hands these on: the context, typed <paramref name="context"/>, and the reading
	/// where they are asked for, and whether the reading locates where <paramref name="locating"/> says the method is told.
	/// </summary>
	/// <remarks>
	/// The tokenizer is not told whether the reading locates: a measuring builds no value, so it says it does not
	/// (<see cref="Arguments"/> with <c>-1</c>).
	/// </remarks>
	public string Parameters(string? context, bool locating)
	{
		if (Context && context is null)
			throw new InvalidOperationException("The machine asks for a context and the grammar declares none.");

		return (Context ? $", {context} context" : "") +
			(Reading ? ", int parserReading" : "") +
			(Locating && locating ? ", int parserLocating" : "");
	}

	/// <summary>
	/// The arguments of a call that hands these on, in the order <see cref="Parameters"/> declares them, each from what
	/// the caller has for it; a null <paramref name="locating"/> where the method called is not told it.
	/// </summary>
	public string Arguments(string context, string reading, string? locating)
	{
		return (Context ? ", " + context : "") +
			(Reading ? ", " + reading : "") +
			(Locating && locating is not null ? ", " + locating : "");
	}
}
