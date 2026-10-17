using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Wp1Fall26Aws.Storage;

/// <summary>
/// Provides policy enforcement and validation rules for document uploads prior to storage in S3.
/// </summary>
/// <remarks>
/// Validation rules protect AWS resources from invalid, corrupted, or potentially malicious file uploads.
/// Enforces file size boundaries (non-empty up to maximum configured bytes), file extension allowlists,
/// and MIME content-type allowlists.
/// </remarks>
public static class DocumentValidator
{
    private static readonly string[] AllowedExtensions = { ".txt", ".pdf", ".png", ".jpg", ".jpeg" };
    private static readonly string[] AllowedContentTypes = { "text/plain", "application/pdf", "image/png", "image/jpeg" };

    /// <summary>
    /// Validates an incoming <see cref="DocumentUpload"/> against the configured storage constraints.
    /// </summary>
    /// <param name="upload">The upload details to inspect (file name, content type, length).</param>
    /// <param name="options">The storage options containing <see cref="S3StorageOptions.MaxFileSizeBytes"/>.</param>
    /// <returns>
    /// A list of human-readable validation error messages. If valid, the list is empty.
    /// </returns>
    public static IReadOnlyList<string> Validate(DocumentUpload upload, S3StorageOptions options)
    {
        var errors = new List<string>();

        if (upload.ContentLength == 0)
        {
            errors.Add("File is empty.");
        }

        if (upload.ContentLength > options.MaxFileSizeBytes)
        {
            errors.Add($"File exceeds the maximum size of {options.MaxFileSizeBytes} bytes.");
        }

        var ext = Path.GetExtension(upload.FileName)?.ToLowerInvariant();
        if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
        {
            errors.Add($"Extension '{ext}' is not allowed.");
        }

        if (!AllowedContentTypes.Contains(upload.ContentType))
        {
            errors.Add($"Content type '{upload.ContentType}' is not allowed.");
        }

        return errors;
    }
}
