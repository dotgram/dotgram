using System.Reflection;
using System.Text;

using DotGram;
using DotGram.Sql.Standard;
using DotGram.Benchmarks;

// g1count <gram-file>: the shipped SQL:2023 grammar compiled with DOTGRAM_COUNTS; per guard site,
// how often it was asked and the materialising walks its values cost, for each stand row.
var gram  = File.ReadAllText(args[0]);
string Select20 = "SELECT " + string.Join(", ", Enumerable.Range(0, 20).Select(i => "a" + i)) + " FROM t WHERE a0 = 1";
var rows = new (string Name, Func<bool> Read)[]
{
	("sql/literal",    () => Counted.TryParseLiteral("1").IsSuccess),
	("sql/column",     () => Counted.TryParseColumnReference("a.b.c").IsSuccess),
	("sql/arithmetic", () => Counted.TryParseValueExpression("(a + b) * c - d / 5").IsSuccess),
	("sql/nest8",      () => Counted.TryParseValueExpression("((((((((a))))))))").IsSuccess),
	("sql/condition",  () => Counted.TryParseSearchCondition("x = 1 AND y IS NOT NULL OR z BETWEEN 1 AND 2").IsSuccess),
	("sql/select1",    () => Counted.TryParseQueryExpression("SELECT a FROM t").IsSuccess),
	("sql/select20",   () => Counted.TryParseQueryExpression(Select20).IsSuccess),
	("sql/values",     () => Counted.TryParseQueryExpression("VALUES (1)").IsSuccess),
};

var ways = typeof(Counted).GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic).Where(static t => t.Name.StartsWith("Ways")).ToList();

foreach (var (name, read) in rows)
{
	foreach (var w in ways)
	{
		((Dictionary<string, long[]>)w.GetField("CountSites", BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null)!).Clear();
		w.GetField("CountWalks", BindingFlags.Static | BindingFlags.NonPublic)!.SetValue(null, 0L);
	}

	var ok = read();
	long walks = 0;
	var sites = new Dictionary<string, long[]>();

	foreach (var w in ways)
	{
		walks += (long)w.GetField("CountWalks", BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null)!;
		foreach (var (k, v) in (Dictionary<string, long[]>)w.GetField("CountSites", BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null)!)
		{
			if (!sites.TryGetValue(k, out var c)) sites[k] = c = new long[2];
			c[0] += v[0]; c[1] += v[1];
		}
	}

	long guardWalks = sites.Values.Sum(static v => v[1]);
	Console.WriteLine($"== {name}: {(ok ? "accepted" : "REFUSED")}; walks {walks}, of them at guards {guardWalks}, guard asks {sites.Values.Sum(static v => v[0])}");

	foreach (var (k, v) in sites.OrderByDescending(static e => e.Value[1]).ThenByDescending(static e => e.Value[0]))
	{
		var at   = int.Parse(k[(k.IndexOf('@') + 1)..]);
		var line = at < 0 ? -1 : gram[..Math.Min(at, gram.Length)].Count(static c => c == '\n') + 1;
		Console.WriteLine($"   {v[0],5} asks {v[1],5} walks  line {line,5}  {k}");
	}
}

namespace DotGram.Benchmarks
{
	[Gram("SqlStandard.gram", Lexical = true)]
	static partial class Counted
	{
	}
}
