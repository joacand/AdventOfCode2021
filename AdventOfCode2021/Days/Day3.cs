using AdventOfCode2021.Data;

namespace AdventOfCode2021.Days
{
    internal sealed class Day3 : IDay
    {
        public string Solve()
        {
            var instructions = EmbeddedResource.ReadInput("InputDay3.txt")
                .Split('\n').Select(x => x.Trim()).ToArray();

            var part1Result = CalculatePowerConsumption(instructions);
            var part2Result = CalculateLifeSupportRating(instructions);

            return $"Part 1: {part1Result}, Part 2: {part2Result}";
        }

        private static int CalculatePowerConsumption(string[] numbers)
        {
            var gamma = "";
            var epsilon = "";

            var reOrderedNumbers = ReOrder(numbers);
            foreach (var number in reOrderedNumbers)
            {
                var mostCommonBit = MostCommonBit(number);
                gamma += mostCommonBit;
                epsilon += Invert(mostCommonBit);
            }

            return Convert.ToInt32(gamma, 2) * Convert.ToInt32(epsilon, 2);
        }

        private static int CalculateLifeSupportRating(string[] numbers)
        {
            var oxygenGeneratorRating = CalculateRating(numbers, true);
            var co2ScrubbingRating = CalculateRating(numbers, false);

            return Convert.ToInt32(oxygenGeneratorRating, 2) * Convert.ToInt32(co2ScrubbingRating, 2);
        }

        private static string CalculateRating(string[] numbers, bool isOxygenGenerator)
        {
            var ind = 0;
            while (numbers.Length > 1)
            {
                var reOrderedNumbers = ReOrder(numbers);
                var number = reOrderedNumbers[ind];
                var mostCommonBit = MostCommonBit(number, isOxygenGenerator);

                numbers = numbers.Where(x => x[ind] == mostCommonBit).ToArray();
                ind++;
            }
            return numbers[0];
        }

        private static string[] ReOrder(string[] instructions)
        {
            List<string> newNumbers = new();

            foreach (var _ in instructions.First())
            {
                newNumbers.Add(string.Empty);
            }

            foreach (var line in instructions)
            {
                for (int i = 0; i < line.Length; i++)
                {
                    newNumbers[i] += line[i];
                }
            }
            return newNumbers.ToArray();
        }

        private static char MostCommonBit(string number, bool isOxygenGenerator = true)
        {
            var ones = number.Count(x => x == '1');
            var zeroes = number.Length - ones;

            // Edge case handling for part 2
            if (ones == zeroes)
            {
                return isOxygenGenerator ? '1' : '0';
            }

            var mostCommonBit = ones > zeroes ? '1' : '0';
            return isOxygenGenerator ? mostCommonBit : Invert(mostCommonBit);
        }

        private static char Invert(char c)
        {
            return c == '1' ? '0' : '1';
        }
    }
}
