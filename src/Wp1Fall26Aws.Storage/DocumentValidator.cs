using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Wp1Fall26Aws.Storage;

public static class DocumentValidator
{
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
        var allowedExtensions = new[] { ".txt", ".pdf", ".png", ".jpg", ".jpeg" };
        if (string.IsNullOrEmpty(ext) || !allowedExtensions.Contains(ext))
        {
            errors.Add($"Extension '{ext}' is not allowed.");
        }

        var allowedContentTypes = new[] { "text/plain", "application/pdf", "image/png", "image/jpeg" };
        if (!allowedContentTypes.Contains(upload.ContentType))
        {
            errors.Add($"Content type '{upload.ContentType}' is not allowed.");
        }

        return errors;
    }
}
