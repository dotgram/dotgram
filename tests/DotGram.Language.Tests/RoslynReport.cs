using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DotGram.Grammar;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace DotGram.Language.Tests;

/// <summary>
/// What Roslyn makes of diagnostics reported in a compilation's C# files: which it shows, and at
/// what severity, once the files' <c>#pragma</c>s and the compilation's options have had their say.
/// </summary>
/// <remarks>
/// Asked of Roslyn itself, by handing the diagnostics to an analyzer that reports them, so that a
/// test holds the editor to what the build does rather than to another reading of the rules.
/// </remarks>
static class RoslynReport
{
	/// <summary>The file a diagnostic with no syntax tree is placed in: a grammar that is a file of its own.</summary>
	public const string GrammarFile = "/grammar.gram";

	/// <summary>Each diagnostic Roslyn shows, with the severity it shows it at.</summary>
	/// <param name="reported">
	/// Each diagnostic and where it is placed: in a C# file of the compilation, or, with no tree, in
	/// <see cref="GrammarFile"/>, as a grammar file's diagnostics are.
	/// </param>
	public static async Task<IReadOnlyList<(string Id, string Message, GramSeverity Severity, TextSpan Span)>> Shown(
		Compilation compilation,
		IReadOnlyList<(GramDiagnostic Diagnostic, SyntaxTree? Tree, TextSpan Span)> reported,
		CancellationToken cancellationToken)
	{
		if (reported.Count == 0)
			return [];

		var shown = await compilation
			.WithAnalyzers([new Reporter(reported)])
			.GetAnalyzerDiagnosticsAsync(cancellationToken)
			.ConfigureAwait(false);

		return
		[
			.. shown
				.Where(static diagnostic => diagnostic.Severity != DiagnosticSeverity.Hidden)
				.Select(static diagnostic => (
					diagnostic.Id,
					diagnostic.GetMessage(),
					diagnostic.Severity switch
					{
						DiagnosticSeverity.Error   => GramSeverity.Error,
						DiagnosticSeverity.Warning => GramSeverity.Warning,
						_                          => GramSeverity.Info,
					},
					diagnostic.Location.SourceSpan))
		];
	}

	// Not a compiler extension anybody loads: an analyzer made in a test and handed to
	// CompilationWithAnalyzers, so that Roslyn's own filtering is what the editor's is held to.
	// The rules for analyzers that ship (where they are built, what they reference, a constant
	// identifier for each rule and the release notes of their rules) do not apply.
#pragma warning disable RS1017, RS1036, RS1038, RS1041, RS2008
	/// <summary>Reports the diagnostics it is made with, each where it is placed, with its own severity.</summary>
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	sealed class Reporter(IReadOnlyList<(GramDiagnostic Diagnostic, SyntaxTree? Tree, TextSpan Span)> reported) : DiagnosticAnalyzer
	{
		readonly Dictionary<(string Id, GramSeverity Severity), DiagnosticDescriptor> _descriptors = reported
			.Select(static one => (one.Diagnostic.Id, one.Diagnostic.Severity))
			.Distinct()
			.ToDictionary(static key => key, static key => new DiagnosticDescriptor(
				key.Id,
				key.Id,
				"{0}",
				"DotGram",
				key.Severity switch
				{
					GramSeverity.Error   => DiagnosticSeverity.Error,
					GramSeverity.Warning => DiagnosticSeverity.Warning,
					_                    => DiagnosticSeverity.Info,
				},
				isEnabledByDefault: true,
				customTags: WellKnownDiagnosticTags.CompilationEnd));

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [.. _descriptors.Values];

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();
			context.RegisterCompilationAction(compilation =>
			{
				foreach (var (diagnostic, place, span) in reported)
				{
					var location = place is null
						? Location.Create(GrammarFile, span, new LinePositionSpan(new LinePosition(0, span.Start), new LinePosition(0, span.End)))
						: Location.Create(place, span);

					compilation.ReportDiagnostic(Diagnostic.Create(
						_descriptors[(diagnostic.Id, diagnostic.Severity)], location, diagnostic.Message));
				}
			});
		}
	}
#pragma warning restore RS1017, RS1036, RS1038, RS1041, RS2008
}
