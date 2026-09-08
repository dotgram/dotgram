using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;

namespace DotGram.Grammar.Emit;

sealed partial class Machine
{
	/// <summary>
	/// Deferral without a tape: what a rule read, kept in a typed shape of its own, and the
	/// author's constructions called by a walk over those shapes once the parse is accepted.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The tape keeps §7.3 by writing what it read into a log and building afterwards, and
	/// pays for it in the walk and in the log. The immediate carrier pays neither and gives
	/// up §7.3. This is the third answer, and the one the second stage exists to measure: a
	/// shape per rule, filled where the tape writes a record, and a <c>Build</c> per shape
	/// that calls the construction — so a factory runs once per node of the accepted
	/// derivation, as §7.3 requires, and what it runs over is fields of a known type rather
	/// than integers read back out of a log.
	/// </para>
	/// <para>
	/// A rule off every cycle is a value held inside whatever captured it, so a leaf costs
	/// no allocation at all; a rule that can reach itself is a reference, because a value
	/// cannot contain itself (<c>Shapes</c>, and <c>benchmarks/DotGram.HandDeferred</c>,
	/// where the shape was written by hand first). A rule with one construction needs no
	/// field saying which it is, and most rules have one: forty-four of the expression
	/// language's seventy-three, and every one of RFC 3986's.
	/// </para>
	/// <para>
	/// <b>What it carries so far</b>: a rule that builds one way, out of runs of text and of
	/// other rules' values, over characters. Everything else is refused and the tape carries
	/// it — the list is <see cref="MixedCarrier.Refuses"/>, and it shortens a shape at a
	/// time, each with <c>CarrierTests</c> holding it to the same language as the tape. That
	/// is the order the immediate carrier was built in and the reason it was never wrong on
	/// the way.
	/// </para>
	/// </remarks>
	sealed class MixedCarrier(Machine machine) : ValueCarrier
	{
		// The record under construction: what Begin was told, and the members put since —
		// the same bookkeeping the immediate carrier does, ending in a shape rather than in
		// a call to the author.
		RuleSymbol? _rule;
		int _factory;
		readonly List<(DirectMember Member, string Value)> _puts = [];

		/// <summary>
		/// Which shapes are not written. Everything named here is left to the tape, and this
		/// is the list that shortens as they are written.
		/// </summary>
		/// <remarks>A register per rule, so a value handed up is not where the caller reads.</remarks>
		public override bool ForwardsInPlace => false;

		/// <remarks>A shape per rule, so two places that read two rules are two types.</remarks>
		public override bool ByPlace => true;

		public override string? Refuses()
		{
			if (machine._graph.Recoveries.Count > 0)
				return "it recovers";

			if (machine._graph.State is not null)
				return "it lays marks over what it reads";

			foreach (var rule in Shaped)
			{
				// A terminal the lexer measured and a machine of its own builds from the
				// text: its shape would have to keep where it stood, and does not yet.
				if (machine._reread is not null && machine._reread.Contains(rule))
					return $"'{rule.Name}' is built again from its text";

				if (machine.IsExtent(rule))
					return $"'{rule.Name}' is an extent";

				if (machine._graph.Climbing.ContainsKey(rule))
					return $"'{rule.Name}' is read at a strength";

				// A guard over a gathered member asks for the run before the record is
				// written, which is the one thing the stacks cannot answer: what is on them
				// is what has been pushed, and taking it is what writing the record does.
				if (NodeWalk.Descendants(machine._graph.Bodies[rule]).Any(one => one is Node.Guard))
					foreach (var member in Union(rule).Concat(Union(rule, steps: true)))
						if (member.Shape is MemberShape.Pieces or MemberShape.Records)
							return $"'{rule.Name}' has a guard and gathers '{member.Member.Name}'";

			}

			return null;
		}

		/// <summary>The rules that have a shape: the ones the reader reads and that build.</summary>
		IEnumerable<RuleSymbol> Shaped =>
			(machine._directRules ?? machine._rules).Where(machine.Valued);

		/// <summary>The method a run's field is built by, named after the field.</summary>
		static string Named(string field) => "Gathered" + field.Substring(2);

		/// <summary>What a rule's shape is called, and the reader's field holding the last one.</summary>
		static string Named(RuleSymbol rule, string? part = null) =>
			(part ?? "Shape") + "_" + CSharpEmitter.IdentifierOf(rule);

		static string Register(RuleSymbol rule) => "shape_" + CSharpEmitter.IdentifierOf(rule);

		/// <summary>
		/// Whether a rule's shape is a reference: whether the rule can be reached from
		/// itself, which is the whole of the question.
		/// </summary>
		/// <remarks>
		/// A value cannot contain itself, so something on every cycle has to be one. Nothing
		/// off a cycle needs to be, and those are held inside whatever captured them — which
		/// is where the leaves are, and why they cost no allocation. Left recursion is not a
		/// cycle for this: §4.3 turned it into a loop before the reader saw it.
		/// </remarks>
		bool Reference(RuleSymbol rule) => machine._graph.Calls.Recurses(rule);

