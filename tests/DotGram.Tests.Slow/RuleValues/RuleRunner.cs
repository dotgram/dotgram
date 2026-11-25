using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

using DotGram.Generation;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DotGram.Tests.RuleValues;

/// <summary>
/// What became of one input of one grammar under one generator.
/// </summary>
/// <param name="Kind">
/// <c>value</c> (it parsed), <c>fail</c> (it compiled and did not match the input), <c>throws</c> (the parser threw
/// something other than a failed match), <c>gram</c> (the generator reported an error), <c>csharp</c> (what it wrote
/// does not compile), <c>crash</c> (the generator threw).
/// </param>
/// <param name="Text">The value as printed, or the diagnostic ids and the first message.</param>
/// <param name="Warnings">The ids of the warnings of the generator and of the C# compiler, sorted, comma-separated.</param>
sealed record Outcome(string Kind, string Text, string Warnings)
{
	/// <summary>The grammar compiled: its outcome is a value or a failed match.</summary>
	public bool Built => Kind is "value" or "fail";

	/// <summary>The C# compiler warned of a possible null (CS86xx).</summary>
	public bool WarnedNull => Warnings.Split(',').Any(static w => w.StartsWith("CS86", StringComparison.Ordinal));

	public override string ToString()
	{
		var shown = Kind is "value" ? Text : Kind.ToUpperInvariant() + " " + Text;

		return Warnings.Length == 0 ? shown : shown + " (warnings " + Warnings + ")";
	}
}

/// <summary>
/// One grammar in one mode: a host class of its own in a syntax tree of its own.
/// </summary>
sealed class Job(RuleShape shape, PrintedGrammar printed, Mode mode, string className)
{
	public RuleShape Shape { get; } = shape;

	public PrintedGrammar Printed { get; } = printed;

	public Mode Mode { get; } = mode;

	public string ClassName { get; } = className;

	/// <summary>The outcome per route, from the generator of the package and from the head.</summary>
	public Outcome[] Package { get; set; } = [];

	public Outcome[] Head { get; set; } = [];
}

/// <summary>
/// A generator the gate compiles with.
/// </summary>
sealed class GeneratorSide
{
	GeneratorSide(string name, Func<ISourceGenerator> create)
	{
		Name   = name;
		Create = create;
	}

	public string Name { get; }

	public Func<ISourceGenerator> Create { get; }

	/// <summary>The generator of this build.</summary>
	public static readonly GeneratorSide HeadSide = new("head", static () => new GramGenerator().AsSourceGenerator());

	/// <summary>
	/// The generator of the published package DotGram 0.3.0, loaded from the copy the project file takes out of the NuGet
	/// cache (<c>PackageDownload</c>) into a load context of its own. Roslyn is the host's: only DotGram.dll is the
	/// package's.
	/// </summary>
	public static readonly Lazy<GeneratorSide> PackageSide = new(static () =>
	{
		var path     = Path.Combine(AppContext.BaseDirectory, "DotGram030", "DotGram.dll");
		var context  = new PackageContext(path);
		var assembly = context.LoadFromAssemblyPath(path);
		var type     = assembly.GetTypes().First(static t => typeof(IIncrementalGenerator).IsAssignableFrom(t) && !t.IsAbstract);

		return new GeneratorSide("v0.3.0", () => ((IIncrementalGenerator)Activator.CreateInstance(type)!).AsSourceGenerator());
	});

	sealed class PackageContext(string path) : AssemblyLoadContext("DotGram 0.3.0", isCollectible: false)
	{
		protected override Assembly? Load(AssemblyName name)
		{
			return name.Name == "DotGram" ? LoadFromAssemblyPath(path) : null;
		}
	}
}

