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
	internal static string ImmediateValuesClass(IReadOnlyList<string> valueTypes, IReadOnlyCollection<string> stacks, string? stateType = null, bool markPositions = false)
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
			CSharpEmitter.Spares(text, "ImmediateValues");
			// A required stack can still be unused by this particular parse. Its high-water
			// mark includes values discarded by backtracking, so every retained reference
			// is cleared before the store is made available to another parse.
			text.Append("\tinternal static void Return(ImmediateValues values)\n\t{\n");
			var capacities = stacks.OrderBy(stack => stack, StringComparer.Ordinal)
				.Select(stack => "values.Stack" + stack + ".Length").ToList();
			if (stateType is not null)
				capacities.Add("values.MarkState.Length");
			if (stateType is not null && markPositions)
				capacities.Add("values.MarkAt.Length");
			text.Append("\t\t// Oversized stores are collected instead of retained by the thread.\n");
			text.Append("\t\tif (0L + ").Append(string.Join(" + ", capacities)).Append(" > 1048576) return;\n\n");
			Emptied(text, "Text");
			Emptied(text, "Spans");

			for (var i = 0; i < valueTypes.Count; i++)
				Emptied(text, TableName(valueTypes[i]));

			CSharpEmitter.Spared(text, "ImmediateValues", "values");
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
			// not `new E[][8]`: the size goes in the first rank, whatever the element is.
			// A rule whose value is a sequence gathered across turns is exactly that type.
			var rank = type.IndexOf('[');
			var core = rank < 0 ? type : type.Substring(0, rank);
			var rest = rank < 0 ? ""   : type.Substring(rank);

			string Made(string count) => "new " + core + "[" + count + "]" + rest;

			text.Append("\tinternal ").Append(type).Append("[] Stack").Append(tag).Append(" = ").Append(Made("8")).Append(";\n");
			text.Append("\tinternal int Count").Append(tag).Append(";\n");
			text.Append("\tinternal int High").Append(tag).Append(";\n\n");
			text.Append("\tinternal void Push").Append(tag).Append('(').Append(type).Append(" item)\n\t{\n");
			text.Append("\t\tif (Count").Append(tag).Append(" == Stack").Append(tag).Append(".Length)\n");
			text.Append("\t\t\tglobal::System.Array.Resize(ref Stack").Append(tag).Append(", Count").Append(tag).Append(" * 2);\n\n");
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
				foreach (var type in machine._valueTypes)
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
				// marks are, and carried where a reading deepens as the registers are.
				if (Unbuilding)
					yield return ("int", "unbuilt");
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
			if (kind == Demand.Kind.Always || !demands.Knows(call))
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
		public override IEnumerable<string> MarkRecords(string name) =>
			Marks ? [$"var {name} = marked;"] : [];

		/// <remarks>Only what <see cref="MarkRecords"/> declared: without marks, a part is handed none.</remarks>
		public override IReadOnlyList<string> RecordMarks(string name) =>
			Marks ? [name] : [];

		/// <remarks>A failed turn has pushed onto the stacks; the mark is where they stood.</remarks>
		public override IEnumerable<string> MarkGathered(RuleSymbol? owner, string name)
		{
			if (owner is null)
				yield break;

			foreach (var stack in GatheredStacks(owner))
				yield return $"var {name}_{stack} = values.Count{stack};";
		}

		public override IEnumerable<string> UnwindRecords(string name) =>
			Marks ? [$"marked = {name};"] : [];

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
		/// A default is what says a local was never written, and for a value type the default
		/// is also a value: an <c>int</c> left out and an <c>int</c> read as nought would be one
		/// answer. So a member that may be left out is kept in a local that can say nothing.
		/// </remarks>
		public override string DeclareRecordLocal(int slot, RuleSymbol rule, bool optional) =>
			optional
				? $"{Local(rule, optional)} r{slot} = default;"
				: $"{Local(rule, optional)} r{slot} = default!;";

		/// <summary>What a record local of a rule is declared as: its type, or one that can be nothing.</summary>
		string Local(RuleSymbol rule, bool optional)
		{
			var type = machine._results.ValueOf(rule);

			return optional && !type.EndsWith("?", StringComparison.Ordinal) ? type + "?" : type;
		}

		public override string DeclareAccumulator(RuleSymbol rule) => $"{machine._results.ValueOf(rule)} fold = default!;";

		public override IEnumerable<string> DeclareGathered(int slot, string elementType) => [];

		public override string RecordLocalType(RuleSymbol rule, bool optional = false) => Local(rule, optional) + " ";

		public override string ResetRecordLocal(int slot, bool optional) =>
			optional ? $"r{slot} = default;" : $"r{slot} = default!;";

		public override string Absent(RuleSymbol rule, string local) => $"ImmediateValues.IsDefault({local})";

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

		public override string PutText(DirectMember member, string from, string to)
		{
			var missing = machine.BorrowedCaptures ? machine.EmptyCapture : member.Member.IsOptional ? "null" : "string.Empty";

			_puts.Add((member, $"({from} < 0 ? {missing} : {machine.Cut(from, $"{to} - {from}")})"));

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

			string Text() => start is null ? "string.Empty" : machine.Cut(start, $"{end} - {start}");
			string Span() => start is null ? "default" : machine.Span(start, $"{end} - {start}");

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

			return $"{into} = {made.Method}({string.Join(", ", arguments)});";

			string Value(DirectMember member)
			{
				foreach (var (put, value) in _puts)
					if (ReferenceEquals(put, member) || put.Index == member.Index)
						return value;

				throw new InvalidOperationException($"Member '{member.Member.Name}' of '{rule.Name}' was never put.");
			}
		}

		public override string Last(RuleSymbol rule) => Register(machine._results.ValueOf(rule));

		string Register(string valueType) =>
			valueType == "SourceSpan" ? "lastSpan" : $"last{TableName(valueType)}";

		public override string PushText(int slot, string from, string to) =>
			$"values.Push{Gathering("Spans")}(((long)({from}) << 32) | (uint)({to}));";

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
				if (machine._directRules is not { } rules ||
					!rules.Any(rule => machine.DirectMembers(rule).Exists(static one => one.Shape == MemberShape.Pieces)))
				{
					return "";
				}

				var file = new Writer(0);

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

		public override string PushRecord(int slot, RuleSymbol rule) =>
			$"values.Push{StackOf(machine._results.ValueOf(rule))}({Last(rule)});";

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

		public override string Materialize(string record, string sinceMark) => "";

		public override string ValueOf(RuleSymbol rule, string record) => record;

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

		public override string RenderBuilder(IReadOnlyList<RuleSymbol> rules) => "";

		public override string RenderStore(IReadOnlyList<string> valueTypes, string? stateType) =>
			ImmediateValuesClass(valueTypes, SharedRequirements ?? GatheredRequirements, stateType, NamesMarks(machine._graph));

		public override string? Refuses()
		{
			if (machine._graph.Recoveries.Count > 0)
				return "it recovers";

			// An extent is carried: its value is the two positions the reader already has.
			// Collecting one is not, and for a reason worth saying rather than hiding — the
			// stack to collect on would be a stack of spans, and there is no table for one
			// because nothing else ever stores a span.
			foreach (var rule in machine._rules)
				if (machine.IsExtent(rule) && machine.Gathered(rule))
					return $"'{rule.Name}' is an extent collected across turns";

			// A member gathered across turns takes what its rule pushed onto the stack of its type
			// since the rule began, so two members of one rule on one stack would take each other's:
			// `a: X* & ';' & b: X*` handed `a` all four of `ab;cd` and `b` none. Kept apart on the
			// tape, whose references carry the slot they were pushed for.
			foreach (var rule in machine._rules)
				if (SharedStack(rule) is { } shared)
					return $"'{rule.Name}' gathers two members onto one stack ({shared})";

			return null;
		}

		/// <summary>The stack two gathered members of the rule would share, or null where none is shared.</summary>
		/// <remarks>What <see cref="GatheredStacks"/> answers, asked without requiring the stacks.</remarks>
		string? SharedStack(RuleSymbol owner)
		{
			var seen  = new HashSet<string>(StringComparer.Ordinal);
			var steps = machine.DirectStepSlots(owner);

			foreach (var member in machine.DirectMembers(owner))
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
		string StackOf(string valueType) =>
			machine.TableFor(valueType) >= 0
				? Gathering(TableName(valueType))
				: throw new InvalidOperationException($"No value table for '{valueType}'.");
	}
}
