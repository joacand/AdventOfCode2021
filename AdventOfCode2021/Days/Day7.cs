using AdventOfCode2021.Data;

namespace AdventOfCode2021.Days
{
    internal sealed class Day7 : IDay
    {
        public string Solve()
        {
            var crabPositions = EmbeddedResource.ReadInput("InputDay7.txt").Split(',').Select(x => int.Parse(x.Trim()));

            var part1Result = FindMinFuel(crabPositions, true);
            var part2Result = FindMinFuel(crabPositions, false);

            return $"Part 1: {part1Result}, Part 2: {part2Result}";
        }

        private static int FindMinFuel(IEnumerable<int> crabPositions, bool part1)
        {
            var minFuel = int.MaxValue;

            for (var position = crabPositions.Min(); position <= crabPositions.Max(); position++)
            {
                var fuelCost = part1
                    ? crabPositions.Sum(crabPosition => Math.Abs(crabPosition - position))
                    : crabPositions.Sum(crabPosition => Math.Abs(crabPosition - position) * (Math.Abs(crabPosition - position) + 1) / 2);

                minFuel = fuelCost < minFuel ? fuelCost : minFuel;
            }

            return minFuel;
        }
    }
}
