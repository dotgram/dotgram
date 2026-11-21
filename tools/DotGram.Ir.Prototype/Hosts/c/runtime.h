/* What every printed parser stands on: the input, the tape of decisions, the furthest refusal,
   the register a function leaves its value in, and the values themselves. Hand-written, once
   for every grammar.

   Ownership: everything a reading makes - sequences, and whatever the host builds - comes out of
   the parser's arena and lives until the next reading begins. Nothing is freed one by one. */

#pragma once

#include <stdbool.h>
#include <stdint.h>
#include <stdlib.h>
#include <string.h>

/* Built with RT_ALWAYS_INLINE (a measurement), every function here is inlined wherever it is
   called: the compiler then cannot take one out of line because a copy of the code that reads the
   tape back calls it too. */
#ifdef RT_ALWAYS_INLINE
#define RT_INLINE static inline __attribute__((always_inline))
#else
#define RT_INLINE static inline
#endif

/* Built with RT_SPLIT (a measurement, printed by a parser that asks for it), the helpers marked
   RT_HOT - those gcc leaves out of line in the code of a first reading once copies that read the
   tape back call them too - are inlined wherever they are called, and those copies call the cold
   instances at the end of this file instead: no out-of-line helper is shared by the two. */
#if defined(RT_SPLIT) && !defined(RT_ALWAYS_INLINE)
#define RT_HOT static inline __attribute__((always_inline))
#else
#define RT_HOT RT_INLINE
#endif

enum { V_NONE, V_TEXT, V_LIST, V_HOST };

struct List;

/* A value: nothing, a text of the input (from, to), a sequence, or what the host built. */
typedef struct Val
{
	int32_t kind;
	int32_t from, to;
	void   *ptr;
} Val;

typedef struct List
{
	int32_t count, capacity;
	Val    *items;
} List;

typedef struct Chunk
{
	struct Chunk *next;
	size_t        used, size;
} Chunk;

/* What a resuming reading keeps: a record a turn begun or a statement ended, behind the sentinel
   that names the statement's entry on the tape. Flags: 1 a statement ended (else a turn began),
   2 it read, 4 it was made unbuilt. Base is where its slots' values begin in kv. */
typedef struct Kept
{
	int32_t key, owner, prev, pos, turns, flags, base, serial;
} Kept;

/* A cut remembered while any record is kept: the place cut to, and its serial. */
typedef struct Cut
{
	int32_t key, serial;
} Cut;

/* What was refused before an attempt that stood was read once more to be built. */
typedef struct KeptRefusal
{
	int32_t at, count;
	int32_t refused[64];
} KeptRefusal;

typedef struct Parser
{
	const uint16_t *text;
	int32_t         len;

	void *host;

	/* What the function that last returned built. */
	Val rv;

	/* Inside a look: nothing is recorded, and nothing is built. */
	int32_t quiet, unbuilt;

	int32_t refusedAt;
	int32_t refused[64];
	int32_t refusedCount;

	/* The tape: a decision and the last value it can take, an entry. */
	int32_t *ways;
	int32_t  waysSize, count, cursor, retries;

	/* What a parser printed to count writes: functions entered and turns begun, decisions opened
	   or read back, turns gone straight to, host calls. A parser printed to be timed never
	   touches them. */
	int64_t kSteps, kEntered, kJumps, kHost;

	/* The records, a stack in the order of their keys (the cursor where each was made), cut with
	   the tape; the values of the slots they hold, a slot a value (a list as its pointer and its
	   length); and the head of each owner, beside the tape and never cleared: a head left from an
	   earlier owner or an earlier reading is told apart by the owner its record names. */
	Kept    *kept;
	int32_t  keptSize, kCount;
	Val     *kv;
	int32_t  kvSize, kvTop;
	int32_t *heads;
	int32_t  headsSize;

	/* Places with no slot of their own on the tape: an entry is named by its key (place, cursor,
	   position), each key a number that stands where a sentinel's slot stands for the heads. */
	int32_t *keyTable;
	int32_t  keyMask;
	int32_t *keys;
	int32_t  keyCount, keysSize;

	/* Wrong on purpose: a head is taken without asking whose it is (the test of the owner check). */
	bool anyOwner;

	/* The cuts made while any record is kept, those that matter: a later cut to a lower or equal
	   place takes the place of an earlier one, so places and serials rise from the bottom. A record
	   is stale once a cut remembered after it went to its key or below. */
	Cut    *cuts;
	int32_t cutsSize, cutTop, serial;

	/* Wrong on purpose: a record is taken whatever was cut after it (the test of the cut check). */
	bool noCutCheck;

	KeptRefusal *refusals;
	int32_t      refusalsSize, refusalsCount;

	Chunk *arena;

	/* Restart by budget (a measurement, printed by a parser that asks for it): what the first
	   reading has read again, the furthest position it has inspected (inside looks too), the
	   readings that were given up and read again from the start by the resuming set, and where the
	   reading goes back to when it gives up. At the end, so that no other field moves. */
	int64_t reread;
	int32_t far, restarts;
	void   *restart;
} Parser;

