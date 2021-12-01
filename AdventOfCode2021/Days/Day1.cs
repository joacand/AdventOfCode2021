using AdventOfCode2021.Data;

namespace AdventOfCode2021.Days
{
    internal sealed class Day1 : IDay
    {
        public string Solve()
        {
            var depths = EmbeddedResource.ReadInput("InputDay1.txt")
                .Split('\n').Select(x => int.Parse(x)).ToArray();

            var increases = CountDepthsIncreases(depths, 1).ToString();

            var threeMeasurementSlidingIncreases = CountDepthsIncreases(depths, 3).ToString();

            return $"Part 1: {increases}, Part 2: {threeMeasurementSlidingIncreases}";
        }

        private static int CountDepthsIncreases(int[] depths, int windowSize)
        {
            var increases = 0;
            for (int i = 0; i + windowSize < depths.Length; i++)
            {
                var depthA = 0;
                var depthB = 0;
                for (int j = 0; j < windowSize; j++)
                {
                    depthA += depths[i + j];
                    depthB += depths[i + j + 1];
                }
                if (depthB > depthA)
                {
                    increases++;
                }
            }
            return increases;
        }
    }
}
