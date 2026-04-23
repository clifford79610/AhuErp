using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AhuErp.Core.Models
{
    /// <summary>
    /// Сотрудник учреждения. Может быть назначен ответственным за документ
    /// и аутентифицироваться в системе (см. <see cref="PasswordHash"/>).
    /// </summary>
    public class Employee
    {
        public int Id { get; set; }

        [Required]
        [StringLength(256)]
        public string FullName { get; set; }

        [StringLength(256)]
        public string Position { get; set; }

        /// <summary>
        /// Роль сотрудника в системе. По умолчанию — ограниченный
        /// <see cref="EmployeeRole.TechSupport"/>, до явного повышения.
        /// </summary>
        public EmployeeRole Role { get; set; } = EmployeeRole.TechSupport;

        /// <summary>
        /// Хэш пароля в формате <c>{iterations}.{base64(salt)}.{base64(hash)}</c>,
        /// рассчитанный через <see cref="Services.IPasswordHasher"/>.
        /// Ни в каком виде чистый пароль не сохраняется.
        /// </summary>
        [StringLength(512)]
        public string PasswordHash { get; set; }

        public virtual ICollection<Document> AssignedDocuments { get; set; } = new HashSet<Document>();
    }
}
