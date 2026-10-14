using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.Tests;

static partial class Bridge
{
	/// <summary>
	/// The sixty-five <c>DROP</c> statements. The standard's take a list, <c>IF EXISTS</c> and a behavior that
	/// may be left out (decided 2026-09-15); the rest are to be a record each, not declared yet.
	/// </summary>
	internal static class Drop
	{
		public static readonly string[] Owns =
		[
			.. typeof(Statement).GetNestedTypes().Where(static one => one.IsSubclassOf(typeof(Statement.Removal))).Select(NameOf),
		];

		/// <summary>
		/// The old drops <c>Statement.DropObject</c> was declared for, by the name of its kind: one record for many, which is rejected.
		/// </summary>
		static readonly string[] Objects = Enum.GetNames<Ast.DropObjectKind>();

		/// <summary>
		/// The old drops a record of their own in <c>SqlAst.TransactSql.cs</c> stands for.
		/// </summary>
		static readonly string[] Own =
		[
			"Statement.DropAssembly", "Statement.DropEventSession", "Statement.DropEventNotification", "Statement.DropExternalLibrary",
			"Statement.DropFulltextIndex", "Statement.DropSensitivityClassification", "Statement.DropMasterKey",
			"Statement.DropDatabaseEncryptionKey", "Statement.DropIndex", "Statement.DropSignature",
		];

		public static bool TryConvert(object old, [NotNullWhen(true)] out Ast.ISqlNode? made)
		{
			made = null;

			if (old is not Statement.Removal removal)
				return false;

			var name = NameOf(old);

			made = removal switch
			{
				Statement.DropTable    => new Ast.Statement.DropTable { Names = Names(removal), IfExists = removal.IfExists },
				Statement.DropView     => new Ast.Statement.DropView { Names = Names(removal), IfExists = removal.IfExists },
				Statement.DropSchema   => new Ast.Statement.DropSchema { Names = Names(removal), IfExists = removal.IfExists },
				Statement.DropSequence => new Ast.Statement.DropSequence { Names = Names(removal), IfExists = removal.IfExists },
				Statement.DropType     => new Ast.Statement.DropType { Names = Names(removal), IfExists = removal.IfExists },
				Statement.DropRole     => new Ast.Statement.DropRole { Names = Each(Names(removal), one => Single(old, one)), IfExists = removal.IfExists },
				Statement.DropFunction => Routines(removal, Ast.RoutineKind.Function),
				Statement.DropProcedure => Routines(removal, Ast.RoutineKind.Procedure),
				Statement.DropTrigger  => Trigger(removal),

				// Statement.DropObject, one record for many kinds, is rejected: each kind is to have a record of its own, not declared yet.
				_ when Array.Exists(Objects, one => name.Equals("Statement.Drop" + one, StringComparison.OrdinalIgnoreCase)) =>
					throw BridgeGap.Missing(old, $"{name} (one record per kind, not declared yet)"),

				_ when Array.IndexOf(Own, name) >= 0 && Inventory.Declared(name) is { } type =>
					throw (Inventory.Written(type) ? BridgeGap.Unbridged(old, name) : BridgeGap.Unwritten(old, name)),

				_ => throw Inventory.Unconverted(old),
			};

			return true;
		}

		/// <summary>
		/// The names dropped, which the old tree keeps as column references; and nothing after them.
		/// </summary>
		static SqlList<Ast.QualifiedName> Names(Statement.Removal old)
		{
			if (old.Tail is not null)
				throw BridgeGap.Open(old, $"inventory {NameOf(old)} (`{old.Tail}` after the names)");

			return Each(old.Names, one => one is Expression.ColumnReference(var text)
				? Values.Name(old, text)
				: throw BridgeGap.Open(old, $"inventory {NameOf(old)} (a name that is {NameOf(one)})"));
		}

		static Ast.Identifier Single(object old, Ast.QualifiedName name)
		{
			return name.Parts.Count == 1 ? name.Parts[0] : throw BridgeGap.Open(old, "inventory Statement.DropRole (a qualified role name)");
		}

		/// <summary>
		/// <c>DROP FUNCTION</c> and <c>DROP PROCEDURE</c>, whose list is of designators. Which of <c>PROC</c> and
		/// <c>PROCEDURE</c> was written the old tree does not keep, and the text says.
		/// </summary>
		static Ast.Statement Routines(Statement.Removal old, Ast.RoutineKind kind)
		{
			var proc = kind == Ast.RoutineKind.Procedure
				&& Recover("Statement.DropRoutine.Proc", Spelled(old), static text => Words(text) is [_, "PROC", ..], false);

			return new Ast.Statement.DropRoutine
			{
				Routines = Each(Names(old), one => new Ast.RoutineDesignator(kind, one)),
				IfExists = old.IfExists,
				Proc     = proc,
			};
		}

		/// <summary>
		/// <c>DROP TRIGGER</c>, and the scope of a DDL trigger, which the old tree keeps as its tail.
		/// </summary>
		static Ast.Statement Trigger(Statement.Removal old)
		{
			if (old.Tail is not { } tail)
				return new Ast.Statement.DropTrigger { Names = Names(old), IfExists = old.IfExists };

			Ast.TriggerScope scope = Words(tail) switch
			{
				["ON", "DATABASE"]          => Ast.TriggerScope.Database,
				["ON", "ALL", "SERVER"]     => Ast.TriggerScope.AllServer,
				_                           => throw BridgeGap.Open(old, $"inventory Statement.DropTrigger (`{tail}`)"),
			};

			return new Ast.Statement.DropTrigger { Names = Names(old with { Tail = null }), IfExists = old.IfExists, On = scope };
		}
	}
}
