using System.Diagnostics;
using InvertedIndex.Root.Services;

namespace InvertedIndex.Root.Index;

public class IndexBuilder
{
    private int _threadsCount;
    private FilesReader _filesReader;
    private InvertedIndex _invertedIndex;
    private ThreadPool.ThreadPool _pool;

    public IndexBuilder(int threadsCount, FilesReader filesReader, InvertedIndex invertedIndex)
    {
        _filesReader = filesReader;
        _invertedIndex = invertedIndex;
        _pool = new ThreadPool.ThreadPool(4, threadsCount);
        _pool.InitiateThreads();
        _pool.StartExecuting();
    }

    public void RebuildIndex(object? state)
    {
        //var stopwatch = Stopwatch.StartNew();

        foreach (var words in _filesReader.GetWordsFromAllFiles())
        {
            var localWords = words;
            _pool.AddTask(() =>
            {
                foreach (var word in localWords.Item1)
                {
                    _invertedIndex.Add(word.ToLower(), localWords.Item2);
                }
            });
        }

        //
        _pool.WaitAllTasks();
        //Console.WriteLine($"Index words count: {_invertedIndex.Index.Count}");
        // stopwatch.Stop();
        // Console.WriteLine(stopwatch.ElapsedMilliseconds);
        // // Console.WriteLine($"Index count: {_index.Index.Count}");
    }
}