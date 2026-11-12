# Rules, parameters, recursion and binding powers

A rule is a name for a recognizer, and everything else in a grammar is built from rules calling
rules. This page covers how a rule is declared, the parameters it may take, how it may call itself,
and how the precedence of the operators of the language being read is said. The specification is
[`syntax.md` §4–§4.3](../../syntax.md#4-rules).

The examples build values with captures and `=>`, as [the first grammar](01-grammar-and-host.md#a-first-grammar)
did; what a rule gives back is told in full in [captures and values](07-values.md), and the guards some
examples use in [guards](11-guards-and-switch.md).

<!-- ru:begin -->
> **RU.** Примеры строят значения захватами и `=>`, как первая грамматика главы 01; что возвращает правило,
> подробно — в главе 07, охранники, которые встречаются в примерах, — в главе 11.
<!-- ru:end -->

## Declaring a rule

```text
Name = body                          the type is named after the rule (or is the text, see values)
Name : Type = body                   the type is given
Name : Type[] = body                 a sequence of Type
Name(params) : Type = body           a rule with parameters
Name? : Type = body                  a rule that may give back over tokens
Name : Type on fail "…" = body       and what a refusal of it says
```

A declaration begins with the rule's name; the type is optional; what a rule gives back is the
subject of [values](07-values.md), and `on fail` is shown there too.

```dotgram
Digit             = ['0'..'9']
Number : int      = Digit+ => @int.Parse(parserText)
Numbers : int[]   = Number & (',' & Number)*

parse Numbers
```

```text examples
ParseNumbers("1,22,333") → [1, 22, 333]
```

Notes:

- Rules may be declared in any order and call each other freely, including before they are declared.
- A rule nothing reaches — no publication, no call, no rebinding — is reported (`GRAM4018`, a
  warning).
- Two rules of one name in one namespace are `GRAM3001`; a call to a name that is no rule is
  `GRAM3002`.

```dotgram GRAM3001
Word = ['a'..'z']+
Word = ['A'..'Z']+

parse Word
```

```dotgram GRAM3002
Pair = Word & '=' & Wrod
Word = ['a'..'z']+

parse Pair
```

```dotgram GRAM4018
Used   = 'a'
Unused = 'b'

parse Used
```

<!-- ru:begin -->
> **RU.** Объявление начинается с имени правила; тип необязателен; формы заголовка — в таблице:
> без типа, с типом, с последовательностью `Type[]`, с параметрами, с `?` («может отдавать» над
> токенами) и с `on fail`. Правила объявляются в любом порядке и вызывают друг друга свободно, в том
> числе до объявления. Правило, до которого ничего не доходит (ни публикация, ни вызов, ни
> перепривязка), — предупреждение `GRAM4018`. Два правила с одним именем в одном пространстве —
> `GRAM3001`; вызов имени, которое не правило, — `GRAM3002`.
<!-- ru:end -->

## Parameters

A rule may take parameters, written like captures: `name` or `name: Type`. Which kind a parameter is
follows from the two vocabularies:

| Declared | Is | Passed |
| --- | --- | --- |
| `item` | a recognizer; its type comes from the call | a rule, a literal, any expression |
| `item: Row` | a recognizer that must produce `Row` | the same |
| `n: int`, `pad: char`, `t: @Tag` | a value | a literal, or a value this rule was handed |
| `n` used only as a count | a value: a number is a value wherever it goes | a number |

```dotgram
Number : int    = { ['0'..'9']+ } => @int.Parse(parserText)
Word   : string = { ['a'..'z']+ } => @(parserText)

List(item, sep) : item[] = item & (sep & item)*
Digits(n)                = ['0'..'9']{n}
Padded(item, pad)        : item = pad* & value: item & pad* => value

Numbers : @int[]  = v: List(Number, ',') => v
Words   : @string[] = v: List(Word, (';' | '|')) => v
Year              = Digits(4)
Spaced  : @string = v: Padded(Word, '.') => v

parse Numbers
parse Words
parse Year
parse Spaced
```

```text examples
ParseNumbers("1,2,3") → [1, 2, 3]
ParseWords("a;b|c") → ["a", "b", "c"]
ParseYear("2026") → "2026"
ParseYear("226") → refused
ParseSpaced("..ab.") → "ab"
```

`: item` in a type position means "of whatever type `item` produces", `: item[]` a sequence of those:
there are no type parameters in the language, and none are needed.

Notes:

- A parameterized rule is **specialized** for each distinct set of arguments, before anything runs: a
  recognizer parameter is never a delegate, and a call costs what calling the rule written out costs.
  A recursive call that would make specializations without end is refused when the grammar is built.
- So a value parameter is always something known when the grammar is built — a literal, or what the
  caller was handed. A captured value is not: it exists only while a parse runs. Pass what was
  captured to a `=>` instead.
- An argument is one alternative: a choice needs brackets, `List(Word, (';' | '|'))`.
- `item[]` gathers the operands that **build** a value: `Word` above builds its text with a `=>`; a
  rule that is only its text builds nothing, even declared `: string`, and `List(Word, …)` would then
  have nothing to collect (`GRAM4008`).
- A value parameter stands where a value goes — a count, an argument of `@M`, inside `@(…)`; one used
  where a piece of grammar goes is refused (`GRAM4013`). `Padded` takes its padding as a recognizer
  for that reason: `pad: char` and then `pad*` is refused.
- A rule whose body is only a call is not typed by the call: `Numbers` above captures the list and
  hands it on, or it would be the text it matched ([values](07-values.md#what-a-rule-gives-back)).

Refused:

```dotgram GRAM4013
List(item, sep) : item[] = item & (sep & item)*
Bare = List

parse Bare
```

```dotgram GRAM4013
List(item, sep) : item[] = item & (sep & item)*
Short = List('a')

parse Short
```

```dotgram GRAM2016
Word  = ['a'..'z']+
Typed = Word<int>

parse Typed
```

A parameterized rule's name alone is no rule, and a call must pass every parameter (`GRAM4013`). A
rule takes no type arguments: only a C# name is generic, written `@Name<T>` (`GRAM2016`).

<!-- ru:begin -->
> **RU.** Правило может принимать параметры, записанные как захваты: `name` или `name: Type`. Вид
> параметра следует из двух словарей (таблица): `item` — распознаватель, тип берётся из вызова;
> `item: Row` — распознаватель, обязанный давать `Row`; `n: int`, `pad: char`, `t: @Tag` — значение;
> `n`, использованный только как счётчик, — значение (число — значение везде). `: item` в позиции типа
> значит «того типа, что даёт `item`», `: item[]` — последовательность таких; параметров-типов в языке
> нет и не нужно. Параметризованное правило **специализируется** для каждого набора аргументов до
> запуска: распознаватель-параметр — не делегат, вызов стоит как вызов выписанного правила;
> рекурсия, порождающая бесконечно много специализаций, отвергается. Поэтому параметр-значение —
> всегда то, что известно при сборке (литерал или то, что передали вызывающему); захваченное значение
> существует только во время разбора — передавайте его в `=>`. Аргумент — одна альтернатива: выбор
> пишется в скобках `List(Word, (';' | '|'))`. `item[]` собирает операнды, которые **строят** значение: `Word` строит свой текст через `=>`;
> правило, которое есть просто свой текст, ничего не строит, даже с `: string`, и тогда `List(Word, …)`
> нечего собирать (`GRAM4008`). Параметр-значение стоит там,
> где ожидается значение (счётчик, аргумент `@M`, внутри `@(…)`); в позиции куска грамматики он
> отвергается (`GRAM4013`) — поэтому `Padded` принимает заполнитель как распознаватель. Правило, тело которого — вызов, не получает тип вызова: `Numbers`
> захватывает список и передаёт его. Отвергаются (`GRAM4013`): имя параметризованного правила без
> аргументов и вызов с неверным числом аргументов; аргументы типа у правила (`Word<int>`) — `GRAM2016`:
> обобщённым бывает только имя C#, `@Name<T>`.
<!-- ru:end -->

## Rules that give back

Over characters a rule call is transparent: when something after a rule fails, the parse may go back into
the rule and take another of its choices ([composition](03-composition.md#ordered-choice-)). In a grammar
read over tokens a rule's answer stands instead, and a rule that needs to give back is marked `Name?`; that
is told with the reading over tokens itself ([`Lexical`](06-lexical.md#choices-stand-and-rules-that-give-back)).
Over characters `?` is accepted and changes nothing.

<!-- ru:begin -->
> **RU.** Над символами вызов правила прозрачен для отката: если после правила что-то не совпало, разбор может
> вернуться внутрь и взять другой выбор. В грамматике над токенами ответ правила стоит, а правило, которому
> нужно отдавать, помечается `Name?` — это описано вместе с чтением над токенами (глава 06). Над символами
> `?` допустим и ничего не меняет.
<!-- ru:end -->

## Levels as rules

The default way to say precedence: one rule per level, each calling the next. It needs no notation
and compiles to the same recursive descent as everything else.

```dotgram
Number : int = { ['0'..'9']+ } => @int.Parse(parserText)
Sum    : int = left: Sum & '+' & right: Product   => @(left + right)
             | left: Sum & '-' & right: Product   => @(left - right)
             | p: Product                         => p
Product : int = left: Product & '*' & right: Atom => @(left * right)
             | a: Atom                            => a
Power  : int = base_: Number & ('^' & exp: Power)? => @(exp is null ? base_ : (int)System.Math.Pow(base_, exp.Value))
Atom   : int = '(' & s: Sum & ')' => s
             | p: Power           => p

parse Sum
```

```text examples
ParseSum("8-2-1") → 5
ParseSum("1+2*3") → 7
ParseSum("2^3^2") → 512
ParseSum("(1+2)*3") → 9
```

**Associativity is the side the recursion is on.** `Sum` recurses on the left, so `8-2-1` is `(8-2)-1`;
`Power` recurses on the right, so `2^3^2` is `2^(3^2)`. There is no notation for it because none is
needed.

Notes:

- Write the right-recursive level with an optional tail, as `Power` is. The form with two alternatives
  that both begin with the same operand reads that operand twice, and where the operand leads back to
  the rule — as an atom does through its brackets — the cost doubles at every level of nesting. The
  compiler warns (`GRAM4016`); it is reported rather than rewritten, because two alternatives and an
  optional tail are not always the same language ([`syntax.md` §4.3](../../syntax.md#43-precedence-and-associativity)).

```dotgram GRAM4016
Number : int = { ['0'..'9']+ } => @int.Parse(parserText)
Power  : int = left: Atom & '^' & right: Power => @((int)System.Math.Pow(left, right))
             | value: Atom                     => value
Atom   : int = '(' & p: Power & ')' => p
             | n: Number            => n

parse Power
```

<!-- ru:begin -->
> **RU.** Способ задать приоритеты по умолчанию — правило на каждый уровень, каждое вызывает следующее;
> нотации не нужно, компилируется в тот же рекурсивный спуск. **Ассоциативность — это сторона
> рекурсии**: `Sum` рекурсивно слева, поэтому `8-2-1` — это `(8-2)-1`; `Power` — справа, поэтому
> `2^3^2` — это `2^(3^2)`. Правоассоциативный уровень пишите с необязательным хвостом, как `Power`:
> форма из двух альтернатив, начинающихся с одного операнда, читает этот операнд дважды, а если
> операнд ведёт обратно в правило (атом — через скобки), стоимость удваивается на каждом уровне
> вложенности. Компилятор предупреждает (`GRAM4016`, последний пример) и не переписывает сам: две
> альтернативы и необязательный хвост — не всегда один и тот же язык.
<!-- ru:end -->

## Left recursion

A rule may call itself first. It is read as a loop over its tails, and its `=>` as a **fold**: the
leading capture of a left-recursive alternative is the value built so far — first by an alternative
that is not left-recursive, then by the previous step.

```dotgram
Number : int = { ['0'..'9']+ } => @int.Parse(parserText)
Digits : @string = l: Digits & ',' & n: Number => @(l + "+" + n)
                 | n: Number                   => @("" + n)

parse Digits
```

```text examples
ParseDigits("1,2,3") → "1+2+3"
```

A guard on a step may read the value so far by that name, which asks for it early: the step's
construction then runs while the input is read rather than after
([guards](11-guards-and-switch.md)).

```dotgram
Number : int = { ['0'..'9']+ } => @int.Parse(parserText)
Short  : @string = l: Short & ',' & n: Number & when @(l.Length < 2) => @(l + n)
                 | n: Number                                          => @("" + n)

find Short as Shorts
```

```text examples
Shorts("1,2,3") → ["12", "3"]
```

The layered shape every expression grammar has works too: a rule that only forwards another's value
(`Primary = p: Call => p | n: Number => n`) is looked through, so recursion that goes through it is
read as direct.

```dotgram
Number  : @string = { ['0'..'9']+ }
Primary : @string = c: Call => c | n: Number => n
Call    : @string = target: Primary & '(' & ')' => @(target + "()")

parse Primary
```

```text examples
ParsePrimary("f()()") → refused
ParsePrimary("1()()") → "1()()"
```

Refused when the grammar is built:

```dotgram GRAM4002
Expr = Expr & '+' & Expr
     | ['0'..'9']

parse Expr
```

```dotgram GRAM4002
Loop = Loop & 'a'

parse Loop
```

```dotgram GRAM4039
Sum = left: Sum & '+' & right: ['0'..'9']
    | ['0'..'9']

parse Sum
```

- `Expr & '+' & Expr` is recursive on both sides: what is written left-associative would read
  right-associative. Write the operands one level down, or use binding powers (below).
- A rule whose every alternative is left-recursive has nothing to start from.
- A left-recursive rule with no `=>` builds nothing to fold, so its steps may not capture
  (`GRAM4039`): give every alternative a `=>`, or capture only in the alternatives that are not
  recursive.
- An intermediary that does something of its own, between a rule and its left recursion, is refused
  too; so is a guard that names another step's name for the value so far (`GRAM4031`).

<!-- ru:begin -->
> **RU.** Правило может вызывать себя первым. Оно читается как цикл по хвостам, а его `=>` — как
> **свёртка**: первый захват леворекурсивной альтернативы — значение, построенное до сих пор
> (сначала нерекурсивной альтернативой, затем предыдущим шагом); пример `Digits` даёт `"1+2+3"`.
> Охранник шага может прочитать накопленное значение по этому имени — это запрашивает его раньше:
> конструирование шага выполняется во время чтения (пример `Short`). Слоистая форма выражений тоже
> работает: правило, которое только передаёт значение другого (`Primary`), «просматривается насквозь»,
> и рекурсия через него читается как прямая. Отвергается при сборке: рекурсия с обеих сторон
> `Expr & '+' & Expr` (записанное левоассоциативным читалось бы правоассоциативным — опустите операнды
> на уровень ниже или используйте силы связывания) — `GRAM4002`; правило, у которого все альтернативы
> леворекурсивны (не с чего начать), — `GRAM4002`; леворекурсивное правило без `=>` со захватами в
> шагах — `GRAM4039`; посредник, делающий что-то своё, и охранник, называющий имя накопленного
> значения другого шага (`GRAM4031`).
<!-- ru:end -->

## Binding powers

The other way to say precedence: one rule, and each alternative states its own strength.

```dotgram
Expr : int = left: Expr & '+' & right: Expr   << 1  => @(left + right)
           | left: Expr & '-' & right: Expr   << 1  => @(left - right)
           | left: Expr & '*' & right: Expr   << 2  => @(left * right)
           | left: Expr & '^' & right: Expr   >> 3  => @((int)System.Math.Pow(left, right))
           | '-' & operand: Expr              >> 4  => @(-operand)
           | left: Expr & '!'                 << 5  => @(Factorial(left))
           | '(' & inner: Expr & ')'                => inner
           | digits: ['0'..'9']+                    => @int.Parse(digits)

parse Expr
```

```csharp
partial class Grammar
{
	static int Factorial(int n)
	{
		return n <= 1 ? 1 : n * Factorial(n - 1);
	}
}
```

```text examples
ParseExpr("1+2*3") → 7
ParseExpr("8-2-1") → 5
ParseExpr("2^3^2") → 512
ParseExpr("-1-2") → -3
ParseExpr("-2^2") → 4
ParseExpr("3!+1") → 7
ParseExpr("(1+2)*3") → 9
```

`<< n` and `>> n` say one thing: at what strength the operand to the right is read.

| Written | Is |
| --- | --- |
| `left: E & op & right: E << n` | infix, left-associative, level `n` |
| `left: E & op & right: E >> n` | infix, right-associative, level `n` |
| `op & operand: E >> n` | prefix, level `n` |
| `left: E & op << n` | postfix, level `n` |
| no marker | an atom: a literal, a group, a call |

Higher binds tighter; levels are whole numbers from 0 to 9999 and need not be contiguous, so a level
can be put between two later. `-1-2` is `-3` because the prefix minus (4) is stronger than the infix
one (1); `-2^2` is `4` because the prefix (4) is stronger than `^` (3) too — the grammar, not a
convention, says so.

**Levels as rules or binding powers?** Levels as rules is the default: no notation, no engine, and for
a handful of levels it is the faster of the two. Binding powers buy an expression language with many
levels written as one rule, and shapes ordered choice cannot settle; they cost a precedence-climbing
loop at run time.

Notes:

- A step is taken only where the operand before it could not have taken it, so the grouping is the
  strengths' own, whatever a `when` in a step accepts.
- A strength binds the operand an alternative ends with. One the alternative may or may not end with —
  inside an optional, a choice or a repetition — is read at 0, and the compiler warns (`GRAM4042`).
- A rule uses one convention or the other: a recursive alternative without a marker beside ones with
  markers is refused (`GRAM4009`; the example below also draws `GRAM4016`, since its two infix
  alternatives begin with the same operand without a marker to share it).

Refused:

```dotgram GRAM2005
E : int = left: E & '+' & right: E << 10000 => @(left + right)
        | d: ['0'..'9'] => @(d[0] - '0')

parse E
```

```dotgram GRAM4009 GRAM4016
E : int = left: E & '+' & right: E << 1 => @(left + right)
        | left: E & '*' & right: E      => @(left * right)
        | d: ['0'..'9'] => @(d[0] - '0')

parse E
```

```dotgram GRAM4042
E : int = left: E & '?' & mid: E & (':' & right: E)? >> 1 => @(left + mid)
        | d: ['0'..'9'] => @(d[0] - '0')

parse E
```

<!-- ru:begin -->
> **RU.** Другой способ задать приоритеты: одно правило, и каждая альтернатива указывает свою силу.
> `<< n` и `>> n` говорят одно: с какой силой читается операнд справа. Таблица: инфикс
> левоассоциативный `<< n`, инфикс правоассоциативный `>> n`, префикс `op & operand: E >> n`, постфикс
> `left: E & op << n`, без маркера — атом. Больше — сильнее; уровни — целые 0…9999, не обязательно
> подряд (можно вставить уровень между). `-1-2` = `-3`, потому что префиксный минус (4) сильнее
> инфиксного (1); `-2^2` = `4`, потому что префикс (4) сильнее `^` (3) — так говорит грамматика.
> **Что выбрать:** правила по уровням — по умолчанию (без нотации и движка, для нескольких уровней
> быстрее); силы связывания дают язык выражений со многими уровнями в одном правиле и формы, которые
> упорядоченный выбор не решает, ценой цикла «precedence climbing» во время работы. Шаг делается только
> там, где предыдущий операнд не мог взять его сам — группировка определяется силами, что бы ни
> принимал `when`. Сила привязана к операнду, которым заканчивается альтернатива; операнд, которым она
> может и не закончиться (внутри `?`, выбора, повторения), читается с силой 0 — предупреждение
> `GRAM4042`. В правиле — одно соглашение: рекурсивная альтернатива без маркера рядом с маркированными
> — `GRAM4009`. Сила вне 0…9999 — `GRAM2005`.
<!-- ru:end -->

## Alternatives that decide after a nested construct

Two alternatives that share a prefix holding a nested instance of their own recursion — a bracket
read as a value, then read again as a condition — read that instance once per alternative, at every
level: two to the depth. The compiler finds the shape and says so (`GRAM4054`, information), and over
tokens on the immediate carrier the reader remembers what the first alternative built, so each level is
read once. The rewrites — factoring the shared prefix, or reading the bracket once and classifying
after with a `switch` — are in [`syntax.md` §4.7](../../syntax.md#47-alternatives-that-decide-after-a-nested-construct).

<!-- ru:begin -->
> **RU.** Две альтернативы с общим префиксом, содержащим вложенный экземпляр их же рекурсии (скобка,
> прочитанная как значение, а затем как условие), читают этот экземпляр по разу на альтернативу на
> каждом уровне — 2^глубины. Компилятор находит эту форму (`GRAM4054`, информация), а над токенами на
> немедленном носителе читатель запоминает построенное первой альтернативой, так что каждый уровень
> читается один раз. Переписывания (вынести общий префикс; прочитать скобку один раз и
> классифицировать `switch`) — в syntax.md §4.7.
<!-- ru:end -->
