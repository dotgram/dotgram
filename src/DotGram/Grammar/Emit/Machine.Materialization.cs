using System;
using System.Collections.Generic;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;

namespace DotGram.Grammar.Emit;

/// <summary>
/// Turning an accepted derivation into values, once there is one.
/// </summary>
/// <remarks>
/// Recognition records and construction runs afterwards, over what was accepted rather
/// than over everything that was tried (docs/implementation.md §3). So this is a pass of
/// its own: it starts when the parse has succeeded, walks the arena the parse left, and
/// knows about grammars only through the types their rules can produce — one table for
/// each, so that a value which is a struct is not boxed on its way in.
/// </remarks>
sealed partial class Machine
{
	void EnsureMaterializer()
	{
		if (_materializer)
			return;

		_materializer = true;

		var helper = new Writer(0);

		using (helper.Block(
			"static void Materialize_DotGram(global::System.ReadOnlySpan<char> text, Parser parser, " +
			"ParserArena entries)"))
			Materialize(helper, cached: true);

		_extra.Add(helper.ToString());
	}

	void Materialize(Writer file, bool cached) =>
		MaterializeRange(file, "0", cached);

	/// <summary>
	/// Turns an accepted derivation into values, bounded to what changed since the last call.
	/// </summary>
	/// <remarks>
	/// <paramref name="fromExpr"/> is <c>"0"</c> for a fresh parse's one full sweep — the
	/// whole arena is what changed, because nothing before it was ever materialized. An eager
	/// materializer passes a rule's own call index instead, and gets the same walk narrowed to
	/// one subtree: <see cref="MaterializeRule"/> already reads a rule's captures purely
	/// through <c>linkHeads</c>/<c>linkNexts</c> starting from its own <c>completedAt</c>, so
	/// bounding where the owner-marking and build sweeps start is the whole of what a caller
	/// has to say to ask for less than everything.
	/// </remarks>
	void MaterializeRange(Writer file, string fromExpr, bool cached)
	{
		file.Line("var values = parser.Materialization(entries.Count);");
		DeclareTables(file);
		if (cached)
			file.Line("var built  = parser.Materialized();");
		file.Line("var linkHeads = parser.MaterializationHeads();");
		file.Line("var linkNexts = parser.MaterializationNexts();");
		file.Line();

		// Grown, not rebuilt from zero: a link written on an earlier, smaller call is still
		// the answer for that index, so only what has not been linked yet needs the walk.
		using (file.Block(
			"for (var derivationAt = parser.LinkedUpTo; derivationAt < entries.Count; derivationAt++)"))
		{
			file.Line("var derivation = entries[derivationAt];");

			var linked =
				"derivation.Kind == ParserEntry.Capture || derivation.Kind == ParserEntry.RuleCapture || " +
				"derivation.Kind == ParserEntry.Construct";

			if (_recoveryPlans.Count > 0)
				linked += " || derivation.Kind == ParserEntry.Recovery";

			using (file.Block($"if (derivation.CallIndex >= 0 && ({linked}))"))
			{
				file.Line("linkNexts[derivationAt] = linkHeads[derivation.CallIndex];");
				file.Line("linkHeads[derivation.CallIndex] = derivationAt;");
			}
		}
		file.Line("parser.LinkedUpTo = entries.Count;");

		// A transparent rule may have completed before a surrounding path backtracked and
		// selected another derivation. Such completed entries can remain useful as history,
		// but only calls reached through RuleCapture from the accepted root may run user
		// construction code. Call entries precede their children, so one forward pass marks
		// the complete accepted value tree without recursion or another typed collection.
		file.Line();
		if (!cached)
			file.Line($"values[{fromExpr}] = parser;");
		using (file.Block($"for (var ownerAt = {fromExpr}; ownerAt < entries.Count; ownerAt++)"))
		{
			file.Line("if (!global::System.Object.ReferenceEquals(values[ownerAt], parser)) continue;");

			using (file.Block(
				"for (var capturedAt = linkHeads[ownerAt]; capturedAt >= 0; " +
				"capturedAt = linkNexts[capturedAt])"))
			{
				file.Line("var candidate = entries[capturedAt];");
				file.Line("if (candidate.Kind == ParserEntry.RuleCapture" +
					(cached ? " && !built[candidate.Position]" : "") + ")");
				file.Then("values[candidate.Position] = parser;");
			}
		}

		if (_recoveryPlans.Count > 0)
		{
			file.Line();

			using (file.Block($"for (var recoveryAt = {fromExpr}; recoveryAt < entries.Count; recoveryAt++)"))
			{
				file.Line("var recovered = entries[recoveryAt];");
				file.Line(
					"if (recovered.Kind != ParserEntry.Recovery || recovered.CallIndex < 0 || " +
					(cached ? "built[recoveryAt] || " : "") +
					"!global::System.Object.ReferenceEquals(values[recoveryAt], parser) && " +
					"!global::System.Object.ReferenceEquals(values[recovered.CallIndex], parser)) continue;");

				using (file.Block("switch (recovered.State)"))
					foreach (var recovery in _recoveryPlans)
						MaterializeRecovery(file, recovery);

				if (cached)
					file.Line("built[recoveryAt] = true;");
			}
		}

		using (file.Block(
			$"for (var completedAt = entries.Count - 1; completedAt >= {fromExpr}; completedAt--)"))
		{
			file.Line("var completed = entries[completedAt];");
			file.Line(
				"if (completed.Kind != ParserEntry.Completed || " +
				(cached ? "built[completedAt] || " : "") +
				"!global::System.Object.ReferenceEquals(values[completedAt], parser)) continue;");

			using (file.Block("switch (completed.RuleIndex)"))
				foreach (var rule in _graph.Rules)
					if (ValueRule(rule) >= 0)
						MaterializeRule(file, rule);

			if (cached)
				file.Line("built[completedAt] = true;");
		}
	}

