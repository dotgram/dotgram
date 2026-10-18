using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.TransactSql;

// The statements whose rules build the common tree themselves. Such a rule hands what it read to a builder of its
// family, which makes the statement of the common tree from it, its parts that are still the T-SQL tree's carried
// across as the bridge carries them; the statement stands where the T-SQL tree holds statements as a Statement.Built,
// which the bridge carries as itself. A reading is carried with the text it is read from, set before it starts
// (Over), so that what a builder reads from the text it can read while the statement is read.
static partial class Bridge
{
	/// <summary>
	/// What a rule hands a builder in place of the T-SQL tree's record: the record's name, under which the gaps of the
	/// statement are counted and said, and where it was written.
	/// </summary>
	internal abstract record Given(string Record) : ISqlSpan
	{
		/// <summary>Where the statement was written, where the rule said so.</summary>
		public SqlSpan Span { get; init; }

		/// <summary>Nothing locates it: it is no node of any tree.</summary>
		void ISqlSpan.Locate(int at, int length)
		{
		}
	}

	/// <summary>
	/// What a rule that defines an object by its name hands its builder, where the T-SQL tree kept the name and the words
	/// after it (<c>Statement.Definition</c>): the same members.
	/// </summary>
	internal sealed record Defining(string Record, string Name) : Given(Record)
	{
		/// <summary>What was written after the name, or null where nothing was.</summary>
		public string? Tail { get; init; }

		/// <summary>The settings written among <see cref="Tail"/>, as nodes, where the rule read them as such.</summary>
		public SqlList<Clause>? Options { get; init; }

		/// <summary>The word the statement opened with, where the syntax lets it be more than one.</summary>
		public string? Verb { get; init; }
	}

	/// <summary>
	/// What a rule hands its builder where its parts were gathered by other rules into a record of the T-SQL tree, which
	/// is then no statement of any tree, only what those rules read.
	/// </summary>
	internal sealed record Read<T>(string Record, T Value) : Given(Record);

	/// <summary>The statements of a text read again and skimmed (<see cref="Skimmed"/>), or null where it is not read.</summary>
	static Statement[]? Skim(string text)
	{
		return Skimmed(() => TransactSqlParser.TryParseSyntaxSql(text)) is { IsSuccess: true, Value: var statements } ? statements : null;
	}

	/// <summary>
	/// The definition the words name, on the common tree, where the T-SQL tree kept the name and no more: the words as the
	/// rule names them, the name, what was written after it, the verb it opened with and the settings among the words.
	/// </summary>
	internal static Statement Defined(string what, string name, string? tail = null, string? verb = null, SqlList<Clause>? options = null)
	{
		var (record, build) = Definition(what);
		var old             = new Defining(record, name) { Tail = tail, Verb = verb is null ? null : Syntax.Squared(verb), Options = options };

		return Hold(old, () => build(old));
	}

