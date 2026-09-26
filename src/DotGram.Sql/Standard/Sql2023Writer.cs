using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DotGram.Sql.Ast;

/// <summary>The SQL:2023 tree back as SQL.</summary>
/// <remarks>
/// <para>
/// <b>What it is for.</b> The tree is lossless (docs/design/sql-ast.md): whatever a parser read it keeps,
/// and so whatever it keeps can be written again. A parser that answers yes and builds something else is
/// found by writing its tree, reading the text written, and holding the two trees to each other —
/// `benchmarks --standard "~production" file` for the standard's grammar, and `--roundtrip` for T-SQL's
/// once T-SQL builds this tree.
/// </para>
/// <para>
/// <b>It writes what the tree holds.</b> Brackets are written where the tree has them — a
/// <see cref="Expression.Parenthesized"/>, a query's <see cref="Statement.Select.Parentheses"/> — and
/// nowhere else: a tree read from text holds every bracket that changed its shape. Key words are written in
/// capitals, one space between tokens, none inside a name or a call; which of two spellings was written, the
/// tree says and this repeats.
/// </para>
/// </remarks>
public static partial class Sql2023Writer
{
	public static string Write(ISqlNode node)
	{
		var text = new Text();

		text.PutNode(node);

		return text.ToString();
	}

	sealed partial class Text
	{
		readonly StringBuilder written = new();

		bool held = true;

		public override string ToString()
		{
			return written.ToString();
		}

		public void PutNode(ISqlNode node)
		{
			switch (node)
			{
				case Statement statement:   PutStatement(statement); break;
				case Expression expression: PutExpression(expression); break;
				case TableSource source:    PutSource(source); break;
				case DataType type:         PutType(type); break;
				case LiteralValue literal:  PutLiteral(literal); break;
				case QualifiedName name:    PutName(name); break;
				case Identifier identifier: PutIdentifier(identifier); break;
				case SelectItem item:       PutItem(item); break;
				default:                    throw Unwritten(node);
			}
		}

		static NotSupportedException Unwritten(object node)
		{
			return new($"{node.GetType().Name} is no node the standard's grammar builds, and is not written.");
		}

		// ── Tokens ─────────────────────────────────────────────────────────────────

		/// <summary>A token, a space before it unless the last token asked for none.</summary>
		void Word(string token)
		{
			if (!held)
				written.Append(' ');

			written.Append(token);
			held = false;
		}

		/// <summary>A token against what stands before it.</summary>
		void Tight(string token)
		{
			written.Append(token);
			held = false;
		}

		/// <summary>No space before the next token.</summary>
		void Hold()
		{
			held = true;
		}

		/// <summary>A call's bracket, against the name before it: `f(`.</summary>
		void Call()
		{
			Tight("(");
			Hold();
		}

		/// <summary>A bracket with a space before it: `IN (`.</summary>
		void Open()
		{
			Word("(");
			Hold();
		}

		void Close()
		{
			Tight(")");
		}

		void Each<T>(IReadOnlyList<T> items, Action<T> put)
		{
			for (var at = 0; at < items.Count; at++)
			{
				if (at > 0)
					Tight(",");

				put(items[at]);
			}
		}

		void Number(long value)
		{
			Word(value.ToString(CultureInfo.InvariantCulture));
		}

		// ── §5.3 Literals, §5.4 Names ──────────────────────────────────────────────

		void PutIdentifier(Identifier identifier)
		{
			Word(identifier.Text);

			if (identifier.UnicodeEscape is { } escape)
				Tight("UESCAPE'" + escape + "'");
		}

		void PutName(QualifiedName name)
		{
			for (var at = 0; at < name.Parts.Count; at++)
			{
				if (at > 0)
				{
					Tight(".");
					Hold();
				}

				PutIdentifier(name.Parts[at]);
			}
		}

		static string Dotted(QualifiedName name)
		{
			var text = new StringBuilder();

			for (var at = 0; at < name.Parts.Count; at++)
			{
				if (at > 0)
					text.Append('.');

				text.Append(name.Parts[at].Text);
			}

			return text.ToString();
		}

		void PutCharacterSet(CharacterSetName name)
		{
			PutName(name.Name);
		}

		void PutCollation(CollationName name)
		{
			PutName(name.Name);
		}

		void PutLiteral(LiteralValue literal)
		{
			switch (literal)
			{
				case LiteralValue.Numeric numeric:
					Word(numeric.Text);
					break;

				// The introducer and the letters before the quote are the literal's, and no space may stand in it.
				case LiteralValue.String text:
					var introducer = text.CharacterSet is { } set ? "_" + Dotted(set.Name) : "";

					Word(text.Kind switch
					{
						StringLiteralKind.National => "N" + text.Text,
						StringLiteralKind.Unicode  => introducer + "U&" + text.Text + (text.UnicodeEscape is { } escape ? "UESCAPE'" + escape + "'" : ""),
						_                          => introducer + text.Text,
					});
					break;

				case LiteralValue.Binary binary:
					Word("X" + binary.Text);
					break;

				case LiteralValue.Boolean truth:
					Word(Truth(truth.Value));
					break;

				case LiteralValue.DateTime datetime:
					Word(datetime.Kind switch { DateTimeLiteralKind.Date => "DATE", DateTimeLiteralKind.Time => "TIME", DateTimeLiteralKind.Timestamp => "TIMESTAMP", _ => throw NoText(datetime.Kind) });
					Word(datetime.Text);
					break;

				case LiteralValue.Interval interval:
					Word("INTERVAL");

					if (interval.Sign is { } sign)
					{
						Word(sign == UnaryOperator.Minus ? "-" : "+");
						Hold();
					}

					Word(interval.Text);
					PutQualifier(interval.Qualifier);
					break;

				case LiteralValue.Null:
					Word("NULL");
					break;

				default:
					throw Unwritten(literal);
			}
		}

		static string Truth(BooleanLiteral value)
		{
			return value switch { BooleanLiteral.True => "TRUE", BooleanLiteral.False => "FALSE", BooleanLiteral.Unknown => "UNKNOWN", _ => throw NoText(value) };
		}

		// ── §10.1 Interval qualifier ───────────────────────────────────────────────

		void PutQualifier(IntervalQualifier qualifier)
		{
			Word(Field(qualifier.Start));

			// `SECOND (p, f)` alone holds both of its precisions.
			if (qualifier.Start == DateTimeField.Second && qualifier.End is null)
			{
				if (qualifier.LeadingPrecision is { } seconds)
				{
					Call();
					Number(seconds);

					if (qualifier.FractionalPrecision is { } fraction)
					{
						Tight(",");
						Number(fraction);
					}

					Close();
				}

				return;
			}

			if (qualifier.LeadingPrecision is { } leading)
			{
				Call();
				Number(leading);
				Close();
			}

			if (qualifier.End is { } end)
			{
				Word("TO");
				Word(Field(end));

				if (end == DateTimeField.Second && qualifier.FractionalPrecision is { } fraction)
				{
					Call();
					Number(fraction);
					Close();
				}
			}
		}

