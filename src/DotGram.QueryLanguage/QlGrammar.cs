using System;

namespace DotGram.QueryLanguage;

/// <summary>
/// The query language's parser: text into <see cref="Syntax.QuerySyntax"/>, nothing resolved.
/// Over tokens, since the language is a small SQL and a choice is decided by the word in front.
/// </summary>
[Gram("QueryLanguage.gram", Lexical = true, Carrier = GramCarrier.Immediate)]
static partial class QlGrammar;
