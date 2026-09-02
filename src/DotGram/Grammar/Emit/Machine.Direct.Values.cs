using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;

namespace DotGram.Grammar.Emit;

/// <summary>
/// Values in the direct rendering: what a rule records about what it captured, and the
/// walk that builds the values from those records once the parse has accepted.
/// </summary>
/// <remarks>
/// <para>
/// The engine records derivation as arena entries and materializes by walking linked lists
/// of them per completed call. Here a valued rule writes one record at its end, after its
/// children have written theirs: the rule, which of its factories, where it began and
/// ended, and then its members in order — a text capture as its start and end, a captured
/// rule as the index of that rule's record, a collected sequence as a count and then each
/// one. Every record begins with its own length, so building is one walk from the front,
/// each record's children already built by the time it is reached, and the last record
/// written is the root's.
/// </para>
/// <para>
/// Backtracking keeps §7.2's promise the same way the arena does: a construct that runs
/// again, or fails outward, first puts the log back to where it stood when the construct
/// began, so a record of a reading given back is gone before anything could build it.
/// </para>
/// </remarks>
sealed partial class Machine
{
	/// <summary>How one member of a rule's value stands in the record.</summary>
	enum MemberShape
	{
		/// <summary>Start and end, or -1 and -1 where nothing was captured.</summary>
		Text,

		/// <summary>A count, then the start and end of each piece: the text is their join.</summary>
		Pieces,

		/// <summary>The index of the captured rule's record, or -1.</summary>
		Record,

		/// <summary>A count, then the record of each element.</summary>
		Records,
	}

	sealed record DirectMember(ResultMember Member, int Index, MemberShape Shape, IReadOnlyList<int> Slots)
	{
		/// <summary>The slots as a mask, for the side stack to collect by.</summary>
		public long Mask
		{
			get
			{
				var mask = 0L;

				foreach (var slot in Slots)
					mask |= 1L << slot;

				return mask;
			}
		}
	}

	/// <summary>The members of a rule's value, each with the shape its record holds it in.</summary>
	List<DirectMember> DirectMembers(RuleSymbol rule)
	{
		var offset  = _captureOffsets[rule];
		var members = _graph.Results[rule];
		var shaped  = new List<DirectMember>(members.Count);

		for (var i = 0; i < members.Count; i++)
		{
			var member = members[i];
			var shape  = member.Rule is null
				? member.Slots.Any(slot => _repeatedCaptures.Contains(offset + slot)) ? MemberShape.Pieces : MemberShape.Text
				: member.IsSequence ? MemberShape.Records : MemberShape.Record;

			shaped.Add(new DirectMember(member, i, shape, member.Slots));
		}

		return shaped;
	}

	/// <summary>The member a capture slot of a rule belongs to, if any.</summary>
	DirectMember? MemberOfSlot(RuleSymbol rule, int slot)
	{
		foreach (var member in DirectMembers(rule))
			if (member.Slots.Contains(slot))
				return member;

		return null;
	}

	/// <summary>Whether a rule keeps a value at all, and so writes a record.</summary>
	bool Valued(RuleSymbol rule) => ValueRule(rule) >= 0;

	/// <summary>Whether a rule writes its record where its body ends, having no construction of its own.</summary>
	bool RecordsAtEnd(RuleSymbol rule) => Valued(rule) && _factories[rule].Count == 0;

	/// <summary>
	/// Whether a rule can be read by methods with its value kept: everything the direct
	/// rendering knows how to record, and nothing it does not yet.
	/// </summary>
	bool DirectValuedRule(RuleSymbol rule)
	{
		if (!Valued(rule))
			return true;

		if (_graph.Folds.ContainsKey(rule) || _graph.Externals.ContainsKey(rule))
			return false;

		var layout = CaptureLayout.Of(
			_graph.Bodies[rule], other => _results.QualifiedOf(other) is not null);

		if (layout.Slots.Count > 60)
			return false;

		foreach (var factory in _factories[rule])
		{
			if (CSharpEmitter.Asks(_graph, factory, "parserInput") ||
				CSharpEmitter.Asks(_graph, factory, "parserState") ||
				_graph.ContextOf(rule) is not null && CSharpEmitter.Asks(_graph, factory, "context"))
			{
				return false;
			}
		}

		return true;
	}

