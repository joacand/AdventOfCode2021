using AdventOfCode2021.Data;

namespace AdventOfCode2021.Days
{
    internal sealed class Day21 : IDay
    {
        public string Solve()
        {
            var playerPositions = EmbeddedResource.ReadInput($"Input{GetType().Name}.txt")
                .Split("\r\n")
                .Select(x => x.Trim())
                .ToArray();

            Player p1 = new(int.Parse(playerPositions[0].TakeLast(2).ToArray()));
            Player p2 = new(int.Parse(playerPositions[1].TakeLast(2).ToArray()));
            Game game = new(p1, p2, new Deterministic100Die(), 1000);
            var part1Result = game.Play();

            p1 = new(int.Parse(playerPositions[0].TakeLast(2).ToArray()));
            p2 = new(int.Parse(playerPositions[1].TakeLast(2).ToArray()));
            game = new(p1, p2, new DiracDie(), 21);
            var part2Result = game.Play();

            return $"Part 1: {part1Result}, Part 2: {part2Result}";
        }

        private class Game
        {
            private Player Player1 { get; }
            private Player Player2 { get; }
            private IDie Die { get; }
            private int WinConditionScore { get; }
            private Dictionary<int, int> DiracRollTimes { get; } = new();
            private Dictionary<(Player, Player, bool), (long p1wins, long p2wins)> MemoziationDict { get; } = new();

            public Game(Player p1, Player p2, IDie die, int winConditionScore)
            {
                Player1 = p1;
                Player2 = p2;
                Die = die;
                WinConditionScore = winConditionScore;
            }

            public long Play()
            {
                if (Die is Deterministic100Die)
                {
                    return PlayForLoserDieScore();
                }

                var diracRolls = new int[] { 1, 2, 3 };
                var diracRollSums = diracRolls.SelectMany(r1 => diracRolls.SelectMany(r2 => diracRolls.Select(r3 => r1 + r2 + r3))).ToArray();

                foreach (var rollSum in diracRollSums)
                {
                    if (!DiracRollTimes.ContainsKey(rollSum))
                    {
                        DiracRollTimes.Add(rollSum, 0);
                    }
                    DiracRollTimes[rollSum] += 1;
                }

                var (p1wins, p2wins) = PlayDiracRecursive(Player1, Player2, true);
                return Math.Max(p1wins, p2wins);
            }

            private (long, long) PlayDiracRecursive(Player p1, Player p2, bool p1sTurn)
            {
                (long totalp1wins, long totalp2wins) accumulator = (0L, 0L);

                if (MemoziationDict.TryGetValue((p1, p2, p1sTurn), out var cachedAccumulator))
                {
                    return cachedAccumulator;
                }

                foreach (var diracRoll in DiracRollTimes)
                {
                    var newPlayer1 = new Player(p1.Position, p1.Score);
                    var newPlayer2 = new Player(p2.Position, p2.Score);

                    var playingPlayer = p1sTurn ? newPlayer1 : newPlayer2;
                    playingPlayer.Move(diracRoll.Key);


                    if (playingPlayer.Score >= WinConditionScore)
                    {
                        accumulator = p1sTurn
                            ? (accumulator.totalp1wins + diracRoll.Value, accumulator.totalp2wins)
                            : (accumulator.totalp1wins, accumulator.totalp2wins + diracRoll.Value);
                    }
                    else
                    {
                        (var p1wins, var p2wins) = PlayDiracRecursive(newPlayer1, newPlayer2, !p1sTurn);
                        accumulator = (
                            accumulator.totalp1wins + (p1wins * diracRoll.Value),
                            accumulator.totalp2wins + (p2wins * diracRoll.Value)
                            );
                    }
                }

                MemoziationDict.Add((p1, p2, p1sTurn), accumulator);
                return accumulator;
            }

            private long PlayForLoserDieScore()
            {
                var currentPlayer = Player1;
                while (Player1.Score < WinConditionScore && Player2.Score < WinConditionScore)
                {
                    var roll = Die.Roll(3);
                    currentPlayer.Move(roll);
                    currentPlayer = currentPlayer == Player1 ? Player2 : Player1;
                }
                var losingPlayer = Player1.Score < WinConditionScore ? Player1 : Player2;
                return losingPlayer.Score * Die.NumberOfRolls;
            }
        }

        private class Player
        {
            public int Position { get; set; }
            public int Score { get; set; }

            public Player(int startPosition, int startingScore = 0)
            {
                Position = startPosition;
                Score = startingScore;
            }

            public void Move(int steps)
            {
                Position = ((-1 + Position + steps) % 10) + 1;
                Score += Position;
            }

            public override bool Equals(object obj)
            {
                return obj is Player player &&
                       Position == player.Position &&
                       Score == player.Score;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Position, Score);
            }
        }

        private interface IDie
        {
            public int NumberOfRolls { get; }
            int Roll(int times);
            int Roll();
        }

        private class Deterministic100Die : IDie
        {
            private int currentIndex = 1;
            public int NumberOfRolls { get; private set; }

            public int Roll(int times)
            {
                var sum = 0;
                for (int i = 0; i < times; i++)
                {
                    sum += Roll();
                }
                return sum;
            }

            public int Roll()
            {
                NumberOfRolls++;
                return ((-1 + currentIndex++) % 100) + 1;
            }
        }

        private class DiracDie : IDie
        {
            public int NumberOfRolls => throw new NotImplementedException();
            public int Roll(int times) => throw new NotImplementedException();
            public int Roll() => throw new NotImplementedException();
        }
    }
}
