using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Wp1Fall26Aws.Storage;

/// <summary>
/// Provides a repository contract for managing document metadata records in application storage.
/// </summary>
/// <remarks>
/// This interface abstracts the persistence mechanism for indexing and retrieving document
/// records, supporting full CRUD (Create, Read, Update, Delete) lifecycle operations.
/// </remarks>
public interface IDocumentRepository
{
    /// <summary>
    /// Retrieves all document records ordered by creation date descending.
    /// </summary>
    /// <param name="ct">A cancellation token to observe while waiting for the operation to complete.</param>
    /// <returns>A read-only list containing all stored document records.</returns>
    Task<IReadOnlyList<DocumentRecord>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Retrieves a specific document record by its unique identifier or S3 object key.
    /// </summary>
    /// <param name="id">The document record identifier or its corresponding S3 object key.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the operation to complete.</param>
    /// <returns>The matching <see cref="DocumentRecord"/> if found; otherwise, <c>null</c>.</returns>
    Task<DocumentRecord?> GetByIdAsync(string id, CancellationToken ct = default);

    /// <summary>
    /// Persists a new document record in the repository.
    /// </summary>
    /// <param name="record">The document record to create and index.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the operation to complete.</param>
    /// <returns>The persisted <see cref="DocumentRecord"/>.</returns>
    Task<DocumentRecord> AddAsync(DocumentRecord record, CancellationToken ct = default);

    /// <summary>
    /// Updates editable metadata (e.g., Title and Description) of an existing document record.
    /// </summary>
    /// <param name="id">The unique identifier of the document to update.</param>
    /// <param name="request">The update payload containing modified title and description values.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the operation to complete.</param>
    /// <returns>The updated <see cref="DocumentRecord"/> if found and updated; otherwise, <c>null</c>.</returns>
    Task<DocumentRecord?> UpdateAsync(string id, UpdateDocumentRequest request, CancellationToken ct = default);

    /// <summary>
    /// Removes a document record from the repository by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the document to delete.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the operation to complete.</param>
    /// <returns><c>true</c> if the document was found and removed; otherwise, <c>false</c>.</returns>
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);
}