	/// <summary>The materializer for one direct machine: a walk over the log, a switch per rule.</summary>
	string RenderDirectMaterializer(IReadOnlyList<RuleSymbol> rules)
	{
		var file = new Writer(0);

		file.Line("/// <summary>Builds the values a direct parse recorded, front to back (Machine.Direct.Values.cs).</summary>");

		using (file.Block(
			$"static void {DirectMaterializer}(" +
			$"{WaysType} ways, global::System.ReadOnlySpan<char> text, DirectValues values{TokensParameter})"))
		{
			file.Line("values.Room(ways.LogCount);");
			file.Line();
			file.Line("var log  = ways.Log;");
			file.Line("var live = values.Live;");
			file.Line();
			// What the root reaches, and nothing else: a valued rule that matched without being
			// captured is in the log, and its factory must not run (docs/syntax.md �7.2).
			file.Line("var starts = values.Starts;");
			file.Line("var listed = 0;");
			file.Line();
			file.Line("for (var at = 0; at < ways.LogCount; at += log[at])");
			file.Then("starts[listed++] = at;");
			file.Line();
			file.Line("live[ways.Last] = true;");
			file.Line();
			using (file.Block("for (var back = listed - 1; back >= 0; back--)"))
			{
				file.Line("var at = starts[back];");
				file.Line();
				file.Line("if (!live[at]) continue;");
				file.Line();
				file.Line("var read = at + 5;");
				file.Line();
				using (file.Block("switch (log[at + 1])"))
					foreach (var rule in rules)
						if (Valued(rule))
							MarkDirectRule(file, rule);
			}


			for (var i = 0; i < _valueTypes.Count; i++)
				file.Line($"var values{i} = values.V{i};");

			file.Line();

			using (file.Block("for (var at = 0; at < ways.LogCount; at += log[at])"))
			{
				file.Line("if (!live[at]) continue;");
				file.Line();
				file.Line("var factory = log[at + 2];");
				file.Line("var start   = log[at + 3];");
				file.Line("var end     = log[at + 4];");
				file.Line("var read    = at + 5;");
				file.Line();

				using (file.Block("switch (log[at + 1])"))
					foreach (var rule in rules)
						if (Valued(rule))
							MaterializeDirectRule(file, rule);
			}
		}

		return file.ToString();
	}

	string DirectMaterializer => $"Materialize_DotGram{_tag}_Direct";

	void MaterializeDirectRule(Writer file, RuleSymbol rule)
	{
		var type      = _results.QualifiedOf(rule)!;
		var members   = DirectMembers(rule);
		var factories = _factories[rule];

		using (file.Block($"case {_ruleIds[rule]}:"))
		{
			if (IsExtent(rule))
			{
				// An extent is never put anywhere: whoever captured it reads its record.
				file.Line("break;");

				return;
			}

			if (_reread is not null && _reread.Contains(rule))
			{
				// A terminal that builds: the lexer measured it, and the character machine of its
				// own builds it from the text.
				file.Line($"{ValueInto(type, "at")} = Value_{CSharpEmitter.IdentifierOf(rule)}_DotGram({Cut("start", "end - start")});");
				file.Line("break;");

				return;
			}

			foreach (var member in members)
				ReadMember(file, member);

			if (factories.Count == 0)
			{
				file.Line($"{ValueInto(type, "at")} = new {type}(");

				using (file.Indent())
					for (var i = 0; i < members.Count; i++)
						file.Line(
							$"captured{i}{(members[i].Member.IsOptional ? "" : "!")}" +
							(i + 1 < members.Count ? "," : ");"));
			}
			else if (factories.Count == 1)
			{
				file.Line(
					$"{ValueInto(type, "at")} = " +
					$"{factories[0].Method}({string.Join(", ", DirectArguments(factories[0], members))});");
			}
			else
			{
				using (file.Block("switch (factory)"))
					for (var factoryIndex = 0; factoryIndex < factories.Count; factoryIndex++)
					{
						file.Line($"case {factoryIndex}:");

						using (file.Indent())
						{
							file.Line(
								$"{ValueInto(type, "at")} = " +
								$"{factories[factoryIndex].Method}({string.Join(", ", DirectArguments(factories[factoryIndex], members))});");
							file.Line("break;");
						}
					}
			}

			file.Line("break;");
		}
	}

	/// <summary>Marks what one record names as reached, given that the record itself is.</summary>
	void MarkDirectRule(Writer file, RuleSymbol rule)
	{
		using (file.Block($"case {_ruleIds[rule]}:"))
		{
			if (!IsExtent(rule) && (_reread is null || !_reread.Contains(rule)))
				foreach (var member in DirectMembers(rule))
					switch (member.Shape)
					{
						case MemberShape.Text:
							file.Line("read += 2;");
							break;

						case MemberShape.Pieces:
							file.Line("read += 1 + log[read] * 2;");
							break;

						case MemberShape.Record:
							file.Line("if (log[read] >= 0) live[log[read]] = true;");
							file.Line("read++;");
							break;

						case MemberShape.Records:
							file.Line("for (var item = 0; item < log[read]; item++)");
							file.Then("live[log[read + 1 + item]] = true;");
							file.Line("read += 1 + log[read];");
							break;
					}

			file.Line("break;");
		}
	}

