using AdventOfCode2021.Data;

namespace AdventOfCode2021.Days
{
    internal sealed class Day15 : IDay
    {
        public string Solve()
        {
            var input = EmbeddedResource.ReadInput("InputDay15.txt").Split('\n').Select(x => x.Trim()).ToList();

            var part1Result = new CaveNavigator(input.ToArray(), true).FindLowestRiskPath();
            var part2Result = new CaveNavigator(input.ToArray(), false).FindLowestRiskPath();

            return $"Part 1: {part1Result}, Part 2: {part2Result}";
        }

        private class CaveNavigator
        {
            private int[,] Cave { get; set; }
            private Dictionary<Coordinate, int> LowestRisk { get; } = new();

            public CaveNavigator(string[] rows, bool part1)
            {
                if (part1)
                {
                    Cave = CreateCaveSegment(rows);
                }
                else
                {
                    Cave = ExpandCave(CreateCaveSegment(rows));
                }
            }

            private static int[,] CreateCaveSegment(string[] rows)
            {
                var caveSegment = new int[rows.Length, rows.First().Length];
                for (var r = 0; r < rows.Length; r++)
                {
                    var row = rows[r];
                    var columns = row.ToCharArray();
                    for (var c = 0; c < columns.Length; c++)
                    {
                        var value = columns[c];
                        caveSegment[r, c] = int.Parse(value.ToString());
                    }
                }
                return caveSegment;
            }

            private static int[,] ExpandCave(int[,] caveSegment)
            {
                var expandedCave = new int[caveSegment.GetLength(0) * 5, caveSegment.GetLength(1) * 5];
                for (var i = 0; i < 5; i++)
                {
                    for (var j = 0; j < 5; j++)
                    {
                        for (var r = 0; r < caveSegment.GetLength(0); r++)
                        {
                            for (var c = 0; c < caveSegment.GetLength(1); c++)
                            {
                                var value = (caveSegment[r, c] + j + i - 1) % 9 + 1;
                                expandedCave[r + (caveSegment.GetLength(0) * i), c + (caveSegment.GetLength(1) * j)] = value;
                            }
                        }
                    }
                }
                return expandedCave;
            }

            public int FindLowestRiskPath()
            {
                var dummyPoint = new Coordinate(-1, -1);
                LowestRisk.Add(dummyPoint, -Cave[0, 0]);

                var startPoint = new Coordinate(0, 0);
                var endPoint = new Coordinate(Cave.GetLength(0) - 1, Cave.GetLength(1) - 1);

                Queue<(Coordinate current, Coordinate previous)> queue = new();
                queue.Enqueue((startPoint, dummyPoint));

                while (queue.Count > 0)
                {
                    (var current, var previous) = queue.Dequeue();

                    var risk = LowestRisk[previous] + Cave[current.R, current.C];

                    if (LowestRisk.ContainsKey(current) && risk >= LowestRisk[current])
                    {
                        continue;
                    }

                    LowestRisk[current] = risk;

                    if (current == endPoint)
                    {
                        continue;
                    }

                    foreach (var adjacent in current.GetAdjacentCoordinates(Cave.GetLength(0), Cave.GetLength(1)))
                    {
                        queue.Enqueue((adjacent, current));
                    }
                }

                return LowestRisk[endPoint];
            }
        }

        private record Coordinate(int R, int C)
        {
            public IEnumerable<Coordinate> GetAdjacentCoordinates(int rowBound, int colBound)
            {
                if (R + 1 < rowBound) yield return new(R + 1, C);
                if (C + 1 < colBound) yield return new(R, C + 1);
                if (R - 1 > 0) yield return new(R - 1, C);
                if (C - 1 > 0) yield return new(R, C - 1);
            }
        };
    }
}
