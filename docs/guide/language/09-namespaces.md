# Namespaces and rebinding

A grammar of more than a page groups its rules: lexical rules apart from syntax, one dialect's rules apart
from another's. Namespaces do the grouping; rebinding — `with (A = B)` — reads the same rules with one of
them replaced, which is how one grammar serves several dialects. The specification is
[`syntax.md` §5–§5.1](../../syntax.md#5-namespaces).

## Namespace blocks

`namespace Name { … }` is a block of declarations. The top of a file is the global namespace; an inner
namespace sees the outer one, and a rule inside is named `Name.Rule` from outside.

```dotgram
namespace Lexical
{
	Word   = { ['a'..'z']+ }
	Number = { ['0'..'9']+ }

	namespace Quoted
	{
		Text = '"' & [^ '"']* & '"'
	}
}

Assign = Lexical.Word & '=' & (Lexical.Number | Lexical.Quoted.Text)

parse Assign
```

```text examples
ParseAssign("x=42") → "x=42"
ParseAssign("x=\"a b\"") → "x=\"a b\""
```

Notes:

- Braces after `namespace Name` are a block, not an expression: in expression position they are an atomic
  group or a count.
- Every namespace becomes a nested static class of the generated code, and a rule's generated type is
  named with its namespaces joined by `_` (`Lexical_Word`).

<!-- ru:begin -->
> **RU.** `namespace Name { … }` — блок объявлений. Верх файла — глобальное пространство; вложенное
> видит внешнее; снаружи правило называется `Name.Rule` (пример `Assign` с вложенным `Lexical.Quoted`).
> Фигурные скобки после `namespace Name` — блок, а не выражение (в выражении это атомарная группа или
> счётчик). Каждое пространство — вложенный статический класс сгенерированного кода; сгенерированный тип
> правила называется с пространствами через `_` (`Lexical_Word`).
<!-- ru:end -->

## Imports

`using Name;` brings the names of a grammar namespace into the current scope unqualified. It stands at
the head of the file or of a `namespace` block, before every declaration (`GRAM2019` otherwise).

```dotgram
using Lexical;

namespace Lexical
{
	Word = { ['a'..'z']+ }
}

namespace Syntax
{
	using Lexical;

	Pair = Word & ':' & Word
}

List = Word & (',' & Word)*

parse Syntax.Pair
parse List
```

```text examples
ParsePair("a:b") → "a:b"
ParseList("a,b,c") → "a,b,c"
```

**What wins.** A rule of the scope's own wins over an imported one of the same name at the top of a
file. Inside a namespace block, declaring a rule whose name already resolves — from an enclosing scope or
through the block's own `using` — is refused (`GRAM3012`): a declaration always means a new rule, and
replacing one is what a rebinding is for (below). Two imports that supply one name are an error where the
name is used, settled by qualifying it, as in C#.

```dotgram
using Lexical;

namespace Lexical
{
	Word = { ['a'..'z']+ }
}

Word = { ['A'..'Z']+ }

parse Word
parse Lexical.Word as ParseLower
```

```text examples
ParseWord("ABC") → "ABC"
ParseWord("abc") → refused
ParseLower("abc") → "abc"
```

Refused:

```dotgram GRAM3012
Word = { ['a'..'z']+ }

namespace Inner
{
	Word = { ['A'..'Z']+ }
}

parse Inner.Word
```

```dotgram GRAM3003
using Missing;

Word = { ['a'..'z']+ }

parse Word
```

```dotgram GRAM3001
Word = { ['a'..'z']+ }
Word = { ['A'..'Z']+ }

parse Word
```

Notes:

- Shadowing a built-in rule — `trivia`, `wordboundary`, `word`, `any`, `none`, `eol`, `eof` — is the
  language's normal mechanism, at any depth, and is never reported ([trivia](08-trivia-and-words.md)).
- The standard library is the namespace `Std`, in view in every grammar ([standard library](10-std.md)).
- A `@using` imports a C# namespace, not a grammar one ([the grammar and its host](01-grammar-and-host.md#imports-using-and-using)).

<!-- ru:begin -->
> **RU.** `using Name;` вносит имена пространства грамматики в текущую область без квалификации; стоит в
> начале файла или блока, до всех объявлений (иначе `GRAM2019`). **Что побеждает.** На верхнем уровне
> файла собственное правило побеждает импортированное с тем же именем (пример `Word`: читается `ABC`).
> Внутри блока `namespace` объявление правила, имя которого уже разрешается (из объемлющей области или
> через `using` блока), отвергается (`GRAM3012`): объявление всегда значит новое правило, а замена — это
> перепривязка. Два импорта с одним именем — ошибка в месте использования, снимается квалификацией, как
> в C#. Неизвестное пространство в `using` — `GRAM3003`; два правила с одним именем в одном пространстве
> — `GRAM3001`. Затенение встроенных правил (`trivia`, `wordboundary`, `word`, `any`, `none`, `eol`,
> `eof`) — обычный механизм языка и никогда не сообщается. Стандартная библиотека — пространство `Std`.
> `@using` импортирует пространство C#, а не грамматики.
<!-- ru:end -->

## Rebinding in a namespace header

`namespace Name with (A = B, …) { … }` rebinds `A` to `B` for the **whole call graph reached from what
the namespace declares** — not only for calls written inside the block.

```dotgram
B = 'c'
A = B
F = A

namespace Ns with (B = D)
{
	E = A
}

D = 'd'

parse F
parse Ns.E
```

```text examples
ParseF("c") → "c"
ParseE("d") → "d"
ParseE("c") → refused
```

`F`, outside, resolves `A` to the ordinary `B`. `E`, inside, resolves the same `A` through `D` — although
`A` is declared outside and never mentions `D`. `A` itself is untouched; the rules reached from inside are
read as copies with the substitution applied.

**Shadowing or rebinding?** An ordinary declaration changes what a name means only where it is lexically
visible, and a rule declared elsewhere never sees it. A header makes the *same* shared rule behave
differently depending on which call path reached it. Reach for a header when that is the point — two
decimal points over one `Number` rule; reach for an ordinary declaration when a rule's meaning is simply
different from here on.

```dotgram
Digits = ['0'..'9']+
Point  = '.'
Comma  = ','
Number = Digits & (Point & Digits)?

namespace European with (Point = Comma)
{
	Amount = Number & ' ' & "EUR"
}

Amount = Number & ' ' & "USD"

parse Amount
parse European.Amount as ParseEuro
```

```text examples
ParseAmount("1.50 USD") → "1.50 USD"
ParseEuro("1,50 EUR") → "1,50 EUR"
ParseEuro("1.50 EUR") → refused
```

Notes:

- Bindings in one header resolve together, against the scope the header is written in:
  `with (A = B, B = C)` sends a call to `A` all the way to `C`.
- A nested namespace inherits its enclosing one's bindings and may replace any of them.
- Both sides must name a visible rule (`GRAM3006`, `GRAM3007`); one rule bound twice in one header is
  `GRAM3008`; a loop, `with (A = B, B = A)`, is `GRAM3011`.
- A parameterized rule may replace, and be replaced by, one of the same signature — the same number and
  kinds of parameters (`GRAM3009` otherwise); the call's arguments are kept.
- Inside a namespace that binds a name, declaring a rule of that name is refused (`GRAM3012`, as any
  declaration of a name that already resolves outside): replace it with a nested header instead.
- A replacement whose result does not fit what the replaced rule's readers expect — a declared type, a
  sequence — is `GRAM4014`.
- `with` is required in a header: `namespace N (A = B)` is `GRAM2006`.

Refused:

```dotgram GRAM2006
B = 'b'
D = 'd'

namespace Ns (B = D)
{
	E = B
}

parse Ns.E
```

```dotgram GRAM3006
D = 'd'

namespace Ns with (Missing = D)
{
	E = D
}

parse Ns.E
```

```dotgram GRAM3011
A = 'a'
B = 'b'

namespace Ns with (A = B, B = A)
{
	E = A
}

parse Ns.E
```

```dotgram GRAM3009
L(x)    = x & x
M(x, y) = x & y

namespace Ns with (L = M)
{
	E = L('a')
}

parse Ns.E
```

```dotgram GRAM3012
B = 'b'
D = 'd'

namespace Ns with (B = D)
{
	B = 'x'
	E = B
}

parse Ns.E
```

<!-- ru:begin -->
> **RU.** `namespace Name with (A = B, …) { … }` перепривязывает `A` к `B` для **всего графа вызовов,
> достижимого из объявленного в пространстве**, а не только для вызовов, написанных в блоке. В примере
> `F` снаружи разрешает `A` к обычному `B`, а `E` внутри разрешает тот же `A` через `D`, хотя `A` объявлен
> снаружи и `D` не упоминает; сам `A` не меняется — достижимые изнутри правила читаются копиями с
> подстановкой. **Затенение или перепривязка?** Обычное объявление меняет смысл имени только там, где оно
> лексически видно; заголовок заставляет *одно и то же* общее правило вести себя по-разному в
> зависимости от пути вызова. Заголовок — когда в этом суть (две десятичные точки над одним `Number`,
> пример `European`); обычное объявление — когда смысл правила просто другой дальше по тексту. Привязки
> одного заголовка разрешаются вместе (`with (A = B, B = C)` ведёт `A` к `C`); вложенное пространство
> наследует привязки внешнего и может заменить любую. Обе стороны — видимые правила (`GRAM3006`,
> `GRAM3007`); одно правило дважды в заголовке — `GRAM3008`; цикл — `GRAM3011`; параметризованное правило
> заменяется только правилом той же сигнатуры (`GRAM3009`), аргументы вызова сохраняются; объявление
> правила с привязанным именем внутри пространства — `GRAM3012`, как любое объявление уже разрешимого
> снаружи имени (используйте вложенный заголовок);
> замена с несовместимым результатом — `GRAM4014`; `with` в заголовке обязателен (`GRAM2006`).
<!-- ru:end -->

## Rebinding one expression: `X with (A = B)`

The same substitution on a single operand, with no block and no name:

```dotgram
Digits = ['0'..'9']+
Point  = '.'
Comma  = ','
Number = Digits & (Point & Digits)?

Row = a: Number & ';' & b: Number with (Point = Comma)

parse Row
```

```text examples
ParseRow("1.5;2,5") → Row { A = "1.5", B = "2,5" }
ParseRow("1.5;2.5") → refused
```

`with` binds as tightly as a quantifier and reaches only the operand it is written on: here `b`'s
`Number`, not `a`'s. `Number+ with (X = Y)` is `(Number+) with (X = Y)`.

**What replaces a rule may be any operand**, not only a rule's name; a choice needs brackets:

```dotgram
Item = ['a'..'z']+
Sep  = ','
List = Item & (Sep & Item)*

Loose = List with (Sep = (',' | ';'))

parse Loose
```

```text examples
ParseLoose("a,b;c") → "a,b;c"
```

The expression becomes a rule of its own, declared where the `with` is written, so it reads the
`trivia`, the imports and the bindings around it. The left side stays a name.

Notes:

- Reach for `X with (…)` when the substitution belongs to one place a rule is used, and for a header once
  more than one call needs it or it deserves a name.
- A `with` inside an argument of a parameterized rule is refused (`GRAM4041`): declare a rule with the
  rebinding and pass that.

<!-- ru:begin -->
> **RU.** Та же подстановка на одном операнде, без блока и имени (пример `Row`: только `Number` у `b`
> читает запятую). `with` связывает так же сильно, как квантор, и действует только на свой операнд:
> `Number+ with (X = Y)` — это `(Number+) with (X = Y)`. **Заменой может быть любой операнд**, не только
> имя правила; выбор — в скобках (пример `Loose`). Выражение становится отдельным правилом, объявленным
> там, где написан `with`, и читает окружающие `trivia`, импорты и привязки; левая сторона — всегда имя.
> `X with (…)` — когда подстановка нужна в одном месте; заголовок — когда она нужна нескольким вызовам или
> заслуживает имени. `with` внутри аргумента параметризованного правила отвергается (`GRAM4041`):
> объявите правило с перепривязкой и передайте его.
<!-- ru:end -->

## Rebinding a publication

A `parse` or `find` directive may carry the substitution itself, which is how one grammar publishes
several parsers:

```dotgram
Digits = ['0'..'9']+
Point  = '.'
Comma  = ','
Number = Digits & (Point & Digits)?

parse Number                        as ParseInvariant
parse Number with (Point = Comma)   as ParseEuropean
```

```text examples
ParseInvariant("3.14") → "3.14"
ParseEuropean("3,14") → "3,14"
ParseEuropean("3.14") → refused
```

The same rebinding answers conditions: `when Version is "2000"` is decided against the rules as each
publication has them, so `parse Test with (Version = "2000")` and `with (Version = "2022")` are two
parsers of one grammar ([conditions](05-guards-and-switch.md#conditions-when-a-is-b)).

**How substitutions stack.** Where several reach one call, the one written closer to where reading starts
applies on top: an outer `with` over an inner one, a caller's header over the header of a namespace it calls
into, a publication's `with` over everything. A binding of a name a lower substitution already replaced does
nothing, since nothing calls that name any more:

```dotgram
B = 'b'
D = 'd'
Y = 'y'

namespace Ns with (B = D)
{
	E = B
}

R = Ns.E with (B = Y)

parse R
```

```text examples
ParseR("d") → "d"
ParseR("y") → refused
```

Inside `Ns`, `B` is already `D`, so the outer `B = Y` has nothing to replace. A substitution that loops once
stacked is `GRAM3011`; two that each need the other applied first are `GRAM4017`.

<!-- ru:begin -->
> **RU.** Директива `parse` или `find` может нести подстановку сама — так одна грамматика публикует
> несколько парсеров (пример: `ParseInvariant` и `ParseEuropean` над одним `Number`). Та же перепривязка
> отвечает на условия `when Version is …` (глава 05). **Как складываются подстановки.** Если к одному
> вызову доходят несколько, та, что написана ближе к началу чтения, применяется поверх: внешний `with`
> поверх внутреннего, заголовок вызывающего поверх заголовка пространства, в которое он вызывает, `with`
> публикации поверх всего. Привязка имени, которое нижняя подстановка уже заменила, ничего не делает —
> это имя больше никто не вызывает (пример `R`: внутри `Ns` `B` уже `D`, и внешнее `B = Y` заменять
> нечего). Подстановка, зациклившаяся после сложения, — `GRAM3011`; две, каждой из которых нужна сначала
> другая, — `GRAM4017`.
<!-- ru:end -->
