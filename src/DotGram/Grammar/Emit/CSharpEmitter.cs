using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

namespace DotGram.Grammar.Emit;

/// <summary>
/// Turns a recognition graph into C#.
/// </summary>
/// <remarks>
/// <para>
/// One machine per published rule — `parse R` and `find R` share one, two publications of
/// different rules get one each — and their recognizers are thin entry wrappers selecting a
/// label and the expected result. Every recognizer has the same external shape: take the
/// input and a position and return the new position or -1. Not every publication reaches a
/// machine at all: one needing none of the three things the arena is for is rendered as an
/// ordinary method instead (<see cref="Machine.CanLower"/>, <c>Machine.Flat.cs</c>).
/// </para>
/// <para>
/// What this file is responsible for is everything around the machines — the published
/// methods, the result types, the factories a <c>=&gt;</c> becomes, the support types
/// shared with the machines, and the layout of the file they all go in. The machines
/// themselves are compiled by <see cref="Machine"/>.
/// </para>
/// <para>
/// The text produced here is compiled in the consumer's project, which is a different
/// set of rules from this assembly's own: see .claude/rules/emitted-code.md.
/// </para>
/// </remarks>
public static partial class CSharpEmitter
{
	/// <param name="graph">The normalized grammar.</param>
	/// <param name="className">
	/// The partial class the generated members go into. A nested host is named by its
	/// chain — <c>Outer.Inner</c> — and written back out nested.
	/// </param>
	/// <param name="namespace">Its namespace, or null for the global one.</param>
	/// <param name="lines">
	/// Where the grammar's own C# came from, for the <c>#line</c> directives of §7.6. Null
	/// emits none, which is right for a caller with no file to point at.
	/// </param>
	/// <summary>
	/// Every method of the finished file held against the size the JIT stops optimizing at.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Over the text, because the text is what every rendering produces and the estimate is
	/// textual anyway — a detector inside one rendering measures that rendering's own
	/// bookkeeping and misses the next one, which is how the first version of this warned
	/// about nothing. A local function is a method of its own to the JIT and is measured as
	/// one here: a header at deeper indent ends the enclosing method's stretch, which is
	/// exactly how the JIT sees it too.
	/// </para>
	/// <para>
	/// A warning, not an error: the parser is correct and merely compiled the way a method
	/// is on its first call, several times slower. The message carries the numbers the
	/// generator acted under, because the remedy is chosen against them.
	/// </para>
	/// </remarks>
	static void Oversee(
		string file, RuleSymbol? anchor, ICollection<GramDiagnostic>? diagnostics)
	{
		if (diagnostics is null)
			return;

		var name = default(string);
		var bodyStart = 0;

		// Inspect slices of the emitted file. Splitting every line and rebuilding each
		// method body duplicates a large fraction of the SQL output just for warnings.
		for (var at = 0; at <= file.Length;)
		{
			var end = file.IndexOf('\n', at);

			if (end < 0)
				end = file.Length;

			var header = MethodHeader(file, at, end);

			if (header is not null)
			{
				Warn(at);
				name = header;
				bodyStart = end < file.Length ? end + 1 : end;
			}

			if (end == file.Length)
				break;

			at = end + 1;
		}

		Warn(file.Length);

		void Warn(int end)
		{
			if (name is not null && Machine.Branches(file, bodyStart, end) is var cost && cost > 2000)
			{
				var at = anchor?.Declaration?.At ?? default;

				diagnostics.Add(new GramDiagnostic(
					Machine.Unoptimized,
					$"The generated method '{name}' is estimated at {cost} basic blocks; " +
					"past about 2000, the JIT compiles a method without optimization and " +
					"this one will run several times slower than it needs to. The " +
					"generator divides what it can under a budget of 1500 per method; " +
					"what is left this large cannot be divided — usually one rule or one " +
					"alternative whose compiled body is over the budget by itself. " +
					"Splitting that rule restores optimization.",
					at.Position, at.Length, GramSeverity.Warning));
			}
		}
	}

	/// <summary>The name a line declares a method or local function under, if it does.</summary>
	static string? MethodHeader(string text, int start, int end)
	{
		while (start < end && char.IsWhiteSpace(text[start]))
			start++;

		foreach (var opening in Openings)
		{
			if (end - start < opening.Length ||
				string.CompareOrdinal(text, start, opening, 0, opening.Length) != 0)
			{
				continue;
			}

			var first = start + opening.Length;
			var at = first;

			while (at < end && (char.IsLetterOrDigit(text[at]) || text[at] == '_'))
				at++;

			if (at > first && at < end && text[at] == '(')
				return text.Substring(first, at - first);
		}

		return null;
	}

	static readonly string[] Openings =
	[
		"static int ", "static bool ", "static void ", "static string ",
		"internal static int ", "internal static bool ", "internal static void ",
		"public static ", "private static ",
		// The reader's members, and the reader's own constructor.
		"public int ", "internal ",
		"int ", "bool ", "void ",
	];

