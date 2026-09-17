using System;
using System.Runtime.InteropServices;

using DotGram.ExpressionLanguage;

var square = ExpressionParser.Compile<Func<int, int>>("(int x) => x * x - 1");
if (square(3) != 8)
	throw new Exception("The compiled expression returned the wrong value.");
var absolute = ExpressionParser.Compile<Func<int, int>>("using System; (int x) => Math.Abs(x)");
if (absolute(-3) != 3)
	throw new Exception("A referenced framework type was not resolved.");
var calculate = ExpressionParser.Compile<Func<int, int, int>>(
	"(int x, int y) => { int sum = x + y; return sum * sum; }");
if (calculate(2, 3) != 25)
	throw new Exception("The compiled block returned the wrong value.");
if (ExpressionParser.TryParse("(string s) => s - 1").IsSuccess)
	throw new Exception("An invalid operator was accepted.");
Console.WriteLine($"DotGram.ExpressionLanguage package smoke: expressions compiled ({RuntimeInformation.FrameworkDescription}).");