		static string Field(DateTimeField field)
		{
			return field switch
			{
				DateTimeField.Year => "YEAR",
				DateTimeField.Month => "MONTH",
				DateTimeField.Day => "DAY",
				DateTimeField.Hour => "HOUR",
				DateTimeField.Minute => "MINUTE",
				DateTimeField.Second => "SECOND",
				_ => throw NoText(field),
			};
		}

		// ── §6.1 Data types ────────────────────────────────────────────────────────

		void PutType(DataType type)
		{
			switch (type)
			{
				case DataType.Character character:
					Word(character.Kind switch
					{
						CharacterTypeKind.Character                    => "CHARACTER",
						CharacterTypeKind.Char                         => "CHAR",
						CharacterTypeKind.CharacterVarying             => "CHARACTER VARYING",
						CharacterTypeKind.CharVarying                  => "CHAR VARYING",
						CharacterTypeKind.Varchar                      => "VARCHAR",
						CharacterTypeKind.CharacterLargeObject         => "CHARACTER LARGE OBJECT",
						CharacterTypeKind.CharLargeObject              => "CHAR LARGE OBJECT",
						CharacterTypeKind.Clob                         => "CLOB",
						CharacterTypeKind.NationalCharacter            => "NATIONAL CHARACTER",
						CharacterTypeKind.NationalChar                 => "NATIONAL CHAR",
						CharacterTypeKind.Nchar                        => "NCHAR",
						CharacterTypeKind.NationalCharacterVarying     => "NATIONAL CHARACTER VARYING",
						CharacterTypeKind.NationalCharVarying          => "NATIONAL CHAR VARYING",
						CharacterTypeKind.NcharVarying                 => "NCHAR VARYING",
						CharacterTypeKind.NationalCharacterLargeObject => "NATIONAL CHARACTER LARGE OBJECT",
						CharacterTypeKind.NcharLargeObject             => "NCHAR LARGE OBJECT",
						CharacterTypeKind.Nclob                        => "NCLOB",
						_                                              => throw NoText(character.Kind),
					});

					if (character.Length is not null || character.LargeObject is not null || character.Max)
					{
						Call();

						if (character.Max)
							Word("MAX");
						else if (character.LargeObject is { } size)
							PutSize(size);
						else
							Number(character.Length!.Value);

						if (character.Unit is { } unit)
							Word(unit == LengthUnit.Characters ? "CHARACTERS" : "OCTETS");

						Close();
					}

					if (character.CharacterSet is { } set)
					{
						Word("CHARACTER SET");
						PutCharacterSet(set);
					}

					if (character.Collation is { } collation)
					{
						Word("COLLATE");
						PutCollation(collation);
					}

					break;

				case DataType.Binary binary:
					Word(binary.Kind switch
					{
						BinaryTypeKind.Binary            => "BINARY",
						BinaryTypeKind.BinaryVarying     => "BINARY VARYING",
						BinaryTypeKind.Varbinary         => "VARBINARY",
						BinaryTypeKind.BinaryLargeObject => "BINARY LARGE OBJECT",
						BinaryTypeKind.Blob              => "BLOB",
						_                                => throw NoText(binary.Kind),
					});

					if (binary.Max)
					{
						Call();
						Word("MAX");
						Close();
					}
					else if (binary.LargeObject is { } large)
					{
						Call();
						PutSize(large);
						Close();
					}
					else if (binary.Length is { } length)
					{
						Call();
						Number(length);
						Close();
					}

					break;

				case DataType.Numeric numeric:
					Word(numeric.Kind switch
					{
						NumericTypeKind.Numeric         => "NUMERIC",
						NumericTypeKind.Decimal         => "DECIMAL",
						NumericTypeKind.Dec             => "DEC",
						NumericTypeKind.SmallInt        => "SMALLINT",
						NumericTypeKind.Integer         => "INTEGER",
						NumericTypeKind.Int             => "INT",
						NumericTypeKind.BigInt          => "BIGINT",
						NumericTypeKind.Float           => "FLOAT",
						NumericTypeKind.Real            => "REAL",
						NumericTypeKind.DoublePrecision => "DOUBLE PRECISION",
						NumericTypeKind.DecFloat        => "DECFLOAT",
						_                               => throw NoText(numeric.Kind),
					});

					if (numeric.Precision is { } precision)
					{
						Call();
						Number(precision);

						if (numeric.Scale is { } scale)
						{
							Tight(",");
							Number(scale);
						}

						Close();
					}

					break;

				case DataType.Boolean:
					Word("BOOLEAN");
					break;

				case DataType.DateTime datetime:
					Word(datetime.Kind switch { DateTimeTypeKind.Date => "DATE", DateTimeTypeKind.Time => "TIME", DateTimeTypeKind.Timestamp => "TIMESTAMP", _ => throw NoText(datetime.Kind) });

					if (datetime.Precision is { } fraction)
					{
						Call();
						Number(fraction);
						Close();
					}

					if (datetime.TimeZone is { } zone)
						Word(zone == TimeZoneMode.With ? "WITH TIME ZONE" : "WITHOUT TIME ZONE");

					break;

				case DataType.Interval interval:
					Word("INTERVAL");
					PutQualifier(interval.Qualifier);
					break;

				case DataType.Row row:
					Word("ROW");
					Call();
					Each(row.Fields, field =>
					{
						PutIdentifier(field.Name);
						PutType(field.Type);
					});
					Close();
					break;

				case DataType.Reference reference:
					Word("REF");
					Call();
					PutName(reference.ReferencedType);
					Close();

					if (reference.Scope is { } scope)
					{
						Word("SCOPE");
						PutName(scope);
					}

					break;

				case DataType.Array array:
					PutType(array.ElementType);
					Word("ARRAY");

					if (array.MaximumCardinality is { } cardinality)
					{
						Tight(array.Trigraphs ? "??(" : "[");
						Hold();
						Number(cardinality);
						Tight(array.Trigraphs ? "??)" : "]");
					}

					break;

				case DataType.Multiset multiset:
					PutType(multiset.ElementType);
					Word("MULTISET");
					break;

				case DataType.UserDefined user:
					PutName(user.Name);
					break;

				case DataType.Json:
					Word("JSON");
					break;

				case DataType.Descriptor:
					Word("DESCRIPTOR");
					break;

				case DataType.GenericTable table:
					Word("TABLE");

					if (table.PassThrough is { } through)
						Word(through == PassThroughMode.PassThrough ? "PASS THROUGH" : "NO PASS THROUGH");

					if (table.Semantics is { } semantics)
						Word(semantics == TableSemantics.Row ? "WITH ROW SEMANTICS" : "WITH SET SEMANTICS");

					if (table.Pruning is { } pruning)
						Word(pruning == TablePruning.PruneOnEmpty ? "PRUNE ON EMPTY" : "KEEP ON EMPTY");

					break;

				default:
					throw Unwritten(type);
			}
		}

