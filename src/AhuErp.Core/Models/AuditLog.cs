using System;
using System.ComponentModel.DataAnnotations;

namespace AhuErp.Core.Models
{
    /// <summary>
    /// Immutable-запись журнала аудита. Поле <see cref="Hash"/> вычисляется
    /// от значимых атрибутов записи + <see cref="PreviousHash"/> предыдущей
    /// записи в потоке — это даёт цепочку целостности (hash chain), которую
    /// нельзя локально подменить без обнаружения. Аудит-сервис не позволяет
    /// удалять/изменять записи, только добавлять.
    /// </summary>
    public class AuditLog
    {
        public int Id { get; set; }

        public DateTime Timestamp { get; set; }

        /// <summary>Сотрудник-инициатор. Null допускается только для системных событий.</summary>
        public int? UserId { get; set; }
        public virtual Employee User { get; set; }

        public AuditActionType ActionType { get; set; }

        [StringLength(128)]
        public string EntityType { get; set; }

        public int? EntityId { get; set; }

        /// <summary>Сериализованное (JSON-подобное) представление прежнего состояния.</summary>
        [StringLength(4000)]
        public string OldValues { get; set; }

        /// <summary>Сериализованное (JSON-подобное) представление нового состояния.</summary>
        [StringLength(4000)]
        public string NewValues { get; set; }

        [StringLength(1024)]
        public string Details { get; set; }

        /// <summary>SHA-256 хэш записи (hex).</summary>
        [StringLength(128)]
        public string Hash { get; set; }

        /// <summary>Хэш предыдущей записи (для построения цепочки).</summary>
        [StringLength(128)]
        public string PreviousHash { get; set; }
    }
}
