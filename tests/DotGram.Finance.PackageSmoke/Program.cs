using System;
using System.Globalization;

using DotGram.Finance.Fix;

var body = "35=0\u000149=S\u000156=T\u000134=1\u000152=20260915-12:00:00\u0001";
var prefix = "8=FIX.4.4\u00019=" + body.Length.ToString(CultureInfo.InvariantCulture) + "\u0001" + body;
var checksum = 0;
foreach (var c in prefix) checksum = (checksum + c) & 255;
var wire = prefix + "10=" + checksum.ToString("000", CultureInfo.InvariantCulture) + "\u0001";
if (!Fix44.TryParse(wire, out var message, out var error) || message is not Heartbeat)
	throw new Exception(error?.ToString() ?? "Expected Heartbeat.");
if (message.Header.SenderCompID != "S" || message.OriginalWire != wire)
	throw new Exception("Package model did not preserve the message.");
Console.WriteLine("DotGram.Finance package smoke: Heartbeat parsed.");
