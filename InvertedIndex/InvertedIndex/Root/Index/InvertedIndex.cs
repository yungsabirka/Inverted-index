namespace InvertedIndex.Root.Index;

public class InvertedIndex
{
    public IReadOnlyDictionary<string, List<string>> Index => _index;

    private Dictionary<string, List<string>> _index = new();
    private object _locker = new();

    public void Add(string word, string index)
    {
        lock (_locker)
        {
            if (Index.TryGetValue(word, out var list))
            {
                if(list.Contains(index) == false)
                    list.Add(index);
            }
            else
            {
                _index.Add(word, [index]);
            }
        }
    }

    public List<string>? GetIndex(string word)
    {
        lock (_locker)
        {
            return _index.ContainsKey(word) ? Index[word] : null;
        }
    }

    public void RemoveWord(string word)
    {
        lock (_locker)
        {
            _index.Remove(word);
        }
    }

    public void RemoveIndex(string index)
    {
        lock (_locker)
        {
            foreach (var pairs in Index.Where(pairs => pairs.Value.Contains(index)))
            {
                pairs.Value.Remove(index);
                if (pairs.Value.Count == 0)
                    _index.Remove(pairs.Key);
            }
        }
    }
}