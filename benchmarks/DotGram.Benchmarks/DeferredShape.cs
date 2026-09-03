using System;

using BenchmarkDotNet.Attributes;

namespace DotGram.Benchmarks;

/// <summary>
/// How a construction deferred until the accepted derivation is known is best carried
/// there.
/// </summary>
/// <remarks>
/// <para>
/// The language defers a <c>=&gt;</c> until recognition has selected the accepted
/// derivation (docs/syntax.md §7.3), which is what lets an author write a factory that is
/// not safe for speculative invocation. Deferring is not in question; how it is carried
/// is. Today it is a log — a record per construction holding the arm and where its
/// arguments are — and a walk at the end that reads the arm, switches on it, reads the
/// arguments back and calls the factory. That walk is thirty percent of a generated SQL
/// parse and has no counterpart in the hand-written one (docs/next.md).
/// </para>
/// <para>
/// Four ways to carry it, over one tree: a hundred and twenty-eight leaves cut from the
/// input, sixty-four pairs over them and a spine of sixty-three joins, which is the shape
/// of <c>a0 = 1 AND a1 = 1 AND …</c>. Each way differs from the first in one thing, so
/// that what the numbers say is about that thing.
/// </para>
/// <list type="bullet">
/// <item><description><b>switched</b> — the log as it is now: values indexed by where the
/// record sits in the log, and a switch over every arm the grammar has. Four of five
/// hundred and twelve ever run, so the jump table is as sparse as a real one.</description></item>
/// <item><description><b>called</b> — the same, with the switch replaced by a table of
/// delegates the generator wrote, built once at class initialization. Nothing is allocated
/// per parse; the dispatch becomes an indirect call.</description></item>
/// <item><description><b>dense</b> — the same switch, with values indexed by which record
/// they are rather than by where it sits. The tables are then as long as the number of
/// records instead of the log's length in words: four times shorter here, and four times
/// less to clear.</description></item>
/// <item><description><b>closures</b> — no log: recognition builds a tree of closures over
/// what it read and the end invokes the root. One allocation a node beyond the tree
/// itself, which is the thing to price.</description></item>
/// </list>
/// </remarks>
[MemoryDiagnoser(false)]
public class DeferredShape
{
	public abstract class Made;

	public sealed class Leaf(string text) : Made
	{
		public readonly string Text = text;
	}

	public sealed class Pair(int arm, Made left, Made right) : Made
	{
		public readonly int  Arm   = arm;
		public readonly Made Left  = left;
		public readonly Made Right = right;
	}

	// What the grammar's own C# would be. The same in every shape: what is compared is how
	// it is reached, not what it costs once reached.
	static Made MakeLeaf(string text, int from, int to) => new Leaf(text.Substring(from, to - from));
	static Made MakePair(Made left, Made right)         => new Pair(1, left, right);
	static Made MakeJoin(Made left, Made right)         => new Pair(2, left, right);

	string _text = null!;
	int[]  _at   = null!;

	[GlobalSetup]
	public void Setup()
	{
		var text = new System.Text.StringBuilder();
		var at   = new int[128 * 2];

		for (var i = 0; i < 128; i++)
		{
			at[i * 2] = text.Length;
			text.Append(i % 2 == 0 ? "a" + i : (i * 7 % 100).ToString());
			at[i * 2 + 1] = text.Length;
		}

		_text = text.ToString();
		_at   = at;
	}

	/// <summary>Where an arm that never runs puts its number, so that each case is its own.</summary>
	public static int Sink;

	int[]  _log     = new int[(128 + 64 + 63) * 4];
	int[]  _leaves  = new int[128];
	int[]  _pairs   = new int[64];
	Made[] _byPlace = new Made[64];
	Made[] _byOrder = new Made[64];
	Made[] _byCall  = new Made[64];
	Made[] _made    = new Made[128];

	/// <summary>
	/// What recognition writes: a record per construction, children before parents, and a
	/// child named by where its record sits in the log.
	/// </summary>
	int RecordByPlace(out int root, out int records)
	{
		var log    = _log;
		var at     = _at;
		var leaves = _leaves;
		var pairs  = _pairs;
		var count  = 0;
		var ord    = 0;

		for (var i = 0; i < 128; i++)
		{
			leaves[i] = count;
			log[count] = 4; log[count + 1] = 0; log[count + 2] = at[i * 2]; log[count + 3] = at[i * 2 + 1];
			count += 4; ord++;
		}

		for (var i = 0; i < 64; i++)
		{
			pairs[i] = count;
			log[count] = 4; log[count + 1] = 1; log[count + 2] = leaves[i * 2]; log[count + 3] = leaves[i * 2 + 1];
			count += 4; ord++;
		}

		var spine = pairs[0];

		for (var i = 0; i < 63; i++)
		{
			log[count] = 4; log[count + 1] = 2; log[count + 2] = spine; log[count + 3] = pairs[i + 1];
			spine = count;
			count += 4; ord++;
		}

		root    = spine;
		records = ord;

		return count;
	}

	/// <summary>
	/// What recognition writes: a record per construction, children before parents, and a
	/// child named by which record it is.
	/// </summary>
	int RecordByOrder(out int root, out int records)
	{
		var log    = _log;
		var at     = _at;
		var leaves = _leaves;
		var pairs  = _pairs;
		var count  = 0;
		var ord    = 0;

		for (var i = 0; i < 128; i++)
		{
			leaves[i] = ord;
			log[count] = 4; log[count + 1] = 0; log[count + 2] = at[i * 2]; log[count + 3] = at[i * 2 + 1];
			count += 4; ord++;
		}

		for (var i = 0; i < 64; i++)
		{
			pairs[i] = ord;
			log[count] = 4; log[count + 1] = 1; log[count + 2] = leaves[i * 2]; log[count + 3] = leaves[i * 2 + 1];
			count += 4; ord++;
		}

		var spine = pairs[0];

		for (var i = 0; i < 63; i++)
		{
			log[count] = 4; log[count + 1] = 2; log[count + 2] = spine; log[count + 3] = pairs[i + 1];
			spine = ord;
			count += 4; ord++;
		}

		root    = spine;
		records = ord;

		return count;
	}

