using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;

namespace DotGram.Grammar.Emit;

sealed partial class Machine
{
	/// <summary>One store is shared by all immediate machines in the generated class.</summary>
	internal static void ShareImmediateStacks(IEnumerable<Machine> machines)
	{
		var carriers = machines.Select(machine => machine.Carrier).OfType<ImmediateCarrier>().ToList();
		if (carriers.Count < 2)
			return;

		var stacks = new HashSet<string>(carriers.SelectMany(carrier => carrier.GatheredRequirements), StringComparer.Ordinal);

		foreach (var carrier in carriers)
			carrier.SharedRequirements = stacks;
	}

	/// <summary>The class an immediate parser rents: only the stacks its readers require.</summary>
	/// <remarks>
	/// <para>
	/// A stack per type for what a rule gathers across turns, and one for pieces of text,
	/// marked and unwound by count like the tape's references and taken as one array when
	/// the record is written — the one allocation a hand-written parser makes for a list.
	/// </para>
	/// <para>
	/// The registers a value is handed through from callee to caller are not here but in the
	/// reader itself (<see cref="ImmediateCarrier.ReaderRegisters"/>): a reader writes one for
	/// every value it builds, and written into this class, on the heap, each went through
	/// the collector's write barrier — a fifth of the parse, measured on the SQL yardstick.
	/// </para>
	/// </remarks>
	/// <param name="isReference">
	/// Whether a value type is a class, which decides how its stack is written to
	/// (<see cref="Machine.IsReferenceType"/>); null treats every type as a struct.
	/// </param>
	internal static string ImmediateValuesClass(IReadOnlyList<string> valueTypes, IReadOnlyCollection<string> stacks, string? stateType = null, bool markPositions = false, Func<string, bool>? isReference = null)
	{
		var text = new StringBuilder();

		text.Append("/// <summary>The stacks used by immediate readers in this class (Machine.Immediate.cs).</summary>\n");
		text.Append("sealed class ImmediateValues\n{\n");
		Stack(text, "string", "Text");

		// Where each piece of a gathered run of text began and ended, as one number: the
		// run is joined the way the tape joins it, which needs the positions and not the text.
		Stack(text, "long", "Spans");

		// The marks standing over what is being read (§7.8). An array here rather than in
		// the reader because it grows and is worth keeping between parses; how deep it
		// stands is the reader's own field, being written at every mark.
		if (stateType is not null)
			text.Append("\tinternal ").Append(stateType).Append("[] MarkState = new ")
				.Append(stateType).Append("[8];\n\n");

		// And where each was placed, for a factory that names `parserMarks`.
		if (stateType is not null && markPositions)
			text.Append("\tinternal int[] MarkAt = new int[8];\n\n");

		for (var i = 0; i < valueTypes.Count; i++)
			Stack(text, valueTypes[i], TableName(valueTypes[i]));

		if (stacks.Count != 0 || stateType is not null)
		{
			text.Append('\n');
			CSharpEmitter.Spares(text, "ImmediateValues", releases: stacks.Count != 0);
			// A required stack can still be unused by this particular parse. Its high-water
			// mark includes values discarded by backtracking, so every retained reference
			// is cleared before the store is made available to another parse.
			text.Append("\tinternal static void Return(ImmediateValues values)\n\t{\n");
			// Taken before the emptying below, which zeroes the high-water marks.
			var high = stacks.OrderBy(stack => stack, StringComparer.Ordinal)
				.Select(stack => "values.High" + stack).ToList();
			if (high.Count != 0)
				text.Append("\t\tvar used = 0L + ").Append(string.Join(" + ", high)).Append(";\n\n");
			Emptied(text, "Text");
			Emptied(text, "Spans");

			for (var i = 0; i < valueTypes.Count; i++)
				Emptied(text, TableName(valueTypes[i]));

			// The bound comes after the clearing above, because the branch it opens returns:
			// a store kept past the bound reaches no line below it, and one parked with its
			// stacks still full kept every value of the last parse alive for as long as the
			// thread kept the store - which is the sentence above this method, unmet.
			var capacities = stacks.OrderBy(stack => stack, StringComparer.Ordinal)
				.Select(stack => "values.Stack" + stack + ".Length").ToList();
			if (stateType is not null)
				capacities.Add("values.MarkState.Length");
			if (stateType is not null && markPositions)
				capacities.Add("values.MarkAt.Length");
			var roomy = high.Count == 0 ? null : "0L + " + string.Join(" + ", capacities) + " > 4L * used";

			text.Append("\n\t\t// Past the bound the store is not thrown away - that was a cliff\n");
			text.Append("\t\t// and not a bound - it is kept while the work keeps wanting it, and handed\n");
			text.Append("\t\t// to the collector once it stops (CSharpEmitter.Outsized). It is emptied\n");
			text.Append("\t\t// first, above: what is kept is the room, never what was built in it.\n");
			text.Append("\t\tif (0L + ").Append(string.Join(" + ", capacities)).Append(" > 1048576)\n");
			CSharpEmitter.Outsized(text, "ImmediateValues", "values", roomy);

			// Room against use, and here the two are the same quantity: every capacity summed
			// above is a stack length, and every term of `used` is that stack's high-water mark.
			// Where a grammar has no stacks at all there is nothing to read a usage from, so the
			// slot is written without a release at all, rather than with one that cannot fire.
			// Four and not two for the reason written beside the direct store: one doubling of
			// room is what a steady workload leaves behind, and two is a workload that changed.
			CSharpEmitter.Spared(text, "ImmediateValues", "values", roomy);
			text.Append("\t}\n\n");
		}

		text.Append("\t/// <summary>Whether a local a record would have been kept in was never written.</summary>\n");
		text.Append("\tinternal static bool IsDefault<T>(T value) => global::System.Collections.Generic.EqualityComparer<T>.Default.Equals(value, default!);\n");
		text.Append("}\n");

		return text.ToString().Replace("\n", Lines.Ending);

		void Emptied(StringBuilder text, string tag)
		{
			if (!stacks.Contains(tag))
				return;

			text.Append("\t\tif (values.High").Append(tag).Append(" > 0)\n\t\t{\n");
			text.Append("\t\t\tglobal::System.Array.Clear(values.Stack").Append(tag)
				.Append(", 0, values.High").Append(tag).Append(");\n");
			text.Append("\t\t\tvalues.Count").Append(tag).Append(" = values.High").Append(tag).Append(" = 0;\n");
			text.Append("\t\t}\n\n");
		}

		void Stack(StringBuilder text, string type, string tag)
		{
			if (!stacks.Contains(tag))
				return;

			// `new T[8]` where the element type is itself an array is written `new E[8][]`,
			// not `new E[][8]` (CSharpEmitter.NewArray). A rule whose value is a sequence
			// gathered across turns is exactly that type.
			string Made(string count)
			{
				return CSharpEmitter.NewArray(type, count);
			}

			text.Append("\tinternal ").Append(type).Append("[] Stack").Append(tag).Append(" = ").Append(Made("8")).Append(";\n");
			text.Append("\tinternal int Count").Append(tag).Append(";\n");
			text.Append("\tinternal int High").Append(tag).Append(";\n\n");
			text.Append("\tinternal void Push").Append(tag).Append('(').Append(type).Append(" item)\n\t{\n");
			text.Append("\t\tif (Count").Append(tag).Append(" == Stack").Append(tag).Append(".Length)\n");
			text.Append("\t\t\tglobal::System.Array.Resize(ref Stack").Append(tag).Append(", Count").Append(tag).Append(" * 2);\n\n");

			// A store into an array of a class type is checked on every write against the
			// array's runtime element type - an array of a derived type may stand behind a
			// reference to the base's - and for a type nothing seals the check is a call into
			// the runtime that walks the item's bases: a tenth of a FIX parse, every field of
			// which goes onto the stack of an abstract type. A span over the array checks the
			// array's type once, at its own construction, and writes without the call. Said
			// only of a class: a struct's store was never checked, and a string's the compiler
			// already knows is sealed.
			if (isReference is not null && isReference(type) && type != "string")
			{
				text.Append("\t\t// Through a span and not the array: the array's store checks the item against the\n");
				text.Append("\t\t// array's runtime element type on every write, the span once, at its construction.\n");
				text.Append("\t\tnew global::System.Span<").Append(type).Append(">(Stack").Append(tag).Append(")[Count").Append(tag).Append("++] = item;\n\n");
			}
			else
				text.Append("\t\tStack").Append(tag).Append("[Count").Append(tag).Append("++] = item;\n\n");
			text.Append("\t\tif (Count").Append(tag).Append(" > High").Append(tag).Append(") High").Append(tag).Append(" = Count").Append(tag).Append(";\n\t}\n\n");
			text.Append("\t/// <summary>What was pushed since the mark, as one array, and the stack back at the mark.</summary>\n");
			text.Append("\tinternal ").Append(type).Append("[] Take").Append(tag).Append("(int from)\n\t{\n");
			text.Append("\t\tvar taken = Peek").Append(tag).Append("(from);\n\n");
			text.Append("\t\tCount").Append(tag).Append(" = from;\n\n");
			text.Append("\t\treturn taken;\n\t}\n\n");
			text.Append("\t/// <summary>What was pushed since the mark, as one array, the stack left as it is.</summary>\n");
			text.Append("\tinternal ").Append(type).Append("[] Peek").Append(tag).Append("(int from)\n\t{\n");
			text.Append("\t\tvar count = Count").Append(tag).Append(" - from;\n\n");
			text.Append("\t\tif (count == 0)\n\t\t\treturn global::System.Array.Empty<").Append(type).Append(">();\n\n");
			text.Append("\t\tvar taken = ").Append(Made("count")).Append(";\n\n");
			text.Append("\t\tglobal::System.Array.Copy(Stack").Append(tag).Append(", from, taken, 0, count);\n\n");
			text.Append("\t\treturn taken;\n\t}\n\n");
		}
	}

