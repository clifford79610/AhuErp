using System;
using System.Collections.Generic;
using System.Linq;
using AhuErp.Core.Models;

namespace AhuErp.Core.Services
{
    /// <summary>
    /// Базовая реализация <see cref="INotificationService"/>: формирует ленту
    /// уведомлений из текущего состояния задач/документов. Список флагов
    /// «прочитано» хранится в RAM (по сессии); долговременное хранение можно
    /// добавить отдельной таблицей в Phase 13, не меняя контракта.
    /// </summary>
    public sealed class NotificationService : INotificationService
    {
        private readonly ITaskService _tasks;
        private readonly IDocumentRepository _documents;
        private readonly ICurrentUserService _users;

        // Ключ — (Kind, EntityId) чтобы повторный Refresh не плодил дубликаты,
        // и сохранял пометку IsRead между вызовами.
        private readonly Dictionary<(NotificationKind, int), Notification> _state =
            new Dictionary<(NotificationKind, int), Notification>();

        public event EventHandler Changed;

        public NotificationService(
            ITaskService tasks,
            IDocumentRepository documents,
            ICurrentUserService users)
        {
            _tasks = tasks ?? throw new ArgumentNullException(nameof(tasks));
            _documents = documents ?? throw new ArgumentNullException(nameof(documents));
            _users = users ?? throw new ArgumentNullException(nameof(users));
        }

        public int UnreadCount => _state.Values.Count(n => !n.IsRead);

        public IReadOnlyList<Notification> ListCurrent()
        {
            return _state.Values
                .OrderBy(n => n.IsRead)
                .ThenByDescending(n => n.CreatedAt)
                .ToList()
                .AsReadOnly();
        }

        public void Refresh()
        {
            var employee = _users.Current;
            if (employee == null)
            {
                if (_state.Count > 0)
                {
                    _state.Clear();
                    RaiseChanged();
                }
                return;
            }

            var now = DateTime.Now;
            var liveKeys = new HashSet<(NotificationKind, int)>();

            // Просроченные поручения.
            foreach (var t in _tasks.ListOverdue(now))
            {
                if (t.ExecutorId != employee.Id) continue;
                var key = (NotificationKind.TaskOverdue, t.Id);
                liveKeys.Add(key);
                if (_state.ContainsKey(key)) continue;
                _state[key] = new Notification
                {
                    Kind = NotificationKind.TaskOverdue,
                    Title = "Просроченное поручение",
                    Body = string.IsNullOrWhiteSpace(t.Description)
                        ? $"Поручение #{t.Id}"
                        : t.Description,
                    CreatedAt = t.Deadline,
                    DocumentId = t.DocumentId,
                    TaskId = t.Id,
                    IsRead = false
                };
            }

            // Активные новые поручения текущему пользователю (как исполнителю).
            foreach (var t in _tasks.ListMyTasks(employee.Id, MyTasksScope.AsExecutor))
            {
                if (t.Status != DocumentTaskStatus.New) continue;
                // Если поручение уже попало в ленту как «Просроченное», не дублируем
                // его в ленте как «Новое» — overdue важнее и actionable.
                if (liveKeys.Contains((NotificationKind.TaskOverdue, t.Id))) continue;
                var key = (NotificationKind.TaskAssigned, t.Id);
                liveKeys.Add(key);
                if (_state.ContainsKey(key)) continue;
                _state[key] = new Notification
                {
                    Kind = NotificationKind.TaskAssigned,
                    Title = "Новое поручение",
                    Body = string.IsNullOrWhiteSpace(t.Description)
                        ? $"Поручение #{t.Id}"
                        : t.Description,
                    CreatedAt = t.CreatedAt,
                    DocumentId = t.DocumentId,
                    TaskId = t.Id,
                    IsRead = false
                };
            }

            // Удаляем устаревшие записи (поручение закрыли — пропадает из ленты).
            var stale = _state.Keys.Where(k => !liveKeys.Contains(k)).ToList();
            foreach (var k in stale) _state.Remove(k);

            RaiseChanged();
        }

        public void MarkAllRead()
        {
            bool anyChanged = false;
            foreach (var n in _state.Values)
            {
                if (n.IsRead) continue;
                n.IsRead = true;
                anyChanged = true;
            }
            if (anyChanged) RaiseChanged();
        }

        private void RaiseChanged() => Changed?.Invoke(this, EventArgs.Empty);
    }
}
