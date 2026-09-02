using System;

namespace DotGram.Benchmarks;

/// <summary>
/// <c>SqlStandard92</c>'s search condition, written by hand: the yardstick every claim
/// about what the generated parser costs is divided by.
/// </summary>
/// <remarks>
/// <para>
/// It is here because it was not, and that cost a week's numbers. Every "so many times the
/// hand-written parser" in <c>docs/next.md</c> came from a file in a scratch directory
/// outside the repository, and the directory was cleared. A yardstick that lives where the
/// machine may delete it is a yardstick that will be gone exactly when a number is
/// questioned. This one is built by the solution, so it cannot rot silently, and
/// <see cref="Agrees"/> is run before the benchmark measures anything.
/// </para>
/// <para>
/// <b>It must accept the same language, or it is not a yardstick.</b> A hand-written
/// parser that quietly reads less is faster for a reason that has nothing to do with how
/// the generated one is built, and the ratio it produces is a lie in the flattering
/// direction. So this follows the grammar rule for rule, ordered choice included, down to
/// the shapes no benchmark input exercises: the subquery forms, every literal, the whole
/// of <c>DataType</c>. Where the grammar backtracks this backtracks, because that is the
/// work being measured.
/// </para>
/// <para>
/// What it does not do is build anything or say why it failed. The generated recognizer it
/// is compared against is asked for the same: <c>TryParseSearchCondition</c> answers
/// whether the input matched, and the comparison is of two recognizers.
/// </para>
/// <para>
/// Written as a PEG recognizer over the input span — a method per rule, a position in and
/// a position out, <c>-1</c> for no match and the caller putting its own position back.
/// That is deliberately the same shape the generated readers have, so what the ratio
/// measures is the machinery and not two different ways of writing a parser.
/// </para>
/// </remarks>
static class HandSql
{
	/// <summary>Whether the whole input is a search condition.</summary>
	public static bool Parse(string text)
	{
		var input = text.AsSpan();
		var end   = SearchCondition(input, 0);

		return end >= 0 && Trivia(input, end) == input.Length;
	}

	// ── Trivia (§4.5), and the terminals that skip it first ─────────────────────

	static int Trivia(ReadOnlySpan<char> s, int p)
	{
		while (p < s.Length)
		{
			var c = s[p];

			if (c is ' ' or '\t' or '\r' or '\n')
			{
				p++;
			}
			else if (c == '-' && p + 1 < s.Length && s[p + 1] == '-')
			{
				p += 2;

				while (p < s.Length && s[p] != '\n' && s[p] != '\r')
					p++;
			}
			else if (c == '/' && p + 1 < s.Length && s[p + 1] == '*')
			{
				p += 2;

				while (p + 1 < s.Length && !(s[p] == '*' && s[p + 1] == '/'))
					p++;

				if (p + 1 >= s.Length)
					return s.Length;

				p += 2;
			}
			else
			{
				break;
			}
		}

		return p;
	}

	/// <summary>One punctuation character, past whatever trivia stands before it.</summary>
	static int Ch(ReadOnlySpan<char> s, int p, char want)
	{
		p = Trivia(s, p);

		return p < s.Length && s[p] == want ? p + 1 : -1;
	}

	/// <summary>A keyword: the word, whole, past the trivia before it (§4.6).</summary>
	static int Kw(ReadOnlySpan<char> s, int p, string word)
	{
		p = Trivia(s, p);

		if (p + word.Length > s.Length)
			return -1;

		for (var i = 0; i < word.Length; i++)
			if (char.ToUpperInvariant(s[p + i]) != word[i])
				return -1;

		var after = p + word.Length;

		return after < s.Length && IsPart(s[after]) ? -1 : after;
	}

	/// <summary>Two punctuation characters with nothing between them.</summary>
	static int Op(ReadOnlySpan<char> s, int p, char first, char second)
	{
		p = Trivia(s, p);

		return p + 1 < s.Length && s[p] == first && s[p + 1] == second ? p + 2 : -1;
	}

	static bool IsStart(char c) => char.IsLetter(c) || c == '_';
	static bool IsPart (char c) => char.IsLetter(c) || char.IsDigit(c) || c == '_';

	// ── §8.12 Search condition ──────────────────────────────────────────────────

	static int SearchCondition(ReadOnlySpan<char> s, int p)
	{
		var at = BooleanTerm(s, p);

		if (at < 0)
			return -1;

		while (true)
		{
			var next = Kw(s, at, "OR");

			if (next < 0)
				return at;

			var right = BooleanTerm(s, next);

			if (right < 0)
				return at;

			at = right;
		}
	}

	static int BooleanTerm(ReadOnlySpan<char> s, int p)
	{
		var at = BooleanFactor(s, p);

		if (at < 0)
			return -1;

		while (true)
		{
			var next = Kw(s, at, "AND");

			if (next < 0)
				return at;

			var right = BooleanFactor(s, next);

			if (right < 0)
				return at;

			at = right;
		}
	}

	static int BooleanFactor(ReadOnlySpan<char> s, int p)
	{
		var not = Kw(s, p, "NOT");

		return BooleanTest(s, not < 0 ? p : not);
	}