	/// <summary>
	/// No deferral: a <c>=&gt;</c> is called the moment its alternative has been read, and
	/// the value is the author's own object from then on.
	/// </summary>
	/// <remarks>
	/// <para>
	/// There is no record. A captured value lives in the reader's local of its own type, a
	/// gathered member is a list, the value so far of a fold is the accumulator itself, and
	/// when the reader reaches the end of an alternative it calls the construction with those
	/// locals as arguments and puts the result in the register for its type. An abandoned
	/// alternative has already called its construction — that is the one thing this carrier
	/// gives up, and <see cref="CarrierKind.Immediate"/> says who is answerable for it — and
	/// leaves nothing to unwind, because nothing it made is anywhere but in locals about to
	/// go out of scope.
	/// </para>
	/// <para>
	/// A guard costs nothing extra here: what it asks for has been built already.
	/// </para>
	/// <para>
	/// <b>What it does not carry yet</b>, and refuses so that the tape does instead:
	/// recovery, and an extent collected across the turns of a repetition — the stack to
	/// collect on would be a stack of spans, and nothing else ever stores one. A single
	/// extent is carried: a <c>SourceSpan</c>-typed rule has no record to be the span of,
	/// and needs none, the two positions being the reader's own locals.
	/// </para>
	/// </remarks>
	sealed class ImmediateCarrier(Machine machine) : ValueCarrier
	{
		// The record under construction: what Begin was told, and the members put since.
		RuleSymbol? _rule;
		int _factory;
		string? _start, _end;
		readonly List<(DirectMember Member, string Value)> _puts = [];

