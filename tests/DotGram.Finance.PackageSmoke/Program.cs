using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using DotGram.Finance;

var body = "35=0\u000149=S\u000156=T\u000134=1\u000152=20260915-12:00:00\u0001";
var prefix = "8=FIX.4.4\u00019=" + body.Length.ToString(CultureInfo.InvariantCulture) + "\u0001" + body;
var checksum = 0;
foreach (var c in prefix) checksum = (checksum + c) & 255;
var wire = prefix + "10=" + checksum.ToString("000", CultureInfo.InvariantCulture) + "\u0001";
if (!FixMessages.TryParse(wire, out var message, out var error) || message is not Heartbeat)
	throw new Exception(error?.ToString() ?? "Expected Heartbeat.");
if (message.Header.SenderCompID != "S" || message.OriginalWire != wire)
	throw new Exception("Package model did not preserve the message.");
if (message.Header.GetField(34)?.TypedValue is not FixField.MsgSeqNum sequence || sequence.Value != 1)
	throw new Exception("Expected a typed sequence-number field.");
if (FixMessages.ParseLog(wire.Replace('\u0001', '|')) is not Heartbeat)
	throw new Exception("Expected a pipe-delimited Heartbeat.");
using var stream = new MemoryStream(Encoding.Latin1.GetBytes(wire + wire));
var count = 0;
foreach (var item in FixMessages.ReadMessages(stream))
{
	if (item is not Heartbeat || item.OriginalWire != wire) throw new Exception("Byte-stream result differs.");
	count++;
}
if (count != 2) throw new Exception("Expected two messages on one stream.");
var fields = Fix.Parse(wire);
if (fields.Length != 8 || fields[0] is not FixField.BeginString)
	throw new Exception("Expected flat typed fields from Fix.");
if (typeof(Fix).Assembly.GetType("DotGram.Examples.Finance.Fix44") != null ||
	typeof(Fix).Assembly.GetReferencedAssemblies().Any(name => name.Name == "DotGram.Examples"))
	throw new Exception("The example parser must not be included in the package.");
Console.WriteLine("DotGram.Finance package smoke: char, byte stream, pipe and typed ADT passed.");
