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
/// are supported; what is unsupported is leaving out the constructor's parentheses before one —
/// <c>new T() { … }</c> reads, <c>new T { … }</c> does not. The wrong account travelled through
/// three hands, gaining confidence at each and gaining no evidence, and the refusal text it would
/// have produced would have told a consumer to stop looking for a form they already have.
/// </para>
/// <para>
/// So these hold what is true whatever is decided about the missing parentheses: what reads today,
/// and what does not. They deliberately do **not** hold the text of any refusal — that wording is
/// open until Igor rules on whether the parentheses should become optional, as they are in C#.
/// </para>
/// </remarks>
public sealed class NewFormsTests
{
	[Theory]
	// An initializer reads when the constructor's parentheses are there.
	[InlineData("using System.Collections.Generic;\n(int x) => { List<int> t = new List<int>() { 448 }; return t.Count; }")]
	[InlineData("using System.Text;\n(int x) => { var b = new StringBuilder() { Capacity = 8 }; return b.Capacity; }")]
	// A constructor with no initializer, and a typed array with one.
	[InlineData("using System.Collections.Generic;\n(int x) => { List<int> t = new List<int>(); t.Add(448); return t.Count; }")]
	[InlineData("(int x) => { int[] t = new int[] { 448, 447 }; return t.Length; }")]
	[InlineData("(int x) => { int[] t = new int[2]; return t.Length; }")]
	public void These_forms_of_new_read(string text) =>
		Assert.True(ExpressionParser.TryParse(text).IsSuccess, text);

	[Theory]
	// The parentheses are not optional here, where C# lets them be left out.
	[InlineData("using System.Collections.Generic;\n(int x) => { List<int> t = new List<int> { 448 }; return t.Count; }")]
	[InlineData("using System.Collections.Generic;\n(int x) => new List<int> { 448 }.Count")]
	[InlineData("using System.Text;\n(int x) => { var b = new StringBuilder { Capacity = 8 }; return b.Capacity; }")]
	// An implicitly typed array: the element type has to be written.
	[InlineData("(int x) => { var t = new[] { 448, 447 }; return t.Length; }")]
	[InlineData("(int x) => { int[] t = new[] { 448, 447 }; return t.Length; }")]
	// An initializer with no `new` at all.
	[InlineData("(int x) => { int[] t = { 448, 447 }; return t.Length; }")]
	public void These_forms_of_new_do_not(string text) =>
		Assert.False(ExpressionParser.TryParse(text).IsSuccess, text);
}
