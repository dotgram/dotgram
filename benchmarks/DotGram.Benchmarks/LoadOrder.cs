using System;
using System.Linq;
using System.Runtime.CompilerServices;

using DotGram.ExpressionLanguage;

namespace DotGram.Benchmarks;

/// <summary>
/// `--load-order`: what a text can name depends on what the process has already loaded.
/// </summary>
/// <remarks>
/// <para>
/// The expression language finds a type by searching the assemblies the process has loaded, and a
/// referenced assembly is loaded lazily, at its first use. So a consumer naming a type from their
/// own dependency gets an answer that depends on what else their program did first.
/// </para>
/// <para>
/// This cannot be a test class beside the others: a test assembly has loaded everything it
/// references long before the first test runs, and an <c>AssemblyLoadContext</c> does not help,
/// since <c>GetAssemblies()</c> sees every context in the process. It has to be a program that has
/// loaded nothing extra — hence a mode here rather than a project of its own. `DotGram.Tests` runs
/// this exe with this mode and reports what it printed.
/// </para>
/// <para>
/// **The one thing that can make this measure itself**: anything in the process touching
/// <c>DotGram.Web</c> before step 1. The dispatcher in <see cref="Program"/> names no type from it,
/// and the touch below is in a method of its own, marked <c>NoInlining</c> — naming a type in the
/// body of the method that runs first loads its assembly when that method is compiled, which is
/// what a first attempt at this witness measured instead of the thing it meant to. Step 1 prints
/// whether the assembly was loaded, so the program says out loud whether it is still answering the
/// question it was written for.
/// </para>
/// </remarks>
static class LoadOrder
{
	/// <summary>A type from an assembly nothing here otherwise uses, named in full.</summary>
	const string Text = "() => DotGram.Web.MediaType.Parse(\"text/plain\")";

	static bool Loaded() =>
		AppDomain.CurrentDomain.GetAssemblies().Any(one => one.GetName().Name == "DotGram.Web");

	[MethodImpl(MethodImplOptions.NoInlining)]
	static string Touch() => typeof(DotGram.Web.MediaType).FullName!;

	/// <summary>Runs the three steps and returns what a process should exit with.</summary>
	public static int Run()
	{
		var wrong = 0;

		var loadedFirst = Loaded();

		Console.WriteLine($"1. DotGram.Web loaded before any parse: {loadedFirst}");

		if (loadedFirst)
		{
			Console.WriteLine(
				"   WRONG: something loaded it before this mode ran, so this run proves nothing. " +
				"Whatever named a type from it — the dispatcher, a field initializer, a mode " +
				"compiled alongside — has to stop, or this check has to move to a process that " +
				"does not.");

			wrong++;
		}

		var before = ExpressionParser.TryParse(Text);

		Console.WriteLine($"2. parse before touching it: success={before.IsSuccess} error={before.Error}");

		// Today this refuses, and this holds that rather than the behaviour we want, so the suite
		// stays green while the decision is open: whether the library may load a caller's
		// referenced assemblies on a miss is Igor's, not this program's. When it is taken, the
		// test below becomes `if (!before.IsSuccess)` and this comment goes with it.
		if (before.IsSuccess)
		{
			Console.WriteLine(
				"   CHANGED: a name from an untouched assembly now resolves. If the reader was " +
				"given the retry-after-loading-references behaviour, this mode should now expect " +
				"success at step 2, and this branch is the reminder to change it.");

			wrong++;
		}

		Console.WriteLine($"   (touching {Touch()})");

		if (!Loaded())
		{
			Console.WriteLine("   WRONG: touching the type did not load its assembly.");

			wrong++;
		}

		var after = ExpressionParser.TryParse(Text);

		Console.WriteLine($"3. parse after touching it: success={after.IsSuccess} error={after.Error}");

		if (!after.IsSuccess)
		{
			Console.WriteLine(
				"   WRONG: the same text still does not parse once the assembly is loaded. Either " +
				"the AssemblyLoad subscription in Names.Loaded stopped clearing the caches, or the " +
				"name cannot be resolved for a reason that has nothing to do with load order.");

			wrong++;
		}

		Console.WriteLine(wrong == 0
			? "OK: the answer depends on load order, exactly as recorded."
			: $"{wrong} of the recorded answers differed.");

		return wrong == 0 ? 0 : 1;
	}
}
