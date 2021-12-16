using AdventOfCode2021.Data;

namespace AdventOfCode2021.Days
{
    internal sealed class Day12 : IDay
    {
        public string Solve()
        {
            var cavesInput = EmbeddedResource.ReadInput("InputDay12.txt").Split('\n').Select(x => x.Trim()).ToList();

            var part1Result = new CaveParser(cavesInput).StartCave.NavigateAllConnections(new(), true, "", new());
            var part2Result = new CaveParser(cavesInput).StartCave.NavigateAllConnections(new(), false, "", new());

            return $"Part 1: {part1Result}, Part 2: {part2Result}";
        }

        private class Cave
        {
            public string CaveId { get; set; }
            public CaveSize Size { get; set; }
            public List<Cave> Connections { get; set; } = new();

            public Cave(string id, CaveSize size)
            {
                CaveId = id;
                Size = size;
            }

            public int NavigateAllConnections(List<Cave> VisitedSmallCaves, bool secondVisitExecuted, string currentPath, List<string> completedPaths)
            {
                currentPath += $",{CaveId}";

                if (CaveId.Equals("end"))
                {
                    if (completedPaths.Contains(currentPath)) { return 0; }
                    completedPaths.Add(currentPath);
                    return 1;
                }

                if (Size == CaveSize.Small)
                {
                    VisitedSmallCaves.Add(this);
                }

                return
                    // Visit without doing a secondary small cave visit
                    Connections
                        .Where(c => !VisitedSmallCaves.Contains(c))
                        .Sum(x => x.NavigateAllConnections(new List<Cave>(VisitedSmallCaves), secondVisitExecuted, currentPath, completedPaths))

                    // Visit with a secondary small cave visit, unless already executed this path
                    + (secondVisitExecuted
                        ? 0
                        : Connections
                            .Where(c => !c.CaveId.Equals("start"))
                            .Sum(x => x.NavigateAllConnections(new List<Cave>(VisitedSmallCaves), true, currentPath, completedPaths))
                    );
            }

            public override bool Equals(object obj)
            {
                return obj is Cave cave &&
                       CaveId.Equals(cave.CaveId);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(CaveId);
            }
        }

        private class CaveParser
        {
            public Cave StartCave { get; set; }
            private List<Cave> Caves { get; set; } = new();

            public CaveParser(List<string> input)
            {
                foreach (var line in input)
                {
                    var split = line.Split('-');
                    var firstCave = new Cave(split[0], CaveToSize(split[0]));
                    var secondCave = new Cave(split[1], CaveToSize(split[1]));

                    if (!Caves.Contains(firstCave))
                    {
                        Caves.Add(firstCave);
                    }
                    firstCave = Caves.First(x => x.CaveId == firstCave.CaveId);

                    if (!Caves.Contains(secondCave))
                    {
                        Caves.Add(secondCave);
                    }
                    secondCave = Caves.First(x => x.CaveId == secondCave.CaveId);

                    firstCave.Connections.Add(secondCave);
                    secondCave.Connections.Add(firstCave);
                }

                StartCave = Caves.First(x => x.CaveId.Equals("start"));
            }

            private static CaveSize CaveToSize(string cave)
            {
                return cave.All(x => char.IsUpper(x))
                    ? CaveSize.Big
                    : CaveSize.Small;
            }
        }

        private enum CaveSize
        {
            Small, Big
        }
    }
}
