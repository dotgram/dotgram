using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using DotGram.Grammar;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotGram.Language;

/// <summary>
/// What the build makes of a diagnostic the generator reports in one file: the severity it is
/// shown at, or nothing where the build does not show it.
/// </summary>
/// <remarks>
/// <para>
/// The generator hands its diagnostics to Roslyn, and Roslyn filters them before anybody sees one:
/// the project's warning level, <c>NoWarn</c>, <c>WarningsAsErrors</c>, <c>WarningsNotAsErrors</c>,
/// <c>TreatWarningsAsErrors</c>, <c>dotnet_diagnostic.GRAMnnnn.severity</c> in an
/// <c>.editorconfig</c> or a global analyzer configuration, and the C# file's
/// <c>#pragma warning disable</c>. The editor computes the same diagnostics itself, so it applies
/// the same filter, or a grammar under a <c>#pragma</c> is shown a warning the build has silenced.
/// </para>
/// <para>
/// The filter is Roslyn's (<c>CSharpDiagnosticFilter.GetDiagnosticReport</c>), in its order. A
/// warning level below the diagnostic's hides it: nought for an error, one for anything else. The
/// project's option for the number comes first; the file's configuration, and then the global
/// one, only where the project set nothing for it or set it back to what it would be
/// (<c>WarningsNotAsErrors</c>, which writes <see cref="ReportDiagnostic.Default"/>) — and then
/// not to make an error of a warning the project said is not one. A configuration's warning is
/// an error under <c>TreatWarningsAsErrors</c>, unless the project said otherwise for the number.
/// What is then suppressed is not shown, and what a <c>#pragma</c> disables is not either, an
/// error included: only the C# compiler's own errors are beyond one. What nothing set follows the
/// project's general option: a warning becomes an error under <c>TreatWarningsAsErrors</c>, and a
/// warning or an information is hidden where every warning is suppressed.
/// </para>
/// <para>
/// A grammar that is a file of its own has no syntax tree: its diagnostics are placed in an
/// additional file, which neither a <c>#pragma</c> nor an <c>.editorconfig</c> section reaches, and
/// only the project's options and a global configuration apply.
/// </para>
/// </remarks>
/// <param name="tree">The C# file the diagnostics are placed in, or null for a grammar that is a file of its own.</param>
/// <param name="options">The compilation's options, or null where there is no compilation to ask and only the <c>#pragma</c>s apply.</param>
sealed class GramBuildSeverity(SyntaxTree? tree, CompilationOptions? options)
{
	readonly SyntaxTree?         _tree    = tree;
	readonly CompilationOptions? _options = options;

	/// <summary>The file's <c>#pragma warning</c> directives in order, read once for every diagnostic asked about.</summary>
	IReadOnlyList<PragmaWarningDirectiveTriviaSyntax>? _pragmas;

	/// <summary>The severity the build reports this at, or null where it is not reported.</summary>
	/// <param name="position">Where in the file it is placed.</param>
	public GramSeverity? Of(GramDiagnostic diagnostic, int position, CancellationToken cancellationToken = default)
	{
		if (diagnostic is null)
			throw new ArgumentNullException(nameof(diagnostic));

		var report = Report(diagnostic, cancellationToken);

		if (report == ReportDiagnostic.Suppress || Disabled(diagnostic.Id, position, cancellationToken))
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

	/// <summary>What the options make of this diagnostic, before a <c>#pragma</c> is asked.</summary>
	ReportDiagnostic Report(GramDiagnostic diagnostic, CancellationToken cancellationToken)
	{
		if (_options is null)
			return ReportDiagnostic.Default;

		var id       = diagnostic.Id;
		var severity = diagnostic.Severity;

		// What Roslyn takes for a diagnostic of this severity that does not name a level of its own.
		if ((severity == GramSeverity.Error ? 0 : 1) > _options.WarningLevel)
			return ReportDiagnostic.Suppress;

		var report    = ReportDiagnostic.Default;
		var specified = false;
		var notError  = false;

		if (_options.SpecificDiagnosticOptions.TryGetValue(id, out var specific))
		{
			report    = specific;
			specified = true;
			notError  = specific == ReportDiagnostic.Default;
		}

		if (_options.SyntaxTreeOptionsProvider is { } provider && (!specified || notError))
		{
			var configured = ReportDiagnostic.Default;
			var found      = _tree is not null && provider.TryGetDiagnosticValue(_tree, id, cancellationToken, out configured) ||
				provider.TryGetGlobalDiagnosticValue(id, cancellationToken, out configured);

			if (found && !(notError && severity == GramSeverity.Warning && configured == ReportDiagnostic.Error))
			{
				specified = true;
				report    = configured;

				if (!notError && report == ReportDiagnostic.Warn && _options.GeneralDiagnosticOption == ReportDiagnostic.Error)
					report = ReportDiagnostic.Error;
			}
		}

		if (report != ReportDiagnostic.Default)
			return report;

		return _options.GeneralDiagnosticOption switch
		{
			ReportDiagnostic.Error    when severity == GramSeverity.Warning && !specified => ReportDiagnostic.Error,
			ReportDiagnostic.Suppress when severity != GramSeverity.Error                 => ReportDiagnostic.Suppress,
			_                                                                             => report,
		};
	}

	/// <summary>Whether a <c>#pragma warning disable</c> before this place, not restored since, names this number or none.</summary>
	bool Disabled(string id, int position, CancellationToken cancellationToken)
	{
		var disabled = false;

		foreach (var pragma in Pragmas(cancellationToken))
		{
			if (pragma.SpanStart >= position)
				break;

			if (pragma.ErrorCodes.Count == 0 || pragma.ErrorCodes.Any(code => Names(code, id)))
				disabled = pragma.DisableOrRestoreKeyword.IsKind(SyntaxKind.DisableKeyword);
		}

		return disabled;
	}

	IReadOnlyList<PragmaWarningDirectiveTriviaSyntax> Pragmas(CancellationToken cancellationToken)
	{
		if (_pragmas is not null)
			return _pragmas;

		var pragmas = new List<PragmaWarningDirectiveTriviaSyntax>();

		if (_tree?.GetRoot(cancellationToken) is CSharpSyntaxNode root)
		{
			for (var directive = root.GetFirstDirective(IsPragma); directive is not null; directive = directive.GetNextDirective(IsPragma))
			{
				pragmas.Add((PragmaWarningDirectiveTriviaSyntax)directive);
			}
		}

		return _pragmas = pragmas;
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
