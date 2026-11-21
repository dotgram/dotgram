using System;

using DotGram;

namespace DotGram.Ir.Prototype;

/// <summary>
/// A repetition that gives turns back inside a turn of another repetition: the first turn of the
/// outer one is read again only where the inner one is asked for a shorter reading.
/// </summary>
static partial class GiveBackGrammars
{
	/// <summary>
	/// <c>B*</c> reads the letter <c>Letter</c> then wants, in every turn of <c>+</c>; on
	/// <c>00a,b;c;</c> it must give <c>b</c> back in the first turn and <c>c</c> in the second.
	/// </summary>
	public const string GivesBackInTurn = """
		Start : @V = { xs: A* & (B* & Letter & ';')+ } & eof => @(Twin.C(0, xs))
		A     : @V = t: '0'                                 => @(Twin.C(1, t))
		B          = Letter & ','?
		Letter     = ['a'..'z']

		parse Start
		""";
}

[Gram(GiveBackGrammars.GivesBackInTurn, Carrier = GramCarrier.Immediate)] static partial class GivesBackInTurnParser;