	static int BooleanTest(ReadOnlySpan<char> s, int p)
	{
		var at = BooleanPrimary(s, p);

		if (at < 0)
			return -1;

		while (true)
		{
			var isAt = Kw(s, at, "IS");

			if (isAt < 0)
				return at;

			var not   = Kw(s, isAt, "NOT");
			var value = TruthValue(s, not < 0 ? isAt : not);

			if (value < 0)
				return at;

			at = value;
		}
	}

	static int TruthValue(ReadOnlySpan<char> s, int p)
	{
		var at = Kw(s, p, "TRUE");

		if (at >= 0)
			return at;

		at = Kw(s, p, "FALSE");

		return at >= 0 ? at : Kw(s, p, "UNKNOWN");
	}

	/// <remarks>
	/// A predicate first, and the parenthesized condition only where the whole predicate
	/// failed — which is ordered choice, and is why `(a + b) * c > d` is not read as a
	/// parenthesized search condition that then finds a `*` after it.
	/// </remarks>
	static int BooleanPrimary(ReadOnlySpan<char> s, int p)
	{
		var at = Predicate(s, p);

		if (at >= 0)
			return at;

		at = Ch(s, p, '(');

		if (at < 0)
			return -1;

		at = SearchCondition(s, at);

		return at < 0 ? -1 : Ch(s, at, ')');
	}

	// ── §8.1 Predicate ──────────────────────────────────────────────────────────

	static int Predicate(ReadOnlySpan<char> s, int p)
	{
		var at = Kw(s, p, "EXISTS");

		if (at >= 0 && Subquery(s, at) is var exists && exists >= 0)
			return exists;

		at = Kw(s, p, "UNIQUE");

		if (at >= 0 && Subquery(s, at) is var unique && unique >= 0)
			return unique;

		at = RowValueConstructor(s, p);

		return at < 0 ? -1 : PredicateTail(s, at);
	}

	static int PredicateTail(ReadOnlySpan<char> s, int p)
	{
		// §8.2 and §8.8 share their operator, so it is read once.
		var at = CompOp(s, p);

		if (at >= 0)
		{
			var quantified = Quantifier(s, at);

			if (quantified >= 0 && Subquery(s, quantified) is var sub && sub >= 0)
				return sub;

			var row = RowValueConstructor(s, at);

			if (row >= 0)
				return row;
		}

		var not = Kw(s, p, "NOT");
		var after = not < 0 ? p : not;

		at = Kw(s, after, "BETWEEN");

		if (at >= 0)
		{
			var low = RowValueConstructor(s, at);

			if (low >= 0 && Kw(s, low, "AND") is var and && and >= 0)
			{
				var high = RowValueConstructor(s, and);

				if (high >= 0)
					return high;
			}
		}

		at = Kw(s, after, "IN");

		if (at >= 0 && InPredicateValue(s, at) is var inValue && inValue >= 0)
			return inValue;

		at = Kw(s, after, "LIKE");

		if (at >= 0)
		{
			var pattern = ValueExpression(s, at);

			if (pattern >= 0)
			{
				var escape = Kw(s, pattern, "ESCAPE");

				if (escape < 0)
					return pattern;

				var how = ValueExpression(s, escape);

				return how < 0 ? pattern : how;
			}
		}

		at = Kw(s, p, "IS");

		if (at >= 0)
		{
			var isNot = Kw(s, at, "NOT");
			var isNull = Kw(s, isNot < 0 ? at : isNot, "NULL");

			if (isNull >= 0)
				return isNull;
		}

		at = Kw(s, p, "MATCH");

		if (at >= 0)
		{
			var unique = Kw(s, at, "UNIQUE");

			if (unique >= 0)
				at = unique;

			var partial = Kw(s, at, "PARTIAL");

			if (partial >= 0)
				at = partial;
			else if (Kw(s, at, "FULL") is var full && full >= 0)
				at = full;

			var match = Subquery(s, at);

			if (match >= 0)
				return match;
		}

		at = Kw(s, p, "OVERLAPS");

		return at < 0 ? -1 : RowValueConstructor(s, at);
	}

	static int CompOp(ReadOnlySpan<char> s, int p)
	{
		var at = Op(s, p, '<', '>');

		if (at >= 0) return at;

		at = Op(s, p, '<', '=');

		if (at >= 0) return at;

		at = Op(s, p, '>', '=');

		if (at >= 0) return at;

		at = Ch(s, p, '=');

		if (at >= 0) return at;

		at = Ch(s, p, '<');

		return at >= 0 ? at : Ch(s, p, '>');
	}

	static int Quantifier(ReadOnlySpan<char> s, int p)
	{
		var at = Kw(s, p, "ALL");

		if (at >= 0) return at;

		at = Kw(s, p, "SOME");

		return at >= 0 ? at : Kw(s, p, "ANY");
	}

	static int InPredicateValue(ReadOnlySpan<char> s, int p)
	{
		var at = Subquery(s, p);

		if (at >= 0)
			return at;

		at = Ch(s, p, '(');

		if (at < 0)
			return -1;

		at = ValueExpression(s, at);

		if (at < 0)
			return -1;

		while (Ch(s, at, ',') is var comma && comma >= 0)
		{
			var next = ValueExpression(s, comma);

			if (next < 0)
				break;

			at = next;
		}

		return Ch(s, at, ')');
	}

	// ── §7.1 Row value constructor ──────────────────────────────────────────────

