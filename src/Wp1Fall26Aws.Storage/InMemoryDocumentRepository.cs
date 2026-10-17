using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Wp1Fall26Aws.Storage;

/// <summary>
/// A thread-safe, in-memory implementation of <see cref="IDocumentRepository"/> backed by <see cref="ConcurrentDictionary{TKey,TValue}"/>.
/// </summary>
/// <remarks>
/// This repository pre-populates initial sample documents to provide an out-of-the-box working state
/// for local testing, video demonstrations, and automated integration tests. All CRUD operations mutate
/// the dictionary concurrently and safely.
/// </remarks>
public class InMemoryDocumentRepository : IDocumentRepository
{
    private readonly ConcurrentDictionary<string, DocumentRecord> _documents = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryDocumentRepository"/> class
    /// and populates it with sample initial records for demonstration and testing.
    /// </summary>
    public InMemoryDocumentRepository()
    {
        // Seed with sample initial records so consumer app has live data on first load,
        // but items can be updated, deleted (leading to empty state), or added to.
        var sample1 = new DocumentRecord(
            Id: "doc-1",
            ObjectKey: "documents/uploads/2026/10/12/sample-project-spec.pdf",
            FileName: "project-spec.pdf",
            ContentType: "application/pdf",
            ContentLength: 104857,
            UploadedUtc: DateTime.UtcNow.AddHours(-2),
            Title: "Project Specification",
            Description: "Architecture and design documentation for cloud storage service.",
            UpdatedUtc: null,
            Metadata: new Dictionary<string, string> { { "original-filename", "project-spec.pdf" } }
        );

        var sample2 = new DocumentRecord(
            Id: "doc-2",
            ObjectKey: "documents/uploads/2026/10/12/sample-readme.txt",
            FileName: "readme.txt",
            ContentType: "text/plain",
            ContentLength: 2048,
            UploadedUtc: DateTime.UtcNow.AddMinutes(-45),
            Title: "Release Notes & Guide",
            Description: "Deployment guide and endpoint documentation for consumer app.",
            UpdatedUtc: null,
            Metadata: new Dictionary<string, string> { { "original-filename", "readme.txt" } }
        );

        _documents[sample1.Id] = sample1;
        _documents[sample2.Id] = sample2;
    }

    /// <summary>
    /// Retrieves all documents ordered by <see cref="DocumentRecord.UploadedUtc"/> descending.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A read-only snapshot list of documents.</returns>
    public Task<IReadOnlyList<DocumentRecord>> GetAllAsync(CancellationToken ct = default)
    {
        IReadOnlyList<DocumentRecord> list = _documents.Values
            .OrderByDescending(d => d.UploadedUtc)
            .ToList();
        return Task.FromResult(list);
    }

    /// <summary>
    /// Retrieves a document by its unique Id or its S3 ObjectKey.
    /// </summary>
    /// <param name="id">The document record identifier or S3 object key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The matching document record if found; otherwise, null.</returns>
    public Task<DocumentRecord?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        if (_documents.TryGetValue(id, out var record))
        {
            return Task.FromResult<DocumentRecord?>(record);
        }

        var byKey = _documents.Values.FirstOrDefault(d => d.ObjectKey == id);
        return Task.FromResult(byKey);
    }

    /// <summary>
    /// Adds a new document record to the concurrent dictionary.
    /// </summary>
    /// <param name="record">The record to store.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The stored document record.</returns>
    public Task<DocumentRecord> AddAsync(DocumentRecord record, CancellationToken ct = default)
    {
        _documents[record.Id] = record;
        return Task.FromResult(record);
    }

    /// <summary>
    /// Updates the Title and Description of an existing record, stamping the current UTC timestamp into <see cref="DocumentRecord.UpdatedUtc"/>.
    /// </summary>
    /// <param name="id">The document identifier to update.</param>
    /// <param name="request">The updated title and description.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The updated document record, or null if the document did not exist.</returns>
    public Task<DocumentRecord?> UpdateAsync(string id, UpdateDocumentRequest request, CancellationToken ct = default)
    {
        if (!_documents.TryGetValue(id, out var existing))
        {
            return Task.FromResult<DocumentRecord?>(null);
        }

        var updated = existing with
        {
            Title = request.Title,
            Description = request.Description,
            UpdatedUtc = DateTime.UtcNow
        };

        _documents[id] = updated;
        return Task.FromResult<DocumentRecord?>(updated);
    }

    /// <summary>
    /// Deletes a document record by its unique identifier.
    /// </summary>
    /// <param name="id">The document identifier to remove.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if the item was removed; otherwise, false.</returns>
    public Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        var removed = _documents.TryRemove(id, out _);
        return Task.FromResult(removed);
    }
}
