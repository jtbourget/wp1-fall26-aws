using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Wp1Fall26Aws.Storage;

namespace Wp1Fall26Aws.Tests;

/// <summary>
/// Represents a recorded call to <see cref="IS3ObjectClient.PutObjectAsync"/>.
/// </summary>
public class PutObjectCall
{
    /// <summary>Gets or sets the target S3 bucket name.</summary>
    public string Bucket { get; set; } = "";
    /// <summary>Gets or sets the S3 object key.</summary>
    public string Key { get; set; } = "";
    /// <summary>Gets or sets the MIME content type.</summary>
    public string ContentType { get; set; } = "";
    /// <summary>Gets or sets the metadata dictionary passed to S3.</summary>
    public IReadOnlyDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}

/// <summary>
/// A test fake implementing <see cref="IS3ObjectClient"/> that records all PutObjectAsync invocations in memory.
/// </summary>
/// <remarks>
/// Used in unit tests to verify interaction counts, bucket arguments, and metadata headers
/// without requiring a live Amazon S3 connection.
/// </remarks>
public class FakeS3ObjectClient : IS3ObjectClient
{
    /// <summary>
    /// Gets the list of recorded put object invocations.
    /// </summary>
    public List<PutObjectCall> Calls { get; } = new();

    /// <summary>
    /// Records the put object call into <see cref="Calls"/> and returns a completed task.
    /// </summary>
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