	[Benchmark(Baseline = true)]
	public Made switched()
	{
		var count = RecordByPlace(out var root, out _);
		var log   = _log;
		var text  = _text;

		var values = _byPlace;

		if (values.Length < count)
			values = _byPlace = new Made[count * 2];

		var ord = 0;

		for (var at = 0; at < count; at += log[at], ord++)
		{
			switch (log[at + 1])
			{
				case 0:
					values[at] = MakeLeaf(text, log[at + 2], log[at + 3]);
					break;

				case 1:
					values[at] = MakePair(values[log[at + 2]], values[log[at + 3]]);
					break;

				case 2:
					values[at] = MakeJoin(values[log[at + 2]], values[log[at + 3]]);
					break;

				case 3: Sink = 3; break;
				case 4: Sink = 4; break;
				case 5: Sink = 5; break;
				case 6: Sink = 6; break;
				case 7: Sink = 7; break;
				case 8: Sink = 8; break;
				case 9: Sink = 9; break;
				case 10: Sink = 10; break;
				case 11: Sink = 11; break;
				case 12: Sink = 12; break;
				case 13: Sink = 13; break;
				case 14: Sink = 14; break;
				case 15: Sink = 15; break;
				case 16: Sink = 16; break;
				case 17: Sink = 17; break;
				case 18: Sink = 18; break;
				case 19: Sink = 19; break;
				case 20: Sink = 20; break;
				case 21: Sink = 21; break;
				case 22: Sink = 22; break;
				case 23: Sink = 23; break;
				case 24: Sink = 24; break;
				case 25: Sink = 25; break;
				case 26: Sink = 26; break;
				case 27: Sink = 27; break;
				case 28: Sink = 28; break;
				case 29: Sink = 29; break;
				case 30: Sink = 30; break;
				case 31: Sink = 31; break;
				case 32: Sink = 32; break;
				case 33: Sink = 33; break;
				case 34: Sink = 34; break;
				case 35: Sink = 35; break;
				case 36: Sink = 36; break;
				case 37: Sink = 37; break;
				case 38: Sink = 38; break;
				case 39: Sink = 39; break;
				case 40: Sink = 40; break;
				case 41: Sink = 41; break;
				case 42: Sink = 42; break;
				case 43: Sink = 43; break;
				case 44: Sink = 44; break;
				case 45: Sink = 45; break;
				case 46: Sink = 46; break;
				case 47: Sink = 47; break;
				case 48: Sink = 48; break;
				case 49: Sink = 49; break;
				case 50: Sink = 50; break;
				case 51: Sink = 51; break;
				case 52: Sink = 52; break;
				case 53: Sink = 53; break;
				case 54: Sink = 54; break;
				case 55: Sink = 55; break;
				case 56: Sink = 56; break;
				case 57: Sink = 57; break;
				case 58: Sink = 58; break;
				case 59: Sink = 59; break;
				case 60: Sink = 60; break;
				case 61: Sink = 61; break;
				case 62: Sink = 62; break;
				case 63: Sink = 63; break;
				case 64: Sink = 64; break;
				case 65: Sink = 65; break;
				case 66: Sink = 66; break;
				case 67: Sink = 67; break;
				case 68: Sink = 68; break;
				case 69: Sink = 69; break;
				case 70: Sink = 70; break;
				case 71: Sink = 71; break;
				case 72: Sink = 72; break;
				case 73: Sink = 73; break;
				case 74: Sink = 74; break;
				case 75: Sink = 75; break;
				case 76: Sink = 76; break;
				case 77: Sink = 77; break;
				case 78: Sink = 78; break;
				case 79: Sink = 79; break;
				case 80: Sink = 80; break;
				case 81: Sink = 81; break;
				case 82: Sink = 82; break;
				case 83: Sink = 83; break;
				case 84: Sink = 84; break;
				case 85: Sink = 85; break;
				case 86: Sink = 86; break;
				case 87: Sink = 87; break;
				case 88: Sink = 88; break;
				case 89: Sink = 89; break;
				case 90: Sink = 90; break;
				case 91: Sink = 91; break;
				case 92: Sink = 92; break;
				case 93: Sink = 93; break;
				case 94: Sink = 94; break;
				case 95: Sink = 95; break;
				case 96: Sink = 96; break;
				case 97: Sink = 97; break;
				case 98: Sink = 98; break;
				case 99: Sink = 99; break;
				case 100: Sink = 100; break;
				case 101: Sink = 101; break;
				case 102: Sink = 102; break;
				case 103: Sink = 103; break;
				case 104: Sink = 104; break;
				case 105: Sink = 105; break;
				case 106: Sink = 106; break;
				case 107: Sink = 107; break;
				case 108: Sink = 108; break;
				case 109: Sink = 109; break;
				case 110: Sink = 110; break;
				case 111: Sink = 111; break;
				case 112: Sink = 112; break;
				case 113: Sink = 113; break;
				case 114: Sink = 114; break;
				case 115: Sink = 115; break;
				case 116: Sink = 116; break;
				case 117: Sink = 117; break;
				case 118: Sink = 118; break;
				case 119: Sink = 119; break;
				case 120: Sink = 120; break;
				case 121: Sink = 121; break;
				case 122: Sink = 122; break;
				case 123: Sink = 123; break;
				case 124: Sink = 124; break;
				case 125: Sink = 125; break;
				case 126: Sink = 126; break;
				case 127: Sink = 127; break;
				case 128: Sink = 128; break;
				case 129: Sink = 129; break;
				case 130: Sink = 130; break;
				case 131: Sink = 131; break;
				case 132: Sink = 132; break;
				case 133: Sink = 133; break;
				case 134: Sink = 134; break;
				case 135: Sink = 135; break;
				case 136: Sink = 136; break;
				case 137: Sink = 137; break;
				case 138: Sink = 138; break;
				case 139: Sink = 139; break;
				case 140: Sink = 140; break;
				case 141: Sink = 141; break;
				case 142: Sink = 142; break;
				case 143: Sink = 143; break;
				case 144: Sink = 144; break;
				case 145: Sink = 145; break;
				case 146: Sink = 146; break;
				case 147: Sink = 147; break;
				case 148: Sink = 148; break;
				case 149: Sink = 149; break;
				case 150: Sink = 150; break;
				case 151: Sink = 151; break;
				case 152: Sink = 152; break;
				case 153: Sink = 153; break;
				case 154: Sink = 154; break;
				case 155: Sink = 155; break;
				case 156: Sink = 156; break;
				case 157: Sink = 157; break;
				case 158: Sink = 158; break;
				case 159: Sink = 159; break;
				case 160: Sink = 160; break;
				case 161: Sink = 161; break;
				case 162: Sink = 162; break;
				case 163: Sink = 163; break;
				case 164: Sink = 164; break;
				case 165: Sink = 165; break;
				case 166: Sink = 166; break;
				case 167: Sink = 167; break;
				case 168: Sink = 168; break;
				case 169: Sink = 169; break;
				case 170: Sink = 170; break;
				case 171: Sink = 171; break;
				case 172: Sink = 172; break;
				case 173: Sink = 173; break;
				case 174: Sink = 174; break;
				case 175: Sink = 175; break;
				case 176: Sink = 176; break;
				case 177: Sink = 177; break;
				case 178: Sink = 178; break;
				case 179: Sink = 179; break;
				case 180: Sink = 180; break;
				case 181: Sink = 181; break;
				case 182: Sink = 182; break;
				case 183: Sink = 183; break;
				case 184: Sink = 184; break;
				case 185: Sink = 185; break;
				case 186: Sink = 186; break;
				case 187: Sink = 187; break;
				case 188: Sink = 188; break;
				case 189: Sink = 189; break;
				case 190: Sink = 190; break;
				case 191: Sink = 191; break;
				case 192: Sink = 192; break;
				case 193: Sink = 193; break;
				case 194: Sink = 194; break;
				case 195: Sink = 195; break;
				case 196: Sink = 196; break;
				case 197: Sink = 197; break;
				case 198: Sink = 198; break;
				case 199: Sink = 199; break;
				case 200: Sink = 200; break;
				case 201: Sink = 201; break;
				case 202: Sink = 202; break;
				case 203: Sink = 203; break;
				case 204: Sink = 204; break;
				case 205: Sink = 205; break;
				case 206: Sink = 206; break;
				case 207: Sink = 207; break;
				case 208: Sink = 208; break;
				case 209: Sink = 209; break;
				case 210: Sink = 210; break;
				case 211: Sink = 211; break;
				case 212: Sink = 212; break;
				case 213: Sink = 213; break;
				case 214: Sink = 214; break;
				case 215: Sink = 215; break;
				case 216: Sink = 216; break;
				case 217: Sink = 217; break;
				case 218: Sink = 218; break;
				case 219: Sink = 219; break;
				case 220: Sink = 220; break;
				case 221: Sink = 221; break;
				case 222: Sink = 222; break;
				case 223: Sink = 223; break;
				case 224: Sink = 224; break;
				case 225: Sink = 225; break;
				case 226: Sink = 226; break;
				case 227: Sink = 227; break;
				case 228: Sink = 228; break;
				case 229: Sink = 229; break;
				case 230: Sink = 230; break;
				case 231: Sink = 231; break;
				case 232: Sink = 232; break;
				case 233: Sink = 233; break;
				case 234: Sink = 234; break;
				case 235: Sink = 235; break;
				case 236: Sink = 236; break;
				case 237: Sink = 237; break;
				case 238: Sink = 238; break;
				case 239: Sink = 239; break;
				case 240: Sink = 240; break;
				case 241: Sink = 241; break;
				case 242: Sink = 242; break;
				case 243: Sink = 243; break;
				case 244: Sink = 244; break;
				case 245: Sink = 245; break;
				case 246: Sink = 246; break;
				case 247: Sink = 247; break;
				case 248: Sink = 248; break;
				case 249: Sink = 249; break;
				case 250: Sink = 250; break;
				case 251: Sink = 251; break;
				case 252: Sink = 252; break;
				case 253: Sink = 253; break;
				case 254: Sink = 254; break;
				case 255: Sink = 255; break;
				case 256: Sink = 256; break;
				case 257: Sink = 257; break;
				case 258: Sink = 258; break;
				case 259: Sink = 259; break;
				case 260: Sink = 260; break;
				case 261: Sink = 261; break;
				case 262: Sink = 262; break;
				case 263: Sink = 263; break;
				case 264: Sink = 264; break;
				case 265: Sink = 265; break;
				case 266: Sink = 266; break;
				case 267: Sink = 267; break;
				case 268: Sink = 268; break;
				case 269: Sink = 269; break;
				case 270: Sink = 270; break;
				case 271: Sink = 271; break;
				case 272: Sink = 272; break;
				case 273: Sink = 273; break;
				case 274: Sink = 274; break;
				case 275: Sink = 275; break;
				case 276: Sink = 276; break;
				case 277: Sink = 277; break;
				case 278: Sink = 278; break;
				case 279: Sink = 279; break;
				case 280: Sink = 280; break;
				case 281: Sink = 281; break;
				case 282: Sink = 282; break;
				case 283: Sink = 283; break;
				case 284: Sink = 284; break;
				case 285: Sink = 285; break;
				case 286: Sink = 286; break;
				case 287: Sink = 287; break;
				case 288: Sink = 288; break;
				case 289: Sink = 289; break;
				case 290: Sink = 290; break;
				case 291: Sink = 291; break;
				case 292: Sink = 292; break;
				case 293: Sink = 293; break;
				case 294: Sink = 294; break;
				case 295: Sink = 295; break;
				case 296: Sink = 296; break;
				case 297: Sink = 297; break;
				case 298: Sink = 298; break;
				case 299: Sink = 299; break;
				case 300: Sink = 300; break;
				case 301: Sink = 301; break;
				case 302: Sink = 302; break;
				case 303: Sink = 303; break;
				case 304: Sink = 304; break;
				case 305: Sink = 305; break;
				case 306: Sink = 306; break;
				case 307: Sink = 307; break;
				case 308: Sink = 308; break;
				case 309: Sink = 309; break;
				case 310: Sink = 310; break;
				case 311: Sink = 311; break;
				case 312: Sink = 312; break;
				case 313: Sink = 313; break;
				case 314: Sink = 314; break;
				case 315: Sink = 315; break;
				case 316: Sink = 316; break;
				case 317: Sink = 317; break;
				case 318: Sink = 318; break;
				case 319: Sink = 319; break;
				case 320: Sink = 320; break;
				case 321: Sink = 321; break;
				case 322: Sink = 322; break;
				case 323: Sink = 323; break;
				case 324: Sink = 324; break;
				case 325: Sink = 325; break;
				case 326: Sink = 326; break;
				case 327: Sink = 327; break;
				case 328: Sink = 328; break;
				case 329: Sink = 329; break;
				case 330: Sink = 330; break;
				case 331: Sink = 331; break;
				case 332: Sink = 332; break;
				case 333: Sink = 333; break;
				case 334: Sink = 334; break;
				case 335: Sink = 335; break;
				case 336: Sink = 336; break;
				case 337: Sink = 337; break;
				case 338: Sink = 338; break;
				case 339: Sink = 339; break;
				case 340: Sink = 340; break;
				case 341: Sink = 341; break;
				case 342: Sink = 342; break;
				case 343: Sink = 343; break;
				case 344: Sink = 344; break;
				case 345: Sink = 345; break;
				case 346: Sink = 346; break;
				case 347: Sink = 347; break;
				case 348: Sink = 348; break;
				case 349: Sink = 349; break;
				case 350: Sink = 350; break;
				case 351: Sink = 351; break;
				case 352: Sink = 352; break;
				case 353: Sink = 353; break;
				case 354: Sink = 354; break;
				case 355: Sink = 355; break;
				case 356: Sink = 356; break;
				case 357: Sink = 357; break;
				case 358: Sink = 358; break;
				case 359: Sink = 359; break;
				case 360: Sink = 360; break;
				case 361: Sink = 361; break;
				case 362: Sink = 362; break;
				case 363: Sink = 363; break;
				case 364: Sink = 364; break;
				case 365: Sink = 365; break;
				case 366: Sink = 366; break;
				case 367: Sink = 367; break;
				case 368: Sink = 368; break;
				case 369: Sink = 369; break;
				case 370: Sink = 370; break;
				case 371: Sink = 371; break;
				case 372: Sink = 372; break;
				case 373: Sink = 373; break;
				case 374: Sink = 374; break;
				case 375: Sink = 375; break;
				case 376: Sink = 376; break;
				case 377: Sink = 377; break;
				case 378: Sink = 378; break;
				case 379: Sink = 379; break;
				case 380: Sink = 380; break;
				case 381: Sink = 381; break;
				case 382: Sink = 382; break;
				case 383: Sink = 383; break;
				case 384: Sink = 384; break;
				case 385: Sink = 385; break;
				case 386: Sink = 386; break;
				case 387: Sink = 387; break;
				case 388: Sink = 388; break;
				case 389: Sink = 389; break;
				case 390: Sink = 390; break;
				case 391: Sink = 391; break;
				case 392: Sink = 392; break;
				case 393: Sink = 393; break;
				case 394: Sink = 394; break;
				case 395: Sink = 395; break;
				case 396: Sink = 396; break;
				case 397: Sink = 397; break;
				case 398: Sink = 398; break;
				case 399: Sink = 399; break;
				case 400: Sink = 400; break;
				case 401: Sink = 401; break;
				case 402: Sink = 402; break;
				case 403: Sink = 403; break;
				case 404: Sink = 404; break;
				case 405: Sink = 405; break;
				case 406: Sink = 406; break;
				case 407: Sink = 407; break;
				case 408: Sink = 408; break;
				case 409: Sink = 409; break;
				case 410: Sink = 410; break;
				case 411: Sink = 411; break;
				case 412: Sink = 412; break;
				case 413: Sink = 413; break;
				case 414: Sink = 414; break;
				case 415: Sink = 415; break;
				case 416: Sink = 416; break;
				case 417: Sink = 417; break;
				case 418: Sink = 418; break;
				case 419: Sink = 419; break;
				case 420: Sink = 420; break;
				case 421: Sink = 421; break;
				case 422: Sink = 422; break;
				case 423: Sink = 423; break;
				case 424: Sink = 424; break;
				case 425: Sink = 425; break;
				case 426: Sink = 426; break;
				case 427: Sink = 427; break;
				case 428: Sink = 428; break;
				case 429: Sink = 429; break;
				case 430: Sink = 430; break;
				case 431: Sink = 431; break;
				case 432: Sink = 432; break;
				case 433: Sink = 433; break;
				case 434: Sink = 434; break;
				case 435: Sink = 435; break;
				case 436: Sink = 436; break;
				case 437: Sink = 437; break;
				case 438: Sink = 438; break;
				case 439: Sink = 439; break;
				case 440: Sink = 440; break;
				case 441: Sink = 441; break;
				case 442: Sink = 442; break;
				case 443: Sink = 443; break;
				case 444: Sink = 444; break;
				case 445: Sink = 445; break;
				case 446: Sink = 446; break;
				case 447: Sink = 447; break;
				case 448: Sink = 448; break;
				case 449: Sink = 449; break;
				case 450: Sink = 450; break;
				case 451: Sink = 451; break;
				case 452: Sink = 452; break;
				case 453: Sink = 453; break;
				case 454: Sink = 454; break;
				case 455: Sink = 455; break;
				case 456: Sink = 456; break;
				case 457: Sink = 457; break;
				case 458: Sink = 458; break;
				case 459: Sink = 459; break;
				case 460: Sink = 460; break;
				case 461: Sink = 461; break;
				case 462: Sink = 462; break;
				case 463: Sink = 463; break;
				case 464: Sink = 464; break;
				case 465: Sink = 465; break;
				case 466: Sink = 466; break;
				case 467: Sink = 467; break;
				case 468: Sink = 468; break;
				case 469: Sink = 469; break;
				case 470: Sink = 470; break;
				case 471: Sink = 471; break;
				case 472: Sink = 472; break;
				case 473: Sink = 473; break;
				case 474: Sink = 474; break;
				case 475: Sink = 475; break;
				case 476: Sink = 476; break;
				case 477: Sink = 477; break;
				case 478: Sink = 478; break;
				case 479: Sink = 479; break;
				case 480: Sink = 480; break;
				case 481: Sink = 481; break;
				case 482: Sink = 482; break;
				case 483: Sink = 483; break;
				case 484: Sink = 484; break;
				case 485: Sink = 485; break;
				case 486: Sink = 486; break;
				case 487: Sink = 487; break;
				case 488: Sink = 488; break;
				case 489: Sink = 489; break;
				case 490: Sink = 490; break;
				case 491: Sink = 491; break;
				case 492: Sink = 492; break;
				case 493: Sink = 493; break;
				case 494: Sink = 494; break;
				case 495: Sink = 495; break;
				case 496: Sink = 496; break;
				case 497: Sink = 497; break;
				case 498: Sink = 498; break;
				case 499: Sink = 499; break;
				case 500: Sink = 500; break;
				case 501: Sink = 501; break;
				case 502: Sink = 502; break;
				case 503: Sink = 503; break;
				case 504: Sink = 504; break;
				case 505: Sink = 505; break;
				case 506: Sink = 506; break;
				case 507: Sink = 507; break;
				case 508: Sink = 508; break;
				case 509: Sink = 509; break;
				case 510: Sink = 510; break;
				case 511: Sink = 511; break;
			}
		}

		var made = values[root];

		global::System.Array.Clear(values, 0, count);

		return made;
	}

