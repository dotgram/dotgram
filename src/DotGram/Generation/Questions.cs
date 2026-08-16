using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using DotGram.Grammar;
using DotGram.Grammar.Parsing;

namespace DotGram.Generation;

/// <summary>What a grammar needs to know about the host's C#, and what it was told.</summary>
/// <param name="Name">A qualified C# name, exactly as it will be asked for.</param>
/// <param name="Arity">
/// The number of arguments a method was called with, or -1 when the question is whether a
/// type of this name exists.
/// </param>
/// <param name="Against">
/// The type <paramref name="Name"/> is being asked to fit into, for an assignability
/// question (§4.1 case 2), and null for every other kind.
/// </param>
readonly record struct Question(string Name, int Arity, string? Against = null)
{
	/// <summary>The arity that marks a question about types rather than about a name.</summary>
	public const int Assignability = -2;

	/// <summary>And the one that asks what a type can be built with (§7.3).</summary>
	public const int Constructors = -3;

	/// <summary>And what can be set on it once it is (§7.3's second way).</summary>
	public const int Properties = -4;

	public static Question Fits(string from, string to) => new(from, Assignability, to);

	public static Question Builds(string type) => new(type, Constructors);

	public static Question Sets(string type) => new(type, Properties);
}

/// <param name="Yes">Whether the host has it.</param>
/// <param name="Role">What kind of method it is, meaningless unless the question was one.</param>
/// <param name="Constructors">
/// What the type can be built with, each constructor as its parameters in order (§7.3).
/// Empty unless the question asked.
/// </param>
/// <remarks>
/// Everything here compares by value, which is the whole point of the stage: an answer
/// that changed by identity alone would rebuild the parser on every keystroke.
/// </remarks>
readonly record struct Answer(
	Question Asked,
	bool Yes,
	MethodRole Role,
	EquatableArray<EquatableArray<MethodParameter>> Constructors = default,
	EquatableArray<ObjectMember> Properties = default);

/// <summary>
/// Everything a grammar could ask the host compilation, worked out from its text alone.
/// </summary>
/// <remarks>
/// <para>
/// This is what lets the expensive half of generation be cached. Binding needs a
/// <c>Compilation</c> — it has to know whether <c>@int.Parse</c> is a real method — and a
/// <c>Compilation</c> is a new object after every keystroke, so anything downstream of it
/// is recomputed for every character typed. Asking the questions first turns that
/// dependency into a small list of answers, which compares by value and hardly ever
/// changes.
/// </para>
/// <para>
/// <b>A superset, deliberately.</b> The set is built from the grammar's syntax rather
/// than by watching the binder ask, because the binder stops asking as soon as one answer
/// satisfies it: <c>TypeInView</c> tries the bare name and then each import in turn, so
/// what it asks depends on what it was told, and a recording pass would record a
/// different set from the one the real pass needs. Every name crossed with every import
/// is more questions than are needed and always includes the ones that are.
/// </para>
/// </remarks>
static class Questions
{
	/// <summary>Every question the grammar's C# names could give rise to.</summary>
	public static ImmutableArray<Question> Of(GrammarFile file)
	{
		var imports   = new List<string>();
		var names     = new List<Question>();
		var declared  = new List<string>();
		var sequences = new List<string>();

		Collect(file.Usings, file.Decls);

		var questions = ImmutableHashSet.CreateBuilder<Question>();

		// §4.1 case 2 asks which of the grammar's own result types fit into a sequence's
		// element type, and it asks after binding — so every pairing is asked for here.
		// The same superset as everywhere in this file: a grammar declaring five types and
		// one sequence asks five questions, of which the ones that matter are a subset.
		foreach (var element in sequences)
			foreach (var type in declared)
				questions.Add(Question.Fits(type, element));

		// §7.3: a rule declaring a type may have it built from its captures, so what every
		// declared type can be built with is asked for. The same superset as the rest of
		// this file — a type that turns out to be built by a `=>` was asked about anyway.
		foreach (var type in declared)
		{
			questions.Add(Question.Builds(type));
			questions.Add(Question.Sets(type));
		}

		foreach (var name in names)
		{
			questions.Add(name);

			// A method is asked for by name and arity; a type only by name. Both are asked
			// unqualified first and then under each import, the way C# itself resolves.
			questions.Add(name with { Arity = -1 });

			foreach (var import in imports)
			{
				questions.Add(name with { Name = import + "." + name.Name });
				questions.Add(new Question(import + "." + name.Name, -1));
			}
		}

		return [.. questions];

		void Collect(IReadOnlyList<Using> usings, IReadOnlyList<Decl> declarations)
		{
			foreach (var import in usings)
				if (import.IsCSharp && !imports.Contains(import.Name))
					imports.Add(import.Name);

			foreach (var declaration in declarations)
				switch (declaration)
				{
					case Decl.Rule(_, _, var type, var body):
						Type(type);
						Walk(body);
						break;

					case Decl.Scope(_, var nested, var inner):
						Collect(nested, inner);
						break;
				}
		}

		void Type(TypeRef? type)
		{
			if (type is null)
				return;

			names.Add(new Question(type.Name, -1));

			(type.IsSequence ? sequences : declared).Add(type.Name);

			// A sequence's element type is a type in its own right, and a rule declaring
			// `: T[]` may itself be an element of another sequence.
			if (type.IsSequence && !declared.Contains(type.Name))
				declared.Add(type.Name);
		}

		void Walk(Expr expression)
		{
			switch (expression)
			{
				// `@Name(a, b)` — a method of two arguments, or a type if it is not one.
				case Expr.Call(var target, var arguments) when target.IsCSharp:
					names.Add(new Question(target.Name, arguments.Count));
					break;

				// `@Name` on its own is asked for as a method of none, then as a type.
				case Expr.Reference(true, var name, var typeArguments):
					names.Add(new Question(name, 0));

					foreach (var argument in typeArguments)
						Type(argument);

					break;

				case Expr.Reference(false, _, var typeArguments):
					foreach (var argument in typeArguments)
						Type(argument);

					break;
			}

			foreach (var child in Dump.Children(expression))
				Walk(child);
		}
	}

