using System;

namespace DotGram.Grammar.Emit;

public static partial class CSharpEmitter
{
	internal const string AdaptiveValuesSupport = """
		/// <summary>A flat prefix and lazily allocated pages, indexed by stable record numbers.</summary>
		struct ValueTable<T>
		{
			Held<T>[] _first;
			Held<T>[][]? _pages;
			int _capacity;

			internal ValueTable(int size)
			{
				_first = new Held<T>[size];
				_pages = null;
				_capacity = size;
			}

			internal int Length => _capacity;
			internal Held<T>[] First => _first;

			internal void Room(int count)
			{
				var size = global::System.Math.Min(count, 256);
				if (_first.Length >= size) return;
				size = global::System.Math.Min(256, global::System.Math.Max(size, _first.Length * 2));
				_capacity += size - _first.Length;
				global::System.Array.Resize(ref _first, size);
			}

			internal ref Held<T> this[int record]
			{
				[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
				get
				{
					if ((uint)record < (uint)_first.Length) return ref _first[record];
					return ref Page(record);
				}
			}

			ref Held<T> Page(int record)
			{
				var index = record >> 6;
				if (_pages == null)
				{
					_pages = new Held<T>[global::System.Math.Max(8, index + 1)][];
					_capacity += _pages.Length;
				}
				else if (index >= _pages.Length)
				{
					var size = global::System.Math.Max(index + 1, _pages.Length * 2);
					_capacity += size - _pages.Length;
					global::System.Array.Resize(ref _pages, size);
				}
				var page = _pages[index];
				if (page == null)
				{
					page = _pages[index] = new Held<T>[64];
					_capacity += page.Length;
				}
				return ref page[record & 63];
			}

			internal void Clear(int count)
			{
				global::System.Array.Clear(_first, 0, global::System.Math.Min(count, _first.Length));
				if (_pages == null) return;
				var end = global::System.Math.Min(_pages.Length, ((count - 1) >> 6) + 1);
				for (var index = 4; index < end; index++)
				{
					var page = _pages[index];
					if (page != null) global::System.Array.Clear(page, 0, page.Length);
				}
			}
		}
		""";
}
