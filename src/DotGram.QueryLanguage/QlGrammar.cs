using System;

namespace DotGram.QueryLanguage;

/// <summary>The query language's parser: text into <see cref="Syntax.QuerySyntax"/>, nothing resolved.</summary>
[Gram("QueryLanguage.gram", Lexical = true, Carrier = GramCarrier.Immediate)]
static partial class QlGrammar;
