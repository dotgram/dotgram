using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace TraceSpike;

/// <summary>The one sink every grammar's adapter forwards to: a stack, the why candidates, a profile.</summary>
sealed class Recorder
{
	public enum Kind
	{
		Why,
		Profile,
	}

	public Kind Mode;

	int[] _rules  = new int[1024];
	int[] _starts = new int[1024];
	long[] _since = new long[1024];
	int _depth;

	public int MaxDepth;
	public long Enters;
	public long Exits;
	public long Mismatched;
	public readonly HashSet<int> Threads = new();
	public bool WatchThreads;

	// Profile: per rule.
	public long[] Entered = new long[16];
	public long[] Failed = new long[16];
	public long[] Ticks = new long[16];

	public readonly List<(int[] Stack, int Position, string[]? Expected)> Candidates = new();

	public void Reset()
	{
		_depth = 0;
		Candidates.Clear();
		Enters = Exits = Mismatched = 0;
		MaxDepth = 0;
	}

	public void Enter(int rule, int position)
	{
		if (_depth == _rules.Length)
		{
			Array.Resize(ref _rules, _depth * 2);
			Array.Resize(ref _starts, _depth * 2);
			Array.Resize(ref _since, _depth * 2);
		}

		_rules[_depth]  = rule;
		_starts[_depth] = position;

		if (Mode == Kind.Profile)
		{
			if (rule >= Entered.Length)
				Grow(rule);

			Entered[rule]++;
			_since[_depth] = Stopwatch.GetTimestamp();
		}

		_depth++;
		Enters++;

		if (_depth > MaxDepth)
			MaxDepth = _depth;

		if (WatchThreads)
			lock (Threads)
				Threads.Add(Environment.CurrentManagedThreadId);
	}

	void Grow(int rule)
	{
		var size = Math.Max(rule + 1, Entered.Length * 2);

		Array.Resize(ref Entered, size);
		Array.Resize(ref Failed, size);
		Array.Resize(ref Ticks, size);
	}

	public void Exit(int rule, int position, int end)
	{
		Exits++;

		if (_depth == 0)
		{
			Mismatched++;
			return;
		}

		if (_rules[_depth - 1] != rule || _starts[_depth - 1] != position)
		{
			Mismatched++;

			// Unwind to the frame this exit closes, if it is on the stack at all.
			var at = _depth - 1;

			while (at >= 0 && (_rules[at] != rule || _starts[at] != position))
				at--;

			if (at < 0)
				return;

			_depth = at + 1;
		}

		_depth--;

		if (Mode == Kind.Profile)
		{
			Ticks[rule] += Stopwatch.GetTimestamp() - _since[_depth];

			if (end < 0)
				Failed[rule]++;
		}
	}

	public void Refused(int position, string[]? expected)
	{
		if (Mode != Kind.Why)
			return;

		Candidates.Add((_rules.AsSpan(0, _depth).ToArray(), position, expected));
	}

	public int Depth => _depth;
}

/// <summary>What a generated match says, read by reflection: the generated type keeps its sets private.</summary>
static class Matches
{
	public static (bool Ok, long Position, string[]? Expected, List<string[]>? Tied, string? Error) Read(object match)
	{
		var type = match.GetType();
		var ok   = (bool)type.GetProperty("IsSuccess")!.GetValue(match)!;
		var at   = Convert.ToInt64(type.GetProperty("Position")!.GetValue(match));

		if (ok)
			return (true, at, null, null, null);

		var expected = (string[]?)type.GetField("_expected", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(match);
		var tied     = (List<string[]>?)type.GetField("_tied", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(match);

		return (false, at, expected, tied, null);
	}
}
