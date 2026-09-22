using System;

namespace DotGram.Finance.Fix;

/// <summary>The schema a message is held to.</summary>
/// <remarks>
/// <para>
/// Passed to <see cref="FixMessage.Validate"/> rather than read from anywhere, so that a process
/// reading two counterparties with two schemas is expressible: each pass over an input has its
/// own, and nothing about one reaches the other.
/// </para>
/// <para>
/// What it carries is the check of each message type, held as one object of ninety-four slots so
/// that a dictionary loaded at run time replaces the slots it describes and leaves the rest. The
/// object is not part of what a consumer sees: what replaces a check is a schema, read from a
/// file, and not a delegate written by hand.
/// </para>
/// </remarks>
public sealed class FixContext
{
	/// <summary>FIX 4.4 as this package compiles it in.</summary>
	public static FixContext Default { get; } = new();

	/// <summary>The check of each message type.</summary>
	internal FixValidators Validators { get; init; } = FixValidators.Default;
}
