using AdventOfCode2021.Data;

namespace AdventOfCode2021.Days
{
    internal sealed class Day11 : IDay
    {
        public string Solve()
        {
            var mapInput = EmbeddedResource.ReadInput("InputDay11.txt").Split('\n').Select(x => x.Trim()).ToArray();

            var part1Result = ExecuteSteps(new OctopusSwarm(mapInput), 100);
            var part2Result = FindFirstSynchronizedFlash(new OctopusSwarm(mapInput));

            return $"Part 1: {part1Result}, Part 2: {part2Result}";
        }

        private static int ExecuteSteps(OctopusSwarm octopusSwarm, int steps)
        {
            for (var i = 0; i < steps; i++)
            {
                octopusSwarm.FlashStep();
            }
            return octopusSwarm.TotalFlashes;
        }

        private static int FindFirstSynchronizedFlash(OctopusSwarm octopusSwarm)
        {
            return octopusSwarm.StepsUntilSynchronizedFlash();
        }

        private class OctopusSwarm
        {
            private int[,] OctopusEnergyLevels { get; init; }

            public int TotalFlashes { get; set; } = 0;

            public OctopusSwarm(string[] rows)
            {
                OctopusEnergyLevels = new int[rows.Length, rows.First().Length];

                for (var r = 0; r < rows.Length; r++)
                {
                    var row = rows[r];
                    var columns = row.ToCharArray();
                    for (var c = 0; c < columns.Length; c++)
                    {
                        var value = columns[c];
                        OctopusEnergyLevels[r, c] = int.Parse(value.ToString());
                    }
                }
            }

            public int StepsUntilSynchronizedFlash()
            {
                var flashesInStep = 0;
                var steps = 0;
                while (flashesInStep != OctopusEnergyLevels.Length)
                {
                    flashesInStep = FlashStep();
                    steps++;
                }
                return steps;
            }

            public int FlashStep()
            {
                var hasFlashedThisStep = new List<Coordinate>();

                List<Coordinate> flashingOctopuses = new();
                for (var r = 0; r < OctopusEnergyLevels.GetLength(0); ++r)
                {
                    for (var c = 0; c < OctopusEnergyLevels.GetLength(1); ++c)
                    {
                        var coord = new Coordinate(r, c);

                        var flashed = IncreaseEnergy(coord);

                        if (flashed)
                        {
                            flashingOctopuses.Add(new(r, c));
                        }
                    }
                }

                foreach (var flashingOctopus in flashingOctopuses
                    .Where(x => !hasFlashedThisStep.Contains(x) &&
                    !x.OutOfBounds(OctopusEnergyLevels.GetLength(0), OctopusEnergyLevels.GetLength(1))))
                {
                    FlashOctopus(flashingOctopus, hasFlashedThisStep);
                }

                return hasFlashedThisStep.Count;
            }

            private void FlashOctopus(Coordinate coord, List<Coordinate> hasFlashedThisStep)
            {
                TotalFlashes++;
                OctopusEnergyLevels[coord.R, coord.C] = 0;
                hasFlashedThisStep.Add(coord);

                foreach (var adjacentOctopus in coord.AdjacentCoordinates()
                    .Where(x => !hasFlashedThisStep.Contains(x) &&
                    !x.OutOfBounds(OctopusEnergyLevels.GetLength(0), OctopusEnergyLevels.GetLength(1))))
                {
                    if (IncreaseEnergy(adjacentOctopus))
                    {
                        FlashOctopus(adjacentOctopus, hasFlashedThisStep);
                    }
                }
            }

            private bool IncreaseEnergy(Coordinate c)
            {
                OctopusEnergyLevels[c.R, c.C] += 1;

                if (OctopusEnergyLevels[c.R, c.C] > 9)
                {
                    return true;
                }
                return false;
            }
        }

        private record Coordinate(int R, int C)
        {
            public List<Coordinate> AdjacentCoordinates()
            {
                return new()
                {
                    new(R + 1, C),
                    new(R, C - 1),
                    new(R, C + 1),
                    new(R - 1, C),

                    // Diagonals
                    new(R - 1, C - 1),
                    new(R - 1, C + 1),
                    new(R + 1, C + 1),
                    new(R + 1, C - 1)
                };
            }

            public bool OutOfBounds(int rows, int columns)
            {
                return R < 0 || R >= rows || C < 0 || C >= columns;
            }
        };
    }
}
