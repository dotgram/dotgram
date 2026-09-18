namespace DotGram.Grammar;

/// <summary>Storage for typed values materialized by a direct tape reader.</summary>
public enum ValueStorageKind
{
	/// <summary>Choose at generation time using grammar structure; no runtime policy dispatch.</summary>
	Auto,

	/// <summary>One array per value type, indexed by record. Favors simple access.</summary>
	Flat,

	/// <summary>A flat prefix followed by lazily allocated pages per value type.</summary>
	Adaptive,
}