#define VAL_NONE ((Val){ V_NONE, 0, 0, NULL })

RT_HOT void *rt_alloc(Parser *P, size_t size)
{
	size = (size + 15) & ~(size_t)15;

	if (P->arena == NULL || P->arena->used + size > P->arena->size)
	{
		size_t room  = size > 65536 ? size : 65536;
		Chunk *chunk = malloc(sizeof(Chunk) + room);

		chunk->next = P->arena;
		chunk->used = 0;
		chunk->size = room;
		P->arena    = chunk;
	}

	void *at = (char *)(P->arena + 1) + P->arena->used;

	P->arena->used += size;

	return at;
}

RT_INLINE void rt_begin(Parser *P, const uint16_t *text, int32_t len)
{
	while (P->arena != NULL)
	{
		Chunk *next = P->arena->next;

		free(P->arena);
		P->arena = next;
	}

	if (P->ways == NULL)
	{
		P->waysSize = 64;
		P->ways     = malloc(sizeof(int32_t) * 64);
		P->heads    = malloc(sizeof(int32_t) * 64);
		P->headsSize = 64;
	}

	P->text         = text;
	P->len          = len;
	P->rv           = VAL_NONE;
	P->quiet        = 0;
	P->unbuilt      = 0;
	P->refusedAt    = -1;
	P->refusedCount = 0;
	P->count        = 0;
	P->cursor       = 0;
	P->retries      = 0;
	P->kSteps       = 0;
	P->kEntered     = 0;
	P->kJumps       = 0;
	P->kHost        = 0;
	P->kCount       = 0;
	P->cutTop       = 0;
	P->serial       = 0;
	P->kvTop        = 0;
	P->refusalsCount = 0;

	if (P->keyCount > 0)
	{
		memset(P->keyTable, 0xff, sizeof(int32_t) * (size_t)(P->keyMask + 1));
		P->keyCount = 0;
	}
}

RT_HOT void rt_refuse(Parser *P, int32_t at, int32_t expected)
{
	if (at > P->refusedAt)
	{
		P->refusedAt    = at;
		P->refusedCount = 0;
	}

	for (int32_t i = 0; i < P->refusedCount; i++)
		if (P->refused[i] == expected)
			return;

	if (P->refusedCount < 64)
		P->refused[P->refusedCount++] = expected;
}

RT_HOT int32_t rt_enter(Parser *P, int32_t last)
{
	if (P->cursor < P->count)
		return P->cursor++;

	if (P->count * 2 + 2 > P->waysSize)
	{
		P->waysSize *= 2;
		P->ways      = realloc(P->ways, sizeof(int32_t) * (size_t)P->waysSize);
		if (P->waysSize > P->headsSize)
		{
			P->headsSize = P->waysSize;
			P->heads     = realloc(P->heads, sizeof(int32_t) * (size_t)P->headsSize);
		}
	}

	P->ways[P->count * 2]     = 0;
	P->ways[P->count * 2 + 1] = last;
	P->count++;
	P->cursor = P->count;

	return P->count - 1;
}

RT_INLINE int32_t rt_enterCounted(Parser *P, int32_t last)
{
	P->kEntered++;

	return rt_enter(P, last);
}

/* What a parser printed to measure the least cost of its kept statements calls where the tape is
   read back: out of line, and nothing in it but a count, so that the entry is one compare. */
static __attribute__((noinline)) void rt_slow(Parser *P)
{
	P->kJumps++;
}

/* The number of an entry into a place with no slot on the tape, where the tape is read back: the
   same place entered at the same cursor and position is the same entry, and finds its records. */
