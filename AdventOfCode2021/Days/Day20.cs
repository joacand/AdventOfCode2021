using AdventOfCode2021.Data;
using System.Text;

namespace AdventOfCode2021.Days
{
    internal sealed class Day20 : IDay
    {
        public string Solve()
        {
            var sections = EmbeddedResource.ReadInput($"Input{GetType().Name}.txt")
                .Split("\r\n\r\n")
                .Select(x => x.Trim())
                .ToArray();

            var enhancementAlgorithm = sections[0].Trim();
            var inputImage = sections[1].Split("\r\n").Select(x => x.Trim()).ToArray();

            var algorithmHandler = new AlgorithmHandler(enhancementAlgorithm, inputImage);
            algorithmHandler.EnhanceImage(2);
            var part1Result = algorithmHandler.TotalLitPixels();

            algorithmHandler = new AlgorithmHandler(enhancementAlgorithm, inputImage);
            algorithmHandler.EnhanceImage(50);
            var part2Result = algorithmHandler.TotalLitPixels();

            return $"Part 1: {part1Result}, Part 2: {part2Result}";
        }

        private class AlgorithmHandler
        {
            private string EnhancementAlgorithm { get; }
            private bool[,] Image { get; set; }
            private bool NewPixelsAreLit { get; set; }
            private bool FlipPixelColor { get; }

            public AlgorithmHandler(string enhancementAlgorithm, string[] inputImage)
            {
                EnhancementAlgorithm = enhancementAlgorithm;
                FlipPixelColor = EnhancementAlgorithm[0] == '#' && EnhancementAlgorithm.Last() == '.';

                Image = new bool[inputImage.Length, inputImage.First().Length];

                for (var x = 0; x < inputImage.Length; x++)
                {
                    var columns = inputImage[x].ToCharArray();
                    for (var y = 0; y < columns.Length; y++)
                    {
                        var value = columns[y];
                        Image[x, y] = value == '#';
                    }
                }
            }

            public int TotalLitPixels()
            {
                var litPixels = 0;
                for (var x = 0; x < Image.GetLength(0); x++)
                {
                    for (var y = 0; y < Image.GetLength(1); y++)
                    {
                        if (Image[x, y]) { litPixels++; }
                    }
                }
                return litPixels;
            }

            public void EnhanceImage(int numberOfEnhancements)
            {
                for (var i = 0; i < numberOfEnhancements; i++)
                {
                    EnhanceImage();
                }
            }

            private void EnhanceImage()
            {
                bool[,] enlargedImage = new bool[Image.GetLength(0) + 2, Image.GetLength(1) + 2];

                if (NewPixelsAreLit)
                {
                    for (var x = 0; x < enlargedImage.GetLength(0); x++)
                    {
                        for (var y = 0; y < enlargedImage.GetLength(1); y++)
                        {
                            enlargedImage[x, y] = true;
                        }
                    }
                }

                for (var x = 0; x < Image.GetLength(0); x++)
                {
                    for (var y = 0; y < Image.GetLength(1); y++)
                    {
                        enlargedImage[x + 1, y + 1] = Image[x, y];
                    }
                }

                Image = enlargedImage;

                ApplyEnhancement();

                if (FlipPixelColor)
                {
                    NewPixelsAreLit = !NewPixelsAreLit;
                }
            }

            private void ApplyEnhancement()
            {
                List<(int xx, int yy, bool result)> pixelActions = new();

                for (var x = 0; x < Image.GetLength(0); x++)
                {
                    for (var y = 0; y < Image.GetLength(1); y++)
                    {
                        var binaryNumber = GetBinaryNumber(Image, x, y);
                        var c = EnhancementAlgorithm[binaryNumber];
                        pixelActions.Add((x, y, c == '#'));
                    }
                }

                foreach (var (xx, yy, result) in pixelActions)
                {
                    Image[xx, yy] = result;
                }
            }

            private int GetBinaryNumber(bool[,] image, int x, int y)
            {
                StringBuilder binary = new();
                for (var i = x - 1; i <= x + 1; i++)
                {
                    for (var j = y - 1; j <= y + 1; j++)
                    {
                        if (i < 0 || j < 0 || i >= image.GetLength(0) || j >= image.GetLength(1))
                        {
                            binary.Append(NewPixelsAreLit ? '1' : '0');
                        }
                        else
                        {
                            binary.Append(image[i, j] ? '1' : '0');
                        }
                    }
                }
                return Convert.ToInt32(binary.ToString(), 2);
            }
        }
    }
}
