using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;

namespace DotGram.Grammar.Emit;

/// <summary>The ways a parser is written out, and what each can hand a construction.</summary>
/// <remarks>
/// <para>
/// A grammar reaches one of several renderings — the general engine over an arena, an
/// ordinary method with no arena at all, a call compiled where it stood — and which one is
/// a decision made from what the grammar needs. What kept going wrong is the other
/// direction: a name the language supplies to a <c>=&gt;</c> was threaded through the
/// rendering it was built in and silently absent from the rest, so a valid grammar produced
/// C# that does not compile.
/// </para>
/// <para>
/// It happened four times in one week — <c>parserState</c> missing from
/// <see cref="SuppliedNames"/>, the fold materializer assembling its own arguments,
/// a sited callee not refused for wanting a <c>context</c>, and the flat renderings not
/// taking one. Every one was a name that reached one rendering and not another, and every
/// one was fixed where it was found. This is the place that stops the next one being found
/// the same way.
/// </para>
/// <para>
/// **What it is not** is a capability system the renderings consult at run time. It is one
/// table, checked by a test: every name the language supplies is either something a
/// rendering hands over or something that refuses that rendering, and there is no third
/// answer. Adding a name and forgetting a rendering fails that test rather than a
/// consumer's build.
/// </para>
/// </remarks>
static class Renderings
{
	/// <summary>How a parser's states are written out.</summary>
	public enum Rendering
	{
		/// <summary>The arena and the dispatcher — what everything else is a specialization of.</summary>
		Engine,

		/// <summary>An ordinary method: no arena, no dispatch (<c>Machine.Flat.cs</c>).</summary>
		Flat,

		/// <summary>A captured call compiled where it stood (<c>Machine.Sites.cs</c>).</summary>
		Site,
	}

	/// <summary>
	/// Everything a construction may be handed that is not one of its own captures.
	/// </summary>
	/// <remarks>
	/// <c>context</c> is here beside the <c>parser…</c> names although a grammar chooses what
	/// it is called: what matters to a rendering is that something reaches a factory from
	/// outside its captures, and by that measure it is one of these.
	/// </remarks>
	public static readonly IReadOnlyList<string> Supplied =
		[.. SuppliedNames.All, "context"];

