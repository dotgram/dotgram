using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;

namespace DotGram.Grammar.Emit;

/// <summary>
/// The reader: a grammar as the methods a person would have written.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why a second rendering by methods.</b> The first one (<c>Machine.Direct.cs</c>) was
/// grown out of the automaton and kept its vocabulary: a rule is one method, but inside it
/// every construct is a labelled region and every failure is a jump. That shape is right
/// for one machine of a thousand states, which is a graph and nothing else; for a method it
/// is a graph nobody asked for. Four passes exist to take dead jumps, dead labels, dead
/// marks and unused locals back out of it, a fifth was written and was wrong, and the
/// reason it was wrong is that a question about one construct had to be asked of the whole
/// method.
/// </para>
/// <para>
/// Here a construct is a statement. A sequence is statements one after another; a failure
/// is <c>return -1</c>; a repetition is a <c>while</c>; a choice is a switch on what the
/// alternatives begin with, or one attempt after another where the first token does not
/// divide them. An alternative that can fail halfway and has a sibling after it becomes a
/// method of its own, because that is how a caller learns it failed without a jump — and
/// where the choice dispatches, no alternative needs one, since failing the alternative the
/// token chose is failing the choice.
/// </para>
/// <para>
/// <b>What it does not do yet</b>, and hands back to the rendering it is replacing: values,
/// guards, marks, folds, climbing, and the tape of ways back that reading characters needs
/// (§4 — over kinds a rule's answer stands, and there is no tape at all). Each of those
/// arrives with its own entry in <c>docs/next.md</c>. <see cref="CanRead"/> is the gate and
/// it refuses rather than guesses.
/// </para>
/// </remarks>
sealed partial class Machine
{
	/// <summary>The refusal recorder the emitted readers call.</summary>
	const string Refusing = "Refuse_DotGram";

	/// <summary>Whether every publication in a group can be written as a reader.</summary>
	public bool CanRead(IReadOnlyList<Publication> publications)
	{
		if (publications.Count == 0 || !CanDirect(publications))
			return false;

		foreach (var rule in DirectRules(publications))
		{
			// A rule that keeps a value records it, and recording is the next thing this
			// rendering learns rather than the first.
			if (Valued(rule) || _graph.Results[rule].Count > 0 ||
				_graph.Folds.ContainsKey(rule) || _graph.Climbing.ContainsKey(rule) ||
				_graph.Externals.ContainsKey(rule))
			{
				return false;
			}

			foreach (var node in NodeWalk.Descendants(_graph.Bodies[rule]))
				switch (node)
				{
					case Node.Empty or Node.Literal or Node.Element or Node.Sequence
						or Node.Choice or Node.Repeat or Node.Glue or Node.Behind:
						break;

					case Node.Lookahead(_, var inside) when !NodeWalk.Descendants(inside)
						.Any(static one => one is Node.Capture or Node.Construct or Node.Guard):
						break;

					case Node.External { HasValue: false }:
						break;

					case Node.Call(_, { Count: 0 }):
						break;

					default:
						return false;
				}
		}

		// The way back is a loop over a tape, and the loop is written but the tape is not.
		return OverKinds && !publications.Any(publication => publication.Rule.GivesBack);
	}

	/// <summary>Every rule of a reading, each as a method, with the entries above them.</summary>
	public string RenderReader(IReadOnlyList<Publication> publications)
	{
		var file  = new Writer(0);
		var rules = DirectRules(publications);
		var seen  = new HashSet<RuleSymbol>();

		BackEdges(publications);

		foreach (var publication in publications)
		{
			if (seen.Add(publication.Rule))
				RenderReaderEntry(file, publication.Rule);
		}

		foreach (var rule in rules)
		{
			_seam = FollowSets.SeamOf(rule, _graph);

			var reader = new ReaderWriter(this, rule);
			var body   = reader.Render(_graph.Bodies[rule]);

			file.Line($"/// <summary><c>{rule.Name}</c>, read by a method of its own.</summary>");

			using (file.Block(
				$"static int {ReaderOf(rule)}(" +
				$"global::System.ReadOnlySpan<char> text, int pos, " +
				$"ref {CSharpEmitter.FailureType} failure, {WaysType} ways{DirectReaderParameters})"))
			{
				file.Write(body);
			}

			file.Line();

			foreach (var (name, part) in reader.Parts)
			{
				file.Line($"/// <summary>One alternative of <c>{rule.Name}</c>, read where it stood.</summary>");

				using (file.Block(
					$"static int {name}(" +
					$"global::System.ReadOnlySpan<char> text, int pos, " +
					$"ref {CSharpEmitter.FailureType} failure, {WaysType} ways{DirectReaderParameters})"))
				{
					file.Write(part);
				}

				file.Line();
			}
		}

		return file.ToString();
	}

