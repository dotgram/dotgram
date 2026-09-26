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

	static bool Loaded()
	{
		return AppDomain.CurrentDomain.GetAssemblies().Any(one => one.GetName().Name == "DotGram.Web");
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	static string Touch()
	{
		return typeof(DotGram.Web.MediaType).FullName!;
	}

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

		// It resolves, and that is the answer now. The decision this branch was waiting for was
		// taken on 2026-09-26: a reading's names are looked for in the calling assembly and what
		// it REFERENCES, as a compilation's are, rather than in whatever the process happens to
		// have loaded — so an assembly the graph names is loaded when the scope is made, and a
		// type in it is nameable before anything has touched it. What this mode now checks is
		// that: step 2 succeeds, and step 3 agrees with it.
		if (!before.IsSuccess)
		{
			Console.WriteLine(
				"   WRONG: a name from a referenced assembly did not resolve. A scope is the " +
				"caller and what it references, so this should not depend on anything having " +
				"touched that assembly first.");

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
				"   WRONG: the same text does not parse even once the assembly is loaded, so the " +
				"name cannot be resolved for a reason that has nothing to do with load order. " +
				"There is no AssemblyLoad subscription to blame any more: a scope's assemblies " +
				"are fixed when it is made.");

			wrong++;
		}

		Console.WriteLine(wrong == 0
			? "OK: the answer does not depend on load order, which is what a scope is for."
			: $"{wrong} of the recorded answers differed.");

		return wrong == 0 ? 0 : 1;
	}
}