	/// <summary>
	/// What each rendering can hand over, and the reason where it cannot.
	/// </summary>
	/// <remarks>
	/// A missing entry is not "no": <see cref="Reason"/> throws for a name nobody has
	/// decided about, which is what makes the test able to tell a decision from an
	/// oversight.
	/// </remarks>
	static readonly Dictionary<(Rendering, string), string?> Answers = new()
	{
		// The engine hands over everything. It is the rendering the others are measured
		// against, and a name it could not supply would be a name the language does not have.
		[(Rendering.Engine, "parserText")]     = null,
		[(Rendering.Engine, "parserSpan")]     = null,
		[(Rendering.Engine, "parserInput")]    = null,
		[(Rendering.Engine, "parserState")]    = null,
		[(Rendering.Engine, "context")]        = null,
		[(Rendering.Engine, "parserPosition")] = null,
		[(Rendering.Engine, "parserOrdinal")]  = null,
		[(Rendering.Engine, "parserLine")]     = null,
		[(Rendering.Engine, "parserColumn")]   = null,
		[(Rendering.Engine, "parserMessage")]  = null,

		// A flat rendering keeps its captures in locals and has no entry saying where the
		// rule began, so what the rule matched and where are the two it cannot answer. The
		// input it refuses for a different reason: a rule that wants the whole of it is one
		// whose value outlives the parse, and that is settled before a rendering is chosen.
		[(Rendering.Flat, "parserText")]  = "a flat rendering keeps no record of where the rule began",
		[(Rendering.Flat, "parserSpan")]  = "a flat rendering keeps no record of where the rule began",
		[(Rendering.Flat, "parserInput")] = "the whole input is refused a flat rendering before this is asked",

		// The marks stand in the arena, which is the thing this rendering exists not to have.
		[(Rendering.Flat, "parserState")] = "a mark is an arena entry and a flat rendering has no arena",

		// And the one that is simply a parameter. It says nothing about how much input is
		// held or how the states are written, so there was never a reason to refuse it —
		// only an omission, which is the defect this file exists because of.
		[(Rendering.Flat, "context")] = null,

		// A site is built from the spans it recorded and nothing else, so anything from
		// outside the captures keeps the call's own boundary.
		[(Rendering.Site, "parserText")]  = "a site's call is built from the spans it recorded",
		[(Rendering.Site, "parserSpan")]  = "a site's call is built from the spans it recorded",
		[(Rendering.Site, "parserInput")] = "a site's call is built from the spans it recorded",
		[(Rendering.Site, "parserState")] = "a site's call is built from the spans it recorded",
		[(Rendering.Site, "context")]     = "a site's call is built from the spans it recorded",

		// The five a `recover` factory is handed. Recovery keeps the engine outright —
		// `CSharpEmitter` asks `RecoversWithin` before a rendering is chosen at all — so
		// these never reach a decision here, and saying so is what stops them being read as
		// undecided.
		[(Rendering.Flat, "parserPosition")] = "recovery keeps the engine, so a recover factory is never rendered flat",
		[(Rendering.Flat, "parserOrdinal")]  = "recovery keeps the engine, so a recover factory is never rendered flat",
		[(Rendering.Flat, "parserLine")]     = "recovery keeps the engine, so a recover factory is never rendered flat",
		[(Rendering.Flat, "parserColumn")]   = "recovery keeps the engine, so a recover factory is never rendered flat",
		[(Rendering.Flat, "parserMessage")]  = "recovery keeps the engine, so a recover factory is never rendered flat",
		[(Rendering.Site, "parserPosition")] = "recovery keeps the engine, so a recover factory is never sited",
		[(Rendering.Site, "parserOrdinal")]  = "recovery keeps the engine, so a recover factory is never sited",
		[(Rendering.Site, "parserLine")]     = "recovery keeps the engine, so a recover factory is never sited",
		[(Rendering.Site, "parserColumn")]   = "recovery keeps the engine, so a recover factory is never sited",
		[(Rendering.Site, "parserMessage")]  = "recovery keeps the engine, so a recover factory is never sited",
	};

	/// <summary>Why this rendering cannot hand that over, or null where it can.</summary>
	/// <exception cref="InvalidOperationException">Nobody has decided.</exception>
	public static string? Reason(Rendering rendering, string name) =>
		Answers.TryGetValue((rendering, name), out var reason)
			? reason
			: throw new InvalidOperationException(
				$"No decision about whether the {rendering} rendering can supply '{name}'. " +
				"Every supplied name is either handed over or refuses the rendering; there " +
				"is no third answer (Renderings.cs).");

	/// <summary>Whether this rendering can hand a factory everything it asks for.</summary>
	public static bool Supplies(Rendering rendering, Machine.Factory factory, RecognitionGraph graph) =>
		!Wants(factory, graph).Any(name => Reason(rendering, name) is not null);

	/// <summary>Which supplied names a factory names, in the order they are declared.</summary>
	/// <remarks>
	/// Asked of the same <see cref="CSharpEmitter.Asks"/> the parameter list is built from,
	/// which is the whole point: what a rendering has to answer for is exactly what the
	/// signature will have in it.
	/// </remarks>
	public static IEnumerable<string> Wants(Machine.Factory factory, RecognitionGraph graph)
	{
		if (CSharpEmitter.WantsText(factory))
			yield return "parserText";

		foreach (var name in Supplied)
		{
			if (name == "parserText")
				continue;

			// A grammar that declares no `context` and no `state` cannot have a factory
			// wanting one, and the emitter's own gates say so the same way.
			if (name == "context" && graph.Context is null)
				continue;

			if (name == "parserState" && graph.State is null)
				continue;

			if (CSharpEmitter.Asks(factory, name))
				yield return name;
		}
	}
}