		/// <summary>A shape as a member or a local holds it: nullable where it is a reference.</summary>
		string Held(RuleSymbol rule) => Named(rule) + (Reference(rule) ? "?" : "");

		/// <summary>
		/// What a shape is handed to be built from, and what it is handed at a call: the
		/// text where the reading is over characters, and the tokens' own text and where
		/// each of them stood where it is over kinds.
		/// </summary>
		/// <remarks>
		/// The names are the reader's, so that <c>Machine.Cut</c> writes the same expression
		/// inside a shape as it writes inside the reader — there being one right way to cut
		/// a run of text out of a reading, and no reason for this to know which it is.
		/// </remarks>
		string Taking =>
			machine.OverKinds
				? "string parserSource, int[] parserStarts, int[] parserLengths"
				: "global::System.ReadOnlySpan<char> text";

		string Given =>
			machine.OverKinds ? "parserSource, parserStarts, parserLengths" : "text";

		/// <summary>Nothing there, said the way the shape says it.</summary>
		string Nothing(RuleSymbol rule, string local) =>
			Reference(rule) ? $"{local} == null" : $"{local}.IsNothing";

		/// <remarks>
		/// A field per rule rather than one per value type: two rules may build the same type
		/// and their shapes are still two types. Fields of the reader for the reason
		/// <see cref="ValueCarrier.ReaderRegisters"/> gives.
		/// </remarks>
		public override IEnumerable<(string Type, string Name)> ReaderRegisters
		{
			get
			{
				foreach (var rule in Shaped)
					yield return (Named(rule), Register(rule));
			}
		}

		public override IEnumerable<(string Type, string Name)> ReaderState
		{
			get
			{
				if (Gathers)
					yield return ("MixedValues", "values");
			}
		}

		/// <summary>Whether anything in this machine gathers, and so rents a store.</summary>
		bool Gathers => Stacks.Count > 0;

		/// <summary>
		/// The stacks a repetition gathers on: one per rule whose values are gathered
		/// somewhere, and one for the runs of text.
		/// </summary>
		/// <remarks>
		/// A fold's run is threaded through its own turns and needs no stack; this is the
		/// other kind of run, the one a `*` makes, whose elements are not links in a chain
		/// and have nowhere to point. Pushed as they are read and taken as one array when
		/// the record is written, which is what the immediate carrier does — and what a
		/// hand-written parser does, being the one allocation a list costs.
		/// </remarks>
		IReadOnlyList<(string Name, string Element)> Stacks
		{
			get
			{
				if (_stacks is not null)
					return _stacks;

				var found = new List<(string Name, string Element)>();

				foreach (var rule in Shaped)
					foreach (var member in Union(rule).Concat(Union(rule, steps: true)))
					{
						if (member.Shape is not (MemberShape.Pieces or MemberShape.Records))
							continue;

						foreach (var (stack, element) in Runs(rule, member))
							if (!found.Exists(one => one.Name == stack))
								found.Add((stack, element));
					}

				return _stacks = found;
			}
		}

		IReadOnlyList<(string Name, string Element)>? _stacks;

		/// <summary>The stack a member is gathered on, and what one element of it is.</summary>
		/// <remarks>
		/// Named by the rule its places read and not by the rule the member names, because
		/// what goes on the stack is a shape and a shape is per rule. Where the places read
		/// different rules the elements would be two types and the run one, which is the
		/// question <see cref="Places"/> answers for a member captured once and this one
		/// refuses for a member gathered.
		/// </remarks>
		string Stack(RuleSymbol owner, DirectMember member) =>
			member.Shape == MemberShape.Pieces ? "Spans" : Named(Element(owner, member));

		RuleSymbol Element(RuleSymbol owner, DirectMember member) =>
			machine.RuleAt(owner, member.Slots[0]) ?? member.Member.Rule!;

		/// <summary>
		/// The stacks one gathered member wants: one, or one for every place it is gathered
		/// from where the places read different rules.
		/// </summary>
		IEnumerable<(string Name, string Element)> Runs(RuleSymbol owner, DirectMember member)
		{
			if (member.Shape == MemberShape.Pieces)
			{
				yield return ("Spans", "long");

				yield break;
			}

			if (Places(owner, member) is { Count: > 0 } places)
			{
				foreach (var slot in places)
					yield return (Named(machine.RuleAt(owner, slot)!), Held(machine.RuleAt(owner, slot)!));

				yield break;
			}

			yield return (Stack(owner, member), Held(Element(owner, member)));
		}

		/// <remarks>
		/// Where the stacks stood when the rule began, so that a part collects from there
		/// and not from where the part did. The same mark the immediate carrier hands over.
		/// </remarks>
		public override string GatherHanding(RuleSymbol owner, bool declared, bool inBody)
		{
			var text = new System.Text.StringBuilder();

			foreach (var stack in Gathered(owner))
				text.Append(declared ? $", int refs_{stack}" : inBody ? $", rb_{stack}" : $", refs_{stack}");

			return text.ToString();
		}

