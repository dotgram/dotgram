Since 0.2.0. The language reads more of C#'s initializers, with, target-typed new and nullable types, refuses what C# refuses in them, and parses faster.

BREAKING CHANGES, and what to do

- Initializers are read as C# reads them, so texts 0.2.0 accepted are refused: an entry of a member initializer that is not Name = … (CS0747); an assignment as an element of a collection initializer, { 1, x = 2 }, { b.Capacity = 2 }, { x += 1 } - 0.2.0 read x = … as an assignment to a variable and added it (write the assignment in parentheses, { (x = 1), 2 }, to keep it an element); a member set twice in one initializer (CS1912); an initializer of the members of a property of a value type (CS1918); a collection initializer on a type that is not IEnumerable (CS1922); an initializer that assigns through a setter the text cannot call or a property with none (CS0200) or a read-only field (CS0191); an object creation that leaves a required member unset (CS9035).
- A switch expression with no natural type as the operand of a plain operator (1 + (x switch { … }), !(x switch { … })) is refused, naming the two arms that meet in nothing (CS8506), where it reached the operator's factory with no type. String concatenation still takes it, boxed to object.
- A member's own initializer, Items = { 1, 2 }, reads the member once for each entry inside it, as C# does, where it read it once for all; the tree is then a block of assignments, not a MemberInit.
- The public type ExpressionParser.ParserInput<T> is removed; nothing public took or returned it.

NEW

- A trailing comma after the last entry of an object, collection or array initializer; empty braces as an object initializer that sets nothing (new List<int> { }, Items = { }); indexers among members (new Dictionary<int, int> { [1] = 2 }), built as a block of assignments in C#'s order.
- new[] { 1, 2L }, typed by the one type its elements convert to (CS0826 where there is none).
- x with { Name = value, … } over a record class or a struct (CS8858 for any other type); with stays a name a variable may have.
- new(…) with its type left out, wherever the place it stands in says the type: a declaration, a member or element of an initializer, an argument, a return, a cast, a throw, a ?: typed by its place.
- T? as System.Nullable<T> in every type position; string? is string; int?? is refused (CS8629). After is and as, a bare ? is read as nullable only where what follows could not begin an expression, as C# reads it.
- A switch with no natural type converts to object as to any other target.

FASTER

- The parser reads with the immediate carrier, building the tree as it reads: accepted text parses about 30 to 55% faster (deeply nested text the most) and the generated parser is about 9% smaller. A text refused late is about 30% slower and allocates more; one refused early costs what it did. The answers - tree, refusal, message, exception - are unchanged.
- Member initializers nested and never closed are read once at every level, where they were read twice (three quarters of a second at seventeen levels). A string left unclosed inside an interpolated string's hole, $"{x", is refused at once, where the time doubled with every further character.

KNOWN ISSUES

- A lambda is never converted to an expression-tree type (Expression<TDelegate>): over an IQueryable, LINQ operators that take a lambda bind to Enumerable and run in memory, and Queryable's methods written out are refused.