using System.Collections.Generic;

using DotGram.Finance.Fix;
using DotGram.Finance.Fix.Fix42;

using Xunit;

namespace DotGram.Finance.Tests;

/// <summary>FIX 4.2 held against its repository: see <see cref="FixRepositoryAgreementTests"/>.</summary>
public sealed class Fix42RepositoryAgreementTests : FixRepositoryAgreementTests
{
	/// <summary>Every message type, by the name and the MsgType the repository gives it.</summary>
	public static TheoryData<string, string> Messages()
	{
		return MessagesOf("FIX.4.2");
	}

	protected override string Version => "FIX.4.2";

	protected override FixContext Context => Fix42Context.Default;

	protected override int PairCount => 14;

	protected override IReadOnlyList<FixFinding> Validate(byte[] wire)
	{
		var message = FixParser.ParseMessage(wire);

		message.Validate(Fix42Context.Default);

		return message.InvalidFindings ?? [];
	}

	protected override bool Check(byte[] wire, string name, FixField field)
	{
		var host  = FixParser.ParseMessage(wire);
		var check = (System.Delegate)typeof(FixValidator42).GetProperty(name)!.GetValue(FixValidator42.Default)!;

		check.DynamicInvoke(Fix42Context.Default, host, field);

		return host.IsValid;
	}
}
