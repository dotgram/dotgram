using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.Tests;

static partial class Bridge
{
	/// <summary>
	/// The flow of a batch, transactions, cursors, variables and the session's <c>SET</c>.
	/// </summary>
	internal static class Procedural
	{
		public static readonly string[] Owns =
		[
			"Statement.Compound", "Statement.If", "Statement.While", "Statement.TryCatch", "Statement.Declare",
			"Statement.Return", "Statement.Print", "Statement.Throw", "Statement.GoTo", "Statement.Break",
			"Statement.Continue", "Statement.Label", "Statement.RaiseError", "Statement.WaitFor",
			"Statement.Transaction", "Statement.DeclareCursor", "Statement.SetCursor",
			"Statement.CursorAction", "Statement.Fetch", "Statement.SetStatement", "Statement.SetVariable",
			"Statement.Use", "Statement.ExecuteAs", "Statement.Revert", "Statement.SetUser", "Statement.LineNumber",
			"Clause.VariableDeclaration", "Clause.CursorDefinition", "Clause.CursorFor",
			.. typeof(SetExpression).GetNestedTypes()
				.SelectMany(static group => group.GetNestedTypes())
				.Where(static one => one.IsSubclassOf(typeof(SetExpression)) && !one.IsAbstract)
				.Select(NameOf),
		];

		public static bool TryConvert(object old, [NotNullWhen(true)] out Ast.ISqlNode? made)
		{
			made = old switch
			{
				Statement.Return(var value)  => new Ast.Statement.Return { Value = value is null ? null : Values.Of(value) },
				Statement.Compound compound  => Compound(compound),
				Statement.If(var condition, var then, var other) =>
					new Ast.Statement.If(Values.Of(condition), StatementOf(then), other is null ? null : StatementOf(other)),
				Statement.While(var condition, var body) => new Ast.Statement.While(Values.Of(condition), StatementOf(body)),
				Statement.TryCatch(var tried, var caught) => new Ast.Statement.TryCatch(Each(tried, StatementOf), Each(caught, StatementOf)),
				Statement.Break              => new Ast.Statement.Break(),
				Statement.Continue           => new Ast.Statement.Continue(),
				Statement.GoTo(Expression.ColumnReference(var label)) => new Ast.Statement.GoTo(Values.Part(old, label)),
				Statement.Label(var label)   => new Ast.Statement.Label(Values.Part(old, label)),
				Statement.Transaction ended  => Transaction(ended),
				Statement.CursorAction acted => Cursor(acted),
				Statement.Fetch fetch        => Fetch(fetch),
				Statement.SetStatement set   => Set(set),
				_ when Owned(Owns, old)      => throw Inventory.Unconverted(old),
				_                            => null,
			};

			return made is not null;
		}

		/// <summary>
		/// <c>COMMIT</c> and <c>ROLLBACK</c>, whose T-SQL parts the new tree holds (decided 2026-09-27): the word
		/// after the verb, the name, and a commit's delayed durability.
		/// </summary>
		static Ast.Statement Transaction(Statement.Transaction old)
		{
			var kind = string.Join(' ', Words(old.Kind));

			if (kind is "BEGIN" or "BEGIN DISTRIBUTED")
				throw BridgeGap.Unwritten(old, "Statement.BeginTransaction");

			if (kind == "SAVE")
				throw BridgeGap.Unwritten(old, "Statement.SaveTransaction");

			var word = old.Word?.ToUpperInvariant();

			Ast.TransactionKeyword? keyword = word switch
			{
				"TRAN"        => Ast.TransactionKeyword.Tran,
				"TRANSACTION" => Ast.TransactionKeyword.Transaction,
				_             => null,
			};

			var work = word == "WORK";

			if (word is not null && keyword is null && !work)
				throw BridgeGap.Open(old, $"inventory Statement.Transaction (the word `{old.Word}`)");

			var name = old.Name is null ? null : new Ast.TransactionName(Values.Part(old, old.Name));

			switch (kind)
			{
				case "COMMIT":
					return new Ast.Statement.Commit { Keyword = keyword, Work = work, Name = name, DelayedDurability = Durability(old) };

				case "ROLLBACK":
					if (old.Tail is not null)
						throw BridgeGap.Open(old, $"inventory Statement.Transaction (ROLLBACK … {old.Tail})");

					return new Ast.Statement.Rollback { Keyword = keyword, Work = work, Name = name };

				default:
					throw BridgeGap.Open(old, $"inventory Statement.Transaction (`{old.Kind}`)");
			}
		}

		static bool? Durability(Statement.Transaction old)
		{
			return old.Tail is null ? null : Words(old.Tail) switch
			{
				["WITH", "(", "DELAYED_DURABILITY", "=", "ON", ")"]  => true,
				["WITH", "(", "DELAYED_DURABILITY", "=", "OFF", ")"] => false,
				_ => throw BridgeGap.Open(old, $"inventory Statement.Transaction (COMMIT … {old.Tail})"),
			};
		}

