using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

using DotGram.Grammar;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotGram.Cli;

/// <summary>
/// A <c>.gram</c> input: the host class a project would have written for it, synthesized from the
/// command line, with <c>--option Name=value</c> as the attribute's named arguments.
/// </summary>
static class GrammarHost
{
	/// <summary>One named property of <c>[Gram]</c>, as the generator's own attribute declares it.</summary>
	/// <param name="Type">The property's type as written there: <c>bool</c>, <c>int</c>, <c>string?</c>, <c>GramCarrier</c>, <c>global::System.Type?</c>.</param>
	/// <param name="Members">For an enum, its members; empty otherwise.</param>
	public sealed record Option(string Name, string Type, IReadOnlyList<string> Members);

	/// <summary>
	/// Every settable property of <c>[Gram]</c> and the class it derives from, read from the text the
	/// generator itself adds to every compilation — so this list is the package's, never a copy of it.
	/// </summary>
	public static IReadOnlyList<Option> Options()
	{
		var tree    = CSharpSyntaxTree.ParseText(GramCompiler.EmitMarkerAttributes().Text);
		var root    = tree.GetCompilationUnitRoot();
		var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>().ToDictionary(static one => one.Identifier.ValueText);
		var enums   = root.DescendantNodes().OfType<EnumDeclarationSyntax>().ToDictionary(
			static one => one.Identifier.ValueText,
			static one => (IReadOnlyList<string>)[.. one.Members.Select(static member => member.Identifier.ValueText)]);

		var options = new List<Option>();

		foreach (var name in new[] { "GramOptionsAttribute", "GramAttribute" })
		{
			if (!classes.TryGetValue(name, out var declaration))
				continue;

			foreach (var property in declaration.Members.OfType<PropertyDeclarationSyntax>())
			{
				if (property.AccessorList?.Accessors.Any(static accessor => accessor.IsKind(SyntaxKind.SetAccessorDeclaration)) != true ||
					!property.Modifiers.Any(SyntaxKind.PublicKeyword))
					continue;

				var type = property.Type.ToString();

				options.Add(new Option(property.Identifier.ValueText, type, enums.TryGetValue(type, out var members) ? members : []));
			}
		}

		return options;
	}

	/// <summary>The named argument <c>Name = value</c> the option is written as, its value checked against its type.</summary>
	/// <exception cref="ToolException">An option <c>[Gram]</c> does not have, or a value its type does not take.</exception>
	public static string Argument(string name, string value, IReadOnlyList<Option> options)
	{
		var option = options.FirstOrDefault(one => string.Equals(one.Name, name, StringComparison.OrdinalIgnoreCase))
			?? throw ToolException.Usage($"[Gram] has no option '{name}'. It has: {Listed(options)}.");

		string Bad(string why)
		{
			throw ToolException.Usage($"--option {option.Name}={value}: {why}. [Gram] takes: {Listed(options)}.");
		}

		var type = option.Type.TrimEnd('?');

		string text;

		if (option.Members.Count > 0)
		{
			var member = option.Members.FirstOrDefault(one => string.Equals(one, value.Trim(), StringComparison.OrdinalIgnoreCase))
				?? Bad($"not one of {string.Join(", ", option.Members)}");

			text = "global::DotGram." + type + "." + member;
		}
		else
			text = type switch
			{
				"bool"   => bool.TryParse(value.Trim(), out var flag) ? (flag ? "true" : "false") : Bad("not true or false"),
				"int"    => int.TryParse(value.Trim(), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var number)
					? number.ToString(CultureInfo.InvariantCulture)
					: Bad("not a whole number"),
				"string" => SymbolDisplay.FormatLiteral(value, quote: true),
				"global::System.Type" or "System.Type" or "Type" => "typeof(" + TypeName(value) + ")",
				_        => Bad($"the tool cannot write a value of type {option.Type}"),
			};

		return option.Name + " = " + text;

		string TypeName(string written)
		{
			var parsed = SyntaxFactory.ParseTypeName(written.Trim());

			if (parsed.ContainsDiagnostics || parsed.FullSpan.Length != written.Trim().Length || written.Trim().Length == 0)
				Bad("not a C# type name");

			return parsed.ToString();
		}
	}

	static string Listed(IReadOnlyList<Option> options)
	{
		return string.Join(", ", options.Select(static option =>
			option.Name + " (" + (option.Members.Count > 0 ? string.Join("|", option.Members) : option.Type.Replace("global::", "")) + ")"));
	}

	/// <summary>The class name a grammar's host gets: <c>--class</c>, else the file's name.</summary>
	public static string ClassName(GenerateOptions options)
	{
		var name = options.ClassName ?? Path.GetFileNameWithoutExtension(options.Input);

		if (!SyntaxFacts.IsValidIdentifier(name) || SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None)
			throw ToolException.Usage(
				options.ClassName is null
					? $"the file name '{name}' is not a C# class name; pass --class <Name>."
					: $"--class '{name}' is not a C# class name.");

		return name;
	}

