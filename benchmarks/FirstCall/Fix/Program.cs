using System;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;

// The first call of the FIX parsers, in phases, in a fresh process (from sql-39's fixfirst).
//
//     fixfirst <directory> generated | hand | generated-bytes | hand-bytes | generated-stream | hand-stream | parse | build | stock
//
// The directory holds DotGram.Finance.dll, and DotGram.Handwritten.dll for `hand`. Each phase prints
// its time, how many methods the runtime compiled during it, their IL, and the time it spent compiling:
//
//   generated   FixParser.Parse of `55=ABC`: load, the type initializers, the first parse, the second.
//   hand        HandFixParser.Parse of the same, the same phases.
//   parse       FixMessages.Parse of a NewOrderSingle: load, FixSchema's type initializer on its own, the
//               first parse, the second.
//   stock       StockCountReader.TryParseCount of the example's four-line count (DotGram.Examples.dll in the
//               directory too): load, the type initializers, the first parse, the second.
//   build       FixMessages.Build of the fields FixParser.Parse read from the same message: the same phases,
//               with the field parse before the first build so that the build is what is timed.
//
// The compiled methods come from the runtime's MethodJittingStarted events through an EventListener,
// which costs time of its own: the absolute figures run above `--stand-paired --first`'s (7.5 ms against
// 5.0 for the first FIX parse). Use this for the anatomy and that for the time. The first parse lists
// every method compiled with its IL.

var directory = args.Length > 1 ? args[0] : null;
var mode      = args.Length > 1 ? args[1] : null;

if (directory is null || mode is not ("generated" or "hand" or "generated-bytes" or "hand-bytes" or "generated-stream" or "hand-stream" or "parse" or "build" or "stock"))
{
	Console.Error.WriteLine("usage: fixfirst <directory with DotGram.Finance.dll> generated | hand | generated-bytes | hand-bytes | generated-stream | hand-stream | parse | build | stock");

	return 2;
}

using var jit = new JitWatch();

var clock  = Stopwatch.StartNew();
var last   = 0L;
var jitAt  = JitInfo.GetCompilationTime();
var plain  = "55=ABC\u0001";
var order  = OrderWire();

var finance = Assembly.LoadFrom(Path.Combine(directory, "DotGram.Finance.dll"));
var handwritten = mode.StartsWith("hand", StringComparison.Ordinal) ? Assembly.LoadFrom(Path.Combine(directory, "DotGram.Handwritten.dll")) : null;

Phase("load", false);

var options = finance.GetType("DotGram.Finance.Fix.FixFieldOptions")!;
var fieldParser = (mode.StartsWith("hand", StringComparison.Ordinal) ? handwritten!.GetType("DotGram.Handwritten.Fix.HandFixParser") : finance.GetType("DotGram.Finance.Fix.FixParser"))!;
var messages = finance.GetType("DotGram.Finance.Fix.FixMessages")!;
var bytes       = mode.EndsWith("-bytes", StringComparison.Ordinal);
var streamed    = mode.EndsWith("-stream", StringComparison.Ordinal);
var parseFields = fieldParser.GetMethod("Parse", streamed ? [typeof(Stream), options, typeof(int), typeof(int?)] : [bytes ? typeof(byte[]) : typeof(string), options])!;

switch (mode)
{
	case "stock":
	{
		var examples = Assembly.LoadFrom(Path.Combine(directory, "DotGram.Examples.dll"));
		var reader   = examples.GetType("DotGram.Examples.Feeds.StockCountReader")!;

		Phase("load examples", false);
		RuntimeHelpers.RunClassConstructor(reader.TypeHandle);

		foreach (var type in reader.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic).Where(static type => !type.ContainsGenericParameters))
			RuntimeHelpers.RunClassConstructor(type.TypeHandle);

		Phase("cctors", false);

		var count = "apples: 12\npears: 7\nplums seven\nEND 3\n";
		var parse = reader.GetMethod("TryParseCount", [typeof(string)])!;
		var call  = () => parse.Invoke(null, [count])!;
		var first = call();

		Phase("first parse", true);
		call();
		Phase("second parse", false);
		Console.WriteLine(first.GetType().GetProperty("IsSuccess")!.GetValue(first)! is true ? "read" : "REFUSED");

		break;
	}

	case "generated" or "hand" or "generated-bytes" or "hand-bytes" or "generated-stream" or "hand-stream":
	{
		var types = new List<Type> { fieldParser };

		if (mode.StartsWith("generated", StringComparison.Ordinal))
		{
			var grammar = finance.GetType("DotGram.Finance.Fix.FixGrammar")!;

			types.Add(grammar);
			types.AddRange(grammar.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic).Where(static type => !type.ContainsGenericParameters));
		}

		foreach (var type in types)
			RuntimeHelpers.RunClassConstructor(type.TypeHandle);

		Phase("cctors", false);

		var input = bytes || streamed ? System.Text.Encoding.Latin1.GetBytes(plain) : (object)plain;
		var call  = () => Materialized(streamed
			? parseFields.Invoke(null, [new MemoryStream((byte[])input, false), null, 4096, null])!
			: parseFields.Invoke(null, [input, null])!);

		var first = call();

		Phase("first parse", true);
		call();
		Phase("second parse", false);
		Console.WriteLine($"fields {first.Length}");

		break;
	}

	case "parse":
	{
		RuntimeHelpers.RunClassConstructor(finance.GetType("DotGram.Finance.Fix.FixSchema")!.TypeHandle);
		Phase("FixSchema cctor", true);
		RuntimeHelpers.RunClassConstructor(messages.TypeHandle);
		Phase("FixMessages cctor", false);

		// A build before D53 takes a FixParseMode and checks the schema inside the parse; a build
		// after it takes none and the schema is a separate call. Both are asked for the work the
		// other does, so what this harness times is the same on either side of the change.
		var parseMode = finance.GetType("DotGram.Finance.Fix.FixParseMode");
		var parse     = messages.GetMethod("Parse", parseMode == null ? [typeof(string)] : [typeof(string), parseMode])!;
		var arguments = parseMode == null ? new object[] { order } : [order, Enum.ToObject(parseMode, 0)];
		var validate  = parseMode != null ? null : finance.GetType("DotGram.Finance.Fix.FixMessage")!.GetMethod("Validate", Type.EmptyTypes)!;

		var call = () =>
		{
			var message = parse.Invoke(null, arguments)!;

			validate?.Invoke(message, null);

			return message;
		};

		var first = Guard(call);

		Phase("first parse", true);
		Guard(call);
		Phase("second parse", false);
		Console.WriteLine(first);

		break;
	}

	default:
	{
		RuntimeHelpers.RunClassConstructor(finance.GetType("DotGram.Finance.Fix.FixSchema")!.TypeHandle);
		Phase("FixSchema cctor", true);
		RuntimeHelpers.RunClassConstructor(messages.TypeHandle);
		Phase("FixMessages cctor", false);

		var fields = parseFields.Invoke(null, [order, null])!;

		Phase("field parse", false);

		var build = messages.GetMethod("Build", [typeof(string), fields.GetType(), finance.GetType("DotGram.Finance.Fix.FixFieldOptions")!])!;
		var call  = () => build.Invoke(null, [order, fields, null])!;

		var first = Guard(call);

		Phase("first build", true);
		Guard(call);
		Phase("second build", false);
		Console.WriteLine(first);

		break;
	}
}

