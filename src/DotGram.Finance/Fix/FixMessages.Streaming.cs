using System;

namespace DotGram.Finance.Fix;

public static partial class FixMessages
{
	/// <summary>Default maximum size of one streamed message, in octets.</summary>
	public const int DefaultMaxMessageLength = 16 * 1024 * 1024;

	/// <summary>Read exactly one message without closing or reading beyond it.</summary>
	public static FixMessage Parse(TextReader input, int maxMessageLength = DefaultMaxMessageLength)
	{
		if (TryParse(input, out var message, out var error, maxMessageLength)) return message!;
		throw new FormatException(error!.ToString());
	}

	/// <summary>Read one message; malformed or incomplete input returns a diagnostic. I/O exceptions propagate.</summary>
	public static bool TryParse(TextReader input, out FixMessage? message, out FixParseError? error, int maxMessageLength = DefaultMaxMessageLength)
	{
		if (input == null) throw new ArgumentNullException(nameof(input));
		ValidateStreamArguments(maxMessageLength);
		message = null;
		var reader = new FrameReader(input, maxMessageLength);
		if (!reader.TryRead(out var wire, out error))
		{
			if (error == null) Fail(0, null, null, "Expected a FIX message.", out error);
			return false;
		}
		return TryParseFrame(wire, out message, out error, null);
	}

	/// <summary>Read concatenated messages, reusing a frame buffer. The caller owns the input.</summary>
	public static IEnumerable<FixMessage> ReadMessages(TextReader input, int maxMessageLength = DefaultMaxMessageLength)
	{
		if (input == null) throw new ArgumentNullException(nameof(input));
		ValidateStreamArguments(maxMessageLength);
		return ReadFrames(new FrameReader(input, maxMessageLength), null);
	}

	/// <summary>Read exactly one message without closing or reading beyond it.</summary>
	public static FixMessage Parse(TextReader input, FixParseOptions options, int maxMessageLength = DefaultMaxMessageLength)
	{
		if (TryParse(input, out var message, out var error, options, maxMessageLength)) return message!;
		throw new FormatException(error!.ToString());
	}

	/// <summary>Read one message; malformed or incomplete input returns a diagnostic. I/O exceptions propagate.</summary>
	public static bool TryParse(TextReader input, out FixMessage? message, out FixParseError? error, FixParseOptions options, int maxMessageLength = DefaultMaxMessageLength)
	{
		if (input == null) throw new ArgumentNullException(nameof(input));
		if (options == null) throw new ArgumentNullException(nameof(options));
		ValidateStreamArguments(maxMessageLength);
		message = null;
		var reader = new FrameReader(input, maxMessageLength, options.Framing.Separator());
		if (!reader.TryRead(out var wire, out error))
		{
			if (error == null) Fail(0, null, null, "Expected a FIX message.", out error);
			return false;
		}
		return TryParseFrame(wire, out message, out error, options);
	}

	/// <summary>Read concatenated messages, reusing a frame buffer. The caller owns the input.</summary>
	public static IEnumerable<FixMessage> ReadMessages(TextReader input, FixParseOptions options, int maxMessageLength = DefaultMaxMessageLength)
	{
		if (input == null) throw new ArgumentNullException(nameof(input));
		if (options == null) throw new ArgumentNullException(nameof(options));
		ValidateStreamArguments(maxMessageLength);
		return ReadFrames(new FrameReader(input, maxMessageLength, options.Framing.Separator()), options);
	}

	/// <summary>Read exactly one message without closing or reading beyond it.</summary>
	public static FixMessage Parse(Stream input, int maxMessageLength = DefaultMaxMessageLength)
	{
		if (TryParse(input, out var message, out var error, maxMessageLength)) return message!;
		throw new FormatException(error!.ToString());
	}

	/// <summary>Read one message; malformed or incomplete input returns a diagnostic. I/O exceptions propagate.</summary>
	public static bool TryParse(Stream input, out FixMessage? message, out FixParseError? error, int maxMessageLength = DefaultMaxMessageLength)
	{
		if (input == null) throw new ArgumentNullException(nameof(input));
		ValidateStreamArguments(maxMessageLength);
		message = null;
		var reader = new FrameReader(input, maxMessageLength);
		if (!reader.TryRead(out var wire, out error))
		{
			if (error == null) Fail(0, null, null, "Expected a FIX message.", out error);
			return false;
		}
		return TryParseFrame(wire, out message, out error, null);
	}

	/// <summary>Read concatenated messages, reusing a frame buffer. The caller owns the input.</summary>
	public static IEnumerable<FixMessage> ReadMessages(Stream input, int maxMessageLength = DefaultMaxMessageLength)
	{
		if (input == null) throw new ArgumentNullException(nameof(input));
		ValidateStreamArguments(maxMessageLength);
		return ReadFrames(new FrameReader(input, maxMessageLength), null);
	}

	/// <summary>Read exactly one message without closing or reading beyond it.</summary>
	public static FixMessage Parse(Stream input, FixParseOptions options, int maxMessageLength = DefaultMaxMessageLength)
	{
		if (TryParse(input, out var message, out var error, options, maxMessageLength)) return message!;
		throw new FormatException(error!.ToString());
	}