		// What the record would have taken off the stacks, for a reading that does not build:
		// the stacks go back to the rule's marks all the same, or what it pushed is taken by
		// whichever rule collects next.
		readonly List<string> _drops = [];
		internal readonly HashSet<string> GatheredRequirements = new(StringComparer.Ordinal);
		internal IReadOnlyCollection<string>? SharedRequirements;

		// Requirements are collected while writing the rule bodies, before entries and
		// reader fields. Sharing the generated store must not add needs to this reader.
		bool NeedsStorage => Marks || GatheredRequirements.Count != 0;
		bool _accumulated;

		/// <remarks>
		/// The stacks, and everything a construction called inside a reader may ask for:
		/// the tokens over kinds, since a text member is cut where it is read; the input and
		/// the context where any factory names them.
		/// </remarks>
		public override IEnumerable<(string Type, string Name)> ReaderState
		{
			get
			{
				if (NeedsStorage)
					yield return ("ImmediateValues", "values");

				if (machine.OverKinds)
					foreach (var token in Machine.TokenState)
						yield return token;

				if (machine.UsesInput)
					yield return ("string", "parserInput");

				if (machine.UsesContext)
					yield return (machine._graph.Context!, "context");

				if (machine.UsesReading)
					yield return ("int", "parserReading");

				if (machine.UsesLocating)
					yield return ("int", "parserLocating");
			}
		}

		/// <summary>One register per value type, holding the last value built of it.</summary>
		/// <remarks>
		/// The tape hands a value from callee to caller through <c>ways.Last</c>, an index
		/// into a log; this hands it through a field of the value's own type. It is sound for
		/// the same reason the index is: every valued rule writes its own record last, after
		/// everything it captured, so when a reader returns the register of its type holds
		/// its value and nothing has come between. A field of the reader and not of the
		/// store it rents, for the reason <see cref="ValueCarrier.ReaderRegisters"/> gives.
		/// </remarks>
		public override IEnumerable<(string Type, string Name)> ReaderRegisters
		{
			get
			{
				// This machine's own types, not the file's: the tables are numbered over every
				// machine's union, but a register is read only by rules of this machine, and a
				// reader is a struct the entry constructs — and so zeroes — on every parse. With
				// the union, SQL:2023's one-token readers carried three hundred registers,
				// three kilobytes written to nothing on each call.
				foreach (var type in machine.MaterializationTypes)
					yield return (type, "last" + TableName(type));

				// A span is not one of the tables — nothing stores one, the tape reading an
				// extent off the record it stands on — so its register is named rather than
				// numbered, and stands only where the grammar has an extent to put in it.
				if (machine._rules.Any(machine.IsExtent))
					yield return ("SourceSpan", "lastSpan");

				// How deep the §7.8 marks stand. A field for the same reason the registers
				// are: it is written at every mark, and a mark is written wherever the
				// grammar puts one — which in a language with `checked(...)` is a hot path.
				if (Marks)
					yield return ("int", "marked");

				// How many calls whose value nobody asks for the reading is inside. Nothing is
				// built while this is above zero (AroundCall). A field for the reason the
				// marks are, and carried where a reading deepens as the registers are. An entry
				// that reads a refused input again for its message begins with it raised
				// (Failure.Unasked, Machine.ReplaysUnasked): that reading hands out no value.
				if (Unbuilding)
					yield return ("int", "unbuilt");

				// What a loop that reads the attempt that stood once more puts back afterwards:
				// the failure as it was before, and how many ties it held (Stood_DotGram). One
				// of each: a replay follows the ways an attempt decided and fails nowhere, so
				// no loop inside it defers, and a loop around a deferring one is building still.
				if (machine.Replays)
				{
					yield return (CSharpEmitter.FailureType, "replayed");
					yield return ("int", "replayedMore");
				}

				// Where a recovered element is, counted on from the one before (Located_DotGram): the
				// bad elements are stepped over in the order they are read, as the tape's walk visits
				// them. Over buffered input the buffer counts, and nothing is kept here.
				if (!machine.BufferedInput && machine._recoveryReads.Values.Any(static read =>
					read.Plan.Recovery.Locates || read.Plan.Recovery.Words))
					yield return ("Located_DotGram", "located");
			}
		}

		/// <summary>
		/// Whether some rule of this machine is read where its value is not asked for, so that
		/// the reader has to know whether it is building (<see cref="Demand"/>).
		/// </summary>
		/// <remarks>
		/// The tape builds nothing while it reads, and afterwards only what the answer is made
		/// of and what a guard named. Building as it reads, this carrier has to leave out the
		/// same things, or it runs a construction the tape never would: the expression
		/// language reads an untyped lambda's body only to find its end, and a construction
		/// in that body, run before the lambda's parameters have types, throws.
		/// </remarks>
		bool Unbuilding => _unbuilding ??= machine._rules.Any(machine.Demands.ReadUnbuilt);

