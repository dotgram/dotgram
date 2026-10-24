using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;

namespace DotGram.Grammar.Emit;

public static partial class CSharpEmitter
{
	/// <summary>
	/// A name the generated code takes for two things in one place, or one an author's publication
	/// takes from a member of the class that holds it.
	/// </summary>
	public const string GeneratedNameClash = "GRAM5018";

	/// <summary>
	/// The public types and members the generated class declares beside what the author published, by
	/// the names a caller reads them under. A publication takes the name of a method of the class, and
	/// a method cannot be called what one of these is.
	/// </summary>
	public static readonly string[] PublicNames =
	[
		MatchType, OutcomeType, "SourceSpan", "ParserInput", RecoveredMethod,
	];

	/// <summary>
	/// The types the generated class nests for its own use, which are not the author's to name and
	/// which a rule's type may not take either.
	/// </summary>
	public static readonly string[] SupportNames =
	[
		FailureType, WindowType, LinesType, Machine.WaysType, "Held", "Parser", "ParserArena", "ParserEntry",
		"DirectValues", "ImmediateValues", "BufferedText", "BufferedBytes", "IParserInputSource",
	];

	/// <summary>
	/// The names a rule's own type may not take, as a set: the host class, which a member may not be
	/// called, and every type the generator nests beside it.
	/// </summary>
	internal static HashSet<string> TypeNamesTaken(string host, bool traced)
	{
		var taken = new HashSet<string>(StringComparer.Ordinal) { host };

		taken.UnionWith(PublicNames);
		taken.UnionWith(SupportNames);

		if (traced)
			taken.UnionWith(TraceNames);

		return taken;
	}

	/// <summary>
	/// What would be written twice into the class the graph is emitted as, said where the author wrote
	/// it: a publication named like something the class declares, two rules that become one C# name,
	/// and two captures that become one property.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The names of the rules' own types are made unique by the generator (<see cref="ResultTypes"/>).
	/// These three are not, because renaming would change what a caller reads: a publication is called
	/// what its author said, a rule by its namespaces joined, a property by its capture.
	/// </para>
	/// </remarks>
	/// <param name="className">The host class, as a chain.</param>
	internal static List<GramDiagnostic> NameClashes(
		RecognitionGraph graph, string className, string? @namespace, bool traced)
	{
		var said    = new List<GramDiagnostic>();
		var results = new ResultTypes(graph, className, @namespace, traced);
		var host    = className.Substring(className.LastIndexOf('.') + 1);
		var angle   = host.IndexOf('<');

		if (angle >= 0)
			host = host.Substring(0, angle);

		var members = new HashSet<string>(PublicNames, StringComparer.Ordinal) { host };

		if (traced)
			members.UnionWith(TraceNames);

		foreach (var publication in graph.Publications)
		{
			if (members.Contains(publication.MethodName))
			{
				said.Add(Clash(
					$"'{publication.MethodName}' is the name of a member of the generated class" +
					(publication.MethodName == host ? ", which no member may share its name with" : "") +
					". Give the publication another name with 'as' (docs/syntax.md §6).",
					publication.At.Position, publication.At.Length));

				continue;
			}

			foreach (var rule in results.Built)
				if (results.NameOf(rule) == publication.MethodName)
				{
					said.Add(Clash(
						$"'{publication.MethodName}' is also the name of the type the generated class declares for " +
						$"'{rule.Name}'. Give the publication another name with 'as' (docs/syntax.md §6).",
						publication.At.Position, publication.At.Length));

					break;
				}
		}

		var written = new Dictionary<string, RuleSymbol>(StringComparer.Ordinal);

		foreach (var rule in graph.Rules)
		{
			if (!written.TryGetValue(IdentifierOf(rule), out var first))
			{
				written[IdentifierOf(rule)] = rule;

				continue;
			}

			said.Add(Clash(
				$"'{rule.Name}' in namespace '{rule.Namespace?.Name}' and '{first.Name}' in namespace " +
				$"'{first.Namespace?.Name}' are both written '{IdentifierOf(rule)}' in the generated code, where a " +
				"namespace is joined to a rule's name with '_'. Rename one of them (docs/syntax.md §7.3).",
				rule.Declaration?.At.Position ?? 0, rule.Declaration?.At.Length ?? 0));
		}

		foreach (var rule in results.Built)
		{
			var owner      = results.NameOf(rule)!;
			var properties = new Dictionary<string, string>(StringComparer.Ordinal);

			foreach (var member in graph.Results[rule])
			{
				var property = ResultTypes.PropertyOf(member, owner);

				if (properties.TryGetValue(property, out var other))
				{
					said.Add(Clash(
						$"The captures '{other}' and '{member.Name}' of '{rule.Name}' both become the property '{property}' of its " +
						"value, which has one of each name. Rename one of them (docs/syntax.md §7.3).",
						rule.Declaration?.At.Position ?? 0, rule.Declaration?.At.Length ?? 0));

					continue;
				}

				properties[property] = member.Name;
			}
		}

		return said;

		static GramDiagnostic Clash(string message, int position, int length)
		{
			return new GramDiagnostic(GeneratedNameClash, message, position, length, GramSeverity.Error);
		}
	}
}
