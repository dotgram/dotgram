# Text bindings: `$q` and `#q`

Some constructs close with the very text that opened them: a Lua long bracket `[==[ … ]==]`, a C# raw
string, a PostgreSQL `$tag$ … $tag$`, a heredoc, a Markdown fence. A grammar cannot list the
openings, because there are as many as there are texts. It binds the opening to a name and matches that
text again. The specification is [`syntax.md` §3.5.1](../../syntax.md#351-binding-and-back-reference).

## Binding and reference: `$q: X` and `$q`

`$q: X` records the text `X` matched under the name `q`. `$q`, later in the same alternative, matches
exactly that text again.

```dotgram
Twice = $w: ['a'..'z']+ & '-' & $w

parse Twice
```

```text examples
ParseTwice("ab-ab") → "ab-ab"
ParseTwice("ab-ac") → refused
ParseTwice("ab-abc") → refused
```

A reference compares item for item, with no regard to how the binding was read: a binding of a
case-insensitive literal that read `End` matches `End` and nothing else.

```dotgram
Block = $k: ("begin"i | "do"i) & ' ' & ['a'..'z']+ & ' ' & $k

parse Block
```

```text examples
ParseBlock("Begin x Begin") → "Begin x Begin"
ParseBlock("Begin x BEGIN") → refused
```

A binding is **not a capture**: it adds no member to the rule's type, and `=>` and `when` do not see it.
To have the text in C# too, capture it as well — the two names live apart:

```dotgram
Tagged : @string = '<' & tag: ($tag: ['a'..'z']+) & '>' & body: ['a'..'z']* & "</" & $tag & '>'
                 => @(tag + ":" + body)

parse Tagged
```

```text examples
ParseTagged("<b>bold</b>") → "b:bold"
ParseTagged("<b>bold</i>") → refused
```

Notes:

- `$q` is an ordinary operand and composes as one: `?!$q`, `$q*`, `r: $q`, `($q & X)`. `$q*` is refused
  where `q` may be empty, as `('a'?)*` is (`GRAM4001`).
- A binding stands where a capture stands, so a quantifier after it repeats the binding: `$q: '"'{3,}`
  is `($q: '"'){3,}`, which binds the whole run, joined.
- The sigil and the name are one token: `$ q` is `GRAM1011`.

```dotgram GRAM1011
Bad = $ q: 'a'
```

<!-- ru:begin -->
> **RU.** Некоторые конструкции закрываются тем же текстом, которым открылись (длинная скобка Lua,
> сырая строка C#, `$tag$` PostgreSQL, heredoc, ограждение Markdown); перечислить открытия нельзя,
> поэтому грамматика привязывает открытие к имени и сравнивает с ним снова. `$q: X` запоминает текст,
> прочитанный `X`, под именем `q`; `$q` дальше в той же альтернативе совпадает ровно с этим текстом
> (пример `Twice`). Ссылка сравнивает элемент за элементом и не учитывает, как читалась привязка:
> привязка регистронезависимого литерала, прочитавшая `Begin`, совпадает только с `Begin` (пример
> `Block`). Привязка — **не захват**: не добавляет члена в тип правила, её не видят `=>` и `when`;
> чтобы получить текст и в C#, захватите его тоже — `tag: ($tag: …)`, имена живут раздельно (пример
> `Tagged`). `$q` — обычный операнд: `?!$q`, `$q*`, `r: $q`, `($q & X)`; `$q*` при возможно пустом `q`
> отвергается (`GRAM4001`). Привязка стоит там, где захват, поэтому квантор после неё повторяет
> привязку: `$q: '"'{3,}` — это `($q: '"'){3,}`, привязка всей серии. Сигил и имя — один токен:
> `$ q` — `GRAM1011`.
<!-- ru:end -->

## Counting by a length: `X{#q}`

`#q` is the length of the text bound to `q`, in input items, and stands only inside count braces.

| Written | Repeats `X` |
| --- | --- |
| `X{#q}` | exactly as many times as `q` is long |
| `X{#q,}` | at least that many |
| `X{n,#q}` | from `n` to that many |
| `X{#q,m}` | from that many to `m` |
| `X{#q,#r}` | from the length of `q` to the length of `r` |

In reStructuredText the underline of a title is at least as long as the title:

```dotgram
Heading = $t: [^ '\r' | '\n']+ & eol & '='{#t,} & eol

parse Heading
```

```text examples
ParseHeading("Intro\n=====\n") → "Intro\n=====\n"
ParseHeading("Intro\n=======\n") → "Intro\n=======\n"
ParseHeading("Intro\n===\n") → refused
```

Notes:

- The count is read when it is reached. There is no arithmetic: `{#q-1}` is not a count.
- A count whose lower bound comes out larger than its upper one simply fails when it is read; the
  lengths are known only then, so it is not a build error.
- A quantifier binds tighter than a lookahead: `?!'}'{#d}` is `(?!'}'){#d}`. To look ahead for the whole
  count, write `?!('}'{#d})`.

```dotgram GRAM2010
Bad = $q: 'a'+ & #q
```

`#q` outside count braces is `GRAM2010`: to match the text itself, write `$q`.

<!-- ru:begin -->
> **RU.** `#q` — длина текста, привязанного к `q`, в элементах входа; стоит только внутри фигурных
> скобок счётчика (таблица форм: `{#q}`, `{#q,}`, `{n,#q}`, `{#q,m}`, `{#q,#r}`). Пример: в
> reStructuredText подчёркивание заголовка не короче заголовка. Счётчик читается в момент
> достижения; арифметики нет (`{#q-1}` — не счётчик); нижняя граница больше верхней просто не
> совпадает при чтении (длины известны только тогда), ошибкой сборки это не является. Квантор
> связывает сильнее просмотра: `?!'}'{#d}` — это `(?!'}'){#d}`; для просмотра всей серии —
> `?!('}'{#d})`. `#q` вне скобок счётчика — `GRAM2010`; чтобы сравнить сам текст, пишите `$q`.
<!-- ru:end -->

## Where a binding can be read

A reference sees a binding to its left in the same alternative of the same rule — through groups,
atomic groups and repetitions, and from inside a lookahead. It does not see one in a sibling of `|`, in a
rule it calls or that calls it, or across a constructing group. **Every route** to a reference must have
bound its name; two alternatives that both bind it share it:

```dotgram
Quoted = ('\'' & $t: Word & '\'' | $t: Word) & ' ' & $t
Word   = ['a'..'z']+

parse Quoted
```

```text examples
ParseQuoted("'ab' ab") → "'ab' ab"
ParseQuoted("ab ab") → "ab ab"
ParseQuoted("ab ac") → refused
```

What a binding holds when what it bound matched nothing follows the line a text capture draws:

| Written | After zero turns |
| --- | --- |
| `$q: (X?)`, `$q: X*`, `$q: (X{m,n})` | `""`, the text of no items |
| `($q: X)*`, `($q: X){m,n}` | `""` too: a repetition of a binding is a binding of the repetition |
| `($q: X)?` | nothing: the route that skipped it never bound `q` |

```dotgram
Dollar = '$' & $tag: (['a'..'z']*) & '$' & ['a'..'z' | ' ']* & '$' & $tag & '$'

parse Dollar
```

```text examples
ParseDollar("$$ a b $$") → "$$ a b $$"
ParseDollar("$x$ a b $x$") → "$x$ a b $x$"
```

Refused:

```dotgram GRAM4032
Opt = ($q: 'a')? & $q

parse Opt
```

```dotgram GRAM4032
Turns = ($q: 'a' & ',')* & $q

parse Turns
```

```dotgram GRAM3021
NoBinding = 'a' & $q

parse NoBinding
```

```dotgram GRAM4033
Inside = ?=($q: 'a') & 'a'

parse Inside
```

```dotgram GRAM4033
Twice = $q: 'a' & $q: 'b' & $q

parse Twice
```

- After `($q: X)?` the skipping route bound nothing (`GRAM4032`): write `$q: (X?)`, which binds `""`
  there.
- A repeated binding whose turns hold more than it, `($q: X & ',')*`, is read only within its turn; after
  the repetition bind the whole run, `$q: (X & ',')*` (`GRAM4032`).
- A name no binding of the rule gives is `GRAM3021`. A rule that needs another rule's text receives it as
  a capture and compares with a guard.
- A binding inside a lookahead would bind nothing, since the lookahead gives back what it read; one name
  bound twice on one route, a reference inside its own binding, and `$q` where a value goes are refused
  too (`GRAM4033`).

<!-- ru:begin -->
> **RU.** Ссылка видит привязку слева от себя в той же альтернативе того же правила — сквозь группы,
> атомарные группы, повторения и изнутри просмотра; не видит в соседней ветке `|`, в вызываемом или
> вызывающем правиле, через конструирующую группу. **Каждый путь** к ссылке обязан привязать имя; две
> альтернативы, обе привязывающие его, делят привязку (пример `Quoted`). Что держит привязка, когда
> привязанное ничего не прочитало (таблица): `$q: (X?)`, `$q: X*`, `$q: (X{m,n})` — `""`;
> `($q: X)*` — тоже `""` (повторение привязки есть привязка повторения); `($q: X)?` — ничего: путь,
> который его пропустил, `q` не привязал. Пример `Dollar` — `$$ … $$` и `$x$ … $x$` одним правилом.
> Отвергается: ссылка после `($q: X)?` — `GRAM4032` (пишите `$q: (X?)`); повторённая привязка, шаг
> которой больше неё (`($q: X & ',')*`), читается только внутри шага — `GRAM4032` (привяжите всю серию);
> имя без привязки в правиле — `GRAM3021` (текст другого правила передаётся захватом и сравнивается
> охранником); привязка внутри просмотра, повторная привязка одного имени на пути, ссылка внутри своей
> привязки, `$q` там, где ожидается значение, — `GRAM4033`.
<!-- ru:end -->

## Backtracking, and making a binding whole

Backtracking is as in a regular expression: when a greedy binding gives an item back, the references
after it are tried again with the shorter text. Where the opening is meant as a whole, write it atomic.

A C# raw string opens with three quotes or more and closes with the same number:

```dotgram
RawString = { $q: '"'{3,} } & { (?!$q & any)* } & $q & ?!'"'

parse RawString
```

```text examples
ParseRawString("\"\"\"a\"b\"\"\"") → "\"\"\"a\"b\"\"\""
ParseRawString("\"\"\"\"a\"\"\"\"") → "\"\"\"\"a\"\"\"\""
ParseRawString("\"\"\"\"a\"\"\"") → refused
```

The braces around the opening say that every opening quote belongs to it: `""""a"""` is not a
three-quote string that begins with a quote. The last `?!'"'` refuses a closing run longer than the
opening.

<!-- ru:begin -->
> **RU.** Откат — как в регулярных выражениях: когда жадная привязка отдаёт элемент, ссылки после неё
> пробуются снова с более коротким текстом. Если открытие задумано целым — делайте его атомарным.
> Пример: сырая строка C# открывается тремя и более кавычками и закрывается тем же числом; скобки
> вокруг открытия говорят, что все открывающие кавычки принадлежат ему (`""""a"""` — не строка из трёх
> кавычек, начинающаяся с кавычки), а `?!'"'` в конце отвергает закрытие длиннее открытия.
<!-- ru:end -->

## Recipes

Each recognizes the token; what is made of it — unindenting, unescaping — is the rule's `=>` or C#'s.

A **Lua long bracket**:

```dotgram
LongString = '[' & $eq: '='* & '[' & { (?!(']' & $eq & ']') & any)* } & ']' & $eq & ']'

parse LongString
```

```text examples
ParseLongString("[[a]]") → "[[a]]"
ParseLongString("[==[a]]b]==]") → "[==[a]]b]==]"
ParseLongString("[==[a]=]") → refused
```

**Rust raw strings**, `#` for the quotes:

```dotgram
RustRaw = 'r' & $h: '#'* & '"' & { (?!('"' & $h) & any)* } & '"' & $h

parse RustRaw
```

```text examples
ParseRustRaw("r#\"a\"b\"#") → "r#\"a\"b\"#"
ParseRustRaw("r\"ab\"") → "r\"ab\""
```

A **heredoc** whose body starts on the next line:

```dotgram
Heredoc = "<<" & $tag: Word & eol & { (?!($tag & EndLine) & Line)* } & $tag & ?=EndLine
Word    = ['A'..'Z' | 'a'..'z' | '_']+
Line    = [^ '\r' | '\n']+ & EndLine | eol
EndLine = eol | eof

find Heredoc as Heredocs
```

```text examples
Heredocs("x = <<END\nline 1\nline 2\nEND\n") → ["<<END\nline 1\nline 2\nEND"]
```

A **Markdown fence**: the closing fence is the same character and at least as long, which is `$f`
followed by more of the same — no comparison needed:

```dotgram
Fenced  = ' '{0,3} & $f: '`'{3,} & [^ '`' | '\r' | '\n']* & eol
          & { (?!(' '{0,3} & $f & '`'* & ' '* & EndLine) & Line)* }
          & (' '{0,3} & $f & '`'* & ' '* & EndLine | eof)
Line    = [^ '\r' | '\n']+ & EndLine | eol
EndLine = eol | eof

parse Fenced
```

```text examples
ParseFenced("```cs\nx\n```\n") → "```cs\nx\n```\n"
ParseFenced("````\nx\n```\n````") → "````\nx\n```\n````"
ParseFenced("```\nnever closed\n") → "```\nnever closed\n"
```

Notes:

- Where the closing begins with a delimiter character of its own (Lua, heredoc), a position that does not
  begin a closing fails at its first item and the scan is linear. For `(?!$q & any)*` every quote compares
  the binding with the text there: the worst case is the text's length times the binding's, which a crafted
  input can make as slow as its opening is long.
- Read from a `TextReader` or a stream, a binding holds the input from where it began until its rule is
  done with it; over bytes, `#q` counts bytes.

<!-- ru:begin -->
> **RU.** Рецепты (каждый распознаёт токен; что из него сделать — дело `=>` или C#): длинная скобка Lua,
> сырые строки Rust (`#` вместо кавычек), heredoc с телом со следующей строки, ограждение Markdown
> (закрывающее — тот же символ и не короче, т. е. `$f` и ещё таких же — сравнение не нужно; незакрытое
> тянется до конца входа). Если закрытие начинается своим символом-разделителем (Lua, heredoc), позиция,
> не начинающая закрытие, отвергается на первом элементе — просмотр линеен. Для `(?!$q & any)*` каждая
> кавычка сравнивает привязку с текстом: в худшем случае длина текста на длину привязки, и подобранный
> вход делает это настолько медленным, насколько длинно открытие. При чтении из `TextReader` или потока
> привязка держит вход от своего начала до конца правила; над байтами `#q` считает байты.
<!-- ru:end -->

## Bindings in a grammar split into tokens

A binding compares text, so it is read over characters only. In a grammar cut into a lexer and a parser
(`Lexical = true`) a terminal of the lexical half may bind: when its body is braced and no other token
can match a longer text that begins like it, the lexer starts it on its regular beginning and measures it
with its own rule.

```dotgram Lexical
namespace Lex
{
	trivia = none

	LongString = { '[' & $eq: '='* & '[' & (?!(']' & $eq & ']') & any)* & ']' & $eq & ']' }
	Name       = ['a'..'z']+
}

trivia = { ' '* }

Assign = Lex.Name & '=' & Lex.LongString

parse Assign
```

```text examples
ParseAssign("s = [=[a]]b]=]") → "s = [=[a]]b]=]"
```

In the syntactic half a binding would bind token kinds, not text, and is refused: compare two captures
with a guard instead, `close: Name & when @(close == open)` (`GRAM4035`).

```dotgram Lexical GRAM4035
namespace Lex
{
	trivia = none

	Name = ['a'..'z']+
}

trivia = { ' '* }

Pair = $n: Lex.Name & '=' & $n

parse Pair
```

Notes:

- Where the lexer cannot read such a terminal as one token, that part of the grammar is read over
  characters and `GRAM5004` says which token stood in the way and what to write.
- Not yet supported, and refused with a message saying so (`GRAM4034`): a binding inside a repetition
  that recovers, a binding in what `trivia` is made of once a grammar is split, and a binding terminal that
  begins with nothing an automaton reads.

<!-- ru:begin -->
> **RU.** Привязка сравнивает текст и потому читается только над символами. В грамматике, разделённой
> на лексер и парсер (`Lexical = true`), привязывать может терминал лексической половины: если его тело
> в скобках и никакой другой токен не может совпасть с более длинным текстом с того же начала, лексер
> начинает его по регулярному началу и измеряет его собственным правилом (пример `Assign` с длинной
> строкой Lua). В синтаксической половине привязка привязывала бы виды токенов, а не текст, и
> отвергается: сравнивайте два захвата охранником, `close: Name & when @(close == open)` (`GRAM4035`).
> Если лексер не может прочитать такой терминал одним токеном, эта часть читается над символами, и
> `GRAM5004` говорит, какой токен мешает и что написать. Пока не поддерживается (`GRAM4034`): привязка
> в восстанавливающемся повторении, в составе `trivia` разделённой грамматики и терминал с привязкой,
> который начинается не с того, что читает автомат.
<!-- ru:end -->