		bool? _unbuilding;

		/// <summary>
		/// What was kept of the demand report, forgotten: the readers were settled, and the report
		/// is asked again with the rules that give back (<see cref="Machine.Demands"/>).
		/// </summary>
		internal void Resettled()
		{
			_unbuilding = null;
		}

		/// <summary>
		/// The methods a loop that defers building on a give-back calls off its hot path
		/// (<see cref="Machine.DefersOnGiveBack"/>): where a loop stands is a local <c>u</c> of
		/// the loop — 0 building, as every loop begins; 1 building still, one way given back; 2
		/// reading unbuilt, the count raised by this loop; 3 reading the attempt that stood once
		/// more — and these move it. Not inlined, so that what a loop writes beside its attempt is
		/// one compare of the local and these calls: the paths they stand on are the rare ones.
		/// </summary>
		/// <remarks>
		/// The replay records as any reading does: a reading that records opens a way at a shut
		/// door where a quiet one breaks (<c>if (!open &amp;&amp; failure.Quiet)</c>), so only a
		/// reading of the same kind as the attempt it replays takes the recorded ways in the same
		/// places. What it records it had already recorded as the attempt that stood, so the
		/// failure is kept before the replay and put back after it, and the ties the replay added
		/// are dropped: the message is the attempt's own, said once.
		/// </remarks>
		static void ReplayMethods(Writer file)
		{
			const string NoInlining =
				"[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]";

			file.Line("/// <summary>An attempt stood after this loop gave a way back twice: read unbuilt, it is read once more to be built (3), with the failure kept to be put back; or the reading that built it stood, and the failure is put back (0).</summary>");
			file.Line(NoInlining);

			using (file.Block("int Stood_DotGram(int u, int s)"))
			{
				using (file.Block("if (u == 2)"))
				{
					file.Line("replayed     = failure;");
					file.Line("replayedMore = failure.ExpectedMore == null ? 0 : failure.ExpectedMore.Count;");
					file.Line("unbuilt      = 0;");
					file.Line("ways.Cursor  = s;");
					file.Line();
					file.Line("return 3;");
				}

				file.Line();
				file.Line("Replayed_DotGram();");
				file.Line();
				file.Line("return 0;");
			}

			file.Line();
			file.Line("/// <summary>What a reading made once more recorded, taken back: it recorded what the attempt it read again had recorded already.</summary>");
			file.Line(NoInlining);

			using (file.Block("void Replayed_DotGram()"))
			{
				file.Line("var more = failure.ExpectedMore;");
				file.Line();
				file.Line("failure = replayed;");
				file.Line();
				file.Line("if (more != null && more.Count > replayedMore)");
				file.Then("more.RemoveRange(replayedMore, more.Count - replayedMore);");
			}

			file.Line();
			file.Line("/// <summary>A way given back: the first is read building still, the second raises the count, where the reading was building.</summary>");
			file.Line(NoInlining);

			using (file.Block("int Retried_DotGram(int u)"))
			{
				using (file.Block("if (u == 1)"))
				{
					file.Line("unbuilt = 1;");
					file.Line();
					file.Line("return 2;");
				}

				file.Line();
				file.Line("return u == 0 && unbuilt == 0 ? 1 : u;");
			}

			file.Line();
			file.Line("/// <summary>A loop that ends without an answer puts back what it raised: the count, or the failure a replay was to put back.</summary>");
			file.Line(NoInlining);

			using (file.Block("void Undeferred_DotGram(int u)"))
			{
				file.Line("if (u == 2)");
				file.Then("unbuilt = 0;");
				file.Line("else if (u == 3)");
				file.Then("Replayed_DotGram();");
			}

			file.Line();
		}

		/// <summary>
		/// <summary>
		/// A call nobody asks the value of is read with the count raised; one a guard or a
		/// <c>with state</c> asks for is read with it at zero, where the rule it stands in
		/// can be read unbuilt.
		/// </summary>
		/// <remarks>
		/// Neither needs a <c>finally</c>, for the reasons the lookahead count does not
		/// (<c>LookingField</c>): a throw ends the parse, and the count lives in that parse's
		/// reader.
		/// </remarks>
		public override (string Before, string After)? AroundCall(RuleSymbol owner, Node.Call call, string local)
		{
			if (!Unbuilding)
				return null;

			var demands = machine.Demands;
			var kind    = demands.Of(call);

			if (kind == Demand.Kind.Never)
				return ("unbuilt++;", "unbuilt--;");

			if (!demands.ReadUnbuilt(owner))
				return null;

			// A call the report does not know is built as it always was: asking again for
			// what is under it is what every reading did before demand was asked at all.
			// Not where the rule called builds nothing and reaches nothing that does: there
			// is nothing under it to ask for, and the count is read by nothing it runs.
			if (kind == Demand.Kind.Always || !demands.Knows(call) && demands.Builds(call.Rule))
				return ($"var {local}u = unbuilt; unbuilt = 0;", $"unbuilt = {local}u;");

			return null;
		}

		/// <remarks>
		/// The marks of the stacks the rule gathers on, so that a part collects from where the
		/// rule began and not from where the part did.
		/// </remarks>
		public override string GatherHanding(RuleSymbol owner, bool declared, bool inBody)
		{
			var text = new StringBuilder();

			foreach (var stack in GatheredStacks(owner))
				text.Append(declared ? $", int refs_{stack}" : inBody ? $", rb_{stack}" : $", refs_{stack}");

			return text.ToString();
		}

