/* Reads a batch of inputs with a printed parser and a host, and says of each what the C# side
   says of it: the verdict, the furthest position, what was built or what was expected, and the
   host calls where the host records them.

     driver <entry> <batch> [answers]

   A batch is a count, then for each input its length in UTF-16 code units and the units, all
   little-endian. IR_STACK_MB reads on a thread with a stack of that many megabytes; a stack
   that runs out is answered with "T stack" for the input being read, and the run ends there. */

#include <pthread.h>
#include <signal.h>
#include <stdio.h>
#include <unistd.h>

#include "runtime.h"
#include "parser.h"

static const char *entry;
static const char *answers;
static uint8_t    *batch;

static int compare(const void *one, const void *other)
{
	return *(const int32_t *)one - *(const int32_t *)other;
}

static void overflowed(int signal)
{
	static const char said[] = "T stack\n.\n";

	(void)signal;
	fflush(stdout);
	(void)!write(1, said, sizeof(said) - 1);
	_exit(0);
}

static void *run(void *unused)
{
	Parser   P     = { 0 };

	P.anyOwner   = getenv("IR_ANY_OWNER") != NULL;
	P.noCutCheck = getenv("IR_NO_CUT_CHECK") != NULL;

	int32_t  count = *(int32_t *)batch;
	uint8_t *at    = batch + 4;

	(void)unused;

	for (int32_t i = 0; i < count; i++)
	{
		int32_t length = *(int32_t *)at;

		at += 4;
		rt_begin(&P, (const uint16_t *)at, length);
		host_begin(&P, answers, NULL);
		at += (size_t)length * 2;

		int32_t end = parse(&P, entry);

		if (end >= 0)
		{
			printf("A %d ", end);
			host_render(&P, P.rv);
			printf("\n");
		}
		else
		{
			qsort(P.refused, (size_t)P.refusedCount, sizeof(int32_t), compare);
			printf("R %d ", P.refusedAt);

			for (int32_t j = 0; j < P.refusedCount; j++)
				printf(j == 0 ? "%d" : ",%d", P.refused[j]);

			printf("\n");
		}

		long long peak = (long long)rt_held(&P);

		rt_release(&P, 4096);

		if (getenv("IR_COUNTS") != NULL)
			printf("K %lld %lld %d %lld %lld %lld %lld %d\n", (long long)P.kSteps, (long long)P.kEntered, P.retries, (long long)P.kJumps, (long long)P.kHost, peak, (long long)rt_held(&P), P.restarts);

		/* A reading restarted by budget: the host calls made after it was given up, before the publication knew. */
		if (P.restarts > 0)
			printf("C %lld\n", (long long)P.callsAfterTrip);

		P.restarts = 0;

		host_end(&P);
		printf(".\n");
		fflush(stdout);
	}

	return NULL;
}

int main(int argc, char **argv)
{
	if (argc < 3)
		return 2;

	entry   = argv[1];
	answers = argc > 3 ? argv[3] : NULL;

	FILE *file = fopen(argv[2], "rb");

	if (file == NULL)
		return 2;

	fseek(file, 0, SEEK_END);

	long size = ftell(file);

	fseek(file, 0, SEEK_SET);
	batch = malloc((size_t)size + 2);

	if (fread(batch, 1, (size_t)size, file) != (size_t)size)
		return 2;

	fclose(file);

	/* A handler for a stack that ran out has to stand on a stack of its own. */
	stack_t          other  = { .ss_sp = malloc(1 << 16), .ss_size = 1 << 16 };
	struct sigaction action = { .sa_handler = overflowed, .sa_flags = SA_ONSTACK };

	sigaltstack(&other, NULL);
	sigaction(SIGSEGV, &action, NULL);

	const char *stack = getenv("IR_STACK_MB");

	if (stack != NULL)
	{
		pthread_attr_t attributes;
		pthread_t      thread;

		pthread_attr_init(&attributes);
		pthread_attr_setstacksize(&attributes, (size_t)atol(stack) << 20);
		pthread_create(&thread, &attributes, run, NULL);
		pthread_join(thread, NULL);
	}
	else
		run(NULL);

	return 0;
}
