using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Wp1Fall26Aws.Storage;

public class S3DocumentStore : IDocumentStore
{
    private readonly IS3ObjectClient _client;
    private readonly S3StorageOptions _options;

    public S3DocumentStore(IS3ObjectClient client, IOptions<S3StorageOptions> options)
    {
        _client = client;
        _options = options.Value;
    }

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
