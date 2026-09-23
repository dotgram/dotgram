using System;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;

// The first call of the FIX parsers, in phases, in a fresh process (from sql-39's fixfirst).
//
//     fixfirst <directory> generated | hand | generated-bytes | hand-bytes | generated-stream | hand-stream | parse | build | stock | validate | validate-generated | validate-all | validate-all-generated
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
//   build       FixMessages.Build of the fields the field parser read from the same message: the same phases,
//               with the field parse before the first build so that the build is what is timed.
//   validate            FixMessage.Validate of a parsed NewOrderSingle -- the walk over the schema tables.
//   validate-generated  NewOrderSingle.ValidateDefault of the same message -- the rule written out per type.
//               Both parse first and do not time the parse, so what is timed is one act: the first call of
//               validation in a fresh process, which is where a generated method of five thousand lines is
//               paid for. They are two modes rather than two phases of one, because a first call is a
//               property of the process and the second act in a process is not a first call.
//
// The compiled methods come from the runtime's MethodJittingStarted events through an EventListener,
// which costs time of its own: the absolute figures run above `--stand-paired --first`'s (7.5 ms against
// 5.0 for the first FIX parse). Use this for the anatomy and that for the time. The first parse lists
// every method compiled with its IL.

var directory = args.Length > 1 ? args[0] : null;
var mode      = args.Length > 1 ? args[1] : null;

// Which message type the validate modes read. A first call is paid per method, and the methods
// differ by three orders of magnitude between a Heartbeat and a TradeCaptureReport, so the cost is
// a row and not a number.
var which = args.Length > 2 ? args[2] : "NewOrderSingle";