		/// <remarks>
		/// Nothing is written down that an abandoned reading could leave behind — except how
		/// deep the marks stand, which is a stack the reader pushes on and a failed attempt
		/// left standing. Taken and put back wherever the tape takes and puts back its log,
		/// which is every place a reading may be abandoned.
		/// </remarks>
		public override IEnumerable<string> MarkRecords(string name)
		{
			return Marks ? [$"var {name} = marked;"] : [];
		}

		/// <remarks>Only what <see cref="MarkRecords"/> declared: without marks, a part is handed none.</remarks>
		public override IReadOnlyList<string> RecordMarks(string name)
		{
			return Marks ? [name] : [];
		}

		/// <remarks>A failed turn has pushed onto the stacks; the mark is where they stood.</remarks>
		public override IEnumerable<string> MarkGathered(RuleSymbol? owner, string name)
		{
			if (owner is null)
				yield break;

			foreach (var stack in GatheredStacks(owner))
				yield return $"var {name}_{stack} = values.Count{stack};";
		}

		public override IEnumerable<string> UnwindRecords(string name)
		{
			return Marks ? [$"marked = {name};"] : [];
		}

		/// <summary>
		/// Whether this parser carries §7.8 marks: whether a state type is declared at all.
		/// </summary>
		/// <remarks>
		/// The declaration and not <see cref="Machine.UsesMarks"/>, which counts the sites
		/// the reader has written so far and therefore answers differently before and after
		/// the mark it is about to write. A grammar that declares a state and places none
		/// keeps an integer nothing moves, which is what an empty span costs.
		/// </remarks>
		bool Marks => machine._graph.State is not null;

		public override IEnumerable<string> UnwindGathered(RuleSymbol? owner, string name)
		{
			if (owner is null)
				yield break;

			foreach (var stack in GatheredStacks(owner))
				yield return $"values.Count{stack} = {name}_{stack};";
		}

		/// <remarks>
		/// A record takes everything pushed onto its stacks since its rule began, so a rule that
		/// pushed some of its turns and then failed has to leave the stacks as it found them: its
		/// caller may read on by another alternative, and the next rule to collect on the same
		/// stack — around the caller, or the failed rule itself, entered again — would take what
		/// the failed one left. T-SQL read <c>CHOOSE (2, 'a', 'b', '</c> that way: the arguments
		/// of the abandoned call, once <c>CHOOSE</c> had been read again as a column name, were
		/// taken for the <c>AT TIME ZONE</c> tail of the expression around it.
		/// </remarks>
		public override IEnumerable<string> GiveBackGathered(RuleSymbol owner, string name)
		{
			return UnwindGathered(owner, name);
		}

		/// <remarks>
		/// A default is what says a local was never written, and for a value type the default
		/// is also a value: an <c>int</c> left out and an <c>int</c> read as nought would be one
		/// answer. So a member that may be left out is kept in a local that can say nothing.
		/// </remarks>
		public override string DeclareRecordLocal(int slot, RuleSymbol rule, bool optional)
		{
			return optional
				? $"{Local(rule, optional)} r{slot} = default;"
				: $"{Local(rule, optional)} r{slot} = default!;";
		}

		/// <summary>What a record local of a rule is declared as: its type, or one that can be nothing.</summary>
		string Local(RuleSymbol rule, bool optional)
		{
			var type = machine._results.ValueOf(rule);

			return optional && !type.EndsWith("?", StringComparison.Ordinal) ? type + "?" : type;
		}

		public override string DeclareAccumulator(RuleSymbol rule)
		{
			return $"{machine._results.ValueOf(rule)} fold = default!;";
		}

		public override IEnumerable<string> DeclareGathered(int slot, string elementType)
		{
			return [];
		}

		public override string RecordLocalType(RuleSymbol rule, bool optional = false)
		{
			return Local(rule, optional) + " ";
		}

		public override string ResetRecordLocal(int slot, bool optional)
		{
			return optional ? $"r{slot} = default;" : $"r{slot} = default!;";
		}

		public override string Absent(RuleSymbol rule, string local)
		{
			return $"ImmediateValues.IsDefault({local})";
		}

		public override string FirstRecord(IReadOnlyList<int> slots, RuleSymbol rule)
		{
			if (slots.Count == 1)
				return $"r{slots[0]}";

			var chain = $"default({machine._results.ValueOf(rule)})!";

			for (var i = slots.Count - 1; i >= 0; i--)
				chain = $"!ImmediateValues.IsDefault(r{slots[i]}) ? r{slots[i]} : {chain}";

			return $"({chain})";
		}

		public override string Begin(RuleSymbol rule, int factory, string? start, string? end)
		{
			_rule    = rule;
			_factory = factory;
			_start   = start;
			_end     = end;
			_puts.Clear();
			_drops.Clear();
			_accumulated = false;

			return "";
		}

		public override string PutAccumulator()
		{
			_accumulated = true;

			return "";
		}

		/// <remarks>
		/// The value so far is in hand, but the register is not sure to hold it: the loop stops
		/// at a step it gave back, and that step may have read a value of the rule's own type
		/// first — <c>l: F &amp; ',' &amp; r: V &amp; '.'</c> read <c>V</c> and found no dot —
		/// which left the register holding what nothing stands on. So the rule writes its value
		/// once more where the turns stop, and is last again, as <see cref="ReaderRegisters"/>
		/// requires.
		/// </remarks>
		public override string Folded(RuleSymbol owner)
		{
			return $"{Last(owner)} = fold;";
		}

