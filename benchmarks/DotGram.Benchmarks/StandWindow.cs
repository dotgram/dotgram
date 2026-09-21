using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace DotGram.Benchmarks;

static partial class Stand
{
	/// <summary>Where a timing window announces itself: one file in the user's temp directory, read by every session before it times anything (expr, 2026-09-20: a window is only known by looking at the process list and asking).</summary>
	internal static string WindowFile => Path.Combine(Path.GetTempPath(), "dotgram-timing-window.txt");

	/// <summary>
	/// A timing run announces itself for as long as it runs: <c>pid</c>, <c>started</c>, <c>until</c> (the end of its <c>--limit</c>, or <c>unknown</c>), <c>what</c> (its arguments). The file is never removed: when the run
	/// ends it is put back to <c>idle</c> (first line), so that its absence means a defect of the stand and not "no window", and a file whose pid is not alive is stale and is overwritten. A child process of a repeated run finds its parent's announcement alive and writes nothing. It does not stop anyone from timing:
	/// it says that a window is open and when it ends, which is what a session that is about to time something needs to know before it asks whose process this is. A run that finds another announcement alive says so.
	/// </summary>
	internal static IDisposable AnnounceWindow(double? limitMinutes, string what)
	{
		try
		{
			var path = WindowFile;

			if (File.Exists(path))
			{
				var lines = File.ReadAllLines(path);
				var owner = lines.FirstOrDefault(static line => line.StartsWith("pid ", StringComparison.Ordinal));

				if (owner is not null && int.TryParse(owner[4..], out var pid) && pid != Environment.ProcessId && IsAlive(pid))
				{
					// Our parent (a repeated run started this child) or another session's window: either way this run announces nothing.
					if (!IsParent(pid))
						Console.Error.WriteLine("Another timing window is announced and its process is alive: " + string.Join("; ", lines));

					return None.Instance;
				}
			}

			File.WriteAllLines(path,
			[
				$"pid {Environment.ProcessId}",
				$"started {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
				$"until {(limitMinutes is { } minutes ? DateTime.Now.AddMinutes(minutes).ToString("yyyy-MM-dd HH:mm:ss") : "unknown")}",
				$"what {what}",
			]);

			return new Announced(path, Environment.ProcessId);
		}
		catch (Exception)
		{
			return None.Instance;
		}
	}

	static bool IsAlive(int pid)
	{
		try
		{
			using var process = Process.GetProcessById(pid);

			return !process.HasExited;
		}
		catch (ArgumentException)
		{
			return false;
		}
	}

	static bool IsParent(int pid)
	{
		try
		{
			// The parent of a child of a repeated run is found from the command line the parent passed: a child is started with the same executable, so its parent is the process that announced.
			return Process.GetProcessById(pid).ProcessName == Process.GetCurrentProcess().ProcessName;
		}
		catch (ArgumentException)
		{
			return false;
		}
	}

	sealed class None : IDisposable
	{
		internal static readonly None Instance = new();

		public void Dispose()
		{
		}
	}

	sealed class Announced(string path, int pid) : IDisposable
	{
		public void Dispose()
		{
			try
			{
				if (File.Exists(path) && File.ReadLines(path).FirstOrDefault() == $"pid {pid}")
					File.WriteAllLines(path, ["idle", $"since {DateTime.Now:yyyy-MM-dd HH:mm:ss}", $"why the window of pid {pid} ended"]);
			}
			catch (IOException)
			{
			}
		}
	}
}