RT_INLINE int32_t rt_key(Parser *P, int32_t place, int32_t cursor, int32_t pos)
{
	if (P->keyTable == NULL || (P->keyCount + 1) * 2 > P->keyMask + 1)
	{
		int32_t size = P->keyTable == NULL ? 256 : (P->keyMask + 1) * 2;

		free(P->keyTable);
		P->keyTable = malloc(sizeof(int32_t) * (size_t)size);
		P->keyMask  = size - 1;
		memset(P->keyTable, 0xff, sizeof(int32_t) * (size_t)size);

		for (int32_t id = 0; id < P->keyCount; id++)
		{
			uint32_t again = ((uint32_t)P->keys[id * 3] * 0x9E3779B1u) ^ ((uint32_t)P->keys[id * 3 + 1] * 0x85EBCA77u) ^ ((uint32_t)P->keys[id * 3 + 2] * 0xC2B2AE3Du);

			again ^= again >> 15;

			while (P->keyTable[again & (uint32_t)P->keyMask] >= 0)
				again++;

			P->keyTable[again & (uint32_t)P->keyMask] = id;
		}
	}

	uint32_t h = ((uint32_t)place * 0x9E3779B1u) ^ ((uint32_t)cursor * 0x85EBCA77u) ^ ((uint32_t)pos * 0xC2B2AE3Du);

	h ^= h >> 15;

	for (;; h++)
	{
		int32_t id = P->keyTable[h & (uint32_t)P->keyMask];

		if (id < 0)
		{
			id = P->keyCount++;

			if (P->keyCount * 3 > P->keysSize)
			{
				P->keysSize = P->keysSize == 0 ? 192 : P->keysSize * 2;
				P->keys     = realloc(P->keys, sizeof(int32_t) * (size_t)P->keysSize);
			}

			if (P->keyCount > P->headsSize)
			{
				P->headsSize *= 2;
				P->heads      = realloc(P->heads, sizeof(int32_t) * (size_t)P->headsSize);
			}

			P->keys[id * 3]     = place;
			P->keys[id * 3 + 1] = cursor;
			P->keys[id * 3 + 2] = pos;
			P->heads[id]        = -1;
			P->keyTable[h & (uint32_t)P->keyMask] = id;

			return id;
		}

		if (P->keys[id * 3] == place && P->keys[id * 3 + 1] == cursor && P->keys[id * 3 + 2] == pos)
			return id;
	}
}

/* A sentinel opened where the tape is read for the first time: a decision that can never move. */
RT_INLINE int32_t rt_sentinel(Parser *P)
{
	return rt_enter(P, 0);
}

/* The tape was cut: what was kept of places no longer behind it goes, and the cut is remembered
   for the records below the top it makes stale. */
RT_HOT void rt_cut(Parser *P)
{
	while (P->kCount > 0 && P->kept[P->kCount - 1].key >= P->count)
	{
		Kept *last = &P->kept[--P->kCount];

		P->heads[last->owner] = last->prev;
		P->kvTop              = last->base;
	}

	if (P->kCount == 0)
	{
		P->cutTop = 0;

		return;
	}

	while (P->cutTop > 0 && P->cuts[P->cutTop - 1].key >= P->count)
		P->cutTop--;

	if (P->cutTop == P->cutsSize)
	{
		P->cutsSize = P->cutsSize == 0 ? 64 : P->cutsSize * 2;
		P->cuts     = realloc(P->cuts, sizeof(Cut) * (size_t)P->cutsSize);
	}

	P->cuts[P->cutTop++] = (Cut){ P->count, ++P->serial };
}

/* Whether a cut remembered after the serial went to the key or below. */
RT_INLINE bool rt_cut_after(Parser *P, int32_t serial, int32_t key)
{
	int32_t top = P->cutTop - 1;

	if (top < 0 || P->cuts[top].serial <= serial)
		return false;

	if (P->cuts[top].key <= key)
		return true;

	int32_t low = 0, high = top;

	while (low < high)
	{
		int32_t middle = (low + high) / 2;

		if (P->cuts[middle].serial > serial)
			high = middle;
		else
			low = middle + 1;
	}

	return P->cuts[low].key <= key;
}

RT_HOT void rt_next(Parser *P, int32_t way, int32_t value)
{
	P->ways[way * 2] = value;
	P->count         = way + 1;
	P->cursor        = way + 1;

	if (P->kCount > 0)
		rt_cut(P);
}

/* The latest good record of a sentinel's entry, or -1: a head left from an earlier entry or
   reading names another owner; stale records on the way are passed over and the head moves on. */
