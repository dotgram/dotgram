using System.Collections.Generic;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Parsing;

namespace DotGram.Grammar.Model;

public sealed partial class GrammarNormalizer
{
	internal static bool PublicationFits(ISymbolResolver resolver, string from, string to, GrammarNamespace ns)
	{
		foreach (var source in Names(from))
			foreach (var target in Names(to))
				if (resolver.IsAssignable(source, target)) return true;
		return false;

		IEnumerable<string> Names(string name)
		{
			yield return name;
			for (var scope = ns; scope is not null; scope = scope.Parent)
				foreach (var import in scope.CSharpImports)
					yield return import + "." + name;
		}
	}

	void CheckPublicationTypes()
	{
		foreach (var publication in _publications)
		{
			if (publication.Yield || publication.ResultType is not { } type) continue;
			var expected = TypeName(type);
			var actual = _types.TryGetValue(publication.Rule, out var declared) ? declared :
				_results[publication.Rule].Count == 0 ? "string" : null;
			// Generated capture records have no symbol in the host compilation yet.
			// They are sealed objects without user-defined bases or interfaces.
			var compatible = actual is null ? expected is "object" or "System.Object" or "global::System.Object" :
				PublicationFits(_resolver, actual, expected, publication.DeclaredIn);
			if (!type.IsCSharp || !compatible)
				_diagnostics.Add(new GramDiagnostic(PublicationTypeMismatch,
					$"The result of '{publication.Rule.Name}' ({actual ?? "generated capture record"}) is not assignable to publication type '{expected}'. Use a compatible C# type written as '@T'.",
					type.At.Position, type.At.Length, GramSeverity.Error));
		}
	}
}
