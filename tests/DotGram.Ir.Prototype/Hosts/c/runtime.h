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
	}

	P->ways[P->count * 2]     = 0;
	P->ways[P->count * 2 + 1] = last;
	P->count++;
	P->cursor = P->count;

	return P->count - 1;
}

static inline void rt_next(Parser *P, int32_t way, int32_t value)
{
	P->ways[way * 2] = value;
	P->count         = way + 1;
	P->cursor        = way + 1;
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