	[Benchmark]
	public Made dense()
	{
		var count = RecordByOrder(out var root, out var records);
		var log   = _log;
		var text  = _text;

		var values = _byOrder;

		if (values.Length < records)
			values = _byOrder = new Made[records * 2];

		var ord = 0;

		for (var at = 0; at < count; at += log[at], ord++)
		{
			switch (log[at + 1])
			{
				case 0:
					values[ord] = MakeLeaf(text, log[at + 2], log[at + 3]);
					break;

				case 1:
					values[ord] = MakePair(values[log[at + 2]], values[log[at + 3]]);
					break;

				case 2:
					values[ord] = MakeJoin(values[log[at + 2]], values[log[at + 3]]);
					break;

				case 3: Sink = 3; break;
				case 4: Sink = 4; break;
				case 5: Sink = 5; break;
				case 6: Sink = 6; break;
				case 7: Sink = 7; break;
				case 8: Sink = 8; break;
				case 9: Sink = 9; break;
				case 10: Sink = 10; break;
				case 11: Sink = 11; break;
				case 12: Sink = 12; break;
				case 13: Sink = 13; break;
				case 14: Sink = 14; break;
				case 15: Sink = 15; break;
				case 16: Sink = 16; break;
				case 17: Sink = 17; break;
				case 18: Sink = 18; break;
				case 19: Sink = 19; break;
				case 20: Sink = 20; break;
				case 21: Sink = 21; break;
				case 22: Sink = 22; break;
				case 23: Sink = 23; break;
				case 24: Sink = 24; break;
				case 25: Sink = 25; break;
				case 26: Sink = 26; break;
				case 27: Sink = 27; break;
				case 28: Sink = 28; break;
				case 29: Sink = 29; break;
				case 30: Sink = 30; break;
				case 31: Sink = 31; break;
				case 32: Sink = 32; break;
				case 33: Sink = 33; break;
				case 34: Sink = 34; break;
				case 35: Sink = 35; break;
				case 36: Sink = 36; break;
				case 37: Sink = 37; break;
				case 38: Sink = 38; break;
				case 39: Sink = 39; break;
				case 40: Sink = 40; break;
				case 41: Sink = 41; break;
				case 42: Sink = 42; break;
				case 43: Sink = 43; break;
				case 44: Sink = 44; break;
				case 45: Sink = 45; break;
				case 46: Sink = 46; break;
				case 47: Sink = 47; break;
				case 48: Sink = 48; break;
				case 49: Sink = 49; break;
				case 50: Sink = 50; break;
				case 51: Sink = 51; break;
				case 52: Sink = 52; break;
				case 53: Sink = 53; break;
				case 54: Sink = 54; break;
				case 55: Sink = 55; break;
				case 56: Sink = 56; break;
				case 57: Sink = 57; break;
				case 58: Sink = 58; break;
				case 59: Sink = 59; break;
				case 60: Sink = 60; break;
				case 61: Sink = 61; break;
				case 62: Sink = 62; break;
				case 63: Sink = 63; break;
				case 64: Sink = 64; break;
				case 65: Sink = 65; break;
				case 66: Sink = 66; break;
				case 67: Sink = 67; break;
				case 68: Sink = 68; break;
				case 69: Sink = 69; break;
				case 70: Sink = 70; break;
				case 71: Sink = 71; break;
				case 72: Sink = 72; break;
				case 73: Sink = 73; break;
				case 74: Sink = 74; break;
				case 75: Sink = 75; break;
				case 76: Sink = 76; break;
				case 77: Sink = 77; break;
				case 78: Sink = 78; break;
				case 79: Sink = 79; break;
				case 80: Sink = 80; break;
				case 81: Sink = 81; break;
				case 82: Sink = 82; break;
				case 83: Sink = 83; break;
				case 84: Sink = 84; break;
				case 85: Sink = 85; break;
				case 86: Sink = 86; break;
				case 87: Sink = 87; break;
				case 88: Sink = 88; break;
				case 89: Sink = 89; break;
				case 90: Sink = 90; break;
				case 91: Sink = 91; break;
				case 92: Sink = 92; break;
				case 93: Sink = 93; break;
				case 94: Sink = 94; break;
				case 95: Sink = 95; break;
				case 96: Sink = 96; break;
				case 97: Sink = 97; break;
				case 98: Sink = 98; break;
				case 99: Sink = 99; break;
				case 100: Sink = 100; break;
				case 101: Sink = 101; break;
				case 102: Sink = 102; break;
				case 103: Sink = 103; break;
				case 104: Sink = 104; break;
				case 105: Sink = 105; break;
				case 106: Sink = 106; break;
				case 107: Sink = 107; break;
				case 108: Sink = 108; break;
				case 109: Sink = 109; break;
				case 110: Sink = 110; break;
				case 111: Sink = 111; break;
				case 112: Sink = 112; break;
				case 113: Sink = 113; break;
				case 114: Sink = 114; break;
				case 115: Sink = 115; break;
				case 116: Sink = 116; break;
				case 117: Sink = 117; break;
				case 118: Sink = 118; break;
				case 119: Sink = 119; break;
				case 120: Sink = 120; break;
				case 121: Sink = 121; break;
				case 122: Sink = 122; break;
				case 123: Sink = 123; break;
				case 124: Sink = 124; break;
				case 125: Sink = 125; break;
				case 126: Sink = 126; break;
				case 127: Sink = 127; break;
				case 128: Sink = 128; break;
				case 129: Sink = 129; break;
				case 130: Sink = 130; break;
				case 131: Sink = 131; break;
				case 132: Sink = 132; break;
				case 133: Sink = 133; break;
				case 134: Sink = 134; break;
				case 135: Sink = 135; break;
				case 136: Sink = 136; break;
				case 137: Sink = 137; break;
				case 138: Sink = 138; break;
				case 139: Sink = 139; break;
				case 140: Sink = 140; break;
				case 141: Sink = 141; break;
				case 142: Sink = 142; break;
				case 143: Sink = 143; break;
				case 144: Sink = 144; break;
				case 145: Sink = 145; break;
				case 146: Sink = 146; break;
				case 147: Sink = 147; break;
				case 148: Sink = 148; break;
				case 149: Sink = 149; break;
				case 150: Sink = 150; break;
				case 151: Sink = 151; break;
				case 152: Sink = 152; break;
				case 153: Sink = 153; break;
				case 154: Sink = 154; break;
				case 155: Sink = 155; break;
				case 156: Sink = 156; break;
				case 157: Sink = 157; break;
				case 158: Sink = 158; break;
				case 159: Sink = 159; break;
				case 160: Sink = 160; break;
				case 161: Sink = 161; break;
				case 162: Sink = 162; break;
				case 163: Sink = 163; break;
				case 164: Sink = 164; break;
				case 165: Sink = 165; break;
				case 166: Sink = 166; break;
				case 167: Sink = 167; break;
				case 168: Sink = 168; break;
				case 169: Sink = 169; break;
				case 170: Sink = 170; break;
				case 171: Sink = 171; break;
				case 172: Sink = 172; break;
				case 173: Sink = 173; break;
				case 174: Sink = 174; break;
				case 175: Sink = 175; break;
				case 176: Sink = 176; break;
				case 177: Sink = 177; break;
				case 178: Sink = 178; break;
				case 179: Sink = 179; break;
				case 180: Sink = 180; break;
				case 181: Sink = 181; break;
				case 182: Sink = 182; break;
				case 183: Sink = 183; break;
				case 184: Sink = 184; break;
				case 185: Sink = 185; break;
				case 186: Sink = 186; break;
				case 187: Sink = 187; break;
				case 188: Sink = 188; break;
				case 189: Sink = 189; break;
				case 190: Sink = 190; break;
				case 191: Sink = 191; break;
				case 192: Sink = 192; break;
				case 193: Sink = 193; break;
				case 194: Sink = 194; break;
				case 195: Sink = 195; break;
				case 196: Sink = 196; break;
				case 197: Sink = 197; break;
				case 198: Sink = 198; break;
				case 199: Sink = 199; break;
				case 200: Sink = 200; break;
				case 201: Sink = 201; break;
				case 202: Sink = 202; break;
				case 203: Sink = 203; break;
				case 204: Sink = 204; break;
				case 205: Sink = 205; break;
				case 206: Sink = 206; break;
				case 207: Sink = 207; break;
				case 208: Sink = 208; break;
				case 209: Sink = 209; break;
				case 210: Sink = 210; break;
				case 211: Sink = 211; break;
				case 212: Sink = 212; break;
				case 213: Sink = 213; break;
				case 214: Sink = 214; break;
				case 215: Sink = 215; break;
				case 216: Sink = 216; break;
				case 217: Sink = 217; break;
				case 218: Sink = 218; break;
				case 219: Sink = 219; break;
				case 220: Sink = 220; break;
				case 221: Sink = 221; break;
				case 222: Sink = 222; break;
				case 223: Sink = 223; break;
				case 224: Sink = 224; break;
				case 225: Sink = 225; break;
				case 226: Sink = 226; break;
				case 227: Sink = 227; break;
				case 228: Sink = 228; break;
				case 229: Sink = 229; break;
				case 230: Sink = 230; break;
				case 231: Sink = 231; break;
				case 232: Sink = 232; break;
				case 233: Sink = 233; break;
				case 234: Sink = 234; break;
				case 235: Sink = 235; break;
				case 236: Sink = 236; break;
				case 237: Sink = 237; break;
				case 238: Sink = 238; break;
				case 239: Sink = 239; break;
				case 240: Sink = 240; break;
				case 241: Sink = 241; break;
				case 242: Sink = 242; break;
				case 243: Sink = 243; break;
				case 244: Sink = 244; break;
				case 245: Sink = 245; break;
				case 246: Sink = 246; break;
				case 247: Sink = 247; break;
				case 248: Sink = 248; break;
				case 249: Sink = 249; break;
				case 250: Sink = 250; break;
				case 251: Sink = 251; break;
				case 252: Sink = 252; break;
				case 253: Sink = 253; break;
				case 254: Sink = 254; break;
				case 255: Sink = 255; break;
				case 256: Sink = 256; break;
				case 257: Sink = 257; break;
				case 258: Sink = 258; break;
				case 259: Sink = 259; break;
				case 260: Sink = 260; break;
				case 261: Sink = 261; break;
				case 262: Sink = 262; break;
				case 263: Sink = 263; break;
				case 264: Sink = 264; break;
				case 265: Sink = 265; break;
				case 266: Sink = 266; break;
				case 267: Sink = 267; break;
				case 268: Sink = 268; break;
				case 269: Sink = 269; break;
				case 270: Sink = 270; break;
				case 271: Sink = 271; break;
				case 272: Sink = 272; break;
				case 273: Sink = 273; break;
				case 274: Sink = 274; break;
				case 275: Sink = 275; break;
				case 276: Sink = 276; break;
				case 277: Sink = 277; break;
				case 278: Sink = 278; break;
				case 279: Sink = 279; break;
				case 280: Sink = 280; break;
				case 281: Sink = 281; break;
				case 282: Sink = 282; break;
				case 283: Sink = 283; break;
				case 284: Sink = 284; break;
				case 285: Sink = 285; break;
				case 286: Sink = 286; break;
				case 287: Sink = 287; break;
				case 288: Sink = 288; break;
				case 289: Sink = 289; break;
				case 290: Sink = 290; break;
				case 291: Sink = 291; break;
				case 292: Sink = 292; break;
				case 293: Sink = 293; break;
				case 294: Sink = 294; break;
				case 295: Sink = 295; break;
				case 296: Sink = 296; break;
				case 297: Sink = 297; break;
				case 298: Sink = 298; break;
				case 299: Sink = 299; break;
				case 300: Sink = 300; break;
				case 301: Sink = 301; break;
				case 302: Sink = 302; break;
				case 303: Sink = 303; break;
				case 304: Sink = 304; break;
				case 305: Sink = 305; break;
				case 306: Sink = 306; break;
				case 307: Sink = 307; break;
				case 308: Sink = 308; break;
				case 309: Sink = 309; break;
				case 310: Sink = 310; break;
				case 311: Sink = 311; break;
				case 312: Sink = 312; break;
				case 313: Sink = 313; break;
				case 314: Sink = 314; break;
				case 315: Sink = 315; break;
				case 316: Sink = 316; break;
				case 317: Sink = 317; break;
				case 318: Sink = 318; break;
				case 319: Sink = 319; break;
				case 320: Sink = 320; break;
				case 321: Sink = 321; break;
				case 322: Sink = 322; break;
				case 323: Sink = 323; break;
				case 324: Sink = 324; break;
				case 325: Sink = 325; break;
				case 326: Sink = 326; break;
				case 327: Sink = 327; break;
				case 328: Sink = 328; break;
				case 329: Sink = 329; break;
				case 330: Sink = 330; break;
				case 331: Sink = 331; break;
				case 332: Sink = 332; break;
				case 333: Sink = 333; break;
				case 334: Sink = 334; break;
				case 335: Sink = 335; break;
				case 336: Sink = 336; break;
				case 337: Sink = 337; break;
				case 338: Sink = 338; break;
				case 339: Sink = 339; break;
				case 340: Sink = 340; break;
				case 341: Sink = 341; break;
				case 342: Sink = 342; break;
				case 343: Sink = 343; break;
				case 344: Sink = 344; break;
				case 345: Sink = 345; break;
				case 346: Sink = 346; break;
				case 347: Sink = 347; break;
				case 348: Sink = 348; break;
				case 349: Sink = 349; break;
				case 350: Sink = 350; break;
				case 351: Sink = 351; break;
				case 352: Sink = 352; break;
				case 353: Sink = 353; break;
				case 354: Sink = 354; break;
				case 355: Sink = 355; break;
				case 356: Sink = 356; break;
				case 357: Sink = 357; break;
				case 358: Sink = 358; break;
				case 359: Sink = 359; break;
				case 360: Sink = 360; break;
				case 361: Sink = 361; break;
				case 362: Sink = 362; break;
				case 363: Sink = 363; break;
				case 364: Sink = 364; break;
				case 365: Sink = 365; break;
				case 366: Sink = 366; break;
				case 367: Sink = 367; break;
				case 368: Sink = 368; break;
				case 369: Sink = 369; break;
				case 370: Sink = 370; break;
				case 371: Sink = 371; break;
				case 372: Sink = 372; break;
				case 373: Sink = 373; break;
				case 374: Sink = 374; break;
				case 375: Sink = 375; break;
				case 376: Sink = 376; break;
				case 377: Sink = 377; break;
				case 378: Sink = 378; break;
				case 379: Sink = 379; break;
				case 380: Sink = 380; break;
				case 381: Sink = 381; break;
				case 382: Sink = 382; break;
				case 383: Sink = 383; break;
				case 384: Sink = 384; break;
				case 385: Sink = 385; break;
				case 386: Sink = 386; break;
				case 387: Sink = 387; break;
				case 388: Sink = 388; break;
				case 389: Sink = 389; break;
				case 390: Sink = 390; break;
				case 391: Sink = 391; break;
				case 392: Sink = 392; break;
				case 393: Sink = 393; break;
				case 394: Sink = 394; break;
				case 395: Sink = 395; break;
				case 396: Sink = 396; break;
				case 397: Sink = 397; break;
				case 398: Sink = 398; break;
				case 399: Sink = 399; break;
				case 400: Sink = 400; break;
				case 401: Sink = 401; break;
				case 402: Sink = 402; break;
				case 403: Sink = 403; break;
				case 404: Sink = 404; break;
				case 405: Sink = 405; break;
				case 406: Sink = 406; break;
				case 407: Sink = 407; break;
				case 408: Sink = 408; break;
				case 409: Sink = 409; break;
				case 410: Sink = 410; break;
				case 411: Sink = 411; break;
				case 412: Sink = 412; break;
				case 413: Sink = 413; break;
				case 414: Sink = 414; break;
				case 415: Sink = 415; break;
				case 416: Sink = 416; break;
				case 417: Sink = 417; break;
				case 418: Sink = 418; break;
				case 419: Sink = 419; break;
				case 420: Sink = 420; break;
				case 421: Sink = 421; break;
				case 422: Sink = 422; break;
				case 423: Sink = 423; break;
				case 424: Sink = 424; break;
				case 425: Sink = 425; break;
				case 426: Sink = 426; break;
				case 427: Sink = 427; break;
				case 428: Sink = 428; break;
				case 429: Sink = 429; break;
				case 430: Sink = 430; break;
				case 431: Sink = 431; break;
				case 432: Sink = 432; break;
				case 433: Sink = 433; break;
				case 434: Sink = 434; break;
				case 435: Sink = 435; break;
				case 436: Sink = 436; break;
				case 437: Sink = 437; break;
				case 438: Sink = 438; break;
				case 439: Sink = 439; break;
				case 440: Sink = 440; break;
				case 441: Sink = 441; break;
				case 442: Sink = 442; break;
				case 443: Sink = 443; break;
				case 444: Sink = 444; break;
				case 445: Sink = 445; break;
				case 446: Sink = 446; break;
				case 447: Sink = 447; break;
				case 448: Sink = 448; break;
				case 449: Sink = 449; break;
				case 450: Sink = 450; break;
				case 451: Sink = 451; break;
				case 452: Sink = 452; break;
				case 453: Sink = 453; break;
				case 454: Sink = 454; break;
				case 455: Sink = 455; break;
				case 456: Sink = 456; break;
				case 457: Sink = 457; break;
				case 458: Sink = 458; break;
				case 459: Sink = 459; break;
				case 460: Sink = 460; break;
				case 461: Sink = 461; break;
				case 462: Sink = 462; break;
				case 463: Sink = 463; break;
				case 464: Sink = 464; break;
				case 465: Sink = 465; break;
				case 466: Sink = 466; break;
				case 467: Sink = 467; break;
				case 468: Sink = 468; break;
				case 469: Sink = 469; break;
				case 470: Sink = 470; break;
				case 471: Sink = 471; break;
				case 472: Sink = 472; break;
				case 473: Sink = 473; break;
				case 474: Sink = 474; break;
				case 475: Sink = 475; break;
				case 476: Sink = 476; break;
				case 477: Sink = 477; break;
				case 478: Sink = 478; break;
				case 479: Sink = 479; break;
				case 480: Sink = 480; break;
				case 481: Sink = 481; break;
				case 482: Sink = 482; break;
				case 483: Sink = 483; break;
				case 484: Sink = 484; break;
				case 485: Sink = 485; break;
				case 486: Sink = 486; break;
				case 487: Sink = 487; break;
				case 488: Sink = 488; break;
				case 489: Sink = 489; break;
				case 490: Sink = 490; break;
				case 491: Sink = 491; break;
				case 492: Sink = 492; break;
				case 493: Sink = 493; break;
				case 494: Sink = 494; break;
				case 495: Sink = 495; break;
				case 496: Sink = 496; break;
				case 497: Sink = 497; break;
				case 498: Sink = 498; break;
				case 499: Sink = 499; break;
				case 500: Sink = 500; break;
				case 501: Sink = 501; break;
				case 502: Sink = 502; break;
				case 503: Sink = 503; break;
				case 504: Sink = 504; break;
				case 505: Sink = 505; break;
				case 506: Sink = 506; break;
				case 507: Sink = 507; break;
				case 508: Sink = 508; break;
				case 509: Sink = 509; break;
				case 510: Sink = 510; break;
				case 511: Sink = 511; break;
			}
		}

		var made = values[root];

		global::System.Array.Clear(values, 0, records);

		return made;
	}