	bool IsExtent(RuleSymbol rule) =>
		_graph.Types.TryGetValue(rule, out var type) && type == "SourceSpan";

	/// <summary>
	/// Reading a rule's value out of the parse — which for an extent means not reading it.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The value table is <c>object?</c> because one automaton serves every rule: what
	/// completed at a position could be any of their types, and the same slot doubles as the
	/// mark saying a value is reachable and unbuilt. That is what puts a struct in a box.
	/// </para>
	/// <para>
	/// An extent needs no slot at all. The entry left where the rule completed already holds
	/// the position it began at and the position it reached — the span is those two numbers,
	/// one array along, and copying them into the table would box them to say what is already
	/// said.
	/// </para>
	/// </remarks>
	string ValueFrom(string type, string index) =>
		type == "SourceSpan"
			? $"new SourceSpan(entries[{index}].Position, entries[{index}].Value - entries[{index}].Position)"
			: TableFor(type) is var table && table >= 0
				? $"values{table}[{index}]"
				: $"({type})values[{index}]!";

	/// <summary>Writing a rule's value where whatever reads it will look.</summary>
	/// <remarks>
	/// The object table keeps the mark rather than the value, so it is cleared here: what
	/// stops a value being built twice is that the mark is gone, and the mark is the only
	/// thing that was ever in there.
	/// </remarks>
	/// <remarks>
	/// A value used to take the mark's place by being written over it. It no longer does —
	/// the mark says a value is reachable and unbuilt, and it now outlives the building. What
	/// stops a second build is <c>built</c>, which is what the guard path always used; the
	/// walk that only runs once never needed either.
	/// </remarks>
	string ValueInto(string type, string index) =>
		TableFor(type) is var table && table >= 0 ? $"values{table}[{index}]" : $"values[{index}]";

