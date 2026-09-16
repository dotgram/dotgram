using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Security.Cryptography;

namespace DotGram.PrefixBenchmarks;

static class Program
{
	sealed record Case(string Name, Func<string, int> Default, Func<string, int> Tables, string Input, int Expected, int Iterations);

	static void Main(string[] args)
	{
		if (args.Length != 3) throw new ArgumentException("Usage: <default-web-dll> <tables-web-dll> <round>");
		var round = int.Parse(args[2], CultureInfo.InvariantCulture);
		Console.WriteLine("# TieredCompilation=" + (Environment.GetEnvironmentVariable("DOTNET_TieredCompilation") ?? "default"));
		var cases = new List<Case>();
		var own = typeof(Program).Assembly;
		void Small(string type, string name, string input, int expected) => cases.Add(new Case(type + "/" + name,
			Bind(own.GetType("DotGram.PrefixBenchmarks." + type + "Default")!, "TryParseStart", false),
			Bind(own.GetType("DotGram.PrefixBenchmarks." + type + "Tables")!, "TryParseStart", false), input, expected, 500000));
		Small("Methods8", "early", "GET", 0);
		Small("Methods8", "late", "TRACE", 7);
		Small("Methods8", "near-invalid", "TRACX", -1);
		Small("Methods8", "truncated", "TRAC", -1);
		Small("Methods8", "invalid-first", "?", -1);
		foreach (var count in new[] { 4, 8, 16, 32, 64 })
		{
			Small("Tags" + count, "early", "X00=a", 0);
			Small("Tags" + count, "middle", $"X{count / 2:D2}=a", count / 2);
			Small("Tags" + count, "late", $"X{count - 1:D2}=a", count - 1);
			Small("Tags" + count, "unknown", "X99=a", -1);
			Small("Tags" + count, "bad-value", "X00=1", -1);
			Small("Tags" + count, "truncated", "X00", -1);
		}
		var old = new AssemblyLoadContext("Default Web").LoadFromAssemblyPath(Path.GetFullPath(args[0]));
		var tables = new AssemblyLoadContext("Table Web").LoadFromAssemblyPath(Path.GetFullPath(args[1]));
		VerifyWebOptions(old, false);
		VerifyWebOptions(tables, true);
		foreach (var type in own.GetTypes().Where(type => type.Name.EndsWith("Tables", StringComparison.Ordinal)))
			if (!type.GetFields(BindingFlags.Static | BindingFlags.NonPublic).Any(field => field.Name.StartsWith("Prefix_DotGram", StringComparison.Ordinal)))
				throw new InvalidOperationException("No prefix table emitted for " + type.Name);
		for (var i = 0; i < 2; i++) Console.WriteLine($"# Web {i} SHA256 {Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(args[i])))}");
		void Web(string type, string name, string input, bool expected) => cases.Add(new Case("Web/" + type + "/" + name,
			Bind(old.GetType("DotGram.Web." + type)!, "TryParse", true),
			Bind(tables.GetType("DotGram.Web." + type)!, "TryParse", true), input, expected ? 1 : 0, 100000));
		Web("UriReference", "short", "https://example.org/a?b=c", true);
		Web("UriReference", "long", "https://example.org/" + string.Concat(Enumerable.Repeat("part/", 100)) + "?x=y", true);
		Web("UriReference", "invalid", "https://example.org/%zz", false);
		Web("JsonValue", "short", "{\"a\":true}", true);
		Web("JsonValue", "long", "[" + string.Join(",", Enumerable.Repeat("{\"a\":true,\"b\":42}", 32)) + "]", true);
		Web("JsonValue", "invalid", "{\"a\":truX}", false);
		Web("MediaType", "short", "text/plain", true);
		Web("MediaType", "long", "application/json; charset=utf-8; profile=\"https://example.org/profile\"", true);
		Web("MediaType", "invalid", "text/", false);
		Web("Timestamp", "short", "2026-09-16T12:34:56Z", true);
		Web("Timestamp", "invalid", "2026-19-16T12:34:56Z", false);
		Web("LanguageTag", "short", "en-US", true);
		Web("LanguageTag", "long", "zh-cmn-Hans-CN-u-ca-chinese-x-private", true);
		Web("LanguageTag", "invalid", "en--US", false);
		foreach (var item in cases)
			if (item.Default(item.Input) != item.Expected || item.Tables(item.Input) != item.Expected)
				throw new InvalidOperationException("Unexpected recognition: " + item.Name);
		if (round < 0) { Console.WriteLine("# All input expectations and strategy options verified"); return; }
		Console.WriteLine("Case,Round,Variant,Iterations,NsPerOp,BytesPerOp");
		foreach (var item in cases)
		{
			Measure(item.Default, item.Input, 20000);
			Measure(item.Tables, item.Input, 20000);
			foreach (var enabled in round % 2 == 0 ? new[] { false, true } : new[] { true, false })
			{
				var parse = enabled ? item.Tables : item.Default;
				GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
				var allocated = GC.GetAllocatedBytesForCurrentThread();
				var start = Stopwatch.GetTimestamp();
				var result = Measure(parse, item.Input, item.Iterations);
				var elapsed = Stopwatch.GetElapsedTime(start);
				allocated = GC.GetAllocatedBytesForCurrentThread() - allocated;
				if (result != (long)item.Expected * item.Iterations) throw new InvalidOperationException(item.Name);
				Console.WriteLine(FormattableString.Invariant($"{item.Name},{round},{(enabled ? "tables" : "default")},{item.Iterations},{elapsed.TotalNanoseconds / item.Iterations:F3},{(double)allocated / item.Iterations:F3}"));
			}
		}
	}

	static void VerifyWebOptions(Assembly assembly, bool expected)
	{
		var count = 0;
		foreach (var type in assembly.GetTypes())
		foreach (var attribute in type.GetCustomAttributesData().Where(attribute => attribute.AttributeType.FullName == "DotGram.GramAttribute"))
		{
			var actual = attribute.NamedArguments.FirstOrDefault(argument => argument.MemberName == "PrefixTables").TypedValue.Value as bool? ?? false;
			if (actual != expected) throw new InvalidOperationException("Unexpected PrefixTables value: " + type.FullName);
			count++;
		}
		if (count != 13) throw new InvalidOperationException("Expected thirteen Web grammar hosts.");
		Console.WriteLine($"# Verified PrefixTables={expected} on {count} Web grammar hosts");
	}

	static Func<string, int> Bind(Type type, string name, bool web)
	{
		var method = type.GetMethods(BindingFlags.Public | BindingFlags.Static).Single(m => m.Name == name && m.GetParameters()[0].ParameterType == typeof(string) && m.GetParameters().Length == (web ? 2 : 1));
		var input = Expression.Parameter(typeof(string), "input");
		if (web)
		{
			var value = Expression.Variable(method.GetParameters()[1].ParameterType.GetElementType()!);
			return Expression.Lambda<Func<string, int>>(Expression.Block([value], Expression.Condition(Expression.Call(method, input, value), Expression.Constant(1), Expression.Constant(0))), input).Compile();
		}
		var match = Expression.Variable(method.ReturnType);
		return Expression.Lambda<Func<string, int>>(Expression.Block([match], Expression.Assign(match, Expression.Call(method, input)),
			Expression.Condition(Expression.Property(match, "IsSuccess"), Expression.Property(match, "Value"), Expression.Constant(-1))), input).Compile();
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	static long Measure(Func<string, int> parse, string input, int count)
	{
		long result = 0;
		for (var i = 0; i < count; i++) result += parse(input);
		return result;
	}
}