	/// <summary>The whole input as one rule, which is what a publication asks for.</summary>
	void RenderReaderEntry(Writer file, RuleSymbol rule)
	{
		_seam = FollowSets.SeamOf(rule, _graph);

		var core = CSharpEmitter.MethodOf(rule) + "_Whole";

		file.Line($"/// <summary>The whole input as <c>{rule.Name}</c>, read by methods.</summary>");

		using (file.Block(
			$"static int {core}(" +
			$"global::System.ReadOnlySpan<char> text, int pos, " +
			$"ref {CSharpEmitter.FailureType} failure{InputParameter}{TokensParameter}{ContextParameter})"))
		{
			var reader = new ReaderWriter(this, rule);
			var body   = _graph.Trivia.TryGetValue(rule, out var seam)
				? new Node.Sequence([seam, new Node.Call(rule, []), seam])
				: (Node)new Node.Call(rule, []);

			// The tape is what a refusal inside a lookahead is kept quiet by, and it is all
			// the readers use it for while they keep no value.
			file.Line($"var ways = {WaysType}.Rent();");
			file.Line();

			using (file.Block("try"))
				file.Write(reader.Render(body, whole: true));

			file.Line("finally");

			using (file.Block(""))
				file.Line($"{WaysType}.Return(ways);");
		}

		file.Line();
	}

	/// <summary>
	/// One rule's body as statements.
	/// </summary>
	/// <remarks>
	/// The position is a local, and every construct that reads moves it. Nothing else is
	/// carried: there is no tape while the readers commit, so a construct that fails has
	/// nothing to put back but the position, and the position is the caller's own copy.
	/// </remarks>
	sealed class ReaderWriter(Machine machine, RuleSymbol owner)
	{
		readonly RecognitionGraph _graph = machine._graph;

		/// <summary>The alternatives written as methods of their own, and their bodies.</summary>
		public List<(string Name, string Body)> Parts { get; } = [];

		int _parts;
		bool _character;

		public string Render(Node body, bool whole = false)
		{
			var code = new Writer(0);

			Emit(code, body);

			if (whole)
			{
				using (code.Block("if (p != text.Length)"))
				{
					code.Line($"{Refusing}(ref failure, p, null, ways);");
					code.Line("return -1;");
				}
			}

			code.Line("return p;");

			var head = new Writer(0);

			head.Line("var p = pos;");

			if (_character)
				head.Line("var c = '\\0';");

			head.Write(code.ToString());

			return head.ToString();
		}

		void Emit(Writer code, Node node)
		{
			switch (node)
			{
				case Node.Empty or Node.Glue:
					break;

				case Node.Literal(var text) { IgnoreCase: var folded }:
					EmitLiteral(code, node, text, folded);
					break;

				case Node.Element element:
					EmitElement(code, element);
					break;

				case Node.Sequence(var parts):
					foreach (var part in parts)
						Emit(code, part);

					break;

				case Node.Choice(var alternatives):
					EmitChoice(code, alternatives);
					break;

				case Node.Repeat repeat:
					EmitRepeat(code, repeat);
					break;

				case Node.Call(var called, _):
					EmitCall(code, called);
					break;

				case Node.Lookahead(var positive, var inside):
					EmitLookahead(code, positive, inside);
					break;

				default:
					throw new InvalidOperationException(
						$"{node.GetType().Name} passed CanRead and the reader has no statement for it.");
			}
		}

		void EmitLiteral(Writer code, Node node, string text, bool folded)
		{
			if (text.Length == 0)
				return;

			var name = machine.DeclareExpected([node.ToString()]);

			if (text.Length == 1)
			{
				var read = folded ? "global::System.Char.ToUpperInvariant(text[p])" : "text[p]";
				var want = CSharpEmitter.Char(folded ? char.ToUpperInvariant(text[0]) : text[0]);

				using (code.Block($"if ((uint)p >= (uint)text.Length || {read} != {want})"))
					Refused(code, name);

				code.Line($"p += {text.Length};");

				return;
			}

			var comparison = folded
				? $"!global::System.MemoryExtensions.Equals(text.Slice(p, {text.Length}), " +
					$"{Spanned(text)}, global::System.StringComparison.OrdinalIgnoreCase)"
				: $"!global::System.MemoryExtensions.SequenceEqual(text.Slice(p, {text.Length}), {Spanned(text)})";

			using (code.Block($"if ((uint)(p + {text.Length}) > (uint)text.Length || {comparison})"))
				Refused(code, name);

			code.Line($"p += {text.Length};");
		}

		void EmitElement(Writer code, Node.Element element)
		{
			var name  = machine.DeclareExpected([element.ToString()]);
			var first = FirstSets.Of(element, _graph);

			_character = true;

			using (code.Block("if ((uint)p >= (uint)text.Length)"))
				Refused(code, name);

			code.Line("c = text[p];");

			using (code.Block($"if (!({machine.RangesTest(first.Ranges, machine.Tabulate)}))"))
				Refused(code, name);

			code.Line("p++;");
		}

