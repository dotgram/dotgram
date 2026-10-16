using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DotGram.Sql.Ast;

public static partial class Sql2023Writer
{
	sealed partial class Text
	{
		/// <summary>
		/// T-SQL's Server family (the server and what runs beside the databases): writes the nodes it owns and answers false for every other.
		/// </summary>
		bool TryPutServer(ISqlNode node)
		{
			switch (node)
			{
				case Statement.Kill kill:
					Word("KILL");
					PutKillTarget(kill, kill.Target);

					if (kill.With is { } with)
					{
						Word("WITH");
						Word(with switch
						{
							KillOption.StatusOnly => "STATUSONLY",
							KillOption.Commit     => "COMMIT",
							KillOption.Rollback   => "ROLLBACK",
							_                     => throw NoText(with),
						});
					}

					return true;

				case Statement.KillStatsJob job:
					Word("KILL STATS JOB");
					PutKillNumber(job, job.Job);
					return true;

				case Statement.KillQueryNotificationSubscription subscription:
					if (subscription.All && subscription.Subscription is not null)
						throw Both(subscription, "All", "Subscription");

					if (!subscription.All && subscription.Subscription is null)
						throw Empty(subscription, "All", "Subscription");

					Word("KILL QUERY NOTIFICATION SUBSCRIPTION");

					if (subscription.Subscription is { } number)
						PutKillNumber(subscription, number);
					else
						Word("ALL");

					return true;

				case Statement.Reconfigure reconfigure:
					Word("RECONFIGURE");

					if (reconfigure.Override)
						Word("WITH OVERRIDE");

					return true;

				case Statement.Shutdown shutdown:
					Word("SHUTDOWN");

					if (shutdown.NoWait)
						Word("WITH NOWAIT");

					return true;

				case Statement.Dbcc dbcc:
					PutDbcc(dbcc);
					return true;

				default:
					return false;
			}
		}

		// ── KILL ──────────────────────────────────────────────────────────────────

		/// <summary>
		/// The grammar's <c>KillNumber</c>: an unsigned numeric literal, which a minus may precede, kept as one token. A plus is
		/// none, and neither is a minus twice.
		/// </summary>
		void PutKillNumber(ISqlNode owner, LiteralValue.Numeric number)
		{
			var digits = number.Text.StartsWith('-') ? number.Text.Substring(1) : number.Text;

			if (!IsNumberOf(new LiteralValue.Numeric(digits, number.Kind), ExternalShape.Number))
				throw Misplaced(owner, "Number", "that is no number the grammar's KillNumber reads: an unsigned numeric literal, a minus before it at most");

			PutSignedNumber(owner, number);
		}

		/// <summary><c>KillSession</c>: a number, or a string (the unit of work), and nothing else.</summary>
		void PutKillTarget(ISqlNode owner, Expression target)
		{
			switch (target)
			{
				case LiteralValue.Numeric number:
					PutKillNumber(owner, number);
					break;

				case LiteralValue.String text when IsQuoted(text):
					PutExpression(text);
					break;

				default:
					throw Misplaced(owner, "Target", "other than a number or a string, in KILL");
			}
		}

		// ── DBCC ──────────────────────────────────────────────────────────────────

		void PutDbcc(Statement.Dbcc dbcc)
		{
			// DbccWord: a word of one part, never in brackets or quotes, a variable or a reserved word.
			if (dbcc.Command.Style != IdentifierStyle.Regular || dbcc.Command.Text.StartsWith('@'))
				throw Misplaced(dbcc, "Command", "that is no plain word: not in brackets or quotes, not a variable");

			Word("DBCC");
			PutOneName(dbcc.Command, "DBCC");

			if (dbcc.Arguments is { } arguments)
			{
				Open();
				Each(arguments, one => PutDbccArgument(one));
				Close();
			}

			if (dbcc.Options.Count > 0)
			{
				Word("WITH");

				for (var at = 0; at < dbcc.Options.Count; at++)
				{
					if (at > 0)
						Tight(",");

					PutDbccOption(dbcc.Options[at], first: at == 0);
				}
			}
		}

		void PutDbccArgument(ExecuteArgument argument)
		{
			if (argument.Output is not null)
				throw Misplaced(argument, "Output", "in DBCC, whose arguments are not passed back");

			if (argument.Name is { } name)
			{
				if (name.Text.StartsWith('@'))
					CheckVariable(name, "DBCC");
				else if (name.Style != IdentifierStyle.Regular)
					throw Misplaced(argument, "Name", "that is not a plain word, in DBCC (DbccWord)");

				if (name.Text.StartsWith('@'))
					Word(name.Text);
				else
					PutOneName(name, "DBCC");

				Word("=");
			}

			PutDbccValue(argument, argument.Value);
		}

