using System.Collections.Generic;
using AhuErp.Core.Models;

namespace AhuErp.Core.Services
{
    /// <summary>
    /// Абстракция доступа к документам. CRUD-операции простые и синхронные —
    /// UI-клиент Phase 2 использует их напрямую из ViewModel.
    /// </summary>
    public interface IDocumentRepository
    {
        IReadOnlyList<Document> ListByType(DocumentType type);
        IReadOnlyList<ArchiveRequest> ListArchiveRequests();
        Document GetById(int id);
        void Add(Document document);
        void Update(Document document);
        void Remove(int id);
    }
}
