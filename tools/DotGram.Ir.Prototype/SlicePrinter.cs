using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

using DotGram.Grammar.Model;

namespace DotGram.Ir.Prototype;

/// <summary>
/// What of a kept statement's code a parser printed to be measured carries: all of it, or one part
/// of it alone — the decision it opens, or the code round it with no decision opened. Only the
/// whole is a parser; the parts read accepted inputs as it does and are timed against it.
/// </summary>
enum SliceAblation
{
	Whole,

	/// <summary>The sentinel opened, nothing else: what the longer tape costs.</summary>
	SentinelOnly,

	/// <summary>Every line of the kept code but the sentinel, which is never opened.</summary>
	WrapperOnly,

	/// <summary>The whole, with every sentinel opened whatever the next item: what the door's test costs.</summary>
	NoDoors,

	/// <summary>The whole but the calls, printed bare: the most that moving a call's code into the function called can save.</summary>
	NoCallSites,

	/// <summary>
	/// The least any design with the same kept statements can cost a reading read for the first
	/// time: every entry one compare of the cursor with the tape's end, the rest out of line and
	/// doing nothing (no door, no flag, no sentinel). Not a parser that resumes.
	/// </summary>
	Ceiling,

	/// <summary>
	/// The form a replay mode that owns the whole reading of a kept statement would print: on the
	/// first reading one compare and the statement as it is; where the tape is read back, a second
	/// copy with the whole of the kept statement's code (opening no sentinel, so that the tape stays
	/// the first reading's). Not a parser that resumes: what the code and its size cost.
	/// </summary>
	SecondCopy,

	/// <summary>
	/// Each function printed twice (C only): the first reading's copy with no kept statement's code at
	/// all, and, where the function keeps a statement, one compare of the cursor with the tape's end on
	/// entry; such an entry where the tape is read back, and every reading again of such a function's
	/// replay, goes to the twin, which is the whole but opens no sentinel (as <see cref="SecondCopy"/>'s
	/// copy does not) and calls only twins. Not a parser that resumes: what the code and its size cost.
	/// </summary>
	ReplayTwin,

	/// <summary>
	/// <see cref="ReplayTwin"/> with a twin that keeps nothing either: what sending the reading back
	/// to a copy of its own costs without what the copy does there.
	/// </summary>
	ReplayTwinBare,

	/// <summary>
	/// <see cref="ReplayTwin"/> where every function's first-reading copy reads its replay once and
	/// sends any reading again to the twin, whether it keeps a statement or not; printed without
	/// resumption, the parser without it with every replay's readings again out of line.
	/// </summary>
	ReplayTwinEvery,

	/// <summary>
	/// <see cref="Ceiling"/> with every replay's readings again out of line as <see cref="ReplayTwinEvery"/>
	/// has them: the ceiling against the parser without resumption printed so.
	/// </summary>
	CeilingEvery,

	/// <summary>
	/// <see cref="ReplayTwinEvery"/> whose twin resumes, with places that have no slot of their own
	/// on the tape: where the tape is read back, an entry into a place is named by its key (the
	/// place, the cursor and the position) and its records are found by the key, kept and judged
	/// stale as a sentinel's are. A parser that resumes.
	/// </summary>
	ReplayTwinKeyed,

	/// <summary>
	/// <see cref="ReplayTwinKeyed"/> printed over a split runtime (see <c>hotRuntime</c> of
	/// <see cref="SlicePrinter.Print"/>): the helpers gcc leaves out of line in the first-reading
	/// copies are inlined there always, and the twins call cold instances of their own. A parser that resumes.
	/// </summary>
	ReplayTwinKeyedSplit,

	/// <summary>
	/// The twins of <see cref="ReplayTwinKeyed"/> alone (C only), entered at the top for the whole
	/// input: no first-reading copy is printed. The resuming set a parser that restarts by budget reads
	/// again with, and what it costs on its own. A parser that resumes.
	/// </summary>
	ResumingSetAlone,

	/// <summary>
	/// Restart by budget (C only): the first reading is <see cref="ReplayTwinEvery"/> printed without
	/// resumption, with a counter on every reading again of a region (a replay, braces, a look, a
	/// choice's alternative, a turn, and an attempt that stood read once more): each charges what it
	/// reads again, at least one, and past the budget (a constant times the furthest position
	/// inspected, plus one) the reading is given up and the input read again from the start by
	/// <see cref="ResumingSetAlone"/>'s twins, printed apart (<see cref="SlicePrinter.PrintResumingSet"/>)
	/// into a translation unit of their own. A parser that resumes where it has to.
	/// </summary>
	RestartByBudget,
}

enum SliceLanguage
{
	TypeScript,
	C,
}

/// <summary>Where a printed TypeScript parser keeps a function's frame while the function calls another.</summary>
public enum SliceFrames
{
	/// <summary>On the language's stack: every call is a call of the language.</summary>
	None,

	/// <summary>
	/// In the parser's own arrays wherever the call may come back to the caller (the two are on
	/// one cycle of calls); every other call is a call of the language, and those nest no deeper
	/// than the grammar has functions.
	/// </summary>
	Recursive,

	/// <summary>In the parser's own arrays for every call: the dearest form, printed to be measured.</summary>
	All,
}

/// <summary>
/// An <see cref="IrProgram"/> printed as a parser in TypeScript or in C: throw-away, and one
/// printer for both, because it prints only what both languages spell alike.
/// </summary>
/// <remarks>
/// <para>
/// A function of the tree is a function of the language, called by plain recursion: the tree
/// has no suspended call (resumption is replay), so a frame is live only while its call is, and
/// turning the calls into explicit frames is the ordinary rewriting of recursion into a loop
/// over a stack. Without it (<see cref="SliceFrames.None"/>, and C always) how deep an input may
/// nest is the language's stack.
/// </para>
/// <para>
/// With frames of its own (TypeScript only) a function that takes part in recursion is printed
/// as a machine: its slots and whatever else lives across a call are in two arrays of the
/// runtime, one of numbers and one of values, its body is cut into numbered states at every such
/// call, and a loop calls whichever machine owns the topmost frame. A call writes the state to
/// come back to, pushes the callee's frame and returns to that loop; the callee's answer comes
/// back as the argument. Statements that hold no such call are printed as they are without frames.
/// </para>
/// <para>
/// A failure is a <c>break</c> out of a <c>do { } while (false)</c> around whatever answers for
/// it, and success is a flag set at the block's end: no <c>goto</c> and no labelled
/// <c>break</c>, so the same text does for a language with neither. A slot is a local of the
/// function, declared once and set back to nothing where its scope is entered.
/// </para>
/// <para>
/// Every host fragment is a call of a named host function, <c>c3</c> or <c>g1</c>, over the
/// captures it names: the fragment's own text is not printed anywhere but in a comment.
/// </para>
/// </remarks>
sealed class SlicePrinter
{
	readonly IrProgram     _program;
	readonly SliceLanguage _language;
	readonly StringBuilder _text = new();

	readonly Dictionary<string, string> _literals = new(StringComparer.Ordinal);

	readonly SliceFrames _frames;

	// Whether the parser counts what it does (for the ladders, never for a parser that is timed).
	readonly bool _counted;

	// What a resuming reading keeps (the evaluator's own fact), and the statement being printed
	// inside its own record, which is not to be wrapped again.
	readonly ResumptionSites _sites;
	readonly bool            _resumes;

	IrStmt? _bare;

	// Printing the copy a replay reads (SecondCopy): nothing in it is copied again, and no sentinel is opened.
	bool _inCopy;

	bool NoSentinel => _ablation == SliceAblation.WrapperOnly || Unopened || Keyed;

	// A copy that opens no sentinel and prints its lookup never taken, so that the tape stays the
	// first reading's: SecondCopy's copy, and ReplayTwin's twin (whose records would be found by a
	// key with no sentinel of their own, which is not printed).
	bool Unopened => _inCopy || _twin && !Keyed;

	// Printing ReplayTwinKeyed's twin: an entry is named by its key, and opens nothing on the tape.
	bool Keyed => _twin && _ablation == SliceAblation.ReplayTwinKeyed;

	// The number each place is keyed by.
	readonly Dictionary<IrStmt, int> _placeIds = new(ReferenceEqualityComparer.Instance);

	// Printing a function's first-reading copy (ReplayTwin), which keeps nothing, or its twin, which keeps all.
	bool _plain;
	bool _twin;

	IrFunction? _function;

	readonly SliceAblation _ablation;

	// Printed over the split runtime (C, a measurement): the runtime's hot helpers are inlined always
	// wherever they are called, and a twin calls their cold instances instead, so that no out-of-line
	// copy of a helper is shared by the first reading and the twins (whose many calls otherwise
	// spend gcc's growth budget and leave the helpers out of line in the first-reading code too).
	bool _hotRuntime;

	// ResumingSetAlone: the twins alone, entered at the top, under the entry's name given.
	bool   _entersTwins;
	string _parseName = "parse";

	// RestartByBudget: the counter on every reading again, the furthest refusal inside looks too,
	// and the budget's constant.
	bool _budget;
	int  _budgetTimes;

	// The helpers gcc's inliner report names as left out of line in first-reading code (with what
	// they call that it also leaves out, and the two whose late inlining printed other code than
	// inlining them always), each with a cold instance in runtime.h under RT_SPLIT. RT_REFUSE, the
	// refusal of a parser that restarts by budget, is a macro over one of them.
	static readonly Regex HotHelpers = new(@"\b(RT_REFUSE|rt_enter|rt_next|rt_cut|rt_retry|rt_refuse|rt_stood|rt_undeferred|rt_replayed|rt_starts|rt_agree|list_add|val_items)\(", RegexOptions.CultureInvariant);

	// What each function reaches by one call or more, and the functions printed as machines.
	readonly Dictionary<IrFunction, HashSet<IrFunction>> _reach    = [];
	readonly HashSet<IrFunction>                         _machines = [];

