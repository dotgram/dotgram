using System;

using Xunit;

namespace DotGram.Ir.Prototype;

/// <summary>The fixtures and the inputs each is read on, for every test that reads them all.</summary>
static class Cases
{
	public sealed record Fixture(string Grammar, Type Parser, string Entry, Func<IrFragment, object?[], bool>? Guards = null);

	public static readonly Dictionary<string, Fixture> Fixtures = new(StringComparer.Ordinal)
	{
		["Resumption"]            = new(Grammars.Resumption,            typeof(ResumptionParser),            "ParseStart"),
		["ResumptionRecursive"]   = new(Grammars.ResumptionRecursive,   typeof(ResumptionRecursiveParser),   "ParseStart"),
		["Commit"]                = new(Grammars.Commit,                typeof(CommitParser),                "ParseStart"),
		["CommitInner"]           = new(Grammars.CommitInner,           typeof(CommitInnerParser),           "ParseStart"),
		["CaptureRollback"]       = new(Grammars.CaptureRollback,       typeof(CaptureRollbackParser),       "ParseStart"),
		["CaptureRollbackChoice"] = new(Grammars.CaptureRollbackChoice, typeof(CaptureRollbackChoiceParser), "ParseStart"),
		["BindingRollback"]       = new(Grammars.BindingRollback,       typeof(BindingRollbackParser),       "ParseStart"),
		["Heading"]               = new(Grammars.Fence,                 typeof(FenceParser),                 "ParseHeading"),
		["Fenced"]                = new(Grammars.Fence,                 typeof(FenceParser),                 "ParseFenced"),
		["ActionTiming"]          = new(Grammars.ActionTiming,          typeof(ActionTimingParser),          "ParseStart"),
		["GuardTiming"]           = new(Grammars.GuardTiming,           typeof(GuardTimingParser),           "ParseStart", static (_, arguments) => ((string)arguments[0]!).Length < 3),
		["Loop"]                  = new(Grammars.Loop,                  typeof(LoopParser),                  "ParseStart"),
		["Rebinding"]             = new(Grammars.Rebinding,             typeof(RebindingParser),             "ParseStart"),
		["Look"]                  = new(Grammars.Look,                  typeof(LookParser),                  "ParseStart"),
		["Csv"]                   = new(Grammars.Csv,                   typeof(CsvParser),                   "ParseFile",  static (_, arguments) => ((string)arguments[0]!).Length <= 16),
		["Json"]                  = new(Grammars.Json,                  typeof(JsonTwinParser),              "ParseJson"),
	};

	public static TheoryData<string, string> Inputs()
	{
		var data = new TheoryData<string, string>();

		foreach (var (fixture, input) in All())
			data.Add(fixture, input);

		return data;
	}

	static List<(string Fixture, string Input)> All()
	{
		var data = new List<(string, string)>();

		void Add(string fixture, params string[] inputs)
		{
			foreach (var input in inputs)
				data.Add((fixture, input));
		}

		Add("Resumption",            "xy", "xyy", "x", "y", "", "xx", "xz", "xyz");
		Add("ResumptionRecursive",   "xy", "xyy", "xxy", "xxxy", "xxyy", "x", "xxz", "xxxxxxxxy");
		Add("Commit",                "xy", "xyy", "x", "");
		Add("CommitInner",           "xy", "xyy", "xyyy", "x");
		Add("CaptureRollback",       "xy", "xxy", "x", "xyz");
		Add("CaptureRollbackChoice", "xy", "xz", "xx", "x");
		Add("BindingRollback",       "ab-a", "ab-ab", "a-a", "ab-b", "abb-ab", "a-");
		Add("Heading",               "Title\n=====\n", "Title\n===\n", "Ti\n=======\n", "\n\n", "T\n");
		Add("Fenced",                "```\ncode\n```\n", "````\n```\n````", "```\ncode\n``\n", "```x\n", "``\n", "   ```\n a\n  ```  \nrest");
		Add("ActionTiming",          "xy", "xyy", "x", "");
		Add("GuardTiming",           "xy", "xyy", "xyyy", "x");
		Add("Loop",                  "a", "ab", "a,b", "a,b,c", "a,,b", "", "abcdefgh");
		Add("Rebinding",             "", "aa", "aabb", "aab", "abab");
		Add("Look",                  "ab", "abc", "a", "");
		Add("Csv",                   "", "a,1\n", "a,1\nbb,22\n", "a,1", "a,\n", "abcdefghijklmnopq,1\n", "a,1\nb;2\n", "a,1\r\nb,2\r");
		Add("Json",                  "1", " [1, 2 ,{\"a\":null}] ", "{}", "[]", "[1,]", "{\"a\" 1}", "tru", "\"\\u12G4\"", "[[[", "01", "1e", "-", "{\"a\":[true,false,{\"b\":\"\\n\"}]}");

		return data;
	}

	/// <summary>The inputs of one fixture.</summary>
	public static List<string> InputsOf(string fixture)
	{
		return [.. All().Where(row => row.Fixture == fixture).Select(static row => row.Input)];
	}
}
