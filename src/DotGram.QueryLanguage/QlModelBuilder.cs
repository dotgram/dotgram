using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DotGram.QueryLanguage;

/// <summary>
/// Declares what a query text may name — the entities, the fields of each and where each field may be
/// used — and is built once into an immutable <see cref="QlModel{TContext}"/>. Not thread-safe.
/// </summary>
/// <typeparam name="TContext">
/// What the host hands every query: its connection, and whatever the entities' sources read from it, such as
/// the tenant of the request.
/// </typeparam>
public sealed class QlModelBuilder<TContext>
{
	/// <summary>Starts an empty model.</summary>
	public QlModelBuilder()
	{
	}

	readonly List<EntityDeclaration>  _entities = [];
	readonly Dictionary<Type, Delegate> _formats  = [];

	/// <summary>
	/// Exposes the rows of the context member <paramref name="source"/> names, <c>db =&gt; db.Orders</c>,
	/// under that member's name, <c>Orders</c>.
	/// </summary>
	/// <param name="source">A lambda whose body is a member of its parameter and nothing else.</param>
	/// <exception cref="ArgumentException">
	/// The body is anything else — a filter, a call: an entity whose rows are chosen by an expression is
	/// given its name with <see cref="Entity{T}(string, Func{TContext, IQueryable{T}})"/>.
	/// </exception>
	public QlEntityBuilder<TContext, T> Entity<T>(Expression<Func<TContext, IQueryable<T>>> source)
	{
		if (source is null)
			throw new ArgumentNullException(nameof(source));

		var body = source.Body;

		while (body is UnaryExpression { NodeType: ExpressionType.Convert } convert)
			body = convert.Operand;

		if (body is not MemberExpression { Expression: ParameterExpression parameter } member || parameter != source.Parameters[0])
			throw new ArgumentException($"The source `{source}` is not a member of the context: give the entity a name.", nameof(source));

		return Add(member.Member.Name, source.Compile());
	}

	/// <summary>
	/// Exposes the rows <paramref name="source"/> returns under <paramref name="name"/>. The source is called for
	/// every query with the query's context, and is where the rows a request may see are chosen:
	/// <c>db =&gt; db.Orders.Where(o =&gt; o.TenantId == db.TenantId)</c>.
	/// </summary>
	public QlEntityBuilder<TContext, T> Entity<T>(string name, Func<TContext, IQueryable<T>> source)
	{
		if (name is null)
			throw new ArgumentNullException(nameof(name));

		if (source is null)
			throw new ArgumentNullException(nameof(source));

		return Add(name, source);
	}

	QlEntityBuilder<TContext, T> Add<T>(string name, Func<TContext, IQueryable<T>> source)
	{
		var entity = new EntityDeclaration(name, typeof(T), context => source(context));

		_entities.Add(entity);

		return new QlEntityBuilder<TContext, T>(this, entity);
	}

	/// <summary>
	/// How a value of <typeparamref name="T"/>, or of <c>T?</c>, is printed in JSON: as the string
	/// <paramref name="format"/> returns, ahead of the built-in formats. <see cref="QlModel{TContext}.QueryData"/>
	/// is not affected and returns the value itself.
	/// </summary>
	/// <param name="format">
	/// Called from the provider's reading of rows, for parallel queries at once: it must be pure and
	/// thread-safe. It is never handed null; a null it returns prints as JSON <c>null</c>.
	/// </param>
	/// <exception cref="ArgumentException">
	/// <typeparamref name="T"/> is <see cref="Nullable{T}"/> (declare the underlying type), <see cref="string"/>,
	/// <see cref="object"/>, an interface or an abstract type, or already has a format. A format applies to
	/// members of exactly its type.
	/// </exception>
	public QlModelBuilder<TContext> Format<T>(Func<T, string?> format)
	{
		if (format is null)
			throw new ArgumentNullException(nameof(format));

		var type = typeof(T);

		if (Nullable.GetUnderlyingType(type) is { } underlying)
			throw new ArgumentException($"A format is declared for `{underlying.Name}`, and applies to `{underlying.Name}?` too.", nameof(format));

		if (type == typeof(string))
			throw new ArgumentException("Strings print as themselves: a format is no place to change what a field holds.", nameof(format));

		if (type == typeof(object) || type.IsInterface || type.IsAbstract)
			throw new ArgumentException($"A format applies to members of exactly its type, and `{type.Name}` is not the type of a member.", nameof(format));

		if (_formats.ContainsKey(type))
			throw new ArgumentException($"`{type.Name}` already has a format.", nameof(format));

		_formats.Add(type, format);

		return this;
	}

