using System;
using AhuErp.Core.Models;
using AhuErp.Core.Services;
using Xunit;

namespace AhuErp.Tests
{
    /// <summary>
    /// Контроль исполнительской дисциплины через <see cref="TaskService"/>:
    /// выдача резолюции, создание поручения, изменение статуса, переадресация,
    /// отчёт по своевременности.
    /// </summary>
    public class TaskServiceTests
    {
        private readonly InMemoryDocumentRepository _docs = new InMemoryDocumentRepository();
        private readonly InMemoryTaskRepository _tasksRepo = new InMemoryTaskRepository();
        private readonly InMemoryAuditLogRepository _auditRepo = new InMemoryAuditLogRepository();
        private readonly AuditService _audit;
        private readonly TaskService _service;
        private readonly Document _doc;

        public TaskServiceTests()
        {
            _audit = new AuditService(_auditRepo);
            _service = new TaskService(_tasksRepo, _docs, _audit);
            _doc = new Document
            {
                Title = "Письмо для контроля",
                Type = DocumentType.Internal,
                CreationDate = DateTime.Now.AddDays(-1),
                Deadline = DateTime.Now.AddDays(10),
            };
            _docs.Add(_doc);
        }

        [Fact]
        public void CreateTask_requires_future_deadline()
        {
            Assert.Throws<ArgumentException>(() => _service.CreateTask(
                _doc.Id, authorId: 1, executorId: 2, description: "Сделать",
                deadline: DateTime.UtcNow.Date.AddDays(-1)));
        }

        [Fact]
        public void CreateTask_logs_audit_and_persists()
        {
            var task = _service.CreateTask(_doc.Id, authorId: 1, executorId: 2,
                description: "Подготовить акт списания", deadline: DateTime.UtcNow.AddDays(3));

            Assert.NotNull(_tasksRepo.GetTask(task.Id));
            var auditLog = _audit.Query(new AuditQueryFilter { ActionType = AuditActionType.TaskAssigned });
            Assert.Single(auditLog);
            Assert.Equal(task.Id, auditLog[0].EntityId);
        }

        [Fact]
        public void UpdateStatus_to_Completed_marks_completion_and_logs()
        {
            var task = _service.CreateTask(_doc.Id, 1, 2, "Описание",
                DateTime.UtcNow.AddDays(2));

            var updated = _service.UpdateStatus(task.Id, DocumentTaskStatus.Completed,
                actorId: 2, reportText: "Готово");

            Assert.Equal(DocumentTaskStatus.Completed, updated.Status);
            Assert.NotNull(updated.CompletedAt);
            Assert.Equal("Готово", updated.ReportText);
            var logs = _audit.Query(new AuditQueryFilter { ActionType = AuditActionType.TaskCompleted });
            Assert.Single(logs);
        }

        [Fact]
        public void Reassign_changes_executor_and_logs()
        {
            var task = _service.CreateTask(_doc.Id, 1, 2, "Описание",
                DateTime.UtcNow.AddDays(2));
            _service.Reassign(task.Id, newExecutorId: 5, actorId: 1, reason: "В отпуске");

            Assert.Equal(5, _tasksRepo.GetTask(task.Id).ExecutorId);
            var logs = _audit.Query(new AuditQueryFilter { ActionType = AuditActionType.TaskReassigned });
            Assert.Single(logs);
        }

        [Fact]
        public void DisciplineReport_classifies_on_time_late_and_overdue()
        {
            // 1) Выполнено в срок.
            var t1 = _service.CreateTask(_doc.Id, 1, 2, "В срок", DateTime.UtcNow.AddDays(2));
            _service.UpdateStatus(t1.Id, DocumentTaskStatus.Completed, 2);

            // 2) Выполнено с опозданием — имитируем через прямую правку.
            var t2 = _service.CreateTask(_doc.Id, 1, 3, "С опозданием",
                DateTime.UtcNow.AddDays(2));
            t2.Deadline = DateTime.UtcNow.AddDays(-1);
            _tasksRepo.UpdateTask(t2);
            _service.UpdateStatus(t2.Id, DocumentTaskStatus.Completed, 3);

            // 3) Просрочено и не выполнено.
            var t3 = _service.CreateTask(_doc.Id, 1, 4, "Просрочено",
                DateTime.UtcNow.AddDays(2));
            t3.Deadline = DateTime.UtcNow.AddDays(-2);
            _tasksRepo.UpdateTask(t3);

            var report = _service.BuildDisciplineReport(
                DateTime.UtcNow.AddDays(-30), DateTime.UtcNow.AddDays(30));

            Assert.Equal(3, report.TotalTasks);
            Assert.Equal(1, report.CompletedOnTime);
            Assert.Equal(1, report.CompletedLate);
            Assert.Equal(1, report.Overdue);
            Assert.True(report.TimelyExecutionRate > 0 && report.TimelyExecutionRate < 1);
            Assert.Equal(3, report.ByExecutor.Count);
        }

        [Fact]
        public void ListMyTasks_filters_by_role()
        {
            _service.CreateTask(_doc.Id, authorId: 1, executorId: 2, description: "A",
                deadline: DateTime.UtcNow.AddDays(1));
            _service.CreateTask(_doc.Id, authorId: 2, executorId: 3, description: "B",
                deadline: DateTime.UtcNow.AddDays(1));

            Assert.Single(_service.ListMyTasks(2, MyTasksScope.AsExecutor));
            Assert.Single(_service.ListMyTasks(1, MyTasksScope.AsAuthor));
            Assert.Equal(2, _service.ListMyTasks(2, MyTasksScope.Any).Count);
        }

        [Fact]
        public void Workflow_is_invoked_on_completion_when_provided()
        {
            var workflow = new RecordingWorkflow();
            var service = new TaskService(_tasksRepo, _docs, _audit, workflow);

            var task = service.CreateTask(_doc.Id, 1, 2, "X", DateTime.UtcNow.AddDays(1));
            service.UpdateStatus(task.Id, DocumentTaskStatus.Completed, 2);

            Assert.Equal(1, workflow.CompletedCount);
        }

        private sealed class RecordingWorkflow : IWorkflowService
        {
            public int CompletedCount;
            public void OnTaskCompleted(DocumentTask task, int actorId) => CompletedCount++;
            public void OnApprovalRouteCompleted(int documentId, int actorId) { }
        }
    }
}
