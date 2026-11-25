using System;
using System.Collections.Generic;

namespace DotGram.Tests.RuleValues;

/// <summary>
/// How the head's outcome of one input stands to the published package's.
/// </summary>
static class RuleDifference
{
	/// <summary>Both built and agree.</summary>
	public const string Same = "same";

	/// <summary>The package refused or wrote C# that did not compile or crashed; the head builds.</summary>
	public const string Relaxed = "relaxed";

	/// <summary>The package built without a warning of a possible null; the head refuses.</summary>
	public const string RefusedNow = "refused-now";

	/// <summary>Both built, without a warning of a possible null on the package's side, and the values differ.</summary>
	public const string ValueChanged = "value-changed";

	/// <summary>The package built only with a warning of a possible null (CS8603 and its kin); the head differs.</summary>
	public const string WarnOnly = "warn-only";

	/// <summary>Neither built.</summary>
	public const string ErrorToError = "error-to-error";

	/// <summary>Every class, in the order a summary prints them.</summary>
	public static readonly IReadOnlyList<string> All = [Same, Relaxed, RefusedNow, ValueChanged, WarnOnly, ErrorToError];

	/// <summary>Classifies one input.</summary>
	public static string Classify(Outcome package, Outcome head)
	{
		if (package.Built && head.Built)
		{
			if (Agree(package, head))
				return Same;

			return package.WarnedNull ? WarnOnly : ValueChanged;
		}

		if (package.Built)
			return package.WarnedNull ? WarnOnly : RefusedNow;

		return head.Built ? Relaxed : ErrorToError;
	}

	static bool Agree(Outcome package, Outcome head)
	{
		return package.Kind == head.Kind && package.Text == head.Text;
	}

	/// <summary>The head built with a warning of a possible null that the package's build did not have.</summary>
	public static bool HeadWarnsNew(Outcome package, Outcome head)
	{
		return head.Built && head.WarnedNull && !(package.Built && package.WarnedNull);
	}

	/// <summary>The class is one a gate with an allow-list must account for.</summary>
	public static bool NeedsAccount(string difference)
	{
		return difference is RefusedNow or ValueChanged or WarnOnly;
	}
}
