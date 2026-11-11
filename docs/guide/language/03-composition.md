# Sequence, choice, groups, quantifiers and lookahead

How recognizers combine: one after another, one or the other, repeated, or tested without being
read. The specification is [`syntax.md` §3.2–§3.4 and §3.8](../../syntax.md#32-composition).

This page is about the operators of `.Gram` itself — `&`, `|`, quantifiers — and the order in which
they bind inside one expression. The precedence of the operators of the language a grammar *reads*
(`*` before `+` in arithmetic) is a property of rules, said with binding powers
(`<< n`, `>> n`, [below](#not-to-be-confused-with-binding-powers) and in
[rules](07-rules.md#binding-powers)).

<!-- ru:begin -->
> **RU.** Эта страница — об операторах самого `.Gram` (`&`, `|`, кванторы) и о порядке их связывания
> внутри одного выражения. Приоритеты операторов того языка, который грамматика *читает* (`*` раньше
> `+` в арифметике), — свойство правил и задаются силами связывания `<< n`, `>> n` (ниже и в главе 07).
<!-- ru:end -->

## Sequence: `&`

`a & b` matches `a`, then `b` where `a` ended. Every pair of operands needs a connector: `.Gram` has
no juxtaposition, and two operands side by side are an error.

```dotgram
Word   = ['a'..'z']+
Assign = Word & '=' & Word

parse Assign
```

```text examples
ParseAssign("x=y") → "x=y"
ParseAssign("x = y") → refused
```

```dotgram GRAM2002
Assign = 'x' '=' 'y'
```

Notes:

- Nothing may stand between the operands unless the grammar says so: `"x = y"` is refused. The rule
  `trivia` is what is woven between them, and it is empty by default
  ([trivia](08-trivia-and-words.md)).
- `a ~ b` is a sequence that never lets trivia between its operands; it matters only once
  `trivia` is not empty ([`~`](08-trivia-and-words.md#adjacency-)).
- The missing connector above is reported where the next operand begins, as the start of a
  declaration that is not one (`GRAM2002`).

<!-- ru:begin -->
> **RU.** `a & b` читает `a`, затем `b` с того места, где закончилось `a`. Между любыми двумя
> операндами нужен соединитель: «склеивания» без оператора в языке нет, и `'x' '=' 'y'` — ошибка
> (о ней сообщается как о не-объявлении в месте второго операнда, `GRAM2002`). Между операндами
> во входе ничего не может стоять, пока грамматика этого не разрешит: `"x = y"` отвергается; между
> ними вплетается правило `trivia`, по умолчанию пустое. `a ~ b` — последовательность, в которую
> `trivia` не вплетается никогда; это важно, только когда `trivia` не пусто.
<!-- ru:end -->

## Ordered choice: `|`

`a | b` tries `a` first and `b` only where `a` does not lead to a match. `&` binds tighter than `|`,
so `x & y | z` is `(x & y) | z`.

```dotgram
Scheme = "http" | "https"
Sign   = '+' & ['0'..'9'] | '-' & ['0'..'9']

parse Scheme
find  Scheme as Schemes
parse Sign
```

```text examples
ParseScheme("https") → "https"
Schemes("https") → ["http"]
ParseSign("-7") → "-7"
```

Over characters a choice **backtracks**: when something after it fails, the next alternative is
tried. `ParseScheme("https")` first takes `"http"`, finds `s` left over where `parse` wants the end,
and tries `"https"`. A `find` asks only for an occurrence, and `"http"` is one, so it is the answer.
Write the longer alternative first where it is the one wanted: `"https" | "http"`.

A rule call is transparent to this: a choice inside a called rule is resumed exactly as one written
inline.

```dotgram
Start = Name & 'y'
Name  = "xy" | "x"

parse Start
```

```text examples
ParseStart("xy") → "xy"
```

`Name` first answers `"xy"`, `'y'` then finds nothing, and the parse goes back into `Name` for
`"x"`.

Notes:

- Alternatives are never reordered, whatever their lengths: order is meaning, as in .NET regular
  expressions ([`syntax.md` §11](../../syntax.md#11-deliberately-out-of-scope)).
- In a grammar split into a lexer and a parser (`Lexical = true`), the syntactic half reads
  tokens, and there the first alternative that matches **stands**: nothing after it sends the
  parse back into it. A rule marked `Name?` gives back
  ([rules](07-rules.md#rules-that-give-back), [host options](15-host-options.md)).
- `|` inside square brackets is set union, a different construct ([elements](02-elements.md)).

<!-- ru:begin -->
> **RU.** `a | b` пробует `a` и берёт `b`, только если `a` не ведёт к совпадению. `&` связывает
> сильнее `|`: `x & y | z` — это `(x & y) | z`. Над символами выбор **откатывается**: если дальше
> что-то не совпало, пробуется следующая альтернатива. `ParseScheme("https")` сначала берёт
> `"http"`, видит лишний `s` (а `parse` требует конец входа) и пробует `"https"`; `find` нужно лишь
> вхождение, и `"http"` — вхождение. Если нужна длинная альтернатива — пишите её первой. Вызов
> правила прозрачен для отката: выбор внутри вызванного правила возобновляется так же, как
> написанный на месте (`Start`/`Name` выше). Альтернативы никогда не переупорядочиваются. В
> грамматике, разделённой на лексер и парсер (`Lexical = true`), синтаксическая половина читает
> токены, и там первая совпавшая альтернатива **стоит**: назад в неё не возвращаются (правило
> `Name?` — отдаёт). `|` в квадратных скобках — объединение множества, другая конструкция.
<!-- ru:end -->

## Groups: `( … )`

Parentheses group an expression into one operand, for a quantifier, a capture or a choice inside a
sequence.

```dotgram
Key   = ('a' | 'b') & 'c'
Pairs = ('a' & 'b')+

parse Key
parse Pairs
```

```text examples
ParseKey("bc") → "bc"
ParsePairs("abab") → "abab"
ParsePairs("aba") → refused
```

Notes:

- A group never changes what is matched; it only says what belongs together.
- With a `=>` inside, a group builds a value of its own ([values](04-values.md#constructing-groups)).
- An expression nested more than 128 levels deep (groups, braces, lookaheads, captures, a call's
  arguments, …) is `GRAM2011`: name the inner part as a rule.

<!-- ru:begin -->
> **RU.** Скобки группируют выражение в один операнд — для квантора, захвата или выбора внутри
> последовательности; что читается, группа не меняет. С `=>` внутри группа строит своё значение.
> Вложенность глубже 128 уровней — `GRAM2011`: вынесите внутреннюю часть в правило.
<!-- ru:end -->

## Atomic groups: `{ … }`

Braces around an expression make it **atomic**: once it has matched, the choices made inside it are
final, and nothing that fails later sends the parse back into it.

```dotgram
Free   = ("xy" | "x") & 'y'
Atomic = { "xy" | "x" } & 'y'

parse Free
parse Atomic
```

```text examples
ParseFree("xy") → "xy"
ParseAtomic("xy") → refused
ParseAtomic("xyy") → "xyy"
```

`Atomic` commits to `"xy"` and cannot take it back for `'x'` alone. Braces are how a grammar says
"this reads to its end and never hands a character back", which is what a lexeme means:

```dotgram
Number = { ['0'..'9']+ }
Greedy = Number & ['0'..'9']

parse Greedy
```

```text examples
ParseGreedy("123") → refused
```

Notes:

- Braces have the same matches and value as the expression inside them; only what a later failure
  may undo changes. A failure *inside* the braces is an ordinary failure.
- Use them where the grammar has a real commit point: lexemes, trivia, a run that should not be
  cut two ways (below). Do not use them where a rule is meant to give back, as a dotted name that
  is a type only as far as it resolves.
- Rule boundaries never commit by themselves over characters: extracting part of an expression into
  a rule changes nothing.
- Braces after an operand with a number inside are a count, `X{3}`; braces where an operand begins
  are an atomic group.

<!-- ru:begin -->
> **RU.** Фигурные скобки делают выражение **атомарным**: после совпадения выборы внутри него
> окончательны, и никакая последующая неудача не возвращает разбор внутрь. `Atomic` фиксирует
> `"xy"` и не может отдать его ради одного `'x'`. Скобками грамматика говорит «читаю до конца и
> ничего не отдаю» — это и есть лексема (`Number` в примере: `Greedy` на `"123"` отвергается).
> Совпадения и значение у скобок те же, что у выражения внутри; неудача внутри скобок — обычная
> неудача. Ставьте их там, где есть реальная точка фиксации (лексемы, trivia, повторение, которое
> можно разрезать двумя способами), и не ставьте там, где правило должно отдавать. Граница правила
> над символами сама ничего не фиксирует. Скобки после операнда с числом — счётчик `X{3}`, в начале
> операнда — атомарная группа.
<!-- ru:end -->

## Quantifiers: `?`, `*`, `+`, `{n}`, `{n,m}`, `{n,}`

Every quantifier is postfix, as in regular expressions:

| Written | Matches |
| --- | --- |
| `X?` | zero or one `X` |
| `X*` | zero or more |
| `X+` | one or more |
| `X{3}` | exactly three |
| `X{2,4}` | two to four |
| `X{2,}` | two or more |

```dotgram
Sign    = '-'?
Digits  = ['0'..'9']+
Year    = ['0'..'9']{4}
Code    = ['A'..'Z']{2,3}
Spaces  = ' '{2,}
Number  = Sign & Digits

parse Number
parse Year
parse Code
parse Spaces
```

```text examples
ParseNumber("-42") → "-42"
ParseNumber("42") → "42"
ParseYear("2026") → "2026"
ParseYear("226") → refused
ParseCode("USD") → "USD"
ParseCode("USDT") → refused
ParseSpaces(" ") → refused
```

Quantifiers are greedy and, over characters, give back: `'a'* & 'a'` reads `aaa` by letting the
repetition return one `a`.

```dotgram
GiveBack = 'a'* & 'a'

parse GiveBack
```

```text examples
ParseGiveBack("aaa") → "aaa"
```

Notes:

- A count may also be a value parameter of the rule, `X{n}` ([parameters](07-rules.md#parameters)),
  or the length of a bound text, `X{#q}` ([bindings](06-bindings.md)).
- There are no lazy (`*?`) or possessive (`*+`) quantifiers, and one operand takes one quantifier:
  write `{ X* }` for possessive, and a group for a quantifier of a quantified operand, `(X+)*`.
- The body of `*`, `+` and `{n,m}` must consume input on every turn, which is checked when the
  grammar is built.

Refused:

```dotgram GRAM4001
Loop = ('a'?)*
```

```dotgram GRAM2005
Never = 'a'{3,2}
```

```dotgram GRAM2005
NoLower = 'a'{,3}
```

```dotgram GRAM2017
Twice = 'a'+*
```

`('a'?)*` could repeat forever without reading (`GRAM4001`). `{3,2}` can never match and `{,3}` has
no lower bound (`GRAM2005`: write `{0,3}`). A second quantifier on one operand is `GRAM2017`, which
says what to write instead: a group for a repetition of a repetition, braces for one that keeps what it
took.

<!-- ru:begin -->
> **RU.** Все кванторы постфиксные, как в регулярных выражениях (таблица выше): `?`, `*`, `+`,
> `{3}`, `{2,4}`, `{2,}`. Они жадные и над символами отдают назад: `'a'* & 'a'` читает `aaa`,
> потому что повторение возвращает одно `a`. Счётчиком может быть параметр-значение правила `{n}`
> или длина привязанного текста `{#q}`. Ленивых (`*?`) и сверхжадных (`*+`) кванторов нет, и у
> операнда один квантор: сверхжадный пишется `{ X* }`, квантор над квантором — через группу
> `(X+)*`. Тело `*`, `+`, `{n,m}` обязано потреблять вход на каждом шаге — это проверяется при
> сборке. Отвергаются: `('a'?)*` — `GRAM4001` (бесконечный цикл без чтения); `{3,2}` и `{,3}` —
> `GRAM2005` (пишите `{0,3}`); второй квантор на одном операнде — `GRAM2017` (сообщение
> подсказывает группу или атомарные скобки).
<!-- ru:end -->

## Lookahead: `?=X` and `?!X`

A lookahead tests what follows without reading it.

| Written | Succeeds where | Value |
| --- | --- | --- |
| `?=X` | `X` matches here | what `X` would give |
| `?!X` | `X` does not match here | none (`""` if captured) |

```dotgram
Keyword    = "if" | "else"
Identifier = ?!Keyword & ['a'..'z']+
Before     = ['a'..'z']+ & ?=';'

parse Identifier
find  Before as Statements
```

```text examples
ParseIdentifier("count") → "count"
ParseIdentifier("if") → refused
ParseIdentifier("iffy") → refused
Statements("a; b, c;") → ["a", "c"]
```

`?!Keyword` refuses `iffy` as well: `"if"` matches at its start. Saying "a keyword is a whole word"
is the job of `wordboundary` ([keyword boundaries](08-trivia-and-words.md#wordboundary)), or of a
lookahead after the keyword: `"if" & ?![\p{L}]`.

A positive lookahead may be captured, and then holds what the operand would have given, without
having moved:

```dotgram
Small : int = n: ?=Number & when @(n < 100) & v: Number => v
Number : int = ['0'..'9']+ => @int.Parse(parserText)

parse Small
```

```text examples
ParseSmall("42") → 42
ParseSmall("420") → refused
```

Notes:

- The spelling is .NET's (`(?=…)`, `(?!…)`), without the parentheses: `?=X` takes one operand, so a
  sequence needs a group, `?!('*' & '/')`.
- A quantifier binds tighter than a lookahead: `?!X*` is `(?!X)*`, which reads nothing and is
  refused (`GRAM4001`); write `?!(X*)`.
- A lookahead reads its own way and gives everything back: a capture inside it, or a guard there that
  reads a capture made before it, is refused (`GRAM4006`). So are a binding `$q:` and a `recover`
  inside one ([bindings](06-bindings.md), [recovery](12-recovery.md)).
- A guard reads nothing, so `?=(when @(g))` is `when @(g)`.

Refused:

```dotgram GRAM4001
NotA = ?!'a'*
```

```dotgram GRAM4006
Peek : @string = ?=(c: 'a') & any => @(c)
```

<!-- ru:begin -->
> **RU.** Просмотр проверяет продолжение, не читая его. `?=X` успешен, если `X` совпадает здесь, и
> даёт значение `X`; `?!X` — если не совпадает, значения нет (захваченный — `""`). `?!Keyword`
> отвергает и `iffy`: `"if"` совпадает в его начале; «ключевое слово — целое слово» — это задача
> `wordboundary` или просмотра после слова: `"if" & ?![\p{L}]`. Захваченный положительный просмотр
> хранит значение операнда, не сдвинувшись (`Small` выше). Написание — как в .NET, без скобок:
> просмотр берёт один операнд, последовательность — в группе `?!('*' & '/')`. Квантор связывает
> сильнее просмотра: `?!X*` — это `(?!X)*`, оно ничего не читает и отвергается (`GRAM4001`); пишите
> `?!(X*)`. Захват внутри просмотра или охранник там, читающий внешний захват, — `GRAM4006`; привязка
> `$q:` и `recover` внутри просмотра тоже запрещены. Охранник ничего не читает: `?=(when @(g))` —
> это `when @(g)`.
<!-- ru:end -->

## Operator order in an expression

How the operators of `.Gram` bind inside one grammar expression, highest to lowest:

| Level | Forms |
| --- | --- |
| 1 | capture `name: X`, binding `$q: X`, lookahead `?=X`, `?!X` — prefixes, nesting either way round |
| 2 | quantifiers `X?`, `X*`, `X+`, `X{n,m}`; suffixes `recover S`, `with (…)`, `with state @(…)` |
| 3 | adjacency `a ~ b` |
| 4 | sequence `a & b` |
| 5 | construction `=> value` |
| 6 | ordered choice `a | b` |

What it means in practice:

```dotgram
Digit  : int = ['0'..'9'] => @(parserText[0] - '0')
Each   : @int[] = ds: Digit* => @(ds)
Letter = ['a'..'z']
Joined : @string = ls: Letter* => @(ls)

parse Each
parse Joined
```

```text examples
ParseEach("123") → [1, 2, 3]
ParseJoined("abc") → "abc"
```

- A quantifier applies to everything before it in its operand: `name: X*` is `(name: X)*`, a
  capture made on every turn. Turns of a rule that builds a value are collected into an array; turns
  of text are joined into one text ([values](04-values.md)). Likewise `?!X*` is `(?!X)*`.
- `=>` binds to one alternative, so every branch of a `|` builds its own value
  ([values](04-values.md)).
- `recover` binds tighter than `|`: `X* recover '|' | eol` is `(X* recover '|') | eol`; a choice of
  synchronizations needs brackets, `recover ('|' | eol)` ([recovery](12-recovery.md)).
- Inside a `when`, `is` binds tighter than `and`, and `and` than `or`
  ([guards](05-guards-and-switch.md)).

<!-- ru:begin -->
> **RU.** Порядок операторов `.Gram` внутри одного выражения грамматики, от высшего к низшему, — в таблице: префиксы (захват, привязка, просмотр) →
> кванторы и суффиксы (`recover`, `with (…)`, `with state`) → `~` → `&` → `=>` → `|`. На практике:
> квантор относится ко всему, что перед ним в операнде: `name: X*` — это `(name: X)*`, захват на
> каждом шаге; шаги правила со значением собираются в массив, шаги текста склеиваются в один текст
> (пример выше); так же `?!X*` — это `(?!X)*`; `=>` привязан к одной альтернативе, поэтому каждая ветка `|` строит своё значение;
> `recover` сильнее `|`, выбор синхронизаций пишется в скобках; внутри `when` `is` сильнее `and`,
> `and` сильнее `or`.
<!-- ru:end -->

### Not to be confused with binding powers

The table above orders the operators of the grammar. The precedence of the operators of the
language being read — `*` before `+`, `^` to the right — is said on the alternatives of a rule, with
`<< n` (left-associative at level `n`) and `>> n` (right-associative). Higher binds tighter. The
whole story is in [rules](07-rules.md#binding-powers); one calculator shows the shape:

```dotgram
Expr : int = left: Expr & '+' & right: Expr   << 1  => @(left + right)
           | left: Expr & '-' & right: Expr   << 1  => @(left - right)
           | left: Expr & '*' & right: Expr   << 2  => @(left * right)
           | left: Expr & '^' & right: Expr   >> 3  => @((int)System.Math.Pow(left, right))
           | digits: ['0'..'9']+                    => @int.Parse(digits)

parse Expr
```

```text examples
ParseExpr("1+2*3") → 7
ParseExpr("8-2-1") → 5
ParseExpr("2^3^2") → 512
ParseExpr("2*3^2") → 18
```

`8-2-1` is `(8-2)-1` because `-` is left-associative, and `2^3^2` is `2^(3^2)` because `^` is
right-associative. A grammar can also say all of this with one rule per level and no markers; which
to choose is in [rules](07-rules.md#levels-as-rules).

<!-- ru:begin -->
> **RU.** Таблица выше упорядочивает операторы самой грамматики. Приоритеты операторов читаемого
> языка (`*` раньше `+`, `^` справа налево) задаются на альтернативах правила силами связывания:
> `<< n` — левоассоциативно на уровне `n`, `>> n` — правоассоциативно; больше — сильнее. Подробно — в
> главе 07; пример калькулятора выше: `8-2-1` — это `(8-2)-1` (левая ассоциативность `-`), `2^3^2` —
> `2^(3^2)` (правая у `^`). То же можно выразить и правилом на каждый уровень без маркеров — выбор
> между способами описан в главе 07.
<!-- ru:end -->

## A run that can be cut two ways

Over characters a refused input is refused only after every reading of it has been tried. A
repetition that can read the same text in two ways makes that exponential:

```dotgram GRAM4052
Run = ('a' | 'a' & 'a')+ & 'b'

parse Run
```

```text examples
ParseRun("aab") → "aab"
```

`aa` is one turn or two, so a long run of `a` without the `b` is tried two to the n ways before it
is refused. The compiler warns (`GRAM4052`, at the rule, with the shortest text read two ways).
There are two cures:

```dotgram
OneReading = 'a'+ & 'b'
Committed  = { ('a' | 'a' & 'a')+ } & 'b'

parse OneReading
parse Committed
```

```text examples
ParseOneReading("aab") → "aab"
ParseCommitted("aab") → "aab"
```

Write the repetition so every text has one reading — this keeps the language — or brace it, which
commits the first reading and so may change the language where a later operand needed a turn given
back. The warning is exact for what it sees and blind past a call to a recursive or braced rule; a
lookahead or a guard that removes one reading is not seen, so it may warn where there is no second
reading ([`syntax.md` §3.3](../../syntax.md#33-quantifiers--postfix-as-in-regular-expressions)).
It is the one warning that can be raised to an error alone:
`<WarningsAsErrors>GRAM4052</WarningsAsErrors>`.

<!-- ru:begin -->
> **RU.** Над символами вход отвергается только после перебора всех его прочтений. Повторение,
> которое читает один и тот же текст двумя способами, делает перебор экспоненциальным: в `Run`
> `aa` — один шаг или два, и длинная цепочка `a` без `b` перебирается 2^n способами. Компилятор
> предупреждает (`GRAM4052`). Лечение: переписать повторение так, чтобы у текста было одно прочтение
> (язык сохраняется), или поставить скобки (фиксирует первое прочтение и может изменить язык, если
> дальше нужен был отданный шаг). Предупреждение точно в том, что видит, слепо за вызовом
> рекурсивного или атомарного правила и не видит просмотров и охранников, поэтому может сработать
> там, где второго прочтения нет. Это единственное предупреждение, которое можно отдельно поднять до
> ошибки: `<WarningsAsErrors>GRAM4052</WarningsAsErrors>`.
<!-- ru:end -->