	// The machine being printed: where its slots are, and how much of the two arrays it has taken.
	// How many numbers and values each machine's frame takes: known once it is printed, and written into the calls of it then.
	readonly Dictionary<IrFunction, (int Numbers, int Values)> _sizes = [];

	Dictionary<IrSlot, string>? _places;
	IrFunction?                 _machine;

	int _numbers;
	int _values;
	int _states;

	int _indent;
	int _temp;

	SlicePrinter(IrProgram program, SliceLanguage language, SliceFrames frames, bool counted, bool resumes, ResumptionCriterion criterion, SliceAblation ablation)
	{
		_ablation = ablation;

		_program  = program;
		_language = language;
		_frames   = language == SliceLanguage.TypeScript ? frames : SliceFrames.None;
		_counted  = counted;
		_sites    = ResumptionSites.Of(program, criterion);
		_resumes  = resumes && _sites.Sites.Count > 0;

		if (_frames == SliceFrames.None)
			return;

		foreach (var function in program.Functions)
		{
			var reached = new HashSet<IrFunction>();
			var pending = new Stack<IrFunction>(Calls(function.Body));

			while (pending.TryPop(out var next))
				if (reached.Add(next))
					foreach (var further in Calls(next.Body))
						pending.Push(further);

			_reach.Add(function, reached);
		}

		foreach (var function in program.Functions)
			if (_frames == SliceFrames.All || _reach[function].Contains(function))
				_machines.Add(function);
	}

	static IEnumerable<IrFunction> Calls(IrStmt stmt)
	{
		if (stmt is IrStmt.Call call)
			yield return call.Target;

		foreach (var child in stmt.Children)
			foreach (var target in Calls(child))
				yield return target;
	}

	bool C => _language == SliceLanguage.C;

	/// <summary>The parser's source: one file, which its language's runtime file stands beside.</summary>
	/// <param name="counted">
	/// Whether the parser counts the functions it enters, the turns it reads and the decisions it
	/// opens or reads back, into the runtime's counters: what a ladder of refusals reads, and
	/// never what a timing does.
	/// </param>
	/// <param name="resumes">
	/// Whether a replay goes straight past what it read before (<see cref="ResumptionSites"/>);
	/// without, the parser is what it was before it did, which is what the cost of it is measured against.
	/// </param>
	/// <param name="criterion">Which statements keep a record (<see cref="ResumptionSites"/>).</param>
	/// <param name="hotRuntime">C only, a measurement: print over the split runtime (<c>RT_SPLIT</c> in runtime.h).</param>
	public static string Print(
		IrProgram program, SliceLanguage language, SliceFrames frames = SliceFrames.None, bool counted = false, bool resumes = true,
		ResumptionCriterion criterion = ResumptionCriterion.FailsBack, SliceAblation ablation = SliceAblation.Whole, bool hotRuntime = false,
		int budget = 4, string parseName = "parse")
	{
		// The split is a way of printing the runtime's calls, not a form of its own: the keyed twin printed over it.
		if (ablation == SliceAblation.ReplayTwinKeyedSplit)
		{
			ablation   = SliceAblation.ReplayTwinKeyed;
			hotRuntime = true;
		}

		var entersTwins = ablation == SliceAblation.ResumingSetAlone;
		var restarts    = ablation == SliceAblation.RestartByBudget;

		if ((entersTwins || restarts) && language != SliceLanguage.C)
			throw new NotSupportedException("Only the C slice prints " + ablation + ".");

		if (entersTwins)
			ablation = SliceAblation.ReplayTwinKeyed;

		if (restarts)
		{
			// Read from the top again, a lazy publication would hand over again what it handed over
			// already: restart is for a publication of the whole input, which is all the tree lowers.
			foreach (var (entry, function) in program.Entries)
			{
				if (!ReadsToTheEnd(function.Body))
					throw new NotSupportedException("A parser that restarts by budget reads only publications of the whole input; " + entry + " is not one.");
			}

			ablation = SliceAblation.ReplayTwinEvery;
			resumes  = false;
		}

		var printer = new SlicePrinter(program, language, frames, counted, resumes, criterion, ablation)
		{
			_hotRuntime  = hotRuntime && language == SliceLanguage.C,
			_entersTwins = entersTwins,
			_parseName   = parseName,
			_budget      = restarts,
			_budgetTimes = budget,
		};

		printer.Run();

		return printer._text.ToString();
	}

	/// <summary>
	/// The resuming set of a parser that restarts by budget: <see cref="SliceAblation.ResumingSetAlone"/>
	/// under the entry <c>parse_resuming</c>, a translation unit of its own (its functions are static).
	/// </summary>
	public static string PrintResumingSet(IrProgram program, bool counted = false)
	{
		return Print(program, SliceLanguage.C, counted: counted, ablation: SliceAblation.ResumingSetAlone, parseName: "parse_resuming");
	}

	// Whether an entry reads to the end of the input itself (the tree's publications all do).
	static bool ReadsToTheEnd(IrStmt stmt)
	{
		return stmt is IrStmt.MatchEnd || stmt is not IrStmt.Call && stmt.Children.Any(ReadsToTheEnd);
	}

	/// <summary>
	/// The host every fragment is a call of, in C, where a host cannot be an object asked by
	/// name: one function a fragment, each handing its number and its arguments to the two
	/// functions the recording host is.
	/// </summary>
	public static string PrintRecordingHostInC(IrProgram program)
	{
		var text = new StringBuilder("#include \"runtime.h\"\n#include \"parser.h\"\n\n");

		text.Append("Val recording_construct(Parser *P, int id, Val *args, int count);\n");
		text.Append("bool recording_guard(Parser *P, int id, Val *args, int count);\n\n");

		foreach (var fragment in program.Fragments)
		{
			var parameters = string.Concat(fragment.Parameters.Select(static (_, index) => ", Val a" + index.ToString(CultureInfo.InvariantCulture)));
			var arguments  = string.Join(", ", fragment.Parameters.Select(static (_, index) => "a" + index.ToString(CultureInfo.InvariantCulture)));
			var count      = fragment.Parameters.Count.ToString(CultureInfo.InvariantCulture);
			var id         = fragment.Id.ToString(CultureInfo.InvariantCulture);

			text.Append(fragment.IsGuard ? "bool" : "Val").Append(" host_").Append(fragment.Name).Append("(Parser *P").Append(parameters).Append(")\n{\n");
			text.Append("\tVal args[] = { ").Append(arguments.Length == 0 ? "VAL_NONE" : arguments).Append(" };\n\n");
			text.Append("\treturn ").Append(fragment.IsGuard ? "recording_guard" : "recording_construct").Append("(P, ").Append(id).Append(", args, ").Append(count).Append(");\n}\n\n");
		}

		return text.ToString();
	}

	/// <summary>A host in C that builds nothing and whose every guard says yes: what a timing of the reading alone is run with.</summary>
	public static string PrintNullHostInC(IrProgram program)
	{
		var text = new StringBuilder("#include \"runtime.h\"\n#include \"parser.h\"\n\n");

		foreach (var fragment in program.Fragments)
		{
			var parameters = string.Concat(fragment.Parameters.Select(static (_, index) => ", Val a" + index.ToString(CultureInfo.InvariantCulture)));

			text.Append(fragment.IsGuard ? "bool" : "Val").Append(" host_").Append(fragment.Name).Append("(Parser *P").Append(parameters).Append(")\n{\n\t(void)P;\n\n");
			text.Append("\treturn ").Append(fragment.IsGuard ? "true" : "VAL_NONE").Append(";\n}\n\n");
		}

		return text.ToString();
	}

	/// <summary>What the C parser declares to its host and its driver.</summary>
	public static string PrintHeaderInC(IrProgram program)
	{
		var text = new StringBuilder("#pragma once\n#include \"runtime.h\"\n\n");

		foreach (var fragment in program.Fragments)
		{
			var parameters = string.Concat(fragment.Parameters.Select(static (name, index) => ", Val a" + index.ToString(CultureInfo.InvariantCulture) + " /* " + name + " */"));

			text.Append("/* ").Append(fragment.Rule).Append(": ").Append(fragment.Text.Replace("*/", "* /", StringComparison.Ordinal)).Append(" */\n");
			text.Append(fragment.IsGuard ? "bool" : "Val").Append(" host_").Append(fragment.Name).Append("(Parser *P").Append(parameters).Append(");\n\n");
		}

		text.Append("int32_t parse(Parser *P, const char *entry);\n");

		return text.ToString();
	}

