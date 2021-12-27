using AdventOfCode2021.Data;

namespace AdventOfCode2021.Days
{
    internal sealed class Day19 : IDay
    {
        public string Solve()
        {
            var scanners = EmbeddedResource.ReadInput($"Input{GetType().Name}.txt")
                .Split("\r\n\r\n")
                .Select(x => x.Trim())
                .Select(x => new Scanner(x.Split("\r\n")));

            var scannerCoordinator = new ScannerCoordinator(scanners);

            scannerCoordinator.AlignScanners();

            var part1Result = scannerCoordinator.CountTotalBeacons(); ;
            var part2Result = scannerCoordinator.FindMaxManhattanDistance(); ;

            return $"Part 1: {part1Result}, Part 2: {part2Result}";
        }

        private class ScannerCoordinator
        {
            private Dictionary<Scanner, List<(Scanner scannerItem, Coordinate alignmentDiff)>> ScannerAlignmentDiffs { get; } = new();
            private List<Scanner> Scanners { get; init; }
            private const int NumberOfBeaconMatchesRequired = 12;

            public ScannerCoordinator(IEnumerable<Scanner> scanners)
            {
                Scanners = scanners.ToList();
            }

            public int CountTotalBeacons()
            {
                var firstScanner = ScannerAlignmentDiffs.FirstOrDefault(x => x.Key.Id == 0);
                Coordinate deltaDiff = new(0, 0, 0);
                var uniqueBeacons = new List<Coordinate>();

                uniqueBeacons.AddRange(firstScanner.Key.BeaconsAfterAlignment);
                firstScanner.Key.PositionRelativeScannerOne = new(0, 0, 0);

                CountBeaconsRecursive(firstScanner.Key, deltaDiff, ref uniqueBeacons);

                return uniqueBeacons.Distinct().Count();
            }

            public int FindMaxManhattanDistance()
            {
                var ManhattanDistance = (Scanner s1, Scanner s2) =>
                    Math.Abs(s1.PositionRelativeScannerOne.X - s2.PositionRelativeScannerOne.X) +
                    Math.Abs(s1.PositionRelativeScannerOne.Y - s2.PositionRelativeScannerOne.Y) +
                    Math.Abs(s1.PositionRelativeScannerOne.Z - s2.PositionRelativeScannerOne.Z);

                return Scanners
                    .SelectMany(s1 => Scanners.Select(s2 => (s1, s2)))
                    .Max(x => ManhattanDistance(x.s1, x.s2));
            }

            public void AlignScanners()
            {
                var activeScanner = Scanners.First();
                activeScanner.BeaconsAfterAlignment = activeScanner.OriginalBeacons;

                var scannersToCheck = new List<Scanner>() { activeScanner };
                var scannerIdsComplete = new List<int> { activeScanner.Id };

                while (activeScanner != null && scannerIdsComplete.Count != Scanners.Count)
                {
                    if (!ScannerAlignmentDiffs.ContainsKey(activeScanner))
                    {
                        ScannerAlignmentDiffs.Add(activeScanner, new());
                    }

                    var pairs = Scanners.Where(x => !scannerIdsComplete.Contains(x.Id)).Select(s2 => (activeScanner, s2));

                    pairs.AsParallel().ForAll(scanningPair =>
                    {
                        if (AlignBeaconsBetweenScanners(scanningPair, out Coordinate diff))
                        {
                            Console.WriteLine($"Found diff-alignment between scanner {scanningPair.activeScanner.Id} and scanner {scanningPair.s2.Id}: {diff}");
                            ScannerAlignmentDiffs[activeScanner].Add((scanningPair.s2, diff));
                        }
                    });

                    scannersToCheck.AddRange(ScannerAlignmentDiffs[activeScanner].Select(x => x.scannerItem));

                    activeScanner = scannersToCheck.Where(x => !scannerIdsComplete.Contains(x.Id)).First();
                    scannerIdsComplete.Add(activeScanner.Id);
                }
            }

            private static bool AlignBeaconsBetweenScanners((Scanner s1, Scanner s2) scannerPair, out Coordinate alignmentDiff)
            {
                Console.WriteLine($"Searching match between {scannerPair.s1} and {scannerPair.s2}");
                alignmentDiff = new(0, 0, 0);
                var s1Beacons = scannerPair.s1.BeaconsAfterAlignment;
                var s2Permutations = scannerPair.s2.BeaconPermutations;

                var beaconPairs = s1Beacons
                    .SelectMany(b1 => s2Permutations
                        .SelectMany(b2permutated => b2permutated
                            .Select(b2 => (b1, b2, b1.SubDiff(b2)))));

                Coordinate outAlignment = new(0, 0, 0);
                var alignmentFound = false;

                // Slow code here - there are a lot of beacon-pairs to go through. Optimization possible.
                beaconPairs.AsParallel().ForAll(x =>
                {
                    var beacon1 = x.b1;
                    var beacon2 = x.b2;
                    var alignmentDiffToApply = x.Item3;

                    var correctAlignment = TestDiff(alignmentDiffToApply, s1Beacons, s2Permutations, out int matchingBeacons, out var s2beaconsAfterAlignment);

                    if (correctAlignment)
                    {
                        outAlignment = alignmentDiffToApply;
                        scannerPair.s2.BeaconsAfterAlignment = s2beaconsAfterAlignment;
                        alignmentFound = true;
                    }
                });

                alignmentDiff = outAlignment;
                return alignmentFound;
            }

