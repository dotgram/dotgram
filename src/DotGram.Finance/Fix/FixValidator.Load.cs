using System;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Threading;

using DotGram.ExpressionLanguage;

namespace DotGram.Finance.Fix;

/// <summary>
/// The slots a loaded dictionary fills: the same straight-line checks the package compiles in,
/// written as text from what the file says and compiled by the expression language.
/// </summary>
/// <remarks>
/// <para>
/// The text is the file's names put in place. A message is the class of its name, a field the
/// property of its name, a component the interface <c>I</c> and its name, and a group the list
/// <c>&lt;Counter&gt;Groups</c> of entries of the class <c>&lt;Counter&gt;Group</c> nested in
/// whatever carries it — the names the package's model is written in, which are the dictionary's. A
/// name the model does not have is refused when the dictionary is applied, by asking the model for
/// it, and the refusal names it.
/// </para>
/// <para>
/// A check is compiled when it is first asked, not when the dictionary is applied: a counterparty's
/// file describes every message type and a reading is held to a few, and compiling all of them cost
/// hundreds of milliseconds for checks nothing would run. Everything a compilation could refuse is
/// asked of the model before the context is made, so that a file the package cannot hold is still
/// refused whole, where it is applied, in the file's order.
/// </para>
/// <para>
/// A dictionary is read into a copy of the slots it is loaded over, so that the context loaded
/// over is unchanged. A slot the file describes is <em>replaced</em>: the check of a message type,
/// of a component, of a group's entries or of a field the file describes is the file's whole
/// check, and what it does not say the slot no longer asks. A file that mentions no message,
/// component or field leaves that slot as it was. A field the file lists values for and the
/// package has no class for has no slot, and nothing is written for it. A tag is written as
/// its number, <c>11</c>: the file's name for a field need not be the one FixTag gives it.
/// </para>
/// </remarks>
abstract partial class FixValidator
{
	static readonly Assembly Here = typeof(FixValidator).Assembly;

	/// <summary>What a text written for this version opens with: the namespaces its names are in.</summary>
	private protected abstract string Using { get; }

	/// <summary>The name of this version's context, the first parameter of every check.</summary>
	private protected abstract string ContextName { get; }

	/// <summary>The same slots in a new object, to be written to without touching this one.</summary>
	internal FixValidator Clone()
	{
		return (FixValidator)MemberwiseClone();
	}

	/// <summary>A copy of these slots with every check the dictionary describes replaced by the file's.</summary>
	/// <exception cref="FormatException">The file names what the model does not have: a message, a component, a field in a place, or a slot.</exception>
	/// <remarks>
	/// Each message, component and coded field of the file is written in the file's order, its names
	/// asked of the model as the text is written, and handed to the emitter. A slot written twice — the
	/// entry of a group two messages carry — keeps the later text. What a slot is given is a stand-in
	/// that compiles its text on its first call and puts the compiled check in its own place, so the
	/// second call is the check itself.
	/// </remarks>
	internal FixValidator Load(FixDictionary dictionary, Action<string, string>? emitted = null)
	{
		var source  = new Source(dictionary);
		var writing = new Writing(emitted is null ? null : new Emitting(emitted), GetType());

		foreach (var message in dictionary.Messages)
		{
			var type = MessageClass(message.Name);

			writing.Write(type, Using + "(" + ContextName + " context, FixMessage." + type + " message) => {\n" +
				Members(writing, "message", "FixMessage." + type, type, message.Members, source, null) + "return message.IsValid; }");
		}

		// A component the version writes into its carriers, with no interface of its own, has no slot
		// either: its members are checked where it is carried.
		foreach (var component in dictionary.Components)
			if (Carried(component.Name))
				writing.Write(component.Name, Using + "(" + ContextName + " context, FixMessage message, I" + component.Name + " block) => {\n" +
					Members(writing, "block", "I" + component.Name, component.Name, component.Members, source, null) + "return message.IsValid; }");

		// In the order of their tags, so that the refusal of a file edited in code is the same on every run.
		var tags = new List<int>(dictionary.Fields.Keys);

		tags.Sort();

		foreach (var tag in tags)
		{
			var field = dictionary.Fields[tag];

			// A field the standard does not define has no slot, and nothing a check could hold.
			if (field.Codes.Count == 0 || FieldSlot(FieldSlotName(field.Name)) is not { } type)
				continue;

			if (FieldText(field.Name, type, field.Codes) is { } text)
				writing.Write(FieldSlotName(field.Name), text);
		}

		var loaded = Clone();

		foreach (var (slot, text) in writing.Written)
			slot.SetValue(loaded, Deferred.Of(loaded, slot, text));

		return loaded;
	}

