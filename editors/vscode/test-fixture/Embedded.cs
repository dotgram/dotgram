// A grammar written into a C# string, with a rule it never defines: the server says so on this
// file, and paints the grammar's words over whatever colours the C# document has.
using DotGram;

namespace Smoke;

[Gram("""
	Start  = Word | Number & Missing
	Word   = ['a'..'z']+
	Number = ['0'..'9']+
	parse Start
	""")]
public partial class Embedded;
