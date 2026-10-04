using System;

using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Emit;

public static partial class CSharpEmitter
{
	/// <param name="tracing">
	/// In a trace build, what it numbers by: the iterator is then a method of its own, handed the
	/// sink by one that is not an iterator, so that the sink is the one set where the call was made
	/// rather than where the first element was asked for.
	/// </param>
	static void EmitYield(
		Writer file, Publication publication, string hands, string takes, string gives = "",
		TraceTables? tracing = null, string name = "", string machine = "methods", string? built = null)
	{
		var type = $"global::System.Collections.Generic.IEnumerable<{publication.ResultType!.Name}>";

		file.Line("/// <summary>Lazily parses consecutive elements; malformed input throws during enumeration.</summary>");

		if (tracing is not null)
		{
			using (file.Block($"{AccessOf(publication)} static {type} {publication.MethodName}(string input{takes})"))
				file.Line($"return {publication.MethodName}_DotGram(input{gives}, Tracing_DotGram.Value);");

			file.Line();
			file.Line($"/// <summary>What <c>{publication.MethodName}</c> enumerates, reporting to the sink that was set where it was called.</summary>");
		}

		using (file.Block(tracing is null
			? $"{AccessOf(publication)} static {type} {publication.MethodName}(string input{takes})"
			: $"static {type} {publication.MethodName}_DotGram(string input{takes}, GramTrace? trace)"))
		{
			if (publication.YieldRecovery) file.Line("var ordinal = 0;");
			file.Line("if (input == null) throw new global::System.ArgumentNullException(nameof(input));");
			if (publication.YieldMinimum > 0)
				file.Line("if (input.Length == 0) throw new global::System.FormatException(\"Expected at least one element at offset 0.\");");
			using (file.Block("for (var start = 0; start < input.Length; )"))
			{
				if (tracing is null)
				{
					file.Line($"var failure = new {FailureType}()" + (publication.YieldRecovery ? " { RecoveryOrdinal = ordinal++ };" : ";"));
					file.Line($"var end = {MethodOf(publication.Rule)}(global::System.MemoryExtensions.AsSpan(input), start{hands});");
					file.Line("if (end <= start) throw new global::System.FormatException(\"Invalid element at offset \" + failure.Position.ToString() + \".\");");
				}
				else
				{
					file.Line($"var failure = new {FailureType}()" + (publication.YieldRecovery ? " { RecoveryOrdinal = ordinal++ };" : ";"));
					file.Line(TraceBegin("read", name, "start", machine, "input", null, sink: "trace"));
					TracedRead(file, $"{MethodOf(publication.Rule)}(global::System.MemoryExtensions.AsSpan(input), start{hands})", built, declare: true, sink: "trace");

					using (file.Block("if (end <= start)"))
					{
						file.Line("var message = \"Invalid element at offset \" + failure.Position.ToString() + \".\";");
						file.Line();
						file.Line("if (trace != null && trace.HearsRejections)");
						file.Then("trace.Rejected(failure.Position, message);");
						file.Line();
						file.Line("throw new global::System.FormatException(message);");
					}
				}

				file.Line(publication.YieldBatch ? "foreach (var item in recognized) yield return item;" : "yield return recognized;");
				file.Line("start = end;");
			}
		}
	}
}