		/// <summary>The stacks one rule gathers on, in one order.</summary>
		IEnumerable<string> Gathered(RuleSymbol owner)
		{
			var seen = new List<string>();

			foreach (var member in Union(owner).Concat(Union(owner, steps: true)))
				if (member.Shape is MemberShape.Pieces or MemberShape.Records)
					foreach (var (stack, _) in Runs(owner, member))
						if (!seen.Contains(stack))
						{
							seen.Add(stack);

							yield return stack;
						}
		}

		public override IEnumerable<string> MarkRecords(string name) => [];

		public override IEnumerable<string> MarkGathered(RuleSymbol? owner, string name)
		{
			if (owner is null)
				yield break;

			foreach (var stack in Gathered(owner))
				yield return $"var {name}_{stack} = values.Count{stack};";
		}

		public override IEnumerable<string> UnwindRecords(string name) => [];

		public override IEnumerable<string> UnwindGathered(RuleSymbol? owner, string name)
		{
			if (owner is null)
				yield break;

			foreach (var stack in Gathered(owner))
				yield return $"values.Count{stack} = {name}_{stack};";
		}

		public override string DeclareRecordLocal(int slot, RuleSymbol rule) => $"{Held(rule)} r{slot} = default;";

		public override string RecordLocalType(RuleSymbol rule) => Held(rule) + " ";

		public override string ResetRecordLocal(int slot) => $"r{slot} = default;";

		public override string Absent(RuleSymbol rule, string local) => Nothing(rule, local);

		public override string FirstRecord(IReadOnlyList<int> slots, RuleSymbol rule)
		{
			if (slots.Count == 1)
				return $"r{slots[0]}";

			// Where the places read different rules the shape keeps one field for each, and
			// what is handed over is each of them: there is no one expression, the locals
			// not sharing a type.
			if (_rule is not null && Places(_rule, machine.MemberOfSlot(_rule, slots[0])!) is { Count: > 0 })
				return string.Join(", ", slots.Select(one => $"r{one}"));

			var chain = "default";

			for (var i = slots.Count - 1; i >= 0; i--)
				chain = $"!({Nothing(rule, "r" + slots[i])}) ? r{slots[i]} : {chain}";

			return chain;
		}

		public override string Begin(RuleSymbol rule, int factory, string? start, string? end)
		{
			_rule    = rule;
			_factory = factory;
			_puts.Clear();

			return "";
		}

		/// <remarks>Where it stands and not what it says: the text is cut when it is built.</remarks>
		public override string PutText(DirectMember member, string from, string to)
		{
			_puts.Add((member, $"{from}, {to}"));

			return "";
		}

		public override string PutRecord(DirectMember member, string record)
		{
			_puts.Add((member, record));

			return "";
		}

		/// <summary>The shape, made now; what it is worth is asked for after the parse.</summary>
		/// <remarks>
		/// A turn of a fold is not the rule's value but one link of its run: made, hung on
		/// the end of what is there, and counted. The rule's own shape is assembled when the
		/// turns are done (<see cref="Folded"/>).
		/// </remarks>
		public override string End(string gatheredFrom)
		{
			var rule = _rule ?? throw new InvalidOperationException("A record ended that never began.");
			var made = machine.DirectMembers(rule, _factory).Select(Value);
			var call = $"{string.Join(", ", made)}";

			if (!Folds(rule))
				return $"{Register(rule)} = {Named(rule)}.{Maker(Which(rule, _factory))}({call});";

			if (!machine.IsStep(rule, _factory))
				return $"{Base(rule)} = {Named(rule, "Base")}.{Maker(Turn(rule, _factory))}({call});";

			var link = $"{Named(rule, "Step")} made = {Named(rule, "Step")}.{Maker(Turn(rule, _factory))}({call});";

			return
				$"{link} if ({Last(rule, "last")} == null) {Last(rule, "first")} = made; " +
				$"else {Last(rule, "last")}.Next = made; {Last(rule, "last")} = made; {Last(rule, "count")}++;";

			string Value(DirectMember member)
			{
				foreach (var (put, value) in _puts)
					if (ReferenceEquals(put, member) || put.Index == member.Index)
						return value;

				throw new InvalidOperationException($"Member '{member.Member.Name}' of '{rule.Name}' was never put.");
			}
		}

		/// <summary>The rule's own shape, once its turns are done: the base, the run, its length.</summary>
		public override string Folded(RuleSymbol owner) =>
			Folds(owner)
				? $"{Register(owner)} = {Named(owner)}.Of({Base(owner)}, {Last(owner, "first")}, {Last(owner, "count")});"
				: "";

		/// <remarks>
		/// The run, and not the value so far: a turn is linked where it is made, so there is
		/// nothing to move afterwards. The base waits here too, because the rule's shape is
		/// made of all four at once and a shape is not written twice.
		/// </remarks>
		public override IEnumerable<(string Type, string Name)> FoldState(RuleSymbol owner)
		{
			yield return ($"{Named(owner, "Base")} ", Base(owner));
			yield return ($"{Named(owner, "Step")}? ", Last(owner, "first"));
			yield return ($"{Named(owner, "Step")}? ", Last(owner, "last"));
			yield return ("int ", Last(owner, "count"));
		}

