/* What resuming costs a reading that is accepted, in C: one parser printed with or without its
   sentinels, compiled on its own, reads a batch of inputs again and again with a host that builds
   nothing (every guard says yes), and says how long a reading of the batch took. The caller runs
   the variants in turns and compares.

     accept <entry> <batch> <repetitions>

   Prints the number of inputs accepted, the milliseconds of the repetitions and the readings given
   up in one pass (a parser that restarts by budget), one line. */

#include <stdio.h>
#include <time.h>

#include "runtime.h"
#include "parser.h"

int main(int argc, char **argv)
{
	if (argc < 4)
		return 2;

	FILE *file = fopen(argv[2], "rb");

	if (file == NULL)
		return 2;

	fseek(file, 0, SEEK_END);

	long size = ftell(file);

	fseek(file, 0, SEEK_SET);

	uint8_t *batch = malloc((size_t)size + 2);

	if (fread(batch, 1, (size_t)size, file) != (size_t)size)
		return 2;

	fclose(file);

	int32_t count = *(int32_t *)batch;
	long    reps  = atol(argv[3]);
	Parser  P     = { 0 };
	int     accepted = 0;

	struct timespec from, to;

	clock_gettime(CLOCK_MONOTONIC, &from);

	for (long r = 0; r < reps; r++)
	{
		uint8_t *at = batch + 4;

		accepted = 0;

		for (int32_t i = 0; i < count; i++)
		{
			int32_t length = *(int32_t *)at;

			at += 4;
			rt_begin(&P, (const uint16_t *)at, length);
			at += (size_t)length * 2;

			if (parse(&P, argv[1]) >= 0)
				accepted++;
		}
	}

	clock_gettime(CLOCK_MONOTONIC, &to);

	/* And the readings a parser that restarts by budget gave up, in one pass of the batch. */
	printf("%d %.4f %d\n", accepted, (double)(to.tv_sec - from.tv_sec) * 1e3 + (double)(to.tv_nsec - from.tv_nsec) / 1e6, reps > 0 ? (int)(P.restarts / reps) : 0);

	return 0;
}
