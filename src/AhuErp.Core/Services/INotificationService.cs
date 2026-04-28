using System;
using System.Collections.Generic;
using AhuErp.Core.Models;

namespace AhuErp.Core.Services
{
    /// <summary>
    /// Тип уведомления.
    /// </summary>
    public enum NotificationKind
    {
        /// <summary>Срок поручения истёк, исполнитель не закрыл.</summary>
        TaskOverdue,
        /// <summary>Назначено новое поручение текущему пользователю.</summary>
        TaskAssigned,
        /// <summary>Документ передан на согласование.</summary>
        ApprovalPending,
        /// <summary>Произвольное системное сообщение.</summary>
        System,
    }

    /// <summary>
    /// DTO одного элемента в ленте уведомлений.
    /// </summary>
    public sealed class Notification
    {
        public NotificationKind Kind { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? DocumentId { get; set; }
        public int? TaskId { get; set; }
        public bool IsRead { get; set; }
    }

    /// <summary>
    /// Сервис ленты уведомлений: строит снапшот «что важного сейчас у
    /// текущего пользователя», даёт пометить прочитанным. Не содержит
    /// собственного persistence — это сервисный слой над уже существующими
    /// репозиториями (просрочки берутся из <see cref="ITaskService.ListOverdue"/>,
    /// статусы — из <see cref="DocumentTask.Status"/>). UI-слой подписывается
    /// на <see cref="Changed"/> и перерисовывает индикатор-«колокольчик».
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Срабатывает после <see cref="Refresh"/>, <see cref="MarkAllRead"/>
        /// или любых других мутаций списка.
        /// </summary>
        event EventHandler Changed;

        /// <summary>
        /// Снапшот текущих уведомлений для пользователя из
        /// <see cref="ICurrentUserService"/>. Сортировка — по убыванию даты,
        /// сначала непрочитанные.
        /// </summary>
        IReadOnlyList<Notification> ListCurrent();

        /// <summary>Количество непрочитанных уведомлений (для бейджа).</summary>
        int UnreadCount { get; }

        /// <summary>Перестроить ленту, сходив в репозитории.</summary>
        void Refresh();

        /// <summary>Пометить все уведомления текущей сессии прочитанными.</summary>
        void MarkAllRead();
    }
}