	/// <summary>An arm as the generator would have written it, reached through a table.</summary>
	delegate void Arm(Made[] values, int[] log, int at, string text);

	static void Arm0(Made[] values, int[] log, int at, string text) => values[at] = MakeLeaf(text, log[at + 2], log[at + 3]);
	static void Arm1(Made[] values, int[] log, int at, string text) => values[at] = MakePair(values[log[at + 2]], values[log[at + 3]]);
	static void Arm2(Made[] values, int[] log, int at, string text) => values[at] = MakeJoin(values[log[at + 2]], values[log[at + 3]]);
	static void Nothing(Made[] values, int[] log, int at, string text) { }

	/// <summary>Built once, at class initialization, and never during a parse.</summary>
	static readonly Arm[] Arms = Table();

	static Arm[] Table()
	{
		var arms = new Arm[512];

		for (var i = 0; i < arms.Length; i++)
			arms[i] = Nothing;

		arms[0] = Arm0;
		arms[1] = Arm1;
		arms[2] = Arm2;

		return arms;
	}

	[Benchmark]
	public Made called()
	{
		var count  = RecordByPlace(out var root, out _);
		var log    = _log;
		var text   = _text;
		var values = _byCall;

		if (values.Length < count)
			values = _byCall = new Made[count * 2];

		for (var at = 0; at < count; at += log[at])
			Arms[log[at + 1]](values, log, at, text);

		var made = values[root];

		global::System.Array.Clear(values, 0, count);

		return made;
	}

