using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace DotGram.Ir.Prototype;

/// <summary>
/// One input read two ways: by the evaluator over the lowered grammar, and by the C# parser the
/// generator wrote for the same grammar, which is the one believed.
/// </summary>
static class Reading
{
	static readonly ConcurrentDictionary<(string, IrLowering.Options?), IrProgram> _programs = new();

	public static IrProgram Program(string grammar, IrLowering.Options? options = null)
	{
		return _programs.GetOrAdd((grammar, options), static key => IrLowering.Lower(Graphs.Of(key.Item1), key.Item2));
	}

	/// <summary>What the evaluator makes of an input, and the host calls it made on the way.</summary>
	public static (IrOutcome Outcome, List<string> Log, IrEvaluator Evaluator) Evaluated(
		string grammar, string entry, string input,
		Func<IrFragment, object?[], bool>? guards = null, string? throwOn = null, IrLowering.Options? options = null)
	{
		var host      = new RecordingHost(guards, throwOn);
		var evaluator = new IrEvaluator(Program(grammar, options), host);

		return (evaluator.Parse(entry, input), host.Log, evaluator);
	}

	/// <summary>What the generated C# parser makes of it, in the same terms.</summary>
	public static (IrOutcome Outcome, List<string> Log, string? Error) Generated(
		Type parser, string entry, string input, string? throwOn = null)
	{
		var method = parser.GetMethod("Try" + entry, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, [typeof(string)])
			?? throw new MissingMethodException(parser.Name, "Try" + entry);

		object? match = null;

		var log = Twin.Record(() =>
		{
			try
			{
				match = method.Invoke(null, [input]);
			}
			catch (TargetInvocationException thrown) when (thrown.InnerException is { } inner)
			{
				System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(inner).Throw();
			}
		}, throwOn);

		var type     = match!.GetType();
		var accepted = (bool)type.GetProperty("IsSuccess")!.GetValue(match)!;
		var position = (long)type.GetProperty("Position")!.GetValue(match)!;
		var value    = accepted ? type.GetProperty("Value")!.GetValue(match) : null;
		var error    = (string?)type.GetProperty("Error")!.GetValue(match);

		return (new IrOutcome(accepted, accepted ? input.Length : (int)position, value, []), log, error);
	}
}