	static int RowValueConstructor(ReadOnlySpan<char> s, int p)
	{
		var at = RowValueConstructorElement(s, p);

		if (at >= 0)
			return at;

		at = Ch(s, p, '(');

		if (at >= 0)
		{
			var first = RowValueConstructorElement(s, at);

			if (first >= 0)
			{
				var rest  = first;
				var count = 0;

				while (Ch(s, rest, ',') is var comma && comma >= 0)
				{
					var next = RowValueConstructorElement(s, comma);

					if (next < 0)
						break;

					rest = next;
					count++;
				}

				if (count > 0 && Ch(s, rest, ')') is var close && close >= 0)
					return close;
			}
		}

		return Subquery(s, p);
	}

	static int RowValueConstructorElement(ReadOnlySpan<char> s, int p)
	{
		var at = ValueExpression(s, p);

		if (at >= 0)
			return at;

		at = Kw(s, p, "NULL");

		return at >= 0 ? at : Kw(s, p, "DEFAULT");
	}

	// ── §6.11 Value expression ──────────────────────────────────────────────────

	static int ValueExpression(ReadOnlySpan<char> s, int p)
	{
		var at = Term(s, p);

		if (at < 0)
			return -1;

		while (true)
		{
			var next = Op(s, at, '|', '|');

			if (next < 0)
			{
				next = Ch(s, at, '+');

				if (next < 0)
					next = Ch(s, at, '-');
			}

			if (next < 0)
				return at;

			var right = Term(s, next);

			if (right < 0)
				return at;

			at = right;
		}
	}

	static int Term(ReadOnlySpan<char> s, int p)
	{
		var at = Factor(s, p);

		if (at < 0)
			return -1;

		while (true)
		{
			var next = Ch(s, at, '*');

			if (next < 0)
				next = Ch(s, at, '/');

			if (next < 0)
				return at;

			var right = Factor(s, next);

			if (right < 0)
				return at;

			at = right;
		}
	}

	static int Factor(ReadOnlySpan<char> s, int p)
	{
		var sign = Ch(s, p, '+');

		if (sign < 0)
			sign = Ch(s, p, '-');

		return ValueExpressionPrimary(s, sign < 0 ? p : sign);
	}

	/// <remarks>
	/// <para>
	/// The one place a hand-written parser earns its keep, and the reason this file is a
	/// yardstick rather than a second implementation of the same idea. The grammar writes
	/// eight alternatives and ordered choice walks them; a person looks at the character
	/// standing here, and where it is a letter, reads the word once and switches on it.
	/// </para>
	/// <para>
	/// Written the other way — eight speculative attempts per operand — this was three to
	/// eight times slower than the generated parser, which is the measurement that says
	/// what the generator's first-set gates are worth.
	/// </para>
	/// </remarks>
	static int ValueExpressionPrimary(ReadOnlySpan<char> s, int p)
	{
		p = Trivia(s, p);

		if (p >= s.Length)
			return -1;

		var c = s[p];

		if (c == '(')
		{
			var sub = Subquery(s, p);

			if (sub >= 0)
				return sub;

			var inner = ValueExpression(s, p + 1);

			return inner < 0 ? -1 : Ch(s, inner, ')');
		}

		if (c is >= '0' and <= '9' or '.')
			return NumericLiteral(s, p);

		if (c == '\'')
			return Quoted(s, p);

		if (c is ':' or '?')
			return GeneralValueSpecification(s, p);

		if (c == '"' || c == '_')
			return c == '_' ? Introduced(s, p) : QualifiedName(s, p);

		if (!IsStart(c))
			return -1;

		var end = p + 1;

		while (end < s.Length && IsPart(s[end]))
			end++;

		var word = s.Slice(p, end - p);

		// A one-letter prefix in front of a quoted string is a literal of that kind, and
		// is not a name: `N'x'`, `B'01'`, `X'ff'`.
		if (end - p == 1 && end < s.Length && s[end] == '\'')
		{
			var letter = char.ToUpperInvariant(c);

			if (letter == 'N') return Quoted(s, end);
			if (letter == 'B') return BitString(s, p, 'B', binary: true);
			if (letter == 'X') return BitString(s, p, 'X', binary: false);
		}

		switch (char.ToUpperInvariant(c))
		{
			case 'A' when Is(word, "AVG"):      return SetFunction(s, p);
			case 'B' when Is(word, "BIT_LENGTH"): return OneArgument(s, p, "BIT_LENGTH");
			case 'C':
				if (Is(word, "CASE") || Is(word, "COALESCE"))          return CaseExpression(s, p);
				if (Is(word, "CAST"))                                  return CastSpecification(s, p);
				if (Is(word, "COUNT"))                                 return SetFunction(s, p);
				if (Is(word, "CHAR_LENGTH") || Is(word, "CHARACTER_LENGTH"))
					return OneArgument(s, p, Is(word, "CHAR_LENGTH") ? "CHAR_LENGTH" : "CHARACTER_LENGTH");
				if (Is(word, "CONVERT"))                               return Using(s, p, "CONVERT");
				if (Is(word, "CURRENT_DATE"))                          return end;
				if (Is(word, "CURRENT_TIME") || Is(word, "CURRENT_TIMESTAMP"))
					return Precision(s, end) is var given && given >= 0 ? given : end;
				if (Is(word, "CURRENT_USER"))                          return end;
				break;
			case 'D' when Is(word, "DATE"):     return Dated(s, p, "DATE");
			case 'E' when Is(word, "EXTRACT"):  return ValueFunction(s, p);
			case 'I' when Is(word, "INTERVAL"): return IntervalLiteral(s, p);
			case 'L' when Is(word, "LOWER"):    return OneArgument(s, p, "LOWER");
			case 'M':
				if (Is(word, "MAX") || Is(word, "MIN"))                return SetFunction(s, p);
				break;
			case 'N':
				if (Is(word, "NULLIF"))                                return CaseExpression(s, p);
				break;
			case 'O' when Is(word, "OCTET_LENGTH"): return OneArgument(s, p, "OCTET_LENGTH");
			case 'P' when Is(word, "POSITION"): return ValueFunction(s, p);
			case 'S':
				if (Is(word, "SUM"))                                   return SetFunction(s, p);
				if (Is(word, "SUBSTRING"))                             return Substring(s, p);
				if (Is(word, "SESSION_USER") || Is(word, "SYSTEM_USER")) return end;
				break;
			case 'T':
				if (Is(word, "TIME") || Is(word, "TIMESTAMP"))
					return Dated(s, p, Is(word, "TIME") ? "TIME" : "TIMESTAMP");
				if (Is(word, "TRIM"))                                  return Trim(s, p);
				if (Is(word, "TRANSLATE"))                             return Using(s, p, "TRANSLATE");
				break;
			case 'U':
				if (Is(word, "UPPER"))                                 return OneArgument(s, p, "UPPER");
				if (Is(word, "USER"))                                  return end;
				break;
			case 'V' when Is(word, "VALUE"):    return end;
		}

		// Anything else that is a reserved word is not a name, and nothing here reads it.
		return IsReserved(word) ? -1 : QualifiedName(s, p);
	}