	/// <summary>The record of the T-SQL tree a definition's words name, and its builder.</summary>
	static (string Record, Func<Defining, Ast.Statement> Build) Definition(string what)
	{
		return what switch
		{
			"TYPE"                        => ("Statement.TypeDefinition",                Tables.CreateType),
			"XML SCHEMA COLLECTION"       => ("Statement.XmlSchemaCollectionDefinition", static old => Routines.SchemaCollection(old, alter: false)),
			"ALTER XML SCHEMA COLLECTION" => ("Statement.AlterXmlSchemaCollection",      static old => Routines.SchemaCollection(old, alter: true)),
			"SYNONYM"                     => ("Statement.SynonymDefinition",             Tables.Synonym),
			"SEQUENCE"                    => ("Statement.SequenceDefinition",            Tables.Sequence),
			"FULLTEXT INDEX"              => ("Statement.FullTextIndexDefinition",       Tables.CreateFullTextIndex),
			"ALTER FULLTEXT INDEX"        => ("Statement.AlterFullTextIndex",            Tables.AlterFullTextIndex),
			"ASSEMBLY"                    => ("Statement.AssemblyDefinition",            Routines.Assembly),
			"CRYPTOGRAPHIC PROVIDER"      => ("Statement.CryptographicProviderDefinition", Security.CryptographicProvider),
			"EXTERNAL LANGUAGE"           => ("Statement.ExternalLanguageDefinition",    External.Language),
			"EXTERNAL MODEL"              => ("Statement.ExternalModelDefinition",       External.Model),
			"RULE"                        => ("Statement.RuleDefinition",                Routines.Rule),
			"DEFAULT"                     => ("Statement.DefaultDefinition",             Routines.Default),
			"AGGREGATE"                   => ("Statement.AggregateDefinition",           Routines.Aggregate),
			"PARTITION FUNCTION"         => ("Statement.PartitionFunctionDefinition",  Database.Definition),
			"ALTER PARTITION FUNCTION"   => ("Statement.AlterPartitionFunction",       Database.Definition),
			"PARTITION SCHEME"           => ("Statement.PartitionSchemeDefinition",    Database.Definition),
			"ALTER PARTITION SCHEME"     => ("Statement.AlterPartitionScheme",         Database.Definition),
			"FULLTEXT CATALOG"           => ("Statement.FullTextCatalogDefinition",    Database.Definition),
			"ALTER FULLTEXT CATALOG"     => ("Statement.AlterFullTextCatalog",         Database.Definition),
			"FULLTEXT STOPLIST"          => ("Statement.FullTextStopListDefinition",   Database.Definition),
			"ALTER FULLTEXT STOPLIST"    => ("Statement.AlterFullTextStopList",        Database.Definition),
			"SEARCH PROPERTY LIST"       => ("Statement.SearchPropertyListDefinition", Database.Definition),
			"ALTER SEARCH PROPERTY LIST" => ("Statement.AlterSearchPropertyList",      Database.Definition),
			_ => throw new ArgumentOutOfRangeException(nameof(what), what, "No rule hands a definition of these words to the common tree."),
		};
	}

	/// <summary>The span a rule's own extent says, as the T-SQL tree's records are given it.</summary>
	internal static SqlSpan At(TransactSqlParser.SourceSpan span)
	{
		return new SqlSpan(span.Start, span.Length);
	}

	/// <summary>
	/// A statement its rule builds on the common tree, held where the T-SQL tree holds statements: what the builder
	/// made, or the gap it found, which carrying the statement throws as the bridge would have.
	/// </summary>
	/// <remarks>
	/// What building it read from the text is taken off this reading's count and kept with the statement, which puts it
	/// back when it is carried: a statement read and then given up leaves nothing counted.
	/// </remarks>
	static Statement Hold(Given given, Func<Ast.Statement> build)
	{
		if (skimming)
			return new Statement.Built(null, null, null, given);

		var mark = recovered?.Count ?? 0;

		try
		{
			var made = build();

			return new Statement.Built(made, null, Taken(mark), given);
		}
		catch (BridgeGap gap)
		{
			return new Statement.Built(null, gap, Taken(mark), given);
		}
	}

	/// <summary>Whether the reading under way is skimmed: its statements hold what their rules were handed, and are not built.</summary>
	[ThreadStatic] static bool skimming;

	/// <summary>
	/// A text read again only for what its rules hand their builders, which a check compares with what the first reading
	/// handed them: nothing is built, and nothing that building would read again is read.
	/// </summary>
	static T Skimmed<T>(Func<T> read)
	{
		var outer = skimming;

		skimming = true;

		try
		{
			return read();
		}
		finally
		{
			skimming = outer;
		}
	}

	static string[]? Taken(int mark)
	{
		if (recovered is not { } list || list.Count <= mark)
			return null;

		var taken = list.GetRange(mark, list.Count - mark).ToArray();

		list.RemoveRange(mark, list.Count - mark);

		return taken;
	}

	/// <summary>
	/// A reading carried with its text: the text the rules' builders read from, and whether the nodes they carry take
	/// their spans, for as long as the reading lasts.
	/// </summary>
	internal static Carriage Over(string text, bool locate)
	{
		var outer = Carriage.Here();

		source    = text;
		recovered = [];
		locating  = locate;

		return outer;
	}

	/// <summary>The statements whose rules built them on the common tree: each carried as what it holds.</summary>
	internal static class Built
	{
		public static readonly string[] Owns = ["Statement.Built"];

		public static bool TryConvert(object old, [NotNullWhen(true)] out Ast.ISqlNode? made)
		{
			made = null;

			if (old is not Statement.Built built)
				return false;

			if (built.Recovered is { } read)
				recovered?.AddRange(read);

			if (built.Gap is { } gap)
				throw gap;

			made = built.Made!;

			return true;
		}
	}
}
