using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar;

namespace DotGram.LanguageServer;

/// <summary>A published analysis's diagnostics as <c>textDocument/publishDiagnostics</c> carries them.</summary>
static class DiagnosticsMap
{
	public const string Source = "dotgram";

	public static IReadOnlyList<Diagnostic> Map(IReadOnlyList<GramDiagnostic> diagnostics, LineMap lines)
	{
		return diagnostics
			.Select(diagnostic => new Diagnostic(
				lines.Range(diagnostic.Position, diagnostic.Length),
				Severity(diagnostic.Severity),
				diagnostic.Id,
				Source,
				diagnostic.Message))
			.ToArray();
	}

	public static int Severity(GramSeverity severity)
	{
		return severity switch
		{
			GramSeverity.Error   => Lsp.SeverityError,
			GramSeverity.Warning => Lsp.SeverityWarning,
			_                    => Lsp.SeverityInformation,
		};
	}
}
