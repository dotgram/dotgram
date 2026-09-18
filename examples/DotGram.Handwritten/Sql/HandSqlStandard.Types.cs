using System;
using System.Collections.Generic;

using DotGram.Sql.Ast;

namespace DotGram.Handwritten;

// ISO/IEC 9075-2:2023 §6.1, the data types.
partial class HandSqlStandard
{
	/// <summary>
	/// <c>&lt;data type&gt;</c>, with <c>&lt;collection type&gt;</c>'s <c>ARRAY</c> and
	/// <c>MULTISET</c> after it as often as they stand there, each wrapping what came before it.
	/// </summary>
	static bool DataType(ref SqlCursor cursor, out DataType type)
	{
		if (!DataTypeBase(ref cursor, out type))
			return false;

		while (true)
		{
			if (cursor.Word == SqlWord.Array)
			{
				cursor.Take();

				int? cardinality = null;
				var  trigraphs   = false;

				if (cursor.Kind is SqlTokenKind.LeftBracket or SqlTokenKind.LeftTrigraph)
				{
					var save = cursor;

					trigraphs = cursor.Kind == SqlTokenKind.LeftTrigraph;

					cursor.Take();

					// The BNF pairs no bracket with its own kind of closing bracket, and neither
					// does this: what the tree keeps is which one opened.
					if (!Unsigned(ref cursor, out cardinality) ||
						!(cursor.Kind is SqlTokenKind.RightBracket or SqlTokenKind.RightTrigraph))
					{
						cursor      = save;
						cardinality = null;
						trigraphs   = false;
					}
					else
					{
						cursor.Take();
					}
				}

				type = new DataType.Array(type, cardinality, trigraphs);

				continue;
			}

			if (cursor.Take(SqlWord.Multiset))
			{
				type = new DataType.Multiset(type);

				continue;
			}

			return true;
		}
	}

	/// <summary>The type itself: a predefined one, a row, a reference, or a user-defined type's name.</summary>
	static bool DataTypeBase(ref SqlCursor cursor, out DataType type)
	{
		switch (cursor.Word)
		{
			case SqlWord.Character:
			case SqlWord.Char:
			case SqlWord.Varchar:
			case SqlWord.Clob:
				return CharacterStringType(ref cursor, out type);

			case SqlWord.National:
			case SqlWord.Nchar:
			case SqlWord.Nclob:
				return NationalCharacterStringType(ref cursor, out type);

			case SqlWord.Binary:
			case SqlWord.Varbinary:
			case SqlWord.Blob:
				return BinaryStringType(ref cursor, out type);

			case SqlWord.Numeric:
			case SqlWord.Decimal:
			case SqlWord.Dec:
			case SqlWord.Smallint:
			case SqlWord.Integer:
			case SqlWord.Int:
			case SqlWord.Bigint:
			case SqlWord.Real:
			case SqlWord.Float:
			case SqlWord.Double:
			case SqlWord.Decfloat:
				return NumericType(ref cursor, out type);

			case SqlWord.Boolean:
				cursor.Take();

				type = new DataType.Boolean();

				return true;

			case SqlWord.Date:
			case SqlWord.Time:
			case SqlWord.Timestamp:
				return DatetimeType(ref cursor, out type);

			case SqlWord.Interval:
			{
				var save = cursor;

				cursor.Take();

				if (IntervalQualifier(ref cursor, out var qualifier))
				{
					type = new DataType.Interval(qualifier);

					return true;
				}

				cursor = save;
				type   = null!;

				return false;
			}

			case SqlWord.Json:
				cursor.Take();

				type = new DataType.Json();

				return true;

			case SqlWord.Row:
				return RowType(ref cursor, out type);

			case SqlWord.Ref:
				return ReferenceType(ref cursor, out type);
		}

		// <path-resolved user-defined type name>.
		if (Names(ref cursor, 3, out var name))
		{
			type = new DataType.UserDefined(name);

			return true;
		}

		type = null!;

		return false;
	}

