using System.Collections;
using System.Reflection;
using System.Text;

// args: dll corpusDir outFile [maxTexts]
// Loads one DotGram.Sql.dll (one per process), reads the corpus scripts, collects guest-shaped
// subtree texts and writes, per input and entry form, the outcome, the error and the tree written back.
var asm = Assembly.LoadFrom(args[0]);
var corpus = Directory.GetFiles(args[1], "*.sql", SearchOption.AllDirectories).OrderBy(f => f, StringComparer.Ordinal)
	.Select(File.ReadAllText).Where(t => t.Length < 200_000).ToArray();
int maxTexts = args.Length > 3 ? int.Parse(args[3]) : 20000;
var output = new StreamWriter(args[2]) { NewLine = "\n" };

var tsql = asm.GetType("DotGram.Sql.TransactSql.TransactSqlParser")!;
var tsqlLocated = tsql.GetNestedType("Located")!;
var std = asm.GetType("DotGram.Sql.Standard.SqlStandardParser")!;
var writer = asm.GetType("DotGram.Sql.SqlWriter")!;
var stdWriter = asm.GetType("DotGram.Sql.Ast.Sql2023Writer")!;
var iSpan = asm.GetType("DotGram.Sql.ISqlSpan")!;
var iNode = asm.GetType("DotGram.Sql.Ast.ISqlNode")!;
var walker = asm.GetType("DotGram.Sql.SqlWalker")!;
var walk = walker.GetMethod("Walk")!;
string[] tsqlRoots = ["Statement", "Query", "Expression", "TableReference", "Clause"];
var writes = tsqlRoots.Select(n => asm.GetType("DotGram.Sql." + n)!).Select(t => (t, writer.GetMethod("Write", [t])!)).ToArray();
var stdWrite = stdWriter.GetMethod("Write", [iNode])!;

string WriteValue(object? v)
{
	try { return WriteValueCore(v); }
	catch (TargetInvocationException e) { return "UNWRITABLE " + e.InnerException!.GetType().Name; }
}
string WriteValueCore(object? v)
{
	if (v is null) return "null";
	foreach (var (t, m) in writes) if (t.IsInstanceOfType(v)) return (string)m.Invoke(null, [v])!;
	if (iNode.IsInstanceOfType(v)) return (string)stdWrite.Invoke(null, [v])!;
	if (v is IEnumerable e && v is not string) { var sb = new StringBuilder("["); foreach (var x in e) sb.Append(WriteValue(x)).Append(";\n"); return sb.Append(']').ToString(); }
	return v.ToString() ?? "";
}
string Spans(object? v)
{
	if (v is null || !iSpan.IsInstanceOfType(v)) return "";
	var sb = new StringBuilder(" spans:");
	Func<object, bool> visit = n => { sb.Append(' ').Append(iSpan.GetProperty("Span")!.GetValue(n)); return true; };
	walk.Invoke(null, [v, visit]);
	return sb.ToString();
}
string Show(object match, bool located)
{
	var t = match.GetType();
	var outcome = t.GetProperty("Outcome")!.GetValue(match)!.ToString();
	var sb = new StringBuilder(outcome);
	var end = t.GetProperty("ReadingEnd");
	if (end is not null) sb.Append(" end=").Append(end.GetValue(match));
	if (outcome == "Success")
	{
		var v = t.GetProperty("Value")!.GetValue(match);
		sb.Append(' ').Append(WriteValue(v));
		if (located) sb.Append(Spans(v));
	}
	else sb.Append(' ').Append(t.GetProperty("Error")!.GetValue(match));
	return sb.ToString().Replace("\n", "\\n");
}
object? Call(MethodInfo m, object[] a)
{
	try { return m.Invoke(null, a); }
	catch (TargetInvocationException e) { return "THROW " + e.InnerException!.GetType().Name + ": " + e.InnerException.Message; }
}
string Res(object? r, bool located) => r is string s ? s : Show(r!, located);

MethodInfo? Form(Type owner, string name, params Type[] ps) => owner.GetMethod(name, ps);