return 0;

// A stream form reads lazily: what it returns is walked to the end so that the first parse is the whole parse.
static Array Materialized(object fields)
{
	return fields is Array array ? array : ((System.Collections.IEnumerable)fields).Cast<object>().ToArray();
}

void Phase(string name, bool everyMethod)
{
	var now                = clock.ElapsedTicks;
	var (count, il, top)   = jit.Take(everyMethod);
	var compiling          = JitInfo.GetCompilationTime();
	var spent              = (compiling - jitAt).TotalMilliseconds;

	jitAt = compiling;

	Console.WriteLine($"{name,-18} {(now - last) * 1000.0 / Stopwatch.Frequency,8:0.00} ms   jit {count,4} methods {il,8} bytes IL {spent,7:0.00} ms compiling   {top}");
	last = clock.ElapsedTicks;
}

// A refusal is a result here, not a crash: the phase timings are what is asked for.
static string Guard(Func<object> call)
{
	try
	{
		return call().GetType().Name + " read";
	}
	catch (TargetInvocationException exception)
	{
		return "refused: " + exception.InnerException?.GetType().Name + " " + exception.InnerException?.Message;
	}
}

// A NewOrderSingle with the framing done: BodyLength (9) counts what follows it up to the checksum,
// and the checksum (10) is the sum of every byte before it, modulo 256.
static string OrderWire()
{
	var body   = "35=D\u000149=SENDER\u000156=TARGET\u000134=1\u000152=20260915-12:00:00\u000111=ORDER\u000121=1\u000155=ABC\u000154=1\u000160=20260915-12:00:00\u000138=100\u000140=2\u000144=12.50\u0001";
	var header = $"8=FIX.4.4\u00019={body.Length}\u0001";
	var sum    = 0;

	foreach (var character in header + body)
		sum += character;

	return $"{header}{body}10={sum % 256:D3}\u0001";
}

sealed class JitWatch : EventListener
{
	readonly object _gate = new();
	readonly Dictionary<string, long> _by = [];
	int  _count;
	long _il;

	protected override void OnEventSourceCreated(EventSource source)
	{
		if (source.Name == "Microsoft-Windows-DotNETRuntime")
			EnableEvents(source, EventLevel.Verbose, (EventKeywords)0x10);
	}

	protected override void OnEventWritten(EventWrittenEventArgs data)
	{
		if (data.EventName != "MethodJittingStarted_V1" || data.Payload is null)
			return;

		var names = data.PayloadNames!;
		var il    = Convert.ToInt64(data.Payload[names.IndexOf("MethodILSize")]);
		var space = data.Payload[names.IndexOf("MethodNamespace")]?.ToString() ?? "";
		var name  = data.Payload[names.IndexOf("MethodName")]?.ToString() ?? "";

		lock (_gate)
		{
			var key = space.Split('.').LastOrDefault() + "::" + name;

			_count++;
			_il += il;
			_by[key] = _by.GetValueOrDefault(key) + il;
		}
	}

	/// <summary>What was compiled since the last call; the biggest three, or every method.</summary>
	public (int Count, long Il, string Top) Take(bool everyMethod)
	{
		// The events arrive on another thread a moment after the method was compiled.
		Thread.Sleep(50);

		lock (_gate)
		{
			var separator = everyMethod ? Environment.NewLine + "      " : ", ";
			var ordered   = _by.OrderByDescending(static pair => pair.Value).Take(everyMethod ? int.MaxValue : 3);
			var top       = string.Join(separator, ordered.Select(static pair => $"{pair.Key} {pair.Value}"));
			var taken     = (_count, _il, top);

			_count = 0;
			_il    = 0;
			_by.Clear();

			return taken;
		}
	}
}
