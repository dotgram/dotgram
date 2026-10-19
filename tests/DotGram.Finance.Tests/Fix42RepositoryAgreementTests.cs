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

		return message.Validate(Fix42Context.Default);
	}

	protected override bool Check(byte[] wire, string name, FixField field)
	{
		var check = (System.Delegate)typeof(FixValidator42).GetProperty(name)!.GetValue(FixValidator42.Default)!;
		var found = new List<FixFinding>();
		var run   = FixCheck.Rent(found);

		try
		{
			check.DynamicInvoke(Fix42Context.Default, run, field);
		}
		finally
		{
			run.Return();
		}

		return found.Count == 0;
	}
}