	/// <summary>
	/// Handing the root's value out, which is the one place a type has to be forgotten.
	/// </summary>
	/// <remarks>
	/// It leaves through <c>out object?</c>, because the recognizer serves every publication
	/// and they do not agree on a type. So a struct is boxed here — once, at the boundary,
	/// where a caller is about to be handed the answer anyway, rather than once per value
	/// built on the way to it. Which table to take it from is the one thing not known until
	/// the call: <c>rootRule</c> says which rule was asked for.
	/// </remarks>
	void RootValue(Writer file)
	{
		using (file.Block("switch (rootRule)"))
		{
			foreach (var rule in _graph.Rules)
			{
				if (ValueRule(rule) < 0)
					continue;

				file.Line($"case {_ruleIds[rule]}:");

				using (file.Indent())
				{
					// An extent was never put anywhere: the wrapper works it out from the
					// position it gave and the one it was told.
					file.Line(IsExtent(rule)
						? "recognized = null;"
						: $"recognized = {ValueFrom(_results.QualifiedOf(rule)!, "0")};");
					file.Line("break;");
				}
			}

			file.Line("default:");

			using (file.Indent())
			{
				file.Line("recognized = values[0];");
				file.Line("break;");
			}
		}
	}

	/// <summary>The tables in view wherever values are read or written.</summary>
	void DeclareTables(Writer writer)
	{
		for (var i = 0; i < _valueTypes.Count; i++)
			writer.Line($"var values{i} = parser.Materialization{i}();");
	}

