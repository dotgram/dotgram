using System;
using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DotGram.Cli;

/// <summary>
/// What the build tells the generators through analyzer configuration — <c>.editorconfig</c> and
/// <c>.globalconfig</c> files, and the <c>build_property.*</c> of the one MSBuild writes under
/// <c>obj</c> — by tree, by additional file, and globally: one provider over one set, as the compiler
/// gives them to a generator.
/// </summary>
sealed class ConfiguredOptions : AnalyzerConfigOptionsProvider
{
	static readonly ImmutableDictionary<string, string> NoValues = ImmutableDictionary.Create<string, string>(StringComparer.OrdinalIgnoreCase);

	static readonly Options None = new(NoValues);

	readonly AnalyzerConfigSet?               _set;
	readonly Options                          _global;
	readonly Dictionary<SyntaxTree, Options>  _trees;
	readonly Dictionary<AdditionalText, Options> _texts;

	ConfiguredOptions(AnalyzerConfigSet? set, Options global, Dictionary<SyntaxTree, Options> trees, Dictionary<AdditionalText, Options> texts)
	{
		_set    = set;
		_global = global;
		_trees  = trees;
		_texts  = texts;
	}

	/// <summary>No configuration at all: a lone grammar has no build to give any.</summary>
	public static ConfiguredOptions Empty { get; } = new(null, None, [], []);

	/// <summary>The configuration files the compiler was given, in its order, applied to each source and additional file.</summary>
	/// <exception cref="ToolException">A configuration file cannot be read.</exception>
	public static ConfiguredOptions Read(IEnumerable<string> paths, IEnumerable<SyntaxTree> trees, IEnumerable<AdditionalText> texts)
	{
		var configs = new List<AnalyzerConfig>();

		foreach (var path in paths)
		{
			try
			{
				configs.Add(AnalyzerConfig.Parse(File.ReadAllText(path), path));
			}
			catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
			{
				throw ToolException.Failure($"cannot read '{path}': {exception.Message}");
			}
		}

		var set = AnalyzerConfigSet.Create(configs);

		var byTree = new Dictionary<SyntaxTree, Options>();
		var byText = new Dictionary<AdditionalText, Options>();

		foreach (var tree in trees)
			byTree[tree] = new Options(set.GetOptionsForSourcePath(tree.FilePath).AnalyzerOptions);

		foreach (var text in texts)
			byText[text] = new Options(set.GetOptionsForSourcePath(text.Path).AnalyzerOptions);

		return new ConfiguredOptions(set, new Options(set.GlobalConfigOptions.AnalyzerOptions), byTree, byText);
	}

	/// <summary>The same, with one more global option: what the tool itself asks of the generator.</summary>
	public ConfiguredOptions WithGlobal(string key, string value)
	{
		return new ConfiguredOptions(_set, new Options(_global.Values.SetItem(key, value)), _trees, _texts);
	}

	/// <summary>
	/// The severities the configuration files give diagnostics (<c>dotnet_diagnostic.ID.severity</c>) for a
	/// file, by its full path: those of the sections that apply to it, then those of the global files.
	/// </summary>
	/// <param name="path">The file a diagnostic is in, or empty where it is in none.</param>
	public (ImmutableDictionary<string, ReportDiagnostic> ForFile, ImmutableDictionary<string, ReportDiagnostic> Global) SeveritiesFor(string path)
	{
		if (_set is null)
			return (NoSeverities, NoSeverities);

		var global = _set.GlobalConfigOptions.TreeOptions;

		return (path.Length > 0 ? _set.GetOptionsForSourcePath(path).TreeOptions : NoSeverities, global);
	}

	static readonly ImmutableDictionary<string, ReportDiagnostic> NoSeverities = ImmutableDictionary.Create<string, ReportDiagnostic>(StringComparer.OrdinalIgnoreCase);

	public override AnalyzerConfigOptions GlobalOptions
	{
		get { return _global; }
	}

	/// <summary>A tree the compiler did not read — a generator's own output — has no options, as in the compiler.</summary>
	public override AnalyzerConfigOptions GetOptions(SyntaxTree tree)
	{
		return _trees.TryGetValue(tree, out var options) ? options : None;
	}

	public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
	{
		return _texts.TryGetValue(textFile, out var options) ? options : None;
	}

	sealed class Options(ImmutableDictionary<string, string> values) : AnalyzerConfigOptions
	{
		public ImmutableDictionary<string, string> Values { get; } = values;

		public override bool TryGetValue(string key, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out string? value)
		{
			return Values.TryGetValue(key, out value);
		}

		public override IEnumerable<string> Keys
		{
			get { return Values.Keys; }
		}
	}
}
