using System.Net;
using System.Text.Json;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using SlaeSolver.BLL.Models;
using SlaeSolver.BLL.Options;
using SlaeSolver.BLL.Solver;

namespace SlaeSolver.BLL.Storage;

public class S3FileStorage: IFileStorage
{
    private readonly IAmazonS3 _s3;
    private readonly string _bucket;

    public  S3FileStorage(IAmazonS3 s3,IOptions<StorageOptions> options)
    {
        _s3 = s3;
        _bucket = options.Value.Bucket;
    }

    public async Task<string> SaveParametersAsync(Guid taskId, TaskParameters parameters)
    {
        var key = $"params/{taskId}.json";
        await PutJsonAsync(key, parameters);
        return key;
    }

    public async Task<TaskParameters> ReadParametersAsync(string relativePath)
    {
        var parameters = await GetJsonAsync<TaskParameters>(relativePath);
        return parameters ?? throw new InvalidOperationException("Файл параметрів пошкоджено");
    }

    public async Task<string> SaveResultAsync(Guid taskId, SolverResult result)
    {
        var key = $"results/{taskId}.json";
        await PutJsonAsync(key, result);
        return key;
    }

    public async Task<SolverResult> ReadResultAsync(string relativePath)
    {
        var parameters = await GetJsonAsync<SolverResult>(relativePath);
        return parameters ?? throw new InvalidOperationException("Файл результатів пошкоджено");
    }

    private async Task PutJsonAsync<T>(string key, T value)
    {
        using var stream = new MemoryStream();
        await JsonSerializer.SerializeAsync(stream, value);
        stream.Position = 0;

        var request = new PutObjectRequest
        {
            BucketName = _bucket,
            Key = key,
            InputStream = stream,
            ContentType = "application/json"
        };
        await _s3.PutObjectAsync(request);
    }

    private async Task<T?> GetJsonAsync<T>(string key)
    {
        try
        {
            using var response = await _s3.GetObjectAsync(_bucket,key);
            return await JsonSerializer.DeserializeAsync<T>(response.ResponseStream);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            throw new InvalidOperationException($"Файл {key} не знайдено у сховищі");
        }
        
    }
}