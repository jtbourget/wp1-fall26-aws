using System.Collections.Generic;

namespace Wp1Fall26Aws.Storage;

/// <summary>
/// Represents the outcome of an attempt to store a document via <see cref="IDocumentStore"/>.
/// </summary>
/// <param name="Success">Indicates whether the upload validation and S3 upload succeeded.</param>
/// <param name="ObjectKey">The generated S3 object key if successful, or null on failure.</param>
/// <param name="Errors">A collection of validation or processing error messages if the operation failed.</param>
/// <param name="Metadata">The user-defined S3 metadata key-value pairs assigned to the object if successful.</param>
public sealed record StoreResult(
    bool Success,
    string? ObjectKey,
    IReadOnlyList<string> Errors,
    IReadOnlyDictionary<string,string>? Metadata);