		public override string Accumulated(RuleSymbol owner) => "";

		public override string DeclareAccumulator(RuleSymbol rule) =>
			string.Join(" ", FoldState(rule).Select(one => $"{one.Type}{one.Name} = default;"));

		public override string PutAccumulator() => "";

		public override string Last(RuleSymbol rule) => Register(rule);

		public override IEnumerable<string> Rent()
		{
			if (Gathers)
				yield return "var values = MixedValues.Rent();";
		}

		public override IEnumerable<string> Return()
		{
			if (Gathers)
				yield return "MixedValues.Return(values);";
		}

		/// <summary>What a parse rents to gather on: a stack per kind of thing gathered.</summary>
		/// <remarks>
		/// Only the stacks, and only where something gathers. What a rule read is in the
		/// reader and in the shapes it made; this is the one thing a run of elements has
		/// nowhere else to go, the elements of a `*` being unable to point at one another
		/// the way a fold's turns can.
		/// </remarks>
		public override string RenderStore(IReadOnlyList<string> valueTypes, string? stateType)
		{
			if (!Gathers)
				return "";

			var file = new Writer(0);

			file.Line("/// <summary>What a mixed parse gathers on: a stack per kind of element (Machine.Mixed.cs).</summary>");

			using (file.Block("sealed class MixedValues"))
			{
				foreach (var (stack, element) in Stacks)
					Stacked(file, stack, element);

				file.Line("[global::System.ThreadStatic]");
				file.Line("static MixedValues? _spare;");
				file.Line();

				using (file.Block("internal static MixedValues Rent()"))
				{
					file.Line("var spare = _spare;");
					file.Line();
					file.Line("if (spare == null)");
					file.Then("return new MixedValues();");
					file.Line();
					file.Line("_spare = null;");
					file.Line();
					file.Line("return spare;");
				}

				file.Line();

				using (file.Block("internal static void Return(MixedValues values)"))
				{
					foreach (var (stack, _) in Stacks)
					{
						// Only what something was pushed on: the rest are asked, which is a
						// compare, rather than told, which was a call.
						using (file.Block($"if (values.High{stack} > 0)"))
						{
							if (stack != "Spans")
								file.Line($"global::System.Array.Clear(values.Stack{stack}, 0, values.High{stack});");

							file.Line($"values.Count{stack} = values.High{stack} = 0;");
						}

						file.Line();
					}

					file.Line("_spare = values;");
				}
			}

			return file.ToString();
		}

		/// <summary>One stack: what is on it, how much of it, and how much it has ever held.</summary>
		static void Stacked(Writer file, string stack, string element)
		{
			file.Line($"internal {element}[] Stack{stack} = new {element}[8];");
			file.Line($"internal int Count{stack};");
			file.Line($"internal int High{stack};");
			file.Line();

			using (file.Block($"internal void Push{stack}({element} item)"))
			{
				file.Line($"if (Count{stack} == Stack{stack}.Length)");
				file.Then($"global::System.Array.Resize(ref Stack{stack}, Count{stack} * 2);");
				file.Line();
				file.Line($"Stack{stack}[Count{stack}++] = item;");
				file.Line();
				file.Line($"if (Count{stack} > High{stack}) High{stack} = Count{stack};");
			}

			file.Line();
			file.Line("/// <summary>What was pushed since the mark, as one array, and the stack back at it.</summary>");

			using (file.Block($"internal {element}[] Take{stack}(int from)"))
			{
				file.Line($"var count = Count{stack} - from;");
				file.Line();
				file.Line("if (count == 0)");
				file.Then($"return global::System.Array.Empty<{element}>();");
				file.Line();
				file.Line($"var taken = new {element}[count];");
				file.Line();
				file.Line($"global::System.Array.Copy(Stack{stack}, from, taken, 0, count);");
				file.Line();
				file.Line($"Count{stack} = from;");
				file.Line();
				file.Line("return taken;");
			}

			file.Line();
		}

		public override IEnumerable<string> BuildRoot(RuleSymbol rule, string type, bool extent)
		{
			yield return $"value = reader.{Register(rule)}.Build({Given});";
		}

		/// <summary>
		/// The places a member is captured, where they read different rules, and nothing
		/// where they all read one and a single field holds it.
		/// </summary>
		IReadOnlyList<int> Places(RuleSymbol owner, DirectMember member)
		{
			if (member.Shape is not (MemberShape.Record or MemberShape.Records))
				return [];

			// The member as the whole rule sees it: one construction knows only its own
			// places, and what the shape holds is a field for every one of them.
			var whole = Union(owner).Concat(Union(owner, steps: true))
				.FirstOrDefault(one => one.Index == member.Index) ?? member;

			if (whole.Slots.Count < 2)
				return [];

			var first = machine.RuleAt(owner, whole.Slots[0]);

			foreach (var slot in whole.Slots)
				if (!ReferenceEquals(machine.RuleAt(owner, slot), first))
					return whole.Slots;

			return [];
		}

		/// <summary>Whether §4.3 turned the rule into a base and a run of turns.</summary>
		bool Folds(RuleSymbol rule) => machine._graph.Folds.ContainsKey(rule);

