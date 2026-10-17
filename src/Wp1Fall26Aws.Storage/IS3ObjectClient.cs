using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Wp1Fall26Aws.Storage;

/// <summary>
/// Defines an abstraction boundary for executing object storage mutations against Amazon S3.
/// </summary>
/// <remarks>
/// This interface decouples application logic from direct AWS SDK dependencies, allowing
/// test suites to easily mock or spy on S3 interactions without live cloud credentials.
/// </remarks>
public interface IS3ObjectClient
{
    /// <summary>
    /// Uploads an object data stream to the designated S3 bucket and object key.
    /// </summary>
    /// <param name="bucket">The target S3 bucket name.</param>
    /// <param name="key">The target partitioned S3 object key.</param>
    /// <param name="content">The readable content stream containing the file bytes.</param>
    /// <param name="contentType">The MIME content type of the object (e.g. "application/pdf").</param>
    /// <param name="metadata">A dictionary of user-defined metadata key-value pairs attached to the S3 object.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the upload to complete.</param>
    /// <returns>A task that completes once the object upload has finished.</returns>
    Task PutObjectAsync(
        string bucket,
        string key,
        Stream content,
        string contentType,
        IReadOnlyDictionary<string,string> metadata,
        CancellationToken ct = default);
}