	static bool Is(ReadOnlySpan<char> word, string one) =>
		word.Equals(one.AsSpan(), StringComparison.OrdinalIgnoreCase);

	/// <summary><c>NAME '(' ValueExpression ')'</c>, the shape six of the value functions share.</summary>
	static int OneArgument(ReadOnlySpan<char> s, int p, string name)
	{
		var at = Kw(s, p, name);

		if (at < 0 || Ch(s, at, '(') is var open && open < 0)
			return -1;

		var value = ValueExpression(s, open);

		return value < 0 ? -1 : Ch(s, value, ')');
	}

	/// <summary><c>COUNT(*)</c> and the aggregates, once the word is known to be one.</summary>
	static int SetFunction(ReadOnlySpan<char> s, int p) => SetFunctionSpecification(s, p);

	// ── §6.9 Set function, §6.16-6.18 value functions ───────────────────────────

	static int SetFunctionSpecification(ReadOnlySpan<char> s, int p)
	{
		var at = Kw(s, p, "COUNT");

		if (at >= 0 &&
			Ch(s, at, '(') is var open && open >= 0 &&
			Ch(s, open, '*') is var star && star >= 0 &&
			Ch(s, star, ')') is var shut && shut >= 0)
		{
			return shut;
		}

		at = SetFunctionType(s, p);

		if (at < 0)
			return -1;

		at = Ch(s, at, '(');

		if (at < 0)
			return -1;

		var quantifier = Kw(s, at, "DISTINCT");

		if (quantifier < 0)
			quantifier = Kw(s, at, "ALL");

		var value = ValueExpression(s, quantifier < 0 ? at : quantifier);

		return value < 0 ? -1 : Ch(s, value, ')');
	}

	static int SetFunctionType(ReadOnlySpan<char> s, int p)
	{
		foreach (var word in new[] { "AVG", "MAX", "MIN", "SUM", "COUNT" })
		{
			var at = Kw(s, p, word);

			if (at >= 0)
				return at;
		}

		return -1;
	}

	static int ValueFunction(ReadOnlySpan<char> s, int p)
	{
		var at = Kw(s, p, "POSITION");

		if (at >= 0 &&
			Ch(s, at, '(') is var open && open >= 0 &&
			ValueExpression(s, open) is var what && what >= 0 &&
			Kw(s, what, "IN") is var inAt && inAt >= 0 &&
			ValueExpression(s, inAt) is var where && where >= 0 &&
			Ch(s, where, ')') is var shut && shut >= 0)
		{
			return shut;
		}

		at = Kw(s, p, "EXTRACT");

		if (at >= 0 &&
			Ch(s, at, '(') is var opened && opened >= 0 &&
			ExtractField(s, opened) is var field && field >= 0 &&
			Kw(s, field, "FROM") is var from && from >= 0 &&
			ValueExpression(s, from) is var source && source >= 0 &&
			Ch(s, source, ')') is var closed && closed >= 0)
		{
			return closed;
		}

		foreach (var word in new[] { "CHAR_LENGTH", "CHARACTER_LENGTH", "OCTET_LENGTH", "BIT_LENGTH", "UPPER", "LOWER" })
		{
			at = Kw(s, p, word);

			if (at >= 0 &&
				Ch(s, at, '(') is var one && one >= 0 &&
				ValueExpression(s, one) is var only && only >= 0 &&
				Ch(s, only, ')') is var done && done >= 0)
			{
				return done;
			}
		}

		at = Substring(s, p);

		if (at >= 0) return at;

		at = Using(s, p, "CONVERT");

		if (at >= 0) return at;

		at = Using(s, p, "TRANSLATE");

		if (at >= 0) return at;

		at = Trim(s, p);

		if (at >= 0) return at;

		at = Kw(s, p, "CURRENT_DATE");

		if (at >= 0) return at;

		at = Kw(s, p, "CURRENT_TIME");

		if (at < 0)
			at = Kw(s, p, "CURRENT_TIMESTAMP");

		if (at < 0)
			return -1;

		var precision = Precision(s, at);

		return precision < 0 ? at : precision;
	}

