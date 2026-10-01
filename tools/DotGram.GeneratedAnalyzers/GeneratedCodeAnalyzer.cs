using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace DotGram.GeneratedAnalyzers;

/// <summary>
/// One analyzer, one walk. Loaded only under <c>DotGramAnalyzeGenerated=true</c> (never in a
/// shipped package), it looks at the generator's OWN output for what a build's ordinary
/// warnings-as-errors never catch there: the stock IDE/CS rules skip generated trees by
/// design (that is what <see cref="GeneratedCodeAnalysisFlags"/> is for), so this project asks
/// to see them anyway (<see cref="AnalysisContext.ConfigureGeneratedCodeAnalysis"/> below) and
/// runs its own small set of checks in one traversal per method body, rather than one
/// traversal per rule.
///
/// A finding whose location falls inside a <c>#line</c>-mapped region is the grammar author's
/// own embedded C# (a <c>=&gt;</c>, a <c>when</c>, a switch selector) and not the emitter's, so
/// every rule here (DGA001, DGA003, DGA004) is silent there — the same convention the stock
/// rules follow through the CI job's SARIF gate (see <c>docs/development.md</c>), applied
/// directly since we control our own reporting.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class GeneratedCodeAnalyzer : DiagnosticAnalyzer
{
	public const string Dga001 = "DGA001";
	public const string Dga001Reason = "DGA001R";
	public const string Dga003 = "DGA003";
	public const string Dga004 = "DGA004";

	static readonly DiagnosticDescriptor Dga001Rule = new(
		Dga001,
		"A local is copied into another local and never read again",
		"'{0}' is copied into '{1}' and never read again; construct '{1}' directly or read '{0}' afterward",
		"DotGram.GeneratedAnalyzers",
		DiagnosticSeverity.Error,
		isEnabledByDefault: true,
		description: "The emitter wrote a value into one LOCAL and, on the very next line, copied it into " +
			"another without reading the first local again anywhere reachable from there. Roslyn compiles " +
			"every line the emitter writes, so a dead copy is generated size with no runtime benefit. A " +
			"field (a reader's own register, carried for a caller to read later) is never the source of " +
			"this diagnostic: only a true local, confirmed through the semantic model, is. Suppress only " +
			"with a stated reason.");

	static readonly DiagnosticDescriptor Dga001ReasonRule = new(
		Dga001Reason,
		"DGA001 suppressed without a reason",
		"'#pragma warning disable DGA001' needs a trailing '// reason' comment on the same line",
		"DotGram.GeneratedAnalyzers",
		DiagnosticSeverity.Error,
		isEnabledByDefault: true,
		description: "DGA001 may be suppressed at an emitter site that has a genuine reason, but the " +
			"suppression must say what the reason is, so a later reader is not left to re-derive it.");

	static readonly DiagnosticDescriptor Dga003Rule = new(
		Dga003,
		"A table is spelled as code",
		"'{0}' is a {1}-armed table of constants spelled as a {2} ({3} bytes); consider a data table",
		"DotGram.GeneratedAnalyzers",
		DiagnosticSeverity.Info,
		isEnabledByDefault: true,
		description: "A switch or if-chain over one variable with more than 32 constant arms, each returning " +
			"or assigning a constant, is a judgement call about data-encoding, not a defect. Reported for the " +
			"generator's own attention, never enforced.");

	static readonly DiagnosticDescriptor Dga004Rule = new(
		Dga004,
		"Possible allocation on a hot emitted path",
		"'{0}' in '{1}' may allocate on a hot path ({2})",
		"DotGram.GeneratedAnalyzers",
		DiagnosticSeverity.Info,
		isEnabledByDefault: true,
		description: "Report-only inventory of likely allocations (a closure or delegate, an array, a LINQ " +
			"call) in emitted Read_/Recognize_/Scan_/Materialize_/Construct_ methods and the buffered Ensure " +
			"path. Construction of a rule's own result value is excluded on purpose: that allocation is the " +
			"parser's job, not overhead. Never enforced.");

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
		ImmutableArray.Create(Dga001Rule, Dga001ReasonRule, Dga003Rule, Dga004Rule);

	// A prefix family this project's own emitted parsers use (Machine.Reader.cs, Machine.cs):
	// the "hot path" DGA004 asks about. Adding a family here is the only change a new emitter
	// naming convention needs.
	static readonly string[] HotPathPrefixes = ["Read_", "Recognize_", "Scan_", "Materialize_", "Construct_", "Ensure"];

	public override void Initialize(AnalysisContext context)
	{
		// The whole point of this analyzer is generated code: ask to see it and to report on
		// it, bypassing the default GeneratedCodeAnalysisFlags.None every stock IDE analyzer
		// registers with. This needs no .editorconfig/.globalconfig cooperation because it is
		// our own registration, not a stock analyzer's.
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
		context.EnableConcurrentExecution();

		context.RegisterCodeBlockAction(AnalyzeCodeBlock);
		context.RegisterSyntaxTreeAction(AnalyzeSuppressionReasons);
	}

	/// <summary>One walk of one method/accessor/constructor body: DGA001, DGA003 and DGA004 together.</summary>
	static void AnalyzeCodeBlock(CodeBlockAnalysisContext context)
	{
		var block = context.CodeBlock;
		var owner = context.OwningSymbol;

		var methodName = owner.Name;
		var isHotPath = HotPathPrefixes.Any(prefix => methodName.StartsWith(prefix, StringComparison.Ordinal));

		foreach (var node in block.DescendantNodesAndSelf())
		{
			if (InLineRegion(node))
				continue;

			switch (node)
			{
				case BlockSyntax blockSyntax:
					AnalyzeCopiesIn(context, blockSyntax);
					break;

				case SwitchStatementSyntax switchStatement:
					AnalyzeSwitchTable(context, switchStatement);
					break;

				case IfStatementSyntax ifStatement when ifStatement.Parent is not ElseClauseSyntax:
					AnalyzeIfChainTable(context, ifStatement);
					break;
			}

			if (isHotPath)
				AnalyzeHotPathAllocation(context, node, methodName);
		}
	}

	// ---- DGA001: a local copied into another local, never read again ------------------------

	static void AnalyzeCopiesIn(CodeBlockAnalysisContext context, BlockSyntax block)
	{
		var model = context.SemanticModel;
		var statements = block.Statements;

		for (var i = 0; i + 1 < statements.Count; i++)
		{
			var match = TryMatchCopy(model, statements[i], statements[i + 1]);
			if (match is not (ILocalSymbol source, string sourceName, string target, TextSpan span))
				continue;

			if (InLineRegion(statements[i]))
				continue;

			if (IsReadAfter(model, source, statements[i + 1]))
				continue;

			context.ReportDiagnostic(Diagnostic.Create(Dga001Rule, Location.Create(context.CodeBlock.SyntaxTree, span), sourceName, target));
		}
	}

	static (ILocalSymbol Source, string SourceName, string Target, TextSpan Span)? TryMatchCopy(
		SemanticModel model, StatementSyntax first, StatementSyntax second)
	{
		// decl-decl: var a = <expr>; var b = a;   -- a LOCAL declared right here (a "var"
		// declaration can only ever introduce a local, never a field, so the semantic check
		// below is only for the symbol reference, never for ruling the declaration itself out).
		if (first is LocalDeclarationStatementSyntax { Declaration.Variables.Count: 1 } firstDecl
			&& second is LocalDeclarationStatementSyntax { Declaration.Variables.Count: 1 } secondDecl)
		{
			var a = firstDecl.Declaration.Variables[0];
			var b = secondDecl.Declaration.Variables[0];

			if (a.Initializer?.Value is not null
				&& b.Initializer?.Value is IdentifierNameSyntax rhs
				&& rhs.Identifier.Text == a.Identifier.Text
				&& model.GetDeclaredSymbol(a) is ILocalSymbol sourceSymbol)
			{
				return (sourceSymbol, a.Identifier.Text, b.Identifier.Text, TextSpan.FromBounds(first.SpanStart, second.Span.End));
			}
		}

		// assign-assign: x = y; z = x;   -- x must be a LOCAL (confirmed through the semantic
		// model), never a field: a reader's own register field is written this same way on
		// purpose (Machine.Reader.cs's EmitRecord), to end the turn current for a CALLER that
		// reads it later -- code this walk, scoped to one method body, cannot see.
		if (first is ExpressionStatementSyntax firstStatement
			&& firstStatement.Expression is AssignmentExpressionSyntax firstAssign
			&& firstAssign.IsKind(SyntaxKind.SimpleAssignmentExpression)
			&& firstAssign.Left is IdentifierNameSyntax x
			&& second is ExpressionStatementSyntax secondStatement
			&& secondStatement.Expression is AssignmentExpressionSyntax secondAssign
			&& secondAssign.IsKind(SyntaxKind.SimpleAssignmentExpression)
			&& secondAssign.Left is IdentifierNameSyntax targetName
			&& secondAssign.Right is IdentifierNameSyntax rhs2
			&& rhs2.Identifier.Text == x.Identifier.Text
			&& model.GetSymbolInfo(x).Symbol is ILocalSymbol sourceSymbol2)
		{
			return (sourceSymbol2, x.Identifier.Text, targetName.Identifier.Text, TextSpan.FromBounds(firstAssign.SpanStart, second.Span.End));
		}

		return null;
	}

	/// <summary>
	/// Whether <paramref name="source"/> is read anywhere ordinary control flow can still
	/// reach from <paramref name="from"/> (the copy's own second statement) onward: the rest
	/// of the block it is in, then — once that block ends — wherever the enclosing
	/// construct's own control flow continues, all the way out to the member boundary.
	/// </summary>
	/// <remarks>
	/// This deliberately never walks INTO a branch this specific copy cannot reach (an
	/// "else", a different "case", the body of a different "if"): a name reused in a sibling
	/// branch is a different value and must not hide a genuine dead copy here, which a plain
	/// whole-member text search could not tell apart. For a construct this walk does not
	/// model precisely (a loop, a switch, try/catch, a single-statement body without braces),
	/// it assumes the value MAY be read rather than risk reporting one that is not actually
	/// dead — the safe direction for a rule that is an error.
	/// </remarks>
	static bool IsReadAfter(SemanticModel model, ILocalSymbol source, StatementSyntax from)
	{
		SyntaxNode node = from;

		while (true)
		{
			if (node.Parent is BlockSyntax block)
			{
				var index = block.Statements.IndexOf((StatementSyntax)node);

				for (var j = index + 1; j < block.Statements.Count; j++)
				{
					if (StatementReads(model, block.Statements[j], source))
						return true;
				}

				node = block;
				continue;
			}

			switch (node.Parent)
			{
				case IfStatementSyntax ifStatement when ifStatement.Statement == node:
					// The "then" branch (braced or not) ends here; control continues after
					// the WHOLE if/else, never into "else" -- a sibling this copy did not run.
					node = ifStatement;
					continue;

				case ElseClauseSyntax elseClause:
					// Symmetrically for the "else" side: continue after the owning if/else,
					// never back into "then".
					node = elseClause.Parent!;
					continue;

				case BaseMethodDeclarationSyntax:
				case AccessorDeclarationSyntax:
				case LocalFunctionStatementSyntax:
				case null:
					return false;

				default:
					return true;
			}
		}
	}

	static bool StatementReads(SemanticModel model, StatementSyntax statement, ILocalSymbol source)
	{
		foreach (var id in statement.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>())
		{
			if (IsRead(model, id, source))
				return true;
		}

		return false;
	}

	static bool IsRead(SemanticModel model, IdentifierNameSyntax id, ILocalSymbol source)
	{
		if (!SymbolEqualityComparer.Default.Equals(model.GetSymbolInfo(id).Symbol, source))
			return false;

		// A plain assignment target ("x = ...;") does not read the prior value of x. A
		// COMPOUND assignment target ("x += 1;") does -- it is shorthand for "x = x + 1" --
		// and must count as a read, or a dead-copy diagnostic could fire on a local that is
		// genuinely still live through exactly this kind of use.
		if (id.Parent is AssignmentExpressionSyntax assign
			&& assign.Left == id
			&& assign.IsKind(SyntaxKind.SimpleAssignmentExpression))
		{
			return false;
		}

		return true;
	}

	// ---- DGA001R: a suppression of DGA001 without a stated reason ----------------------------

	static void AnalyzeSuppressionReasons(SyntaxTreeAnalysisContext context)
	{
		var root = context.Tree.GetRoot(context.CancellationToken);

		foreach (var trivia in root.DescendantTrivia())
		{
			if (trivia.GetStructure() is not PragmaWarningDirectiveTriviaSyntax pragma || !pragma.DisableOrRestoreKeyword.IsKind(SyntaxKind.DisableKeyword))
				continue;

			// An exact match on the error code, not a substring: "Contains" would also catch
			// a rule whose id merely contains "DGA001" as a prefix (there is none today, but
			// nothing stops a DGA0010 from existing later) and wrongly ask it for a reason too.
			var namesDga001 = pragma.ErrorCodes.Any(code => code.ToString().Trim() == Dga001);
			if (!namesDga001)
				continue;

			// A reason is a trailing "// ..." comment on the pragma's own line: the same
			// convention as the suppress/restore fence this repository already emits around
			// CS0649 (Support.cs), except here the analyzer checks it rather than trusting it.
			var line = pragma.GetLocation().GetLineSpan().StartLinePosition.Line;
			var lineText = context.Tree.GetText(context.CancellationToken).Lines[line].ToString();
			var at = lineText.IndexOf("//", StringComparison.Ordinal);
			var hasReason = at >= 0 && lineText.Substring(at + 2).Trim().Length > 0;

			if (!hasReason)
				context.ReportDiagnostic(Diagnostic.Create(Dga001ReasonRule, pragma.GetLocation()));
		}
	}

	// ---- DGA003: a table spelled as code ------------------------------------------------------

	const int TableArmThreshold = 32;

	static void AnalyzeSwitchTable(CodeBlockAnalysisContext context, SwitchStatementSyntax switchStatement)
	{
		var constantArms = switchStatement.Sections
			.SelectMany(section => section.Labels)
			.OfType<CaseSwitchLabelSyntax>()
			.Count(label => IsConstantLike(label.Value));

		if (constantArms <= TableArmThreshold)
			return;

		var kind = switchStatement.Sections.Any(s => s.Statements.Any(st => st is ReturnStatementSyntax)) ? "return switch" : "switch";
		Report(context, switchStatement, constantArms, kind);
	}

	static void AnalyzeIfChainTable(CodeBlockAnalysisContext context, IfStatementSyntax head)
	{
		var arms = 0;
		IfStatementSyntax? current = head;
		var allConstant = true;

		while (current is not null)
		{
			if (current.Condition is BinaryExpressionSyntax binary
				&& binary.IsKind(SyntaxKind.EqualsExpression)
				&& (IsConstantLike(binary.Right) || IsConstantLike(binary.Left)))
			{
				arms++;
			}
			else
			{
				allConstant = false;
			}

			current = current.Else?.Statement switch
			{
				IfStatementSyntax nested => nested,
				_ => null,
			};
		}

		if (!allConstant || arms <= TableArmThreshold)
			return;

		Report(context, head, arms, "if-chain");
	}

	static void Report(CodeBlockAnalysisContext context, SyntaxNode node, int arms, string kind)
	{
		var owner = context.OwningSymbol.Name;
		var bytes = System.Text.Encoding.UTF8.GetByteCount(node.ToFullString());

		context.ReportDiagnostic(Diagnostic.Create(Dga003Rule, node.GetLocation(), owner, arms, kind, bytes));
	}

	static bool IsConstantLike(ExpressionSyntax expression)
	{
		return expression is LiteralExpressionSyntax or MemberAccessExpressionSyntax { Name.Identifier.Text: not null };
	}

	// ---- DGA004: report-only allocation inventory on a hot emitted path -----------------------

	static void AnalyzeHotPathAllocation(CodeBlockAnalysisContext context, SyntaxNode node, string methodName)
	{
		switch (node)
		{
			// Result-value construction is the parser's job, not overhead: excluded on purpose
			// (a rule's own record/class, or a user factory's return value). Anything else
			// `new` builds here (an array backing a table, a StringBuilder, a boxed struct
			// passed as object) is overhead and is reported.
			case ObjectCreationExpressionSyntax { Type: var type } creation when !LooksLikeResultType(type):
				ReportAllocation(context, creation, methodName, "object creation");
				break;

			case ArrayCreationExpressionSyntax or ImplicitArrayCreationExpressionSyntax:
				ReportAllocation(context, node, methodName, "array allocation");
				break;

			case AnonymousFunctionExpressionSyntax or ParenthesizedLambdaExpressionSyntax or SimpleLambdaExpressionSyntax:
				ReportAllocation(context, node, methodName, "closure or delegate");
				break;

			case InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax { Name.Identifier.Text: var name } }
				when LinqMethodNames.Contains(name):
				ReportAllocation(context, node, methodName, "LINQ call");
				break;
		}
	}

	static readonly HashSet<string> LinqMethodNames =
		["Where", "Select", "SelectMany", "OrderBy", "OrderByDescending", "GroupBy", "ToList", "ToArray", "ToDictionary", "Aggregate"];

	// A rule's own value type is nested in (or named for) the host class the parser attaches
	// to, or is one of this project's own support records (SourceSpan and the like); a plain
	// syntactic guess is enough for a report-only inventory, and erring toward "looks like a
	// result type" (excluding it) is the safe direction here, since the cost of this check
	// missing a real allocation is a report never enforces anyway.
	static bool LooksLikeResultType(TypeSyntax type)
	{
		var name = type switch
		{
			QualifiedNameSyntax qualified => qualified.Right.Identifier.Text,
			GenericNameSyntax generic => generic.Identifier.Text,
			IdentifierNameSyntax identifier => identifier.Identifier.Text,
			_ => type.ToString(),
		};

		return name is not ("StringBuilder" or "List" or "Dictionary" or "HashSet" or "Queue" or "Stack");
	}

	static void ReportAllocation(CodeBlockAnalysisContext context, SyntaxNode node, string methodName, string what)
	{
		if (InLineRegion(node))
			return;

		context.ReportDiagnostic(Diagnostic.Create(Dga004Rule, node.GetLocation(), what, methodName, node.ToString() is { Length: <= 60 } s ? s : what));
	}

	// ---- #line exclusion: the grammar author's own embedded C#, never ours to flag -----------

	static bool InLineRegion(SyntaxNode node)
	{
		var tree = node.SyntaxTree;
		var mapped = tree.GetMappedLineSpan(node.Span);

		return mapped.Path != tree.FilePath && !string.IsNullOrEmpty(mapped.Path);
	}
}
