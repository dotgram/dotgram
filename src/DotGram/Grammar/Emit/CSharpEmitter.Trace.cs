using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using DotGram.Grammar.Model;

namespace DotGram.Grammar.Emit;

/// <summary>Spike: what a trace build adds around the readers' own events.</summary>
public static partial class CSharpEmitter
{
	static readonly Regex FailureDeclared = new(
		@"(?m)^([ \t]*)/// <summary>Where a match got before it gave up, and why\.</summary>(\r?\n)\1struct Failure\r?\n\1\{\r?\n",
		RegexOptions.Compiled);

	static readonly Regex RefusalRecorded = new(
		@"(static void Refuse_DotGram(?:_Over)?\(ref Failure failure, int at, string\[\]\?? expected[^\n]*\n([ \t]*)\{\r?\n[ \t]*if \(failure\.Looking > 0\)\r?\n[ \t]*return;\r?\n)",
		RegexOptions.Compiled);

	/// <summary>The sink, its scope, the rule table, and the events outside the readers.</summary>
	static string Traced(string text, RecognitionGraph graph, Machine? first, bool declares)
	{
		if (declares)
			text = FailureDeclared.Replace(text, match =>
			{
				var indent = match.Groups[1].Value;
				var ending = match.Groups[2].Value;
				var names  = string.Join(", ", graph.Rules.Select(rule => "\"" + rule.Name.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\""));
				var kinds  = string.Join(", ", graph.Rules.Select(rule => first?.TraceKindOf(rule) ?? 0));
				var added  = new StringBuilder();

				void Line(string line)
				{
					added.Append(line.Length == 0 ? "" : indent + line).Append(ending);
				}

				Line("/// <summary>Receives what a traced reading does.</summary>");
				Line("public abstract class GramTrace");
				Line("{");
				Line("\t/// <summary>A rule's reader was entered at a position.</summary>");
				Line("\tpublic virtual void Enter(int rule, int position) { }");
				Line("\t/// <summary>A rule's reader returned: where it ended, or -1.</summary>");
				Line("\tpublic virtual void Exit(int rule, int position, int end) { }");
				Line("\t/// <summary>A recording read refused at a position, wanting a set.</summary>");
				Line("\tpublic virtual void Refused(int position, string[]? expected) { }");
				Line("}");
				Line("");
				Line("static readonly global::System.Threading.AsyncLocal<GramTrace?> Tracing_DotGram = new global::System.Threading.AsyncLocal<GramTrace?>();");
				Line("");
				Line("/// <summary>Traces every read started in this flow until the scope is disposed.</summary>");
				Line("public static global::System.IDisposable Tracing(GramTrace? sink)");
				Line("{");
				Line("\tvar scope = new TraceScope_DotGram(Tracing_DotGram.Value);");
				Line("\tTracing_DotGram.Value = sink;");
				Line("\treturn scope;");
				Line("}");
				Line("");
				Line("sealed class TraceScope_DotGram : global::System.IDisposable");
				Line("{");
				Line("\treadonly GramTrace? previous;");
				Line("\tinternal TraceScope_DotGram(GramTrace? previous) { this.previous = previous; }");
				Line("\tpublic void Dispose() { Tracing_DotGram.Value = previous; }");
				Line("}");
				Line("");
				Line("/// <summary>The rules, by the id their events carry.</summary>");
				Line($"public static readonly string[] TraceRules = {{ {names} }};");
				Line("/// <summary>Per rule: 0 read by a method of its own, 1 a trivial leaf.</summary>");
				Line($"public static readonly byte[] TraceKinds = {{ {kinds} }};");
				Line("");

				return added + match.Value +
					indent + "\t/// <summary>The sink of this read, taken from the flow when the read began.</summary>" + ending +
					indent + "\tpublic GramTrace? Trace;" + ending;
			});

		text = RefusalRecorded.Replace(text, match =>
		{
			var indent = match.Groups[2].Value;
			var ending = match.Value.EndsWith("\r\n") ? "\r\n" : "\n";

			return match.Value + indent + "\tfailure.Trace?.Refused(at, expected);" + ending;
		});

		return text
			.Replace("new Failure { Quiet = true }", "new Failure { Quiet = true, Trace = Tracing_DotGram.Value }")
			.Replace("new Failure()", "new Failure { Trace = Tracing_DotGram.Value }");
	}
}