	static int Substring(ReadOnlySpan<char> s, int p)
	{
		var at = Kw(s, p, "SUBSTRING");

		if (at < 0 ||
			Ch(s, at, '(') is var open && open < 0 ||
			ValueExpression(s, open) is var source && source < 0 ||
			Kw(s, source, "FROM") is var from && from < 0 ||
			ValueExpression(s, from) is var start && start < 0)
		{
			return -1;
		}

		var forAt = Kw(s, start, "FOR");

		if (forAt >= 0 && ValueExpression(s, forAt) is var length && length >= 0)
			start = length;

		return Ch(s, start, ')');
	}

	static int Using(ReadOnlySpan<char> s, int p, string name)
	{
		var at = Kw(s, p, name);

		if (at < 0 ||
			Ch(s, at, '(') is var open && open < 0 ||
			ValueExpression(s, open) is var value && value < 0 ||
			Kw(s, value, "USING") is var using_ && using_ < 0 ||
			QualifiedName(s, using_) is var name_ && name_ < 0)
		{
			return -1;
		}

		return Ch(s, name_, ')');
	}

	static int Trim(ReadOnlySpan<char> s, int p)
	{
		var at = Kw(s, p, "TRIM");

		if (at < 0)
			return -1;

		var open = Ch(s, at, '(');

		if (open < 0)
			return -1;

		// The longer form first, which is what ordered choice says.
		var how = TrimSpecification(s, open);
		var source = ValueExpression(s, how < 0 ? open : how);
		var from = Kw(s, source < 0 ? (how < 0 ? open : how) : source, "FROM");

		if (from >= 0 && ValueExpression(s, from) is var target && target >= 0 &&
			Ch(s, target, ')') is var shut && shut >= 0)
		{
			return shut;
		}

		var only = ValueExpression(s, open);

		return only < 0 ? -1 : Ch(s, only, ')');
	}

	static int TrimSpecification(ReadOnlySpan<char> s, int p)
	{
		var at = Kw(s, p, "LEADING");

		if (at >= 0) return at;

		at = Kw(s, p, "TRAILING");

		return at >= 0 ? at : Kw(s, p, "BOTH");
	}

	static int ExtractField(ReadOnlySpan<char> s, int p)
	{
		foreach (var word in new[]
		{
			"YEAR", "MONTH", "DAY", "HOUR", "MINUTE", "SECOND", "TIMEZONE_HOUR", "TIMEZONE_MINUTE",
		})
		{
			var at = Kw(s, p, word);

			if (at >= 0)
				return at;
		}

		return -1;
	}

	// ── §6.9 Case, §6.10 Cast ───────────────────────────────────────────────────

	static int CaseExpression(ReadOnlySpan<char> s, int p)
	{
		var at = Kw(s, p, "NULLIF");

		if (at >= 0 &&
			Ch(s, at, '(') is var open && open >= 0 &&
			ValueExpression(s, open) is var left && left >= 0 &&
			Ch(s, left, ',') is var comma && comma >= 0 &&
			ValueExpression(s, comma) is var right && right >= 0 &&
			Ch(s, right, ')') is var shut && shut >= 0)
		{
			return shut;
		}

		at = Kw(s, p, "COALESCE");

		if (at >= 0 && Ch(s, at, '(') is var opened && opened >= 0)
		{
			var one = ValueExpression(s, opened);

			if (one >= 0)
			{
				while (Ch(s, one, ',') is var next && next >= 0)
				{
					var more = ValueExpression(s, next);

					if (more < 0)
						break;

					one = more;
				}

				if (Ch(s, one, ')') is var closed && closed >= 0)
					return closed;
			}
		}

		at = Kw(s, p, "CASE");

		if (at < 0)
			return -1;

		// The simple form first: `CASE x WHEN ...`, where the searched form has no
		// operand between `CASE` and its first `WHEN`.
		var operand = ValueExpression(s, at);

		if (operand >= 0)
		{
			var simple = Whens(s, operand, searched: false);

			if (simple >= 0)
				return simple;
		}

		return Whens(s, at, searched: true);
	}

	static int Whens(ReadOnlySpan<char> s, int p, bool searched)
	{
		var at    = p;
		var count = 0;

		while (Kw(s, at, "WHEN") is var when && when >= 0)
		{
			var test = searched ? SearchCondition(s, when) : ValueExpression(s, when);

			if (test < 0 ||
				Kw(s, test, "THEN") is var then && then < 0 ||
				Result(s, then) is var result && result < 0)
			{
				break;
			}

			at = result;
			count++;
		}

		if (count == 0)
			return -1;

		var elseAt = Kw(s, at, "ELSE");

		if (elseAt >= 0 && Result(s, elseAt) is var otherwise && otherwise >= 0)
			at = otherwise;

		return Kw(s, at, "END");
	}

