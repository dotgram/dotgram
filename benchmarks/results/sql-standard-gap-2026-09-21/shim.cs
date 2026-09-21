// Roslyn injects this where a compilation embeds types; the generated attributes file names it.
// The real build gets it from the polyfill in DotGram.Sql's own obj, which this project does not
// compile, so it is supplied here rather than by adding a package whose version would then be a
// second thing to keep in step.
namespace Microsoft.CodeAnalysis
{
	[global::System.AttributeUsage(global::System.AttributeTargets.All, AllowMultiple = false)]
	internal sealed class EmbeddedAttribute : global::System.Attribute
	{
	}
}
