using System;
using System.IO;
using System.Reflection;

namespace DotGram.Tests;

/// <summary>What the tests of buffered input read with, in DotGram.Tests and in DotGram.Tests.Slow.</summary>
public sealed partial class BufferedInputTests
{
	/// <summary>What a buffered reading answers, all four of them.</summary>
	/// <remarks>
	/// The message among them, because nothing else holds what a buffered or byte reading says
	/// when it refuses: the refusal record is written for the string form, and the one branch
	/// this repository has on what the platform can do lives in the buffered reader. So the
	/// check that both sides answer alike has to include what they say, not only whether and
	/// where (critic, Q20).
	/// </remarks>
	static (bool Success, object? Value, long Position, string? Error) Read(Assembly assembly, TextReader reader, int capacity, int limit = int.MaxValue)
	{
		var match = assembly.GetType("Grammar")!.GetMethod("TryParseStart", [typeof(TextReader), typeof(int?), typeof(int?)])!.Invoke(null, [reader, capacity, limit])!;
		object? Get(string property) => match.GetType().GetProperty(property)!.GetValue(match);
		return ((bool)Get("IsSuccess")!, Get("Value"), (long)Get("Position")!, (string?)Get("Error"));
	}

	sealed class ShortReader(string text, int chunk) : TextReader
	{
		int _position;
		public bool Disposed { get; private set; }
		public override int Read(char[] buffer, int index, int count)
		{
			var length = Math.Min(Math.Min(chunk, count), text.Length - _position);
			text.CopyTo(_position, buffer, index, length);
			_position += length;
			return length;
		}
		protected override void Dispose(bool disposing) { Disposed = true; base.Dispose(disposing); }
	}

	sealed class ShortStream(byte[] bytes) : MemoryStream(bytes)
	{
		public override int Read(byte[] buffer, int offset, int count) => base.Read(buffer, offset, Math.Min(count, 1));
	}
}
