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
		.Replace("IParserInputSource<char>", "IParserInputSource<byte>")
		.Replace("char[]", "byte[]")
		.Replace("ArrayPool<char>", "ArrayPool<byte>")
		.Replace("Array.Empty<char>", "Array.Empty<byte>")
		.Replace("new char[", "new byte[")
		.Replace("out char value", "out byte value")
		.Replace("public char Get", "public byte Get")
		.Replace("char stop", "byte stop")
		.Replace(" retained characters;", " retained bytes;")
		.Replace("private bool _ended;", "private bool _ended;\n\t// The caller's own array, read in place (D7): never given to the pool.\n\tprivate bool _borrowed;")
		.Replace("if (_buffer.Length <= KeptLength)", "if (!_borrowed && _buffer.Length <= KeptLength)")
		.Replace("public void Dispose()", "public BufferedBytes(global::System.ReadOnlyMemory<byte> input)\n\t{\n\t\t_input = null!;\n\t\t_limit = int.MaxValue;\n\t\tif (global::System.Runtime.InteropServices.MemoryMarshal.TryGetArray(input, out global::System.ArraySegment<byte> held) && held.Array != null)\n\t\t{\n\t\t\t// Positions count from the segment's start; the array is indexed from its own.\n\t\t\t_buffer = held.Array;\n\t\t\t_start = -held.Offset;\n\t\t}\n\t\telse\n\t\t{\n\t\t\t// Memory that is not an array is copied once, and that copy is still the caller's to keep.\n\t\t\t_buffer = input.ToArray();\n\t\t}\n\t\t_count = input.Length;\n\t\t_capacity = input.Length;\n\t\t_ended = true;\n\t\t_borrowed = true;\n\t}\n\n\tpublic void Dispose()")
		.Replace("ReadOnlySpan<char>", "ReadOnlySpan<byte>")
		.Replace("public bool Peek(int position, out byte value)", """
			// Compares where it stands and hands back nothing into the buffer (Machine.Cut).
			public bool Matches(int position, string literal)
			{
				if (!Ensure(position, literal.Length)) return false;
				for (var i = 0; i < literal.Length; i++)
					if (_buffer[position - _start + i] != literal[i]) return false;
				return true;
			}

			public bool Peek(int position, out byte value)
			""");

	static string BufferedMethod(Publication publication, bool bytes = false, bool inPlace = false) =>
		"ReadBuffered_" + publication.MethodName + (bytes ? "_Bytes" : "") + (inPlace ? "_Memory" : "");

	static void AddBufferedMachines(
		RecognitionGraph graph, ResultTypes results, ILineMap? lines, List<Compiled> machines,
		bool requested, bool byteRequested, bool overKinds, ICollection<GramDiagnostic>? diagnostics, int? partSize, bool spanCaptures, bool prefixTables,
		Dictionary<string, (string Name, string Declaration)>? expectedTables)
	{
		if (!requested && !byteRequested && !graph.Publications.Any(one => one.BufferedInput || one.BufferedBytes))
			return;

		var added = new List<Compiled>();

		foreach (var publication in graph.Publications)
		for (var form = 0; form < 2; form++)
		{
			var bytes = form == 1;
			if (bytes ? !byteRequested && !publication.BufferedBytes : !requested && !publication.BufferedInput)
				continue;

			var rules = Reaches(graph, publication.Rule);
			var why = overKinds ? "token-kind input is not supported by this form yet" :
				rules.Any(rule => NodeWalk.Descendants(graph.Bodies[rule]).Any(node => node is Node.External { UsesInputView: false }))
					? "external recognizers require contiguous input" : null;
			if (why is null && bytes) why = ByteRefusal(graph, rules);
			var tag = "_Buffered_" + publication.MethodName + (bytes ? "_Bytes" : "");
			while (machines.Concat(added).Any(compiled => compiled.Tag == tag)) tag += "_";
			Machine? machine = null;
			// Compile before emission so unsupported source-dependent factories cannot leave
			// an apparently usable public entry point in the generated file.
			if (why is null)
			{
				machine = new Machine(graph, results, lines, only: rules, tag: tag,
					partSize: partSize, bufferedInput: true, bufferedBytes: bytes, spanCaptures: spanCaptures,
					bufferedFind: publication.Kind != PublishKind.Parse, prefixTables: prefixTables, expectedTables: expectedTables, deferCompilation: true);
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
			// Read by methods where the string form is, over the buffer instead of a span
			// (docs/design/fix-reader-buffered-2026-09-18.md): the same reader, asking the buffer
			// through the machine's helpers. Only there, so that the two forms of one publication
			// are one rendering and answer alike to the character — a refusal inside a literal is
			// placed where the literal began by the reader and where it broke off by the engine.
			// Not where the engine proves it can let the buffer go as it reads, which the reader
			// does not do yet; nor where a reading can reach itself and would hand its input to
			// another thread, which a buffer does not go with (GRAM5014). An external recognizer
			// reading the input through its view may make the buffer fetch more under the reader,
			// which is safe because nothing is let go here and the reader holds positions, never
			// spans, across anything that can fetch (the contract at BufferedText.Fill).
			var readable = publication.Kind == PublishKind.Parse &&
				machines.Exists(one => one.Direct && one.Publications.Contains(publication)) &&
				machine!.CanDirect([publication]) && !machine.CanReleaseBuffered;
			var direct = readable && !machine!.Probes;

			// The one reason a publication read by methods over a string is read by the engine
			// over a buffer that is a limit of this compiler rather than a choice, said as such.
			if (readable && !direct)
				diagnostics?.Add(new GramDiagnostic(GramCompiler.BufferedOnEngine,
					$"'{publication.MethodName}' over a {(bytes ? "stream" : "reader")} is read by the engine, where over a " +
					"string it is read by methods: a rule it reaches can reach itself, and a reading that deep goes " +
					"on in another thread with the whole input, which a buffer cannot hand over.",
					publication.At.Position, publication.At.Length, GramSeverity.Info));
			added.Add(new Compiled(machine!, [publication], "Recognize_DotGram" + tag, tag, false, direct));

			// Bytes the caller holds whole are read by methods where the string is, even where the
			// stream is not. What keeps a stream on the engine is what a stream does: an external
			// recognizer reading through its view may make the buffer fetch more under the reader,
			// and the engine lets the window go as it reads. Bytes held whole do neither — the view
			// the engine hands an external is handed it here too, and never fetches — so they get a
			// machine of their own, and the stream keeps what it proves (fix-reader, byte[] in
			// place, A).
			if (bytes && !direct && publication.Kind == PublishKind.Parse &&
				machines.Exists(one => one.Direct && one.Publications.Contains(publication)))
			{
				var memoryTag = tag + "_Memory";
				var memory = new Machine(graph, results, lines, only: rules, tag: memoryTag,
					partSize: partSize, bufferedInput: true, bufferedBytes: true, spanCaptures: spanCaptures,
					prefixTables: prefixTables, expectedTables: expectedTables, deferCompilation: true) { InPlace = true };

				if (memory.CanDirect([publication]) && !memory.Probes)
				{
					machine!.MemoryElsewhere = true;
					added.Add(new Compiled(memory, [publication], "Recognize_DotGram" + memoryTag, memoryTag, false, true));
				}
			}
		}
		// Keep single-rule release proofs and small parsers independent. Large,
		// substantially overlapping readers may share states within one input form.
		var reached = added.ToDictionary(compiled => compiled,
			compiled => Reaches(graph, compiled.Publications[0].Rule));
		for (var host = 0; host < added.Count; host++)
		{
			var owner = added[host];
			var rules = new HashSet<RuleSymbol>(reached[owner]);
			if (owner.Direct || !Large(rules))
				continue;

			var publications = owner.Publications.ToList();
			var guests = new List<int>();
			for (var guest = host + 1; guest < added.Count; guest++)
			{
				var candidate = added[guest];
				var other = reached[candidate];
				if (candidate.Direct || candidate.Machine.BufferedBytes != owner.Machine.BufferedBytes ||
					candidate.Machine.UsesContext != owner.Machine.UsesContext || !Large(other) ||
					rules.Count(other.Contains) * 10 < Math.Max(rules.Count, other.Count) * 9)
					continue;

				rules.UnionWith(other);
				publications.AddRange(candidate.Publications);
				guests.Add(guest);
			}

			if (guests.Count == 0)
				continue;

			var shared = new Machine(graph, results, lines, only: graph.Rules.Where(rules.Contains).ToArray(),
				tag: owner.Tag, partSize: partSize, bufferedInput: true, bufferedBytes: owner.Machine.BufferedBytes,
				spanCaptures: spanCaptures, bufferedFind: publications.Any(one => one.Kind != PublishKind.Parse),
				prefixTables: prefixTables, expectedTables: expectedTables, deferCompilation: true);
			added[host] = owner with { Machine = shared, Publications = publications };
			for (var guest = guests.Count - 1; guest >= 0; guest--)
				added.RemoveAt(guests[guest]);
		}

		foreach (var compiled in added)
		{
			compiled.Machine.CompileRules();
			if (!compiled.Direct)
				foreach (var publication in compiled.Publications)
					compiled.Machine.Register(publication.Rule, whole: publication.Kind == PublishKind.Parse);
			machines.Add(compiled);
		}

		bool Large(IReadOnlyCollection<RuleSymbol> rules)
		{
			return rules.Count > 1 && rules.SelectMany(rule => NodeWalk.Descendants(graph.Bodies[rule]))
				.Take(128).Count() == 128;
		}
	}

	static void EmitBufferedPublication(
		Writer file, RecognitionGraph graph, ResultTypes results, Compiled compiled, Publication publication)
	{
		if (!compiled.Machine.BufferedBytes)
		{
			EmitBufferedForm(file, graph, results, compiled, publication, "global::System.IO.TextReader", buffered: true);

			return;
		}

		// The machine for bytes held whole writes those entries and nothing else; the stream's
		// then writes only its own (InPlace, MemoryElsewhere).
		if (!compiled.Machine.InPlace)
			EmitBufferedForm(file, graph, results, compiled, publication, "global::System.IO.Stream", buffered: true);

		if (compiled.Machine.MemoryElsewhere)
			return;

		// Bytes the caller already holds are read by the same machine over the array itself:
		// one fill that is the whole input, and the end known at once (D7). No buffer to size,
		// so no buffer parameters.
		EmitBufferedForm(file, graph, results, compiled, publication, "global::System.ReadOnlyMemory<byte>", buffered: false);
	}

	static void EmitBufferedForm(
		Writer file, RecognitionGraph graph, ResultTypes results, Compiled compiled, Publication publication,
		string inputType, bool buffered)
	{
		var machine = compiled.Machine;
		var type = results.QualifiedOf(publication.Rule);
		var bytes = machine.BufferedBytes;
		var parameters = buffered ? ", int? bufferSize = null, int? maxRetained = null" : "";
		var construct = buffered ? "(input, bufferSize ?? DefaultBufferSize, maxRetained ?? DefaultMaxRetained)" : "(input)";
		var forward = buffered ? ", bufferSize, maxRetained" : "";
		var value = publication.ResultType is { } contract ? contract.Name + (contract.IsSequence ? "[]" : "") : type ?? (bytes ? "byte[]" : "string");
		var match = $"{MatchType}<{value}>";
		var method = publication.MethodName;
		var context = machine.UsesContext ? $", {graph.Context} context" : "";
		var hands = (graph.Climbing.ContainsKey(publication.Rule) ? ", 0" : "") +
			", ref failure" + (type is null ? "" : ", out var value") +
			(machine.UsesContext ? ", context" : "") +
			(machine.UsesReading ? $", {publication.Reading}" : "");

		if (!buffered)
		{
			// A byte[] is the same bytes, handed over; checked for null here, because the
			// conversion to memory would quietly make an empty input of it.
			var passed = "new global::System.ReadOnlyMemory<byte>(input ?? throw new global::System.ArgumentNullException(nameof(input)))" +
				(machine.UsesContext ? ", context" : "");

			void Forward(string returns, string name)
			{
				file.Line("/// <summary>The same, over bytes the caller already holds.</summary>");
				file.Line($"{AccessOf(publication)} static {returns} {name}(byte[] input{context}) => {name}({passed});");
			}

			if (publication.Kind == PublishKind.Yield)
				Forward($"global::System.Collections.Generic.IEnumerable<{publication.ResultType!.Name}>", method);
			else if (publication.Kind == PublishKind.Find)
				Forward($"global::System.Collections.Generic.IEnumerable<{match}>", method);
			else
			{
				Forward(match, "Try" + method);
				Forward(value, method);
			}
		}
		// A buffered lazy overload is not the iterator itself. An iterator keeps each parameter
		// twice, so int? made its object 16 bytes larger a call (measured, D9), and a check made
		// inside it waits for the first MoveNext. The overload resolves the defaults and checks
		// its arguments at the call, then hands plain ints to a private iterator.
		var lazyConstruct = buffered ? "(input, bufferSize, maxRetained)" : construct;

		string Lazily(string returns)
		{
			if (!buffered)
				return $"{AccessOf(publication)} static {returns} {method}({inputType} input{context})";

			var iterator = "Iterate_DotGram_" + method;

			using (file.Block($"{AccessOf(publication)} static {returns} {method}({inputType} input{context}{parameters})"))
			{
				file.Line("if (input == null) throw new global::System.ArgumentNullException(nameof(input));");
				file.Line("var capacity = bufferSize ?? DefaultBufferSize;");
				file.Line("var limit = maxRetained ?? DefaultMaxRetained;");
				file.Line("if (capacity <= 0) throw new global::System.ArgumentOutOfRangeException(nameof(bufferSize));");
				file.Line("if (limit <= 0) throw new global::System.ArgumentOutOfRangeException(nameof(maxRetained));");
				file.Line($"return {iterator}(input{(machine.UsesContext ? ", context" : "")}, capacity, limit);");
			}

			return $"private static {returns} {iterator}({inputType} input{context}, int bufferSize, int maxRetained)";
		}

		if (publication.Kind == PublishKind.Yield)
		{
			file.Line("/// <summary>Lazily parses consecutive buffered elements; leaves input open.</summary>");
			using (file.Block(Lazily($"global::System.Collections.Generic.IEnumerable<{publication.ResultType!.Name}>")))
			{
				file.Line($"using var text = new {(bytes ? "BufferedBytes" : "BufferedText")}{lazyConstruct};");
				file.Line("var start = 0;");
				if (publication.YieldRecovery) file.Line("var ordinal = 0;");
				if (publication.YieldMinimum > 0)
					file.Line("if (!text.Peek(0, out _)) throw new global::System.FormatException(\"Expected at least one element at offset 0.\");");
				using (file.Block("while (text.Peek(start, out _))"))
				{
					file.Line($"var failure = new {FailureType}()" + (publication.YieldRecovery ? " { RecoveryOrdinal = ordinal++ };" : ";"));
					file.Line($"var end = {BufferedMethod(publication, bytes, machine.InPlace)}(text, start{hands});");
					file.Line("if (end <= start) throw new global::System.FormatException(\"Invalid element at offset \" + failure.Position.ToString() + \".\");");
					file.Line(publication.YieldBatch ? "foreach (var item in value) yield return item;" : "yield return value;");
					file.Line("start = end;");
					// What an element read is let go once it is handed back, a line or a column
					// asked for or not: the buffer counts line breaks as it lets them go (Located).
					// Only a look behind reads back past where an element began.
					if (!Reaches(graph, publication.Rule).Any(rule =>
						NodeWalk.Descendants(graph.Bodies[rule]).Any(node => node is Node.Behind)))
						file.Line("text.ReleaseBefore(start);");
				}
			}
			return;
		}
		if (publication.Kind == PublishKind.Find)
		{
			var retain = Reaches(graph, publication.Rule).Any(rule =>
				NodeWalk.Descendants(graph.Bodies[rule]).Any(node => node is Node.Behind));
			file.Line("/// <summary>Lazily finds occurrences through a reusable buffer; leaves input open.</summary>");
			using (file.Block(Lazily($"global::System.Collections.Generic.IEnumerable<{match}>")))
			{
				file.Line($"using var text = new {(bytes ? "BufferedBytes" : "BufferedText")}{lazyConstruct};");
				file.Line("var start = 0;");
				using (file.Block("while (true)"))
				{
					file.Line($"var failure = new {FailureType}();");
					file.Line($"var end = {BufferedMethod(publication, bytes, machine.InPlace)}(text, start{hands});");
					using (file.Block("if (end >= 0)"))
						file.Line($"yield return {match}.Success({(type is null ? bytes ? "text.Slice(start, end - start).ToArray()" : "text.Slice(start, end - start).ToString()" : "value")}, start, end - start);");
					file.Line("if (end <= start && !text.Peek(start, out _)) yield break;");
					file.Line("start = end > start ? end : checked(start + 1);");
					if (!retain) file.Line("text.ReleaseBefore(start);");
				}
			}
			return;
		}
		file.Line("/// <summary>Parses buffered input synchronously without retrying at refill boundaries.</summary>");
		file.Line("/// <remarks>The caller owns input. Pending backtracking and captures may retain the whole input.</remarks>");
		using (file.Block($"{AccessOf(publication)} static {match} Try{method}(" +
			$"{inputType} input{context}{parameters})"))
		{
			file.Line($"using var text = new {(bytes ? "BufferedBytes" : "BufferedText")}{construct};");
			file.Line($"var failure = new {FailureType}();");
			file.Line($"var end = {BufferedMethod(publication, bytes, machine.InPlace)}(text, 0{hands});");
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
			$"{inputType} input{context}{parameters})"))
		{
			file.Line($"var match = Try{method}(input{(machine.UsesContext ? ", context" : "")}{forward});");
			file.Line("if (!match.IsSuccess) throw new global::System.FormatException(match.Error);");
			file.Line("return match.Value;");
		}
	}

	/// <summary>
	/// A buffer that says which line a position is on and how far into it: line breaks counted
	/// as far as asked, and as far as released, so that nothing is ever read behind what the
	/// buffer still holds (D5). A position asked about behind the last one is counted back to
	/// over the distance, and where that reaches what was released, the line began where the
	/// release left it. Written only where a recovery asks for a line or a column: every other
	/// buffer releases without counting anything.
	/// </summary>
	internal static string Located(string buffered, bool locating)
	{
		if (!locating)
			return WithoutLine(WithoutLine(buffered, "/*COUNT_RELEASED*/"), "/*LOCATED*/");

		var at      = buffered.IndexOf("/*LOCATED*/", StringComparison.Ordinal);
		var begins  = buffered.LastIndexOf('\n', at) + 1;
		var indent  = buffered.Substring(begins, at - begins);
		var ending  = buffered.Contains("\r\n") ? "\r\n" : "\n";
		var members = new System.Text.StringBuilder();
		var lines   = LocatedMembers.Replace("\r\n", "\n").Split('\n');

		for (var i = 0; i < lines.Length; i++)
		{
			if (i > 0)
			{
				members.Append(ending);

				if (lines[i].Length > 0)
					members.Append(indent);
			}

			members.Append(lines[i]);
		}

		return buffered
			.Replace("/*COUNT_RELEASED*/", "CountReleased(position);")
			.Replace("/*LOCATED*/", members.ToString());
	}

	/// <summary>The text without the line a placeholder stands on.</summary>
	static string WithoutLine(string text, string token)
	{
		var at = text.IndexOf(token, StringComparison.Ordinal);

		if (at < 0)
			return text;

		var begins = text.LastIndexOf('\n', at) + 1;
		var ends   = text.IndexOf('\n', at);

		// And the empty line that set it apart, where one did: nothing is left to set apart.
		if (begins >= 2 && text[begins - 2] == '\n')
			begins -= 1;
		else if (begins >= 3 && text[begins - 2] == '\r' && text[begins - 3] == '\n')
			begins -= 2;

		return text.Remove(begins, (ends < 0 ? text.Length : ends + 1) - begins);
	}

	const string LocatedMembers = """
		private int _lineAt;
		private int _lines;
		private int _lineStart;
		private int _releasedLine;

		public int LineAt(int position)
		{
			MoveLine(position);
			return _lines + 1;
		}

		public int ColumnAt(int position)
		{
			MoveLine(position);
			return position - _lineStart + 1;
		}

		// Counted as far as the release, and where the line holding it began, for a question
		// about a place behind that reaches the release.
		private void CountReleased(int position)
		{
			MoveLine(position);
			_releasedLine = _lineStart;
		}

		private void MoveLine(int position)
		{
			if (position >= _lineAt)
			{
				for (; _lineAt < position; _lineAt++)
					if (_buffer[_lineAt - _start] == '\n')
					{
						_lines++;
						_lineStart = _lineAt + 1;
					}

				return;
			}

			for (var at = _lineAt - 1; at >= position; at--)
				if (_buffer[at - _start] == '\n')
					_lines--;

			_lineAt    = position;
			_lineStart = position;

			while (_lineStart > _released && _buffer[_lineStart - 1 - _start] != '\n')
				_lineStart--;

			if (_lineStart == _released)
				_lineStart = _releasedLine;
		}
		""";

	const string BufferedTextClass = """
		private sealed class BufferedText : global::System.IDisposable, IParserInputSource<char>
		{
			private readonly global::System.IO.TextReader _input;
			private readonly int _limit;
			private char[] _buffer;
			// Pool buckets may be larger: read size and retention use the requested capacity.
			private int _capacity;
			private int _start;
			private int _released;
			private int _count;
			private bool _ended;

			// A buffer longer than this is let go instead of returned to the shared pool, which
			// keeps what it is given for the life of the process: one long record would otherwise
			// hold its buffer, and the halves it grew through, long after the parse ended (D5).
			// Not returning costs time at every size, because the buffer grows by doubling and
			// each step past 85,000 bytes is a fresh large-object allocation. Measured on FIX, one
			// length/data record repeated through the Stream overload, pooled against never
			// pooled: 4 KB 0.88 against 1.09 us, 64 KB 4.5 against 31, 256 KB 29 against 52,
			// 1 MB 131 against 422, 4 MB 866 against 1,404. So the line sits high, where the
			// token and value stores already let go of what is oversized, and a record below it
			// keeps pooled speed; above it a parse pays the table, and the process keeps none of it.
			private const int KeptLength = 1048576;

			public BufferedText(global::System.IO.TextReader input, int capacity, int limit)
			{
				if (input == null) throw new global::System.ArgumentNullException(nameof(input));
				if (capacity <= 0) throw new global::System.ArgumentOutOfRangeException(nameof(capacity));
				if (limit <= 0) throw new global::System.ArgumentOutOfRangeException(nameof(limit));
				_input = input;
				_limit = limit;
				_capacity = global::System.Math.Min(capacity, limit);
				_buffer = global::System.Buffers.ArrayPool<char>.Shared.Rent(_capacity);
			}

			public void Dispose()
			{
				if (_capacity == 0) return;
				if (_buffer.Length <= KeptLength)
					global::System.Buffers.ArrayPool<char>.Shared.Return(_buffer, clearArray: true);
				_buffer = global::System.Array.Empty<char>();
				_capacity = 0;
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
					if (_count - _start == _capacity && _released > _start)
					{
						global::System.Array.Copy(_buffer, _released - _start, _buffer, 0, _count - _released);
						_start = _released;
					}
					if (_count - _start == _capacity)
					{
						if (_capacity == _limit)
						{
							// TextReader.Peek is optional; verify EOF with the block-read contract.
							var probe = new char[1];
							if (_input.Read(probe, 0, 1) == 0) { _ended = true; break; }
							throw new global::System.IO.IOException("Buffered input needs more than " + _limit.ToString(global::System.Globalization.CultureInfo.InvariantCulture) + " retained characters; pass a larger maxRetained.");
						}
						var capacity = _capacity <= _limit / 2 ? _capacity * 2 : _limit;
						var grown = global::System.Buffers.ArrayPool<char>.Shared.Rent(capacity);
						global::System.Array.Copy(_buffer, 0, grown, 0, _count - _start);
						// The old array is cleared and given back here, and a compaction above moves
						// what it holds: a span into this buffer does not outlive a fill. So across
						// anything that can fill — a read, a rule, an external recognizer — a reader
						// holds positions, never spans (Machine.Cut, the contract of the emitter).
						if (_buffer.Length <= KeptLength)
							global::System.Buffers.ArrayPool<char>.Shared.Return(_buffer, clearArray: true);
						_buffer = grown;
						_capacity = capacity;
					}
					var room = global::System.Math.Min(_capacity - (_count - _start), int.MaxValue - _count);
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
				/*COUNT_RELEASED*/
				_released = position;
			}

			/*LOCATED*/

			// Where the input ended, once Ensure has said it has: the position after its last
			// character. Before that it is only how far the input has been read.
			public int End => _count;

			// The first position at or after `from` holding a stop, or -1 once the input has ended
			// without one. It reads on only as far as it has to look, a block at a time, and holds
			// what it has looked at from `from` on, which is what the caller has not yet released:
			// so it keeps no more than a reading of the same text character by character would,
			// and a stop beyond the limit is the same IOException (D5).
			public int IndexOf(int from, char stop0)
			{
				for (var at = from; Ensure(at, 1); at = _count)
				{
					var found = global::System.MemoryExtensions.IndexOf(Held(at), stop0);
					if (found >= 0) return at + found;
				}
				return -1;
			}

			public int IndexOf(int from, char stop0, char stop1)
			{
				for (var at = from; Ensure(at, 1); at = _count)
				{
					var found = global::System.MemoryExtensions.IndexOfAny(Held(at), stop0, stop1);
					if (found >= 0) return at + found;
				}
				return -1;
			}

			public int IndexOf(int from, char stop0, char stop1, char stop2)
			{
				for (var at = from; Ensure(at, 1); at = _count)
				{
					var found = global::System.MemoryExtensions.IndexOfAny(Held(at), stop0, stop1, stop2);
					if (found >= 0) return at + found;
				}
				return -1;
			}

			public int IndexOf(int from, global::System.ReadOnlySpan<char> stops)
			{
				for (var at = from; Ensure(at, 1); at = _count)
				{
					var found = global::System.MemoryExtensions.IndexOfAny(Held(at), stops);
					if (found >= 0) return at + found;
				}
				return -1;
			}

			// What has been read from `at` on, which Ensure has just said holds at least one.
			private global::System.ReadOnlySpan<char> Held(int at) =>
				new global::System.ReadOnlySpan<char>(_buffer, at - _start, _count - at);
		}

		[global::System.Diagnostics.Conditional("DOTGRAM_TRACE")]
		static void Trace(string action, int state, int position, int arena, BufferedText text, string rule)
		{
			Trace(action, state, position, arena);
		}

		// The end of the trace hook, which a file with no engine goes without (CSharpEmitter).
		""";
}
