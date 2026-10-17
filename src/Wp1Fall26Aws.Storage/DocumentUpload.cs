using System.IO;

namespace Wp1Fall26Aws.Storage;

/// <summary>
/// Encapsulates the metadata and stream payload of a file submitted for storage.
/// </summary>
/// <param name="FileName">The raw file name including extension.</param>
/// <param name="ContentType">The MIME content type asserted by the client.</param>
/// <param name="ContentLength">The length of the content stream in bytes.</param>
/// <param name="Content">The readable binary stream containing the document content.</param>
public sealed record DocumentUpload(
    string FileName,
    string ContentType,
    long ContentLength,
    Stream Content);
