namespace InvertedIndex.Root.ThreadPool;

using System.Diagnostics;

public class ThreadPool
{
    private List<BlockingQueue<Action>> _queueList = new();
    private List<Thread> _threadsList = new();

    private int _threadsPerQueue;
    private int _amountOfQueue;
    private int _executingTaskCounter;
    private int _waitTaskCounter;

    private float _executingTaskTime;
    private float _waitTaskTime;

    private bool _isContinueExecuting = true;
    private bool _isRunningApp = true;

    private object _executeLocker = new();
    private object _appLocker = new();

    public List<BlockingQueue<Action>> QueueList => _queueList;
    public int QueueListCount => _queueList.Count;
    public bool IsRunningApp => _isRunningApp;

    public float AverageExecutingTime => _executingTaskTime / _executingTaskCounter;
    public float AverageWaitingTime => _waitTaskTime / _waitTaskCounter;
    public int WaitTaskCounter => _waitTaskCounter;
    public int ExecutingTaskCounter => _executingTaskCounter;
    public int ThreadsCount => _threadsList.Count;

    public ThreadPool(int amountOfQueue = 2, int threadsPerQueue = 2)
    {
        _threadsPerQueue = threadsPerQueue;
        _amountOfQueue = amountOfQueue;

        if (amountOfQueue <= 0 || threadsPerQueue <= 0)
            throw new ArgumentOutOfRangeException();

        for (int i = 0; i < amountOfQueue; i++)
            _queueList.Add(new BlockingQueue<Action>());
    }

    private void ExecuteTask(int queueNumber)
    {
        Stopwatch executingTimer = new();
        Stopwatch waitTimer = new();
        while (_isRunningApp)
        {
            Action? task = null;
            if (queueNumber >= _queueList.Count)
                throw new Exception("Queue Number was out of range");

            lock (_executeLocker)
            {
                while (_queueList[queueNumber].Count == 0 && _isContinueExecuting)
                {
                    try
                    {
                        _waitTaskCounter++;
                        waitTimer.Start();
                        Monitor.Wait(_executeLocker);
                        waitTimer.Stop();
                    }
                    catch
                    {
                        throw new ThreadInterruptedException();
                    }
                }

                if (_isContinueExecuting)
                {
                    task = _queueList[queueNumber].Dequeue();
                    _executingTaskCounter++;
                    Monitor.Pulse(_executeLocker);
                }
            }

            if (task != null)
            {
                executingTimer.Start();
                try
                {
                    task.Invoke();
                }
                finally
                {
                    executingTimer.Stop();
                    lock (_executeLocker)
                    {
                        _executingTaskCounter--; 
                        Monitor.PulseAll(_executeLocker);
                    }
                }
            }
            if (_isRunningApp && _isContinueExecuting == false)
            {
                lock (_appLocker)
                {
                    Console.WriteLine("Some thread paused");
                    Monitor.Wait(_appLocker);
                }
            }
        }

        Console.WriteLine("Some thread stopped");
        lock (_executeLocker)
        {
            _waitTaskTime += waitTimer.ElapsedMilliseconds;
            _executingTaskTime += executingTimer.ElapsedMilliseconds;
        }
    }

    public void InitiateThreads()
    {
        if (_amountOfQueue <= 0 || _threadsPerQueue <= 0)
            throw new ArgumentOutOfRangeException();

        for (int i = 0; i < _amountOfQueue; i++)
        {
            int k = i;
            for (int j = 0; j < _threadsPerQueue; j++)
                _threadsList.Add(new Thread(() => ExecuteTask(k)));
        }
    }

    public void AddTask(Action func)
    {
        lock (_executeLocker)
        {
            var mostFreeQueue = _queueList.OrderBy(queue => queue.Count).FirstOrDefault();
            mostFreeQueue?.Enqueue(func);
            Monitor.PulseAll(_executeLocker);
        }
    }

    public void StartExecuting()
    {
        if (_threadsList.Count == 0)
            throw new Exception("Threads list is empty");

        foreach (var thread in _threadsList)
            thread.Start();
    }
    
    public void WaitAllTasks()
    {
        lock (_executeLocker)
        {
            while (_queueList.Any(queue => queue.Count > 0) || _executingTaskCounter > 0)
            {
                Monitor.Wait(_executeLocker);
            }
        }
    }

    public void PauseExecuting()
    {
        lock (_executeLocker)
        {
            _isContinueExecuting = false;
            Monitor.PulseAll(_executeLocker);
        }
    }

    public void RestartExecuting()
    {
        lock (_appLocker)
        {
            _isContinueExecuting = true;
            Monitor.PulseAll(_appLocker);
        }
    }

    public void StopExecuting()
    {
        lock (_executeLocker)
        {
            lock (_appLocker)
            {
                _isRunningApp = false;
                _isContinueExecuting = false;
                Monitor.PulseAll(_executeLocker);
                Monitor.PulseAll(_appLocker);
            }
        }

        foreach (var thread in _threadsList)
            thread.Join();

        Console.WriteLine("All threads were stopped");
    }
}