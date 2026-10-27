using System.Text.Json;
using Microsoft.Extensions.Options;
using SlaeSolver.BLL.Models;
using SlaeSolver.BLL.Options;

namespace SlaeSolver.BLL.Storage;

public class LocalFileStorage: IFileStorage
{
    private readonly string _root;

    public LocalFileStorage(IOptions<StorageOptions> options)
    {
        _root = Path.GetFullPath(options.Value.Root);
        Directory.CreateDirectory(Path.Combine(_root, "params"));
        Directory.CreateDirectory(Path.Combine(_root, "results"));
    }
    
    public async Task<string> SaveParametersAsync(Guid taskId, TaskParameters parameters)
    {
        string relativePath = $"params/{taskId}.json";
        string fullPath = Path.Combine(_root, relativePath);
        await using var fs = File.Create(fullPath);
        await JsonSerializer.SerializeAsync(fs, parameters);

        return relativePath;
    }

    public async Task<TaskParameters> ReadParametersAsync(string relativePath)
    {
        string fullPath = Path.Combine(_root, relativePath);
        await using var fs = File.OpenRead(fullPath);
        var parameters = await JsonSerializer.DeserializeAsync<TaskParameters>(fs);
        if (parameters is null)
        {
            throw new InvalidOperationException("Файл параметрів пошкоджено");
        }

        return parameters;
    }
}