using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

public sealed class BufferedPoolTests
{
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Buffers_are_returned_on_completion_failure_and_disposal_without_owning_input(bool bytes)
	{
		var result = GramCompiler.Compile("""
			Item : @int = value: ['a'..'z']+ & ';' => @(value.Length)
			Feed : @int[] = Item*
			parse Feed as Whole
			parse Feed as Lazy yield : @int
			find Item as Search
			""", new GramCompilerOptions
		{
			BufferedInput = true, BufferedBytes = true, CSharpScanner = RoslynCSharpScanner.Instance,
		});
		EmittedCode.Quiet(result.Diagnostics);
		var source = Assert.Single(result.Sources).Text
			.Replace("global::System.Buffers.ArrayPool<char>.Shared", "ProbePool<char>.Shared")
			.Replace("global::System.Buffers.ArrayPool<byte>.Shared", "ProbePool<byte>.Shared");
		var assembly = EmittedCode.Compile(source + Pool);
		var pool = assembly.GetType("ProbePool`1")!.MakeGenericType(bytes ? typeof(byte) : typeof(char));
		var inputs = new List<object>();
		int Active() => (int)pool.GetProperty("Active")!.GetValue(null)!;
		object Call(string method, string input, int limit = 100, bool throws = false)
		{
			object reader = bytes ? new InputStream(Encoding.ASCII.GetBytes(input), throws) : new InputReader(input, throws);
			inputs.Add(reader);
			try
			{
				return assembly.GetType("Grammar")!.GetMethod(method, [bytes ? typeof(Stream) : typeof(TextReader), typeof(int), typeof(int)])!
					.Invoke(null, [reader, 2, limit])!;
			}
			finally
			{
				Assert.False(bytes ? ((InputStream)reader).Disposed : ((InputReader)reader).Disposed);
			}
		}

		Assert.Equal(new[] { 3, 7 }, (int[])Call("Whole", "abc;defghij;"));
		Assert.Equal(0, Active());
		// The pool deliberately returns oversized arrays. The logical limit still applies.
		Assert.Equal(new[] { 3 }, (int[])Call("Whole", "abc;", 4));
		Assert.Equal(0, Active());
		Assert.IsType<IOException>(Assert.Throws<TargetInvocationException>(() => Call("Whole", "abc;", 3)).InnerException);
		Assert.Equal(0, Active());
		Assert.IsType<FormatException>(Assert.Throws<TargetInvocationException>(() => Call("Whole", "abc;!")).InnerException);
		Assert.Equal(0, Active());
		Assert.IsType<IOException>(Assert.Throws<TargetInvocationException>(() => Call("Whole", "abc;", throws: true)).InnerException);
		Assert.Equal(0, Active());

		foreach (var method in new[] { "Lazy", "Search" })
		{
			var unused = ((IEnumerable)Call(method, "abc;")).GetEnumerator();
			Assert.Equal(0, Active());
			((IDisposable)unused).Dispose();
			Assert.Equal(0, Active());
			var first = ((IEnumerable)Call(method, "abc;defghij;")).GetEnumerator();
			var second = ((IEnumerable)Call(method, "xy;z;")).GetEnumerator();
			Assert.True(first.MoveNext());
			Assert.True(second.MoveNext());
			Assert.Equal(2, Active());
			((IDisposable)first).Dispose();
			((IDisposable)first).Dispose();
			Assert.Equal(1, Active());
			Assert.True(second.MoveNext());
			Assert.False(second.MoveNext());
			Assert.Equal(0, Active());
			((IDisposable)second).Dispose();
			Assert.Equal(0, Active());
			Assert.Equal(2, ((IEnumerable)Call(method, "abc;defghij;")).Cast<object>().Count());
			Assert.Equal(0, Active());
			var broken = ((IEnumerable)Call(method, "abc;", throws: true)).GetEnumerator();
			Assert.Throws<IOException>(() => broken.MoveNext());
			Assert.Equal(0, Active());
			((IDisposable)broken).Dispose();
		}
		var malformed = ((IEnumerable)Call("Lazy", "abc;!")).GetEnumerator();
		Assert.True(malformed.MoveNext());
		Assert.Throws<FormatException>(() => malformed.MoveNext());
		Assert.Equal(0, Active());
		((IDisposable)malformed).Dispose();
		foreach (var input in inputs)
		{
			Assert.False(bytes ? ((InputStream)input).Disposed : ((InputReader)input).Disposed);
			((IDisposable)input).Dispose();
		}
	}

	sealed class InputStream(byte[] input, bool throws) : MemoryStream(input)
	{
		public bool Disposed { get; private set; }
		public override int Read(byte[] buffer, int offset, int count) => throws ? throw new IOException() : base.Read(buffer, offset, count);
		protected override void Dispose(bool disposing) { Disposed = true; base.Dispose(disposing); }
	}

	sealed class InputReader(string input, bool throws) : StringReader(input)
	{
		public bool Disposed { get; private set; }
		public override int Read(char[] buffer, int offset, int count) => throws ? throw new IOException() : base.Read(buffer, offset, count);
		protected override void Dispose(bool disposing) { Disposed = true; base.Dispose(disposing); }
	}

	// An oversized, reusable bucket exposes logical-limit errors and double returns.
	const string Pool = """
		public sealed class ProbePool<T> : global::System.Buffers.ArrayPool<T>
		{
			public new static readonly ProbePool<T> Shared = new ProbePool<T>();
			readonly global::System.Collections.Generic.HashSet<T[]> leases = new global::System.Collections.Generic.HashSet<T[]>();
			readonly global::System.Collections.Generic.List<T[]> free = new global::System.Collections.Generic.List<T[]>();
			public static int Active => Shared.leases.Count;
			public override T[] Rent(int minimumLength)
			{
				T[] array = new T[minimumLength + 7];
				for (var i = 0; i < free.Count; i++)
					if (free[i].Length >= minimumLength) { array = free[i]; free.RemoveAt(i); break; }
				if (!leases.Add(array)) throw new global::System.Exception("Already rented.");
				return array;
			}
			public override void Return(T[] array, bool clearArray = false)
			{
				if (!leases.Remove(array)) throw new global::System.Exception("Returned twice.");
				if (!clearArray) throw new global::System.Exception("Input must be cleared.");
				global::System.Array.Clear(array, 0, array.Length);
				free.Add(array);
			}
		}
		""";
}
