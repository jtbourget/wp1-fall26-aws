using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Wp1Fall26Aws.Storage;

namespace Wp1Fall26Aws.Tests;

/// <summary>
/// A test spy implementing <see cref="IS3ObjectClient"/> that captures and stores the most recent
/// PutObject invocation details for test assertion.
/// </summary>
/// <remarks>
/// Test spies record indirect outputs from the system under test (SUT) without needing complex mocking libraries.
/// In unit and integration tests, asserting against <see cref="LastRequest"/> allows verification that
/// the storage layer correctly formulated the S3 bucket, key, and metadata headers.
/// </remarks>
public sealed class RecordingS3ObjectClientSpy : IS3ObjectClient
{
    /// <summary>
    /// Gets the details of the most recent PutObject invocation, or <c>null</c> if no call was made.
    /// </summary>
    public PutObjectRequest? LastRequest { get; private set; }

    /// <summary>
    /// Records the arguments of the PutObject request and returns a completed task.
    /// </summary>
    /// <param name="bucket">Target bucket name.</param>
    /// <param name="key">S3 object key.</param>
    /// <param name="content">File byte stream.</param>
    /// <param name="contentType">MIME type.</param>
    /// <param name="metadata">Object metadata.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A completed task.</returns>
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

    /// <summary>
    /// Immutable record holding the arguments captured during a <see cref="PutObjectAsync"/> call.
    /// </summary>
    /// <param name="Bucket">The destination S3 bucket name.</param>
    /// <param name="Key">The partitioned S3 object key.</param>
    /// <param name="Content">The file data stream.</param>
    /// <param name="ContentType">The file MIME type.</param>
    /// <param name="Metadata">The metadata dictionary attached to the upload.</param>
    public sealed record PutObjectRequest(
        string Bucket,
        string Key,
        Stream Content,
        string ContentType,
        IReadOnlyDictionary<string, string> Metadata);
}
