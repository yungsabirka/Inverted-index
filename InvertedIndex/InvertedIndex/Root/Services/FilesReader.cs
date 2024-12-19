using System.Text.RegularExpressions;

namespace InvertedIndex.Root.Services;

public partial class FilesReader
{
    private const string DataPath = @"C:\Users\sabir\RiderProjects\InvertedIndex\InvertedIndex\Data";
    private List<string> _filePaths = new();

    private IEnumerable<string> GetFilePaths()
        => Directory.EnumerateFiles(DataPath, "*.*", SearchOption.AllDirectories);

    public IEnumerable<(string[], string)> GetWordsFromAllFiles()
    {
        var filePaths = GetFilePaths();
        foreach (var path in filePaths)
        {
            var cleanedPath = path.Replace(DataPath, string.Empty);
            if (_filePaths.Contains(cleanedPath))
                continue;

            _filePaths.Add(cleanedPath);
            var text = File.ReadAllText(path);
            var cleanedText = WordsRegex().Replace(text, "").Split(" ");
            yield return (cleanedText, cleanedPath);
        }
    }

    [GeneratedRegex(@"[^a-zA-Z\s]")]
    private static partial Regex WordsRegex();
}