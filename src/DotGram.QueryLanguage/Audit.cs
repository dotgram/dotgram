using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace DotGram.QueryLanguage;

/// <summary>
/// The check of the finished tree, always on: an iterative walk that holds every node to one
/// table of what the translator may build. It does not trust the translator — a node the table
/// does not list is refused whoever built it — and it counts the tree linq2db will be given, not
/// the text: depth and size are measured here, on the output.
/// </summary>
static class Audit
{
	static readonly HashSet<ExpressionType> Arithmetic =
	[
		ExpressionType.Add, ExpressionType.Subtract, ExpressionType.Multiply, ExpressionType.Divide, ExpressionType.Modulo,
	];

	static readonly HashSet<ExpressionType> Comparisons =
	[
		ExpressionType.Equal, ExpressionType.NotEqual, ExpressionType.LessThan, ExpressionType.LessThanOrEqual,
		ExpressionType.GreaterThan, ExpressionType.GreaterThanOrEqual,
	];

	static readonly HashSet<Type> Scalars =
	[
		typeof(bool), typeof(int), typeof(long), typeof(decimal), typeof(double), typeof(string),
		typeof(DateTime), typeof(DateTimeOffset), typeof(Guid), typeof(char),
	];

	/// <summary>The operator methods of the language's types: what C# compiles an operator on them into.</summary>
	static readonly HashSet<MethodInfo> Operators = BuildOperators();

	static HashSet<MethodInfo> BuildOperators()
	{
		var names = new HashSet<string>(StringComparer.Ordinal)
		{
			"op_Addition", "op_Subtraction", "op_Multiply", "op_Division", "op_Modulus", "op_UnaryNegation",
			"op_Equality", "op_Inequality", "op_LessThan", "op_LessThanOrEqual", "op_GreaterThan", "op_GreaterThanOrEqual",
		};

		var methods = new HashSet<MethodInfo>();

		foreach (var type in new[] { typeof(decimal), typeof(DateTime), typeof(DateTimeOffset), typeof(Guid), typeof(string) })
			foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
				if (names.Contains(method.Name))
					methods.Add(method);

		return methods;
	}

	public readonly record struct Stats(int Nodes, int Depth);

	public static Stats Check(Expression tree, Expression root, QlEntity entity, IQlAdapter adapter, QlLimits limits)
	{
		var members = new HashSet<MemberInfo> { entity.Key };

		foreach (var column in entity.Columns)
			members.Add(column.Member);

		var nodes    = 0;
		var deepest  = 0;
		var stack    = new Stack<(Expression Node, int Depth, ParameterExpression? Row)>();
		var children = new List<Expression>(4);

		stack.Push((tree, 1, null));

		while (stack.Count > 0)
		{
			var (node, depth, row) = stack.Pop();

			// The root is the host's: its policy, its mapping. It is not the text's, and not counted.
			if (ReferenceEquals(node, root))
				continue;

			if (++nodes > limits.MaxNodes)
				throw Refuse($"the tree has more than {limits.MaxNodes} nodes", QlErrorKind.Limit);

			if (depth > deepest)
				deepest = depth;

			if (depth > limits.MaxTreeDepth)
				throw Refuse($"the tree is deeper than {limits.MaxTreeDepth}", QlErrorKind.Limit);

			children.Clear();

			var inner = row;

			switch (node)
			{
				case MethodCallExpression call:
				{
					var method = call.Method;

					if (!Allowed(method, adapter))
						throw Refuse("a call of " + method.Name);

					if (method.IsGenericMethod)
						foreach (var argument in method.GetGenericArguments())
							if (argument != entity.ClrType && !IsScalar(argument) && argument != typeof(object[]))
								throw Refuse("a type argument " + argument.Name);

					if (call.Object is not null)
						throw Refuse("an instance call");

					children.AddRange(call.Arguments);

					break;
				}

				case UnaryExpression unary:
				{
					switch (unary.NodeType)
					{
						case ExpressionType.Quote:
							if (unary.Operand is not LambdaExpression)
								throw Refuse("a quote of something not a lambda");
							break;

						case ExpressionType.Not:
						case ExpressionType.Negate:
							break;

						case ExpressionType.Convert:
							if (!IsScalar(unary.Operand.Type) || !(IsScalar(unary.Type) || unary.Type == typeof(object)))
								throw Refuse("a conversion to " + unary.Type.Name);

							if (unary.Method is not null && !Operators.Contains(unary.Method))
								throw Refuse("a conversion by " + unary.Method.Name);
							break;

						default:
							throw Refuse("a unary " + unary.NodeType);
					}

					if (unary.NodeType != ExpressionType.Quote && unary.Method is not null && !Operators.Contains(unary.Method))
						throw Refuse("an operator method " + unary.Method.Name);

					children.Add(unary.Operand);

					break;
				}

				case BinaryExpression binary:
				{
					var type = binary.NodeType;

					if (!Arithmetic.Contains(type) && !Comparisons.Contains(type) && type is not (ExpressionType.AndAlso or ExpressionType.OrElse))
						throw Refuse("a binary " + type);

					if (binary.Method is not null && !Operators.Contains(binary.Method))
						throw Refuse("an operator method " + binary.Method.Name);

					if (binary.Conversion is not null)
						throw Refuse("a coalescing conversion");

					children.Add(binary.Left);
					children.Add(binary.Right);

					break;
				}

				case LambdaExpression lambda:
				{
					if (row is not null || lambda.Parameters.Count != 1 || lambda.Parameters[0].Type != entity.ClrType)
						throw Refuse("a lambda that is not over a row");

					inner = lambda.Parameters[0];
					children.Add(lambda.Body);

					break;
				}

				case ParameterExpression parameter:
					if (parameter != row)
						throw Refuse("a parameter that is not the row");
					break;

				case MemberExpression member:
				{
					if (member.Expression is ConstantExpression { Value: { } holder } && QlHolder.IsHolder(holder.GetType()) && member.Member.Name == "Value")
					{
						// A value of the query: counted as the one node it is to linq2db.
						break;
					}

					if (member.Expression != row || row is null || !members.Contains(member.Member))
						throw Refuse("a read of " + member.Member.Name);

					break;
				}

				case ConstantExpression constant:
					if (!IsScalar(constant.Type))
						throw Refuse("a constant of type " + constant.Type.Name);
					break;

				case NewArrayExpression array:
					if (array.NodeType != ExpressionType.NewArrayInit || array.Type != typeof(object[]))
						throw Refuse("an array");

					children.AddRange(array.Expressions);

					break;

				default:
					throw Refuse("a node " + node.NodeType);
			}

			foreach (var child in children)
				stack.Push((child, depth + 1, inner));
		}

		CheckRowIndependent(tree, root);

		return new Stats(nodes, deepest);
	}