	void Run()
	{
		if (C)
		{
			Line("#include <string.h>");

			if (_hotRuntime)
				Line("#define RT_SPLIT");

			if (_budget)
			{
				Line("#define RT_RESTART");
				Line("#ifndef RT_BUDGET");
				Line($"#define RT_BUDGET {_budgetTimes.ToString(CultureInfo.InvariantCulture)}");
				Line("#endif");
			}

			Line("#include \"runtime.h\"");
			Line("#include \"parser.h\"");
			Line();

			if (_budget)
			{
				Line("int32_t parse_resuming(Parser *P, const char *entry);");
				Line();
			}

			if (_parseName != "parse")
			{
				Line($"int32_t {_parseName}(Parser *P, const char *entry);");
				Line();
			}

			foreach (var function in _program.Functions)
			{
				if (!(_entersTwins && Twinned(function)))
					Line($"static int32_t {Name(function)}(Parser *P, int32_t p);");

				if (Twinned(function))
					Line($"static {TwinAttributes} int32_t {Name(function)}_r(Parser *P, int32_t p, int32_t u0);");
			}
		}
		else
		{
			Line("import { Runtime } from \"./runtime.ts\";");
			Line();
			Line("// The host: a function a fragment, over the captures the fragment names.");
			Line("export interface Host");
			Line("{");

			foreach (var fragment in _program.Fragments)
			{
				var parameters = string.Join(", ", fragment.Parameters.Select(static name => name + ": any"));

				Line($"\t/** {fragment.Rule}: {fragment.Text.Replace("*/", "* /", StringComparison.Ordinal)} */");
				Line($"\t{fragment.Name}({parameters}): {(fragment.IsGuard ? "boolean" : "any")};");
			}

			Line("}");
		}

		Line();

		var body = _text.Length;

		foreach (var function in _program.Functions)
		{
			if (_machines.Contains(function))
				Machine(function);
			else
				Function(function);
		}

		foreach (var (function, (numbers, values)) in _sizes)
		{
			var index = function.Index.ToString(CultureInfo.InvariantCulture);

			_text.Replace($"@numbers{index}@", numbers.ToString(CultureInfo.InvariantCulture));
			_text.Replace($"@values{index}@", values.ToString(CultureInfo.InvariantCulture));
		}

		if (_machines.Count > 0)
		{
			// The loop every machine returns to: the topmost frame says whose turn it is, and what
			// one machine returns is what the next is given - a position to read from, or an answer.
			Line("const machines: ((P: Runtime, p: number) => number)[] = [" + string.Join(", ", _program.Functions.Select(function => _machines.Contains(function) ? Name(function) + "_m" : "none")) + "];");
			Line();
			Line("function none(P: Runtime, p: number): number");
			Line("{");
			Line("\tthrow new Error(\"Not a machine.\");");
			Line("}");
			Line();
			Line("function run(P: Runtime, id: number, numbers: number, values: number, p: number): number");
			Line("{");
			Line("\tconst base = P.top;");
			Line();
			Line("\tP.call(id, numbers, values);");
			Line();
			Line("\twhile (P.top > base)");
			Line("\t\tp = machines[P.I[P.top - 1]](P, p);");
			Line();
			Line("\treturn p;");
			Line("}");
			Line();
		}

		// The texts the functions compare the input with, which C wants as arrays before them.
		if (C && _literals.Count > 0)
		{
			var tables = new StringBuilder();

			foreach (var (text, name) in _literals)
				tables.Append("static const uint16_t ").Append(name).Append("[] = { ").Append(string.Join(", ", text.Select(static unit => ((int)unit).ToString(CultureInfo.InvariantCulture)))).Append(" };\n");

			_text.Insert(body, tables.Append('\n').ToString());
		}

		// The doors' tables, before the functions too (TypeScript reads them only once a parse runs).
		if (_doorTables.Count > 0)
		{
			var tables = new StringBuilder();

			foreach (var (bits, name) in _doorTables)
				tables.Append(C ? $"static const uint32_t {name}[4] = {{ {bits.Replace(",", "u, ", StringComparison.Ordinal)}u }};\n" : $"const {name} = new Int32Array([{bits.Replace(",", " | 0, ", StringComparison.Ordinal)} | 0]);\n");

			_text.Insert(body, tables.Append('\n').ToString());
		}

		if (C)
		{
			if (_budget)
			{
				// The first reading, until it has read again more than the budget: then from the start,
				// resuming. The entry that can be gone back to is a function of its own, so that the
				// first reading's own entry is printed and inlined as the parser without restart has it.
				Line("static __attribute__((noinline)) int32_t parse_first(Parser *P, const char *entry);");
				Line();
				Line($"int32_t {_parseName}(Parser *P, const char *entry)");
				Line("{");
				Line("\tvoid *back[5];");
				Line();
				Line("\tP->reread  = 0;");
				Line("\tP->far     = -1;");
				Line("\tP->restart = back;");
				Line();
				Line("\tif (__builtin_setjmp(back) != 0)");
				Line("\t{");
				Line("\t\trt_restarted(P);");
				Line();
				Line("\t\treturn parse_resuming(P, entry);");
				Line("\t}");
				Line();
				Line("\treturn parse_first(P, entry);");
				Line("}");
				Line();
				Line("static __attribute__((noinline)) int32_t parse_first(Parser *P, const char *entry)");
			}
			else
				Line($"int32_t {_parseName}(Parser *P, const char *entry)");

			Line("{");

			foreach (var (entry, function) in _program.Entries)
				Line($"\tif (strcmp(entry, \"{entry}\") == 0) return {Name(function)}{(_entersTwins && Twinned(function) ? "_r(P, 0, 0)" : "(P, 0)")};");

			Line("\treturn -2;");
			Line("}");
		}
		else
		{
			Line("export function parse(P: Runtime, entry: string): number");
			Line("{");
			Line("\tswitch (entry)");
			Line("\t{");

			foreach (var (entry, function) in _program.Entries)
				Line($"\t\tcase \"{entry}\": return {Name(function)}(P, 0);");

			Line("\t\tdefault: throw new Error(\"No entry \" + entry);");
			Line("\t}");
			Line("}");
			Line();
			Line("export const expected: string[] = " + "[" + string.Join(", ", _program.Expected.Select(Quote)) + "];");
		}
	}

	static string Name(IrFunction function)
	{
		return "f" + function.Index.ToString(CultureInfo.InvariantCulture) + "_" + new string([.. function.Name.Select(static unit => char.IsAsciiLetterOrDigit(unit) ? unit : '_')]);
	}

	static string Quote(string text)
	{
		var quoted = new StringBuilder("\"");

		foreach (var unit in text)
		{
			if (unit is '"' or '\\' || unit < ' ' || unit > '~')
				quoted.Append("\\u").Append(((int)unit).ToString("x4", CultureInfo.InvariantCulture));
			else
				quoted.Append(unit);
		}

		return quoted.Append('"').ToString();
	}

	void Line(string text = "")
	{
		if (text.Length > 0)
			_text.Append('\t', _indent).Append(text);

		_text.Append('\n');
	}

	string Temp(string stem)
	{
		return stem + (_temp++).ToString(CultureInfo.InvariantCulture);
	}

	// The spellings the two languages do not share.
	string Int(string name, string value)
	{
		return C ? $"int32_t {name} = {value};" : $"let {name} = {value};";
	}

	string Flag(string name)
	{
		return C ? $"bool {name} = false;" : $"let {name} = false;";
	}

	string Member(string name)
	{
		return C ? "P->" + name : "P." + name;
	}

	string Runtime(string function, string arguments)
	{
		return C ? $"rt_{function}(P{(arguments.Length > 0 ? ", " : "")}{arguments})" : $"P.{function}({arguments})";
	}

	string Unit(string at)
	{
		return C ? $"P->text[{at}]" : $"text.charCodeAt({at})";
	}

	/// <summary>A decision opened or read back, counted where the parser counts.</summary>
	string Enter(string last)
	{
		return _counted ? Runtime("enterCounted", last) : Runtime("enter", last);
	}

	/// <summary>What counts one more step, where the parser counts: a function entered, a turn begun.</summary>
	void Step()
	{
		if (_counted)
			Line(C ? "P->kSteps++;" : "P.kSteps++;");
	}

	// The doors' tables: the items below 128 a door lets through, a bit an item, by the ranges' spelling.
	readonly Dictionary<string, string> _doorTables = new(StringComparer.Ordinal);

	/// <summary>
	/// Whether the next item can begin what a kept statement reads: below 128 one bit of a table,
	/// above it the ranges there are, compared one by one (rare in the inputs that pay for this).
	/// </summary>
	string DoorTest(IReadOnlyList<CharRange> door)
	{
		var bits = new uint[4];
		var high = new List<CharRange>();

		foreach (var range in door)
		{
			for (int unit = range.From; unit <= Math.Min((int)range.To, 127); unit++)
				bits[unit >> 5] |= 1u << (unit & 31);

			if (range.To >= 128)
				high.Add(new CharRange((char)Math.Max(128, (int)range.From), range.To));
		}

		var spelled = string.Join(",", bits.Select(static word => word.ToString(CultureInfo.InvariantCulture)));

		if (!_doorTables.TryGetValue(spelled, out var table))
			_doorTables.Add(spelled, table = "D" + _doorTables.Count.ToString(CultureInfo.InvariantCulture));

		var above = high.Count == 0 ? (C ? "0" : "false") : Within(new IrStmt.MatchSet(high, false, 0));

		return C
			? $"p < len && (c = {Unit("p")}, c < 128 ? ({table}[c >> 5] >> (c & 31)) & 1u : {above})"
			: $"p < len && (c = {Unit("p")}, c < 128 ? (({table}[c >> 5] >> (c & 31)) & 1) !== 0 : {above})";
	}

	/// <summary>Whether the statement opens a sentinel and keeps a record.</summary>
	bool Kept(IrStmt stmt)
	{
		return _resumes && !_plain && !(_twin && _ablation == SliceAblation.ReplayTwinBare) && _ablation is not (SliceAblation.SentinelOnly or SliceAblation.Ceiling or SliceAblation.CeilingEvery) && !(_ablation == SliceAblation.NoCallSites && stmt is IrStmt.Call) &&
			!ReferenceEquals(stmt, _bare) && _sites.Keeps(stmt);
	}

	/// <summary>For <see cref="SliceAblation.SentinelOnly"/>: the decision a kept statement opens, and the statement as it is.</summary>
	bool SentinelAlone(IrStmt stmt)
	{
		if (!(_resumes && _ablation is SliceAblation.SentinelOnly or SliceAblation.Ceiling or SliceAblation.CeilingEvery && !ReferenceEquals(stmt, _bare) && _sites.Keeps(stmt)))
			return false;

		if (_ablation is SliceAblation.Ceiling or SliceAblation.CeilingEvery)
			Line($"if ({Member("cursor")} < {Member("count")}) {Runtime("slow", "")};");
		else if (_sites.Doors.TryGetValue(stmt, out var door))
			Line($"if ({DoorTest(door)}) {Enter("0")};");
		else
			Line($"{Enter("0")};");

		return true;
	}

	/// <summary>The slots a kept statement writes: what a record holds and gives back (the evaluator's own list).</summary>
	static IReadOnlyList<IrSlot> Writes(IrStmt stmt)
	{
		return stmt switch
		{
			IrStmt.Repeat repeat => repeat.Resets,
			IrStmt.Commit commit => commit.Resets,
			IrStmt.Look look     => look.Resets,
			IrStmt.Call { Into: { } into } => [into],
			_                    => [],
		};
	}

