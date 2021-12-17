using AdventOfCode2021.Data;

namespace AdventOfCode2021.Days
{
    internal sealed class Day13 : IDay
    {
        public string Solve()
        {
            var input = EmbeddedResource.ReadInput("InputDay13.txt").Split('\n').Select(x => x.Trim()).ToList();

            var paper = input.TakeWhile(x => !string.IsNullOrWhiteSpace(x)).ToList();
            var instructions = input.SkipWhile(x => !string.IsNullOrWhiteSpace(x)).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

            var part1Result = FoldWithInstructions(new TransparentPaper(paper.Select(x => x.Split(','))), instructions.Take(1));

            var transparentPaperPart2 = new TransparentPaper(paper.Select(x => x.Split(',')));
            FoldWithInstructions(transparentPaperPart2, instructions);
            Console.WriteLine("Printing paper to see the eight letter answer:\n");
            transparentPaperPart2.PrintPaper();

            return $"Part 1: {part1Result}, Part 2: -";
        }

        private static int FoldWithInstructions(TransparentPaper transparentPaper, IEnumerable<string> foldInstructions)
        {
            foreach (var foldInstruction in foldInstructions)
            {
                var split = foldInstruction.Split(' ');
                var instrSplit = split.Last().Split('=');
                var direction = instrSplit[0];
                var index = int.Parse(instrSplit[1]);

                transparentPaper.Fold(direction, index);
            }
            return transparentPaper.TotalDots();
        }

        private class TransparentPaper
        {
            private int[,] Paper { get; set; }

            public TransparentPaper(IEnumerable<string[]> dots)
            {
                var maxCol = dots.Select(x => int.Parse(x[0])).Max() + 1;
                var maxRow = dots.Select(x => int.Parse(x[1])).Max() + 1;

                Paper = new int[maxCol, maxRow];

                foreach (var dot in dots)
                {
                    var col = int.Parse(dot[0]);
                    var row = int.Parse(dot[1]);
                    Paper[col, row] = 1;
                }
            }

            public void Fold(string direction, int index)
            {
                if (direction == "y")
                {
                    FoldVertical(index);
                }
                else
                {
                    FoldHorizontal(index);
                }
            }

            public int TotalDots()
            {
                var dots = 0;
                foreach (var p in Paper)
                {
                    if (p == 1) dots++;
                }
                return dots;
            }

            public void PrintPaper()
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Print(Paper);
                Console.ForegroundColor = ConsoleColor.White;
            }

            private void FoldVertical(int index)
            {
                var newRowLength = Paper.GetLength(1) - (Paper.GetLength(1) - index);
                var topPaper = new int[Paper.GetLength(0), newRowLength];
                CopyFromPaper(topPaper, 0, 0);

                var bottomPaper = new int[Paper.GetLength(0), newRowLength];
                CopyFromPaper(bottomPaper, 0, bottomPaper.GetLength(1) + 1);

                var flippedBottomPaper = bottomPaper.FlipVertical();
                SuperImposeArrays(topPaper, flippedBottomPaper);
                Paper = topPaper;
            }

            private void FoldHorizontal(int index)
            {
                var newColLength = Paper.GetLength(0) - (Paper.GetLength(0) - index);
                var leftPaper = new int[newColLength, Paper.GetLength(1)];
                CopyFromPaper(leftPaper, 0, 0);

                var rightPaper = new int[newColLength, Paper.GetLength(1)];
                CopyFromPaper(rightPaper, rightPaper.GetLength(0) + 1, 0);

                var flippedBottomPaper = rightPaper.FlipHorizontal();
                SuperImposeArrays(leftPaper, flippedBottomPaper);
                Paper = leftPaper;
            }

            private void CopyFromPaper(int[,] array1, int colShift, int rowShift)
            {
                for (var r = 0; r < array1.GetLength(1); r++)
                {
                    for (var c = 0; c < array1.GetLength(0); c++)
                    {
                        array1[c, r] = Paper[c + colShift, r + rowShift];
                    }
                }
            }

            private static void SuperImposeArrays(int[,] array1, int[,] array2)
            {
                for (var r = 0; r < array1.GetLength(1); r++)
                {
                    for (var c = 0; c < array1.GetLength(0); c++)
                    {
                        if (array1[c, r] == 0) { array1[c, r] = array2[c, r]; }
                    }
                }
            }

            private static void Print(int[,] paper)
            {
                for (var r = 0; r < paper.GetLength(1); r++)
                {
                    for (var c = 0; c < paper.GetLength(0); c++)
                    {
                        Console.Write(string.Format("{0} ", paper[c, r] == 1 ? '#' : ' '));
                    }
                    Console.Write(Environment.NewLine);
                }
                Console.WriteLine();
            }
        }
    }

    public static class Day13Extensions
    {
        public static T[,] FlipHorizontal<T>(this T[,] array)
        {
            var rows = array.GetLength(0);
            var columns = array.GetLength(1);

            T[,] flippedArray = new T[rows, columns];

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    flippedArray[r, c] = array[rows - 1 - r, c];
                }
            }

            return flippedArray;
        }

        public static T[,] FlipVertical<T>(this T[,] array)
        {
            var rows = array.GetLength(0);
            var columns = array.GetLength(1);

            T[,] flippedArray = new T[rows, columns];

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    flippedArray[r, c] = array[r, columns - 1 - c];
                }
            }

            return flippedArray;
        }
    }
}