	// ── The character string types ─────────────────────────────────────────────

	/// <summary>
	/// <c>CHARACTER</c>, <c>CHAR</c>, <c>VARCHAR</c> and <c>CLOB</c>, each keeping which of its
	/// spellings was written, with the character set and the collation the type may name.
	/// </summary>
	static bool CharacterStringType(ref SqlCursor cursor, out DataType type)
	{
		var word = cursor.Word;

		cursor.Take();

		CharacterTypeKind kind;
		StringLength      length = default;

		switch (word)
		{
			case SqlWord.Varchar:
				kind = CharacterTypeKind.Varchar;

				CharacterLength(ref cursor, ref length);

				break;

			case SqlWord.Clob:
				kind = CharacterTypeKind.Clob;

				LargeObjectCharacterLength(ref cursor, ref length);

				break;

			default:
			{
				var spelled = word == SqlWord.Character;

				if (cursor.Take(SqlWord.Varying))
				{
					kind = spelled ? CharacterTypeKind.CharacterVarying : CharacterTypeKind.CharVarying;

					CharacterLength(ref cursor, ref length);

					break;
				}

				// `CHARACTER LARGE OBJECT`, and `CHARACTER` alone where the rest does not follow.
				var large = cursor;

				if (cursor.Take(SqlWord.Large) && cursor.TakeWord("OBJECT"))
				{
					kind = spelled ? CharacterTypeKind.CharacterLargeObject : CharacterTypeKind.CharLargeObject;

					LargeObjectCharacterLength(ref cursor, ref length);

					break;
				}

				cursor = large;
				kind   = spelled ? CharacterTypeKind.Character : CharacterTypeKind.Char;

				CharacterLength(ref cursor, ref length);

				break;
			}
		}

		type = Characters(kind, length, ref cursor, set: true);

		return true;
	}

	/// <summary>The national character string types, which name no character set of their own.</summary>
	static bool NationalCharacterStringType(ref SqlCursor cursor, out DataType type)
	{
		var word = cursor.Word;
		var save = cursor;

		cursor.Take();

		CharacterTypeKind kind;
		StringLength      length = default;

		switch (word)
		{
			case SqlWord.Nclob:
				kind = CharacterTypeKind.Nclob;

				LargeObjectCharacterLength(ref cursor, ref length);

				break;

			case SqlWord.Nchar:
				if (cursor.Take(SqlWord.Varying))
				{
					kind = CharacterTypeKind.NcharVarying;

					CharacterLength(ref cursor, ref length);

					break;
				}

				var object_ = cursor;

				if (cursor.Take(SqlWord.Large) && cursor.TakeWord("OBJECT"))
				{
					kind = CharacterTypeKind.NcharLargeObject;

					LargeObjectCharacterLength(ref cursor, ref length);

					break;
				}

				cursor = object_;
				kind   = CharacterTypeKind.Nchar;

				CharacterLength(ref cursor, ref length);

				break;

			default:
			{
				// NATIONAL, which CHARACTER or CHAR follows.
				var spelled = cursor.Word == SqlWord.Character;

				if (!cursor.Take(SqlWord.Character) && !cursor.Take(SqlWord.Char))
					goto refused;

				if (cursor.Take(SqlWord.Varying))
				{
					kind = spelled ? CharacterTypeKind.NationalCharacterVarying : CharacterTypeKind.NationalCharVarying;

					CharacterLength(ref cursor, ref length);

					break;
				}

				// Only `NATIONAL CHARACTER LARGE OBJECT` is spelled out; `NATIONAL CHAR` is not.
				var large = cursor;

				if (spelled && cursor.Take(SqlWord.Large) && cursor.TakeWord("OBJECT"))
				{
					kind = CharacterTypeKind.NationalCharacterLargeObject;

					LargeObjectCharacterLength(ref cursor, ref length);

					break;
				}

				cursor = large;
				kind   = spelled ? CharacterTypeKind.NationalCharacter : CharacterTypeKind.NationalChar;

				CharacterLength(ref cursor, ref length);

				break;
			}
		}

		type = Characters(kind, length, ref cursor, set: false);

		return true;

	refused:

		cursor = save;
		type   = null!;

		return false;
	}

