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

		// A stack per type for what a rule gathers across turns, and one for pieces of text,
		// marked and unwound by count like the tape's references and taken as one array when
		// the record is written — the one allocation a hand-written parser makes for a list.
		text.Append('\n');
		Stack(text, "string", "Text");

		for (var i = 0; i < valueTypes.Count; i++)
			Stack(text, valueTypes[i], i.ToString(global::System.Globalization.CultureInfo.InvariantCulture));

		text.Append("\n\t[global::System.ThreadStatic]\n\tstatic EagerValues? _spare;\n\n");
		text.Append("\tinternal static EagerValues Rent()\n\t{\n\t\tvar spare = _spare;\n\n\t\tif (spare == null)\n\t\t\treturn new EagerValues();\n\n\t\t_spare = null;\n\n\t\treturn spare;\n\t}\n\n");
		text.Append("\tinternal static void Return(EagerValues values)\n\t{\n");

		for (var i = 0; i < valueTypes.Count; i++)
			text.Append("\t\tvalues.Last").Append(i).Append(" = default!;\n");

		text.Append("\t\tglobal::System.Array.Clear(values.StackText, 0, values.HighText);\n\t\tvalues.CountText = values.HighText = 0;\n");

		for (var i = 0; i < valueTypes.Count; i++)
			text.Append("\t\tglobal::System.Array.Clear(values.Stack").Append(i).Append(", 0, values.High").Append(i).Append(");\n")
				.Append("\t\tvalues.Count").Append(i).Append(" = values.High").Append(i).Append(" = 0;\n");

		text.Append("\t\t_spare = values;\n\t}\n\n");
		text.Append("\t/// <summary>Whether a local a record would have been kept in was never written.</summary>\n");
		text.Append("\tinternal static bool IsDefault<T>(T value) => global::System.Collections.Generic.EqualityComparer<T>.Default.Equals(value, default!);\n");
		text.Append("}\n");

		return text.ToString().Replace("\n", Lines.Ending);

		static void Stack(StringBuilder text, string type, string tag)
		{
			text.Append("\tinternal ").Append(type).Append("[] Stack").Append(tag).Append(" = new ").Append(type).Append("[8];\n");
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
			text.Append("\t\tvar taken = new ").Append(type).Append("[count];\n\n");
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
	/// the span of), and recovery. Each is a shape the first measurement did not need and
	/// the second carrier can add.
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

		/// <remarks>
		/// The registers, and everything a construction called inside a reader may ask for:
		/// the tokens over kinds, since a text member is cut where it is read; the input and
		/// the context where any factory names them.
		/// </remarks>
		public override IEnumerable<(string Type, string Name)> ReaderState
		{
			get
			{
				yield return ("EagerValues", "values");

				if (machine.OverKinds)
					foreach (var token in Machine.TokenState)
						yield return token;

				if (machine.UsesInput)
					yield return ("string", "parserInput");

				if (machine.UsesContext)
					yield return (machine._graph.Context!, "context");
			}
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

		public override IEnumerable<string> MarkRecords(string name) => [];

		/// <remarks>A failed turn has pushed onto the stacks; the mark is where they stood.</remarks>
		public override IEnumerable<string> MarkGathered(RuleSymbol? owner, string name)
		{
			if (owner is null)
				yield break;

			foreach (var stack in GatheredStacks(owner))
				yield return $"var {name}_{stack} = values.Count{stack};";
		}

		public override IEnumerable<string> UnwindRecords(string name) => [];

		public override IEnumerable<string> UnwindGathered(RuleSymbol? owner, string name)
		{
			if (owner is null)
				yield break;

			foreach (var stack in GatheredStacks(owner))
				yield return $"values.Count{stack} = {name}_{stack};";
		}

		public override string DeclareRecordLocal(int slot, string valueType) => $"{valueType} r{slot} = default!;";

		public override string DeclareAccumulator(string valueType) => $"{valueType} fold = default!;";

		public override IEnumerable<string> DeclareGathered(int slot, string elementType) => [];

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

		/// <summary>
		/// What the rule pushed since it began, taken off its type's stack as one array — the
		/// text joined where the member is pieces of it.
		/// </summary>
		public override string Collect(DirectMember member, string from, bool pairs)
		{
			var stack = pairs ? "Text" : StackOf(machine._results.ValueOf(member.Member.Rule));
			var taken = $"values.Take{stack}({from}_{stack})";

			_puts.Add((member, pairs ? $"string.Concat({taken})" : taken));

			return "";
		}

		/// <summary>The construction, called now, its result in the register of the rule's type.</summary>
		public override string End(string gatheredFrom)
		{
			var rule = _rule ?? throw new InvalidOperationException("A record ended that never began.");
			var type = machine._results.QualifiedOf(rule)!;
			var into = $"values.Last{machine.TableFor(type)}";

			// A terminal that builds, over kinds: the lexer measured it, and the character
			// machine of its own builds it from the text — now rather than in the walk.
			if (machine._reread is not null && machine._reread.Contains(rule))
			{
				if (_start is null)
					throw new InvalidOperationException($"'{rule.Name}' builds from its text and its reader keeps no positions.");

				return $"{into} = Value_{CSharpEmitter.IdentifierOf(rule)}_DotGram({machine.Cut(_start, $"{_end} - {_start}")});";
			}

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
			$"values.PushText({machine.Cut(from, $"{to} - {from}")});";

		public override string PushRecord(int slot, string valueType) =>
			$"values.Push{StackOf(valueType)}({Last(valueType)});";

		public override string Mark(int kind, int site) =>
			throw new InvalidOperationException("The eager carrier does not carry marks; Refuses should have said so.");

		public override string Materialize(string record, string sinceMark) => "";

		public override string ValueOf(string type, string record) => record;

		/// <remarks>Peeked rather than taken: the record written later collects the same items.</remarks>
		public override void Gathered(Writer code, string from, IReadOnlyList<int> slots, string handed, string type, string build, bool text)
		{
			var stack = text ? "Text" : StackOf(type);

			code.Line($"var {handed} = values.Peek{stack}({from}_{stack});");
		}

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

			foreach (var rule in machine._rules)
				if (machine.IsExtent(rule))
					return $"'{rule.Name}' is an extent";

			return null;
		}

		/// <summary>The stacks a rule gathers on — one per type its gathered members have, and the text's.</summary>
		IEnumerable<string> GatheredStacks(RuleSymbol owner)
		{
			var seen = new HashSet<string>(StringComparer.Ordinal);

			foreach (var member in machine.DirectMembers(owner))
			{
				var stack = member.Shape switch
				{
					MemberShape.Pieces  => "Text",
					MemberShape.Records => StackOf(machine._results.ValueOf(member.Member.Rule)),
					_                   => null,
				};

				if (stack is not null && seen.Add(stack))
					yield return stack;
			}
		}

		/// <summary>The stack a type's gathered values go on: the one numbered as its table is.</summary>
		string StackOf(string valueType) =>
			machine.TableFor(valueType) is var table && table >= 0
				? table.ToString(System.Globalization.CultureInfo.InvariantCulture)
				: throw new InvalidOperationException($"No value table for '{valueType}'.");
	}
}
