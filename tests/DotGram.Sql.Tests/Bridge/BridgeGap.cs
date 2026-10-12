using System;

namespace DotGram.Sql.Tests;

/// <summary>
/// What <see cref="Bridge"/> throws for an old node it cannot carry onto the new tree yet: the old
/// record, and why.
/// </summary>
/// <remarks>
/// The reasons are a closed set, so that the gap report counts them rather than reads them:
/// <list type="bullet">
/// <item><see cref="NoTarget"/>: the new tree has no record or member for what the old node holds.</item>
/// <item><see cref="NoWriterArm"/>: the target exists and <c>Sql2023Writer</c> refuses it.</item>
/// <item><see cref="NeedsDecision"/>: the shape is open, and the inventory row that says so is named.</item>
/// <item><see cref="NotKept"/>: the old tree does not keep what the new tree records, so no arm could
/// say which text was written.</item>
/// </list>
/// </remarks>
sealed class BridgeGap : Exception
{
	public const string NoTarget      = "no target record";
	public const string NoWriterArm   = "target exists, no writer arm";
	public const string NeedsDecision = "needs decision";
	public const string NotKept       = "not kept by the old tree";

	BridgeGap(string oldType, string kind, string detail)
		: base($"{oldType}: {kind}: {detail}")
	{
		OldType = oldType;
		Kind    = kind;
		Detail  = detail;
	}

	/// <summary>
	/// The old record the gap is in, as <c>Statement.Select</c> or <c>Clause.Top</c>.
	/// </summary>
	public string OldType { get; }

	/// <summary>
	/// One of the four reasons.
	/// </summary>
	public string Kind { get; }

	/// <summary>
	/// What is missing: the member, the record or the inventory row.
	/// </summary>
	public string Detail { get; }

	/// <summary>
	/// The reason and what it names, as the gap report counts it.
	/// </summary>
	public string Reason => $"{Kind}: {Detail}";

	public static BridgeGap Missing(object old, string detail)
	{
		return new(Bridge.NameOf(old), NoTarget, detail);
	}

	public static BridgeGap Unwritten(object old, string detail)
	{
		return new(Bridge.NameOf(old), NoWriterArm, detail);
	}

	public static BridgeGap Open(object old, string row)
	{
		return new(Bridge.NameOf(old), NeedsDecision, row);
	}

	public static BridgeGap Lost(object old, string detail)
	{
		return new(Bridge.NameOf(old), NotKept, detail);
	}
}