if (directory is null || mode is not ("generated" or "hand" or "generated-bytes" or "hand-bytes" or "generated-stream" or "hand-stream" or "parse" or "build" or "stock" or "validate" or "validate-generated" or "validate-all" or "validate-all-generated"))
{
	Console.Error.WriteLine("usage: fixfirst <directory with DotGram.Finance.dll> generated | hand | generated-bytes | hand-bytes | generated-stream | hand-stream | parse | build | stock | validate | validate-generated | validate-all | validate-all-generated");

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

var options = (finance.GetType("DotGram.Finance.Fix.FixContext") ?? finance.GetType("DotGram.Finance.Fix.FixFieldOptions"))!;
var fieldParser = (mode.StartsWith("hand", StringComparison.Ordinal) ? handwritten!.GetType("DotGram.Handwritten.Fix.HandFixParser") : finance.GetType("DotGram.Finance.Fix.FixParser"))!;
var messages = finance.GetType("DotGram.Finance.Fix.FixMessages")!;
var bytes       = mode.EndsWith("-bytes", StringComparison.Ordinal);
var streamed    = mode.EndsWith("-stream", StringComparison.Ordinal);
// The field calls of this package were renamed when its door was merged -- ParseFields from a
// buffer, ReadFields from a stream -- while the hand-written parser beside it kept Parse. A
// harness that runs against an older build of the library has to answer to both names, so it asks
// for the current one and falls back rather than dying with a null it cannot explain.
var parseTypes  = streamed ? new[] { typeof(Stream), options, typeof(int), typeof(int?) } : [bytes ? typeof(byte[]) : typeof(string), options];
var parseNames  = mode.StartsWith("hand", StringComparison.Ordinal) ? ["Parse"] : new[] { streamed ? "ReadFields" : "ParseFields", "Parse" };
var parseFields = parseNames.Select(name => fieldParser.GetMethod(name, parseTypes)).FirstOrDefault(found => found != null)
	?? throw new MissingMethodException($"{fieldParser.Name} has none of {string.Join(", ", parseNames)} for this input.");

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

	case "validate-all":
	case "validate-all-generated":
	{
		RuntimeHelpers.RunClassConstructor(finance.GetType("DotGram.Finance.Fix.FixSchema")!.TypeHandle);
		Phase("FixSchema cctor", true);
		RuntimeHelpers.RunClassConstructor(messages.TypeHandle);
		Phase("FixMessages cctor", false);

		var parseOne = messages.GetMethod("Parse", [typeof(string), options])!;
		var built    = new List<object>();

		// One message of every type the package knows, found by asking it: a MsgType it does not
		// describe comes back as the Custom class, and everything else is one of the ninety-three.
		// Reading the list out of the package rather than writing it here means a type that is
		// added or removed changes the count below instead of being quietly left out.
		foreach (var candidate in Candidates())
		{
			try
			{
				var message = parseOne.Invoke(null, [WireOfType(candidate), null])!;

				if (message.GetType().Name != "Custom")
					built.Add(message);
			}
			catch (TargetInvocationException)
			{
				// Not a message type; the candidates are a superset on purpose.
			}
		}

		Phase($"parse {built.Count} types", false);

		var wanted = mode.EndsWith("-generated", StringComparison.Ordinal);
		var walk   = finance.GetType("DotGram.Finance.Fix.FixMessage")!.GetMethod("Validate", Type.EmptyTypes)!;
		var rules  = built.ToDictionary(
			static message => message,
			message => wanted
				? message.GetType().GetMethod("ValidateDefault", BindingFlags.Public | BindingFlags.Static)
					?? throw new MissingMethodException($"{message.GetType().Name} has no generated rule in this build.")
				: walk);

		var round = () =>
		{
			var found = 0;

			foreach (var message in built)
				found += ((Array)(wanted ? rules[message].Invoke(null, [message])! : rules[message].Invoke(message, null)!)).Length;

			return found;
		};

		var first = round();

		Phase("first validate all", true);

		var second = round();

		Phase("second validate all", false);
		Console.WriteLine($"{built.Count} types, {first} findings ({second} again)");

		break;
	}

	case "validate":
	case "validate-generated":
	{
		RuntimeHelpers.RunClassConstructor(finance.GetType("DotGram.Finance.Fix.FixSchema")!.TypeHandle);
		Phase("FixSchema cctor", true);
		RuntimeHelpers.RunClassConstructor(messages.TypeHandle);
		Phase("FixMessages cctor", false);

		var parse   = messages.GetMethod("Parse", [typeof(string), options])!;
		var message = parse.Invoke(null, [Wire(which), null])!;

		Phase("parse", false);

		// The walk is a call on the message; the generated rule is a static of the message's own
		// class. They are asked for the same act by the two roads they are reached by.
		var generated = mode == "validate-generated";
		var rule      = generated
			? message.GetType().GetMethod("ValidateDefault", BindingFlags.Public | BindingFlags.Static)
				?? throw new MissingMethodException($"{message.GetType().Name} has no generated rule in this build.")
			: finance.GetType("DotGram.Finance.Fix.FixMessage")!.GetMethod("Validate", Type.EmptyTypes)!;

		var call = generated
			? new Func<object>(() => rule.Invoke(null, [message])!)
			: () => rule.Invoke(message, null)!;

		var first = Guard(call);

		Phase("first validate", true);
		Guard(call);
		Phase("second validate", false);
		Console.WriteLine($"{which} {first}");

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

		var build = messages.GetMethod("Build", [typeof(string), fields.GetType(), options])!;
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

// A message of the named type with a standard header and nothing in its body. The body is empty on
// purpose: what a first call costs is dominated by compiling the method, which happens whole
// whichever branches the data takes, and an empty body is the one body every type can be given
// alike. What validation then reports is a list of what the type requires, which is a result of the
// right size to print and the wrong thing to read anything into.
static string Wire(string which)
{
	return WireOfType(which switch
	{
		"Heartbeat"          => "0",
		"NewOrderSingle"     => "D",
		"TradeCaptureReport" => "AE",
		"ExecutionReport"    => "8",
		_                    => throw new ArgumentException($"no wire for {which}"),
	});
}

// Every MsgType FIX 4.4 could spell, which is a superset: what is not a type comes back as the
// Custom class and is dropped. One letter, one digit, and two letters, which is how the standard
// numbers them.
static IEnumerable<string> Candidates()
{
	for (var digit = '0'; digit <= '9'; digit++)
		yield return digit.ToString();

	for (var letter = 'A'; letter <= 'Z'; letter++)
		yield return letter.ToString();

	for (var letter = 'a'; letter <= 'z'; letter++)
		yield return letter.ToString();

	for (var first = 'A'; first <= 'B'; first++)
		for (var second = 'A'; second <= 'Z'; second++)
			yield return string.Concat(first, second);
}

static string WireOfType(string type)
{
	var body   = $"35={type}\u000149=SENDER\u000156=TARGET\u000134=1\u000152=20260915-12:00:00\u0001";
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
