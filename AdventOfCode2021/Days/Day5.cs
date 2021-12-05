using AdventOfCode2021.Data;

namespace AdventOfCode2021.Days
{
    internal sealed class Day5 : IDay
    {
        private static int MapSize => 1000;

        public string Solve()
        {
            var lines = EmbeddedResource.ReadInput("InputDay5.txt").Split('\n').Select(x => x.Trim());

            var coordinates = ParseCoordinates(lines);

            var part1Result = Solve(new int[MapSize, MapSize], coordinates, false);
            var part2Result = Solve(new int[MapSize, MapSize], coordinates, true);

            return $"Part 1: {part1Result}, Part 2: {part2Result}";
        }

        private static int Solve(int[,] map, IEnumerable<Coordinate> coordinates, bool includeDiagonals)
        {
            foreach (var c in coordinates.Where(c => includeDiagonals || !c.IsDiagonal))
            {
                if (c.IsDiagonal)
                {
                    HandleDiagonalCase(c, map);
                }
                else
                {
                    HandleNonDiagonalCase(c, map);
                }
            }

            return CountOverlaps(map);
        }

        private static void HandleDiagonalCase(Coordinate c, int[,] map)
        {
            for (int ix = 0, iy = 0; ix < c.Xdistance; ix++, iy++)
            {
                UpdateMap(c, ix, iy, map);
            }
        }

        private static void HandleNonDiagonalCase(Coordinate c, int[,] map)
        {
            for (int ix = 0; ix < c.Xdistance; ix++)
            {
                for (int iy = 0; iy < c.Ydistance; iy++)
                {
                    UpdateMap(c, ix, iy, map);
                }
            }
        }

        private static void UpdateMap(Coordinate c, int ix, int iy, int[,] map)
        {
            var xPos = c.X1 < c.X2 ? c.X1 + ix : c.X1 - ix;
            var yPos = c.Y1 < c.Y2 ? c.Y1 + iy : c.Y1 - iy;
            map[xPos, yPos] += 1;
        }

        private static int CountOverlaps(int[,] map)
        {
            var overlaps = 0;
            for (var r = 0; r < map.GetLength(0); ++r)
            {
                for (var c = 0; c < map.GetLength(1); ++c)
                {
                    if (map[r, c] > 1) overlaps++;
                }
            }
            return overlaps;
        }

        private static List<Coordinate> ParseCoordinates(IEnumerable<string> lines)
        {
            var result = new List<Coordinate>();
            foreach (var line in lines)
            {
                var leftRight = line.Split(" -> ");
                var left = leftRight[0];
                var right = leftRight[1];

                var leftSplit = left.Split(',');
                var x1 = int.Parse(leftSplit[0]);
                var y1 = int.Parse(leftSplit[1]);

                var rightSplit = right.Split(',');
                var x2 = int.Parse(rightSplit[0]);
                var y2 = int.Parse(rightSplit[1]);

                result.Add(new(x1, y1, x2, y2));
            }
            return result;
        }

        private record Coordinate(int X1, int Y1, int X2, int Y2)
        {
            public bool IsDiagonal => X1 != X2 && Y1 != Y2;
            public int Xdistance => Math.Abs(X1 - X2) + 1;
            public int Ydistance => Math.Abs(Y1 - Y2) + 1;
        };
    }
}
