using System.Collections.Generic;
using AhuErp.Core.Models;

namespace AhuErp.Core.Services
{
    /// <summary>
    /// Доступ к данным журнала аудита. Записи только добавляются — операции
    /// удаления/обновления преднамеренно отсутствуют.
    /// </summary>
    public interface IAuditLogRepository
    {
        AuditLog GetLast();
        AuditLog Add(AuditLog log);
        IReadOnlyList<AuditLog> Query(AuditQueryFilter filter);
        IReadOnlyList<AuditLog> ListAllOrdered();
    }
}
