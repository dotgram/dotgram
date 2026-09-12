using System;
using System.Linq.Expressions;
using System.Reflection;

namespace DotGram.ExpressionLanguage;

// What a member is, asked apart from the text that wrote it.
//
// Everything the resolution beside this file does — which members a type has, which
// overload the arguments fit, what converts to what — is a question about types and about
// the assemblies loaded, and it needs exactly two things from the reading: the assembly the
// text is read for, and the namespaces its `using`s named. Given those two it answers
// without a parser, which is what lets it be asked without one.
//
// Nested rather than standing on its own, as `State` is, because it reaches the statics
// beside it. A type of its own here would have meant opening a dozen members to the
// assembly to say the same thing.

public static partial class ExpressionParser
{
	/// <summary>A member chosen, and the arguments as that member takes them.</summary>
	/// <remarks>
	/// Both halves, on purpose. Choosing an overload and converting the arguments to what it
	/// takes are one act — the conversions are how the choice was made — and a caller handed
	/// only the member would have to work them out a second time, by a second rule.
	/// </remarks>
	public readonly record struct Resolution(MemberInfo Member, Expression[] Arguments);

	/// <summary>What a member is, for a text read on behalf of one assembly with these `using`s.</summary>
	/// <remarks>
	/// The `using`s are the list the reading fills as it goes, not a copy of it: a directive
	/// is recorded while the text is read, and this must see what has been recorded by the
	/// time it is asked.
	/// </remarks>
	public sealed class MemberResolver
	{
		public MemberResolver(Assembly caller, IReadOnlyList<string>? imports)
		{
			Caller   = caller ?? throw new ArgumentNullException(nameof(caller));
			_imports = imports;
		}

		readonly IReadOnlyList<string>? _imports;

		/// <summary>The assembly the text is read for, whose internal members it may name.</summary>
		public Assembly Caller { get; }

		/// <summary>A call on a value: its own method where it has one, an extension where it has none.</summary>
		/// <remarks>
		/// C#'s order, and C#'s reason for it. An extension method is looked for only where
		/// nothing of the receiver's own fits, so a `using` can never take a method away from
		/// the type that declares one — bringing a namespace into a text changes what names
		/// mean, and must not change what a type does.
		/// </remarks>
		/// <returns>
		/// A static method where the winner is an extension, the receiver standing first among
		/// the arguments; an instance method otherwise. Which it is, the member says.
		/// </returns>
		public Resolution Method(Expression target, string name, Expression[] arguments)
		{
			if (target is null)
				throw new ArgumentNullException(nameof(target));

			if (arguments is null)
				throw new ArgumentNullException(nameof(arguments));

			var missing = $"'{target.Type.Name}' has no method '{name}'";

			if (Methods(target.Type, name, instance: true, arguments, Caller) is { Count: > 0 } own)
				return Chose(own, arguments, missing);

			var extended = new Expression[arguments.Length + 1];

			extended[0] = target;

			arguments.CopyTo(extended, 1);

			if (Extensions(name, extended, Caller, _imports) is { Count: > 0 } found)
				return Chose(found, extended, $"nothing extends '{target.Type.Name}' with '{name}'");

			// Neither, which is said in the words the language has always used for a method
			// that is not there — the empty list is what says there was nothing to choose from.
			return Chose([], arguments, missing);
		}

		/// <summary>A call on a type: the static method C# would choose for these arguments.</summary>
		public Resolution Static(Type type, string name, Expression[] arguments) =>
			type is null
				? throw new ArgumentNullException(nameof(type))
				: Chose(
					Methods(type, name, instance: false, arguments, Caller), arguments,
					$"'{type.Name}' has no method '{name}'");

		/// <summary>The constructor C# would choose for these arguments.</summary>
		/// <remarks>
		/// A value type's constructor of no arguments is in no metadata and is nobody's to
		/// choose: that one is <c>Expression.New</c>'s own form, and the caller reads it before
		/// asking here.
		/// </remarks>
		public Resolution Constructor(Type type, Expression[] arguments) =>
			type is null
				? throw new ArgumentNullException(nameof(type))
				: Chose(Constructing(type, arguments, Caller), arguments, $"'{type.Name}' has no constructor");

		/// <summary>The indexer C# would choose for these indices.</summary>
		/// <remarks>
		/// An array's element is no indexer — it is a node of the tree — and the caller reads
		/// that before asking here, for the reason a value type's default constructor is read
		/// before asking about constructors.
		/// </remarks>
		public Resolution Indexer(Expression target, Expression[] indices) =>
			target is null
				? throw new ArgumentNullException(nameof(target))
				: Chose(
					Indexers(target.Type, indices, Caller), indices,
					$"'{target.Type.Name}' has no indexer");

		/// <summary>A delegate's <c>Invoke</c>, with the arguments it takes.</summary>
		/// <remarks>
		/// One candidate and nothing to choose between, so this asks whether it applies and
		/// says no in the API's own words where it does not.
		/// </remarks>
		public Resolution Delegated(Expression target, Expression[] arguments)
		{
			if (target is null)
				throw new ArgumentNullException(nameof(target));

			if (target.Type.GetMethod("Invoke") is null)
				throw new InvalidOperationException($"'{target.Type.Name}' is not a delegate.");

			return Invoking(target, arguments);
		}
	}
}
