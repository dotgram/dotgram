using System;
using System.Runtime.InteropServices;

using DotGram.Finance.Fix44;

namespace DotGram.Handwritten.Fix;

/// <summary>
/// Independent FIX field reader sharing only the field model and primitive conversions.
/// </summary>
public static class HandFixParser
{
	public static FixField[] Parse(string input, FixContext? options = null)
	{
		ArgumentNullException.ThrowIfNull(input);
		return Read(new Input<char>(input.AsMemory()), false, options).ToArray();
	}

	/// <summary>
	/// Reads wire fields separated by SOH from a copy of the input.
	/// </summary>
	/// <remarks>
	/// The parser reads strings, so the input is copied into one first; no returned field refers to the copy.
	/// </remarks>
	public static FixField[] Parse(ReadOnlySpan<char> input, FixContext? options = null)
	{
		return Parse(input.ToString(), options);
	}

	public static FixField[] Parse(byte[] input, FixContext? options = null)
	{
		ArgumentNullException.ThrowIfNull(input);
		return Read(new Input<byte>(input), false, options).ToArray();
	}

	public static IEnumerable<FixField> Parse(TextReader input, FixContext? options = null)
	{
		ArgumentNullException.ThrowIfNull(input);
		var settings = options ?? FixContext.Default;
		return ReadText(input, false, options, settings.BufferSize, settings.MaxRetained);
	}

	public static IEnumerable<FixField> Parse(Stream input, FixContext? options = null)
	{
		ArgumentNullException.ThrowIfNull(input);
		var settings = options ?? FixContext.Default;
		return ReadBytes(input, false, options, settings.BufferSize, settings.MaxRetained);
	}

	public static FixField[] ParseLog(string input, FixContext? options = null)
	{
		ArgumentNullException.ThrowIfNull(input);
		return Read(new Input<char>(input.AsMemory()), true, options).ToArray();
	}

	/// <summary>
	/// Reads log fields separated by a pipe with optional surrounding spaces from a copy of the input.
	/// </summary>
	/// <remarks>
	/// The parser reads strings, so the input is copied into one first; no returned field refers to the copy.
	/// </remarks>
	public static FixField[] ParseLog(ReadOnlySpan<char> input, FixContext? options = null)
	{
		return ParseLog(input.ToString(), options);
	}

	public static FixField[] ParseLog(byte[] input, FixContext? options = null)
	{
		ArgumentNullException.ThrowIfNull(input);
		return Read(new Input<byte>(input), true, options).ToArray();
	}

	public static IEnumerable<FixField> ParseLog(TextReader input, FixContext? options = null)
	{
		ArgumentNullException.ThrowIfNull(input);
		var settings = options ?? FixContext.Default;
		return ReadText(input, true, options, settings.BufferSize, settings.MaxRetained);
	}

	public static IEnumerable<FixField> ParseLog(Stream input, FixContext? options = null)
	{
		ArgumentNullException.ThrowIfNull(input);
		var settings = options ?? FixContext.Default;
		return ReadBytes(input, true, options, settings.BufferSize, settings.MaxRetained);
	}

	static IEnumerable<FixField> ReadText(TextReader input, bool log, FixContext? options, int bufferSize, int maxRetained)
	{
		foreach (var field in Read(new Input<char>(input, null, bufferSize, maxRetained), log, options))
			yield return field;
	}

	static IEnumerable<FixField> ReadBytes(Stream input, bool log, FixContext? options, int bufferSize, int maxRetained)
	{
		foreach (var field in Read(new Input<byte>(null, input, bufferSize, maxRetained), log, options))
			yield return field;
	}

	static IEnumerable<FixField> Read<T>(Input<T> input, bool log, FixContext? options)
		where T : unmanaged
	{
		options ??= FixContext.Default;
		var position = 0;
		var pair     = new Pair();

		while (input.Peek(position) >= 0)
		{
			var start = position;
			var field = Field(input, log, options, ref pair, ref position, out var error);

			if (field == null)
			{
				// Recovery starts at the failed field.
				var end = FindSeparator(input, start, log);
				position = end;
				Separator(input, log, ref position);
				field = Invalid(input, start, end, error!);
			}

			input.Discard(position);
			yield return field;
		}
	}

	// The data tag a length field has announced, and how long its data is: the next field only.
	struct Pair
	{
		public int Expected;
		public int Size;
	}

	static FixField? Field<T>(Input<T> input, bool log, FixContext options, ref Pair pair, ref int position, out string? error)
		where T : unmanaged
	{
		var start = position;
		error = "Expected a positive tag followed by '='.";
		if (!Number(input, ref position, false, out var tag) || input.Peek(position) != '=')
			return null;

		position++;
		var valueStart = position;
		var expected   = pair.Expected;
		var kind       = options.Kind(tag);
		int end;

		pair.Expected = 0;

		if (tag == expected)
		{
			error = "The binary payload is shorter than its declared length.";
			if (pair.Size > int.MaxValue - position || (pair.Size > 0 && input.Peek(position + pair.Size - 1) < 0))
				return null;

			end = position + pair.Size;
		}
		else if (kind > 0)
		{
			// A length too large to count by is still a length; it only measures nothing.
			error = "Expected a length.";
			var fits = Number(input, ref position, true, out var length);
			if (position == valueStart)
				return null;

			end = position;

			if (fits)
			{
				pair.Size     = length;
				pair.Expected = options.DataTag(tag);
			}
		}
		else
		{
			error = "A data tag requires its preceding length tag.";
			if (kind < 0)
				return null;

			end = FindSeparator(input, position, log);
			error = "Expected a nonempty field value.";
			if (end == position)
				return null;
		}

		position = end;
		error = "Expected a separator or end of input after the field.";
		if (!Separator(input, log, ref position) && input.Peek(position) >= 0)
			return null;

		var field = Create(input, tag, valueStart, end, kind < 0, options.FixFieldFactory);
		field.WithTerminator(position - end).Locate(start, position - start);
		error = null;
		return field;
	}