		/// <remarks>
		/// Where a guard of the rule cut the member already, what it cut is the value when it
		/// stands on the same positions: the same text, cut once. Compared where it is taken,
		/// so a capture written again after the guard — another turn, another alternative —
		/// is cut on its own.
		/// </remarks>
		public override string PutText(DirectMember member, string from, string to, string? cached = null)
		{
			var missing = machine.BorrowedCaptures ? machine.EmptyCapture : member.Member.IsOptional ? "null" : "string.Empty";
			var cut     = $"({from} < 0 ? {missing} : {machine.Cut(from, $"{to} - {from}")})";

			_puts.Add((member, cached is null ? cut : $"({cached}From == {from} && {cached}To == {to} ? {cached} : {cut})"));

			return "";
		}

		public override string PutRecord(DirectMember member, string record)
		{
			_puts.Add((member, record));

			return "";
		}

		/// <summary>
		/// What the rule pushed since it began, taken off its type's stack as one array — the
		/// text joined where the member is pieces of it.
		/// </summary>
		public override string Collect(DirectMember member, string from, bool pairs)
		{
			var stack = pairs ? Gathering("Spans") : StackOf(machine._results.ValueOf(member.Member.Rule));
			var taken = $"values.Take{stack}({from}_{stack})";

			_puts.Add((member, pairs ? $"Joined_DotGram({taken}, {(member.Member.IsOptional ? "true" : "false")})" : taken));
			_drops.Add($"values.Count{stack} = {from}_{stack};");

			return "";
		}

		/// <summary>
		/// The construction, called now, its result in the register of the rule's type — where
		/// the reading is building; a rule some reading of which is not asks first.
		/// </summary>
		public override string End(string gatheredFrom)
		{
			var rule  = _rule ?? throw new InvalidOperationException("A record ended that never began.");
			var built = Built(rule);

			if (!Unbuilding || !machine.Demands.ReadUnbuilt(rule) || built.Length == 0)
				return built;

			return _drops.Count == 0
				? "if (unbuilt == 0) " + built
				: "if (unbuilt == 0) " + built + " else { " + string.Join(" ", _drops) + " }";
		}

		string Built(RuleSymbol rule)
		{
			var type = machine._results.QualifiedOf(rule)!;
			var into = Register(type);

			// A terminal that builds, over kinds: the lexer measured it, and the character
			// machine of its own builds it from the text — now rather than in the walk.
			if (machine._reread is not null && machine._reread.Contains(rule))
			{
				if (_start is null)
					throw new InvalidOperationException($"'{rule.Name}' builds from its text and its reader keeps no positions.");

				return $"{into} = Value_{CSharpEmitter.IdentifierOf(rule)}_DotGram({machine.TokenOf(_start)});";
			}

			// The span an alternative stands on, where the record would have carried one —
			// asked for only by a factory that wants it, since asking has the file emit the
			// helper that makes one.
			var (start, end) = (_start, _end);

			string Text()
			{
				return start is null ? "string.Empty" : machine.Cut(start, $"{end} - {start}");
			}

			string Span()
			{
				return start is null ? "default" : machine.Span(start, $"{end} - {start}");
			}

			// An extent is where it matched and nothing else — no members to pass and no
			// factory to call. The tape reads it off the record the rule stands on; here the
			// two positions are the reader's own locals and the span is made from them.
			if (machine.IsExtent(rule))
				return $"{into} = {Span()};";

			if (_factory < 0)
			{
				// No `=>`: the value is the members, in the order the rule lists them.
				var members = machine.DirectMembers(rule, _factory);
				var passed  = members.Select(member => Value(member) + (member.Member.IsOptional ? "" : "!"));

				return $"{into} = new {type}({string.Join(", ", passed)});";
			}

			var made = machine._factories[rule][_factory];
			var accumulated = _accumulated;
			var arguments   = machine.DirectArguments(
				rule, made, machine.DirectMembers(rule, _factory), Text, Span,
				() => accumulated ? "fold" : "default!", Value);

			var call = $"{made.Method}({string.Join(", ", arguments)})";

			return $"{into} = {(start is null ? call : machine.Offered(made, call, start, $"{end} - {start}"))};";

			string Value(DirectMember member)
			{
				foreach (var (put, value) in _puts)
					if (ReferenceEquals(put, member) || put.Index == member.Index)
						return value;

				throw new InvalidOperationException($"Member '{member.Member.Name}' of '{rule.Name}' was never put.");
			}
		}

		public override string Last(RuleSymbol rule)
		{
			return Register(machine._results.ValueOf(rule));
		}

		string Register(string valueType)
		{
			return valueType == "SourceSpan" ? "lastSpan" : $"last{TableName(valueType)}";
		}

		public override string PushText(int slot, string from, string to)
		{
			return $"values.Push{Gathering("Spans")}(((long)({from}) << 32) | (uint)({to}));";
		}

