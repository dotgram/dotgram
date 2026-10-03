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

	/// <summary>A frame of the why stack: shared by every candidate taken under it, so a refusal costs O(1).</summary>
	public sealed class Frame
	{
		public readonly int Rule;
		public readonly Frame? Parent;
		public readonly int Depth;

		public Frame(int rule, Frame? parent)
		{
			Rule   = rule;
			Parent = parent;
			Depth  = parent is null ? 1 : parent.Depth + 1;
		}

		public int[] ToArray()
		{
			var all = new int[Depth];
			var at  = Depth;

			for (var frame = this; frame is not null; frame = frame.Parent)
				all[--at] = frame.Rule;

			return all;
		}
	}

	Frame? _top;
	int _furthest = -1;

	/// <summary>Only the candidates at the furthest position so far: the final failure cannot be nearer.</summary>
	public readonly List<(Frame? Top, int Position, string[]? Expected)> Taken = new();

	public IEnumerable<(int[] Stack, int Position, string[]? Expected)> Candidates
	{
		get
		{
			foreach (var (top, position, expected) in Taken)
				yield return (top?.ToArray() ?? [], position, expected);
		}
	}

	public long Refusals;

	public void Reset()
	{
		_depth = 0;
		_top = null;
		_furthest = -1;
		Refusals = 0;
		Taken.Clear();
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

		if (Mode == Kind.Why)
			_top = new Frame(rule, _top);

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

			while (_depth > at + 1)
			{
				_depth--;
				_top = _top?.Parent;
			}
		}

		_depth--;
		_top = _top?.Parent;

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

		Refusals++;

		if (position < _furthest)
			return;

		if (position > _furthest)
		{
			_furthest = position;
			Taken.Clear();
		}

		Taken.Add((_top, position, expected));
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

		var error = (string?)type.GetProperty("Error")?.GetValue(match);

		return (false, at, expected, tied, error);
	}
}
