using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

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
	/// the names a caller reads them under: the answer a publication gives, the hooks the author may
	/// implement, and the defaults of a reader that the package's own code reads. A publication takes the name of a method of the class, and
	/// a method cannot be called what one of these is; nor can a rule's type.
	/// </summary>
	public static readonly string[] PublicNames =
	[
		MatchType, OutcomeType, "SourceSpan", "ParserInput", RecoveredMethod, "OnHandingOff", "OnTakingOver",
		"DefaultBufferSize", "DefaultMaxRetained",
	];

	/// <summary>The public method of a lexer, which only a grammar read over tokens has.</summary>
	public static readonly string[] LexerNames = ["Scan"];

	/// <summary>
	/// The members of the generated class written for its own use that keep their plain names. Every
	/// other one has <c>_DotGram</c> in its name, which no name of the author's can (GRAM1012). The
	/// types <c>Failure</c> and <c>ParserEntry</c> are written so many times that the longer name would
	/// be 0.88% of the source of the snapshots; the fields of the pools of parsers and of tokens begin
	/// with an underscore and a lower-case letter, which a rule's type or a publication is not called
	/// in earnest, and renaming them would add 0.35%. They are reserved instead: not a name a
	/// publication or a rule's type may take.
	/// </summary>
	public static readonly string[] SupportNames =
	[
		FailureType, "ParserEntry",
		"_cutInputs", "_cutNext", "_cutTokens", "_deeperParserCount", "_deeperParsers", "_deeperTokenCount",
		"_deeperTokens", "_largeParser", "_largeParserIdle", "_largeParserLetGo", "_largeTokens",
		"_largeTokensIdle", "_largeTokensLetGo", "_spareParser", "_spareParserIdle", "_spareParserLetGo",
		"_spareTies", "_spareTokens", "_spareTokensIdle", "_spareTokensLetGo",
	];

	/// <summary>
	/// What the generated class writes under a name made from the author's: the walk, the scanner, the
	/// factory of a rule and the reader of a publication. A name the author gives a publication or a
	/// rule's type may not begin with one, so that it cannot be one of them.
	/// </summary>
	public static readonly string[] FamilyPrefixes = ["Recognize_", "Scan_", "Construct_", "ReadBuffered_"];

	/// <summary>
	/// The methods a publication is written as: its name, and for a <c>parse</c> the <c>Try</c> beside it.
	/// </summary>
	internal static IEnumerable<string> MembersOf(Publication publication)
	{
		yield return publication.MethodName;

		if (publication.Kind == PublishKind.Parse)
			yield return "Try" + publication.MethodName;
	}

	/// <summary>The methods the generated class writes for one rule, under names made from the rule's.</summary>
	static IEnumerable<string> FamilyOf(RecognitionGraph graph, RuleSymbol rule)
	{
		var name = IdentifierOf(rule);

		yield return "Recognize_" + name;

		if (graph.Publications.Any(publication => ReferenceEquals(publication.Rule, rule)))
			yield return "Recognize_" + name + "_Whole";

		yield return "Scan_" + name;

		// One factory for each construction of the rule, the first under the plain name.
		var constructions = graph.Bodies.TryGetValue(rule, out var body)
			? NodeWalk.Descendants(body).Count(static node => node is Node.Construct)
			: 0;

		for (var one = 0; one < constructions; one++)
			yield return "Construct_" + name + (one == 0 ? "" : "_" + one.ToString(System.Globalization.CultureInfo.InvariantCulture));
	}

	/// <summary>
	/// The names a rule's own type may not take, as a set: the host class, which a member may not be
	/// called; every type and member the generator writes under a plain name; and every method it makes
	/// from the grammar's own names, the factory, walk and scanner of each rule and each publication.
	/// </summary>
	internal static HashSet<string> TypeNamesTaken(string host, bool traced, RecognitionGraph graph, bool lexical = false)
	{
		var taken = new HashSet<string>(StringComparer.Ordinal) { host };

		taken.UnionWith(PublicNames);
		taken.UnionWith(SupportNames);

		if (traced)
			taken.UnionWith(TraceNames);

		if (lexical)
			taken.UnionWith(LexerNames);

		foreach (var publication in graph.Publications)
			taken.UnionWith(MembersOf(publication));

		foreach (var rule in graph.Rules)
			taken.UnionWith(FamilyOf(graph, rule));

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
		RecognitionGraph graph, string className, string? @namespace, bool traced, bool lexical = false)
	{
		var said    = new List<GramDiagnostic>();
		var results = new ResultTypes(graph, className, @namespace, traced, lexical);
		var host    = className.Substring(className.LastIndexOf('.') + 1);
		var angle   = host.IndexOf('<');

		if (angle >= 0)
			host = host.Substring(0, angle);

		var members = new HashSet<string>(PublicNames.Concat(SupportNames), StringComparer.Ordinal) { host };

		if (traced)
			members.UnionWith(TraceNames);

		if (lexical)
			members.UnionWith(LexerNames);

		var names = new Dictionary<string, Publication>(StringComparer.Ordinal);

		foreach (var publication in graph.Publications)
			foreach (var name in MembersOf(publication))
			{
				if (members.Contains(name) || Array.Exists(FamilyPrefixes, prefix => name.StartsWith(prefix, StringComparison.Ordinal)))
				{
					said.Add(Clash(
						$"'{publication.MethodName}' " + (name == publication.MethodName ? "" : $"writes '{name}', which ") +
						"is the name of a member of the generated class" +
						(name == host ? ", which no member may share its name with" : "") +
						". Give the publication another name with 'as' (docs/syntax.md §6).",
						publication.At.Position, publication.At.Length));

					break;
				}

				if (names.TryGetValue(name, out var earlier) && !ReferenceEquals(earlier, publication))
				{
					said.Add(Clash(
						$"'{publication.MethodName}' writes '{name}', which '{earlier.MethodName}' writes too. " +
						"Give one of the publications another name with 'as' (docs/syntax.md §6).",
						publication.At.Position, publication.At.Length));

					break;
				}

				names[name] = publication;
			}

		var written = new Dictionary<string, RuleSymbol>(StringComparer.Ordinal);

		foreach (var rule in graph.Rules)
		{
			RuleSymbol? first = null;
			string? twice = null;

			// The rule's own name, marked so that it is told from the methods made from it.
			foreach (var name in FamilyOf(graph, rule).Prepend("= " + IdentifierOf(rule)))
				if (written.TryGetValue(name, out var other) && !ReferenceEquals(other, rule))
				{
					first = other;
					twice = name;

					break;
				}

			if (first is null)
			{
				foreach (var name in FamilyOf(graph, rule).Prepend("= " + IdentifierOf(rule)))
					written[name] = rule;

				continue;
			}

			said.Add(Clash(
				$"'{rule.Name}' in namespace '{rule.Namespace?.Name}' and '{first.Name}' in namespace " +
				$"'{first.Namespace?.Name}' both write '{twice!.TrimStart('=', ' ')}' in the generated code, where a namespace is joined " +
				"to a rule's name with '_' and the methods of a rule are named after it. Rename one of them " +
				"(docs/syntax.md §7.3).",
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
