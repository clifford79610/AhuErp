using System.Collections.Generic;
using AhuErp.Core.Models;

namespace AhuErp.Core.Services
{
    /// <summary>
    /// Granular permissions matrix: пары «модуль:действие». Используется там,
    /// где недостаточно «видит / не видит модуль» (это уже даёт <see cref="RolePolicy"/>),
    /// а нужна точная авторизация команды (можно ли регистрировать документ,
    /// согласовывать, удалять номенклатуру и т.п.). Подход декларативный —
    /// расширяется добавлением констант + строки в <see cref="_grants"/>.
    /// </summary>
    public static class Permissions
    {
        // ── Документы / РКК ───────────────────────────────────────────────
        public const string DocumentRead = "Document.Read";
        public const string DocumentWrite = "Document.Write";
        public const string DocumentRegister = "Document.Register";
        public const string DocumentDelete = "Document.Delete";

        // ── Поручения / задачи ────────────────────────────────────────────
        public const string TaskAssign = "Task.Assign";
        public const string TaskComplete = "Task.Complete";
        public const string TaskDelegate = "Task.Delegate";

        // ── Согласование / Workflow ───────────────────────────────────────
        public const string WorkflowApprove = "Workflow.Approve";
        public const string WorkflowReturn = "Workflow.Return";

        // ── Аудит ─────────────────────────────────────────────────────────
        public const string AuditView = "Audit.View";
        public const string AuditVerify = "Audit.Verify";

        // ── НСИ ───────────────────────────────────────────────────────────
        public const string NsiRead = "NSI.Read";
        public const string NsiManage = "NSI.Manage";

        // ── Отчёты ────────────────────────────────────────────────────────
        public const string ReportRun = "Report.Run";
        public const string ReportExport = "Report.Export";

        // ── АХД ───────────────────────────────────────────────────────────
        public const string InventoryWriteOff = "Inventory.WriteOff";
        public const string FleetBook = "Fleet.Book";
        public const string ArchiveProcess = "Archive.Process";

        private static readonly IReadOnlyDictionary<EmployeeRole, HashSet<string>> _grants =
            new Dictionary<EmployeeRole, HashSet<string>>
            {
                [EmployeeRole.Admin] = new HashSet<string>
                {
                    DocumentRead, DocumentWrite, DocumentRegister, DocumentDelete,
                    TaskAssign, TaskComplete, TaskDelegate,
                    WorkflowApprove, WorkflowReturn,
                    AuditView, AuditVerify,
                    NsiRead, NsiManage,
                    ReportRun, ReportExport,
                    InventoryWriteOff, FleetBook, ArchiveProcess,
                },
                [EmployeeRole.Manager] = new HashSet<string>
                {
                    DocumentRead, DocumentWrite, DocumentRegister,
                    TaskAssign, TaskComplete, TaskDelegate,
                    WorkflowApprove, WorkflowReturn,
                    AuditView,
                    NsiRead,
                    ReportRun, ReportExport,
                    InventoryWriteOff, FleetBook, ArchiveProcess,
                },
                [EmployeeRole.Archivist] = new HashSet<string>
                {
                    DocumentRead, DocumentWrite, DocumentRegister,
                    TaskComplete,
                    AuditView,
                    NsiRead, NsiManage,
                    ReportRun, ReportExport,
                    ArchiveProcess,
                },
                [EmployeeRole.TechSupport] = new HashSet<string>
                {
                    DocumentRead,
                    TaskComplete,
                    NsiRead,
                },
                [EmployeeRole.WarehouseManager] = new HashSet<string>
                {
                    DocumentRead, DocumentWrite,
                    TaskComplete,
                    NsiRead,
                    ReportRun, ReportExport,
                    InventoryWriteOff, FleetBook,
                },
            };

        /// <summary>
        /// True, если роли разрешено выполнять действие <paramref name="permission"/>.
        /// Ключи — константы этого класса.
        /// </summary>
        public static bool Has(EmployeeRole role, string permission) =>
            _grants.TryGetValue(role, out var set) && set.Contains(permission);

        /// <summary>
        /// Все назначенные роли разрешения. Удобно для отладки и UI «что я могу».
        /// </summary>
        public static IReadOnlyCollection<string> ListFor(EmployeeRole role) =>
            _grants.TryGetValue(role, out var set)
                ? (IReadOnlyCollection<string>)set
                : new HashSet<string>();
    }
}
