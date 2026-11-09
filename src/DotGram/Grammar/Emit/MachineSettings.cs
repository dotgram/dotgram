using System.Collections.Generic;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;

namespace DotGram.Grammar.Emit;

/// <summary>
/// What one <see cref="Machine"/> is built for: the compiler's options as this machine takes them,
/// and what the emitter decided about it — the rules it reads, the input it reads them over, the
/// tables it shares with its siblings.
/// </summary>
/// <remarks>
/// <para>
/// A record rather than the options themselves, because a file's machines do not all take the
/// options alike: a buffered machine reads a buffer whatever the file was asked for, and the
/// machine that values a token again reads characters with none of the file's carrier or
/// tables. The emitter makes one for the file and each machine is that one <c>with</c> what is
/// its own, so what every machine shares is said once.
/// </para>
/// <para>
/// The machine reads these where it needs them and keeps no copy: it does its work in its
/// constructor, so all of it has to be known there — a property set after construction would
/// be set after the states were written.
/// </para>
/// </remarks>
sealed record MachineSettings
{
	/// <summary>Where a position of the grammar's text stands in its file, for <c>#line</c>; null writes none.</summary>
	public ILineMap? Lines { get; init; }

	/// <summary>
	/// Whether input the machine reads may run out under it — what a streaming reading has to be
	/// ready for (<see cref="CSharpEmitter.Streaming"/>).
	/// </summary>
	public bool Starves { get; init; }

	/// <summary>
	/// The rules this machine compiles, or null for all of them. A machine is built for one
	/// published rule and what that rule reaches, so a grammar with several publications has
	/// several machines and none of them carries the others' states.
	/// </summary>
	public IReadOnlyCollection<RuleSymbol>? Only { get; init; }

	/// <summary>
	/// What distinguishes this machine's method and array names from another's in the same
	/// file. Empty where there is only one, so a grammar with a single publication is
	/// compiled to exactly the names it always was.
	/// </summary>
	public MachineTag Tag { get; init; } = MachineTag.None;

	/// <summary>
	/// How large a divided recognizer's parts should be aimed to be, or null for the measured
	/// default (<see cref="GramCompilerOptions.PartSize"/>).
	/// </summary>
	public int? PartSize { get; init; }

	/// <summary>Whether the machine reads token kinds rather than characters (<see cref="Machine.OverKinds"/>).</summary>
	public bool OverKinds { get; init; }

	/// <summary>
	/// The terminals whose value is not built here but read again from their own text.
	/// </summary>
	/// <remarks>
	/// One token stands where a whole rule used to, so the captures its <c>=&gt;</c> was
	/// written against are inside it and out of reach. What this machine records is the
	/// extent; a second machine over characters turns that extent into the value
	/// (`LexicalSplit.Valued`).
	/// </remarks>
	public IReadOnlyCollection<RuleSymbol>? Reread { get; init; }

	/// <summary>
	/// Which carrier the machine was asked for — the immediate one where the host left it to the
	/// generator — and what it gets is <see cref="Machine.Carrier"/>.
	/// </summary>
	public CarrierKind Carrier { get; init; } = CarrierKind.Tape;

	/// <summary>
	/// How many stacks a parse may take past the one it began on, or nought for as many as there
	/// is memory for (<see cref="GramCompilerOptions.Stacks"/>).
	/// </summary>
	public int Stacks { get; init; }

	/// <summary>
	/// What each kind stands for, where this machine reads kinds — the one thing that can say
	/// a refusal in the words the grammar wrote rather than in the numbers the lexer made up.
	/// </summary>
	public TerminalInventory? Inventory { get; init; }

	/// <summary>
	/// Which rules of the graph are read where the reading may not stand, asked once for the
	/// file and handed to every machine the immediate carrier was asked for; null on the tape.
	/// </summary>
	public Replay.Report? Replay { get; init; }

	/// <summary>Whether the machine reads a buffer filled from a reader or a stream rather than a whole string.</summary>
	public bool BufferedInput { get; init; }