RT_INLINE int32_t rt_head(Parser *P, int32_t sentinel)
{
	int32_t h = P->heads[sentinel];

	while (h >= 0 && h < P->kCount && (P->anyOwner || P->kept[h].owner == sentinel))
	{
		if (P->noCutCheck || (P->kept[h].key < P->count && !rt_cut_after(P, P->kept[h].serial, P->kept[h].key)))
		{
			P->heads[sentinel] = h;

			return h;
		}

		h = P->kept[h].prev;
	}

	return -1;
}

/* Whether a record can stand for what the reading would do: one made while building, always; one
   made unbuilt, only in a reading that builds nothing. */
RT_INLINE bool rt_usable(Parser *P, int32_t h)
{
	return (P->kept[h].flags & 4) == 0 || P->unbuilt > 0;
}

/* A record of where a turn began (end false) or a statement ended, at the cursor, where one is
   wanted: none of the owner yet, a later turn than its last, or one made unbuilt where this reading
   builds. The index, whose n slots the caller fills, or -1. */
RT_INLINE int32_t rt_keep(Parser *P, int32_t owner, bool end, int32_t p, int32_t turns, bool read, int32_t n)
{
	int32_t key = P->cursor;
	int32_t h   = rt_head(P, owner);

	if (h >= 0 && !(end ? (P->kept[h].flags & 1) == 0 : (P->kept[h].flags & 1) == 0 && P->kept[h].turns < turns) && rt_usable(P, h))
		return -1;

	if (P->kCount == P->keptSize)
	{
		P->keptSize = P->keptSize == 0 ? 64 : P->keptSize * 2;
		P->kept     = realloc(P->kept, sizeof(Kept) * (size_t)P->keptSize);
	}

	if (P->kvTop + n > P->kvSize)
	{
		P->kvSize = P->kvTop + n > P->kvSize * 2 ? P->kvTop + n : (P->kvSize == 0 ? 64 : P->kvSize * 2);
		P->kv     = realloc(P->kv, sizeof(Val) * (size_t)P->kvSize);
	}

	int32_t i = P->kCount++;

	P->kept[i] = (Kept){ key, owner, h, p, turns, (end ? 1 : 0) | (read ? 2 : 0) | (P->unbuilt > 0 ? 4 : 0), P->kvTop, P->serial };
	P->kvTop  += n;
	P->heads[owner] = i;

	return i;
}

/* What the tape and the records hold on to, in bytes, as allocated. */
RT_INLINE int64_t rt_held(Parser *P)
{
	return (int64_t)P->waysSize * 4 + (int64_t)P->waysSize * 4 + (int64_t)P->keptSize * (int64_t)sizeof(Kept) + (int64_t)P->kvSize * (int64_t)sizeof(Val);
}

/* After a reading: storage grown past the bound (in entries of the tape) goes back to its first
   size, so that one long refusal does not keep its arrays for every reading after it. */
RT_INLINE void rt_release(Parser *P, int32_t bound)
{
	if (P->waysSize > bound * 2)
	{
		P->waysSize = 64;
		P->ways     = realloc(P->ways, sizeof(int32_t) * 64);
		P->heads    = realloc(P->heads, sizeof(int32_t) * 64);
		P->headsSize = 64;
	}

	if (P->keysSize > bound * 3)
	{
		P->keysSize = 0;
		P->keyMask  = 0;
		P->keyCount = 0;
		free(P->keys);
		free(P->keyTable);
		P->keys     = NULL;
		P->keyTable = NULL;
	}

	if (P->keptSize > bound)
	{
		P->keptSize = 64;
		P->kept     = realloc(P->kept, sizeof(Kept) * 64);
	}

	if (P->kvSize > bound)
	{
		P->kvSize = 64;
		P->kv     = realloc(P->kv, sizeof(Val) * 64);
	}
}

/* A way given back: the first is read building still; the second, where the reading was building, stops building. */
RT_INLINE int32_t rt_retried(Parser *P, int32_t u)
{
	if (u == 1)
	{
		P->unbuilt = 1;

		return 2;
	}

	return u == 0 && P->unbuilt == 0 ? 1 : u;
}

RT_HOT void rt_replayed(Parser *P)
{
	KeptRefusal *kept = &P->refusals[--P->refusalsCount];

	P->refusedAt    = kept->at;
	P->refusedCount = kept->count;
	memcpy(P->refused, kept->refused, sizeof(int32_t) * (size_t)kept->count);
}

/* An attempt stood after the loop stopped building: it is read once more, built, from the loop's
   start (3); or that reading stood, and what it said of a refusal is taken back (0). */
