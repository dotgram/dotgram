using DotGram.Sql;
using DotGram.Sql.Standard;

namespace DotGram.Benchmarks;

// Lab only (not for landing): SQL:2023 with the six rules that gathered two members onto one stack
// split so that each new rule holds the second member alone, compiled immediately and, for the
// same grammar text, on the tape.
[Gram("SqlStandardCarried.gram", Lexical = true, Carrier = GramCarrier.Immediate)]
public static partial class CarriedSqlStandard
{
}

[Gram("SqlStandardCarried.gram", Lexical = true, Carrier = GramCarrier.Tape)]
public static partial class CarriedSqlStandardTape
{
}
