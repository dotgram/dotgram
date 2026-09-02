using System;
using System.Runtime.CompilerServices;

namespace DotGram.Benchmarks;

/// <summary>
/// The hand-written parser the first day's numbers were divided by, recovered verbatim.
/// </summary>
/// <remarks>
/// <para>
/// Written on 2026-09-01 into a scratch directory outside the repository, used for every
/// "so many times the hand-written parser" in <c>docs/next.md</c>, and lost when the
/// directory was cleared. Recovered from the session transcript on 2026-09-03: the body
/// below is byte for byte what ran, and the only edits are the namespace and the name.
/// </para>
/// <para>
/// <b>It reads a fraction of the language, and says so</b> — its own first comment ends
/// "Only what the benchmark inputs need". Whitespace is the whole of its trivia, so a
/// comment stops it; ten reserved words where the grammar has seventy-five; a predicate
/// tail is a comparison or <c>IS [NOT] NULL</c> and nothing else — no <c>BETWEEN</c>,
/// <c>IN</c>, <c>LIKE</c>; a primary is a number, a name, a quoted string or a
/// parenthesis — no <c>CASE</c>, <c>CAST</c>, function, literal of any other kind, or
/// subquery; a number may be <c>1.2.3</c>; a quoted string cannot double its quote. It
/// was checked against the generated parser on the seven benchmark inputs and on nothing
/// else, which is why <c>SqlAgainst</c> holds it to those seven and prints, rather than
/// throws on, where it parts from the other three over the corpus.
/// </para>
/// <para>
/// Kept because a number that was quoted deserves the program that produced it beside
/// the ones that replaced it, so the reader can see what the old ratio was made of. It is
/// not the yardstick: <see cref="HandSqlTokens"/> is.
/// </para>
/// </remarks>
static class HandSqlOriginal
{
	static ReadOnlySpan<char> T => default;

