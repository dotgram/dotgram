using System.Diagnostics.CodeAnalysis;

// What RequiresUnreferencedCode and RequiresDynamicCode on ExpressionParser do not reach: the types the
// generator nests in it, which carry no attribute of their own (a struct cannot) and call the class's
// members. Each is reached only through ExpressionParser's entry points, and those warn where an
// application calls them; what is suppressed here is that same warning again, inside the parser.

[assembly: UnconditionalSuppressMessage("Trimming", "IL2026", Scope = "type", Target = "T:DotGram.ExpressionLanguage.ExpressionParser.Reader_DotGram_Lambda", Justification = "Reached only through ExpressionParser, which requires unreferenced code.")]
[assembly: UnconditionalSuppressMessage("AOT", "IL3050", Scope = "type", Target = "T:DotGram.ExpressionLanguage.ExpressionParser.Reader_DotGram_Lambda", Justification = "Reached only through ExpressionParser, which requires dynamic code.")]
[assembly: UnconditionalSuppressMessage("Trimming", "IL2026", Scope = "type", Target = "T:DotGram.ExpressionLanguage.ExpressionParser.Deep_DotGram_Lambda", Justification = "Reached only through ExpressionParser, which requires unreferenced code.")]
[assembly: UnconditionalSuppressMessage("AOT", "IL3050", Scope = "type", Target = "T:DotGram.ExpressionLanguage.ExpressionParser.Deep_DotGram_Lambda", Justification = "Reached only through ExpressionParser, which requires dynamic code.")]
[assembly: UnconditionalSuppressMessage("Trimming", "IL2026", Scope = "type", Target = "T:DotGram.ExpressionLanguage.ExpressionParser.Match`1", Justification = "Reached only through ExpressionParser, which requires unreferenced code.")]
[assembly: UnconditionalSuppressMessage("AOT", "IL3050", Scope = "type", Target = "T:DotGram.ExpressionLanguage.ExpressionParser.Match`1", Justification = "Reached only through ExpressionParser, which requires dynamic code.")]
