using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
	/// What every name the generator makes from an author's name carries between its kind and the
	/// author's identifiers, and what no name of the author's can hold (GRAM1012).
	/// </summary>
	public const string NameMark = "_DotGram";

	/// <summary>
	/// The kinds of the names the generator makes from an author's names in the class it writes into:
	/// <c>&lt;Kind&gt;_DotGram_&lt;Id&gt;</c>, or <c>&lt;Kind&gt;_DotGram_&lt;Id1&gt;_DotGram_&lt;Id2&gt;</c> for a name made from two,
	/// or <c>&lt;Kind&gt;_DotGram</c> for one of a machine that has no name of its own. A kind is a word of
	/// this table, with <c>{v}</c> standing for one of <see cref="Variants"/> and <c>#</c> for a number.
	/// </summary>
	/// <remarks>
	/// <para>
	/// No name of the author's can be one of these, since every one holds <c>_DotGram</c> and no name of the
	/// author's can (GRAM1012) — a publication, a rule's type and a member written by hand in the class
	/// alike — so nothing has to be reserved for them. And two of them are one name only where kind and
	/// identifiers are: the kind ends at the first <c>_</c> and holds none, and every later <c>_DotGram_</c>
	/// separates two identifiers, since no identifier holds <c>_DotGram</c> either (GRAM5018 refuses the one
	/// a namespace can make). Nothing is ever written after an identifier: every variant of a name is a
	/// kind of its own.
	/// </para>
	/// <para>
	/// The kinds are short, since a factory or a walk is named at every call to it and the factories alone
	/// are most of what the large grammars write: <c>New</c> is a factory, <c>Rec</c> a rule's walk.
	/// </para>
	/// </remarks>
	public static readonly string[] HostKinds =
	[
		// A rule's: its walk, under the machine that writes it; the walk that also insists the input
		// ended, and the stages and probes a streamed parse reads it by; the factory of each `=>`, and of
		// each over bytes; the factory of a `recover`'s `=>`; its scanners.
		"Rec{v}", "RecWhole{v}", "WholePart#", "WholeContinue#", "WholeTrivia", "WholeSync",
		"New", "New#", "NewBytes", "NewBytes#", "Recover", "Recover#", "RecoverSpan{v}", "RecoverSpan{v}#",
		"Scan{v}", "Read{v}",

		// A rule's value read again from the text of a token (CSharpEmitter.Value), and the rule a
		// continuation of another is read for.
		"Value", "Measure", "Reread", "RereadFor",

		// A publication's: its buffered reader, its iterator, the form a segmented grammar reads its
		// lines by and the Try beside it; a call into the host's code.
		"Buf", "BufBytes", "BufMemory", "BufBytesMemory", "Iterate", "Own", "TryOwn", "Host#",

		// A machine's, named after the rule it was compiled for, or with no name where it is the only one.
		"Recognize{v}", "RecognizeDispatch{v}", "RecognizePart{v}#", "RecognizeGuard{v}#", "RecognizeGuardKey{v}#", "RecognizeSharpen{v}#", "RecognizeFurther{v}", "RecognizeRanOut{v}",
		"RecognizeAgreeing{v}", "RecognizeClass{v}#", "RecognizeBits{v}#", "RecognizeBit{v}", "RecognizeGroups{v}#",
		"RecognizeSet{v}#", "RecognizeIn{v}", "RecognizeFoldSoFar{v}#",
		"Materialize{v}", "MaterializePart{v}#", "MaterializeConstruct{v}#Part#", "MaterializeDirect{v}",
		"MaterializeDirectPart{v}#", "MaterializeDirectReach{v}", "MaterializeDirectReaches{v}#",
		"MaterializeDirectAsksMarks{v}", "MaterializeDirectArm{v}#",
		"Reader{v}", "Deep{v}", "Same{v}", "Prefix{v}#", "PrefixNext{v}#", "PrefixDepth{v}#", "PrefixLongest{v}#",
		"PrefixMembers{v}#", "PrefixExpected{v}#", "PrefixMiss{v}#", "Reach{v}", "Letters{v}", "TokenAt{v}", "Text{v}", "Span{v}",
		"Offer{v}", "Locate{v}", "Searched{v}",
	];

	/// <summary>
	/// The kinds of the names made from an author's inside a class the generator declares for its own use
	/// (the reader, the engine's tables): <c>&lt;Kind&gt;_&lt;Id&gt;</c>, without the mark, since no name of the
	/// author's reaches those classes. The first <c>_</c> ends the kind, so two are one name only where
	/// kind and identifier are.
	/// </summary>
	public static readonly string[] NestedKinds =
	[
		"Read", "ReadBody", "ReadPart#", "ReadBroken", "ReadBroken#", "RecRead", "RecReadBody", "RecWholeRead",
		"RecWholeReadBody", "CountEntered",
	];

	/// <summary>
	/// What a machine's kinds say about the machine (<c>{v}</c> in <see cref="HostKinds"/>): nothing for the
	/// machine of a published rule, or which of the file's own it is — the one that reads a terminal's value
	/// again, the seam's — or that it reads buffered input, over bytes or over memory the caller holds.
	/// </summary>
	public static readonly string[] Variants = ["", "Value", "Seam", "Buf", "BufBytes", "BufMemory", "BufBytesMemory"];

	/// <summary>A name made from an author's names, in the class the generator writes into.</summary>
	/// <param name="kind">A kind of <see cref="HostKinds"/>, its variant and numbers written in.</param>
	/// <param name="identifiers">The author's names it is made from, first the one it is most about.</param>
	internal static string Named(string kind, params string?[] identifiers)
	{
		System.Diagnostics.Debug.Assert(IsKind(kind, HostKinds), $"'{kind}' is not a kind of the table");

		var written = identifiers.Where(static one => one is { Length: > 0 }).ToArray();

		return written.Length == 0
			? kind + NameMark
			: kind + NameMark + "_" + string.Join(NameMark + "_", written);
	}

	/// <summary>A name made from an author's name inside a class the generator declares for its own use.</summary>
	internal static string Inner(string kind, string identifier)
	{
		System.Diagnostics.Debug.Assert(IsKind(kind, NestedKinds), $"'{kind}' is not a kind of the table");

		return kind + "_" + identifier;
	}

	/// <summary>
	/// Every kind a table allows, numbers written as <c>#</c>: the closed set a name's kind is looked up in.
	/// </summary>
	public static IEnumerable<(string Kind, string Pattern)> Expanded(IEnumerable<string> table)
	{
		foreach (var pattern in table)
			foreach (var variant in pattern.Contains("{v}") ? Variants : [""])
				yield return (pattern.Replace("{v}", variant), pattern);
	}

	/// <summary>Whether a written kind is one of a table, its numbers read as <c>#</c>.</summary>
	public static bool IsKind(string kind, IEnumerable<string> table)
	{
		var shape = Shape(kind);

		return Expanded(table).Any(one => one.Kind == shape);
	}

	/// <summary>A written kind with every run of digits made one <c>#</c>.</summary>
	public static string Shape(string kind)
	{
		var shape = new StringBuilder(kind.Length);

		for (var at = 0; at < kind.Length; at++)
			if (!char.IsDigit(kind[at]))
				shape.Append(kind[at]);
			else if (at == 0 || !char.IsDigit(kind[at - 1]))
				shape.Append('#');

		return shape.ToString();
	}

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
	/// publication or a rule's type may take. A closed list: no name is reserved for being made from
	/// an author's, since every such name carries the mark (<see cref="HostKinds"/>).
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
	/// The methods a publication is written as: its name, and for a <c>parse</c> the <c>Try</c> beside it.
	/// </summary>
	internal static IEnumerable<string> MembersOf(Publication publication)
	{
		yield return publication.MethodName;

		if (publication.Kind == PublishKind.Parse)
			yield return "Try" + publication.MethodName;
	}

	/// <summary>
	/// The names the generated class keeps whatever the grammar says, which an author's publication or
	/// rule's type may not take: the host class, which a member may not be called, and the closed lists of
	/// the plain names it writes.
	/// </summary>
	static HashSet<string> Reserved(string host, bool traced, bool lexical)
	{
		var reserved = new HashSet<string>(PublicNames.Concat(SupportNames), StringComparer.Ordinal) { host };

		if (traced)
			reserved.UnionWith(TraceNames);

		if (lexical)
			reserved.UnionWith(LexerNames);

		return reserved;
	}

	/// <summary>
	/// The names a rule's own type may not take, as a set: what <see cref="Reserved"/> keeps, and every
	/// method a publication is written as. A name the generator makes from the grammar's own names is none
	/// of them, since it carries the mark.
	/// </summary>
	internal static HashSet<string> TypeNamesTaken(string host, bool traced, RecognitionGraph graph, bool lexical = false)
	{
		var taken = Reserved(host, traced, lexical);

		foreach (var publication in graph.Publications)
			taken.UnionWith(MembersOf(publication));

		return taken;
	}

	/// <summary>
	/// What would be written twice into the class the graph is emitted as, said where the author wrote
	/// it: a publication named like something the class keeps, two publications writing one name, two
	/// rules that become one C# name, and two captures that become one property.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The names of the rules' own types are made unique by the generator (<see cref="ResultTypes"/>).
	/// These are not, because renaming would change what a caller reads: a publication is called
	/// what its author said, a rule by its namespaces joined, a property by its capture. Every other
	/// name is made from these by <see cref="Named"/>, and is one name only where they are.
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

		var members = Reserved(host, traced, lexical);
		var names   = new Dictionary<string, Publication>(StringComparer.Ordinal);

		foreach (var publication in graph.Publications)
			foreach (var name in MembersOf(publication))
			{
				if (members.Contains(name))
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
			var identifier = IdentifierOf(rule);

			// A namespace joined to a rule whose name begins with the mark puts the mark inside the
			// identifier, where it would read as the separator of two.
			if (identifier.Contains(NameMark))
			{
				said.Add(Clash(
					$"'{rule.Name}' in namespace '{rule.Namespace?.Name}' is written '{identifier}' in the generated code, " +
					"where a namespace is joined to a rule's name with '_', and '_DotGram' is kept for the generator's own " +
					"names. Rename it (docs/syntax.md §7.3).",
					rule.Declaration?.At.Position ?? 0, rule.Declaration?.At.Length ?? 0));

				continue;
			}

			if (written.TryGetValue(identifier, out var first) && !ReferenceEquals(first, rule))
			{
				said.Add(Clash(
					$"'{rule.Name}' in namespace '{rule.Namespace?.Name}' and '{first.Name}' in namespace " +
					$"'{first.Namespace?.Name}' are both written '{identifier}' in the generated code, where a namespace is " +
					"joined to a rule's name with '_'. Rename one of them (docs/syntax.md §7.3).",
					rule.Declaration?.At.Position ?? 0, rule.Declaration?.At.Length ?? 0));

				continue;
			}

			written[identifier] = rule;
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

	/// <summary>
	/// The author's names the generated class is written under, for a test that holds the generated names
	/// apart: every rule's identifier and publication's name, which a derived name is made from; the
	/// methods the publications are written as; and the rules' types.
	/// </summary>
	internal static GramNames NamesOf(RecognitionGraph graph, IEnumerable<RecognitionGraph> more, string className, string? @namespace, bool traced, bool lexical)
	{
		var results     = new ResultTypes(graph, className, @namespace, traced, lexical);
		var identifiers = new SortedSet<string>(StringComparer.Ordinal);

		foreach (var one in more.Prepend(graph))
		{
			foreach (var rule in one.Rules)
				identifiers.Add(IdentifierOf(rule));

			foreach (var publication in one.Publications)
				identifiers.Add(publication.MethodName);
		}

		return new GramNames(
			[.. identifiers],
			[.. graph.Publications.SelectMany(MembersOf).Distinct()],
			[.. results.Built.Select(rule => results.NameOf(rule)!)]);
	}
}

