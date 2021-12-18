using AdventOfCode2021.Data;

namespace AdventOfCode2021.Days
{
    internal sealed class Day14 : IDay
    {
        public string Solve()
        {
            var input = EmbeddedResource.ReadInput("InputDay14.txt").Split('\n').Select(x => x.Trim()).ToList();

            var part1Result = new PolymerHandler(input).MaxMinOccurancesAfterSteps(10);
            var part2Result = new PolymerHandler(input).MaxMinOccurancesAfterSteps(40);

            return $"Part 1: {part1Result}, Part 2: {part2Result}";
        }

        private class PolymerHandler
        {
            private string PolymerTemplate { get; }
            private List<(string, (string, string))> Instructions { get; set; } = new();
            private Dictionary<string, long> PairCounter { get; set; } = new();

            public PolymerHandler(List<string> input)
            {
                PolymerTemplate = input.First().Trim();

                var pairInsertions = input.Skip(2).Select(x => x.Trim()).ToList();
                Instructions = pairInsertions.Select(x =>
                {
                    var split = x.Split(" -> ");
                    var left = $"{split[0][0]}{split[0][1]}";
                    var right = ($"{split[0][0]}{split[1][0]}", $"{split[1][0]}{split[0][1]}");
                    return (left, right);
                }).ToList();

                for (int i = 0, j = 1; j < PolymerTemplate.Length; i++, j++)
                {
                    var pair = $"{PolymerTemplate[i]}{PolymerTemplate[j]}";
                    if (!PairCounter.ContainsKey(pair))
                    {
                        PairCounter[pair] = 0;
                    }
                    PairCounter[pair] += 1;
                }
            }

            public long MaxMinOccurancesAfterSteps(int steps)
            {
                for (var i = 0; i < steps; i++)
                {
                    PerformStep();
                }

                var occuranceResults = PairCounter.GroupBy(x => x.Key.Last()).Select(x => x.Sum(y => y.Value));

                return occuranceResults.Max() - occuranceResults.Min();
            }

            private void PerformStep()
            {
                var resultingPairCounter = new Dictionary<string, long>(PairCounter);

                foreach (var instruction in Instructions)
                {
                    if (PairCounter.ContainsKey(instruction.Item1) && PairCounter[instruction.Item1] > 0)
                    {
                        var occurances = PairCounter[instruction.Item1];

                        resultingPairCounter[instruction.Item1] -= occurances; // The pair is getting replaced by two new pairs

                        var replacementPair1 = instruction.Item2.Item1;
                        var replacementPair2 = instruction.Item2.Item2;

                        if (!resultingPairCounter.ContainsKey(replacementPair1)) { resultingPairCounter[replacementPair1] = 0; }
                        if (!resultingPairCounter.ContainsKey(replacementPair2)) { resultingPairCounter[replacementPair2] = 0; }

                        resultingPairCounter[replacementPair1] += occurances;
                        resultingPairCounter[replacementPair2] += occurances;
                    }
                }

                PairCounter = resultingPairCounter;
            }
        }
    }
}
