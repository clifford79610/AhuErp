using System;
using System.Linq;
using AhuErp.Core.Models;
using AhuErp.Core.Services;
using Xunit;

namespace AhuErp.Tests
{
    /// <summary>
    /// <see cref="NotificationService"/> должен:
    /// - возвращать пустой список без активного пользователя;
    /// - формировать уведомления о просрочках исполнителю текущего пользователя;
    /// - оставлять флаг IsRead между вызовами Refresh;
    /// - удалять уведомление, если поручение пропадает из выдачи.
    /// </summary>
    public class NotificationServiceTests
    {
        private sealed class FakeCurrentUser : ICurrentUserService
        {
            public Employee Current { get; set; }
            public int? CurrentId => Current?.Id;
            public bool IsAuthenticated => Current != null;
        }

        private readonly InMemoryDocumentRepository _docs = new InMemoryDocumentRepository();
        private readonly InMemoryTaskRepository _tasksRepo = new InMemoryTaskRepository();
        private readonly InMemoryAuditLogRepository _auditRepo = new InMemoryAuditLogRepository();
        private readonly AuditService _audit;
        private readonly TaskService _tasks;
        private readonly FakeCurrentUser _users = new FakeCurrentUser();
        private readonly NotificationService _service;

        private readonly Employee _executor = new Employee { Id = 10, FullName = "Исполнитель И.", Role = EmployeeRole.TechSupport };
        private readonly Employee _author = new Employee { Id = 1, FullName = "Автор А.", Role = EmployeeRole.Manager };

        public NotificationServiceTests()
        {
            _audit = new AuditService(_auditRepo);
            _tasks = new TaskService(_tasksRepo, _docs, _audit);
            _service = new NotificationService(_tasks, _docs, _users);
        }

        private Document NewDoc()
        {
            var d = new Document
            {
                Title = "Документ",
                Type = DocumentType.Internal,
                CreationDate = DateTime.Now.AddDays(-2),
                Deadline = DateTime.Now.AddDays(5)
            };
            _docs.Add(d);
            return d;
        }

        [Fact]
        public void Refresh_without_authenticated_user_yields_empty_list()
        {
            _users.Current = null;
            _service.Refresh();
            Assert.Empty(_service.ListCurrent());
            Assert.Equal(0, _service.UnreadCount);
        }

        [Fact]
        public void Refresh_creates_TaskOverdue_notification_for_current_executor()
        {
            _users.Current = _executor;
            var doc = NewDoc();

            _tasks.CreateTask(
                documentId: doc.Id,
                authorId: _author.Id,
                executorId: _executor.Id,
                description: "Подготовить отчёт",
                deadline: DateTime.Now.AddSeconds(1));

            // Ждать секунду в тесте плохо — переводим вручную в просроченное
            // через прямую установку deadline на InMemoryTaskRepository.
            var task = _tasksRepo.ListByDocument(doc.Id).Single();
            task.Deadline = DateTime.Now.AddMinutes(-5);

            _service.Refresh();

            var list = _service.ListCurrent();
            Assert.Contains(list, n => n.Kind == NotificationKind.TaskOverdue && n.TaskId == task.Id);
            Assert.True(_service.UnreadCount >= 1);
        }

        [Fact]
        public void Refresh_does_not_show_other_users_overdue_tasks()
        {
            _users.Current = _executor;
            var doc = NewDoc();

            // Назначаем поручение НЕ текущему пользователю (другому исполнителю).
            var otherExec = 999;
            _tasks.CreateTask(doc.Id, _author.Id, otherExec, "Чужое поручение", DateTime.Now.AddSeconds(1));
            var t = _tasksRepo.ListByDocument(doc.Id).Single();
            t.Deadline = DateTime.Now.AddMinutes(-1);

            _service.Refresh();

            Assert.DoesNotContain(_service.ListCurrent(), n => n.Kind == NotificationKind.TaskOverdue);
        }

        [Fact]
        public void MarkAllRead_clears_unread_count_but_keeps_items()
        {
            _users.Current = _executor;
            var doc = NewDoc();
            _tasks.CreateTask(doc.Id, _author.Id, _executor.Id, "X", DateTime.Now.AddSeconds(1));
            _tasksRepo.ListByDocument(doc.Id).Single().Deadline = DateTime.Now.AddMinutes(-1);
            _service.Refresh();
            Assert.True(_service.UnreadCount > 0);

            _service.MarkAllRead();

            Assert.Equal(0, _service.UnreadCount);
            Assert.NotEmpty(_service.ListCurrent());
        }

        [Fact]
        public void Refresh_drops_notification_when_task_removed_from_overdue_set()
        {
            _users.Current = _executor;
            var doc = NewDoc();
            _tasks.CreateTask(doc.Id, _author.Id, _executor.Id, "Y", DateTime.Now.AddSeconds(1));
            var task = _tasksRepo.ListByDocument(doc.Id).Single();
            task.Deadline = DateTime.Now.AddMinutes(-5);
            _service.Refresh();
            Assert.NotEmpty(_service.ListCurrent());

            // Закрываем поручение — оно перестаёт быть просроченным и должно
            // пропасть из ленты после следующего Refresh.
            _tasks.UpdateStatus(task.Id, DocumentTaskStatus.Completed, _executor.Id, "Готово");

            _service.Refresh();
            Assert.DoesNotContain(_service.ListCurrent(),
                n => n.Kind == NotificationKind.TaskOverdue && n.TaskId == task.Id);
        }

        [Fact]
        public void Changed_event_fires_on_refresh()
        {
            _users.Current = _executor;
            int fires = 0;
            _service.Changed += (s, e) => fires++;
            _service.Refresh();
            Assert.True(fires >= 1);
        }
    }
}
