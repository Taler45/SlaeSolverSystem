using LoadBalancer.API.Models;
using LoadBalancer.API.Options;
using Microsoft.Extensions.Options;

namespace LoadBalancer.API.Services;

public class ServerRegistry
{
    private readonly List<BackendServer> _servers = new();
    private readonly object _lock = new();
    private int _next;


    public ServerRegistry(IOptions<LoadBalancerOptions> options)
    {
        foreach (var server in options.Value.Servers)
        {
            Add(server.Id, server.Url);
        }
    }
    
    public bool Add(string id, string url)
    {
        lock (_lock)
        {
            if (_servers.Any(s => s.Id == id)) return false;
            _servers.Add(new BackendServer(id,url));
            return true;
        }
    }

    public List<(string Id, string Url)> GetTargets() 
    {
        lock (_lock)
        {
            return _servers.Select(s => (s.Id, s.Url)).ToList();
        }
    }
    
    private BackendServer? Find(string id)
    {
        return _servers.FirstOrDefault(s => s.Id == id);
    }

    public bool UpdateLoad(string id, ServerLoad load)
    {
        lock (_lock)
        {
            var server = Find(id);
            if (server is null)
            {
                return false;
            }

            bool wasDead = !server.IsAlive;

            server.ActiveTasks = load.ActiveTasks;
            server.MaxTasks = load.MaxTasks;
            server.FreeSlots = load.FreeSlots;
            server.TotalWeight = load.TotalWeight;
            server.IsAlive = true;
            server.LastSeenAt = DateTime.UtcNow;

            return wasDead;
        }
    }

    public bool MarkDead(string id)
    {
        lock (_lock)
        {
            var server = Find(id);
            if (server is null || !server.IsAlive)
            {
                return false;
            }

            server.IsAlive = false;
            return true;
        }
    }


    public bool Remove(string id)
    {
        lock (_lock)
        {
            var remove = Find(id);
            if (remove is null)
            {
                return false;
            }
            return _servers.Remove(remove);
        }
    }

    public List<ServerStatusDto> Snapshot()
    {
        lock (_lock)
        {
            return _servers.Select(s => new ServerStatusDto(s.Id, s.IsAlive, s.ActiveTasks, s.MaxTasks, s.FreeSlots,
                s.TotalWeight, s.LastSeenAt)).ToList();
        }
    }

    public BackendServer? ChooseAny(string? expectedId = null)
    {
        lock (_lock)
        {
            var cirkleSer = _servers.Where(s => (s.Id != expectedId) && (s.IsAlive == true)).ToList();
            if (cirkleSer.Count == 0)
            {
                return null;
            }

            var server = cirkleSer[_next % cirkleSer.Count()];
            _next++;
            return server;
        }
    }

    public BackendServer? ChooseForNewTask(int matrixSize)
    {
        lock (_lock)
        {
            var alive = _servers.Where(s => s.IsAlive).ToList();
            if (alive.Count == 0)
            {
                return null;                    
            }
            var withSlots = alive.Where(s => s.FreeSlots > 0).ToList();
            var candidates = withSlots.Count > 0 ? withSlots : alive;  

            var chosen = candidates.OrderBy(s => s.TotalWeight).First();

            chosen.TotalWeight += (long)matrixSize * matrixSize;
            if (chosen.FreeSlots > 0)             
            {
                chosen.FreeSlots--;
                chosen.ActiveTasks++;
            }

            return chosen;
        }
    }

}