using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace DotGram.Benchmarks;

static partial class Stand
{
	/// <summary>
	/// Where a timing window lives: <c>/ramdisk/locks</c> on this machine, <c>DOTGRAM_WINDOW_DIR</c> when it is set, the temp directory elsewhere. The same rule as
	/// <c>benchmarks/WindowLib.ps1</c> and <c>benchmarks/Aside.sh</c>, which is what makes the three see one window (D147).
	/// </summary>
	internal static string WindowDirectory
	{
		get
		{
			var directory = Environment.GetEnvironmentVariable("DOTGRAM_WINDOW_DIR") is { Length: > 0 } named ? named
				: Directory.Exists("/ramdisk") ? "/ramdisk/locks"
				: Path.GetTempPath();

			Directory.CreateDirectory(directory);

			return directory;
		}
	}

	/// <summary>The announcement: who holds the window, since when, until when and what for. Information for whoever reads it; the lock is the answer.</summary>
	internal static string WindowFile => Path.Combine(WindowDirectory, "timing-window.txt");

	static string WindowLock => Path.Combine(WindowDirectory, "timing-window.lock");
	static string BuildsLock => Path.Combine(WindowDirectory, "timing-builds.lock");

	/// <summary>
	/// A timing run holds the window for as long as it runs: the lock <c>timing-window.lock</c>, exclusively, which the kernel lets go when the process ends however it ends, and the announcement
	/// beside it (<c>pid</c>, <c>started</c>, <c>until</c>, <c>what</c>). It then waits, at most twenty minutes, for the builds that hold <c>timing-builds.lock</c> (they run through
	/// <c>benchmarks/Aside.sh</c>) to end, naming them, and holds that lock too, so that no build starts until the run ends. It sets <c>DOTGRAM_WINDOW_HOLDER</c> to its pid, so that the
	/// children a repeated run starts know they are inside it; a run started inside a window that way (or by <c>Run-Announced.ps1</c>) announces nothing. It does not stop anyone from timing: a
	/// run that finds another window open says so and goes on, and a run whose builds did not end in time says so and goes on, and neither is a figure to quote.
	/// </summary>
	internal static IDisposable AnnounceWindow(double? limitMinutes, string what)
	{
		try
		{
			if (Environment.GetEnvironmentVariable("DOTGRAM_ASIDE") is { Length: > 0 })
			{
				Console.Error.WriteLine("This timing was started through benchmarks/Aside.sh, whose builds lock a window waits for: it announces nothing, and its numbers are not to be quoted. Run it through Run-Announced.ps1.");

				return None.Instance;
			}

			FileStream? gate = null;

			// A shared probe (Window.ps1, Aside.sh) holds the lock for milliseconds: a few seconds of retries ride over it.
			for (var attempt = 0; attempt < 25 && gate is null; attempt++)
				if ((gate = TryLock(WindowLock, exclusive: true)) is null)
					Thread.Sleep(200);

			if (gate is null)
			{
				var lines  = ReadWindowFile();
				var holder = Environment.GetEnvironmentVariable("DOTGRAM_WINDOW_HOLDER");

				if (holder is not { Length: > 0 } || lines.FirstOrDefault() != $"pid {holder}")
					Console.Error.WriteLine("Another timing window is open: " + string.Join("; ", lines) + ". This run times beside it, and its numbers are not to be quoted.");

				return None.Instance;
			}

			var started = DateTime.Now;
			var until   = limitMinutes is { } minutes ? started.AddMinutes(minutes).ToString("yyyy-MM-dd HH:mm:ss") : "unknown";

			Environment.SetEnvironmentVariable("DOTGRAM_WINDOW_HOLDER", Environment.ProcessId.ToString(System.Globalization.CultureInfo.InvariantCulture));
			WriteAnnouncement(started, until, what, "waiting for the builds that hold timing-builds.lock");

			var builds   = TryLock(BuildsLock, exclusive: true);
			var deadline = DateTime.Now.AddMinutes(20);
			var said     = DateTime.MinValue;

			while (builds is null && DateTime.Now < deadline)
			{
				if ((DateTime.Now - said).TotalSeconds >= 60)
				{
					Console.Error.WriteLine("Waiting for builds to end before the window opens: " + string.Join("; ", LockHolders(BuildsLock)));
					said = DateTime.Now;
				}

				Thread.Sleep(2000);
				builds = TryLock(BuildsLock, exclusive: true);
			}

			if (builds is null)
				Console.Error.WriteLine("Builds did not end in twenty minutes; timing beside them, and the numbers are not to be quoted: " + string.Join("; ", LockHolders(BuildsLock)));

			WriteAnnouncement(started, until, what, builds is null ? "open BESIDE builds that did not end" : $"open since {DateTime.Now:HH:mm:ss}");

			return new Announced(gate, builds);
		}
		catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
		{
			return None.Instance;
		}
	}

