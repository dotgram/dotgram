using System;
using System.Globalization;
using System.Text;

using DotGram.Grammar.Model;

namespace DotGram.Ir.Prototype;

enum SliceLanguage
{
	TypeScript,
	C,
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
/// over a stack — not done here. How deep an input may nest is therefore the language's stack.
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

	int _indent;
	int _temp;

	SlicePrinter(IrProgram program, SliceLanguage language)
	{
		_program  = program;
		_language = language;
	}

	bool C => _language == SliceLanguage.C;

	/// <summary>The parser's source: one file, which its language's runtime file stands beside.</summary>
	public static string Print(IrProgram program, SliceLanguage language)
	{
		var printer = new SlicePrinter(program, language);

		printer.Run();

		return printer._text.ToString();
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
			Line("#include \"runtime.h\"");
			Line("#include \"parser.h\"");
			Line();

			foreach (var function in _program.Functions)
				Line($"static int32_t {Name(function)}(Parser *P, int32_t p);");
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
			Function(function);

		// The texts the functions compare the input with, which C wants as arrays before them.
		if (C && _literals.Count > 0)
		{
			var tables = new StringBuilder();

			foreach (var (text, name) in _literals)
				tables.Append("static const uint16_t ").Append(name).Append("[] = { ").Append(string.Join(", ", text.Select(static unit => ((int)unit).ToString(CultureInfo.InvariantCulture)))).Append(" };\n");

			_text.Insert(body, tables.Append('\n').ToString());
		}

		if (C)
		{
			Line("int32_t parse(Parser *P, const char *entry)");
			Line("{");

			foreach (var (entry, function) in _program.Entries)
				Line($"\tif (strcmp(entry, \"{entry}\") == 0) return {Name(function)}(P, 0);");

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

	string Decision(string way)
	{
		return C ? $"P->ways[{way} * 2]" : $"P.ways[{way} * 2]";
	}

	static string Slot(IrSlot slot)
	{
		return "s" + slot.Index.ToString(CultureInfo.InvariantCulture);
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

	void Function(IrFunction function)
	{
		_temp = 0;

		Line(C ? $"static int32_t {Name(function)}(Parser *P, int32_t p)" : $"function {Name(function)}(P: Runtime, p: number): number");
		Line("{");
		_indent++;

		if (!C)
			Line("const text = P.text;");

		Line(C ? "const int32_t len = P->len;" : "const len = P.len;");
		Line(C ? "uint16_t c = 0;" : "let c = 0;");

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
	string[] Keep(IReadOnlyList<IrSlot> slots)
	{
		var kept = new string[slots.Count];

		for (var i = 0; i < kept.Length; i++)
		{
			var slot = slots[i];

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
				Line(Int(end, C ? $"{Name(call.Target)}(P, p)" : $"{Name(call.Target)}(P, p)"));
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
					Line(Int(way, Runtime("enter", last.ToString(CultureInfo.InvariantCulture))));

				for (var i = 0; i <= last; i++)
				{
					var index = i.ToString(CultureInfo.InvariantCulture);

					Line(choice.Resumable ? $"if (!{read} && {Decision(way)} <= {index})" : $"if (!{read})");
					Line("{");
					_indent++;

					var kept = Keep(choice.Resets[i]);

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
			case IrStmt.Repeat { Body: IrStmt.MatchSet set, Resumable: false, Resets.Count: 0, Min: IrExpr.Number min, Max: null or IrExpr.Number } run:
			{
				var turns = Temp("n");
				var most  = run.Max is IrExpr.Number max ? max.Value.ToString(CultureInfo.InvariantCulture) : "2147483647";

				Line("{");
				_indent++;
				Line(Int(turns, "0"));
				Line($"while ({turns} < {most})");
				Line("{");
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

				Line("{");
				_indent++;
				Line(Int(turns, "0"));
				Line(Int(least, Expression(repeat.Min)));
				Line(Int(most, repeat.Max is null ? "2147483647" : Expression(repeat.Max)));
				Line($"while ({turns} < {most})");
				Line("{");
				_indent++;
				Line(Int(start, "p"));
				Line(Flag(read));

				if (repeat.Resumable)
				{
					Line(Int(way, "-1"));
					Line($"if ({turns} >= {least})");
					Line("{");
					Line($"\t{way} = {Runtime("enter", "1")};");
					Line($"\tif ({Decision(way)} == 1) break;");
					Line("}");
				}

				var kept = Keep(repeat.Resets);

				Attempt(repeat.Body, read);
				Line($"if (!{read})");
				Line("{");
				_indent++;
				Line($"p = {start};");
				PutBack(repeat.Resets, kept);

				if (repeat.Resumable)
					Line($"if ({way} >= 0) {Runtime("next", way + ", 1")};");

				Line("break;");
				_indent--;
				Line("}");
				Line($"{turns}++;");
				Line($"if (p == {start}) break;");
				_indent--;
				Line("}");
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
				Sealed(commit.Body, commit.Resets, fail, null);

				break;

			case IrStmt.Replay replay:
			{
				var start   = Temp("r");
				var segment = Temp("seg");
				var read    = Temp("ok");

				Line("{");
				_indent++;
				Line(Int(start, "p"));
				Line(Int(segment, Member("cursor")));
				Line(Flag(read));
				Line("for (;;)");
				Line("{");
				_indent++;
				Line($"p = {start};");
				Attempt(replay.Body, read);
				Line($"if ({read}) break;");
				Line($"if (!({Member("cursor")} > {segment} && {Runtime("retry", segment)})) break;");
				_indent--;
				Line("}");
				Line($"if (!{read}) {fail}");
				_indent--;
				Line("}");

				break;
			}

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
	void Sealed(IrStmt body, IReadOnlyList<IrSlot> resets, string fail, IrStmt.Look? look)
	{
		var start  = Temp("l");
		var cursor = Temp("lc");
		var count  = Temp("ln");
		var read   = Temp("ok");

		Line("{");
		_indent++;
		Line(Int(start, "p"));
		Line(Int(cursor, Member("cursor")));
		Line(Int(count, Member("count")));
		Line(Flag(read));

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
		Line($"if ({read}) break;");
		PutBack(resets, kept);
		Line($"if (!({Member("count")} > {count} && {Runtime("retry", count)})) break;");
		_indent--;
		Line("}");

		if (look is not null)
		{
			Line($"{Member("quiet")}--;");
			Line($"{Member("unbuilt")}--;");
		}

		Line($"{Member("count")} = {count};");
		Line($"{Member("cursor")} = {cursor};");

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

	string Literal(string text)
	{
		if (!_literals.TryGetValue(text, out var name))
			_literals.Add(text, name = "L" + _literals.Count.ToString(CultureInfo.InvariantCulture));

		return name;
	}
}
