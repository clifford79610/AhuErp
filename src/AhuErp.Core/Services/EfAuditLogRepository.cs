using System;
using System.Collections.Generic;
using System.Linq;
using AhuErp.Core.Data;
using AhuErp.Core.Models;

namespace AhuErp.Core.Services
{
    /// <summary>
    /// EF6-реализация <see cref="IAuditLogRepository"/>. Записи immutable —
    /// поддерживается только добавление и чтение.
    /// </summary>
    public sealed class EfAuditLogRepository : IAuditLogRepository
    {
        private readonly AhuDbContext _ctx;

        public EfAuditLogRepository(AhuDbContext ctx)
        {
            _ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
        }

        public AuditLog GetLast()
            => _ctx.AuditLogs.OrderByDescending(a => a.Id).FirstOrDefault();

        public AuditLog Add(AuditLog log)
        {
            if (log == null) throw new ArgumentNullException(nameof(log));
            _ctx.AuditLogs.Add(log);
            _ctx.SaveChanges();
            return log;
        }

        public IReadOnlyList<AuditLog> Query(AuditQueryFilter filter)
        {
            IQueryable<AuditLog> q = _ctx.AuditLogs;
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
            => _ctx.AuditLogs.OrderBy(e => e.Id).ToList().AsReadOnly();
    }
}
