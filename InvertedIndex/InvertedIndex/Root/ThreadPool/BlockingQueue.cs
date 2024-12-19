namespace InvertedIndex.Root.ThreadPool;

public class BlockingQueue<T>
{
    private Queue<T> _queue;

    public int Count
    {
        get
        {
            lock (_queue)
            {
                return _queue.Count;
            }
        }
    }

    public BlockingQueue()
    {
        _queue = new();
    }

    public void Enqueue(T item)
    {
        lock (_queue)
        {
            _queue.Enqueue(item);
        }
    }

    public T Dequeue()
    {
        lock (_queue)
        {
            return _queue.Dequeue();
        }
    }
}
