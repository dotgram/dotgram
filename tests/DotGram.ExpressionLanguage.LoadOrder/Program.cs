using System;
using System.Linq;
using System.Runtime.CompilerServices;

using DotGram.ExpressionLanguage;

namespace DotGram.ExpressionLanguage.LoadOrder;

/// <summary>
/// What the reader answers about a name depends on what the process has already loaded, and this
/// is the only shape that can see it.
/// </summary>
/// <remarks>
/// <para>
/// <c>Names.Loaded</c> searches <c>AppDomain.CurrentDomain.GetAssemblies()</c>, so a type in an
/// assembly the process has not touched is not there to be found — and a referenced assembly is
/// loaded lazily, at its first use. A consumer naming a type from their own dependency therefore
/// gets an answer that depends on what else their program has done first.
/// </para>
/// <para>
/// No test beside the others can show this: a test assembly has loaded everything it references
/// long before the first test runs, and an <c>AssemblyLoadContext</c> does not help either, since
/// <c>GetAssemblies()</c> sees every context in the process. Hence a program, and hence a project.
/// </para>
/// <para>
/// The touch is in a method of its own, never called before the first parse, and marked
/// <c>NoInlining</c>: naming a type in the body of the method that runs first loads its assembly
/// when that method is compiled, which is what a first attempt at this measured instead of the
/// thing it meant to.
/// </para>
/// <para>
/// It prints what happened and exits 0 when the answers were the ones recorded below, 1 otherwise,
/// naming what differed. The test that runs it reports that text, so a red run in CI says what is
/// wrong rather than only that something is.
/// </para>
/// </remarks>
static class Probe
{
	/// <summary>A type from an assembly this program does not otherwise use, named in full.</summary>
	const string Text = "() => DotGram.Web.MediaType.Parse(\"text/plain\")";

	static bool Loaded() =>
		AppDomain.CurrentDomain.GetAssemblies().Any(one => one.GetName().Name == "DotGram.Web");

	[MethodImpl(MethodImplOptions.NoInlining)]
	static string Touch() => typeof(DotGram.Web.MediaType).FullName!;

	static int Main()
	{
		var wrong = 0;

		var loadedFirst = Loaded();

		Console.WriteLine($"1. DotGram.Web loaded before any parse: {loadedFirst}");

		if (loadedFirst)
		{
			Console.WriteLine("   WRONG: nothing should have loaded it yet, so this run proves nothing.");

			wrong++;
		}

		var before = ExpressionParser.TryParse(Text);

		Console.WriteLine($"2. parse before touching it: success={before.IsSuccess} error={before.Error}");

		// Today this refuses, and the test holds that rather than the behaviour we want, so that the
		// suite stays green while the decision is open: whether the library may load a caller's
		// referenced assemblies on a miss is Igor's, not this program's. When it is taken, the line
		// below becomes `if (!before.IsSuccess)` and this comment goes with it.
		if (before.IsSuccess)
		{
			Console.WriteLine(
				"   CHANGED: a name from an untouched assembly now resolves. If the reader was given " +
				"the retry-after-loading-references behaviour, this program should now expect success " +
				"at step 2 and this branch is the reminder to change it.");

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
				"   WRONG: the same text still does not parse once the assembly is loaded. Either the " +
				"AssemblyLoad subscription in Names.Loaded stopped clearing the caches, or the name " +
				"cannot be resolved for a reason that has nothing to do with load order.");

			wrong++;
		}

		Console.WriteLine(wrong == 0
			? "OK: the answer depends on load order, exactly as recorded."
			: $"{wrong} of the recorded answers differed.");

		return wrong == 0 ? 0 : 1;
	}
}