	public static bool Parse(string s)
	{
		var text = s.AsSpan();
		var p = SearchCondition(text, Ws(text, 0));
		return p >= 0 && Ws(text, p) == text.Length;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static int Ws(ReadOnlySpan<char> t, int p) { while ((uint)p < (uint)t.Length && (t[p] == ' ' || t[p] == '\t' || t[p] == '\n' || t[p] == '\r')) p++; return p; }

	static bool Kw(ReadOnlySpan<char> t, ref int p, string kw)
	{
		var q = Ws(t, p);
		if (t.Length - q < kw.Length || !t.Slice(q, kw.Length).Equals(kw, StringComparison.OrdinalIgnoreCase)) return false;
		var e = q + kw.Length;
		if ((uint)e < (uint)t.Length && (char.IsLetterOrDigit(t[e]) || t[e] == '_')) return false;
		p = e; return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static bool Ch(ReadOnlySpan<char> t, ref int p, char c) { var q = Ws(t, p); if ((uint)q < (uint)t.Length && t[q] == c) { p = q + 1; return true; } return false; }

	static int SearchCondition(ReadOnlySpan<char> t, int p)
	{
		p = BooleanTerm(t, p); if (p < 0) return -1;
		while (true) { var q = p; if (!Kw(t, ref q, "OR")) return p; var r = BooleanTerm(t, q); if (r < 0) return p; p = r; }
	}
	static int BooleanTerm(ReadOnlySpan<char> t, int p)
	{
		p = BooleanFactor(t, p); if (p < 0) return -1;
		while (true) { var q = p; if (!Kw(t, ref q, "AND")) return p; var r = BooleanFactor(t, q); if (r < 0) return p; p = r; }
	}
	static int BooleanFactor(ReadOnlySpan<char> t, int p) { Kw(t, ref p, "NOT"); return BooleanTest(t, p); }
	static int BooleanTest(ReadOnlySpan<char> t, int p)
	{
		p = BooleanPrimary(t, p); if (p < 0) return -1;
		while (true) { var q = p; if (!Kw(t, ref q, "IS")) return p; Kw(t, ref q, "NOT"); if (!(Kw(t, ref q, "TRUE") || Kw(t, ref q, "FALSE") || Kw(t, ref q, "UNKNOWN"))) return p; p = q; }
	}
	static int BooleanPrimary(ReadOnlySpan<char> t, int p)
	{
		var r = Predicate(t, p); if (r >= 0) return r;
		var q = p; if (!Ch(t, ref q, '(')) return -1;
		q = SearchCondition(t, q); if (q < 0) return -1;
		return Ch(t, ref q, ')') ? q : -1;
	}
	static int Predicate(ReadOnlySpan<char> t, int p)
	{
		p = RowValueConstructor(t, p); if (p < 0) return -1;
		return PredicateTail(t, p);
	}
	static int PredicateTail(ReadOnlySpan<char> t, int p)
	{
		var q = p;
		if (CompOp(t, ref q)) { var r = RowValueConstructor(t, q); if (r >= 0) return r; }
		q = p;
		if (Kw(t, ref q, "IS")) { Kw(t, ref q, "NOT"); if (Kw(t, ref q, "NULL")) return q; }
		return -1;
	}
	static bool CompOp(ReadOnlySpan<char> t, ref int p)
	{
		var q = Ws(t, p);
		if ((uint)q >= (uint)t.Length) return false;
		var c = t[q];
		if (c == '=') { p = q + 1; return true; }
		if (c == '<') { p = q + 1; if ((uint)p < (uint)t.Length && (t[p] == '>' || t[p] == '=')) p++; return true; }
		if (c == '>') { p = q + 1; if ((uint)p < (uint)t.Length && t[p] == '=') p++; return true; }
		return false;
	}
	static int RowValueConstructor(ReadOnlySpan<char> t, int p)
	{
		// '(' element (',' element)+ ')'  |  element   (subquery omitted)
		var q = p;
		if (Ch(t, ref q, '('))
		{
			var r = RowValueConstructorElement(t, q);
			if (r >= 0) { var n = 0; while (true) { var s = r; if (!Ch(t, ref s, ',')) break; s = RowValueConstructorElement(t, s); if (s < 0) break; r = s; n++; } if (n > 0 && Ch(t, ref r, ')')) return r; }
		}
		return RowValueConstructorElement(t, p);
	}
	static int RowValueConstructorElement(ReadOnlySpan<char> t, int p) { var q = p; if (Kw(t, ref q, "NULL")) return q; return ValueExpression(t, p); }
	static int ValueExpression(ReadOnlySpan<char> t, int p)
	{
		p = Term(t, p); if (p < 0) return -1;
		while (true) { var q = p; if (!(Ch(t, ref q, '+') || Ch(t, ref q, '-'))) return p; var r = Term(t, q); if (r < 0) return p; p = r; }
	}
	static int Term(ReadOnlySpan<char> t, int p)
	{
		p = Factor(t, p); if (p < 0) return -1;
		while (true) { var q = p; if (!(Ch(t, ref q, '*') || Ch(t, ref q, '/'))) return p; var r = Factor(t, q); if (r < 0) return p; p = r; }
	}
	static int Factor(ReadOnlySpan<char> t, int p) { var q = p; if (!Ch(t, ref q, '+')) Ch(t, ref q, '-'); return NumericPrimary(t, q); }
	static int NumericPrimary(ReadOnlySpan<char> t, int p) => ValueExpressionPrimary(t, p);
	static int ValueExpressionPrimary(ReadOnlySpan<char> t, int p)
	{
		var q = Ws(t, p);
		if ((uint)q >= (uint)t.Length) return -1;
		var c = t[q];
		if (c >= '0' && c <= '9') { while ((uint)q < (uint)t.Length && (t[q] >= '0' && t[q] <= '9' || t[q] == '.')) q++; return q; }
		if (char.IsLetter(c) || c == '_') { var e = q; while ((uint)e < (uint)t.Length && (char.IsLetterOrDigit(t[e]) || t[e] == '_')) e++; if (IsReserved(t.Slice(q, e - q))) return -1; while (true) { var d = e; if (!Ch(t, ref d, '.')) break; d = Ws(t, d); var f = d; while ((uint)f < (uint)t.Length && (char.IsLetterOrDigit(t[f]) || t[f] == '_')) f++; if (f == d) break; e = f; } return e; }
		if (c == '\'') { q++; while ((uint)q < (uint)t.Length && t[q] != '\'') q++; return (uint)q < (uint)t.Length ? q + 1 : -1; }
		if (c == '(') { q++; var r = ValueExpression(t, q); if (r < 0) return -1; return Ch(t, ref r, ')') ? r : -1; }
		return -1;
	}
	static bool IsReserved(ReadOnlySpan<char> w) =>
		w.Equals("AND", StringComparison.OrdinalIgnoreCase) || w.Equals("OR", StringComparison.OrdinalIgnoreCase) || w.Equals("NOT", StringComparison.OrdinalIgnoreCase) ||
		w.Equals("IS", StringComparison.OrdinalIgnoreCase) || w.Equals("NULL", StringComparison.OrdinalIgnoreCase) || w.Equals("TRUE", StringComparison.OrdinalIgnoreCase) ||
		w.Equals("FALSE", StringComparison.OrdinalIgnoreCase) || w.Equals("UNKNOWN", StringComparison.OrdinalIgnoreCase) || w.Equals("IN", StringComparison.OrdinalIgnoreCase) ||
		w.Equals("BETWEEN", StringComparison.OrdinalIgnoreCase);
}
