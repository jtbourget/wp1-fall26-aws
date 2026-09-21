using System.IO;

namespace Wp1Fall26Aws.Storage;

public sealed record DocumentUpload(
    string FileName,
    string ContentType,
    long ContentLength,
    Stream Content);
