using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Wp1Fall26Aws.Storage;

/// <summary>
/// Provides document storage orchestration by validating uploads, generating hierarchical
/// partitioned S3 object keys, assembling metadata headers, and persisting objects to S3.
/// </summary>
/// <remarks>
/// Key Partitioning Strategy:
/// <c>{KeyPrefix}/{yyyy}/{MM}/{dd}/{guid:N}{.extension}</c>
/// This organizes uploads by year, month, and day while guaranteeing uniqueness with 32-character hex GUIDs.
/// </remarks>
public class S3DocumentStore : IDocumentStore
{
    private readonly IS3ObjectClient _client;
    private readonly S3StorageOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="S3DocumentStore"/> class.
    /// </summary>
    /// <param name="client">The underlying S3 client boundary service.</param>
    /// <param name="options">Configured S3 storage options (bucket name, prefix, max size).</param>
    public S3DocumentStore(IS3ObjectClient client, IOptions<S3StorageOptions> options)
    {
        _client = client;
        _options = options.Value;
    }

    /// <summary>
    /// Validates an incoming document upload and asynchronously uploads the file to S3 if valid.
    /// </summary>
    /// <param name="upload">The document upload containing filename, content type, length, and content stream.</param>
    /// <param name="ct">A cancellation token for the asynchronous operation.</param>
    /// <returns>
    /// A <see cref="StoreResult"/> containing:
    /// <list type="bullet">
    ///   <item><description><c>Success = true</c>, generated <c>ObjectKey</c>, and S3 <c>Metadata</c> if valid.</description></item>
    ///   <item><description><c>Success = false</c> and validation <c>Errors</c> if rejected (S3 client is never invoked).</description></item>
    /// </list>
    /// </returns>
    public async Task<StoreResult> StoreAsync(DocumentUpload upload, CancellationToken ct = default)
    {
        var validationErrors = DocumentValidator.Validate(upload, _options);
        if (validationErrors.Count > 0)
        {
            return new StoreResult(false, null, validationErrors, null);
        }

        var ext = Path.GetExtension(upload.FileName).ToLowerInvariant();
        var datePath = DateTime.UtcNow.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
        var guid = Guid.NewGuid().ToString("N");
        var key = $"{_options.KeyPrefix}/{datePath}/{guid}{ext}";

        var metadata = new Dictionary<string, string>
        {
            { "original-filename", upload.FileName },
            { "content-type", upload.ContentType },
            { "uploaded-utc", DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture) },
            { "content-length", upload.ContentLength.ToString(CultureInfo.InvariantCulture) }
        };

        await _client.PutObjectAsync(
            _options.BucketName,
            key,
            upload.Content,
            upload.ContentType,
            metadata,
            ct);

        return new StoreResult(true, key, Array.Empty<string>(), metadata);
    }
}