	static bool Number<T>(Input<T> input, ref int position, bool zero, out int value)
		where T : unmanaged
	{
		value = 0;
		var digit = input.Peek(position);
		if (digit < (zero ? '0' : '1') || digit > '9')
			return false;

		var overflow = false;
		do
		{
			if (value > (int.MaxValue - (digit - '0')) / 10)
				overflow = true;
			else if (!overflow)
				value = value * 10 + digit - '0';
			position++;
			digit = input.Peek(position);
		}
		while (digit is >= '0' and <= '9');

		return !overflow;
	}

	static bool Separator<T>(Input<T> input, bool log, ref int position)
		where T : unmanaged
	{
		var end = position;
		if (log)
			while (input.Peek(end) == ' ')
				end++;

		if (input.Peek(end) != (log ? '|' : 1))
			return false;

		end++;
		if (log)
			while (input.Peek(end) == ' ')
				end++;

		position = end;
		return true;
	}

	static int FindSeparator<T>(Input<T> input, int position, bool log)
		where T : unmanaged
	{
		var spaces = -1;
		int next;
		while ((next = input.Peek(position)) >= 0)
		{
			if (next == (log ? '|' : 1))
				return log && spaces >= 0 ? spaces : position;

			if (log && next == ' ')
			{
				if (spaces < 0)
					spaces = position;
			}
			else
				spaces = -1;
			position++;
		}
		return position;
	}

	static FixField Create<T>(Input<T> input, int tag, int valueStart, int end, bool binary, Func<int, FixCustomField?>? custom)
		where T : unmanaged
	{
		var value = input.Slice(valueStart, end - valueStart);
		if (typeof(T) == typeof(char))
		{
			var chars = MemoryMarshal.Cast<T, char>(value);
			if (!binary)
				return FixFieldBuilder.Value(tag, chars, custom);

			return FixFieldBuilder.Binary(tag, FixConvert.Data(chars), custom);
		}
		else
		{
			var bytes = MemoryMarshal.Cast<T, byte>(value);
			if (!binary)
				return FixFieldBuilder.Value(tag, bytes, custom);

			return FixFieldBuilder.Binary(tag, FixConvert.Data(bytes), custom);
		}
	}

	static FixField Invalid<T>(Input<T> input, int start, int end, string error)
		where T : unmanaged
	{
		var raw = input.Slice(start, end - start);
		return typeof(T) == typeof(char)
			? new FixField.Invalid(MemoryMarshal.Cast<T, char>(raw), start, error)
			: new FixField.Invalid(MemoryMarshal.Cast<T, byte>(raw), start, error);
	}

	sealed class Input<T>
		where T : unmanaged
	{
		ReadOnlyMemory<T> _memory;
		T[]? _buffer;
		readonly TextReader? _reader;
		readonly Stream? _stream;
		readonly int _readSize;
		readonly int _maxRetained;
		int _start;
		int _count;
		bool _eof;

		public Input(ReadOnlyMemory<T> memory)
		{
			_memory = memory;
			_count = memory.Length;
			_eof = true;
		}

		public Input(TextReader? reader, Stream? stream, int bufferSize, int maxRetained)
		{
			_reader = reader;
			_stream = stream;
			_readSize = bufferSize;
			_maxRetained = maxRetained;
			_buffer = new T[Math.Min(bufferSize, maxRetained)];
			_memory = _buffer;
		}

		public int Peek(int position)
		{
			var offset = position - _start;
			while (offset >= _count && !_eof)
			{
				if (_count == _buffer!.Length)
				{
					if (_count == _maxRetained)
						throw new IOException($"A FIX field needs more than {_maxRetained} retained characters or bytes; pass a larger maxRetained.");

					var capacity = (int)Math.Min(_maxRetained, Math.Max((long)_count + 1, (long)_count * 2));
					Array.Resize(ref _buffer, capacity);
					_memory = _buffer;
				}

				var size = Math.Min(_readSize, _buffer.Length - _count);
				var read = _reader != null
					? _reader.Read((char[])(object)_buffer, _count, size)
					: _stream!.Read((byte[])(object)_buffer, _count, size);
				_count += read;
				_eof = read == 0;
			}

			if (offset >= _count)
				return -1;

			return typeof(T) == typeof(char)
				? MemoryMarshal.Cast<T, char>(_memory.Span)[offset]
				: MemoryMarshal.Cast<T, byte>(_memory.Span)[offset];
		}

		public ReadOnlySpan<T> Slice(int position, int length)
		{
			return _memory.Span.Slice(position - _start, length);
		}

		public void Discard(int position)
		{
			if (_buffer == null)
				return;

			var consumed = position - _start;
			Array.Copy(_buffer, consumed, _buffer, 0, _count - consumed);
			_count -= consumed;
			_start = position;
		}
	}
}
