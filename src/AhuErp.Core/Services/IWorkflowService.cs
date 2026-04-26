using AhuErp.Core.Models;

namespace AhuErp.Core.Services
{
    /// <summary>
    /// Интеграционный сервис, реагирующий на ключевые события документа:
    /// завершение поручения, переход маршрута согласования и т.д.
    /// Может автоматически создавать связанные хозяйственные операции,
    /// например списание ТМЦ при завершении поручения по акту списания.
    /// </summary>
    public interface IWorkflowService
    {
        /// <summary>Реакция на завершение поручения по документу.</summary>
        void OnTaskCompleted(DocumentTask task, int actorId);

        /// <summary>Реакция на завершение прохождения маршрута согласования.</summary>
        void OnApprovalRouteCompleted(int documentId, int actorId);
    }
}
