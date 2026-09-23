using System;

namespace DotGram.Finance.Fix;

static partial class FixMessages
{
	/// <summary>Default maximum size of one streamed message, in octets.</summary>
	public const int DefaultMaxMessageLength = 16 * 1024 * 1024;

	/// <summary>Reads exactly one message without closing the input or reading beyond it.</summary>
	/// <param name="input">The input.</param>
	/// <param name="context">Null reads wire framing with the standard length/data dictionary.</param>
	/// <param name="maxMessageLength">The largest message that will be read, in octets.</param>
	/// <exception cref="FormatException">The input does not begin with a message.</exception>
	public static FixMessage Parse(TextReader input, FixContext? context = null, int maxMessageLength = DefaultMaxMessageLength)
	{
		if (TryParse(input, out var message, out var error, context, maxMessageLength)) return message!;
		throw new FormatException(error!.ToString());
	}

	/// <summary>Reads one message; malformed or incomplete input answers with a diagnostic.</summary>
	/// <param name="input">The input.</param>
	/// <param name="message">The message read, or null.</param>
	/// <param name="error">The first problem found, or null.</param>
	/// <param name="context">Null reads wire framing with the standard length/data dictionary.</param>
	/// <param name="maxMessageLength">The largest message that will be read, in octets.</param>
	/// <returns>False with the first problem found in <paramref name="error"/>. I/O exceptions propagate.</returns>
	public static bool TryParse(TextReader input, out FixMessage? message, out FixParseError? error, FixContext? context = null, int maxMessageLength = DefaultMaxMessageLength)
	{
		if (input == null) throw new ArgumentNullException(nameof(input));

		ValidateStreamArguments(maxMessageLength);

		message = null;

		var reader = new FrameReader(input, maxMessageLength, (context?.Framing ?? FixFraming.Wire).Separator());

		if (!reader.TryRead(out var wire, out error))
		{
			if (error == null)
				Fail(0, null, null, "Expected a FIX message.", out error);
			return false;
		}

		return TryParseFrame(wire, out message, out error, context);
	}

	/// <summary>Reads concatenated messages, reusing a frame buffer. The caller owns the input.</summary>
	/// <param name="input">The input.</param>
	/// <param name="context">Null reads wire framing with the standard length/data dictionary.</param>
	/// <param name="maxMessageLength">The largest message that will be read, in octets.</param>
	public static IEnumerable<FixMessage> ReadMessages(TextReader input, FixContext? context = null, int maxMessageLength = DefaultMaxMessageLength)
	{
		if (input == null) throw new ArgumentNullException(nameof(input));

		ValidateStreamArguments(maxMessageLength);

		return ReadFrames(new FrameReader(input, maxMessageLength, (context?.Framing ?? FixFraming.Wire).Separator()), context);
	}

	/// <summary>Reads exactly one message without closing the input or reading beyond it.</summary>
	/// <param name="input">The input.</param>
	/// <param name="context">Null reads wire framing with the standard length/data dictionary.</param>
	/// <param name="maxMessageLength">The largest message that will be read, in octets.</param>
	/// <exception cref="FormatException">The input does not begin with a message.</exception>
	public static FixMessage Parse(Stream input, FixContext? context = null, int maxMessageLength = DefaultMaxMessageLength)
	{
		if (TryParse(input, out var message, out var error, context, maxMessageLength))
			return message!;

		throw new FormatException(error!.ToString());
	}

	/// <summary>Reads one message; malformed or incomplete input answers with a diagnostic.</summary>
	/// <param name="input">The input.</param>
	/// <param name="message">The message read, or null.</param>
	/// <param name="error">The first problem found, or null.</param>
	/// <param name="context">Null reads wire framing with the standard length/data dictionary.</param>
	/// <param name="maxMessageLength">The largest message that will be read, in octets.</param>
	/// <returns>False with the first problem found in <paramref name="error"/>. I/O exceptions propagate.</returns>
	public static bool TryParse(Stream input, out FixMessage? message, out FixParseError? error, FixContext? context = null, int maxMessageLength = DefaultMaxMessageLength)
	{
		if (input == null) throw new ArgumentNullException(nameof(input));

		ValidateStreamArguments(maxMessageLength);

		message = null;

		var reader = new FrameReader(input, maxMessageLength, (context?.Framing ?? FixFraming.Wire).Separator());

		if (!reader.TryRead(out var wire, out error))
		{
			if (error == null)
				Fail(0, null, null, "Expected a FIX message.", out error);
			return false;
		}

		return TryParseFrame(wire, out message, out error, context);
	}

	/// <summary>Reads concatenated messages, reusing a frame buffer. The caller owns the input.</summary>
	/// <param name="input">The input.</param>
	/// <param name="context">Null reads wire framing with the standard length/data dictionary.</param>
	/// <param name="maxMessageLength">The largest message that will be read, in octets.</param>
	public static IEnumerable<FixMessage> ReadMessages(Stream input, FixContext? context = null, int maxMessageLength = DefaultMaxMessageLength)
	{
		if (input == null) throw new ArgumentNullException(nameof(input));

		ValidateStreamArguments(maxMessageLength);

		return ReadFrames(new FrameReader(input, maxMessageLength, (context?.Framing ?? FixFraming.Wire).Separator()), context);
	}

