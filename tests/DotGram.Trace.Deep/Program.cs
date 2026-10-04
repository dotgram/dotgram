using System;
using System.Diagnostics;

using DotGram.Web;

// A JSON array opened `depth` deep and never closed: refused at the end with every level open, and
// a refusal at every level on the way down. What GramWhy keeps is a frame a rule entered and the
// refusals at the furthest position; this says how much the process needed for that.
var depth = args.Length > 0 ? int.Parse(args[0], System.Globalization.CultureInfo.InvariantCulture) : 50_000;
var text  = new string('[', depth);
var why   = new Rfc8259.GramWhy();

Rfc8259.Match<JsonValue> match;

using (Rfc8259.Tracing(why))
	match = Rfc8259.TryParseJson(text);

var deepest = 0;

foreach (var path in why.Paths)
	deepest = Math.Max(deepest, path.Rules.Count);

using var self = Process.GetCurrentProcess();

self.Refresh();

Console.WriteLine($"refused={!match.IsSuccess}");
Console.WriteLine($"same={why.Message == match.Error && why.Position == match.Position}");
Console.WriteLine($"paths={why.Paths.Count}");
Console.WriteLine($"deepest={deepest}");
Console.WriteLine($"peak={self.PeakWorkingSet64}");

return 0;
