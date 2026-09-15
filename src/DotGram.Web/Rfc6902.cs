using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;

namespace DotGram.Web;

/// <summary>A JSON Patch document as RFC 6902 defines one: operations applied in order.</summary>
/// <remarks>
/// <para>
/// A patch is read from JSON by <see cref="Parse"/> or <see cref="Read"/> and applied by <see cref="Apply"/>. Applying
/// changes nothing it is given: a <see cref="JsonValue"/> cannot change, so the result is a new document that
/// shares what the patch did not touch, and a patch that fails leaves no half-patched document behind (§5).
/// </para>
/// </remarks>
/// <param name="Operations">In the order they are applied.</param>
public sealed record JsonPatch(IReadOnlyList<JsonPatch.Operation> Operations)
{
	/// <summary>The patch a JSON text holds (RFC 6902 §3): an array of operation objects.</summary>
	/// <exception cref="FormatException">The text is not JSON.</exception>
	/// <exception cref="JsonPatchException">The text is JSON but no patch; the message says where.</exception>
	public static JsonPatch Parse(string text) => Rfc6902.ReadPatch(JsonValue.Parse(text));

	/// <summary>The patch a JSON document holds.</summary>
	/// <exception cref="JsonPatchException">The document is no patch; the message says where.</exception>
	public static JsonPatch Read(JsonValue document) => Rfc6902.ReadPatch(document);

	/// <summary>The patch a JSON document holds, or why it holds none.</summary>
	public static bool TryRead(JsonValue document, [NotNullWhen(true)] out JsonPatch? patch, [NotNullWhen(false)] out string? error) =>
		Rfc6902.TryReadPatch(document, out patch, out error);

	/// <summary>Whether two values are equal as §4.6 compares them for <c>test</c>.</summary>
	/// <remarks>
	/// Strings by their code units; numbers by value, so <c>1</c>, <c>1.0</c> and <c>10e-1</c> are equal, at any
	/// precision; arrays element by element; objects by members whatever their order, each matched once by name
	/// and value; literals by being the same.
	/// </remarks>
	public static bool AreEqual(JsonValue left, JsonValue right) => Rfc6902.AreEqual(left, right);

	/// <summary>Equal to another patch with equal operations in the same order.</summary>
	public bool Equals(JsonPatch? other) => other is not null && Structural.Same(Operations, other.Operations);

	public override int GetHashCode() => Structural.Hash(Operations);

	/// <summary>One of §4's six operations. A closed set: the six are nested here and nothing outside can add a seventh.</summary>
	public abstract record Operation
	{
		Operation(JsonPointer path) => Path = path;

		/// <summary>The target location.</summary>
		public JsonPointer Path { get; }

		/// <summary>§4.1: <see cref="Value"/> at the target location — a new member, a replaced one, or an element inserted.</summary>
		public sealed record Add(JsonPointer Path, JsonValue Value) : Operation(Path);

		/// <summary>§4.2: the value at the target location taken out.</summary>
		public sealed record Remove(JsonPointer Path) : Operation(Path);

		/// <summary>§4.3: the value at the target location, which has to exist, replaced by <see cref="Value"/>.</summary>
		public sealed record Replace(JsonPointer Path, JsonValue Value) : Operation(Path);

		/// <summary>§4.4: the value at <see cref="From"/> taken out and added at the target location.</summary>
		public sealed record Move(JsonPointer From, JsonPointer Path) : Operation(Path);

		/// <summary>§4.5: the value at <see cref="From"/> added at the target location.</summary>
		public sealed record Copy(JsonPointer From, JsonPointer Path) : Operation(Path);

		/// <summary>§4.6: whether the value at the target location equals <see cref="Value"/>, as <see cref="AreEqual"/> compares.</summary>
		public sealed record Test(JsonPointer Path, JsonValue Value) : Operation(Path);

