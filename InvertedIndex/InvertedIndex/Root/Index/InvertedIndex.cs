namespace InvertedIndex.Root.Index;

public class InvertedIndex
{
    private IndexDictionary _index;

    public InvertedIndex(int threadsCount)
    {
        _index = new IndexDictionary(threadsCount);
    }
    
    public bool IsBuilded => _index.IsQueuesEmpty();
    
    public void Add(string word, string index)
    {
        _index.Add(word, index);
    }

    public List<string>? GetIndex(string word)
    {
        return _index.GetValue(word);
    }
}