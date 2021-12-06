using AdventOfCode2021.Data;

namespace AdventOfCode2021.Days
{
    internal sealed class Day6 : IDay
    {
        public string Solve()
        {
            var initialFish = EmbeddedResource.ReadInput("InputDay6.txt").Split(',').Select(x => int.Parse(x.Trim())).ToArray();

            var part1Result = Solve(80, initialFish);
            var part2Result = Solve(256, initialFish);

            return $"Part 1: {part1Result}, Part 2: {part2Result}";
        }

        private static long Solve(int days, int[] initialFish)
        {
            var fishBuckets = new FishBuckets(initialFish);
            for (var i = 0; i <= days; i++)
            {
                fishBuckets.PassDay();
            }
            return fishBuckets.TotalFish();
        }

        private class FishBuckets
        {
            private Dictionary<int, long> Buckets { get; } = new();
            private int DayCounter { get; set; }
            private int BucketCount { get; } = 7;

            // Lazy solution for the new fishes having "two extra days"
            private long warmupBucket1;
            private long warmupBucket2;

            public FishBuckets(IEnumerable<int> initialFish)
            {
                for (var i = 0; i < BucketCount; i++)
                {
                    Buckets[i] = 0;
                }
                foreach (var f in initialFish)
                {
                    Buckets[f] += 1;
                }
            }

            public void PassDay()
            {
                DayCounter = (DayCounter + 1) % BucketCount;
                CreateFishes();
            }

            public long TotalFish()
            {
                long totalFish = 0;
                for (var i = 0; i < BucketCount; i++)
                {
                    totalFish += Buckets[i];
                }
                return totalFish;
            }

            private void CreateFishes()
            {
                var newFishes = warmupBucket1;
                warmupBucket1 = warmupBucket2;
                warmupBucket2 = Buckets[DayCounter];

                Buckets[DayCounter] += newFishes;
            }
        }
    }
}