	/// <summary>Compiles every check a load left to be compiled on its first call: what a test asks, to hold them all to the model at once.</summary>
	/// <returns>How many were compiled.</returns>
	internal int CompileDeferred()
	{
		var compiled = 0;

		foreach (var slot in GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
			if (slot.PropertyType.IsSubclassOf(typeof(Delegate)) && slot.GetValue(this) is Delegate { Target: IDeferred deferred })
			{
				deferred.Compiled();
				compiled++;
			}

		return compiled;
	}

	/// <summary>The texts a load writes, each for the slot of its name.</summary>
	sealed class Writing(Emitting? emitting, Type checks)
	{
		int _job;

		public List<(PropertyInfo Slot, string Text)> Written { get; } = [];

		public void Write(string slot, string text)
		{
			emitting?.Write(_job++, slot, text);

			var property = checks.GetProperty(slot, BindingFlags.Public | BindingFlags.Instance)
				?? throw new FormatException($"The dictionary describes '{slot}', which this package has no slot for." + Environment.NewLine + text);

			Written.Add((property, text));
		}
	}

	/// <summary>A dictionary as a load asks it: its components by name, and its fields' tags by name.</summary>
	sealed class Source
	{
		public readonly FixDictionary              Dictionary;
		public readonly Dictionary<string, int>    Tags = new(StringComparer.Ordinal);

		public Source(FixDictionary dictionary)
		{
			Dictionary = dictionary;

			foreach (var field in dictionary.Fields)
			{
				if (Tags.TryGetValue(field.Value.Name, out var already))
					throw new FormatException($"The name '{field.Value.Name}' is given to tags {Math.Min(already, field.Key)} and {Math.Max(already, field.Key)}.");

				Tags.Add(field.Value.Name, field.Key);
			}
		}
	}

	/// <summary>What stands in a slot until its check is first asked.</summary>
	interface IDeferred
	{
		Delegate Compiled();
	}

	/// <summary>
	/// A slot's check, compiled the first time it is called. The compiled check then takes the slot's
	/// place in the checks it belongs to; a context is shared between threads, and two threads that call
	/// it at once each compile it and keep one, which is harmless.
	/// </summary>
	abstract class Deferred(FixValidator owner, PropertyInfo slot, string text) : IDeferred
	{
		Delegate? _compiled;

		public Delegate Compiled()
		{
			if (Volatile.Read(ref _compiled) is { } done)
				return done;

			Interlocked.CompareExchange(ref _compiled, FixValidator.Compile(slot.PropertyType, text), null);
			slot.SetValue(owner, _compiled);

			return _compiled!;
		}

		/// <summary>The stand-in for a slot, a delegate of the slot's own type.</summary>
		public static Delegate Of(FixValidator owner, PropertyInfo slot, string text)
		{
			var types = slot.PropertyType.GetGenericArguments();
			var shape = types.Length switch
			{
				3 => typeof(Of2<,>),
				4 => typeof(Of3<,,>),
				5 => typeof(Of4<,,,>),
				_ => throw new InvalidOperationException($"A slot of {types.Length - 1} parameters: {slot.Name}."),
			};

			var parameters = new Type[types.Length - 1];

			Array.Copy(types, parameters, parameters.Length);

			var stand = Activator.CreateInstance(shape.MakeGenericType(parameters), owner, slot, text)!;

			return Delegate.CreateDelegate(slot.PropertyType, stand, "Invoke");
		}
	}

	sealed class Of2<T1, T2>(FixValidator owner, PropertyInfo slot, string text) : Deferred(owner, slot, text)
	{
		public bool Invoke(T1 a, T2 b)
		{
			return ((Func<T1, T2, bool>)Compiled())(a, b);
		}
	}

	sealed class Of3<T1, T2, T3>(FixValidator owner, PropertyInfo slot, string text) : Deferred(owner, slot, text)
	{
		public bool Invoke(T1 a, T2 b, T3 c)
		{
			return ((Func<T1, T2, T3, bool>)Compiled())(a, b, c);
		}
	}

	sealed class Of4<T1, T2, T3, T4>(FixValidator owner, PropertyInfo slot, string text) : Deferred(owner, slot, text)
	{
		public bool Invoke(T1 a, T2 b, T3 c, T4 d)
		{
			return ((Func<T1, T2, T3, T4, bool>)Compiled())(a, b, c, d);
		}
	}

	/// <summary>
	/// The emitter of a load: a slot's file is the text of the last write to the slot, which is the text
	/// the slot keeps.
	/// </summary>
	sealed class Emitting(Action<string, string> emit)
	{
		readonly Dictionary<string, Latest> _latest = new(StringComparer.Ordinal);

		sealed class Latest
		{
			public int Job = -1;
		}

		public void Write(int job, string slot, string text)
		{
			if (!_latest.TryGetValue(slot, out var latest))
				_latest.Add(slot, latest = new Latest());

			lock (latest)
			{
				if (job < latest.Job)
					return;

				latest.Job = job;
				emit(slot, text);
			}
		}
	}

	static Delegate Compile(Type delegateType, string text)
	{
		var method = typeof(ExpressionParser)
			.GetMethod(nameof(ExpressionParser.Compile), [typeof(string), typeof(Assembly)])!
			.MakeGenericMethod(delegateType);

		try
		{
			return (Delegate)method.Invoke(null, [text, Here])!;
		}
		catch (TargetInvocationException e) when (e.InnerException is not null)
		{
			throw new FormatException("A check written from the dictionary could not be compiled: " + e.InnerException.Message + Environment.NewLine + text, e.InnerException);
		}
	}

	// ── the texts ────────────────────────────────────────────────────────────────────────────────

	// The shape of a compiled-in check, one member at a time, in the file's order: a field asked for
	// when required and handed to its slot when present; a component asked for when required and
	// handed to its slot when it has anything; a group's counter likewise, its count held to its
	// entries and each entry handed to the entry's slot, whose text is written beside this one.
	// `subject` is what holds the members, `type` the C# type a group of it nests in, `slot` the
	// slot name a group of it extends, and `opener` the field an entry opens with.
	string Members(Writing writing, string subject, string type, string slot, List<FixDictionaryMember> members, Source source, string? opener)
	{
		var body    = new StringBuilder();
		var carrier = Model(type);

		foreach (var member in members)
		{
			switch (member.Kind)
			{
				case FixDictionaryMemberKind.Field:
					Holds(carrier, type, member.Name, "field");
					Field(body, subject, member.Name, member.Required, source, opener);
					break;

				case FixDictionaryMemberKind.Component when !Carried(member.Name):
				{
					// FIX 5.0 SP2 writes most of its groups as components, MsgTypeGrp and the rest, and
					// the version reads each as the group it is, in the carrier: so is its check.
					if (!source.Dictionary.Components.Contains(member.Name))
						throw new FormatException($"The component '{member.Name}' is used and not described.");

					body.Append(Members(writing, subject, type, slot, source.Dictionary.Components[member.Name].Members, source, opener));

					break;
				}

				case FixDictionaryMemberKind.Component:
				{
					if (carrier is not null && !typeof(FixDictionary).Assembly.GetType(GetType().Namespace + ".I" + member.Name)!.IsAssignableFrom(carrier))
						throw new FormatException($"The dictionary places the component '{member.Name}' in '{type}', which does not carry it.");

					var cast = "((I" + member.Name + ")" + subject + ")";

					if (member.Required)
						body.Append("if (").Append(GetType().Name).Append(".Empty").Append(cast).Append(") FixValidator.Absent(message, ").Append(Tag(First(member, source), source)).Append(");\n")
							.Append("else context.Validators.").Append(member.Name).Append(".Invoke(context, message, ").Append(subject).Append(");\n");
					else
						body.Append("if (!").Append(GetType().Name).Append(".Empty").Append(cast).Append(") context.Validators.").Append(member.Name).Append(".Invoke(context, message, ").Append(subject).Append(");\n");

					break;
				}

				case FixDictionaryMemberKind.Group:
				{
					var counter = member.Name;
					var list    = subject + "." + counter + "Groups";
					var entry   = type + "." + counter + "Group";
					var inner   = slot + "_" + counter;

					Holds(carrier, type, counter, "group");

					if (carrier is not null && Model(entry) is null)
						throw new FormatException($"The dictionary places the group '{counter}' in '{type}', which does not carry it.");

					Field(body, subject, counter, member.Required, source, opener);

					body.Append("FixValidator.Counted(message, ").Append(subject).Append('.').Append(counter).Append(", ").Append(list).Append(");\n")
						.Append("if (").Append(list).Append(" != null) for (var i = 0; i < ").Append(list).Append(".Count; i++) context.Validators.")
						.Append(inner).Append(".Invoke(context, message, ").Append(list).Append("[i], i);\n");

					// What the file says of the entry is the entry's check, replaced like any other slot.
					if (member.Members.Count > 0)
						writing.Write(inner, Using + "(" + ContextName + " context, FixMessage message, " + entry + " entry, int index) => {\n" +
							Members(writing, "entry", entry, inner, member.Members, source, First(member.Members[0], source)) + "return message.IsValid; }");

					break;
				}
			}
		}

		return body.ToString();
	}

	void Field(StringBuilder body, string subject, string name, bool required, Source source, string? opener)
	{
		var value = subject + "." + name;
		var call  = "context.Validators." + FieldSlotName(name) + ".Invoke(context, message, " + value + ");\n";

		// The field an entry opens with is there, or there would be no entry.
		if (name == opener)
			body.Append(call);
		else if (required && opener is not null)
			body.Append("if (").Append(value).Append(" == null) FixValidator.Missing(message, ").Append(Tag(name, source))
				.Append(", entry.").Append(opener).Append(".Position, index);\nelse ").Append(call);
		else if (required)
			body.Append("if (").Append(value).Append(" == null) FixValidator.Missing(message, ").Append(Tag(name, source)).Append(");\nelse ").Append(call);
		else
			body.Append("if (").Append(value).Append(" != null) ").Append(call);
	}

	// A field's tag as the text says it: its number, which is the same in every version, where its
	// name is the file's and may not be the one FixTag gives it. A fragment may name a field it does
	// not describe, and that is the standard's field of that name: the version's name for it first,
	// FIX 4.4's IOIid, and then FixTag's, which is the newest version's.
	string Tag(string name, Source source)
	{
		if (source.Tags.TryGetValue(name, out var tag) || (tag = FieldTag(name)) > 0 ||
			typeof(FixTag).GetField(name, BindingFlags.Public | BindingFlags.Static)?.GetRawConstantValue() is int standard && (tag = standard) > 0)
			return tag.ToString(CultureInfo.InvariantCulture);

		throw new FormatException($"The field '{name}' is used and not described.");
	}

	// The first field of a member: the field itself, a group's counter, or a component's first field
	// as the file describes the component — which is how the wire marks where an entry begins, and
	// the tag a required component is said to be missing by.
	static string First(FixDictionaryMember member, Source source)
	{
		if (member.Kind != FixDictionaryMemberKind.Component)
			return member.Name;

		if (!source.Dictionary.Components.Contains(member.Name) || source.Dictionary.Components[member.Name].Members.Count == 0)
			throw new FormatException($"The component '{member.Name}' is used and not described.");

		return First(source.Dictionary.Components[member.Name].Members[0], source);
	}

	// The model's type a text names, `FixMessage.NewOrderSingle.NoAllocsGroup` or `IParties`: nested
	// types are joined by a plus in the runtime's name. Null where the model has none.
	Type? Model(string type)
	{
		return GetType().Assembly.GetType(GetType().Namespace + "." + type.Replace('.', '+'));
	}

	// Whether the model's carrier has a member of this name: its own, a base class's or, for an
	// interface, one it extends. What the expression language would refuse to compile, asked first.
	static void Holds(Type? carrier, string type, string name, string what)
	{
		if (carrier is null)
			return;

		const BindingFlags Found = BindingFlags.Public | BindingFlags.Instance;

		if (carrier.GetProperty(name, Found) is not null)
			return;

		if (carrier.IsInterface)
			foreach (var extended in carrier.GetInterfaces())
				if (extended.GetProperty(name, Found) is not null)
					return;

		throw new FormatException($"The dictionary places the {what} '{name}' in '{type}', which has no member of that name.");
	}

	/// <summary>Whether the version carries a component of this name as the thing it is, an interface of its own.</summary>
	bool Carried(string component)
	{
		return GetType().Assembly.GetType(GetType().Namespace + ".I" + component) is not null;
	}

	/// <summary>The class of a message the file names so: its name, except where a version cannot keep it.</summary>
	/// <remarks>
	/// C# gives a member no name its class has, and FIX 5.0 SP2 names a message SecurityStatus that
	/// carries the field SecurityStatus: the class is SecurityStatusMessage, and its slot with it.
	/// </remarks>
	private protected virtual string MessageClass(string name)
	{
		return name;
	}

	/// <summary>The tag of a field the version names otherwise than <see cref="FixTag"/> does, or 0.</summary>
	private protected virtual int FieldTag(string name)
	{
		return 0;
	}

	/// <summary>The slot of a field the file names so: its name, except where a message or a component has the name first.</summary>
	private protected virtual string FieldSlotName(string name)
	{
		return name;
	}

	// The class of the field a field's slot is handed, where the name is a field's slot.
	Type? FieldSlot(string name)
	{
		var slot = GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance)?.PropertyType;

		return slot is { IsGenericType: true } && slot.GetGenericTypeDefinition() == typeof(Func<,,,>) && slot.GetGenericArguments()[2] is var field && field.IsSubclassOf(typeof(FixField))
			? field
			: null;
	}

