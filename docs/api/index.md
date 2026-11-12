# API reference

The generator, `DotGram`, ships no runtime assembly: everything a generated parser needs is
emitted into your own compilation. The libraries below are packages of their own, written in
.Gram and read like any other .NET library.

| Package | What it holds |
| --- | --- |
| `DotGram.Sql` | SQL parsers (SQL:2023, SQL-92, T-SQL), the one tree they build, its writer and walker. |
| `DotGram.Web` | Parsers and checks for the formats of the web. |
| `DotGram.Finance` | FIX: the wire read into typed fields, and the messages built over them. |
| `DotGram.ExpressionLanguage` | The C#-style expression language. |
