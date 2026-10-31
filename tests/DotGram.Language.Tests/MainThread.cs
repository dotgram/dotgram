using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;

namespace DotGram.Language.Tests;

/// <summary>
/// One thread with a synchronization context of its own, as an editor's main thread is: what is posted
/// to it runs on it, in order. <see cref="SwitchTo"/> moves an async method onto it the way Visual
/// Studio's <c>SwitchToMainThreadAsync</c> does — for the rest of that method, and not for its caller.
/// </summary>
sealed class MainThread : IDisposable
{
	readonly BlockingCollection<(SendOrPostCallback Callback, object? State)> _queue = new();
	readonly Thread _thread;

	public MainThread()
	{
		_thread = new Thread(Pump) { IsBackground = true, Name = "test main thread" };
		_thread.Start();
	}

	public int ManagedThreadId => _thread.ManagedThreadId;

	public bool IsCurrent => Thread.CurrentThread == _thread;

	public Awaitable SwitchTo()
	{
		return new Awaitable(this);
	}

	public void Dispose()
	{
		_queue.CompleteAdding();
		_thread.Join();
	}

	void Pump()
	{
		SynchronizationContext.SetSynchronizationContext(new Context(this));

		foreach (var (callback, state) in _queue.GetConsumingEnumerable())
			callback(state);
	}

	sealed class Context(MainThread owner) : SynchronizationContext
	{
		public override void Post(SendOrPostCallback d, object? state)
		{
			owner._queue.Add((d, state));
		}

		public override void Send(SendOrPostCallback d, object? state)
		{
			throw new NotSupportedException();
		}
	}

	public readonly struct Awaitable(MainThread owner) : INotifyCompletion
	{
		public Awaitable GetAwaiter()
		{
			return this;
		}

		public bool IsCompleted => owner.IsCurrent;

		public void OnCompleted(Action continuation)
		{
			owner._queue.Add((static state => ((Action)state!)(), continuation));
		}

		public void GetResult()
		{
		}
	}
}
