using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Wp1Fall26Aws.Storage;

namespace Wp1Fall26Aws.Tests;

/// <summary>
/// A spy for the S3 object client to record the last put request.
/// Returns a successful Task when called.
/// </summary>
public sealed class RecordingS3ObjectClientSpy : IS3ObjectClient
{
    public PutObjectRequest? LastRequest { get; private set; }

    public Task PutObjectAsync(
        string bucket,
        string key,
        Stream content,
        string contentType,
        IReadOnlyDictionary<string, string> metadata,
        CancellationToken ct = default)
    {
        LastRequest = new PutObjectRequest(bucket, key, content, contentType, metadata);
        return Task.CompletedTask;
    }

    public sealed record PutObjectRequest(
        string Bucket,
        string Key,
        Stream Content,
        string ContentType,
        IReadOnlyDictionary<string, string> Metadata);
}
