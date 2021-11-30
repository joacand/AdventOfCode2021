using System.Reflection;

namespace AdventOfCode2021.Data
{
    internal static class EmbeddedResource
    {
        public static string ReadInput(string fileName)
        {
            using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                $"{nameof(AdventOfCode2021)}.{nameof(Data)}.{fileName}");
            using StreamReader reader = new(stream);
            return reader.ReadToEnd();
        }
    }
}