	/// <summary>A record's slots written from the slots, at the record <paramref name="record"/> where it was kept.</summary>
	void Hold(IReadOnlyList<IrSlot> slots, string record)
	{
		if (slots.Count == 0)
			return;

		var b = Temp("kb");

		Line(C ? $"int32_t {b} = P->kept[{record}].base;" : $"const {b} = P.kBase[{record}];");

		for (var i = 0; i < slots.Count; i++)
		{
			var slot = Slot(slots[i]);
			var at   = i.ToString(CultureInfo.InvariantCulture);

			if (C)
			{
				Line(slots[i].Kind switch
				{
					IrSlotKind.List  => $"P->kv[{b} + {at}] = (Val){{ V_LIST, {slot} == NULL ? 0 : {slot}->count, 0, {slot} }};",
					IrSlotKind.Value => $"P->kv[{b} + {at}] = {slot};",
					_                => $"P->kv[{b} + {at}] = (Val){{ V_NONE, {slot}, 0, NULL }};",
				});
			}
			else
			{
				Line($"P.kv[{b} + {(i * 2).ToString(CultureInfo.InvariantCulture)}] = {slot};");

				if (slots[i].Kind == IrSlotKind.List)
					Line($"P.kv[{b} + {(i * 2 + 1).ToString(CultureInfo.InvariantCulture)}] = {slot} === undefined ? 0 : {slot}.length;");
			}
		}
	}

	/// <summary>The slots put back as the record <paramref name="record"/> holds them.</summary>
	void Give(IReadOnlyList<IrSlot> slots, string record)
	{
		if (slots.Count == 0)
			return;

		var b = Temp("kb");

		Line(C ? $"int32_t {b} = P->kept[{record}].base;" : $"const {b} = P.kBase[{record}];");

		for (var i = 0; i < slots.Count; i++)
		{
			var slot = Slot(slots[i]);
			var at   = i.ToString(CultureInfo.InvariantCulture);

			if (C)
			{
				Line(slots[i].Kind switch
				{
					IrSlotKind.List  => $"{slot} = (List *)P->kv[{b} + {at}].ptr; if ({slot} != NULL) {slot}->count = P->kv[{b} + {at}].from;",
					IrSlotKind.Value => $"{slot} = P->kv[{b} + {at}];",
					_                => $"{slot} = P->kv[{b} + {at}].from;",
				});
			}
			else
			{
				Line($"{slot} = P.kv[{b} + {(i * 2).ToString(CultureInfo.InvariantCulture)}];");

				if (slots[i].Kind == IrSlotKind.List)
					Line($"if ({slot} !== undefined) {slot}.length = P.kv[{b} + {(i * 2 + 1).ToString(CultureInfo.InvariantCulture)}];");
			}
		}
	}

	/// <summary>A record kept where the reading is a replay: of where a turn begins, or of where the statement ended.</summary>
	void Record(string owner, bool end, string position, string turns, string read, IReadOnlyList<IrSlot> slots)
	{
		var r = Temp("kr");

		Line($"if ({Member("cursor")} < {Member("count")} && {owner} >= 0)");
		Line("{");
		_indent++;
		Line(Int(r, Runtime("keep", $"{owner}, {(end ? "true" : "false")}, {position}, {turns}, {read}, {slots.Count.ToString(CultureInfo.InvariantCulture)}")));

		if (slots.Count > 0)
		{
			Line($"if ({r} >= 0)");
			Line("{");
			_indent++;
			Hold(slots, r);
			_indent--;
			Line("}");
		}
		else if (C)
			Line($"(void){r};");

		_indent--;
		Line("}");
	}

	/// <summary>
	/// A sentinel opened (into <paramref name="owner"/>, a local declared before or a place of the frame),
	/// and where it is read back, the latest record of its entry taken if it stands for this
	/// reading — any record of a repetition, only where a statement ended of anything else.
	/// </summary>
	void Sentinel(IrStmt stmt, string owner, Action<string> taken, bool endOnly)
	{
		var h = Temp("h");

		// Where the next item cannot begin what the statement reads, its work is bounded and it opens nothing.
		if (_ablation != SliceAblation.NoDoors && _sites.Doors.TryGetValue(stmt, out var door))
			Line($"if ({DoorTest(door)})");

		Line("{");
		_indent++;

		if (NoSentinel)
			Line($"{owner} = -1;");

		// Read for the first time, a sentinel is opened and nothing else is asked; read back, its
		// latest record is looked for. (In the copy a replay reads, the lookup is printed but never
		// taken: there is no sentinel to read back.)
		Line(Unopened ? $"if ({Member("cursor")} < {Member("count")} && {Member("noCutCheck")})" : $"if ({Member("cursor")} < {Member("count")})");
		Line("{");
		_indent++;

		if (Keyed)
		{
			if (!_placeIds.TryGetValue(stmt, out var place))
				_placeIds.Add(stmt, place = _placeIds.Count);

			Line($"{owner} = rt_key(P, {place.ToString(CultureInfo.InvariantCulture)}, P->cursor, p);");
		}
		else
			Line($"{owner} = {(_counted ? Enter("0") : (C ? "P->cursor++" : "P.cursor++"))};");

		Line(Int(h, Runtime("head", owner)));
		Line($"if ({h} >= 0 && {Runtime("usable", h)}{(endOnly ? $" && ({Field(h, "flags")} & 1) != 0" : "")})");
		Line("{");
		_indent++;
		taken(h);
		_indent--;
		Line("}");
		_indent--;
		Line("}");

		if (!NoSentinel)
			Line($"else {owner} = {(_counted ? Enter("0") : Runtime("sentinel", ""))};");

		_indent--;
		Line("}");
	}

	/// <summary>A kept statement other than a repetition: its sentinel, and past it where a record of its end stands, or it read and its end kept.</summary>
	void Site(IrStmt stmt, string fail)
	{
		var owner  = Temp("o");
		var read   = Temp("ok");
		var taken  = Temp("taken");
		var writes = Writes(stmt);

		Line("{");
		_indent++;
		Line(Int(owner, "-1"));
		Line(Flag(read));
		Line(Flag(taken));
		Sentinel(stmt, owner, h =>
		{
			Line($"p = {Field(h, "pos")};");
			Line($"{Member("cursor")} = {Field(h, "key")};");
			Give(writes, h);
			Line($"{read} = ({Field(h, "flags")} & 2) != 0;");
			Line($"{taken} = true;");
			Jumped();
		}, endOnly: true);
		Line($"if (!{taken})");
		Line("{");
		_indent++;

		var outer = _bare;

		_bare = stmt;
		Attempt(stmt, read);
		_bare = outer;

		Record(owner, end: true, "p", "0", read, writes);
		_indent--;
		Line("}");
		Line($"if (!{read}) {fail}");
		_indent--;
		Line("}");
	}

	/// <summary><see cref="Site"/> for a statement with a call out of the machine in it: its sentinel in the frame, its end and its failure states of their own.</summary>
	void FlatSite(IrStmt stmt, int fail)
	{
		var owner  = FrameNumber();
		var after  = State();
		var failed = State();
		var writes = Writes(stmt);

		Line($"{owner} = -1;");
		Sentinel(stmt, owner, h =>
		{
			Line($"p = {Field(h, "pos")};");
			Line($"{Member("cursor")} = {Field(h, "key")};");
			Give(writes, h);
			Jumped();
			Line($"if (({Field(h, "flags")} & 2) != 0) {Jump(after)}");
			Line(FailTo(fail));
		}, endOnly: true);

		var outer = _bare;

		_bare = stmt;
		Flat(stmt, failed);
		_bare = outer;

		Record(owner, end: true, "p", "0", "true", writes);
		Line(Jump(after));
		Place(failed);
		Record(owner, end: true, "p", "0", "false", writes);
		Line(FailTo(fail));
		Place(after);
	}

	string Field(string record, string name)
	{
		return C ? $"P->kept[{record}].{name}" : $"P.k{char.ToUpperInvariant(name[0])}{name[1..]}[{record}]";
	}

	void Jumped()
	{
		if (_counted)
			Line(C ? "P->kJumps++;" : "P.kJumps++;");
	}

	string Decision(string way)
	{
		return C ? $"P->ways[{way} * 2]" : $"P.ways[{way} * 2]";
	}

	string Slot(IrSlot slot)
	{
		return _places is { } places ? places[slot] : "s" + slot.Index.ToString(CultureInfo.InvariantCulture);
	}

	string Nothing(IrSlot slot)
	{
		return slot.Kind switch
		{
			IrSlotKind.Position or IrSlotKind.Rebound => "-1",
			IrSlotKind.List                           => C ? "NULL" : "undefined",
			_                                         => C ? "VAL_NONE" : "undefined",
		};
	}

	/// <summary>
	/// Whether the function is printed with a twin (<see cref="SliceAblation.ReplayTwin"/>): every one is,
	/// so that twins call only twins and the first reading's functions only each other, and the first
	/// reading's calls are what they were without resumption (a twin calling them would make each a
	/// function called twice, which the compiler stops inlining).
	/// </summary>
	bool Twinned(IrFunction function)
	{
		return C && _ablation is SliceAblation.ReplayTwin or SliceAblation.ReplayTwinBare or SliceAblation.ReplayTwinEvery or SliceAblation.CeilingEvery or SliceAblation.ReplayTwinKeyed &&
			(_resumes || _ablation == SliceAblation.ReplayTwinEvery);
	}

	/// <summary>Whether the function keeps a statement: where its first reading's copy sends a reading back to the twin.</summary>
	bool Places(IrFunction function)
	{
		return _resumes && Keeps(function.Body);

		bool Keeps(IrStmt stmt)
		{
			return _sites.Keeps(stmt) || stmt.Children.Any(Keeps);
		}
	}

