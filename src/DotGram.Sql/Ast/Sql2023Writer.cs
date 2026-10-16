using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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
/// <see cref="Expression.Parenthesized"/>, a <see cref="Query.Parenthesized"/> — and nowhere else: a tree
/// read from text holds every bracket that changed its shape. The one exception is a tree built by hand whose
/// text would mean something else without a pair — a set operation (<c>PutQuery</c>), an operand that binds
/// looser than its operator (<c>Strength</c>, <c>PutOperand</c>): it gets the fewest that keep its shape, and
/// read back it holds them as the <see cref="Expression.Parenthesized"/> and <see cref="Query.Parenthesized"/>
/// nodes the text has. Key words are written in
/// capitals, one space between tokens, none inside a name or a call; which of two spellings was written, the
/// tree says and this repeats.
/// </para>
/// <para>
/// <b>Any depth the parser reads is written</b>: every method that can reach a node of its own kind
/// checks the stack first and, where it runs low, goes on on a thread of its own (<c>SqlStack</c>).
/// </para>
/// </remarks>
public static partial class Sql2023Writer
{
	/// <summary>The tree's text, as the standard reads it. For text T-SQL reads, use <see cref="TransactSqlWriter.Write"/>.</summary>
	public static string Write(ISqlNode node)
	{
		return Write(node, transactSql: false);
	}

	/// <summary>The one writer, for the standard's reading or for T-SQL's (<see cref="TransactSqlWriter"/>).</summary>
	internal static string Write(ISqlNode node, bool transactSql)
	{
		var text = new Text(transactSql);

		text.PutNode(node);

		return text.ToString();
	}

	sealed partial class Text
	{
		readonly StringBuilder written = new();

		/// <summary>
		/// Whether the text is for T-SQL, which refuses besides what every reading refuses the trees T-SQL would read as
		/// others (<see cref="TransactSqlWriter"/>). Set once, by the entry that makes the writer.
		/// </summary>
		readonly bool transactSql;

		public Text(bool transactSql)
		{
			this.transactSql = transactSql;
		}

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
				case QualifiedName name:    PutName(name); break;
				case Identifier identifier: PutIdentifier(identifier); break;
				case Query query:           PutQuery(query); break;
				case SelectColumn column:   PutColumn(column); break;
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
			if (!held || Fuses(token))
				written.Append(' ');

			written.Append(token);
			held = false;
		}

		/// <summary>A token against what stands before it.</summary>
		void Tight(string token)
		{
			if (Fuses(token))
				written.Append(' ');

			written.Append(token);
			held = false;
		}

		/// <summary>
		/// Whether <paramref name="token"/>, written against what stands before it, would make a comment's
		/// delimiter of the two: `- -1` written tight is `--1`, the rest of the line a comment, and `/` against
		/// `*` opens a block comment. Every token goes through <see cref="Word"/> or <see cref="Tight"/>, so
		/// checking here keeps every pair of tokens apart, whichever node wrote them.
		/// </summary>
		bool Fuses(string token)
		{
			if (written.Length == 0 || token.Length == 0)
				return false;

			var last  = written[written.Length - 1];
			var first = token[0];

			return (last == '-' && first == '-') || (last == '/' && first == '*') || (last == '*' && first == '/');
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

		void Each<T>(SqlList<T> items, Action<T> put)
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
			PutIdentifier(identifier, pseudo: false);
		}

		/// <summary>
		/// A name. <paramref name="pseudo"/> says the place reads one of the engine's own columns (<c>$ACTION</c>), which only the
		/// grammar's qualified name does, and not an <c>Identifier</c>.
		/// </summary>
		void PutIdentifier(Identifier identifier, bool pseudo)
		{
			// T-SQL: SELECT Clause (Transact-SQL). A string is a column's alias and nothing else's name.
			if (identifier.Style is IdentifierStyle.String or IdentifierStyle.NationalString)
				throw Misplaced(identifier, "Style", "a string, which only a select column's alias is written as");

			CheckReserved(identifier);

			if (transactSql)
				CheckExternalName(identifier, "a name", pseudo);

			Word(identifier.Text);

			if (identifier.UnicodeEscape is { } escape)
				Tight("UESCAPE'" + escape + "'");
		}

		void PutName(QualifiedName name)
		{
			PutName(name, pseudoFrom: int.MaxValue);
		}

		/// <summary>
		/// A name, whose parts from <paramref name="pseudoFrom"/> may be one of the engine's own columns (<c>$ACTION</c>): a place that
		/// reads a column says 0, and one that reads an <c>Identifier</c> for its first part says 1. A name is a column only where the
		/// grammar says so, which <see cref="PutName(QualifiedName)"/> does not.
		/// </summary>
		void PutName(QualifiedName name, int pseudoFrom)
		{
			if (name.Count == 0)
				throw new ArgumentException("A name of no parts, default(QualifiedName), is no name and is not written.", nameof(name));

			for (var at = 0; at < name.Count; at++)
			{
				if (at > 0)
				{
					Tight(".");
					Hold();
				}

				PutPart(name, at, at >= pseudoFrom);
			}
		}

