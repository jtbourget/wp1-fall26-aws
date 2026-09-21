using System.Threading;
using System.Threading.Tasks;

namespace Wp1Fall26Aws.Storage;

public interface IDocumentStore
{
    Task<StoreResult> StoreAsync(DocumentUpload upload, CancellationToken ct = default);
}