		/// <summary>The operation as a patch document writes it.</summary>
		public JsonValue ToJson()
		{
			var members = new List<KeyValuePair<string, JsonValue>>(3)
			{
				Member("op", Name),
			};

			if (this is Move { From: var movedFrom })
				members.Add(Member("from", movedFrom.ToString()));
			else if (this is Copy { From: var copiedFrom })
				members.Add(Member("from", copiedFrom.ToString()));

			members.Add(Member("path", Path.ToString()));

			if (this is Add { Value: var added })
				members.Add(new("value", added));
			else if (this is Replace { Value: var replacement })
				members.Add(new("value", replacement));
			else if (this is Test { Value: var expected })
				members.Add(new("value", expected));

			return new JsonValue.Object(members);
		}

		/// <summary>The operation as compact JSON text.</summary>
		/// <remarks>Sealed, or each case would print itself the way a record does instead.</remarks>
		public sealed override string ToString() => ToJson().ToString();

		string Name => this switch
		{
			Add     => "add",
			Remove  => "remove",
			Replace => "replace",
			Move    => "move",
			Copy    => "copy",
			_       => "test",
		};

		static KeyValuePair<string, JsonValue> Member(string name, string value) => new(name, new JsonValue.String(value));
	}

	/// <summary>The document this patch makes of <paramref name="document"/>.</summary>
	/// <exception cref="JsonPatchException">An operation was not successful; the message says which and why.</exception>
	public JsonValue Apply(JsonValue document) =>
		TryApply(document, out var result, out var error) ? result : throw new JsonPatchException(error);

	/// <summary>The document this patch makes of <paramref name="document"/>, or why it makes none.</summary>
	public bool TryApply(JsonValue document, [NotNullWhen(true)] out JsonValue? result, [NotNullWhen(false)] out string? error)
	{
		if (document is null)
			throw new ArgumentNullException(nameof(document));

		result = document;

		for (var index = 0; index < Operations.Count; index++)
		{
			if (Rfc6902.Applied(result, Operations[index], out error) is not { } next)
			{
				error = $"Operation {index} ({Operations[index]}): {error}";
				result = null;
				return false;
			}

			result = next;
		}

		error = null;
		return true;
	}

	/// <summary>The patch as a patch document writes it: an array of operation objects.</summary>
	public JsonValue ToJson()
	{
		var items = new JsonValue[Operations.Count];

		for (var index = 0; index < items.Length; index++)
			items[index] = Operations[index].ToJson();

		return new JsonValue.Array(items);
	}

	/// <summary>The patch as compact JSON text.</summary>
	public override string ToString() => ToJson().ToString();
}

/// <summary>A JSON Patch document that is not one, or an operation that was not successful (RFC 6902 §5).</summary>
public sealed class JsonPatchException(string message) : Exception(message);

// RFC 6902, JavaScript Object Notation (JSON) Patch. There is no grammar here: a patch document's syntax is
// JSON's, which Rfc8259 reads, and a path is a JSON Pointer, which Rfc6901 reads. What is here is §4's
// meaning of the operations and §5's rule that a patch is applied whole or not at all.
//
// Where the RFC leaves room:
//
//   * `op` and `path` appear once, and so does `value` or `from` where the operation takes one; a member
//     written twice is an error, since JSON keeps both and the RFC says which counts for neither. Members
//     an operation does not define are ignored, however often they are written (§4).
//   * A member name an object holds twice is a location that does not exist, as Rfc6901 resolves it (§4 of
//     RFC 6901 calls it undefined), so adding there replaces nothing, and removing there fails.
//   * `remove` of the whole document is an error. Erratum 4787, held for document update, asks for that,
//     and a document with nothing in it is not a JSON value.

/// <summary>RFC 6902's JSON Patch: reading a patch document, and the equality its <c>test</c> operation asks.</summary>
static class Rfc6902
{
	/// <summary>The patch a JSON Patch document holds.</summary>
	/// <exception cref="JsonPatchException">The document is not a patch; the message says where.</exception>
	public static JsonPatch ReadPatch(JsonValue document) =>
		TryReadPatch(document, out var patch, out var error) ? patch : throw new JsonPatchException(error);

