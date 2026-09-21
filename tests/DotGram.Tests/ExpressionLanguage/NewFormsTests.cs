using DotGram.ExpressionLanguage;

using Xunit;

namespace DotGram.Tests.ExpressionLanguage;

/// <summary>
/// Which forms of <c>new</c> this language reads, written down because the question was answered
/// wrongly once.
/// </summary>
/// <remarks>
/// <para>
/// A report reached the architect's journal saying collection initializers were unsupported. They
/// were supported all along; what was not was leaving out the constructor's parentheses before
/// one. The wrong account travelled through three hands, gaining confidence at each and gaining no
/// evidence, and the refusal text it would have produced would have told a consumer to stop
/// looking for a form they already had.
/// </para>
/// <para>
/// Igor then ruled that the parentheses become optional before an initializer, as in C#, so
/// <c>new List&lt;int&gt; { 448 }</c> reads now. What stays refused is <c>new T</c> with neither
/// tail — the parentheses are optional only before an initializer, never on their own.
/// </para>
/// <para>
/// These hold the forms and not the wording of any refusal: what a refusal says is a separate
/// question, still open, and one this file should not quietly settle.
/// </para>
/// </remarks>
public sealed class NewFormsTests
{
	[Theory]
	// An initializer reads with the constructor's parentheses…
	[InlineData("using System.Collections.Generic;\n(int x) => { List<int> t = new List<int>() { 448 }; return t.Count; }")]
	[InlineData("using System.Text;\n(int x) => { var b = new StringBuilder() { Capacity = 8 }; return b.Capacity; }")]
	// …and without them, as in C# (Igor, 2026-09-20).
	[InlineData("using System.Collections.Generic;\n(int x) => { List<int> t = new List<int> { 448 }; return t.Count; }")]
	[InlineData("using System.Collections.Generic;\n(int x) => new List<int> { 448 }.Count")]
	[InlineData("using System.Text;\n(int x) => { var b = new StringBuilder { Capacity = 8 }; return b.Capacity; }")]
	// A constructor with no initializer, and a typed array with one.
	[InlineData("using System.Collections.Generic;\n(int x) => { List<int> t = new List<int>(); t.Add(448); return t.Count; }")]
	[InlineData("(int x) => { int[] t = new int[] { 448, 447 }; return t.Length; }")]
	[InlineData("(int x) => { int[] t = new int[2]; return t.Length; }")]
	public void These_forms_of_new_read(string text) =>
		Assert.True(ExpressionParser.TryParse(text).IsSuccess, text);

	[Theory]
	// `new T` alone stays refused: the parentheses are optional only before an initializer,
	// which is C#'s rule too. Leaving them out on their own would be an extension nobody asked
	// for, and the guard on the rule is what keeps it out.
	[InlineData("using System.Collections.Generic;\n(int x) => { List<int> t = new List<int>; return t.Count; }")]
	// An empty initializer is refused, with or without parentheses: `Elements` wants at least
	// one. That predates the change and is left alone — one edit, one change of language.
	[InlineData("using System.Collections.Generic;\n(int x) => { List<int> t = new List<int> { }; return t.Count; }")]
	// An implicitly typed array: the element type has to be written.
	[InlineData("(int x) => { var t = new[] { 448, 447 }; return t.Length; }")]
	[InlineData("(int x) => { int[] t = new[] { 448, 447 }; return t.Length; }")]
	// An initializer with no `new` at all.
	[InlineData("(int x) => { int[] t = { 448, 447 }; return t.Length; }")]
	public void These_forms_of_new_do_not(string text) =>
		Assert.False(ExpressionParser.TryParse(text).IsSuccess, text);
}