// 1. Scripts as before: T-SQL TryParseScript whole/60%, SQL:2023 statement.
var tryScript = Form(tsql, "TryParseScript", typeof(string))!;
var stdStatement = Form(std, "TryParseDirectSQLStatement", typeof(string))!;
var tsqlStatement = Form(tsql, "TryParseStatement", typeof(string))!;
var tsqlTexts = new List<(string Kind, string Text)>();
var stdTexts = new List<(string Kind, string Text)>();
for (int i = 0; i < corpus.Length; i++)
{
	var s = corpus[i];
	var m = tryScript.Invoke(null, [s])!;
	output.WriteLine($"script {i}: {Show(m, false)}");
	output.WriteLine($"script60 {i}: {Res(Call(tryScript, [s.Substring(0, s.Length * 3 / 5)]), false)}");
	var sm = stdStatement.Invoke(null, [s])!;
	output.WriteLine($"std {i}: {Show(sm, false)}");
	// Subtrees of accepted trees, written back.
	if (m.GetType().GetProperty("Outcome")!.GetValue(m)!.ToString() == "Success")
		foreach (var b in (IEnumerable)m.GetType().GetProperty("Value")!.GetValue(m)!)
			foreach (var st in (IEnumerable)b.GetType().GetProperty("Statements")!.GetValue(b)!)
				walk.Invoke(null, [st, (Func<object, bool>)(n =>
				{
					var tn = n.GetType();
					string? kind = null;
					for (var bt = tn; bt is not null; bt = bt.BaseType) if (bt.Namespace == "DotGram.Sql" && tsqlRoots.Contains(bt.Name)) { kind = bt.Name; break; }
					if (kind is "Expression" or "Query" || kind == "Statement" && tn.Name == "Select") { var w = WriteValue(n); if (!w.StartsWith("UNWRITABLE")) tsqlTexts.Add((kind!, w)); }
					return true;
				})]);
	if (sm.GetType().GetProperty("Outcome")!.GetValue(sm)!.ToString() == "Success")
		walk.Invoke(null, [sm.GetType().GetProperty("Value")!.GetValue(sm)!, (Func<object, bool>)(n =>
		{
			if (iNode.IsInstanceOfType(n)) { var w = WriteValue(n); if (!w.StartsWith("UNWRITABLE")) stdTexts.Add((n.GetType().Name, w)); }
			return true;
		})]);
}
List<string> Pick(IEnumerable<string> all, int max)
{
	var d = all.Where(t => t.Length > 0).Distinct(StringComparer.Ordinal).OrderBy(t => t, StringComparer.Ordinal).ToList();
	if (d.Count <= max) return d;
	var step = (double)d.Count / max;
	return Enumerable.Range(0, max).Select(k => d[(int)(k * step)]).ToList();
}
var tsqlPool = Pick(tsqlTexts.Select(t => t.Text), maxTexts);
var stdPool = Pick(stdTexts.Select(t => t.Text).Concat(tsqlTexts.Where(t => t.Kind == "Expression").Select(t => t.Text)), maxTexts);
Console.WriteLine($"corpus {corpus.Length}; tsql subtree texts {tsqlTexts.Count} -> {tsqlPool.Count}; std {stdTexts.Count} -> {stdPool.Count}");

// 2. Guest entries, every form.
void Guest(Type owner, string name, List<string> pool, bool located)
{
	var whole = Form(owner, name, typeof(string));
	if (whole is null) { output.WriteLine($"MISSING {owner.Name}.{name}"); return; }
	var at = Form(owner, name, typeof(string), typeof(int));
	var window = Form(owner, name, typeof(string), typeof(int), typeof(int));
	var label = owner.Name + "." + name;
	foreach (var t in pool)
	{
		output.WriteLine($"{label} whole [{t}]: {Res(Call(whole, [t]), located)}");
		output.WriteLine($"{label} 60 [{t}]: {Res(Call(whole, [t.Substring(0, t.Length * 3 / 5)]), located)}");
		if (at is not null) output.WriteLine($"{label} at: {Res(Call(at, ["SELECT 1; " + t, 10]), located)}");
		if (window is not null) output.WriteLine($"{label} window: {Res(Call(window, ["( " + t + " ) x", 2, t.Length]), located)}");
	}
}
foreach (var n in new[] { "TryParseSearchCondition", "TryParseValueExpression", "TryParseQuery", "TryParseSelect" })
{
	Guest(tsql, n, tsqlPool, false);
	Guest(tsqlLocated, n, tsqlPool, true);
}
// The SQL:2023 entries: every publication of the parser except the statement host's, read off the type.
var stdNames = std.GetMethods(BindingFlags.Public | BindingFlags.Static).Where(m => m.Name.StartsWith("TryParse") && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(string))
	.Select(m => m.Name).Distinct().OrderBy(n => n, StringComparer.Ordinal).ToList();
Console.WriteLine("std entries: " + string.Join(", ", stdNames));
foreach (var n in stdNames)
	Guest(std, n, stdPool, false);
output.Flush();
Console.WriteLine("done");
