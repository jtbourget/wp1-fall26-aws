using System;
using System.Collections.Generic;

namespace Wp1Fall26Aws.Storage;

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

public sealed record CreateDocumentRequest(
    string Title,
    string Description,
    string? FileName = null,
    string? ContentType = null,
    string? Content = null);

public sealed record UpdateDocumentRequest(
    string Title,
    string Description);
