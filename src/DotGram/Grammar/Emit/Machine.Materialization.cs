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
	/// <summary>
	/// The walk, as a method of its own rather than written out wherever a parse accepts.
	/// </summary>
	/// <remarks>
	/// Emitted for every grammar that builds anything, not only for one whose guards read a
	/// value mid-parse. The automaton is one method and the walk is large; leaving it inline
	/// grew the one method whose size docs/next.md's "Future optimization gate" measured as
	/// the thing still worth moving. Naming it also lets a profiler say what materialization
	/// costs, which nothing could while it was part of a method that is also the whole
	/// recognizer.
	/// </remarks>
	void EnsureMaterializer()
	{
		if (_materializer)
			return;

		_materializer = true;

		var helper = new Writer(0);

		using (helper.Block(
			$"static void Materialize_DotGram{_tag}(global::System.ReadOnlySpan<char> text, Parser parser, " +
			$"ParserArena entries{InputParameter})"))
			Materialize(helper, cached: Caches);

		_extra.Add(helper.ToString());
	}

	/// <summary>
	/// Turns an accepted derivation into values, walking whatever the arena holds since the
	/// last call.
	/// </summary>
	void Materialize(Writer file, bool cached)
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
		// construction code.
		//
		// Two shapes, and which one applies is the same question `cached` already answers:
		// what the marks mean when this is entered.
		//
		// Without caching this runs once, at `Accept:`, and the root call is the only thing
		// marked — so the accepted value tree can be reached from it, and the calls it
		// reaches kept rather than looked for a second time. A URL scans twenty-odd entries
		// twice to act on two.
		//
		// With caching a `when` guard calls this mid-parse, having marked whichever values
		// its own condition asks for: the marked set is then not reachable from the root
		// and not knowable without looking, so the scan is still what finds it. Seeding a
		// walk from the root there would silently build a different set from the one the
		// guard asked about, which is the sort of thing the guard tests in
		// `GeneratorDriverTests` exist to catch, and did.
		file.Line();

		if (cached)
		{
			using (file.Block("for (var ownerAt = 0; ownerAt < entries.Count; ownerAt++)"))
			{
				file.Line("if (!global::System.Object.ReferenceEquals(values[ownerAt], parser)) continue;");

				using (file.Block(
					"for (var capturedAt = linkHeads[ownerAt]; capturedAt >= 0; " +
					"capturedAt = linkNexts[capturedAt])"))
				{
					file.Line("var candidate = entries[capturedAt];");
					file.Line("if (candidate.Kind == ParserEntry.RuleCapture && !built[candidate.Position])");
					file.Then("values[candidate.Position] = parser;");
				}
			}
		}
		else
		{
			file.Line("values[0] = parser;");
			file.Line();
			file.Line("var owners     = parser.MaterializationOwners();");
			file.Line("var ownerCount = 0;");
			file.Line();
			file.Line("owners[ownerCount++] = 0;");
			file.Line();

			// Grows while it is walked, which is the whole of the breadth-first order: a
			// call appended here is one whose own captures have not been looked at yet.
			// The mark doubles as the guard against keeping the same call twice — a scan
			// visits an index once however many ways it was reached, and a list would
			// otherwise run that call's `=>` once per way.
			using (file.Block("for (var ownerIndex = 0; ownerIndex < ownerCount; ownerIndex++)"))
			{
				file.Line("var ownerAt = owners[ownerIndex];");
				file.Line();

				using (file.Block(
					"for (var capturedAt = linkHeads[ownerAt]; capturedAt >= 0; " +
					"capturedAt = linkNexts[capturedAt])"))
				{
					file.Line("var candidate = entries[capturedAt];");
					file.Line();

					using (file.Block(
						"if (candidate.Kind == ParserEntry.RuleCapture && " +
						"!global::System.Object.ReferenceEquals(values[candidate.Position], parser))"))
					{
						file.Line("values[candidate.Position] = parser;");
						file.Line("owners[ownerCount++] = candidate.Position;");
					}
				}
			}
		}

		if (_recoveryPlans.Count > 0)
		{
			file.Line();

			using (file.Block("for (var recoveryAt = 0; recoveryAt < entries.Count; recoveryAt++)"))
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

		// A value has to be built before the call that reads it is, and both shapes say so
		// the same way round, by different means.
		//
		// The list was built outwards from the root, so back to front is every child before
		// its parent whatever the arena looks like. Descending index says the same only
		// because a child's call is written after its parent's — true, and true by an
		// invariant a walk over a list does not have to depend on.
		//
		// The kind and the mark are tested in both: one compare each against something
		// already in hand, and neither is a fact this walk is the right place to be
		// certain about.
		file.Line();

		using (file.Block(cached
			? "for (var completedAt = entries.Count - 1; completedAt >= 0; completedAt--)"
			: "for (var ownerIndex = ownerCount - 1; ownerIndex >= 0; ownerIndex--)"))
		{
			if (!cached)
				file.Line("var completedAt = owners[ownerIndex];");

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

		// A value-returning external recognizer (§7.1's third row) has no captures to walk
		// and no factory to call — recognition already ran the method once, speculatively,
		// keeping only the bool and the position it moved (Machine.cs's Node.External
		// case). Its value is recovered the same way any other typed value is here: on
		// demand, from what the arena recorded — completed.Position is where the call
		// began (Machine.cs's Return: label), which is all the method needs to be asked
		// again.
		if (_graph.Externals.TryGetValue(rule, out var method))
		{
			using (file.Block($"case {_ruleIds[rule]}:"))
			{
				file.Line("var externalPos = completed.Position;");
				file.Line($"{method}(text, ref externalPos, out var externalValue);");
				file.Line($"{ValueInto(type, "completedAt")} = externalValue;");
				file.Line("break;");
			}

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

			// One walk of a call's captures, not one per member. The list is chained through
			// `linkNexts`, so every step of it is a load from somewhere else in the arena
			// again — walking it once per member had a rule of five members chase the same
			// pointers five times. `MaterializationCost.cs` is what said this was worth
			// finding: of what captures cost, seven eighths is this machinery and one
			// eighth is the strings it ends in.
			//
			// A sequence member keeps two walks of its own. It has to count before it can
			// fill, which is a different shape from collecting one position, and merging it
			// in would mean counting for members that are not sequences.
			var scalars = new List<int>();

			for (var memberIndex = 0; memberIndex < members.Count; memberIndex++)
			{
				var member = members[memberIndex];

				if (member is { Rule: not null, IsSequence: true })
					continue;

				scalars.Add(memberIndex);

				if (member.Rule is not null)
					file.Line($"var captured{memberIndex}At = -1;");
				else
				{
					file.Line($"var captured{memberIndex}From = -1;");
					file.Line($"var captured{memberIndex}To   = -1;");
				}
			}

			if (scalars.Count > 0)
			{
				using (file.Block(
					"for (var capturedAt = linkHeads[completedAt]; capturedAt >= 0; " +
					"capturedAt = linkNexts[capturedAt])"))
				{
					file.Line("var candidate = entries[capturedAt];");
					file.Line();

					// A slot belongs to one member, so the state that names it is a jump
					// rather than a run of comparisons. The kind is still tested inside:
					// a `Recovery` entry's own state numbering is not a capture slot's, and
					// nothing says the two cannot land on the same number.
					using (file.Block("switch (candidate.State)"))
						foreach (var memberIndex in scalars)
						{
							var member = members[memberIndex];

							foreach (var slot in member.Slots)
								file.Line($"case {offset + slot}:");

							using (file.Indent())
							{
								if (member.Rule is not null)
								{
									// First in list order, which is what breaking out of a
									// walk of its own used to mean.
									file.Line(
										"if (candidate.Kind == ParserEntry.RuleCapture && " +
										$"candidate.CallIndex == completedAt && captured{memberIndex}At < 0)");
									file.Then($"captured{memberIndex}At = candidate.Position;");
								}
								else
									using (file.Block(
										"if (candidate.Kind == ParserEntry.Capture && " +
										"candidate.CallIndex == completedAt)"))
									{
										file.Line($"if (captured{memberIndex}To < 0)");
										file.Then($"captured{memberIndex}To = candidate.Value;");
										file.Line($"captured{memberIndex}From = candidate.Position;");
									}

								file.Line("break;");
							}
						}
				}

				file.Line();
			}

			for (var memberIndex = 0; memberIndex < members.Count; memberIndex++)
			{
				var member = members[memberIndex];

				if (member.Rule is not null)
				{
					if (member.IsSequence)
					{
						var slots = new List<string>(member.Slots.Count);

						foreach (var slot in member.Slots)
							slots.Add($"candidate.State == {offset + slot}");

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

					if (CSharpEmitter.Asks(factory, "parserInput"))
						arguments.Add("parserInput");

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
