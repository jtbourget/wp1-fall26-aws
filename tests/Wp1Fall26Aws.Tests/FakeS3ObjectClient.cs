using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Wp1Fall26Aws.Storage;

namespace Wp1Fall26Aws.Tests;

public class PutObjectCall
{
    public string Bucket { get; set; } = "";
    public string Key { get; set; } = "";
    public string ContentType { get; set; } = "";
    public IReadOnlyDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}

public class FakeS3ObjectClient : IS3ObjectClient
{
    public List<PutObjectCall> Calls { get; } = new();

    public Task PutObjectAsync(
        string bucket,
        string key,
        Stream content,
        string contentType,
        IReadOnlyDictionary<string, string> metadata,
        CancellationToken ct = default)
    {
        Calls.Add(new PutObjectCall
        {
            Bucket = bucket,
            Key = key,
            ContentType = contentType,
            Metadata = metadata
        });
        return Task.CompletedTask;
    }
}
