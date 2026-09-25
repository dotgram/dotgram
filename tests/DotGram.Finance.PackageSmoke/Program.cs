using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using DotGram.Finance.Fix;
using DotGram.Finance.Fix.Fix44;

using Fix42 = DotGram.Finance.Fix.Fix42;
using Fix50 = DotGram.Finance.Fix.Fix50;

// A Heartbeat, framed by hand: what a consumer of the package has to be able to do with it.
var body     = "35=0\u000149=S\u000156=T\u000134=1\u000152=20260915-12:00:00.250\u0001";
var prefix   = "8=FIX.4.4\u00019=" + body.Length.ToString(CultureInfo.InvariantCulture) + "\u0001" + body;
var checksum = 0;

foreach (var c in prefix)
	checksum = (checksum + c) & 255;

var wire = prefix + "10=" + checksum.ToString("000", CultureInfo.InvariantCulture) + "\u0001";

if (!FixParser.TryParseMessage(wire, out var message, out var error) || message is not FixMessage.Heartbeat heartbeat)
	throw new Exception(error?.ToString() ?? "Expected a Heartbeat.");

// The typed fields, and the types a consumer sees them in: the runtime's own, on every framework
// the package ships for. On .NET 8 this program runs the netstandard2.0 build, whose DateOnly comes
// from a package that forwards it to the runtime's; a DateOnly that were the package's own would
// not be this program's.
if (heartbeat.SenderCompID?.Value != "S" || heartbeat.MsgSeqNum?.Value != 1L)
	throw new Exception("Expected the typed header fields.");

if (heartbeat.SendingTime?.Value != new DateTimeOffset(2026, 9, 15, 12, 0, 0, 250, TimeSpan.Zero))
	throw new Exception("Expected SendingTime as an instant at offset zero.");

if (typeof(FixField.Date).BaseType?.GetGenericArguments()[0] != typeof(DateOnly)
	|| typeof(FixField.Time).BaseType?.GetGenericArguments()[0] != typeof(TimeOnly))
	throw new Exception("A date and a time of day must be the runtime's own DateOnly and TimeOnly.");

// The schema: the compiled-in one, and one loaded over it from a fragment of a QuickFIX dictionary.
if (!heartbeat.Validate(Fix44Context.Default) || !heartbeat.IsValid)
	throw new Exception("A well-formed Heartbeat must hold to the compiled-in schema.");

var loaded = Fix44Context.Default.Load(
	"<fix><messages><message name='Heartbeat' msgtype='0' msgcat='admin'><field name='TestReqID' required='Y'/></message></messages></fix>");

if (FixParser.ParseMessage(wire).Validate(loaded) || FixParser.ParseMessage(wire).Validate(Fix44Context.Default) == false)
	throw new Exception("A loaded schema must replace the compiled-in check of the type it describes, and leave the compiled-in context as it was.");

// The other doors: pipe-delimited text, a byte stream, the octets themselves, and flat fields.
if (FixParser.ParseMessage(wire.Replace('\u0001', '|'), Fix44Context.WithLogFraming) is not FixMessage.Heartbeat)
	throw new Exception("Expected a pipe-delimited Heartbeat.");

using var stream = new MemoryStream(Encoding.Latin1.GetBytes(wire + wire));

var count = 0;

foreach (var item in FixParser.ReadMessages(stream))
{
	if (item is not FixMessage.Heartbeat { MsgSeqNum.Value: 1L })
		throw new Exception("Byte-stream result differs.");

	count++;
}

if (count != 2)
	throw new Exception("Expected two messages on one stream.");

var octets = Encoding.Latin1.GetBytes(wire);

if (FixParser.ParseMessage(octets) is not FixMessage.Heartbeat)
	throw new Exception("Expected a Heartbeat from the octets themselves.");

if (FixParser.ParseMessages(octets).Length != 1 || !FixParser.TryParseMessage(octets, out var tried, out _) || tried is not FixMessage.Heartbeat)
	throw new Exception("The octet doors do not agree with each other.");

var fields = FixParser.ParseFields(wire);

if (fields.Length != 8 || fields[0] is not FixField.Text { Tag: FixTag.BeginString })
	throw new Exception("Expected flat typed fields from FixParser.");

// FIX 4.2, a namespace of its own with the same names: the same Heartbeat under its BeginString is
// its own class and holds to its own schema, and the 4.4 one does not.
var prefix42 = "8=FIX.4.29=" + body.Length.ToString(CultureInfo.InvariantCulture) + "" + body;
var wire42   = prefix42 + "10=" + (prefix42.Sum(c => (int)c) % 256).ToString("000", CultureInfo.InvariantCulture) + "";

if (Fix42.FixParser.ParseMessage(wire42) is not Fix42.FixMessage.Heartbeat heartbeat42 || !heartbeat42.Validate(Fix42.Fix42Context.Default))
	throw new Exception("A well-formed FIX 4.2 Heartbeat must hold to the compiled-in FIX 4.2 schema.");

if (FixParser.ParseMessage(wire42).Validate(Fix44Context.Default))
	throw new Exception("A FIX 4.2 BeginString must not hold to the FIX 4.4 schema.");

// FIX 5.0 SP2 over FIXT 1.1, and a zoned value it adds: a SecurityStatus, whose class is
// SecurityStatusMessage, with the time of day of an instrument's maturity at an offset.
var body50   = "35=f\u000149=S\u000156=T\u000134=1\u000152=20260915-12:00:00.250\u000155=IBM\u00011079=13:09+05:30\u0001";
var prefix50 = "8=FIXT.1.1\u00019=" + body50.Length.ToString(CultureInfo.InvariantCulture) + "\u0001" + body50;
var wire50   = prefix50 + "10=" + (prefix50.Sum(c => (int)c) % 256).ToString("000", CultureInfo.InvariantCulture) + "\u0001";

if (Fix50.FixParser.ParseMessage(wire50) is not Fix50.FixMessage.SecurityStatusMessage status || !status.Validate(Fix50.Fix50Context.Default))
	throw new Exception("A well-formed FIX 5.0 SP2 SecurityStatus must hold to the compiled-in FIX 5.0 SP2 schema.");

if (status.MaturityTime?.Value != (new TimeOnly(13, 9), new TimeSpan(5, 30, 0)))
	throw new Exception("Expected a TZTimeOnly as the clock and its offset.");

if (typeof(FixParser).Assembly.GetType("DotGram.Finance.Fix.Fix44.Fix44Parser") != null ||
	typeof(FixParser).Assembly.GetType("DotGram.Examples.Finance.Fix44") != null ||
	typeof(FixParser).Assembly.GetReferencedAssemblies().Any(name => name.Name is "DotGram.Finance.Fix44" or "DotGram.Examples"))
	throw new Exception("The Fix44 fixture must not be included in the package.");

Console.WriteLine("DotGram.Finance package smoke: char, octets, byte stream, pipe, typed fields, the runtime's own date types, and the schema compiled in and loaded, and FIX 4.2 and FIX 5.0 SP2 beside FIX 4.4 passed.");
