using AhuErp.Core.Models;

namespace AhuErp.Core.Services
{
    /// <summary>
    /// Тонкий read-only взгляд на «кто сейчас работает в системе».
    /// Введён в Foundation extras чтобы перестать пробрасывать <c>userId</c>
    /// параметром через все бизнес-сервисы (<c>InventoryService.ProcessTransaction</c>,
    /// <c>TaskService.UpdateStatus</c> и т.п.). <see cref="IAuthService"/>
    /// расширяет этот контракт логин/логаут-API; для большинства сервисов
    /// достаточно знать только текущего сотрудника.
    /// </summary>
    public interface ICurrentUserService
    {
        /// <summary>Текущий активный сотрудник или <c>null</c>.</summary>
        Employee Current { get; }

        /// <summary>
        /// Удобный shortcut: <see cref="Employee.Id"/> если сессия активна,
        /// иначе <c>null</c>. Возврат <c>int?</c> сделан чтобы вызовы вида
        /// <c>_users.CurrentId ?? throw new InvalidOperationException(...)</c>
        /// читались без явного null-check на <see cref="Current"/>.
        /// </summary>
        int? CurrentId { get; }

        /// <summary>True, если в системе есть активный пользователь.</summary>
        bool IsAuthenticated { get; }
    }
}