		/// <summary>A large object's length: `2K` is one token, and its multiplier is written against the number.</summary>
		void PutSize(LargeObjectSize size)
		{
			Number(size.Value);

			if (size.Multiplier is { } multiplier)
				Tight(multiplier.ToString());
		}

		// ── §6.28 Value expressions, §8 Predicates ─────────────────────────────────

		void PutExpression(Expression expression)
		{
			switch (expression)
			{
				case Expression.Literal literal:
					PutLiteral(literal.Value);
					break;

				case Expression.Reference reference:
					PutName(reference.Name);
					break;

				case Expression.Parameter parameter:
					PutParameter(parameter);
					break;

				case Expression.Current current:
					PutCurrent(current);
					break;

				case Expression.Unary unary when unary.Operator == UnaryOperator.Not:
					Word("NOT");
					PutExpression(unary.Operand);
					break;

				// A sign is written against its operand, so that `- -` never becomes a comment's `--`.
				// The operator is switched on rather than compared with one value: `Word(op == Minus ? "-"
				// : "+")` wrote T-SQL's `~` as `+`, which is the catch-all defect wearing a ternary.
				case Expression.Unary unary:
					Word(unary.Operator switch
					{
						UnaryOperator.Minus      => "-",
						UnaryOperator.Plus       => "+",
						UnaryOperator.BitwiseNot => "~",
						_                        => throw NoText(unary.Operator),
					});
					Hold();
					PutExpression(unary.Operand);
					break;

				case Expression.Binary binary:
					PutExpression(binary.Left);
					Word(binary.Operator switch
					{
						BinaryOperator.Add         => "+",
						BinaryOperator.Subtract    => "-",
						BinaryOperator.Multiply    => "*",
						BinaryOperator.Divide      => "/",
						BinaryOperator.Concatenate => "||",
						BinaryOperator.And         => "AND",
						BinaryOperator.Or          => "OR",
						BinaryOperator.Modulo      => "%",
						BinaryOperator.BitwiseAnd  => "&",
						BinaryOperator.BitwiseOr   => "|",
						BinaryOperator.BitwiseXor  => "^",
						BinaryOperator.ShiftLeft   => "<<",
						BinaryOperator.ShiftRight  => ">>",
						_                          => throw NoText(binary.Operator),
					});
					PutExpression(binary.Right);
					break;

				case Expression.MultisetOperation multiset:
					PutExpression(multiset.Left);
					Word("MULTISET");
					Word(multiset.Operator switch { MultisetOperator.Union => "UNION", MultisetOperator.Except => "EXCEPT", MultisetOperator.Intersect => "INTERSECT", _ => throw NoText(multiset.Operator) });
					PutQuantifier(multiset.Quantifier);
					PutExpression(multiset.Right);
					break;

				case Expression.Row row:
					if (row.RowKeyword)
					{
						Word("ROW");
						Call();
					}
					else
						Open();

					Each(row.Items, PutExpression);
					Close();
					break;

				case Expression.Parenthesized parenthesized:
					Open();
					PutExpression(parenthesized.Value);
					Close();
					break;

				case Expression.Subquery subquery:
					PutSubquery(subquery.Query);
					break;

				case Expression.Case @case:
					Word("CASE");

					if (@case.Operand is { } operand)
						PutExpression(operand);

					foreach (var when in @case.Whens)
					{
						Word("WHEN");
						Each(when.When, PutExpression);
						Word("THEN");
						PutExpression(when.Then);
					}

					if (@case.Else is { } otherwise)
					{
						Word("ELSE");
						PutExpression(otherwise);
					}

					Word("END");
					break;

				// Where a simple case's operand is left out of a predicate, nothing is written.
				case Expression.CaseOperand:
					break;

				case Expression.Cast cast:
					Word("CAST");
					Call();
					PutExpression(cast.Value);
					Word("AS");
					PutType(cast.Target);

					if (cast.Format is { } format)
					{
						Word("FORMAT");
						Word(format);
					}

					Close();
					break;

				case Expression.Invocation invocation:
					PutInvocation(invocation);
					break;

				case Expression.Asterisk:
					Word("*");
					break;

				case Expression.Member member:
					PutMember(member);
					break;

				case Expression.Generalized generalized:
					Open();
					PutExpression(generalized.Value);
					Word("AS");
					PutType(generalized.Type);
					Close();
					break;

				case Expression.Treat treat:
					Word("TREAT");
					Call();
					PutExpression(treat.Value);
					Word("AS");
					PutType(treat.Target);
					Close();
					break;

				case Expression.New @new:
					Word("NEW");
					PutName(@new.TypeName);
					PutArguments(@new.Arguments);
					break;

				case Expression.Dereference dereference:
					Word("DEREF");
					Call();
					PutExpression(dereference.Value);
					Close();
					break;

				case Expression.Array array:
					Word("ARRAY");
					PutEnumeration(array.Items, array.Trigraphs);
					break;

				case Expression.Multiset multiset:
					Word("MULTISET");
					PutEnumeration(multiset.Items, multiset.Trigraphs);
					break;

				case Expression.CollectionQuery collection:
					Word(collection.Kind switch { CollectionKind.Array => "ARRAY", CollectionKind.Multiset => "MULTISET", CollectionKind.Table => "TABLE", _ => throw NoText(collection.Kind) });
					PutSubquery(collection.Query);
					break;

				case Expression.Element element:
					PutExpression(element.Collection);
					Tight(element.Trigraphs ? "??(" : "[");
					Hold();
					PutExpression(element.Index);

					if (element.To is { } to)
					{
						Word("TO");
						PutExpression(to);
					}

					Tight(element.Trigraphs ? "??)" : "]");
					break;

				case Expression.Wildcard wildcard:
					PutExpression(wildcard.Target);
					Tight(wildcard.Kind == WildcardKind.Member ? ".*" : "[*]");
					break;

				case Expression.JsonAccessor accessor:
					PutExpression(accessor.Target);
					PutPathAccessor(accessor.Accessor);
					break;

				case Expression.NextValue next:
					Word("NEXT VALUE FOR");
					PutName(next.Sequence);
					break;

				case Expression.Collate collate:
					PutExpression(collate.Value);
					Word("COLLATE");
					PutCollation(collate.Collation);
					break;

				case Expression.AtTimeZone zone:
					PutExpression(zone.Value);

					if (zone.Zone is { } at)
					{
						Word("AT TIME ZONE");
						PutExpression(at);
					}
					else
						Word("AT LOCAL");

					break;

				case Expression.IntervalQualified qualified:
					PutExpression(qualified.Value);
					PutQualifier(qualified.Qualifier);
					break;

				case Expression.Substring substring:
					Word("SUBSTRING");
					Call();
					PutExpression(substring.Value);
					Word("FROM");
					PutExpression(substring.From);

					if (substring.For is { } forLength)
					{
						Word("FOR");
						PutExpression(forLength);
					}

					PutUnits(substring.Using);
					Close();
					break;

				case Expression.SubstringSimilar similar:
					Word("SUBSTRING");
					Call();
					PutExpression(similar.Value);
					Word("SIMILAR");
					PutExpression(similar.Pattern);
					Word("ESCAPE");
					PutExpression(similar.Escape);
					Close();
					break;

				case Expression.Trim trim:
					Word("TRIM");
					Call();

					if (trim.Specification is { } specification)
						Word(specification switch { TrimSpecification.Leading => "LEADING", TrimSpecification.Trailing => "TRAILING", TrimSpecification.Both => "BOTH", _ => throw NoText(specification) });

					if (trim.Character is { } character)
						PutExpression(character);

					if (trim.FromKeyword)
						Word("FROM");

					PutExpression(trim.Source);
					Close();
					break;

				case Expression.Overlay overlay:
					Word("OVERLAY");
					Call();
					PutExpression(overlay.Value);
					Word("PLACING");
					PutExpression(overlay.Placing);
					Word("FROM");
					PutExpression(overlay.From);

					if (overlay.For is { } span)
					{
						Word("FOR");
						PutExpression(span);
					}

					PutUnits(overlay.Using);
					Close();
					break;

				case Expression.Position position:
					Word("POSITION");
					Call();
					PutExpression(position.Value);
					Word("IN");
					PutExpression(position.Within);
					PutUnits(position.Using);
					Close();
					break;

				case Expression.Length length:
					Word(length.Function switch { LengthFunction.CharLength => "CHAR_LENGTH", LengthFunction.CharacterLength => "CHARACTER_LENGTH", LengthFunction.OctetLength => "OCTET_LENGTH", _ => throw NoText(length.Function) });
					Call();
					PutExpression(length.Value);
					PutUnits(length.Using);
					Close();
					break;

				case Expression.Extract extract:
					Word("EXTRACT");
					Call();
					Word(extract.Field switch
					{
						ExtractField.Year           => "YEAR",
						ExtractField.Month          => "MONTH",
						ExtractField.Day            => "DAY",
						ExtractField.Hour           => "HOUR",
						ExtractField.Minute         => "MINUTE",
						ExtractField.Second         => "SECOND",
						ExtractField.TimezoneHour   => "TIMEZONE_HOUR",
						ExtractField.TimezoneMinute                           => "TIMEZONE_MINUTE",
						_                           => throw NoText(extract.Field),
					});
					Word("FROM");
					PutExpression(extract.Source);
					Close();
					break;

				case Expression.Normalize normalize:
					Word("NORMALIZE");
					Call();
					PutExpression(normalize.Value);

					if (normalize.Form is { } form)
					{
						Tight(",");
						Word(form.ToString());

						if (normalize.MaxLength is { } maximum)
						{
							Tight(",");
							PutExpression(maximum);

							if (normalize.MaxLengthMultiplier is { } multiplier)
								Tight(multiplier.ToString());

							if (normalize.MaxLengthUnit is { } unit)
								Word(unit == LengthUnit.Characters ? "CHARACTERS" : "OCTETS");
						}
					}

					Close();
					break;

				case Expression.TranslateUsing translate:
					Word(translate.Function == TranslateFunction.Convert ? "CONVERT" : "TRANSLATE");
					Call();
					PutExpression(translate.Value);
					Word("USING");
					PutName(translate.Name);
					Close();
					break;

				case Expression.Regex regex:
					PutRegex(regex);
					break;

				case Expression.Comparison comparison:
					PutExpression(comparison.Left);
					Word(comparison.Exclamation ? "!=" : Comparison(comparison.Operator));
					PutExpression(comparison.Right);
					break;

				case Expression.Between between:
					PutExpression(between.Value);
					PutNot(between.Not);
					Word("BETWEEN");

					if (between.Symmetry is { } symmetry)
						Word(symmetry == BetweenSymmetry.Asymmetric ? "ASYMMETRIC" : "SYMMETRIC");

					PutExpression(between.Lower);
					Word("AND");
					PutExpression(between.Upper);
					break;

				case Expression.In @in:
					PutExpression(@in.Value);
					PutNot(@in.Not);
					Word("IN");

					if (@in.SourceValue is InSource.Query inQuery)
						PutSubquery(inQuery.Value);
					else
					{
						Open();
						Each(((InSource.Values)@in.SourceValue).Items, PutExpression);
						Close();
					}

					break;

				case Expression.Like like:
					PutExpression(like.Value);
					PutNot(like.Not);
					Word(like.Kind switch { LikeKind.Like => "LIKE", LikeKind.Similar => "SIMILAR TO", LikeKind.Regex => "LIKE_REGEX", _ => throw NoText(like.Kind) });
					PutExpression(like.Pattern);

					if (like.Escape is { } escape)
					{
						Word("ESCAPE");
						PutExpression(escape);
					}

					if (like.Flag is { } flag)
					{
						Word("FLAG");
						PutExpression(flag);
					}

					break;

				case Expression.IsNull isNull:
					PutExpression(isNull.Value);
					Word("IS");
					PutNot(isNull.Not);
					Word("NULL");
					break;

				case Expression.IsTruth isTruth:
					PutExpression(isTruth.Value);
					Word("IS");
					PutNot(isTruth.Not);
					Word(Truth(isTruth.Truth));
					break;

				case Expression.QuantifiedComparison quantified:
					PutExpression(quantified.Left);
					Word(Comparison(quantified.Operator));
					Word(quantified.Quantifier switch { Quantifier.All => "ALL", Quantifier.Some => "SOME", Quantifier.Any => "ANY", _ => throw NoText(quantified.Quantifier) });
					PutSubquery(quantified.Query);
					break;

				case Expression.Exists exists:
					Word("EXISTS");
					PutSubquery(exists.Query);
					break;

				case Expression.Unique unique:
					Word("UNIQUE");

					if (unique.Nulls is { } nulls)
						Word(nulls == NullDistinctness.Distinct ? "NULLS DISTINCT" : "NULLS NOT DISTINCT");

					PutSubquery(unique.Query);
					break;

				case Expression.Match match:
					PutExpression(match.Value);
					Word("MATCH");

					if (match.UniqueKeyword)
						Word("UNIQUE");

					if (match.Type is { } type)
						Word(type switch { MatchType.Simple => "SIMPLE", MatchType.Partial => "PARTIAL", MatchType.Full => "FULL", _ => throw NoText(type) });

					PutSubquery(match.Query);
					break;

				case Expression.Overlaps overlaps:
					PutExpression(overlaps.Left);
					Word("OVERLAPS");
					PutExpression(overlaps.Right);
					break;

				case Expression.IsDistinct distinct:
					PutExpression(distinct.Left);
					Word("IS");
					PutNot(distinct.Not);
					Word("DISTINCT FROM");
					PutExpression(distinct.Right);
					break;

				case Expression.IsNormalized normalized:
					PutExpression(normalized.Value);
					Word("IS");
					PutNot(normalized.Not);

					if (normalized.Form is { } normalForm)
						Word(normalForm.ToString());

					Word("NORMALIZED");
					break;

				case Expression.MemberOf memberOf:
					PutExpression(memberOf.Value);
					PutNot(memberOf.Not);
					Word(memberOf.OfKeyword ? "MEMBER OF" : "MEMBER");
					PutExpression(memberOf.Collection);
					break;

				case Expression.SubmultisetOf submultiset:
					PutExpression(submultiset.Value);
					PutNot(submultiset.Not);
					Word(submultiset.OfKeyword ? "SUBMULTISET OF" : "SUBMULTISET");
					PutExpression(submultiset.Collection);
					break;

				case Expression.IsSet isSet:
					PutExpression(isSet.Value);
					Word("IS");
					PutNot(isSet.Not);
					Word("A SET");
					break;

				case Expression.IsOf isOf:
					PutExpression(isOf.Value);
					Word("IS");
					PutNot(isOf.Not);
					Word("OF");
					Open();
					Each(isOf.Types, test =>
					{
						if (test.Only)
							Word("ONLY");

						PutName(test.TypeName);
					});
					Close();
					break;

				case Expression.PeriodPredicate period:
					PutPeriod(period.Left);
					Word(period.Operator switch
					{
						PeriodOperator.Overlaps            => "OVERLAPS",
						PeriodOperator.Equals              => "EQUALS",
						PeriodOperator.Contains            => "CONTAINS",
						PeriodOperator.Precedes            => "PRECEDES",
						PeriodOperator.Succeeds            => "SUCCEEDS",
						PeriodOperator.ImmediatelyPrecedes => "IMMEDIATELY PRECEDES",
						PeriodOperator.ImmediatelySucceeds                                  => "IMMEDIATELY SUCCEEDS",
						_                                  => throw NoText(period.Operator),
					});

					if (period.Right is PeriodRight.Period right)
						PutPeriod(right.Value);
					else
						PutExpression(((PeriodRight.Point)period.Right).Value);

					break;

				case Expression.JsonPredicate json:
					PutExpression(json.Value);

					if (json.Input is { } input)
					{
						PutInput(input);
						Word("IS");
					}
					else
						Word("IS");

					PutNot(json.Not);
					Word("JSON");

					if (json.Type is { } predicateType)
						Word(predicateType switch { JsonPredicateType.Value => "VALUE", JsonPredicateType.Array => "ARRAY", JsonPredicateType.Object => "OBJECT", JsonPredicateType.Scalar => "SCALAR", _ => throw NoText(predicateType) });

					PutUniqueness(json.Uniqueness);
					break;

				case Expression.JsonExists exists:
					Word("JSON_EXISTS");
					Call();
					PutCommon(exists.Common);

					if (exists.OnError is { } onError)
					{
						Word(onError switch { JsonExistsErrorBehavior.True => "TRUE", JsonExistsErrorBehavior.False => "FALSE", JsonExistsErrorBehavior.Unknown => "UNKNOWN", JsonExistsErrorBehavior.Error => "ERROR", _ => throw NoText(onError) });
						Word("ON ERROR");
					}

					Close();
					break;

				case Expression.JsonValue value:
					Word("JSON_VALUE");
					Call();
					PutCommon(value.Common);

					if (value.Returning is { } returning)
					{
						Word("RETURNING");
						PutType(returning);
					}

					if (value.OnEmpty is { } valueOnEmpty)
					{
						PutValueBehavior(valueOnEmpty);
						Word("ON EMPTY");
					}

					if (value.OnError is { } valueOnError)
					{
						PutValueBehavior(valueOnError);
						Word("ON ERROR");
					}

					Close();
					break;

				case Expression.JsonQuery query:
					Word("JSON_QUERY");
					Call();
					PutCommon(query.Common);
					PutOutput(query.Output);

					if (query.Wrapper is { } wrapper)
					{
						Word(Wrapper(wrapper));
						Word("WRAPPER");
					}

					PutQuotes(query.Quotes);

					if (query.OnEmpty is { } queryOnEmpty)
					{
						Word(QueryBehavior(queryOnEmpty));
						Word("ON EMPTY");
					}

					if (query.OnError is { } queryOnError)
					{
						Word(QueryBehavior(queryOnError));
						Word("ON ERROR");
					}

					Close();
					break;

				case Expression.JsonObject jsonObject:
					Word("JSON_OBJECT");
					Call();
					Each(jsonObject.Members, PutMember);
					PutNullHandling(jsonObject.Nulls);
					PutUniqueness(jsonObject.Uniqueness);
					PutOutput(jsonObject.Output);
					Close();
					break;

				case Expression.JsonArray jsonArray:
					Word("JSON_ARRAY");
					Call();
					Each(jsonArray.Elements, PutElement);
					PutNullHandling(jsonArray.Nulls);
					PutOutput(jsonArray.Output);
					Close();
					break;

				case Expression.JsonArrayQuery arrayQuery:
					Word("JSON_ARRAY");
					Call();
					PutQuery(arrayQuery.Query);

					if (arrayQuery.Format is { } queryFormat)
						PutInput(queryFormat);

					PutNullHandling(arrayQuery.Nulls);
					PutOutput(arrayQuery.Output);
					Close();
					break;

				case Expression.JsonObjectAggregate objectAggregate:
					PutSemantics(objectAggregate.Semantics);
					Word("JSON_OBJECTAGG");
					Call();
					PutMember(objectAggregate.Pair);
					PutNullHandling(objectAggregate.Nulls);
					PutUniqueness(objectAggregate.Uniqueness);
					PutOutput(objectAggregate.Output);
					Close();
					PutFilterAndOver(objectAggregate.Filter, objectAggregate.Over);
					break;

				case Expression.JsonArrayAggregate arrayAggregate:
					PutSemantics(arrayAggregate.Semantics);
					Word("JSON_ARRAYAGG");
					Call();
					PutElement(arrayAggregate.Item);

					if (arrayAggregate.OrderBy is { } order)
					{
						Word("ORDER BY");
						PutSorts(order);
					}

					PutNullHandling(arrayAggregate.Nulls);
					PutOutput(arrayAggregate.Output);
					Close();
					PutFilterAndOver(arrayAggregate.Filter, arrayAggregate.Over);
					break;

				case Expression.JsonParse parse:
					Word("JSON");
					Call();
					PutExpression(parse.Value);

					if (parse.Input is { } parseInput)
						PutInput(parseInput);

					PutUniqueness(parse.Uniqueness);
					Close();
					break;

				case Expression.JsonScalar scalar:
					Word("JSON_SCALAR");
					Call();
					PutExpression(scalar.Value);
					Close();
					break;

				case Expression.JsonSerialize serialize:
					Word("JSON_SERIALIZE");
					Call();
					PutExpression(serialize.Value);
					PutOutput(serialize.Output);
					Close();
					break;

				case Expression.DescriptorConstructor descriptor:
					Word("DESCRIPTOR");
					Call();
					Each(descriptor.Columns, column =>
					{
						PutIdentifier(column.Name);

						if (column.Type is { } columnType)
							PutType(columnType);
					});
					Close();
					break;

				case Expression.CollationFor collationFor:
					Word("COLLATION FOR");
					Open();
					PutExpression(collationFor.Value);
					Close();
					break;

				case Expression.Default:
					Word("DEFAULT");
					break;

				case Expression.RowMarker marker:
					Word(marker.Kind switch
					{
						RowMarkerKind.BeginPartition => "BEGIN_PARTITION",
						RowMarkerKind.BeginFrame     => "BEGIN_FRAME",
						RowMarkerKind.CurrentRow     => "CURRENT_ROW",
						RowMarkerKind.FrameRow       => "FRAME_ROW",
						RowMarkerKind.EndFrame       => "END_FRAME",
						RowMarkerKind.EndPartition                            => "END_PARTITION",
						_                            => throw NoText(marker.Kind),
					});

					if (marker.DeltaSign is { } deltaSign)
					{
						Word(deltaSign == UnaryOperator.Minus ? "-" : "+");
						PutExpression(marker.Delta!);
					}

					break;

				case Expression.ValueOf valueOf:
					Word("VALUE_OF");
					Call();
					PutExpression(valueOf.Value);
					Word("AT");
					PutExpression(valueOf.At);

					if (valueOf.Otherwise is { } otherwiseValue)
					{
						Tight(",");
						PutExpression(otherwiseValue);
					}

					Close();
					break;

				default:
					throw Unwritten(expression);
			}
		}

