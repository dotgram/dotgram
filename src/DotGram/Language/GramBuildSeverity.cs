using System;
using System.Linq;
using System.Threading;

using DotGram.Grammar;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotGram.Language;

/// <summary>
/// What the build makes of a diagnostic the generator reports at a place in a C# file: the
/// severity it is shown at, or nothing where the build does not show it.
/// </summary>
/// <remarks>
/// <para>
/// The generator hands its diagnostics to Roslyn, and Roslyn applies the C# file's
/// <c>#pragma warning disable</c> and the project's configuration to them before anybody sees
/// one: <c>NoWarn</c>, <c>WarningsAsErrors</c>, <c>TreatWarningsAsErrors</c>, and
/// <c>dotnet_diagnostic.GRAMnnnn.severity</c> in an <c>.editorconfig</c> or a global analyzer
/// configuration. The editor computes the same diagnostics itself, so it applies the same rules,
/// or a grammar under a <c>#pragma</c> is shown a warning the build has silenced.
/// </para>
/// <para>
/// Roslyn's order: the file's configuration, then the global one, then the project's own
/// options for that number, then <c>TreatWarningsAsErrors</c> for a warning nothing set. A
/// <c>#pragma</c> silences whatever it names, an error included: only the C# compiler's own
/// errors are beyond one, and the generator's are not those.
/// </para>
/// </remarks>
static class GramBuildSeverity
{
	/// <summary>The severity the build reports this at, or null where it is not reported.</summary>
	/// <param name="tree">The C# file the diagnostic is placed in.</param>
	/// <param name="position">Where in that file it is placed.</param>
	/// <param name="options">The compilation's options, or null where there is no compilation to ask.</param>
	public static GramSeverity? Of(
		GramDiagnostic diagnostic,
		SyntaxTree tree,
		int position,
		CompilationOptions? options,
		CancellationToken cancellationToken = default)
	{
		var report = Configured(diagnostic, tree, options, cancellationToken);

		if (report == ReportDiagnostic.Default &&
			diagnostic.Severity == GramSeverity.Warning &&
			options?.GeneralDiagnosticOption == ReportDiagnostic.Error)
			report = ReportDiagnostic.Error;

		if (Disabled(tree, diagnostic.Id, position, cancellationToken))
			return null;

		return report switch
		{
			ReportDiagnostic.Default => diagnostic.Severity,
			ReportDiagnostic.Error   => GramSeverity.Error,
			ReportDiagnostic.Warn    => GramSeverity.Warning,
			ReportDiagnostic.Info    => GramSeverity.Info,
			_                        => null,
		};
	}

	/// <summary>What the configuration says of this number, or <see cref="ReportDiagnostic.Default"/>.</summary>
	static ReportDiagnostic Configured(
		GramDiagnostic diagnostic, SyntaxTree tree, CompilationOptions? options, CancellationToken cancellationToken)
	{
		if (options is null)
			return ReportDiagnostic.Default;

		var report = ReportDiagnostic.Default;

		if (options.SyntaxTreeOptionsProvider is { } provider &&
			(provider.TryGetDiagnosticValue(tree, diagnostic.Id, cancellationToken, out var configured) ||
			 provider.TryGetGlobalDiagnosticValue(diagnostic.Id, cancellationToken, out configured)))
			report = configured;

		if (options.SpecificDiagnosticOptions.TryGetValue(diagnostic.Id, out var specific))
			report = specific;

		return report;
	}

	/// <summary>Whether a <c>#pragma warning disable</c> before this place, not restored since, names this number or none.</summary>
	static bool Disabled(SyntaxTree tree, string id, int position, CancellationToken cancellationToken)
	{
		var disabled = false;

		if (tree.GetRoot(cancellationToken) is not CSharpSyntaxNode root)
			return false;

		for (var directive = root.GetFirstDirective(IsPragma);
			directive is PragmaWarningDirectiveTriviaSyntax pragma && pragma.SpanStart < position;
			directive = directive.GetNextDirective(IsPragma))
			if (pragma.ErrorCodes.Count == 0 || pragma.ErrorCodes.Any(code => Names(code, id)))
				disabled = pragma.DisableOrRestoreKeyword.IsKind(SyntaxKind.DisableKeyword);

		return disabled;
	}

	static bool IsPragma(DirectiveTriviaSyntax directive)
	{
		return directive is PragmaWarningDirectiveTriviaSyntax;
	}

	static bool Names(ExpressionSyntax code, string id)
	{
		return code is IdentifierNameSyntax name && string.Equals(name.Identifier.ValueText, id, StringComparison.Ordinal);
	}
}