	void MaterializeRule(Writer file, RuleSymbol rule)
	{
		var offset    = _captureOffsets[rule];
		var members   = _graph.Results[rule];
		var type      = _results.QualifiedOf(rule)!;
		var factories = _factories[rule];

		// Its value is the entry it left, so the walk has nothing to run here. The case is
		// still written: the switch is over every rule that has a value, and this one has.
		if (IsExtent(rule))
		{
			using (file.Block($"case {_ruleIds[rule]}:"))
				file.Line("break;");

			return;
		}

		using (file.Block($"case {_ruleIds[rule]}:"))
		{
			if (_graph.Folds.ContainsKey(rule))
			{
				MaterializeFold(file, rule, type, offset, factories);
				file.Line("break;");

				return;
			}

			for (var memberIndex = 0; memberIndex < members.Count; memberIndex++)
			{
				var member = members[memberIndex];
				var slots  = new List<string>(member.Slots.Count);

				foreach (var slot in member.Slots)
					slots.Add($"candidate.State == {offset + slot}");

				if (member.Rule is not null)
				{
					if (member.IsSequence)
					{
						var element = _results.ValueOf(member.Rule);
						var recovered = new List<string>();

						foreach (var plan in _recoveryPlans)
							if (plan.Rule == rule && plan.Recovery.Factory is not null)
								foreach (var slot in member.Slots)
									if (plan.Slot == offset + slot)
										recovered.Add($"candidate.Kind == ParserEntry.Recovery && candidate.State == {plan.Id}");

						var accepted =
							$"candidate.Kind == ParserEntry.RuleCapture && candidate.CallIndex == completedAt && " +
							$"({string.Join(" || ", slots)})";
						var collected = recovered.Count == 0
							? accepted
							: $"({accepted}) || ({string.Join(" || ", recovered)})";

						file.Line($"var captured{memberIndex}Count = 0;");

						using (file.Block(
							$"for (var capturedAt{memberIndex} = linkHeads[completedAt]; capturedAt{memberIndex} >= 0; " +
							$"capturedAt{memberIndex} = linkNexts[capturedAt{memberIndex}])"))
						{
							file.Line($"var candidate = entries[capturedAt{memberIndex}];");
							file.Line($"if ({collected}) captured{memberIndex}Count++;");
						}

						file.Line($"var captured{memberIndex} = new {element}[captured{memberIndex}Count];");
						file.Line($"var captured{memberIndex}Item = captured{memberIndex}Count;");

						using (file.Block(
							$"for (var capturedAt{memberIndex} = linkHeads[completedAt]; capturedAt{memberIndex} >= 0; " +
							$"capturedAt{memberIndex} = linkNexts[capturedAt{memberIndex}])"))
						{
							file.Line($"var candidate = entries[capturedAt{memberIndex}];");

							using (file.Block($"if ({collected})"))
							{
								if (recovered.Count > 0)
								{
									file.Line($"var capturedValueAt = candidate.Kind == ParserEntry.Recovery ? capturedAt{memberIndex} : candidate.Position;");
									file.Line(
										$"captured{memberIndex}[--captured{memberIndex}Item] = " +
										ValueFrom(element, "capturedValueAt") + ";");
								}
								else
									file.Line(
										$"captured{memberIndex}[--captured{memberIndex}Item] = " +
										ValueFrom(element, "candidate.Position") + ";");
							}
						}

						file.Line();

						continue;
					}

					file.Line($"var captured{memberIndex}At = -1;");

					using (file.Block(
						$"for (var capturedAt{memberIndex} = linkHeads[completedAt]; capturedAt{memberIndex} >= 0; " +
						$"capturedAt{memberIndex} = linkNexts[capturedAt{memberIndex}])"))
					{
						file.Line($"var candidate = entries[capturedAt{memberIndex}];");

						using (file.Block(
							$"if (candidate.Kind == ParserEntry.RuleCapture && candidate.CallIndex == completedAt && " +
							$"({string.Join(" || ", slots)}))"))
						{
							file.Line($"captured{memberIndex}At = candidate.Position;");
							file.Line("break;");
						}
					}

					var capturedType = _results.ValueOf(member.Rule);

					if (!member.IsOptional)
						file.Line($"global::System.Diagnostics.Debug.Assert(captured{memberIndex}At >= 0);");

					file.Line(member.IsOptional
						? $"{capturedType}? captured{memberIndex} = captured{memberIndex}At < 0 ? " +
							$"default({capturedType}?) : " +
							ValueFrom(capturedType, $"captured{memberIndex}At") + ";"
						: $"var captured{memberIndex} = " +
							ValueFrom(capturedType, $"captured{memberIndex}At") + ";");
					file.Line();

					continue;
				}

				file.Line($"var captured{memberIndex}From = -1;");
				file.Line($"var captured{memberIndex}To   = -1;");

				using (file.Block(
					$"for (var capturedAt{memberIndex} = linkHeads[completedAt]; capturedAt{memberIndex} >= 0; " +
					$"capturedAt{memberIndex} = linkNexts[capturedAt{memberIndex}])"))
				{
					file.Line($"var candidate = entries[capturedAt{memberIndex}];");

					using (file.Block(
						$"if (candidate.Kind == ParserEntry.Capture && candidate.CallIndex == completedAt && " +
						$"({string.Join(" || ", slots)}))"))
					{
						file.Line($"if (captured{memberIndex}To < 0)");
						file.Then($"captured{memberIndex}To = candidate.Value;");
						file.Line($"captured{memberIndex}From = candidate.Position;");
					}
				}

				file.Line(
					$"var captured{memberIndex} = captured{memberIndex}From < 0 ? " +
					(member.IsOptional ? "null" : "string.Empty") + " : " +
					$"text.Slice(captured{memberIndex}From, captured{memberIndex}To - " +
					$"captured{memberIndex}From).ToString();");
				file.Line();
			}

		if (factories.Count == 0)
		{
			file.Line($"{ValueInto(type, "completedAt")} = new {type}(");

			using (file.Indent())
				for (var i = 0; i < members.Count; i++)
					file.Line(
						$"captured{i}{(members[i].IsOptional ? "" : "!")}" +
						(i + 1 < members.Count ? "," : ");"));
		}
		else
		{
			file.Line("var chosen = -1;");

			using (file.Block(
				"for (var chosenAt = linkHeads[completedAt]; chosenAt >= 0; " +
				"chosenAt = linkNexts[chosenAt])"))
			{
				file.Line("var candidate = entries[chosenAt];");

				using (file.Block(
					"if (candidate.Kind == ParserEntry.Construct && candidate.CallIndex == completedAt)"))
				{
					file.Line("chosen = candidate.State;");
					file.Line("break;");
				}
			}

			file.Line("global::System.Diagnostics.Debug.Assert(chosen >= 0);");

			using (file.Block("switch (chosen)"))
				for (var factoryIndex = 0; factoryIndex < factories.Count; factoryIndex++)
				{
					var factory = factories[factoryIndex];
					var arguments = new List<string>();

					// Materialized only where the expression names it. It is the whole of
					// what the rule matched, so building it for an expression that never
					// looks at it doubles what a parse allocates — twice the string, for a
					// rule whose value is the capture inside it.
					if (CSharpEmitter.WantsText(factory))
						arguments.Add(
							"text.Slice(completed.Position, completed.Value - completed.Position).ToString()");

					if (CSharpEmitter.Asks(factory, "parserSpan"))
						arguments.Add(
							"new SourceSpan(" +
							"completed.Position, completed.Value - completed.Position)");

					foreach (var member in factory.Members)
					{
						if (member.Name == "parserText" || member.Name == factory.Accumulator)
							continue;

						for (var memberIndex = 0; memberIndex < members.Count; memberIndex++)
							if (members[memberIndex].Name == member.Name)
							{
								arguments.Add(
									!member.IsOptional && members[memberIndex] is { Rule: not null, IsOptional: true }
										? $"({_results.ValueOf(members[memberIndex].Rule)})captured{memberIndex}!"
										: $"captured{memberIndex}{(member.IsOptional ? "" : "!")}");
								break;
							}
					}

					file.Line($"case {factoryIndex}:");

					using (file.Indent())
					{
						file.Line(
							$"{ValueInto(type, "completedAt")} = " +
							$"{factory.Method}({string.Join(", ", arguments)});");
						file.Line("break;");
					}
				}
		}

		file.Line("break;");
	}
	}

