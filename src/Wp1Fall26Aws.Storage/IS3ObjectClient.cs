using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Wp1Fall26Aws.Storage;

public interface IS3ObjectClient
{
    Task PutObjectAsync(
        string bucket,
        string key,
        Stream content,
        string contentType,
        IReadOnlyDictionary<string,string> metadata,
        CancellationToken ct = default);
}
