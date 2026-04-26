using System.Collections.Generic;
using System.Linq;
using AhuErp.Core.Models;

namespace AhuErp.Core.Services
{
    /// <summary>
    /// In-memory реализация <see cref="IAuditLogRepository"/> — для тестов и
    /// демонстрационного режима. Сохраняет порядок добавления, выдаёт
    /// инкрементальный Id.
    /// </summary>
    public sealed class InMemoryAuditLogRepository : IAuditLogRepository
    {
        private readonly List<AuditLog> _entries = new List<AuditLog>();
        private int _nextId = 1;

        public AuditLog GetLast() => _entries.Count == 0 ? null : _entries[_entries.Count - 1];

        public AuditLog Add(AuditLog log)
        {
            log.Id = _nextId++;
            _entries.Add(log);
            return log;
        }

        public IReadOnlyList<AuditLog> Query(AuditQueryFilter filter)
        {
            IEnumerable<AuditLog> q = _entries;
            if (filter.From.HasValue) q = q.Where(e => e.Timestamp >= filter.From.Value);
            if (filter.To.HasValue) q = q.Where(e => e.Timestamp <= filter.To.Value);
            if (filter.UserId.HasValue) q = q.Where(e => e.UserId == filter.UserId.Value);
            if (!string.IsNullOrWhiteSpace(filter.EntityType))
                q = q.Where(e => e.EntityType == filter.EntityType);
            if (filter.EntityId.HasValue) q = q.Where(e => e.EntityId == filter.EntityId.Value);
            if (filter.ActionType.HasValue) q = q.Where(e => e.ActionType == filter.ActionType.Value);
            q = q.OrderByDescending(e => e.Id);
            if (filter.Top.HasValue) q = q.Take(filter.Top.Value);
            return q.ToList().AsReadOnly();
        }

        public IReadOnlyList<AuditLog> ListAllOrdered()
            => _entries.OrderBy(e => e.Id).ToList().AsReadOnly();
    }
}
