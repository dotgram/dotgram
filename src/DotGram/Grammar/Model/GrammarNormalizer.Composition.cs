using System;
using System.Collections.Generic;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Parsing;

namespace DotGram.Grammar.Model;

public sealed partial class GrammarNormalizer
{
	/// <summary>
	/// Which type the object handed to a parse actually is, given that several grammars in
	/// it may each have declared what they need of it.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A `context : @T` is a contract, not a variable (docs/next.md, "Decided: `context` is
	/// a contract"). The rules a grammar wrote are bound to the type that grammar declared,
	/// and a grammar including it may need more of the same object — so one object flows
	/// down and the parts of a composed grammar legitimately see it through different static
	/// types. That is ordinary subtyping, and the one condition it puts on the whole is
	/// stated here: the type actually handed over has to satisfy every contract along the
	/// way.
	/// </para>
	/// <para>
	/// The effective type is the root's declaration where there is one — the grammar being
	/// compiled is the one whose caller supplies the object, so it has the last word. Where
	/// it declares none, the effective type is the first inherited contract that satisfies
	/// all the others, which with a real host is the most derived of them and with none is
	/// simply the nearest base. Where no contract satisfies the rest there is no single type
	/// to hand over and the grammar has to say which it means.
	/// </para>
	/// </remarks>
	void ReconcileContexts()
	{
		var contracts = new List<(GrammarNamespace Namespace, TypeRef Declared)>();

		Collect(_model.Root);

		if (contracts.Count == 0)
			return;

		_context = _model.Context ?? Widest();

		if (_context is null)
			return;

		foreach (var (_, declared) in contracts)
			if (!Satisfies(_context, declared))
				Report(
					GrammarBinder.ContextNotRefined,
					$"The context handed to this parse is '{_context.Name}', and the rules included " +
					$"from elsewhere were written against '{declared.Name}', which it is not. A " +
					$"grammar including another may strengthen the contract; it cannot replace it.",
					_context.At);

		void Collect(GrammarNamespace ns)
		{
			if (ns.Context is { } declared)
				contracts.Add((ns, declared));

			foreach (var nested in ns.Nested)
				Collect(nested);
		}

		// The first that satisfies every other, rather than the unique one: with a host that
		// answers nothing — the grammar half run without a compilation — every candidate
		// satisfies every other, and a tie there has to settle somewhere rather than become
		// a diagnostic about a question that was never really asked. Namespace order puts
		// the nearest base first, which is the answer a reader would give.
		TypeRef? Widest()
		{
			foreach (var (_, candidate) in contracts)
			{
				var satisfies = true;

				foreach (var (_, other) in contracts)
					if (!Satisfies(candidate, other))
					{
						satisfies = false;
						break;
					}

				if (satisfies)
					return candidate;
			}

			Report(
				GrammarBinder.ContextNotRefined,
				"The grammars included here declare contexts that no one type satisfies, and this " +
				"grammar declares none of its own to reconcile them. Declare the 'context' this " +
				"parse is handed.",
				contracts[0].Declared.At);

			return null;
		}
	}

	/// <summary>
	/// Whether an object of one contract may be seen through another.
	/// </summary>
	/// <remarks>
	/// Through the seam, and the same one §4.1 case 2 uses: assignability is inheritance,
	/// interfaces and conversions, and a grammar can see none of them. Identical names are
	/// answered here rather than asked, so a grammar that declares the same contract as the
	/// one it includes needs no host at all.
	/// </remarks>
	bool Satisfies(TypeRef from, TypeRef to) =>
		from.Name == to.Name || _resolver.IsAssignable(from.Name, to.Name);

	/// <summary>
	/// The one type every mark in a parse is written in, where more than one grammar in the
	/// composition has an opinion about it.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A `state : @T` does not compose the way a `context : @T` does, and the reason is not
	/// the variance that makes it mechanically impossible — <c>ReadOnlySpan&lt;T&gt;</c> being
	/// invariant is only how the impossibility shows up. Underneath it: a context is one
	/// object flowing down, and a mark is a heterogeneous stack of values placed by several
	/// authors and read by whichever hook recognizes its own. Different shapes compose
	/// differently, and the shape of a stack read as one span admits exactly one element
	/// type.
	/// </para>
	/// <para>
	/// So a grammar included in another may declare the same state and no other. Which is
	/// why §7.8 says a grammar meant to be inherited declares its state as a reference type:
	/// a consumer cannot extend an enum to add a concern of their own.
	/// </para>
	/// </remarks>
	void ReconcileState()
	{
		var declared = new List<TypeRef>();

		Collect(_model.Root);

		if (declared.Count == 0)
			return;

		_state = _model.State ?? declared[0];

		foreach (var one in declared)
			if (one.Name != _state.Name)
				Report(
					GrammarBinder.StateNotInvariant,
					$"Every mark a parse places is written in one type, and this parse writes " +
					$"'{_state.Name}'. A grammar included here declares '{one.Name}', which would be " +
					$"a second type for part of one answer. Two concerns are told apart by their " +
					$"values, read by the hook that cares.",
					one.At);

		void Collect(GrammarNamespace ns)
		{
			if (ns.State is { } state)
				declared.Add(state);

			foreach (var nested in ns.Nested)
				Collect(nested);
		}
	}

	TypeRef? _context;
	TypeRef? _state;
}
