using System.Collections.Generic;
using AhuErp.Core.Models;

namespace AhuErp.Core.Services
{
    /// <summary>
    /// Декларативное сопоставление <see cref="EmployeeRole"/> и разрешённых
    /// модулей навигации. Используется и ViewModel-ем главного окна для
    /// фильтрации меню, и (в будущем) серверной авторизацией команд.
    /// </summary>
    public static class RolePolicy
    {
        public const string Dashboard = nameof(Dashboard);
        public const string Office = nameof(Office);
        public const string Archive = nameof(Archive);
        public const string ItService = nameof(ItService);
        public const string Fleet = nameof(Fleet);
        public const string Warehouse = nameof(Warehouse);

        private static readonly IReadOnlyDictionary<EmployeeRole, HashSet<string>> _allowed =
            new Dictionary<EmployeeRole, HashSet<string>>
            {
                [EmployeeRole.Admin] = new HashSet<string>
                {
                    Dashboard, Office, Archive, ItService, Fleet, Warehouse
                },
                [EmployeeRole.Manager] = new HashSet<string>
                {
                    Dashboard, Office, Archive, ItService, Fleet, Warehouse
                },
                [EmployeeRole.Archivist] = new HashSet<string>
                {
                    Dashboard, Archive
                },
                [EmployeeRole.TechSupport] = new HashSet<string>
                {
                    Dashboard, ItService
                },
                [EmployeeRole.WarehouseManager] = new HashSet<string>
                {
                    Dashboard, Office, Fleet, Warehouse
                },
            };

        /// <summary>
        /// True, если сотруднику с данной ролью виден модуль <paramref name="moduleKey"/>.
        /// Ключи — константы этого класса.
        /// </summary>
        public static bool IsAllowed(EmployeeRole role, string moduleKey)
        {
            return _allowed.TryGetValue(role, out var set) && set.Contains(moduleKey);
        }
    }
}
