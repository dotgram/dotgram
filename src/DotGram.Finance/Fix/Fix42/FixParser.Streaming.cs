using System;
using System.Diagnostics.CodeAnalysis;

namespace DotGram.Finance.Fix.Fix42;

// Written by generate.cs from the FIX 4.2 repository; not edited by hand. From Templates/FixParser.Streaming.cs.in.
// Derived from the FIX Protocol specification (FIX Unified Repository, 2010 edition), Copyright FIX Protocol Limited, https://www.fixtrading.org.

public static partial class FixParser
{
	static void ValidateStreamArguments(int maxMessageLength)
	{
		if (maxMessageLength < 1) throw new ArgumentOutOfRangeException(nameof(maxMessageLength));
	}

	static IEnumerable<FixMessage> ReadFrames(FrameReader reader, Fix42Context? context)
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

	static bool TryParseFrame(Frame frame, [NotNullWhen(true)] out FixMessage? message, [NotNullWhen(false)] out FixParseError? error, Fix42Context? context)
	{
		return frame.Bytes != null
			? TryParseBytes(frame.Bytes, out message, out error, context)
			: TryParseText(frame.Text, out message, out error, context);
	}

	sealed class FrameReader
	{
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
			_maximum   = maximum;
			_buffer    = new char[Math.Min(4096, maximum)];
		}

		public FrameReader(Stream input, int maximum, char separator = '\u0001')
		{
			_byteInput  = input;
			_separator  = separator;
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

		// The shortest frame header: 8=X, the separator and 9=.
		const int MinHeader = 6;

		public bool TryRead(out Frame wire, out FixParseError? error)
		{
			wire   = default;
			error  = null;
			_count = 0;

			if (!ReadTo(1))
				return false;

			if (_maximum < MinHeader)
				return Fail(_count, FixTag.BodyLength, null, "Message exceeds maxMessageLength.", out error);

			// Whatever BeginString the frame names: it is cut by BodyLength as any other, and a version
			// not this one's is a finding of Validate, as it is for a buffer.
			if (!ReadTo(2))
				return Fail(_count, FixTag.BeginString, null, "Truncated FIX header.", out error);

			if (At(0) != '8' || At(1) != '=')
				return Fail(0, FixTag.BeginString, null, "Expected BeginString followed by BodyLength.", out error);

			var at = 2;

			while (true)
			{
				// The BeginString is bounded as the whole message is, not by a length of its own.
				if (at >= _maximum)
					return Fail(at, FixTag.BeginString, null, "Message exceeds maxMessageLength.", out error);

				if (!ReadTo(at + 1))
					return Fail(_count, FixTag.BeginString, null, "Truncated FIX header.", out error);

				var c = At(at);

				if (c == _separator && at > 2)
					break;

				if (c == _separator)
					return Fail(at, FixTag.BeginString, null, "Expected BeginString followed by BodyLength.", out error);

				at++;
			}

			if (!ReadTo(at + 3))
				return Fail(_count, FixTag.BodyLength, null, "Truncated FIX header.", out error);

			if (At(at + 1) != '9' || At(at + 2) != '=')
				return Fail(at + 1, FixTag.BodyLength, null, "Expected BeginString followed by BodyLength.", out error);

			var length = 0;
			var digits = 0;

			while (true)
			{
				if (_count == _maximum)
					return Fail(_count, FixTag.BodyLength, null, "Message exceeds maxMessageLength.", out error);

				if (!ReadTo(_count + 1))
					return Fail(_count, FixTag.BodyLength, null, "Truncated BodyLength.", out error);

				var c = At(_count - 1);

				if (c == _separator && digits != 0)
					break;

				if (c < '0' || c > '9')
					return Fail(_count - 1, FixTag.BodyLength, null, "BodyLength must contain decimal digits.", out error);

				if (length > (_maximum - (c - '0')) / 10 || c - '0' > _maximum)
					return Fail(_count - 1, FixTag.BodyLength, null, "Message exceeds maxMessageLength.", out error);

				length = length * 10 + c - '0';
				digits++;
			}

			if ((long)_count + length + 7 > _maximum)
				return Fail(_count, FixTag.BodyLength, null, "Message exceeds maxMessageLength.", out error);

			if (!ReadTo(_count + length + 7))
				return Fail(_count, null, null, "Truncated FIX message.", out error);

			wire = _byteBuffer != null ? new Frame(null, [.. _byteBuffer.AsSpan(0, _count)]) : new Frame(new string(_buffer!, 0, _count), null);

			return true;
		}
	}
}