		void PutNot(bool not)
		{
			if (not)
				Word("NOT");
		}

		void PutQuantifier(SetQuantifier? quantifier)
		{
			if (quantifier is { } one)
				Word(one == SetQuantifier.All ? "ALL" : "DISTINCT");
		}

		void PutUnits(CharacterLengthUnits? units)
		{
			if (units is { } one)
				Word(one == CharacterLengthUnits.Characters ? "USING CHARACTERS" : "USING OCTETS");
		}

		// A value with no arm is a fact the tree holds and the text would lose, so it is refused rather
		// than answered. The switches used to end in a catch-all returning a literal, and a value added
		// to an enum printed as the last arm: `ComparisonOperator.NotLess` wrote `>=`.
		static Exception NoText<T>(T value) where T : struct, Enum
		{
			return new ArgumentOutOfRangeException(
				nameof(value), value, "The writer has no text for " + typeof(T).Name + "." + value + ".");
		}

		static string Comparison(ComparisonOperator op)
		{
			return op switch
			{
				ComparisonOperator.Equal => "=",
				ComparisonOperator.NotEqual => "<>",
				ComparisonOperator.Less => "<",
				ComparisonOperator.Greater => ">",
				ComparisonOperator.LessOrEqual => "<=",
				ComparisonOperator.GreaterOrEqual => ">=",
				ComparisonOperator.NotLess => "!<",
				ComparisonOperator.NotGreater => "!>",
				_ => throw NoText(op),
			};
		}