	void MaterializeFold(
		Writer file, RuleSymbol rule, string type, int offset,
		IReadOnlyList<Factory> factories)
	{
		file.Line($"{type} accumulated = default!;");
		file.Line("var hasAccumulated = false;");
		file.Line("var partFrom = completedAt + 1;");
		file.Line();

		using (file.Block(
			"for (var constructAt = completedAt + 1; constructAt < entries.Count; constructAt++)"))
		{
			file.Line("var construct = entries[constructAt];");
			file.Line(
				"if (construct.Kind != ParserEntry.Construct || construct.CallIndex != completedAt) continue;");
			file.Line();

			using (file.Block("switch (construct.State)"))
				for (var factoryIndex = 0; factoryIndex < factories.Count; factoryIndex++)
				{
					var factory = factories[factoryIndex];

					file.Line($"case {factoryIndex}:");

					using (file.Indent())
					using (file.Block(""))
					{
						var arguments = new List<string>();

						if (CSharpEmitter.WantsText(factory))
							arguments.Add(
								"text.Slice(completed.Position, completed.Value - completed.Position).ToString()");

						if (CSharpEmitter.Asks(factory, "parserSpan"))
							arguments.Add(
								"new SourceSpan(" +
								"completed.Position, completed.Value - completed.Position)");

						if (factory.Accumulator is not null)
						{
							file.Line("global::System.Diagnostics.Debug.Assert(hasAccumulated);");
							arguments.Add("accumulated");
						}

						var captured = 0;

						foreach (var member in factory.Members)
						{
							if (member.Name == "parserText" || member.Name == factory.Accumulator)
								continue;

							arguments.Add(MaterializeFoldMember(
								file, rule, member, captured++, offset,
								sequence: member.IsSequence && factory.Accumulator is null));
						}

						file.Line(
							$"accumulated = {factory.Method}({string.Join(", ", arguments)});");
						file.Line("hasAccumulated = true;");
						file.Line("break;");
					}
				}

			file.Line();
			file.Line("partFrom = constructAt + 1;");
		}

		file.Line();
		file.Line("global::System.Diagnostics.Debug.Assert(hasAccumulated);");
		file.Line($"{ValueInto(type, "completedAt")} = accumulated;");
	}