		/// <remarks>
		/// §10's join, written once for the reader and called where a run of text is collected.
		/// The tape's walk joins the same way (Machine.Direct.Values.cs): pieces that tile are
		/// one cut from the first start to the last end, and over kinds that cut is the answer
		/// and not a shortcut, because what stands between two adjacent tokens is part of the
		/// value the same reading over characters gives. Pieces that do not tile are cut one
		/// by one.
		/// </remarks>
		public override string ReaderMethods
		{
			get
			{
				var file = new Writer(0);

				if (machine.Replays)
					ReplayMethods(file);

				if (machine._directRules is not { } rules ||
					!rules.Any(rule => machine.DirectMembers(rule).Exists(static one => one.Shape == MemberShape.Pieces)))
				{
					return file.ToString();
				}

				file.Line("/// <summary>A run of text gathered across turns, joined: one cut where its pieces tile, each piece on its own where they do not.</summary>");

				using (file.Block($"{(machine.BorrowedCaptures ? machine.CaptureSpanType : "string")} Joined_DotGram(long[] pieces, bool optional)"))
				{
					file.Line("if (pieces.Length == 0)");
					file.Then(machine.BorrowedCaptures ? "return default;" : "return optional ? null! : string.Empty;");
					file.Line();
					file.Line("var first  = (int)(pieces[0] >> 32);");
					file.Line("var last   = (int)pieces[pieces.Length - 1];");
					file.Line("var length = 0;");
					file.Line();
					file.Line("foreach (var piece in pieces)");
					file.Then("length += (int)piece - (int)(piece >> 32);");
					file.Line();
					file.Line("if (last - first == length)");
					file.Then($"return {machine.Cut("first", "length")};");
					file.Line();
					file.Line(machine.BorrowedCaptures ? $"var built = new {machine.CaptureElement}[length]; int filled = 0;" : "var built = new global::System.Text.StringBuilder();");
					file.Line();

					using (file.Block("foreach (var piece in pieces)"))
					{
						file.Line("var from = (int)(piece >> 32);");
						file.Line();
						file.Line(machine.BorrowedCaptures ? $"text.Slice(from, (int)piece - from).CopyTo(new global::System.Span<{machine.CaptureElement}>(built, filled, (int)piece - from)); filled += (int)piece - from;" : $"built.Append({machine.Cut("from", "(int)piece - from")});");
					}

					file.Line();
					file.Line(machine.BorrowedCaptures ? "return built;" : "return built.ToString();");
				}

				file.Line();

				return file.ToString();
			}
		}

		public override string PushRecord(int slot, RuleSymbol rule)
		{
			return $"values.Push{StackOf(machine._results.ValueOf(rule))}({Last(rule)});";
		}

		/// <remarks>
		/// Built where it is stepped over, by the <c>recover</c> factory, from the four numbers the
		/// tape's record would hold, named as the walk names them (Machine.RecoverySupplied): past
		/// the turn it stands in, nothing takes it back (Commit, the turn's point).
		/// </remarks>
		public override IEnumerable<string> Recovered(RecoveryPlan plan, int slot, RuleSymbol element, bool positions, bool tagged)
		{
			if (plan.Recovery.Asks.Count > 0)
				yield return "var recovered = (Position: pos, Value: to, AtomicIndex: reach, RuleIndex: ordinal);";

			var arguments = string.Join(", ", plan.Recovery.Asks.Select(name =>
				machine.RecoverySupplied(name, plan, "located", "expecting")));
			// A yield's step keeps its element as the item it hands out, gathering nothing.
			var built     = (WordsOnce(plan.Recovery, "expecting") is { } once ? once + " " : "") + (plan.Recovery.YieldStep
				? $"{Last(element)} = {plan.Method}({arguments});"
				: $"{Last(element)} = {plan.Method}({arguments}); {PushRecord(slot, element)}");

			yield return Unbuilding ? $"if (unbuilt == 0) {{ {built} }}" : built;
		}

		/// <remarks>
		/// A live stack rather than a pair of records on a log. The tape writes the mark down
		/// and the walk at the end replays it into a stack to know what stood over a value;
		/// here the value is built while the mark stands, so the stack is the answer as it is.
		/// </remarks>
		public override string Mark(int kind, int site)
		{
			if (kind != -1)
				return "marked--;";

			// Where the mark is placed is where the reading under it begins, which is the
			// reader's position now: a factory built under it is handed that (`parserMarks`).
			if (NamesMarks(machine._graph))
				return
					$"if (marked == values.MarkState.Length) {{ global::System.Array.Resize(ref values.MarkState, marked * 2); " +
					$"global::System.Array.Resize(ref values.MarkAt, marked * 2); }} " +
					$"values.MarkAt[marked] = {machine.At("p")}; " +
					$"values.MarkState[marked++] = {machine._marks[site]};";

			return
				$"if (marked == values.MarkState.Length) global::System.Array.Resize(ref values.MarkState, marked * 2); " +
				$"values.MarkState[marked++] = {machine._marks[site]};";
		}

		public override string Materialize(string record, string sinceMark)
		{
			return "";
		}

		public override string ValueOf(RuleSymbol rule, string record)
		{
			return record;
		}

		/// <remarks>Peeked rather than taken: the record written later collects the same items.</remarks>
		public override void Gathered(Writer code, string from, IReadOnlyList<int> slots, string handed, string type, string build, bool text)
		{
			var stack = text ? Gathering("Text") : StackOf(type);

			code.Line($"var {handed} = values.Peek{stack}({from}_{stack});");
		}

		public override IEnumerable<string> Rent()
		{
			if (NeedsStorage)
				yield return "var values = ImmediateValues.Rent();";
		}

		public override IEnumerable<string> Return()
		{
			if (NeedsStorage)
				yield return "ImmediateValues.Return(values);";
		}

