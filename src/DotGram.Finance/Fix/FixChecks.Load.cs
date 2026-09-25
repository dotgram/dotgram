using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

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
/// whatever carries it — the names the package's model is written in, which are QuickFIX's. Nothing
/// is looked up or asked first: a name the model does not have is a check the expression language
/// cannot compile, and the refusal names it.
/// </para>
/// <para>
/// A dictionary is read into a copy of the slots it is loaded over, so that the context loaded
/// over is unchanged. A slot the file describes is <em>replaced</em>: the check of a message type,
/// of a component, of a group's entries or of a field the file describes is the file's whole
/// check, and what it does not say the slot no longer asks. A file that mentions no message,
/// component or field leaves that slot as it was. A field the file lists values for and the
/// package has no class for has no slot, and nothing is written for it. A tag is written as
/// its number, <c>(FixTag)11</c>: the file's name for a field need not be the one FixTag gives it.
/// </para>
/// </remarks>
abstract partial class FixChecks
{
	static readonly Assembly Here = typeof(FixChecks).Assembly;

	/// <summary>What a text written for this version opens with: the namespaces its names are in.</summary>
	private protected abstract string Using { get; }

	/// <summary>The name of this version's context, the first parameter of every check.</summary>
	private protected abstract string ContextName { get; }

	/// <summary>The same slots in a new object, to be written to without touching this one.</summary>
	internal FixChecks Clone()
	{
		return (FixChecks)MemberwiseClone();
	}

	/// <summary>A copy of these slots with every check the dictionary describes replaced by the file's.</summary>
	/// <exception cref="FormatException">A check written from the file does not compile: it names what the model does not have.</exception>
	/// <remarks>
	/// Each message, component and coded field of the file is a job: its texts written, handed to the
	/// emitter, and compiled. The jobs run side by side, and the slots they wrote are set in the order
	/// the file said them, so that a slot written twice — the entry of a group two messages carry —
	/// keeps the later text; a file refused in more than one place is refused for the first place in
	/// its order, whichever job failed first in time. The parser's state is a value of each call and
	/// its caches are concurrent, which is what makes that sound.
	/// </remarks>
	internal FixChecks Load(FixDictionary dictionary, Action<string, string>? emitted = null)
	{
		var jobs = new List<Action<Writing>>();

		foreach (var (_, name, members) in dictionary.Messages)
			jobs.Add(writing => writing.Write(name, Using + "(" + ContextName + " context, FixMessage." + name + " message) => {\n" +
				Members(writing, "message", "FixMessage." + name, name, members, dictionary, null) + "return message.IsValid; }"));

		foreach (var (name, members) in dictionary.Components)
			jobs.Add(writing => writing.Write(name, Using + "(" + ContextName + " context, FixMessage message, I" + name + " block) => {\n" +
				Members(writing, "block", "I" + name, name, members, dictionary, null) + "return message.IsValid; }"));

		foreach (var field in dictionary.Fields.Values)
		{
			// A field the standard does not define has no slot, and nothing a check could hold.
			if (field.Codes is not { } codes || FieldSlot(field.Name) is not { } type)
				continue;

			jobs.Add(writing =>
			{
				if (FieldText(type, codes) is { } text)
					writing.Write(field.Name, text);
			});
		}

		var writings = new Writing[jobs.Count];
		var failures = new Exception?[jobs.Count];
		var emitting = emitted is null ? null : new Emitting(emitted);

		// Every job runs to its end, and what is refused is the first job's refusal in the file's
		// order: which job fails first in time is the scheduler's, and a refusal is not allowed to be.
		Parallel.For(0, jobs.Count, at =>
		{
			try
			{
				var writing = new Writing(emitting, at, GetType());

				jobs[at](writing);
				writing.Compile();

				writings[at] = writing;
			}
			catch (Exception e)
			{
				failures[at] = e;
			}
		});

		foreach (var failure in failures)
			if (failure is not null)
				ExceptionDispatchInfo.Capture(failure).Throw();

		var loaded = Clone();

		foreach (var writing in writings)
			foreach (var (slot, _, compiled) in writing.Written)
				slot.SetValue(loaded, compiled);

		return loaded;
	}

	/// <summary>One job of a load: the texts it writes, and the delegates they compile to.</summary>
	sealed class Writing(Emitting? emitting, int job, Type checks)
	{
		public List<(PropertyInfo Slot, string Text, Delegate? Compiled)> Written { get; } = [];

		public void Write(string slot, string text)
		{
			emitting?.Write(job, slot, text);

			var property = checks.GetProperty(slot, BindingFlags.Public | BindingFlags.Instance)
				?? throw new FormatException($"The dictionary describes '{slot}', which this package has no slot for." + Environment.NewLine + text);

			Written.Add((property, text, null));
		}

		public void Compile()
		{
			for (var at = 0; at < Written.Count; at++)
				Written[at] = (Written[at].Slot, Written[at].Text, FixChecks.Compile(Written[at].Slot.PropertyType, Written[at].Text));
		}
	}

	/// <summary>
	/// The emitter shared by the jobs: a slot's file is the text of the last job that wrote the slot,
	/// which is the text the slot keeps, so a job that comes to a slot a later job has written leaves
	/// the file alone.
	/// </summary>
	sealed class Emitting(Action<string, string> emit)
	{
		readonly ConcurrentDictionary<string, Latest> _latest = new(StringComparer.Ordinal);

		sealed class Latest
		{
			public int Job = -1;
		}

