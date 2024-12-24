namespace InvertedIndex.Root.Index;

public class InvertedIndex
{
    //public IReadOnlyDictionary<string, List<string>> Index => _index;

    //private Dictionary<string, List<string>> _index = new();
    private IndexDictionary _index;
    private object _locker = new();

    public InvertedIndex(int threadsCount)
    {
        _index = new IndexDictionary(threadsCount);
    }
    public void Add(string word, string index)
    {
        // lock (_locker)
        // {
        //     if (Index.TryGetValue(word, out var list))
        //     {
        //         if(list.Contains(index) == false)
        //             list.Add(index);
        //     }
        //     else
        //     {
        //         _index.Add(word, [index]);
        //     }
        // }
        _index.Add(word, index);
    }

    public bool IsBuilded => _index.IsQueuesEmpty();

    public List<string>? GetIndex(string word)
    {
        // lock (_locker)
        // {
        //     return _index.ContainsKey(word) ? Index[word] : null;
        // }
        return _index.GetValue(word);
    }
}