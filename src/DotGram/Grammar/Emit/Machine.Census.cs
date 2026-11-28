using System;
using System.Collections.Generic;

using DotGram.Grammar.Model;

namespace DotGram.Grammar.Emit;

sealed partial class Machine
{
	/// <summary>
	/// For a test that counts what a refusal can say at each place by how the place is printed: told every
	/// node a display is asked of on this thread by what is not a reader — the jump machine, the flat one,
	/// a search — and what it was said as; for a choice said by what it predicts, its first alternative. Not
	/// a setting; null tells nothing, and nothing printed depends on it.
	/// </summary>
	internal static Action<Node, IReadOnlyList<string>>? NonReaderDisplaysTold
	{
		get { return _nonReaderSaid; }
		set { _nonReaderSaid = value; }
	}

	[ThreadStatic]
	static Action<Node, IReadOnlyList<string>>? _nonReaderSaid;

	/// <summary>Tells <see cref="NonReaderDisplaysTold"/> a node's display, where it is asked of by what is not a reader.</summary>
	void TellNonReader(Node node, IReadOnlyList<string> said)
	{
		if (_nonReaderSaid is { } told && !_reading && !_predicting)
			told(node, said);
	}

	/// <summary>
	/// Tells <see cref="NonReaderDisplaysTold"/> what a predicted choice is said as, where it is asked of by what
	/// is not a reader and not inside another prediction.
	/// </summary>
	void TellNonReaderPredicted(IReadOnlyList<Node> alternatives, IReadOnlyList<string> said, bool predicting)
	{
		if (_nonReaderSaid is { } told && !_reading && !predicting && alternatives.Count > 0)
			told(alternatives[0], ["(predicted)", .. said]);
	}
}
