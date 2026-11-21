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
		["ListsInSequence"]       = new(Grammars.ListsInSequence,       typeof(ListsInSequenceParser),       "ParseStart"),
		["ListRuleTwice"]         = new(Grammars.ListRuleTwice,         typeof(ListRuleTwiceParser),         "ParseStart"),
		["NestedSettled"]         = new(Grammars.NestedSettled,         typeof(NestedSettledParser),         "ParseStart"),
		["Throwing"]              = new(Grammars.Throwing,              typeof(ThrowingParser),              "ParseStart"),
		["GivenBackTwice"]        = new(Grammars.GivenBackTwice,        typeof(GivenBackTwiceParser),        "ParseStart"),
		["Arithmetic"]            = new(Grammars.Arithmetic,            typeof(ArithmeticParser),            "ParseExpr"),
		["Config"]                = new(Grammars.Config,                typeof(ConfigParser),                "ParseFile"),
		["SettledBefore"]         = new(Grammars.SettledBefore,         typeof(SettledBeforeParser),         "ParseStart"),
		["SealedBefore"]          = new(Grammars.SealedBefore,          typeof(SealedBeforeParser),          "ParseStart"),
		["CalledBefore"]          = new(Grammars.CalledBefore,          typeof(CalledBeforeParser),          "ParseStart"),
		["FailedTurnBefore"]      = new(Grammars.FailedTurnBefore,      typeof(FailedTurnBeforeParser),      "ParseStart"),
		["NestedSealedBefore"]    = new(Grammars.NestedSealedBefore,    typeof(NestedSealedBeforeParser),    "ParseStart"),
		["LongLookBefore"]        = new(Grammars.LongLookBefore,        typeof(LongLookBeforeParser),        "ParseStart"),
		["SealedCaptureBefore"]   = new(Grammars.SealedCaptureBefore,   typeof(SealedCaptureBeforeParser),   "ParseStart"),
		["DecidingNestBefore"]    = new(Grammars.DecidingNestBefore,    typeof(DecidingNestBeforeParser),    "ParseStart"),
		["SameBefore"]            = new(Grammars.SameBefore,            typeof(SameBeforeParser),            "ParseStart"),
		["YamlLineBefore"]        = new(Grammars.YamlLineBefore,        typeof(YamlLineBeforeParser),        "ParseStart"),
		["DeepSplit"]             = new(Grammars.DeepSplit,             typeof(DeepSplitParser),             "ParseStart"),
		["DeepSplitOpen"]         = new(Grammars.DeepSplitOpen,         typeof(DeepSplitOpenParser),         "ParseStart"),
		["XmlNameGivesBack"]      = new(Grammars.XmlNameGivesBack,      typeof(XmlNameGivesBackParser),      "ParseXml"),
		["TurnGivesBack"]         = new(Grammars.TurnGivesBack,         typeof(TurnGivesBackParser),         "ParseStart"),
		["StaleRecord"]           = new(Grammars.StaleRecord,           typeof(StaleRecordParser),           "ParseStart"),
		["FailsInsideDecider"]    = new(Grammars.FailsInsideDecider,    typeof(FailsInsideDeciderParser),    "ParseStart"),
		["FailsInCaller"]         = new(Grammars.FailsInCaller,         typeof(FailsInCallerParser),         "ParseStart"),
		["LastInBraces"]          = new(Grammars.LastInBraces,          typeof(LastInBracesParser),          "ParseStart"),
		["FailsRoundACycle"]      = new(Grammars.FailsRoundACycle,      typeof(FailsRoundACycleParser),      "ParseStart"),
		["LookAfterDecider"]      = new(Grammars.LookAfterDecider,      typeof(LookAfterDeciderParser),      "ParseStart"),
		["StoodInRecursion"]      = new(Grammars.StoodInRecursion,      typeof(StoodInRecursionParser),      "ParseStart"),
		["GivesBackInTurn"]       = new(GiveBackGrammars.GivesBackInTurn, typeof(GivesBackInTurnParser),   "ParseStart"),
		["GivesBackPastStop"]     = new(GiveBackGrammars.GivesBackPastStop, typeof(GivesBackPastStopParser), "ParseStart"),
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
		Add("ListsInSequence",       "xyyxy", "xyy", "y", "", "xyxy", "xyxyyxyx", "xyyxyz", "xxyxx", "xyxyxyy");
		Add("ListRuleTwice",         "xyyxy", "xyy", "y", "", "xyxy", "xyxyyxyx", "xyyxyz", "xxyxx", "xyxyxyy");
		Add("NestedSettled",         "a", "1;a", "12;34;a,b", "12;34;a,b!", ";;;ab", "1;2a", "12;;3;abc,d", "12;34;a,b,c,");
		Add("Throwing",              "a", "1:a", "1:2:3a,b", "1:2:3a,b!", "1:2:ab,c", "12:a", "1:2:a,b,");
		Add("GivenBackTwice",        "ab", "abc", "a,b,c,d", "abcde", "abc!", "a,b,c!", "a", "a,b");
		Add("Arithmetic",            "1", "(1^0^4)^1/", "(1^0^4)^1", "1+2*3^4^5-(6/7)", "1+", "1*2/", "((1)", "2^", "1+2+3+4+", "(1+(2*(3^(4)))");
		Add("Config",                "", "k=ab;", "k=ab;j=ab;", "k=ab;j=ab;!", "k=ax;", "k=ab;j=a", "k=;", "k=ab;j=ax;i=ab;");
		Add("SettledBefore",         "a", "00a", "000a,b,c", "00a,b!", "0b,", "000", "a,b,c,", "0a0");
		Add("SealedBefore",          "a", "00a", "000a,b,c", "00a,b!", "0b,", "000", "a,b,c,", "0a0");
		Add("CalledBefore",          "a", "  a", "   a,b,c", "  a,b!", " b,", "   ", "a,b,c,", " a ");
		Add("FailedTurnBefore",      "a", "ab;a", "ab;cd;a,b", "abc", "abc!", "ab;a,b,c!", "a,b,", ";a");
		Add("NestedSealedBefore",    "<>a", "<00>a", "<000>a,b,c", "<00>a,b!", "<0b,", "<00", "<>a,b,c,", "00a");
		Add("LongLookBefore",        "a", "00a", "000a,b,c", "00a,b!", "0b,", "000", "a,b,c,", "0a0");
		Add("SealedCaptureBefore",   "a;a", "00ab;a", "000x;a,b,c", "00a;a,b!", "0;b,", "000", "x;a,b,c,", "0a0;a");
		Add("DecidingNestBefore",    "(ya", "((y)a", "(((y))a,b,c", "((y)a,b!", "(y)a", "((y", "(ya,b,c,", "y");
		Add("SameBefore",            "0=0a", "00=00a", "000=000a,b,c", "00=00a,b!", "0=00a", "00=0a", "0=0a,b,c,", "=a");
		Add("YamlLineBefore",        "1:a", "  12:a", "   123:a,b,c", "  1:a,b!", " :a", "1:", "1:a,b,c,", "  a:b");
		Add("DeepSplit",             "!", "b!", "(bb)!", "((bbb))!", "((bbb))", "(b))!", "((b)!", "bbbb");
		Add("DeepSplitOpen",         "!", "b!", "(bb!", "((bbb!", "((bbb", "(b)!", "((b", "bbbb");
		Add("XmlNameGivesBack",      "<ab=\"1\"></a>", "<ab=\"1\">", "<ab=\"1\"></ab>", "<a b=\"1\">", "<ab=\"1\"/>", "<a></a>", "<a x=\"1\" y='2'><b>text</b></a>", "<abc=\"1\"d='2'></a>", "<a><bc='x'/></a>");
		Add("TurnGivesBack",         "abc", "acabc", "abcac", "ab", "acab", "abcab", "a", "abcabd");
		Add("StaleRecord",           "abbc!", "aaabbbbc", "aaabbbbc!", "abcbc", "aabbcbcbc", "aaabbbcbbcb!", "abbbbbbbc", "bc!");
		Add("FailsInsideDecider",    "z", "00z", "qab", "00qa,b,c", "000qa,b,!", "0qa,", "q", "00", "0z0");
		Add("FailsInCaller",         "a", "00ab", "000a,b,c", "00a,b,!", "0a,", "0", "a,b", "!");
		Add("LastInBraces",          "a", "00a", "000a,b,c", "00a,b,!", "0a,b;c", "0", "a,b;", "0a;!");
		Add("FailsRoundACycle",      "a", "00ab", "0(0a,b,)c", "00a,b,!", "(a,)", "((0a))b", "0(", "a,b,");
		Add("LookAfterDecider",      "a!", "00a,b!", "000a,b,c!", "00a,b,!", "0a,", "a", "0!", "a,b!x");
		Add("StoodInRecursion",      "x", "(x)ab", "(x)abc0y", "((x)abc0y)a,b,c00yy;", "(x)a", "((x)ab", "(x)ab0y;y", "(x)");
		Add("GivesBackInTurn",       "00a,b;c;", "a;", "ab;", "a,b;", "a,b,;", "0a,b;c,d;e;", "00a,b;c", "a;;", "");
		Add("GivesBackPastStop",     "0\"a,b;\"", "\"\"", "00\"ab;\"", "\"a,b,c;\"\"\"d,e;\"", "\"a,;\"", "\"\"\"\"", "0\"a,b;", "\"\"\"", "\"a;\"", "a");
		Add("Json",                  "1", " [1, 2 ,{\"a\":null}] ", "{}", "[]", "[1,]", "{\"a\" 1}", "tru", "\"\\u12G4\"", "[[[", "01", "1e", "-", "{\"a\":[true,false,{\"b\":\"\\n\"}]}");

		return data;
	}

	/// <summary>The inputs of one fixture.</summary>
	public static List<string> InputsOf(string fixture)
	{
		return [.. All().Where(row => row.Fixture == fixture).Select(static row => row.Input)];
	}
}
