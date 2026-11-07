using System.Collections.Concurrent;

namespace SlaeSolver.BLL.Execution;

public class RunningTasksRegistry
{
    private readonly ConcurrentDictionary<Guid, int> _tasks = new();

    public int ActiveCount
    {
        get
        {
            return _tasks.Count;
        }
    }

    public long TotalWeight
    {
        get
        {
            long result = 0;
            foreach (var v in _tasks.Values)
            {
                result += (long)v * v;
            }
            return result;
        }
    }

    public void Add(Guid taskId, int matrixSize)
    {
        _tasks[taskId] = matrixSize;
    }
    
    public bool Remove(Guid taskId)
    {
        return _tasks.TryRemove(taskId, out _);
    }
}