	// A twin is out of line and cold: inlined into the first reading's copy, the whole of it widened
	// every entry's frame; not cold, its call was laid out among the first reading's code (measured).
	const string TwinAttributes = "__attribute__((noinline, cold))";

	void Function(IrFunction function)
	{
		if (!Twinned(function))
		{
			Copy(function);

			return;
		}

		if (!_entersTwins)
		{
			_plain = true;
			Copy(function);
			_plain = false;
		}

		_twin  = true;

		var start = _text.Length;

		Copy(function);
		_twin = false;

		if (_hotRuntime)
		{
			var twin = HotHelpers.Replace(_text.ToString(start, _text.Length - start), "$1_cold(");

			_text.Length = start;
			_text.Append(twin);
		}
	}

	void Copy(IrFunction function)
	{
		_temp     = 0;
		_function = function;

		if (_twin)
			Line($"static {TwinAttributes} int32_t {Name(function)}_r(Parser *P, int32_t p, int32_t u0)");
		else
			Line(C ? $"static int32_t {Name(function)}(Parser *P, int32_t p)" : $"function {Name(function)}(P: Runtime, p: number): number");

		Line("{");
		_indent++;

		if (_plain && Places(function) && _ablation != SliceAblation.CeilingEvery)
			Line($"if (P->cursor < P->count) return {Name(function)}_r(P, p, 0);");
		else if (_twin)
			Line("(void)u0;");

		if (!C)
			Line("const text = P.text;");

		Line(C ? "const int32_t len = P->len;" : "const len = P.len;");
		Line(C ? "uint16_t c = 0;" : "let c = 0;");
		Step();

		foreach (var slot in Slots(function.Body).Distinct().OrderBy(static slot => slot.Index))
		{
			Line(slot.Kind switch
			{
				IrSlotKind.Position or IrSlotKind.Rebound => Int(Slot(slot), "-1") + $" // {slot.Name}",
				IrSlotKind.List                           => (C ? $"List *{Slot(slot)} = NULL;" : $"let {Slot(slot)}: any[] | undefined = undefined;") + $" // {slot.Name}",
				_                                         => (C ? $"Val {Slot(slot)} = VAL_NONE;" : $"let {Slot(slot)}: any = undefined;") + $" // {slot.Name}{(slot.Type is null ? "" : " : " + slot.Type)}",
			});
		}

		Line();
		Statement(function.Body, "return -1;");

		if (function.Result is { } result)
			Line($"{Member("rv")} = {Slot(result)};");

		Line(C ? "(void)c; (void)len;" : "void c;");
		Line("return p;");
		_indent--;
		Line("}");
		Line();
	}

	static IEnumerable<IrSlot> Slots(IrStmt stmt)
	{
		if (stmt is IrStmt.Scope scope)
			foreach (var slot in scope.Slots)
				yield return slot;

		foreach (var child in stmt.Children)
			foreach (var slot in Slots(child))
				yield return slot;
	}

	string Refuse(string at, int expected)
	{
		// Restart by budget: a refusal inside a look says nothing, but the position it inspected is
		// the furthest the reading has reached, where it is further than any before.
		if (_budget)
			return $"RT_REFUSE(P, {at}, {expected.ToString(CultureInfo.InvariantCulture)});";

		return $"if ({Member("quiet")} == 0 && {at} >= {Member("refusedAt")}) {Runtime("refuse", at + ", " + expected.ToString(CultureInfo.InvariantCulture))};";
	}

	/// <summary>The condition under which the item in <c>c</c> is one of the set's.</summary>
	static string Within(IrStmt.MatchSet set)
	{
		if (set.Ranges.Count == 0)
			return set.IsNegated ? "true" : "false";

		var test = string.Join(" || ", set.Ranges.Select(static range => range.From == range.To
			? "c == " + ((int)range.From).ToString(CultureInfo.InvariantCulture)
			: "(c >= " + ((int)range.From).ToString(CultureInfo.InvariantCulture) + " && c <= " + ((int)range.To).ToString(CultureInfo.InvariantCulture) + ")"));

		return set.IsNegated ? "!(" + test + ")" : "(" + test + ")";
	}

	string Expression(IrExpr expr)
	{
		return expr switch
		{
			IrExpr.Position      => "p",
			IrExpr.Slot slot     => Slot(slot.Of),
			IrExpr.Text text     => C ? $"val_text({Slot(text.From)}, p)" : $"text.substring({Slot(text.From)}, p)",
			IrExpr.Items items   => C ? $"val_items(P, {Slot(items.Of)})" : $"({Slot(items.Of)} === undefined ? [] : {Slot(items.Of)}.slice())",
			IrExpr.Number number => number.Value.ToString(CultureInfo.InvariantCulture),
			IrExpr.Length length => $"({Slot(length.From)} < 0 || {Slot(length.To)} < 0 ? 0 : {Slot(length.To)} - {Slot(length.From)})",
			_                    => throw new ArgumentOutOfRangeException(nameof(expr)),
		};
	}

	/// <summary>What the slots a failed alternative may have written held before it, kept in locals.</summary>
	/// <param name="inFrame">In the machine's frame, where a call stands between the keeping and the putting back.</param>
	string[] Keep(IReadOnlyList<IrSlot> slots, bool inFrame = false)
	{
		var kept = new string[slots.Count];

		for (var i = 0; i < kept.Length; i++)
		{
			var slot = slots[i];

			if (inFrame)
			{
				kept[i] = slot.Kind == IrSlotKind.Value ? FrameValue() : FrameNumber();

				Line(slot.Kind == IrSlotKind.List
					? $"{kept[i]} = {Slot(slot)} === undefined ? -1 : {Slot(slot)}.length;"
					: $"{kept[i]} = {Slot(slot)};");

				continue;
			}

			kept[i] = Temp("k");

			Line(slot.Kind switch
			{
				IrSlotKind.List  => C ? $"int32_t {kept[i]} = {Slot(slot)} == NULL ? -1 : {Slot(slot)}->count;" : $"const {kept[i]} = {Slot(slot)} === undefined ? -1 : {Slot(slot)}.length;",
				IrSlotKind.Value => C ? $"Val {kept[i]} = {Slot(slot)};" : $"const {kept[i]} = {Slot(slot)};",
				_                => C ? $"int32_t {kept[i]} = {Slot(slot)};" : $"const {kept[i]} = {Slot(slot)};",
			});
		}

		return kept;
	}

	void PutBack(IReadOnlyList<IrSlot> slots, string[] kept)
	{
		for (var i = 0; i < kept.Length; i++)
		{
			var slot = slots[i];

			Line(slot.Kind == IrSlotKind.List
				? C ? $"if ({kept[i]} < 0) {Slot(slot)} = NULL; else {Slot(slot)}->count = {kept[i]};" : $"if ({kept[i]} < 0) {Slot(slot)} = undefined; else {Slot(slot)}!.length = {kept[i]};"
				: $"{Slot(slot)} = {kept[i]};");
		}
	}

	/// <summary>A body that answers for its own failures: run once, with a flag that says whether it read.</summary>
	void Attempt(IrStmt body, string flag)
	{
		Line("do");
		Line("{");
		_indent++;
		Statement(body, "break;");
		Line($"{flag} = true;");
		_indent--;
		Line("} while (false);");
	}

