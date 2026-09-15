using System;
using System.Collections.Generic;
using System.IO;

namespace DotGram.Finance.Fix;

public static partial class Fix44
{
	/// <summary>Default maximum size of one streamed message, in octets.</summary>
	public const int DefaultMaxMessageLength = 16 * 1024 * 1024;

	/// <summary>Read exactly one message without closing or reading beyond it.</summary>
	public static FixMessage Parse(TextReader input, FixParseMode mode = FixParseMode.Strict, int maxMessageLength = DefaultMaxMessageLength)
	{
		if (TryParse(input, out var message, out var error, mode, maxMessageLength)) return message!;
		throw new FormatException(error!.ToString());
	}

	/// <summary>Read one message; malformed or incomplete input returns a diagnostic. I/O exceptions propagate.</summary>
	public static bool TryParse(TextReader input, out FixMessage? message, out FixParseError? error, FixParseMode mode = FixParseMode.Strict, int maxMessageLength = DefaultMaxMessageLength)
	{
		if (input == null) throw new ArgumentNullException(nameof(input));
		ValidateStreamArguments(mode, maxMessageLength);
		message = null;
		var reader = new FrameReader(input, maxMessageLength);
		if (!reader.TryRead(out var wire, out error))
		{
			if (error == null) Fail(0, null, null, "Expected a FIX message.", out error);
			return false;
		}
		return TryParseCore(wire, out message, out error, mode, null);
	}

	/// <summary>Read concatenated messages, reusing a frame buffer. The caller owns the input.</summary>
	public static IEnumerable<FixMessage> ReadMessages(TextReader input, FixParseMode mode = FixParseMode.Strict, int maxMessageLength = DefaultMaxMessageLength)
	{
		if (input == null) throw new ArgumentNullException(nameof(input));
		ValidateStreamArguments(mode, maxMessageLength);
		return ReadFrames(new FrameReader(input, maxMessageLength), mode, null);
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
		ValidateStreamArguments(options.Mode, maxMessageLength);
		message = null;
		var reader = new FrameReader(input, maxMessageLength);
		if (!reader.TryRead(out var wire, out error))
		{
			if (error == null) Fail(0, null, null, "Expected a FIX message.", out error);
			return false;
		}
		return TryParseCore(wire, out message, out error, options.Mode, options);
	}

	/// <summary>Read concatenated messages, reusing a frame buffer. The caller owns the input.</summary>
	public static IEnumerable<FixMessage> ReadMessages(TextReader input, FixParseOptions options, int maxMessageLength = DefaultMaxMessageLength)
	{
		if (input == null) throw new ArgumentNullException(nameof(input));
		if (options == null) throw new ArgumentNullException(nameof(options));
		ValidateStreamArguments(options.Mode, maxMessageLength);
		return ReadFrames(new FrameReader(input, maxMessageLength), options.Mode, options);
	}

	/// <summary>Read exactly one message without closing or reading beyond it.</summary>
	public static FixMessage Parse(Stream input, FixParseMode mode = FixParseMode.Strict, int maxMessageLength = DefaultMaxMessageLength)
	{
		if (TryParse(input, out var message, out var error, mode, maxMessageLength)) return message!;
		throw new FormatException(error!.ToString());
	}

	/// <summary>Read one message; malformed or incomplete input returns a diagnostic. I/O exceptions propagate.</summary>
	public static bool TryParse(Stream input, out FixMessage? message, out FixParseError? error, FixParseMode mode = FixParseMode.Strict, int maxMessageLength = DefaultMaxMessageLength)
	{
		if (input == null) throw new ArgumentNullException(nameof(input));
		ValidateStreamArguments(mode, maxMessageLength);
		message = null;
		var reader = new FrameReader(new OctetReader(input), maxMessageLength);
		if (!reader.TryRead(out var wire, out error))
		{
			if (error == null) Fail(0, null, null, "Expected a FIX message.", out error);
			return false;
		}
		return TryParseCore(wire, out message, out error, mode, null);
	}

	/// <summary>Read concatenated messages, reusing a frame buffer. The caller owns the input.</summary>
	public static IEnumerable<FixMessage> ReadMessages(Stream input, FixParseMode mode = FixParseMode.Strict, int maxMessageLength = DefaultMaxMessageLength)
	{
		if (input == null) throw new ArgumentNullException(nameof(input));
		ValidateStreamArguments(mode, maxMessageLength);
		return ReadFrames(new FrameReader(new OctetReader(input), maxMessageLength), mode, null);
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
		ValidateStreamArguments(options.Mode, maxMessageLength);
		message = null;
		var reader = new FrameReader(new OctetReader(input), maxMessageLength);
		if (!reader.TryRead(out var wire, out error))
		{
			if (error == null) Fail(0, null, null, "Expected a FIX message.", out error);
			return false;
		}
		return TryParseCore(wire, out message, out error, options.Mode, options);
	}

