# A grammar and its host

A `.gram` grammar describes how text becomes typed values; the generator turns it into C#
methods on a partial class, the **host**. This page covers the file, its declarations and the
`@` that switches between the grammar's names and C#'s. The specification is
[`syntax.md` §1–§2](../../syntax.md#1-the-file-and-the-host-class).

## Where a grammar lives

A grammar is attached to a `partial` class with `[Gram]`, in one of three ways:

```csharp
[Gram]                                  // the file FeedGrammar.gram, found by the class's name
public partial class FeedGrammar;

[Gram("formats/feed.gram")]             // a file named explicitly
public partial class FeedGrammar;

[Gram("""                               // the grammar written right here
	Digits = ['0'..'9']+
	parse Digits
	""")]
public partial class Numbers;
```

A `.gram` file is given to the build as `<AdditionalFiles Include="…" />`. Every method the grammar
publishes becomes a static member of the host: `Numbers.ParseDigits("42")`.

Notes:

- The host must be `partial` (`GRAM0002`). A file that cannot be found is `GRAM0003`, two files
  answering to one name `GRAM0004`.
- The host may also be generated outside a build by the `dotnet dotgram` tool ([`cli.md`](../../cli.md)).

<!-- ru:begin -->
> **RU.** Грамматика прикрепляется к `partial`-классу (хосту) атрибутом `[Gram]` тремя способами:
> файл, найденный по имени класса; файл, указанный явно; текст грамматики прямо в атрибуте (пример
> выше). Файл `.gram` передаётся в сборку как `<AdditionalFiles>`. Каждый опубликованный метод
> становится статическим членом хоста. Хост обязан быть `partial` (`GRAM0002`); ненайденный файл —
> `GRAM0003`, два подходящих файла — `GRAM0004`. Хост можно сгенерировать и вне сборки утилитой
> `dotnet dotgram`.
<!-- ru:end -->

## A first grammar

```dotgram
Sum    : int = left: Sum & '+' & right: Number => @(left + right)
             | n: Number                        => n
Number : int = ['0'..'9']+ => @int.Parse(parserText)

parse Sum
```

```text examples
ParseSum("1+2+3") → 6
ParseSum("42") → 42
ParseSum("1+") → refused
TryParseSum("1+x") → refused
```

Everything a grammar is made of is in it:

- a **rule** is a name, an optional type after `:`, `=`, and a body;
- `&` joins operands into a sequence and `|` separates alternatives;
- `left:`, `right:` and `n:` **capture** what an operand read;
- `=>` builds the alternative's value, here with C# behind `@`;
- `parse Sum` **publishes** the rule: it makes `ParseSum`, which throws `FormatException` on a
  refusal, and `TryParseSum`, which answers a `Match<int>`.

`Sum` calls itself first; such a rule is read as a loop and its `=>` as a fold
([rules](07-rules.md#left-recursion)).

<!-- ru:begin -->
> **RU.** В примере выше есть всё, из чего состоит грамматика: **правило** — имя, необязательный
> тип после `:`, `=` и тело; `&` соединяет операнды в последовательность, `|` разделяет
> альтернативы; `left:`, `right:`, `n:` — **захваты** прочитанного; `=>` строит значение
> альтернативы (здесь C#-кодом после `@`); `parse Sum` **публикует** правило: появляются
> `ParseSum` (бросает `FormatException` при отказе) и `TryParseSum` (возвращает `Match<int>`).
> `Sum` вызывает сам себя первым — такое правило читается циклом, а его `=>` — свёрткой.
<!-- ru:end -->

## Layout and comments

Whitespace and line breaks in a grammar mean nothing, and comments are C#'s: `// to the end of
the line` and `/* … */`, which does not nest.

```dotgram
/* A grammar may be laid out freely. */
Pair = Word & ',' & Word   // one rule
Word = ['a'..'z']+         // another; no separator between them

parse Pair
```

```text examples
ParsePair("ab,cd") → "ab,cd"
ParsePair("ab, cd") → refused
```

**Rules need no separator.** Every pair of operands is joined by `&`, `|` or `~`, so an expression
can never run on into the next rule: a name followed by `=`, `:` or `(` where the expression is
already complete begins a new declaration.

Notes:

- Spaces in the grammar are not spaces in the input: `"ab, cd"` is refused above. What may stand
  between operands in the input is the rule `trivia` ([trivia](08-trivia-and-words.md)).
- An unterminated `/*` is `GRAM1003`; a character nothing begins is `GRAM1005`.

<!-- ru:begin -->
> **RU.** Пробелы и переводы строк в грамматике ничего не значат; комментарии — как в C#: `//` до
> конца строки и `/* … */` (без вложенности). **Разделитель правил не нужен**: любые два операнда
> соединены `&`, `|` или `~`, поэтому выражение не может «перетечь» в следующее правило — имя, за
> которым идёт `=`, `:` или `(` там, где выражение уже закончено, начинает новое объявление.
> Пробелы в грамматике — не пробелы во входе: `"ab, cd"` выше отвергается; что может стоять между
> операндами во входе, задаёт правило `trivia`. Незакрытый `/*` — `GRAM1003`; символ, с которого
> ничего не начинается, — `GRAM1005`.
<!-- ru:end -->

## Declarations

A grammar is a sequence of declarations, in any order:

| Declaration | Example | Page |
| --- | --- | --- |
| a rule | `Name = body`, `Name : Type = body`, `Name(p) = body` | [rules](07-rules.md) |
| a publication | `parse Name`, `find Name`, `internal parse X as Y` | [publication](11-publication.md) |
| a namespace | `namespace Lexical { … }`, `namespace N with (A = B) { … }` | [namespaces](09-namespaces.md) |
| an import | `using Lexical;`, `@using System.Text;` (first in the file or block) | below |
| a context type | `context : @Names` | [context and state](13-context-and-state.md) |
| a state type | `state : @Overflow` | [context and state](13-context-and-state.md) |

The words that begin a declaration — `namespace`, `parse`, `find`, `public`, `internal`,
`private`, `positional`, `context`, `state` — are words only there. A rule may be called `parse`
or `state`: what follows the name (`=`, `:`, `(`) decides.

```dotgram
parse  = "parse"
state  = "state"
Phrase = parse & ' ' & state

parse Phrase
```

```text examples
ParsePhrase("parse state") → "parse state"
```

Anything else at the top of a grammar is `GRAM2002`.

<!-- ru:begin -->
> **RU.** Грамматика — последовательность объявлений в любом порядке: правило, публикация,
> пространство имён, импорт (только в начале файла или блока), тип контекста, тип состояния
> (таблица выше). Слова, начинающие объявление (`namespace`, `parse`, `find`, `public`,
> `internal`, `private`, `positional`, `context`, `state`), — ключевые только в этой позиции:
> правило может называться `parse` или `state`, решает то, что идёт после имени (`=`, `:`, `(`).
> Всё прочее на верхнем уровне — `GRAM2002`.
<!-- ru:end -->

## Two vocabularies: `@` and its absence

A bare name is looked up among the grammar's rules (and, inside a rule, its captures and
parameters). A name after `@` is C#'s: a type, a method, a property. There is no fallback in
either direction.

| Written | Means |
| --- | --- |
| `Row` | the rule `Row` |
| `@Row` | the C# type or member `Row` |
| `string`, `int`, `char`, `bool`, `decimal`, … | always the C# type; these words cannot name a rule |
| `@(expression)` | a C# expression, handed to the compiler as written |

What may follow `@`: a name (`@DateOnly`), a dotted name (`@Encoding.UTF8`), a call
(`@int.Parse(text)`), a generic name (`@List<int>`), or a parenthesized C# expression
(`@(y * 10000 + m * 100)`).

```dotgram
@using System;
@using System.Globalization;

Year : int = ['0'..'9']{4} => @int.Parse(parserText, CultureInfo.InvariantCulture)
Date : @DateOnly = y: Year & '-' & m: Two & '-' & d: Two => @(new DateOnly(y, m, d))
Two  : int = ['0'..'9']{2} => @(int.Parse(parserText))

parse Date
```

```text examples
ParseDate("2026-11-10") → 11/10/2026
ParseDate("2026-1-10") → refused
```

Notes:

- The generator does not parse the C# after `@`; it finds where it ends and hands the text to
  the compiler, so C# mistakes are C# errors, reported against the grammar's line.
- An exception thrown by the C# is not a refusal: `ParseDate("2026-13-10")` matches the text and
  then `new DateOnly` throws `ArgumentOutOfRangeException` out of the parse. A value that may not
  exist is checked with a guard ([guards](05-guards-and-switch.md)).
- Inside `@(…)` and the arguments of `@M(…)` everything is C#: `=> @M(a, b)` and `=> @(M(a, b))`
  are the same construction.
- There is no block form (`@{ … }`): code belongs in the host class.
- `@(` must be written together: `@ (a + b)` is a call on a missing name. An unterminated
  `@(` is `GRAM1006`.

<!-- ru:begin -->
> **RU.** Голое имя ищется среди правил грамматики (а внутри правила — среди его захватов и
> параметров); имя после `@` — это C#: тип, метод, свойство. Никакого «запасного» поиска в другую
> сторону нет. Ключевые слова C# (`string`, `int`, …) — всегда типы C# и не могут быть именами
> правил. После `@` может стоять имя, имя через точку, вызов, обобщённое имя или C#-выражение в
> скобках. Генератор не разбирает C#: он лишь находит конец вставки и передаёт текст компилятору,
> поэтому ошибки C# — это ошибки C#, привязанные к строке грамматики. Внутри `@(…)` и аргументов
> `@M(…)` всё — C#. Блочной формы `@{ … }` нет. `@(` пишется слитно; незакрытая `@(` — `GRAM1006`.
> Исключение из C#-кода — не отказ: `ParseDate("2026-13-10")` совпадает с текстом, а затем
> `new DateOnly` бросает `ArgumentOutOfRangeException` наружу; допустимость значения проверяется
> охранником (`when`). (Значение `DateOnly` в примере показано через его `ToString` в инвариантной культуре.)
<!-- ru:end -->

## Imports: `@using` and `using`

`@using X.Y;` imports a C# namespace for the C# the grammar names; `using X;` imports a grammar
namespace. Both are written as C# writes a `using` directive, semicolon included, and stand
before the declarations of a file or of a `namespace` block.

```dotgram
@using System.Text;
using Lexical;

namespace Lexical
{
	Word = ['a'..'z']+
}

Pair : string = a: Word & '=' & b: Word => @(new StringBuilder(a).Append(b).ToString())

parse Pair
```

```text examples
ParsePair("ab=cd") → "abcd"
```

An import names a namespace declared anywhere in the grammar, before or after it. A `using`
written after a declaration is not an import, and is read as the start of a rule:

```dotgram GRAM2001 GRAM2002
Word = ['a'..'z']+

using Lexical;
```

Notes:

- No C# namespace is imported for a grammar by default — not even `System`: `@DateOnly` above
  needs `@using System;` (`GRAM3004` otherwise).
- An unknown grammar namespace in `using` is `GRAM3003`. Two imports that supply one name are an
  error where the name is used, settled by qualifying it (`Lexical.Word`).
- The standard library is the namespace `Std`, always present: `Std.Integer`, or `using Std;`
  and then `Integer` ([standard library](10-std.md)).

<!-- ru:begin -->
> **RU.** `@using X.Y;` импортирует пространство имён C# (для C#-кода грамматики), `using X;` —
> пространство имён грамматики. Оба пишутся как директива C#, с точкой с запятой, и стоят перед
> объявлениями файла или блока `namespace`; импортировать можно пространство, объявленное где угодно
> в грамматике. `using` после объявления — уже не импорт, а начало правила (второй пример,
> `GRAM2001`/`GRAM2002`). Пространства имён C# по умолчанию не импортируются, даже `System`
> (`GRAM3004`). Неизвестное пространство грамматики — `GRAM3003`; два импорта с одинаковым именем —
> ошибка в месте использования, снимается квалификацией. Стандартная библиотека — пространство `Std`.
<!-- ru:end -->
