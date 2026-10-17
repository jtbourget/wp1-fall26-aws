using System;
using System.Collections.Generic;

namespace Wp1Fall26Aws.Storage;

/// <summary>
/// Represents the persisted metadata for a document stored in the repository and AWS S3.
/// </summary>
/// <param name="Id">The unique repository record identifier (e.g. GUID hex string or "doc-1").</param>
/// <param name="ObjectKey">The full S3 object key (e.g. "documents/uploads/2026/10/12/uuid.pdf").</param>
/// <param name="FileName">The original file name as provided during upload.</param>
/// <param name="ContentType">The MIME content type (e.g. "application/pdf", "text/plain").</param>
/// <param name="ContentLength">The file size in bytes.</param>
/// <param name="UploadedUtc">The UTC timestamp when the document was initially uploaded.</param>
/// <param name="Title">The user-assigned or derived document title.</param>
/// <param name="Description">An optional descriptive summary of the document contents.</param>
/// <param name="UpdatedUtc">The UTC timestamp when metadata was last modified, or null if unmodified.</param>
/// <param name="Metadata">Optional dictionary containing user-defined S3 object metadata headers.</param>
public sealed record DocumentRecord(
    string Id,
    string ObjectKey,
    string FileName,
    string ContentType,
    long ContentLength,
    DateTime UploadedUtc,
    string Title,
    string Description,
    DateTime? UpdatedUtc = null,
    IReadOnlyDictionary<string, string>? Metadata = null);

/// <summary>
/// Represents the request payload for creating a document via raw JSON POST.
/// </summary>
/// <param name="Title">The required user-facing title of the document.</param>
/// <param name="Description">An optional description of the document.</param>
/// <param name="FileName">An optional custom file name; defaults to a generated GUID .txt name if omitted.</param>
/// <param name="ContentType">An optional MIME type; defaults to "text/plain" if omitted.</param>
/// <param name="Content">The raw string content to write into the document file stream.</param>
public sealed record CreateDocumentRequest(
    string Title,
    string Description,
    string? FileName = null,
    string? ContentType = null,
    string? Content = null);

/// <summary>
/// Represents the request payload for updating an existing document's editable metadata.
/// </summary>
/// <param name="Title">The updated document title (required).</param>
/// <param name="Description">The updated document description.</param>
public sealed record UpdateDocumentRequest(
    string Title,
    string Description);
