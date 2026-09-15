using System;
using System.Runtime.InteropServices;

using DotGram.Web;

var document = JsonValue.Parse("{\"items\":[1,2]}");
var patch = JsonPatch.Parse("[{\"op\":\"replace\",\"path\":\"/items/1\",\"value\":42}]");
var changed = patch.Apply(document);
if (JsonPointer.Parse("/items/1").Resolve(changed) is not JsonValue.Number number ||
	!number.TryToInt64(out var value) || value != 42)
	throw new Exception("Expected the patched JSON number.");
if (JsonPointer.Parse("/items/1").Resolve(document)?.ToString() != "2")
	throw new Exception("The patch changed its input.");
if (!JsonValue.Parse(changed.ToString()).Equals(changed))
	throw new Exception("JSON did not survive a writer round trip.");
if (JsonValue.TryParse("{", out _))
	throw new Exception("Malformed JSON was accepted.");
Console.WriteLine($"DotGram.Web package smoke: JSON, Pointer and Patch parsed ({RuntimeInformation.FrameworkDescription}).");
