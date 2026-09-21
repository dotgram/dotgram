using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using DotGram.Finance.Fix;

var body = "35=0\u000149=S\u000156=T\u000134=1\u000152=20260915-12:00:00\u0001";
var prefix = "8=FIX.4.4\u00019=" + body.Length.ToString(CultureInfo.InvariantCulture) + "\u0001" + body;
var checksum = 0;
foreach (var c in prefix) checksum = (checksum + c) & 255;
var wire = prefix + "10=" + checksum.ToString("000", CultureInfo.InvariantCulture) + "\u0001";
if (!FixParser.TryParseMessage(wire, out var message, out var error) || message is not FixMessage.Heartbeat)
	throw new Exception(error?.ToString() ?? "Expected Heartbeat.");
if (message.Header.SenderCompID != "S" || message.OriginalWire != wire)
	throw new Exception("Package model did not preserve the message.");
if (message.Header.GetField(34)?.TypedValue is not FixField.MsgSeqNum sequence || sequence.Value != 1)
	throw new Exception("Expected a typed sequence-number field.");
if (FixParser.ParseMessage(wire.Replace('\u0001', '|'), FixFieldOptions.Log) is not FixMessage.Heartbeat)
	throw new Exception("Expected a pipe-delimited Heartbeat.");
using var stream = new MemoryStream(Encoding.Latin1.GetBytes(wire + wire));
var count = 0;
foreach (var item in FixParser.ReadMessages(stream))
{
	if (item is not FixMessage.Heartbeat || item.OriginalWire != wire) throw new Exception("Byte-stream result differs.");
	count++;
}
if (count != 2) throw new Exception("Expected two messages on one stream.");
var octets = Encoding.Latin1.GetBytes(wire);
if (FixParser.ParseMessage(octets) is not FixMessage.Heartbeat octetHeartbeat || octetHeartbeat.OriginalWire != wire)
	throw new Exception("Expected a Heartbeat from the octets themselves.");
if (FixParser.ParseMessages(octets).Length != 1 ||
	!FixParser.TryParseMessage(octets, out var tried, out _) || tried!.OriginalWire != wire)
	throw new Exception("The octet doors do not agree with each other.");
var fields = FixParser.ParseFields(wire);
if (fields.Length != 8 || fields[0] is not FixField.BeginString)
	throw new Exception("Expected flat typed fields from FixParser.");
if (typeof(FixParser).Assembly.GetType("DotGram.Finance.Fix44.Fix44Parser") != null ||
	typeof(FixParser).Assembly.GetType("DotGram.Examples.Finance.Fix44") != null ||
	typeof(FixParser).Assembly.GetReferencedAssemblies().Any(name => name.Name is "DotGram.Finance.Fix44" or "DotGram.Examples"))
	throw new Exception("The Fix44 fixture must not be included in the package.");
Console.WriteLine("DotGram.Finance package smoke: char, octets, byte stream, pipe and typed ADT passed.");
