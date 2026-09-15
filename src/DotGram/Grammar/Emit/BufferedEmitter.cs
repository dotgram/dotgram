using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

namespace DotGram.Grammar.Emit;

public static partial class CSharpEmitter
{
	static string? ByteRefusal(RecognitionGraph graph, IReadOnlyCollection<RuleSymbol> rules)
	{
		if (Locating(graph)) return "line and column tracking requires character input";
		foreach (var rule in rules)
		foreach (var node in NodeWalk.Descendants(graph.Bodies[rule]))
		{
			if (node is Node.Literal literal && (literal.IgnoreCase || literal.Text.Any(c => c > 255)))
				return "byte literals must be case-sensitive values in 0..255";
			var element = node as Node.Element ?? (node as Node.Behind)?.Test;
			if (element is not null && (element.Categories.Count > 0 || element.References.Count > 0 ||
				element.Ranges.Any(range => range.To > 255 && !(range.From == 0 && range.To == char.MaxValue))))
				return "byte sets must use values in 0..255 without Unicode categories or external sets";
		}
		return null;
	}

	// One storage algorithm, specialized while generating the source. The resulting
	// parsers use concrete arrays and block reads; they share no runtime input interface.
	static string BufferedByteClass() => BufferedTextClass
		.Replace("BufferedText", "BufferedBytes")
		.Replace("global::System.IO.TextReader", "global::System.IO.Stream")
		.Replace("char[]", "byte[]")
		.Replace("new char[", "new byte[")
		.Replace("out char value", "out byte value")
		.Replace("public char Get", "public byte Get")
		.Replace("ReadOnlySpan<char>", "ReadOnlySpan<byte>")
		.Replace("public bool Peek(int position, out byte value)", """
			public bool Matches(int position, string literal)
			{
				if (!Ensure(position, literal.Length)) return false;
				for (var i = 0; i < literal.Length; i++)
					if (_buffer[position - _start + i] != literal[i]) return false;
				return true;
			}

			public bool Peek(int position, out byte value)
			""");

	static string BufferedMethod(Publication publication, bool bytes = false) => "ReadBuffered_" + publication.MethodName + (bytes ? "_Bytes" : "");

	static void AddBufferedMachines(
		RecognitionGraph graph, ResultTypes results, ILineMap? lines, List<Compiled> machines,
		bool requested, bool byteRequested, bool overKinds, ICollection<GramDiagnostic>? diagnostics, int? partSize, bool spanCaptures)
	{
		foreach (var publication in graph.Publications)
		for (var form = 0; form < 2; form++)
		{
			var bytes = form == 1;
			if (publication.Kind != PublishKind.Parse && !publication.BufferedInput && !publication.BufferedBytes)
				continue;
			if (bytes ? !byteRequested && !publication.BufferedBytes : !requested && !publication.BufferedInput)
				continue;

			var rules = Reaches(graph, publication.Rule);
			var why = publication.Kind != PublishKind.Parse ? "only parse publications are supported" :
				overKinds ? "token-kind input is not supported by this form yet" :
				rules.Any(rule => NodeWalk.Descendants(graph.Bodies[rule]).Any(node => node is Node.External))
					? "external recognizers require contiguous input" : null;
			if (why is null && bytes) why = ByteRefusal(graph, rules);
			var tag = "_Buffered_" + publication.MethodName + (bytes ? "_Bytes" : "");
			while (machines.Any(compiled => compiled.Tag == tag)) tag += "_";
			Machine? machine = null;
			// Compile before emission so unsupported source-dependent factories cannot leave
			// an apparently usable public entry point in the generated file.
			if (why is null)
			{
				machine = new Machine(graph, results, lines, only: rules, tag: tag,
					partSize: partSize, bufferedInput: true, bufferedBytes: bytes, spanCaptures: spanCaptures);
				machine.Register(publication.Rule, whole: true);
				if (machine.UsesInput)
					why = "parserInput requires the complete input string";
			}
			if (why is not null)
			{
				diagnostics?.Add(new GramDiagnostic(GramCompiler.BufferedUnsupported,
					$"Cannot generate buffered input for '{publication.MethodName}': {why}.",
					publication.At.Position, publication.At.Length, GramSeverity.Error));
				continue;
			}
			machines.Add(new Compiled(machine!, [publication], "Recognize_DotGram" + tag, tag, false));
		}
	}

