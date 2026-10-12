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
	/// <c>CREATE SEQUENCE</c> and <c>ALTER SEQUENCE</c> have an arm, as the A row here; every other record
	/// throws the gap its inventory row names.
	/// </remarks>
	internal static class Routines
	{
		public static readonly string[] Owns =
		[
			"Statement.CreateProcedure", "Statement.CreateFunction", "Statement.CreateTrigger",
			"Statement.Execute", "Statement.TriggerSwitch", "Statement.AggregateDefinition",
			"Statement.AssemblyDefinition", "Statement.TypeDefinition", "Statement.SynonymDefinition",
			"Statement.SequenceDefinition", "Statement.RuleDefinition", "Statement.DefaultDefinition",
			"Statement.XmlSchemaCollectionDefinition", "Statement.AlterXmlSchemaCollection", "Clause.ParameterDeclaration",
			"Clause.ExecutionContext",
		];

		public static bool TryConvert(object old, [NotNullWhen(true)] out Ast.ISqlNode? made)
		{
			made = old switch
			{
				Statement.SequenceDefinition sequence => Sequence(sequence),
				_ when Owned(Owns, old)               => throw Inventory.Unconverted(old),
				_                                     => null,
			};

			return made is not null;
		}

		/// <summary>
		/// The words that open a sequence's option, after which a data type's text ends.
		/// </summary>
		static readonly string[] Openers = ["START", "INCREMENT", "MINVALUE", "MAXVALUE", "NO", "CYCLE", "CACHE", "RESTART"];

		/// <summary>
		/// <c>CREATE SEQUENCE</c> and <c>ALTER SEQUENCE</c>. The old tree keeps the options twice: as text, which
		/// says <c>NO</c> and the type, and as nodes, which hold the values; the two are read side by side, in the
		/// order written. <c>CACHE</c> has no place in <see cref="Ast.SequenceOption"/>.
		/// </summary>
		static Ast.Statement Sequence(Statement.SequenceDefinition old)
		{
			var words   = old.Tail is { } tail ? Words(tail) : [];
			var options = old.Options ?? [];
			var made    = new List<Ast.SequenceOption>();
			var at      = 0;

			foreach (var option in options)
			{
				if (option is not Clause.Option(var name, var value, _, _, _))
					throw BridgeGap.Open(old, "inventory Statement.SequenceDefinition (an option that is not one)");

				var word = name.ToUpperInvariant();
				bool no;

				// Find the option's own word in the text, and whether NO stood before it.
				while (at < words.Length && words[at] != word)
					at++;

				if (at >= words.Length)
					throw BridgeGap.Lost(old, $"the sequence option {name} is not where the text has it");

				no = at > 0 && words[at - 1] == "NO";
				at++;

				made.Add(word switch
				{
					"AS"        => new Ast.SequenceOption.DataTypeOption(Values.Type(old, TypeText(old.Tail!, word))),
					"START"     => new Ast.SequenceOption.Start(Values.Of(value!)),
					"INCREMENT" => new Ast.SequenceOption.Increment(Values.Of(value!)),
					"MINVALUE"  => no ? new Ast.SequenceOption.Min(null, true) : new Ast.SequenceOption.Min(Values.Of(value!)),
					"MAXVALUE"  => no ? new Ast.SequenceOption.Max(null, true) : new Ast.SequenceOption.Max(Values.Of(value!)),
					"CYCLE"     => new Ast.SequenceOption.Cycle(!no),
					"RESTART"   => new Ast.SequenceOption.Restart(value is null ? null : Values.Of(value)),
					"CACHE"     => throw BridgeGap.Missing(old, "SequenceOption has no CACHE (inventory Statement.SequenceDefinition)"),
					_           => throw BridgeGap.Open(old, $"inventory Statement.SequenceDefinition (the option {name})"),
				});
			}

			var sequence = Values.Name(old, old.Name);

			return old.Verb?.ToUpperInvariant() switch
			{
				"CREATE" => new Ast.Statement.CreateSequence { Name = sequence, Options = [.. made] },
				"ALTER"  => new Ast.Statement.AlterSequence { Name = sequence, Options = [.. made] },
				_        => throw BridgeGap.Open(old, $"inventory Statement.SequenceDefinition (the verb {old.Verb})"),
			};
		}

		/// <summary>
		/// The text of the type after <c>AS</c>: up to the word that opens the next option.
		/// </summary>
		static string TypeText(string tail, string word)
		{
			var start = tail.IndexOf(word, StringComparison.OrdinalIgnoreCase) + word.Length;
			var end   = tail.Length;

			foreach (var opener in Openers)
			{
				var at = tail.IndexOf(" " + opener, start, StringComparison.OrdinalIgnoreCase);

				if (at >= 0 && at < end)
					end = at;
			}

			return tail[start..end].Trim();
		}
	}
}