	/// <summary>A character string type, with the character set and the collation written after it.</summary>
	static DataType Characters(CharacterTypeKind kind, StringLength length, ref SqlCursor cursor, bool set)
	{
		CharacterSetName? characterSet = null;

		if (set)
		{
			var save = cursor;

			if (cursor.Take(SqlWord.Character) && cursor.Take(SqlWord.Set))
			{
				if (!CharacterSetSpecification(ref cursor, out characterSet))
					cursor = save;
			}
			else
			{
				cursor = save;
			}
		}

		CollationName? collation = null;

		if (cursor.Word == SqlWord.Collate)
		{
			var save = cursor;

			cursor.Take();

			if (Names(ref cursor, 3, out var name))
				collation = new CollationName(name);
			else
				cursor = save;
		}

		return new DataType.Character(kind, length.Characters, length.Unit, characterSet, collation, length.LargeObject);
	}

	/// <summary>
	/// <c>&lt;character length&gt;</c> in brackets, where any was written. The BNF writes the
	/// brackets and what they hold as one optional group, so a bracket that opens anything else is
	/// not the type's and is left where it stands.
	/// </summary>
	static void CharacterLength(ref SqlCursor cursor, ref StringLength length)
	{
		if (cursor.Kind != SqlTokenKind.LeftParen)
			return;

		var save = cursor;

		cursor.Take();

		if (Unsigned(ref cursor, out var characters))
		{
			var written = new StringLength(characters, null, Units(ref cursor));

			if (cursor.Take(SqlTokenKind.RightParen))
			{
				length = written;

				return;
			}
		}

		cursor = save;
	}

	/// <summary><c>&lt;character large object length&gt;</c> in brackets, where any was written.</summary>
	static void LargeObjectCharacterLength(ref SqlCursor cursor, ref StringLength length)
	{
		if (cursor.Kind != SqlTokenKind.LeftParen)
			return;

		var save = cursor;

		cursor.Take();

		if (LargeObjectLength(ref cursor, out var size))
		{
			var written = new StringLength(null, size, Units(ref cursor));

			if (cursor.Take(SqlTokenKind.RightParen))
			{
				length = written;

				return;
			}
		}

		cursor = save;
	}

	/// <summary><c>&lt;large object length&gt;</c>: a number, and the multiplier after it.</summary>
	static bool LargeObjectLength(ref SqlCursor cursor, out LargeObjectSize size)
	{
		size = default;

		if (cursor.Kind != SqlTokenKind.Number || !IsUnsignedInteger(cursor.Span))
			return false;

		var value = Long(cursor.Span);

		cursor.Take();

		// `2 K` is a length and `2K` one token of it; either way the letter stands on its own here.
		char? multiplier = null;

		if (cursor.Kind == SqlTokenKind.Word && cursor.Span.Length == 1)
		{
			var letter = cursor.Span[0];

			if ((letter | 0x20) is 'k' or 'm' or 'g' or 't' or 'p')
			{
				multiplier = letter;

				cursor.Take();
			}
		}

		size = new LargeObjectSize(value, multiplier);

		return true;
	}

	/// <summary><c>&lt;char length units&gt;</c>, which §5.2 does not reserve.</summary>
	static LengthUnit? Units(ref SqlCursor cursor)
	{
		if (cursor.TakeWord("CHARACTERS"))
			return LengthUnit.Characters;

		if (cursor.TakeWord("OCTETS"))
			return LengthUnit.Octets;

		return null;
	}

	// ── The binary, numeric and datetime types ─────────────────────────────────

