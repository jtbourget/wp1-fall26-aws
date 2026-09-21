using System.Collections.Generic;

namespace Wp1Fall26Aws.Storage;

public sealed record StoreResult(
    bool Success,
    string? ObjectKey,
    IReadOnlyList<string> Errors,
    IReadOnlyDictionary<string,string>? Metadata);
