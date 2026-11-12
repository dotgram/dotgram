# Trivia, adjacency and words

Whitespace, comments and keyword boundaries are not notation of their own. They are three built-in
rules — `trivia`, `wordboundary` and `word` — that a grammar redefines, and the compiler does
something automatic around them once they are not empty. The specification is
[`syntax.md` §4.5–§4.6](../../syntax.md#45-trivia--insignificant-whitespace-and-comments).

The examples call `Std` and declare namespaces as [the grammar and its host](01-grammar-and-host.md)
introduced them; both have chapters of their own later, [the standard library](09-std.md) and
[namespaces](10-namespaces.md).

<!-- ru:begin -->
> **RU.** Примеры вызывают `Std` и объявляют пространства имён так, как их представила глава 01; у обоих
> дальше свои главы — 09 и 10.
<!-- ru:end -->

## `trivia`: what may stand between operands

`trivia` is inserted at every seam between the operands of a sequence. It is `none` by default, so
by default nothing is inserted and nothing may stand between operands. A grammar to which
whitespace is insignificant redefines it:

```dotgram
trivia = { ' '* }

Word = { ['a'..'z']+ }
Pair = Word & '=' & Word

parse Pair
```

```text examples
ParsePair("a=b") → "a=b"
ParsePair("a = b") → "a = b"
ParsePair("  a = b  ") → "a = b"
ParsePair("a b = c") → refused
```

Notes:

- A whole `parse` also reads trivia before and after the rule, and the value leaves it out: the
  text of `"  a = b  "` is `"a = b"`.
- `trivia` must accept empty input (`GRAM4003`): it is inserted everywhere, and a second insertion
  reads nothing.
- When `trivia` is empty the insertions are dropped before anything is generated: a grammar that
  does not use it pays nothing.

```dotgram GRAM4003
trivia = ' '+
Pair   = 'a' & 'b'

parse Pair
```

<!-- ru:begin -->
> **RU.** `trivia` вставляется в каждый шов между операндами последовательности. По умолчанию это
> `none`, поэтому по умолчанию ничего не вставляется и между операндами ничего стоять не может.
> Грамматика, для которой пробелы незначимы, переопределяет его (пример выше). Целый `parse` читает
> trivia ещё и до и после правила, а значение её не включает: текст `"  a = b  "` — `"a = b"`.
> `trivia` обязана принимать пустой вход (`GRAM4003`): она вставляется везде, и повторная вставка
> ничего не читает. Пустая `trivia` выбрасывается до генерации — грамматика без неё ничего не платит.
<!-- ru:end -->

## Where the seams are

Trivia goes where an author wrote `&` — and between the turns of a repetition whose element is a
sequence or a value:

```dotgram
trivia = { ' '* }

Word    = { ['a'..'z']+ }
Entry   : @string = w: Word => w
List    = Word & (',' & Word)*
Several = Word*
Entries : @string[] = Entry*

parse List
parse Several
parse Entries
```

```text examples
ParseList("a , b ,c") → "a , b ,c"
ParseSeveral("abcd") → "abcd"
ParseSeveral("ab cd") → refused
ParseEntries("ab cd") → ["ab", "cd"]
```

| Shape | Seams |
| --- | --- |
| `a & b` | between `a` and `b` |
| `A & (S & A)*` | between every operand, and between the turns: the repeated part is a sequence |
| `Entry*` where `Entry` builds a value | between the turns: a list of things |
| `Word*`, `['0'..'9']+` — a repetition of something valueless | none: a lexeme is written this way |
| `X?`, a repetition of a choice | none |
| before a `when` | none: a guard reads nothing |

A spaced run of one valueless thing is the one case nothing can infer — `Word*` and `Digit*` have the
same shape. The author says which by naming `trivia`, an ordinary rule:

```dotgram
trivia = { ' '* }

Word  = { ['a'..'z']+ }
Words = Word & (trivia & Word)*

parse Words
```

```text examples
ParseWords("ab cd  ef") → "ab cd  ef"
```

<!-- ru:begin -->
> **RU.** Trivia вставляется там, где автор написал `&`, и между шагами повторения, элемент которого —
> последовательность или значение (таблица выше): `a & b` — между `a` и `b`; `A & (S & A)*` — между
> всеми операндами и шагами; `Entry*`, где `Entry` строит значение, — между шагами (список вещей);
> повторение чего-то без значения (`Word*`, `['0'..'9']+`) — без швов: так пишется лексема; `X?` и
> повторение выбора — без швов; перед `when` — без шва (охранник ничего не читает). Единственный
> случай, который нельзя вывести, — разделённый пробелами ряд одной вещи без значения (`Word*` и
> `Digit*` одинаковы по форме); автор говорит это сам, назвав правило `trivia` (пример `Words`).
<!-- ru:end -->

## Comments, and why trivia is braced

Comments are trivia too. The standard library has the usual ones:

```dotgram
trivia = { (Std.Spacing | Std.LineComment("//") | Std.Nested("/*", "*/"))* }

Word = { ['a'..'z']+ }
Pair = Word & '=' & Word

parse Pair
```

```text examples
ParsePair("a // note\n = /* x /* y */ */ b") → "a // note\n = /* x /* y */ */ b"
```

**Brace the trivia.** Without braces a failing parse may give back what trivia read, one character at
a time, and re-read a comment's interior as syntax. Where what stands beside a seam reads the same
characters as trivia — a field that may contain spaces — every run of spaces then has as many readings
as it is long, and a refused input is tried in every combination: a few malformed rows take seconds.
The compiler warns of that shape (`GRAM4051`):

```dotgram GRAM4051
trivia = ' '*

Field = [^ ',' | '\n']+
Row   = Field & (',' & Field)*

parse Row
```

`trivia = { ' '* }` reads each run once. Braced trivia is read by a scanner, which cannot follow a rule
that reaches itself; a comment that nests is written `Std.Nested(open, close)`, which a scanner reads
with a counter ([standard library](09-std.md)).

<!-- ru:begin -->
> **RU.** Комментарии — тоже trivia; в стандартной библиотеке есть обычные (пример выше, в том числе
> вложенный `Std.Nested("/*", "*/")`). **Заключайте trivia в скобки.** Без них неудачный разбор может
> отдавать прочитанное trivia по символу и перечитывать внутренность комментария как синтаксис. Если
> рядом со швом стоит то, что читает те же символы (поле, в котором бывают пробелы), каждая серия
> пробелов получает столько прочтений, какова её длина, и отвергнутый вход перебирается во всех
> сочетаниях — несколько кривых строк читаются секундами. Компилятор предупреждает об этой форме
> (`GRAM4051`). `trivia = { ' '* }` читает каждую серию один раз. Атомарную trivia читает сканер,
> который не следует за самоссылающимся правилом; вложенный комментарий пишется `Std.Nested`, который
> сканер читает счётчиком.
<!-- ru:end -->

## Lexemes under trivia

A rule spelled out of single characters — a number, a name — is a lexeme, and under non-empty trivia
its seams would let `1 . 5` be a number. Write such a rule where `trivia` is empty, or join its parts
with `~` (below). The compiler warns of the shape almost never meant: every operand tests one item, at
least one is a class or `any`, and a seam is woven between two of them (`GRAM4043`).

```dotgram GRAM4043
trivia = { ' '* }

Number = Std.Digit+ & ('.' & Std.Digit+)?

parse Number
```

```dotgram
trivia = { ' '* }

Number = Std.Digit+ ~ ('.' ~ Std.Digit+)?

parse Number
```

```text examples
ParseNumber("1.5") → "1.5"
ParseNumber("1 . 5") → refused
```

Notes:

- A run is not one item, so `Std.Digits & ('.' & Std.Digits)?` is not told — it reads `1 .5` all the
  same. Write numbers and strings built from runs with `~` by hand.
- Braces do not silence `GRAM4043`: they commit a reading and keep every seam.

<!-- ru:begin -->
> **RU.** Правило, составленное из одиночных символов (число, имя), — лексема, и при непустой trivia
> его швы позволили бы читать `1 . 5` как число. Пишите такое правило там, где `trivia` пуста, или
> соединяйте части `~`. Компилятор предупреждает о форме, которую почти никогда не имеют в виду: каждый
> операнд проверяет один элемент, хотя бы один — класс или `any`, и между двумя из них вплетён шов
> (`GRAM4043`, первый пример; второй, с `~`, отвергает `1 . 5`). Серия — не один элемент:
> `Std.Digits & ('.' & Std.Digits)?` не сообщается, но так же читает `1 .5` — такие правила пишите с
> `~` вручную. Скобки `GRAM4043` не гасят: они фиксируют прочтение, но швы остаются.
<!-- ru:end -->

## Trivia by namespace

`trivia` is an ordinary rule, so it is shadowed as any rule is: a namespace that declares its own reads
with it. A rule uses the `trivia` visible where it is **declared**, not where it is called.

```dotgram
using Lexical;

trivia = { ' '* }

namespace Lexical
{
	trivia = none

	Name   = ['a'..'z'] & ['a'..'z' | '0'..'9']*
	Number = ['0'..'9']+
}

Assign = Name & '=' & Number

parse Assign
```

```text examples
ParseAssign("x1 = 42") → "x1 = 42"
ParseAssign("x 1 = 42") → refused
```

Notes:

- The `trivia` visible in a namespace is looked up along the namespace's own chain first — the
  namespace, then each one around it — and only then among the namespaces it imports. Where neither
  finds one, it is the built-in `none`. So `Assign` above reads spaces: the `trivia = none` that
  `using Lexical;` brings in is shadowed by the `trivia` around it.
- Two imports at one scope that each declare a `trivia`, in a scope that declares none, are refused
  (`GRAM3022`): the order of the `using`s never decides. Declare the one meant, `trivia = A.trivia`.
- A substitution replaces `trivia` like any rule: `namespace N with (trivia = none) { … }` reuses rules
  under another whitespace ([namespaces](10-namespaces.md)).

```dotgram GRAM3022
using A;
using B;

namespace A { trivia = { ' '* } }
namespace B { trivia = { '\t'* } }

Pair = 'x' & 'y'

parse Pair
```

<!-- ru:begin -->
> **RU.** `trivia` — обычное правило и затеняется как любое: пространство имён, объявившее свою,
> читает с ней. Правило использует `trivia`, видимую там, где оно **объявлено**, а не вызвано
> (пример: `Name` и `Number` в `Lexical` без пробелов, `Assign` — с пробелами). Видимая `trivia`
> ищется сначала по цепочке самого пространства (оно и объемлющие), и лишь затем среди импортов; если
> нигде нет — встроенная `none`. Поэтому `Assign` читает пробелы: `trivia = none`, принесённая
> `using Lexical;`, затенена внешней. Два импорта на одном уровне, каждый со своей `trivia`, при
> отсутствии своей — `GRAM3022` (порядок `using` ничего не решает); объявите нужную:
> `trivia = A.trivia`. Подстановка заменяет `trivia`, как любое правило:
> `namespace N with (trivia = none) { … }`.
<!-- ru:end -->

## Adjacency: `~`

`a ~ b` is `a & b` with no seam: what `b` matches begins exactly where `a` ended. It binds tighter than
`&`, so `a & b ~ c` is `a & (b ~ c)`.

```dotgram
trivia = { ' '* }

Name  = { ['a'..'z']+ }
Shift = Name & '>' ~ '>' & Name
Type  = Name & ('<' & Type & (',' & Type)* & '>')?

parse Shift
parse Type
```

```text examples
ParseShift("x >> y") → "x >> y"
ParseShift("x > > y") → refused
ParseType("list<list<int> >") → "list<list<int> >"
ParseType("list<list<int>>") → "list<list<int>>"
```

It exists for operators that share their characters with something else: a shift is two `>` with
nothing between them, and a nested type argument list closes with two that belong to different levels.
Writing the shift as `">>"` cannot do both: a literal is one token, and with `>>` among the tokens the
inner list would be handed a shift.

Notes:

- Over tokens (`Lexical = true`) `~` asks the tokens themselves whether they touch, so a grammar reads
  the same language either way ([reading over tokens](06-lexical.md#adjacency-and-word-over-tokens)).
- Inside a namespace whose `trivia` is `none`, `~` says nothing and is allowed for that reason.

<!-- ru:begin -->
> **RU.** `a ~ b` — это `a & b` без шва: `b` начинается ровно там, где закончилось `a`. Связывает
> сильнее `&`: `a & b ~ c` — это `a & (b ~ c)`. Нужен для операторов, делящих символы с чем-то ещё:
> сдвиг — два `>` без промежутка, а вложенный список аргументов типа закрывается двумя `>` разных
> уровней (пример: `x >> y` — сдвиг, `x > > y` — отказ, `list<list<int>>` закрывается дважды).
> Литерал `">>"` так не может: литерал — один токен, и внутренний список получил бы сдвиг. Над
> токенами `~` спрашивает сами токены, соприкасаются ли они, — язык тот же. В пространстве с
> `trivia = none` `~` ничего не говорит и потому разрешён.
<!-- ru:end -->

## `wordboundary`

`wordboundary` names the characters that continue a word. It is `none` by default; once it is not
empty, every string literal **whose characters all fall in that class** is guarded on both sides: it
may not be followed by a word character, nor preceded by one.

```dotgram
trivia       = { ' '* }
wordboundary = ['a'..'z' | 'A'..'Z' | '0'..'9' | '_']

Name    = { ['a'..'z']+ }
If      = "if" & Name
Keyword = "if" | "else"
Ident   = ?!Keyword & Name

parse If
parse Ident
```

```text examples
ParseIf("if x") → "if x"
ParseIf("ifx") → refused
ParseIdent("iffy") → "iffy"
ParseIdent("if") → refused
```

With the boundary declared, `"if"` no longer matches the start of `iffy`, so `?!Keyword` lets `iffy`
through (compare [lookahead](03-composition.md#lookahead-x-and-x)). Whether a literal qualifies is
decided when the grammar is built: `"if"` gets the checks, `"("` does not. The checks come before the
trivia insertion, so what is asked is whether a letter follows the keyword, not the whitespace.

Notes:

- The boundary may be named through another rule, `wordboundary = WordChar`, and Unicode categories
  count as classes.
- A namespace whose own `trivia` is empty does not inherit a `wordboundary` declared outside: its
  literals are parts of one lexeme. One that declares its own beside its empty trivia keeps it.
- Over tokens, the lexer asks the first question of the character after a keyword's token; `1MB` is two
  tokens.

<!-- ru:begin -->
> **RU.** `wordboundary` называет символы, продолжающие слово. По умолчанию `none`; если не пуст,
> каждый строковый литерал, **все символы которого входят в этот класс**, охраняется с обеих сторон:
> за ним и перед ним не может стоять символ слова. С объявленной границей `"if"` больше не совпадает
> с началом `iffy`, и `?!Keyword` пропускает `iffy` (пример выше; сравни главу 03). Подходит ли литерал,
> решается при сборке: `"if"` получает проверки, `"("` — нет. Проверки стоят до вставки trivia:
> спрашивается, идёт ли буква сразу после ключевого слова. Границу можно задать через другое правило,
> категории Unicode считаются классами. Пространство с пустой собственной `trivia` не наследует
> внешнюю `wordboundary` (его литералы — части одной лексемы); объявившее свою рядом с пустой trivia —
> сохраняет. Над токенами лексер задаёт первый вопрос символу после токена; `1MB` — два токена.
<!-- ru:end -->

## `word`

`word` is one whole word made of what `wordboundary` says continues one, whatever the word says: a
maximal run, which cannot read half of a word however the parse backtracks.

```dotgram
trivia       = { ' '* }
wordboundary = ['A'..'Z' | 'a'..'z' | '_']

Option         = word & '=' & ['0'..'9']+
PermissionEnd  = "ON"i | "TO"i
PermissionWord = ?!PermissionEnd & word
Permission     = PermissionWord & PermissionWord* & "ON"i & word

parse Option
parse Permission
```

```text examples
ParseOption("FILLFACTOR = 80") → "FILLFACTOR = 80"
ParsePermission("VIEW DEFINITION ON t") → "VIEW DEFINITION ON t"
ParsePermission("ON t") → refused
```

It is for places where a language admits a word and does not care which: an option's name, a
permission spelled out of words, a name belonging to another catalogue. Where only some words will do,
the shape is a lookahead in front of `word`, not a list of every word the specification happened to
have.

Notes:

- `word` in a grammar with no `wordboundary` would be a run of nothing (`GRAM4019`).
- A boundary that can match nothing, `['a'..'z']?`, would make `word` a run that never ends
  (`GRAM4001`, at the boundary).
- Over tokens `word` is one test of the token kind: a keyword, or a class every character of which
  continues a word.

```dotgram GRAM4019
Name = word

parse Name
```

<!-- ru:begin -->
> **RU.** `word` — одно целое слово из символов, которые `wordboundary` считает продолжением слова,
> каким бы оно ни было: максимальная серия, которая не прочтёт пол-слова, как бы разбор ни
> откатывался. Нужен там, где язык допускает слово и не важно какое: имя опции, право из нескольких
> слов, имя из чужого каталога. Если подходят не все слова — просмотр перед `word`, а не список всех
> слов спецификации (пример `PermissionWord`). `word` без `wordboundary` — `GRAM4019`; граница,
> которая может ничего не совпасть (`['a'..'z']?`), сделала бы `word` бесконечным — `GRAM4001`. Над
> токенами `word` — одна проверка вида токена.
<!-- ru:end -->
