using System.Text.RegularExpressions;

namespace InvertedIndex.Root.Services;

public partial class FilesReader
{
    private readonly string _dataPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Data");
    private List<string> _filePaths = [];

    private IEnumerable<string> GetFilePaths()
        => Directory.EnumerateFiles(_dataPath, "*.*", SearchOption.AllDirectories);

    public IEnumerable<(string[], string)> GetWordsFromAllFiles()
    {
        var filePaths = GetFilePaths();
        foreach (var path in filePaths)
        {
            var cleanedPath = path.Replace(_dataPath, string.Empty);
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