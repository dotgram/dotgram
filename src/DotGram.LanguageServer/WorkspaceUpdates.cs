using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Text;

namespace DotGram.LanguageServer;

/// <summary>
/// What the editor and the file system change in the server's workspace, applied in order once the
/// folders are read, and no more of it kept than the workspace still needs.
/// </summary>
/// <remarks>
/// <para>
/// An open grammar's updates are kept one per file, the latest: a burst of edits waiting for the
/// folders, or behind a slow update, holds one text, not one per keystroke, and a close drops the
/// text it no longer needs.
/// </para>
/// <para>
/// A watched file is read off the queue, each read on its own, and only the read last asked for of
/// a file is applied: a read that stalls — a file on a share that does not answer — holds up neither
/// the other updates nor the texts behind them.
/// </para>
/// </remarks>
sealed class WorkspaceUpdates(Func<Task<FolderWorkspace>> folder, Action<Exception> report)
{
	enum Kind
	{
		Opened,
		Edited,
		Closed,
	}

	readonly object                                                  _gate     = new();
	readonly Dictionary<string, (Kind Kind, SourceText? Text)>       _grammars = new(StringComparer.Ordinal);
	readonly Dictionary<string, long>                                _asked    = new(StringComparer.Ordinal);
	readonly Dictionary<string, string?>                             _read     = new(StringComparer.Ordinal);
	readonly List<string>                                            _order    = [];
	long                                                             _reads;
	bool                                                             _draining;
	Task                                                             _drained  = Task.CompletedTask;

	/// <summary>Completes once every update asked for so far has been applied, or is a read still running; a test's barrier.</summary>
	public Task Drained
	{
		get
		{
			lock (_gate)
				return _drained;
		}
	}

	public void Opened(string path, SourceText text)
	{
		Grammar(path, Kind.Opened, text);
	}

	public void Edited(string path, SourceText text)
	{
		lock (_gate)
		{
			// Edits after an open that is not applied yet are that open, with the latest text.
			var kind = _grammars.TryGetValue(path, out var pending) && pending.Kind == Kind.Opened ? Kind.Opened : Kind.Edited;

			Pending(path);
			_grammars[path] = (kind, text);
			Schedule();
		}
	}

	/// <summary>A grammar was closed: its pending text is dropped, and its file is what it is again.</summary>
	public void Closed(string path)
	{
		Grammar(path, Kind.Closed, null);
		FileChanged(path, deleted: false);
	}

	/// <summary>A watched file changed: it is read off the queue, and the read applied in order once it is done.</summary>
	public void FileChanged(string path, bool deleted)
	{
		long read;

		lock (_gate)
			read = _asked[path] = ++_reads;

		if (deleted)
		{
			Read(path, read, null);
			return;
		}

		_ = Task.Run(() => Read(path, read, FolderWorkspace.Read(path)));
	}

	void Read(string path, long read, string? text)
	{
		lock (_gate)
		{
			// A read asked for later replaces this one, which is dropped with its text.
			if (!_asked.TryGetValue(path, out var asked) || asked != read)
				return;

			_asked.Remove(path);
			Pending(path);
			_read[path] = text;
			Schedule();
		}
	}

	void Grammar(string path, Kind kind, SourceText? text)
	{
		lock (_gate)
		{
			Pending(path);
			_grammars[path] = (kind, text);
			Schedule();
		}
	}

	void Pending(string path)
	{
		if (!_grammars.ContainsKey(path) && !_read.ContainsKey(path))
			_order.Add(path);
	}

	void Schedule()
	{
		if (_draining)
			return;

		_draining = true;
		_drained  = folder().ContinueWith(task => Drain(task.Result), CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default);
	}

	void Drain(FolderWorkspace workspace)
	{
		while (true)
		{
			string                             path;
			(Kind Kind, SourceText? Text)?     grammar = null;
			(bool Read, string? Text)          read    = default;

			lock (_gate)
			{
				if (_order.Count == 0)
				{
					_draining = false;
					return;
				}

				path = _order[0];
				_order.RemoveAt(0);

				if (_grammars.Remove(path, out var pending))
					grammar = pending;

				if (_read.Remove(path, out var text))
					read = (true, text);
			}

			try
			{
				switch (grammar)
				{
					case { Kind: Kind.Opened, Text: { } opened }:
						workspace.GrammarOpened(path, opened);
						break;

					case { Kind: Kind.Edited, Text: { } edited }:
						workspace.GrammarEdited(path, edited);
						break;

					case { Kind: Kind.Closed }:
						workspace.GrammarClosed(path);
						break;
				}

				if (read.Read)
					workspace.FileRead(path, read.Text);
			}
			catch (Exception exception) when (exception is not OutOfMemoryException)
			{
				report(exception);
			}
		}
	}
}
