using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.Tests;

static partial class Bridge
{
	/// <summary>
	/// The server and what runs beside the databases: resource governance, server configuration, endpoints, availability groups, and the administrative commands.
	/// </summary>
	/// <remarks>
	/// KILL, RECONFIGURE, SHUTDOWN and DBCC are nodes, carried as they stand but for the options of a DBCC, which are read again from
	/// the text they were written in (the old tree upper-cases a word it keeps). The old tree keeps what follows the name of the
	/// other objects as text (<c>Tail</c>), which the arms read token by token as the grammar's rules read it; a text the arm does not
	/// read to its end is a gap.
	/// </remarks>
	internal static class Server
	{
		public static readonly string[] Owns =
		[
			"Statement.ResourcePoolDefinition", "Statement.WorkloadGroupDefinition", "Statement.AlterResourceGovernor",
			"Statement.AlterServerConfiguration", "Statement.AvailabilityGroupDefinition", "Statement.EndpointDefinition",
			"Statement.Kill", "Statement.Reconfigure", "Statement.Shutdown",
			"Statement.Dbcc",
		];

		public static bool TryConvert(object old, [NotNullWhen(true)] out Ast.ISqlNode? made)
		{
			made = old switch
			{
				Statement.Kill kill                    => Kill(kill),
				Statement.Reconfigure reconfigure      => new Ast.Statement.Reconfigure(reconfigure.Override),
				Statement.Shutdown shutdown            => new Ast.Statement.Shutdown(shutdown.NoWait),
				Statement.Dbcc dbcc                    => Dbcc(dbcc),
				_ when Owned(Owns, old)                => throw Inventory.Unconverted(old),
				_                                      => null,
			};

			return made is not null;
		}

		// ── KILL, DBCC ────────────────────────────────────────────────────────────

		static Ast.Statement Kill(Statement.Kill old)
		{
			var target = Values.Of(old.Target);

			switch (old.Kind)
			{
				case "STATS JOB":
					return new Ast.Statement.KillStatsJob(target as Ast.LiteralValue.Numeric ?? throw BridgeGap.Open(old, "inventory Statement.Kill (a statistics job that is no number)"));

				case "QUERY NOTIFICATION SUBSCRIPTION":
					return old.Target is Expression.ColumnReference { Text: var all } && all.Equals("ALL", StringComparison.OrdinalIgnoreCase)
						? new Ast.Statement.KillQueryNotificationSubscription { All = true }
						: new Ast.Statement.KillQueryNotificationSubscription { Subscription = target as Ast.LiteralValue.Numeric ?? throw BridgeGap.Open(old, "inventory Statement.Kill (a subscription that is no number)") };

				case "":
					Ast.KillOption? with = old.With?.ToUpperInvariant() switch
					{
						null         => null,
						"STATUSONLY" => Ast.KillOption.StatusOnly,
						"COMMIT"     => Ast.KillOption.Commit,
						"ROLLBACK"   => Ast.KillOption.Rollback,
						_            => throw BridgeGap.Open(old, $"inventory Statement.Kill (WITH {old.With})"),
					};

					return new Ast.Statement.Kill(target) { With = with };

				default:
					throw BridgeGap.Open(old, $"inventory Statement.Kill ({old.Kind})");
			}
		}

		static Ast.Statement Dbcc(Statement.Dbcc old)
		{
			return new Ast.Statement.Dbcc(Values.Part(old, old.Command))
			{
				Arguments = old.Arguments is { } arguments ? Each(arguments, one => DbccArgument(old, one)) : null,
				Options   = Each(old.Options, one => DbccOption(old, one)),
			};
		}

		static Ast.ExecuteArgument DbccArgument(Statement.Dbcc owner, Expression old)
		{
			return old is Expression.NamedArgument(var name, var value)
				? new Ast.ExecuteArgument(DbccValue(owner, value)) { Name = name.StartsWith('@') ? new Ast.Identifier(name) : Values.Part(owner, name) }
				: new Ast.ExecuteArgument(DbccValue(owner, old));
		}

		/// <summary>A value, money among them: the grammar reads blanks between the sign of money and its digits, which the tree keeps none of.</summary>
		static Ast.Expression DbccValue(Statement.Dbcc owner, Expression old)
		{
			return Values.Of(old) is Ast.LiteralValue.Numeric { Kind: Ast.NumericLiteralKind.Money } money && money.Text.Any(char.IsWhiteSpace)
				? throw BridgeGap.Lost(owner, $"money written with blanks (`{money.Text}`), which the tree writes without")
				: Values.Of(old);
		}

		/// <summary>
		/// An option read again from the text it was written in: the old tree upper-cases the words it keeps, and the tree keeps them as
		/// written.
		/// </summary>
		static Ast.Option DbccOption(Statement.Dbcc owner, Clause clause)
		{
			var read = new Broker.Tail(owner, Spelled(clause) ?? throw BridgeGap.Lost(owner, "the text of an option, which the old tree keeps with its words in capitals"));
			var name = read.Name();

			// Words joined by JOIN: the name, and the rest as its words.
			var words = new List<Ast.Identifier>();

			while (read.Is("JOIN"))
			{
				read.Expect("JOIN");
				words.Add(new Ast.Identifier("JOIN"));
				words.Add(read.Name());
			}

			Ast.Option option;

			if (read.TakePunctuation("="))
				option = new Ast.Option(name, read.SettingValue(), []) { Words = words.ToSqlList() };
			else if (read.IsPunctuation("("))
				option = new Ast.Option(name, null, read.Bracketed()) { Words = words.ToSqlList() };
			else
				option = new Ast.Option(name, null, []) { Words = words.ToSqlList() };

			read.Finish();

			return option;
		}
	}
}
