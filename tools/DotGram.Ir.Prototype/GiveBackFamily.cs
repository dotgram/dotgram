using System;

using DotGram;

namespace DotGram.Ir.Prototype;

/// <summary>
/// Written by enumeration, not by hand: <c>R1 &amp; (R2 &amp; X)Q</c>, a repetition before a repetition of
/// a turn that gives back. <c>R2</c> is <c>B*</c>, <c>B+</c> or <c>B?</c> with <c>B = Letter &amp; ','?</c>;
/// <c>X</c> begins as <c>B</c> does (<c>Letter</c>, <c>Letter &amp; Letter</c>); the turn ends in
/// <c>';'</c> or in nothing (only under <c>{2}</c>: under <c>+</c> and <c>*</c> the generator refuses a turn
/// that can be cut two ways); <c>Q</c> is <c>+</c>, <c>*</c> or <c>{2}</c>; it stands bare, in braces or
/// in a look; and the grammar builds under the deferred carrier, under the immediate one, or builds
/// nothing.
/// </summary>
static class GiveBackFamily
{
	public const string StarLSemiPlusPlainDeferred = """
		Start : @V = xs: A* & (bs: B* & Letter & ';')+ & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string StarLSemiPlusPlainImmediate = """
		Start : @V = xs: A* & (bs: B* & Letter & ';')+ & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string StarLSemiPlusPlainValueless = """
		Start = A* & (B* & Letter & ';')+ & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']

		parse Start
		""";

	public const string StarLSemiPlusBracesDeferred = """
		Start : @V = { xs: A* & (bs: B* & Letter & ';')+ } & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string StarLSemiPlusBracesImmediate = """
		Start : @V = { xs: A* & (bs: B* & Letter & ';')+ } & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string StarLSemiPlusBracesValueless = """
		Start = { A* & (B* & Letter & ';')+ } & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']

		parse Start
		""";

