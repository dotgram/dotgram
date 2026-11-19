/* A hand-written host for the JSON grammar of DotGram.Web (RFC 8259): the ten host functions its
   fragments were lifted into, building a tree, and the tree written back as DotGram.Web writes it.
   The numbers are the fragments' order in the grammar; parser.h says which text each one was.

   Ownership: the tree is the parser's arena's, like every value of a reading - a number points
   into the input, a string is undone into the arena - and it lives until the next reading begins. */

#include <stdio.h>

#include "runtime.h"
#include "parser.h"

enum { J_NULL, J_FALSE, J_TRUE, J_NUMBER, J_STRING, J_ARRAY, J_OBJECT };

struct Member;

typedef struct Json
{
	int             kind;
	int32_t         count;    /* units of a number or a string, items of an array, members of an object */
	const uint16_t *units;
	struct Json   **items;
	struct Member **members;
} Json;

typedef struct Member
{
	const uint16_t *name;
	int32_t         length;
	Json           *value;
} Member;

static Json json_null  = { J_NULL,  0, NULL, NULL, NULL };
static Json json_false = { J_FALSE, 0, NULL, NULL, NULL };
static Json json_true  = { J_TRUE,  0, NULL, NULL, NULL };

static Json *make(Parser *P, int kind)
{
	Json *json = rt_alloc(P, sizeof(Json));

	memset(json, 0, sizeof(Json));
	json->kind = kind;

	return json;
}

static int hex(uint16_t unit)
{
	return unit <= '9' ? unit - '0' : (unit | 0x20) - 'a' + 10;
}

/* The token as it was read, quotation marks and all, to the text it stands for (section 7). */
static const uint16_t *unescaped(Parser *P, Val token, int32_t *length)
{
	const uint16_t *inside = P->text + token.from + 1;
	int32_t         count  = token.to - token.from - 2;
	uint16_t       *out    = rt_alloc(P, sizeof(uint16_t) * (size_t)(count + 1));
	int32_t         made   = 0;

	for (int32_t at = 0; at < count; at++)
	{
		uint16_t unit = inside[at];

		if (unit != '\\')
		{
			out[made++] = unit;
			continue;
		}

		uint16_t escaped = inside[++at];

		switch (escaped)
		{
			case 'b': out[made++] = '\b'; break;
			case 'f': out[made++] = '\f'; break;
			case 'n': out[made++] = '\n'; break;
			case 'r': out[made++] = '\r'; break;
			case 't': out[made++] = '\t'; break;

			case 'u':
				out[made++] = (uint16_t)(hex(inside[at + 1]) << 12 | hex(inside[at + 2]) << 8 | hex(inside[at + 3]) << 4 | hex(inside[at + 4]));
				at += 4;
				break;

			default:
				out[made++] = escaped;
				break;
		}
	}

	*length = made;

	return out;
}

static Val joined(Parser *P, Val first, Val rest)
{
	List *all = NULL;

	if (first.kind != V_NONE)
	{
		List *more = rest.ptr;

		all = list_add(P, NULL, first);

		for (int32_t i = 0; i < more->count; i++)
			all = list_add(P, all, more->items[i]);
	}

	return val_items(P, all);
}

Val host_c0(Parser *P) { (void)P; return val_host(&json_false); }
Val host_c1(Parser *P) { (void)P; return val_host(&json_null); }
Val host_c2(Parser *P) { (void)P; return val_host(&json_true); }

Val host_c3(Parser *P, Val text)
{
	Json *json = make(P, J_NUMBER);

	json->units = P->text + text.from;
	json->count = text.to - text.from;

	return val_host(json);
}

Val host_c4(Parser *P, Val text)
{
	Json *json = make(P, J_STRING);

	json->units = unescaped(P, text, &json->count);

	return val_host(json);
}

Val host_c5(Parser *P, Val members)
{
	Json *json = make(P, J_OBJECT);
	List *list = members.ptr;

	json->count   = list->count;
	json->members = rt_alloc(P, sizeof(Member *) * (size_t)(list->count + 1));

	for (int32_t i = 0; i < list->count; i++)
		json->members[i] = list->items[i].ptr;

	return val_host(json);
}