		/// <summary>
		/// A part of a name. In T-SQL's reading a part with no text that is not the last is left out, as <c>db..t</c> leaves out
		/// the schema: it is no name to check, and nothing stands in its place but the space that keeps the dot off the token
		/// before it.
		/// </summary>
		void PutPart(QualifiedName name, int at)
		{
			PutPart(name, at, pseudo: false);
		}

		void PutPart(QualifiedName name, int at, bool pseudo)
		{
			if (transactSql && at < name.Count - 1 && name[at] is { Text.Length: 0, Style: IdentifierStyle.Regular, UnicodeEscape: null })
				Word("");
			else
				PutIdentifier(name[at], pseudo);
		}

		static string Dotted(QualifiedName name)
		{
			if (name.Count == 0)
				throw new ArgumentException("A name of no parts, default(QualifiedName), is no name and is not written.", nameof(name));

			var text = new StringBuilder();

			for (var at = 0; at < name.Count; at++)
			{
				if (at > 0)
					text.Append('.');

				text.Append(name[at].Text);
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
					Word(NumericText(numeric));
					break;

				// The introducer and the letters before the quote are the literal's, and no space may stand in it.
				case LiteralValue.String text:
					if (transactSql && !IsQuoted(text))
						throw Misplaced(text, "Kind", "or CharacterSet that T-SQL does not read: a Unicode string, or one with an introducer");

					var introducer = text.CharacterSet is { } set ? "_" + Dotted(set.Name) : "";

					Word(text.Kind switch
					{
						StringLiteralKind.National  => "N" + text.Text,
						StringLiteralKind.Unicode   => introducer + "U&" + text.Text + (text.UnicodeEscape is { } escape ? "UESCAPE'" + escape + "'" : ""),
						StringLiteralKind.Character => introducer + text.Text,
						_                           => throw NoText(text.Kind),
					});
					break;

				// T-SQL's `0x0A` keeps the digits after its introducer; the standard's `X'0A'` the quoted digits. A text
				// that is not the flag's is refused, rather than written as another literal.
				case LiteralValue.Binary binary:
					if (transactSql && !binary.ZeroX)
						throw Misplaced(binary, "ZeroX", "false, which T-SQL does not read: its binary string is written 0x…");

					if (binary.ZeroX == binary.Text.StartsWith("'", StringComparison.Ordinal))
						throw Misplaced(binary, "Text", binary.ZeroX ? "in quotes on a 0x literal" : "without quotes on an X literal");

					if (binary.ZeroX && !HexDigits(binary.Text))
						throw Misplaced(binary, "Text", "that is not hex digits on a 0x literal");

					Word(binary.ZeroX ? "0x" + binary.Text : "X" + binary.Text);
					break;

				case LiteralValue.Boolean truth:
					if (transactSql)
						throw Misplaced(truth, "Value", "that T-SQL does not read: it has no boolean literal");

					Word(Truth(truth.Value));
					break;

				case LiteralValue.DateTime datetime:
					if (transactSql)
						throw Misplaced(datetime, "Kind", "that T-SQL does not read: it has no date, time or timestamp literal (an ODBC escape is another node)");

					Word(datetime.Kind switch { DateTimeLiteralKind.Date => "DATE", DateTimeLiteralKind.Time => "TIME", DateTimeLiteralKind.Timestamp => "TIMESTAMP", _ => throw NoText(datetime.Kind) });
					Word(datetime.Text);
					break;

				case LiteralValue.Interval interval:
					if (transactSql)
						throw Misplaced(interval, "Text", "that T-SQL does not read: it has no interval literal");

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
					if (TryPutTransactSql(literal))
						break;

					throw Unwritten(literal);
			}
		}

		/// <summary>
		/// <c>TSql.MoneyLiteral</c> with a sign in front: a currency sign from the closed list the engine reads, a sign and blanks
		/// as far apart as they like, then digits with a point that hugs them. The standard's writer carries it for the T-SQL
		/// nodes it writes.
		/// </summary>
		const string MoneyNumber = @"^[+-]?[$¢-¥৲-৳฿៛₠-₱﷼﹩＄￠-￡￥-￦][\u0001-\u001F\u0085\u200B\p{Zs}\p{Zl}\p{Zp}]*[+-]?[\u0001-\u001F\u0085\u200B\p{Zs}\p{Zl}\p{Zp}]*[0-9]+(?:\.[0-9]*)?\z";

		const string Dec = "[0-9](?:_?[0-9])*";

		/// <summary>
		/// <c>UnsignedNumericLiteral</c> as the standard reads it, after its sign: an exact or approximate number, or an integer in
		/// any radix, digits grouped by single underscores.
		/// </summary>
		static readonly string StandardNumber = "(?:(?:" + Dec + @"\." + "(?:" + Dec + ")?|\\." + Dec + "|0[xX](?:_?[0-9a-fA-F])+|0[oO](?:_?[0-7])+|0[bB](?:_?[01])+|" + Dec + ")(?:[eE][+-]?" + Dec + ")?)";

