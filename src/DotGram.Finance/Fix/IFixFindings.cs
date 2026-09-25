namespace DotGram.Finance.Fix;

/// <summary>What a finding is said to: a message of any version, which keeps what is wrong with it.</summary>
interface IFixFindings
{
	/// <summary>Adds one finding.</summary>
	void AddFinding(FixFinding finding);
}