		/// <remarks>Read off the reader, which is what the register is a field of.</remarks>
		public override IEnumerable<string> BuildRoot(RuleSymbol rule, string type, bool extent)
		{
			yield return $"value = reader.{Register(type)};";
		}

		public override string RenderBuilder(IReadOnlyList<RuleSymbol> rules)
		{
			return "";
		}

		public override string RenderStore(IReadOnlyList<string> valueTypes, string? stateType)
		{
			return ImmediateValuesClass(valueTypes, SharedRequirements ?? GatheredRequirements, stateType, NamesMarks(machine._graph), machine.IsReferenceType);
		}

		/// <remarks>
		/// Every reason, not the first: a machine refused for one rule was reported as refused for
		/// that rule alone, and lifting it showed the next one only on the next build.
		/// </remarks>
		public override string? Refuses()
		{
			var reasons = new List<string>();

			if (machine.RecoveryRefusal() is { } recovers)
				reasons.Add(recovers);

			// A sequence gathered from operands of several types in the order they were read
			// would take them off as many stacks as there are types, and the order between the
			// stacks is not kept. The tape keeps every record in order.
			foreach (var rule in machine._rules)
				if (machine._graph.Results[rule].Any(static member => member.Element is not null))
					reasons.Add($"'{rule.Name}' gathers values of several types into one sequence");

			// An extent is carried: its value is the two positions the reader already has.
			// Collecting one is not, and for a reason worth saying rather than hiding — the
			// stack to collect on would be a stack of spans, and there is no table for one
			// because nothing else ever stores a span.
			foreach (var rule in machine._rules)
				if (machine.IsExtent(rule) && machine.Gathered(rule))
					reasons.Add($"'{rule.Name}' is an extent collected across turns");

			// A member gathered across turns takes what its rule pushed onto the stack of its type
			// since the rule began, so two members of one rule on one stack would take each other's:
			// `a: X* & ';' & b: X*` handed `a` all four of `ab;cd` and `b` none. Kept apart on the
			// tape, whose references carry the slot they were pushed for.
			foreach (var rule in machine._rules)
				if (SharedStack(rule) is { } shared)
					reasons.Add($"'{rule.Name}' gathers two members onto one stack ({shared})");

			return reasons.Count == 0 ? null : string.Join("; ", reasons);
		}

		/// <summary>The stack two gathered members of one factory would share, or null where none is shared.</summary>
		/// <remarks>
		/// Asked a factory at a time. Each factory's record collects its own members from where the
		/// rule began, and an alternative that fails gives back what it pushed before the next is
		/// read, so two members on one stack meet only where one factory takes both — in one
		/// alternative, or in alternatives grouped under one construction. Members of two
		/// alternatives with a factory each never see each other's turns, and asking across all of
		/// them refused seven rules of standard SQL that were read correctly.
		/// </remarks>
		string? SharedStack(RuleSymbol owner)
		{
			var factories = machine._factories.TryGetValue(owner, out var made) ? made.Count : 0;

			if (factories == 0)
				return SharedStack(owner, -1);

			for (var factory = 0; factory < factories; factory++)
				if (SharedStack(owner, factory) is { } shared)
					return shared;

			return null;
		}

		/// <remarks>What <see cref="GatheredStacks"/> answers, asked of one factory without requiring the stacks.</remarks>
		string? SharedStack(RuleSymbol owner, int factory)
		{
			var seen  = new HashSet<string>(StringComparer.Ordinal);
			var steps = machine.DirectStepSlots(owner);

			foreach (var member in machine.DirectMembers(owner, factory))
			{
				if (member.Slots.All(steps.Contains))
					continue;

				var stack = member.Shape switch
				{
					MemberShape.Pieces  => "text",
					MemberShape.Records => machine._results.ValueOf(member.Member.Rule),
					_                   => null,
				};

				if (stack is not null && !seen.Add(stack))
					return stack;
			}

			return null;
		}

		/// <summary>The stacks a rule gathers on — one per type its gathered members have, and the text's.</summary>
		/// <remarks>
		/// Not the captures of a fold's steps: a step's is written once a turn and built into
		/// the value so far there and then (§4.3), so it is a sequence to the walk over the
		/// tape's records and never on a stack here — and a rule that only folds marked and
		/// unwound a stack it never pushed on, every turn.
		/// </remarks>
		IEnumerable<string> GatheredStacks(RuleSymbol owner)
		{
			var seen  = new HashSet<string>(StringComparer.Ordinal);
			var steps = machine.DirectStepSlots(owner);

			foreach (var member in machine.DirectMembers(owner))
			{
				if (member.Slots.All(steps.Contains))
					continue;

				var stack = member.Shape switch
				{
					MemberShape.Pieces  => Gathering("Spans"),
					MemberShape.Records => StackOf(machine._results.ValueOf(member.Member.Rule)),
					_                   => null,
				};

				if (stack is not null && seen.Add(stack))
					yield return stack;
			}
		}

		/// <summary>Require a stack when writing an operation that uses it.</summary>
		string Gathering(string stack)
		{
			GatheredRequirements.Add(stack);

			return stack;
		}

		/// <summary>The stack a type's gathered values go on: the one numbered as its table is.</summary>
		string StackOf(string valueType)
		{
			return machine.TableFor(valueType) >= 0
				? Gathering(TableName(valueType))
				: throw new InvalidOperationException($"No value table for '{valueType}'.");
		}
	}
}
