using AdventOfCode2021.Data;

namespace AdventOfCode2021.Days
{
    internal sealed class Day10 : IDay
    {
        private List<char> OpenCharacters { get; } =
            new() { '{', '<', '[', '(' };
        private Dictionary<char, char> ExpectedClosingCharacters { get; } =
            new() { { '{', '}' }, { '<', '>' }, { '[', ']' }, { '(', ')' } };

        public string Solve()
        {
            var navigationSubsystem = EmbeddedResource.ReadInput("InputDay10.txt").Split('\n').Select(x => x.Trim()).ToArray();

            var part1Result = FindSyntaxErrorScore(navigationSubsystem);
            var part2Result = FindAutocompleteMiddleScore(navigationSubsystem);

            return $"Part 1: {part1Result}, Part 2: {part2Result}";
        }

        private long FindSyntaxErrorScore(string[] navigationSubsystem)
        {
            return navigationSubsystem.Sum(FindSyntaxErrorScore);
        }

        private long FindSyntaxErrorScore(string navigationSubsystemLine)
        {
            Stack<char> openedBrackets = new();

            foreach (var c in navigationSubsystemLine)
            {
                if (OpenCharacters.Contains(c))
                {
                    openedBrackets.Push(c);
                }
                else
                {
                    if (c != ExpectedClosingCharacters[openedBrackets.Pop()])
                    {
                        return ScoreOfSyntaxError(c);
                    }
                }
            }
            return 0;
        }

        private long FindAutocompleteMiddleScore(string[] navigationSubsystem)
        {
            var autocompleteScores = FindAutocompleteScores(navigationSubsystem.Where(x => !IsCorrupted(x))).ToList();
            autocompleteScores.Sort();
            return autocompleteScores[autocompleteScores.Count / 2];
        }

        private IEnumerable<long> FindAutocompleteScores(IEnumerable<string> incompleteLines)
        {
            return incompleteLines.Select(FindAutocompleteScore);
        }

        private long FindAutocompleteScore(string navigationSubsystemLine)
        {
            Stack<char> openedBrackets = new();

            foreach (var c in navigationSubsystemLine)
            {
                if (OpenCharacters.Contains(c))
                {
                    openedBrackets.Push(c);
                }
                else
                {
                    openedBrackets.Pop();
                }
            }

            List<char> addedChars = new();
            while (openedBrackets.TryPop(out char bracketCharacter))
            {
                addedChars.Add(ExpectedClosingCharacters[bracketCharacter]);
            }
            return addedChars.Aggregate((long)0, (acc, val) => (acc * 5) + ScoreOfAutocomplete(val));
        }

        private bool IsCorrupted(string navigationSystem)
        {
            return FindSyntaxErrorScore(navigationSystem) > 0;
        }

        private static long ScoreOfSyntaxError(char c)
        {
            return c switch
            {
                ')' => 3,
                ']' => 57,
                '}' => 1197,
                '>' => 25137,
                _ => 0
            };
        }

        private static long ScoreOfAutocomplete(char c)
        {
            return c switch
            {
                ')' => 1,
                ']' => 2,
                '}' => 3,
                '>' => 4,
                _ => 0
            };
        }
    }
}
