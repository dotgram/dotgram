using DotGram;

namespace DotGram.VisualStudio.Tests.Playground;

// Host for RecoveryPlayground.gram. It is a file of its own rather than another namespace in
// the standalone playground because `recover` turns on line and column tracking, which
// GRAM4026 refuses to combine with that file's buffered byte publication.
[Gram("RecoveryPlayground.gram")]
public static partial class RecoveryPlayground;
