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
readonly record struct Question(string Name, int Arity);

/// <param name="Yes">Whether the host has it.</param>
/// <param name="Role">What kind of method it is, meaningless unless the question was one.</param>
readonly record struct Answer(Question Asked, bool Yes, MethodRole Role);

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
		var imports = new List<string>();
		var names   = new List<Question>();

		Collect(file.Usings, file.Decls);

		var questions = ImmutableHashSet.CreateBuilder<Question>();

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
			if (type is not null)
				names.Add(new Question(type.Name, -1));
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
			answers.Add(question.Arity < 0
				? new Answer(question, resolver.TypeExists(question.Name), default)
				: new Answer(
					question,
					resolver.TryResolveMethod(question.Name, question.Arity, out var role),
					role));

		return answers.ToImmutable();
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

	public bool TryResolveMethod(string qualifiedName, int argumentCount, out MethodRole role)
	{
		var answer = Look(new Question(qualifiedName, argumentCount));

		role = answer.Role;

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
