using DotGram;

namespace DotGram.Examples.Languages;

// The tape's reading of the example, beside the immediate one the example names: the same
// grammar, compiled again into a nested class whose constructions run once the parse has
// accepted (docs/syntax.md §6.6). Here and not in the example, which shows one reading.
[GramOptions(Carrier = GramCarrier.Tape, Suffix = "Tape")]
public sealed partial class Filter
{
}
