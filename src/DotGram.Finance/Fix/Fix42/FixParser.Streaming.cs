using System;

namespace DotGram.Finance.Fix.Fix42;

// Written by generate.py from the FIX 4.2 repository; not edited by hand. From Templates/FixParser.Streaming.cs.in.
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

	static bool TryParseFrame(Frame frame, out FixMessage? message, out FixParseError? error, Fix42Context? context)
	{
		return frame.Bytes != null
			? TryParseBytes(frame.Bytes, out message, out error, context)
			: TryParseText(frame.Text, out message, out error, context);
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
			_prefix    = "8=FIX.4.2" + separator + "9=";
			_maximum   = maximum;
			_buffer    = new char[Math.Min(4096, maximum)];
		}

		public FrameReader(Stream input, int maximum, char separator = '\u0001')
		{
			_byteInput  = input;
			_separator  = separator;
			_prefix     = "8=FIX.4.2" + separator + "9=";
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
				return Fail(_count, FixTag.BodyLength, null, "Message exceeds maxMessageLength.", out error);

			if (!ReadTo(_prefix.Length))
				return Fail(_count, FixTag.BeginString, null, "Truncated FIX header.", out error);

			for (var i = 0; i < _prefix.Length; i++)
				if (At(i) != _prefix[i])
					return Fail(i, FixTag.BeginString, null, "Expected BeginString FIX.4.2 followed by BodyLength.", out error);

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
