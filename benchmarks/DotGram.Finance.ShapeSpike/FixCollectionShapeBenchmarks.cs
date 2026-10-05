// SPIKE (not for merging): the exposure shape of the lists a FIX message hands its consumer, measured as a
// consumer sees it. One class holds every row of every version (Rows.Fix42.cs, Rows.Fix44.cs, Rows.Fix50.cs);
// a window runs the rows of its category. The class is built once per column, with the column's symbol
// (FIX_SHAPE_B and so on) selecting the code a consumer of that column writes.

using System.Text;

using BenchmarkDotNet.Attributes;

namespace DotGram.Finance.Benchmarks.ShapeSpike;

[MemoryDiagnoser]
public partial class FixCollectionShapeBenchmarks
{
	// A row that walks pre-parsed Orders walks this many, so that it lasts long enough to be read.
	const int Batch = 100;

	const int Parties = 8;
	const int Entries = 1000;

	[GlobalSetup]
	public void Setup()
	{
		Setup42();
		Setup44();
		Setup50();

		var print = Environment.GetEnvironmentVariable("FIX_SPIKE_PRINT") == "1";
		var wrong = new List<string>();

		void Check(string row, long actual)
		{
			if (print)
				Console.WriteLine($"\t\t[\"{row}\"] = {actual},");
			else if (!Expected.TryGetValue(row, out var expected) || expected != actual)
				wrong.Add($"{row}: {actual}, expected {(Expected.TryGetValue(row, out var e) ? e.ToString() : "nothing")}");
		}

		Check42(Check);
		Check44(Check);
		Check50(Check);

		if (wrong.Count > 0)
			throw new InvalidOperationException("The column does not do the work the others do: " + string.Join("; ", wrong));
	}

	static byte[] Latin1(string text)
	{
		return Encoding.Latin1.GetBytes(text);
	}

	// 4.2 and 4.4 begin FIX.4.n; 5.0 is FIXT.1.1 with the application version in the header.
	static string Wire(string version, string type, string fields)
	{
		var begin  = version == "50" ? "FIXT.1.1" : version == "42" ? "FIX.4.2" : "FIX.4.4";
		var appl   = version == "50" ? "1128=9|" : "";
		var body   = ("35=" + type + "|49=S|56=T|34=1|52=20260915-12:00:00|" + appl + fields).Replace('|', '\u0001');
		var wire   = "8=" + begin + "\u00019=" + body.Length.ToString(System.Globalization.CultureInfo.InvariantCulture) + "\u0001" + body;
		var sum    = 0;

		foreach (var c in wire)
			sum = (sum + c) & 255;

		return wire + "10=" + sum.ToString("000", System.Globalization.CultureInfo.InvariantCulture) + "\u0001";
	}

	// A NewOrderSingle: eight parties of two sub-IDs each (4.2: eight allocations), ExecInst of four values.
	static string OrderBody(string version, bool withGroup)
	{
		var body = new StringBuilder("11=ORDER|21=1|55=ABC|54=1|60=20260915-12:00:00|38=100|40=2|44=12.50|18=1 2 3 4|");

		if (!withGroup)
			return body.ToString();

		if (version == "42")
		{
			body.Append("78=").Append(Parties).Append('|');

			for (var i = 0; i < Parties; i++)
				body.Append("79=ACCT").Append(i).Append("|80=12.5|");
		}
		else
		{
			body.Append("453=").Append(Parties).Append('|');

			for (var i = 0; i < Parties; i++)
				body.Append("448=P").Append(i).Append("|447=D|452=1|802=2|523=S").Append(i).Append("a|803=1|523=S").Append(i).Append("b|803=2|");
		}

		return body.ToString();
	}

	// A MarketDataSnapshotFullRefresh of a thousand entries, bid and offer alternating.
	static string SnapshotBody()
	{
		var body = new StringBuilder("55=ABC|262=REQ|268=").Append(Entries).Append('|');

		for (var i = 0; i < Entries; i++)
			body.Append("269=").Append(i % 2).Append("|270=12.50|271=100|");

		return body.ToString();
	}
}
