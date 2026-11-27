using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DotGram.QueryLanguage;

// The members of a JSON message around the rows: the status a message opens with, the columns, the warnings, the
// error of a refusal, and the description of a text. The rows themselves are written by QlJson, inside the provider's
// reading; everything here is written once a message.
//
// A message opens with "status". "started" says the text was accepted and its rows follow, and such a message closes
// with "result" ("completed", or "failed" where the rows stopped short): a message without "result" was cut off.
// "error" says nothing was read, and the message carries the error alone. "ok" opens a description, which reads no
// row and so is whole when it is written.

/// <summary>Writes the parts of the JSON messages that are not rows.</summary>
static class QlJsonDocument
{
	/// <summary><c>{"status":"started","columns":[…]</c>, and the warnings where there are any.</summary>
	public static void Start(StringBuilder json, string[] names, IReadOnlyList<QlWarning> warnings)
	{
		json.Append("{\"status\":\"started\",\"columns\":[");

		for (var i = 0; i < names.Length; i++)
		{
			if (i > 0)
				json.Append(',');

			json.Append(QlJson.Quote(names[i]));
		}

		json.Append(']');

		Warnings(json, warnings);
	}

	/// <summary><c>"count":N,"truncated":…,"result":"completed"}</c>: the end of a message whose every row was read.</summary>
	public static void Complete(StringBuilder json, int count, bool truncated)
	{
		json.Append(",\"count\":");
		json.Append(count.ToString(CultureInfo.InvariantCulture));
		json.Append(",\"truncated\":");
		json.Append(truncated ? "true" : "false");
		json.Append(",\"result\":\"completed\"}");
	}

	/// <summary><c>{"status":"error","error":{"code":…,"message":…,"position":…}}</c>, the position only where there is one.</summary>
	public static string Error(QlError error)
	{
		var json = new StringBuilder();

		json.Append("{\"status\":\"error\",\"error\":");

		Item(json, error.Code.ToString(), error.Message, error.Position);

		json.Append('}');

		return json.ToString();
	}

	/// <summary>
	/// <c>{"status":"ok","columns":[{"name","type","nullable"},…],"parameters":[{"name","type","nullable","list"},…]}</c>,
	/// and the warnings where there are any.
	/// </summary>
	public static string Describe(QlQueryInfo info)
	{
		var json = new StringBuilder();

		json.Append("{\"status\":\"ok\",\"columns\":[");

		for (var i = 0; i < info.Columns.Count; i++)
		{
			var column = info.Columns[i];

			if (i > 0)
				json.Append(',');

			Typed(json, column.Name, column.Type, column.IsNullable);

			json.Append('}');
		}

		json.Append("],\"parameters\":[");

		for (var i = 0; i < info.Parameters.Count; i++)
		{
			var parameter = info.Parameters[i];

			if (i > 0)
				json.Append(',');

			Typed(json, parameter.Name, parameter.Type, parameter.IsNullable);

			json.Append(",\"list\":");
			json.Append(parameter.IsList ? "true" : "false");
			json.Append('}');
		}

		json.Append(']');

		Warnings(json, info.Warnings);

		json.Append('}');

		return json.ToString();
	}

	/// <summary><c>{"name":…,"type":…,"nullable":…</c>, left open for what follows.</summary>
	static void Typed(StringBuilder json, string name, QlType type, bool nullable)
	{
		json.Append("{\"name\":");
		json.Append(QlJson.Quote(name));
		json.Append(",\"type\":\"");
		json.Append(type.ToString());
		json.Append("\",\"nullable\":");
		json.Append(nullable ? "true" : "false");
	}

	/// <summary><c>,"warnings":[…]</c>, or nothing where there are none: a warning is a hint, not a status.</summary>
	static void Warnings(StringBuilder json, IReadOnlyList<QlWarning> warnings)
	{
		if (warnings.Count == 0)
			return;

		json.Append(",\"warnings\":[");

		for (var i = 0; i < warnings.Count; i++)
		{
			if (i > 0)
				json.Append(',');

			Item(json, warnings[i].Code.ToString(), warnings[i].Message, warnings[i].Position);
		}

		json.Append(']');
	}

	/// <summary><c>{"code":…,"message":…,"position":…}</c>: an error or a warning, the position only where there is one.</summary>
	static void Item(StringBuilder json, string code, string message, int? position)
	{
		json.Append("{\"code\":\"");
		json.Append(code);
		json.Append("\",\"message\":");
		json.Append(QlJson.Quote(message));

		if (position is { } at)
		{
			json.Append(",\"position\":");
			json.Append(at.ToString(CultureInfo.InvariantCulture));
		}

		json.Append('}');
	}
}