RT_HOT int32_t rt_stood(Parser *P, int32_t u, int32_t segment)
{
	if (u == 2)
	{
		if (P->refusalsCount == P->refusalsSize)
		{
			P->refusalsSize = P->refusalsSize == 0 ? 8 : P->refusalsSize * 2;
			P->refusals     = realloc(P->refusals, sizeof(KeptRefusal) * (size_t)P->refusalsSize);
		}

		KeptRefusal *kept = &P->refusals[P->refusalsCount++];

		kept->at    = P->refusedAt;
		kept->count = P->refusedCount;
		memcpy(kept->refused, P->refused, sizeof(int32_t) * (size_t)P->refusedCount);
		P->unbuilt = 0;
		P->cursor  = segment;

		return 3;
	}

	rt_replayed(P);

	return 0;
}

/* A loop that ends without an answer puts back what it raised. */
RT_HOT void rt_undeferred(Parser *P, int32_t u)
{
	if (u == 2)
		P->unbuilt = 0;
	else if (u == 3)
		rt_replayed(P);
}

RT_HOT bool rt_retry(Parser *P, int32_t segment)
{
	for (int32_t way = P->cursor - 1; way >= segment; way--)
	{
		if (P->ways[way * 2] < P->ways[way * 2 + 1])
		{
			P->ways[way * 2]++;
			P->count  = way + 1;
			P->cursor = segment;
			P->retries++;

			if (P->kCount > 0)
				rt_cut(P);

			return true;
		}
	}

	return false;
}

RT_HOT bool rt_starts(Parser *P, int32_t at, const uint16_t *literal, int32_t length)
{
	return at + length <= P->len && memcmp(P->text + at, literal, sizeof(uint16_t) * (size_t)length) == 0;
}

/* How far the input agrees with a text it does not go on to match. */
RT_HOT int32_t rt_agree(Parser *P, int32_t at, const uint16_t *literal, int32_t length)
{
	int32_t agreed = 0;

	while (agreed < length && at + agreed < P->len && P->text[at + agreed] == literal[agreed])
		agreed++;

	return agreed;
}

RT_INLINE bool rt_same(Parser *P, int32_t from, int32_t at, int32_t length)
{
	return memcmp(P->text + from, P->text + at, sizeof(uint16_t) * (size_t)length) == 0;
}

RT_INLINE Val val_text(int32_t from, int32_t to)
{
	return (Val){ V_TEXT, from, to, NULL };
}

RT_INLINE Val val_host(void *built)
{
	return (Val){ V_HOST, 0, 0, built };
}

RT_HOT List *list_add(Parser *P, List *list, Val value)
{
	if (list == NULL)
	{
		list           = rt_alloc(P, sizeof(List));
		list->count    = 0;
		list->capacity = 4;
		list->items    = rt_alloc(P, sizeof(Val) * 4);
	}

	if (list->count == list->capacity)
	{
		Val *grown = rt_alloc(P, sizeof(Val) * (size_t)list->capacity * 2);

		memcpy(grown, list->items, sizeof(Val) * (size_t)list->count);
		list->items     = grown;
		list->capacity *= 2;
	}

	list->items[list->count++] = value;

	return list;
}

/* The values a list holds now, as a sequence that no later turn changes. */
RT_HOT Val val_items(Parser *P, List *list)
{
	List *copy = rt_alloc(P, sizeof(List));

	copy->count    = list == NULL ? 0 : list->count;
	copy->capacity = copy->count;
	copy->items    = copy->count == 0 ? NULL : rt_alloc(P, sizeof(Val) * (size_t)copy->count);

	if (copy->count > 0)
		memcpy(copy->items, list->items, sizeof(Val) * (size_t)copy->count);

	return (Val){ V_LIST, 0, 0, copy };
}

#ifdef RT_RESTART
#ifndef RT_BUDGET
#define RT_BUDGET 4
#endif

/* A reading again of a region of the first reading, from start: what it costs is charged, at least
   one, and past RT_BUDGET times what the reading has inspected the reading is given up. The
   furthest position is the most of every refusal outside looks (refusedAt) and inside them (far). */
static __attribute__((noinline, cold, unused)) void rt_charge(Parser *P, int32_t start)
{
	int32_t far = P->far > P->refusedAt ? P->far : P->refusedAt;

	if (far < start)
		far = start;

	P->far     = far;
	P->reread += far - start > 1 ? far - start : 1;

	if (P->reread > (int64_t)RT_BUDGET * (far + 1))
		__builtin_longjmp((void **)P->restart, 1);
}