/// <summary>
/// What tells one machine's names from another's in one class: the variant its kinds carry
/// (<see cref="CSharpEmitter.Variants"/>), and the author's name it was compiled for — the rule of its
/// group, the publication it reads buffered — or none where it is the only one of its variant.
/// </summary>
readonly record struct MachineTag(string Variant, string? Id)
{
	/// <summary>The machine of a grammar with one, and of every machine over nothing published.</summary>
	public static MachineTag None { get; } = new("", null);

	/// <summary>The machine that reads a terminal's value again (CSharpEmitter.Value).</summary>
	public static MachineTag Value { get; } = new("Value", null);

	/// <summary>The machine a lexer skips the trivia with (CSharpEmitter.Seam).</summary>
	public static MachineTag Seam { get; } = new("Seam", null);

	/// <summary>A name of this machine's own: <paramref name="word"/>, the variant, then <paramref name="rest"/> (its numbers).</summary>
	public string Named(string word, string rest = "")
	{
		return CSharpEmitter.Named(word + Variant + rest, Id);
	}

	/// <summary>A name this machine writes for one of the grammar's names, the machine's own after it.</summary>
	public string Of(string word, string identifier, string rest = "")
	{
		return CSharpEmitter.Named(word + Variant + rest, identifier, Id);
	}

	/// <summary>A name this machine writes for one of the grammar's names, in a number's place.</summary>
	public string Named(string word, int number)
	{
		return Named(word, number.ToString(System.Globalization.CultureInfo.InvariantCulture));
	}
}

/// <summary>
/// The author's names a generated class is written under (<see cref="GramCompilation.Names"/>): the
/// identifiers of the rules and the names of the publications, which the generator's own names are
/// made from; the methods the publications are written as; and the types of the rules.
/// </summary>
public sealed record GramNames(IReadOnlyList<string> Identifiers, IReadOnlyList<string> Publications, IReadOnlyList<string> Types);
