using System.Collections.Generic;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Parsing;

namespace DotGram.Grammar.Model;

sealed partial class GrammarNormalizer
{
	internal static bool PublicationFits(ITargetLanguage language, string from, string to, GrammarNamespace ns)
	{
		foreach (var source in Names(from))
			foreach (var target in Names(to))
				if (language.Fits(source, target)) return true;
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
				_results[publication.Rule].Count == 0 ? _language.Spelling(KnownType.Text) : null;
			// Generated capture records have no symbol in the host compilation yet.
			// They are sealed objects without user-defined bases or interfaces.
			var compatible = actual is null ? IsObject(expected) :
				PublicationFits(_language, actual, expected, publication.DeclaredIn);
			if (!type.IsCSharp || !compatible)
				_diagnostics.Add(new GramDiagnostic(PublicationTypeMismatch,
					$"The result of '{publication.Rule.Name}' ({actual ?? "generated capture record"}) is not assignable to publication type '{expected}'. Use a compatible C# type written as '@T'.",
					type.At.Position, type.At.Length, GramSeverity.Error));
		}

		// `object` by its keyword or its name in System; `Object` alone may be a type of the host's.
		bool IsObject(string spelling)
		{
			return _language.Classify(spelling, out var form) == KnownType.Object && form != TypeSpellingForm.Simple;
		}
	}
}