	/// <param name="fail">What a failure of this statement is, where it stands: a <c>break</c> or a <c>return</c>.</param>
	void Statement(IrStmt stmt, string fail)
	{
		if (_ablation == SliceAblation.SecondCopy && !_inCopy && Kept(stmt))
		{
			var outer = _bare;

			Line($"if ({Member("cursor")} < {Member("count")})");
			Line("{");
			_indent++;
			_inCopy = true;
			Statement(stmt, fail);
			_inCopy = false;
			_indent--;
			Line("}");
			Line("else");
			Line("{");
			_indent++;
			_bare = stmt;
			Statement(stmt, fail);
			_bare = outer;
			_indent--;
			Line("}");

			return;
		}

		if (SentinelAlone(stmt))
		{
			var outer = _bare;

			_bare = stmt;
			Statement(stmt, fail);
			_bare = outer;

			return;
		}

		if (stmt is not IrStmt.Repeat && Kept(stmt))
		{
			Site(stmt, fail);

			return;
		}

		switch (stmt)
		{
			case IrStmt.Block block:
				foreach (var item in block.Items)
					Statement(item, fail);

				break;

			case IrStmt.Scope scope:
				foreach (var slot in scope.Slots)
					Line($"{Slot(slot)} = {Nothing(slot)};");

				Statement(scope.Body, fail);

				break;

			case IrStmt.MatchSet set:
				Line($"if (p >= len) {{ {Refuse("p", set.Expected)} {fail} }}");
				Line($"c = {Unit("p")};");
				Line($"if (!{Within(set)}) {{ {Refuse("p", set.Expected)} {fail} }}");
				Line("p++;");

				break;

			case IrStmt.MatchText literal:
			{
				if (literal.IgnoreCase)
					throw new NotSupportedException("The slice prints no text that ignores case.");

				var length = literal.Text.Length.ToString(CultureInfo.InvariantCulture);

				if (literal.Text.Length == 1)
				{
					Line($"if (p >= len || {Unit("p")} != {((int)literal.Text[0]).ToString(CultureInfo.InvariantCulture)}) {{ {Refuse("p", literal.Expected)} {fail} }}");
					Line("p++;");

					break;
				}

				var spelled = C ? Literal(literal.Text) + ", " + length : Quote(literal.Text);
				var at      = Temp("a");

				Line($"if (!{Runtime("starts", "p, " + spelled)})");
				Line("{");
				Line("\t" + Int(at, "p + " + Runtime("agree", "p, " + spelled)));
				Line($"\t{Refuse(at, literal.Expected)}");
				Line($"\t{fail}");
				Line("}");
				Line($"p += {length};");

				break;
			}

			case IrStmt.MatchSame same:
			{
				var from   = Temp("f");
				var length = Temp("n");

				Line("{");
				_indent++;
				Line(Int(from, $"{Slot(same.From)} < 0 ? 0 : {Slot(same.From)}"));
				Line(Int(length, $"{Slot(same.From)} < 0 || {Slot(same.To)} < 0 ? 0 : {Slot(same.To)} - {Slot(same.From)}"));
				Line($"if (p + {length} > len || !{Runtime("same", $"{from}, p, {length}")}) {{ {Refuse("p", same.Expected)} {fail} }}");
				Line($"p += {length};");
				_indent--;
				Line("}");

				break;
			}

			case IrStmt.MatchEnd end:
				Line($"if (p != len) {{ {Refuse("p", end.Expected)} {fail} }}");

				break;

			case IrStmt.Call call:
			{
				var end = Temp("q");

				Line("{");
				_indent++;
				Line(Int(end, _twin ? $"{Name(call.Target)}_r(P, p, 0)" : $"{Name(call.Target)}(P, p)"));

				// A machine further down may have outgrown the array of numbers and left another in its place.
				if (_machine is not null && (_machines.Contains(call.Target) || _reach[call.Target].Overlaps(_machines)))
					Line("I = P.I;");

				Line($"if ({end} < 0) {fail}");
				Line($"p = {end};");

				if (call.Into is { } into)
				{
					Line(call.Appends
						? C ? $"if (P->unbuilt == 0) {Slot(into)} = list_add(P, {Slot(into)}, P->rv);" : $"if (P.unbuilt == 0) ({Slot(into)} ??= []).push(P.rv);"
						: $"if ({Member("unbuilt")} == 0) {Slot(into)} = {Member("rv")};");
				}

				_indent--;
				Line("}");

				break;
			}

			case IrStmt.Choice choice:
			{
				var start = Temp("c");
				var read  = Temp("ok");
				var way   = Temp("w");
				var last  = choice.Alternatives.Count - 1;

				Line("{");
				_indent++;
				Line(Int(start, "p"));
				Line(Flag(read));

				if (choice.Resumable)
					Line(Int(way, Enter(last.ToString(CultureInfo.InvariantCulture))));

				for (var i = 0; i <= last; i++)
				{
					var index = i.ToString(CultureInfo.InvariantCulture);

					Line(choice.Resumable ? $"if (!{read} && {Decision(way)} <= {index})" : $"if (!{read})");
					Line("{");
					_indent++;

					var kept = Keep(choice.Resets[i]);

					if (choice.Resumable && i < last)
					{
						// The alternative is read again with each of its own decisions that can still
						// move before the choice moves on: its next alternative comes after them.
						var mark = Temp("m");

						Line(Int(mark, Member("cursor")));
						Line("for (;;)");
						Line("{");
						_indent++;
						Attempt(choice.Alternatives[i], read);
						Line($"if ({read} || !({Member("cursor")} > {mark} && {Runtime("retry", mark)})) break;");
						Charge(start);
						Line($"p = {start};");
						PutBack(choice.Resets[i], kept);
						_indent--;
						Line("}");
					}
					else
						Attempt(choice.Alternatives[i], read);

					Line($"if (!{read})");
					Line("{");
					_indent++;
					Line($"p = {start};");
					PutBack(choice.Resets[i], kept);

					if (choice.Resumable && i < last)
						Line(Runtime("next", $"{way}, {(i + 1).ToString(CultureInfo.InvariantCulture)}") + ";");

					_indent--;
					Line("}");
					_indent--;
					Line("}");
				}

				Line($"if (!{read}) {fail}");
				_indent--;
				Line("}");

				break;
			}

			// A run of one set that nothing is ever asked back from: a loop over the input.
			case IrStmt.Repeat { Body: IrStmt.MatchSet set, Resumable: false, Resets.Count: 0, Min: IrExpr.Number min, Max: null or IrExpr.Number } run when !Kept(run):
			{
				var turns = Temp("n");
				var most  = run.Max is IrExpr.Number max ? max.Value.ToString(CultureInfo.InvariantCulture) : "2147483647";

				Line("{");
				_indent++;
				Line(Int(turns, "0"));
				Line($"while ({turns} < {most})");
				Line("{");

				if (_counted)
					Line(C ? "\tP->kSteps++;" : "\tP.kSteps++;");

				Line($"\tif (p >= len) {{ {Refuse("p", set.Expected)} break; }}");
				Line($"\tc = {Unit("p")};");
				Line($"\tif (!{Within(set)}) {{ {Refuse("p", set.Expected)} break; }}");
				Line("\tp++;");
				Line($"\t{turns}++;");
				Line("}");

				if (min.Value > 0)
					Line($"if ({turns} < {min.Value.ToString(CultureInfo.InvariantCulture)}) {fail}");

				_indent--;
				Line("}");

				break;
			}

			case IrStmt.Repeat repeat:
			{
				var turns = Temp("n");
				var least = Temp("min");
				var most  = Temp("max");
				var start = Temp("t");
				var read  = Temp("ok");
				var way   = Temp("w");

				var kept   = Kept(repeat);
				var owner  = Temp("o");

				Line("{");
				_indent++;
				Line(Int(turns, "0"));
				Line(Int(least, Expression(repeat.Min)));
				Line(Int(most, repeat.Max is null ? "2147483647" : Expression(repeat.Max)));

				if (kept)
				{
					Line(Int(owner, "-1"));
					Sentinel(repeat, owner, (h) =>
					{
						Line($"p = {Field(h, "pos")};");
						Line($"{turns} = {Field(h, "turns")};");
						Line($"{Member("cursor")} = {Field(h, "key")};");
						Give(repeat.Resets, h);
						Line($"if (({Field(h, "flags")} & 1) != 0) {most} = {turns};");
						Jumped();
					}, endOnly: false);
				}

				Line($"while ({turns} < {most})");
				Line("{");
				_indent++;
				Step();
				Line(Int(start, "p"));
				Line(Flag(read));

				if (kept)
					Record(owner, end: false, start, turns, "false", repeat.Resets);

				if (repeat.Resumable)
				{
					Line(Int(way, "-1"));
					Line($"if ({turns} >= {least})");
					Line("{");
					Line($"\t{way} = {Enter("1")};");
					Line($"\tif ({Decision(way)} == 1) break;");
					Line("}");
				}

				var held = Keep(repeat.Resets);

				if (repeat.Resumable)
				{
					// A turn that fails is read again with each of its own decisions that can still
					// move before the repetition gives it up: its own decision comes before them.
					var mark = Temp("m");

					Line(Int(mark, Member("cursor")));
					Line("for (;;)");
					Line("{");
					_indent++;
					Attempt(repeat.Body, read);
					Line($"if ({read} || !({way} >= 0 && {Member("cursor")} > {mark} && {Runtime("retry", mark)})) break;");
					Charge(start);
					Line($"p = {start};");
					PutBack(repeat.Resets, held);
					_indent--;
					Line("}");
				}
				else
					Attempt(repeat.Body, read);
				Line($"if (!{read})");
				Line("{");
				_indent++;
				Line($"p = {start};");
				PutBack(repeat.Resets, held);

				if (repeat.Resumable)
					Line($"if ({way} >= 0) {Runtime("next", way + ", 1")};");

				Line("break;");
				_indent--;
				Line("}");
				Line($"{turns}++;");
				Line($"if (p == {start}) break;");
				_indent--;
				Line("}");

				if (kept)
					Record(owner, end: true, "p", turns, $"{turns} >= {least}", repeat.Resets);

				Line($"if ({turns} < {least}) {fail}");
				_indent--;
				Line("}");

				break;
			}

			case IrStmt.Look look:
				Sealed(look.Body, look.Resets, fail, look);

				break;

			case IrStmt.Commit commit when !IrLowering.Decides(commit.Body) && commit.Resets.Count == 0:
				Statement(commit.Body, fail);

				break;

			case IrStmt.Commit commit:
				Sealed(commit.Body, commit.Resets, fail, null, commit.Defers);

				break;

			case IrStmt.Replay replay:
			{
				var start   = Temp("r");
				var segment = Temp("seg");
				var read    = Temp("ok");

				Line("{");
				_indent++;
				var u       = Temp("u");

				Line(Int(start, "p"));
				Line(Int(segment, Member("cursor")));
				Line(Flag(read));

				if (replay.Defers)
					Line(Int(u, _twin ? "u0" : "0"));

				if (_plain && (Places(_function!) || _ablation is SliceAblation.ReplayTwinEvery or SliceAblation.CeilingEvery or SliceAblation.ReplayTwinKeyed))
				{
					// One reading; any reading again is the twin's, from where this one began.
					Line($"p = {start};");
					Attempt(replay.Body, read);
					Line($"if (!{read})");
					Line("{");
					_indent++;
					Line($"if (!({Member("cursor")} > {segment} && {Runtime("retry", segment)})) {fail}");
					Charge(start);
					Line($"return {Name(_function!)}_r(P, {start}, {(replay.Defers ? Runtime("retried", u) : "0")});");
					_indent--;
					Line("}");
					_indent--;
					Line("}");

					break;
				}

				Line("for (;;)");
				Line("{");
				_indent++;
				Line($"p = {start};");
				Attempt(replay.Body, read);

				if (replay.Defers)
				{
					// The deferral of building as the generated parser keeps it: an attempt that stood
					// unbuilt is read once more, built; the second way given back stops building.
					Line($"if ({read})");
					Line("{");
					Line($"\tif ({u} > 1) {{ {u} = {Runtime("stood", $"{u}, {segment}")}; if ({u} > 0) {{ {(_budget ? $"RT_CHARGE(P, {start}); " : "")}{read} = false; continue; }} }}");
					Line("\tbreak;");
					Line("}");
					Line($"if (!({Member("cursor")} > {segment} && {Runtime("retry", segment)})) {{ if ({u} > 1) {Runtime("undeferred", u)}; break; }}");
					Charge(start);
					Line($"{u} = {Runtime("retried", u)};");
				}
				else
				{
					Line($"if ({read}) break;");
					Line($"if (!({Member("cursor")} > {segment} && {Runtime("retry", segment)})) break;");
					Charge(start);
				}

				_indent--;
				Line("}");
				Line($"if (!{read}) {fail}");
				_indent--;
				Line("}");

				break;
			}

			// A list taken whole is a copy; a reading that builds nothing takes none but what a guard reads.
			case IrStmt.Assign { Value: IrExpr.Items } assign when !_sites.ReadByGuards.Contains(assign.Slot):
				Line($"if ({Member("unbuilt")} == 0) {Slot(assign.Slot)} = {Expression(assign.Value)};");

				break;

			case IrStmt.Assign assign:
				Line($"{Slot(assign.Slot)} = {Expression(assign.Value)};");

				break;

			case IrStmt.Append append:
				Line(C
					? $"{Slot(append.List)} = list_add(P, {Slot(append.List)}, {Expression(append.Value)});"
					: $"({Slot(append.List)} ??= []).push({Expression(append.Value)});");

				break;

			case IrStmt.Guard guard:
			{
				var arguments = string.Join(", ", guard.Arguments.Select(Expression));

				Line(C
					? $"if (!host_{guard.Fragment.Name}(P{(arguments.Length > 0 ? ", " : "")}{arguments})) {{ {Refuse("p", guard.Expected)} {fail} }}"
					: $"if (!P.host.{guard.Fragment.Name}({arguments})) {{ {Refuse("p", guard.Expected)} {fail} }}");

				break;
			}

			case IrStmt.Construct build:
			{
				var arguments = string.Join(", ", build.Arguments.Select(Expression));

				Line(C
					? $"if (P->unbuilt == 0) {Slot(build.Into)} = host_{build.Fragment.Name}(P{(arguments.Length > 0 ? ", " : "")}{arguments});"
					: $"if (P.unbuilt == 0) {Slot(build.Into)} = P.host.{build.Fragment.Name}({arguments});");

				break;
			}

			default:
				throw new NotSupportedException("The slice does not print " + stmt.GetType().Name + ".");
		}
	}

