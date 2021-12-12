using AdventOfCode2021.Data;

namespace AdventOfCode2021.Days
{
    internal sealed class Day9 : IDay
    {
        public string Solve()
        {
            var mapInput = EmbeddedResource.ReadInput("InputDay9.txt").Split('\n').Select(x => x.Trim()).ToArray();

            var cave = new Cave(mapInput);

            var part1Result = cave.FindLowPointRiskLevels();
            var part2Result = cave.FindThreeLargestBasinSizesProduct();

            return $"Part 1: {part1Result}, Part 2: {part2Result}";
        }

        private class Cave
        {
            private int[,] Map { get; init; }

            public Cave(string[] rows)
            {
                Map = new int[rows.Length, rows.First().Length];

                for (var r = 0; r < rows.Length; r++)
                {
                    var row = rows[r];
                    var columns = row.ToCharArray();
                    for (var c = 0; c < columns.Length; c++)
                    {
                        var value = columns[c];
                        Map[r, c] = int.Parse(value.ToString());
                    }
                }
            }

            public int FindLowPointRiskLevels()
            {
                return FindLowPoints().Sum(x => Map[x.R, x.C] + 1);
            }

            public int FindThreeLargestBasinSizesProduct()
            {
                return FindBasinSizes(FindLowPoints())
                    .OrderByDescending(x => x)
                    .Take(3)
                    .Aggregate(1, (acc, val) => acc * val);
            }

            private IEnumerable<int> FindBasinSizes(IEnumerable<Coordinate> lowPoints)
            {
                return lowPoints.Select(coord => Crawl(coord, new List<Coordinate>()));
            }

            private int Crawl(Coordinate coord, IList<Coordinate> crawledCoords)
            {
                if (coord.OutOfBounds(Map.GetLength(0), Map.GetLength(1)) ||
                    Map[coord.R, coord.C] == 9)
                {
                    return 0;
                }

                crawledCoords.Add(coord);

                return 1 + coord.AdjacentCoordinates()
                    .Where(x => !crawledCoords.Contains(x))
                    .Sum(x => Crawl(x, crawledCoords));
            }

            private IEnumerable<Coordinate> FindLowPoints()
            {
                for (var r = 0; r < Map.GetLength(0); ++r)
                {
                    for (var c = 0; c < Map.GetLength(1); ++c)
                    {
                        var coord = new Coordinate(r, c);

                        if (IsLowPoint(coord))
                        {
                            yield return coord;
                        }
                    }
                }
            }

            private bool IsLowPoint(Coordinate coord)
            {
                return coord.AdjacentCoordinates().All(x => IsHigher(x, Map[coord.R, coord.C]));
            }

            private bool IsHigher(Coordinate coord, int value)
            {
                return coord.OutOfBounds(Map.GetLength(0), Map.GetLength(1)) || Map[coord.R, coord.C] > value;
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
                    new(R - 1, C)
                };
            }

            public bool OutOfBounds(int rows, int columns)
            {
                return R < 0 || R >= rows || C < 0 || C >= columns;
            }
        };
    }
}