		/// <summary>The locals a folding rule carries, named after it.</summary>
		static string Base(RuleSymbol rule) => "base_" + CSharpEmitter.IdentifierOf(rule);

		static string Last(RuleSymbol rule, string what) => what + "_" + CSharpEmitter.IdentifierOf(rule);

		/// <summary>How many things a rule's shape may be.</summary>
		/// <remarks>
		/// A rule with no <c>=&gt;</c> builds its own type out of its members, which is one
		/// way of being like any other. A folding rule has two counts, its bases' and its
		/// turns', because they are two shapes.
		/// </remarks>
		int Ways(RuleSymbol rule) => Math.Max(1, Constructions(rule, steps: false).Count);

		int Turns(RuleSymbol rule) => Constructions(rule, steps: true).Count;

		/// <summary>The factories of a rule that are turns of its fold, or that are not.</summary>
		List<int> Constructions(RuleSymbol rule, bool steps)
		{
			var found = new List<int>();

			for (var i = 0; i < machine._factories[rule].Count; i++)
				if (machine.IsStep(rule, i) == steps)
					found.Add(i);

			return found;
		}

		/// <summary>Which of its kind a factory is, and which factory that one is.</summary>
		int Which(RuleSymbol rule, int factory) => machine._factories[rule].Count == 0 ? 0 : factory;

		int Turn(RuleSymbol rule, int factory) =>
			machine._factories[rule].Count == 0 ? 0 : Constructions(rule, machine.IsStep(rule, factory)).IndexOf(factory);

		int Factory(RuleSymbol rule, int which, bool steps)
		{
			var found = Constructions(rule, steps);

			return found.Count == 0 ? -1 : found[which];
		}

		static string Maker(int which) => "Of" + which.ToString(System.Globalization.CultureInfo.InvariantCulture);

		/// <summary>Every member of every construction of one kind, each named once and in one order.</summary>
		IReadOnlyList<DirectMember> Union(RuleSymbol rule, bool steps = false)
		{
			var union = new List<DirectMember>();
			var found = Constructions(rule, steps);

			if (found.Count == 0)
				found.Add(-1);

			foreach (var factory in found)
				foreach (var member in machine.DirectMembers(rule, factory))
				{
					var already = union.FindIndex(one => one.Index == member.Index);

					if (already < 0)
					{
						union.Add(member);

						continue;
					}

					// Every place the member is captured, across the constructions: one of
					// them may read a rule another does not.
					var slots = new List<int>(union[already].Slots);

					foreach (var slot in member.Slots)
						if (!slots.Contains(slot))
							slots.Add(slot);

					slots.Sort();

					union[already] = union[already] with { Slots = slots };
				}

			union.Sort((a, b) => a.Index.CompareTo(b.Index));

			return union;
		}

		/// <summary>
		/// One shape per rule: what it read, which way it read it, and what it is worth once
		/// the parse is accepted. A folding rule has three — the base, one turn of the run,
		/// and the rule itself, which is the base and the run and how long it is.
		/// </summary>
		/// <remarks>
		/// One type for the rule and a byte saying which construction it holds, rather than a
		/// type per construction and a virtual call to build it. The fields are the union of
		/// the constructions', so a rule of thirty alternatives has a shape wide enough for
		/// all of them; whether that is worth what it saves is what the yardsticks are for
		/// (docs/next.md).
		/// </remarks>
		public override string RenderBuilder(IReadOnlyList<RuleSymbol> rules)
		{
			var file = new Writer(0);

			foreach (var rule in Shaped)
			{
				if (Folds(rule))
				{
					Shape(file, rule, "Base", steps: false);
					Turned(file, rule);
					Run(file, rule);

					continue;
				}

				Shape(file, rule, null, steps: false);
			}

			return file.ToString();
		}

		/// <summary>A shape that holds one reading of a rule and knows what it is worth.</summary>
		void Shape(Writer file, RuleSymbol rule, string? part, bool steps)
		{
			var fields = Fields(rule, Union(rule, steps)).ToList();
			var ways   = Math.Max(1, Constructions(rule, steps).Count);
			var name   = Named(rule, part);
			var value  = part is null;

			file.Line($"/// <summary>What <c>{rule.Name}</c> read, and what it is worth (Machine.Mixed.cs).</summary>");

			using (file.Block(
				Reference(rule) && value
					? $"private sealed class {name}"
					: $"private readonly struct {name}"))
			{
				foreach (var (type, field) in fields)
					file.Line($"private readonly {type} {field};");

				if (ways > 1)
					file.Line("private readonly byte which;");

				// A shape read is a shape made, and one never made is a member that was not
				// there. A class says that by being null; a struct has no null to say it
				// with, so it says it here.
				if (!(Reference(rule) && value))
					file.Line("private readonly bool read;");

				file.Line();

				var taken = fields.ConvertAll(one => one.Type + " " + one.Name.Substring(1));

				if (ways > 1)
					taken.Insert(0, "byte which");

				file.Line($"private {name}({string.Join(", ", taken)})");

				using (file.Block(""))
				{
					if (ways > 1)
						file.Line("this.which = which;");

					foreach (var (_, field) in fields)
						file.Line($"this.{field} = {field.Substring(1)};");

					if (!(Reference(rule) && value))
						file.Line("this.read = true;");
				}

				for (var which = 0; which < ways; which++)
					Maker(file, rule, name, fields, ways, which, steps);

				file.Line();

				if (!(Reference(rule) && value))
				{
					file.Line("/// <summary>Whether the reading that would have made this one ever happened.</summary>");
					file.Line("internal bool IsNothing { get { return !this.read; } }");
					file.Line();
				}

				file.Line($"internal {machine._results.ValueOf(rule)} Build({Taking})");

				using (file.Block(""))
					Builds(file, rule, ways, steps, null);

				Gatherings(file, rule, Union(rule, steps));
			}

			file.Line();
		}