	static void EmitBufferedPublication(
		Writer file, RecognitionGraph graph, ResultTypes results, Compiled compiled, Publication publication)
	{
		var machine = compiled.Machine;
		var type = results.QualifiedOf(publication.Rule);
		var bytes = machine.BufferedBytes;
		var inputType = bytes ? "global::System.IO.Stream" : "global::System.IO.TextReader";
		var value = type ?? (bytes ? "byte[]" : "string");
		var match = $"{MatchType}<{value}>";
		var method = publication.MethodName;
		var context = machine.UsesContext ? $", {graph.Context} context" : "";
		var hands = (graph.Climbing.ContainsKey(publication.Rule) ? ", 0" : "") +
			", ref failure" + (type is null ? "" : ", out var value") +
			(machine.UsesContext ? ", context" : "") +
			(machine.UsesReading ? $", {publication.Reading}" : "");
		file.Line("/// <summary>Parses buffered input synchronously without retrying at refill boundaries.</summary>");
		file.Line("/// <remarks>The caller owns input. Pending backtracking and captures may retain the whole input.</remarks>");
		using (file.Block($"{AccessOf(publication)} static {match} Try{method}(" +
			$"{inputType} input{context}, int bufferSize = 4096, int maxRetained = int.MaxValue)"))
		{
			file.Line($"var text = new {(bytes ? "BufferedBytes" : "BufferedText")}(input, bufferSize, maxRetained);");
			file.Line($"var failure = new {FailureType}();");
			file.Line($"var end = {BufferedMethod(publication, bytes)}(text, 0{hands});");
			using (file.Block("if (end < 0)"))
			{
				file.Line("var starved = failure.OutOfInput == failure.Position + 1 || !text.Peek(failure.Position, out _);");
				file.Line($"return {match}.Failed(starved ? {OutcomeType}.Starved : {OutcomeType}.NoMatch, " +
					"starved ? \"Expected more input.\" : \"Input does not match.\", " +
					"failure.Position, failure.Expected, failure.ExpectedMore);");
			}
			file.Line($"return {match}.Success({(type is null ? bytes ? "text.Slice(0, end).ToArray()" : "text.Slice(0, end).ToString()" : "value")}, 0, end);");
		}
		using (file.Block($"{AccessOf(publication)} static {value} {method}(" +
			$"{inputType} input{context}, int bufferSize = 4096, int maxRetained = int.MaxValue)"))
		{
			file.Line($"var match = Try{method}(input{(machine.UsesContext ? ", context" : "")}, bufferSize, maxRetained);");
			file.Line("if (!match.IsSuccess) throw new global::System.FormatException(match.Error);");
			file.Line("return match.Value;");
		}
	}

	const string BufferedTextClass = """
		private sealed class BufferedText
		{
			private readonly global::System.IO.TextReader _input;
			private readonly int _limit;
			private char[] _buffer;
			private int _start;
			private int _released;
			private int _count;
			private bool _ended;

			public BufferedText(global::System.IO.TextReader input, int capacity, int limit)
			{
				if (input == null) throw new global::System.ArgumentNullException(nameof(input));
				if (capacity <= 0) throw new global::System.ArgumentOutOfRangeException(nameof(capacity));
				if (limit <= 0) throw new global::System.ArgumentOutOfRangeException(nameof(limit));
				_input = input;
				_limit = limit;
				_buffer = new char[global::System.Math.Min(capacity, limit)];
			}

			public bool Peek(int position, out char value)
			{
				if (!Ensure(position, 1)) { value = default; return false; }
				value = _buffer[position - _start];
				return true;
			}

			public char Get(int position)
			{
				if (!Peek(position, out var value)) throw new global::System.InvalidOperationException("Read past end of input.");
				return value;
			}

			public bool Ensure(int position, int length)
			{
				if (position < _start || length < 0) throw new global::System.ArgumentOutOfRangeException();
				if (position <= _count && length <= _count - position) return true;
				return Fill(position, length);
			}

			private bool Fill(int position, int length)
			{
				while (!_ended && (position > _count || length > _count - position))
				{
					if (_count - _start == _buffer.Length && _released > _start)
					{
						global::System.Array.Copy(_buffer, _released - _start, _buffer, 0, _count - _released);
						_start = _released;
					}
					if (_count - _start == _buffer.Length)
					{
						if (_buffer.Length == _limit)
						{
							// TextReader.Peek is optional; verify EOF with the block-read contract.
							var probe = new char[1];
							if (_input.Read(probe, 0, 1) == 0) { _ended = true; break; }
							throw new global::System.IO.IOException("Buffered input retention limit exceeded.");
						}
						var capacity = _buffer.Length <= _limit / 2 ? _buffer.Length * 2 : _limit;
						global::System.Array.Resize(ref _buffer, capacity);
					}
					var room = global::System.Math.Min(_buffer.Length - (_count - _start), int.MaxValue - _count);
					if (room == 0) throw new global::System.IO.IOException("Buffered input position limit exceeded.");
					var read = _input.Read(_buffer, _count - _start, room);
					if (read == 0) _ended = true;
					else _count += read;
				}
				return position <= _count && length <= _count - position;
			}

			public global::System.ReadOnlySpan<char> Slice(int from, int length)
			{
				if (!Ensure(from, length)) throw new global::System.ArgumentOutOfRangeException();
				return new global::System.ReadOnlySpan<char>(_buffer, from - _start, length);
			}

			public void ReleaseBefore(int position)
			{
				if (position < _released || position > _count) throw new global::System.ArgumentOutOfRangeException(nameof(position));
				_released = position;
			}
		}

		[global::System.Diagnostics.Conditional("DOTGRAM_TRACE")]
		static void Trace(string action, int state, int position, int arena, BufferedText text, string rule)
		{
			Trace(action, state, position, arena);
		}

		static int LineAt(BufferedText text, int position)
		{
			var line = 1;
			for (var at = 0; at < position; at++) if (text.Get(at) == '\n') line++;
			return line;
		}
		static int ColumnAt(BufferedText text, int position)
		{
			var column = 1;
			for (var at = 0; at < position; at++) column = text.Get(at) == '\n' ? 1 : column + 1;
			return column;
		}
		""";
}
