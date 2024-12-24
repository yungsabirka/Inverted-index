using System.Diagnostics;
using InvertedIndex.Root.Services;

namespace InvertedIndex.Root.Index;

public class IndexBuilder
{
    private int _threadsCount;
    private FilesReader _filesReader;
    private InvertedIndex _invertedIndex;

    public IndexBuilder(FilesReader filesReader, InvertedIndex invertedIndex)
    {
        _filesReader = filesReader;
        _invertedIndex = invertedIndex;
    }

    public void RebuildIndex(object? state)
    {
        foreach (var words in _filesReader.GetWordsFromAllFiles())
        {
            var localWords = words;
            foreach (var word in localWords.Item1)
            {
                _invertedIndex.Add(word.ToLower(), localWords.Item2);
            }
        }
    }
}