using System;

using DotGram;

namespace DotGram.Ir.Prototype;

/// <summary>
/// A repetition that gives turns back inside a turn of another repetition the plan writes no way
/// for: a turn past the fewest is read only where the inner one is asked for a shorter reading.
/// </summary>
/// <remarks>
/// Each comes twice: as the grammar reads, with what <c>A*</c> reads taken as text, and built, with a
/// construction for each <c>A</c> in front of the repetition, which is what tells apart a turn read
/// again alone from the replay of the whole rule reading it again.
/// </remarks>
static class GiveBackGrammars
{
	/// <summary>
	/// <c>B*</c> reads the letter <c>Letter</c> then wants, in every turn of <c>+</c>; on
	/// <c>00a,b;c;</c> it must give <c>b</c> back in the first turn and <c>c</c> in the second. The
	/// braces have sealed before <c>eof</c> fails, so a turn given up as it first failed stays given up.
	/// </summary>
	public const string GivesBackInTurn = """
		Start : @V = { xs: A* & (B* & Letter & ';')+ } & eof => @(Twin.C(0, xs))
		A          = '0'
		B          = Letter & ','?
		Letter     = ['a'..'z']

		parse Start
		""";

	/// <summary><see cref="GivesBackInTurn"/>, each <c>A</c> built.</summary>
	public const string GivesBackInTurnBuilt = """
		Start : @V = { xs: A* & (B* & Letter & ';')+ } & eof => @(Twin.C(0, (object)xs))
		A     : @V = t: '0'                                 => @(Twin.C(1, t))
		B          = Letter & ','?
		Letter     = ['a'..'z']

		parse Start
		""";

	/// <summary>
	/// The same give-back in a turn of a repetition read past a one-character stop: what follows
	/// the closing quote cannot begin with a quote, so a doubled quote is never the end, and the plan
	/// writes no way for the repetition. On <c>0"a,b;"</c> the turn <c>B+ &amp; Letter &amp; ';'</c> must
	/// give <c>b</c> back. No braces stand between the repetition and its stop, so a turn given up
	/// there is read again by the replay of the whole rule, which answers alike.
	/// </summary>
	public const string GivesBackPastStop = """
		Start : @V = xs: A* & '"' & ("\"\"" | B+ & Letter & ';')* & '"' & eof => @(Twin.C(0, xs))
		A          = '0'
		B          = Letter & ','?
		Letter     = ['a'..'z']

		parse Start
		""";

	/// <summary><see cref="GivesBackPastStop"/>, each <c>A</c> built: the replay of the whole rule builds <c>A*</c> once more.</summary>
	public const string GivesBackPastStopBuilt = """
		Start : @V = xs: A* & '"' & ("\"\"" | B+ & Letter & ';')* & '"' & eof => @(Twin.C(0, (object)xs))
		A     : @V = t: '0'                                                  => @(Twin.C(1, t))
		B          = Letter & ','?
		Letter     = ['a'..'z']

		parse Start
		""";
}

[Gram(GiveBackGrammars.GivesBackInTurn, Carrier = GramCarrier.Immediate)]        static partial class GivesBackInTurnParser;
[Gram(GiveBackGrammars.GivesBackInTurnBuilt, Carrier = GramCarrier.Immediate)]   static partial class GivesBackInTurnBuiltParser;
[Gram(GiveBackGrammars.GivesBackPastStop, Carrier = GramCarrier.Immediate)]      static partial class GivesBackPastStopParser;
[Gram(GiveBackGrammars.GivesBackPastStopBuilt, Carrier = GramCarrier.Immediate)] static partial class GivesBackPastStopBuiltParser;
