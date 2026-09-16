using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Emit;

public static partial class CSharpEmitter
{
	static void EmitYield(Writer file, Publication publication, string hands, string takes)
	{
		file.Line("/// <summary>Lazily parses consecutive elements; malformed input throws during enumeration.</summary>");
		using (file.Block($"{AccessOf(publication)} static global::System.Collections.Generic.IEnumerable<{publication.ResultType!.Name}> {publication.MethodName}(string input{takes})"))
		{
			file.Line("if (input == null) throw new global::System.ArgumentNullException(nameof(input));");
			if (publication.YieldMinimum > 0)
				file.Line("if (input.Length == 0) throw new global::System.FormatException(\"Expected at least one element at offset 0.\");");
			using (file.Block("for (var start = 0; start < input.Length; )"))
			{
				file.Line($"var failure = new {FailureType}();");
				file.Line($"var end = {MethodOf(publication.Rule)}(global::System.MemoryExtensions.AsSpan(input), start{hands});");
				file.Line("if (end <= start) throw new global::System.FormatException(\"Invalid element at offset \" + failure.Position.ToString() + \".\");");
				file.Line("yield return recognized;");
				file.Line("start = end;");
			}
		}
	}
}