		void PutPeriod(PeriodValue period)
		{
			if (period is PeriodValue.Reference reference)
			{
				PutName(reference.Name);
				return;
			}

			var range = (PeriodValue.Range)period;

			Word("PERIOD");
			Open();
			PutExpression(range.Start);
			Tight(",");
			PutExpression(range.End);
			Close();
		}

		void PutParameter(Expression.Parameter parameter)
		{
			if (parameter.Kind == ParameterKind.Dynamic)
			{
				Word("?");
				return;
			}

			Word(":");
			Hold();
			PutIdentifier(parameter.Name!);

			if (parameter.Indicator is { } indicator)
			{
				if (parameter.IndicatorKeyword)
					Word("INDICATOR");

				Word(":");
				Hold();
				PutIdentifier(indicator);
			}
		}

		void PutCurrent(Expression.Current current)
		{
			switch (current.Kind)
			{
				case CurrentValue.TransformGroupForType:
					Word("CURRENT_TRANSFORM_GROUP_FOR_TYPE");
					PutName(current.TypeName!);
					return;

				case CurrentValue.Time or CurrentValue.Timestamp or CurrentValue.LocalTime or CurrentValue.LocalTimestamp:
					Word(current.Kind switch
					{
						CurrentValue.Time      => "CURRENT_TIME",
						CurrentValue.Timestamp => "CURRENT_TIMESTAMP",
						CurrentValue.LocalTime => "LOCALTIME",
						CurrentValue.LocalTimestamp => "LOCALTIMESTAMP",
						_                      => throw NoText(current.Kind),
					});

					if (current.Precision is { } precision)
					{
						Call();
						PutExpression(precision);
						Close();
					}

					return;

				default:
					Word(current.Kind switch
					{
						CurrentValue.Catalog               => "CURRENT_CATALOG",
						CurrentValue.Date                  => "CURRENT_DATE",
						CurrentValue.DefaultTransformGroup => "CURRENT_DEFAULT_TRANSFORM_GROUP",
						CurrentValue.Path                  => "CURRENT_PATH",
						CurrentValue.Role                  => "CURRENT_ROLE",
						CurrentValue.Schema                => "CURRENT_SCHEMA",
						CurrentValue.User                  => "USER",
						CurrentValue.SessionUser           => "SESSION_USER",
						CurrentValue.SystemUser            => "SYSTEM_USER",
						CurrentValue.CurrentUser           => "CURRENT_USER",
						CurrentValue.Value                 => "VALUE",
						_                                  => throw NoText(current.Kind),
					});
					return;
			}
		}

