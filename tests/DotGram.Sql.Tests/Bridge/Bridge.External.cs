using System;
using System.Diagnostics.CodeAnalysis;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.Tests;

static partial class Bridge
{
	/// <summary>
	/// External resources: external data sources, file formats, libraries, languages, models and resource pools.
	/// </summary>
	/// <remarks>No arm yet: every record here throws the gap its inventory row names.</remarks>
	internal static class External
	{
		public static readonly string[] Owns =
		[
			"Statement.ExternalDataSourceDefinition", "Statement.ExternalFileFormatDefinition", "Statement.ExternalLibraryDefinition",
			"Statement.ExternalLanguageDefinition", "Statement.ExternalModelDefinition", "Statement.ExternalResourcePoolDefinition",
		];

		public static bool TryConvert(object old, [NotNullWhen(true)] out Ast.ISqlNode? made)
		{
			made = null;

			return Owned(Owns, old) ? throw Inventory.Unconverted(old) : false;
		}
	}
}
