using System;
using System.Collections.Generic;
using System.Text;

namespace DotGram.Grammar.Emit;

/// <summary>
/// The documentation comments of a generated file kept only where somebody outside the file can
/// read them: on a member a consumer's code could see from another assembly.
/// </summary>
/// <remarks>
/// <para>
/// The emitter documents what it writes as it writes it, which is how it is read: the comment beside
/// a reader's part says what the part is. In the consumer's compilation the same words on a private
/// member are text nobody sees in an editor and the compiler parses on every build. A public member's
/// comment is what an editor shows over the call, and the C# compiler asks for it (CS1591) where the
/// consumer documents their assembly; everything else loses its comment here, in one place, rather
/// than at each of the places that write one.
/// </para>
/// <para>
/// A pass over lines, which is enough because the text is the emitter's own: one declaration a line,
/// and the types it stands in told by the braces open around it. The author's own C#, which stands
/// between a <c>#line</c> directive and the <c>#line default</c> after it, is left as written.
/// </para>
/// <para>
/// A type is visible where it says <c>public</c> (or <c>protected</c>) and whatever holds it is visible
/// too. A <c>partial</c> type that says nothing takes its accessibility from the author's declaration,
/// which is not in the file, so it is taken to be visible: the host's class is one, and dropping its
/// comments would be dropping the documentation of the public parser.
/// </para>
/// </remarks>
static class PublicDocs
{
	/// <summary><paramref name="text"/> without the documentation of members no other assembly sees.</summary>
	public static string Keep(string text)
	{
		var output = new StringBuilder(text.Length);
		var blocks = new List<Block>();
		var docs   = new List<string>();
		var user   = false;
		var at     = 0;

		// A type declared, waiting for the brace that opens it.
		Block? declared = null;

		while (at < text.Length)
		{
			var end  = text.IndexOf('\n', at);
			var next = end < 0 ? text.Length : end + 1;
			var line = text.Substring(at, next - at);
			var code = line.Trim();

			at = next;

			// The author's C#, between the directive that maps it to the grammar and the one that
			// ends the mapping: whatever it documents, it documents for the author.
			if (code.StartsWith("#line ", StringComparison.Ordinal))
				user = !code.StartsWith("#line default", StringComparison.Ordinal) && !code.StartsWith("#line hidden", StringComparison.Ordinal);

			if (user)
			{
				Flush(output, docs, true);
				output.Append(line);
				Braces(code, blocks, ref declared);

				continue;
			}

			// A comment is held until what it documents: past attributes, directives and blank
			// lines, which the compiler reads between the two as well.
			if (code.StartsWith("///", StringComparison.Ordinal) ||
				docs.Count > 0 && (code.Length == 0 || code[0] is '[' or '#'))
			{
				docs.Add(line);

				continue;
			}

			var holder  = Holder(blocks);
			var words   = code.Split([' ', '\t', '('], StringSplitOptions.RemoveEmptyEntries);
			var type    = TypeKeyword(words);
			var visible = Visible(words, holder.Visible, holder.Listed, type is not null);

			Flush(output, docs, visible);

			if (type is not null)
				declared = new Block(true, visible, type is "interface" or "enum");

			output.Append(line);
			Braces(code, blocks, ref declared);
		}

		Flush(output, docs, true);

		return output.ToString();
	}

	/// <summary>A block open where a line is read: whether it is a type's body, and if so, whether the type is seen outside.</summary>
	/// <param name="Listed">Whether the type is an interface or an enum, whose members are as visible as it is.</param>
	readonly record struct Block(bool Type, bool Visible, bool Listed);

	/// <summary>The type whose body a line stands in, or the file's own level, which is seen.</summary>
	static Block Holder(List<Block> blocks)
	{
		for (var i = blocks.Count - 1; i >= 0; i--)
			if (blocks[i].Type)
				return blocks[i];

		return new Block(true, true, false);
	}

	/// <summary>
	/// The blocks a line opens and closes, counted by its braces outside strings, characters and
	/// comments: the first it opens after a type's declaration is that type's body.
	/// </summary>
	/// <remarks>
	/// By braces rather than by indentation, which the emitter does not hold to everywhere: a
	/// field written under a <c>#pragma</c> stands a level out from its neighbours.
	/// </remarks>
	static void Braces(string code, List<Block> blocks, ref Block? declared)
	{
		for (var i = 0; i < code.Length; i++)
		{
			var c = code[i];

			if (c == '/' && i + 1 < code.Length && code[i + 1] == '/')
				return;

			if (c is '"' or '\'')
			{
				var verbatim = c == '"' && i > 0 && (code[i - 1] == '@' || i > 1 && code[i - 1] == '$' && code[i - 2] == '@');

				for (i++; i < code.Length && code[i] != c; i++)
					if (code[i] == '\\' && !verbatim)
						i++;

				continue;
			}

			if (c == '{')
			{
				blocks.Add(declared ?? new Block(false, false, false));
				declared = null;
			}
			else if (c == '}' && blocks.Count > 0)
			{
				blocks.RemoveAt(blocks.Count - 1);
			}
		}
	}

	/// <summary>A comment held back, written or left out, and its attributes written either way.</summary>
	static void Flush(StringBuilder output, List<string> docs, bool keep)
	{
		foreach (var line in docs)
			if (keep || !line.TrimStart().StartsWith("///", StringComparison.Ordinal))
				output.Append(line);

		docs.Clear();
	}

	/// <summary>The keyword that makes a line a type's declaration, or null where it is not one.</summary>
	static string? TypeKeyword(string[] words)
	{
		foreach (var word in words)
			switch (word)
			{
				case "public" or "internal" or "private" or "protected" or "static" or "sealed" or "abstract" or
					"readonly" or "ref" or "partial" or "unsafe" or "new":
					continue;

				case "class" or "struct" or "interface" or "enum":
					return word;

				default:
					return null;
			}

		return null;
	}

	/// <summary>Whether a declaration is seen from another assembly, given whether what holds it is.</summary>
	/// <param name="listed">Whether what holds it is an interface or an enum, whose members are as visible as it is.</param>
	static bool Visible(string[] words, bool held, bool listed, bool type)
	{
		if (!held)
			return false;

		if (listed)
			return true;

		var partial = false;

		foreach (var word in words)
			switch (word)
			{
				case "public":
					return true;

				// `private protected` is seen by nothing outside the assembly; `protected` alone is.
				case "private" or "internal":
					return false;

				case "protected":
					return !Array.Exists(words, static one => one is "private");

				case "partial":
					partial = true;
					break;
			}

		return type && partial;
	}
}
