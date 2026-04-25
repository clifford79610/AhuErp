using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AhuErp.Core.Data;
using AhuErp.Core.Models;

namespace AhuErp.Core.Services
{
    /// <summary>
    /// EF6-реализация <see cref="IDocumentRepository"/> поверх <see cref="AhuDbContext"/>.
    /// Использует длинноживущий контекст (singleton в DI), все обращения — с UI-потока
    /// (см. <see cref="ViewModels.DashboardViewModel"/>: снимок данных перед Task.Run).
    /// TPH-наследники (<see cref="ArchiveRequest"/>, <see cref="ItTicket"/>) живут в той
    /// же таблице <c>Documents</c> и различаются дискриминатором <c>DocumentKind</c>.
    /// </summary>
    public sealed class EfDocumentRepository : IDocumentRepository
    {
        private readonly AhuDbContext _ctx;

        public EfDocumentRepository(AhuDbContext ctx)
        {
            _ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
        }

        public IReadOnlyList<Document> ListByType(DocumentType type)
        {
            return _ctx.Documents
                .Where(d => d.Type == type
                            && !(d is ArchiveRequest)
                            && !(d is ItTicket))
                .ToList()
                .AsReadOnly();
        }

        public IReadOnlyList<ArchiveRequest> ListArchiveRequests()
        {
            return _ctx.Documents.OfType<ArchiveRequest>().ToList().AsReadOnly();
        }

        public IReadOnlyList<ItTicket> ListItTickets()
        {
            return _ctx.Documents.OfType<ItTicket>().ToList().AsReadOnly();
        }

        public IReadOnlyList<Document> ListInventoryEligibleDocuments()
        {
            return _ctx.Documents
                .Where(d => d.Type == DocumentType.Internal
                            || d.Type == DocumentType.It
                            || d is ItTicket)
                .ToList()
                .AsReadOnly();
        }

        public Document GetById(int id) => _ctx.Documents.Find(id);

        public void Add(Document document)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));
            _ctx.Documents.Add(document);
            _ctx.SaveChanges();
        }

        public void Update(Document document)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));
            // Если сущность уже в трекере (типичный сценарий — её достали через Find/List
            // и тут же редактируют) — EF6 уже знает об изменениях. Если detached
            // (после рестарта контекста или при ручной сборке) — присоединяем явно.
            if (_ctx.Entry(document).State == EntityState.Detached)
            {
                _ctx.Documents.Attach(document);
                _ctx.Entry(document).State = EntityState.Modified;
            }
            _ctx.SaveChanges();
        }

        public void Remove(int id)
        {
            var doc = _ctx.Documents.Find(id);
            if (doc == null) return;
            _ctx.Documents.Remove(doc);
            _ctx.SaveChanges();
        }
    }
}
