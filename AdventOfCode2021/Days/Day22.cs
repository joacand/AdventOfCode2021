using AdventOfCode2021.Data;

namespace AdventOfCode2021.Days
{
    internal sealed class Day22 : IDay
    {
        public string Solve()
        {
            var cubeInstructions = EmbeddedResource.ReadInput($"Input{GetType().Name}.txt")
                .Split("\r\n")
                .Select(x => x.Trim())
                .ToArray();

            var reactorHandler = new ReactorHandler(cubeInstructions);

            var part1Result = reactorHandler.ExecuteInitializeProcedure(50);
            var part2Result = reactorHandler.ExecuteRebootProcedure();

            return $"Part 1: {part1Result}, Part 2: {part2Result}";
        }

        private class ReactorHandler
        {
            private List<Instruction> Instructions { get; }

            public ReactorHandler(string[] instructions)
            {
                Instructions = instructions.Select(x => new Instruction(x)).ToList();
            }

            public int ExecuteInitializeProcedure(int coordinateLimit)
            {
                return new InitializeProcedure().ExecuteInitializeProcedure(Instructions, coordinateLimit);
            }

            public long ExecuteRebootProcedure()
            {
                return new RebootProcedure().ExecuteRebootProcedure(Instructions);
            }
        }

        private class InitializeProcedure
        {
            private HashSet<Coordinate> CubesOnline { get; } = new();

            public int ExecuteInitializeProcedure(IEnumerable<Instruction> instructions, int coordinateLimit)
            {
                CubesOnline.Clear();
                foreach (var instruction in instructions)
                {
                    SwitchCubesWithLimit(instruction, coordinateLimit);
                }
                return CubesOnline.Count;
            }

            private void SwitchCubesWithLimit(Instruction instruction, int coordinateLimit)
            {
                for (var x = instruction.StartWithLimit(coordinateLimit).X; x <= instruction.EndWithLimit(coordinateLimit).X; x++)
                    for (var y = instruction.StartWithLimit(coordinateLimit).Y; y <= instruction.EndWithLimit(coordinateLimit).Y; y++)
                        for (var z = instruction.StartWithLimit(coordinateLimit).Z; z <= instruction.EndWithLimit(coordinateLimit).Z; z++)
                        {
                            if (instruction.On)
                            {
                                CubesOnline.Add(new(x, y, z));
                            }
                            else
                            {
                                CubesOnline.Remove(new(x, y, z));
                            }
                        }
            }
        }

        private class RebootProcedure
        {
            private List<Cube> CubesToAdd { get; } = new();
            private List<Cube> CubesToSubtract { get; } = new();

            public long ExecuteRebootProcedure(IEnumerable<Instruction> instructions)
            {
                foreach (var instruction in instructions)
                {
                    CollectCuboidOperations(instruction);
                }
                return CalculateOnlineCubes();
            }

            private void CollectCuboidOperations(Instruction instruction)
            {
                AddOverlaps(instruction.Cube);

                if (instruction.On)
                {
                    CubesToAdd.Add(instruction.Cube);
                }
            }

            private void AddOverlaps(Cube newCube)
            {
                var addCubes = new List<Cube>();
                var subtractCubes = new List<Cube>();

                foreach (var cube in CubesToAdd)
                {
                    if (newCube.Overlap(cube, out var overlapSpan))
                    {
                        subtractCubes.Add(overlapSpan);
                    }
                }
                foreach (var cube in CubesToSubtract)
                {
                    if (newCube.Overlap(cube, out var overlapSpan))
                    {
                        addCubes.Add(overlapSpan);
                    }
                }

                CubesToAdd.AddRange(addCubes);
                CubesToSubtract.AddRange(subtractCubes);
            }

