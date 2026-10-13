using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.Tests;

static partial class Bridge
{
	/// <summary>
	/// Procedures, functions, triggers, EXECUTE, and the module-like objects defined beside them.
	/// </summary>
	/// <remarks>
	/// Every record throws the gap its inventory row names; sequences, synonyms and types are the Tables family's.
	/// </remarks>
	internal static class Routines
	{
		public static readonly string[] Owns =
		[
			"Statement.CreateProcedure", "Statement.CreateFunction", "Statement.CreateTrigger",
			"Statement.Execute", "Statement.TriggerSwitch", "Statement.AggregateDefinition",
			"Statement.AssemblyDefinition", "Statement.RuleDefinition", "Statement.DefaultDefinition",
			"Statement.XmlSchemaCollectionDefinition", "Statement.AlterXmlSchemaCollection", "Clause.ParameterDeclaration",
			"Clause.ExecutionContext",
		];

		public static bool TryConvert(object old, [NotNullWhen(true)] out Ast.ISqlNode? made)
		{
			made = old switch
			{
				_ when Owned(Owns, old) => throw Inventory.Unconverted(old),
				_                       => null,
			};

			return made is not null;
		}
	}
}
