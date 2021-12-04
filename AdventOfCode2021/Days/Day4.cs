using AdventOfCode2021.Data;

namespace AdventOfCode2021.Days
{
    internal sealed class Day4 : IDay
    {
        public string Solve()
        {
            var bingoInput = EmbeddedResource.ReadInput("InputDay4.txt")
                .Split('\n').Select(x => x.Trim());

            var numbersPicked = bingoInput.First().Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(x => int.Parse(x));

            var bingoBoards = ParseBingoBoards(bingoInput);

            var finalScoreFirstWinner = FindFirstWinner(bingoBoards, numbersPicked);

            var finalScoreLastWinner = FindLastWinner(bingoBoards, numbersPicked);

            return $"Part 1: {finalScoreFirstWinner}, Part 2: {finalScoreLastWinner}";
        }

        private static List<BingoBoard> ParseBingoBoards(IEnumerable<string> bingoInput)
        {
            List<BingoBoard> bingoBoards = new();
            List<string> boardAccumulator = new();
            foreach (var line in bingoInput.Skip(2))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    bingoBoards.Add(new BingoBoard(boardAccumulator.ToArray()));
                    boardAccumulator.Clear();
                }
                else
                {
                    boardAccumulator.Add(line);
                }
            }
            bingoBoards.Add(new BingoBoard(boardAccumulator.ToArray()));
            return bingoBoards;
        }

        private static int FindFirstWinner(IEnumerable<BingoBoard> bingoBoards, IEnumerable<int> numbersPicked)
        {
            var finalScore = -1;
            PlayNumbers(bingoBoards, numbersPicked, (board, number) =>
            {
                if (board.HasWon(out var unmarkedNumbers))
                {
                    finalScore = unmarkedNumbers.Sum() * number;
                    return true;
                }
                return false;
            });
            return finalScore;
        }

        private static int FindLastWinner(IEnumerable<BingoBoard> bingoBoards, IEnumerable<int> numbersPicked)
        {
            var lastFinalScore = -1;
            List<BingoBoard> winningBoards = new();

            PlayNumbers(bingoBoards, numbersPicked, (board, number) =>
            {
                if (board.HasWon(out var unmarkedNumbers) && !winningBoards.Contains(board))
                {
                    winningBoards.Add(board);
                    lastFinalScore = unmarkedNumbers.Sum() * number;
                }
                return false;
            });
            return lastFinalScore;
        }

        private static void PlayNumbers(
            IEnumerable<BingoBoard> bingoBoards,
            IEnumerable<int> numbersPicked,
            Func<BingoBoard, int, bool> ResultHandler)
        {
            foreach (var number in numbersPicked)
            {
                foreach (var board in bingoBoards)
                {
                    board.PullNumber(number);
                    if (ResultHandler(board, number)) { return; }
                }
            }
        }

        private class BingoBoard
        {
            public int[,] Board { get; set; } = new int[5, 5];
            public bool[,] Marks { get; set; } = new bool[5, 5];
            public Guid BoardId { get; set; } = Guid.NewGuid();

            public BingoBoard(string[] rows)
            {
                for (int r = 0; r < rows.Length; r++)
                {
                    var row = rows[r];
                    var columns = row.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    for (int c = 0; c < columns.Length; c++)
                    {
                        var value = columns[c];
                        Board[r, c] = int.Parse(value);
                    }
                }
            }

            public void PullNumber(int number)
            {
                if (TryFindIndex(number, out (int row, int col) location))
                {
                    Marks[location.row, location.col] = true;
                }
            }

            public bool HasWon(out List<int> unmarkedNumbers)
            {
                unmarkedNumbers = new();

                for (int r = 0, c = 0; r < Board.GetLength(0); r++, c = 0)
                {
                    bool colWin = true;
                    for (; c < Board.GetLength(1); c++)
                    {
                        colWin &= Marks[r, c];
                    }
                    if (colWin)
                    {
                        unmarkedNumbers = GetUnmarkedNumbers();
                        return true;
                    }
                }
                for (int r = 0, c = 0; c < Board.GetLength(1); c++, r = 0)
                {
                    bool rowWin = true;
                    for (; r < Board.GetLength(0); r++)
                    {
                        rowWin &= Marks[r, c];
                    }
                    if (rowWin)
                    {
                        unmarkedNumbers = GetUnmarkedNumbers();
                        return true;
                    }
                }
                return false;
            }

            private bool TryFindIndex(int number, out (int row, int col) location)
            {
                for (var r = 0; r < Board.GetLength(0); r++)
                {
                    for (var c = 0; c < Board.GetLength(1); c++)
                    {
                        if (Board[r, c].Equals(number))
                        {
                            location = (r, c);
                            return true;
                        }
                    }
                }
                location = (-1, -1);
                return false;
            }

            private List<int> GetUnmarkedNumbers()
            {
                List<int> result = new();
                for (var r = 0; r < Board.GetLength(0); ++r)
                {
                    for (var c = 0; c < Board.GetLength(1); ++c)
                    {
                        if (!Marks[r, c])
                        {
                            result.Add(Board[r, c]);
                        }
                    }
                }
                return result;
            }

            public override bool Equals(object obj) => obj is BingoBoard board && BoardId.Equals(board.BoardId);
            public override int GetHashCode() => HashCode.Combine(BoardId);
        }
    }
}
