using System;
using System.Diagnostics;
using System.IO;

using Xunit;

namespace DotGram.Tests.ExpressionLanguage;

/// <summary>
/// The one check that has to run in a process of its own (D-load-order).
/// </summary>
/// <remarks>
/// <para>
/// The expression language finds a type by searching the assemblies the process has loaded, and a
/// referenced assembly is loaded lazily. So what a text can name depends on what the program did
/// before asking — and no test beside the others can see that, because this assembly has loaded
/// everything it references before the first test runs. Running it here would be the worst kind of
/// test: not merely failing to catch the defect, but convincing a reader that it would have.
/// </para>
/// <para>
/// So the check is a program, <c>DotGram.ExpressionLanguage.LoadOrder</c>, and this runs it and
/// reports what it printed. The project is referenced with
/// <c>ReferenceOutputAssembly="false"</c>: that fixes the build order without letting this
/// assembly hold its types, which would load them and put us back where we started.
/// </para>
/// </remarks>
public sealed class LoadOrderTests
{
	[Fact]
	public void What_a_text_can_name_is_what_the_probe_recorded()
	{
		var program = Program();

		Assert.True(File.Exists(program), $"The load-order program is not built: {program}");

		using var ran = Process.Start(new ProcessStartInfo(program)
		{
			RedirectStandardOutput = true,
			RedirectStandardError  = true,
		})!;

		var said = ran.StandardOutput.ReadToEnd() + ran.StandardError.ReadToEnd();

		ran.WaitForExit();

		// Its own account of what differed, not just its exit code: a red run in CI has to say what
		// is wrong, and the program is the only thing that knows.
		Assert.True(ran.ExitCode == 0, said);
	}

	/// <summary>Where the program lands, beside this assembly's own output.</summary>
	static string Program()
	{
		var here = new DirectoryInfo(AppContext.BaseDirectory);
		var root = here;

		while (root is not null && !File.Exists(Path.Combine(root.FullName, "DotGram.slnx")))
			root = root.Parent;

		if (root is null)
			throw new InvalidOperationException("The repository root is not above the test binaries.");

		// The same configuration and framework this assembly was built for: the two projects are
		// built together, so the one that ran this is the one to run.
		var framework    = here.Name;
		var configuration = here.Parent!.Name;

		return Path.Combine(
			root.FullName,
			"tests",
			"DotGram.ExpressionLanguage.LoadOrder",
			"bin",
			configuration,
			framework,
			"DotGram.ExpressionLanguage.LoadOrder" + (OperatingSystem.IsWindows() ? ".exe" : ""));
	}
}