	/// <summary>
	/// The floor: no deferral at all, the tree built where it is read, which is what the
	/// hand-written parser does and what the language does not let a generated one do. It is
	/// here to price the deferral, not as a candidate.
	/// </summary>
	[Benchmark]
	public Made eager()
	{
		var text   = _text;
		var at     = _at;
		var made   = _made;

		for (var i = 0; i < 128; i++)
			made[i] = MakeLeaf(text, at[i * 2], at[i * 2 + 1]);

		var spine = MakePair(made[0], made[1]);

		for (var i = 1; i < 64; i++)
			spine = MakeJoin(spine, MakePair(made[i * 2], made[i * 2 + 1]));

		return spine;
	}

	[Benchmark]
	public Made closures()
	{
		var text   = _text;
		var at     = _at;
		var leaves = new Func<Made>[128];
		var pairs  = new Func<Made>[64];

		for (var i = 0; i < 128; i++)
		{
			var from = at[i * 2];
			var to   = at[i * 2 + 1];

			leaves[i] = () => MakeLeaf(text, from, to);
		}

		for (var i = 0; i < 64; i++)
		{
			var l = leaves[i * 2];
			var r = leaves[i * 2 + 1];

			pairs[i] = () => MakePair(l(), r());
		}

		var spine = pairs[0];

		for (var i = 0; i < 63; i++)
		{
			var l = spine;
			var r = pairs[i + 1];

			spine = () => MakeJoin(l(), r());
		}

		return spine();
	}
}
