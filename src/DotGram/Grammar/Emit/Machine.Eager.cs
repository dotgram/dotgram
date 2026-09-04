using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Emit;

sealed partial class Machine
{
	/// <summary>The class an eager parser rents: one register per value type, holding the last value built of it.</summary>
	/// <remarks>
	/// The tape hands a value from callee to caller through <c>ways.Last</c>, an index into a
	/// log; this hands it through a field of the value's own type. It is sound for the same
	/// reason the index is: every valued rule writes its own record last, after everything it
	/// captured, so when a reader returns the register of its type holds its value and nothing
	/// has come between.
	/// </remarks>
	internal static string EagerValuesClass(IReadOnlyList<string> valueTypes)
	{
		var text = new StringBuilder();

		text.Append("/// <summary>What an eager parse hands between its readers: the last value built of each type (Machine.Eager.cs).</summary>\n");
		text.Append("sealed class EagerValues\n{\n");

		for (var i = 0; i < valueTypes.Count; i++)
			text.Append("\tinternal ").Append(valueTypes[i]).Append(" Last").Append(i).Append(" = default!;\n");

		text.Append("\n\t[global::System.ThreadStatic]\n\tstatic EagerValues? _spare;\n\n");
		text.Append("\tinternal static EagerValues Rent()\n\t{\n\t\tvar spare = _spare;\n\n\t\tif (spare == null)\n\t\t\treturn new EagerValues();\n\n\t\t_spare = null;\n\n\t\treturn spare;\n\t}\n\n");
		text.Append("\tinternal static void Return(EagerValues values)\n\t{\n");

		for (var i = 0; i < valueTypes.Count; i++)
			text.Append("\t\tvalues.Last").Append(i).Append(" = default!;\n");

		text.Append("\t\t_spare = values;\n\t}\n\n");
		text.Append("\t/// <summary>Whether a local a record would have been kept in was never written.</summary>\n");
		text.Append("\tinternal static bool IsDefault<T>(T value) => global::System.Collections.Generic.EqualityComparer<T>.Default.Equals(value, default!);\n");
		text.Append("}\n");

		return text.ToString().Replace("\n", Lines.Ending);
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
	/// gives up, and <see cref="CarrierKind.Eager"/> says who is answerable for it — and
	/// leaves nothing to unwind, because nothing it made is anywhere but in locals about to
	/// go out of scope.
	/// </para>
	/// <para>
	/// A guard costs nothing extra here: what it asks for has been built already.
	/// </para>
	/// <para>
	/// <b>What it does not carry yet</b>, and refuses so that the tape does instead: marks
	/// and parser state (§7.8), extents (a <c>SourceSpan</c>-typed rule has no record to be
	/// the span of), recovery, and the valuing of a terminal over kinds. Each is a shape the
	/// first measurement did not need and the second carrier can add.
	/// </para>
	/// </remarks>
	sealed class EagerCarrier(Machine machine) : ValueCarrier
	{
		// The record under construction: what Begin was told, and the members put since.
		RuleSymbol? _rule;
		int _factory;
		string? _start, _end;
		readonly List<(DirectMember Member, string Value)> _puts = [];
		bool _accumulated;

		public override string ReaderParameter => ", EagerValues values";

		public override string ReaderArgument => ", values";

		public override string GatherHanding(RuleSymbol owner, bool declared, bool inBody)
		{
			var text = new StringBuilder();

			foreach (var (slot, element) in GatheredSlots(owner))
				text.Append(declared ? $", global::System.Collections.Generic.List<{element}> g{slot}" : $", g{slot}");

			return text.ToString();
		}

		public override IEnumerable<string> MarkRecords(string name) => [];

		/// <remarks>A failed turn has pushed into the lists; the mark is where they stood.</remarks>
		public override IEnumerable<string> MarkGathered(RuleSymbol? owner, string name)
		{
			if (owner is null)
				yield break;

			foreach (var (slot, _) in GatheredSlots(owner))
				yield return $"var {name}_{slot} = g{slot}.Count;";
		}

		public override IEnumerable<string> UnwindRecords(string name) => [];

		public override IEnumerable<string> UnwindGathered(RuleSymbol? owner, string name)
		{
			if (owner is null)
				yield break;

			foreach (var (slot, _) in GatheredSlots(owner))
				yield return $"g{slot}.RemoveRange({name}_{slot}, g{slot}.Count - {name}_{slot});";
		}

		public override string DeclareRecordLocal(int slot, string valueType) => $"{valueType} r{slot} = default!;";

		public override string DeclareAccumulator(string valueType) => $"{valueType} fold = default!;";

		public override IEnumerable<string> DeclareGathered(int slot, string elementType)
		{
			yield return $"var g{slot} = new global::System.Collections.Generic.List<{elementType}>();";
		}

		public override string RecordLocalType(string valueType) => valueType + " ";

		public override string ResetRecordLocal(int slot) => $"r{slot} = default!;";

		public override string Absent(string local) => $"EagerValues.IsDefault({local})";

		public override string FirstRecord(IReadOnlyList<int> slots, string valueType)
		{
			if (slots.Count == 1)
				return $"r{slots[0]}";

			var chain = $"default({valueType})!";

			for (var i = slots.Count - 1; i >= 0; i--)
				chain = $"!EagerValues.IsDefault(r{slots[i]}) ? r{slots[i]} : {chain}";

			return $"({chain})";
		}

		public override string Begin(RuleSymbol rule, int factory, string? start, string? end)
		{
			_rule    = rule;
			_factory = factory;
			_start   = start;
			_end     = end;
			_puts.Clear();
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
			var missing = member.Member.IsOptional ? "null" : "string.Empty";

			_puts.Add((member, $"({from} < 0 ? {missing} : {machine.Cut(from, $"{to} - {from}")})"));

			return "";
		}

		public override string PutRecord(DirectMember member, string record)
		{
			_puts.Add((member, record));

			return "";
		}

		public override string Collect(DirectMember member, string from, bool pairs)
		{
			_puts.Add((member, Joined(member.Slots, pairs)));

			return "";
		}

		/// <summary>The lists of a member's slots as one value: the text joined, or the records as an array.</summary>
		static string Joined(IReadOnlyList<int> slots, bool pairs)
		{
			if (slots.Count == 1)
				return pairs ? $"string.Concat(g{slots[0]})" : $"g{slots[0]}.ToArray()";

			var all = string.Join(", ", slots.Select(static slot => $"g{slot}"));

			return pairs
				? $"string.Concat({string.Join(", ", slots.Select(static slot => $"string.Concat(g{slot})"))})"
				: $"global::System.Linq.Enumerable.ToArray(global::System.Linq.Enumerable.Concat({all}))";
		}

		/// <summary>The construction, called now, its result in the register of the rule's type.</summary>
		public override string End(string gatheredFrom)
		{
			var rule = _rule ?? throw new InvalidOperationException("A record ended that never began.");
			var type = machine._results.QualifiedOf(rule)!;
			var into = $"values.Last{machine.TableFor(type)}";

			// The span an alternative stands on, where the record would have carried one —
			// asked for only by a factory that wants it, since asking has the file emit the
			// helper that makes one.
			var (start, end) = (_start, _end);

			string Text() => start is null ? "string.Empty" : machine.Cut(start, $"{end} - {start}");
			string Span() => start is null ? "default" : machine.Span(start, $"{end} - {start}");

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

		public override string Last(string valueType) => $"values.Last{machine.TableFor(valueType)}";

		public override string PushText(int slot, string from, string to) =>
			$"g{slot}.Add({machine.Cut(from, $"{to} - {from}")});";

		public override string PushRecord(int slot, string valueType) => $"g{slot}.Add({Last(valueType)});";

		public override string Mark(int kind, int site) =>
			throw new InvalidOperationException("The eager carrier does not carry marks; Refuses should have said so.");

		public override string Materialize(string record, string sinceMark) => "";

		public override string ValueOf(string type, string record) => record;

		public override void Gathered(Writer code, string from, IReadOnlyList<int> slots, string handed, string type, string build) =>
			code.Line($"var {handed} = {Joined(slots, pairs: false)};");

		public override IEnumerable<string> Rent()
		{
			yield return "var values = EagerValues.Rent();";
		}

		public override IEnumerable<string> Return()
		{
			yield return "EagerValues.Return(values);";
		}

		public override IEnumerable<string> BuildRoot(string type, bool extent)
		{
			yield return $"value = {Last(type)};";
		}

		public override string RenderBuilder(IReadOnlyList<RuleSymbol> rules) => "";

		public override string? Refuses()
		{
			if (machine.UsesMarks || machine._graph.State is not null)
				return "it uses marks";

			if (machine._graph.Recoveries.Count > 0)
				return "it recovers";

			if (machine._reread is not null)
				return "a terminal builds over kinds";

			foreach (var rule in machine._rules)
				if (machine.IsExtent(rule))
					return $"'{rule.Name}' is an extent";

			return null;
		}

		/// <summary>The slots a rule gathers into, with what each gathers.</summary>
		IEnumerable<(int Slot, string Element)> GatheredSlots(RuleSymbol owner)
		{
			foreach (var member in machine.DirectMembers(owner))
				if (member.Shape is MemberShape.Pieces or MemberShape.Records)
					foreach (var slot in member.Slots)
						yield return (slot, member.Shape == MemberShape.Pieces ? "string" : machine._results.ValueOf(member.Member.Rule));
		}
	}
}
