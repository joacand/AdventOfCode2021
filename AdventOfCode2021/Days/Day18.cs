using AdventOfCode2021.Data;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AdventOfCode2021.Days
{
    internal sealed class Day18 : IDay
    {
        public string Solve()
        {
            var numbersToSum = EmbeddedResource.ReadInput($"Input{GetType().Name}.txt").Split('\n').Select(x => x.Trim()).ToList();

            var numbers = ParseNumbers(numbersToSum);
            var resultingFishnumber = AddNumbers(numbers);
            var part1Result = resultingFishnumber.Magnitude();

            numbers = ParseNumbers(numbersToSum);
            var allCombinationResults = CalculateAllMagnitudeSums(numbers);
            var part2Result = allCombinationResults.Max();

            return $"Part 1: {part1Result}, Part 2: {part2Result}";
        }

        private static List<SnailfishNumber> ParseNumbers(IEnumerable<string> numbersToSum)
        {
            List<SnailfishNumber> numbers = new();

            foreach (var number in numbersToSum)
            {
                var snailfishNumber = ParseSnailfish(number, null);
                numbers.Add(snailfishNumber);
            }

            return numbers;
        }

        #region Parsing
        private static SnailfishNumber ParseSnailfish(string number, SnailfishNumber parent)
        {
            var snailfishNumber = new SnailfishNumber();
            if (!number.Contains(','))
            {
                snailfishNumber.Value = int.Parse(number);
                snailfishNumber.Parent = parent;
                return snailfishNumber;
            }

            number = number[1..(number.Length - 1)];
            var middleIndex = FindMiddleIndex(number);

            snailfishNumber.Left = ParseSnailfish(number[0..middleIndex], snailfishNumber);
            snailfishNumber.Right = ParseSnailfish(number[(middleIndex + 1)..number.Length], snailfishNumber);
            snailfishNumber.Parent = parent;
            return snailfishNumber;
        }

        private static int FindMiddleIndex(string input)
        {
            var middleIndex = -1;
            var lowestBracketCount = int.MaxValue;
            var brackets = 0;

            for (var i = 0; i < input.Length; i++)
            {
                var c = input[i];

                if (c == '[') { brackets++; }
                if (c == ']') { brackets--; }
                if (c == ',')
                {
                    if (brackets < lowestBracketCount)
                    {
                        lowestBracketCount = brackets;
                        middleIndex = i;
                    }
                }
            }
            return middleIndex;
        }
        #endregion

        private static SnailfishNumber AddNumbers(IEnumerable<SnailfishNumber> numbers)
        {
            var accumulator = numbers.First();
            accumulator.ReduceIfNeeded();

            foreach (var number in numbers.Skip(1))
            {
                number.ReduceIfNeeded();
                accumulator = Add(accumulator, number);
            }

            return accumulator;
        }

        private static List<int> CalculateAllMagnitudeSums(List<SnailfishNumber> numbers)
        {
            var pairs = numbers.SelectMany(a => numbers.Select(b => new[] { a.DeepClone(), b.DeepClone() })).Where(x => x[0].Id != x[1].Id).ToList();
            return pairs.Select(x => AddNumbers(new List<SnailfishNumber> { x[0], x[1] })).Select(x => x.Magnitude()).ToList();
        }

        private static SnailfishNumber Add(SnailfishNumber number1, SnailfishNumber number2)
        {
            var result = new SnailfishNumber
            {
                Left = number1,
                Right = number2
            };
            number1.Parent = result;
            number2.Parent = result;

            result.ReduceIfNeeded();

            return result;
        }

        [Serializable]
        private class SnailfishNumber
        {
            public Guid Id = Guid.NewGuid();
            public SnailfishNumber Parent { get; set; }
            public SnailfishNumber Left { get; set; }
            public SnailfishNumber Right { get; set; }
            public int? Value { get; set; }

            public int Magnitude()
            {
                if (Value.HasValue) return Value.Value;
                return (3 * (Left?.Magnitude() ?? 0)) + (2 * (Right?.Magnitude() ?? 0));
            }

            private int MaxNestedLevel => 1 + Math.Max(Left?.MaxNestedLevel ?? 0, Right?.MaxNestedLevel ?? 0);
            private bool AnyNumberHigherThan9 => Value > 9 || (Left?.AnyNumberHigherThan9 ?? false) || (Right?.AnyNumberHigherThan9 ?? false);

            public SnailfishNumber()
            { }

            public void ReduceIfNeeded()
            {
                var reduceMethod = GetNeededReduceMethod();
                if (reduceMethod == ReduceMethod.None) { return; }
                switch (reduceMethod)
                {
                    case ReduceMethod.Explode: Explode(); break;
                    case ReduceMethod.Split: Split(); break;
                }
                ReduceIfNeeded();
            }

            private void Explode()
            {
                var numberToExplode = FindExplodingNumber();

                Explode(numberToExplode, left: true);
                Explode(numberToExplode, left: false);

                numberToExplode.Left = null;
                numberToExplode.Right = null;
                numberToExplode.Value = 0;
            }

            private SnailfishNumber FindExplodingNumber(int nestLevel = 6)
            {
                if (nestLevel == 2)
                {
                    return this;
                }
                else if (Left != null && Left.MaxNestedLevel == nestLevel - 1)
                {
                    return Left.FindExplodingNumber(nestLevel - 1);
                }
                return Right.FindExplodingNumber(nestLevel - 1);
            }

            private static void Explode(SnailfishNumber numberToExplode, bool left)
            {
                var lastSnailfish = FindLastSnailfish(numberToExplode, left);

                if (lastSnailfish == null)
                {
                    return;
                }

                var valueToAdd = left
                    ? numberToExplode.Left.Value.Value
                    : numberToExplode.Right.Value.Value;

                if (lastSnailfish.MaxNestedLevel == 2)
                {
                    var fishToAdd = left
                        ? lastSnailfish.Right
                        : lastSnailfish.Left;
                    fishToAdd.Value += valueToAdd;
                }

                var snailfishToCheck = left
                    ? lastSnailfish.Parent.Right
                    : lastSnailfish.Parent.Left;

                if (snailfishToCheck.MaxNestedLevel > 2 || snailfishToCheck == numberToExplode)
                {
                    lastSnailfish.Value += valueToAdd;
                }
                else
                {
                    snailfishToCheck.Value += valueToAdd;
                }
            }

            private static SnailfishNumber FindLastSnailfish(SnailfishNumber numberToExplode, bool left)
            {
                var parent = left
                    ? numberToExplode.Parent.Left
                    : numberToExplode.Parent.Right;

                var lastSnailfish = parent;
                var parentsToGoUp = 0;

                while (parent == numberToExplode)
                {
                    var tempParent = parent.Parent;
                    for (var i = 0; i < parentsToGoUp; i++)
                    {
                        tempParent = tempParent.Parent;
                    }
                    parentsToGoUp++;

                    if (tempParent == null)
                    {
                        lastSnailfish = null;
                        break;
                    }

                    var prev = tempParent;
                    var temp2 = tempParent;
                    while (temp2.Value == null)
                    {
                        prev = temp2;
                        temp2 = left
                            ? temp2.Left
                            : temp2.Right;
                    }
                    parent = prev;
                    lastSnailfish = left
                        ? prev.Left
                        : prev.Right;

                    if (parent != numberToExplode)
                    {
                        var parentToCheck = left
                            ? tempParent?.Left?.Right
                            : tempParent?.Right?.Left;
                        if (parentToCheck != null)
                        {
                            var fetch = left
                                ? tempParent.Left
                                : tempParent.Right;

                            var tmp = left
                                ? fetch.Right
                                : fetch.Left;

                            var tmpToCheck = left
                                ? tmp.Right
                                : tmp.Left;
                            while (tmpToCheck != null)
                            {
                                tmp = left
                                    ? tmp.Right
                                    : tmp.Left;

                                tmpToCheck = left
                                    ? tmp.Right
                                    : tmp.Left;
                            }
                            lastSnailfish = tmp;
                        }
                    }
                }

                return lastSnailfish;
            }

            private void Split()
            {
                var numberToSplit = FindSplittingNumber();

                var snailfishLeft = new SnailfishNumber()
                {
                    Value = numberToSplit.Value / 2,
                    Parent = numberToSplit
                };
                var snailfishRight = new SnailfishNumber()
                {
                    Value = (int)Math.Ceiling((double)numberToSplit.Value / 2),
                    Parent = numberToSplit
                };

                numberToSplit.Value = null;
                numberToSplit.Left = snailfishLeft;
                numberToSplit.Right = snailfishRight;
            }

            private SnailfishNumber FindSplittingNumber()
            {
                SnailfishNumber result = null;

                if (Value != null && Value > 9)
                {
                    return this;
                }
                if (Left != null)
                {
                    result = Left.FindSplittingNumber();
                    if (result != null)
                    {
                        return result;
                    }
                    return Right.FindSplittingNumber();
                }
                else if (Right != null)
                {
                    return Right.FindSplittingNumber();
                }
                return result;
            }

            private ReduceMethod GetNeededReduceMethod()
            {
                if (MaxNestedLevel > 5) { return ReduceMethod.Explode; }
                if (AnyNumberHigherThan9) { return ReduceMethod.Split; }
                return ReduceMethod.None;
            }

            private enum ReduceMethod
            {
                None,
                Explode,
                Split
            }

            public override string ToString()
            {
                if (Value != null)
                {
                    return Value.ToString();
                }
                return $"[{Left},{Right}]";
            }

            public override bool Equals(object obj)
            {
                return obj is SnailfishNumber number &&
                       Id.Equals(number.Id);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Id);
            }

            #region Clone code
            private static readonly JsonSerializerOptions JsonOptions = new()
            {
                ReferenceHandler = ReferenceHandler.Preserve
            };
            public SnailfishNumber DeepClone()
            {
                var json = JsonSerializer.Serialize(this, JsonOptions);
                return (SnailfishNumber)JsonSerializer.Deserialize(json, GetType(), JsonOptions);
            }
            #endregion
        }
    }
}