		/// <summary>
		/// <c>SET TRANSACTION ISOLATION LEVEL</c>, the one <c>SET</c> the standard has a statement for. The old
		/// tree does not keep <c>TRAN</c> against <c>TRANSACTION</c>, and the text says which.
		/// </summary>
		static Ast.Statement Set(Statement.SetStatement old)
		{
			if (old.Items is not [SetExpression.Transactions.IsolationLevel(var level)])
				throw Inventory.Unconverted(old);

			Ast.IsolationLevel isolation = string.Join(' ', Words(level)) switch
			{
				"READ UNCOMMITTED" => Ast.IsolationLevel.ReadUncommitted,
				"READ COMMITTED"   => Ast.IsolationLevel.ReadCommitted,
				"REPEATABLE READ"  => Ast.IsolationLevel.RepeatableRead,
				"SERIALIZABLE"     => Ast.IsolationLevel.Serializable,
				"SNAPSHOT"         => Ast.IsolationLevel.Snapshot,
				_                  => throw BridgeGap.Open(old, $"inventory SetExpression.Transactions.IsolationLevel (`{level}`)"),
			};

			var tran = Recover("Statement.SetTransaction.Tran", Spelled(old), static text => Words(text) is [_, "TRAN", ..], false);

			return new Ast.Statement.SetTransaction { Modes = [new Ast.TransactionMode.Isolation(isolation)], Tran = tran };
		}

		static Ast.CursorReference Reference(Statement old, Expression cursor, bool global)
		{
			return cursor is Expression.ColumnReference(var name)
				? new Ast.CursorReference(Values.Name(old, name), Global: global)
				: new Ast.CursorReference(null, Values.Of(cursor), Global: global);
		}

		static Ast.Statement Cursor(Statement.CursorAction old)
		{
			return string.Join(' ', Words(old.Verb)) switch
			{
				"OPEN"       => new Ast.Statement.OpenCursor { Cursor = Reference(old, old.Cursor, old.Global) },
				"CLOSE"      => new Ast.Statement.CloseCursor { Cursor = Reference(old, old.Cursor, old.Global) },
				"DEALLOCATE" => throw BridgeGap.Unwritten(old, "Statement.DeallocateCursor"),
				_            => throw BridgeGap.Open(old, $"inventory Statement.CursorAction (`{old.Verb}`)"),
			};
		}

		static Ast.Statement Fetch(Statement.Fetch old)
		{
			Ast.FetchOrientation? orientation = old.Orientation?.ToUpperInvariant() switch
			{
				null       => null,
				"NEXT"     => new Ast.FetchOrientation(Ast.FetchOrientationKind.Next),
				"PRIOR"    => new Ast.FetchOrientation(Ast.FetchOrientationKind.Prior),
				"FIRST"    => new Ast.FetchOrientation(Ast.FetchOrientationKind.First),
				"LAST"     => new Ast.FetchOrientation(Ast.FetchOrientationKind.Last),
				"ABSOLUTE" => new Ast.FetchOrientation(Ast.FetchOrientationKind.Absolute, old.Offset is null ? null : Values.Of(old.Offset)),
				"RELATIVE" => new Ast.FetchOrientation(Ast.FetchOrientationKind.Relative, old.Offset is null ? null : Values.Of(old.Offset)),
				_          => throw BridgeGap.Open(old, $"inventory Statement.Fetch (`{old.Orientation}`)"),
			};

			return new Ast.Statement.FetchCursor
			{
				Orientation = orientation,
				FromKeyword = old.From,
				Cursor      = Reference(old, old.Cursor, old.Global),
				Into        = old.Into is { } into ? new Ast.DynamicArguments.Values(Each(into, Values.Of)) : null,
			};
		}

		/// <summary>
		/// A block, and a natively compiled one's settings: the isolation level, whose name says TRAN or TRANSACTION, and
		/// the five others, a name and a value each.
		/// </summary>
		static Ast.Statement Compound(Statement.Compound old)
		{
			return new Ast.Statement.Compound(Each(old.Statements, StatementOf))
			{
				Atomic = old.Atomic is { } atomic ? Each(atomic, one => AtomicOption(old, one)) : null,
			};
		}

		static Ast.AtomicOption AtomicOption(Statement old, Clause option)
		{
			if (option is not Clause.Option { Name: var name, Value: { } value, Options.Length: 0, Partitions: null, Bare: false })
				throw BridgeGap.Open(old, "inventory Statement.Compound (an atomic option that is not a name and a value)");

			var words = Words(name);

			if (words is ["TRANSACTION" or "TRAN", "ISOLATION", "LEVEL"])
			{
				Ast.IsolationLevel level = value is Expression.ColumnReference(var text) ? string.Join(' ', Words(text)) switch
				{
					"READ UNCOMMITTED" => Ast.IsolationLevel.ReadUncommitted,
					"READ COMMITTED"   => Ast.IsolationLevel.ReadCommitted,
					"REPEATABLE READ"  => Ast.IsolationLevel.RepeatableRead,
					"SERIALIZABLE"     => Ast.IsolationLevel.Serializable,
					"SNAPSHOT"         => Ast.IsolationLevel.Snapshot,
					_                  => throw BridgeGap.Open(old, $"inventory Statement.Compound (the level `{text}`)"),
				} : throw BridgeGap.Open(old, "inventory Statement.Compound (a level that is not words)");

				return new Ast.AtomicOption.Isolation(level) { Tran = words[0] == "TRAN" };
			}

			if (words.Length != 1)
				throw BridgeGap.Open(old, $"inventory Statement.Compound (the atomic option `{name}`)");

			return new Ast.AtomicOption.Setting(new Ast.Option(new Ast.Identifier(name), Values.Of(value), []));
		}
	}
}