	string? FieldText(string name, Type field, List<string> codes)
	{
		var value = field.BaseType!.GetGenericArguments()[0];
		var body  = new StringBuilder();

		if (value == typeof(bool))
			return null;

		// A code the check could not be compiled with, asked of the value's type first.
		foreach (var code in codes)
		{
			var fits = value == typeof(char)    ? code.Length == 1
			         : value == typeof(long)    ? long.TryParse(code, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out _)
			         : value == typeof(decimal) ? decimal.TryParse(code, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out _)
			         : value == typeof(string) || value == typeof(string[]);

			if (!fits)
				throw new FormatException($"The field '{name}' lists '{code}', which is not a value of its type, {value.Name}.");
		}

		body.Append("if (!field.IsValid) FixValidator.Invalid(message, field);\n");

		if (value == typeof(string[]))
		{
			body.Append("else foreach (var code in field.Value) if (");
			Codes(body, "code", codes, value);
			body.Append(") { FixValidator.Invalid(message, field); break; }\n");
		}
		else
		{
			body.Append("else if (");
			Codes(body, "field.Value", codes, value);
			body.Append(") FixValidator.Invalid(message, field);\n");
		}

		return Using + "(" + ContextName + " context, FixMessage message, FixField." + field.Name + " field) => {\n" + body + "return message.IsValid; }";
	}

	// The value is none of the codes: a switch over them, in the literal form its type reads,
	// which compiles to a jump table over a character or a number and a hash over strings.
	static void Codes(StringBuilder body, string subject, List<string> codes, Type value)
	{
		body.Append(subject).Append(" switch { ");

		for (var i = 0; i < codes.Count; i++)
		{
			if (i > 0) body.Append(" or ");

			if (value == typeof(char))
				body.Append('\'').Append(codes[i] == "\\" || codes[i] == "'" ? "\\" + codes[i] : codes[i]).Append('\'');
			else if (value == typeof(string) || value == typeof(string[]))
				body.Append('"').Append(codes[i].Replace("\\", "\\\\").Replace("\"", "\\\"")).Append('"');
			else
				body.Append(codes[i]);
		}

		body.Append(" => false, _ => true }");
	}
}
