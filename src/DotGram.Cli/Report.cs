using System;
using System.Globalization;

using Microsoft.CodeAnalysis;

namespace DotGram.Cli;

/// <summary>Diagnostics written the way csc writes them, so that an editor's error list can read them.</summary>
static class Report
{
	/// <summary><c>path(line,col): error GRAM1234: message</c>, through any <c>#line</c>.</summary>
	public static string Format(Diagnostic diagnostic)
	{
		var severity = diagnostic.Severity switch
		{
			DiagnosticSeverity.Error   => "error",
			DiagnosticSeverity.Warning => "warning",
			_                          => "info",
		};

		var place = Place(diagnostic.Location);

		return (place.Length > 0 ? place + ": " : "") +
			severity + " " + diagnostic.Id + ": " + diagnostic.GetMessage(CultureInfo.InvariantCulture);
	}

	/// <summary><c>path(line,col)</c>, the line and column one-based, or nothing for no place.</summary>
	public static string Place(Location location)
	{
		if (location == Location.None)
			return "";

		var span = location.GetMappedLineSpan();

		if (!span.IsValid || span.Path.Length == 0)
			return "";

		return string.Create(
			CultureInfo.InvariantCulture,
			$"{span.Path}({span.StartLinePosition.Line + 1},{span.StartLinePosition.Character + 1})");
	}

	/// <summary>Writes each once, in order: two frameworks of one project report the same grammar twice.</summary>
	public static void Write(TextWriter output, IEnumerable<Diagnostic> diagnostics, Func<Diagnostic, bool> which)
	{
		var said = new HashSet<string>(StringComparer.Ordinal);

		foreach (var diagnostic in diagnostics)
			if (which(diagnostic) && said.Add(Format(diagnostic)))
				output.WriteLine(Format(diagnostic));
	}
}