		void PutEnumeration(IReadOnlyList<Expression> items, bool trigraphs)
		{
			Tight(trigraphs ? "??(" : "[");
			Hold();
			Each(items, PutExpression);
			Tight(trigraphs ? "??)" : "]");
		}

		// ── Calls ──────────────────────────────────────────────────────────────────

		void PutInvocation(Expression.Invocation invocation)
		{
			PutSemantics(invocation.Semantics);
			PutName(invocation.Name);

			if (!invocation.WithoutParentheses)
			{
				Call();
				PutQuantifier(invocation.Quantifier);
				Each(invocation.Arguments, PutArgument);

				if (invocation.OrderBy is { } order)
				{
					Word("ORDER BY");
					PutSorts(order);
				}

				if (invocation.Overflow is { } overflow)
				{
					Word("ON OVERFLOW");

					if (!overflow.Truncate)
						Word("ERROR");
					else
					{
						Word("TRUNCATE");

						if (overflow.Filler is { } filler)
							PutExpression(filler);

						Word(overflow.WithCount == true ? "WITH COUNT" : "WITHOUT COUNT");
					}
				}

				Close();
			}

			if (invocation.From is { } from)
				Word(from == FromFirstOrLast.First ? "FROM FIRST" : "FROM LAST");

			if (invocation.Nulls is { } nulls)
				Word(nulls == NullTreatment.RespectNulls ? "RESPECT NULLS" : "IGNORE NULLS");

			if (invocation.WithinGroup is { } within)
			{
				Word("WITHIN GROUP");
				Open();
				Word("ORDER BY");
				PutSorts(within.OrderBy);
				Close();
			}

			PutFilterAndOver(invocation.Filter, invocation.Over);
		}

