using System;
using System.Collections.Generic;

using DotGram.Grammar.Model;
using DotGram.Grammar.Emit;

namespace DotGram.Grammar;

/// <summary>
/// The state of the generator that is kept per thread rather than handed down: what the file being
/// emitted gathers, and what one question of the first sets has spent and is inside.
/// </summary>
/// <remarks>
/// <para>
/// Per thread because the generator may compile two grammars at once on two threads, and kept here
/// rather than in fields of their own because a walk that runs out of stack carries on on another
/// thread (<see cref="DeepStack"/>): a thread-static field would be empty there, and what the walk
/// wrote to it lost — a test of categories called in the file and never declared. This one object
/// goes with the walk and comes back with it, so whatever lives in it is the same on either side.
/// </para>
/// <para>
/// <b>So any state a walk keeps per thread and that outlives a call that may move belongs here.</b>
/// The thread-static fields left elsewhere — the sorting buffers of the first sets, the tables the
/// lexer is written from — are each set and read inside one step that makes no such call.
/// </para>
/// </remarks>
sealed class Ambient
{
	[ThreadStatic]
	static Ambient? _current;

	/// <summary>This thread's, made at the first question.</summary>
	public static Ambient Current
	{
		get
		{
			return _current ??= new Ambient();
		}
	}

	/// <summary>Makes <paramref name="ambient"/> this thread's: a thread a walk has moved to takes its caller's.</summary>
	internal static void Adopt(Ambient ambient)
	{
		_current = ambient;
	}

	/// <summary>What the file being emitted gathers as it is written; null outside <c>CSharpEmitter.Emit</c>.</summary>
	public EmissionScope? Emission { get; set; }

	/// <summary>What the question of the first sets being asked has left to spend (<c>FirstSets.Budget</c>).</summary>
	public int FirstSetsBudget;

	/// <summary>The bindings a question of the first sets is inside, so that one reaching itself is not walked again.</summary>
	public HashSet<TextBinding>? InsideBindings;
}
