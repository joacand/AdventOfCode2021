using AdventOfCode2021.Data;

namespace AdventOfCode2021.Days
{
    internal sealed class Day16 : IDay
    {
        public string Solve()
        {
            var binaryInput = EmbeddedResource.ReadInput($"Input{GetType().Name}.txt").HexToBinary();

            var part1Result = new Packet(binaryInput, true).VersionNumberSum;
            var part2Result = new Packet(binaryInput, false).ResultingValue;

            return $"Part 1: {part1Result}, Part 2: {part2Result}";
        }

        private class Packet
        {
            private string BinaryInput { get; }
            private bool Part1 { get; }
            private int TotalIndexValueUsed { get; set; }

            private string VersionBin { get; set; }
            private int Version => VersionBin.BinaryStringToInt();

            private string TypeIdBin { get; set; }
            private int TypeId => TypeIdBin.BinaryStringToInt();

            private string LengthTypeIdBin { get; set; }
            private int LengthTypeId => LengthTypeIdBin.BinaryStringToInt();

            private string LiteralValueBin { get; set; }
            private long LiteralValue
            {
                get => LiteralValueBin.BinaryStringToLong();
                set => LiteralValueBin = value.LongToBinaryString();
            }

            private string SubpacketLengthBin { get; set; }
            private int SubpacketLength => SubpacketLengthBin.BinaryStringToInt();

            private string SubpacketNumberBin { get; set; }
            private int SubpacketNumber => SubpacketNumberBin.BinaryStringToInt();

            private PacketType Type { get; set; }

            private List<Packet> Subpackets { get; set; } = new();

            public int VersionNumberSum => Version + Subpackets.Sum(x => x.VersionNumberSum);

            public long ResultingValue => LiteralValue;

            public Packet(string binaryInput, bool part1)
            {
                Part1 = part1;
                BinaryInput = binaryInput;
                ParseInput();
            }

            private void ParseInput()
            {
                var state = ParsingState.Version;
                var index = 0;

                while (state != ParsingState.Complete)
                {
                    switch (state)
                    {
                        case ParsingState.Version: ParseVersion(ref index, ref state); break;
                        case ParsingState.TypeId: ParseTypeId(ref index, ref state); break;
                        case ParsingState.TypeIdLength: ParseTypeIdLength(ref index, ref state); break;
                        case ParsingState.Subpackets: ParseSubpackets(ref index, ref state); break;
                        case ParsingState.Literal: ParseLiteral(ref index, ref state); break;
                        case ParsingState.SubpacketLength: ParseSubpacketLength(ref index, ref state); break;
                        case ParsingState.SubpacketNumber: ParseSubpacketNumber(ref index, ref state); break;
                        case ParsingState.ExecuteOperator: ParseExecuteOperator(ref state); break;
                    }
                }
            }

            private void ParseVersion(ref int index, ref ParsingState state)
            {
                const int versionLength = 3;

                VersionBin = BinaryInput[index..(index + versionLength)];
                index += versionLength;
                state = ParsingState.TypeId;
            }

            private void ParseTypeId(ref int index, ref ParsingState state)
            {
                const int packetTypeIdLength = 3;

                TypeIdBin = BinaryInput[index..(index + packetTypeIdLength)];
                index += packetTypeIdLength;
                Type = GetType(TypeId);
                state = Type == PacketType.Literal
                    ? ParsingState.Literal
                    : ParsingState.TypeIdLength;
            }

            private PacketType GetType(long typeId)
            {
                if (Part1)
                {
                    return typeId == 4
                        ? PacketType.Literal
                        : PacketType.Operator;
                }

                return typeId switch
                {
                    0 => PacketType.Sum,
                    1 => PacketType.Product,
                    2 => PacketType.Min,
                    3 => PacketType.Max,
                    4 => PacketType.Literal,
                    5 => PacketType.GreaterThan,
                    6 => PacketType.LessThan,
                    7 => PacketType.Equals,
                    _ => throw new ArgumentOutOfRangeException(nameof(typeId))
                };
            }

            private void ParseTypeIdLength(ref int index, ref ParsingState state)
            {
                const int typeIdLengthLength = 1;

                LengthTypeIdBin = BinaryInput[index].ToString();
                index += typeIdLengthLength;
                state = LengthTypeId == 0
                    ? ParsingState.SubpacketLength
                    : ParsingState.SubpacketNumber;
            }