		void PutSemantics(RowPatternSemantics? semantics)
		{
			if (semantics is { } one)
				Word(one == RowPatternSemantics.Running ? "RUNNING" : "FINAL");
		}

		void PutFilterAndOver(FilterClause? filter, WindowReference? over)
		{
			if (filter is not null)
			{
				Word("FILTER");
				Open();
				Word("WHERE");
				PutExpression(filter.Condition);
				Close();
			}

			if (over is not null)
			{
				Word("OVER");

				if (over is WindowReference.NameRef name)
					PutIdentifier(name.Name);
				else
					PutWindow(((WindowReference.Specification)over).Value);
			}
		}

		void PutArguments(IReadOnlyList<Argument> arguments)
		{
			Call();
			Each(arguments, PutArgument);
			Close();
		}

		// An argument's `AS` names a type without brackets: `f (a AS t)`. A generalized expression anywhere
		// else is written in its own, `(a AS t).m ()`.
		void PutArgument(Argument argument)
		{
			if (argument.NamedAssignment)
			{
				PutIdentifier(argument.Name!);
				Word("=>");
			}

			if (argument.Value is Expression.Generalized generalized)
			{
				PutExpression(generalized.Value);
				Word("AS");
				PutType(generalized.Type);
			}
			else
				PutExpression(argument.Value);
		}

		void PutMember(Expression.Member member)
		{
			PutExpression(member.Target);

			switch (member.Kind)
			{
				case MemberAccessKind.Dot:
					Tight(".");
					Hold();
					break;

				case MemberAccessKind.Dereference:
					Word("->");
					break;

				default:
					Tight("::");
					Hold();
					break;
			}

			PutIdentifier(member.Name);

			if (member.Arguments is { } arguments)
				PutArguments(arguments);
		}

		void PutRegex(Expression.Regex regex)
		{
			Word(regex.Function switch
			{
				RegexFunction.OccurrencesRegex => "OCCURRENCES_REGEX",
				RegexFunction.PositionRegex    => "POSITION_REGEX",
				RegexFunction.SubstringRegex   => "SUBSTRING_REGEX",
				RegexFunction.TranslateRegex                              => "TRANSLATE_REGEX",
				_                              => throw NoText(regex.Function),
			});
			Call();

			if (regex.StartOrAfter is { } where)
				Word(where == RegexPositionStartOrAfter.Start ? "START" : "AFTER");

			PutExpression(regex.Pattern);

			if (regex.Flag is { } flag)
			{
				Word("FLAG");
				PutExpression(flag);
			}

			Word("IN");
			PutExpression(regex.Value);

			if (regex.Replacement is { } replacement)
			{
				Word("WITH");
				PutExpression(replacement);
			}

			if (regex.From is { } start)
			{
				Word("FROM");
				PutExpression(start);
			}

			PutUnits(regex.Using);

			if (regex.AllOccurrences)
				Word("OCCURRENCE ALL");
			else if (regex.Occurrence is { } occurrence)
			{
				Word("OCCURRENCE");
				PutExpression(occurrence);
			}

			if (regex.Group is { } group)
			{
				Word("GROUP");
				PutExpression(group);
			}

			Close();
		}

		// ── JSON ───────────────────────────────────────────────────────────────────

		void PutCommon(JsonApiCommon common)
		{
			PutExpression(common.Context);

			if (common.ContextFormat is { } format)
				PutInput(format);

			Tight(",");
			Word(common.PathSpecification);

			if (common.PathName is { } name)
			{
				Word("AS");
				PutIdentifier(name);
			}

			if (common.Passing.Count > 0)
			{
				Word("PASSING");
				Each(common.Passing, argument =>
				{
					PutExpression(argument.Value);

					if (argument.Format is { } argumentFormat)
						PutInput(argumentFormat);

					Word("AS");
					PutIdentifier(argument.Name);
				});
			}
		}

		void PutInput(JsonInputClause input)
		{
			Word("FORMAT");
			PutRepresentation(input.Representation);
		}

		void PutRepresentation(JsonRepresentation representation)
		{
			Word("JSON");

			if (representation.Encoding is { } encoding)
				Word(encoding switch { JsonEncoding.Utf8 => "ENCODING UTF8", JsonEncoding.Utf16 => "ENCODING UTF16", JsonEncoding.Utf32 => "ENCODING UTF32", _ => throw NoText(encoding) });
		}

		void PutOutput(JsonOutput? output)
		{
			if (output is null)
				return;

			Word("RETURNING");
			PutType(output.Type);

			if (output.Format is { } format)
			{
				Word("FORMAT");
				PutRepresentation(format);
			}
		}

		void PutMember(JsonMember member)
		{
			if (member.Syntax == JsonMemberSyntax.KeyValue)
				Word("KEY");

			PutExpression(member.Key);

			if (member.Syntax == JsonMemberSyntax.Colon)
				Word(":");
			else
				Word("VALUE");

			PutExpression(member.Value);

			if (member.Format is { } format)
				PutInput(format);
		}

		void PutElement(JsonElement element)
		{
			PutExpression(element.Value);

			if (element.Format is { } format)
				PutInput(format);
		}

		void PutNullHandling(JsonNullHandling? nulls)
		{
			if (nulls is { } one)
				Word(one == JsonNullHandling.NullOnNull ? "NULL ON NULL" : "ABSENT ON NULL");
		}

		void PutUniqueness(JsonKeyUniqueness? uniqueness)
		{
			if (uniqueness is { } one)
				Word(one switch
				{
					JsonKeyUniqueness.WithUniqueKeys    => "WITH UNIQUE KEYS",
					JsonKeyUniqueness.WithUnique        => "WITH UNIQUE",
					JsonKeyUniqueness.WithoutUniqueKeys => "WITHOUT UNIQUE KEYS",
					JsonKeyUniqueness.WithoutUnique                                   => "WITHOUT UNIQUE",
					_                                   => throw NoText(one),
				});
		}

		void PutQuotes(JsonQuotes? quotes)
		{
			if (quotes is null)
				return;

			Word(quotes.Behavior == JsonQuotesBehavior.Keep ? "KEEP QUOTES" : "OMIT QUOTES");

			if (quotes.OnScalarString)
				Word("ON SCALAR STRING");
		}

