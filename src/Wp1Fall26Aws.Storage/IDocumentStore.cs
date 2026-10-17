using System.Threading;
using System.Threading.Tasks;

namespace Wp1Fall26Aws.Storage;

/// <summary>
/// Defines the abstraction for persisting document streams and metadata to an object storage system.
/// </summary>
/// <remarks>
/// Implementations handle file validation, object key partitioning, metadata tagging,
/// and uploading binary payloads to the underlying storage provider (e.g., Amazon S3).
/// </remarks>
public interface IDocumentStore
{
    /// <summary>
    /// Validates and asynchronously persists a document upload payload to the object store.
    /// </summary>
    /// <param name="upload">The document upload metadata and data stream to store.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the operation to complete.</param>
    /// <returns>
    /// A <see cref="StoreResult"/> indicating whether the operation succeeded, including the generated
    /// object key and assigned metadata on success, or validation/storage errors on failure.
    /// </returns>
    Task<StoreResult> StoreAsync(DocumentUpload upload, CancellationToken ct = default);
}