	/// <summary>Whether what the buffer holds is bytes rather than characters.</summary>
	public bool BufferedBytes { get; init; }

	/// <summary>Whether text captures are spans of the input (<see cref="GramCompilerOptions.SpanCaptures"/>).</summary>
	public bool SpanCaptures { get; init; }

	/// <summary>Whether a buffered machine is read by a <c>find</c> as well as a <c>parse</c>.</summary>
	public bool BufferedFind { get; init; }

	/// <summary>
	/// Whether the machine reads its publications only from a position, a flat machine beside it
	/// reading them whole (CSharpEmitter.AddPositionalMachines): it has no entry that demands the end.
	/// </summary>
	public bool PositionalOnly { get; init; }

	/// <summary>Whether a choice may be decided by a table of prefixes (<see cref="GramCompilerOptions.PrefixTables"/>).</summary>
	public bool PrefixTables { get; init; }

	/// <summary>The diagnostic lists shared by sibling machines of one parser class; null where there is one machine.</summary>
	public Dictionary<string, int>? ExpectedTables { get; init; }

	/// <summary>
	/// Whether the rules are compiled later, by <see cref="Machine.CompileRules"/>, rather than in
	/// the constructor: where the emitter has still to settle which machines it keeps.
	/// </summary>
	public bool DeferCompilation { get; init; }

	/// <summary>Whether some reading of this machine records no failure (<see cref="Machine.Quiets"/>).</summary>
	public bool Quiets { get; init; }

	/// <summary>What the file's trace build numbers its events by, where it is one; null writes none of it.</summary>
	public TraceTables? Tracing { get; init; }

	/// <summary>The sets of the parser class the machine writes into; null gives the machine sets of its own.</summary>
	public ExpectedSets? ExpectedSets { get; init; }

	/// <summary>
	/// Whether the quiet reading writes down how far it got (<see cref="Machine.KeepsDoors"/>);
	/// null for wherever the machine quiets outside a trace build. Told outright by a machine
	/// written into a trace build without tracing itself (the one that reads a terminal again).
	/// </summary>
	public bool? KeepsDoors { get; init; }

	/// <summary>Whether the machine tells where a position stands through a window (<see cref="Machine.Places"/>).</summary>
	public bool Places { get; init; }

	/// <summary>The engine's recoveries that tell places through a window (<see cref="Machine.Places"/>).</summary>
	public IReadOnlyList<Recovery> PlacedRecoveries { get; init; } = [];

	/// <summary>Whether the value of a terminal read again over its text asks whether the reading locates.</summary>
	public bool RereadLocates { get; init; }

	/// <summary>
	/// Whether this machine reads bytes the caller already holds, whole, and nothing that streams
	/// (<see cref="Machine.InPlace"/>).
	/// </summary>
	public bool InPlace { get; init; }

	/// <summary>Whether a build asked for the carriers report.</summary>
	public bool Reporting { get; init; }

	/// <summary>Whether the machine counts its rules' entries (<see cref="GramCompilerOptions.CountRules"/>).</summary>
	public bool CountsRules { get; init; }

	/// <summary>
	/// Whether a construction of the grammar may throw on a reading the parse gives up
	/// (<see cref="GramCompilerOptions.ConstructionsMayThrow"/>).
	/// </summary>
	public bool ConstructionsMayThrow { get; init; }

	/// <summary>Whether the reader may remember where its rules failed (<see cref="GramCompilerOptions.MemoiseFailures"/>).</summary>
	public bool MemoisesFailures { get; init; }

	/// <summary>Whether the reader may remember where its rules succeeded (<see cref="GramCompilerOptions.MemoiseSuccesses"/>).</summary>
	public bool MemoisesSuccesses { get; init; }

	/// <summary>
	/// The choices the shared-recursion analysis found in the grammar (<see cref="SharedRecursion"/>);
	/// null where the analysis was not run.
	/// </summary>
	public IReadOnlyList<SharedRecursion.Pair>? SharedChoices { get; init; }
}