	/// <summary>
	/// Braces, or a look where <paramref name="look"/> is given: the body's decisions written
	/// past everything on the tape, the body run again over them until it reads or they run out,
	/// and the tape as it was afterwards.
	/// </summary>
	void Sealed(IrStmt body, IReadOnlyList<IrSlot> resets, string fail, IrStmt.Look? look, bool defers = false)
	{
		var start  = Temp("l");
		var cursor = Temp("lc");
		var count  = Temp("ln");
		var read   = Temp("ok");
		var u      = Temp("u");

		Line("{");
		_indent++;
		Line(Int(start, "p"));
		Line(Int(cursor, Member("cursor")));
		Line(Int(count, Member("count")));
		Line(Flag(read));

		if (defers)
			Line(Int(u, "0"));

		var kept = Keep(resets);

		Line($"{Member("cursor")} = {count};");

		if (look is not null)
		{
			Line($"{Member("quiet")}++;");
			Line($"{Member("unbuilt")}++;");
		}

		Line("for (;;)");
		Line("{");
		_indent++;
		Line($"p = {start};");
		Attempt(body, read);

		if (defers)
		{
			// An attempt that stood unbuilt is read once more, built; any other that read ends the loop.
			Line($"if ({read})");
			Line("{");
			_indent++;
			Line($"if ({u} > 1) {u} = {Runtime("stood", $"{u}, {count}")};");
			Line($"if ({u} < 2) break;");
			Line($"{read} = false;");
			PutBack(resets, kept);
			Charge(start);
			Line("continue;");
			_indent--;
			Line("}");
			PutBack(resets, kept);
			Line($"if (!({Member("count")} > {count} && {Runtime("retry", count)})) {{ if ({u} > 1) {Runtime("undeferred", u)}; break; }}");
			Charge(start);
			Line($"{u} = {Runtime("retried", u)};");
		}
		else
		{
			Line($"if ({read}) break;");
			PutBack(resets, kept);
			Line($"if (!({Member("count")} > {count} && {Runtime("retry", count)})) break;");
			Charge(start);
		}

		_indent--;
		Line("}");

		if (look is not null)
		{
			Line($"{Member("quiet")}--;");
			Line($"{Member("unbuilt")}--;");
		}

		Line($"{Member("count")} = {count};");
		Line($"{Member("cursor")} = {cursor};");

		if (_resumes)
			Line($"if ({Member("kCount")} > 0) {Runtime("cut", "")};");

		if (look is not null)
		{
			Line($"p = {start};");
			PutBack(resets, kept);
			Line($"if ({read} != {(look.IsPositive ? "true" : "false")}) {{ {Refuse("p", look.Expected)} {fail} }}");
		}
		else
			Line($"if (!{read}) {fail}");

		_indent--;
		Line("}");
	}

	/// <summary>Restart by budget: a region read again from <paramref name="start"/>, charged.</summary>
	void Charge(string start)
	{
		if (_budget)
			Line($"RT_CHARGE(P, {start});");
	}

	// ---- Frames of the parser's own ----

	string FrameNumber()
	{
		return "I[b + " + (_numbers++).ToString(CultureInfo.InvariantCulture) + "]";
	}

	string FrameValue()
	{
		return "V[v + " + (_values++).ToString(CultureInfo.InvariantCulture) + "]";
	}

	int State()
	{
		return _states++;
	}

	/// <summary>The state begins here; what stands before it runs on into it.</summary>
	void Place(int state)
	{
		_indent--;
		Line($"case {state.ToString(CultureInfo.InvariantCulture)}:");
		_indent++;
	}

	static string Jump(int state)
	{
		return $"{{ st = {state.ToString(CultureInfo.InvariantCulture)}; continue; }}";
	}

	/// <summary>A failure: the state that answers for it, or the machine's own, which gives its frame back.</summary>
	static string FailTo(int state)
	{
		return state < 0 ? "{ P.top = b; P.valueTop = v; return -1; }" : Jump(state);
	}

	/// <summary>Whether the statement holds a call that leaves the machine, so that it has to be cut into states.</summary>
	bool Leaves(IrStmt stmt)
	{
		return stmt is IrStmt.Call call && Leaves(call) || stmt.Children.Any(Leaves);
	}

	bool Leaves(IrStmt.Call call)
	{
		return _frames == SliceFrames.All || _reach[call.Target].Contains(_machine!);
	}

	string Sizes(IrFunction function)
	{
		var index = function.Index.ToString(CultureInfo.InvariantCulture);

		return $"@numbers{index}@, @values{index}@";
	}

	/// <summary>
	/// A function with a frame of its own. Of the numbers the first is the state to go on from
	/// and the last says whose frame it is; slots and what is kept across a call lie between.
	/// </summary>
	void Machine(IrFunction function)
	{
		_temp    = 0;
		_numbers = 1;
		_values  = 0;
		_states  = 1;
		_machine = function;
		_places  = [];

		foreach (var slot in Slots(function.Body).Distinct().OrderBy(static slot => slot.Index))
			_places.Add(slot, slot.Kind is IrSlotKind.Position or IrSlotKind.Rebound ? FrameNumber() : FrameValue());

		var index = function.Index.ToString(CultureInfo.InvariantCulture);

		Line($"function {Name(function)}_m(P: Runtime, p: number): number");
		Line("{");
		_indent++;
		Line("const text = P.text;");
		Line("const len = P.len;");
		Line("const V = P.V;");
		Line($"const b = P.top - @numbers{index}@;");
		Line($"const v = P.valueTop - @values{index}@;");
		Line("let I = P.I;");
		Line("let c = 0;");
		Line("let st = I[b];");

		if (_counted)
			Line("if (st == 0) P.kSteps++;");

		foreach (var (slot, place) in _places.OrderBy(static pair => pair.Key.Index))
			Line($"// {place}: {slot.Name}{(slot.Type is null ? "" : " : " + slot.Type)}");

		Line();
		Line("for (;;)");
		Line("{");
		_indent++;
		Line("switch (st)");
		Line("{");
		_indent += 2;
		Place(0);
		Flat(function.Body, -1);

		if (function.Result is { } result)
			Line($"P.rv = {Slot(result)};");

		Line("void c; void len; void text; void V; void v;");
		Line("P.top = b;");
		Line("P.valueTop = v;");
		Line("return p;");
		_indent -= 2;
		Line("}");
		_indent--;
		Line("}");
		_indent--;
		Line("}");
		Line();

		// For a caller that is not on a cycle with it: a call of the language, which runs the machines until this one has answered.
		Line($"function {Name(function)}(P: Runtime, p: number): number");
		Line("{");
		Line($"\treturn run(P, {index}, {Sizes(function)}, p);");
		Line("}");
		Line();

		_sizes.Add(function, (_numbers + 1, _values));

		_machine = null;
		_places  = null;
	}

