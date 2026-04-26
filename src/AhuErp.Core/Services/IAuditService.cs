using System;
using System.Collections.Generic;
using AhuErp.Core.Models;

namespace AhuErp.Core.Services
{
    /// <summary>
    /// Сервис журнала аудита. Все вызовы сохраняют immutable-запись с
    /// hash-цепочкой целостности (см. <see cref="AuditLog"/>). Чтение
    /// поддерживает фильтрацию для административной панели.
    /// </summary>
    public interface IAuditService
    {
        /// <summary>
        /// Записать событие аудита. Возвращает сохранённую запись с заполненным
        /// <see cref="AuditLog.Hash"/> и ссылкой на предыдущую запись.
        /// </summary>
        AuditLog Record(
            AuditActionType actionType,
            string entityType,
            int? entityId,
            int? userId,
            string oldValues = null,
            string newValues = null,
            string details = null);

        IReadOnlyList<AuditLog> Query(AuditQueryFilter filter);

        /// <summary>
        /// Проверить целостность hash-цепочки. Возвращает первую испорченную
        /// запись или <c>null</c>, если цепочка корректна.
        /// </summary>
        AuditLog VerifyChain();
    }

    /// <summary>Фильтр выборки журнала аудита.</summary>
    public sealed class AuditQueryFilter
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public int? UserId { get; set; }
        public string EntityType { get; set; }
        public int? EntityId { get; set; }
        public AuditActionType? ActionType { get; set; }
        public int? Top { get; set; }
    }
}
