using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DotGram.Grammar.Emit;

/// <summary>
/// Where a state can go on to, recorded where the text that means it is written.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="PlanLayout"/> needs the same thing and recovers it by reading the finished
/// text back with two regular expressions. That makes every jump's spelling load-bearing,
/// and the two halves fail differently when a spelling drifts. A missed <c>goto</c> leaves
/// its target judged unreachable and so unwritten, and the jump then names a label that is
/// not there — which the C# compiler says out loud. A missed resume leaves the state out of
/// the dispatch instead: the block is written, the code compiles, and a parse that should
/// have resumed there falls to the default and refuses input it ought to accept.
/// </para>
/// <para>
/// So neither is read back. The call that writes a state's name records the edge and mints
/// a mark holding the number, and <see cref="Settle"/> puts the final name in once layout
/// knows it. One call, one argument, two uses of it — there is no second account to keep in
/// step and nothing to check the first against.
/// </para>
/// </remarks>
sealed partial class Machine
{
	// ── The graph (§where a state goes, said once) ─────────────────────────────────

	/// <summary>What a state's own text says about where the parse can go on from it.</summary>
	sealed class Edges
	{
		/// <summary>States a <c>goto</c> written in this one names.</summary>
		public readonly List<int> Jumps = [];

		/// <summary>States an arena entry pushed in this one names, to resume at.</summary>
		public readonly List<int> Resumes = [];
	}

	/// <summary>
	/// The edges of each state, by the writer that is its body.
	/// </summary>
	/// <remarks>
	/// A state's body is one writer and is never composed from others, so the writer is the
	/// state as far as recording goes. Anything else written to — the file, a header — is not
	/// in the table and is absent here, which is what makes a lookup that finds nothing the
	/// right answer rather than a missed edge.
	/// </remarks>
	readonly Dictionary<Writer, Edges> _edges = [];

	/// <summary>
	/// The label of a state, written as a mark rather than as the name it will have.
	/// </summary>
	/// <remarks>
	/// A state's number is not final where it is written: layout collapses the ones that do
	/// nothing but point somewhere, and everything naming a collapsed state has to name what
	/// it collapsed into. So nothing writes the number — it writes a mark holding it, and
	/// <see cref="Settle"/> puts the final name in when there is one. What the mark buys
	/// beyond that is that it cannot be spelled by accident or missed by a pattern: only
	/// this and <see cref="Resuming"/> make one, so the marks in a body are exactly the
	/// states it names, and there is no second account of that to keep in step.
	/// </remarks>
	string Label(Writer at, int state)
	{
		if (_edges.TryGetValue(at, out var edges))
			edges.Jumps.Add(state);

		return Mark(Jumps, state);
	}

	/// <summary>
	/// The state an arena entry resumes at, written as a mark for the same reason.
	/// </summary>
	string Resuming(Writer at, int state)
	{
		if (_edges.TryGetValue(at, out var edges))
			edges.Resumes.Add(state);

		return Mark(Lands, state);
	}

	/// <summary>
	/// A state named in a body: the fence, what naming it means, the number, the fence.
	/// </summary>
	/// <remarks>
	/// U+0001 because generated C# cannot contain one — every character a grammar can put
	/// in a literal goes through <c>CSharpEmitter.Char</c> or <c>EscapeExpected</c>, and
	/// both write a control character as an escape. <see cref="Settle"/> checks that none
	/// survives into the file, which is a stronger guard than looking for what should have
	/// been rewritten: a mark left behind is a compile error in the consumer's build rather
	/// than a state quietly named wrong.
	/// </remarks>
	static string Mark(char kind, int state) => $"{Fence}{kind}{state.ToString(CultureInfo.InvariantCulture)}{Fence}";

	const char Fence  = '\u0001';
	const char Jumps  = 'J';
	const char Lands = 'R';

	/// <summary>
	/// The same text with every mark replaced by what it means, once layout has settled
	/// which state is which.
	/// </summary>
	string Settle(string body)
	{
		if (body.IndexOf(Fence) < 0)
			return body;

		var text = new StringBuilder(body.Length);
		var at   = 0;

		while (at < body.Length)
		{
			var opened = body.IndexOf(Fence, at);

			if (opened < 0)
			{
				text.Append(body, at, body.Length - at);

				break;
			}

			text.Append(body, at, opened - at);

			var closed = body.IndexOf(Fence, opened + 1);

			if (closed < 0)
				throw new InvalidOperationException(
					"A state mark was written without its end: [" +
					body.Replace(Fence.ToString(), "|") + "] (Machine.Graph.cs)");

			var kind   = body[opened + 1];
			var state  = int.Parse(body.Substring(opened + 2, closed - opened - 2), CultureInfo.InvariantCulture);
			var landed = Resolved(state);

			text.Append(kind == Jumps ? Label(landed) : landed.ToString(CultureInfo.InvariantCulture));

			at = closed + 1;
		}

		return text.ToString();
	}

	/// <summary>What the state at an index recorded, which is nothing where it wrote nothing.</summary>
	Edges Recorded(int index) =>
		index >= 0 && index < _states.Count && _edges.TryGetValue(_states[index], out var edges)
			? edges
			: None;

	static readonly Edges None = new();

}