	/// <summary>The patch a JSON text holds.</summary>
	/// <exception cref="JsonPatchException">The text is JSON but not a patch.</exception>
	public static JsonPatch ParsePatch(string text) => ReadPatch(Rfc8259.ParseJson(text));

	/// <summary>The patch a JSON Patch document holds, or why it holds none.</summary>
	public static bool TryReadPatch(JsonValue document, [NotNullWhen(true)] out JsonPatch? patch, [NotNullWhen(false)] out string? error)
	{
		if (document is null)
			throw new ArgumentNullException(nameof(document));

		patch = null;

		if (document is not JsonValue.Array items)
		{
			error = "A patch document is an array of operations (§3).";
			return false;
		}

		var operations = new JsonPatch.Operation[items.Items.Count];

		for (var index = 0; index < operations.Length; index++)
		{
			if (Operation(items.Items[index], out error) is not { } operation)
			{
				error = $"Operation {index}: {error}";
				return false;
			}

			operations[index] = operation;
		}

		patch = new JsonPatch(operations);
		error = null;
		return true;
	}

	/// <summary>Whether two values are equal as §4.6 compares them for <c>test</c>.</summary>
	/// <remarks>
	/// Strings by their code units; numbers by value, so <c>1</c>, <c>1.0</c> and <c>10e-1</c> are equal, at any
	/// precision; arrays element by element; objects by members whatever their order, each matched once by name
	/// and value; literals by being the same.
	/// </remarks>
	public static bool AreEqual(JsonValue left, JsonValue right)
	{
		if (left is null)
			throw new ArgumentNullException(nameof(left));

		if (right is null)
			throw new ArgumentNullException(nameof(right));

		switch (left, right)
		{
			case (JsonValue.String one, JsonValue.String other):
				return string.Equals(one.Value, other.Value, StringComparison.Ordinal);

			case (JsonValue.Number one, JsonValue.Number other):
				return Normal(one.Text) == Normal(other.Text);

			case (JsonValue.Boolean one, JsonValue.Boolean other):
				return one.Value == other.Value;

			case (JsonValue.Null, JsonValue.Null):
				return true;

			case (JsonValue.Array one, JsonValue.Array other):
				if (one.Items.Count != other.Items.Count)
					return false;

				for (var index = 0; index < one.Items.Count; index++)
					if (!AreEqual(one.Items[index], other.Items[index]))
						return false;

				return true;

			case (JsonValue.Object one, JsonValue.Object other):
				if (one.Members.Count != other.Members.Count)
					return false;

				var unmatched = new Dictionary<string, List<JsonValue>>(StringComparer.Ordinal);

				foreach (var member in other.Members)
				{
					if (!unmatched.TryGetValue(member.Key, out var values))
						unmatched.Add(member.Key, values = []);

					values.Add(member.Value);
				}

				foreach (var member in one.Members)
				{
					if (!unmatched.TryGetValue(member.Key, out var values))
						return false;

					var found = values.FindIndex(value => AreEqual(member.Value, value));

					if (found < 0)
						return false;

					values.RemoveAt(found);
				}

				return true;

			default:
				return false;
		}
	}

	// ── Reading ──────────────────────────────────────────────────────────────────

	static JsonPatch.Operation? Operation(JsonValue item, out string? error)
	{
		if (item is not JsonValue.Object members)
		{
			error = "An operation is an object (§3).";
			return null;
		}

		if (Single(members, "op", out error) is not { } op)
			return null;

		if (op is not JsonValue.String { Value: var name })
		{
			error = "'op' is a string (§4).";
			return null;
		}

		if (name is not ("add" or "remove" or "replace" or "move" or "copy" or "test"))
		{
			error = $"'{name}' is no operation (§4).";
			return null;
		}

		if (Pointer(members, "path", out error) is not { } path)
			return null;

		switch (name)
		{
			case "remove":
				return new JsonPatch.Operation.Remove(path);

			case "move" or "copy":
				if (Pointer(members, "from", out error) is not { } from)
					return null;

				return name == "move" ? new JsonPatch.Operation.Move(from, path) : new JsonPatch.Operation.Copy(from, path);

			default:
				if (Single(members, "value", out error) is not { } value)
					return null;

				return name switch
				{
					"add"     => new JsonPatch.Operation.Add(path, value),
					"replace" => new JsonPatch.Operation.Replace(path, value),
					_         => new JsonPatch.Operation.Test(path, value),
				};
		}
	}