	/// <param name="partSize">
	/// How large a divided recognizer's parts should be aimed to be, or null for the
	/// measured default. A wish rather than a requirement — see <c>Machine.PartSize</c>.
	/// </param>
	/// <param name="lexical">
	/// The split this graph is the syntactic half of, where it is one. Then the file carries
	/// a lexical machine as well, the seam is skipped rather than woven, and a position is a
	/// token — so the text a value is cut from travels beside the kinds rather than being what
	/// is read (docs/design/lexical-adt-design.md).
	/// </param>
	public static string Emit(
		RecognitionGraph graph, string className, string? @namespace = null, ILineMap? lines = null,
		ICollection<GramDiagnostic>? diagnostics = null, int? partSize = null,
		LexicalSplit? lexical = null, bool direct = true, CarrierKind carrier = CarrierKind.Tape,
		int stacks = 0, string? suffix = null, bool? shared = null, bool inherits = false,
		string? languageId = null, string? languageSource = null,
		string? languageClassifications = null, string? languageRecognitionContract = null,
		IReadOnlyList<string>? statics = null, string? grammarSource = null, bool suffixDeclared = false,
		ValueStorageKind valueStorage = ValueStorageKind.Auto, bool bufferedInput = false, bool bufferedBytes = false, bool spanCaptures = false, bool prefixTables = true, ICollection<string>? sourceParts = null, int sourceFileSize = 0,
		int maxRetained = int.MaxValue, int bufferSize = 4096)
	{
		statics ??= [];

		var overKinds = lexical is not null;
		var directAllowed = direct;

		if (graph is null)
			throw new ArgumentNullException(nameof(graph));

		var file    = new Writer(0);
		var scope   = new Stack<IDisposable>();
		var scoped  = suffix is { Length: > 0 } ? className + "." + suffix : className;
		var results = new ResultTypes(graph, scoped, @namespace);
		// One machine per published rule. `parse R` and `find R` share one — the same rule,
		// two entry states — while two publications of different rules get one each, even
		// where both call a third rule, which is then compiled into both. `parse R with
		// (...)` twice needs no case of its own: `with` clones what it reaches, so what
		// reaches here is two rules.
		//
		// A grammar with nothing published is one machine over everything, which is what a
		// caller asking only for the recognizers wants.
		var groups   = Published(graph);
		var machines = new List<Compiled>();
		// Exact ordered diagnostic lists may be shared by sibling machines in this scope.
		// Single-machine parsers retain their existing names and need no shared registry.
		var expectedTables = groups.Count > 1 || bufferedInput || bufferedBytes ||
			graph.Publications.Any(static one => one.BufferedInput || one.BufferedBytes)
			? new Dictionary<string, (string Name, string Declaration)>(StringComparer.Ordinal)
			: null;

		// A machine's tag names everything it writes, and two machines sharing one write the same
		// names twice. The two the file keeps for itself — the second read of a terminal's value
		// and the seam — are named by what they are, and a rule may be called that too: a
		// grammar publishing a rule named `Value` wrote `Recognize_DotGram_Value_Expected0` twice.
		var tags = new HashSet<string>(StringComparer.Ordinal) { "_Value", "_Seam" };

		// What the graph says about readings that may not stand, asked once for every machine
		// left to choose its own carrier, and not at all where the author chose.
		var replay = carrier == CarrierKind.Auto ? Replay.Of(graph) : null;

		// A `find` over text tries every start and throws each failure away, and a parse of text in
		// memory reads quietly first where it may; so their machines are written to ask before they
		// record (Q7.2).
		var quiets = graph.Publications.Any(publication =>
			publication.Kind == PublishKind.Find ||
			publication.Kind == PublishKind.Parse && ReadsQuietlyFirst(graph));

		foreach (var group in groups)
		{
			var tag = groups.Count > 1 && group.Rule is not null ? "_" + IdentifierOf(group.Rule) : "";

			while (tag.Length > 0 && !tags.Add(tag))
				tag += "_";
			var only = groups.Count > 1 ? Reaches(graph, group.Rule) : null;
			var made = new Machine(
				graph, results, lines, Streaming(graph, overKinds), only, tag, partSize, overKinds,
				lexical?.Valued, carrier, stacks, lexical?.Inventory, replay, spanCaptures: spanCaptures, prefixTables: prefixTables, expectedTables: expectedTables, deferCompilation: groups.Count > 1, quiets: quiets);

			// Every publication of this rule needs none of the three things the arena is
			// for: no recursion, no backtracking, no deferred construction. Asked of one
			// machine's own publications now — a sibling that cannot lower no longer costs
			// this one its flat path (docs/next.md, "Future optimization gate") — and of
			// the rules this machine actually reaches: a recovery, a climb or a streamed
			// read elsewhere in the grammar is some other machine's business.
			var lowered = group.Publications.Count > 0 &&
				!RecoversWithin(graph, only) &&
				!ClimbsWithin(graph, only) &&
				!group.Publications.Any(publication => Streams(graph, publication, overKinds)) &&
				group.Publications.All(
					publication =>
						made.CanLower(publication.Rule, publication.Kind == PublishKind.Parse) ||
						made.CanLowerValued(publication.Rule, publication.Kind == PublishKind.Parse));

			made.Anchor = group.Publications.Count > 0 ? group.Publications[0].Rule : group.Rule;

			// Methods where the flat path cannot go and the engine need not: the rules this
			// machine reaches recognize and build nothing, and every publication reads the
			// whole input (Machine.Direct.cs).
			var asMethods = !lowered && directAllowed &&
				!group.Publications.Any(publication => Streams(graph, publication, overKinds)) &&
				made.CanDirect(group.Publications);

			machines.Add(new Compiled(
				made, group.Publications, "Recognize_DotGram" + tag, tag, lowered, asMethods));
		}

		// A publication whose rule another machine already reaches is read by that machine,
		// entered at its own rule, rather than by a second copy of everything it reaches.
		Joined(graph, machines, overKinds);
		// Share substantial overlap only when every member already requires the tape.
		// A small publication must not acquire a large sibling's parsing infrastructure.
		var reached = machines.ToDictionary(compiled => compiled, Rules);
		for (var host = 0; host < machines.Count; host++)
		{
			var owner = machines[host];
			if (!Eligible(owner)) continue;
			var union = new HashSet<RuleSymbol>(reached[owner]);
			var guests = new List<int>();
			var publications = owner.Publications.ToList();
			for (var guest = host + 1; guest < machines.Count; guest++)
			{
				var candidate = machines[guest];
				if (!Eligible(candidate) || candidate.Machine.BuildsDuringRecognition != owner.Machine.BuildsDuringRecognition) continue;
				var other = reached[candidate];
				if (union.Count(other.Contains) * 10 < Math.Max(union.Count, other.Count) * 9) continue;
				union.UnionWith(other);
				guests.Add(guest);
				publications.AddRange(candidate.Publications);
			}
			if (guests.Count == 0) continue;
			var made = new Machine(
				graph, results, lines, Streaming(graph, overKinds), graph.Rules.Where(union.Contains).ToArray(),
				owner.Tag, partSize, overKinds, lexical?.Valued, carrier, stacks, lexical?.Inventory,
				replay, spanCaptures: spanCaptures, prefixTables: prefixTables, expectedTables: expectedTables, deferCompilation: true, quiets: quiets);
			made.Anchor = owner.Machine.Anchor;
			if (!made.CanDirect(publications)) continue;
			machines[host] = owner with { Machine = made, Publications = publications };
			for (var guest = guests.Count - 1; guest >= 0; guest--)
				machines.RemoveAt(guests[guest]);
		}

		bool Eligible(Compiled compiled)
		{
			// A reading is passed by each publication at runtime, so different readings
			// may share the same rule bodies without sharing their selected version.
			return compiled.Direct && !compiled.Flat &&
				reached[compiled].Count >= 128 && compiled.Publications.All(publication =>
					!Streams(graph, publication, overKinds)) &&
				(carrier == CarrierKind.Tape || carrier == CarrierKind.Auto && replay is not null &&
					reached[compiled].Any(rule => results.QualifiedOf(rule) is not null && !replay.Keeps(rule)));
		}
		HashSet<RuleSymbol> Rules(Compiled compiled) => new(compiled.Publications.SelectMany(publication => Reaches(graph, publication.Rule)));

		foreach (var compiled in machines)
			compiled.Machine.CompileRules();

		AddBufferedMachines(graph, results, lines, machines, bufferedInput, bufferedBytes, overKinds, diagnostics, partSize, spanCaptures, prefixTables, expectedTables);

		// A second machine over the characters, for the terminals whose value the lexer
		// cannot carry — see `LexicalSplit.Valued`. It parses one token's text and builds
		// what that rule builds, which is the same code the character parser would have run
		// and is reached from the syntactic machine's materializer.
		//
		// Built here rather than beside the lexer because it has to join the file's value
		// tables: it writes into the same parser, and a machine numbers a type by where it
		// sits in one list they all agree on.
		var valuing = lexical is not null && (lexical.Valued.Count > 0 || Measuring(lexical).Count > 0)
			? new Machine(
				lexical.Source,
				new ResultTypes(lexical.Source, scoped, @namespace),
				lines,
				only: Rereads(lexical),
				tag: "_Value",
				quiets: true)
			: null;

		// Each terminal that builds is a root of its own, and a whole one: the text handed to
		// it is exactly what the lexer measured, so anything left over is the two machines
		// disagreeing rather than a longer match. Registered before the tables are gathered,
		// because compiling a root is what discovers the types under it.
		if (valuing is not null && lexical is not null)
		{
			foreach (var rule in lexical.Valued)
				valuing.Register(rule, whole: true);

			// And what measures the rest of a terminal the lexer only begins. Not whole: it is
			// entered where the beginning ended, and where it stops is the answer.
			foreach (var rule in Measuring(lexical))
				valuing.Register(rule, whole: false);
		}

		// Every machine numbers a value type by where it sits in one list, because the parser
		// they share holds one table per entry. The union is only knowable once they all
		// exist, so it is handed back to each of them before a line is rendered.
		var tables = ValueTables(machines, valuing);

		foreach (var compiled in machines)
			compiled.Machine.ShareValueTables(tables);

		valuing?.ShareValueTables(tables);
		Machine.ShareAdaptiveStores(machines.Where(static compiled => compiled.Direct).Select(static compiled => compiled.Machine), valueStorage);

		file.Line("// <auto-generated/>");
		file.Line("#nullable enable");

		// A dialect names its publications what the grammar it inherits named its own,
		// and both classes are whole parsers — so the derived one hides the base's
		// members, deliberately: `TransactSql.ParseSelect` reads T-SQL. The question
		// CS0108 asks is answered here, where the code it is about was written.
		if (inherits)
		{
			file.Line("#pragma warning disable CS0108 // a dialect hides what it inherits, on purpose");
		}

		file.Line();

		// A block namespace rather than a file-scoped one: the consumer's language
		// version is unknown (.claude/rules/emitted-code.md).
		if (@namespace is not null)
			scope.Push(file.Block($"namespace {@namespace}"));

		// The grammar's own `@using` directives and no others. Everything this file
		// generates is written with `global::`; these are here for the C# the grammar
		// supplied, which was written expecting them (§1). And the hosts of the grammars this
		// one includes, whether or not it has any of its own: they used to be written only
		// beside those, and a grammar that included another and said no `@using` called the
		// helpers of the included class by names nothing in its file could see.
		if (graph.CSharpImports.Count > 0 || statics.Count > 0)
		{
			foreach (var import in graph.CSharpImports)
				file.Line($"using {import};");

			// The host of every grammar this one is built on, so that a rule which came from
			// there goes on calling the helpers its author wrote. Two of them offering the
			// same name is an ordinary C# ambiguity and is refused as one; a member the
			// including class declares under a name an included grammar calls is used instead
			// of theirs, silently, which is the one collision nothing here can see.
			foreach (var import in statics)
				file.Line($"using static {import};");

			file.Line();
		}

		var namespaceEnd = file.Length;
		var classParts = className.Split('.');
		for (var i = 0; i < classParts.Length; i++)
		{
			// The grammar itself, so that a class in a referenced assembly can be included
			// without its `.gram` file being anywhere near. Only on the compilation that owns
			// the class: a `Suffix` puts a second reading in a nested class, and what an
			// including grammar names is the class, not the reading.
			if (i == classParts.Length - 1 && grammarSource is not null && suffix is not { Length: > 0 })
				file.Line($"[global::DotGram.GramSourceAttribute({Quoted(grammarSource)})]");

			if (i == classParts.Length - 1 && languageId is not null && languageSource is not null)
				file.Line(LanguageDescriptorAttribute(
					graph,
					languageId,
					languageSource,
					languageClassifications ?? "",
					languageRecognitionContract ?? ""));

			scope.Push(file.Block($"partial class {classParts[i]}"));
		}

		// A second compilation of the same grammar in the same host goes in a class of its
		// own, because a file's support — the match, the failure, the lexer, the value tables
		// — is written once and named for what it is, and two copies in one scope would
		// collide name for name. Written out here rather than declared by the author, so it
		// is `public static` and not the `private` a nested class would default to, and
		// `partial` so that the pooling hooks it declares have somewhere to be written.
		// The types the host's own C# names — the span a factory is handed, and the match a
		// caller reads — belong to the host and not to a compilation of it. Where there are
		// several they are written here, outside every scope, by the first: a nested class
		// reads them by their simple names, and a grammar whose `=>` hands a span to a
		// method of the host would otherwise be handing it a type of the same name that is
		// not the same type. Unconditionally, because what the others need is not known
		// here — a struct nothing names costs a compilation nothing.
		if (shared == true)
		{
			file.Write(SourceSpanStruct);
			file.Line();
			file.Write(OutcomeEnum);
			file.Line();
			file.Write(MatchStruct);
			file.Line();
			file.Write(ParserInputTypes);
			file.Line();
		}

		// Unless the author declared that class, in which case what the author wrote says who
		// may see it and this part says nothing: `internal static partial class Immediate { }`
		// beside `[GramOptions(Suffix = "Immediate")]` is a reading nobody outside can reach.
		if (suffix is { Length: > 0 })
			scope.Push(file.Block(suffixDeclared
				? $"static partial class {suffix}"
				: $"public static partial class {suffix}"));

		var separated = new List<string>();
		if (sourceParts is not null && sourceFileSize > 0)
			file.SeparateMethods = methods =>
			{
				if (methods.Length < sourceFileSize)
					return false;
				var part = new Writer(file.Depth);
				part.Write(methods);
				separated.Add(part.ToString());
				return true;
			};

		foreach (var compiled in machines)
			foreach (var publication in compiled.Publications)
			{
				if (compiled.Machine.BufferedInput)
				{
					EmitBufferedPublication(file, graph, results, compiled, publication);
					continue;
				}

				EmitPublication(
					file,
					publication,
					results,
					graph.Climbing.ContainsKey(publication.Rule),
					Streams(graph, publication, overKinds) &&
						!bufferedInput && !publication.BufferedInput,
					compiled.Flat,
					compiled.Machine.Ties,
					compiled.Machine.UsesInput,
					compiled.Machine.UsesContext ? graph.Context : null,
					overKinds,
					compiled.Direct && compiled.Machine.Probes,
					compiled.Machine.UsesReading ? publication.Reading : null,
					compiled.Direct,
					diagnostics,
					compiled.Tag,
					ReadsQuietlyFirst(graph));

				file.Line();
			}

		foreach (var rule in results.Built)
		{
			EmitResultType(file, graph, results, rule);
			file.Line();
		}

		var byteRules = new HashSet<RuleSymbol>(machines.Where(compiled => compiled.Machine.BufferedBytes)
			.SelectMany(compiled => compiled.Publications.SelectMany(publication => Reaches(graph, publication.Rule))));
		foreach (var rule in graph.Rules)
		{
			if (!graph.Types.ContainsKey(rule))
				continue;

			foreach (var factory in FactoriesOf(graph, results, rule))
			{
				EmitFactory(file, graph, rule, factory, results, lines, spanCaptures);
				if (byteRules.Contains(rule))
					EmitFactory(file, graph, rule, factory with { Method = factory.Method + "_Bytes" }, results, lines, true, true);
				file.Line();
			}
		}

		// And the ones the second read calls. A terminal that builds keeps its `=>` where the
		// captures it names still exist, which is over characters — the syntactic graph holds
		// that rule with no members at all, so nothing above would write these.
		if (valuing is not null && lexical is not null)
			foreach (var rule in Rereads(lexical))
			{
				if (!lexical.Source.Types.ContainsKey(rule))
					continue;

				foreach (var factory in FactoriesOf(lexical.Source, valuing.Results, rule))
				{
					EmitFactory(file, lexical.Source, rule, factory, valuing.Results, lines);
					file.Line();
				}
			}

		if (machines.Count > 0)
			foreach (var rule in graph.Rules)
			{
				var recoveries = RecoveriesIn(graph, results, rule);

				for (var found = 0; found < recoveries.Count; found++)
				{
					var (_, recovery, slot) = recoveries[found];

					if (recovery.Factory is not null && slot >= 0)
						EmitRecoveryFactory(
							file, results, rule, RecoveryMethod(rule, found), recovery, graph, slot);
				}
			}

		var continuationProbes = new Dictionary<(RuleSymbol Rule, int Stage), (string Name, int Entry)>();
		var streamedParts = new Dictionary<(RuleSymbol Rule, int Stage), (string Name, int Entry)>();
		var streamedSyncs = new Dictionary<RuleSymbol, (string Name, int Entry)>();

		foreach (var compiled in machines)
			EmitRecognizers(
				file, graph, results, compiled,
				continuationProbes, streamedParts, streamedSyncs, overKinds);


		// `parse` demands the input end. Asking the rule and then checking would leave it
		// unable to go back for a longer match — the check has to be inside the machine,
		// where failing it is an ordinary failure and the stack still has somewhere to
		// resume. So the rule's body is compiled again, with end-of-input on the end.
		foreach (var publication in graph.Publications)
		{
			if (publication.Kind != PublishKind.Parse)
				continue;

			// §6.3 over a reader. The parts that are not calls — `eof`, a separator, the
			// trivia normalization inserted — have no recognizer of their own, so each gets
			// one: the driver runs them in order and they have to be runnable one at a time.
			if (!bufferedInput && !publication.BufferedInput && Streams(graph, publication, overKinds) && StagesOf(graph, publication.Rule) is { } stages)
			{
				var parts = new List<string>(stages.Count);

				for (var i = 0; i < stages.Count; i++)
				{
					if (stages[i].Rule is not null)
					{
						parts.Add("");

						continue;
					}

					parts.Add(streamedParts[(publication.Rule, i)].Name);
				}

				// What the repetition was told to do about a bad element, and where the
				// parse may pick up again — compiled here rather than inside a machine,
				// because in a stream it is the driver that steps over one.
				// One, and one is all a streamed rule may mark: the driver steps over a
				// bad element as it hands the good ones back, and it is reading one
				// repetition at a time (GRAM4010, GrammarNormalizer.Checks.cs).
				var found   = RecoveryIn(graph, results, publication.Rule);
				var sync    = (string?)null;
				var factory = RecoveryMethod(publication.Rule, 0);

				if (found is not null)
					sync = streamedSyncs[publication.Rule].Name;

				var compiled = machines.Find(one => one.Publications.Contains(publication));

				string Recognizer(RuleSymbol rule)
				{
					// Joined publications already share the plain wrapper in this machine.
					if (compiled is not null && compiled.Publications.Any(one => one.Rule == rule))
						return MethodOf(rule);

					return MethodOf(graph, rule, compiled?.Machine.Anchor, compiled?.Tag ?? "");
				}

				EmitStreamingParse(
					file, graph, publication, results, stages, parts,
					found?.Recovery, sync, factory,
					stage => continuationProbes.TryGetValue((publication.Rule, stage), out var probe)
						? probe.Name
						: null,
					machines.Exists(static compiled => compiled.Machine.UsesInput),
					Recognizer);
				file.Line();
			}
		}

		// Every machine's, not one machine's: a materializer and its guards belong to the
		// machine that named them, and a file has one set per machine. Identical expected
		// arrays have shared names and are emitted once, by the first machine that uses them.
		var writtenExpected = expectedTables is null ? null : new HashSet<string>(StringComparer.Ordinal);

		foreach (var compiled in machines)
			foreach (var extra in compiled.Machine.Extras(writtenExpected))
			{
				file.Write(extra);
				file.Line();
			}
		// The source graph too, where there is one: a rule that names a span may have moved
		// into the second read, and the type it names is the file's either way.
		if (shared is null && (UsesSourceSpan(graph) || lexical is not null && UsesSourceSpan(lexical.Source)))
		{
			file.Write(SourceSpanStruct);
			file.Line();
		}

		if (shared is null && graph.Publications.Count > 0)
		{
			file.Write(OutcomeEnum);
			file.Line();
			file.Write(MatchStruct);
			file.Line();
		}

		if (machines.Count > 0)
		{
			file.Write(FailureStructWith(
				reach: graph.Recoveries.Count > 0 && Streaming(graph, overKinds),
				starved: Streaming(graph, overKinds),
				expected: true,
				// The machine that reads a terminal again is an engine too, and an engine
				// writes the ties it meets. It is not among `machines`, so a grammar whose
				// syntax was all flat declared a failure its lexical half could not compile.
				expectedMore: valuing is not null || machines.Exists(static compiled =>
					!compiled.Flat || compiled.Machine.Ties),
				recoveryOrdinal: graph.Publications.Any(publication => publication.YieldRecovery),
				// A reader's refusals inside a lookahead are counted on the failure (Refuse_DotGram).
				looking: machines.Exists(static compiled => compiled.Direct),
				// A reading whose failure nothing reads records none (Q7.2): a `find` over text, and the
				// lexer measuring or valuing a token again.
				quiet: quiets || valuing is not null));
			file.Line();
		}

		if (shared is null && (machines.Exists(static compiled => compiled.Machine.BufferedInput) ||
			graph.Bodies.Values.Any(body => NodeWalk.Descendants(body).Any(node => node is Node.External { UsesInputView: true }))))
			file.Write(ParserInputTypes);

		if (machines.Exists(static compiled => compiled.Machine.BufferedInput))
		{
			// What a streaming overload takes where its caller passes null: the grammar's own
			// [Gram(MaxRetained, BufferSize)], read-only here, overridden per call.
			file.Line("/// <summary>What a buffered parse retains at most where a call does not say ([Gram(MaxRetained)]).</summary>");
			file.Line($"internal const int DefaultMaxRetained = {maxRetained.ToString(System.Globalization.CultureInfo.InvariantCulture)};");
			file.Line("/// <summary>A buffered parse's initial capacity where a call does not say ([Gram(BufferSize)]).</summary>");
			file.Line($"internal const int DefaultBufferSize = {bufferSize.ToString(System.Globalization.CultureInfo.InvariantCulture)};");
			file.Line();
		}

		if (machines.Exists(static compiled => compiled.Machine.BufferedInput && !compiled.Machine.BufferedBytes))
			file.Write(BufferedTextClass);
		if (machines.Exists(static compiled => compiled.Machine.BufferedBytes))
			file.Write(BufferedByteClass());

		if (Streaming(graph, overKinds))
		{
			file.Write(WindowClass);
			file.Line();
			file.Write(LinesClass);
			file.Line();
		}

		if (Reporting(graph))
		{
			file.Write(RecoveredHook);
			file.Line();
		}

		if (Locating(graph))
		{
			file.Write(LocateHelper);
			file.Line();
		}

		// The lexical half: the machine that reads characters and answers with kinds, the
		// seam it skips between them, and the loop that puts the two together. Written here
		// rather than beside the publications because every publication of a split grammar
		// calls the same one.
		if (lexical is not null)
		{
			file.Write(Lexical(lexical, valuing));
			file.Line();
		}

		// One runtime for the file however many machines there are: the arena, the value
		// tables and the links are the parser's, not a machine's, and a machine that lowered
		// needs none of them. The tables have to be the union — every machine numbers a type
		// by where it sits in this list, so they must all be looking at the same list.
		if (machines.Exists(static compiled => compiled.Direct))
		{
			file.Write(DirectSupport.Replace("/*DEEPER*/", DeeperSpares.ToString(System.Globalization.CultureInfo.InvariantCulture)));
			file.Line();

			// Each carrier's own store, where its machines rent one: the tables the tape's
			// walk fills, or the stacks an immediate parse gathers on. Asked of the carrier
			// rather than switched on here, so that a carrier renting nothing — one that
			// keeps what it read in the reader — writes nothing, and two machines carrying
			// the same way write it once.
			Machine.ShareImmediateStacks(machines.Where(static compiled => compiled.Direct).Select(static compiled => compiled.Machine));
			Machine.ShareDirectStores(machines.Where(static compiled => compiled.Direct).Select(static compiled => compiled.Machine));
			var stores = new List<string>();

			foreach (var compiled in machines)
			{
				if (!compiled.Direct)
					continue;

				var store = compiled.Machine.CarrierStore(tables, graph.State);

				if (store.Length > 0 && !stores.Contains(store))
					stores.Add(store);
			}

			foreach (var store in stores)
			{
				file.Write(store);
				file.Line();
			}
		}

		// The engine's runtime — the pooled parser, its arena and the pooling hooks over it —
		// is written only where a machine runs on the engine, the valuing machine over the
		// characters included. A file rendered by methods throughout carries none of it:
		// the tape above is all those methods need, and a class a consumer cannot reach from
		// any parse is two hundred lines to compile for nothing. A host that had implemented
		// the hooks over such a file loses them with the class, which is right — what they
		// rented, nothing rents.
		if (machines.Exists(static compiled => !compiled.Flat && !compiled.Direct) || valuing is not null)
			file.Write(ParserRuntime(
				graph.Climbing.Count > 0,
				machines.Exists(static compiled => compiled.Machine.Caches),
				machines.Exists(static compiled => compiled.Machine.UsesMarks),
				tables));

		// A carrier the author asked for and did not get, said once per reason and said here:
		// which carrier a machine took is settled by what it turned out to hold, and nothing
		// before the readers are written knows. Information and not a warning: the parser
		// that comes out is correct and is the one the tape would have written, and nothing
		// the author did is wrong — but a carrier chosen and silently not used is a
		// measurement about to be misread.
		if (carrier is not (CarrierKind.Tape or CarrierKind.Auto) && diagnostics is not null)
		{
			// Two ways to end up on the tape after asking not to be, and only one of them
			// used to be said. A carrier that refuses a machine says so. A machine with no
			// reader does not refuse the carrier — it never asks it — and the file that came
			// out was the same file byte for byte in silence. Asked only where *no* machine in
			// the grammar is read by methods: one `find` beside a `parse` is a machine on the
			// engine next to one the carrier is carrying, which is GRAM5005's subject.
			var reader  = machines.Exists(static one => one.Direct);
			var refused = new List<string>();

			foreach (var compiled in machines)
			{
				var why = compiled.Machine.CarrierRefusal ??
					(reader ? null : Unread(compiled, graph, overKinds, carrier));

				if (why is null || refused.Contains(why))
					continue;

				refused.Add(why);

				diagnostics.Add(new GramDiagnostic(
					GramCompiler.CarrierRefused,
					$"This grammar is carried on the tape rather than as {carrier}: {why}. " +
					"The parser is the one it would have been without the request.",
					0,
					0,
					GramSeverity.Info));
			}
		}
		else if (carrier == CarrierKind.Auto && diagnostics is not null)
		{
			// Left to the generator, which says what it chose (GRAM5012): the one thing a parse
			// carried immediately gives up is a parse that fails having run constructions, and
			// an author whose constructions mind has to be told the word that takes it back.
			// The machines kept on the tape where they could have been carried are said as one,
			// by the names the grammar gave its rules: a grammar is one thing to its author however
			// many machines it came out as, and a rule cloned by a `with` or a dialect is still the
			// rule that was written. Where nothing was chosen between — nothing is read by methods, nothing is
			// built, or the carrier would refuse — nothing is said.
			if (machines.Exists(static one => one.Direct && one.Machine.CarriesImmediately))
				diagnostics.Add(new GramDiagnostic(
					GramCompiler.CarrierChosen,
					"This grammar is carried as Immediate: nothing it builds is read for a derivation " +
					"that is then given up, so every construction runs where it is read and none waits " +
					"for a walk at the end (§3.7). A parse that fails may already have run the " +
					"constructions of what it read before failing; Carrier = GramCarrier.Tape holds " +
					"every one back until a parse has accepted.",
					0,
					0,
					GramSeverity.Info));

			var kept = machines
				.Where(static one => one.Direct)
				.Select(static one => one.Machine.KeptOnTape)
				.OfType<Machine.Kept>()
				.ToList();

			if (kept.Count > 0)
			{
				var building = Named(kept.SelectMany(static one => one.Building));
				var replayed = Named(kept.SelectMany(static one => one.Replayed));
				var again    = Named(kept.SelectMany(static one => one.Again));

				var why = replayed.Count > 0
					? $"{replayed.Count} of the {building.Count} rules it builds are read for derivations " +
						$"that may not stand — {Listed(replayed)}"
					: $"{Listed(again)} can be read again after answering, and what the first reading " +
						"built would stay built";

				diagnostics.Add(new GramDiagnostic(
					GramCompiler.CarrierChosen,
					$"This grammar is carried on the tape: {why}. Under Carrier = GramCarrier.Immediate " +
					"those constructions would run for readings that were then given up. A construction " +
					"that only builds does not mind, and for one that does not mind that carrier puts " +
					"down a walk of about two fifths of a parse.",
					0,
					0,
					GramSeverity.Info));
			}

			static List<string> Named(IEnumerable<RuleSymbol> rules) =>
				[.. rules.Select(static rule => rule.Declaration?.Name ?? rule.Name).Distinct(StringComparer.Ordinal)];

			static string Listed(List<string> names) =>
				string.Join(", ", names.Take(3)) + (names.Count > 3 ? " and " + (names.Count - 3) + " more" : "");
		}

		while (scope.Count > 0)
			scope.Pop().Dispose();

		// Every value table the machines named by its type, said as its number in the one
		// order they all share — which is only known now (Machine.TableName).
		var written = Numbered(file.ToString(), tables);

		// A mark stands for a state whose final name was not known when it was written, and
		// `Settle` puts the name in. One that reaches here would be a control character in
		// the consumer's source — so it is caught here, where the failure names the
		// generator, rather than there, where it names nothing.
		if (written.IndexOf('\u0001') >= 0)
			throw new InvalidOperationException(
				"A state mark reached the generated file, which means a body was written " +
				"and never settled (Machine.Graph.cs).");

		if (diagnostics is not null)
		{
			// Over kinds the language says a rule's answer stands (§4), and it is the
			// methods that say it. A machine the methods refused runs on the engine, which
			// backtracks into a rule that already matched — so the grammar is told which
			// rule cost it that, rather than reading one way and running the other.
			if (lexical is not null)
				foreach (var compiled in machines)
					if (!compiled.Direct && !compiled.Flat && compiled.Machine.Refusal is var (rule, why))
					{
						var at = (rule ?? compiled.Publications[0].Rule).Declaration?.At ?? default;

						diagnostics.Add(new GramDiagnostic(
							Machine.Backtracks,
							$"'{(rule ?? compiled.Publications[0].Rule).Name}' cannot be read by methods because " +
							$"{why}, so this grammar's syntactic half runs on the shared engine. There a choice " +
							"that has matched can be revisited when something later fails, which is what reading " +
							"characters means and not what reading kinds means (docs/syntax.md §4). The parse is " +
							"correct as ordered choice over characters; it is the committed reading the notation " +
							"promises over kinds that is not what runs.",
							at.Position, at.Length, GramSeverity.Warning));
					}

			foreach (var compiled in machines)
				foreach (var warning in compiled.Machine.Oversized)
					diagnostics.Add(warning);

			Oversee(
				written,
				machines.Count > 0 ? machines[0].Machine.Anchor : null,
				diagnostics);
		}

		if (separated.Count > 0)
		{
			// Repeat only the namespace/imports and partial declarations. Attributes,
			// fields, initializers and constructors remain in the original file.
			var header = new Writer(@namespace is null ? 0 : 1);
			var closing = new Stack<IDisposable>();
			foreach (var part in classParts)
				closing.Push(header.Block($"partial class {part}"));
			if (suffix is { Length: > 0 })
				closing.Push(header.Block(suffixDeclared ? $"static partial class {suffix}" : $"public static partial class {suffix}"));
			var opening = written.Substring(0, namespaceEnd) + header.ToString();
			var closeAt = header.Length;
			while (closing.Count > 0)
				closing.Pop().Dispose();
			var ending = header.ToString().Substring(closeAt) + (@namespace is null ? "" : "}" + Lines.Ending);
			foreach (var methods in separated)
			{
				var part = Numbered(opening + methods + ending, tables);
				if (part.IndexOf('\u0001') >= 0)
					throw new InvalidOperationException("An unsettled state mark reached a generated source part.");
				Oversee(part, machines.Count > 0 ? machines[0].Machine.Anchor : null, diagnostics);
				sourceParts!.Add(part);
			}
		}
		return written;
	}