		void EmitCall(Writer code, RuleSymbol called)
		{
			var result = $"q{_calls++}";

			if (machine._backEdges.Contains((owner, called)))
				code.Line("global::System.Runtime.CompilerServices.RuntimeHelpers.EnsureSufficientExecutionStack();");

			code.Line(
				$"var {result} = {machine.ReaderOf(called)}(text, p, ref failure, ways{machine.DirectReaderArguments});");
			code.Line($"if ({result} < 0) return -1;");
			code.Line($"p = {result};");
		}

		int _calls;

		/// <summary>
		/// Alternatives in order: the one the token chooses where a token chooses, and one
		/// attempt after another where none does.
		/// </summary>
		/// <remarks>
		/// Where the choice dispatches, an alternative that fails is a choice that fails —
		/// no other could have matched here — so each is written where it stands and ends
		/// the reader. Where it does not, every alternative but the last becomes a method,
		/// because <c>-1</c> is how one tells its caller to try the next.
		/// </remarks>
		void EmitChoice(Writer code, IReadOnlyList<Node> alternatives)
		{
			if (alternatives.Count == 1)
			{
				Emit(code, alternatives[0]);

				return;
			}

			if (machine.Dispatchable(alternatives) is { } groups && groups.All(one => one.Members.Count == 1))
			{
				var name = machine.DeclareExpected([machine.PredictedDisplay(alternatives)]);

				_character = true;

				using (code.Block("if ((uint)p >= (uint)text.Length)"))
					Refused(code, name);

				code.Line("c = text[p];");

				using (code.Block("switch (c)"))
				{
					foreach (var group in groups)
					{
						var labels = "";

						foreach (var range in group.Set.Ranges)
							for (var one = range.From; ; one++)
							{
								labels += $"case {CSharpEmitter.Char(one)}: ";

								if (one == range.To)
									break;
							}

						code.Line(labels);

						using (code.Indent())
						using (code.Block(""))
						{
							Emit(code, group.Members[0]);
							code.Line("break;");
						}
					}

					code.Line("default:");

					using (code.Indent())
						Refused(code, name);
				}

				return;
			}

			// One attempt after another. Each but the last is a method, so that its failure
			// is a number rather than a jump out of the middle of this one.
			var tried = $"q{_calls++}";

			code.Line($"var {tried} = -1;");

			for (var i = 0; i < alternatives.Count - 1; i++)
			{
				var part = Extracted(alternatives[i]);

				using (code.Block($"if ({tried} < 0)"))
					code.Line(
						$"{tried} = {part}(text, p, ref failure, ways{machine.DirectReaderArguments});");
			}

			using (code.Block($"if ({tried} < 0)"))
			{
				Emit(code, alternatives[alternatives.Count - 1]);
				code.Line($"{tried} = p;");
			}

			code.Line($"p = {tried};");
		}

		/// <summary>One alternative as a method: the position it reached, or -1.</summary>
		string Extracted(Node alternative)
		{
			var name  = machine.ReaderOf(owner) + "_Part" + _parts++;
			var apart = new ReaderWriter(machine, owner);

			Parts.Add((name, apart.Render(alternative)));

			foreach (var made in apart.Parts)
				Parts.Add(made);

			return name;
		}

		void EmitRepeat(Writer code, Node.Repeat repeat)
		{
			var (body, min, max) = repeat;
			var turns = min > 0 || max is not null ? $"t{_calls++}" : null;

			if (turns is not null)
				code.Line($"var {turns} = 0;");

			using (code.Block("while (true)"))
			{
				if (max is { } limit)
				{
					code.Line($"if ({turns} >= {limit})");
					code.Then("break;");
					code.Line();
				}

				var turn = $"q{_calls++}";

				code.Line($"var {turn} = {Extracted(body)}(text, p, ref failure, ways{machine.DirectReaderArguments});");
				code.Line();
				code.Line($"if ({turn} < 0 || {turn} == p)");
				code.Then("break;");
				code.Line();
				code.Line($"p = {turn};");

				if (turns is not null)
					code.Line($"{turns}++;");
			}

			if (min > 0)
			{
				code.Line();
				code.Line($"if ({turns} < {min})");
				code.Then("return -1;");
			}
		}

		void EmitLookahead(Writer code, bool positive, Node inside)
		{
			var seen = $"q{_calls++}";

			code.Line($"var {seen} = {Extracted(inside)}(text, p, ref failure, ways{machine.DirectReaderArguments});");
			code.Line();
			code.Line($"if ({seen} {(positive ? "<" : ">=")} 0)");
			code.Then("return -1;");
		}

		void Refused(Writer code, string expected)
		{
			code.Line($"{Refusing}(ref failure, p, {expected}, ways);");
			code.Line("return -1;");
		}
	}
}
