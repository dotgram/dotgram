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
	int32_t key, owner, prev, pos, turns, flags, base;
} Kept;

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

	/* Wrong on purpose: a head is taken without asking whose it is (the test of the owner check). */
	bool anyOwner;

	KeptRefusal *refusals;
	int32_t      refusalsSize, refusalsCount;

	Chunk *arena;
} Parser;

#define VAL_NONE ((Val){ V_NONE, 0, 0, NULL })

static inline void *rt_alloc(Parser *P, size_t size)
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

static inline void rt_begin(Parser *P, const uint16_t *text, int32_t len)
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
	P->kvTop        = 0;
	P->refusalsCount = 0;
}

static inline void rt_refuse(Parser *P, int32_t at, int32_t expected)
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

static inline int32_t rt_enter(Parser *P, int32_t last)
{
	if (P->cursor < P->count)
		return P->cursor++;

	if (P->count * 2 + 2 > P->waysSize)
	{
		P->waysSize *= 2;
		P->ways      = realloc(P->ways, sizeof(int32_t) * (size_t)P->waysSize);
		P->heads     = realloc(P->heads, sizeof(int32_t) * (size_t)P->waysSize);
	}

	P->ways[P->count * 2]     = 0;
	P->ways[P->count * 2 + 1] = last;
	P->count++;
	P->cursor = P->count;

	return P->count - 1;
}

static inline int32_t rt_enterCounted(Parser *P, int32_t last)
{
	P->kEntered++;

	return rt_enter(P, last);
}

/* The tape was cut: what was kept of places no longer behind it goes. */
static inline void rt_cut(Parser *P)
{
	while (P->kCount > 0 && P->kept[P->kCount - 1].key >= P->count)
	{
		Kept *last = &P->kept[--P->kCount];

		P->heads[last->owner] = last->prev;
		P->kvTop              = last->base;
	}
}

static inline void rt_next(Parser *P, int32_t way, int32_t value)
{
	P->ways[way * 2] = value;
	P->count         = way + 1;
	P->cursor        = way + 1;

	if (P->kCount > 0)
		rt_cut(P);
}

/* The latest record of a sentinel's entry, or -1. */
static inline int32_t rt_head(Parser *P, int32_t sentinel)
{
	int32_t h = P->heads[sentinel];

	return h >= 0 && h < P->kCount && (P->anyOwner || P->kept[h].owner == sentinel) ? h : -1;
}

/* Whether a record can stand for what the reading would do: one made while building, always; one
   made unbuilt, only in a reading that builds nothing. */
static inline bool rt_usable(Parser *P, int32_t h)
{
	return (P->kept[h].flags & 4) == 0 || P->unbuilt > 0;
}

/* A record of where a turn began (end false) or a statement ended, at the cursor, where one is
   wanted: none of the owner yet, a later turn than its last, or one made unbuilt where this reading
   builds. The index, whose n slots the caller fills, or -1. */
static inline int32_t rt_keep(Parser *P, int32_t owner, bool end, int32_t p, int32_t turns, bool read, int32_t n)
{
	int32_t key  = P->cursor;
	int32_t h    = rt_head(P, owner);
	bool    fits = P->kCount == 0 || P->kept[P->kCount - 1].key <= key;

	if (h >= 0 && !(end ? (P->kept[h].flags & 1) == 0 : (P->kept[h].flags & 1) == 0 && P->kept[h].turns < turns) && !(!rt_usable(P, h) && fits))
		return -1;

	if (!fits)
		abort();

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

	P->kept[i] = (Kept){ key, owner, h, p, turns, (end ? 1 : 0) | (read ? 2 : 0) | (P->unbuilt > 0 ? 4 : 0), P->kvTop };
	P->kvTop  += n;
	P->heads[owner] = i;

	return i;
}

/* What the tape and the records hold on to, in bytes, as allocated. */
static inline int64_t rt_held(Parser *P)
{
	return (int64_t)P->waysSize * 4 + (int64_t)P->waysSize * 4 + (int64_t)P->keptSize * (int64_t)sizeof(Kept) + (int64_t)P->kvSize * (int64_t)sizeof(Val);
}

/* After a reading: storage grown past the bound (in entries of the tape) goes back to its first
   size, so that one long refusal does not keep its arrays for every reading after it. */
static inline void rt_release(Parser *P, int32_t bound)
{
	if (P->waysSize > bound * 2)
	{
		P->waysSize = 64;
		P->ways     = realloc(P->ways, sizeof(int32_t) * 64);
		P->heads    = realloc(P->heads, sizeof(int32_t) * 64);
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
static inline int32_t rt_retried(Parser *P, int32_t u)
{
	if (u == 1)
	{
		P->unbuilt = 1;

		return 2;
	}

	return u == 0 && P->unbuilt == 0 ? 1 : u;
}

static inline void rt_replayed(Parser *P)
{
	KeptRefusal *kept = &P->refusals[--P->refusalsCount];

	P->refusedAt    = kept->at;
	P->refusedCount = kept->count;
	memcpy(P->refused, kept->refused, sizeof(int32_t) * (size_t)kept->count);
}

/* An attempt stood after the loop stopped building: it is read once more, built, from the loop's
   start (3); or that reading stood, and what it said of a refusal is taken back (0). */
static inline int32_t rt_stood(Parser *P, int32_t u, int32_t segment)
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
static inline void rt_undeferred(Parser *P, int32_t u)
{
	if (u == 2)
		P->unbuilt = 0;
	else if (u == 3)
		rt_replayed(P);
}

static inline bool rt_retry(Parser *P, int32_t segment)
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

static inline bool rt_starts(Parser *P, int32_t at, const uint16_t *literal, int32_t length)
{
	return at + length <= P->len && memcmp(P->text + at, literal, sizeof(uint16_t) * (size_t)length) == 0;
}

/* How far the input agrees with a text it does not go on to match. */
static inline int32_t rt_agree(Parser *P, int32_t at, const uint16_t *literal, int32_t length)
{
	int32_t agreed = 0;

	while (agreed < length && at + agreed < P->len && P->text[at + agreed] == literal[agreed])
		agreed++;

	return agreed;
}

static inline bool rt_same(Parser *P, int32_t from, int32_t at, int32_t length)
{
	return memcmp(P->text + from, P->text + at, sizeof(uint16_t) * (size_t)length) == 0;
}

static inline Val val_text(int32_t from, int32_t to)
{
	return (Val){ V_TEXT, from, to, NULL };
}

static inline Val val_host(void *built)
{
	return (Val){ V_HOST, 0, 0, built };
}

static inline List *list_add(Parser *P, List *list, Val value)
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
static inline Val val_items(Parser *P, List *list)
{
	List *copy = rt_alloc(P, sizeof(List));

	copy->count    = list == NULL ? 0 : list->count;
	copy->capacity = copy->count;
	copy->items    = copy->count == 0 ? NULL : rt_alloc(P, sizeof(Val) * (size_t)copy->count);

	if (copy->count > 0)
		memcpy(copy->items, list->items, sizeof(Val) * (size_t)copy->count);

	return (Val){ V_LIST, 0, 0, copy };
}

/* What a host gives the driver, beside the functions the parser calls. */
void host_begin(Parser *P, const char *answers, const char *throwOn);
void host_render(Parser *P, Val value);
void host_end(Parser *P);