	/// <summary>Reads one member out of the record into <c>captured{i}</c>.</summary>
	void ReadMember(Writer file, DirectMember member)
	{
		var i        = member.Index;
		var optional = member.Member.IsOptional;

		switch (member.Shape)
		{
			case MemberShape.Text:
				file.Line($"var from{i} = log[read++];");
				file.Line($"var to{i}   = log[read++];");
				file.Line(
					$"var captured{i} = from{i} < 0 ? {(optional ? "null" : "string.Empty")} : " +
					Cut($"from{i}", $"to{i} - from{i}") + ";");
				break;

			case MemberShape.Pieces:
				file.Line($"var count{i} = log[read++];");
				file.Line($"string{(optional ? "?" : "")} captured{i};");

				using (file.Block($"if (count{i} == 0)"))
					file.Line($"captured{i} = {(optional ? "null" : "string.Empty")};");

				using (file.Block($"else if (count{i} == 1)"))
				{
					file.Line($"captured{i} = {Cut("log[read]", "log[read + 1] - log[read]")};");
					file.Line("read += 2;");
				}

				using (file.Block("else"))
				{
					file.Line($"var length{i} = 0;");
					file.Line();
					file.Line($"for (var piece = 0; piece < count{i}; piece++)");
					file.Then($"length{i} += log[read + piece * 2 + 1] - log[read + piece * 2];");
					file.Line();
					file.Line($"var chars{i} = new char[length{i}];");
					file.Line($"var filled{i} = 0;");
					file.Line();

					using (file.Block($"for (var piece = 0; piece < count{i}; piece++)"))
					{
						file.Line("var pieceFrom = log[read++];");
						file.Line("var pieceTo   = log[read++];");

						var text = OverKinds
							? "global::System.MemoryExtensions.AsSpan(" + Cut("pieceFrom", "pieceTo - pieceFrom") + ")"
							: "text.Slice(pieceFrom, pieceTo - pieceFrom)";

						file.Line(
							$"{text}.CopyTo(new global::System.Span<char>(chars{i}, filled{i}, pieceTo - pieceFrom));");
						file.Line($"filled{i} += pieceTo - pieceFrom;");
					}

					file.Line();
					file.Line($"captured{i} = new string(chars{i});");
				}

				break;

			case MemberShape.Record:
			{
				var valueType = _results.ValueOf(member.Member.Rule);

				file.Line($"var record{i} = log[read++];");
				file.Line(
					optional
						? $"{valueType}? captured{i} = record{i} < 0 ? default({valueType}?) : {RecordValue(valueType, $"record{i}")};"
						: $"var captured{i} = {RecordValue(valueType, $"record{i}")};");
				break;
			}

			case MemberShape.Records:
			{
				var valueType = _results.ValueOf(member.Member.Rule);

				file.Line($"var count{i} = log[read++];");
				var bracket = valueType.IndexOf('[');
				var created = bracket < 0
					? $"new {valueType}[count{i}]"
					: $"new {valueType.Substring(0, bracket)}[count{i}]{valueType.Substring(bracket)}";

				file.Line($"var captured{i} = {created};");
				file.Line();
				using (file.Block($"for (var item = 0; item < count{i}; item++)"))
				{
					file.Line($"var record{i} = log[read++];");
					file.Line($"captured{i}[item] = {RecordValue(valueType, $"record{i}")};");
				}
				break;
			}
		}

		file.Line();
	}

	/// <summary>The value a record holds: from its type's table, or for an extent the record itself.</summary>
	string RecordValue(string type, string record) =>
		type == "SourceSpan"
			? Span($"log[{record} + 3]", $"log[{record} + 4] - log[{record} + 3]")
			: TableFor(type) is var table && table >= 0
				? $"values{table}[{record}]"
				: throw new InvalidOperationException($"No value table for '{type}'.");

	/// <summary>The factory's arguments, in the order the factory's parameters were written.</summary>
	List<string> DirectArguments(Factory factory, IReadOnlyList<DirectMember> members)
	{
		var arguments = new List<string>();

		if (CSharpEmitter.WantsText(_graph, factory))
			arguments.Add(Cut("start", "end - start"));

		if (CSharpEmitter.Asks(_graph, factory, "parserSpan"))
			arguments.Add(Span("start", "end - start"));

		foreach (var wanted in factory.Members)
		{
			if (wanted.Name == "parserText" || wanted.Name == factory.Accumulator)
				continue;

			foreach (var member in members)
				if (member.Member.Name == wanted.Name)
				{
					arguments.Add(
						!wanted.IsOptional && member.Member is { Rule: not null, IsOptional: true }
							? $"({_results.ValueOf(member.Member.Rule)})captured{member.Index}!"
							: $"captured{member.Index}{(wanted.IsOptional ? "" : "!")}");
					break;
				}
		}

		return arguments;
	}
}