		static readonly Regex TransactSqlInteger     = new(@"^[+-]?[0-9]+\z");
		static readonly Regex TransactSqlDecimal     = new(@"^[+-]?(?:[0-9]+\.[0-9]*|\.[0-9]+)\z");
		static readonly Regex TransactSqlApproximate = new(@"^[+-]?(?:[0-9]+\.[0-9]*|\.[0-9]+|[0-9]+)[eE][+-]?[0-9]*\z");
		static readonly Regex Money                  = new(MoneyNumber);
		static readonly Regex StandardNumberText     = new("^[+-]?" + StandardNumber + @"\z");

		/// <summary>
		/// The text of a number, held to its kind in T-SQL's reading and to some numeric literal in the standard's: a text that is
		/// no number is not written, since it would reach the output as it stands (`1; SHUTDOWN;--` is a number to no one), and in
		/// T-SQL a text of another kind than the node's is another literal when read back (`1.5` is a decimal, not an integer).
		/// T-SQL has no integer in another radix and no number with underscores: its hexadecimal literal is a binary string.
		/// </summary>
		string NumericText(LiteralValue.Numeric numeric)
		{
			var text = numeric.Text;

			var reads = transactSql
				? numeric.Kind switch
				{
					NumericLiteralKind.DecimalInteger => TransactSqlInteger.IsMatch(text),
					NumericLiteralKind.Decimal        => TransactSqlDecimal.IsMatch(text),
					NumericLiteralKind.Approximate    => TransactSqlApproximate.IsMatch(text),
					NumericLiteralKind.Money          => Money.IsMatch(text),
					_                                 => false,
				}
				: numeric.Kind == NumericLiteralKind.Money
					? Money.IsMatch(text)
					: StandardNumberText.IsMatch(text);

			if (!reads)
				throw Misplaced(numeric, "Text", $"that is no {numeric.Kind} number literal{(transactSql ? " of T-SQL" : "")}, which would reach the output as it stands");

			return text;
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

		/// <summary>
		/// The key words T-SQL also reads as a type's name, bracketed, quoted or qualified by <c>sys</c>: the same type.
		/// </summary>
		static readonly HashSet<string> NamedKeyWords = new(StringComparer.OrdinalIgnoreCase)
		{
			"INT", "INTEGER", "SMALLINT", "BIGINT", "DEC", "DECIMAL", "NUMERIC", "FLOAT", "REAL", "CHAR", "CHARACTER",
			"VARCHAR", "NCHAR", "NVARCHAR", "BINARY", "VARBINARY", "DATE", "TIME", "JSON", "XML",
		};

		/// <summary>
		/// A type's key word, or, where T-SQL's <c>WrittenName</c> is set, that name in its place: refused where the
		/// name is not the key word as an identifier, or is qualified by anything but <c>sys</c>.
		/// </summary>
		void PutTypeName(DataType type, string keyword)
		{
			if (type.WrittenName is not { } written)
			{
				Word(keyword);

				return;
			}

			if (!NamedKeyWords.Contains(keyword)
				|| written.Count is 0 or > 2
				|| written.Count == 2 && !Unquoted(written[0].Text).Equals("SYS", StringComparison.OrdinalIgnoreCase)
				|| !Unquoted(written[written.Count - 1].Text).TrimEnd().Equals(keyword, StringComparison.OrdinalIgnoreCase))
				throw Misplaced(type, "WrittenName", $"that is not {keyword} written as a name, bare or qualified by sys");

			PutName(written);
		}

		/// <summary>
		/// Whether a 0x literal's text is hex digits, perhaps broken by T-SQL's line continuation, a backslash and the
		/// end of a line.
		/// </summary>
		static bool HexDigits(string text)
		{
			for (var at = 0; at < text.Length; at++)
			{
				if (Uri.IsHexDigit(text[at]))
					continue;

				if (text[at] != '\\')
					return false;

				if (at + 1 < text.Length && text[at + 1] == '\n')
					at++;
				else if (at + 2 < text.Length && text[at + 1] == '\r' && text[at + 2] == '\n')
					at += 2;
				else if (at + 1 < text.Length && text[at + 1] == '\r')
					at++;
				else
					return false;
			}

			return true;
		}

		/// <summary>A name part's text without its delimiters.</summary>
		static string Unquoted(string text)
		{
			return text.Length >= 2 && (text[0] == '[' && text[text.Length - 1] == ']' || text[0] == '"' && text[text.Length - 1] == '"')
				? text.Substring(1, text.Length - 2)
				: text;
		}

		void PutType(DataType type)
		{
			if (!SqlStack.Enough())
			{
				SqlStack.Deeper(this, type, static (into, node) => into.PutType(node));

				return;
			}

			// T-SQL: the name of a type written as an identifier is only the name of a type the key word names.
			if (type.WrittenName is not null && type is not (DataType.Character or DataType.Binary or DataType.Numeric or DataType.DateTime or DataType.Json or DataType.Xml))
				throw Misplaced(type, "WrittenName", "on a type that is not written by a key word of one word");

			switch (type)
			{
				case DataType.Character character:
					PutTypeName(character, character.Kind switch
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
						CharacterTypeKind.Nvarchar                     => "NVARCHAR",
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
					PutTypeName(binary, binary.Kind switch
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
					PutTypeName(numeric, numeric.Kind switch
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
					PutTypeName(datetime, datetime.Kind switch { DateTimeTypeKind.Date => "DATE", DateTimeTypeKind.Time => "TIME", DateTimeTypeKind.Timestamp => "TIMESTAMP", _ => throw NoText(datetime.Kind) });

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
					// T-SQL: the name of a system type reads back as that type.
					if (transactSql && IsSystemTypeName(user.Name, out _))
						throw Misplaced(user, "Name", "that is a system type of T-SQL, which reads back as that type");

					PutName(user.Name);
					break;

				case DataType.Json json:
					PutTypeName(json, "JSON");
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
					if (TryPutTransactSql(type))
						break;

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

		// How tightly an expression binds where it is written without brackets, from the loosest to the
		// tightest. A tree read from text holds a Parenthesized wherever its shape needed one, so on such a
		// tree the levels never add a bracket; on a tree built by hand they add the fewest that keep its shape.
		//
		// The standard's (§6.28, §8.1, §8.12): OR, AND, NOT over a boolean test, IS [NOT] TRUE over a
		// predicate, a predicate over values, `+ - ||` and MULTISET UNION and EXCEPT, `* /` and MULTISET
		// INTERSECT, one sign over a primary and what follows it, and one of COLLATE, AT and an interval
		// qualifier after a primary. So `- -a`, `NOT NOT a`, `a IS TRUE IS TRUE` and `a COLLATE x COLLATE y`
		// are not read there, and each takes a bracket.
		//
		// T-SQL's (TSqlValueExpression, TSqlValuePrimary, TSqlBooleanFactor): the same towers with its
		// operators in them — `& | ^ << >>` as weak as `+`, `%` as strong as `*`, `~` a sign — and signs and
		// NOTs that stand on each other. After a primary come its zones, each with a COLLATE that may stand
		// before its AT, and one COLLATE last: `a COLLATE x AT TIME ZONE b COLLATE y`.
		static class Tightness
		{
			public const int Disjunct       = 1;  // OR
			public const int Conjunct       = 2;  // AND
			public const int Negated        = 3;  // NOT
			public const int Tested         = 4;  // IS [NOT] TRUE, FALSE, UNKNOWN
			public const int Predicate      = 5;  // a comparison, BETWEEN, IN, LIKE, IS NULL and every other predicate
			public const int Additive       = 6;  // + - || & | ^ << >>, MULTISET UNION and EXCEPT
			public const int Multiplicative = 7;  // * / %, MULTISET INTERSECT
			public const int Signed         = 8;  // + - ~ before an operand
			public const int Postfixed      = 9;  // COLLATE; in the standard also AT and an interval qualifier
			public const int Zoned          = 10; // T-SQL's AT TIME ZONE
			public const int Primary        = 11; // everything that brackets or names itself
		}

		int Strength(Expression expression)
		{
			return expression switch
			{
				Expression.Binary { Operator: BinaryOperator.Or }  => Tightness.Disjunct,
				Expression.Binary { Operator: BinaryOperator.And } => Tightness.Conjunct,
				Expression.Binary { Operator: BinaryOperator.Multiply or BinaryOperator.Divide or BinaryOperator.Modulo } => Tightness.Multiplicative,
				Expression.Binary => Tightness.Additive,
				Expression.Unary { Operator: UnaryOperator.Not } => Tightness.Negated,
				Expression.Unary => Tightness.Signed,
				Expression.MultisetOperation { Operator: MultisetOperator.Intersect } => Tightness.Multiplicative,
				Expression.MultisetOperation => Tightness.Additive,
				Expression.IsTruth => Tightness.Tested,
				Expression.Comparison or Expression.Between or Expression.In or Expression.Like or Expression.IsNull
					or Expression.QuantifiedComparison or Expression.Exists or Expression.Unique or Expression.Match
					or Expression.Overlaps or Expression.IsDistinct or Expression.IsNormalized or Expression.MemberOf
					or Expression.SubmultisetOf or Expression.IsSet or Expression.IsOf or Expression.PeriodPredicate
					or Expression.JsonPredicate or Expression.GraphMatch => Tightness.Predicate,

				// The standard reads JSON_EXISTS as a predicate, T-SQL as a call.
				Expression.JsonExists => transactSql ? Tightness.Primary : Tightness.Predicate,
				Expression.Collate or Expression.IntervalQualified => Tightness.Postfixed,
				Expression.AtTimeZone => transactSql ? Tightness.Zoned : Tightness.Postfixed,
				_ => Tightness.Primary,
			};
		}

		/// <summary>
		/// An operand that must bind at least as tightly as <paramref name="least"/>, in a bracket where it
		/// does not. A left-associative operator asks its own strength of its left operand and one more of its
		/// right: `a - (b - c)`.
		/// </summary>
		/// <remarks>
		/// What the operator passed on for its operand — a condition, in T-SQL's reading — is still there for
		/// it inside the bracket, which is what a bracket is in that reading.
		/// </remarks>
		void PutOperand(Expression operand, int least)
		{
			if (Strength(operand) >= least)
			{
				PutExpression(operand);

				return;
			}

			Open();
			PutExpression(operand);
			Close();
		}

		/// <summary>
		/// The zone after AT TIME ZONE: a primary, which the standard may follow with an interval qualifier
		/// and T-SQL may put signs before; anything else in a bracket.
		/// </summary>
		void PutZone(Expression zone)
		{
			var fits = zone is Expression.IntervalQualified && !transactSql;

			if (!fits)
			{
				var bottom = zone;
				var signed = false;

				// T-SQL's signs stand on each other; under the last, an operand that binds looser than a
				// sign is bracketed by the sign itself, and only a COLLATE or an AT would run on past it.
				while (transactSql && bottom is Expression.Unary { Operator: not UnaryOperator.Not } sign)
				{
					bottom = sign.Operand;
					signed = true;
				}

				fits = Strength(bottom) >= Tightness.Primary || signed && Strength(bottom) < Tightness.Signed;
			}

			if (fits)
				PutExpression(zone);
			else
			{
				Open();
				PutExpression(zone);
				Close();
			}
		}

		void PutExpression(Expression expression)
		{
			if (!SqlStack.Enough())
			{
				SqlStack.Deeper(this, expression, static (into, node) => into.PutExpression(node));

				return;
			}

			// T-SQL's reading: a condition where PutCondition asked for one, a value everywhere else.
			var condition = TakeCondition(expression);

			// A `*` or `t.*` is read at a place that asked for one, and nowhere else.
			var star = this.star;
			var row  = this.row;

			this.star = false;
			this.row  = false;

			switch (expression)
			{
				case LiteralValue literal:
					CheckUnsigned(literal);
					PutLiteral(literal);
					break;

				case Expression.Reference reference:
					if (transactSql && Plain(reference.Name[0], "VALUE"))
						throw Misplaced(reference, "Name", "that begins with the plain word VALUE, which T-SQL reads as the current value; bracketed it is a name");

					PutReference(reference.Name);
					break;

				case Expression.Parameter parameter:
					PutParameter(parameter);
					break;

				case Expression.Current current:
					PutCurrent(current);
					break;

				case Expression.Unary unary when unary.Operator == UnaryOperator.Not:
					Word("NOT");
					PassCondition(condition);
					PutOperand(unary.Operand, transactSql ? Tightness.Negated : Tightness.Tested);
					break;

				// A sign is written against its operand; where the operand begins with a sign of its own, `Fuses`
				// keeps the two apart, so that `- -1` never becomes a comment's `--1`.
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
					PutOperand(unary.Operand, transactSql ? Tightness.Signed : Tightness.Postfixed);
					break;

				case Expression.Binary binary:
					var binding = Strength(binary);

					PassCondition(condition);
					PutOperand(binary.Left, binding);
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
					PassCondition(condition);
					PutOperand(binary.Right, binding + 1);
					break;

				case Expression.MultisetOperation multiset:
					PutOperand(multiset.Left, Strength(multiset));
					Word("MULTISET");
					Word(multiset.Operator switch { MultisetOperator.Union => "UNION", MultisetOperator.Except => "EXCEPT", MultisetOperator.Intersect => "INTERSECT", _ => throw NoText(multiset.Operator) });
					PutQuantifier(multiset.Quantifier);
					PutOperand(multiset.Right, Strength(multiset) + 1);
					break;

				case Expression.Row rowValue:
					if (transactSql && (!row || rowValue.RowKeyword))
						throw Misplaced(rowValue, "Items", "that T-SQL does not read as a value: it has no row value constructor or ROW (…), and reads a list in brackets only as an argument of the full-text and rowset functions");

					if (rowValue.RowKeyword)
					{
						Word("ROW");
						Call();
					}
					else
						Open();

					Each(rowValue.Items, item =>
					{
						// The items of a column list in T-SQL include `*` and `t.*`.
						this.star = true;
						PutExpression(item);
					});
					Close();
					break;

				case Expression.Parenthesized parenthesized:
					Open();
					PassCondition(condition);
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

						// A searched CASE's WHEN is a condition; a simple one's, a value it is compared with.
						if (@case.Operand is null)
							Each(when.When, one => PutCondition(when, one));
						else
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
					// T-SQL: TRY_CAST (Transact-SQL) has no FORMAT.
					if (cast.Try && cast.Format is not null)
						throw Both(cast, "Try", "Format");

					Word(cast.Try ? "TRY_CAST" : "CAST");
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
					if (transactSql && !star)
						throw Misplaced(expression, "a node", "outside an argument or a full-text column list, where T-SQL reads no *");

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
					PutOperand(element.Collection, Tightness.Primary);
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
					if (transactSql && !star)
						throw Misplaced(expression, "a node", "outside an argument, where T-SQL reads no .*");

					PutOperand(wildcard.Target, Tightness.Primary);
					Tight(wildcard.Kind switch { WildcardKind.Member => ".*", WildcardKind.Array => "[*]", _ => throw NoText(wildcard.Kind) });
					break;

				case Expression.JsonAccessor accessor:
					PutOperand(accessor.Target, Tightness.Primary);
					PutPathAccessor(accessor.Accessor);
					break;

				case Expression.NextValue next:
					Word("NEXT VALUE FOR");
					PutName(next.Sequence);

					// T-SQL: NEXT VALUE FOR (Transact-SQL). The order the statement's rows take their values in.
					if (next.Over is { } over)
					{
						Word("OVER");
						PutWindow(over);
					}

					break;

				case Expression.Collate collate:
					PutOperand(collate.Value, transactSql ? Tightness.Zoned : Tightness.Primary);
					Word("COLLATE");
					PutCollation(collate.Collation);
					break;

				case Expression.AtTimeZone zone:
					PutOperand(zone.Value, transactSql ? Tightness.Postfixed : Tightness.Primary);

					if (zone.Zone is { } at)
					{
						Word("AT TIME ZONE");
						PutZone(at);
					}
					else
						Word("AT LOCAL");

					break;

				case Expression.IntervalQualified qualified:
					PutOperand(qualified.Value, Tightness.Primary);
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
					// `!=` is the spelling of the inequality, and of no other operator.
					if (comparison.Exclamation && comparison.Operator != ComparisonOperator.NotEqual)
						throw Misplaced(comparison, "Exclamation", "on a comparison that is not an inequality");

					PutOperand(comparison.Left, Tightness.Additive);
					Word(comparison.Exclamation ? "!=" : Comparison(comparison.Operator));
					PutOperand(comparison.Right, Tightness.Additive);
					break;

				case Expression.Between between:
					PutOperand(between.Value, Tightness.Additive);
					PutNot(between.Not);
					Word("BETWEEN");

					if (between.Symmetry is { } symmetry)
						Word(symmetry == BetweenSymmetry.Asymmetric ? "ASYMMETRIC" : "SYMMETRIC");

					PutOperand(between.Lower, Tightness.Additive);
					Word("AND");
					PutOperand(between.Upper, Tightness.Additive);
					break;

				case Expression.In @in:
					PutOperand(@in.Value, Tightness.Additive);
					PutNot(@in.Not);
					Word("IN");

					if (@in.SourceValue is InSource.Subquery inQuery)
						PutSubquery(inQuery.Query);
					else
					{
						Open();
						Each(((InSource.Values)@in.SourceValue).Items, PutExpression);
						Close();
					}

					break;

				case Expression.Like like:
					PutOperand(like.Value, Tightness.Additive);
					PutNot(like.Not);
					Word(like.Kind switch { LikeKind.Like => "LIKE", LikeKind.Similar => "SIMILAR TO", LikeKind.Regex => "LIKE_REGEX", _ => throw NoText(like.Kind) });
					PutOperand(like.Pattern, Tightness.Additive);

					// T-SQL: LIKE (Transact-SQL). The escape in ODBC's braces.
					if (like.BracedEscape)
					{
						if (like.Escape is not { } braced || like.Kind != LikeKind.Like)
							throw Misplaced(like, "BracedEscape", "without an Escape, or on another kind than LIKE");

						Word("{");
						Hold();
						Word("ESCAPE");
						PutOperand(braced, Tightness.Additive);
						Tight("}");
					}
					else if (like.Escape is { } escape)
					{
						Word("ESCAPE");
						PutOperand(escape, Tightness.Additive);
					}

					if (like.Flag is { } flag)
					{
						Word("FLAG");
						PutOperand(flag, Tightness.Additive);
					}

					break;

				case Expression.IsNull isNull:
					PutOperand(isNull.Value, Tightness.Additive);
					Word("IS");
					PutNot(isNull.Not);
					Word("NULL");
					break;

				case Expression.IsTruth isTruth:
					PutOperand(isTruth.Value, Tightness.Predicate);
					Word("IS");
					PutNot(isTruth.Not);
					Word(Truth(isTruth.Truth));
					break;

				case Expression.QuantifiedComparison quantified:
					PutOperand(quantified.Left, Tightness.Additive);
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
					PutOperand(match.Value, Tightness.Additive);
					Word("MATCH");

					if (match.UniqueKeyword)
						Word("UNIQUE");

					if (match.Type is { } type)
						Word(type switch { MatchType.Simple => "SIMPLE", MatchType.Partial => "PARTIAL", MatchType.Full => "FULL", _ => throw NoText(type) });

					PutSubquery(match.Query);
					break;

				case Expression.Overlaps overlaps:
					PutOperand(overlaps.Left, Tightness.Additive);
					Word("OVERLAPS");
					PutOperand(overlaps.Right, Tightness.Additive);
					break;

				case Expression.IsDistinct distinct:
					PutOperand(distinct.Left, Tightness.Additive);
					Word("IS");
					PutNot(distinct.Not);
					Word("DISTINCT FROM");
					PutOperand(distinct.Right, Tightness.Additive);
					break;

				case Expression.IsNormalized normalized:
					PutOperand(normalized.Value, Tightness.Additive);
					Word("IS");
					PutNot(normalized.Not);

					if (normalized.Form is { } normalForm)
						Word(normalForm.ToString());

					Word("NORMALIZED");
					break;

				case Expression.MemberOf memberOf:
					PutOperand(memberOf.Value, Tightness.Additive);
					PutNot(memberOf.Not);
					Word(memberOf.OfKeyword ? "MEMBER OF" : "MEMBER");
					PutOperand(memberOf.Collection, Tightness.Additive);
					break;

				case Expression.SubmultisetOf submultiset:
					PutOperand(submultiset.Value, Tightness.Additive);
					PutNot(submultiset.Not);
					Word(submultiset.OfKeyword ? "SUBMULTISET OF" : "SUBMULTISET");
					PutOperand(submultiset.Collection, Tightness.Additive);
					break;

				case Expression.IsSet isSet:
					PutOperand(isSet.Value, Tightness.Additive);
					Word("IS");
					PutNot(isSet.Not);
					Word("A SET");
					break;

				case Expression.IsOf isOf:
					PutOperand(isOf.Value, Tightness.Additive);
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
						PutOperand(((PeriodRight.Point)period.Right).Value, Tightness.Additive);

					break;

				case Expression.JsonPredicate json:
					PutOperand(json.Value, Tightness.Additive);

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
					PutCommon(exists.Common, standard: true);

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
					PutCommon(value.Common, standard: value.OnEmpty is not null || value.OnError is not null);

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

					// T-SQL: JSON_QUERY (Transact-SQL). `JSON_QUERY (x)`, with no path, which the standard has no
					// text for: the context alone, with nothing of the standard's beside it.
					if (query.Common.Path is null)
					{
						if (query is not { Output: null, Quotes: null, OnEmpty: null, OnError: null, Wrapper: null or JsonWrapperBehavior.WithArray, Common: { PathName: null, Passing.Count: 0, ContextFormat: null } })
							throw Misplaced(query, "Common.Path", "null beside what only the standard's JSON_QUERY writes");

						PutExpression(query.Common.Context);
					}
					else
						PutCommon(query.Common, standard: query is not { Output: null, Quotes: null, OnEmpty: null, OnError: null, Wrapper: null or JsonWrapperBehavior.WithArray });
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
					if (TryPutTransactSql(expression))
						break;

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
				Word(one switch { SetQuantifier.All => "ALL", SetQuantifier.Distinct => "DISTINCT", _ => throw NoText(one) });
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

		// The same refusal for a node rather than an enum value. Where two properties stand for two
		// different constructs, a node with both set has no text: whatever is printed loses one of them.
		static Exception Both(object node, string one, string other)
		{
			return new ArgumentException(
				$"{node.GetType().Name} has both {one} and {other}; they are different constructs and at most one is written.",
				nameof(node));
		}

		// And where a node holds something where the text has no place for it, it has no text either.
		static Exception Misplaced(object node, string what, string where)
		{
			return new ArgumentException(
				$"{node.GetType().Name} has {what} {where}; there is no text for it there.",
				nameof(node));
		}

		// And where a node stands for one of several things, a node that is none of them has no text either.
		static Exception Empty(object node, params string[] any)
		{
			return new ArgumentException(
				$"{node.GetType().Name} has none of {string.Join(", ", any)}, so there is nothing to write.",
				nameof(node));
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
					PutName(current.TypeName!.Value);
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

		void PutEnumeration(SqlList<Expression> items, bool trigraphs)
		{
			Tight(trigraphs ? "??(" : "[");
			Hold();
			Each(items, PutExpression);
			Tight(trigraphs ? "??)" : "]");
		}

		// ── Calls ──────────────────────────────────────────────────────────────────

		void PutInvocation(Expression.Invocation invocation)
		{
			PutInvocation(invocation, anyWord: false);
		}

		/// <summary>
		/// A call. Its name of one part is written through <see cref="PutCallName"/>: a reserved word the grammar reads as a
		/// function's name, or any word where <paramref name="anyWord"/> says the place reads one (ODBC's name in <c>{fn …}</c>).
		/// </summary>
		void PutInvocation(Expression.Invocation invocation, bool anyWord)
		{
			PutSemantics(invocation.Semantics);
			PutCallName(invocation.Name, anyWord);

			if (!invocation.WithoutParentheses)
			{
				Call();
				PutQuantifier(invocation.Quantifier);

				// T-SQL: IIF (Transact-SQL). Its first argument is a condition (SearchCondition).
				var iif = invocation.Name is [{ Style: IdentifierStyle.Regular } called] && Plain(called, "IIF") && invocation.Arguments.Count == 3;

				// T-SQL: a column list in brackets is an argument of the full-text and rowset functions, and of no other.
				var lists = transactSql && invocation.Name is [{ Style: IdentifierStyle.Regular } listed] && Plain(listed, "CONTAINS", "FREETEXT", "OPENROWSET", "SEMANTICKEYPHRASETABLE", "SEMANTICSIMILARITYTABLE", "SEMANTICSIMILARITYDETAILSTABLE");

				Each(invocation.Arguments, argument =>
				{
					condition = iif && transactSql;
					iif = false;
					listArgument = lists;
					PutArgument(argument);
				});

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
				Word(nulls switch { NullTreatment.RespectNulls => "RESPECT NULLS", NullTreatment.IgnoreNulls => "IGNORE NULLS", _ => throw NoText(nulls) });

			if (invocation.WithinGroup is { } within)
			{
				Word("WITHIN GROUP");
				Open();
				Word("ORDER BY");
				PutSorts(within.OrderBy);
				Close();
			}

			// T-SQL: SHORTEST_PATH (Transact-SQL). The other WITHIN GROUP, over a graph path's nodes.
			if (invocation.GraphPath)
			{
				if (invocation.WithinGroup is not null)
					throw Both(invocation, "GraphPath", "WithinGroup");

				Word("WITHIN GROUP");
				Open();
				Word("GRAPH PATH");
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
				PutCondition(filter, filter.Condition);
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

		void PutArguments(SqlList<Argument> arguments)
		{
			Call();
			Each(arguments, PutArgument);
			Close();
		}

		// An argument's `AS` names a type without brackets: `f (a AS t)`. A generalized expression anywhere
		// else is written in its own, `(a AS t).m ()`.
		void PutArgument(Argument argument)
		{
			if (transactSql && argument.Name is not null)
				throw Misplaced(argument, "Name", "that T-SQL does not read: its function calls pass arguments by position");

			// A name without `=>` has no text, and `=>` without a name none either: either would be dropped or crash.
			if (argument.Name is not null != argument.NamedAssignment)
				throw Misplaced(argument, "Name", "set without NamedAssignment, or NamedAssignment without a Name");

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
			{
				star = true;
				row  = listArgument;
				listArgument = false;
				PutExpression(argument.Value);
			}
		}

		void PutMember(Expression.Member member)
		{
			// T-SQL reads `a.m` as a name, and `->` not at all; a method's brackets or `::` make a member.
			if (transactSql && (member.Kind == MemberAccessKind.Dereference || member.Kind == MemberAccessKind.Dot && member.Arguments is null && member.Target is Expression.Reference))
				throw Misplaced(member, "Kind", "or Target that T-SQL does not read as a member: `a.m` is a name, and `->` is not T-SQL's");

			PutOperand(member.Target, Tightness.Primary);

			switch (member.Kind)
			{
				case MemberAccessKind.Dot:
					Tight(".");
					Hold();
					break;

				case MemberAccessKind.Dereference:
					Word("->");
					break;

				case MemberAccessKind.StaticMethod:
					Tight("::");
					Hold();
					break;

				default:
					throw NoText(member.Kind);
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

		/// <summary>
		/// The context, the path and what may follow it. T-SQL writes any value as a path; where something only the
		/// standard writes stands beside it (<paramref name="standard"/>, or the common's own name, PASSING or FORMAT),
		/// the path is the standard's, a character string, as nothing else reads back there.
		/// </summary>
		void PutCommon(JsonApiCommon common, bool standard = false)
		{
			if ((standard || common.PathName is not null || common.Passing.Count > 0 || common.ContextFormat is not null)
				&& common.Path is not (null or LiteralValue.String { Kind: StringLiteralKind.Character }))
				throw Misplaced(common, "Path", "that is not a character string, beside what only the standard's JSON functions write");

			PutExpression(common.Context);

			if (common.ContextFormat is { } format)
				PutInput(format);

			// The standard's path, or T-SQL's value; JSON_QUERY's one-argument form is its own writer's.
			Tight(",");
			PutExpression(common.Path ?? throw Empty(common, "Path"));

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
			// T-SQL: JSON_OBJECT (Transact-SQL), JSON_OBJECTAGG (Transact-SQL). A key and its value are joined by a colon, never by KEY or VALUE.
			if (member.Syntax is not (JsonMemberSyntax.KeyValue or JsonMemberSyntax.Value or JsonMemberSyntax.Colon))
				throw NoText(member.Syntax);

			if (transactSql && member.Syntax != JsonMemberSyntax.Colon)
				throw Misplaced(member, "Syntax", "other than the colon, which is how T-SQL joins a key and its value");

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
				Word(one switch { JsonNullHandling.NullOnNull => "NULL ON NULL", JsonNullHandling.AbsentOnNull => "ABSENT ON NULL", _ => throw NoText(one) });
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
			if (!SqlStack.Enough())
			{
				SqlStack.Deeper(this, path, static (into, node) => into.PutPath(node));

				return;
			}

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
			if (!SqlStack.Enough())
			{
				SqlStack.Deeper(this, predicate, static (into, node) => into.PutPathPredicate(node));

				return;
			}

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