	/// <summary>Freezes the declarations into a model.</summary>
	/// <param name="options">The bounds of every query; <see cref="QlOptions.Default"/> when null.</param>
	/// <exception cref="ArgumentException">
	/// A name that is not a name of the language, two names that differ only in case, or a bound that is not
	/// positive.
	/// </exception>
	public QlModel<TContext> Build(QlOptions? options = null)
	{
		options ??= QlOptions.Default;

		Check(options);

		var entities = new Dictionary<string, ModelEntity>(StringComparer.OrdinalIgnoreCase);

		foreach (var declaration in _entities)
		{
			Names.Check(declaration.Name, "entity");

			if (entities.TryGetValue(declaration.Name, out var other))
				throw new ArgumentException($"The entities `{other.Name}` and `{declaration.Name}` have one name.");

			var fields = new Dictionary<string, ModelField>(StringComparer.OrdinalIgnoreCase);

			foreach (var field in declaration.Fields)
			{
				Names.Check(field.Name, $"field of `{declaration.Name}`");

				if (fields.TryGetValue(field.Name, out var twin))
					throw new ArgumentException($"`{declaration.Name}` has the fields `{twin.Name}` ({twin.Member.Name}) and `{field.Name}` ({field.Member.Name}): their names differ only in case.");

				fields.Add(field.Name, field);
			}

			entities.Add(declaration.Name, new ModelEntity(declaration.Name, declaration.Type, declaration.Source, [.. declaration.Fields]));
		}

		return new QlModel<TContext>(entities, new Dictionary<Type, Delegate>(_formats), options);
	}

	static void Check(QlOptions options)
	{
		Positive(options.MaxTextLength,         nameof(options.MaxTextLength));
		Positive(options.MaxSyntaxDepth,        nameof(options.MaxSyntaxDepth));
		Positive(options.MaxNodes,              nameof(options.MaxNodes));
		Positive(options.MaxTreeDepth,          nameof(options.MaxTreeDepth));
		Positive(options.MaxArithmeticOperands, nameof(options.MaxArithmeticOperands));
		Positive(options.MaxInList,             nameof(options.MaxInList));
		Positive(options.MaxColumns,            nameof(options.MaxColumns));
		Positive(options.MaxStringLength,       nameof(options.MaxStringLength));
		Positive(options.MaxParameterBytes,     nameof(options.MaxParameterBytes));
		Positive(options.MaxPagingLevels,       nameof(options.MaxPagingLevels));

		if (options.MaxRows is { } rows)
			Positive(rows, nameof(options.MaxRows));

		if (options.MaxSkip < 0)
			throw new ArgumentException($"{nameof(options.MaxSkip)} is negative.", nameof(options));
	}

	static void Positive(int value, string name)
	{
		if (value <= 0)
			throw new ArgumentException($"{name} is not positive.", "options");
	}

	internal sealed class EntityDeclaration(string name, Type type, Func<TContext, IQueryable> source)
	{
		public string                       Name   { get; } = name;
		public Type                         Type   { get; } = type;
		public Func<TContext, IQueryable>   Source { get; } = source;
		public List<ModelField>             Fields { get; } = [];
	}
}