		/// <summary>One turn of a fold: what it captured, and the turn after it.</summary>
		/// <remarks>
		/// A class, and the only shape here that is one whatever the rule is: the run is
		/// threaded through its own turns, so a turn has to be something the turn before it
		/// can point at. That is what makes the run cost nothing beside the turns — no array
		/// grown per parse, and nothing to grow it into the large object heap.
		/// </remarks>
		void Turned(Writer file, RuleSymbol rule)
		{
			var fields = Fields(rule, Union(rule, steps: true)).ToList();
			var ways   = Turns(rule);
			var name   = Named(rule, "Step");

			file.Line($"/// <summary>One turn of <c>{rule.Name}</c>'s fold, and the turn after it (Machine.Mixed.cs).</summary>");

			using (file.Block($"private sealed class {name}"))
			{
				foreach (var (type, field) in fields)
					file.Line($"private readonly {type} {field};");

				if (ways > 1)
					file.Line("private readonly byte which;");

				file.Line();
				file.Line($"internal {name}? Next;");
				file.Line();

				var taken = fields.ConvertAll(one => one.Type + " " + one.Name.Substring(1));

				if (ways > 1)
					taken.Insert(0, "byte which");

				file.Line($"private {name}({string.Join(", ", taken)})");

				using (file.Block(""))
				{
					if (ways > 1)
						file.Line("this.which = which;");

					foreach (var (_, field) in fields)
						file.Line($"this.{field} = {field.Substring(1)};");
				}

				for (var which = 0; which < ways; which++)
					Maker(file, rule, name, fields, ways, which, steps: true);

				file.Line();
				file.Line(
					$"internal {machine._results.ValueOf(rule)} Build(" +
					$"{machine._results.ValueOf(rule)} value, {Taking})");

				using (file.Block(""))
					Builds(file, rule, ways, steps: true, accumulator: "value");

				Gatherings(file, rule, Union(rule, steps: true));
			}

			file.Line();
		}

		/// <summary>The rule itself: the base, the run over it, and how long the run is.</summary>
		void Run(Writer file, RuleSymbol rule)
		{
			var name = Named(rule);
			var step = Named(rule, "Step");
			var made = Named(rule, "Base");

			file.Line($"/// <summary>What <c>{rule.Name}</c> read: a base and a run of turns over it (Machine.Mixed.cs).</summary>");

			using (file.Block(
				Reference(rule) ? $"private sealed class {name}" : $"private readonly struct {name}"))
			{
				file.Line($"private readonly {made} _base;");
				file.Line($"private readonly {step}? _first;");
				file.Line("private readonly int _count;");

				if (!Reference(rule))
					file.Line("private readonly bool read;");

				file.Line();
				file.Line($"private {name}({made} one, {step}? first, int count)");

				using (file.Block(""))
				{
					file.Line("this._base  = one;");
					file.Line("this._first = first;");
					file.Line("this._count = count;");

					if (!Reference(rule))
						file.Line("this.read = true;");
				}

				file.Line();
				file.Line($"internal static {name} Of({made} one, {step}? first, int count)");

				using (file.Block(""))
					file.Line($"return new {name}(one, first, count);");

				file.Line();

				if (!Reference(rule))
				{
					file.Line("/// <summary>Whether the reading that would have made this one ever happened.</summary>");
					file.Line("internal bool IsNothing { get { return !this.read; } }");
					file.Line();
				}

				file.Line($"internal {machine._results.ValueOf(rule)} Build({Taking})");

				using (file.Block(""))
				{
					// Counted rather than tested against null: the count is known when the
					// turns are done and costs nothing to keep, and the loop then has one
					// exit. A frame per turn is what this is written to avoid.
					file.Line($"var value = this._base.Build({Given});");
					file.Line("var step  = this._first;");
					file.Line();

					using (file.Block("for (var turn = 0; turn < this._count; turn++)"))
					{
						file.Line($"value = step!.Build(value, {Given});");
						file.Line("step  = step.Next;");
					}

					file.Line();
					file.Line("return value;");
				}
			}

			file.Line();
		}

