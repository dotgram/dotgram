using System;

namespace DotGram.Sql.Ast;

public static partial class SqlWriter
{
	sealed partial class Text
	{
		/// <summary>
		/// One key as PostgreSQL's index_elem reads it: a column's name, a call by name or by a key word, or an expression in
		/// brackets — any other expression gets the brackets, and reads back inside them — then its collation, its operator
		/// class with that class's options, its direction and the place of its nulls.
		/// </summary>
		void PutIndexElement(IndexElement element)
		{
			switch (element.Key)
			{
				case Expression.Reference { Name.Count: 1 }:
				case Expression.Invocation { Filter: null, WithinGroup: null, Over: null }:
				case Expression.Parenthesized:
				case Expression.Cast or Expression.Extract or Expression.Treat or Expression.Trim or Expression.Position
					or Expression.Substring or Expression.Overlay or Expression.Normalize or Expression.CollationFor:
					PutExpression(element.Key);
					break;

				default:
					Open();
					PutExpression(element.Key);
					Close();
					break;
			}

			PutCollate(element.Collation);

			if (element.OperatorClass is { } operatorClass)
			{
				PutName(operatorClass);

				if (element.OperatorClassOptions.Count > 0)
				{
					Open();
					Each(element.OperatorClassOptions, PutDefinitionOption);
					Close();
				}
			}
			else if (element.OperatorClassOptions.Count > 0)
			{
				throw Misplaced(element, nameof(element.OperatorClassOptions), "without an operator class, whose options they are");
			}

			if (element.Direction is { } direction)
				Word(SortWord(direction));

			if (element.Nulls is { } nulls)
				Word(nulls switch { NullOrdering.First => "NULLS FIRST", NullOrdering.Last => "NULLS LAST", _ => throw NoText(nulls) });
		}

		/// <summary>
		/// One option of a <c>WITH (…)</c> as PostgreSQL's reloption_elem and def_elem read it: a name, and <c>= value</c> where
		/// one was written — a number, a string, a word, or the boolean literals.
		/// </summary>
		void PutDefinitionOption(Option option)
		{
			if (option.Options.Count > 0 || option.Words.Count > 0 || option.EqualsOmitted)
				throw Misplaced(option, nameof(option.Options), "of its own, further words or a left-out =, which a storage parameter does not read");

			PutIdentifier(option.Name);

			if (option.Value is not { } value)
				return;

			Word("=");

			switch (value)
			{
				case LiteralValue.Boolean or LiteralValue.String:
					PutExpression(value);
					break;

				case LiteralValue.Numeric or Expression.Unary { Operator: UnaryOperator.Minus or UnaryOperator.Plus, Operand: LiteralValue.Numeric }:
					PutExpression(value);
					break;

				case Expression.Reference { Name.Count: > 0 } reference:
					PutName(reference.Name);
					break;

				default:
					throw Misplaced(option, nameof(option.Value), "other than a number, a string, a word or a boolean, which are what a storage parameter is set to");
			}
		}

		/// <summary>PostgreSQL's <c>WITH (…)</c> of storage parameters, where any were written.</summary>
		void PutStorageParameters(SqlList<Option> options)
		{
			if (options.Count == 0)
				return;

			Word("WITH");
			Open();
			Each(options, PutDefinitionOption);
			Close();
		}
	}
}