/// <summary>Declares the fields of one entity. Not thread-safe.</summary>
public sealed class QlEntityBuilder<TContext, T>
{
	internal QlEntityBuilder(QlModelBuilder<TContext> model, QlModelBuilder<TContext>.EntityDeclaration entity)
	{
		_model  = model;
		_entity = entity;
	}

	readonly QlModelBuilder<TContext>                   _model;
	readonly QlModelBuilder<TContext>.EntityDeclaration _entity;

	// RS0026 asks that no two overloads both take optional parameters, so that adding one later cannot make a
	// call ambiguous. These two are the designed pair and differ by a leading required string, which no call
	// to the other can bind to; `use` is optional in both on purpose, so that a field reads the same either way.
#pragma warning disable RS0026

	/// <summary>Exposes a member of <typeparamref name="T"/> under the member's own name.</summary>
	/// <param name="member">A lambda whose body reads a property or a field of its parameter: <c>o =&gt; o.Total</c>.</param>
	/// <param name="use">
	/// Where the field may be named; null is <see cref="QlUse.Select"/> | <see cref="QlUse.Filter"/> |
	/// <see cref="QlUse.Sort"/>, and will never mean more.
	/// </param>
	public QlEntityBuilder<TContext, T> Column<TValue>(Expression<Func<T, TValue>> member, QlUse? use = null)
	{
		var info = Member(member);

		return Add(info.Name, info, use);
	}

	/// <summary>
	/// Exposes a member of <typeparamref name="T"/> as the field <paramref name="name"/>. One member may be
	/// exposed under two names, each with its own uses: an old name kept beside a new one.
	/// </summary>
	/// <inheritdoc cref="Column{TValue}(Expression{Func{T, TValue}}, QlUse?)"/>
	public QlEntityBuilder<TContext, T> Column<TValue>(string name, Expression<Func<T, TValue>> member, QlUse? use = null)
	{
		if (name is null)
			throw new ArgumentNullException(nameof(name));

		return Add(name, Member(member), use);
	}

#pragma warning restore RS0026

	/// <summary>Continues with the next entity, as <see cref="QlModelBuilder{TContext}.Entity{T}(Expression{Func{TContext, IQueryable{T}}})"/>.</summary>
	public QlEntityBuilder<TContext, T2> Entity<T2>(Expression<Func<TContext, IQueryable<T2>>> source)
	{
		return _model.Entity(source);
	}

	/// <summary>Continues with the next entity, as <see cref="QlModelBuilder{TContext}.Entity{T}(string, Func{TContext, IQueryable{T}})"/>.</summary>
	public QlEntityBuilder<TContext, T2> Entity<T2>(string name, Func<TContext, IQueryable<T2>> source)
	{
		return _model.Entity(name, source);
	}

	QlEntityBuilder<TContext, T> Add(string name, MemberInfo member, QlUse? use)
	{
		var uses = use ?? QlUse.Select | QlUse.Filter | QlUse.Sort;

		if (uses == 0 || (uses & ~(QlUse.Select | QlUse.Filter | QlUse.Sort)) != 0)
			throw new ArgumentException($"`{uses}` is not a use of a field: give Select, Filter or Sort, or leave it out.", nameof(use));

		_entity.Fields.Add(new ModelField(name, member, uses, _entity.Fields.Count));

		return this;
	}

	static MemberInfo Member<TValue>(Expression<Func<T, TValue>> member)
	{
		if (member is null)
			throw new ArgumentNullException(nameof(member));

		if (member.Body is MemberExpression { Expression: ParameterExpression parameter, Member: var info } && parameter == member.Parameters[0])
		{
			if (info is PropertyInfo { CanRead: true, GetMethod.IsStatic: false } or FieldInfo { IsStatic: false })
				return info;
		}

		throw new ArgumentException($"`{member}` does not read a member of `{typeof(T).Name}`: a field is a property or a field of the entity, read directly.", nameof(member));
	}
}
