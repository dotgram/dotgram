using System;
using System.Collections.Generic;

namespace DotGram.Grammar.Emit;

/// <summary>
/// How a reader carries what it read until the derivation is accepted and the author's
/// constructions can run.
/// </summary>
/// <remarks>
/// <para>
/// The reader recognizes; something else holds the pieces of the value it is not yet
/// allowed to build. Today that something is the tape — a log of records and a stack of
/// gathered references, both on <c>Ways</c> — and until this seam was cut the reader wrote
/// the tape's own calls at some fifty sites. Behind this type it writes the same fifty
/// sites, and what they turn into is the carrier's business.
/// </para>
/// <para>
/// Every method returns the C# to emit, or nothing where a carrier has nothing to do at
/// that site: a carrier that keeps values in locals has no store to mark and nothing to put
/// back when an alternative fails. The names are for what a site <em>means</em> — a record
/// begun, a member put in it, the store put back to a mark — and not for how the tape does
/// it, so that the next carrier can answer the same questions differently
/// (<c>docs/next.md</c>, the redesign).
/// </para>
/// <para>
/// This is the first stage of that seam, cut with one carrier behind it and the generated
/// code unchanged to the byte. It is therefore exactly as fine-grained as the reader's
/// existing emissions and no finer: two marks rather than one, because the tape has two
/// stores and the reader marks them at different places under different conditions.
/// </para>
/// </remarks>
abstract class Carrier
{
	// ---- marks and unwinding ---------------------------------------------------------------

	/// <summary>A local remembering where the records stood, to put them back to.</summary>
	public abstract string MarkRecords(string name);

	/// <summary>A local remembering where the gathered references stood.</summary>
	public abstract string MarkGathered(string name);

	/// <summary>The records put back to a mark — and with them whatever a guard built above it.</summary>
	public abstract IEnumerable<string> UnwindRecords(string name);

	/// <summary>The gathered references put back to a mark.</summary>
	public abstract string UnwindGathered(string name);

	// ---- a record -------------------------------------------------------------------------

	/// <summary>A record of one alternative of a rule begun, with the span it stands on where those are kept.</summary>
	public abstract string Begin(int arm, string? start, string? end);

	/// <summary>The value so far, as a fold step's first member (§4.3).</summary>
	public abstract string PutAccumulator();

	/// <summary>A member that is a span of text.</summary>
	public abstract string PutText(string from, string to);

	/// <summary>A member that is another record.</summary>
	public abstract string PutRecord(string record);

	/// <summary>A member gathered across a repetition: everything pushed since the mark, of the given slots.</summary>
	public abstract string Collect(string from, long slots, bool pairs);

	/// <summary>The record closed, with the gathered references above the mark consumed.</summary>
	public abstract string End(string gatheredFrom);

	/// <summary>An expression for the record most recently closed.</summary>
	public abstract string Last { get; }

	// ---- gathering ------------------------------------------------------------------------

	/// <summary>One piece of text pushed for a member gathered across turns.</summary>
	public abstract string PushText(int slot, string from, string to);

	/// <summary>One record pushed for a member gathered across turns.</summary>
	public abstract string PushRecord(int slot);

	/// <summary>A §7.8 mark, opened or closed, at the position.</summary>
	public abstract string Mark(int kind, int site);

	// ---- building -------------------------------------------------------------------------

	/// <summary>A record built into a value where the reader is, for a guard that asks (§3.6).</summary>
	public abstract string Materialize(string record, string sinceMark);

	/// <summary>The value a built record holds, as a guard sees it.</summary>
	public abstract string ValueOf(string type, string record);

	/// <summary>
	/// The gathered references of the given slots, counted and then visited: what a guard
	/// that names a sequence member is handed.
	/// </summary>
	public abstract void Gathered(Writer code, string from, long slots, string handed, string type, string build);

	/// <summary>What an entry rents before reading, beside the ways.</summary>
	public abstract IEnumerable<string> Rent();

	/// <summary>And returns after.</summary>
	public abstract IEnumerable<string> Return();

	/// <summary>The whole derivation built into the entry's value.</summary>
	public abstract IEnumerable<string> BuildRoot(string type, bool extent);

	/// <summary>The code that builds records into values, once per file.</summary>
	public abstract string RenderBuilder(IReadOnlyList<Binding.RuleSymbol> rules);
}