            private static bool TestDiff(
                Coordinate alignmentDiffToApply,
                IEnumerable<Coordinate> s1Beacons,
                List<List<Coordinate>> s2beaconPermutations,
                out int matchingBeacons,
                out List<Coordinate> s2beaconsAfterAlignment)
            {
                matchingBeacons = 0;
                s2beaconsAfterAlignment = new();

                foreach (var s2beaconPermutation in s2beaconPermutations)
                {
                    s2beaconsAfterAlignment = s2beaconPermutation;

                    var s2permutationsWithDiffs = s2beaconPermutation.Select(x => x.AddDiff(alignmentDiffToApply));
                    matchingBeacons = s2permutationsWithDiffs.Intersect(s1Beacons).Count();

                    if (matchingBeacons >= NumberOfBeaconMatchesRequired)
                    {
                        return true;
                    }
                }
                return false;
            }

            private void CountBeaconsRecursive(Scanner scanner, Coordinate deltaDiff, ref List<Coordinate> uniqueBeacons)
            {
                if (!ScannerAlignmentDiffs.ContainsKey(scanner)) { return; }

                var alignedScanners = ScannerAlignmentDiffs[scanner];

                foreach (var (scannerItem, alignmentDiff) in alignedScanners)
                {
                    var newDelta = deltaDiff.AddDiff(alignmentDiff);
                    var beaconsAfterAppliedDiff = scannerItem.BeaconsAfterAlignment.Select(x => x.AddDiff(newDelta));
                    scannerItem.PositionRelativeScannerOne = newDelta;

                    uniqueBeacons.AddRange(beaconsAfterAppliedDiff);

                    CountBeaconsRecursive(scannerItem, newDelta, ref uniqueBeacons);
                }
            }
        }

        private class Scanner
        {
            public int Id { get; }
            public List<Coordinate> OriginalBeacons { get; set; } = new();
            public List<Coordinate> BeaconsAfterAlignment { get; set; } = new();
            public List<List<Coordinate>> BeaconPermutations { get; private set; }
            public Coordinate PositionRelativeScannerOne { get; set; }

            public Scanner(string[] input)
            {
                Id = int.Parse(input[0].Split("--- scanner ")[1].Split(' ')[0]);

                foreach (var beaconCoordinate in input.Skip(1))
                {
                    var split = beaconCoordinate.Split(',');
                    var beacon = new Coordinate(int.Parse(split[0]), int.Parse(split[1]), int.Parse(split[2]));
                    OriginalBeacons.Add(beacon);
                }

                CreateBeaconPermutations();
            }

            private void CreateBeaconPermutations()
            {
                BeaconPermutations = new()
                {
                    // Towards +x
                    OriginalBeacons.Select(c => new Coordinate(c.X, c.Y, c.Z)).ToList(),
                    OriginalBeacons.Select(c => new Coordinate(c.X, c.Z, -c.Y)).ToList(),
                    OriginalBeacons.Select(c => new Coordinate(c.X, -c.Y, -c.Z)).ToList(),
                    OriginalBeacons.Select(c => new Coordinate(c.X, -c.Z, c.Y)).ToList(),
                    // Towards -x
                    OriginalBeacons.Select(c => new Coordinate(-c.X, -c.Y, c.Z)).ToList(),
                    OriginalBeacons.Select(c => new Coordinate(-c.X, c.Z, c.Y)).ToList(),
                    OriginalBeacons.Select(c => new Coordinate(-c.X, c.Y, -c.Z)).ToList(),
                    OriginalBeacons.Select(c => new Coordinate(-c.X, -c.Z, -c.Y)).ToList(),
                    // Towards +y
                    OriginalBeacons.Select(c => new Coordinate(c.Y, -c.X, c.Z)).ToList(),
                    OriginalBeacons.Select(c => new Coordinate(c.Y, c.Z, c.X)).ToList(),
                    OriginalBeacons.Select(c => new Coordinate(c.Y, c.X, -c.Z)).ToList(),
                    OriginalBeacons.Select(c => new Coordinate(c.Y, -c.Z, -c.X)).ToList(),
                    // Towards -y
                    OriginalBeacons.Select(c => new Coordinate(-c.Y, c.X, c.Z)).ToList(),
                    OriginalBeacons.Select(c => new Coordinate(-c.Y, c.Z, -c.X)).ToList(),
                    OriginalBeacons.Select(c => new Coordinate(-c.Y, -c.X, -c.Z)).ToList(),
                    OriginalBeacons.Select(c => new Coordinate(-c.Y, -c.Z, c.X)).ToList(),
                    // Towards +z
                    OriginalBeacons.Select(c => new Coordinate(c.Z, c.Y, -c.X)).ToList(),
                    OriginalBeacons.Select(c => new Coordinate(c.Z, -c.X, -c.Y)).ToList(),
                    OriginalBeacons.Select(c => new Coordinate(c.Z, -c.Y, c.X)).ToList(),
                    OriginalBeacons.Select(c => new Coordinate(c.Z, c.X, c.Y)).ToList(),
                    // Towards -z
                    OriginalBeacons.Select(c => new Coordinate(-c.Z, -c.Y, -c.X)).ToList(),
                    OriginalBeacons.Select(c => new Coordinate(-c.Z, -c.X, c.Y)).ToList(),
                    OriginalBeacons.Select(c => new Coordinate(-c.Z, c.Y, c.X)).ToList(),
                    OriginalBeacons.Select(c => new Coordinate(-c.Z, c.X, -c.Y)).ToList()
                };
            }

            public override string ToString()
            {
                return Id.ToString();
            }
        }

        private record Coordinate(int X, int Y, int Z)
        {
            public Coordinate AddDiff(Coordinate diff)
            {
                return new(X + diff.X, Y + diff.Y, Z + diff.Z);
            }

            public Coordinate SubDiff(Coordinate diff)
            {
                return new(X - diff.X, Y - diff.Y, Z - diff.Z);
            }
        }
    }
}