		/// <summary>
		/// What a gathered member is worth: the elements it kept, each asked what it is
		/// worth, in an array of the author's own — or, for pieces of text, cut and joined.
		/// </summary>
		void Gatherings(Writer file, RuleSymbol rule, IReadOnlyList<DirectMember> members)
		{
			foreach (var member in members)
			{
				if (member.Shape is not (MemberShape.Pieces or MemberShape.Records))
					continue;

				if (member.Shape == MemberShape.Records && Places(rule, member) is { Count: > 0 } places)
				{
					foreach (var slot in places)
						Gathering(file, rule, member, $"_g{member.Index}_{slot}", machine.RuleAt(rule, slot)!);

					continue;
				}

				Gathering(
					file, rule, member, $"_g{member.Index}",
					member.Shape == MemberShape.Pieces ? null : Element(rule, member));
			}
		}

		/// <summary>One run, as the author's own array or as the text its pieces join into.</summary>
		void Gathering(Writer file, RuleSymbol rule, DirectMember member, string run, RuleSymbol? element)
		{
			var made = element is null ? "string" : machine._results.ValueOf(member.Member.Rule) + "[]";

			file.Line();
			file.Line($"private {made} {Named(run)}({Taking})");

			using (file.Block(""))
			{
				if (element is null)
				{
					file.Line($"var made = new string[this.{run}.Length];");
					file.Line();

					using (file.Block("for (var one = 0; one < made.Length; one++)"))
					{
						file.Line($"var span = this.{run}[one];");
						file.Line("var from = (int)(span >> 32);");
						file.Line();
						file.Line($"made[one] = {machine.Cut("from", "(int)(uint)span - from")};");
					}

					file.Line();
					file.Line("return string.Concat(made);");

					return;
				}

				file.Line($"var made = new {machine._results.ValueOf(member.Member.Rule)}[this.{run}.Length];");
				file.Line();
				file.Line("for (var one = 0; one < made.Length; one++)");
				file.Then($"made[one] = this.{run}[one]{(Reference(element) ? "!" : "")}.Build({Given});");
				file.Line();
				file.Line("return made;");
			}
		}

		/// <summary>One way of making a shape: its own fields filled, and the rest left absent.</summary>
		void Maker(
			Writer file, RuleSymbol rule, string name,
			IReadOnlyList<(string Type, string Name)> fields, int ways, int which, bool steps)
		{
			var mine   = Fields(rule, machine.DirectMembers(rule, Factory(rule, which, steps))).ToList();
			var passed = new List<string>();

			if (ways > 1)
				passed.Add(which.ToString(System.Globalization.CultureInfo.InvariantCulture));

			// What a construction does not have, as what stands for absent: a run of text
			// that was never read begins nowhere.
			foreach (var (kind, field) in fields)
				passed.Add(
					mine.Exists(one => one.Name == field) ? field.Substring(1) :
					field[1] == 'a' || field[1] == 'b'    ? "-1" :
					field[1] == 'g'                       ? Empty(kind) :
					                                        "default");

			file.Line();
			file.Line(
				$"internal static {name} {Maker(which)}(" +
				$"{string.Join(", ", mine.ConvertAll(one => one.Type + " " + one.Name.Substring(1)))})");

			using (file.Block(""))
				file.Line($"return new {name}({string.Join(", ", passed)});");
		}

		/// <summary>
		/// A run nothing gathered. Not <c>default</c>, which for an array is null and which
		/// a construction reading it would have to be told about; the empty one is shared
		/// and costs nothing.
		/// </summary>
		static string Empty(string kind) =>
			$"global::System.Array.Empty<{kind.Substring(0, kind.Length - 2)}>()";

		/// <summary>What a shape is worth: one construction, or a switch over which it holds.</summary>
		void Builds(Writer file, RuleSymbol rule, int ways, bool steps, string? accumulator)
		{
			if (ways == 1)
			{
				file.Line($"return {Built(rule, Factory(rule, 0, steps), accumulator)};");

				return;
			}

			using (file.Block("switch (this.which)"))
			{
				for (var which = 0; which < ways - 1; which++)
				{
					file.Line($"case {which}:");

					using (file.Indent())
						file.Line($"return {Built(rule, Factory(rule, which, steps), accumulator)};");
				}

				file.Line("default:");

				using (file.Indent())
					file.Line($"return {Built(rule, Factory(rule, ways - 1, steps), accumulator)};");
			}
		}