	static int Result(ReadOnlySpan<char> s, int p)
	{
		var at = ValueExpression(s, p);

		return at >= 0 ? at : Kw(s, p, "NULL");
	}

	static int CastSpecification(ReadOnlySpan<char> s, int p)
	{
		var at = Kw(s, p, "CAST");

		if (at < 0 || Ch(s, at, '(') is var open && open < 0)
			return -1;

		var operand = ValueExpression(s, open);

		if (operand < 0)
			operand = Kw(s, open, "NULL");

		if (operand < 0 ||
			Kw(s, operand, "AS") is var asAt && asAt < 0 ||
			DataType(s, asAt) is var type && type < 0)
		{
			return -1;
		}

		return Ch(s, type, ')');
	}

	// ── §6.1 Data type ──────────────────────────────────────────────────────────

	static int DataType(ReadOnlySpan<char> s, int p)
	{
		var at = Character(s, p);

		if (at >= 0) return at;

		at = National(s, p);

		if (at >= 0) return at;

		at = Kw(s, p, "BIT");

		if (at >= 0)
		{
			var varying = Kw(s, at, "VARYING");

			if (varying >= 0)
				at = varying;

			var length = Length(s, at);

			return length < 0 ? at : length;
		}

		at = Kw(s, p, "NUMERIC");

		if (at < 0) at = Kw(s, p, "DECIMAL");
		if (at < 0) at = Kw(s, p, "DEC");

		if (at >= 0)
		{
			var scale = Scale(s, at);

			return scale < 0 ? at : scale;
		}

		foreach (var word in new[] { "INTEGER", "INT", "SMALLINT", "REAL", "DATE" })
		{
			at = Kw(s, p, word);

			if (at >= 0)
				return at;
		}

		at = Kw(s, p, "FLOAT");

		if (at >= 0)
		{
			var length = Length(s, at);

			return length < 0 ? at : length;
		}

		at = Kw(s, p, "DOUBLE");

		if (at >= 0 && Kw(s, at, "PRECISION") is var precision && precision >= 0)
			return precision;

		at = Kw(s, p, "TIME");

		if (at < 0)
			at = Kw(s, p, "TIMESTAMP");

		if (at >= 0)
		{
			var length = Length(s, at);

			if (length >= 0)
				at = length;

			if (Kw(s, at, "WITH") is var with && with >= 0 &&
				Kw(s, with, "TIME") is var time && time >= 0 &&
				Kw(s, time, "ZONE") is var zone && zone >= 0)
			{
				at = zone;
			}

			return at;
		}

		at = Kw(s, p, "INTERVAL");

		return at < 0 ? -1 : IntervalQualifier(s, at);
	}

	static int Character(ReadOnlySpan<char> s, int p)
	{
		var at = Kw(s, p, "VARCHAR");

		if (at < 0)
		{
			at = Kw(s, p, "CHARACTER");

			if (at < 0)
				at = Kw(s, p, "CHAR");

			if (at < 0)
				return -1;

			var varying = Kw(s, at, "VARYING");

			if (varying >= 0)
				at = varying;
		}

		var length = Length(s, at);

		if (length >= 0)
			at = length;

		if (Kw(s, at, "CHARACTER") is var set && set >= 0 &&
			Kw(s, set, "SET") is var setAt && setAt >= 0 &&
			QualifiedName(s, setAt) is var name && name >= 0)
		{
			at = name;
		}

		return at;
	}

	static int National(ReadOnlySpan<char> s, int p)
	{
		var at = Kw(s, p, "NCHAR");

		if (at < 0)
		{
			at = Kw(s, p, "NATIONAL");

			if (at < 0)
				return -1;

			var kind = Kw(s, at, "CHARACTER");

			if (kind < 0)
				kind = Kw(s, at, "CHAR");

			if (kind < 0)
				return -1;

			at = kind;
		}

		var varying = Kw(s, at, "VARYING");

		if (varying >= 0)
			at = varying;

		var length = Length(s, at);

		return length < 0 ? at : length;
	}

	static int Length(ReadOnlySpan<char> s, int p)
	{
		var at = Ch(s, p, '(');

		if (at < 0)
			return -1;

		at = Digits(s, at);

		return at < 0 ? -1 : Ch(s, at, ')');
	}

	static int Scale(ReadOnlySpan<char> s, int p)
	{
		var at = Ch(s, p, '(');

		if (at < 0)
			return -1;

		at = Digits(s, at);

		if (at < 0)
			return -1;

		var comma = Ch(s, at, ',');

		if (comma >= 0 && Digits(s, comma) is var second && second >= 0)
			at = second;

		return Ch(s, at, ')');
	}

	static int Precision(ReadOnlySpan<char> s, int p) => Length(s, p);

	static int IntervalQualifier(ReadOnlySpan<char> s, int p)
	{
		var at = SingleDatetimeField(s, p);

		if (at < 0)
			return -1;

		var to = Kw(s, at, "TO");

		if (to >= 0 && SingleDatetimeField(s, to) is var second && second >= 0)
			at = second;

		return at;
	}