            private long CalculateOnlineCubes()
            {
                var onlineCubes = 0L;
                foreach (var cube in CubesToAdd)
                {
                    onlineCubes += (long)
                        Math.Abs(cube.Start.X - cube.End.X) *
                        Math.Abs(cube.Start.Y - cube.End.Y) *
                        Math.Abs(cube.Start.Z - cube.End.Z);
                }
                foreach (var cube in CubesToSubtract)
                {
                    onlineCubes -= (long)
                        Math.Abs(cube.Start.X - cube.End.X) *
                        Math.Abs(cube.Start.Y - cube.End.Y) *
                        Math.Abs(cube.Start.Z - cube.End.Z);
                }
                return onlineCubes;
            }
        }

        private class Instruction
        {
            public bool On { get; set; }
            public Cube Cube { get; set; }

            public Coordinate StartWithLimit(int limit)
            {
                return new Coordinate(
                    Cube.Start.X < -limit ? -limit : Cube.Start.X,
                    Cube.Start.Y < -limit ? -limit : Cube.Start.Y,
                    Cube.Start.Z < -limit ? -limit : Cube.Start.Z);
            }

            public Coordinate EndWithLimit(int limit)
            {
                return new Coordinate(
                    Cube.End.X > limit ? limit - 1 : Cube.End.X - 1,
                    Cube.End.Y > limit ? limit - 1 : Cube.End.Y - 1,
                    Cube.End.Z > limit ? limit - 1 : Cube.End.Z - 1);
            }

            public Instruction(string instruction)
            {
                var split = instruction.Split(' ');
                On = split[0].Trim().Equals("on", StringComparison.OrdinalIgnoreCase);

                var coordinateSplit = split[1].Split(',');
                var xSplit = coordinateSplit[0].Split('=')[1].Split("..").Select(int.Parse).ToArray();
                var ySplit = coordinateSplit[1].Split('=')[1].Split("..").Select(int.Parse).ToArray();
                var zSplit = coordinateSplit[2].Split('=')[1].Split("..").Select(int.Parse).ToArray();

                Cube = new(
                    new(xSplit[0], ySplit[0], zSplit[0]),
                    new(xSplit[1] + 1, ySplit[1] + 1, zSplit[1] + 1));
            }
        }

        private record Cube(Coordinate Start, Coordinate End)
        {
            public bool Overlap(Cube otherCube, out Cube overlappingCube)
            {
                overlappingCube = new(new(0, 0, 0), new(0, 0, 0));

                var overlapX = End.X > otherCube.Start.X && Start.X < otherCube.End.X;
                var overlapY = End.Y > otherCube.Start.Y && Start.Y < otherCube.End.Y;
                var overlapZ = End.Z > otherCube.Start.Z && Start.Z < otherCube.End.Z;

                if (overlapX && overlapY && overlapZ)
                {
                    overlappingCube = GetOverlappingCube(otherCube);
                    return true;
                }
                return false;
            }

            private Cube GetOverlappingCube(Cube otherCube)
            {
                var (xStart, xEnd) = GetOverlappingCoordinate(Start.X, End.X, otherCube.Start.X, otherCube.End.X);
                var (yStart, yEnd) = GetOverlappingCoordinate(Start.Y, End.Y, otherCube.Start.Y, otherCube.End.Y);
                var (zStart, zEnd) = GetOverlappingCoordinate(Start.Z, End.Z, otherCube.Start.Z, otherCube.End.Z);

                return new Cube(new(xStart, yStart, zStart), new(xEnd, yEnd, zEnd));
            }

            private static (int newStart, int newEnd) GetOverlappingCoordinate(
                int thisStart, int thisEnd, int otherStart, int otherEnd)
            {
                int newStart;
                int newEnd;
                if (thisStart <= otherStart && thisEnd <= otherEnd)
                {
                    newStart = otherStart;
                    newEnd = thisEnd;
                }
                else if (thisStart >= otherStart && thisEnd <= otherEnd)
                {
                    newStart = thisStart;
                    newEnd = thisEnd;
                }
                else if (thisStart <= otherStart && thisEnd >= otherEnd)
                {
                    newStart = otherStart;
                    newEnd = otherEnd;
                }
                else
                {
                    newStart = thisStart;
                    newEnd = otherEnd;
                }
                return (newStart, newEnd);
            }
        }

        private record Coordinate(int X, int Y, int Z);
    }
}
