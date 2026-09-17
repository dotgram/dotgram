using System;
using System.Runtime.CompilerServices;

using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Model;

/// <summary>Contextual answers owned by one graph whose FIRST estimates have settled.</summary>
sealed class ContinuationCache
{
	internal readonly record struct Key(Node Node, RuleSymbol? Seam, int Plain, int AfterSeam);

	sealed class Keys : IEqualityComparer<Key>
	{
		public bool Equals(Key x, Key y) => ReferenceEquals(x.Node, y.Node) &&
			ReferenceEquals(x.Seam, y.Seam) && x.Plain == y.Plain && x.AfterSeam == y.AfterSeam;

		public int GetHashCode(Key key)
		{
			unchecked
			{
				var hash = RuntimeHelpers.GetHashCode(key.Node);
				hash = hash * 31 + (key.Seam is null ? 0 : RuntimeHelpers.GetHashCode(key.Seam));
				return (hash * 31 + key.Plain) * 31 + key.AfterSeam;
			}
		}
	}

	sealed class FirstIdentity : IEqualityComparer<FirstSets.First>
	{
		public bool Equals(FirstSets.First? x, FirstSets.First? y) => ReferenceEquals(x, y);
		public int GetHashCode(FirstSets.First value) => RuntimeHelpers.GetHashCode(value);
	}

	sealed class FirstContents : IEqualityComparer<FirstSets.First>
	{
		public bool Equals(FirstSets.First? x, FirstSets.First? y) => ReferenceEquals(x, y) ||
			x is not null && y is not null && FirstSets.Same(x, y);

		public int GetHashCode(FirstSets.First value)
		{
			unchecked
			{
				var hash = (value.Anything ? 1 : 0) | (value.Nothing ? 2 : 0) | (value.Ends ? 4 : 0);
				foreach (var range in value.Ranges)
					hash = (hash * 31 + range.From) * 31 + range.To;
				return hash;
			}
		}
	}

	readonly Dictionary<FirstSets.First, int> _identities = new(new FirstIdentity());
	readonly Dictionary<FirstSets.First, int> _contents = new(new FirstContents());
	internal readonly Dictionary<Key, FollowSets.Continuation> Precedes = new(new Keys());
	internal readonly Dictionary<Key, bool> Possessive = new(new Keys());
	internal readonly Dictionary<Key, bool> NeverGivesBack = new(new Keys());

	int Identify(FirstSets.First first)
	{
		if (_identities.TryGetValue(first, out var id))
			return id;
		if (!_contents.TryGetValue(first, out id))
			_contents.Add(first, id = _contents.Count);
		_identities.Add(first, id);
		return id;
	}

	internal Key Of(Node node, FollowSets.Continuation following, RuleSymbol? seam) =>
		new(node, seam, Identify(following.Plain), Identify(following.AfterSeam));
}