	string MaterializeFoldMember(
		Writer file, RuleSymbol rule, ResultMember member, int memberIndex, int offset, bool sequence)
	{
		var slots = new List<string>(member.Slots.Count);

		foreach (var slot in member.Slots)
			slots.Add($"candidate.State == {offset + slot}");

		var kind = member.Rule is null ? "ParserEntry.Capture" : "ParserEntry.RuleCapture";
		var test =
			$"candidate.Kind == {kind} && candidate.CallIndex == completedAt && " +
			$"({string.Join(" || ", slots)})";

		if (sequence)
		{
			var element = member.Rule is null ? "string" : _results.ValueOf(member.Rule);

			file.Line($"var foldCaptured{memberIndex}Count = 0;");

			using (file.Block(
				$"for (var candidateAt = partFrom; candidateAt < constructAt; candidateAt++)"))
			{
				file.Line("var candidate = entries[candidateAt];");
				file.Line($"if ({test}) foldCaptured{memberIndex}Count++;");
			}

			file.Line($"var foldCaptured{memberIndex} = new {element}[foldCaptured{memberIndex}Count];");
			file.Line($"var foldCaptured{memberIndex}Item = 0;");

			using (file.Block(
				$"for (var candidateAt = partFrom; candidateAt < constructAt; candidateAt++)"))
			{
				file.Line("var candidate = entries[candidateAt];");

				using (file.Block($"if ({test})"))
					file.Line(member.Rule is null
						? $"foldCaptured{memberIndex}[foldCaptured{memberIndex}Item++] = " +
							"text.Slice(candidate.Position, candidate.Value - candidate.Position).ToString();"
						: $"foldCaptured{memberIndex}[foldCaptured{memberIndex}Item++] = " +
							ValueFrom(element, "candidate.Position") + ";");
			}

			return $"foldCaptured{memberIndex}";
		}

		file.Line($"var foldCaptured{memberIndex}At = -1;");

		using (file.Block(
			$"for (var candidateAt = partFrom; candidateAt < constructAt; candidateAt++)"))
		{
			file.Line("var candidate = entries[candidateAt];");

			using (file.Block($"if ({test})"))
				file.Line($"foldCaptured{memberIndex}At = candidateAt;");
		}

		var type = member.Rule is null ? "string" : _results.ValueOf(member.Rule);

		if (!member.IsOptional)
			file.Line($"global::System.Diagnostics.Debug.Assert(foldCaptured{memberIndex}At >= 0);");

		file.Line(member.Rule is null
			? $"var foldCaptured{memberIndex} = foldCaptured{memberIndex}At < 0 ? " +
				(member.IsOptional ? "null" : "string.Empty") + " : " +
				$"text.Slice(entries[foldCaptured{memberIndex}At].Position, " +
				$"entries[foldCaptured{memberIndex}At].Value - " +
				$"entries[foldCaptured{memberIndex}At].Position).ToString();"
			: member.IsOptional
				? $"{type}? foldCaptured{memberIndex} = foldCaptured{memberIndex}At < 0 ? " +
					$"default({type}?) : " +
					ValueFrom(type, $"entries[foldCaptured{memberIndex}At].Position") + ";"
				: $"var foldCaptured{memberIndex} = " +
					ValueFrom(type, $"entries[foldCaptured{memberIndex}At].Position") + ";");

		return $"foldCaptured{memberIndex}{(member.IsOptional ? "" : "!")}";
	}
}