	/// <summary>The host's file: <c>[Gram("file.gram", options)] partial class Name</c> in its namespace.</summary>
	public static string Text(GenerateOptions options, string className, IReadOnlyList<string> arguments)
	{
		var @namespace = options.Namespace;

		if (@namespace is not null && !@namespace.Split('.').All(static part => SyntaxFacts.IsValidIdentifier(part) && SyntaxFacts.GetKeywordKind(part) == SyntaxKind.None))
			throw ToolException.Usage($"--namespace '{@namespace}' is not a C# namespace.");

		var attribute = new StringBuilder("[global::DotGram.Gram(")
			.Append(SymbolDisplay.FormatLiteral(Path.GetFileName(options.Input), quote: true));

		foreach (var argument in arguments)
			attribute.Append(", ").Append(argument);

		attribute.Append(")]");

		var declaration = (options.Public ? "public" : "internal") + " partial class " + className;
		var text        = new StringBuilder();

		text.Append("// <auto-generated/>\r\n");
		text.Append("// The host class of ").Append(Path.GetFileName(options.Input)).Append(", written by dotnet dotgram generate.\r\n");

		if (@namespace is null)
		{
			text.Append(attribute).Append("\r\n");
			text.Append(declaration).Append("\r\n{\r\n}\r\n");
		}
		else
		{
			text.Append("namespace ").Append(@namespace).Append("\r\n{\r\n");
			text.Append('\t').Append(attribute).Append("\r\n");
			text.Append('\t').Append(declaration).Append("\r\n\t{\r\n\t}\r\n");
			text.Append("}\r\n");
		}

		return text.ToString();
	}

	static readonly EnumerationOptions Every = new() { RecurseSubdirectories = true, IgnoreInaccessible = true, AttributesToSkip = 0 };

	/// <summary>The C# files <c>--sources</c> names: files, or globs with <c>*</c>, <c>?</c> and <c>**</c>.</summary>
	public static List<string> Sources(IEnumerable<string> patterns)
	{
		var files = new List<string>();

		foreach (var pattern in patterns)
		{
			if (pattern.IndexOfAny(['*', '?']) < 0)
			{
				if (!File.Exists(pattern))
					throw ToolException.Failure($"--sources '{pattern}' does not exist.");

				files.Add(pattern);
				continue;
			}

			var separators = new[] { '/', '\\' };
			var parts      = pattern.Split(separators);
			var fixedParts = parts.TakeWhile(static part => part.IndexOfAny(['*', '?']) < 0).ToArray();
			var root       = string.Join(Path.DirectorySeparatorChar, fixedParts);

			if (root.Length == 0)
				root = Path.GetPathRoot(pattern) ?? ".";

			if (!Directory.Exists(root))
				throw ToolException.Failure($"--sources '{pattern}': {root} does not exist.");

			var expression = new Regex(
				"^" + string.Join("/", parts.Skip(fixedParts.Length).Select(static part => part == "**"
					? "\u0001"
					: Regex.Escape(part).Replace("\\*", "[^/]*").Replace("\\?", "[^/]")))
					.Replace("\u0001/", "(?:.*/)?").Replace("\u0001", ".*") + "$",
				RegexOptions.CultureInvariant | (OperatingSystem.IsWindows() ? RegexOptions.IgnoreCase : RegexOptions.None));

			// A folder that cannot be read is passed over, as MSBuild's globs pass it over; hidden ones are
			// looked into, as they were before (the options' default would skip them).
			var matched = Directory.EnumerateFiles(root, "*", Every)
				.Where(file => expression.IsMatch(Path.GetRelativePath(root, file).Replace('\\', '/')))
				.Order(StringComparer.Ordinal)
				.ToList();

			if (matched.Count == 0)
				throw ToolException.Failure($"--sources '{pattern}' matches no file.");

			files.AddRange(matched);
		}

		return [.. files.Distinct(OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal)];
	}

	/// <summary>
	/// A class in the sources whose own <c>[Gram]</c> claims this grammar: a second host, which the
	/// synthesized one would collide with. A <c>partial</c> declaration of the host without a
	/// <c>[Gram]</c> — the members its hooks call — is not one.
	/// </summary>
	public static string? SecondHost(IEnumerable<SyntaxTree> sources, string grammarFileName)
	{
		foreach (var tree in sources)
			foreach (var attribute in tree.GetRoot().DescendantNodes().OfType<AttributeSyntax>())
			{
				var name = attribute.Name.ToString();

				if (name is not ("Gram" or "GramAttribute" or "DotGram.Gram" or "DotGram.GramAttribute" or "global::DotGram.Gram" or "global::DotGram.GramAttribute"))
					continue;

				var first = attribute.ArgumentList?.Arguments.FirstOrDefault(static argument => argument.NameEquals is null);

				// `[Gram]` alone takes the grammar named after the class.
				var claims = first is null
					? attribute.Parent?.Parent is ClassDeclarationSyntax owner &&
						string.Equals(owner.Identifier.ValueText + ".gram", grammarFileName, StringComparison.OrdinalIgnoreCase)
					: first.Expression is LiteralExpressionSyntax { Token.Value: string source } &&
						(source == grammarFileName || source.EndsWith("/" + grammarFileName, StringComparison.Ordinal) || source.EndsWith("\\" + grammarFileName, StringComparison.Ordinal));

				if (claims)
					return Report.Place(attribute.GetLocation());
			}

		return null;
	}
}
