using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;

// The first call of SQL:2023's parser, in phases, in a fresh process (from sql-39's `first`).
//
//     sqlfirst <directory with DotGram.Sql.dll> <entry point> <input>
//
// The entry point is a `TryParse` method of SqlStandardParser that takes a string. Each phase prints
// its time, how many methods the runtime compiled during it and the time it spent compiling (JitInfo;
// no events, so nothing to slow the phases down): load, the type initializer of the parser and then of
// each nested type that takes long enough to be worth naming, the first tokenize where the parser has
// one, the first parse, the second.

if (args.Length < 3)
{
	Console.Error.WriteLine("usage: sqlfirst <directory with DotGram.Sql.dll> <entry point> <input>");

	return 2;
}

var clock   = Stopwatch.StartNew();
var last    = 0L;
var methods = JitInfo.GetCompiledMethodCount();
var jitAt   = JitInfo.GetCompilationTime();

var assembly = Assembly.LoadFrom(Path.Combine(args[0], "DotGram.Sql.dll"));

Phase("load");

var type = assembly.GetType("DotGram.Sql.Standard.SqlStandardParser")!;

RuntimeHelpers.RunClassConstructor(type.TypeHandle);
Phase("cctor " + type.Name);

foreach (var nested in type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic).Where(static one => !one.ContainsGenericParameters))
{
	RuntimeHelpers.RunClassConstructor(nested.TypeHandle);

	// A nested type whose initializer is nothing is not a line of the report, and not a phase.
	if ((clock.ElapsedTicks - last) * 1000.0 / Stopwatch.Frequency > 0.5)
		Phase("cctor " + nested.Name);
	else
		Reset();
}

var tokenize = type.GetMethod("Tokenize_DotGram", BindingFlags.NonPublic | BindingFlags.Static, [typeof(string)]);

if (tokenize is not null)
{
	tokenize.Invoke(null, [args[2]]);
	Phase("first tokenize");
}

var parse = type.GetMethod(args[1], [typeof(string)])!;
var call  = () => parse.Invoke(null, [args[2]])!;

var match = call();

Phase("first parse");
call();
Phase("second parse");
Console.WriteLine((bool)match.GetType().GetProperty("IsSuccess")!.GetValue(match)! ? "read" : "REFUSED");

return 0;

void Phase(string name)
{
	var now       = clock.ElapsedTicks;
	var compiled  = JitInfo.GetCompiledMethodCount();
	var compiling = JitInfo.GetCompilationTime();

	Console.WriteLine($"{name,-22} {(now - last) * 1000.0 / Stopwatch.Frequency,8:0.00} ms   jit {compiled - methods,5} methods {(compiling - jitAt).TotalMilliseconds,8:0.00} ms");
	Reset();
}

void Reset()
{
	last    = clock.ElapsedTicks;
	methods = JitInfo.GetCompiledMethodCount();
	jitAt   = JitInfo.GetCompilationTime();
}