	/// <summary>The value of the one member of a name, or null with the reason where there is none or more than one.</summary>
	static JsonValue? Single(JsonValue.Object members, string name, out string? error)
	{
		JsonValue? found = null;

		foreach (var member in members.Members)
		{
			if (!string.Equals(member.Key, name, StringComparison.Ordinal))
				continue;

			if (found is not null)
			{
				error = $"'{name}' is written more than once.";
				return null;
			}

			found = member.Value;
		}

		error = found is null ? $"'{name}' is missing." : null;
		return found;
	}

	static JsonPointer? Pointer(JsonValue.Object members, string name, out string? error)
	{
		if (Single(members, name, out error) is not { } value)
			return null;

		if (value is not JsonValue.String { Value: var text })
		{
			error = $"'{name}' is a string holding a JSON Pointer.";
			return null;
		}

		var pointer = Rfc6901.TryParsePointer(text);

		if (!pointer.IsSuccess)
		{
			error = $"'{name}' is no JSON Pointer: '{text}'.";
			return null;
		}

		return pointer.Value;
	}

	// ── Applying ─────────────────────────────────────────────────────────────────

	enum Change
	{
		Add,
		Remove,
		Replace,
	}

	/// <summary>The document one operation makes, or null with the reason.</summary>
	internal static JsonValue? Applied(JsonValue document, JsonPatch.Operation operation, out string? error)
	{
		switch (operation)
		{
			case JsonPatch.Operation.Add add:
				return Changed(document, add.Path.Tokens, 0, Change.Add, add.Value, out error);

			case JsonPatch.Operation.Remove remove:
				return Changed(document, remove.Path.Tokens, 0, Change.Remove, null, out error);

			case JsonPatch.Operation.Replace replace:
				return Changed(document, replace.Path.Tokens, 0, Change.Replace, replace.Value, out error);

			case JsonPatch.Operation.Move move:
			{
				if (move.From.Resolve(document) is not { } moved)
				{
					error = $"'{move.From}' does not exist.";
					return null;
				}

				if (move.From.Equals(move.Path))
				{
					error = null;
					return document;
				}

				if (IsPrefix(move.From, move.Path))
				{
					error = $"'{move.From}' cannot be moved into its own child '{move.Path}' (§4.4).";
					return null;
				}

				if (Changed(document, move.From.Tokens, 0, Change.Remove, null, out error) is not { } removed)
					return null;

				return Changed(removed, move.Path.Tokens, 0, Change.Add, moved, out error);
			}

			case JsonPatch.Operation.Copy copy:
				if (copy.From.Resolve(document) is not { } copied)
				{
					error = $"'{copy.From}' does not exist.";
					return null;
				}

				return Changed(document, copy.Path.Tokens, 0, Change.Add, copied, out error);

			case JsonPatch.Operation.Test test:
				if (test.Path.Resolve(document) is not { } actual)
				{
					error = $"'{test.Path}' does not exist.";
					return null;
				}

				if (!AreEqual(actual, test.Value))
				{
					error = $"'{test.Path}' is {actual}, not {test.Value}.";
					return null;
				}

				error = null;
				return document;

			default:
				throw new ArgumentOutOfRangeException(nameof(operation));
		}
	}

