using System.Collections.Generic;

using DotGram.Finance.Fix;
using DotGram.Finance.Fix.Fix50;

using Xunit;

namespace DotGram.Finance.Tests;

/// <summary>FIX 5.0 SP2 held against its repository: see <see cref="FixRepositoryAgreementTests"/>.</summary>
public sealed class Fix50RepositoryAgreementTests : FixRepositoryAgreementTests
{
	/// <summary>Every message type, by the name and the MsgType the repository gives it.</summary>
	public static TheoryData<string, string> Messages()
	{
		return MessagesOf("FIX.5.0SP2");
	}

	protected override string Version => "FIX.5.0SP2";

	protected override FixContext Context => Fix50Context.Default;

	protected override string BeginString => "FIXT.1.1";

	protected override int PairCount => 24;

	// HaltReason is HaltReasonInt in the dictionaries, and RateSource's slot is RateSourceField, since
	// the component RateSource has the name first.
	protected override Dictionary<int, string> Renamed => new() { [327] = "HaltReasonInt", [1446] = "RateSourceField" };

	protected override IReadOnlyList<FixFinding> Validate(byte[] wire)
	{
		var message = FixParser.ParseMessage(wire);

		message.Validate(Fix50Context.Default);

		return message.InvalidFindings ?? [];
	}

	protected override bool Check(byte[] wire, string name, FixField field)
	{
		var host  = FixParser.ParseMessage(wire);
		var check = (System.Delegate)typeof(FixValidator50).GetProperty(name)!.GetValue(FixValidator50.Default)!;

		check.DynamicInvoke(Fix50Context.Default, host, field);

		return host.IsValid;
	}
}