	static void EmitEngine(Writer file, Machine machine, string engine)
	{
		var methods = machine.RenderEngine(engine, field => { file.Line(field); file.Line(); });
		file.Methods(methods);
	}

	/// <summary>The file with every value table said by its number rather than by its type.</summary>
	/// <remarks>
	/// Once, here, because only here is the order every machine agreed on known. A type the
	/// file keeps no table for is the generator's mistake, and it says so rather than writing
	/// a number that reads another table.
	/// </remarks>
	static string Numbered(string text, IReadOnlyList<string> tables)
	{
		var open = text.IndexOf(Machine.TableOpens);

		if (open < 0)
			return text;

		var into = new StringBuilder(text.Length);
		var from = 0;

		while (open >= 0)
		{
			var close  = text.IndexOf(Machine.TableCloses, open);
			var type   = text.Substring(open + 1, close - open - 1);
			var number = -1;

			for (var at = 0; at < tables.Count; at++)
			{
				if (tables[at] == type)
				{
					number = at;

					break;
				}
			}

			if (number < 0)
				throw new InvalidOperationException($"A value table for '{type}' was named, and the file keeps none.");

			into.Append(text, from, open - from).Append(number.ToString(System.Globalization.CultureInfo.InvariantCulture));

			from = close + 1;
			open = text.IndexOf(Machine.TableOpens, from);
		}

		return into.Append(text, from, text.Length - from).ToString();
	}

	static string LanguageDescriptorAttribute(
		RecognitionGraph graph,
		string languageId,
		string source,
		string classifications,
		string recognitionContract) =>
		"[global::DotGram.GramLanguageDescriptorAttribute(" +
		$"3, \"{EscapeString(languageId)}\", \"{Hash(source)}\", " +
		$"\"{Base64(source)}\", \"{Base64(Entries(graph))}\", \"{Base64(classifications)}\", " +
		$"\"{Base64(recognitionContract)}\")]";

	static string Entries(RecognitionGraph graph) => string.Join(
		"\n",
		graph.Publications.Select(publication =>
			publication.MethodName + "\t" + publication.Kind + "\t" + publication.Rule.Name));

	static string Base64(string value) =>
		Convert.ToBase64String(Encoding.UTF8.GetBytes(value));

	static string Hash(string value)
	{
		using var sha = SHA256.Create();
		var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(value));
		var text = new StringBuilder(bytes.Length * 2);

		foreach (var item in bytes)
			text.Append(item.ToString("x2"));

