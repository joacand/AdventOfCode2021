using AdventOfCode2021.Data;

namespace AdventOfCode2021.Days
{
    internal sealed class Day2 : IDay
    {
        public string Solve()
        {
            var instructions = EmbeddedResource.ReadInput("InputDay2.txt")
                .Split('\n').ToArray();

            int horizontalPosition, depth;

            (horizontalPosition, depth, _) = MoveSubmarinePart1(instructions, (0, 0, 0));
            var part1Result = horizontalPosition * depth;

            (horizontalPosition, depth, _) = MoveSubmarinePart2(instructions, (0, 0, 0));
            var part2Result = horizontalPosition * depth;

            return $"Part 1: {part1Result}, Part 2: {part2Result}";
        }

        private static (int horizontalPosition, int depth, int aim) MoveSubmarinePart1(
            string[] instructions, (int horizontalPosition, int depth, int aim) submarineCoordinates)
        {
            foreach (var instructionStr in instructions)
            {
                var instruction = new Instruction(instructionStr);

                switch (instruction.Movement)
                {
                    case Movement.forward:
                        submarineCoordinates.horizontalPosition += instruction.Value;
                        break;
                    case Movement.down:
                        submarineCoordinates.depth += instruction.Value;
                        break;
                    case Movement.up:
                        submarineCoordinates.depth -= instruction.Value;
                        break;
                }
            }
            return submarineCoordinates;
        }

        private static (int horizontalPosition, int depth, int aim) MoveSubmarinePart2(
            string[] instructions, (int horizontalPosition, int depth, int aim) submarineCoordinates)
        {
            foreach (var instructionStr in instructions)
            {
                var instruction = new Instruction(instructionStr);

                switch (instruction.Movement)
                {
                    case Movement.forward:
                        submarineCoordinates.horizontalPosition += instruction.Value;
                        submarineCoordinates.depth += submarineCoordinates.aim * instruction.Value;
                        break;
                    case Movement.down:
                        submarineCoordinates.aim += instruction.Value;
                        break;
                    case Movement.up:
                        submarineCoordinates.aim -= instruction.Value;
                        break;
                }
            }
            return submarineCoordinates;
        }

        private class Instruction
        {
            public Movement Movement { get; set; }
            public int Value { get; set; }

            public Instruction(string instruction)
            {
                var parts = instruction.Split(' ');
                Movement = Enum.Parse<Movement>(parts[0]);
                Value = int.Parse(parts[1]);
            }
        }

        private enum Movement
        {
            forward,
            down,
            up
        }
    }
}
