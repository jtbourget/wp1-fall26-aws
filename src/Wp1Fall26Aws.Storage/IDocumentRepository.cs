using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Wp1Fall26Aws.Storage;

public interface IDocumentRepository
{
    Task<IReadOnlyList<DocumentRecord>> GetAllAsync(CancellationToken ct = default);
    Task<DocumentRecord?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<DocumentRecord> AddAsync(DocumentRecord record, CancellationToken ct = default);
    Task<DocumentRecord?> UpdateAsync(string id, UpdateDocumentRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);
}
