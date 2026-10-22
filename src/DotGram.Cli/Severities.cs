using System;
using System.Collections.Immutable;
using System.Globalization;

using Microsoft.CodeAnalysis;

namespace DotGram.Cli;

/// <summary>
/// The severity the build gives a diagnostic of the generator, from the configuration files and the
/// compiler's <c>/nowarn</c> and <c>/warnaserror</c>.
/// </summary>
static class Severities
{
	/// <summary>The diagnostics at the severities the build would report them at, those it suppresses dropped.</summary>
	public static ImmutableArray<Diagnostic> Apply(ImmutableArray<Diagnostic> diagnostics, ConfiguredOptions configured, Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions options)
	{
		var result = ImmutableArray.CreateBuilder<Diagnostic>(diagnostics.Length);

		foreach (var diagnostic in diagnostics)
		{
			var (forFile, global) = configured.SeveritiesFor(TreePathOf(diagnostic));

			// The compiler's own order: /nowarn and /warnaserror:id first, then the sections that apply to the
			// file, then the global files. A warning is then an error where /warnaserror asks for all of them,
			// the severity a configuration file gave it included; /warnaserror-:id is the one way to keep it.
			var specific = options.SpecificDiagnosticOptions.TryGetValue(diagnostic.Id, out var report);

			if (!specific && !forFile.TryGetValue(diagnostic.Id, out report) && !global.TryGetValue(diagnostic.Id, out report))
				report = ReportDiagnostic.Default;

			if (options.GeneralDiagnosticOption == ReportDiagnostic.Error && !specific &&
				(report == ReportDiagnostic.Warn || (report == ReportDiagnostic.Default && diagnostic.Severity == DiagnosticSeverity.Warning)))
				report = ReportDiagnostic.Error;

			switch (report)
			{
				case ReportDiagnostic.Suppress:
					continue;

				case ReportDiagnostic.Error:
					result.Add(With(diagnostic, DiagnosticSeverity.Error));
					break;

				case ReportDiagnostic.Warn:
					result.Add(With(diagnostic, DiagnosticSeverity.Warning));
					break;

				case ReportDiagnostic.Info:
					result.Add(With(diagnostic, DiagnosticSeverity.Info));
					break;

				case ReportDiagnostic.Hidden:
					result.Add(With(diagnostic, DiagnosticSeverity.Hidden));
					break;

				default:
					result.Add(diagnostic);
					break;
			}
		}

		return result.ToImmutable();
	}

	/// <summary>
	/// The path of the C# source the diagnostic is in, or empty. The sections of an <c>.editorconfig</c>
	/// apply to syntax trees only: a diagnostic in a grammar file, which is not one, takes the severity of
	/// the global files and the compiler's options alone.
	/// </summary>
	static string TreePathOf(Diagnostic diagnostic)
	{
		return diagnostic.Location.SourceTree?.FilePath ?? "";
	}

	static Diagnostic With(Diagnostic diagnostic, DiagnosticSeverity severity)
	{
		if (diagnostic.Severity == severity)
			return diagnostic;

		return Diagnostic.Create(
			diagnostic.Id,
			diagnostic.Descriptor.Category,
			diagnostic.GetMessage(CultureInfo.InvariantCulture),
			severity,
			diagnostic.DefaultSeverity,
			diagnostic.Descriptor.IsEnabledByDefault,
			severity == DiagnosticSeverity.Error ? 0 : Math.Max(1, diagnostic.WarningLevel),
			diagnostic.IsSuppressed,
			diagnostic.Descriptor.Title,
			diagnostic.Descriptor.Description,
			diagnostic.Descriptor.HelpLinkUri,
			diagnostic.Location,
			diagnostic.AdditionalLocations,
			diagnostic.Descriptor.CustomTags,
			diagnostic.Properties);
	}
}
