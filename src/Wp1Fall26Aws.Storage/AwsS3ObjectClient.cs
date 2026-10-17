using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;

namespace Wp1Fall26Aws.Storage;

/// <summary>
/// Implements <see cref="IS3ObjectClient"/> using the AWS SDK for .NET (<see cref="IAmazonS3"/>).
/// </summary>
/// <remarks>
/// Handles uploading files directly to an Amazon S3 bucket, applying custom user metadata
/// to the S3 object header. When running in local development without a configured bucket,
/// the client safely bypasses live network calls to prevent unauthenticated crashes.
/// </remarks>
public class AwsS3ObjectClient : IS3ObjectClient
{
    private readonly IAmazonS3 _s3Client;

    /// <summary>
    /// Initializes a new instance of the <see cref="AwsS3ObjectClient"/> class.
    /// </summary>
    /// <param name="s3Client">The AWS S3 client instance provided via dependency injection.</param>
    public AwsS3ObjectClient(IAmazonS3 s3Client)
    {
        _s3Client = s3Client;
    }

    /// <summary>
    /// Constructs and sends a <see cref="PutObjectRequest"/> to Amazon S3.
    /// </summary>
    /// <param name="bucket">The target S3 bucket name. If null or whitespace, the upload is skipped (local mock mode).</param>
    /// <param name="key">The partitioned key identifier for the object.</param>
    /// <param name="content">The readable file byte stream.</param>
    /// <param name="contentType">The MIME type of the uploaded file.</param>
    /// <param name="metadata">Custom metadata dictionary stored alongside the S3 object.</param>
    /// <param name="ct">Cancellation token for the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous S3 put object operation.</returns>
    public async Task PutObjectAsync(
        string bucket,
        string key,
        Stream content,
        string contentType,
        IReadOnlyDictionary<string, string> metadata,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(bucket))
        {
            // Offline / local development without configured bucket
            return;
        }

        var request = new PutObjectRequest
        {
            BucketName = bucket,
            Key = key,
            InputStream = content,
            ContentType = contentType
        };

        foreach (var kvp in metadata)
        {
            request.Metadata[kvp.Key] = kvp.Value;
        }

        await _s3Client.PutObjectAsync(request, ct);
    }
}
