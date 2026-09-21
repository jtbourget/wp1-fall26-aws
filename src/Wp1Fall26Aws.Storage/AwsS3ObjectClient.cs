using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;

namespace Wp1Fall26Aws.Storage;

public class AwsS3ObjectClient : IS3ObjectClient
{
    private readonly IAmazonS3 _s3Client;

    public AwsS3ObjectClient(IAmazonS3 s3Client)
    {
        _s3Client = s3Client;
    }

    public async Task PutObjectAsync(
        string bucket,
        string key,
        Stream content,
        string contentType,
        IReadOnlyDictionary<string, string> metadata,
        CancellationToken ct = default)
    {
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