	/// <summary>
	/// <paramref name="node"/> with the change made at the location <paramref name="tokens"/> reach from
	/// <paramref name="at"/> on: each object and array on the way rebuilt, everything beside them shared.
	/// </summary>
	static JsonValue? Changed(JsonValue node, IReadOnlyList<string> tokens, int at, Change change, JsonValue? value, out string? error)
	{
		if (at == tokens.Count)
		{
			if (change == Change.Remove)
			{
				error = "The whole document cannot be removed.";
				return null;
			}

			error = null;
			return value;
		}

		var token = tokens[at];
		var last  = at == tokens.Count - 1;

		switch (node)
		{
			case JsonValue.Object members:
			{
				var index = -1;

				for (var place = 0; place < members.Members.Count; place++)
				{
					if (!string.Equals(members.Members[place].Key, token, StringComparison.Ordinal))
						continue;

					if (index >= 0)
					{
						index = -1;
						break;
					}

					index = place;
				}

				var list = new List<KeyValuePair<string, JsonValue>>(members.Members);

				if (last && change == Change.Add && index < 0)
				{
					list.Add(new(token, value!));
					error = null;
					return new JsonValue.Object(list);
				}

				if (index < 0)
				{
					error = $"'{Location(tokens, at + 1)}' does not exist.";
					return null;
				}

				if (last && change == Change.Remove)
					list.RemoveAt(index);
				else if (Changed(list[index].Value, tokens, at + 1, change, value, out error) is { } inner)
					list[index] = new(token, inner);
				else
					return null;

				error = null;
				return new JsonValue.Object(list);
			}

			case JsonValue.Array items:
			{
				var count = items.Items.Count;

				int? index = last && change == Change.Add && JsonPointer.IsPastTheEnd(token) ? count : JsonPointer.ArrayIndex(token);

				if (index is not { } place || place > count || place == count && !(last && change == Change.Add))
				{
					error = $"'{token}' is no element of the array at '{Location(tokens, at)}' that can be {(last ? change.ToString().ToLowerInvariant() + "d" : "reached")}.";
					return null;
				}

				var list = new List<JsonValue>(items.Items);

				if (last && change == Change.Add)
					list.Insert(place, value!);
				else if (last && change == Change.Remove)
					list.RemoveAt(place);
				else if (Changed(list[place], tokens, at + 1, change, value, out error) is { } inner)
					list[place] = inner;
				else
					return null;

				error = null;
				return new JsonValue.Array(list);
			}

			default:
				error = $"'{Location(tokens, at)}' is {node}, which has no members or elements.";
				return null;
		}
	}

	static bool IsPrefix(JsonPointer prefix, JsonPointer path)
	{
		if (prefix.Tokens.Count >= path.Tokens.Count)
			return false;

		for (var index = 0; index < prefix.Tokens.Count; index++)
			if (!string.Equals(prefix.Tokens[index], path.Tokens[index], StringComparison.Ordinal))
				return false;

		return true;
	}

	static string Location(IReadOnlyList<string> tokens, int count)
	{
		var part = new string[count];

		for (var index = 0; index < count; index++)
			part[index] = tokens[index];

		return new JsonPointer(part).ToString();
	}

	/// <summary>A number's value, written one way: its sign, its significant digits and the power of ten after the last.</summary>
	static (bool Negative, string Digits, BigInteger Exponent) Normal(string text)
	{
		var negative = text[0] == '-';
		var start    = negative ? 1 : 0;
		var e        = text.IndexOfAny(['e', 'E']);
		var end      = e < 0 ? text.Length : e;
		var dot      = text.IndexOf('.', start, end - start);

		var digits   = dot < 0 ? text.Substring(start, end - start) : text.Substring(start, dot - start) + text.Substring(dot + 1, end - dot - 1);
		var exponent = (e < 0 ? BigInteger.Zero : BigInteger.Parse(text.Substring(e + 1), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture))
			- (dot < 0 ? 0 : end - dot - 1);

		digits = digits.TrimStart('0');

		if (digits.Length == 0)
			return (false, "", BigInteger.Zero);

		var trimmed = digits.TrimEnd('0');

		return (negative, trimmed, exponent + (digits.Length - trimmed.Length));
	}
}