		/// <summary>
		/// The grammar's <c>DbccValue</c>: a number, a minus before it allowed, as one token; money; a string, plain or national; a
		/// binary string; NULL; a variable; or a name of one part that is not reserved. Not an expression, not DEFAULT, not <c>a.b</c>.
		/// </summary>
		void PutDbccValue(ISqlNode owner, Expression value)
		{
			switch (value)
			{
				case LiteralValue.Numeric { Kind: NumericLiteralKind.Money } money when IsMoney(money.Text):
					PutExpression(money);
					return;

				case LiteralValue.Numeric { Kind: not NumericLiteralKind.Money } number:
					PutKillNumber(owner, number);
					return;

				case LiteralValue.String text when IsQuoted(text):
				case LiteralValue.Binary { ZeroX: true }:
				case LiteralValue.Null:
					PutExpression(value);
					return;

				case Expression.Variable variable:
					CheckVariable(variable.Name, "DBCC");
					PutExpression(variable);
					return;

				case Expression.Reference { Name: [{ } word] }:
					PutOneName(word, "DBCC");
					return;
			}

			throw Misplaced(owner, "Value", "of a form the grammar's DbccValue does not read: an expression, DEFAULT, a name of two parts");
		}

		/// <summary>
		/// Money as the grammar's <c>MoneyLiteral</c> reads it: one of the currency signs it lists, a sign after it at most, and digits with
		/// a point that has digits before it.
		/// </summary>
		static bool IsMoney(string text)
		{
			return Regex.IsMatch(text, @"^[$¢-¥৲-৳฿៛₠-₱﷼﹩＄￠-￡￥-￦][+-]?[0-9]+(?:\.[0-9]*)?$");
		}

		/// <summary>
		/// A word, <c>MAXDOP = n</c> (a number with no exponent), or <c>WAIT_AT_LOW_PRIORITY (MAX_DURATION = n [MINUTES],
		/// ABORT_AFTER_WAIT = word)</c>; the first of the options may be words joined by JOIN.
		/// </summary>
		void PutDbccOption(Option option, bool first)
		{
			if (option.EqualsOmitted || option.Order is not null || option.Partitions is not null || option.Place is not null)
				throw Misplaced(option, "EqualsOmitted, Order, Partitions or Place", "in DBCC, whose options are words");

			PutDbccWord(option.Name, option);

			if (option.Words.Count > 0)
			{
				if (!first)
					throw Misplaced(option, "Words", "after the first option, which alone reads words joined by JOIN");

				if (option.Words.Count % 2 != 0 || option.Value is not null || option.Options.Count > 0)
					throw Misplaced(option, "Words", "that are not words joined by JOIN, with no value");

				for (var at = 0; at < option.Words.Count; at += 2)
				{
					if (!Plain(option.Words[at], "JOIN"))
						throw Misplaced(option, "Words", "that are not joined by JOIN");

					PutKeyWord(option.Words[at]);
					PutDbccWord(option.Words[at + 1], option);
				}

				return;
			}

			if (Plain(option.Name, "MAXDOP") && option.Value is not null)
			{
				if (option.Options.Count > 0)
					throw Misplaced(option, "Options", "after MAXDOP, which reads none");

				Word("=");

				if (option.Value is not LiteralValue.Numeric number || !IsNumberOf(number, ExternalShape.Exact))
					throw Misplaced(option, "Value", "of MAXDOP that is not a number with no sign and no exponent");

				PutExpression(number);
				return;
			}

			if (option.Options.Count > 0)
			{
				// WAIT_AT_LOW_PRIORITY, with its bracket, each item once.
				if (!Plain(option.Name, "WAIT_AT_LOW_PRIORITY") || option.Value is not null)
					throw Misplaced(option, "Options", "on a word that has none: only WAIT_AT_LOW_PRIORITY is given a bracket");

				var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

				Open();

				for (var at = 0; at < option.Options.Count; at++)
				{
					var item = option.Options[at];

					if (item.Words.Count > 0 || item.Options.Count > 0 || item.EqualsOmitted || item.Value is not { } itemValue || !seen.Add(item.Name.Text))
						throw Misplaced(item, "a setting", "of WAIT_AT_LOW_PRIORITY that is not a name and a value, once");

					if (at > 0)
						Tight(",");

					if (Plain(item.Name, "MAX_DURATION"))
					{
						PutKeyWord(item.Name);
						Word("=");

						var amount = itemValue is Expression.Measured { Unit: var unit, Value: var inner } && Plain(unit, "MINUTES") ? inner : itemValue;

						if (amount is not LiteralValue.Numeric duration || !IsNumberOf(duration, ExternalShape.Number))
							throw Misplaced(item, "Value", "that is not a number in minutes");

						PutExpression(duration);

						if (itemValue is Expression.Measured measured)
							PutKeyWord(measured.Unit);
					}
					else if (Plain(item.Name, "ABORT_AFTER_WAIT"))
					{
						PutKeyWord(item.Name);
						Word("=");

						if (itemValue is not Expression.Reference { Name: [{ } word] })
							throw Misplaced(item, "Value", "that is not a word");

						PutDbccWord(word, item);
					}
					else
						throw Misplaced(item, "Name", "that WAIT_AT_LOW_PRIORITY does not read");
				}

				Close();
				return;
			}

			if (option.Value is not null)
				throw Misplaced(option, "Value", "on a word, which DBCC reads with a value only after MAXDOP");
		}

		/// <summary>A word the grammar's <c>DbccWord</c> reads: a plain name of one part, never a reserved word.</summary>
		void PutDbccWord(Identifier word, ISqlNode owner)
		{
			if (word.Style != IdentifierStyle.Regular || word.Text.StartsWith('@'))
				throw Misplaced(owner, "a word", "that is not a plain word: DBCC reads no name in brackets or quotes, no variable");

			PutOneName(word, "DBCC");
		}
	}
}