	/// <summary>Read concatenated messages, reusing a frame buffer. The caller owns the input.</summary>
	public static IEnumerable<FixMessage> ReadMessages(Stream input, FixParseOptions options, int maxMessageLength = DefaultMaxMessageLength)
	{
		if (input == null) throw new ArgumentNullException(nameof(input));
		if (options == null) throw new ArgumentNullException(nameof(options));
		ValidateStreamArguments(options.Mode, maxMessageLength);
		return ReadFrames(new FrameReader(new OctetReader(input), maxMessageLength), options.Mode, options);
	}

	static void ValidateStreamArguments(FixParseMode mode, int maxMessageLength)
	{
		if (mode != FixParseMode.Strict && mode != FixParseMode.Lenient) throw new ArgumentOutOfRangeException(nameof(mode));
		if (maxMessageLength < 1) throw new ArgumentOutOfRangeException(nameof(maxMessageLength));
	}

	static IEnumerable<FixMessage> ReadFrames(FrameReader reader, FixParseMode mode, FixParseOptions? options)
	{
		while (true)
		{
			if (!reader.TryRead(out var wire, out var error))
			{
				if (error != null) throw new FormatException(error.ToString());
				yield break;
			}
			if (!TryParseCore(wire, out var message, out error, mode, options)) throw new FormatException(error!.ToString());
			yield return message!;
		}
	}

	sealed class FrameReader
	{
		const string Prefix = "8=FIX.4.4\u00019=";
		readonly TextReader input;
		readonly int maximum;
		char[] buffer;
		int count;

		public FrameReader(TextReader input, int maximum)
		{
			this.input = input;
			this.maximum = maximum;
			buffer = new char[Math.Min(4096, maximum)];
		}

		bool ReadTo(int target)
		{
			while (count < target)
			{
				if (count == buffer.Length)
					Array.Resize(ref buffer, (int)Math.Min(maximum, Math.Max((long)count + 1, (long)count * 2)));
				var read = input.Read(buffer, count, Math.Min(target - count, buffer.Length - count));
				if (read == 0) return false;
				count += read;
			}
			return true;
		}

		public bool TryRead(out string? wire, out FixParseError? error)
		{
			wire = null;
			error = null;
			count = 0;
			if (!ReadTo(1)) return false;
			if (maximum < Prefix.Length) return Fail(count, 9, null, "Message exceeds maxMessageLength.", out error);
			if (!ReadTo(Prefix.Length)) return Fail(count, 8, null, "Truncated FIX header.", out error);
			for (var i = 0; i < Prefix.Length; i++)
				if (buffer[i] != Prefix[i]) return Fail(i, 8, null, "Expected BeginString FIX.4.4 followed by BodyLength.", out error);
			var length = 0;
			var digits = 0;
			while (true)
			{
				if (count == maximum) return Fail(count, 9, null, "Message exceeds maxMessageLength.", out error);
				if (!ReadTo(count + 1)) return Fail(count, 9, null, "Truncated BodyLength.", out error);
				var c = buffer[count - 1];
				if (c == '\u0001' && digits != 0) break;
				if (c < '0' || c > '9') return Fail(count - 1, 9, null, "BodyLength must contain decimal digits.", out error);
				if (length > (maximum - (c - '0')) / 10 || c - '0' > maximum)
					return Fail(count - 1, 9, null, "Message exceeds maxMessageLength.", out error);
				length = length * 10 + c - '0';
				digits++;
			}
			if ((long)count + length + 7 > maximum) return Fail(count, 9, null, "Message exceeds maxMessageLength.", out error);
			if (!ReadTo(count + length + 7)) return Fail(count, null, null, "Truncated FIX message.", out error);
			wire = new string(buffer, 0, count);
			return true;
		}
	}

	// Lossless octet mapping, without decoding or read-ahead into the next frame.
	sealed class OctetReader : TextReader
	{
		readonly Stream input;
		readonly byte[] buffer = new byte[4096];
		public OctetReader(Stream input) => this.input = input;
		public override int Read(char[] target, int index, int count)
		{
			var read = input.Read(buffer, 0, Math.Min(count, buffer.Length));
			for (var i = 0; i < read; i++) target[index + i] = (char)buffer[i];
			return read;
		}
	}
}