	static void WriteAnnouncement(DateTime started, string until, string what, string state)
	{
		File.WriteAllLines(WindowFile,
		[
			$"pid {Environment.ProcessId}",
			$"started {started:yyyy-MM-dd HH:mm:ss}",
			$"until {until}",
			$"what {what}",
			$"state {state}",
		]);
	}

	static string[] ReadWindowFile()
	{
		try
		{
			return File.ReadAllLines(WindowFile);
		}
		catch (IOException)
		{
			return [];
		}
	}

	/// <summary>
	/// A lock taken without waiting, or null. On Linux a <see cref="FileShare.None"/> open is an exclusive <c>flock</c> and any other share a shared one, so the lock is the one
	/// <c>flock(1)</c> in <c>Aside.sh</c> takes.
	/// </summary>
	static FileStream? TryLock(string path, bool exclusive)
	{
		try
		{
			return exclusive
				? new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)
				: new FileStream(path, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite);
		}
		catch (IOException)
		{
			return null;
		}
	}

	/// <summary>Who holds a lock, from <c>/proc/locks</c>: the pid and the command line of each holder (Linux; nothing elsewhere).</summary>
	static IEnumerable<string> LockHolders(string path)
	{
		if (!OperatingSystem.IsLinux())
			return [];

		try
		{
			var start = new ProcessStartInfo("stat", ["-c", "%i", path]) { RedirectStandardOutput = true, UseShellExecute = false };

			using var stat = Process.Start(start)!;

			var inode = stat.StandardOutput.ReadToEnd().Trim();

			stat.WaitForExit();

			return
			[
				.. File.ReadAllLines("/proc/locks")
					.Select(static line => line.Split(' ', StringSplitOptions.RemoveEmptyEntries))
					.Where(fields => fields.Length >= 6 && fields[5].EndsWith(":" + inode, StringComparison.Ordinal))
					.Select(static fields => $"pid {fields[4]} {CommandLine(fields[4])}"),
			];
		}
		catch (Exception exception) when (exception is IOException or System.ComponentModel.Win32Exception)
		{
			return [];
		}
	}

	static string CommandLine(string pid)
	{
		try
		{
			return File.ReadAllText($"/proc/{pid}/cmdline").Replace('\0', ' ').Trim();
		}
		catch (IOException)
		{
			return "(gone)";
		}
	}

	sealed class None : IDisposable
	{
		internal static readonly None Instance = new();

		public void Dispose()
		{
		}
	}

	sealed class Announced(FileStream gate, FileStream? builds) : IDisposable
	{
		public void Dispose()
		{
			try
			{
				File.WriteAllLines(WindowFile, ["idle", $"since {DateTime.Now:yyyy-MM-dd HH:mm:ss}", $"why the window of pid {Environment.ProcessId} ended"]);
			}
			catch (IOException)
			{
			}

			Environment.SetEnvironmentVariable("DOTGRAM_WINDOW_HOLDER", null);
			builds?.Dispose();
			gate.Dispose();
		}
	}
}
