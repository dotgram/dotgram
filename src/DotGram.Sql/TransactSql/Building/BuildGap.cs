using System;

namespace DotGram.Sql.TransactSql;

/// <summary>
/// What <see cref="Build"/> throws for what a rule read and the common tree has no place for yet: the record it was
/// read into, under the name it had in the old tree, and why; the entries hand it back as an extension.
/// </summary>
/// <remarks>
/// The reasons are a closed set, so that the gap report counts them rather than reads them:
/// <list type="bullet">
/// <item><see cref="NoTarget"/>: the new tree has no record or member for what the old node holds.</item>
/// <item><see cref="NoWriterArm"/>: the target exists and <c>Sql2023Writer</c> refuses it.</item>
/// <item><see cref="NeedsDecision"/>: the shape is open, and the inventory row that says so is named.</item>
/// <item><see cref="NotKept"/>: the old tree does not keep what the new tree records, so no arm could
/// say which text was written.</item>
/// <item><see cref="NoConversion"/>: the target exists and the writer writes it; no builder converts the reading to it yet.</item>
/// </list>
/// </remarks>
sealed class BuildGap : Exception
{
	public const string NoTarget      = "no target record";
	public const string NoWriterArm   = "target exists, no writer arm";
	public const string NeedsDecision = "needs decision";
	public const string NotKept       = "not kept by the old tree";
	public const string NoConversion  = "no conversion";

	BuildGap(string oldType, string kind, string detail)
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
	/// One of the five reasons.
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

	public static BuildGap Missing(object old, string detail)
	{
		return new(Build.NameOf(old), NoTarget, detail);
	}

	public static BuildGap Unwritten(object old, string detail)
	{
		return new(Build.NameOf(old), NoWriterArm, detail);
	}

	public static BuildGap Open(object old, string row)
	{
		return new(Build.NameOf(old), NeedsDecision, row);
	}

	public static BuildGap Unconverted(object old, string detail)
	{
		return new(Build.NameOf(old), NoConversion, detail);
	}

	public static BuildGap Lost(object old, string detail)
	{
		return new(Build.NameOf(old), NotKept, detail);
	}
}