	/// <summary>
	/// Inside a lambda, a sub-tree that does not read the row is computed once, and linq2db may
	/// compute it on the host rather than in the database. Such a sub-tree may hold the query's
	/// values and the operators of the table, and nothing else: no call, no member read, nothing
	/// made — so nothing the host would run on the text's behalf but arithmetic on values.
	/// </summary>
	static void CheckRowIndependent(Expression tree, Expression root)
	{
		// Post-order without recursion: a node is visited after its children.
		var dependent = new Dictionary<Expression, bool>(ReferenceEqualityComparer.Instance);
		var stack     = new Stack<(Expression Node, bool Expanded, bool InLambda)>();

		stack.Push((tree, false, false));

		while (stack.Count > 0)
		{
			var (node, expanded, inLambda) = stack.Pop();

			if (ReferenceEquals(node, root))
				continue;

			if (!expanded)
			{
				stack.Push((node, true, inLambda));

				var inner = inLambda || node is LambdaExpression;

				foreach (var child in Children(node))
					stack.Push((child, false, inner));

				continue;
			}

			var reads = node is ParameterExpression;

			foreach (var child in Children(node))
				reads |= dependent.TryGetValue(child, out var d) && d;

			dependent[node] = reads;

			if (!inLambda || reads)
				continue;

			var allowed = node switch
			{
				MemberExpression { Expression: ConstantExpression { Value: { } holder } } => QlHolder.IsHolder(holder.GetType()),
				ConstantExpression                                                       => true,
				UnaryExpression { Method: null }                                         => true,
				BinaryExpression { Method: var method }                                  => method is null || Operators.Contains(method),
				UnaryExpression { Method: var method }                                   => Operators.Contains(method),
				_                                                                        => false,
			};

			if (!allowed)
				throw Refuse("a computation that does not read the row: " + node.NodeType);
		}
	}

	static IEnumerable<Expression> Children(Expression node)
	{
		switch (node)
		{
			case MethodCallExpression call:
				return call.Object is null ? call.Arguments : [call.Object, .. call.Arguments];

			case UnaryExpression unary:
				return [unary.Operand];

			case BinaryExpression binary:
				return [binary.Left, binary.Right];

			case LambdaExpression lambda:
				return [lambda.Body];

			case MemberExpression { Expression: { } target }:
				return [target];

			case NewArrayExpression array:
				return array.Expressions;

			default:
				return [];
		}
	}

	static bool Allowed(MethodInfo method, IQlAdapter adapter)
	{
		var definition = method.IsGenericMethod ? method.GetGenericMethodDefinition() : method;

		return Translator.Methods.Contains(definition) || Operators.Contains(method) || adapter.IsAllowed(method);
	}

	static bool IsScalar(Type type)
	{
		return Scalars.Contains(Nullable.GetUnderlyingType(type) ?? type);
	}

	static QlException Refuse(string what, QlErrorKind kind = QlErrorKind.Audit)
	{
		return new QlException(new QlError(kind, kind == QlErrorKind.Limit ? "The query is too large: " + what + "." : "The query was refused by its audit: " + what + "."));
	}
}