	/// <summary>
	/// A statement of a machine. One that holds no call out of the machine is printed as ever,
	/// its failure a jump; any other is laid out as states, with what has to outlive the call in
	/// the frame.
	/// </summary>
	/// <param name="fail">The state a failure of this statement goes to; the machine's own failure where negative.</param>
	void Flat(IrStmt stmt, int fail)
	{
		if (!Leaves(stmt))
		{
			Statement(stmt, FailTo(fail));

			return;
		}

		if (SentinelAlone(stmt))
		{
			var outer = _bare;

			_bare = stmt;
			Flat(stmt, fail);
			_bare = outer;

			return;
		}

		if (stmt is not IrStmt.Repeat && Kept(stmt))
		{
			FlatSite(stmt, fail);

			return;
		}

		switch (stmt)
		{
			case IrStmt.Block block:
				foreach (var item in block.Items)
					Flat(item, fail);

				break;

			case IrStmt.Scope scope:
				foreach (var slot in scope.Slots)
					Line($"{Slot(slot)} = {Nothing(slot)};");

				Flat(scope.Body, fail);

				break;

			case IrStmt.Call call:
			{
				var back = State();

				// The callee's answer comes back as p; the position the call was made at is not kept,
				// because whatever takes a failure puts its own back.
				Line($"I[b] = {back.ToString(CultureInfo.InvariantCulture)};");
				Line($"P.call({call.Target.Index.ToString(CultureInfo.InvariantCulture)}, {Sizes(call.Target)});");
				Line("return p;");
				Place(back);
				Line($"if (p < 0) {FailTo(fail)}");

				if (call.Into is { } into)
				{
					Line(call.Appends
						? $"if (P.unbuilt == 0) ({Slot(into)} ??= []).push(P.rv);"
						: $"if (P.unbuilt == 0) {Slot(into)} = P.rv;");
				}

				break;
			}

			case IrStmt.Choice choice:
			{
				var start = FrameNumber();
				var way   = choice.Resumable ? FrameNumber() : "";
				var done  = State();
				var last  = choice.Alternatives.Count - 1;

				Line($"{start} = p;");

				if (choice.Resumable)
					Line($"{way} = {Enter(last.ToString(CultureInfo.InvariantCulture))};");

				for (var i = 0; i <= last; i++)
				{
					var index = i.ToString(CultureInfo.InvariantCulture);
					var after = State();

					if (choice.Resumable)
						Line($"if ({Decision(way)} > {index}) {Jump(after)}");

					if (Leaves(choice.Alternatives[i]))
					{
						var failed = State();
						var kept   = Keep(choice.Resets[i], inFrame: true);
						var retry  = choice.Resumable && i < last;
						var mark   = retry ? FrameNumber() : "";
						var begin  = retry ? State() : -1;

						if (retry)
						{
							Line($"{mark} = P.cursor;");
							Place(begin);
						}

						Flat(choice.Alternatives[i], failed);
						Line(Jump(done));
						Place(failed);
						Line($"p = {start};");
						PutBack(choice.Resets[i], kept);

						if (retry)
						{
							Line($"if (P.cursor > {mark} && {Runtime("retry", mark)}) {Jump(begin)}");
							Line(Runtime("next", $"{way}, {(i + 1).ToString(CultureInfo.InvariantCulture)}") + ";");
						}
					}
					else
					{
						var read = Temp("ok");

						Line("{");
						_indent++;
						Line(Flag(read));

						var kept = Keep(choice.Resets[i]);

						if (choice.Resumable && i < last)
						{
							var mark = Temp("m");

							Line(Int(mark, "P.cursor"));
							Line("for (;;)");
							Line("{");
							_indent++;
							Attempt(choice.Alternatives[i], read);
							Line($"if ({read} || !(P.cursor > {mark} && {Runtime("retry", mark)})) break;");
							Line($"p = {start};");
							PutBack(choice.Resets[i], kept);
							_indent--;
							Line("}");
						}
						else
							Attempt(choice.Alternatives[i], read);

						Line($"if ({read}) {Jump(done)}");
						Line($"p = {start};");
						PutBack(choice.Resets[i], kept);

						if (choice.Resumable && i < last)
							Line(Runtime("next", $"{way}, {(i + 1).ToString(CultureInfo.InvariantCulture)}") + ";");

						_indent--;
						Line("}");
					}

					Place(after);
				}

				Line(FailTo(fail));
				Place(done);

				break;
			}

			case IrStmt.Repeat repeat:
			{
				var turns  = FrameNumber();
				var least  = FrameNumber();
				var most   = FrameNumber();
				var start  = FrameNumber();
				var way    = repeat.Resumable ? FrameNumber() : "";
				var again  = State();
				var failed = State();
				var done   = State();

				var keeps = Kept(repeat);
				var owner = keeps ? FrameNumber() : "";

				Line($"{turns} = 0;");
				Line($"{least} = {Expression(repeat.Min)};");
				Line($"{most} = {(repeat.Max is null ? "2147483647" : Expression(repeat.Max))};");

				if (keeps)
				{
					Line($"{owner} = -1;");
					Sentinel(repeat, owner, h =>
					{
						Line($"p = {Field(h, "pos")};");
						Line($"{turns} = {Field(h, "turns")};");
						Line($"{Member("cursor")} = {Field(h, "key")};");
						Give(repeat.Resets, h);
						Jumped();
						Line($"if (({Field(h, "flags")} & 1) != 0) {Jump(done)}");
					}, endOnly: false);
				}

				Place(again);
				Step();
				Line($"if ({turns} >= {most}) {Jump(done)}");
				Line($"{start} = p;");

				if (keeps)
					Record(owner, end: false, start, turns, "false", repeat.Resets);

				if (repeat.Resumable)
				{
					Line($"{way} = -1;");
					Line($"if ({turns} >= {least})");
					Line("{");
					Line($"\t{way} = {Enter("1")};");
					Line($"\tif ({Decision(way)} == 1) {Jump(done)}");
					Line("}");
				}

				var kept  = Keep(repeat.Resets, inFrame: true);
				var mark  = repeat.Resumable ? FrameNumber() : "";
				var begin = repeat.Resumable ? State() : -1;

				if (repeat.Resumable)
				{
					Line($"{mark} = P.cursor;");
					Place(begin);
				}

				Flat(repeat.Body, failed);
				Line($"{turns}++;");
				Line($"if (p == {start}) {Jump(done)}");
				Line(Jump(again));
				Place(failed);
				Line($"p = {start};");
				PutBack(repeat.Resets, kept);

				if (repeat.Resumable)
				{
					Line($"if ({way} >= 0 && P.cursor > {mark} && {Runtime("retry", mark)}) {Jump(begin)}");
					Line($"if ({way} >= 0) {Runtime("next", way + ", 1")};");
				}

				Place(done);

				if (keeps)
					Record(owner, end: true, "p", turns, $"{turns} >= {least}", repeat.Resets);

				Line($"if ({turns} < {least}) {FailTo(fail)}");

				break;
			}

			case IrStmt.Look look:
				FlatSealed(look.Body, look.Resets, fail, look);

				break;

			case IrStmt.Commit commit when !IrLowering.Decides(commit.Body) && commit.Resets.Count == 0:
				Flat(commit.Body, fail);

				break;

			case IrStmt.Commit commit:
				FlatSealed(commit.Body, commit.Resets, fail, null, commit.Defers);

				break;

			case IrStmt.Replay replay:
			{
				var start   = FrameNumber();
				var segment = FrameNumber();
				var again   = State();
				var failed  = State();
				var done    = State();

				var u = replay.Defers ? FrameNumber() : "";

				Line($"{start} = p;");
				Line($"{segment} = P.cursor;");

				if (replay.Defers)
					Line($"{u} = 0;");

				Place(again);
				Line($"p = {start};");
				Flat(replay.Body, failed);

				if (replay.Defers)
					Line($"if ({u} > 1) {{ {u} = P.stood({u}, {segment}); if ({u} > 0) {Jump(again)} }}");

				Line(Jump(done));
				Place(failed);

				if (replay.Defers)
				{
					Line($"if (P.cursor > {segment} && {Runtime("retry", segment)}) {{ {u} = P.retried({u}); {Jump(again)} }}");
					Line($"if ({u} > 1) P.undeferred({u});");
				}
				else
					Line($"if (P.cursor > {segment} && {Runtime("retry", segment)}) {Jump(again)}");

				Line(FailTo(fail));
				Place(done);

				break;
			}

			default:
				throw new NotSupportedException("The slice does not cut " + stmt.GetType().Name + " into states.");
		}
	}

	/// <summary><see cref="Sealed"/>, with a call out of the machine somewhere in the body.</summary>
	void FlatSealed(IrStmt body, IReadOnlyList<IrSlot> resets, int fail, IrStmt.Look? look, bool defers = false)
	{
		var start  = FrameNumber();
		var cursor = FrameNumber();
		var count  = FrameNumber();
		var read   = FrameNumber();
		var u      = defers ? FrameNumber() : "";
		var again  = State();
		var failed = State();
		var done   = State();

		Line($"{start} = p;");
		Line($"{cursor} = P.cursor;");
		Line($"{count} = P.count;");

		if (defers)
			Line($"{u} = 0;");

		var kept = Keep(resets, inFrame: true);

		Line($"P.cursor = {count};");

		if (look is not null)
		{
			Line("P.quiet++;");
			Line("P.unbuilt++;");
		}

		Place(again);
		Line($"p = {start};");
		Flat(body, failed);

		if (defers)
		{
			Line($"if ({u} > 1)");
			Line("{");
			_indent++;
			Line($"{u} = P.stood({u}, {count});");
			Line($"if ({u} > 0)");
			Line("{");
			_indent++;
			PutBack(resets, kept);
			Line(Jump(again));
			_indent--;
			Line("}");
			_indent--;
			Line("}");
		}

		Line($"{read} = 1;");
		Line(Jump(done));
		Place(failed);
		PutBack(resets, kept);

		if (defers)
		{
			Line($"if (P.count > {count} && {Runtime("retry", count)}) {{ {u} = P.retried({u}); {Jump(again)} }}");
			Line($"if ({u} > 1) P.undeferred({u});");
		}
		else
			Line($"if (P.count > {count} && {Runtime("retry", count)}) {Jump(again)}");

		Line($"{read} = 0;");
		Place(done);

		if (look is not null)
		{
			Line("P.quiet--;");
			Line("P.unbuilt--;");
		}

		Line($"P.count = {count};");
		Line($"P.cursor = {cursor};");

		if (_resumes)
			Line("if (P.kCount > 0) P.cut();");

		if (look is not null)
		{
			Line($"p = {start};");
			PutBack(resets, kept);
			Line($"if ({read} != {(look.IsPositive ? "1" : "0")}) {{ {Refuse("p", look.Expected)} {FailTo(fail)} }}");
		}
		else
			Line($"if ({read} == 0) {FailTo(fail)}");
	}

	string Literal(string text)
	{
		if (!_literals.TryGetValue(text, out var name))
			_literals.Add(text, name = "L" + _literals.Count.ToString(CultureInfo.InvariantCulture));

		return name;
	}
}
