// A type in the GLOBAL namespace, declared on purpose and in a file of its own, because a
// file-scoped namespace would put it somewhere. `NameOrderTests` needs a name that exists both
// here and inside a namespace a text can `using`, so that which of the two a bare name means is
// a question with an answer.
//
// Public, because the Roslyn oracle beside it compiles a SEPARATE assembly and an internal
// type would be invisible there (CS0122) — the language itself sees the caller's internals,
// so only the oracle needs this.
//
// Nothing else may be added to this file. A global type is reachable from every text this
// assembly reads and shadows anything of the same name a `using` would have brought in
// — which is exactly what this is here to show, and exactly what cost a day when
// `GeneratorDriverTests` did it by accident to `Spans`.

/// <summary>What a bare name means when the global namespace declares it.</summary>
public static class Shadowed
{
	public static string Where()
	{
		return "global";
	}
}
