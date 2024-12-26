using System.Collections.Concurrent;

namespace InvertedIndex.Root.Index;

public class IndexDictionary
{
    private List<List<IndexEntity>> _indexList = new(8);
    private BlockingCollection<(string Key, string Value, int Position)>[] _queues;
    private Thread[] _threads;
    private object _locker = new();

    public IndexDictionary(int threadsCount)
    {
        _threads = new Thread[threadsCount];
        _queues = new BlockingCollection<(string Key, string Value, int Position)>[threadsCount];

        for (int i = 0; i < threadsCount; i++)
        {
            _queues[i] = new BlockingCollection<(string, string, int)>();
            int localIndex = i;
            _threads[i] = new Thread(() => ProcessQueue(_queues[localIndex]));
            _threads[i].Start();
        }
    }

    private void ProcessQueue(BlockingCollection<(string Key, string Value, int Position)> queue)
    {
        foreach (var item in queue.GetConsumingEnumerable())
        {
            AddByThread(item.Key, item.Value, item.Position);
        }
    }

    public bool IsQueuesEmpty()
    {
        return _queues.All(queue => queue.Count == 0);
    }

    private void AddByThread(string key, string value, int position)
    {
        CheckCapacity(position);

        var entity = _indexList[position].Find(entity => entity.Key == key);
        if (entity == null)
            _indexList[position].Add(new IndexEntity(key, value));
        else
        {
            if (entity.Values.Contains(value) == false)
                entity.Values.Add(value);
        }
    }

    public void Add(string key, string value)
    {
        var position = HashKey(key);
        var threadNumber = position % _threads.Length;
        _queues[threadNumber].Add((key, value, position));
    }

    public List<string>? GetValue(string key)
    {
        var position = HashKey(key);

        if (position > _indexList.Count || _indexList[position].Count == 0)
            return null;

        var entity = _indexList[position].Find(entity => entity.Key == key);

        return entity?.Values;
    }

    private void CheckCapacity(int position)
    {
        lock (_locker)
        {
            if (_indexList.Count > position)
                return;

            var newCapacity = Math.Max(_indexList.Count * 2, position + 1);
            _indexList.Capacity = newCapacity;

            while (_indexList.Count < newCapacity)
            {
                _indexList.Add([]);
            }
        }
    }

    private int HashKey(string key)
    {
        var hashCode = key.GetHashCode();
        var index = hashCode % 3571;
        return index > 0 ? index : -index;
    }
}

public class IndexEntity
{
    public string Key;
    public List<string> Values = [];

    public IndexEntity(string key, string value)
    {
        Key = key;
        Values.Add(value);
    }
}