	public const string StarLSemiPlusLookDeferred = """
		Start : @V = xs: A* & ?=((B* & Letter & ';')+ & eof) & Rest & eof => @(Twin.C(0, (object)xs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string StarLSemiPlusLookImmediate = """
		Start : @V = xs: A* & ?=((B* & Letter & ';')+ & eof) & Rest & eof => @(Twin.C(0, (object)xs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string StarLSemiPlusLookValueless = """
		Start = A* & ?=((B* & Letter & ';')+ & eof) & Rest & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string StarLSemiStarPlainDeferred = """
		Start : @V = xs: A* & (bs: B* & Letter & ';')* & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string StarLSemiStarPlainImmediate = """
		Start : @V = xs: A* & (bs: B* & Letter & ';')* & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string StarLSemiStarPlainValueless = """
		Start = A* & (B* & Letter & ';')* & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']

		parse Start
		""";

	public const string StarLSemiStarBracesDeferred = """
		Start : @V = { xs: A* & (bs: B* & Letter & ';')* } & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string StarLSemiStarBracesImmediate = """
		Start : @V = { xs: A* & (bs: B* & Letter & ';')* } & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string StarLSemiStarBracesValueless = """
		Start = { A* & (B* & Letter & ';')* } & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']

		parse Start
		""";

	public const string StarLSemiStarLookDeferred = """
		Start : @V = xs: A* & ?=((B* & Letter & ';')* & eof) & Rest & eof => @(Twin.C(0, (object)xs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string StarLSemiStarLookImmediate = """
		Start : @V = xs: A* & ?=((B* & Letter & ';')* & eof) & Rest & eof => @(Twin.C(0, (object)xs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string StarLSemiStarLookValueless = """
		Start = A* & ?=((B* & Letter & ';')* & eof) & Rest & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string StarLSemiTwoPlainDeferred = """
		Start : @V = xs: A* & (bs: B* & Letter & ';'){2} & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string StarLSemiTwoPlainImmediate = """
		Start : @V = xs: A* & (bs: B* & Letter & ';'){2} & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string StarLSemiTwoPlainValueless = """
		Start = A* & (B* & Letter & ';'){2} & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']

		parse Start
		""";

	public const string StarLSemiTwoBracesDeferred = """
		Start : @V = { xs: A* & (bs: B* & Letter & ';'){2} } & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string StarLSemiTwoBracesImmediate = """
		Start : @V = { xs: A* & (bs: B* & Letter & ';'){2} } & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string StarLSemiTwoBracesValueless = """
		Start = { A* & (B* & Letter & ';'){2} } & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']

		parse Start
		""";

	public const string StarLSemiTwoLookDeferred = """
		Start : @V = xs: A* & ?=((B* & Letter & ';'){2} & eof) & Rest & eof => @(Twin.C(0, (object)xs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string StarLSemiTwoLookImmediate = """
		Start : @V = xs: A* & ?=((B* & Letter & ';'){2} & eof) & Rest & eof => @(Twin.C(0, (object)xs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string StarLSemiTwoLookValueless = """
		Start = A* & ?=((B* & Letter & ';'){2} & eof) & Rest & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string StarLBareTwoPlainDeferred = """
		Start : @V = xs: A* & (bs: B* & Letter){2} & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string StarLBareTwoPlainImmediate = """
		Start : @V = xs: A* & (bs: B* & Letter){2} & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string StarLBareTwoPlainValueless = """
		Start = A* & (B* & Letter){2} & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']

		parse Start
		""";

	public const string StarLBareTwoBracesDeferred = """
		Start : @V = { xs: A* & (bs: B* & Letter){2} } & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string StarLBareTwoBracesImmediate = """
		Start : @V = { xs: A* & (bs: B* & Letter){2} } & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string StarLBareTwoBracesValueless = """
		Start = { A* & (B* & Letter){2} } & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']

		parse Start
		""";

	public const string StarLBareTwoLookDeferred = """
		Start : @V = xs: A* & ?=((B* & Letter){2} & eof) & Rest & eof => @(Twin.C(0, (object)xs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string StarLBareTwoLookImmediate = """
		Start : @V = xs: A* & ?=((B* & Letter){2} & eof) & Rest & eof => @(Twin.C(0, (object)xs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string StarLBareTwoLookValueless = """
		Start = A* & ?=((B* & Letter){2} & eof) & Rest & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string StarLLSemiPlusPlainDeferred = """
		Start : @V = xs: A* & (bs: B* & Letter & Letter & ';')+ & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string StarLLSemiPlusPlainImmediate = """
		Start : @V = xs: A* & (bs: B* & Letter & Letter & ';')+ & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string StarLLSemiPlusPlainValueless = """
		Start = A* & (B* & Letter & Letter & ';')+ & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']

		parse Start
		""";

	public const string StarLLSemiPlusBracesDeferred = """
		Start : @V = { xs: A* & (bs: B* & Letter & Letter & ';')+ } & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string StarLLSemiPlusBracesImmediate = """
		Start : @V = { xs: A* & (bs: B* & Letter & Letter & ';')+ } & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string StarLLSemiPlusBracesValueless = """
		Start = { A* & (B* & Letter & Letter & ';')+ } & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']

		parse Start
		""";

	public const string StarLLSemiPlusLookDeferred = """
		Start : @V = xs: A* & ?=((B* & Letter & Letter & ';')+ & eof) & Rest & eof => @(Twin.C(0, (object)xs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string StarLLSemiPlusLookImmediate = """
		Start : @V = xs: A* & ?=((B* & Letter & Letter & ';')+ & eof) & Rest & eof => @(Twin.C(0, (object)xs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string StarLLSemiPlusLookValueless = """
		Start = A* & ?=((B* & Letter & Letter & ';')+ & eof) & Rest & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string StarLLSemiStarPlainDeferred = """
		Start : @V = xs: A* & (bs: B* & Letter & Letter & ';')* & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string StarLLSemiStarPlainImmediate = """
		Start : @V = xs: A* & (bs: B* & Letter & Letter & ';')* & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string StarLLSemiStarPlainValueless = """
		Start = A* & (B* & Letter & Letter & ';')* & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']

		parse Start
		""";

	public const string StarLLSemiStarBracesDeferred = """
		Start : @V = { xs: A* & (bs: B* & Letter & Letter & ';')* } & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string StarLLSemiStarBracesImmediate = """
		Start : @V = { xs: A* & (bs: B* & Letter & Letter & ';')* } & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string StarLLSemiStarBracesValueless = """
		Start = { A* & (B* & Letter & Letter & ';')* } & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']

		parse Start
		""";

	public const string StarLLSemiStarLookDeferred = """
		Start : @V = xs: A* & ?=((B* & Letter & Letter & ';')* & eof) & Rest & eof => @(Twin.C(0, (object)xs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string StarLLSemiStarLookImmediate = """
		Start : @V = xs: A* & ?=((B* & Letter & Letter & ';')* & eof) & Rest & eof => @(Twin.C(0, (object)xs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string StarLLSemiStarLookValueless = """
		Start = A* & ?=((B* & Letter & Letter & ';')* & eof) & Rest & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string StarLLSemiTwoPlainDeferred = """
		Start : @V = xs: A* & (bs: B* & Letter & Letter & ';'){2} & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string StarLLSemiTwoPlainImmediate = """
		Start : @V = xs: A* & (bs: B* & Letter & Letter & ';'){2} & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string StarLLSemiTwoPlainValueless = """
		Start = A* & (B* & Letter & Letter & ';'){2} & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']

		parse Start
		""";

	public const string StarLLSemiTwoBracesDeferred = """
		Start : @V = { xs: A* & (bs: B* & Letter & Letter & ';'){2} } & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string StarLLSemiTwoBracesImmediate = """
		Start : @V = { xs: A* & (bs: B* & Letter & Letter & ';'){2} } & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string StarLLSemiTwoBracesValueless = """
		Start = { A* & (B* & Letter & Letter & ';'){2} } & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']

		parse Start
		""";

	public const string StarLLSemiTwoLookDeferred = """
		Start : @V = xs: A* & ?=((B* & Letter & Letter & ';'){2} & eof) & Rest & eof => @(Twin.C(0, (object)xs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string StarLLSemiTwoLookImmediate = """
		Start : @V = xs: A* & ?=((B* & Letter & Letter & ';'){2} & eof) & Rest & eof => @(Twin.C(0, (object)xs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string StarLLSemiTwoLookValueless = """
		Start = A* & ?=((B* & Letter & Letter & ';'){2} & eof) & Rest & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string StarLLBareTwoPlainDeferred = """
		Start : @V = xs: A* & (bs: B* & Letter & Letter){2} & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string StarLLBareTwoPlainImmediate = """
		Start : @V = xs: A* & (bs: B* & Letter & Letter){2} & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string StarLLBareTwoPlainValueless = """
		Start = A* & (B* & Letter & Letter){2} & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']

		parse Start
		""";

	public const string StarLLBareTwoBracesDeferred = """
		Start : @V = { xs: A* & (bs: B* & Letter & Letter){2} } & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string StarLLBareTwoBracesImmediate = """
		Start : @V = { xs: A* & (bs: B* & Letter & Letter){2} } & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string StarLLBareTwoBracesValueless = """
		Start = { A* & (B* & Letter & Letter){2} } & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']

		parse Start
		""";

	public const string StarLLBareTwoLookDeferred = """
		Start : @V = xs: A* & ?=((B* & Letter & Letter){2} & eof) & Rest & eof => @(Twin.C(0, (object)xs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string StarLLBareTwoLookImmediate = """
		Start : @V = xs: A* & ?=((B* & Letter & Letter){2} & eof) & Rest & eof => @(Twin.C(0, (object)xs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string StarLLBareTwoLookValueless = """
		Start = A* & ?=((B* & Letter & Letter){2} & eof) & Rest & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string PlusLSemiPlusPlainDeferred = """
		Start : @V = xs: A* & (bs: B+ & Letter & ';')+ & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string PlusLSemiPlusPlainImmediate = """
		Start : @V = xs: A* & (bs: B+ & Letter & ';')+ & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string PlusLSemiPlusPlainValueless = """
		Start = A* & (B+ & Letter & ';')+ & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']

		parse Start
		""";

	public const string PlusLSemiPlusBracesDeferred = """
		Start : @V = { xs: A* & (bs: B+ & Letter & ';')+ } & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string PlusLSemiPlusBracesImmediate = """
		Start : @V = { xs: A* & (bs: B+ & Letter & ';')+ } & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string PlusLSemiPlusBracesValueless = """
		Start = { A* & (B+ & Letter & ';')+ } & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']

		parse Start
		""";

	public const string PlusLSemiPlusLookDeferred = """
		Start : @V = xs: A* & ?=((B+ & Letter & ';')+ & eof) & Rest & eof => @(Twin.C(0, (object)xs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string PlusLSemiPlusLookImmediate = """
		Start : @V = xs: A* & ?=((B+ & Letter & ';')+ & eof) & Rest & eof => @(Twin.C(0, (object)xs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string PlusLSemiPlusLookValueless = """
		Start = A* & ?=((B+ & Letter & ';')+ & eof) & Rest & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string PlusLSemiStarPlainDeferred = """
		Start : @V = xs: A* & (bs: B+ & Letter & ';')* & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string PlusLSemiStarPlainImmediate = """
		Start : @V = xs: A* & (bs: B+ & Letter & ';')* & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string PlusLSemiStarPlainValueless = """
		Start = A* & (B+ & Letter & ';')* & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']

		parse Start
		""";

	public const string PlusLSemiStarBracesDeferred = """
		Start : @V = { xs: A* & (bs: B+ & Letter & ';')* } & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string PlusLSemiStarBracesImmediate = """
		Start : @V = { xs: A* & (bs: B+ & Letter & ';')* } & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string PlusLSemiStarBracesValueless = """
		Start = { A* & (B+ & Letter & ';')* } & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']

		parse Start
		""";

	public const string PlusLSemiStarLookDeferred = """
		Start : @V = xs: A* & ?=((B+ & Letter & ';')* & eof) & Rest & eof => @(Twin.C(0, (object)xs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string PlusLSemiStarLookImmediate = """
		Start : @V = xs: A* & ?=((B+ & Letter & ';')* & eof) & Rest & eof => @(Twin.C(0, (object)xs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string PlusLSemiStarLookValueless = """
		Start = A* & ?=((B+ & Letter & ';')* & eof) & Rest & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string PlusLSemiTwoPlainDeferred = """
		Start : @V = xs: A* & (bs: B+ & Letter & ';'){2} & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string PlusLSemiTwoPlainImmediate = """
		Start : @V = xs: A* & (bs: B+ & Letter & ';'){2} & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string PlusLSemiTwoPlainValueless = """
		Start = A* & (B+ & Letter & ';'){2} & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']

		parse Start
		""";

	public const string PlusLSemiTwoBracesDeferred = """
		Start : @V = { xs: A* & (bs: B+ & Letter & ';'){2} } & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string PlusLSemiTwoBracesImmediate = """
		Start : @V = { xs: A* & (bs: B+ & Letter & ';'){2} } & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string PlusLSemiTwoBracesValueless = """
		Start = { A* & (B+ & Letter & ';'){2} } & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']

		parse Start
		""";

	public const string PlusLSemiTwoLookDeferred = """
		Start : @V = xs: A* & ?=((B+ & Letter & ';'){2} & eof) & Rest & eof => @(Twin.C(0, (object)xs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string PlusLSemiTwoLookImmediate = """
		Start : @V = xs: A* & ?=((B+ & Letter & ';'){2} & eof) & Rest & eof => @(Twin.C(0, (object)xs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string PlusLSemiTwoLookValueless = """
		Start = A* & ?=((B+ & Letter & ';'){2} & eof) & Rest & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string PlusLBareTwoPlainDeferred = """
		Start : @V = xs: A* & (bs: B+ & Letter){2} & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string PlusLBareTwoPlainImmediate = """
		Start : @V = xs: A* & (bs: B+ & Letter){2} & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string PlusLBareTwoPlainValueless = """
		Start = A* & (B+ & Letter){2} & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']

		parse Start
		""";

	public const string PlusLBareTwoBracesDeferred = """
		Start : @V = { xs: A* & (bs: B+ & Letter){2} } & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string PlusLBareTwoBracesImmediate = """
		Start : @V = { xs: A* & (bs: B+ & Letter){2} } & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string PlusLBareTwoBracesValueless = """
		Start = { A* & (B+ & Letter){2} } & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']

		parse Start
		""";

	public const string PlusLBareTwoLookDeferred = """
		Start : @V = xs: A* & ?=((B+ & Letter){2} & eof) & Rest & eof => @(Twin.C(0, (object)xs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string PlusLBareTwoLookImmediate = """
		Start : @V = xs: A* & ?=((B+ & Letter){2} & eof) & Rest & eof => @(Twin.C(0, (object)xs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string PlusLBareTwoLookValueless = """
		Start = A* & ?=((B+ & Letter){2} & eof) & Rest & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string PlusLLSemiPlusPlainDeferred = """
		Start : @V = xs: A* & (bs: B+ & Letter & Letter & ';')+ & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string PlusLLSemiPlusPlainImmediate = """
		Start : @V = xs: A* & (bs: B+ & Letter & Letter & ';')+ & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string PlusLLSemiPlusPlainValueless = """
		Start = A* & (B+ & Letter & Letter & ';')+ & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']

		parse Start
		""";

	public const string PlusLLSemiPlusBracesDeferred = """
		Start : @V = { xs: A* & (bs: B+ & Letter & Letter & ';')+ } & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string PlusLLSemiPlusBracesImmediate = """
		Start : @V = { xs: A* & (bs: B+ & Letter & Letter & ';')+ } & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string PlusLLSemiPlusBracesValueless = """
		Start = { A* & (B+ & Letter & Letter & ';')+ } & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']

		parse Start
		""";

	public const string PlusLLSemiPlusLookDeferred = """
		Start : @V = xs: A* & ?=((B+ & Letter & Letter & ';')+ & eof) & Rest & eof => @(Twin.C(0, (object)xs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string PlusLLSemiPlusLookImmediate = """
		Start : @V = xs: A* & ?=((B+ & Letter & Letter & ';')+ & eof) & Rest & eof => @(Twin.C(0, (object)xs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string PlusLLSemiPlusLookValueless = """
		Start = A* & ?=((B+ & Letter & Letter & ';')+ & eof) & Rest & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string PlusLLSemiStarPlainDeferred = """
		Start : @V = xs: A* & (bs: B+ & Letter & Letter & ';')* & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string PlusLLSemiStarPlainImmediate = """
		Start : @V = xs: A* & (bs: B+ & Letter & Letter & ';')* & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string PlusLLSemiStarPlainValueless = """
		Start = A* & (B+ & Letter & Letter & ';')* & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']

		parse Start
		""";

	public const string PlusLLSemiStarBracesDeferred = """
		Start : @V = { xs: A* & (bs: B+ & Letter & Letter & ';')* } & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string PlusLLSemiStarBracesImmediate = """
		Start : @V = { xs: A* & (bs: B+ & Letter & Letter & ';')* } & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string PlusLLSemiStarBracesValueless = """
		Start = { A* & (B+ & Letter & Letter & ';')* } & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']

		parse Start
		""";

	public const string PlusLLSemiStarLookDeferred = """
		Start : @V = xs: A* & ?=((B+ & Letter & Letter & ';')* & eof) & Rest & eof => @(Twin.C(0, (object)xs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string PlusLLSemiStarLookImmediate = """
		Start : @V = xs: A* & ?=((B+ & Letter & Letter & ';')* & eof) & Rest & eof => @(Twin.C(0, (object)xs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string PlusLLSemiStarLookValueless = """
		Start = A* & ?=((B+ & Letter & Letter & ';')* & eof) & Rest & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string PlusLLSemiTwoPlainDeferred = """
		Start : @V = xs: A* & (bs: B+ & Letter & Letter & ';'){2} & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string PlusLLSemiTwoPlainImmediate = """
		Start : @V = xs: A* & (bs: B+ & Letter & Letter & ';'){2} & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string PlusLLSemiTwoPlainValueless = """
		Start = A* & (B+ & Letter & Letter & ';'){2} & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']

		parse Start
		""";

	public const string PlusLLSemiTwoBracesDeferred = """
		Start : @V = { xs: A* & (bs: B+ & Letter & Letter & ';'){2} } & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string PlusLLSemiTwoBracesImmediate = """
		Start : @V = { xs: A* & (bs: B+ & Letter & Letter & ';'){2} } & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string PlusLLSemiTwoBracesValueless = """
		Start = { A* & (B+ & Letter & Letter & ';'){2} } & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']

		parse Start
		""";

	public const string PlusLLSemiTwoLookDeferred = """
		Start : @V = xs: A* & ?=((B+ & Letter & Letter & ';'){2} & eof) & Rest & eof => @(Twin.C(0, (object)xs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string PlusLLSemiTwoLookImmediate = """
		Start : @V = xs: A* & ?=((B+ & Letter & Letter & ';'){2} & eof) & Rest & eof => @(Twin.C(0, (object)xs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string PlusLLSemiTwoLookValueless = """
		Start = A* & ?=((B+ & Letter & Letter & ';'){2} & eof) & Rest & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string PlusLLBareTwoPlainDeferred = """
		Start : @V = xs: A* & (bs: B+ & Letter & Letter){2} & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string PlusLLBareTwoPlainImmediate = """
		Start : @V = xs: A* & (bs: B+ & Letter & Letter){2} & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string PlusLLBareTwoPlainValueless = """
		Start = A* & (B+ & Letter & Letter){2} & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']

		parse Start
		""";

	public const string PlusLLBareTwoBracesDeferred = """
		Start : @V = { xs: A* & (bs: B+ & Letter & Letter){2} } & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string PlusLLBareTwoBracesImmediate = """
		Start : @V = { xs: A* & (bs: B+ & Letter & Letter){2} } & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string PlusLLBareTwoBracesValueless = """
		Start = { A* & (B+ & Letter & Letter){2} } & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']

		parse Start
		""";

	public const string PlusLLBareTwoLookDeferred = """
		Start : @V = xs: A* & ?=((B+ & Letter & Letter){2} & eof) & Rest & eof => @(Twin.C(0, (object)xs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string PlusLLBareTwoLookImmediate = """
		Start : @V = xs: A* & ?=((B+ & Letter & Letter){2} & eof) & Rest & eof => @(Twin.C(0, (object)xs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string PlusLLBareTwoLookValueless = """
		Start = A* & ?=((B+ & Letter & Letter){2} & eof) & Rest & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string OptLSemiPlusPlainDeferred = """
		Start : @V = xs: A* & (bs: B? & Letter & ';')+ & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string OptLSemiPlusPlainImmediate = """
		Start : @V = xs: A* & (bs: B? & Letter & ';')+ & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string OptLSemiPlusPlainValueless = """
		Start = A* & (B? & Letter & ';')+ & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']

		parse Start
		""";

	public const string OptLSemiPlusBracesDeferred = """
		Start : @V = { xs: A* & (bs: B? & Letter & ';')+ } & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string OptLSemiPlusBracesImmediate = """
		Start : @V = { xs: A* & (bs: B? & Letter & ';')+ } & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string OptLSemiPlusBracesValueless = """
		Start = { A* & (B? & Letter & ';')+ } & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']

		parse Start
		""";

	public const string OptLSemiPlusLookDeferred = """
		Start : @V = xs: A* & ?=((B? & Letter & ';')+ & eof) & Rest & eof => @(Twin.C(0, (object)xs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string OptLSemiPlusLookImmediate = """
		Start : @V = xs: A* & ?=((B? & Letter & ';')+ & eof) & Rest & eof => @(Twin.C(0, (object)xs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string OptLSemiPlusLookValueless = """
		Start = A* & ?=((B? & Letter & ';')+ & eof) & Rest & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string OptLSemiStarPlainDeferred = """
		Start : @V = xs: A* & (bs: B? & Letter & ';')* & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string OptLSemiStarPlainImmediate = """
		Start : @V = xs: A* & (bs: B? & Letter & ';')* & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string OptLSemiStarPlainValueless = """
		Start = A* & (B? & Letter & ';')* & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']

		parse Start
		""";

	public const string OptLSemiStarBracesDeferred = """
		Start : @V = { xs: A* & (bs: B? & Letter & ';')* } & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string OptLSemiStarBracesImmediate = """
		Start : @V = { xs: A* & (bs: B? & Letter & ';')* } & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string OptLSemiStarBracesValueless = """
		Start = { A* & (B? & Letter & ';')* } & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']

		parse Start
		""";

	public const string OptLSemiStarLookDeferred = """
		Start : @V = xs: A* & ?=((B? & Letter & ';')* & eof) & Rest & eof => @(Twin.C(0, (object)xs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string OptLSemiStarLookImmediate = """
		Start : @V = xs: A* & ?=((B? & Letter & ';')* & eof) & Rest & eof => @(Twin.C(0, (object)xs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string OptLSemiStarLookValueless = """
		Start = A* & ?=((B? & Letter & ';')* & eof) & Rest & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string OptLSemiTwoPlainDeferred = """
		Start : @V = xs: A* & (bs: B? & Letter & ';'){2} & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string OptLSemiTwoPlainImmediate = """
		Start : @V = xs: A* & (bs: B? & Letter & ';'){2} & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string OptLSemiTwoPlainValueless = """
		Start = A* & (B? & Letter & ';'){2} & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']

		parse Start
		""";

	public const string OptLSemiTwoBracesDeferred = """
		Start : @V = { xs: A* & (bs: B? & Letter & ';'){2} } & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string OptLSemiTwoBracesImmediate = """
		Start : @V = { xs: A* & (bs: B? & Letter & ';'){2} } & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string OptLSemiTwoBracesValueless = """
		Start = { A* & (B? & Letter & ';'){2} } & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']

		parse Start
		""";

	public const string OptLSemiTwoLookDeferred = """
		Start : @V = xs: A* & ?=((B? & Letter & ';'){2} & eof) & Rest & eof => @(Twin.C(0, (object)xs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string OptLSemiTwoLookImmediate = """
		Start : @V = xs: A* & ?=((B? & Letter & ';'){2} & eof) & Rest & eof => @(Twin.C(0, (object)xs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string OptLSemiTwoLookValueless = """
		Start = A* & ?=((B? & Letter & ';'){2} & eof) & Rest & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string OptLBareTwoPlainDeferred = """
		Start : @V = xs: A* & (bs: B? & Letter){2} & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string OptLBareTwoPlainImmediate = """
		Start : @V = xs: A* & (bs: B? & Letter){2} & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string OptLBareTwoPlainValueless = """
		Start = A* & (B? & Letter){2} & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']

		parse Start
		""";

	public const string OptLBareTwoBracesDeferred = """
		Start : @V = { xs: A* & (bs: B? & Letter){2} } & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string OptLBareTwoBracesImmediate = """
		Start : @V = { xs: A* & (bs: B? & Letter){2} } & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string OptLBareTwoBracesValueless = """
		Start = { A* & (B? & Letter){2} } & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']

		parse Start
		""";

	public const string OptLBareTwoLookDeferred = """
		Start : @V = xs: A* & ?=((B? & Letter){2} & eof) & Rest & eof => @(Twin.C(0, (object)xs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string OptLBareTwoLookImmediate = """
		Start : @V = xs: A* & ?=((B? & Letter){2} & eof) & Rest & eof => @(Twin.C(0, (object)xs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string OptLBareTwoLookValueless = """
		Start = A* & ?=((B? & Letter){2} & eof) & Rest & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string OptLLSemiPlusPlainDeferred = """
		Start : @V = xs: A* & (bs: B? & Letter & Letter & ';')+ & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string OptLLSemiPlusPlainImmediate = """
		Start : @V = xs: A* & (bs: B? & Letter & Letter & ';')+ & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string OptLLSemiPlusPlainValueless = """
		Start = A* & (B? & Letter & Letter & ';')+ & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']

		parse Start
		""";

	public const string OptLLSemiPlusBracesDeferred = """
		Start : @V = { xs: A* & (bs: B? & Letter & Letter & ';')+ } & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string OptLLSemiPlusBracesImmediate = """
		Start : @V = { xs: A* & (bs: B? & Letter & Letter & ';')+ } & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string OptLLSemiPlusBracesValueless = """
		Start = { A* & (B? & Letter & Letter & ';')+ } & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']

		parse Start
		""";

	public const string OptLLSemiPlusLookDeferred = """
		Start : @V = xs: A* & ?=((B? & Letter & Letter & ';')+ & eof) & Rest & eof => @(Twin.C(0, (object)xs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string OptLLSemiPlusLookImmediate = """
		Start : @V = xs: A* & ?=((B? & Letter & Letter & ';')+ & eof) & Rest & eof => @(Twin.C(0, (object)xs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string OptLLSemiPlusLookValueless = """
		Start = A* & ?=((B? & Letter & Letter & ';')+ & eof) & Rest & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string OptLLSemiStarPlainDeferred = """
		Start : @V = xs: A* & (bs: B? & Letter & Letter & ';')* & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string OptLLSemiStarPlainImmediate = """
		Start : @V = xs: A* & (bs: B? & Letter & Letter & ';')* & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string OptLLSemiStarPlainValueless = """
		Start = A* & (B? & Letter & Letter & ';')* & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']

		parse Start
		""";

	public const string OptLLSemiStarBracesDeferred = """
		Start : @V = { xs: A* & (bs: B? & Letter & Letter & ';')* } & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string OptLLSemiStarBracesImmediate = """
		Start : @V = { xs: A* & (bs: B? & Letter & Letter & ';')* } & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string OptLLSemiStarBracesValueless = """
		Start = { A* & (B? & Letter & Letter & ';')* } & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']

		parse Start
		""";

	public const string OptLLSemiStarLookDeferred = """
		Start : @V = xs: A* & ?=((B? & Letter & Letter & ';')* & eof) & Rest & eof => @(Twin.C(0, (object)xs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string OptLLSemiStarLookImmediate = """
		Start : @V = xs: A* & ?=((B? & Letter & Letter & ';')* & eof) & Rest & eof => @(Twin.C(0, (object)xs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string OptLLSemiStarLookValueless = """
		Start = A* & ?=((B? & Letter & Letter & ';')* & eof) & Rest & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string OptLLSemiTwoPlainDeferred = """
		Start : @V = xs: A* & (bs: B? & Letter & Letter & ';'){2} & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string OptLLSemiTwoPlainImmediate = """
		Start : @V = xs: A* & (bs: B? & Letter & Letter & ';'){2} & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string OptLLSemiTwoPlainValueless = """
		Start = A* & (B? & Letter & Letter & ';'){2} & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']

		parse Start
		""";

	public const string OptLLSemiTwoBracesDeferred = """
		Start : @V = { xs: A* & (bs: B? & Letter & Letter & ';'){2} } & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string OptLLSemiTwoBracesImmediate = """
		Start : @V = { xs: A* & (bs: B? & Letter & Letter & ';'){2} } & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string OptLLSemiTwoBracesValueless = """
		Start = { A* & (B? & Letter & Letter & ';'){2} } & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']

		parse Start
		""";

	public const string OptLLSemiTwoLookDeferred = """
		Start : @V = xs: A* & ?=((B? & Letter & Letter & ';'){2} & eof) & Rest & eof => @(Twin.C(0, (object)xs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string OptLLSemiTwoLookImmediate = """
		Start : @V = xs: A* & ?=((B? & Letter & Letter & ';'){2} & eof) & Rest & eof => @(Twin.C(0, (object)xs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string OptLLSemiTwoLookValueless = """
		Start = A* & ?=((B? & Letter & Letter & ';'){2} & eof) & Rest & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string OptLLBareTwoPlainDeferred = """
		Start : @V = xs: A* & (bs: B? & Letter & Letter){2} & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string OptLLBareTwoPlainImmediate = """
		Start : @V = xs: A* & (bs: B? & Letter & Letter){2} & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string OptLLBareTwoPlainValueless = """
		Start = A* & (B? & Letter & Letter){2} & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']

		parse Start
		""";

	public const string OptLLBareTwoBracesDeferred = """
		Start : @V = { xs: A* & (bs: B? & Letter & Letter){2} } & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string OptLLBareTwoBracesImmediate = """
		Start : @V = { xs: A* & (bs: B? & Letter & Letter){2} } & eof => @(Twin.C(0, xs, bs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']

		parse Start
		""";

	public const string OptLLBareTwoBracesValueless = """
		Start = { A* & (B? & Letter & Letter){2} } & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']

		parse Start
		""";

	public const string OptLLBareTwoLookDeferred = """
		Start : @V = xs: A* & ?=((B? & Letter & Letter){2} & eof) & Rest & eof => @(Twin.C(0, (object)xs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string OptLLBareTwoLookImmediate = """
		Start : @V = xs: A* & ?=((B? & Letter & Letter){2} & eof) & Rest & eof => @(Twin.C(0, (object)xs))
		A     : @V = t: '0' => @(Twin.C(1, t))
		B     : @W = t: Letter & ','? => @(Twin.D(2, t))
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	public const string OptLLBareTwoLookValueless = """
		Start = A* & ?=((B? & Letter & Letter){2} & eof) & Rest & eof
		A     = '0'
		B     = Letter & ','?
		Letter = ['a'..'z']
		Rest  = ['0' | 'a'..'z' | ',' | ';']*

		parse Start
		""";

	/// <summary>Every member: its name, its grammar, its generated parser.</summary>
	public static readonly (string Name, string Grammar, Type Parser)[] Members =
	[
		(nameof(StarLSemiPlusPlainDeferred), StarLSemiPlusPlainDeferred, typeof(StarLSemiPlusPlainDeferredParser)),
		(nameof(StarLSemiPlusPlainImmediate), StarLSemiPlusPlainImmediate, typeof(StarLSemiPlusPlainImmediateParser)),
		(nameof(StarLSemiPlusPlainValueless), StarLSemiPlusPlainValueless, typeof(StarLSemiPlusPlainValuelessParser)),
		(nameof(StarLSemiPlusBracesDeferred), StarLSemiPlusBracesDeferred, typeof(StarLSemiPlusBracesDeferredParser)),
		(nameof(StarLSemiPlusBracesImmediate), StarLSemiPlusBracesImmediate, typeof(StarLSemiPlusBracesImmediateParser)),
		(nameof(StarLSemiPlusBracesValueless), StarLSemiPlusBracesValueless, typeof(StarLSemiPlusBracesValuelessParser)),
		(nameof(StarLSemiPlusLookDeferred), StarLSemiPlusLookDeferred, typeof(StarLSemiPlusLookDeferredParser)),
		(nameof(StarLSemiPlusLookImmediate), StarLSemiPlusLookImmediate, typeof(StarLSemiPlusLookImmediateParser)),
		(nameof(StarLSemiPlusLookValueless), StarLSemiPlusLookValueless, typeof(StarLSemiPlusLookValuelessParser)),
		(nameof(StarLSemiStarPlainDeferred), StarLSemiStarPlainDeferred, typeof(StarLSemiStarPlainDeferredParser)),
		(nameof(StarLSemiStarPlainImmediate), StarLSemiStarPlainImmediate, typeof(StarLSemiStarPlainImmediateParser)),
		(nameof(StarLSemiStarPlainValueless), StarLSemiStarPlainValueless, typeof(StarLSemiStarPlainValuelessParser)),
		(nameof(StarLSemiStarBracesDeferred), StarLSemiStarBracesDeferred, typeof(StarLSemiStarBracesDeferredParser)),
		(nameof(StarLSemiStarBracesImmediate), StarLSemiStarBracesImmediate, typeof(StarLSemiStarBracesImmediateParser)),
		(nameof(StarLSemiStarBracesValueless), StarLSemiStarBracesValueless, typeof(StarLSemiStarBracesValuelessParser)),
		(nameof(StarLSemiStarLookDeferred), StarLSemiStarLookDeferred, typeof(StarLSemiStarLookDeferredParser)),
		(nameof(StarLSemiStarLookImmediate), StarLSemiStarLookImmediate, typeof(StarLSemiStarLookImmediateParser)),
		(nameof(StarLSemiStarLookValueless), StarLSemiStarLookValueless, typeof(StarLSemiStarLookValuelessParser)),
		(nameof(StarLSemiTwoPlainDeferred), StarLSemiTwoPlainDeferred, typeof(StarLSemiTwoPlainDeferredParser)),
		(nameof(StarLSemiTwoPlainImmediate), StarLSemiTwoPlainImmediate, typeof(StarLSemiTwoPlainImmediateParser)),
		(nameof(StarLSemiTwoPlainValueless), StarLSemiTwoPlainValueless, typeof(StarLSemiTwoPlainValuelessParser)),
		(nameof(StarLSemiTwoBracesDeferred), StarLSemiTwoBracesDeferred, typeof(StarLSemiTwoBracesDeferredParser)),
		(nameof(StarLSemiTwoBracesImmediate), StarLSemiTwoBracesImmediate, typeof(StarLSemiTwoBracesImmediateParser)),
		(nameof(StarLSemiTwoBracesValueless), StarLSemiTwoBracesValueless, typeof(StarLSemiTwoBracesValuelessParser)),
		(nameof(StarLSemiTwoLookDeferred), StarLSemiTwoLookDeferred, typeof(StarLSemiTwoLookDeferredParser)),
		(nameof(StarLSemiTwoLookImmediate), StarLSemiTwoLookImmediate, typeof(StarLSemiTwoLookImmediateParser)),
		(nameof(StarLSemiTwoLookValueless), StarLSemiTwoLookValueless, typeof(StarLSemiTwoLookValuelessParser)),
		(nameof(StarLBareTwoPlainDeferred), StarLBareTwoPlainDeferred, typeof(StarLBareTwoPlainDeferredParser)),
		(nameof(StarLBareTwoPlainImmediate), StarLBareTwoPlainImmediate, typeof(StarLBareTwoPlainImmediateParser)),
		(nameof(StarLBareTwoPlainValueless), StarLBareTwoPlainValueless, typeof(StarLBareTwoPlainValuelessParser)),
		(nameof(StarLBareTwoBracesDeferred), StarLBareTwoBracesDeferred, typeof(StarLBareTwoBracesDeferredParser)),
		(nameof(StarLBareTwoBracesImmediate), StarLBareTwoBracesImmediate, typeof(StarLBareTwoBracesImmediateParser)),
		(nameof(StarLBareTwoBracesValueless), StarLBareTwoBracesValueless, typeof(StarLBareTwoBracesValuelessParser)),
		(nameof(StarLBareTwoLookDeferred), StarLBareTwoLookDeferred, typeof(StarLBareTwoLookDeferredParser)),
		(nameof(StarLBareTwoLookImmediate), StarLBareTwoLookImmediate, typeof(StarLBareTwoLookImmediateParser)),
		(nameof(StarLBareTwoLookValueless), StarLBareTwoLookValueless, typeof(StarLBareTwoLookValuelessParser)),
		(nameof(StarLLSemiPlusPlainDeferred), StarLLSemiPlusPlainDeferred, typeof(StarLLSemiPlusPlainDeferredParser)),
		(nameof(StarLLSemiPlusPlainImmediate), StarLLSemiPlusPlainImmediate, typeof(StarLLSemiPlusPlainImmediateParser)),
		(nameof(StarLLSemiPlusPlainValueless), StarLLSemiPlusPlainValueless, typeof(StarLLSemiPlusPlainValuelessParser)),
		(nameof(StarLLSemiPlusBracesDeferred), StarLLSemiPlusBracesDeferred, typeof(StarLLSemiPlusBracesDeferredParser)),
		(nameof(StarLLSemiPlusBracesImmediate), StarLLSemiPlusBracesImmediate, typeof(StarLLSemiPlusBracesImmediateParser)),
		(nameof(StarLLSemiPlusBracesValueless), StarLLSemiPlusBracesValueless, typeof(StarLLSemiPlusBracesValuelessParser)),
		(nameof(StarLLSemiPlusLookDeferred), StarLLSemiPlusLookDeferred, typeof(StarLLSemiPlusLookDeferredParser)),
		(nameof(StarLLSemiPlusLookImmediate), StarLLSemiPlusLookImmediate, typeof(StarLLSemiPlusLookImmediateParser)),
		(nameof(StarLLSemiPlusLookValueless), StarLLSemiPlusLookValueless, typeof(StarLLSemiPlusLookValuelessParser)),
		(nameof(StarLLSemiStarPlainDeferred), StarLLSemiStarPlainDeferred, typeof(StarLLSemiStarPlainDeferredParser)),
		(nameof(StarLLSemiStarPlainImmediate), StarLLSemiStarPlainImmediate, typeof(StarLLSemiStarPlainImmediateParser)),
		(nameof(StarLLSemiStarPlainValueless), StarLLSemiStarPlainValueless, typeof(StarLLSemiStarPlainValuelessParser)),
		(nameof(StarLLSemiStarBracesDeferred), StarLLSemiStarBracesDeferred, typeof(StarLLSemiStarBracesDeferredParser)),
		(nameof(StarLLSemiStarBracesImmediate), StarLLSemiStarBracesImmediate, typeof(StarLLSemiStarBracesImmediateParser)),
		(nameof(StarLLSemiStarBracesValueless), StarLLSemiStarBracesValueless, typeof(StarLLSemiStarBracesValuelessParser)),
		(nameof(StarLLSemiStarLookDeferred), StarLLSemiStarLookDeferred, typeof(StarLLSemiStarLookDeferredParser)),
		(nameof(StarLLSemiStarLookImmediate), StarLLSemiStarLookImmediate, typeof(StarLLSemiStarLookImmediateParser)),
		(nameof(StarLLSemiStarLookValueless), StarLLSemiStarLookValueless, typeof(StarLLSemiStarLookValuelessParser)),
		(nameof(StarLLSemiTwoPlainDeferred), StarLLSemiTwoPlainDeferred, typeof(StarLLSemiTwoPlainDeferredParser)),
		(nameof(StarLLSemiTwoPlainImmediate), StarLLSemiTwoPlainImmediate, typeof(StarLLSemiTwoPlainImmediateParser)),
		(nameof(StarLLSemiTwoPlainValueless), StarLLSemiTwoPlainValueless, typeof(StarLLSemiTwoPlainValuelessParser)),
		(nameof(StarLLSemiTwoBracesDeferred), StarLLSemiTwoBracesDeferred, typeof(StarLLSemiTwoBracesDeferredParser)),
		(nameof(StarLLSemiTwoBracesImmediate), StarLLSemiTwoBracesImmediate, typeof(StarLLSemiTwoBracesImmediateParser)),
		(nameof(StarLLSemiTwoBracesValueless), StarLLSemiTwoBracesValueless, typeof(StarLLSemiTwoBracesValuelessParser)),
		(nameof(StarLLSemiTwoLookDeferred), StarLLSemiTwoLookDeferred, typeof(StarLLSemiTwoLookDeferredParser)),
		(nameof(StarLLSemiTwoLookImmediate), StarLLSemiTwoLookImmediate, typeof(StarLLSemiTwoLookImmediateParser)),
		(nameof(StarLLSemiTwoLookValueless), StarLLSemiTwoLookValueless, typeof(StarLLSemiTwoLookValuelessParser)),
		(nameof(StarLLBareTwoPlainDeferred), StarLLBareTwoPlainDeferred, typeof(StarLLBareTwoPlainDeferredParser)),
		(nameof(StarLLBareTwoPlainImmediate), StarLLBareTwoPlainImmediate, typeof(StarLLBareTwoPlainImmediateParser)),
		(nameof(StarLLBareTwoPlainValueless), StarLLBareTwoPlainValueless, typeof(StarLLBareTwoPlainValuelessParser)),
		(nameof(StarLLBareTwoBracesDeferred), StarLLBareTwoBracesDeferred, typeof(StarLLBareTwoBracesDeferredParser)),
		(nameof(StarLLBareTwoBracesImmediate), StarLLBareTwoBracesImmediate, typeof(StarLLBareTwoBracesImmediateParser)),
		(nameof(StarLLBareTwoBracesValueless), StarLLBareTwoBracesValueless, typeof(StarLLBareTwoBracesValuelessParser)),
		(nameof(StarLLBareTwoLookDeferred), StarLLBareTwoLookDeferred, typeof(StarLLBareTwoLookDeferredParser)),
		(nameof(StarLLBareTwoLookImmediate), StarLLBareTwoLookImmediate, typeof(StarLLBareTwoLookImmediateParser)),
		(nameof(StarLLBareTwoLookValueless), StarLLBareTwoLookValueless, typeof(StarLLBareTwoLookValuelessParser)),
		(nameof(PlusLSemiPlusPlainDeferred), PlusLSemiPlusPlainDeferred, typeof(PlusLSemiPlusPlainDeferredParser)),
		(nameof(PlusLSemiPlusPlainImmediate), PlusLSemiPlusPlainImmediate, typeof(PlusLSemiPlusPlainImmediateParser)),
		(nameof(PlusLSemiPlusPlainValueless), PlusLSemiPlusPlainValueless, typeof(PlusLSemiPlusPlainValuelessParser)),
		(nameof(PlusLSemiPlusBracesDeferred), PlusLSemiPlusBracesDeferred, typeof(PlusLSemiPlusBracesDeferredParser)),
		(nameof(PlusLSemiPlusBracesImmediate), PlusLSemiPlusBracesImmediate, typeof(PlusLSemiPlusBracesImmediateParser)),
		(nameof(PlusLSemiPlusBracesValueless), PlusLSemiPlusBracesValueless, typeof(PlusLSemiPlusBracesValuelessParser)),
		(nameof(PlusLSemiPlusLookDeferred), PlusLSemiPlusLookDeferred, typeof(PlusLSemiPlusLookDeferredParser)),
		(nameof(PlusLSemiPlusLookImmediate), PlusLSemiPlusLookImmediate, typeof(PlusLSemiPlusLookImmediateParser)),
		(nameof(PlusLSemiPlusLookValueless), PlusLSemiPlusLookValueless, typeof(PlusLSemiPlusLookValuelessParser)),
		(nameof(PlusLSemiStarPlainDeferred), PlusLSemiStarPlainDeferred, typeof(PlusLSemiStarPlainDeferredParser)),
		(nameof(PlusLSemiStarPlainImmediate), PlusLSemiStarPlainImmediate, typeof(PlusLSemiStarPlainImmediateParser)),
		(nameof(PlusLSemiStarPlainValueless), PlusLSemiStarPlainValueless, typeof(PlusLSemiStarPlainValuelessParser)),
		(nameof(PlusLSemiStarBracesDeferred), PlusLSemiStarBracesDeferred, typeof(PlusLSemiStarBracesDeferredParser)),
		(nameof(PlusLSemiStarBracesImmediate), PlusLSemiStarBracesImmediate, typeof(PlusLSemiStarBracesImmediateParser)),
		(nameof(PlusLSemiStarBracesValueless), PlusLSemiStarBracesValueless, typeof(PlusLSemiStarBracesValuelessParser)),
		(nameof(PlusLSemiStarLookDeferred), PlusLSemiStarLookDeferred, typeof(PlusLSemiStarLookDeferredParser)),
		(nameof(PlusLSemiStarLookImmediate), PlusLSemiStarLookImmediate, typeof(PlusLSemiStarLookImmediateParser)),
		(nameof(PlusLSemiStarLookValueless), PlusLSemiStarLookValueless, typeof(PlusLSemiStarLookValuelessParser)),
		(nameof(PlusLSemiTwoPlainDeferred), PlusLSemiTwoPlainDeferred, typeof(PlusLSemiTwoPlainDeferredParser)),
		(nameof(PlusLSemiTwoPlainImmediate), PlusLSemiTwoPlainImmediate, typeof(PlusLSemiTwoPlainImmediateParser)),
		(nameof(PlusLSemiTwoPlainValueless), PlusLSemiTwoPlainValueless, typeof(PlusLSemiTwoPlainValuelessParser)),
		(nameof(PlusLSemiTwoBracesDeferred), PlusLSemiTwoBracesDeferred, typeof(PlusLSemiTwoBracesDeferredParser)),
		(nameof(PlusLSemiTwoBracesImmediate), PlusLSemiTwoBracesImmediate, typeof(PlusLSemiTwoBracesImmediateParser)),
		(nameof(PlusLSemiTwoBracesValueless), PlusLSemiTwoBracesValueless, typeof(PlusLSemiTwoBracesValuelessParser)),
		(nameof(PlusLSemiTwoLookDeferred), PlusLSemiTwoLookDeferred, typeof(PlusLSemiTwoLookDeferredParser)),
		(nameof(PlusLSemiTwoLookImmediate), PlusLSemiTwoLookImmediate, typeof(PlusLSemiTwoLookImmediateParser)),
		(nameof(PlusLSemiTwoLookValueless), PlusLSemiTwoLookValueless, typeof(PlusLSemiTwoLookValuelessParser)),
		(nameof(PlusLBareTwoPlainDeferred), PlusLBareTwoPlainDeferred, typeof(PlusLBareTwoPlainDeferredParser)),
		(nameof(PlusLBareTwoPlainImmediate), PlusLBareTwoPlainImmediate, typeof(PlusLBareTwoPlainImmediateParser)),
		(nameof(PlusLBareTwoPlainValueless), PlusLBareTwoPlainValueless, typeof(PlusLBareTwoPlainValuelessParser)),
		(nameof(PlusLBareTwoBracesDeferred), PlusLBareTwoBracesDeferred, typeof(PlusLBareTwoBracesDeferredParser)),
		(nameof(PlusLBareTwoBracesImmediate), PlusLBareTwoBracesImmediate, typeof(PlusLBareTwoBracesImmediateParser)),
		(nameof(PlusLBareTwoBracesValueless), PlusLBareTwoBracesValueless, typeof(PlusLBareTwoBracesValuelessParser)),
		(nameof(PlusLBareTwoLookDeferred), PlusLBareTwoLookDeferred, typeof(PlusLBareTwoLookDeferredParser)),
		(nameof(PlusLBareTwoLookImmediate), PlusLBareTwoLookImmediate, typeof(PlusLBareTwoLookImmediateParser)),
		(nameof(PlusLBareTwoLookValueless), PlusLBareTwoLookValueless, typeof(PlusLBareTwoLookValuelessParser)),
		(nameof(PlusLLSemiPlusPlainDeferred), PlusLLSemiPlusPlainDeferred, typeof(PlusLLSemiPlusPlainDeferredParser)),
		(nameof(PlusLLSemiPlusPlainImmediate), PlusLLSemiPlusPlainImmediate, typeof(PlusLLSemiPlusPlainImmediateParser)),
		(nameof(PlusLLSemiPlusPlainValueless), PlusLLSemiPlusPlainValueless, typeof(PlusLLSemiPlusPlainValuelessParser)),
		(nameof(PlusLLSemiPlusBracesDeferred), PlusLLSemiPlusBracesDeferred, typeof(PlusLLSemiPlusBracesDeferredParser)),
		(nameof(PlusLLSemiPlusBracesImmediate), PlusLLSemiPlusBracesImmediate, typeof(PlusLLSemiPlusBracesImmediateParser)),
		(nameof(PlusLLSemiPlusBracesValueless), PlusLLSemiPlusBracesValueless, typeof(PlusLLSemiPlusBracesValuelessParser)),
		(nameof(PlusLLSemiPlusLookDeferred), PlusLLSemiPlusLookDeferred, typeof(PlusLLSemiPlusLookDeferredParser)),
		(nameof(PlusLLSemiPlusLookImmediate), PlusLLSemiPlusLookImmediate, typeof(PlusLLSemiPlusLookImmediateParser)),
		(nameof(PlusLLSemiPlusLookValueless), PlusLLSemiPlusLookValueless, typeof(PlusLLSemiPlusLookValuelessParser)),
		(nameof(PlusLLSemiStarPlainDeferred), PlusLLSemiStarPlainDeferred, typeof(PlusLLSemiStarPlainDeferredParser)),
		(nameof(PlusLLSemiStarPlainImmediate), PlusLLSemiStarPlainImmediate, typeof(PlusLLSemiStarPlainImmediateParser)),
		(nameof(PlusLLSemiStarPlainValueless), PlusLLSemiStarPlainValueless, typeof(PlusLLSemiStarPlainValuelessParser)),
		(nameof(PlusLLSemiStarBracesDeferred), PlusLLSemiStarBracesDeferred, typeof(PlusLLSemiStarBracesDeferredParser)),
		(nameof(PlusLLSemiStarBracesImmediate), PlusLLSemiStarBracesImmediate, typeof(PlusLLSemiStarBracesImmediateParser)),
		(nameof(PlusLLSemiStarBracesValueless), PlusLLSemiStarBracesValueless, typeof(PlusLLSemiStarBracesValuelessParser)),
		(nameof(PlusLLSemiStarLookDeferred), PlusLLSemiStarLookDeferred, typeof(PlusLLSemiStarLookDeferredParser)),
		(nameof(PlusLLSemiStarLookImmediate), PlusLLSemiStarLookImmediate, typeof(PlusLLSemiStarLookImmediateParser)),
		(nameof(PlusLLSemiStarLookValueless), PlusLLSemiStarLookValueless, typeof(PlusLLSemiStarLookValuelessParser)),
		(nameof(PlusLLSemiTwoPlainDeferred), PlusLLSemiTwoPlainDeferred, typeof(PlusLLSemiTwoPlainDeferredParser)),
		(nameof(PlusLLSemiTwoPlainImmediate), PlusLLSemiTwoPlainImmediate, typeof(PlusLLSemiTwoPlainImmediateParser)),
		(nameof(PlusLLSemiTwoPlainValueless), PlusLLSemiTwoPlainValueless, typeof(PlusLLSemiTwoPlainValuelessParser)),
		(nameof(PlusLLSemiTwoBracesDeferred), PlusLLSemiTwoBracesDeferred, typeof(PlusLLSemiTwoBracesDeferredParser)),
		(nameof(PlusLLSemiTwoBracesImmediate), PlusLLSemiTwoBracesImmediate, typeof(PlusLLSemiTwoBracesImmediateParser)),
		(nameof(PlusLLSemiTwoBracesValueless), PlusLLSemiTwoBracesValueless, typeof(PlusLLSemiTwoBracesValuelessParser)),
		(nameof(PlusLLSemiTwoLookDeferred), PlusLLSemiTwoLookDeferred, typeof(PlusLLSemiTwoLookDeferredParser)),
		(nameof(PlusLLSemiTwoLookImmediate), PlusLLSemiTwoLookImmediate, typeof(PlusLLSemiTwoLookImmediateParser)),
		(nameof(PlusLLSemiTwoLookValueless), PlusLLSemiTwoLookValueless, typeof(PlusLLSemiTwoLookValuelessParser)),
		(nameof(PlusLLBareTwoPlainDeferred), PlusLLBareTwoPlainDeferred, typeof(PlusLLBareTwoPlainDeferredParser)),
		(nameof(PlusLLBareTwoPlainImmediate), PlusLLBareTwoPlainImmediate, typeof(PlusLLBareTwoPlainImmediateParser)),
		(nameof(PlusLLBareTwoPlainValueless), PlusLLBareTwoPlainValueless, typeof(PlusLLBareTwoPlainValuelessParser)),
		(nameof(PlusLLBareTwoBracesDeferred), PlusLLBareTwoBracesDeferred, typeof(PlusLLBareTwoBracesDeferredParser)),
		(nameof(PlusLLBareTwoBracesImmediate), PlusLLBareTwoBracesImmediate, typeof(PlusLLBareTwoBracesImmediateParser)),
		(nameof(PlusLLBareTwoBracesValueless), PlusLLBareTwoBracesValueless, typeof(PlusLLBareTwoBracesValuelessParser)),
		(nameof(PlusLLBareTwoLookDeferred), PlusLLBareTwoLookDeferred, typeof(PlusLLBareTwoLookDeferredParser)),
		(nameof(PlusLLBareTwoLookImmediate), PlusLLBareTwoLookImmediate, typeof(PlusLLBareTwoLookImmediateParser)),
		(nameof(PlusLLBareTwoLookValueless), PlusLLBareTwoLookValueless, typeof(PlusLLBareTwoLookValuelessParser)),
		(nameof(OptLSemiPlusPlainDeferred), OptLSemiPlusPlainDeferred, typeof(OptLSemiPlusPlainDeferredParser)),
		(nameof(OptLSemiPlusPlainImmediate), OptLSemiPlusPlainImmediate, typeof(OptLSemiPlusPlainImmediateParser)),
		(nameof(OptLSemiPlusPlainValueless), OptLSemiPlusPlainValueless, typeof(OptLSemiPlusPlainValuelessParser)),
		(nameof(OptLSemiPlusBracesDeferred), OptLSemiPlusBracesDeferred, typeof(OptLSemiPlusBracesDeferredParser)),
		(nameof(OptLSemiPlusBracesImmediate), OptLSemiPlusBracesImmediate, typeof(OptLSemiPlusBracesImmediateParser)),
		(nameof(OptLSemiPlusBracesValueless), OptLSemiPlusBracesValueless, typeof(OptLSemiPlusBracesValuelessParser)),
		(nameof(OptLSemiPlusLookDeferred), OptLSemiPlusLookDeferred, typeof(OptLSemiPlusLookDeferredParser)),
		(nameof(OptLSemiPlusLookImmediate), OptLSemiPlusLookImmediate, typeof(OptLSemiPlusLookImmediateParser)),
		(nameof(OptLSemiPlusLookValueless), OptLSemiPlusLookValueless, typeof(OptLSemiPlusLookValuelessParser)),
		(nameof(OptLSemiStarPlainDeferred), OptLSemiStarPlainDeferred, typeof(OptLSemiStarPlainDeferredParser)),
		(nameof(OptLSemiStarPlainImmediate), OptLSemiStarPlainImmediate, typeof(OptLSemiStarPlainImmediateParser)),
		(nameof(OptLSemiStarPlainValueless), OptLSemiStarPlainValueless, typeof(OptLSemiStarPlainValuelessParser)),
		(nameof(OptLSemiStarBracesDeferred), OptLSemiStarBracesDeferred, typeof(OptLSemiStarBracesDeferredParser)),
		(nameof(OptLSemiStarBracesImmediate), OptLSemiStarBracesImmediate, typeof(OptLSemiStarBracesImmediateParser)),
		(nameof(OptLSemiStarBracesValueless), OptLSemiStarBracesValueless, typeof(OptLSemiStarBracesValuelessParser)),
		(nameof(OptLSemiStarLookDeferred), OptLSemiStarLookDeferred, typeof(OptLSemiStarLookDeferredParser)),
		(nameof(OptLSemiStarLookImmediate), OptLSemiStarLookImmediate, typeof(OptLSemiStarLookImmediateParser)),
		(nameof(OptLSemiStarLookValueless), OptLSemiStarLookValueless, typeof(OptLSemiStarLookValuelessParser)),
		(nameof(OptLSemiTwoPlainDeferred), OptLSemiTwoPlainDeferred, typeof(OptLSemiTwoPlainDeferredParser)),
		(nameof(OptLSemiTwoPlainImmediate), OptLSemiTwoPlainImmediate, typeof(OptLSemiTwoPlainImmediateParser)),
		(nameof(OptLSemiTwoPlainValueless), OptLSemiTwoPlainValueless, typeof(OptLSemiTwoPlainValuelessParser)),
		(nameof(OptLSemiTwoBracesDeferred), OptLSemiTwoBracesDeferred, typeof(OptLSemiTwoBracesDeferredParser)),
		(nameof(OptLSemiTwoBracesImmediate), OptLSemiTwoBracesImmediate, typeof(OptLSemiTwoBracesImmediateParser)),
		(nameof(OptLSemiTwoBracesValueless), OptLSemiTwoBracesValueless, typeof(OptLSemiTwoBracesValuelessParser)),
		(nameof(OptLSemiTwoLookDeferred), OptLSemiTwoLookDeferred, typeof(OptLSemiTwoLookDeferredParser)),
		(nameof(OptLSemiTwoLookImmediate), OptLSemiTwoLookImmediate, typeof(OptLSemiTwoLookImmediateParser)),
		(nameof(OptLSemiTwoLookValueless), OptLSemiTwoLookValueless, typeof(OptLSemiTwoLookValuelessParser)),
		(nameof(OptLBareTwoPlainDeferred), OptLBareTwoPlainDeferred, typeof(OptLBareTwoPlainDeferredParser)),
		(nameof(OptLBareTwoPlainImmediate), OptLBareTwoPlainImmediate, typeof(OptLBareTwoPlainImmediateParser)),
		(nameof(OptLBareTwoPlainValueless), OptLBareTwoPlainValueless, typeof(OptLBareTwoPlainValuelessParser)),
		(nameof(OptLBareTwoBracesDeferred), OptLBareTwoBracesDeferred, typeof(OptLBareTwoBracesDeferredParser)),
		(nameof(OptLBareTwoBracesImmediate), OptLBareTwoBracesImmediate, typeof(OptLBareTwoBracesImmediateParser)),
		(nameof(OptLBareTwoBracesValueless), OptLBareTwoBracesValueless, typeof(OptLBareTwoBracesValuelessParser)),
		(nameof(OptLBareTwoLookDeferred), OptLBareTwoLookDeferred, typeof(OptLBareTwoLookDeferredParser)),
		(nameof(OptLBareTwoLookImmediate), OptLBareTwoLookImmediate, typeof(OptLBareTwoLookImmediateParser)),
		(nameof(OptLBareTwoLookValueless), OptLBareTwoLookValueless, typeof(OptLBareTwoLookValuelessParser)),
		(nameof(OptLLSemiPlusPlainDeferred), OptLLSemiPlusPlainDeferred, typeof(OptLLSemiPlusPlainDeferredParser)),
		(nameof(OptLLSemiPlusPlainImmediate), OptLLSemiPlusPlainImmediate, typeof(OptLLSemiPlusPlainImmediateParser)),
		(nameof(OptLLSemiPlusPlainValueless), OptLLSemiPlusPlainValueless, typeof(OptLLSemiPlusPlainValuelessParser)),
		(nameof(OptLLSemiPlusBracesDeferred), OptLLSemiPlusBracesDeferred, typeof(OptLLSemiPlusBracesDeferredParser)),
		(nameof(OptLLSemiPlusBracesImmediate), OptLLSemiPlusBracesImmediate, typeof(OptLLSemiPlusBracesImmediateParser)),
		(nameof(OptLLSemiPlusBracesValueless), OptLLSemiPlusBracesValueless, typeof(OptLLSemiPlusBracesValuelessParser)),
		(nameof(OptLLSemiPlusLookDeferred), OptLLSemiPlusLookDeferred, typeof(OptLLSemiPlusLookDeferredParser)),
		(nameof(OptLLSemiPlusLookImmediate), OptLLSemiPlusLookImmediate, typeof(OptLLSemiPlusLookImmediateParser)),
		(nameof(OptLLSemiPlusLookValueless), OptLLSemiPlusLookValueless, typeof(OptLLSemiPlusLookValuelessParser)),
		(nameof(OptLLSemiStarPlainDeferred), OptLLSemiStarPlainDeferred, typeof(OptLLSemiStarPlainDeferredParser)),
		(nameof(OptLLSemiStarPlainImmediate), OptLLSemiStarPlainImmediate, typeof(OptLLSemiStarPlainImmediateParser)),
		(nameof(OptLLSemiStarPlainValueless), OptLLSemiStarPlainValueless, typeof(OptLLSemiStarPlainValuelessParser)),
		(nameof(OptLLSemiStarBracesDeferred), OptLLSemiStarBracesDeferred, typeof(OptLLSemiStarBracesDeferredParser)),
		(nameof(OptLLSemiStarBracesImmediate), OptLLSemiStarBracesImmediate, typeof(OptLLSemiStarBracesImmediateParser)),
		(nameof(OptLLSemiStarBracesValueless), OptLLSemiStarBracesValueless, typeof(OptLLSemiStarBracesValuelessParser)),
		(nameof(OptLLSemiStarLookDeferred), OptLLSemiStarLookDeferred, typeof(OptLLSemiStarLookDeferredParser)),
		(nameof(OptLLSemiStarLookImmediate), OptLLSemiStarLookImmediate, typeof(OptLLSemiStarLookImmediateParser)),
		(nameof(OptLLSemiStarLookValueless), OptLLSemiStarLookValueless, typeof(OptLLSemiStarLookValuelessParser)),
		(nameof(OptLLSemiTwoPlainDeferred), OptLLSemiTwoPlainDeferred, typeof(OptLLSemiTwoPlainDeferredParser)),
		(nameof(OptLLSemiTwoPlainImmediate), OptLLSemiTwoPlainImmediate, typeof(OptLLSemiTwoPlainImmediateParser)),
		(nameof(OptLLSemiTwoPlainValueless), OptLLSemiTwoPlainValueless, typeof(OptLLSemiTwoPlainValuelessParser)),
		(nameof(OptLLSemiTwoBracesDeferred), OptLLSemiTwoBracesDeferred, typeof(OptLLSemiTwoBracesDeferredParser)),
		(nameof(OptLLSemiTwoBracesImmediate), OptLLSemiTwoBracesImmediate, typeof(OptLLSemiTwoBracesImmediateParser)),
		(nameof(OptLLSemiTwoBracesValueless), OptLLSemiTwoBracesValueless, typeof(OptLLSemiTwoBracesValuelessParser)),
		(nameof(OptLLSemiTwoLookDeferred), OptLLSemiTwoLookDeferred, typeof(OptLLSemiTwoLookDeferredParser)),
		(nameof(OptLLSemiTwoLookImmediate), OptLLSemiTwoLookImmediate, typeof(OptLLSemiTwoLookImmediateParser)),
		(nameof(OptLLSemiTwoLookValueless), OptLLSemiTwoLookValueless, typeof(OptLLSemiTwoLookValuelessParser)),
		(nameof(OptLLBareTwoPlainDeferred), OptLLBareTwoPlainDeferred, typeof(OptLLBareTwoPlainDeferredParser)),
		(nameof(OptLLBareTwoPlainImmediate), OptLLBareTwoPlainImmediate, typeof(OptLLBareTwoPlainImmediateParser)),
		(nameof(OptLLBareTwoPlainValueless), OptLLBareTwoPlainValueless, typeof(OptLLBareTwoPlainValuelessParser)),
		(nameof(OptLLBareTwoBracesDeferred), OptLLBareTwoBracesDeferred, typeof(OptLLBareTwoBracesDeferredParser)),
		(nameof(OptLLBareTwoBracesImmediate), OptLLBareTwoBracesImmediate, typeof(OptLLBareTwoBracesImmediateParser)),
		(nameof(OptLLBareTwoBracesValueless), OptLLBareTwoBracesValueless, typeof(OptLLBareTwoBracesValuelessParser)),
		(nameof(OptLLBareTwoLookDeferred), OptLLBareTwoLookDeferred, typeof(OptLLBareTwoLookDeferredParser)),
		(nameof(OptLLBareTwoLookImmediate), OptLLBareTwoLookImmediate, typeof(OptLLBareTwoLookImmediateParser)),
		(nameof(OptLLBareTwoLookValueless), OptLLBareTwoLookValueless, typeof(OptLLBareTwoLookValuelessParser)),
	];
}

[Gram(GiveBackFamily.StarLSemiPlusPlainDeferred, Carrier = GramCarrier.Deferred)] static partial class StarLSemiPlusPlainDeferredParser;
[Gram(GiveBackFamily.StarLSemiPlusPlainImmediate, Carrier = GramCarrier.Immediate)] static partial class StarLSemiPlusPlainImmediateParser;
[Gram(GiveBackFamily.StarLSemiPlusPlainValueless)] static partial class StarLSemiPlusPlainValuelessParser;
[Gram(GiveBackFamily.StarLSemiPlusBracesDeferred, Carrier = GramCarrier.Deferred)] static partial class StarLSemiPlusBracesDeferredParser;
[Gram(GiveBackFamily.StarLSemiPlusBracesImmediate, Carrier = GramCarrier.Immediate)] static partial class StarLSemiPlusBracesImmediateParser;
[Gram(GiveBackFamily.StarLSemiPlusBracesValueless)] static partial class StarLSemiPlusBracesValuelessParser;
[Gram(GiveBackFamily.StarLSemiPlusLookDeferred, Carrier = GramCarrier.Deferred)] static partial class StarLSemiPlusLookDeferredParser;
[Gram(GiveBackFamily.StarLSemiPlusLookImmediate, Carrier = GramCarrier.Immediate)] static partial class StarLSemiPlusLookImmediateParser;
[Gram(GiveBackFamily.StarLSemiPlusLookValueless)] static partial class StarLSemiPlusLookValuelessParser;
[Gram(GiveBackFamily.StarLSemiStarPlainDeferred, Carrier = GramCarrier.Deferred)] static partial class StarLSemiStarPlainDeferredParser;
[Gram(GiveBackFamily.StarLSemiStarPlainImmediate, Carrier = GramCarrier.Immediate)] static partial class StarLSemiStarPlainImmediateParser;
[Gram(GiveBackFamily.StarLSemiStarPlainValueless)] static partial class StarLSemiStarPlainValuelessParser;
[Gram(GiveBackFamily.StarLSemiStarBracesDeferred, Carrier = GramCarrier.Deferred)] static partial class StarLSemiStarBracesDeferredParser;
[Gram(GiveBackFamily.StarLSemiStarBracesImmediate, Carrier = GramCarrier.Immediate)] static partial class StarLSemiStarBracesImmediateParser;
[Gram(GiveBackFamily.StarLSemiStarBracesValueless)] static partial class StarLSemiStarBracesValuelessParser;
[Gram(GiveBackFamily.StarLSemiStarLookDeferred, Carrier = GramCarrier.Deferred)] static partial class StarLSemiStarLookDeferredParser;
[Gram(GiveBackFamily.StarLSemiStarLookImmediate, Carrier = GramCarrier.Immediate)] static partial class StarLSemiStarLookImmediateParser;
[Gram(GiveBackFamily.StarLSemiStarLookValueless)] static partial class StarLSemiStarLookValuelessParser;
[Gram(GiveBackFamily.StarLSemiTwoPlainDeferred, Carrier = GramCarrier.Deferred)] static partial class StarLSemiTwoPlainDeferredParser;
[Gram(GiveBackFamily.StarLSemiTwoPlainImmediate, Carrier = GramCarrier.Immediate)] static partial class StarLSemiTwoPlainImmediateParser;
[Gram(GiveBackFamily.StarLSemiTwoPlainValueless)] static partial class StarLSemiTwoPlainValuelessParser;
[Gram(GiveBackFamily.StarLSemiTwoBracesDeferred, Carrier = GramCarrier.Deferred)] static partial class StarLSemiTwoBracesDeferredParser;
[Gram(GiveBackFamily.StarLSemiTwoBracesImmediate, Carrier = GramCarrier.Immediate)] static partial class StarLSemiTwoBracesImmediateParser;
[Gram(GiveBackFamily.StarLSemiTwoBracesValueless)] static partial class StarLSemiTwoBracesValuelessParser;
[Gram(GiveBackFamily.StarLSemiTwoLookDeferred, Carrier = GramCarrier.Deferred)] static partial class StarLSemiTwoLookDeferredParser;
[Gram(GiveBackFamily.StarLSemiTwoLookImmediate, Carrier = GramCarrier.Immediate)] static partial class StarLSemiTwoLookImmediateParser;
[Gram(GiveBackFamily.StarLSemiTwoLookValueless)] static partial class StarLSemiTwoLookValuelessParser;
[Gram(GiveBackFamily.StarLBareTwoPlainDeferred, Carrier = GramCarrier.Deferred)] static partial class StarLBareTwoPlainDeferredParser;
[Gram(GiveBackFamily.StarLBareTwoPlainImmediate, Carrier = GramCarrier.Immediate)] static partial class StarLBareTwoPlainImmediateParser;
[Gram(GiveBackFamily.StarLBareTwoPlainValueless)] static partial class StarLBareTwoPlainValuelessParser;
[Gram(GiveBackFamily.StarLBareTwoBracesDeferred, Carrier = GramCarrier.Deferred)] static partial class StarLBareTwoBracesDeferredParser;
[Gram(GiveBackFamily.StarLBareTwoBracesImmediate, Carrier = GramCarrier.Immediate)] static partial class StarLBareTwoBracesImmediateParser;
[Gram(GiveBackFamily.StarLBareTwoBracesValueless)] static partial class StarLBareTwoBracesValuelessParser;
[Gram(GiveBackFamily.StarLBareTwoLookDeferred, Carrier = GramCarrier.Deferred)] static partial class StarLBareTwoLookDeferredParser;
[Gram(GiveBackFamily.StarLBareTwoLookImmediate, Carrier = GramCarrier.Immediate)] static partial class StarLBareTwoLookImmediateParser;
[Gram(GiveBackFamily.StarLBareTwoLookValueless)] static partial class StarLBareTwoLookValuelessParser;
[Gram(GiveBackFamily.StarLLSemiPlusPlainDeferred, Carrier = GramCarrier.Deferred)] static partial class StarLLSemiPlusPlainDeferredParser;
[Gram(GiveBackFamily.StarLLSemiPlusPlainImmediate, Carrier = GramCarrier.Immediate)] static partial class StarLLSemiPlusPlainImmediateParser;
[Gram(GiveBackFamily.StarLLSemiPlusPlainValueless)] static partial class StarLLSemiPlusPlainValuelessParser;
[Gram(GiveBackFamily.StarLLSemiPlusBracesDeferred, Carrier = GramCarrier.Deferred)] static partial class StarLLSemiPlusBracesDeferredParser;
[Gram(GiveBackFamily.StarLLSemiPlusBracesImmediate, Carrier = GramCarrier.Immediate)] static partial class StarLLSemiPlusBracesImmediateParser;
[Gram(GiveBackFamily.StarLLSemiPlusBracesValueless)] static partial class StarLLSemiPlusBracesValuelessParser;
[Gram(GiveBackFamily.StarLLSemiPlusLookDeferred, Carrier = GramCarrier.Deferred)] static partial class StarLLSemiPlusLookDeferredParser;
[Gram(GiveBackFamily.StarLLSemiPlusLookImmediate, Carrier = GramCarrier.Immediate)] static partial class StarLLSemiPlusLookImmediateParser;
[Gram(GiveBackFamily.StarLLSemiPlusLookValueless)] static partial class StarLLSemiPlusLookValuelessParser;
[Gram(GiveBackFamily.StarLLSemiStarPlainDeferred, Carrier = GramCarrier.Deferred)] static partial class StarLLSemiStarPlainDeferredParser;
[Gram(GiveBackFamily.StarLLSemiStarPlainImmediate, Carrier = GramCarrier.Immediate)] static partial class StarLLSemiStarPlainImmediateParser;
[Gram(GiveBackFamily.StarLLSemiStarPlainValueless)] static partial class StarLLSemiStarPlainValuelessParser;
[Gram(GiveBackFamily.StarLLSemiStarBracesDeferred, Carrier = GramCarrier.Deferred)] static partial class StarLLSemiStarBracesDeferredParser;
[Gram(GiveBackFamily.StarLLSemiStarBracesImmediate, Carrier = GramCarrier.Immediate)] static partial class StarLLSemiStarBracesImmediateParser;
[Gram(GiveBackFamily.StarLLSemiStarBracesValueless)] static partial class StarLLSemiStarBracesValuelessParser;
[Gram(GiveBackFamily.StarLLSemiStarLookDeferred, Carrier = GramCarrier.Deferred)] static partial class StarLLSemiStarLookDeferredParser;
[Gram(GiveBackFamily.StarLLSemiStarLookImmediate, Carrier = GramCarrier.Immediate)] static partial class StarLLSemiStarLookImmediateParser;
[Gram(GiveBackFamily.StarLLSemiStarLookValueless)] static partial class StarLLSemiStarLookValuelessParser;
[Gram(GiveBackFamily.StarLLSemiTwoPlainDeferred, Carrier = GramCarrier.Deferred)] static partial class StarLLSemiTwoPlainDeferredParser;
[Gram(GiveBackFamily.StarLLSemiTwoPlainImmediate, Carrier = GramCarrier.Immediate)] static partial class StarLLSemiTwoPlainImmediateParser;
[Gram(GiveBackFamily.StarLLSemiTwoPlainValueless)] static partial class StarLLSemiTwoPlainValuelessParser;
[Gram(GiveBackFamily.StarLLSemiTwoBracesDeferred, Carrier = GramCarrier.Deferred)] static partial class StarLLSemiTwoBracesDeferredParser;
[Gram(GiveBackFamily.StarLLSemiTwoBracesImmediate, Carrier = GramCarrier.Immediate)] static partial class StarLLSemiTwoBracesImmediateParser;
[Gram(GiveBackFamily.StarLLSemiTwoBracesValueless)] static partial class StarLLSemiTwoBracesValuelessParser;
[Gram(GiveBackFamily.StarLLSemiTwoLookDeferred, Carrier = GramCarrier.Deferred)] static partial class StarLLSemiTwoLookDeferredParser;
[Gram(GiveBackFamily.StarLLSemiTwoLookImmediate, Carrier = GramCarrier.Immediate)] static partial class StarLLSemiTwoLookImmediateParser;
[Gram(GiveBackFamily.StarLLSemiTwoLookValueless)] static partial class StarLLSemiTwoLookValuelessParser;
[Gram(GiveBackFamily.StarLLBareTwoPlainDeferred, Carrier = GramCarrier.Deferred)] static partial class StarLLBareTwoPlainDeferredParser;
[Gram(GiveBackFamily.StarLLBareTwoPlainImmediate, Carrier = GramCarrier.Immediate)] static partial class StarLLBareTwoPlainImmediateParser;
[Gram(GiveBackFamily.StarLLBareTwoPlainValueless)] static partial class StarLLBareTwoPlainValuelessParser;
[Gram(GiveBackFamily.StarLLBareTwoBracesDeferred, Carrier = GramCarrier.Deferred)] static partial class StarLLBareTwoBracesDeferredParser;
[Gram(GiveBackFamily.StarLLBareTwoBracesImmediate, Carrier = GramCarrier.Immediate)] static partial class StarLLBareTwoBracesImmediateParser;
[Gram(GiveBackFamily.StarLLBareTwoBracesValueless)] static partial class StarLLBareTwoBracesValuelessParser;
[Gram(GiveBackFamily.StarLLBareTwoLookDeferred, Carrier = GramCarrier.Deferred)] static partial class StarLLBareTwoLookDeferredParser;
[Gram(GiveBackFamily.StarLLBareTwoLookImmediate, Carrier = GramCarrier.Immediate)] static partial class StarLLBareTwoLookImmediateParser;
[Gram(GiveBackFamily.StarLLBareTwoLookValueless)] static partial class StarLLBareTwoLookValuelessParser;
[Gram(GiveBackFamily.PlusLSemiPlusPlainDeferred, Carrier = GramCarrier.Deferred)] static partial class PlusLSemiPlusPlainDeferredParser;
[Gram(GiveBackFamily.PlusLSemiPlusPlainImmediate, Carrier = GramCarrier.Immediate)] static partial class PlusLSemiPlusPlainImmediateParser;
[Gram(GiveBackFamily.PlusLSemiPlusPlainValueless)] static partial class PlusLSemiPlusPlainValuelessParser;
[Gram(GiveBackFamily.PlusLSemiPlusBracesDeferred, Carrier = GramCarrier.Deferred)] static partial class PlusLSemiPlusBracesDeferredParser;
[Gram(GiveBackFamily.PlusLSemiPlusBracesImmediate, Carrier = GramCarrier.Immediate)] static partial class PlusLSemiPlusBracesImmediateParser;
[Gram(GiveBackFamily.PlusLSemiPlusBracesValueless)] static partial class PlusLSemiPlusBracesValuelessParser;
[Gram(GiveBackFamily.PlusLSemiPlusLookDeferred, Carrier = GramCarrier.Deferred)] static partial class PlusLSemiPlusLookDeferredParser;
[Gram(GiveBackFamily.PlusLSemiPlusLookImmediate, Carrier = GramCarrier.Immediate)] static partial class PlusLSemiPlusLookImmediateParser;
[Gram(GiveBackFamily.PlusLSemiPlusLookValueless)] static partial class PlusLSemiPlusLookValuelessParser;
[Gram(GiveBackFamily.PlusLSemiStarPlainDeferred, Carrier = GramCarrier.Deferred)] static partial class PlusLSemiStarPlainDeferredParser;
[Gram(GiveBackFamily.PlusLSemiStarPlainImmediate, Carrier = GramCarrier.Immediate)] static partial class PlusLSemiStarPlainImmediateParser;
[Gram(GiveBackFamily.PlusLSemiStarPlainValueless)] static partial class PlusLSemiStarPlainValuelessParser;
[Gram(GiveBackFamily.PlusLSemiStarBracesDeferred, Carrier = GramCarrier.Deferred)] static partial class PlusLSemiStarBracesDeferredParser;
[Gram(GiveBackFamily.PlusLSemiStarBracesImmediate, Carrier = GramCarrier.Immediate)] static partial class PlusLSemiStarBracesImmediateParser;
[Gram(GiveBackFamily.PlusLSemiStarBracesValueless)] static partial class PlusLSemiStarBracesValuelessParser;
[Gram(GiveBackFamily.PlusLSemiStarLookDeferred, Carrier = GramCarrier.Deferred)] static partial class PlusLSemiStarLookDeferredParser;
[Gram(GiveBackFamily.PlusLSemiStarLookImmediate, Carrier = GramCarrier.Immediate)] static partial class PlusLSemiStarLookImmediateParser;
[Gram(GiveBackFamily.PlusLSemiStarLookValueless)] static partial class PlusLSemiStarLookValuelessParser;
[Gram(GiveBackFamily.PlusLSemiTwoPlainDeferred, Carrier = GramCarrier.Deferred)] static partial class PlusLSemiTwoPlainDeferredParser;
[Gram(GiveBackFamily.PlusLSemiTwoPlainImmediate, Carrier = GramCarrier.Immediate)] static partial class PlusLSemiTwoPlainImmediateParser;
[Gram(GiveBackFamily.PlusLSemiTwoPlainValueless)] static partial class PlusLSemiTwoPlainValuelessParser;
[Gram(GiveBackFamily.PlusLSemiTwoBracesDeferred, Carrier = GramCarrier.Deferred)] static partial class PlusLSemiTwoBracesDeferredParser;
[Gram(GiveBackFamily.PlusLSemiTwoBracesImmediate, Carrier = GramCarrier.Immediate)] static partial class PlusLSemiTwoBracesImmediateParser;
[Gram(GiveBackFamily.PlusLSemiTwoBracesValueless)] static partial class PlusLSemiTwoBracesValuelessParser;
[Gram(GiveBackFamily.PlusLSemiTwoLookDeferred, Carrier = GramCarrier.Deferred)] static partial class PlusLSemiTwoLookDeferredParser;
[Gram(GiveBackFamily.PlusLSemiTwoLookImmediate, Carrier = GramCarrier.Immediate)] static partial class PlusLSemiTwoLookImmediateParser;
[Gram(GiveBackFamily.PlusLSemiTwoLookValueless)] static partial class PlusLSemiTwoLookValuelessParser;
[Gram(GiveBackFamily.PlusLBareTwoPlainDeferred, Carrier = GramCarrier.Deferred)] static partial class PlusLBareTwoPlainDeferredParser;
[Gram(GiveBackFamily.PlusLBareTwoPlainImmediate, Carrier = GramCarrier.Immediate)] static partial class PlusLBareTwoPlainImmediateParser;
[Gram(GiveBackFamily.PlusLBareTwoPlainValueless)] static partial class PlusLBareTwoPlainValuelessParser;
[Gram(GiveBackFamily.PlusLBareTwoBracesDeferred, Carrier = GramCarrier.Deferred)] static partial class PlusLBareTwoBracesDeferredParser;
[Gram(GiveBackFamily.PlusLBareTwoBracesImmediate, Carrier = GramCarrier.Immediate)] static partial class PlusLBareTwoBracesImmediateParser;
[Gram(GiveBackFamily.PlusLBareTwoBracesValueless)] static partial class PlusLBareTwoBracesValuelessParser;
[Gram(GiveBackFamily.PlusLBareTwoLookDeferred, Carrier = GramCarrier.Deferred)] static partial class PlusLBareTwoLookDeferredParser;
[Gram(GiveBackFamily.PlusLBareTwoLookImmediate, Carrier = GramCarrier.Immediate)] static partial class PlusLBareTwoLookImmediateParser;
[Gram(GiveBackFamily.PlusLBareTwoLookValueless)] static partial class PlusLBareTwoLookValuelessParser;
[Gram(GiveBackFamily.PlusLLSemiPlusPlainDeferred, Carrier = GramCarrier.Deferred)] static partial class PlusLLSemiPlusPlainDeferredParser;
[Gram(GiveBackFamily.PlusLLSemiPlusPlainImmediate, Carrier = GramCarrier.Immediate)] static partial class PlusLLSemiPlusPlainImmediateParser;
[Gram(GiveBackFamily.PlusLLSemiPlusPlainValueless)] static partial class PlusLLSemiPlusPlainValuelessParser;
[Gram(GiveBackFamily.PlusLLSemiPlusBracesDeferred, Carrier = GramCarrier.Deferred)] static partial class PlusLLSemiPlusBracesDeferredParser;
[Gram(GiveBackFamily.PlusLLSemiPlusBracesImmediate, Carrier = GramCarrier.Immediate)] static partial class PlusLLSemiPlusBracesImmediateParser;
[Gram(GiveBackFamily.PlusLLSemiPlusBracesValueless)] static partial class PlusLLSemiPlusBracesValuelessParser;
[Gram(GiveBackFamily.PlusLLSemiPlusLookDeferred, Carrier = GramCarrier.Deferred)] static partial class PlusLLSemiPlusLookDeferredParser;
[Gram(GiveBackFamily.PlusLLSemiPlusLookImmediate, Carrier = GramCarrier.Immediate)] static partial class PlusLLSemiPlusLookImmediateParser;
[Gram(GiveBackFamily.PlusLLSemiPlusLookValueless)] static partial class PlusLLSemiPlusLookValuelessParser;
[Gram(GiveBackFamily.PlusLLSemiStarPlainDeferred, Carrier = GramCarrier.Deferred)] static partial class PlusLLSemiStarPlainDeferredParser;
[Gram(GiveBackFamily.PlusLLSemiStarPlainImmediate, Carrier = GramCarrier.Immediate)] static partial class PlusLLSemiStarPlainImmediateParser;
[Gram(GiveBackFamily.PlusLLSemiStarPlainValueless)] static partial class PlusLLSemiStarPlainValuelessParser;
[Gram(GiveBackFamily.PlusLLSemiStarBracesDeferred, Carrier = GramCarrier.Deferred)] static partial class PlusLLSemiStarBracesDeferredParser;
[Gram(GiveBackFamily.PlusLLSemiStarBracesImmediate, Carrier = GramCarrier.Immediate)] static partial class PlusLLSemiStarBracesImmediateParser;
[Gram(GiveBackFamily.PlusLLSemiStarBracesValueless)] static partial class PlusLLSemiStarBracesValuelessParser;
[Gram(GiveBackFamily.PlusLLSemiStarLookDeferred, Carrier = GramCarrier.Deferred)] static partial class PlusLLSemiStarLookDeferredParser;
[Gram(GiveBackFamily.PlusLLSemiStarLookImmediate, Carrier = GramCarrier.Immediate)] static partial class PlusLLSemiStarLookImmediateParser;
[Gram(GiveBackFamily.PlusLLSemiStarLookValueless)] static partial class PlusLLSemiStarLookValuelessParser;
[Gram(GiveBackFamily.PlusLLSemiTwoPlainDeferred, Carrier = GramCarrier.Deferred)] static partial class PlusLLSemiTwoPlainDeferredParser;
[Gram(GiveBackFamily.PlusLLSemiTwoPlainImmediate, Carrier = GramCarrier.Immediate)] static partial class PlusLLSemiTwoPlainImmediateParser;
[Gram(GiveBackFamily.PlusLLSemiTwoPlainValueless)] static partial class PlusLLSemiTwoPlainValuelessParser;
[Gram(GiveBackFamily.PlusLLSemiTwoBracesDeferred, Carrier = GramCarrier.Deferred)] static partial class PlusLLSemiTwoBracesDeferredParser;
[Gram(GiveBackFamily.PlusLLSemiTwoBracesImmediate, Carrier = GramCarrier.Immediate)] static partial class PlusLLSemiTwoBracesImmediateParser;
[Gram(GiveBackFamily.PlusLLSemiTwoBracesValueless)] static partial class PlusLLSemiTwoBracesValuelessParser;
[Gram(GiveBackFamily.PlusLLSemiTwoLookDeferred, Carrier = GramCarrier.Deferred)] static partial class PlusLLSemiTwoLookDeferredParser;
[Gram(GiveBackFamily.PlusLLSemiTwoLookImmediate, Carrier = GramCarrier.Immediate)] static partial class PlusLLSemiTwoLookImmediateParser;
[Gram(GiveBackFamily.PlusLLSemiTwoLookValueless)] static partial class PlusLLSemiTwoLookValuelessParser;
[Gram(GiveBackFamily.PlusLLBareTwoPlainDeferred, Carrier = GramCarrier.Deferred)] static partial class PlusLLBareTwoPlainDeferredParser;
[Gram(GiveBackFamily.PlusLLBareTwoPlainImmediate, Carrier = GramCarrier.Immediate)] static partial class PlusLLBareTwoPlainImmediateParser;
[Gram(GiveBackFamily.PlusLLBareTwoPlainValueless)] static partial class PlusLLBareTwoPlainValuelessParser;
[Gram(GiveBackFamily.PlusLLBareTwoBracesDeferred, Carrier = GramCarrier.Deferred)] static partial class PlusLLBareTwoBracesDeferredParser;
[Gram(GiveBackFamily.PlusLLBareTwoBracesImmediate, Carrier = GramCarrier.Immediate)] static partial class PlusLLBareTwoBracesImmediateParser;
[Gram(GiveBackFamily.PlusLLBareTwoBracesValueless)] static partial class PlusLLBareTwoBracesValuelessParser;
[Gram(GiveBackFamily.PlusLLBareTwoLookDeferred, Carrier = GramCarrier.Deferred)] static partial class PlusLLBareTwoLookDeferredParser;
[Gram(GiveBackFamily.PlusLLBareTwoLookImmediate, Carrier = GramCarrier.Immediate)] static partial class PlusLLBareTwoLookImmediateParser;
[Gram(GiveBackFamily.PlusLLBareTwoLookValueless)] static partial class PlusLLBareTwoLookValuelessParser;
[Gram(GiveBackFamily.OptLSemiPlusPlainDeferred, Carrier = GramCarrier.Deferred)] static partial class OptLSemiPlusPlainDeferredParser;
[Gram(GiveBackFamily.OptLSemiPlusPlainImmediate, Carrier = GramCarrier.Immediate)] static partial class OptLSemiPlusPlainImmediateParser;
[Gram(GiveBackFamily.OptLSemiPlusPlainValueless)] static partial class OptLSemiPlusPlainValuelessParser;
[Gram(GiveBackFamily.OptLSemiPlusBracesDeferred, Carrier = GramCarrier.Deferred)] static partial class OptLSemiPlusBracesDeferredParser;
[Gram(GiveBackFamily.OptLSemiPlusBracesImmediate, Carrier = GramCarrier.Immediate)] static partial class OptLSemiPlusBracesImmediateParser;
[Gram(GiveBackFamily.OptLSemiPlusBracesValueless)] static partial class OptLSemiPlusBracesValuelessParser;
[Gram(GiveBackFamily.OptLSemiPlusLookDeferred, Carrier = GramCarrier.Deferred)] static partial class OptLSemiPlusLookDeferredParser;
[Gram(GiveBackFamily.OptLSemiPlusLookImmediate, Carrier = GramCarrier.Immediate)] static partial class OptLSemiPlusLookImmediateParser;
[Gram(GiveBackFamily.OptLSemiPlusLookValueless)] static partial class OptLSemiPlusLookValuelessParser;
[Gram(GiveBackFamily.OptLSemiStarPlainDeferred, Carrier = GramCarrier.Deferred)] static partial class OptLSemiStarPlainDeferredParser;
[Gram(GiveBackFamily.OptLSemiStarPlainImmediate, Carrier = GramCarrier.Immediate)] static partial class OptLSemiStarPlainImmediateParser;
[Gram(GiveBackFamily.OptLSemiStarPlainValueless)] static partial class OptLSemiStarPlainValuelessParser;
[Gram(GiveBackFamily.OptLSemiStarBracesDeferred, Carrier = GramCarrier.Deferred)] static partial class OptLSemiStarBracesDeferredParser;
[Gram(GiveBackFamily.OptLSemiStarBracesImmediate, Carrier = GramCarrier.Immediate)] static partial class OptLSemiStarBracesImmediateParser;
[Gram(GiveBackFamily.OptLSemiStarBracesValueless)] static partial class OptLSemiStarBracesValuelessParser;
[Gram(GiveBackFamily.OptLSemiStarLookDeferred, Carrier = GramCarrier.Deferred)] static partial class OptLSemiStarLookDeferredParser;
[Gram(GiveBackFamily.OptLSemiStarLookImmediate, Carrier = GramCarrier.Immediate)] static partial class OptLSemiStarLookImmediateParser;
[Gram(GiveBackFamily.OptLSemiStarLookValueless)] static partial class OptLSemiStarLookValuelessParser;
[Gram(GiveBackFamily.OptLSemiTwoPlainDeferred, Carrier = GramCarrier.Deferred)] static partial class OptLSemiTwoPlainDeferredParser;
[Gram(GiveBackFamily.OptLSemiTwoPlainImmediate, Carrier = GramCarrier.Immediate)] static partial class OptLSemiTwoPlainImmediateParser;
[Gram(GiveBackFamily.OptLSemiTwoPlainValueless)] static partial class OptLSemiTwoPlainValuelessParser;
[Gram(GiveBackFamily.OptLSemiTwoBracesDeferred, Carrier = GramCarrier.Deferred)] static partial class OptLSemiTwoBracesDeferredParser;
[Gram(GiveBackFamily.OptLSemiTwoBracesImmediate, Carrier = GramCarrier.Immediate)] static partial class OptLSemiTwoBracesImmediateParser;
[Gram(GiveBackFamily.OptLSemiTwoBracesValueless)] static partial class OptLSemiTwoBracesValuelessParser;
[Gram(GiveBackFamily.OptLSemiTwoLookDeferred, Carrier = GramCarrier.Deferred)] static partial class OptLSemiTwoLookDeferredParser;
[Gram(GiveBackFamily.OptLSemiTwoLookImmediate, Carrier = GramCarrier.Immediate)] static partial class OptLSemiTwoLookImmediateParser;
[Gram(GiveBackFamily.OptLSemiTwoLookValueless)] static partial class OptLSemiTwoLookValuelessParser;
[Gram(GiveBackFamily.OptLBareTwoPlainDeferred, Carrier = GramCarrier.Deferred)] static partial class OptLBareTwoPlainDeferredParser;
[Gram(GiveBackFamily.OptLBareTwoPlainImmediate, Carrier = GramCarrier.Immediate)] static partial class OptLBareTwoPlainImmediateParser;
[Gram(GiveBackFamily.OptLBareTwoPlainValueless)] static partial class OptLBareTwoPlainValuelessParser;
[Gram(GiveBackFamily.OptLBareTwoBracesDeferred, Carrier = GramCarrier.Deferred)] static partial class OptLBareTwoBracesDeferredParser;
[Gram(GiveBackFamily.OptLBareTwoBracesImmediate, Carrier = GramCarrier.Immediate)] static partial class OptLBareTwoBracesImmediateParser;
[Gram(GiveBackFamily.OptLBareTwoBracesValueless)] static partial class OptLBareTwoBracesValuelessParser;
[Gram(GiveBackFamily.OptLBareTwoLookDeferred, Carrier = GramCarrier.Deferred)] static partial class OptLBareTwoLookDeferredParser;
[Gram(GiveBackFamily.OptLBareTwoLookImmediate, Carrier = GramCarrier.Immediate)] static partial class OptLBareTwoLookImmediateParser;
[Gram(GiveBackFamily.OptLBareTwoLookValueless)] static partial class OptLBareTwoLookValuelessParser;
[Gram(GiveBackFamily.OptLLSemiPlusPlainDeferred, Carrier = GramCarrier.Deferred)] static partial class OptLLSemiPlusPlainDeferredParser;
[Gram(GiveBackFamily.OptLLSemiPlusPlainImmediate, Carrier = GramCarrier.Immediate)] static partial class OptLLSemiPlusPlainImmediateParser;
[Gram(GiveBackFamily.OptLLSemiPlusPlainValueless)] static partial class OptLLSemiPlusPlainValuelessParser;
[Gram(GiveBackFamily.OptLLSemiPlusBracesDeferred, Carrier = GramCarrier.Deferred)] static partial class OptLLSemiPlusBracesDeferredParser;
[Gram(GiveBackFamily.OptLLSemiPlusBracesImmediate, Carrier = GramCarrier.Immediate)] static partial class OptLLSemiPlusBracesImmediateParser;
[Gram(GiveBackFamily.OptLLSemiPlusBracesValueless)] static partial class OptLLSemiPlusBracesValuelessParser;
[Gram(GiveBackFamily.OptLLSemiPlusLookDeferred, Carrier = GramCarrier.Deferred)] static partial class OptLLSemiPlusLookDeferredParser;
[Gram(GiveBackFamily.OptLLSemiPlusLookImmediate, Carrier = GramCarrier.Immediate)] static partial class OptLLSemiPlusLookImmediateParser;
[Gram(GiveBackFamily.OptLLSemiPlusLookValueless)] static partial class OptLLSemiPlusLookValuelessParser;
[Gram(GiveBackFamily.OptLLSemiStarPlainDeferred, Carrier = GramCarrier.Deferred)] static partial class OptLLSemiStarPlainDeferredParser;
[Gram(GiveBackFamily.OptLLSemiStarPlainImmediate, Carrier = GramCarrier.Immediate)] static partial class OptLLSemiStarPlainImmediateParser;
[Gram(GiveBackFamily.OptLLSemiStarPlainValueless)] static partial class OptLLSemiStarPlainValuelessParser;
[Gram(GiveBackFamily.OptLLSemiStarBracesDeferred, Carrier = GramCarrier.Deferred)] static partial class OptLLSemiStarBracesDeferredParser;
[Gram(GiveBackFamily.OptLLSemiStarBracesImmediate, Carrier = GramCarrier.Immediate)] static partial class OptLLSemiStarBracesImmediateParser;
[Gram(GiveBackFamily.OptLLSemiStarBracesValueless)] static partial class OptLLSemiStarBracesValuelessParser;
[Gram(GiveBackFamily.OptLLSemiStarLookDeferred, Carrier = GramCarrier.Deferred)] static partial class OptLLSemiStarLookDeferredParser;
[Gram(GiveBackFamily.OptLLSemiStarLookImmediate, Carrier = GramCarrier.Immediate)] static partial class OptLLSemiStarLookImmediateParser;
[Gram(GiveBackFamily.OptLLSemiStarLookValueless)] static partial class OptLLSemiStarLookValuelessParser;
[Gram(GiveBackFamily.OptLLSemiTwoPlainDeferred, Carrier = GramCarrier.Deferred)] static partial class OptLLSemiTwoPlainDeferredParser;
[Gram(GiveBackFamily.OptLLSemiTwoPlainImmediate, Carrier = GramCarrier.Immediate)] static partial class OptLLSemiTwoPlainImmediateParser;
[Gram(GiveBackFamily.OptLLSemiTwoPlainValueless)] static partial class OptLLSemiTwoPlainValuelessParser;
[Gram(GiveBackFamily.OptLLSemiTwoBracesDeferred, Carrier = GramCarrier.Deferred)] static partial class OptLLSemiTwoBracesDeferredParser;
[Gram(GiveBackFamily.OptLLSemiTwoBracesImmediate, Carrier = GramCarrier.Immediate)] static partial class OptLLSemiTwoBracesImmediateParser;
[Gram(GiveBackFamily.OptLLSemiTwoBracesValueless)] static partial class OptLLSemiTwoBracesValuelessParser;
[Gram(GiveBackFamily.OptLLSemiTwoLookDeferred, Carrier = GramCarrier.Deferred)] static partial class OptLLSemiTwoLookDeferredParser;
[Gram(GiveBackFamily.OptLLSemiTwoLookImmediate, Carrier = GramCarrier.Immediate)] static partial class OptLLSemiTwoLookImmediateParser;
[Gram(GiveBackFamily.OptLLSemiTwoLookValueless)] static partial class OptLLSemiTwoLookValuelessParser;
[Gram(GiveBackFamily.OptLLBareTwoPlainDeferred, Carrier = GramCarrier.Deferred)] static partial class OptLLBareTwoPlainDeferredParser;
[Gram(GiveBackFamily.OptLLBareTwoPlainImmediate, Carrier = GramCarrier.Immediate)] static partial class OptLLBareTwoPlainImmediateParser;
[Gram(GiveBackFamily.OptLLBareTwoPlainValueless)] static partial class OptLLBareTwoPlainValuelessParser;
[Gram(GiveBackFamily.OptLLBareTwoBracesDeferred, Carrier = GramCarrier.Deferred)] static partial class OptLLBareTwoBracesDeferredParser;
[Gram(GiveBackFamily.OptLLBareTwoBracesImmediate, Carrier = GramCarrier.Immediate)] static partial class OptLLBareTwoBracesImmediateParser;
[Gram(GiveBackFamily.OptLLBareTwoBracesValueless)] static partial class OptLLBareTwoBracesValuelessParser;
[Gram(GiveBackFamily.OptLLBareTwoLookDeferred, Carrier = GramCarrier.Deferred)] static partial class OptLLBareTwoLookDeferredParser;
[Gram(GiveBackFamily.OptLLBareTwoLookImmediate, Carrier = GramCarrier.Immediate)] static partial class OptLLBareTwoLookImmediateParser;
[Gram(GiveBackFamily.OptLLBareTwoLookValueless)] static partial class OptLLBareTwoLookValuelessParser;
