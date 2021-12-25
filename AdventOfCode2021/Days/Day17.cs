using AdventOfCode2021.Data;

namespace AdventOfCode2021.Days
{
    internal sealed class Day17 : IDay
    {
        public string Solve()
        {
            var input = EmbeddedResource.ReadInput($"Input{GetType().Name}.txt");

            var probeLauncer = new ProbeLauncher(input);

            var (maxHeight, successfulInitialVelocities) = probeLauncer.FindMaxHeightAndTotalVelocities();

            var part1Result = maxHeight;
            var part2Result = successfulInitialVelocities;

            return $"Part 1: {part1Result}, Part 2: {part2Result}";
        }

        private class ProbeLauncher
        {
            private int XStart { get; }
            private int XEnd { get; }
            private int YStart { get; }
            private int YEnd { get; }

            private HashSet<(int, int)> SuccessulInitialVelocities { get; } = new();

            public ProbeLauncher(string input)
            {
                var xySplit = new string(input.Skip(13).ToArray())
                    .Split(", ").Select(x => new string(x.Skip(2).ToArray())).ToArray();

                var x = xySplit[0].Split("..");
                XStart = int.Parse(x[0]);
                XEnd = int.Parse(x[1]);

                var y = xySplit[1].Split("..");
                YStart = int.Parse(y[0]);
                YEnd = int.Parse(y[1]);
            }

            public (int maxHeight, int successfulInitialVelocities) FindMaxHeightAndTotalVelocities()
            {
                var maxHeight = int.MinValue;

                for (var y = YStart; y <= XEnd * 2; y++)
                {
                    for (var x = -XEnd; x <= XEnd; x++)
                    {
                        var probe = new Probe(new(x, y));

                        var targetHit = LaunchProbe(probe);

                        if (targetHit)
                        {
                            SuccessulInitialVelocities.Add((x, y));

                            if (probe.MaxY > maxHeight)
                            {
                                maxHeight = probe.MaxY;
                            }
                        }
                    }
                }

                return (maxHeight, SuccessulInitialVelocities.Count);
            }

            private bool LaunchProbe(Probe probe)
            {
                while (!OvershotTarget(probe))
                {
                    probe.Step();

                    if (WithinTarget(probe))
                    {
                        return true;
                    }
                }
                return false;
            }

            private bool WithinTarget(Probe probe)
            {
                return
                    probe.Coordinate.Y >= YStart &&
                    probe.Coordinate.Y <= YEnd &&
                    probe.Coordinate.X >= XStart &&
                    probe.Coordinate.X <= XEnd;
            }

            private bool OvershotTarget(Probe probe)
            {
                return probe.Coordinate.Y < YStart || probe.Coordinate.X > XEnd;
            }
        }

        private class Probe
        {
            private HashSet<int> PastYValues { get; set; } = new() { 0 };

            public int MaxY => PastYValues.Max();
            public Coordinate Coordinate { get; set; }
            public Coordinate Velocity { get; set; }

            public Probe(Coordinate initialVelocity)
            {
                Coordinate = new Coordinate(0, 0);
                Velocity = initialVelocity;
            }

            public void Step()
            {
                Coordinate.X += Velocity.X;
                Coordinate.Y += Velocity.Y;

                PastYValues.Add(Coordinate.Y);

                Velocity.Y -= 1;

                if (Velocity.X != 0)
                {
                    Velocity.X = Velocity.X > 0
                        ? Velocity.X -= 1
                        : Velocity.X += 1;
                }
            }
        }

        private class Coordinate
        {
            public int X { get; set; }
            public int Y { get; set; }

            public Coordinate(int x, int y)
            {
                X = x;
                Y = y;
            }
        };
    }
}