		/// <summary>
		/// A shape's fields, in the order its constructor takes them: two integers where a
		/// member is a run of text, and the captured rule's own shape where it is a record.
		/// </summary>
		IEnumerable<(string Type, string Name)> Fields(RuleSymbol owner, IReadOnlyList<DirectMember> members)
		{
			foreach (var member in members)
				if (member.Shape == MemberShape.Text)
				{
					yield return ("int", $"_a{member.Index}");
					yield return ("int", $"_b{member.Index}");
				}
				else if (member.Shape is MemberShape.Pieces or MemberShape.Records)
				{
					// A run per place where the places read different rules: `CASE WHEN` and
					// `CASE x WHEN` gather their whens from two rules, and one reading of the
					// rule gathers from one of them, its alternative having chosen.
					if (Places(owner, member) is { Count: > 0 } runs)
					{
						foreach (var slot in runs)
							if (member.Slots.Contains(slot))
								yield return (Held(machine.RuleAt(owner, slot)!) + "[]", $"_g{member.Index}_{slot}");
					}
					else
					{
						yield return (
							(member.Shape == MemberShape.Pieces ? "long" : Held(Element(owner, member))) + "[]",
							$"_g{member.Index}");
					}
				}
				else if (Places(owner, member) is { Count: > 0 } places)
				{
					// Those of them this construction captures: the shape holds a field for
					// every place, and a maker takes only the ones its own reading fills.
					foreach (var slot in places)
						if (member.Slots.Contains(slot))
							yield return (Held(machine.RuleAt(owner, slot)!), $"_m{member.Index}_{slot}");
				}
				else
				{
					yield return (Held(member.Member.Rule!), $"_m{member.Index}");
				}
		}

		/// <summary>One construction, over the fields the shape kept for it.</summary>
		string Built(RuleSymbol rule, int factory, string? accumulator)
		{
			var members = machine.DirectMembers(rule, factory);

			if (factory < 0)
				return $"new {machine._results.QualifiedOf(rule)!}({string.Join(", ", members.Select(Value))})";

			var made      = machine._factories[rule][factory];
			var arguments = machine.DirectArguments(
				rule, made, members,
				() => "text.ToString()", () => "default",
				() => accumulator ?? "default!", Value);

			return $"{made.Method}({string.Join(", ", arguments)})";

			string Value(DirectMember member)
			{
				if (member.Shape == MemberShape.Record && Places(rule, member) is { Count: > 0 } places)
				{
					var chain = "default";

					for (var i = places.Count - 1; i >= 0; i--)
					{
						var one = $"this._m{member.Index}_{places[i]}";

						chain = $"!({Nothing(machine.RuleAt(rule, places[i])!, one)}) ? {one}{Sure(machine.RuleAt(rule, places[i])!)}.Build({Given}) : {chain}";
					}

					return "(" + chain + ")";
				}

				return Held(member);
			}

			string Held(DirectMember member) =>
				member.Shape is MemberShape.Pieces or MemberShape.Records
					? $"this.{Named(Places(rule, member) is { Count: > 0 } ? $"_g{member.Index}_{member.Slots[0]}" : $"_g{member.Index}")}({Given})"
				: member.Shape == MemberShape.Text
					? $"(this._a{member.Index} < 0 ? {(member.Member.IsOptional ? "null" : "string.Empty")} : " +
						machine.Cut($"this._a{member.Index}", $"this._b{member.Index} - this._a{member.Index}") + ")"
					: member.Member.IsOptional
						? $"({Nothing(member.Member.Rule!, "this._m" + member.Index)} ? default : this._m{member.Index}{Sure(member.Member.Rule!)}.Build({Given}))"
						: $"this._m{member.Index}{Sure(member.Member.Rule!)}.Build({Given})";

			// A shape held by reference is nullable where it stands, and read only where
			// the reading that filled it happened: the compiler is told so rather than
			// warning about it in somebody else's build.
			string Sure(RuleSymbol one) => Reference(one) ? "!" : "";
		}

		// What the shapes written so far do not need, and Refuses keeps the machine from
		// asking for.

		/// <remarks>The stacks are the store's; a rule declares only where it began on them.</remarks>
		public override IEnumerable<string> DeclareGathered(int slot, string elementType) => [];

		public override string Collect(DirectMember member, string from, bool pairs)
		{
			var stack = Stack(_rule!, member);

			_puts.Add((member, $"values.Take{stack}({from}_{stack})"));

			return "";
		}

		/// <remarks>
		/// Where the piece stands and not what it says, in one number: the text is cut when
		/// it is built, like every other run of text here, so a reading given back has cut
		/// nothing.
		/// </remarks>
		public override string PushText(int slot, string from, string to) =>
			$"values.PushSpans(((long){from} << 32) | (uint){to});";

		public override string PushRecord(int slot, RuleSymbol rule) =>
			$"values.Push{Named(rule)}({Register(rule)});";

		public override string Mark(int kind, int site) => throw Unwritten();

		/// <remarks>
		/// Nothing: a shape is a shape the moment it is made, and what it is worth is one
		/// call away. This is where the tape does its walk over the log for one record.
		/// </remarks>
		public override string Materialize(string record, string sinceMark) => "";

		/// <remarks>
		/// Built where the guard stands, and again where the rule's own value is: a guard
		/// that reads a capture reads it before the derivation is accepted, and a
		/// construction is not called twice by anything else. §7.3 counts what an accepted
		/// derivation runs, and a guard is not one of those.
		/// </remarks>
		public override string ValueOf(RuleSymbol rule, string record) => $"{record}.Build({Given})";

		public override void Gathered(
			Writer code, string from, IReadOnlyList<int> slots, string handed, string type, string build, bool text) =>
			throw Unwritten();

		static InvalidOperationException Unwritten() =>
			new($"The mixed carrier was asked to carry something it refuses ({nameof(Machine)}.Mixed.cs).");
	}
}