	static bool BinaryStringType(ref SqlCursor cursor, out DataType type)
	{
		var word = cursor.Word;
		var save = cursor;

		cursor.Take();

		BinaryTypeKind   kind;
		int?             length = null;
		LargeObjectSize? large  = null;

		switch (word)
		{
			case SqlWord.Varbinary:
				kind   = BinaryTypeKind.Varbinary;
				length = Length(ref cursor);

				break;

			case SqlWord.Blob:
				kind  = BinaryTypeKind.Blob;
				large = Size(ref cursor);

				break;

			default:
				if (cursor.Take(SqlWord.Varying))
				{
					kind   = BinaryTypeKind.BinaryVarying;
					length = Length(ref cursor);

					break;
				}

				var object_ = cursor;

				if (cursor.Take(SqlWord.Large) && cursor.TakeWord("OBJECT"))
				{
					kind  = BinaryTypeKind.BinaryLargeObject;
					large = Size(ref cursor);

					break;
				}

				cursor = object_;
				kind   = BinaryTypeKind.Binary;
				length = Length(ref cursor);

				break;
		}

		type = new DataType.Binary(kind, length, large);

		return true;
	}

	static bool NumericType(ref SqlCursor cursor, out DataType type)
	{
		var word = cursor.Word;
		var save = cursor;

		cursor.Take();

		NumericTypeKind kind;
		int?            precision = null, scale = null;

		switch (word)
		{
			case SqlWord.Numeric:
			case SqlWord.Decimal:
			case SqlWord.Dec:
				kind = word == SqlWord.Numeric ? NumericTypeKind.Numeric : word == SqlWord.Decimal ? NumericTypeKind.Decimal : NumericTypeKind.Dec;

				if (cursor.Kind == SqlTokenKind.LeftParen)
				{
					var brackets = cursor;

					cursor.Take();

					if (!Unsigned(ref cursor, out precision) ||
						cursor.Take(SqlTokenKind.Comma) && !Unsigned(ref cursor, out scale) ||
						!cursor.Take(SqlTokenKind.RightParen))
					{
						cursor    = brackets;
						precision = null;
						scale     = null;
					}
				}

				break;

			case SqlWord.Float:
			case SqlWord.Decfloat:
				kind      = word == SqlWord.Float ? NumericTypeKind.Float : NumericTypeKind.DecFloat;
				precision = Length(ref cursor);

				break;

			case SqlWord.Double:
				if (!cursor.Take(SqlWord.Precision))
					goto refused;

				kind = NumericTypeKind.DoublePrecision;

				break;

			default:
				kind = word switch
				{
					SqlWord.Smallint => NumericTypeKind.SmallInt,
					SqlWord.Integer  => NumericTypeKind.Integer,
					SqlWord.Int      => NumericTypeKind.Int,
					SqlWord.Bigint   => NumericTypeKind.BigInt,
					_                => NumericTypeKind.Real,
				};

				break;
		}

		type = new DataType.Numeric(kind, precision, scale);

		return true;

	refused:

		cursor = save;
		type   = null!;

		return false;
	}

	static bool DatetimeType(ref SqlCursor cursor, out DataType type)
	{
		var word = cursor.Word;
		var save = cursor;

		cursor.Take();

		if (word == SqlWord.Date)
		{
			type = new DataType.DateTime(DateTimeTypeKind.Date);

			return true;
		}

		var precision = Length(ref cursor);

		TimeZoneMode? zone = null;

		var zoned = cursor;

		if (cursor.Take(SqlWord.With))
			zone = TimeZoneMode.With;
		else if (cursor.Take(SqlWord.Without))
			zone = TimeZoneMode.Without;

		if (zone is not null && !(cursor.Take(SqlWord.Time) && cursor.TakeWord("ZONE")))
		{
			cursor = zoned;
			zone   = null;
		}

		type = new DataType.DateTime(word == SqlWord.Timestamp ? DateTimeTypeKind.Timestamp : DateTimeTypeKind.Time, precision, zone);

		return true;
	}