	static int SingleDatetimeField(ReadOnlySpan<char> s, int p)
	{
		var at = ExtractField(s, p);

		if (at < 0)
			return -1;

		var scale = Scale(s, at);

		return scale < 0 ? at : scale;
	}

	// ── §5.3 Literals and §6.3 value specifications ─────────────────────────────

	static int UnsignedValueSpecification(ReadOnlySpan<char> s, int p)
	{
		var at = UnsignedLiteral(s, p);

		return at >= 0 ? at : GeneralValueSpecification(s, p);
	}

	static int UnsignedLiteral(ReadOnlySpan<char> s, int p)
	{
		var at = NumericLiteral(s, p);

		if (at >= 0) return at;

		at = Introduced(s, p);

		if (at >= 0) return at;

		at = Prefixed(s, p, 'N');

		if (at >= 0) return at;

		at = BitString(s, p, 'B', binary: true);

		if (at >= 0) return at;

		at = BitString(s, p, 'X', binary: false);

		if (at >= 0) return at;

		at = Dated(s, p, "DATE");

		if (at >= 0) return at;

		at = Dated(s, p, "TIME");

		if (at >= 0) return at;

		at = Dated(s, p, "TIMESTAMP");

		if (at >= 0) return at;

		return IntervalLiteral(s, p);
	}

	/// <remarks>Approximate first: `1E5` is not `1` followed by a name (§5.3).</remarks>
	static int NumericLiteral(ReadOnlySpan<char> s, int p)
	{
		var exact = ExactNumeric(s, p);

		if (exact < 0)
			return -1;

		if (exact < s.Length && (s[exact] == 'e' || s[exact] == 'E'))
		{
			var at = exact + 1;

			if (at < s.Length && (s[at] == '+' || s[at] == '-'))
				at++;

			var digits = Run(s, at);

			if (digits > at)
				return digits;
		}

		return exact;
	}

	static int ExactNumeric(ReadOnlySpan<char> s, int p)
	{
		p = Trivia(s, p);

		var at = Run(s, p);

		if (at > p)
		{
			if (at < s.Length && s[at] == '.')
			{
				var after = Run(s, at + 1);

				return after > at + 1 ? after : at + 1;
			}

			return at;
		}

		if (p < s.Length && s[p] == '.')
		{
			var after = Run(s, p + 1);

			return after > p + 1 ? after : -1;
		}

		return -1;
	}

	static int Digits(ReadOnlySpan<char> s, int p)
	{
		p = Trivia(s, p);

		var at = Run(s, p);

		return at > p ? at : -1;
	}

	static int Run(ReadOnlySpan<char> s, int p)
	{
		while (p < s.Length && s[p] >= '0' && s[p] <= '9')
			p++;

		return p;
	}

	/// <summary><c>_charset'text'</c>, and the bare quoted string it falls back to.</summary>
	static int Introduced(ReadOnlySpan<char> s, int p)
	{
		var at = Trivia(s, p);

		if (at < s.Length && s[at] == '_')
		{
			var name = at + 1;

			if (name < s.Length && IsStart(s[name]))
			{
				while (name < s.Length && IsPart(s[name]))
					name++;

				var quoted = Quoted(s, name);

				if (quoted >= 0)
					return quoted;
			}
		}

		return Quoted(s, at);
	}

	static int Prefixed(ReadOnlySpan<char> s, int p, char letter)
	{
		var at = Trivia(s, p);

		return at < s.Length && char.ToUpperInvariant(s[at]) == letter ? Quoted(s, at + 1) : -1;
	}

	static int BitString(ReadOnlySpan<char> s, int p, char letter, bool binary)
	{
		var at = Trivia(s, p);

		if (at >= s.Length || char.ToUpperInvariant(s[at]) != letter)
			return -1;

		at++;

		if (at >= s.Length || s[at] != '\'')
			return -1;

		at++;

		while (at < s.Length && s[at] != '\'')
		{
			var c = s[at];
			var ok = binary
				? c is '0' or '1'
				: c is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F';

			if (!ok)
				return -1;

			at++;
		}

		return at < s.Length ? at + 1 : -1;
	}

	static int Dated(ReadOnlySpan<char> s, int p, string word)
	{
		var at = Kw(s, p, word);

		if (at < 0 || at >= s.Length || !(s[at] is ' ' or '\t' or '\r' or '\n'))
			return -1;

		while (at < s.Length && s[at] is ' ' or '\t' or '\r' or '\n')
			at++;

		return Quoted(s, at);
	}

	static int IntervalLiteral(ReadOnlySpan<char> s, int p)
	{
		var at = Kw(s, p, "INTERVAL");

		if (at < 0)
			return -1;

		var sign = Trivia(s, at);

		if (sign < s.Length && (s[sign] == '+' || s[sign] == '-'))
			sign++;

		var quoted = Quoted(s, sign);

		return quoted < 0 ? -1 : IntervalQualifier(s, quoted);
	}

	/// <summary><c>'text'</c>, with a doubled quote standing for one (§5.3).</summary>
	static int Quoted(ReadOnlySpan<char> s, int p)
	{
		p = Trivia(s, p);

		if (p >= s.Length || s[p] != '\'')
			return -1;

		p++;

		while (p < s.Length)
		{
			if (s[p] != '\'')
			{
				p++;

				continue;
			}

			if (p + 1 < s.Length && s[p + 1] == '\'')
			{
				p += 2;

				continue;
			}

			return p + 1;
		}

		return -1;
	}

