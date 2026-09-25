using System.Collections.Generic;

using DotGram.Finance.Fix;
using DotGram.Finance.Fix.Fix44;

using Xunit;

namespace DotGram.Finance.Tests;

/// <summary>FIX 4.4 held against its repository: see <see cref="FixRepositoryAgreementTests"/>.</summary>
public sealed class Fix44RepositoryAgreementTests : FixRepositoryAgreementTests
{
	/// <summary>Every message type, by the name and the MsgType the repository gives it.</summary>
	public static TheoryData<string, string> Messages()
	{
		return MessagesOf("FIX.4.4");
	}

	protected override string Version => "FIX.4.4";

	protected override FixContext Context => Fix44Context.Default;

	protected override int PairCount => 16;

	// Hop: see A_components_declared_type_agrees_with_its_shape.
	protected override string[] Misdeclared => ["Hop"];

	protected override Dictionary<int, string> Renamed => new() { [23] = "IOIid", [33] = "LinesOfText" };

	protected override IReadOnlyList<FixFinding> Validate(byte[] wire)
	{
		var message = FixParser.ParseMessage(wire);

		message.Validate(Fix44Context.Default);

		return message.InvalidFindings ?? [];
	}

	protected override bool Check(byte[] wire, string name, FixField field)
	{
		var host  = FixParser.ParseMessage(wire);
		var check = (System.Delegate)typeof(FixValidator44).GetProperty(name)!.GetValue(FixValidator44.Default)!;

		check.DynamicInvoke(Fix44Context.Default, host, field);

		return host.IsValid;
	}
}