	/// <summary>An unsigned integer in brackets, where any was written.</summary>
	static int? Length(ref SqlCursor cursor)
	{
		if (cursor.Kind != SqlTokenKind.LeftParen)
			return null;

		var save = cursor;

		cursor.Take();

		if (Unsigned(ref cursor, out var length) && cursor.Take(SqlTokenKind.RightParen))
			return length;

		cursor = save;

		return null;
	}

	/// <summary>A large object's size in brackets, where any was written.</summary>
	static LargeObjectSize? Size(ref SqlCursor cursor)
	{
		if (cursor.Kind != SqlTokenKind.LeftParen)
			return null;

		var save = cursor;

		cursor.Take();

		if (LargeObjectLength(ref cursor, out var value) && cursor.Take(SqlTokenKind.RightParen))
			return value;

		cursor = save;

		return null;
	}

	// ── The row and reference types ────────────────────────────────────────────

	static bool RowType(ref SqlCursor cursor, out DataType type)
	{
		var save = cursor;

		cursor.Take();

		type = null!;

		if (!cursor.Take(SqlTokenKind.LeftParen))
		{
			cursor = save;

			return false;
		}

		var fields = new List<FieldDefinition>();

		while (true)
		{
			if (!Identifier(ref cursor, out var name) || !DataType(ref cursor, out var field))
			{
				cursor = save;

				return false;
			}

			fields.Add(new FieldDefinition(name, field));

			if (!cursor.Take(SqlTokenKind.Comma))
				break;
		}

		if (!cursor.Take(SqlTokenKind.RightParen))
		{
			cursor = save;

			return false;
		}

		type = new DataType.Row(fields);

		return true;
	}

	static bool ReferenceType(ref SqlCursor cursor, out DataType type)
	{
		var save = cursor;

		cursor.Take();

		type = null!;

		if (!cursor.Take(SqlTokenKind.LeftParen) || !Names(ref cursor, 3, out var referenced) || !cursor.Take(SqlTokenKind.RightParen))
		{
			cursor = save;

			return false;
		}

		QualifiedName? scope = null;

		if (cursor.Word == SqlWord.Scope)
		{
			var scoped = cursor;

			cursor.Take();

			if (TableName(ref cursor, out var scoping))
				scope = scoping;
			else
				cursor = scoped;
		}

		type = new DataType.Reference(referenced, scope);

		return true;
	}

	// ── §6.1 <character set specification>, <collate clause> ───────────────────

	/// <summary>
	/// <c>&lt;character set specification&gt;</c>: names, and an <c>&lt;SQL language
	/// identifier&gt;</c> last — Latin letters, digits and underscores, and no rule against a
	/// reserved word where it stands.
	/// </summary>
	static bool CharacterSetSpecification(ref SqlCursor cursor, out CharacterSetName? name)
	{
		var save  = cursor;
		var parts = new List<Identifier>();

		for (var taken = 0; taken < 2; taken++)
		{
			var qualified = cursor;

			if (!Identifier(ref cursor, out var part) || !cursor.Take(SqlTokenKind.Dot))
			{
				cursor = qualified;

				break;
			}

			parts.Add(part);
		}

		if (cursor.Kind != SqlTokenKind.Word || !IsLanguageIdentifier(cursor.Span))
		{
			cursor = save;
			name   = null;

			return false;
		}

		parts.Add(new Identifier(cursor.TextOf(cursor.Token)));

		cursor.Take();

		name = new CharacterSetName(new QualifiedName(parts.ToArray()));

		return true;
	}

	static bool IsLanguageIdentifier(ReadOnlySpan<char> word)
	{
		if ((uint)((word[0] | 0x20) - 'a') > 25)
			return false;

		foreach (var ch in word)
			if ((uint)((ch | 0x20) - 'a') > 25 && !SqlCursor.IsDigit(ch) && ch != '_')
				return false;

		return true;
	}

}

/// <summary>A length as a character string type writes it: a number or a large object's size, and its units.</summary>
readonly record struct StringLength(int? Characters, LargeObjectSize? LargeObject, LengthUnit? Unit);