		void PutValueBehavior(JsonValueBehavior behavior)
		{
			switch (behavior)
			{
				case JsonValueBehavior.Error:
					Word("ERROR");
					break;

				case JsonValueBehavior.Null:
					Word("NULL");
					break;

				default:
					Word("DEFAULT");
					PutExpression(((JsonValueBehavior.Default)behavior).Value);
					break;
			}
		}

		static string Wrapper(JsonWrapperBehavior wrapper)
		{
			return wrapper switch
			{
				JsonWrapperBehavior.Without => "WITHOUT",
				JsonWrapperBehavior.WithoutArray => "WITHOUT ARRAY",
				JsonWrapperBehavior.With => "WITH",
				JsonWrapperBehavior.WithConditional => "WITH CONDITIONAL",
				JsonWrapperBehavior.WithUnconditional => "WITH UNCONDITIONAL",
				JsonWrapperBehavior.WithArray => "WITH ARRAY",
				JsonWrapperBehavior.WithConditionalArray => "WITH CONDITIONAL ARRAY",
				JsonWrapperBehavior.WithUnconditionalArray => "WITH UNCONDITIONAL ARRAY",
				_ => throw NoText(wrapper),
			};
		}

		static string QueryBehavior(JsonQueryBehavior behavior)
		{
			return behavior switch
			{
				JsonQueryBehavior.Error => "ERROR",
				JsonQueryBehavior.Null => "NULL",
				JsonQueryBehavior.EmptyArray => "EMPTY ARRAY",
				JsonQueryBehavior.EmptyObject => "EMPTY OBJECT",
				_ => throw NoText(behavior),
			};
		}

		// ── §6.39 The path language, as far as the SQL tokens reach ────────────────

		void PutPathAccessor(JsonPathAccessor accessor)
		{
			switch (accessor)
			{
				case JsonPathAccessor.WildcardMember:
					Tight(".*");
					break;

				case JsonPathAccessor.WildcardArray:
					Tight("[*]");
					break;

				case JsonPathAccessor.Array array:
					Tight("[");
					Hold();
					Each(array.Subscripts, subscript =>
					{
						PutPath(subscript.From);

						if (subscript.To is { } to)
						{
							Word("TO");
							PutPath(to);
						}
					});
					Tight("]");
					break;

				case JsonPathAccessor.Filter filter:
					Word("?");
					Open();
					PutPathPredicate(filter.Predicate);
					Close();
					break;

				case JsonPathAccessor.Method method:
					Tight(".");
					Hold();
					PutPathMethod(method.Value);
					break;

				default:
					throw Unwritten(accessor);
			}
		}

		void PutPath(JsonPathExpression path)
		{
			switch (path)
			{
				case JsonPathExpression.Variable variable when variable.Kind == JsonPathVariableKind.Context:
					Word("$");
					break;

				case JsonPathExpression.Variable variable when variable.Kind == JsonPathVariableKind.Last:
					Word("LAST");
					break;

				// Two signs in a row keep a space between them: `--` begins a comment.
				case JsonPathExpression.Unary unary:
					Word(unary.Operator == JsonPathUnaryOperator.Minus ? "-" : "+");

					if (unary.Operand is not JsonPathExpression.Unary)
						Hold();

					PutPath(unary.Operand);
					break;

				case JsonPathExpression.Binary binary:
					PutPath(binary.Left);
					Word(binary.Operator switch
					{
						JsonPathBinaryOperator.Add      => "+",
						JsonPathBinaryOperator.Subtract => "-",
						JsonPathBinaryOperator.Multiply => "*",
						JsonPathBinaryOperator.Divide   => "/",
						JsonPathBinaryOperator.Modulo                               => "%",
						_                               => throw NoText(binary.Operator),
					});
					PutPath(binary.Right);
					break;

				case JsonPathExpression.Access access:
					PutPath(access.Target);
					PutPathAccessor(access.Accessor);
					break;

				case JsonPathExpression.Parenthesized parenthesized:
					Open();
					PutPath(parenthesized.Value);
					Close();
					break;

				default:
					throw Unwritten(path);
			}
		}

		void PutPathPredicate(JsonPathPredicate predicate)
		{
			switch (predicate)
			{
				case JsonPathPredicate.Exists exists:
					Word("EXISTS");
					Open();
					PutPath(exists.Expression);
					Close();
					break;

				case JsonPathPredicate.Comparison comparison:
					PutPath(comparison.Left);
					Word(comparison.Operator switch
					{
						JsonPathComparisonOperator.Equal        => "==",
						JsonPathComparisonOperator.NotEqual     => "<>",
						JsonPathComparisonOperator.NotEqualBang => "!=",
						JsonPathComparisonOperator.Less         => "<",
						JsonPathComparisonOperator.Greater      => ">",
						JsonPathComparisonOperator.LessOrEqual  => "<=",
						JsonPathComparisonOperator.GreaterOrEqual                                       => ">=",
						_                                       => throw NoText(comparison.Operator),
					});
					PutPath(comparison.Right);
					break;

				case JsonPathPredicate.IsUnknown unknown:
					Open();
					PutPathPredicate(unknown.Predicate);
					Close();
					Word("IS UNKNOWN");
					break;

				case JsonPathPredicate.Parenthesized parenthesized:
					Open();
					PutPathPredicate(parenthesized.Predicate);
					Close();
					break;

				case JsonPathPredicate.Or or:
					for (var at = 0; at < or.Items.Count; at++)
					{
						if (at > 0)
							Word("||");

						PutPathPredicate(or.Items[at]);
					}

					break;

				default:
					throw Unwritten(predicate);
			}
		}

		void PutPathMethod(JsonMethod method)
		{
			Word(method.Kind switch
			{
				JsonMethodKind.Type        => "type",
				JsonMethodKind.Size        => "size",
				JsonMethodKind.Double      => "double",
				JsonMethodKind.Ceiling     => "ceiling",
				JsonMethodKind.Floor       => "floor",
				JsonMethodKind.Abs         => "abs",
				JsonMethodKind.DateTime    => "datetime",
				JsonMethodKind.KeyValue    => "keyvalue",
				JsonMethodKind.BigInt      => "bigint",
				JsonMethodKind.Boolean     => "boolean",
				JsonMethodKind.Date        => "date",
				JsonMethodKind.Decimal     => "decimal",
				JsonMethodKind.Integer     => "integer",
				JsonMethodKind.Number      => "number",
				JsonMethodKind.String      => "string",
				JsonMethodKind.Time        => "time",
				JsonMethodKind.TimeTz      => "time_tz",
				JsonMethodKind.Timestamp   => "timestamp",
				JsonMethodKind.TimestampTz => "timestamp_tz",
				_                          => throw NoText(method.Kind),
			});
			Call();

			if (method.Precision is { } precision)
			{
				Number(precision);

				if (method.Scale is { } scale)
				{
					Tight(",");
					Number(scale);
				}
			}

			Close();
		}
	}
}
