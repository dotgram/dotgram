using System;
using System.Globalization;

namespace DotGram.Finance.Fix44;

/// <summary>
/// How the input separates one field from the next.
/// </summary>
public enum FixFraming
{
	/// <summary>
	/// Wire framing: every field ends with SOH, U+0001.
	/// </summary>
	Wire,

	/// <summary>
	/// Log framing: every field ends with a vertical bar, which may carry surrounding spaces.
	/// </summary>
	Log,
}

static class FixFramings
{
	/// <summary>
	/// The character the framing ends a field with.
	/// </summary>
	public static char Separator(this FixFraming framing)
	{
		return framing == FixFraming.Log ? '|' : '\u0001';
	}
}

/// <summary>A malformed message, identified by its zero-based character offset.</summary>
public sealed class FixParseError
{
	internal FixParseError(int position, int? tag, string? messageType, string reason)
	{
		Position = position;
		Tag = tag;
		MessageType = messageType;
		Reason = reason;
	}

	/// <summary>
	/// The zero-based character offset at which the problem was found.
	/// </summary>
	public int Position { get; }
	/// <summary>
	/// The tag of the field involved, or null when the problem is not in one field.
	/// </summary>
	public int? Tag { get; }
	/// <summary>
	/// The MsgType, or null when the problem comes before it is known.
	/// </summary>
	public string? MessageType { get; }
	/// <summary>
	/// What is wrong, in English.
	/// </summary>
	public string Reason { get; }
	/// <summary>
	/// Formats the message type, tag, offset and reason on one line.
	/// </summary>
	public override string ToString()
	{
		return $"FIX {MessageType ?? "?"}, tag {Tag?.ToString(CultureInfo.InvariantCulture) ?? "?"}, offset {Position}: {Reason}";
	}
}