	static int GeneralValueSpecification(ReadOnlySpan<char> s, int p)
	{
		var at = Ch(s, p, ':');

		if (at >= 0)
		{
			var name = Identifier(s, at);

			if (name < 0)
				return -1;

			var indicator = Kw(s, name, "INDICATOR");
			var colon     = Ch(s, indicator < 0 ? name : indicator, ':');

			if (colon >= 0 && Identifier(s, colon) is var second && second >= 0)
				return second;

			return name;
		}

		at = Ch(s, p, '?');

		if (at >= 0)
			return at;

		foreach (var word in new[] { "CURRENT_USER", "SESSION_USER", "SYSTEM_USER", "USER", "VALUE" })
		{
			at = Kw(s, p, word);

			if (at >= 0)
				return at;
		}

		return -1;
	}

	// ── §6.4 Column reference, §5.2 reserved words ──────────────────────────────

	static int QualifiedName(ReadOnlySpan<char> s, int p)
	{
		var at = Identifier(s, p);

		if (at < 0)
			return -1;

		while (Ch(s, at, '.') is var dot && dot >= 0)
		{
			var next = Identifier(s, dot);

			if (next < 0)
				break;

			at = next;
		}

		return at;
	}

	static int Identifier(ReadOnlySpan<char> s, int p)
	{
		p = Trivia(s, p);

		if (p < s.Length && s[p] == '"')
		{
			var at = p + 1;

			while (at < s.Length)
			{
				if (s[at] != '"')
				{
					at++;

					continue;
				}

				if (at + 1 < s.Length && s[at + 1] == '"')
				{
					at += 2;

					continue;
				}

				return at + 1;
			}

			return -1;
		}

		if (p >= s.Length || !IsStart(s[p]))
			return -1;

		var end = p + 1;

		while (end < s.Length && IsPart(s[end]))
			end++;

		return IsReserved(s.Slice(p, end - p)) ? -1 : end;
	}

	/// <summary>
	/// What an identifier may not be (§5.2), grouped by first letter so a word is compared
	/// against the handful that could match it rather than against all seventy-five.
	/// </summary>
	static bool IsReserved(ReadOnlySpan<char> word) =>
		char.ToUpperInvariant(word[0]) switch
		{
			'A' => Any(word, "ALL", "AND", "ANY", "AS", "AVG"),
			'B' => Any(word, "BETWEEN", "BIT_LENGTH", "BOTH", "BY"),
			'C' => Any(word, "CASE", "CAST", "CHARACTER_LENGTH", "CHAR_LENGTH", "COALESCE",
			                 "CONVERT", "COUNT", "CROSS", "CURRENT_DATE", "CURRENT_TIME",
			                 "CURRENT_TIMESTAMP", "CURRENT_USER"),
			'D' => Any(word, "DEFAULT", "DISTINCT"),
			'E' => Any(word, "ELSE", "END", "ESCAPE", "EXISTS", "EXTRACT"),
			'F' => Any(word, "FALSE", "FOR", "FROM", "FULL"),
			'G' => Any(word, "GROUP"),
			'H' => Any(word, "HAVING"),
			'I' => Any(word, "IN", "INTERVAL", "IS"),
			'J' => Any(word, "JOIN"),
			'L' => Any(word, "LEADING", "LIKE", "LOWER"),
			'M' => Any(word, "MATCH", "MAX", "MIN"),
			'N' => Any(word, "NOT", "NULL", "NULLIF"),
			'O' => Any(word, "OCTET_LENGTH", "ON", "OR", "ORDER", "OVERLAPS"),
			'P' => Any(word, "PARTIAL", "POSITION"),
			'S' => Any(word, "SELECT", "SESSION_USER", "SOME", "SUBSTRING", "SUM", "SYSTEM_USER"),
			'T' => Any(word, "TABLE", "THEN", "TRAILING", "TRANSLATE", "TRIM", "TRUE"),
			'U' => Any(word, "UNIQUE", "UNKNOWN", "UPPER", "USER", "USING"),
			'V' => Any(word, "VALUE", "VALUES"),
			'W' => Any(word, "WHEN", "WHERE"),
			_   => false,
		};

	static bool Any(ReadOnlySpan<char> word, params string[] words)
	{
		foreach (var one in words)
			if (word.Equals(one.AsSpan(), StringComparison.OrdinalIgnoreCase))
				return true;

		return false;
	}

	// ── The seam where the query level would go ─────────────────────────────────

	/// <summary>
	/// The same stub the grammar has: anything balanced that opens with one of the three
	/// words a query begins with.
	/// </summary>
	static int Subquery(ReadOnlySpan<char> s, int p)
	{
		var at = Ch(s, p, '(');

		if (at < 0)
			return -1;

		var opening = Kw(s, at, "SELECT");

		if (opening < 0) opening = Kw(s, at, "VALUES");
		if (opening < 0) opening = Kw(s, at, "TABLE");

		if (opening < 0)
			return -1;

		var depth = 1;

		for (at = opening; at < s.Length; at++)
		{
			if (s[at] == '(')
				depth++;
			else if (s[at] == ')' && --depth == 0)
				return at + 1;
		}

		return -1;
	}
}