	/// <summary>Asks the host, once per question.</summary>
	public static ImmutableArray<Answer> Ask(ImmutableArray<Question> questions, ISymbolResolver resolver)
	{
		var answers = ImmutableArray.CreateBuilder<Answer>(questions.Length);

		foreach (var question in questions)
			answers.Add(question.Arity switch
			{
				Question.Assignability =>
					new Answer(question, resolver.IsAssignable(question.Name, question.Against!), default),

				Question.Constructors => new Answer(
					question,
					resolver.TryResolveConstructors(question.Name, out var constructors),
					default,
					Shapes(constructors)),

				Question.Properties => new Answer(
					question,
					resolver.TryResolveSettableProperties(question.Name, out var properties),
					default,
					default,
					new EquatableArray<ObjectMember>([.. properties])),

				< 0 => new Answer(question, resolver.TypeExists(question.Name), default),

				_ => new Answer(
					question,
					resolver.TryResolveMethod(question.Name, question.Arity, out var role),
					role),
			});

		return answers.ToImmutable();
	}

	/// <summary>The host's answer as something that compares by value.</summary>
	static EquatableArray<EquatableArray<MethodParameter>> Shapes(
		IReadOnlyList<IReadOnlyList<MethodParameter>> constructors)
	{
		var shapes = ImmutableArray.CreateBuilder<EquatableArray<MethodParameter>>(constructors.Count);

		foreach (var constructor in constructors)
			shapes.Add(new EquatableArray<MethodParameter>([.. constructor]));

		return new EquatableArray<EquatableArray<MethodParameter>>(shapes.ToImmutable());
	}
}

/// <summary>
/// A resolver that consults a list of answers rather than a compilation.
/// </summary>
/// <remarks>
/// What the binder talks to once the questions have been asked, so that nothing
/// downstream of it holds a <c>Compilation</c> and everything downstream of it can be
/// cached. Asked something that was not collected it answers no — which would be a
/// wrong answer, so <see cref="Missed"/> records it and a test insists the list is empty.
/// </remarks>
sealed class AnsweredSymbolResolver(ImmutableArray<Answer> answers) : ISymbolResolver
{
	readonly Dictionary<Question, Answer> _answers = Index(answers);

	static Dictionary<Question, Answer> Index(ImmutableArray<Answer> answers)
	{
		var index = new Dictionary<Question, Answer>();

		foreach (var answer in answers)
			index[answer.Asked] = answer;

		return index;
	}

	public bool TypeExists(string qualifiedName) => Look(new Question(qualifiedName, -1)).Yes;

	public bool IsAssignable(string from, string to) =>
		string.Equals(from, to, StringComparison.Ordinal) || Look(Question.Fits(from, to)).Yes;

	public bool TryResolveMethod(string qualifiedName, int argumentCount, out MethodRole role)
	{
		var answer = Look(new Question(qualifiedName, argumentCount));

		role = answer.Role;

		return answer.Yes;
	}

	public bool TryResolveSettableProperties(string qualifiedName, out IReadOnlyList<ObjectMember> properties)
	{
		var answer = Look(Question.Sets(qualifiedName));

		properties = answer.Properties.Items;

		return answer.Yes;
	}

	public bool TryResolveConstructors(
		string qualifiedName, out IReadOnlyList<IReadOnlyList<MethodParameter>> constructors)
	{
		var answer = Look(Question.Builds(qualifiedName));
		var found  = new List<IReadOnlyList<MethodParameter>>();

		foreach (var shape in answer.Constructors.Items)
			found.Add(shape.Items);

		constructors = found;

		return answer.Yes;
	}

	/// <summary>
	/// The answer, or a failure — never a guess.
	/// </summary>
	/// <remarks>
	/// A question nobody foresaw cannot be answered here: the host is no longer in reach,
	/// and answering "no" would be a wrong answer rather than a slow one — a grammar
	/// refused for naming a type that exists. Throwing reaches the consumer as CS8785 and
	/// no parser, which is bad but visible; the alternative is a diagnostic they cannot
	/// act on about a type they spelled correctly.
	/// </remarks>
	Answer Look(Question question) =>
		_answers.TryGetValue(question, out var answer)
			? answer
			: throw new InvalidOperationException(
				$"The question collector did not foresee '{question.Name}'" +
				(question.Arity < 0 ? " as a type" : $" as a method of {question.Arity}") +
				". This is a defect in DotGram.Generation.Questions, not in the grammar.");
}