	/// <summary>Read one message; malformed or incomplete input returns a diagnostic. I/O exceptions propagate.</summary>
	public static bool TryParse(Stream input, out FixMessage? message, out FixParseError? error, FixParseOptions options, int maxMessageLength = DefaultMaxMessageLength)
	{
		if (input == null) throw new ArgumentNullException(nameof(input));
		if (options == null) throw new ArgumentNullException(nameof(options));
		ValidateStreamArguments(maxMessageLength);
		message = null;
		var reader = new FrameReader(input, maxMessageLength, options.Framing.Separator());
		if (!reader.TryRead(out var wire, out error))
		{
			if (error == null) Fail(0, null, null, "Expected a FIX message.", out error);
			return false;
		}
		return TryParseFrame(wire, out message, out error, options);
	}

	/// <summary>Read concatenated messages, reusing a frame buffer. The caller owns the input.</summary>
	public static IEnumerable<FixMessage> ReadMessages(Stream input, FixParseOptions options, int maxMessageLength = DefaultMaxMessageLength)
	{
		if (input == null) throw new ArgumentNullException(nameof(input));
		if (options == null) throw new ArgumentNullException(nameof(options));
		ValidateStreamArguments(maxMessageLength);
		return ReadFrames(new FrameReader(input, maxMessageLength, options.Framing.Separator()), options);
	}

	static void ValidateStreamArguments(int maxMessageLength)
	{
		if (maxMessageLength < 1) throw new ArgumentOutOfRangeException(nameof(maxMessageLength));
	}

	static IEnumerable<FixMessage> ReadFrames(FrameReader reader, FixParseOptions? options)
	{
		while (true)
		{
			if (!reader.TryRead(out var wire, out var error))
			{
				if (error != null) throw new FormatException(error.ToString());
				yield break;
			}
			if (!TryParseFrame(wire, out var message, out error, options)) throw new FormatException(error!.ToString());
			yield return message!;
		}
	}

	readonly struct Frame
	{
		public Frame(string? text, byte[]? bytes) { Text = text; Bytes = bytes; }
		public string? Text { get; }
		public byte[]? Bytes { get; }
	}

	static bool TryParseFrame(Frame frame, out FixMessage? message, out FixParseError? error, FixParseOptions? options)
		=> frame.Bytes != null ? TryParseBytes(frame.Bytes, out message, out error, options) : TryParseCore(frame.Text, out message, out error, options);

	sealed class FrameReader
	{
		readonly string prefix;
		readonly char separator;
		readonly TextReader? textInput;
		readonly Stream? byteInput;
		readonly int maximum;
		char[]? buffer;
		byte[]? byteBuffer;
		int count;

		public FrameReader(TextReader input, int maximum, char separator = '\u0001')
		{
			textInput = input;
			this.separator = separator;
			prefix = "8=FIX.4.4" + separator + "9=";
			this.maximum = maximum;
			buffer = new char[Math.Min(4096, maximum)];
		}

		public FrameReader(Stream input, int maximum, char separator = '\u0001')
		{
			byteInput = input;
			this.separator = separator;
			prefix = "8=FIX.4.4" + separator + "9=";
			this.maximum = maximum;
			byteBuffer = new byte[Math.Min(4096, maximum)];
		}
		int At(int index) => byteBuffer != null ? byteBuffer[index] : buffer![index];
		bool ReadTo(int target)
		{
			while (count < target)
			{
				var capacity = byteBuffer?.Length ?? buffer!.Length;
				if (count == capacity)
				{
					capacity = (int)Math.Min(maximum, Math.Max((long)count + 1, (long)count * 2));
					if (byteBuffer != null) Array.Resize(ref byteBuffer, capacity);
					else Array.Resize(ref buffer, capacity);
				}
				var wanted = Math.Min(target - count, capacity - count);
				var read = byteInput != null ? byteInput.Read(byteBuffer!, count, wanted) : textInput!.Read(buffer!, count, wanted);
				if (read == 0) return false;
				count += read;
			}
			return true;
		}

		public bool TryRead(out Frame wire, out FixParseError? error)
		{
			wire = default;
			error = null;
			count = 0;
			if (!ReadTo(1)) return false;
			if (maximum < prefix.Length) return Fail(count, 9, null, "Message exceeds maxMessageLength.", out error);
			if (!ReadTo(prefix.Length)) return Fail(count, 8, null, "Truncated FIX header.", out error);
			for (var i = 0; i < prefix.Length; i++)
				if (At(i) != prefix[i]) return Fail(i, 8, null, "Expected BeginString FIX.4.4 followed by BodyLength.", out error);
			var length = 0;
			var digits = 0;
			while (true)
			{
				if (count == maximum) return Fail(count, 9, null, "Message exceeds maxMessageLength.", out error);
				if (!ReadTo(count + 1)) return Fail(count, 9, null, "Truncated BodyLength.", out error);
				var c = At(count - 1);
				if (c == separator && digits != 0) break;
				if (c < '0' || c > '9') return Fail(count - 1, 9, null, "BodyLength must contain decimal digits.", out error);
				if (length > (maximum - (c - '0')) / 10 || c - '0' > maximum)
					return Fail(count - 1, 9, null, "Message exceeds maxMessageLength.", out error);
				length = length * 10 + c - '0';
				digits++;
			}
			if ((long)count + length + 7 > maximum) return Fail(count, 9, null, "Message exceeds maxMessageLength.", out error);
			if (!ReadTo(count + length + 7)) return Fail(count, null, null, "Truncated FIX message.", out error);
			wire = byteBuffer != null ? new Frame(null, byteBuffer.AsSpan(0, count).ToArray()) : new Frame(new string(buffer!, 0, count), null);
			return true;
		}
	}

}
