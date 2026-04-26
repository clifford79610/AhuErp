using System;
using AhuErp.Core.Models;

namespace AhuErp.Core.Services
{
    /// <summary>
    /// Базовая реализация <see cref="IWorkflowService"/>: переводит документ
    /// в статус <see cref="DocumentStatus.Completed"/>, когда все его поручения
    /// завершены, и пишет события в журнал аудита. Расширяемые сценарии
    /// (автоматическое создание InventoryTransaction и т.п.) подключаются
    /// через перегрузки в наследниках или конкретные обработчики.
    /// </summary>
    public sealed class WorkflowService : IWorkflowService
    {
        private readonly IDocumentRepository _documents;
        private readonly ITaskRepository _tasks;
        private readonly IAuditService _audit;

        public WorkflowService(
            IDocumentRepository documents,
            ITaskRepository tasks,
            IAuditService audit)
        {
            _documents = documents ?? throw new ArgumentNullException(nameof(documents));
            _tasks = tasks ?? throw new ArgumentNullException(nameof(tasks));
            _audit = audit ?? throw new ArgumentNullException(nameof(audit));
        }

        public void OnTaskCompleted(DocumentTask task, int actorId)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            var doc = _documents.GetById(task.DocumentId);
            if (doc == null) return;

            // Если у документа больше нет открытых поручений, переводим его
            // в статус Completed: это даёт автоматическое снятие с контроля
            // и закрытие документа без ручного действия.
            var allTasks = _tasks.ListByDocument(doc.Id);
            bool anyOpen = false;
            foreach (var t in allTasks)
            {
                if (t.Status != DocumentTaskStatus.Completed
                    && t.Status != DocumentTaskStatus.Cancelled)
                {
                    anyOpen = true;
                    break;
                }
            }
            if (!anyOpen && doc.Status != DocumentStatus.Completed)
            {
                var oldStatus = doc.Status;
                doc.Status = DocumentStatus.Completed;
                _documents.Update(doc);
                _audit.Record(AuditActionType.StatusChanged, nameof(Document), doc.Id, actorId,
                    oldValues: $"Status={oldStatus}",
                    newValues: $"Status={DocumentStatus.Completed}",
                    details: "Автоматическое закрытие документа: все поручения исполнены.");
            }
        }

        public void OnApprovalRouteCompleted(int documentId, int actorId)
        {
            var doc = _documents.GetById(documentId);
            if (doc == null) return;
            doc.ApprovalStatus = ApprovalRouteStatus.Completed;
            _documents.Update(doc);
            _audit.Record(AuditActionType.ApprovalSigned, nameof(Document), doc.Id, actorId,
                details: "Маршрут согласования завершён.");
        }
    }
}
