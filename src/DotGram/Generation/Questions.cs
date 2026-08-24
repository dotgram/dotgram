using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using DotGram.Grammar;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

namespace DotGram.Generation;

/// <summary>What a grammar needs to know about the host's C#, and what it was told.</summary>
/// <param name="Name">A qualified C# name, exactly as it will be asked for.</param>
/// <param name="Kind">
/// One of the constants below, identifying the type relationship being asked about.
/// </param>
/// <param name="Against">
/// The type <paramref name="Name"/> is being asked to fit into, for an assignability
/// question (§4.1 case 2), and null for every other kind.
/// </param>
readonly record struct Question(string Name, int Kind, string? Against = null)
{
	/// <summary>The kind that asks whether a type exists.</summary>
	public const int Exists = -1;

	/// <summary>The kind that asks whether one type fits another.</summary>
	public const int Assignability = -2;

	/// <summary>And the one that asks what a type can be built with (§7.3).</summary>
	public const int Constructors = -3;

	/// <summary>And what can be set on it once it is (§7.3's second way).</summary>
	public const int Properties = -4;

	/// <summary>And whether a bare `@Name` recognizer hands back a value of its own (§7.1).</summary>
	public const int ExternalValue = -5;

	public static Question Fits(string from, string to) => new(from, Assignability, to);

	public static Question Builds(string type) => new(type, Constructors);

	public static Question Sets(string type) => new(type, Properties);

	/// <param name="against">
	/// The type <c>T</c> would have to fit for a whole rule's body to be exactly this call,
	/// or null for a captured or otherwise nested use, which only asks what <c>T</c> is.
	/// </param>
	public static Question ValueOf(string method, string? against = null) => new(method, ExternalValue, against);
}

/// <param name="Yes">Whether the host has it.</param>
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
	EquatableArray<EquatableArray<MethodParameter>> Constructors = default,
	EquatableArray<ObjectMember> Properties = default,
	string? ExternalType = null,
	bool ExternalAmbiguous = false);

