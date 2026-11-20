using System;
using System.Collections.Generic;

namespace DotGram.QueryLanguage;

/// <summary>
/// The values passed with a text, checked against what the text needs before anything is built: each present, of a
/// type that fits, within its bounds; all of them within <see cref="QlOptions.MaxParameterBytes"/>; every <c>skip</c>
/// zero or more, and all of them together within <see cref="QlOptions.MaxSkip"/>.
/// </summary>
static class Arguments
{
	/// <summary>The values in parameter order, each converted to its parameter's type; a list as the list of its items.</summary>
	public static object?[] Bind(BoundQuery query, IReadOnlyDictionary<string, object?>? given, QlOptions options)
	{
		var values = new object?[query.Parameters.Length];
		var bytes  = 0L;

		foreach (var parameter in query.Parameters)
		{
			if (!TryFind(given, parameter.Name, out var value))
				throw Error(QlErrorCode.Parameter, $"`@{parameter.Name}` has no value.", parameter);

			var type = parameter.Type!.Value;

			if (parameter.IsList)
			{
				var items = value is null ? null : Coercion.Items(value, options.MaxInList) ?? throw Error(QlErrorCode.Parameter, $"`@{parameter.Name}` takes a list of {type.Name}.", parameter);

				if (items is null)
					throw Error(QlErrorCode.Parameter, $"`@{parameter.Name}` takes a list of {type.Name}, and is null.", parameter);

				if (items.Count > options.MaxInList)
					throw Error(QlErrorCode.Limit, $"`@{parameter.Name}` has more than {options.MaxInList} values; at most {options.MaxInList} are taken.", parameter);

				for (var i = 0; i < items.Count; i++)
				{
					items[i] = One(parameter, type, items[i], options);
					bytes   += Coercion.Bytes(items[i]);
				}

				values[parameter.Index] = items;
			}
			else
			{
				values[parameter.Index] = One(parameter, type, value, options);
				bytes                  += Coercion.Bytes(values[parameter.Index]);
			}

			if (bytes > options.MaxParameterBytes)
				throw Error(QlErrorCode.Limit, $"The values are larger than {options.MaxParameterBytes} bytes.", parameter);
		}

		Pages(query, values, options);

		return values;
	}

	static bool TryFind(IReadOnlyDictionary<string, object?>? given, string name, out object? value)
	{
		value = null;

		if (given is null)
			return false;

		if (given.TryGetValue(name, out value))
			return true;

		// The text's names ignore case; a host's dictionary may not.
		foreach (var pair in given)
		{
			if (Names.Comparer.Equals(pair.Key, name))
			{
				value = pair.Value;

				return true;
			}
		}

		return false;
	}

	static object? One(Parameter parameter, ScalarType type, object? value, QlOptions options)
	{
		if (value is null)
		{
			if (!parameter.Nullable || parameter.IsCount)
				throw Error(QlErrorCode.Parameter, $"`@{parameter.Name}` may not be null here.", parameter);

			return null;
		}

		if (!Coercion.TryConvert(value, type, out var converted))
			throw Error(QlErrorCode.Parameter, $"`@{parameter.Name}` takes {(type.Type == QlType.Other ? "a value of type " + type.Name : "a " + type.Name)}, and `{ScalarType.ShortName(value.GetType())}` does not convert to it without loss.", parameter);

		if (converted is string text && text.Length > options.MaxStringLength)
			throw Error(QlErrorCode.Limit, $"`@{parameter.Name}` is longer than {options.MaxStringLength} characters.", parameter);

		if (parameter.IsCount && converted is < 0)
			throw Error(QlErrorCode.Parameter, $"`@{parameter.Name}` is a count of rows: zero or more.", parameter);

		return converted;
	}

	/// <summary>Every <c>skip</c> together, saturating rather than wrapping, within <see cref="QlOptions.MaxSkip"/>.</summary>
	static void Pages(BoundQuery query, object?[] values, QlOptions options)
	{
		var skipped = 0L;

		foreach (var clause in query.Clauses)
		{
			if (clause is not BoundPage { Skip: true } page)
				continue;

			var count = page.Count switch
			{
				BoundLiteral literal     => (int)literal.Value!,
				BoundParameter parameter => (int)values[parameter.Parameter.Index]!,
				_                        => 0,
			};

			skipped = Math.Min(skipped + count, long.MaxValue / 2);

			if (skipped > options.MaxSkip)
				throw new QlException(QlErrorCode.Limit, $"The query skips more than {options.MaxSkip} rows.", page.Position);
		}
	}

	static QlException Error(QlErrorCode code, string message, Parameter parameter)
	{
		return new QlException(code, message, parameter.Position);
	}
}
