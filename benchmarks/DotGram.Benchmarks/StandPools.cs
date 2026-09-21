using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DotGram.Benchmarks;

static partial class Stand
{
	/// <summary>
	/// What the pools of one side hold on the calling thread, read from the state and not from the heap (performance-ff, 2026-09-21: the retained bytes after a parse do not say WHICH pool holds them, and the change that was
	/// written to release one left the figure of eighteen rows unmoved). Every generated parser keeps its rented stores in thread-static fields of nested types (<c>_spare</c>, <c>_deeper</c>, <c>_large</c>, <c>_largeLetGo</c>, ...): each
	/// such field of the given types and of every type nested in them is read on this thread, and what it holds is counted by CAPACITY, the length of every array field of the store times its element size. A slot that
	/// holds a store is named with the number of its arrays, their elements and their bytes and its largest array; a weak slot whose target is still alive is named with a tilde. The readings of a store that has been emptied
	/// are the room it keeps, which is what a pool is for. <c>ArrayPool&lt;T&gt;.Shared</c> is not read: its buckets are not reachable from here, and what it keeps is in the retained figure but not in this line.
	/// </summary>
	static string PoolReadout(Type[] roots, out long bytes)
	{
		var lines = new List<string>();
		var total = 0L;
		var seen  = new HashSet<Type>();

		void Visit(Type type)
		{
			if (!seen.Add(type) || type.ContainsGenericParameters)
				return;

			foreach (var field in type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
			{
				if (field.GetCustomAttribute<ThreadStaticAttribute>() is null)
					continue;

				var held = new PoolHeld();

				Measure(field.GetValue(null), held, weak: false);

				if (held.Arrays == 0)
					continue;

				total += held.Bytes;
				lines.Add($"{type.Name}.{field.Name}{(held.Weak ? "~" : "")}: {held.Stores:N0} {(held.Stores == 1 ? "store" : "stores")}, {held.Arrays:N0} arrays, {held.Elements:N0} elements, {held.Bytes / 1024.0:N0} KB (largest {held.LargestName} {held.LargestLength:N0})");
			}

			foreach (var nested in type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic))
				Visit(nested);
		}

		foreach (var root in roots)
			Visit(root);

		bytes = total;

		return lines.Count == 0 ? "no thread-static pool holds a store" : string.Join("; ", lines) + $"; together {total / 1024.0:N0} KB";
	}

	sealed class PoolHeld
	{
		public int    Stores;
		public int    Arrays;
		public long   Elements;
		public long   Bytes;
		public bool   Weak;
		public string LargestName = "";
		public long   LargestLength;
		public long   LargestBytes;
	}

	static readonly Dictionary<Type, int> ElementSizes = [];

	static int ElementSize(Type element)
	{
		if (!ElementSizes.TryGetValue(element, out var size))
			ElementSizes[element] = size = (int)typeof(Unsafe).GetMethod(nameof(Unsafe.SizeOf))!.MakeGenericMethod(element).Invoke(null, null)!;

		return size;
	}

	static void Measure(object? value, PoolHeld held, bool weak)
	{
		switch (value)
		{
			case null:
			case string:
				return;

			case Array array when array.GetType().GetElementType()!.IsValueType:
				held.Stores++;
				Add(held, array, "<slot>", weak);

				return;

			case Array array:
				// An array of stores (the deeper spares): each store in it.
				foreach (var one in array)
					Measure(one, held, weak);

				return;
		}

		var type = value.GetType();

		if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(WeakReference<>))
		{
			var arguments = new object?[] { null };

			if ((bool)type.GetMethod("TryGetTarget")!.Invoke(value, arguments)!)
				Measure(arguments[0], held, weak: true);

			return;
		}

		if (type.IsPrimitive || type.IsEnum)
			return;

		// A store: the capacity of every array it holds, in the fields of its class and of its base classes.
		var arraysBefore = held.Arrays;

		for (var current = type; current is not null && current != typeof(object); current = current.BaseType)
		{
			foreach (var field in current.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
			{
				if (field.FieldType.IsArray && field.FieldType.GetElementType()!.IsValueType && field.GetValue(value) is Array array)
					Add(held, array, field.Name, weak);
			}
		}

		if (held.Arrays > arraysBefore)
			held.Stores++;

		if (weak && held.Arrays > arraysBefore)
			held.Weak = true;
	}

	static void Add(PoolHeld held, Array array, string name, bool weak)
	{
		var bytes = (long)array.Length * ElementSize(array.GetType().GetElementType()!);

		held.Arrays++;
		held.Elements += array.Length;
		held.Bytes    += bytes;
		held.Weak     |= weak;

		if (bytes > held.LargestBytes)
		{
			held.LargestBytes  = bytes;
			held.LargestLength = array.Length;
			held.LargestName   = name;
		}
	}
}