		public void Write(int job, string slot, string text)
		{
			var latest = _latest.GetOrAdd(slot, static _ => new Latest());

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
	string Members(Writing writing, string subject, string type, string slot, List<FixDictionary.Member> members, FixDictionary dictionary, string? opener)
	{
		var body = new StringBuilder();

		foreach (var member in members)
		{
			switch (member.Kind)
			{
				case FixDictionary.Member.Field:
					Field(body, subject, member.Name, member.Required, dictionary, opener);
					break;

				case FixDictionary.Member.Component:
				{
					var cast = "((I" + member.Name + ")" + subject + ")";

					if (member.Required)
						body.Append("if (FixValidators.Empty").Append(cast).Append(") FixChecks.Absent(message, ").Append(Tag(First(member, dictionary), dictionary)).Append(");\n")
							.Append("else context.Validators.").Append(member.Name).Append(".Invoke(context, message, ").Append(subject).Append(");\n");
					else
						body.Append("if (!FixValidators.Empty").Append(cast).Append(") context.Validators.").Append(member.Name).Append(".Invoke(context, message, ").Append(subject).Append(");\n");

					break;
				}

				case FixDictionary.Member.Group:
				{
					var counter = member.Name;
					var list    = subject + "." + counter + "Groups";
					var entry   = type + "." + counter + "Group";
					var inner   = slot + "_" + counter;

					Field(body, subject, counter, member.Required, dictionary, opener);

					body.Append("FixChecks.Counted(message, ").Append(subject).Append('.').Append(counter).Append(", ").Append(list).Append(");\n")
						.Append("if (").Append(list).Append(" != null) for (var i = 0; i < ").Append(list).Append(".Count; i++) context.Validators.")
						.Append(inner).Append(".Invoke(context, message, ").Append(list).Append("[i], i);\n");

					// What the file says of the entry is the entry's check, replaced like any other slot.
					if (member.Members.Count > 0)
						writing.Write(inner, Using + "(" + ContextName + " context, FixMessage message, " + entry + " entry, int index) => {\n" +
							Members(writing, "entry", entry, inner, member.Members, dictionary, First(member.Members[0], dictionary)) + "return message.IsValid; }");

					break;
				}
			}
		}

		return body.ToString();
	}

	static void Field(StringBuilder body, string subject, string name, bool required, FixDictionary dictionary, string? opener)
	{
		var value = subject + "." + name;
		var call  = "context.Validators." + name + ".Invoke(context, message, " + value + ");\n";

		// The field an entry opens with is there, or there would be no entry.
		if (name == opener)
			body.Append(call);
		else if (required && opener is not null)
			body.Append("if (").Append(value).Append(" == null) FixChecks.Missing(message, ").Append(Tag(name, dictionary))
				.Append(", entry.").Append(opener).Append(".Position, index);\nelse ").Append(call);
		else if (required)
			body.Append("if (").Append(value).Append(" == null) FixChecks.Missing(message, ").Append(Tag(name, dictionary)).Append(");\nelse ").Append(call);
		else
			body.Append("if (").Append(value).Append(" != null) ").Append(call);
	}

	// A field's tag as the text says it: its number, which is the same in every version, where its
	// name is the file's and may not be the one FixTag gives it. A fragment may name a field it does
	// not describe, and that is the standard's field of that name.
	static string Tag(string name, FixDictionary dictionary)
	{
		if (dictionary.Tags.TryGetValue(name, out var tag) || Enum.TryParse<FixTag>(name, out var standard) && (tag = (int)standard) > 0)
			return "(FixTag)" + tag.ToString(CultureInfo.InvariantCulture);

		throw new FormatException($"The field '{name}' is used and not described.");
	}

	// The first field of a member: the field itself, a group's counter, or a component's first field
	// as the file describes the component — which is how the wire marks where an entry begins, and
	// the tag a required component is said to be missing by.
	static string First(FixDictionary.Member member, FixDictionary dictionary)
	{
		if (member.Kind != FixDictionary.Member.Component)
			return member.Name;

		if (!dictionary.Components.TryGetValue(member.Name, out var members) || members.Count == 0)
			throw new FormatException($"The component '{member.Name}' is used and not described.");

		return First(members[0], dictionary);
	}

	// The class of the field a field's slot is handed, where the name is a field's slot.
	Type? FieldSlot(string name)
	{
		var slot = GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance)?.PropertyType;

		return slot is { IsGenericType: true } && slot.GetGenericTypeDefinition() == typeof(Func<,,,>) && slot.GetGenericArguments()[2] is var field && field.IsSubclassOf(typeof(FixField))
			? field
			: null;
	}

	string? FieldText(Type field, string[] codes)
	{
		var value = field.BaseType!.GetGenericArguments()[0];
		var body  = new StringBuilder();

		if (value == typeof(bool))
			return null;

		body.Append("if (!field.IsValid) FixChecks.Invalid(message, field);\n");

		if (value == typeof(string[]))
		{
			body.Append("else foreach (var code in field.Value) if (");
			Codes(body, "code", codes, value);
			body.Append(") { FixChecks.Invalid(message, field); break; }\n");
		}
		else
		{
			body.Append("else if (");
			Codes(body, "field.Value", codes, value);
			body.Append(") FixChecks.Invalid(message, field);\n");
		}

		return Using + "(" + ContextName + " context, FixMessage message, FixField." + field.Name + " field) => {\n" + body + "return message.IsValid; }";
	}

	// The value is none of the codes: a switch over them, in the literal form its type reads,
	// which compiles to a jump table over a character or a number and a hash over strings.
	static void Codes(StringBuilder body, string subject, string[] codes, Type value)
	{
		body.Append(subject).Append(" switch { ");

		for (var i = 0; i < codes.Length; i++)
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