/* The first reading given up: the tape, the records and the refusal as a reading begins, for the
   resuming set to read the input again from the start. What was built stays in the arena. */
static __attribute__((noinline, cold, unused)) void rt_restarted(Parser *P)
{
	P->rv            = VAL_NONE;
	P->quiet         = 0;
	P->unbuilt       = 0;
	P->refusedAt     = -1;
	P->refusedCount  = 0;
	P->count         = 0;
	P->cursor        = 0;
	P->kCount        = 0;
	P->cutTop        = 0;
	P->serial        = 0;
	P->kvTop         = 0;
	P->refusalsCount = 0;
	P->restarts++;

	if (P->keyCount > 0)
	{
		memset(P->keyTable, 0xff, sizeof(int32_t) * (size_t)(P->keyMask + 1));
		P->keyCount = 0;
	}
}

/* RT_NO_COUNTER (a measurement): the first reading printed with no counter at all, to be held to the
   parser without it; RT_NO_CHARGE and RT_NO_REACH leave out one half each. */
#if defined(RT_NO_COUNTER) || defined(RT_NO_CHARGE)
#define RT_CHARGE(P, start)
#else
#define RT_CHARGE(P, start) rt_charge(P, start)
#endif

/* A refusal: outside a look as ever; inside one it says nothing, but the position it inspected is
   the furthest the reading has reached, where it is further than any before. The test of the look
   moves into the helper, so that the code of a refusal is one compare, as it was two. */
RT_INLINE void rt_refuse_far(Parser *P, int32_t at, int32_t expected)
{
	if (P->quiet != 0)
	{
		if (at > P->far)
			P->far = at;

		return;
	}

	rt_refuse(P, at, expected);
}

/* The twins over the split runtime call cold instances (RT_REFUSE_cold), as the parser without restart has them. */
#if defined(RT_NO_COUNTER) || defined(RT_NO_REACH)
#define RT_REFUSE(P, at, expected) if ((P)->quiet == 0 && (at) >= (P)->refusedAt) rt_refuse(P, at, expected)
#define RT_REFUSE_cold(P, at, expected) if ((P)->quiet == 0 && (at) >= (P)->refusedAt) rt_refuse_cold(P, at, expected)
#else
#define RT_REFUSE(P, at, expected) if ((at) >= (P)->refusedAt) rt_refuse_far(P, at, expected)
#define RT_REFUSE_cold(P, at, expected) if ((at) >= (P)->refusedAt) rt_refuse_far_cold(P, at, expected)
#endif
#endif

#ifdef RT_SPLIT
#define RT_COLD static __attribute__((noinline, cold, unused))

RT_COLD void rt_refuse_cold(Parser *P, int32_t at, int32_t expected) { rt_refuse(P, at, expected); }
#ifdef RT_RESTART
RT_COLD void rt_refuse_far_cold(Parser *P, int32_t at, int32_t expected) { rt_refuse_far(P, at, expected); }
#endif
RT_COLD int32_t rt_enter_cold(Parser *P, int32_t last) { return rt_enter(P, last); }
RT_COLD void rt_cut_cold(Parser *P) { rt_cut(P); }
RT_COLD void rt_next_cold(Parser *P, int32_t way, int32_t value) { rt_next(P, way, value); }
RT_COLD void rt_replayed_cold(Parser *P) { rt_replayed(P); }
RT_COLD int32_t rt_stood_cold(Parser *P, int32_t u, int32_t segment) { return rt_stood(P, u, segment); }
RT_COLD void rt_undeferred_cold(Parser *P, int32_t u) { rt_undeferred(P, u); }
RT_COLD bool rt_retry_cold(Parser *P, int32_t segment) { return rt_retry(P, segment); }
RT_COLD List *list_add_cold(Parser *P, List *list, Val value) { return list_add(P, list, value); }
RT_COLD Val val_items_cold(Parser *P, List *list) { return val_items(P, list); }
RT_COLD bool rt_starts_cold(Parser *P, int32_t at, const uint16_t *literal, int32_t length) { return rt_starts(P, at, literal, length); }
RT_COLD int32_t rt_agree_cold(Parser *P, int32_t at, const uint16_t *literal, int32_t length) { return rt_agree(P, at, literal, length); }
#endif

/* What a host gives the driver, beside the functions the parser calls. */
void host_begin(Parser *P, const char *answers, const char *throwOn);
void host_render(Parser *P, Val value);
void host_end(Parser *P);