		return text.ToString();
	}

	static string EscapeString(string value) =>
		value.Replace("\\", "\\\\").Replace("\"", "\\\"");

	/// <summary>
	/// One directive (docs/syntax.md §6). <c>parse</c> makes an asserting method and an
	/// asking one; <c>find</c> makes a sequence.
	/// </summary>
	/// <remarks>
	/// No <c>out</c> parameters anywhere: what a match has to say goes in
	/// <see cref="MatchType"/>, where the next thing it has to say is a field rather than
	/// another parameter on every signature.
	/// </remarks>
	/// <summary>
	/// One machine's recognizers: the lowered methods where it lowered, and the engine, its
	/// wrappers and its probes where it did not.
	/// </summary>
	/// <remarks>
	/// Called once per published rule. The dictionaries are the file's rather than this
	/// machine's, because what fills them here is read again where the streaming entry
	/// points are written, and they are keyed by rule, which belongs to one machine.
	/// </remarks>
	static void EmitRecognizers(
		Writer file,
		RecognitionGraph graph,
		ResultTypes results,
		Compiled compiled,
		Dictionary<(RuleSymbol Rule, int Stage), (string Name, int Entry)> continuationProbes,
		Dictionary<(RuleSymbol Rule, int Stage), (string Name, int Entry)> streamedParts,
		Dictionary<RuleSymbol, (string Name, int Entry)> streamedSyncs,
		bool overKinds)
	{
		var machine = compiled.Machine;
		var engine  = compiled.Engine;

		if (machine.BufferedInput)
		{
			foreach (var publication in compiled.Publications)
				machine.Register(publication.Rule, whole: publication.Kind == PublishKind.Parse);
			EmitEngine(file, machine, engine);
			file.Write(machine.RenderScanners());
			foreach (var publication in compiled.Publications)
				file.Write(machine.RenderWrapper(publication.Rule, BufferedMethod(publication, machine.BufferedBytes), engine, whole: publication.Kind == PublishKind.Parse));
			return;
		}

		// This machine's own, unlike the dictionaries: what it holds is read only here, to
		// decide which rules need a wrapper besides the published ones. Shared, a machine
		// would try to wrap a rule that belongs to another and does not exist in this one.
		var streamedRules = new HashSet<RuleSymbol>();

		// What this machine added, kept apart from what the file holds: the
		// dictionaries are read again where the streaming entry points are written, so
		// every machine writes into them, but each may only emit its own — a probe
		// belongs to the states it probes.
		var mine = new List<(string Name, int Entry, bool Sync)>();

		if (compiled.Direct)
		{
			file.Methods(machine.RenderReader(compiled.Publications));

			return;
		}

		if (compiled.Flat)
		{
			var rendered = new HashSet<(RuleSymbol Rule, bool Whole)>();

			foreach (var publication in compiled.Publications)
			{
				var whole = publication.Kind == PublishKind.Parse;

				if (!rendered.Add((publication.Rule, whole)))
					continue;

				// Under the name the caller uses, with no wrapper between: the valueless
				// form has nothing for a wrapper to do, and the valued form carries the
				// same `out` parameter the wrapper would otherwise have added.
				var name = whole ? WholeOf(publication.Rule) : MethodOf(publication.Rule);

				file.Write(machine.CanLower(publication.Rule, whole)
					? machine.RenderFlat(publication.Rule, name, whole)
					: machine.RenderFlatValued(publication.Rule, name, whole));
				file.Line();
			}

			// The scanners the flat states call are methods of their own, the same as
			// when the engine calls them.
			var flatScanners = machine.RenderScanners();

			if (flatScanners.Length > 0)
				file.Write(flatScanners);
		}



		if (!compiled.Flat && !compiled.Direct)

		{

			foreach (var publication in compiled.Publications)

			{

				machine.Register(publication.Rule, publication.Kind == PublishKind.Parse);



				if (publication.Kind != PublishKind.Parse || !Streams(graph, publication, overKinds) ||

					StagesOf(graph, publication.Rule) is not { } stages)

					continue;



				for (var stage = 0; stage < stages.Count; stage++)

				{

					// The seam of a spaced collection is read by the driver between the
					// elements it hands over, so it needs a recognizer under its own name.
					if (stages[stage].Seam is { } seamRule)
					{
						streamedRules.Add(seamRule);
						machine.Register(seamRule, whole: false);
					}

					if (stages[stage].Rule is { } stagedRule)

					{

						streamedRules.Add(stagedRule);

						machine.Register(stagedRule, whole: false);

					}

					else

					{

						var part = WholeOf(publication.Rule) + "_Part" + stage;

						var at = machine.Register(stages[stage].Node);

						streamedParts[(publication.Rule, stage)] = (part, at);
						mine.Add((part, at, Sync: false));
					}



					if (!stages[stage].Repeated || continuationProbes.ContainsKey((publication.Rule, stage)))

						continue;



					var suffix = new List<Node>(stages.Count - stage - 1);



					for (var after = stage + 1; after < stages.Count; after++)

						suffix.Add(stages[after].Node);



					// A parse continuation includes the implicit end-of-input check.
					suffix.Add(new Node.Lookahead(false, new Node.Element(true, [], [], [])));

					var continuation = suffix.Count switch

					{

						0 => new Node.Empty(),

						1 => suffix[0],

						_ => new Node.Sequence(suffix),

					};

					var probe = WholeOf(publication.Rule) + "_Continue" + stage;



					var entry = machine.Register(continuation);

					continuationProbes[(publication.Rule, stage)] = (probe, entry);
					mine.Add((probe, entry, Sync: false));
				}



				if (RecoveryIn(graph, results, publication.Rule) is { } recoveryFound &&

					!streamedSyncs.ContainsKey(publication.Rule))

				{

					var sync = WholeOf(publication.Rule) + "_Sync";

					var at = machine.Register(recoveryFound.Recovery.Sync);

					streamedSyncs[publication.Rule] = (sync, at);
					mine.Add((sync, at, Sync: true));
				}

			}



			EmitEngine(file, machine, engine);

			file.Line();

			var scanners = machine.RenderScanners();

			if (scanners.Length > 0)
				file.Write(scanners);



			var wrappers = new HashSet<(RuleSymbol Rule, bool Whole)>();



			foreach (var publication in compiled.Publications)

			{

				var whole = publication.Kind == PublishKind.Parse;



				if (!wrappers.Add((publication.Rule, whole)))

					continue;



				file.Write(machine.RenderWrapper(

					publication.Rule,

					whole ? WholeOf(publication.Rule) : MethodOf(publication.Rule),

					engine,

					whole));

				file.Line();

				// And the same rule entered where the caller says it begins, which is what a
				// positional overload calls. The state is already a root — `Register` adds the
				// one that demands no end whatever the publication asked for — so this is a
				// wrapper over it and nothing more.
				if (whole && wrappers.Add((publication.Rule, false)))
				{
					file.Write(machine.RenderWrapper(
						publication.Rule, MethodOf(publication.Rule), engine, whole: false));

					file.Line();
				}
			}



			if (compiled.Publications.Count == 0)

				foreach (var rule in graph.Rules)

				{

					file.Write(machine.RenderWrapper(rule, MethodOf(rule), engine, whole: false));

					file.Line();

				}



			foreach (var rule in streamedRules)

				if (wrappers.Add((rule, false)))

				{

					file.Write(machine.RenderWrapper(
						rule,
						MethodOf(graph, rule, machine.Anchor, compiled.Tag),
						engine,
						whole: false));

					file.Line();

				}



			foreach (var probe in mine)
			{
				var entry = machine.Numbered(probe.Entry);

				file.Write(probe.Sync
					? Machine.RenderSyncProbe(
						probe.Name, engine, entry, graph.Climbing.Count > 0, machine.UsesInput)
					: Machine.RenderProbe(
						probe.Name, engine, entry, graph.Climbing.Count > 0, machine.UsesInput));
				file.Line();
			}
		}
	}

	/// <param name="direct">
	/// Whether this machine's rules are read by methods rather than by the engine. Together
	/// with <paramref name="flat"/> it says which of the three renderings stands behind this
	/// publication, and so which entries there are to call — a question the published methods
	/// have to ask now that a rule may be entered at a position as well as at zero.
	/// </param>
	/// <param name="diagnostics">
	/// Where a remark about this publication goes. Null where the caller wants none; an
	/// overload that is not offered says so here, as §6.3's reader overload does.
	/// </param>
	static void EmitPublication(
		Writer file, Publication publication, ResultTypes results, bool climbs, bool streams, bool flat,
		bool ties, bool input, string? context, bool overKinds = false, bool probes = false,
		int? reading = null, bool direct = false, ICollection<GramDiagnostic>? diagnostics = null,
		string tag = "", bool quietFirst = false)
	{
		// The grammar's own state (§7.7), where anything in this machine names it. The
		// caller makes one and hands it over; a grammar that declares none, or declares one
		// and never names it, is published exactly as it was before the name existed.
		var takes = context is null ? "" : $", {context} context";
		var gives = context is null ? "" : ", context";

		// Which reading of the grammar this publication is (`when … is …`), where the machine it
		// shares with the others asks. A number and not an argument the caller writes: the
		// parser is the reading, and nothing about it is chosen while it runs.
		var numbered = reading is { } number ? $", {number}" : "";

		var method = publication.MethodName;
		// What the author called it, which is not always what the rule is called by now: a
		// publication carrying `with (...)` publishes a clone of the rule, and `Lambda_With1`
		// is a name that appears nowhere in the grammar and should appear nowhere a reader
		// looks — not in the summary, and not in what a refusal says.
		var name   = publication.Rule.Declaration?.Name ?? publication.Rule.Name;
		var built  = results.QualifiedOf(publication.Rule);
		var value  = publication.ResultType is { } contract ? contract.Name + (contract.IsSequence ? "[]" : "") : built ?? "string";
		var match  = $"{MatchType}<{value}>";

		// A rule that builds hands its value back through the recognizer; one that does
		// not leaves the extent it matched, and the text is cut from the input.
		// A rule of binding powers is asked at strength 0, which admits all of it (§4.3.1).
		var hands = (climbs ? ", 0" : "") +
			(built is null ? ", ref failure" : ", ref failure, out var recognized") +
			(input ? ", input" : "") + (overKinds ? ", source, starts, lengths" : "") + gives + numbered +
			(probes ? ", parserWhole" : "");

		// The same call from a window, where there is no whole input to hand over — and no
		// rule under it that could ask for one, because a publication whose rules do is
		// refused a stream (Retention, GRAM5001).
		//
		// The context is handed over here as everywhere else. It is the caller's object and
		// says nothing about how much input is held, so a window changes nothing about it —
		// unlike the input itself, which a window by definition does not have. Leaving it
		// out was a recognizer called with one argument short, in generated code.
		var streamedHands = (climbs ? ", 0" : "") +
			(built is null ? ", ref failure" : ", ref failure, out var recognized") +
			(input ? ", null!" : "") + gives + numbered +
			(probes ? ", default" : "");

		// Over kinds a position is a token, so what a publication hands back has to be cut
		// from the text the tokens came from rather than from what the machine was reading —
		// the same care the machine takes for a capture, taken once more at the edge.
		string Recognized(string from, string to) =>
			built is not null ? "recognized" :
			overKinds        ? $"Text_DotGram{tag}(source, starts, lengths, {from}, {to})" :
			$"input.Substring({from}, {to})";

		// Where the reading begins, which recognizer reads it, and what the match then says
		// about where it was. Named rather than written inline because the same body writes
		// the form that begins where it was told: there the position is the caller's, the
		// recognizer is the one that demands no end, and the extent is measured from it.
		var begins   = "0";
		var reader   = WholeOf(publication.Rule);
		var position = "0";
		var extent   = overKinds ? "over" : "end";

		if (publication.Kind == PublishKind.Yield)
		{
			EmitYield(file, publication, hands, takes);
			return;
		}

		if (publication.Kind == PublishKind.Find)
		{
			EmitFind(file, publication, method, name, value, match, hands, Recognized, takes);

			if (streams)
			{
				file.Line();

				EmitStreamingFind(
					file, publication, method, name, match, streamedHands, built is not null, takes);
			}

			return;
		}

		file.Line($"/// <summary>Parses the whole input as <c>{name}</c>.</summary>");
		file.Line("/// <exception cref=\"global::System.FormatException\">");
		file.Line($"/// The input is not <c>{name}</c>. <c>Try{method}</c> answers instead.");
		file.Line("/// </exception>");

		using (file.Block($"{AccessOf(publication)} static {value} {method}(string input{takes})"))
		{
			file.Line($"var match = Try{method}(input{gives});");
			file.Line();
			file.Line("if (match.IsSuccess)");
			file.Then("return match.Value;");
			file.Line();
			file.Line("throw new global::System.FormatException(match.Error + \" at \" + match.Position.ToString());");
		}

		file.Line();
		file.Line($"/// <summary>Parses the whole input as <c>{name}</c>, answering rather than throwing.</summary>");

		Asking("string input", positional: false);

		// And the same rule read where the caller says it begins. No end of input is
		// demanded — what comes back says how far the reading got — which is what lets a
		// host read one piece of a text it is already holding, with every position in it
		// still meaning what it meant.
		//
		// Offered where the rules are read by the engine or by methods. A lowered publication
		// was compiled with the single entry a whole parse asks for — its rules were proved to
		// need nothing more only against the end of the input — so it has no entry for these
		// to call. §6.3 says so, and the grammar is not told on every compilation: the
		// overload's absence is plain where it is called, and nothing in the grammar is wrong.
		if (!flat)
		{
			file.Line();
			file.Line($"/// <summary>Reads a <c>{name}</c> beginning at <paramref name=\"at\"/>.</summary>");
			file.Line("/// <remarks>");
			file.Line("/// The input is not required to end there: what comes back says where the");
			file.Line("/// reading began and how far it got, so a caller may go on from it. A position");
			file.Line("/// is an offset into the text" +
				(overKinds ? ", and a reading begins at a token, so one has to begin there." : "."));
			file.Line("/// </remarks>");

			begins   = overKinds ? "from" : "at";
			reader   = MethodOf(publication.Rule);
			position = "at";
			extent   = overKinds ? "over - at" : "end - at";

			Asking("string input, int at", positional: true);

			// And read inside a window: from `at`, and seeing nothing past `at + length`. What
			// a host holding a text reads a piece of it with when the piece is not where a
			// token of the whole text begins — the hole of an interpolated string, the body
			// of a template — and when what follows the piece is not this language at all.
			file.Line();
			file.Line($"/// <summary>Reads a <c>{name}</c> inside a window of the input.</summary>");
			file.Line("/// <remarks>");
			file.Line("/// The reading begins at <paramref name=\"at\"/> and sees no character from");
			file.Line("/// <c>at + length</c> on; it is not required to reach that far, and what comes back");
			file.Line("/// says how far it got. Positions are offsets into the whole input." +
				(overKinds
					? " Only the window is cut into tokens, so a reading may begin where no token of"
					: ""));
			if (overKinds)
			{
				file.Line("/// the whole input does, and a character no token begins with ends the tokens");
				file.Line("/// there rather than refusing the reading.");
			}
			file.Line("/// </remarks>");

			begins = overKinds ? "0" : "at";

			Asking("string input, int at, int length", positional: true, windowed: true);
		}

		void Asking(string parameters, bool positional, bool windowed = false)
		{
			// The positional form takes a parameter called `at`, so what the body would have
			// called the place it stopped is called something else there. Only there: the
			// whole form is emitted exactly as it always was.
			var halt = positional ? "halted" : "at";

			using (file.Block($"{AccessOf(publication)} static {match} Try{method}({parameters}{takes})"))
			{
				// A window is refused before anything is read where it does not lie in the text.
				if (windowed)
				{
					using (file.Block("if (at < 0 || length < 0 || at > input.Length - length)"))
						file.Line(
							$"return {match}.Failed({OutcomeType}.NoMatch, " +
							"\"The window \" + at.ToString() + \"..\" + (at + length).ToString() + " +
							"\" is outside the input.\", at, null, null);");

					file.Line();
				}

				// A split grammar reads its input twice: once into kinds and once as kinds. What
				// the caller hands over is a string either way — the two halves are the parser's
				// business and not theirs.
				if (overKinds)
				{
					file.Line("var source = input;");
					file.Line(windowed
						? "var tokens = Tokenize_DotGram(source, at, at + length);"
						: "var tokens = Tokenize_DotGram(source);");
					file.Line();
					file.Line("var starts  = tokens.Starts;");
					file.Line("var lengths = tokens.Lengths;");
					file.Line("var count   = tokens.Count;");
					file.Line();

					// Inside a window a character no token begins with is where the tokens end:
					// what the caller asked for is a reading of what can be read from `at`, and
					// the piece of text after it is commonly not this language at all.
					if (!windowed)
					{
						using (file.Block("if (tokens.Stopped >= 0)"))
						{
							file.Line($"var {halt} = tokens.Stopped;");
							file.Line();
							file.Line("Recycle_DotGram(tokens);");
							file.Line();

							// The lexer stopped where no token begins, and the character standing
							// there is what there is to say: the syntactic half never ran, so it has
							// no expectation to offer. A character a message could not show plainly
							// — a control, the quote around it, a backslash — is shown by its code.
							file.Line($"var stopped = {halt} < source.Length ? source[{halt}] : '\\0';");
							file.Line();
							file.Line(
								$"return {match}.Failed({OutcomeType}.NoMatch, " +
								$"{halt} >= source.Length ? \"Expected more input.\" : " +
								"\"Unexpected character '\" + " +
								"(global::System.Char.IsControl(stopped) || stopped == '\\'' || stopped == '\\\\' " +
								"? \"\\\\u\" + ((int)stopped).ToString(\"X4\", global::System.Globalization.CultureInfo.InvariantCulture) " +
								$": stopped.ToString()) + \"'.\", {halt}, null, null);");
						}

						file.Line();
					}

					// Out here a position is an offset into the text; in there it is a token.
					// A reading has to begin where one begins, and where none does the caller
					// named a place inside a token or past the end.
					if (positional && !windowed)
					{
						// Named with this machine's tag, as everything it writes is: two machines
						// in one class must not collide (Machine.Provenance).
						file.Line($"var from = TokenAt_DotGram{tag}(starts, count, at);");
						file.Line();

						using (file.Block("if (from < 0)"))
						{
							file.Line("Recycle_DotGram(tokens);");
							file.Line();
							file.Line(
								$"return {match}.Failed({OutcomeType}.NoMatch, " +
								"\"No token begins at \" + at.ToString() + \".\", at, null, null);");
						}

						file.Line();
					}
				}
				else if (positional && !windowed)
				{
					// The same refusal over characters, where a position is already one.
					using (file.Block("if (at < 0 || at > input.Length)"))
						file.Line(
							$"return {match}.Failed({OutcomeType}.NoMatch, " +
							"\"Position \" + at.ToString() + \" is outside the input.\", at, null, null);");

					file.Line();
				}

				// Fully qualified, and as a static call rather than an extension method:
				// the emitted file carries no usings at all (.claude/rules/emitted-code.md).
				file.Line(
					overKinds
						? "var text    = new global::System.ReadOnlySpan<char>(tokens.Kinds, 0, count);"
						: windowed
							? "var text    = global::System.MemoryExtensions.AsSpan(input, 0, at + length);"
							: "var text    = global::System.MemoryExtensions.AsSpan(input);");

				// The same characters again, in the one shape that may be handed to another
				// thread: what a reading that runs the stack low goes on with.
				if (probes)
					file.Line(
						overKinds
							? "var parserWhole = new global::System.ReadOnlyMemory<char>(tokens.Kinds, 0, count);"
							: windowed
								? "var parserWhole = global::System.MemoryExtensions.AsMemory(input, 0, at + length);"
								: "var parserWhole = global::System.MemoryExtensions.AsMemory(input);");

				// Carried through every recognizer this call reaches, so that what comes back
				// is the furthest the input was followed and not merely "no".
				file.Line(quietFirst
					? $"var failure = new {FailureType} {{ Quiet = true }};"
					: $"var failure = new {FailureType}();");
				file.Line();
				file.Line($"var end = {reader}(text, {begins}{hands});");
				file.Line();

				// A reading that records nothing is the fast one, and most input is accepted by
				// it. Where it refused, the same reading is run again recording, and what that one
				// says is the refusal — the one a single recording reading would have given, as
				// nothing the first did depended on what it did not record (Q7.2).
				if (quietFirst)
				{
					using (file.Block("if (end < 0)"))
					{
						file.Line($"failure = new {FailureType}();");
						file.Line(
							$"end     = {reader}(text, {begins}" +
							$"{hands.Replace("out var recognized", "out recognized")});");
					}

					file.Line();
				}

				file.Line("if (end < 0)");
				using (file.Block(""))
				{
					// Nothing is built here. What the arrays recorded is handed over as it
					// stands, and `Match<T>.Error` merges and words it if anybody asks —
					// a caller that only wants to know whether the input matched pays for
					// none of it. `.NET`'s own `Group.Value` is the same bargain from the
					// other side: it stores where a capture was and cuts the string on
					// access. A flat recognizer without checkpoint sites never reaches a
					// tie at all (Machine.Flat.cs's own Fail:), so it has no second array
					// to hand over; one with them accumulates ties the way the engine does.
					//
					// The one thing chosen here rather than there is which literal stands in
					// when nothing named what would have fit: only this end knows how far the
					// input went, and both answers are literals, so choosing costs a branch
					// and no allocation at all. The same test says which outcome this is
					// (§7.5), which is why the two are read off one comparison.
					file.Line("var starved = failure.OutOfInput == failure.Position + 1 || failure.Position >= text.Length;");
					file.Line();
					file.Line("var otherwise = starved");
					file.Then("? \"Expected more input.\"");
					file.Then($": \"Input does not match '{name}'.\";");
					file.Line();
					if (overKinds)
					{
						// Past the last token of a window is where its tokens ended: where the lexer
						// stopped inside it, or its edge.
						file.Line(windowed
							? $"var {halt} = failure.Position < count ? starts[failure.Position] : " +
								"tokens.Stopped >= 0 ? tokens.Stopped : at + length;"
							: $"var {halt} = failure.Position < count ? starts[failure.Position] : source.Length;");
						file.Line();
						file.Line("Recycle_DotGram(tokens);");
						file.Line();
					}

					file.Line(
						$"return {match}.Failed(" +
						$"starved ? {OutcomeType}.Starved : {OutcomeType}.NoMatch, " +
						"otherwise, " +
						// Against the token count and not the array's length: the caller sizes the
						// arrays for the worst case and fills the front of them, so past the count
						// they hold zeros — and a refusal at the end came back as a refusal at the
						// beginning, which the character parser next door reported correctly. The
						// count is the length of the kinds, there being one character a token.
						(overKinds ? $"{halt}, " : "failure.Position, ") +
						"failure.Expected, " +
						(flat && !ties ? "null);" : "failure.ExpectedMore);"));
				}
				file.Line();
				if (overKinds)
				{
					// Both read the arrays, so both are worked out before the set goes back.
					file.Line($"var whole = {Recognized(begins, "end")};");
					file.Line("var over  = end == 0 ? 0 : starts[end - 1] + lengths[end - 1];");
					file.Line();
					file.Line("Recycle_DotGram(tokens);");
					file.Line();
					file.Line($"return {match}.Success(whole, {position}, {extent});");
				}
				else
				{
					file.Line($"return {match}.Success({Recognized(begins, extent)}, {position}, {extent});");
				}
			}
		}
	}

	/// <summary>
	/// The lexical half of a split grammar: the scanner, the seam, and the loop.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Three things and none of them new. <c>LexerEmitter</c> writes the machine that reads
	/// the patterns together; §4.5's <c>trivia</c> is already an atomic-braced rule that
	/// compiles to a scanner with nothing written down, so it is asked for rather than
	/// recognized again; and the loop between them is what a tokenizer is.
	/// </para>
	/// <para>
	/// Whitespace is skipped and not reported, which is why it is not a pattern. Making it
	/// one was tried: the subset construction crosses a comment's <c>any</c> with every atom
	/// of every other pattern, and it ran for ten minutes without finishing.
	/// </para>
	/// </remarks>
	static string Lexical(LexicalSplit lexical, Machine? valuing)
	{
		var file = new Writer(0);

		file.Write(LexerEmitter.Emit(lexical.Inventory.Machine!));
		file.Line();

		var seam     = Seam(lexical);
		var skipping = lexical.Trivia
			.Select(rule => (Rule: rule, Name: seam.Scanner(rule)))
			.FirstOrDefault(one => one.Name is not null);

		foreach (var extra in seam.Extras())
		{
			file.Write(extra);
			file.Line();
		}

		file.Write(seam.RenderScanners());

		if (valuing is not null)
			file.Write(Rereading(lexical, valuing));

		file.Line("/// <summary>What a tokenized parse reads: the kinds, and where each one was.</summary>");
		file.Line("/// <remarks>");
		file.Line("/// Three arrays and a count, kept for the next parse on this thread the way the");
		file.Line("/// parser itself is. Allocated afresh they were the whole of what a split grammar");
		file.Line("/// cost over a character one on a short input — three allocations sized to the");
		file.Line("/// input, against a parse that reads a dozen tokens.");
		file.Line("/// </remarks>");
		file.Line("/// <remarks>");
		file.Line("/// Sized by the tokens there turn out to be rather than by the characters there");
		file.Line("/// are. A token is several characters, so the input's length is a bound four or");
		file.Line("/// more times what a document needs: three and three-quarter megabytes of SQL");
		file.Line("/// asked for thirty-eight of arrays and used nine. The guess starts at a quarter");
		file.Line("/// of the input and doubles, which for ordinary text never grows at all.");
		file.Line("/// </remarks>");

		using (file.Block("sealed class Tokens_DotGram"))
		{
			file.Line("internal char[] Kinds   = new char[0];");
			file.Line("internal int[]  Starts  = new int[0];");
			file.Line("internal int[]  Lengths = new int[0];");
			file.Line("internal int    Count;");
			file.Line("internal int    Stopped;");
			file.Line();

			using (file.Block("internal void Room(int length)"))
			{
				file.Line("if (Kinds.Length >= length)");
				file.Then("return;");
				file.Line();
				file.Line("Kinds   = new char[length];");
				file.Line("Starts  = new int[length];");
				file.Line("Lengths = new int[length];");
			}

			file.Line();
			file.Line("/// <summary>Room for one more, keeping what is already written.</summary>");

			using (file.Block("internal void Grow(int count)"))
			{
				file.Line("if (Kinds.Length > count)");
				file.Then("return;");
				file.Line();
				file.Line("var size = Kinds.Length < 16 ? 16 : Kinds.Length * 2;");
				file.Line();
				file.Line("global::System.Array.Resize(ref Kinds,   size);");
				file.Line("global::System.Array.Resize(ref Starts,  size);");
				file.Line("global::System.Array.Resize(ref Lengths, size);");
			}
		}

		file.Line();
		file.Line("/// <summary>The last set this thread used — one slot, taken out while in use.</summary>");
		file.Line("/// <remarks>");
		file.Line("/// Taken out rather than shared, so a parse reached from inside another — a guard");
		file.Line("/// that parses, a value that does — gets its own. Ordinary buffers are reused;");
		file.Line("/// stores exceeding the retained-capacity budget are left for collection.");
		file.Line("/// </remarks>");
		file.Line("[global::System.ThreadStatic]");
		file.Line("static Tokens_DotGram? _spareTokens;");
		file.Line();
		file.Line("/// <summary>Spares below the one slot, for parses reached from inside others; made the first time one is.</summary>");
		file.Line("[global::System.ThreadStatic]");
		file.Line("static Tokens_DotGram?[]? _deeperTokens;");
		file.Line();
		file.Line("[global::System.ThreadStatic]");
		file.Line("static int _deeperTokenCount;");
		file.Line();

		using (file.Block("static Tokens_DotGram Rented_DotGram()"))
		{
			file.Line("var spare = _spareTokens;");
			file.Line();
			file.Line("if (spare != null)");
			file.Then("_spareTokens = null;");

			using (file.Block("else if (_deeperTokenCount > 0)"))
			{
				file.Line("spare = _deeperTokens![--_deeperTokenCount]!;");
				file.Line("_deeperTokens![_deeperTokenCount] = null;");
			}

			file.Line("else");
			file.Then("return new Tokens_DotGram();");
			file.Line();
			file.Line("return spare;");
		}

		file.Line();

		using (file.Block("static void Recycle_DotGram(Tokens_DotGram tokens)"))
		{
			file.Line("if ((long)tokens.Kinds.Length + tokens.Starts.Length + tokens.Lengths.Length > 1048576)");
			file.Then("return;");
			file.Line();
			file.Line("if (_spareTokens == null)");
			file.Then("_spareTokens = tokens;");
			file.Line($"else if (_deeperTokenCount < {DeeperSpares})");
			file.Then($"(_deeperTokens ??= new Tokens_DotGram?[{DeeperSpares}])[_deeperTokenCount++] = tokens;");
		}

		file.Line();
		file.Line("/// <summary>The input as kinds, with where each one was.</summary>");
		file.Line("/// <remarks>");
		file.Line("/// The seam first and then a terminal, which is §4.5 read from the other side:");
		file.Line("/// trivia stands between operands, so between tokens is exactly where it stands.");
		file.Line("/// A character that begins no terminal stops the scan and is reported as where");
		file.Line("/// the input stopped being this language.");
		file.Line("/// </remarks>");

		file.Line("static Tokens_DotGram Tokenize_DotGram(string input) => Tokenize_DotGram(input, 0, input.Length);");
		file.Line();
		file.Line("/// <summary>The kinds between two offsets of the input, with where each one was.</summary>");
		file.Line("/// <remarks>");
		file.Line("/// What a reading over a window of the text reads. The characters past the window are");
		file.Line("/// not there for it — a token that would run over the edge ends at it — and the");
		file.Line("/// positions are still offsets into the whole input.");
		file.Line("/// </remarks>");

		using (file.Block("static Tokens_DotGram Tokenize_DotGram(string input, int from, int to)"))
		{
			file.Line("var tokens = Rented_DotGram();");
			file.Line();
			// A quarter of the characters is a fair first guess at how many tokens there
			// are, and being wrong costs a doubling rather than a document's worth of array.
			file.Line("tokens.Room((to - from) / 4 + 16);");
			file.Line();
			file.Line("var text    = global::System.MemoryExtensions.AsSpan(input, 0, to);");
			file.Line("var kinds   = tokens.Kinds;");
			file.Line("var starts  = tokens.Starts;");
			file.Line("var lengths = tokens.Lengths;");
			file.Line();
			file.Line("var count = 0;");
			file.Line("var p     = from;");
			file.Line();

			using (file.Block("while (true)"))
			{
				if (skipping.Name is { } scanner)
				{
					file.Line($"var skipped = {scanner}(text, p);");
					file.Line();
					file.Line("if (skipped > p)");
					file.Then("p = skipped;");
					file.Line();
				}

				file.Line("if (p >= text.Length)");
				file.Then("break;");
				file.Line();

				// The terminals the lexer does not measure, asked before it and in the order
				// the grammar names them: there is no automaton to run them together, so there
				// is nothing to decide between them by, and ordered choice (§3) is the only
				// rule that does not need one. A grammar with none of them writes exactly what
				// it always wrote.
				if (lexical.Inventory.Externals.Count > 0)
				{
					file.Line("var kind = 0;");
					file.Line("var end  = p;");
					file.Line();

					var asked = 0;

					foreach (var (pattern, number) in lexical.Inventory.Externals)
					{
						var at = "at" + asked++;

						file.Line($"var {at} = p;");
						file.Line();

						using (file.Block($"if (kind == 0 && {pattern.Method}(text, ref {at}) && {at} > p)"))
						{
							file.Line($"kind = {number};");
							file.Line($"end  = {at};");
						}

						file.Line();
					}

					file.Line("if (kind == 0)");
					file.Then("end = Scan(text, p, out kind);");
				}
				else
				{
					file.Line("var end = Scan(text, p, out var kind);");
				}

				file.Line();

				// A terminal the lexer only begins goes on from where its beginning ended, and
				// is measured there by the host or by its rule's machine over characters. Asked
				// only when the automaton stopped on that beginning, so it costs a comparison
				// on every other token; a refusal is a character nothing reads.
				var continued = 0;

				foreach (var continuation in lexical.Inventory.Continued)
				{
					using (file.Block($"{(continued++ == 0 ? "if" : "else if")} (kind == {continuation.Kind})"))
					{
						if (MeasuredBy(lexical, continuation.Tail) is { } rule)
						{
							var output = valuing!.Results.QualifiedOf(rule) is null ? "" : ", out _";

							file.Line($"var failure  = new {FailureType} {{ Quiet = true }};");
							// The whole input where a construction in that machine asks for it: a
							// terminal's value may be a piece of the text it has to read again later.
							file.Line(
								$"var measured = Measure_{IdentifierOf(rule)}_DotGram(text, end, ref failure{output}" +
								$"{(valuing!.UsesInput ? ", input" : "")});");
							file.Line();
							file.Line("if (measured < 0)");
							file.Then("kind = 0;");
							file.Line("else");
							file.Then("end = measured;");
						}
						else
						{
							file.Line("var measured = end;");
							file.Line();
							file.Line($"if ({HostMeasure(lexical, continuation.Tail)})");
							file.Then("end = measured;");
							file.Line("else");
							file.Then("kind = 0;");
						}
					}

					// A longer token that starts with the beginning: the automaton stopped on
					// it and never read this terminal past its beginning, so the rest is
					// measured here too. Longer wins; as long, the token is both; shorter, or
					// refused, it is what the automaton said (TerminalInventory.Continuation).
					foreach (var (longer, union) in continuation.Extended)
					{
						var beginning = continuation.Beginning!;
						var begins    = beginning.Length == 1
							? $"text[p] == {Character(beginning[0])}"
							: $"global::System.MemoryExtensions.StartsWith(text.Slice(p), global::System.MemoryExtensions.AsSpan({Quoted(beginning)}))";

						using (file.Block($"else if (kind == {longer} && {begins})"))
						{
							var from = $"p + {beginning.Length}";

							if (MeasuredBy(lexical, continuation.Tail) is { } rule)
							{
								var output = valuing!.Results.QualifiedOf(rule) is null ? "" : ", out _";

								file.Line($"var failure  = new {FailureType} {{ Quiet = true }};");
								file.Line(
									$"var measured = Measure_{IdentifierOf(rule)}_DotGram(text, {from}, ref failure{output}" +
									$"{(valuing!.UsesInput ? ", input" : "")});");
							}
							else
							{
								file.Line($"var measured = {from};");
								file.Line();
								file.Line($"if (!{HostMeasure(lexical, continuation.Tail)})");
								file.Then("measured = -1;");
							}

							file.Line();

							using (file.Block("if (measured > end)"))
							{
								file.Line($"kind = {continuation.Kind};");
								file.Line("end  = measured;");
							}

							file.Line("else if (measured == end)");
							file.Then($"kind = {union};");
						}
					}
				}

				if (continued > 0)
					file.Line();

				using (file.Block("if (kind == 0 || end <= p)"))
				{
					file.Line("tokens.Count   = count;");
					file.Line("tokens.Stopped = p;");
					file.Line();
					file.Line("return tokens;");
				}

				file.Line();

				using (file.Block("if (count == kinds.Length)"))
				{
					file.Line("tokens.Grow(count);");
					file.Line();
					file.Line("kinds   = tokens.Kinds;");
					file.Line("starts  = tokens.Starts;");
					file.Line("lengths = tokens.Lengths;");
				}

				file.Line();
				file.Line("kinds  [count] = (char)kind;");
				file.Line("starts [count] = p;");
				file.Line("lengths[count] = end - p;");
				file.Line("count++;");
				file.Line();
				file.Line("p = end;");
			}

			file.Line();
			file.Line("tokens.Count   = count;");
			file.Line("tokens.Stopped = -1;");
			file.Line();
			file.Line("return tokens;");
		}

		return file.ToString();
	}

	/// <summary>
	/// <c>find</c>: every occurrence, one at a time.
	/// </summary>
	/// <remarks>
	/// An iterator rather than an array, so that a document with a million occurrences
	/// costs one at a time — and so that "the first one" and "the ones that satisfy this"
	/// are LINQ's rather than three more directives. The span is made where it is passed
	/// and never held in a local, which is what lets this be an iterator at all.
	/// </remarks>
	static void EmitFind(
		Writer file, Publication publication, string method, string name,
		string value, string match, string hands, Func<string, string, string> recognized,
		string takes)
	{
		file.Line($"/// <summary>Every occurrence of <c>{name}</c>, in order, found as it is asked for.</summary>");

		using (file.Block(
			$"{AccessOf(publication)} static global::System.Collections.Generic.IEnumerable<{match}> {method}(string input{takes})"))
		{
			using (file.Block("for (var start = 0; start <= input.Length; )"))
			{
				file.Line($"var failure = new {FailureType} {{ Quiet = true }};");
				file.Line();
				file.Line(
					$"var end = {MethodOf(publication.Rule)}(" +
					$"global::System.MemoryExtensions.AsSpan(input), start{hands});");
				file.Line();

				using (file.Block("if (end < 0)"))
				{
					file.Line("start++;");
					file.Line("continue;");
				}

				file.Line();
				file.Line($"yield return {match}.Success({recognized("start", "end - start")}, start, end - start);");
				file.Line();
				file.Line("// A rule that matches nothing would otherwise find it for ever.");
				file.Line("start = end > start ? end : start + 1;");
			}
		}
	}

	/// <summary>
	/// The type a rule's captures are built into: a constructor and a get-only property
	/// per capture, and nothing else.
	/// </summary>
	/// <remarks>
	/// <c>public</c>, unlike the support types of §6.1 — those are shared between
	/// assemblies and so have a version to skew, while this one is generated from the
	/// grammar it belongs to, in the assembly that uses it. That is what lets a published
	/// method hand it back.
	/// </remarks>
	static void EmitResultType(Writer file, RecognitionGraph graph, ResultTypes results, RuleSymbol rule)
	{
		var name    = results.NameOf(rule)!;
		var members = graph.Results[rule];

		string Type(ResultMember member) =>
			results.ValueOf(member.Rule) + (member.IsSequence ? "[]" : member.IsOptional ? "?" : "");

		file.Line($"/// <summary>What the rule <c>{rule.Name}</c> recognized.</summary>");

		using (file.Block($"public sealed class {name}"))
		{
			var parameters = members.Select(member => $"{Type(member)} {ResultTypes.ParameterOf(member)}");

			using (file.Block($"public {name}({string.Join(", ", parameters)})"))
				foreach (var member in members)
					file.Line($"{ResultTypes.PropertyOf(member, name)} = {ResultTypes.ParameterOf(member)};");

			foreach (var member in members)
			{
				file.Line();
				file.Line($"/// <summary>The <c>{member.Name}</c> capture.</summary>");
				file.Line($"public {Type(member)} {ResultTypes.PropertyOf(member, name)} {{ get; }}");
			}
		}
	}

	/// <summary>
	/// The methods a rule's <c>=&gt;</c> expressions become: the C# they named, with the
	/// captures as parameters.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A method rather than an expression written where the value is assigned, and that
	/// is what makes the capture names usable at all — inside the machine they would have
	/// to dodge every local it has, and a capture called <c>p</c> or <c>state</c> would
	/// collide with the recognizer itself. Here they are parameters, in a scope of their
	/// own, named exactly as the grammar named them.
	/// </para>
	/// <para>
	/// One per alternative that carries a <c>=&gt;</c>, in the order they are written,
	/// which is the order the machine numbers them by.
	/// </para>
	/// </remarks>
	internal static IReadOnlyList<Machine.Factory> FactoriesOf(
		RecognitionGraph graph, ResultTypes results, RuleSymbol rule)
	{
		return results.FactoriesOf(graph, rule);
	}

	internal static IReadOnlyList<Machine.Factory> BuildFactories(
		RecognitionGraph graph, ResultTypes results, RuleSymbol rule)
	{
		var name   = "Construct_" + IdentifierOf(rule);
		var fold   = graph.Folds.TryGetValue(rule, out var found0) ? found0 : null;
		var layout = LayoutOf(graph, results, rule);
		var found  = new List<Machine.Factory>();

		foreach (var node in Fold.Of(graph.Bodies[rule], fold))
		{
			if (node is not Node.Construct)
				continue;

			var from    = layout.Before(node);
			var to      = layout.After(node);
			var own     = Writes(layout, node);
			var visible = new List<ResultMember>();

			// Only what this alternative wrote, and optional only where this alternative may
			// skip it. A sibling's captures are neither its business nor ever written when it
			// is the one that matched.
			//
			// The range alone does not say that. Before is where an abandoned attempt is put
			// back to, which is in front of the whole choice, so every alternative's range
			// begins at the first one's and a `=>` was handed a chain of tests over slots its
			// own reading can never have filled — one per sibling in front of it, which is
			// quadratic in the alternatives. Eleven of them in the expression language's
			// `Assignment` wrote sixty-six tests where eleven would do.
			foreach (var member in graph.Results[rule])
			{
				var mine = new List<int>();

				foreach (var slot in member.Slots)
					if (slot >= from && slot < to && own.Contains(slot))
						mine.Add(slot);

				if (mine.Count > 0)
					visible.Add(member with
					{
						Slots      = mine,
						// The head a fold shares stands in front of this alternative and is
						// as much a part of it as the tail is.
						IsOptional = !GrammarNormalizer.Writes(node, member.Name) &&
							(layout.SharedHead(node) is not { } head ||
								!GrammarNormalizer.Writes(head, member.Name)),
					});
			}

			found.Add(new Machine.Factory(
				node,
				found.Count == 0 ? name : name + "_" + found.Count,
				visible,
				fold is not null && fold.Accumulators.TryGetValue(node, out var accumulator)
					? accumulator
					: null,
				graph.Located.Contains(rule)));
		}

		return found;
	}

	/// <summary>
	/// The slots one alternative writes: those captured under it, and those of the head
	/// a left-factoring moved out in front of the choice it stands in, which is as much
	/// its own as what it kept.
	/// </summary>
	static HashSet<int> Writes(CaptureLayout layout, Node alternative)
	{
		var found = new HashSet<int>();

		Gather(alternative);

		if (layout.SharedHead(alternative) is { } head)
			Gather(head);

		return found;

		void Gather(Node node)
		{
			foreach (var one in NodeWalk.Descendants(node))
				if (one is Node.Capture && layout.SlotOrNone(one) is var slot && slot >= 0)
					found.Add(slot);
		}
	}

	/// <summary>
	/// Why this machine has no reader for a carrier to be a property of, or null where it
	/// has one.
	/// </summary>
	/// <remarks>
	/// The words are <see cref="Machine.Refusal"/>'s, which GRAM5005 already says over
	/// kinds; over characters they were worked out and never spoken, so a carrier asked for
	/// over such a grammar changed nothing and said nothing.
	/// </remarks>
	static string? Unread(
		Compiled compiled, RecognitionGraph graph, bool overKinds, CarrierKind carrier) =>
		compiled.Direct ? null :

		// The carrier's own answer first, where it has one. Several of these can be true
		// of one grammar at once — a recovering rule keeps it off the reader and would
		// also be refused by the carrier — and the one worth saying is the one that names
		// what to change to get what was asked for.
		compiled.Machine.WouldRefuse(carrier) ??

		(compiled.Flat ? "it needs no arena at all — no recursion, no backtracking and no " +
				"construction held back — so it is compiled flat, and a carrier carries what a " +
				"reader holds" :
			compiled.Publications.Any(one => Streams(graph, one, overKinds))
				? "it is read a window at a time, which the engine does" :
			compiled.Machine.Refusal is var (rule, why)
				? (rule is null ? "it" : $"'{rule.Name}'") + $" cannot be read by methods because {why}"
				: "it is not read by methods");

	/// <summary>Where a rule's captures are, with its fold loop known for what it is.</summary>
	static CaptureLayout LayoutOf(RecognitionGraph graph, ResultTypes results, RuleSymbol rule) =>
		CaptureLayout.Of(
			graph.Bodies[rule],
			other => results.QualifiedOf(other) is not null,
			graph.Folds.TryGetValue(rule, out var fold) ? fold.Loop : null);

	/// <summary>
	/// A line of the author's own C#, under a <c>#line</c> that points back at where they
	/// wrote it (§7.6).
	/// </summary>
	/// <remarks>
	/// <para>
	/// Everything else in this file is machine-written and an error in it is our bug. This
	/// is the exception: the text came from the grammar, the C# compiler will have things
	/// to say about it, and it must say them on the grammar's line. Without the directive
	/// the author is sent into a generated file they did not write and cannot edit.
	/// </para>
	/// <para>
	/// Padded out to the column too, and that is not decoration: an error under one
	/// argument of <c>@Add(l, r)</c> should sit under that argument. The line is written
	/// with no indent of its own so that the padding is the whole of the column.
	/// </para>
	/// <para>
	/// The plain form, not the span form C# 10 added: the consumer's language version is
	/// not ours to assume (.claude/rules/emitted-code.md).
	/// </para>
	/// </remarks>
	/// <remarks>
	/// Only an expression has text of its own; the four the language supplies are written
	/// by the cases above, which know what they are writing and where it is not from.
	/// </remarks>
	/// <summary>
	/// A string as C# spells it, escapes and all.
	/// </summary>
	/// <remarks>
	/// Written here rather than taken from Roslyn: <c>Grammar/</c> does not reference it, and
	/// the rule is short enough to say. A verbatim literal would be shorter to produce and
	/// would put the grammar's own line breaks into the generated file, which is a great deal
	/// of noise in something nobody reads.
	/// </remarks>
	internal static string Quoted(string text)
	{
		var made = new System.Text.StringBuilder(text.Length + 16).Append('"');

		foreach (var c in text)
			switch (c)
			{
				case '"':  made.Append("\\\""); break;
				case '\\': made.Append("\\\\"); break;
				case '\n': made.Append("\\n");  break;
				case '\r': made.Append("\\r");  break;
				case '\t': made.Append("\\t");  break;
				default:
					if (c < ' ')
						made.Append("\\u").Append(((int)c).ToString("x4"));
					else
						made.Append(c);

					break;
			}

		return made.Append('"').ToString();
	}

	internal static void Handed(Writer file, ILineMap? lines, Node.Construct construct)
	{
		if (construct.How is Construction.Expression { Text: var text, At: var at })
			Handed(file, lines, at, text + ";");
	}

	internal static void Handed(Writer file, ILineMap? lines, int at, string text)
	{
		if (lines is null || at < 0 || !lines.TryMap(at, out var path, out var line, out var column))
		{
			file.Line("\t" + text);

			return;
		}

		file.Exactly($"#line {line.ToString(System.Globalization.CultureInfo.InvariantCulture)} \"{path}\"");
		file.Exactly(new string(' ', column - 1) + text);
		file.Exactly("#line default");
	}

	static void EmitFactory(
		Writer file, RecognitionGraph graph, RuleSymbol rule, Machine.Factory factory,
		ResultTypes results, ILineMap? lines, bool spanCaptures = false, bool bytes = false)
	{
		var captureType = spanCaptures ? $"global::System.ReadOnlySpan<{(bytes ? "byte" : "char")}>" : "string";
		var parameters = new List<string>();

		// What §8.2 supplies that a construction can want: what the rule matched, and where
		// it matched. Both are passed when the expression says so — a name found inside a
		// string literal costs an unused parameter, and reading it exactly would mean lexing
		// C# on this side. What it saves is not a parameter: the text is the whole of what
		// the rule matched, built as a string on every construction, and most expressions
		// want the captures inside rather than the run around them.
		if (WantsText(graph, factory))
			parameters.Add(captureType + " parserText");

		if (WantsSpan(graph, factory))
			parameters.Add("SourceSpan parserSpan");

		// The whole input, for a construction that wants to keep where it matched and cut
		// the string later rather than be handed one now — §8.2. Nothing else here hands
		// over anything the parse did not itself produce, and that is the point of it: a
		// value built from this outlives the parse holding the input alive, which is a
		// bargain only the author of the grammar can strike.
		if (Asks(graph, factory, "parserInput"))
			parameters.Add("string parserInput");

		// The grammar's own state (§7.7), typed by the contract *this rule's* grammar
		// declared rather than by the effective type. The object is the caller's and every
		// hook sees the same one; what differs is the type it is seen through, which belongs
		// to whoever wrote the code. A rule included from another grammar goes on meaning
		// what it meant, and the call upcasts on its own (docs/next.md, "Decided: `context`
		// is a contract").
		if (graph.ContextOf(rule) is { } contract && Asks(graph, factory, "context"))
			parameters.Add($"{contract} context");

		// The marks standing over this construction, outermost first (§7.8). A span rather
		// than an array because it is a view of a buffer the walk reuses: it is right for
		// the length of the call and no longer, which is what a factory needs and all it
		// may keep.
		if (graph.State is not null && Asks(graph, factory, "parserState"))
			parameters.Add($"global::System.ReadOnlySpan<{graph.State}> parserState");

		// And where each of them was placed, in the same order: what tells one mark from
		// another of the same value, which a jump needs to say which loop it leaves.
		if (graph.State is not null && Asks(graph, factory, "parserMarks"))
			parameters.Add("global::System.ReadOnlySpan<int> parserMarks");

		// A fold step is handed the value built so far under the name it captured the
		// rule itself by (§4.3). It is not a capture any more — the rewrite took the call
		// away — so it is written in here rather than found among the members.
		if (factory.Accumulator is { Length: > 0 } accumulator)
			parameters.Add($"{graph.Types[rule]} {accumulator}");

		foreach (var member in factory.Members)
			if (member.Name != "parserText" && member.Name != factory.Accumulator)
				parameters.Add(
					(spanCaptures && member.Rule is null ? captureType : results.ValueOf(member.Rule)) +

					// A fold step is applied once per iteration and is handed that
					// iteration's captures, so what collects for the rule arrives here as
					// one of what it collected.
					(member.IsSequence && factory.Accumulator is null ? "[]" :
						member.IsOptional && !(spanCaptures && member.Rule is null) ? "?" : "") +

					" " + ResultTypes.ParameterOf(member));

		var head    = $"static {graph.Types[rule]} {factory.Method}({string.Join(", ", parameters)})";
		var summary = $"/// <summary>What <c>{rule.Name}</c> builds its value with";

		switch (((Node.Construct)factory.Of).How)
		{
			// The author's own C#, written under a `#line` pointing back at it (§7.6).
			case Construction.Expression when factory.Located:

				// Offered the range it was read over, which is a statement and not an
				// expression. Offered rather than written: a rule may hand back a value another
				// rule made, and the range it was read over is then wider than the value's own.
				// What the offer does with it is the named interface's business, not this
				// file's — see `LocationType` on the attribute.
				//
				// A block body rather than a helper, so that nothing at all is emitted for the
				// grammars — most of them — that ask for no locations.
				file.Line(summary + " (docs/syntax.md §7.3), and where it was written.</summary>");

				using (file.Block(head))
				{
					// The declared type and not `var`: the author's C# may be a conditional whose
					// arms are two different records, and only a target type tells the compiler
					// which of them the whole expression is (CS0173).
					file.Line($"{graph.Types[rule]} made =");
					Handed(file, lines, (Node.Construct)factory.Of);
					file.Line();
					file.Line("made.Locate(parserSpan.Start, parserSpan.Length);");
					file.Line();
					file.Line("return made;");
				}

				break;

			// The author's own C#, written under a `#line` pointing back at it (§7.6).
			case Construction.Expression:

				file.Line(summary + " (docs/syntax.md §7.3).</summary>");
				file.Line(head + " =>");
				Handed(file, lines, (Node.Construct)factory.Of);

				break;

			// §4.1 case 3: the value is one of the operands, handed back as it stands.
			case Construction.Operand:
			{
				var value = "default";

				foreach (var member in factory.Members)
					if (member.Name.StartsWith("item", StringComparison.Ordinal))
						value = ResultTypes.ParameterOf(member);

				file.Line(summary + ": what its operand was (§4.1 case 3).</summary>");
				file.Line(head + " =>");
				file.Line($"	{value};");

				break;
			}

			// §7.3's first way: the captures fill the declared type's constructor, in the
			// order the case carries.
			case Construction.Constructor(var order):
			{
				var arguments = new List<string>();

				foreach (var name in order)
					foreach (var member in factory.Members)
						if (member.Name == name)
							arguments.Add(ResultTypes.ParameterOf(member));

				file.Line(summary + " (§7.3).</summary>");
				file.Line(head + " =>");
				file.Line($"	new {graph.Types[rule]}({string.Join(", ", arguments)});");

				break;
			}

			// §7.3's second way: made, then written into. An object initializer rather than
			// assignments, because `init` and `required` can only be written in one.
			case Construction.Initializer(var bindings):
			{
				var written = new List<string>();

				foreach (var binding in bindings)
					foreach (var member in factory.Members)
						if (member.Name == binding.Capture)
							written.Add($"{binding.Property} = {ResultTypes.ParameterOf(member)}");

				file.Line(summary + " (§7.3).</summary>");
				file.Line(head + " =>");
				file.Line($"	new {graph.Types[rule]} {{ {string.Join(", ", written)} }};");

				break;
			}

			// §4.1 case 2: the grammar wrote no expression, so there is none to write out.
			// A body rather than an expression, because a repetition contributes an unknown
			// number of elements and an optional operand contributes none.
			case Construction.Sequence:
			{
				var element = graph.Types[rule].Substring(0, graph.Types[rule].Length - "[]".Length);

				file.Line($"/// <summary>Everything <c>{rule.Name}</c> is made of, in order (§4.1 case 2).</summary>");

				// One repetition and nothing else means the array handed in already is the
				// result, fresh from the materializer and shared with nobody: hand it back
				// rather than counting it into a copy of itself.
				if (factory.Members.Count == 1 && factory.Members[0].IsSequence)
				{
					var only = ResultTypes.ParameterOf(factory.Members[0]);

					file.Line(head + " =>");
					file.Line($"	{only} ?? new {element}[0];");

					break;
				}

				using (file.Block(head))
				{
					file.Line("var count = 0;");

					foreach (var member in factory.Members)
					{
						var value = ResultTypes.ParameterOf(member);

						if (member.IsSequence)
							file.Line($"if ({value} != null) count += {value}.Length;");
						else if (member.IsOptional)
							file.Line($"if ({value} != null) count++;");
						else
							file.Line("count++;");
					}

					file.Line();
					file.Line($"var items = new {element}[count];");
					file.Line("var at    = 0;");
					file.Line();

					foreach (var member in factory.Members)
					{
						var value = ResultTypes.ParameterOf(member);

						// A sequence capture already has its exact array. Copy it into this
						// rule's exact result rather than introducing a typed accumulator.
						if (member.IsSequence)
						{
							using (file.Block($"if ({value} != null)"))
							{
								file.Line($"global::System.Array.Copy({value}, 0, items, at, {value}.Length);");
								file.Line($"at += {value}.Length;");
							}
						}
						else if (member.IsOptional)
						{
							file.Line($"if ({value} != null)");
							file.Then($"items[at++] = ({element}){value};");
						}
						else
						{
							file.Line($"items[at++] = {value};");
						}
					}

					file.Line();
					file.Line("return items;");
				}

				break;
			}
		}
	}

	/// <summary>Whether a construction names one of the parameters §8.2 supplies.</summary>
	/// <summary>
	/// Whether a construction wants the text the rule matched — which it can say in two
	/// ways.
	/// </summary>
	/// <remarks>
	/// An expression says it by naming it, and that is the way there is a test for. The
	/// other, kept because <see cref="EmitFactory"/> has always left room for it, is a
	/// member of that name — how the forms of §7.3 that are not expressions would ask, if
	/// §7.3 matched a constructor against the supplied names as well as the captures. It
	/// does not today, so this arm is insurance rather than a feature.
	/// </remarks>
	internal static bool WantsText(Machine.Factory factory) => WantsText(null, factory);

	/// <inheritdoc cref="WantsText(Machine.Factory)"/>
	internal static bool WantsText(RecognitionGraph? graph, Machine.Factory factory)
	{
		if (factory.Of is Node.Construct { How: Construction.Expression { Text: var text } } &&
			Uses(graph, text, "parserText"))
			return true;

		foreach (var member in factory.Members)
			if (member.Name == "parserText")
				return true;

		return false;
	}

	/// <summary>
	/// Whether a grammar names the one type §4.1 case 4 offers besides <c>string</c>.
	/// </summary>
	/// <remarks>
	/// Three ways to name it: a rule declaring it, a construction asking for the supplied
	/// <c>parserSpan</c>, and a <c>recover</c> factory, whose C# is written by the consumer
	/// and not read here. The last is taken on trust, which emits an unused type into a
	/// grammar that recovers and never asks — the direction that compiles.
	/// </remarks>
	static bool UsesSourceSpan(RecognitionGraph graph)
	{
		// A rule told where it was written is handed one whether or not any C# names it.
		if (graph.Located.Count > 0)
			return true;

		foreach (var type in graph.Types.Values)
			if (type == "SourceSpan")
				return true;

		foreach (var rule in graph.Rules)
			if (graph.Bodies.TryGetValue(rule, out var body))
				foreach (var node in NodeWalk.Descendants(body))
					if (node is Node.Construct { How: Construction.Expression { Text: var text } } &&
						text.Contains("parserSpan") ||
						node is Node.Guard { Text: var condition } && condition.Contains("parserSpan"))
					{
						return true;
					}

		return graph.Recoveries.Count > 0;
	}

	internal static bool Asks(Machine.Factory factory, string name) =>
		factory.Of is Node.Construct { How: Construction.Expression { Text: var text } } &&
		Uses(null, text, name);

	/// <summary>Whether an embedded expression asks the parser for that name.</summary>
	/// <remarks>
	/// <para>
	/// From the syntax where a graph has it — a name is what the C# parser calls a name,
	/// which is neither text inside a literal nor the member half of a member access.
	/// </para>
	/// <para>
	/// The spelling is the fallback, and only that: it is what this did always, and it was
	/// wrong in both directions. `@(Log("parserInput"))` claimed the whole input and so
	/// refused the grammar its flat rendering; `@(other.context)` claimed the context,
	/// because a dot is not an identifier character. A graph built without a scanner, or an
	/// expression that would not parse, still gets the old answer — which is over-immediate
	/// rather than absent, and so adds a parameter rather than dropping one.
	/// </para>
	/// </remarks>
	internal static bool Uses(RecognitionGraph? graph, string text, string name) =>
		graph is not null && graph.FreeNames.TryGetValue(text, out var free)
			? free.Contains(name)
			: name.StartsWith("parser", StringComparison.Ordinal) ? text.Contains(name) : Names(text, name);

	/// <summary>The same, for a factory whose graph is in hand.</summary>
	/// <summary>
	/// Whether a construction is handed the range it was read from — because its C# asked
	/// for it by name, or because what it builds is told where it was written.
	/// </summary>
	internal static bool WantsSpan(RecognitionGraph graph, Machine.Factory factory) =>
		factory.Located || Asks(graph, factory, "parserSpan");

	internal static bool Asks(RecognitionGraph graph, Machine.Factory factory, string name) =>
		factory.Of is Node.Construct { How: Construction.Expression { Text: var text } } &&
		Uses(graph, text, name);

	/// <summary>Whether this C# names that identifier, rather than merely containing it.</summary>
	/// <remarks>
	/// The supplied names of §8.2 all begin with `parser`, and a substring test is enough
	/// for them — that prefix is what it is for. A name the author chose has no such
	/// protection: `context` is inside `contexts` and `myContext`, and taking either for
	/// the name itself would put a parameter on a publication that does not need one.
	/// </remarks>
	internal static bool Names(string text, string name)
	{
		for (var at = text.IndexOf(name, StringComparison.Ordinal); at >= 0;
			at = text.IndexOf(name, at + 1, StringComparison.Ordinal))
		{
			var before = at == 0 || !Continues(text[at - 1]);
			var after  = at + name.Length == text.Length || !Continues(text[at + name.Length]);

			if (before && after)
				return true;
		}

		return false;

		static bool Continues(char c) => char.IsLetterOrDigit(c) || c == '_';
	}

	/// <summary>
	/// The repetition of a rule that was marked <c>recover</c>, the slot its elements
	/// collect into, and what it was told (§8.2).
	/// </summary>
	internal static (Node Repetition, Recovery Recovery, int Slot)? RecoveryIn(
		RecognitionGraph graph, ResultTypes results, RuleSymbol rule)
	{
		var found = RecoveriesIn(graph, results, rule);

		return found.Count == 0 ? null : found[0];
	}

	/// <summary>
	/// Every repetition of a rule that was marked <c>recover</c>, in the order the rule
	/// reads, each with the slot its elements collect into and what it was told (§8.2).
	/// </summary>
	/// <remarks>
	/// A rule may mark more than one: each is its own repetition with its own sync, its
	/// own <c>=&gt;</c> and its own sequence to put a rejection in, and the arena has
	/// dispatched a recovery by plan since there was one plan. What has to differ per
	/// recovery is the name of the factory the grammar's <c>=&gt;</c> becomes, which
	/// <see cref="RecoveryMethod"/> settles for both halves of the emitter at once.
	/// </remarks>
	internal static IReadOnlyList<(Node Repetition, Recovery Recovery, int Slot)> RecoveriesIn(
		RecognitionGraph graph, ResultTypes results, RuleSymbol rule)
	{
		if (graph.Recoveries.Count == 0 || !graph.Bodies.ContainsKey(rule))
			return [];

		var layout = LayoutOf(graph, results, rule);
		var found  = new List<(Node, Recovery, int)>();

		Find(graph.Bodies[rule], -1);

		return found;

		// The capture is where a recovered element goes: the same sequence its successful
		// siblings collect into. It may be either side of the quantifier — `rows: Row*` is
		// `(rows: Row)*`, because a capture binds tighter (§10) — so both are looked at.
		void Find(Node node, int slot)
		{
			if (node is Node.Repeat(var repeated, _, _) && graph.Recoveries.TryGetValue(node, out var recovery))
			{
				found.Add((
					node, recovery,
					slot >= 0 ? slot : repeated is Node.Capture ? layout.SlotOf(repeated) : -1));

				return;
			}

			if (node is Node.Capture(_, var captured))
			{
				Find(captured, layout.SlotOf(node));

				return;
			}

			foreach (var child in Children(node))
				Find(child, -1);
		}
	}

	/// <summary>
	/// What the factory a <c>recover</c>'s <c>=&gt;</c> becomes is called. The first keeps
	/// the name it always had, so a rule with one recovery — every rule that had one until
	/// now — generates exactly the text it did.
	/// </summary>
	internal static string RecoveryMethod(RuleSymbol rule, int index) =>
		MethodOf(rule) + "_Recover" +
		(index == 0 ? "" : (index + 1).ToString(System.Globalization.CultureInfo.InvariantCulture));

	static IEnumerable<Node> Children(Node node) => node switch
	{
		Node.Sequence(var nodes)     => nodes,
		Node.Choice(var nodes)       => nodes,
		Node.Repeat(var body, _, _)  => [body],
		Node.Construct(var body, _)  => [body],
		Node.Atomic(var body)        => [body],
		Node.Marked(var body, _)     => [body],
		_                            => [],
	};

	/// <summary>
	/// What a broken element becomes: the C# the <c>recover</c> named, with the extent it
	/// covered and where in the sequence it was.
	/// </summary>
	/// <param name="method">
	/// The whole name, from <see cref="RecoveryMethod"/> — a rule may mark more than one
	/// repetition, and two factories cannot share a name.
	/// </param>
	internal static void EmitRecoveryFactory(
		Writer file, ResultTypes results, RuleSymbol rule, string method, Recovery recovery,
		RecognitionGraph graph, int slot, string? textType = null)
	{
		var element    = LayoutOf(graph, results, rule).Slots[slot].Rule;
		var parameters = new List<string>();

		foreach (var name in recovery.Asks)
			parameters.Add((name == "parserText" && textType is not null ? textType : TypeOfSupplied(name)) + " " + name);

		file.Line($"/// <summary>What <c>{rule.Name}</c> makes of an element it could not read.</summary>");
		file.Line(
			$"static {results.ValueOf(element)} {method}({string.Join(", ", parameters)}) =>");
		file.Line("\t" + recovery.Factory + ";");
		file.Line();
	}

	/// <summary>Whether any <c>recover</c> in this grammar asked where its element was.</summary>
	/// <remarks>
	/// A <c>recover</c> without a <c>=&gt;</c> always asks: the hook it reports on takes
	/// the line and the column whether or not anybody implements it.
	/// </remarks>
	static bool Locating(RecognitionGraph graph)
	{
		foreach (var recovery in graph.Recoveries.Values)
		{
			if (recovery.Factory is null)
				return true;

			var asked = recovery.Asks;

			if (asked.Contains("parserLine") || asked.Contains("parserColumn"))
				return true;
		}

		return false;
	}

	/// <summary>Whether any <c>recover</c> in this grammar reports out of band (§8.3).</summary>
	static bool Reporting(RecognitionGraph graph)
	{
		foreach (var recovery in graph.Recoveries.Values)
			if (recovery.Factory is null)
				return true;

		return false;
	}

	/// <summary>
	/// The C# type of a name §8.2 supplies to a failure factory.
	/// </summary>
	/// <remarks>
	/// <c>position</c> is a <c>long</c> and everything else counting is an <c>int</c>,
	/// which is the frozen rule: an absolute offset is into the input, and an input may be
	/// a file larger than an <c>int</c> can index, while an extent is into a buffer and a
	/// buffer never is. A line number and an ordinal are counts of things in that file
	/// rather than positions in it, and nothing counts two billion lines.
	/// </remarks>
	static string TypeOfSupplied(string name) => name switch
	{
		"parserText" or "parserMessage" or "parserInput" => "string",
		"parserSpan"                    => "SourceSpan",
		"parserPosition"                => "long",
		_                               => "int",
	};

	internal static string MethodOf(RuleSymbol rule) => "Recognize_" + IdentifierOf(rule);

	/// <summary>The name one machine's wrapper for a rule goes under.</summary>
	/// <remarks>
	/// A published rule's own machine writes the wrapper under the plain name, and every
	/// call meaning "recognize this rule" finds it there. A machine that streams a rule
	/// published elsewhere needs a wrapper of its own — over its engine, entered at its
	/// state — and cannot have that name, so it takes the machine's tag, the same one the
	/// engine carries. Both are wanted, and they are not one method twice: the reader
	/// enters this machine where it left off, and `find` enters the one compiled for it.
	/// </remarks>
	internal static string MethodOf(
		RecognitionGraph graph, RuleSymbol rule, RuleSymbol? owner, string tag) =>
		MethodOf(rule) + (rule.Equals(owner) || !IsPublished(graph, rule) ? "" : tag);

	/// <summary>What a publication's methods are declared as: what its directive said (§6).</summary>
	internal static string AccessOf(Publication publication) =>
		publication.Access switch
		{
			PublishAccess.Internal => "internal",
			PublishAccess.Private  => "private",
			_                      => "public",
		};

	/// <summary>Whether a rule is published, and so owns the plain name of its wrapper.</summary>
	static bool IsPublished(RecognitionGraph graph, RuleSymbol rule)
	{
		foreach (var publication in graph.Publications)
			if (rule.Equals(publication.Rule))
				return true;

		return false;
	}

	/// <summary>One published rule's machine, and the names it is emitted under.</summary>
	sealed record Compiled(
		Machine Machine,
		IReadOnlyList<Publication> Publications,
		string Engine,
		string Tag,
		bool Flat,
		bool Direct = false);

	/// <summary>
	/// The published rules, each with its own publications, in the order they were written.
	/// </summary>
	/// <remarks>
	/// A grammar that publishes nothing still gets one group, with no rule of its own and no
	/// publications: the recognizers are emitted for every rule and there is nothing to split
	/// by.
	/// </remarks>
	static List<(RuleSymbol? Rule, IReadOnlyList<Publication> Publications)> Published(
		RecognitionGraph graph)
	{
		var groups = new List<(RuleSymbol? Rule, IReadOnlyList<Publication> Publications)>();
		var byRule = new Dictionary<RuleSymbol, List<Publication>>();

		foreach (var publication in graph.Publications)
		{
			// The same rule published twice — `parse R` and `find R` — has always shared a
			// machine, entered at two states. Two *different* rules that can each reach the
			// other share one now, and for the same reason: a machine is built over what its
			// root reaches, mutual reachability makes those sets equal, and what used to
			// happen instead was the whole grammar compiled once per publication.
			//
			// The expression layer of standard SQL is what made that visible. `SearchCondition`
			// reaches `ValueExpression` through its predicates and `ValueExpression` reaches
			// back through `CASE`, so publishing both wrote 119,722 lines where one machine
			// writes 60,150 — the second entry point cost a complete second copy.
			var host = byRule.TryGetValue(publication.Rule, out var already)
				? already
				: Sharing(groups, graph, publication.Rule);

			if (host is not null)
			{
				host.Add(publication);
				byRule[publication.Rule] = host;

				continue;
			}

			var mine = new List<Publication> { publication };

			byRule[publication.Rule] = mine;
			groups.Add((publication.Rule, mine));
		}

		if (groups.Count == 0 && graph.Rules.Count > 0)
			groups.Add((null, []));

		return groups;
	}

	/// <summary>
	/// The publications a rule may join: those of a rule it can reach and which can reach
	/// it, or null where there are none.
	/// </summary>
	/// <remarks>
	/// Mutual reachability, because a machine is compiled over what its root reaches and two
	/// rules reaching each other reach the same things. A rule reached one way only is asked
	/// about later, by <see cref="Joined"/>, once the machines exist: whether it may join
	/// depends on how each of them turned out to be written.
	/// </remarks>
	/// <summary>
	/// Every machine whose root another machine reaches, folded into that one.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A machine holds every rule its root reaches, so a publication of one of those rules
	/// needs nothing of its own but an entry — and it used to get a whole machine, compiling
	/// again everything it reaches. `Expression` inside `Lambda`, `QueryExpression` inside
	/// `DirectSelect` and `Uri` inside `UriReference` were a fifth to a half of their files.
	/// </para>
	/// <para>
	/// Only where the larger machine is written the way the smaller one was, so that joining
	/// changes how neither publication is read: a lowered publication keeps its flat method,
	/// methods join methods and the engine joins the engine, and a streamed publication keeps
	/// the machine it was compiled for. Methods are asked again over both lists of
	/// publications, since what they need is worked out from the publications they serve.
	/// </para>
	/// </remarks>
	static void Joined(RecognitionGraph graph, List<Compiled> machines, bool overKinds)
	{
		for (var joined = true; joined; )
		{
			joined = false;

			for (var guest = 0; guest < machines.Count && !joined; guest++)
			{
				var smaller = machines[guest];

				if (smaller.Flat ||
					smaller.Machine.Anchor is not { } root ||
					smaller.Publications.Count == 0 ||
					smaller.Publications.Any(publication => Streams(graph, publication, overKinds)))
				{
					continue;
				}

				for (var host = 0; host < machines.Count; host++)
				{
					var larger = machines[host];

					if (host == guest ||
						larger.Flat ||
						larger.Direct != smaller.Direct ||
						larger.Machine.Anchor is not { } owner ||
						!Reaches(graph, owner).Contains(root))
					{
						continue;
					}

					var publications = larger.Publications.Concat(smaller.Publications).ToList();

					if (larger.Direct && !larger.Machine.CanDirect(publications))
					{
						// Put back what the refused question worked out.
						larger.Machine.CanDirect(larger.Publications);

						continue;
					}

					machines[host] = larger with { Publications = publications };
					machines.RemoveAt(guest);
					joined = true;

					break;
				}
			}
		}
	}

	static List<Publication>? Sharing(
		List<(RuleSymbol? Rule, IReadOnlyList<Publication> Publications)> groups,
		RecognitionGraph graph,
		RuleSymbol rule)
	{
		foreach (var (owner, publications) in groups)
			if (owner is not null && graph.Calls.Together(owner, rule))
				return (List<Publication>)publications;

		return null;
	}

	/// <summary>
	/// The value types every machine numbers against, in one order they all agree on.
	/// </summary>
	/// <remarks>
	/// A machine names a type by where it sits in this list and the parser holds one table
	/// per entry, so two machines disagreeing about the order would each be reading the
	/// other's table. Built by taking them in machine order and keeping the first sighting,
	/// which is the order a single machine produced before there was more than one.
	/// </remarks>
	static IReadOnlyList<string> ValueTables(IReadOnlyList<Compiled> machines, Machine? valuing)
	{
		var tables = new List<string>();

		foreach (var compiled in machines)
			foreach (var type in compiled.Machine.ValueTypes)
				if (!tables.Contains(type))
					tables.Add(type);

		// Last, so that adding a second read to a grammar never renumbers what the syntactic
		// machines had already agreed on.
		if (valuing is not null)
			foreach (var type in valuing.ValueTypes)
				if (!tables.Contains(type))
					tables.Add(type);

		return tables;
	}

	/// <summary>
	/// The second read: one character machine, and a way in for each terminal that builds.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The lexer says where a terminal ended; this says what it was worth. It is the rule's
	/// own character machine — the same states the unsplit parser would have run — over
	/// exactly the text of the one token, so whatever the author wrote in <c>=&gt; @(...)</c>
	/// runs with the captures it was written against.
	/// </para>
	/// <para>
	/// It cannot fail and is not asked to: the extent is known, and the lexer accepted it by
	/// this very rule. A refusal would mean the two machines disagree about a language they
	/// were built from one grammar, so the value comes back as the type's default and the
	/// parse carries on rather than a parser throwing at its own inconsistency.
	/// </para>
	/// </remarks>
	/// <summary>
	/// The construction of a lexical rule whose value is its own text put through the
	/// author's C#, or null where reading it again is what the value needs.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A token's value is normally had by reading the token again with a machine of its
	/// own: the lexer answered with a kind and kept none of the shape inside it, and the
	/// <c>=&gt;</c> names captures that shape held. But a rule like <c>Dec : @string =
	/// t: DecRun =&gt; @(t.Replace("_", ""))</c> names one capture, and every character
	/// the rule reads is inside it — so <c>t</c> <b>is</b> the token, and the second
	/// reading answers a question already answered.
	/// </para>
	/// <para>
	/// It was not free: every integer in an expression cost a string cut, a run of the
	/// value automaton over it, a boxed result and a cast. Measured on the expression
	/// language, an operand that is a number cost about a hundred nanoseconds more than
	/// one that is a name, where the hand-written parser pays the same for both
	/// (docs/next.md).
	/// </para>
	/// <para>
	/// Refused where the construction wants anything the token alone cannot answer: a
	/// supplied name, the grammar's state, a second capture, a capture of a rule rather
	/// than of text, or one character read outside the capture — <c>Hex = "0x"i &amp;
	/// t: HexRun</c> reads two, and its <c>t</c> is not its token.
	/// </para>
	/// </remarks>
	static string? Verbatim(RecognitionGraph graph, ResultTypes results, RuleSymbol rule)
	{
		var factories = FactoriesOf(graph, results, rule);

		if (factories.Count != 1 || !graph.Bodies.TryGetValue(rule, out var body))
			return null;

		var factory = factories[0];

		// Anything supplied is something the token does not say.
		if (WantsText(graph, factory) || Asks(graph, factory, "parserSpan") ||
			Asks(graph, factory, "parserInput") || Asks(graph, factory, "context") ||
			Asks(graph, factory, "parserState") || Asks(graph, factory, "parserMarks"))
		{
			return null;
		}

		if (factory.Members.Count != 1 || factory.Members[0].Rule is not null ||
			factory.Members[0].IsSequence)
		{
			return null;
		}

		Node? capture = null;

		foreach (var node in NodeWalk.Descendants(body))
			if (node is Node.Capture)
			{
				if (capture is not null)
					return null;

				capture = node;
			}

		if (capture is null)
			return null;

		// Every character the rule reads has to be inside the capture, or what the
		// capture holds is not what the token holds.
		var inside = NodeWalk.ByIdentity(NodeWalk.Descendants(capture));

		foreach (var node in NodeWalk.Descendants(body))
			if (node is Node.Literal or Node.Element or Node.External or Node.Call &&
				!inside.Contains(node))
			{
				return null;
			}

		return factory.Method;
	}

	static string Rereading(LexicalSplit lexical, Machine valuing)
	{
		var file   = new Writer(1);
		var engine = "Recognize_DotGram_Value";

		// The engine first, and written last: rendering it is what decides this machine
		// needs a materializer at all, and `Extra` is where that then is.
		var body     = valuing.RenderEngine(engine);
		var scanners = valuing.RenderScanners();

		foreach (var extra in valuing.Extras())
		{
			file.Write(extra);
			file.Line();
		}

		file.Write(body);
		file.Line();

		if (scanners.Length > 0)
		{
			file.Write(scanners);
			file.Line();
		}

		// A terminal whose value is its own text still needs that text as a string, and most
		// of them are one character — a digit. `Text_DotGram` keeps those in a table rather
		// than cutting them again (every operator of standard SQL was a fresh string before
		// it did), and a construction handed its token here would otherwise undo that for
		// every one-digit number. Written once, and only where such a terminal exists.
		if (lexical.Valued.Any(rule => Verbatim(lexical.Source, valuing.Results, rule) is not null))
		{
			file.Line("/// <summary>The strings of one character a token's own text comes to, made once each.</summary>");
			file.Line("static readonly string[] Letters_DotGram_Value = new string[128];");
			file.Line();
			file.Line("/// <summary>A token's text, and a character of it kept rather than cut again.</summary>");

			using (file.Block("static string Cut_DotGram_Value(string source, int at, int length)"))
			{
				using (file.Block("if (length == 1 && source[at] < 128)"))
				{
					file.Line("var one = source[at];");
					file.Line();
					file.Line("return Letters_DotGram_Value[one] ??= source.Substring(at, 1);");
				}

				file.Line();
				file.Line("return source.Substring(at, length);");
			}

			file.Line();
		}

		foreach (var rule in lexical.Valued)
		{
			var type = valuing.Results.QualifiedOf(rule)!;
			var read = "Reread_" + IdentifierOf(rule) + "_DotGram";

			// Where the token is the whole of what the construction can be told, there is
			// nothing to read again and the construction is called on the text itself.
			if (Verbatim(lexical.Source, valuing.Results, rule) is { } made)
			{
				file.Line($"/// <summary>What the text of one <c>{rule.Name}</c> token is worth.</summary>");
				file.Line("/// <remarks>The token itself: the capture spans it, so nothing about how it was read is wanted.</remarks>");
				file.Line(
					$"static {type} Value_{IdentifierOf(rule)}_DotGram(string source, int at, int length) => " +
					$"{made}(Cut_DotGram_Value(source, at, length));");
				file.Line();

				continue;
			}

			file.Write(valuing.RenderWrapper(rule, read, engine, whole: true));
			file.Line();

			file.Line($"/// <summary>What the text of one <c>{rule.Name}</c> token is worth.</summary>");
			file.Line("/// <remarks>");
			file.Line("/// Read where the token stands in the text and not over a copy of it: the span ends");
			file.Line("/// where the token ends, so the whole reading still has to consume exactly the token,");
			file.Line("/// and a position inside it is a position in the input.");
			file.Line("/// </remarks>");

			// Over the source up to the token's end, entered where the token begins. A cut-out
			// string started every position inside at zero and cost a string a token; a span
			// costs nothing and keeps the positions true. The reading stays whole: a rule's
			// first derivation may be shorter than what the lexer measured, and only reading to
			// the end of the span says it was the token that was read.
			using (file.Block($"static {type} Value_{IdentifierOf(rule)}_DotGram(string source, int at, int length)"))
			{
				file.Line($"var failure = new {FailureType} {{ Quiet = true }};");
				file.Line();
				file.Line(
					$"return {read}(global::System.MemoryExtensions.AsSpan(source, 0, at + length), at, " +
					$"ref failure, out {type} value{(valuing.UsesInput ? ", source" : "")}) < 0 ? default! : value;");
			}

			file.Line();
		}

		// What the tokenizer calls where the automaton stopped on the beginning of a terminal
		// it does not end. Entered there and not whole: how far it reads is the answer.
		foreach (var rule in Measuring(lexical))
		{
			file.Line($"/// <summary>Where a token ends whose rest <c>{rule.Name}</c> reads.</summary>");
			file.Write(valuing.RenderWrapper(rule, "Measure_" + IdentifierOf(rule) + "_DotGram", engine, whole: false));
			file.Line();
		}

		return file.ToString();
	}

	/// <summary>
	/// Every rule the second read compiles: the terminals that build, and what they reach.
	/// </summary>
	/// <summary>
	/// A machine over the original graph, asked for the seam. Only the seam is compiled: what
	/// it costs is the scanner it renders, not the parser it could have.
	/// </summary>
	/// <remarks>
	/// Built from the trivia and everything the trivia reaches. A machine compiles every rule
	/// it is given, and a comment the trivia calls is compiled as a call wherever it is not a
	/// scanner of its own — so a machine holding the trivia alone jumped to a state nobody
	/// wrote, and the generator failed on the first grammar whose trivia called a lexeme of
	/// its own.
	/// </remarks>
	internal static Machine Seam(LexicalSplit lexical) =>
		new(
			lexical.Source,
			new ResultTypes(lexical.Source, "Lexical", null),
			null,
			only: [.. lexical.Trivia.SelectMany(rule => Reaches(lexical.Source, rule)).Distinct()],
			// Tagged, because everything a machine emits is named after its tag and this one
			// stands in a file another machine has already filled: without it the seam's
			// character tables collide with the syntax's, name for name.
			tag: "_Seam");

	/// <summary>The rules that measure the rest of a terminal the lexer only begins, each once.</summary>
	static IReadOnlyList<RuleSymbol> Measuring(LexicalSplit lexical) =>
	[
		.. lexical.Inventory.Continued
			.Select(one => MeasuredBy(lexical, one.Tail))
			.OfType<RuleSymbol>()
			.Distinct(),
	];

	/// <summary>The rule a terminal's rest is measured by, or null where the host measures it.</summary>
	static RuleSymbol? MeasuredBy(LexicalSplit lexical, Node tail) =>
		tail is Node.Call(var rule, _) &&
		lexical.Source.Bodies.TryGetValue(rule, out var body) &&
		body is not Node.External
			? rule
			: null;

	/// <summary>The host's call measuring a terminal's rest, moving <c>measured</c> on.</summary>
	/// <remarks>
	/// §7.1's second row where the grammar wrote a bare <c>@M</c>, and its third where the host
	/// has an overload with a value — which the normalizer made a rule of. The value is not
	/// wanted here: the lexer asks how far, and a terminal that builds reads its value again.
	/// </remarks>
	/// <summary>A character as C# spells it between single quotes.</summary>
	static string Character(char c) => c switch
	{
		'\'' => @"'\''",
		'\\' => @"'\\'",
		< ' ' => @"'\u" + ((int)c).ToString("x4", System.Globalization.CultureInfo.InvariantCulture) + "'",
		_    => "'" + c + "'",
	};

	static string HostMeasure(LexicalSplit lexical, Node tail) =>
		tail switch
		{
			Node.External(var method) => $"{method}(text, ref measured)",
			Node.Call(var rule, _) when lexical.Source.Bodies[rule] is Node.External(var method) =>
				$"{method}(text, ref measured, out _)",
			_ => throw new InvalidOperationException($"{tail} is not measured by the host"),
		};

	static HashSet<RuleSymbol> Rereads(LexicalSplit lexical)
	{
		var reached = new HashSet<RuleSymbol>();
		var pending = new Stack<RuleSymbol>(lexical.Valued.Concat(Measuring(lexical)));

		while (pending.Count > 0)
		{
			var rule = pending.Pop();

			if (!reached.Add(rule) || !lexical.Source.Bodies.TryGetValue(rule, out var body))
				continue;

			foreach (var node in NodeWalk.Descendants(body))
				if (node is Node.Call(var called, _))
					pending.Push(called);
		}

		return reached;
	}

	/// <summary>Every rule a published one reaches, its own trivia included.</summary>
	/// <remarks>
	/// The trivia matters and is easy to miss: a `parse` compiles the rule wrapped in it
	/// (<c>Machine.BodyOf</c>), so whatever the trivia calls belongs to that machine as much
	/// as the body does. A machine built without it would jump to a state nobody wrote.
	/// </remarks>
	static HashSet<RuleSymbol> Reaches(RecognitionGraph graph, RuleSymbol? root) => graph.Reaches(root);

	/// <summary>
	/// Whether a parse of text in memory reads quietly first and records only on reading a
	/// refusal again (Q7.2).
	/// </summary>
	/// <remarks>
	/// <para>
	/// A second reading has to begin where the first began. A grammar with a context hands the
	/// reading an object its guards and constructions write into, which the generator cannot
	/// rewind, so it keeps one recording reading until a way to rewind one is decided
	/// (docs/design/diagnostics-off-the-hot-path-2026-09-18.md, §4).
	/// </para>
	/// <para>
	/// A recovering grammar hands its <c>recover</c> factories what the failure recorded
	/// while the parse goes on, so a quiet reading would hand them nothing. A stream, a buffer
	/// and a <c>yield</c> are not asked: their publications are written elsewhere and keep
	/// recording.
	/// </para>
	/// </remarks>
	static bool ReadsQuietlyFirst(RecognitionGraph graph) =>
		graph.Context is null && graph.Recoveries.Count == 0;

	/// <summary>Whether a recovery sits inside anything <paramref name="only"/> reaches.</summary>
	/// <remarks>
	/// Recoveries are keyed by node, so the reachable rules' bodies are walked for one.
	/// A null <paramref name="only"/> is the single-machine case, where reachable means
	/// the whole graph.
	/// </remarks>
	static bool RecoversWithin(RecognitionGraph graph, IReadOnlyCollection<RuleSymbol>? only)
	{
		if (graph.Recoveries.Count == 0)
			return false;

		if (only is null)
			return true;

		foreach (var rule in only)
		{
			if (graph.Bodies.TryGetValue(rule, out var body) &&
				NodeWalk.Descendants(body).Any(graph.Recoveries.ContainsKey))
				return true;

			if (graph.Trivia.TryGetValue(rule, out var trivia) &&
				NodeWalk.Descendants(trivia).Any(graph.Recoveries.ContainsKey))
				return true;
		}

		return false;
	}

	/// <summary>Whether anything <paramref name="only"/> reaches climbs precedence.</summary>
	static bool ClimbsWithin(RecognitionGraph graph, IReadOnlyCollection<RuleSymbol>? only) =>
		only is null
			? graph.Climbing.Count > 0
			: graph.Climbing.Keys.Any(only.Contains);

	/// <summary>
	/// A rule's name as one C# identifier, unique across the grammar.
	/// </summary>
	/// <remarks>
	/// The short name is not unique and is not meant to be — shadowing is what a namespace
	/// is for, so a grammar with a <c>trivia</c> per namespace is the ordinary case rather
	/// than a clash. The namespaces a rule is declared in are prefixed to tell them apart,
	/// named rather than numbered so that a reader of the generated code can still see
	/// which rule a method came from. The built-in rules' namespace is not an identifier
	/// and is left off: its names are fixed, and a grammar that shadows one of them takes
	/// the name with it.
	/// </remarks>
	internal static string IdentifierOf(RuleSymbol rule)
	{
		var name = rule.Name;

		for (var ns = rule.Namespace; ns is { Name.Length: > 0 }; ns = ns.Parent)
			if (IsIdentifier(ns.Name))
				name = ns.Name + "_" + name;

		return name;
	}

	static bool IsIdentifier(string name)
	{
		foreach (var c in name)
			if (!char.IsLetterOrDigit(c) && c != '_')
				return false;

		return true;
	}

	/// <summary>The recognizer that also insists the input ended — what `parse` calls.</summary>
	static string WholeOf(RuleSymbol rule) => MethodOf(rule) + "_Whole";

	internal static string Test(Node.Element element, Func<IReadOnlyList<CharRange>, string?>? tabulate = null)
	{
		var tests = new List<string>();

		// A class of any width is one test when it is read from a table, and the caller
		// that can declare one says so by handing the means to. Only where there is
		// nothing else in the element: a Unicode category is not in the table, and an
		// inversion is about what the table does not hold.
		if (tabulate is not null &&
			element is { IsNegated: false, Categories.Count: 0, References.Count: 0 } &&
			tabulate(element.Ranges) is { } table)
		{
			return $"c <= {Machine.TableSize - 1} && {table}[c] != 0";
		}

		foreach (var range in element.Ranges)
			tests.Add(range.IsSingle
				? $"c == {Char(range.From)}"
				: $"(c >= {Char(range.From)} && c <= {Char(range.To)})");

		// `\p{Lu}` is the regular-expression spelling; the enum member is
		// UppercaseLetter. `\p{L}` is not one category but five — and five categories
		// used to be five classifications of the same character. The enum's values fit
		// an int, so several categories are one classification and one mask test.
		var categories = new List<string>();

		foreach (var category in element.Categories)
			foreach (var name in UnicodeCategories.Expand(category))
				if (!categories.Contains(name))
					categories.Add(name);

		var mask = 0;

		foreach (var name in categories)
			mask |= 1 << (int)Enum.Parse(typeof(System.Globalization.UnicodeCategory), name);

		if (categories.Count == 1)
			tests.Add(
				"global::System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) == " +
				$"global::System.Globalization.UnicodeCategory.{categories[0]}");
		else if (categories.Count > 1)
			tests.Add(
				"((1 << (int)global::System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c)) & " +
				$"0x{mask:X}) != 0");

		// §7.1's element predicate: `bool M(char c)` asks the same question about one item
		// that a range does, so it joins the set as one more test. Written as the grammar
		// wrote it — the grammar's own `@using` directives are in the file, which is what
		// they are there for — rather than qualified, which nothing here could do.
		foreach (var reference in element.References)
			if (reference is CSharpSymbol predicate)
				tests.Add($"{predicate.Name}(c)");

		// An empty set admits nothing, and its complement admits everything. Said as
		// constants rather than as `!(false)`, so the caller can drop the test — and with
		// it the character it would otherwise read and never look at.
		if (tests.Count == 0)
			return element.IsNegated ? "true" : "false";

		var test  = string.Join(" || ", tests);
		var whole = element.IsNegated ? $"!({test})" : $"({test})";

		// A category is a call into `CharUnicodeInfo` and a mask test, taken once for every
		// character of every name a grammar reads. Below 128 the answer is a lookup — and
		// fixed there, because the categories of ASCII are settled by the standard and
		// cannot drift between the compiler that wrote the table and the runtime that
		// reads it. Above it the test that was there before, unchanged.
		if (tabulate is null || element.Categories.Count == 0 || element.References.Count > 0)
			return whole;

		var low = Ascii(element, mask);

		if (low.Count == 0)
			return $"(c >= {AsciiSize} && {whole})";

		if (low.Count == 1 && low[0].From == '\0' && low[0].To == AsciiSize - 1)
			return $"(c < {AsciiSize} || {whole})";

		var reached = tabulate(low) is { } named
			? $"{named}[c] != 0"
			: string.Join(" || ", low.Select(static one => one.IsSingle
				? $"c == {Char(one.From)}"
				: $"(c >= {Char(one.From)} && c <= {Char(one.To)})"));

		return $"(c < {AsciiSize} ? {reached} : {whole})";
	}

	/// <summary>How far up a class is answered from a table rather than by asking Unicode.</summary>
	/// <remarks>
	/// ASCII and no further, deliberately. The general categories of Latin-1 are as settled
	/// as ASCII's in practice, but "in practice" is the wrong footing for a table baked into
	/// a consumer's assembly by one runtime and read by another: below 128 the answer is not
	/// a fact about a Unicode version.
	/// </remarks>
	internal const int AsciiSize = 128;

	/// <summary>The characters below <see cref="AsciiSize"/> an element admits, as ranges.</summary>
	/// <param name="mask">The element's categories, as a bit per <c>UnicodeCategory</c>.</param>
	static IReadOnlyList<CharRange> Ascii(Node.Element element, int mask)
	{
		var found = new List<CharRange>();
		var from  = -1;

		for (var c = 0; c <= AsciiSize; c++)
		{
			var takes = c < AsciiSize && Admits(element, mask, (char)c);

			if (takes && from < 0)
				from = c;
			else if (!takes && from >= 0)
			{
				found.Add(new CharRange((char)from, (char)(c - 1)));
				from = -1;
			}
		}

		return found;
	}

	/// <summary>Whether an element with no C# predicate in it admits a character.</summary>
	static bool Admits(Node.Element element, int mask, char c)
	{
		var takes  = false;
		var ranges = element.Ranges;

		// By index: asked for each of the 128 characters of every element written, and a
		// `foreach` over the interface makes an enumerator each time.
		for (var i = 0; i < ranges.Count; i++)
			if (c >= ranges[i].From && c <= ranges[i].To)
			{
				takes = true;
				break;
			}

		if (!takes && mask != 0)
			takes = (1 << (int)System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) & mask) != 0;

		return takes != element.IsNegated;
	}

	/// <summary>
	/// A char as a C# character literal.
	/// </summary>
	/// <remarks>
	/// Everything outside plain printable ASCII goes out as <c>\uXXXX</c>. Putting the
	/// character itself into the file would work for most of them and produce a source
	/// file with a raw control character in it for the rest — and the emitted text is
	/// read by people, not only by a compiler.
	/// </remarks>
	internal static string Char(char value) => _chars[value] ??= Spelled(value);

	/// <summary>Each character's literal, written the first time it is asked for.</summary>
	/// <remarks>
	/// Every test of every character a machine makes spells one, and building the string again
	/// each time was two gigabytes of what generating the SQL parsers allocated. Two threads
	/// missing one slot spell the same literal, and whichever lands is kept.
	/// </remarks>
	static readonly string?[] _chars = new string?[char.MaxValue + 1];

	static string Spelled(char value) => value switch
	{
		'\''                         => @"'\''",
		'\\'                         => @"'\\'",
		>= ' ' and <= '~'            => $"'{value}'",
		'\0'                         => @"'\0'",
		'\a'                         => @"'\a'",
		'\b'                         => @"'\b'",
		'\f'                         => @"'\f'",
		'\n'                         => @"'\n'",
		'\r'                         => @"'\r'",
		'\t'                         => @"'\t'",
		'\v'                         => @"'\v'",
		_                            => $@"'\u{(int)value:X4}'",
	};
}
