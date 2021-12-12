using AdventOfCode2021.Data;

namespace AdventOfCode2021.Days
{
    internal sealed class Day8 : IDay
    {
        public string Solve()
        {
            var puzzleEntries = EmbeddedResource.ReadInput("InputDay8.txt").Split('\n').Select(x => new PuzzleEntry(x)).ToList();

            var part1Result = Count1478Digits(puzzleEntries.Select(x => x.OutputValues));
            var part2Result = DecodeAndCount(puzzleEntries);

            return $"Part 1: {part1Result}, Part 2: {part2Result}";
        }

        private static int Count1478Digits(IEnumerable<IEnumerable<string>> outputValues)
        {
            return outputValues.Sum(x =>
            {
                var staticSegments = new int[] { 2, 4, 3, 7 };
                return x.Count(x => staticSegments.Contains(x.Length));
            });
        }

        private static long DecodeAndCount(IEnumerable<PuzzleEntry> puzzleEntries)
        {
            return puzzleEntries.Sum(x =>
            {
                var decodingMap = Decode(x.InputValues);
                return CountValues(decodingMap, x.OutputValues);
            });
        }

        private static Dictionary<int, List<char>> Decode(IEnumerable<string> inputValues)
        {
            var decodingMap = InitializeDecodingMap();

            Add1478Digits(decodingMap, inputValues);

            AddFiveSegmentDigits(decodingMap, inputValues);

            AddSixSegmentDigits(decodingMap, inputValues);

            return decodingMap;
        }

        private static Dictionary<int, List<char>> InitializeDecodingMap()
        {
            return new()
            {
                { 0, new() },
                { 1, new() },
                { 2, new() },
                { 3, new() },
                { 4, new() },
                { 5, new() },
                { 6, new() },
                { 7, new() },
                { 8, new() },
                { 9, new() }
            };
        }

        /// <summary>
        /// Digits 1, 4, 7, 8 can only map to one value - we get these for free.
        /// </summary>
        private static void Add1478Digits(IDictionary<int, List<char>> decodingMap, IEnumerable<string> inputValues)
        {
            foreach (var value in inputValues)
            {
                switch (value.Length)
                {
                    case 2: decodingMap[1] = value.ToCharArray().ToList(); break;
                    case 4: decodingMap[4] = value.ToCharArray().ToList(); break;
                    case 3: decodingMap[7] = value.ToCharArray().ToList(); break;
                    case 7: decodingMap[8] = value.ToCharArray().ToList(); break;
                }
            }
        }

        /// <summary>
        /// Using the digits from before we can figure out the five segmented digits by looking at the overlaps.
        /// </summary>
        private static void AddFiveSegmentDigits(IDictionary<int, List<char>> decodingMap, IEnumerable<string> inputValues)
        {
            foreach (var value in inputValues.Where(x => x.Length == 5))
            {
                // Only a 3 intersects with 1 this way
                if (value.Intersect(decodingMap[1]).Count() == 2)
                {
                    decodingMap[3] = value.ToCharArray().ToList();
                }
                // Only a 5 intersects with 4 this way
                else if (value.Except(decodingMap[4]).Count() == 2)
                {
                    decodingMap[5] = value.ToCharArray().ToList();
                }
                // Otherwise it must be a 2 (only option left)
                else
                {
                    decodingMap[2] = value.ToCharArray().ToList();
                }
            }
        }

        /// <summary>
        /// Using the digits from before we can figure out the six segmented digits by looking at the overlaps.
        /// </summary>
        private static void AddSixSegmentDigits(IDictionary<int, List<char>> decodingMap, IEnumerable<string> inputValues)
        {
            foreach (var value in inputValues.Where(x => x.Length == 6))
            {
                // Only a 6 intersects with 1 this way
                if (value.Intersect(decodingMap[1]).Count() == 1)
                {
                    decodingMap[6] = value.ToCharArray().ToList();
                }
                // Only a 9 intersects with 4 this way
                else if (value.Intersect(decodingMap[4]).Count() == 4)
                {
                    decodingMap[9] = value.ToCharArray().ToList();
                }
                // Otherwise it must be a 0 (only option left)
                else
                {
                    decodingMap[0] = value.ToCharArray().ToList();
                }
            }
        }

        private static long CountValues(IReadOnlyDictionary<int, List<char>> decodingMap, IEnumerable<string> outputValues)
        {
            var result = "";
            foreach (var value in outputValues)
            {
                var valueSet = value.ToHashSet();
                foreach (var decodingEntry in decodingMap)
                {
                    if (valueSet.SetEquals(decodingEntry.Value.ToHashSet()))
                    {
                        result += decodingEntry.Key.ToString();
                        break;
                    }
                }
            }
            return long.Parse(result);
        }

        private class PuzzleEntry
        {
            public IEnumerable<string> InputValues { get; init; }
            public IEnumerable<string> OutputValues { get; init; }

            public PuzzleEntry(string input)
            {
                var split = input.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                InputValues = split[0].Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                OutputValues = split[1].Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            }
        }
    }
}