            private void ParseSubpackets(ref int index, ref ParsingState state)
            {
                if (!string.IsNullOrWhiteSpace(SubpacketNumberBin))
                {
                    Subpackets.AddRange(ParseNumberOfSubpackets(ref index));
                }
                else
                {
                    Subpackets.AddRange(ParseMaxLengthSubpackets(ref index));
                }

                state = ParsingState.ExecuteOperator;
                TotalIndexValueUsed = index;
            }

            private IEnumerable<Packet> ParseNumberOfSubpackets(ref int index)
            {
                var subPackets = new List<Packet>();
                for (var i = 0; i < SubpacketNumber; i++)
                {
                    subPackets.Add(ParseSubpacket(ref index, out var _));
                }
                return subPackets;
            }

            private IEnumerable<Packet> ParseMaxLengthSubpackets(ref int index)
            {
                var subPackets = new List<Packet>();
                var totalLengthUsed = 0;

                while (totalLengthUsed < SubpacketLength)
                {
                    subPackets.Add(ParseSubpacket(ref index, out var lengthUsed));
                    totalLengthUsed += lengthUsed;
                }

                return subPackets;
            }

            private Packet ParseSubpacket(ref int index, out int lengthUsed)
            {
                var binaryInput = BinaryInput[index..];
                var newPacket = new Packet(binaryInput, Part1);

                index += newPacket.TotalIndexValueUsed;
                lengthUsed = newPacket.TotalIndexValueUsed;
                return newPacket;
            }

            private void ParseLiteral(ref int index, ref ParsingState state)
            {
                const int literalLength = 4;

                var totalLiteralBin = string.Empty;
                var parsingFinished = false;

                while (!parsingFinished)
                {
                    parsingFinished = BinaryInput[index++] == '0';

                    var literalNumber = BinaryInput[index..(index + literalLength)];
                    index += literalLength;
                    totalLiteralBin += literalNumber;
                }

                LiteralValueBin = totalLiteralBin;
                TotalIndexValueUsed = index;
                state = ParsingState.Complete;
            }

            private void ParseSubpacketLength(ref int index, ref ParsingState state)
            {
                const int subpacketLengthLength = 15;

                SubpacketLengthBin = BinaryInput[index..(index + subpacketLengthLength)];
                index += subpacketLengthLength;
                state = ParsingState.Subpackets;
            }

            private void ParseSubpacketNumber(ref int index, ref ParsingState state)
            {
                const int subpacketLengthLength = 11;

                SubpacketNumberBin = BinaryInput[index..(index + subpacketLengthLength)];
                index += subpacketLengthLength;
                state = ParsingState.Subpackets;
            }

            private void ParseExecuteOperator(ref ParsingState state)
            {
                LiteralValue = Type switch
                {
                    PacketType.Sum => Subpackets.Sum(x => x.LiteralValue),
                    PacketType.Product => Subpackets.Aggregate(1L, (acc, val) => acc * val.LiteralValue),
                    PacketType.Min => Subpackets.Min(x => x.LiteralValue),
                    PacketType.Max => Subpackets.Max(x => x.LiteralValue),
                    PacketType.GreaterThan => Subpackets[0].LiteralValue > Subpackets[1].LiteralValue ? 1 : 0,
                    PacketType.LessThan => Subpackets[0].LiteralValue < Subpackets[1].LiteralValue ? 1 : 0,
                    PacketType.Equals => Subpackets[0].LiteralValue == Subpackets[1].LiteralValue ? 1 : 0,
                    _ => 0
                };
                state = ParsingState.Complete;
            }

            private enum ParsingState
            {
                Version,
                TypeId,
                TypeIdLength,
                SubpacketLength,
                SubpacketNumber,
                ExecuteOperator,
                Subpackets,
                Literal,
                Complete
            }

            private enum PacketType
            {
                Unknown,
                Literal,
                Sum,
                Min,
                Max,
                GreaterThan,
                LessThan,
                Equals,
                Product,
                Operator // Only used for part 1
            }
        }
    }

    public static class Day16Extensions
    {
        public static string HexToBinary(this string hexInput)
        {
            return string.Join(string.Empty, hexInput.Select(n => Convert.ToString(Convert.ToInt64(n.ToString(), 16), 2).PadLeft(4, '0')));
        }
        public static long BinaryStringToLong(this string input) => Convert.ToInt64(input, 2);
        public static string LongToBinaryString(this long input) => Convert.ToString(input, 2);
        public static int BinaryStringToInt(this string input) => Convert.ToInt32(input, 2);
    }
}
