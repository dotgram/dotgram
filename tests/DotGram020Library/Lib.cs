using DotGram;

namespace OldLib;

[Gram("Lexemes.gram", IncludedAs = "Lex")]
public static partial class Lexemes { }

[GramInclude(typeof(Lexemes), As = "L")]
[Gram("Pairs.gram")]
[GramOptions(Suffix = "Fast", Carrier = GramCarrier.Immediate)]
public static partial class Pairs { }
