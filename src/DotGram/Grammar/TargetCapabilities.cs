using System;
using System.Collections.Generic;

namespace DotGram.Grammar;

/// <summary>
/// What a target language and its environment can do, where a decision of the generator forks on it.
/// </summary>
/// <remarks>
/// <para>
/// One record per target, immutable, asked by whichever stage decides (<see cref="ITargetLanguage.Can"/>).
/// A decision reads what the target can and what the author asked for, both: a capability is not an
/// option, and an option is not a capability.
/// </para>
/// <para>
/// The rules it is kept by. A member is a capability of a language or of where it runs, named for
/// what it can do, never for a defect of one target or of this generator. Every member is required:
/// a new one does not compile until every target has answered it, and none has a default to inherit.
/// A member exists only with a decision that forks on it and a test that makes a target of C# with
/// that one answer changed, and holds what comes out. And where a grammar needs what a target
/// cannot do and there is no other way to read it, that is said at the grammar, not written around.
/// </para>
/// </remarks>
sealed class TargetCapabilities
{
	readonly bool _unstructuredJumps;
	readonly bool _byteStringLiterals;

	/// <summary>
	/// Whether code may jump to a label anywhere in its method (<c>goto</c>). The two machines that
	/// are written as states and jumps between them need it: the flat machine and the jump machine.
	/// A target without it reads a publication through the structured reader, which is asked first
	/// there, and is offered no streamed form, which only the jump machine reads; a publication the
	/// reader refuses, and a terminal's value read again over tokens, are refused for the target.
	/// </summary>
	/// <remarks>
	/// What the fork covers is the machines of publications, and no more yet. Still written with
	/// <c>goto</c> whatever the answer, and so work for the first target without it: the lexer's
	/// scanners and seams, the standard library's scanners, the one machine of a grammar with
	/// nothing published, and the reader's own way out of a remembered rule (its memo).
	/// <c>TargetCapabilityTests</c> holds where that boundary is, over the snapshots.
	/// </remarks>
	public required bool UnstructuredJumps
	{
		get
		{
			Reads?.Add(nameof(UnstructuredJumps));

			return _unstructuredJumps;
		}
		init
		{
			_unstructuredJumps = value;
		}
	}

	/// <summary>
	/// Whether a literal can be written as the bytes of its UTF-8 encoding, held in the program's
	/// data: what the lexer's tables are written as where the author asks for it.
	/// </summary>
	public required bool ByteStringLiterals
	{
		get
		{
			Reads?.Add(nameof(ByteStringLiterals));

			return _byteStringLiterals;
		}
		init
		{
			_byteStringLiterals = value;
		}
	}

	/// <summary>
	/// Where each capability asked about is noted, by name, as it is asked: for a test that holds
	/// which decisions read which. Null, as it is outside such a test, notes nothing.
	/// </summary>
	public ICollection<string>? Reads { get; init; }

	/// <summary>The same answers, with what is asked noted in <paramref name="reads"/>.</summary>
	public TargetCapabilities Recording(ICollection<string> reads)
	{
		return new TargetCapabilities
		{
			UnstructuredJumps  = _unstructuredJumps,
			ByteStringLiterals = _byteStringLiterals,
			Reads              = reads,
		};
	}
}