	static void ValidateStreamArguments(int maxMessageLength)
	{
		if (maxMessageLength < 1) throw new ArgumentOutOfRangeException(nameof(maxMessageLength));
	}

	static IEnumerable<FixMessage> ReadFrames(FrameReader reader, FixContext? context)
	{
		while (true)
		{
			if (!reader.TryRead(out var wire, out var error))
			{
				if (error != null)
					throw new FormatException(error.ToString());
				yield break;
			}

			if (!TryParseFrame(wire, out var message, out error, context))
				throw new FormatException(error!.ToString());

			yield return message!;
		}
	}

	readonly struct Frame
	{
		public Frame(string? text, byte[]? bytes)
		{
			Text  = text;
			Bytes = bytes;
		}

		public string? Text  { get; }
		public byte[]? Bytes { get; }
	}

	static bool TryParseFrame(Frame frame, out FixMessage? message, out FixParseError? error, FixContext? context)
	{
		return frame.Bytes != null
			? TryParseBytes(frame.Bytes, out message, out error, context)
			: TryParseCore(frame.Text, out message, out error, context);
	}

	sealed class FrameReader
	{
		readonly string      _prefix;
		readonly char        _separator;
		readonly TextReader? _textInput;
		readonly Stream?     _byteInput;
		readonly int         _maximum;
		char[]?              _buffer;
		byte[]?              _byteBuffer;
		int                  _count;

		public FrameReader(TextReader input, int maximum, char separator = '\u0001')
		{
			_textInput = input;
			_separator = separator;
			_prefix    = "8=FIX.4.4" + separator + "9=";
			_maximum   = maximum;
			_buffer    = new char[Math.Min(4096, maximum)];
		}

		public FrameReader(Stream input, int maximum, char separator = '\u0001')
		{
			_byteInput  = input;
			_separator  = separator;
			_prefix     = "8=FIX.4.4" + separator + "9=";
			_maximum    = maximum;
			_byteBuffer = new byte[Math.Min(4096, maximum)];
		}

		int At(int index)
		{
			return _byteBuffer != null ? _byteBuffer[index] : _buffer![index];
		}

		bool ReadTo(int target)
		{
			while (_count < target)
			{
				var capacity = _byteBuffer?.Length ?? _buffer!.Length;

				if (_count == capacity)
				{
					capacity = (int)Math.Min(_maximum, Math.Max((long)_count + 1, (long)_count * 2));

					if (_byteBuffer != null)
						Array.Resize(ref _byteBuffer, capacity);
					else
						Array.Resize(ref _buffer, capacity);
				}

				var wanted = Math.Min(target - _count, capacity - _count);
				var read   = _byteInput?.Read(_byteBuffer!, _count, wanted) ?? _textInput!.Read(_buffer!, _count, wanted);

				if (read == 0)
					return false;

				_count += read;
			}

			return true;
		}

		public bool TryRead(out Frame wire, out FixParseError? error)
		{
			wire   = default;
			error  = null;
			_count = 0;

			if (!ReadTo(1))
				return false;

			if (_maximum < _prefix.Length)
				return Fail(_count, 9, null, "Message exceeds maxMessageLength.", out error);

			if (!ReadTo(_prefix.Length))
				return Fail(_count, 8, null, "Truncated FIX header.", out error);

			for (var i = 0; i < _prefix.Length; i++)
				if (At(i) != _prefix[i])
					return Fail(i, 8, null, "Expected BeginString FIX.4.4 followed by BodyLength.", out error);

			var length = 0;
			var digits = 0;

			while (true)
			{
				if (_count == _maximum)
					return Fail(_count, 9, null, "Message exceeds maxMessageLength.", out error);

				if (!ReadTo(_count + 1))
					return Fail(_count, 9, null, "Truncated BodyLength.", out error);

				var c = At(_count - 1);

				if (c == _separator && digits != 0)
					break;

				if (c < '0' || c > '9')
					return Fail(_count - 1, 9, null, "BodyLength must contain decimal digits.", out error);

				if (length > (_maximum - (c - '0')) / 10 || c - '0' > _maximum)
					return Fail(_count - 1, 9, null, "Message exceeds maxMessageLength.", out error);

				length = length * 10 + c - '0';
				digits++;
			}

			if ((long)_count + length + 7 > _maximum)
				return Fail(_count, 9, null, "Message exceeds maxMessageLength.", out error);

			if (!ReadTo(_count + length + 7))
				return Fail(_count, null, null, "Truncated FIX message.", out error);

			wire = _byteBuffer != null ? new Frame(null, [.. _byteBuffer.AsSpan(0, _count)]) : new Frame(new string(_buffer!, 0, _count), null);

			return true;
		}
	}
}
