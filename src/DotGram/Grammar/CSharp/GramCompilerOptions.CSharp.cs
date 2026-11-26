using System;
using System.Collections.Generic;

namespace DotGram.Grammar;

/// <summary>The options that say what C# the parser is generated into, each where every caller has always set it.</summary>
sealed partial class GramCompilerOptions
{
	/// <summary>The C# it is generated into.</summary>
	public CSharpHostShape Host { get; } = new();

	/// <inheritdoc cref="CSharpHostShape.ClassName"/>
	public string ClassName { get => Host.ClassName; set => Host.ClassName = value; }

	/// <inheritdoc cref="CSharpHostShape.Namespace"/>
	public string? Namespace { get => Host.Namespace; set => Host.Namespace = value; }

	/// <inheritdoc cref="CSharpHostShape.SymbolResolver"/>
	public ISymbolResolver SymbolResolver { get => Host.SymbolResolver; set => Host.SymbolResolver = value; }

	/// <inheritdoc cref="CSharpHostShape.CSharpScanner"/>
	public ICSharpScanner? CSharpScanner { get => Host.CSharpScanner; set => Host.CSharpScanner = value; }

	/// <inheritdoc cref="CSharpHostShape.LineMap"/>
	public ILineMap? LineMap { get => Host.LineMap; set => Host.LineMap = value; }

	/// <inheritdoc cref="CSharpHostShape.LocationType"/>
	public string? LocationType { get => Host.LocationType; set => Host.LocationType = value; }

	/// <inheritdoc cref="CSharpHostShape.LocationTypeIsClass"/>
	public bool LocationTypeIsClass { get => Host.LocationTypeIsClass; set => Host.LocationTypeIsClass = value; }

	/// <inheritdoc cref="CSharpHostShape.LocatedFacade"/>
	public string? LocatedFacade { get => Host.LocatedFacade; set => Host.LocatedFacade = value; }

	/// <inheritdoc cref="CSharpHostShape.LocatedFacadeDeclared"/>
	public bool LocatedFacadeDeclared { get => Host.LocatedFacadeDeclared; set => Host.LocatedFacadeDeclared = value; }

	/// <inheritdoc cref="CSharpHostShape.StaticImports"/>
	public IReadOnlyList<string> StaticImports { get => Host.StaticImports; set => Host.StaticImports = value; }

	/// <inheritdoc cref="CSharpHostShape.Suffix"/>
	public string? Suffix { get => Host.Suffix; set => Host.Suffix = value; }

	/// <inheritdoc cref="CSharpHostShape.SuffixDeclared"/>
	public bool SuffixDeclared { get => Host.SuffixDeclared; set => Host.SuffixDeclared = value; }

	/// <inheritdoc cref="CSharpHostShape.Readings"/>
	public IReadOnlyList<string> Readings { get => Host.Readings; set => Host.Readings = value; }

	/// <inheritdoc cref="CSharpHostShape.SpanDeclared"/>
	public bool SpanDeclared { get => Host.SpanDeclared; set => Host.SpanDeclared = value; }

	/// <inheritdoc cref="CSharpHostShape.SpanDeclaredInReading"/>
	public bool SpanDeclaredInReading { get => Host.SpanDeclaredInReading; set => Host.SpanDeclaredInReading = value; }

	/// <inheritdoc cref="CSharpHostShape.MembersDeclared"/>
	public IReadOnlyList<string> MembersDeclared { get => Host.MembersDeclared; set => Host.MembersDeclared = value; }

	/// <inheritdoc cref="CSharpHostShape.SharesLimits"/>
	public bool SharesLimits { get => Host.SharesLimits; set => Host.SharesLimits = value; }

	/// <inheritdoc cref="CSharpHostShape.MembersDeclaredInReading"/>
	public IReadOnlyList<string> MembersDeclaredInReading { get => Host.MembersDeclaredInReading; set => Host.MembersDeclaredInReading = value; }

	/// <inheritdoc cref="CSharpHostShape.SharedTypes"/>
	public bool SharedTypes { get => Host.SharedTypes; set => Host.SharedTypes = value; }

	/// <inheritdoc cref="CSharpHostShape.Inherits"/>
	public bool Inherits { get => Host.Inherits; set => Host.Inherits = value; }
}
