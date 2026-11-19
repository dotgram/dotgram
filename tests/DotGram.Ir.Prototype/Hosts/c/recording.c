/* The recording host: every construction builds the text that says which fragment it was and
   over what, every call is written down in the order it ran, and a guard answers what the C#
   parser's guard answered over the same arguments. The functions the parser calls by name are
   printed beside the parser, a line each, and come here with their number. */

#include <stdio.h>

#include "runtime.h"

typedef struct Text
{
	char  *at;
	size_t length, room;
} Text;

static char **log_lines;
static int    log_count, log_room;

static char **answer_lines;
static int    answer_count;

static void put(Text *text, const char *more, size_t length)
{
	if (text->length + length + 1 > text->room)
	{
		text->room = (text->length + length + 1) * 2;
		text->at   = realloc(text->at, text->room);
	}

	memcpy(text->at + text->length, more, length);
	text->length += length;
	text->at[text->length] = 0;
}

static void puts_(Text *text, const char *more)
{
	put(text, more, strlen(more));
}

/* One spelling for every reading: a text in quotes, nothing as null, a sequence in brackets. */
static void canon(Parser *P, Text *text, Val value)
{
	char spelled[8];

	switch (value.kind)
	{
		case V_NONE:
			puts_(text, "null");
			break;

		case V_HOST:
			puts_(text, value.ptr);
			break;

		case V_TEXT:
			puts_(text, "\"");

			for (int32_t i = value.from; i < value.to; i++)
			{
				uint16_t unit = P->text[i];

				if (unit == '"' || unit == '\\' || unit < 0x20 || unit > 0x7e)
				{
					snprintf(spelled, sizeof(spelled), "\\u%04x", unit);
					puts_(text, spelled);
				}
				else
				{
					spelled[0] = (char)unit;
					put(text, spelled, 1);
				}
			}

			puts_(text, "\"");
			break;

		case V_LIST:
		{
			List *list = value.ptr;

			puts_(text, "[");

			for (int32_t i = 0; i < list->count; i++)
			{
				if (i > 0)
					puts_(text, ",");

				canon(P, text, list->items[i]);
			}

			puts_(text, "]");
			break;
		}
	}
}

static Text call(Parser *P, char kind, int id, Val *args, int count)
{
	Text text = { 0 };
	char name[16];

	snprintf(name, sizeof(name), "%c%d(", kind, id);
	puts_(&text, name);

	for (int i = 0; i < count; i++)
	{
		if (i > 0)
			puts_(&text, ",");

		canon(P, &text, args[i]);
	}

	puts_(&text, ")");

	return text;
}

static void record(char *line)
{
	if (log_count == log_room)
	{
		log_room  = log_room == 0 ? 64 : log_room * 2;
		log_lines = realloc(log_lines, sizeof(char *) * (size_t)log_room);
	}

	log_lines[log_count++] = line;
}

Val recording_construct(Parser *P, int id, Val *args, int count)
{
	Text  text  = call(P, 'c', id, args, count);
	char *built = rt_alloc(P, text.length + 1);

	memcpy(built, text.at, text.length + 1);
	record(text.at);

	return val_host(built);
}

bool recording_guard(Parser *P, int id, Val *args, int count)
{
	Text text = call(P, 'g', id, args, count);

	for (int i = 0; i < answer_count; i++)
	{
		size_t length = strlen(answer_lines[i]);

		if (length > text.length && strncmp(answer_lines[i], text.at, text.length) == 0 && answer_lines[i][text.length] == '=')
		{
			bool answer = strcmp(answer_lines[i] + text.length + 1, "true") == 0;

			puts_(&text, answer ? "=true" : "=false");
			record(text.at);

			return answer;
		}
	}

	fprintf(stderr, "No answer recorded for %s\n", text.at);
	exit(4);
}

void host_begin(Parser *P, const char *answers, const char *throwOn)
{
	(void)P;
	(void)throwOn;

	if (answers != NULL && answer_lines == NULL)
	{
		FILE *file = fopen(answers, "rb");
		char *line = NULL;
		size_t room = 0;

		answer_lines = malloc(sizeof(char *) * 65536);

		while (file != NULL && answer_count < 65536 && getline(&line, &room, file) > 0)
		{
			line[strcspn(line, "\r\n")] = 0;
			answer_lines[answer_count++] = strdup(line);
		}
	}
}

void host_render(Parser *P, Val value)
{
	Text text = { 0 };

	canon(P, &text, value);
	fputs(text.at, stdout);
	free(text.at);
}

void host_end(Parser *P)
{
	(void)P;

	for (int i = 0; i < log_count; i++)
	{
		printf("L %s\n", log_lines[i]);
		free(log_lines[i]);
	}

	log_count = 0;
}
