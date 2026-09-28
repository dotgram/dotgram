using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

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

// A plugin: this program's own assembly loaded again into a context of its own, which serves it a
// second copy of the package. A text read for the plugin names what the plugin references, so the
// type it reaches must be that copy and not the one this program runs on. On .NET 8 this is the
// netstandard2.0 asset, which reaches the load context by reflection.
var plugins = new PluginContext(typeof(ExpressionParser).Assembly.Location);
var plugin  = plugins.LoadFromAssemblyPath(typeof(PluginContext).Assembly.Location);
var named   = ExpressionParser.TryParse("() => DotGram.ExpressionLanguage.ResolutionScope.DefaultImports", plugin);
if (!named.IsSuccess)
	throw new Exception("A plugin's reference was not resolved: " + named.Error);
if (named.Value.Body is not MemberExpression { Member.DeclaringType: { } declaring } ||
	AssemblyLoadContext.GetLoadContext(declaring.Assembly) != plugins)
	throw new Exception("A plugin's reference was resolved outside the plugin's load context.");
Console.WriteLine($"DotGram.ExpressionLanguage package smoke: expressions compiled, a plugin's references found in its context ({RuntimeInformation.FrameworkDescription}).");

sealed class PluginContext(string package) : AssemblyLoadContext
{
	protected override Assembly? Load(AssemblyName name)
	{
		return name.Name == "DotGram.ExpressionLanguage" ? LoadFromAssemblyPath(package) : null;
	}
}
