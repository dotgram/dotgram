using System;
using System.Collections.Generic;

namespace DotGram.Grammar;

/// <summary>The targets bound for a caller that holds a C# scanner, or none.</summary>
static partial class ClassificationTargets
{
	/// <summary>Each target bound, in the order given.</summary>
	/// <param name="grammar">The grammar's own text, without what it includes.</param>
	/// <param name="targets">The targets, each as it was written.</param>
	/// <param name="scanner">What reads the grammar's C# blocks, as it is read anywhere else.</param>
	public static IReadOnlyList<ClassificationTarget> Bind(string grammar, IReadOnlyList<string> targets, ICSharpScanner? scanner)
	{
		return BindIn(grammar, targets, CSharpLanguage.Reading(scanner));
	}
}