/// <summary>
/// Compiles a batch of jobs with one generator and runs what compiled. Many host classes go into one compilation, each in
/// its own syntax tree; a diagnostic belongs to the job whose tree it is in (a generated tree belongs to the job whose
/// class it extends). A job with an error is taken out and the rest emitted again: a batch is never split to find a
/// culprit. Each batch's assembly is loaded into a collectible context and unloaded after it ran.
/// </summary>
static class RuleRunner
{
	static readonly CSharpParseOptions Parse = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);

	static readonly CSharpCompilationOptions Options = new(
		OutputKind.DynamicallyLinkedLibrary,
		nullableContextOptions: NullableContextOptions.Enable,
		concurrentBuild: false);

	/// <summary>The framework, and nothing of DotGram: the generator writes all a grammar needs into the host.</summary>
	static readonly ImmutableArray<MetadataReference> References =
	[
		.. ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
			.Split(Path.PathSeparator)
			.Where(static p => Path.GetFileName(p) is var f && (f.StartsWith("System.", StringComparison.Ordinal) || f is "netstandard.dll" or "mscorlib.dll"))
			.Select(static p => (MetadataReference)MetadataReference.CreateFromFile(p)),
	];

	/// <summary>Counts of what happened that a run should report: batches whose generator threw as a whole, and the like.</summary>
	public static int WholeBatchCrashes;

	/// <summary>Diagnostics that could not be given to a job (no location in a job's tree).</summary>
	public static int Unattributed;

	/// <summary>Compiles and runs the batch with the side's generator, and stores the outcomes on the jobs.</summary>
	public static void Run(IReadOnlyList<Job> batch, GeneratorSide side)
	{
		var outcomes = Compile(batch, side);

		foreach (var job in batch)
		{
			if (side.Name == "head")
				job.Head = outcomes[job];
			else
				job.Package = outcomes[job];
		}
	}

	static Dictionary<Job, Outcome[]> Compile(IReadOnlyList<Job> batch, GeneratorSide side)
	{
		var trees = batch.ToDictionary(
			static job => CSharpSyntaxTree.ParseText(job.Printed.Host(job.Mode, job.ClassName), Parse, job.ClassName + ".cs"),
			static job => job);

		var compilation = CSharpCompilation.Create("RuleValues" + Guid.NewGuid().ToString("N"), trees.Keys, References, Options);

		var run = CSharpGeneratorDriver
			.Create([side.Create()], parseOptions: Parse)
			.RunGeneratorsAndUpdateCompilation(compilation, out var output, out _)
			.GetRunResult();

		var result = new Dictionary<Job, Outcome[]>();

		if (run.Results.Any(static r => r.Exception is not null))
		{
			// The generator threw for the whole run: nothing can be given to one job. Each job is run alone, which is the
			// one case a batch is taken apart, and is counted.
			if (batch.Count == 1)
			{
				var exception = run.Results.First(static r => r.Exception is not null).Exception!;

				result[batch[0]] = Same(batch[0], new Outcome("crash", exception.GetType().Name + ": " + OneLine(exception.Message), ""));
				return result;
			}

			System.Threading.Interlocked.Increment(ref WholeBatchCrashes);

			foreach (var job in batch)
				foreach (var (key, value) in Compile([job], side))
					result[key] = value;

			return result;
		}

		// Generated trees belong to the job whose class they extend.
		var owner = new Dictionary<SyntaxTree, Job>();

		foreach (var (tree, job) in trees)
			owner[tree] = job;

		var byClass = batch.ToDictionary(static j => j.ClassName);

		foreach (var generated in run.Results.SelectMany(static r => r.GeneratedSources))
		{
			var hint = generated.HintName;
			var job  = batch.FirstOrDefault(j => hint.StartsWith(j.ClassName + ".", StringComparison.Ordinal) || hint.Contains("." + j.ClassName + ".", StringComparison.Ordinal));

			if (job is null)
			{
				var text = generated.SourceText.ToString();

				job = batch.FirstOrDefault(j => text.Contains("partial class " + j.ClassName, StringComparison.Ordinal));
			}

			if (job is not null)
				owner[generated.SyntaxTree] = job;
		}

		var errors   = new Dictionary<Job, List<Diagnostic>>();
		var warnings = batch.ToDictionary(static j => j, static _ => new SortedSet<string>(StringComparer.Ordinal));

		bool Attribute(Diagnostic diagnostic, Dictionary<Job, List<Diagnostic>> into)
		{
			var tree = diagnostic.Location.SourceTree;

			if (tree is null || !owner.TryGetValue(tree, out var job))
			{
				if (diagnostic.Severity == DiagnosticSeverity.Error)
					System.Threading.Interlocked.Increment(ref Unattributed);

				return false;
			}

			if (diagnostic.Severity == DiagnosticSeverity.Error)
			{
				if (!into.TryGetValue(job, out var list))
					into[job] = list = [];

				list.Add(diagnostic);
			}
			else if (diagnostic.Severity == DiagnosticSeverity.Warning)
			{
				warnings[job].Add(diagnostic.Id);
			}

			return true;
		}

		var gram = new Dictionary<Job, List<Diagnostic>>();

		foreach (var diagnostic in run.Diagnostics)
			Attribute(diagnostic, gram);

		foreach (var (job, list) in gram)
		{
			var crash = list.Any(static d => d.Id == "GRAM0001");

			result[job] = Same(job, new Outcome(crash ? "crash" : "gram", Ids(list) + " " + OneLine(list[0].GetMessage(CultureInfo.InvariantCulture)), ""));
		}

		// What compiled: emit, take out the jobs with C# errors, emit again.
		var removed = new HashSet<Job>(gram.Keys);
		var current = output.RemoveSyntaxTrees(owner.Where(p => removed.Contains(p.Value)).Select(static p => p.Key));

		using var image = new MemoryStream();

		for (var round = 0; ; round++)
		{
			image.SetLength(0);

			var emitted = current.Emit(image);
			var csharp  = new Dictionary<Job, List<Diagnostic>>();

			foreach (var set in warnings.Values)
				set.RemoveWhere(static w => w.StartsWith("CS", StringComparison.Ordinal));

			foreach (var diagnostic in emitted.Diagnostics)
				Attribute(diagnostic, csharp);

			if (emitted.Success)
				break;

			if (csharp.Count == 0 || round == 8)
			{
				// Errors nobody owns: every job left is reported with the first of them.
				var first = emitted.Diagnostics.First(static d => d.Severity == DiagnosticSeverity.Error);

				foreach (var job in batch.Where(j => !removed.Contains(j)))
					result[job] = Same(job, new Outcome("csharp", "UNOWNED " + first.Id + " " + OneLine(first.GetMessage(CultureInfo.InvariantCulture)), ""));

				return result;
			}

			foreach (var (job, list) in csharp)
			{
				result[job] = Same(job, new Outcome("csharp", Ids(list) + " " + OneLine(list[0].GetMessage(CultureInfo.InvariantCulture)), string.Join(",", warnings[job])));
				removed.Add(job);
			}

			current = current.RemoveSyntaxTrees(owner.Where(p => csharp.ContainsKey(p.Value)).Select(static p => p.Key));
		}

		var context = new AssemblyLoadContext("RuleValues", isCollectible: true);

		try
		{
			image.Position = 0;

			var assembly = context.LoadFromStream(image);

			foreach (var job in batch.Where(j => !removed.Contains(j)))
			{
				var type  = assembly.GetType(job.ClassName)!;
				var shown = string.Join(",", warnings[job]);

				result[job] = [.. job.Printed.Routes.Select(route => Invoke(type, job.Mode, route.Input, shown))];
			}
		}
		finally
		{
			context.Unload();
		}

		return result;
	}

	static Outcome[] Same(Job job, Outcome outcome)
	{
		return [.. job.Printed.Routes.Select(_ => outcome)];
	}

	static string Ids(IEnumerable<Diagnostic> diagnostics)
	{
		return string.Join(",", diagnostics.Select(static d => d.Id).Distinct().Order(StringComparer.Ordinal));
	}

	static Outcome Invoke(Type type, Mode mode, string input, string warnings)
	{
		try
		{
			object? value;

			if (mode == Mode.B)
			{
				var by   = type.GetMethods(BindingFlags.Public | BindingFlags.Static).First(static m => m.Name == "By" && m.GetParameters() is [{ ParameterType: var p }, ..] && p == typeof(Stream));
				var args = by.GetParameters().Select(static p => p.HasDefaultValue ? p.DefaultValue : null).ToArray();

				args[0] = new MemoryStream(Encoding.UTF8.GetBytes(input));
				value   = by.Invoke(null, args);
			}
			else
			{
				value = type.GetMethod("ParseStart", [typeof(string)])!.Invoke(null, [input]);
			}

			return new Outcome("value", Show(value), warnings);
		}
		catch (TargetInvocationException e) when (e.InnerException is { } inner)
		{
			// A failed match is what the parser says of input it does not read; anything else it throws is a defect.
			var failed = inner.GetType().Name.Contains("Parse", StringComparison.Ordinal) || inner is FormatException;

			return new Outcome(failed ? "fail" : "throws", failed ? "" : inner.GetType().Name + ": " + OneLine(inner.Message), warnings);
		}
	}

	/// <summary>
	/// Prints a value so that it names its source: strings quoted, sequences in brackets, spans as their bounds
	/// <c>[start..end)</c>, generated types as their members, host types as their own <c>ToString</c>.
	/// </summary>
	public static string Show(object? value)
	{
		if (value is null)
			return "<null>";

		if (value is string text)
			return "\"" + text + "\"";

		if (value is char c)
			return "'" + c + "'";

		if (value is byte[] bytes)
			return "bytes\"" + Encoding.UTF8.GetString(bytes) + "\"";

		if (value is IEnumerable sequence)
			return "[" + string.Join(",", sequence.Cast<object?>().Select(Show)) + "]";

		var type = value.GetType();

		if (type.Name == "SourceSpan")
			return "[" + type.GetProperty("Start")!.GetValue(value) + ".." + type.GetProperty("End")!.GetValue(value) + ")";

		if (type.Name is "Box" or "Pr" || type.IsPrimitive)
			return Convert.ToString(value, CultureInfo.InvariantCulture)!;

		if (type.Name.StartsWith("Match", StringComparison.Ordinal) && type.GetProperty("IsSuccess") is { } success)
			return success.GetValue(value) is false ? "FAIL" : Show(type.GetProperty("Value")!.GetValue(value));

		// A type the generator made: its public members, in declaration order.
		var members = type.GetFields(BindingFlags.Public | BindingFlags.Instance)
			.Select(f => f.Name + "=" + Show(f.GetValue(value)))
			.Concat(type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.Where(static p => p.GetIndexParameters().Length == 0)
				.Select(p => p.Name + "=" + Show(p.GetValue(value))));

		return type.Name + "{" + string.Join(",", members) + "}";
	}

	static string OneLine(string text)
	{
		return text.Replace("\r", " ", StringComparison.Ordinal).Replace("\n", " ", StringComparison.Ordinal);
	}
}
