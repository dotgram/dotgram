using DotGram.Grammar.Binding;
using DotGram.Grammar.Parsing;

namespace DotGram.Grammar;

/// <summary>Why a file is compiled without its settled copies, by the kind of what put a machine of it beyond the readers.</summary>
enum SettleCause
{
	/// <summary>A <c>parse</c> that methods cannot read: it runs on the engine, and reading it by methods would be the remedy.</summary>
	Engine,

	/// <summary>A <c>find</c> or a <c>yield</c>, which the engine always reads.</summary>
	Find,

	/// <summary>A <c>parse</c> with no result type, which is also read a window at a time, by the engine.</summary>
	Window,

	/// <summary>A <c>positional parse</c>, read from a position.</summary>
	Position,

	/// <summary>A <c>stream</c> or <c>stream bytes</c>, read from a buffer.</summary>
	Stream,

	/// <summary>A plain <c>parse</c> the plan compiled as a flat machine, which is not a reader either.</summary>
	Flat,

	/// <summary>A grammar that names <c>eos</c> reads segments, and every machine of it is on the engine: no publication is to blame.</summary>
	Segments,
}

/// <summary>
/// What put a file beyond the readers, and the publication to name for it: the first, in the order
/// written, that is of the cause; none for <see cref="SettleCause.Segments"/>.
/// </summary>
sealed record SettleBlock(SettleCause Cause, Publication? By)
{
	/// <summary>
	/// What a publication asks for by itself that no reader over a string can give, or null where it
	/// asks for nothing of the kind.
	/// </summary>
	public static SettleCause? Asked(Publication publication)
	{
		if (publication.Kind != PublishKind.Parse || publication.Yield)
			return SettleCause.Find;

		if (publication.Positional)
			return SettleCause.Position;

		if (publication.BufferedInput || publication.BufferedBytes)
			return SettleCause.Stream;

		return null;
	}
}