Val host_c6(Parser *P, Val first, Val rest)
{
	return joined(P, first, rest);
}

Val host_c7(Parser *P, Val name, Val value)
{
	Member *member = rt_alloc(P, sizeof(Member));

	member->name  = unescaped(P, name, &member->length);
	member->value = value.ptr;

	return val_host(member);
}

Val host_c8(Parser *P, Val items)
{
	Json *json = make(P, J_ARRAY);
	List *list = items.ptr;

	json->count = list->count;
	json->items = rt_alloc(P, sizeof(Json *) * (size_t)(list->count + 1));

	for (int32_t i = 0; i < list->count; i++)
		json->items[i] = list->items[i].ptr;

	return val_host(json);
}

Val host_c9(Parser *P, Val first, Val rest)
{
	return joined(P, first, rest);
}

static void unit_out(uint16_t unit)
{
	if (unit < 0x80)
		putchar(unit);
	else if (unit < 0x800)
	{
		putchar(0xc0 | unit >> 6);
		putchar(0x80 | (unit & 0x3f));
	}
	else
	{
		putchar(0xe0 | unit >> 12);
		putchar(0x80 | (unit >> 6 & 0x3f));
		putchar(0x80 | (unit & 0x3f));
	}
}

static void quoted(const uint16_t *units, int32_t length)
{
	putchar('"');

	for (int32_t i = 0; i < length; i++)
	{
		uint16_t unit = units[i];

		switch (unit)
		{
			case '"':  fputs("\\\"", stdout); break;
			case '\\': fputs("\\\\", stdout); break;
			case '\b': fputs("\\b", stdout);  break;
			case '\f': fputs("\\f", stdout);  break;
			case '\n': fputs("\\n", stdout);  break;
			case '\r': fputs("\\r", stdout);  break;
			case '\t': fputs("\\t", stdout);  break;

			default:
				if (unit < 0x20 || (unit >= 0xd800 && unit <= 0xdfff))
					printf("\\u%04x", unit);
				else
					unit_out(unit);

				break;
		}
	}

	putchar('"');
}

/* What is still to be written: a value, a text, or the name of a member with its colon. */
typedef struct Pending
{
	int         kind;
	const void *what;
} Pending;

static Pending *pending;
static size_t   pending_count, pending_room;

static void push(int kind, const void *what)
{
	if (pending_count == pending_room)
	{
		pending_room = pending_room == 0 ? 256 : pending_room * 2;
		pending      = realloc(pending, sizeof(Pending) * pending_room);
	}

	pending[pending_count++] = (Pending){ kind, what };
}

void host_begin(Parser *P, const char *answers, const char *throwOn)
{
	(void)P;
	(void)answers;
	(void)throwOn;
}

/* With a stack of its own: a text the parser read may nest deeper than a call stack goes. */
void host_render(Parser *P, Val value)
{
	(void)P;
	push(0, value.ptr);

	while (pending_count > 0)
	{
		Pending next = pending[--pending_count];

		if (next.kind == 1)
		{
			fputs(next.what, stdout);
			continue;
		}

		if (next.kind == 2)
		{
			const Member *member = next.what;

			quoted(member->name, member->length);
			putchar(':');
			continue;
		}

		const Json *json = next.what;

		switch (json->kind)
		{
			case J_OBJECT:
				putchar('{');
				push(1, "}");

				for (int32_t i = json->count - 1; i >= 0; i--)
				{
					push(0, json->members[i]->value);
					push(2, json->members[i]);

					if (i > 0)
						push(1, ",");
				}

				break;

			case J_ARRAY:
				putchar('[');
				push(1, "]");

				for (int32_t i = json->count - 1; i >= 0; i--)
				{
					push(0, json->items[i]);

					if (i > 0)
						push(1, ",");
				}

				break;

			case J_STRING:
				quoted(json->units, json->count);
				break;

			case J_NUMBER:
				for (int32_t i = 0; i < json->count; i++)
					putchar(json->units[i]);

				break;

			case J_TRUE:  fputs("true", stdout);  break;
			case J_FALSE: fputs("false", stdout); break;
			default:      fputs("null", stdout);  break;
		}
	}
}

void host_end(Parser *P)
{
	(void)P;
}