/// <summary>
/// Everything a grammar could ask the host compilation, worked out from its text alone.
/// </summary>
/// <remarks>
/// <para>
/// This is what lets the expensive half of generation be cached. Binding needs a
/// <c>Compilation</c> for declared C# types, their constructors and their properties,
/// and a <c>Compilation</c> is a new object after every keystroke, so anything downstream
/// of it is recomputed for every character typed. Asking those questions first turns
/// that dependency into a small list of answers, which compares by value and hardly ever
/// changes. A C# method's role still follows from syntactic position, and the generated
/// C# compiler binds the emitted call — the one exception is a bare `@Name` operand,
/// which asks whether the host also has a value-returning overload (§7.1's third row),
/// since notation alone cannot say which of the two the author meant.
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
		var externals = new List<string>();
		var producers = new List<(string Method, string Against)>();

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
		// Under each import as well, because a type is written the way C# would write it
		// beside a `using` and the grammar half searches the same way (§7.3).
		foreach (var type in declared)
		{
			questions.Add(Question.Builds(type));
			questions.Add(Question.Sets(type));

			foreach (var import in imports)
			{
				questions.Add(Question.Builds(import + "." + type));
				questions.Add(Question.Sets(import + "." + type));
			}
		}

		foreach (var name in names)
		{
			questions.Add(name);

			// Asked unqualified first and then under each import, the way C# itself resolves.
			foreach (var import in imports)
				questions.Add(name with { Name = import + "." + name.Name });
		}

		// Not qualified under each import the way a type name is: a method is found by
		// Roslyn searching the compilation for its simple name (RoslynSymbolResolver.
		// TryResolveExternalValue), not by trying it beside each `using` in turn.
		foreach (var method in externals)
			questions.Add(Question.ValueOf(method));

		foreach (var (method, against) in producers)
			questions.Add(Question.ValueOf(method, against));

		// Sorted, because what this is conceptually is a set and what it travels as is an
		// array compared element by element. Two runs that asked the same questions must
		// produce the same array or the incremental stage rebuilds for nothing, and the
		// enumeration order of a hash set is not a promise anybody made. Stable within one
		// process today; a contract now.
		return
		[
			.. questions
				.OrderBy(static question => question.Name, StringComparer.Ordinal)
				.ThenBy(static question => question.Kind)
				.ThenBy(static question => question.Against ?? "", StringComparer.Ordinal),
		];

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

						// A rule's whole body being one bare @Name is §4.1 case 3 applied to a
						// value-returning external recognizer rather than to another rule — known
						// from syntax alone, both halves of it, unlike T itself.
						if (type is not null && body is Expr.Reference(true, var method, _))
							producers.Add((method, GrammarNormalizer.TypeName(type)));

						break;

					case Decl.Context(_, _, var nested, var inner):
						Collect(nested, inner);
						break;
				}
		}

		void Type(TypeRef? type)
		{
			if (type is null)
				return;

			names.Add(new Question(type.Name, Question.Exists));

			(type.IsSequence ? sequences : declared).Add(type.Name);

			// A sequence's element type is a type in its own right, and a rule declaring
			// `: T[]` may itself be an element of another sequence — as `T[]`, which is the
			// name the graph carries and therefore the name it asks about. Both spellings
			// go in: the element, because it is what a sequence collects, and the array,
			// because a rule whose value is one can be an operand of another sequence.
			if (type.IsSequence)
			{
				if (!declared.Contains(type.Name))
					declared.Add(type.Name);

				if (!declared.Contains(type.Name + "[]"))
					declared.Add(type.Name + "[]");
			}
		}

		void Walk(Expr expression)
		{
			switch (expression)
			{
				// C# values and methods are deliberately not questions for the generator.
				// Walk only the recognizing half of a construction; the compiler owns its
				// value and every guard.
				case Expr.Construct(var pattern, _):
					Walk(pattern);
					return;

				case Expr.Guard:
					return;

				case Expr.Reference(false, _, var typeArguments):
					foreach (var argument in typeArguments)
						Type(argument);

					break;

				// A bare @Name in operand position may be §7.1's third row — a value
				// overload — rather than its second, and only the host can tell. Never
				// reached for [@Name] (ElementSet has no children) or for a when/=> value
				// (Guard/Construct already returned above), so this is exactly the
				// external-recognizer-as-operand shape.
				case Expr.Reference(true, var method, _):
					externals.Add(method);
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
			answers.Add(question.Kind switch
			{
				Question.Assignability =>
					new Answer(question, resolver.IsAssignable(question.Name, question.Against!)),

				Question.Constructors => new Answer(
					question,
					resolver.TryResolveConstructors(question.Name, out var constructors),
					Shapes(constructors)),

				Question.Properties => new Answer(
					question,
					resolver.TryResolveSettableProperties(question.Name, out var properties),
					default,
					new EquatableArray<ObjectMember>([.. properties])),

				Question.Exists => new Answer(question, resolver.TypeExists(question.Name)),

				Question.ExternalValue =>
					resolver.TryResolveExternalValue(question.Name, question.Against, out var valueType) switch
					{
						ExternalValueResolution.Found     => new Answer(question, true, ExternalType: valueType),
						ExternalValueResolution.Ambiguous => new Answer(question, false, ExternalAmbiguous: true),
						_                                 => new Answer(question, false),
					},

				_ => throw new InvalidOperationException($"Unknown question kind {question.Kind}."),
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

	public bool TypeExists(string qualifiedName) => Look(new Question(qualifiedName, Question.Exists)).Yes;

	public bool IsAssignable(string from, string to) =>
		string.Equals(from, to, StringComparison.Ordinal) || Look(Question.Fits(from, to)).Yes;

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

	public ExternalValueResolution TryResolveExternalValue(string methodName, string? against, out string? valueType)
	{
		var answer = Look(Question.ValueOf(methodName, against));

		valueType = answer.ExternalType;

		return answer.Yes ? ExternalValueResolution.Found
			: answer.ExternalAmbiguous ? ExternalValueResolution.Ambiguous
			: ExternalValueResolution.NotFound;
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
				$"The question collector did not foresee the type question for '{question.Name}' " +
				$"(kind {question.Kind}). This is a defect in DotGram.Generation.Questions, not in the grammar.");
}
